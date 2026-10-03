import { TuiRoot, TuiButton, TuiIcon } from "@taiga-ui/core";
import { TuiBlockStatus, TuiNavigation } from "@taiga-ui/layout"
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthComponent } from './auth/auth/auth.component';
import { UserService } from "./auth/user.service";
import { toSignal } from '@angular/core/rxjs-interop';
import { filter } from "rxjs";
import { BudgetSelectorComponent } from "./budget/budget-selector/budget-selector.component";
import { ThemeService } from "./theme.service";
import { AppVersionService } from "./app-version.service";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AuthComponent, TuiRoot, TuiNavigation, TuiBlockStatus, RouterLink, RouterLinkActive, BudgetSelectorComponent, TuiButton, TuiIcon],
  templateUrl: './app.component.html',
  styleUrl: './app.component.less',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  title = 'budget-client';
  private readonly newBudgetRoutePattern = new RegExp("^/budget/new(?:/|$)");
  private readonly budgetIdPattern = new RegExp("^/budget/([^/]+)");
  private readonly operationsContextPattern = new RegExp("^/budget/[^/]+/(operations|transfers)(?:/|$)");
  private readonly detailsContextPattern = new RegExp("^/budget/[^/]+/details(?:/|$)");
  private readonly readingSettingsContextPattern = new RegExp("^/budget/[^/]+/reading-settings(?:/|$)");

  private readonly user = inject(UserService);
  private readonly theme = inject(ThemeService);
  private readonly versionService = inject(AppVersionService);
  private readonly router = inject(Router);

  readonly appVersion = toSignal(this.versionService.getVersion(), { initialValue: null as string | null });
  readonly currentUser = this.user.currentUser;
  readonly isAuthenticated = computed(() => this.currentUser().isAuthenticated);
  readonly ownerName = computed(() => this.currentUser().ownerInfo?.name);
  readonly isDarkTheme = this.theme.isDark;

  private readonly navigationEnd = toSignal<NavigationEnd | null>(
    this.router.events.pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd)),
    { initialValue: null },
  );

  readonly currentUrl = computed(() => {
    this.navigationEnd();
    return this.router.url;
  });

  readonly selectedBudgetId = computed(() =>
    this.budgetIdPattern.exec(this.currentUrl())?.[1] ?? null
  );

  readonly navLinks = computed<{ label: string; commands: string[] }[]>(() => {
    const url = this.currentUrl();
    const budgetId = this.selectedBudgetId();

    if (this.newBudgetRoutePattern.test(url)) {
      return [];
    }

    if (!budgetId) {
      return [];
    }

    const isDetailsSelected = this.detailsContextPattern.test(url);
    const isReadingSettingsSelected = this.readingSettingsContextPattern.test(url);
    const baseLinks = [
      { label: 'logbook', commands: ['/budget', budgetId, 'operations', 'logbook'] },
      { label: 'operations', commands: ['/budget', budgetId, 'operations'] },
      { label: 'details', commands: ['/budget', budgetId, 'details'] }
    ];

    if (isDetailsSelected || isReadingSettingsSelected) {
      baseLinks.push({ label: 'file reading settings', commands: ['/budget', budgetId, 'reading-settings'] });
    }

    if (this.operationsContextPattern.test(url)) {
      return [
        { label: 'logbook', commands: ['/budget', budgetId, 'operations', 'logbook'] },
        { label: 'operations', commands: ['/budget', budgetId, 'operations'] },
        { label: 'retag', commands: ['/budget', budgetId, 'operations', 'retag'] },
        { label: 'bulk changes', commands: ['/budget', budgetId, 'operations', 'bulk-changes'] },
        { label: 'import', commands: ['/budget', budgetId, 'operations', 'import'] },
        { label: 'manual import', commands: ['/budget', budgetId, 'operations', 'manual-import'] },
        { label: 'delete', commands: ['/budget', budgetId, 'operations', 'delete'] },
        { label: 'transfers', commands: ['/budget', budgetId, 'transfers'] },
        { label: 'details', commands: ['/budget', budgetId, 'details'] }
      ];
    }

    return baseLinks;
  });

  toggleTheme(): void {
    this.theme.toggleTheme();
  }
}
