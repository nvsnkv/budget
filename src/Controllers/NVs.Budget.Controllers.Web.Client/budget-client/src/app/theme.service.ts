import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'budget-app-theme';
  private readonly darkTheme = signal<boolean>(this.getInitialTheme());

  readonly isDark = this.darkTheme.asReadonly();

  toggleTheme(): void {
    const newTheme = !this.darkTheme();
    this.darkTheme.set(newTheme);
    localStorage.setItem(this.THEME_KEY, newTheme ? 'dark' : 'light');
  }

  private getInitialTheme(): boolean {
    const savedTheme = localStorage.getItem(this.THEME_KEY);
    if (savedTheme) {
      return savedTheme === 'dark';
    }
    // Check system preference
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
  }
}
