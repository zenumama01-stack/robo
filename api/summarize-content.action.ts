import { ActionResultSimple, RunActionParams, ActionParam, ActionEngineBase } from "@memberjunction/actions-base";
import { BaseAction, ActionEngineServer } from "@memberjunction/actions";
import { LogError, RunView } from "@memberjunction/core";
import type { AIPromptEntityExtended } from '@memberjunction/ai-core-plus';
interface SummaryCitation {
    text: string;           // Exact quote from content
    url: string;            // URL with anchor if available
    anchorId?: string;      // HTML element ID
    context?: string;       // Surrounding text
    relevance: string;      // Why this quote matters
interface SummaryOutput {
    summary: string;
    wordCount: number;
    citations?: SummaryCitation[];
        pageTitle?: string;
        sourceUrl: string;
 * Action that creates a concise summary of content with citations to source material.
 * Can work with either a URL (fetch + summarize) or pre-fetched content.
 * - Flexible input: URL or pre-fetched content
 * - Configurable summary length (word count target)
 * - Optional citations with quotes and links
 * - Focus instructions for domain-specific summarization
 * - Multiple output formats (paragraph, bullets, hybrid)
 * - Automatic anchor link detection for precise citations
 * // Summarize from URL
 *   ActionName: 'Summarize Content',
 *     Name: 'url',
 *     Value: 'https://docs.memberjunction.org/architecture'
 *     Name: 'summaryWords',
 *     Value: 200
 *     Name: 'instructions',
 *     Value: 'Focus on metadata layer and entity system'
 * // Summarize pre-fetched content
 *     Name: 'content',
 *     Value: '<html>...</html>'
 *     Name: 'sourceUrl',
 *     Name: 'includeCitations',
@RegisterClass(BaseAction, "SummarizeContentAction")
export class SummarizeContentAction extends BaseAction {
            const url = this.getStringParam(params, "url");
            const content = this.getStringParam(params, "content");
            const sourceUrl = this.getStringParam(params, "sourceurl");
            const summaryWords = this.getNumericParam(params, "summarywords", 200);
            const includeCitations = this.getBooleanParam(params, "includecitations", true);
            const maxCitations = this.getNumericParam(params, "maxcitations", 5);
            const instructions = this.getStringParam(params, "instructions", "Provide a comprehensive summary");
            const format = this.getStringParam(params, "format", "paragraph");
            // Validation: Must have either URL or content
            if (!url && !content) {
                return this.createErrorResult(
                    "Must provide either 'url' or 'content' parameter",
                    "MISSING_INPUT"
            // Validation: If content provided, must have sourceUrl
            if (content && !sourceUrl) {
                    "When using 'content' parameter, 'sourceUrl' is required for citations",
                    "MISSING_SOURCE_URL"
            let pageContent: string;
            let finalSourceUrl: string;
            // Fetch content if URL provided
            if (url) {
                const fetchResult = await this.fetchWebPageContent(url, params);
                if (!fetchResult.success) {
                        `Failed to fetch web page: ${fetchResult.error}`,
                        "FETCH_FAILED"
                pageContent = fetchResult.content!;
                finalSourceUrl = url;
                // Use provided content
                pageContent = content!;
                finalSourceUrl = sourceUrl!;
            // Call the LLM prompt action to do the actual summarization
            const summaryResult = await this.generateSummary(pageContent, finalSourceUrl, {
                summaryWords,
                includeCitations,
                maxCitations,
                instructions,
                format
            }, params);
            if (!summaryResult.success) {
                    `Failed to generate summary: ${summaryResult.error}`,
                    "SUMMARIZATION_FAILED"
            const outputData = summaryResult.data as SummaryOutput;
            this.addOutputParam(params, "summary", outputData.summary);
            this.addOutputParam(params, "wordCount", outputData.wordCount);
            if (outputData.citations) {
                this.addOutputParam(params, "citations", outputData.citations);
            if (outputData.keyPoints) {
                this.addOutputParam(params, "keyPoints", outputData.keyPoints);
            if (outputData.metadata) {
                this.addOutputParam(params, "metadata", outputData.metadata);
                Message: JSON.stringify(outputData, null, 2)
            LogError(`Error in Summarize Content action: ${error}`);
                `Error summarizing content: ${error instanceof Error ? error.message : String(error)}`,
                "UNEXPECTED_ERROR"
     * Fetch web page content using the Get Web Page Content action
    private async fetchWebPageContent(
        url: string,
        params: RunActionParams
    ): Promise<{ success: boolean; content?: string; error?: string }> {
            // Load the Web Page Content action
            await ActionEngineBase.Instance.Config(false, params.ContextUser);
            const action = ActionEngineBase.Instance.Actions.find(a => a.Name === 'Web Page Content');
                    error: "Action 'Web Page Content' not found"
            const actionParams: ActionParam[] = [{
                Name: "url",
                Type: "Input",
                Value: url
            const runParams = new RunActionParams();
            runParams.Action = action;
            runParams.Params = actionParams;
            runParams.ContextUser = params.ContextUser;
            runParams.SkipActionLog = true;
            const engine = new ActionEngineServer();
            const result = await engine.RunAction(runParams);
                    error: result.Message || "Failed to fetch web page"
            // Extract content from result
            const content = result.Message || "";
     * Generate summary using AI Prompts package directly
    private async generateSummary(
        sourceUrl: string,
            summaryWords: number;
            includeCitations: boolean;
            maxCitations: number;
            instructions: string;
    ): Promise<{ success: boolean; data?: SummaryOutput; error?: string }> {
            // Ensure AIEngine is initialized
            // Get the summarization prompt from AIEngine
            const prompt = this.getPromptByNameAndCategory('Summarize Content', 'MJ: System');
                    error: "Prompt 'Summarize Content' not found. Ensure metadata has been synced."
            // Build prompt parameters with data context
                sourceUrl,
                summaryWords: options.summaryWords,
                includeCitations: options.includeCitations,
                maxCitations: options.maxCitations,
                instructions: options.instructions,
                format: options.format
            promptParams.cleanValidationSyntax = true;
            const result = await runner.ExecutePrompt<SummaryOutput>(promptParams);
                    error: result.errorMessage || "Prompt execution failed"
                data: result.result
     * Get prompt by name and category from AIEngine
    private getPromptByNameAndCategory(name: string, category: string): AIPromptEntityExtended | undefined {
        return AIEngine.Instance.Prompts.find(p => p.Name.trim().toLowerCase() === name.trim().toLowerCase() && 
                                                   p.Category?.trim().toLowerCase() === category?.trim().toLowerCase());
    // Helper methods (consistent with other actions)
    private getStringParam(params: RunActionParams, paramName: string, defaultValue?: string): string | undefined {
        if (!param || param.Value === undefined || param.Value === null) {
        const value = String(param.Value).trim();
        return value.length > 0 ? value : defaultValue;
    private getNumericParam(params: RunActionParams, paramName: string, defaultValue: number): number {
        const parsed = Number(param.Value);
        return isNaN(parsed) ? defaultValue : parsed;
    private getBooleanParam(params: RunActionParams, paramName: string, defaultValue: boolean): boolean {
        const value = String(param.Value).trim().toLowerCase();
        if (value === "true" || value === "1" || value === "yes") {
        if (value === "false" || value === "0" || value === "no") {
    private addOutputParam(params: RunActionParams, name: string, value: unknown): void {
            Type: "Output",
            Value: value
    private createErrorResult(message: string, code: string): ActionResultSimple {
            ResultCode: code
