import { Component, OnDestroy, OnInit } from '@angular/core';
import { BudgetApiService as BudgetApiService } from '../budget-api.service';
import { BudgetResponse } from '../models';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { TuiButton, TuiDataList, TuiDropdown } from '@taiga-ui/core';
import { TuiChevron } from '@taiga-ui/kit'
import { BehaviorSubject, filter, Observable, Subscription } from 'rxjs';
import { AsyncPipe, CommonModule } from '@angular/common';

@Component({
  selector: 'app-budget-selector',
  templateUrl: './budget-selector.component.html',
  styleUrls: ['./budget-selector.component.less'],
  imports: [CommonModule, TuiButton, TuiChevron, TuiDataList, TuiDropdown, RouterLink, AsyncPipe]
})
export class BudgetSelectorComponent implements OnInit, OnDestroy {
  private budgetIdPattern = new RegExp("^/budget/([^/]*)(/.*)?$");
  private budgetSub: Subscription | undefined;
  private routerSub: Subscription | undefined;
  private selectedBudgetId: string | null = null;

  budgets$: Observable<BudgetResponse[]> | undefined;
  budgetsSnapshot: BudgetResponse[] = [];
  selectedBudget$: BehaviorSubject<BudgetResponse | undefined> = new BehaviorSubject<BudgetResponse | undefined>(undefined);
  
  constructor(
    private budgetApiService: BudgetApiService, 
    private router: Router
  ) {}
  
  ngOnDestroy(): void {
    console.log('destroying budget selector component');
    this.budgetSub?.unsubscribe();
    this.routerSub?.unsubscribe();
  }

  ngOnInit(): void {
    this.setBudgetIdFrom(this.router.url);

    this.budgets$ = this.budgetApiService.getAllBudgets();
    this.routerSub = this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(n => {
        this.setBudgetIdFrom(n.url);
        this.updateSelectedBudget();
      });

    this.budgetSub = this.budgets$.subscribe(budgets => {
      this.budgetsSnapshot = budgets;
      this.updateSelectedBudget();
    });
  }

  setBudgetIdFrom(url:string) {
    this.selectedBudgetId = this.budgetIdPattern.exec(url)?.[1] ?? null;
  }

  updateSelectedBudget() {
    if (this.selectedBudgetId) {
      const selectedBudget = this.budgetsSnapshot.find(budget => budget.id === this.selectedBudgetId);
      this.selectedBudget$.next(selectedBudget);
    } else {
      this.selectedBudget$.next(undefined);
    }
  }
}