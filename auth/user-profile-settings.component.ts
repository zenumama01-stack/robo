import { Component, OnInit, OnDestroy, ChangeDetectorRef, NgZone } from '@angular/core';
import { EventCodes, SharedService } from '@memberjunction/ng-shared';
interface IconCategory {
  icons: string[];
  selector: 'mj-user-profile-settings',
  templateUrl: './user-profile-settings.component.html',
  styleUrls: ['./user-profile-settings.component.css']
export class UserProfileSettingsComponent implements OnInit, OnDestroy {
  currentUser!: MJUserEntity;
  selectedTab: 'upload' | 'url' | 'icon' | 'provider' = 'url';
  // Form state
  imageUrlInput = '';
  selectedIconClass = '';
  uploadedImageBase64 = '';
  uploadedFileName = '';
  previewUrl = '';
  previewIconClass = '';
  showSuccessMessage = false;
  // Icon search state
  iconSearchTerm = '';
  iconSearch$ = new BehaviorSubject<string>('');
  filteredIconCategories: IconCategory[] = [];
  totalFilteredIcons = 0;
  // Icon picker data
  iconCategories: IconCategory[] = [
      name: 'Users',
      icons: [
        'fa-solid fa-user',
        'fa-solid fa-user-tie',
        'fa-solid fa-user-astronaut',
        'fa-solid fa-user-ninja',
        'fa-solid fa-user-secret',
        'fa-solid fa-user-graduate',
        'fa-solid fa-user-doctor',
        'fa-solid fa-user-gear',
        'fa-regular fa-circle-user',
        'fa-solid fa-user-check',
        'fa-solid fa-user-shield',
        'fa-solid fa-user-crown',
        'fa-solid fa-user-pilot',
        'fa-solid fa-user-cowboy',
        'fa-solid fa-user-chef'
      name: 'Business',
        'fa-solid fa-briefcase',
        'fa-solid fa-building',
        'fa-solid fa-chart-line',
        'fa-solid fa-handshake',
        'fa-solid fa-trophy',
        'fa-solid fa-medal',
        'fa-solid fa-award',
        'fa-solid fa-lightbulb',
        'fa-solid fa-rocket',
        'fa-solid fa-star'
      name: 'Tech',
        'fa-solid fa-laptop-code',
        'fa-solid fa-terminal',
        'fa-solid fa-microchip',
        'fa-solid fa-robot',
        'fa-solid fa-brain',
        'fa-solid fa-code',
        'fa-solid fa-server',
        'fa-solid fa-database',
        'fa-solid fa-network-wired',
        'fa-solid fa-bug'
      name: 'Fun',
        'fa-solid fa-face-smile',
        'fa-solid fa-face-grin',
        'fa-solid fa-face-laugh',
        'fa-solid fa-face-wink',
        'fa-solid fa-heart',
        'fa-solid fa-fire',
        'fa-solid fa-bolt',
        'fa-solid fa-gem',
        'fa-solid fa-crown',
        'fa-solid fa-hat-wizard'
      name: 'Animals',
        'fa-solid fa-cat',
        'fa-solid fa-dog',
        'fa-solid fa-dragon',
        'fa-solid fa-dove',
        'fa-solid fa-fish'
    this.currentUser = await md.GetEntityObject<MJUserEntity>('MJ: Users');
    await this.currentUser.Load(currentUserInfo.ID);
    // Initialize filtered icons
    this.filteredIconCategories = [...this.iconCategories];
    this.totalFilteredIcons = this.iconCategories.reduce(
      (sum, cat) => sum + cat.icons.length,
    // Setup icon search subscription
    this.setupIconSearchSubscription();
    this.loadCurrentAvatar();
   * Initializes the icon search subscription with debounce
  private setupIconSearchSubscription(): void {
    this.iconSearch$
        debounceTime(200), // Faster debounce for local filtering
      .subscribe((searchTerm) => {
        this.filterIcons(searchTerm);
   * Handles icon search input changes
  onIconSearchChange(event: Event): void {
    this.iconSearchTerm = value;
    this.iconSearch$.next(value);
   * Filters icons based on search term
   * Matches icon class name parts (e.g., "user" matches "fa-user-tie")
  private filterIcons(searchTerm: string): void {
      // Show all icons
    this.filteredIconCategories = [];
    this.totalFilteredIcons = 0;
    for (const category of this.iconCategories) {
      const matchingIcons = category.icons.filter((icon) => {
        // Extract icon name from class (e.g., "fa-solid fa-user-tie" -> "user-tie")
        const iconName = this.extractIconName(icon);
        return iconName.includes(term);
      if (matchingIcons.length > 0) {
        this.filteredIconCategories.push({
          name: category.name,
          icons: matchingIcons
        this.totalFilteredIcons += matchingIcons.length;
   * Extracts the icon name from a Font Awesome class string
   * e.g., "fa-solid fa-user-tie" -> "user-tie"
  extractIconName(iconClass: string): string {
    const parts = iconClass.split(' ');
      if (part.startsWith('fa-') && !['fa-solid', 'fa-regular', 'fa-light', 'fa-brands'].includes(part)) {
        return part.substring(3); // Remove "fa-" prefix
    return iconClass.toLowerCase();
   * Clears the icon search
  clearIconSearch(): void {
    this.iconSearchTerm = '';
    this.iconSearch$.next('');
   * Loads the current avatar settings from the user entity
  private loadCurrentAvatar(): void {
    if (this.currentUser.UserImageURL) {
      this.imageUrlInput = this.currentUser.UserImageURL;
      this.previewUrl = this.currentUser.UserImageURL;
      // Determine if it's a Base64 upload or URL
      if (this.userAvatarService.isValidBase64DataUri(this.currentUser.UserImageURL)) {
        this.selectedTab = 'upload';
        this.uploadedImageBase64 = this.currentUser.UserImageURL;
        this.uploadedFileName = 'Current uploaded image';
        this.selectedTab = 'url';
    } else if (this.currentUser.UserImageIconClass) {
      this.selectedIconClass = this.currentUser.UserImageIconClass;
      this.previewIconClass = this.currentUser.UserImageIconClass;
      this.selectedTab = 'icon';
      // Default to URL tab with empty state
   * Switches between tabs and updates preview
  selectTab(tab: 'upload' | 'url' | 'icon' | 'provider'): void {
    this.selectedTab = tab;
    this.updatePreview();
   * Handles file selection from native input
  async onFileSelected(event: Event): Promise<void> {
    if (!file) {
    // Clear any previous errors
    // Validate file type
    if (!file.type.match(/^image\/(png|jpeg|jpg|gif|webp)$/)) {
      this.errorMessage = 'Please select a valid image file (PNG, JPG, GIF, WEBP)';
      input.value = ''; // Clear the input
    // Validate file size (200KB)
    const maxSize = 200 * 1024;
      this.errorMessage = `Image must be smaller than 200KB. Your image is ${Math.round(file.size / 1024)}KB`;
    // Convert to Base64
      this.uploadedImageBase64 = await this.userAvatarService.fileToBase64(file);
      this.uploadedFileName = file.name;
      this.previewUrl = this.uploadedImageBase64;
      this.previewIconClass = ''; // Clear icon preview
      this.errorMessage = 'Failed to process image. Please try again.';
      console.error('Error converting file to Base64:', error);
   * Clears uploaded file
  clearUpload(): void {
    this.uploadedFileName = '';
    this.uploadedImageBase64 = '';
    this.previewUrl = '';
    // Reset file input
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
   * Handles URL input changes
  onUrlChange(): void {
    if (this.imageUrlInput && this.imageUrlInput.trim().length > 0) {
      if (this.userAvatarService.isValidUrl(this.imageUrlInput)) {
        this.previewUrl = this.imageUrlInput;
        this.errorMessage = 'Please enter a valid URL';
   * Handles icon selection
  selectIcon(iconClass: string): void {
    this.selectedIconClass = iconClass;
    this.previewIconClass = iconClass;
    this.previewUrl = ''; // Clear image preview
   * Checks if an icon is currently selected
  isIconSelected(iconClass: string): boolean {
    return this.selectedIconClass === iconClass;
   * Syncs avatar from authentication provider
   * NOTE: This is a placeholder - actual implementation should be done
   * in the calling application which has access to auth services
  async syncFromProvider(): Promise<void> {
    this.errorMessage = 'Avatar sync from provider is not yet implemented in settings. Please use the automatic sync on login or manually upload an image.';
    // TODO: Implement auth provider integration
    // The calling application should provide a way to get auth claims
    // and call userAvatarService.syncFromImageUrl() with the appropriate URL and headers
   * Reverts avatar to default (clears both fields)
   * This will trigger auto-sync from auth provider on next login
  async revertToDefault(): Promise<void> {
    this.showSuccessMessage = false;
      // Clear both avatar fields
      this.currentUser.UserImageURL = null;
      this.currentUser.UserImageIconClass = null;
      const saved = await this.currentUser.Save();
        // Clear local state
        this.imageUrlInput = '';
        this.selectedIconClass = '';
        this.previewIconClass = '';
        this.showSuccess('Avatar reverted to default! Your auth provider image will sync on next login.');
        // Notify header component to update avatar display
          eventCode: EventCodes.AvatarUpdated,
            imageUrl: null,
            iconClass: null
        this.errorMessage = 'Failed to revert avatar. Please try again.';
      console.error('Error reverting avatar:', error);
        this.errorMessage = 'An error occurred while reverting. Please try again.';
   * Updates the preview based on current tab
  private updatePreview(): void {
    switch (this.selectedTab) {
      case 'upload':
        if (this.uploadedImageBase64) {
        if (this.imageUrlInput && this.userAvatarService.isValidUrl(this.imageUrlInput)) {
      case 'icon':
        if (this.selectedIconClass) {
          this.previewIconClass = this.selectedIconClass;
   * Saves avatar settings to database
      // Update user entity based on selected tab
          if (!this.uploadedImageBase64) {
            this.errorMessage = 'Please select an image to upload';
          this.currentUser.UserImageURL = this.uploadedImageBase64;
          if (!this.imageUrlInput || !this.userAvatarService.isValidUrl(this.imageUrlInput)) {
            this.errorMessage = 'Please enter a valid image URL';
          this.currentUser.UserImageURL = this.imageUrlInput;
          if (!this.selectedIconClass) {
            this.errorMessage = 'Please select an icon';
          this.currentUser.UserImageIconClass = this.selectedIconClass;
        this.showSuccess('Avatar updated successfully!');
            imageUrl: this.currentUser.UserImageURL,
            iconClass: this.currentUser.UserImageIconClass
        this.errorMessage = 'Failed to save avatar. Please try again.';
      console.error('Error saving avatar:', error);
        this.errorMessage = 'An error occurred while saving. Please try again.';
   * Cancels changes and reverts to saved state
   * Shows success message temporarily
  private showSuccess(message: string): void {
    this.showSuccessMessage = true;
    this.sharedService.CreateSimpleNotification(message, 'success', 3000);
    // Hide success message after 3 seconds
