import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { ImportResultViewComponent } from './import-result-view.component';
import { OperationsApiService } from '../../../operations-api.service';
import { OperationsHelperService } from '../../operations-helper.service';
import { NotificationService } from '../../notification.service';
import { DeleteResultResponse, ImportResultResponse, OperationResponse, TransferResponse, UpdateResultResponse } from '../../../../budget/models';

describe('ImportResultViewComponent', () => {
  let component: ImportResultViewComponent;
  let fixture: ComponentFixture<ImportResultViewComponent>;
  let operationsApi: {
    registerTransfers: ReturnType<typeof vi.fn>;
    removeTransfers: ReturnType<typeof vi.fn>;
    triggerRefresh: ReturnType<typeof vi.fn>;
  };
  let operationsHelper: {
    updateOperations: ReturnType<typeof vi.fn>;
    updateOperation: ReturnType<typeof vi.fn>;
    deleteOperations: ReturnType<typeof vi.fn>;
  };

  const operation = (id: string, description: string): OperationResponse => ({
    id,
    version: 'v1',
    timestamp: '2026-01-10T11:30:00Z',
    amount: { value: -100, currencyCode: 'RUB' },
    description,
    notes: '',
    budgetId: 'budget-id',
    tags: []
  });

  const transfer = (sourceId: string, sinkId: string): TransferResponse => ({
    sourceId,
    source: operation(sourceId, 'Source operation'),
    sinkId,
    sink: operation(sinkId, 'Sink operation'),
    fee: { value: 0, currencyCode: 'RUB' },
    comment: 'Test transfer',
    accuracy: 'Likely'
  });

  const importResult = (): ImportResultResponse => ({
    registeredOperations: [operation('op-1', 'Salary'), operation('op-2', 'Coffee')],
    duplicates: [[operation('op-3', 'Dup 1'), operation('op-4', 'Dup 2')]],
    errors: [],
    successes: [],
    registeredTransfers: [transfer('op-1', 'op-2')],
    unregisteredTransfers: [transfer('op-3', 'op-4')]
  });

  beforeEach(async () => {
    operationsApi = {
      registerTransfers: vi.fn().mockReturnValue(of(void 0)),
      removeTransfers: vi.fn().mockReturnValue(of(void 0)),
      triggerRefresh: vi.fn()
    };
    operationsHelper = {
      updateOperations: vi.fn().mockReturnValue(of({ updatedOperations: [], errors: [], successes: [] } as UpdateResultResponse)),
      updateOperation: vi.fn().mockReturnValue(of({ updatedOperations: [], errors: [], successes: [] } as UpdateResultResponse)),
      deleteOperations: vi.fn().mockReturnValue(of({ errors: [], successes: [] } as DeleteResultResponse))
    };

    await TestBed.configureTestingModule({
      imports: [ImportResultViewComponent],
      providers: [
        { provide: OperationsApiService, useValue: operationsApi },
        { provide: OperationsHelperService, useValue: operationsHelper },
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

    fixture = TestBed.createComponent(ImportResultViewComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.componentRef.setInput('budgetId', 'budget-id');
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should expose imported operations and transfers from the result', () => {
    fixture.componentRef.setInput('budgetId', 'budget-id');
    fixture.componentRef.setInput('result', importResult());
    fixture.detectChanges();

    expect(component.registeredOperations()).toHaveLength(2);
    expect(component.registeredTransfers()).toHaveLength(1);
    expect(component.unregisteredTransfers()).toHaveLength(1);
    expect(component.duplicatesList()).toHaveLength(1);
  });

  it('should reset state when a new result arrives', () => {
    fixture.componentRef.setInput('budgetId', 'budget-id');
    fixture.componentRef.setInput('result', importResult());
    fixture.detectChanges();

    fixture.componentRef.setInput('result', null);
    fixture.detectChanges();

    expect(component.registeredOperations()).toHaveLength(0);
    expect(component.unregisteredTransfers()).toHaveLength(0);
    expect(component.hasTransfers()).toBe(false);
  });

  it('should move transfer from possible to registered after registration', () => {
    fixture.componentRef.setInput('budgetId', 'budget-id');
    fixture.componentRef.setInput('result', importResult());
    fixture.detectChanges();

    const possible = component.unregisteredTransfers()[0];
    component.onTransferRegistered(possible);

    expect(operationsApi.registerTransfers).toHaveBeenCalledWith('budget-id', {
      transfers: [{
        sourceId: possible.sourceId,
        sinkId: possible.sinkId,
        comment: possible.comment,
        accuracy: possible.accuracy,
        fee: possible.fee
      }]
    });
    expect(component.unregisteredTransfers()).toHaveLength(0);
    expect(component.registeredTransfers()).toHaveLength(2);
    expect(operationsApi.triggerRefresh).toHaveBeenCalledWith('budget-id');
  });

  it('should move transfer back to possible after unregistration', () => {
    fixture.componentRef.setInput('budgetId', 'budget-id');
    fixture.componentRef.setInput('result', importResult());
    fixture.detectChanges();

    vi.spyOn(window, 'confirm').mockReturnValue(true);
    const registered = component.registeredTransfers()[0];
    component.onTransferDeleted(registered);

    expect(operationsApi.removeTransfers).toHaveBeenCalledWith('budget-id', {
      sourceIds: [registered.sourceId],
      all: false
    });
    expect(component.registeredTransfers()).toHaveLength(0);
    expect(component.unregisteredTransfers()).toHaveLength(2);
  });

  it('should remove deleted operations from the list', () => {
    fixture.componentRef.setInput('budgetId', 'budget-id');
    fixture.componentRef.setInput('result', importResult());
    fixture.detectChanges();

    vi.spyOn(window, 'confirm').mockReturnValue(true);
    component.onDeleteOperations([component.registeredOperations()[0]]);

    expect(operationsHelper.deleteOperations).toHaveBeenCalledWith('budget-id', ['op-1']);
    expect(component.registeredOperations().map(op => op.id)).toEqual(['op-2']);
    expect(operationsApi.triggerRefresh).toHaveBeenCalledWith('budget-id');
  });

  it('should delegate operation updates to the helper service', () => {
    fixture.componentRef.setInput('budgetId', 'budget-id');
    fixture.componentRef.setInput('result', importResult());
    fixture.detectChanges();

    const updated = component.registeredOperations()[0];
    component.onUpdateOperations([updated]);

    expect(operationsHelper.updateOperations).toHaveBeenCalledWith('budget-id', [updated]);
    expect(component.registeredOperations()).toHaveLength(2);
  });
});
