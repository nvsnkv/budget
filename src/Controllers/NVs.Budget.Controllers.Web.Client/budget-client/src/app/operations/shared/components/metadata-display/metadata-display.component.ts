import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ObjectKeysPipe } from '../../pipes/object-keys.pipe';

@Component({
  selector: 'app-metadata-display',
  standalone: true,
  imports: [CommonModule, ObjectKeysPipe],
  templateUrl: './metadata-display.component.html',
  styleUrls: ['./metadata-display.component.less']
})
export class MetadataDisplayComponent {
  @Input() metadata: Record<string, any> | null | undefined = null;
  @Input() title = 'Details';
}

