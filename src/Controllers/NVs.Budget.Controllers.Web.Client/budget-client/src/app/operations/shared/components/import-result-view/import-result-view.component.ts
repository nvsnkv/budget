import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { TuiButton, TuiTitle } from '@taiga-ui/core';
import { ImportResultResponse, OperationResponse, RegisterTransferRequest, TransferResponse } from '../../../../budget/models';
import { OperationsTableComponent } from '../../../operations-table/operations-table.component';
import { TransfersTableComponent } from '../../../transfers-table/transfers-table.component';
import { NotificationService } from '../../notification.service';
import { OperationsHelperService } from '../../operations-helper.service';
import { OperationsApiService } from '../../../operations-api.service';
import { OperationResultComponent } from '../operation-result/operation-result.component';

@Component({
  selector: 'app-import-result-view',
  standalone: true,
  imports: [TuiButton, TuiTitle, OperationsTableComponent, TransfersTableComponent, OperationResultComponent],
  templateUrl: './import-result-view.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrls: ['./import-result-view.component.less']
})
export class ImportResultViewComponent {
  readonly budgetId = input.required<string>();
  readonly result = input<ImportResultResponse | null>(null);

  readonly registeredOperations = signal<OperationResponse[]>([]);
  readonly registeredTransfers = signal<TransferResponse[]>([]);
  readonly unregisteredTransfers = signal<TransferResponse[]>([]);
  readonly duplicatesList = signal<OperationResponse[][]>([]);

  readonly hasTransfers = computed(() => this.registeredTransfers().length > 0 || this.unregisteredTransfers().length > 0);

  isMutating = signal(false);
  showDuplicates = false;

  private readonly operationsApi = inject(OperationsApiService);
  private readonly operationsHelper = inject(OperationsHelperService);
  private readonly notificationService = inject(NotificationService);

  constructor() {
    effect(() => {
      const result = this.result();
      this.registeredOperations.set(result?.registeredOperations ? [...result.registeredOperations] : []);
      this.registeredTransfers.set(result?.registeredTransfers ? [...result.registeredTransfers] : []);
      this.unregisteredTransfers.set(result?.unregisteredTransfers ? [...result.unregisteredTransfers] : []);
      this.duplicatesList.set(result?.duplicates ? [...result.duplicates] : []);
      this.showDuplicates = false;
    });
  }

  toggleDuplicates(): void {
    this.showDuplicates = !this.showDuplicates;
  }

  onUpdateOperations(operations: OperationResponse[]): void {
    this.isMutating.set(true);

    this.operationsHelper.updateOperations(this.budgetId(), operations).subscribe({
      next: result => {
        this.isMutating.set(false);

        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map(e => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to update operations: ${errorMessage}`).subscribe();
          return;
        }

        this.notificationService.showSuccess(`Updated ${operations.length} operation${operations.length === 1 ? '' : 's'} successfully`).subscribe();
        this.operationsApi.triggerRefresh(this.budgetId());
      },
      error: error => {
        this.isMutating.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to update operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onDeleteOperations(operations: OperationResponse[]): void {
    const count = operations.length;
    if (count === 0) {
      return;
    }

    const confirmMessage = `Are you sure you want to delete ${count} operation${count === 1 ? '' : 's'}?\n\nThis action cannot be undone.`;
    if (!confirm(confirmMessage)) {
      return;
    }

    this.isMutating.set(true);

    this.operationsHelper.deleteOperations(this.budgetId(), operations.map(operation => operation.id)).subscribe({
      next: result => {
        this.isMutating.set(false);

        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map((e: any) => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to delete operations: ${errorMessage}`).subscribe();
          return;
        }

        this.notificationService.showSuccess(`Deleted ${count} operation${count === 1 ? '' : 's'} successfully`).subscribe();
        const deletedIds = new Set(operations.map(operation => operation.id));
        this.registeredOperations.update(operations => operations.filter(operation => !deletedIds.has(operation.id)));
        this.operationsApi.triggerRefresh(this.budgetId());
      },
      error: error => {
        this.isMutating.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to delete operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onUpdateOperationNote(operation: OperationResponse): void {
    const current = this.registeredOperations().find(o => o.id === operation.id);
    const previousNotes = current?.notes ?? '';

    this.operationsHelper.updateOperation(this.budgetId(), operation).subscribe({
      next: result => {
        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map(e => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to update notes: ${errorMessage}`).subscribe();
          if (current) {
            current.notes = previousNotes;
          }
          return;
        }

        this.operationsApi.triggerRefresh(this.budgetId());
      },
      error: error => {
        const errorMessage = this.notificationService.handleError(error, 'Failed to update notes');
        this.notificationService.showError(errorMessage).subscribe();
        if (current) {
          current.notes = previousNotes;
        }
      }
    });
  }

  onTransferRegistered(transfer: TransferResponse): void {
    this.isMutating.set(true);

    const request: RegisterTransferRequest = {
      sourceId: transfer.sourceId,
      sinkId: transfer.sinkId,
      comment: transfer.comment,
      accuracy: transfer.accuracy,
      fee: transfer.fee
    };

    this.operationsApi.registerTransfers(this.budgetId(), { transfers: [request] }).subscribe({
      next: () => {
        this.isMutating.set(false);
        this.notificationService.showSuccess('Transfer registered successfully').subscribe();
        this.unregisteredTransfers.update(transfers => transfers.filter(t => t.sourceId !== transfer.sourceId));
        this.registeredTransfers.update(transfers => [...transfers, transfer]);
        this.operationsApi.triggerRefresh(this.budgetId());
      },
      error: error => {
        this.isMutating.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to register transfer');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onTransferDeleted(transfer: TransferResponse): void {
    const confirmMessage = `Are you sure you want to unregister this transfer?\n\nSource: ${transfer.source.description}\nSink: ${transfer.sink.description}\nFee: ${transfer.fee.value} ${transfer.fee.currencyCode}\n\nOperations will be kept, only the transfer link is removed.`;

    if (!confirm(confirmMessage)) {
      return;
    }

    this.isMutating.set(true);

    this.operationsApi.removeTransfers(this.budgetId(), {
      sourceIds: [transfer.sourceId],
      all: false
    }).subscribe({
      next: () => {
        this.isMutating.set(false);
        this.notificationService.showSuccess('Transfer unregistered successfully').subscribe();
        this.registeredTransfers.update(transfers => transfers.filter(t => t.sourceId !== transfer.sourceId));
        this.unregisteredTransfers.update(transfers => [...transfers, transfer]);
        this.operationsApi.triggerRefresh(this.budgetId());
      },
      error: error => {
        this.isMutating.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to unregister transfer');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }
}
