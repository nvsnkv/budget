import { VarianceReportResponse, VarianceResponse } from '../budget/models';
import { CriteriaMatrixCategoryRow, CriteriaMatrixRangeColumn } from '../shared/criteria-range-matrix/criteria-range-matrix.models';

const cellKey = (path: string, rangeName: string) => `${path}\u0001${rangeName}`;

/** Row tree shape mirrors the API variance tree (same order as logbook / plan criteria). */
export interface VarianceRowNode {
  path: string;
  label: string;
  level: number;
  hasChildren: boolean;
  children: VarianceRowNode[];
}

function dfsVarianceAccumulate(root: VarianceResponse, accumulator: Map<string, VarianceResponse>): void {
  const walk = (node: VarianceResponse, segments: string[]) => {
    const desc = node.description || 'Universal';
    const nextSegments = [...segments, desc];
    const pathKey = nextSegments.join('/');
    accumulator.set(cellKey(pathKey, node.range.name), node);
    for (const child of node.children ?? []) {
      walk(child, nextSegments);
    }
  };
  walk(root, []);
}

/**
 * Preserves `children` order from the API (matches logbook / criterion subcriteria order).
 */
export function buildVarianceRowTree(root: VarianceResponse): VarianceRowNode {
  const walk = (node: VarianceResponse, segments: string[]): VarianceRowNode => {
    const desc = node.description || 'Universal';
    const nextSegments = [...segments, desc];
    const pathKey = nextSegments.join('/');
    const children = (node.children ?? []).map(ch => walk(ch, nextSegments));
    return {
      path: pathKey,
      label: desc,
      level: segments.length,
      hasChildren: children.length > 0,
      children
    };
  };
  return walk(root, []);
}

export function collectExpandablePaths(node: VarianceRowNode): string[] {
  const paths: string[] = [];
  if (node.hasChildren) {
    paths.push(node.path);
    for (const child of node.children) {
      paths.push(...collectExpandablePaths(child));
    }
  }
  return paths;
}

export function flattenVarianceRows(root: VarianceRowNode, expandedRows: Set<string>): CriteriaMatrixCategoryRow[] {
  const rows: CriteriaMatrixCategoryRow[] = [];
  const walk = (node: VarianceRowNode) => {
    rows.push({
      path: node.path,
      label: node.label,
      level: node.level,
      hasChildren: node.hasChildren
    });
    if (node.hasChildren && expandedRows.has(node.path)) {
      for (const child of node.children) {
        walk(child);
      }
    }
  };
  walk(root);
  return rows;
}

/**
 * Pivots API variance (one tree per period column) into a matrix model.
 * Row order follows the first column's tree — the same order the backend uses for `children`.
 */
export function buildVarianceMatrixModel(report: VarianceReportResponse): {
  rangeColumns: CriteriaMatrixRangeColumn[];
  varianceRowTree: VarianceRowNode | null;
  expandablePaths: string[];
  varianceByPathAndRange: Map<string, VarianceResponse>;
} {
  const roots = report.variances ?? [];
  if (roots.length === 0) {
    return { rangeColumns: [], varianceRowTree: null, expandablePaths: [], varianceByPathAndRange: new Map() };
  }

  const rangeColumns: CriteriaMatrixRangeColumn[] = roots.map(root => ({
    name: root.range.name,
    from: root.range.from,
    till: root.range.till
  }));

  const varianceByPathAndRange = new Map<string, VarianceResponse>();
  for (const root of roots) {
    dfsVarianceAccumulate(root, varianceByPathAndRange);
  }

  const varianceRowTree = buildVarianceRowTree(roots[0]);
  const expandablePaths = collectExpandablePaths(varianceRowTree);

  return { rangeColumns, varianceRowTree, expandablePaths, varianceByPathAndRange };
}

export function getVarianceCell(
  varianceByPathAndRange: Map<string, VarianceResponse>,
  path: string,
  rangeName: string
): VarianceResponse | undefined {
  return varianceByPathAndRange.get(cellKey(path, rangeName));
}
