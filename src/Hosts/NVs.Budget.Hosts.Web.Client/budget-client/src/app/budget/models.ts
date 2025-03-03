// models.ts
export interface BudgetResponse {
    id: string;
    name?: string;
    version?: string;
    taggingCriteria?: string;
    transferCriteria?: string;
    logbookCriteria?: string;
  }
  
  export interface CreateBudgetRequest {
    name?: string;
  }
  
  export interface IError {
    message?: string;
    metadata?: any;
    reasons?: IError[];
  }
  
  export interface UpdateBudgetRequest extends BudgetResponse {
    id: string;
  }