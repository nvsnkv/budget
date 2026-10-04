import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TuiButton, TuiLoader, TuiTitle } from '@taiga-ui/core';
import { BudgetApiService } from '../../budget/budget-api.service';
import { BudgetResponse, ImportResultResponse, UnregisteredOperationRequest } from '../../budget/models';
import { OperationsApiService } from '../operations-api.service';
import { NotificationService } from '../shared/notification.service';
import { ImportResultViewComponent } from '../shared/components/import-result-view/import-result-view.component';
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
    FormsModule,
    TuiButton,
    TuiLoader,
    TuiTitle,
    ImportResultViewComponent,
    AttributesEditorComponent
  ],
  templateUrl: './manual-import-operations.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrls: ['./manual-import-operations.component.less']
})
export class ManualImportOperationsComponent {
  readonly budgetId: string;
  budget = signal<BudgetResponse | null>(null);
  isLoading = signal(false);

  transferConfidenceLevel = '';
  manualRows: ManualOperationRow[] = [];
  importResult = signal<ImportResultResponse | null>(null);

  readonly currencyItems: string[] = ['RUB', 'USD', 'EUR'];
  readonly confidenceItems: string[] = ['Exact', 'Likely'];

  constructor(
    private router: Router,
    private operationsApi: OperationsApiService,
    private budgetApi: BudgetApiService,
    private notificationService: NotificationService,
    route: ActivatedRoute
  ) {
    this.budgetId = route.snapshot.params['budgetId'];
    this.manualRows = [this.createEmptyRow()];
    this.loadBudget();
  }

  loadBudget(): void {
    this.isLoading.set(true);
    this.budgetApi.getBudgetById(this.budgetId).subscribe({
      next: budget => {
        this.budget.set(budget || null);
        this.isLoading.set(false);
      },
      error: error => {
        this.isLoading.set(false);
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
    if (!this.budget()) {
      this.notificationService.showError('Budget is not loaded yet').subscribe();
      return;
    }

    const buildResult = this.buildRequest();
    if (!buildResult.isValid) {
      this.notificationService.showError(buildResult.message).subscribe();
      return;
    }
    const request = buildResult.operations;

    this.isLoading.set(true);
    this.importResult.set(null);

    this.operationsApi.importManualOperations(
      this.budgetId,
      request,
      this.budget()!.version,
      this.transferConfidenceLevel || undefined
    ).subscribe({
      next: result => {
        this.isLoading.set(false);
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
        this.isLoading.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to import operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  viewOperations(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations']);
  }

  private applyImportResult(result: ImportResultResponse): void {
    this.importResult.set(result);
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
        timestamp: row.timestampLocal,
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
      timestampLocal: this.formatDateTimeLocal(now),
      amountValue: null,
      currencyCode: 'RUB',
      description: '',
      attributes: {}
    };
  }

  private formatDateTimeLocal(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }
}
