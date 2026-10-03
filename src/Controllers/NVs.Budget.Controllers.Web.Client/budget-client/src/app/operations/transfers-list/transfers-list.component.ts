import { ChangeDetectionStrategy, Component, computed, inject, resource, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { catchError } from 'rxjs';
import { OperationsApiService } from '../operations-api.service';
import { TransferResponse, TransfersListResponse, RegisterTransferRequest } from '../../budget/models';
import { TuiButton, TuiLoader, TuiTitle, TuiLabel, TuiInput } from '@taiga-ui/core';
import { TuiChevron, TuiDataListWrapper, TuiSelect } from '@taiga-ui/kit';
import { TransfersTableComponent } from '../transfers-table/transfers-table.component';
import { NotificationService } from '../shared/notification.service';
import { calendarDateToUtcExclusiveEnd, calendarDateToUtcStart } from '../../shared/date-api.utils';

interface TransfersFilters {
  from: string;
  till: string;
  accuracy: string;
}

@Component({
  selector: 'app-transfers-list',
  standalone: true,
  imports: [
    FormsModule,
    TuiButton,
    TuiLoader,
    TuiInput,
    TuiLabel,
    TuiTitle,
    TuiChevron,
    TuiDataListWrapper,
    TuiSelect,
    TransfersTableComponent
  ],
  templateUrl: './transfers-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrls: ['./transfers-list.component.less']
})
export class TransfersListComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly operationsApi = inject(OperationsApiService);
  private readonly notificationService = inject(NotificationService);

  readonly budgetId: string = this.route.snapshot.params['budgetId'];

  fromDate = signal(this.formatDateForInput(new Date(new Date().getFullYear(), new Date().getMonth() - 1, new Date().getDate())));
  tillDate = signal(this.formatDateForInput(new Date()));
  accuracyFilter = signal('All');

  isMutating = signal(false);
  selectedUnregisteredTransfers = signal<TransferResponse[]>([]);
  showRecordedTransfers = true;

  // Registration form
  showRegisterForm = false;
  newTransfer: RegisterTransferRequest = {
    sourceId: '',
    sinkId: '',
    comment: '',
    accuracy: 'Likely'
  };

  accuracyOptions = ['Likely', 'Exact'];
  readonly accuracyFilterOptions = ['All', ...this.accuracyOptions];

  private readonly transfersResource = resource<TransfersListResponse, TransfersFilters>({
    params: () => ({
      from: this.fromDate(),
      till: this.tillDate(),
      accuracy: this.accuracyFilter() === 'All' ? '' : this.accuracyFilter(),
    }),
    loader: ({ params }) => firstValueFrom(
      this.operationsApi.searchTransfers(
        this.budgetId,
        params.from ? calendarDateToUtcStart(params.from) : undefined,
        params.till ? calendarDateToUtcExclusiveEnd(params.till) : undefined,
        params.accuracy || undefined
      ).pipe(
        catchError(error => {
          const errorMessage = this.notificationService.handleError(error, 'Failed to load transfers');
          this.notificationService.showError(errorMessage).subscribe();
          return of({ recorded: [], unregistered: [] });
        })
      )
    ),
  });

  readonly transfers = computed(() => this.transfersResource.value());

  resetFilters(): void {
    const now = new Date();
    this.tillDate.set(this.formatDateForInput(now));
    this.fromDate.set(this.formatDateForInput(new Date(now.getFullYear(), now.getMonth() - 1, now.getDate())));
    this.accuracyFilter.set('All');
  }

  private formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  toggleRegisterForm(): void {
    this.showRegisterForm = !this.showRegisterForm;
    if (!this.showRegisterForm) {
      this.resetNewTransfer();
    }
  }

  resetNewTransfer(): void {
    this.newTransfer = {
      sourceId: '',
      sinkId: '',
      comment: '',
      accuracy: 'Likely'
    };
  }

  registerTransfer(): void {
    if (!this.newTransfer.sourceId || !this.newTransfer.sinkId || !this.newTransfer.comment) {
      this.notificationService.showError('Please fill in all required fields').subscribe();
      return;
    }

    this.isMutating.set(true);

    this.operationsApi.registerTransfers(this.budgetId, {
      transfers: [this.newTransfer]
    }).subscribe({
      next: () => {
        this.isMutating.set(false);
        this.notificationService.showSuccess('Transfer registered successfully').subscribe();
        this.resetNewTransfer();
        this.showRegisterForm = false;
        this.transfersResource.reload();
      },
      error: (error) => {
        this.isMutating.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to register transfer');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onDeleteTransfer(transfer: TransferResponse): void {
    const confirmMessage = `Are you sure you want to delete this transfer?\n\nSource: ${transfer.source.description}\nSink: ${transfer.sink.description}\nFee: ${transfer.fee.value} ${transfer.fee.currencyCode}\n\nThis action cannot be undone.`;

    if (!confirm(confirmMessage)) {
      return;
    }

    this.isMutating.set(true);

    this.operationsApi.removeTransfers(this.budgetId, {
      sourceIds: [transfer.sourceId],
      all: false
    }).subscribe({
      next: () => {
        this.isMutating.set(false);
        this.notificationService.showSuccess('Transfer deleted successfully').subscribe();
        this.transfersResource.reload();
      },
      error: (error) => {
        this.isMutating.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to delete transfer');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  quickRegisterTransfer(transfer: TransferResponse): void {
    this.isMutating.set(true);

    const request: RegisterTransferRequest = {
      sourceId: transfer.sourceId,
      sinkId: transfer.sinkId,
      comment: transfer.comment,
      accuracy: transfer.accuracy,
      fee: transfer.fee
    };

    this.operationsApi.registerTransfers(this.budgetId, {
      transfers: [request]
    }).subscribe({
      next: () => {
        this.isMutating.set(false);
        this.notificationService.showSuccess('Transfer registered successfully').subscribe();
        this.transfersResource.reload();
      },
      error: (error) => {
        this.isMutating.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to register transfer');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onUnregisteredSelectionChanged(transfers: TransferResponse[]): void {
    this.selectedUnregisteredTransfers.set(transfers);
  }

  registerSelectedTransfers(): void {
    if (this.selectedUnregisteredTransfers().length === 0) {
      this.notificationService.showError('Select at least one transfer to register').subscribe();
      return;
    }

    this.isMutating.set(true);
    const requests = this.selectedUnregisteredTransfers().map(transfer => ({
      sourceId: transfer.sourceId,
      sinkId: transfer.sinkId,
      comment: transfer.comment,
      accuracy: transfer.accuracy,
      fee: transfer.fee
    }));

    this.operationsApi.registerTransfers(this.budgetId, {
      transfers: requests
    }).subscribe({
      next: () => {
        this.isMutating.set(false);
        this.notificationService.showSuccess('Transfers registered successfully').subscribe();
        this.selectedUnregisteredTransfers.set([]);
        this.transfersResource.reload();
      },
      error: (error) => {
        this.isMutating.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to register transfers');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  toggleRecordedTransfers(): void {
    this.showRecordedTransfers = !this.showRecordedTransfers;
  }

  navigateToOperations(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations']);
  }

  navigateToBudget(): void {
    this.router.navigate(['/budget', this.budgetId, 'details']);
  }
}
