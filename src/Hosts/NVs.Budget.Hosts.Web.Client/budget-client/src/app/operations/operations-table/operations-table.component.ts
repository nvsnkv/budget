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
  @Input() operations: OperationResponse[] = [];
  @Input() showActions = true;
  @Output() operationDeleted = new EventEmitter<OperationResponse>();
  @Output() operationUpdated = new EventEmitter<OperationResponse>();
  
  expandedOperationId: string | null = null;
  editingOperationId: string | null = null;
  editingOperation: EditableOperation | null = null;

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
      amount: {
        value: this.editingOperation.amount,
        currencyCode: this.editingOperation.currencyCode
      },
      tags: this.editingOperation.tags,
      attributes: this.editingOperation.attributes
    };

    this.operationUpdated.emit(updatedOperation);
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
}

