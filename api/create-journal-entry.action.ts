import { QuickBooksBaseAction } from '../quickbooks-base.action';
 * Journal entry line interface
export interface JournalEntryLine {
    debit?: number;
    credit?: number;
    entityType?: 'Customer' | 'Vendor' | 'Employee';
    entityId?: string;
    classId?: string;
    departmentId?: string;
 * Action to create a journal entry in QuickBooks Online
@RegisterClass(BaseAction, 'CreateQuickBooksJournalEntryAction')
export class CreateQuickBooksJournalEntryAction extends QuickBooksBaseAction {
        return 'Creates a journal entry in QuickBooks Online with automatic validation';
            (this as any)._params = params.Params;
            // Get parameter values
            const entryDate = this.getParamValue(params.Params, 'EntryDate') || new Date();
            const docNumber = this.getParamValue(params.Params, 'DocNumber');
            const privateNote = this.getParamValue(params.Params, 'PrivateNote');
            const linesData = this.getParamValue(params.Params, 'Lines');
            const adjustmentEntry = this.getParamValue(params.Params, 'AdjustmentEntry') || false;
            // Validate and parse lines
            const lines = this.parseAndValidateLines(linesData);
            // Validate journal entry balance
            if (!this.validateJournalEntryBalance(lines)) {
                    Message: 'Journal entry is not balanced. Total debits must equal total credits.',
                    Params: params.Params
            // Build the journal entry object for QuickBooks
            const journalEntry = {
                DocNumber: docNumber,
                TxnDate: this.formatQBODate(entryDate instanceof Date ? entryDate : new Date(entryDate)),
                PrivateNote: privateNote,
                Adjustment: adjustmentEntry,
                Line: lines.map((line, index) => this.mapToQBOJournalLine(line, index + 1))
            // Create the journal entry in QuickBooks
            const response = await this.makeQBORequest<{ JournalEntry: any }>(
                'journalentry',
                'POST',
                journalEntry,
            const createdEntry = response.JournalEntry;
            // Set output parameters
            const outputParams: ActionParam[] = [
                    Name: 'JournalEntryID',
                    Value: createdEntry.Id,
                    Type: 'Output'
                    Name: 'DocNumber',
                    Value: createdEntry.DocNumber,
                    Name: 'TotalAmount',
                    Value: createdEntry.TotalAmt,
                    Name: 'CreatedDate',
                    Value: createdEntry.MetaData.CreateTime,
                Params: [...params.Params, ...outputParams],
                Message: `Journal entry ${createdEntry.DocNumber} created successfully`
                Message: errorMessage,
     * Parse and validate journal entry lines
    private parseAndValidateLines(linesParam: any): JournalEntryLine[] {
        if (!linesParam) {
            throw new Error('Lines parameter is required');
        let lines: JournalEntryLine[];
        // Handle if lines is a JSON string
        if (typeof linesParam === 'string') {
                lines = JSON.parse(linesParam);
                throw new Error('Invalid JSON format for Lines parameter');
            lines = linesParam;
        if (!Array.isArray(lines)) {
            throw new Error('Lines must be an array');
        if (lines.length < 2) {
            throw new Error('Journal entry must have at least 2 lines');
        // Validate each line
        lines.forEach((line, index) => {
            if (!line.accountId) {
                throw new Error(`Line ${index + 1}: accountId is required`);
            if (line.debit === undefined && line.credit === undefined) {
                throw new Error(`Line ${index + 1}: either debit or credit amount is required`);
            if (line.debit !== undefined && line.credit !== undefined) {
                throw new Error(`Line ${index + 1}: cannot have both debit and credit on the same line`);
            if (line.debit !== undefined && line.debit < 0) {
                throw new Error(`Line ${index + 1}: debit amount cannot be negative`);
            if (line.credit !== undefined && line.credit < 0) {
                throw new Error(`Line ${index + 1}: credit amount cannot be negative`);
     * Map journal entry line to QuickBooks format
    private mapToQBOJournalLine(line: JournalEntryLine, lineNumber: number): any {
        const qbLine: any = {
            DetailType: 'JournalEntryLineDetail',
            Amount: line.debit || line.credit || 0,
            JournalEntryLineDetail: {
                PostingType: line.debit ? 'Debit' : 'Credit',
                AccountRef: {
                    value: line.accountId
        // Add description if provided
        if (line.description) {
            qbLine.Description = line.description;
        // Add entity reference if provided
        if (line.entityType && line.entityId) {
            qbLine.JournalEntryLineDetail.Entity = {
                Type: line.entityType,
                EntityRef: {
                    value: line.entityId
        // Add class reference if provided
        if (line.classId) {
            qbLine.JournalEntryLineDetail.ClassRef = {
                value: line.classId
        // Add department reference if provided
        if (line.departmentId) {
            qbLine.JournalEntryLineDetail.DepartmentRef = {
                value: line.departmentId
        return qbLine;
