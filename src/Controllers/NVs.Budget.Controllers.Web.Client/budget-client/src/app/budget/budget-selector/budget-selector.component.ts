import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { BudgetApiService as BudgetApiService } from '../budget-api.service';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { TuiButton, TuiDataList, TuiDropdown } from '@taiga-ui/core';
import { TuiChevron } from '@taiga-ui/kit'
import { toSignal } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs';

@Component({
  selector: 'app-budget-selector',
  templateUrl: './budget-selector.component.html',
  styleUrls: ['./budget-selector.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TuiButton, TuiChevron, TuiDataList, TuiDropdown, RouterLink]
})
export class BudgetSelectorComponent {
  private readonly budgetIdPattern = new RegExp("^/budget/([^/]+)");

  private readonly budgetApiService = inject(BudgetApiService);
  private readonly router = inject(Router);

  readonly budgets = this.budgetApiService.budgets;

  private readonly navigationEnd = toSignal<NavigationEnd | null>(
    this.router.events.pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd)),
    { initialValue: null },
  );

  readonly currentUrl = computed(() => {
    this.navigationEnd();
    return this.router.url;
  });

  readonly selectedBudget = computed(() => {
    const budgetId = this.budgetIdPattern.exec(this.currentUrl())?.[1];
    return budgetId ? this.budgets().find(budget => budget.id === budgetId) : undefined;
  });
}
