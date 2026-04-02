import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
 * Service for managing user avatar operations across the application.
 * NOTE: This service does NOT depend on any Explorer-specific packages to remain
 * usable across different Angular applications. All auth provider logic should
 * be handled by the calling code.
export class UserAvatarService {
  constructor(private http: HttpClient) {}
   * Syncs user avatar from an image URL (typically from auth provider profile).
   * Downloads the image, converts to Base64, and saves to the user entity.
   * @param user - The MJUserEntity to update with avatar data
   * @param imageUrl - URL to the image (can be from Microsoft Graph, Google, etc.)
   * @param authHeaders - Optional headers for authenticated requests (e.g., { 'Authorization': 'Bearer token' })
   * @returns Promise<boolean> - true if avatar was synced and saved, false otherwise
  async syncFromImageUrl(
    user: MJUserEntity,
    imageUrl: string,
    authHeaders?: Record<string, string>
      if (!imageUrl || imageUrl.trim().length === 0) {
        console.warn('No image URL provided for avatar sync');
      // Fetch the image as a blob
      const headers: Record<string, string> = authHeaders || {};
      const blob: Blob = await firstValueFrom(
        this.http.get(imageUrl, { headers, responseType: 'blob' })
      // Convert blob to Base64 data URI
      const base64 = await this.blobToBase64(blob);
      // Update user entity
      user.UserImageURL = base64;
      user.UserImageIconClass = null; // Clear icon if we have an image
      const saved = await user.Save();
        console.log('Successfully synced avatar from image URL');
        console.warn('Failed to save avatar to database');
      console.warn('Could not sync avatar from image URL:', error);
   * Converts a Blob to a Base64 data URI string
   * Returns format: "data:image/png;base64,iVBORw0KG..."
  private blobToBase64(blob: Blob): Promise<string> {
      reader.onloadend = () => resolve(reader.result as string);
      reader.readAsDataURL(blob);
   * Converts a File to a Base64 data URI string
   * Used for file uploads in settings UI
  fileToBase64(file: File): Promise<string> {
      reader.onerror = error => reject(error);
   * Validates if a string is a valid URL
  isValidUrl(url: string): boolean {
    if (!url || url.trim().length === 0) {
      new URL(url);
   * Validates if a string is a valid Base64 data URI
  isValidBase64DataUri(dataUri: string): boolean {
    if (!dataUri || !dataUri.startsWith('data:')) {
    const regex = /^data:image\/(png|jpeg|jpg|gif|webp);base64,/;
    return regex.test(dataUri);
   * Gets the display URL for an avatar based on user settings
   * Priority: UserImageURL > UserImageIconClass > default
   * @param user - The MJUserEntity
   * @param defaultUrl - Optional default URL if no avatar is set
   * @returns The URL to display, or null if using an icon
  getAvatarDisplayUrl(user: MJUserEntity, defaultUrl: string = 'assets/user.png'): string | null {
      return user.UserImageURL;
    if (user.UserImageIconClass) {
      return null; // Indicates icon should be used instead
    return defaultUrl;
   * Gets the icon class for an avatar if using icon mode
  getAvatarIconClass(user: MJUserEntity, defaultIcon: string = 'fa-solid fa-user'): string | null {
      return user.UserImageIconClass;
    if (!user.UserImageURL) {
