__all__ = ["ImageURL"]
class ImageURL(BaseModel):
    The external URL of the image, must be a supported image types: jpeg, jpg, png,
    gif, webp.
    """Specifies the detail level of the image.
    `low` uses fewer tokens, you can opt in to high resolution using `high`. Default
    value is `auto`
