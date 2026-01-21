import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { TuiButton, TuiDialogService, TuiLoader, TuiTitle } from '@taiga-ui/core';
import { UserService } from '../auth/user.service';
import { BudgetApiService } from '../budget/budget-api.service';
import { BudgetResponse } from '../budget/models';
import { Observable, map, catchError, of } from 'rxjs';
import { AsyncPipe, CommonModule } from '@angular/common';
import { TuiCardLarge } from '@taiga-ui/layout';
import { TuiChip } from '@taiga-ui/kit';

@Component({
  selector: 'app-index',
  standalone: true,
  imports: [
    CommonModule,
    AsyncPipe, 
    TuiButton,
    TuiCardLarge,
    TuiChip,
    TuiLoader,
    TuiTitle
  ],
  templateUrl: './index.component.html',
  styleUrl: './index.component.less'
})
export class IndexComponent {
  isAuthenticated$: Observable<boolean>;
  budgets$: Observable<BudgetResponse[]>;

  constructor(
    private user: UserService,
    private budgetService: BudgetApiService,
    private router: Router,
    private dialogService: TuiDialogService
  ) {
    this.isAuthenticated$ = user.current$.pipe(map(u => u.isAuthenticated));
    this.budgets$ = this.budgetService.getAllBudgets().pipe(
      catchError(error => {
        console.error('Error loading budgets:', error);
        return of([]);
      })
    );
  }

  createNewBudget(): void {
    this.router.navigate(['/budget/new']);
  }

  viewBudget(budgetId: string): void {
    this.router.navigate(['/budget', budgetId]);
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
