 * @fileoverview Base encryption engine for MemberJunction field-level encryption.
 * The EncryptionEngineBase provides metadata caching for encryption-related entities
 * (keys, algorithms, sources) and can be used by both client and server code.
 * Server-side implementations should extend this class to add the actual
 * encryption/decryption operations.
 * import { EncryptionEngineBase } from '@memberjunction/core-entities';
 * // Configure the engine (loads metadata)
 * await EncryptionEngineBase.Instance.Config(false, contextUser);
 * // Access cached metadata
 * const key = EncryptionEngineBase.Instance.GetKeyByID(keyId);
 * const algorithm = EncryptionEngineBase.Instance.GetAlgorithmByID(algoId);
import { BaseEngine, BaseEnginePropertyConfig, IMetadataProvider, UserInfo, RegisterForStartup } from "@memberjunction/core";
import { ENCRYPTION_MARKER } from "@memberjunction/global";
    MJEncryptionKeySourceEntity
} from "../generated/entity_subclasses";
 * Configuration for a loaded encryption key, combining key, algorithm, and source data.
 * This is a convenience type that aggregates related encryption configuration.
export interface EncryptionKeyConfiguration {
    /** The encryption key entity */
    key: MJEncryptionKeyEntity;
    /** The encryption algorithm entity */
    algorithm: MJEncryptionAlgorithmEntity;
    /** The key source entity */
    source: MJEncryptionKeySourceEntity;
    /** The marker to use for encrypted values (from key or default) */
 * Base engine class for encryption metadata caching.
 * This class extends BaseEngine to provide automatic caching of encryption-related
 * entities with auto-refresh when those entities are modified. It can be used in
 * both client and server contexts.
 * - Caches all encryption keys, algorithms, and sources
 * - Auto-refreshes when entity data changes
 * - Provides convenient lookup methods
 * - Works in both client and server environments
 * ## For Server-Side Encryption
 * For actual encryption/decryption operations, use or extend the EncryptionEngine
 * class in the @memberjunction/encryption package, which extends this base class.
export class EncryptionEngineBase extends BaseEngine<EncryptionEngineBase> {
     * Cached array of encryption keys loaded from the database.
    private _encryptionKeys: MJEncryptionKeyEntity[] = [];
     * Cached array of encryption algorithms loaded from the database.
    private _encryptionAlgorithms: MJEncryptionAlgorithmEntity[] = [];
     * Cached array of encryption key sources loaded from the database.
    private _encryptionKeySources: MJEncryptionKeySourceEntity[] = [];
     * Gets the singleton instance of the encryption engine base.
     * const engine = EncryptionEngineBase.Instance;
     * const keys = engine.EncryptionKeys;
    public static get Instance(): EncryptionEngineBase {
        return super.getInstance<EncryptionEngineBase>();
     * This method should be called before accessing any cached data. It loads
     * all encryption keys, algorithms, and key sources into memory.
     * // Initial load
     * // Force refresh after external changes
     * await EncryptionEngineBase.Instance.Config(true, contextUser);
                PropertyName: '_encryptionKeys',
                PropertyName: '_encryptionAlgorithms',
                EntityName: 'MJ: Encryption Algorithms',
                PropertyName: '_encryptionKeySources',
                EntityName: 'MJ: Encryption Key Sources',
    // GETTERS FOR CACHED DATA
     * Gets all cached encryption keys.
     * @returns Array of all encryption key entities
    public get EncryptionKeys(): MJEncryptionKeyEntity[] {
        return this._encryptionKeys;
     * Gets only active encryption keys.
     * @returns Array of encryption keys where IsActive is true
    public get ActiveEncryptionKeys(): MJEncryptionKeyEntity[] {
        return this._encryptionKeys.filter(k => k.IsActive);
     * Gets all cached encryption algorithms.
     * @returns Array of all encryption algorithm entities
    public get EncryptionAlgorithms(): MJEncryptionAlgorithmEntity[] {
        return this._encryptionAlgorithms;
     * Gets only active encryption algorithms.
     * @returns Array of encryption algorithms where IsActive is true
    public get ActiveEncryptionAlgorithms(): MJEncryptionAlgorithmEntity[] {
        return this._encryptionAlgorithms.filter(a => a.IsActive);
     * Gets all cached encryption key sources.
     * @returns Array of all encryption key source entities
    public get EncryptionKeySources(): MJEncryptionKeySourceEntity[] {
        return this._encryptionKeySources;
     * Gets only active encryption key sources.
     * @returns Array of encryption key sources where IsActive is true
    public get ActiveEncryptionKeySources(): MJEncryptionKeySourceEntity[] {
        return this._encryptionKeySources.filter(s => s.IsActive);
    // LOOKUP METHODS
     * Gets an encryption key by its ID.
     * @param keyId - The UUID of the encryption key
     * @returns The encryption key entity, or undefined if not found
     * const key = engine.GetKeyByID('550e8400-e29b-41d4-a716-446655440000');
     * if (key) {
     *   console.log(`Key: ${key.Name}, Status: ${key.Status}`);
    public GetKeyByID(keyId: string): MJEncryptionKeyEntity | undefined {
        return this._encryptionKeys.find(k => k.ID === keyId);
     * Gets an encryption key by its name.
     * @param name - The name of the encryption key (case-insensitive)
    public GetKeyByName(name: string): MJEncryptionKeyEntity | undefined {
        const lowerName = name.trim().toLowerCase();
        return this._encryptionKeys.find(k => k.Name.trim().toLowerCase() === lowerName);
     * Gets an encryption algorithm by its ID.
     * @param algorithmId - The UUID of the encryption algorithm
     * @returns The encryption algorithm entity, or undefined if not found
    public GetAlgorithmByID(algorithmId: string): MJEncryptionAlgorithmEntity | undefined {
        return this._encryptionAlgorithms.find(a => a.ID === algorithmId);
     * Gets an encryption algorithm by its name.
     * @param name - The name of the algorithm (e.g., 'AES-256-GCM')
    public GetAlgorithmByName(name: string): MJEncryptionAlgorithmEntity | undefined {
        return this._encryptionAlgorithms.find(a => a.Name.trim().toLowerCase() === lowerName);
     * Gets an encryption key source by its ID.
     * @param sourceId - The UUID of the key source
     * @returns The encryption key source entity, or undefined if not found
    public GetKeySourceByID(sourceId: string): MJEncryptionKeySourceEntity | undefined {
        return this._encryptionKeySources.find(s => s.ID === sourceId);
     * Gets an encryption key source by its driver class name.
     * @param driverClass - The driver class name (e.g., 'EnvVarKeySource')
    public GetKeySourceByDriverClass(driverClass: string): MJEncryptionKeySourceEntity | undefined {
        const lowerClass = driverClass.trim().toLowerCase();
        return this._encryptionKeySources.find(s => s.DriverClass.trim().toLowerCase() === lowerClass);
    // CONVENIENCE METHODS
     * Gets the full configuration for an encryption key, including its algorithm and source.
     * This method aggregates the key, its associated algorithm, and source into
     * a single configuration object for convenience.
     * @returns The full key configuration, or undefined if key not found
     * @throws Error if the key's algorithm or source cannot be found
     * const config = engine.GetKeyConfiguration(keyId);
     * if (config) {
     *   console.log(`Algorithm: ${config.algorithm.Name}`);
     *   console.log(`Source: ${config.source.DriverClass}`);
     *   console.log(`Marker: ${config.marker}`);
    public GetKeyConfiguration(keyId: string): EncryptionKeyConfiguration | undefined {
        const key = this.GetKeyByID(keyId);
        const algorithm = this.GetAlgorithmByID(key.EncryptionAlgorithmID);
        if (!algorithm) {
                `Encryption algorithm not found for key "${key.Name}": ${key.EncryptionAlgorithmID}. ` +
                'The algorithm may have been deleted.'
        const source = this.GetKeySourceByID(key.EncryptionKeySourceID);
                `Encryption key source not found for key "${key.Name}": ${key.EncryptionKeySourceID}. ` +
                'The key source may have been deleted.'
            algorithm,
            marker: key.Marker || ENCRYPTION_MARKER
     * Validates that a key is usable for encryption operations.
     * Checks that the key, its algorithm, and its source are all active and valid.
     * @param keyId - The UUID of the encryption key to validate
     * @returns Object with isValid boolean and optional error message
     * const result = engine.ValidateKey(keyId);
     * if (!result.isValid) {
     *   console.error(`Key validation failed: ${result.error}`);
    public ValidateKey(keyId: string): { isValid: boolean; error?: string } {
            return { isValid: false, error: `Encryption key not found: ${keyId}` };
            return { isValid: false, error: `Encryption key "${key.Name}" is not active` };
            return { isValid: false, error: `Encryption key "${key.Name}" has expired` };
            return { isValid: false, error: `Algorithm not found for key "${key.Name}"` };
            return { isValid: false, error: `Algorithm "${algorithm.Name}" is not active` };
            return { isValid: false, error: `Key source not found for key "${key.Name}"` };
            return { isValid: false, error: `Key source "${source.Name}" is not active` };
     * Gets the marker prefix for a specific encryption key.
     * @returns The marker string (from key or default ENCRYPTION_MARKER)
    public GetKeyMarker(keyId: string): string {
        return key?.Marker || ENCRYPTION_MARKER;
