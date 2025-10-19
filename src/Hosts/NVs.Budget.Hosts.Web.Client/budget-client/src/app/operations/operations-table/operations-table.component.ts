import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OperationResponse } from '../../budget/models';
import { TuiButton, TuiExpand } from '@taiga-ui/core';
import { TuiChip } from '@taiga-ui/kit';

@Component({
  selector: 'app-operations-table',
  standalone: true,
  imports: [
    CommonModule,
    TuiButton,
    TuiExpand,
    TuiChip
  ],
  templateUrl: './operations-table.component.html',
  styleUrls: ['./operations-table.component.less']
})
export class OperationsTableComponent {
  @Input() operations: OperationResponse[] = [];
  @Input() showActions = true;
  @Output() operationDeleted = new EventEmitter<OperationResponse>();
  
  expandedOperationId: string | null = null;

  toggleOperationDetails(operationId: string): void {
    this.expandedOperationId = this.expandedOperationId === operationId ? null : operationId;
  }

  formatCurrency(amount: number, currencyCode: string): string {
    // Format number with space as thousand separator
    const formattedAmount = amount.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
    return `${formattedAmount} ${currencyCode}`;
  }

  formatDate(timestamp: string): string {
    const date = new Date(timestamp);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');
    return `${year}.${month}.${day}, ${hours}:${minutes}:${seconds}`;
  }

  getObjectKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }

  deleteOperation(operation: OperationResponse): void {
    this.operationDeleted.emit(operation);
  }
}

