from typing import Union, Mapping, Optional, cast
from ..types import image_edit_params, image_generate_params, image_create_variation_params
from .._types import Body, Omit, Query, Headers, NotGiven, FileTypes, SequenceNotStr, omit, not_given
from .._utils import extract_files, required_args, maybe_transform, deepcopy_minimal, async_maybe_transform
from ..types.image_model import ImageModel
from ..types.images_response import ImagesResponse
from ..types.image_gen_stream_event import ImageGenStreamEvent
from ..types.image_edit_stream_event import ImageEditStreamEvent
__all__ = ["Images", "AsyncImages"]
class Images(SyncAPIResource):
    def with_raw_response(self) -> ImagesWithRawResponse:
        return ImagesWithRawResponse(self)
    def with_streaming_response(self) -> ImagesWithStreamingResponse:
        return ImagesWithStreamingResponse(self)
    def create_variation(
        image: FileTypes,
        model: Union[str, ImageModel, None] | Omit = omit,
        response_format: Optional[Literal["url", "b64_json"]] | Omit = omit,
        size: Optional[Literal["256x256", "512x512", "1024x1024"]] | Omit = omit,
    ) -> ImagesResponse:
        """Creates a variation of a given image.
        This endpoint only supports `dall-e-2`.
          image: The image to use as the basis for the variation(s). Must be a valid PNG file,
              less than 4MB, and square.
          model: The model to use for image generation. Only `dall-e-2` is supported at this
              time.
          n: The number of images to generate. Must be between 1 and 10.
          response_format: The format in which the generated images are returned. Must be one of `url` or
              `b64_json`. URLs are only valid for 60 minutes after the image has been
              generated.
          size: The size of the generated images. Must be one of `256x256`, `512x512`, or
              `1024x1024`.
                "image": image,
                "response_format": response_format,
                "size": size,
        files = extract_files(cast(Mapping[str, object], body), paths=[["image"]])
            "/images/variations",
            body=maybe_transform(body, image_create_variation_params.ImageCreateVariationParams),
            cast_to=ImagesResponse,
    def edit(
        image: Union[FileTypes, SequenceNotStr[FileTypes]],
        prompt: str,
        background: Optional[Literal["transparent", "opaque", "auto"]] | Omit = omit,
        input_fidelity: Optional[Literal["high", "low"]] | Omit = omit,
        mask: FileTypes | Omit = omit,
        output_compression: Optional[int] | Omit = omit,
        output_format: Optional[Literal["png", "jpeg", "webp"]] | Omit = omit,
        partial_images: Optional[int] | Omit = omit,
        quality: Optional[Literal["standard", "low", "medium", "high", "auto"]] | Omit = omit,
        size: Optional[Literal["256x256", "512x512", "1024x1024", "1536x1024", "1024x1536", "auto"]] | Omit = omit,
        """Creates an edited or extended image given one or more source images and a
        prompt.
        This endpoint supports GPT Image models (`gpt-image-1.5`, `gpt-image-1`,
        `gpt-image-1-mini`, and `chatgpt-image-latest`) and `dall-e-2`.
          image: The image(s) to edit. Must be a supported image file or an array of images.
              For the GPT image models (`gpt-image-1`, `gpt-image-1-mini`, and
              `gpt-image-1.5`), each image should be a `png`, `webp`, or `jpg` file less than
              50MB. You can provide up to 16 images. `chatgpt-image-latest` follows the same
              input constraints as GPT image models.
              For `dall-e-2`, you can only provide one image, and it should be a square `png`
              file less than 4MB.
          prompt: A text description of the desired image(s). The maximum length is 1000
              characters for `dall-e-2`, and 32000 characters for the GPT image models.
          background: Allows to set transparency for the background of the generated image(s). This
              parameter is only supported for the GPT image models. Must be one of
              `transparent`, `opaque` or `auto` (default value). When `auto` is used, the
              model will automatically determine the best background for the image.
              If `transparent`, the output format needs to support transparency, so it should
              be set to either `png` (default value) or `webp`.
          input_fidelity: Control how much effort the model will exert to match the style and features,
              especially facial features, of input images. This parameter is only supported
              for `gpt-image-1` and `gpt-image-1.5` and later models, unsupported for
              `gpt-image-1-mini`. Supports `high` and `low`. Defaults to `low`.
          mask: An additional image whose fully transparent areas (e.g. where alpha is zero)
              indicate where `image` should be edited. If there are multiple images provided,
              the mask will be applied on the first image. Must be a valid PNG file, less than
              4MB, and have the same dimensions as `image`.
          model: The model to use for image generation. Defaults to `gpt-image-1.5`.
          output_compression: The compression level (0-100%) for the generated images. This parameter is only
              supported for the GPT image models with the `webp` or `jpeg` output formats, and
              defaults to 100.
          output_format: The format in which the generated images are returned. This parameter is only
              supported for the GPT image models. Must be one of `png`, `jpeg`, or `webp`. The
              default value is `png`.
          partial_images: The number of partial images to generate. This parameter is used for streaming
              responses that return partial images. Value must be between 0 and 3. When set to
              0, the response will be a single image sent in one streaming event.
              Note that the final image may be sent before the full number of partial images
              are generated if the full image is generated more quickly.
          quality: The quality of the image that will be generated for GPT image models. Defaults
              to `auto`.
              generated. This parameter is only supported for `dall-e-2` (default is `url` for
              `dall-e-2`), as GPT image models always return base64-encoded images.
          size: The size of the generated images. Must be one of `1024x1024`, `1536x1024`
              (landscape), `1024x1536` (portrait), or `auto` (default value) for the GPT image
              models, and one of `256x256`, `512x512`, or `1024x1024` for `dall-e-2`.
          stream: Edit the image in streaming mode. Defaults to `false`. See the
              [Image generation guide](https://platform.openai.com/docs/guides/image-generation)
              for more information.
    ) -> Stream[ImageEditStreamEvent]:
    ) -> ImagesResponse | Stream[ImageEditStreamEvent]:
    @required_args(["image", "prompt"], ["image", "prompt", "stream"])
                "background": background,
                "input_fidelity": input_fidelity,
                "mask": mask,
                "output_compression": output_compression,
                "output_format": output_format,
                "partial_images": partial_images,
                "quality": quality,
        files = extract_files(cast(Mapping[str, object], body), paths=[["image"], ["image", "<array>"], ["mask"]])
                image_edit_params.ImageEditParamsStreaming if stream else image_edit_params.ImageEditParamsNonStreaming,
            stream_cls=Stream[ImageEditStreamEvent],
    def generate(
        moderation: Optional[Literal["low", "auto"]] | Omit = omit,
        quality: Optional[Literal["standard", "hd", "low", "medium", "high", "auto"]] | Omit = omit,
        size: Optional[
            Literal["auto", "1024x1024", "1536x1024", "1024x1536", "256x256", "512x512", "1792x1024", "1024x1792"]
        | Omit = omit,
        style: Optional[Literal["vivid", "natural"]] | Omit = omit,
        Creates an image given a prompt.
        [Learn more](https://platform.openai.com/docs/guides/images).
          prompt: A text description of the desired image(s). The maximum length is 32000
              characters for the GPT image models, 1000 characters for `dall-e-2` and 4000
              characters for `dall-e-3`.
          model: The model to use for image generation. One of `dall-e-2`, `dall-e-3`, or a GPT
              image model (`gpt-image-1`, `gpt-image-1-mini`, `gpt-image-1.5`). Defaults to
              `dall-e-2` unless a parameter specific to the GPT image models is used.
          moderation: Control the content-moderation level for images generated by the GPT image
              models. Must be either `low` for less restrictive filtering or `auto` (default
              value).
          n: The number of images to generate. Must be between 1 and 10. For `dall-e-3`, only
              `n=1` is supported.
              supported for the GPT image models. Must be one of `png`, `jpeg`, or `webp`.
          quality: The quality of the image that will be generated.
              - `auto` (default value) will automatically select the best quality for the
                given model.
              - `high`, `medium` and `low` are supported for the GPT image models.
              - `hd` and `standard` are supported for `dall-e-3`.
              - `standard` is the only option for `dall-e-2`.
          response_format: The format in which generated images with `dall-e-2` and `dall-e-3` are
              returned. Must be one of `url` or `b64_json`. URLs are only valid for 60 minutes
              after the image has been generated. This parameter isn't supported for the GPT
              image models, which always return base64-encoded images.
              models, one of `256x256`, `512x512`, or `1024x1024` for `dall-e-2`, and one of
              `1024x1024`, `1792x1024`, or `1024x1792` for `dall-e-3`.
          stream: Generate the image in streaming mode. Defaults to `false`. See the
              for more information. This parameter is only supported for the GPT image models.
          style: The style of the generated images. This parameter is only supported for
              `dall-e-3`. Must be one of `vivid` or `natural`. Vivid causes the model to lean
              towards generating hyper-real and dramatic images. Natural causes the model to
              produce more natural, less hyper-real looking images.
    ) -> Stream[ImageGenStreamEvent]:
    ) -> ImagesResponse | Stream[ImageGenStreamEvent]:
    @required_args(["prompt"], ["prompt", "stream"])
                    "moderation": moderation,
                    "style": style,
                image_generate_params.ImageGenerateParamsStreaming
                else image_generate_params.ImageGenerateParamsNonStreaming,
            stream_cls=Stream[ImageGenStreamEvent],
class AsyncImages(AsyncAPIResource):
    def with_raw_response(self) -> AsyncImagesWithRawResponse:
        return AsyncImagesWithRawResponse(self)
    def with_streaming_response(self) -> AsyncImagesWithStreamingResponse:
        return AsyncImagesWithStreamingResponse(self)
    async def create_variation(
            body=await async_maybe_transform(body, image_create_variation_params.ImageCreateVariationParams),
    async def edit(
    ) -> AsyncStream[ImageEditStreamEvent]:
    ) -> ImagesResponse | AsyncStream[ImageEditStreamEvent]:
            stream_cls=AsyncStream[ImageEditStreamEvent],
    async def generate(
    ) -> AsyncStream[ImageGenStreamEvent]:
    ) -> ImagesResponse | AsyncStream[ImageGenStreamEvent]:
            stream_cls=AsyncStream[ImageGenStreamEvent],
