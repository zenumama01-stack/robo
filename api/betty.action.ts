 * Betty Action - Interface to organization's knowledge base assistant
 * Provides access to Betty Bot, an AI assistant trained on your organization's
 * knowledge base including documentation, policies, procedures, and institutional knowledge.
 * Betty can answer questions, provide guidance, and help users find relevant information
 * from your company's internal knowledge repository.
 * @module @memberjunction/actions
import { BettyBotLLM } from "@memberjunction/ai-betty-bot";
import { ChatParams, ChatMessageRole, ChatMessage, GetAIAPIKey } from "@memberjunction/ai";
 * Betty Action - Queries your organization's knowledge base assistant
 * This action provides access to Betty Bot, an AI assistant specifically trained on your
 * organization's knowledge base. Betty can answer questions about company policies, procedures,
 * documentation, and other institutional knowledge.
 * - Answers questions using your organization's knowledge base
 * - Provides contextual responses with optional reference links
 * - Supports full conversation history for context-aware interactions
 * - Returns structured responses with optional supporting documentation
 * Response format:
 * - BettyResponse: Text answer from Betty
 * - BettyReferences: Array of reference objects (if available)
 *   - Each reference contains: { link: string, title: string, type: string }
 * // Simple question
 *   ActionName: 'Betty',
 *     Name: 'UserPrompt',
 *     Value: 'What is our vacation policy?'
 * // Question with full conversation context
 *     Name: 'ConversationMessages',
 *     Value: [
 *       { role: 'user', content: 'What is our vacation policy?' },
 *       { role: 'assistant', content: 'Our vacation policy...' },
 *       { role: 'user', content: 'How do I request time off?' }
@RegisterClass(BaseAction, "BettyAction")
export class BettyAction extends BaseAction {
     * Executes Betty knowledge base query
     *   - UserPrompt: Direct question for Betty (alternative to ConversationMessages)
     *   - ConversationMessages: Full conversation history for context-aware responses (preferred)
     * @returns Betty's response with optional reference links
            const userPromptParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'userprompt');
            const conversationMessagesParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'conversationmessages');
            // Build conversation messages array
            let messages: ChatMessage[];
            if (conversationMessagesParam && conversationMessagesParam.Value) {
                // Use full conversation history if provided
                if (Array.isArray(conversationMessagesParam.Value)) {
                    messages = conversationMessagesParam.Value as ChatMessage[];
                        Message: "ConversationMessages parameter must be an array of ChatMessage objects",
                        ResultCode: "INVALID_PARAMETERS"
            } else if (userPromptParam && userPromptParam.Value) {
                // Fallback to simple user prompt
                messages = [{
                    content: userPromptParam.Value.toString().trim()
                    Message: "Either UserPrompt or ConversationMessages parameter is required",
                    ResultCode: "MISSING_PARAMETERS"
            // Validate we have at least one message
                    Message: "ConversationMessages cannot be empty",
            // Get Betty API key using AI package standard approach
            const apiKey = GetAIAPIKey('BettyBotLLM');
                    Message: "Betty API key not found. Set AI_VENDOR_API_KEY__BETTYBOTLLM environment variable",
                    ResultCode: "MISSING_API_KEY"
            // Create Betty LLM instance
            const betty = new BettyBotLLM(apiKey);
            const chatParams: ChatParams = {
                model: 'betty', // Betty doesn't use model selection, but required by interface
                temperature: 0.7, // Default temperature
                maxOutputTokens: 2000 // Reasonable default
            // Execute chat completion
            const result = await betty.ChatCompletion(chatParams);
            if (!result.success || !result.data) {
                    Message: result.errorMessage || "Betty returned an error",
                    ResultCode: "BETTY_ERROR"
            // Extract response
            const assistantMessage = result.data.choices?.[0]?.message;
            if (!assistantMessage) {
                    Message: "Betty did not return a response",
                    ResultCode: "EMPTY_RESPONSE"
            const bettyResponse = assistantMessage.content;
            // Extract references if provided
            // Betty returns references in multiple formats:
            // - choice[1]: Formatted text (for display)
            // - choice[2]: Raw JSON structure (for programmatic access)
            let bettyReferences: any[] | undefined;
            if (result.data.choices.length > 2) {
                // Use the structured JSON from choice[2]
                const referencesJsonChoice = result.data.choices[2];
                if (referencesJsonChoice.message.content && referencesJsonChoice.finish_reason === 'references_json') {
                        bettyReferences = JSON.parse(referencesJsonChoice.message.content);
                        // If JSON parsing fails, fall back to text parsing from choice[1]
                        if (result.data.choices.length > 1) {
                            bettyReferences = this.parseReferences(result.data.choices[1].message.content);
            } else if (result.data.choices.length > 1) {
                // Backwards compatibility: parse text format from choice[1]
                const referencesChoice = result.data.choices[1];
                if (referencesChoice.message.content) {
                    bettyReferences = this.parseReferences(referencesChoice.message.content);
            // Add output parameters
                Name: 'BettyResponse',
                Value: bettyResponse,
                Type: "Output"
            if (bettyReferences && bettyReferences.length > 0) {
                    Name: 'BettyReferences',
                    Value: bettyReferences,
            // Create formatted response with markdown reference links
            let formattedResponse = bettyResponse;
                formattedResponse += '\n\n**References:**\n';
                for (const ref of bettyReferences) {
                    if (ref.title && ref.link) {
                        formattedResponse += `- [${ref.title}](${ref.link})`;
                        if (ref.type && ref.type.trim().toLowerCase() !== 'unknown') {
                            formattedResponse += ` (${ref.type})`;
                        formattedResponse += '\n';
                Name: 'FormattedResponse',
                Value: formattedResponse,
            // Return success with formatted message
            const resultData: any = {
                response: bettyResponse
                resultData.references = bettyReferences;
                ResultCode: "SUCCESS",
                Message: JSON.stringify(resultData, null, 2)
                Message: `Failed to query Betty: ${errorMessage}`,
     * Parses Betty's reference text into structured reference objects.
     * This is a fallback method for backwards compatibility when structured JSON is not available.
     * Normally, Betty returns structured references as JSON in choice[2], but if that's not
     * available (older API version or error), this method parses the formatted text from choice[1].
     * @param referencesText - Formatted reference text from Betty
     * @returns Array of reference objects with title and link properties
    private parseReferences(referencesText: string): any[] {
        const references: any[] = [];
        // Betty formats references as "Title: URL"
        // Example: "Company Policy: https://example.com/policy"
        const lines = referencesText.split('\n');
            const trimmedLine = line.trim();
            if (!trimmedLine || trimmedLine.toLowerCase().includes('here are some')) {
                continue; // Skip header or empty lines
            const colonIndex = trimmedLine.lastIndexOf(':');
            if (colonIndex > 0) {
                const title = trimmedLine.substring(0, colonIndex).trim();
                const link = trimmedLine.substring(colonIndex + 1).trim();
                if (title && link) {
                    references.push({
                        link: link
        return references;
