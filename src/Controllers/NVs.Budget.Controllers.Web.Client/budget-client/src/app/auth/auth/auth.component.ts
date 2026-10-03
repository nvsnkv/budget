// auth-status.component.ts
import { ChangeDetectionStrategy, Component, computed, inject, OnInit } from '@angular/core';
import { AuthService } from './auth.service';
import { UserService } from '../user.service';
import { TuiLink } from '@taiga-ui/core';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TuiLink],
})
export class AuthComponent implements OnInit {
  private readonly user = inject(UserService);
  private readonly authService = inject(AuthService);

  readonly isAuthenticated = computed(() => this.user.currentUser().isAuthenticated);

  ngOnInit() {
    // Base URL is already set by APP_INITIALIZER
    this.checkAuthentication();
  }

  get BaseUrl() {
    return this.authService.BaseUrl;
  }

  checkAuthentication() {
    this.authService.whoAmI().subscribe(response => {
      if (response.isAuthenticated) {
        this.user.setCurrentUser({
          isAuthenticated: true,
          id: response.user!.id,
          ownerInfo: response.owner
        })
      }
      else {
        this.user.setCurrentUser({
          isAuthenticated: false
        })
      }
    });
  }
}
