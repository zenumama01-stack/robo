import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ContextMenuSelectEvent } from '@progress/kendo-angular-menu';
import { TreeItemAddRemoveArgs } from '@progress/kendo-angular-treeview';
  selector: 'mj-files-category-tree',
  templateUrl: './category-tree.html',
  styleUrls: ['./category-tree.css'],
export class CategoryTreeComponent implements OnInit {
  @Output() categorySelected = new EventEmitter<string | undefined>();
  public showNew: boolean = false;
  public newCategoryName = '';
  public selectedKeys = [];
  public renameFileCategory: MJFileCategoryEntity | undefined;
  public categoriesData: MJFileCategoryEntity[] = [];
  async createNewCategory() {
    this.showNew = true;
  cancelNewCategory() {
    this.showNew = false;
  async handleDrop(e: TreeItemAddRemoveArgs) {
    console.log(e);
    const sourceCategory: MJFileCategoryEntity = e.sourceItem.item.dataItem;
    const targetCategory: MJFileCategoryEntity = e.destinationItem.item.dataItem;
    sourceCategory.ParentID = targetCategory.ID;
    await sourceCategory.Save();
  async saveNewCategory() {
    const categoryEntity: MJFileCategoryEntity = await this.md.GetEntityObject('MJ: File Categories');
    categoryEntity.NewRecord();
    categoryEntity.Name = this.newCategoryName;
    await categoryEntity?.Save();
    this.categoriesData = [...this.categoriesData, categoryEntity];
  async deleteCategory(fileCategory: MJFileCategoryEntity) {
    const { ID } = fileCategory;
    const success = await fileCategory.Delete();
      console.error('Unable to delete file category:', fileCategory);
      this.sharedService.CreateSimpleNotification(`Unable to delete category '${fileCategory.Name}'`, 'error');
    this.categoriesData = this.categoriesData.filter((c) => c.ID !== ID);
  clearSelection() {
    this.categorySelected.emit(undefined);
  handleMenuSelect(e: ContextMenuSelectEvent) {
    const action = e.item?.text?.toLowerCase() ?? '';
      case 'rename':
        this.renameFileCategory = e.item.data;
        this.deleteCategory(e.item.data);
  cancelRename() {
    this.renameFileCategory?.Revert();
    this.renameFileCategory = undefined;
  async saveRename() {
    await this.renameFileCategory?.Save();
      EntityName: 'MJ: File Categories',
      this.categoriesData = <MJFileCategoryEntity[]>result.Results;
      throw new Error('Error loading file categories: ' + result.ErrorMessage);
