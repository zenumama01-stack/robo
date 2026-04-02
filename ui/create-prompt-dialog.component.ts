import { Component, OnInit, OnDestroy, ChangeDetectorRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { WindowRef } from '@progress/kendo-angular-dialog';
import { MJTemplateEntity, MJAIPromptTypeEntity, MJTemplateContentEntity } from '@memberjunction/core-entities';
import { TemplateEditorConfig } from '../../shared/components/template-editor.component';
import { AIPromptManagementService } from '../AIPrompts/ai-prompt-management.service';
import { TemplateSelectorConfig } from '../AIPrompts/template-selector-dialog.component';
export interface CreatePromptConfig {
  /** Title for the dialog */
  /** Initial name for the prompt */
  /** Pre-selected prompt type ID */
export interface CreatePromptResult {
  /** Created prompt entity (not saved to database) */
  /** Created template entity (not saved to database) */
  template?: MJTemplateEntity;
  /** Template content entities (not saved to database) */
  templateContents?: MJTemplateContentEntity[];
 * Dialog for creating new AI Prompts with essential fields and basic template editing.
 * Creates entities but does not save them - returns entities for parent to add to PendingRecords.
 * This ensures atomicity with the parent form's save operation.
  selector: 'mj-create-prompt-dialog',
  templateUrl: './create-prompt-dialog.component.html',
  styleUrls: ['./create-prompt-dialog.component.css']
export class CreatePromptDialogComponent implements OnInit, OnDestroy {
  config: CreatePromptConfig = {};
  public result = new Subject<CreatePromptResult | null>();
  // Form and validation
  promptForm: FormGroup;
  // Data
  availablePromptTypes$ = new BehaviorSubject<MJAIPromptTypeEntity[]>([]);
  // Entities (not saved to database)
  promptEntity: AIPromptEntityExtended | null = null;
  templateEntity: MJTemplateEntity | null = null;
  templateContents: MJTemplateContentEntity[] = [];
  // Template editor
  @ViewChild('templateEditor') templateEditor: any; // Template editor component reference
  showTemplateEditor = false;
  templateEditorConfig: TemplateEditorConfig = {
    allowEdit: true,
    showRunButton: false,
    compactMode: true  // Compact mode for dialog
  // Template state
  templateMode: 'new' | 'existing' = 'new';
    private cdr: ChangeDetectorRef,
    private aiPromptManagementService: AIPromptManagementService
    this.promptForm = this.createForm();
    this.loadInitialData();
    this.setupFormWatching();
  private createForm(): FormGroup {
    return new FormGroup({
      name: new FormControl(this.config.initialName || '', [Validators.required]),
      description: new FormControl(''),
      typeID: new FormControl(this.config.initialTypeID || '', [Validators.required]),
      status: new FormControl('Pending'),
      outputType: new FormControl('string'),
      templateMode: new FormControl('new')
  private setupFormWatching() {
    // Watch template mode changes
    this.promptForm.get('templateMode')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(mode => {
        this.templateMode = mode;
        this.handleTemplateModeChange(mode);
  private async loadInitialData() {
      // Load prompt types
      const typesResult = await rv.RunView<MJAIPromptTypeEntity>({
        OrderBy: 'Name ASC',
      if (typesResult.Success && typesResult.Results) {
        this.availablePromptTypes$.next(typesResult.Results);
        // Set default type if not specified
        if (!this.config.initialTypeID && typesResult.Results.length > 0) {
          this.promptForm.patchValue({ typeID: typesResult.Results[0].ID });
      // Create the prompt entity
      this.promptEntity = await md.GetEntityObject<AIPromptEntityExtended>('MJ: AI Prompts');
      this.promptEntity.NewRecord();
      // Set default values
      this.promptEntity.Status = 'Pending';
      this.promptEntity.OutputType = 'string';
      this.promptEntity.ValidationBehavior = 'None';
      this.promptEntity.EnableCaching = false;
      // Create default template since it's required
      await this.createNewTemplate();
      this.showTemplateEditor = true;
      console.error('Error loading prompt creation data:', error);
        'Error loading data for prompt creation',
  public async handleTemplateModeChange(mode: string) {
    if (mode === 'new') {
    } else if (mode === 'existing') {
      await this.openTemplateSelector();
  private async createNewTemplate() {
    if (!this.promptEntity) return;
      // Create template entity
      this.templateEntity = await md.GetEntityObject<MJTemplateEntity>('MJ: Templates');
      this.templateEntity.NewRecord();
      const promptName = this.promptForm.get('name')?.value || 'New Prompt';
      this.templateEntity.Name = `${promptName} Template`;
      this.templateEntity.Description = `Template for ${promptName}`;
      this.templateEntity.UserID = md.CurrentUser.ID;
      // Link template to prompt
      this.promptEntity.TemplateID = this.templateEntity.ID;
      console.error('Error creating new template:', error);
        'Error creating template',
  public onTemplateContentChange(contents: MJTemplateContentEntity[]) {
    this.templateContents = contents || [];
  public async save() {
    if (!this.promptForm.valid || !this.promptEntity) {
        'Please fill in all required fields',
      // Update prompt entity with form values
      const formValue = this.promptForm.value;
      this.promptEntity.Name = formValue.name;
      this.promptEntity.Description = formValue.description || '';
      this.promptEntity.TypeID = formValue.typeID;
      this.promptEntity.Status = formValue.status;
      this.promptEntity.OutputType = formValue.outputType;
      // Get template contents if template editor is active
      if (this.templateEditor && this.showTemplateEditor) {
        // Get the template contents from the editor without saving
        // The parent form will handle saving in the proper order
        this.templateContents = this.templateEditor.templateContents || [];
        // Ensure the template contents have the correct TemplateID
        if (this.templateContents && this.templateEntity) {
          this.templateContents.forEach(content => {
            content.TemplateID = this.templateEntity!.ID;
      // Return the created entities (not saved to database)
      const result: CreatePromptResult = {
        prompt: this.promptEntity,
        template: this.templateEntity || undefined,
        templateContents: this.templateContents.length > 0 ? this.templateContents : undefined
      this.result.next(result);
      console.error('Error preparing prompt for creation:', error);
        'Error preparing prompt for creation',
  public cancel() {
   * Opens the template selector dialog to link an existing template
  private async openTemplateSelector() {
    const config: TemplateSelectorConfig = {
      title: 'Select Template for AI Prompt',
      showActiveOnly: true
      const result = await this.aiPromptManagementService.openTemplateSelectorDialog(config).toPromise();
      if (result && result.selectedTemplates && result.selectedTemplates.length > 0) {
        // Link the selected template
        this.templateEntity = result.selectedTemplates[0];
        this.promptEntity!.TemplateID = this.templateEntity.ID;
        // Update UI to show selected template info
        this.showTemplateEditor = false;
          `Template "${this.templateEntity.Name}" linked successfully`,
        // User cancelled, revert to new template mode
        this.promptForm.patchValue({ templateMode: 'new' });
      console.error('Error opening template selector:', error);
        'Error opening template selector',
      // Revert to new template mode
  // Getter for template debugging
  public get currentTemplate(): MJTemplateEntity | null {
    return this.templateEntity;
