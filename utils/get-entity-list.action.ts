import { Metadata } from "@memberjunction/core";
 * Action that returns a lightweight list of all entities in the system.
 * Returns only entity name, schema name, and description - perfect for discovering
 * what entities exist before drilling into details with Explore Database Schema.
 * This action is optimized for research agents that need to:
 * - Discover available entities without guessing names
 * - Get a quick overview of the data model
 * - Find entities by description/purpose
 * - Identify correct entity names before detailed exploration
 * - Uses cached metadata (no database queries)
 * - Extremely fast and lightweight
 * - Optional schema filtering
 * - Optional scope filtering
 * - Sorted alphabetically for easy scanning
 * // Get all entities
 *   ActionName: 'Get Entity List'
 * // Get only entities in specific schema
 *   ActionName: 'Get Entity List',
 *     Name: 'SchemaFilter',
 *     Value: 'dbo'
@RegisterClass(BaseAction, "Get Entity List")
export class GetEntityListAction extends BaseAction {
            // Get MJ metadata (cached, no DB queries)
            if (schemaFilter) {
                const schemas = schemaFilter.split(',').map(s => s.trim().toLowerCase());
                entities = entities.filter(e => schemas.includes(e.SchemaName.toLowerCase()));
            if (scopeFilter) {
                entities = entities.filter(e => {
                    if (!e.ScopeDefault) return true; // Include entities with no scope
                    const scopes = e.ScopeDefault.split(',').map(s => s.trim().toLowerCase());
                    return scopes.includes(scopeFilter.toLowerCase()) || scopes.includes('all');
            if (!includeVirtualEntities) {
                entities = entities.filter(e => !e.VirtualEntity);
            // Build lightweight entity list (name, schema, description only)
            const entityList = entities.map(e => ({
                Name: e.Name,
                SchemaName: e.SchemaName,
                Description: e.Description || '',
                IsVirtual: e.VirtualEntity
                // Sort by schema first, then by name
                if (a.SchemaName !== b.SchemaName) {
                    return a.SchemaName.localeCompare(b.SchemaName);
            // Build detailed message for agent consumption
            const message = this.buildDetailedMessage(entityList, schemaFilter, scopeFilter);
                Entities: entityList,
                TotalCount: entityList.length,
                ResultCode: "LIST_FAILED",
                Message: `Entity list retrieval failed: ${errorMessage}`
     * Build detailed message with entity list for agent consumption
        entities: Array<{ Name: string; SchemaName: string; Description: string; IsVirtual: boolean }>,
        schemaFilter?: string,
        scopeFilter?: string
        lines.push(`# Entity List`);
        lines.push(`\nFound ${entities.length} entity(ies)`);
            lines.push(`Schema Filter: ${schemaFilter}`);
            lines.push(`Scope Filter: ${scopeFilter}`);
        const schemas = [...new Set(entities.map(e => e.SchemaName))].sort();
            const schemaEntities = entities.filter(e => e.SchemaName === schema);
            lines.push(`\n## Schema: ${schema} (${schemaEntities.length} entities)\n`);
            for (const entity of schemaEntities) {
                const virtualFlag = entity.IsVirtual ? ' [VIRTUAL]' : '';
                lines.push(`- **${entity.Name}**${virtualFlag}`);
                    lines.push(`  ${entity.Description}`);
        lines.push(`\nUse "Explore Database Schema" action with EntityPattern parameter to get detailed field information for specific entities.`);
