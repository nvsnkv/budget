import { Routes } from '@angular/router';
import { NewBudgetComponent } from './budget/new-budget/new-budget.component';
import { BudgetDetailComponent } from './budget/budget-detail/budget-detail.component';
import { IndexComponent } from './index/index.component';

export const routes: Routes = [
    { path: 'budget/new', component: NewBudgetComponent },
    { path: 'budget/:budgetId', component: BudgetDetailComponent },
    { path: '', component: IndexComponent }
];
