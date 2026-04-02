import { Component, Output, EventEmitter, Input  } from '@angular/core';
import { BaseEntity, EntityFieldInfo } from '@memberjunction/core';
  selector: 'mj-find-record-dialog',
  templateUrl: './dialog.component.html',
  styleUrls: ['./dialog.component.css']
export class FindRecordDialogComponent {
  @Input() DialogTitle: string = 'Find Record';
  @Input() DialogWidth: string = '700px';
  @Input() DialogHeight: string = '450px';
   * Optional, set this to the currently selected record to start the dialog with that record selected, if desired. This property will be updated as the user selects records in the dialog.
  @Input() SelectedRecord: BaseEntity | null = null;
      // do init stuff here as needed
  ///// REST OF THE BELOW JUST GET MAPPED TO THE CONTAINED RecordSelectorComponent
   * The name of the entity to show records for.
   * Optional, list of fields to be displayed in the grid of search results, if not specified, the default fields will be displayed
  @Input() DisplayFields: EntityFieldInfo[] = []; // Fields to display in the grid
    * When a record is selected, this event is emitted with the selected record
  @Output() OnRecordSelected = new EventEmitter<BaseEntity>();
  public BubbleOnRecordSelected(record: any) {
    this.SelectedRecord = record;
    this.OnRecordSelected.emit(record);
import { Component, Output, EventEmitter, Input, ChangeDetectorRef, ElementRef, ContentChild, AfterContentInit } from '@angular/core';
 * Generic base dialog component that can be used as a base for other dialogs by using this component as shown here in any other Angular component.
 * The custom-actions slot shown below is entirely optional, if you don't need anything beyond OK/Cancel, you can leave this out entirely. You can also turn off the
 * built-in OK and Cancel buttons using the ShowOKButton and ShowCancelButton properties.
 * <mj-generic-dialog DialogTitle="Your Dialog Title" [DialogVisible]="YourVisibleStateVariable" (DialogClosed)="YourDialogClosedEventHandler($event)">
 *   <div>
 *      Your content goes in here
 *   </div>
 *   <div custom-actions>
 *     <button kendoButton (click)="customOkClick()" themeColor="primary">Custom OK</button>
 *     <button kendoButton (click)="customCancelClick()">Custom Cancel</button>
 *     <button kendoButton (click)="additionalAction()">Additional Action</button>
 * </mj-generic-dialog>
  selector: 'mj-generic-dialog',
export class GenericDialogComponent implements AfterContentInit {
  @Input() DialogTitle: string = 'Default Title';
   * Optional, width of the dialog in pixels or percentage 
   * Optional, height of the dialog in pixels or percentage 
   * Ability to turn off the built-in OK button if it is not desired
  @Input() ShowOKButton: boolean = true;
   * Text displayed on the OK button, defaults to "OK" if not provided
  @Input() OKButtonText: string = "OK";
   * Text displayed on the Cancel button, defaults to "Cancel" if not provided
  @Input() CancelButtonText: string = "Cancel";
   * Ability to turn off the built-in Cancel button if it is not desired
   * Determines if the dialog is visible or not, bind this to a variable in your containing component that is changed to true when you want the dialog shown. When the user closes the dialog this property will 
   * be set to false automatically.
      // showing the dialog when it wasn't shown, refresh the data
      this.RefreshData.emit();
    this.cdr.detectChanges(); // Ensure visibility updates immediately
   * This event is fired during the component lifecycle if the dialog wants the user of the dialog to refresh the data provided within its content.
  @Output() RefreshData = new EventEmitter<void>();
   * Internal event handler for the Cancel button, you can call this method directly if you want to simulate the user clicking on the Cancel button
  public HandleCancelClick() {
   * Internal event handler for the OK button, you can call this method directly if you want to simulate the user clicking on the OK button
  public HandleOKClick() {
  private _hasCustomActions: boolean = false;
   * Returns true if the dialog has custom actions defined in the custom-actions slot
  public get HasCustomActions(): boolean {
    return this._hasCustomActions;
  @ContentChild('custom-actions', { static: false }) customActions!: ElementRef;
  ngAfterContentInit() {
      this._hasCustomActions = !!this.customActions;
  selector: 'mj-record-selector-dialog',
export class RecordSelectorDialogComponent {
  @Input() DialogTitle: string = 'Select Records';
      this.RefreshInitialValues();
   * The field name within the entity to show in the list items
  @Input() DisplayField: string = '';
   * The field name within the entity that has a CSS class representing an icon that should be displayed in the list items
  @Input() DisplayIconField: string = '';
   * The list of records that are available
  @Input() AvailableRecords: BaseEntity[] = [];
   * The list of records that are selected
  @Input() SelectedRecords: BaseEntity[] = [];
   * The list of records that are not selected
  @Input() UnselectedRecords: BaseEntity[] = [];
  protected RefreshInitialValues() {
    this._initialSelected = this.SelectedRecords.slice();
    this._initialUnselected = this.UnselectedRecords.slice();
    // now modify the SelectedRecords Array and UnselectedRecords arrays in place in order
    // to ensure they're the same arrays and drive data binding changes
    this.SelectedRecords.length = 0;
    this.UnselectedRecords.length = 0;
    this._initialSelected.forEach(r => this.SelectedRecords.push(r));
    this._initialUnselected.forEach(r => this.UnselectedRecords.push(r));
