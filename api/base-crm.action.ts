import { ActionParam } from '@memberjunction/actions-base';
 * Base class for all CRM-related actions.
 * Provides common functionality and patterns for interacting with Customer Relationship Management systems.
@RegisterClass(BaseAction, 'BaseCRMAction')
export abstract class BaseCRMAction extends BaseAction {
     * The CRM provider this action is designed for (e.g., 'HubSpot', 'Salesforce', etc.)
    protected abstract crmProvider: string;
     * Common CRM parameters that many actions will need
    protected getCommonCRMParams(): ActionParam[] {
     * Gets the company integration record for the specified company and CRM
     * Example: BIZAPPS_HUBSPOT_12345_API_KEY
        const envKey = `BIZAPPS_${this.crmProvider.toUpperCase().replace(/\s+/g, '_')}_${companyId}_${credentialType.toUpperCase()}`;
     * Gets API credentials - first tries environment variables, then falls back to database
    protected async getAPICredentials(integration: MJCompanyIntegrationEntity): Promise<{ 
        apiSecret?: string; 
        accessToken?: string;
        const envApiKey = this.getCredentialFromEnv(companyId, 'API_KEY');
        const envApiSecret = this.getCredentialFromEnv(companyId, 'API_SECRET');
        const envClientId = this.getCredentialFromEnv(companyId, 'CLIENT_ID');
        const envClientSecret = this.getCredentialFromEnv(companyId, 'CLIENT_SECRET');
        if (envApiKey || envAccessToken) {
                apiKey: envApiKey,
                apiSecret: envApiSecret,
                refreshToken: envRefreshToken,
                clientId: envClientId,
                clientSecret: envClientSecret
        if (!integration.APIKey && !integration.AccessToken) {
            throw new Error(`No API credentials found for ${this.integrationName} integration. Please set environment variables or configure in database.`);
            apiKey: integration.APIKey || undefined,
            accessToken: integration.AccessToken || undefined,
     * Gets the base URL for API calls
    protected async getAPIBaseURL(integration: MJCompanyIntegrationEntity): Promise<string> {
        // Check if custom URL is stored in the integration
        if (integration.CustomAttribute1) {
            return integration.CustomAttribute1;
        // Return empty string - derived classes should override this
     * Helper to get parameter value with type safety
    protected getParamValue(params: ActionParam[], paramName: string): any {
        const param = params.find(p => p.Name === paramName);
     * Standard date format for CRM systems (ISO 8601)
    protected formatCRMDate(date: Date): string {
        return date.toISOString();
     * Parse date from CRM format
    protected parseCRMDate(dateString: string): Date {
     * Format phone numbers to E.164 format if possible
    protected formatPhoneNumber(phone: string): string {
        if (!phone) return '';
        // Remove all non-numeric characters
        const cleaned = phone.replace(/\D/g, '');
        // If it's a US number (10 digits), format it
        if (cleaned.length === 10) {
            return `+1${cleaned}`;
        // If it already has country code
        if (cleaned.length === 11 && cleaned.startsWith('1')) {
            return `+${cleaned}`;
        // Return cleaned version for other formats
        return cleaned;
     * Validate email format
    protected isValidEmail(email: string): boolean {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
     * Helper to build consistent error messages for CRM operations
    protected buildCRMErrorMessage(operation: string, details: string, systemError?: any): string {
        let message = `CRM operation failed: ${operation}. ${details}`;
     * Map deal/opportunity stages to common statuses
    protected mapDealStatus(stage: string): 'open' | 'won' | 'lost' | 'unknown' {
        const lowerStage = stage.toLowerCase();
        if (lowerStage.includes('won') || lowerStage.includes('closed-won') || lowerStage.includes('success')) {
            return 'won';
        if (lowerStage.includes('lost') || lowerStage.includes('closed-lost') || lowerStage.includes('failed')) {
            return 'lost';
        if (lowerStage.includes('open') || lowerStage.includes('active') || lowerStage.includes('qualified')) {
            return 'open';
        return 'unknown';
     * Common activity types mapping
    protected mapActivityType(type: string): 'call' | 'email' | 'meeting' | 'task' | 'note' | 'other' {
        const activityMap: Record<string, 'call' | 'email' | 'meeting' | 'task' | 'note' | 'other'> = {
            'call': 'call',
            'phone': 'call',
            'email': 'email',
            'meeting': 'meeting',
            'appointment': 'meeting',
            'task': 'task',
            'todo': 'task',
            'note': 'note',
            'comment': 'note'
        return activityMap[type.toLowerCase()] || 'other';
