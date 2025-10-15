import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { switchMap, catchError, of } from 'rxjs';
import { OperationsApiService } from '../operations-api.service';
import { BudgetApiService } from '../../budget/budget-api.service';
import { BudgetResponse, UnregisteredOperationRequest, ImportOperationsRequest } from '../../budget/models';
import { 
  TuiButton, 
  TuiDialogService, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel
} from '@taiga-ui/core';
import { TuiCardLarge } from '@taiga-ui/layout';

@Component({
  selector: 'app-import-operations',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TuiButton,
    TuiLoader,
    TuiTextfield,
    TuiLabel,
    TuiCardLarge,
    TuiTitle
  ],
  templateUrl: './import-operations.component.html',
  styleUrls: ['./import-operations.component.less']
})
export class ImportOperationsComponent implements OnInit {
  budgetId!: string;
  budget: BudgetResponse | null = null;
  isLoading = false;
  
  importForm!: FormGroup;
  csvContent: string = '';
  importResult: { registered: number; duplicates: number; errors: string[] } | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private budgetApi: BudgetApiService,
    private fb: FormBuilder,
    private dialogService: TuiDialogService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    
    this.importForm = this.fb.group({
      transferConfidenceLevel: ['']
    });

    this.loadBudget();
  }

  loadBudget(): void {
    this.isLoading = true;
    this.budgetApi.getBudgetById(this.budgetId).subscribe({
      next: (budget) => {
        this.budget = budget || null;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error, 'Failed to load budget');
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    const reader = new FileReader();
    
    reader.onload = (e) => {
      this.csvContent = e.target?.result as string;
    };
    
    reader.onerror = () => {
      this.showError('Failed to read file');
    };
    
    reader.readAsText(file);
  }

  importCsv(): void {
    if (!this.csvContent || !this.budget) {
      this.showError('Please select a CSV file first');
      return;
    }

    this.isLoading = true;
    this.importResult = null;

    // Parse CSV content
    const operations = this.parseCsv(this.csvContent);
    if (operations.length === 0) {
      this.isLoading = false;
      this.showError('No valid operations found in CSV');
      return;
    }

    const request: ImportOperationsRequest = {
      budgetVersion: this.budget.version,
      operations: operations,
      transferConfidenceLevel: this.importForm.value.transferConfidenceLevel || undefined
    };

    this.operationsApi.importOperations(this.budgetId, request).subscribe({
      next: (result) => {
        this.isLoading = false;
        this.importResult = {
          registered: result.registeredOperations.length,
          duplicates: result.duplicates.length,
          errors: result.errors
        };
        
        if (result.errors.length === 0) {
          this.showSuccess(`Successfully imported ${result.registeredOperations.length} operations`);
          this.operationsApi.triggerRefresh(this.budgetId);
        } else {
          this.showError(`Import completed with errors: ${result.errors.join('; ')}`);
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error, 'Failed to import operations');
      }
    });
  }

  parseCsv(content: string): UnregisteredOperationRequest[] {
    const lines = content.split('\n').filter(line => line.trim());
    if (lines.length < 2) return []; // Need header + at least one row

    const header = lines[0].split(',').map(h => h.trim());
    const operations: UnregisteredOperationRequest[] = [];

    // Find column indices
    const timestampIdx = header.findIndex(h => h.toLowerCase() === 'timestamp' || h.toLowerCase() === 'date');
    const amountIdx = header.findIndex(h => h.toLowerCase() === 'amount');
    const currencyIdx = header.findIndex(h => h.toLowerCase() === 'currency');
    const descriptionIdx = header.findIndex(h => h.toLowerCase() === 'description');

    if (timestampIdx === -1 || amountIdx === -1 || currencyIdx === -1 || descriptionIdx === -1) {
      this.showError('CSV must have columns: timestamp, amount, currency, description');
      return [];
    }

    for (let i = 1; i < lines.length; i++) {
      const values = lines[i].split(',').map(v => v.trim());
      
      try {
        const operation: UnregisteredOperationRequest = {
          timestamp: new Date(values[timestampIdx]).toISOString(),
          amount: {
            value: parseFloat(values[amountIdx]),
            currencyCode: values[currencyIdx]
          },
          description: values[descriptionIdx],
          attributes: {}
        };

        // Add remaining columns as attributes
        for (let j = 0; j < header.length; j++) {
          if (j !== timestampIdx && j !== amountIdx && j !== currencyIdx && j !== descriptionIdx) {
            operation.attributes![header[j]] = values[j];
          }
        }

        operations.push(operation);
      } catch (error) {
        console.warn(`Skipping invalid row ${i + 1}:`, error);
      }
    }

    return operations;
  }

  viewOperations(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations']);
  }

  private handleError(error: any, defaultMessage: string): void {
    let errorMessage = defaultMessage;
    
    if (error.status === 400 && Array.isArray(error.error)) {
      const errors = error.error as any[];
      errorMessage = errors.map(e => e.message || e).join('; ');
    } else if (error.error?.message) {
      errorMessage = error.error.message;
    }
    
    this.showError(errorMessage);
  }

  private showError(message: string): void {
    this.dialogService.open(message, {
      label: 'Error',
      size: 'm',
      closeable: true,
      dismissible: true
    }).subscribe();
  }

  private showSuccess(message: string): void {
    this.dialogService.open(message, {
      label: 'Success',
      size: 's',
      closeable: true,
      dismissible: true
    }).subscribe();
  }
}

