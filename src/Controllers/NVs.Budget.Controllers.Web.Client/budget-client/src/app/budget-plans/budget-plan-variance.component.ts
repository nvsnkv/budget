import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TuiButton, TuiLoader, TuiNotification, TuiTitle } from '@taiga-ui/core';
import { VarianceReportResponse, VarianceResponse } from '../budget/models';
import { BudgetPlanApiService } from './budget-plan-api.service';
import { browserIanaTimeZoneId } from '../shared/browser-timezone';
import {
  buildVarianceMatrixModel,
  collectExpandablePaths,
  flattenVarianceRows,
  getVarianceCell,
  VarianceRowNode
} from './variance-matrix.utils';
import { CriteriaRangeMatrixComponent } from '../shared/criteria-range-matrix/criteria-range-matrix.component';
import {
  CriteriaMatrixCategoryRow,
  CriteriaMatrixRangeColumn
} from '../shared/criteria-range-matrix/criteria-range-matrix.models';

@Component({
  selector: 'app-budget-plan-variance',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    TuiButton,
    TuiLoader,
    TuiNotification,
    TuiTitle,
    CriteriaRangeMatrixComponent
  ],
  templateUrl: './budget-plan-variance.component.html',
  styleUrls: ['./budget-plan-variance.component.less']
})
export class BudgetPlanVarianceComponent implements OnInit {
  budgetId = '';
  planId = '';
  report: VarianceReportResponse | null = null;
  rangeColumns: CriteriaMatrixRangeColumn[] = [];
  matrixRows: CriteriaMatrixCategoryRow[] = [];
  varianceByCell = new Map<string, VarianceResponse>();
  private varianceRowTree: VarianceRowNode | null = null;
  /** Paths of rows that have children and are expanded (same behaviour as logbook). */
  expandedRows = new Set<string>();
  isLoading = false;
  error = '';

  constructor(private route: ActivatedRoute, private api: BudgetPlanApiService) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    this.planId = this.route.snapshot.params['planId'];
    this.isLoading = true;
    this.api.getVariance(this.budgetId, this.planId, browserIanaTimeZoneId()).subscribe({
      next: report => {
        this.report = report;
        const model = buildVarianceMatrixModel(report);
        this.rangeColumns = model.rangeColumns;
        this.varianceByCell = model.varianceByPathAndRange;
        this.varianceRowTree = model.varianceRowTree;
        this.expandedRows = new Set(model.expandablePaths);
        this.rebuildMatrixRows();
        this.isLoading = false;
      },
      error: error => {
        this.error = error?.error?.[0]?.message ?? error?.message ?? 'Failed to load variance';
        this.isLoading = false;
      }
    });
  }

  get isAllExpanded(): boolean {
    if (!this.varianceRowTree) {
      return false;
    }

    const expandable = collectExpandablePaths(this.varianceRowTree);
    return expandable.length > 0 && expandable.every(path => this.expandedRows.has(path));
  }

  toggleRow(path: string): void {
    if (this.expandedRows.has(path)) {
      this.expandedRows.delete(path);
    } else {
      this.expandedRows.add(path);
    }

    this.rebuildMatrixRows();
  }

  isRowExpanded(path: string): boolean {
    return this.expandedRows.has(path);
  }

  toggleExpandAll(): void {
    if (!this.varianceRowTree) {
      return;
    }

    if (this.isAllExpanded) {
      this.expandedRows.clear();
    } else {
      this.expandedRows = new Set(collectExpandablePaths(this.varianceRowTree));
    }

    this.rebuildMatrixRows();
  }

  private rebuildMatrixRows(): void {
    if (!this.varianceRowTree) {
      this.matrixRows = [];
      return;
    }

    this.matrixRows = flattenVarianceRows(this.varianceRowTree, this.expandedRows);
  }

  lookupVariance(path: string, rangeName: string): VarianceResponse | undefined {
    return getVarianceCell(this.varianceByCell, path, rangeName);
  }

  formatMoney(money: { value: number; currencyCode: string }): string {
    return `${money.value} ${money.currencyCode}`;
  }

  tooltipPlanVariance(v: VarianceResponse): string {
    return [`Planned: ${this.formatMoney(v.expected)}`, `Actual: ${this.formatMoney(v.actual)}`].join(' · ');
  }
}
