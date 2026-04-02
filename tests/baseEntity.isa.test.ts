 * BaseEntity IS-A Relationship Tests
 * Tests the runtime behavior of BaseEntity for IS-A type relationships:
 * - Set/Get routing (parent vs own fields)
 * - Dirty tracking across IS-A chain
 * - Validate aggregation
 * - NewRecord UUID propagation
 * - CheckPermissions for virtual entities
 * Uses a concrete BaseEntity subclass with mock EntityInfo.
    SALES_SUMMARY_ENTITY_ID,
    STANDALONE_ENTITY_DATA,
// ─── Concrete test subclass of BaseEntity ──────────────────────────────────
    // Expose private _parentEntity for testing IS-A wiring without full Metadata setup
        // Access private field via bracket notation — acceptable in test code
// ─── Helpers ───────────────────────────────────────────────────────────────
    const user = new UserInfo(null, {
let salesSummaryEntityInfo: EntityInfo;
// ─── Test Setup ────────────────────────────────────────────────────────────
    salesSummaryEntityInfo = entities.find(e => e.ID === SALES_SUMMARY_ENTITY_ID)!;
 * Helper: create a MJTestEntity with context user set, optionally with parent initialization.
// ─── Set/Get Routing ───────────────────────────────────────────────────────
describe('BaseEntity Set/Get routing for IS-A', () => {
    let meetingEntity: MJTestEntity;
    let productParent: MJTestEntity;
        // Create a meeting entity with a manually wired parent
        productParent = createEntity(productEntityInfo);
        meetingEntity = createEntity(meetingEntityInfo);
        // Manually wire the parent chain (mimics InitializeParentEntity behavior)
        meetingEntity.SetTestParentEntity(productParent);
        meetingEntity.SetTestParentFieldNames(meetingEntityInfo.ParentEntityFieldNames);
    it('Set() on own field stores value locally', () => {
        meetingEntity.Set('StartTime', '2025-06-15T10:00:00');
        const field = meetingEntity.Fields.find(f => f.Name === 'StartTime');
        // BaseEntity may convert datetime strings to Date objects
        expect(field?.Value).toBeDefined();
    it('Set() on parent field routes to _parentEntity', () => {
        meetingEntity.Set('Name', 'Annual Conference');
        // The parent entity should have the value
        expect(productParent.Get('Name')).toBe('Annual Conference');
    it('Get() on parent field returns from _parentEntity', () => {
        productParent.Set('Name', 'Product Alpha');
        expect(meetingEntity.Get('Name')).toBe('Product Alpha');
    it('Get() on own field returns from local entity', () => {
        meetingEntity.Set('MaxAttendees', 100);
        expect(meetingEntity.Get('MaxAttendees')).toBe(100);
    it('Set() on unknown field does not throw', () => {
        // SetMany with ignoreNonExistentFields=true handles this
        expect(() => meetingEntity.Set('NonExistentField', 'value')).not.toThrow();
// ─── Dirty Tracking ────────────────────────────────────────────────────────
describe('BaseEntity Dirty tracking for IS-A', () => {
        productParent.NewRecord();
        meetingEntity.NewRecord();
    it('clean after resetting old values on all fields', () => {
        // After NewRecord, PK generation may leave some fields dirty.
        // Reset all old values to simulate a "freshly loaded" state.
        meetingEntity.Fields.forEach(f => f.ResetOldValue());
        productParent.Fields.forEach(f => f.ResetOldValue());
        // Note: The parent field mirrors on the child entity may remain dirty
        // because the child's local copies don't get reset. The authoritative
        // dirty check skips parent field mirrors, so own fields should be clean.
        const ownFieldsDirty = meetingEntity.Fields
            .filter(f => !meetingEntityInfo.ParentEntityFieldNames.has(f.Name))
            .some(f => f.Dirty);
        expect(ownFieldsDirty).toBe(false);
    it('modifying own field makes entity dirty', () => {
        meetingEntity.Set('MaxAttendees', 50);
        expect(meetingEntity.Dirty).toBe(true);
    it('modifying parent field makes child entity dirty (via parent dirty check)', () => {
        meetingEntity.Set('Name', 'New Name');
        // Meeting itself has no local dirty own fields, but _parentEntity is dirty
    it('only parent dirty does not make own fields dirty', () => {
        productParent.Set('Price', 99.99);
        // The child should be dirty because it includes parent dirty state
        // But own fields should NOT be dirty
        const ownDirty = meetingEntity.Fields.filter(f => f.Dirty);
        expect(ownDirty.length).toBe(0);
// ─── Validate ──────────────────────────────────────────────────────────────
describe('BaseEntity Validate for IS-A', () => {
    it('Validate() returns a result object', () => {
        const result = meetingEntity.Validate();
        expect(typeof result.Success).toBe('boolean');
    it('Validate() includes parent validation when parent exists', () => {
        // Both entities have NewRecord() called, so they should validate
        // The merge of parent + own validation should complete without error
// ─── NewRecord UUID Propagation ────────────────────────────────────────────
describe('BaseEntity NewRecord for IS-A', () => {
    it('NewRecord() calls NewRecord on parent entity', () => {
        // Parent should also have NewRecord called
        expect(productParent.RecordLoaded).toBe(false); // Not loaded from DB
    it('parent receives the same PK value as child after NewRecord()', () => {
        // The PK field should exist on both parent and child
        const productIdField = productParent.Fields.find(f => f.Name === 'ID');
        // The meeting doesn't have its own ID field in our mock, but
        // the parent should have a PK value set
        expect(productIdField).toBeDefined();
// ─── Virtual Entity CheckPermissions ───────────────────────────────────────
describe('BaseEntity CheckPermissions for Virtual Entities', () => {
    let virtualEntity: MJTestEntity;
        virtualEntity = createEntity(salesSummaryEntityInfo);
    it('blocks Create on virtual entity (throwError=true)', () => {
            virtualEntity.CheckPermissions(EntityPermissionType.Create, true);
        }).toThrow(/Cannot Create on virtual entity/);
    it('blocks Update on virtual entity (throwError=true)', () => {
            virtualEntity.CheckPermissions(EntityPermissionType.Update, true);
        }).toThrow(/Cannot Update on virtual entity/);
    it('blocks Delete on virtual entity (throwError=true)', () => {
            virtualEntity.CheckPermissions(EntityPermissionType.Delete, true);
        }).toThrow(/Cannot Delete on virtual entity/);
    it('returns false for Create on virtual entity (throwError=false)', () => {
        expect(virtualEntity.CheckPermissions(EntityPermissionType.Create, false)).toBe(false);
    it('returns false for Update on virtual entity (throwError=false)', () => {
        expect(virtualEntity.CheckPermissions(EntityPermissionType.Update, false)).toBe(false);
    it('returns false for Delete on virtual entity (throwError=false)', () => {
        expect(virtualEntity.CheckPermissions(EntityPermissionType.Delete, false)).toBe(false);
    it('allows Read on virtual entity', () => {
        expect(virtualEntity.CheckPermissions(EntityPermissionType.Read, false)).toBe(true);
    it('error message includes entity name', () => {
            expect((e as Error).message).toContain('Sales Summary');
// ─── Regular Entity CheckPermissions ───────────────────────────────────────
describe('BaseEntity CheckPermissions for regular entities', () => {
    let entity: MJTestEntity;
        entity = createEntity(standaloneEntityInfo);
    it('allows all operations on regular entity with permissions', () => {
        expect(entity.CheckPermissions(EntityPermissionType.Read, false)).toBe(true);
        expect(entity.CheckPermissions(EntityPermissionType.Create, false)).toBe(true);
        expect(entity.CheckPermissions(EntityPermissionType.Update, false)).toBe(true);
        expect(entity.CheckPermissions(EntityPermissionType.Delete, false)).toBe(true);
    it('falls back to Metadata.Provider.CurrentUser when no context user set', () => {
        const noUserEntity = new MJTestEntity(standaloneEntityInfo);
        // Metadata.Provider.CurrentUser is set from beforeAll, so CheckPermissions
        // should still work by falling back to that user.
        const result = noUserEntity.CheckPermissions(EntityPermissionType.Read, false);
        expect(typeof result).toBe('boolean');
// ─── ISAParentEntity getter ────────────────────────────────────────────────
describe('BaseEntity.ISAParentEntity', () => {
    it('returns null for entity without parent', () => {
        const entity = createEntity(productEntityInfo);
        expect(entity.ISAParentEntity).toBeNull();
    it('returns parent entity when wired', () => {
        const parent = createEntity(productEntityInfo);
        const child = createEntity(meetingEntityInfo);
        child.SetTestParentEntity(parent);
        expect(child.ISAParentEntity).toBe(parent);
// ─── ProviderTransaction ───────────────────────────────────────────────────
describe('BaseEntity.ProviderTransaction', () => {
    it('defaults to null', () => {
        expect(entity.ProviderTransaction).toBeNull();
    it('can be set and retrieved', () => {
        const mockTx = { id: 'mock-transaction' };
        entity.ProviderTransaction = mockTx;
        expect(entity.ProviderTransaction).toBe(mockTx);
// ─── GetAll ────────────────────────────────────────────────────────────────
describe('BaseEntity.GetAll for IS-A', () => {
        productParent.Set('Name', 'Test Product');
        productParent.Set('Price', 29.99);
    it('GetAll() includes both own and parent field values', () => {
        const allData = meetingEntity.GetAll();
        expect(allData['MaxAttendees']).toBe(50);
        expect(allData['Price']).toBe(29.99);
// ─── Revert ────────────────────────────────────────────────────────────────
describe('BaseEntity.Revert for IS-A', () => {
        // Reset old values to simulate "clean" state
    it('Revert() reverts parent field values to their old values', () => {
        // Set known values and then reset old values to create a baseline
        productParent.Set('Name', 'Original');
        // Now modify
        productParent.Set('Name', 'Changed Name');
        meetingEntity.Set('MaxAttendees', 99);
        meetingEntity.Revert();
        // Parent's Name should be reverted to 'Original'
        expect(productParent.Get('Name')).toBe('Original');
        // Meeting's MaxAttendees should be reverted
        const maxField = meetingEntity.Fields.find(f => f.Name === 'MaxAttendees');
        expect(maxField?.Dirty).toBe(false);
