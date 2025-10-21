import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OperationsApiService } from '../operations-api.service';
import { 
  TuiButton, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel
} from '@taiga-ui/core';
import { TuiCardLarge } from '@taiga-ui/layout';
import { TuiAccordion } from '@taiga-ui/kit';
import { NotificationService } from '../shared/notification.service';
import { OperationsHelperService } from '../shared/operations-helper.service';
import { CriteriaFilterComponent } from '../shared/components/criteria-filter/criteria-filter.component';
import { OperationsTableComponent } from '../operations-table/operations-table.component';
import { CriteriaExample } from '../shared/models/example.interface';
import { LogbookEntryResponse, LogbookResponse, OperationResponse } from '../../budget/models';
import { DateFormatPipe } from '../shared/pipes/date-format.pipe';
import { CurrencyFormatPipe } from '../shared/pipes/currency-format.pipe';

@Component({
  selector: 'app-logbook-view',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TuiButton,
    TuiLoader,
    TuiCardLarge,
    TuiTitle,
    TuiTextfield,
    TuiLabel,
    TuiAccordion,
    CriteriaFilterComponent,
    OperationsTableComponent,
    DateFormatPipe,
    CurrencyFormatPipe
  ],
  templateUrl: './logbook-view.component.html',
  styleUrls: ['./logbook-view.component.less']
})
export class LogbookViewComponent implements OnInit {
  budgetId!: string;
  isLoading = false;
  logbook: LogbookResponse | null = null;
  
  currentCriteria = '';
  fromDate: string = '';
  tillDate: string = '';
  
  expandedEntries = new Set<string>();
  expandedOperations = new Set<string>();
  
  criteriaExamples: CriteriaExample[] = [
    { label: 'All operations:', code: 'o => true' },
    { label: 'Positive amounts:', code: 'o => o.Amount.Amount > 0' },
    { label: 'Negative amounts:', code: 'o => o.Amount.Amount < 0' },
    { label: 'Contains text:', code: 'o => o.Description.Contains("groceries")' },
    { label: 'By tag:', code: 'o => o.Tags.Any(t => t.Value == "food")' },
    { label: 'Amount range:', code: 'o => o.Amount.Amount >= -1000 && o.Amount.Amount <= -100' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private notificationService: NotificationService,
    private operationsHelper: OperationsHelperService
  ) {
    this.resetToDefaultDateRange();
  }

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    this.loadLogbook();
  }

  onCriteriaSubmitted(criteria: string): void {
    this.currentCriteria = criteria;
    this.loadLogbook();
  }

  onCriteriaCleared(): void {
    this.currentCriteria = '';
    this.resetToDefaultDateRange();
    this.loadLogbook();
  }

  private resetToDefaultDateRange(): void {
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59);
    
    this.fromDate = firstDay.toISOString().slice(0, 16);
    this.tillDate = lastDay.toISOString().slice(0, 16);
  }

  loadLogbook(): void {
    this.isLoading = true;
    this.logbook = null;
    this.expandedEntries.clear();
    this.expandedOperations.clear();

    const from = this.fromDate ? new Date(this.fromDate) : undefined;
    const till = this.tillDate ? new Date(this.tillDate) : undefined;

    this.operationsApi.getLogbook(
      this.budgetId,
      from,
      till,
      this.currentCriteria || undefined
    ).subscribe({
      next: (result) => {
        this.isLoading = false;
        this.logbook = result;
        
        if (result.errors.length === 0) {
          this.notificationService.showSuccess('Logbook loaded successfully').subscribe();
        } else {
          const errorMessage = `Logbook loaded with ${result.errors.length} errors`;
          this.notificationService.showWarning(errorMessage).subscribe();
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to load logbook');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  toggleEntry(path: string): void {
    if (this.expandedEntries.has(path)) {
      this.expandedEntries.delete(path);
    } else {
      this.expandedEntries.add(path);
    }
  }

  isExpanded(path: string): boolean {
    return this.expandedEntries.has(path);
  }

  toggleOperations(path: string, event: Event): void {
    event.stopPropagation();
    if (this.expandedOperations.has(path)) {
      this.expandedOperations.delete(path);
    } else {
      this.expandedOperations.add(path);
    }
  }

  areOperationsExpanded(path: string): boolean {
    return this.expandedOperations.has(path);
  }

  getEntryPath(parent: string, description: string): string {
    return parent ? `${parent}/${description}` : description;
  }

  hasChildren(entry: LogbookEntryResponse): boolean {
    return entry.children && entry.children.length > 0;
  }

  viewOperations(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }

  setDateRange(type: 'currentMonth' | 'lastMonth' | 'currentYear' | 'lastYear'): void {
    const now = new Date();
    let from: Date;
    let till: Date;

    switch (type) {
      case 'currentMonth':
        from = new Date(now.getFullYear(), now.getMonth(), 1);
        till = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59);
        break;
      case 'lastMonth':
        from = new Date(now.getFullYear(), now.getMonth() - 1, 1);
        till = new Date(now.getFullYear(), now.getMonth(), 0, 23, 59, 59);
        break;
      case 'currentYear':
        from = new Date(now.getFullYear(), 0, 1);
        till = new Date(now.getFullYear(), 11, 31, 23, 59, 59);
        break;
      case 'lastYear':
        from = new Date(now.getFullYear() - 1, 0, 1);
        till = new Date(now.getFullYear() - 1, 11, 31, 23, 59, 59);
        break;
    }

    this.fromDate = from.toISOString().slice(0, 16);
    this.tillDate = till.toISOString().slice(0, 16);
    this.loadLogbook();
  }

  hasMetadata(error: any): boolean {
    return error.metadata && Object.keys(error.metadata).length > 0;
  }

  getMetadataKeys(metadata: Record<string, any>): string[] {
    return Object.keys(metadata);
  }

  formatMetadataValue(value: any): string {
    if (value === null || value === undefined) {
      return 'null';
    }
    if (typeof value === 'object') {
      try {
        return JSON.stringify(value, null, 2);
      } catch {
        return String(value);
      }
    }
    return String(value);
  }

  hasNestedReasons(error: any): boolean {
    return error.reasons && error.reasons.length > 0;
  }

  hasOperations(entry: LogbookEntryResponse): boolean {
    return entry.operations && entry.operations.length > 0;
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
          this.loadLogbook();
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
          this.loadLogbook();
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

