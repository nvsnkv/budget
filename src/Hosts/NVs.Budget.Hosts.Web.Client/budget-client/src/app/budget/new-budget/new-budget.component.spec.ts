import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewBudgetComponent } from './new-budget.component';

describe('NewBudgetComponent', () => {
  let component: NewBudgetComponent;
  let fixture: ComponentFixture<NewBudgetComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NewBudgetComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NewBudgetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
