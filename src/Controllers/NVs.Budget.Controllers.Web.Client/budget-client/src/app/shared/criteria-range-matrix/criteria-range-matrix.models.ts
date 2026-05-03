export interface CriteriaMatrixRangeColumn {
  /** Stable key for tracking and cell lookup */
  name: string;
  from?: string;
  till?: string;
  /** Shown in the header instead of `name` when provided */
  headerLabel?: string;
}

export interface CriteriaMatrixCategoryRow<P = unknown> {
  path: string;
  label: string;
  level: number;
  hasChildren?: boolean;
  /** Optional opaque row context for cell/category templates */
  payload?: P;
}
