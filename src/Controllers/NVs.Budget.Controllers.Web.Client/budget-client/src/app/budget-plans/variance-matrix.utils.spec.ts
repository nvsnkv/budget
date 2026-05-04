import { NamedRangeResponse } from '../budget/models';
import {
  buildVarianceMatrixModel,
  buildVarianceRowTree,
  collectExpandablePaths,
  flattenVarianceRows
} from './variance-matrix.utils';

describe('variance-matrix.utils', () => {
  const range = (name: string): NamedRangeResponse => ({
    name,
    from: '2026-01-01T00:00:00.000Z',
    till: '2026-02-01T00:00:00.000Z'
  });

  it('orders rows by API tree DFS (not alphabetical)', () => {
    const report = {
      plan: {} as any,
      variances: [
        {
          description: 'Root',
          range: range('P1'),
          expected: { value: 0, currencyCode: 'RUB' },
          actual: { value: 0, currencyCode: 'RUB' },
          difference: { value: 0, currencyCode: 'RUB' },
          hasPlan: false,
          hasActual: false,
          isPlannedOnly: false,
          actualComparison: 0,
          children: [
            {
              description: 'Zebra',
              range: range('P1'),
              expected: { value: 1, currencyCode: 'RUB' },
              actual: { value: 0, currencyCode: 'RUB' },
              difference: { value: 0, currencyCode: 'RUB' },
              hasPlan: true,
              hasActual: false,
              isPlannedOnly: true,
              actualComparison: 0,
              children: []
            },
            {
              description: 'Alpha',
              range: range('P1'),
              expected: { value: 2, currencyCode: 'RUB' },
              actual: { value: 0, currencyCode: 'RUB' },
              difference: { value: 0, currencyCode: 'RUB' },
              hasPlan: true,
              hasActual: false,
              isPlannedOnly: true,
              actualComparison: 0,
              children: []
            }
          ]
        }
      ]
    };

    const model = buildVarianceMatrixModel(report);
    const tree = model.varianceRowTree!;
    const expanded = new Set(collectExpandablePaths(tree));
    const rows = flattenVarianceRows(tree, expanded);

    const labels = rows.map(r => r.label);
    expect(labels).toEqual(['Root', 'Zebra', 'Alpha']);
  });

  it('buildVarianceRowTree preserves nested order', () => {
    const root = {
      description: 'All',
      range: range('Jan'),
      expected: { value: 0, currencyCode: 'RUB' },
      actual: { value: 0, currencyCode: 'RUB' },
      difference: { value: 0, currencyCode: 'RUB' },
      hasPlan: false,
      hasActual: false,
      isPlannedOnly: false,
      actualComparison: 0,
      children: [
        {
          description: 'Housing',
          range: range('Jan'),
          expected: { value: 0, currencyCode: 'RUB' },
          actual: { value: 0, currencyCode: 'RUB' },
          difference: { value: 0, currencyCode: 'RUB' },
          hasPlan: false,
          hasActual: false,
          isPlannedOnly: false,
          actualComparison: 0,
          children: [
            {
              description: 'Rent',
              range: range('Jan'),
              expected: { value: 0, currencyCode: 'RUB' },
              actual: { value: 0, currencyCode: 'RUB' },
              difference: { value: 0, currencyCode: 'RUB' },
              hasPlan: false,
              hasActual: false,
              isPlannedOnly: false,
              actualComparison: 0,
              children: []
            }
          ]
        },
        {
          description: 'Food',
          range: range('Jan'),
          expected: { value: 0, currencyCode: 'RUB' },
          actual: { value: 0, currencyCode: 'RUB' },
          difference: { value: 0, currencyCode: 'RUB' },
          hasPlan: false,
          hasActual: false,
          isPlannedOnly: false,
          actualComparison: 0,
          children: []
        }
      ]
    };

    const tree = buildVarianceRowTree(root as any);
    const expanded = new Set(collectExpandablePaths(tree));
    const rows = flattenVarianceRows(tree, expanded);
    expect(rows.map(r => r.path)).toEqual(['All', 'All/Housing', 'All/Housing/Rent', 'All/Food']);
  });
});
