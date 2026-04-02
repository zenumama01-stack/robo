import { Component, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { EntityInfo, CompositeKey, Metadata } from '@memberjunction/core';
import { ERDCompositeComponent, ERDCompositeState } from '@memberjunction/ng-entity-relationship-diagram';
import { ResourceData, MJUserSettingEntity, UserInfoEngine } from '@memberjunction/core-entities';
/** Settings key for ERD state persistence */
const ERD_SETTINGS_KEY = 'MJ.Admin.Entity.ERD';
  selector: 'mj-entity-admin-dashboard',
  templateUrl: './entity-admin-dashboard.component.html',
  styleUrls: ['./entity-admin-dashboard.component.css']
@RegisterClass(BaseDashboard, 'EntityAdmin')
export class EntityAdminDashboardComponent extends BaseDashboard implements AfterViewInit, OnDestroy {
  @ViewChild('erdComposite', { static: false }) erdComposite!: ERDCompositeComponent;
  public isRefreshingERD = false;
  // Filter panel visibility for header controls
  public filteredEntities: EntityInfo[] = [];
  private userStateChangeSubject = new Subject<ERDCompositeState>();
  private hasLoadedUserState = false;
  private userSettingEntity: MJUserSettingEntity | null = null;
    // Setup debounced state persistence to MJ: User Settings
    this.userStateChangeSubject.pipe(
      debounceTime(1000)
      this.saveStateToUserSettings(state);
    return "Entity Administration"
    this.userStateChangeSubject.complete();
    // Initialize dashboard - called by BaseDashboard
    // Data loading is handled by ERDCompositeComponent
    if (this.erdComposite) {
      this.erdComposite.onToggleFilterPanel();
  public onStateChange(state: ERDCompositeState): void {
    // Update local state to keep header controls in sync
    this.filteredEntities = this.erdComposite?.filteredEntities || [];
    if (state.selectedEntityId && this.erdComposite) {
      this.selectedEntity = this.erdComposite.entities.find(e => e.ID === state.selectedEntityId) || null;
    // Load user state when data becomes available for the first time
    if (this.erdComposite?.isDataLoaded && !this.hasLoadedUserState) {
      this.hasLoadedUserState = true;
      this.loadStateFromUserSettings();
  public onUserStateChange(state: ERDCompositeState): void {
    // Queue state for debounced persistence
    this.userStateChangeSubject.next(state);
  public onEntityOpened(entity: EntityInfo): void {
    this.openEntity(entity);
  public onOpenRecord(event: {EntityName: string, RecordID: string}): void {
      EntityName: event.EntityName,
      RecordPKey: new CompositeKey([{FieldName: 'ID', Value: event.RecordID}])
  public openEntity(entity: EntityInfo): void {
    this.Interaction.emit({
      type: 'openEntity',
      entity: entity,
      data: { entityId: entity.ID, entityName: entity.Name }
   * Load ERD state from MJ: User Settings entity using UserInfoEngine for cached access
  private async loadStateFromUserSettings(): Promise<void> {
      // Find setting from cached user settings
      const setting = engine.UserSettings.find(s => s.Setting === ERD_SETTINGS_KEY);
        this.userSettingEntity = setting;
        if (this.userSettingEntity.Value) {
          const savedState = JSON.parse(this.userSettingEntity.Value) as Partial<ERDCompositeState>;
            this.erdComposite.loadUserState(savedState);
      console.warn('Failed to load ERD state from User Settings:', error);
   * Save ERD state to MJ: User Settings entity using UserInfoEngine for cached lookup
  private async saveStateToUserSettings(state: ERDCompositeState): Promise<void> {
      // Find existing setting from cached user settings if not already loaded
      if (!this.userSettingEntity) {
          this.userSettingEntity = await this.metadata.GetEntityObject<MJUserSettingEntity>('MJ: User Settings');
          this.userSettingEntity.UserID = userId;
          this.userSettingEntity.Setting = ERD_SETTINGS_KEY;
      // Save the state as JSON
      this.userSettingEntity.Value = JSON.stringify(state);
      await this.userSettingEntity.Save();
      console.warn('Failed to save ERD state to User Settings:', error);
