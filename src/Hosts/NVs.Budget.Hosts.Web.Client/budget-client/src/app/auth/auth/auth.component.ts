// auth-status.component.ts
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { CommonModule } from '@angular/common';
import { environment } from '../../../environments/environment'
import { UserService } from '../user.service';
import { TuiLink } from '@taiga-ui/core';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css'],
  imports: [CommonModule, TuiLink],
})
export class AuthComponent implements OnInit {
  isAuthenticated = false;

  constructor(
    private authService: AuthService,
    private user: UserService
  ) {}

  ngOnInit() {
    this.authService.setBaseUrl(environment.apiUrl); // Задаем базовый URL сервера
    this.checkAuthentication();
  }

  get BaseUrl() {
    return this.authService.BaseUrl;
  }

  checkAuthentication() {
    this.authService.whoAmI().subscribe(response => {
      this.isAuthenticated = response.isAuthenticated;
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