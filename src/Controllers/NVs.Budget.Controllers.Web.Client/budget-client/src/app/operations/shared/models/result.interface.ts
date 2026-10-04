import { IError, ISuccess } from '../../../budget/models';

export interface OperationResult {
  errors: IError[];
  successes: ISuccess[];
}

