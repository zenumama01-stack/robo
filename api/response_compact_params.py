__all__ = ["ResponseCompactParams"]
class ResponseCompactParams(TypedDict, total=False):
    model: Required[
    """Model ID used to generate the response, like `gpt-5` or `o3`.
    prompt_cache_key: Optional[str]
    """A key to use when reading from or writing to the prompt cache."""
