    ProviderOperation
import sgMail, { MailDataRequired } from '@sendgrid/mail';
import { __API_KEY } from "./config";
 * Credentials for SendGrid email provider.
 *     apiKey: 'SG.your-api-key'
 *     apiKey: 'SG.your-api-key',
export interface SendGridCredentials extends ProviderCredentialsBase {
     * SendGrid API key. Typically starts with 'SG.'
     * If not provided, falls back to COMMUNICATION_VENDOR_API_KEY__SENDGRID environment variable.
 * Implementation of the SendGrid provider for sending and receiving messages.
 * SendGrid is a transactional email service. This provider supports:
 * - Sending single messages
 * - Sending to multiple recipients (via engine)
 * It does NOT support:
 * - Fetching messages (no inbox access)
 * await engine.SendSingleMessage('SendGrid', 'Standard Email', message);
 * await engine.SendSingleMessage('SendGrid', 'Standard Email', message, undefined, false, {
 *     apiKey: 'SG.customer-specific-key'
@RegisterClass(BaseCommunicationProvider, 'SendGrid')
export class SendGridProvider extends BaseCommunicationProvider {
     * Returns the list of operations supported by the SendGrid provider.
     * SendGrid is a transactional email service (send-only) and does not support
     * mailbox operations like fetching messages, folders, or attachments.
            'SendSingleMessage'
            // Note: SendGrid is send-only - no inbox access, no fetching, no drafts
            // GetMessages, ForwardMessage, ReplyToMessage throw errors
            // CreateDraft returns not supported
     * Sends a single message using SendGrid.
     * @param credentials - Optional SendGrid credentials override
        credentials?: SendGridCredentials
        // Resolve credentials: request values override env vars
        const apiKey = resolveCredentialValue(
            credentials?.apiKey,
            __API_KEY,
            disableFallback
        validateRequiredCredentials({ apiKey }, ['apiKey'], 'SendGrid');
        const from: string = message.From;
        // Set API key for this request
        sgMail.setApiKey(apiKey!);
        const msg: MailDataRequired = {
            to: message.To,
                email: from,
                name: message.FromName
            cc: message.CCRecipients,
            bcc: message.BCCRecipients,
            subject: message.ProcessedSubject,
            text: message.ProcessedBody,
            html: message.ProcessedHTMLBody,
            trackingSettings: {
                subscriptionTracking: {
                    enable: false
        * Should be ready to go - but needs SG testing.
        if(message.Headers){
            msg.headers = Object.fromEntries(Object.entries(message.Headers).map(([key, value]) => [`X-${key}`, value])) as Record<string, string>;
        if (message.SendAt) {
            const time = message.SendAt.getTime();
            const unitTime = Math.floor(time / 1000);
            msg.sendAt = unitTime;
            const result = await sgMail.send(msg);
            if (result && result.length > 0 && result[0].statusCode >= 200 && result[0].statusCode < 300) {
                LogStatus(`Email sent to ${msg.to}: ${result[0].statusCode}`);
                LogError(`Error sending email to ${msg.to}:`, undefined, result);
                LogError(result[0].body);
                    Error: result[0].toString()
            const errorResponse = (error as { response?: { body?: unknown } })?.response?.body;
            LogError(`Error sending email to ${msg.to}:`, undefined, error);
            if (errorResponse) {
                LogError(errorResponse);
     * Fetches messages from the provider.
     * @remarks SendGrid does not support fetching messages (no inbox access).
        throw new Error("SendGridProvider does not support fetching messages");
     * Forwards a message to another client using the provider.
     * @remarks SendGrid does not support forwarding messages.
    public ForwardMessage(
        throw new Error("SendGridProvider does not support forwarding messages");
     * Replies to a message using the provider.
     * @remarks SendGrid does not support replying to messages.
    public ReplyToMessage(
        throw new Error("SendGridProvider does not support replying to messages");
     * @remarks SendGrid does not support creating drafts (no mailbox access).
            ErrorMessage: 'SendGrid does not support creating draft messages. Drafts are only supported by email providers with mailbox access (Gmail, MS Graph).'
