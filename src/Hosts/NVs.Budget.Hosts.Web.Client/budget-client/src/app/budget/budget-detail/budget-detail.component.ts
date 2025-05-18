import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable, map } from 'rxjs';
import { BudgetApiService } from '../budget-api.service';

@Component({
  selector: 'app-budget-detail',
  imports: [AsyncPipe],
  templateUrl: './budget-detail.component.html',
  styleUrl: './budget-detail.component.less'
})

export class BudgetDetailComponent implements OnInit {
  budgetId$?: Observable<string>;

  constructor(private route: ActivatedRoute, apiService: BudgetApiService) {
    this.baseUrl = apiService.baseUrl;
  }

  public readonly baseUrl: string;

  ngOnInit(): void {
    this.budgetId$ = this.route.params.pipe(map(params => params['budgetId']));
  }
}
