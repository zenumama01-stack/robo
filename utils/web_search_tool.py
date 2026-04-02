__all__ = ["WebSearchTool", "Filters", "UserLocation"]
class Filters(BaseModel):
    """Filters for the search."""
    """Allowed domains for the search.
    If not provided, all domains are allowed. Subdomains of the provided domains are
    allowed as well.
    Example: `["pubmed.ncbi.nlm.nih.gov"]`
    """The approximate location of the user."""
    type: Optional[Literal["approximate"]] = None
class WebSearchTool(BaseModel):
    """Search the Internet for sources related to the prompt.
    Learn more about the
    [web search tool](https://platform.openai.com/docs/guides/tools-web-search).
    type: Literal["web_search", "web_search_2025_08_26"]
    One of `web_search` or `web_search_2025_08_26`.
