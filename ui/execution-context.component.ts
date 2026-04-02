import { Component, Input, ChangeDetectionStrategy, OnChanges, SimpleChanges } from '@angular/core';
import { RunContextDetails } from '@memberjunction/testing-engine-base';
 * Displays execution context information for a test run or test suite run.
 * Shows machine info, user info, and detailed runtime context in a clean, organized layout.
  selector: 'mj-execution-context',
    <div class="execution-context">
      <!-- Machine & User Section -->
      <div class="context-section">
          <h4>Machine & User</h4>
        <div class="context-grid">
          @if (machineName) {
            <div class="context-item">
              <div class="context-icon">
                <i class="fas fa-desktop"></i>
              <div class="context-content">
                <div class="context-label">Machine Name</div>
                <div class="context-value monospace">{{ machineName }}</div>
          @if (machineId) {
                <i class="fas fa-fingerprint"></i>
                <div class="context-label">Machine ID</div>
                <div class="context-value monospace">{{ machineId }}</div>
          @if (runByUserName) {
                <div class="context-label">Run By</div>
                <div class="context-value">{{ runByUserName }}</div>
          @if (runByUserEmail) {
                <i class="fas fa-envelope"></i>
                <div class="context-label">Email</div>
                <div class="context-value">{{ runByUserEmail }}</div>
      <!-- Runtime Environment Section -->
      @if (contextDetails) {
            <i class="fas fa-cogs"></i>
            <h4>Runtime Environment</h4>
            @if (contextDetails.osType) {
                  <i class="fas" [ngClass]="getOSIcon()"></i>
                  <div class="context-label">Operating System</div>
                  <div class="context-value">{{ getOSDisplayName() }}</div>
                  @if (contextDetails.osVersion) {
                    <div class="context-detail">{{ contextDetails.osVersion }}</div>
            @if (contextDetails.nodeVersion) {
                  <i class="fab fa-node-js"></i>
                  <div class="context-label">Node.js</div>
                  <div class="context-value">{{ contextDetails.nodeVersion }}</div>
            @if (contextDetails.timezone) {
                  <i class="fas fa-globe"></i>
                  <div class="context-label">Timezone</div>
                  <div class="context-value">{{ contextDetails.timezone }}</div>
            @if (contextDetails.locale) {
                  <i class="fas fa-language"></i>
                  <div class="context-label">Locale</div>
                  <div class="context-value">{{ contextDetails.locale }}</div>
            @if (contextDetails.ipAddress) {
                  <i class="fas fa-network-wired"></i>
                  <div class="context-label">IP Address</div>
                  <div class="context-value monospace">{{ contextDetails.ipAddress }}</div>
      <!-- CI/CD Section -->
      @if (hasCIInfo()) {
        <div class="context-section ci-section">
            <i class="fas fa-rocket"></i>
            <h4>CI/CD Pipeline</h4>
          <div class="ci-banner" [ngClass]="getCIProviderClass()">
            <div class="ci-provider">
              <i class="fab" [ngClass]="getCIProviderIcon()"></i>
              <span class="ci-provider-name">{{ contextDetails?.ciProvider }}</span>
            @if (contextDetails?.pipelineId) {
                  <div class="context-label">Pipeline</div>
                  <div class="context-value">{{ contextDetails?.pipelineId }}</div>
            @if (contextDetails?.buildNumber) {
                  <i class="fas fa-hashtag"></i>
                  <div class="context-label">Build Number</div>
                  <div class="context-value monospace">{{ contextDetails?.buildNumber }}</div>
            @if (contextDetails?.branch) {
                  <i class="fas fa-code-branch"></i>
                  <div class="context-label">Branch</div>
                  <div class="context-value monospace">{{ contextDetails?.branch }}</div>
            @if (contextDetails?.prNumber) {
                  <i class="fas fa-code-pull-request"></i>
                  <div class="context-label">Pull Request</div>
                  <div class="context-value">#{{ contextDetails?.prNumber }}</div>
      @if (!hasAnyData()) {
          <h3>No Execution Context</h3>
          <p>Execution context information is not available for this run.</p>
    .execution-context {
    .context-section {
      background: var(--mj-card-background, #ffffff);
    .section-header i {
      color: var(--mj-text-color, #1f2937);
    .context-grid {
      background: var(--mj-background-subtle, #f9fafb);
    .context-item:hover {
      background: var(--mj-background-hover, #f3f4f6);
    .context-icon {
      background: var(--mj-primary-color-light, #dbeafe);
    .context-icon i {
    .context-content {
    .context-label {
    .context-value {
    .context-value.monospace {
      font-family: 'SF Mono', 'Monaco', 'Menlo', monospace;
    .context-detail {
    /* CI/CD Section Styles */
    .ci-section .section-header i {
    .ci-banner {
      background: linear-gradient(135deg, #1f2937, #374151);
    .ci-banner.github {
      background: linear-gradient(135deg, #24292e, #40444d);
    .ci-banner.azure {
      background: linear-gradient(135deg, #0078d4, #106ebe);
    .ci-banner.jenkins {
      background: linear-gradient(135deg, #d33833, #e6635c);
    .ci-banner.circleci {
      background: linear-gradient(135deg, #343434, #4a4a4a);
    .ci-banner.gitlab {
      background: linear-gradient(135deg, #fc6d26, #e24329);
    .ci-banner.travis {
      background: linear-gradient(135deg, #3eaaaf, #67c2b1);
    .ci-provider {
    .ci-provider i {
    .ci-provider-name {
    /* Node.js icon color */
    .fa-node-js {
      color: #68a063 !important;
export class ExecutionContextComponent implements OnChanges {
  /** Machine hostname */
  @Input() machineName: string | null = null;
  /** Machine ID (MAC address) */
  @Input() machineId: string | null = null;
  /** User name who ran the test */
  @Input() runByUserName: string | null = null;
  /** User email who ran the test */
  @Input() runByUserEmail: string | null = null;
  /** JSON string of RunContextDetails */
  @Input() runContextDetailsJson: string | null = null;
  /** Parsed context details */
  contextDetails: RunContextDetails | null = null;
    if (changes['runContextDetailsJson']) {
      this.parseContextDetails();
  private parseContextDetails(): void {
    if (this.runContextDetailsJson) {
        this.contextDetails = JSON.parse(this.runContextDetailsJson) as RunContextDetails;
        this.contextDetails = null;
  hasAnyData(): boolean {
      this.machineName ||
      this.machineId ||
      this.runByUserName ||
      this.runByUserEmail ||
      this.contextDetails
  hasCIInfo(): boolean {
      this.contextDetails?.ciProvider ||
      this.contextDetails?.pipelineId ||
      this.contextDetails?.buildNumber ||
      this.contextDetails?.branch ||
      this.contextDetails?.prNumber
  getOSIcon(): string {
    const osType = this.contextDetails?.osType?.toLowerCase();
    if (osType === 'darwin') return 'fa-apple';
    if (osType === 'linux') return 'fa-linux';
    if (osType === 'win32' || osType === 'windows') return 'fa-windows';
    return 'fa-desktop';
  getOSDisplayName(): string {
    if (osType === 'darwin') return 'macOS';
    if (osType === 'linux') return 'Linux';
    if (osType === 'win32' || osType === 'windows') return 'Windows';
    return this.contextDetails?.osType || 'Unknown';
  getCIProviderClass(): string {
    const provider = this.contextDetails?.ciProvider?.toLowerCase() || '';
    if (provider.includes('github')) return 'github';
    if (provider.includes('azure')) return 'azure';
    if (provider.includes('jenkins')) return 'jenkins';
    if (provider.includes('circle')) return 'circleci';
    if (provider.includes('gitlab')) return 'gitlab';
    if (provider.includes('travis')) return 'travis';
  getCIProviderIcon(): string {
    if (provider.includes('github')) return 'fa-github';
    if (provider.includes('azure')) return 'fa-microsoft';
    if (provider.includes('jenkins')) return 'fa-jenkins';
    if (provider.includes('circle')) return 'fa-circle';
    if (provider.includes('gitlab')) return 'fa-gitlab';
    if (provider.includes('travis')) return 'fa-travis';
    return 'fa-rocket';
