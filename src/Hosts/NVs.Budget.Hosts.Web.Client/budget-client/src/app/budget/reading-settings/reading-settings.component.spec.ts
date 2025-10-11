import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReadingSettingsComponent } from './reading-settings.component';
import { ActivatedRoute } from '@angular/router';
import { BudgetApiService } from '../budget-api.service';
import { of } from 'rxjs';

describe('ReadingSettingsComponent', () => {
  let component: ReadingSettingsComponent;
  let fixture: ComponentFixture<ReadingSettingsComponent>;

  beforeEach(async () => {
    const apiServiceMock = {
      getReadingSettings: jasmine.createSpy('getReadingSettings').and.returnValue(of({ settings: {} })),
      updateReadingSettings: jasmine.createSpy('updateReadingSettings').and.returnValue(of(void 0))
    };

    const activatedRouteMock = {
      params: of({ budgetId: 'test-id' })
    };

    await TestBed.configureTestingModule({
      imports: [ReadingSettingsComponent],
      providers: [
        { provide: BudgetApiService, useValue: apiServiceMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ReadingSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

