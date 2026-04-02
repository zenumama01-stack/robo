 * Action that sends messages to Microsoft Teams via incoming webhooks
 *   ActionName: 'Teams Webhook',
 *     Value: 'https://company.webhook.office.com/webhookb2/...'
 *     Value: 'Build completed successfully!'
 * // Rich card with sections
 *     Value: 'Deployment Status'
 *     Value: 'Production deployment completed'
 *     Name: 'ThemeColor',
 *     Value: '00FF00'
 *     Name: 'Sections',
 *       activityTitle: 'Deployment Details',
 *       facts: [
 *         { name: 'Environment', value: 'Production' },
 *         { name: 'Version', value: 'v2.5.0' },
 *         { name: 'Duration', value: '5 minutes' }
 *       ]
 * // Adaptive Card
 *     Name: 'Card',
 *       type: 'AdaptiveCard',
 *       version: '1.2',
 *       body: [{
 *         type: 'TextBlock',
 *         text: 'Hello from MemberJunction!',
 *         size: 'Large',
 *         weight: 'Bolder'
@RegisterClass(BaseAction, "Teams Webhook")
export class TeamsWebhookAction extends BaseAction {
     * Sends messages to Microsoft Teams via webhook
     *   - WebhookURL: Teams incoming webhook URL (required)
     *   - Message: Plain text message (required if no Card)
     *   - Card: Adaptive Card JSON (optional, overrides other formatting)
     *   - Title: Card title (optional)
     *   - ThemeColor: Hex color for card accent (optional, e.g., '0076D7')
     *   - Summary: Card summary (optional)
     *   - Sections: Array of MessageCard sections (optional)
     *   - PotentialAction: Array of actions for the card (optional)
            const card = JSONParamHelper.getJSONParam(params, 'card');
            const title = this.getParamValue(params, 'title');
            const themeColor = this.getParamValue(params, 'themecolor');
            const summary = this.getParamValue(params, 'summary');
            const sections = JSONParamHelper.getJSONParam(params, 'sections');
            const potentialAction = JSONParamHelper.getJSONParam(params, 'potentialaction');
            if (!webhookURL.includes('webhook.office.com')) {
                    Message: "Invalid Teams webhook URL format",
            // Validate content
            if (!message && !card) {
                    Message: "Either Message or Card parameter is required",
            let payload: any;
            if (card) {
                // Use Adaptive Card
                payload = {
                    type: "message",
                    attachments: [{
                        contentType: "application/vnd.microsoft.card.adaptive",
                        content: card
                // Use MessageCard format
                    "@type": "MessageCard",
                    "@context": "https://schema.org/extensions"
                if (summary) {
                    payload.summary = summary;
                } else if (message) {
                    payload.summary = message.substring(0, 100);
                    payload.title = title;
                if (themeColor) {
                    // Ensure color is in hex format without #
                    payload.themeColor = themeColor.replace('#', '');
                if (sections && Array.isArray(sections)) {
                    payload.sections = sections;
                if (potentialAction && Array.isArray(potentialAction)) {
                    payload.potentialAction = potentialAction;
            // Send to Teams
                // Success response from Teams is "1"
                const isSuccess = response.data === 1 || response.data === "1";
                if (isSuccess) {
                        Name: 'TeamsResponse',
                            message: "Teams message sent successfully",
                        Message: `Teams webhook returned unexpected response: ${response.data}`,
                        ResultCode: "UNEXPECTED_RESPONSE"
            } else if (response.status === 400) {
                // Bad request - usually malformed payload
                    Message: `Invalid payload: ${response.data || 'Bad Request'}`,
                    ResultCode: "INVALID_PAYLOAD"
            } else if (response.status === 413) {
                // Payload too large
                    Message: "Payload too large. Teams webhooks have a size limit.",
                    ResultCode: "PAYLOAD_TOO_LARGE"
            } else if (response.status === 429) {
                // Rate limited
                    Message: "Rate limited. Too many requests to Teams webhook.",
                    ResultCode: "RATE_LIMITED"
                // Other error
                    Message: `Teams webhook failed: HTTP ${response.status} - ${response.data}`,
                    ResultCode: `HTTP_${response.status}`
                Message: `Teams webhook failed: ${error instanceof Error ? error.message : String(error)}`,
