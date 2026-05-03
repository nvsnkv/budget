import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TuiButton, TuiLoader, TuiNotification, TuiTitle } from '@taiga-ui/core';
import { VarianceReportResponse, VarianceResponse } from '../budget/models';
import { BudgetPlanApiService } from './budget-plan-api.service';
import { buildVarianceMatrixModel, getVarianceCell } from './variance-matrix.utils';
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
        const model = buildVarianceMatrixModel(report);
        this.rangeColumns = model.rangeColumns;
        this.matrixRows = model.rows;
        this.varianceByCell = model.varianceByPathAndRange;
        this.isLoading = false;
      },
      error: error => {
        this.error = error?.error?.[0]?.message ?? error?.message ?? 'Failed to load variance';
        this.isLoading = false;
      }
    });
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
