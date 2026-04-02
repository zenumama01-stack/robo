 * Shared base interfaces for core query information.
 * These interfaces define the contract for query metadata that flows between
 * MemberJunction and external systems.
 * Key design decisions:
 * - PascalCase property names for consistency with MJ conventions
 * - No timestamps or internal metadata that external systems typically don't need
 * Base interface for query field information.
 * Describes individual columns/fields returned by a query.
export interface IQueryFieldInfoBase {
     * Unique identifier for this field record
     * The base SQL type without parameters (e.g., "nvarchar", "decimal")
    SQLBaseType: string;
     * The full SQL type including parameters (e.g., "nvarchar(100)", "decimal(18,2)")
    SQLFullType: string;
    SourceEntityID: string;
     * Name of the source entity
    SourceEntity: string;
    SourceFieldName: string;
     * Whether this field is computed rather than directly selected
    ComputationDescription: string;
    IsSummary: boolean;
    SummaryDescription: string;
 * Base interface for query parameter information.
 * Describes parameters that can be passed to parameterized queries.
export interface IQueryParameterInfoBase {
     * Unique identifier for this parameter record
     * The name of the parameter as it appears in the template (e.g., {{parameterName}})
    DefaultValue: string;
    SampleValue: string;
    ValidationFilters: string;
 * Base interface for query entity information.
 * Tracks which MemberJunction entities are referenced by a query.
export interface IQueryEntityInfoBase {
     * Unique identifier for this entity reference record
     * Foreign key to the referenced entity
     * Name of the referenced entity
 * Base interface for query permission information.
 * Defines which roles can access/execute a query.
export interface IQueryPermissionInfoBase {
     * Unique identifier for this permission record
     * Name of the role
    Role: string;
 * Base interface for query information shared between MJCore and external systems.
 * Contains the core metadata needed to understand and execute stored queries.
export interface IQueryInfoBase {
    CategoryID: string;
     * Full hierarchical path of the category (e.g., "/MJ/AI/Agents/")
    CategoryPath: string;
    Status: 'Pending' | 'In-Review' | 'Approved' | 'Rejected' | 'Obsolete';
    QualityRank: number;
     * Used for smart cache refresh to determine if cached data is stale.
    CacheValidationSQL: string | null;
     * Optional JSON-serialized embedding vector for similarity search
    EmbeddingVector?: string | null;
     * The AI Model ID used to generate the embedding vector
    EmbeddingModelID?: string | null;
    EmbeddingModelName?: string | null;
     * Field metadata for this query
    Fields: IQueryFieldInfoBase[];
     * Parameter definitions for this parameterized query
    Parameters: IQueryParameterInfoBase[];
     * Entities referenced by this query
    Entities?: IQueryEntityInfoBase[];
