import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, startWith, switchMap, tap } from 'rxjs';
import { BudgetResponse, CreateBudgetRequest, UpdateBudgetRequest } from './models'; // Импортируйте модели, соответствующие схеме OpenAPI
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BudgetApiService {
  public readonly baseUrl = environment.apiUrl + '/api/v0.1'; // Ваш базовый URL для запросов
  private refresh$ = new BehaviorSubject<boolean>(false);

  constructor(private http: HttpClient) {}

  /**
   * Получение списка бюджетов
   */
  getAllBudgets(): Observable<BudgetResponse[]> {
    return this.refresh$.pipe(
      startWith(undefined), // Инициируем первый запрос сразу
      switchMap(() => 
        this.http.get<BudgetResponse[]>(`${this.baseUrl}/budget`, { withCredentials: true })
      ));
  }

  /**
   * Создание нового бюджета
   */
  createBudget(request: CreateBudgetRequest): Observable<BudgetResponse> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.post<BudgetResponse>(`${this.baseUrl}/budget`, request, { headers, withCredentials: true })
    .pipe(tap(() => this.refresh$.next(true)));
  }

  /**
   * Обновление существующего бюджета
   */
  updateBudget(id: string, request: UpdateBudgetRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/budget/${id}`, request, { withCredentials: true }).pipe(tap(() => this.refresh$.next(true)));;
  }

  /**
   * Получение конкретного бюджета по ID
   */
  getBudgetById(id: string): Observable<BudgetResponse> {
    return this.http.get<BudgetResponse>(`${this.baseUrl}/budget/${id}`, { withCredentials: true });
  }
}