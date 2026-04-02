import { Metadata, EntityInfo, EntityFieldInfo, EntityRelationshipInfo } from "@memberjunction/core";
 * Action that retrieves comprehensive database schema information using MemberJunction's
 * rich metadata layer instead of raw INFORMATION_SCHEMA queries.
 * This action provides research agents with deep understanding of the data model including:
 * - Business context (descriptions, display names)
 * - Field metadata (types, defaults, value lists, related entities)
 * - Semantic relationships (not just foreign keys)
 * - Virtual entities (logical, not physical)
 * - API permissions and settings
 * - Audit configuration
 * - UI display preferences
 * - Entity name pattern filtering with wildcards
 * - Schema filtering (focus on specific schemas)
 * - Optional field details with rich metadata
 * - Optional relationship information with semantic meaning
 * - Optional permission information
 * - Virtual entity support
 * - Scope-based filtering
 * - Configurable output size limits
 * // Get all customer-related entities with full metadata
 *   ActionName: 'Explore Database Schema',
 *     Name: 'EntityPattern',
 *     Value: '%Customer%'
 *     Name: 'IncludeFields',
 *     Name: 'IncludeRelationships',
@RegisterClass(BaseAction, "Explore Database Schema")
export class ExploreDatabaseSchemaAction extends BaseAction {
            const entityPattern = this.getStringParam(params, "entitypattern");
            const schemaFilter = this.getStringParam(params, "schemafilter");
            const includeFields = this.getBooleanParam(params, "includefields", true);
            const includeRelationships = this.getBooleanParam(params, "includerelationships", true);
            const includePermissions = this.getBooleanParam(params, "includepermissions", false);
            const includeVirtualEntities = this.getBooleanParam(params, "includevirtualentities", true);
            const maxEntities = this.getNumericParam(params, "maxentities", 100);
            const scopeFilter = this.getStringParam(params, "scopefilter");
            // Get MJ metadata
            entities = this.filterEntities(entities, {
                entityPattern,
                schemaFilter,
                includeVirtualEntities,
                scopeFilter,
                maxEntities
            if (entities.length === 0) {
                    ResultCode: "NO_ENTITIES_FOUND",
                    Message: "No entities match the specified filter criteria"
            // Build result objects
            const formattedEntities = entities.map(entity =>
                this.formatEntityInfo(entity, {
                    includeFields,
                    includeRelationships,
                    includePermissions
            // Group by schema
            const schemas = this.groupBySchema(formattedEntities);
            const summary = this.calculateSummary(formattedEntities);
            const executionTimeMs = Date.now() - startTime;
            // Build detailed result message with entity information
            const message = this.buildDetailedMessage(formattedEntities, schemas, summary, {
                includeRelationships
                Entities: formattedEntities,
                Schemas: schemas,
                EntitySummary: summary,
                ExecutionTimeMs: executionTimeMs
                ResultCode: "EXPLORATION_FAILED",
                Message: `Schema exploration failed: ${errorMessage}`
     * Filter entities based on parameters
    private filterEntities(
        entities: EntityInfo[],
            entityPattern?: string;
            schemaFilter?: string;
            includeVirtualEntities: boolean;
            scopeFilter?: string;
            maxEntities: number;
    ): EntityInfo[] {
        return entities.filter(entity => {
            // Entity name pattern filter (supports wildcards % and *)
            if (filters.entityPattern) {
                if (!this.matchesPattern(entity.Name, filters.entityPattern)) {
            // Schema filter (comma-separated list)
            if (filters.schemaFilter) {
                const schemas = filters.schemaFilter.split(',').map(s => s.trim().toLowerCase());
                if (!schemas.includes(entity.SchemaName.toLowerCase())) {
            // Virtual entity filter
            if (!filters.includeVirtualEntities && entity.VirtualEntity) {
            // Scope filter
            if (filters.scopeFilter && entity.ScopeDefault) {
                const scopes = entity.ScopeDefault.split(',').map(s => s.trim().toLowerCase());
                if (!scopes.includes(filters.scopeFilter.toLowerCase()) && !scopes.includes('all')) {
        }).slice(0, filters.maxEntities);
     * Check if entity name matches pattern (supports % and * wildcards)
    private matchesPattern(name: string, pattern: string): boolean {
        // Convert SQL-style % wildcards to regex-style .*
        // Also support * as wildcard
            .replace(/[.+?^${}()|[\]\\]/g, '\\$&') // Escape special regex chars
            .replace(/%/g, '.*')  // Convert % to .*
        return regex.test(name);
     * Format EntityInfo into research-friendly structure
    private formatEntityInfo(
            includeFields: boolean;
            includeRelationships: boolean;
            includePermissions: boolean;
        const result: Record<string, any> = {
            ID: entity.ID,
            Name: entity.Name,
            DisplayName: entity.DisplayName || entity.Name,
            Description: entity.Description,
            SchemaName: entity.SchemaName,
            BaseTable: entity.BaseTable,
            BaseView: entity.BaseView,
            IsVirtual: entity.VirtualEntity,
            Icon: entity.Icon,
            // API Settings
            APISettings: {
                IncludeInAPI: entity.IncludeInAPI,
                AllowCreateAPI: entity.AllowCreateAPI,
                AllowUpdateAPI: entity.AllowUpdateAPI,
                AllowDeleteAPI: entity.AllowDeleteAPI,
                AllowUserSearchAPI: entity.AllowUserSearchAPI,
                AllowAllRowsAPI: entity.AllowAllRowsAPI,
                CustomResolverAPI: entity.CustomResolverAPI
            // Audit Settings
            AuditSettings: {
                TrackRecordChanges: entity.TrackRecordChanges,
                AuditRecordAccess: entity.AuditRecordAccess,
                AuditViewRuns: entity.AuditViewRuns
            // Search Settings
            SearchSettings: {
                FullTextSearchEnabled: entity.FullTextSearchEnabled,
                FullTextCatalog: entity.FullTextCatalog,
                FullTextIndex: entity.FullTextIndex
            // Other Settings
            AllowRecordMerge: entity.AllowRecordMerge,
            CascadeDeletes: entity.CascadeDeletes,
            DeleteType: entity.DeleteType,
            UserViewMaxRows: entity.UserViewMaxRows,
            ScopeDefault: entity.ScopeDefault
        // Include fields if requested
        if (options.includeFields && entity.Fields) {
            result.Fields = entity.Fields.map(field => this.formatFieldInfo(field));
            result.FieldCount = result.Fields.length;
        // Include relationships if requested
        if (options.includeRelationships && entity.RelatedEntities) {
            result.Relationships = entity.RelatedEntities.map(rel => this.formatRelationshipInfo(rel));
            result.RelationshipCount = result.Relationships.length;
        // Include permissions if requested
        if (options.includePermissions && entity.Permissions) {
            result.Permissions = entity.Permissions.map(perm => ({
                Role: perm.Role,
                RoleSQLName: perm.RoleSQLName,
                CanCreate: perm.CanCreate,
                CanRead: perm.CanRead,
                CanUpdate: perm.CanUpdate,
                CanDelete: perm.CanDelete
            result.PermissionCount = result.Permissions.length;
     * Format EntityFieldInfo into research-friendly structure
    private formatFieldInfo(field: EntityFieldInfo): Record<string, any> {
        const fieldInfo: Record<string, any> = {
            Name: field.Name,
            DisplayName: field.DisplayName || field.Name,
            Description: field.Description,
            Type: field.Type,
            Length: field.Length,
            Precision: field.Precision,
            Scale: field.Scale,
            AllowsNull: field.AllowsNull,
            DefaultValue: field.DefaultValue,
            IsPrimaryKey: field.IsPrimaryKey,
            IsUnique: field.IsUnique,
            IsNameField: field.IsNameField,
            IsVirtual: field.IsVirtual,
            AutoIncrement: field.AutoIncrement,
            Category: field.Category,
            // Related entity info (for foreign keys)
            RelatedEntity: field.RelatedEntity,
            RelatedEntityFieldName: field.RelatedEntityFieldName,
            RelatedEntityDisplayType: field.RelatedEntityDisplayType,
            // Value list (for dropdowns/validation)
            ValueListType: field.ValueListType,
            // API settings
            AllowUpdateAPI: field.AllowUpdateAPI,
            AllowUpdateInView: field.AllowUpdateInView,
            IncludeInUserSearchAPI: field.IncludeInUserSearchAPI,
            FullTextSearchEnabled: field.FullTextSearchEnabled,
            // UI settings
            DefaultInView: field.DefaultInView,
            IncludeInGeneratedForm: field.IncludeInGeneratedForm,
            GeneratedFormSection: field.GeneratedFormSection,
            DefaultColumnWidth: field.DefaultColumnWidth,
            // Scope
            ScopeDefault: field.ScopeDefault,
            Status: field.Status
        // Include value list values if available
        if (field.EntityFieldValues && field.EntityFieldValues.length > 0) {
            fieldInfo.EntityFieldValues = field.EntityFieldValues.map(v => ({
                Value: v.Value,
                Code: v.Code,
                Description: v.Description,
                Sequence: v.Sequence
        return fieldInfo;
     * Format EntityRelationshipInfo into research-friendly structure
    private formatRelationshipInfo(rel: EntityRelationshipInfo): Record<string, any> {
            ID: rel.ID,
            Type: rel.Type,
            RelatedEntity: rel.RelatedEntity,
            RelatedEntityClassName: rel.RelatedEntityClassName,
            EntityKeyField: rel.EntityKeyField,
            RelatedEntityJoinField: rel.RelatedEntityJoinField,
            DisplayName: rel.DisplayName,
            DisplayInForm: rel.DisplayInForm,
            DisplayLocation: rel.DisplayLocation,
            BundleInAPI: rel.BundleInAPI,
            IncludeInParentAllQuery: rel.IncludeInParentAllQuery,
            Sequence: rel.Sequence
     * Group entities by schema name
    private groupBySchema(entities: Record<string, any>[]): Record<string, any>[] {
        const schemaMap: Record<string, any> = {};
        for (const entity of entities) {
            if (!schemaMap[schemaName]) {
                schemaMap[schemaName] = {
                    SchemaName: schemaName,
                    Entities: [],
                    EntityCount: 0,
                    VirtualEntityCount: 0,
                    PhysicalEntityCount: 0
            schemaMap[schemaName].Entities.push(entity);
            schemaMap[schemaName].EntityCount++;
            if (entity.IsVirtual) {
                schemaMap[schemaName].VirtualEntityCount++;
                schemaMap[schemaName].PhysicalEntityCount++;
        return Object.values(schemaMap).sort((a, b) =>
            a.SchemaName.localeCompare(b.SchemaName)
     * Build detailed message with entity information for agent consumption
        formattedEntities: Record<string, any>[],
        schemas: Record<string, any>[],
        summary: Record<string, any>,
        // Summary header
        lines.push(`# Database Schema Exploration Results`);
        lines.push(`\nFound ${formattedEntities.length} entity(ies) across ${schemas.length} schema(s)`);
        if (options.includeFields) {
            lines.push(`Total Fields: ${summary.TotalFields}`);
        if (options.includeRelationships) {
            lines.push(`Total Relationships: ${summary.TotalRelationships}`);
        // Entity details grouped by schema
        for (const schema of schemas) {
            lines.push(`\n## Schema: ${schema.SchemaName}`);
            lines.push(`Entities: ${schema.EntityCount} (${schema.PhysicalEntityCount} physical, ${schema.VirtualEntityCount} virtual)\n`);
            for (const entity of schema.Entities) {
                lines.push(`\n### ${entity.DisplayName} (${entity.Name})`);
                if (entity.Description) {
                    lines.push(`**Description:** ${entity.Description}`);
                lines.push(`**Base Table:** ${entity.BaseTable || 'N/A'}`);
                lines.push(`**Base View:** ${entity.BaseView || 'N/A'}`);
                lines.push(`**Virtual:** ${entity.IsVirtual ? 'Yes' : 'No'}`);
                lines.push(`**Schema:** ${entity.SchemaName}`);
                // API Settings (condensed)
                const apiFlags: string[] = [];
                if (entity.APISettings.IncludeInAPI) apiFlags.push('IncludeInAPI');
                if (entity.APISettings.AllowCreateAPI) apiFlags.push('Create');
                if (entity.APISettings.AllowUpdateAPI) apiFlags.push('Update');
                if (entity.APISettings.AllowDeleteAPI) apiFlags.push('Delete');
                if (entity.APISettings.AllowUserSearchAPI) apiFlags.push('Search');
                if (apiFlags.length > 0) {
                    lines.push(`**API Support:** ${apiFlags.join(', ')}`);
                if (options.includeFields && entity.Fields && entity.Fields.length > 0) {
                    lines.push(`\n#### Fields (${entity.Fields.length})`);
                    for (const field of entity.Fields) {
                        const fieldLine: string[] = [];
                        fieldLine.push(`- **${field.DisplayName || field.Name}** (${field.Name})`);
                        fieldLine.push(`Type: ${field.Type}`);
                        if (field.Length) fieldLine.push(`Length: ${field.Length}`);
                        if (field.IsPrimaryKey) fieldLine.push('PRIMARY KEY');
                        if (field.IsUnique) fieldLine.push('UNIQUE');
                        if (!field.AllowsNull) fieldLine.push('NOT NULL');
                        if (field.DefaultValue) fieldLine.push(`Default: ${field.DefaultValue}`);
                        // Related entity info
                        if (field.RelatedEntity) {
                            fieldLine.push(`→ Related to: ${field.RelatedEntity}.${field.RelatedEntityFieldName || 'ID'}`);
                        // Value list
                            const values = field.EntityFieldValues.map((v: any) => v.Value).join(', ');
                            fieldLine.push(`Values: [${values}]`);
                        lines.push(`  ${fieldLine.join(' | ')}`);
                        if (field.Description) {
                            lines.push(`    *${field.Description}*`);
                // Relationships
                if (options.includeRelationships && entity.Relationships && entity.Relationships.length > 0) {
                    lines.push(`\n#### Relationships (${entity.Relationships.length})`);
                    for (const rel of entity.Relationships) {
                        const relLine: string[] = [];
                        relLine.push(`- **${rel.DisplayName || rel.RelatedEntity}**`);
                        relLine.push(`Type: ${rel.Type}`);
                        relLine.push(`Entity: ${rel.RelatedEntity}`);
                        relLine.push(`Join: ${entity.Name}.${rel.EntityKeyField} → ${rel.RelatedEntity}.${rel.RelatedEntityJoinField}`);
                        if (rel.BundleInAPI) relLine.push('BundleInAPI');
                        if (rel.DisplayInForm) relLine.push(`Display: ${rel.DisplayLocation}`);
                        lines.push(`  ${relLine.join(' | ')}`);
                lines.push(''); // Blank line between entities
    private calculateSummary(
        formattedEntities: Record<string, any>[]
        const summary: Record<string, any> = {
            TotalEntities: formattedEntities.length,
            PhysicalEntities: formattedEntities.filter(e => !e.IsVirtual).length,
            VirtualEntities: formattedEntities.filter(e => e.IsVirtual).length,
            TotalFields: 0,
            TotalRelationships: 0,
            SchemaList: [...new Set(formattedEntities.map(e => e.SchemaName))].sort(),
            EntitiesWithAPI: formattedEntities.filter(e => e.APISettings.IncludeInAPI).length,
            EntitiesWithFullTextSearch: formattedEntities.filter(e => e.SearchSettings.FullTextSearchEnabled).length,
            EntitiesWithAudit: formattedEntities.filter(e => e.AuditSettings.TrackRecordChanges).length
        // Count fields and relationships if included
        for (const entity of formattedEntities) {
            if (entity.FieldCount != null) {
                summary.TotalFields += entity.FieldCount;
            if (entity.RelationshipCount != null) {
                summary.TotalRelationships += entity.RelationshipCount;
     * Helper to get boolean parameter value
