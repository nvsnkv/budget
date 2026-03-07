import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TuiButton, TuiLoader, TuiTitle } from '@taiga-ui/core';
import { BudgetApiService } from '../../budget/budget-api.service';
import { BudgetResponse, ImportResultResponse, UnregisteredOperationRequest } from '../../budget/models';
import { OperationsApiService } from '../operations-api.service';
import { NotificationService } from '../shared/notification.service';
import { OperationResultComponent } from '../shared/components/operation-result/operation-result.component';
import { ImportResult } from '../shared/models/result.interface';
import { OperationsTableComponent } from '../operations-table/operations-table.component';
import { AttributesEditorComponent } from '../shared/components/attributes-editor/attributes-editor.component';

interface ManualOperationRow {
  timestampLocal: string;
  amountValue: number | null;
  currencyCode: string;
  description: string;
  attributes: Record<string, any>;
}

@Component({
  selector: 'app-manual-import-operations',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TuiButton,
    TuiLoader,
    TuiTitle,
    OperationResultComponent,
    OperationsTableComponent,
    AttributesEditorComponent
  ],
  templateUrl: './manual-import-operations.component.html',
  styleUrls: ['./manual-import-operations.component.less']
})
export class ManualImportOperationsComponent implements OnInit {
  budgetId!: string;
  budget: BudgetResponse | null = null;
  isLoading = false;

  transferConfidenceLevel = '';
  manualRows: ManualOperationRow[] = [];
  importResult: ImportResult | null = null;
  showDuplicates = false;

  readonly currencyItems: string[] = ['RUB', 'USD', 'EUR'];
  readonly confidenceItems: string[] = ['Exact', 'Likely'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private budgetApi: BudgetApiService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    this.manualRows = [this.createEmptyRow()];
    this.loadBudget();
  }

  loadBudget(): void {
    this.isLoading = true;
    this.budgetApi.getBudgetById(this.budgetId).subscribe({
      next: budget => {
        this.budget = budget || null;
        this.isLoading = false;
      },
      error: error => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to load budget');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  addRow(): void {
    this.manualRows = [...this.manualRows, this.createEmptyRow()];
  }

  removeRow(index: number): void {
    if (this.manualRows.length === 1) {
      this.manualRows = [this.createEmptyRow()];
      return;
    }

    this.manualRows = this.manualRows.filter((_, i) => i !== index);
  }

  importManualOperations(): void {
    if (!this.budget) {
      this.notificationService.showError('Budget is not loaded yet').subscribe();
      return;
    }

    const buildResult = this.buildRequest();
    if (!buildResult.isValid) {
      this.notificationService.showError(buildResult.message).subscribe();
      return;
    }
    const request = buildResult.operations;

    this.isLoading = true;
    this.importResult = null;

    this.operationsApi.importManualOperations(
      this.budgetId,
      request,
      this.budget.version,
      this.transferConfidenceLevel || undefined
    ).subscribe({
      next: result => {
        this.isLoading = false;
        this.applyImportResult(result);

        if (result.errors.length === 0) {
          this.notificationService.showSuccess(`Successfully imported ${result.registeredOperations.length} operations`).subscribe();
          this.operationsApi.triggerRefresh(this.budgetId);
        } else {
          const errorMessage = result.errors.length > 5
            ? `Import completed with ${result.errors.length} errors. Check the results below.`
            : 'Import completed with errors. See details below.';
          this.notificationService.showError(errorMessage).subscribe();
        }
      },
      error: error => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to import operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  toggleDuplicates(): void {
    this.showDuplicates = !this.showDuplicates;
  }

  getDuplicatesList(): any[] {
    return this.importResult?.duplicatesList || [];
  }

  viewOperations(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations']);
  }

  private applyImportResult(result: ImportResultResponse): void {
    this.importResult = {
      registered: result.registeredOperations.length,
      duplicates: result.duplicates.length,
      errors: result.errors,
      successes: result.successes,
      duplicatesList: result.duplicates
    };
  }

  private buildRequest(): { isValid: boolean; message: string; operations: UnregisteredOperationRequest[] } {
    const rowsWithData = this.manualRows.filter(row => this.hasMeaningfulData(row));
    if (rowsWithData.length === 0) {
      return { isValid: false, message: 'Please add at least one operation before importing', operations: [] };
    }

    const operations: UnregisteredOperationRequest[] = [];
    for (let i = 0; i < rowsWithData.length; i++) {
      const row = rowsWithData[i];
      const rowNumber = i + 1;

      if (!row.timestampLocal) {
        return { isValid: false, message: `Row ${rowNumber}: timestamp is required`, operations: [] };
      }

      if (row.amountValue === null || Number.isNaN(row.amountValue)) {
        return { isValid: false, message: `Row ${rowNumber}: amount is required`, operations: [] };
      }

      if (!row.currencyCode) {
        return { isValid: false, message: `Row ${rowNumber}: currency is required`, operations: [] };
      }

      if (!row.description.trim()) {
        return { isValid: false, message: `Row ${rowNumber}: description is required`, operations: [] };
      }

      operations.push({
        timestamp: new Date(row.timestampLocal).toISOString(),
        amount: {
          value: row.amountValue,
          currencyCode: row.currencyCode
        },
        description: row.description.trim(),
        attributes: Object.keys(row.attributes).length > 0 ? row.attributes : undefined
      });
    }

    return { isValid: true, message: '', operations };
  }

  private hasMeaningfulData(row: ManualOperationRow): boolean {
    return Boolean(
      row.timestampLocal ||
      row.description.trim() ||
      Object.keys(row.attributes).length > 0 ||
      row.amountValue !== null
    );
  }

  private createEmptyRow(): ManualOperationRow {
    const now = new Date();
    now.setSeconds(0, 0);

    return {
      timestampLocal: this.toDateTimeLocalValue(now),
      amountValue: null,
      currencyCode: 'RUB',
      description: '',
      attributes: {}
    };
  }

  private toDateTimeLocalValue(date: Date): string {
    const tzOffsetMinutes = date.getTimezoneOffset();
    const localDate = new Date(date.getTime() - tzOffsetMinutes * 60_000);
    return localDate.toISOString().slice(0, 16);
  }
}
