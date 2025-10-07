import { AsyncPipe, CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, map, switchMap, catchError, of, tap } from 'rxjs';
import { BudgetApiService } from '../budget-api.service';
import { BudgetResponse, UpdateBudgetRequest, Owner } from '../models';
import { 
  TuiButton, 
  TuiDialogService, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel
} from '@taiga-ui/core';
import {
  TuiAccordion,
  TuiChip,
  TuiTextarea
} from '@taiga-ui/kit';
import { TuiCardLarge } from '@taiga-ui/layout';

@Component({
  selector: 'app-budget-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TuiButton,
    TuiLoader,
    TuiTextfield,
    TuiLabel,
    TuiCardLarge,
    TuiAccordion,
    TuiChip,
    TuiTextarea,
    TuiTitle
  ],
  templateUrl: './budget-detail.component.html',
  styleUrls: ['./budget-detail.component.less']
})
export class BudgetDetailComponent implements OnInit {
  budgetId$!: Observable<string>;
  budget$!: Observable<BudgetResponse | undefined>;
  budget: BudgetResponse | null = null;
  
  budgetForm!: FormGroup;
  isEditMode = false;
  isLoading = false;

  // LogbookCriteria type options
  readonly tagBasedCriterionTypes = ['Including', 'Excluding', 'OneOf'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: BudgetApiService,
    private fb: FormBuilder,
    private dialogService: TuiDialogService
  ) {}

  ngOnInit(): void {
    this.budgetId$ = this.route.params.pipe(map(params => params['budgetId']));
    
    this.budget$ = this.budgetId$.pipe(
      switchMap(id => this.apiService.getBudgetById(id).pipe(
        tap(budget => {
          this.budget = budget || null;
          this.initForm();
        }),
        catchError(error => {
          console.error('Error fetching budget:', error);
          this.showError('Failed to load budget details');
          return of(undefined);
        })
      ))
    );
  }

  initForm(): void {
    if (!this.budget) return;

    this.budgetForm = this.fb.group({
      name: [this.budget.name, Validators.required],
      version: [this.budget.version],
      taggingCriteria: this.fb.array(
        this.budget.taggingCriteria.map(tc => this.fb.group({
          tag: [tc.tag, Validators.required],
          condition: [tc.condition, Validators.required]
        }))
      ),
      transferCriteria: this.fb.array(
        this.budget.transferCriteria.map(tc => this.fb.group({
          accuracy: [tc.accuracy, Validators.required],
          comment: [tc.comment, Validators.required],
          criterion: [tc.criterion, Validators.required]
        }))
      ),
      logbookCriteria: this.createLogbookCriteriaGroup(this.budget.logbookCriteria)
    });
  }

  createLogbookCriteriaGroup(criteria: any): FormGroup {
    // Determine criteria type
    let criteriaType = 'universal';
    if (criteria.type && criteria.tags) {
      criteriaType = 'tag-based';
    } else if (criteria.criteria) {
      criteriaType = 'criteria-based';
    } else if (criteria.isUniversal) {
      criteriaType = 'universal';
    }

    return this.fb.group({
      criteriaType: [criteriaType],
      description: [criteria.description || '', Validators.required],
      type: [criteria.type || ''],
      tags: [criteria.tags ? criteria.tags.join(', ') : ''],
      substitution: [criteria.substitution || ''],
      criteria: [criteria.criteria || '']
    });
  }

  get taggingCriteria(): FormArray {
    return this.budgetForm?.get('taggingCriteria') as FormArray;
  }

  get transferCriteria(): FormArray {
    return this.budgetForm?.get('transferCriteria') as FormArray;
  }

  get logbookCriteria(): FormGroup {
    return this.budgetForm?.get('logbookCriteria') as FormGroup;
  }

  toggleEditMode(): void {
    this.isEditMode = !this.isEditMode;
    if (!this.isEditMode) {
      this.initForm();
    }
  }

  addTaggingCriterion(): void {
    this.taggingCriteria.push(this.fb.group({
      tag: ['', Validators.required],
      condition: ['', Validators.required]
    }));
  }

  removeTaggingCriterion(index: number): void {
    this.taggingCriteria.removeAt(index);
  }

  addTransferCriterion(): void {
    this.transferCriteria.push(this.fb.group({
      accuracy: ['Exact', Validators.required],
      comment: ['', Validators.required],
      criterion: ['', Validators.required]
    }));
  }

  removeTransferCriterion(index: number): void {
    this.transferCriteria.removeAt(index);
  }

  saveBudget(): void {
    if (!this.budgetForm.valid || !this.budget) return;

    this.isLoading = true;
    const formValue = this.budgetForm.value;
    
    const logbookCriteriaValue = formValue.logbookCriteria;
    const criteriaType = logbookCriteriaValue.criteriaType;
    
    const logbookCriteria: any = {
      description: logbookCriteriaValue.description,
      substitution: logbookCriteriaValue.substitution || undefined,
      subcriteria: undefined
    };

    if (criteriaType === 'universal') {
      logbookCriteria.isUniversal = true;
      logbookCriteria.type = undefined;
      logbookCriteria.tags = undefined;
      logbookCriteria.criteria = undefined;
    } else if (criteriaType === 'tag-based') {
      logbookCriteria.type = logbookCriteriaValue.type;
      logbookCriteria.tags = logbookCriteriaValue.tags ? 
        logbookCriteriaValue.tags.split(',').map((t: string) => t.trim()).filter((t: string) => t) : 
        undefined;
      logbookCriteria.isUniversal = undefined;
      logbookCriteria.criteria = undefined;
    } else if (criteriaType === 'criteria-based') {
      logbookCriteria.criteria = logbookCriteriaValue.criteria;
      logbookCriteria.isUniversal = undefined;
      logbookCriteria.type = undefined;
      logbookCriteria.tags = undefined;
    }
    
    const request: UpdateBudgetRequest = {
      name: formValue.name,
      version: this.budget.version,
      taggingCriteria: formValue.taggingCriteria,
      transferCriteria: formValue.transferCriteria,
      logbookCriteria: logbookCriteria
    };

    this.apiService.updateBudget(this.budget.id, request).subscribe({
      next: () => {
        this.isLoading = false;
        this.isEditMode = false;
        this.showSuccess('Budget updated successfully');
        window.location.reload();
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error, 'Failed to update budget');
      }
    });
  }

  deleteBudget(): void {
    if (!this.budget) return;

    const confirmed = confirm('Are you sure you want to delete this budget? This action cannot be undone.');
    if (confirmed && this.budget) {
        this.isLoading = true;
        this.apiService.removeBudget(this.budget.id, this.budget.version).subscribe({
          next: () => {
            this.isLoading = false;
            this.showSuccess('Budget deleted successfully');
            this.router.navigate(['/']);
          },
          error: (error) => {
            this.isLoading = false;
            this.handleError(error, 'Failed to delete budget');
          }
        });
    }
  }

  downloadYaml(): void {
    if (!this.budget) return;

    this.apiService.downloadBudgetYaml(this.budget.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `budget-${this.budget!.name}.yaml`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      },
      error: (error) => {
        this.handleError(error, 'Failed to download YAML');
      }
    });
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
}

