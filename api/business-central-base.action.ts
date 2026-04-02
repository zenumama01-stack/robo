import { BaseAccountingAction } from '../../base/base-accounting-action';
import { MJCompanyIntegrationEntity } from '@memberjunction/core-entities';
 * Base class for all Microsoft Dynamics 365 Business Central actions.
 * Handles BC-specific authentication and API interaction patterns.
@RegisterClass(BaseAction, 'BusinessCentralBaseAction')
export abstract class BusinessCentralBaseAction extends BaseAccountingAction {
    protected accountingProvider = 'Business Central';
    protected integrationName = 'Microsoft Dynamics 365 Business Central';
     * Business Central API version
    protected apiVersion = 'v2.0';
     * Makes an authenticated request to Business Central API
    protected async makeBCRequest<T = any>(
        method: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE' = 'GET',
        body?: any,
    ): Promise<T> {
            throw new Error('Context user is required for Business Central API calls');
        // Get company ID from action params
        const companyId = this.getParamValue(this.params, 'CompanyID');
        if (!companyId) {
            throw new Error('CompanyID parameter is required');
        // Get the integration credentials
        const integration = await this.getCompanyIntegration(companyId, contextUser);
        // Get OAuth tokens (from env vars or database)
        const { accessToken } = await this.getOAuthTokens(integration);
        // Get Business Central environment and company info
        const environment = integration.CustomAttribute1 || 'production';
        const bcCompanyId = integration.ExternalSystemID;
        const tenantId = integration.CustomAttribute1 || this.getCredentialFromEnv(companyId, 'TENANT_ID');
        if (!bcCompanyId) {
            throw new Error('Business Central Company ID not found. Set in CompanyIntegration.ExternalSystemID');
            throw new Error('Tenant ID not found. Set in CompanyIntegration.CustomAttribute1 or environment variable');
        // Build the full URL
        const baseUrl = await this.getBusinessCentralAPIUrl(integration, tenantId, environment);
        const fullUrl = `${baseUrl}/companies(${bcCompanyId})/${endpoint}`;
        // Prepare headers
            'Authorization': `Bearer ${accessToken}`,
        // Add API version header
        headers['api-version'] = this.apiVersion;
            const response = await fetch(fullUrl, {
                body: body ? JSON.stringify(body) : undefined
                let errorMessage = `Business Central API error: ${response.status} ${response.statusText}`;
                    const errorJson = JSON.parse(errorText);
                        errorMessage = `Business Central API error: ${errorJson.error.message} (Code: ${errorJson.error.code})`;
                    errorMessage += ` - ${errorText}`;
            const result = await response.json();
            if (error instanceof Error) {
            throw new Error(`Business Central API request failed: ${error}`);
     * Handles Business Central OData queries
    protected async queryBC<T = any>(
        filters?: string[],
        select?: string[],
        expand?: string[],
        orderBy?: string,
        top?: number,
        const queryParams: string[] = [];
        if (filters && filters.length > 0) {
            queryParams.push(`$filter=${filters.join(' and ')}`);
        if (select && select.length > 0) {
            queryParams.push(`$select=${select.join(',')}`);
        if (expand && expand.length > 0) {
            queryParams.push(`$expand=${expand.join(',')}`);
        if (orderBy) {
            queryParams.push(`$orderby=${orderBy}`);
        if (top) {
            queryParams.push(`$top=${top}`);
        const queryString = queryParams.length > 0 ? `?${queryParams.join('&')}` : '';
        return this.makeBCRequest<T>(`${resource}${queryString}`, 'GET', undefined, contextUser);
     * Formats date for Business Central API (ISO 8601)
    protected formatBCDate(date: Date): string {
     * Parses Business Central date format
    protected parseBCDate(dateString: string): Date {
        return new Date(dateString);
     * Maps Business Central account types to standard categories
    protected mapAccountType(bcAccountType: string): string {
        const typeMap: Record<string, string> = {
            'Posting': 'Posting',
            'Heading': 'Header',
            'Total': 'Total',
            'Begin-Total': 'Subtotal',
            'End-Total': 'Subtotal'
        return typeMap[bcAccountType] || 'Other';
     * Maps Business Central account category to standard type
    protected mapAccountCategory(category: string): string {
        const categoryMap: Record<string, string> = {
            'Assets': 'Asset',
            'Liabilities': 'Liability',
            'Equity': 'Equity',
            'Income': 'Revenue',
            'Cost of Goods Sold': 'Expense',
            'Expense': 'Expense'
        return categoryMap[category] || 'Other';
     * Gets the appropriate Business Central API URL
    protected async getBusinessCentralAPIUrl(
        integration: MJCompanyIntegrationEntity,
        tenantId: string,
        environment: string
        // Default Business Central API URL pattern
        // Format: https://api.businesscentral.dynamics.com/v2.0/{tenant-id}/{environment}/api/v2.0
        return `https://api.businesscentral.dynamics.com/v2.0/${tenantId}/${environment}/api/${this.apiVersion}`;
     * Helper to build OData filter expressions
    protected buildFilterExpression(field: string, operator: string, value: any): string {
            return `${field} ${operator} '${value}'`;
        } else if (value instanceof Date) {
            return `${field} ${operator} ${value.toISOString()}`;
            return `${field} ${operator} ${value}`;
     * Current action parameters (set by the framework)
    protected params: any;
