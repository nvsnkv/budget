import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface AppConfig {
  apiUrl: string;
}

@Injectable({
  providedIn: 'root'
})
export class AppConfigService {
  private config?: AppConfig;

  constructor(private http: HttpClient) {}

  loadConfig(): Promise<void> {
    return firstValueFrom(
      this.http.get<AppConfig>('/api/config')
    ).then(config => {
      this.config = config;
    }).catch(error => {
      console.error('Failed to load application configuration:', error);
      // Fallback to default config
      this.config = {
        apiUrl: 'https://localhost:25001'
      };
    });
  }

  get apiUrl(): string {
    return this.config?.apiUrl || 'https://localhost:25001';
  }
}