class ImagesWithRawResponse:
    def __init__(self, images: Images) -> None:
        self._images = images
        self.create_variation = _legacy_response.to_raw_response_wrapper(
            images.create_variation,
        self.edit = _legacy_response.to_raw_response_wrapper(
            images.edit,
        self.generate = _legacy_response.to_raw_response_wrapper(
            images.generate,
class AsyncImagesWithRawResponse:
    def __init__(self, images: AsyncImages) -> None:
        self.create_variation = _legacy_response.async_to_raw_response_wrapper(
        self.edit = _legacy_response.async_to_raw_response_wrapper(
        self.generate = _legacy_response.async_to_raw_response_wrapper(
class ImagesWithStreamingResponse:
        self.create_variation = to_streamed_response_wrapper(
        self.edit = to_streamed_response_wrapper(
        self.generate = to_streamed_response_wrapper(
class AsyncImagesWithStreamingResponse:
        self.create_variation = async_to_streamed_response_wrapper(
        self.edit = async_to_streamed_response_wrapper(
        self.generate = async_to_streamed_response_wrapper(
Utility functions for handling images.
Requires Pillow as you might imagine.
import zlib
from django.core.files import File
class ImageFile(File):
    A mixin for use alongside django.core.files.base.File, which provides
    additional features for dealing with images.
        return self._get_image_dimensions()[0]
        return self._get_image_dimensions()[1]
    def _get_image_dimensions(self):
        if not hasattr(self, "_dimensions_cache"):
            close = self.closed
            self.open()
            self._dimensions_cache = get_image_dimensions(self, close=close)
        return self._dimensions_cache
def get_image_dimensions(file_or_path, close=False):
    Return the (width, height) of an image, given an open file or a path. Set
    'close' to True to close the file at the end if it is initially in an open
    from PIL import ImageFile as PillowImageFile
    p = PillowImageFile.Parser()
    if hasattr(file_or_path, "read"):
        file = file_or_path
        file_pos = file.tell()
        file.seek(0)
            file = open(file_or_path, "rb")
        close = True
        # Most of the time Pillow only needs a small chunk to parse the image
        # and get the dimensions, but with some TIFF files Pillow needs to
        # parse the whole file.
        chunk_size = 1024
            data = file.read(chunk_size)
                p.feed(data)
            except zlib.error as e:
                # ignore zlib complaining on truncated stream, just feed more
                # data to parser (ticket #19457).
                if e.args[0].startswith("Error -5"):
            except struct.error:
                # Ignore PIL failing on a too short buffer when reads return
                # less bytes than expected. Skip and feed more data to the
                # parser (ticket #24544).
            except RuntimeError:
                # e.g. "RuntimeError: could not create decoder object" for
                # WebP files. A different chunk_size may work.
            if p.image:
                return p.image.size
            chunk_size *= 2
        if close:
            file.close()
            file.seek(file_pos)
