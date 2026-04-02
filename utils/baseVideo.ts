 * Base class for all video generation models. Each AI model will have a sub-class implementing the abstract methods in this base class. Not all 
export abstract class BaseVideoGenerator extends BaseModel {
    public abstract CreateAvatarVideo(params: AvatarVideoParams): Promise<VideoResult>;
    public abstract CreateVideoTranslation(params: VideoTranslationParams): Promise<VideoResult>;
    public abstract GetAvatars(): Promise<AvatarInfo[]>;
export class VideoResult {
     * When success == false, this will contain the error message
     * Platform-specific video ID for the generated video when success == true
    videoId: string 
export class AvatarInfo {
    gender: string;
    previewImageUrl: string;
    previewVideoUrl: string;
export class VideoTranslationParams {
    // to be done
export class AvatarVideoParams {
     * Title of the video for storage in the provider's history
     * Generate captions for the video if true, otherwise do not generate captions
    caption?: boolean;
     * Width of the requested video such as 1280 for 1280 pixels
    outputWidth: number;
     * Height of the requested video such as 720 for 720 pixels
    outputHeight: number;
    avatarId: string;
    scale: number;
    offsetX: number;
    offsetY: number;
    audioAssetId: string;
    imageAssetId: string;
    avatarStyle: string; // 'circle' etc.
