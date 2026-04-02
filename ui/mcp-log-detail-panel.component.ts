 * @fileoverview MCP Log Detail Slide-Out Panel Component
 * Displays detailed information about an MCP tool execution log
 * in a slide-out panel from the right side.
import { Component, Input, Output, EventEmitter, ChangeDetectorRef, HostListener, OnInit, OnDestroy } from '@angular/core';
import { MCPExecutionLogData } from '../mcp-dashboard.component';
 * Extended log data with parsed input/output
export interface LogDetailData extends MCPExecutionLogData {
 * Expandable section state
interface ExpandableSection {
    selector: 'mj-mcp-log-detail-panel',
    templateUrl: './mcp-log-detail-panel.component.html',
    styleUrls: ['./mcp-log-detail-panel.component.css'],
export class MCPLogDetailPanelComponent implements OnInit, OnDestroy {
    // Inputs
    @Input() Log: LogDetailData | null = null;
    // Outputs
    @Output() RunAgain = new EventEmitter<{ toolId: string; connectionId: string }>();
    // Expandable Sections State
    InputArgsSection: ExpandableSection = { expanded: true };
    ResultSection: ExpandableSection = { expanded: true };
    ErrorSection: ExpandableSection = { expanded: true };
    // Panel Width / Resize State
    private readonly PANEL_WIDTH_SETTING_KEY = 'mcp-log-detail-panel/width';
    private readonly MIN_PANEL_WIDTH = 320;
    private readonly MAX_PANEL_WIDTH = 800;
    private readonly DEFAULT_PANEL_WIDTH = 500;
    private readonly MOBILE_BREAKPOINT = 768;
    PanelWidth: number = this.DEFAULT_PANEL_WIDTH;
    IsResizing = false;
    get IsMobileMode(): boolean {
        return typeof window !== 'undefined' && window.innerWidth <= this.MOBILE_BREAKPOINT;
    private widthPersistSubject = new Subject<number>();
        this.widthPersistSubject.pipe(
        ).subscribe(width => {
            this.persistPanelWidth(width);
        this.loadSavedPanelWidth();
    // Panel Resize Handlers
    onResizeStart(event: MouseEvent): void {
        if (this.IsMobileMode) return;
        const newWidth = window.innerWidth - event.clientX;
        this.PanelWidth = Math.min(
            Math.max(newWidth, this.MIN_PANEL_WIDTH),
            Math.min(this.MAX_PANEL_WIDTH, window.innerWidth - 50)
            this.widthPersistSubject.next(this.PanelWidth);
    @HostListener('window:resize')
    onWindowResize(): void {
        if (this.IsMobileMode) {
            this.PanelWidth = window.innerWidth;
        } else if (this.PanelWidth > window.innerWidth - 50) {
            this.PanelWidth = Math.max(this.MIN_PANEL_WIDTH, window.innerWidth - 50);
    private loadSavedPanelWidth(): void {
            const savedWidth = UserInfoEngine.Instance.GetSetting(this.PANEL_WIDTH_SETTING_KEY);
            if (savedWidth) {
                const width = parseInt(savedWidth, 10);
                if (!isNaN(width) && width >= this.MIN_PANEL_WIDTH && width <= this.MAX_PANEL_WIDTH) {
            console.warn('[MCPLogDetailPanel] Failed to load saved panel width:', error);
    private async persistPanelWidth(width: number): Promise<void> {
            await UserInfoEngine.Instance.SetSetting(this.PANEL_WIDTH_SETTING_KEY, String(width));
            console.warn('[MCPLogDetailPanel] Failed to persist panel width:', error);
    closePanel(): void {
    onRunAgain(): void {
        if (this.Log?.ToolID && this.Log?.ConnectionID) {
            this.RunAgain.emit({
                toolId: this.Log.ToolID,
                connectionId: this.Log.ConnectionID
    // Section Toggle & Copy Actions
    toggleSection(section: ExpandableSection): void {
        section.expanded = !section.expanded;
    async copyInputArgs(): Promise<void> {
        await this.copyToClipboard(this.FormattedInputArgs, 'Input arguments copied to clipboard');
    async copyResult(): Promise<void> {
        await this.copyToClipboard(this.FormattedResult, 'Result copied to clipboard');
    async copyError(): Promise<void> {
        await this.copyToClipboard(this.Log?.ErrorMessage || '', 'Error message copied to clipboard');
    private async copyToClipboard(text: string, successMessage: string): Promise<void> {
            // Could emit an event or show a toast here
            console.log(successMessage);
    // Formatting Helpers
    formatJson(jsonString: string | null | undefined): string {
        if (!jsonString) return '';
            return jsonString;
    get FormattedInputArgs(): string {
        return this.formatJson(this.Log?.InputArgs);
    get FormattedResult(): string {
        return this.formatJson(this.Log?.Result);
            case 'Success': return 'fa-solid fa-check-circle';
            case 'Error': return 'fa-solid fa-times-circle';
            case 'Running': return 'fa-solid fa-spinner fa-spin';
    getStatusClass(status: string): string {
            case 'Success': return 'status-success';
            case 'Error': return 'status-error';
            case 'Running': return 'status-running';
