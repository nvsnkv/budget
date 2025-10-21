import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OperationsApiService } from '../operations-api.service';
import { 
  TuiButton, 
  TuiLoader,
  TuiTitle,
  TuiTextfield,
  TuiLabel
} from '@taiga-ui/core';
import { TuiCardLarge } from '@taiga-ui/layout';
import { TuiAccordion } from '@taiga-ui/kit';
import { NotificationService } from '../shared/notification.service';
import { CriteriaFilterComponent } from '../shared/components/criteria-filter/criteria-filter.component';
import { ExamplesSectionComponent } from '../shared/components/examples-section/examples-section.component';
import { CriteriaExample } from '../shared/models/example.interface';
import { LogbookEntryResponse, LogbookResponse, RangedLogbookEntryResponse, NamedRangeResponse } from '../../budget/models';

interface CriteriaRow {
  description: string;
  path: string;
  level: number;
  rangeData: Map<string, LogbookEntryResponse>;
  hasChildren: boolean;
}
import { DateFormatPipe } from '../shared/pipes/date-format.pipe';
import { CurrencyFormatPipe } from '../shared/pipes/currency-format.pipe';

@Component({
  selector: 'app-logbook-view',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TuiButton,
    TuiLoader,
    TuiCardLarge,
    TuiTitle,
    TuiTextfield,
    TuiLabel,
    TuiAccordion,
    CriteriaFilterComponent,
    ExamplesSectionComponent,
    DateFormatPipe,
    CurrencyFormatPipe
  ],
  templateUrl: './logbook-view.component.html',
  styleUrls: ['./logbook-view.component.less']
})
export class LogbookViewComponent implements OnInit {
  budgetId!: string;
  isLoading = false;
  logbook: LogbookResponse | null = null;
  
  currentCriteria = '';
  fromDate: string = '';
  tillDate: string = '';
  cronExpression = '';
  
  ranges: NamedRangeResponse[] = [];
  criteriaRows: CriteriaRow[] = [];
  expandedRows = new Set<string>();
  
  criteriaExamples: CriteriaExample[] = [
    { label: 'All operations:', code: 'o => true' },
    { label: 'Positive amounts:', code: 'o => o.Amount.Amount > 0' },
    { label: 'Negative amounts:', code: 'o => o.Amount.Amount < 0' },
    { label: 'Contains text:', code: 'o => o.Description.Contains("groceries")' },
    { label: 'By tag:', code: 'o => o.Tags.Any(t => t.Value == "food")' },
    { label: 'Amount range:', code: 'o => o.Amount.Amount >= -1000 && o.Amount.Amount <= -100' }
  ];

