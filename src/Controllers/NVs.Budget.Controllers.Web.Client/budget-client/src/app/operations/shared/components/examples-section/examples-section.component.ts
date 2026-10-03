import { Component, Input } from '@angular/core';
import { TuiButton } from '@taiga-ui/core';
import { CriteriaExample } from '../../models/example.interface';

@Component({
  selector: 'app-examples-section',
  standalone: true,
  imports: [TuiButton],
  templateUrl: './examples-section.component.html',
  styleUrls: ['./examples-section.component.less']
})
export class ExamplesSectionComponent {
  @Input() examples: CriteriaExample[] = [];
  @Input() title = 'Common Examples';
  @Input() expanded = false;

  toggle(): void {
    this.expanded = !this.expanded;
  }
}

