from typing import List, Iterable
from ..responses.response_includable import ResponseIncludable
__all__ = ["ItemCreateParams"]
class ItemCreateParams(TypedDict, total=False):
    items: Required[Iterable[ResponseInputItemParam]]
    """The items to add to the conversation. You may add up to 20 items at a time."""
    include: List[ResponseIncludable]
    """Additional fields to include in the response.
    See the `include` parameter for
