 * For each Skip API Analysis result, it is possible for Skip to provide a set of tableDataColumns that describe the data that is being returned in this shape.
export interface SkipColumnInfo {
    simpleDataType: 'string' | 'number' | 'date' | 'boolean';
 * Enumerates the possible values for a given field
export interface SkipEntityFieldValueInfo {
     * Possible value
     * Value to show user, if different from value
 * Describes a single field in an entity.
export interface SkipEntityFieldInfo {
    isUnique: boolean;
    length: number;
    autoIncrement: boolean;
    valueListType?: string;
    extendedType?: string;
    defaultColumnWidth: number;
    isNameField: boolean;
    relatedEntityID?: string;
    relatedEntityFieldName?: string;
    relatedEntitySchemaName?: string;
    relatedEntityBaseView?: string;
    possibleValues?: SkipEntityFieldValueInfo[];
 * Defines relationships between entities, including foreign key relationships and
 * many-to-many relationships through junction tables.
export interface SkipEntityRelationshipInfo {
    relatedEntityID: string;
    entityKeyField: string;
    relatedEntityJoinField: string;
    joinView: string;
    joinEntityJoinField: string;
    joinEntityInverseJoinField: string;
    entityBaseView: string;
    relatedEntityBaseView: string;
 * Info about a single entity including fields and relationships
export interface SkipEntityInfo {
    fields: SkipEntityFieldInfo[];
    relatedEntities: SkipEntityRelationshipInfo[];
     * If rows packed is set to anything other than none, the data is provided in the rows property.
    rowsPacked?: 'None' | 'Sample' | 'All';
     * If rowsPacked === 'Sample', this additional property is used to indicate the method used to sample the rows
    rowsSampleMethod?: 'random' | 'top n' | 'bottom n';
     * Optional, the metadata can include an array of rows that can be used to provide context to Skip for the data that is being passed in.
    rows?: unknown[];
