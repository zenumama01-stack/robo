# generate this file using `./generate_file.sh`
file = Path("/tmp/big_test_file.txt")
def from_disk() -> None:
    print("uploading file from disk")
    upload = client.uploads.upload_file_chunked(
        file=file,
        mime_type="txt",
        purpose="batch",
    rich.print(upload)
def from_in_memory() -> None:
    print("uploading file from memory")
    # read the data into memory ourselves to simulate
    # it coming from somewhere else
    data = file.read_bytes()
    filename = "my_file.txt"
        file=data,
        filename=filename,
        bytes=len(data),
if "memory" in sys.argv:
    from_in_memory()
    from_disk()
import builtins
from ...types import FilePurpose, upload_create_params, upload_complete_params
from ..._types import Body, Omit, Query, Headers, NotGiven, SequenceNotStr, omit, not_given
from ...types.upload import Upload
from ...types.file_purpose import FilePurpose
__all__ = ["Uploads", "AsyncUploads"]
# 64MB
DEFAULT_PART_SIZE = 64 * 1024 * 1024
class Uploads(SyncAPIResource):
    def parts(self) -> Parts:
        return Parts(self._client)
    def with_raw_response(self) -> UploadsWithRawResponse:
        return UploadsWithRawResponse(self)
    def with_streaming_response(self) -> UploadsWithStreamingResponse:
        return UploadsWithStreamingResponse(self)
    def upload_file_chunked(
        file: os.PathLike[str],
        mime_type: str,
        bytes: int | None = None,
        part_size: int | None = None,
        md5: str | Omit = omit,
    ) -> Upload:
        """Splits a file into multiple 64MB parts and uploads them sequentially."""
        file: bytes,
        bytes: int,
        """Splits an in-memory file into multiple 64MB parts and uploads them sequentially."""
        file: os.PathLike[str] | bytes,
        filename: str | None = None,
        """Splits the given file into multiple parts and uploads them sequentially.
        client.uploads.upload_file(
            file=Path("my-paper.pdf"),
            mime_type="pdf",
            purpose="assistants",
        if isinstance(file, builtins.bytes):
                raise TypeError("The `filename` argument must be given for in-memory files")
            if bytes is None:
                raise TypeError("The `bytes` argument must be given for in-memory files")
            if not isinstance(file, Path):
                file = Path(file)
            if not filename:
                filename = file.name
                bytes = file.stat().st_size
        upload = self.create(
            bytes=bytes,
            mime_type=mime_type,
            purpose=purpose,
        part_ids: list[str] = []
        if part_size is None:
            part_size = DEFAULT_PART_SIZE
            buf: io.FileIO | io.BytesIO = io.BytesIO(file)
            buf = io.FileIO(file)
                data = buf.read(part_size)
                    # EOF
                part = self.parts.create(upload_id=upload.id, data=data)
                log.info("Uploaded part %s for upload %s", part.id, upload.id)
                part_ids.append(part.id)
            buf.close()
        return self.complete(upload_id=upload.id, part_ids=part_ids, md5=md5)
        expires_after: upload_create_params.ExpiresAfter | Omit = omit,
        Creates an intermediate
        [Upload](https://platform.openai.com/docs/api-reference/uploads/object) object
        that you can add
        [Parts](https://platform.openai.com/docs/api-reference/uploads/part-object) to.
        Currently, an Upload can accept at most 8 GB in total and expires after an hour
        after you create it.
        Once you complete the Upload, we will create a
        [File](https://platform.openai.com/docs/api-reference/files/object) object that
        contains all the parts you uploaded. This File is usable in the rest of our
        platform as a regular File object.
        For certain `purpose` values, the correct `mime_type` must be specified. Please
        refer to documentation for the
        [supported MIME types for your use case](https://platform.openai.com/docs/assistants/tools/file-search#supported-files).
        For guidance on the proper filename extensions for each purpose, please follow
        the documentation on
        [creating a File](https://platform.openai.com/docs/api-reference/files/create).
        Returns the Upload object with status `pending`.
          bytes: The number of bytes in the file you are uploading.
          filename: The name of the file to upload.
          mime_type: The MIME type of the file.
              This must fall within the supported MIME types for your file purpose. See the
              supported MIME types for assistants and vision.
          purpose: The intended purpose of the uploaded file.
              [documentation on File purposes](https://platform.openai.com/docs/api-reference/files/create#files-create-purpose).
            "/uploads",
                    "bytes": bytes,
                    "filename": filename,
                    "mime_type": mime_type,
                upload_create_params.UploadCreateParams,
            cast_to=Upload,
        """Cancels the Upload.
        No Parts may be added after an Upload is cancelled.
        Returns the Upload object with status `cancelled`.
            path_template("/uploads/{upload_id}/cancel", upload_id=upload_id),
    def complete(
        part_ids: SequenceNotStr[str],
        Completes the
        [Upload](https://platform.openai.com/docs/api-reference/uploads/object).
        Within the returned Upload object, there is a nested
        is ready to use in the rest of the platform.
        You can specify the order of the Parts by passing in an ordered list of the Part
        The number of bytes uploaded upon completion must match the number of bytes
        initially specified when creating the Upload object. No Parts may be added after
        an Upload is completed. Returns the Upload object with status `completed`,
        including an additional `file` property containing the created usable File
          part_ids: The ordered list of Part IDs.
          md5: The optional md5 checksum for the file contents to verify if the bytes uploaded
              matches what you expect.
            path_template("/uploads/{upload_id}/complete", upload_id=upload_id),
                    "part_ids": part_ids,
                    "md5": md5,
                upload_complete_params.UploadCompleteParams,
class AsyncUploads(AsyncAPIResource):
    def parts(self) -> AsyncParts:
        return AsyncParts(self._client)
    def with_raw_response(self) -> AsyncUploadsWithRawResponse:
        return AsyncUploadsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncUploadsWithStreamingResponse:
        return AsyncUploadsWithStreamingResponse(self)
    async def upload_file_chunked(
            if not isinstance(file, anyio.Path):
                file = anyio.Path(file)
                stat = await file.stat()
                bytes = stat.st_size
        upload = await self.create(
        if isinstance(file, anyio.Path):
            fd = await file.open("rb")
            async with fd:
                    data = await fd.read(part_size)
                    part = await self.parts.create(upload_id=upload.id, data=data)
            buf = io.BytesIO(file)
        return await self.complete(upload_id=upload.id, part_ids=part_ids, md5=md5)
    async def complete(
class UploadsWithRawResponse:
    def __init__(self, uploads: Uploads) -> None:
        self._uploads = uploads
            uploads.create,
            uploads.cancel,
        self.complete = _legacy_response.to_raw_response_wrapper(
            uploads.complete,
    def parts(self) -> PartsWithRawResponse:
        return PartsWithRawResponse(self._uploads.parts)
class AsyncUploadsWithRawResponse:
    def __init__(self, uploads: AsyncUploads) -> None:
        self.complete = _legacy_response.async_to_raw_response_wrapper(
    def parts(self) -> AsyncPartsWithRawResponse:
        return AsyncPartsWithRawResponse(self._uploads.parts)
class UploadsWithStreamingResponse:
        self.complete = to_streamed_response_wrapper(
    def parts(self) -> PartsWithStreamingResponse:
        return PartsWithStreamingResponse(self._uploads.parts)
class AsyncUploadsWithStreamingResponse:
        self.complete = async_to_streamed_response_wrapper(
    def parts(self) -> AsyncPartsWithStreamingResponse:
        return AsyncPartsWithStreamingResponse(self._uploads.parts)
            f"/uploads/{upload_id}/cancel",
        an Upload is completed.
            f"/uploads/{upload_id}/complete",
