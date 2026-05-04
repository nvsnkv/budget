import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TuiButton, TuiLoader, TuiNotification, TuiTitle } from '@taiga-ui/core';
import { BudgetPlanResponse } from '../budget/models';
import { BudgetPlanApiService } from './budget-plan-api.service';

@Component({
  selector: 'app-budget-plans-list',
  standalone: true,
  imports: [CommonModule, RouterLink, TuiButton, TuiLoader, TuiNotification, TuiTitle],
  template: `
    <section class="plans-container">
      <tui-loader [overlay]="true" [showLoader]="isLoading">
        <div class="plans-card">
          <header class="header">
            <h2 tuiTitle size="l">Budget plans</h2>
            <a
              tuiButton
              appearance="primary"
              size="m"
              [routerLink]="['/budget', budgetId, 'plans', 'new']">
              Create plan
            </a>
          </header>

          @if (error) {
            <tui-notification appearance="error" size="m">{{ error }}</tui-notification>
          }

          @if (!isLoading && plans.length === 0) {
            <div class="empty-state">
              <h3 tuiTitle size="m">No plans yet</h3>
              <p>Create a budget plan to compare expected and actual logbook values.</p>
              <a
                tuiButton
                appearance="primary"
                size="m"
                [routerLink]="['/budget', budgetId, 'plans', 'new']">
                Create first plan
              </a>
            </div>
          }

          <div class="plans-grid">
            <article *ngFor="let plan of plans" class="plan-card">
              <div>
                <h3 tuiTitle size="m">{{ plan.name }}</h3>
                <p class="plan-meta">{{ plan.from | date }} - {{ plan.till | date }} · {{ plan.currencyCode }}</p>
              </div>
              <nav class="plan-actions">
                <a tuiButton appearance="secondary" size="s" [routerLink]="['/budget', budgetId, 'plans', plan.id]">Edit</a>
                <a tuiButton appearance="secondary" size="s" [routerLink]="['/budget', budgetId, 'plans', plan.id, 'variance']">Variance</a>
                <button tuiButton type="button" appearance="flat" size="s" (click)="copy(plan)">Copy</button>
                <button tuiButton type="button" appearance="destructive" size="s" (click)="remove(plan)">Delete</button>
              </nav>
            </article>
          </div>
        </div>
      </tui-loader>
    </section>
  `,
  styles: [`
    .plans-container { padding: 1rem; }
    .plans-card {
      background: var(--tui-background-base);
      border: 1px solid var(--tui-border-normal);
      border-radius: 1rem;
      padding: 1rem;
    }
    .header,
    .plan-card,
    .plan-actions {
      display: flex;
      align-items: center;
      gap: .75rem;
    }
    .header,
    .plan-card {
      justify-content: space-between;
    }
    .plans-grid {
      display: grid;
      gap: 1rem;
      margin-top: 1rem;
    }
    .plan-card {
      border: 1px solid var(--tui-border-normal);
      border-radius: .75rem;
      padding: 1rem;
    }
    .plan-meta {
      color: var(--tui-text-secondary);
      margin: .25rem 0 0;
    }
    .empty-state {
      display: grid;
      gap: .75rem;
      justify-items: start;
      padding: 2rem 0;
    }
  `]
})
export class BudgetPlansListComponent implements OnInit {
  budgetId = '';
  plans: BudgetPlanResponse[] = [];
  isLoading = false;
  error = '';

  constructor(private route: ActivatedRoute, private api: BudgetPlanApiService) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    this.load();
  }

  copy(plan: BudgetPlanResponse): void {
    this.api.copyPlan(this.budgetId, plan.id, { name: `${plan.name} copy` }).subscribe({
      next: () => this.load(),
      error: error => this.error = this.errorMessage(error, 'Failed to copy plan')
    });
  }

  remove(plan: BudgetPlanResponse): void {
    this.api.deletePlan(this.budgetId, plan.id).subscribe({
      next: () => this.load(),
      error: error => this.error = this.errorMessage(error, 'Failed to delete plan')
    });
  }

  private load(): void {
    this.isLoading = true;
    this.error = '';
    this.api.listPlans(this.budgetId).subscribe({
      next: plans => {
        this.plans = plans;
        this.isLoading = false;
      },
      error: error => {
        this.error = this.errorMessage(error, 'Failed to load plans');
        this.isLoading = false;
      }
    });
  }

  private errorMessage(error: any, fallback: string): string {
    return error?.error?.[0]?.message ?? error?.message ?? fallback;
  }
}
