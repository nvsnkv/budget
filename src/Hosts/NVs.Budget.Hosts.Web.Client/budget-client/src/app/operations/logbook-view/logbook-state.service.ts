import { Injectable } from '@angular/core';

interface LogbookState {
  expandedRows: Set<string>;
  scrollPosition: number;
}

@Injectable({
  providedIn: 'root'
})
export class LogbookStateService {
  private stateMap = new Map<string, LogbookState>();

  saveState(budgetId: string, expandedRows: Set<string>, scrollPosition: number): void {
    this.stateMap.set(budgetId, {
      expandedRows: new Set(expandedRows),
      scrollPosition
    });
  }

  getState(budgetId: string): LogbookState | undefined {
    return this.stateMap.get(budgetId);
  }

  clearState(budgetId: string): void {
    this.stateMap.delete(budgetId);
  }

  hasState(budgetId: string): boolean {
    return this.stateMap.has(budgetId);
  }
}

