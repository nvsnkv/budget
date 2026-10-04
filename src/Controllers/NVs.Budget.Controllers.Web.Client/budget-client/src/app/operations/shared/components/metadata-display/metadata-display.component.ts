import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ObjectKeysPipe } from '../../pipes/object-keys.pipe';

@Component({
  selector: 'app-metadata-display',
  standalone: true,
  imports: [ObjectKeysPipe],
  templateUrl: './metadata-display.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrls: ['./metadata-display.component.less']
})
export class MetadataDisplayComponent {
  readonly metadata = input<Record<string, any> | null | undefined>(null);
  readonly title = input('Details');
}
