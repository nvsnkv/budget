import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OperationResponse } from '../../budget/models';
import { TuiButton, TuiExpand, TuiTextfield } from '@taiga-ui/core';
import { TuiChip } from '@taiga-ui/kit';
import { CurrencyFormatPipe } from '../shared/pipes/currency-format.pipe';
import { DateFormatPipe } from '../shared/pipes/date-format.pipe';
import { ObjectKeysPipe } from '../shared/pipes/object-keys.pipe';

interface EditableOperation {
  id: string;
  description: string;
  notes: string;
  amount: number;
  currencyCode: string;
  tags: string[];
  attributes: Record<string, string>;
}

@Component({
  selector: 'app-operations-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TuiButton,
    TuiExpand,
    TuiChip,
    TuiTextfield,
    CurrencyFormatPipe,
    DateFormatPipe,
    ObjectKeysPipe
  ],
  templateUrl: './operations-table.component.html',
  styleUrls: ['./operations-table.component.less']
})
export class OperationsTableComponent {
  private _operations: OperationResponse[] = [];
  @Input() set operations(value: OperationResponse[]) {
    this._operations = value;
    this.syncNoteDrafts(value);
  }

  get operations(): OperationResponse[] {
    return this._operations;
  }
  @Input() showActions = true;
  @Output() operationsUpdated = new EventEmitter<OperationResponse[]>();
  @Output() operationsDeleted = new EventEmitter<OperationResponse[]>();
  @Output() operationNoteUpdated = new EventEmitter<OperationResponse>();
  
  expandedOperationId: string | null = null;
  editingOperations: Record<string, EditableOperation> = {};
  pendingDeleteIds = new Set<string>();
  sortField: 'amount' | 'timestamp' | 'description' | null = null;
  sortDirection: 'asc' | 'desc' | null = null;
  noteDrafts: Record<string, string> = {};
  noteSaveStatus: Record<string, 'idle' | 'saving' | 'saved' | 'error'> = {};
  private pendingNoteSaves: Record<string, string> = {};
  noteEditingId: string | null = null;
  showChangedOnly = false;

  get displayedOperations(): OperationResponse[] {
    const baseOperations = this.showChangedOnly
      ? this.operations.filter(operation => this.isChanged(operation.id))
      : this.operations;

    if (!this.sortField || !this.sortDirection) return baseOperations;

    const directionMultiplier = this.sortDirection === 'asc' ? 1 : -1;

    return [...baseOperations].sort((left, right) => {
      switch (this.sortField) {
        case 'amount':
          return (left.amount.value - right.amount.value) * directionMultiplier;
        case 'timestamp':
          return (Date.parse(left.timestamp) - Date.parse(right.timestamp)) * directionMultiplier;
        case 'description':
          return left.description.localeCompare(right.description, undefined, { sensitivity: 'base' }) * directionMultiplier;
      }

      return 0;
    });
  }

  toggleSort(field: 'amount' | 'timestamp' | 'description'): void {
    if (this.sortField !== field) {
      this.sortField = field;
      this.sortDirection = 'desc';
      return;
    }

    if (!this.sortDirection) {
      this.sortDirection = 'desc';
      return;
    }

    this.sortDirection = this.sortDirection === 'desc' ? 'asc' : null;
    if (!this.sortDirection) {
      this.sortField = null;
    }
  }

  getSortIndicator(field: 'amount' | 'timestamp' | 'description'): string {
    if (this.sortField !== field || !this.sortDirection) return '↕';
    return this.sortDirection === 'asc' ? '↑' : '↓';
  }

  toggleOperationDetails(operationId: string): void {
    this.expandedOperationId = this.expandedOperationId === operationId ? null : operationId;
  }

  toggleDelete(operation: OperationResponse): void {
    if (this.pendingDeleteIds.has(operation.id)) {
      this.pendingDeleteIds.delete(operation.id);
      return;
    }

    this.pendingDeleteIds.add(operation.id);
    delete this.editingOperations[operation.id];
  }

  startEdit(operation: OperationResponse): void {
    this.editingOperations[operation.id] = {
      id: operation.id,
      description: operation.description,
      notes: operation.notes ?? '',
      amount: operation.amount.value,
      currencyCode: operation.amount.currencyCode,
      tags: [...operation.tags],
      attributes: { ...operation.attributes || {} }
    };
  }

  cancelEdit(operationId: string): void {
    delete this.editingOperations[operationId];
  }

  saveAllEdits(): void {
    const updatedOperations = this.operations
      .filter(operation => this.editingOperations[operation.id])
      .map(operation => this.buildUpdatedOperation(operation, this.editingOperations[operation.id]));

    if (updatedOperations.length === 0) {
      return;
    }

    this.operationsUpdated.emit(updatedOperations);
    for (const updated of updatedOperations) {
      this.noteDrafts[updated.id] = updated.notes ?? '';
    }
    this.editingOperations = {};
  }

  deleteAllMarked(): void {
    const operationsToDelete = this.operations.filter(operation => this.pendingDeleteIds.has(operation.id));
    if (operationsToDelete.length === 0) {
      return;
    }

    this.operationsDeleted.emit(operationsToDelete);
    this.pendingDeleteIds.clear();
  }

