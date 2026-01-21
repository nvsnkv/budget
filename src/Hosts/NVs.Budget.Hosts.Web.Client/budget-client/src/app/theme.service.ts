import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'budget-app-theme';
  private isDarkTheme$ = new BehaviorSubject<boolean>(this.getInitialTheme());

  get isDark$(): Observable<boolean> {
    return this.isDarkTheme$.asObservable();
  }

  get isDark(): boolean {
    return this.isDarkTheme$.value;
  }

  toggleTheme(): void {
    const newTheme = !this.isDarkTheme$.value;
    this.isDarkTheme$.next(newTheme);
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

