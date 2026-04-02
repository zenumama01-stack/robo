import { BaseArtifactViewerPluginComponent } from '../base-artifact-viewer.component';
 * Viewer component for code artifacts (Python, C#, Java, TypeScript, JavaScript, SQL, CSS, XML)
  selector: 'mj-code-artifact-viewer',
    <div class="code-artifact-viewer" [ngClass]="cssClass">
        <div class="language-badge">{{ languageLabel }}</div>
        <button class="btn-icon" title="Copy Code" (click)="onCopy()">
          [(ngModel)]="codeContent"
          [language]="language"
          [readonly]="readonly"
    .code-artifact-viewer {
@RegisterClass(BaseArtifactViewerPluginComponent, 'CodeArtifactViewerPlugin')
export class CodeArtifactViewerComponent extends BaseArtifactViewerPluginComponent {
  public codeContent = '';
  public language = 'plaintext';
  public languageLabel = 'Code';
   * Code artifacts always have content to display
  public override get hasDisplayContent(): boolean {
    this.codeContent = this.getContent();
    this.detectLanguage();
  private detectLanguage(): void {
    const ct = this.contentType?.toLowerCase() || '';
    // Map content types to Monaco editor language modes
    if (ct.includes('python')) {
      this.language = 'python';
      this.languageLabel = 'Python';
    } else if (ct.includes('csharp') || ct.includes('c#')) {
      this.language = 'csharp';
      this.languageLabel = 'C#';
    } else if (ct.includes('java')) {
      this.language = 'java';
      this.languageLabel = 'Java';
    } else if (ct.includes('typescript')) {
      this.language = 'typescript';
      this.languageLabel = 'TypeScript';
    } else if (ct.includes('javascript')) {
      this.language = 'javascript';
      this.languageLabel = 'JavaScript';
    } else if (ct.includes('sql')) {
      this.language = 'sql';
      this.languageLabel = 'SQL';
    } else if (ct.includes('css')) {
      this.language = 'css';
      this.languageLabel = 'CSS';
    } else if (ct.includes('xml')) {
      this.language = 'xml';
      this.languageLabel = 'XML';
      this.language = 'plaintext';
      this.languageLabel = 'Code';
  onCopy(): void {
    if (this.codeContent) {
      navigator.clipboard.writeText(this.codeContent).then(() => {
        console.log('✅ Copied code to clipboard');
