import { TemplateRef } from '@angular/core';
import { CustomToolbarButton } from './form-events';
 * Configuration object for the form toolbar.
 * Controls which buttons/sections are visible and behavioral options.
 * Pass to `<mj-form-toolbar [config]="toolbarConfig">` to customize the toolbar.
 * Use `DEFAULT_TOOLBAR_CONFIG` as a starting point and override specific properties.
 * const myConfig: FormToolbarConfig = {
 *   ...DEFAULT_TOOLBAR_CONFIG,
 *   ShowDeleteButton: false,         // Hide delete for this form
 *   ShowEntityHierarchy: true,       // Show IS-A breadcrumb
 *   ShowFavoriteButton: false,       // No favorites in this app
export interface FormToolbarConfig {
  // ---- Visibility ----
  /** Show the Edit button in read mode. Default: true */
  ShowEditButton: boolean;
  /** Show the Delete button in read mode. Default: true */
  ShowDeleteButton: boolean;
  /** Show the Favorite/Unfavorite button. Default: true */
  ShowFavoriteButton: boolean;
  /** Show the Record History button (only if entity tracks changes). Default: true */
  ShowHistoryButton: boolean;
  /** Show the Lists button for managing list membership. Default: true */
  ShowListButton: boolean;
  /** Show the IS-A entity hierarchy breadcrumb. Default: true */
  ShowEntityHierarchy: boolean;
  /** Show section controls (search, expand/collapse, count). Default: true */
  ShowSectionControls: boolean;
  /** Show the edit banner when entering edit mode. Default: true */
  ShowEditBanner: boolean;
  /** Show the "Changes" button when record is dirty in edit mode. Default: true */
  ShowChangesButton: boolean;
  /** Show the section search/filter input. Default: true */
  ShowSectionFilter: boolean;
  /** Allow drag-and-drop section reordering. Default: true */
  AllowSectionReorder: boolean;
  /** Show the "Manage Sections" button (section manager drawer). Default: true */
  ShowSectionManager: boolean;
  // ---- Behavior ----
   * Whether the toolbar should be sticky at the top when scrolling.
  StickyToolbar: boolean;
  // ---- Custom Slots ----
   * Optional template reference for additional toolbar actions.
   * Rendered after the standard buttons via ng-content or TemplateRef.
  AdditionalActions: TemplateRef<unknown> | null;
   * Custom toolbar buttons to display in the read-mode toolbar.
   * Shown after the standard buttons (edit, delete, favorite, history, lists).
  CustomButtons: CustomToolbarButton[];
 * Default toolbar configuration - everything visible and sticky.
 * Use as a base and override individual properties.
export const DEFAULT_TOOLBAR_CONFIG: FormToolbarConfig = {
  ShowEditButton: true,
  ShowDeleteButton: true,
  ShowFavoriteButton: true,
  ShowHistoryButton: true,
  ShowListButton: true,
  ShowEntityHierarchy: true,
  ShowSectionControls: true,
  ShowEditBanner: true,
  ShowChangesButton: true,
  ShowSectionFilter: true,
  AllowSectionReorder: true,
  ShowSectionManager: true,
  StickyToolbar: true,
  AdditionalActions: null,
  CustomButtons: []
 * Toolbar config for use inside MJ Explorer where favorites, history, and
 * list management are handled by the host app.
 * Pass as `[ToolbarConfig]="EXPLORER_TOOLBAR_CONFIG"` on the container.
export const EXPLORER_TOOLBAR_CONFIG: FormToolbarConfig = {
 * Configuration for a single toolbar button
export interface ToolbarButton {
  /** Unique identifier for the button */
  /** Tooltip text shown on hover */
  /** Optional text label to display alongside icon */
  /** Position within the toolbar */
  position?: 'left' | 'right';
  /** Whether the button is disabled */
  /** Whether the button is visible */
  visible?: boolean;
  /** Handler function called when button is clicked */
  handler?: (editor: EditorView) => void | Promise<void>;
  /** Optional custom component to render instead of default button */
  customRenderer?: Type<any>;
 * Group of toolbar buttons
export interface ToolbarButtonGroup {
  /** Unique identifier for the group */
  /** Buttons in this group */
  buttons: ToolbarButton[];
  /** Whether to show a separator after this group */
  separator?: boolean;
 * Main toolbar configuration
export interface ToolbarConfig {
  /** Whether the toolbar is enabled (defaults to false) */
  /** Position of the toolbar relative to editor */
  position?: 'top' | 'bottom';
  /** Individual buttons (simple configuration) */
  buttons?: ToolbarButton[];
  /** Button groups (advanced configuration) */
  groups?: ToolbarButtonGroup[];
  /** Custom CSS class to apply to toolbar */
  customClass?: string;
 * Event emitted when a toolbar button is clicked
export interface ToolbarActionEvent {
  /** ID of the button that was clicked */
  buttonId: string;
  /** Reference to the editor instance */
  editor: EditorView;
 * Pre-defined toolbar buttons for common operations
export const TOOLBAR_BUTTONS = {
  COPY: {
    id: 'copy',
    icon: 'fa-regular fa-copy',
    tooltip: 'Copy to clipboard',
    handler: async (editor: EditorView) => {
      const text = editor.state.doc.toString();
        // Could emit success event here
        console.error('Failed to copy text:', err);
  } as ToolbarButton,
  CUT: {
    id: 'cut',
    icon: 'fa-solid fa-scissors',
    tooltip: 'Cut',
        // Clear the editor
        editor.dispatch({
          changes: { from: 0, to: editor.state.doc.length, insert: '' }
        console.error('Failed to cut text:', err);
  PASTE: {
    id: 'paste',
    icon: 'fa-solid fa-paste',
    tooltip: 'Paste from clipboard',
        const text = await navigator.clipboard.readText();
        const pos = editor.state.selection.main.head;
          changes: { from: pos, to: pos, insert: text }
        console.error('Failed to paste text:', err);
  UNDO: {
    id: 'undo',
    icon: 'fa-solid fa-undo',
    tooltip: 'Undo',
    handler: (editor: EditorView) => {
      // CodeMirror will handle undo through its own commands
      editor.focus();
      document.execCommand('undo');
  REDO: {
    id: 'redo',
    icon: 'fa-solid fa-redo',
    tooltip: 'Redo',
      // CodeMirror will handle redo through its own commands
      document.execCommand('redo');
  FORMAT: {
    id: 'format',
    icon: 'fa-solid fa-indent',
    tooltip: 'Format code',
      // This would need language-specific formatting
      console.log('Format not yet implemented');
  SEARCH: {
    id: 'search',
    icon: 'fa-solid fa-search',
    tooltip: 'Search',
      // Open CodeMirror search panel
      // This would need to trigger CodeMirror's search command
      console.log('Search not yet implemented');
  FULLSCREEN: {
    id: 'fullscreen',
    tooltip: 'Toggle fullscreen',
      const container = editor.dom.closest('.mj-code-editor-wrapper');
        container.classList.toggle('fullscreen');
        // Update icon
        const button = container.querySelector('[data-button-id="fullscreen"] i');
        if (button) {
          button.className = container.classList.contains('fullscreen') 
            ? 'fa-solid fa-compress' 
            : 'fa-solid fa-expand';
  SELECT_ALL: {
    id: 'select-all',
    icon: 'fa-solid fa-i-cursor',
    tooltip: 'Select all',
        selection: { anchor: 0, head: editor.state.doc.length }
  CLEAR: {
    id: 'clear',
    icon: 'fa-solid fa-eraser',
    tooltip: 'Clear editor',
  } as ToolbarButton
 * Helper function to create a simple copy button configuration
export function createCopyButton(customLabel?: string): ToolbarButton {
    ...TOOLBAR_BUTTONS.COPY,
    label: customLabel
 * Helper function to create a toolbar with just a copy button
export function createCopyOnlyToolbar(): ToolbarConfig {
    buttons: [TOOLBAR_BUTTONS.COPY]
