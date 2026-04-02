import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, OnChanges, SimpleChanges, inject, HostListener, ChangeDetectorRef, NgZone } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { MJApplicationEntity, MJApplicationEntityEntity, MJEntityEntity } from '@memberjunction/core-entities';
export interface ApplicationDialogData {
  application?: MJApplicationEntity;
  mode: 'create' | 'edit';
interface ApplicationEntityConfig {
  entity: MJEntityEntity;
  applicationEntity?: MJApplicationEntityEntity;
  defaultForNewUser: boolean;
  hasChanges: boolean;
export interface ApplicationDialogResult {
  action: 'save' | 'cancel';
  selector: 'mj-application-dialog',
  templateUrl: './application-dialog.component.html',
  styleUrls: ['./application-dialog.component.css']
export class ApplicationDialogComponent implements OnInit, OnDestroy, OnChanges {
  @Input() data: ApplicationDialogData | null = null;
  @Output() result = new EventEmitter<ApplicationDialogResult>();
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);
  private ngZone = inject(NgZone);
  public applicationForm: FormGroup;
  // Entity management
  public applicationEntities: ApplicationEntityConfig[] = [];
  public availableEntities: MJEntityEntity[] = [];
  public allEntities: MJEntityEntity[] = [];
  // Search filter for available entities
  // Section expansion state
  public sectionExpanded = {
    entities: true,
    systemInfo: false
  // Fullscreen state
  public isFullscreen = false;
    this.applicationForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
    // Initial setup
    if (changes['visible'] && this.visible) {
      this.initializeDialog();
  private async initializeDialog(): Promise<void> {
    if (!this.visible) return;
      // Load all entities first
      await this.loadAllEntities();
      if (this.data?.application && this.isEditMode) {
        await this.loadApplicationData();
      console.error('Error initializing dialog:', error);
        this.error = error instanceof Error ? error.message : 'Failed to load dialog data';
  private async loadAllEntities(): Promise<void> {
    const result = await rv.RunView<MJEntityEntity>({
    this.allEntities = result.Success ? result.Results : [];
    this.applicationForm.reset({
      name: '',
      description: ''
    this.availableEntities = [...this.allEntities];
  onEscapeKey(): void {
  public get windowTitle(): string {
    return this.isEditMode ? 'Edit Application' : 'Create New Application';
  public get isEditMode(): boolean {
    return this.data?.mode === 'edit';
  private async loadApplicationData(): Promise<void> {
    if (!this.data?.application) return;
    const app = this.data.application;
    this.applicationForm.patchValue({
      name: app.Name,
      description: app.Description
    // Load existing MJApplicationEntity records
    await this.loadApplicationEntities(app.ID);
  private async loadApplicationEntities(applicationId: string): Promise<void> {
        ExtraFilter: `ApplicationID='${applicationId}'`,
        OrderBy: 'Sequence ASC'
        const usedEntityIds = new Set<string>();
          const entity = this.allEntities.find(e => e.ID === appEntity.EntityID);
            this.applicationEntities.push({
              applicationEntity: appEntity,
              sequence: appEntity.Sequence || 0,
              defaultForNewUser: appEntity.DefaultForNewUser || false,
              hasChanges: false
            usedEntityIds.add(entity.ID);
        // Set available entities (excluding already assigned ones)
        this.availableEntities = this.allEntities.filter(e => !usedEntityIds.has(e.ID));
  public addEntity(entity: MJEntityEntity): void {
    // Add entity to application
      sequence: this.applicationEntities.length + 1,
      defaultForNewUser: false,
    // Update all sequences to be consecutive
    this.updateSequences();
    // Remove from available entities
    this.availableEntities = this.availableEntities.filter(e => e.ID !== entity.ID);
  public removeEntity(config: ApplicationEntityConfig): void {
    // Remove from application entities
    this.applicationEntities = this.applicationEntities.filter(ae => ae.entity.ID !== config.entity.ID);
    // Add back to available entities if not already there
    if (!this.availableEntities.find(e => e.ID === config.entity.ID)) {
      this.availableEntities.push(config.entity);
      this.availableEntities.sort((a, b) => (a.Name || '').localeCompare(b.Name || ''));
  public moveEntityUp(index: number): void {
      const temp = this.applicationEntities[index];
      this.applicationEntities[index] = this.applicationEntities[index - 1];
      this.applicationEntities[index - 1] = temp;
  public moveEntityDown(index: number): void {
    if (index < this.applicationEntities.length - 1) {
      this.applicationEntities[index] = this.applicationEntities[index + 1];
      this.applicationEntities[index + 1] = temp;
  private updateSequences(): void {
    this.applicationEntities.forEach((config, index) => {
      config.sequence = index + 1;
      if (!config.isNew) {
        config.hasChanges = true;
  public onDefaultForNewUserChange(config: ApplicationEntityConfig): void {
  public get hasEntityChanges(): boolean {
    return this.applicationEntities.some(ae => ae.isNew || ae.hasChanges);
  // Filtered available entities based on search term
  public get filteredAvailableEntities(): MJEntityEntity[] {
    if (!this.entitySearchTerm || !this.entitySearchTerm.trim()) {
      return this.availableEntities;
    const searchLower = this.entitySearchTerm.toLowerCase().trim();
    return this.availableEntities.filter(entity =>
      (entity.Name || '').toLowerCase().includes(searchLower) ||
      (entity.Description || '').toLowerCase().includes(searchLower)
  public onEntitySearchChange(event: Event): void {
    this.entitySearchTerm = value;
  public clearEntitySearch(): void {
  public toggleSection(section: 'basicInfo' | 'entities' | 'systemInfo'): void {
    this.sectionExpanded[section] = !this.sectionExpanded[section];
  public toggleFullscreen(): void {
    this.isFullscreen = !this.isFullscreen;
  public onEntityDrop(event: CdkDragDrop<ApplicationEntityConfig[]>): void {
    if (event.previousIndex !== event.currentIndex) {
      moveItemInArray(this.applicationEntities, event.previousIndex, event.currentIndex);
  public async onSubmit(): Promise<void> {
    if (this.applicationForm.invalid) {
      this.markFormGroupTouched(this.applicationForm);
      let application: MJApplicationEntity;
      if (this.isEditMode && this.data?.application) {
        // Edit existing application
        application = this.data.application;
        // Create new application
        application = await this.metadata.GetEntityObject<MJApplicationEntity>('MJ: Applications');
        application.NewRecord();
      // Update application properties
      const formValue = this.applicationForm.value;
      application.Name = formValue.name;
      application.Description = formValue.description || null;
      // Save application
      const saveResult = await application.Save();
        throw new Error(application.LatestResult?.Message || 'Failed to save application');
      // Save application entities if there are changes
      if (this.hasEntityChanges) {
        await this.saveApplicationEntities(application.ID);
      this.result.emit({ action: 'save', application });
        this.error = error instanceof Error ? error.message : 'An unexpected error occurred';
  private async saveApplicationEntities(applicationId: string): Promise<void> {
    // Save or update each MJApplicationEntity record
    for (const config of this.applicationEntities) {
      if (config.isNew || config.hasChanges) {
        let appEntity: MJApplicationEntityEntity;
        if (config.isNew) {
          // Create new MJApplicationEntity
          appEntity = await this.metadata.GetEntityObject<MJApplicationEntityEntity>('MJ: Application Entities');
          appEntity.NewRecord();
          appEntity.ApplicationID = applicationId;
          appEntity.EntityID = config.entity.ID;
        } else if (config.applicationEntity) {
          // Update existing MJApplicationEntity
          appEntity = config.applicationEntity;
        appEntity.Sequence = config.sequence;
        appEntity.DefaultForNewUser = config.defaultForNewUser;
        const saveResult = await appEntity.Save();
          console.warn(`Failed to save MJApplicationEntity for ${config.entity.Name}:`, appEntity.LatestResult?.Message);
    this.result.emit({ action: 'cancel' });
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