  cronExamples: CriteriaExample[] = [
    { label: 'Daily:', code: '0 0 * * *' },
    { label: 'Weekly (Mondays):', code: '0 0 * * 1' },
    { label: 'Monthly (1st day):', code: '0 0 1 * *' },
    { label: 'Bi-weekly:', code: '0 0 1,15 * *' },
    { label: 'Quarterly:', code: '0 0 1 1,4,7,10 *' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private notificationService: NotificationService
  ) {
    this.resetToDefaultDateRange();
  }

  ngOnInit(): void {
    this.budgetId = this.route.snapshot.params['budgetId'];
    
    // Restore filter parameters from query params if present
    const queryParams = this.route.snapshot.queryParams;
    
    if (queryParams['from']) {
      this.fromDate = queryParams['from'];
    }
    
    if (queryParams['till']) {
      this.tillDate = queryParams['till'];
    }
    
    if (queryParams['criteria']) {
      this.currentCriteria = queryParams['criteria'];
    }
    
    if (queryParams['cronExpression']) {
      this.cronExpression = queryParams['cronExpression'];
    }
    
    this.loadLogbook();
  }

  onCriteriaSubmitted(criteria: string): void {
    this.currentCriteria = criteria;
    this.updateUrlAndLoadLogbook();
  }

  onCriteriaCleared(): void {
    this.currentCriteria = '';
    this.cronExpression = '';
    this.resetToDefaultDateRange();
    this.updateUrlAndLoadLogbook();
  }

  private updateUrlAndLoadLogbook(): void {
    // Update URL with current filter state
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        from: this.fromDate || undefined,
        till: this.tillDate || undefined,
        criteria: this.currentCriteria || undefined,
        cronExpression: this.cronExpression || undefined
      },
      queryParamsHandling: 'merge'
    });
    
    this.loadLogbook();
  }

  private resetToDefaultDateRange(): void {
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59);
    
    this.fromDate = firstDay.toISOString().slice(0, 16);
    this.tillDate = lastDay.toISOString().slice(0, 16);
  }

  loadLogbook(): void {
    this.isLoading = true;
    this.logbook = null;
    this.ranges = [];
    this.criteriaRows = [];
    this.expandedRows.clear();

    const from = this.fromDate ? new Date(this.fromDate) : undefined;
    const till = this.tillDate ? new Date(this.tillDate) : undefined;

    this.operationsApi.getLogbook(
      this.budgetId,
      from,
      till,
      this.currentCriteria || undefined,
      this.cronExpression || undefined
    ).subscribe({
      next: (result) => {
        this.isLoading = false;
        this.logbook = result;
        
        if (result.ranges && result.ranges.length > 0) {
          this.ranges = result.ranges.map(r => r.range);
          this.criteriaRows = this.buildCriteriaRows(result.ranges);
        }
        
        if (result.errors.length > 0) {
          const errorMessage = `Logbook loaded with ${result.errors.length} errors`;
          this.notificationService.showWarning(errorMessage).subscribe();
        }
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = this.notificationService.handleError(error, 'Failed to load logbook');
        this.notificationService.showError(errorMessage).subscribe();
      }
    });
  }

  private buildCriteriaRows(rangedEntries: RangedLogbookEntryResponse[]): CriteriaRow[] {
    const rows: CriteriaRow[] = [];
    
    // Get all unique criteria paths from the first range to establish row structure
    if (rangedEntries.length === 0) return rows;
    
    const firstEntry = rangedEntries[0].entry;
    this.collectCriteriaPaths(firstEntry, '', 0, rows, rangedEntries);
    
    return rows;
  }

  private collectCriteriaPaths(
    entry: LogbookEntryResponse, 
    parentPath: string, 
    level: number,
    rows: CriteriaRow[],
    rangedEntries: RangedLogbookEntryResponse[]
  ): void {
    const currentPath = parentPath ? `${parentPath}/${entry.description}` : entry.description;
    
    // Create range data map for this criteria
    const rangeData = new Map<string, LogbookEntryResponse>();
    
    for (const rangedEntry of rangedEntries) {
      const entryData = this.findEntryByPath(rangedEntry.entry, currentPath);
      if (entryData) {
        rangeData.set(rangedEntry.range.name, entryData);
      }
    }
    
    const hasChildren = entry.children && entry.children.length > 0;
    
    rows.push({
      description: entry.description,
      path: currentPath,
      level,
      rangeData,
      hasChildren
    });
    
    // Recursively add children
    if (hasChildren) {
      for (const child of entry.children) {
        this.collectCriteriaPaths(child, currentPath, level + 1, rows, rangedEntries);
      }
    }
  }

  private findEntryByPath(entry: LogbookEntryResponse, targetPath: string): LogbookEntryResponse | null {
    const currentPath = entry.description;
    
    if (currentPath === targetPath) {
      return entry;
    }
    
    if (targetPath.startsWith(currentPath + '/')) {
      const remainingPath = targetPath.substring(currentPath.length + 1);
      
      for (const child of entry.children) {
        const found = this.findEntryByPath(child, remainingPath);
        if (found) return found;
      }
    }
    
    return null;
  }

  toggleRow(path: string): void {
    if (this.expandedRows.has(path)) {
      this.expandedRows.delete(path);
    } else {
      this.expandedRows.add(path);
    }
  }

  isRowExpanded(path: string): boolean {
    return this.expandedRows.has(path);
  }

  isRowVisible(row: CriteriaRow): boolean {
    if (row.level === 0) return true;
    
    // Check if all parent rows are expanded
    const pathParts = row.path.split('/');
    for (let i = 1; i < pathParts.length; i++) {
      const parentPath = pathParts.slice(0, i).join('/');
      if (!this.expandedRows.has(parentPath)) {
        return false;
      }
    }
    return true;
  }

  viewGroupOperations(row: CriteriaRow, rangeName: string, event: Event): void {
    event.stopPropagation();
    
    this.router.navigate(['/budget', this.budgetId, 'operations', 'logbook', 'group'], {
      queryParams: {
        rangeName: rangeName,
        criteriaPath: row.path,
        from: this.fromDate,
        till: this.tillDate,
        criteria: this.currentCriteria || undefined,
        cronExpression: this.cronExpression || undefined
      }
    });
  }

  getEntryForRange(row: CriteriaRow, rangeName: string): LogbookEntryResponse | undefined {
    return row.rangeData.get(rangeName);
  }

  hasOperationsInRange(row: CriteriaRow, rangeName: string): boolean {
    const entry = this.getEntryForRange(row, rangeName);
    return entry ? (entry.operations && entry.operations.length > 0) : false;
  }

  viewOperations(): void {
    this.router.navigate(['/budget', this.budgetId]);
  }

  setDateRange(type: 'currentMonth' | 'lastMonth' | 'currentYear' | 'lastYear'): void {
    const now = new Date();
    let from: Date;
    let till: Date;

    switch (type) {
      case 'currentMonth':
        from = new Date(now.getFullYear(), now.getMonth(), 1);
        till = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59);
        break;
      case 'lastMonth':
        from = new Date(now.getFullYear(), now.getMonth() - 1, 1);
        till = new Date(now.getFullYear(), now.getMonth(), 0, 23, 59, 59);
        break;
      case 'currentYear':
        from = new Date(now.getFullYear(), 0, 1);
        till = new Date(now.getFullYear(), 11, 31, 23, 59, 59);
        break;
      case 'lastYear':
        from = new Date(now.getFullYear() - 1, 0, 1);
        till = new Date(now.getFullYear() - 1, 11, 31, 23, 59, 59);
        break;
    }

    this.fromDate = from.toISOString().slice(0, 16);
    this.tillDate = till.toISOString().slice(0, 16);
    this.updateUrlAndLoadLogbook();
  }

  hasMetadata(error: any): boolean {
    return error.metadata && Object.keys(error.metadata).length > 0;
  }

  getMetadataKeys(metadata: Record<string, any>): string[] {
    return Object.keys(metadata);
  }

  formatMetadataValue(value: any): string {
    if (value === null || value === undefined) {
      return 'null';
    }
    if (typeof value === 'object') {
      try {
        return JSON.stringify(value, null, 2);
      } catch {
        return String(value);
      }
    }
    return String(value);
  }

  hasNestedReasons(error: any): boolean {
    return error.reasons && error.reasons.length > 0;
  }
}



