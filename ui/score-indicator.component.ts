  selector: 'app-score-indicator',
    <div class="score-indicator" [class]="getColorClass()">
      @if (showBar) {
        <div class="score-bar-container">
          <div class="score-bar" [style.width.%]="score * 100"></div>
      <div class="score-value">
          <i [class]="getIcon()"></i>
        <span class="score-text">{{ formatScore(score) }}</span>
    .score-indicator {
    .score-bar-container {
      transition: width 0.3s ease, background-color 0.3s ease;
    .score-indicator--excellent .score-bar {
      background: linear-gradient(90deg, #4caf50, #66bb6a);
    .score-indicator--good .score-bar {
      background: linear-gradient(90deg, #8bc34a, #9ccc65);
    .score-indicator--fair .score-bar {
      background: linear-gradient(90deg, #ffc107, #ffca28);
    .score-indicator--poor .score-bar {
      background: linear-gradient(90deg, #ff9800, #ffa726);
    .score-indicator--fail .score-bar {
      background: linear-gradient(90deg, #f44336, #e57373);
    .score-value {
    .score-indicator--excellent .score-value {
    .score-indicator--good .score-value {
    .score-indicator--fair .score-value {
    .score-indicator--poor .score-value {
    .score-indicator--fail .score-value {
    .score-value i {
export class ScoreIndicatorComponent {
  @Input() score!: number; // 0-1.0000
  @Input() showBar = true;
  @Input() decimals = 4;
  formatScore(score: number): string {
    if (score == null) return 'N/A';
    return score.toFixed(this.decimals);
  getColorClass(): string {
    if (this.score >= 0.9) return 'score-indicator--excellent';
    if (this.score >= 0.8) return 'score-indicator--good';
    if (this.score >= 0.6) return 'score-indicator--fair';
    if (this.score >= 0.4) return 'score-indicator--poor';
    return 'score-indicator--fail';
  getIcon(): string {
    if (this.score >= 0.9) return 'fa-solid fa-star';
    if (this.score >= 0.8) return 'fa-solid fa-check';
    if (this.score >= 0.6) return 'fa-solid fa-minus';
    if (this.score >= 0.4) return 'fa-solid fa-exclamation';
