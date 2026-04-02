 * Interface for a General Ledger Entry from Business Central
export interface BCGeneralLedgerEntry {
    entryNumber: number;
    postingDate: Date;
    documentNumber: string;
    documentType: string;
    accountId: string;
    accountNumber: string;
    accountName: string;
    debitAmount: number;
    creditAmount: number;
    amount: number;
    dimensionSetLines?: BCDimensionSetLine[];
export interface BCDimensionSetLine {
    valueId: string;
    valueCode: string;
    valueDisplayName: string;
 * Action to retrieve General Ledger Entries from Microsoft Dynamics 365 Business Central
@RegisterClass(BaseAction, 'GetBusinessCentralGeneralLedgerEntriesAction')
export class GetBusinessCentralGeneralLedgerEntriesAction extends BusinessCentralBaseAction {
        return 'Retrieves General Ledger Entries (journal entries) from Microsoft Dynamics 365 Business Central';
            // Date range filters
            const startDate = this.getParamValue(params.Params, 'StartDate');
            if (startDate) {
                filters.push(`postingDate ge ${this.formatBCDate(new Date(startDate))}`);
            const endDate = this.getParamValue(params.Params, 'EndDate');
            if (endDate) {
                filters.push(`postingDate le ${this.formatBCDate(new Date(endDate))}`);
            // Account filter
            const accountNumber = this.getParamValue(params.Params, 'AccountNumber');
            if (accountNumber) {
                filters.push(`accountNumber eq '${accountNumber}'`);
            // Document number filter
            const documentNumber = this.getParamValue(params.Params, 'DocumentNumber');
            if (documentNumber) {
                filters.push(`documentNumber eq '${documentNumber}'`);
            // Document type filter
            const documentType = this.getParamValue(params.Params, 'DocumentType');
            if (documentType) {
                filters.push(`documentType eq '${documentType}'`);
            // Amount filters
            const minAmount = this.getParamValue(params.Params, 'MinAmount');
            if (minAmount !== undefined) {
                filters.push(`amount ge ${minAmount}`);
            const maxAmount = this.getParamValue(params.Params, 'MaxAmount');
            if (maxAmount !== undefined) {
                filters.push(`amount le ${maxAmount}`);
                'entryNumber',
                'postingDate',
                'documentNumber',
                'documentType',
                'accountId',
                'accountNumber',
                'description',
                'debitAmount',
                'creditAmount',
                'amount',
            // Expand dimensions if requested
            const expand = [];
            const includeDimensions = this.getParamValue(params.Params, 'IncludeDimensions');
            if (includeDimensions) {
                expand.push('dimensionSetLines');
            const orderBy = 'postingDate desc,entryNumber desc';
            const maxResults = this.getParamValue(params.Params, 'MaxResults') || 500;
                'generalLedgerEntries',
                expand,
            const bcEntries = response.value || [];
            const glEntries: BCGeneralLedgerEntry[] = [];
            // Map entries and get account names
            for (const entry of bcEntries) {
                const mappedEntry = await this.mapBCEntryToGLEntry(entry, contextUser);
                glEntries.push(mappedEntry);
            const summary = this.calculateEntrySummary(glEntries);
            if (!params.Params.find(p => p.Name === 'GLEntries')) {
                    Name: 'GLEntries',
                    Value: glEntries
                params.Params.find(p => p.Name === 'GLEntries')!.Value = glEntries;
                    Value: glEntries.length
                params.Params.find(p => p.Name === 'TotalCount')!.Value = glEntries.length;
                Message: `Successfully retrieved ${glEntries.length} general ledger entries from Business Central`
     * Maps a Business Central GL entry to our standard interface
    private async mapBCEntryToGLEntry(bcEntry: any, contextUser: any): Promise<BCGeneralLedgerEntry> {
        // Get account name if not included
        let accountName = bcEntry.accountName || '';
        if (!accountName && bcEntry.accountId) {
                const accountResponse = await this.makeBCRequest<any>(
                    `generalLedgerAccounts(${bcEntry.accountId})`,
                    'GET',
                accountName = accountResponse.displayName || '';
                // If we can't get the account name, just use the number
                accountName = bcEntry.accountNumber;
            id: bcEntry.id,
            entryNumber: bcEntry.entryNumber,
            postingDate: this.parseBCDate(bcEntry.postingDate),
            documentNumber: bcEntry.documentNumber || '',
            documentType: bcEntry.documentType || '',
            accountId: bcEntry.accountId,
            accountNumber: bcEntry.accountNumber,
            accountName: accountName,
            description: bcEntry.description || '',
            debitAmount: bcEntry.debitAmount || 0,
            creditAmount: bcEntry.creditAmount || 0,
            amount: bcEntry.amount || 0,
            lastModifiedDateTime: this.parseBCDate(bcEntry.lastModifiedDateTime),
            dimensionSetLines: bcEntry.dimensionSetLines ? this.mapDimensionSetLines(bcEntry.dimensionSetLines) : undefined
     * Map dimension set lines
    private mapDimensionSetLines(dimensions: any[]): BCDimensionSetLine[] {
        return dimensions.map(dim => ({
            id: dim.id,
            code: dim.code,
            displayName: dim.displayName || dim.code,
            valueId: dim.valueId,
            valueCode: dim.valueCode,
            valueDisplayName: dim.valueDisplayName || dim.valueCode
     * Calculate entry summary statistics
    private calculateEntrySummary(entries: BCGeneralLedgerEntry[]): any {
            totalEntries: entries.length,
            totalDebits: 0,
            totalCredits: 0,
            netAmount: 0,
            earliestDate: null as Date | null,
            latestDate: null as Date | null,
            entriesByType: {} as Record<string, number>,
            entriesByAccount: {} as Record<string, { count: number; debit: number; credit: number }>,
            isBalanced: true
            // Sum totals
            summary.totalDebits += entry.debitAmount;
            summary.totalCredits += entry.creditAmount;
            summary.netAmount += entry.amount;
            // Track dates
            if (!summary.earliestDate || entry.postingDate < summary.earliestDate) {
                summary.earliestDate = entry.postingDate;
            if (!summary.latestDate || entry.postingDate > summary.latestDate) {
                summary.latestDate = entry.postingDate;
            // Count by type
            const type = entry.documentType || 'Other';
            summary.entriesByType[type] = (summary.entriesByType[type] || 0) + 1;
            // Sum by account
            const accountKey = `${entry.accountNumber} - ${entry.accountName}`;
            if (!summary.entriesByAccount[accountKey]) {
                summary.entriesByAccount[accountKey] = { count: 0, debit: 0, credit: 0 };
            summary.entriesByAccount[accountKey].count++;
            summary.entriesByAccount[accountKey].debit += entry.debitAmount;
            summary.entriesByAccount[accountKey].credit += entry.creditAmount;
        // Check if balanced (debits = credits)
        summary.isBalanced = Math.abs(summary.totalDebits - summary.totalCredits) < 0.01;
                Name: 'StartDate',
                Name: 'EndDate',
                Name: 'AccountNumber',
                Name: 'DocumentNumber',
                Name: 'DocumentType',
                Name: 'MinAmount',
                Name: 'MaxAmount',
                Name: 'IncludeDimensions',
                Value: 500
