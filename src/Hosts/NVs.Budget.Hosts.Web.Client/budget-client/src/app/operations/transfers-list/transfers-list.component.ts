import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, catchError, of } from 'rxjs';
import { OperationsApiService } from '../operations-api.service';
import { TransferResponse, RegisterTransferRequest } from '../../budget/models';
import { 
  TuiButton, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel
} from '@taiga-ui/core';
import { TuiCardLarge } from '@taiga-ui/layout';
import { TransfersTableComponent } from '../transfers-table/transfers-table.component';
import { NotificationService } from '../shared/notification.service';
import { CriteriaFilterComponent } from '../shared/components/criteria-filter/criteria-filter.component';
import { CriteriaExample } from '../shared/models/example.interface';

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
    TuiCardLarge,
    TuiTitle,
    TransfersTableComponent,
    CriteriaFilterComponent
  ],
  templateUrl: './transfers-list.component.html',
  styleUrls: ['./transfers-list.component.less']
})
export class TransfersListComponent implements OnInit {
  budgetId!: string;
  transfers$!: Observable<TransferResponse[]>;
  transfers: TransferResponse[] = [];
  isLoading = false;
  
  currentCriteria = '';
  accuracy = '';
  
  // Registration form
  showRegisterForm = false;
  newTransfer: RegisterTransferRequest = {
    sourceId: '',
    sinkId: '',
    comment: '',
    accuracy: 'Likely'
  };
  
  accuracyOptions = ['Likely', 'Exact'];
  
  criteriaExamples: CriteriaExample[] = [
    { label: 'All transfers:', code: 'o => true' },
    { label: 'Exact accuracy:', code: 'o => o.Tags.Any(t => t.Value == "Transfer")' },
    { label: 'Likely accuracy:', code: 'o => o.Amount.Amount < 0' },
    { label: 'By comment:', code: 'o => o.Description.Contains("transfer")' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    this.loadTransfers();
  }

  loadTransfers(): void {
    this.transfers$ = this.operationsApi.searchTransfers(
      this.budgetId,
      this.currentCriteria || undefined,
      this.accuracy || undefined
    ).pipe(
      catchError(error => {
        const errorMessage = this.notificationService.handleError(error, 'Failed to load transfers');
        this.notificationService.showError(errorMessage).subscribe();
        return of([]);
      })
    );
    
    this.transfers$.subscribe(transfers => this.transfers = transfers);
  }

  onCriteriaSubmitted(criteria: string): void {
    this.currentCriteria = criteria;
    this.loadTransfers();
  }

  onCriteriaCleared(): void {
    this.currentCriteria = '';
    this.accuracy = '';
    this.loadTransfers();
  }

  onAccuracyChange(): void {
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

  navigateToOperations(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations']);
  }

  navigateToBudget(): void {
    this.router.navigate(['/budget', this.budgetId, 'details']);
  }
}

