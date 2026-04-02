import { ChangeDetectorRef, Component, NgZone, OnDestroy } from '@angular/core';
import { MJApplicationEntity, MJApplicationEntityEntity, ResourceData } from '@memberjunction/core-entities';
import { ApplicationDialogData, ApplicationDialogResult } from './application-dialog/application-dialog.component';
interface AppStats {
  totalApplications: number;
  activeApplications: number;
  totalEntities: number;
  publicEntities: number;
interface FilterOptions {
  status: 'all' | 'active' | 'inactive';
  search: string;
  selector: 'mj-application-management',
  templateUrl: './application-management.component.html',
  styleUrls: ['./application-management.component.css']
@RegisterClass(BaseDashboard, 'ApplicationManagement')
export class ApplicationManagementComponent extends BaseDashboard implements OnDestroy {
  public applications: MJApplicationEntity[] = [];
  public filteredApplications: MJApplicationEntity[] = [];
  public selectedApp: MJApplicationEntity | null = null;
  // Application entities mapping
  public appEntities: Map<string, MJApplicationEntityEntity[]> = new Map();
  public stats: AppStats = {
    totalApplications: 0,
    activeApplications: 0,
    totalEntities: 0,
    publicEntities: 0
  public filters$ = new BehaviorSubject<FilterOptions>({
    search: ''
  public showApplicationDialog = false;
  public applicationDialogData: ApplicationDialogData | null = null;
  public showDeleteConfirm = false;
  public expandedAppId: string | null = null;
  constructor(private cdr: ChangeDetectorRef, private ngZone: NgZone) {
    return "Application Management"
  public async loadInitialData(): Promise<void> {
      // Load applications and their entities
      const [apps, appEntities] = await Promise.all([
        this.loadApplications(),
        this.loadApplicationEntities()
      this.applications = apps;
      this.processApplicationEntities(appEntities);
      console.error('Error loading application data:', error);
      this.error = 'Failed to load application data. Please try again.';
  private async loadApplications(): Promise<MJApplicationEntity[]> {
    const result = await rv.RunView<MJApplicationEntity>({
      EntityName: 'MJ: Applications',
  private async loadApplicationEntities(): Promise<MJApplicationEntityEntity[]> {
      OrderBy: 'ApplicationID, Sequence'
  private processApplicationEntities(appEntities: MJApplicationEntityEntity[]): void {
    this.appEntities.clear();
    for (const appEntity of appEntities) {
      const appId = appEntity.ApplicationID;
      if (!this.appEntities.has(appId)) {
        this.appEntities.set(appId, []);
      this.appEntities.get(appId)!.push(appEntity);
    let filtered = [...this.applications];
    // Apply status filter - for now, all applications are considered active
    // In the future, we might add an IsActive field to the Applications table
    if (filters.status === 'inactive') {
      // Currently no way to determine inactive apps
      filtered = filtered.filter(app =>
        app.Name?.toLowerCase().includes(searchLower) ||
        app.Description?.toLowerCase().includes(searchLower)
    this.filteredApplications = filtered;
    // For now, consider all applications as active
    const activeApps = this.applications;
    let totalEntities = 0;
    let publicEntities = 0;
    for (const [, entities] of this.appEntities) {
      totalEntities += entities.length;
      publicEntities += entities.filter(e => e.DefaultForNewUser).length;
      totalApplications: this.applications.length,
      activeApplications: activeApps.length,
      totalEntities,
      publicEntities
  // Public methods for template
  public onSearchChange(event: Event): void {
    this.updateFilter({ search: value });
  public onStatusFilterChange(status: 'all' | 'active' | 'inactive'): void {
    this.updateFilter({ status });
  public updateFilter(partial: Partial<FilterOptions>): void {
      ...this.filters$.value,
      ...partial
  public toggleAppExpansion(appId: string): void {
    this.expandedAppId = this.expandedAppId === appId ? null : appId;
  public isAppExpanded(appId: string): boolean {
    return this.expandedAppId === appId;
  public getAppEntities(appId: string): MJApplicationEntityEntity[] {
    return this.appEntities.get(appId) || [];
  public getEntityInfo(entityId: string): any {
    return this.metadata.Entities.find(e => e.ID === entityId);
  public createNewApplication(): void {
    this.applicationDialogData = {
      mode: 'create'
    this.showApplicationDialog = true;
  public editApplication(app: MJApplicationEntity): void {
  public confirmDeleteApplication(app: MJApplicationEntity): void {
    this.selectedApp = app;
  public async deleteApplication(): Promise<void> {
    if (!this.selectedApp) return;
      // Delete the application
      const deleteResult = await this.selectedApp.Delete();
      if (!deleteResult) {
        throw new Error(this.selectedApp.LatestResult?.Message || 'Failed to delete application');
      this.selectedApp = null;
      console.error('Error deleting application:', error);
        this.error = error instanceof Error ? error.message : 'Failed to delete application';
  public onApplicationDialogResult(result: ApplicationDialogResult): void {
    this.showApplicationDialog = false;
    this.applicationDialogData = null;
    if (result.action === 'save') {
      // Refresh the application list after save
  public getAppIcon(app: MJApplicationEntity): string {
    // Map application names to appropriate icons based on their purpose
    const name = (app.Name || '').toLowerCase();
    // Common application type mappings
    if (name.includes('admin') || name.includes('management')) {
    if (name.includes('report') || name.includes('analytics') || name.includes('dashboard')) {
      return 'fa-chart-line';
    if (name.includes('user') || name.includes('people') || name.includes('employee')) {
      return 'fa-users';
    if (name.includes('settings') || name.includes('config')) {
      return 'fa-sliders';
    if (name.includes('data') || name.includes('database')) {
      return 'fa-database';
    if (name.includes('file') || name.includes('document')) {
      return 'fa-file-alt';
    if (name.includes('mail') || name.includes('email') || name.includes('message')) {
      return 'fa-envelope';
    if (name.includes('search') || name.includes('explorer')) {
      return 'fa-search';
    if (name.includes('calendar') || name.includes('schedule') || name.includes('event')) {
      return 'fa-calendar';
    if (name.includes('security') || name.includes('auth') || name.includes('permission')) {
      return 'fa-shield-alt';
    if (name.includes('integration') || name.includes('api') || name.includes('connect')) {
      return 'fa-plug';
    if (name.includes('workflow') || name.includes('process') || name.includes('automation')) {
      return 'fa-project-diagram';
    if (name.includes('ai') || name.includes('intelligence') || name.includes('machine')) {
      return 'fa-brain';
    if (name.includes('home') || name.includes('main') || name.includes('default')) {
      return 'fa-home';
    // Default icon for applications
    return 'fa-grid-2';
  public getAppStatusClass(app: MJApplicationEntity): string {
    // For now, all apps are considered active
  public getAppStatusLabel(app: MJApplicationEntity): string {
