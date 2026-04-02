from ....types.evals.runs import output_item_list_params
from ....types.evals.runs.output_item_list_response import OutputItemListResponse
from ....types.evals.runs.output_item_retrieve_response import OutputItemRetrieveResponse
__all__ = ["OutputItems", "AsyncOutputItems"]
class OutputItems(SyncAPIResource):
    def with_raw_response(self) -> OutputItemsWithRawResponse:
        return OutputItemsWithRawResponse(self)
    def with_streaming_response(self) -> OutputItemsWithStreamingResponse:
        return OutputItemsWithStreamingResponse(self)
        output_item_id: str,
    ) -> OutputItemRetrieveResponse:
        Get an evaluation run output item by ID.
        if not output_item_id:
            raise ValueError(f"Expected a non-empty value for `output_item_id` but received {output_item_id!r}")
                "/evals/{eval_id}/runs/{run_id}/output_items/{output_item_id}",
                eval_id=eval_id,
                output_item_id=output_item_id,
            cast_to=OutputItemRetrieveResponse,
        status: Literal["fail", "pass"] | Omit = omit,
    ) -> SyncCursorPage[OutputItemListResponse]:
        Get a list of output items for an evaluation run.
          after: Identifier for the last output item from the previous pagination request.
          limit: Number of output items to retrieve.
          order: Sort order for output items by timestamp. Use `asc` for ascending order or
          status: Filter output items by status. Use `failed` to filter by failed output items or
              `pass` to filter by passed output items.
            path_template("/evals/{eval_id}/runs/{run_id}/output_items", eval_id=eval_id, run_id=run_id),
            page=SyncCursorPage[OutputItemListResponse],
                        "status": status,
                    output_item_list_params.OutputItemListParams,
            model=OutputItemListResponse,
class AsyncOutputItems(AsyncAPIResource):
    def with_raw_response(self) -> AsyncOutputItemsWithRawResponse:
        return AsyncOutputItemsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncOutputItemsWithStreamingResponse:
        return AsyncOutputItemsWithStreamingResponse(self)
    ) -> AsyncPaginator[OutputItemListResponse, AsyncCursorPage[OutputItemListResponse]]:
            page=AsyncCursorPage[OutputItemListResponse],
class OutputItemsWithRawResponse:
    def __init__(self, output_items: OutputItems) -> None:
        self._output_items = output_items
            output_items.retrieve,
            output_items.list,
class AsyncOutputItemsWithRawResponse:
    def __init__(self, output_items: AsyncOutputItems) -> None:
class OutputItemsWithStreamingResponse:
class AsyncOutputItemsWithStreamingResponse:
            f"/evals/{eval_id}/runs/{run_id}/output_items/{output_item_id}",
            f"/evals/{eval_id}/runs/{run_id}/output_items",
