import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { FileUploadEvent } from '../file-upload/file-upload';
 * Downloads a file from the provided URL.
 * @param url - The URL of the file to be downloaded.
 * @param fileName - The name to be given to the downloaded file.
 * @param contentType - The content type of the file. Defaults to 'application/octet-stream'.
 * @returns Promise<void> - A promise that resolves when the file download is complete.
const downloadFromUrl = async (url: string, fileName: string, contentType?: string | null) => {
  // First, fetch the data and create a blob with the correct content type
  const rawBlob = await response.blob();
  const blob = new Blob([rawBlob], { type: contentType || 'application/octet-stream' });
  // Then add a temporary link to the document and click it
  const blobUrl = window.URL.createObjectURL(blob);
  link.href = blobUrl;
  link.setAttribute('download', fileName);
  link.parentNode?.removeChild(link);
  window.URL.revokeObjectURL(blobUrl);
const FileDownloadQuery = gql`
  query FileDownloadUrl($FileID: String!) {
    MJFile(ID: $FileID) {
      DownloadUrl
const FileDownloadQuerySchema = z.object({
  MJFile: z.object({
    ContentType: z.string().optional(),
    DownloadUrl: z.string(),
  selector: 'mj-files-grid',
  templateUrl: './files-grid.html',
  styleUrls: ['./files-grid.css'],
export class FilesGridComponent implements OnInit, OnChanges {
  public files: MJFileEntity[] = [];
  public editFile: MJFileEntity | undefined;
    if (changes['CategoryID']) {
   * Resets the edited file.
   * This method reverts any changes made to the edited file by calling the Revert method of the MJFileEntity class. It then sets the editFile property to undefined, indicating that there is no longer an edited file.
   * @returns void
  public resetEditFile() {
    this.editFile?.Revert();
    this.editFile = undefined;
   * Saves the edited file.
   * This method saves the changes made to the edited file. It first checks if there is an edited file available. If so, it sets the isLoading property to true to indicate that the save operation is in progress. Then, it calls the Save method of the edited file to save the changes. If the save operation is successful, it creates a success notification using the sharedService.CreateSimpleNotification method. The notification message includes the ID and name of the saved file. Finally, it sets the edited file to undefined and sets the isLoading property to false to indicate that the save operation is complete.
   * @returns Promise<void> - A promise that resolves when the save operation is complete.
   * @throws Error - If there is an error during the save operation.
  public async saveEditFile() {
    if (this.editFile) {
      const success = await this.editFile.Save();
        this.sharedService.CreateSimpleNotification(`Successfully saved file ${this.editFile.ID} ${this.editFile.Name}`, 'success');
        this.sharedService.CreateSimpleNotification(`Unable to save file ${this.editFile.ID} ${this.editFile.Name}`, 'error');
   * Downloads a file using the provided MJFileEntity.
   * @param file - The MJFileEntity representing the file to be downloaded.
   * @throws Error - If there is an error during the file download process.
  public downloadFile = async (file: MJFileEntity) => {
    const result = await GraphQLDataProvider.ExecuteGQL(FileDownloadQuery, {
      FileID: file.ID,
    const parsedResult = FileDownloadQuerySchema.safeParse(result);
      const downloadUrl = parsedResult.data.MJFile.DownloadUrl;
      const success = downloadFromUrl(downloadUrl, file.Name, file.ContentType);
        this.sharedService.CreateSimpleNotification(`Unable to download file ${file.ID} ${file.Name}`, 'error');
      console.error(parsedResult.error);
   * Determines whether a file can be deleted based on its status and creation time.
   * @param file - The MJFileEntity representing the file to be checked.
   * @returns boolean - True if the file can be deleted, false otherwise.
  public canBeDeleted(file: MJFileEntity): boolean {
    const status = file.Status;
    const deletable = status === 'Uploaded' || Date.now() - +file.__mj_CreatedAt > 10 * 60 * 60;
    // console.log({ status, deletable, ID: file.ID, CreatedAt: file.CreatedAt });
    return deletable;
   * Deletes a file using the provided MJFileEntity.
   * @param file - The MJFileEntity representing the file to be deleted.
   * @returns Promise<void> - A promise that resolves when the file deletion is complete.
   * @throws Error - If there is an error during the file deletion process.
  public deleteFile = async (file: MJFileEntity) => {
    const ID = file.ID;
    const Name = file.Name;
    const deleteResult = await file.Delete();
      this.sharedService.CreateSimpleNotification(`Successfully deleted file ${ID} ${Name}`, 'info');
      this.files = this.files.filter((f) => typeof f.ID === 'string' && f.ID !== ID);
      this.sharedService.CreateSimpleNotification(`Unable to delete file ${ID} ${Name}`, 'error');
   * Handles the file upload event, sending a notification in case of failure and otherwise adding
   * the newly uploaded files to the files currently displayed.
   * @param e - The file upload event.
  public handleFileUpload(e: FileUploadEvent) {
    if (!e.success) {
      this.sharedService.CreateSimpleNotification(`Unable to upload file '${e.file.name}'`, 'error');
    this.files.push(e.file);
   * Refreshes the data by running a view and loading the files.
   * @returns {Promise<void>} - A promise that resolves when the data is refreshed.
      EntityName: 'MJ: Files',
      ...(this.CategoryID !== undefined && { ExtraFilter: `CategoryID='${this.CategoryID}'` }),
      this.files = <MJFileEntity[]>result.Results ?? [];
      throw new Error('Error loading files: ' + result.ErrorMessage);
