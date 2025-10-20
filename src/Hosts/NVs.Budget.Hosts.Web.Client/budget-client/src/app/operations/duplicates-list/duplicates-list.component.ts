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
import { TuiCardLarge } from '@taiga-ui/layout';
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
    TuiCardLarge,
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
          this.loadDuplicates('o => true');
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
          this.loadDuplicates('o => true');
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

