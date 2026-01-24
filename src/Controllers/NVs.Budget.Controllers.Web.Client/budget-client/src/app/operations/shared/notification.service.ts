import { Injectable } from '@angular/core';
import { TuiDialogService } from '@taiga-ui/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  constructor(private dialogService: TuiDialogService) {}

  showError(message: string): Observable<any> {
    return this.dialogService.open(message, {
      label: 'Error',
      size: 'm',
      closeable: true,
      dismissible: true
    });
  }

  showSuccess(message: string): Observable<any> {
    return this.dialogService.open(message, {
      label: 'Success',
      size: 's',
      closeable: true,
      dismissible: true
    });
  }

  showWarning(message: string): Observable<any> {
    return this.dialogService.open(message, {
      label: 'Warning',
      size: 'm',
      closeable: true,
      dismissible: true
    });
  }

  confirm(message: string, title: string = 'Confirm'): Observable<boolean> {
    return new Observable(observer => {
      this.dialogService.open(message, {
        label: title,
        size: 'm',
        closeable: true,
        dismissible: true
      }).subscribe({
        next: () => {
          observer.next(true);
          observer.complete();
        },
        error: () => {
          observer.next(false);
          observer.complete();
        }
      });
    });
  }

  handleError(error: any, defaultMessage: string): string {
    let errorMessage = defaultMessage;
    
    if (error.status === 400 && Array.isArray(error.error)) {
      const errors = error.error as any[];
      errorMessage = errors.map(e => e.message || e).join('; ');
    } else if (error.error?.message) {
      errorMessage = error.error.message;
    }
    
    return errorMessage;
  }
}

