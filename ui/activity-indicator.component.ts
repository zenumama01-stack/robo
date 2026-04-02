import { ActivityIndicatorConfig } from '../../models/notification.model';
 * Displays activity indicators for agent processes, typing, etc.
  selector: 'mj-activity-indicator',
    @if (config?.show) {
      <div class="activity-indicator" [class.activity-agent]="config?.type === 'agent'" [class.activity-processing]="config?.type === 'processing'" [class.activity-typing]="config?.type === 'typing'">
        <div class="activity-dots">
        @if (config?.text) {
          <span class="activity-text">{{ config?.text }}</span>
    .activity-indicator {
    .activity-agent {
    .activity-processing {
      color: #D97706;
    .activity-typing {
    .activity-dots {
    .dot {
      animation: dot-pulse 1.4s ease-in-out infinite;
    .dot:nth-child(1) {
    .dot:nth-child(2) {
    .dot:nth-child(3) {
    @keyframes dot-pulse {
    .activity-text {
export class ActivityIndicatorComponent {
  @Input() config?: ActivityIndicatorConfig;
