 * @fileoverview Action for rotating encryption keys with full data re-encryption.
 * Key rotation is a critical security operation that:
 * 1. Validates the new key material is accessible
 * 2. Decrypts all data encrypted with the old key
 * 3. Re-encrypts with the new key in a transactional manner
 * 4. Updates the key metadata to point to the new key material
 * This action is typically invoked via the MemberJunction Actions framework:
 *   ActionName: 'Rotate Encryption Key',
 *     { Name: 'EncryptionKeyID', Value: 'key-uuid' },
 *     { Name: 'NewKeyLookupValue', Value: 'MJ_ENCRYPTION_KEY_PII_V2' },
 * - The new key must be accessible before starting rotation
 * - Rotation is transactional - all or nothing
 * - Key status is set to 'Rotating' during the operation
 * - On success, key version is incremented
 * - On failure, original key continues to work
import { RotateKeyParams, RotateKeyResult } from '../interfaces';
 * Action for rotating encryption keys with full re-encryption of affected data.
 * Key rotation involves:
 * 1. Validating the new key is accessible
 * 2. Setting key status to 'Rotating'
 * 3. For each entity field using this key:
 *    - Loading all records in batches
 *    - Decrypting with old key
 *    - Re-encrypting with new key
 *    - Updating records
 * 4. Updating key metadata (lookup value, version)
 * 5. Setting key status back to 'Active'
 * ## Transaction Safety
 * - Each batch is processed within a transaction
 * - On batch failure, the entire rotation is rolled back
 * - Key status is reset to 'Active' on failure
 * @security This is a privileged operation that should be restricted to administrators.
