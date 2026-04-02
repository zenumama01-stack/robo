import { ViewConfigSummary, QuickSaveEvent } from '../types';
 * QuickSaveDialogComponent - Focused modal for saving views quickly
 * Replaces the 7+ click "create new view" flow with a focused 2-3 click dialog.
 * Shows essential fields (name, description, share) plus a summary preview
 * of what the view configuration includes.
 * Footer buttons determine the action:
 * - No existing view: "Create View" button
 * - Existing view: "Update" (primary) + "Save As New" (secondary) buttons
 * <mj-quick-save-dialog
 *   [IsOpen]="showQuickSave"
 *   [ViewEntity]="currentView"
 *   [EntityName]="entity.Name"
 *   [Summary]="configSummary"
 *   [IsSaving]="isSaving"
 *   (Save)="onQuickSave($event)"
 *   (Close)="showQuickSave = false"
 *   (OpenAdvanced)="openConfigPanel()">
 * </mj-quick-save-dialog>
  selector: 'mj-quick-save-dialog',
  templateUrl: './quick-save-dialog.component.html',
  styleUrls: ['./quick-save-dialog.component.css']
export class QuickSaveDialogComponent implements OnChanges {
   * The existing view entity (null = creating new)
  @Input() ViewEntity: UserViewEntityExtended | null = null;
   * Display name of the entity being viewed
   * Summary of what the current view configuration includes
   * Whether a save is in progress
  @Input() IsSaving: boolean = false;
   * Whether to default to Save As New mode (no longer used for toggle, kept for API compat)
  @Input() DefaultSaveAsNew: boolean = false;
   * Emitted when the user saves
  @Output() Save = new EventEmitter<QuickSaveEvent>();
   * Emitted when user wants to open the full config panel
  @Output() OpenAdvanced = new EventEmitter<void>();
  public IsShared: boolean = false;
   * Initialize form from view entity or defaults
    if (this.ViewEntity) {
      this.Name = this.ViewEntity.Name;
      this.Description = this.ViewEntity.Description || '';
      this.IsShared = this.ViewEntity.IsShared;
      this.IsShared = false;
   * Handle save button click
   * @param saveAsNew - true to create a new view, false to update existing
  OnSave(saveAsNew: boolean): void {
    if (!this.Name.trim() || this.IsSaving) return;
      Description: this.Description,
      IsShared: this.IsShared,
      SaveAsNew: saveAsNew
   * Handle close/cancel
   * Open advanced configuration panel
  OnOpenAdvanced(): void {
    this.OpenAdvanced.emit();
