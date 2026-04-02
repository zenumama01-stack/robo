import { IMetadataProvider, IRunViewProvider, LogError, Metadata, RunView } from '@memberjunction/core';
import { MJDataContextEntity, MJDataContextItemEntity } from '@memberjunction/core-entities';
  selector: 'mj-data-context',
  templateUrl: './ng-data-context.component.html',
  styleUrls: ['./ng-data-context.component.css']
export class DataContextComponent implements OnInit {
  public dataContextRecord?: MJDataContextEntity;
  public dataContextItems: MJDataContextItemEntity[] = [];
  public searchTerm: string = '';
  public showSQLPreview: boolean = false;
  public previewSQL: string = '';
  public copiedField: string = '';
  public expandedItems: { [key: string]: boolean } = {};
  public get filteredItems(): MJDataContextItemEntity[] {
      return this.dataContextItems;
    return this.dataContextItems.filter(item => 
      item.Type?.toLowerCase().includes(term) ||
      item.SQL?.toLowerCase().includes(term) ||
      (item.EntityID ? this.getEntityName(item.EntityID)?.toLowerCase().includes(term) : false) ||
      item.Description?.toLowerCase().includes(term)
  public get itemCount(): number {
    return this.filteredItems.length;
    if(this.dataContextId){
      this.LoadDataContext(this.dataContextId);
  async LoadDataContext(dataContextId: string) {
      if (dataContextId) {
        const p = this.ProviderToUse;
        this.dataContextRecord = await p.GetEntityObject<MJDataContextEntity>("MJ: Data Contexts", p.CurrentUser);
        await this.dataContextRecord.Load(dataContextId);
        const rv = new RunView(<IRunViewProvider><any>p);
        const response = await rv.RunView<MJDataContextItemEntity>(
            EntityName: "MJ: Data Context Items", 
            ExtraFilter: `DataContextID='${dataContextId}'`,
        if(response.Success){
          this.dataContextItems = response.Results;
          this.errorMessage = response.ErrorMessage || 'Failed to load data context items';
          LogError(response.ErrorMessage);
      this.errorMessage = 'An error occurred while loading the data context';
    const typeIcons: Record<string, string> = {
      'sql': 'fa-solid fa-database',
      'view': 'fa-solid fa-table',
      'query': 'fa-solid fa-magnifying-glass',
      'entity': 'fa-solid fa-cube',
      'record': 'fa-solid fa-file'
    return typeIcons[type?.toLowerCase()] || 'fa-solid fa-question';
  public getTypeColor(type: string): string {
    const typeColors: Record<string, string> = {
      'sql': '#2196f3',
      'view': '#4caf50',
      'query': '#ff9800',
      'entity': '#9c27b0',
      'record': '#f44336'
    return typeColors[type?.toLowerCase()] || '#757575';
  public getEntityName(entityId: string | null): string | undefined {
    if (!entityId) return undefined;
    return md.Entities.find(e => e.ID === entityId)?.Name;
  public async getViewName(viewId: string): Promise<string | undefined> {
      const view = await p.GetEntityObject("Views", p.CurrentUser);
      await (view as any).Load(viewId);
      return (view as any).Get('Name');
  public async getQueryName(queryId: string): Promise<string | undefined> {
      const query = await p.GetEntityObject("MJ: Queries", p.CurrentUser);
      await (query as any).Load(queryId);
      return (query as any).Get('Name');
  public onSearchChange(): void {
    // Reset expanded items when searching
    this.expandedItems = {};
  public toggleItemExpansion(itemId: string): void {
    this.expandedItems[itemId] = !this.expandedItems[itemId];
  public async copyToClipboard(text: string, fieldName: string): Promise<void> {
      this.copiedField = fieldName;
        this.copiedField = '';
      LogError(`Failed to copy to clipboard: ${err}`);
  public previewSQLCode(sql: string): void {
    this.previewSQL = sql;
    this.showSQLPreview = true;
  public closeSQLPreview(): void {
    this.showSQLPreview = false;
    this.previewSQL = '';
  public navigateToEntity(entityId: string): void {
    // This would be implemented based on your navigation system
    console.log('Navigate to entity:', entityId);
  public navigateToView(viewId: string): void {
    console.log('Navigate to view:', viewId);
  public navigateToQuery(queryId: string): void {
    console.log('Navigate to query:', queryId);
    await this.LoadDataContext(this.dataContextId);
    // Implement CSV export functionality
    const headers = ['Type', 'SQL', 'View', 'Query', 'Entity', 'Record ID', 'Description'];
    const rows = this.filteredItems.map(item => [
      item.Type,
      item.SQL,
      item.ViewID,
      item.QueryID,
      this.getEntityName(item.EntityID) || item.EntityID,
      item.RecordID,
      item.Description || ''
    const csv = [headers, ...rows].map(row => row.map(cell => `"${cell}"`).join(',')).join('\n');
    a.download = `data-context-${this.dataContextRecord?.Name || this.dataContextId}.csv`;
