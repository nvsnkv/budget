import { AsyncPipe, CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, map, switchMap, catchError, of, tap } from 'rxjs';
import { BudgetApiService } from '../budget-api.service';
import { BudgetResponse, UpdateBudgetRequest, Owner, ChangeBudgetOwnersRequest } from '../models';
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
  ownersForm!: FormGroup;
  isEditMode = false;
  isOwnersEditMode = false;
  isLoading = false;
  availableOwners: Owner[] = [];
  selectedOwnerIds = new Set<string>();

  // Section visibility toggles
  showTaggingCriteria = true;
  showTransferCriteria = true;
  showLogbookCriteria = true;

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
          this.initOwnersForm();
        }),
        catchError(error => {
          console.error('Error fetching budget:', error);
          this.showError('Failed to load budget details');
          return of(undefined);
        })
      ))
    );

    this.loadOwners();
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

  initOwnersForm(): void {
    if (!this.budget) return;

    const initialOwnerIds = this.budget.owners.map(owner => owner.id);
    this.selectedOwnerIds = new Set(initialOwnerIds);
    this.ownersForm = this.fb.group({
      ownerIds: [initialOwnerIds, Validators.required]
    });
  }

  createLogbookCriteriaGroup(criteria: any): FormGroup {
    // Determine criteria type
    let criteriaType = 'group';
    let isUniversal = false;
    
    if (criteria.type && criteria.tags) {
      criteriaType = 'tag-based';
    } else if (criteria.criteria && (!criteria.subcriteria || criteria.subcriteria.length === 0)) {
      // It's criteria-based only if it has criteria expression but no subcriteria
      criteriaType = 'criteria-based';
    } else {
      // It's a group (has subcriteria or is universal)
      criteriaType = 'group';
      isUniversal = criteria.isUniversal || false;
    }

    const group = this.fb.group({
      criteriaType: [criteriaType],
      description: [criteria.description || '', Validators.required],
      isUniversal: [isUniversal],
      type: [criteria.type || ''],
      tags: [criteria.tags ? criteria.tags.join(', ') : ''],
      substitution: [criteria.substitution || ''],
      criteria: [criteria.criteria || ''],
      subcriteria: this.fb.array(
        criteria.subcriteria?.map((sub: any) => this.createLogbookCriteriaGroup(sub)) || []
      )
    });

    return group;
  }

  getSubcriteria(criteriaGroup: FormGroup): FormArray {
    return criteriaGroup.get('subcriteria') as FormArray;
  }

  addSubcriterion(criteriaGroup: FormGroup): void {
    const subcriteria = this.getSubcriteria(criteriaGroup);
    subcriteria.push(this.createLogbookCriteriaGroup({
      description: '',
      subcriteria: []
    }));
  }

  removeSubcriterion(criteriaGroup: FormGroup, index: number): void {
    const subcriteria = this.getSubcriteria(criteriaGroup);
    subcriteria.removeAt(index);
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
    if (this.isEditMode) {
      this.isOwnersEditMode = false;
    }
  }

  toggleOwnersEdit(): void {
    this.isOwnersEditMode = !this.isOwnersEditMode;
    if (this.isOwnersEditMode) {
      this.initOwnersForm();
    }
  }

  cancelOwnersEdit(): void {
    this.isOwnersEditMode = false;
    this.initOwnersForm();
  }

  toggleOwnerSelection(ownerId: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      this.selectedOwnerIds.add(ownerId);
    } else {
      this.selectedOwnerIds.delete(ownerId);
    }
    this.syncOwnerIdsControl();
  }

  saveOwners(): void {
    if (!this.budget) return;

    const ownerIds = Array.from(this.selectedOwnerIds);
    if (ownerIds.length === 0) {
      this.showError('Please select at least one owner.');
      return;
    }

    const request: ChangeBudgetOwnersRequest = {
      budget: {
        id: this.budget.id,
        version: this.budget.version
      },
      ownerIds
    };

    this.isLoading = true;
    this.apiService.changeBudgetOwners(request).subscribe({
      next: () => {
        this.isLoading = false;
        this.isOwnersEditMode = false;
        this.showSuccess('Budget owners updated successfully');
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error, 'Failed to update budget owners');
      }
    });
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

  toggleTaggingCriteria(): void {
    this.showTaggingCriteria = !this.showTaggingCriteria;
  }

  toggleTransferCriteria(): void {
    this.showTransferCriteria = !this.showTransferCriteria;
  }

  toggleLogbookCriteria(): void {
    this.showLogbookCriteria = !this.showLogbookCriteria;
  }

  private syncOwnerIdsControl(): void {
    if (!this.ownersForm) return;
    this.ownersForm.get('ownerIds')?.setValue(Array.from(this.selectedOwnerIds));
    this.ownersForm.get('ownerIds')?.markAsDirty();
  }

  buildLogbookCriteriaFromForm(formGroup: FormGroup): any {
    const criteriaType = formGroup.get('criteriaType')?.value;
    const description = formGroup.get('description')?.value;
    const substitution = formGroup.get('substitution')?.value;
    const subcriteriaArray = formGroup.get('subcriteria') as FormArray;
    const isUniversal = formGroup.get('isUniversal')?.value;

    const baseCriteria: any = {
      description,
      substitution: substitution || undefined
    };

    if (criteriaType === 'group') {
      // Group type - may have isUniversal flag and/or pre-filter criteria
      if (isUniversal) {
        baseCriteria.isUniversal = true;
      }
      
      // Add pre-filter criteria if specified and not universal
      const criteriaExpr = formGroup.get('criteria')?.value;
      if (criteriaExpr && !isUniversal) {
        baseCriteria.criteria = criteriaExpr;
      }
    } else if (criteriaType === 'tag-based') {
      const tags = formGroup.get('tags')?.value;
      baseCriteria.type = formGroup.get('type')?.value;
      baseCriteria.tags = tags ? tags.split(',').map((t: string) => t.trim()).filter((t: string) => t) : undefined;
    } else if (criteriaType === 'criteria-based') {
      baseCriteria.criteria = formGroup.get('criteria')?.value;
    }

    // Recursively build subcriteria
    if (subcriteriaArray && subcriteriaArray.length > 0) {
      baseCriteria.subcriteria = subcriteriaArray.controls.map(ctrl => 
        this.buildLogbookCriteriaFromForm(ctrl as FormGroup)
      );
    }

    return baseCriteria;
  }

  saveBudget(): void {
    if (!this.budgetForm.valid || !this.budget) return;

    this.isLoading = true;
    const formValue = this.budgetForm.value;
    
    const logbookCriteria = this.buildLogbookCriteriaFromForm(this.logbookCriteria);
    
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

  navigateToReadingSettings(): void {
    if (this.budget) {
      this.router.navigate(['/budget', this.budget.id, 'reading-settings']);
    }
  }

  navigateToOperations(): void {
    if (this.budget) {
      this.router.navigate(['/budget', this.budget.id, 'operations']);
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

  uploadYaml(): void {
    if (!this.budget) return;

    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.yaml,.yml';
    input.onchange = (event: any) => {
      const file = event.target?.files?.[0];
      if (!file) return;

      const reader = new FileReader();
      reader.onload = (e) => {
        const yamlContent = e.target?.result as string;
        if (!yamlContent) {
          this.showError('Failed to read file content');
          return;
        }

        this.isLoading = true;
        this.apiService.uploadBudgetYaml(this.budget!.id, yamlContent).subscribe({
          next: () => {
            this.isLoading = false;
            this.showSuccess('Budget updated successfully from YAML');
            window.location.reload();
          },
          error: (error) => {
            this.isLoading = false;
            this.handleError(error, 'Failed to upload YAML');
          }
        });
      };
      reader.onerror = () => {
        this.showError('Failed to read file');
      };
      reader.readAsText(file);
    };
    input.click();
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

  private loadOwners(): void {
    this.apiService.getOwners().subscribe({
      next: (owners) => {
        this.availableOwners = owners;
        this.initOwnersForm();
      },
      error: (error) => {
        console.error('Error fetching owners:', error);
        this.showError('Failed to load owners list');
      }
    });
  }
}

