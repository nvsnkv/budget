import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TuiButton } from '@taiga-ui/core';
import { IError, ISuccess } from '../../../../budget/models';
import { MetadataDisplayComponent } from '../metadata-display/metadata-display.component';

@Component({
  selector: 'app-operation-result',
  standalone: true,
  imports: [TuiButton, MetadataDisplayComponent],
  templateUrl: './operation-result.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrls: ['./operation-result.component.less']
})
export class OperationResultComponent {
  readonly successes = input<ISuccess[]>([]);
  readonly errors = input<IError[]>([]);

  showSuccesses = true;
  showErrors = true;

  toggleSuccesses(): void {
    this.showSuccesses = !this.showSuccesses;
  }

  toggleErrors(): void {
    this.showErrors = !this.showErrors;
  }
}
