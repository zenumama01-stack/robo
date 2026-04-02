 * @fileoverview Type definitions for server-rendered SVG visualizations.
 * These types support the MemberJunction infographic system and provide
 * consistent interfaces across all visualization actions.
import { ActionResultSimple } from '@memberjunction/actions-base';
 * ViewBox configuration for SVG canvas dimensions and padding
export interface ViewBox {
    /** Final pixel width of the SVG */
    /** Final pixel height of the SVG */
    /** Padding around content (uniform or per-side) */
    padding?: number | { top?: number; right?: number; bottom?: number; left?: number };
 * Font specification for text rendering
export interface FontSpec {
    /** Font family stack (e.g., 'Inter, Arial, sans-serif') */
    family?: string;
    /** Font size in pixels (default: 12-14) */
    /** Font weight (400, 600, 'bold', etc.) */
    weight?: number | string;
    /** Line height in em units */
    lineHeight?: number;
 * Color palette configuration
export type Palette =
    | { type: 'named'; name: 'mjDefault' | 'gray' | 'pastel' | 'highContrast' }
    | {
          type: 'manual';
          background?: string;
          foreground?: string;
          categorical?: string[];
          sequential?: string[];
 * Branding and visual identity configuration
export interface Branding {
    /** Color palette to use */
    palette?: Palette;
    /** Font specification */
    font?: FontSpec;
    /** Optional logo SVG snippet to place in corner */
    logoSvg?: string;
    /** Optional watermark text */
    watermarkText?: string;
 * Accessibility configuration for WCAG compliance
export interface Accessibility {
    /** Title for <title> element */
    /** Description for <desc> element */
    desc?: string;
    /** ARIA role for root SVG */
    ariaRole?: 'img' | 'graphics-document';
    /** Include data table for screen readers */
    includeDataTable?: boolean;
 * Animation configuration (primarily for web display)
export interface AnimationOptions {
    /** Enable animations (default: false for print-first) */
    /** Prefer CSS animations over inline JS */
    preferCSS?: boolean;
    /** Animation duration in milliseconds */
    /** Easing function */
    ease?: 'linear' | 'ease' | 'ease-in' | 'ease-out' | 'ease-in-out';
    /** Stagger delay between items in milliseconds */
    staggerMs?: number;
 * Interactivity configuration (requires inline JS)
export interface InteractivityOptions {
    /** Enable interactivity (default: false) */
    /** Show tooltips on hover */
    tooltips?: boolean;
    /** Enable expand/collapse for trees */
    expandCollapse?: boolean;
    /** Enable pan and zoom */
    panZoom?: boolean;
    /** Minimum zoom scale (default: 0.5) */
    panZoomMinScale?: number;
    /** Maximum zoom scale (default: 3) */
    panZoomMaxScale?: number;
    /** Show zoom control buttons (default: false) */
    panZoomShowControls?: boolean;
    /** Highlight connected nodes on hover (network graphs) */
    highlightConnected?: boolean;
 * Export and rendering controls
export interface ExportControls {
    /** Inline CSS in <style> tag (default: true) */
    embedCSS?: boolean;
    /** Inline JavaScript in <script> tag (default: false) */
    embedJS?: boolean;
    /** Prefix for element IDs to ensure uniqueness */
    idPrefix?: string;
    /** Seed for deterministic random layouts */
    /** Wrap SVG in scrollable HTML container */
    wrapWithContainer?: boolean;
    /** Maximum container width (default: 1200) */
    maxContainerWidth?: number;
    /** Maximum container height (default: 800) */
    maxContainerHeight?: number;
 * Base configuration shared by all SVG actions
export interface ActionBase {
    /** ViewBox dimensions and padding */
    viewBox: ViewBox;
    /** Branding and visual identity */
    branding?: Branding;
    /** Accessibility configuration */
    a11y?: Accessibility;
    /** Animation options */
    animation?: AnimationOptions;
    /** Interactivity options */
    interactivity?: InteractivityOptions;
    /** Export controls */
    export?: ExportControls;
    /** Pass-through metadata for tracing/debugging */
    meta?: Record<string, unknown>;
 * Standardized result from SVG actions
export interface SVGActionResult extends ActionResultSimple {
    /** SVG XML string ready to embed */
    svg?: string;
    /** Width in pixels */
    /** Height in pixels */
    /** Warnings about rendering issues */
    warnings?: string[];
    /** Diagnostic information */
    diagnostics?: Record<string, unknown>;
// DIAGRAM-SPECIFIC TYPES
 * Types of nodes in flowcharts
export type FlowNodeKind = 'start' | 'end' | 'process' | 'decision' | 'input' | 'output' | 'subprocess';
 * Node in a flowchart
export interface FlowNode {
    /** Unique node identifier */
    /** Node type/shape */
    kind: FlowNodeKind;
    /** Display label */
    /** Optional note/description */
    note?: string;
    /** Optional icon SVG */
    /** Override width (auto-calculated if not provided) */
    /** Override height (auto-calculated if not provided) */
 * Edge connecting flowchart nodes
export interface FlowEdge {
    /** Source node ID */
    from: string;
    /** Target node ID */
    to: string;
    /** Optional edge label */
    /** Render as dashed line */
    dashed?: boolean;
 * Layout configuration for flowcharts
export interface FlowLayout {
    /** Layout direction */
    direction?: 'TB' | 'LR' | 'RL' | 'BT';
    /** Vertical spacing between ranks */
    rankSep?: number;
    /** Horizontal spacing between nodes */
    nodeSep?: number;
 * Node in an org chart
export interface OrgNode {
    /** Person/position name */
    /** Job title/role */
    /** Avatar URL (data URI or omitted) */
    /** Child nodes in hierarchy */
    children?: OrgNode[];
    /** Highlight this node */
    highlight?: boolean;
 * Attribute in an entity
export interface ERAttribute {
    /** Attribute name */
    /** Data type */
    /** Primary key indicator */
    pk?: boolean;
    /** Foreign key indicator */
    fk?: boolean;
 * Table/Entity in ER diagram
export interface ERTable {
    /** Unique table identifier */
    /** Table display name */
    /** Table attributes/columns */
    attrs: ERAttribute[];
 * Relationship between entities
export interface ERRelation {
    /** Source table ID */
    /** Target table ID */
    /** Relationship cardinality */
    label?: '1-1' | '1-N' | 'N-M';
// WORD CLOUD TYPES
 * Word with weight for cloud/tag visualization
export interface WordItem {
    /** The word/term */
    /** Weight/frequency/importance */
    weight: number;
 * Word cloud layout configuration
export interface CloudLayout {
    /** Rotation strategy */
    rotate?: 'none' | 'few' | 'mixed';
    /** Padding between words */
    /** Spiral algorithm type */
    spiral?: 'archimedean' | 'rectangular';
    /** Minimum font size */
    minFont?: number;
    /** Maximum font size */
    maxFont?: number;
// NETWORK/GRAPH TYPES
 * Node in a network graph
export interface GraphNode {
    /** Group/category for coloring */
    group?: string;
    /** Node size (auto-calculated if not provided) */
 * Edge connecting graph nodes
export interface GraphEdge {
    /** Optional edge identifier */
    source: string;
    target: string;
    /** Edge weight/strength */
    weight?: number;
    /** Directed edge (arrow) */
    directed?: boolean;
 * Network layout type
export type NetworkLayoutType = 'force' | 'radial' | 'tree' | 'grid';
 * Physics simulation parameters for force-directed layouts
export interface PhysicsParams {
    /** Charge/repulsion strength (negative = repel) */
    charge?: number;
    /** Ideal distance between connected nodes */
    linkDistance?: number;
    /** Number of simulation iterations */
 * Decision tree node (hierarchical)
export interface DecisionNode {
    /** Node label */
    /** Optional value/probability */
    /** Child nodes in tree */
    children?: DecisionNode[];
    /** Collapsed state (if interactivity enabled) */
    collapsed?: boolean;
    /** Optional note */
 * Node shape for decision trees
export type NodeShape = 'rect' | 'circle' | 'pill';
