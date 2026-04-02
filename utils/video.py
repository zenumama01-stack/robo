    video = await client.videos.create_and_poll(
        model="sora-2",
        prompt="A video of the words 'Thank you' in sparkling letters",
    if video.status == "completed":
        print("Video successfully completed: ", video)
        print("Video creation failed. Status: ", video.status)
from .video_size import VideoSize
from .video_model import VideoModel
from .video_seconds import VideoSeconds
from .video_create_error import VideoCreateError
__all__ = ["Video"]
class Video(BaseModel):
    """Structured information describing a generated video job."""
    """Unique identifier for the video job."""
    """Unix timestamp (seconds) for when the job completed, if finished."""
    """Unix timestamp (seconds) for when the job was created."""
    error: Optional[VideoCreateError] = None
    """Error payload that explains why generation failed, if applicable."""
    """Unix timestamp (seconds) for when the downloadable assets expire, if set."""
    model: VideoModel
    """The video generation model that produced the job."""
    object: Literal["video"]
    """The object type, which is always `video`."""
    progress: int
    """Approximate completion percentage for the generation task."""
    """The prompt that was used to generate the video."""
    remixed_from_video_id: Optional[str] = None
    """Identifier of the source video if this video is a remix."""
    seconds: Union[str, VideoSeconds]
    """Duration of the generated clip in seconds.
    For extensions, this is the stitched total duration.
    size: VideoSize
    """The resolution of the generated video."""
    status: Literal["queued", "in_progress", "completed", "failed"]
    """Current lifecycle status of the video job."""
class Video(OneDriveObjectBase):
    """Duration of the generated clip in seconds."""
