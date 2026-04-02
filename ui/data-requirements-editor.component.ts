import { ComponentStudioStateService } from '../../services/component-studio-state.service';
  selector: 'mj-data-requirements-editor',
    <div class="data-requirements-editor">
        <span class="header-title">
          Data Requirements
              <i class="fa-solid fa-check"></i> Apply Changes
      <div class="editor-body">
        @if (State.SelectedComponent) {
          @if (HasDataRequirements) {
            <div class="summary-bar">
              <span class="summary-item">
                <i class="fa-solid fa-table"></i>
                {{ EntityCount }} {{ EntityCount === 1 ? 'entity' : 'entities' }}
                {{ QueryCount }} {{ QueryCount === 1 ? 'query' : 'queries' }}
              <span class="summary-item mode-badge">
                {{ DataMode }}
              [(ngModel)]="EditableContent"
              [placeholder]="'Enter data requirements JSON...'"
              (ngModelChange)="OnContentChanged()">
            <p>Select a component to edit its data requirements.</p>
    .data-requirements-editor {
    .header-title i {
    .summary-bar {
    .summary-item {
    .mode-badge {
      padding: 1px 8px;
    .editor-body {
    .json-editor-container mj-code-editor {
export class DataRequirementsEditorComponent {
  EditableContent: string = '';
  IsEditing: boolean = false;
  private _parsedRequirements: Record<string, unknown> | null = null;
  get HasDataRequirements(): boolean {
    return this._parsedRequirements != null;
  get EntityCount(): number {
    if (!this._parsedRequirements) return 0;
    const entities = this._parsedRequirements['entities'];
    return Array.isArray(entities) ? entities.length : 0;
  get QueryCount(): number {
    const queries = this._parsedRequirements['queries'];
    return Array.isArray(queries) ? queries.length : 0;
  get DataMode(): string {
    if (!this._parsedRequirements) return '';
    return (this._parsedRequirements['mode'] as string) || 'views';
    this.loadContent();
    this.State.StateChanged.subscribe(() => {
  OnContentChanged(): void {
    this.IsEditing = true;
    this.parseContent();
      const spec: ComponentSpec = JSON.parse(this.State.EditableSpec);
      const dataReq = JSON.parse(this.EditableContent);
      spec.dataRequirements = dataReq;
      this.State.UpdateSpec(spec);
      console.error('Error applying data requirements changes:', error);
  private loadContent(): void {
      const spec = JSON.parse(this.State.EditableSpec);
      const dataReq = spec.dataRequirements || { mode: 'views', entities: [], queries: [] };
      this.EditableContent = JSON.stringify(dataReq, null, 2);
      this.EditableContent = '';
      this._parsedRequirements = null;
  private parseContent(): void {
      this._parsedRequirements = JSON.parse(this.EditableContent);
