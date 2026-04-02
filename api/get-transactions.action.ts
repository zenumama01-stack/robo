 * Standard transaction interface
export interface Transaction {
    transactionType: string;
    transactionNumber?: string;
    transactionDate: Date;
    referenceNumber?: string;
    memo?: string;
    entityName?: string; // Customer/Vendor name
    lines: TransactionLine[];
    metadata: Record<string, any>; // Store original QB data
export interface TransactionLine {
    accountName?: string;
    itemName?: string;
    quantity?: number;
    rate?: number;
 * Action to retrieve transactions from QuickBooks Online
@RegisterClass(BaseAction, 'GetQuickBooksTransactionsAction')
export class GetQuickBooksTransactionsAction extends QuickBooksBaseAction {
        return 'Retrieves transactions from QuickBooks Online with flexible filtering options';
                    ResultCode: 'ERROR_NO_CONTEXT_USER',
                    Message: 'Context user is required for QuickBooks API calls',
            const transactionType = this.getParamValue(params.Params, 'TransactionType');
            const transactions: Transaction[] = [];
            if (transactionType) {
                // Get specific transaction type
                const result = await this.getTransactionsByType(transactionType, params, contextUser);
                transactions.push(...result);
                // Get all transaction types
                const types = ['Invoice', 'Bill', 'Payment', 'JournalEntry', 'Deposit', 'Purchase'];
                for (const type of types) {
                        const result = await this.getTransactionsByType(type, params, contextUser);
                        // Log but continue with other types
                        console.warn(`Failed to retrieve ${type} transactions:`, error);
            // Sort by date descending
            transactions.sort((a, b) => b.transactionDate.getTime() - a.transactionDate.getTime());
            // Apply max results limit
            const maxResults = Math.min(this.getParamValue(params.Params, 'MaxResults') || 100, 1000);
            const limitedTransactions = transactions.slice(0, maxResults);
                    Name: 'Transactions',
                    Value: limitedTransactions,
                    Value: limitedTransactions.length,
                    Name: 'HasMore',
                    Value: transactions.length > maxResults,
                Message: `Successfully retrieved ${limitedTransactions.length} transactions`,
                Params: [...params.Params, ...outputParams]
     * Get transactions of a specific type
    private async getTransactionsByType(
        params: RunActionParams,
        contextUser: any
    ): Promise<Transaction[]> {
        // Build query for specific transaction type
        let query = `SELECT * FROM ${type}`;
        // Add date range filter
            conditions.push(`TxnDate >= '${this.formatQBODate(new Date(startDate))}'`);
            conditions.push(`TxnDate <= '${this.formatQBODate(new Date(endDate))}'`);
        // Add amount filters
            conditions.push(`TotalAmt >= ${minAmount}`);
            conditions.push(`TotalAmt <= ${maxAmount}`);
        // Add entity filter based on transaction type
        const entityId = this.getParamValue(params.Params, 'EntityID');
            if (['Invoice', 'Payment', 'Deposit'].includes(type)) {
                conditions.push(`CustomerRef = '${entityId}'`);
            } else if (['Bill', 'Purchase'].includes(type)) {
                conditions.push(`VendorRef = '${entityId}'`);
        // Add ordering and limit
        query += ' ORDER BY TxnDate DESC';
        query += ` MAXRESULTS ${this.getParamValue(params.Params, 'MaxResults') || 100}`;
        const response = await this.queryQBO<{ QueryResponse: { [key: string]: any[] } }>(
        const qbTransactions = response.QueryResponse?.[type] || [];
        return qbTransactions.map(qbTxn => this.mapQuickBooksTransaction(type, qbTxn));
     * Map QuickBooks transaction to standard format
    private mapQuickBooksTransaction(type: string, qbTxn: any): Transaction {
        const transaction: Transaction = {
            id: qbTxn.Id,
            transactionType: type,
            transactionNumber: qbTxn.DocNumber,
            transactionDate: this.parseQBODate(qbTxn.TxnDate),
            amount: qbTxn.TotalAmt || 0,
            currency: qbTxn.CurrencyRef?.value || 'USD',
            status: this.getTransactionStatus(type, qbTxn),
            referenceNumber: qbTxn.PrivateNote || qbTxn.CustomerMemo?.value,
            memo: qbTxn.Memo,
            entityName: this.getEntityName(type, qbTxn),
            entityId: this.getEntityId(type, qbTxn),
            lines: this.mapTransactionLines(type, qbTxn),
            metadata: qbTxn
        return transaction;
     * Get transaction status based on type
    private getTransactionStatus(type: string, qbTxn: any): string {
            case 'Invoice':
                return qbTxn.Balance > 0 ? 'Unpaid' : 'Paid';
            case 'Bill':
            case 'Payment':
                return 'Completed';
            case 'JournalEntry':
                return 'Posted';
                return qbTxn.TxnStatus || 'Unknown';
     * Get entity name based on transaction type
    private getEntityName(type: string, qbTxn: any): string | undefined {
            return qbTxn.CustomerRef?.name;
            return qbTxn.VendorRef?.name;
     * Get entity ID based on transaction type
    private getEntityId(type: string, qbTxn: any): string | undefined {
            return qbTxn.CustomerRef?.value;
            return qbTxn.VendorRef?.value;
     * Map transaction lines
    private mapTransactionLines(type: string, qbTxn: any): TransactionLine[] {
        const lines: TransactionLine[] = [];
        const qbLines = qbTxn.Line || [];
        qbLines.forEach((line: any, index: number) => {
            // Skip summary lines
            if (line.DetailType === 'SubTotalLineDetail') {
            const mappedLine: TransactionLine = {
                id: line.Id || `${qbTxn.Id}-${index}`,
                lineNumber: index + 1,
                description: line.Description,
                amount: line.Amount || 0,
                accountId: this.getLineAccountId(line),
                accountName: this.getLineAccountName(line),
                itemId: line.SalesItemLineDetail?.ItemRef?.value || line.ItemBasedExpenseLineDetail?.ItemRef?.value,
                itemName: line.SalesItemLineDetail?.ItemRef?.name || line.ItemBasedExpenseLineDetail?.ItemRef?.name,
                quantity: line.SalesItemLineDetail?.Qty || line.ItemBasedExpenseLineDetail?.Qty,
                rate: line.SalesItemLineDetail?.UnitPrice || line.ItemBasedExpenseLineDetail?.UnitPrice
            lines.push(mappedLine);
     * Get account ID from line detail
    private getLineAccountId(line: any): string | undefined {
        return line.AccountBasedExpenseLineDetail?.AccountRef?.value ||
               line.DepositLineDetail?.AccountRef?.value ||
               line.JournalEntryLineDetail?.AccountRef?.value;
     * Get account name from line detail
    private getLineAccountName(line: any): string | undefined {
        return line.AccountBasedExpenseLineDetail?.AccountRef?.name ||
               line.DepositLineDetail?.AccountRef?.name ||
               line.JournalEntryLineDetail?.AccountRef?.name;
