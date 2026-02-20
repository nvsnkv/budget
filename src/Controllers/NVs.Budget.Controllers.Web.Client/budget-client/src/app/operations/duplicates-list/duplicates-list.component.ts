import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { OperationsApiService } from '../operations-api.service';
import { OperationResponse } from '../../budget/models';
import { 
  TuiButton, 
  TuiLoader,
  TuiTitle
} from '@taiga-ui/core';
import { OperationsTableComponent } from '../operations-table/operations-table.component';
import { NotificationService } from '../shared/notification.service';
import { OperationsHelperService } from '../shared/operations-helper.service';
import { CriteriaFilterComponent } from '../shared/components/criteria-filter/criteria-filter.component';
import { CriteriaExample } from '../shared/models/example.interface';

@Component({
  selector: 'app-duplicates-list',
  standalone: true,
  imports: [
    CommonModule,
    TuiButton,
    TuiLoader,
    TuiTitle,
    OperationsTableComponent,
    CriteriaFilterComponent
  ],
  templateUrl: './duplicates-list.component.html',
  styleUrls: ['./duplicates-list.component.less']
})
export class DuplicatesListComponent implements OnInit {
  budgetId!: string;
  duplicateGroups: OperationResponse[][] = [];
  isLoading = false;
  
  criteriaExamples: CriteriaExample[] = [
    { label: 'All operations:', code: 'o => true' },
    { label: 'Specific year:', code: 'o => o.Timestamp.Year == 2024' },
    { label: 'Negative amounts only:', code: 'o => o.Amount.Amount < 0' },
    { label: 'By description contains:', code: 'o => o.Description.Contains("coffee")' },
    { label: 'Recent operations:', code: 'o => o.Timestamp > DateTime.Now.AddDays(-30)' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private notificationService: NotificationService,
    private operationsHelper: OperationsHelperService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    this.loadDuplicates('o => true');
  }

  loadDuplicates(criteria: string): void {
    this.isLoading = true;
    
    this.operationsApi.getDuplicates(this.budgetId, criteria || undefined).subscribe({
      next: (groups) => {
        this.duplicateGroups = groups;
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = this.notificationService.handleError(error, 'Failed to load duplicates');
        this.notificationService.showError(errorMessage).subscribe();
        this.isLoading = false;
      }
    });
  }

  onCriteriaSubmitted(criteria: string): void {
    this.loadDuplicates(criteria);
  }

  onCriteriaCleared(): void {
    this.loadDuplicates('o => true');
  }

  clearFilters(): void {
    this.onCriteriaCleared();
  }

  navigateToOperations(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }

  navigateToBudget(): void {
    this.router.navigate(['/budget', this.budgetId, 'details']);
  }

  getTotalDuplicates(): number {
    return this.duplicateGroups.reduce((total, group) => total + group.length, 0);
  }

  onDeleteOperations(operations: OperationResponse[]): void {
    const count = operations.length;
    if (count === 0) return;

    const confirmMessage = `Are you sure you want to delete ${count} operation${count === 1 ? '' : 's'}?\n\nThis action cannot be undone.`;
    
    if (!confirm(confirmMessage)) {
      return;
    }

    this.isLoading = true;
    
    this.operationsHelper.deleteOperations(this.budgetId, operations.map(operation => operation.id)).subscribe({
      next: (result) => {
        this.isLoading = false;
        
        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map((e: any) => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to delete operations: ${errorMessage}`).subscribe();
        } else {
          this.notificationService.showSuccess(`Deleted ${count} operation${count === 1 ? '' : 's'} successfully`).subscribe();
          this.loadDuplicates('o => true');
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to delete operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onUpdateOperations(operations: OperationResponse[]): void {
    this.isLoading = true;
    
    this.operationsHelper.updateOperations(this.budgetId, operations).subscribe({
      next: (result) => {
        this.isLoading = false;
        
        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map(e => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to update operations: ${errorMessage}`).subscribe();
        } else {
          const count = result.updatedOperations?.length ?? operations.length;
          this.notificationService.showSuccess(`Updated ${count} operation${count === 1 ? '' : 's'} successfully`).subscribe();
          this.loadDuplicates('o => true');
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to update operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onUpdateOperationNote(operation: OperationResponse): void {
    const current = this.findOperation(operation.id);
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
        this.replaceOperation(updatedOperation);
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

  private replaceOperation(updated: OperationResponse): void {
    this.duplicateGroups = this.duplicateGroups.map(group =>
      group.map(item => item.id === updated.id ? updated : item)
    );
  }

  private findOperation(operationId: string): OperationResponse | undefined {
    return this.duplicateGroups.flat().find(operation => operation.id === operationId);
  }
}

