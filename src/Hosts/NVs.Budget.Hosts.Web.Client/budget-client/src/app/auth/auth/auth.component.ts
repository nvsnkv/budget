// auth-status.component.ts
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { CommonModule } from '@angular/common';
import { environment } from '../../../environments/environment'

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css'],
  imports: [CommonModule],
})
export class AuthComponent implements OnInit {
  isAuthenticated = false;

  constructor(
    private authService: AuthService,
    private router: Router
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
    });
  }
}