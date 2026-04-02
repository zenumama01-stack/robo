import { ActionResultSimple, RunActionParams } from '@memberjunction/actions-base';
 * Base class for all QuickBooks Online actions.
 * Handles QB-specific authentication and API interaction patterns.
@RegisterClass(BaseAction, 'QuickBooksBaseAction')
export abstract class QuickBooksBaseAction extends BaseAccountingAction {
    protected accountingProvider = 'QuickBooks Online';
    protected integrationName = 'QuickBooks Online';
     * QuickBooks API version
    protected apiVersion = 'v3';
     * QuickBooks minor version for API compatibility
    protected minorVersion = '65'; // Latest as of 2024
     * Makes an authenticated request to QuickBooks Online API
    protected async makeQBORequest<T = any>(
        method: 'GET' | 'POST' | 'PUT' | 'DELETE' = 'GET',
            throw new Error('Context user is required for QuickBooks API calls');
        const companyId = this.getParamValue(this._params || [], 'CompanyID');
        // Get QuickBooks company ID (realm ID) from ExternalSystemID
        const realmId = integration.ExternalSystemID || this.getCredentialFromEnv(companyId, 'REALM_ID');
        if (!realmId) {
            throw new Error('QuickBooks Realm ID not found. Set in CompanyIntegration.ExternalSystemID or environment variable');
        // Build the full URL using the environment from integration
        const baseUrl = await this.getQuickBooksAPIUrl(integration);
        const fullUrl = `${baseUrl}/${this.apiVersion}/company/${realmId}/${endpoint}`;
        // Add minor version header for API compatibility
        if (this.minorVersion) {
            headers['Intuit-Company-ID'] = realmId;
            headers['Accept'] = `application/json;minorversion=${this.minorVersion}`;
                let errorMessage = `QuickBooks API error: ${response.status} ${response.statusText}`;
                    if (errorJson.Fault && errorJson.Fault.Error) {
                        const qbError = errorJson.Fault.Error[0];
                        errorMessage = `QuickBooks API error: ${qbError.Message} (Code: ${qbError.code})`;
            throw new Error(`QuickBooks API request failed: ${error}`);
     * Handles QuickBooks query language requests
    protected async queryQBO<T = any>(
        const encodedQuery = encodeURIComponent(query);
        return this.makeQBORequest<T>(`query?query=${encodedQuery}`, 'GET', undefined, contextUser);
     * Converts QuickBooks date format to standard ISO format
    protected parseQBODate(qboDate: string): Date {
        // QuickBooks uses YYYY-MM-DD format
        return new Date(qboDate + 'T00:00:00Z');
     * Formats date for QuickBooks API
    protected formatQBODate(date: Date): string {
     * Maps QuickBooks account types to standard accounting categories
    protected mapAccountType(qboAccountType: string): string {
            'Bank': 'Asset',
            'Accounts Receivable': 'Asset',
            'Other Current Asset': 'Asset',
            'Fixed Asset': 'Asset',
            'Other Asset': 'Asset',
            'Accounts Payable': 'Liability',
            'Credit Card': 'Liability',
            'Long Term Liability': 'Liability',
            'Other Current Liability': 'Liability',
            'Other Income': 'Revenue',
            'Expense': 'Expense',
            'Other Expense': 'Expense'
        return typeMap[qboAccountType] || 'Other';
     * Gets the appropriate QuickBooks API URL based on configuration
    protected async getQuickBooksAPIUrl(integration: MJCompanyIntegrationEntity): Promise<string> {
        // First, check if there's a URL in the Integration entity
        // The Integration property should be loaded via the view, not accessed as a sub-property
        const integrationNavURL = (integration as any).IntegrationNavigationBaseURL;
        if (integrationNavURL) {
            return integrationNavURL;
        // Fall back to environment-based URL
        const isSandbox = integration.CustomAttribute1?.toLowerCase() === 'sandbox';
        return isSandbox 
            ? 'https://sandbox-quickbooks.api.intuit.com'
            : 'https://quickbooks.api.intuit.com';
     * Store the params for use in other methods
    private _params: RunActionParams['Params'];
     * Override the required abstract method
        // Store params for use in other methods
        this._params = params.Params;
        // This is an abstract base class, so we don't implement the actual logic here
        // Subclasses must implement this method
        throw new Error('InternalRunAction must be implemented by subclasses');
