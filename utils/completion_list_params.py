__all__ = ["CompletionListParams"]
class CompletionListParams(TypedDict, total=False):
    """Identifier for the last chat completion from the previous pagination request."""
    """Number of Chat Completions to retrieve."""
    """A list of metadata keys to filter the Chat Completions by. Example:
    """The model used to generate the Chat Completions."""
    """Sort order for Chat Completions by timestamp.
    Use `asc` for ascending order or `desc` for descending order. Defaults to `asc`.
