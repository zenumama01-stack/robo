import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, ChangeDetectorRef, NgZone } from '@angular/core';
import { EntityInfo, EntityRelationshipInfo, RunView, Metadata, RunViewParams, EntityFieldValueListType, EntityFieldInfo, CompositeKey } from '@memberjunction/core';
import { buildCompositeKey, buildPkString } from '../utils/record.util';
interface RelatedEntityData {
  relationship: EntityRelationshipInfo;
  relatedEntityName: string;
  isLoadingRecords: boolean;
 * Field display types for categorizing how to render each field
type FieldDisplayType = 'primary-key' | 'foreign-key' | 'enum' | 'regular';
 * Enhanced field display info with type categorization
interface FieldDisplay {
  type: FieldDisplayType;
  // For FK fields - the display name from the virtual/mapped field
  // For FK fields - related entity info for navigation
  relatedRecordId?: string;
 * Event emitted when navigating to a related entity
export interface NavigateToRelatedEvent {
  filter: string;
 * Event emitted when opening a related record
export interface OpenRelatedRecordEvent {
 * Event emitted when opening a foreign key record
export interface OpenForeignKeyRecordEvent {
 * EntityRecordDetailPanelComponent - A reusable panel for displaying entity record details
 * This component provides a detail panel view for entity records with:
 * - Primary key display with copy functionality
 * - Foreign key fields showing friendly names with navigation
 * - Enum fields displayed as pills
 * - Related entities with expandable record lists
 * - Configurable sections for details and relationships
 * <mj-entity-record-detail-panel
 *   [record]="selectedRecord"
 *   (close)="onClosePanel()"
 *   (openRecord)="onOpenRecord($event)"
 *   (navigateToRelated)="onNavigateToRelated($event)">
 * </mj-entity-record-detail-panel>
  selector: 'mj-entity-record-detail-panel',
  templateUrl: './entity-record-detail-panel.component.html',
  styleUrls: ['./entity-record-detail-panel.component.css']
export class EntityRecordDetailPanelComponent implements OnChanges {
  @Input() record: Record<string, unknown> | null = null;
  @Output() openRecord = new EventEmitter<Record<string, unknown>>();
  @Output() navigateToRelated = new EventEmitter<NavigateToRelatedEvent>();
  @Output() openRelatedRecord = new EventEmitter<OpenRelatedRecordEvent>();
  @Output() openForeignKeyRecord = new EventEmitter<OpenForeignKeyRecordEvent>();
  // Related entity counts
  public relatedEntities: RelatedEntityData[] = [];
  public isLoadingRelationships = false;
  // Sections expanded state
  public detailsSectionExpanded = true;
    if (changes['record'] && this.record && this.entity) {
      this.loadRelationshipCounts();
   * Load counts for related entities using batch RunViews call
  private async loadRelationshipCounts(): Promise<void> {
    if (!this.entity || !this.record) return;
    this.isLoadingRelationships = true;
    this.relatedEntities = [];
    // Get relationships where this entity is the related entity (foreign keys pointing TO this record)
    const relationships = this.entity.RelatedEntities;
    if (relationships.length === 0) {
      this.isLoadingRelationships = false;
    // Build a CompositeKey for the current record
    const compositeKey = buildCompositeKey(this.record, this.entity);
    // Get the first PK value for the join field filter
    const pkValue = compositeKey.KeyValuePairs[0]?.Value;
    if (!pkValue) {
    // Build batch query params for all relationships
    const viewParams: RunViewParams[] = relationships.map(rel => ({
      EntityName: rel.RelatedEntity,
      ExtraFilter: `${rel.RelatedEntityJoinField}='${pkValue}'`,
      const results = await rv.RunViews(viewParams);
      // Map results back to relationship data
      this.relatedEntities = relationships.map((rel, index) => {
          relationship: rel,
          relatedEntityName: rel.RelatedEntity,
          count: result.Success ? result.TotalRowCount : 0,
          isExpanded: false,
          isLoadingRecords: false
      console.warn('Failed to load relationship counts:', error);
      // Initialize with zero counts on error
      this.relatedEntities = relationships.map(rel => ({
   * Get key fields to display in details section, categorized by type
  get displayFields(): FieldDisplay[] {
    if (!this.entity || !this.record) return [];
    const fields: FieldDisplay[] = [];
    const excludePatterns = ['__mj_', 'password', 'secret', 'token'];
      // Skip system fields and sensitive fields
      if (excludePatterns.some(p => field.Name.toLowerCase().includes(p))) continue;
      // Skip very long text fields (but not FK fields which are usually GUIDs)
      if (field.Length && field.Length > 500 && !field.RelatedEntityID) continue;
      const value = this.record[field.Name];
      // Handle Primary Key fields specially
      if (field.IsPrimaryKey) {
        fields.push({
          type: 'primary-key',
          label: this.formatFieldLabel(field),
          value: value !== null && value !== undefined ? String(value) : ''
      // Handle Foreign Key fields - show the related record name instead of ID
      if (field.RelatedEntityID && field.RelatedEntityID.length > 0) {
        const fkDisplay = this.buildForeignKeyDisplay(field, value);
        if (fkDisplay) {
          fields.push(fkDisplay);
      // Skip empty values for regular fields
      if (value === null || value === undefined || String(value).trim() === '') continue;
      // Limit regular fields to reasonable number
      if (fields.filter(f => f.type === 'regular' || f.type === 'enum').length >= 10) continue;
      // Check if this field has enumerated values
      const isEnum = field.ValueListTypeEnum !== EntityFieldValueListType.None &&
                     field.EntityFieldValues.length > 0;
        type: isEnum ? 'enum' : 'regular',
        value: this.formatFieldValue(value, field.Name)
   * Build display info for a foreign key field
   * Uses RelatedEntityNameFieldMap to get the human-readable name
   * Label comes from the virtual field's DisplayNameOrName (e.g., "Template" not "Template ID")
  private buildForeignKeyDisplay(field: EntityFieldInfo, value: unknown): FieldDisplay | null {
    if (value === null || value === undefined || String(value).trim() === '') {
    const fkValue = String(value);
    let displayValue = fkValue;
    let label = field.DisplayNameOrName; // Fallback to FK field's label
    // Try to get the display name from the mapped field
    // RelatedEntityNameFieldMap tells us which field contains the name of the related record
    if (field.RelatedEntityNameFieldMap && field.RelatedEntityNameFieldMap.trim().length > 0) {
      const mappedValue = this.record![field.RelatedEntityNameFieldMap];
      if (mappedValue !== null && mappedValue !== undefined && String(mappedValue).trim() !== '') {
        displayValue = String(mappedValue);
      // Use the mapped field's DisplayNameOrName for the label
      const mappedField = this.entity!.Fields.find(f => f.Name === field.RelatedEntityNameFieldMap);
      if (mappedField) {
        label = mappedField.DisplayNameOrName;
      // Fallback: try to find a virtual field with the same name minus "ID" suffix
      // e.g., for "TemplateID", look for "Template" field
      const baseName = field.Name.replace(/ID$/i, '');
      if (baseName !== field.Name) {
        const virtualField = this.entity!.Fields.find(f =>
          f.Name.toLowerCase() === baseName.toLowerCase() && f.IsVirtual
        if (virtualField) {
          const virtualValue = this.record![virtualField.Name];
          if (virtualValue !== null && virtualValue !== undefined && String(virtualValue).trim() !== '') {
            displayValue = String(virtualValue);
          // Use the virtual field's DisplayNameOrName for the label
          label = virtualField.DisplayNameOrName;
      type: 'foreign-key',
      value: fkValue,
      displayValue: displayValue,
      relatedEntityName: field.RelatedEntity || undefined,
      relatedRecordId: fkValue
   * Format field name to display label using EntityFieldInfo's built-in property
  private formatFieldLabel(field: EntityFieldInfo): string {
   * Format field value for display
  private formatFieldValue(value: unknown, fieldName: string): string {
    // Handle dates
    // Handle booleans
    // Handle numbers that look like currency
      const nameLower = fieldName.toLowerCase();
      if (nameLower.includes('amount') ||
          nameLower.includes('price') ||
          nameLower.includes('cost') ||
          nameLower.includes('total') ||
          nameLower.includes('value')) {
        return `$${value.toLocaleString()}`;
    const strValue = String(value);
    // Truncate long strings
    if (strValue.length > 100) {
      return strValue.substring(0, 100) + '...';
    return strValue;
   * Get record title
  get recordTitle(): string {
    if (!this.entity || !this.record) return 'Record';
    if (this.entity.NameField) {
      const name = this.record[this.entity.NameField.Name];
    return buildPkString(this.record, this.entity);
   * Handle close button click
   * Handle open record button click
  onOpenRecord(): void {
      this.openRecord.emit(this.record);
   * Copy primary key value to clipboard
  copyToClipboard(value: string, event: Event): void {
    navigator.clipboard.writeText(value).then(() => {
      console.log('Copied to clipboard:', value);
   * Open a foreign key record (FK link click)
   * Emits openForeignKeyRecord event for parent to handle opening the record
  onForeignKeyClick(field: FieldDisplay, event: Event): void {
    if (field.relatedEntityName && field.relatedRecordId) {
      this.openForeignKeyRecord.emit({
        entityName: field.relatedEntityName,
        recordId: field.relatedRecordId
   * Check if a FK display value is different from the raw ID (i.e., we have a name to show)
  hasFriendlyName(field: FieldDisplay): boolean {
    return field.type === 'foreign-key' &&
           field.displayValue !== undefined &&
           field.displayValue !== field.value;
   * Toggle expansion of related entity section and load records if needed
  async toggleRelatedEntityExpansion(relEntity: RelatedEntityData, event: Event): Promise<void> {
    if (relEntity.count === 0) return;
    relEntity.isExpanded = !relEntity.isExpanded;
    // Load records on first expansion
    if (relEntity.isExpanded && relEntity.records.length === 0 && !relEntity.isLoadingRecords) {
      await this.loadRelatedRecords(relEntity);
   * Load actual records for a related entity
  private async loadRelatedRecords(relEntity: RelatedEntityData): Promise<void> {
    if (!this.record || !this.entity) return;
    if (!pkValue) return;
    relEntity.isLoadingRecords = true;
      // Look up related entity info to compute fields
      const relatedEntityInfo = this.metadata.Entities.find(e => e.Name === relEntity.relationship.RelatedEntity);
      const fields = relatedEntityInfo
        ? [...relatedEntityInfo.PrimaryKeys.map(pk => pk.Name),
           ...(relatedEntityInfo.NameField ? [relatedEntityInfo.NameField.Name] : []),
           ...relatedEntityInfo.Fields.filter(f => f.DefaultInView).map(f => f.Name)]
        EntityName: relEntity.relationship.RelatedEntity,
        ExtraFilter: `${relEntity.relationship.RelatedEntityJoinField}='${pkValue}'`,
        ...(fields ? { Fields: fields } : {}),
        MaxRows: 10 // Limit inline display to 10 records
        relEntity.records = result.Results;
      console.warn(`Failed to load records for ${relEntity.relatedEntityName}:`, error);
        relEntity.isLoadingRecords = false;
   * Handle click on individual related record - opens in new tab
  onRelatedRecordClick(relEntity: RelatedEntityData, record: Record<string, unknown>, event: Event): void {
    this.openRelatedRecord.emit({
      entityName: relEntity.relatedEntityName,
   * Navigate to view all related records (when count > 10)
  onViewAllRelated(relEntity: RelatedEntityData, event: Event): void {
    this.navigateToRelated.emit({
      filter: `${relEntity.relationship.RelatedEntityJoinField}='${pkValue}'`
   * Get display name for a related record
  getRelatedRecordDisplayName(relEntity: RelatedEntityData, record: Record<string, unknown>): string {
    const entityInfo = this.metadata.Entities.find(e => e.Name === relEntity.relatedEntityName);
    if (entityInfo?.NameField) {
      const name = record[entityInfo.NameField.Name];
      return buildPkString(record, entityInfo);
    return 'Record';
   * Get subtitle/secondary info for a related record
  getRelatedRecordSubtitle(relEntity: RelatedEntityData, record: Record<string, unknown>): string {
    if (!entityInfo) return '';
    // Look for common subtitle fields
    const subtitleFieldNames = ['Description', 'Status', 'Type', 'Email', 'Date', 'Amount', 'Total'];
    for (const fieldName of subtitleFieldNames) {
      const field = entityInfo.Fields.find(f =>
        f.Name.includes(fieldName) && f.Name !== entityInfo.NameField?.Name
        const value = record[field.Name];
        if (value !== null && value !== undefined) {
          return this.formatFieldValue(value, field.Name);
   * Toggle section expansion
  toggleSection(section: 'details' | 'relationships'): void {
    if (section === 'details') {
      this.detailsSectionExpanded = !this.detailsSectionExpanded;
   * Get only related entities that have records (count > 0)
  get relatedEntitiesWithRecords(): RelatedEntityData[] {
    return this.relatedEntities.filter(r => r.count > 0);
   * Get icon for related entity by looking up EntityInfo from Metadata
  getRelatedEntityIcon(relEntity: RelatedEntityData): string {
    if (entityInfo?.Icon) {
      return this.formatEntityIcon(entityInfo.Icon);
   * Get the icon class for the current entity
  getEntityIconClass(): string {
    if (!this.entity?.Icon) {
    return this.formatEntityIcon(this.entity.Icon);
