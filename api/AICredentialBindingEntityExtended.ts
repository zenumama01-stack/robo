import { BaseEntity, RunView, ValidationResult, ValidationErrorInfo, ValidationErrorType } from '@memberjunction/core';
import { MJAICredentialBindingEntity, MJCredentialEntity } from '@memberjunction/core-entities';
import { AIEngineBase } from './BaseAIEngine';
 * Extended MJAICredentialBindingEntity class that validates credential types match vendor expectations.
 * When a credential is bound, this validates that the credential's CredentialTypeID matches the
 * CredentialTypeID expected by the vendor (as declared on the AIVendor record).
@RegisterClass(BaseEntity, 'MJ: AI Credential Bindings')
export class AICredentialBindingEntityExtended extends MJAICredentialBindingEntity {
     * Async validation that checks credential type compatibility.
     * Uses AIEngineBase.Instance for cached AI entity data and RunView for credentials.
    public override async ValidateAsync(): Promise<ValidationResult> {
        // Start with base validation (including synchronous validation)
        const result = await super.ValidateAsync();
        // Only validate credential type if we have both a credential and can determine the vendor
        if (this.CredentialID) {
            await this.validateCredentialTypeMatch(result);
     * Turn on async validation
    override get DefaultSkipAsyncValidation(): boolean {
        return false; // don't skip async validation
     * Validates that the bound credential's type matches the vendor's expected credential type.
     * The vendor is determined based on the binding type:
     * - Vendor binding: directly references the vendor
     * - ModelVendor binding: gets vendor from the ModelVendor record
     * - PromptModel binding: gets vendor from the PromptModel -> ModelVendor -> Vendor chain
    private async validateCredentialTypeMatch(result: ValidationResult): Promise<void> {
        // Get the expected credential type from the vendor
        const vendorCredentialTypeId = this.getExpectedCredentialTypeId();
        // If the vendor doesn't have a credential type requirement, skip validation
        if (!vendorCredentialTypeId) {
        // Load the credential to get its type
        const credential = await this.loadCredential();
        if (!credential) {
            result.Errors.push(new ValidationErrorInfo(
                'CredentialID',
                `Could not load credential with ID '${this.CredentialID}' to validate type compatibility.`,
                this.CredentialID,
                ValidationErrorType.Warning
        // Compare credential types
        if (credential.CredentialTypeID !== vendorCredentialTypeId) {
            const vendorName = this.getVendorNameForError();
            const expectedTypeName = this.getExpectedCredentialTypeName();
                `Credential type mismatch: The credential '${credential.Name}' has type '${credential.CredentialType || credential.CredentialTypeID}' but vendor '${vendorName}' expects type '${expectedTypeName || vendorCredentialTypeId}'.`,
                ValidationErrorType.Failure
     * Gets the expected CredentialTypeID from the vendor based on the binding type.
     * Uses AIEngineBase cached data for lookups.
    private getExpectedCredentialTypeId(): string | null {
        const engine = AIEngineBase.Instance;
        switch (this.BindingType) {
            case 'Vendor': {
                // Direct vendor binding - get type from the vendor
                const vendor = engine.Vendors.find(v => v.ID === this.AIVendorID);
                return vendor?.CredentialTypeID ?? null;
            case 'ModelVendor': {
                // ModelVendor binding - get vendor from ModelVendor, then get type
                const modelVendor = engine.ModelVendors.find(mv => mv.ID === this.AIModelVendorID);
                if (modelVendor) {
                    const vendor = engine.Vendors.find(v => v.ID === modelVendor.VendorID);
            case 'PromptModel': {
                // PromptModel binding - get vendor from PromptModel's VendorID
                const promptModel = engine.PromptModels.find(pm => pm.ID === this.AIPromptModelID);
                if (promptModel && promptModel.VendorID) {
                    const vendor = engine.Vendors.find(v => v.ID === promptModel.VendorID);
     * Gets the expected credential type name for error messages.
    private getExpectedCredentialTypeName(): string | null {
                return vendor?.CredentialType ?? null;
     * Gets the vendor name for error messages based on the binding type.
    private getVendorNameForError(): string {
                return vendor?.Name ?? this.AIVendorID ?? 'Unknown';
                    return vendor?.Name ?? modelVendor.Vendor ?? 'Unknown';
                return this.AIModelVendorID ?? 'Unknown';
                    return vendor?.Name ?? promptModel.Vendor ?? 'Unknown';
                return this.AIPromptModelID ?? 'Unknown';
                return 'Unknown';
     * Loads the credential using RunView.
     * Credentials are not cached in AIEngineBase, so we need to query them.
    private async loadCredential(): Promise<MJCredentialEntity | null> {
        if (!this.CredentialID) {
        const result = await rv.RunView<MJCredentialEntity>({
            EntityName: 'MJ: Credentials',
            ResultType: 'entity_object',
            CacheLocal: true // load all records and use cache for speed, filter client side below
        }, this.ContextCurrentUser);
            return result.Results.find(c => c.ID === this.CredentialID)
