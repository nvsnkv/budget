import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BudgetPlanResponse } from '../budget/models';
import { BudgetPlanApiService } from './budget-plan-api.service';

@Component({
  selector: 'app-budget-plans-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="plans">
      <header>
        <h1>Budget plans</h1>
        <a [routerLink]="['/budget', budgetId, 'plans', 'new']">Create plan</a>
      </header>

      <p *ngIf="isLoading">Loading plans...</p>
      <p *ngIf="error" class="error">{{ error }}</p>
      <p *ngIf="!isLoading && plans.length === 0">No plans yet.</p>

      <article *ngFor="let plan of plans" class="plan-card">
        <h2>{{ plan.name }}</h2>
        <p>{{ plan.from | date }} - {{ plan.till | date }} · {{ plan.currencyCode }}</p>
        <nav>
          <a [routerLink]="['/budget', budgetId, 'plans', plan.id]">Edit</a>
          <a [routerLink]="['/budget', budgetId, 'plans', plan.id, 'variance']">Variance</a>
          <button type="button" (click)="copy(plan)">Copy</button>
          <button type="button" (click)="remove(plan)">Delete</button>
        </nav>
      </article>
    </section>
  `,
  styles: [`
    .plans { padding: 1rem; }
    header { display: flex; justify-content: space-between; align-items: center; }
    .plan-card { border: 1px solid #ddd; border-radius: 8px; margin: 1rem 0; padding: 1rem; }
    nav { display: flex; gap: .75rem; align-items: center; }
    .error { color: #b00020; }
  `]
})
export class BudgetPlansListComponent implements OnInit {
  budgetId = '';
  plans: BudgetPlanResponse[] = [];
  isLoading = false;
  error = '';

  constructor(private api: BudgetPlanApiService) {}

  ngOnInit(): void {
    this.budgetId = location.pathname.split('/')[2];
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
