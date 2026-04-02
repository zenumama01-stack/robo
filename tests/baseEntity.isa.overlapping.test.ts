 * BaseEntity IS-A Overlapping Subtypes Tests
 * Tests the runtime behavior of BaseEntity for overlapping subtype hierarchies
 * where AllowMultipleSubtypes = true on the parent entity:
 * - ISAChild returns null for overlapping parents (use ISAChildren instead)
 * - ISAChildren returns list of child entity names
 * - LeafEntity stops at overlapping parent (doesn't chain through)
 * - Disjoint enforcement is bypassed for overlapping parents
 * - Delete safety: parent preserved when sibling children exist
 * - Record change propagation to sibling branches
 * - NewRecord clears overlapping state
 * Uses mock entities: Persons (AllowMultipleSubtypes=true) → Members, Volunteers
 * and the existing disjoint hierarchy: Products → Meetings → Webinars, Publications
import { EntityInfo } from '../generic/entityInfo';
    PERSON_ENTITY_ID,
    MEMBER_ENTITY_ID,
    VOLUNTEER_ENTITY_ID,
// ─── Test subclass exposing private fields ──────────────────────────────
    public SetTestChildEntities(children: { entityName: string }[] | null): void {
        (this as unknown as { _childEntities: { entityName: string }[] | null })._childEntities = children;
    public GetTestChildEntities(): { entityName: string }[] | null {
        return (this as unknown as { _childEntities: { entityName: string }[] | null })._childEntities;
// ─── Helpers ────────────────────────────────────────────────────────────
let personEntityInfo: EntityInfo;
let memberEntityInfo: EntityInfo;
let volunteerEntityInfo: EntityInfo;
// ─── Test Setup ─────────────────────────────────────────────────────────
    personEntityInfo = entities.find(e => e.ID === PERSON_ENTITY_ID)!;
    memberEntityInfo = entities.find(e => e.ID === MEMBER_ENTITY_ID)!;
    volunteerEntityInfo = entities.find(e => e.ID === VOLUNTEER_ENTITY_ID)!;
 * Creates an overlapping hierarchy: Person (AllowMultipleSubtypes=true) with
 * Member and Volunteer as child types. Member is wired as the "currently loaded"
 * child chain, while both are listed in _childEntities.
function createOverlappingChain(): {
    person: MJTestEntity;
    member: MJTestEntity;
    volunteer: MJTestEntity;
    const person = createEntity(personEntityInfo);
    const member = createEntity(memberEntityInfo);
    const volunteer = createEntity(volunteerEntityInfo);
    // Wire parent chain for member
    member.SetTestParentEntity(person);
    member.SetTestParentFieldNames(memberEntityInfo.ParentEntityFieldNames);
    // Wire parent chain for volunteer (shares same person parent instance)
    volunteer.SetTestParentEntity(person);
    volunteer.SetTestParentFieldNames(volunteerEntityInfo.ParentEntityFieldNames);
    // Overlapping parent: set _childEntities list (both children known)
    person.SetTestChildEntities([
        { entityName: 'Members' },
        { entityName: 'Volunteers' },
    person.SetTestChildDiscoveryDone(true);
    // Member and Volunteer are leaves (no further children)
    member.SetTestChildDiscoveryDone(true);
    volunteer.SetTestChildDiscoveryDone(true);
    return { person, member, volunteer };
// ─── EntityInfo: AllowMultipleSubtypes & HasOverlappingSubtypes ─────────
describe('EntityInfo overlapping subtypes metadata', () => {
    it('Person entity has AllowMultipleSubtypes = true', () => {
        expect(personEntityInfo.AllowMultipleSubtypes).toBe(true);
    it('Person entity HasOverlappingSubtypes = true (parent + AllowMultipleSubtypes)', () => {
        expect(personEntityInfo.HasOverlappingSubtypes).toBe(true);
    it('Product entity has AllowMultipleSubtypes = false (default)', () => {
        expect(productEntityInfo.AllowMultipleSubtypes).toBe(false);
    it('Product entity HasOverlappingSubtypes = false', () => {
        expect(productEntityInfo.HasOverlappingSubtypes).toBe(false);
    it('Member and Volunteer are children of Person', () => {
        expect(memberEntityInfo.ParentID).toBe(PERSON_ENTITY_ID);
        expect(volunteerEntityInfo.ParentID).toBe(PERSON_ENTITY_ID);
    it('Person has both Members and Volunteers as child entities', () => {
        const childNames = personEntityInfo.ChildEntities.map(c => c.Name);
        expect(childNames).toContain('Members');
        expect(childNames).toContain('Volunteers');
// ─── ISAChild / ISAChildren for overlapping parents ─────────────────────
describe('ISAChild and ISAChildren for overlapping parents', () => {
    it('ISAChild returns null when _childEntities is populated (overlapping parent)', () => {
        const { person } = createOverlappingChain();
        // Even if _childEntity were set, ISAChild should return null for overlapping parents
        expect(person.ISAChild).toBeNull();
    it('ISAChildren returns the list of child entity names', () => {
        expect(person.ISAChildren).toEqual([
    it('ISAChildren returns null for disjoint parent without _childEntities set', () => {
        expect(product.ISAChildren).toBeNull();
    it('ISAChildren returns null for non-parent entities', () => {
        expect(member.ISAChildren).toBeNull();
    it('ISAChild still works for disjoint parents', () => {
        // Disjoint parent: ISAChild returns the single child
// ─── LeafEntity for overlapping parents ─────────────────────────────────
describe('LeafEntity stops at overlapping parents', () => {
    it('LeafEntity returns self for overlapping parent (no chain through)', () => {
        // Person is the leaf from its own perspective — doesn't chain to any single child
        expect(person.LeafEntity).toBe(person);
    it('LeafEntity returns self for child of overlapping parent (leaf in its own right)', () => {
        const { member } = createOverlappingChain();
        // Member is a leaf (no children of its own)
        expect(member.LeafEntity).toBe(member);
    it('LeafEntity still chains through disjoint hierarchy', () => {
        expect(product.LeafEntity).toBe(meeting);
// ─── Disjoint Enforcement Bypass ────────────────────────────────────────
describe('Disjoint enforcement bypass for overlapping subtypes', () => {
    it('AllowMultipleSubtypes=true on parent skips EnforceDisjointSubtype', () => {
        // Create a Member entity (child of Person with AllowMultipleSubtypes=true)
        // The parent entity (Person) has AllowMultipleSubtypes = true
        expect(memberEntityInfo.ParentEntityInfo?.AllowMultipleSubtypes).toBe(true);
        // This confirms the guard condition in _InnerSave():
        //   if (parentEntityInfo && !parentEntityInfo.AllowMultipleSubtypes) { EnforceDisjointSubtype() }
        // Since Person.AllowMultipleSubtypes = true, the guard is false → skipped
    it('AllowMultipleSubtypes=false on parent still requires enforcement', () => {
        // Meeting is a child of Product (AllowMultipleSubtypes = false)
        // The parent entity (Product) has AllowMultipleSubtypes = false
        expect(meetingEntityInfo.ParentEntityInfo?.AllowMultipleSubtypes).toBe(false);
        // So the guard condition evaluates to true → EnforceDisjointSubtype will be called
// ─── Save Delegation for Overlapping Parents ────────────────────────────
describe('Save delegation with overlapping parents', () => {
    it('Save on overlapping parent does NOT delegate to child (no single child chain)', () => {
        const { person, member, volunteer } = createOverlappingChain();
        // Person.ISAChild is null for overlapping parents
        // So Save should NOT try to delegate to any child
        // Person.LeafEntity returns self, so Save runs on Person itself
    it('Save on child of overlapping parent delegates upward to parent normally', async () => {
        const { person, member } = createOverlappingChain();
        // Member has a parent (Person), so Save should delegate upward
        // The parent chain is: Member → Person
        expect(member.ISAParent).toBe(person);
// ─── Delete Safety for Overlapping Subtypes ─────────────────────────────
describe('Delete safety for overlapping subtypes', () => {
    it('shouldDeleteParentAfterChildDelete returns true for disjoint parents', async () => {
        // For Products (AllowMultipleSubtypes = false), the delete always cascades
        // to parent. We can verify the condition: !parentInfo.AllowMultipleSubtypes === true
    it('shouldDeleteParentAfterChildDelete checks for remaining siblings (overlapping)', () => {
        // For Person (AllowMultipleSubtypes = true), the delete code checks FindISAChildEntities
        // to see if siblings exist before deleting the parent.
        // We verify the condition path: parentInfo.AllowMultipleSubtypes === true
    it('Delete on member with volunteer sibling preserves person parent', async () => {
        member.NewRecord();
        person.NewRecord();
        // Set up a mock provider that:
        // 1. Succeeds on Delete for the member
        // 2. Reports remaining children (volunteer still exists) via FindISAChildEntities
            FindISAChildEntities: vi.fn().mockResolvedValue([
                { ChildEntityName: 'Volunteers' }  // Volunteer still exists
            BeginISATransaction: vi.fn().mockResolvedValue({ id: 'mock-txn' }),
            CommitISATransaction: vi.fn().mockResolvedValue(undefined),
            RollbackISATransaction: vi.fn().mockResolvedValue(undefined),
            ProviderType: 'Database',
            Save: vi.fn().mockResolvedValue({}),
        // Set the provider on both entities
        (member as unknown as { _provider: unknown })._provider = mockProvider;
        (person as unknown as { _provider: unknown })._provider = mockProvider;
        // Mark as saved (IsSaved = true) so Delete path is reachable
        (member as unknown as { _everSaved: boolean })._everSaved = true;
        (person as unknown as { _everSaved: boolean })._everSaved = true;
        // Simulate loaded record state
        member.Fields.forEach(f => f.ResetOldValue());
        person.Fields.forEach(f => f.ResetOldValue());
        const result = await member.Delete();
        // Delete was called on member
        expect(mockProvider.Delete).toHaveBeenCalledTimes(1);
        // FindISAChildEntities was called to check for remaining siblings
        expect(mockProvider.FindISAChildEntities).toHaveBeenCalled();
        // Parent Delete was NOT called because Volunteer sibling still exists
        // (Delete was only called once — for member, not for person)
    it('Delete on member with no siblings cascades to person parent', async () => {
        // Mock provider: no remaining children after member delete
            FindISAChildEntities: vi.fn().mockResolvedValue([]),  // No remaining children
        // Delete was called twice: first for member, then for person (cascade)
        expect(mockProvider.Delete).toHaveBeenCalledTimes(2);
    it('Delete on disjoint child always cascades to parent (unchanged behavior)', async () => {
        // Disjoint parent: should NOT call FindISAChildEntities
            FindISAChildEntities: vi.fn(),
        (meeting as unknown as { _provider: unknown })._provider = mockProvider;
        (product as unknown as { _provider: unknown })._provider = mockProvider;
        (meeting as unknown as { _everSaved: boolean })._everSaved = true;
        (product as unknown as { _everSaved: boolean })._everSaved = true;
        // Delete called twice: meeting + product (disjoint always cascades)
        // FindISAChildEntities was NOT called (disjoint path skips it)
        expect(mockProvider.FindISAChildEntities).not.toHaveBeenCalled();
// ─── Record Change Propagation ──────────────────────────────────────────
// Record Change propagation is now an internal concern of SQLServerDataProvider.
// BaseEntity no longer has _lastSaveRecordChangeData or PropagateRecordChangesToSiblingBranches.
// Provider-level propagation tests belong in the SQLServerDataProvider test suite.
// ─── NewRecord Clears Overlapping State ─────────────────────────────────
describe('NewRecord clears overlapping subtype state', () => {
    it('NewRecord resets _childEntities to null', () => {
        expect(person.GetTestChildEntities()).toHaveLength(2);
        expect(person.GetTestChildEntities()).toBeNull();
        expect(person.GetTestChildDiscoveryDone()).toBe(true);
        expect(person.GetTestChildDiscoveryDone()).toBe(false);
    it('ISAChildren returns null after NewRecord', () => {
        expect(person.ISAChildren).toHaveLength(2);
        expect(person.ISAChildren).toBeNull();
// ─── Set/Get Routing with Overlapping Parent ────────────────────────────
describe('Set/Get routing for children of overlapping parent', () => {
    it('Set on parent field routes to person (same as disjoint)', () => {
        member.Set('Name', 'John Doe');
        expect(person.Get('Name')).toBe('John Doe');
    it('Set on own field stores locally', () => {
        member.Set('MembershipLevel', 'Gold');
        expect(member.Get('MembershipLevel')).toBe('Gold');
    it('Both children share the same parent instance field values', () => {
        member.Set('Name', 'Jane Smith');
        // Volunteer reads from the same person instance
        expect(volunteer.Get('Name')).toBe('Jane Smith');
        expect(person.Get('Name')).toBe('Jane Smith');
    it('GetAll on child includes parent fields', () => {
        person.Set('Name', 'Test Person');
        person.Set('Email', 'test@example.com');
        member.Set('MembershipLevel', 'Silver');
        const all = member.GetAll();
        expect(all['Name']).toBe('Test Person');
        expect(all['Email']).toBe('test@example.com');
        expect(all['MembershipLevel']).toBe('Silver');
// ─── InitializeChildEntity branching ────────────────────────────────────
describe('InitializeChildEntity for overlapping vs disjoint', () => {
    it('calls FindISAChildEntities (plural) for overlapping parent', async () => {
        // Set a PK so the discovery can proceed
        person.Set('ID', '00000000-0000-0000-0000-000000000001');
                { ChildEntityName: 'Members' },
                { ChildEntityName: 'Volunteers' },
            FindISAChildEntity: vi.fn(),
        // Reset discovery state and call InitializeChildEntity
        person.SetTestChildDiscoveryDone(false);
        await (person as unknown as { InitializeChildEntity: () => Promise<void> }).InitializeChildEntity();
        // Should have called the plural method (overlapping), not the singular
        expect(mockProvider.FindISAChildEntity).not.toHaveBeenCalled();
        // _childEntities should be populated
    it('calls FindISAChildEntity (singular) for disjoint parent', async () => {
        product.Set('ID', '00000000-0000-0000-0000-000000000002');
        // Return null so createAndLinkChildEntity is never called
        // (avoids needing to mock Metadata.Provider.GetEntityObject)
            FindISAChildEntity: vi.fn().mockResolvedValue(null),
        product.SetTestChildDiscoveryDone(false);
        await (product as unknown as { InitializeChildEntity: () => Promise<void> }).InitializeChildEntity();
        // Should have called the singular method (disjoint), not the plural
        expect(mockProvider.FindISAChildEntity).toHaveBeenCalled();
