import { SuiteHierarchyNode } from '../../services/testing-instrumentation.service';
  selector: 'app-suite-tree',
    <div class="suite-tree">
          <button class="tree-action-btn" (click)="expandAll()" title="Expand All">
          <button class="tree-action-btn" (click)="collapseAll()" title="Collapse All">
        @if (suites && suites.length > 0) {
          @for (suite of suites; track suite.id) {
              <app-suite-tree-node
                [node]="suite"
                [level]="0"
                [selectedId]="selectedSuiteId"
                (nodeClick)="onNodeClick($event)"
                (toggleExpand)="onToggleExpand($event)"
              ></app-suite-tree-node>
          <div class="no-suites">
            <p>No test suites found</p>
    .suite-tree {
    .tree-header h4 {
    .tree-header h4 i {
    .tree-actions {
    .tree-action-btn {
    .tree-action-btn:hover {
    .tree-action-btn i {
    .no-suites {
    .no-suites i {
    .no-suites p {
export class SuiteTreeComponent {
  @Input() suites: SuiteHierarchyNode[] = [];
  @Input() selectedSuiteId: string | null = null;
  @Output() suiteSelect = new EventEmitter<string>();
  onNodeClick(suiteId: string): void {
    this.suiteSelect.emit(suiteId);
  onToggleExpand(node: SuiteHierarchyNode): void {
  expandAll(): void {
    this.setExpandedRecursive(this.suites, true);
  collapseAll(): void {
    this.setExpandedRecursive(this.suites, false);
  private setExpandedRecursive(nodes: SuiteHierarchyNode[], expanded: boolean): void {
      node.expanded = expanded;
      if (node.children && node.children.length > 0) {
        this.setExpandedRecursive(node.children, expanded);
  selector: 'app-suite-tree-node',
    <div class="tree-node" [style.padding-left.px]="level * 16">
        [class.selected]="node.id === selectedId"
        (click)="onClick()"
        @if (node.children && node.children.length > 0) {
            (click)="onToggle($event)"
            <i class="fa-solid" [class.fa-chevron-right]="!node.expanded" [class.fa-chevron-down]="node.expanded"></i>
        @if (!node.children || node.children.length === 0) {
        <i class="fa-solid fa-folder suite-icon"></i>
        <span class="suite-name">{{ node.name }}</span>
        <div class="suite-metrics">
          <span class="test-count" title="Test Count">
            {{ node.testCount }}
          <span class="pass-rate" [class]="getPassRateClass(node.passRate)" title="Pass Rate">
            {{ node.passRate.toFixed(0) }}%
      @if (node.expanded && node.children && node.children.length > 0) {
        @for (child of node.children; track child.id) {
              [node]="child"
              [level]="level + 1"
              [selectedId]="selectedId"
              (nodeClick)="nodeClick.emit($event)"
              (toggleExpand)="toggleExpand.emit($event)"
    .suite-metrics {
    .test-count i {
    .pass-rate {
    .pass-rate.excellent {
    .pass-rate.good {
      background: rgba(139, 195, 74, 0.1);
      color: #8bc34a;
    .pass-rate.fair {
    .pass-rate.poor {
export class SuiteTreeNodeComponent {
  @Input() node!: SuiteHierarchyNode;
  @Input() level = 0;
  @Input() selectedId: string | null = null;
  @Output() nodeClick = new EventEmitter<string>();
  @Output() toggleExpand = new EventEmitter<SuiteHierarchyNode>();
  onClick(): void {
    this.nodeClick.emit(this.node.id);
  onToggle(event: Event): void {
    this.toggleExpand.emit(this.node);
  getPassRateClass(passRate: number): string {
    if (passRate >= 90) return 'excellent';
    if (passRate >= 75) return 'good';
    if (passRate >= 50) return 'fair';
    return 'poor';
