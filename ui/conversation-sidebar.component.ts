  selector: 'mj-conversation-sidebar',
    <div class="conversation-sidebar">
      @if (activeTab === 'conversations') {
            (conversationSelected)="conversationSelected.emit($event)"
            (newConversationRequested)="newConversationRequested.emit()"
            (pinSidebarRequested)="onPinSidebarRequested()"
            (unpinSidebarRequested)="onUnpinSidebarRequested()">
      @if (activeTab === 'collections') {
          <mj-collection-tree
          </mj-collection-tree>
    .placeholder {
    .placeholder p {
export class ConversationSidebarComponent {
  @Input() isSidebarPinned: boolean = true;
  @Input() isMobileView: boolean = false;
  @Output() pinSidebarRequested = new EventEmitter<void>();
  @Output() unpinSidebarRequested = new EventEmitter<void>();
  onPinSidebarRequested(): void {
  onUnpinSidebarRequested(): void {
