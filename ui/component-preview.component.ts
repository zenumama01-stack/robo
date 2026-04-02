import { ReactComponentEvent, MJReactComponent } from '@memberjunction/ng-react';
  ComponentError
 * Viewport size preset for the component preview
export type ViewportSize = 'mobile' | 'tablet' | 'desktop';
interface ViewportPreset {
  Size: ViewportSize;
  MaxWidth: string;
 * Component Preview - TOP section of CENTER panel.
 * Renders the live React component preview with toolbar controls.
  selector: 'mj-component-preview',
  templateUrl: './component-preview.component.html',
  styleUrls: ['./component-preview.component.css']
export class ComponentPreviewComponent implements OnInit, OnDestroy {
  @ViewChild('reactComponent') ReactComponentRef?: MJReactComponent;
  @Output() AskAIToFix = new EventEmitter<ComponentError>();
  // --- Viewport ---
  public ActiveViewport: ViewportSize = 'desktop';
  public readonly ViewportPresets: ViewportPreset[] = [
    { Size: 'mobile', Label: 'Mobile (375px)', Icon: 'fa-mobile-screen', MaxWidth: '375px' },
    { Size: 'tablet', Label: 'Tablet (768px)', Icon: 'fa-tablet-screen-button', MaxWidth: '768px' },
    { Size: 'desktop', Label: 'Desktop (100%)', Icon: 'fa-desktop', MaxWidth: '100%' }
  // --- Local spec for refresh cycle ---
  public LocalComponentSpec: ComponentSpec | null = null;
    this.syncSpecFromState();
    this.State.StateChanged
    this.State.RefreshComponent
        this.refreshPreview();
  // TOOLBAR ACTIONS
  public RunSelectedComponent(): void {
    if (this.State.SelectedComponent) {
      this.State.RunComponent(this.State.SelectedComponent);
  public StopComponent(): void {
    MJReactComponent.forceClearRegistries();
  public RefreshComponent(): void {
    if (this.State.SelectedComponent && this.State.IsRunning) {
  public SetViewport(size: ViewportSize): void {
    this.ActiveViewport = size;
  public SendErrorToAI(): void {
    if (this.State.CurrentError) {
      this.AskAIToFix.emit(this.State.CurrentError);
      this.State.SendErrorToAI.emit(this.State.CurrentError);
  // VIEWPORT HELPERS
  public GetActivePreset(): ViewportPreset {
    return this.ViewportPresets.find(p => p.Size === this.ActiveViewport) || this.ViewportPresets[2];
  public GetPreviewContainerMaxWidth(): string {
    return this.GetActivePreset().MaxWidth;
  // REACT COMPONENT EVENTS
  public OnComponentEvent(event: ReactComponentEvent): void {
    if (event.type === 'error') {
      this.State.CurrentError = {
        type: event.payload?.source || 'Component Error',
        message: event.payload?.error || 'An error occurred while rendering the component',
        technicalDetails: event.payload?.errorInfo || event.payload
    } else if (event.type === 'loaded') {
      const resolvedSpec = event.payload?.resolvedSpec as ComponentSpec | undefined;
      if (resolvedSpec) {
        this.State.UpdateWithResolvedSpec(resolvedSpec);
  public OnOpenEntityRecord(event: { entityName: string; key: CompositeKey }): void {
    SharedService.Instance.OpenEntityRecord(event.entityName, event.key);
  // STATE HELPERS
  public GetComponentName(): string {
    if (!this.State.SelectedComponent) return '';
    return this.State.GetComponentName(this.State.SelectedComponent);
  public GetComponentDescription(): string | undefined {
    if (!this.State.SelectedComponent) return undefined;
    return this.State.GetComponentDescription(this.State.SelectedComponent);
  // PRIVATE
  private syncSpecFromState(): void {
    this.LocalComponentSpec = this.State.ComponentSpec;
   * Refresh the preview by nulling the spec, detecting changes,
   * then restoring the spec after a short delay.
  private refreshPreview(): void {
    if (!this.State.SelectedComponent) return;
    const spec = this.State.GetComponentSpec(this.State.SelectedComponent);
    // Null out to force React to unmount
    this.LocalComponentSpec = null;
    // Re-set after a brief pause to force fresh mount
      this.LocalComponentSpec = spec;
      this.State.ComponentSpec = spec;
      this.State.CurrentError = null;
        console.error('Error during refresh detectChanges:', error);
    }, 10);
