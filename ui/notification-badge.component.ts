import { BadgeConfig } from '../../models/notification.model';
 * Displays notification badges with various styles and animations
 * Supports count, dot, pulse, and new badge types
  selector: 'mj-notification-badge',
    @if (badgeConfig?.show) {
      <div class="notification-badge-container">
        @if (badgeConfig?.type === 'count' && badgeConfig?.count != null && badgeConfig?.count! > 0) {
            class="notification-badge badge-count"
            [class.badge-high]="badgeConfig?.priority === 'high'"
            [class.badge-urgent]="badgeConfig?.priority === 'urgent'"
            [class.badge-animate]="badgeConfig?.animate">
            {{ formatCount(badgeConfig?.count!) }}
        @if (badgeConfig?.type === 'dot') {
            class="notification-badge badge-dot"
        @if (badgeConfig?.type === 'pulse') {
            class="notification-badge badge-pulse"
            [class.badge-urgent]="badgeConfig?.priority === 'urgent'">
            <span class="pulse-ring"></span>
            <span class="pulse-ring-delay"></span>
            @if (badgeConfig?.count != null && badgeConfig?.count! > 0) {
              <span class="pulse-count">{{ formatCount(badgeConfig?.count!) }}</span>
        @if (badgeConfig?.type === 'new') {
            class="notification-badge badge-new"
    .notification-badge-container {
    /* Count badge */
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.15);
    .badge-count.badge-high {
    .badge-count.badge-urgent {
    /* Dot badge */
    .badge-dot {
    .badge-dot.badge-high {
    .badge-dot.badge-urgent {
    /* Pulse badge with animated rings */
    .badge-pulse {
    .badge-pulse.badge-high {
    .badge-pulse.badge-urgent {
      animation: pulse-ring 2s cubic-bezier(0.215, 0.61, 0.355, 1) infinite;
    .pulse-ring-delay {
      animation-delay: 1s;
    @keyframes pulse-ring {
        transform: translate(-50%, -50%) scale(0.8);
        transform: translate(-50%, -50%) scale(1.3);
        transform: translate(-50%, -50%) scale(1.5);
    .pulse-count {
    /* New badge */
    .badge-new {
    /* Pop-in animation for badges */
    .badge-animate {
      animation: badge-pop 300ms cubic-bezier(0.68, -0.55, 0.265, 1.55);
    @keyframes badge-pop {
        transform: scale(0);
    /* Shake animation for urgent badges */
    .badge-urgent.badge-animate {
      animation: badge-pop 300ms cubic-bezier(0.68, -0.55, 0.265, 1.55),
                 badge-shake 400ms ease-in-out 300ms;
    @keyframes badge-shake {
      0%, 100% { transform: translateX(0); }
      10%, 30%, 50%, 70%, 90% { transform: translateX(-2px); }
      20%, 40%, 60%, 80% { transform: translateX(2px); }
    /* Hover effects */
    .notification-badge:hover {
    .badge-pulse:hover .pulse-ring,
    .badge-pulse:hover .pulse-ring-delay {
      animation-play-state: paused;
export class NotificationBadgeComponent implements OnInit, OnDestroy {
  @Input() badgeConfig?: BadgeConfig;
  private _loadedBadgeConfig: BadgeConfig | null = null;
  constructor(private notificationService: NotificationService) {}
    // If badgeConfig not provided but conversationId is, load from service
    if (!this.badgeConfig && this.conversationId) {
      this.notificationService
        .getBadgeConfig$(this.conversationId)
        .subscribe(config => {
          this._loadedBadgeConfig = config;
  get displayConfig(): BadgeConfig | null {
    return this.badgeConfig || this._loadedBadgeConfig;
   * Formats count for display
   * Shows 99+ for counts over 99
  formatCount(count: number): string {
    return count > 99 ? '99+' : count.toString();
