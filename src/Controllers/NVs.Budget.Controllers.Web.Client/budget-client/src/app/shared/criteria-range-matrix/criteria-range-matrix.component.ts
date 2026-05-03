import { NgTemplateOutlet } from '@angular/common';
import { Component, Input, TemplateRef } from '@angular/core';
import { CriteriaMatrixCategoryRow, CriteriaMatrixRangeColumn } from './criteria-range-matrix.models';

@Component({
  selector: 'app-criteria-range-matrix',
  standalone: true,
  imports: [NgTemplateOutlet],
  templateUrl: './criteria-range-matrix.component.html',
  styleUrls: ['./criteria-range-matrix.component.less']
})
export class CriteriaRangeMatrixComponent {
  @Input({ required: true }) rangeColumns!: CriteriaMatrixRangeColumn[];
  @Input({ required: true }) rows!: CriteriaMatrixCategoryRow[];
  @Input() categoryTitle = 'Category';

  @Input({ required: true })
  cellTemplate!: TemplateRef<{
    $implicit: CriteriaMatrixCategoryRow;
    rangeName: string;
    range: CriteriaMatrixRangeColumn;
  }>;

  @Input()
  categoryTemplate?: TemplateRef<{ $implicit: CriteriaMatrixCategoryRow }>;

  @Input()
  rangeHeaderTemplate?: TemplateRef<{ $implicit: CriteriaMatrixRangeColumn }>;
}
