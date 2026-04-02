 * JotForm API response structures
export interface JotFormSubmission {
    form_id: string;
    ip: string;
    created_at: string;
    new: string;
    flag: string;
    answers: Record<string, {
        order: string;
        answer?: string | string[];
        prettyFormat?: string;
export interface JotFormSubmissionsResult {
    responseCode: number;
    content: JotFormSubmission[];
    offset: number;
 * Base class for all JotForm actions.
 * Handles JotForm-specific authentication and API interaction patterns.
@RegisterClass(BaseAction, 'JotFormBaseAction')
export abstract class JotFormBaseAction extends BaseFormBuilderAction {
        return 'JotForm';
        return 'https://api.jotform.com';
    private currentAPIKey: string | null = null;
     * Get axios instance with JotForm authentication
    protected getAxiosInstance(apiKey: string, region?: 'us' | 'eu' | 'hipaa'): AxiosInstance {
        const baseURL = this.getRegionalBaseUrl(region);
        if (!this.axiosInstance || this.currentAPIKey !== apiKey) {
            this.currentAPIKey = apiKey;
                baseURL,
                params: {
                    apiKey  // JotForm uses API key in query params
                        LogStatus('JotForm rate limit hit. Waiting 60 seconds...');
                        await this.sleep(60000);
     * Get regional base URL
    protected getRegionalBaseUrl(region?: 'us' | 'eu' | 'hipaa'): string {
        switch (region) {
            case 'eu':
                return 'https://eu-api.jotform.com';
            case 'hipaa':
                return 'https://hipaa-api.jotform.com';
     * Get submissions from a JotForm
    protected async getJotFormSubmissions(
            offset?: number;
            filter?: Record<string, string>;
            orderby?: string;
            region?: 'us' | 'eu' | 'hipaa';
    ): Promise<JotFormSubmissionsResult> {
                limit: options?.limit || 100,
                offset: options?.offset || 0
                params.filter = JSON.stringify(options.filter);
            if (options?.orderby) {
                params.orderby = options.orderby;
            const response = await this.getAxiosInstance(apiKey, options?.region).get(
                `/form/${formId}/submissions`,
            LogError('Failed to get JotForm submissions:', error);
            throw this.handleJotFormError(error);
     * Get all submissions with automatic pagination
    protected async getAllJotFormSubmissions(
            maxSubmissions?: number;
    ): Promise<JotFormSubmission[]> {
        const allSubmissions: JotFormSubmission[] = [];
        let offset = 0;
        const limit = 1000;
        const maxSubmissions = options?.maxSubmissions || 10000;
                const result = await this.getJotFormSubmissions(formId, apiKey, {
                    limit: Math.min(limit, maxSubmissions - allSubmissions.length),
                    offset,
                    filter: options?.filter,
                    orderby: options?.orderby,
                    region: options?.region
                if (result.content && result.content.length > 0) {
                    allSubmissions.push(...result.content);
                    if (allSubmissions.length >= maxSubmissions) {
                        LogStatus(`Reached max submissions limit of ${maxSubmissions}`);
                    if (result.content.length < limit) {
                    offset += limit;
            LogStatus(`Retrieved ${allSubmissions.length} submissions from JotForm`);
            return allSubmissions;
            LogError('Failed to get all JotForm submissions:', error);
     * Get a single submission by ID
    protected async getSingleJotFormSubmission(
        submissionId: string,
        region?: 'us' | 'eu' | 'hipaa'
    ): Promise<JotFormSubmission> {
            const response = await this.getAxiosInstance(apiKey, region).get(
                `/submission/${submissionId}`,
                { params: { apiKey } }
            return response.data.content;
            LogError('Failed to get single JotForm submission:', error);
     * Get form details
    protected async getJotFormDetails(
                `/form/${formId}`,
            LogError('Failed to get JotForm details:', error);
     * Get form questions/fields
    protected async getJotFormQuestions(
                `/form/${formId}/questions`,
            LogError('Failed to get JotForm questions:', error);
     * Create a new submission
    protected async createJotFormSubmission(
        submissionData: Record<string, any>,
            const params: Record<string, any> = { apiKey };
            // Add submission data to params
            Object.entries(submissionData).forEach(([fieldId, value]) => {
                params[`submission[${fieldId}]`] = value;
            const response = await this.getAxiosInstance(apiKey, region).post(
            LogError('Failed to create JotForm submission:', error);
     * Normalize JotForm submission to common format
    protected normalizeJotFormSubmission(jfSubmission: JotFormSubmission): FormResponse {
        const answers: FormAnswer[] = Object.entries(jfSubmission.answers || {}).map(([fieldId, answerData]) => {
            let answerValue: any = answerData.answer;
            // Handle array answers
            if (Array.isArray(answerValue)) {
                answerValue = answerValue.filter(v => v !== '');
                fieldId,
                fieldType: answerData.type,
                question: answerData.text || answerData.name,
                answer: answerValue || answerData.prettyFormat,
        const submittedAt = new Date(jfSubmission.created_at);
        const completed = jfSubmission.status === 'ACTIVE';
            responseId: jfSubmission.id,
            formId: jfSubmission.form_id,
            answerDetails: answers, // For now, use answers as answerDetails since JotForm doesn't have simpleAnswers
                userAgent: jfSubmission.ip,
                platform: 'JotForm'
     * Handle JotForm-specific errors
    protected handleJotFormError(error: any): Error {
                return new Error('Invalid JotForm API key. Please check your authentication.');
                return new Error('Insufficient permissions to access this JotForm resource.');
                return new Error('JotForm form or submission not found.');
                return new Error('JotForm API rate limit exceeded. Please try again later.');
            } else if (data?.message) {
                return new Error(`JotForm API error: ${data.message}`);
     * Sleep helper for rate limiting
