import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TuiButton, TuiLabel, TuiLoader, TuiNotification, TuiTextfield, TuiTitle } from '@taiga-ui/core';
import {
  BudgetPlanResponse,
  LogbookCriteriaResponse,
  UpsertBudgetPlanRequest,
  UpsertPlanExpectationRequest
} from '../budget/models';
import { BudgetApiService } from '../budget/budget-api.service';
import { BudgetPlanApiService } from './budget-plan-api.service';

@Component({
  selector: 'app-budget-plan-editor',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    TuiButton,
    TuiLabel,
    TuiLoader,
    TuiNotification,
    TuiTextfield,
    TuiTitle
  ],
  template: `
    <section class="editor-container">
      <tui-loader [overlay]="true" [showLoader]="isSaving">
        <div class="editor-card">
          <header class="header">
            <div>
              <a tuiButton appearance="flat" size="s" [routerLink]="['/budget', budgetId, 'plans']">Back to plans</a>
              <h2 tuiTitle size="l">{{ planId ? 'Edit budget plan' : 'Create budget plan' }}</h2>
            </div>
          </header>

          @if (error) {
            <tui-notification appearance="error" size="m">{{ error }}</tui-notification>
          }

          <form class="plan-form" (ngSubmit)="save()">
            <div class="form-grid">
              <tui-textfield>
                <label tuiLabel>Name</label>
                <input tuiTextfield name="name" [(ngModel)]="name" required />
              </tui-textfield>

              <tui-textfield>
                <label tuiLabel>From</label>
                <input tuiTextfield name="from" type="datetime-local" [(ngModel)]="from" required />
              </tui-textfield>

              <tui-textfield>
                <label tuiLabel>Till</label>
                <input tuiTextfield name="till" type="datetime-local" [(ngModel)]="till" required />
              </tui-textfield>

              <tui-textfield>
                <label tuiLabel>Cron expression</label>
                <input tuiTextfield name="cron" [(ngModel)]="cronExpression" placeholder="0 0 1 * *" />
              </tui-textfield>

              <tui-textfield>
                <label tuiLabel>Currency</label>
                <input tuiTextfield name="currency" [(ngModel)]="currencyCode" required />
              </tui-textfield>

              <label tuiLabel class="select-field">
                Logbook criteria
                <select
                  name="criteria"
                  [(ngModel)]="selectedCriteriaDescription"
                  required>
                  <option *ngFor="let criteria of availableCriteria" [value]="criteriaOptionValue(criteria)">
                    {{ criteriaOptionValue(criteria) }}
                  </option>
                </select>
              </label>
            </div>

            <section class="expectations-section">
              <div class="section-header">
                <h3 tuiTitle size="m">Expected cells</h3>
                <button tuiButton type="button" appearance="secondary" size="s" (click)="addExpectation()">Add cell</button>
              </div>

              <div class="expectations-table">
                <table>
                  <thead>
                    <tr><th>Subcriterion</th><th>From</th><th>Till</th><th>Amount</th><th>Note</th><th></th></tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let expectation of expectations; let i = index">
                      <td><input class="table-input" name="sub{{i}}" [(ngModel)]="expectation.subcriterionName" placeholder="Root cell" /></td>
                      <td><input class="table-input" name="from{{i}}" type="datetime-local" [(ngModel)]="expectation.from" required /></td>
                      <td><input class="table-input" name="till{{i}}" type="datetime-local" [(ngModel)]="expectation.till" required /></td>
                      <td><input class="table-input" name="amount{{i}}" type="number" [(ngModel)]="expectation.amount" required /></td>
                      <td><input class="table-input" name="note{{i}}" [(ngModel)]="expectation.note" /></td>
                      <td>
                        <button tuiButton type="button" appearance="destructive" size="s" (click)="removeExpectation(i)">Remove</button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </section>

            <footer class="form-actions">
              <button tuiButton type="submit" appearance="primary" size="m" [disabled]="isSaving">Save</button>
            </footer>
          </form>
        </div>
      </tui-loader>
    </section>
  `,
  styles: [`
    .editor-container { padding: 1rem; }
    .editor-card {
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
    .plan-form,
    .expectations-section {
      display: grid;
      gap: 1rem;
    }
    .form-grid {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr));
    }
    .section-header,
    .form-actions {
      align-items: center;
      display: flex;
      gap: .75rem;
      justify-content: space-between;
    }
    .expectations-table {
      overflow-x: auto;
    }
    table {
      border-collapse: collapse;
      min-width: 900px;
      width: 100%;
    }
    th,
    td {
      border-bottom: 1px solid var(--tui-border-normal);
      padding: .5rem;
      text-align: left;
    }
    th {
      color: var(--tui-text-secondary);
      font-weight: 600;
    }
    .table-input {
      background: var(--tui-background-base);
      border: 1px solid var(--tui-border-normal);
      border-radius: .5rem;
      box-sizing: border-box;
      padding: .5rem;
      width: 100%;
    }
    .select-field {
      display: grid;
      gap: .35rem;
    }
    .select-field select {
      background: var(--tui-background-base);
      border: 1px solid var(--tui-border-normal);
      border-radius: .75rem;
      box-sizing: border-box;
      min-height: 3rem;
      padding: .5rem .75rem;
      width: 100%;
    }
  `]
})
export class BudgetPlanEditorComponent implements OnInit {
  budgetId = '';
  planId: string | null = null;
  version: string | undefined;
  name = '';
  from = '';
  till = '';
  cronExpression = '';
  currencyCode = 'RUB';
  selectedCriteriaDescription = '';
  availableCriteria: LogbookCriteriaResponse[] = [];
  expectations: EditableExpectation[] = [];
  error = '';
  isSaving = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private budgetApi: BudgetApiService,
    private planApi: BudgetPlanApiService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    this.planId = this.route.snapshot.params['planId'] ?? null;
    this.from = this.toInputDate(new Date(new Date().getFullYear(), 0, 1).toISOString());
    this.till = this.toInputDate(new Date(new Date().getFullYear() + 1, 0, 1).toISOString());

