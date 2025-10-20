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
import { CriteriaFilterComponent } from '../shared/components/criteria-filter/criteria-filter.component';
import { OperationResultComponent } from '../shared/components/operation-result/operation-result.component';
import { CriteriaExample } from '../shared/models/example.interface';
import { OperationResult } from '../shared/models/result.interface';

@Component({
  selector: 'app-delete-operations',
  standalone: true,
  imports: [
    CommonModule,
    TuiButton,
    TuiLoader,
    TuiCardLarge,
    TuiTitle,
    CriteriaFilterComponent,
    OperationResultComponent
  ],
  templateUrl: './delete-operations.component.html',
  styleUrls: ['./delete-operations.component.less']
})
export class DeleteOperationsComponent implements OnInit {
  budgetId!: string;
  isLoading = false;
  deleteResult: OperationResult | null = null;
  currentCriteria = 'o => true';
  
  criteriaExamples: CriteriaExample[] = [
    { label: 'All operations:', code: 'o => true' },
    { label: 'Negative amounts:', code: 'o => o.Amount.Amount < 0' },
    { label: 'Specific year:', code: 'o => o.Timestamp.Year == 2023' },
    { label: 'Contains text:', code: 'o => o.Description.Contains("test")' },
    { label: 'By tag:', code: 'o => o.Tags.Any(t => t.Value == "unwanted")' },
    { label: 'By attribute:', code: 'o => o.Attributes.ContainsKey("error")' },
    { label: 'Amount range:', code: 'o => o.Amount.Amount >= -100 && o.Amount.Amount <= -10' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
  }

  onCriteriaSubmitted(criteria: string): void {
    this.currentCriteria = criteria;
    this.deleteOperations(criteria);
  }

  deleteOperations(criteria: string): void {
    const confirmMessage = `Are you sure you want to delete all operations matching the criteria:\n\n${criteria}\n\nThis action cannot be undone.`;
    
    const confirmed = confirm(confirmMessage);
    if (!confirmed) return;

    this.isLoading = true;
    this.deleteResult = null;

    this.operationsApi.removeOperations(this.budgetId, { criteria }).subscribe({
      next: (result) => {
        this.isLoading = false;
        this.deleteResult = {
          errors: result.errors,
          successes: result.successes
        };
        
        if (result.errors.length === 0) {
          this.notificationService.showSuccess('Operations deleted successfully').subscribe();
          this.operationsApi.triggerRefresh(this.budgetId);
        } else {
          const errorMessage = result.errors.length > 5 
            ? `Deletion completed with ${result.errors.length} errors. Check the results below.`
            : `Deletion completed with errors. See details below.`;
          this.notificationService.showError(errorMessage).subscribe();
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to delete operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  viewOperations(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }

  resetResult(): void {
    this.deleteResult = null;
  }
}

