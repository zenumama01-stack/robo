  GetMessageMessage,
  DownloadAttachmentResult
import googleApis from 'googleapis';
 * Credentials for Gmail provider using OAuth2.
 * Extend ProviderCredentialsBase to support per-request credential override.
 * **TEMPORARY INTERFACE**: This interface is part of the interim credential solution for 2.x patch release.
 * In a future release, this will be integrated with the comprehensive credential management system.
export interface GmailCredentials extends ProviderCredentialsBase {
  /** Google OAuth2 Client ID */
  /** Google OAuth2 Client Secret */
  /** OAuth2 Redirect URI */
  /** OAuth2 Refresh Token */
  /** Service account email (optional) */
  serviceAccountEmail?: string;
 * Resolved Gmail credentials after merging request credentials with environment fallback
interface ResolvedGmailCredentials {
  refreshToken: string;
  serviceAccountEmail: string;
 * Cached Gmail client with associated user email
interface CachedGmailClient {
  client: googleApis.gmail_v1.Gmail;
  userEmail: string | null;
 * Implementation of the Gmail provider for sending and receiving messages
@RegisterClass(BaseCommunicationProvider, 'Gmail')
export class GmailProvider extends BaseCommunicationProvider {
  /** Cached Gmail client for environment credentials */
  private envGmailClient: CachedGmailClient | null = null;
  /** Cache of Gmail clients for per-request credentials */
  private clientCache: Map<string, CachedGmailClient> = new Map();
   * Resolves credentials by merging request credentials with environment fallback
  private resolveCredentials(credentials?: GmailCredentials): ResolvedGmailCredentials {
    const clientId = resolveCredentialValue(credentials?.clientId, Config.GMAIL_CLIENT_ID, disableFallback);
    const clientSecret = resolveCredentialValue(credentials?.clientSecret, Config.GMAIL_CLIENT_SECRET, disableFallback);
    const redirectUri = resolveCredentialValue(credentials?.redirectUri, Config.GMAIL_REDIRECT_URI, disableFallback);
    const refreshToken = resolveCredentialValue(credentials?.refreshToken, Config.GMAIL_REFRESH_TOKEN, disableFallback);
    const serviceAccountEmail = resolveCredentialValue(credentials?.serviceAccountEmail, Config.GMAIL_SERVICE_ACCOUNT_EMAIL, disableFallback);
    // Validate required credentials
      { clientId, clientSecret, redirectUri, refreshToken },
      ['clientId', 'clientSecret', 'redirectUri', 'refreshToken'],
      'Gmail'
      redirectUri: redirectUri!,
      refreshToken: refreshToken!,
      serviceAccountEmail: serviceAccountEmail || ''
   * Creates a Gmail client with the given credentials
  private createGmailClient(creds: ResolvedGmailCredentials): googleApis.gmail_v1.Gmail {
    // Create OAuth2 client
    const oauth2Client = new googleApis.google.auth.OAuth2(
      creds.clientSecret,
      creds.redirectUri
    // Set refresh token to automatically refresh access tokens
    oauth2Client.setCredentials({
      refresh_token: creds.refreshToken
    // Create Gmail API client
    return googleApis.google.gmail({
      version: 'v1',
      auth: oauth2Client
   * Gets a Gmail client for the given credentials, using caching for efficiency
  private getGmailClient(creds: ResolvedGmailCredentials): CachedGmailClient {
      creds.clientId === Config.GMAIL_CLIENT_ID &&
      creds.clientSecret === Config.GMAIL_CLIENT_SECRET &&
      creds.refreshToken === Config.GMAIL_REFRESH_TOKEN;
      if (!this.envGmailClient) {
        this.envGmailClient = {
          client: this.createGmailClient(creds),
          userEmail: null
      return this.envGmailClient;
    const cacheKey = `${creds.clientId}:${creds.refreshToken.substring(0, 10)}`;
    let cached = this.clientCache.get(cacheKey);
    if (!cached) {
      cached = {
      this.clientCache.set(cacheKey, cached);
   * Gets the authenticated user's email address for a given cached client
  private async getUserEmail(cached: CachedGmailClient): Promise<string | null> {
    if (cached.userEmail) {
      return cached.userEmail;
      // Get user profile to verify authentication
      const response = await cached.client.users.getProfile({
        userId: 'me'
      if (response.data && response.data.emailAddress) {
        cached.userEmail = response.data.emailAddress;
      LogError('Failed to get Gmail user email', undefined, error);
   * Encode and format email content for Gmail API
  private createEmailContent(message: ProcessedMessage, creds: ResolvedGmailCredentials): string {
    // Get sender email
    const from = message.From || creds.serviceAccountEmail;
    const fromName = message.FromName || '';
    const fromHeader = fromName ? `${fromName} <${from}>` : from;
    // Create email content
    const subject = message.ProcessedSubject;
    const to = message.To;
    const cc = message.CCRecipients?.join(', ') || '';
    const bcc = message.BCCRecipients?.join(', ') || '';
    let emailContent = [
      `From: ${fromHeader}`,
      `To: ${to}`,
      `Subject: ${subject}`
    // Add CC and BCC if present
    if (cc) emailContent.push(`Cc: ${cc}`);
    if (bcc) emailContent.push(`Bcc: ${bcc}`);
    // Add content type and message body
    if (message.ProcessedHTMLBody) {
      // For HTML emails
      const boundary = `boundary_${Math.random().toString(36).substring(2)}`;
      emailContent.push('MIME-Version: 1.0');
      emailContent.push(`Content-Type: multipart/alternative; boundary="${boundary}"`);
      emailContent.push('');
      // Text part
      emailContent.push(`--${boundary}`);
      emailContent.push('Content-Type: text/plain; charset=UTF-8');
      emailContent.push(message.ProcessedBody || '');
      // HTML part
      emailContent.push('Content-Type: text/html; charset=UTF-8');
      emailContent.push(message.ProcessedHTMLBody);
      emailContent.push(`--${boundary}--`);
      // Plain text email
    return Buffer.from(emailContent.join('\r\n')).toString('base64').replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
   * Sends a single message using the Gmail API
    credentials?: GmailCredentials
      // Resolve credentials (request credentials with env fallback)
      const cached = this.getGmailClient(creds);
      // Get user email
      const userEmail = await this.getUserEmail(cached);
      if (!userEmail) {
          Error: 'Could not get user email'
      // Create raw email content in base64 URL-safe format
      const raw = this.createEmailContent(message, creds);
      // Send the email
      const result = await cached.client.users.messages.send({
        userId: 'me',
        requestBody: {
          raw
      if (result && result.status >= 200 && result.status < 300) {
        LogStatus(`Email sent via Gmail: ${result.statusText}`);
        LogError('Failed to send email via Gmail', undefined, result);
          Error: `Failed to send email: ${result?.statusText || 'Unknown error'}`
      const errorMessage = error instanceof Error ? error.message : 'Error sending message';
      LogError('Error sending message via Gmail', undefined, error);
        Error: errorMessage
   * Gets messages from Gmail
  ): Promise<GetMessagesResult> {
          Messages: [],
          ErrorMessage: 'Could not get user email'
      // Build query
      let query = '';
        query = 'is:unread';
      if (params.ContextData?.query) {
        query = params.ContextData.query as string;
      // Get messages
      const response = await cached.client.users.messages.list({
        maxResults: params.NumMessages,
        q: query
      if (!response.data.messages || response.data.messages.length === 0) {
      // Get full message details for each message ID
      const messagePromises = response.data.messages.map(async (message) => {
        const fullMessage = await cached.client.users.messages.get({
          id: message.id || '',
          format: 'full'
        return fullMessage.data;
      const fullMessages = await Promise.all(messagePromises);
      // Process messages into standard format
      const processedMessages: GetMessageMessage[] = fullMessages.map(message => {
        // Extract headers
        const headers = message.payload?.headers || [];
        const getHeader = (name: string) => {
          const header = headers.find(h => h.name?.toLowerCase() === name.toLowerCase());
          return header ? header.value : '';
        const from = getHeader('from');
        const to = getHeader('to');
        const subject = getHeader('subject');
        const replyTo = getHeader('reply-to') ? [getHeader('reply-to')] : [from];
        // Extract body
        let body = '';
        if (message.payload?.body?.data) {
          // Base64 encoded data
          body = Buffer.from(message.payload.body.data, 'base64').toString('utf-8');
        } else if (message.payload?.parts) {
          // Multipart message, try to find text part
          const textPart = message.payload.parts.find(part => part.mimeType === 'text/plain');
          if (textPart && textPart.body?.data) {
            body = Buffer.from(textPart.body.data, 'base64').toString('utf-8');
          From: from || '',
          To: to || '',
          ReplyTo: replyTo.map(r => r || ''),
          Subject: subject || '',
          Body: body,
          ExternalSystemRecordID: message.id || '',
          ThreadID: message.threadId || ''
      // Mark as read if requested
      if (params.ContextData?.MarkAsRead) {
        for (const message of fullMessages) {
          if (message.id) {
            await this.markMessageAsRead(cached.client, message.id);
        Messages: processedMessages,
        SourceData: fullMessages
      const errorMessage = error instanceof Error ? error.message : 'Error getting messages';
      LogError('Error getting messages from Gmail', undefined, error);
   * Reply to a message using Gmail API
   * @param params - Parameters for replying to a message
          ErrorMessage: 'Message ID not provided'
      // Get the original message to obtain threadId
      const originalMessage = await cached.client.users.messages.get({
        id: params.MessageID
      if (!originalMessage.data.threadId) {
          ErrorMessage: 'Could not get thread ID from original message'
      // Create raw email content
      const raw = this.createEmailContent(params.Message, creds);
      // Send the reply in the same thread
          raw,
          threadId: originalMessage.data.threadId
          Result: result.data
          ErrorMessage: `Failed to reply to message: ${result?.statusText || 'Unknown error'}`
      const errorMessage = error instanceof Error ? error.message : 'Error replying to message';
      LogError('Error replying to message via Gmail', undefined, error);
   * Forward a message using Gmail API
   * @param params - Parameters for forwarding a message
      // Get the original message
        id: params.MessageID,
        format: 'raw'
      if (!originalMessage.data.raw) {
          ErrorMessage: 'Could not get raw content of original message'
      // Convert raw message to proper format
      const rawContent = Buffer.from(originalMessage.data.raw, 'base64').toString('utf-8');
      // Build forwarded message
      const to = params.ToRecipients.join(', ');
      const cc = params.CCRecipients?.join(', ') || '';
      const bcc = params.BCCRecipients?.join(', ') || '';
      // Parse the original email to extract subject
      const subjectMatch = rawContent.match(/Subject: (.*?)(\r?\n)/);
      const subject = subjectMatch ? `Fwd: ${subjectMatch[1]}` : 'Fwd: ';
      // Headers for new message
      const emailContent = [
        `From: ${userEmail}`,
      // Add content type
      emailContent.push(`Content-Type: multipart/mixed; boundary="${boundary}"`);
      // Forward comment
      if (params.Message) {
        emailContent.push(params.Message);
      // Original message as attachment
      emailContent.push('Content-Type: message/rfc822; name="forwarded_message.eml"');
      emailContent.push('Content-Disposition: attachment; filename="forwarded_message.eml"');
      emailContent.push(rawContent);
      // Encode email content
      const raw = Buffer.from(emailContent.join('\r\n')).toString('base64')
        .replace(/=+$/, '');
      // Send the forwarded message
          ErrorMessage: `Failed to forward message: ${result?.statusText || 'Unknown error'}`
      const errorMessage = error instanceof Error ? error.message : 'Error forwarding message';
      LogError('Error forwarding message via Gmail', undefined, error);
   * Helper to mark a message as read
  private async markMessageAsRead(gmailClient: googleApis.gmail_v1.Gmail, messageId: string): Promise<boolean> {
      await gmailClient.users.messages.modify({
        id: messageId,
          removeLabelIds: ['UNREAD']
      LogError(`Error marking message ${messageId} as read`, undefined, error);
   * Creates a draft message in Gmail
   * @param params - Parameters for creating a draft
      // Reuse existing email content creation logic
      // Create draft using Gmail API
      const result = await cached.client.users.drafts.create({
          message: { raw }
        LogStatus(`Draft created via Gmail: ${result.data.id}`);
          DraftID: result.data.id || undefined,
        LogError('Failed to create draft via Gmail', undefined, result);
          ErrorMessage: `Failed to create draft: ${result?.statusText || 'Unknown error'}`
      LogError('Error creating draft via Gmail', undefined, error);
  // EXTENDED OPERATIONS - Gmail supports all mailbox operations via labels
   * Returns the list of operations supported by the Gmail provider.
   * Gmail supports all operations through its label-based system.
      'ListFolders',       // Gmail uses labels instead of folders
   * Gets a single message by ID
      const response = await cached.client.users.messages.get({
      const message = this.parseGmailMessage(response.data);
        SourceData: response.data
      const errorMessage = error instanceof Error ? error.message : 'Error getting message';
      LogError(`Error getting message ${params.MessageID} from Gmail`, undefined, error);
   * Deletes a message from Gmail
        await cached.client.users.messages.delete({
        // Move to trash (adds TRASH label, removes INBOX)
        await cached.client.users.messages.trash({
      LogStatus(`Message ${params.MessageID} deleted from Gmail (permanent: ${params.PermanentDelete})`);
      const errorMessage = error instanceof Error ? error.message : 'Error deleting message';
      LogError(`Error deleting message ${params.MessageID} from Gmail`, undefined, error);
   * Moves a message to a different label (Gmail's equivalent of folders)
   * In Gmail, moving is done by adding/removing labels
      // First get current labels on the message
      const message = await cached.client.users.messages.get({
        format: 'minimal'
      const currentLabels = message.data.labelIds || [];
      // Remove INBOX and other category labels, add the destination label
      const labelsToRemove = currentLabels.filter(label =>
        label === 'INBOX' ||
        label === 'CATEGORY_PERSONAL' ||
        label === 'CATEGORY_SOCIAL' ||
        label === 'CATEGORY_PROMOTIONS' ||
        label === 'CATEGORY_UPDATES' ||
        label === 'CATEGORY_FORUMS'
      await cached.client.users.messages.modify({
          addLabelIds: [params.DestinationFolderID],
          removeLabelIds: labelsToRemove
      LogStatus(`Message ${params.MessageID} moved to label ${params.DestinationFolderID}`);
        NewMessageID: params.MessageID // Gmail doesn't change message ID on move
      const errorMessage = error instanceof Error ? error.message : 'Error moving message';
      LogError(`Error moving message ${params.MessageID} in Gmail`, undefined, error);
   * Lists Gmail labels (Gmail's equivalent of folders)
   * @param params - Parameters for listing labels
      const response = await cached.client.users.labels.list({
      if (!response.data.labels) {
      // Get detailed info for each label if counts requested
      let labels = response.data.labels;
      if (params.IncludeCounts) {
        const detailedLabels = await Promise.all(
          labels.map(async (label) => {
            if (!label.id) return label;
              const detail = await cached.client.users.labels.get({
                id: label.id
              return detail.data;
        labels = detailedLabels;
      const folders: MessageFolder[] = labels.map(label => ({
        ID: label.id || '',
        Name: label.name || '',
        MessageCount: label.messagesTotal || undefined,
        UnreadCount: label.messagesUnread || undefined,
        IsSystemFolder: label.type === 'system',
        SystemFolderType: this.mapGmailLabelToSystemFolder(label.id || '')
      // Filter by parent if specified (Gmail doesn't have nested labels in the API the same way)
      // User labels can have "/" in names to simulate hierarchy
        const parent = folders.find(f => f.ID === params.ParentFolderID);
          const parentPrefix = parent.Name + '/';
            Folders: folders.filter(f => f.Name.startsWith(parentPrefix)),
            Result: labels
      const errorMessage = error instanceof Error ? error.message : 'Error listing labels';
      LogError('Error listing labels from Gmail', undefined, error);
   * Marks messages as read or unread
      // Process all messages
      await Promise.all(
        params.MessageIDs.map(async (messageId) => {
            requestBody: params.IsRead
              ? { removeLabelIds: ['UNREAD'] }
              : { addLabelIds: ['UNREAD'] }
      const errorMessage = error instanceof Error ? error.message : 'Error marking messages';
      LogError('Error marking messages as read/unread in Gmail', undefined, error);
   * Archives a message (removes INBOX label in Gmail)
      // In Gmail, archiving is simply removing the INBOX label
          removeLabelIds: ['INBOX']
      LogStatus(`Message ${params.MessageID} archived in Gmail`);
      const errorMessage = error instanceof Error ? error.message : 'Error archiving message';
      LogError(`Error archiving message ${params.MessageID} in Gmail`, undefined, error);
   * Searches messages using Gmail's search syntax
      // Build Gmail search query
      let query = params.Query;
      // Add date filters if specified
        const fromDateStr = this.formatDateForGmail(params.FromDate);
        query += ` after:${fromDateStr}`;
        const toDateStr = this.formatDateForGmail(params.ToDate);
        query += ` before:${toDateStr}`;
      // Add folder/label filter
        query += ` label:${params.FolderID}`;
      // Search messages
        maxResults: params.MaxResults || 50
          TotalCount: 0
      // Get full message details
      const fullMessages = await Promise.all(
        response.data.messages.map(async (msg) => {
          const full = await cached.client.users.messages.get({
            id: msg.id || '',
          return full.data;
      const messages = fullMessages.map(msg => this.parseGmailMessage(msg));
        TotalCount: response.data.resultSizeEstimate || messages.length,
      const errorMessage = error instanceof Error ? error.message : 'Error searching messages';
      LogError('Error searching messages in Gmail', undefined, error);
   * Lists attachments on a message
      if (!response.data.payload) {
      const attachments: MessageAttachment[] = [];
      this.extractAttachments(response.data.payload, attachments);
        Result: response.data
      const errorMessage = error instanceof Error ? error.message : 'Error listing attachments';
      LogError(`Error listing attachments for message ${params.MessageID}`, undefined, error);
   * Downloads an attachment from a message
      // First get attachment metadata to find filename and content type
      let attachmentInfo: { filename: string; contentType: string } | null = null;
      if (message.data.payload) {
        attachmentInfo = this.findAttachmentInfo(message.data.payload, params.AttachmentID);
      // Download the attachment
      const response = await cached.client.users.messages.attachments.get({
        messageId: params.MessageID,
        id: params.AttachmentID
      if (!response.data.data) {
          ErrorMessage: 'Attachment content not found'
      // Gmail returns base64url encoded data, convert to standard base64
      const base64Data = response.data.data.replace(/-/g, '+').replace(/_/g, '/');
      const content = Buffer.from(base64Data, 'base64');
        ContentBase64: base64Data,
        Filename: attachmentInfo?.filename || 'attachment',
        ContentType: attachmentInfo?.contentType || 'application/octet-stream',
      const errorMessage = error instanceof Error ? error.message : 'Error downloading attachment';
      LogError(`Error downloading attachment ${params.AttachmentID}`, undefined, error);
   * Parses a Gmail message into the standard GetMessageMessage format
  private parseGmailMessage(message: googleApis.gmail_v1.Schema$Message): GetMessageMessage {
    const dateStr = getHeader('date');
    // Parse date
    let receivedAt: Date | undefined;
    if (dateStr) {
        receivedAt = new Date(dateStr);
    // Internal date from Gmail (epoch milliseconds)
    let createdAt: Date | undefined;
    if (message.internalDate) {
      createdAt = new Date(parseInt(message.internalDate, 10));
      ReplyTo: replyTo.map(r => r || '').filter(r => r !== ''),
      ThreadID: message.threadId || '',
      ReceivedAt: receivedAt,
      CreatedAt: createdAt
   * Maps Gmail label IDs to system folder types
  private mapGmailLabelToSystemFolder(labelId: string): MessageFolder['SystemFolderType'] {
    const labelMap: Record<string, MessageFolder['SystemFolderType']> = {
      'INBOX': 'inbox',
      'SENT': 'sent',
      'DRAFT': 'drafts',
      'TRASH': 'trash',
      'SPAM': 'spam',
      'STARRED': 'other',
      'IMPORTANT': 'other',
      'UNREAD': 'other',
      'CATEGORY_PERSONAL': 'other',
      'CATEGORY_SOCIAL': 'other',
      'CATEGORY_PROMOTIONS': 'other',
      'CATEGORY_UPDATES': 'other',
      'CATEGORY_FORUMS': 'other'
    return labelMap[labelId] || undefined;
   * Formats a date for Gmail search query (YYYY/MM/DD)
  private formatDateForGmail(date: Date): string {
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    return `${year}/${month}/${day}`;
   * Recursively extracts attachment information from message parts
  private extractAttachments(
    part: googleApis.gmail_v1.Schema$MessagePart,
    attachments: MessageAttachment[]
    // Check if this part is an attachment
    if (part.filename && part.body?.attachmentId) {
      attachments.push({
        ID: part.body.attachmentId,
        Filename: part.filename,
        ContentType: part.mimeType || 'application/octet-stream',
        Size: part.body.size || 0,
        IsInline: part.headers?.some(h =>
          h.name?.toLowerCase() === 'content-disposition' &&
          h.value?.toLowerCase().includes('inline')
        ) || false,
        ContentID: part.headers?.find(h =>
          h.name?.toLowerCase() === 'content-id'
        )?.value?.replace(/[<>]/g, '') || undefined
    // Recursively process nested parts
    if (part.parts) {
      for (const nestedPart of part.parts) {
        this.extractAttachments(nestedPart, attachments);
   * Finds attachment info (filename, content type) by attachment ID
  private findAttachmentInfo(
    attachmentId: string
  ): { filename: string; contentType: string } | null {
    if (part.body?.attachmentId === attachmentId) {
        filename: part.filename || 'attachment',
        contentType: part.mimeType || 'application/octet-stream'
        const result = this.findAttachmentInfo(nestedPart, attachmentId);
