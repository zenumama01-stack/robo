__all__ = ["WebSearchPreviewToolParam", "UserLocation"]
class UserLocation(TypedDict, total=False):
    city: Optional[str]
    country: Optional[str]
    region: Optional[str]
    timezone: Optional[str]
class WebSearchPreviewToolParam(TypedDict, total=False):
    type: Required[Literal["web_search_preview", "web_search_preview_2025_03_11"]]
    search_content_types: List[Literal["text", "image"]]
    user_location: Optional[UserLocation]
