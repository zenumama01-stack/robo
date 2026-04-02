import { Metadata, RunView } from "@memberjunction/core";
 * Action that returns field details and sample data for a specific entity.
 * Perfect for understanding entity structure before writing queries.
 * Returns:
 * - Field names, types, descriptions
 * - Sample data (top 3 rows by default)
 * - Total row count
 * - Primary key information
 * - Related entity references
 * This action uses RunView (cached metadata + query) - very fast and efficient.
 * // Get details for Customers entity
 *   ActionName: 'Get Entity Details',
 *     Value: 'Customers'
 * // Get more sample rows
 *     Value: 'Orders'
 *     Name: 'SampleRowCount',
 *     Value: 10
@RegisterClass(BaseAction, "Get Entity Details")
export class GetEntityDetailsAction extends BaseAction {
            const entityName = this.getStringParam(params, "entityname");
            const sampleRowCount = this.getNumericParam(params, "samplerowcount", 3);
            const includeRelatedEntityInfo = this.getBooleanParam(params, "includerelatedentityinfo", true);
            // Get entity metadata
                    ResultCode: "ENTITY_NOT_FOUND",
                    Message: `Entity '${entityName}' not found. Use 'Get Entity List' action to see available entities.`
            // Build field information
            const fields = entity.Fields.map(f => ({
                Name: f.Name,
                DisplayName: f.DisplayName,
                Type: f.Type,
                SQLFullType: f.SQLFullType,
                Description: f.Description || '',
                IsPrimaryKey: f.IsPrimaryKey,
                IsNameField: f.IsNameField,
                AllowsNull: f.AllowsNull,
                DefaultValue: f.DefaultValue,
                RelatedEntity: f.RelatedEntity || null,
                RelatedEntityFieldName: f.RelatedEntityFieldName || null,
                ValueListType: f.ValueListType,
                HasValueList: f.EntityFieldValues && f.EntityFieldValues.length > 0,
                ValueListValues: f.EntityFieldValues && f.EntityFieldValues.length > 0
                    ? f.EntityFieldValues.map(v => v.Value).join(', ')
            // Get sample data and total count using RunView
            let sampleData: any[] = [];
            let totalRowCount = undefined;
            if (sampleRowCount && sampleRowCount > 0) {
                    MaxRows: sampleRowCount,
                        ResultCode: "SAMPLE_DATA_FAILED",
                        Message: `Failed to retrieve sample data: ${result.ErrorMessage}`
                // Ensure we only return the requested number of rows
                const allResults = result.Results || [];
                sampleData = allResults.slice(0, sampleRowCount);
                totalRowCount = result.TotalRowCount || 0;
            // Build primary key info
            const primaryKeyFields = fields.filter(f => f.IsPrimaryKey).map(f => f.Name);
            // Build related entity info (if requested)
            let relatedEntities: any[] = [];
            if (includeRelatedEntityInfo) {
                const relatedEntityNames = new Set(
                        .filter(f => f.RelatedEntity)
                        .map(f => f.RelatedEntity!)
                relatedEntities = Array.from(relatedEntityNames).map(name => ({
                    FieldsReferencingThis: fields
                        .filter(f => f.RelatedEntity === name)
            // Build detailed message
                sampleData,
                totalRowCount,
                primaryKeyFields,
                relatedEntities
                Fields: fields,
                PrimaryKeyFields: primaryKeyFields,
                RelatedEntities: relatedEntities,
                SampleData: sampleData,
                TotalRowCount: totalRowCount,
                SampleRowCount: sampleData.length,
                ResultCode: "DETAILS_FAILED",
                Message: `Entity details retrieval failed: ${errorMessage}`
        entity: any,
        fields: any[],
        sampleData: any[],
        totalRowCount: number,
        primaryKeyFields: string[],
        relatedEntities: any[]
        lines.push(`# Entity Details: ${entity.Name}`);
        lines.push(`\n**Schema:** ${entity.SchemaName}`);
        lines.push(`**Base View:** ${entity.BaseView}`);
        lines.push(`**Virtual Entity:** ${entity.VirtualEntity ? 'Yes' : 'No'}`);
        lines.push(`**Total Rows:** ${totalRowCount.toLocaleString()}`);
        // Primary Keys
        if (primaryKeyFields.length > 0) {
            lines.push(`## Primary Key${primaryKeyFields.length > 1 ? 's' : ''}`);
            lines.push(primaryKeyFields.map(f => `- ${f}`).join('\n'));
        lines.push(`## Fields (${fields.length})\n`);
        for (const field of fields) {
            const parts: string[] = [`- **${field.Name}**`];
            if (field.DisplayName && field.DisplayName !== field.Name) {
                parts.push(`(${field.DisplayName})`);
            parts.push(`\`${field.SQLFullType}\``);
            const attributes: string[] = [];
            if (field.IsPrimaryKey) attributes.push('PK');
            if (field.IsNameField) attributes.push('NAME FIELD');
            if (!field.AllowsNull) attributes.push('NOT NULL');
            if (field.DefaultValue) attributes.push(`Default: ${field.DefaultValue}`);
            if (attributes.length > 0) {
                parts.push(`[${attributes.join(', ')}]`);
            lines.push(parts.join(' '));
                lines.push(`  *${field.Description}*`);
                lines.push(`  → References: **${field.RelatedEntity}**.${field.RelatedEntityFieldName || 'ID'}`);
            if (field.HasValueList) {
                lines.push(`  Allowed Values: ${field.ValueListValues}`);
        // Related Entities
        if (relatedEntities.length > 0) {
            lines.push(`## Related Entities (${relatedEntities.length})\n`);
            for (const rel of relatedEntities) {
                lines.push(`- **${rel.Name}** (via ${rel.FieldsReferencingThis.join(', ')})`);
        // Sample Data
        if (sampleData.length > 0) {
            lines.push(`## Sample Data (${sampleData.length} of ${totalRowCount} total rows)\n`);
            lines.push(JSON.stringify(sampleData, null, 2));
            lines.push(`## Sample Data\n*No data available (empty table)*`);
        lines.push(`\n**Use this information to write accurate SQL queries using the correct field names and types.**`);
