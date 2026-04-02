 * A slide-in panel from the right that wraps the CreateAgentPanel.
 * <mj-create-agent-slidein
 *     (Closed)="showPanel = false">
 * </mj-create-agent-slidein>
    selector: 'mj-create-agent-slidein',
        <div class="cas-backdrop" [class.cas-visible]="IsVisible" (click)="OnClose()"></div>
        <div class="cas-panel" [class.cas-visible]="IsVisible" [style.width.px]="WidthPx">
          <div class="cas-resize-handle" (mousedown)="OnResizeStart($event)">
            <div class="cas-resize-grip"></div>
          <div class="cas-header">
            <div class="cas-title-group">
              <i class="fa-solid fa-robot cas-title-icon"></i>
                <h2 class="cas-title">{{ PanelTitle }}</h2>
                  <p class="cas-subtitle">
            <button class="cas-close-btn" (click)="OnClose()">
          <div class="cas-body">
        .cas-backdrop {
        .cas-backdrop.cas-visible {
        .cas-panel {
            min-width: 480px;
        .cas-panel.cas-visible {
        .cas-resize-handle {
        .cas-resize-handle:hover .cas-resize-grip,
        .cas-resize-handle:active .cas-resize-grip {
        .cas-resize-grip {
        .cas-resize-handle:hover {
        .cas-header {
        .cas-title-group {
        .cas-title-icon {
        .cas-title {
        .cas-subtitle {
        .cas-close-btn {
        .cas-close-btn:hover {
        .cas-body {
export class CreateAgentSlideInComponent {
    public WidthPx = 640;
    private readonly minWidth = 480;
    public get PanelTitle(): string {
