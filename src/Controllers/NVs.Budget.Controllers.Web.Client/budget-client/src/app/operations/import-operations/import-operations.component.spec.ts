import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { provideRouter } from '@angular/router';
import { By } from '@angular/platform-browser';
import { of } from 'rxjs';
import { provideTaiga } from '@taiga-ui/core';

import { ImportOperationsComponent } from './import-operations.component';
import { OperationsApiService } from '../operations-api.service';
import { BudgetApiService } from '../../budget/budget-api.service';
import { NotificationService } from '../shared/notification.service';
import { BudgetResponse } from '../../budget/models';

describe('ImportOperationsComponent', () => {
  let component: ImportOperationsComponent;
  let fixture: ComponentFixture<ImportOperationsComponent>;

  const budget: BudgetResponse = {
    id: 'b-1',
    name: 'Test budget',
    version: 'v1',
    owners: [],
    taggingCriteria: [],
    transferCriteria: [],
    logbookCriteria: []
  };

  beforeAll(() => {
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      value: (query: string) => ({
        matches: false,
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      }),
    });
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportOperationsComponent],
      providers: [
        provideTaiga(),
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { params: { budgetId: 'b-1' } } } },
        { provide: BudgetApiService, useValue: { getBudgetById: () => of(budget) } },
        { provide: OperationsApiService, useValue: { importOperations: vi.fn(), triggerRefresh: vi.fn() } },
        {
          provide: NotificationService,
          useValue: {
            showSuccess: vi.fn().mockReturnValue(of(undefined)),
            showError: vi.fn().mockReturnValue(of(undefined)),
            handleError: vi.fn().mockReturnValue('Error')
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ImportOperationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should disable the import button until a file is selected', () => {
    const importButton = getImportButton();
    expect(importButton.disabled).toBe(true);
  });

  it('should enable the import button once a file is selected', () => {
    component.selectedFile.set(new File(['a,b'], 'statement.csv', { type: 'text/csv' }));
    fixture.detectChanges();

    expect(getImportButton().disabled).toBe(false);
  });

  function getImportButton(): HTMLButtonElement {
    const button = fixture.debugElement
      .queryAll(By.css('button'))
      .map(b => b.nativeElement as HTMLButtonElement)
      .find(b => b.textContent?.trim() === 'Import Operations');

    if (!button) {
      throw new Error('Import Operations button not found');
    }

    return button;
  }
});
