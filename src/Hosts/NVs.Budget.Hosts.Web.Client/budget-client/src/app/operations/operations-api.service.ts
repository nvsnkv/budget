import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, startWith, switchMap } from 'rxjs';
import { 
  OperationResponse,
  UpdateOperationsRequest,
  RemoveOperationsRequest,
  ImportResultResponse,
  UpdateResultResponse
} from '../budget/models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OperationsApiService {
  public readonly baseUrl = environment.apiUrl + '/api/v0.1';
  private refresh$ = new BehaviorSubject<string | null>(null);

  constructor(private http: HttpClient) {}

  /**
   * Get all operations for a specific budget
   */
  getOperations(
    budgetId: string,
    criteria?: string,
    outputCurrency?: string,
    excludeTransfers: boolean = false
  ): Observable<OperationResponse[]> {
    return this.refresh$.pipe(
      startWith(undefined),
      switchMap(() => {
        let url = `${this.baseUrl}/budget/${budgetId}/operations`;
        const params = new URLSearchParams();
        
        if (criteria) {
          params.append('criteria', criteria);
        }
        if (outputCurrency) {
          params.append('outputCurrency', outputCurrency);
        }
        if (excludeTransfers) {
          params.append('excludeTransfers', 'true');
        }

        const queryString = params.toString();
        if (queryString) {
          url += `?${queryString}`;
        }

        return this.http.get<OperationResponse[]>(url, { withCredentials: true });
      })
    );
  }

  /**
   * Import operations into a budget from CSV file
   */
  importOperations(
    budgetId: string, 
    file: File, 
    budgetVersion: string,
    transferConfidenceLevel?: string,
    filePattern?: string
  ): Observable<ImportResultResponse> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('budgetVersion', budgetVersion);
    if (transferConfidenceLevel) {
      formData.append('transferConfidenceLevel', transferConfidenceLevel);
    }
    if (filePattern) {
      formData.append('filePattern', filePattern);
    }

    return this.http.post<ImportResultResponse>(
      `${this.baseUrl}/budget/${budgetId}/operations/import`,
      formData,
      { withCredentials: true }
    );
  }

  /**
   * Update existing operations
   */
  updateOperations(budgetId: string, request: UpdateOperationsRequest): Observable<UpdateResultResponse> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.put<UpdateResultResponse>(
      `${this.baseUrl}/budget/${budgetId}/operations`,
      request,
      { headers, withCredentials: true }
    );
  }

  /**
   * Remove operations matching criteria
   */
  removeOperations(budgetId: string, request: RemoveOperationsRequest): Observable<void> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.request<void>(
      'DELETE',
      `${this.baseUrl}/budget/${budgetId}/operations`,
      { 
        headers,
        body: request,
        withCredentials: true 
      }
    );
  }

  /**
   * Trigger refresh for operations list
   */
  triggerRefresh(budgetId: string): void {
    this.refresh$.next(budgetId);
  }
}

