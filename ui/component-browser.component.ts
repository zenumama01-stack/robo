import { Subscription } from 'rxjs';
  ComponentStudioStateService,
  DisplayComponent,
  FileLoadedComponent
} from '../../services/component-studio-state.service';
  selector: 'mj-component-browser',
  templateUrl: './component-browser.component.html',
  styleUrls: ['./component-browser.component.css']
export class ComponentBrowserComponent implements OnInit, OnDestroy {
  @Output() NewComponent = new EventEmitter<void>();
  @Output() ImportFromFile = new EventEmitter<void>();
  @Output() ImportFromText = new EventEmitter<void>();
  @Output() ImportFromArtifact = new EventEmitter<void>();
  public ImportDropdownOpen = false;
  private stateChangedSub: Subscription | null = null;
    this.stateChangedSub = this.State.StateChanged.subscribe(() => {
    if (this.stateChangedSub) {
      this.stateChangedSub.unsubscribe();
      this.stateChangedSub = null;
  // SEARCH
  OnSearchChange(value: string): void {
    this.State.SearchQuery = value;
  // CARD EXPANSION
  ToggleComponentExpansion(component: DisplayComponent): void {
    const componentId = this.State.GetComponentId(component);
    const expandedId = this.State.ExpandedComponent
      ? this.State.GetComponentId(this.State.ExpandedComponent)
    this.State.ExpandedComponent = expandedId === componentId ? null : component;
  IsExpanded(component: DisplayComponent): boolean {
    if (!this.State.ExpandedComponent) return false;
    return this.State.GetComponentId(this.State.ExpandedComponent) === this.State.GetComponentId(component);
  // RUNNING STATE CHECKS
  IsComponentRunning(component: DisplayComponent): boolean {
    if (!this.State.SelectedComponent || !this.State.IsRunning) return false;
    return this.State.GetComponentId(this.State.SelectedComponent) === this.State.GetComponentId(component);
  IsAnotherComponentRunning(component: DisplayComponent): boolean {
    return this.State.GetComponentId(this.State.SelectedComponent) !== this.State.GetComponentId(component);
  // ACTIONS
  OnRunComponent(component: DisplayComponent, event: Event): void {
    this.State.RunComponent(component);
  OnStopComponent(event: Event): void {
    this.State.StopComponent();
  OnSwitchComponent(component: DisplayComponent, event: Event): void {
  OnToggleFavorite(component: DisplayComponent, event: Event): void {
    this.State.ToggleFavorite(component);
  OnRemoveFileComponent(component: FileLoadedComponent, event: Event): void {
    this.State.RemoveFileLoadedComponent(component);
  // IMPORT DROPDOWN
  ToggleImportDropdown(): void {
    this.ImportDropdownOpen = !this.ImportDropdownOpen;
  CloseImportDropdown(): void {
    this.ImportDropdownOpen = false;
  OnImportFromFile(): void {
    this.CloseImportDropdown();
    this.ImportFromFile.emit();
  OnImportFromText(): void {
    this.ImportFromText.emit();
  OnImportFromArtifact(): void {
    this.ImportFromArtifact.emit();
    this.NewComponent.emit();
  GetStatusBadgeClass(component: DisplayComponent): string {
    if (this.State.IsFileLoadedComponent(component)) {
      const status = this.State.GetComponentStatus(component);
      if (status === 'Text') return 'text';
      if (status === 'Artifact') return 'artifact';
      return 'file';
    if (status === 'Deprecated') return 'deprecated';
    if (status === 'Published') return 'published';
    return 'draft';
  GetStatusLabel(component: DisplayComponent): string {
      return status || 'File';
    return this.State.GetComponentStatus(component) || 'Draft';
  GetFileBadgeTooltip(component: DisplayComponent): string {
    if (status === 'Text') return 'Imported from text input';
    if (status === 'Artifact') return 'Loaded from artifact';
    const filename = this.State.GetComponentFilename(component);
    return filename ? `Loaded from ${filename}` : 'Loaded from file';
  GetFileBadgeLabel(component: DisplayComponent): string {
    if (status === 'Text') return 'Text Import';
    if (status === 'Artifact') return 'Artifact';
    return this.State.GetComponentFilename(component) || 'File';
  GetFileBadgeIcon(component: DisplayComponent): string {
    if (status === 'Text') return 'fa-keyboard';
    if (status === 'Artifact') return 'fa-database';
  TrackByComponentId(_index: number, component: DisplayComponent): string {
    return this.State.GetComponentId(component);
  TrackByCategoryName(_index: number, category: { name: string }): string {
    return category.name;
