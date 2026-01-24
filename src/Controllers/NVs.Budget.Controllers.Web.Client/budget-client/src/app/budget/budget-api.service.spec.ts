import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { BudgetApiService } from './budget-api.service';
import { BudgetResponse, CreateBudgetRequest, UpdateBudgetRequest } from './models';

describe('BudgetApiService', () => {
  let service: BudgetApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [BudgetApiService, provideHttpClientTesting()]
    });

    service = TestBed.inject(BudgetApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Проверяем, что все ожидаемые запросы были выполнены
  });

  it('should get all budgets', () => {
    const mockData: BudgetResponse[] = [
      { id: '123', name: 'Test Budget' },
      { id: '456', name: 'Another Budget' }
    ];

    service.getAllBudgets().subscribe((data: BudgetResponse[]) => {
      expect(data).toEqual(mockData); // Проверка, что возвращаемые данные совпадают с ожидаемыми
    });

    const req = httpMock.expectOne({ method: 'GET', url: '/api/v0.1/Budget' });
    expect(req.request.withCredentials).toBe(true); // Проверяем, что с запросом отправляются куки
    req.flush(mockData); // Эмулируем успешный ответ сервера
  });

  it('should create a new budget', () => {
    const request: CreateBudgetRequest = { name: 'New Budget' };
    const expectedResponse: BudgetResponse = { id: '789', name: 'New Budget' };

    service.createBudget(request).subscribe((response: BudgetResponse) => {
      expect(response).toEqual(expectedResponse);
    });

    const req = httpMock.expectOne({ method: 'POST', url: '/api/v0.1/Budget' });
    expect(req.request.body).toEqual(request); // Проверяем, что тело запроса совпадает с ожидаемым
    expect(req.request.headers.has('Content-Type')).toBeTruthy(); // Проверяем наличие заголовка Content-Type
    expect(req.request.withCredentials).toBe(true); // Проверяем, что с запросом отправляются куки
    req.flush(expectedResponse); // Эмулируем успешный ответ сервера
  });

  it('should update an existing budget', () => {
    const id = '123';
    const request: UpdateBudgetRequest = { id, name: 'Updated Budget' };

    service.updateBudget(id, request).subscribe(() => {});

    const req = httpMock.expectOne({ method: 'PUT', url: `/api/v0.1/Budget/${id}` });
    expect(req.request.body).toEqual(request); // Проверяем, что тело запроса совпадает с ожидаемым
    expect(req.request.withCredentials).toBe(true); // Проверяем, что с запросом отправляются куки
    req.flush(null); // Эмулируем успешный ответ сервера (без тела)
  });

  it('should get a specific budget by id', () => {
    const id = '123';
    const mockData: BudgetResponse = { id: '123', name: 'Test Budget' };

    service.getBudgetById(id).subscribe((data) => {
      expect(data).toEqual(mockData); // Проверка, что возвращаемые данные совпадают с ожидаемыми
    });

    const req = httpMock.expectOne({ method: 'GET', url: `/api/v0.1/Budget/${id}` });
    expect(req.request.withCredentials).toBe(true); // Проверяем, что с запросом отправляются куки
    req.flush(mockData); // Эмулируем успешный ответ сервера
  });
});