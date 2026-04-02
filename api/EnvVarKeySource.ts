 * @fileoverview Environment variable key source provider.
 * This provider retrieves encryption keys from environment variables.
 * It's the simplest and most commonly used key source for development
 * and containerized deployments.
 * 1. Generate a key: `openssl rand -base64 32`
 * 2. Set environment variable: `export MJ_ENCRYPTION_KEY_PII="<base64-key>"`
 * 3. Configure in database with KeyLookupValue = 'MJ_ENCRYPTION_KEY_PII'
 * Keys must be base64-encoded. For AES-256, generate with:
 * openssl rand -base64 32
 * For AES-128:
 * openssl rand -base64 16
 * During rotation, store the new key in a separate variable:
 * export MJ_ENCRYPTION_KEY_PII="<current-key>"
 * export MJ_ENCRYPTION_KEY_PII_NEW="<new-key>"
 * After rotation completes, remove the old key and optionally
 * rename the new key to the original variable name.
 * - Environment variables may be logged or visible to child processes
 * - Consider using secrets managers for production deployments
 * - Ensure proper access controls on the runtime environment
 * - Never commit keys to source control
 * Encryption key source that retrieves keys from environment variables.
 * This is the default and recommended key source for:
 * - Development environments
 * - Docker/Kubernetes deployments with secret injection
 * - Serverless functions with environment configuration
 * Keys are expected to be base64-encoded strings in the environment.
 * The provider decodes them to raw bytes for crypto operations.
 * // The provider is automatically instantiated by ClassFactory
 * // based on database configuration. For manual usage:
 * import { EnvVarKeySource } from '@memberjunction/encryption';
 * const source = new EnvVarKeySource();
 * // Check if key exists
 * if (await source.KeyExists('MJ_ENCRYPTION_KEY_PII')) {
 *   const keyBytes = await source.GetKey('MJ_ENCRYPTION_KEY_PII');
 *   console.log(`Key length: ${keyBytes.length} bytes`);
@RegisterClass(EncryptionKeySourceBase, 'EnvVarKeySource')
export class EnvVarKeySource extends EncryptionKeySourceBase {
        return 'Environment Variable';
     * Validates the source configuration.
     * For environment variables, configuration is always valid as
     * keys are validated at lookup time. This allows the source
     * to be used dynamically for any environment variable.
     * @returns Always returns `true`
        // Environment variable source is always valid
        // Keys are validated at lookup time, not configuration time
     * Checks if an environment variable containing a key exists.
     * @param lookupValue - The environment variable name to check
     * @returns Promise resolving to `true` if the variable is defined
        // Validate the lookup value format (env var name)
        if (!this.isValidEnvVarName(lookupValue)) {
        return process.env[lookupValue] !== undefined;
     * Retrieves key material from an environment variable.
     * The environment variable should contain a base64-encoded key.
     * - `KEY_NAME` for version 1 (default)
     * - `KEY_NAME_V2` for version 2
     * - etc.
     * @param lookupValue - The environment variable name
     * @throws Error if the environment variable is not set
     * // Get current key
     * // Get specific version during rotation
     * // The above looks for MJ_ENCRYPTION_KEY_PII_V2
                'Invalid lookup value: environment variable name is required. ' +
                'Provide the name of the environment variable containing the base64-encoded key.'
        // Validate env var name format to prevent injection
                `Invalid environment variable name: "${lookupValue}". ` +
                'Names must start with a letter or underscore and contain only ' +
        // Build the full environment variable name
        // For versions other than '1', append _V{version}
        const envVarName = this.buildEnvVarName(lookupValue, keyVersion);
        const keyValue = process.env[envVarName];
                `Encryption key not found in environment variable: "${envVarName}". ` +
                'Ensure the environment variable is set with a base64-encoded key value. ' +
                'Generate a key with: openssl rand -base64 32'
                `Encryption key in environment variable "${envVarName}" is empty. ` +
                'The variable must contain a non-empty base64-encoded key value.'
            // Verify we got actual bytes (empty buffer check)
            // Validate it was actually valid base64
            // by checking the round-trip
            const roundTrip = keyBytes.toString('base64');
            const normalized = keyValue.replace(/\s+/g, '');
            if (roundTrip !== normalized && roundTrip + '=' !== normalized && roundTrip + '==' !== normalized) {
                // If they don't match, the input wasn't valid base64
                // (We allow for padding differences)
                throw new Error('Input does not appear to be valid base64 encoding');
                `Invalid base64 encoding for encryption key in "${envVarName}". ` +
     * Builds the full environment variable name with optional version suffix.
     * @param baseName - The base environment variable name
     * @returns The full environment variable name
    private buildEnvVarName(baseName: string, keyVersion?: string): string {
        // For other versions, append _V{version}
        return `${baseName}_V${keyVersion}`;
     * Validates that a string is a valid environment variable name.
     * Valid names:
     * - Start with a letter (A-Z, a-z) or underscore (_)
     * - Contain only letters, numbers, and underscores
     * This prevents injection attacks where malicious lookupValues
     * could be crafted to access unintended variables.
    private isValidEnvVarName(name: string): boolean {
        // Must be non-empty
        // Maximum reasonable length to prevent DoS
        // Standard environment variable naming rules
        // Can contain letters, numbers, underscores
        const envVarPattern = /^[A-Za-z_][A-Za-z0-9_]*$/;
        return envVarPattern.test(name);
