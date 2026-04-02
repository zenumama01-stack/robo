import { Component, Input, OnInit, OnDestroy, AfterViewInit, ViewChild, ElementRef, OnChanges, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { TrendData } from '../../services/ai-instrumentation.service';
export interface TimeSeriesConfig {
  showGrid?: boolean;
  colors?: string[];
  useDualAxis?: boolean;
export interface DataPointClickEvent {
  data: TrendData;
  metric: string;
  event: MouseEvent;
  selector: 'app-time-series-chart',
    <div class="time-series-chart">
      @if (title) {
          <h4 class="chart-title">{{ title }}</h4>
          @if (showLegend) {
              @for (metric of visibleMetrics; track metric) {
                  class="legend-item"
                  (click)="toggleMetric(metric)"
                  [class.legend-item--disabled]="!isMetricVisible(metric)"
                    class="legend-color"
                    [style.background-color]="getMetricColor(metric)"
                  <span class="legend-label">{{ getMetricLabel(metric) }}</span>
    .time-series-chart {
      overflow: hidden; /* Ensure content doesn't overflow */
      flex-shrink: 0; /* Prevent header from being squeezed */
    .legend-item--disabled {
    .legend-label {
      min-height: 0; /* Important: allows flex child to shrink below content size */
      background: rgba(0, 0, 0, 0.8);
    :host ::ng-deep .chart-line {
    :host ::ng-deep .chart-area {
      fill-opacity: 0.1;
    :host ::ng-deep .chart-dot {
      transition: r 0.1s ease;
    :host ::ng-deep .chart-dot:hover {
      filter: drop-shadow(0 0 4px rgba(0,0,0,0.3));
    :host ::ng-deep .grid-line {
      stroke: #f0f0f0;
    :host ::ng-deep .axis-y-left {
    :host ::ng-deep .axis-y-right {
export class TimeSeriesChartComponent implements OnInit, OnDestroy, AfterViewInit, OnChanges {
  @Input() data: TrendData[] = [];
  @Input() config: TimeSeriesConfig = {};
  @Input() showLegend = true;
  @Input() showControls = true;
  @Output() dataPointClick: EventEmitter<DataPointClickEvent> = new EventEmitter<DataPointClickEvent>();
  @Output() timeRangeChange = new EventEmitter<string>();
  private margin = { top: 10, right: 70, bottom: 50, left: 70 }; // Increased bottom margin for x-axis labels
  private defaultColors = ['#2196f3', '#4caf50', '#ff9800', '#f44336', '#9c27b0'];
  // Metrics configuration
  visibleMetrics = ['executions', 'cost', 'tokens', 'avgTime', 'errors'];
  private hiddenMetrics = new Set<string>();
    // Cleanup D3 event listeners
    d3.select(window).on('resize.timeseries', null);
    d3.select(window).on('resize.timeseries', () => this.updateChart());
  private updateChart() {
    const containerWidth = container.clientWidth;
    const containerHeight = container.clientHeight;
    // Use config dimensions or fallback to container dimensions
    this.width = (this.config.width || containerWidth) - this.margin.left - this.margin.right;
    this.height = (this.config.height || Math.max(containerHeight, 200)) - this.margin.top - this.margin.bottom;
    // Ensure minimum dimensions for usability
    this.width = Math.max(this.width, 200);
    this.height = Math.max(this.height, 150);
    const xScale = d3.scaleTime()
      .domain(d3.extent(this.data, d => d.timestamp) as [Date, Date])
      .range([0, this.width]);
    // Create separate scales for different metrics
    const scales = this.createMetricScales();
    // Draw grid
    if (this.config.showGrid !== false) {
      this.drawGrid(g, xScale, scales);
    this.drawAxes(g, xScale, scales);
    // Draw data lines and areas
    this.drawMetrics(g, xScale, scales);
    // Draw interactive elements
    this.drawInteractiveElements(g, xScale, scales);
  private createMetricScales() {
    const scales: { [key: string]: d3.ScaleLinear<number, number> } = {};
    if (this.config.useDualAxis !== false) {
      // Dual axis mode: Left axis (cost, avgTime), Right axis (executions, tokens, errors)
      const leftAxisMetrics = ['cost', 'avgTime'];
      const rightAxisMetrics = ['executions', 'tokens', 'errors'];
      // Create left axis scale (cost and time)
      const leftValues = leftAxisMetrics.flatMap(metric =>
        this.data.map(d => {
          const value = this.getMetricValue(d, metric);
          // Normalize avgTime to seconds for better scale comparison with cost
          return metric === 'avgTime' ? (value || 0) / 1000 : (value || 0);
        }).filter((v): v is number => v != null)
      if (leftValues.length > 0) {
        const maxLeftValue = Math.max(...leftValues);
        const leftScale = d3.scaleLinear()
          .domain([0, maxLeftValue])
          .range([this.height, 0])
        leftAxisMetrics.forEach(metric => scales[metric] = leftScale);
      // Create right axis scale (count-based metrics)
      const rightValues = rightAxisMetrics.flatMap(metric =>
        this.data.map(d => this.getMetricValue(d, metric)).filter((v): v is number => v != null)
      if (rightValues.length > 0) {
        const maxRightValue = Math.max(...rightValues);
        const rightScale = d3.scaleLinear()
          .domain([0, maxRightValue])
        rightAxisMetrics.forEach(metric => scales[metric] = rightScale);
      // Single axis mode (original behavior)
      const metricGroups = {
        count: ['executions', 'errors'],
        cost: ['cost'],
        tokens: ['tokens'],
        time: ['avgTime']
      Object.entries(metricGroups).forEach(([groupName, metrics]) => {
        const allValues = metrics.flatMap(metric =>
        if (allValues.length > 0) {
          const maxValue = Math.max(...allValues);
          const scale = d3.scaleLinear()
            .domain([0, maxValue])
          metrics.forEach(metric => scales[metric] = scale);
    return scales;
  private drawGrid(g: any, xScale: any, scales: any) {
    // Vertical grid lines
    g.selectAll('.grid-line-x')
      .data(xScale.ticks(6))
      .attr('class', 'grid-line grid-line-x')
      .attr('x1', (d: Date) => xScale(d))
      .attr('x2', (d: Date) => xScale(d))
      .attr('y2', this.height);
    // Horizontal grid lines (use first scale)
    const firstScale = Object.values(scales)[0] as d3.ScaleLinear<number, number>;
    if (firstScale) {
      g.selectAll('.grid-line-y')
        .data(firstScale.ticks(5))
        .attr('class', 'grid-line grid-line-y')
        .attr('x2', this.width)
        .attr('y1', (d: number) => firstScale(d))
        .attr('y2', (d: number) => firstScale(d));
  private drawAxes(g: any, xScale: any, scales: any) {
    // X axis
      .call(d3.axisBottom(xScale)
        .ticks(this.getOptimalTickCount())
        .tickFormat(this.getTimeFormat() as any));
      // Dual Y-axis mode
      // Left Y axis (cost, time)
      const leftScale = scales[leftAxisMetrics[0]];
      if (leftScale) {
          .attr('class', 'axis axis-y axis-y-left')
          .call(d3.axisLeft(leftScale)
            .ticks(5)
            .tickFormat((d) => {
              const value = d as number;
              // Format based on value range - if > 1, likely cost, else time in seconds
              return value > 1 ? `$${value.toFixed(2)}` : `${value.toFixed(1)}s`;
        // Left axis label
          .attr('class', 'axis-label axis-label-left')
          .attr('y', 0 - this.margin.left + 20)
          .text('Cost ($) / Time (s)');
      // Right Y axis (counts)
      const rightScale = scales[rightAxisMetrics[0]];
      if (rightScale) {
          .attr('class', 'axis axis-y axis-y-right')
          .attr('transform', `translate(${this.width},0)`)
          .call(d3.axisRight(rightScale)
              // Format large numbers with K/M suffixes
              if (value >= 1000000) return `${(value / 1000000).toFixed(1)}M`;
              if (value >= 1000) return `${(value / 1000).toFixed(1)}K`;
        // Right axis label
          .attr('class', 'axis-label axis-label-right')
          .attr('transform', 'rotate(90)')
          .attr('y', 0 - this.width - this.margin.right + 20)
          .attr('x', this.height / 2)
          .text('Count (Executions / Tokens)');
      // Single Y axis (original behavior)
          .call(d3.axisLeft(firstScale).ticks(5));
  private drawMetrics(g: any, xScale: any, scales: any) {
    // Create a group for lines and areas
    const linesGroup = g.append('g').attr('class', 'lines-group');
    // First draw all lines and areas
    this.visibleMetrics.forEach((metric) => {
      if (this.hiddenMetrics.has(metric) || !scales[metric]) return;
      const color = this.getMetricColor(metric);
      const scale = scales[metric];
      // Create line generator with proper value transformation
      const line = d3.line<TrendData>()
        .x(d => xScale(d.timestamp))
        .y(d => {
          const value = this.getMetricValue(d, metric) || 0;
          // Normalize avgTime to seconds if using dual axis
          const transformedValue = (this.config.useDualAxis !== false && metric === 'avgTime') 
            ? value / 1000 : value;
          return scale(transformedValue);
      // Create area generator with proper value transformation
      const area = d3.area<TrendData>()
        .y0(this.height)
        .y1(d => {
      // Draw area (optional)
      if (metric === 'executions' || metric === 'cost') {
        linesGroup.append('path')
          .datum(this.data)
          .attr('class', `chart-area chart-area--${metric}`)
          .attr('d', area)
          .attr('fill', color);
        .attr('class', `chart-line chart-line--${metric}`)
        .attr('d', line)
        .attr('stroke', color);
    // Then create dots group on top of everything
    const dotsGroup = g.append('g').attr('class', 'dots-group').style('pointer-events', 'all');
    // Draw all dots in a separate pass so they're all on top
      // Draw dots with click events - only for non-zero values
      const dotsData = this.data.filter(d => {
        return value != null && value > 0;
      const dots = dotsGroup.selectAll(`.chart-dot--${metric}`)
        .data(dotsData)
        .enter().append('circle')
        .attr('class', `chart-dot chart-dot--${metric}`)
        .attr('cx', (d: TrendData) => xScale(d.timestamp))
        .attr('cy', (d: TrendData) => {
        .attr('r', 4) // Slightly larger for easier clicking
        .attr('stroke', color)
        .attr('stroke-width', 2)
        .style('pointer-events', 'all') // Ensure clicks are captured
        .style('z-index', 1000) // Ensure dots are on top
        .attr('data-metric', metric) // Add data attribute for debugging
        .on('mouseenter', (event: MouseEvent, d: TrendData) => {
          // Bring to front on hover
          const dot = d3.select(event.currentTarget as SVGCircleElement);
          dot.raise();
          dot.attr('r', 6);
          if (this.config.showTooltip !== false) {
            this.showTooltip(event, d);
        .on('mouseleave', (event: MouseEvent, d: TrendData) => {
          dot.attr('r', 4);
          // Hide tooltip
            this.hideTooltip();
        .on('click', (event: MouseEvent, d: TrendData) => {
          // Only emit if there's actual data
          if (value != null && value > 0) {
            this.dataPointClick.emit({ data: d, metric, event });
  private drawInteractiveElements(g: any, xScale: any, scales: any) {
    if (this.config.showTooltip === false) return;
    // Note: We don't need an overlay for tooltips as the dots themselves handle interactions
    // The overlay was preventing click events on dots
  private findClosestDataPoint(targetDate: Date): TrendData | null {
    if (!this.data.length) return null;
    return this.data.reduce((closest, current) => {
      const currentDiff = Math.abs(current.timestamp.getTime() - targetDate.getTime());
      const closestDiff = Math.abs(closest.timestamp.getTime() - targetDate.getTime());
      return currentDiff < closestDiff ? current : closest;
  private showTooltip(event: MouseEvent, data: TrendData) {
      <div><strong>${d3.timeFormat('%H:%M')(data.timestamp)}</strong></div>
      <div>Executions: ${data.executions.toLocaleString()}</div>
      <div>Cost: $${data.cost.toFixed(4)}</div>
      <div>Tokens: ${data.tokens.toLocaleString()}</div>
      <div>Avg Time: ${(data.avgTime / 1000).toFixed(1)}s</div>
      <div>Errors: ${data.errors}</div>
      <div style="margin-top: 8px; padding-top: 8px; border-top: 1px solid rgba(255,255,255,0.3); font-size: 11px; color: rgba(255,255,255,0.8);">
        Click data points to drill down
  private getMetricValue(data: TrendData, metric: string): number | null {
  getMetricColor(metric: string): string {
    const colors = this.config.colors || this.defaultColors;
    const index = this.visibleMetrics.indexOf(metric);
  getMetricLabel(metric: string): string {
      cost: 'Cost ($)',
      avgTime: 'Avg Time (ms)',
  isMetricVisible(metric: string): boolean {
    return !this.hiddenMetrics.has(metric);
  toggleMetric(metric: string): void {
    if (this.hiddenMetrics.has(metric)) {
      this.hiddenMetrics.delete(metric);
      this.hiddenMetrics.add(metric);
  private getTimeFormat(): (date: Date) => string {
    if (this.data.length < 2) {
      return d3.timeFormat('%H:%M');
    // Calculate the time span of the data
    const firstDate = this.data[0].timestamp;
    const lastDate = this.data[this.data.length - 1].timestamp;
    const timeDiff = lastDate.getTime() - firstDate.getTime();
    const hours = timeDiff / (1000 * 60 * 60);
    const days = hours / 24;
    // Choose format based on time span
      // For up to 24 hours, show hours and minutes
    } else if (days <= 7) {
      // For up to 7 days, show day and time
      return d3.timeFormat('%a %H:%M'); // e.g., "Mon 14:00"
    } else if (days <= 30) {
      // For up to 30 days, show month/day
      return d3.timeFormat('%m/%d'); // e.g., "06/13"
      // For longer periods, show month/day/year
      return d3.timeFormat('%m/%d/%y'); // e.g., "06/13/25"
  private getOptimalTickCount(): number {
    // Adjust tick count based on time span and chart width
    const pixelsPerTick = 100; // Minimum pixels between ticks
    const maxTicks = Math.floor(this.width / pixelsPerTick);
    if (days <= 1) {
      return Math.min(8, maxTicks); // Show more ticks for hourly data
      return Math.min(7, maxTicks); // One per day for weekly view
      return Math.min(6, maxTicks); // Fewer ticks for monthly view
      return Math.min(5, maxTicks); // Even fewer for longer periods
