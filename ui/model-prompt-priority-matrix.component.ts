import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { AIModelEntityExtended, AIPromptEntityExtended } from '@memberjunction/ai-core-plus';
interface PromptModelAssociation {
  promptId: string;
  promptName: string;
  association: MJAIPromptModelEntity | null;
  isNew: boolean;
  isModified: boolean;
interface MatrixCell {
  association: PromptModelAssociation | null;
  canAssign: boolean;
  selector: 'app-model-prompt-priority-matrix',
  templateUrl: './model-prompt-priority-matrix.component.html',
  styleUrls: ['./model-prompt-priority-matrix.component.css']
export class ModelPromptPriorityMatrixComponent implements OnInit, OnDestroy {
  @Input() selectedPrompts: AIPromptEntityExtended[] = [];
  @Input() selectedModels: AIModelEntityExtended[] = [];
  @Input() readonly = false;
  @Output() associationsChange = new EventEmitter<PromptModelAssociation[]>();
  @Output() stateChange = new EventEmitter<any>();
  @Output() promptSelected = new EventEmitter<AIPromptEntityExtended>();
  public prompts: AIPromptEntityExtended[] = [];
  public associations: PromptModelAssociation[] = [];
  public matrix: MatrixCell[][] = [];
  // UI State
  public loadingMessage = '';
  public viewMode: 'matrix' | 'list' = 'matrix';
  public sortBy: 'prompt' | 'model' | 'priority' = 'priority';
  public showInactiveAssociations = false;
  // Selection and editing
  public selectedCells: Set<string> = new Set();
  public editingCell: string | null = null;
  public bulkEditMode = false;
  public bulkEditPriority = 1;
  public bulkEditStatus = 'Active';
  public promptFilter$ = new BehaviorSubject<string>('');
  public modelFilter$ = new BehaviorSubject<string>('');
  public statusFilter$ = new BehaviorSubject<string>('all');
  public performanceData: { [key: string]: any } = {};
  public showPerformanceOverlay = false;
  public async loadData(): Promise<void> {
      this.loadingMessage = 'Loading prompts, models, and associations...';
      const [prompts, models, associations] = await Promise.all([
        this.loadPrompts(),
        this.loadModels(),
        this.loadAssociations()
      this.prompts = this.selectedPrompts.length > 0 ? this.selectedPrompts : prompts;
      this.models = this.selectedModels.length > 0 ? this.selectedModels : models;
      this.buildAssociations(associations);
      this.buildMatrix();
      LogStatus('Model-prompt priority matrix loaded successfully');
      this.error = 'Failed to load matrix data. Please try again.';
      LogError('Error loading matrix data', undefined, error);
  private async loadPrompts(): Promise<AIPromptEntityExtended[]> {
      UserSearchString: '',
      IgnoreMaxRows: false,
      return result.Results as AIPromptEntityExtended[];
      throw new Error('Failed to load AI prompts');
  private async loadModels(): Promise<AIModelEntityExtended[]> {
      ExtraFilter: "IsActive = 1",
      MaxRows: 200
      return result.Results as AIModelEntityExtended[];
      throw new Error('Failed to load AI models');
  private async loadAssociations(): Promise<MJAIPromptModelEntity[]> {
      OrderBy: 'Priority',
      MaxRows: 2000
      return result.Results as MJAIPromptModelEntity[];
      throw new Error('Failed to load prompt-model associations');
  private buildAssociations(dbAssociations: MJAIPromptModelEntity[]): void {
    this.associations = [];
    // Create associations for existing database records
    dbAssociations.forEach(dbAssoc => {
      const prompt = this.prompts.find(p => p.ID === dbAssoc.PromptID);
      const model = this.models.find(m => m.ID === dbAssoc.ModelID);
      if (prompt && model) {
        this.associations.push({
          modelName: model.Name,
          priority: dbAssoc.Priority || 1,
          status: dbAssoc.Status || 'Active',
          association: dbAssoc,
          isNew: false,
          isModified: false
  private buildMatrix(): void {
    this.matrix = [];
    this.prompts.forEach((prompt, promptIndex) => {
      this.matrix[promptIndex] = [];
      this.models.forEach((model, modelIndex) => {
        const association = this.associations.find(a => 
          a.promptId === prompt.ID && a.modelId === model.ID
        this.matrix[promptIndex][modelIndex] = {
          association: association || null,
          canAssign: this.canAssignModelToPrompt(prompt, model)
  private canAssignModelToPrompt(prompt: AIPromptEntityExtended, model: AIModelEntityExtended): boolean {
    if (prompt.OutputType && model.AIModelTypeID) {
      // Add business logic for compatibility checking
  public getCellKey(promptIndex: number, modelIndex: number): string {
    return `${promptIndex}-${modelIndex}`;
  public getCellClass(cell: MatrixCell): string {
    const classes = ['matrix-cell'];
    if (cell.association) {
      classes.push('has-association');
      classes.push(`priority-${Math.min(cell.association.priority, 5)}`);
      if (cell.association.status === 'Inactive') {
        classes.push('inactive');
      if (cell.association.isNew) {
        classes.push('new');
      if (cell.association.isModified) {
        classes.push('modified');
      classes.push('no-association');
    if (!cell.canAssign) {
      classes.push('cannot-assign');
    const cellKey = this.getCellKey(
      this.prompts.findIndex(p => p.ID === cell.promptId),
      this.models.findIndex(m => m.ID === cell.modelId)
    if (this.selectedCells.has(cellKey)) {
      classes.push('selected');
    if (this.editingCell === cellKey) {
      classes.push('editing');
    return classes.join(' ');
  public onCellClick(promptIndex: number, modelIndex: number, event: MouseEvent): void {
    if (this.readonly) return;
    const cellKey = this.getCellKey(promptIndex, modelIndex);
    const cell = this.matrix[promptIndex][modelIndex];
    if (event.ctrlKey || event.metaKey) {
      // Multi-select mode
        this.selectedCells.delete(cellKey);
        this.selectedCells.add(cellKey);
    } else if (event.shiftKey && this.selectedCells.size > 0) {
      // Range select mode
      this.selectRange(promptIndex, modelIndex);
      // Single select mode
      this.selectedCells.clear();
      if (cell.canAssign) {
  public onCellDoubleClick(promptIndex: number, modelIndex: number): void {
      this.editingCell = cellKey;
      if (!cell.association) {
        // Create new association
        this.createAssociation(cell.promptId, cell.modelId);
  private selectRange(endPromptIndex: number, endModelIndex: number): void {
    const selectedKeys = Array.from(this.selectedCells);
    if (selectedKeys.length === 0) return;
    const lastSelectedKey = selectedKeys[selectedKeys.length - 1];
    const [startPromptIndex, startModelIndex] = lastSelectedKey.split('-').map(Number);
    const minPromptIndex = Math.min(startPromptIndex, endPromptIndex);
    const maxPromptIndex = Math.max(startPromptIndex, endPromptIndex);
    const minModelIndex = Math.min(startModelIndex, endModelIndex);
    const maxModelIndex = Math.max(startModelIndex, endModelIndex);
    for (let p = minPromptIndex; p <= maxPromptIndex; p++) {
      for (let m = minModelIndex; m <= maxModelIndex; m++) {
        const cell = this.matrix[p][m];
        if (cell && cell.canAssign) {
          this.selectedCells.add(this.getCellKey(p, m));
  public createAssociation(promptId: string, modelId: string, priority: number = 1): void {
    const prompt = this.prompts.find(p => p.ID === promptId);
    const model = this.models.find(m => m.ID === modelId);
    if (!prompt || !model) return;
    const newAssociation: PromptModelAssociation = {
      modelId,
      association: null,
      isNew: true,
    this.associations.push(newAssociation);
    this.associationsChange.emit(this.associations);
  public updateAssociation(promptId: string, modelId: string, updates: Partial<PromptModelAssociation>): void {
    const associationIndex = this.associations.findIndex(a => 
      a.promptId === promptId && a.modelId === modelId
    if (associationIndex >= 0) {
      const association = this.associations[associationIndex];
      Object.assign(association, updates);
      if (!association.isNew) {
        association.isModified = true;
  public removeAssociation(promptId: string, modelId: string): void {
      this.associations.splice(associationIndex, 1);
  public bulkUpdateSelectedCells(): void {
    if (this.selectedCells.size === 0) return;
    this.selectedCells.forEach(cellKey => {
      const [promptIndex, modelIndex] = cellKey.split('-').map(Number);
          this.updateAssociation(cell.promptId, cell.modelId, {
            priority: this.bulkEditPriority,
            status: this.bulkEditStatus
          this.createAssociation(cell.promptId, cell.modelId, this.bulkEditPriority);
    this.bulkEditMode = false;
  public bulkRemoveSelectedCells(): void {
      if (cell && cell.association) {
        this.removeAssociation(cell.promptId, cell.modelId);
  public async saveChanges(): Promise<void> {
      this.loadingMessage = 'Saving associations...';
      if (!md) throw new Error('Metadata provider not available');
      const savePromises: Promise<boolean>[] = [];
      for (const association of this.associations) {
        if (association.isNew || association.isModified) {
          let entity: MJAIPromptModelEntity;
          if (association.association) {
            // Update existing
            entity = await md.GetEntityObject<MJAIPromptModelEntity>('MJ: AI Prompt Models', md.CurrentUser);
            await entity.Load(association.association.ID);
            // Create new
          entity.PromptID = association.promptId;
          entity.ModelID = association.modelId;
          entity.Priority = association.priority;
          entity.Status = association.status as any;
          savePromises.push(entity.Save());
      const results = await Promise.all(savePromises);
      const failures = results.filter(r => !r).length;
      if (failures === 0) {
        this.notificationService.CreateSimpleNotification('All associations saved successfully', 'success', 3000);
        // Reload data to get fresh state
        await this.loadData();
        this.notificationService.CreateSimpleNotification(`${failures} association(s) failed to save`, 'warning', 4000);
      this.error = 'Failed to save associations. Please try again.';
      LogError('Error saving associations', undefined, error);
      this.notificationService.CreateSimpleNotification('Failed to save associations', 'error', 4000);
  public hasUnsavedChanges(): boolean {
    return this.associations.some(a => a.isNew || a.isModified);
  public discardChanges(): void {
    if (!this.hasUnsavedChanges()) return;
    const confirm = window.confirm('Discard all unsaved changes?');
    if (confirm) {
  public getAssociationCount(): number {
    return this.associations.filter(a => a.status === 'Active').length;
  public getModelAssociationCount(modelId: string): number {
    return this.associations.filter(a => a.modelId === modelId && a.status === 'Active').length;
  public getPromptAssociationCount(promptId: string): number {
    return this.associations.filter(a => a.promptId === promptId && a.status === 'Active').length;
  public getCellTooltip(association: any): string {
    if (!association) return 'No association';
    return `Priority: ${association.priority || 'Not set'}`;
  public getAveragePriority(): number {
    const activeAssociations = this.associations.filter(a => a.status === 'Active');
    if (activeAssociations.length === 0) return 0;
    const sum = activeAssociations.reduce((total, a) => total + a.priority, 0);
    return Math.round((sum / activeAssociations.length) * 100) / 100;
  public sortAssociations(): void {
    this.associations.sort((a, b) => {
          comparison = a.promptName.localeCompare(b.promptName);
        case 'model':
          comparison = a.modelName.localeCompare(b.modelName);
        case 'priority':
          comparison = a.priority - b.priority;
  public toggleSortDirection(): void {
    this.sortAssociations();
  public onViewModeChange(mode: 'matrix' | 'list'): void {
    this.editingCell = null;
  public exportMatrix(): void {
      prompts: this.prompts.map(p => ({ id: p.ID, name: p.Name })),
      models: this.models.map(m => ({ id: m.ID, name: m.Name })),
      associations: this.associations.map(a => ({
        promptId: a.promptId,
        promptName: a.promptName,
        modelId: a.modelId,
        modelName: a.modelName,
        priority: a.priority,
        status: a.status
      exportDate: new Date().toISOString()
    const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' });
    a.download = `prompt-model-matrix-${new Date().toISOString().split('T')[0]}.json`;
  public selectPrompt(prompt: AIPromptEntityExtended): void {
    this.promptSelected.emit(prompt);
