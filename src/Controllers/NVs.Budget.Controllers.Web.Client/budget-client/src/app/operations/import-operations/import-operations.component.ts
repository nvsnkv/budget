import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OperationsApiService } from '../operations-api.service';
import { BudgetApiService } from '../../budget/budget-api.service';
import { BudgetResponse, ImportResultResponse } from '../../budget/models';
import { TuiButton, TuiLoader, TuiTitle, TuiLabel, TuiInput } from '@taiga-ui/core';
import { TuiChevron, TuiDataListWrapper, TuiSelect } from '@taiga-ui/kit';
import { NotificationService } from '../shared/notification.service';
import { ImportResultViewComponent } from '../shared/components/import-result-view/import-result-view.component';

@Component({
  selector: 'app-import-operations',
  standalone: true,
  imports: [    ReactiveFormsModule,
    TuiButton,
    TuiLoader,
    TuiInput,
    TuiLabel,
    TuiTitle,
    TuiChevron,
    TuiDataListWrapper,
    TuiSelect,
    ImportResultViewComponent
  ],
  templateUrl: './import-operations.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrls: ['./import-operations.component.less']
})
export class ImportOperationsComponent {
  readonly budgetId: string;
  budget = signal<BudgetResponse | null>(null);
  isLoading = signal(false);

  selectedFile = signal<File | null>(null);
  importResult = signal<ImportResultResponse | null>(null);
  readonly confidenceItems: string[] = ['Exact', 'Likely'];

  private readonly fb = inject(FormBuilder);

  readonly importForm = this.fb.group({
    transferConfidenceLevel: ['Exact'],
    filePattern: ['']
  });

  constructor(
    private router: Router,
    private operationsApi: OperationsApiService,
    private budgetApi: BudgetApiService,
    private notificationService: NotificationService,
    route: ActivatedRoute
  ) {
    this.budgetId = route.snapshot.params['budgetId'];
    this.loadBudget();
  }

  loadBudget(): void {
    this.isLoading.set(true);
    this.budgetApi.getBudgetById(this.budgetId).subscribe({
      next: (budget) => {
        this.budget.set(budget || null);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.isLoading.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to load budget');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      this.selectedFile.set(null);
      return;
    }

    this.selectedFile.set(input.files[0]);
  }

  importCsv(): void {
    const budget = this.budget();
    const file = this.selectedFile();
    if (!file || !budget) {
      this.notificationService.showError('Please select a CSV file first').subscribe();
      return;
    }

    this.isLoading.set(true);
    this.importResult.set(null);

    const transferConfidenceLevel = this.importForm.value.transferConfidenceLevel || undefined;
    const filePattern = this.importForm.value.filePattern || undefined;

    this.operationsApi.importOperations(
      this.budgetId,
      file,
      budget.version,
      transferConfidenceLevel,
      filePattern
    ).subscribe({
      next: (result) => {
        this.isLoading.set(false);
        this.importResult.set(result);

        if (result.errors.length === 0) {
          this.notificationService.showSuccess(`Successfully imported ${result.registeredOperations.length} operations`).subscribe();
          this.operationsApi.triggerRefresh(this.budgetId);
        } else {
          const errorMessage = result.errors.length > 5 
            ? `Import completed with ${result.errors.length} errors. Check the results below.`
            : `Import completed with errors. See details below.`;
          this.notificationService.showError(errorMessage).subscribe();
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        const errorMessage = this.notificationService.handleError(error, 'Failed to import operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  viewOperations(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }
}

