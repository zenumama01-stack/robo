 * Mock for @memberjunction/core-entities
 * Provides mock entity classes for unit tests
// Base mock entity class
class MockBaseEntity {
    private _data: Record<string, unknown> = {};
    constructor(data?: Record<string, unknown>) {
        if (data) {
            Object.assign(this._data, data);
    get ID(): string { return this._data.ID as string || ''; }
    set ID(value: string) { this._data.ID = value; }
    async Save(): Promise<boolean> {
    NewRecord(): void {
        // Reset data for a new record
        this._data = {};
    async Load(id: string): Promise<boolean> {
        this._data.ID = id;
    GetAll(): Record<string, unknown> {
        return { ...this._data };
    SetMany(data: Record<string, unknown>): void {
// API Key Entity
export class MJAPIKeyEntity extends MockBaseEntity {
    get Hash(): string { return this['_data'].Hash as string || ''; }
    set Hash(value: string) { this['_data'].Hash = value; }
    get UserID(): string { return this['_data'].UserID as string || ''; }
    set UserID(value: string) { this['_data'].UserID = value; }
    get Label(): string { return this['_data'].Label as string || ''; }
    set Label(value: string | null) { this['_data'].Label = value; }
    get Description(): string | null { return this['_data'].Description as string | null; }
    set Description(value: string | null) { this['_data'].Description = value; }
    get Status(): string { return this['_data'].Status as string || 'Active'; }
    set Status(value: string) { this['_data'].Status = value; }
    get ExpiresAt(): Date | null { return this['_data'].ExpiresAt as Date | null; }
    set ExpiresAt(value: Date | null) { this['_data'].ExpiresAt = value; }
    get LastUsedAt(): Date | null { return this['_data'].LastUsedAt as Date | null; }
    set LastUsedAt(value: Date | null) { this['_data'].LastUsedAt = value; }
    get CreatedByUserID(): string { return this['_data'].CreatedByUserID as string || ''; }
    set CreatedByUserID(value: string) { this['_data'].CreatedByUserID = value; }
// API Application Entity
export class MJAPIApplicationEntity extends MockBaseEntity {
    get Name(): string { return this['_data'].Name as string || ''; }
    set Name(value: string) { this['_data'].Name = value; }
    get IsActive(): boolean { return this['_data'].IsActive as boolean ?? true; }
    set IsActive(value: boolean) { this['_data'].IsActive = value; }
// API Key Application Entity (binding between key and app)
export class MJAPIKeyApplicationEntity extends MockBaseEntity {
    get APIKeyID(): string { return this['_data'].APIKeyID as string || ''; }
    set APIKeyID(value: string) { this['_data'].APIKeyID = value; }
    get ApplicationID(): string { return this['_data'].ApplicationID as string || ''; }
    set ApplicationID(value: string) { this['_data'].ApplicationID = value; }
// API Scope Entity
export class MJAPIScopeEntity extends MockBaseEntity {
    get FullPath(): string { return this['_data'].FullPath as string || ''; }
    set FullPath(value: string) { this['_data'].FullPath = value; }
    get ParentID(): string | null { return this['_data'].ParentID as string | null; }
    set ParentID(value: string | null) { this['_data'].ParentID = value; }
    get Category(): string { return this['_data'].Category as string || ''; }
    set Category(value: string) { this['_data'].Category = value; }
    get ResourceType(): string | null { return this['_data'].ResourceType as string | null; }
    set ResourceType(value: string | null) { this['_data'].ResourceType = value; }
// API Application Scope Entity (ceiling rules)
export class MJAPIApplicationScopeEntity extends MockBaseEntity {
    get ScopeID(): string { return this['_data'].ScopeID as string || ''; }
    set ScopeID(value: string) { this['_data'].ScopeID = value; }
    get ResourcePattern(): string { return this['_data'].ResourcePattern as string || '*'; }
    set ResourcePattern(value: string) { this['_data'].ResourcePattern = value; }
    get PatternType(): string { return this['_data'].PatternType as string || 'Include'; }
    set PatternType(value: string) { this['_data'].PatternType = value; }
    get IsDeny(): boolean { return this['_data'].IsDeny as boolean ?? false; }
    set IsDeny(value: boolean) { this['_data'].IsDeny = value; }
    get Priority(): number { return this['_data'].Priority as number ?? 0; }
    set Priority(value: number) { this['_data'].Priority = value; }
// API Key Scope Entity (key-level scope grants)
export class MJAPIKeyScopeEntity extends MockBaseEntity {
// User Entity
export class MJUserEntity extends MockBaseEntity {
    get Email(): string { return this['_data'].Email as string || ''; }
    set Email(value: string) { this['_data'].Email = value; }
// API Key Usage Log Entity
export class MJAPIKeyUsageLogEntity extends MockBaseEntity {
    get ApplicationID(): string | null { return this['_data'].ApplicationID as string | null; }
    set ApplicationID(value: string | null) { this['_data'].ApplicationID = value; }
    get Endpoint(): string { return this['_data'].Endpoint as string || ''; }
    set Endpoint(value: string) { this['_data'].Endpoint = value; }
    get Operation(): string | null { return this['_data'].Operation as string | null; }
    set Operation(value: string | null) { this['_data'].Operation = value; }
    get Method(): string { return this['_data'].Method as string || 'POST'; }
    set Method(value: string) { this['_data'].Method = value; }
    get StatusCode(): number { return this['_data'].StatusCode as number ?? 200; }
    set StatusCode(value: number) { this['_data'].StatusCode = value; }
    get ResponseTimeMs(): number | null { return this['_data'].ResponseTimeMs as number | null; }
    set ResponseTimeMs(value: number | null) { this['_data'].ResponseTimeMs = value; }
    get RequestedResource(): string | null { return this['_data'].RequestedResource as string | null; }
    set RequestedResource(value: string | null) { this['_data'].RequestedResource = value; }
    get EvaluatedRules(): string | null { return this['_data'].EvaluatedRules as string | null; }
    set EvaluatedRules(value: string | null) { this['_data'].EvaluatedRules = value; }
    get DeniedReason(): string | null { return this['_data'].DeniedReason as string | null; }
    set DeniedReason(value: string | null) { this['_data'].DeniedReason = value; }
    get IPAddress(): string | null { return this['_data'].IPAddress as string | null; }
    set IPAddress(value: string | null) { this['_data'].IPAddress = value; }
    get UserAgent(): string | null { return this['_data'].UserAgent as string | null; }
    set UserAgent(value: string | null) { this['_data'].UserAgent = value; }
 * Used by Credentials/Engine unit tests
    protected _data: Record<string, unknown> = {};
    Save = vi.fn().mockResolvedValue(true);
    NewRecord = vi.fn();
    Load = vi.fn().mockResolvedValue(true);
    GetAll(): Record<string, unknown> { return { ...this._data }; }
export class MJCredentialEntity extends MockBaseEntity {
    get Name(): string { return this._data.Name as string || ''; }
    set Name(value: string) { this._data.Name = value; }
    get CredentialTypeID(): string { return this._data.CredentialTypeID as string || ''; }
    set CredentialTypeID(value: string) { this._data.CredentialTypeID = value; }
    get Values(): string { return this._data.Values as string || ''; }
    set Values(value: string) { this._data.Values = value; }
    get IsActive(): boolean { return this._data.IsActive as boolean ?? true; }
    set IsActive(value: boolean) { this._data.IsActive = value; }
    get IsDefault(): boolean { return this._data.IsDefault as boolean ?? false; }
    set IsDefault(value: boolean) { this._data.IsDefault = value; }
    get ExpiresAt(): Date | null { return this._data.ExpiresAt as Date | null ?? null; }
    set ExpiresAt(value: Date | null) { this._data.ExpiresAt = value; }
    get LastUsedAt(): Date | null { return this._data.LastUsedAt as Date | null ?? null; }
    set LastUsedAt(value: Date | null) { this._data.LastUsedAt = value; }
    get LastValidatedAt(): Date | null { return this._data.LastValidatedAt as Date | null ?? null; }
    set LastValidatedAt(value: Date | null) { this._data.LastValidatedAt = value; }
    get CategoryID(): string | null { return this._data.CategoryID as string | null ?? null; }
    set CategoryID(value: string | null) { this._data.CategoryID = value; }
    get IconClass(): string | null { return this._data.IconClass as string | null ?? null; }
    set IconClass(value: string | null) { this._data.IconClass = value; }
    get Description(): string | null { return this._data.Description as string | null ?? null; }
    set Description(value: string | null) { this._data.Description = value; }
export class MJCredentialTypeEntity extends MockBaseEntity {
    get FieldSchema(): string { return this._data.FieldSchema as string || ''; }
    set FieldSchema(value: string) { this._data.FieldSchema = value; }
    get ValidationEndpoint(): string | null { return this._data.ValidationEndpoint as string | null ?? null; }
    set ValidationEndpoint(value: string | null) { this._data.ValidationEndpoint = value; }
export class MJCredentialCategoryEntity extends MockBaseEntity {
export class MJAuditLogEntity extends MockBaseEntity {
    get UserID(): string { return this._data.UserID as string || ''; }
    set UserID(value: string) { this._data.UserID = value; }
    get AuditLogTypeID(): string { return this._data.AuditLogTypeID as string || ''; }
    set AuditLogTypeID(value: string) { this._data.AuditLogTypeID = value; }
    get Status(): string { return this._data.Status as string || ''; }
    set Status(value: string) { this._data.Status = value; }
    get Description(): string { return this._data.Description as string || ''; }
    set Description(value: string) { this._data.Description = value; }
    get Details(): string { return this._data.Details as string || ''; }
    set Details(value: string) { this._data.Details = value; }
    get EntityID(): string | null { return this._data.EntityID as string | null ?? null; }
    set EntityID(value: string | null) { this._data.EntityID = value; }
    get RecordID(): string | null { return this._data.RecordID as string | null ?? null; }
    set RecordID(value: string | null) { this._data.RecordID = value; }
