import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, catchError, of } from 'rxjs';
import { OperationsApiService } from '../operations-api.service';
import { OperationResponse } from '../../budget/models';
import { 
  TuiButton, 
  TuiDialogService, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel,
  TuiExpand
} from '@taiga-ui/core';
import { TuiCardLarge } from '@taiga-ui/layout';
import { TuiAccordion, TuiTextarea, TuiCheckbox } from '@taiga-ui/kit';
import { OperationsTableComponent } from '../operations-table/operations-table.component';

@Component({
  selector: 'app-operations-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TuiButton,
    TuiLoader,
    TuiTextfield,
    TuiLabel,
    TuiCardLarge,
    TuiTitle,
    TuiAccordion,
    TuiExpand,
    TuiTextarea,
    TuiCheckbox,
    OperationsTableComponent
  ],
  templateUrl: './operations-list.component.html',
  styleUrls: ['./operations-list.component.less']
})
export class OperationsListComponent implements OnInit {
  budgetId!: string;
  operations$!: Observable<OperationResponse[]>;
  operations: OperationResponse[] = [];
  isLoading = false;
  
  filterForm!: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private fb: FormBuilder,
    private dialogService: TuiDialogService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    
    this.filterForm = this.fb.group({
      criteria: [''],
      outputCurrency: [''],
      excludeTransfers: [false]
    });

    this.loadOperations();
  }

  loadOperations(): void {
    const formValue = this.filterForm.value;
    this.operations$ = this.operationsApi.getOperations(
      this.budgetId,
      formValue.criteria || undefined,
      formValue.outputCurrency || undefined,
      formValue.excludeTransfers
    ).pipe(
      catchError(error => {
        this.handleError(error, 'Failed to load operations');
        return of([]);
      })
    );
    
    // Subscribe to update the array for the table component
    this.operations$.subscribe(ops => this.operations = ops);
  }

  applyFilters(): void {
    this.loadOperations();
  }

  clearFilters(): void {
    this.filterForm.reset({
      criteria: '',
      outputCurrency: '',
      excludeTransfers: false
    });
    this.loadOperations();
  }

  onCriteriaKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && event.ctrlKey) {
      event.preventDefault();
      this.applyFilters();
    }
  }

  navigateToImport(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'import']);
  }

  navigateToBudget(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }

  navigateToDelete(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'delete']);
  }

  private handleError(error: any, defaultMessage: string): void {
    let errorMessage = defaultMessage;
    
    if (error.status === 400 && Array.isArray(error.error)) {
      const errors = error.error as any[];
      errorMessage = errors.map(e => e.message || e).join('; ');
    } else if (error.error?.message) {
      errorMessage = error.error.message;
    }
    
    this.showError(errorMessage);
  }

  private showError(message: string): void {
    this.dialogService.open(message, {
      label: 'Error',
      size: 'm',
      closeable: true,
      dismissible: true
    }).subscribe();
  }

  private showSuccess(message: string): void {
    this.dialogService.open(message, {
      label: 'Success',
      size: 's',
      closeable: true,
      dismissible: true
    }).subscribe();
  }

  // Helper method for template to access Object.keys
  getObjectKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }
}

