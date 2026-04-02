import { Component, Output, EventEmitter, Input, ViewChild } from '@angular/core';
import { BaseEntity  } from '@memberjunction/core';
import { ListBoxComponent, ListBoxToolbarConfig } from '@progress/kendo-angular-listbox';
  selector: 'mj-record-selector',
  templateUrl: './record-selector.component.html',
  styleUrls: ['./record-selector.component.css']
export class RecordSelectorComponent  {
  @ViewChild('unselected', { static: false }) unselectedListBox!: ListBoxComponent;
  @ViewChild('selected', { static: false }) selectedListBox!: ListBoxComponent;
  onDblClick(event: MouseEvent, listType: 'unselected' | 'selected'): void {
    const targetElement = event.target as HTMLElement;
    const listItemElement = targetElement.closest('.k-list-item');
    if (listItemElement && listItemElement.parentElement) {
      const itemIndex = Array.from(listItemElement.parentElement.children).indexOf(listItemElement);
      if (listType === 'unselected') {
        const item = this.UnselectedRecords[itemIndex];
        this.SelectedRecords.push(item);
        this.UnselectedRecords.splice(itemIndex, 1);
        const item = this.SelectedRecords[itemIndex];
        this.UnselectedRecords.push(item);
        this.SelectedRecords.splice(itemIndex, 1);
      if (this.unselectedListBox)
        this.unselectedListBox.clearSelection();
      if (this.selectedListBox)
        this.selectedListBox.clearSelection();
