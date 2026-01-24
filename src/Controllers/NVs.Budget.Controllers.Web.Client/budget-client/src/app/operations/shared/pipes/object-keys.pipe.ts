import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'objectKeys',
  standalone: true
})
export class ObjectKeysPipe implements PipeTransform {
  transform(obj: Record<string, any> | null | undefined): string[] {
    return obj ? Object.keys(obj) : [];
  }
}

