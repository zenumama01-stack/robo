 * Base class for all LMS-related actions.
 * Provides common functionality and patterns for interacting with Learning Management Systems.
@RegisterClass(BaseAction, 'BaseLMSAction')
export abstract class BaseLMSAction extends BaseAction {
     * The LMS provider this action is designed for (e.g., 'LearnWorlds', 'Moodle', etc.)
    protected abstract lmsProvider: string;
     * Common LMS parameters that many actions will need
    protected getCommonLMSParams(): ActionParam[] {
     * Gets the company integration record for the specified company and LMS
     * Example: BIZAPPS_LEARNWORLDS_12345_API_KEY
        const envKey = `BIZAPPS_${this.lmsProvider.toUpperCase().replace(/\s+/g, '_')}_${companyId}_${credentialType.toUpperCase()}`;
    protected async getAPICredentials(integration: MJCompanyIntegrationEntity): Promise<{ apiKey?: string; apiSecret?: string; accessToken?: string }> {
     * Standard date format for LMS systems (ISO 8601)
    protected formatLMSDate(date: Date): string {
     * Parse date from LMS format
    protected parseLMSDate(dateString: string): Date {
     * Calculate progress percentage
    protected calculateProgressPercentage(completed: number, total: number): number {
        return Math.round((completed / total) * 100);
        const secs = seconds % 60;
        } else if (minutes > 0) {
            return `${secs}s`;
     * Helper to build consistent error messages for LMS operations
    protected buildLMSErrorMessage(operation: string, details: string, systemError?: any): string {
        let message = `LMS operation failed: ${operation}. ${details}`;
     * Common enrollment status mapping
    protected mapEnrollmentStatus(status: string): 'active' | 'completed' | 'expired' | 'suspended' | 'unknown' {
        const statusMap: Record<string, 'active' | 'completed' | 'expired' | 'suspended' | 'unknown'> = {
            'active': 'active',
            'completed': 'completed',
            'finished': 'completed',
            'expired': 'expired',
            'suspended': 'suspended',
            'paused': 'suspended',
            'inactive': 'suspended'
        return statusMap[status.toLowerCase()] || 'unknown';
