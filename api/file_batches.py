from typing import Dict, Iterable, Optional
from typing_extensions import Union, Literal
from concurrent.futures import Future, ThreadPoolExecutor, as_completed
from ...types import FileChunkingStrategyParam
from ..._types import Body, Omit, Query, Headers, NotGiven, FileTypes, SequenceNotStr, omit, not_given
from ..._utils import is_given, path_template, maybe_transform, async_maybe_transform
from ...types.file_object import FileObject
from ...types.vector_stores import file_batch_create_params, file_batch_list_files_params
from ...types.file_chunking_strategy_param import FileChunkingStrategyParam
from ...types.vector_stores.vector_store_file import VectorStoreFile
from ...types.vector_stores.vector_store_file_batch import VectorStoreFileBatch
__all__ = ["FileBatches", "AsyncFileBatches"]
class FileBatches(SyncAPIResource):
    def with_raw_response(self) -> FileBatchesWithRawResponse:
        return FileBatchesWithRawResponse(self)
    def with_streaming_response(self) -> FileBatchesWithStreamingResponse:
        return FileBatchesWithStreamingResponse(self)
        vector_store_id: str,
        attributes: Optional[Dict[str, Union[str, float, bool]]] | Omit = omit,
        chunking_strategy: FileChunkingStrategyParam | Omit = omit,
        files: Iterable[file_batch_create_params.File] | Omit = omit,
    ) -> VectorStoreFileBatch:
        Create a vector store file batch.
          attributes: Set of 16 key-value pairs that can be attached to an object. This can be useful
              querying for objects via API or the dashboard. Keys are strings with a maximum
              length of 64 characters. Values are strings with a maximum length of 512
              characters, booleans, or numbers.
          chunking_strategy: The chunking strategy used to chunk the file(s). If not set, will use the `auto`
              strategy. Only applicable if `file_ids` is non-empty.
          file_ids: A list of [File](https://platform.openai.com/docs/api-reference/files) IDs that
              the vector store should use. Useful for tools like `file_search` that can access
              files. If `attributes` or `chunking_strategy` are provided, they will be applied
              to all files in the batch. The maximum batch size is 2000 files. Mutually
              exclusive with `files`.
          files: A list of objects that each include a `file_id` plus optional `attributes` or
              `chunking_strategy`. Use this when you need to override metadata for specific
              files. The global `attributes` or `chunking_strategy` will be ignored and must
              be specified for each file. The maximum batch size is 2000 files. Mutually
              exclusive with `file_ids`.
        if not vector_store_id:
            raise ValueError(f"Expected a non-empty value for `vector_store_id` but received {vector_store_id!r}")
            path_template("/vector_stores/{vector_store_id}/file_batches", vector_store_id=vector_store_id),
                    "attributes": attributes,
                file_batch_create_params.FileBatchCreateParams,
            cast_to=VectorStoreFileBatch,
        Retrieves a vector store file batch.
                "/vector_stores/{vector_store_id}/file_batches/{batch_id}",
                vector_store_id=vector_store_id,
                batch_id=batch_id,
        """Cancel a vector store file batch.
        This attempts to cancel the processing of
        files in this batch as soon as possible.
                "/vector_stores/{vector_store_id}/file_batches/{batch_id}/cancel",
        """Create a vector store batch and poll until all files have been processed."""
        batch = self.create(
            attributes=attributes,
            chunking_strategy=chunking_strategy,
            file_ids=file_ids,
        # TODO: don't poll unless necessary??
            batch.id,
    def list_files(
        filter: Literal["in_progress", "completed", "failed", "cancelled"] | Omit = omit,
    ) -> SyncCursorPage[VectorStoreFile]:
        Returns a list of vector store files in a batch.
          filter: Filter by file status. One of `in_progress`, `completed`, `failed`, `cancelled`.
                "/vector_stores/{vector_store_id}/file_batches/{batch_id}/files",
            page=SyncCursorPage[VectorStoreFile],
                        "filter": filter,
                    file_batch_list_files_params.FileBatchListFilesParams,
            model=VectorStoreFile,
        """Wait for the given file batch to be processed.
        Note: this will return even if one of the files failed to process, you need to
        check batch.file_counts.failed_count to handle this case.
                batch_id,
            batch = response.parse()
            if batch.file_counts.in_progress > 0:
            return batch
    def upload_and_poll(
        files: Iterable[FileTypes],
        max_concurrency: int = 5,
        file_ids: SequenceNotStr[str] = [],
        """Uploads the given files concurrently and then creates a vector store file batch.
        If you've already uploaded certain files that you want to include in this batch
        then you can pass their IDs through the `file_ids` argument.
        By default, if any file upload fails then an exception will be eagerly raised.
        The number of concurrency uploads is configurable using the `max_concurrency`
        Note: this method only supports `asyncio` or `trio` as the backing async
        runtime.
        results: list[FileObject] = []
        with ThreadPoolExecutor(max_workers=max_concurrency) as executor:
            futures: list[Future[FileObject]] = [
                executor.submit(
                    self._client.files.create,
                for file in files
        for future in as_completed(futures):
            exc = future.exception()
            if exc:
                raise exc
            results.append(future.result())
        batch = self.create_and_poll(
            file_ids=[*file_ids, *(f.id for f in results)],
class AsyncFileBatches(AsyncAPIResource):
    def with_raw_response(self) -> AsyncFileBatchesWithRawResponse:
        return AsyncFileBatchesWithRawResponse(self)
    def with_streaming_response(self) -> AsyncFileBatchesWithStreamingResponse:
        return AsyncFileBatchesWithStreamingResponse(self)
        batch = await self.create(
    ) -> AsyncPaginator[VectorStoreFile, AsyncCursorPage[VectorStoreFile]]:
            page=AsyncCursorPage[VectorStoreFile],
    async def upload_and_poll(
        uploaded_files: list[FileObject] = []
        async_library = sniffio.current_async_library()
        if async_library == "asyncio":
            async def asyncio_upload_file(semaphore: asyncio.Semaphore, file: FileTypes) -> None:
                async with semaphore:
                    file_obj = await self._client.files.create(
                    uploaded_files.append(file_obj)
            semaphore = asyncio.Semaphore(max_concurrency)
            tasks = [asyncio_upload_file(semaphore, file) for file in files]
            await asyncio.gather(*tasks)
        elif async_library == "trio":
            # We only import if the library is being used.
            # We support Python 3.7 so are using an older version of trio that does not have type information
            import trio  # type: ignore # pyright: ignore[reportMissingTypeStubs]
            async def trio_upload_file(limiter: trio.CapacityLimiter, file: FileTypes) -> None:
                async with limiter:
            limiter = trio.CapacityLimiter(max_concurrency)
            async with trio.open_nursery() as nursery:
                    nursery.start_soon(trio_upload_file, limiter, file)  # pyright: ignore [reportUnknownMemberType]
                f"Async runtime {async_library} is not supported yet. Only asyncio or trio is supported",
        batch = await self.create_and_poll(
            file_ids=[*file_ids, *(f.id for f in uploaded_files)],
class FileBatchesWithRawResponse:
    def __init__(self, file_batches: FileBatches) -> None:
        self._file_batches = file_batches
            file_batches.create,
            file_batches.retrieve,
            file_batches.cancel,
        self.list_files = _legacy_response.to_raw_response_wrapper(
            file_batches.list_files,
class AsyncFileBatchesWithRawResponse:
    def __init__(self, file_batches: AsyncFileBatches) -> None:
        self.list_files = _legacy_response.async_to_raw_response_wrapper(
class FileBatchesWithStreamingResponse:
        self.list_files = to_streamed_response_wrapper(
class AsyncFileBatchesWithStreamingResponse:
        self.list_files = async_to_streamed_response_wrapper(
              to all files in the batch. Mutually exclusive with `files`.
              be specified for each file. Mutually exclusive with `file_ids`.
            f"/vector_stores/{vector_store_id}/file_batches",
            f"/vector_stores/{vector_store_id}/file_batches/{batch_id}",
            f"/vector_stores/{vector_store_id}/file_batches/{batch_id}/cancel",
            f"/vector_stores/{vector_store_id}/file_batches/{batch_id}/files",
