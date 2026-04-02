 * Shared constants and utilities for the version-history package.
// Entity name constants
/** Entity name for Version Labels */
export const ENTITY_VERSION_LABELS = 'MJ: Version Labels';
/** Entity name for Version Label Items */
export const ENTITY_VERSION_LABEL_ITEMS = 'MJ: Version Label Items';
/** Entity name for Version Label Restores */
export const ENTITY_VERSION_LABEL_RESTORES = 'MJ: Version Label Restores';
/** Entity name for Record Changes */
export const ENTITY_RECORD_CHANGES = 'MJ: Record Changes';
// SQL safety utilities
 * Escape a string value for safe inclusion in a SQL filter.
 * Doubles single quotes to prevent SQL injection.
export function escapeSqlString(value: string): string {
    return String(value).replace(/'/g, "''");
 * Build a safe SQL equality filter: FieldName = 'escapedValue'
export function sqlEquals(fieldName: string, value: string): string {
    return `${fieldName} = '${escapeSqlString(value)}'`;
 * Build a safe SQL LIKE filter: FieldName LIKE '%escapedValue%'
export function sqlContains(fieldName: string, value: string): string {
    return `${fieldName} LIKE '%${escapeSqlString(value)}%'`;
 * Build a safe SQL IN filter: FieldName IN ('a','b','c')
export function sqlIn(fieldName: string, values: string[]): string {
    const escaped = values.map(v => `'${escapeSqlString(v)}'`).join(', ');
    return `${fieldName} IN (${escaped})`;
 * Build a safe SQL NOT IN filter: FieldName NOT IN ('a','b','c')
export function sqlNotIn(fieldName: string, values: string[]): string {
    return `${fieldName} NOT IN (${escaped})`;
// Composite key utilities
import { CompositeKey, EntityInfo } from '@memberjunction/core';
 * Build a CompositeKey from entity metadata and a record data object.
 * Shared utility to avoid duplication across Walker and SnapshotBuilder.
export function buildCompositeKeyFromRecord(
    record: Record<string, unknown>
): CompositeKey {
    const pairs = entityInfo.PrimaryKeys.map(pk => ({
        Value: record[pk.Name],
    return new CompositeKey(pairs);
 * Build a CompositeKey for loading by ID (single-field PK).
 * Uses the entity's actual first primary key name rather than hardcoding 'ID'.
export function buildPrimaryKeyForLoad(
    return new CompositeKey([{
 * Build a CompositeKey for an entity that we know uses 'ID' as PK.
 * Only for MJ system entities (VersionLabel, VersionLabelItem, etc.)
 * where we control the schema and know the PK is always 'ID'.
export function buildIdKey(id: string): CompositeKey {
    return new CompositeKey([{ FieldName: 'ID', Value: id }]);
// Record change utilities
import { BaseEntity, Metadata, RunView, UserInfo, LogError } from '@memberjunction/core';
 * Load a FullRecordJSON snapshot from a RecordChange entry.
 * Returns parsed JSON or null on failure.
 * Shared by DiffEngine and RestoreEngine.
export async function loadRecordChangeSnapshot(
        Fields: ['ID', 'FullRecordJSON'],
        LogError(`VersionHistory: RecordChange '${recordChangeId}' not found`);
    const jsonStr = result.Results[0]['FullRecordJSON'] as string;
    if (!jsonStr) {
        LogError(`VersionHistory: RecordChange '${recordChangeId}' has null FullRecordJSON`);
        return JSON.parse(jsonStr);
        LogError(`VersionHistory: Failed to parse FullRecordJSON for RecordChange '${recordChangeId}'`);
 * Load a strongly-typed entity by its ID using InnerLoad with the entity's actual PK name.
 * Returns null if not found or on error.
export async function loadEntityById<T extends BaseEntity = BaseEntity>(
): Promise<T | null> {
        LogError(`VersionHistory: Entity '${entityName}' not found in metadata`);
    const entity = await md.GetEntityObject<T>(entityName, contextUser);
    const key = buildPrimaryKeyForLoad(entityInfo, id);
    if (!loaded) return null;
