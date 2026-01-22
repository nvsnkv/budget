import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, startWith, switchMap, tap } from 'rxjs';
import { 
  BudgetResponse, 
  RegisterBudgetRequest, 
  UpdateBudgetRequest, 
  ChangeBudgetOwnersRequest, 
  MergeBudgetsRequest,
  IError,
  FileReadingSettingResponse,
  Owner
} from './models';
import { AppConfigService } from '../config/app-config.service';

@Injectable({
  providedIn: 'root'
})
export class BudgetApiService {
  public readonly baseUrl: string;
  private refresh$ = new BehaviorSubject<boolean>(false);

  constructor(
    private http: HttpClient,
    private configService: AppConfigService
  ) {
    this.baseUrl = this.configService.apiUrl + '/api/v0.1';
  }

  /**
   * Get all budgets available to the current user
   */
  getAllBudgets(): Observable<BudgetResponse[]> {
    return this.refresh$.pipe(
      startWith(undefined),
      switchMap(() => 
        this.http.get<BudgetResponse[]>(`${this.baseUrl}/budget`, { withCredentials: true })
      ));
  }

  /**
   * Get all available owners
   */
  getOwners(): Observable<Owner[]> {
    return this.http.get<Owner[]>(`${this.baseUrl}/owners`, { withCredentials: true });
  }

  /**
   * Get budget by ID (from list)
   */
  getBudgetById(id: string): Observable<BudgetResponse | undefined> {
    return this.getAllBudgets().pipe(
      switchMap(budgets => [budgets.find(b => b.id === id)])
    );
  }

  /**
   * Register a new budget
   */
  createBudget(request: RegisterBudgetRequest): Observable<BudgetResponse> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.post<BudgetResponse>(`${this.baseUrl}/budget`, request, { 
      headers, 
      withCredentials: true 
    }).pipe(tap(() => this.refresh$.next(true)));
  }

  /**
   * Update an existing budget
   */
  updateBudget(id: string, request: UpdateBudgetRequest): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.put<void>(`${this.baseUrl}/budget/${id}`, request, { 
      headers,
      withCredentials: true 
    }).pipe(tap(() => this.refresh$.next(true)));
  }

  /**
   * Change budget owners
   */
  changeBudgetOwners(request: ChangeBudgetOwnersRequest): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.put<void>(`${this.baseUrl}/budget/owners`, request, { 
      headers,
      withCredentials: true 
    }).pipe(tap(() => this.refresh$.next(true)));
  }

  /**
   * Remove a budget
   */
  removeBudget(id: string, version: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/budget/${id}?version=${encodeURIComponent(version)}`, { 
      withCredentials: true 
    }).pipe(tap(() => this.refresh$.next(true)));
  }

  /**
   * Merge multiple budgets
   */
  mergeBudgets(request: MergeBudgetsRequest): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.post<void>(`${this.baseUrl}/budget/merge`, request, { 
      headers,
      withCredentials: true 
    }).pipe(tap(() => this.refresh$.next(true)));
  }

  /**
   * Download budget configuration as YAML
   */
  downloadBudgetYaml(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/budget/${id}`, {
      responseType: 'blob',
      headers: new HttpHeaders().set('Accept', 'application/yaml'),
      withCredentials: true
    });
  }

  /**
   * Upload budget configuration from YAML content
   */
  uploadBudgetYaml(id: string, yamlContent: string): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/yaml');
    return this.http.put<void>(`${this.baseUrl}/budget/${id}`, yamlContent, {
      headers,
      withCredentials: true
    }).pipe(tap(() => this.refresh$.next(true)));
  }

  /**
   * Download CSV reading options as YAML
   */
  downloadCsvOptionsYaml(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/budget/${id}/csv-options.yaml`, {
      responseType: 'blob',
      withCredentials: true
    });
  }

  /**
   * Upload CSV reading options from YAML file
   */
  uploadCsvOptionsYaml(id: string, file: File): Observable<void> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.put<void>(`${this.baseUrl}/budget/${id}/csv-options`, formData, { withCredentials: true });
  }

  /**
   * Get file reading settings for a budget
   */
  getReadingSettings(budgetId: string): Observable<Record<string, FileReadingSettingResponse>> {
    return this.http.get<Record<string, FileReadingSettingResponse>>(`${this.baseUrl}/budget/${budgetId}/reading-settings`, {
      withCredentials: true
    });
  }

  /**
   * Update file reading settings for a budget
   */
  updateReadingSettings(budgetId: string, settings: Record<string, FileReadingSettingResponse>): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.put<void>(`${this.baseUrl}/budget/${budgetId}/reading-settings`, settings, {
      headers,
      withCredentials: true
    });
  }

  /**
   * Download reading settings as YAML
   */
  downloadReadingSettingsYaml(budgetId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/budget/${budgetId}/reading-settings`, {
      responseType: 'blob',
      headers: new HttpHeaders().set('Accept', 'application/yaml'),
      withCredentials: true
    });
  }

  /**
   * Upload reading settings from YAML content
   */
  uploadReadingSettingsYaml(budgetId: string, yamlContent: string): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/yaml');
    return this.http.put<void>(`${this.baseUrl}/budget/${budgetId}/reading-settings`, yamlContent, {
      headers,
      withCredentials: true
    });
  }
}