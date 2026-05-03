import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AppConfigService } from '../config/app-config.service';
import { BudgetPlanApiService } from './budget-plan-api.service';
import { BudgetPlanResponse, UpsertBudgetPlanRequest, VarianceReportResponse } from '../budget/models';

describe('BudgetPlanApiService', () => {
  let service: BudgetPlanApiService;
  let httpMock: HttpTestingController;
  const appConfigStub = { apiUrl: 'https://api.test' };
  const baseUrl = `${appConfigStub.apiUrl}/api/v0.1/budget/budget-1/plans`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        BudgetPlanApiService,
        { provide: AppConfigService, useValue: appConfigStub }
      ]
    });

    service = TestBed.inject(BudgetPlanApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should list budget plans', () => {
    const response = [plan()];

    service.listPlans('budget-1').subscribe(value => expect(value).toEqual(response));

    const req = httpMock.expectOne({ method: 'GET', url: baseUrl });
    expect(req.request.withCredentials).toBe(true);
    req.flush(response);
  });

  it('should create a budget plan', () => {
    const request: UpsertBudgetPlanRequest = {
      name: 'Plan',
      from: '2026-01-01T00:00:00.000Z',
      till: '2026-02-01T00:00:00.000Z',
      logbookCriteria: { description: 'All', isUniversal: true },
      currencyCode: 'RUB'
    };

    service.createPlan('budget-1', request).subscribe(value => expect(value).toEqual(plan()));

    const req = httpMock.expectOne({ method: 'POST', url: baseUrl });
    expect(req.request.body).toEqual(request);
    expect(req.request.withCredentials).toBe(true);
    req.flush(plan());
  });

  it('should send upsert timestamps as UTC ISO strings with Z suffix', () => {
    const request: UpsertBudgetPlanRequest = {
      name: 'Plan',
      from: '2031-11-07T06:41',
      till: '2031-12-07T07:06',
      logbookCriteria: { description: 'All', isUniversal: true },
      currencyCode: 'RUB',
      expectations: [
        {
          expectedAmount: { value: -10, currencyCode: 'RUB' },
          from: '2031-11-07T06:41',
          till: '2031-12-07T07:06'
        }
      ]
    };

    service.createPlan('budget-1', request).subscribe();

    const req = httpMock.expectOne({ method: 'POST', url: baseUrl });
    const body = req.request.body as UpsertBudgetPlanRequest;
    expect(body.from).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/);
    expect(body.till).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/);
    expect(body.expectations![0].from).toMatch(/Z$/);
    expect(body.expectations![0].till).toMatch(/Z$/);
    req.flush(plan());
  });

  it('should load variance report', () => {
    const response: VarianceReportResponse = {
      plan: plan(),
      variances: []
    };

    service.getVariance('budget-1', 'plan-1').subscribe(value => expect(value).toEqual(response));

    const req = httpMock.expectOne({ method: 'GET', url: `${baseUrl}/plan-1/variance` });
    expect(req.request.withCredentials).toBe(true);
    req.flush(response);
  });

  function plan(): BudgetPlanResponse {
    return {
      id: 'plan-1',
      budgetId: 'budget-1',
      name: 'Plan',
      version: 'v1',
      from: '2026-01-01T00:00:00.000Z',
      till: '2026-02-01T00:00:00.000Z',
      logbookCriteria: { description: 'All', isUniversal: true },
      currencyCode: 'RUB',
      expectedAmount: { value: 0, currencyCode: 'RUB' },
      expectations: []
    };
  }
});
