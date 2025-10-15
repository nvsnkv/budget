import { Routes } from '@angular/router';
import { NewBudgetComponent } from './budget/new-budget/new-budget.component';
import { BudgetDetailComponent } from './budget/budget-detail/budget-detail.component';
import { ReadingSettingsComponent } from './budget/reading-settings/reading-settings.component';
import { ImportOperationsComponent } from './operations/import-operations/import-operations.component';
import { OperationsListComponent } from './operations/operations-list/operations-list.component';
import { IndexComponent } from './index/index.component';

export const routes: Routes = [
    { path: 'budget/new', component: NewBudgetComponent },
    { path: 'budget/:budgetId/operations/import', component: ImportOperationsComponent },
    { path: 'budget/:budgetId/operations', component: OperationsListComponent },
    { path: 'budget/:budgetId/reading-settings', component: ReadingSettingsComponent },
    { path: 'budget/:budgetId', component: BudgetDetailComponent },
    { path: '', component: IndexComponent }
];
