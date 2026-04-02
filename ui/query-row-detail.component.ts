import { Metadata, QueryInfo, QueryFieldInfo, EntityInfo } from '@memberjunction/core';
import { QueryGridColumnConfig } from '../query-data-grid/models/query-grid-types';
 * Event emitted when navigating to a linked entity record
export interface RowDetailEntityLinkEvent {
 * Field display configuration for the detail panel
interface DetailField {
    formattedValue: string;
    sqlType: string;
    isLongText: boolean;
 * Settings key for panel width persistence
const PANEL_WIDTH_SETTING_KEY = 'QueryViewer_RowDetailPanel_Width';
 * Settings key for hide empty fields preference
const HIDE_EMPTY_FIELDS_KEY = 'QueryViewer_RowDetailPanel_HideEmptyFields';
 * Row detail slide-in panel component.
 * Displays a single row's data in a formatted, grouped view with entity links.
    selector: 'mj-query-row-detail',
    templateUrl: './query-row-detail.component.html',
    styleUrls: ['./query-row-detail.component.css'],
                animate('150ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 }))
export class QueryRowDetailComponent implements OnInit, OnDestroy {
    private _rowData: Record<string, unknown> | null = null;
    set RowData(value: Record<string, unknown> | null) {
        this._rowData = value;
            this.buildDetailFields();
    get RowData(): Record<string, unknown> | null {
        return this._rowData;
    private _columns: QueryGridColumnConfig[] = [];
    set Columns(value: QueryGridColumnConfig[]) {
        if (this._rowData) {
    get Columns(): QueryGridColumnConfig[] {
    @Input() RowIndex: number = 0;
    @Input() TotalRows: number = 0;
    @Output() EntityLinkClick = new EventEmitter<RowDetailEntityLinkEvent>();
    @Output() NavigateRow = new EventEmitter<'prev' | 'next'>();
    public PrimaryKeyFields: DetailField[] = [];
    public ForeignKeyFields: DetailField[] = [];
    public RegularFields: DetailField[] = [];
    public PanelWidth: number = 400;
    public IsResizing: boolean = false;
    public HideEmptyFields: boolean = true;
    private minWidth = 300;
    private maxWidth = 800;
        this.loadPersistedWidth();
        this.loadHideEmptyFieldsPreference();
    // Field Building
    private buildDetailFields(): void {
        if (!this._rowData || !this._columns.length) {
            this.PrimaryKeyFields = [];
            this.ForeignKeyFields = [];
            this.RegularFields = [];
        const primaryKeys: DetailField[] = [];
        const foreignKeys: DetailField[] = [];
        const regular: DetailField[] = [];
        for (const col of this._columns) {
            const value = this._rowData[col.field];
            const field = this.buildDetailField(col, value);
                primaryKeys.push(field);
            } else if (field.isForeignKey) {
                foreignKeys.push(field);
                regular.push(field);
        this.PrimaryKeyFields = primaryKeys;
        this.ForeignKeyFields = foreignKeys;
        this.RegularFields = regular;
    private buildDetailField(col: QueryGridColumnConfig, value: unknown): DetailField {
        const formattedValue = this.formatValue(value, col);
        const isLongText = typeof value === 'string' && value.length > 200;
            displayName: col.title,
            formattedValue,
            sqlType: col.sqlBaseType,
            isPrimaryKey: col.isPrimaryKey || false,
            isForeignKey: col.isForeignKey || false,
            targetEntityName: col.targetEntityName,
            targetEntityIcon: col.targetEntityIcon,
            sourceEntityName: col.sourceEntityName,
            isLongText,
    private formatValue(value: unknown, col: QueryGridColumnConfig): string {
            return '(empty)';
        // Boolean
        // Date/Time
            const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
            let relativeTime = '';
            if (diffHours < 1) {
                relativeTime = 'just now';
            } else if (diffHours < 24) {
                relativeTime = `${diffHours} hour${diffHours === 1 ? '' : 's'} ago`;
                relativeTime = `${diffDays} day${diffDays === 1 ? '' : 's'} ago`;
            const formatted = baseType === 'date'
                ? date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
                : date.toLocaleString('en-US', {
            return relativeTime ? `${formatted} • ${relativeTime}` : formatted;
        // JSON detection
        if (strValue.startsWith('{') || strValue.startsWith('[')) {
                JSON.parse(strValue);
                return strValue; // Will be formatted as JSON in template
                // Not valid JSON, continue
    // User Actions
    public onEntityLinkClick(field: DetailField): void {
        if (field.targetEntityName && field.value != null) {
                entityName: field.targetEntityName,
                recordId: String(field.value),
                fieldName: field.name
    public onNavigatePrev(): void {
        this.NavigateRow.emit('prev');
    public onNavigateNext(): void {
        this.NavigateRow.emit('next');
    public toggleFieldExpand(field: DetailField): void {
        field.isExpanded = !field.isExpanded;
    public copyValue(field: DetailField): void {
        const textToCopy = field.value != null ? String(field.value) : '';
        navigator.clipboard.writeText(textToCopy);
    public copyRowAsJson(): void {
            const json = JSON.stringify(this._rowData, null, 2);
            navigator.clipboard.writeText(json);
    public isJson(value: string): boolean {
        return (trimmed.startsWith('{') && trimmed.endsWith('}')) ||
               (trimmed.startsWith('[') && trimmed.endsWith(']'));
    public formatJson(value: string): string {
        const containerRect = this.elementRef.nativeElement.parentElement?.getBoundingClientRect();
        if (!containerRect) return;
        const newWidth = containerRect.right - event.clientX;
        this.PanelWidth = Math.min(Math.max(newWidth, this.minWidth), this.maxWidth);
    // Persistence
    private loadPersistedWidth(): void {
            const savedWidth = UserInfoEngine.Instance.GetSetting(PANEL_WIDTH_SETTING_KEY);
                if (!isNaN(width) && width >= this.minWidth && width <= this.maxWidth) {
            console.error('[query-row-detail] Failed to load persisted width:', error);
            await UserInfoEngine.Instance.SetSetting(PANEL_WIDTH_SETTING_KEY, String(width));
            console.error('[query-row-detail] Failed to persist panel width:', error);
    private loadHideEmptyFieldsPreference(): void {
            const saved = UserInfoEngine.Instance.GetSetting(HIDE_EMPTY_FIELDS_KEY);
            if (saved !== undefined) {
                this.HideEmptyFields = saved === 'true';
            console.error('[query-row-detail] Failed to load hide empty fields preference:', error);
    private async persistHideEmptyFieldsPreference(): Promise<void> {
            await UserInfoEngine.Instance.SetSetting(HIDE_EMPTY_FIELDS_KEY, String(this.HideEmptyFields));
            console.error('[query-row-detail] Failed to persist hide empty fields preference:', error);
    public toggleHideEmptyFields(): void {
        this.HideEmptyFields = !this.HideEmptyFields;
        this.persistHideEmptyFieldsPreference();
     * Returns visible fields based on HideEmptyFields setting
    public getVisibleFields(fields: DetailField[]): DetailField[] {
        if (!this.HideEmptyFields) {
        return fields.filter(f => f.value !== null && f.value !== undefined && f.value !== '');
     * Returns count of empty fields for display
    public getEmptyFieldCount(fields: DetailField[]): number {
        return fields.filter(f => f.value === null || f.value === undefined || f.value === '').length;
    // Keyboard Navigation
        if (!this.Visible) return;
        } else if (event.key === 'ArrowUp' || event.key === 'k') {
            if (this.RowIndex > 0) {
                this.onNavigatePrev();
        } else if (event.key === 'ArrowDown' || event.key === 'j') {
            if (this.RowIndex < this.TotalRows - 1) {
                this.onNavigateNext();
