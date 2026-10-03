import { Component, OnInit, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TuiButton, TuiLoader, TuiTitle, TuiLabel, TuiDropdown, TuiInput } from '@taiga-ui/core';

import { OperationsApiService } from '../operations-api.service';
import { NotificationService } from '../shared/notification.service';
import { OperationsHelperService } from '../shared/operations-helper.service';
import { OperationResponse } from '../../budget/models';

import { CriteriaFilterComponent } from '../shared/components/criteria-filter/criteria-filter.component';
import { OperationsTableComponent } from '../operations-table/operations-table.component';
import { AttributesEditorComponent } from '../shared/components/attributes-editor/attributes-editor.component';
import { CriteriaExample } from '../shared/models/example.interface';

@Component({
  selector: 'app-bulk-changes',
  standalone: true,
  imports: [    FormsModule,
    ReactiveFormsModule,
    TuiButton,
    TuiLoader,
    TuiTitle,
    TuiLabel,
    TuiInput,
    TuiDropdown,
    CriteriaFilterComponent,
    OperationsTableComponent,
    AttributesEditorComponent
  ],
  templateUrl: './bulk-changes.component.html',
  styleUrls: ['./bulk-changes.component.less']
})
export class BulkChangesComponent implements OnInit {
  budgetId!: string;
  isLoading = false;
  operations: OperationResponse[] = [];

  currentCriteria = 'o => true';
  criteriaExamples: CriteriaExample[] = [
    { label: 'All operations:', code: 'o => true' },
    { label: 'Specific year:', code: 'o => o.Timestamp.Year == 2023' },
    { label: 'Contains text:', code: 'o => o.Description.Contains("groceries")' },
    { label: 'Specific tag:', code: 'o => o.Tags.Any(t => t.Value == "food")' }
  ];

  tagsToAdd = '';
  tagsToRemove = '';

  attributesToAdd: Record<string, any> = {};
  attributesToRemove = '';

  @ViewChild(OperationsTableComponent) operationsTable!: OperationsTableComponent;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private notificationService: NotificationService,
    private operationsHelper: OperationsHelperService
  ) {}

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
  }

  loadAndApply(criteria: string): void {
    this.currentCriteria = criteria;
    this.isLoading = true;

    this.operationsApi.getOperations(this.budgetId, criteria, undefined, false).subscribe({
      next: (ops) => {
        this.isLoading = false;
        this.operations = ops;
        
        // Wait for next tick so that OperationsTableComponent updates its operations input
        setTimeout(() => {
          this.applyBulkChangesToDrafts();
        });
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to load operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  applyBulkChangesToDrafts(): void {
    if (!this.operationsTable) return;

    const tagsToAddList = this.tagsToAdd.split(',').map(t => t.trim()).filter(t => t);
    const tagsToRemoveList = this.tagsToRemove.split(',').map(t => t.trim()).filter(t => t);
    const attrsToRemoveList = this.attributesToRemove.split(',').map(a => a.trim()).filter(a => a);

    for (const operation of this.operations) {
      // Put operation into edit mode
      this.operationsTable.startEdit(operation);
      const draft = this.operationsTable.editingOperations[operation.id];
      if (!draft) continue;

      // Apply tag changes
      if (tagsToRemoveList.length > 0) {
        draft.tags = draft.tags.filter(t => !tagsToRemoveList.includes(t));
      }
      if (tagsToAddList.length > 0) {
        for (const tag of tagsToAddList) {
          if (!draft.tags.includes(tag)) {
            draft.tags.push(tag);
          }
        }
      }

      // Apply attribute changes
      if (attrsToRemoveList.length > 0) {
        for (const attr of attrsToRemoveList) {
          delete draft.attributes[attr];
        }
      }
      if (Object.keys(this.attributesToAdd).length > 0) {
        draft.attributes = { ...draft.attributes, ...this.attributesToAdd };
      }
    }

    // Force show changed only to see the edits clearly
    this.operationsTable.showChangedOnly = true;
  }

  onUpdateOperations(updatedOperations: OperationResponse[]): void {
    this.isLoading = true;

    this.operationsHelper.updateOperations(this.budgetId, updatedOperations).subscribe({
      next: (result) => {
        this.isLoading = false;

        if (result.errors && result.errors.length > 0) {
          const errorMessage = result.errors.map(e => e.message || 'Unknown error').join('; ');
          this.notificationService.showError(`Failed to update operations: ${errorMessage}`).subscribe();
        } else {
          const count = result.updatedOperations?.length ?? updatedOperations.length;
          this.notificationService.showSuccess(`Updated ${count} operation${count === 1 ? '' : 's'} successfully`).subscribe();
          this.operations = []; // Clear list after successful save
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to update operations');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }
}

