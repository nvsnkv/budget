import { Routes } from '@angular/router';
import { NewBudgetComponent } from './budget/new-budget/new-budget.component';
import { BudgetSettingsComponent } from './budget/budget-settings/budget-settings.component';
import { IndexComponent } from './index/index.component';

export const routes: Routes = [
    { path: 'budget/new', component: NewBudgetComponent },
    { path: 'budget/:budgetId/settings', component: BudgetSettingsComponent },
    { path: '', component: IndexComponent }
];
