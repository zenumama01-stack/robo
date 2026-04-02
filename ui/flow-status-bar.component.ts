 * Status bar at the bottom of the flow editor showing counts and state.
  selector: 'mj-flow-status-bar',
    <div class="mj-flow-status-bar">
      <span class="mj-flow-status-item">
        {{ NodeCount }} {{ NodeCount === 1 ? 'node' : 'nodes' }}
        {{ ConnectionCount }} {{ ConnectionCount === 1 ? 'connection' : 'connections' }}
      @if (SelectedCount > 0) {
          {{ SelectedCount }} selected
      <span class="mj-flow-status-item mj-flow-status-item--right">
        {{ ZoomLevel }}%
    .mj-flow-status-bar {
      padding: 4px 14px;
    .mj-flow-status-item {
      i { font-size: 10px; }
      &--right {
export class FlowStatusBarComponent {
  @Input() NodeCount = 0;
  @Input() ConnectionCount = 0;
  @Input() SelectedCount = 0;
  @Input() ZoomLevel = 100;