    this.budgetApi.getBudgetById(this.budgetId).subscribe({
      next: budget => {
        this.availableCriteria = budget?.logbookCriteria ?? [];
        if (!this.planId) {
          this.selectedCriteriaDescription = this.availableCriteria[0]?.description ?? '';
        }
      },
      error: error => this.error = this.errorMessage(error, 'Failed to load budget criteria')
    });

    if (this.planId) {
      this.planApi.getPlan(this.budgetId, this.planId).subscribe({
        next: plan => this.applyPlan(plan),
        error: error => this.error = this.errorMessage(error, 'Failed to load plan')
      });
    } else {
      this.addExpectation();
    }
  }

  get availableCriteriaDescriptions(): string[] {
    return this.availableCriteria.map(criteria => this.criteriaOptionValue(criteria));
  }

  addExpectation(): void {
    this.expectations.push({
      from: this.from,
      till: this.till,
      amount: 0
    });
  }

  removeExpectation(index: number): void {
    this.expectations.splice(index, 1);
  }

  save(): void {
    const criteria = this.availableCriteria.find(c => this.criteriaOptionValue(c) === this.selectedCriteriaDescription) ?? this.availableCriteria[0];
    if (!criteria) {
      this.error = 'Select logbook criteria before saving';
      return;
    }

    const request: UpsertBudgetPlanRequest = {
      name: this.name,
      version: this.version,
      from: this.toIsoDate(this.from),
      till: this.toIsoDate(this.till),
      cronExpression: this.cronExpression || undefined,
      logbookCriteria: criteria,
      currencyCode: this.currencyCode,
      expectations: this.expectations.map(e => this.toExpectationRequest(e))
    };

    this.isSaving = true;
    const save$ = this.planId
      ? this.planApi.updatePlan(this.budgetId, this.planId, request)
      : this.planApi.createPlan(this.budgetId, request);

    save$.subscribe({
      next: () => this.router.navigate(['/budget', this.budgetId, 'plans']),
      error: error => {
        this.error = this.errorMessage(error, 'Failed to save plan');
        this.isSaving = false;
      }
    });
  }

  private applyPlan(plan: BudgetPlanResponse): void {
    this.version = plan.version;
    this.name = plan.name;
    this.from = this.toInputDate(plan.from);
    this.till = this.toInputDate(plan.till);
    this.cronExpression = plan.cronExpression ?? '';
    this.currencyCode = plan.currencyCode;
    this.selectedCriteriaDescription = this.criteriaOptionValue(plan.logbookCriteria);
    this.expectations = plan.expectations.map(e => ({
      id: e.id,
      subcriterionName: e.subcriterionName,
      from: this.toInputDate(e.from),
      till: this.toInputDate(e.till),
      amount: e.expectedAmount.value,
      note: e.note
    }));
  }

  private toExpectationRequest(expectation: EditableExpectation): UpsertPlanExpectationRequest {
    return {
      id: expectation.id,
      subcriterionName: expectation.subcriterionName || undefined,
      from: this.toIsoDate(expectation.from),
      till: this.toIsoDate(expectation.till),
      expectedAmount: {
        value: Number(expectation.amount),
        currencyCode: this.currencyCode
      },
      note: expectation.note || undefined
    };
  }

  private toInputDate(value: string): string {
    return value ? value.substring(0, 16) : '';
  }

  criteriaOptionValue(criteria: LogbookCriteriaResponse): string {
    return criteria.description || 'Universal';
  }

  private toIsoDate(value: string): string {
    return new Date(value).toISOString();
  }

  private errorMessage(error: any, fallback: string): string {
    return error?.error?.[0]?.message ?? error?.message ?? fallback;
  }
}

interface EditableExpectation {
  id?: string;
  subcriterionName?: string;
  from: string;
  till: string;
  amount: number;
  note?: string;
}
