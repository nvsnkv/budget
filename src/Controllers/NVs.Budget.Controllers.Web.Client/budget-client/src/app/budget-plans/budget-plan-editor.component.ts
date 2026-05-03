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
import { CriteriaRangeMatrixComponent } from '../shared/criteria-range-matrix/criteria-range-matrix.component';
import { CriteriaMatrixCategoryRow, CriteriaMatrixRangeColumn } from '../shared/criteria-range-matrix/criteria-range-matrix.models';

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
    TuiTitle,
    CriteriaRangeMatrixComponent
  ],
  templateUrl: './budget-plan-editor.component.html',
  styleUrls: ['./budget-plan-editor.component.less']
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

  get planMatrixColumns(): CriteriaMatrixRangeColumn[] {
    const spanByKey = new Map<string, { from: string; till: string }>();
    for (const e of this.expectations) {
      const key = this.periodSpanKey(e);
      if (!spanByKey.has(key)) {
        spanByKey.set(key, { from: e.from, till: e.till });
      }
    }
    return [...spanByKey.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([name, span]) => ({
        name,
        from: span.from,
        till: span.till,
        headerLabel: this.formatPlanPeriodLabel(span.from, span.till)
      }));
  }

  get planMatrixRows(): CriteriaMatrixCategoryRow[] {
    const seen = new Map<string, { level: number; label: string }>();
    for (const e of this.expectations) {
      const path = this.expectationRowPath(e);
      if (!seen.has(path)) {
        seen.set(path, {
          level: e.hierarchyLevel ?? 0,
          label: e.subcriterionName ?? 'Root'
        });
      }
    }
    return [...seen.entries()]
      .sort((a, b) => {
        const byLevel = a[1].level - b[1].level;
        return byLevel !== 0 ? byLevel : a[1].label.localeCompare(b[1].label, undefined, { sensitivity: 'base' });
      })
      .map(([path, meta]) => ({
        path,
        label: meta.label,
        level: meta.level,
        hasChildren: false
      }));
  }

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
          this.selectedCriteriaDescription = this.availableCriteria[0]
            ? this.criteriaOptionValue(this.availableCriteria[0])
            : '';
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

  removeExpectationsForRow(path: string): void {
    this.expectations = this.expectations.filter(e => this.expectationRowPath(e) !== path);
  }

  getPlanExpectation(rowPath: string, col: CriteriaMatrixRangeColumn): EditableExpectation | undefined {
    return this.expectations.find(
      e => this.expectationRowPath(e) === rowPath && this.periodSpanKey(e) === col.name
    );
  }

  expectationAmountControlName(rowPath: string, columnKey: string): string {
    return `plan_amt_${this.sanitizeControlToken(rowPath)}__${this.sanitizeControlToken(columnKey)}`;
  }

  expectationNoteControlName(rowPath: string, columnKey: string): string {
    return `plan_note_${this.sanitizeControlToken(rowPath)}__${this.sanitizeControlToken(columnKey)}`;
  }

  generateExpectationsMatrix(): void {
    const criteria = this.selectedCriteria;
    if (!criteria) {
      this.error = 'Select logbook criteria before generating expectations';
      return;
    }

    const ranges = this.buildExpectationRanges();
    if (!ranges.length) {
      return;
    }

    const subcriteria = this.getHierarchicalSubcriteria(criteria);
    const cells = subcriteria.length > 0 ? subcriteria : [undefined];
    const totalCells = ranges.length * cells.length;
    if (totalCells > 1000) {
      this.error = `Generated matrix is too large (${totalCells} cells). Narrow the date range or cron expression.`;
      return;
    }

    const existing = new Map(this.expectations.map(expectation => [this.expectationKey(expectation), expectation]));
    this.expectations = ranges.flatMap(range =>
      cells.map(cell => {
        const subcriterionName = cell?.name;
        const previous = existing.get(this.expectationKey({ from: range.from, till: range.till, subcriterionName }));
        return {
          id: previous?.id,
          subcriterionName,
          hierarchyLevel: cell?.level ?? previous?.hierarchyLevel,
          from: range.from,
          till: range.till,
          amount: previous?.amount ?? 0,
          note: previous?.note
        };
      })
    );
    this.error = '';
  }

  save(): void {
    const criteria = this.selectedCriteria;
    if (!criteria) {
      this.error = 'Select logbook criteria before saving';
      return;
    }

    const request: UpsertBudgetPlanRequest = {
      name: this.name,
      version: this.version,
      from: this.from,
      till: this.till,
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
      hierarchyLevel: this.findSubcriteriaLevel(plan.logbookCriteria, e.subcriterionName),
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
      from: expectation.from,
      till: expectation.till,
      expectedAmount: {
        value: Number(expectation.amount),
        currencyCode: this.currencyCode
      },
      note: expectation.note || undefined
    };
  }

  private toInputDate(value: string): string {
    if (!value) {
      return '';
    }
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) {
      return '';
    }
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }

  criteriaOptionValue(criteria: LogbookCriteriaResponse): string {
    return criteria.description || 'Universal';
  }

  private get selectedCriteria(): LogbookCriteriaResponse | undefined {
    return this.availableCriteria.find(c => this.criteriaOptionValue(c) === this.selectedCriteriaDescription) ?? this.availableCriteria[0];
  }

  private buildExpectationRanges(): EditableRange[] {
    const from = new Date(this.from);
    const till = new Date(this.till);
    if (Number.isNaN(from.getTime()) || Number.isNaN(till.getTime()) || till < from) {
      this.error = 'Enter a valid plan date range before generating expectations';
      return [];
    }

    if (!this.cronExpression.trim()) {
      return [{ from: this.from, till: this.till }];
    }

    const schedule = this.parseCronExpression(this.cronExpression);
    if (!schedule) {
      return [];
    }

    const scanStart = new Date(from);
    scanStart.setDate(scanStart.getDate() - 1);
    scanStart.setSeconds(0, 0);
    const scanEnd = new Date(till);
    scanEnd.setDate(scanEnd.getDate() + 1);
    scanEnd.setSeconds(0, 0);

    const occurrences: Date[] = [];
    const cursor = new Date(scanStart);
    let scannedMinutes = 0;
    while (cursor <= scanEnd) {
      if (cursor > scanStart && this.matchesCron(cursor, schedule)) {
        occurrences.push(new Date(cursor));
      }

      cursor.setMinutes(cursor.getMinutes() + 1);
      scannedMinutes++;
      if (scannedMinutes > 1_100_000) {
        this.error = 'Cron expression scan is too large. Narrow the date range before generating expectations.';
        return [];
      }
    }

    if (occurrences.length < 2) {
      this.error = 'Cron expression must generate at least 2 occurrences within the date range';
      return [];
    }

    return occurrences.slice(1).map((occurrence, index) => ({
      from: this.toLocalInputDate(occurrences[index]),
      till: this.toLocalInputDate(occurrence)
    }));
  }

  private getHierarchicalSubcriteria(criteria: LogbookCriteriaResponse): HierarchicalCriteriaCell[] {
    const result: HierarchicalCriteriaCell[] = [];
    const seen = new Set<string>();
    const walk = (current: LogbookCriteriaResponse, level: number) => {
      for (const child of current.subcriteria ?? []) {
        const description = this.criteriaOptionValue(child);
        if (description && !seen.has(description)) {
          seen.add(description);
          result.push({ name: description, level });
        }
        walk(child, level + 1);
      }
    };

    walk(criteria, 0);
    return result;
  }

  private findSubcriteriaLevel(criteria: LogbookCriteriaResponse, subcriterionName?: string): number | undefined {
    if (!subcriterionName) {
      return undefined;
    }

    const stack = (criteria.subcriteria ?? []).map(child => ({ criteria: child, level: 0 }));
    while (stack.length) {
      const current = stack.shift();
      if (!current) {
        break;
      }

      if (this.criteriaOptionValue(current.criteria) === subcriterionName) {
        return current.level;
      }

      stack.unshift(...(current.criteria.subcriteria ?? []).map(child => ({ criteria: child, level: current.level + 1 })));
    }

    return undefined;
  }

  private parseCronExpression(expression: string): CronSchedule | null {
    const parts = expression.trim().split(/\s+/);
    if (parts.length !== 5) {
      this.error = 'Cron expression must contain 5 fields: minute hour day month weekday';
      return null;
    }

    try {
      return {
        minutes: this.parseCronField(parts[0], 0, 59),
        hours: this.parseCronField(parts[1], 0, 23),
        days: this.parseCronField(parts[2], 1, 31),
        months: this.parseCronField(parts[3], 1, 12),
        weekdays: this.parseCronField(parts[4], 0, 7)
      };
    } catch (error: any) {
      this.error = error?.message ?? 'Invalid cron expression';
      return null;
    }
  }

  private parseCronField(value: string, min: number, max: number): Set<number> {
    const values = new Set<number>();
    for (const part of value.split(',')) {
      const [rangePart, stepPart] = part.split('/');
      const step = stepPart ? Number(stepPart) : 1;
      if (!Number.isInteger(step) || step < 1) {
        throw new Error(`Invalid cron step "${part}"`);
      }

      const [start, end] = rangePart === '*'
        ? [min, max]
        : rangePart.includes('-')
          ? rangePart.split('-').map(Number)
          : [Number(rangePart), Number(rangePart)];

      if (!Number.isInteger(start) || !Number.isInteger(end) || start < min || end > max || start > end) {
        throw new Error(`Invalid cron field "${part}"`);
      }

      for (let current = start; current <= end; current += step) {
        values.add(current);
      }
    }

    return values;
  }

  private matchesCron(date: Date, schedule: CronSchedule): boolean {
    const weekday = date.getDay();
    return schedule.minutes.has(date.getMinutes())
      && schedule.hours.has(date.getHours())
      && schedule.days.has(date.getDate())
      && schedule.months.has(date.getMonth() + 1)
      && (schedule.weekdays.has(weekday) || (weekday === 0 && schedule.weekdays.has(7)));
  }

  private expectationKey(expectation: Pick<EditableExpectation, 'from' | 'till' | 'subcriterionName'>): string {
    return `${expectation.from}|${expectation.till}|${expectation.subcriterionName ?? ''}`;
  }

  private expectationRowPath(expectation: EditableExpectation): string {
    return expectation.subcriterionName ?? '';
  }

  private periodSpanKey(expectation: Pick<EditableExpectation, 'from' | 'till'>): string {
    return `${expectation.from}|${expectation.till}`;
  }

  private formatPlanPeriodLabel(from: string, till: string): string {
    const shorten = (s: string) => s.replace('T', ' ').slice(0, 16);
    return `${shorten(from)} → ${shorten(till)}`;
  }

  private sanitizeControlToken(raw: string): string {
    return raw.replace(/\W+/g, '_').slice(0, 96);
  }

  private toLocalInputDate(date: Date): string {
    const pad = (value: number) => value.toString().padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }

  private errorMessage(error: any, fallback: string): string {
    return error?.error?.[0]?.message ?? error?.message ?? fallback;
  }
}

interface EditableExpectation {
  id?: string;
  subcriterionName?: string;
  hierarchyLevel?: number;
  from: string;
  till: string;
  amount: number;
  note?: string;
}

interface HierarchicalCriteriaCell {
  name: string;
  level: number;
}

interface EditableRange {
  from: string;
  till: string;
}

interface CronSchedule {
  minutes: Set<number>;
  hours: Set<number>;
  days: Set<number>;
  months: Set<number>;
  weekdays: Set<number>;
}
