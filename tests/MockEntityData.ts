 * Mock Entity Data for Testing IS-A Relationships and Virtual Entities
 * Provides pre-built EntityInfo init data representing a realistic IS-A hierarchy:
 *   Products (root parent)
 *     │     └── Webinars (child of Meetings — 3-level chain)
 * Also includes a Virtual Entity (Sales Summary) for testing read-only enforcement.
// ─── Shared role / permission scaffolding ──────────────────────────────────
export const MOCK_ROLE_ID = 'role-admin-001';
export const MOCK_USER_ID = 'user-test-001';
function makePermission(entityID: string): Record<string, unknown> {
        ID: `perm-${entityID}`,
        EntityID: entityID,
        CanCreate: true,
        CanUpdate: true,
// ─── Products (root parent) ────────────────────────────────────────────────
export const PRODUCT_ENTITY_ID = 'entity-products-001';
export const PRODUCT_FIELDS = [
        ID: 'f-prod-id',
        EntityID: PRODUCT_ENTITY_ID,
        Name: 'ID',
        IsPrimaryKey: true,
        IsSoftPrimaryKey: false,
        IsSoftForeignKey: false,
        IsNameField: false,
        AllowUpdateAPI: false,
        ValueListType: 'None',
        Sequence: 1,
        Entity: 'Products',
        ID: 'f-prod-name',
        Length: 255,
        IsNameField: true,
        Sequence: 2,
        ID: 'f-prod-price',
        Type: 'decimal',
        Sequence: 3,
export const PRODUCT_ENTITY_DATA = {
    ID: PRODUCT_ENTITY_ID,
    Name: 'Products',
    BaseTable: 'Product',
    BaseView: 'vwProducts',
    VirtualEntity: false,
    IncludeInAPI: true,
    EntityFields: PRODUCT_FIELDS,
    EntityPermissions: [makePermission(PRODUCT_ENTITY_ID)],
    EntityRelationships: [],
    EntitySettings: [],
// ─── Meetings (child of Products) ──────────────────────────────────────────
export const MEETING_ENTITY_ID = 'entity-meetings-001';
export const MEETING_FIELDS = [
        ID: 'f-meet-starttime',
        EntityID: MEETING_ENTITY_ID,
        Type: 'datetime2',
        Entity: 'Meetings',
        ID: 'f-meet-maxattendees',
        Name: 'MaxAttendees',
        Type: 'int',
export const MEETING_ENTITY_DATA = {
    ID: MEETING_ENTITY_ID,
    Name: 'Meetings',
    BaseTable: 'Meeting',
    BaseView: 'vwMeetings',
    ParentID: PRODUCT_ENTITY_ID,
    EntityFields: MEETING_FIELDS,
    EntityPermissions: [makePermission(MEETING_ENTITY_ID)],
// ─── Webinars (child of Meetings — 3-level IS-A chain) ────────────────────
export const WEBINAR_ENTITY_ID = 'entity-webinars-001';
export const WEBINAR_FIELDS = [
        ID: 'f-web-platformurl',
        EntityID: WEBINAR_ENTITY_ID,
        Name: 'PlatformURL',
        Length: 500,
        Entity: 'Webinars',
        ID: 'f-web-isrecorded',
        Name: 'IsRecorded',
        Type: 'bit',
        DefaultValue: '0',
export const WEBINAR_ENTITY_DATA = {
    ID: WEBINAR_ENTITY_ID,
    Name: 'Webinars',
    BaseTable: 'Webinar',
    BaseView: 'vwWebinars',
    ParentID: MEETING_ENTITY_ID,
    EntityFields: WEBINAR_FIELDS,
    EntityPermissions: [makePermission(WEBINAR_ENTITY_ID)],
// ─── Publications (child of Products) ──────────────────────────────────────
export const PUBLICATION_ENTITY_ID = 'entity-publications-001';
export const PUBLICATION_FIELDS = [
        ID: 'f-pub-isbn',
        EntityID: PUBLICATION_ENTITY_ID,
        Name: 'ISBN',
        Length: 20,
        Entity: 'Publications',
export const PUBLICATION_ENTITY_DATA = {
    ID: PUBLICATION_ENTITY_ID,
    Name: 'Publications',
    BaseTable: 'Publication',
    BaseView: 'vwPublications',
    EntityFields: PUBLICATION_FIELDS,
    EntityPermissions: [makePermission(PUBLICATION_ENTITY_ID)],
// ─── Virtual Entity: Sales Summary ─────────────────────────────────────────
export const SALES_SUMMARY_ENTITY_ID = 'entity-sales-summary-001';
export const SALES_SUMMARY_FIELDS = [
        ID: 'f-ss-summaryid',
        EntityID: SALES_SUMMARY_ENTITY_ID,
        Name: 'SummaryID',
        IsSoftPrimaryKey: true,
        Entity: 'Sales Summary',
        ID: 'f-ss-regionname',
        Name: 'RegionName',
        ID: 'f-ss-totalsales',
        Name: 'TotalSales',
export const SALES_SUMMARY_ENTITY_DATA = {
    ID: SALES_SUMMARY_ENTITY_ID,
    Name: 'Sales Summary',
    BaseTable: 'vwSalesSummary',
    BaseView: 'vwSalesSummary',
    VirtualEntity: true,
    AllowCreateAPI: false,
    AllowDeleteAPI: false,
    EntityFields: SALES_SUMMARY_FIELDS,
    EntityPermissions: [makePermission(SALES_SUMMARY_ENTITY_ID)],
// ─── Standalone non-IS-A entity (for comparison tests) ────────────────────
export const STANDALONE_ENTITY_ID = 'entity-standalone-001';
export const STANDALONE_ENTITY_DATA = {
    ID: STANDALONE_ENTITY_ID,
    Name: 'Standalone Items',
    BaseTable: 'StandaloneItem',
    BaseView: 'vwStandaloneItems',
    EntityFields: [
            ID: 'f-sa-id',
            EntityID: STANDALONE_ENTITY_ID,
            Entity: 'Standalone Items',
            ID: 'f-sa-name',
    EntityPermissions: [makePermission(STANDALONE_ENTITY_ID)],
// ─── Overlapping Subtype Hierarchy ───────────────────────────────────────
// Persons (root parent, AllowMultipleSubtypes = true)
//   ├── Members (child of Persons)
//   └── Volunteers (child of Persons)
// This hierarchy tests overlapping subtypes where a single Person record can
// have BOTH a Member and a Volunteer child record simultaneously.
export const PERSON_ENTITY_ID = 'entity-persons-001';
export const PERSON_FIELDS = [
        ID: 'f-person-id',
        EntityID: PERSON_ENTITY_ID,
        Entity: 'Persons',
        ID: 'f-person-name',
        ID: 'f-person-email',
export const PERSON_ENTITY_DATA = {
    ID: PERSON_ENTITY_ID,
    Name: 'Persons',
    BaseView: 'vwPersons',
    EntityFields: PERSON_FIELDS,
    EntityPermissions: [makePermission(PERSON_ENTITY_ID)],
// ─── Members (child of Persons) ──────────────────────────────────────────
export const MEMBER_ENTITY_ID = 'entity-members-001';
export const MEMBER_FIELDS = [
        ID: 'f-member-level',
        EntityID: MEMBER_ENTITY_ID,
        Name: 'MembershipLevel',
        Length: 50,
        Entity: 'Members',
export const MEMBER_ENTITY_DATA = {
    ID: MEMBER_ENTITY_ID,
    Name: 'Members',
    BaseTable: 'Member',
    BaseView: 'vwMembers',
    ParentID: PERSON_ENTITY_ID,
    EntityFields: MEMBER_FIELDS,
    EntityPermissions: [makePermission(MEMBER_ENTITY_ID)],
// ─── Volunteers (child of Persons) ───────────────────────────────────────
export const VOLUNTEER_ENTITY_ID = 'entity-volunteers-001';
export const VOLUNTEER_FIELDS = [
        ID: 'f-volunteer-area',
        EntityID: VOLUNTEER_ENTITY_ID,
        Name: 'VolunteerArea',
        Entity: 'Volunteers',
export const VOLUNTEER_ENTITY_DATA = {
    ID: VOLUNTEER_ENTITY_ID,
    Name: 'Volunteers',
    BaseTable: 'Volunteer',
    BaseView: 'vwVolunteers',
    EntityFields: VOLUNTEER_FIELDS,
    EntityPermissions: [makePermission(VOLUNTEER_ENTITY_ID)],
 * All entity data arrays — use this to set up a mock Metadata.Provider.Entities
 * that has the full hierarchy available for ParentEntityInfo / ChildEntities lookups.
export const ALL_ENTITY_DATA = [
    SALES_SUMMARY_ENTITY_DATA,
    PERSON_ENTITY_DATA,
    MEMBER_ENTITY_DATA,
    VOLUNTEER_ENTITY_DATA,
