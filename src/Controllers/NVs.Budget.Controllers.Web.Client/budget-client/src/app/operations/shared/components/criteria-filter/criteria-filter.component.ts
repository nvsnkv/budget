import { ChangeDetectionStrategy, Component, effect, inject, input, output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TuiButton, TuiLabel, TuiInput } from '@taiga-ui/core';
import { TuiTextarea } from '@taiga-ui/kit';
import { CriteriaExample } from '../../models/example.interface';
import { ExamplesSectionComponent } from '../examples-section/examples-section.component';
import { CtrlEnterDirective } from '../../directives/ctrl-enter.directive';

@Component({
  selector: 'app-criteria-filter',
  standalone: true,
  imports: [    ReactiveFormsModule,
    TuiButton,
    TuiInput,
    TuiLabel,
    TuiTextarea,
    ExamplesSectionComponent,
    CtrlEnterDirective
  ],
  templateUrl: './criteria-filter.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrls: ['./criteria-filter.component.less']
})
export class CriteriaFilterComponent {
  readonly initialCriteria = input('o => true');
  readonly examples = input<CriteriaExample[]>([]);
  readonly showExamplesInitially = input(false);
  readonly criteriaSubmitted = output<string>();
  readonly criteriaCleared = output<void>();

  filterForm: FormGroup;

  private readonly fb = inject(FormBuilder);

  constructor() {
    this.filterForm = this.fb.group({
      criteria: [this.initialCriteria()]
    });

    effect(() => {
      this.filterForm.patchValue({ criteria: this.initialCriteria() }, { emitEvent: false });
    });
  }

  apply(): void {
    const criteria = this.filterForm.value.criteria;
    this.criteriaSubmitted.emit(criteria);
  }

  clear(): void {
    this.filterForm.patchValue({ criteria: this.initialCriteria() });
    this.criteriaCleared.emit();
  }
}
