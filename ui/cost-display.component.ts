  selector: 'app-cost-display',
    <div class="cost-display" [class]="getMagnitudeClass()">
      @if (showIcon) {
        <i class="fa-solid fa-dollar-sign cost-icon"></i>
      <span class="cost-value">{{ formatCost(cost) }}</span>
      @if (label) {
        <span class="cost-label">{{ label }}</span>
    .cost-display {
    .cost-icon {
    .cost-label {
    .cost-display--low {
    .cost-display--medium {
    .cost-display--high {
    .cost-display--normal {
export class CostDisplayComponent {
  @Input() cost!: number;
  @Input() showIcon = true;
  @Input() label?: string;
  @Input() decimals = 6;
  @Input() threshold = { low: 0.01, high: 1.0 }; // Default thresholds in USD
    if (cost == null) return '$0.00';
    // Format based on magnitude
    if (cost >= 1000) {
      return `$${(cost / 1000).toFixed(2)}K`;
    } else if (cost >= 1) {
      return `$${cost.toFixed(this.decimals)}`;
  getMagnitudeClass(): string {
    if (this.cost < this.threshold.low) return 'cost-display--low';
    if (this.cost >= this.threshold.high) return 'cost-display--high';
    if (this.cost >= this.threshold.low && this.cost < this.threshold.high) return 'cost-display--medium';
    return 'cost-display--normal';
