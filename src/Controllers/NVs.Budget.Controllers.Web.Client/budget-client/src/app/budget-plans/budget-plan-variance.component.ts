import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { VarianceReportResponse, VarianceResponse } from '../budget/models';
import { BudgetPlanApiService } from './budget-plan-api.service';

@Component({
  selector: 'app-budget-plan-variance',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="variance">
      <a [routerLink]="['/budget', budgetId, 'plans']">Back to plans</a>
      <h1>Plan variance</h1>
      <p *ngIf="isLoading">Loading variance...</p>
      <p *ngIf="error" class="error">{{ error }}</p>

      <ng-container *ngIf="report">
        <h2>{{ report.plan.name }}</h2>
        <table>
          <thead>
            <tr>
              <th>Cell</th>
              <th>Range</th>
              <th>Actual</th>
              <th>Variance</th>
            </tr>
          </thead>
          <tbody>
            <ng-container *ngFor="let variance of report.variances">
              <ng-container *ngTemplateOutlet="row; context: { variance: variance, level: 0 }"></ng-container>
            </ng-container>
          </tbody>
        </table>
      </ng-container>

      <ng-template #row let-variance="variance" let-level="level">
        <tr [class.planned-only]="variance.isPlannedOnly">
          <td [style.padding-left.rem]="level * 1.25">{{ variance.description || 'Universal' }}</td>
          <td>{{ variance.range.name }}</td>
          <td
            [class.actual-less]="variance.hasPlan && variance.actualComparison < 0"
            [class.actual-greater]="variance.hasPlan && variance.actualComparison > 0"
            [title]="'Plan: ' + formatMoney(variance.expected)">
            {{ formatMoney(variance.actual) }}
          </td>
          <td>{{ formatMoney(variance.difference) }}</td>
        </tr>
        <ng-container *ngFor="let child of variance.children">
          <ng-container *ngTemplateOutlet="row; context: { variance: child, level: level + 1 }"></ng-container>
        </ng-container>
      </ng-template>
    </section>
  `,
  styles: [`
    .variance { padding: 1rem; }
    table { border-collapse: collapse; width: 100%; }
    th, td { border-bottom: 1px solid #ddd; padding: .5rem; text-align: left; }
    .actual-less { color: #b00020; }
    .actual-greater { color: #0a7f28; }
    .planned-only { font-style: italic; }
    .error { color: #b00020; }
  `]
})
export class BudgetPlanVarianceComponent implements OnInit {
  budgetId = '';
  planId = '';
  report: VarianceReportResponse | null = null;
  isLoading = false;
  error = '';

  constructor(private route: ActivatedRoute, private api: BudgetPlanApiService) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    this.planId = this.route.snapshot.params['planId'];
    this.isLoading = true;
    this.api.getVariance(this.budgetId, this.planId).subscribe({
      next: report => {
        this.report = report;
        this.isLoading = false;
      },
      error: error => {
        this.error = error?.error?.[0]?.message ?? error?.message ?? 'Failed to load variance';
        this.isLoading = false;
      }
    });
  }

  formatMoney(money: { value: number; currencyCode: string }): string {
    return `${money.value} ${money.currencyCode}`;
  }
}
