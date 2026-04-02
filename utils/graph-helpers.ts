 * Graph visualization and entity metadata formatting utilities
 * Entity metadata formatted for LLM prompts (concise version)
  FieldCount: number;
  RelatedEntities: Array<{ name: string; type: string }>;
 * Generates a simple text-based relationship graph for LLM prompts
 * Output format:
 * Customers: → Orders, → Addresses
 * Orders: → OrderDetails, → Customers
 * Products: → OrderDetails, → Categories
export function generateRelationshipGraph(entities: EntityInfo[]): string {
    if (entity.RelatedEntities.length === 0) {
      lines.push(`${entity.Name}: (no relationships)`);
    const relations = entity.RelatedEntities
      .map(rel => `→ ${rel.RelatedEntity}`)
    lines.push(`${entity.Name}: ${relations}`);
 * Generates a Mermaid diagram for richer visualization
 * This can be used in prompts that support Mermaid syntax.
 * ```mermaid
 * graph LR
 *   Customers[Customers] --> Orders[Orders]
 *   Orders[Orders] --> OrderDetails[OrderDetails]
export function generateMermaidDiagram(entities: EntityInfo[]): string {
  const lines = ['graph LR'];
  const processedPairs = new Set<string>();
    const safeEntityName = entity.Name.replace(/\s/g, '_');
      const safeRelatedName = rel.RelatedEntity.replace(/\s/g, '_');
      const pairKey = [safeEntityName, safeRelatedName].sort().join('|');
      if (!processedPairs.has(pairKey)) {
        lines.push(`  ${safeEntityName}[${entity.Name}] --> ${safeRelatedName}[${rel.RelatedEntity}]`);
        processedPairs.add(pairKey);
 * Formats entity metadata for LLM prompt (concise version with key info)
 * Extracts only the essential information needed for entity grouping:
 * - Entity name and description
 * - Schema name
 * - Field count (as a proxy for data richness)
 * - Related entities with relationship types
export function formatEntitiesForPrompt(entities: EntityInfo[]): EntityMetadataForPrompt[] {
  return entities.map(entity => ({
    Description: entity.Description || 'No description available',
    SchemaName: entity.SchemaName || 'dbo',
    FieldCount: entity.Fields.length,
    RelatedEntities: entity.RelatedEntities.map(rel => ({
      name: rel.RelatedEntity,
      type: rel.Type
