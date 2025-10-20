import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OperationsApiService } from '../operations-api.service';
import { BudgetApiService } from '../../budget/budget-api.service';
import { OperationResponse, BudgetResponse } from '../../budget/models';
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
import { OperationsTableComponent } from '../operations-table/operations-table.component';

@Component({
  selector: 'app-duplicates-list',
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
    TuiTextarea,
    OperationsTableComponent
  ],
  templateUrl: './duplicates-list.component.html',
  styleUrls: ['./duplicates-list.component.less']
})
export class DuplicatesListComponent implements OnInit {
  budgetId!: string;
  duplicateGroups: OperationResponse[][] = [];
  isLoading = false;
  
  filterForm!: FormGroup;
  showExamples = false;

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
    
    this.filterForm = this.fb.group({
      criteria: ['o => true']
    });

    this.loadDuplicates();
  }

  loadDuplicates(): void {
    this.isLoading = true;
    const criteria = this.filterForm.value.criteria || undefined;
    
    this.operationsApi.getDuplicates(this.budgetId, criteria).subscribe({
      next: (groups) => {
        this.duplicateGroups = groups;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load duplicates', error);
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    this.loadDuplicates();
  }

  clearFilters(): void {
    this.filterForm.patchValue({ criteria: 'o => true' });
    this.loadDuplicates();
  }

  toggleExamples(): void {
    this.showExamples = !this.showExamples;
  }

  onCriteriaKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && event.ctrlKey) {
      event.preventDefault();
      this.applyFilters();
    }
  }

  navigateToOperations(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }

  navigateToBudget(): void {
    this.router.navigate(['/budget', this.budgetId, 'details']);
  }

  getTotalDuplicates(): number {
    return this.duplicateGroups.reduce((total, group) => total + group.length, 0);
  }

  onDeleteOperation(operation: OperationResponse): void {
    const confirmMessage = `Are you sure you want to delete this operation?\n\n${operation.description}\n${operation.amount.value} ${operation.amount.currencyCode}\n\nThis action cannot be undone.`;
    
    if (!confirm(confirmMessage)) {
      return;
    }

    this.isLoading = true;
    const criteria = `o => o.Id == Guid.Parse("${operation.id}")`;
    
    this.operationsApi.removeOperations(this.budgetId, { criteria }).subscribe({
      next: (result) => {
        this.isLoading = false;
        
        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map(e => e.message || 'Unknown error').join('; ');
          this.showError(`Failed to delete operation: ${errorMessage}`);
        } else {
          this.showSuccess('Operation deleted successfully');
          this.loadDuplicates();
        }
      },
      error: (error: any) => {
        this.isLoading = false;
        this.handleError(error, 'Failed to delete operation');
      }
    });
  }

  onUpdateOperation(operation: OperationResponse): void {
    this.isLoading = true;
    
    // Get the budget to access its version
    this.budgetApi.getAllBudgets().subscribe({
      next: (budgetList: BudgetResponse[]) => {
        const budget = budgetList.find((b: BudgetResponse) => b.id === this.budgetId);
        if (!budget) {
          this.isLoading = false;
          this.showError('Budget not found');
          return;
        }

        const request = {
          operations: [operation],
          budgetVersion: budget.version,
          transferConfidenceLevel: undefined,
          taggingMode: 'Skip'
        };

        this.operationsApi.updateOperations(this.budgetId, request).subscribe({
          next: (result) => {
            this.isLoading = false;
            
            if (result.errors && result.errors.length > 0) {
              const errorMessage = result.errors.map(e => e.message || 'Unknown error').join('; ');
              this.showError(`Failed to update operation: ${errorMessage}`);
            } else {
              this.showSuccess('Operation updated successfully');
              this.loadDuplicates();
            }
          },
          error: (error: any) => {
            this.isLoading = false;
            this.handleError(error, 'Failed to update operation');
          }
        });
      },
      error: (error: any) => {
        this.isLoading = false;
        this.handleError(error, 'Failed to load budget');
      }
    });
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

