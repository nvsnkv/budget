import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
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
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <section class="editor">
      <a [routerLink]="['/budget', budgetId, 'plans']">Back to plans</a>
      <h1>{{ planId ? 'Edit budget plan' : 'Create budget plan' }}</h1>
      <p *ngIf="error" class="error">{{ error }}</p>

      <form (ngSubmit)="save()">
        <label>Name <input name="name" [(ngModel)]="name" required /></label>
        <label>From <input name="from" type="datetime-local" [(ngModel)]="from" required /></label>
        <label>Till <input name="till" type="datetime-local" [(ngModel)]="till" required /></label>
        <label>Cron expression <input name="cron" [(ngModel)]="cronExpression" placeholder="0 0 1 * *" /></label>
        <label>Currency <input name="currency" [(ngModel)]="currencyCode" required /></label>
        <label>Logbook criteria
          <select name="criteria" [(ngModel)]="selectedCriteriaDescription" required>
            <option *ngFor="let criteria of availableCriteria" [value]="criteria.description">{{ criteria.description || 'Universal' }}</option>
          </select>
        </label>

        <h2>Expected cells</h2>
        <table>
          <thead>
            <tr><th>Subcriterion</th><th>From</th><th>Till</th><th>Amount</th><th>Note</th><th></th></tr>
          </thead>
          <tbody>
            <tr *ngFor="let expectation of expectations; let i = index">
              <td><input name="sub{{i}}" [(ngModel)]="expectation.subcriterionName" placeholder="Root cell" /></td>
              <td><input name="from{{i}}" type="datetime-local" [(ngModel)]="expectation.from" required /></td>
              <td><input name="till{{i}}" type="datetime-local" [(ngModel)]="expectation.till" required /></td>
              <td><input name="amount{{i}}" type="number" [(ngModel)]="expectation.amount" required /></td>
              <td><input name="note{{i}}" [(ngModel)]="expectation.note" /></td>
              <td><button type="button" (click)="removeExpectation(i)">Remove</button></td>
            </tr>
          </tbody>
        </table>
        <button type="button" (click)="addExpectation()">Add cell</button>

        <footer>
          <button type="submit" [disabled]="isSaving">Save</button>
        </footer>
      </form>
    </section>
  `,
  styles: [`
    .editor { padding: 1rem; }
    form { display: grid; gap: 1rem; max-width: 1100px; }
    label { display: grid; gap: .25rem; }
    table { width: 100%; border-collapse: collapse; }
    th, td { border-bottom: 1px solid #ddd; padding: .4rem; }
    input, select { padding: .4rem; }
    footer { display: flex; gap: .75rem; }
    .error { color: #b00020; }
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
    const criteria = this.availableCriteria.find(c => c.description === this.selectedCriteriaDescription) ?? this.availableCriteria[0];
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
    this.selectedCriteriaDescription = plan.logbookCriteria.description;
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
