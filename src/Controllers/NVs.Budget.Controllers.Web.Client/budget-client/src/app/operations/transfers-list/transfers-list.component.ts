import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, catchError, of } from 'rxjs';
import { OperationsApiService } from '../operations-api.service';
import { TransferResponse, TransfersListResponse, RegisterTransferRequest } from '../../budget/models';
import { 
  TuiButton, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel
} from '@taiga-ui/core';
import { TuiChevron, TuiDataListWrapper, TuiSelect } from '@taiga-ui/kit';
import { TransfersTableComponent } from '../transfers-table/transfers-table.component';
import { NotificationService } from '../shared/notification.service';

@Component({
  selector: 'app-transfers-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TuiButton,
    TuiLoader,
    TuiTextfield,
    TuiLabel,
    TuiTitle,
    TuiChevron,
    TuiDataListWrapper,
    TuiSelect,
    TransfersTableComponent
  ],
  templateUrl: './transfers-list.component.html',
  styleUrls: ['./transfers-list.component.less']
})
export class TransfersListComponent implements OnInit {
  budgetId!: string;
  transfers$!: Observable<TransfersListResponse>;
  isLoading = false;
  selectedUnregisteredTransfers: TransferResponse[] = [];
  showRecordedTransfers = true;
  
  fromDate: string = '';
  tillDate: string = '';
  accuracy = '';
  accuracyFilter = 'All';
  
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

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    // Set default dates: last month to now
    const now = new Date();
    this.tillDate = this.formatDateForInput(now);
    const lastMonth = new Date(now.getFullYear(), now.getMonth() - 1, now.getDate());
    this.fromDate = this.formatDateForInput(lastMonth);
    this.loadTransfers();
  }

  private formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private parseDateInput(dateString: string): Date | null {
    if (!dateString) return null;
    const date = new Date(dateString);
    return isNaN(date.getTime()) ? null : date;
  }

  loadTransfers(): void {
    const from = this.parseDateInput(this.fromDate);
    const till = this.parseDateInput(this.tillDate);
    
    this.transfers$ = this.operationsApi.searchTransfers(
      this.budgetId,
      from || undefined,
      till || undefined,
      this.accuracy || undefined
    ).pipe(
      catchError(error => {
        const errorMessage = this.notificationService.handleError(error, 'Failed to load transfers');
        this.notificationService.showError(errorMessage).subscribe();
        return of({ recorded: [], unregistered: [] });
      })
    );
  }

  onDateChange(): void {
    this.loadTransfers();
  }

  onAccuracyChange(): void {
    this.accuracy = this.accuracyFilter === 'All' ? '' : this.accuracyFilter;
    this.loadTransfers();
  }

  resetFilters(): void {
    const now = new Date();
    this.tillDate = this.formatDateForInput(now);
    const lastMonth = new Date(now.getFullYear(), now.getMonth() - 1, now.getDate());
    this.fromDate = this.formatDateForInput(lastMonth);
    this.accuracy = '';
    this.accuracyFilter = 'All';
    this.loadTransfers();
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

    this.isLoading = true;

    this.operationsApi.registerTransfers(this.budgetId, {
      transfers: [this.newTransfer]
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.notificationService.showSuccess('Transfer registered successfully').subscribe();
        this.resetNewTransfer();
        this.showRegisterForm = false;
        this.loadTransfers();
      },
      error: (error) => {
        this.isLoading = false;
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

    this.isLoading = true;

    this.operationsApi.removeTransfers(this.budgetId, {
      sourceIds: [transfer.sourceId],
      all: false
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.notificationService.showSuccess('Transfer deleted successfully').subscribe();
        this.loadTransfers();
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to delete transfer');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  quickRegisterTransfer(transfer: TransferResponse): void {
    this.isLoading = true;

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
        this.isLoading = false;
        this.notificationService.showSuccess('Transfer registered successfully').subscribe();
        this.loadTransfers();
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to register transfer');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onUnregisteredSelectionChanged(transfers: TransferResponse[]): void {
    this.selectedUnregisteredTransfers = transfers;
  }

  registerSelectedTransfers(): void {
    if (this.selectedUnregisteredTransfers.length === 0) {
      this.notificationService.showError('Select at least one transfer to register').subscribe();
      return;
    }

    this.isLoading = true;
    const requests = this.selectedUnregisteredTransfers.map(transfer => ({
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
        this.isLoading = false;
        this.notificationService.showSuccess('Transfers registered successfully').subscribe();
        this.selectedUnregisteredTransfers = [];
        this.loadTransfers();
      },
      error: (error) => {
        this.isLoading = false;
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

