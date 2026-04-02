import { MentionEditorComponent, PendingAttachment } from '../mention/mention-editor.component';
 * Reusable message input box component (presentational)
 * Now uses MentionEditorComponent for rich @mention functionality with chips
 * - Text input with keyboard shortcuts via MentionEditorComponent
 * - @mention autocomplete with visual chips (contentEditable)
 * - Send button
 * Does NOT handle:
 * - Saving messages to database
 * - Agent invocation
 * - Artifact creation
 * - Conversation management
  selector: 'mj-message-input-box',
  templateUrl: './message-input-box.component.html',
  styleUrls: ['./message-input-box.component.css']
export class MessageInputBoxComponent {
  @ViewChild('mentionEditor') mentionEditor?: MentionEditorComponent;
  @Input() placeholder: string = 'Type your message to start a new conversation...';
  @Input() showCharacterCount: boolean = false;
  @Input() rows: number = 3;
  @Input() maxAttachmentSizeBytes: number = 20 * 1024 * 1024; // 20MB
  @Input() acceptedFileTypes: string = 'image/*';
  @Output() textSubmitted = new EventEmitter<string>();
  get canSend(): boolean {
    const hasText = this.value.trim().length > 0;
    const hasAttachments = this.mentionEditor?.hasAttachments() || false;
    return !this.disabled && (hasText || hasAttachments);
   * Handle value changes from MentionEditorComponent
  onValueChange(newValue: string): void {
    this.valueChange.emit(this.value);
   * Handle attachment changes from MentionEditorComponent
  onAttachmentsChanged(attachments: PendingAttachment[]): void {
    this.attachmentsChanged.emit(attachments);
   * Handle attachment errors from MentionEditorComponent
  onAttachmentError(error: string): void {
    this.attachmentError.emit(error);
   * Handle attachment click from MentionEditorComponent
  onAttachmentClicked(attachment: PendingAttachment): void {
   * Handle Enter key from MentionEditorComponent
   * Extracts plain text with JSON-encoded mentions for message submission
  onEnterPressed(_text: string): void {
    this.onSendClick();
   * Handle mention selection from MentionEditorComponent
    // MentionEditorComponent already inserts the mention chip
    // This is just for additional tracking/analytics if needed
   * Send the message
   * Extracts plain text with JSON-encoded mentions for proper persistence
  onSendClick(): void {
    if (this.canSend) {
      // Get plain text with JSON-encoded mentions (preserves configuration info)
      const textToSend = this.mentionEditor?.getPlainTextWithJsonMentions() || this.value.trim();
      this.textSubmitted.emit(textToSend);
      this.value = ''; // Clear input after sending
      // Clear the editor content
      if (this.mentionEditor) {
        this.mentionEditor.clear();
   * Handle clicks on the container - focus the mention editor
   * Only moves cursor to end if clicking outside the contentEditable area
    // Don't handle clicks on the send button
    if (target.closest('.send-button-icon')) {
    const editor = this.mentionEditor?.editorRef?.nativeElement;
    // If clicking directly on the editor or its children, let the browser handle cursor placement
    if (target === editor || editor.contains(target)) {
    // Only if clicking on the container (empty space), focus and move cursor to end
    if (selection) {
   * Public method to focus the input programmatically
  focus(): void {
    if (editor) {
   * Get mention chip data including configuration presets
  getMentionChipsData(): Array<{ id: string; type: string; name: string; presetId?: string; presetName?: string }> {
    return this.mentionEditor?.getMentionChipsData() || [];
   * Get pending attachments from the editor
  getPendingAttachments(): PendingAttachment[] {
    return this.mentionEditor?.getPendingAttachments() || [];
   * Open file picker programmatically
  openFilePicker(): void {
    this.mentionEditor?.openFilePicker();
