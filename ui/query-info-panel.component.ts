import { QueryInfo, QueryFieldInfo, QueryParameterInfo } from '@memberjunction/core';
 * User Settings key for info panel width
const INFO_PANEL_WIDTH_KEY = 'QueryViewer_InfoPanelWidth';
 * Event emitted when opening the full query record
export interface OpenQueryRecordEvent {
    queryName: string;
 * A slide-in panel that displays detailed query information.
 * - Expandable sections for fields, parameters, and SQL
 * - Resizable width with persistence
 * - Open button to navigate to full record
    selector: 'mj-query-info-panel',
    templateUrl: './query-info-panel.component.html',
    styleUrls: ['./query-info-panel.component.css'],
                style({ transform: 'translateX(100%)' }),
                animate('250ms ease-out', style({ transform: 'translateX(0)' }))
                animate('200ms ease-in', style({ transform: 'translateX(100%)' }))
                animate('150ms ease-in', style({ opacity: 0 }))
export class QueryInfoPanelComponent implements OnInit, OnDestroy {
    @Input() QueryInfo: QueryInfo | null = null;
    @Input() ShowOverlay: boolean = true;
    @Output() OpenRecord = new EventEmitter<OpenQueryRecordEvent>();
    public PanelWidth: number = 450;
    public ExpandedSections: Set<string> = new Set(['overview', 'fields']);
        // Debounce width persistence
        this.loadPanelWidth();
    // Section Toggle
    public ToggleSection(section: string): void {
        if (this.ExpandedSections.has(section)) {
            this.ExpandedSections.delete(section);
            this.ExpandedSections.add(section);
    public IsSectionExpanded(section: string): boolean {
        return this.ExpandedSections.has(section);
    // Panel Actions
    public OnOpenRecord(): void {
        if (this.QueryInfo) {
            this.OpenRecord.emit({
                queryId: this.QueryInfo.ID,
                queryName: this.QueryInfo.Name
    // Resize Handling
    OnMouseMove(event: MouseEvent): void {
        this.PanelWidth = Math.min(Math.max(newWidth, 350), 800);
    OnMouseUp(): void {
    // Width Persistence
    private loadPanelWidth(): void {
            const saved = UserInfoEngine.Instance.GetSetting(INFO_PANEL_WIDTH_KEY);
                this.PanelWidth = parseInt(saved, 10) || 450;
            console.warn('[query-info-panel] Failed to load panel width:', error);
            await UserInfoEngine.Instance.SetSetting(INFO_PANEL_WIDTH_KEY, String(width));
            console.warn('[query-info-panel] Failed to persist panel width:', error);
    public GetFieldTypeDisplay(field: QueryFieldInfo): string {
        return field.SQLFullType || field.SQLBaseType || 'unknown';
    public GetParamTypeDisplay(param: QueryParameterInfo): string {
        return param.Type || 'string';
    public TrackByField(index: number, field: QueryFieldInfo): string {
    public TrackByParam(index: number, param: QueryParameterInfo): string {
        return param.Name;
