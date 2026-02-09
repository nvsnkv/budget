import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of, shareReplay } from 'rxjs';
import { AppConfigService } from './config/app-config.service';

interface VersionResponse {
  version: string;
}

@Injectable({
  providedIn: 'root'
})
export class AppVersionService {
  private version$?: Observable<string | null>;

  constructor(
    private http: HttpClient,
    private configService: AppConfigService
  ) {}

  getVersion(): Observable<string | null> {
    if (!this.version$) {
      const url = `${this.configService.apiUrl}/api/v0.1/version`;
      this.version$ = this.http.get<VersionResponse>(url).pipe(
        map(response => response.version),
        catchError(error => {
          console.error('Failed to load app version:', error);
          return of(null);
        }),
        shareReplay(1)
      );
    }

    return this.version$;
  }
}
