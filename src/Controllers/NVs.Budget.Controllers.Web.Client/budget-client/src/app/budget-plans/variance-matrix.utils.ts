import { VarianceReportResponse, VarianceResponse } from '../budget/models';
import { CriteriaMatrixCategoryRow, CriteriaMatrixRangeColumn } from '../shared/criteria-range-matrix/criteria-range-matrix.models';

const cellKey = (path: string, rangeName: string) => `${path}\u0001${rangeName}`;

function comparePaths(a: string, b: string): number {
  const sa = a.split('/');
  const sb = b.split('/');
  if (sa.length !== sb.length) {
    return sa.length - sb.length;
  }
  return a.localeCompare(b, undefined, { sensitivity: 'base' });
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
 * Pivot API variance roots (one tree per cron range column) into a single category × periods matrix model.
 */
export function buildVarianceMatrixModel(report: VarianceReportResponse): {
  rangeColumns: CriteriaMatrixRangeColumn[];
  rows: CriteriaMatrixCategoryRow[];
  varianceByPathAndRange: Map<string, VarianceResponse>;
} {
  const roots = report.variances ?? [];
  if (roots.length === 0) {
    return { rangeColumns: [], rows: [], varianceByPathAndRange: new Map() };
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

  const paths = new Set<string>();
  for (const key of varianceByPathAndRange.keys()) {
    paths.add(key.split('\u0001')[0]);
  }

  const orderedPaths = [...paths].sort(comparePaths);

  const rows: CriteriaMatrixCategoryRow[] = orderedPaths.map(pathKey => {
    const segments = pathKey.split('/');
    const descendants = [...paths].some(p => p !== pathKey && p.startsWith(`${pathKey}/`));
    return {
      path: pathKey,
      label: segments[segments.length - 1],
      level: segments.length - 1,
      hasChildren: descendants
    };
  });

  return { rangeColumns, rows, varianceByPathAndRange };
}

export function getVarianceCell(
  varianceByPathAndRange: Map<string, VarianceResponse>,
  path: string,
  rangeName: string
): VarianceResponse | undefined {
  return varianceByPathAndRange.get(cellKey(path, rangeName));
}
