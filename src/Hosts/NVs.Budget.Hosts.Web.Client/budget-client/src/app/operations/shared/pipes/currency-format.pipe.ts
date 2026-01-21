import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'currencyFormat',
  standalone: true
})
export class CurrencyFormatPipe implements PipeTransform {
  transform(amount: number, currencyCode: string | null = null): string {
    // Format number with space as thousand separator
    const formattedAmount = amount.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
    return currencyCode ? `${formattedAmount} ${currencyCode}` : formattedAmount;
  }
}

