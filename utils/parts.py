from ..._types import Body, Query, Headers, NotGiven, FileTypes, not_given
from ...types.uploads import part_create_params
from ...types.uploads.upload_part import UploadPart
__all__ = ["Parts", "AsyncParts"]
class Parts(SyncAPIResource):
    def with_raw_response(self) -> PartsWithRawResponse:
        return PartsWithRawResponse(self)
    def with_streaming_response(self) -> PartsWithStreamingResponse:
        return PartsWithStreamingResponse(self)
        upload_id: str,
        data: FileTypes,
    ) -> UploadPart:
        Adds a
        [Part](https://platform.openai.com/docs/api-reference/uploads/part-object) to an
        [Upload](https://platform.openai.com/docs/api-reference/uploads/object) object.
        A Part represents a chunk of bytes from the file you are trying to upload.
        Each Part can be at most 64 MB, and you can add Parts until you hit the Upload
        maximum of 8 GB.
        It is possible to add multiple Parts in parallel. You can decide the intended
        order of the Parts when you
        [complete the Upload](https://platform.openai.com/docs/api-reference/uploads/complete).
          data: The chunk of bytes for this Part.
        if not upload_id:
            raise ValueError(f"Expected a non-empty value for `upload_id` but received {upload_id!r}")
        body = deepcopy_minimal({"data": data})
        files = extract_files(cast(Mapping[str, object], body), paths=[["data"]])
            path_template("/uploads/{upload_id}/parts", upload_id=upload_id),
            body=maybe_transform(body, part_create_params.PartCreateParams),
            cast_to=UploadPart,
class AsyncParts(AsyncAPIResource):
    def with_raw_response(self) -> AsyncPartsWithRawResponse:
        return AsyncPartsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncPartsWithStreamingResponse:
        return AsyncPartsWithStreamingResponse(self)
            body=await async_maybe_transform(body, part_create_params.PartCreateParams),
class PartsWithRawResponse:
    def __init__(self, parts: Parts) -> None:
        self._parts = parts
            parts.create,
class AsyncPartsWithRawResponse:
    def __init__(self, parts: AsyncParts) -> None:
class PartsWithStreamingResponse:
class AsyncPartsWithStreamingResponse:
            f"/uploads/{upload_id}/parts",
