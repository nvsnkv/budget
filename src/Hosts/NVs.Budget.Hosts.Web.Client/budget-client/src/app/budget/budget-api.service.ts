import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BudgetResponse, CreateBudgetRequest, UpdateBudgetRequest } from './models'; // Импортируйте модели, соответствующие схеме OpenAPI
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BudgetApiService {
  private baseUrl = environment.apiUrl + '/api/v0.1'; // Ваш базовый URL для запросов

  constructor(private http: HttpClient) {}

  /**
   * Получение списка бюджетов
   */
  getAllBudgets(): Observable<BudgetResponse[]> {
    return this.http.get<BudgetResponse[]>(`${this.baseUrl}/budget`, { withCredentials: true });
  }

  /**
   * Создание нового бюджета
   */
  createBudget(request: CreateBudgetRequest): Observable<BudgetResponse> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.post<BudgetResponse>(`${this.baseUrl}/budget`, request, { headers, withCredentials: true });
  }

  /**
   * Обновление существующего бюджета
   */
  updateBudget(id: string, request: UpdateBudgetRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/budget/${id}`, request, { withCredentials: true });
  }

  /**
   * Получение конкретного бюджета по ID
   */
  getBudgetById(id: string): Observable<BudgetResponse> {
    return this.http.get<BudgetResponse>(`${this.baseUrl}/budget/${id}`, { withCredentials: true });
  }
}