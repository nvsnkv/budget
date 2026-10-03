import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TuiButton } from '@taiga-ui/core';
import { TuiChip } from '@taiga-ui/kit';
import { TransferResponse } from '../../budget/models';
import { CurrencyFormatPipe } from '../shared/pipes/currency-format.pipe';
import { DateFormatPipe } from '../shared/pipes/date-format.pipe';
import { OperationsTableComponent } from '../operations-table/operations-table.component';

@Component({
  selector: 'app-transfers-table',
  standalone: true,
  imports: [
    TuiButton,
    TuiChip,
    CurrencyFormatPipe,
    DateFormatPipe,
    OperationsTableComponent
  ],
  templateUrl: './transfers-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrls: ['./transfers-table.component.less']
})
export class TransfersTableComponent {
  readonly transfers = input<TransferResponse[]>([]);
  readonly showActions = input(true);
  readonly showQuickRegister = input(false);
  readonly enableSelection = input(false);
  readonly transferDeleted = output<TransferResponse>();
  readonly transferRegistered = output<TransferResponse>();
  readonly selectionChanged = output<TransferResponse[]>();

  expandedTransferId: string | null = null;
  selectedTransferIds = new Set<string>();

  toggleTransferDetails(transferId: string): void {
    this.expandedTransferId = this.expandedTransferId === transferId ? null : transferId;
  }

  deleteTransfer(transfer: TransferResponse): void {
    this.transferDeleted.emit(transfer);
  }

  registerTransfer(transfer: TransferResponse): void {
    this.transferRegistered.emit(transfer);
  }

  toggleSelection(transfer: TransferResponse, isSelected: boolean): void {
    if (isSelected) {
      this.selectedTransferIds.add(transfer.sourceId);
    } else {
      this.selectedTransferIds.delete(transfer.sourceId);
    }
    this.emitSelection();
  }

  toggleSelectAll(isSelected: boolean): void {
    this.selectedTransferIds.clear();
    if (isSelected) {
      for (const transfer of this.transfers()) {
        this.selectedTransferIds.add(transfer.sourceId);
      }
    }
    this.emitSelection();
  }

  isSelected(transfer: TransferResponse): boolean {
    return this.selectedTransferIds.has(transfer.sourceId);
  }

  isAllSelected(): boolean {
    return this.transfers().length > 0 && this.selectedTransferIds.size === this.transfers().length;
  }

  private emitSelection(): void {
    const selectedTransfers = this.transfers().filter(transfer => this.selectedTransferIds.has(transfer.sourceId));
    this.selectionChanged.emit(selectedTransfers);
  }

  getTransferOperations(transfer: TransferResponse) {
    return [transfer.source, transfer.sink];
  }

  trackByIndex(index: number): number {
    return index;
  }
}
