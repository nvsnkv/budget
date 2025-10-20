import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, catchError, of } from 'rxjs';
import { OperationsApiService } from '../operations-api.service';
import { OperationResponse } from '../../budget/models';
import { 
  TuiButton, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel,
  TuiExpand
} from '@taiga-ui/core';
import { TuiCardLarge } from '@taiga-ui/layout';
import { TuiAccordion, TuiTextarea, TuiCheckbox } from '@taiga-ui/kit';
import { OperationsTableComponent } from '../operations-table/operations-table.component';
import { NotificationService } from '../shared/notification.service';
import { OperationsHelperService } from '../shared/operations-helper.service';
import { CtrlEnterDirective } from '../shared/directives/ctrl-enter.directive';

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
    OperationsTableComponent,
    CtrlEnterDirective
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
    private notificationService: NotificationService,
    private operationsHelper: OperationsHelperService
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
        const errorMessage = this.notificationService.handleError(error, 'Failed to load operations');
        this.notificationService.showError(errorMessage).subscribe();
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

  navigateToImport(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'import']);
  }

  navigateToBudget(): void {
    this.router.navigate(['/budget', this.budgetId, 'details']);
  }

  navigateToDelete(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'delete']);
  }

  navigateToDuplicates(): void {
    this.router.navigate(['/budget', this.budgetId, 'operations', 'duplicates']);
  }

  onDeleteOperation(operation: OperationResponse): void {
    const confirmMessage = `Are you sure you want to delete this operation?\n\n${operation.description}\n${operation.amount.value} ${operation.amount.currencyCode}\n\nThis action cannot be undone.`;
    
    if (!confirm(confirmMessage)) {
      return;
    }

    this.isLoading = true;
    
    this.operationsHelper.deleteOperation(this.budgetId, operation.id).subscribe({
      next: (result) => {
        this.isLoading = false;
        
        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map((e: any) => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to delete operation: ${errorMessage}`).subscribe();
        } else {
          this.notificationService.showSuccess('Operation deleted successfully').subscribe();
          this.loadOperations();
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to delete operation');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  onUpdateOperation(operation: OperationResponse): void {
    this.isLoading = true;
    
    this.operationsHelper.updateOperation(this.budgetId, operation).subscribe({
      next: (result) => {
        this.isLoading = false;
        
        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map(e => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to update operation: ${errorMessage}`).subscribe();
        } else {
          this.notificationService.showSuccess('Operation updated successfully').subscribe();
          this.loadOperations();
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to update operation');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }
}

