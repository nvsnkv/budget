import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, catchError, of } from 'rxjs';
import { OperationsApiService } from '../operations-api.service';
import { OperationResponse } from '../../budget/models';
import {
  TuiButton,
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel
} from '@taiga-ui/core';
import { TuiCardLarge } from '@taiga-ui/layout';
import {TuiCheckbox, TuiChevron, TuiDataListWrapper, TuiSelect} from '@taiga-ui/kit';
import { OperationsTableComponent } from '../operations-table/operations-table.component';
import { NotificationService } from '../shared/notification.service';
import { OperationsHelperService } from '../shared/operations-helper.service';
import { CriteriaFilterComponent } from '../shared/components/criteria-filter/criteria-filter.component';
import { CriteriaExample } from '../shared/models/example.interface';

@Component({
  selector: 'app-operations-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TuiButton,
    TuiLoader,
    TuiTextfield,
    TuiChevron,
    TuiDataListWrapper,
    TuiLabel,
    TuiCardLarge,
    TuiTitle,
    TuiCheckbox,
    OperationsTableComponent,
    CriteriaFilterComponent,
    TuiSelect
  ],
  templateUrl: './operations-list.component.html',
  styleUrls: ['./operations-list.component.less']
})
export class OperationsListComponent implements OnInit {
  budgetId!: string;
  operations$!: Observable<OperationResponse[]>;
  operations: OperationResponse[] = [];
  isLoading = false;

  currentCriteria = `o => o.Timestamp.Year == ${new Date().getFullYear()} && o.Timestamp.Month >= ${new Date().getMonth()}`;
  outputCurrency = '';
  excludeTransfers = true;

  criteriaExamples: CriteriaExample[] = [
    { label: 'All operations:', code: 'o => true' },
    { label: 'Positive amounts:', code: 'o => o.Amount.Amount > 0' },
    { label: 'Negative amounts:', code: 'o => o.Amount.Amount < 0' },
    { label: 'Specific year:', code: 'o => o.Timestamp.Year == 2023' },
    { label: 'Contains text:', code: 'o => o.Description.Contains("groceries")' },
    { label: 'By tag:', code: 'o => o.Tags.Any(t => t.Value == "food")' },
    { label: 'Without tags:', code: 'o => o.Tags.Count == 0' },
    { label: 'Amount range:', code: 'o => o.Amount.Amount >= -1000 && o.Amount.Amount <= -100' }
  ];

  readonly items: string[] = ["RUB", "USD", "EUR"];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private notificationService: NotificationService,
    private operationsHelper: OperationsHelperService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    this.loadOperations();
  }

  loadOperations(): void {
    this.operations$ = this.operationsApi.getOperations(
      this.budgetId,
      this.currentCriteria || undefined,
      this.outputCurrency || undefined,
      this.excludeTransfers
    ).pipe(
      catchError(error => {
        const errorMessage = this.notificationService.handleError(error, 'Failed to load operations');
        this.notificationService.showError(errorMessage).subscribe();
        return of([]);
      })
    );

    // Subscribe to update the array for the table component
    this.operations$.subscribe(ops => this.operations = ops);
  }

  onCriteriaSubmitted(criteria: string): void {
    this.currentCriteria = criteria;
    this.loadOperations();
  }

  onCriteriaCleared(): void {
    this.currentCriteria = '';
    this.outputCurrency = '';
    this.excludeTransfers = false;
    this.loadOperations();
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

  navigateToRetag(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'retag']);
  }

  navigateToLogbook(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'logbook']);
  }

  navigateToTransfers(): void {
    this.router.navigate(['/budget', this.budgetId, 'transfers']);
  }

  onDeleteOperation(operation: OperationResponse): void {
    const confirmMessage = `Are you sure you want to delete this operation?\n\n${operation.description}\n${operation.amount.value} ${operation.amount.currencyCode}\n\nThis action cannot be undone.`;

    if (!confirm(confirmMessage)) {
      return;
    }

    this.isLoading = true;

    this.operationsHelper.deleteOperation(this.budgetId, operation.id).subscribe({
      next: (result) => {
        this.isLoading = false;

        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map((e: any) => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to delete operation: ${errorMessage}`).subscribe();
        } else {
          this.notificationService.showSuccess('Operation deleted successfully').subscribe();
          this.loadOperations();
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to delete operation');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onUpdateOperation(operation: OperationResponse): void {
    this.isLoading = true;

    this.operationsHelper.updateOperation(this.budgetId, operation).subscribe({
      next: (result) => {
        this.isLoading = false;

        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map(e => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to update operation: ${errorMessage}`).subscribe();
        } else {
          this.notificationService.showSuccess('Operation updated successfully').subscribe();
          this.loadOperations();
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to update operation');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }
}
