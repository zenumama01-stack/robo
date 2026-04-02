 * Interface for a GL Account (Chart of Accounts) entry from Business Central
export interface BCGLAccount {
    subCategory: string;
    accountType: string;
    directPosting: boolean;
    blocked: boolean;
    netChange: number;
    indentation: number;
 * Action to retrieve the Chart of Accounts from Microsoft Dynamics 365 Business Central
@RegisterClass(BaseAction, 'GetBusinessCentralGLAccountsAction')
export class GetBusinessCentralGLAccountsAction extends BusinessCentralBaseAction {
        return 'Retrieves the Chart of Accounts (GL Accounts) from Microsoft Dynamics 365 Business Central';
                filters.push("blocked eq false");
            const accountTypes = this.getParamValue(params.Params, 'AccountTypes');
            if (accountTypes) {
                const types = accountTypes.split(',').map((t: string) => t.trim());
                const typeFilters = types.map((type: string) => `accountType eq '${type}'`);
                if (typeFilters.length > 0) {
                    filters.push(`(${typeFilters.join(' or ')})`);
            const categories = this.getParamValue(params.Params, 'Categories');
            if (categories) {
                const cats = categories.split(',').map((c: string) => c.trim());
                const catFilters = cats.map((cat: string) => `category eq '${cat}'`);
                if (catFilters.length > 0) {
                    filters.push(`(${catFilters.join(' or ')})`);
                'category',
                'subCategory',
                'accountType',
                'directPosting',
                'lastModifiedDateTime',
                'netChange',
                'indentation'
            const orderBy = 'number';
            const maxResults = this.getParamValue(params.Params, 'MaxResults') || 1000;
                'generalLedgerAccounts',
            const bcAccounts = response.value || [];
            const glAccounts: BCGLAccount[] = bcAccounts.map(account => this.mapBCAccountToGLAccount(account));
            const summary = this.calculateAccountSummary(glAccounts);
            if (!params.Params.find(p => p.Name === 'GLAccounts')) {
                    Name: 'GLAccounts',
                    Value: glAccounts
                params.Params.find(p => p.Name === 'GLAccounts')!.Value = glAccounts;
                    Value: glAccounts.length
                params.Params.find(p => p.Name === 'TotalCount')!.Value = glAccounts.length;
                Message: `Successfully retrieved ${glAccounts.length} GL accounts from Business Central`
     * Maps a Business Central account to our standard GL Account interface
    private mapBCAccountToGLAccount(bcAccount: any): BCGLAccount {
            id: bcAccount.id,
            number: bcAccount.number,
            displayName: bcAccount.displayName,
            category: bcAccount.category,
            subCategory: bcAccount.subCategory || '',
            accountType: bcAccount.accountType,
            directPosting: bcAccount.directPosting || false,
            balance: bcAccount.balance || 0,
            blocked: bcAccount.blocked || false,
            lastModifiedDateTime: this.parseBCDate(bcAccount.lastModifiedDateTime),
            netChange: bcAccount.netChange || 0,
            debitAmount: bcAccount.debitAmount || 0,
            creditAmount: bcAccount.creditAmount || 0,
            indentation: bcAccount.indentation || 0
     * Calculate account summary statistics
    private calculateAccountSummary(accounts: BCGLAccount[]): any {
            totalAccounts: accounts.length,
            activeAccounts: accounts.filter(a => !a.blocked).length,
            blockedAccounts: accounts.filter(a => a.blocked).length,
            postingAccounts: accounts.filter(a => a.directPosting).length,
            headerAccounts: accounts.filter(a => !a.directPosting).length,
            accountsByCategory: {} as Record<string, number>,
            accountsByType: {} as Record<string, number>,
            totalDebit: 0,
            totalCredit: 0,
            netBalance: 0
            // Count by category
            summary.accountsByCategory[account.category] = (summary.accountsByCategory[account.category] || 0) + 1;
            summary.accountsByType[account.accountType] = (summary.accountsByType[account.accountType] || 0) + 1;
            summary.totalDebit += account.debitAmount;
            summary.totalCredit += account.creditAmount;
            summary.netBalance += account.balance;
                Name: 'AccountTypes',
                Name: 'Categories',
                Value: 1000
