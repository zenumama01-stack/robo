 * Typeform API response structures
export interface TypeformResponseItem {
    landing_id: string;
    landed_at: string;
    submitted_at: string;
        user_agent?: string;
        network_id?: string;
    hidden?: Record<string, any>;
    calculated?: {
        field: {
            ref?: string;
        file_url?: string;
        number?: number;
        boolean?: boolean;
        choice?: {
            other?: string;
        choices?: {
            labels: string[];
        date?: string;
        phone_number?: string;
        payment?: {
            amount: string;
            last4: string;
export interface TypeformResponsesResult {
    total_items: number;
    items: TypeformResponseItem[];
 * Base class for all Typeform actions.
 * Handles Typeform-specific authentication and API interaction patterns.
@RegisterClass(BaseAction, 'TypeformBaseAction')
export abstract class TypeformBaseAction extends BaseFormBuilderAction {
        return 'Typeform';
        return 'https://api.typeform.com';
    private currentAPIToken: string | null = null;
    private oauth2Manager: OAuth2Manager | null = null;
     * Creates an OAuth2Manager for Typeform authentication.
     * Configures the OAuth2 endpoints and credentials for Typeform.
        const clientId = process.env['BIZAPPS_TYPEFORM_CLIENT_ID'];
        const clientSecret = process.env['BIZAPPS_TYPEFORM_CLIENT_SECRET'];
        // Check if we have stored tokens for this company
        const oauth = new OAuth2Manager({
            tokenEndpoint: 'https://api.typeform.com/oauth/token',
            authorizationEndpoint: 'https://api.typeform.com/oauth/authorize',
            scopes: ['forms:read', 'forms:write', 'responses:read', 'accounts:read'],
            accessToken: credentials.accessToken,
            refreshToken: credentials.apiKey, // Store refresh token in apiKey field
            onTokenUpdate: async (tokens) => {
                // TODO: Persist updated tokens back to Company Integrations table
                LogStatus(`Typeform OAuth2 tokens updated for company ${companyId}`);
        return oauth;
     * Get axios instance with Typeform authentication
    protected getAxiosInstance(apiToken: string): AxiosInstance {
        if (!this.axiosInstance || this.currentAPIToken !== apiToken) {
            this.currentAPIToken = apiToken;
                    'Authorization': `Bearer ${apiToken}`
                        LogStatus(`Typeform rate limit hit. Waiting ${waitTime}ms before retry...`);
     * Get responses from a Typeform
    protected async getTypeformResponses(
        apiToken: string,
            since?: string;
            until?: string;
            after?: string;
            before?: string;
            completed?: boolean;
            sort?: string;
            query?: string;
            fields?: string[];
    ): Promise<TypeformResponsesResult> {
                page_size: options?.pageSize || 25
            if (options?.since) params.since = options.since;
            if (options?.until) params.until = options.until;
            if (options?.after) params.after = options.after;
            if (options?.before) params.before = options.before;
            if (options?.completed !== undefined) {
                params.completed = options.completed ? 'true' : 'false';
            if (options?.sort) params.sort = options.sort;
            if (options?.query) params.query = options.query;
            if (options?.fields && options.fields.length > 0) {
                params.fields = options.fields.join(',');
            const response = await this.getAxiosInstance(apiToken).get(
            LogError('Failed to get Typeform responses:', error);
            throw this.handleTypeformError(error);
    protected async getAllTypeformResponses(
    ): Promise<TypeformResponseItem[]> {
        const allResponses: TypeformResponseItem[] = [];
        let after: string | undefined = undefined;
                const result = await this.getTypeformResponses(formId, apiToken, {
                    pageSize: Math.min(1000, maxResponses - allResponses.length),
                    since: options?.since,
                    until: options?.until,
                    completed: options?.completed,
                    sort: options?.sort,
                    query: options?.query,
                    after
                allResponses.push(...result.items);
                if (result.items.length === 0) {
                const lastItem = result.items[result.items.length - 1];
                after = lastItem.token;
                if (result.items.length < 1000) {
            LogStatus(`Retrieved ${allResponses.length} responses from Typeform`);
            LogError('Failed to get all Typeform responses:', error);
     * Get a single response by token
    protected async getSingleTypeformResponse(
        responseToken: string,
        apiToken: string
    ): Promise<TypeformResponseItem> {
                pageSize: 1,
                after: responseToken
            if (!result.items || result.items.length === 0) {
                throw new Error(`Response with token ${responseToken} not found`);
            return result.items[0];
            LogError('Failed to get single Typeform response:', error);
     * Get form details from Typeform API
    protected async getFormDetails(formId: string, apiToken: string): Promise<any> {
            const response = await this.getAxiosInstance(apiToken).get(`/forms/${formId}`);
            LogError('Failed to get Typeform form details:', error);
     * Normalize Typeform response to common format
    protected normalizeTypeformResponse(tfResponse: TypeformResponseItem, formFields?: any[]): FormResponse {
        // Create field title lookup map from form fields
        const fieldTitleMap = new Map<string, string>();
        if (formFields) {
            formFields.forEach((field: any) => {
                // Use the field ref as key (matches what's in response.question) and title as value
                if (field.ref && field.title) {
                    fieldTitleMap.set(field.ref, field.title);
        const answerDetails: FormAnswer[] = tfResponse.answers.map(answer => {
            let question = answer.field.ref || answer.field.id;
            switch (answer.type) {
                case 'email':
                    answerValue = answer.email;
                case 'url':
                    answerValue = answer.url;
                    answerValue = answer.file_url;
                    answerValue = answer.number;
                    answerValue = answer.boolean;
                case 'choice':
                    answerValue = answer.choice?.label;
                    if (answer.choice?.other) {
                        answerValue += ` (Other: ${answer.choice.other})`;
                case 'choices':
                    answerValue = answer.choices?.labels || [];
                    if (answer.choices?.other) {
                        answerValue.push(`Other: ${answer.choices.other}`);
                    answerValue = answer.date;
                case 'phone_number':
                    answerValue = answer.phone_number;
                case 'payment':
                    answerValue = answer.payment ? {
                        amount: answer.payment.amount,
                        last4: answer.payment.last4,
                        name: answer.payment.name
                    } : null;
                    answerValue = null;
                fieldId: answer.field.id,
                fieldType: answer.type,
                choices: answer.type === 'choices' ? answer.choices?.labels : undefined
        // Generate answers object with question titles as keys (renamed from simpleAnswers)
        const answers: Record<string, any> = {};
        answerDetails.forEach(answer => {
            const questionTitle = fieldTitleMap.get(answer.question) || answer.question;
            // Clean up the question title to be a valid object key
            const cleanKey = questionTitle.replace(/[^a-zA-Z0-9\s]/g, '').trim();
            answers[cleanKey] = answer.answer;
        const completed = !!tfResponse.submitted_at;
        const submittedAt = tfResponse.submitted_at
            ? new Date(tfResponse.submitted_at)
            : new Date(tfResponse.landed_at);
            responseId: tfResponse.token,
            formId: tfResponse.landing_id,
            answerDetails, // Renamed from answers
            answers, // Renamed from simpleAnswers
                browser: tfResponse.metadata?.browser,
                platform: tfResponse.metadata?.platform,
                referer: tfResponse.metadata?.referer,
                userAgent: tfResponse.metadata?.user_agent
            calculatedFields: tfResponse.calculated,
            hiddenFields: tfResponse.hidden
     * Handle Typeform-specific errors
    protected handleTypeformError(error: any): Error {
                return new Error('Invalid Typeform API token. Please check your authentication.');
                return new Error('Insufficient permissions to access this Typeform resource.');
                return new Error('Typeform form or response not found.');
                return new Error('Typeform API rate limit exceeded. Please try again later.');
                return new Error(`Typeform API error: ${data.message}`);
     * Format date for Typeform API (ISO 8601)
    protected formatTypeformDate(date: Date): string {
     * Parse Typeform date string or Unix timestamp
    protected parseTypeformDate(dateValue: string | number): Date {
        if (typeof dateValue === 'number') {
            return new Date(dateValue * 1000);
