__all__ = ["WebSearchPreviewTool", "UserLocation"]
class UserLocation(BaseModel):
    """The user's location."""
    type: Literal["approximate"]
    city: Optional[str] = None
    country: Optional[str] = None
    region: Optional[str] = None
    timezone: Optional[str] = None
class WebSearchPreviewTool(BaseModel):
    """This tool searches the web for relevant results to use in a response.
    Learn more about the [web search tool](https://platform.openai.com/docs/guides/tools-web-search).
    type: Literal["web_search_preview", "web_search_preview_2025_03_11"]
    """The type of the web search tool.
    One of `web_search_preview` or `web_search_preview_2025_03_11`.
    search_content_types: Optional[List[Literal["text", "image"]]] = None
    search_context_size: Optional[Literal["low", "medium", "high"]] = None
    """High level guidance for the amount of context window space to use for the
    search.
    One of `low`, `medium`, or `high`. `medium` is the default.
    user_location: Optional[UserLocation] = None
