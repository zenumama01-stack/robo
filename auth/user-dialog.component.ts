import { MJUserEntity, MJRoleEntity, MJUserRoleEntity } from '@memberjunction/core-entities';
export interface UserDialogData {
  user?: MJUserEntity;
  availableRoles: MJRoleEntity[];
export interface UserDialogResult {
  selector: 'mj-user-dialog',
  templateUrl: './user-dialog.component.html',
  styleUrls: ['./user-dialog.component.css']
export class UserDialogComponent implements OnInit, OnDestroy, OnChanges {
  @Input() data: UserDialogData | null = null;
  @Output() result = new EventEmitter<UserDialogResult>();
  public userForm: FormGroup;
  public selectedRoleIds = new Set<string>();
  public existingUserRoles: MJUserRoleEntity[] = [];
    this.userForm = this.fb.group({
      name: ['', [Validators.required, Validators.email]],
      firstName: [''],
      lastName: [''],
      email: ['', [Validators.required, Validators.email]],
      type: ['User', Validators.required],
      isActive: [true]
    // Always clear state when data changes to prevent persistence bugs
    if (changes['data']) {
      this.selectedRoleIds.clear();
      this.existingUserRoles = [];
      if (this.data?.user && this.isEditMode) {
        this.loadUserData();
    // Reset form when dialog becomes visible and not in edit mode
    this.userForm.reset({
      firstName: '',
      lastName: '',
      email: '',
      type: 'User',
    return this.isEditMode ? 'Edit User' : 'Create New User';
  private async loadUserData(): Promise<void> {
    if (!this.data?.user) return;
    const user = this.data.user;
    this.userForm.patchValue({
      name: user.Name,
      firstName: user.FirstName,
      lastName: user.LastName,
      email: user.Email,
      title: user.Title,
      type: user.Type,
      isActive: user.IsActive
    // Load existing user roles
    await this.loadExistingUserRoles(user.ID);
  private async loadExistingUserRoles(userId: string): Promise<void> {
        ExtraFilter: `UserID='${userId}'`,
        this.existingUserRoles = result.Results;
        // Pre-select existing roles
        for (const userRole of this.existingUserRoles) {
          this.selectedRoleIds.add(userRole.RoleID);
      console.warn('Failed to load existing user roles:', error);
  public onRoleToggle(roleId: string, event: Event): void {
    const checkbox = event.target as HTMLInputElement;
    if (checkbox.checked) {
      this.selectedRoleIds.add(roleId);
      this.selectedRoleIds.delete(roleId);
  public toggleRole(roleId: string): void {
    if (this.selectedRoleIds.has(roleId)) {
    if (this.userForm.invalid) {
      this.markFormGroupTouched(this.userForm);
      let user: MJUserEntity;
      if (this.isEditMode && this.data?.user) {
        // Edit existing user
        user = this.data.user;
        // Create new user
        user = await this.metadata.GetEntityObject<MJUserEntity>('MJ: Users');
        user.NewRecord();
      // Update user properties
      const formValue = this.userForm.value;
      user.Name = formValue.name;
      user.FirstName = formValue.firstName;
      user.LastName = formValue.lastName;
      user.Email = formValue.email;
      user.Title = formValue.title;
      user.Type = formValue.type;
      user.IsActive = formValue.isActive;
      // Save user
      const saveResult = await user.Save();
        throw new Error(user.LatestResult?.Message || 'Failed to save user');
      // Handle role assignments
      await this.updateUserRoles(user.ID);
      this.result.emit({ action: 'save', user });
      console.error('Error saving user:', error);
  private async updateUserRoles(userId: string): Promise<void> {
      // Get current role IDs from existing UserRole entities
      const existingRoleIds = new Set(this.existingUserRoles.map(ur => ur.RoleID));
      // Determine roles to add and remove
      const rolesToAdd = Array.from(this.selectedRoleIds).filter(roleId => !existingRoleIds.has(roleId));
      const rolesToRemove = this.existingUserRoles.filter(userRole => !this.selectedRoleIds.has(userRole.RoleID));
      // Remove unselected roles
      for (const userRole of rolesToRemove) {
          await userRole.Delete();
          console.warn('Failed to remove role:', userRole.RoleID, error);
      // Add new selected roles
      for (const roleId of rolesToAdd) {
          userRole.RoleID = roleId;
          const saveResult = await userRole.Save();
            console.warn('Failed to assign role:', roleId, userRole.LatestResult?.Message);
          console.warn('Failed to assign role:', roleId, error);
      console.error('Error updating user roles:', error);