  isEditing(operationId: string): boolean {
    return Boolean(this.editingOperations[operationId]);
  }

  isDeleting(operationId: string): boolean {
    return this.pendingDeleteIds.has(operationId);
  }

  isChanged(operationId: string): boolean {
    return this.isEditing(operationId) || this.isDeleting(operationId);
  }

  get hasPendingEdits(): boolean {
    return Object.keys(this.editingOperations).length > 0;
  }

  get pendingEditsCount(): number {
    return Object.keys(this.editingOperations).length;
  }

  get hasPendingDeletes(): boolean {
    return this.pendingDeleteIds.size > 0;
  }

  get pendingDeletesCount(): number {
    return this.pendingDeleteIds.size;
  }

  toggleShowChangedOnly(): void {
    this.showChangedOnly = !this.showChangedOnly;
  }

  addTag(operationId: string): void {
    const editingOperation = this.editingOperations[operationId];
    if (editingOperation) {
      editingOperation.tags.push('');
    }
  }

  removeTag(operationId: string, index: number): void {
    const editingOperation = this.editingOperations[operationId];
    if (editingOperation) {
      editingOperation.tags.splice(index, 1);
    }
  }

  trackByIndex(index: number): number {
    return index;
  }

  addAttribute(operationId: string): void {
    const editingOperation = this.editingOperations[operationId];
    if (editingOperation) {
      const key = `key${Object.keys(editingOperation.attributes).length + 1}`;
      editingOperation.attributes[key] = '';
    }
  }

  removeAttribute(operationId: string, key: string): void {
    const editingOperation = this.editingOperations[operationId];
    if (editingOperation) {
      delete editingOperation.attributes[key];
    }
  }

  updateAttributeKey(operationId: string, oldKey: string, newKey: string): void {
    const editingOperation = this.editingOperations[operationId];
    if (editingOperation && oldKey !== newKey && newKey) {
      const value = editingOperation.attributes[oldKey];
      delete editingOperation.attributes[oldKey];
      editingOperation.attributes[newKey] = value;
    }
  }

  beginNotesEdit(operation: OperationResponse): void {
    if (!(operation.id in this.noteDrafts)) {
      this.noteDrafts[operation.id] = operation.notes ?? '';
    }
    this.noteSaveStatus[operation.id] = 'idle';
  }

  startNoteEdit(operation: OperationResponse): void {
    this.noteEditingId = operation.id;
    this.beginNotesEdit(operation);
  }

  saveNotes(operation: OperationResponse): void {
    const currentNotes = operation.notes ?? '';
    const nextNotes = (this.noteDrafts[operation.id] ?? '').trimEnd();
    if (nextNotes === currentNotes) {
      return;
    }

    this.noteDrafts[operation.id] = nextNotes;
    this.pendingNoteSaves[operation.id] = nextNotes;
    this.noteSaveStatus[operation.id] = 'saving';
    this.operationNoteUpdated.emit({
      ...operation,
      notes: nextNotes
    });
  }

  private syncNoteDrafts(operations: OperationResponse[]): void {
    const seenIds = new Set(operations.map(operation => operation.id));
    for (const operation of operations) {
      this.noteDrafts[operation.id] = operation.notes ?? '';
      if (this.pendingNoteSaves[operation.id] !== undefined) {
        if (operation.notes === this.pendingNoteSaves[operation.id]) {
          this.noteSaveStatus[operation.id] = 'saved';
          delete this.pendingNoteSaves[operation.id];
        } else {
          this.noteSaveStatus[operation.id] = 'error';
          delete this.pendingNoteSaves[operation.id];
        }
      }
    }

    for (const id of Object.keys(this.noteDrafts)) {
      if (!seenIds.has(id)) {
        delete this.noteDrafts[id];
        delete this.noteSaveStatus[id];
        delete this.pendingNoteSaves[id];
      }
    }
    for (const id of Object.keys(this.editingOperations)) {
      if (!seenIds.has(id)) {
        delete this.editingOperations[id];
      }
    }
    for (const id of Array.from(this.pendingDeleteIds)) {
      if (!seenIds.has(id)) {
        this.pendingDeleteIds.delete(id);
      }
    }
    if (this.showChangedOnly && !this.hasPendingEdits && !this.hasPendingDeletes) {
      this.showChangedOnly = false;
    }
  }

  onNoteEnter(operation: OperationResponse, event: Event): void {
    event.preventDefault();
    this.saveNotes(operation);
    (event.target as HTMLInputElement).blur();
  }

  onNoteBlur(operation: OperationResponse): void {
    this.saveNotes(operation);
    this.noteEditingId = null;
  }

  private buildUpdatedOperation(
    operation: OperationResponse,
    editingOperation: EditableOperation
  ): OperationResponse {
    return {
      ...operation,
      description: editingOperation.description,
      notes: editingOperation.notes,
      amount: {
        value: editingOperation.amount,
        currencyCode: editingOperation.currencyCode
      },
      tags: editingOperation.tags,
      attributes: editingOperation.attributes
    };
  }
}

