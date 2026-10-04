import { Component, ChangeDetectionStrategy, signal } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { BudgetApiService } from '../budget-api.service';
import { RegisterBudgetRequest } from '../models';

import { TuiButton, TuiError, TuiNotification, TuiInput, TuiCheckbox, tuiValidationErrorsProvider } from '@taiga-ui/core';
import { TuiForm } from '@taiga-ui/layout';
import { Router } from '@angular/router';

@Component({
  selector: 'app-new-budget',
  templateUrl: './new-budget.component.html',
  styleUrls: ['./new-budget.component.less'],
  imports: [FormsModule, ReactiveFormsModule, TuiNotification, TuiInput, TuiButton, TuiError, TuiForm, TuiCheckbox],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [tuiValidationErrorsProvider({required: 'Please enter budget name'})]
})
export class NewBudgetComponent {
  nameGroup = new FormGroup({
    name: new FormControl('', [Validators.required]),
    generateDemoBudget: new FormControl(false),
  });

  errorMessage = signal<string | null>(null);

  constructor(private budgetService: BudgetApiService, private router: Router) {}

  onSubmit() {
    if (!this.nameGroup.controls.name.valid) {
      this.errorMessage.set('Please enter budget name.');
      return;
    }

    const request: RegisterBudgetRequest = {
      name: this.nameGroup.controls.name.value ?? '',
      generateDemoBudget: this.nameGroup.controls.generateDemoBudget.value ?? false,
    };

    this.budgetService.createBudget(request).subscribe({
      next: (response) => {
        this.resetForm();
        this.router.navigate(['/budget', response.id, 'operations']);
      },
      error: (error) => {
        this.handleError(error);
      }
    });
  }

  resetForm() {
    this.nameGroup.controls.name.setValue('');
    this.nameGroup.controls.generateDemoBudget.setValue(false);
    this.errorMessage.set(null);
  }

  handleError(error: any) {
    if (error.status === 400 && Array.isArray(error.error)) {
      this.errorMessage.set(error.error.map((err: any) => err.message).join(', '));
    } else {
      this.errorMessage.set('Error creating budget. Please try again.');
    }
  }
}