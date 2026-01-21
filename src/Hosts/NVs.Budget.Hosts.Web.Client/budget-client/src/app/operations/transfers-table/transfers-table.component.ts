import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TuiButton, TuiExpand } from '@taiga-ui/core';
import { TuiChip } from '@taiga-ui/kit';
import { TransferResponse } from '../../budget/models';
import { CurrencyFormatPipe } from '../shared/pipes/currency-format.pipe';
import { DateFormatPipe } from '../shared/pipes/date-format.pipe';
import { OperationsTableComponent } from '../operations-table/operations-table.component';

@Component({
  selector: 'app-transfers-table',
  standalone: true,
  imports: [
    CommonModule,
    TuiButton,
    TuiExpand,
    TuiChip,
    CurrencyFormatPipe,
    DateFormatPipe,
    OperationsTableComponent
  ],
  templateUrl: './transfers-table.component.html',
  styleUrls: ['./transfers-table.component.less']
})
export class TransfersTableComponent {
  @Input() transfers: TransferResponse[] = [];
  @Input() showActions = true;
  @Input() showQuickRegister = false;
  @Output() transferDeleted = new EventEmitter<TransferResponse>();
  @Output() transferRegistered = new EventEmitter<TransferResponse>();
  
  expandedTransferId: string | null = null;

  toggleTransferDetails(transferId: string): void {
    this.expandedTransferId = this.expandedTransferId === transferId ? null : transferId;
  }

  deleteTransfer(transfer: TransferResponse): void {
    this.transferDeleted.emit(transfer);
  }

  registerTransfer(transfer: TransferResponse): void {
    this.transferRegistered.emit(transfer);
  }

  getTransferOperations(transfer: TransferResponse) {
    return [transfer.source, transfer.sink];
  }

  trackByIndex(index: number): number {
    return index;
  }
}

