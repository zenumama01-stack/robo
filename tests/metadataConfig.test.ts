 * Unit tests for database-metadata-config.json parsing logic.
 * Tests the four extraction methods on ManageMetadataBase:
 *   - extractTablesFromConfig (soft PK/FK for regular tables)
 *   - extractVirtualEntitiesFromConfig (virtual entity definitions)
 *   - extractISARelationshipsFromConfig (IS-A parent-child declarations)
 *   - extractEntitiesFromConfig (Entity-table attribute overrides)
 * Also tests deriveEntityNameFromView and the interaction between sections.
// Mock heavy dependencies that are not needed for config parsing tests
vi.mock('mssql', () => ({}));
   configInfo: {},
   currentWorkingDirectory: '/tmp',
   getSettingValue: vi.fn(),
   mj_core_schema: () => '__mj',
   outputDir: '/tmp',
vi.mock('@memberjunction/core', async (importOriginal) => {
   const actual = await importOriginal<typeof import('@memberjunction/core')>();
      // Override logging to prevent output during tests
vi.mock('@memberjunction/core-entities', async (importOriginal) => {
   const actual = await importOriginal<typeof import('@memberjunction/core-entities')>();
      // Keep real exports, no overrides needed for this test
   logMessage: vi.fn(),
   logStatus: vi.fn(),
vi.mock('../Database/sql', () => ({
   SQLUtilityBase: class {},
vi.mock('../Misc/advanced_generation', () => ({
   AdvancedGeneration: class {},
vi.mock('@memberjunction/global', async (importOriginal) => {
   const actual = await importOriginal<typeof import('@memberjunction/global')>();
      // Keep real exports
vi.mock('uuid', () => ({ v4: vi.fn(() => 'mock-uuid') }));
vi.mock('../Misc/sql_logging', () => ({
   SQLLogging: class {},
   AIEngine: class {},
   ManageMetadataBase,
   SoftPKFKTableConfig,
   VirtualEntityConfig,
   ISARelationshipConfig,
   EntityConfig,
} from '../Database/manage-metadata';
// Test harness: expose protected methods for testing via a thin subclass
class TestableManageMetadata extends ManageMetadataBase {
   public testExtractTables(config: Record<string, unknown>): SoftPKFKTableConfig[] {
      return this.extractTablesFromConfig(config);
   public testExtractVirtualEntities(config: Record<string, unknown>): VirtualEntityConfig[] {
      return this.extractVirtualEntitiesFromConfig(config);
   public testExtractISARelationships(config: Record<string, unknown>): ISARelationshipConfig[] {
      return this.extractISARelationshipsFromConfig(config);
   public testExtractEntities(config: Record<string, unknown>): EntityConfig[] {
      return this.extractEntitiesFromConfig(config);
   public testDeriveEntityName(viewName: string): string {
      return this.deriveEntityNameFromView(viewName);
// Mock data: various config shapes
/** Full config with all three sections */
const fullConfig: Record<string, unknown> = {
   '$schema': './database-metadata-config.schema.json',
   'description': 'Test config with all sections',
   'version': '1.2',
   'VirtualEntities': [
         ViewName: 'vwCustomerOrderSummary',
         SchemaName: 'AdvancedEntities',
         EntityName: 'AE Customer Order Summaries',
         Description: 'Aggregated customer orders',
         PrimaryKey: ['CustomerID'],
         ForeignKeys: [
               FieldName: 'CustomerID',
               RelatedTable: 'Customer',
               RelatedField: 'ID',
               Description: 'FK to Customer',
         ViewName: 'vwSalesDashboard',
         EntityName: 'Sales Dashboard',
         PrimaryKey: ['RegionID'],
   'ISARelationships': [
         ChildEntity: 'AE Meetings',
         ParentEntity: 'AE Products',
         ChildEntity: 'AE Webinars',
         ParentEntity: 'AE Meetings',
         ChildEntity: 'AE Publications',
   'dbo': [
         TableName: 'Orders',
         Description: 'Orders table',
         PrimaryKey: [{ FieldName: 'OrderID', Description: 'PK' }],
               RelatedTable: 'Customers',
               Description: 'FK to Customers',
   'Entities': [
         BaseTable: 'Person',
         AllowMultipleSubtypes: true,
         BaseTable: 'Organization',
   'AdvancedEntities': [
         TableName: 'Product',
         PrimaryKey: [{ FieldName: 'ID' }],
         ForeignKeys: [],
/** Config with only VirtualEntities — no tables, no ISA */
const virtualOnlyConfig: Record<string, unknown> = {
   '$schema': './schema.json',
   'version': '1.1',
         ViewName: 'vwMetrics',
         PrimaryKey: ['MetricID'],
/** Config with only ISARelationships — no tables, no virtual entities */
const isaOnlyConfig: Record<string, unknown> = {
         ChildEntity: 'Meetings',
         ParentEntity: 'Products',
/** Config with only schema-as-key tables — no virtual entities, no ISA */
const tablesOnlyConfig: Record<string, unknown> = {
         TableName: 'Users',
         PrimaryKey: [{ FieldName: 'UserID' }],
         TableName: 'Roles',
         PrimaryKey: [{ FieldName: 'RoleID' }],
   'custom': [
         TableName: 'Widgets',
               FieldName: 'OwnerID',
               RelatedTable: 'Users',
               RelatedField: 'UserID',
/** Config with flat Tables array (legacy format) */
const legacyTablesConfig: Record<string, unknown> = {
   'Tables': [
         TableName: 'LegacyOrders',
         PrimaryKey: [{ FieldName: 'OrderID' }],
         TableName: 'LegacyItems',
         PrimaryKey: [{ FieldName: 'ItemID' }],
               FieldName: 'OrderID',
               RelatedTable: 'LegacyOrders',
               RelatedField: 'OrderID',
/** Empty config — no sections at all */
const emptyConfig: Record<string, unknown> = {
   'version': '1.0',
/** Config with empty arrays for all sections */
const emptyArraysConfig: Record<string, unknown> = {
   'VirtualEntities': [],
   'ISARelationships': [],
   'dbo': [],
/** Config with malformed/unexpected values */
const malformedConfig: Record<string, unknown> = {
   'VirtualEntities': 'not-an-array',
   'ISARelationships': 42,
   'dbo': { not: 'an-array' },
/** Config with VirtualEntities missing optional fields */
const minimalVirtualConfig: Record<string, unknown> = {
      { ViewName: 'vwSimple' },
      { ViewName: 'vwComplex', SchemaName: 'analytics' },
/** Config with ISA missing optional SchemaName */
const minimalISAConfig: Record<string, unknown> = {
      { ChildEntity: 'Webinars', ParentEntity: 'Meetings' },
      { ChildEntity: 'Meetings', ParentEntity: 'Products', SchemaName: 'myschema' },
/** Config combining legacy Tables array with VirtualEntities and ISA */
const mixedFormatConfig: Record<string, unknown> = {
      { SchemaName: 'app', TableName: 'Items', PrimaryKey: [{ FieldName: 'ID' }], ForeignKeys: [] },
      { ViewName: 'vwItemSummary', PrimaryKey: ['ItemID'] },
      { ChildEntity: 'SpecialItems', ParentEntity: 'Items', SchemaName: 'app' },
/** Config with multiple schemas having tables, plus ISA and VirtualEntities */
const multiSchemaConfig: Record<string, unknown> = {
      { ViewName: 'vwReport1', SchemaName: 'reports', PrimaryKey: ['ReportID'] },
      { ChildEntity: 'ChildA', ParentEntity: 'ParentA', SchemaName: 'schema1' },
      { ChildEntity: 'ChildB', ParentEntity: 'ParentB', SchemaName: 'schema2' },
   'schema1': [
      { TableName: 'ParentA', PrimaryKey: [{ FieldName: 'ID' }], ForeignKeys: [] },
   'schema2': [
      { TableName: 'ParentB', PrimaryKey: [{ FieldName: 'ID' }], ForeignKeys: [] },
   'reports': [
      { TableName: 'ReportData', PrimaryKey: [{ FieldName: 'DataID' }], ForeignKeys: [] },
let manager: TestableManageMetadata;
beforeAll(() => {
   manager = new TestableManageMetadata();
// ===== extractTablesFromConfig =====
describe('extractTablesFromConfig', () => {
   test('extracts tables from schema-as-key format', () => {
      const tables = manager.testExtractTables(tablesOnlyConfig);
      expect(tables).toHaveLength(3);
      const dboTables = tables.filter(t => t.SchemaName === 'dbo');
      expect(dboTables).toHaveLength(2);
      expect(dboTables.map(t => t.TableName).sort()).toEqual(['Roles', 'Users']);
      const customTables = tables.filter(t => t.SchemaName === 'custom');
      expect(customTables).toHaveLength(1);
      expect(customTables[0].TableName).toBe('Widgets');
      expect(customTables[0].ForeignKeys).toHaveLength(1);
      expect(customTables[0].ForeignKeys[0].FieldName).toBe('OwnerID');
   test('extracts tables from legacy flat Tables array', () => {
      const tables = manager.testExtractTables(legacyTablesConfig);
      expect(tables).toHaveLength(2);
      expect(tables[0].SchemaName).toBe('dbo');
      expect(tables[0].TableName).toBe('LegacyOrders');
      // Second table has no SchemaName specified — should default to 'dbo'
      expect(tables[1].SchemaName).toBe('dbo');
      expect(tables[1].TableName).toBe('LegacyItems');
   test('skips metadata keys ($schema, description, version)', () => {
      const tables = manager.testExtractTables(fullConfig);
      // Should only get dbo and AdvancedEntities tables, NOT $schema/description/version
      const tableNames = tables.map(t => t.TableName);
      expect(tableNames).toContain('Orders');
      expect(tableNames).toContain('Product');
      expect(tableNames).not.toContain('$schema');
      expect(tableNames).not.toContain('description');
   test('skips VirtualEntities key — does not parse as tables', () => {
      // VirtualEntities array items don't have TableName so should be excluded
      expect(tableNames).not.toContain('vwCustomerOrderSummary');
      expect(tableNames).not.toContain('vwSalesDashboard');
   test('skips ISARelationships key — does not parse as tables', () => {
      expect(tableNames).not.toContain('AE Meetings');
      expect(tableNames).not.toContain('AE Products');
   test('returns empty array for empty config', () => {
      const tables = manager.testExtractTables(emptyConfig);
      expect(tables).toEqual([]);
   test('returns empty array for config with empty arrays', () => {
      const tables = manager.testExtractTables(emptyArraysConfig);
   test('handles malformed config gracefully — non-array values skipped', () => {
      const tables = manager.testExtractTables(malformedConfig);
      // 'dbo' is an object not an array, so it should be skipped
   test('flat Tables format takes priority over schema-as-key', () => {
      // When Tables array is present, schema-as-key entries are NOT parsed
         'Tables': [{ SchemaName: 'x', TableName: 'T1', PrimaryKey: [], ForeignKeys: [] }],
         'dbo': [{ TableName: 'T2', PrimaryKey: [], ForeignKeys: [] }],
      const tables = manager.testExtractTables(config);
      expect(tables).toHaveLength(1);
      expect(tables[0].TableName).toBe('T1');
   test('preserves PrimaryKey and ForeignKeys from schema-as-key format', () => {
      const orders = tables.find(t => t.TableName === 'Orders');
      expect(orders).toBeDefined();
      expect(orders!.PrimaryKey).toHaveLength(1);
      expect(orders!.PrimaryKey[0].FieldName).toBe('OrderID');
      expect(orders!.ForeignKeys).toHaveLength(1);
      expect(orders!.ForeignKeys[0].FieldName).toBe('CustomerID');
      expect(orders!.ForeignKeys[0].RelatedTable).toBe('Customers');
   test('extracts from multiple schemas in full config', () => {
      const tables = manager.testExtractTables(multiSchemaConfig);
      expect(tables.length).toBeGreaterThanOrEqual(3);
      const schemas = [...new Set(tables.map(t => t.SchemaName))].sort();
      expect(schemas).toEqual(['reports', 'schema1', 'schema2']);
// ===== extractVirtualEntitiesFromConfig =====
describe('extractVirtualEntitiesFromConfig', () => {
   test('extracts virtual entities with all fields', () => {
      const ves = manager.testExtractVirtualEntities(fullConfig);
      expect(ves).toHaveLength(2);
      const summary = ves[0];
      expect(summary.ViewName).toBe('vwCustomerOrderSummary');
      expect(summary.SchemaName).toBe('AdvancedEntities');
      expect(summary.EntityName).toBe('AE Customer Order Summaries');
      expect(summary.Description).toBe('Aggregated customer orders');
      expect(summary.PrimaryKey).toEqual(['CustomerID']);
      expect(summary.ForeignKeys).toHaveLength(1);
      expect(summary.ForeignKeys![0].FieldName).toBe('CustomerID');
      expect(summary.ForeignKeys![0].RelatedTable).toBe('Customer');
   test('extracts virtual entity with minimal fields', () => {
      const ves = manager.testExtractVirtualEntities(minimalVirtualConfig);
      expect(ves[0].ViewName).toBe('vwSimple');
      expect(ves[0].SchemaName).toBeUndefined();
      expect(ves[0].EntityName).toBeUndefined();
      expect(ves[0].Description).toBeUndefined();
      expect(ves[0].PrimaryKey).toBeUndefined();
      expect(ves[0].ForeignKeys).toBeUndefined();
      expect(ves[1].ViewName).toBe('vwComplex');
      expect(ves[1].SchemaName).toBe('analytics');
   test('returns empty array when VirtualEntities key is missing', () => {
      const ves = manager.testExtractVirtualEntities(tablesOnlyConfig);
      expect(ves).toEqual([]);
   test('returns empty array when VirtualEntities is empty', () => {
      const ves = manager.testExtractVirtualEntities(emptyArraysConfig);
   test('returns empty array when VirtualEntities is not an array', () => {
      const ves = manager.testExtractVirtualEntities(malformedConfig);
      const ves = manager.testExtractVirtualEntities(emptyConfig);
   test('coexists with ISA and table configs', () => {
      const ves = manager.testExtractVirtualEntities(mixedFormatConfig);
      expect(ves).toHaveLength(1);
      expect(ves[0].ViewName).toBe('vwItemSummary');
   test('multiple virtual entities without ForeignKeys', () => {
      const dashboard = ves[1];
      expect(dashboard.ViewName).toBe('vwSalesDashboard');
      expect(dashboard.EntityName).toBe('Sales Dashboard');
      expect(dashboard.PrimaryKey).toEqual(['RegionID']);
      expect(dashboard.ForeignKeys).toBeUndefined();
// ===== extractISARelationshipsFromConfig =====
describe('extractISARelationshipsFromConfig', () => {
   test('extracts IS-A relationships with all fields', () => {
      const rels = manager.testExtractISARelationships(fullConfig);
      expect(rels).toHaveLength(3);
      expect(rels[0]).toEqual({
      expect(rels[1]).toEqual({
      expect(rels[2]).toEqual({
   test('extracts IS-A relationships with optional SchemaName omitted', () => {
      const rels = manager.testExtractISARelationships(minimalISAConfig);
      expect(rels).toHaveLength(2);
      expect(rels[0].ChildEntity).toBe('Webinars');
      expect(rels[0].ParentEntity).toBe('Meetings');
      expect(rels[0].SchemaName).toBeUndefined();
      expect(rels[1].SchemaName).toBe('myschema');
   test('returns empty array when ISARelationships key is missing', () => {
      const rels = manager.testExtractISARelationships(tablesOnlyConfig);
      expect(rels).toEqual([]);
   test('returns empty array when ISARelationships is empty', () => {
      const rels = manager.testExtractISARelationships(emptyArraysConfig);
   test('returns empty array when ISARelationships is not an array', () => {
      const rels = manager.testExtractISARelationships(malformedConfig);
      const rels = manager.testExtractISARelationships(emptyConfig);
   test('coexists with VirtualEntities and table configs', () => {
      const rels = manager.testExtractISARelationships(mixedFormatConfig);
      expect(rels).toHaveLength(1);
      expect(rels[0].ChildEntity).toBe('SpecialItems');
      expect(rels[0].ParentEntity).toBe('Items');
      expect(rels[0].SchemaName).toBe('app');
   test('only config with ISA works correctly', () => {
      const rels = manager.testExtractISARelationships(isaOnlyConfig);
      expect(rels[0].ChildEntity).toBe('Meetings');
      expect(rels[0].ParentEntity).toBe('Products');
   test('multiple schemas in ISA relationships', () => {
      const rels = manager.testExtractISARelationships(multiSchemaConfig);
      expect(rels[0].SchemaName).toBe('schema1');
      expect(rels[1].SchemaName).toBe('schema2');
// ===== extractEntitiesFromConfig =====
describe('extractEntitiesFromConfig', () => {
   test('extracts entities with attributes from full config', () => {
      const entities = manager.testExtractEntities(fullConfig);
      expect(entities).toHaveLength(2);
      expect(entities[0].BaseTable).toBe('Person');
      expect(entities[0].SchemaName).toBe('AdvancedEntities');
      expect(entities[0].AllowMultipleSubtypes).toBe(true);
      expect(entities[1].BaseTable).toBe('Organization');
      expect(entities[1].SchemaName).toBe('AdvancedEntities');
      expect(entities[1].AllowMultipleSubtypes).toBe(true);
   test('returns empty array when Entities key is missing', () => {
      expect(manager.testExtractEntities(tablesOnlyConfig)).toEqual([]);
   test('returns empty array when Entities is not an array', () => {
      const config: Record<string, unknown> = { Entities: 'not-an-array' };
      expect(manager.testExtractEntities(config)).toEqual([]);
      expect(manager.testExtractEntities(emptyConfig)).toEqual([]);
   test('filters out entries missing BaseTable', () => {
         Entities: [
            { SchemaName: 'dbo', AllowMultipleSubtypes: true },
            { BaseTable: 'Valid', SchemaName: 'dbo', TrackRecordChanges: false },
      const entities = manager.testExtractEntities(config);
      expect(entities).toHaveLength(1);
      expect(entities[0].BaseTable).toBe('Valid');
   test('filters out entries missing SchemaName', () => {
            { BaseTable: 'NoSchema', AllowMultipleSubtypes: true },
            { BaseTable: 'HasSchema', SchemaName: 'dbo' },
      expect(entities[0].BaseTable).toBe('HasSchema');
   test('preserves arbitrary attribute keys', () => {
               BaseTable: 'MyTable',
               SchemaName: 'myschema',
               TrackRecordChanges: false,
               Description: 'Custom description',
      expect(entities[0].TrackRecordChanges).toBe(false);
      expect(entities[0].Description).toBe('Custom description');
   test('returns empty array when Entities array is empty', () => {
      const config: Record<string, unknown> = { Entities: [] };
// ===== deriveEntityNameFromView =====
describe('deriveEntityNameFromView', () => {
   test('strips "vw" prefix and adds spaces', () => {
      expect(manager.testDeriveEntityName('vwCustomerOrderSummary')).toBe('Customer Order Summary');
   test('strips "v_" prefix and adds spaces', () => {
      expect(manager.testDeriveEntityName('v_SalesDashboard')).toBe('Sales Dashboard');
   test('handles single-word view name', () => {
      expect(manager.testDeriveEntityName('vwMetrics')).toBe('Metrics');
   test('handles view name without prefix', () => {
      expect(manager.testDeriveEntityName('SalesReport')).toBe('Sales Report');
   test('handles all-lowercase name', () => {
      expect(manager.testDeriveEntityName('vwsimple')).toBe('simple');
   test('handles already-spaced result', () => {
      expect(manager.testDeriveEntityName('vwABCMetrics')).toBe('ABCMetrics');
// ===== Cross-section interaction tests =====
describe('cross-section interactions', () => {
   test('all four extractors work independently on full config', () => {
      const isas = manager.testExtractISARelationships(fullConfig);
      expect(tables.length).toBeGreaterThan(0);
      expect(ves.length).toBeGreaterThan(0);
      expect(isas.length).toBeGreaterThan(0);
      expect(entities.length).toBeGreaterThan(0);
      // Tables should NOT contain items from VirtualEntities, ISA, or Entities
      for (const ve of ves) {
         expect(tableNames).not.toContain(ve.ViewName);
      for (const isa of isas) {
         expect(tableNames).not.toContain(isa.ChildEntity);
         expect(tableNames).not.toContain(isa.ParentEntity);
      for (const ec of entities) {
         expect(tableNames).not.toContain(ec.BaseTable);
   test('Entities key is not parsed as a schema-as-key table section', () => {
      // "Person" and "Organization" are in the Entities array, not table configs
      expect(tableNames).not.toContain('Person');
      expect(tableNames).not.toContain('Organization');
   test('empty config returns empty from all extractors', () => {
      expect(manager.testExtractTables(emptyConfig)).toEqual([]);
      expect(manager.testExtractVirtualEntities(emptyConfig)).toEqual([]);
      expect(manager.testExtractISARelationships(emptyConfig)).toEqual([]);
   test('malformed config returns empty from all extractors', () => {
      expect(manager.testExtractTables(malformedConfig)).toEqual([]);
      expect(manager.testExtractVirtualEntities(malformedConfig)).toEqual([]);
      expect(manager.testExtractISARelationships(malformedConfig)).toEqual([]);
      expect(manager.testExtractEntities(malformedConfig)).toEqual([]);
   test('mixed format (legacy Tables + VE + ISA) parses all sections', () => {
      const tables = manager.testExtractTables(mixedFormatConfig);
      const isas = manager.testExtractISARelationships(mixedFormatConfig);
      expect(tables[0].TableName).toBe('Items');
      expect(isas).toHaveLength(1);
      expect(isas[0].ChildEntity).toBe('SpecialItems');
   test('multi-schema config with all sections', () => {
      const ves = manager.testExtractVirtualEntities(multiSchemaConfig);
      const isas = manager.testExtractISARelationships(multiSchemaConfig);
      // 3 tables across 3 schemas
      // 1 virtual entity
      expect(ves[0].SchemaName).toBe('reports');
      // 2 ISA relationships
      expect(isas).toHaveLength(2);
   test('virtual-only config has no tables, ISA, or entities', () => {
      expect(manager.testExtractTables(virtualOnlyConfig)).toEqual([]);
      expect(manager.testExtractVirtualEntities(virtualOnlyConfig)).toHaveLength(1);
      expect(manager.testExtractISARelationships(virtualOnlyConfig)).toEqual([]);
      expect(manager.testExtractEntities(virtualOnlyConfig)).toEqual([]);
   test('ISA-only config has no tables, virtual entities, or entities', () => {
      expect(manager.testExtractTables(isaOnlyConfig)).toEqual([]);
      expect(manager.testExtractVirtualEntities(isaOnlyConfig)).toEqual([]);
      expect(manager.testExtractISARelationships(isaOnlyConfig)).toHaveLength(1);
      expect(manager.testExtractEntities(isaOnlyConfig)).toEqual([]);
   test('tables-only config has no virtual entities, ISA, or entities', () => {
      expect(manager.testExtractTables(tablesOnlyConfig).length).toBeGreaterThan(0);
      expect(manager.testExtractVirtualEntities(tablesOnlyConfig)).toEqual([]);
      expect(manager.testExtractISARelationships(tablesOnlyConfig)).toEqual([]);
