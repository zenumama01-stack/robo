  ChangeDetectionStrategy, ChangeDetectorRef, inject,
  OnInit, OnChanges, SimpleChanges, ViewEncapsulation
  BaseEntity, EntityInfo, EntityFieldInfo, Metadata, CompositeKey
import { EntityHierarchyNavigationEvent } from '../types/navigation-events';
import { IsaRelatedItem } from './isa-hierarchy-utils';
 * Represents a single field value displayed in the ISA related card.
export interface IsaCardFieldDisplay {
  Value: string;
  IsBooleanTrue?: boolean;
  IsBooleanFalse?: boolean;
 * Compact card showing summary fields for one IS-A related entity record
 * (sibling or child). Displays DefaultInView fields by default with
 * expand/collapse for remaining fields.
 * Used inside `<mj-isa-related-panel>` to show related IS-A records
 * alongside the main form.
  selector: 'mj-isa-related-card',
  templateUrl: './isa-related-card.component.html',
  styleUrls: ['./isa-related-card.component.css']
export class MjIsaRelatedCardComponent implements OnInit, OnChanges {
  /** The entity name for the related record (e.g., "Members", "Speakers") */
  @Input() EntityName = '';
  /** The primary key shared with the parent record */
  @Input() PrimaryKey: CompositeKey | null = null;
  /** The record name of the current form's entity (to suppress if same) */
  @Input() CurrentRecordName = '';
  /** The relationship type label */
  @Input() Relationship: 'sibling' | 'child' = 'sibling';
  /** Depth in the IS-A hierarchy (0 = direct child/sibling, 1 = grandchild, etc.) */
  @Input() Depth = 0;
  /** Child items nested under this entity (rendered as sub-cards within this card) */
  @Input() Children: IsaRelatedItem[] = [];
  /** Emitted when user clicks "Open" to navigate to this related record */
  @Output() Navigate = new EventEmitter<EntityHierarchyNavigationEvent>();
  EntityInfoRef: EntityInfo | null = null;
  RelatedRecord: BaseEntity | null = null;
  LoadError = false;
  Expanded = false;
  DefaultFields: IsaCardFieldDisplay[] = [];
  ExtraFields: IsaCardFieldDisplay[] = [];
  /** Display name for the entity */
    return this.EntityInfoRef?.DisplayNameOrName ?? this.EntityName;
  /** Icon class from entity metadata */
  get IconClass(): string {
    return this.EntityInfoRef?.Icon ?? 'fa-solid fa-link';
  /** Record name of the related record (suppressed if same as current) */
  get RecordName(): string {
    if (!this.RelatedRecord) return '';
    const name = this.RelatedRecord.GetRecordName();
    if (!name) return '';
    const nameStr = String(name);
    // Suppress if same as the current form's record name
    if (nameStr === this.CurrentRecordName) return '';
    return nameStr;
    this.LoadRelatedRecord();
    if ((changes['EntityName'] || changes['PrimaryKey']) && !changes['EntityName']?.firstChange) {
  /** Load the related entity record and extract field data */
  async LoadRelatedRecord(): Promise<void> {
    if (!this.EntityName || !this.PrimaryKey) return;
    this.LoadError = false;
      this.EntityInfoRef = md.EntityByName(this.EntityName);
      if (!this.EntityInfoRef) {
        this.LoadError = true;
      const entity = await md.GetEntityObject<BaseEntity>(this.EntityName);
      const loaded = await entity.InnerLoad(this.PrimaryKey);
      this.RelatedRecord = entity;
      this.BuildFieldDisplayLists();
  /** Split fields into DefaultInView (shown by default) and extra (hidden) */
  private BuildFieldDisplayLists(): void {
    if (!this.RelatedRecord || !this.EntityInfoRef) return;
    const parentFieldNames = this.EntityInfoRef.ParentEntityFieldNames;
    const ownFields = this.EntityInfoRef.Fields.filter(f =>
      !f.IsVirtual &&
      !parentFieldNames.has(f.Name) &&
      f.Name !== '__mj_CreatedAt' &&
      f.Name !== '__mj_UpdatedAt' &&
      f.IncludeInGeneratedForm
    const defaultFields: IsaCardFieldDisplay[] = [];
    const extraFields: IsaCardFieldDisplay[] = [];
    for (const field of ownFields) {
      const display = this.BuildFieldDisplay(field);
      if (field.DefaultInView) {
        defaultFields.push(display);
        extraFields.push(display);
    this.DefaultFields = defaultFields;
    this.ExtraFields = extraFields;
  /** Create a display representation of a single field value */
  private BuildFieldDisplay(field: EntityFieldInfo): IsaCardFieldDisplay {
    const rawValue = this.RelatedRecord!.Get(field.Name);
    const label = field.DisplayNameOrName;
    if (field.TSType === 'boolean') {
        Label: label,
        Value: rawValue ? 'Yes' : 'No',
        IsBooleanTrue: !!rawValue,
        IsBooleanFalse: !rawValue
    // Format the value for display
      Value: this.FormatFieldValue(rawValue, field)
  /** Format a field value for compact display */
  private FormatFieldValue(value: unknown, field: EntityFieldInfo): string {
    // Date formatting
      return value.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    // Number formatting
      if (field.ExtendedType === 'money' || field.Type?.toLowerCase().includes('money')) {
        return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  /** Toggle expanded state to show/hide extra fields */
  ToggleExpand(): void {
    this.Expanded = !this.Expanded;
  /** Handle "Open" link click — emit navigation event */
  OnOpenClick(event: MouseEvent): void {
    if (!this.PrimaryKey) return;
      Kind: 'entity-hierarchy',
      PrimaryKey: this.PrimaryKey,
      Direction: this.Relationship === 'sibling' ? 'child' : 'child'
