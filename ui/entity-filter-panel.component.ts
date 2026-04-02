import { EntityInfo } from '@memberjunction/core';
 * Filter configuration for entity filtering.
export interface EntityFilter {
  schemaName: string | null;
  entityStatus: string | null;
  baseTable: string;
 * Entity filter panel component that provides filtering controls for entities.
 * Supports filtering by schema, entity name, base table, and status.
 * This component is designed to be used alongside the ERD diagram to filter
 * which entities are displayed.
  selector: 'mj-entity-filter-panel',
  templateUrl: './entity-filter-panel.component.html',
  styleUrls: ['./entity-filter-panel.component.css']
export class EntityFilterPanelComponent implements OnInit, OnChanges {
  /** All entities available for filtering */
  /** Currently filtered entities (for display count) */
  @Input() filteredEntities: EntityInfo[] = [];
  /** Current filter values */
  @Input() filters: EntityFilter = {
    schemaName: null,
    entityName: '',
    entityStatus: null,
    baseTable: '',
  /** Emitted when any filter value changes */
  @Output() filtersChange = new EventEmitter<EntityFilter>();
  /** Emitted when filter is applied (for debouncing) */
  /** Emitted when reset button is clicked */
  /** Emitted when close button is clicked */
  public distinctSchemas: Array<{ text: string; value: string }> = [];
    this.updateDistinctSchemas();
    if (changes['entities']) {
  private updateDistinctSchemas(): void {
    const schemas = new Set<string>();
    this.entities.forEach(entity => {
      if (entity.SchemaName) {
        schemas.add(entity.SchemaName);
    this.distinctSchemas = Array.from(schemas)
      .sort()
      .map(schema => ({ text: schema, value: schema }));
