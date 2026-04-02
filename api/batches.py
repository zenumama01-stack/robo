from ..types import batch_list_params, batch_create_params
from .._types import Body, Omit, Query, Headers, NotGiven, omit, not_given
from .._utils import path_template, maybe_transform, async_maybe_transform
from .._compat import cached_property
from .._resource import SyncAPIResource, AsyncAPIResource
from .._response import to_streamed_response_wrapper, async_to_streamed_response_wrapper
from ..pagination import SyncCursorPage, AsyncCursorPage
from ..types.batch import Batch
from .._base_client import AsyncPaginator, make_request_options
from ..types.shared_params.metadata import Metadata
__all__ = ["Batches", "AsyncBatches"]
class Batches(SyncAPIResource):
    def with_raw_response(self) -> BatchesWithRawResponse:
        This property can be used as a prefix for any HTTP method call to return
        the raw response object instead of the parsed content.
        For more information, see https://www.github.com/openai/openai-python#accessing-raw-response-data-eg-headers
        return BatchesWithRawResponse(self)
    def with_streaming_response(self) -> BatchesWithStreamingResponse:
        An alternative to `.with_raw_response` that doesn't eagerly read the response body.
        For more information, see https://www.github.com/openai/openai-python#with_streaming_response
        return BatchesWithStreamingResponse(self)
        completion_window: Literal["24h"],
        endpoint: Literal[
            "/v1/responses",
            "/v1/chat/completions",
            "/v1/embeddings",
            "/v1/completions",
            "/v1/moderations",
            "/v1/images/generations",
            "/v1/images/edits",
            "/v1/videos",
        input_file_id: str,
        metadata: Optional[Metadata] | Omit = omit,
        output_expires_after: batch_create_params.OutputExpiresAfter | Omit = omit,
        # Use the following arguments if you need to pass additional parameters to the API that aren't available via kwargs.
        # The extra values given here take precedence over values defined on the client or passed to this method.
    ) -> Batch:
        Creates and executes a batch from an uploaded file of requests
          completion_window: The time frame within which the batch should be processed. Currently only `24h`
              is supported.
          endpoint: The endpoint to be used for all requests in the batch. Currently
              `/v1/responses`, `/v1/chat/completions`, `/v1/embeddings`, `/v1/completions`,
              `/v1/moderations`, `/v1/images/generations`, `/v1/images/edits`, and
              `/v1/videos` are supported. Note that `/v1/embeddings` batches are also
              restricted to a maximum of 50,000 embedding inputs across all requests in the
              batch.
          input_file_id: The ID of an uploaded file that contains requests for the new batch.
              See [upload file](https://platform.openai.com/docs/api-reference/files/create)
              for how to upload a file.
              Your input file must be formatted as a
              [JSONL file](https://platform.openai.com/docs/api-reference/batch/request-input),
              and must be uploaded with the purpose `batch`. The file can contain up to 50,000
              requests, and can be up to 200 MB in size.
          metadata: Set of 16 key-value pairs that can be attached to an object. This can be useful
              for storing additional information about the object in a structured format, and
              querying for objects via API or the dashboard.
              Keys are strings with a maximum length of 64 characters. Values are strings with
              a maximum length of 512 characters.
          output_expires_after: The expiration policy for the output and/or error file that are generated for a
          extra_headers: Send extra headers
          extra_query: Add additional query parameters to the request
          extra_body: Add additional JSON properties to the request
          timeout: Override the client-level default timeout for this request, in seconds
            "/batches",
            body=maybe_transform(
                    "completion_window": completion_window,
                    "endpoint": endpoint,
                    "input_file_id": input_file_id,
                    "metadata": metadata,
                    "output_expires_after": output_expires_after,
                batch_create_params.BatchCreateParams,
            cast_to=Batch,
    def retrieve(
        batch_id: str,
        Retrieves a batch.
        if not batch_id:
            raise ValueError(f"Expected a non-empty value for `batch_id` but received {batch_id!r}")
        return self._get(
            path_template("/batches/{batch_id}", batch_id=batch_id),
    def list(
        after: str | Omit = omit,
        limit: int | Omit = omit,
    ) -> SyncCursorPage[Batch]:
        """List your organization's batches.
          after: A cursor for use in pagination.
        `after` is an object ID that defines your place
              in the list. For instance, if you make a list request and receive 100 objects,
              ending with obj_foo, your subsequent call can include after=obj_foo in order to
              fetch the next page of the list.
          limit: A limit on the number of objects to be returned. Limit can range between 1 and
              100, and the default is 20.
        return self._get_api_list(
            page=SyncCursorPage[Batch],
                extra_headers=extra_headers,
                extra_query=extra_query,
                extra_body=extra_body,
                query=maybe_transform(
                        "after": after,
                        "limit": limit,
                    batch_list_params.BatchListParams,
            model=Batch,
    def cancel(
        """Cancels an in-progress batch.
        The batch will be in status `cancelling` for up to
        10 minutes, before changing to `cancelled`, where it will have partial results
        (if any) available in the output file.
            path_template("/batches/{batch_id}/cancel", batch_id=batch_id),
class AsyncBatches(AsyncAPIResource):
    def with_raw_response(self) -> AsyncBatchesWithRawResponse:
        return AsyncBatchesWithRawResponse(self)
    def with_streaming_response(self) -> AsyncBatchesWithStreamingResponse:
        return AsyncBatchesWithStreamingResponse(self)
            body=await async_maybe_transform(
    async def retrieve(
        return await self._get(
    ) -> AsyncPaginator[Batch, AsyncCursorPage[Batch]]:
            page=AsyncCursorPage[Batch],
    async def cancel(
class BatchesWithRawResponse:
    def __init__(self, batches: Batches) -> None:
        self._batches = batches
        self.create = _legacy_response.to_raw_response_wrapper(
            batches.create,
        self.retrieve = _legacy_response.to_raw_response_wrapper(
            batches.retrieve,
        self.list = _legacy_response.to_raw_response_wrapper(
            batches.list,
        self.cancel = _legacy_response.to_raw_response_wrapper(
            batches.cancel,
class AsyncBatchesWithRawResponse:
    def __init__(self, batches: AsyncBatches) -> None:
        self.create = _legacy_response.async_to_raw_response_wrapper(
        self.retrieve = _legacy_response.async_to_raw_response_wrapper(
        self.list = _legacy_response.async_to_raw_response_wrapper(
        self.cancel = _legacy_response.async_to_raw_response_wrapper(
class BatchesWithStreamingResponse:
        self.create = to_streamed_response_wrapper(
        self.retrieve = to_streamed_response_wrapper(
        self.list = to_streamed_response_wrapper(
        self.cancel = to_streamed_response_wrapper(
class AsyncBatchesWithStreamingResponse:
        self.create = async_to_streamed_response_wrapper(
        self.retrieve = async_to_streamed_response_wrapper(
        self.list = async_to_streamed_response_wrapper(
        self.cancel = async_to_streamed_response_wrapper(
              `/v1/moderations`, `/v1/images/generations`, and `/v1/images/edits` are
              supported. Note that `/v1/embeddings` batches are also restricted to a maximum
              of 50,000 embedding inputs across all requests in the batch.
            f"/batches/{batch_id}",
            f"/batches/{batch_id}/cancel",
