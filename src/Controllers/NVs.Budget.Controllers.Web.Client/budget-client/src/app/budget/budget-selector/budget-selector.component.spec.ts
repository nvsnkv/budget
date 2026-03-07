import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { BudgetSelectorComponent } from './budget-selector.component';
import { BudgetApiService } from '../budget-api.service';

describe('BudgetSelectorComponent', () => {
  let component: BudgetSelectorComponent;
  let fixture: ComponentFixture<BudgetSelectorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BudgetSelectorComponent],
      providers: [
        provideRouter([]),
        {
          provide: BudgetApiService,
          useValue: {
            getAllBudgets: () => of([])
          }
        }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BudgetSelectorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
