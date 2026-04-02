 * Internal toolbar for the generic flow editor canvas controls.
 * Provides zoom, fit-to-screen, auto-layout, and grid/minimap toggles.
  selector: 'mj-flow-toolbar',
    <div class="mj-flow-toolbar">
      <div class="mj-flow-toolbar-group">
        <button class="mj-flow-toolbar-btn" (click)="ZoomInClicked.emit()" title="Zoom In">
        <span class="mj-flow-toolbar-zoom">{{ ZoomLevel }}%</span>
        <button class="mj-flow-toolbar-btn" (click)="ZoomOutClicked.emit()" title="Zoom Out">
          <i class="fa-solid fa-magnifying-glass-minus"></i>
        <button class="mj-flow-toolbar-btn" (click)="FitToScreenClicked.emit()" title="Fit to Screen">
      <div class="mj-flow-toolbar-divider"></div>
        <button class="mj-flow-toolbar-btn"
          [class.mj-flow-toolbar-btn--active]="!PanMode"
          (click)="PanModeToggled.emit(false)"
          title="Select Mode (pointer)">
          [class.mj-flow-toolbar-btn--active]="PanMode"
          (click)="PanModeToggled.emit(true)"
          title="Pan Mode (drag to move canvas)">
          <button class="mj-flow-toolbar-btn" (click)="AutoLayoutClicked.emit()" title="Auto Arrange">
          [class.mj-flow-toolbar-btn--active]="ShowGrid"
          (click)="GridToggled.emit(!ShowGrid)"
          title="Toggle Grid">
          <i class="fa-solid fa-border-all"></i>
          [class.mj-flow-toolbar-btn--active]="ShowMinimap"
          (click)="MinimapToggled.emit(!ShowMinimap)"
          title="Toggle Minimap">
          <i class="fa-solid fa-map"></i>
          [class.mj-flow-toolbar-btn--active]="ShowLegend"
          (click)="LegendToggled.emit(!ShowLegend)"
          title="Toggle Legend">
    .mj-flow-toolbar {
    .mj-flow-toolbar-group {
    .mj-flow-toolbar-divider {
    .mj-flow-toolbar-btn {
    .mj-flow-toolbar-btn:hover {
    .mj-flow-toolbar-btn--active {
    .mj-flow-toolbar-btn--active:hover {
    .mj-flow-toolbar-zoom {
      min-width: 38px;
export class FlowToolbarComponent {
  @Input() PanMode = false;
  @Output() ZoomInClicked = new EventEmitter<void>();
  @Output() ZoomOutClicked = new EventEmitter<void>();
  @Output() FitToScreenClicked = new EventEmitter<void>();
  @Output() AutoLayoutClicked = new EventEmitter<void>();
  @Output() PanModeToggled = new EventEmitter<boolean>();
