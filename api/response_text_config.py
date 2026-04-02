from .response_format_text_config import ResponseFormatTextConfig
__all__ = ["ResponseTextConfig"]
class ResponseTextConfig(BaseModel):
    verbosity: Optional[Literal["low", "medium", "high"]] = None
