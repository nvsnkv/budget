import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';
import { TuiButton } from '@taiga-ui/core';
import { CriteriaExample } from '../../models/example.interface';

@Component({
  selector: 'app-examples-section',
  standalone: true,
  imports: [TuiButton],
  templateUrl: './examples-section.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrls: ['./examples-section.component.less']
})
export class ExamplesSectionComponent {
  readonly examples = input<CriteriaExample[]>([]);
  readonly title = input('Common Examples');
  readonly expanded = model(false);

  toggle(): void {
    this.expanded.set(!this.expanded());
  }
}
