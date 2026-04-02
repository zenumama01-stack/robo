 * Types and interfaces for the Query Data Grid component.
 * These are tailored for query results which differ from entity data:
 * - Read-only (no CRUD operations)
 * - Client-side sorting only (query SQL defines ORDER BY)
 * - No smart filter (parameters only)
 * - Support for entity linking via SourceEntityID
import { QueryInfo, QueryFieldInfo, QueryParameterInfo, Metadata, EntityInfo, EntityFieldInfo } from '@memberjunction/core';
// Selection Mode
 * Selection mode for the query grid
export type QueryGridSelectionMode = 'none' | 'single' | 'multiple' | 'checkbox';
 * Configuration for a query grid column
 * Derived from QueryFieldInfo metadata
export interface QueryGridColumnConfig {
    /** Field name from query results */
    /** Description/tooltip for the column header */
    /** Column is sortable (client-side) */
    sortable: boolean;
    resizable: boolean;
    reorderable: boolean;
    /** SQL base type for formatting */
    sqlBaseType: string;
    /** SQL full type for display */
    sqlFullType: string;
    /** Order/sequence of this column */
    /** Source entity ID if this column references an entity */
    sourceEntityId?: string;
    /** Source entity name if this column references an entity */
    sourceEntityName?: string;
    /** Source field name in the entity (e.g., 'ID' for primary key) */
    sourceFieldName?: string;
    /** Whether this column contains linkable entity IDs */
    isEntityLink: boolean;
     * The entity name to navigate to when clicking the link.
     * - For primary keys: this is the source entity itself
     * - For foreign keys: this is the related entity the FK points to
    targetEntityName?: string;
     * The entity ID to navigate to when clicking the link.
    targetEntityId?: string;
     * Whether this field is a primary key of the source entity
    isPrimaryKey?: boolean;
     * Whether this field is a foreign key to another entity
    isForeignKey?: boolean;
     * Icon class for the target entity (from EntityInfo.Icon)
     * Used to display the entity's icon in the column header
    targetEntityIcon?: string;
// Sort State
 * Sort state for a column (client-side sorting)
export interface QueryGridSortState {
// Grid State (for persistence)
 * Complete grid state for persistence to User Settings
export interface QueryGridState {
    /** Sort state (client-side) */
    sort: QueryGridSortState[];
// Parameter State (for persistence)
 * Parameter values for a query execution
export interface QueryParameterValues {
    [parameterName: string]: string | number | boolean | Date | string[] | null;
// Visual Configuration
export type QueryGridHeaderStyle = 'flat' | 'elevated' | 'gradient' | 'bold';
 * Configuration for query grid visual appearance
export interface QueryGridVisualConfig {
    /** Header style preset */
    headerStyle?: QueryGridHeaderStyle;
    /** Custom header background color */
    /** Contrast level for alternating rows */
    /** Format dates with a friendly format */
    /** Render boolean cells as checkmark/x icons */
    /** Selection indicator color */
    /** Selection indicator width */
    /** Checkbox style */
    /** Show skeleton loading rows */
    /** Number of skeleton rows */
    /** Border radius for container */
    /** Cell padding preset */
    /** Primary accent color */
 * Default visual configuration for query grid
export const DEFAULT_QUERY_VISUAL_CONFIG: Required<QueryGridVisualConfig> = {
    headerBackground: '',
    headerTextColor: '',
// Export Options
 * Export options for query results
export interface QueryExportOptions {
 * Event fired when a row is clicked
export interface QueryRowClickEvent {
    /** The row data */
    rowData: Record<string, unknown>;
    /** Row index */
    /** Column that was clicked */
    column?: QueryGridColumnConfig;
    /** Cell value */
    cellValue?: unknown;
    /** Original mouse event */
 * Event fired when an entity link is clicked
export interface QueryEntityLinkClickEvent {
    /** Record ID (primary key value) */
    /** The column config (null if clicked from row detail panel) */
    column: QueryGridColumnConfig | null;
 * Event fired when grid state changes
export interface QueryGridStateChangedEvent {
    /** The new grid state */
    state: QueryGridState;
    changeType: 'column-resize' | 'column-move' | 'column-visibility' | 'sort';
 * Event fired when selection changes
export interface QuerySelectionChangedEvent {
    /** Selected row indices */
    selectedIndices: number[];
    /** Selected row data */
    selectedRows: Record<string, unknown>[];
 * Result of resolving the target entity for navigation
interface TargetEntityInfo {
    isForeignKey: boolean;
 * Determines the target entity for navigation based on source entity field metadata.
 * - If the source field is a primary key, target is the source entity itself
 * - If the source field is a foreign key, target is the related entity
 * Also retrieves the target entity's icon for display in column headers.
function resolveTargetEntity(
    sourceEntityName: string | undefined,
    sourceFieldName: string | undefined,
    md: Metadata
): TargetEntityInfo {
    if (!sourceEntityName || !sourceFieldName) {
        return { isPrimaryKey: false, isForeignKey: false };
    // Look up the source entity
    const sourceEntity = md.Entities.find(e => e.Name === sourceEntityName);
    if (!sourceEntity) {
    // Find the field in the source entity
    const entityField = sourceEntity.Fields.find(f => f.Name === sourceFieldName);
    if (!entityField) {
    // Check if it's a primary key
    if (entityField.IsPrimaryKey) {
            targetEntityName: sourceEntityName,
            targetEntityId: sourceEntity.ID,
            targetEntityIcon: sourceEntity.Icon || undefined,
            isPrimaryKey: true,
            isForeignKey: false
    // Check if it's a foreign key (has RelatedEntity)
    if (entityField.RelatedEntity && entityField.RelatedEntity.trim().length > 0) {
        const relatedEntity = md.Entities.find(e => e.Name === entityField.RelatedEntity);
            targetEntityName: entityField.RelatedEntity,
            targetEntityId: relatedEntity?.ID,
            targetEntityIcon: relatedEntity?.Icon || undefined,
            isForeignKey: true
 * Builds column configs from QueryFieldInfo metadata
export function buildColumnsFromQueryFields(fields: QueryFieldInfo[]): QueryGridColumnConfig[] {
    return fields
        .sort((a, b) => (a.Sequence || 0) - (b.Sequence || 0))
        .map((field, index) => {
            // Resolve the target entity using metadata
            const targetInfo = resolveTargetEntity(field.SourceEntity, field.SourceFieldName, md);
            // Determine if this is an entity link
            // It's linkable if we have a valid target entity (either PK or FK)
            const isEntityLink = !!(targetInfo.targetEntityName && (targetInfo.isPrimaryKey || targetInfo.isForeignKey));
            // Determine alignment based on type
            let align: 'left' | 'center' | 'right' = 'left';
            const baseType = (field.SQLBaseType || '').toLowerCase();
            if (['int', 'bigint', 'decimal', 'numeric', 'float', 'money', 'smallmoney', 'real'].includes(baseType)) {
            } else if (['bit'].includes(baseType)) {
                align = 'center';
                title: field.Name, // Could be enhanced to parse CamelCase
                description: field.Description || undefined,
                width: undefined,
                minWidth: 80,
                maxWidth: undefined, // No max width limit - let users resize freely
                reorderable: true,
                sqlBaseType: field.SQLBaseType || 'nvarchar',
                sqlFullType: field.SQLFullType || 'nvarchar(max)',
                align,
                order: index,
                sourceEntityId: field.SourceEntityID || undefined,
                sourceEntityName: field.SourceEntity || undefined,
                sourceFieldName: field.SourceFieldName || undefined,
                isEntityLink,
                targetEntityName: targetInfo.targetEntityName,
                targetEntityId: targetInfo.targetEntityId,
                isPrimaryKey: targetInfo.isPrimaryKey,
                isForeignKey: targetInfo.isForeignKey,
                targetEntityIcon: targetInfo.targetEntityIcon,
                pinned: null,
                flex: undefined
 * Gets the User Settings key for query grid state
export function getQueryGridStateKey(queryId: string): string {
    return `QueryViewer_${queryId}_GridState`;
 * Gets the User Settings key for query parameters
export function getQueryParamsKey(queryId: string): string {
    return `QueryViewer_${queryId}_LastParams`;
 * Infers column configuration from actual data when query has no field metadata.
 * Examines the first row to determine column names and types.
export function buildColumnsFromData(data: Record<string, unknown>[]): QueryGridColumnConfig[] {
    // Get column names from first row
    const firstRow = data[0];
    const columnNames = Object.keys(firstRow);
    return columnNames.map((name, index) => {
        // Infer type from value
        const value = firstRow[name];
        let sqlBaseType = 'nvarchar';
            sqlBaseType = Number.isInteger(value) ? 'int' : 'decimal';
        } else if (typeof value === 'boolean') {
            sqlBaseType = 'bit';
            sqlBaseType = 'datetime';
        } else if (typeof value === 'string') {
            // Check if it looks like a date
            const datePattern = /^\d{4}-\d{2}-\d{2}/;
            if (datePattern.test(value)) {
            field: name,
            description: undefined,
            maxWidth: undefined,
            sqlBaseType,
            sqlFullType: sqlBaseType,
            sourceEntityId: undefined,
            sourceEntityName: undefined,
            sourceFieldName: undefined,
            isEntityLink: false,
            targetEntityName: undefined,
            targetEntityId: undefined,
            isForeignKey: false,
            targetEntityIcon: undefined,
