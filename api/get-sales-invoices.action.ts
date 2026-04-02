 * Interface for a Sales Invoice from Business Central
export interface BCSalesInvoice {
    invoiceDate: Date;
    dueDate: Date;
    customerNumber: string;
    customerName: string;
    customerId: string;
    totalAmountExcludingTax: number;
    totalTaxAmount: number;
    totalAmountIncludingTax: number;
    remainingAmount: number;
    shipmentMethodId?: string;
    billToName: string;
    billToCustomerId: string;
    shipToName?: string;
    shipToContact?: string;
    sellToAddressLine1?: string;
    sellToAddressLine2?: string;
    sellToCity?: string;
    sellToState?: string;
    sellToCountry?: string;
    sellToPostCode?: string;
    externalDocumentNumber?: string;
    lines?: BCSalesInvoiceLine[];
export interface BCSalesInvoiceLine {
    lineNumber: number;
    lineType: string;
    itemId?: string;
    accountId?: string;
    quantity: number;
    unitPrice: number;
    discountAmount: number;
    discountPercent: number;
    netAmount: number;
    taxCode?: string;
    amountExcludingTax: number;
    taxPercent: number;
    amountIncludingTax: number;
 * Action to retrieve sales invoices from Microsoft Dynamics 365 Business Central
@RegisterClass(BaseAction, 'GetBusinessCentralSalesInvoicesAction')
export class GetBusinessCentralSalesInvoicesAction extends BusinessCentralBaseAction {
        return 'Retrieves sales invoices from Microsoft Dynamics 365 Business Central with comprehensive filtering options';
            // Customer filter
            const customerNumber = this.getParamValue(params.Params, 'CustomerNumber');
            if (customerNumber) {
                filters.push(`customerNumber eq '${customerNumber}'`);
            const status = this.getParamValue(params.Params, 'Status');
            if (status) {
                filters.push(`status eq '${status}'`);
                filters.push(`invoiceDate ge ${this.formatBCDate(new Date(startDate))}`);
                filters.push(`invoiceDate le ${this.formatBCDate(new Date(endDate))}`);
            // Due date filters
            const dueStartDate = this.getParamValue(params.Params, 'DueStartDate');
            if (dueStartDate) {
                filters.push(`dueDate ge ${this.formatBCDate(new Date(dueStartDate))}`);
            const dueEndDate = this.getParamValue(params.Params, 'DueEndDate');
            if (dueEndDate) {
                filters.push(`dueDate le ${this.formatBCDate(new Date(dueEndDate))}`);
                filters.push(`totalAmountIncludingTax ge ${minAmount}`);
                filters.push(`totalAmountIncludingTax le ${maxAmount}`);
            // Only unpaid filter
            const onlyUnpaid = this.getParamValue(params.Params, 'OnlyUnpaid');
            if (onlyUnpaid) {
                filters.push('remainingAmount gt 0');
                'invoiceDate',
                'dueDate',
                'customerNumber',
                'customerName',
                'customerId',
                'status',
                'totalAmountExcludingTax',
                'totalTaxAmount',
                'totalAmountIncludingTax',
                'remainingAmount',
                'shipmentMethodId',
                'billToName',
                'billToCustomerId',
                'shipToName',
                'shipToContact',
                'sellToAddressLine1',
                'sellToAddressLine2',
                'sellToCity',
                'sellToState',
                'sellToCountry',
                'sellToPostCode',
                'externalDocumentNumber',
            // Expand lines if requested
            const includeLines = this.getParamValue(params.Params, 'IncludeLines');
            if (includeLines) {
                expand.push('salesInvoiceLines');
            const orderBy = 'invoiceDate desc,number desc';
                'salesInvoices',
            const bcInvoices = response.value || [];
            const invoices: BCSalesInvoice[] = bcInvoices.map(invoice => this.mapBCSalesInvoice(invoice));
            const summary = this.calculateInvoiceSummary(invoices);
            if (!params.Params.find(p => p.Name === 'Invoices')) {
                    Name: 'Invoices',
                    Value: invoices
                params.Params.find(p => p.Name === 'Invoices')!.Value = invoices;
                    Value: invoices.length
                params.Params.find(p => p.Name === 'TotalCount')!.Value = invoices.length;
                Message: `Successfully retrieved ${invoices.length} sales invoices from Business Central`
     * Map Business Central sales invoice to standard format
    private mapBCSalesInvoice(bcInvoice: any): BCSalesInvoice {
            id: bcInvoice.id,
            number: bcInvoice.number,
            invoiceDate: this.parseBCDate(bcInvoice.invoiceDate),
            dueDate: this.parseBCDate(bcInvoice.dueDate),
            customerNumber: bcInvoice.customerNumber,
            customerName: bcInvoice.customerName,
            customerId: bcInvoice.customerId,
            status: bcInvoice.status,
            totalAmountExcludingTax: bcInvoice.totalAmountExcludingTax || 0,
            totalTaxAmount: bcInvoice.totalTaxAmount || 0,
            totalAmountIncludingTax: bcInvoice.totalAmountIncludingTax || 0,
            remainingAmount: bcInvoice.remainingAmount || 0,
            currencyCode: bcInvoice.currencyCode || 'USD',
            paymentTermsId: bcInvoice.paymentTermsId,
            shipmentMethodId: bcInvoice.shipmentMethodId,
            billToName: bcInvoice.billToName,
            billToCustomerId: bcInvoice.billToCustomerId,
            shipToName: bcInvoice.shipToName,
            shipToContact: bcInvoice.shipToContact,
            sellToAddressLine1: bcInvoice.sellToAddressLine1,
            sellToAddressLine2: bcInvoice.sellToAddressLine2,
            sellToCity: bcInvoice.sellToCity,
            sellToState: bcInvoice.sellToState,
            sellToCountry: bcInvoice.sellToCountry,
            sellToPostCode: bcInvoice.sellToPostCode,
            externalDocumentNumber: bcInvoice.externalDocumentNumber,
            lastModifiedDateTime: this.parseBCDate(bcInvoice.lastModifiedDateTime),
            lines: bcInvoice.salesInvoiceLines ? this.mapInvoiceLines(bcInvoice.salesInvoiceLines) : undefined
     * Map invoice lines
    private mapInvoiceLines(lines: any[]): BCSalesInvoiceLine[] {
        return lines.map(line => ({
            id: line.id,
            lineNumber: line.lineNumber || line.sequence,
            lineType: line.lineType,
            itemId: line.itemId,
            accountId: line.accountId,
            description: line.description || '',
            quantity: line.quantity || 0,
            unitPrice: line.unitPrice || 0,
            discountAmount: line.discountAmount || 0,
            discountPercent: line.discountPercent || 0,
            netAmount: line.netAmount || 0,
            taxCode: line.taxCode,
            amountExcludingTax: line.amountExcludingTax || 0,
            taxPercent: line.taxPercent || 0,
            totalTaxAmount: line.totalTaxAmount || 0,
            amountIncludingTax: line.amountIncludingTax || 0
     * Calculate invoice summary statistics
    private calculateInvoiceSummary(invoices: BCSalesInvoice[]): any {
            totalInvoices: invoices.length,
            totalAmountExcludingTax: 0,
            totalTaxAmount: 0,
            totalAmountIncludingTax: 0,
            totalRemainingAmount: 0,
            totalPaidAmount: 0,
            averageInvoiceAmount: 0,
            invoicesByStatus: {} as Record<string, number>,
            overdueInvoices: 0,
            oldestUnpaidDate: null as Date | null,
            largestUnpaidAmount: 0
            summary.totalAmountExcludingTax += invoice.totalAmountExcludingTax;
            summary.totalTaxAmount += invoice.totalTaxAmount;
            summary.totalAmountIncludingTax += invoice.totalAmountIncludingTax;
            summary.totalRemainingAmount += invoice.remainingAmount;
            summary.totalPaidAmount += (invoice.totalAmountIncludingTax - invoice.remainingAmount);
            // Count by status
            summary.invoicesByStatus[invoice.status] = (summary.invoicesByStatus[invoice.status] || 0) + 1;
            // Track overdue
            if (invoice.remainingAmount > 0 && invoice.dueDate < today) {
                summary.overdueInvoices++;
                summary.overdueAmount += invoice.remainingAmount;
            // Track oldest unpaid
            if (invoice.remainingAmount > 0) {
                if (!summary.oldestUnpaidDate || invoice.invoiceDate < summary.oldestUnpaidDate) {
                    summary.oldestUnpaidDate = invoice.invoiceDate;
                if (invoice.remainingAmount > summary.largestUnpaidAmount) {
                    summary.largestUnpaidAmount = invoice.remainingAmount;
        summary.averageInvoiceAmount = invoices.length > 0 
            ? summary.totalAmountIncludingTax / invoices.length 
                Name: 'CustomerNumber',
                Name: 'Status',
                Name: 'DueStartDate',
                Name: 'DueEndDate',
                Name: 'OnlyUnpaid',
                Name: 'IncludeLines',
                Value: true
