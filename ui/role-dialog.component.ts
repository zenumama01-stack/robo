import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, OnChanges, SimpleChanges, inject, HostListener, ViewEncapsulation, ChangeDetectorRef, NgZone } from '@angular/core';
export interface RoleDialogData {
  role?: MJRoleEntity;
export interface RoleDialogResult {
  selector: 'mj-role-dialog',
  templateUrl: './role-dialog.component.html',
  styleUrls: ['./role-dialog.component.css']
export class RoleDialogComponent implements OnInit, OnDestroy, OnChanges {
  @Input() data: RoleDialogData | null = null;
  @Output() result = new EventEmitter<RoleDialogResult>();
  public roleForm: FormGroup;
    this.roleForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
      directoryId: ['']
    if (changes['data'] && this.data?.role && this.isEditMode) {
      this.loadRoleData();
    // Reset form if switching to create mode or dialog becomes visible
    if (changes['visible'] && this.visible && !this.isEditMode) {
    this.roleForm.reset({
      directoryId: ''
    return this.isEditMode ? 'Edit Role' : 'Create New Role';
  public get isSystemRole(): boolean {
    if (!this.data?.role) return false;
    return systemRoleNames.includes(this.data.role.Name || '');
  private loadRoleData(): void {
    if (!this.data?.role) return;
    const role = this.data.role;
    this.roleForm.patchValue({
      name: role.Name,
      description: role.Description,
      directoryId: role.DirectoryID
    // Disable name editing for system roles
    if (this.isSystemRole) {
      this.roleForm.get('name')?.disable();
    if (this.roleForm.invalid) {
      this.markFormGroupTouched(this.roleForm);
      let role: MJRoleEntity;
      if (this.isEditMode && this.data?.role) {
        // Edit existing role
        role = this.data.role;
        // Create new role
        role = await this.metadata.GetEntityObject<MJRoleEntity>('MJ: Roles');
        role.NewRecord();
      // Update role properties
      const formValue = this.roleForm.value;
      // Only update name if not a system role
      if (!this.isSystemRole) {
        role.Name = formValue.name;
      role.Description = formValue.description;
      role.DirectoryID = formValue.directoryId || null;
      // Save role
      const saveResult = await role.Save();
        throw new Error(role.LatestResult?.Message || 'Failed to save role');
      this.result.emit({ action: 'save', role });
      console.error('Error saving role:', error);
