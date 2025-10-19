import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OperationsApiService } from '../operations-api.service';
import { BudgetApiService } from '../../budget/budget-api.service';
import { BudgetResponse, IError, ISuccess } from '../../budget/models';
import { 
  TuiButton, 
  TuiDialogService, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel
} from '@taiga-ui/core';
import { TuiCardLarge } from '@taiga-ui/layout';
import { OperationsTableComponent } from '../operations-table/operations-table.component';

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
    TuiTitle,
    OperationsTableComponent
  ],
  templateUrl: './import-operations.component.html',
  styleUrls: ['./import-operations.component.less']
})
export class ImportOperationsComponent implements OnInit {
  budgetId!: string;
  budget: BudgetResponse | null = null;
  isLoading = false;
  
  importForm!: FormGroup;
  selectedFile: File | null = null;
  importResult: { 
    registered: number;
    duplicates: number; 
    errors: IError[];
    successes: ISuccess[];
  } | null = null;

  // Section toggles
  showSuccesses = true;
  showErrors = true;
  showDuplicates = false;

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
      transferConfidenceLevel: [''],
      filePattern: ['']
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
    if (!input.files || input.files.length === 0) {
      this.selectedFile = null;
      return;
    }

    this.selectedFile = input.files[0];
  }

  importCsv(): void {
    if (!this.selectedFile || !this.budget) {
      this.showError('Please select a CSV file first');
      return;
    }

    this.isLoading = true;
    this.importResult = null;

    const transferConfidenceLevel = this.importForm.value.transferConfidenceLevel || undefined;
    const filePattern = this.importForm.value.filePattern || undefined;

    this.operationsApi.importOperations(
      this.budgetId, 
      this.selectedFile, 
      this.budget.version,
      transferConfidenceLevel,
      filePattern
    ).subscribe({
      next: (result) => {
        this.isLoading = false;
        this.importResult = {
          registered: result.registeredOperations.length,
          duplicates: result.duplicates.length,
          errors: result.errors,
          successes: result.successes
        };
        
        // Store full result for displaying details
        (this.importResult as any).duplicatesList = result.duplicates;
        
        if (result.errors.length === 0) {
          this.showSuccess(`Successfully imported ${result.registeredOperations.length} operations`);
          this.operationsApi.triggerRefresh(this.budgetId);
        } else {
          const errorMessage = result.errors.length > 5 
            ? `Import completed with ${result.errors.length} errors. Check the results below.`
            : `Import completed with errors. See details below.`;
          this.showError(errorMessage);
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error, 'Failed to import operations');
      }
    });
  }

  viewOperations(): void {
    this.router.navigate(['/budget', this.budgetId]);
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

  // Helper method to access Object.keys in template
  getObjectKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }

  toggleSuccesses(): void {
    this.showSuccesses = !this.showSuccesses;
  }

  toggleErrors(): void {
    this.showErrors = !this.showErrors;
  }

  toggleDuplicates(): void {
    this.showDuplicates = !this.showDuplicates;
  }

  getDuplicatesList(): any[] {
    return (this.importResult as any)?.duplicatesList || [];
  }
}

