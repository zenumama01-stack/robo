import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TileLayoutReorderEvent, TileLayoutResizeEvent } from '@progress/kendo-angular-layout';
import { DashboardConfigDetails, DashboardItem } from '../../single-dashboard.component';
  selector: 'app-edit-dashboard',
  templateUrl: './edit-dashboard.component.html',
  styleUrls: ['./edit-dashboard.component.css']
export class EditDashboardComponent {
  @Output() onSave = new EventEmitter<any>();
  @Output() triggerAddItem = new EventEmitter<any>();
  @Input() public editMode: boolean = false;
  @Input() public config: DashboardConfigDetails = new DashboardConfigDetails();
  @Input() public items: DashboardItem[] = [];
  public _items: DashboardItem[] = [];
  public itemsChanged: boolean = false;
      this._items = [];
        this._items.push(dashboardItem);
  removeItem(e: any): void {
    // remove the selected item from the dashboard
    const index = this._items.indexOf(e);
      this._items.splice(index, 1);
      this.itemsChanged = true;
    const item = this._items.find(i => i.uniqueId === parseInt(e.item.elem.nativeElement.id));
        this._items.splice(e.oldIndex, 1);
        this._items.splice(e.newIndex, 0, item);  
  closeDialog(event: any = null): void {
  saveChanges() {
    this.onSave.emit({
      itemsChanged: this.itemsChanged,
      items: this._items,
      config: this.config,
  onItemSelect(event: any) {
    if(event.ID){
      this.triggerAddItem.emit(event);
