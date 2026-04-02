__all__ = ["ImageFile"]
class ImageFile(BaseModel):
    The [File](https://platform.openai.com/docs/api-reference/files) ID of the image
    in the message content. Set `purpose="vision"` when uploading the File if you
    need to later display the file content.
    detail: Optional[Literal["auto", "low", "high"]] = None
    """Specifies the detail level of the image if specified by the user.
    `low` uses fewer tokens, you can opt in to high resolution using `high`.
