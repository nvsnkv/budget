import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
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
import { TuiTextarea } from '@taiga-ui/kit';

@Component({
  selector: 'app-delete-operations',
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
    TuiTextarea
  ],
  templateUrl: './delete-operations.component.html',
  styleUrls: ['./delete-operations.component.less']
})
export class DeleteOperationsComponent implements OnInit {
  budgetId!: string;
  budget: BudgetResponse | null = null;
  isLoading = false;
  
  deleteForm!: FormGroup;
  deleteResult: {
    errors: IError[];
    successes: ISuccess[];
  } | null = null;
  
  // Section toggles
  showExamples = false;
  showSuccesses = true;
  showErrors = true;

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
    
    this.deleteForm = this.fb.group({
      criteria: ['o => true', Validators.required]
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

  toggleExamples(): void {
    this.showExamples = !this.showExamples;
  }

  deleteOperations(): void {
    if (!this.deleteForm.valid || !this.budget) {
      this.showError('Please provide a valid criteria expression');
      return;
    }

    const criteria = this.deleteForm.value.criteria;
    const confirmMessage = `Are you sure you want to delete all operations matching the criteria:\n\n${criteria}\n\nThis action cannot be undone.`;
    
    const confirmed = confirm(confirmMessage);
    if (!confirmed) return;

    this.isLoading = true;
    this.deleteResult = null;

    this.operationsApi.removeOperations(this.budgetId, { criteria }).subscribe({
      next: (result) => {
        this.isLoading = false;
        this.deleteResult = {
          errors: result.errors,
          successes: result.successes
        };
        
        if (result.errors.length === 0) {
          this.showSuccess('Operations deleted successfully');
          this.operationsApi.triggerRefresh(this.budgetId);
        } else {
          const errorMessage = result.errors.length > 5 
            ? `Deletion completed with ${result.errors.length} errors. Check the results below.`
            : `Deletion completed with errors. See details below.`;
          this.showError(errorMessage);
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error, 'Failed to delete operations');
      }
    });
  }

  viewOperations(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations']);
  }

  resetForm(): void {
    this.deleteForm.patchValue({ criteria: 'o => true' });
    this.deleteResult = null;
  }

  toggleSuccesses(): void {
    this.showSuccesses = !this.showSuccesses;
  }

  toggleErrors(): void {
    this.showErrors = !this.showErrors;
  }

  // Helper method to access Object.keys in template
  getObjectKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
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

