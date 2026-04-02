import { Component, Input, ChangeDetectionStrategy, ChangeDetectorRef, OnChanges, SimpleChanges } from '@angular/core';
import { CompositeKey, Metadata, EntityInfo } from '@memberjunction/core';
 * A clickable pill component that displays a link to a related entity record.
 * Shows the entity icon (from metadata) and either the record name or entity name.
 * Clicking opens the entity record in a new tab.
 * <mj-entity-link-pill
 *   [entityName]="'MJ: AI Agent Runs'"
 *   [recordId]="run.TargetLogID"
 *   [recordName]="run.AgentRunName">
 * </mj-entity-link-pill>
  selector: 'mj-entity-link-pill',
    @if (entityInfo && recordId) {
      <span class="entity-link-pill" (click)="openRecord()" [title]="tooltipText">
        <i class="entity-icon" [ngClass]="iconClass"></i>
        <span class="entity-label">{{ displayLabel }}</span>
        <i class="fas fa-external-link-alt pill-action"></i>
    .entity-link-pill {
      background: linear-gradient(135deg, rgba(59, 130, 246, 0.08), rgba(99, 102, 241, 0.08));
      max-width: 200px;
    .entity-link-pill:hover {
      background: linear-gradient(135deg, rgba(59, 130, 246, 0.15), rgba(99, 102, 241, 0.15));
      border-color: rgba(59, 130, 246, 0.4);
      box-shadow: 0 2px 8px rgba(59, 130, 246, 0.15);
    .entity-label {
      max-width: 150px;
    .pill-action {
    .entity-link-pill:hover .pill-action {
export class EntityLinkPillComponent implements OnChanges {
   * The entity name to link to (e.g., 'MJ: AI Agent Runs')
  @Input() entityName: string | null = null;
   * The record ID to link to
  @Input() recordId: string | null = null;
   * Optional display name for the record. If not provided, uses entity name.
  @Input() recordName: string | null = null;
  entityInfo: EntityInfo | null = null;
  constructor(private cdr: ChangeDetectorRef) {}
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['entityName'] && this.entityName) {
      this.entityInfo = this.metadata.EntityByName(this.entityName);
  get iconClass(): string {
    if (this.entityInfo?.Icon) {
      // Entity icon is typically a Font Awesome class like 'fa-robot'
      // Ensure it has the proper prefix
      const icon = this.entityInfo.Icon;
      if (icon.startsWith('fa-')) {
        return `fas ${icon}`;
      return icon;
    // Default icon if none specified
    return 'fas fa-link';
  get displayLabel(): string {
    if (this.recordName) {
      return this.recordName;
    if (this.entityInfo) {
      return this.entityInfo.Name;
    return 'View Record';
  get tooltipText(): string {
    const entityLabel = this.entityInfo?.Name || 'Record';
      return `Open ${entityLabel}: ${this.recordName}`;
    return `Open ${entityLabel}`;
  openRecord(): void {
    if (this.entityName && this.recordId) {
      SharedService.Instance.OpenEntityRecord(this.entityName, CompositeKey.FromID(this.recordId));
