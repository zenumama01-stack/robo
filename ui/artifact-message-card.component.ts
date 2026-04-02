import { UserInfo, RunView } from '@memberjunction/core';
import { ArtifactIconService } from '../services/artifact-icon.service';
 * Artifact message card component - displays a simple info bar for artifacts in conversation messages.
 * Shows artifact icon, name, type badge, and version. Click to open full artifact viewer.
  selector: 'mj-artifact-message-card',
    <div class="artifact-message-card" [class.loading]="loading" [class.error]="error">
        <div class="artifact-skeleton">
          <div class="skeleton-text"></div>
      } @else if (error) {
        <div class="artifact-error">
          <span>Failed to load artifact</span>
      } @else if (artifact) {
        <div class="artifact-info-bar" (click)="openFullView()">
          <div class="artifact-icon">
            <i class="fa-solid" [ngClass]="getArtifactIcon()"></i>
            <span class="artifact-name">{{ displayName }}</span>
              <span class="artifact-type-badge" [style.background]="getTypeBadgeColor()">
                {{ artifact.Type }}
              <span class="artifact-version">v{{ currentVersion?.VersionNumber || 1 }}</span>
          <div class="open-icon">
    .artifact-message-card {
    .artifact-skeleton {
      background: #F9FAFB;
      border: 1px solid #E5E7EB;
      background: #E5E7EB;
    .skeleton-text {
    .artifact-error {
      background: #FEE2E2;
      border: 1px solid #FECACA;
      color: #DC2626;
    .artifact-error i {
    .artifact-info-bar {
      transition: all 200ms ease;
    .artifact-info-bar:hover {
      border-color: #1e40af;
      box-shadow: 0 2px 8px rgba(30, 64, 175, 0.1);
    .artifact-icon {
      color: #6B7280;
    .artifact-type-badge {
    .artifact-version {
    .open-icon {
      color: #9CA3AF;
      transition: color 200ms ease;
    .artifact-info-bar:hover .open-icon {
export class ArtifactMessageCardComponent implements OnInit, OnDestroy {
  @Input() artifactId!: string;
  @Input() versionNumber?: number;
  @Input() currentUser!: UserInfo;
  @Input() artifact?: MJArtifactEntity; // Optional - if provided, skips loading
  @Input() artifactVersion?: MJArtifactVersionEntity; // Optional - if provided, skips loading
  @Output() actionPerformed = new EventEmitter<{action: string; artifact: MJArtifactEntity; version?: MJArtifactVersionEntity}>();
  public _artifact: MJArtifactEntity | null = null;
  public _currentVersion: MJArtifactVersionEntity | null = null;
  public loading = true;
  public error = false;
  constructor(private artifactIconService: ArtifactIconService) {}
    // If entities are provided, use them directly
    if (this.artifact && this.artifactVersion) {
      this._artifact = this.artifact;
      this._currentVersion = this.artifactVersion;
      // Otherwise load from database
      await this.loadArtifact();
  // Getters to access the internal properties
  public get artifactEntity(): MJArtifactEntity | null {
    return this._artifact;
  public get currentVersion(): MJArtifactVersionEntity | null {
    return this._currentVersion;
  private async loadArtifact(): Promise<void> {
    if (!this.artifactId) {
      this.error = true;
      this.error = false;
      // Load artifact directly
        EntityName: 'MJ: Conversation Artifacts',
        ExtraFilter: `ID='${this.artifactId}'`,
      }, this.currentUser);
        this._artifact = result.Results[0];
        // Load version content
        await this.loadVersionContent();
      console.error('Error loading artifact:', err);
  private async loadVersionContent(): Promise<void> {
    if (!this._artifact) return;
      const filter = this.versionNumber
        ? `ArtifactID='${this._artifact.ID}' AND VersionNumber=${this.versionNumber}`
        : `ArtifactID='${this._artifact.ID}'`;
        this._currentVersion = result.Results[0];
      console.error('Error loading version content:', err);
   * Get the display name - prefer version-specific name if available, otherwise use artifact name
  public get displayName(): string {
    if (this._currentVersion?.Name) {
      return this._currentVersion.Name;
    return this._artifact?.Name || 'Untitled';
   * Get the display description - prefer version-specific description if available, otherwise use artifact description
  public get displayDescription(): string | null {
    if (this._currentVersion?.Description) {
      return this._currentVersion.Description;
    return this._artifact?.Description || null;
  public get isCodeArtifact(): boolean {
    if (!this._artifact) return false;
    const name = this._artifact.Name?.toLowerCase() || '';
    const codeExtensions = ['.js', '.ts', '.jsx', '.tsx', '.py', '.java', '.cs', '.cpp', '.c', '.go', '.rs', '.sql', '.html', '.css', '.scss'];
    return codeExtensions.some(ext => name.endsWith(ext));
   * Get the icon for this artifact using the centralized icon service.
   * Fallback priority: Plugin icon > Metadata icon > Hardcoded mapping > Generic icon
  public getArtifactIcon(): string {
    if (!this._artifact) return 'fa-file';
    return this.artifactIconService.getArtifactIcon(this._artifact);
  public getTypeBadgeColor(): string {
    if (!this._artifact) return '#6B7280';
    const type = this._artifact.Type?.toLowerCase() || '';
    if (type.includes('code')) return '#8B5CF6'; // Purple
    if (type.includes('report')) return '#3B82F6'; // Blue
    if (type.includes('dashboard')) return '#10B981'; // Green
    if (type.includes('document')) return '#F59E0B'; // Orange
    return '#6B7280'; // Gray
  public openFullView(): void {
    if (this._artifact) {
      this.actionPerformed.emit({ action: 'open', artifact: this._artifact, version: this._currentVersion || undefined });
