import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfigService } from '../config/app-config.service';
import {
  BudgetPlanResponse,
  CopyBudgetPlanRequest,
  UpsertBudgetPlanRequest,
  UpsertPlanExpectationRequest,
  VarianceReportResponse
} from '../budget/models';

@Injectable({ providedIn: 'root' })
export class BudgetPlanApiService {
  private readonly jsonHeaders = new HttpHeaders().set('Content-Type', 'application/json');
  private readonly baseUrl: string;

  constructor(private http: HttpClient, private configService: AppConfigService) {
    this.baseUrl = `${this.configService.apiUrl}/api/v0.1`;
  }

  listPlans(budgetId: string): Observable<BudgetPlanResponse[]> {
    return this.http.get<BudgetPlanResponse[]>(this.plansUrl(budgetId), { withCredentials: true });
  }

  getPlan(budgetId: string, planId: string): Observable<BudgetPlanResponse> {
    return this.http.get<BudgetPlanResponse>(`${this.plansUrl(budgetId)}/${planId}`, { withCredentials: true });
  }

  createPlan(budgetId: string, request: UpsertBudgetPlanRequest): Observable<BudgetPlanResponse> {
    const body = this.planUpsertJsonBody(request);
    return this.http.post<BudgetPlanResponse>(this.plansUrl(budgetId), body, {
      headers: this.jsonHeaders,
      withCredentials: true
    });
  }

  updatePlan(budgetId: string, planId: string, request: UpsertBudgetPlanRequest): Observable<BudgetPlanResponse> {
    const body = this.planUpsertJsonBody(request);
    return this.http.put<BudgetPlanResponse>(`${this.plansUrl(budgetId)}/${planId}`, body, {
      headers: this.jsonHeaders,
      withCredentials: true
    });
  }

  deletePlan(budgetId: string, planId: string): Observable<void> {
    return this.http.delete<void>(`${this.plansUrl(budgetId)}/${planId}`, { withCredentials: true });
  }

  copyPlan(budgetId: string, planId: string, request: CopyBudgetPlanRequest): Observable<BudgetPlanResponse> {
    return this.http.post<BudgetPlanResponse>(`${this.plansUrl(budgetId)}/${planId}/copy`, request, {
      headers: this.jsonHeaders,
      withCredentials: true
    });
  }

  getVariance(budgetId: string, planId: string): Observable<VarianceReportResponse> {
    return this.http.get<VarianceReportResponse>(`${this.plansUrl(budgetId)}/${planId}/variance`, { withCredentials: true });
  }

  private plansUrl(budgetId: string): string {
    return `${this.baseUrl}/budget/${budgetId}/plans`;
  }

  /**
   * Same as operations logbook transport: instants as ISO-8601 UTC (`Date#toISOString()`).
   * The plan editor binds `datetime-local` strings; conversion stays in this service.
   */
  private planUpsertJsonBody(request: UpsertBudgetPlanRequest): UpsertBudgetPlanRequest {
    const { expectations, from, till, ...rest } = request;
    return {
      ...rest,
      from: new Date(from).toISOString(),
      till: new Date(till).toISOString(),
      ...(expectations !== undefined
        ? { expectations: expectations.map(e => this.expectationJsonTimestamps(e)) }
        : {})
    };
  }

  private expectationJsonTimestamps(e: UpsertPlanExpectationRequest): UpsertPlanExpectationRequest {
    return {
      ...e,
      from: new Date(e.from).toISOString(),
      till: new Date(e.till).toISOString()
    };
  }
}
