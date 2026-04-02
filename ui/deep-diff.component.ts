import { Component, Input, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { DeepDiffer, DeepDiffResult, DiffChangeType, DiffChange } from '@memberjunction/global';
export interface DeepDiffItem extends DiffChange {
  children: DeepDiffItem[];
  parentPath: string;
  selector: 'mj-deep-diff',
  templateUrl: './deep-diff.component.html',
  styleUrls: ['./deep-diff.component.css'],
export class DeepDiffComponent implements OnInit {
  private _oldValue: any;
  private _newValue: any;
  private _showUnchanged: boolean = false;
  private _maxDepth: number = 10;
  private _maxStringLength: number = 100;
  private _treatNullAsUndefined: boolean = false;
  get oldValue(): any {
    return this._oldValue;
  set oldValue(value: any) {
    this._oldValue = value;
    this.generateDiff();
  get newValue(): any {
    return this._newValue;
  set newValue(value: any) {
    this._newValue = value;
  get showUnchanged(): boolean {
    return this._showUnchanged;
  set showUnchanged(value: boolean) {
    this._showUnchanged = value;
    this.updateDifferConfig();
  get maxDepth(): number {
    return this._maxDepth;
  set maxDepth(value: number) {
    this._maxDepth = value;
  get maxStringLength(): number {
    return this._maxStringLength;
  set maxStringLength(value: number) {
    this._maxStringLength = value;
  get treatNullAsUndefined(): boolean {
    return this._treatNullAsUndefined;
  set treatNullAsUndefined(value: boolean) {
    this._treatNullAsUndefined = value;
  @Input() truncateValues: boolean = true;
  diffResult: DeepDiffResult | null = null;
  diffItems: DeepDiffItem[] = [];
  filter: string = '';
  filterType: 'all' | 'added' | 'removed' | 'modified' | 'unchanged' = 'all';
  expandedValuesMap: { [key: string]: boolean } = {};
  private differ: DeepDiffer;
    this.differ = new DeepDiffer({
      includeUnchanged: false,
      maxDepth: this.maxDepth,
      maxStringLength: this.maxStringLength,
      treatNullAsUndefined: this.treatNullAsUndefined
  private updateDifferConfig(): void {
    this.differ.updateConfig({
      includeUnchanged: this.showUnchanged,
  private generateDiff(): void {
    if (this.oldValue === undefined && this.newValue === undefined) {
      this.diffResult = null;
      this.diffItems = [];
    this.diffResult = this.differ.diff(this.oldValue, this.newValue);
    this.diffItems = this.buildHierarchicalItems(this.diffResult.changes);
    if (this.expandAll) {
      this.expandAllItems();
  private buildHierarchicalItems(changes: DiffChange[]): DeepDiffItem[] {
    const rootItems: DeepDiffItem[] = [];
    const itemMap = new Map<string, DeepDiffItem>();
    // Sort by path to ensure parents come before children
    const sortedChanges = [...changes].sort((a, b) => {
      const aDepth = a.path.split('.').length;
      const bDepth = b.path.split('.').length;
      if (aDepth !== bDepth) return aDepth - bDepth;
      return a.path.localeCompare(b.path);
    for (const change of sortedChanges) {
      const pathParts = change.path === 'root' ? [] : change.path.split('.');
      const level = pathParts.length;
      const parentPath = pathParts.slice(0, -1).join('.');
      const item: DeepDiffItem = {
        ...change,
        isExpanded: level === 0,  // Expand only root level items by default
        parentPath
      itemMap.set(change.path, item);
      if (parentPath && itemMap.has(parentPath)) {
        itemMap.get(parentPath)!.children.push(item);
  toggleItem(item: DeepDiffItem): void {
  expandAllItems(): void {
    const expand = (items: DeepDiffItem[]) => {
        item.isExpanded = true;
        if (item.children.length > 0) {
          expand(item.children);
    expand(this.diffItems);
  collapseAllItems(): void {
    const collapse = (items: DeepDiffItem[]) => {
        item.isExpanded = false;
          collapse(item.children);
    collapse(this.diffItems);
  get filteredItems(): DeepDiffItem[] {
    if (!this.filter && this.filterType === 'all') {
      return this.diffItems;
    const filterFn = (item: DeepDiffItem): boolean => {
      const matchesType = this.filterType === 'all' || 
        (this.filterType === 'added' && item.type === DiffChangeType.Added) ||
        (this.filterType === 'removed' && item.type === DiffChangeType.Removed) ||
        (this.filterType === 'modified' && item.type === DiffChangeType.Modified) ||
        (this.filterType === 'unchanged' && item.type === DiffChangeType.Unchanged);
      const matchesText = !this.filter || 
        item.path.toLowerCase().includes(this.filter.toLowerCase()) ||
        item.description.toLowerCase().includes(this.filter.toLowerCase());
      return matchesType && matchesText;
    const filterRecursive = (items: DeepDiffItem[]): DeepDiffItem[] => {
      return items.reduce((acc, item) => {
        const childMatches = filterRecursive(item.children);
        if (filterFn(item) || childMatches.length > 0) {
          acc.push({
            children: childMatches,
            isExpanded: item.isExpanded  // Preserve the original expanded state
      }, [] as DeepDiffItem[]);
    return filterRecursive(this.diffItems);
  getIcon(type: DiffChangeType): string {
      case DiffChangeType.Added: return 'fa-plus-circle';
      case DiffChangeType.Removed: return 'fa-minus-circle';
      case DiffChangeType.Modified: return 'fa-edit';
      case DiffChangeType.Unchanged: return 'fa-check-circle';
  getTypeClass(type: DiffChangeType): string {
      case DiffChangeType.Added: return 'added';
      case DiffChangeType.Removed: return 'removed';
      case DiffChangeType.Modified: return 'modified';
      case DiffChangeType.Unchanged: return 'unchanged';
  formatValue(value: any, path: string): string {
    const isExpanded = !!this.expandedValuesMap[path];
    const shouldTruncate = this.truncateValues && !isExpanded;
      if (shouldTruncate && value.length > this.maxStringLength) {
        return `"${value.substring(0, this.maxStringLength)}..."`;
      return `"${value}"`;
        const json = JSON.stringify(value, null, 2);
        if (shouldTruncate && json.length > 200) {
          // Show a preview for objects
          const preview = JSON.stringify(value).substring(0, 50);
          return `${preview}... (${this.getObjectSize(value)} properties)`;
        return json;
  isValueTruncated(value: any, path: string): boolean {
    if (!this.truncateValues || this.expandedValuesMap[path]) {
      return value.length > this.maxStringLength;
        return json.length > 200;
  toggleValueExpansion(path: string, event: Event): void {
    // Create a new object to trigger change detection
    this.expandedValuesMap = {
      ...this.expandedValuesMap,
      [path]: !this.expandedValuesMap[path]
  private getObjectSize(obj: any): number {
      return obj.length;
    return Object.keys(obj).length;
  copyToClipboard(text: string): void {
      // Could add a toast notification here
  isExpanded(path: string): boolean {
    return !!this.expandedValuesMap[path];
  copyValueToClipboard(value: any, event: Event): void {
    let textToCopy: string;
      textToCopy = 'undefined';
    } else if (value === null) {
      textToCopy = 'null';
        textToCopy = JSON.stringify(value, null, 2);
        textToCopy = String(value);
    navigator.clipboard.writeText(textToCopy).then(() => {
  exportDiff(): void {
    if (!this.diffResult) return;
    const blob = new Blob([JSON.stringify(this.diffResult, null, 2)], 
      { type: 'application/json' });
    a.download = `diff-${new Date().toISOString()}.json`;
