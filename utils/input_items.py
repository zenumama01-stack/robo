from typing import Any, List, cast
from ..._utils import path_template, maybe_transform
from ...types.responses import input_item_list_params
from ...types.responses.response_item import ResponseItem
__all__ = ["InputItems", "AsyncInputItems"]
class InputItems(SyncAPIResource):
    def with_raw_response(self) -> InputItemsWithRawResponse:
        return InputItemsWithRawResponse(self)
    def with_streaming_response(self) -> InputItemsWithStreamingResponse:
        return InputItemsWithStreamingResponse(self)
        response_id: str,
    ) -> SyncCursorPage[ResponseItem]:
        Returns a list of input items for a given response.
              Response creation above for more information.
        if not response_id:
            raise ValueError(f"Expected a non-empty value for `response_id` but received {response_id!r}")
            path_template("/responses/{response_id}/input_items", response_id=response_id),
            page=SyncCursorPage[ResponseItem],
                    input_item_list_params.InputItemListParams,
            model=cast(Any, ResponseItem),  # Union types cannot be passed in as arguments in the type system
class AsyncInputItems(AsyncAPIResource):
    def with_raw_response(self) -> AsyncInputItemsWithRawResponse:
        return AsyncInputItemsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncInputItemsWithStreamingResponse:
        return AsyncInputItemsWithStreamingResponse(self)
    ) -> AsyncPaginator[ResponseItem, AsyncCursorPage[ResponseItem]]:
            page=AsyncCursorPage[ResponseItem],
class InputItemsWithRawResponse:
    def __init__(self, input_items: InputItems) -> None:
        self._input_items = input_items
            input_items.list,
class AsyncInputItemsWithRawResponse:
    def __init__(self, input_items: AsyncInputItems) -> None:
class InputItemsWithStreamingResponse:
class AsyncInputItemsWithStreamingResponse:
from ..._utils import maybe_transform
            f"/responses/{response_id}/input_items",
