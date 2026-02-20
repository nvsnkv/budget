import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OperationsApiService } from '../operations-api.service';
import { BudgetApiService } from '../../budget/budget-api.service';
import { BudgetResponse } from '../../budget/models';
import { 
  TuiButton, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel
} from '@taiga-ui/core';
import { OperationsTableComponent } from '../operations-table/operations-table.component';
import { NotificationService } from '../shared/notification.service';
import { OperationResultComponent } from '../shared/components/operation-result/operation-result.component';
import { ImportResult } from '../shared/models/result.interface';

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
    TuiTitle,
    OperationsTableComponent,
    OperationResultComponent
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
  importResult: ImportResult | null = null;

  // Section toggles
  showDuplicates = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private budgetApi: BudgetApiService,
    private fb: FormBuilder,
    private notificationService: NotificationService
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
        const errorMessage = this.notificationService.handleError(error, 'Failed to load budget');
        this.notificationService.showError(errorMessage).subscribe();
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
      this.notificationService.showError('Please select a CSV file first').subscribe();
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
          successes: result.successes,
          duplicatesList: result.duplicates
        };
        
        if (result.errors.length === 0) {
          this.notificationService.showSuccess(`Successfully imported ${result.registeredOperations.length} operations`).subscribe();
          this.operationsApi.triggerRefresh(this.budgetId);
        } else {
          const errorMessage = result.errors.length > 5 
            ? `Import completed with ${result.errors.length} errors. Check the results below.`
            : `Import completed with errors. See details below.`;
          this.notificationService.showError(errorMessage).subscribe();
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to import operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  viewOperations(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }

  toggleDuplicates(): void {
    this.showDuplicates = !this.showDuplicates;
  }

  getDuplicatesList(): any[] {
    return this.importResult?.duplicatesList || [];
  }
}

