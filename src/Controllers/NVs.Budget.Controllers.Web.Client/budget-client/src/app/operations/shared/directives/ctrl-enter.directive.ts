import { Directive, HostListener, output } from '@angular/core';

@Directive({
  selector: '[appCtrlEnter]',
  standalone: true
})
export class CtrlEnterDirective {
  readonly appCtrlEnter = output<void>();

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && event.ctrlKey) {
      event.preventDefault();
      this.appCtrlEnter.emit();
    }
  }
}
