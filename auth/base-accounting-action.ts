import { BaseAction } from '@memberjunction/actions';
import { ActionParam, ActionResultSimple, RunActionParams } from '@memberjunction/actions-base';
import { MJCompanyIntegrationEntity, MJIntegrationEntity } from '@memberjunction/core-entities';
 * Base class for all accounting-related actions.
 * Provides common functionality and patterns for interacting with accounting systems.
@RegisterClass(BaseAction, 'BaseAccountingAction')
export abstract class BaseAccountingAction extends BaseAction {
     * The accounting provider this action is designed for (e.g., 'QuickBooks', 'NetSuite', etc.)
     * Can be 'Generic' for provider-agnostic actions
    protected abstract accountingProvider: string;
     * The integration name to look up in the Integration entity
    protected abstract integrationName: string;
     * Cached company integration for the current execution
    private _companyIntegration: MJCompanyIntegrationEntity | null = null;
     * Override of the required abstract method from BaseAction
    protected abstract InternalRunAction(params: RunActionParams): Promise<ActionResultSimple>;
     * Helper to get a parameter value from the params array
    protected getParamValue(params: ActionParam[], name: string): any {
        const param = params.find(p => p.Name === name);
     * Common accounting parameters that many actions will need
    protected getCommonAccountingParams(): ActionParam[] {
                Name: 'CompanyID',
                Type: 'Input',
                Value: null
                Name: 'FiscalYear',
                Name: 'AccountingPeriod',
     * Gets the company integration record for the specified company and accounting system
    protected async getCompanyIntegration(companyId: string, contextUser: UserInfo): Promise<MJCompanyIntegrationEntity> {
        if (this._companyIntegration && this._companyIntegration.CompanyID === companyId) {
            return this._companyIntegration;
        const result = await rv.RunView<MJCompanyIntegrationEntity>({
            EntityName: 'MJ: Company Integrations',
            ExtraFilter: `CompanyID = '${companyId}' AND Integration.Name = '${this.integrationName}'`,
            throw new Error(`Failed to retrieve company integration: ${result.ErrorMessage}`);
            throw new Error(`No ${this.integrationName} integration found for company ${companyId}. Please configure the integration first.`);
        this._companyIntegration = result.Results[0];
     * Gets credentials from environment variables
     * Format: BIZAPPS_{PROVIDER}_{COMPANY_ID}_{CREDENTIAL_TYPE}
     * Example: BIZAPPS_QUICKBOOKS_12345_ACCESS_TOKEN
    protected getCredentialFromEnv(companyId: string, credentialType: string): string | undefined {
        const envKey = `BIZAPPS_${this.accountingProvider.toUpperCase().replace(/\s+/g, '_')}_${companyId}_${credentialType.toUpperCase()}`;
        return process.env[envKey];
     * Gets OAuth tokens - first tries environment variables, then falls back to database
    protected async getOAuthTokens(integration: MJCompanyIntegrationEntity): Promise<{ accessToken: string; refreshToken?: string }> {
        const companyId = integration.CompanyID;
        // Try environment variables first
        const envAccessToken = this.getCredentialFromEnv(companyId, 'ACCESS_TOKEN');
        const envRefreshToken = this.getCredentialFromEnv(companyId, 'REFRESH_TOKEN');
        if (envAccessToken) {
                accessToken: envAccessToken,
                refreshToken: envRefreshToken
        // Fall back to database (for backwards compatibility)
        if (!integration.AccessToken) {
            throw new Error(`No access token found for ${this.integrationName} integration. Please set environment variable BIZAPPS_${this.accountingProvider.toUpperCase().replace(/\s+/g, '_')}_${companyId}_ACCESS_TOKEN or configure in database.`);
        // Check if token is expired
        if (integration.TokenExpirationDate && new Date(integration.TokenExpirationDate) < new Date()) {
            throw new Error(`Access token for ${this.integrationName} has expired. Please re-authenticate.`);
            accessToken: integration.AccessToken!,
            refreshToken: integration.RefreshToken || undefined
     * Gets the base URL for API calls from the integration
    protected async getAPIBaseURL(contextUser: UserInfo): Promise<string> {
        const integration = await md.GetEntityObject<MJIntegrationEntity>('MJ: Integrations', contextUser);
        const result = await rv.RunView<MJIntegrationEntity>({
            EntityName: 'MJ: Integrations',
            ExtraFilter: `Name = '${this.integrationName}'`,
            throw new Error(`Integration configuration not found for ${this.integrationName}`);
        return result.Results[0].NavigationBaseURL || '';
     * Validates common accounting data formats
    protected validateAccountNumber(accountNumber: string): boolean {
        // Basic validation - can be overridden by specific providers
        return /^[0-9\-\.]+$/.test(accountNumber);
     * Validates journal entry balance (debits must equal credits)
    protected validateJournalEntryBalance(lines: Array<{debit?: number, credit?: number}>): boolean {
        const totalDebits = lines.reduce((sum, line) => sum + (line.debit || 0), 0);
        const totalCredits = lines.reduce((sum, line) => sum + (line.credit || 0), 0);
        return Math.abs(totalDebits - totalCredits) < 0.01; // Allow for minor rounding differences
     * Formats currency values consistently
    protected formatCurrency(amount: number, currencyCode: string = 'USD'): string {
            currency: currencyCode,
        }).format(amount);
     * Standard date format for accounting systems (ISO 8601)
    protected formatAccountingDate(date: Date): string {
        return date.toISOString().split('T')[0];
     * Helper to build consistent error messages for accounting operations
    protected buildAccountingErrorMessage(operation: string, details: string, systemError?: any): string {
        let message = `Accounting operation failed: ${operation}. ${details}`;
        if (systemError) {
            message += ` System error: ${systemError.message || systemError}`;
