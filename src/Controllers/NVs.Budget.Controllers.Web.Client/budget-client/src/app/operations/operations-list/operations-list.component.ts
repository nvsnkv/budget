import { ChangeDetectionStrategy, Component, computed, inject, resource, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { catchError } from 'rxjs';
import { OperationsApiService } from '../operations-api.service';
import { OperationResponse } from '../../budget/models';
import { TuiButton, TuiLoader, TuiTitle } from '@taiga-ui/core';
import {TuiChevron, TuiDataListWrapper, TuiSelect} from '@taiga-ui/kit';
import { OperationsTableComponent } from '../operations-table/operations-table.component';
import { NotificationService } from '../shared/notification.service';
import { OperationsHelperService } from '../shared/operations-helper.service';
import { CriteriaFilterComponent } from '../shared/components/criteria-filter/criteria-filter.component';

interface OperationsFilters {
  criteria: string;
  outputCurrency: string;
  excludeTransfers: boolean;
}

@Component({
  selector: 'app-operations-list',
  standalone: true,
  imports: [
    FormsModule,
    TuiButton,
    TuiLoader,
    TuiChevron,
    TuiDataListWrapper,
    TuiTitle,
    OperationsTableComponent,
    CriteriaFilterComponent,
    TuiSelect
  ],
  templateUrl: './operations-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrls: ['./operations-list.component.less']
})
export class OperationsListComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly operationsApi = inject(OperationsApiService);
  private readonly notificationService = inject(NotificationService);
  private readonly operationsHelper = inject(OperationsHelperService);

  readonly budgetId: string = this.route.snapshot.params['budgetId'];

  currentCriteria = signal(`o => o.Timestamp.Year == ${new Date().getFullYear()} && o.Timestamp.Month >= ${new Date().getMonth()}`);
  outputCurrency = signal('');
  excludeTransfers = signal(true);
  isMutating = signal(false);

  readonly items: string[] = ["RUB", "USD", "EUR"];

  private readonly operationsResource = resource<OperationResponse[], OperationsFilters>({
    params: () => ({
      criteria: this.currentCriteria(),
      outputCurrency: this.outputCurrency(),
      excludeTransfers: this.excludeTransfers(),
    }),
    loader: ({ params }) => firstValueFrom(
      this.operationsApi.getOperations(
        this.budgetId,
        params.criteria || undefined,
        params.outputCurrency || undefined,
        params.excludeTransfers
      ).pipe(
        catchError(error => {
          const errorMessage = this.notificationService.handleError(error, 'Failed to load operations');
          this.notificationService.showError(errorMessage).subscribe();
          return of<OperationResponse[]>([]);
        })
      )
    ),
  });

  readonly operations = computed(() => this.operationsResource.value() ?? []);

  onCriteriaSubmitted(criteria: string): void {
    this.currentCriteria.set(criteria);
  }

  onCriteriaCleared(): void {
    this.currentCriteria.set('o => true');
    this.outputCurrency.set('');
    this.excludeTransfers.set(false);
  }

  navigateToImport(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'import']);
  }

  navigateToManualImport(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'manual-import']);
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

  navigateToBulkChanges(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'bulk-changes']);
  }

  navigateToLogbook(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'logbook']);
  }

  navigateToTransfers(): void {
    this.router.navigate(['/budget', this.budgetId, 'transfers']);
  }

  onDeleteOperations(operations: OperationResponse[]): void {
    const count = operations.length;
    if (count === 0) return;

    const confirmMessage = `Are you sure you want to delete ${count} operation${count === 1 ? '' : 's'}?\n\nThis action cannot be undone.`;
    if (!confirm(confirmMessage)) {
      return;
    }

    this.isMutating.set(true);

    this.operationsHelper.deleteOperations(this.budgetId, operations.map(operation => operation.id)).subscribe({
      next: (result) => {
        this.isMutating.set(false);

        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map((e: any) => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to delete operations: ${errorMessage}`).subscribe();
        } else {
          this.notificationService.showSuccess(`Deleted ${count} operation${count === 1 ? '' : 's'} successfully`).subscribe();
          this.operationsResource.reload();
        }
      },
      error: (error) => {
        this.isMutating.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to delete operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onUpdateOperations(operations: OperationResponse[]): void {
    this.isMutating.set(true);

    this.operationsHelper.updateOperations(this.budgetId, operations).subscribe({
      next: (result) => {
        this.isMutating.set(false);

        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map(e => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to update operations: ${errorMessage}`).subscribe();
        } else {
          const count = result.updatedOperations?.length ?? operations.length;
          this.notificationService.showSuccess(`Updated ${count} operation${count === 1 ? '' : 's'} successfully`).subscribe();
          this.operationsResource.reload();
        }
      },
      error: (error) => {
        this.isMutating.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to update operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onUpdateOperationNote(operation: OperationResponse): void {
    const current = this.operations().find(o => o.id === operation.id);
    const previousNotes = current?.notes ?? '';

    this.operationsHelper.updateOperation(this.budgetId, operation).subscribe({
      next: (result) => {
        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map(e => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to update notes: ${errorMessage}`).subscribe();
          if (current) {
            current.notes = previousNotes;
          }
          return;
        }

        this.operationsResource.reload();
      },
      error: (error) => {
        const errorMessage = this.notificationService.handleError(error, 'Failed to update notes');
        this.notificationService.showError(errorMessage).subscribe();
        if (current) {
          current.notes = previousNotes;
        }
      }
    });
  }
}
