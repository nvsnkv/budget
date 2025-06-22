import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable, map } from 'rxjs';
import { BudgetApiService } from '../budget-api.service';

@Component({
  selector: 'app-budget-settings',
  imports: [AsyncPipe],
  templateUrl: './budget-settings.component.html',
  styleUrl: './budget-settings.component.less'
})

export class BudgetSettingsComponent implements OnInit {
  budgetId$?: Observable<string>;

  constructor(private route: ActivatedRoute, apiService: BudgetApiService) {
    this.baseUrl = apiService.baseUrl;
  }

  public readonly baseUrl: string;

  ngOnInit(): void {
    this.budgetId$ = this.route.params.pipe(map(params => params['budgetId']));
  }
} 