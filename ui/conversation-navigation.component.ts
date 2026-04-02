import { NavigationTab } from '../../models/conversation-state.model';
  selector: 'mj-conversation-navigation',
    <div class="conversation-navigation">
      <div class="nav-left">
        <button class="nav-btn sidebar-toggle" (click)="sidebarToggled.emit()">
          <i class="fas fa-bars"></i>
          class="nav-tab"
          [class.active]="activeTab === 'conversations'"
          (click)="tabChanged.emit('conversations')">
          [class.active]="activeTab === 'collections'"
          (click)="tabChanged.emit('collections')">
          <span>Collections</span>
          [class.active]="activeTab === 'tasks'"
          (click)="tabChanged.emit('tasks')">
      <div class="nav-right">
        <mj-tasks-dropdown
          [conversationId]="conversationId"
          (navigateToConversation)="navigateToConversation.emit($event)">
        </mj-tasks-dropdown>
        <button class="nav-btn search-btn" (click)="searchTriggered.emit()" title="Search (Ctrl+K)">
        <button class="nav-btn refresh-btn" (click)="refreshTriggered.emit()" title="Refresh agent cache">
    .conversation-navigation {
    .nav-left { display: flex; align-items: center; gap: 12px; }
    .nav-title { margin: 0; font-size: 18px; font-weight: 600; }
      background: rgba(255,255,255,0.05);
      border-bottom-color: #AAE7FD;
    .nav-right { display: flex; gap: 8px; }
    .nav-btn {
    .nav-btn:hover {
      .nav-tab span {
      .nav-tab i {
      .nav-right {
      .nav-btn i {
      .nav-right .nav-btn:last-child {
export class ConversationNavigationComponent {
  @Input() activeTab: NavigationTab = 'conversations';
  @Input() conversationId: string | null = null;
  @Output() tabChanged = new EventEmitter<NavigationTab>();
  @Output() sidebarToggled = new EventEmitter<void>();
  @Output() searchTriggered = new EventEmitter<void>();
  @Output() refreshTriggered = new EventEmitter<void>();
  @Output() navigateToConversation = new EventEmitter<{conversationId: string; taskId: string}>();
