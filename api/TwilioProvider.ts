import twilio, { Twilio } from 'twilio';
 * Credentials for Twilio provider.
export interface TwilioCredentials extends ProviderCredentialsBase {
  /** Twilio Account SID */
  accountSid?: string;
  /** Twilio Auth Token */
  authToken?: string;
  /** Twilio Phone Number for SMS */
  phoneNumber?: string;
  /** Optional WhatsApp number (if using WhatsApp messaging) */
  whatsappNumber?: string;
  /** Optional Facebook Page ID (if using Facebook Messenger) */
  facebookPageId?: string;
 * Resolved Twilio credentials after merging request credentials with environment fallback
interface ResolvedTwilioCredentials {
  accountSid: string;
  authToken: string;
  whatsappNumber: string;
  facebookPageId: string;
 * Implementation of the Twilio provider for sending and receiving messages (SMS, WhatsApp, Facebook Messenger)
@RegisterClass(BaseCommunicationProvider, 'Twilio')
export class TwilioProvider extends BaseCommunicationProvider {
  /** Cached Twilio client for environment credentials */
  private envTwilioClient: Twilio | null = null;
  /** Cache of Twilio clients for per-request credentials */
  private clientCache: Map<string, Twilio> = new Map();
   * Returns the list of operations supported by the Twilio provider.
   * Twilio is a messaging provider (SMS, WhatsApp, Messenger) and does not support
   * mailbox operations like folders, archiving, or attachments.
      'ReplyToMessage'
      // Note: CreateDraft is NOT supported - Twilio is real-time messaging only
      // Mailbox operations (folders, archive, attachments) are not applicable to SMS/messaging
  private resolveCredentials(credentials?: TwilioCredentials): ResolvedTwilioCredentials {
    const accountSid = resolveCredentialValue(credentials?.accountSid, Config.TWILIO_ACCOUNT_SID, disableFallback);
    const authToken = resolveCredentialValue(credentials?.authToken, Config.TWILIO_AUTH_TOKEN, disableFallback);
    const phoneNumber = resolveCredentialValue(credentials?.phoneNumber, Config.TWILIO_PHONE_NUMBER, disableFallback);
    const whatsappNumber = resolveCredentialValue(credentials?.whatsappNumber, Config.TWILIO_WHATSAPP_NUMBER, disableFallback);
    const facebookPageId = resolveCredentialValue(credentials?.facebookPageId, Config.TWILIO_FACEBOOK_PAGE_ID, disableFallback);
      { accountSid, authToken, phoneNumber },
      ['accountSid', 'authToken', 'phoneNumber'],
      'Twilio'
      accountSid: accountSid!,
      authToken: authToken!,
      phoneNumber: phoneNumber!,
      whatsappNumber: whatsappNumber || '',
      facebookPageId: facebookPageId || ''
   * Gets a Twilio client for the given credentials, using caching for efficiency
  private getTwilioClient(creds: ResolvedTwilioCredentials): Twilio {
      creds.accountSid === Config.TWILIO_ACCOUNT_SID &&
      creds.authToken === Config.TWILIO_AUTH_TOKEN;
      if (!this.envTwilioClient) {
        this.envTwilioClient = twilio(creds.accountSid, creds.authToken);
      return this.envTwilioClient;
    const cacheKey = `${creds.accountSid}`;
      client = twilio(creds.accountSid, creds.authToken);
   * Determine the message channel type based on recipient format
  private getChannelType(to: string): 'sms' | 'whatsapp' | 'messenger' {
    if (to.startsWith('whatsapp:')) {
      return 'whatsapp';
    } else if (to.startsWith('messenger:')) {
      return 'messenger';
      return 'sms';
   * Format the sender number/ID based on channel type and credentials
  private formatFrom(channelType: 'sms' | 'whatsapp' | 'messenger', creds: ResolvedTwilioCredentials): string {
    switch (channelType) {
      case 'whatsapp':
        return creds.whatsappNumber ? `whatsapp:${creds.whatsappNumber}` : '';
      case 'messenger':
        return creds.facebookPageId ? `messenger:${creds.facebookPageId}` : '';
      case 'sms':
        return creds.phoneNumber;
   * Format the recipient number/ID if needed
  private formatTo(to: string, channelType: 'sms' | 'whatsapp' | 'messenger'): string {
    // If already formatted with prefix, return as is
    if (to.startsWith('whatsapp:') || to.startsWith('messenger:')) {
      return to;
    // Format based on channel type
        return `whatsapp:${to}`;
        return `messenger:${to}`;
   * Sends a single message using Twilio
    credentials?: TwilioCredentials
      if (!message.To) {
          Error: 'Recipient not specified'
      const twilioClient = this.getTwilioClient(creds);
      // Determine channel type (SMS, WhatsApp, Messenger)
      const channelType = this.getChannelType(message.To);
      // Format sender and recipient
      const from = message.From || this.formatFrom(channelType, creds);
      const to = this.formatTo(message.To, channelType);
      // Ensure from is configured for this channel
      if (!from) {
          Error: `${channelType.toUpperCase()} sender not configured`
      // Prepare message body
      // For SMS and messaging channels, we use plain text
      // HTML is not supported, so we use the text body
      const body = message.ProcessedBody || '';
      // Optional media URLs if specified in context data
      const mediaUrls = message.ContextData?.mediaUrls as string[] || [];
      // Send the message
      const result = await twilioClient.messages.create({
        from,
        to,
        ...(mediaUrls.length > 0 && { mediaUrl: mediaUrls })
      LogStatus(`${channelType.toUpperCase()} message sent via Twilio (SID: ${result.sid})`);
      LogError('Error sending message via Twilio', undefined, error);
   * Gets messages from Twilio
      const queryParams: Record<string, unknown> = {
        limit: params.NumMessages
      // Filter by date sent
      if (params.ContextData?.dateSent) {
        queryParams.dateSent = params.ContextData.dateSent;
      // Filter by sender
      if (params.ContextData?.from) {
        queryParams.from = params.ContextData.from;
      // Filter by recipient
      queryParams.to = params.Identifier || params.ContextData?.to || undefined;
      // Fetch messages
      const messages = await twilioClient.messages.list(queryParams);
      // Format messages into standard structure
      const formattedMessages = messages.map((message) => {
          From: message.from || '',
          To: message.to || '',
          Body: message.body || '',
          ExternalSystemRecordID: message.sid,
          Subject: '', // SMS doesn't have subject
          ThreadID: message.sid // Using message SID as thread ID as Twilio doesn't have thread concept
        Messages: formattedMessages,
        SourceData: messages
      const errorMessage = error instanceof Error ? error.message : 'Error fetching messages';
      LogError('Error fetching messages from Twilio', undefined, error);
   * Reply to a message using Twilio
      // Get original message to determine recipient and channel
      const originalMessage = await twilioClient.messages(params.MessageID).fetch();
      if (!originalMessage) {
          ErrorMessage: 'Original message not found'
      // The recipient of our reply is the sender of the original message
      const to = originalMessage.from || '';
      if (!to) {
          ErrorMessage: 'Could not determine recipient for reply'
      // Determine channel type
      const channelType = this.getChannelType(to);
      // Format sender
      const from = params.Message.From || this.formatFrom(channelType, creds);
      // Prepare message content
      const body = params.Message.ProcessedBody || '';
      // Optional media URLs
      const mediaUrls = params.Message.ContextData?.mediaUrls as string[] || [];
      // Send the reply
      LogError('Error replying to message via Twilio', undefined, error);
   * Forward a message using Twilio
   * Note: Twilio doesn't have a native "forward" concept, so we implement it as a new message
   * that includes the content of the original message
      if (!params.MessageID || !params.ToRecipients || params.ToRecipients.length === 0) {
          ErrorMessage: 'Message ID or recipients not provided'
      // Create forwarded message content
      const forwardPrefix = 'Forwarded message:\n';
      const originalSender = `From: ${originalMessage.from}\n`;
      const originalContent = originalMessage.body || '';
      const forwardComment = params.Message ? `${params.Message}\n\n` : '';
      const body = `${forwardComment}${forwardPrefix}${originalSender}${originalContent}`;
      // Send to all recipients
      const results = await Promise.all(params.ToRecipients.map(async (recipient) => {
        const channelType = this.getChannelType(recipient);
        const from = this.formatFrom(channelType, creds);
        const to = this.formatTo(recipient, channelType);
        return twilioClient.messages.create({
          // If original had media, we can forward it
          ...(originalMessage.numMedia !== '0' && { mediaUrl: [originalMessage.uri] })
        Result: results
      LogError('Error forwarding message via Twilio', undefined, error);
   * Twilio does not support creating draft messages
   * @param params - Parameters for creating a draft (not used)
   * @param credentials - Optional credentials (not used for Twilio)
      ErrorMessage: 'Twilio does not support creating draft messages. Drafts are only supported by email providers with mailbox access (Gmail, MS Graph).'
