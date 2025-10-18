import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, catchError, of } from 'rxjs';
import { OperationsApiService } from '../operations-api.service';
import { BudgetApiService } from '../../budget/budget-api.service';
import { BudgetResponse, OperationResponse } from '../../budget/models';
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
import { TuiChip, TuiAccordion } from '@taiga-ui/kit';

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
    TuiChip,
    TuiAccordion,
    TuiExpand
  ],
  templateUrl: './operations-list.component.html',
  styleUrls: ['./operations-list.component.less']
})
export class OperationsListComponent implements OnInit {
  budgetId!: string;
  budget: BudgetResponse | null = null;
  operations$!: Observable<OperationResponse[]>;
  isLoading = false;
  
  filterForm!: FormGroup;
  expandedOperationId: string | null = null;

  readonly columns = ['timestamp', 'description', 'amount', 'tags', 'actions'];

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

    this.loadBudget();
    this.loadOperations();
  }

  loadBudget(): void {
    this.budgetApi.getBudgetById(this.budgetId).subscribe({
      next: (budget) => {
        this.budget = budget || null;
      },
      error: (error) => {
        this.handleError(error, 'Failed to load budget');
      }
    });
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

  toggleOperationDetails(operationId: string): void {
    this.expandedOperationId = this.expandedOperationId === operationId ? null : operationId;
  }

  formatCurrency(amount: number, currencyCode: string): string {
    return `${amount.toFixed(2)} ${currencyCode}`;
  }

  formatDate(timestamp: string): string {
    const date = new Date(timestamp);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false
    });
  }

  navigateToImport(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'import']);
  }

  navigateToBudget(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }

  navigateToDelete(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'delete']);
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

