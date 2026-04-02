import { MJUserSettingEntity, UserInfoEngine } from '@memberjunction/core-entities';
  EVALUATION_PREFS_SETTING_KEY
} from '../models/evaluation.types';
 * Service for managing user evaluation display preferences.
 * Persists preferences to MJ: User Settings entity.
export class EvaluationPreferencesService {
  private readonly _preferences$ = new BehaviorSubject<EvaluationPreferences>(DEFAULT_EVALUATION_PREFERENCES);
  private _settingEntity: MJUserSettingEntity | null = null;
  private _loaded = false;
  private _saving = false;
  /** Observable of current evaluation preferences */
  get preferences$(): Observable<EvaluationPreferences> {
    return this._preferences$.asObservable();
  /** Current preferences value */
  get preferences(): EvaluationPreferences {
    return this._preferences$.value;
  /** Whether preferences have been loaded */
  get loaded(): boolean {
    // Auto-load on first access
    this.load();
   * Load preferences from User Settings
  async load(): Promise<void> {
    if (this._loaded) return;
      const setting = engine.UserSettings.find(s => s.Setting === EVALUATION_PREFS_SETTING_KEY);
        this._settingEntity = setting;
        const parsed = JSON.parse(setting.Value) as Partial<EvaluationPreferences>;
        this._preferences$.next({
          ...DEFAULT_EVALUATION_PREFERENCES,
          ...parsed
      console.warn('Failed to load evaluation preferences:', error);
      // Keep defaults on error
   * Update a single preference
  async updatePreference<K extends keyof EvaluationPreferences>(
    key: K,
    value: EvaluationPreferences[K]
    const current = this._preferences$.value;
    const updated = { ...current, [key]: value };
    // Ensure at least one is enabled
      console.warn('At least one evaluation type must be enabled');
    this._preferences$.next(updated);
    await this.save(updated);
   * Update all preferences at once
  async updateAll(prefs: Partial<EvaluationPreferences>): Promise<void> {
    const updated = { ...this._preferences$.value, ...prefs };
   * Toggle a specific preference
    const newValue = !current[key];
    const updated = { ...current, [key]: newValue };
   * Reset to default preferences
    this._preferences$.next(DEFAULT_EVALUATION_PREFERENCES);
    await this.save(DEFAULT_EVALUATION_PREFERENCES);
   * Save preferences to User Settings
  private async save(prefs: EvaluationPreferences): Promise<void> {
    if (this._saving) return;
    this._saving = true;
        this._saving = false;
      // Find or create setting entity
      if (!this._settingEntity) {
        const existing = engine.UserSettings.find(s => s.Setting === EVALUATION_PREFS_SETTING_KEY);
          this._settingEntity = existing;
          this._settingEntity = await md.GetEntityObject<MJUserSettingEntity>('MJ: User Settings');
          this._settingEntity.UserID = userId;
          this._settingEntity.Setting = EVALUATION_PREFS_SETTING_KEY;
      this._settingEntity.Value = JSON.stringify(prefs);
      await this._settingEntity.Save();
      console.warn('Failed to save evaluation preferences:', error);
   * Check if showing any human-related metrics
  get showingHuman(): boolean {
    return this._preferences$.value.showHuman;
   * Check if showing any auto-related metrics
  get showingAuto(): boolean {
    return this._preferences$.value.showAuto;
   * Check if showing execution status
  get showingExecution(): boolean {
    return this._preferences$.value.showExecution;
