import { MessageAttachment } from '../components/message/message-item.component';
import { PendingAttachment } from '../components/mention/mention-editor.component';
 * Handles loading, saving, and converting attachments between different formats.
   * Load all attachments for a list of conversation detail IDs.
   * Returns a map of ConversationDetailID -> MessageAttachment[]
  async loadAttachmentsForMessages(
  ): Promise<Map<string, MessageAttachment[]>> {
    const result = new Map<string, MessageAttachment[]>();
    if (!conversationDetailIds || conversationDetailIds.length === 0) {
      if (attachmentResult.Success && attachmentResult.Results) {
        for (const attachment of attachmentResult.Results) {
          const detailId = attachment.ConversationDetailID;
          if (!result.has(detailId)) {
            result.set(detailId, []);
          const messageAttachment = this.convertToMessageAttachment(attachment);
          result.get(detailId)!.push(messageAttachment);
      console.error('Error loading attachments:', error);
   * Load attachments for a single message
  async loadAttachmentsForMessage(
  ): Promise<MessageAttachment[]> {
    const map = await this.loadAttachmentsForMessages([conversationDetailId], contextUser);
    return map.get(conversationDetailId) || [];
   * Save pending attachments to the database.
   * Returns the saved attachment entities.
   * @param conversationDetailId - ID of the conversation detail to attach to
   * @param pendingAttachments - Array of pending attachments from the mention editor
  async saveAttachments(
    pendingAttachments: PendingAttachment[],
    const savedAttachments: MJConversationDetailAttachmentEntity[] = [];
    for (let i = 0; i < pendingAttachments.length; i++) {
      const pending = pendingAttachments[i];
        // Determine attachment type from MIME type
        const attachmentType = ConversationUtility.GetAttachmentTypeFromMime(pending.mimeType);
        attachment.ModalityID = this.getModalityIdForType(attachmentType);
        attachment.MimeType = pending.mimeType;
        attachment.FileName = pending.fileName;
        attachment.FileSizeBytes = pending.sizeBytes;
        attachment.Width = pending.width ?? null;
        attachment.Height = pending.height ?? null;
        attachment.DurationSeconds = null; // PendingAttachment doesn't have duration
        attachment.ThumbnailBase64 = pending.thumbnailUrl ?? null;
        // Store inline data - extract base64 from data URL
        if (pending.dataUrl) {
          const base64Data = this.extractBase64FromDataUrl(pending.dataUrl);
          attachment.InlineData = base64Data;
          savedAttachments.push(attachment);
          console.error('Failed to save attachment:', attachment.LatestResult);
        console.error('Error saving attachment:', error);
    return savedAttachments;
   * Create attachment reference tokens for message text.
   * These tokens are stored in the Message field to reference attachments.
  createAttachmentReferences(attachments: MJConversationDetailAttachmentEntity[]): string {
    return attachments
      .map(att => {
          type: this.getAttachmentTypeFromModality(att.ModalityID),
          sizeBytes: att.FileSizeBytes,
          thumbnailBase64: att.ThumbnailBase64 ?? undefined
        return ConversationUtility.CreateAttachmentReference(content);
   * Convert a database entity to a MessageAttachment for display
  private convertToMessageAttachment(entity: MJConversationDetailAttachmentEntity): MessageAttachment {
    // Determine content URL
    let contentUrl: string | undefined;
    let thumbnailUrl: string | undefined;
    if (entity.InlineData) {
      // Create data URL from inline data
      contentUrl = `data:${entity.MimeType};base64,${entity.InlineData}`;
    } else if (entity.FileID) {
      // TODO: Integrate with MJStorage to get pre-authenticated URL
      contentUrl = undefined; // Will need to be loaded separately
    // Use thumbnail if available
    if (entity.ThumbnailBase64) {
      thumbnailUrl = entity.ThumbnailBase64.startsWith('data:')
        ? entity.ThumbnailBase64
        : `data:image/jpeg;base64,${entity.ThumbnailBase64}`;
      type: this.getAttachmentTypeFromModality(entity.ModalityID),
      mimeType: entity.MimeType,
      fileName: entity.FileName,
      sizeBytes: entity.FileSizeBytes,
      width: entity.Width ?? undefined,
      height: entity.Height ?? undefined,
      thumbnailUrl: thumbnailUrl,
      contentUrl: contentUrl
   * Get the AttachmentType from a modality ID
  private getAttachmentTypeFromModality(modalityId: string): AttachmentType {
    const modality = AIEngineBase.Instance.Modalities.find(m => m.ID === modalityId)
    const name = modality?.Name?.toLowerCase() || '';
    if (name === 'image') return 'Image';
    if (name === 'video') return 'Video';
    if (name === 'audio') return 'Audio';
   * Get the modality ID for an attachment type
  private getModalityIdForType(type: AttachmentType): string {
    // Map AttachmentType to modality name
    // Modality names are 'Image', 'Audio', 'Video', 'File'
    let modalityName = type.toLowerCase().trim();
    if (modalityName === 'document') {
      modalityName = 'file';
    const modality = AIEngineBase.Instance.Modalities.find(m => m.Name?.trim().toLowerCase() === modalityName);
      return modality.ID;
    // Fallback to 'file' modality if specific type not found
    // recursive call, so long as the current modality isn't file as that
    // would cause infinite recursion/stack overflow
    if (modalityName !== 'file') {
      return this.getModalityIdForType('Document');
   * Extract base64 data from a data URL
  private extractBase64FromDataUrl(dataUrl: string): string {
    const matches = dataUrl.match(/^data:[^;]+;base64,(.+)$/);
    return matches ? matches[1] : dataUrl;
   * Create a thumbnail from an image file.
   * Returns a base64 data URL of the thumbnail.
  async createThumbnail(
    file: File,
    maxSize: number = 200
      if (!file.type.startsWith('image/')) {
        resolve(null);
          let width = img.width;
          let height = img.height;
          // Scale down to maxSize
            if (width > maxSize) {
              height = Math.round((height * maxSize) / width);
              width = maxSize;
            if (height > maxSize) {
              width = Math.round((width * maxSize) / height);
              height = maxSize;
          canvas.width = width;
          canvas.height = height;
            ctx.drawImage(img, 0, 0, width, height);
            resolve(canvas.toDataURL('image/jpeg', 0.7));
        img.onerror = () => resolve(null);
        img.src = e.target?.result as string;
   * Get image dimensions from a file
  async getImageDimensions(file: File): Promise<{ width: number; height: number } | null> {
          resolve({ width: img.width, height: img.height });
   * Process a file and create a PendingAttachment.
   * This creates the data structure needed for the mention editor component.
  async processFile(file: File): Promise<PendingAttachment> {
    // Read file data URL
    const dataUrl = await this.fileToDataUrl(file);
    const pending: PendingAttachment = {
      id: this.generateTempId(),
      file: file,
      dataUrl: dataUrl,
    // Get image dimensions and create thumbnail
    const type = ConversationUtility.GetAttachmentTypeFromMime(file.type);
    if (type === 'Image') {
      const dimensions = await this.getImageDimensions(file);
      if (dimensions) {
        pending.width = dimensions.width;
        pending.height = dimensions.height;
      const thumbnail = await this.createThumbnail(file);
      if (thumbnail) {
        pending.thumbnailUrl = thumbnail;
    return pending;
   * Convert a file to a data URL
  private fileToDataUrl(file: File): Promise<string> {
      reader.onerror = () => reject(reader.error);
   * Generate a temporary ID for pending attachments
  private generateTempId(): string {
    return `temp-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
