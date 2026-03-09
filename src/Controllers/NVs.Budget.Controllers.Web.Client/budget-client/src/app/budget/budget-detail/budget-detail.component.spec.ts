import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { BudgetDetailComponent } from './budget-detail.component';
import { BudgetApiService } from '../budget-api.service';

describe('BudgetDetailComponent', () => {
  let component: BudgetDetailComponent;
  let fixture: ComponentFixture<BudgetDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BudgetDetailComponent],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { params: of({ budgetId: 'test-id' }) } },
        {
          provide: BudgetApiService,
          useValue: {
            getBudgetById: () => of(undefined),
            getOwners: () => of([]),
            updateBudget: () => of(void 0),
            removeBudget: () => of(void 0),
            changeBudgetOwners: () => of(void 0),
            downloadBudgetYaml: () => of(new Blob()),
            uploadBudgetYaml: () => of(void 0),
            downloadTaggingCriteriaYaml: () => of(new Blob()),
            uploadTaggingCriteriaYaml: () => of(void 0),
            downloadTransferCriteriaYaml: () => of(new Blob()),
            uploadTransferCriteriaYaml: () => of(void 0),
            downloadLogbookCriterionYaml: () => of(new Blob()),
            uploadLogbookCriterionYaml: () => of(void 0)
          }
        }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BudgetDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

