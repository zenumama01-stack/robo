 * Configuration options for the markdown component
export interface MarkdownConfig {
   * Enable Prism.js syntax highlighting for code blocks
  enableHighlight?: boolean;
  enableMermaid?: boolean;
   * Enable copy-to-clipboard button on code blocks
  enableCodeCopy?: boolean;
  enableCollapsibleHeadings?: boolean;
   * Heading level at which to start collapsing (1-6)
   * Only applies when enableCollapsibleHeadings is true
  collapsibleHeadingLevel?: 1 | 2 | 3 | 4 | 5 | 6;
  collapsibleDefaultExpanded?: boolean;
   * - [] = All collapsed (same as collapsibleDefaultExpanded: false)
   * @default undefined (uses collapsibleDefaultExpanded)
   * Enable GitHub-style alerts ([!NOTE], [!WARNING], etc.)
  enableAlerts?: boolean;
   * Converts:
   * - "quotes" to "curly quotes"
   * - -- to en-dash (–)
   * - --- to em-dash (—)
   * - ... to ellipsis (…)
  enableSmartypants?: boolean;
  enableSvgRenderer?: boolean;
   * When enabled, HTML tags in the markdown are rendered as actual HTML
   * instead of being sanitized/stripped.
   * Note: Even with enableHtml=true, scripts and event handlers are stripped
   * unless enableJavaScript is also true.
  enableHtml?: boolean;
   * Enable JavaScript execution in HTML content.
   * When enabled, <script> tags and on* event handlers are allowed.
   * WARNING: This is a major security risk. Only enable for fully trusted content.
   * In most cases, you want enableHtml=true with enableJavaScript=false.
  enableJavaScript?: boolean;
   * Enable GitHub-style heading IDs for anchor links
  enableHeadingIds?: boolean;
   * Prefix for heading IDs to avoid conflicts
   * @default ''
  headingIdPrefix?: string;
  enableLineNumbers?: boolean;
   * Custom CSS class to apply to the markdown container
  containerClass?: string;
   * Prism.js theme to use (must be loaded in angular.json or via CSS import)
   * Common themes: 'prism', 'prism-dark', 'prism-okaidia', 'prism-tomorrow', 'prism-coy'
  prismTheme?: string;
   * Mermaid theme configuration
   * @default 'default'
  mermaidTheme?: 'default' | 'dark' | 'forest' | 'neutral' | 'base';
   * Set to false only if you trust the markdown source completely
  sanitize?: boolean;
export const DEFAULT_MARKDOWN_CONFIG: Required<Omit<MarkdownConfig, 'autoExpandLevels'>> & { autoExpandLevels?: number[] } = {
  enableHighlight: true,
  enableMermaid: true,
  enableCodeCopy: true,
  enableCollapsibleHeadings: false,
  collapsibleHeadingLevel: 2,
  collapsibleDefaultExpanded: true,
  enableAlerts: true,
  enableSmartypants: true,
  enableSvgRenderer: true,
  enableHtml: false,
  enableJavaScript: false,
  enableHeadingIds: true,
  headingIdPrefix: '',
  enableLineNumbers: false,
  containerClass: '',
  prismTheme: 'prism-okaidia',
  mermaidTheme: 'default',
  sanitize: true
 * Event emitted when markdown rendering is complete
export interface MarkdownRenderEvent {
   * The rendered HTML string
  html: string;
   * Time taken to render in milliseconds
  renderTime: number;
   * Whether mermaid diagrams were rendered
  hasMermaid: boolean;
   * Whether code blocks were highlighted
  hasCodeBlocks: boolean;
   * List of heading IDs generated (for TOC building)
  headingIds: HeadingInfo[];
 * Information about a heading in the document
export interface HeadingInfo {
   * The heading ID (for anchor links)
   * The heading text content
   * The heading level (1-6)
   * The raw markdown text
 * Alert types supported by marked-alert
export type AlertType = 'note' | 'tip' | 'important' | 'warning' | 'caution';
 * Configuration for a custom alert variant
export interface AlertVariant {
  titleClassName?: string;
