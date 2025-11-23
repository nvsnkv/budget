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

// Operation models
export interface MoneyResponse {
  value: number;
  currencyCode: string;
}

export interface OperationResponse {
  id: string;
  version: string;
  timestamp: string;
  amount: MoneyResponse;
  description: string;
  budgetId: string;
  tags: string[];
  attributes?: Record<string, any>;
}

export interface UpdateOperationRequest {
  id: string;
  version: string;
  timestamp: string;
  amount: MoneyResponse;
  description: string;
  tags: string[];
  attributes?: Record<string, any>;
}

export interface UpdateOperationsRequest {
  budgetVersion: string;
  operations: UpdateOperationRequest[];
  transferConfidenceLevel?: string;
  taggingMode: string;
}

export interface RemoveOperationsRequest {
  criteria: string;
}

export interface RetagOperationsRequest {
  budgetVersion: string;
  criteria: string;
  fromScratch: boolean;
}

export interface IReason {
  message?: string;
  metadata?: Record<string, any>;
  reasons?: IReason[];
}

export interface IError extends IReason {}

export interface ISuccess extends IReason {}

export interface ImportResultResponse {
  registeredOperations: OperationResponse[];
  duplicates: OperationResponse[][];
  errors: IError[];
  successes: ISuccess[];
}

export interface UpdateResultResponse {
  updatedOperations: OperationResponse[];
  errors: IError[];
  successes: ISuccess[];
}

export interface DeleteResultResponse {
  errors: IError[];
  successes: ISuccess[];
}

export interface RetagResultResponse {
  errors: IError[];
  successes: ISuccess[];
}

// Logbook models
export interface NamedRangeResponse {
  name: string;
  from: string;
  till: string;
}

export interface LogbookEntryResponse {
  description: string;
  sum: MoneyResponse;
  from: string;
  till: string;
  operationsCount: number;
  operations: OperationResponse[];
  children: LogbookEntryResponse[];
}

export interface RangedLogbookEntryResponse {
  range: NamedRangeResponse;
  entry: LogbookEntryResponse;
}

export interface LogbookResponse {
  ranges: RangedLogbookEntryResponse[];
  errors: IError[];
  successes: ISuccess[];
}

// Transfer models
export interface TransferResponse {
  sourceId: string;
  source: OperationResponse;
  sinkId: string;
  sink: OperationResponse;
  fee: MoneyResponse;
  comment: string;
  accuracy: string;
}

export interface RegisterTransferRequest {
  sourceId: string;
  sinkId: string;
  fee?: MoneyResponse;
  comment: string;
  accuracy: string;
}

export interface RegisterTransfersRequest {
  transfers: RegisterTransferRequest[];
}

export interface RemoveTransfersRequest {
  sourceIds: string[];
  all: boolean;
}