import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable, map, switchMap, catchError, of } from 'rxjs';
import { BudgetApiService } from '../budget-api.service';
import { BudgetResponse } from '../models';
import { TuiButton, TuiDialogService } from '@taiga-ui/core';

@Component({
  selector: 'app-budget-settings',
  imports: [AsyncPipe, TuiButton],
  templateUrl: './budget-settings.component.html',
  styleUrls: ['./budget-settings.component.less']
})

export class BudgetSettingsComponent implements OnInit {
  budgetId$?: Observable<string>;
  budget$?: Observable<BudgetResponse | null | undefined>;
  budgetId: string | null = null;
  budget: BudgetResponse | null = null;

  constructor(
    private route: ActivatedRoute, 
    private apiService: BudgetApiService,
    private dialogService: TuiDialogService
  ) {
    this.baseUrl = apiService.baseUrl;
  }

  public readonly baseUrl: string;

  ngOnInit(): void {
    this.budgetId$ = this.route.params.pipe(map(params => params['budgetId']));
    
    // Fetch budget details when budgetId changes
    this.budget$ = this.budgetId$.pipe(
      switchMap(id => {
        this.budgetId = id;
        return this.apiService.getBudgetById(id).pipe(
          catchError(error => {
            console.error('Error fetching budget:', error);
            this.dialogService.open('Failed to load budget details', {
              label: 'Error',
              size: 'm',
              closeable: true,
              dismissible: true,
            }).subscribe();
            return of(null);
          })
        );
      })
    );

    // Subscribe to budget$ to store the budget data
    this.budget$?.subscribe(budget => {
      this.budget = budget || null;
    });
  }

  downloadBudget() {
    if (this.budgetId) {
      this.apiService.downloadBudgetYaml(this.budgetId).subscribe(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `budget-${this.budgetId}.yaml`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      });
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.apiService.uploadBudgetYaml(file).subscribe({
        next: () => {
          this.dialogService.open('Budget uploaded successfully', {
            label: 'Success',
            size: 'm',
            closeable: true,
            dismissible: true,
          }).subscribe(() => {
            window.location.reload();
          });
        },
        error: (error) => {
          let errorMessage = 'Произошла ошибка при загрузке файла.';
          if (error.status === 400 && error.error instanceof Array) {
            errorMessage = error.error.map((err: any) => err.message).join(', ');
          }
          this.dialogService.open(errorMessage, {
            label: 'Ошибка загрузки',
            size: 'm',
            closeable: true,
            dismissible: true,
          }).subscribe();
        }
      });
    }
  }

  downloadCsvOptions() {
    if (this.budgetId) {
      this.apiService.downloadCsvOptionsYaml(this.budgetId).subscribe(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `csv-options-${this.budgetId}.yaml`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      });
    }
  }

  onCsvOptionsFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0 && this.budgetId) {
      const file = input.files[0];
      this.apiService.uploadCsvOptionsYaml(this.budgetId, file).subscribe({
        next: () => {
          this.dialogService.open('CSV options updated successfully', {
            label: 'Success',
            size: 'm',
            closeable: true,
            dismissible: true,
          }).subscribe(() => {
            // Refresh the page when user clicks "Ok"
            window.location.reload();
          });
        },
        error: (error) => {
          let errorMessage = 'Error uploading CSV options.';
          if (error.status === 400 && error.error instanceof Array) {
            errorMessage = error.error.map((err: any) => err.message).join(', ');
          }
          this.dialogService.open(errorMessage, {
            label: 'Upload Error',
            size: 'm',
            closeable: true,
            dismissible: true,
          }).subscribe();
        }
      });
    }
  }
} 