import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, httpResource } from '@angular/common/http';
import { computed } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import {
  BudgetResponse,
  RegisterBudgetRequest,
  UpdateBudgetRequest,
  ChangeBudgetOwnersRequest,
  MergeBudgetsRequest,
  IError,
  FileReadingSettingResponse,
  Owner,
  UpdateTaggingCriteriaRequest,
  UpdateTransferCriteriaRequest,
  UpdateLogbookCriteriaRequest
} from './models';
import { AppConfigService } from '../config/app-config.service';

@Injectable({
  providedIn: 'root'
})
export class BudgetApiService {
  public readonly baseUrl: string;

  private readonly budgetsResource = httpResource<BudgetResponse[]>(
    () => ({ url: `${this.baseUrl}/budget`, withCredentials: true }),
  );

  /** All budgets visible to the current user; reloaded automatically after every mutation. */
  readonly budgets = computed<BudgetResponse[]>(() => this.budgetsResource.value() ?? []);
  readonly budgetsLoading = computed(() => this.budgetsResource.isLoading());
  readonly budgetsError = computed(() => this.budgetsResource.error());

  constructor(
    private http: HttpClient,
    private configService: AppConfigService
  ) {
    this.baseUrl = this.configService.apiUrl + '/api/v0.1';
  }

  /** One-shot fetch of all budgets, for imperative callers that do not want the shared signal. */
  getAllBudgets(): Observable<BudgetResponse[]> {
    return this.http.get<BudgetResponse[]>(`${this.baseUrl}/budget`, { withCredentials: true });
  }

  /**
   * Get budget by ID (from list)
   */
  getBudgetById(id: string): Observable<BudgetResponse | undefined> {
    return this.getAllBudgets().pipe(
      map(budgets => budgets.find(b => b.id === id))
    );
  }

  private reloadBudgets(): void {
    this.budgetsResource.reload();
  }

  /**
   * Get all available owners
   */
  getOwners(): Observable<Owner[]> {
    return this.http.get<Owner[]>(`${this.baseUrl}/owners`, { withCredentials: true });
  }

  /**
   * Register a new budget
   */
  createBudget(request: RegisterBudgetRequest): Observable<BudgetResponse> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.post<BudgetResponse>(`${this.baseUrl}/budget`, request, { 
      headers, 
      withCredentials: true 
    }).pipe(tap(() => this.reloadBudgets()));
  }

  /**
   * Update an existing budget
   */
  updateBudget(id: string, request: UpdateBudgetRequest): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.put<void>(`${this.baseUrl}/budget/${id}`, request, { 
      headers,
      withCredentials: true 
    }).pipe(tap(() => this.reloadBudgets()));
  }

  /**
   * Change budget owners
   */
  changeBudgetOwners(request: ChangeBudgetOwnersRequest): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.put<void>(`${this.baseUrl}/budget/owners`, request, { 
      headers,
      withCredentials: true 
    }).pipe(tap(() => this.reloadBudgets()));
  }

  /**
   * Remove a budget
   */
  removeBudget(id: string, version: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/budget/${id}?version=${encodeURIComponent(version)}`, { 
      withCredentials: true 
    }).pipe(tap(() => this.reloadBudgets()));
  }

  /**
   * Merge multiple budgets
   */
  mergeBudgets(request: MergeBudgetsRequest): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.post<void>(`${this.baseUrl}/budget/merge`, request, { 
      headers,
      withCredentials: true 
    }).pipe(tap(() => this.reloadBudgets()));
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
    }).pipe(tap(() => this.reloadBudgets()));
  }

  downloadTaggingCriteriaYaml(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/budget/${id}/criteria/tagging`, {
      responseType: 'blob',
      headers: new HttpHeaders().set('Accept', 'application/yaml'),
      withCredentials: true
    });
  }

  uploadTaggingCriteriaYaml(id: string, yamlContent: string): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/yaml');
    return this.http.put<void>(`${this.baseUrl}/budget/${id}/criteria/tagging`, yamlContent, {
      headers,
      withCredentials: true
    }).pipe(tap(() => this.reloadBudgets()));
  }

  updateTaggingCriteria(id: string, request: UpdateTaggingCriteriaRequest): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.put<void>(`${this.baseUrl}/budget/${id}/criteria/tagging`, request, {
      headers,
      withCredentials: true
    }).pipe(tap(() => this.reloadBudgets()));
  }

  downloadTransferCriteriaYaml(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/budget/${id}/criteria/transfers`, {
      responseType: 'blob',
      headers: new HttpHeaders().set('Accept', 'application/yaml'),
      withCredentials: true
    });
  }

  uploadTransferCriteriaYaml(id: string, yamlContent: string): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/yaml');
    return this.http.put<void>(`${this.baseUrl}/budget/${id}/criteria/transfers`, yamlContent, {
      headers,
      withCredentials: true
    }).pipe(tap(() => this.reloadBudgets()));
  }

  updateTransferCriteria(id: string, request: UpdateTransferCriteriaRequest): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.put<void>(`${this.baseUrl}/budget/${id}/criteria/transfers`, request, {
      headers,
      withCredentials: true
    }).pipe(tap(() => this.reloadBudgets()));
  }

  downloadLogbookCriterionYaml(id: string, name: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/budget/${id}/criteria/logbook/${encodeURIComponent(name)}`, {
      responseType: 'blob',
      headers: new HttpHeaders().set('Accept', 'application/yaml'),
      withCredentials: true
    });
  }

  uploadLogbookCriterionYaml(id: string, name: string, yamlContent: string): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/yaml');
    return this.http.put<void>(`${this.baseUrl}/budget/${id}/criteria/logbook/${encodeURIComponent(name)}`, yamlContent, {
      headers,
      withCredentials: true
    }).pipe(tap(() => this.reloadBudgets()));
  }

  updateLogbookCriterion(id: string, name: string, request: UpdateLogbookCriteriaRequest): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.put<void>(`${this.baseUrl}/budget/${id}/criteria/logbook/${encodeURIComponent(name)}`, request, {
      headers,
      withCredentials: true
    }).pipe(tap(() => this.reloadBudgets()));
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