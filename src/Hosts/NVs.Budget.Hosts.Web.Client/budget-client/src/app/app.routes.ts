import { Routes } from '@angular/router';
import { NewBudgetComponent } from './budget/new-budget/new-budget.component';
import { BudgetDetailComponent } from './budget/budget-detail/budget-detail.component';
import { ReadingSettingsComponent } from './budget/reading-settings/reading-settings.component';
import { ImportOperationsComponent } from './operations/import-operations/import-operations.component';
import { OperationsListComponent } from './operations/operations-list/operations-list.component';
import { DeleteOperationsComponent } from './operations/delete-operations/delete-operations.component';
import { RetagOperationsComponent } from './operations/retag-operations/retag-operations.component';
import { LogbookViewComponent } from './operations/logbook-view/logbook-view.component';
import { DuplicatesListComponent } from './operations/duplicates-list/duplicates-list.component';
import { IndexComponent } from './index/index.component';

export const routes: Routes = [
    { path: 'budget/new', component: NewBudgetComponent },
    { path: 'budget/:budgetId/operations/import', component: ImportOperationsComponent },
    { path: 'budget/:budgetId/operations/delete', component: DeleteOperationsComponent },
    { path: 'budget/:budgetId/operations/retag', component: RetagOperationsComponent },
    { path: 'budget/:budgetId/operations/logbook', component: LogbookViewComponent },
    { path: 'budget/:budgetId/operations/duplicates', component: DuplicatesListComponent },
    { path: 'budget/:budgetId/operations', component: OperationsListComponent },
    { path: 'budget/:budgetId/details', component: BudgetDetailComponent },
    { path: 'budget/:budgetId/reading-settings', component: ReadingSettingsComponent },
    { path: 'budget/:budgetId', redirectTo: 'budget/:budgetId/operations', pathMatch: 'full' },
    { path: '', component: IndexComponent }
];
