import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TuiButton } from '@taiga-ui/core';
import { IError, ISuccess } from '../../../../budget/models';
import { MetadataDisplayComponent } from '../metadata-display/metadata-display.component';

@Component({
  selector: 'app-operation-result',
  standalone: true,
  imports: [CommonModule, TuiButton, MetadataDisplayComponent],
  templateUrl: './operation-result.component.html',
  styleUrls: ['./operation-result.component.less']
})
export class OperationResultComponent {
  @Input() successes: ISuccess[] = [];
  @Input() errors: IError[] = [];
  
  showSuccesses = true;
  showErrors = true;

  toggleSuccesses(): void {
    this.showSuccesses = !this.showSuccesses;
  }

  toggleErrors(): void {
    this.showErrors = !this.showErrors;
  }
}