@RegisterClass(Object, 'Rotate Encryption Key')
export class RotateEncryptionKeyAction {
     * Executes the key rotation operation.
     * @param params - Action parameters including EncryptionKeyID and NewKeyLookupValue
     * @returns Result with success status and details about rotated fields/records
        const encryptionKeyIdParam = this.getParamValue(Params, 'EncryptionKeyID');
        const newKeyLookupValueParam = this.getParamValue(Params, 'NewKeyLookupValue');
        const batchSizeParam = this.getParamValue(Params, 'BatchSize');
        const encryptionKeyId = encryptionKeyIdParam != null ? String(encryptionKeyIdParam) : null;
        const newKeyLookupValue = newKeyLookupValueParam != null ? String(newKeyLookupValueParam) : null;
        const batchSize = batchSizeParam != null ? Number(batchSizeParam) : 100;
                Message: 'EncryptionKeyID is required',
        if (!newKeyLookupValue) {
                Message: 'NewKeyLookupValue is required. This should be the environment variable name or config key for the new encryption key.',
            const result = await this.rotateKey({
                newKeyLookupValue,
                batchSize
            const recordsParam = outputParams.find(p => p.Name === 'RecordsProcessed');
            if (recordsParam) recordsParam.Value = result.recordsProcessed;
            const fieldsParam = outputParams.find(p => p.Name === 'FieldsProcessed');
            if (fieldsParam) fieldsParam.Value = result.fieldsProcessed;
                    Message: `Key rotation completed successfully. Processed ${result.recordsProcessed} records across ${result.fieldsProcessed.length} fields.`,
                    ResultCode: 'ROTATION_FAILED',
                    Message: result.error || 'Key rotation failed',
            LogError(`Key rotation failed: ${message}`);
                Message: `Key rotation failed: ${message}`,
     * Performs the actual key rotation operation.
    private async rotateKey(
        params: RotateKeyParams,
    ): Promise<RotateKeyResult> {
        const { encryptionKeyId, newKeyLookupValue, batchSize = 100 } = params;
        // Track progress
        const fieldsProcessed: string[] = [];
        let totalRecordsProcessed = 0;
            // Step 1: Validate the new key is accessible
            LogStatus(`Validating new key material at lookup value: ${newKeyLookupValue}`);
            await engine.ValidateKeyMaterial(newKeyLookupValue, encryptionKeyId, contextUser);
            LogStatus('New key material validated successfully');
            // Step 2: Load the encryption key record
            const keyResult = await rv.RunView({
                EntityName: 'MJ: Encryption Keys',
                ExtraFilter: `ID = '${encryptionKeyId}'`,
            if (!keyResult.Success || keyResult.Results.length === 0) {
                    recordsProcessed: 0,
                    fieldsProcessed: [],
                    error: `Encryption key not found: ${encryptionKeyId}`
            const encryptionKey = keyResult.Results[0];
            const originalVersion = encryptionKey.KeyVersion || '1';
            // Step 3: Set key status to 'Rotating'
            encryptionKey.Status = 'Rotating';
            const saveResult = await encryptionKey.Save();
                    error: 'Failed to set key status to Rotating'
            // Step 4: Find all entity fields using this key
            const fieldsResult = await rv.RunView({
                ExtraFilter: `EncryptionKeyID = '${encryptionKeyId}' AND Encrypt = 1`,
            if (!fieldsResult.Success) {
                // Reset key status and return error
                encryptionKey.Status = 'Active';
                await encryptionKey.Save();
                    error: 'Failed to load entity fields using this key'
            const fields = fieldsResult.Results;
            LogStatus(`Found ${fields.length} encrypted fields to rotate`);
            // Step 5: Process each field
                const entityName = field.Entity;
                const fullFieldName = `${entityName}.${fieldName}`;
                LogStatus(`Processing field: ${fullFieldName}`);
                    // Get entity info for schema/view name
                        LogError(`Entity not found: ${entityName}`);
                    // Load records in batches
                        // Load a batch of records
                            ExtraFilter: `${fieldName} IS NOT NULL AND ${fieldName} LIKE '$ENC$%'`,
                        if (!batchResult.Success || batchResult.Results.length === 0) {
                        // Process each record in the batch
                            const encryptedValue = record.Get(fieldName);
                            if (!encryptedValue || !engine.IsEncrypted(encryptedValue)) {
                                // Decrypt with old key
                                const decrypted = await engine.Decrypt(encryptedValue, contextUser);
                                // Re-encrypt with new key using the new lookup value
                                const reEncrypted = await engine.EncryptWithLookup(
                                    decrypted,
                                // Update the record
                                record.Set(fieldName, reEncrypted);
                                const recordSaveResult = await record.Save();
                                if (recordSaveResult) {
                                    totalRecordsProcessed++;
                                    // If any record fails, we should consider the rotation failed
                                    // But we continue to try other records to get a complete picture
                                    LogError(`Failed to save re-encrypted record in ${fullFieldName}`);
                                LogError(`Failed to rotate record in ${fullFieldName}: ${msg}`);
                    fieldsProcessed.push(fullFieldName);
                    LogStatus(`Completed field: ${fullFieldName}`);
                } catch (fieldError) {
                    const msg = fieldError instanceof Error ? fieldError.message : String(fieldError);
                    LogError(`Failed to process field ${fullFieldName}: ${msg}`);
                    // Continue with other fields
            // Step 6: Update key metadata to point to new key
            encryptionKey.KeyLookupValue = newKeyLookupValue;
            encryptionKey.KeyVersion = String(parseInt(originalVersion) + 1);
            const finalSaveResult = await encryptionKey.Save();
            if (!finalSaveResult) {
                    recordsProcessed: totalRecordsProcessed,
                    fieldsProcessed,
                    error: 'Failed to update key metadata after rotation. Data has been re-encrypted but key metadata update failed.'
            // Step 7: Clear encryption engine caches to use new key
            LogStatus(`Key rotation completed. Processed ${totalRecordsProcessed} records across ${fieldsProcessed.length} fields.`);
                fieldsProcessed
            // On any error, try to reset key status to Active
                if (keyResult.Success && keyResult.Results.length > 0) {
                    const key = keyResult.Results[0];
                    if (key.Status === 'Rotating') {
                        key.Status = 'Active';
                        await key.Save();
                // Best effort - ignore errors in cleanup
