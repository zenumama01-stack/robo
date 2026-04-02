import * as d3Scale from 'd3-scale';
import * as d3Shape from 'd3-shape';
import { SVGActionResult, ViewBox, Branding } from './shared/svg-types';
import { getPalette, generateCSS, getFontSpec, getColorForIndex } from './shared/svg-theming';
 * Chart data point interface
interface ChartDataPoint {
    /** Label or category name */
    /** X-axis value */
    x?: number | string;
    /** Y-axis value */
    y?: number;
    /** Value (for pie charts or single-value points) */
    value?: number;
    /** Optional category for grouping/coloring */
 * Action that generates SVG charts from data using D3.
 * Supports bar, line, pie, scatter, and area charts.
 * This action is designed for AI agents and workflows to create publication-quality
 * visualizations from structured data without writing visualization code.
 * // Simple bar chart
 *   ActionName: 'Create SVG Chart',
 *     { Name: 'ChartType', Value: 'bar' },
 *     { Name: 'Data', Value: JSON.stringify([
 *       { label: 'A', value: 28 },
 *       { label: 'B', value: 55 },
 *       { label: 'C', value: 43 }
 *     ]) },
 *     { Name: 'Title', Value: 'Sample Bar Chart' }
 * // Line chart with X/Y data
 *     { Name: 'ChartType', Value: 'line' },
 *       { x: 1, y: 10 },
 *       { x: 2, y: 25 },
 *       { x: 3, y: 15 }
 *     ]) }
 * // Pie chart
 *     { Name: 'ChartType', Value: 'pie' },
 *       { label: 'LLM', value: 67 },
 *       { label: 'Embeddings', value: 10 }
