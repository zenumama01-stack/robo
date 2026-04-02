import { MJUserEntity, MJUserSettingEntity } from '@memberjunction/core-entities';
 * Setting key for developer mode in MJ: User Settings entity
const DEVELOPER_MODE_SETTING_KEY = 'Explorer.DeveloperMode';
 * Service to manage Developer Mode functionality across Explorer apps.
 * Developer Mode shows additional debugging tools and developer-focused
 * features in the UI. Only users with Developer, Admin, or System Administrator
 * roles can enable developer mode.
 * Settings are persisted using the MJ: User Settings entity via UserInfoEngine.
 * constructor(private devMode: DeveloperModeService) {}
 * async ngOnInit() {
 *     await this.devMode.Initialize(userEntity);
 *     // Subscribe to changes
 *     this.devMode.IsEnabled$.subscribe(enabled => {
 *         this.showDevTools = enabled;
 *     });
export class DeveloperModeService {
    private _isEnabled$ = new BehaviorSubject<boolean>(false);
    private _isDeveloper$ = new BehaviorSubject<boolean>(false);
    private _currentUser: MJUserEntity | null = null;
    // Role names that qualify as "developer"
    private static readonly DEVELOPER_ROLES = [
        'Developer',
        'Admin',
        'System Administrator',
        'Integration'
     * Observable for developer mode enabled state.
     * Emits whenever developer mode is toggled.
    public get IsEnabled$(): Observable<boolean> {
        return this._isEnabled$.asObservable();
     * Observable for whether user has developer role.
     * This determines if they CAN enable developer mode.
    public get IsDeveloper$(): Observable<boolean> {
        return this._isDeveloper$.asObservable();
     * Current enabled state (synchronous access)
    public get IsEnabled(): boolean {
        return this._isEnabled$.value;
     * Whether user has developer role (synchronous access)
    public get IsDeveloper(): boolean {
        return this._isDeveloper$.value;
     * Whether the service has been initialized
    public get IsInitialized(): boolean {
        return this._initialized;
     * Initialize service with current user.
     * Call this after login/authentication completes.
    public async Initialize(user: MJUserEntity): Promise<void> {
        this._currentUser = user;
        // Check if user has a developer role
        const hasDeveloperRole = await this.CheckDeveloperRole(user);
        this._isDeveloper$.next(hasDeveloperRole);
        // Load saved preference from User Settings (only if user is a developer)
        if (hasDeveloperRole) {
            const savedState = await this.LoadSetting();
            this._isEnabled$.next(savedState);
            // Non-developers always have dev mode disabled
            this._isEnabled$.next(false);
     * Toggle developer mode on/off.
     * Only works if user has developer role.
     * @returns The new state, or false if user cannot enable dev mode
    public async Toggle(): Promise<boolean> {
        if (!this.IsDeveloper) {
            console.warn('Developer mode not available - user does not have Developer role');
        const newState = !this.IsEnabled;
        this._isEnabled$.next(newState);
        await this.SaveSetting(newState);
        return newState;
     * Enable developer mode (if user has developer role)
    public async Enable(): Promise<void> {
        if (this.IsDeveloper) {
            this._isEnabled$.next(true);
            await this.SaveSetting(true);
     * Disable developer mode
    public async Disable(): Promise<void> {
        await this.SaveSetting(false);
     * Reset the service (e.g., on logout)
    public Reset(): void {
        this._isDeveloper$.next(false);
        this._initialized = false;
        this._currentUser = null;
     * Load the developer mode setting from User Settings entity
    private async LoadSetting(): Promise<boolean> {
            const settingValue = engine.GetSetting(DEVELOPER_MODE_SETTING_KEY);
            return settingValue === 'true';
            console.warn('Failed to load developer mode setting:', error);
     * Save the developer mode setting to User Settings entity
    private async SaveSetting(enabled: boolean): Promise<void> {
            await engine.SetSetting(DEVELOPER_MODE_SETTING_KEY, String(enabled));
            console.warn('Failed to save developer mode setting:', error);
     * Check if user has a developer role by querying User Roles entity
    private async CheckDeveloperRole(user: MJUserEntity): Promise<boolean> {
            // Get user's roles via the User Roles junction table
            const userRolesResult = await rv.RunView<{ RoleID: string }>({
                ExtraFilter: `UserID='${user.ID}'`,
                Fields: ['RoleID']
            if (!userRolesResult.Success || !userRolesResult.Results?.length) {
            const roleIds = userRolesResult.Results.map(ur => ur.RoleID);
            // Get the role names
            const rolesResult = await rv.RunView<{ Name: string }>({
                ExtraFilter: `ID IN (${roleIds.map(id => `'${id}'`).join(',')})`,
                Fields: ['Name']
            if (!rolesResult.Success || !rolesResult.Results?.length) {
            // Check if any role matches our developer roles (case-insensitive)
            const userRoleNames = rolesResult.Results.map(r => r.Name.toLowerCase());
            return DeveloperModeService.DEVELOPER_ROLES.some(devRole =>
                userRoleNames.includes(devRole.toLowerCase())
            console.error('Error checking developer role:', error);
