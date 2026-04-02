export type TestStatus = 'Passed' | 'Failed' | 'Skipped' | 'Error' | 'Running' | 'Pending' | 'Timeout';
  selector: 'app-test-status-badge',
      class="test-status-badge"
      [class.test-status-badge--passed]="status === 'Passed'"
      [class.test-status-badge--failed]="status === 'Failed'"
      [class.test-status-badge--skipped]="status === 'Skipped'"
      [class.test-status-badge--error]="status === 'Error'"
      [class.test-status-badge--running]="status === 'Running'"
      [class.test-status-badge--pending]="status === 'Pending'"
      [class.test-status-badge--timeout]="status === 'Timeout'"
      <span class="badge-text">{{ status }}</span>
    .test-status-badge {
    .test-status-badge i {
    .test-status-badge--passed {
    .test-status-badge--failed {
    .test-status-badge--skipped {
    .test-status-badge--error {
      border: 1px solid rgba(255, 152, 0, 0.2);
    .test-status-badge--running {
    .test-status-badge--pending {
    .test-status-badge--timeout {
      background: rgba(255, 152, 0, 0.15);
      border: 1px solid rgba(255, 152, 0, 0.3);
export class TestStatusBadgeComponent {
  @Input() status!: TestStatus;
    switch (this.status) {
        return 'fa-solid fa-times-circle';
        return 'fa-solid fa-stopwatch';
