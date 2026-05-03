import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TuiButton, TuiLoader, TuiNotification, TuiTitle } from '@taiga-ui/core';
import { VarianceReportResponse, VarianceResponse } from '../budget/models';
import { BudgetPlanApiService } from './budget-plan-api.service';

@Component({
  selector: 'app-budget-plan-variance',
  standalone: true,
  imports: [CommonModule, RouterLink, TuiButton, TuiLoader, TuiNotification, TuiTitle],
  template: `
    <section class="variance-container">
      <tui-loader [overlay]="true" [showLoader]="isLoading">
        <div class="variance-card">
          <header class="header">
            <div>
              <a tuiButton appearance="flat" size="s" [routerLink]="['/budget', budgetId, 'plans']">Back to plans</a>
              <h2 tuiTitle size="l">Plan variance</h2>
            </div>
          </header>

          @if (error) {
            <tui-notification appearance="error" size="m">{{ error }}</tui-notification>
          }

          <ng-container *ngIf="report">
            <div class="report-header">
              <h3 tuiTitle size="m">{{ report.plan.name }}</h3>
              <p>{{ report.plan.from | date }} - {{ report.plan.till | date }} · {{ report.plan.currencyCode }}</p>
            </div>

            <div class="variance-table">
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
            </div>
          </ng-container>
        </div>
      </tui-loader>

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
    .variance-container { padding: 1rem; }
    .variance-card {
      background: var(--tui-background-base);
      border: 1px solid var(--tui-border-normal);
      border-radius: 1rem;
      padding: 1rem;
    }
    .header {
      display: flex;
      justify-content: space-between;
      margin-bottom: 1rem;
    }
    .report-header {
      margin: 1rem 0;
    }
    .report-header p {
      color: var(--tui-text-secondary);
      margin: .25rem 0 0;
    }
    .variance-table {
      overflow-x: auto;
    }
    table {
      border-collapse: collapse;
      min-width: 720px;
      width: 100%;
    }
    th,
    td {
      border-bottom: 1px solid var(--tui-border-normal);
      padding: .75rem .5rem;
      text-align: left;
    }
    th {
      color: var(--tui-text-secondary);
      font-weight: 600;
    }
    .actual-less { color: var(--tui-text-negative); }
    .actual-greater { color: var(--tui-text-positive); }
    .planned-only {
      color: var(--tui-text-secondary);
      font-style: italic;
    }
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
