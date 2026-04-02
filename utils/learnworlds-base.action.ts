import { BaseLMSAction } from '../../base/base-lms.action';
 * Base class for all LearnWorlds LMS actions.
 * Handles LearnWorlds-specific authentication and API interaction patterns.
@RegisterClass(BaseAction, 'LearnWorldsBaseAction')
export abstract class LearnWorldsBaseAction extends BaseLMSAction {
    protected lmsProvider = 'LearnWorlds';
    protected integrationName = 'LearnWorlds';
     * LearnWorlds API version
    protected apiVersion = 'v2';
     * Makes an authenticated request to LearnWorlds API
    protected async makeLearnWorldsRequest<T = any>(
            throw new Error('Context user is required for LearnWorlds API calls');
        // Get API credentials (from env vars or database)
            throw new Error('API Key is required for LearnWorlds integration');
        // Get the school domain from ExternalSystemID or environment
        const schoolDomain = integration.ExternalSystemID || this.getCredentialFromEnv(companyId, 'SCHOOL_DOMAIN');
        if (!schoolDomain) {
            throw new Error('School domain not found. Set in CompanyIntegration.ExternalSystemID or environment variable');
        const baseUrl = `https://${schoolDomain}/api/${this.apiVersion}`;
            'Authorization': `Bearer ${credentials.apiKey}`,
            'Lw-Client': 'MemberJunction'
                let errorMessage = `LearnWorlds API error: ${response.status} ${response.statusText}`;
                        errorMessage = `LearnWorlds API error: ${errorJson.error.message || errorJson.error}`;
                    } else if (errorJson.message) {
                        errorMessage = `LearnWorlds API error: ${errorJson.message}`;
            throw new Error(`LearnWorlds API request failed: ${error}`);
     * Makes a paginated request to LearnWorlds API
    protected async makeLearnWorldsPaginatedRequest<T = any>(
        let page = 1;
        const limit = params.limit || 50;
                page: page.toString(),
            const response = await this.makeLearnWorldsRequest<{
                data: T[];
                meta?: {
                    totalPages: number;
                    totalItems: number;
            if (response.data && Array.isArray(response.data)) {
                results.push(...response.data);
            if (response.meta && response.meta.page < response.meta.totalPages) {
                page++;
            // Respect max results if specified
     * Convert LearnWorlds date format to Date object
    protected parseLearnWorldsDate(dateString: string | number): Date {
        // LearnWorlds sometimes returns timestamps as seconds since epoch
     * Format date for LearnWorlds API (ISO 8601)
    protected formatLearnWorldsDate(date: Date): string {
     * Map LearnWorlds user status to standard status
    protected mapUserStatus(status: string): 'active' | 'inactive' | 'suspended' {
        const statusMap: Record<string, 'active' | 'inactive' | 'suspended'> = {
            'inactive': 'inactive',
            'blocked': 'suspended'
        return statusMap[status.toLowerCase()] || 'inactive';
     * Map LearnWorlds enrollment status
    protected mapLearnWorldsEnrollmentStatus(enrollment: any): 'active' | 'completed' | 'expired' | 'suspended' {
        if (enrollment.completed) {
            return 'completed';
        if (enrollment.expired) {
            return 'expired';
        if (enrollment.suspended || !enrollment.active) {
            return 'suspended';
        return 'active';
     * Calculate progress from LearnWorlds data
    protected calculateProgress(progressData: any): {
        completedUnits: number;
        totalUnits: number;
        timeSpent: number;
            percentage: progressData.percentage || 0,
            completedUnits: progressData.completed_units || 0,
            totalUnits: progressData.total_units || 0,
            timeSpent: progressData.time_spent || 0
