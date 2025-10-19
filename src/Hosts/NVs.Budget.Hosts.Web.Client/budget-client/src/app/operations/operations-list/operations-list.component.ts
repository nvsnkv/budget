import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, catchError, of } from 'rxjs';
import { OperationsApiService } from '../operations-api.service';
import { BudgetApiService } from '../../budget/budget-api.service';
import { OperationResponse, BudgetResponse } from '../../budget/models';
import { 
  TuiButton, 
  TuiDialogService, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel,
  TuiExpand
} from '@taiga-ui/core';
import { TuiCardLarge } from '@taiga-ui/layout';
import { TuiAccordion, TuiTextarea, TuiCheckbox } from '@taiga-ui/kit';
import { OperationsTableComponent } from '../operations-table/operations-table.component';

@Component({
  selector: 'app-operations-list',
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
    TuiAccordion,
    TuiExpand,
    TuiTextarea,
    TuiCheckbox,
    OperationsTableComponent
  ],
  templateUrl: './operations-list.component.html',
  styleUrls: ['./operations-list.component.less']
})
export class OperationsListComponent implements OnInit {
  budgetId!: string;
  operations$!: Observable<OperationResponse[]>;
  operations: OperationResponse[] = [];
  isLoading = false;
  
  filterForm!: FormGroup;

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
      criteria: [''],
      outputCurrency: [''],
      excludeTransfers: [false]
    });

    this.loadOperations();
  }

  loadOperations(): void {
    const formValue = this.filterForm.value;
    this.operations$ = this.operationsApi.getOperations(
      this.budgetId,
      formValue.criteria || undefined,
      formValue.outputCurrency || undefined,
      formValue.excludeTransfers
    ).pipe(
      catchError(error => {
        this.handleError(error, 'Failed to load operations');
        return of([]);
      })
    );
    
    // Subscribe to update the array for the table component
    this.operations$.subscribe(ops => this.operations = ops);
  }

  applyFilters(): void {
    this.loadOperations();
  }

  clearFilters(): void {
    this.filterForm.reset({
      criteria: '',
      outputCurrency: '',
      excludeTransfers: false
    });
    this.loadOperations();
  }

  onCriteriaKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && event.ctrlKey) {
      event.preventDefault();
      this.applyFilters();
    }
  }

  navigateToImport(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'import']);
  }

  navigateToBudget(): void {
    this.router.navigate(['/budget', this.budgetId, 'details']);
  }

  navigateToDelete(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'delete']);
  }

  navigateToDuplicates(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'duplicates']);
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
          this.loadOperations();
        }
      },
      error: (error) => {
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
              this.loadOperations();
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

  // Helper method for template to access Object.keys
  getObjectKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }
}

