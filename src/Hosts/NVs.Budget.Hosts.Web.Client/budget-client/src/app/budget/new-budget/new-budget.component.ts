import { Component } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { BudgetApiService } from '../budget-api.service';
import { CreateBudgetRequest } from '../models';
import { CommonModule } from '@angular/common';
import { TuiButton, TuiNotification, TuiTextfield } from '@taiga-ui/core';

@Component({
  selector: 'app-new-budget',
  templateUrl: './new-budget.component.html',
  styleUrls: ['./new-budget.component.less'],
  imports: [FormsModule, ReactiveFormsModule, CommonModule, TuiNotification, TuiTextfield, TuiButton],
})
export class NewBudgetComponent {
  nameGroup = new FormGroup({
    name: new FormControl('', [Validators.required]),
  });

  errorMessage: string | null = null;

  constructor(private budgetService: BudgetApiService) {}

  onSubmit() {
    if (!this.nameGroup.controls.name.valid) {
      this.errorMessage = 'Пожалуйста, введите название бюджета.';
      return;
    }

    const request: CreateBudgetRequest = {
      name: this.nameGroup.controls.name.value ?? '',
    };

    this.budgetService.createBudget(request).subscribe({
      next: (response) => {
        alert(`Бюджет успешно создан: ${response.id}`);
        this.resetForm();
      },
      error: (error) => {
        this.handleError(error);
      }
    });
  }

  resetForm() {
    this.nameGroup.controls.name.setValue('');
    this.errorMessage = null;
  }

  handleError(error: any) {
    if (error.status === 400 && error.error instanceof Array) {
      this.errorMessage = error.error.map((err: any) => err.message).join(', ');
    } else {
      this.errorMessage = 'Произошла ошибка при создании бюджета. Попробуйте снова позже.';
    }
  }
}