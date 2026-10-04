import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TuiButton, TuiDialogService, TuiLoader, TuiTitle } from '@taiga-ui/core';
import { UserService } from '../auth/user.service';
import { BudgetApiService } from '../budget/budget-api.service';
import { BudgetResponse } from '../budget/models';
import { TuiChip } from '@taiga-ui/kit';

@Component({
  selector: 'app-index',
  standalone: true,
  imports: [
    TuiButton,
    TuiChip,
    TuiLoader,
    TuiTitle
  ],
  templateUrl: './index.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './index.component.less'
})
export class IndexComponent {
  private readonly user = inject(UserService);
  private readonly budgetService = inject(BudgetApiService);
  private readonly router = inject(Router);
  private readonly dialogService = inject(TuiDialogService);

  readonly isAuthenticated = computed(() => this.user.currentUser().isAuthenticated);
  readonly budgets = this.budgetService.budgets;
  readonly isLoading = this.budgetService.budgetsLoading;

  constructor() {
    effect(() => {
      const error = this.budgetService.budgetsError();
      if (error) {
        console.error('Error loading budgets:', error);
      }
    });
  }

  createNewBudget(): void {
    this.router.navigate(['/budget/new']);
  }

  viewBudget(budgetId: string): void {
    this.router.navigate(['/budget', budgetId, 'operations']);
  }

  deleteBudget(budget: BudgetResponse, event: Event): void {
    event.stopPropagation();

    const confirmed = confirm(`Are you sure you want to delete budget "${budget.name}"?`);
    if (confirmed) {
        this.budgetService.removeBudget(budget.id, budget.version).subscribe({
          next: () => {
            this.dialogService.open('Budget deleted successfully', {
              label: 'Success',
              size: 's'
            }).subscribe();
          },
          error: (error) => {
            let errorMessage = 'Failed to delete budget';
            if (error.status === 400 && Array.isArray(error.error)) {
              errorMessage = error.error.map((err: any) => err.message).join('; ');
            }
            this.dialogService.open(errorMessage, {
              label: 'Error',
              size: 'm'
            }).subscribe();
          }
        });
    }
  }
}
