 * Interface for a GL Code (Chart of Accounts) entry
export interface GLCode {
    subType: string;
    active: boolean;
    fullyQualifiedName: string;
 * QuickBooks Account object structure (partial)
interface QBOAccount {
    FullyQualifiedName: string;
    Active: boolean;
    Classification: string;
    AccountType: string;
    AccountSubType: string;
    CurrentBalance: number;
    CurrentBalanceWithSubAccounts: number;
    ParentRef?: {
    AcctNum?: string;
    SubAccount: boolean;
 * Action to retrieve the Chart of Accounts (GL Codes) from QuickBooks Online
@RegisterClass(BaseAction, 'GetQuickBooksGLCodesAction')
export class GetQuickBooksGLCodesAction extends QuickBooksBaseAction {
        return 'Retrieves the Chart of Accounts (GL Codes) from QuickBooks Online for a specific company';
            const includeInactive = this.getParamValue(params.Params, 'IncludeInactive') || false;
            const parentAccountID = this.getParamValue(params.Params, 'ParentAccountID');
            // Build the query
            if (!includeInactive) {
                const types = accountTypes.split(',').map((t: string) => `'${t.trim()}'`);
            // Add parent account filter
            if (parentAccountID) {
                conditions.push(`ParentRef = '${parentAccountID}'`);
            query += ' ORDER BY FullyQualifiedName';
            const response = await this.queryQBO<{ QueryResponse: { Account: QBOAccount[] } }>(
            // Process the results
            const accounts = response.QueryResponse?.Account || [];
            const glCodes: GLCode[] = accounts.map(account => this.mapQBOAccountToGLCode(account));
                    Name: 'GLCodes',
                    Value: glCodes,
                    Value: glCodes.length,
                Message: `Successfully retrieved ${glCodes.length} GL codes from QuickBooks`
     * Maps a QuickBooks account to our standard GL Code interface
    private mapQBOAccountToGLCode(account: QBOAccount): GLCode {
            id: account.Id,
            code: account.AcctNum || account.Id,
            name: account.Name,
            type: this.mapAccountType(account.AccountType),
            subType: account.AccountSubType,
            active: account.Active,
            parentId: account.ParentRef?.value,
            fullyQualifiedName: account.FullyQualifiedName
        // In QuickBooks: Asset, Expense = Debit; Liability, Equity, Revenue = Credit
