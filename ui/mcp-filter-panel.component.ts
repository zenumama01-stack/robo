 * @fileoverview MCP Filter Panel Component
 * Provides filtering controls for the MCP Dashboard.
 * Matches the pattern used by agent-filter-panel component.
 * @module MCP Filter Panel
import { MCPDashboardFilters, MCPDashboardTab } from './mcp-dashboard.component';
  selector: 'mj-mcp-filter-panel',
  templateUrl: './mcp-filter-panel.component.html',
  styleUrls: ['./mcp-filter-panel.component.css']
export class MCPFilterPanelComponent {
  @Input() filters: MCPDashboardFilters = {
  @Input() activeTab: MCPDashboardTab = 'servers';
  @Input() totalCount = 0;
  @Input() filteredCount = 0;
  @Output() filtersChange = new EventEmitter<MCPDashboardFilters>();
  public serverStatusOptions = [
    { text: 'Inactive', value: 'Inactive' }
  public connectionStatusOptions = [
    { text: 'Error', value: 'Error' }
  public toolStatusOptions = [
    { text: 'Deprecated', value: 'Deprecated' }
  public logStatusOptions = [
    this.filters = { ...this.filters, searchTerm: value };
    this.onFilterChange();
  public onServerStatusChange(value: string): void {
    this.filters = { ...this.filters, serverStatus: value };
  public onConnectionStatusChange(value: string): void {
    this.filters = { ...this.filters, connectionStatus: value };
  public onToolStatusChange(value: string): void {
    this.filters = { ...this.filters, toolStatus: value };
  public onLogStatusChange(value: string): void {
    this.filters = { ...this.filters, logStatus: value };
    this.filters = {
    return this.filters.searchTerm !== '' ||
           this.filters.serverStatus !== 'all' ||
           this.filters.connectionStatus !== 'all' ||
           this.filters.toolStatus !== 'all' ||
           this.filters.logStatus !== 'all';
  public get currentStatusOptions(): { text: string; value: string }[] {
    switch (this.activeTab) {
        return this.serverStatusOptions;
        return this.connectionStatusOptions;
        return this.toolStatusOptions;
        return this.logStatusOptions;
  public get currentStatusValue(): string {
        return this.filters.serverStatus;
        return this.filters.connectionStatus;
        return this.filters.toolStatus;
        return this.filters.logStatus;
        return 'all';
  public onCurrentStatusChange(value: string): void {
        this.onServerStatusChange(value);
        this.onConnectionStatusChange(value);
        this.onToolStatusChange(value);
        this.onLogStatusChange(value);
  public getTabLabel(): string {
        return 'Server';
        return 'Connection';
        return 'Tool';
        return 'Log';
        return 'Item';
