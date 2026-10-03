import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TuiButton } from '@taiga-ui/core';

interface AttributeEntry {
  key: string;
  value: string;
}

@Component({
  selector: 'app-attributes-editor',
  standalone: true,
  imports: [FormsModule, TuiButton],
  templateUrl: './attributes-editor.component.html',
  styleUrls: ['./attributes-editor.component.less']
})
export class AttributesEditorComponent implements OnChanges {
  @Input() model: Record<string, any> | null | undefined = {};
  @Output() modelChange = new EventEmitter<Record<string, any>>();

  entries: AttributeEntry[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['model']) {
      this.entries = Object.entries(this.model ?? {}).map(([key, value]) => ({
        key,
        value: value == null ? '' : String(value)
      }));
    }
  }

  addAttribute(): void {
    const key = this.nextDefaultKey();
    this.entries = [...this.entries, { key, value: '' }];
    this.emitModel();
  }

  removeAttribute(index: number): void {
    this.entries = this.entries.filter((_, i) => i !== index);
    this.emitModel();
  }

  updateEntry(index: number, key: string, value: string): void {
    this.entries[index] = { key, value };
    this.emitModel();
  }

  trackByIndex(index: number): number {
    return index;
  }

  private emitModel(): void {
    const next: Record<string, any> = {};
    for (const entry of this.entries) {
      const key = entry.key.trim();
      if (!key) {
        continue;
      }
      next[key] = entry.value;
    }

    this.modelChange.emit(next);
  }

  private nextDefaultKey(): string {
    const existingKeys = new Set(this.entries.map(entry => entry.key));
    let i = this.entries.length + 1;
    while (existingKeys.has(`key${i}`)) {
      i++;
    }
    return `key${i}`;
  }
}
