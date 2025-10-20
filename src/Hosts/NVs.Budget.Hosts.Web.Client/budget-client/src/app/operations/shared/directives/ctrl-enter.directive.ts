import { Directive, EventEmitter, HostListener, Output } from '@angular/core';

@Directive({
  selector: '[appCtrlEnter]',
  standalone: true
})
export class CtrlEnterDirective {
  @Output() appCtrlEnter = new EventEmitter<void>();

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && event.ctrlKey) {
      event.preventDefault();
      this.appCtrlEnter.emit();
    }
  }
}

