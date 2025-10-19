import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OperationsApiService } from '../operations-api.service';
import { OperationResponse } from '../../budget/models';
import { 
  TuiButton, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel
} from '@taiga-ui/core';
import { TuiCardLarge } from '@taiga-ui/layout';
import { TuiTextarea } from '@taiga-ui/kit';
import { OperationsTableComponent } from '../operations-table/operations-table.component';

@Component({
  selector: 'app-duplicates-list',
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
    TuiTextarea,
    OperationsTableComponent
  ],
  templateUrl: './duplicates-list.component.html',
  styleUrls: ['./duplicates-list.component.less']
})
export class DuplicatesListComponent implements OnInit {
  budgetId!: string;
  duplicateGroups: OperationResponse[][] = [];
  isLoading = false;
  
  filterForm!: FormGroup;
  showExamples = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    
    this.filterForm = this.fb.group({
      criteria: ['o => true']
    });

    this.loadDuplicates();
  }

  loadDuplicates(): void {
    this.isLoading = true;
    const criteria = this.filterForm.value.criteria || undefined;
    
    this.operationsApi.getDuplicates(this.budgetId, criteria).subscribe({
      next: (groups) => {
        this.duplicateGroups = groups;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load duplicates', error);
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    this.loadDuplicates();
  }

  clearFilters(): void {
    this.filterForm.patchValue({ criteria: 'o => true' });
    this.loadDuplicates();
  }

  toggleExamples(): void {
    this.showExamples = !this.showExamples;
  }

  onCriteriaKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && event.ctrlKey) {
      event.preventDefault();
      this.applyFilters();
    }
  }

  navigateToOperations(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }

  navigateToBudget(): void {
    this.router.navigate(['/budget', this.budgetId, 'details']);
  }

  getTotalDuplicates(): number {
    return this.duplicateGroups.reduce((total, group) => total + group.length, 0);
  }
}

