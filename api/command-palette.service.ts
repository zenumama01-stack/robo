 * Service for managing command palette state and recent apps tracking.
 * Responsibilities:
 * - Manage open/close state of command palette
 * - Track recently accessed applications (top 3)
 * - Persist recent apps via UserInfoEngine
export class CommandPaletteService {
  private isOpen$ = new BehaviorSubject<boolean>(false);
  private readonly recentAppsKey = 'CommandPalette.RecentApps';
  private readonly maxRecentApps = 3;
   * Observable of command palette open/close state
  get IsOpen(): Observable<boolean> {
    return this.isOpen$.asObservable();
   * Open the command palette
  Open(): void {
    this.isOpen$.next(true);
    this.isOpen$.next(false);
   * Track that a user accessed an application.
   * Adds the app ID to the front of the recent apps list,
   * removes duplicates, and limits to max 3 apps.
   * Persists via UserInfoEngine.
   * @param appId - ID of the application that was accessed
  async TrackAppAccess(appId: string): Promise<void> {
      // Load current recent apps
      const recentApps = await this.GetRecentApps();
      // Add to front, remove duplicates, limit to max
      const updated = [
        appId,
        ...recentApps.filter(id => id !== appId)
      ].slice(0, this.maxRecentApps);
      // Save back to UserInfoEngine
      await engine.SetSetting(this.recentAppsKey, JSON.stringify(updated));
      console.error('Failed to track app access:', error);
   * Get the list of recently accessed application IDs (up to 3)
   * @returns Promise resolving to array of app IDs
  async GetRecentApps(): Promise<string[]> {
      const json = await engine.GetSetting(this.recentAppsKey);
      if (!json) {
      console.error('Failed to load recent apps:', error);
   * Clear all recent apps history
  async ClearRecentApps(): Promise<void> {
      await engine.SetSetting(this.recentAppsKey, JSON.stringify([]));
      console.error('Failed to clear recent apps:', error);
