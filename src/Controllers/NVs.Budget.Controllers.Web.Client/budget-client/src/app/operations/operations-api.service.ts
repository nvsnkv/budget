import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, startWith, switchMap } from 'rxjs';
import { 
  OperationResponse,
  UpdateOperationsRequest,
  RemoveOperationsRequest,
  RetagOperationsRequest,
  ImportResultResponse,
  UpdateResultResponse,
  DeleteResultResponse,
  RetagResultResponse,
  LogbookResponse,
  TransfersListResponse,
  RegisterTransfersRequest,
  RemoveTransfersRequest
} from '../budget/models';
import { AppConfigService } from '../config/app-config.service';

@Injectable({
  providedIn: 'root'
})
export class OperationsApiService {
  public readonly baseUrl: string;
  private refresh$ = new BehaviorSubject<string | null>(null);
  
  private static readonly jsonHeaders = new HttpHeaders().set('Content-Type', 'application/json');

  constructor(
    private http: HttpClient,
    private configService: AppConfigService
  ) {
    this.baseUrl = this.configService.apiUrl + '/api/v0.1';
  }

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
    return this.http.put<UpdateResultResponse>(
      `${this.baseUrl}/budget/${budgetId}/operations`,
      request,
      { headers: OperationsApiService.jsonHeaders, withCredentials: true }
    );
  }

  /**
   * Remove operations matching criteria
   */
  removeOperations(budgetId: string, request: RemoveOperationsRequest): Observable<DeleteResultResponse> {
    return this.http.request<DeleteResultResponse>(
      'DELETE',
      `${this.baseUrl}/budget/${budgetId}/operations`,
      { 
        headers: OperationsApiService.jsonHeaders,
        body: request,
        withCredentials: true 
      }
    );
  }

  /**
   * Retag operations matching criteria
   */
  retagOperations(budgetId: string, request: RetagOperationsRequest): Observable<RetagResultResponse> {
    return this.http.post<RetagResultResponse>(
      `${this.baseUrl}/budget/${budgetId}/operations/retag`,
      request,
      { headers: OperationsApiService.jsonHeaders, withCredentials: true }
    );
  }

  /**
   * Trigger refresh for operations list
   */
  triggerRefresh(budgetId: string): void {
    this.refresh$.next(budgetId);
  }

  /**
   * Get duplicate operations
   */
  getDuplicates(budgetId: string, criteria?: string): Observable<OperationResponse[][]> {
    const params: any = {};
    if (criteria) {
      params.criteria = criteria;
    }
    return this.http.get<OperationResponse[][]>(
      `${this.baseUrl}/budget/${budgetId}/operations/duplicates`,
      { params, withCredentials: true }
    );
  }

  /**
   * Get logbook (aggregated operations statistics)
   */
  getLogbook(
    budgetId: string,
    logbookId: string,
    from?: Date,
    till?: Date,
    criteria?: string,
    cronExpression?: string,
    outputCurrency?: string
  ): Observable<LogbookResponse> {
    const params: any = {};
    if (from) {
      params.from = from.toISOString();
    }
    if (till) {
      params.till = till.toISOString();
    }
    params.logbookId = logbookId;
    if (criteria) {
      params.criteria = criteria;
    }
    if (cronExpression) {
      params.cronExpression = cronExpression;
    }
    if (outputCurrency) {
      params.outputCurrency = outputCurrency;
    }
    return this.http.get<LogbookResponse>(
      `${this.baseUrl}/budget/${budgetId}/operations/logbook`,
      { params, withCredentials: true }
    );
  }

  /**
   * Search transfers in a budget
   */
  searchTransfers(
    budgetId: string,
    from?: Date,
    till?: Date,
    accuracy?: string
  ): Observable<TransfersListResponse> {
    const params: any = {};
    if (from) {
      params.from = from.toISOString();
    }
    if (till) {
      params.till = till.toISOString();
    }
    if (accuracy) {
      params.accuracy = accuracy;
    }
    return this.http.get<TransfersListResponse>(
      `${this.baseUrl}/budget/${budgetId}/transfers`,
      { params, withCredentials: true }
    );
  }

  /**
   * Register new transfers
   */
  registerTransfers(
    budgetId: string,
    request: RegisterTransfersRequest
  ): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/budget/${budgetId}/transfers`,
      request,
      { headers: OperationsApiService.jsonHeaders, withCredentials: true }
    );
  }

  /**
   * Remove transfers
   */
  removeTransfers(
    budgetId: string,
    request: RemoveTransfersRequest
  ): Observable<void> {
    return this.http.request<void>(
      'DELETE',
      `${this.baseUrl}/budget/${budgetId}/transfers`,
      {
        headers: OperationsApiService.jsonHeaders,
        body: request,
        withCredentials: true
      }
    );
  }
}

