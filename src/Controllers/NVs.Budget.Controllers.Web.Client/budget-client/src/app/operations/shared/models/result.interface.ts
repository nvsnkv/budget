import { IError, ISuccess } from '../../../budget/models';

export interface OperationResult {
  errors: IError[];
  successes: ISuccess[];
}

export interface ImportResult extends OperationResult {
  registered: number;
  duplicates: number;
  duplicatesList?: any[];
}

