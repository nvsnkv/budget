import { Component, enableProdMode } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthComponent } from './auth/auth/auth.component';
import { environment } from '../environments/environment';
import { AuthService } from './auth/auth/auth.service';

if (environment.production) {
  enableProdMode();
 } 

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AuthComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'budget-client';
}
