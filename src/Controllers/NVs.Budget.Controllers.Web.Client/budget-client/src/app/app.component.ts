import { TuiRoot, TuiButton, TuiIcon } from "@taiga-ui/core";
import { TuiBlockStatus, TuiNavigation } from "@taiga-ui/layout"
import { Component, enableProdMode } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthComponent } from './auth/auth/auth.component';
import { environment } from '../environments/environment';
import { UserService } from "./auth/user.service";
import { CommonModule } from "@angular/common";
import { combineLatest, filter, map, Observable, startWith } from "rxjs";
import { BudgetSelectorComponent } from "./budget/budget-selector/budget-selector.component";
import { ThemeService } from "./theme.service";
import { AppVersionService } from "./app-version.service";

if (environment.production) {
  enableProdMode();
} 

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AuthComponent, TuiRoot, TuiNavigation, TuiBlockStatus, CommonModule, RouterLink, RouterLinkActive, BudgetSelectorComponent, TuiButton, TuiIcon],
  templateUrl: './app.component.html',
  styleUrl: './app.component.less'
})
export class AppComponent {
  title = 'budget-client';
  private readonly budgetIdPattern = new RegExp("^/budget/([^/]+)");
  private readonly operationsContextPattern = new RegExp("^/budget/[^/]+/(operations|transfers)(?:/|$)");

  get currentUser$() { return this.user.current$; }
  get isAuthenticated$() { return this.user.current$.pipe(map(u => u.isAuthenticated)); }
  get userId$() { return this.user.current$.pipe(map(u => u.id)); }
  get ownerName$() { return this.user.current$.pipe(map(u => u.ownerInfo?.name)); }
  get isDarkTheme$() { return this.theme.isDark$; }
  get appVersion$() { return this.versionService.getVersion(); }
  readonly currentUrl$: Observable<string>;
  readonly selectedBudgetId$: Observable<string | null>;
  readonly navLinks$: Observable<{ label: string; commands: string[] }[]>;

  constructor(
    private user: UserService,
    private theme: ThemeService,
    private versionService: AppVersionService,
    private router: Router
  ) {
    this.currentUrl$ = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      map(() => this.router.url),
      startWith(this.router.url)
    );

    this.selectedBudgetId$ = this.currentUrl$.pipe(
      map(url => this.budgetIdPattern.exec(url)?.[1] ?? null)
    );

    this.navLinks$ = combineLatest([this.selectedBudgetId$, this.currentUrl$]).pipe(
      map(([budgetId, url]) => {
        if (!budgetId) {
          return [];
        }

        if (this.operationsContextPattern.test(url)) {
          return [
            { label: 'logbook', commands: ['/budget', budgetId, 'operations', 'logbook'] },
            { label: 'operations', commands: ['/budget', budgetId, 'operations'] },
            { label: 'transfers', commands: ['/budget', budgetId, 'transfers'] },
            { label: 'import', commands: ['/budget', budgetId, 'operations', 'import'] },
            { label: 'delete', commands: ['/budget', budgetId, 'operations', 'delete'] },
            { label: 'settings', commands: ['/budget', budgetId, 'reading-settings'] }
          ];
        }

        return [
          { label: 'logbook', commands: ['/budget', budgetId, 'operations', 'logbook'] },
          { label: 'operations', commands: ['/budget', budgetId, 'operations'] },
          { label: 'settings', commands: ['/budget', budgetId, 'reading-settings'] }
        ];
      })
    );
  }  
  
  toggleTheme(): void {
    this.theme.toggleTheme();
  }
}
