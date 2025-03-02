// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthResponse } from './auth-response.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl: string = "";

  constructor(private http: HttpClient) {}

  get BaseUrl(): string {
    return this.baseUrl;
  }

  setBaseUrl(url: string): void {
    this.baseUrl = url;
  }

  whoAmI(): Observable<AuthResponse> {
    return this.http.get<AuthResponse>(this.buildUrl('auth/whoami'));
  }

  logout(): Observable<void> {
    return this.http.post<void>(this.buildUrl('auth/logout'), {});
  }

  private buildUrl(endpoint: string): string {
    return `${this.baseUrl}/${endpoint}`;
  }
}