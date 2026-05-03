import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { Subject } from 'rxjs';
import { BudgetApiService } from '../budget/budget-api.service';
import { BudgetResponse, BudgetPlanResponse } from '../budget/models';
import { BudgetPlanEditorComponent } from './budget-plan-editor.component';
import { BudgetPlanApiService } from './budget-plan-api.service';

describe('BudgetPlanEditorComponent', () => {
  let fixture: ComponentFixture<BudgetPlanEditorComponent>;
  let component: BudgetPlanEditorComponent;
  let budget$: Subject<BudgetResponse | undefined>;
  let plan$: Subject<BudgetPlanResponse>;

  beforeEach(async () => {
    budget$ = new Subject<BudgetResponse | undefined>();
    plan$ = new Subject<BudgetPlanResponse>();

    await TestBed.configureTestingModule({
      imports: [BudgetPlanEditorComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              params: {
                budgetId: 'budget-1',
                planId: 'plan-1'
              }
            }
          }
        },
        {
          provide: BudgetApiService,
          useValue: {
            getBudgetById: () => budget$.asObservable()
          }
        },
        {
          provide: BudgetPlanApiService,
          useValue: {
            getPlan: () => plan$.asObservable()
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BudgetPlanEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should not overwrite edited plan criteria when budget criteria load later', () => {
    plan$.next(plan('Saved Criteria'));
    budget$.next(budget(['First Criteria', 'Saved Criteria']));

    expect(component.selectedCriteriaDescription).toBe('Saved Criteria');
  });

  function budget(criteria: string[]): BudgetResponse {
    return {
      id: 'budget-1',
      name: 'Budget',
      version: 'v1',
      owners: [],
      taggingCriteria: [],
      transferCriteria: [],
      logbookCriteria: criteria.map(description => ({ description, isUniversal: true }))
    };
  }

  function plan(criteria: string): BudgetPlanResponse {
    return {
      id: 'plan-1',
      budgetId: 'budget-1',
      name: 'Plan',
      version: 'v1',
      from: '2026-01-01T00:00:00.000Z',
      till: '2026-02-01T00:00:00.000Z',
      logbookCriteria: {
        description: criteria,
        isUniversal: true
      },
      currencyCode: 'RUB',
      expectedAmount: {
        value: 0,
        currencyCode: 'RUB'
      },
      expectations: []
    };
  }
});
