import { TuiButton, TuiRoot } from "@taiga-ui/core";
import { TuiBlockStatus, TuiNavigation } from "@taiga-ui/layout"
import { Component, enableProdMode } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthComponent } from './auth/auth/auth.component';
import { environment } from '../environments/environment';

if (environment.production) {
  enableProdMode();
 } 

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AuthComponent, TuiRoot, TuiNavigation, TuiBlockStatus, TuiButton],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'budget-client';
}
