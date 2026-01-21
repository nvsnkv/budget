import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TuiButton } from '@taiga-ui/core';
import { CriteriaExample } from '../../models/example.interface';

@Component({
  selector: 'app-examples-section',
  standalone: true,
  imports: [CommonModule, TuiButton],
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

