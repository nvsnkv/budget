import { TuiRoot } from "@taiga-ui/core";
import { TuiBlockStatus, TuiNavigation } from "@taiga-ui/layout"
import { Component, enableProdMode } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthComponent } from './auth/auth/auth.component';
import { environment } from '../environments/environment';
import { UserService } from "./auth/user.service";
import { CommonModule } from "@angular/common";
import { map } from "rxjs";
import { BudgetSelectorComponent } from "./budget/budget-selector/budget-selector.component";

if (environment.production) {
  enableProdMode();
 } 

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AuthComponent, TuiRoot, TuiNavigation, TuiBlockStatus, CommonModule, RouterLink, BudgetSelectorComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.less'
})
export class AppComponent {
  title = 'budget-client';

  get currentUser$() { return this.user.current$; }
  get isAuthenticated$() { return this.user.current$.pipe(map(u => u.isAuthenticated)); }
  get userId$() { return this.user.current$.pipe(map(u => u.id)); }
  get ownerName$() { return this.user.current$.pipe(map(u => u.ownerInfo?.name)); }

  constructor(private user: UserService) {  }  
}
