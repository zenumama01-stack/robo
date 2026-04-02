from typing import TYPE_CHECKING, Mapping, cast
from typing_extensions import Literal, assert_never
from ..types import (
    VideoSize,
    VideoSeconds,
    video_edit_params,
    video_list_params,
    video_remix_params,
    video_create_params,
    video_extend_params,
    video_create_character_params,
    video_download_content_params,
from ..pagination import SyncConversationCursorPage, AsyncConversationCursorPage
from ..types.video import Video
from .._utils._utils import is_given
from ..types.video_size import VideoSize
from ..types.video_seconds import VideoSeconds
from ..types.video_model_param import VideoModelParam
from ..types.video_delete_response import VideoDeleteResponse
from ..types.video_get_character_response import VideoGetCharacterResponse
from ..types.video_create_character_response import VideoCreateCharacterResponse
__all__ = ["Videos", "AsyncVideos"]
class Videos(SyncAPIResource):
    def with_raw_response(self) -> VideosWithRawResponse:
        return VideosWithRawResponse(self)
    def with_streaming_response(self) -> VideosWithStreamingResponse:
        return VideosWithStreamingResponse(self)
        input_reference: video_create_params.InputReference | Omit = omit,
        model: VideoModelParam | Omit = omit,
        seconds: VideoSeconds | Omit = omit,
        size: VideoSize | Omit = omit,
    ) -> Video:
        Create a new video generation job from a prompt and optional reference assets.
          prompt: Text prompt that describes the video to generate.
          input_reference: Optional reference asset upload or reference object that guides generation.
          model: The video generation model to use (allowed values: sora-2, sora-2-pro). Defaults
              to `sora-2`.
          seconds: Clip duration in seconds (allowed values: 4, 8, 12). Defaults to 4 seconds.
          size: Output resolution formatted as width x height (allowed values: 720x1280,
              1280x720, 1024x1792, 1792x1024). Defaults to 720x1280.
                "input_reference": input_reference,
                "seconds": seconds,
        files = extract_files(cast(Mapping[str, object], body), paths=[["input_reference"]])
            "/videos",
            body=maybe_transform(body, video_create_params.VideoCreateParams),
            cast_to=Video,
    def create_and_poll(
        poll_interval_ms: int | Omit = omit,
        """Create a video and wait for it to be processed."""
        video = self.create(
            prompt=prompt,
            input_reference=input_reference,
            seconds=seconds,
            size=size,
        return self.poll(
            video.id,
            poll_interval_ms=poll_interval_ms,
    def poll(
        video_id: str,
        """Wait for the vector store file to finish processing.
        Note: this will return even if the file failed to process, you need to check
        file.last_error and file.status to handle these cases
        headers: dict[str, str] = {"X-Stainless-Poll-Helper": "true"}
        if is_given(poll_interval_ms):
            headers["X-Stainless-Custom-Poll-Interval"] = str(poll_interval_ms)
            response = self.with_raw_response.retrieve(
                video_id,
                extra_headers=headers,
            video = response.parse()
            if video.status == "in_progress" or video.status == "queued":
                if not is_given(poll_interval_ms):
                    from_header = response.headers.get("openai-poll-after-ms")
                    if from_header is not None:
                        poll_interval_ms = int(from_header)
                        poll_interval_ms = 1000
                self._sleep(poll_interval_ms / 1000)
            elif video.status == "completed" or video.status == "failed":
                return video
                    assert_never(video.status)
        Fetch the latest metadata for a generated video.
        if not video_id:
            raise ValueError(f"Expected a non-empty value for `video_id` but received {video_id!r}")
            path_template("/videos/{video_id}", video_id=video_id),
    ) -> SyncConversationCursorPage[Video]:
        List recently generated videos for the current project.
          after: Identifier for the last item from the previous pagination request
          limit: Number of items to retrieve
          order: Sort order of results by timestamp. Use `asc` for ascending order or `desc` for
              descending order.
            page=SyncConversationCursorPage[Video],
                    video_list_params.VideoListParams,
            model=Video,
    ) -> VideoDeleteResponse:
        Permanently delete a completed or failed video and its stored assets.
            cast_to=VideoDeleteResponse,
    def create_character(
        name: str,
        video: FileTypes,
    ) -> VideoCreateCharacterResponse:
        Create a character from an uploaded video.
          name: Display name for this API character.
          video: Video file used to create a character.
                "video": video,
        files = extract_files(cast(Mapping[str, object], body), paths=[["video"]])
            "/videos/characters",
            body=maybe_transform(body, video_create_character_params.VideoCreateCharacterParams),
            cast_to=VideoCreateCharacterResponse,
    def download_content(
        variant: Literal["video", "thumbnail", "spritesheet"] | Omit = omit,
        Download the generated video bytes or a derived preview asset.
        Streams the rendered video content for the specified video job.
          variant: Which downloadable asset to return. Defaults to the MP4 video.
            path_template("/videos/{video_id}/content", video_id=video_id),
                query=maybe_transform({"variant": variant}, video_download_content_params.VideoDownloadContentParams),
        video: video_edit_params.Video,
        Create a new video generation job by editing a source video or existing
        generated video.
          prompt: Text prompt that describes how to edit the source video.
          video: Reference to the completed video to edit.
            "/videos/edits",
            body=maybe_transform(body, video_edit_params.VideoEditParams),
    def extend(
        seconds: VideoSeconds,
        video: video_extend_params.Video,
        Create an extension of a completed video.
          prompt: Updated text prompt that directs the extension generation.
          seconds: Length of the newly generated extension segment in seconds (allowed values: 4,
              8, 12, 16, 20).
          video: Reference to the completed video to extend.
            "/videos/extensions",
            body=maybe_transform(body, video_extend_params.VideoExtendParams),
    def get_character(
        character_id: str,
    ) -> VideoGetCharacterResponse:
        Fetch a character.
        if not character_id:
            raise ValueError(f"Expected a non-empty value for `character_id` but received {character_id!r}")
            path_template("/videos/characters/{character_id}", character_id=character_id),
            cast_to=VideoGetCharacterResponse,
    def remix(
        Create a remix of a completed video using a refreshed prompt.
          prompt: Updated text prompt that directs the remix generation.
            path_template("/videos/{video_id}/remix", video_id=video_id),
            body=maybe_transform({"prompt": prompt}, video_remix_params.VideoRemixParams),
class AsyncVideos(AsyncAPIResource):
    def with_raw_response(self) -> AsyncVideosWithRawResponse:
        return AsyncVideosWithRawResponse(self)
    def with_streaming_response(self) -> AsyncVideosWithStreamingResponse:
        return AsyncVideosWithStreamingResponse(self)
            body=await async_maybe_transform(body, video_create_params.VideoCreateParams),
    async def create_and_poll(
        video = await self.create(
        return await self.poll(
    async def poll(
            response = await self.with_raw_response.retrieve(
                await self._sleep(poll_interval_ms / 1000)
    ) -> AsyncPaginator[Video, AsyncConversationCursorPage[Video]]:
            page=AsyncConversationCursorPage[Video],
    async def create_character(
            body=await async_maybe_transform(body, video_create_character_params.VideoCreateCharacterParams),
    async def download_content(
                query=await async_maybe_transform(
                    {"variant": variant}, video_download_content_params.VideoDownloadContentParams
            body=await async_maybe_transform(body, video_edit_params.VideoEditParams),
    async def extend(
            body=await async_maybe_transform(body, video_extend_params.VideoExtendParams),
    async def get_character(
    async def remix(
            body=await async_maybe_transform({"prompt": prompt}, video_remix_params.VideoRemixParams),
class VideosWithRawResponse:
    def __init__(self, videos: Videos) -> None:
        self._videos = videos
            videos.create,
            videos.retrieve,
            videos.list,
            videos.delete,
        self.create_character = _legacy_response.to_raw_response_wrapper(
            videos.create_character,
        self.download_content = _legacy_response.to_raw_response_wrapper(
            videos.download_content,
            videos.edit,
        self.extend = _legacy_response.to_raw_response_wrapper(
            videos.extend,
        self.get_character = _legacy_response.to_raw_response_wrapper(
            videos.get_character,
        self.remix = _legacy_response.to_raw_response_wrapper(
            videos.remix,
class AsyncVideosWithRawResponse:
    def __init__(self, videos: AsyncVideos) -> None:
        self.create_character = _legacy_response.async_to_raw_response_wrapper(
        self.download_content = _legacy_response.async_to_raw_response_wrapper(
        self.extend = _legacy_response.async_to_raw_response_wrapper(
        self.get_character = _legacy_response.async_to_raw_response_wrapper(
        self.remix = _legacy_response.async_to_raw_response_wrapper(
class VideosWithStreamingResponse:
        self.create_character = to_streamed_response_wrapper(
        self.download_content = to_custom_streamed_response_wrapper(
        self.extend = to_streamed_response_wrapper(
        self.get_character = to_streamed_response_wrapper(
        self.remix = to_streamed_response_wrapper(
class AsyncVideosWithStreamingResponse:
        self.create_character = async_to_streamed_response_wrapper(
        self.download_content = async_to_custom_streamed_response_wrapper(
        self.extend = async_to_streamed_response_wrapper(
        self.get_character = async_to_streamed_response_wrapper(
        self.remix = async_to_streamed_response_wrapper(
        input_reference: FileTypes | Omit = omit,
        Create a video
          input_reference: Optional image reference that guides generation.
        Retrieve a video
            f"/videos/{video_id}",
        List videos
        Delete a video
        """Download video content
          variant: Which downloadable asset to return.
        Defaults to the MP4 video.
            f"/videos/{video_id}/content",
        Create a video remix
            f"/videos/{video_id}/remix",
