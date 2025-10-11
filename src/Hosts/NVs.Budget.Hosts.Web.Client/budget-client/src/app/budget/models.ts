// Budget models matching C# API
export interface Owner {
  id: string;
  name: string;
}

export interface TaggingCriterionResponse {
  tag: string;
  condition: string;
}

export interface TransferCriterionResponse {
  accuracy: string;
  comment: string;
  criterion: string;
}

export interface LogbookCriteriaResponse {
  description: string;
  subcriteria?: LogbookCriteriaResponse[];
  type?: string;
  tags?: string[];
  substitution?: string;
  criteria?: string;
  isUniversal?: boolean;
}

export interface BudgetResponse {
  id: string;
  name: string;
  version: string;
  owners: Owner[];
  taggingCriteria: TaggingCriterionResponse[];
  transferCriteria: TransferCriterionResponse[];
  logbookCriteria: LogbookCriteriaResponse;
}

export interface BudgetIdentifier {
  id: string;
  version: string;
}

export interface RegisterBudgetRequest {
  name: string;
}

export interface ChangeBudgetOwnersRequest {
  budget: BudgetIdentifier;
  ownerIds: string[];
}

export interface UpdateBudgetRequest {
  name: string;
  version: string;
  taggingCriteria?: TaggingCriterionResponse[];
  transferCriteria?: TransferCriterionResponse[];
  logbookCriteria?: LogbookCriteriaResponse;
}

export interface MergeBudgetsRequest {
  budgetIds: string[];
  purgeEmptyBudgets: boolean;
}

export interface IError {
  message?: string;
  metadata?: any;
  reasons?: IError[];
}

// File Reading Settings models
export interface ValidationRuleResponse {
  pattern: string;
  condition: string;
  value: string;
  errorMessage: string;
}

export interface FileReadingSettingResponse {
  culture: string;
  encoding: string;
  dateTimeKind: string;
  fields: Record<string, string>;
  attributes: Record<string, string>;
  validation: ValidationRuleResponse[];
}