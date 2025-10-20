import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TuiButton, TuiTextfield, TuiLabel } from '@taiga-ui/core';
import { TuiTextarea } from '@taiga-ui/kit';
import { CriteriaExample } from '../../models/example.interface';
import { ExamplesSectionComponent } from '../examples-section/examples-section.component';
import { CtrlEnterDirective } from '../../directives/ctrl-enter.directive';

@Component({
  selector: 'app-criteria-filter',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TuiButton,
    TuiTextfield,
    TuiLabel,
    TuiTextarea,
    ExamplesSectionComponent,
    CtrlEnterDirective
  ],
  templateUrl: './criteria-filter.component.html',
  styleUrls: ['./criteria-filter.component.less']
})
export class CriteriaFilterComponent implements OnInit {
  @Input() initialCriteria = 'o => true';
  @Input() examples: CriteriaExample[] = [];
  @Input() showExamplesInitially = false;
  @Output() criteriaSubmitted = new EventEmitter<string>();
  @Output() criteriaCleared = new EventEmitter<void>();

  filterForm!: FormGroup;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.filterForm = this.fb.group({
      criteria: [this.initialCriteria]
    });
  }

  apply(): void {
    const criteria = this.filterForm.value.criteria;
    this.criteriaSubmitted.emit(criteria);
  }

  clear(): void {
    this.filterForm.patchValue({ criteria: this.initialCriteria });
    this.criteriaCleared.emit();
  }
}

