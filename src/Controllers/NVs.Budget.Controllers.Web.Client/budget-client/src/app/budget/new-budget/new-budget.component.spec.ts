import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';

import { NewBudgetComponent } from './new-budget.component';
import { BudgetApiService } from '../budget-api.service';

describe('NewBudgetComponent', () => {
  let component: NewBudgetComponent;
  let fixture: ComponentFixture<NewBudgetComponent>;
  let budgetService: jasmine.SpyObj<BudgetApiService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    budgetService = jasmine.createSpyObj<BudgetApiService>('BudgetApiService', ['createBudget']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [NewBudgetComponent],
      providers: [
        { provide: BudgetApiService, useValue: budgetService },
        { provide: Router, useValue: router }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NewBudgetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should send generateDemoBudget=true when checkbox is selected', () => {
    budgetService.createBudget.and.returnValue(of({
      id: 'budget-id',
      name: 'Demo',
      version: 'v1',
      owners: [],
      taggingCriteria: [],
      transferCriteria: [],
      logbookCriteria: []
    }));

    component.nameGroup.controls.name.setValue('Demo');
    component.nameGroup.controls.generateDemoBudget.setValue(true);
    component.onSubmit();

    expect(budgetService.createBudget).toHaveBeenCalledWith({
      name: 'Demo',
      generateDemoBudget: true
    });
    expect(router.navigate).toHaveBeenCalledWith(['/budget', 'budget-id', 'operations']);
  });

  it('should show error if create budget fails', () => {
    budgetService.createBudget.and.returnValue(throwError(() => ({
      status: 400,
      error: [{ message: 'Validation failed' }]
    })));

    component.nameGroup.controls.name.setValue('Demo');
    component.onSubmit();

    expect(component.errorMessage).toContain('Validation failed');
  });
});
