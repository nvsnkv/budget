import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { OperationsApiService } from '../operations-api.service';
import { 
  TuiButton, 
  TuiLoader,
  TuiTitle
} from '@taiga-ui/core';
import { TuiCardLarge } from '@taiga-ui/layout';
import { NotificationService } from '../shared/notification.service';
import { OperationsHelperService } from '../shared/operations-helper.service';
import { OperationsTableComponent } from '../operations-table/operations-table.component';
import { LogbookResponse, OperationResponse } from '../../budget/models';

@Component({
  selector: 'app-logbook-group',
  standalone: true,
  imports: [
    CommonModule,
    TuiButton,
    TuiLoader,
    TuiCardLarge,
    TuiTitle,
    OperationsTableComponent
  ],
  templateUrl: './logbook-group.component.html',
  styleUrls: ['./logbook-group.component.less']
})
export class LogbookGroupComponent implements OnInit {
  budgetId!: string;
  rangeName!: string;
  criteriaPath!: string;
  fromDate!: string;
  tillDate!: string;
  criteria?: string;
  cronExpression?: string;
  
  isLoading = false;
  operations: OperationResponse[] = [];
  groupTitle = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private notificationService: NotificationService,
    private operationsHelper: OperationsHelperService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    this.rangeName = this.route.snapshot.queryParams['rangeName'] || '';
    this.criteriaPath = this.route.snapshot.queryParams['criteriaPath'] || '';
    this.fromDate = this.route.snapshot.queryParams['from'] || '';
    this.tillDate = this.route.snapshot.queryParams['till'] || '';
    this.criteria = this.route.snapshot.queryParams['criteria'];
    this.cronExpression = this.route.snapshot.queryParams['cronExpression'];
    
    const pathParts = this.criteriaPath.split('/');
    const criteriaName = pathParts[pathParts.length - 1] || 'Group';
    this.groupTitle = `${criteriaName} - ${this.rangeName}`;
    
    this.loadOperations();
  }

  loadOperations(): void {
    this.isLoading = true;
    
    const from = this.fromDate ? new Date(this.fromDate) : undefined;
    const till = this.tillDate ? new Date(this.tillDate) : undefined;

    this.operationsApi.getLogbook(
      this.budgetId,
      from,
      till,
      this.criteria,
      this.cronExpression
    ).subscribe({
      next: (result: LogbookResponse) => {
        this.isLoading = false;
        
        // Find the specific range and criteria path
        const rangedEntry = result.ranges.find(r => r.range.name === this.rangeName);
        if (rangedEntry) {
          const entry = this.findEntryByPath(rangedEntry.entry, this.criteriaPath);
          if (entry) {
            this.operations = entry.operations || [];
          }
        }
        
        if (this.operations.length === 0) {
          this.notificationService.showWarning('No operations found for this group').subscribe();
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to load operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  private findEntryByPath(entry: any, targetPath: string): any {
    const currentPath = entry.description;
    
    if (currentPath === targetPath) {
      return entry;
    }
    
    if (targetPath.startsWith(currentPath + '/')) {
      const remainingPath = targetPath.substring(currentPath.length + 1);
      
      for (const child of (entry.children || [])) {
        const found = this.findEntryByPath(child, remainingPath);
        if (found) return found;
      }
    }
    
    return null;
  }

  backToLogbook(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'logbook'], {
      queryParams: {
        from: this.fromDate,
        till: this.tillDate,
        criteria: this.criteria,
        cronExpression: this.cronExpression
      }
    });
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

  onUpdateOperationNote(operation: OperationResponse): void {
    const current = this.operations.find(o => o.id === operation.id);
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

        const updatedOperation = result.updatedOperations?.[0] ?? operation;
        this.operations = this.operations.map(item =>
          item.id === updatedOperation.id ? updatedOperation : item
        );
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

