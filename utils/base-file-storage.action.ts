import { MJFileStorageAccountEntity, MJFileStorageProviderEntity } from "@memberjunction/core-entities";
import { FileStorageBase, initializeDriverWithAccountCredentials } from "@memberjunction/storage";
 * Abstract base class for file storage operations.
 * Provides shared functionality for all file storage action implementations:
 * - Storage provider lookup and initialization
 * - Parameter extraction helpers
 * - Result creation utilities
 * - Error handling patterns
export abstract class BaseFileStorageAction extends BaseAction {
     * Get storage account entity by name
     * @param accountName - Name of the storage account
     * @returns MJFileStorageAccountEntity or null if not found
    protected async getStorageAccount(accountName: string, contextUser: UserInfo): Promise<MJFileStorageAccountEntity | null> {
        const result = await rv.RunView<MJFileStorageAccountEntity>({
            EntityName: 'MJ: File Storage Accounts',
            ExtraFilter: `Name='${accountName.replace(/'/g, "''")}'`,
     * Get storage provider entity by ID
     * @param providerId - ID of the storage provider
     * @returns MJFileStorageProviderEntity or null if not found
    protected async getStorageProviderById(providerId: string, contextUser: UserInfo): Promise<MJFileStorageProviderEntity | null> {
        const result = await rv.RunView<MJFileStorageProviderEntity>({
            EntityName: 'MJ: File Storage Providers',
            ExtraFilter: `ID='${providerId}'`,
     * Initialize storage driver using the enterprise credential model
     * @param accountEntity - MJFileStorageAccountEntity to initialize
     * @param providerEntity - MJFileStorageProviderEntity for the account
     * @param contextUser - User context for credential access
     * @returns Initialized FileStorageBase driver
    protected async initializeDriver(
        accountEntity: MJFileStorageAccountEntity,
        providerEntity: MJFileStorageProviderEntity,
    ): Promise<FileStorageBase> {
        return initializeDriverWithAccountCredentials({
            accountEntity,
            providerEntity,
     * Get storage account and initialize driver in one step using enterprise model
     * @param params - Action parameters containing StorageAccount
     * @returns Initialized driver and result if error occurred
    protected async getDriverFromParams(params: RunActionParams): Promise<{ driver?: FileStorageBase; error?: ActionResultSimple }> {
        const accountName = this.getStringParam(params, 'storageaccount');
        if (!accountName) {
                error: this.createErrorResult("StorageAccount parameter is required", "MISSING_ACCOUNT")
        const account = await this.getStorageAccount(accountName, params.ContextUser);
                error: this.createErrorResult(`Storage account '${accountName}' not found`, "ACCOUNT_NOT_FOUND")
        const provider = await this.getStorageProviderById(account.ProviderID, params.ContextUser);
                error: this.createErrorResult(`Storage provider not found for account '${accountName}'`, "PROVIDER_NOT_FOUND")
        const driver = await this.initializeDriver(account, provider, params.ContextUser);
        return { driver };
     * Helper to add output parameter
    protected addOutputParam(params: RunActionParams, name: string, value: unknown): void {
     * Helper to create success result
    protected createSuccessResult(data: Record<string, unknown>, params?: RunActionParams): ActionResultSimple {
            Message: JSON.stringify(data, null, 2),
            Params: params?.Params
     * Helper to create error result
    protected createErrorResult(message: string, code: string): ActionResultSimple {
    protected getParamValue(params: RunActionParams, paramName: string): string | undefined {
        return param?.Value as string | undefined;
     * Get string parameter value (guaranteed to be string or undefined)
    protected getStringParam(params: RunActionParams, paramName: string): string | undefined {
        const value = this.getParamValue(params, paramName);
        if (value === undefined || value === null) return undefined;
     * Get string parameter value with default
    protected getStringParamWithDefault(params: RunActionParams, paramName: string, defaultValue: string): string {
        return this.getStringParam(params, paramName) ?? defaultValue;
     * Get boolean parameter value with default
    protected getBooleanParam(params: RunActionParams, paramName: string, defaultValue: boolean = false): boolean {
        return String(value).toLowerCase() === 'true';
     * Get numeric parameter value with default
    protected getNumericParam(params: RunActionParams, paramName: string, defaultValue: number = 0): number {
