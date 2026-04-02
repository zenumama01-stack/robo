import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, ViewChild, ElementRef } from '@angular/core';
import { EntityInfo, EntityFieldInfo, EntityFieldValueInfo, Metadata } from '@memberjunction/core';
export interface EntityDetailsOpenRecordEvent {
 * Entity details panel component that displays detailed information about a selected entity.
 * Shows entity metadata, fields with filtering capabilities, and related entities.
 * This component is designed to be used alongside the ERD diagram to provide
 * a detailed view of the currently selected entity.
  selector: 'mj-entity-details',
  templateUrl: './entity-details.component.html',
  styleUrls: ['./entity-details.component.css']
export class EntityDetailsComponent implements OnChanges {
  @ViewChild('fieldsListContainer', { static: false }) fieldsListContainer!: ElementRef;
  @ViewChild('relationshipsListContainer', { static: false }) relationshipsListContainer!: ElementRef;
  /** The currently selected entity to display details for */
  @Input() selectedEntity: EntityInfo | null = null;
  /** All entity fields for looking up field information */
  @Input() allEntityFields: EntityFieldInfo[] = [];
  /** Whether the fields section is expanded */
  @Input() fieldsSectionExpanded = true;
  /** Whether the relationships section is expanded */
  @Input() relationshipsSectionExpanded = true;
  /** Emitted when user clicks to open the entity record */
  @Output() openEntity = new EventEmitter<EntityInfo>();
  /** Emitted when user clicks the close button */
  /** Emitted when fields section is toggled */
  @Output() fieldsSectionToggle = new EventEmitter<void>();
  /** Emitted when relationships section is toggled */
  @Output() relationshipsSectionToggle = new EventEmitter<void>();
  /** Emitted when a related entity is selected (clicked in relationships list) */
  /** Emitted when requesting to open an entity record */
  @Output() openRecord = new EventEmitter<EntityDetailsOpenRecordEvent>();
  public fieldFilter: 'all' | 'keys' | 'foreign_keys' | 'regular' = 'all';
  public expandedFieldDescriptions = new Set<string>();
  public expandedFieldValues = new Set<string>();
  public expandedFieldDetails = new Set<string>();
  private previousSelectedEntityId: string | null = null;
    if (changes['selectedEntity'] && !changes['selectedEntity'].firstChange) {
      const currentEntityId = this.selectedEntity?.ID || null;
      // Check if entity actually changed
      if (currentEntityId !== this.previousSelectedEntityId) {
        // Reset scroll positions when entity changes
        this.resetScrollPositions();
        this.previousSelectedEntityId = currentEntityId;
  private resetScrollPositions(): void {
    // Use setTimeout to ensure the DOM is updated
      if (this.fieldsListContainer?.nativeElement) {
        this.fieldsListContainer.nativeElement.scrollTop = 0;
      if (this.relationshipsListContainer?.nativeElement) {
        this.relationshipsListContainer.nativeElement.scrollTop = 0;
  public onOpenEntity(): void {
        RecordID: this.selectedEntity.ID
  public onClosePanel(): void {
  public toggleFieldsSection(): void {
    this.fieldsSectionToggle.emit();
  public toggleRelationshipsSection(): void {
    this.relationshipsSectionToggle.emit();
  public setFieldFilter(filter: 'all' | 'keys' | 'foreign_keys' | 'regular'): void {
    this.fieldFilter = filter;
  public getEntityFields(entityId: string): EntityFieldInfo[] {
    if (!entityId) return [];
    let fields = this.allEntityFields.filter(f => f.EntityID === entityId);
    switch (this.fieldFilter) {
      case 'keys':
        return fields.filter(f => f.IsPrimaryKey || f.Name.toLowerCase().includes('id'));
      case 'foreign_keys':
        return fields.filter(f => f.RelatedEntityID && !f.IsPrimaryKey);
      case 'regular':
        return fields.filter(f => !f.IsPrimaryKey && !f.RelatedEntityID);
  public getRelatedEntities(entityId: string): EntityInfo[] {
    const relatedEntityIds = new Set<string>();
    // Get entities that this entity references (foreign keys)
    this.allEntityFields
      .filter(f => f.EntityID === entityId && f.RelatedEntityID)
      .forEach(f => relatedEntityIds.add(f.RelatedEntityID!));
    // Get entities that reference this entity
      .filter(f => f.RelatedEntityID === entityId)
      .forEach(f => relatedEntityIds.add(f.EntityID));
    // Remove the current entity from the set (don't return self-references)
    relatedEntityIds.delete(entityId);
    // Convert to actual EntityInfo objects
    const allEntities = md.Entities;
    const retVals: EntityInfo[] = [];
    relatedEntityIds.forEach(id => {
      const entity = allEntities.find(e => e.ID === id);
        retVals.push(entity);
    return retVals;
  public onFieldClick(field: EntityFieldInfo): void {
    this.toggleFieldDetails(field.ID);
  public toggleFieldDescription(fieldId: string): void {
    if (this.expandedFieldDescriptions.has(fieldId)) {
      this.expandedFieldDescriptions.delete(fieldId);
      this.expandedFieldDescriptions.add(fieldId);
  public toggleFieldValues(fieldId: string): void {
    if (this.expandedFieldValues.has(fieldId)) {
      this.expandedFieldValues.delete(fieldId);
      this.expandedFieldValues.add(fieldId);
  public toggleFieldDetails(fieldId: string): void {
    if (this.expandedFieldDetails.has(fieldId)) {
      this.expandedFieldDetails.delete(fieldId);
      this.expandedFieldDetails.add(fieldId);
  public isFieldDescriptionExpanded(fieldId: string): boolean {
    return this.expandedFieldDescriptions.has(fieldId);
  public isFieldValuesExpanded(fieldId: string): boolean {
    return this.expandedFieldValues.has(fieldId);
  public isFieldDetailsExpanded(fieldId: string): boolean {
    return this.expandedFieldDetails.has(fieldId);
  public hasFieldPossibleValues(field: EntityFieldInfo): boolean {
    return field.EntityFieldValues && field.EntityFieldValues.length > 0;
  public getFieldPossibleValues(field: EntityFieldInfo): string[] {
    if (!field.EntityFieldValues) return [];
    return field.EntityFieldValues.map(v => v.Value).slice(0, 10);
  public getSortedEntityFieldValues(field: EntityFieldInfo): EntityFieldValueInfo[] {
    return field.EntityFieldValues.sort((a, b) => {
      if (a.Sequence !== undefined && b.Sequence !== undefined) {
      return a.Value.localeCompare(b.Value);
  public onRelatedEntityClick(event: Event, field: EntityFieldInfo): void {
      // Find the related entity and select it in the ERD
      const relatedEntity = md.Entities.find(e => e.ID === field.RelatedEntityID);
      if (relatedEntity) {
        this.entitySelected.emit(relatedEntity);
  public selectEntity(entity: EntityInfo, _zoomTo: boolean = false): void {
