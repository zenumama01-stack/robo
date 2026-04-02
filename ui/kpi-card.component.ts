export interface KPICardData {
  value: string | number;
  color: 'primary' | 'success' | 'warning' | 'danger' | 'info';
  trend?: {
    direction: 'up' | 'down' | 'stable';
  loading?: boolean;
  selector: 'app-kpi-card',
    <div class="kpi-card" [class]="'kpi-card--' + data.color">
      <div class="kpi-card__header">
        <div class="kpi-card__icon">
          <i [class]="'fa-solid ' + data.icon"></i>
        <div class="kpi-card__title">{{ data.title }}</div>
      <div class="kpi-card__content">
        @if (!data.loading) {
          <div class="kpi-card__value">
            {{ formatValue(data.value) }}
        @if (data.loading) {
          <div class="kpi-card__loading">
        @if (data.subtitle && !data.loading) {
          <div class="kpi-card__subtitle">
            {{ data.subtitle }}
        @if (data.trend && !data.loading) {
          <div class="kpi-card__trend">
            <i [class]="getTrendIcon()" [style.color]="getTrendColor()"></i>
            <span class="trend-percentage" [style.color]="getTrendColor()">
              {{ data.trend.percentage }}%
            <span class="trend-period">{{ data.trend.period }}</span>
      border-left: 4px solid transparent;
      min-height: 130px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
      transform: translateY(-3px);
    .kpi-card--primary { border-left-color: #6366f1; }
    .kpi-card--success { border-left-color: #10b981; }
    .kpi-card--warning { border-left-color: #f59e0b; }
    .kpi-card--danger { border-left-color: #ef4444; }
    .kpi-card--info { border-left-color: #8b5cf6; }
    .kpi-card__header {
    .kpi-card__icon {
    .kpi-card--primary .kpi-card__icon {
    .kpi-card--success .kpi-card__icon {
    .kpi-card--warning .kpi-card__icon {
    .kpi-card--danger .kpi-card__icon {
    .kpi-card--info .kpi-card__icon {
    .kpi-card__title {
    .kpi-card__content {
    .kpi-card__value {
      font-size: 26px;
      line-height: 1.1;
    .kpi-card__subtitle {
    .kpi-card__trend {
    .trend-percentage {
    .trend-period {
    .kpi-card__loading {
        min-height: 110px;
export class KPICardComponent implements OnInit {
  @Input() data!: KPICardData;
    if (!this.data) {
      throw new Error('KPICardComponent requires data input');
  formatValue(value: string | number): string {
      // Format large numbers with appropriate suffixes
        return (value / 1000000).toFixed(1) + 'M';
        return (value / 1000).toFixed(1) + 'K';
      } else if (value % 1 !== 0) {
  getTrendIcon(): string {
    if (!this.data.trend) return '';
    switch (this.data.trend.direction) {
      case 'up':
        return 'fa-solid fa-arrow-up';
      case 'down':
        return 'fa-solid fa-arrow-down';
      case 'stable':
        return 'fa-solid fa-minus';
  getTrendColor(): string {
    if (!this.data.trend) return '#999';
        return '#4caf50';
        return '#f44336';
        return '#999';
