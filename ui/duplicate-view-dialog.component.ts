import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { ViewConfigSummary } from '../types';
 * Event emitted when user confirms the duplicate action
export interface DuplicateViewEvent {
  /** The user-chosen name for the copy */
 * DuplicateViewDialogComponent - Modal for duplicating a view with a custom name
 * Shows the source view name, allows renaming the copy, and displays
 * a metadata summary (filters, columns, sorts) of what will be duplicated.
 * <mj-duplicate-view-dialog
 *   [IsOpen]="showDuplicateDialog"
 *   [SourceViewName]="viewToDuplicate?.Name"
 *   [Summary]="duplicateSummary"
 *   (Duplicate)="onDuplicateConfirmed($event)"
 *   (Cancel)="showDuplicateDialog = false">
 * </mj-duplicate-view-dialog>
  selector: 'mj-duplicate-view-dialog',
  templateUrl: './duplicate-view-dialog.component.html',
  styleUrls: ['./duplicate-view-dialog.component.css']
export class DuplicateViewDialogComponent implements OnChanges {
  @Input() SourceViewName: string = '';
  @Input() Summary: ViewConfigSummary | null = null;
  @Output() Duplicate = new EventEmitter<DuplicateViewEvent>();
  public NewName: string = '';
  public NameTouched: boolean = false;
      this.NewName = this.SourceViewName ? `${this.SourceViewName} (Copy)` : '';
      this.NameTouched = false;
  OnDuplicate(): void {
    if (!this.NewName.trim()) return;
    this.Duplicate.emit({ Name: this.NewName.trim() });
