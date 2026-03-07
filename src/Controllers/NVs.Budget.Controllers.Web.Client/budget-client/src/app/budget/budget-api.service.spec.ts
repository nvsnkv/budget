import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { BudgetApiService } from './budget-api.service';
import { BudgetResponse, RegisterBudgetRequest, UpdateBudgetRequest } from './models';
import { AppConfigService } from '../config/app-config.service';

describe('BudgetApiService', () => {
  let service: BudgetApiService;
  let httpMock: HttpTestingController;
  const appConfigStub = { apiUrl: 'https://api.test' };
  const baseUrl = `${appConfigStub.apiUrl}/api/v0.1`;

  const makeBudget = (id: string, name: string): BudgetResponse => ({
    id,
    name,
    version: 'v1',
    owners: [],
    taggingCriteria: [],
    transferCriteria: [],
    logbookCriteria: []
  });

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        BudgetApiService,
        { provide: AppConfigService, useValue: appConfigStub }
      ]
    });

    service = TestBed.inject(BudgetApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Проверяем, что все ожидаемые запросы были выполнены
  });

  it('should get all budgets', () => {
    const mockData: BudgetResponse[] = [
      makeBudget('123', 'Test Budget'),
      makeBudget('456', 'Another Budget')
    ];

    service.getAllBudgets().subscribe((data: BudgetResponse[]) => {
      expect(data).toEqual(mockData);
    });

    const requests = httpMock.match((req) => req.method === 'GET' && req.url === `${baseUrl}/budget`);
    expect(requests.length).toBe(2);
    requests.forEach((req) => expect(req.request.withCredentials).toBe(true));
    const activeRequest = requests.find((req) => !req.cancelled);
    expect(activeRequest).toBeDefined();
    activeRequest!.flush(mockData);
  });

  it('should create a new budget', () => {
    const request: RegisterBudgetRequest = { name: 'New Budget' };
    const expectedResponse: BudgetResponse = makeBudget('789', 'New Budget');

    service.createBudget(request).subscribe((response: BudgetResponse) => {
      expect(response).toEqual(expectedResponse);
    });

    const req = httpMock.expectOne({ method: 'POST', url: `${baseUrl}/budget` });
    expect(req.request.body).toEqual(request);
    expect(req.request.headers.has('Content-Type')).toBeTruthy();
    expect(req.request.withCredentials).toBe(true);
    req.flush(expectedResponse);
  });

  it('should update an existing budget', () => {
    const id = '123';
    const request: UpdateBudgetRequest = { name: 'Updated Budget', version: 'v1' };

    service.updateBudget(id, request).subscribe(() => {});

    const req = httpMock.expectOne({ method: 'PUT', url: `${baseUrl}/budget/${id}` });
    expect(req.request.body).toEqual(request);
    expect(req.request.withCredentials).toBe(true);
    req.flush(null);
  });

  it('should get a specific budget by id from budgets list', () => {
    const id = '123';
    const mockData: BudgetResponse[] = [
      makeBudget('123', 'Test Budget'),
      makeBudget('456', 'Another Budget')
    ];

    service.getBudgetById(id).subscribe((data) => {
      expect(data).toEqual(mockData[0]);
    });

    const requests = httpMock.match((req) => req.method === 'GET' && req.url === `${baseUrl}/budget`);
    expect(requests.length).toBe(2);
    requests.forEach((req) => expect(req.request.withCredentials).toBe(true));
    const activeRequest = requests.find((req) => !req.cancelled);
    expect(activeRequest).toBeDefined();
    activeRequest!.flush(mockData);
  });
});