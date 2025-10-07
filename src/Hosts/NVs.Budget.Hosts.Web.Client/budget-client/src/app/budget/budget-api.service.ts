import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, startWith, switchMap, tap } from 'rxjs';
import { 
  BudgetResponse, 
  RegisterBudgetRequest, 
  UpdateBudgetRequest, 
  ChangeBudgetOwnersRequest, 
  MergeBudgetsRequest,
  IError
} from './models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BudgetApiService {
  public readonly baseUrl = environment.apiUrl + '/api/v0.1';
  private refresh$ = new BehaviorSubject<boolean>(false);

  constructor(private http: HttpClient) {}

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
   * Upload budget configuration from YAML file
   */
  uploadBudgetYaml(id: string, file: File): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/yaml');
    return this.http.put<void>(`${this.baseUrl}/budget/${id}`, file, {
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
}