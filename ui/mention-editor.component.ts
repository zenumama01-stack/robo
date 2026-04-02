import { MentionAutocompleteService, MentionSuggestion } from '../../services/mention-autocomplete.service';
import { ChatMessageContentBlock } from '@memberjunction/ai';
 * Represents a pending attachment that hasn't been uploaded yet
export interface PendingAttachment {
  /** Local ID for tracking */
  /** Original File object */
  file: File;
  /** Base64 data URL for display */
  dataUrl: string;
  /** MIME type */
  /** Width in pixels (for images) */
  /** Height in pixels (for images) */
  /** Small thumbnail URL for preview */
 * ContentEditable-based mention editor with visual chips/pills
 * Provides Slack/Teams-style mention UX with immutable mention tokens
  selector: 'mj-mention-editor',
  templateUrl: './mention-editor.component.html',
  styleUrls: ['./mention-editor.component.css'],
      useExisting: forwardRef(() => MentionEditorComponent),
export class MentionEditorComponent implements OnInit, AfterViewInit, ControlValueAccessor {
  @ViewChild('editor', { static: false }) editorRef!: ElementRef<HTMLDivElement>;
  @Input() placeholder: string = 'Type @ to mention agents or users...';
  @Input() currentUser?: UserInfo;
  @Input() enableMentions: boolean = true;
  // Attachment settings
  @Input() enableAttachments: boolean = true;
  @Input() maxAttachments: number = 10;
  @Input() maxAttachmentSizeBytes: number = 20 * 1024 * 1024; // 20MB default
  @Input() acceptedFileTypes: string = 'image/*'; // MIME types to accept
  @Output() valueChange = new EventEmitter<string>();
  @Output() mentionSelected = new EventEmitter<MentionSuggestion>();
  @Output() enterPressed = new EventEmitter<string>();
  @Output() attachmentsChanged = new EventEmitter<PendingAttachment[]>();
  @Output() attachmentError = new EventEmitter<string>();
  @Output() attachmentClicked = new EventEmitter<PendingAttachment>();
  // Pending attachments state
  public pendingAttachments: PendingAttachment[] = [];
  public isDragOver: boolean = false;
  // Mention dropdown state
  public showMentionDropdown: boolean = false;
  public mentionSuggestions: MentionSuggestion[] = [];
  public mentionDropdownPosition: { top: number; left: number } = { top: 0, left: 0 };
  public mentionDropdownShowAbove: boolean = false;
  private mentionStartIndex: number = -1;
  private mentionQuery: string = '';
  private onChange: (value: string) => void = () => {};
  public onTouched: () => void = () => {};
    private mentionAutocomplete: MentionAutocompleteService,
    if (this.enableMentions && this.currentUser) {
      await this.mentionAutocomplete.initialize(this.currentUser);
    // Auto-focus the editor
      this.editorRef?.nativeElement?.focus();
   * Handle clicks on the container - focus the editor if clicking outside the contentEditable
  onContainerClick(event: MouseEvent): void {
    const editor = this.editorRef?.nativeElement;
    // Don't handle clicks on the dropdown or its children
    if (target.closest('mj-mention-dropdown')) {
    // If clicking on container or any element that's not the editor itself, focus the editor
    if (target !== editor && !editor?.contains(target)) {
      editor?.focus();
      // Move cursor to end of content
      const selection = window.getSelection();
      const range = document.createRange();
      if (editor && selection) {
        range.selectNodeContents(editor);
        range.collapse(false); // Collapse to end
        selection.removeAllRanges();
        selection.addRange(range);
   * Handle input changes in contentEditable
  onInput(): void {
    const plainText = this.getPlainText();
    this.onChange(plainText);
    this.valueChange.emit(plainText);
    // Handle @mention autocomplete
      this.handleMentionInput();
   * Handle blur event - close dropdown when editor loses focus
  onBlur(): void {
    // Call form control touched callback
    this.onTouched();
    // Close dropdown when editor loses focus
    // Use setTimeout to allow mousedown events on dropdown to fire first
      if (this.showMentionDropdown) {
        this.closeMentionDropdown();
   * Handle paste event - images or plain text
  onPaste(event: ClipboardEvent): void {
    // Check for image data in clipboard
    if (this.enableAttachments && event.clipboardData?.items) {
      const items = Array.from(event.clipboardData.items);
      const imageItems = items.filter(item => item.type.startsWith('image/'));
      if (imageItems.length > 0) {
        // Process each image
        for (const item of imageItems) {
          const file = item.getAsFile();
          if (file) {
            this.processFile(file);
    // No images - handle as plain text
    // Get plain text from clipboard
    const text = event.clipboardData?.getData('text/plain') || '';
    // Insert plain text at cursor position
    if (!selection || selection.rangeCount === 0) return;
    const range = selection.getRangeAt(0);
    range.deleteContents();
    // Insert text as text node (not HTML)
    const textNode = document.createTextNode(text);
    range.insertNode(textNode);
    // Move cursor to end of inserted text
    range.setStartAfter(textNode);
    range.collapse(true);
    // Trigger input event to update model
    this.onInput();
   * Handle keydown events
  onKeyDown(event: KeyboardEvent): void {
    // Enter alone: Send message (if dropdown not showing)
    if (event.key === 'Enter' && !event.shiftKey && !this.showMentionDropdown) {
      this.enterPressed.emit(plainText);
    // Backspace: Check if deleting a mention chip
    if (event.key === 'Backspace') {
      this.handleBackspace(event);
    // Handle mention dropdown navigation
      // Let the dropdown handle arrow keys, enter, escape
      // (We'll pass these through to mention-dropdown component)
   * Handle @mention input detection
  private handleMentionInput(): void {
    const textBeforeCursor = this.getTextBeforeCursor(range);
    // Find the last @ before cursor
    const lastAtIndex = textBeforeCursor.lastIndexOf('@');
    if (lastAtIndex === -1) {
    // Check if there's a space between @ and cursor (means mention was completed)
    const textAfterAt = textBeforeCursor.substring(lastAtIndex + 1);
    if (textAfterAt.includes(' ')) {
    // Extract query
    this.mentionQuery = textAfterAt;
    this.mentionStartIndex = lastAtIndex;
    // Get suggestions
    this.mentionSuggestions = this.mentionAutocomplete.getSuggestions(this.mentionQuery, !!this.currentUser);
    if (this.mentionSuggestions.length > 0) {
      this.showMentionDropdown = true;
      this.positionMentionDropdown();
   * Get text before cursor position
  private getTextBeforeCursor(range: Range): string {
    const tempRange = range.cloneRange();
    tempRange.selectNodeContents(this.editorRef.nativeElement);
    tempRange.setEnd(range.startContainer, range.startOffset);
    return tempRange.toString();
   * Position the mention dropdown
  private positionMentionDropdown(): void {
    if (!editor) return;
    // Get the parent container (message-input-box-container) for alignment
    const container = editor.closest('.message-input-box-container');
    const containerRect = container?.getBoundingClientRect();
    const cursorRect = range.getBoundingClientRect();
    // Check space below vs above
    const spaceBelow = window.innerHeight - cursorRect.bottom;
    const spaceAbove = cursorRect.top;
    const dropdownHeight = Math.min(this.mentionSuggestions.length * 56, 300);
    this.mentionDropdownShowAbove = spaceBelow < dropdownHeight && spaceAbove > spaceBelow;
    if (this.mentionDropdownShowAbove) {
      // Position above, aligning with container top if possible
      this.mentionDropdownPosition = {
        top: containerRect ? containerRect.top + window.scrollY : cursorRect.top + window.scrollY - 4,
        left: cursorRect.left + window.scrollX
      // Position below cursor, but align bottom edge with container top
        top: containerRect ? containerRect.top + window.scrollY : cursorRect.bottom + window.scrollY + 4,
   * Handle mention selection from dropdown
  onMentionSelected(suggestion: MentionSuggestion): void {
    this.insertMentionChip(suggestion);
    this.mentionSelected.emit(suggestion);
    // Refocus the editor after selection
   * Insert a mention chip at the current cursor position
  private insertMentionChip(suggestion: MentionSuggestion): void {
    // Delete the @query text
    const deleteLength = textBeforeCursor.length - lastAtIndex;
    range.setStart(range.startContainer, range.startOffset - deleteLength);
    // Create mention chip element
    const chip = this.createMentionChip(suggestion);
    // Insert chip
    range.insertNode(chip);
    // Add space after chip
    const space = document.createTextNode(' ');
    range.collapse(false);
    range.insertNode(space);
    // Move cursor after the space
    range.setStartAfter(space);
   * Create a mention chip DOM element
  private createMentionChip(suggestion: MentionSuggestion): HTMLSpanElement {
    const chip = document.createElement('span');
    chip.className = 'mention-chip';
    chip.contentEditable = 'false'; // Make chip non-editable
    chip.setAttribute('data-mention-id', suggestion.id);
    chip.setAttribute('data-mention-type', suggestion.type);
    chip.setAttribute('data-mention-name', suggestion.name);
    // For agents, get configuration presets (AIEngine.Config() already called during app init)
    let presets: MJAIAgentConfigurationEntity[] = [];
    if (suggestion.type === 'agent') {
      presets = AIEngineBase.Instance.GetAgentConfigurationPresets(suggestion.id, true);
      // Store default preset
      // IMPORTANT: Store the AIAgentConfiguration.ID (preset ID), not AIConfigurationID
      // The backend mapping will convert preset ID -> AIConfigurationID
      const defaultPreset = presets.find(p => p.IsDefault) || presets[0];
      if (defaultPreset) {
        chip.setAttribute('data-preset-id', defaultPreset.ID || '');
        chip.setAttribute('data-preset-name', defaultPreset.Name || '');
    // Apply inline styles directly
    const isUser = suggestion.type === 'user';
    chip.style.cssText = `
      background: ${isUser ? 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)' : 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'};
      border: 2px solid ${isUser ? 'rgba(240, 147, 251, 0.4)' : 'rgba(102, 126, 234, 0.4)'};
    // Add icon or image
    if (suggestion.type === 'agent' && suggestion.imageUrl) {
      // Use image if available (LogoURL from agent)
      img.src = suggestion.imageUrl;
      img.alt = suggestion.displayName;
      img.style.cssText = 'width: 16px; height: 16px; border-radius: 50%; object-fit: cover;';
      chip.appendChild(img);
    } else if (suggestion.type === 'agent' && suggestion.icon === 'mj-icon-skip') {
      // Special handling for mj-icon-skip: use the SVG data URI as an image
      img.src = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 101.89918457031249 96.83947368421053'%3E%3Cg transform='translate(-0.1288232421875,-0.0)'%3E%3Cpath d='M93.85,41.56c-.84,0-1.62.2-2.37.55-3-4.35-7.49-8.12-13.04-11.04l.04-7.18v-14.44h-10.24v17.6c-1.52-.43-3.07-.8-4.67-1.11V0h-10.24v24.72s-.09,0-.14,0h-4.38s-.1,0-.14,0V7.3h-10.24v18.62c-1.6.32-3.15.69-4.67,1.11v-11.67h-10.24v6.09l.04,9.6c-5.55,2.92-10.04,6.7-13.04,11.04-.75-.35-1.53-.55-2.37-.55-4.5,0-8.14,5.61-8.14,12.51s3.64,12.53,8.14,12.53c.58,0,1.14-.12,1.67-.29,4.1,6.62,11.54,12.06,20.98,15.28l.79.13v7.05c0,2.97,1.45,5.58,3.87,6.99,1.18.69,2.5,1.04,3.85,1.03,1.4,0,2.83-.37,4.15-1.12l7.54-4.29,7.56,4.3c1.31.74,2.73,1.12,4.13,1.12s2.67-.35,3.85-1.04c2.42-1.41,3.86-4.02,3.86-6.98v-7.05l.79-.13c9.44-3.22,16.89-8.66,20.98-15.28.54.17,1.09.29,1.68.29,4.5,0,8.14-5.61,8.14-12.53s-3.63-12.51-8.14-12.51' fill='%23AAAAAA'/%3E%3Cpath d='M86.69,50.87c0-12.22-13.6-19.1-28.94-20.66-4.48-.47-9.19-.54-13.52,0-15.34,1.53-28.93,8.41-28.93,20.66,0,8.55,5.7,15.55,12.68,15.55h7.94c3.05,2.5,6.93,4.1,11.08,4.71,2.65.4,5.44.46,8.01,0,4.15-.6,8.05-2.2,11.1-4.71h7.92c6.97,0,12.68-7,12.68-15.55' fill='white' opacity='0.9'/%3E%3Cpath d='M57.83,55.82c-1.19,2.58-3.8,4.35-6.84,4.35s-5.65-1.77-6.84-4.35h13.68Z' fill='%23AAAAAA'/%3E%3Cpath d='M32.52,41.14c1.74,0,3.18,2.13,3.18,4.76s-1.44,4.74-3.18,4.74-3.16-2.13-3.16-4.74,1.41-4.76,3.16-4.76' fill='%23AAAAAA'/%3E%3Cpath d='M69.46,41.14c1.74,0,3.16,2.13,3.16,4.76s-1.41,4.74-3.16,4.74-3.18-2.13-3.18-4.74,1.41-4.76,3.18-4.76' fill='%23AAAAAA'/%3E%3Cpath d='M63.91,76.15c-.82-.48-1.84-.43-2.8.12l-10.13,5.75-10.11-5.75c-.96-.55-1.98-.59-2.8-.12-.82.47-1.29,1.38-1.29,2.49v10.12c0,1.11.47,2.02,1.28,2.49.38.22.8.33,1.24.33.51,0,1.05-.15,1.57-.44l10.12-5.75,10.11,5.75c.52.29,1.05.44,1.56.44.44,0,.86-.11,1.24-.33.81-.48,1.28-1.38,1.28-2.49v-10.12c0-1.11-.47-2.02-1.28-2.49' fill='white' opacity='0.9'/%3E%3C/g%3E%3C/svg%3E";
      // Use icon for users or agents without images
      const icon = document.createElement('i');
      icon.style.cssText = 'font-size: 12px; opacity: 0.95;';
      if (suggestion.type === 'agent' && suggestion.icon) {
        icon.className = this.getIconClasses(suggestion.icon);
      } else if (suggestion.type === 'user') {
        icon.className = 'fa-solid fa-user';
        icon.className = 'fa-solid fa-robot';
      chip.appendChild(icon);
    // Add space between icon/image and text
    chip.appendChild(space);
    // Add text
    const text = document.createTextNode(suggestion.displayName);
    chip.appendChild(text);
    // Add dropdown if 2+ presets for agents
    if (suggestion.type === 'agent' && presets.length >= 2) {
      this.addConfigurationDropdown(chip, presets);
    return chip;
   * Add configuration preset dropdown to agent chip
  private addConfigurationDropdown(chip: HTMLSpanElement, presets: MJAIAgentConfigurationEntity[]): void {
    // Store default preset for comparison
    // Add preset indicator text (only shown when non-default selected)
    const presetIndicator = document.createElement('span');
    presetIndicator.className = 'preset-indicator';
    presetIndicator.style.cssText = `
      font-family: 'SF Mono', 'Monaco', 'Inconsolata', 'Roboto Mono', 'Courier New', monospace;
      border: 1px solid rgba(255, 255, 255, 0.15);
    chip.appendChild(presetIndicator);
    // Add dropdown chevron button
    const chevron = document.createElement('i');
    chevron.className = 'fa-solid fa-chevron-down';
    chevron.style.cssText = `
    chip.appendChild(chevron);
    // Create dropdown menu (initially hidden) - append to body for global positioning
    const dropdown = document.createElement('div');
    dropdown.className = 'preset-dropdown';
    dropdown.style.cssText = `
    // Add preset options to dropdown
    presets.forEach(preset => {
      const option = document.createElement('div');
      option.className = 'preset-option';
      option.style.cssText = `
      // Check if this is the selected preset
      const currentPresetId = chip.getAttribute('data-preset-id');
      const isSelected = preset.ID === currentPresetId;
      // Add checkmark for selected option
      const checkmark = document.createElement('i');
      checkmark.className = 'fa-solid fa-check';
      checkmark.style.cssText = `
        opacity: ${isSelected ? '1' : '0'};
      option.appendChild(checkmark);
      // Add preset text content
      const textContainer = document.createElement('div');
      textContainer.style.cssText = 'flex: 1;';
      const displayName = document.createElement('div');
      displayName.textContent = preset.DisplayName || preset.Name;
      displayName.style.cssText = `
      textContainer.appendChild(displayName);
      if (preset.Description) {
        const description = document.createElement('div');
        description.textContent = preset.Description;
        description.style.cssText = `
        textContainer.appendChild(description);
      option.appendChild(textContainer);
      // Hover effects
      option.addEventListener('mouseenter', () => {
        option.style.background = '#f5f5f5';
      option.addEventListener('mouseleave', () => {
        option.style.background = 'white';
      // Click handler to select preset
      option.addEventListener('click', (e) => {
        // Update chip data attributes
        // IMPORTANT: Store preset.ID (AIAgentConfiguration.ID), not AIConfigurationID
        chip.setAttribute('data-preset-id', preset.ID || '');
        chip.setAttribute('data-preset-name', preset.Name || '');
        // Update preset indicator visibility and text
        const isDefault = preset.ID === defaultPreset.ID;
        if (isDefault) {
          presetIndicator.style.display = 'none';
          presetIndicator.style.display = 'inline';
          presetIndicator.textContent = preset.DisplayName || preset.Name;
        // Update checkmarks
        dropdown.querySelectorAll('.preset-option').forEach((opt, idx) => {
          const check = opt.querySelector('i.fa-check') as HTMLElement;
          if (check) {
            check.style.opacity = idx === presets.indexOf(preset) ? '1' : '0';
        // Close dropdown
        dropdown.style.display = 'none';
      dropdown.appendChild(option);
    // Append dropdown to document body for global positioning
    document.body.appendChild(dropdown);
    // Helper function to position dropdown relative to chip
    const positionDropdown = () => {
      const chipRect = chip.getBoundingClientRect();
      const spaceBelow = viewportHeight - chipRect.bottom;
      // Set left position (same for both above/below)
      dropdown.style.left = `${chipRect.left}px`;
      // First, show dropdown to measure its actual height
      const wasHidden = dropdown.style.display === 'none';
      if (wasHidden) {
        dropdown.style.visibility = 'hidden';
        dropdown.style.display = 'block';
      const dropdownHeight = dropdown.offsetHeight;
      const spaceAbove = chipRect.top;
      // Position dropdown above chip if not enough space below
      if (spaceBelow < dropdownHeight && spaceAbove > spaceBelow) {
        // Show above - position so bottom of dropdown is 4px above top of chip
        dropdown.style.top = `${chipRect.top - dropdownHeight - 4}px`;
        // Show below (default) - position 4px below bottom of chip
        dropdown.style.top = `${chipRect.bottom + 4}px`;
      // Restore visibility
        dropdown.style.visibility = 'visible';
    // Toggle dropdown on chevron click
    chevron.addEventListener('click', (e) => {
      const isVisible = dropdown.style.display === 'block';
      if (isVisible) {
        positionDropdown();
    const closeDropdown = (e: MouseEvent) => {
      if (!dropdown.contains(e.target as Node) && !chip.contains(e.target as Node)) {
    document.addEventListener('click', closeDropdown);
    // Cleanup: remove dropdown from body and event listeners when chip is removed
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        mutation.removedNodes.forEach((node) => {
          if (node === chip) {
            document.removeEventListener('click', closeDropdown);
            if (dropdown.parentNode) {
              dropdown.parentNode.removeChild(dropdown);
            observer.disconnect();
    if (chip.parentNode) {
      observer.observe(chip.parentNode, { childList: true });
   * Get icon classes with proper FA prefix
  private getIconClasses(iconClass: string): string {
    if (!iconClass) return 'fa-solid fa-robot';
   * Handle backspace key - delete entire chip if cursor is right after one
  private handleBackspace(event: KeyboardEvent): void {
    // Check if cursor is right after a mention chip
    if (range.collapsed && range.startOffset > 0) {
      const prevNode = range.startContainer.childNodes[range.startOffset - 1];
      if (prevNode && (prevNode as HTMLElement).classList?.contains('mention-chip')) {
        prevNode.remove();
   * Close mention dropdown
  closeMentionDropdown(): void {
    this.showMentionDropdown = false;
    this.mentionSuggestions = [];
    this.mentionStartIndex = -1;
    this.mentionQuery = '';
   * Convert editor HTML to plain text with @mentions
  private getPlainText(): string {
    if (!editor) return '';
    let text = '';
    const nodes = editor.childNodes;
    for (let i = 0; i < nodes.length; i++) {
      const node = nodes[i];
      if (node.nodeType === Node.TEXT_NODE) {
        text += node.textContent || '';
      } else if (node.nodeType === Node.ELEMENT_NODE) {
        const element = node as HTMLElement;
        if (element.classList.contains('mention-chip')) {
          const name = element.getAttribute('data-mention-name') || '';
          // Use quoted format if name has spaces
          text += name.includes(' ') ? `@"${name}"` : `@${name}`;
        } else if (element.tagName === 'BR') {
          text += '\n';
        } else if (element.tagName === 'DIV') {
          // Handle line breaks from contentEditable
          if (i > 0) text += '\n';
          text += this.getNodeText(element);
          text += element.textContent || '';
   * Get text from a node recursively
  private getNodeText(node: Node): string {
    const children = node.childNodes;
    for (let i = 0; i < children.length; i++) {
      const child = children[i];
      if (child.nodeType === Node.TEXT_NODE) {
        text += child.textContent || '';
      } else if (child.nodeType === Node.ELEMENT_NODE) {
        const element = child as HTMLElement;
   * Set editor content from plain text (for programmatic updates)
  private setEditorContent(text: string): void {
    // For now, just set as plain text
    // TODO: Parse @mentions and render as chips
    editor.textContent = text;
  // ControlValueAccessor implementation
      this.setEditorContent(value);
    } else if (this.editorRef?.nativeElement) {
      this.editorRef.nativeElement.textContent = '';
  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  setDisabledState(isDisabled: boolean): void {
    if (this.editorRef?.nativeElement) {
      this.editorRef.nativeElement.contentEditable = (!isDisabled).toString();
   * Focus the editor
  public focus(): void {
   * Clear the editor content and pending attachments
    this.clearPendingAttachments();
   * Extract mention chips with their configuration data
   * Returns array of objects containing mention info and preset configuration
  public getMentionChipsData(): Array<{ id: string; type: string; name: string; presetId?: string; presetName?: string }> {
    if (!editor) return [];
    const chips: Array<{ id: string; type: string; name: string; presetId?: string; presetName?: string }> = [];
    const mentionElements = editor.querySelectorAll('.mention-chip');
    mentionElements.forEach(chip => {
      const element = chip as HTMLElement;
      const id = element.getAttribute('data-mention-id') || '';
      const type = element.getAttribute('data-mention-type') || '';
      const presetId = element.getAttribute('data-preset-id') || undefined;
      const presetName = element.getAttribute('data-preset-name') || undefined;
      chips.push({
        ...(presetId ? { presetId } : {}),
        ...(presetName ? { presetName } : {})
    return chips;
   * Get the plain text value with mentions encoded as JSON
   * This format preserves configuration information when messages are saved
   * Format: @{type:"agent",id:"uuid",name:"Agent Name",configId:"uuid",config:"High"}
  public getPlainTextWithJsonMentions(): string {
    let plainText = '';
    const childNodes = Array.from(editor.childNodes);
    for (const node of childNodes) {
        // Regular text node
        plainText += node.textContent || '';
          // Extract mention data attributes
          const presetId = element.getAttribute('data-preset-id');
          const presetName = element.getAttribute('data-preset-name');
          // Build JSON mention object (compact format - no spaces)
          const mentionObj: Record<string, string> = {
            name
          // Add configuration fields if present (for agents with non-default presets)
          if (presetId) {
            mentionObj.configId = presetId;
          if (presetName) {
            mentionObj.config = presetName;
          // Encode as JSON mention
          // Don't remove spaces from the JSON string - spaces in values (like agent names) need to be preserved
          plainText += `@${JSON.stringify(mentionObj)}`;
          // Other HTML element (e.g., <br>)
          if (element.tagName === 'BR') {
            plainText += '\n';
            plainText += element.textContent || '';
    return plainText;
  // ==================== Attachment Handling Methods ====================
   * Handle drag over event
    if (!this.enableAttachments) return;
    this.isDragOver = true;
   * Handle drag leave event
  onDragLeave(event: DragEvent): void {
    this.isDragOver = false;
   * Handle drop event
    if (event.dataTransfer?.files) {
      const files = Array.from(event.dataTransfer.files);
        if (this.isAcceptedFile(file)) {
   * Handle file input change (from file picker)
  onFileSelected(event: Event): void {
    if (input.files) {
      const files = Array.from(input.files);
      // Reset input so same file can be selected again
   * Process a file for attachment
  private async processFile(file: File): Promise<void> {
    const validation = this.validateFile(file);
    if (!validation.valid) {
      this.attachmentError.emit(validation.error!);
    // Read file as data URL
    const dataUrl = await this.readFileAsDataUrl(file);
    if (!dataUrl) {
      this.attachmentError.emit('Failed to read file');
    // Get image dimensions if applicable
    let width: number | undefined;
    let height: number | undefined;
    if (file.type.startsWith('image/')) {
      const dimensions = await this.getImageDimensions(dataUrl);
      width = dimensions.width;
      height = dimensions.height;
    // Create pending attachment
    const attachment: PendingAttachment = {
      id: this.generateAttachmentId(),
      dataUrl,
      mimeType: file.type,
      fileName: file.name,
      sizeBytes: file.size,
      thumbnailUrl: file.type.startsWith('image/') ? dataUrl : undefined
    // Add to pending attachments
    this.pendingAttachments.push(attachment);
    this.attachmentsChanged.emit([...this.pendingAttachments]);
   * Validate a file against settings
  private validateFile(file: File): { valid: boolean; error?: string } {
    // Check count limit
    if (this.pendingAttachments.length >= this.maxAttachments) {
        error: `Maximum ${this.maxAttachments} attachments allowed`
    // Check size limit
    if (file.size > this.maxAttachmentSizeBytes) {
      const maxMB = (this.maxAttachmentSizeBytes / (1024 * 1024)).toFixed(1);
      const fileMB = (file.size / (1024 * 1024)).toFixed(1);
        error: `File size (${fileMB}MB) exceeds maximum (${maxMB}MB)`
    // Check file type
    if (!this.isAcceptedFile(file)) {
        error: `File type ${file.type || 'unknown'} not accepted`
   * Check if file type is accepted
  private isAcceptedFile(file: File): boolean {
    if (this.acceptedFileTypes === '*' || this.acceptedFileTypes === '*/*') {
    const acceptedTypes = this.acceptedFileTypes.split(',').map(t => t.trim());
    for (const accepted of acceptedTypes) {
      // Handle wildcard MIME types like "image/*"
      if (accepted.endsWith('/*')) {
        const category = accepted.slice(0, -2);
        if (file.type.startsWith(category + '/')) {
      } else if (accepted.startsWith('.')) {
        // Handle file extensions like ".png"
        if (file.name.toLowerCase().endsWith(accepted.toLowerCase())) {
      } else if (file.type === accepted) {
        // Exact MIME type match
   * Read file as data URL
  private readFileAsDataUrl(file: File): Promise<string | null> {
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = () => resolve(null);
      reader.readAsDataURL(file);
   * Get image dimensions
  private getImageDimensions(dataUrl: string): Promise<{ width: number; height: number }> {
      const img = new Image();
      img.onload = () => {
        resolve({ width: img.naturalWidth, height: img.naturalHeight });
      img.onerror = () => {
        resolve({ width: 0, height: 0 });
      img.src = dataUrl;
   * Generate unique attachment ID
  private generateAttachmentId(): string {
    return `att_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
   * Remove a pending attachment by ID
  public removeAttachment(id: string): void {
    const index = this.pendingAttachments.findIndex(a => a.id === id);
      this.pendingAttachments.splice(index, 1);
   * Get all pending attachments
  public getPendingAttachments(): PendingAttachment[] {
    return [...this.pendingAttachments];
   * Clear all pending attachments
  public clearPendingAttachments(): void {
    this.pendingAttachments = [];
    this.attachmentsChanged.emit([]);
   * Handle click on an attachment thumbnail
  public onAttachmentClick(attachment: PendingAttachment): void {
    this.attachmentClicked.emit(attachment);
   * Check if there are any pending attachments
  public hasAttachments(): boolean {
    return this.pendingAttachments.length > 0;
   * Trigger file picker programmatically
  public openFilePicker(): void {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = this.acceptedFileTypes;
    input.multiple = true;
    input.onchange = (e) => this.onFileSelected(e);
    input.click();
