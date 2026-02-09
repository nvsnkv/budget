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
import { TuiAccordion, TuiCheckbox, TuiChevron, TuiDataListWrapper, TuiSelect } from '@taiga-ui/kit';
import { NotificationService } from '../shared/notification.service';
import { CriteriaFilterComponent } from '../shared/components/criteria-filter/criteria-filter.component';
import { ExamplesSectionComponent } from '../shared/components/examples-section/examples-section.component';
import { CriteriaExample } from '../shared/models/example.interface';
import { LogbookEntryResponse, LogbookResponse, RangedLogbookEntryResponse, NamedRangeResponse } from '../../budget/models';
import { LogbookStateService } from './logbook-state.service';

interface CriteriaRow {
  description: string;
  path: string;
  level: number;
  rangeData: Map<string, LogbookEntryResponse>;
  hasChildren: boolean;
  children: CriteriaRow[];
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
    TuiCheckbox,
    TuiChevron,
    TuiDataListWrapper,
    TuiSelect,
    CriteriaFilterComponent,
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
  showRelative = false;
  invertRelative = false;
  highlightRelative = true;
  
  currentCriteria = '';
  fromDate: string = '';
  tillDate: string = '';
  cronExpression = '';
  outputCurrency = '';
  useDatePresets = true;
  useCronPresets = false;
  selectedDatePreset: 'lastYear' | 'currentYear' | 'lastMonth' | 'currentMonth' | null = null;
  selectedCronPreset: 'monthly' | 'yearly' | null = null;
  showFilters = true;
  
  ranges: NamedRangeResponse[] = [];
  criteriaRows: CriteriaRow[] = [];
  private criteriaTree: CriteriaRow[] = [];
  expandedRows = new Set<string>();
  private groupSorts = new Map<string, { rangeName: string; direction: 'asc' | 'desc' }>();
  
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

  readonly items: string[] = ["RUB", "USD", "EUR"];
  private readonly relativeNumberFormat = new Intl.NumberFormat(undefined, {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2
  }); 

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private operationsApi: OperationsApiService,
    private notificationService: NotificationService,
    private logbookStateService: LogbookStateService
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

    if (queryParams['outputCurrency']) {
      this.outputCurrency = queryParams['outputCurrency'];
    }
    
