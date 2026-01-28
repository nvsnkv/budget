import { Injectable } from '@angular/core';
import { Observable, switchMap, throwError } from 'rxjs';
import { OperationsApiService } from '../operations-api.service';
import { BudgetApiService } from '../../budget/budget-api.service';
import { OperationResponse, BudgetResponse, UpdateResultResponse } from '../../budget/models';

@Injectable({
  providedIn: 'root'
})
export class OperationsHelperService {
  constructor(
    private operationsApi: OperationsApiService,
    private budgetApi: BudgetApiService
  ) {}

  /**
   * Delete a single operation by ID
   */
  deleteOperation(budgetId: string, operationId: string): Observable<any> {
    const criteria = `o => o.Id == Guid.Parse("${operationId}")`;
    return this.operationsApi.removeOperations(budgetId, { criteria });
  }

  /**
   * Update a single operation
   */
  updateOperation(budgetId: string, operation: OperationResponse): Observable<UpdateResultResponse> {
    return this.updateOperations(budgetId, [operation]);
  }

  /**
   * Update multiple operations
   */
  updateOperations(budgetId: string, operations: OperationResponse[]): Observable<UpdateResultResponse> {
    return this.budgetApi.getAllBudgets().pipe(
      switchMap((budgetList: BudgetResponse[]) => {
        const budget = budgetList.find((b: BudgetResponse) => b.id === budgetId);
        
        if (!budget) {
          return throwError(() => new Error('Budget not found'));
        }

        const request = {
          operations,
          budgetVersion: budget.version,
          transferConfidenceLevel: undefined,
          taggingMode: 'Skip'
        };

        return this.operationsApi.updateOperations(budgetId, request);
      })
    );
  }
}

