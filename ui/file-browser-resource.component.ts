 * File Browser Resource Wrapper - displays the file browser in a tab
 * Extends BaseResourceComponent to work with the MJ resource type system
@RegisterClass(BaseResourceComponent, 'FileBrowserResource')
  selector: 'mj-file-browser-resource',
    <div class="file-browser-resource-container">
    .file-browser-resource-container {
export class FileBrowserResource extends BaseResourceComponent {
      this.loadFileBrowser();
   * Load the file browser (currently just notifies load complete)
   * In future phases, this could pass configuration from ResourceData
  private async loadFileBrowser(): Promise<void> {
      // File browser loads immediately
      // In future, could pass provider selection or folder path from Data.Configuration
      console.error('Error loading file browser:', error);
   * Get the display name for the file browser resource
    return data.Name || 'File Browser';
   * Get the icon class for file browser resources
  override async GetResourceIconClass(data: ResourceData): Promise<string> {
