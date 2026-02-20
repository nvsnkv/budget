import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { OperationsApiService } from '../operations-api.service';
import { BudgetApiService } from '../../budget/budget-api.service';
import { 
  TuiButton, 
  TuiLoader,
  TuiTitle,
  TuiLabel
} from '@taiga-ui/core';
import { TuiCheckbox } from '@taiga-ui/kit';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '../shared/notification.service';
import { CriteriaFilterComponent } from '../shared/components/criteria-filter/criteria-filter.component';
import { OperationResultComponent } from '../shared/components/operation-result/operation-result.component';
import { CriteriaExample } from '../shared/models/example.interface';
import { OperationResult } from '../shared/models/result.interface';

@Component({
  selector: 'app-retag-operations',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TuiButton,
    TuiLoader,
    TuiTitle,
    TuiLabel,
    TuiCheckbox,
    CriteriaFilterComponent,
    OperationResultComponent
  ],
  templateUrl: './retag-operations.component.html',
  styleUrls: ['./retag-operations.component.less']
})
export class RetagOperationsComponent implements OnInit {
  budgetId!: string;
  budgetVersion!: string;
  isLoading = false;
  retagResult: OperationResult | null = null;
  currentCriteria = 'o => true';
  fromScratch = false;
  
  criteriaExamples: CriteriaExample[] = [
    { label: 'All operations:', code: 'o => true' },
    { label: 'Specific year:', code: 'o => o.Timestamp.Year == 2023' },
    { label: 'Contains text:', code: 'o => o.Description.Contains("groceries")' },
    { label: 'Missing tags:', code: 'o => o.Tags.Count == 0' },
    { label: 'Date range:', code: 'o => o.Timestamp >= DateTime.Parse("2023-01-01") && o.Timestamp < DateTime.Parse("2024-01-01")' },
    { label: 'By amount range:', code: 'o => o.Amount.Amount >= -1000 && o.Amount.Amount <= -100' },
    { label: 'Specific attribute:', code: 'o => o.Attributes.ContainsKey("category") && o.Attributes["category"] == "food"' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private budgetApi: BudgetApiService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    this.loadBudgetVersion();
  }

  loadBudgetVersion(): void {
    this.budgetApi.getAllBudgets().subscribe({
      next: (budgets: any) => {
        const budget = budgets.find((b: any) => b.id === this.budgetId);
        if (budget) {
          this.budgetVersion = budget.version;
        }
      },
      error: (error: any) => {
        const errorMessage = this.notificationService.handleError(error, 'Failed to load budget');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  retagOperations(criteria: string): void {
    this.currentCriteria = criteria;
    const action = this.fromScratch ? 'retag from scratch' : 'retag';
    const confirmMessage = `Are you sure you want to ${action} all operations matching the criteria:\n\n${criteria}\n\n${this.fromScratch ? 'This will remove all existing tags and apply tagging criteria from the beginning.' : 'This will apply tagging criteria to operations that match.'}`;
    
    const confirmed = confirm(confirmMessage);
    if (!confirmed) return;

    if (!this.budgetVersion) {
      this.notificationService.showError('Budget version not loaded. Please try again.').subscribe();
      return;
    }

    this.isLoading = true;
    this.retagResult = null;

    const request = {
      budgetVersion: this.budgetVersion,
      criteria: criteria,
      fromScratch: this.fromScratch
    };

    this.operationsApi.retagOperations(this.budgetId, request).subscribe({
      next: (result) => {
        this.isLoading = false;
        this.retagResult = {
          errors: result.errors,
          successes: result.successes
        };
        
        if (result.errors.length === 0) {
          this.notificationService.showSuccess('Operations retagged successfully').subscribe();
          this.operationsApi.triggerRefresh(this.budgetId);
          // Reload budget version after successful retag
          this.loadBudgetVersion();
        } else {
          const errorMessage = result.errors.length > 5 
            ? `Retagging completed with ${result.errors.length} errors. Check the results below.`
            : `Retagging completed with errors. See details below.`;
          this.notificationService.showError(errorMessage).subscribe();
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to retag operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  viewOperations(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }

  resetResult(): void {
    this.retagResult = null;
  }
}

