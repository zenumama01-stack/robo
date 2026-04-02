 * API Key Authorization Interfaces
// API KEY GENERATION AND MANAGEMENT INTERFACES
 * Result of generating a new API key
export interface GeneratedAPIKey {
    /** The raw API key (show to user once, never store) */
    Raw: string;
    /** SHA-256 hash of the key (store in database) */
    Hash: string;
 * Parameters for creating a new API key
export interface CreateAPIKeyParams {
    /** User ID the key belongs to */
    UserId: string;
    /** Label for identifying the key */
    Label: string;
    /** Optional description */
    /** Optional expiration date */
    ExpiresAt?: Date;
 * Result of creating a new API key
export interface CreateAPIKeyResult {
    /** Whether creation succeeded */
    /** The raw API key (only returned on success, show to user once) */
    RawKey?: string;
    /** The database ID of the created API key */
    APIKeyId?: string;
    /** Error message if creation failed */
    Error?: string;
 * Options for validating an API key
export interface APIKeyValidationOptions {
    /** The raw API key to validate */
    RawKey: string;
     * Optional application ID to check binding.
     * If provided, validation will check if the key is authorized for this specific application.
     * Keys bound to specific apps will fail validation if used with a different app.
     * Keys with no app bindings (global keys) will pass validation for any app.
    ApplicationId?: string;
     * Optional application name (alternative to ApplicationId).
     * Will be looked up to get the ApplicationId.
    ApplicationName?: string;
    /** Optional endpoint for logging */
    Endpoint?: string;
    /** Optional HTTP method for logging */
    Method?: string;
    /** Optional operation name for logging */
    Operation?: string;
    /** Optional status code for logging */
    StatusCode?: number;
    /** Optional response time in ms for logging */
    ResponseTimeMs?: number;
    /** Optional client IP address for logging */
    IPAddress?: string;
    /** Optional client user agent for logging */
    UserAgent?: string;
 * Result of validating an API key
export interface APIKeyValidationResult {
    IsValid: boolean;
    /** The user context if valid */
    User?: import('@memberjunction/core').UserInfo;
    /** The API key ID if valid */
     * The SHA-256 hash of the validated API key.
     * Provided so callers can use it for subsequent Authorize() calls
     * without re-hashing the raw key.
    APIKeyHash?: string;
    /** Error message if invalid */
// AUTHORIZATION INTERFACES
 * Request for authorization evaluation
export interface AuthorizationRequest {
    /** The API key ID (from validated key) */
    APIKeyId: string;
    /** The user ID associated with the API key */
    /** The application ID making the request */
    ApplicationId: string;
    /** The scope path being requested (e.g., 'view:run', 'agent:execute') */
    ScopePath: string;
    /** The specific resource being accessed (e.g., entity name, agent name) */
    Resource: string;
    /** Optional additional context */
    Context?: Record<string, unknown>;
 * Result of authorization evaluation
export interface AuthorizationResult {
    /** Whether access is allowed */
    Allowed: boolean;
    /** Human-readable reason for the decision */
    Reason: string;
    /** The app-level rule that matched (if any) */
    MatchedAppRule?: ScopeRuleMatch;
    /** The key-level rule that matched (if any) */
    MatchedKeyRule?: ScopeRuleMatch;
    /** All rules evaluated during the check */
    EvaluatedRules: EvaluatedRule[];
 * A matched scope rule
export interface ScopeRuleMatch {
    /** Rule ID */
    /** Scope ID */
    ScopeId: string;
    /** Scope full path */
    /** Resource pattern that matched */
    Pattern: string | null;
    /** Pattern type */
    PatternType: 'Include' | 'Exclude';
    /** Whether this is a deny rule */
    IsDeny: boolean;
    /** Rule priority */
 * Details of a rule evaluation
export interface EvaluatedRule {
    /** Which level this rule came from */
    Level: 'application' | 'key';
    /** The rule that was evaluated */
    Rule: ScopeRuleMatch;
    /** Whether the pattern matched the resource */
    Matched: boolean;
    /** The specific pattern that matched (from comma-separated list) */
    PatternMatched: string | null;
    /** Result of this rule evaluation */
    Result: 'Allowed' | 'Denied' | 'NoMatch';
 * Scope rule from database (app or key level)
export interface ScopeRule {
    ScopeID: string;
    /** Resource pattern (glob) */
    ResourcePattern: string | null;
    /** Is this a deny rule */
    /** Priority (higher = evaluated first) */
 * Application scope ceiling rule
export interface ApplicationScopeRule extends ScopeRule {
    /** Application ID */
    ApplicationID: string;
 * API key scope rule
export interface KeyScopeRule extends ScopeRule {
    /** API Key ID */
    APIKeyID: string;
// USAGE LOGGING INTERFACES
 * Usage log entry for audit trail
export interface UsageLogEntry {
    ApplicationId: string | null;
    /** Endpoint accessed */
    Endpoint: string;
    /** Operation performed */
    Operation: string | null;
    /** HTTP method */
    Method: string;
    /** HTTP status code */
    StatusCode: number;
    /** Response time in ms */
    ResponseTimeMs: number | null;
    /** Client IP address */
    IPAddress: string | null;
    /** Client user agent */
    UserAgent: string | null;
    /** Resource that was requested */
    RequestedResource: string | null;
    /** Scopes that were evaluated */
    ScopesEvaluated: EvaluatedRule[];
    /** Authorization result */
    AuthorizationResult: 'Allowed' | 'Denied' | 'NoScopesRequired';
    /** Reason for denial (if denied) */
    DeniedReason: string | null;
// CONFIGURATION INTERFACES
 * Configuration for the API Key Engine
export interface APIKeyEngineConfig {
    /** Whether scope enforcement is enabled (default: true) */
    enforcementEnabled?: boolean;
    /** Whether to log all requests (default: true) */
    loggingEnabled?: boolean;
    /** Default behavior when key has no scopes (default: 'allow') */
    defaultBehaviorNoScopes?: 'allow' | 'deny';
    /** Cache TTL in milliseconds for scope rules (default: 60000) */
    scopeCacheTTLMs?: number;
 * @fileoverview Type definitions and interfaces for the MemberJunction encryption system.
 * This module defines the core types used throughout the encryption package:
 * - Configuration interfaces for key sources
 * - Parsed encrypted value structure
 * - Key configuration for runtime operations
 * @see {@link EncryptionEngine} for the main encryption/decryption API
 * @see {@link EncryptionKeySourceBase} for implementing custom key sources
 * Configuration passed to encryption key source providers during instantiation.
 * Key sources use this configuration to understand how to retrieve key material.
 * The specific properties used depend on the key source implementation.
 * // Environment variable source config
 * const envConfig: EncryptionKeySourceConfig = {
 *   lookupValue: 'MJ_ENCRYPTION_KEY_PII'
 * // Config file source config
 * const fileConfig: EncryptionKeySourceConfig = {
 *   lookupValue: 'pii_master_key'
export interface EncryptionKeySourceConfig {
     * The identifier used to look up the key from the source.
     * - For env vars: the environment variable name
     * - For config files: the key name in the encryptionKeys section
     * - For vaults: the secret path
    lookupValue?: string;
     * Optional additional configuration specific to the key source.
     * Providers can cast this to their specific config type.
    additionalConfig?: Record<string, unknown>;
 * Represents the parsed components of an encrypted value string.
 * Encrypted values follow the format:
 * This structure allows the encryption engine to:
 * 1. Identify which key was used for encryption
 * 2. Determine the algorithm for decryption
 * 3. Extract the IV and ciphertext for the crypto operation
 * 4. Verify authenticity with the auth tag (for AEAD algorithms)
 * console.log(parts.keyId); // UUID of the encryption key
 * console.log(parts.algorithm); // 'AES-256-GCM'
export interface EncryptedValueParts {
     * The encryption marker prefix (always '$ENC$').
     * Used for quick detection of encrypted values.
    marker: string;
     * The UUID of the encryption key used.
     * References the EncryptionKey entity in the database.
     * The algorithm name used for encryption.
     * Matches the Name field in the EncryptionAlgorithm entity.
     * @example 'AES-256-GCM', 'AES-256-CBC'
    algorithm: string;
     * Base64-encoded initialization vector.
     * Randomly generated for each encryption operation.
    iv: string;
     * Base64-encoded encrypted data.
    ciphertext: string;
     * Base64-encoded authentication tag for AEAD algorithms.
     * Only present for algorithms like AES-GCM that provide authentication.
     * Undefined for non-AEAD algorithms like AES-CBC.
    authTag?: string;
 * Complete key configuration loaded from the database.
 * This structure contains everything needed to perform encryption
 * or decryption operations, cached for performance.
 * @internal Used by EncryptionEngine for key management
export interface KeyConfiguration {
    /** The encryption key's unique identifier (UUID) */
     * Current version of the key for this configuration.
     * Incremented during key rotation operations.
    keyVersion: string;
     * Prefix marker for encrypted values.
     * Defaults to '$ENC$' but can be customized per key.
    /** Algorithm configuration */
        /** Display name of the algorithm (e.g., 'AES-256-GCM') */
        /** Node.js crypto module algorithm identifier (e.g., 'aes-256-gcm') */
        nodeCryptoName: string;
        /** Required key length in bits (e.g., 256 for AES-256) */
        keyLengthBits: number;
        /** Required IV length in bytes (e.g., 12 for GCM, 16 for CBC) */
        ivLengthBytes: number;
         * Whether the algorithm provides Authenticated Encryption with Associated Data.
         * AEAD algorithms (like AES-GCM) provide both confidentiality and integrity.
        isAEAD: boolean;
    /** Key source configuration for retrieving key material */
         * The registered class name of the key source provider.
         * Used with ClassFactory to instantiate the provider.
         * The lookup value passed to the key source.
         * Interpretation depends on the source type.
        lookupValue: string;
 * Result structure returned by key rotation operations.
 * @see {@link RotateEncryptionKeyAction} for the rotation action
export interface RotateKeyResult {
    /** Whether the rotation completed successfully */
    /** Total number of records that were re-encrypted */
    recordsProcessed: number;
     * List of fields that were processed.
     * Format: 'EntityName.FieldName'
    fieldsProcessed: string[];
    /** Error message if rotation failed */
 * Parameters for key rotation operations.
export interface RotateKeyParams {
    /** UUID of the encryption key to rotate */
    encryptionKeyId: string;
     * Lookup value for the new key material.
     * The new key must be accessible via the key source before rotation.
    newKeyLookupValue: string;
     * Number of records to process per batch.
     * Larger batches are faster but use more memory.
 * Result structure for field encryption operations.
 * @see {@link EnableFieldEncryptionAction}
export interface EnableFieldEncryptionResult {
    /** Whether the operation completed successfully */
    /** Number of records that were encrypted */
    recordsEncrypted: number;
    /** Number of records that were already encrypted (skipped) */
    recordsSkipped: number;
    /** Error message if the operation failed */
 * Parameters for enabling encryption on existing data.
export interface EnableFieldEncryptionParams {
    /** UUID of the EntityField to encrypt */
    entityFieldId: string;
import { BaseEntity } from "./baseEntity";
import { EntityDependency, EntityInfo,  RecordChange, RecordDependency, RecordMergeRequest, RecordMergeResult, EntityDocumentTypeInfo } from "./entityInfo";
import { ApplicationInfo } from "./applicationInfo";
import { RunViewParams } from "../views/runView";
import { AuditLogTypeInfo, AuthorizationInfo, RoleInfo, RowLevelSecurityFilterInfo, UserInfo } from "./securityInfo";
import { TransactionGroupBase } from "./transactionGroup";
import { RunReportParams } from "./runReport";
import { QueryCategoryInfo, QueryFieldInfo, QueryInfo, QueryPermissionInfo, QueryEntityInfo, QueryParameterInfo } from "./queryInfo";
import { RunQueryParams } from "./runQuery";
import { LibraryInfo } from "./libraryInfo";
import { CompositeKey } from "./compositeKey";
import { ExplorerNavigationItem } from "./explorerNavigationItem";
 * Base configuration class for data providers.
 * Contains schema inclusion/exclusion rules and configuration data.
 * Used to configure which database schemas should be included or excluded from metadata scanning.
export class ProviderConfigDataBase<D = any> {
    private _includeSchemas: string[] = [];
    private _excludeSchemas: string[] = [];
    private _MJCoreSchemaName: string = '__mj';
    private _data: D;
    private _ignoreExistingMetadata: boolean = false;
    public get Data(): D {
    public get IncludeSchemas(): string[] {
        return this._includeSchemas;
    public get MJCoreSchemaName(): string {
        return this._MJCoreSchemaName;
    public get ExcludeSchemas(): string[] {
        return this._excludeSchemas;
    public get IgnoreExistingMetadata(): boolean {
        return this._ignoreExistingMetadata;
     * Constructor for ProviderConfigDataBase
     * @param MJCoreSchemaName 
     * @param includeSchemas 
     * @param ignoreExistingMetadata if set to true, even if a global provider is already registered for the Metadata static Provider member, this class will still load up fresh metadata for itself. By default this is off and a class will use existing loaded metadata if it exists
    constructor(data: D, MJCoreSchemaName: string = '__mj', includeSchemas?: string[], excludeSchemas?: string[], ignoreExistingMetadata: boolean = true) {
        this._data = data;
        this._MJCoreSchemaName = MJCoreSchemaName;
        if (includeSchemas)
            this._includeSchemas = includeSchemas;
        if (excludeSchemas)
            this._excludeSchemas = excludeSchemas;
        this._ignoreExistingMetadata = ignoreExistingMetadata;
 * Information about metadata timestamps and record counts.
 * Used to track when metadata was last updated and how many records exist.
 * Helps determine if local metadata cache is up-to-date with the server.
export class MetadataInfo {
    ID: string
    Type: string
    UpdatedAt: Date
    RowCount: number
export const ProviderType = {
    Database: 'Database',
    Network: 'Network',
export type ProviderType = typeof ProviderType[keyof typeof ProviderType];
 * Represents a potential duplicate record with its probability score.
 * Extends CompositeKey to support multi-field primary keys.
 * Used in duplicate detection and record merging operations.
export class PotentialDuplicate extends CompositeKey {
    ProbabilityScore: number;
 * Request parameters for finding potential duplicate records.
 * Supports various matching strategies including list-based and document-based comparisons.
 * Can use either a pre-defined list or entity document for duplicate detection.
export class PotentialDuplicateRequest {
    * The ID of the entity the record belongs to
    * The ID of the List entity to use
    ListID: string;
     * The Primary Key values of each record
     * we're checking for duplicates
    RecordIDs: CompositeKey[]; 
    EntityDocumentID?: string;
    * The minimum score in order to consider a record a potential duplicate
    ProbabilityScore?: number;
    Options?: any;
 * Result of a potential duplicate search for a single record.
 * Contains the record being checked and all potential duplicates found.
 * Includes match details and duplicate run information for tracking.
export class PotentialDuplicateResult {
        this.RecordCompositeKey = new CompositeKey();
        this.Duplicates = [];
        this.DuplicateRunDetailMatchRecordIDs = [];
    RecordCompositeKey: CompositeKey;
    Duplicates: PotentialDuplicate[];
    DuplicateRunDetailMatchRecordIDs: string[];
 * Response wrapper for potential duplicate searches.
 * Includes status information and array of results.
 * Status can be 'Inprogress' for asynchronous operations, 'Success' when complete, or 'Error' on failure.
export class PotentialDuplicateResponse {
    Status: 'Inprogress' | 'Success' | 'Error';
    PotentialDuplicateResult: PotentialDuplicateResult[];
 * Interface for entity data providers.
 * Defines core CRUD operations and record change tracking.
 * Implementations handle database-specific operations for entity persistence.
export interface IEntityDataProvider {
    Config(configData: ProviderConfigDataBase): Promise<boolean>
    Load(entity: BaseEntity, CompositeKey: CompositeKey, EntityRelationshipsToLoad: string[], user: UserInfo) : Promise<{}>
    Save(entity: BaseEntity, user: UserInfo, options: EntitySaveOptions) : Promise<{}>
    Delete(entity: BaseEntity, options: EntityDeleteOptions, user: UserInfo) : Promise<boolean>
    GetRecordChanges(entityName: string, CompositeKey: CompositeKey): Promise<RecordChange[]>
     * Discovers which IS-A child entity, if any, has a record with the given primary key.
     * Used by BaseEntity.InitializeChildEntity() after loading a record to find the
     * most-derived child type. Implementations should execute a single UNION ALL query
     * across all child entity tables for efficiency.
     * @param entityInfo The parent entity's EntityInfo (to find its child entity types)
    FindISAChildEntity?(entityInfo: EntityInfo, recordPKValue: string, contextUser?: UserInfo): Promise<{ ChildEntityName: string } | null>;
     * Discovers ALL IS-A child entities that have records with the given primary key.
     * Used for overlapping subtype parents (AllowMultipleSubtypes = true) where multiple
     * children can coexist. Same UNION ALL query as FindISAChildEntity, but returns all matches.
    FindISAChildEntities?(entityInfo: EntityInfo, recordPKValue: string, contextUser?: UserInfo): Promise<{ ChildEntityName: string }[]>;
     * Begin an independent provider-level transaction for IS-A chain orchestration.
     * Returns a provider-specific transaction object (e.g., sql.Transaction for SQLServer).
     * Separate from the provider's internal transaction management (TransactionGroup system).
     * Optional — client-side providers (GraphQL) do not implement this.
    BeginISATransaction?(): Promise<unknown>;
     * Commit an IS-A chain transaction.
     * @param txn The transaction object returned from BeginISATransaction()
    CommitISATransaction?(txn: unknown): Promise<void>;
     * Rollback an IS-A chain transaction.
    RollbackISATransaction?(txn: unknown): Promise<void>;
 * Save options used when saving an entity record.
 * Provides fine-grained control over the save operation including validation,
 * action execution, and conflict detection.
export class EntitySaveOptions {
     * If set to true, the record will be saved to the database even if nothing is detected to be "dirty" or changed since the prior load.
    IgnoreDirtyState: boolean = false;
     * If set to true, an AI actions associated with the entity will be skipped during the save operation
    SkipEntityAIActions?: boolean = false;
     * If set to true, any Entity Actions associated with invocation types of Create or Update will be skipped during the save operation
    SkipEntityActions?: boolean = false;
     * When set to true, the save operation will BYPASS Validate() and the actual process of saving changes to the database but WILL invoke any associated actions (AI Actions, Entity Actions, etc...)
     * Subclasses can also override the Save() method to provide custom logic that will be invoked when ReplayOnly is set to true
    ReplayOnly?: boolean = false;
     * Setting this to true means that the system will not look for inconsistency between the state of the record at the time it was loaded and the current database version of the record. This is normally on
     * because it is a good way to prevent overwriting changes made by other users that happened after your version of the record was loaded. However, in some cases, you may want to skip this check, such as when you are
     * updating a record that you know has not been changed by anyone else since you loaded it. In that case, you can set this property to true to skip the check which will be more efficient.
     * * IMPORTANT: This is only used for client-side providers. On server-side providers, this check never occurs because server side operations are as up to date as this check would yield. 
    SkipOldValuesCheck?: boolean = false;
     * When set to true, the entity will skip the asynchronous ValidateAsync() method during save.
     * This is an advanced setting and should only be used when you are sure the async validation is not needed.
     * The default behavior is to run the async validation and the default value is undefined.
     * Also, you can set an Entity level default in a BaseEntity subclass by overriding the DefaultSkipAsyncValidation() getter property.
     * @see BaseEntity.DefaultSkipAsyncValidation
    SkipAsyncValidation?: boolean = undefined;
     * When true, this entity is being saved as part of an IS-A parent chain
     * initiated by a child entity. Provider behavior:
     * - GraphQLDataProvider: full ORM pipeline runs, skip network call
     * - SQLServerDataProvider: real save using shared ProviderTransaction
    IsParentEntitySave?: boolean = false;
     * The entity name of the child that initiated this parent save in an IS-A chain.
     * Used by server-side providers to skip the active branch when propagating
     * Record Change entries to sibling branches of overlapping parents.
     * Only set when IsParentEntitySave is true.
    ISAActiveChildEntityName?: string;
 * Options used when deleting an entity record.
 * Controls whether associated actions and AI operations should be executed
 * during the deletion process.
export class EntityDeleteOptions {
     * If set to true, an AI actions associated with the entity will be skipped during the delete operation
     * If set to true, any Entity Actions associated with invocation types of Delete will be skipped during the delete operation
     * When set to true, the save operation will BYPASS Validate() and the actual process of deleting the record from the database but WILL invoke any associated actions (AI Actions, Entity Actions, etc...)
     * Subclasses can also override the Delete() method to provide custom logic that will be invoked when ReplayOnly is set to true
     * When true, this entity is being deleted as part of an IS-A parent chain
     * initiated by a child entity. The child deletes itself first (FK constraint),
     * then cascades deletion to its parent.
    IsParentEntityDelete?: boolean = false;
 * Options used when merging entity records.
 * Controls transaction isolation and other merge-specific behaviors.
export class EntityMergeOptions {
    // nothing here yet, define for future use
 * Input parameters for retrieving entity record names.
 * Used for batch operations to get display names for multiple records.
export class EntityRecordNameInput  {
 * Result of an entity record name lookup operation.
 * Contains the display name and status information for the requested record.
export class EntityRecordNameResult  {
    RecordName?: string;
 * Interface for local storage providers.
 * Abstracts storage operations to support different storage backends
 * (e.g., browser localStorage, IndexedDB, file system).
 * Implementations should handle the optional category parameter as follows:
 * - **IndexedDB**: Create separate object stores per category (e.g., `mj:RunViewCache`)
 * - **localStorage**: Prefix keys with `[mj]:[category]:[key]`
 * - **Memory**: Use nested Map structure (Map<category, Map<key, value>>)
 * When category is not provided, use a default category (e.g., 'default' or 'general').
export interface ILocalStorageProvider {
     * Retrieves an item from storage.
     * @param key - The key to retrieve
     * @param category - Optional category for key isolation (e.g., 'RunViewCache', 'Metadata')
    GetItem(key: string, category?: string): Promise<string | null>;
     * Stores an item in storage.
     * @param key - The key to store under
     * @param value - The value to store
     * @param category - Optional category for key isolation
    SetItem(key: string, value: string, category?: string): Promise<void>;
     * Removes an item from storage.
     * @param key - The key to remove
    Remove(key: string, category?: string): Promise<void>;
     * Clears all items in a specific category.
     * If no category is specified, clears the default category only.
     * @param category - The category to clear
    ClearCategory?(category: string): Promise<void>;
     * Gets all keys in a specific category.
     * @param category - The category to list keys from
    GetCategoryKeys?(category: string): Promise<string[]>;
 * Provider interface for filesystem operations.
 * Enables environment-specific file I/O without requiring `eval("require('fs')")` or other
 * bundler-unfriendly patterns. Server-side providers (e.g. SQLServerDataProvider) supply a
 * Node.js implementation; browser-side providers return null from IMetadataProvider.FileSystemProvider.
 * Follows the same pattern as ILocalStorageProvider — defined in core, implemented per environment.
export interface IFileSystemProvider {
     * Appends content to a file, creating it if it doesn't exist.
     * @param filePath - Full path to the file
     * @param content - Content to append
    AppendToFile(filePath: string, content: string): Promise<void>;
     * Writes content to a file, overwriting if it exists.
     * @param content - Content to write
    WriteFile(filePath: string, content: string): Promise<void>;
     * Reads content from a file.
     * @returns File content or null if file doesn't exist
    ReadFile(filePath: string): Promise<string | null>;
     * Checks if a file exists.
    FileExists(filePath: string): Promise<boolean>;
 * Core interface for metadata providers in MemberJunction.
 * Provides access to all system metadata including entities, applications, security, and queries.
 * This is the primary interface for accessing MemberJunction's metadata layer.
 * Implementations typically cache metadata locally for performance.
export interface IMetadataProvider {
    get ProviderType(): ProviderType
    get DatabaseConnection(): any
    Config(configData: ProviderConfigDataBase, providerToUse?: IMetadataProvider): Promise<boolean>
    get Entities(): EntityInfo[]
    get Applications(): ApplicationInfo[]
    get CurrentUser(): UserInfo
    get Roles(): RoleInfo[]
    get RowLevelSecurityFilters(): RowLevelSecurityFilterInfo[]
    get AuditLogTypes(): AuditLogTypeInfo[]
    get Authorizations(): AuthorizationInfo[]
    get Queries(): QueryInfo[]
    get QueryFields(): QueryFieldInfo[]
    get QueryCategories(): QueryCategoryInfo[]
    get QueryPermissions(): QueryPermissionInfo[]
    get QueryEntities(): QueryEntityInfo[]
    get QueryParameters(): QueryParameterInfo[]
    get Libraries(): LibraryInfo[]
    get VisibleExplorerNavigationItems(): ExplorerNavigationItem[]
    get AllExplorerNavigationItems(): ExplorerNavigationItem[]
    get LatestRemoteMetadata(): MetadataInfo[]
    get LatestLocalMetadata(): MetadataInfo[]
    get AllMetadata(): AllMetadata
    LocalMetadataObsolete(type?: string): boolean
     * Creates a new instance of a BaseEntity subclass for the specified entity and calls NewRecord() to initialize it.
     * The UUID will be automatically generated for non-auto-increment uniqueidentifier primary keys.
     * @param entityName - The name of the entity to create (e.g., "Users", "Customers")
     * @param contextUser - Optional context user for permissions (mainly used server-side)
     * @returns Promise resolving to the newly created entity instance with NewRecord() already called
    GetEntityObject<T extends BaseEntity>(entityName: string, contextUser?: UserInfo): Promise<T>
     * Creates a new instance of a BaseEntity subclass and loads an existing record using the provided key.
     * @param loadKey - CompositeKey containing the primary key value(s) to load
     * @returns Promise resolving to the entity instance with the specified record loaded
     * @throws Error if the record cannot be found or loaded
    GetEntityObject<T extends BaseEntity>(entityName: string, loadKey: CompositeKey, contextUser?: UserInfo): Promise<T>
     * Returns a list of dependencies - records that are linked to the specified Entity/RecordID combination. A dependency is as defined by the relationships in the database. The MemberJunction metadata that is used
     * @param CompositeKey the compositeKey for the record to check
    GetRecordDependencies(entityName: string, CompositeKey: CompositeKey): Promise<RecordDependency[]>  
     * Returns a list of record IDs that are possible duplicates of the specified record. 
     * @param params Object containing many properties used in fetching records and determining which ones to return
    GetRecordDuplicates(params: PotentialDuplicateRequest, contextUser?: UserInfo): Promise<PotentialDuplicateResponse>
     * Returns a list of entity dependencies, basically metadata that tells you the links to this entity from all other entities.
    GetEntityDependencies(entityName: string): Promise<EntityDependency[]> 
     * This method will merge two or more records based on the request provided. The RecordMergeRequest type you pass in specifies the record that will survive the merge, the records to merge into the surviving record, and an optional field map that can update values in the surviving record, if desired. The process followed is:
     * 1. A transaction is started
     * 2. The surviving record is loaded and fields are updated from the field map, if provided, and the record is saved. If a FieldMap not provided within the request object, this step is skipped.
     * 3. For each of the records that will be merged INTO the surviving record, we call the GetEntityDependencies() method and get a list of all other records in the database are linked to the record to be deleted. We then go through each of those dependencies and update the link to point to the SurvivingRecordID and save the record.
     * 4. The record to be deleted is then deleted.
     * 5. The transaction is committed if all of the above steps are succesful, otherwise it is rolled back.
     * The return value from this method contains detailed information about the execution of the process. In addition, all attempted merges are logged in the RecordMergeLog and RecordMergeDeletionLog tables.
     * @param request 
    MergeRecords(request: RecordMergeRequest, contextUser?: UserInfo, options?: EntityMergeOptions): Promise<RecordMergeResult> 
     * Returns the Name of the specific recordId for a given entityName. This is done by
     * looking for the IsNameField within the EntityFields collection for a given entity.
     * If no IsNameField is found, but a field called "Name" exists, that value is returned. Otherwise null returned
     * @param CompositeKey
     * @param contextUser - optional user context for permissions
     * @param forceRefresh - if true, bypasses cache and fetches fresh from database
     * @returns the name of the record
    GetEntityRecordName(entityName: string, compositeKey: CompositeKey, contextUser?: UserInfo, forceRefresh?: boolean): Promise<string>
     * Returns one or more record names using the same logic as GetEntityRecordName, but for multiple records at once - more efficient to use this method if you need to get multiple record names at once
     * @param info
     * @returns an array of EntityRecordNameResult objects
    GetEntityRecordNames(info: EntityRecordNameInput[], contextUser?: UserInfo, forceRefresh?: boolean): Promise<EntityRecordNameResult[]>
     * Asynchronous lookup of a cached entity record name. Returns the cached name if available, or undefined if not cached.
     * Use this for synchronous contexts (like template rendering) where you can't await GetEntityRecordName().
     * @param compositeKey - The primary key value(s) for the record
     * @param loadIfNeeded - If set to true, will load from database if not already cached
     * @returns The cached display name, or undefined if not in cache
    GetCachedRecordName(entityName: string, compositeKey: CompositeKey, loadIfNeeded?: boolean): Promise<string | undefined>;
     * Stores a record name in the cache for later synchronous retrieval via GetCachedRecordName().
     * Called automatically by BaseEntity after Load(), LoadFromData(), and Save() operations.
     * @param recordName - The display name to cache
    SetCachedRecordName(entityName: string, compositeKey: CompositeKey, recordName: string): void
    GetRecordFavoriteStatus(userId: string, entityName: string, CompositeKey: CompositeKey, contextUser?: UserInfo): Promise<boolean>
    SetRecordFavoriteStatus(userId: string, entityName: string, CompositeKey: CompositeKey, isFavorite: boolean, contextUser: UserInfo): Promise<void>
    CreateTransactionGroup(): Promise<TransactionGroupBase>
    Refresh(providerToUse?: IMetadataProvider): Promise<boolean>
    RefreshIfNeeded(providerToUse?: IMetadataProvider): Promise<boolean>
    CheckToSeeIfRefreshNeeded(providerToUse?: IMetadataProvider): Promise<boolean>
    get LocalStorageProvider(): ILocalStorageProvider
     * Returns the filesystem provider for the current environment, or null if filesystem
     * operations are not available (e.g. in browser environments).
     * Server-side providers should return a NodeFileSystemProvider instance.
    get FileSystemProvider(): IFileSystemProvider | null
    RefreshRemoteMetadataTimestamps(providerToUse?: IMetadataProvider): Promise<boolean>
    SaveLocalMetadataToStorage(): Promise<void>
    RemoveLocalMetadataFromStorage(): Promise<void>
     * Always retrieves data from the server - this method does NOT check cache. To use cached local values if available, call GetAndCacheDatasetByName() instead
    GetDatasetByName(datasetName: string, itemFilters?: DatasetItemFilterType[], contextUser?: UserInfo, providerToUse?: IMetadataProvider): Promise<DatasetResultType>;
     * Retrieves the date status information for a dataset and all its items from the server. This method will match the datasetName and itemFilters to the server's dataset and item filters to determine a match
    GetDatasetStatusByName(datasetName: string, itemFilters?: DatasetItemFilterType[], contextUser?: UserInfo, providerToUse?: IMetadataProvider): Promise<DatasetStatusResultType>;
     * Gets a database by name, if required, and caches it in a format available to the client (e.g. IndexedDB, LocalStorage, File, etc). The cache method is Provider specific
     * If itemFilters are provided, the combination of datasetName and the filters are used to determine a match in the cache
    GetAndCacheDatasetByName(datasetName: string, itemFilters?: DatasetItemFilterType[], contextUser?: UserInfo): Promise<DatasetResultType>  
     * Returns the timestamp of the local cached version of a given datasetName or null if there is no local cache for the 
     * specified dataset
     * @param datasetName the name of the dataset to check
     * @param itemFilters optional filters to apply to the dataset
    GetLocalDatasetTimestamp(datasetName: string, itemFilters?: DatasetItemFilterType[]): Promise<Date>
     * This routine checks to see if the local cache version of a given datasetName/itemFilters combination is up to date with the server or not
    IsDatasetCacheUpToDate(datasetName: string, itemFilters?: DatasetItemFilterType[]): Promise<boolean> 
     * This routine gets the local cached version of a given datasetName/itemFilters combination, it does NOT check the server status first. 
    GetCachedDataset(datasetName: string, itemFilters?: DatasetItemFilterType[]): Promise<DatasetResultType> 
     * Stores a dataset in the local cache. If itemFilters are provided, the combination of datasetName and the filters are used to build a key and determine a match in the cache
     * @param dataset 
    CacheDataset(datasetName: string, itemFilters: DatasetItemFilterType[], dataset: DatasetResultType): Promise<void> 
     * Determines if a given datasetName/itemFilters combination is cached locally or not
    IsDatasetCached(datasetName: string, itemFilters?: DatasetItemFilterType[]): Promise<boolean> 
     * Creates a key for the given datasetName and itemFilters combination
    GetDatasetCacheKey(datasetName: string, itemFilters?: DatasetItemFilterType[]): string 
     * If the specified datasetName is cached, this method will clear the cache. If itemFilters are provided, the combination of datasetName and the filters are used to determine a match in the cache
    ClearDatasetCache(datasetName: string, itemFilters?: DatasetItemFilterType[]): Promise<void> 
     * Provides access the configuration object that was initially provided to configure the provider
    get ConfigData(): ProviderConfigDataBase
 * Single aggregate result value - can be a number, string, Date, boolean, or null
export type AggregateValue = number | string | Date | boolean | null;
 * Result of a single aggregate expression calculation
export interface AggregateResult {
    /** The expression that was calculated */
    expression: string;
    /** The alias (or expression if no alias provided) */
    /** The calculated value */
    value: AggregateValue;
    /** If calculation failed, the error message */
 * Result of a RunView() execution.
 * Contains the query results along with execution metadata like timing,
 * row counts, and error information.
export type RunViewResult<T = any> = {
     * Was the view run successful or not
     * The array of records returned by the view, only valid if Success is true
    Results: Array<T>;
     * The newly created UserViews.ID value - only provided if RunViewParams.SaveViewResults=true
     * Number of rows returned in the Results[] array
     * Total number of rows that match the view criteria, not just the number returned in the Results[] array
     * This number will only be different when the view is configured to have a UserViewMaxRows value and the criteria of the view in question
     * has more than that # of rows. Otherwise it will be the same value as RowCount.
     * Time elapsed in executing the view (in milliseconds)
     * If Success is false, this will contain a message describing the error condition.
     * Results of aggregate calculations, in same order as input Aggregates array.
     * Only present if Aggregates were requested in RunViewParams.
    AggregateResults?: AggregateResult[];
     * Execution time for aggregate query specifically (in milliseconds).
     * Only present if Aggregates were requested.
    AggregateExecutionTime?: number;
 * Interface for providers that execute views.
 * Supports parameterized view execution with filtering and pagination.
 * Views are the primary way to query entity data in MemberJunction.
export interface IRunViewProvider {
    RunView<T = any>(params: RunViewParams, contextUser?: UserInfo): Promise<RunViewResult<T>>
    RunViews<T = any>(params: RunViewParams[], contextUser?: UserInfo): Promise<RunViewResult<T>[]>
     * Executes multiple RunView requests with smart cache checking.
     * For each view with cacheStatus provided, the server checks if the cached data is still current
     * before executing the full query. This reduces unnecessary data transfer when cached data is valid.
     * @param params Array of view parameters with optional cache status for each
     * @param contextUser Optional user context for permissions
     * @returns Response containing status and fresh data only for stale caches
    RunViewsWithCacheCheck?<T = unknown>(params: RunViewWithCacheCheckParams[], contextUser?: UserInfo): Promise<RunViewsWithCacheCheckResponse<T>>
// RUNVIEW SMART CACHE CHECK TYPES
 * Client-side cache status information for a single RunView request.
 * Sent to the server to determine if cached data is still current.
export type RunViewCacheStatus = {
     * The maximum __mj_UpdatedAt value from the client's cached results.
     * Used to detect if any records have been added or updated.
    maxUpdatedAt: string;
     * The number of rows in the client's cached results.
     * Used to detect if any records have been deleted.
 * Parameters for a single RunView request with optional cache status.
 * When cacheStatus is provided, the server will check if the cache is current
 * before executing the full query.
export type RunViewWithCacheCheckParams = {
     * The standard RunView parameters
    params: RunViewParams;
     * Optional cache status from the client. If provided, the server will
     * check if the cached data is still current before returning results.
     * If not provided, the server will always execute the query.
    cacheStatus?: RunViewCacheStatus;
 * Differential update data containing only changes since the client's cached state.
 * Used to efficiently update client caches without transferring the entire dataset.
export type DifferentialData<T = unknown> = {
     * Records that have been created or updated since the client's maxUpdatedAt.
     * These should be merged into the client's cache, replacing any existing records with the same primary key.
    updatedRows: T[];
     * Primary key values (as concatenated strings) of records that have been deleted.
     * Format uses CompositeKey.ToConcatenatedString() - e.g., "ID|abc123" or "Field1|val1||Field2|val2"
     * These should be removed from the client's cache.
 * Result for a single RunView with cache check.
 * The server returns 'current' (cache valid), 'differential' (partial update), or 'stale' (full refresh needed).
export type RunViewWithCacheCheckResult<T = unknown> = {
     * The index of this view in the batch request (for correlation)
     * 'current' means the client's cache is still valid - no data returned
     * 'differential' means only changes are returned - client should merge with existing cache
     * 'stale' means full refresh is needed - fresh data is included in results (fallback when entity doesn't track changes)
     * 'error' means there was an error checking/executing the view
    status: 'current' | 'differential' | 'stale' | 'error';
     * The fresh results - only populated when status is 'stale' (full refresh)
    results?: T[];
     * Differential update data - only populated when status is 'differential'
     * Contains updated/created rows and deleted record IDs since client's maxUpdatedAt
    differentialData?: DifferentialData<T>;
     * The maximum __mj_UpdatedAt from the results - populated when status is 'stale' or 'differential'
     * The row count of the results - populated when status is 'stale' or 'differential'
     * For 'differential', this is the NEW total row count after applying the delta
     * Error message if status is 'error'
     * Aggregate results - populated when status is 'stale' or 'differential' and aggregates were requested.
     * For 'differential', aggregates are always re-computed fresh (can't be incrementally updated).
    aggregateResults?: AggregateResult[];
 * Response from RunViewsWithCacheCheck - contains results for each view in the batch
export type RunViewsWithCacheCheckResponse<T = unknown> = {
     * Whether the overall operation succeeded
     * Results for each view in the batch, in the same order as the input
    results: RunViewWithCacheCheckResult<T>[];
     * Overall error message if success is false
 * Result of executing a saved query.
 * Contains the query results and execution metadata.
export type RunQueryResult = {
    Results: any[];
     * Total number of rows that would be returned without pagination.
     * Only differs from RowCount when StartRow or MaxRows are used.
     * Parameters that were applied to the query, including defaults
    AppliedParameters?: Record<string, any>;
// RUNQUERY SMART CACHE CHECK TYPES
 * Client-side cache status information for a single RunQuery request.
 * Uses fingerprint data (maxUpdatedAt + rowCount) for efficient cache validation.
export type RunQueryCacheStatus = {
 * Parameters for a single RunQuery request with optional cache status.
export type RunQueryWithCacheCheckParams = {
     * The standard RunQuery parameters
    params: RunQueryParams;
     * use the Query's CacheValidationSQL to check if cached data is still current.
    cacheStatus?: RunQueryCacheStatus;
 * Result for a single RunQuery with cache check.
 * The server returns either 'current' (cache is valid) or 'stale' (with fresh data).
export type RunQueryWithCacheCheckResult<T = unknown> = {
     * The index of this query in the batch request (for correlation)
     * The query ID for reference
     * 'stale' means the cache is outdated - fresh data is included in results
     * 'error' means there was an error checking/executing the query
     * 'no_validation' means the query doesn't have CacheValidationSQL configured
    status: 'current' | 'stale' | 'error' | 'no_validation';
     * The fresh results - only populated when status is 'stale' or 'no_validation'
     * The maximum __mj_UpdatedAt from the results - only when status is 'stale'
     * The row count of the results - only when status is 'stale'
 * Response from RunQueriesWithCacheCheck - contains results for each query in the batch
export type RunQueriesWithCacheCheckResponse<T = unknown> = {
     * Results for each query in the batch, in the same order as the input
    results: RunQueryWithCacheCheckResult<T>[];
 * Interface for providers that execute stored queries.
 * Supports execution of pre-defined SQL queries with security controls.
 * Queries must be pre-approved and stored in the Query entity.
export interface IRunQueryProvider {
    RunQuery(params: RunQueryParams, contextUser?: UserInfo): Promise<RunQueryResult>
     * Executes multiple queries in a single batch operation.
     * More efficient than calling RunQuery multiple times as it reduces network overhead.
     * @param params - Array of query parameters
     * @param contextUser - Optional user context for permissions
     * @returns Array of query results in the same order as input params
    RunQueries(params: RunQueryParams[], contextUser?: UserInfo): Promise<RunQueryResult[]>
     * Executes multiple query requests with smart cache checking.
     * For each query with cacheStatus provided, the server uses the Query's CacheValidationSQL
     * to check if the cached data is still current before executing the full query.
     * This reduces unnecessary data transfer when cached data is valid.
     * @param params Array of query parameters with optional cache status for each
    RunQueriesWithCacheCheck?<T = unknown>(params: RunQueryWithCacheCheckParams[], contextUser?: UserInfo): Promise<RunQueriesWithCacheCheckResponse<T>>
 * Result of executing a report.
 * Contains the report data and execution metadata.
export type RunReportResult = {
    ReportID: string;
 * Interface for providers that execute reports.
 * Handles report generation with various output formats.
 * Reports combine data from multiple sources and apply formatting.
export interface IRunReportProvider {
    RunReport(params: RunReportParams, contextUser?: UserInfo): Promise<RunReportResult>
 * Result of loading a dataset.
 * Contains all dataset items with their data and metadata.
 * Datasets are collections of related entity data loaded together.
export type DatasetResultType = {
    DatasetID: string;
    DatasetName: string;
    LatestUpdateDate: Date;
    Results: DatasetItemResultType[];
 * Result for a single item within a dataset.
 * Represents one entity's data within the larger dataset collection.
export type DatasetItemResultType = {
     * Optional, provides the latest update date for the results provided
    LatestUpdateDate?: Date;
     * Optional, a message if this item failed to load
     * Optional, if not provided Success is assumed to be true
    Success?: boolean;
 * Filter specification for a dataset item.
 * Allows applying custom WHERE clauses to individual dataset items.
export type DatasetItemFilterType = {
    ItemCode: string;
 * Status information for a dataset.
 * Used to check if cached data is up-to-date without loading the full dataset.
export type DatasetStatusResultType = {
    EntityUpdateDates: DatasetStatusEntityUpdateDateType[];
 * Update date information for a single entity within a dataset.
 * Tracks when each entity's data was last modified.
export type DatasetStatusEntityUpdateDateType = {
    UpdateDate: Date;
 * AllMetadata is used to pass all metadata around in a single object for convenience and type safety.
 * Contains all system metadata collections including entities, applications, security, and queries.
 * This class provides a centralized way to access all MemberJunction metadata.
export class AllMetadata {
    CurrentUser: UserInfo = null;
    // Arrays of Metadata below
    AllEntities: EntityInfo[] = [];
    AllApplications: ApplicationInfo[] = [];
    AllRoles: RoleInfo[] = [];
    AllRowLevelSecurityFilters: RowLevelSecurityFilterInfo[] = [];
    AllAuditLogTypes: AuditLogTypeInfo[] = [];
    AllAuthorizations: AuthorizationInfo[] = [];
    AllQueryCategories: QueryCategoryInfo[] = [];
    AllQueries: QueryInfo[] = [];
    AllQueryFields: QueryFieldInfo[] = [];
    AllQueryPermissions: QueryPermissionInfo[] = [];
    AllQueryEntities: QueryEntityInfo[] = [];
    AllQueryParameters: QueryParameterInfo[] = [];
    AllEntityDocumentTypes: EntityDocumentTypeInfo[] = [];
    AllLibraries: LibraryInfo[] = [];
    AllExplorerNavigationItems: ExplorerNavigationItem[] = [];
 * Represents the result of a simple text embedding operation. Not 
 * implemented in @memberjunction/core package but defined here and
 * implemented in sub-classes that live exclusively on the server-side
export interface SimpleEmbeddingResult {
