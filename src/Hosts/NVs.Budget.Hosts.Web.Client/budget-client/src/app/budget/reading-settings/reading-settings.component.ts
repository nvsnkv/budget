import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, map } from 'rxjs';
import { BudgetApiService } from '../budget-api.service';
import { FileReadingSettingResponse, ValidationRuleResponse } from '../models';
import {
  TuiButton,
  TuiDialogService,
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel,
  TuiDataList
} from '@taiga-ui/core';
import {
  TuiAccordion
} from '@taiga-ui/kit';
import { TuiCardLarge } from '@taiga-ui/layout';

@Component({
  selector: 'app-reading-settings',
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
    TuiTitle,
    TuiDataList
  ],
  templateUrl: './reading-settings.component.html',
  styleUrls: ['./reading-settings.component.less']
})
export class ReadingSettingsComponent implements OnInit {
  budgetId$!: Observable<string>;
  budgetId: string = '';
  settings: Record<string, FileReadingSettingResponse> = {};
  
  settingsForm!: FormGroup;
  isEditMode = false;
  isLoading = false;
  editingPattern: string | null = null;
  isAddingNew = false;

  readonly dateTimeKindOptions = ['Local', 'Utc', 'Unspecified'];
  readonly validationConditionOptions = ['Equals', 'NotEquals'];
  
