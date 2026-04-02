import { FacebookBaseAction, FacebookAlbum } from '../facebook-base.action';
import { MediaFile, SocialMediaErrorCode } from '../../../base/base-social.action';
 * Creates a photo album on a Facebook page and optionally uploads photos to it.
 * Albums help organize related photos together.
@RegisterClass(BaseAction, 'FacebookCreateAlbumAction')
export class FacebookCreateAlbumAction extends FacebookBaseAction {
        return 'Creates a photo album on a Facebook page and optionally uploads photos to it';
                Name: 'PageID',
                Name: 'AlbumName',
                Name: 'Location',
                Name: 'Privacy',
                Value: 'EVERYONE',
                Name: 'Photos',
                Name: 'PhotoCaptions',
                Name: 'CoverPhotoIndex',
                Value: 0,
                Name: 'MakeAlbumPublic',
            const pageId = this.getParamValue(Params, 'PageID');
            const albumName = this.getParamValue(Params, 'AlbumName');
            if (!pageId) {
                Message: 'PageID is required',
            if (!albumName) {
                Message: 'AlbumName is required',
            const description = this.getParamValue(Params, 'Description') as string;
            const location = this.getParamValue(Params, 'Location') as string;
            const privacy = this.getParamValue(Params, 'Privacy') as string;
            const photos = this.getParamValue(Params, 'Photos') as MediaFile[];
            const photoCaptions = this.getParamValue(Params, 'PhotoCaptions') as string[];
            const coverPhotoIndex = this.getParamValue(Params, 'CoverPhotoIndex') as number || 0;
            const makeAlbumPublic = this.getParamValue(Params, 'MakeAlbumPublic') !== false;
            // Validate photos are images
            if (photos && photos.length > 0) {
                for (const photo of photos) {
                    if (!photo.mimeType.startsWith('image/')) {
                Message: `Only image files are allowed in albums. File ${photo.filename} is ${photo.mimeType}`,
                ResultCode: 'INVALID_MEDIA'
            LogStatus(`Creating album "${albumName}" on Facebook page ${pageId}...`);
            // Create the album
            const albumData: any = {
                name: albumName,
                privacy: {
                    value: privacy
                albumData.message = description;
            if (location) {
                albumData.location = location;
            const albumResponse = await axios.post(
                `${this.apiBaseUrl}/${pageId}/albums`,
                albumData,
            const albumId = albumResponse.data.id;
            LogStatus(`Album created with ID: ${albumId}`);
            // Upload photos if provided
            const uploadedPhotos: any[] = [];
            let coverPhotoId: string | null = null;
                LogStatus(`Uploading ${photos.length} photos to the album...`);
                for (let i = 0; i < photos.length; i++) {
                    const photo = photos[i];
                    const caption = photoCaptions?.[i] || '';
                        const photoId = await this.uploadPhotoToAlbum(
                            albumId,
                            photo,
                            !makeAlbumPublic // Keep unpublished if album is not public yet
                        uploadedPhotos.push({
                            id: photoId,
                            filename: photo.filename,
                            caption
                        if (i === coverPhotoIndex) {
                            coverPhotoId = photoId;
                        LogStatus(`Uploaded photo ${i + 1}/${photos.length}: ${photo.filename}`);
                        LogError(`Failed to upload photo ${photo.filename}: ${error}`);
                        // Continue with other photos
            // Set cover photo if specified and photos were uploaded
            if (coverPhotoId) {
                    await this.setAlbumCoverPhoto(albumId, coverPhotoId, pageToken);
                    LogStatus(`Set cover photo for album`);
                    LogError(`Failed to set cover photo: ${error}`);
                    // Non-critical error, continue
            // Get the final album details
            const album = await this.getAlbumDetails(albumId, pageToken);
            LogStatus(`Album "${albumName}" created successfully with ${uploadedPhotos.length} photos`);
                Message: `Album created successfully with ${uploadedPhotos.length} photos`,
            LogError(`Failed to create Facebook album: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Upload a photo to an album
    private async uploadPhotoToAlbum(
        albumId: string,
        photo: MediaFile,
        caption: string,
        unpublished: boolean = false
        const fileData = typeof photo.data === 'string' 
            ? Buffer.from(photo.data, 'base64') 
            : photo.data;
            contentType: photo.mimeType
        if (caption) {
            formData.append('message', caption);
        if (unpublished) {
            formData.append('published', 'false');
            `${this.apiBaseUrl}/${albumId}/photos`,
            formData,
     * Set the cover photo for an album
    private async setAlbumCoverPhoto(albumId: string, photoId: string, pageToken: string): Promise<void> {
        await axios.post(
            `${this.apiBaseUrl}/${albumId}`,
                cover_photo: photoId
     * Get album details
    private async getAlbumDetails(albumId: string, pageToken: string): Promise<FacebookAlbum | null> {
            const response = await axios.get(
                        fields: 'id,name,description,link,cover_photo,count,created_time,photos.limit(1){id,images}'
            LogError(`Failed to get album details: ${error}`);
