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
  @Output() operationDeleted = new EventEmitter<OperationResponse>();
  @Output() operationUpdated = new EventEmitter<OperationResponse>();
  @Output() operationNoteUpdated = new EventEmitter<OperationResponse>();
  
  expandedOperationId: string | null = null;
  editingOperationId: string | null = null;
  editingOperation: EditableOperation | null = null;
  sortField: 'amount' | 'timestamp' | 'description' | null = null;
  sortDirection: 'asc' | 'desc' | null = null;
  noteDrafts: Record<string, string> = {};
  noteSaveStatus: Record<string, 'idle' | 'saving' | 'saved' | 'error'> = {};
  private pendingNoteSaves: Record<string, string> = {};
  noteEditingId: string | null = null;

  get displayedOperations(): OperationResponse[] {
    if (!this.sortField || !this.sortDirection) return this.operations;

    const directionMultiplier = this.sortDirection === 'asc' ? 1 : -1;

    return [...this.operations].sort((left, right) => {
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

  deleteOperation(operation: OperationResponse): void {
    this.operationDeleted.emit(operation);
  }

  startEdit(operation: OperationResponse): void {
    this.editingOperationId = operation.id;
    this.editingOperation = {
      id: operation.id,
      description: operation.description,
      notes: operation.notes ?? '',
      amount: operation.amount.value,
      currencyCode: operation.amount.currencyCode,
      tags: [...operation.tags],
      attributes: { ...operation.attributes || {} }
    };
  }

  cancelEdit(): void {
    this.editingOperationId = null;
    this.editingOperation = null;
  }

  saveEdit(operation: OperationResponse): void {
    if (!this.editingOperation) return;

    const updatedOperation: OperationResponse = {
      ...operation,
      description: this.editingOperation.description,
      notes: this.editingOperation.notes,
      amount: {
        value: this.editingOperation.amount,
        currencyCode: this.editingOperation.currencyCode
      },
      tags: this.editingOperation.tags,
      attributes: this.editingOperation.attributes
    };

    this.operationUpdated.emit(updatedOperation);
    this.noteDrafts[operation.id] = updatedOperation.notes;
    this.editingOperationId = null;
    this.editingOperation = null;
  }

  isEditing(operationId: string): boolean {
    return this.editingOperationId === operationId;
  }

  addTag(): void {
    if (this.editingOperation) {
      this.editingOperation.tags.push('');
    }
  }

  removeTag(index: number): void {
    if (this.editingOperation) {
      this.editingOperation.tags.splice(index, 1);
    }
  }

  trackByIndex(index: number): number {
    return index;
  }

  addAttribute(): void {
    if (this.editingOperation) {
      const key = `key${Object.keys(this.editingOperation.attributes).length + 1}`;
      this.editingOperation.attributes[key] = '';
    }
  }

  removeAttribute(key: string): void {
    if (this.editingOperation) {
      delete this.editingOperation.attributes[key];
    }
  }

  updateAttributeKey(oldKey: string, newKey: string): void {
    if (this.editingOperation && oldKey !== newKey && newKey) {
      const value = this.editingOperation.attributes[oldKey];
      delete this.editingOperation.attributes[oldKey];
      this.editingOperation.attributes[newKey] = value;
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
}