  // Make Object available in template
  readonly Object = Object;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: BudgetApiService,
    private fb: FormBuilder,
    private dialogService: TuiDialogService
  ) {}

  ngOnInit(): void {
    this.budgetId$ = this.route.params.pipe(map(params => params['budgetId']));
    
    this.budgetId$.subscribe(id => {
      this.budgetId = id;
      this.loadSettings();
    });
  }

  loadSettings(): void {
    this.isLoading = true;
    this.apiService.getReadingSettings(this.budgetId).subscribe({
      next: (response) => {
        this.settings = response || {};
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error, 'Failed to load reading settings');
      }
    });
  }

  getPatterns(): string[] {
    return Object.keys(this.settings);
  }

  startEdit(pattern: string): void {
    this.editingPattern = pattern;
    const setting = this.settings[pattern];
    this.settingsForm = this.createSettingForm(pattern, setting);
    this.isEditMode = true;
  }

  startAddNew(): void {
    this.isAddingNew = true;
    this.editingPattern = null;
    this.settingsForm = this.createSettingForm('', {
      culture: 'en-US',
      encoding: 'utf-8',
      dateTimeKind: 'Local',
      fields: {},
      attributes: {},
      validation: []
    });
    this.isEditMode = true;
  }

  createSettingForm(pattern: string, setting: FileReadingSettingResponse): FormGroup {
    return this.fb.group({
      pattern: [pattern, Validators.required],
      culture: [setting.culture, Validators.required],
      encoding: [setting.encoding, Validators.required],
      dateTimeKind: [setting.dateTimeKind, Validators.required],
      fields: this.fb.array(
        Object.entries(setting.fields).map(([key, value]) => 
          this.fb.group({
            key: [key, Validators.required],
            value: [value, Validators.required]
          })
        )
      ),
      attributes: this.fb.array(
        Object.entries(setting.attributes).map(([key, value]) => 
          this.fb.group({
            key: [key, Validators.required],
            value: [value, Validators.required]
          })
        )
      ),
      validation: this.fb.array(
        setting.validation.map(v => 
          this.fb.group({
            pattern: [v.pattern, Validators.required],
            condition: [v.condition, Validators.required],
            value: [v.value, Validators.required],
            errorMessage: [v.errorMessage, Validators.required]
          })
        )
      )
    });
  }

  get fields(): FormArray {
    return this.settingsForm?.get('fields') as FormArray;
  }

  get attributes(): FormArray {
    return this.settingsForm?.get('attributes') as FormArray;
  }

  get validationRules(): FormArray {
    return this.settingsForm?.get('validation') as FormArray;
  }

  addField(): void {
    this.fields.push(this.fb.group({
      key: ['', Validators.required],
      value: ['', Validators.required]
    }));
  }

  removeField(index: number): void {
    this.fields.removeAt(index);
  }

  addAttribute(): void {
    this.attributes.push(this.fb.group({
      key: ['', Validators.required],
      value: ['', Validators.required]
    }));
  }

  removeAttribute(index: number): void {
    this.attributes.removeAt(index);
  }

  addValidationRule(): void {
    this.validationRules.push(this.fb.group({
      pattern: ['', Validators.required],
      condition: ['Equals', Validators.required],
      value: ['', Validators.required],
      errorMessage: ['', Validators.required]
    }));
  }

  removeValidationRule(index: number): void {
    this.validationRules.removeAt(index);
  }

  saveSettings(): void {
    if (!this.settingsForm.valid) return;

    this.isLoading = true;
    const formValue = this.settingsForm.value;
    
    // Convert arrays to dictionaries
    const fields: Record<string, string> = {};
    formValue.fields.forEach((f: any) => {
      fields[f.key] = f.value;
    });

    const attributes: Record<string, string> = {};
    formValue.attributes.forEach((a: any) => {
      attributes[a.key] = a.value;
    });

    const validation: ValidationRuleResponse[] = formValue.validation.map((v: any) => ({
      pattern: v.pattern,
      condition: v.condition,
      value: v.value,
      errorMessage: v.errorMessage
    }));

    const newSetting: FileReadingSettingResponse = {
      culture: formValue.culture,
      encoding: formValue.encoding,
      dateTimeKind: formValue.dateTimeKind,
      fields,
      attributes,
      validation
    };

    // Create updated settings
    const updatedSettings = { ...this.settings };
    
    // If editing and pattern changed, remove old pattern
    if (this.editingPattern && this.editingPattern !== formValue.pattern) {
      delete updatedSettings[this.editingPattern];
    }
    
    updatedSettings[formValue.pattern] = newSetting;

    this.apiService.updateReadingSettings(this.budgetId, updatedSettings).subscribe({
      next: () => {
        this.isLoading = false;
        this.isEditMode = false;
        this.isAddingNew = false;
        this.editingPattern = null;
        this.showSuccess('Settings saved successfully');
        this.loadSettings();
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error, 'Failed to save settings');
      }
    });
  }

  deletePattern(pattern: string): void {
    if (!confirm(`Are you sure you want to delete the pattern "${pattern}"?`)) {
      return;
    }

    this.isLoading = true;
    const updatedSettings = { ...this.settings };
    delete updatedSettings[pattern];

    this.apiService.updateReadingSettings(this.budgetId, updatedSettings).subscribe({
      next: () => {
        this.isLoading = false;
        this.showSuccess('Pattern deleted successfully');
        this.loadSettings();
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error, 'Failed to delete pattern');
      }
    });
  }

  cancelEdit(): void {
    this.isEditMode = false;
    this.isAddingNew = false;
    this.editingPattern = null;
    this.settingsForm = null as any;
  }

  goBack(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }

  private showSuccess(message: string): void {
    this.dialogService
      .open(message, { label: 'Success', size: 's' })
      .subscribe();
  }

  private handleError(error: any, defaultMessage: string): void {
    let errorMessage = defaultMessage;
    
    if (error?.error) {
      if (Array.isArray(error.error)) {
        const errors = error.error.map((e: any) => e.message || e).join(', ');
        errorMessage = `${defaultMessage}: ${errors}`;
      } else if (typeof error.error === 'string') {
        errorMessage = `${defaultMessage}: ${error.error}`;
      } else if (error.error.message) {
        errorMessage = `${defaultMessage}: ${error.error.message}`;
      }
    }

    this.dialogService
      .open(errorMessage, { label: 'Error', size: 'm' })
      .subscribe();
  }
}

