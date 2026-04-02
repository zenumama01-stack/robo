__all__ = ["ResponseInputImageContent"]
class ResponseInputImageContent(BaseModel):
    Learn about [image inputs](https://platform.openai.com/docs/guides/vision)
    detail: Optional[Literal["low", "high", "auto", "original"]] = None
    detail: Optional[Literal["low", "high", "auto"]] = None
