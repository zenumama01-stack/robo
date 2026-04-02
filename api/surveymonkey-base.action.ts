 * SurveyMonkey API response structures
export interface SurveyMonkeyResponse {
    href: string;
    recipient_id?: string;
    collection_mode: string;
    response_status: string;
    custom_value?: string;
    email_address?: string;
    ip_address?: string;
    logic_path?: Record<string, any>;
    page_path?: Array<{ id: string; }>;
    collector_id: string;
    survey_id: string;
    custom_variables?: Record<string, string>;
    edit_url?: string;
    analyze_url?: string;
    total_time: number;
    date_created: string;
    date_modified: string;
    pages?: Array<{
        questions: Array<{
            variable_id?: string;
                choice_id?: string;
                row_id?: string;
                col_id?: string;
                other_id?: string;
                tag_data?: Array<{
                    tag?: string;
export interface SurveyMonkeyResponsesResult {
    data: SurveyMonkeyResponse[];
    links: {
        self: string;
        next?: string;
export interface SurveyMonkeySurveyDetails {
    nickname?: string;
    language: string;
    question_count: number;
    page_count: number;
    response_count: number;
    buttons_text?: {
        next_button?: string;
        prev_button?: string;
        done_button?: string;
        exit_button?: string;
    collect_url?: string;
    summary_url?: string;
export interface SurveyMonkeyCollector {
 * Base class for all SurveyMonkey actions.
 * Handles SurveyMonkey-specific authentication and API interaction patterns.
@RegisterClass(BaseAction, 'SurveyMonkeyBaseAction')
export abstract class SurveyMonkeyBaseAction extends BaseFormBuilderAction {
        return 'SurveyMonkey';
        return 'https://api.surveymonkey.com/v3';
     * Get axios instance with SurveyMonkey authentication
                        LogStatus(`SurveyMonkey rate limit hit. Waiting ${waitTime}ms before retry...`);
     * Get responses from a SurveyMonkey survey
    protected async getSurveyMonkeyResponses(
        surveyId: string,
            per_page?: number;
            page?: number;
            start_created_at?: string;
            end_created_at?: string;
            start_modified_at?: string;
            end_modified_at?: string;
            sort_order?: 'ASC' | 'DESC';
            sort_by?: 'date_modified' | 'date_created';
            status?: 'completed' | 'partial' | 'overquota' | 'disqualified';
    ): Promise<SurveyMonkeyResponsesResult> {
                per_page: options?.per_page || 100
            if (options?.page !== undefined) params.page = options.page;
            if (options?.start_created_at) params.start_created_at = options.start_created_at;
            if (options?.end_created_at) params.end_created_at = options.end_created_at;
            if (options?.start_modified_at) params.start_modified_at = options.start_modified_at;
            if (options?.end_modified_at) params.end_modified_at = options.end_modified_at;
            if (options?.sort_order) params.sort_order = options.sort_order;
            if (options?.sort_by) params.sort_by = options.sort_by;
            if (options?.status) params.status = options.status;
                `/surveys/${surveyId}/responses/bulk`,
            LogError('Failed to get SurveyMonkey responses:', error);
            throw this.handleSurveyMonkeyError(error);
     * Get all responses with automatic pagination
    protected async getAllSurveyMonkeyResponses(
    ): Promise<SurveyMonkeyResponse[]> {
        const allResponses: SurveyMonkeyResponse[] = [];
        let currentPage = 1;
                const result = await this.getSurveyMonkeyResponses(surveyId, accessToken, {
                    per_page: Math.min(100, maxResponses - allResponses.length),
                    page: currentPage,
                    start_created_at: options?.start_created_at,
                    end_created_at: options?.end_created_at,
                    start_modified_at: options?.start_modified_at,
                    end_modified_at: options?.end_modified_at,
                    sort_order: options?.sort_order,
                    sort_by: options?.sort_by,
                    status: options?.status
                allResponses.push(...result.data);
                if (result.data.length === 0 || !result.links.next) {
                if (result.data.length < result.per_page) {
                currentPage++;
            LogStatus(`Retrieved ${allResponses.length} responses from SurveyMonkey`);
            LogError('Failed to get all SurveyMonkey responses:', error);
     * Get a single response by ID with detailed information
    protected async getSingleSurveyMonkeyResponse(
    ): Promise<SurveyMonkeyResponse> {
                `/surveys/${surveyId}/responses/${responseId}/details`
            LogError('Failed to get single SurveyMonkey response:', error);
     * Get survey details
    protected async getSurveyMonkeyDetails(
    ): Promise<SurveyMonkeySurveyDetails> {
                `/surveys/${surveyId}`
            LogError('Failed to get SurveyMonkey survey details:', error);
     * Get survey pages and questions for detailed response normalization
    protected async getSurveyMonkeyPages(
                `/surveys/${surveyId}/pages`
            LogError('Failed to get SurveyMonkey pages:', error);
     * Get collectors for a survey
    protected async getSurveyMonkeyCollectors(
    ): Promise<SurveyMonkeyCollector[]> {
                `/surveys/${surveyId}/collectors`
            return response.data.data;
            LogError('Failed to get SurveyMonkey collectors:', error);
     * Create a new collector for a survey
    protected async createSurveyMonkeyCollector(
        collectorData: {
            type: 'weblink' | 'email';
            thank_you_message?: string;
            redirect_url?: string;
    ): Promise<SurveyMonkeyCollector> {
            const response = await this.getAxiosInstance(accessToken).post(
                `/surveys/${surveyId}/collectors`,
                collectorData
            LogError('Failed to create SurveyMonkey collector:', error);
     * Update survey details
    protected async updateSurveyMonkey(
        updateData: {
            language?: string;
            const response = await this.getAxiosInstance(accessToken).patch(
                `/surveys/${surveyId}`,
                updateData
            LogError('Failed to update SurveyMonkey survey:', error);
     * Normalize SurveyMonkey response to common format
    protected normalizeSurveyMonkeyResponse(smResponse: SurveyMonkeyResponse): FormResponse {
        if (smResponse.pages) {
            for (const page of smResponse.pages) {
                for (const question of page.questions) {
                    for (const answer of question.answers) {
                        let fieldType = 'unknown';
                        if (answer.text !== undefined) {
                            answerValue = answer.text;
                            fieldType = 'text';
                        } else if (answer.choice_id) {
                            answerValue = answer.choice_id;
                            fieldType = 'choice';
                        } else if (answer.row_id || answer.col_id) {
                            answerValue = {
                                row: answer.row_id,
                                col: answer.col_id
                            fieldType = 'matrix';
                        } else if (answer.tag_data) {
                            answerValue = answer.tag_data.map(t => t.text);
                            fieldType = 'tag';
                            fieldId: question.id,
                            question: question.variable_id || question.id,
                            choices: answer.tag_data ? answer.tag_data.map(t => t.text) : undefined
        const completed = smResponse.response_status === 'completed';
        const submittedAt = new Date(smResponse.date_modified);
            responseId: smResponse.id,
            formId: smResponse.survey_id,
            answerDetails: answers, // For now, use answers as answerDetails since SurveyMonkey doesn't have simpleAnswers
                userAgent: smResponse.ip_address,
                platform: 'SurveyMonkey'
            calculatedFields: {
                total_time: smResponse.total_time,
                collection_mode: smResponse.collection_mode,
                response_status: smResponse.response_status
            hiddenFields: smResponse.custom_variables
     * Handle SurveyMonkey-specific errors
    protected handleSurveyMonkeyError(error: any): Error {
                return new Error('Invalid SurveyMonkey access token. Please check your authentication.');
                return new Error('Insufficient permissions to access this SurveyMonkey resource.');
                return new Error('SurveyMonkey survey or response not found.');
                return new Error('SurveyMonkey API rate limit exceeded. Please try again later.');
                return new Error(`SurveyMonkey API error: ${data.error.message}`);
                return new Error(`SurveyMonkey API error: ${data.message}`);
     * Format date for SurveyMonkey API (ISO 8601 format)
    protected formatSurveyMonkeyDate(date: Date): string {
     * Parse SurveyMonkey date string
    protected parseSurveyMonkeyDate(dateValue: string): Date {
