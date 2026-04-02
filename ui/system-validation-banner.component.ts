import { SystemValidationService, SystemValidationIssue } from '../services/system-validation.service';
  selector: 'mj-system-validation-banner',
    @if (issues.length > 0) {
      <div class="system-validation-wrapper">
        <!-- Dark overlay for serious errors -->
        @if (hasErrors) {
          <div class="system-validation-overlay"></div>
        <div class="system-validation-container">
          @for (issue of issues; track issue) {
              class="system-validation-banner"
              [ngClass]="'system-validation-' + issue.severity">
              <div class="banner-content">
                <div class="banner-icon">
                  @if (issue.severity === 'error') {
                    <span class="severity-icon severity-error">❌</span>
                  @if (issue.severity === 'warning') {
                    <span class="severity-icon severity-warning">⚠️</span>
                  @if (issue.severity === 'info') {
                    <span class="severity-icon severity-info">ℹ️</span>
                <div class="banner-message">
                  <h3>{{ issue.message }}</h3>
                  @if (issue.details) {
                    <p>{{ issue.details }}</p>
                  @if (issue.help) {
                    <p class="help-text">{{ issue.help }}</p>
                <div class="banner-close">
                  @if (issue.severity !== 'error') {
                    <button (click)="dismissIssue(issue.id)" aria-label="Dismiss" class="dismiss-button">
    .system-validation-wrapper {
    .system-validation-overlay {
      background-color: rgba(0, 0, 0, 0.5);
    .system-validation-container {
    .system-validation-banner {
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      animation: slide-down 0.3s ease-out;
    .system-validation-error {
      background-color: #ffebee;
      border-left: 4px solid #f44336;
    .system-validation-warning {
      background-color: #fff8e1;
    .system-validation-info {
      background-color: #e3f2fd;
    .banner-content {
    .banner-icon {
    .severity-icon {
    .severity-error {
    .severity-warning {
    .severity-info {
    .banner-message {
    .banner-message h3 {
    .banner-message p {
    .dismiss-button {
    .dismiss-button:hover {
    @keyframes slide-down {
        transform: translateY(-20px);
export class SystemValidationBannerComponent implements OnInit, OnDestroy {
  issues: SystemValidationIssue[] = [];
  hasErrors = false;
  private subscription: Subscription | undefined;
    this.subscription = this.validationService.validationIssues$.subscribe(issues => {
      this.issues = issues;
      // Check if there are any error-level issues
      this.hasErrors = issues.some(issue => issue.severity === 'error');
    if (this.subscription) {
      this.subscription.unsubscribe();
  dismissIssue(id: string) {
    this.validationService.removeIssue(id);