@RegisterClass(BaseAction, "__CreateSVGChart")
export class CreateSVGChartAction extends BaseAction {
     * Generates an SVG chart from the provided data and configuration
     *   - ChartType: Type of chart (bar, line, pie, scatter, area)
     *   - Data: JSON array of data objects
     *   - Title: Chart title (optional)
     *   - XAxisLabel: X-axis label (optional)
     *   - YAxisLabel: Y-axis label (optional)
     *   - Width: Chart width in pixels (optional, default: 800)
     *   - Height: Chart height in pixels (optional, default: 600)
     *   - Palette: Color palette name (optional, default: 'mjDefault')
     *   - ShowGrid: Show grid lines (optional, default: false)
     *   - ShowLegend: Show legend (optional, default: false)
     *   - Success: true if chart was generated successfully
     *   - Message: The SVG string or error message
    protected async InternalRunAction(params: RunActionParams): Promise<SVGActionResult> {
            // Extract chart type
            const chartTypeParam = this.getParamValue(params, 'ChartType');
            if (!chartTypeParam) {
                    Message: "ChartType parameter is required (bar, line, pie, scatter, area)",
            const chartType = this.ensureString(chartTypeParam, 'ChartType').toLowerCase();
            const validTypes = ['bar', 'line', 'pie', 'scatter', 'area'];
            if (!validTypes.includes(chartType)) {
                    Message: `Invalid ChartType: ${chartType}. Valid types: ${validTypes.join(', ')}`,
                    ResultCode: "INVALID_CHART_TYPE"
            const dataParam = this.getParamValue(params, 'Data');
            if (!dataParam) {
            const data = this.parseJSON<ChartDataPoint[]>(dataParam, 'Data');
            if (!Array.isArray(data) || data.length === 0) {
                    Message: "Data must be a non-empty JSON array",
                    ResultCode: "INVALID_DATA"
            // Parse common parameters
            const width = parseInt(this.ensureString(this.getParamValue(params, 'Width') || '800', 'Width'));
            const height = parseInt(this.ensureString(this.getParamValue(params, 'Height') || '600', 'Height'));
            const title = this.ensureString(this.getParamValue(params, 'Title') || '', 'Title');
            const xAxisLabel = this.ensureString(this.getParamValue(params, 'XAxisLabel') || '', 'XAxisLabel');
            const yAxisLabel = this.ensureString(this.getParamValue(params, 'YAxisLabel') || '', 'YAxisLabel');
            const paletteName = this.ensureString(this.getParamValue(params, 'Palette') || 'mjDefault', 'Palette');
            const showGridParam = this.getParamValue(params, 'ShowGrid');
            const showGrid = showGridParam ? this.ensureString(showGridParam, 'ShowGrid').toLowerCase() === 'true' : false;
            const showLegendParam = this.getParamValue(params, 'ShowLegend');
            const showLegend = showLegendParam ? this.ensureString(showLegendParam, 'ShowLegend').toLowerCase() === 'true' : false;
            // Create branding configuration
            const branding: Branding = {
                palette: { type: 'named', name: paletteName as any }
            // Create viewBox configuration
            const viewBox: ViewBox = {
                height,
                padding: { top: title ? 60 : 40, right: 40, bottom: xAxisLabel ? 80 : 60, left: yAxisLabel ? 80 : 60 }
            // Generate chart based on type
            let svg: string;
            switch (chartType) {
                    svg = await this.renderBarChart(data, viewBox, branding, title, xAxisLabel, yAxisLabel, showGrid, showLegend);
                    svg = await this.renderLineChart(data, viewBox, branding, title, xAxisLabel, yAxisLabel, showGrid, showLegend);
                case 'area':
                    svg = await this.renderAreaChart(data, viewBox, branding, title, xAxisLabel, yAxisLabel, showGrid, showLegend);
                    svg = await this.renderPieChart(data, viewBox, branding, title, showLegend);
                case 'scatter':
                    svg = await this.renderScatterChart(data, viewBox, branding, title, xAxisLabel, yAxisLabel, showGrid, showLegend);
                        Message: `Unsupported chart type: ${chartType}`,
                Message: svg,
                svg,
                height
                Message: `Failed to generate chart: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: "CHART_GENERATION_FAILED"
     * Renders a bar chart
    private async renderBarChart(
        data: ChartDataPoint[],
        viewBox: ViewBox,
        branding: Branding,
        xAxisLabel: string,
        yAxisLabel: string,
        showGrid: boolean,
        _showLegend: boolean
        const vb = SVGUtils.calculateViewBox(viewBox);
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'bar-chart');
        const svg = doc.querySelector('svg')!;
        const ns = svg.namespaceURI!;
        // Add accessibility and styles
            SVGUtils.addA11y(svg, { title, ariaRole: 'img' });
        SVGUtils.addStyles(svg, generateCSS(branding));
        const palette = getPalette(branding.palette);
        const font = getFontSpec(branding.font);
        // Extract labels and values
        const labels = data.map(d => d.label || String(d.x || ''));
        const values = data.map(d => d.value || d.y || 0);
        const maxValue = Math.max(...values);
        const minValue = Math.min(0, Math.min(...values)); // Include 0 in scale
        // Create scales
        const xScale = d3Scale.scaleBand()
            .domain(labels)
            .range([vb.x, vb.x + vb.contentWidth])
        const yScale = d3Scale.scaleLinear()
            .domain([minValue, maxValue])
            .range([vb.y + vb.contentHeight, vb.y])
            .nice();
        // Create chart group
        const chartGroup = doc.createElementNS(ns, 'g');
        chartGroup.setAttribute('id', 'chart-content');
        svg.appendChild(chartGroup);
        // Draw grid if requested
        if (showGrid) {
            this.drawGrid(doc, chartGroup, xScale, yScale, vb, palette, 'vertical');
        // Draw bars
        data.forEach((d, i) => {
            const value = d.value || d.y || 0;
            const label = d.label || String(d.x || '');
            const x = xScale(label)!;
            const barWidth = xScale.bandwidth();
            const barHeight = Math.abs(yScale(value) - yScale(0));
            const y = value >= 0 ? yScale(value) : yScale(0);
            const rect = doc.createElementNS(ns, 'rect');
            rect.setAttribute('x', String(x));
            rect.setAttribute('y', String(y));
            rect.setAttribute('width', String(barWidth));
            rect.setAttribute('height', String(barHeight));
            rect.setAttribute('fill', getColorForIndex(i, branding.palette));
            rect.setAttribute('stroke', 'none');
            rect.setAttribute('rx', '2');
            chartGroup.appendChild(rect);
            // Add value label on top of bar
            const valueText = doc.createElementNS(ns, 'text');
            valueText.setAttribute('x', String(x + barWidth / 2));
            valueText.setAttribute('y', String(y - 5));
            valueText.setAttribute('text-anchor', 'middle');
            valueText.setAttribute('font-family', font.family);
            valueText.setAttribute('font-size', String(font.size - 2));
            valueText.setAttribute('fill', palette.foreground);
            valueText.textContent = String(Math.round(value * 100) / 100);
            chartGroup.appendChild(valueText);
        // Draw axes
        this.drawXAxis(doc, svg, xScale, vb, palette, font, xAxisLabel);
        this.drawYAxis(doc, svg, yScale, vb, palette, font, yAxisLabel);
            this.addTitle(doc, svg, title, viewBox.width, font);
        return SVGUtils.sanitizeSVG(svg.outerHTML);
     * Renders a line chart
    private async renderLineChart(
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'line-chart');
        // Extract x and y values
        const xValues = data.map(d => typeof d.x === 'number' ? d.x : parseFloat(String(d.x || 0)));
        const yValues = data.map(d => d.y || d.value || 0);
        const xScale = d3Scale.scaleLinear()
            .domain([Math.min(...xValues), Math.max(...xValues)])
            .domain([Math.min(...yValues), Math.max(...yValues)])
            this.drawGrid(doc, chartGroup, xScale, yScale, vb, palette, 'both');
        // Create line generator
        const lineGenerator = d3Shape.line<ChartDataPoint>()
            .x(d => xScale(typeof d.x === 'number' ? d.x : parseFloat(String(d.x || 0))))
            .y(d => yScale(d.y || d.value || 0));
        // Draw line
        const linePath = doc.createElementNS(ns, 'path');
        linePath.setAttribute('d', lineGenerator(data) || '');
        linePath.setAttribute('fill', 'none');
        linePath.setAttribute('stroke', getColorForIndex(0, branding.palette));
        linePath.setAttribute('stroke-width', '3');
        linePath.setAttribute('stroke-linecap', 'round');
        linePath.setAttribute('stroke-linejoin', 'round');
        chartGroup.appendChild(linePath);
        // Draw data points
        data.forEach((d) => {
            const xVal = typeof d.x === 'number' ? d.x : parseFloat(String(d.x || 0));
            const yVal = d.y || d.value || 0;
            const circle = doc.createElementNS(ns, 'circle');
            circle.setAttribute('cx', String(xScale(xVal)));
            circle.setAttribute('cy', String(yScale(yVal)));
            circle.setAttribute('r', '5');
            circle.setAttribute('fill', getColorForIndex(0, branding.palette));
            circle.setAttribute('stroke', '#fff');
            circle.setAttribute('stroke-width', '2');
            chartGroup.appendChild(circle);
        this.drawXAxisNumeric(doc, svg, xScale, vb, palette, font, xAxisLabel);
     * Renders an area chart
    private async renderAreaChart(
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'area-chart');
            .domain([0, Math.max(...yValues)]) // Start from 0 for area charts
        // Create area generator
        const areaGenerator = d3Shape.area<ChartDataPoint>()
            .y0(yScale(0))
            .y1(d => yScale(d.y || d.value || 0));
        // Draw area
        const areaPath = doc.createElementNS(ns, 'path');
        areaPath.setAttribute('d', areaGenerator(data) || '');
        const areaColor = getColorForIndex(0, branding.palette);
        areaPath.setAttribute('fill', areaColor);
        areaPath.setAttribute('fill-opacity', '0.3');
        areaPath.setAttribute('stroke', 'none');
        chartGroup.appendChild(areaPath);
        // Draw line on top
        linePath.setAttribute('stroke', areaColor);
        linePath.setAttribute('stroke-width', '2');
     * Renders a pie chart
    private async renderPieChart(
        showLegend: boolean
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'pie-chart');
        // Calculate center and radius
        const centerX = vb.x + vb.contentWidth / 2;
        const centerY = vb.y + vb.contentHeight / 2;
        const radius = Math.min(vb.contentWidth, vb.contentHeight) / 2 - 20;
        // Create pie layout
        const pieGenerator = d3Shape.pie<ChartDataPoint>()
            .value(d => d.value || 0)
            .sort(null);
        const arcGenerator = d3Shape.arc<d3Shape.PieArcDatum<ChartDataPoint>>()
            .innerRadius(0)
            .outerRadius(radius);
        const labelArcGenerator = d3Shape.arc<d3Shape.PieArcDatum<ChartDataPoint>>()
            .innerRadius(radius * 0.6)
            .outerRadius(radius * 0.6);
        chartGroup.setAttribute('transform', `translate(${centerX}, ${centerY})`);
        // Generate pie slices
        const arcs = pieGenerator(data);
        const total = data.reduce((sum, d) => sum + (d.value || 0), 0);
        arcs.forEach((arc, i) => {
            // Draw slice
            const path = doc.createElementNS(ns, 'path');
            path.setAttribute('d', arcGenerator(arc) || '');
            path.setAttribute('fill', getColorForIndex(i, branding.palette));
            chartGroup.appendChild(path);
            // Add percentage label if slice is large enough
            const percentage = ((arc.data.value || 0) / total) * 100;
            if (percentage > 5) {
                const centroid = labelArcGenerator.centroid(arc);
                const text = doc.createElementNS(ns, 'text');
                text.setAttribute('x', String(centroid[0]));
                text.setAttribute('y', String(centroid[1]));
                text.setAttribute('font-family', font.family);
                text.setAttribute('font-size', String(font.size));
                text.textContent = `${Math.round(percentage)}%`;
                chartGroup.appendChild(text);
        // Add legend if requested
        if (showLegend) {
            this.addLegend(doc, svg, data, branding, vb, font);
     * Renders a scatter plot
    private async renderScatterChart(
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'scatter-chart');
        // Draw scatter points
            circle.setAttribute('r', '6');
            circle.setAttribute('fill', getColorForIndex(i, branding.palette));
            circle.setAttribute('fill-opacity', '0.7');
            circle.setAttribute('stroke', getColorForIndex(i, branding.palette));
     * Draws grid lines
    private drawGrid(
        doc: Document,
        container: Element,
        xScale: any,
        yScale: d3Scale.ScaleLinear<number, number>,
        vb: ReturnType<typeof SVGUtils.calculateViewBox>,
        palette: ReturnType<typeof getPalette>,
        direction: 'horizontal' | 'vertical' | 'both'
        const ns = container.namespaceURI!;
        const gridGroup = doc.createElementNS(ns, 'g');
        gridGroup.setAttribute('class', 'grid');
        container.appendChild(gridGroup);
        // Horizontal grid lines (for Y axis)
        if (direction === 'horizontal' || direction === 'both') {
            const yTicks = yScale.ticks(5);
            yTicks.forEach(tick => {
                const line = doc.createElementNS(ns, 'line');
                line.setAttribute('x1', String(vb.x));
                line.setAttribute('y1', String(yScale(tick)));
                line.setAttribute('x2', String(vb.x + vb.contentWidth));
                line.setAttribute('y2', String(yScale(tick)));
                line.setAttribute('stroke', palette.foreground);
                line.setAttribute('stroke-opacity', '0.1');
                line.setAttribute('stroke-dasharray', '2,2');
                gridGroup.appendChild(line);
        // Vertical grid lines (for X axis)
        if (direction === 'vertical' || direction === 'both') {
            if (typeof xScale.ticks === 'function') {
                const xTicks = xScale.ticks(5);
                xTicks.forEach((tick: number) => {
                    line.setAttribute('x1', String(xScale(tick)));
                    line.setAttribute('y1', String(vb.y));
                    line.setAttribute('x2', String(xScale(tick)));
                    line.setAttribute('y2', String(vb.y + vb.contentHeight));
     * Draws X-axis for categorical data
    private drawXAxis(
        svg: SVGElement,
        xScale: d3Scale.ScaleBand<string>,
        font: Required<import('./shared/svg-types').FontSpec>,
        label: string
        const axisGroup = doc.createElementNS(ns, 'g');
        axisGroup.setAttribute('class', 'x-axis');
        svg.appendChild(axisGroup);
        // Draw axis line
        const axisLine = doc.createElementNS(ns, 'line');
        axisLine.setAttribute('x1', String(vb.x));
        axisLine.setAttribute('y1', String(vb.y + vb.contentHeight));
        axisLine.setAttribute('x2', String(vb.x + vb.contentWidth));
        axisLine.setAttribute('y2', String(vb.y + vb.contentHeight));
        axisLine.setAttribute('stroke', palette.foreground);
        axisLine.setAttribute('stroke-width', '2');
        axisGroup.appendChild(axisLine);
        // Draw labels
        xScale.domain().forEach(label => {
            const x = xScale(label)! + xScale.bandwidth() / 2;
            text.setAttribute('x', String(x));
            text.setAttribute('y', String(vb.y + vb.contentHeight + 20));
            text.setAttribute('fill', palette.foreground);
            text.textContent = label;
            axisGroup.appendChild(text);
        // Add axis label if provided
            const labelText = doc.createElementNS(ns, 'text');
            labelText.setAttribute('x', String(vb.x + vb.contentWidth / 2));
            labelText.setAttribute('y', String(vb.y + vb.contentHeight + 50));
            labelText.setAttribute('text-anchor', 'middle');
            labelText.setAttribute('font-family', font.family);
            labelText.setAttribute('font-size', String(font.size + 2));
            labelText.setAttribute('font-weight', '600');
            labelText.setAttribute('fill', palette.foreground);
            labelText.textContent = label;
            axisGroup.appendChild(labelText);
     * Draws X-axis for numeric data
    private drawXAxisNumeric(
        xScale: d3Scale.ScaleLinear<number, number>,
        // Draw ticks and labels
        const ticks = xScale.ticks(5);
        ticks.forEach(tick => {
            const x = xScale(tick);
            // Tick mark
            const tickLine = doc.createElementNS(ns, 'line');
            tickLine.setAttribute('x1', String(x));
            tickLine.setAttribute('y1', String(vb.y + vb.contentHeight));
            tickLine.setAttribute('x2', String(x));
            tickLine.setAttribute('y2', String(vb.y + vb.contentHeight + 5));
            tickLine.setAttribute('stroke', palette.foreground);
            tickLine.setAttribute('stroke-width', '1');
            axisGroup.appendChild(tickLine);
            text.textContent = String(Math.round(tick * 100) / 100);
     * Draws Y-axis
    private drawYAxis(
        axisGroup.setAttribute('class', 'y-axis');
        axisLine.setAttribute('y1', String(vb.y));
        axisLine.setAttribute('x2', String(vb.x));
        const ticks = yScale.ticks(5);
            const y = yScale(tick);
            tickLine.setAttribute('x1', String(vb.x - 5));
            tickLine.setAttribute('y1', String(y));
            tickLine.setAttribute('x2', String(vb.x));
            tickLine.setAttribute('y2', String(y));
            text.setAttribute('x', String(vb.x - 10));
            text.setAttribute('y', String(y));
            text.setAttribute('text-anchor', 'end');
            labelText.setAttribute('x', String(vb.x - 50));
            labelText.setAttribute('y', String(vb.y + vb.contentHeight / 2));
            labelText.setAttribute('transform', `rotate(-90, ${vb.x - 50}, ${vb.y + vb.contentHeight / 2})`);
     * Adds a title to the chart
    private addTitle(
        width: number,
        font: Required<import('./shared/svg-types').FontSpec>
        text.setAttribute('x', String(width / 2));
        text.setAttribute('y', '30');
        text.setAttribute('font-size', String(font.size + 6));
        text.textContent = title;
     * Adds a legend for pie charts
    private addLegend(
        const legendGroup = doc.createElementNS(ns, 'g');
        legendGroup.setAttribute('class', 'legend');
        legendGroup.setAttribute('transform', `translate(${vb.x + vb.contentWidth + 20}, ${vb.y})`);
        svg.appendChild(legendGroup);
            const g = doc.createElementNS(ns, 'g');
            g.setAttribute('transform', `translate(0, ${i * 25})`);
            // Color box
            rect.setAttribute('width', '15');
            rect.setAttribute('height', '15');
            g.appendChild(rect);
            text.setAttribute('x', '20');
            text.setAttribute('y', '12');
            text.textContent = d.label || `Item ${i + 1}`;
            g.appendChild(text);
            legendGroup.appendChild(g);
     * Helper to safely parse JSON
    private parseJSON<T>(value: any, paramName: string): T {
     * Helper to ensure a parameter value is a string
    private ensureString(value: any, paramName: string): string {
        if (typeof value === 'number' || typeof value === 'boolean') {
            `Parameter '${paramName}' must be a string, number, or boolean. ` +
            `Received ${typeof value}. If providing JSON data, ensure it's passed as a string.`
