import { Subject, Observable } from 'rxjs';
 * Service for requesting tabs to be opened.
 * This service provides a way for any component in the app to request
 * that a new tab be opened without needing direct access to the
 * WorkspaceStateManager.
 * Tab requests are queued and can be replayed when subscribers connect.
 * This ensures requests aren't lost if they arrive before the shell is ready.
export class TabService {
  private tabRequest$ = new Subject<TabRequest>();
  private queuedRequests: TabRequest[] = [];
   * Flag to suppress the next ResourceResolver processing.
   * Used when URL changes are just syncing to the current active tab,
   * not actual navigation requests that should create/focus tabs.
  private _suppressNextResolve = false;
   * Observable of tab open requests
  get TabRequests(): Observable<TabRequest> {
    return this.tabRequest$.asObservable();
   * Request that a new tab be opened
  OpenTab(request: TabRequest): void {
    // Store in queue for replay
    this.queuedRequests.push(request);
    // Emit to current subscribers
    this.tabRequest$.next(request);
   * Get all queued tab requests (for replay on subscription)
  GetQueuedRequests(): TabRequest[] {
    return [...this.queuedRequests];
   * Clear the queue after requests have been processed
  ClearQueue(): void {
    this.queuedRequests = [];
   * Helper method to open a record in a tab
  OpenRecord(entityName: string, recordId: string, applicationId: string): void {
    this.OpenTab({
      Title: `${entityName}`,
      Route: `/record/${entityName}/${recordId}`,
        resourceType: 'record',
        recordId
   * Helper method to open a view in a tab
  OpenView(viewId: string, viewName: string, applicationId: string): void {
      Title: viewName,
      Route: `/view/${viewId}`,
        resourceType: 'view',
        viewId
   * Helper method to open a dashboard in a tab
  OpenDashboard(dashboardId: string, dashboardName: string, applicationId: string): void {
      Title: dashboardName,
      Route: `/dashboard/${dashboardId}`,
        resourceType: 'dashboard',
        dashboardId
   * Helper method to open a report in a tab
  OpenReport(reportId: string, reportName: string, applicationId: string): void {
      Title: reportName,
      Route: `/report/${reportId}`,
        resourceType: 'report',
        reportId
   * Helper method to open a query in a tab
  OpenQuery(queryId: string, queryName: string, applicationId: string): void {
      Title: queryName,
      Route: `/query/${queryId}`,
        resourceType: 'query',
        queryId
   * Helper method to open a list in a tab
  OpenList(listId: string, listName: string, applicationId: string): void {
      Title: listName,
      ResourceRecordId: listId,
        resourceType: 'Custom',
        driverClass: 'ListDetailResource',
        recordId: listId
   * Signal that the next ResourceResolver call should be suppressed.
   * Call this before navigating when the URL change is just syncing
   * to the current active tab, not a real navigation request.
  SuppressNextResolve(): void {
    this._suppressNextResolve = true;
   * Check if the current resolve should be suppressed.
   * Returns true if SuppressNextResolve() was called and the flag hasn't been cleared.
  ShouldSuppressResolve(): boolean {
    return this._suppressNextResolve;
   * Clear the suppress flag. Call this after checking ShouldSuppressResolve()
   * to reset the flag for future navigations.
  ClearSuppressFlag(): void {
    this._suppressNextResolve = false;
