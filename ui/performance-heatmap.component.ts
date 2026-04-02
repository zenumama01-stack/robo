import { Component, Input, OnInit, AfterViewInit, ViewChild, ElementRef, OnChanges, SimpleChanges, OnDestroy } from '@angular/core';
export interface HeatmapData {
  avgTime: number;
  value?: number; // Computed performance score
export interface HeatmapConfig {
  margin?: { top: number; right: number; bottom: number; left: number };
  colorScheme?: string[];
  showTooltip?: boolean;
  animationDuration?: number;
  selector: 'app-performance-heatmap',
    <div class="performance-heatmap">
        <h4 class="chart-title">{{ title || 'Agent vs Model Performance' }}</h4>
        <div class="chart-controls">
          <div class="metric-selector">
            <label>Metric:</label>
            <select [(ngModel)]="selectedMetric" (change)="updateChart()">
              <option value="performance">Performance Score</option>
              <option value="avgTime">Avg Execution Time</option>
              <option value="successRate">Success Rate</option>
        <svg #chartSvg></svg>
        <div class="chart-tooltip" #tooltip style="display: none;"></div>
        <div class="legend-title">{{ getLegendTitle() }}</div>
        <div class="legend-gradient" #legendGradient></div>
        <div class="legend-labels">
          <span class="legend-min">{{ formatLegendValue(minValue) }}</span>
          <span class="legend-max">{{ formatLegendValue(maxValue) }}</span>
    .performance-heatmap {
    .chart-controls {
    .metric-selector {
    .metric-selector label {
    .metric-selector select {
    .chart-tooltip {
      background: rgba(0, 0, 0, 0.85);
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
    .legend-title {
    .legend-gradient {
    .legend-labels {
    /* Chart styles */
    :host ::ng-deep .heatmap-cell {
      stroke: white;
      stroke-width: 1;
    :host ::ng-deep .heatmap-cell:hover {
      stroke: #333;
    :host ::ng-deep .axis {
    :host ::ng-deep .axis path {
      stroke: #ddd;
    :host ::ng-deep .axis .tick line {
    :host ::ng-deep .axis .tick text {
      fill: #666;
    :host ::ng-deep .axis-label {
      fill: #333;
export class PerformanceHeatmapComponent implements OnInit, AfterViewInit, OnChanges, OnDestroy {
  @Input() data: HeatmapData[] = [];
  @Input() title?: string;
  @Input() config: HeatmapConfig = {};
  @ViewChild('chartSvg', { static: true }) chartSvg!: ElementRef<SVGElement>;
  @ViewChild('tooltip', { static: true }) tooltip!: ElementRef<HTMLDivElement>;
  @ViewChild('legendGradient', { static: true }) legendGradient!: ElementRef<HTMLDivElement>;
  private svg!: d3.Selection<SVGElement, unknown, null, undefined>;
  private width = 0;
  private height = 0;
  private margin = { top: 40, right: 20, bottom: 60, left: 80 };
  // Chart configuration
  private defaultColorScheme = ['#f7fbff', '#deebf7', '#c6dbef', '#9ecae1', '#6baed6', '#4292c6', '#2171b5', '#08519c', '#08306b'];
  // Data processing
  selectedMetric = 'performance';
  processedData: HeatmapData[] = [];
  uniqueAgents: string[] = [];
  uniqueModels: string[] = [];
  minValue = 0;
  maxValue = 1;
    this.applyConfig();
    this.initChart();
    this.processData();
    this.updateChart();
    if (changes['data'] && !changes['data'].firstChange) {
    if (changes['config'] && !changes['config'].firstChange) {
    d3.select(window).on('resize.heatmap', null);
  private applyConfig() {
    const config = this.config;
    this.margin = { ...this.margin, ...config.margin };
  private initChart() {
    this.svg = d3.select(this.chartSvg.nativeElement);
    this.initLegend();
    // Set up responsive behavior
    d3.select(window).on('resize.heatmap', () => this.updateChart());
  private processData() {
    if (!this.data || this.data.length === 0) {
      this.processedData = [];
      this.uniqueAgents = [];
      this.uniqueModels = [];
    // Calculate performance scores and process data
    this.processedData = this.data.map(d => ({
      ...d,
      value: this.calculatePerformanceScore(d)
    // Get unique agents and models
    this.uniqueAgents = Array.from(new Set(this.processedData.map(d => d.agent))).sort();
    this.uniqueModels = Array.from(new Set(this.processedData.map(d => d.model))).sort();
    // Update value range based on selected metric
    this.updateValueRange();
  private calculatePerformanceScore(data: HeatmapData): number {
    // Normalize avgTime (lower is better, scale 0-1)
    const maxTime = Math.max(...this.data.map(d => d.avgTime));
    const normalizedTime = maxTime > 0 ? 1 - (data.avgTime / maxTime) : 1;
    // Success rate is already 0-1
    const normalizedSuccess = data.successRate;
    // Weighted combination (60% success rate, 40% speed)
    return normalizedSuccess * 0.6 + normalizedTime * 0.4;
  private updateValueRange() {
    let values: number[];
    switch (this.selectedMetric) {
      case 'avgTime':
        values = this.processedData.map(d => d.avgTime);
      case 'successRate':
        values = this.processedData.map(d => d.successRate);
      case 'performance':
        values = this.processedData.map(d => d.value || 0);
    this.minValue = Math.min(...values);
    this.maxValue = Math.max(...values);
    // Ensure reasonable range
    if (this.minValue === this.maxValue) {
      this.maxValue = this.minValue + 1;
  updateChart() {
    if (!this.processedData || this.processedData.length === 0) {
      this.svg.selectAll('*').remove();
    this.calculateDimensions();
    this.drawChart();
    this.updateLegend();
  private calculateDimensions() {
    const container = this.chartSvg.nativeElement.parentElement!;
    this.width = (this.config.width || container.clientWidth) - this.margin.left - this.margin.right;
    this.height = (this.config.height || Math.max(300, this.uniqueAgents.length * 30 + 100)) - this.margin.top - this.margin.bottom;
    this.svg
      .attr('width', this.width + this.margin.left + this.margin.right)
      .attr('height', this.height + this.margin.top + this.margin.bottom);
  private drawChart() {
    const g = this.svg.append('g')
      .attr('transform', `translate(${this.margin.left},${this.margin.top})`);
    const xScale = d3.scaleBand()
      .domain(this.uniqueModels)
      .range([0, this.width])
      .padding(0.05);
    const yScale = d3.scaleBand()
      .domain(this.uniqueAgents)
      .range([0, this.height])
      .domain([this.minValue, this.maxValue])
    // Draw cells
    const cells = g.selectAll('.heatmap-cell')
      .data(this.processedData)
      .attr('class', 'heatmap-cell')
      .attr('x', d => xScale(d.model) || 0)
      .attr('y', d => yScale(d.agent) || 0)
      .attr('width', xScale.bandwidth())
      .attr('height', yScale.bandwidth())
      .attr('fill', d => colorScale(this.getMetricValue(d)))
      .on('mouseover', (event, d) => this.showTooltip(event, d))
      .on('mouseout', () => this.hideTooltip());
    // Add animation
    if (this.config.animationDuration !== 0) {
      cells
        .attr('opacity', 0)
        .transition()
        .duration(this.config.animationDuration || 500)
        .delay((d, i) => i * 20)
        .attr('opacity', 1);
    this.drawAxes(g, xScale, yScale);
    // Add value labels on cells (for smaller datasets)
    if (this.processedData.length <= 50) {
      g.selectAll('.cell-label')
        .attr('class', 'cell-label')
        .attr('x', d => (xScale(d.model) || 0) + xScale.bandwidth() / 2)
        .attr('y', d => (yScale(d.agent) || 0) + yScale.bandwidth() / 2)
        .attr('dominant-baseline', 'middle')
        .attr('fill', d => this.getTextColor(colorScale(this.getMetricValue(d))))
        .text(d => this.formatCellValue(this.getMetricValue(d)));
  private drawAxes(g: any, xScale: any, yScale: any) {
    // X axis (models)
    g.append('g')
      .attr('class', 'axis axis-x')
      .attr('transform', `translate(0,${this.height})`)
      .call(d3.axisBottom(xScale))
      .attr('transform', 'rotate(-45)');
    // Y axis (agents)
      .attr('class', 'axis axis-y')
      .call(d3.axisLeft(yScale));
    // Axis labels
    g.append('text')
      .attr('class', 'axis-label')
      .attr('y', 0 - this.margin.left)
      .attr('x', 0 - (this.height / 2))
      .text('MJ: AI Agents');
      .attr('transform', `translate(${this.width / 2}, ${this.height + this.margin.bottom - 10})`)
      .text('MJ: AI Models');
  private getMetricValue(data: HeatmapData): number {
        return data.avgTime;
        return data.successRate;
        return data.value || 0;
  private getTextColor(backgroundColor: string): string {
    // Convert color to RGB and calculate luminance
    const rgb = d3.rgb(backgroundColor);
    const luminance = (0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b) / 255;
    return luminance > 0.5 ? '#333' : '#fff';
  private formatCellValue(value: number): string {
        return `${(value / 1000).toFixed(1)}s`;
        return `${(value * 100).toFixed(0)}%`;
        return value.toFixed(2);
  private showTooltip(event: MouseEvent, data: HeatmapData) {
    const tooltip = d3.select(this.tooltip.nativeElement);
    const content = `
      <div><strong>${data.agent} × ${data.model}</strong></div>
      <div>Performance Score: ${(data.value || 0).toFixed(3)}</div>
      <div>Success Rate: ${(data.successRate * 100).toFixed(1)}%</div>
      <div>Avg Time: ${(data.avgTime / 1000).toFixed(2)}s</div>
      .style('display', 'block')
      .html(content)
      .style('left', (event.offsetX + 10) + 'px')
      .style('top', (event.offsetY - 10) + 'px');
  private hideTooltip() {
    d3.select(this.tooltip.nativeElement)
      .style('display', 'none');
  private initLegend() {
    const gradient = d3.select(this.legendGradient.nativeElement)
      .attr('width', '100%')
      .attr('height', '100%')
      .append('defs')
      .attr('id', 'legend-gradient')
      .attr('x2', '100%');
    // Add color stops
    const colorScheme = this.config.colorScheme || this.defaultColorScheme;
    colorScheme.forEach((color, i) => {
        .attr('offset', `${(i / (colorScheme.length - 1)) * 100}%`)
        .attr('stop-color', color);
    d3.select(this.legendGradient.nativeElement)
      .select('svg')
  private updateLegend() {
    // Update gradient colors based on current color scale
      .select('linearGradient');
    gradient.selectAll('stop').remove();
    // Create 10 color stops
    for (let i = 0; i <= 10; i++) {
      const t = i / 10;
      const value = this.minValue + t * (this.maxValue - this.minValue);
        .attr('offset', `${t * 100}%`)
        .attr('stop-color', colorScale(value));
  getLegendTitle(): string {
        return 'Execution Time';
        return 'Success Rate';
        return 'Performance Score';
  formatLegendValue(value: number): string {
