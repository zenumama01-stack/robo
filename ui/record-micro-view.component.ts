import { Component, Input, Output, EventEmitter, OnInit, ChangeDetectorRef, ChangeDetectionStrategy, HostListener } from '@angular/core';
import { RunView, Metadata, EntityInfo, CompositeKey } from '@memberjunction/core';
import { MicroViewData } from '../types';
    DiffClass: string;
    IsNull: boolean;
    IsBoolean: boolean;
    BooleanValue: boolean;
    IsJson: boolean;
    /** When set, the field is a FK and this holds the display value from the related virtual field */
    ForeignKeyDisplayValue: string;
    /** When set, user can click to navigate to this related record */
    ForeignKeyEntityName: string;
    /** The raw FK value (a GUID) for navigation */
    ForeignKeyRecordId: string;
interface RecordChangeSimple {
/** Event emitted when user wants to navigate to a related FK record */
export interface EntityLinkClickEvent {
    CompositeKey: CompositeKey;
    selector: 'mj-record-micro-view',
    templateUrl: './record-micro-view.component.html',
    styleUrls: ['./record-micro-view.component.css'],
export class MjRecordMicroViewComponent implements OnInit {
    @Input() Data!: MicroViewData;
    @Input() Inline = false;
    @Output() OpenRecord = new EventEmitter<EntityLinkClickEvent>();
    public Fields: FieldDisplay[] = [];
    public ShowNullValues = false;
    /** Formatted display of the record primary key (simplified for single-value PKs) */
    public DisplayRecordID = '';
    /** The value of the IsNameField for display in the header */
    public RecordDisplayName = '';
    /** Icon CSS class from EntityInfo.Icon, falls back to 'fa-solid fa-table' */
    public EntityIcon = 'fa-solid fa-table';
        if (this.Inline) {
        this.DisplayRecordID = this.formatRecordId(this.Data.RecordID);
        this.loadRecordData();
        if (!this.Inline) {
            setTimeout(() => this.Close.emit(), 250);
    public GetTypeIcon(type: string): string {
            'uniqueidentifier': 'fa-solid fa-fingerprint',
            'datetime': 'fa-solid fa-calendar',
            'datetimeoffset': 'fa-solid fa-calendar',
            'date': 'fa-solid fa-calendar-day',
            'time': 'fa-solid fa-clock',
            'bit': 'fa-solid fa-toggle-on',
            'boolean': 'fa-solid fa-toggle-on',
            'int': 'fa-solid fa-hashtag',
            'bigint': 'fa-solid fa-hashtag',
            'float': 'fa-solid fa-hashtag',
            'decimal': 'fa-solid fa-hashtag',
            'money': 'fa-solid fa-hashtag',
            'number': 'fa-solid fa-hashtag',
            'nvarchar': 'fa-solid fa-font',
            'varchar': 'fa-solid fa-font',
            'ntext': 'fa-solid fa-align-left',
            'text': 'fa-solid fa-align-left',
            'string': 'fa-solid fa-font',
            'object': 'fa-solid fa-code',
            'null': 'fa-solid fa-circle-xmark'
        return icons[type.toLowerCase()] ?? 'fa-solid fa-font';
    public get HasDiffs(): boolean {
        return this.Data.FieldDiffs != null && this.Data.FieldDiffs.length > 0;
    public get VisibleFields(): FieldDisplay[] {
        if (this.ShowNullValues) {
            return this.Fields;
        return this.Fields.filter(f => !f.IsNull);
    public get HiddenNullCount(): number {
        return this.Fields.filter(f => f.IsNull).length;
    public ToggleShowNulls(): void {
        this.ShowNullValues = !this.ShowNullValues;
    public OnEntityLinkClicked(event: MouseEvent, field: FieldDisplay): void {
        if (field.ForeignKeyEntityName && field.ForeignKeyRecordId) {
            const pkey = new CompositeKey([{ FieldName: 'ID', Value: field.ForeignKeyRecordId }]);
                EntityName: field.ForeignKeyEntityName,
                RecordID: field.ForeignKeyRecordId,
        if (this.Data.EntityName && this.Data.RecordID) {
            const rawId = this.extractRawId(this.Data.RecordID);
                EntityName: this.Data.EntityName,
    private async loadRecordData(): Promise<void> {
            let recordData = this.Data.FullRecordJSON;
            if (!recordData && this.Data.RecordChangeID) {
                recordData = await this.loadRecordChangeJson(this.Data.RecordChangeID);
            if (recordData) {
                const entityInfo = this.findEntityInfo();
                this.EntityIcon = entityInfo?.Icon || 'fa-solid fa-table';
                this.RecordDisplayName = this.extractNameFieldValue(entityInfo, recordData);
                this.Fields = this.buildFieldList(recordData, entityInfo);
                this.ErrorMessage = 'Unable to load record data.';
            this.ErrorMessage = e instanceof Error ? e.message : 'Failed to load record data.';
    private async loadRecordChangeJson(changeId: string): Promise<Record<string, unknown> | null> {
        const result = await rv.RunView<RecordChangeSimple>({
            ExtraFilter: `ID = '${changeId}'`,
            const json = result.Results[0].FullRecordJSON;
                return JSON.parse(json) as Record<string, unknown>;
    // Field building
    private buildFieldList(data: Record<string, unknown>, entityInfo: EntityInfo | undefined): FieldDisplay[] {
        const diffMap = this.buildDiffMap();
        const fkMap = this.buildForeignKeyMap(entityInfo, data);
        const virtualFieldNames = this.getVirtualFieldNames(entityInfo);
            // Skip virtual fields that we've merged into their FK counterpart
            if (virtualFieldNames.has(key)) continue;
            const fieldMeta = entityInfo?.Fields.find(f => f.Name === key);
            const diff = diffMap.get(key);
            const fkInfo = fkMap.get(key);
            const fieldType = fieldMeta?.Type ?? this.inferType(value);
            const isBooleanField = this.isBooleanType(fieldType, value);
            const isJsonField = this.isJsonValue(value);
                DisplayName: fieldMeta?.DisplayNameOrName ?? key,
                Value: this.formatValue(value, isJsonField),
                Type: fieldType,
                Description: fieldMeta?.Description ?? '',
                DiffClass: diff?.ChangeType ? `diff-${diff.ChangeType.toLowerCase()}` : '',
                OldValue: diff?.OldValue ?? '',
                IsNull: value == null,
                IsBoolean: isBooleanField,
                BooleanValue: this.toBooleanValue(value),
                IsJson: isJsonField,
                Sequence: fieldMeta?.Sequence ?? 9999,
                ForeignKeyDisplayValue: fkInfo?.DisplayValue ?? '',
                ForeignKeyEntityName: fkInfo?.EntityName ?? '',
                ForeignKeyRecordId: fkInfo?.RecordId ?? ''
        return fields.sort((a, b) => a.Sequence - b.Sequence);
     * Find the entity's NameField (uses EntityInfo.NameField getter which checks
     * IsNameField first, then falls back to a field called "Name").
    private extractNameFieldValue(entityInfo: EntityInfo | undefined, data: Record<string, unknown>): string {
        const nameField = entityInfo.NameField;
        if (nameField && data[nameField.Name] != null) {
            return String(data[nameField.Name]);
    private findEntityInfo(): EntityInfo | undefined {
        if (this.Data.EntityName && this.Data.EntityName !== 'Unknown') {
            return this.metadata.Entities.find(e => e.Name === this.Data.EntityName);
        if (this.Data.EntityID) {
            return this.metadata.Entities.find(e => e.ID === this.Data.EntityID);
    private buildDiffMap(): Map<string, { ChangeType: string; OldValue: string }> {
        const map = new Map<string, { ChangeType: string; OldValue: string }>();
        if (this.Data.FieldDiffs) {
            for (const diff of this.Data.FieldDiffs) {
                map.set(diff.FieldName, { ChangeType: diff.ChangeType, OldValue: diff.OldValue });
     * Build a map of FK field name -> { DisplayValue, EntityName, RecordId }
     * by matching FK fields to their associated virtual (display) fields.
    private buildForeignKeyMap(
        entityInfo: EntityInfo | undefined,
        data: Record<string, unknown>
    ): Map<string, { DisplayValue: string; EntityName: string; RecordId: string }> {
        const map = new Map<string, { DisplayValue: string; EntityName: string; RecordId: string }>();
        if (!entityInfo) return map;
        for (const field of entityInfo.Fields) {
            if (!field.RelatedEntityID || field.Name === 'ID') continue;
            // Find the related entity name
            const relatedEntity = this.metadata.Entities.find(e => e.ID === field.RelatedEntityID);
            if (!relatedEntity) continue;
            const fkValue = data[field.Name];
            if (fkValue == null) continue;
            // Find display value from the associated virtual field
            const displayValue = this.findFkDisplayValue(entityInfo, field.Name, data);
            map.set(field.Name, {
                DisplayValue: displayValue,
                EntityName: relatedEntity.Name,
                RecordId: String(fkValue)
     * Find the display value for a FK field by checking multiple strategies:
     * 1. Look for a virtual field with RelatedEntityNameFieldMap pointing to this FK
     * 2. Look for any virtual field whose name is derived from the FK name
     * 3. Scan data keys for common naming patterns (e.g., "TypeID" -> "Type")
    private findFkDisplayValue(entityInfo: EntityInfo, fkFieldName: string, data: Record<string, unknown>): string {
        // Strategy 1: Metadata-linked virtual field via RelatedEntityNameFieldMap
        const linkedVirtual = entityInfo.Fields.find(f =>
            f.RelatedEntityNameFieldMap === fkFieldName && f.IsVirtual
        if (linkedVirtual && data[linkedVirtual.Name] != null) {
            return String(data[linkedVirtual.Name]);
        // Strategy 2: Find any virtual field that looks like a display field for this FK
        // by checking all virtual fields and matching via naming patterns
        const virtualFields = entityInfo.Fields.filter(f => f.IsVirtual);
        for (const vf of virtualFields) {
            // Check if this virtual field's RelatedEntityNameFieldMap matches (case-insensitive)
            if (vf.RelatedEntityNameFieldMap &&
                vf.RelatedEntityNameFieldMap.toLowerCase() === fkFieldName.toLowerCase() &&
                data[vf.Name] != null) {
                return String(data[vf.Name]);
        // Strategy 3: Try common naming conventions in the data itself
        // e.g., "TypeID" -> look for "Type" key in data
        //        "OwnerUserID" -> look for "Owner User" or "OwnerUser"
        // Strip trailing "ID" from the FK name and try variations
        if (fkFieldName.endsWith('ID') && fkFieldName.length > 2) {
            const baseName = fkFieldName.slice(0, -2);
            // Direct match in data (e.g., "CategoryID" -> "Category")
            if (data[baseName] != null && typeof data[baseName] === 'string' &&
                !this.looksLikeGuid(data[baseName] as string)) {
                return String(data[baseName]);
            // Try with space before capitals (e.g., "OwnerUserID" -> "Owner User")
            const spacedName = baseName.replace(/([a-z])([A-Z])/g, '$1 $2');
            if (spacedName !== baseName && data[spacedName] != null &&
                typeof data[spacedName] === 'string' &&
                !this.looksLikeGuid(data[spacedName] as string)) {
                return String(data[spacedName]);
    /** Quick check if a string looks like a GUID (to avoid using GUIDs as display values). */
    private looksLikeGuid(value: string): boolean {
        return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value);
     * Get the set of virtual field names so we can skip them in the main field list.
     * Virtual fields are display-only fields whose values are merged into FK rows.
     * We skip ALL virtual fields — not just ones with RelatedEntityNameFieldMap —
     * because they are derived/computed and don't represent stored data.
    private getVirtualFieldNames(entityInfo: EntityInfo | undefined): Set<string> {
        if (!entityInfo) return names;
            if (field.IsVirtual) {
                names.add(field.Name);
    // Record ID formatting
     * For single-value PKs like "ID|<uuid>", extract just the uuid.
     * For composite keys, show the full concatenated string.
    private formatRecordId(recordId: string): string {
     * Extract the raw ID value from a potentially formatted record ID string.
    private extractRawId(recordId: string): string {
    // Type helpers
    private isBooleanType(type: string, value: unknown): boolean {
        if (t === 'bit' || t === 'boolean') return true;
        if (value === true || value === false) return true;
    private toBooleanValue(value: unknown): boolean {
        if (value === true || value === 1 || value === '1' || value === 'true') return true;
    private isJsonValue(value: unknown): boolean {
        if (typeof value === 'object' && value != null) return true;
    // Formatting helpers
    private formatValue(value: unknown, isJson: boolean): string {
        if (typeof value === 'number') return String(value);
            if (this.isDateString(value)) return this.formatDate(value);
            // Try to pretty-print JSON strings
            if (isJson) return this.formatJsonString(value);
            if (value.length > 300) return value.substring(0, 300) + '...';
    private formatJsonString(value: string): string {
    private isDateString(value: string): boolean {
        return /^\d{4}-\d{2}-\d{2}T/.test(value);
    private formatDate(value: string): string {
            const d = new Date(value);
                month: 'short', day: 'numeric', year: 'numeric',
                hour: '2-digit', minute: '2-digit'
    private inferType(value: unknown): string {
        if (typeof value === 'boolean') return 'bit';
        if (typeof value === 'number') return 'int';
            if (this.isDateString(value)) return 'datetime';
            if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value)) return 'uniqueidentifier';
            return 'nvarchar';
