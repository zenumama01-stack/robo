from .._types import FileTypes, SequenceNotStr
__all__ = ["ImageEditParamsBase", "ImageEditParamsNonStreaming", "ImageEditParamsStreaming"]
class ImageEditParamsBase(TypedDict, total=False):
    image: Required[Union[FileTypes, SequenceNotStr[FileTypes]]]
    """The image(s) to edit. Must be a supported image file or an array of images.
    prompt: Required[str]
    """A text description of the desired image(s).
    The maximum length is 1000 characters for `dall-e-2`, and 32000 characters for
    the GPT image models.
    background: Optional[Literal["transparent", "opaque", "auto"]]
    Allows to set transparency for the background of the generated image(s). This
    input_fidelity: Optional[Literal["high", "low"]]
    Control how much effort the model will exert to match the style and features,
    mask: FileTypes
    """An additional image whose fully transparent areas (e.g.
    where alpha is zero) indicate where `image` should be edited. If there are
    multiple images provided, the mask will be applied on the first image. Must be a
    valid PNG file, less than 4MB, and have the same dimensions as `image`.
    """The model to use for image generation. Defaults to `gpt-image-1.5`."""
    output_compression: Optional[int]
    """The compression level (0-100%) for the generated images.
    This parameter is only supported for the GPT image models with the `webp` or
    `jpeg` output formats, and defaults to 100.
    output_format: Optional[Literal["png", "jpeg", "webp"]]
    This parameter is only supported for the GPT image models. Must be one of `png`,
    `jpeg`, or `webp`. The default value is `png`.
    partial_images: Optional[int]
    """The number of partial images to generate.
    This parameter is used for streaming responses that return partial images. Value
    must be between 0 and 3. When set to 0, the response will be a single image sent
    in one streaming event.
    quality: Optional[Literal["standard", "low", "medium", "high", "auto"]]
    """The quality of the image that will be generated for GPT image models.
    Defaults to `auto`.
    image has been generated. This parameter is only supported for `dall-e-2`
    (default is `url` for `dall-e-2`), as GPT image models always return
    base64-encoded images.
    size: Optional[Literal["256x256", "512x512", "1024x1024", "1536x1024", "1024x1536", "auto"]]
    Must be one of `1024x1024`, `1536x1024` (landscape), `1024x1536` (portrait), or
    `auto` (default value) for the GPT image models, and one of `256x256`,
    `512x512`, or `1024x1024` for `dall-e-2`.
class ImageEditParamsNonStreaming(ImageEditParamsBase, total=False):
    """Edit the image in streaming mode.
    Defaults to `false`. See the
class ImageEditParamsStreaming(ImageEditParamsBase):
ImageEditParams = Union[ImageEditParamsNonStreaming, ImageEditParamsStreaming]
