 * Represents an app item in the configuration UI
interface AppConfigItem {
  App: BaseApplication;
  UserAppId: string | null;
  IsDirty: boolean;
 * Inline component for configuring user's application visibility and order.
 * Allows users to:
 * - Select which applications to show in the app switcher
 * - Reorder applications via drag-and-drop
  selector: 'mj-application-settings',
  templateUrl: './application-settings.component.html',
  styleUrls: ['./application-settings.component.css']
export class ApplicationSettingsComponent implements OnInit {
  // All available apps from the system
  AllApps: AppConfigItem[] = [];
  // User's selected apps (active and ordered)
  ActiveApps: AppConfigItem[] = [];
  // Available apps not yet selected
  AvailableApps: AppConfigItem[] = [];
  SuccessMessage = '';
  // Native drag-and-drop state
  private draggedItem: AppConfigItem | null = null;
  private draggedIndex = -1;
  private dropTargetIndex = -1;
    await this.LoadConfiguration();
   * Loads the user's current app configuration
  async LoadConfiguration(): Promise<void> {
      // Load all system apps from ApplicationManager
      const systemApps = this.appManager.GetAllSystemApps();
      // Load user's UserApplication records
      const userAppsResult = await rv.RunView<MJUserApplicationEntity>({
        EntityName: 'MJ: User Applications',
        ExtraFilter: `UserID = '${md.CurrentUser.ID}'`,
        OrderBy: 'Sequence, Application',
      const userApps: MJUserApplicationEntity[] = userAppsResult.Success ? userAppsResult.Results : [];
      // Build app config items
      this.AllApps = this.BuildAppConfigItems(systemApps, userApps);
      // Separate into active (selected) and available (unselected)
      this.RefreshAppLists();
      this.ErrorMessage = 'Failed to load app configuration. Please try again.';
      LogError('Error loading app configuration:', undefined, error instanceof Error ? error.message : String(error));
   * Builds app config items by matching system apps with user's UserApplication records
  private BuildAppConfigItems(systemApps: BaseApplication[], userApps: MJUserApplicationEntity[]): AppConfigItem[] {
    const items: AppConfigItem[] = [];
    for (const app of systemApps) {
      const userApp = userApps.find(ua => ua.ApplicationID === app.ID);
        App: app,
        UserAppId: userApp?.ID || null,
        Sequence: userApp?.Sequence ?? 999,
        IsActive: userApp?.IsActive ?? false,
        IsDirty: false
    return items;
   * Separates apps into active and available lists based on IsActive state
  private RefreshAppLists(): void {
    this.ActiveApps = this.AllApps
      .filter(item => item.IsActive)
      .sort((a, b) => a.Sequence - b.Sequence);
    this.AvailableApps = this.AllApps
      .filter(item => !item.IsActive)
      .sort((a, b) => a.App.Name.localeCompare(b.App.Name));
   * Native drag start handler
  OnDragStart(event: DragEvent, item: AppConfigItem, index: number): void {
    this.draggedItem = item;
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/plain', index.toString());
   * Native drag over handler - allows drop
  OnDragOver(event: DragEvent): void {
      event.dataTransfer.dropEffect = 'move';
   * Native drag enter handler - tracks drop target
  OnDragEnter(event: DragEvent, index: number): void {
    this.dropTargetIndex = index;
   * Native drag end handler - cleanup
  OnDragEnd(): void {
    this.draggedItem = null;
    this.dropTargetIndex = -1;
   * Native drop handler - reorder items
  OnDrop(event: DragEvent): void {
    if (this.draggedIndex >= 0 && this.dropTargetIndex >= 0 && this.draggedIndex !== this.dropTargetIndex) {
      // Remove item from old position
      const [movedItem] = this.ActiveApps.splice(this.draggedIndex, 1);
      this.ActiveApps.splice(this.dropTargetIndex, 0, movedItem);
      // Update sequences based on new order
      this.ActiveApps.forEach((item, idx) => {
        if (item.Sequence !== idx) {
          item.Sequence = idx;
          item.IsDirty = true;
    // Reset drag state
    this.OnDragEnd();
   * Adds an app to the user's active list
  AddApp(item: AppConfigItem): void {
    item.IsActive = true;
    item.Sequence = this.ActiveApps.length;
   * Removes an app from the user's active list
  RemoveApp(item: AppConfigItem): void {
    item.IsActive = false;
    item.Sequence = 999;
    // Resequence remaining active apps
    this.ActiveApps.forEach((activeItem, index) => {
      if (activeItem.Sequence !== index) {
        activeItem.Sequence = index;
        activeItem.IsDirty = true;
   * Moves an app up in the order
  MoveUp(item: AppConfigItem): void {
    const index = this.ActiveApps.indexOf(item);
      const prevItem = this.ActiveApps[index - 1];
      // Swap sequences
      const tempSeq = item.Sequence;
      item.Sequence = prevItem.Sequence;
      prevItem.Sequence = tempSeq;
      prevItem.IsDirty = true;
      // Re-sort
      this.ActiveApps = [...this.ActiveApps].sort((a, b) => a.Sequence - b.Sequence);
   * Moves an app down in the order
  MoveDown(item: AppConfigItem): void {
    if (index < this.ActiveApps.length - 1) {
      const nextItem = this.ActiveApps[index + 1];
      item.Sequence = nextItem.Sequence;
      nextItem.Sequence = tempSeq;
      nextItem.IsDirty = true;
   * Checks if there are any unsaved changes
  HasChanges(): boolean {
    return this.AllApps.some(item => item.IsDirty);
   * Saves the user's app configuration
  async Save(): Promise<void> {
    if (!this.HasChanges()) {
      // Process each app config item
      for (const item of this.AllApps) {
        if (!item.IsDirty) continue;
        if (item.UserAppId) {
          // Update existing UserApplication record
          await this.UpdateUserApplication(md, item);
        } else if (item.IsActive) {
          // Create new UserApplication record (only if active)
          await this.CreateUserApplication(md, item);
      // Reload the ApplicationManager to reflect changes
      LogStatus('User app configuration saved, reloading ApplicationManager...');
      this.SuccessMessage = 'App configuration saved successfully!';
      this.sharedService.CreateSimpleNotification(this.SuccessMessage, 'success', 3000);
      // Clear dirty flags
      this.AllApps.forEach(item => item.IsDirty = false);
      this.ErrorMessage = 'Failed to save configuration. Please try again.';
      LogError('Error saving app configuration:', undefined, error instanceof Error ? error.message : String(error));
   * Updates an existing UserApplication record
  private async UpdateUserApplication(md: Metadata, item: AppConfigItem): Promise<void> {
    const userApp = await md.GetEntityObject<MJUserApplicationEntity>('MJ: User Applications');
    await userApp.Load(item.UserAppId!);
    userApp.Sequence = item.Sequence;
    userApp.IsActive = item.IsActive;
    const saved = await userApp.Save();
      throw new Error(`Failed to update UserApplication for ${item.App.Name}: ${userApp.LatestResult}`);
    item.IsDirty = false;
    LogStatus(`Updated UserApplication for ${item.App.Name}: sequence=${item.Sequence}, isActive=${item.IsActive}`);
   * Creates a new UserApplication record
  private async CreateUserApplication(md: Metadata, item: AppConfigItem): Promise<void> {
    userApp.NewRecord();
    userApp.UserID = md.CurrentUser.ID;
    userApp.ApplicationID = item.App.ID;
      throw new Error(`Failed to create UserApplication for ${item.App.Name}: ${userApp.LatestResult}`);
    item.UserAppId = userApp.ID;
    LogStatus(`Created UserApplication for ${item.App.Name}: sequence=${item.Sequence}`);
   * Resets all changes and reloads the configuration
  async Reset(): Promise<void> {
   * Check if drop target is active
  IsDropTarget(index: number): boolean {
    return this.dropTargetIndex === index;
