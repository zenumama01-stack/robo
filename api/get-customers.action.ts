import { BusinessCentralBaseAction } from '../business-central-base.action';
 * Interface for a Customer from Business Central
export interface BCCustomer {
    number: string;
    displayName: string;
    phoneNumber: string;
    address: BCAddress;
    postalCode: string;
    currencyCode: string;
    paymentTermsId?: string;
    paymentMethodId?: string;
    taxLiable: boolean;
    taxAreaId?: string;
    blocked: string; // ' ', 'Invoice', 'Ship', 'All'
    balance: number;
    overdueAmount: number;
    totalSalesExcludingTax: number;
    lastModifiedDateTime: Date;
export interface BCAddress {
    street?: string;
    city?: string;
    countryLetterCode?: string;
    postalCode?: string;
 * Action to retrieve customers from Microsoft Dynamics 365 Business Central
@RegisterClass(BaseAction, 'GetBusinessCentralCustomersAction')
export class GetBusinessCentralCustomersAction extends BusinessCentralBaseAction {
     * Description of the action
    public get Description(): string {
        return 'Retrieves customers from Microsoft Dynamics 365 Business Central with filtering and search options';
     * Main execution method
            const contextUser = params.ContextUser;
                    Message: 'Context user is required for Business Central API calls'
            // Store params for base class methods
            this.params = params.Params;
            // Build filters based on parameters
            const filters: string[] = [];
            // Search text filter
            const searchText = this.getParamValue(params.Params, 'SearchText');
            if (searchText) {
                filters.push(`contains(displayName,'${searchText}') or contains(number,'${searchText}') or contains(email,'${searchText}')`);
            // Blocked filter
            const includeBlocked = this.getParamValue(params.Params, 'IncludeBlocked');
            if (!includeBlocked) {
                filters.push("blocked eq ' '");
            // Customer type filter
            const customerType = this.getParamValue(params.Params, 'CustomerType');
            if (customerType) {
                filters.push(`type eq '${customerType}'`);
            // Balance filters
            const minBalance = this.getParamValue(params.Params, 'MinBalance');
            if (minBalance !== undefined) {
                filters.push(`balance ge ${minBalance}`);
            const maxBalance = this.getParamValue(params.Params, 'MaxBalance');
            if (maxBalance !== undefined) {
                filters.push(`balance le ${maxBalance}`);
            // Overdue filter
            const onlyOverdue = this.getParamValue(params.Params, 'OnlyOverdue');
            if (onlyOverdue) {
                filters.push('overdueAmount gt 0');
            // Select fields to retrieve
            const select = [
                'id',
                'number',
                'displayName',
                'type',
                'email',
                'phoneNumber',
                'address',
                'city',
                'state',
                'country',
                'postalCode',
                'currencyCode',
                'paymentTermsId',
                'paymentMethodId',
                'taxLiable',
                'taxAreaId',
                'blocked',
                'balance',
                'overdueAmount',
                'totalSalesExcludingTax',
                'lastModifiedDateTime'
            // Order by
            const sortBy = this.getParamValue(params.Params, 'SortBy') || 'displayName';
            const orderBy = this.mapSortField(sortBy);
            // Max results
            const maxResults = this.getParamValue(params.Params, 'MaxResults') || 100;
            // Execute the query
            const response = await this.queryBC<{ value: any[] }>(
                'customers',
                select,
                maxResults,
            const bcCustomers = response.value || [];
            const customers: BCCustomer[] = bcCustomers.map(customer => this.mapBCCustomer(customer));
            // Calculate summary
            const summary = this.calculateCustomerSummary(customers);
            // Create output parameters
            if (!params.Params.find(p => p.Name === 'Customers')) {
                params.Params.push({
                    Name: 'Customers',
                    Type: 'Output',
                    Value: customers
                params.Params.find(p => p.Name === 'Customers')!.Value = customers;
            if (!params.Params.find(p => p.Name === 'TotalCount')) {
                    Name: 'TotalCount',
                    Value: customers.length
                params.Params.find(p => p.Name === 'TotalCount')!.Value = customers.length;
            if (!params.Params.find(p => p.Name === 'Summary')) {
                    Name: 'Summary',
                    Value: summary
                params.Params.find(p => p.Name === 'Summary')!.Value = summary;
                ResultCode: 'SUCCESS',
                Message: `Successfully retrieved ${customers.length} customers from Business Central`
                Message: errorMessage
     * Map sort field name
    private mapSortField(sortBy: string): string {
        const sortMap: Record<string, string> = {
            'displayName': 'displayName',
            'number': 'number',
            'balance': 'balance desc',
            'overdueAmount': 'overdueAmount desc',
            'lastModified': 'lastModifiedDateTime desc'
        return sortMap[sortBy] || 'displayName';
     * Map Business Central customer to standard format
    private mapBCCustomer(bcCustomer: any): BCCustomer {
            id: bcCustomer.id,
            number: bcCustomer.number,
            displayName: bcCustomer.displayName,
            type: bcCustomer.type || 'Company',
            email: bcCustomer.email || '',
            phoneNumber: bcCustomer.phoneNumber || '',
            address: this.mapAddress(bcCustomer.address),
            city: bcCustomer.city || '',
            state: bcCustomer.state || '',
            country: bcCustomer.country || '',
            postalCode: bcCustomer.postalCode || '',
            currencyCode: bcCustomer.currencyCode || 'USD',
            paymentTermsId: bcCustomer.paymentTermsId,
            paymentMethodId: bcCustomer.paymentMethodId,
            taxLiable: bcCustomer.taxLiable || true,
            taxAreaId: bcCustomer.taxAreaId,
            blocked: bcCustomer.blocked || ' ',
            balance: bcCustomer.balance || 0,
            overdueAmount: bcCustomer.overdueAmount || 0,
            totalSalesExcludingTax: bcCustomer.totalSalesExcludingTax || 0,
            lastModifiedDateTime: this.parseBCDate(bcCustomer.lastModifiedDateTime)
     * Map address object
    private mapAddress(address: any): BCAddress {
        if (!address) {
            street: address.street,
            city: address.city,
            state: address.state,
            countryLetterCode: address.countryLetterCode,
            postalCode: address.postalCode
     * Calculate customer summary statistics
    private calculateCustomerSummary(customers: BCCustomer[]): any {
            totalCustomers: customers.length,
            activeCustomers: customers.filter(c => c.blocked === ' ').length,
            blockedCustomers: customers.filter(c => c.blocked !== ' ').length,
            totalBalance: customers.reduce((sum, c) => sum + c.balance, 0),
            totalOverdue: customers.reduce((sum, c) => sum + c.overdueAmount, 0),
            totalSales: customers.reduce((sum, c) => sum + c.totalSalesExcludingTax, 0),
            averageBalance: customers.length > 0 
                ? customers.reduce((sum, c) => sum + c.balance, 0) / customers.length 
            customersWithBalance: customers.filter(c => c.balance > 0).length,
            customersWithOverdue: customers.filter(c => c.overdueAmount > 0).length,
            customersByType: customers.reduce((acc, c) => {
                acc[c.type] = (acc[c.type] || 0) + 1;
            }, {} as Record<string, number>)
     * Define the parameters for this action
    public get Params(): ActionParam[] {
        const baseParams = this.getCommonAccountingParams();
        const specificParams: ActionParam[] = [
                Name: 'SearchText',
                Name: 'IncludeBlocked',
                Value: false
                Name: 'CustomerType',
                Name: 'MinBalance',
                Name: 'MaxBalance',
                Name: 'OnlyOverdue',
                Name: 'SortBy',
                Value: 'displayName'
                Name: 'MaxResults',
                Value: 100
        return [...baseParams, ...specificParams];