    this.loadLogbook();
  }

  onCriteriaSubmitted(criteria: string): void {
    this.currentCriteria = criteria;
    this.clearStateAndReload();
  }

  onCriteriaCleared(): void {
    this.currentCriteria = '';
    this.cronExpression = '';
    this.outputCurrency = '';
    this.selectedDatePreset = null;
    this.selectedCronPreset = null;
    this.resetToDefaultDateRange();
    this.clearStateAndReload();
  }

  private clearStateAndReload(): void {
    // Clear saved state when filters change
    this.logbookStateService.clearState(this.budgetId);
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
        cronExpression: this.cronExpression || undefined,
        outputCurrency: this.outputCurrency || undefined
      },
      queryParamsHandling: 'merge'
    });
    
    this.loadLogbook();
  }

  private resetToDefaultDateRange(): void {
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    
    this.fromDate = this.toDateInputValue(firstDay);
    this.tillDate = this.toDateInputValue(lastDay);
  }

  loadLogbook(): void {
    this.isLoading = true;
    this.logbook = null;
    this.ranges = [];
    this.criteriaRows = [];
    this.criteriaTree = [];
    this.groupSorts.clear();

    const from = this.fromDate ? this.parseDateInput(this.fromDate) : undefined;
    const till = this.tillDate ? this.parseDateInput(this.tillDate) : undefined;

    this.operationsApi.getLogbook(
      this.budgetId,
      from,
      till,
      this.currentCriteria || undefined,
      this.cronExpression || undefined,
      this.outputCurrency || undefined
    ).subscribe({
      next: (result) => {
        this.isLoading = false;
        this.logbook = result;
        
        if (result.ranges && result.ranges.length > 0) {
          this.ranges = result.ranges.map(r => r.range);
          this.criteriaTree = this.buildCriteriaTree(result.ranges);
          this.criteriaRows = this.flattenCriteriaRows(this.criteriaTree);
          
          // Restore expansion state if it exists, otherwise start fresh
          const savedState = this.logbookStateService.getState(this.budgetId);
          if (savedState) {
            this.expandedRows = savedState.expandedRows;
            this.criteriaRows = this.flattenCriteriaRows(this.criteriaTree);
            
            // Restore scroll position after view is rendered
            setTimeout(() => {
              window.scrollTo(0, savedState.scrollPosition);
            }, 100);
          } else {
            this.expandAllRows();
            this.criteriaRows = this.flattenCriteriaRows(this.criteriaTree);
          }
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

  private buildCriteriaTree(rangedEntries: RangedLogbookEntryResponse[]): CriteriaRow[] {
    if (rangedEntries.length === 0) return [];
    const firstEntry = rangedEntries[0].entry;
    return [this.buildCriteriaNode(firstEntry, '', 0, rangedEntries)];
  }

  private buildCriteriaNode(
    entry: LogbookEntryResponse, 
    parentPath: string, 
    level: number,
    rangedEntries: RangedLogbookEntryResponse[]
  ): CriteriaRow {
    const currentPath = parentPath ? `${parentPath}/${entry.description}` : entry.description;
    
    // Create range data map for this criteria
    const rangeData = new Map<string, LogbookEntryResponse>();
    
    for (const rangedEntry of rangedEntries) {
      const entryData = this.findEntryByPath(rangedEntry.entry, currentPath);
      if (entryData) {
        rangeData.set(rangedEntry.range.name, entryData);
      }
    }
    
    const children = entry.children?.map(child =>
      this.buildCriteriaNode(child, currentPath, level + 1, rangedEntries)
    ) ?? [];
    const hasChildren = children.length > 0;
    
    return {
      description: entry.description,
      path: currentPath,
      level,
      rangeData,
      hasChildren,
      children
    };
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
    this.criteriaRows = this.flattenCriteriaRows(this.criteriaTree);
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

  get isAllExpanded(): boolean {
    const expandable = this.getExpandablePaths();
    return expandable.length > 0 && expandable.every(path => this.expandedRows.has(path));
  }

  toggleExpandAll(): void {
    if (this.isAllExpanded) {
      this.expandedRows.clear();
    } else {
      this.expandAllRows();
    }
    this.criteriaRows = this.flattenCriteriaRows(this.criteriaTree);
  }

  toggleGroupSort(row: CriteriaRow, rangeName: string, event: Event): void {
    event.stopPropagation();
    const existing = this.groupSorts.get(row.path);
    if (!existing || existing.rangeName !== rangeName) {
      this.groupSorts.set(row.path, { rangeName, direction: 'desc' });
    } else if (existing.direction === 'desc') {
      this.groupSorts.set(row.path, { rangeName, direction: 'asc' });
    } else {
      this.groupSorts.delete(row.path);
    }
    this.criteriaRows = this.flattenCriteriaRows(this.criteriaTree);
  }

  resetSorting(): void {
    if (this.groupSorts.size === 0) return;
    this.groupSorts.clear();
    this.criteriaRows = this.flattenCriteriaRows(this.criteriaTree);
  }

  get hasActiveSorts(): boolean {
    return this.groupSorts.size > 0;
  }

  getGroupSortIndicator(row: CriteriaRow, rangeName: string): string {
    const existing = this.groupSorts.get(row.path);
    if (!existing || existing.rangeName !== rangeName) return '↕';
    return existing.direction === 'asc' ? '↑' : '↓';
  }

  getRowSortIndicator(row: CriteriaRow): string {
    const existing = this.groupSorts.get(row.path);
    if (!existing) return '';
    return existing.direction === 'asc' ? '↑' : '↓';
  }

  getRangeSortIndicator(rangeName: string): string {
    const directions = new Set<'asc' | 'desc'>();
    for (const sort of this.groupSorts.values()) {
      if (sort.rangeName === rangeName) {
        directions.add(sort.direction);
      }
    }
    if (directions.size === 0) return '';
    if (directions.size > 1) return '↕';
    return directions.has('asc') ? '↑' : '↓';
  }

  private flattenCriteriaRows(tree: CriteriaRow[]): CriteriaRow[] {
    const rows: CriteriaRow[] = [];
    for (const root of tree) {
      this.addFlattenedRow(root, rows);
    }
    return rows;
  }

  private addFlattenedRow(row: CriteriaRow, rows: CriteriaRow[]): void {
    rows.push(row);
    if (!row.hasChildren || !this.expandedRows.has(row.path)) {
      return;
    }
    const children = this.getSortedChildren(row);
    for (const child of children) {
      this.addFlattenedRow(child, rows);
    }
  }

  private getSortedChildren(row: CriteriaRow): CriteriaRow[] {
    const sortConfig = this.groupSorts.get(row.path);
    const children = [...row.children];
    if (!sortConfig) return children;

    const directionMultiplier = sortConfig.direction === 'asc' ? 1 : -1;
    return children.sort((left, right) => {
      const leftValue = this.getRangeSum(left, sortConfig.rangeName);
      const rightValue = this.getRangeSum(right, sortConfig.rangeName);
      if (leftValue === rightValue) {
        return left.description.localeCompare(right.description, undefined, { sensitivity: 'base' });
      }
      return (leftValue - rightValue) * directionMultiplier;
    });
  }

  private getRangeSum(row: CriteriaRow, rangeName: string): number {
    return row.rangeData.get(rangeName)?.sum.value ?? 0;
  }

  private getBaseInfo(row: CriteriaRow): { index: number; value: number } | null {
    for (let i = 0; i < this.ranges.length; i += 1) {
      const candidateRangeName = this.ranges[i].name;
      const candidateValue = this.getRangeSum(row, candidateRangeName);
      if (candidateValue !== 0) {
        return { index: i, value: candidateValue };
      }
    }
    return null;
  }

  private getRelativeChangeValue(row: CriteriaRow, rangeName: string): number | null {
    if (this.ranges.length === 0) {
      return null;
    }
    const rangeIndex = this.ranges.findIndex(range => range.name === rangeName);
    const currentValue = this.getRangeSum(row, rangeName);
    const baseInfo = this.getBaseInfo(row);
    if (!baseInfo) {
      return null;
    }
    if (rangeIndex < baseInfo.index) {
      return null;
    }
    if (rangeIndex === baseInfo.index) {
      return 0;
    }
    if (currentValue === 0) {
      return null;
    }
    let previous = 0;
    for (let i = rangeIndex - 1; i >= baseInfo.index; i -= 1) {
      const candidateRangeName = this.ranges[i].name;
      const candidateValue = this.getRangeSum(row, candidateRangeName);
      if (candidateValue !== 0) {
        previous = candidateValue;
        break;
      }
    }
    if (previous === 0) {
      return null;
    }
    if (!Number.isFinite(previous) || !Number.isFinite(currentValue)) {
      return null;
    }
    const delta = currentValue - previous;
    return (delta / Math.abs(previous)) * 100;
  }

  private expandAllRows(): void {
    this.expandedRows = new Set(this.getExpandablePaths());
  }

  private getExpandablePaths(): string[] {
    const paths: string[] = [];
    const walk = (row: CriteriaRow) => {
      if (row.hasChildren) {
        paths.push(row.path);
        row.children.forEach(walk);
      }
    };
    this.criteriaTree.forEach(walk);
    return paths;
  }

  viewGroupOperations(row: CriteriaRow, rangeName: string, event: Event): void {
    event.stopPropagation();
    
    // Save current state before navigating
    this.logbookStateService.saveState(
      this.budgetId, 
      this.expandedRows,
      window.scrollY
    );
    
    this.router.navigate(['/budget', this.budgetId, 'operations', 'logbook', 'group'], {
      queryParams: {
        rangeName: rangeName,
        criteriaPath: row.path,
        from: this.fromDate,
        till: this.tillDate,
        criteria: this.currentCriteria || undefined,
        cronExpression: this.cronExpression || undefined,
        outputCurrency: this.outputCurrency || undefined
      }
    });
  }

  getEntryForRange(row: CriteriaRow, rangeName: string): LogbookEntryResponse | undefined {
    return row.rangeData.get(rangeName);
  }

  getRelativeChangeInfo(row: CriteriaRow, rangeName: string): { value: number | null; display: string } {
    const rangeIndex = this.ranges.findIndex(range => range.name === rangeName);
    const baseInfo = this.getBaseInfo(row);
    if (baseInfo && rangeIndex === baseInfo.index) {
      return { value: 0, display: 'base' };
    }
    const value = this.getRelativeChangeValue(row, rangeName);
    if (value === null || !Number.isFinite(value)) {
      return { value: null, display: '-' };
    }
    const adjustedValue = this.invertRelative ? -value : value;
    const sign = adjustedValue > 0 ? '▲ ' : adjustedValue < 0 ? '▼ ' : '';
    return {
      value: adjustedValue,
      display: `${sign}${this.relativeNumberFormat.format(Math.abs(adjustedValue))}%`
    };
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
        till = new Date(now.getFullYear(), now.getMonth() + 1, 0);
        break;
      case 'lastMonth':
        from = new Date(now.getFullYear(), now.getMonth() - 1, 1);
        till = new Date(now.getFullYear(), now.getMonth(), 0);
        break;
      case 'currentYear':
        from = new Date(now.getFullYear(), 0, 1);
        till = new Date(now.getFullYear(), 11, 31);
        break;
      case 'lastYear':
        from = new Date(now.getFullYear() - 1, 0, 1);
        till = new Date(now.getFullYear() - 1, 11, 31);
        break;
    }

    this.fromDate = this.toDateInputValue(from);
    this.tillDate = this.toDateInputValue(till);
    this.selectedDatePreset = type;
  }

  setCronPreset(type: 'monthly' | 'yearly'): void {
    this.cronExpression = type === 'monthly' ? '0 0 1 * *' : '0 0 1 1 *';
    this.selectedCronPreset = type;
  }

  private parseDateInput(value: string): Date {
    const [year, month, day] = value.split('-').map(Number);
    if (!Number.isFinite(year) || !Number.isFinite(month) || !Number.isFinite(day)) {
      return new Date(value);
    }
    return new Date(year, month - 1, day);
  }

  private toDateInputValue(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
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



