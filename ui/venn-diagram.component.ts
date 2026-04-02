  AfterViewInit,
import { VennData, VennSet, VennIntersection } from '../../services/list-set-operations.service';
 * Event emitted when a Venn region is clicked
export interface VennRegionClickEvent {
  intersection: VennIntersection;
  recordIds: string[];
 * Interactive D3-based Venn diagram component for visualizing list overlaps.
 * - Proportional circle sizing based on list size
 * - Animated transitions
 * - Hover highlighting
 * - Click to select regions
 * - Responsive sizing
  selector: 'mj-venn-diagram',
    <div class="venn-container" #vennContainer>
      <svg #vennSvg class="venn-svg"></svg>
      <!-- Tooltip -->
      <div class="venn-tooltip" [class.visible]="tooltipVisible"
        [style.left.px]="tooltipX" [style.top.px]="tooltipY">
        <div class="tooltip-title">{{ tooltipTitle }}</div>
        <div class="tooltip-count">{{ tooltipCount }} records</div>
      @if (data && data.sets.length > 0) {
        <div class="venn-legend">
          @for (set of data.sets; track set) {
              <div class="legend-color" [style.background-color]="set.color"></div>
                <span class="legend-name">{{ set.listName }}</span>
                <span class="legend-count">{{ set.size }} items</span>
      @if (!data || data.sets.length === 0) {
          <p>Add lists to visualize their overlaps</p>
    .venn-container {
      background: linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%);
    .venn-svg {
    .venn-tooltip {
      transform: translateY(8px);
      transition: opacity 0.15s, transform 0.15s;
    .venn-tooltip.visible {
    .tooltip-title {
    .tooltip-count {
    .venn-legend {
      left: 16px;
      background: rgba(255, 255, 255, 0.95);
    .legend-count {
    /* SVG styles */
    :host ::ng-deep .venn-circle {
    :host ::ng-deep .venn-circle:hover {
    :host ::ng-deep .venn-label {
      fill: white;
    :host ::ng-deep .intersection-region {
    :host ::ng-deep .intersection-region:hover {
      fill: rgba(0,0,0,0.75);
    :host ::ng-deep .intersection-region.selected {
      fill: rgba(156, 39, 176, 0.9);
      stroke: #7B1FA2;
    :host ::ng-deep .intersection-label-group {
    :host ::ng-deep .intersection-label-group:hover {
export class VennDiagramComponent implements AfterViewInit, OnChanges, OnDestroy {
  @ViewChild('vennContainer') containerRef!: ElementRef<HTMLDivElement>;
  @ViewChild('vennSvg') svgRef!: ElementRef<SVGSVGElement>;
  @Input() data: VennData | null = null;
  @Input() selectedRegion: VennIntersection | null = null;
  @Output() regionClick = new EventEmitter<VennRegionClickEvent>();
  @Output() regionHover = new EventEmitter<VennIntersection | null>();
  // Tooltip state
  tooltipVisible = false;
  tooltipX = 0;
  tooltipY = 0;
  tooltipTitle = '';
  tooltipCount = 0;
  // Track if hovering an intersection label (to suppress circle tooltips)
  private isHoveringIntersectionLabel = false;
  private svg: d3.Selection<SVGSVGElement, unknown, null, undefined> | null = null;
  private resizeObserver: ResizeObserver | null = null;
    this.initializeSvg();
    this.setupResizeObserver();
    this.render();
    if ((changes['data'] || changes['selectedRegion']) && this.svg) {
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
  private initializeSvg(): void {
    if (!this.svgRef) return;
    this.svg = d3.select(this.svgRef.nativeElement);
    this.updateDimensions();
  private setupResizeObserver(): void {
    if (!this.containerRef) return;
    this.resizeObserver = new ResizeObserver(() => {
    this.resizeObserver.observe(this.containerRef.nativeElement);
  private updateDimensions(): void {
    const rect = this.containerRef.nativeElement.getBoundingClientRect();
    this.width = rect.width;
    this.height = rect.height;
  private render(): void {
    if (!this.svg || !this.data || this.data.sets.length === 0) {
    // Clear previous content
    const margin = { top: 40, right: 40, bottom: 80, left: 40 };
    const drawWidth = this.width - margin.left - margin.right;
    const drawHeight = this.height - margin.top - margin.bottom;
    if (drawWidth <= 0 || drawHeight <= 0) return;
    const g = this.svg
    // Render based on number of sets
    const sets = this.data.sets;
    if (sets.length === 1) {
      this.renderSingleSet(g, sets[0], drawWidth, drawHeight);
    } else if (sets.length === 2) {
      this.renderTwoSets(g, sets, drawWidth, drawHeight);
    } else if (sets.length === 3) {
      this.renderThreeSets(g, sets, drawWidth, drawHeight);
    } else if (sets.length === 4) {
      this.renderFourSets(g, sets, drawWidth, drawHeight);
      this.renderMultipleSets(g, sets, drawWidth, drawHeight);
  private renderSingleSet(
    g: d3.Selection<SVGGElement, unknown, null, undefined>,
    set: VennSet,
    height: number
    const cx = width / 2;
    const cy = height / 2;
    const radius = Math.min(width, height) * 0.35;
    // Draw circle
    g.append('circle')
      .attr('class', 'venn-circle')
      .attr('r', radius)
      .attr('fill', set.color)
      .attr('opacity', 0.6)
      .on('mouseenter', (event: MouseEvent) => this.showCircleTooltip(event, set.listName, set.size))
      .on('mousemove', (event: MouseEvent) => this.moveTooltip(event))
      .on('mouseleave', () => this.hideTooltip())
      .on('click', () => this.onCircleClick(set));
    // Draw label
      .attr('class', 'venn-label')
      .attr('x', cx)
      .attr('y', cy)
      .text(set.size.toString());
  private renderTwoSets(
    sets: VennSet[],
    // Calculate proportional radii based on list sizes
    const { radii, baseRadius } = this.calculateProportionalRadii(sets, width, height, 2);
    const r1 = radii[0];
    const r2 = radii[1];
    // Calculate overlap to ensure visual intersection
    const avgRadius = (r1 + r2) / 2;
    const overlap = avgRadius * 0.5;
    const positions = [
      { cx: width / 2 - r1 + overlap / 2, r: r1, set: sets[0] },
      { cx: width / 2 + r2 - overlap / 2, r: r2, set: sets[1] }
    // Draw circles
    for (const pos of positions) {
        .attr('cx', pos.cx)
        .attr('r', pos.r)
        .attr('fill', pos.set.color)
        .attr('opacity', 0.5)
        .on('mouseenter', (event: MouseEvent) => this.showCircleTooltip(event, pos.set.listName, pos.set.size))
        .on('click', () => this.onCircleClick(pos.set));
    // Find intersection data
    const intersection = this.data?.intersections.find(
      i => i.setIds.length === 2 && i.setIds.includes(sets[0].listId) && i.setIds.includes(sets[1].listId)
    // Draw intersection labels
    const leftOnlyIntersection = this.data?.intersections.find(
      i => i.setIds.length === 1 && i.setIds[0] === sets[0].listId
    const rightOnlyIntersection = this.data?.intersections.find(
      i => i.setIds.length === 1 && i.setIds[0] === sets[1].listId
    // Left only - clickable region
    if (leftOnlyIntersection) {
      this.addClickableLabel(g, positions[0].cx - positions[0].r * 0.3, cy, leftOnlyIntersection);
    // Right only - clickable region
    if (rightOnlyIntersection) {
      this.addClickableLabel(g, positions[1].cx + positions[1].r * 0.3, cy, rightOnlyIntersection);
    // Intersection - clickable region (center between the two circles)
    if (intersection && intersection.size > 0) {
      this.addClickableLabel(g, width / 2, cy, intersection);
   * Add a clickable label with background for intersection regions
  private addClickableLabel(
    intersection: VennIntersection
    const labelGroup = g.append('g')
      .attr('class', 'intersection-label-group')
      .style('cursor', 'pointer');
    // Add background circle/pill for better click target
    const text = intersection.size.toString();
    const padding = 8;
    const fontSize = intersection.setIds.length >= 3 ? 14 : 12;
    const rectWidth = Math.max(text.length * fontSize * 0.7 + padding * 2, 32);
    // The rect is the main hover/click target
    const rect = labelGroup.append('rect')
      .attr('x', x - rectWidth / 2)
      .attr('y', y - fontSize / 2 - padding / 2)
      .attr('width', rectWidth)
      .attr('height', fontSize + padding)
      .attr('rx', (fontSize + padding) / 2)
      .attr('ry', (fontSize + padding) / 2)
      .attr('fill', 'rgba(0,0,0,0.6)')
      .attr('class', this.isRegionSelected(intersection) ? 'intersection-region selected' : 'intersection-region');
    // Text has pointer-events: none to prevent hover flickering
    labelGroup.append('text')
      .attr('y', y + fontSize * 0.35)
      .style('font-size', `${fontSize}px`)
      .text(text);
    // Attach events to the rect (the main clickable area)
    // Show tooltip for intersection labels with proper event handling to prevent flickering
    rect
      .on('mouseenter', (event: MouseEvent) => {
        // Set flag to suppress circle tooltips and prevent their mousemove from calling detectChanges
        this.isHoveringIntersectionLabel = true;
        // Hide any existing tooltip immediately without triggering detectChanges
        this.tooltipVisible = false;
        // Show the label's tooltip
        this.showLabelTooltip(event, intersection);
      .on('mousemove', (event: MouseEvent) => {
        // Update tooltip position without calling detectChanges (avoid flicker)
        this.moveLabelTooltipPosition(event);
      .on('mouseleave', (event: MouseEvent) => {
        this.isHoveringIntersectionLabel = false;
      .on('click', (event: MouseEvent) => {
        this.onIntersectionClick(intersection);
   * Check if a region is currently selected
  private isRegionSelected(intersection: VennIntersection): boolean {
    if (!this.selectedRegion) return false;
    return this.selectedRegion.setIds.length === intersection.setIds.length &&
           this.selectedRegion.setIds.every(id => intersection.setIds.includes(id));
   * Calculate proportional radii using a small/medium/large approach
   * This avoids extreme size differences (e.g., 1000 vs 10 items)
  private calculateProportionalRadii(
    numSets: number
  ): { radii: number[]; baseRadius: number } {
    // Determine base radius based on number of sets
    let baseRadius: number;
    if (numSets === 1) {
      baseRadius = Math.min(width, height) * 0.35;
    } else if (numSets === 2) {
      baseRadius = Math.min(width / 2.5, height / 2) * 0.8;
    } else if (numSets === 3) {
      baseRadius = Math.min(width, height) * 0.28;
      const cols = Math.ceil(Math.sqrt(numSets));
      const cellSize = Math.min(width / cols, height / cols);
      baseRadius = cellSize * 0.35;
    // Find the size range
    const sizes = sets.map(s => s.size);
    const maxSize = Math.max(...sizes, 1);
    const minSize = Math.min(...sizes, 1);
    // If all same size or very close, use same radius
    if (maxSize === minSize || maxSize / minSize < 1.5) {
        radii: sets.map(() => baseRadius),
        baseRadius
    // Use logarithmic scaling with bounds for visual balance
    // Scale factor ranges from 0.7 to 1.0 based on relative size
    const minScale = 0.7;
    const maxScale = 1.0;
    const radii = sets.map(set => {
      // Use log scale for smoother visual representation
      const logMin = Math.log(minSize + 1);
      const logMax = Math.log(maxSize + 1);
      const logSize = Math.log(set.size + 1);
      const normalized = (logSize - logMin) / (logMax - logMin);
      // Scale to minScale-maxScale range
      const scale = minScale + normalized * (maxScale - minScale);
      return baseRadius * scale;
    return { radii, baseRadius };
  private renderThreeSets(
    const { radii, baseRadius } = this.calculateProportionalRadii(sets, width, height, 3);
    const avgRadius = radii.reduce((a, b) => a + b, 0) / radii.length;
    const offset = avgRadius * 0.65; // Distance from center
    // Position circles in a triangle with individual radii
      { cx: cx, cy: cy - offset * 0.8, r: radii[0], set: sets[0] }, // Top
      { cx: cx - offset * 0.9, cy: cy + offset * 0.5, r: radii[1], set: sets[1] }, // Bottom left
      { cx: cx + offset * 0.9, cy: cy + offset * 0.5, r: radii[2], set: sets[2] } // Bottom right
        .attr('cy', pos.cy)
    // Draw clickable intersection labels
    for (const intersection of this.data?.intersections || []) {
      const labelPos = this.getIntersectionLabelPosition(intersection, positions, cx, cy, avgRadius);
      if (labelPos && intersection.size > 0) {
        this.addClickableLabel(g, labelPos.x, labelPos.y, intersection);
  private getIntersectionLabelPosition(
    intersection: VennIntersection,
    positions: Array<{ cx: number; cy: number; r?: number; set: VennSet }>,
    cx: number,
    cy: number,
  ): { x: number; y: number } | null {
    const n = intersection.setIds.length;
    if (n === 1) {
      // Find the single set position
      const pos = positions.find(p => p.set.listId === intersection.setIds[0]);
      if (!pos) return null;
      // Use individual radius if available, otherwise use provided radius
      const r = pos.r || radius;
      // Move label towards the outer edge, away from center
      const dx = pos.cx - cx;
      const dy = pos.cy - cy;
      const dist = Math.sqrt(dx * dx + dy * dy);
      const offsetFactor = 0.6;
        x: pos.cx + (dx / dist) * r * offsetFactor,
        y: pos.cy + (dy / dist) * r * offsetFactor
    } else if (n === 2) {
      // Average of two positions
      const pos1 = positions.find(p => p.set.listId === intersection.setIds[0]);
      const pos2 = positions.find(p => p.set.listId === intersection.setIds[1]);
      if (!pos1 || !pos2) return null;
        x: (pos1.cx + pos2.cx) / 2,
        y: (pos1.cy + pos2.cy) / 2
    } else if (n === 3) {
      // Center of all three
      return { x: cx, y: cy };
   * Render a 4-set Venn diagram using overlapping ellipses.
   * This is a simplified approach - true 4-set Venn diagrams use complex curves.
   * We use two pairs of rotated ellipses to create all 15 possible regions.
  private renderFourSets(
    // Use ellipses for 4-set Venn
    const baseSize = Math.min(width, height) * 0.35;
    const rx = baseSize * 1.2; // Ellipse horizontal radius
    const ry = baseSize * 0.7; // Ellipse vertical radius
    // Position ellipses with rotations to create overlapping regions
    // Two ellipses rotated ~45 degrees and two rotated ~-45 degrees
      { cx: cx - baseSize * 0.3, cy: cy - baseSize * 0.15, rx, ry, rotation: 45, set: sets[0] },
      { cx: cx + baseSize * 0.3, cy: cy - baseSize * 0.15, rx, ry, rotation: -45, set: sets[1] },
      { cx: cx - baseSize * 0.3, cy: cy + baseSize * 0.15, rx, ry, rotation: -45, set: sets[2] },
      { cx: cx + baseSize * 0.3, cy: cy + baseSize * 0.15, rx, ry, rotation: 45, set: sets[3] }
    // Draw ellipses
      g.append('ellipse')
        .attr('rx', pos.rx)
        .attr('ry', pos.ry)
        .attr('transform', `rotate(${pos.rotation}, ${pos.cx}, ${pos.cy})`)
        .attr('opacity', 0.4)
    // For 4 sets, we need to calculate positions for all 15 regions
      const labelPos = this.getFourSetIntersectionPosition(intersection, positions, cx, cy, baseSize);
   * Calculate label position for 4-set Venn intersections
  private getFourSetIntersectionPosition(
    positions: Array<{ cx: number; cy: number; set: VennSet }>,
    baseSize: number
    const setIndices = intersection.setIds.map(id =>
      positions.findIndex(p => p.set.listId === id)
    ).filter(i => i >= 0);
    if (setIndices.length !== n) return null;
      // Single set - position at outer edge
      const pos = positions[setIndices[0]];
      const dist = Math.sqrt(dx * dx + dy * dy) || 1;
        x: pos.cx + (dx / dist) * baseSize * 0.6,
        y: pos.cy + (dy / dist) * baseSize * 0.6
      // Two sets - average position, offset from center
      const pos1 = positions[setIndices[0]];
      const pos2 = positions[setIndices[1]];
      const avgX = (pos1.cx + pos2.cx) / 2;
      const avgY = (pos1.cy + pos2.cy) / 2;
      // Offset slightly from the center
      const dx = avgX - cx;
      const dy = avgY - cy;
        x: avgX + (dx / dist) * baseSize * 0.2,
        y: avgY + (dy / dist) * baseSize * 0.2
      // Three sets - find the set NOT in the intersection
      const missingIndex = [0, 1, 2, 3].find(i => !setIndices.includes(i));
      if (missingIndex === undefined) return { x: cx, y: cy };
      // Position opposite to the missing set
      const missingPos = positions[missingIndex];
      const dx = cx - missingPos.cx;
      const dy = cy - missingPos.cy;
        x: cx + (dx / dist) * baseSize * 0.25,
        y: cy + (dy / dist) * baseSize * 0.25
    } else if (n === 4) {
      // All four sets - center
  private renderMultipleSets(
    // For more than 3 sets, use a grid layout
    const cols = Math.ceil(Math.sqrt(sets.length));
    const rows = Math.ceil(sets.length / cols);
    const cellWidth = width / cols;
    const cellHeight = height / rows;
    const radius = Math.min(cellWidth, cellHeight) * 0.35;
    sets.forEach((set, i) => {
      const col = i % cols;
      const row = Math.floor(i / cols);
      const cx = cellWidth * col + cellWidth / 2;
      const cy = cellHeight * row + cellHeight / 2;
      // Add list name below
        .attr('y', cy + radius + 16)
        .text(set.listName.length > 15 ? set.listName.substring(0, 15) + '...' : set.listName);
   * Show tooltip for circle/ellipse elements.
   * This is suppressed when hovering an intersection label.
  private showCircleTooltip(event: MouseEvent, title: string, count: number): void {
    // Don't show circle tooltip if hovering an intersection label
    if (this.isHoveringIntersectionLabel) {
    this.showTooltip(event, title, count);
  private showTooltip(event: MouseEvent, title: string, count: number): void {
    this.tooltipTitle = title;
    this.tooltipCount = count;
    this.tooltipVisible = true;
    this.moveTooltip(event);
  private moveTooltip(event: MouseEvent): void {
    // Skip updating if hovering an intersection label (prevents flicker from circle mousemove events)
    const container = this.containerRef?.nativeElement;
    const rect = container.getBoundingClientRect();
    this.tooltipX = event.clientX - rect.left + 10;
    this.tooltipY = event.clientY - rect.top - 30;
   * Show tooltip for intersection label with proper intersection name
  private showLabelTooltip(event: MouseEvent, intersection: VennIntersection): void {
    // Build the tooltip title from the intersection
    const setNames = intersection.setIds.map(id => {
      const set = this.data?.sets.find(s => s.listId === id);
      return set?.listName || 'Unknown';
    if (intersection.setIds.length === 1) {
      this.tooltipTitle = `Only in ${setNames[0]}`;
      this.tooltipTitle = setNames.join(' ∩ ');
    this.tooltipCount = intersection.size;
   * Update tooltip position for label without triggering extra change detection
  private moveLabelTooltipPosition(event: MouseEvent): void {
    // Don't call detectChanges here - we call it once in showLabelTooltip
    // and the Angular zone will handle the position updates
  private hideTooltip(): void {
    // Don't hide tooltip if we're hovering an intersection label
  private onCircleClick(set: VennSet): void {
    // Find the "only in this set" intersection
      i => i.setIds.length === 1 && i.setIds[0] === set.listId
    if (intersection) {
      this.regionClick.emit({
        intersection,
        recordIds: intersection.recordIds
  private onIntersectionClick(intersection: VennIntersection): void {
