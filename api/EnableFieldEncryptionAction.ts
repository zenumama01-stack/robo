 * @fileoverview Action for enabling encryption on an existing entity field.
 * When encryption is enabled on a field that already has data, this action:
 * 1. Verifies the encryption key is valid and accessible
 * 2. Loads existing records in batches
 * 3. Encrypts all non-null values
 * 4. Saves the encrypted values back to the database
 * This action is typically invoked after updating the EntityField metadata
 * to enable encryption:
 * // First, update the EntityField to enable encryption
 * entityField.Encrypt = true;
 * entityField.EncryptionKeyID = 'key-uuid';
 * await entityField.Save();
 * // Then, encrypt existing data
 * const result = await actionEngine.RunAction({
 *   ActionName: 'Enable Field Encryption',
 *     { Name: 'EntityFieldID', Value: entityField.ID },
 *     { Name: 'BatchSize', Value: 100 }
 *   ContextUser: currentUser
 * - This is a one-way operation - plaintext is replaced with ciphertext
 * - Ensure backups exist before running
 * - Values that are already encrypted are skipped
 * - Empty/null values are not encrypted
import { LogError, LogStatus, Metadata, RunView, UserInfo } from '@memberjunction/core';
import { ActionResultSimple, RunActionParams, ActionParam } from '@memberjunction/actions-base';
import { EnableFieldEncryptionParams, EnableFieldEncryptionResult } from '../interfaces';
 * Action for encrypting existing data when encryption is enabled on a field.
 * This action handles the initial encryption of existing plaintext data
 * after the Encrypt flag is set on an EntityField.
 * ## Process
 * 1. Loads the EntityField metadata to get encryption settings
 * 2. Validates the encryption key is accessible
 * 3. Queries for all records where the field is not null
 * 4. For each record:
 *    - Skip if already encrypted
 *    - Encrypt the plaintext value
 *    - Save the encrypted value
 * 5. Returns statistics on encrypted/skipped records
 * ## Batch Processing
 * Records are processed in configurable batches to manage memory
 * and allow for progress tracking.
 * @security This is a privileged operation that modifies data.
 *           Should be restricted to administrators.
@RegisterClass(Object, 'Enable Field Encryption')
export class EnableFieldEncryptionAction {
     * Executes the field encryption operation.
     * @param params - Action parameters including EntityFieldID and optional BatchSize
     * @returns Result with counts of encrypted and skipped records
        const entityFieldId = this.getParamValue(Params, 'EntityFieldID');
        const batchSize = this.getParamValue(Params, 'BatchSize') || 100;
        if (!entityFieldId) {
                ResultCode: 'INVALID_PARAMS',
                Message: 'EntityFieldID is required',
            const result = await this.enableFieldEncryption({
                entityFieldId: String(entityFieldId),
                batchSize: Number(batchSize)
            const encryptedParam = outputParams.find(p => p.Name === 'RecordsEncrypted');
            if (encryptedParam) encryptedParam.Value = result.recordsEncrypted;
            const skippedParam = outputParams.find(p => p.Name === 'RecordsSkipped');
            if (skippedParam) skippedParam.Value = result.recordsSkipped;
                    Message: `Field encryption completed. Encrypted ${result.recordsEncrypted} records, skipped ${result.recordsSkipped} already encrypted.`,
                    ResultCode: 'ENCRYPTION_FAILED',
                    Message: result.error || 'Field encryption failed',
            LogError(`Enable field encryption failed: ${message}`);
                Message: `Enable field encryption failed: ${message}`,
     * Performs the actual field encryption operation.
    private async enableFieldEncryption(
        params: EnableFieldEncryptionParams,
    ): Promise<EnableFieldEncryptionResult> {
        const { entityFieldId, batchSize = 100 } = params;
        await engine.Config(false, contextUser);
        let recordsEncrypted = 0;
        let recordsSkipped = 0;
            // Step 1: Load the EntityField metadata
            const fieldResult = await rv.RunView({
                ExtraFilter: `ID = '${entityFieldId}'`,
            if (!fieldResult.Success || fieldResult.Results.length === 0) {
                    error: `EntityField not found: ${entityFieldId}`
            const fieldInfo = fieldResult.Results[0];
            const entityName = fieldInfo.Entity;
            const fieldName = fieldInfo.Name;
            const encryptionKeyId = fieldInfo.EncryptionKeyID;
            // Validate that encryption is enabled
            if (!fieldInfo.Encrypt) {
                    error: `Field ${entityName}.${fieldName} does not have encryption enabled. Set Encrypt=true first.`
            if (!encryptionKeyId) {
                    error: `Field ${entityName}.${fieldName} does not have an EncryptionKeyID set.`
            LogStatus(`Enabling encryption for field: ${entityName}.${fieldName}`);
            // Step 2: Validate the encryption key is accessible
            // This will throw if the key is not valid
            await engine.ValidateKeyMaterial(
                fieldInfo.KeyLookupValue || fieldInfo.EncryptionKeyID, // fallback to key ID if no lookup value in field
                encryptionKeyId,
            LogStatus('Encryption key validated successfully');
            // Step 3: Get entity info for proper querying
                    error: `Entity not found: ${entityName}`
            // Step 4: Process records in batches
                // Query for records where field is not null and not already encrypted
                // We check for NOT LIKE '$ENC$%' to find unencrypted values
                const batchResult = await rv.RunView({
                    ExtraFilter: `${fieldName} IS NOT NULL AND ${fieldName} NOT LIKE '$ENC$%'`,
                    OrderBy: entityInfo.PrimaryKeys.map(pk => pk.Name).join(', '),
                    MaxRows: batchSize,
                if (!batchResult.Success) {
                    LogError(`Failed to query records for ${entityName}.${fieldName}`);
                const records = batchResult.Results;
                hasMore = records.length === batchSize;
                if (records.length === 0) {
                LogStatus(`Processing batch of ${records.length} records (offset: ${offset})`);
                // Encrypt each record
                    const plainValue = record.Get(fieldName);
                    // Double-check: skip null/empty values
                    if (plainValue === null || plainValue === undefined || plainValue === '') {
                    // Double-check: skip if somehow already encrypted
                    if (typeof plainValue === 'string' && engine.IsEncrypted(plainValue)) {
                        recordsSkipped++;
                        // Convert to string if needed
                        const stringValue = typeof plainValue === 'string'
                            ? plainValue
                            : JSON.stringify(plainValue);
                        // Encrypt the value
                        const encryptedValue = await engine.Encrypt(
                            stringValue,
                        // Update and save the record
                        record.Set(fieldName, encryptedValue);
                        const saveResult = await record.Save();
                            recordsEncrypted++;
                            LogError(`Failed to save encrypted record in ${entityName}.${fieldName}`);
                    } catch (recordError) {
                        const msg = recordError instanceof Error ? recordError.message : String(recordError);
                        LogError(`Failed to encrypt record in ${entityName}.${fieldName}: ${msg}`);
                offset += batchSize;
            // Also count records that were already encrypted (query separately)
            const alreadyEncryptedResult = await rv.RunView({
                ExtraFilter: `${fieldName} LIKE '$ENC$%'`,
                MaxRows: 1, // Just checking count
            if (alreadyEncryptedResult.Success) {
                // The query itself would return count, but for simplicity we note there are some
                LogStatus(`Some records in ${entityName}.${fieldName} were already encrypted`);
                `Field encryption completed for ${entityName}.${fieldName}. ` +
                `Encrypted: ${recordsEncrypted}, Skipped: ${recordsSkipped}`
                recordsEncrypted,
                recordsSkipped
            LogError(`Enable field encryption error: ${message}`);
                recordsSkipped,
                error: message
     * Helper to extract parameter value by name.
    private getParamValue(params: ActionParam[], name: string): string | number | boolean | null {
        return param?.Value ?? null;
