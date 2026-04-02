from typing import TYPE_CHECKING, Any, cast
    sub = subparser.add_parser("files.create")
        "-f",
        "--file",
        help="File to upload",
        "-p",
        "--purpose",
        help="Why are you uploading this file? (see https://platform.openai.com/docs/api-reference/ for purposes)",
    sub.set_defaults(func=CLIFile.create, args_model=CLIFileCreateArgs)
    sub = subparser.add_parser("files.retrieve")
    sub.add_argument("-i", "--id", required=True, help="The files ID")
    sub.set_defaults(func=CLIFile.get, args_model=CLIFileCreateArgs)
    sub = subparser.add_parser("files.delete")
    sub.set_defaults(func=CLIFile.delete, args_model=CLIFileCreateArgs)
    sub = subparser.add_parser("files.list")
    sub.set_defaults(func=CLIFile.list)
class CLIFileIDArgs(BaseModel):
    id: str
class CLIFileCreateArgs(BaseModel):
    purpose: str
class CLIFile:
    def create(args: CLIFileCreateArgs) -> None:
        file = get_client().files.create(
            purpose=cast(Any, args.purpose),
        print_model(file)
    def get(args: CLIFileIDArgs) -> None:
        file = get_client().files.retrieve(file_id=args.id)
    def delete(args: CLIFileIDArgs) -> None:
        file = get_client().files.delete(file_id=args.id)
    def list() -> None:
        files = get_client().files.list()
        for file in files:
from typing import Mapping, cast
from ..types import FilePurpose, file_list_params, file_create_params
from .._types import Body, Omit, Query, Headers, NotGiven, FileTypes, omit, not_given
from .._utils import extract_files, path_template, maybe_transform, deepcopy_minimal, async_maybe_transform
from .._response import (
    to_streamed_response_wrapper,
    async_to_streamed_response_wrapper,
    to_custom_streamed_response_wrapper,
    async_to_custom_streamed_response_wrapper,
from ..types.file_object import FileObject
from ..types.file_deleted import FileDeleted
from ..types.file_purpose import FilePurpose
__all__ = ["Files", "AsyncFiles"]
class Files(SyncAPIResource):
    def with_raw_response(self) -> FilesWithRawResponse:
        return FilesWithRawResponse(self)
    def with_streaming_response(self) -> FilesWithStreamingResponse:
        return FilesWithStreamingResponse(self)
        file: FileTypes,
        purpose: FilePurpose,
        expires_after: file_create_params.ExpiresAfter | Omit = omit,
    ) -> FileObject:
        """Upload a file that can be used across various endpoints.
        Individual files can be
        up to 512 MB, and each project can store up to 2.5 TB of files in total. There
        is no organization-wide storage limit.
        - The Assistants API supports files up to 2 million tokens and of specific file
          types. See the
          [Assistants Tools guide](https://platform.openai.com/docs/assistants/tools)
          for details.
        - The Fine-tuning API only supports `.jsonl` files. The input also has certain
          required formats for fine-tuning
          [chat](https://platform.openai.com/docs/api-reference/fine-tuning/chat-input)
          or
          [completions](https://platform.openai.com/docs/api-reference/fine-tuning/completions-input)
          models.
        - The Batch API only supports `.jsonl` files up to 200 MB in size. The input
          also has a specific required
          [format](https://platform.openai.com/docs/api-reference/batch/request-input).
        Please [contact us](https://help.openai.com/) if you need to increase these
        storage limits.
          file: The File object (not file name) to be uploaded.
          purpose:
              The intended purpose of the uploaded file. One of:
              - `assistants`: Used in the Assistants API
              - `batch`: Used in the Batch API
              - `fine-tune`: Used for fine-tuning
              - `vision`: Images used for vision fine-tuning
              - `user_data`: Flexible file type for any purpose
              - `evals`: Used for eval data sets
          expires_after: The expiration policy for a file. By default, files with `purpose=batch` expire
              after 30 days and all other files are persisted until they are manually deleted.
        body = deepcopy_minimal(
                "file": file,
                "purpose": purpose,
                "expires_after": expires_after,
        files = extract_files(cast(Mapping[str, object], body), paths=[["file"]])
        # It should be noted that the actual Content-Type header that will be
        # sent to the server will contain a `boundary` parameter, e.g.
        extra_headers = {"Content-Type": "multipart/form-data", **(extra_headers or {})}
            "/files",
            body=maybe_transform(body, file_create_params.FileCreateParams),
            cast_to=FileObject,
        file_id: str,
        Returns information about a specific file.
        if not file_id:
            raise ValueError(f"Expected a non-empty value for `file_id` but received {file_id!r}")
            path_template("/files/{file_id}", file_id=file_id),
        order: Literal["asc", "desc"] | Omit = omit,
        purpose: str | Omit = omit,
    ) -> SyncCursorPage[FileObject]:
        """Returns a list of files.
              10,000, and the default is 10,000.
          order: Sort order by the `created_at` timestamp of the objects. `asc` for ascending
              order and `desc` for descending order.
          purpose: Only return files with the given purpose.
            page=SyncCursorPage[FileObject],
                        "order": order,
                    file_list_params.FileListParams,
            model=FileObject,
    ) -> FileDeleted:
        Delete a file and remove it from all vector stores.
        return self._delete(
            cast_to=FileDeleted,
    def content(
        Returns the contents of the specified file.
        extra_headers = {"Accept": "application/binary", **(extra_headers or {})}
            path_template("/files/{file_id}/content", file_id=file_id),
    @typing_extensions.deprecated("The `.content()` method should be used instead")
    def retrieve_content(
            cast_to=str,
    def wait_for_processing(
        id: str,
        poll_interval: float = 5.0,
        max_wait_seconds: float = 30 * 60,
        """Waits for the given file to be processed, default timeout is 30 mins."""
        TERMINAL_STATES = {"processed", "error", "deleted"}
        start = time.time()
        file = self.retrieve(id)
        while file.status not in TERMINAL_STATES:
            self._sleep(poll_interval)
            if time.time() - start > max_wait_seconds:
                    f"Giving up on waiting for file {id} to finish processing after {max_wait_seconds} seconds."
class AsyncFiles(AsyncAPIResource):
    def with_raw_response(self) -> AsyncFilesWithRawResponse:
        return AsyncFilesWithRawResponse(self)
    def with_streaming_response(self) -> AsyncFilesWithStreamingResponse:
        return AsyncFilesWithStreamingResponse(self)
            body=await async_maybe_transform(body, file_create_params.FileCreateParams),
    ) -> AsyncPaginator[FileObject, AsyncCursorPage[FileObject]]:
            page=AsyncCursorPage[FileObject],
        return await self._delete(
    async def content(
    async def retrieve_content(
    async def wait_for_processing(
        file = await self.retrieve(id)
            await self._sleep(poll_interval)
class FilesWithRawResponse:
    def __init__(self, files: Files) -> None:
        self._files = files
            files.create,
            files.retrieve,
            files.list,
        self.delete = _legacy_response.to_raw_response_wrapper(
            files.delete,
        self.content = _legacy_response.to_raw_response_wrapper(
            files.content,
        self.retrieve_content = (  # pyright: ignore[reportDeprecated]
            _legacy_response.to_raw_response_wrapper(
                files.retrieve_content,  # pyright: ignore[reportDeprecated],
class AsyncFilesWithRawResponse:
    def __init__(self, files: AsyncFiles) -> None:
        self.delete = _legacy_response.async_to_raw_response_wrapper(
        self.content = _legacy_response.async_to_raw_response_wrapper(
            _legacy_response.async_to_raw_response_wrapper(
class FilesWithStreamingResponse:
        self.delete = to_streamed_response_wrapper(
        self.content = to_custom_streamed_response_wrapper(
            to_streamed_response_wrapper(
class AsyncFilesWithStreamingResponse:
        self.delete = async_to_streamed_response_wrapper(
        self.content = async_to_custom_streamed_response_wrapper(
            async_to_streamed_response_wrapper(
from ...._types import Body, Omit, Query, Headers, NoneType, NotGiven, FileTypes, omit, not_given
from ...._utils import extract_files, path_template, maybe_transform, deepcopy_minimal, async_maybe_transform
from ....types.containers import file_list_params, file_create_params
from ....types.containers.file_list_response import FileListResponse
from ....types.containers.file_create_response import FileCreateResponse
from ....types.containers.file_retrieve_response import FileRetrieveResponse
    def content(self) -> Content:
        return Content(self._client)
        file: FileTypes | Omit = omit,
        file_id: str | Omit = omit,
    ) -> FileCreateResponse:
        Create a Container File
        You can send either a multipart/form-data request with the raw file content, or
        a JSON request with a file ID.
          file_id: Name of the file to create.
                "file_id": file_id,
            path_template("/containers/{container_id}/files", container_id=container_id),
            cast_to=FileCreateResponse,
    ) -> FileRetrieveResponse:
        Retrieve Container File
            path_template("/containers/{container_id}/files/{file_id}", container_id=container_id, file_id=file_id),
            cast_to=FileRetrieveResponse,
    ) -> SyncCursorPage[FileListResponse]:
        """List Container files
            page=SyncCursorPage[FileListResponse],
            model=FileListResponse,
        Delete Container File
    def content(self) -> AsyncContent:
        return AsyncContent(self._client)
    ) -> AsyncPaginator[FileListResponse, AsyncCursorPage[FileListResponse]]:
            page=AsyncCursorPage[FileListResponse],
    def content(self) -> ContentWithRawResponse:
        return ContentWithRawResponse(self._files.content)
    def content(self) -> AsyncContentWithRawResponse:
        return AsyncContentWithRawResponse(self._files.content)
    def content(self) -> ContentWithStreamingResponse:
        return ContentWithStreamingResponse(self._files.content)
    def content(self) -> AsyncContentWithStreamingResponse:
        return AsyncContentWithStreamingResponse(self._files.content)
from typing import TYPE_CHECKING, Dict, Union, Optional
from ...pagination import SyncPage, AsyncPage, SyncCursorPage, AsyncCursorPage
from ...types.vector_stores import file_list_params, file_create_params, file_update_params
from ...types.vector_stores.file_content_response import FileContentResponse
from ...types.vector_stores.vector_store_file_deleted import VectorStoreFileDeleted
    ) -> VectorStoreFile:
        Create a vector store file by attaching a
        [File](https://platform.openai.com/docs/api-reference/files) to a
        [vector store](https://platform.openai.com/docs/api-reference/vector-stores/object).
          file_id: A [File](https://platform.openai.com/docs/api-reference/files) ID that the
              vector store should use. Useful for tools like `file_search` that can access
            path_template("/vector_stores/{vector_store_id}/files", vector_store_id=vector_store_id),
                file_create_params.FileCreateParams,
            cast_to=VectorStoreFile,
        Retrieves a vector store file.
                "/vector_stores/{vector_store_id}/files/{file_id}", vector_store_id=vector_store_id, file_id=file_id
        attributes: Optional[Dict[str, Union[str, float, bool]]],
        Update attributes on a vector store file.
            body=maybe_transform({"attributes": attributes}, file_update_params.FileUpdateParams),
        Returns a list of vector store files.
    ) -> VectorStoreFileDeleted:
        """Delete a vector store file.
        This will remove the file from the vector store but
        the file itself will not be deleted. To delete the file, use the
        [delete file](https://platform.openai.com/docs/api-reference/files/delete)
        endpoint.
            cast_to=VectorStoreFileDeleted,
        """Attach a file to the given vector store and wait for it to be processed."""
        self.create(
            file_id=file_id,
            file_id,
            file = response.parse()
            if file.status == "in_progress":
            elif file.status == "cancelled" or file.status == "completed" or file.status == "failed":
                    assert_never(file.status)
    def upload(
        """Upload a file to the `files` API and then attach it to the given vector store.
        Note the file will be asynchronously processed (you can use the alternative
        polling helper method to wait for processing to complete).
        file_obj = self._client.files.create(file=file, purpose="assistants")
        return self.create(vector_store_id=vector_store_id, file_id=file_obj.id, chunking_strategy=chunking_strategy)
        """Add a file to a vector store and poll until processing is complete."""
        return self.create_and_poll(
            file_id=file_obj.id,
    ) -> SyncPage[FileContentResponse]:
        Retrieve the parsed contents of a vector store file.
                "/vector_stores/{vector_store_id}/files/{file_id}/content",
            page=SyncPage[FileContentResponse],
            model=FileContentResponse,
            body=await async_maybe_transform({"attributes": attributes}, file_update_params.FileUpdateParams),
        await self.create(
    async def upload(
        file_obj = await self._client.files.create(file=file, purpose="assistants")
        return await self.create(
            vector_store_id=vector_store_id, file_id=file_obj.id, chunking_strategy=chunking_strategy
        return await self.create_and_poll(
    ) -> AsyncPaginator[FileContentResponse, AsyncPage[FileContentResponse]]:
            page=AsyncPage[FileContentResponse],
            files.update,
        self.content = to_streamed_response_wrapper(
        self.content = async_to_streamed_response_wrapper(
This namespace contains endpoints and data types for basic file operations.
class AddTagArg(bb.Struct):
    :ivar files.AddTagArg.path: Path to the item to be tagged.
    :ivar files.AddTagArg.tag_text: The value of the tag to add. Will be
        automatically converted to lowercase letters.
        '_tag_text_value',
                 tag_text=None):
        self._tag_text_value = bb.NOT_SET
        if tag_text is not None:
            self.tag_text = tag_text
    tag_text = bb.Attribute("tag_text")
        super(AddTagArg, self)._process_custom_annotations(annotation_type, field_path, processor)
AddTagArg_validator = bv.Struct(AddTagArg)
class BaseTagError(bb.Union):
        :rtype: BaseTagError
        super(BaseTagError, self)._process_custom_annotations(annotation_type, field_path, processor)
BaseTagError_validator = bv.Union(BaseTagError)
class AddTagError(BaseTagError):
    :ivar files.AddTagError.too_many_tags: The item already has the maximum
        supported number of tags.
    too_many_tags = None
    def is_too_many_tags(self):
        Check if the union tag is ``too_many_tags``.
        return self._tag == 'too_many_tags'
        super(AddTagError, self)._process_custom_annotations(annotation_type, field_path, processor)
AddTagError_validator = bv.Union(AddTagError)
class GetMetadataArg(bb.Struct):
    :ivar files.GetMetadataArg.path: The path of a file or folder on Dropbox.
    :ivar files.GetMetadataArg.include_media_info: If true,
        ``FileMetadata.media_info`` is set for photo and video.
    :ivar files.GetMetadataArg.include_deleted: If true,
        :class:`DeletedMetadata` will be returned for deleted file or folder,
        otherwise ``LookupError.not_found`` will be returned.
    :ivar files.GetMetadataArg.include_has_explicit_shared_members: If true, the
        results will include a flag for each file indicating whether or not
        that file has any explicit members.
    :ivar files.GetMetadataArg.include_property_groups: If set to a valid list
        of template IDs, ``FileMetadata.property_groups`` is set if there exists
        property data associated with the file and each of the listed templates.
        '_include_media_info_value',
        '_include_deleted_value',
        '_include_has_explicit_shared_members_value',
        '_include_property_groups_value',
                 include_media_info=None,
                 include_deleted=None,
                 include_has_explicit_shared_members=None,
        self._include_media_info_value = bb.NOT_SET
        self._include_deleted_value = bb.NOT_SET
        self._include_has_explicit_shared_members_value = bb.NOT_SET
        self._include_property_groups_value = bb.NOT_SET
        if include_media_info is not None:
            self.include_media_info = include_media_info
        if include_deleted is not None:
            self.include_deleted = include_deleted
        if include_has_explicit_shared_members is not None:
            self.include_has_explicit_shared_members = include_has_explicit_shared_members
        if include_property_groups is not None:
            self.include_property_groups = include_property_groups
    include_media_info = bb.Attribute("include_media_info")
    include_deleted = bb.Attribute("include_deleted")
    include_has_explicit_shared_members = bb.Attribute("include_has_explicit_shared_members")
    # Instance attribute type: file_properties.TemplateFilterBase (validator is set below)
    include_property_groups = bb.Attribute("include_property_groups", nullable=True, user_defined=True)
        super(GetMetadataArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetMetadataArg_validator = bv.Struct(GetMetadataArg)
class AlphaGetMetadataArg(GetMetadataArg):
    :ivar files.AlphaGetMetadataArg.include_property_templates: If set to a
        valid list of template IDs, ``FileMetadata.property_groups`` is set for
        '_include_property_templates_value',
        super(AlphaGetMetadataArg, self).__init__(path,
        self._include_property_templates_value = bb.NOT_SET
        if include_property_templates is not None:
            self.include_property_templates = include_property_templates
    include_property_templates = bb.Attribute("include_property_templates", nullable=True)
        super(AlphaGetMetadataArg, self)._process_custom_annotations(annotation_type, field_path, processor)
AlphaGetMetadataArg_validator = bv.Struct(AlphaGetMetadataArg)
class GetMetadataError(bb.Union):
        :rtype: GetMetadataError
        super(GetMetadataError, self)._process_custom_annotations(annotation_type, field_path, processor)
GetMetadataError_validator = bv.Union(GetMetadataError)
class AlphaGetMetadataError(GetMetadataError):
    def properties_error(cls, val):
        Create an instance of this class set to the ``properties_error`` tag
        :param file_properties.LookUpPropertiesError val:
        :rtype: AlphaGetMetadataError
        return cls('properties_error', val)
    def is_properties_error(self):
        Check if the union tag is ``properties_error``.
        return self._tag == 'properties_error'
    def get_properties_error(self):
        Only call this if :meth:`is_properties_error` is true.
        :rtype: file_properties.LookUpPropertiesError
        if not self.is_properties_error():
            raise AttributeError("tag 'properties_error' not set")
        super(AlphaGetMetadataError, self)._process_custom_annotations(annotation_type, field_path, processor)
AlphaGetMetadataError_validator = bv.Union(AlphaGetMetadataError)
class CommitInfo(bb.Struct):
    :ivar files.CommitInfo.path: Path in the user's Dropbox to save the file.
    :ivar files.CommitInfo.mode: Selects what to do if the file already exists.
    :ivar files.CommitInfo.autorename: If there's a conflict, as determined by
        ``mode``, have the Dropbox server try to autorename the file to avoid
        conflict.
    :ivar files.CommitInfo.client_modified: The value to store as the
        ``client_modified`` timestamp. Dropbox automatically records the time at
        which the file was written to the Dropbox servers. It can also record an
        additional timestamp, provided by Dropbox desktop clients, mobile
        clients, and API apps of when the file was actually created or modified.
    :ivar files.CommitInfo.mute: Normally, users are made aware of any file
        modifications in their Dropbox account via notifications in the client
        software. If ``True``, this tells the clients that this modification
        shouldn't result in a user notification.
    :ivar files.CommitInfo.property_groups: List of custom properties to add to
    :ivar files.CommitInfo.strict_conflict: Be more strict about how each
        :class:`WriteMode` detects conflict. For example, always return a
        conflict error when ``mode`` = ``WriteMode.update`` and the given "rev"
        doesn't match the existing file's "rev", even if the existing file has
        been deleted. This also forces a conflict even when the target path
        refers to a file with identical contents.
        '_autorename_value',
        '_client_modified_value',
        '_mute_value',
        '_strict_conflict_value',
                 autorename=None,
                 mute=None,
                 strict_conflict=None):
        self._autorename_value = bb.NOT_SET
        self._client_modified_value = bb.NOT_SET
        self._mute_value = bb.NOT_SET
        self._strict_conflict_value = bb.NOT_SET
        if autorename is not None:
            self.autorename = autorename
        if client_modified is not None:
            self.client_modified = client_modified
        if mute is not None:
            self.mute = mute
        if strict_conflict is not None:
            self.strict_conflict = strict_conflict
    # Instance attribute type: WriteMode (validator is set below)
    autorename = bb.Attribute("autorename")
    client_modified = bb.Attribute("client_modified", nullable=True)
    mute = bb.Attribute("mute")
    # Instance attribute type: list of [file_properties.PropertyGroup] (validator is set below)
    property_groups = bb.Attribute("property_groups", nullable=True)
    strict_conflict = bb.Attribute("strict_conflict")
        super(CommitInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
CommitInfo_validator = bv.Struct(CommitInfo)
class ContentSyncSetting(bb.Struct):
    :ivar files.ContentSyncSetting.id: Id of the item this setting is applied
        to.
    :ivar files.ContentSyncSetting.sync_setting: Setting for this item.
        '_sync_setting_value',
        self._sync_setting_value = bb.NOT_SET
        if sync_setting is not None:
            self.sync_setting = sync_setting
    # Instance attribute type: SyncSetting (validator is set below)
    sync_setting = bb.Attribute("sync_setting", user_defined=True)
        super(ContentSyncSetting, self)._process_custom_annotations(annotation_type, field_path, processor)
ContentSyncSetting_validator = bv.Struct(ContentSyncSetting)
class ContentSyncSettingArg(bb.Struct):
    :ivar files.ContentSyncSettingArg.id: Id of the item this setting is applied
    :ivar files.ContentSyncSettingArg.sync_setting: Setting for this item.
    # Instance attribute type: SyncSettingArg (validator is set below)
        super(ContentSyncSettingArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ContentSyncSettingArg_validator = bv.Struct(ContentSyncSettingArg)
class CreateFolderArg(bb.Struct):
    :ivar files.CreateFolderArg.path: Path in the user's Dropbox to create.
    :ivar files.CreateFolderArg.autorename: If there's a conflict, have the
        Dropbox server try to autorename the folder to avoid the conflict.
                 autorename=None):
        super(CreateFolderArg, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateFolderArg_validator = bv.Struct(CreateFolderArg)
class CreateFolderBatchArg(bb.Struct):
    :ivar files.CreateFolderBatchArg.paths: List of paths to be created in the
        user's Dropbox. Duplicate path arguments in the batch are considered
        only once.
    :ivar files.CreateFolderBatchArg.autorename: If there's a conflict, have the
    :ivar files.CreateFolderBatchArg.force_async: Whether to force the create to
        happen asynchronously.
        '_paths_value',
        '_force_async_value',
                 paths=None,
                 force_async=None):
        self._paths_value = bb.NOT_SET
        self._force_async_value = bb.NOT_SET
        if paths is not None:
            self.paths = paths
        if force_async is not None:
            self.force_async = force_async
    paths = bb.Attribute("paths")
    force_async = bb.Attribute("force_async")
        super(CreateFolderBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateFolderBatchArg_validator = bv.Struct(CreateFolderBatchArg)
class CreateFolderBatchError(bb.Union):
    :ivar files.CreateFolderBatchError.too_many_files: The operation would
        involve too many files or folders.
    too_many_files = None
    def is_too_many_files(self):
        Check if the union tag is ``too_many_files``.
        return self._tag == 'too_many_files'
        super(CreateFolderBatchError, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateFolderBatchError_validator = bv.Union(CreateFolderBatchError)
class CreateFolderBatchJobStatus(async_.PollResultBase):
    :ivar CreateFolderBatchResult CreateFolderBatchJobStatus.complete: The batch
        create folder has finished.
    :ivar CreateFolderBatchError CreateFolderBatchJobStatus.failed: The batch
        create folder has failed.
    def complete(cls, val):
        Create an instance of this class set to the ``complete`` tag with value
        :param CreateFolderBatchResult val:
        :rtype: CreateFolderBatchJobStatus
        return cls('complete', val)
    def failed(cls, val):
        Create an instance of this class set to the ``failed`` tag with value
        :param CreateFolderBatchError val:
        return cls('failed', val)
    def is_failed(self):
        Check if the union tag is ``failed``.
        return self._tag == 'failed'
    def get_complete(self):
        The batch create folder has finished.
        Only call this if :meth:`is_complete` is true.
        :rtype: CreateFolderBatchResult
        if not self.is_complete():
            raise AttributeError("tag 'complete' not set")
    def get_failed(self):
        The batch create folder has failed.
        Only call this if :meth:`is_failed` is true.
        :rtype: CreateFolderBatchError
        if not self.is_failed():
            raise AttributeError("tag 'failed' not set")
        super(CreateFolderBatchJobStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateFolderBatchJobStatus_validator = bv.Union(CreateFolderBatchJobStatus)
class CreateFolderBatchLaunch(async_.LaunchResultBase):
    Result returned by
    :meth:`dropbox.dropbox_client.Dropbox.files_create_folder_batch` that may
    either launch an asynchronous job or complete synchronously.
        :rtype: CreateFolderBatchLaunch
        super(CreateFolderBatchLaunch, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateFolderBatchLaunch_validator = bv.Union(CreateFolderBatchLaunch)
class FileOpsResult(bb.Struct):
        super(FileOpsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
FileOpsResult_validator = bv.Struct(FileOpsResult)
class CreateFolderBatchResult(FileOpsResult):
    :ivar files.CreateFolderBatchResult.entries: Each entry in
        ``CreateFolderBatchArg.paths`` will appear at the same position inside
        ``CreateFolderBatchResult.entries``.
        '_entries_value',
                 entries=None):
        super(CreateFolderBatchResult, self).__init__()
        self._entries_value = bb.NOT_SET
        if entries is not None:
            self.entries = entries
    # Instance attribute type: list of [CreateFolderBatchResultEntry] (validator is set below)
    entries = bb.Attribute("entries")
        super(CreateFolderBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateFolderBatchResult_validator = bv.Struct(CreateFolderBatchResult)
class CreateFolderBatchResultEntry(bb.Union):
    def success(cls, val):
        Create an instance of this class set to the ``success`` tag with value
        :param CreateFolderEntryResult val:
        :rtype: CreateFolderBatchResultEntry
        return cls('success', val)
    def failure(cls, val):
        Create an instance of this class set to the ``failure`` tag with value
        :param CreateFolderEntryError val:
        return cls('failure', val)
    def is_success(self):
        Check if the union tag is ``success``.
        return self._tag == 'success'
    def is_failure(self):
        Check if the union tag is ``failure``.
        return self._tag == 'failure'
    def get_success(self):
        Only call this if :meth:`is_success` is true.
        :rtype: CreateFolderEntryResult
        if not self.is_success():
            raise AttributeError("tag 'success' not set")
    def get_failure(self):
        Only call this if :meth:`is_failure` is true.
        :rtype: CreateFolderEntryError
        if not self.is_failure():
            raise AttributeError("tag 'failure' not set")
        super(CreateFolderBatchResultEntry, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateFolderBatchResultEntry_validator = bv.Union(CreateFolderBatchResultEntry)
class CreateFolderEntryError(bb.Union):
        :param WriteError val:
        :rtype: WriteError
        super(CreateFolderEntryError, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateFolderEntryError_validator = bv.Union(CreateFolderEntryError)
class CreateFolderEntryResult(bb.Struct):
    :ivar files.CreateFolderEntryResult.metadata: Metadata of the created
        '_metadata_value',
                 metadata=None):
        self._metadata_value = bb.NOT_SET
        if metadata is not None:
            self.metadata = metadata
    # Instance attribute type: FolderMetadata (validator is set below)
    metadata = bb.Attribute("metadata", user_defined=True)
        super(CreateFolderEntryResult, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateFolderEntryResult_validator = bv.Struct(CreateFolderEntryResult)
class CreateFolderError(bb.Union):
        :rtype: CreateFolderError
        super(CreateFolderError, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateFolderError_validator = bv.Union(CreateFolderError)
class CreateFolderResult(FileOpsResult):
    :ivar files.CreateFolderResult.metadata: Metadata of the created folder.
        super(CreateFolderResult, self).__init__()
        super(CreateFolderResult, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateFolderResult_validator = bv.Struct(CreateFolderResult)
class DeleteArg(bb.Struct):
    :ivar files.DeleteArg.path: Path in the user's Dropbox to delete.
    :ivar files.DeleteArg.parent_rev: Perform delete if given "rev" matches the
        existing file's latest "rev". This field does not support deleting a
        '_parent_rev_value',
        self._parent_rev_value = bb.NOT_SET
        if parent_rev is not None:
            self.parent_rev = parent_rev
    parent_rev = bb.Attribute("parent_rev", nullable=True)
        super(DeleteArg, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteArg_validator = bv.Struct(DeleteArg)
class DeleteBatchArg(bb.Struct):
    # Instance attribute type: list of [DeleteArg] (validator is set below)
        super(DeleteBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteBatchArg_validator = bv.Struct(DeleteBatchArg)
class DeleteBatchError(bb.Union):
    :ivar files.DeleteBatchError.too_many_write_operations: Use
        ``DeleteError.too_many_write_operations``.
        :meth:`dropbox.dropbox_client.Dropbox.files_delete_batch` now provides
        smaller granularity about which entry has failed because of this.
        super(DeleteBatchError, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteBatchError_validator = bv.Union(DeleteBatchError)
class DeleteBatchJobStatus(async_.PollResultBase):
    :ivar DeleteBatchResult DeleteBatchJobStatus.complete: The batch delete has
        finished.
    :ivar DeleteBatchError DeleteBatchJobStatus.failed: The batch delete has
        failed.
        :param DeleteBatchResult val:
        :rtype: DeleteBatchJobStatus
        :param DeleteBatchError val:
        The batch delete has finished.
        :rtype: DeleteBatchResult
        The batch delete has failed.
        :rtype: DeleteBatchError
        super(DeleteBatchJobStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteBatchJobStatus_validator = bv.Union(DeleteBatchJobStatus)
class DeleteBatchLaunch(async_.LaunchResultBase):
    Result returned by :meth:`dropbox.dropbox_client.Dropbox.files_delete_batch`
    that may either launch an asynchronous job or complete synchronously.
        :rtype: DeleteBatchLaunch
        super(DeleteBatchLaunch, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteBatchLaunch_validator = bv.Union(DeleteBatchLaunch)
class DeleteBatchResult(FileOpsResult):
    :ivar files.DeleteBatchResult.entries: Each entry in
        ``DeleteBatchArg.entries`` will appear at the same position inside
        ``DeleteBatchResult.entries``.
        super(DeleteBatchResult, self).__init__()
    # Instance attribute type: list of [DeleteBatchResultEntry] (validator is set below)
        super(DeleteBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteBatchResult_validator = bv.Struct(DeleteBatchResult)
class DeleteBatchResultData(bb.Struct):
    :ivar files.DeleteBatchResultData.metadata: Metadata of the deleted object.
    # Instance attribute type: Metadata (validator is set below)
        super(DeleteBatchResultData, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteBatchResultData_validator = bv.Struct(DeleteBatchResultData)
class DeleteBatchResultEntry(bb.Union):
        :param DeleteBatchResultData val:
        :rtype: DeleteBatchResultEntry
        :param DeleteError val:
        :rtype: DeleteBatchResultData
        :rtype: DeleteError
        super(DeleteBatchResultEntry, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteBatchResultEntry_validator = bv.Union(DeleteBatchResultEntry)
class DeleteError(bb.Union):
    :ivar files.DeleteError.too_many_write_operations: There are too many write
        operations in user's Dropbox. Please retry this request.
    :ivar files.DeleteError.too_many_files: There are too many files in one
        request. Please retry with fewer files.
    def path_lookup(cls, val):
        Create an instance of this class set to the ``path_lookup`` tag with
        return cls('path_lookup', val)
    def path_write(cls, val):
        Create an instance of this class set to the ``path_write`` tag with
        return cls('path_write', val)
    def is_path_lookup(self):
        Check if the union tag is ``path_lookup``.
        return self._tag == 'path_lookup'
    def is_path_write(self):
        Check if the union tag is ``path_write``.
        return self._tag == 'path_write'
    def get_path_lookup(self):
        Only call this if :meth:`is_path_lookup` is true.
        if not self.is_path_lookup():
            raise AttributeError("tag 'path_lookup' not set")
    def get_path_write(self):
        Only call this if :meth:`is_path_write` is true.
        if not self.is_path_write():
            raise AttributeError("tag 'path_write' not set")
        super(DeleteError, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteError_validator = bv.Union(DeleteError)
class DeleteResult(FileOpsResult):
    :ivar files.DeleteResult.metadata: Metadata of the deleted object.
        super(DeleteResult, self).__init__()
        super(DeleteResult, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteResult_validator = bv.Struct(DeleteResult)
class Metadata(bb.Struct):
    Metadata for a file or folder.
    :ivar files.Metadata.name: The last component of the path (including
        extension). This never contains a slash.
    :ivar files.Metadata.path_lower: The lowercased full path in the user's
        Dropbox. This always starts with a slash. This field will be null if the
        file or folder is not mounted.
    :ivar files.Metadata.path_display: The cased path to be used for display
        purposes only. In rare instances the casing will not correctly match the
        user's filesystem, but this behavior will match the path provided in the
        Core API v1, and at least the last path component will have the correct
        casing. Changes to only the casing of paths won't be returned by
        :meth:`dropbox.dropbox_client.Dropbox.files_list_folder_continue`. This
        field will be null if the file or folder is not mounted.
    :ivar files.Metadata.parent_shared_folder_id: Please use
        ``FileSharingInfo.parent_shared_folder_id`` or
        ``FolderSharingInfo.parent_shared_folder_id`` instead.
    :ivar files.Metadata.preview_url: The preview URL of the file.
        '_path_lower_value',
        '_path_display_value',
        '_parent_shared_folder_id_value',
        '_preview_url_value',
                 path_lower=None,
                 path_display=None,
                 parent_shared_folder_id=None,
                 preview_url=None):
        self._path_lower_value = bb.NOT_SET
        self._path_display_value = bb.NOT_SET
        self._parent_shared_folder_id_value = bb.NOT_SET
        self._preview_url_value = bb.NOT_SET
        if path_lower is not None:
            self.path_lower = path_lower
        if path_display is not None:
            self.path_display = path_display
        if parent_shared_folder_id is not None:
            self.parent_shared_folder_id = parent_shared_folder_id
        if preview_url is not None:
            self.preview_url = preview_url
    path_lower = bb.Attribute("path_lower", nullable=True)
    path_display = bb.Attribute("path_display", nullable=True)
    parent_shared_folder_id = bb.Attribute("parent_shared_folder_id", nullable=True)
    preview_url = bb.Attribute("preview_url", nullable=True)
        super(Metadata, self)._process_custom_annotations(annotation_type, field_path, processor)
Metadata_validator = bv.StructTree(Metadata)
class DeletedMetadata(Metadata):
    Indicates that there used to be a file or folder at this path, but it no
    longer exists.
        super(DeletedMetadata, self).__init__(name,
                                              path_lower,
                                              path_display,
                                              parent_shared_folder_id,
                                              preview_url)
        super(DeletedMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
DeletedMetadata_validator = bv.Struct(DeletedMetadata)
class Dimensions(bb.Struct):
    Dimensions for a photo or video.
    :ivar files.Dimensions.height: Height of the photo/video.
    :ivar files.Dimensions.width: Width of the photo/video.
        '_height_value',
        '_width_value',
                 height=None,
                 width=None):
        self._height_value = bb.NOT_SET
        self._width_value = bb.NOT_SET
        if height is not None:
            self.height = height
        if width is not None:
            self.width = width
    height = bb.Attribute("height")
    width = bb.Attribute("width")
        super(Dimensions, self)._process_custom_annotations(annotation_type, field_path, processor)
Dimensions_validator = bv.Struct(Dimensions)
class DownloadArg(bb.Struct):
    :ivar files.DownloadArg.path: The path of the file to download.
    :ivar files.DownloadArg.rev: Please specify revision in ``path`` instead.
        '_rev_value',
        self._rev_value = bb.NOT_SET
        if rev is not None:
            self.rev = rev
    rev = bb.Attribute("rev", nullable=True)
        super(DownloadArg, self)._process_custom_annotations(annotation_type, field_path, processor)
DownloadArg_validator = bv.Struct(DownloadArg)
class DownloadError(bb.Union):
    :ivar files.DownloadError.unsupported_file: This file type cannot be
        downloaded directly; use
        :meth:`dropbox.dropbox_client.Dropbox.files_export` instead.
    unsupported_file = None
        :rtype: DownloadError
    def is_unsupported_file(self):
        Check if the union tag is ``unsupported_file``.
        return self._tag == 'unsupported_file'
        super(DownloadError, self)._process_custom_annotations(annotation_type, field_path, processor)
DownloadError_validator = bv.Union(DownloadError)
class DownloadZipArg(bb.Struct):
    :ivar files.DownloadZipArg.path: The path of the folder to download.
        super(DownloadZipArg, self)._process_custom_annotations(annotation_type, field_path, processor)
DownloadZipArg_validator = bv.Struct(DownloadZipArg)
class DownloadZipError(bb.Union):
    :ivar files.DownloadZipError.too_large: The folder or a file is too large to
        download.
    :ivar files.DownloadZipError.too_many_files: The folder has too many files
        to download.
    too_large = None
        :rtype: DownloadZipError
    def is_too_large(self):
        Check if the union tag is ``too_large``.
        return self._tag == 'too_large'
        super(DownloadZipError, self)._process_custom_annotations(annotation_type, field_path, processor)
DownloadZipError_validator = bv.Union(DownloadZipError)
class DownloadZipResult(bb.Struct):
        super(DownloadZipResult, self)._process_custom_annotations(annotation_type, field_path, processor)
DownloadZipResult_validator = bv.Struct(DownloadZipResult)
class ExportArg(bb.Struct):
    :ivar files.ExportArg.path: The path of the file to be exported.
    :ivar files.ExportArg.export_format: The file format to which the file
        should be exported. This must be one of the formats listed in the file's
        export_options returned by
        :meth:`dropbox.dropbox_client.Dropbox.files_get_metadata`. If none is
        specified, the default format (specified in export_as in file metadata)
        will be used.
        '_export_format_value',
        self._export_format_value = bb.NOT_SET
        if export_format is not None:
            self.export_format = export_format
    export_format = bb.Attribute("export_format", nullable=True)
        super(ExportArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ExportArg_validator = bv.Struct(ExportArg)
class ExportError(bb.Union):
    :ivar files.ExportError.non_exportable: This file type cannot be exported.
        Use :meth:`dropbox.dropbox_client.Dropbox.files_download` instead.
    :ivar files.ExportError.invalid_export_format: The specified export format
        is not a valid option for this file type.
    :ivar files.ExportError.retry_error: The exportable content is not yet
        available. Please retry later.
    non_exportable = None
    invalid_export_format = None
    retry_error = None
        :rtype: ExportError
    def is_non_exportable(self):
        Check if the union tag is ``non_exportable``.
        return self._tag == 'non_exportable'
    def is_invalid_export_format(self):
        Check if the union tag is ``invalid_export_format``.
        return self._tag == 'invalid_export_format'
    def is_retry_error(self):
        Check if the union tag is ``retry_error``.
        return self._tag == 'retry_error'
        super(ExportError, self)._process_custom_annotations(annotation_type, field_path, processor)
ExportError_validator = bv.Union(ExportError)
class ExportInfo(bb.Struct):
    Export information for a file.
    :ivar files.ExportInfo.export_as: Format to which the file can be exported
    :ivar files.ExportInfo.export_options: Additional formats to which the file
        can be exported. These values can be specified as the export_format in
        /files/export.
        '_export_as_value',
        '_export_options_value',
                 export_as=None,
                 export_options=None):
        self._export_as_value = bb.NOT_SET
        self._export_options_value = bb.NOT_SET
        if export_as is not None:
            self.export_as = export_as
        if export_options is not None:
            self.export_options = export_options
    export_as = bb.Attribute("export_as", nullable=True)
    export_options = bb.Attribute("export_options", nullable=True)
        super(ExportInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
ExportInfo_validator = bv.Struct(ExportInfo)
class ExportMetadata(bb.Struct):
    :ivar files.ExportMetadata.name: The last component of the path (including
    :ivar files.ExportMetadata.size: The file size in bytes.
    :ivar files.ExportMetadata.export_hash: A hash based on the exported file
        content. This field can be used to verify data integrity. Similar to
        content hash. For more information see our `Content hash
    :ivar files.ExportMetadata.paper_revision: If the file is a Paper doc, this
        gives the latest doc revision which can be used in
        :meth:`dropbox.dropbox_client.Dropbox.files_paper_update`.
        '_size_value',
        '_export_hash_value',
        '_paper_revision_value',
                 size=None,
                 export_hash=None,
        self._size_value = bb.NOT_SET
        self._export_hash_value = bb.NOT_SET
        self._paper_revision_value = bb.NOT_SET
        if size is not None:
        if export_hash is not None:
            self.export_hash = export_hash
        if paper_revision is not None:
            self.paper_revision = paper_revision
    size = bb.Attribute("size")
    export_hash = bb.Attribute("export_hash", nullable=True)
    paper_revision = bb.Attribute("paper_revision", nullable=True)
        super(ExportMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
ExportMetadata_validator = bv.Struct(ExportMetadata)
class ExportResult(bb.Struct):
    :ivar files.ExportResult.export_metadata: Metadata for the exported version
        of the file.
    :ivar files.ExportResult.file_metadata: Metadata for the original file.
        '_export_metadata_value',
        '_file_metadata_value',
                 export_metadata=None,
                 file_metadata=None):
        self._export_metadata_value = bb.NOT_SET
        self._file_metadata_value = bb.NOT_SET
        if export_metadata is not None:
            self.export_metadata = export_metadata
        if file_metadata is not None:
            self.file_metadata = file_metadata
    # Instance attribute type: ExportMetadata (validator is set below)
    export_metadata = bb.Attribute("export_metadata", user_defined=True)
    # Instance attribute type: FileMetadata (validator is set below)
    file_metadata = bb.Attribute("file_metadata", user_defined=True)
        super(ExportResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ExportResult_validator = bv.Struct(ExportResult)
class FileCategory(bb.Union):
    :ivar files.FileCategory.image: jpg, png, gif, and more.
    :ivar files.FileCategory.document: doc, docx, txt, and more.
    :ivar files.FileCategory.pdf: pdf.
    :ivar files.FileCategory.spreadsheet: xlsx, xls, csv, and more.
    :ivar files.FileCategory.presentation: ppt, pptx, key, and more.
    :ivar files.FileCategory.audio: mp3, wav, mid, and more.
    :ivar files.FileCategory.video: mov, wmv, mp4, and more.
    :ivar files.FileCategory.folder: dropbox folder.
    :ivar files.FileCategory.paper: dropbox paper doc.
    :ivar files.FileCategory.others: any file not in one of the categories
        above.
    image = None
    document = None
    pdf = None
    spreadsheet = None
    presentation = None
    audio = None
    video = None
    folder = None
    paper = None
    others = None
    def is_image(self):
        Check if the union tag is ``image``.
        return self._tag == 'image'
    def is_document(self):
        Check if the union tag is ``document``.
        return self._tag == 'document'
    def is_pdf(self):
        Check if the union tag is ``pdf``.
        return self._tag == 'pdf'
    def is_spreadsheet(self):
        Check if the union tag is ``spreadsheet``.
        return self._tag == 'spreadsheet'
    def is_presentation(self):
        Check if the union tag is ``presentation``.
        return self._tag == 'presentation'
    def is_audio(self):
        Check if the union tag is ``audio``.
        return self._tag == 'audio'
    def is_video(self):
        Check if the union tag is ``video``.
        return self._tag == 'video'
    def is_folder(self):
        Check if the union tag is ``folder``.
        return self._tag == 'folder'
    def is_paper(self):
        Check if the union tag is ``paper``.
        return self._tag == 'paper'
    def is_others(self):
        Check if the union tag is ``others``.
        return self._tag == 'others'
        super(FileCategory, self)._process_custom_annotations(annotation_type, field_path, processor)
FileCategory_validator = bv.Union(FileCategory)
class FileLock(bb.Struct):
    :ivar files.FileLock.content: The lock description.
        '_content_value',
                 content=None):
        self._content_value = bb.NOT_SET
        if content is not None:
    # Instance attribute type: FileLockContent (validator is set below)
    content = bb.Attribute("content", user_defined=True)
        super(FileLock, self)._process_custom_annotations(annotation_type, field_path, processor)
FileLock_validator = bv.Struct(FileLock)
class FileLockContent(bb.Union):
    :ivar files.FileLockContent.unlocked: Empty type to indicate no lock.
    :ivar SingleUserLock FileLockContent.single_user: A lock held by a single
    unlocked = None
    def single_user(cls, val):
        Create an instance of this class set to the ``single_user`` tag with
        :param SingleUserLock val:
        :rtype: FileLockContent
        return cls('single_user', val)
    def is_unlocked(self):
        Check if the union tag is ``unlocked``.
        return self._tag == 'unlocked'
    def is_single_user(self):
        Check if the union tag is ``single_user``.
        return self._tag == 'single_user'
    def get_single_user(self):
        A lock held by a single user.
        Only call this if :meth:`is_single_user` is true.
        :rtype: SingleUserLock
        if not self.is_single_user():
            raise AttributeError("tag 'single_user' not set")
        super(FileLockContent, self)._process_custom_annotations(annotation_type, field_path, processor)
FileLockContent_validator = bv.Union(FileLockContent)
class FileLockMetadata(bb.Struct):
    :ivar files.FileLockMetadata.is_lockholder: True if caller holds the file
        lock.
    :ivar files.FileLockMetadata.lockholder_name: The display name of the lock
        holder.
    :ivar files.FileLockMetadata.lockholder_account_id: The account ID of the
        lock holder if known.
    :ivar files.FileLockMetadata.created: The timestamp of the lock was created.
        '_is_lockholder_value',
        '_lockholder_name_value',
        '_lockholder_account_id_value',
                 is_lockholder=None,
                 lockholder_name=None,
                 lockholder_account_id=None,
                 created=None):
        self._is_lockholder_value = bb.NOT_SET
        self._lockholder_name_value = bb.NOT_SET
        self._lockholder_account_id_value = bb.NOT_SET
        if is_lockholder is not None:
            self.is_lockholder = is_lockholder
        if lockholder_name is not None:
            self.lockholder_name = lockholder_name
        if lockholder_account_id is not None:
            self.lockholder_account_id = lockholder_account_id
    is_lockholder = bb.Attribute("is_lockholder", nullable=True)
    lockholder_name = bb.Attribute("lockholder_name", nullable=True)
    lockholder_account_id = bb.Attribute("lockholder_account_id", nullable=True)
    created = bb.Attribute("created", nullable=True)
        super(FileLockMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
FileLockMetadata_validator = bv.Struct(FileLockMetadata)
class FileMetadata(Metadata):
    :ivar files.FileMetadata.id: A unique identifier for the file.
    :ivar files.FileMetadata.client_modified: For files, this is the
        modification time set by the desktop client when the file was added to
        Dropbox. Since this time is not verified (the Dropbox server stores
        whatever the desktop client sends up), this should only be used for
        display purposes (such as sorting) and not, for example, to determine if
        a file has changed or not.
    :ivar files.FileMetadata.server_modified: The last time the file was
        modified on Dropbox.
    :ivar files.FileMetadata.rev: A unique identifier for the current revision
        of a file. This field is the same rev as elsewhere in the API and can be
        used to detect changes and avoid conflicts.
    :ivar files.FileMetadata.size: The file size in bytes.
    :ivar files.FileMetadata.media_info: Additional information if the file is a
        photo or video. This field will not be set on entries returned by
        :meth:`dropbox.dropbox_client.Dropbox.files_list_folder`,
        :meth:`dropbox.dropbox_client.Dropbox.files_list_folder_continue`, or
        :meth:`dropbox.dropbox_client.Dropbox.files_get_thumbnail_batch`,
        starting December 2, 2019.
    :ivar files.FileMetadata.symlink_info: Set if this file is a symlink.
    :ivar files.FileMetadata.sharing_info: Set if this file is contained in a
    :ivar files.FileMetadata.is_downloadable: If true, file can be downloaded
        directly; else the file must be exported.
    :ivar files.FileMetadata.export_info: Information about format this file can
        be exported to. This filed must be set if ``is_downloadable`` is set to
        false.
    :ivar files.FileMetadata.property_groups: Additional information if the file
        has custom properties with the property template specified.
    :ivar files.FileMetadata.has_explicit_shared_members: This flag will only be
        present if include_has_explicit_shared_members  is true in
        :meth:`dropbox.dropbox_client.Dropbox.files_list_folder` or
        :meth:`dropbox.dropbox_client.Dropbox.files_get_metadata`. If this  flag
        is present, it will be true if this file has any explicit shared
        members. This is different from sharing_info in that this could be true
        in the case where a file has explicit members but is not contained
        within  a shared folder.
    :ivar files.FileMetadata.content_hash: A hash of the file content. This
        field can be used to verify data integrity. For more information see our
    :ivar files.FileMetadata.file_lock_info: If present, the metadata associated
        with the file's current lock.
        '_server_modified_value',
        '_media_info_value',
        '_symlink_info_value',
        '_sharing_info_value',
        '_is_downloadable_value',
        '_export_info_value',
        '_has_explicit_shared_members_value',
        '_content_hash_value',
        '_file_lock_info_value',
                 server_modified=None,
                 rev=None,
                 preview_url=None,
                 media_info=None,
                 symlink_info=None,
                 sharing_info=None,
                 is_downloadable=None,
                 export_info=None,
                 has_explicit_shared_members=None,
                 content_hash=None,
                 file_lock_info=None):
        super(FileMetadata, self).__init__(name,
        self._server_modified_value = bb.NOT_SET
        self._media_info_value = bb.NOT_SET
        self._symlink_info_value = bb.NOT_SET
        self._sharing_info_value = bb.NOT_SET
        self._is_downloadable_value = bb.NOT_SET
        self._export_info_value = bb.NOT_SET
        self._has_explicit_shared_members_value = bb.NOT_SET
        self._content_hash_value = bb.NOT_SET
        self._file_lock_info_value = bb.NOT_SET
        if server_modified is not None:
            self.server_modified = server_modified
        if media_info is not None:
            self.media_info = media_info
        if symlink_info is not None:
            self.symlink_info = symlink_info
        if sharing_info is not None:
            self.sharing_info = sharing_info
        if is_downloadable is not None:
            self.is_downloadable = is_downloadable
        if export_info is not None:
            self.export_info = export_info
        if has_explicit_shared_members is not None:
            self.has_explicit_shared_members = has_explicit_shared_members
        if content_hash is not None:
            self.content_hash = content_hash
        if file_lock_info is not None:
            self.file_lock_info = file_lock_info
    client_modified = bb.Attribute("client_modified")
    server_modified = bb.Attribute("server_modified")
    rev = bb.Attribute("rev")
    # Instance attribute type: MediaInfo (validator is set below)
    media_info = bb.Attribute("media_info", nullable=True, user_defined=True)
    # Instance attribute type: SymlinkInfo (validator is set below)
    symlink_info = bb.Attribute("symlink_info", nullable=True, user_defined=True)
    # Instance attribute type: FileSharingInfo (validator is set below)
    sharing_info = bb.Attribute("sharing_info", nullable=True, user_defined=True)
    is_downloadable = bb.Attribute("is_downloadable")
    # Instance attribute type: ExportInfo (validator is set below)
    export_info = bb.Attribute("export_info", nullable=True, user_defined=True)
    has_explicit_shared_members = bb.Attribute("has_explicit_shared_members", nullable=True)
    content_hash = bb.Attribute("content_hash", nullable=True)
    # Instance attribute type: FileLockMetadata (validator is set below)
    file_lock_info = bb.Attribute("file_lock_info", nullable=True, user_defined=True)
        super(FileMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
FileMetadata_validator = bv.Struct(FileMetadata)
class SharingInfo(bb.Struct):
    Sharing info for a file or folder.
    :ivar files.SharingInfo.read_only: True if the file or folder is inside a
        read-only shared folder.
        '_read_only_value',
                 read_only=None):
        self._read_only_value = bb.NOT_SET
        if read_only is not None:
            self.read_only = read_only
    read_only = bb.Attribute("read_only")
        super(SharingInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingInfo_validator = bv.Struct(SharingInfo)
class FileSharingInfo(SharingInfo):
    Sharing info for a file which is contained by a shared folder.
    :ivar files.FileSharingInfo.parent_shared_folder_id: ID of shared folder
        that holds this file.
    :ivar files.FileSharingInfo.modified_by: The last user who modified the
        file. This field will be null if the user's account has been deleted.
        '_modified_by_value',
                 read_only=None,
                 modified_by=None):
        super(FileSharingInfo, self).__init__(read_only)
        self._modified_by_value = bb.NOT_SET
        if modified_by is not None:
            self.modified_by = modified_by
    parent_shared_folder_id = bb.Attribute("parent_shared_folder_id")
    modified_by = bb.Attribute("modified_by", nullable=True)
        super(FileSharingInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
FileSharingInfo_validator = bv.Struct(FileSharingInfo)
class FileStatus(bb.Union):
    active = None
    deleted = None
    def is_active(self):
        Check if the union tag is ``active``.
        return self._tag == 'active'
    def is_deleted(self):
        Check if the union tag is ``deleted``.
        return self._tag == 'deleted'
        super(FileStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
FileStatus_validator = bv.Union(FileStatus)
class FolderMetadata(Metadata):
    :ivar files.FolderMetadata.id: A unique identifier for the folder.
    :ivar files.FolderMetadata.shared_folder_id: Please use ``sharing_info``
    :ivar files.FolderMetadata.sharing_info: Set if the folder is contained in a
        shared folder or is a shared folder mount point.
    :ivar files.FolderMetadata.property_groups: Additional information if the
        file has custom properties with the property template specified. Note
        that only properties associated with user-owned templates, not
        team-owned templates, can be attached to folders.
        '_shared_folder_id_value',
                 shared_folder_id=None,
        super(FolderMetadata, self).__init__(name,
        self._shared_folder_id_value = bb.NOT_SET
        if shared_folder_id is not None:
            self.shared_folder_id = shared_folder_id
    shared_folder_id = bb.Attribute("shared_folder_id", nullable=True)
    # Instance attribute type: FolderSharingInfo (validator is set below)
        super(FolderMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
FolderMetadata_validator = bv.Struct(FolderMetadata)
class FolderSharingInfo(SharingInfo):
    Sharing info for a folder which is contained in a shared folder or is a
    shared folder mount point.
    :ivar files.FolderSharingInfo.parent_shared_folder_id: Set if the folder is
        contained by a shared folder.
    :ivar files.FolderSharingInfo.shared_folder_id: If this folder is a shared
        folder mount point, the ID of the shared folder mounted at this
        location.
    :ivar files.FolderSharingInfo.traverse_only: Specifies that the folder can
        only be traversed and the user can only see a limited subset of the
        contents of this folder because they don't have read access to this
        folder. They do, however, have access to some sub folder.
    :ivar files.FolderSharingInfo.no_access: Specifies that the folder cannot be
        accessed by the user.
        '_traverse_only_value',
        '_no_access_value',
                 traverse_only=None,
                 no_access=None):
        super(FolderSharingInfo, self).__init__(read_only)
        self._traverse_only_value = bb.NOT_SET
        self._no_access_value = bb.NOT_SET
        if traverse_only is not None:
            self.traverse_only = traverse_only
        if no_access is not None:
            self.no_access = no_access
    traverse_only = bb.Attribute("traverse_only")
    no_access = bb.Attribute("no_access")
        super(FolderSharingInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
FolderSharingInfo_validator = bv.Struct(FolderSharingInfo)
class GetCopyReferenceArg(bb.Struct):
    :ivar files.GetCopyReferenceArg.path: The path to the file or folder you
        want to get a copy reference to.
        super(GetCopyReferenceArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetCopyReferenceArg_validator = bv.Struct(GetCopyReferenceArg)
class GetCopyReferenceError(bb.Union):
        :rtype: GetCopyReferenceError
        super(GetCopyReferenceError, self)._process_custom_annotations(annotation_type, field_path, processor)
GetCopyReferenceError_validator = bv.Union(GetCopyReferenceError)
class GetCopyReferenceResult(bb.Struct):
    :ivar files.GetCopyReferenceResult.metadata: Metadata of the file or folder.
    :ivar files.GetCopyReferenceResult.copy_reference: A copy reference to the
    :ivar files.GetCopyReferenceResult.expires: The expiration date of the copy
        reference. This value is currently set to be far enough in the future so
        that expiration is effectively not an issue.
        '_copy_reference_value',
        '_expires_value',
                 metadata=None,
                 copy_reference=None,
                 expires=None):
        self._copy_reference_value = bb.NOT_SET
        self._expires_value = bb.NOT_SET
        if copy_reference is not None:
            self.copy_reference = copy_reference
        if expires is not None:
            self.expires = expires
    copy_reference = bb.Attribute("copy_reference")
    expires = bb.Attribute("expires")
        super(GetCopyReferenceResult, self)._process_custom_annotations(annotation_type, field_path, processor)
GetCopyReferenceResult_validator = bv.Struct(GetCopyReferenceResult)
class GetTagsArg(bb.Struct):
    :ivar files.GetTagsArg.paths: Path to the items.
                 paths=None):
        super(GetTagsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetTagsArg_validator = bv.Struct(GetTagsArg)
class GetTagsResult(bb.Struct):
    :ivar files.GetTagsResult.paths_to_tags: List of paths and their
        corresponding tags.
        '_paths_to_tags_value',
                 paths_to_tags=None):
        self._paths_to_tags_value = bb.NOT_SET
        if paths_to_tags is not None:
            self.paths_to_tags = paths_to_tags
    # Instance attribute type: list of [PathToTags] (validator is set below)
    paths_to_tags = bb.Attribute("paths_to_tags")
        super(GetTagsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
GetTagsResult_validator = bv.Struct(GetTagsResult)
class GetTemporaryLinkArg(bb.Struct):
    :ivar files.GetTemporaryLinkArg.path: The path to the file you want a
        temporary link to.
        super(GetTemporaryLinkArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetTemporaryLinkArg_validator = bv.Struct(GetTemporaryLinkArg)
class GetTemporaryLinkError(bb.Union):
    :ivar files.GetTemporaryLinkError.email_not_verified: This user's email
        address is not verified. This functionality is only available on
        accounts with a verified email address. Users can verify their email
        address `here <https://www.dropbox.com/help/317>`_.
    :ivar files.GetTemporaryLinkError.unsupported_file: Cannot get temporary
        link to this file type; use
    :ivar files.GetTemporaryLinkError.not_allowed: The user is not allowed to
        request a temporary link to the specified file. For example, this can
        occur if the file is restricted or if the user's links are `banned
        <https://help.dropbox.com/files-folders/share/banned-links>`_.
    email_not_verified = None
    not_allowed = None
        :rtype: GetTemporaryLinkError
    def is_email_not_verified(self):
        Check if the union tag is ``email_not_verified``.
        return self._tag == 'email_not_verified'
    def is_not_allowed(self):
        Check if the union tag is ``not_allowed``.
        return self._tag == 'not_allowed'
        super(GetTemporaryLinkError, self)._process_custom_annotations(annotation_type, field_path, processor)
GetTemporaryLinkError_validator = bv.Union(GetTemporaryLinkError)
class GetTemporaryLinkResult(bb.Struct):
    :ivar files.GetTemporaryLinkResult.metadata: Metadata of the file.
    :ivar files.GetTemporaryLinkResult.link: The temporary link which can be
        used to stream content the file.
        '_link_value',
                 link=None):
        self._link_value = bb.NOT_SET
        if link is not None:
            self.link = link
    link = bb.Attribute("link")
        super(GetTemporaryLinkResult, self)._process_custom_annotations(annotation_type, field_path, processor)
GetTemporaryLinkResult_validator = bv.Struct(GetTemporaryLinkResult)
class GetTemporaryUploadLinkArg(bb.Struct):
    :ivar files.GetTemporaryUploadLinkArg.commit_info: Contains the path and
        other optional modifiers for the future upload commit. Equivalent to the
        parameters provided to
        :meth:`dropbox.dropbox_client.Dropbox.files_upload`.
    :ivar files.GetTemporaryUploadLinkArg.duration: How long before this link
        expires, in seconds.  Attempting to start an upload with this link
        longer than this period  of time after link creation will result in an
        error.
        '_commit_info_value',
        '_duration_value',
                 commit_info=None,
                 duration=None):
        self._commit_info_value = bb.NOT_SET
        self._duration_value = bb.NOT_SET
        if commit_info is not None:
            self.commit_info = commit_info
        if duration is not None:
            self.duration = duration
    # Instance attribute type: CommitInfo (validator is set below)
    commit_info = bb.Attribute("commit_info", user_defined=True)
    # Instance attribute type: float (validator is set below)
    duration = bb.Attribute("duration")
        super(GetTemporaryUploadLinkArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetTemporaryUploadLinkArg_validator = bv.Struct(GetTemporaryUploadLinkArg)
class GetTemporaryUploadLinkResult(bb.Struct):
    :ivar files.GetTemporaryUploadLinkResult.link: The temporary link which can
        be used to stream a file to a Dropbox location.
        super(GetTemporaryUploadLinkResult, self)._process_custom_annotations(annotation_type, field_path, processor)
GetTemporaryUploadLinkResult_validator = bv.Struct(GetTemporaryUploadLinkResult)
class GetThumbnailBatchArg(bb.Struct):
    Arguments for
    :meth:`dropbox.dropbox_client.Dropbox.files_get_thumbnail_batch`.
    :ivar files.GetThumbnailBatchArg.entries: List of files to get thumbnails.
    # Instance attribute type: list of [ThumbnailArg] (validator is set below)
        super(GetThumbnailBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetThumbnailBatchArg_validator = bv.Struct(GetThumbnailBatchArg)
class GetThumbnailBatchError(bb.Union):
    :ivar files.GetThumbnailBatchError.too_many_files: The operation involves
        more than 25 files.
        super(GetThumbnailBatchError, self)._process_custom_annotations(annotation_type, field_path, processor)
GetThumbnailBatchError_validator = bv.Union(GetThumbnailBatchError)
class GetThumbnailBatchResult(bb.Struct):
    :ivar files.GetThumbnailBatchResult.entries: List of files and their
        thumbnails.
    # Instance attribute type: list of [GetThumbnailBatchResultEntry] (validator is set below)
        super(GetThumbnailBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
GetThumbnailBatchResult_validator = bv.Struct(GetThumbnailBatchResult)
class GetThumbnailBatchResultData(bb.Struct):
    :ivar files.GetThumbnailBatchResultData.thumbnail: A string containing the
        base64-encoded thumbnail data for this file.
        '_thumbnail_value',
                 thumbnail=None):
        self._thumbnail_value = bb.NOT_SET
        if thumbnail is not None:
            self.thumbnail = thumbnail
    thumbnail = bb.Attribute("thumbnail")
        super(GetThumbnailBatchResultData, self)._process_custom_annotations(annotation_type, field_path, processor)
GetThumbnailBatchResultData_validator = bv.Struct(GetThumbnailBatchResultData)
class GetThumbnailBatchResultEntry(bb.Union):
    :ivar ThumbnailError GetThumbnailBatchResultEntry.failure: The result for
        this file if it was an error.
        :param GetThumbnailBatchResultData val:
        :rtype: GetThumbnailBatchResultEntry
        :param ThumbnailError val:
        :rtype: GetThumbnailBatchResultData
        The result for this file if it was an error.
        :rtype: ThumbnailError
        super(GetThumbnailBatchResultEntry, self)._process_custom_annotations(annotation_type, field_path, processor)
GetThumbnailBatchResultEntry_validator = bv.Union(GetThumbnailBatchResultEntry)
class GpsCoordinates(bb.Struct):
    GPS coordinates for a photo or video.
    :ivar files.GpsCoordinates.latitude: Latitude of the GPS coordinates.
    :ivar files.GpsCoordinates.longitude: Longitude of the GPS coordinates.
        '_latitude_value',
        '_longitude_value',
                 latitude=None,
                 longitude=None):
        self._latitude_value = bb.NOT_SET
        self._longitude_value = bb.NOT_SET
        if latitude is not None:
            self.latitude = latitude
        if longitude is not None:
            self.longitude = longitude
    latitude = bb.Attribute("latitude")
    longitude = bb.Attribute("longitude")
        super(GpsCoordinates, self)._process_custom_annotations(annotation_type, field_path, processor)
GpsCoordinates_validator = bv.Struct(GpsCoordinates)
class HighlightSpan(bb.Struct):
    :ivar files.HighlightSpan.highlight_str: String to be determined whether it
        should be highlighted or not.
    :ivar files.HighlightSpan.is_highlighted: The string should be highlighted
        or not.
        '_highlight_str_value',
        '_is_highlighted_value',
                 highlight_str=None,
                 is_highlighted=None):
        self._highlight_str_value = bb.NOT_SET
        self._is_highlighted_value = bb.NOT_SET
        if highlight_str is not None:
            self.highlight_str = highlight_str
        if is_highlighted is not None:
            self.is_highlighted = is_highlighted
    highlight_str = bb.Attribute("highlight_str")
    is_highlighted = bb.Attribute("is_highlighted")
        super(HighlightSpan, self)._process_custom_annotations(annotation_type, field_path, processor)
HighlightSpan_validator = bv.Struct(HighlightSpan)
class ImportFormat(bb.Union):
    The import format of the incoming Paper doc content.
    :ivar files.ImportFormat.html: The provided data is interpreted as standard
        HTML.
    :ivar files.ImportFormat.markdown: The provided data is interpreted as
        markdown.
    :ivar files.ImportFormat.plain_text: The provided data is interpreted as
        plain text.
    html = None
    markdown = None
    plain_text = None
    def is_html(self):
        Check if the union tag is ``html``.
        return self._tag == 'html'
    def is_markdown(self):
        Check if the union tag is ``markdown``.
        return self._tag == 'markdown'
    def is_plain_text(self):
        Check if the union tag is ``plain_text``.
        return self._tag == 'plain_text'
        super(ImportFormat, self)._process_custom_annotations(annotation_type, field_path, processor)
ImportFormat_validator = bv.Union(ImportFormat)
class ListFolderArg(bb.Struct):
    :ivar files.ListFolderArg.path: A unique identifier for the file.
    :ivar files.ListFolderArg.recursive: If true, the list folder operation will
        be applied recursively to all subfolders and the response will contain
    :ivar files.ListFolderArg.include_media_info: If true,
        ``FileMetadata.media_info`` is set for photo and video. This parameter
        will no longer have an effect starting December 2, 2019.
    :ivar files.ListFolderArg.include_deleted: If true, the results will include
        entries for files and folders that used to exist but were deleted.
    :ivar files.ListFolderArg.include_has_explicit_shared_members: If true, the
    :ivar files.ListFolderArg.include_mounted_folders: If true, the results will
        include entries under mounted folders which includes app folder, shared
    :ivar files.ListFolderArg.limit: The maximum number of results to return per
        request. Note: This is an approximate number and there can be slightly
        more entries returned in some cases.
    :ivar files.ListFolderArg.shared_link: A shared link to list the contents
        of. If the link is password-protected, the password must be provided. If
        this field is present, ``ListFolderArg.path`` will be relative to root
        of the shared link. Only non-recursive mode is supported for shared
        link.
    :ivar files.ListFolderArg.include_property_groups: If set to a valid list of
        template IDs, ``FileMetadata.property_groups`` is set if there exists
    :ivar files.ListFolderArg.include_non_downloadable_files: If true, include
        files that are not downloadable, i.e. Google Docs.
        '_recursive_value',
        '_include_mounted_folders_value',
        '_shared_link_value',
        '_include_non_downloadable_files_value',
                 recursive=None,
                 include_mounted_folders=None,
                 include_non_downloadable_files=None):
        self._recursive_value = bb.NOT_SET
        self._include_mounted_folders_value = bb.NOT_SET
        self._shared_link_value = bb.NOT_SET
        self._include_non_downloadable_files_value = bb.NOT_SET
        if recursive is not None:
            self.recursive = recursive
        if include_mounted_folders is not None:
            self.include_mounted_folders = include_mounted_folders
        if shared_link is not None:
            self.shared_link = shared_link
        if include_non_downloadable_files is not None:
            self.include_non_downloadable_files = include_non_downloadable_files
    recursive = bb.Attribute("recursive")
    include_mounted_folders = bb.Attribute("include_mounted_folders")
    limit = bb.Attribute("limit", nullable=True)
    # Instance attribute type: SharedLink (validator is set below)
    shared_link = bb.Attribute("shared_link", nullable=True, user_defined=True)
    include_non_downloadable_files = bb.Attribute("include_non_downloadable_files")
        super(ListFolderArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderArg_validator = bv.Struct(ListFolderArg)
class ListFolderContinueArg(bb.Struct):
    :ivar files.ListFolderContinueArg.cursor: The cursor returned by your last
        call to :meth:`dropbox.dropbox_client.Dropbox.files_list_folder` or
        :meth:`dropbox.dropbox_client.Dropbox.files_list_folder_continue`.
        super(ListFolderContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderContinueArg_validator = bv.Struct(ListFolderContinueArg)
class ListFolderContinueError(bb.Union):
    :ivar files.ListFolderContinueError.reset: Indicates that the cursor has
        been invalidated. Call
        :meth:`dropbox.dropbox_client.Dropbox.files_list_folder` to obtain a new
        cursor.
        :rtype: ListFolderContinueError
        super(ListFolderContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderContinueError_validator = bv.Union(ListFolderContinueError)
class ListFolderError(bb.Union):
        :rtype: ListFolderError
    def template_error(cls, val):
        Create an instance of this class set to the ``template_error`` tag with
        :param file_properties.TemplateError val:
        return cls('template_error', val)
    def is_template_error(self):
        Check if the union tag is ``template_error``.
        return self._tag == 'template_error'
    def get_template_error(self):
        Only call this if :meth:`is_template_error` is true.
        :rtype: file_properties.TemplateError
        if not self.is_template_error():
            raise AttributeError("tag 'template_error' not set")
        super(ListFolderError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderError_validator = bv.Union(ListFolderError)
class ListFolderGetLatestCursorResult(bb.Struct):
    :ivar files.ListFolderGetLatestCursorResult.cursor: Pass the cursor into
        :meth:`dropbox.dropbox_client.Dropbox.files_list_folder_continue` to see
        what's changed in the folder since your previous query.
        super(ListFolderGetLatestCursorResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderGetLatestCursorResult_validator = bv.Struct(ListFolderGetLatestCursorResult)
class ListFolderLongpollArg(bb.Struct):
    :ivar files.ListFolderLongpollArg.cursor: A cursor as returned by
        Cursors retrieved by setting ``ListFolderArg.include_media_info`` to
        ``True`` are not supported.
    :ivar files.ListFolderLongpollArg.timeout: A timeout in seconds. The request
        will block for at most this length of time, plus up to 90 seconds of
        random jitter added to avoid the thundering herd problem. Care should be
        taken when using this parameter, as some network infrastructure does not
        '_timeout_value',
        self._timeout_value = bb.NOT_SET
    timeout = bb.Attribute("timeout")
        super(ListFolderLongpollArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderLongpollArg_validator = bv.Struct(ListFolderLongpollArg)
class ListFolderLongpollError(bb.Union):
    :ivar files.ListFolderLongpollError.reset: Indicates that the cursor has
        super(ListFolderLongpollError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderLongpollError_validator = bv.Union(ListFolderLongpollError)
class ListFolderLongpollResult(bb.Struct):
    :ivar files.ListFolderLongpollResult.changes: Indicates whether new changes
        are available. If true, call
        :meth:`dropbox.dropbox_client.Dropbox.files_list_folder_continue` to
        retrieve the changes.
    :ivar files.ListFolderLongpollResult.backoff: If present, backoff for at
        least this many seconds before calling
        :meth:`dropbox.dropbox_client.Dropbox.files_list_folder_longpoll` again.
        '_changes_value',
        '_backoff_value',
                 changes=None,
                 backoff=None):
        self._changes_value = bb.NOT_SET
        self._backoff_value = bb.NOT_SET
        if changes is not None:
            self.changes = changes
        if backoff is not None:
    changes = bb.Attribute("changes")
    backoff = bb.Attribute("backoff", nullable=True)
        super(ListFolderLongpollResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderLongpollResult_validator = bv.Struct(ListFolderLongpollResult)
class ListFolderResult(bb.Struct):
    :ivar files.ListFolderResult.entries: The files and (direct) subfolders in
        the folder.
    :ivar files.ListFolderResult.cursor: Pass the cursor into
    :ivar files.ListFolderResult.has_more: If true, then there are more entries
        available. Pass the cursor to
        retrieve the rest.
                 entries=None,
    # Instance attribute type: list of [Metadata] (validator is set below)
        super(ListFolderResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderResult_validator = bv.Struct(ListFolderResult)
class ListRevisionsArg(bb.Struct):
    :ivar files.ListRevisionsArg.path: The path to the file you want to see the
        revisions of.
    :ivar files.ListRevisionsArg.mode: Determines the behavior of the API in
        listing the revisions for a given file path or id.
    :ivar files.ListRevisionsArg.limit: The maximum number of revision entries
    # Instance attribute type: ListRevisionsMode (validator is set below)
        super(ListRevisionsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListRevisionsArg_validator = bv.Struct(ListRevisionsArg)
class ListRevisionsError(bb.Union):
        :rtype: ListRevisionsError
        super(ListRevisionsError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListRevisionsError_validator = bv.Union(ListRevisionsError)
class ListRevisionsMode(bb.Union):
    :ivar files.ListRevisionsMode.path: Returns revisions with the same file
        path as identified by the latest file entry at the given file path or
        id.
    :ivar files.ListRevisionsMode.id: Returns revisions with the same file id as
        identified by the latest file entry at the given file path or id.
    path = None
    def is_id(self):
        Check if the union tag is ``id``.
        return self._tag == 'id'
        super(ListRevisionsMode, self)._process_custom_annotations(annotation_type, field_path, processor)
ListRevisionsMode_validator = bv.Union(ListRevisionsMode)
class ListRevisionsResult(bb.Struct):
    :ivar files.ListRevisionsResult.is_deleted: If the file identified by the
        latest revision in the response is either deleted or moved.
    :ivar files.ListRevisionsResult.server_deleted: The time of deletion if the
        file was deleted.
    :ivar files.ListRevisionsResult.entries: The revisions for the file. Only
        revisions that are not deleted will show up here.
        '_server_deleted_value',
                 server_deleted=None):
        self._server_deleted_value = bb.NOT_SET
        if server_deleted is not None:
            self.server_deleted = server_deleted
    server_deleted = bb.Attribute("server_deleted", nullable=True)
    # Instance attribute type: list of [FileMetadata] (validator is set below)
        super(ListRevisionsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListRevisionsResult_validator = bv.Struct(ListRevisionsResult)
class LockConflictError(bb.Struct):
    :ivar files.LockConflictError.lock: The lock that caused the conflict.
        '_lock_value',
                 lock=None):
        self._lock_value = bb.NOT_SET
        if lock is not None:
            self.lock = lock
    # Instance attribute type: FileLock (validator is set below)
    lock = bb.Attribute("lock", user_defined=True)
        super(LockConflictError, self)._process_custom_annotations(annotation_type, field_path, processor)
LockConflictError_validator = bv.Struct(LockConflictError)
class LockFileArg(bb.Struct):
    :ivar files.LockFileArg.path: Path in the user's Dropbox to a file.
        super(LockFileArg, self)._process_custom_annotations(annotation_type, field_path, processor)
LockFileArg_validator = bv.Struct(LockFileArg)
class LockFileBatchArg(bb.Struct):
    :ivar files.LockFileBatchArg.entries: List of 'entries'. Each 'entry'
        contains a path of the file which will be locked or queried. Duplicate
        path arguments in the batch are considered only once.
    # Instance attribute type: list of [LockFileArg] (validator is set below)
        super(LockFileBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
LockFileBatchArg_validator = bv.Struct(LockFileBatchArg)
class LockFileBatchResult(FileOpsResult):
    :ivar files.LockFileBatchResult.entries: Each Entry in the 'entries' will
        have '.tag' with the operation status (e.g. success), the metadata for
        the file and the lock state after the operation.
        super(LockFileBatchResult, self).__init__()
    # Instance attribute type: list of [LockFileResultEntry] (validator is set below)
        super(LockFileBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
LockFileBatchResult_validator = bv.Struct(LockFileBatchResult)
class LockFileError(bb.Union):
    :ivar LookupError LockFileError.path_lookup: Could not find the specified
    :ivar files.LockFileError.too_many_write_operations: There are too many
        write operations in user's Dropbox. Please retry this request.
    :ivar files.LockFileError.too_many_files: There are too many files in one
    :ivar files.LockFileError.no_write_permission: The user does not have
        permissions to change the lock state or access the file.
    :ivar files.LockFileError.cannot_be_locked: Item is a type that cannot be
        locked.
    :ivar files.LockFileError.file_not_shared: Requested file is not currently
        shared.
    :ivar LockConflictError LockFileError.lock_conflict: The user action
        conflicts with an existing lock on the file.
    :ivar files.LockFileError.internal_error: Something went wrong with the job
        on Dropbox's end. You'll need to verify that the action you were taking
    no_write_permission = None
    cannot_be_locked = None
    file_not_shared = None
        :rtype: LockFileError
    def lock_conflict(cls, val):
        Create an instance of this class set to the ``lock_conflict`` tag with
        :param LockConflictError val:
        return cls('lock_conflict', val)
    def is_no_write_permission(self):
        Check if the union tag is ``no_write_permission``.
        return self._tag == 'no_write_permission'
    def is_cannot_be_locked(self):
        Check if the union tag is ``cannot_be_locked``.
        return self._tag == 'cannot_be_locked'
    def is_file_not_shared(self):
        Check if the union tag is ``file_not_shared``.
        return self._tag == 'file_not_shared'
    def is_lock_conflict(self):
        Check if the union tag is ``lock_conflict``.
        return self._tag == 'lock_conflict'
        Could not find the specified resource.
    def get_lock_conflict(self):
        The user action conflicts with an existing lock on the file.
        Only call this if :meth:`is_lock_conflict` is true.
        :rtype: LockConflictError
        if not self.is_lock_conflict():
            raise AttributeError("tag 'lock_conflict' not set")
        super(LockFileError, self)._process_custom_annotations(annotation_type, field_path, processor)
LockFileError_validator = bv.Union(LockFileError)
class LockFileResult(bb.Struct):
    :ivar files.LockFileResult.metadata: Metadata of the file.
    :ivar files.LockFileResult.lock: The file lock state after the operation.
        super(LockFileResult, self)._process_custom_annotations(annotation_type, field_path, processor)
LockFileResult_validator = bv.Struct(LockFileResult)
class LockFileResultEntry(bb.Union):
        :param LockFileResult val:
        :rtype: LockFileResultEntry
        :param LockFileError val:
        :rtype: LockFileResult
        super(LockFileResultEntry, self)._process_custom_annotations(annotation_type, field_path, processor)
LockFileResultEntry_validator = bv.Union(LockFileResultEntry)
    :ivar Optional[str] files.LookupError.malformed_path: The given path does
        not satisfy the required path format. Please refer to the :link:`Path
        formats documentation
        https://www.dropbox.com/developers/documentation/http/documentation#path-formats`
    :ivar files.LookupError.not_found: There is nothing at the given path.
    :ivar files.LookupError.not_file: We were expecting a file, but the given
        path refers to something that isn't a file.
    :ivar files.LookupError.not_folder: We were expecting a folder, but the
        given path refers to something that isn't a folder.
    :ivar files.LookupError.restricted_content: The file cannot be transferred
        because the content is restricted. For example, we might restrict a file
        due to legal requirements.
    :ivar files.LookupError.unsupported_content_type: This operation is not
        supported for this content type.
    :ivar files.LookupError.locked: The given path is locked.
    unsupported_content_type = None
    locked = None
        :param Optional[str] val:
    def is_unsupported_content_type(self):
        Check if the union tag is ``unsupported_content_type``.
        return self._tag == 'unsupported_content_type'
    def is_locked(self):
        Check if the union tag is ``locked``.
        return self._tag == 'locked'
        The given path does not satisfy the required path format. Please refer
        to the `Path formats documentation
        <https://www.dropbox.com/developers/documentation/http/documentation#path-formats>`_
        :rtype: Optional[str]
class MediaInfo(bb.Union):
    :ivar files.MediaInfo.pending: Indicate the photo/video is still under
        processing and metadata is not available yet.
    :ivar MediaMetadata MediaInfo.metadata: The metadata for the photo/video.
    pending = None
    def metadata(cls, val):
        Create an instance of this class set to the ``metadata`` tag with value
        :param MediaMetadata val:
        :rtype: MediaInfo
        return cls('metadata', val)
    def is_pending(self):
        Check if the union tag is ``pending``.
        return self._tag == 'pending'
    def is_metadata(self):
        Check if the union tag is ``metadata``.
        return self._tag == 'metadata'
    def get_metadata(self):
        The metadata for the photo/video.
        Only call this if :meth:`is_metadata` is true.
        :rtype: MediaMetadata
        if not self.is_metadata():
            raise AttributeError("tag 'metadata' not set")
        super(MediaInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
MediaInfo_validator = bv.Union(MediaInfo)
class MediaMetadata(bb.Struct):
    Metadata for a photo or video.
    :ivar files.MediaMetadata.dimensions: Dimension of the photo/video.
    :ivar files.MediaMetadata.location: The GPS coordinate of the photo/video.
    :ivar files.MediaMetadata.time_taken: The timestamp when the photo/video is
        taken.
        '_dimensions_value',
        '_location_value',
        '_time_taken_value',
                 dimensions=None,
                 location=None,
                 time_taken=None):
        self._dimensions_value = bb.NOT_SET
        self._location_value = bb.NOT_SET
        self._time_taken_value = bb.NOT_SET
        if dimensions is not None:
            self.dimensions = dimensions
            self.location = location
        if time_taken is not None:
            self.time_taken = time_taken
    # Instance attribute type: Dimensions (validator is set below)
    dimensions = bb.Attribute("dimensions", nullable=True, user_defined=True)
    # Instance attribute type: GpsCoordinates (validator is set below)
    location = bb.Attribute("location", nullable=True, user_defined=True)
    time_taken = bb.Attribute("time_taken", nullable=True)
        super(MediaMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
MediaMetadata_validator = bv.StructTree(MediaMetadata)
class MetadataV2(bb.Union):
    Metadata for a file, folder or other resource types.
        :param Metadata val:
        :rtype: MetadataV2
        :rtype: Metadata
        super(MetadataV2, self)._process_custom_annotations(annotation_type, field_path, processor)
MetadataV2_validator = bv.Union(MetadataV2)
class MinimalFileLinkMetadata(bb.Struct):
    :ivar files.MinimalFileLinkMetadata.url: URL of the shared link.
    :ivar files.MinimalFileLinkMetadata.id: Unique identifier for the linked
    :ivar files.MinimalFileLinkMetadata.path: Full path in the user's Dropbox.
        This always starts with a slash. This field will only be present only if
        the linked file is in the authenticated user's Dropbox.
    :ivar files.MinimalFileLinkMetadata.rev: A unique identifier for the current
        revision of a file. This field is the same rev as elsewhere in the API
        and can be used to detect changes and avoid conflicts.
    id = bb.Attribute("id", nullable=True)
    path = bb.Attribute("path", nullable=True)
        super(MinimalFileLinkMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
MinimalFileLinkMetadata_validator = bv.Struct(MinimalFileLinkMetadata)
class RelocationBatchArgBase(bb.Struct):
    :ivar files.RelocationBatchArgBase.entries: List of entries to be moved or
        copied. Each entry is :class:`RelocationPath`.
    :ivar files.RelocationBatchArgBase.autorename: If there's a conflict with
        any file, have the Dropbox server try to autorename that file to avoid
        the conflict.
    # Instance attribute type: list of [RelocationPath] (validator is set below)
        super(RelocationBatchArgBase, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchArgBase_validator = bv.Struct(RelocationBatchArgBase)
class MoveBatchArg(RelocationBatchArgBase):
    :ivar files.MoveBatchArg.allow_ownership_transfer: Allow moves by owner even
        if it would result in an ownership transfer for the content being moved.
        '_allow_ownership_transfer_value',
                 allow_ownership_transfer=None):
        super(MoveBatchArg, self).__init__(entries,
        self._allow_ownership_transfer_value = bb.NOT_SET
        if allow_ownership_transfer is not None:
            self.allow_ownership_transfer = allow_ownership_transfer
    allow_ownership_transfer = bb.Attribute("allow_ownership_transfer")
        super(MoveBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MoveBatchArg_validator = bv.Struct(MoveBatchArg)
class MoveIntoFamilyError(bb.Union):
    :ivar files.MoveIntoFamilyError.is_shared_folder: Moving shared folder into
        Family Room folder is not allowed.
    is_shared_folder = None
    def is_is_shared_folder(self):
        Check if the union tag is ``is_shared_folder``.
        return self._tag == 'is_shared_folder'
        super(MoveIntoFamilyError, self)._process_custom_annotations(annotation_type, field_path, processor)
MoveIntoFamilyError_validator = bv.Union(MoveIntoFamilyError)
class MoveIntoVaultError(bb.Union):
    :ivar files.MoveIntoVaultError.is_shared_folder: Moving shared folder into
        Vault is not allowed.
        super(MoveIntoVaultError, self)._process_custom_annotations(annotation_type, field_path, processor)
MoveIntoVaultError_validator = bv.Union(MoveIntoVaultError)
class PaperContentError(bb.Union):
    :ivar files.PaperContentError.insufficient_permissions: Your account does
        not have permissions to edit Paper docs.
    :ivar files.PaperContentError.content_malformed: The provided content was
        malformed and cannot be imported to Paper.
    :ivar files.PaperContentError.doc_length_exceeded: The Paper doc would be
        too large, split the content into multiple docs.
    :ivar files.PaperContentError.image_size_exceeded: The imported document
        contains an image that is too large. The current limit is 1MB. This only
        applies to HTML with data URI.
    insufficient_permissions = None
    content_malformed = None
    doc_length_exceeded = None
    image_size_exceeded = None
    def is_insufficient_permissions(self):
        Check if the union tag is ``insufficient_permissions``.
        return self._tag == 'insufficient_permissions'
    def is_content_malformed(self):
        Check if the union tag is ``content_malformed``.
        return self._tag == 'content_malformed'
    def is_doc_length_exceeded(self):
        Check if the union tag is ``doc_length_exceeded``.
        return self._tag == 'doc_length_exceeded'
    def is_image_size_exceeded(self):
        Check if the union tag is ``image_size_exceeded``.
        return self._tag == 'image_size_exceeded'
        super(PaperContentError, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperContentError_validator = bv.Union(PaperContentError)
class PaperCreateArg(bb.Struct):
    :ivar files.PaperCreateArg.path: The fully qualified path to the location in
        the user's Dropbox where the Paper Doc should be created. This should
        include the document's title and end with .paper.
    :ivar files.PaperCreateArg.import_format: The format of the provided data.
        '_import_format_value',
                 import_format=None):
        self._import_format_value = bb.NOT_SET
        if import_format is not None:
            self.import_format = import_format
    # Instance attribute type: ImportFormat (validator is set below)
    import_format = bb.Attribute("import_format", user_defined=True)
        super(PaperCreateArg, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperCreateArg_validator = bv.Struct(PaperCreateArg)
class PaperCreateError(PaperContentError):
    :ivar files.PaperCreateError.invalid_path: The file could not be saved to
        the specified location.
    :ivar files.PaperCreateError.email_unverified: The user's email must be
        verified to create Paper docs.
    :ivar files.PaperCreateError.invalid_file_extension: The file path must end
        in .paper.
    :ivar files.PaperCreateError.paper_disabled: Paper is disabled for your
    invalid_path = None
    invalid_file_extension = None
    def is_invalid_path(self):
        Check if the union tag is ``invalid_path``.
        return self._tag == 'invalid_path'
    def is_invalid_file_extension(self):
        Check if the union tag is ``invalid_file_extension``.
        return self._tag == 'invalid_file_extension'
        super(PaperCreateError, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperCreateError_validator = bv.Union(PaperCreateError)
class PaperCreateResult(bb.Struct):
    :ivar files.PaperCreateResult.url: URL to open the Paper Doc.
    :ivar files.PaperCreateResult.result_path: The fully qualified path the
        Paper Doc was actually created at.
    :ivar files.PaperCreateResult.file_id: The id to use in Dropbox APIs when
        referencing the Paper Doc.
    :ivar files.PaperCreateResult.paper_revision: The current doc revision.
        '_result_path_value',
        '_file_id_value',
                 result_path=None,
                 file_id=None,
        self._result_path_value = bb.NOT_SET
        self._file_id_value = bb.NOT_SET
        if result_path is not None:
            self.result_path = result_path
        if file_id is not None:
            self.file_id = file_id
    result_path = bb.Attribute("result_path")
    file_id = bb.Attribute("file_id")
    paper_revision = bb.Attribute("paper_revision")
        super(PaperCreateResult, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperCreateResult_validator = bv.Struct(PaperCreateResult)
class PaperDocUpdatePolicy(bb.Union):
    :ivar files.PaperDocUpdatePolicy.update: Sets the doc content to the
        provided content if the provided paper_revision matches the latest doc
        revision. Otherwise, returns an error.
    :ivar files.PaperDocUpdatePolicy.overwrite: Sets the doc content to the
        provided content without checking paper_revision.
    :ivar files.PaperDocUpdatePolicy.prepend: Adds the provided content to the
        beginning of the doc without checking paper_revision.
    :ivar files.PaperDocUpdatePolicy.append: Adds the provided content to the
        end of the doc without checking paper_revision.
    update = None
    overwrite = None
    prepend = None
    append = None
    def is_overwrite(self):
        Check if the union tag is ``overwrite``.
        return self._tag == 'overwrite'
    def is_prepend(self):
        Check if the union tag is ``prepend``.
        return self._tag == 'prepend'
    def is_append(self):
        Check if the union tag is ``append``.
        return self._tag == 'append'
        super(PaperDocUpdatePolicy, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperDocUpdatePolicy_validator = bv.Union(PaperDocUpdatePolicy)
class PaperUpdateArg(bb.Struct):
    :ivar files.PaperUpdateArg.path: Path in the user's Dropbox to update. The
        path must correspond to a Paper doc or an error will be returned.
    :ivar files.PaperUpdateArg.import_format: The format of the provided data.
    :ivar files.PaperUpdateArg.doc_update_policy: How the provided content
        should be applied to the doc.
    :ivar files.PaperUpdateArg.paper_revision: The latest doc revision. Required
        '_doc_update_policy_value',
                 import_format=None,
                 doc_update_policy=None,
        self._doc_update_policy_value = bb.NOT_SET
        if doc_update_policy is not None:
            self.doc_update_policy = doc_update_policy
    # Instance attribute type: PaperDocUpdatePolicy (validator is set below)
    doc_update_policy = bb.Attribute("doc_update_policy", user_defined=True)
        super(PaperUpdateArg, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperUpdateArg_validator = bv.Struct(PaperUpdateArg)
class PaperUpdateError(PaperContentError):
    :ivar files.PaperUpdateError.revision_mismatch: The provided revision does
        not match the document head.
    :ivar files.PaperUpdateError.doc_archived: This operation is not allowed on
        archived Paper docs.
    :ivar files.PaperUpdateError.doc_deleted: This operation is not allowed on
        deleted Paper docs.
    revision_mismatch = None
    doc_archived = None
    doc_deleted = None
        :rtype: PaperUpdateError
    def is_revision_mismatch(self):
        Check if the union tag is ``revision_mismatch``.
        return self._tag == 'revision_mismatch'
    def is_doc_archived(self):
        Check if the union tag is ``doc_archived``.
        return self._tag == 'doc_archived'
    def is_doc_deleted(self):
        Check if the union tag is ``doc_deleted``.
        return self._tag == 'doc_deleted'
        super(PaperUpdateError, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperUpdateError_validator = bv.Union(PaperUpdateError)
class PaperUpdateResult(bb.Struct):
    :ivar files.PaperUpdateResult.paper_revision: The current doc revision.
        super(PaperUpdateResult, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperUpdateResult_validator = bv.Struct(PaperUpdateResult)
class PathOrLink(bb.Union):
        :rtype: PathOrLink
    def link(cls, val):
        Create an instance of this class set to the ``link`` tag with value
        :param SharedLinkFileInfo val:
        return cls('link', val)
    def is_link(self):
        Check if the union tag is ``link``.
        return self._tag == 'link'
    def get_link(self):
        Only call this if :meth:`is_link` is true.
        :rtype: SharedLinkFileInfo
        if not self.is_link():
            raise AttributeError("tag 'link' not set")
        super(PathOrLink, self)._process_custom_annotations(annotation_type, field_path, processor)
PathOrLink_validator = bv.Union(PathOrLink)
class PathToTags(bb.Struct):
    :ivar files.PathToTags.path: Path of the item.
    :ivar files.PathToTags.tags: Tags assigned to this item.
        '_tags_value',
                 tags=None):
        self._tags_value = bb.NOT_SET
        if tags is not None:
            self.tags = tags
    # Instance attribute type: list of [Tag] (validator is set below)
    tags = bb.Attribute("tags")
        super(PathToTags, self)._process_custom_annotations(annotation_type, field_path, processor)
PathToTags_validator = bv.Struct(PathToTags)
class PhotoMetadata(MediaMetadata):
    Metadata for a photo.
        super(PhotoMetadata, self).__init__(dimensions,
                                            time_taken)
        super(PhotoMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
PhotoMetadata_validator = bv.Struct(PhotoMetadata)
class PreviewArg(bb.Struct):
    :ivar files.PreviewArg.path: The path of the file to preview.
    :ivar files.PreviewArg.rev: Please specify revision in ``path`` instead.
        super(PreviewArg, self)._process_custom_annotations(annotation_type, field_path, processor)
PreviewArg_validator = bv.Struct(PreviewArg)
class PreviewError(bb.Union):
    :ivar LookupError PreviewError.path: An error occurs when downloading
        metadata for the file.
    :ivar files.PreviewError.in_progress: This preview generation is still in
        progress and the file is not ready  for preview yet.
    :ivar files.PreviewError.unsupported_extension: The file extension is not
        supported preview generation.
    :ivar files.PreviewError.unsupported_content: The file content is not
        supported for preview generation.
    unsupported_extension = None
    unsupported_content = None
        :rtype: PreviewError
    def is_unsupported_extension(self):
        Check if the union tag is ``unsupported_extension``.
        return self._tag == 'unsupported_extension'
    def is_unsupported_content(self):
        Check if the union tag is ``unsupported_content``.
        return self._tag == 'unsupported_content'
        An error occurs when downloading metadata for the file.
        super(PreviewError, self)._process_custom_annotations(annotation_type, field_path, processor)
PreviewError_validator = bv.Union(PreviewError)
class PreviewResult(bb.Struct):
    :ivar files.PreviewResult.file_metadata: Metadata corresponding to the file
        received as an argument. Will be populated if the endpoint is called
        with a path (ReadPath).
    :ivar files.PreviewResult.link_metadata: Minimal metadata corresponding to
        the file received as an argument. Will be populated if the endpoint is
        called using a shared link (SharedLinkFileInfo).
        '_link_metadata_value',
                 file_metadata=None,
                 link_metadata=None):
        self._link_metadata_value = bb.NOT_SET
        if link_metadata is not None:
            self.link_metadata = link_metadata
    file_metadata = bb.Attribute("file_metadata", nullable=True, user_defined=True)
    # Instance attribute type: MinimalFileLinkMetadata (validator is set below)
    link_metadata = bb.Attribute("link_metadata", nullable=True, user_defined=True)
        super(PreviewResult, self)._process_custom_annotations(annotation_type, field_path, processor)
PreviewResult_validator = bv.Struct(PreviewResult)
class RelocationPath(bb.Struct):
    :ivar files.RelocationPath.from_path: Path in the user's Dropbox to be
        copied or moved.
    :ivar files.RelocationPath.to_path: Path in the user's Dropbox that is the
        destination.
        '_from_path_value',
        '_to_path_value',
                 from_path=None,
                 to_path=None):
        self._from_path_value = bb.NOT_SET
        self._to_path_value = bb.NOT_SET
        if from_path is not None:
            self.from_path = from_path
        if to_path is not None:
            self.to_path = to_path
    from_path = bb.Attribute("from_path")
    to_path = bb.Attribute("to_path")
        super(RelocationPath, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationPath_validator = bv.Struct(RelocationPath)
class RelocationArg(RelocationPath):
    :ivar files.RelocationArg.allow_shared_folder: This flag has no effect.
    :ivar files.RelocationArg.autorename: If there's a conflict, have the
        Dropbox server try to autorename the file to avoid the conflict.
    :ivar files.RelocationArg.allow_ownership_transfer: Allow moves by owner
        even if it would result in an ownership transfer for the content being
        moved. This does not apply to copies.
        '_allow_shared_folder_value',
                 to_path=None,
                 allow_shared_folder=None,
        super(RelocationArg, self).__init__(from_path,
                                            to_path)
        self._allow_shared_folder_value = bb.NOT_SET
        if allow_shared_folder is not None:
            self.allow_shared_folder = allow_shared_folder
    allow_shared_folder = bb.Attribute("allow_shared_folder")
        super(RelocationArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationArg_validator = bv.Struct(RelocationArg)
class RelocationBatchArg(RelocationBatchArgBase):
    :ivar files.RelocationBatchArg.allow_shared_folder: This flag has no effect.
    :ivar files.RelocationBatchArg.allow_ownership_transfer: Allow moves by
        owner even if it would result in an ownership transfer for the content
        being moved. This does not apply to copies.
        super(RelocationBatchArg, self).__init__(entries,
        super(RelocationBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchArg_validator = bv.Struct(RelocationBatchArg)
class RelocationError(bb.Union):
    :ivar files.RelocationError.cant_copy_shared_folder: Shared folders can't be
        copied.
    :ivar files.RelocationError.cant_nest_shared_folder: Your move operation
        would result in nested shared folders.  This is not allowed.
    :ivar files.RelocationError.cant_move_folder_into_itself: You cannot move a
        folder into itself.
    :ivar files.RelocationError.too_many_files: The operation would involve more
        than 10,000 files and folders.
    :ivar files.RelocationError.duplicated_or_nested_paths: There are
        duplicated/nested paths among ``RelocationArg.from_path`` and
        ``RelocationArg.to_path``.
    :ivar files.RelocationError.cant_transfer_ownership: Your move operation
        would result in an ownership transfer. You may reissue the request with
        the field ``RelocationArg.allow_ownership_transfer`` to true.
    :ivar files.RelocationError.insufficient_quota: The current user does not
        have enough space to move or copy the files.
    :ivar files.RelocationError.internal_error: Something went wrong with the
        job on Dropbox's end. You'll need to verify that the action you were
        taking succeeded, and if not, try again. This should happen very rarely.
    :ivar files.RelocationError.cant_move_shared_folder: Can't move the shared
        folder to the given destination.
    :ivar MoveIntoVaultError RelocationError.cant_move_into_vault: Some content
        cannot be moved into Vault under certain circumstances, see detailed
    :ivar MoveIntoFamilyError RelocationError.cant_move_into_family: Some
        content cannot be moved into the Family Room folder under certain
        circumstances, see detailed error.
    cant_copy_shared_folder = None
    cant_nest_shared_folder = None
    cant_move_folder_into_itself = None
    duplicated_or_nested_paths = None
    cant_transfer_ownership = None
    insufficient_quota = None
    cant_move_shared_folder = None
    def from_lookup(cls, val):
        Create an instance of this class set to the ``from_lookup`` tag with
        :rtype: RelocationError
        return cls('from_lookup', val)
    def from_write(cls, val):
        Create an instance of this class set to the ``from_write`` tag with
        return cls('from_write', val)
    def to(cls, val):
        Create an instance of this class set to the ``to`` tag with value
        return cls('to', val)
    def cant_move_into_vault(cls, val):
        Create an instance of this class set to the ``cant_move_into_vault`` tag
        :param MoveIntoVaultError val:
        return cls('cant_move_into_vault', val)
    def cant_move_into_family(cls, val):
        Create an instance of this class set to the ``cant_move_into_family``
        :param MoveIntoFamilyError val:
        return cls('cant_move_into_family', val)
    def is_from_lookup(self):
        Check if the union tag is ``from_lookup``.
        return self._tag == 'from_lookup'
    def is_from_write(self):
        Check if the union tag is ``from_write``.
        return self._tag == 'from_write'
    def is_to(self):
        Check if the union tag is ``to``.
        return self._tag == 'to'
    def is_cant_copy_shared_folder(self):
        Check if the union tag is ``cant_copy_shared_folder``.
        return self._tag == 'cant_copy_shared_folder'
    def is_cant_nest_shared_folder(self):
        Check if the union tag is ``cant_nest_shared_folder``.
        return self._tag == 'cant_nest_shared_folder'
    def is_cant_move_folder_into_itself(self):
        Check if the union tag is ``cant_move_folder_into_itself``.
        return self._tag == 'cant_move_folder_into_itself'
    def is_duplicated_or_nested_paths(self):
        Check if the union tag is ``duplicated_or_nested_paths``.
        return self._tag == 'duplicated_or_nested_paths'
    def is_cant_transfer_ownership(self):
        Check if the union tag is ``cant_transfer_ownership``.
        return self._tag == 'cant_transfer_ownership'
    def is_insufficient_quota(self):
        Check if the union tag is ``insufficient_quota``.
        return self._tag == 'insufficient_quota'
    def is_cant_move_shared_folder(self):
        Check if the union tag is ``cant_move_shared_folder``.
        return self._tag == 'cant_move_shared_folder'
    def is_cant_move_into_vault(self):
        Check if the union tag is ``cant_move_into_vault``.
        return self._tag == 'cant_move_into_vault'
    def is_cant_move_into_family(self):
        Check if the union tag is ``cant_move_into_family``.
        return self._tag == 'cant_move_into_family'
    def get_from_lookup(self):
        Only call this if :meth:`is_from_lookup` is true.
        if not self.is_from_lookup():
            raise AttributeError("tag 'from_lookup' not set")
    def get_from_write(self):
        Only call this if :meth:`is_from_write` is true.
        if not self.is_from_write():
            raise AttributeError("tag 'from_write' not set")
    def get_to(self):
        Only call this if :meth:`is_to` is true.
        if not self.is_to():
            raise AttributeError("tag 'to' not set")
    def get_cant_move_into_vault(self):
        Some content cannot be moved into Vault under certain circumstances, see
        detailed error.
        Only call this if :meth:`is_cant_move_into_vault` is true.
        :rtype: MoveIntoVaultError
        if not self.is_cant_move_into_vault():
            raise AttributeError("tag 'cant_move_into_vault' not set")
    def get_cant_move_into_family(self):
        Some content cannot be moved into the Family Room folder under certain
        Only call this if :meth:`is_cant_move_into_family` is true.
        :rtype: MoveIntoFamilyError
        if not self.is_cant_move_into_family():
            raise AttributeError("tag 'cant_move_into_family' not set")
        super(RelocationError, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationError_validator = bv.Union(RelocationError)
class RelocationBatchError(RelocationError):
    :ivar files.RelocationBatchError.too_many_write_operations: There are too
        many write operations in user's Dropbox. Please retry this request.
        super(RelocationBatchError, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchError_validator = bv.Union(RelocationBatchError)
class RelocationBatchErrorEntry(bb.Union):
    :ivar RelocationError RelocationBatchErrorEntry.relocation_error: User
        errors that retry won't help.
    :ivar files.RelocationBatchErrorEntry.internal_error: Something went wrong
        with the job on Dropbox's end. You'll need to verify that the action you
        were taking succeeded, and if not, try again. This should happen very
        rarely.
    :ivar files.RelocationBatchErrorEntry.too_many_write_operations: There are
        too many write operations in user's Dropbox. Please retry this request.
    def relocation_error(cls, val):
        Create an instance of this class set to the ``relocation_error`` tag
        :param RelocationError val:
        :rtype: RelocationBatchErrorEntry
        return cls('relocation_error', val)
    def is_relocation_error(self):
        Check if the union tag is ``relocation_error``.
        return self._tag == 'relocation_error'
    def get_relocation_error(self):
        User errors that retry won't help.
        Only call this if :meth:`is_relocation_error` is true.
        if not self.is_relocation_error():
            raise AttributeError("tag 'relocation_error' not set")
        super(RelocationBatchErrorEntry, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchErrorEntry_validator = bv.Union(RelocationBatchErrorEntry)
class RelocationBatchJobStatus(async_.PollResultBase):
    :ivar RelocationBatchResult RelocationBatchJobStatus.complete: The copy or
        move batch job has finished.
    :ivar RelocationBatchError RelocationBatchJobStatus.failed: The copy or move
        batch job has failed with exception.
        :param RelocationBatchResult val:
        :rtype: RelocationBatchJobStatus
        :param RelocationBatchError val:
        The copy or move batch job has finished.
        :rtype: RelocationBatchResult
        The copy or move batch job has failed with exception.
        :rtype: RelocationBatchError
        super(RelocationBatchJobStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchJobStatus_validator = bv.Union(RelocationBatchJobStatus)
class RelocationBatchLaunch(async_.LaunchResultBase):
    Result returned by :meth:`dropbox.dropbox_client.Dropbox.files_copy_batch`
    or :meth:`dropbox.dropbox_client.Dropbox.files_move_batch` that may either
    launch an asynchronous job or complete synchronously.
        :rtype: RelocationBatchLaunch
        super(RelocationBatchLaunch, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchLaunch_validator = bv.Union(RelocationBatchLaunch)
class RelocationBatchResult(FileOpsResult):
        super(RelocationBatchResult, self).__init__()
    # Instance attribute type: list of [RelocationBatchResultData] (validator is set below)
        super(RelocationBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchResult_validator = bv.Struct(RelocationBatchResult)
class RelocationBatchResultData(bb.Struct):
    :ivar files.RelocationBatchResultData.metadata: Metadata of the relocated
        super(RelocationBatchResultData, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchResultData_validator = bv.Struct(RelocationBatchResultData)
class RelocationBatchResultEntry(bb.Union):
        :rtype: RelocationBatchResultEntry
        :param RelocationBatchErrorEntry val:
        super(RelocationBatchResultEntry, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchResultEntry_validator = bv.Union(RelocationBatchResultEntry)
class RelocationBatchV2JobStatus(async_.PollResultBase):
    :meth:`dropbox.dropbox_client.Dropbox.files_copy_batch_check` or
    :meth:`dropbox.dropbox_client.Dropbox.files_move_batch_check` that may
    either be in progress or completed with result for each entry.
    :ivar RelocationBatchV2Result RelocationBatchV2JobStatus.complete: The copy
        or move batch job has finished.
        :param RelocationBatchV2Result val:
        :rtype: RelocationBatchV2JobStatus
        :rtype: RelocationBatchV2Result
        super(RelocationBatchV2JobStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchV2JobStatus_validator = bv.Union(RelocationBatchV2JobStatus)
class RelocationBatchV2Launch(async_.LaunchResultBase):
        :rtype: RelocationBatchV2Launch
        super(RelocationBatchV2Launch, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchV2Launch_validator = bv.Union(RelocationBatchV2Launch)
class RelocationBatchV2Result(FileOpsResult):
    :ivar files.RelocationBatchV2Result.entries: Each entry in
        CopyBatchArg.entries or ``MoveBatchArg.entries`` will appear at the same
        position inside ``RelocationBatchV2Result.entries``.
        super(RelocationBatchV2Result, self).__init__()
    # Instance attribute type: list of [RelocationBatchResultEntry] (validator is set below)
        super(RelocationBatchV2Result, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationBatchV2Result_validator = bv.Struct(RelocationBatchV2Result)
class RelocationResult(FileOpsResult):
    :ivar files.RelocationResult.metadata: Metadata of the relocated object.
        super(RelocationResult, self).__init__()
        super(RelocationResult, self)._process_custom_annotations(annotation_type, field_path, processor)
RelocationResult_validator = bv.Struct(RelocationResult)
class RemoveTagArg(bb.Struct):
    :ivar files.RemoveTagArg.path: Path to the item to tag.
    :ivar files.RemoveTagArg.tag_text: The tag to remove. Will be automatically
        super(RemoveTagArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RemoveTagArg_validator = bv.Struct(RemoveTagArg)
class RemoveTagError(BaseTagError):
    :ivar files.RemoveTagError.tag_not_present: That tag doesn't exist at this
    tag_not_present = None
    def is_tag_not_present(self):
        Check if the union tag is ``tag_not_present``.
        return self._tag == 'tag_not_present'
        super(RemoveTagError, self)._process_custom_annotations(annotation_type, field_path, processor)
RemoveTagError_validator = bv.Union(RemoveTagError)
class RestoreArg(bb.Struct):
    :ivar files.RestoreArg.path: The path to save the restored file.
    :ivar files.RestoreArg.rev: The revision to restore.
        super(RestoreArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RestoreArg_validator = bv.Struct(RestoreArg)
class RestoreError(bb.Union):
    :ivar LookupError RestoreError.path_lookup: An error occurs when downloading
    :ivar WriteError RestoreError.path_write: An error occurs when trying to
        restore the file to that path.
    :ivar files.RestoreError.invalid_revision: The revision is invalid. It may
        not exist or may point to a deleted file.
    :ivar files.RestoreError.in_progress: The restore is currently executing,
        but has not yet completed.
    invalid_revision = None
        :rtype: RestoreError
    def is_invalid_revision(self):
        Check if the union tag is ``invalid_revision``.
        return self._tag == 'invalid_revision'
        An error occurs when trying to restore the file to that path.
        super(RestoreError, self)._process_custom_annotations(annotation_type, field_path, processor)
RestoreError_validator = bv.Union(RestoreError)
class SaveCopyReferenceArg(bb.Struct):
    :ivar files.SaveCopyReferenceArg.copy_reference: A copy reference returned
        by :meth:`dropbox.dropbox_client.Dropbox.files_copy_reference_get`.
    :ivar files.SaveCopyReferenceArg.path: Path in the user's Dropbox that is
        the destination.
        super(SaveCopyReferenceArg, self)._process_custom_annotations(annotation_type, field_path, processor)
SaveCopyReferenceArg_validator = bv.Struct(SaveCopyReferenceArg)
class SaveCopyReferenceError(bb.Union):
    :ivar files.SaveCopyReferenceError.invalid_copy_reference: The copy
        reference is invalid.
    :ivar files.SaveCopyReferenceError.no_permission: You don't have permission
        to save the given copy reference. Please make sure this app is same app
        which created the copy reference and the source user is still linked to
        the app.
    :ivar files.SaveCopyReferenceError.not_found: The file referenced by the
        copy reference cannot be found.
    :ivar files.SaveCopyReferenceError.too_many_files: The operation would
        involve more than 10,000 files and folders.
    invalid_copy_reference = None
        :rtype: SaveCopyReferenceError
    def is_invalid_copy_reference(self):
        Check if the union tag is ``invalid_copy_reference``.
        return self._tag == 'invalid_copy_reference'
        super(SaveCopyReferenceError, self)._process_custom_annotations(annotation_type, field_path, processor)
SaveCopyReferenceError_validator = bv.Union(SaveCopyReferenceError)
class SaveCopyReferenceResult(bb.Struct):
    :ivar files.SaveCopyReferenceResult.metadata: The metadata of the saved file
        or folder in the user's Dropbox.
        super(SaveCopyReferenceResult, self)._process_custom_annotations(annotation_type, field_path, processor)
SaveCopyReferenceResult_validator = bv.Struct(SaveCopyReferenceResult)
class SaveUrlArg(bb.Struct):
    :ivar files.SaveUrlArg.path: The path in Dropbox where the URL will be saved
    :ivar files.SaveUrlArg.url: The URL to be saved.
                 url=None):
        super(SaveUrlArg, self)._process_custom_annotations(annotation_type, field_path, processor)
SaveUrlArg_validator = bv.Struct(SaveUrlArg)
class SaveUrlError(bb.Union):
    :ivar files.SaveUrlError.download_failed: Failed downloading the given URL.
        The URL may be  password-protected and the password provided was
        incorrect,  or the link may be disabled.
    :ivar files.SaveUrlError.invalid_url: The given URL is invalid.
    :ivar files.SaveUrlError.not_found: The file where the URL is saved to no
    download_failed = None
    invalid_url = None
        :rtype: SaveUrlError
    def is_download_failed(self):
        Check if the union tag is ``download_failed``.
        return self._tag == 'download_failed'
    def is_invalid_url(self):
        Check if the union tag is ``invalid_url``.
        return self._tag == 'invalid_url'
        super(SaveUrlError, self)._process_custom_annotations(annotation_type, field_path, processor)
SaveUrlError_validator = bv.Union(SaveUrlError)
class SaveUrlJobStatus(async_.PollResultBase):
    :ivar FileMetadata SaveUrlJobStatus.complete: Metadata of the file where the
        URL is saved to.
        :param FileMetadata val:
        :rtype: SaveUrlJobStatus
        :param SaveUrlError val:
        Metadata of the file where the URL is saved to.
        :rtype: FileMetadata
        super(SaveUrlJobStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
SaveUrlJobStatus_validator = bv.Union(SaveUrlJobStatus)
class SaveUrlResult(async_.LaunchResultBase):
    :ivar FileMetadata SaveUrlResult.complete: Metadata of the file where the
        :rtype: SaveUrlResult
        super(SaveUrlResult, self)._process_custom_annotations(annotation_type, field_path, processor)
SaveUrlResult_validator = bv.Union(SaveUrlResult)
class SearchArg(bb.Struct):
    :ivar files.SearchArg.path: The path in the user's Dropbox to search. Should
    :ivar files.SearchArg.query: The string to search for. Query string may be
        rewritten to improve relevance of results. The string is split on spaces
        into multiple tokens. For file name searching, the last token is used
        for prefix matching (i.e. "bat c" matches "bat cave" but not "batman
        car").
    :ivar files.SearchArg.start: The starting index within the search results
        (used for paging).
    :ivar files.SearchArg.max_results: The maximum number of search results to
        return.
    :ivar files.SearchArg.mode: The search mode (filename, filename_and_content,
        or deleted_filename). Note that searching file content is only available
        for Dropbox Business accounts.
        '_start_value',
        '_max_results_value',
                 start=None,
                 max_results=None,
                 mode=None):
        self._start_value = bb.NOT_SET
        self._max_results_value = bb.NOT_SET
        if start is not None:
            self.start = start
        if max_results is not None:
            self.max_results = max_results
    start = bb.Attribute("start")
    max_results = bb.Attribute("max_results")
    # Instance attribute type: SearchMode (validator is set below)
        super(SearchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchArg_validator = bv.Struct(SearchArg)
class SearchError(bb.Union):
    :ivar files.SearchError.internal_error: Something went wrong, please try
        again.
        :rtype: SearchError
    def invalid_argument(cls, val):
        Create an instance of this class set to the ``invalid_argument`` tag
        return cls('invalid_argument', val)
    def is_invalid_argument(self):
        Check if the union tag is ``invalid_argument``.
        return self._tag == 'invalid_argument'
    def get_invalid_argument(self):
        Only call this if :meth:`is_invalid_argument` is true.
        if not self.is_invalid_argument():
            raise AttributeError("tag 'invalid_argument' not set")
        super(SearchError, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchError_validator = bv.Union(SearchError)
class SearchMatch(bb.Struct):
    :ivar files.SearchMatch.match_type: The type of the match.
    :ivar files.SearchMatch.metadata: The metadata for the matched file or
        '_match_type_value',
                 match_type=None,
        self._match_type_value = bb.NOT_SET
        if match_type is not None:
            self.match_type = match_type
    # Instance attribute type: SearchMatchType (validator is set below)
    match_type = bb.Attribute("match_type", user_defined=True)
        super(SearchMatch, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchMatch_validator = bv.Struct(SearchMatch)
class SearchMatchFieldOptions(bb.Struct):
    :ivar files.SearchMatchFieldOptions.include_highlights: Whether to include
        highlight span from file title.
        '_include_highlights_value',
        self._include_highlights_value = bb.NOT_SET
        if include_highlights is not None:
            self.include_highlights = include_highlights
    include_highlights = bb.Attribute("include_highlights")
        super(SearchMatchFieldOptions, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchMatchFieldOptions_validator = bv.Struct(SearchMatchFieldOptions)
class SearchMatchType(bb.Union):
    Indicates what type of match was found for a given item.
    :ivar files.SearchMatchType.filename: This item was matched on its file or
        folder name.
    :ivar files.SearchMatchType.content: This item was matched based on its file
        contents.
    :ivar files.SearchMatchType.both: This item was matched based on both its
        contents and its file name.
    filename = None
    both = None
    def is_filename(self):
        Check if the union tag is ``filename``.
        return self._tag == 'filename'
    def is_content(self):
        Check if the union tag is ``content``.
        return self._tag == 'content'
    def is_both(self):
        Check if the union tag is ``both``.
        return self._tag == 'both'
        super(SearchMatchType, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchMatchType_validator = bv.Union(SearchMatchType)
class SearchMatchTypeV2(bb.Union):
    :ivar files.SearchMatchTypeV2.filename: This item was matched on its file or
    :ivar files.SearchMatchTypeV2.file_content: This item was matched based on
        its file contents.
    :ivar files.SearchMatchTypeV2.filename_and_content: This item was matched
        based on both its contents and its file name.
    :ivar files.SearchMatchTypeV2.image_content: This item was matched on image
        content.
    file_content = None
    filename_and_content = None
    image_content = None
    def is_file_content(self):
        Check if the union tag is ``file_content``.
        return self._tag == 'file_content'
    def is_filename_and_content(self):
        Check if the union tag is ``filename_and_content``.
        return self._tag == 'filename_and_content'
    def is_image_content(self):
        Check if the union tag is ``image_content``.
        return self._tag == 'image_content'
        super(SearchMatchTypeV2, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchMatchTypeV2_validator = bv.Union(SearchMatchTypeV2)
class SearchMatchV2(bb.Struct):
    :ivar files.SearchMatchV2.metadata: The metadata for the matched file or
    :ivar files.SearchMatchV2.match_type: The type of the match.
    :ivar files.SearchMatchV2.highlight_spans: The list of HighlightSpan
        determines which parts of the file title should be highlighted.
        '_highlight_spans_value',
                 highlight_spans=None):
        self._highlight_spans_value = bb.NOT_SET
        if highlight_spans is not None:
            self.highlight_spans = highlight_spans
    # Instance attribute type: MetadataV2 (validator is set below)
    # Instance attribute type: SearchMatchTypeV2 (validator is set below)
    match_type = bb.Attribute("match_type", nullable=True, user_defined=True)
    # Instance attribute type: list of [HighlightSpan] (validator is set below)
    highlight_spans = bb.Attribute("highlight_spans", nullable=True)
        super(SearchMatchV2, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchMatchV2_validator = bv.Struct(SearchMatchV2)
class SearchMode(bb.Union):
    :ivar files.SearchMode.filename: Search file and folder names.
    :ivar files.SearchMode.filename_and_content: Search file and folder names as
        well as file contents.
    :ivar files.SearchMode.deleted_filename: Search for deleted file and folder
    deleted_filename = None
    def is_deleted_filename(self):
        Check if the union tag is ``deleted_filename``.
        return self._tag == 'deleted_filename'
        super(SearchMode, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchMode_validator = bv.Union(SearchMode)
class SearchOptions(bb.Struct):
    :ivar files.SearchOptions.path: Scopes the search to a path in the user's
        Dropbox. Searches the entire Dropbox if not specified.
    :ivar files.SearchOptions.max_results: The maximum number of search results
        to return.
    :ivar files.SearchOptions.order_by: Specified property of the order of
        search results. By default, results are sorted by relevance.
    :ivar files.SearchOptions.file_status: Restricts search to the given file
        status.
    :ivar files.SearchOptions.filename_only: Restricts search to only match on
        filenames.
    :ivar files.SearchOptions.file_extensions: Restricts search to only the
        extensions specified. Only supported for active file search.
    :ivar files.SearchOptions.file_categories: Restricts search to only the file
        categories specified. Only supported for active file search.
    :ivar files.SearchOptions.account_id: Restricts results to the given account
        '_order_by_value',
        '_file_status_value',
        '_filename_only_value',
        '_file_extensions_value',
        '_file_categories_value',
        '_account_id_value',
                 order_by=None,
                 file_status=None,
                 filename_only=None,
                 file_extensions=None,
                 file_categories=None,
                 account_id=None):
        self._order_by_value = bb.NOT_SET
        self._file_status_value = bb.NOT_SET
        self._filename_only_value = bb.NOT_SET
        self._file_extensions_value = bb.NOT_SET
        self._file_categories_value = bb.NOT_SET
        self._account_id_value = bb.NOT_SET
        if order_by is not None:
            self.order_by = order_by
        if file_status is not None:
            self.file_status = file_status
        if filename_only is not None:
            self.filename_only = filename_only
        if file_extensions is not None:
            self.file_extensions = file_extensions
        if file_categories is not None:
            self.file_categories = file_categories
        if account_id is not None:
            self.account_id = account_id
    # Instance attribute type: SearchOrderBy (validator is set below)
    order_by = bb.Attribute("order_by", nullable=True, user_defined=True)
    # Instance attribute type: FileStatus (validator is set below)
    file_status = bb.Attribute("file_status", user_defined=True)
    filename_only = bb.Attribute("filename_only")
    file_extensions = bb.Attribute("file_extensions", nullable=True)
    # Instance attribute type: list of [FileCategory] (validator is set below)
    file_categories = bb.Attribute("file_categories", nullable=True)
    account_id = bb.Attribute("account_id", nullable=True)
        super(SearchOptions, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchOptions_validator = bv.Struct(SearchOptions)
class SearchOrderBy(bb.Union):
    relevance = None
    last_modified_time = None
    def is_relevance(self):
        Check if the union tag is ``relevance``.
        return self._tag == 'relevance'
    def is_last_modified_time(self):
        Check if the union tag is ``last_modified_time``.
        return self._tag == 'last_modified_time'
        super(SearchOrderBy, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchOrderBy_validator = bv.Union(SearchOrderBy)
class SearchResult(bb.Struct):
    :ivar files.SearchResult.matches: A list (possibly empty) of matches for the
    :ivar files.SearchResult.more: Used for paging. If true, indicates there is
        another page of results available that can be fetched by calling
        :meth:`dropbox.dropbox_client.Dropbox.files_search` again.
    :ivar files.SearchResult.start: Used for paging. Value to set the start
        argument to when calling
        :meth:`dropbox.dropbox_client.Dropbox.files_search` to fetch the next
        '_more_value',
                 more=None,
                 start=None):
        self._more_value = bb.NOT_SET
        if more is not None:
            self.more = more
    # Instance attribute type: list of [SearchMatch] (validator is set below)
    more = bb.Attribute("more")
        super(SearchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchResult_validator = bv.Struct(SearchResult)
class SearchV2Arg(bb.Struct):
    :ivar files.SearchV2Arg.query: The string to search for. May match across
        multiple fields based on the request arguments.
    :ivar files.SearchV2Arg.options: Options for more targeted search results.
    :ivar files.SearchV2Arg.match_field_options: Options for search results
        match fields.
    :ivar files.SearchV2Arg.include_highlights: Deprecated and moved this option
        to SearchMatchFieldOptions.
        '_options_value',
        '_match_field_options_value',
        self._options_value = bb.NOT_SET
        self._match_field_options_value = bb.NOT_SET
        if options is not None:
            self.options = options
        if match_field_options is not None:
            self.match_field_options = match_field_options
    # Instance attribute type: SearchOptions (validator is set below)
    options = bb.Attribute("options", nullable=True, user_defined=True)
    # Instance attribute type: SearchMatchFieldOptions (validator is set below)
    match_field_options = bb.Attribute("match_field_options", nullable=True, user_defined=True)
    include_highlights = bb.Attribute("include_highlights", nullable=True)
        super(SearchV2Arg, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchV2Arg_validator = bv.Struct(SearchV2Arg)
class SearchV2ContinueArg(bb.Struct):
    :ivar files.SearchV2ContinueArg.cursor: The cursor returned by your last
        call to :meth:`dropbox.dropbox_client.Dropbox.files_search`. Used to
        fetch the next page of results.
        super(SearchV2ContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchV2ContinueArg_validator = bv.Struct(SearchV2ContinueArg)
class SearchV2Result(bb.Struct):
    :ivar files.SearchV2Result.matches: A list (possibly empty) of matches for
        the query.
    :ivar files.SearchV2Result.has_more: Used for paging. If true, indicates
        there is another page of results available that can be fetched by
        calling :meth:`dropbox.dropbox_client.Dropbox.files_search_continue`
        with the cursor.
    :ivar files.SearchV2Result.cursor: Pass the cursor into
        :meth:`dropbox.dropbox_client.Dropbox.files_search_continue` to fetch
                 has_more=None,
    # Instance attribute type: list of [SearchMatchV2] (validator is set below)
        super(SearchV2Result, self)._process_custom_annotations(annotation_type, field_path, processor)
SearchV2Result_validator = bv.Struct(SearchV2Result)
class SharedLink(bb.Struct):
    :ivar files.SharedLink.url: Shared link url.
    :ivar files.SharedLink.password: Password for the shared link.
        '_password_value',
                 password=None):
        self._password_value = bb.NOT_SET
            self.password = password
    password = bb.Attribute("password", nullable=True)
        super(SharedLink, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedLink_validator = bv.Struct(SharedLink)
class SharedLinkFileInfo(bb.Struct):
    :ivar files.SharedLinkFileInfo.url: The shared link corresponding to either
        a file or shared link to a folder. If it is for a folder shared link, we
        use the path param to determine for which file in the folder the view is
        for.
    :ivar files.SharedLinkFileInfo.path: The path corresponding to a file in a
        shared link to a folder. Required for shared links to folders.
    :ivar files.SharedLinkFileInfo.password: Password for the shared link.
        Required for password-protected shared links to files  unless it can be
        read from a cookie.
        super(SharedLinkFileInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedLinkFileInfo_validator = bv.Struct(SharedLinkFileInfo)
class SingleUserLock(bb.Struct):
    :ivar files.SingleUserLock.created: The time the lock was created.
    :ivar files.SingleUserLock.lock_holder_account_id: The account ID of the
    :ivar files.SingleUserLock.lock_holder_team_id: The id of the team of the
        account holder if it exists.
        '_lock_holder_account_id_value',
        '_lock_holder_team_id_value',
                 lock_holder_account_id=None,
                 lock_holder_team_id=None):
        self._lock_holder_account_id_value = bb.NOT_SET
        self._lock_holder_team_id_value = bb.NOT_SET
        if lock_holder_account_id is not None:
            self.lock_holder_account_id = lock_holder_account_id
        if lock_holder_team_id is not None:
            self.lock_holder_team_id = lock_holder_team_id
    lock_holder_account_id = bb.Attribute("lock_holder_account_id")
    lock_holder_team_id = bb.Attribute("lock_holder_team_id", nullable=True)
        super(SingleUserLock, self)._process_custom_annotations(annotation_type, field_path, processor)
SingleUserLock_validator = bv.Struct(SingleUserLock)
class SymlinkInfo(bb.Struct):
    :ivar files.SymlinkInfo.target: The target this symlink points to.
        '_target_value',
                 target=None):
        self._target_value = bb.NOT_SET
        if target is not None:
            self.target = target
    target = bb.Attribute("target")
        super(SymlinkInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
SymlinkInfo_validator = bv.Struct(SymlinkInfo)
class SyncSetting(bb.Union):
    :ivar files.SyncSetting.default: On first sync to members' computers, the
        specified folder will follow its parent folder's setting or otherwise
        follow default sync behavior.
    :ivar files.SyncSetting.not_synced: On first sync to members' computers, the
        specified folder will be set to not sync with selective sync.
    :ivar files.SyncSetting.not_synced_inactive: The specified folder's
        not_synced setting is inactive due to its location or other
        configuration changes. It will follow its parent folder's setting.
    default = None
    not_synced = None
    not_synced_inactive = None
    def is_default(self):
        Check if the union tag is ``default``.
        return self._tag == 'default'
    def is_not_synced(self):
        Check if the union tag is ``not_synced``.
        return self._tag == 'not_synced'
    def is_not_synced_inactive(self):
        Check if the union tag is ``not_synced_inactive``.
        return self._tag == 'not_synced_inactive'
        super(SyncSetting, self)._process_custom_annotations(annotation_type, field_path, processor)
SyncSetting_validator = bv.Union(SyncSetting)
class SyncSettingArg(bb.Union):
    :ivar files.SyncSettingArg.default: On first sync to members' computers, the
    :ivar files.SyncSettingArg.not_synced: On first sync to members' computers,
        the specified folder will be set to not sync with selective sync.
        super(SyncSettingArg, self)._process_custom_annotations(annotation_type, field_path, processor)
SyncSettingArg_validator = bv.Union(SyncSettingArg)
class SyncSettingsError(bb.Union):
    :ivar files.SyncSettingsError.unsupported_combination: Setting this
        combination of sync settings simultaneously is not supported.
    :ivar files.SyncSettingsError.unsupported_configuration: The specified
        configuration is not supported.
    unsupported_combination = None
    unsupported_configuration = None
        :rtype: SyncSettingsError
    def is_unsupported_combination(self):
        Check if the union tag is ``unsupported_combination``.
        return self._tag == 'unsupported_combination'
    def is_unsupported_configuration(self):
        Check if the union tag is ``unsupported_configuration``.
        return self._tag == 'unsupported_configuration'
        super(SyncSettingsError, self)._process_custom_annotations(annotation_type, field_path, processor)
SyncSettingsError_validator = bv.Union(SyncSettingsError)
class Tag(bb.Union):
    Tag that can be added in multiple ways.
    :ivar UserGeneratedTag Tag.user_generated_tag: Tag generated by the user.
    def user_generated_tag(cls, val):
        Create an instance of this class set to the ``user_generated_tag`` tag
        :param UserGeneratedTag val:
        :rtype: Tag
        return cls('user_generated_tag', val)
    def is_user_generated_tag(self):
        Check if the union tag is ``user_generated_tag``.
        return self._tag == 'user_generated_tag'
    def get_user_generated_tag(self):
        Tag generated by the user.
        Only call this if :meth:`is_user_generated_tag` is true.
        :rtype: UserGeneratedTag
        if not self.is_user_generated_tag():
            raise AttributeError("tag 'user_generated_tag' not set")
        super(Tag, self)._process_custom_annotations(annotation_type, field_path, processor)
Tag_validator = bv.Union(Tag)
class ThumbnailArg(bb.Struct):
    :ivar files.ThumbnailArg.path: The path to the image file you want to
    :ivar files.ThumbnailArg.format: The format for the thumbnail image, jpeg
        (default) or png. For  images that are photos, jpeg should be preferred,
        while png is  better for screenshots and digital arts.
    :ivar files.ThumbnailArg.size: The size for the thumbnail image.
    :ivar files.ThumbnailArg.mode: How to resize and crop the image to achieve
        the desired size.
        '_format_value',
                 format=None,
        self._format_value = bb.NOT_SET
        if format is not None:
    # Instance attribute type: ThumbnailFormat (validator is set below)
    format = bb.Attribute("format", user_defined=True)
    # Instance attribute type: ThumbnailSize (validator is set below)
    size = bb.Attribute("size", user_defined=True)
    # Instance attribute type: ThumbnailMode (validator is set below)
        super(ThumbnailArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ThumbnailArg_validator = bv.Struct(ThumbnailArg)
class ThumbnailError(bb.Union):
    :ivar LookupError ThumbnailError.path: An error occurs when downloading
        metadata for the image.
    :ivar files.ThumbnailError.unsupported_extension: The file extension doesn't
        allow conversion to a thumbnail.
    :ivar files.ThumbnailError.unsupported_image: The image cannot be converted
        to a thumbnail.
    :ivar files.ThumbnailError.conversion_error: An error occurs during
        thumbnail conversion.
    unsupported_image = None
    conversion_error = None
    def is_unsupported_image(self):
        Check if the union tag is ``unsupported_image``.
        return self._tag == 'unsupported_image'
    def is_conversion_error(self):
        Check if the union tag is ``conversion_error``.
        return self._tag == 'conversion_error'
        An error occurs when downloading metadata for the image.
        super(ThumbnailError, self)._process_custom_annotations(annotation_type, field_path, processor)
ThumbnailError_validator = bv.Union(ThumbnailError)
class ThumbnailFormat(bb.Union):
    jpeg = None
    png = None
    def is_jpeg(self):
        Check if the union tag is ``jpeg``.
        return self._tag == 'jpeg'
    def is_png(self):
        Check if the union tag is ``png``.
        return self._tag == 'png'
        super(ThumbnailFormat, self)._process_custom_annotations(annotation_type, field_path, processor)
ThumbnailFormat_validator = bv.Union(ThumbnailFormat)
class ThumbnailMode(bb.Union):
    :ivar files.ThumbnailMode.strict: Scale down the image to fit within the
        given size.
    :ivar files.ThumbnailMode.bestfit: Scale down the image to fit within the
        given size or its transpose.
    :ivar files.ThumbnailMode.fitone_bestfit: Scale down the image to completely
        cover the given size or its transpose.
    strict = None
    bestfit = None
    fitone_bestfit = None
    def is_strict(self):
        Check if the union tag is ``strict``.
        return self._tag == 'strict'
    def is_bestfit(self):
        Check if the union tag is ``bestfit``.
        return self._tag == 'bestfit'
    def is_fitone_bestfit(self):
        Check if the union tag is ``fitone_bestfit``.
        return self._tag == 'fitone_bestfit'
        super(ThumbnailMode, self)._process_custom_annotations(annotation_type, field_path, processor)
ThumbnailMode_validator = bv.Union(ThumbnailMode)
class ThumbnailSize(bb.Union):
    :ivar files.ThumbnailSize.w32h32: 32 by 32 px.
    :ivar files.ThumbnailSize.w64h64: 64 by 64 px.
    :ivar files.ThumbnailSize.w128h128: 128 by 128 px.
    :ivar files.ThumbnailSize.w256h256: 256 by 256 px.
    :ivar files.ThumbnailSize.w480h320: 480 by 320 px.
    :ivar files.ThumbnailSize.w640h480: 640 by 480 px.
    :ivar files.ThumbnailSize.w960h640: 960 by 640 px.
    :ivar files.ThumbnailSize.w1024h768: 1024 by 768 px.
    :ivar files.ThumbnailSize.w2048h1536: 2048 by 1536 px.
    w32h32 = None
    w64h64 = None
    w128h128 = None
    w256h256 = None
    w480h320 = None
    w640h480 = None
    w960h640 = None
    w1024h768 = None
    w2048h1536 = None
    def is_w32h32(self):
        Check if the union tag is ``w32h32``.
        return self._tag == 'w32h32'
    def is_w64h64(self):
        Check if the union tag is ``w64h64``.
        return self._tag == 'w64h64'
    def is_w128h128(self):
        Check if the union tag is ``w128h128``.
        return self._tag == 'w128h128'
    def is_w256h256(self):
        Check if the union tag is ``w256h256``.
        return self._tag == 'w256h256'
    def is_w480h320(self):
        Check if the union tag is ``w480h320``.
        return self._tag == 'w480h320'
    def is_w640h480(self):
        Check if the union tag is ``w640h480``.
        return self._tag == 'w640h480'
    def is_w960h640(self):
        Check if the union tag is ``w960h640``.
        return self._tag == 'w960h640'
    def is_w1024h768(self):
        Check if the union tag is ``w1024h768``.
        return self._tag == 'w1024h768'
    def is_w2048h1536(self):
        Check if the union tag is ``w2048h1536``.
        return self._tag == 'w2048h1536'
        super(ThumbnailSize, self)._process_custom_annotations(annotation_type, field_path, processor)
ThumbnailSize_validator = bv.Union(ThumbnailSize)
class ThumbnailV2Arg(bb.Struct):
    :ivar files.ThumbnailV2Arg.resource: Information specifying which file to
        preview. This could be a path to a file, a shared link pointing to a
        file, or a shared link pointing to a folder, with a relative path.
    :ivar files.ThumbnailV2Arg.format: The format for the thumbnail image, jpeg
    :ivar files.ThumbnailV2Arg.size: The size for the thumbnail image.
    :ivar files.ThumbnailV2Arg.mode: How to resize and crop the image to achieve
        '_resource_value',
                 resource=None,
        self._resource_value = bb.NOT_SET
        if resource is not None:
            self.resource = resource
    # Instance attribute type: PathOrLink (validator is set below)
    resource = bb.Attribute("resource", user_defined=True)
        super(ThumbnailV2Arg, self)._process_custom_annotations(annotation_type, field_path, processor)
ThumbnailV2Arg_validator = bv.Struct(ThumbnailV2Arg)
class ThumbnailV2Error(bb.Union):
    :ivar LookupError ThumbnailV2Error.path: An error occurred when downloading
    :ivar files.ThumbnailV2Error.unsupported_extension: The file extension
        doesn't allow conversion to a thumbnail.
    :ivar files.ThumbnailV2Error.unsupported_image: The image cannot be
        converted to a thumbnail.
    :ivar files.ThumbnailV2Error.conversion_error: An error occurred during
    :ivar files.ThumbnailV2Error.access_denied: Access to this shared link is
        forbidden.
    :ivar files.ThumbnailV2Error.not_found: The shared link does not exist.
    access_denied = None
        :rtype: ThumbnailV2Error
    def is_access_denied(self):
        Check if the union tag is ``access_denied``.
        return self._tag == 'access_denied'
        An error occurred when downloading metadata for the image.
        super(ThumbnailV2Error, self)._process_custom_annotations(annotation_type, field_path, processor)
ThumbnailV2Error_validator = bv.Union(ThumbnailV2Error)
class UnlockFileArg(bb.Struct):
    :ivar files.UnlockFileArg.path: Path in the user's Dropbox to a file.
        super(UnlockFileArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UnlockFileArg_validator = bv.Struct(UnlockFileArg)
class UnlockFileBatchArg(bb.Struct):
    :ivar files.UnlockFileBatchArg.entries: List of 'entries'. Each 'entry'
        contains a path of the file which will be unlocked. Duplicate path
        arguments in the batch are considered only once.
    # Instance attribute type: list of [UnlockFileArg] (validator is set below)
        super(UnlockFileBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UnlockFileBatchArg_validator = bv.Struct(UnlockFileBatchArg)
class UploadArg(CommitInfo):
    :ivar files.UploadArg.content_hash: A hash of the file content uploaded in
        this call. If provided and the uploaded content does not match this
        hash, an error will be returned. For more information see our `Content
        hash <https://www.dropbox.com/developers/reference/content-hash>`_ page.
                 strict_conflict=None,
        super(UploadArg, self).__init__(path,
                                        strict_conflict)
        super(UploadArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadArg_validator = bv.Struct(UploadArg)
class UploadError(bb.Union):
    :ivar UploadWriteFailed UploadError.path: Unable to save the uploaded
        contents to a file.
    :ivar InvalidPropertyGroupError UploadError.properties_error: The supplied
        property group is invalid. The file has uploaded without property
    :ivar files.UploadError.payload_too_large: The request payload must be at
        most 150 MB.
    :ivar files.UploadError.content_hash_mismatch: The content received by the
        Dropbox server in this call does not match the provided content hash.
    payload_too_large = None
    content_hash_mismatch = None
        :param UploadWriteFailed val:
        :rtype: UploadError
        :param file_properties.InvalidPropertyGroupError val:
    def is_payload_too_large(self):
        Check if the union tag is ``payload_too_large``.
        return self._tag == 'payload_too_large'
    def is_content_hash_mismatch(self):
        Check if the union tag is ``content_hash_mismatch``.
        return self._tag == 'content_hash_mismatch'
        Unable to save the uploaded contents to a file.
        :rtype: UploadWriteFailed
        The supplied property group is invalid. The file has uploaded without
        property groups.
        :rtype: file_properties.InvalidPropertyGroupError
        super(UploadError, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadError_validator = bv.Union(UploadError)
class UploadSessionAppendArg(bb.Struct):
    :ivar files.UploadSessionAppendArg.cursor: Contains the upload session ID
        and the offset.
    :ivar files.UploadSessionAppendArg.close: If true, the current session will
        be closed, at which point you won't be able to call
        :meth:`dropbox.dropbox_client.Dropbox.files_upload_session_append`
        anymore with the current session.
    :ivar files.UploadSessionAppendArg.content_hash: A hash of the file content
        uploaded in this call. If provided and the uploaded content does not
        match this hash, an error will be returned. For more information see our
        '_close_value',
                 close=None,
        self._close_value = bb.NOT_SET
        if close is not None:
            self.close = close
    # Instance attribute type: UploadSessionCursor (validator is set below)
    cursor = bb.Attribute("cursor", user_defined=True)
    close = bb.Attribute("close")
        super(UploadSessionAppendArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionAppendArg_validator = bv.Struct(UploadSessionAppendArg)
class UploadSessionLookupError(bb.Union):
    :ivar files.UploadSessionLookupError.not_found: The upload session ID was
        not found or has expired. Upload sessions are valid for 7 days.
    :ivar UploadSessionOffsetError UploadSessionLookupError.incorrect_offset:
        The specified offset was incorrect. See the value for the correct
        offset. This error may occur when a previous request was received and
        processed successfully but the client did not receive the response, e.g.
        due to a network error.
    :ivar files.UploadSessionLookupError.closed: You are attempting to append
        data to an upload session that has already been closed (i.e. committed).
    :ivar files.UploadSessionLookupError.not_closed: The session must be closed
        before calling upload_session/finish_batch.
    :ivar files.UploadSessionLookupError.too_large: You can not append to the
        upload session because the size of a file should not reach the max file
        size limit (i.e. 350GB).
    :ivar files.UploadSessionLookupError.concurrent_session_invalid_offset: For
        concurrent upload sessions, offset needs to be multiple of 4194304
    :ivar files.UploadSessionLookupError.concurrent_session_invalid_data_size:
        For concurrent upload sessions, only chunks with size multiple of
        4194304 bytes can be uploaded.
    :ivar files.UploadSessionLookupError.payload_too_large: The request payload
        must be at most 150 MB.
    closed = None
    not_closed = None
    concurrent_session_invalid_offset = None
    concurrent_session_invalid_data_size = None
    def incorrect_offset(cls, val):
        Create an instance of this class set to the ``incorrect_offset`` tag
        :param UploadSessionOffsetError val:
        :rtype: UploadSessionLookupError
        return cls('incorrect_offset', val)
    def is_incorrect_offset(self):
        Check if the union tag is ``incorrect_offset``.
        return self._tag == 'incorrect_offset'
    def is_closed(self):
        Check if the union tag is ``closed``.
        return self._tag == 'closed'
    def is_not_closed(self):
        Check if the union tag is ``not_closed``.
        return self._tag == 'not_closed'
    def is_concurrent_session_invalid_offset(self):
        Check if the union tag is ``concurrent_session_invalid_offset``.
        return self._tag == 'concurrent_session_invalid_offset'
    def is_concurrent_session_invalid_data_size(self):
        Check if the union tag is ``concurrent_session_invalid_data_size``.
        return self._tag == 'concurrent_session_invalid_data_size'
    def get_incorrect_offset(self):
        Only call this if :meth:`is_incorrect_offset` is true.
        :rtype: UploadSessionOffsetError
        if not self.is_incorrect_offset():
            raise AttributeError("tag 'incorrect_offset' not set")
        super(UploadSessionLookupError, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionLookupError_validator = bv.Union(UploadSessionLookupError)
class UploadSessionAppendError(UploadSessionLookupError):
    :ivar files.UploadSessionAppendError.content_hash_mismatch: The content
        received by the Dropbox server in this call does not match the provided
        content hash.
        super(UploadSessionAppendError, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionAppendError_validator = bv.Union(UploadSessionAppendError)
class UploadSessionCursor(bb.Struct):
    :ivar files.UploadSessionCursor.session_id: The upload session ID (returned
        by :meth:`dropbox.dropbox_client.Dropbox.files_upload_session_start`).
    :ivar files.UploadSessionCursor.offset: Offset in bytes at which data should
        be appended. We use this to make sure upload data isn't lost or
        duplicated in the event of a network error.
        '_session_id_value',
        '_offset_value',
                 session_id=None,
                 offset=None):
        self._session_id_value = bb.NOT_SET
        self._offset_value = bb.NOT_SET
        if session_id is not None:
            self.session_id = session_id
        if offset is not None:
            self.offset = offset
    session_id = bb.Attribute("session_id")
    offset = bb.Attribute("offset")
        super(UploadSessionCursor, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionCursor_validator = bv.Struct(UploadSessionCursor)
class UploadSessionFinishArg(bb.Struct):
    :ivar files.UploadSessionFinishArg.cursor: Contains the upload session ID
    :ivar files.UploadSessionFinishArg.commit: Contains the path and other
        optional modifiers for the commit.
    :ivar files.UploadSessionFinishArg.content_hash: A hash of the file content
        '_commit_value',
                 commit=None,
        self._commit_value = bb.NOT_SET
        if commit is not None:
            self.commit = commit
    commit = bb.Attribute("commit", user_defined=True)
        super(UploadSessionFinishArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionFinishArg_validator = bv.Struct(UploadSessionFinishArg)
class UploadSessionFinishBatchArg(bb.Struct):
    :ivar files.UploadSessionFinishBatchArg.entries: Commit information for each
        file in the batch.
    # Instance attribute type: list of [UploadSessionFinishArg] (validator is set below)
        super(UploadSessionFinishBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionFinishBatchArg_validator = bv.Struct(UploadSessionFinishBatchArg)
class UploadSessionFinishBatchJobStatus(async_.PollResultBase):
    :ivar UploadSessionFinishBatchResult
        UploadSessionFinishBatchJobStatus.complete: The
        :meth:`dropbox.dropbox_client.Dropbox.files_upload_session_finish_batch`
        has finished.
        :param UploadSessionFinishBatchResult val:
        :rtype: UploadSessionFinishBatchJobStatus
        :rtype: UploadSessionFinishBatchResult
        super(UploadSessionFinishBatchJobStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionFinishBatchJobStatus_validator = bv.Union(UploadSessionFinishBatchJobStatus)
class UploadSessionFinishBatchLaunch(async_.LaunchResultBase):
        :rtype: UploadSessionFinishBatchLaunch
        super(UploadSessionFinishBatchLaunch, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionFinishBatchLaunch_validator = bv.Union(UploadSessionFinishBatchLaunch)
class UploadSessionFinishBatchResult(bb.Struct):
    :ivar files.UploadSessionFinishBatchResult.entries: Each entry in
        ``UploadSessionFinishBatchArg.entries`` will appear at the same position
        inside ``UploadSessionFinishBatchResult.entries``.
    # Instance attribute type: list of [UploadSessionFinishBatchResultEntry] (validator is set below)
        super(UploadSessionFinishBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionFinishBatchResult_validator = bv.Struct(UploadSessionFinishBatchResult)
class UploadSessionFinishBatchResultEntry(bb.Union):
        :rtype: UploadSessionFinishBatchResultEntry
        :param UploadSessionFinishError val:
        :rtype: UploadSessionFinishError
        super(UploadSessionFinishBatchResultEntry, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionFinishBatchResultEntry_validator = bv.Union(UploadSessionFinishBatchResultEntry)
class UploadSessionFinishError(bb.Union):
    :ivar UploadSessionLookupError UploadSessionFinishError.lookup_failed: The
        session arguments are incorrect; the value explains the reason.
    :ivar WriteError UploadSessionFinishError.path: Unable to save the uploaded
        contents to a file. Data has already been appended to the upload
        session. Please retry with empty data body and updated offset.
    :ivar InvalidPropertyGroupError UploadSessionFinishError.properties_error:
    :ivar files.UploadSessionFinishError.too_many_shared_folder_targets: The
        batch request commits files into too many different shared folders.
        Please limit your batch request to files contained in a single shared
    :ivar files.UploadSessionFinishError.too_many_write_operations: There are
        too many write operations happening in the user's Dropbox. You should
        retry uploading this file.
    :ivar files.UploadSessionFinishError.concurrent_session_data_not_allowed:
        Uploading data not allowed when finishing concurrent upload session.
    :ivar files.UploadSessionFinishError.concurrent_session_not_closed:
        Concurrent upload sessions need to be closed before finishing.
    :ivar files.UploadSessionFinishError.concurrent_session_missing_data: Not
        all pieces of data were uploaded before trying to finish the session.
    :ivar files.UploadSessionFinishError.payload_too_large: The request payload
    :ivar files.UploadSessionFinishError.content_hash_mismatch: The content
    too_many_shared_folder_targets = None
    concurrent_session_data_not_allowed = None
    concurrent_session_not_closed = None
    concurrent_session_missing_data = None
    def lookup_failed(cls, val):
        Create an instance of this class set to the ``lookup_failed`` tag with
        :param UploadSessionLookupError val:
        return cls('lookup_failed', val)
    def is_lookup_failed(self):
        Check if the union tag is ``lookup_failed``.
        return self._tag == 'lookup_failed'
    def is_too_many_shared_folder_targets(self):
        Check if the union tag is ``too_many_shared_folder_targets``.
        return self._tag == 'too_many_shared_folder_targets'
    def is_concurrent_session_data_not_allowed(self):
        Check if the union tag is ``concurrent_session_data_not_allowed``.
        return self._tag == 'concurrent_session_data_not_allowed'
    def is_concurrent_session_not_closed(self):
        Check if the union tag is ``concurrent_session_not_closed``.
        return self._tag == 'concurrent_session_not_closed'
    def is_concurrent_session_missing_data(self):
        Check if the union tag is ``concurrent_session_missing_data``.
        return self._tag == 'concurrent_session_missing_data'
    def get_lookup_failed(self):
        The session arguments are incorrect; the value explains the reason.
        Only call this if :meth:`is_lookup_failed` is true.
        if not self.is_lookup_failed():
            raise AttributeError("tag 'lookup_failed' not set")
        Unable to save the uploaded contents to a file. Data has already been
        appended to the upload session. Please retry with empty data body and
        updated offset.
        super(UploadSessionFinishError, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionFinishError_validator = bv.Union(UploadSessionFinishError)
class UploadSessionOffsetError(bb.Struct):
    :ivar files.UploadSessionOffsetError.correct_offset: The offset up to which
        data has been collected.
        '_correct_offset_value',
                 correct_offset=None):
        self._correct_offset_value = bb.NOT_SET
        if correct_offset is not None:
            self.correct_offset = correct_offset
    correct_offset = bb.Attribute("correct_offset")
        super(UploadSessionOffsetError, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionOffsetError_validator = bv.Struct(UploadSessionOffsetError)
class UploadSessionStartArg(bb.Struct):
    :ivar files.UploadSessionStartArg.close: If true, the current session will
    :ivar files.UploadSessionStartArg.session_type: Type of upload session you
        want to start. If not specified, default is
        ``UploadSessionType.sequential``.
    :ivar files.UploadSessionStartArg.content_hash: A hash of the file content
        '_session_type_value',
        self._session_type_value = bb.NOT_SET
        if session_type is not None:
            self.session_type = session_type
    # Instance attribute type: UploadSessionType (validator is set below)
    session_type = bb.Attribute("session_type", nullable=True, user_defined=True)
        super(UploadSessionStartArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionStartArg_validator = bv.Struct(UploadSessionStartArg)
class UploadSessionStartBatchArg(bb.Struct):
    :ivar files.UploadSessionStartBatchArg.session_type: Type of upload session
        you want to start. If not specified, default is
    :ivar files.UploadSessionStartBatchArg.num_sessions: The number of upload
        sessions to start.
        '_num_sessions_value',
                 num_sessions=None,
        self._num_sessions_value = bb.NOT_SET
        if num_sessions is not None:
            self.num_sessions = num_sessions
    num_sessions = bb.Attribute("num_sessions")
        super(UploadSessionStartBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionStartBatchArg_validator = bv.Struct(UploadSessionStartBatchArg)
class UploadSessionStartBatchResult(bb.Struct):
    :ivar files.UploadSessionStartBatchResult.session_ids: A List of unique
        identifiers for the upload session. Pass each session_id to
        :meth:`dropbox.dropbox_client.Dropbox.files_upload_session_append` and
        :meth:`dropbox.dropbox_client.Dropbox.files_upload_session_finish`.
        '_session_ids_value',
                 session_ids=None):
        self._session_ids_value = bb.NOT_SET
        if session_ids is not None:
            self.session_ids = session_ids
    session_ids = bb.Attribute("session_ids")
        super(UploadSessionStartBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionStartBatchResult_validator = bv.Struct(UploadSessionStartBatchResult)
class UploadSessionStartError(bb.Union):
    :ivar files.UploadSessionStartError.concurrent_session_data_not_allowed:
        Uploading data not allowed when starting concurrent upload session.
    :ivar files.UploadSessionStartError.concurrent_session_close_not_allowed:
        Can not start a closed concurrent upload session.
    :ivar files.UploadSessionStartError.payload_too_large: The request payload
    :ivar files.UploadSessionStartError.content_hash_mismatch: The content
    concurrent_session_close_not_allowed = None
    def is_concurrent_session_close_not_allowed(self):
        Check if the union tag is ``concurrent_session_close_not_allowed``.
        return self._tag == 'concurrent_session_close_not_allowed'
        super(UploadSessionStartError, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionStartError_validator = bv.Union(UploadSessionStartError)
class UploadSessionStartResult(bb.Struct):
    :ivar files.UploadSessionStartResult.session_id: A unique identifier for the
        upload session. Pass this to
                 session_id=None):
        super(UploadSessionStartResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionStartResult_validator = bv.Struct(UploadSessionStartResult)
class UploadSessionType(bb.Union):
    :ivar files.UploadSessionType.sequential: Pieces of data are uploaded
        sequentially one after another. This is the default behavior.
    :ivar files.UploadSessionType.concurrent: Pieces of data can be uploaded in
        concurrent RPCs in any order.
    sequential = None
    concurrent = None
    def is_sequential(self):
        Check if the union tag is ``sequential``.
        return self._tag == 'sequential'
    def is_concurrent(self):
        Check if the union tag is ``concurrent``.
        return self._tag == 'concurrent'
        super(UploadSessionType, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadSessionType_validator = bv.Union(UploadSessionType)
class UploadWriteFailed(bb.Struct):
    :ivar files.UploadWriteFailed.reason: The reason why the file couldn't be
        saved.
    :ivar files.UploadWriteFailed.upload_session_id: The upload session ID; data
        has already been uploaded to the corresponding upload session and this
        ID may be used to retry the commit with
        '_upload_session_id_value',
                 upload_session_id=None):
        self._upload_session_id_value = bb.NOT_SET
        if upload_session_id is not None:
            self.upload_session_id = upload_session_id
    # Instance attribute type: WriteError (validator is set below)
    upload_session_id = bb.Attribute("upload_session_id")
        super(UploadWriteFailed, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadWriteFailed_validator = bv.Struct(UploadWriteFailed)
class UserGeneratedTag(bb.Struct):
        super(UserGeneratedTag, self)._process_custom_annotations(annotation_type, field_path, processor)
UserGeneratedTag_validator = bv.Struct(UserGeneratedTag)
class VideoMetadata(MediaMetadata):
    Metadata for a video.
    :ivar files.VideoMetadata.duration: The duration of the video in
        milliseconds.
                 time_taken=None,
        super(VideoMetadata, self).__init__(dimensions,
    duration = bb.Attribute("duration", nullable=True)
        super(VideoMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
VideoMetadata_validator = bv.Struct(VideoMetadata)
class WriteConflictError(bb.Union):
    :ivar files.WriteConflictError.file: There's a file in the way.
    :ivar files.WriteConflictError.folder: There's a folder in the way.
    :ivar files.WriteConflictError.file_ancestor: There's a file at an ancestor
        path, so we couldn't create the required parent folders.
    file = None
    file_ancestor = None
    def is_file(self):
        Check if the union tag is ``file``.
        return self._tag == 'file'
    def is_file_ancestor(self):
        Check if the union tag is ``file_ancestor``.
        return self._tag == 'file_ancestor'
        super(WriteConflictError, self)._process_custom_annotations(annotation_type, field_path, processor)
WriteConflictError_validator = bv.Union(WriteConflictError)
class WriteError(bb.Union):
    :ivar Optional[str] files.WriteError.malformed_path: The given path does not
        satisfy the required path format. Please refer to the :link:`Path
    :ivar WriteConflictError WriteError.conflict: Couldn't write to the target
        path because there was something in the way.
    :ivar files.WriteError.no_write_permission: The user doesn't have
        permissions to write to the target location.
    :ivar files.WriteError.insufficient_space: The user doesn't have enough
        available space (bytes) to write more data.
    :ivar files.WriteError.disallowed_name: Dropbox will not save the file or
        folder because of its name.
    :ivar files.WriteError.team_folder: This endpoint cannot move or delete team
    :ivar files.WriteError.operation_suppressed: This file operation is not
        allowed at this path.
    :ivar files.WriteError.too_many_write_operations: There are too many write
    insufficient_space = None
    disallowed_name = None
    team_folder = None
    operation_suppressed = None
    def conflict(cls, val):
        Create an instance of this class set to the ``conflict`` tag with value
        :param WriteConflictError val:
        return cls('conflict', val)
    def is_conflict(self):
        Check if the union tag is ``conflict``.
        return self._tag == 'conflict'
    def is_insufficient_space(self):
        Check if the union tag is ``insufficient_space``.
        return self._tag == 'insufficient_space'
    def is_disallowed_name(self):
        Check if the union tag is ``disallowed_name``.
        return self._tag == 'disallowed_name'
    def is_team_folder(self):
        Check if the union tag is ``team_folder``.
        return self._tag == 'team_folder'
    def is_operation_suppressed(self):
        Check if the union tag is ``operation_suppressed``.
        return self._tag == 'operation_suppressed'
    def get_conflict(self):
        Couldn't write to the target path because there was something in the
        way.
        Only call this if :meth:`is_conflict` is true.
        :rtype: WriteConflictError
        if not self.is_conflict():
            raise AttributeError("tag 'conflict' not set")
        super(WriteError, self)._process_custom_annotations(annotation_type, field_path, processor)
WriteError_validator = bv.Union(WriteError)
class WriteMode(bb.Union):
    Your intent when writing a file to some path. This is used to determine what
    constitutes a conflict and what the autorename strategy is. In some
    situations, the conflict behavior is identical: (a) If the target path
    doesn't refer to anything, the file is always written; no conflict. (b) If
    the target path refers to a folder, it's always a conflict. (c) If the
    target path refers to a file with identical contents, nothing gets written;
    no conflict. The conflict checking differs in the case where there's a file
    at the target path with contents different from the contents you're trying
    to write.
    :ivar files.WriteMode.add: Do not overwrite an existing file if there is a
        conflict. The autorename strategy is to append a number to the file
        name. For example, "document.txt" might become "document (2).txt".
    :ivar files.WriteMode.overwrite: Always overwrite the existing file. The
        autorename strategy is the same as it is for ``add``.
    :ivar str files.WriteMode.update: Overwrite if the given "rev" matches the
        existing file's "rev". The supplied value should be the latest known
        "rev" of the file, for example, from :type:`FileMetadata`, from when the
        file was last downloaded by the app. This will cause the file on the
        Dropbox servers to be overwritten if the given "rev" matches the
        existing file's current "rev" on the Dropbox servers. The autorename
        strategy is to append the string "conflicted copy" to the file name. For
        example, "document.txt" might become "document (conflicted copy).txt" or
        "document (Panda's conflicted copy).txt".
    add = None
        :rtype: WriteMode
    def is_add(self):
        Check if the union tag is ``add``.
        return self._tag == 'add'
        Overwrite if the given "rev" matches the existing file's "rev". The
        supplied value should be the latest known "rev" of the file, for
        example, from :class:`FileMetadata`, from when the file was last
        downloaded by the app. This will cause the file on the Dropbox servers
        to be overwritten if the given "rev" matches the existing file's current
        "rev" on the Dropbox servers. The autorename strategy is to append the
        string "conflicted copy" to the file name. For example, "document.txt"
        might become "document (conflicted copy).txt" or "document (Panda's
        conflicted copy).txt".
        super(WriteMode, self)._process_custom_annotations(annotation_type, field_path, processor)
WriteMode_validator = bv.Union(WriteMode)
CopyBatchArg_validator = RelocationBatchArgBase_validator
CopyBatchArg = RelocationBatchArgBase
FileId_validator = bv.String(min_length=4, pattern='id:.+')
ListFolderCursor_validator = bv.String(min_length=1)
MalformedPathError_validator = bv.Nullable(bv.String())
Path_validator = bv.String(pattern='/(.|[\\r\\n])*')
PathR_validator = bv.String(pattern='(/(.|[\\r\\n])*)?|(ns:[0-9]+(/.*)?)')
PathROrId_validator = bv.String(pattern='(/(.|[\\r\\n])*)?|id:.*|(ns:[0-9]+(/.*)?)')
ReadPath_validator = bv.String(pattern='(/(.|[\\r\\n])*|id:.*)|(rev:[0-9a-f]{9,})|(ns:[0-9]+(/.*)?)')
Rev_validator = bv.String(min_length=9, pattern='[0-9a-f]+')
SearchV2Cursor_validator = bv.String(min_length=1)
Sha256HexHash_validator = bv.String(min_length=64, max_length=64)
SharedLinkUrl_validator = bv.String()
TagText_validator = bv.String(min_length=1, max_length=32, pattern='[\\w]+')
WritePath_validator = bv.String(pattern='(/(.|[\\r\\n])*)|(ns:[0-9]+(/.*)?)')
WritePathOrId_validator = bv.String(pattern='(/(.|[\\r\\n])*)|(ns:[0-9]+(/.*)?)|(id:.*)')
AddTagArg.path.validator = Path_validator
AddTagArg.tag_text.validator = TagText_validator
AddTagArg._all_field_names_ = set([
    'tag_text',
AddTagArg._all_fields_ = [
    ('path', AddTagArg.path.validator),
    ('tag_text', AddTagArg.tag_text.validator),
BaseTagError._path_validator = LookupError_validator
BaseTagError._other_validator = bv.Void()
BaseTagError._tagmap = {
    'path': BaseTagError._path_validator,
    'other': BaseTagError._other_validator,
BaseTagError.other = BaseTagError('other')
AddTagError._too_many_tags_validator = bv.Void()
AddTagError._tagmap = {
    'too_many_tags': AddTagError._too_many_tags_validator,
AddTagError._tagmap.update(BaseTagError._tagmap)
AddTagError.too_many_tags = AddTagError('too_many_tags')
GetMetadataArg.path.validator = ReadPath_validator
GetMetadataArg.include_media_info.validator = bv.Boolean()
GetMetadataArg.include_deleted.validator = bv.Boolean()
GetMetadataArg.include_has_explicit_shared_members.validator = bv.Boolean()
GetMetadataArg.include_property_groups.validator = bv.Nullable(file_properties.TemplateFilterBase_validator)
GetMetadataArg._all_field_names_ = set([
    'include_media_info',
    'include_deleted',
    'include_has_explicit_shared_members',
    'include_property_groups',
GetMetadataArg._all_fields_ = [
    ('path', GetMetadataArg.path.validator),
    ('include_media_info', GetMetadataArg.include_media_info.validator),
    ('include_deleted', GetMetadataArg.include_deleted.validator),
    ('include_has_explicit_shared_members', GetMetadataArg.include_has_explicit_shared_members.validator),
    ('include_property_groups', GetMetadataArg.include_property_groups.validator),
AlphaGetMetadataArg.include_property_templates.validator = bv.Nullable(bv.List(file_properties.TemplateId_validator))
AlphaGetMetadataArg._all_field_names_ = GetMetadataArg._all_field_names_.union(set(['include_property_templates']))
AlphaGetMetadataArg._all_fields_ = GetMetadataArg._all_fields_ + [('include_property_templates', AlphaGetMetadataArg.include_property_templates.validator)]
GetMetadataError._path_validator = LookupError_validator
GetMetadataError._tagmap = {
    'path': GetMetadataError._path_validator,
AlphaGetMetadataError._properties_error_validator = file_properties.LookUpPropertiesError_validator
AlphaGetMetadataError._tagmap = {
    'properties_error': AlphaGetMetadataError._properties_error_validator,
AlphaGetMetadataError._tagmap.update(GetMetadataError._tagmap)
CommitInfo.path.validator = WritePathOrId_validator
CommitInfo.mode.validator = WriteMode_validator
CommitInfo.autorename.validator = bv.Boolean()
CommitInfo.client_modified.validator = bv.Nullable(common.DropboxTimestamp_validator)
CommitInfo.mute.validator = bv.Boolean()
CommitInfo.property_groups.validator = bv.Nullable(bv.List(file_properties.PropertyGroup_validator))
CommitInfo.strict_conflict.validator = bv.Boolean()
CommitInfo._all_field_names_ = set([
    'autorename',
    'client_modified',
    'mute',
    'strict_conflict',
CommitInfo._all_fields_ = [
    ('path', CommitInfo.path.validator),
    ('mode', CommitInfo.mode.validator),
    ('autorename', CommitInfo.autorename.validator),
    ('client_modified', CommitInfo.client_modified.validator),
    ('mute', CommitInfo.mute.validator),
    ('property_groups', CommitInfo.property_groups.validator),
    ('strict_conflict', CommitInfo.strict_conflict.validator),
ContentSyncSetting.id.validator = FileId_validator
ContentSyncSetting.sync_setting.validator = SyncSetting_validator
ContentSyncSetting._all_field_names_ = set([
    'sync_setting',
ContentSyncSetting._all_fields_ = [
    ('id', ContentSyncSetting.id.validator),
    ('sync_setting', ContentSyncSetting.sync_setting.validator),
ContentSyncSettingArg.id.validator = FileId_validator
ContentSyncSettingArg.sync_setting.validator = SyncSettingArg_validator
ContentSyncSettingArg._all_field_names_ = set([
ContentSyncSettingArg._all_fields_ = [
    ('id', ContentSyncSettingArg.id.validator),
    ('sync_setting', ContentSyncSettingArg.sync_setting.validator),
CreateFolderArg.path.validator = WritePath_validator
CreateFolderArg.autorename.validator = bv.Boolean()
CreateFolderArg._all_field_names_ = set([
CreateFolderArg._all_fields_ = [
    ('path', CreateFolderArg.path.validator),
    ('autorename', CreateFolderArg.autorename.validator),
CreateFolderBatchArg.paths.validator = bv.List(WritePath_validator, max_items=10000)
CreateFolderBatchArg.autorename.validator = bv.Boolean()
CreateFolderBatchArg.force_async.validator = bv.Boolean()
CreateFolderBatchArg._all_field_names_ = set([
    'paths',
    'force_async',
CreateFolderBatchArg._all_fields_ = [
    ('paths', CreateFolderBatchArg.paths.validator),
    ('autorename', CreateFolderBatchArg.autorename.validator),
    ('force_async', CreateFolderBatchArg.force_async.validator),
CreateFolderBatchError._too_many_files_validator = bv.Void()
CreateFolderBatchError._other_validator = bv.Void()
CreateFolderBatchError._tagmap = {
    'too_many_files': CreateFolderBatchError._too_many_files_validator,
    'other': CreateFolderBatchError._other_validator,
CreateFolderBatchError.too_many_files = CreateFolderBatchError('too_many_files')
CreateFolderBatchError.other = CreateFolderBatchError('other')
CreateFolderBatchJobStatus._complete_validator = CreateFolderBatchResult_validator
CreateFolderBatchJobStatus._failed_validator = CreateFolderBatchError_validator
CreateFolderBatchJobStatus._other_validator = bv.Void()
CreateFolderBatchJobStatus._tagmap = {
    'complete': CreateFolderBatchJobStatus._complete_validator,
    'failed': CreateFolderBatchJobStatus._failed_validator,
    'other': CreateFolderBatchJobStatus._other_validator,
CreateFolderBatchJobStatus._tagmap.update(async_.PollResultBase._tagmap)
CreateFolderBatchJobStatus.other = CreateFolderBatchJobStatus('other')
CreateFolderBatchLaunch._complete_validator = CreateFolderBatchResult_validator
CreateFolderBatchLaunch._other_validator = bv.Void()
CreateFolderBatchLaunch._tagmap = {
    'complete': CreateFolderBatchLaunch._complete_validator,
    'other': CreateFolderBatchLaunch._other_validator,
CreateFolderBatchLaunch._tagmap.update(async_.LaunchResultBase._tagmap)
CreateFolderBatchLaunch.other = CreateFolderBatchLaunch('other')
FileOpsResult._all_field_names_ = set([])
FileOpsResult._all_fields_ = []
CreateFolderBatchResult.entries.validator = bv.List(CreateFolderBatchResultEntry_validator)
CreateFolderBatchResult._all_field_names_ = FileOpsResult._all_field_names_.union(set(['entries']))
CreateFolderBatchResult._all_fields_ = FileOpsResult._all_fields_ + [('entries', CreateFolderBatchResult.entries.validator)]
CreateFolderBatchResultEntry._success_validator = CreateFolderEntryResult_validator
CreateFolderBatchResultEntry._failure_validator = CreateFolderEntryError_validator
CreateFolderBatchResultEntry._tagmap = {
    'success': CreateFolderBatchResultEntry._success_validator,
    'failure': CreateFolderBatchResultEntry._failure_validator,
CreateFolderEntryError._path_validator = WriteError_validator
CreateFolderEntryError._other_validator = bv.Void()
CreateFolderEntryError._tagmap = {
    'path': CreateFolderEntryError._path_validator,
    'other': CreateFolderEntryError._other_validator,
CreateFolderEntryError.other = CreateFolderEntryError('other')
CreateFolderEntryResult.metadata.validator = FolderMetadata_validator
CreateFolderEntryResult._all_field_names_ = set(['metadata'])
CreateFolderEntryResult._all_fields_ = [('metadata', CreateFolderEntryResult.metadata.validator)]
CreateFolderError._path_validator = WriteError_validator
CreateFolderError._tagmap = {
    'path': CreateFolderError._path_validator,
CreateFolderResult.metadata.validator = FolderMetadata_validator
CreateFolderResult._all_field_names_ = FileOpsResult._all_field_names_.union(set(['metadata']))
CreateFolderResult._all_fields_ = FileOpsResult._all_fields_ + [('metadata', CreateFolderResult.metadata.validator)]
DeleteArg.path.validator = WritePathOrId_validator
DeleteArg.parent_rev.validator = bv.Nullable(Rev_validator)
DeleteArg._all_field_names_ = set([
    'parent_rev',
DeleteArg._all_fields_ = [
    ('path', DeleteArg.path.validator),
    ('parent_rev', DeleteArg.parent_rev.validator),
DeleteBatchArg.entries.validator = bv.List(DeleteArg_validator, max_items=1000)
DeleteBatchArg._all_field_names_ = set(['entries'])
DeleteBatchArg._all_fields_ = [('entries', DeleteBatchArg.entries.validator)]
DeleteBatchError._too_many_write_operations_validator = bv.Void()
DeleteBatchError._other_validator = bv.Void()
DeleteBatchError._tagmap = {
    'too_many_write_operations': DeleteBatchError._too_many_write_operations_validator,
    'other': DeleteBatchError._other_validator,
DeleteBatchError.too_many_write_operations = DeleteBatchError('too_many_write_operations')
DeleteBatchError.other = DeleteBatchError('other')
DeleteBatchJobStatus._complete_validator = DeleteBatchResult_validator
DeleteBatchJobStatus._failed_validator = DeleteBatchError_validator
DeleteBatchJobStatus._other_validator = bv.Void()
DeleteBatchJobStatus._tagmap = {
    'complete': DeleteBatchJobStatus._complete_validator,
    'failed': DeleteBatchJobStatus._failed_validator,
    'other': DeleteBatchJobStatus._other_validator,
DeleteBatchJobStatus._tagmap.update(async_.PollResultBase._tagmap)
DeleteBatchJobStatus.other = DeleteBatchJobStatus('other')
DeleteBatchLaunch._complete_validator = DeleteBatchResult_validator
DeleteBatchLaunch._other_validator = bv.Void()
DeleteBatchLaunch._tagmap = {
    'complete': DeleteBatchLaunch._complete_validator,
    'other': DeleteBatchLaunch._other_validator,
DeleteBatchLaunch._tagmap.update(async_.LaunchResultBase._tagmap)
DeleteBatchLaunch.other = DeleteBatchLaunch('other')
DeleteBatchResult.entries.validator = bv.List(DeleteBatchResultEntry_validator)
DeleteBatchResult._all_field_names_ = FileOpsResult._all_field_names_.union(set(['entries']))
DeleteBatchResult._all_fields_ = FileOpsResult._all_fields_ + [('entries', DeleteBatchResult.entries.validator)]
DeleteBatchResultData.metadata.validator = Metadata_validator
DeleteBatchResultData._all_field_names_ = set(['metadata'])
DeleteBatchResultData._all_fields_ = [('metadata', DeleteBatchResultData.metadata.validator)]
DeleteBatchResultEntry._success_validator = DeleteBatchResultData_validator
DeleteBatchResultEntry._failure_validator = DeleteError_validator
DeleteBatchResultEntry._tagmap = {
    'success': DeleteBatchResultEntry._success_validator,
    'failure': DeleteBatchResultEntry._failure_validator,
DeleteError._path_lookup_validator = LookupError_validator
DeleteError._path_write_validator = WriteError_validator
DeleteError._too_many_write_operations_validator = bv.Void()
DeleteError._too_many_files_validator = bv.Void()
DeleteError._other_validator = bv.Void()
DeleteError._tagmap = {
    'path_lookup': DeleteError._path_lookup_validator,
    'path_write': DeleteError._path_write_validator,
    'too_many_write_operations': DeleteError._too_many_write_operations_validator,
    'too_many_files': DeleteError._too_many_files_validator,
    'other': DeleteError._other_validator,
DeleteError.too_many_write_operations = DeleteError('too_many_write_operations')
DeleteError.too_many_files = DeleteError('too_many_files')
DeleteError.other = DeleteError('other')
DeleteResult.metadata.validator = Metadata_validator
DeleteResult._all_field_names_ = FileOpsResult._all_field_names_.union(set(['metadata']))
DeleteResult._all_fields_ = FileOpsResult._all_fields_ + [('metadata', DeleteResult.metadata.validator)]
Metadata.name.validator = bv.String()
Metadata.path_lower.validator = bv.Nullable(bv.String())
Metadata.path_display.validator = bv.Nullable(bv.String())
Metadata.parent_shared_folder_id.validator = bv.Nullable(common.SharedFolderId_validator)
Metadata.preview_url.validator = bv.Nullable(bv.String())
Metadata._field_names_ = set([
    'path_lower',
    'path_display',
    'parent_shared_folder_id',
    'preview_url',
Metadata._all_field_names_ = Metadata._field_names_
Metadata._fields_ = [
    ('name', Metadata.name.validator),
    ('path_lower', Metadata.path_lower.validator),
    ('path_display', Metadata.path_display.validator),
    ('parent_shared_folder_id', Metadata.parent_shared_folder_id.validator),
    ('preview_url', Metadata.preview_url.validator),
Metadata._all_fields_ = Metadata._fields_
Metadata._tag_to_subtype_ = {
    ('file',): FileMetadata_validator,
    ('folder',): FolderMetadata_validator,
    ('deleted',): DeletedMetadata_validator,
Metadata._pytype_to_tag_and_subtype_ = {
    FileMetadata: (('file',), FileMetadata_validator),
    FolderMetadata: (('folder',), FolderMetadata_validator),
    DeletedMetadata: (('deleted',), DeletedMetadata_validator),
Metadata._is_catch_all_ = False
DeletedMetadata._field_names_ = set([])
DeletedMetadata._all_field_names_ = Metadata._all_field_names_.union(DeletedMetadata._field_names_)
DeletedMetadata._fields_ = []
DeletedMetadata._all_fields_ = Metadata._all_fields_ + DeletedMetadata._fields_
Dimensions.height.validator = bv.UInt64()
Dimensions.width.validator = bv.UInt64()
Dimensions._all_field_names_ = set([
    'height',
    'width',
Dimensions._all_fields_ = [
    ('height', Dimensions.height.validator),
    ('width', Dimensions.width.validator),
DownloadArg.path.validator = ReadPath_validator
DownloadArg.rev.validator = bv.Nullable(Rev_validator)
DownloadArg._all_field_names_ = set([
    'rev',
DownloadArg._all_fields_ = [
    ('path', DownloadArg.path.validator),
    ('rev', DownloadArg.rev.validator),
DownloadError._path_validator = LookupError_validator
DownloadError._unsupported_file_validator = bv.Void()
DownloadError._other_validator = bv.Void()
DownloadError._tagmap = {
    'path': DownloadError._path_validator,
    'unsupported_file': DownloadError._unsupported_file_validator,
    'other': DownloadError._other_validator,
DownloadError.unsupported_file = DownloadError('unsupported_file')
DownloadError.other = DownloadError('other')
DownloadZipArg.path.validator = ReadPath_validator
DownloadZipArg._all_field_names_ = set(['path'])
DownloadZipArg._all_fields_ = [('path', DownloadZipArg.path.validator)]
DownloadZipError._path_validator = LookupError_validator
DownloadZipError._too_large_validator = bv.Void()
DownloadZipError._too_many_files_validator = bv.Void()
DownloadZipError._other_validator = bv.Void()
DownloadZipError._tagmap = {
    'path': DownloadZipError._path_validator,
    'too_large': DownloadZipError._too_large_validator,
    'too_many_files': DownloadZipError._too_many_files_validator,
    'other': DownloadZipError._other_validator,
DownloadZipError.too_large = DownloadZipError('too_large')
DownloadZipError.too_many_files = DownloadZipError('too_many_files')
DownloadZipError.other = DownloadZipError('other')
DownloadZipResult.metadata.validator = FolderMetadata_validator
DownloadZipResult._all_field_names_ = set(['metadata'])
DownloadZipResult._all_fields_ = [('metadata', DownloadZipResult.metadata.validator)]
ExportArg.path.validator = ReadPath_validator
ExportArg.export_format.validator = bv.Nullable(bv.String())
ExportArg._all_field_names_ = set([
    'export_format',
ExportArg._all_fields_ = [
    ('path', ExportArg.path.validator),
    ('export_format', ExportArg.export_format.validator),
ExportError._path_validator = LookupError_validator
ExportError._non_exportable_validator = bv.Void()
ExportError._invalid_export_format_validator = bv.Void()
ExportError._retry_error_validator = bv.Void()
ExportError._other_validator = bv.Void()
ExportError._tagmap = {
    'path': ExportError._path_validator,
    'non_exportable': ExportError._non_exportable_validator,
    'invalid_export_format': ExportError._invalid_export_format_validator,
    'retry_error': ExportError._retry_error_validator,
    'other': ExportError._other_validator,
ExportError.non_exportable = ExportError('non_exportable')
ExportError.invalid_export_format = ExportError('invalid_export_format')
ExportError.retry_error = ExportError('retry_error')
ExportError.other = ExportError('other')
ExportInfo.export_as.validator = bv.Nullable(bv.String())
ExportInfo.export_options.validator = bv.Nullable(bv.List(bv.String()))
ExportInfo._all_field_names_ = set([
    'export_as',
    'export_options',
ExportInfo._all_fields_ = [
    ('export_as', ExportInfo.export_as.validator),
    ('export_options', ExportInfo.export_options.validator),
ExportMetadata.name.validator = bv.String()
ExportMetadata.size.validator = bv.UInt64()
ExportMetadata.export_hash.validator = bv.Nullable(Sha256HexHash_validator)
ExportMetadata.paper_revision.validator = bv.Nullable(bv.Int64())
ExportMetadata._all_field_names_ = set([
    'size',
    'export_hash',
    'paper_revision',
ExportMetadata._all_fields_ = [
    ('name', ExportMetadata.name.validator),
    ('size', ExportMetadata.size.validator),
    ('export_hash', ExportMetadata.export_hash.validator),
    ('paper_revision', ExportMetadata.paper_revision.validator),
ExportResult.export_metadata.validator = ExportMetadata_validator
ExportResult.file_metadata.validator = FileMetadata_validator
ExportResult._all_field_names_ = set([
    'export_metadata',
    'file_metadata',
ExportResult._all_fields_ = [
    ('export_metadata', ExportResult.export_metadata.validator),
    ('file_metadata', ExportResult.file_metadata.validator),
FileCategory._image_validator = bv.Void()
FileCategory._document_validator = bv.Void()
FileCategory._pdf_validator = bv.Void()
FileCategory._spreadsheet_validator = bv.Void()
FileCategory._presentation_validator = bv.Void()
FileCategory._audio_validator = bv.Void()
FileCategory._video_validator = bv.Void()
FileCategory._folder_validator = bv.Void()
FileCategory._paper_validator = bv.Void()
FileCategory._others_validator = bv.Void()
FileCategory._other_validator = bv.Void()
FileCategory._tagmap = {
    'image': FileCategory._image_validator,
    'document': FileCategory._document_validator,
    'pdf': FileCategory._pdf_validator,
    'spreadsheet': FileCategory._spreadsheet_validator,
    'presentation': FileCategory._presentation_validator,
    'audio': FileCategory._audio_validator,
    'video': FileCategory._video_validator,
    'folder': FileCategory._folder_validator,
    'paper': FileCategory._paper_validator,
    'others': FileCategory._others_validator,
    'other': FileCategory._other_validator,
FileCategory.image = FileCategory('image')
FileCategory.document = FileCategory('document')
FileCategory.pdf = FileCategory('pdf')
FileCategory.spreadsheet = FileCategory('spreadsheet')
FileCategory.presentation = FileCategory('presentation')
FileCategory.audio = FileCategory('audio')
FileCategory.video = FileCategory('video')
FileCategory.folder = FileCategory('folder')
FileCategory.paper = FileCategory('paper')
FileCategory.others = FileCategory('others')
FileCategory.other = FileCategory('other')
FileLock.content.validator = FileLockContent_validator
FileLock._all_field_names_ = set(['content'])
FileLock._all_fields_ = [('content', FileLock.content.validator)]
FileLockContent._unlocked_validator = bv.Void()
FileLockContent._single_user_validator = SingleUserLock_validator
FileLockContent._other_validator = bv.Void()
FileLockContent._tagmap = {
    'unlocked': FileLockContent._unlocked_validator,
    'single_user': FileLockContent._single_user_validator,
    'other': FileLockContent._other_validator,
FileLockContent.unlocked = FileLockContent('unlocked')
FileLockContent.other = FileLockContent('other')
FileLockMetadata.is_lockholder.validator = bv.Nullable(bv.Boolean())
FileLockMetadata.lockholder_name.validator = bv.Nullable(bv.String())
FileLockMetadata.lockholder_account_id.validator = bv.Nullable(users_common.AccountId_validator)
FileLockMetadata.created.validator = bv.Nullable(common.DropboxTimestamp_validator)
FileLockMetadata._all_field_names_ = set([
    'is_lockholder',
    'lockholder_name',
    'lockholder_account_id',
FileLockMetadata._all_fields_ = [
    ('is_lockholder', FileLockMetadata.is_lockholder.validator),
    ('lockholder_name', FileLockMetadata.lockholder_name.validator),
    ('lockholder_account_id', FileLockMetadata.lockholder_account_id.validator),
    ('created', FileLockMetadata.created.validator),
FileMetadata.id.validator = Id_validator
FileMetadata.client_modified.validator = common.DropboxTimestamp_validator
FileMetadata.server_modified.validator = common.DropboxTimestamp_validator
FileMetadata.rev.validator = Rev_validator
FileMetadata.size.validator = bv.UInt64()
FileMetadata.media_info.validator = bv.Nullable(MediaInfo_validator)
FileMetadata.symlink_info.validator = bv.Nullable(SymlinkInfo_validator)
FileMetadata.sharing_info.validator = bv.Nullable(FileSharingInfo_validator)
FileMetadata.is_downloadable.validator = bv.Boolean()
FileMetadata.export_info.validator = bv.Nullable(ExportInfo_validator)
FileMetadata.property_groups.validator = bv.Nullable(bv.List(file_properties.PropertyGroup_validator))
FileMetadata.has_explicit_shared_members.validator = bv.Nullable(bv.Boolean())
FileMetadata.content_hash.validator = bv.Nullable(Sha256HexHash_validator)
FileMetadata.file_lock_info.validator = bv.Nullable(FileLockMetadata_validator)
FileMetadata._field_names_ = set([
    'server_modified',
    'media_info',
    'symlink_info',
    'sharing_info',
    'is_downloadable',
    'export_info',
    'has_explicit_shared_members',
    'content_hash',
    'file_lock_info',
FileMetadata._all_field_names_ = Metadata._all_field_names_.union(FileMetadata._field_names_)
FileMetadata._fields_ = [
    ('id', FileMetadata.id.validator),
    ('client_modified', FileMetadata.client_modified.validator),
    ('server_modified', FileMetadata.server_modified.validator),
    ('rev', FileMetadata.rev.validator),
    ('size', FileMetadata.size.validator),
    ('media_info', FileMetadata.media_info.validator),
    ('symlink_info', FileMetadata.symlink_info.validator),
    ('sharing_info', FileMetadata.sharing_info.validator),
    ('is_downloadable', FileMetadata.is_downloadable.validator),
    ('export_info', FileMetadata.export_info.validator),
    ('property_groups', FileMetadata.property_groups.validator),
    ('has_explicit_shared_members', FileMetadata.has_explicit_shared_members.validator),
    ('content_hash', FileMetadata.content_hash.validator),
    ('file_lock_info', FileMetadata.file_lock_info.validator),
FileMetadata._all_fields_ = Metadata._all_fields_ + FileMetadata._fields_
SharingInfo.read_only.validator = bv.Boolean()
SharingInfo._all_field_names_ = set(['read_only'])
SharingInfo._all_fields_ = [('read_only', SharingInfo.read_only.validator)]
FileSharingInfo.parent_shared_folder_id.validator = common.SharedFolderId_validator
FileSharingInfo.modified_by.validator = bv.Nullable(users_common.AccountId_validator)
FileSharingInfo._all_field_names_ = SharingInfo._all_field_names_.union(set([
    'modified_by',
FileSharingInfo._all_fields_ = SharingInfo._all_fields_ + [
    ('parent_shared_folder_id', FileSharingInfo.parent_shared_folder_id.validator),
    ('modified_by', FileSharingInfo.modified_by.validator),
FileStatus._active_validator = bv.Void()
FileStatus._deleted_validator = bv.Void()
FileStatus._other_validator = bv.Void()
FileStatus._tagmap = {
    'active': FileStatus._active_validator,
    'deleted': FileStatus._deleted_validator,
    'other': FileStatus._other_validator,
FileStatus.active = FileStatus('active')
FileStatus.deleted = FileStatus('deleted')
FileStatus.other = FileStatus('other')
FolderMetadata.id.validator = Id_validator
FolderMetadata.shared_folder_id.validator = bv.Nullable(common.SharedFolderId_validator)
FolderMetadata.sharing_info.validator = bv.Nullable(FolderSharingInfo_validator)
FolderMetadata.property_groups.validator = bv.Nullable(bv.List(file_properties.PropertyGroup_validator))
FolderMetadata._field_names_ = set([
    'shared_folder_id',
FolderMetadata._all_field_names_ = Metadata._all_field_names_.union(FolderMetadata._field_names_)
FolderMetadata._fields_ = [
    ('id', FolderMetadata.id.validator),
    ('shared_folder_id', FolderMetadata.shared_folder_id.validator),
    ('sharing_info', FolderMetadata.sharing_info.validator),
    ('property_groups', FolderMetadata.property_groups.validator),
FolderMetadata._all_fields_ = Metadata._all_fields_ + FolderMetadata._fields_
FolderSharingInfo.parent_shared_folder_id.validator = bv.Nullable(common.SharedFolderId_validator)
FolderSharingInfo.shared_folder_id.validator = bv.Nullable(common.SharedFolderId_validator)
FolderSharingInfo.traverse_only.validator = bv.Boolean()
FolderSharingInfo.no_access.validator = bv.Boolean()
FolderSharingInfo._all_field_names_ = SharingInfo._all_field_names_.union(set([
    'traverse_only',
    'no_access',
FolderSharingInfo._all_fields_ = SharingInfo._all_fields_ + [
    ('parent_shared_folder_id', FolderSharingInfo.parent_shared_folder_id.validator),
    ('shared_folder_id', FolderSharingInfo.shared_folder_id.validator),
    ('traverse_only', FolderSharingInfo.traverse_only.validator),
    ('no_access', FolderSharingInfo.no_access.validator),
GetCopyReferenceArg.path.validator = ReadPath_validator
GetCopyReferenceArg._all_field_names_ = set(['path'])
GetCopyReferenceArg._all_fields_ = [('path', GetCopyReferenceArg.path.validator)]
GetCopyReferenceError._path_validator = LookupError_validator
GetCopyReferenceError._other_validator = bv.Void()
GetCopyReferenceError._tagmap = {
    'path': GetCopyReferenceError._path_validator,
    'other': GetCopyReferenceError._other_validator,
GetCopyReferenceError.other = GetCopyReferenceError('other')
GetCopyReferenceResult.metadata.validator = Metadata_validator
GetCopyReferenceResult.copy_reference.validator = bv.String()
GetCopyReferenceResult.expires.validator = common.DropboxTimestamp_validator
GetCopyReferenceResult._all_field_names_ = set([
    'copy_reference',
    'expires',
GetCopyReferenceResult._all_fields_ = [
    ('metadata', GetCopyReferenceResult.metadata.validator),
    ('copy_reference', GetCopyReferenceResult.copy_reference.validator),
    ('expires', GetCopyReferenceResult.expires.validator),
GetTagsArg.paths.validator = bv.List(Path_validator)
GetTagsArg._all_field_names_ = set(['paths'])
GetTagsArg._all_fields_ = [('paths', GetTagsArg.paths.validator)]
GetTagsResult.paths_to_tags.validator = bv.List(PathToTags_validator)
GetTagsResult._all_field_names_ = set(['paths_to_tags'])
GetTagsResult._all_fields_ = [('paths_to_tags', GetTagsResult.paths_to_tags.validator)]
GetTemporaryLinkArg.path.validator = ReadPath_validator
GetTemporaryLinkArg._all_field_names_ = set(['path'])
GetTemporaryLinkArg._all_fields_ = [('path', GetTemporaryLinkArg.path.validator)]
GetTemporaryLinkError._path_validator = LookupError_validator
GetTemporaryLinkError._email_not_verified_validator = bv.Void()
GetTemporaryLinkError._unsupported_file_validator = bv.Void()
GetTemporaryLinkError._not_allowed_validator = bv.Void()
GetTemporaryLinkError._other_validator = bv.Void()
GetTemporaryLinkError._tagmap = {
    'path': GetTemporaryLinkError._path_validator,
    'email_not_verified': GetTemporaryLinkError._email_not_verified_validator,
    'unsupported_file': GetTemporaryLinkError._unsupported_file_validator,
    'not_allowed': GetTemporaryLinkError._not_allowed_validator,
    'other': GetTemporaryLinkError._other_validator,
GetTemporaryLinkError.email_not_verified = GetTemporaryLinkError('email_not_verified')
GetTemporaryLinkError.unsupported_file = GetTemporaryLinkError('unsupported_file')
GetTemporaryLinkError.not_allowed = GetTemporaryLinkError('not_allowed')
GetTemporaryLinkError.other = GetTemporaryLinkError('other')
GetTemporaryLinkResult.metadata.validator = FileMetadata_validator
GetTemporaryLinkResult.link.validator = bv.String()
GetTemporaryLinkResult._all_field_names_ = set([
GetTemporaryLinkResult._all_fields_ = [
    ('metadata', GetTemporaryLinkResult.metadata.validator),
    ('link', GetTemporaryLinkResult.link.validator),
GetTemporaryUploadLinkArg.commit_info.validator = CommitInfo_validator
GetTemporaryUploadLinkArg.duration.validator = bv.Float64(min_value=60.0, max_value=14400.0)
GetTemporaryUploadLinkArg._all_field_names_ = set([
    'commit_info',
    'duration',
GetTemporaryUploadLinkArg._all_fields_ = [
    ('commit_info', GetTemporaryUploadLinkArg.commit_info.validator),
    ('duration', GetTemporaryUploadLinkArg.duration.validator),
GetTemporaryUploadLinkResult.link.validator = bv.String()
GetTemporaryUploadLinkResult._all_field_names_ = set(['link'])
GetTemporaryUploadLinkResult._all_fields_ = [('link', GetTemporaryUploadLinkResult.link.validator)]
GetThumbnailBatchArg.entries.validator = bv.List(ThumbnailArg_validator)
GetThumbnailBatchArg._all_field_names_ = set(['entries'])
GetThumbnailBatchArg._all_fields_ = [('entries', GetThumbnailBatchArg.entries.validator)]
GetThumbnailBatchError._too_many_files_validator = bv.Void()
GetThumbnailBatchError._other_validator = bv.Void()
GetThumbnailBatchError._tagmap = {
    'too_many_files': GetThumbnailBatchError._too_many_files_validator,
    'other': GetThumbnailBatchError._other_validator,
GetThumbnailBatchError.too_many_files = GetThumbnailBatchError('too_many_files')
GetThumbnailBatchError.other = GetThumbnailBatchError('other')
GetThumbnailBatchResult.entries.validator = bv.List(GetThumbnailBatchResultEntry_validator)
GetThumbnailBatchResult._all_field_names_ = set(['entries'])
GetThumbnailBatchResult._all_fields_ = [('entries', GetThumbnailBatchResult.entries.validator)]
GetThumbnailBatchResultData.metadata.validator = FileMetadata_validator
GetThumbnailBatchResultData.thumbnail.validator = bv.String()
GetThumbnailBatchResultData._all_field_names_ = set([
    'thumbnail',
GetThumbnailBatchResultData._all_fields_ = [
    ('metadata', GetThumbnailBatchResultData.metadata.validator),
    ('thumbnail', GetThumbnailBatchResultData.thumbnail.validator),
GetThumbnailBatchResultEntry._success_validator = GetThumbnailBatchResultData_validator
GetThumbnailBatchResultEntry._failure_validator = ThumbnailError_validator
GetThumbnailBatchResultEntry._other_validator = bv.Void()
GetThumbnailBatchResultEntry._tagmap = {
    'success': GetThumbnailBatchResultEntry._success_validator,
    'failure': GetThumbnailBatchResultEntry._failure_validator,
    'other': GetThumbnailBatchResultEntry._other_validator,
GetThumbnailBatchResultEntry.other = GetThumbnailBatchResultEntry('other')
GpsCoordinates.latitude.validator = bv.Float64()
GpsCoordinates.longitude.validator = bv.Float64()
GpsCoordinates._all_field_names_ = set([
    'latitude',
    'longitude',
GpsCoordinates._all_fields_ = [
    ('latitude', GpsCoordinates.latitude.validator),
    ('longitude', GpsCoordinates.longitude.validator),
HighlightSpan.highlight_str.validator = bv.String()
HighlightSpan.is_highlighted.validator = bv.Boolean()
HighlightSpan._all_field_names_ = set([
    'highlight_str',
    'is_highlighted',
HighlightSpan._all_fields_ = [
    ('highlight_str', HighlightSpan.highlight_str.validator),
    ('is_highlighted', HighlightSpan.is_highlighted.validator),
ImportFormat._html_validator = bv.Void()
ImportFormat._markdown_validator = bv.Void()
ImportFormat._plain_text_validator = bv.Void()
ImportFormat._other_validator = bv.Void()
ImportFormat._tagmap = {
    'html': ImportFormat._html_validator,
    'markdown': ImportFormat._markdown_validator,
    'plain_text': ImportFormat._plain_text_validator,
    'other': ImportFormat._other_validator,
ImportFormat.html = ImportFormat('html')
ImportFormat.markdown = ImportFormat('markdown')
ImportFormat.plain_text = ImportFormat('plain_text')
ImportFormat.other = ImportFormat('other')
ListFolderArg.path.validator = PathROrId_validator
ListFolderArg.recursive.validator = bv.Boolean()
ListFolderArg.include_media_info.validator = bv.Boolean()
ListFolderArg.include_deleted.validator = bv.Boolean()
ListFolderArg.include_has_explicit_shared_members.validator = bv.Boolean()
ListFolderArg.include_mounted_folders.validator = bv.Boolean()
ListFolderArg.limit.validator = bv.Nullable(bv.UInt32(min_value=1, max_value=2000))
ListFolderArg.shared_link.validator = bv.Nullable(SharedLink_validator)
ListFolderArg.include_property_groups.validator = bv.Nullable(file_properties.TemplateFilterBase_validator)
ListFolderArg.include_non_downloadable_files.validator = bv.Boolean()
ListFolderArg._all_field_names_ = set([
    'recursive',
    'include_mounted_folders',
    'limit',
    'shared_link',
    'include_non_downloadable_files',
ListFolderArg._all_fields_ = [
    ('path', ListFolderArg.path.validator),
    ('recursive', ListFolderArg.recursive.validator),
    ('include_media_info', ListFolderArg.include_media_info.validator),
    ('include_deleted', ListFolderArg.include_deleted.validator),
    ('include_has_explicit_shared_members', ListFolderArg.include_has_explicit_shared_members.validator),
    ('include_mounted_folders', ListFolderArg.include_mounted_folders.validator),
    ('limit', ListFolderArg.limit.validator),
    ('shared_link', ListFolderArg.shared_link.validator),
    ('include_property_groups', ListFolderArg.include_property_groups.validator),
    ('include_non_downloadable_files', ListFolderArg.include_non_downloadable_files.validator),
ListFolderContinueArg.cursor.validator = ListFolderCursor_validator
ListFolderContinueArg._all_field_names_ = set(['cursor'])
ListFolderContinueArg._all_fields_ = [('cursor', ListFolderContinueArg.cursor.validator)]
ListFolderContinueError._path_validator = LookupError_validator
ListFolderContinueError._reset_validator = bv.Void()
ListFolderContinueError._other_validator = bv.Void()
ListFolderContinueError._tagmap = {
    'path': ListFolderContinueError._path_validator,
    'reset': ListFolderContinueError._reset_validator,
    'other': ListFolderContinueError._other_validator,
ListFolderContinueError.reset = ListFolderContinueError('reset')
ListFolderContinueError.other = ListFolderContinueError('other')
ListFolderError._path_validator = LookupError_validator
ListFolderError._template_error_validator = file_properties.TemplateError_validator
ListFolderError._other_validator = bv.Void()
ListFolderError._tagmap = {
    'path': ListFolderError._path_validator,
    'template_error': ListFolderError._template_error_validator,
    'other': ListFolderError._other_validator,
ListFolderError.other = ListFolderError('other')
ListFolderGetLatestCursorResult.cursor.validator = ListFolderCursor_validator
ListFolderGetLatestCursorResult._all_field_names_ = set(['cursor'])
ListFolderGetLatestCursorResult._all_fields_ = [('cursor', ListFolderGetLatestCursorResult.cursor.validator)]
ListFolderLongpollArg.cursor.validator = ListFolderCursor_validator
ListFolderLongpollArg.timeout.validator = bv.UInt64(min_value=30, max_value=480)
ListFolderLongpollArg._all_field_names_ = set([
ListFolderLongpollArg._all_fields_ = [
    ('cursor', ListFolderLongpollArg.cursor.validator),
    ('timeout', ListFolderLongpollArg.timeout.validator),
ListFolderLongpollError._reset_validator = bv.Void()
ListFolderLongpollError._other_validator = bv.Void()
ListFolderLongpollError._tagmap = {
    'reset': ListFolderLongpollError._reset_validator,
    'other': ListFolderLongpollError._other_validator,
ListFolderLongpollError.reset = ListFolderLongpollError('reset')
ListFolderLongpollError.other = ListFolderLongpollError('other')
ListFolderLongpollResult.changes.validator = bv.Boolean()
ListFolderLongpollResult.backoff.validator = bv.Nullable(bv.UInt64())
ListFolderLongpollResult._all_field_names_ = set([
    'changes',
    'backoff',
ListFolderLongpollResult._all_fields_ = [
    ('changes', ListFolderLongpollResult.changes.validator),
    ('backoff', ListFolderLongpollResult.backoff.validator),
ListFolderResult.entries.validator = bv.List(Metadata_validator)
ListFolderResult.cursor.validator = ListFolderCursor_validator
ListFolderResult.has_more.validator = bv.Boolean()
ListFolderResult._all_field_names_ = set([
    'entries',
ListFolderResult._all_fields_ = [
    ('entries', ListFolderResult.entries.validator),
    ('cursor', ListFolderResult.cursor.validator),
    ('has_more', ListFolderResult.has_more.validator),
ListRevisionsArg.path.validator = PathOrId_validator
ListRevisionsArg.mode.validator = ListRevisionsMode_validator
ListRevisionsArg.limit.validator = bv.UInt64(min_value=1, max_value=100)
ListRevisionsArg._all_field_names_ = set([
ListRevisionsArg._all_fields_ = [
    ('path', ListRevisionsArg.path.validator),
    ('mode', ListRevisionsArg.mode.validator),
    ('limit', ListRevisionsArg.limit.validator),
ListRevisionsError._path_validator = LookupError_validator
ListRevisionsError._other_validator = bv.Void()
ListRevisionsError._tagmap = {
    'path': ListRevisionsError._path_validator,
    'other': ListRevisionsError._other_validator,
ListRevisionsError.other = ListRevisionsError('other')
ListRevisionsMode._path_validator = bv.Void()
ListRevisionsMode._id_validator = bv.Void()
ListRevisionsMode._other_validator = bv.Void()
ListRevisionsMode._tagmap = {
    'path': ListRevisionsMode._path_validator,
    'id': ListRevisionsMode._id_validator,
    'other': ListRevisionsMode._other_validator,
ListRevisionsMode.path = ListRevisionsMode('path')
ListRevisionsMode.id = ListRevisionsMode('id')
ListRevisionsMode.other = ListRevisionsMode('other')
ListRevisionsResult.is_deleted.validator = bv.Boolean()
ListRevisionsResult.server_deleted.validator = bv.Nullable(common.DropboxTimestamp_validator)
ListRevisionsResult.entries.validator = bv.List(FileMetadata_validator)
ListRevisionsResult._all_field_names_ = set([
    'server_deleted',
ListRevisionsResult._all_fields_ = [
    ('is_deleted', ListRevisionsResult.is_deleted.validator),
    ('server_deleted', ListRevisionsResult.server_deleted.validator),
    ('entries', ListRevisionsResult.entries.validator),
LockConflictError.lock.validator = FileLock_validator
LockConflictError._all_field_names_ = set(['lock'])
LockConflictError._all_fields_ = [('lock', LockConflictError.lock.validator)]
LockFileArg.path.validator = WritePathOrId_validator
LockFileArg._all_field_names_ = set(['path'])
LockFileArg._all_fields_ = [('path', LockFileArg.path.validator)]
LockFileBatchArg.entries.validator = bv.List(LockFileArg_validator)
LockFileBatchArg._all_field_names_ = set(['entries'])
LockFileBatchArg._all_fields_ = [('entries', LockFileBatchArg.entries.validator)]
LockFileBatchResult.entries.validator = bv.List(LockFileResultEntry_validator)
LockFileBatchResult._all_field_names_ = FileOpsResult._all_field_names_.union(set(['entries']))
LockFileBatchResult._all_fields_ = FileOpsResult._all_fields_ + [('entries', LockFileBatchResult.entries.validator)]
LockFileError._path_lookup_validator = LookupError_validator
LockFileError._too_many_write_operations_validator = bv.Void()
LockFileError._too_many_files_validator = bv.Void()
LockFileError._no_write_permission_validator = bv.Void()
LockFileError._cannot_be_locked_validator = bv.Void()
LockFileError._file_not_shared_validator = bv.Void()
LockFileError._lock_conflict_validator = LockConflictError_validator
LockFileError._internal_error_validator = bv.Void()
LockFileError._other_validator = bv.Void()
LockFileError._tagmap = {
    'path_lookup': LockFileError._path_lookup_validator,
    'too_many_write_operations': LockFileError._too_many_write_operations_validator,
    'too_many_files': LockFileError._too_many_files_validator,
    'no_write_permission': LockFileError._no_write_permission_validator,
    'cannot_be_locked': LockFileError._cannot_be_locked_validator,
    'file_not_shared': LockFileError._file_not_shared_validator,
    'lock_conflict': LockFileError._lock_conflict_validator,
    'internal_error': LockFileError._internal_error_validator,
    'other': LockFileError._other_validator,
LockFileError.too_many_write_operations = LockFileError('too_many_write_operations')
LockFileError.too_many_files = LockFileError('too_many_files')
LockFileError.no_write_permission = LockFileError('no_write_permission')
LockFileError.cannot_be_locked = LockFileError('cannot_be_locked')
LockFileError.file_not_shared = LockFileError('file_not_shared')
LockFileError.internal_error = LockFileError('internal_error')
LockFileError.other = LockFileError('other')
LockFileResult.metadata.validator = Metadata_validator
LockFileResult.lock.validator = FileLock_validator
LockFileResult._all_field_names_ = set([
    'lock',
LockFileResult._all_fields_ = [
    ('metadata', LockFileResult.metadata.validator),
    ('lock', LockFileResult.lock.validator),
LockFileResultEntry._success_validator = LockFileResult_validator
LockFileResultEntry._failure_validator = LockFileError_validator
LockFileResultEntry._tagmap = {
    'success': LockFileResultEntry._success_validator,
    'failure': LockFileResultEntry._failure_validator,
LookupError._malformed_path_validator = MalformedPathError_validator
LookupError._unsupported_content_type_validator = bv.Void()
LookupError._locked_validator = bv.Void()
    'unsupported_content_type': LookupError._unsupported_content_type_validator,
    'locked': LookupError._locked_validator,
LookupError.unsupported_content_type = LookupError('unsupported_content_type')
LookupError.locked = LookupError('locked')
MediaInfo._pending_validator = bv.Void()
MediaInfo._metadata_validator = MediaMetadata_validator
MediaInfo._tagmap = {
    'pending': MediaInfo._pending_validator,
    'metadata': MediaInfo._metadata_validator,
MediaInfo.pending = MediaInfo('pending')
MediaMetadata.dimensions.validator = bv.Nullable(Dimensions_validator)
MediaMetadata.location.validator = bv.Nullable(GpsCoordinates_validator)
MediaMetadata.time_taken.validator = bv.Nullable(common.DropboxTimestamp_validator)
MediaMetadata._field_names_ = set([
    'dimensions',
    'time_taken',
MediaMetadata._all_field_names_ = MediaMetadata._field_names_
MediaMetadata._fields_ = [
    ('dimensions', MediaMetadata.dimensions.validator),
    ('location', MediaMetadata.location.validator),
    ('time_taken', MediaMetadata.time_taken.validator),
MediaMetadata._all_fields_ = MediaMetadata._fields_
MediaMetadata._tag_to_subtype_ = {
    ('photo',): PhotoMetadata_validator,
    ('video',): VideoMetadata_validator,
MediaMetadata._pytype_to_tag_and_subtype_ = {
    PhotoMetadata: (('photo',), PhotoMetadata_validator),
    VideoMetadata: (('video',), VideoMetadata_validator),
MediaMetadata._is_catch_all_ = False
MetadataV2._metadata_validator = Metadata_validator
MetadataV2._other_validator = bv.Void()
MetadataV2._tagmap = {
    'metadata': MetadataV2._metadata_validator,
    'other': MetadataV2._other_validator,
MetadataV2.other = MetadataV2('other')
MinimalFileLinkMetadata.url.validator = bv.String()
MinimalFileLinkMetadata.id.validator = bv.Nullable(Id_validator)
MinimalFileLinkMetadata.path.validator = bv.Nullable(bv.String())
MinimalFileLinkMetadata.rev.validator = Rev_validator
MinimalFileLinkMetadata._all_field_names_ = set([
MinimalFileLinkMetadata._all_fields_ = [
    ('url', MinimalFileLinkMetadata.url.validator),
    ('id', MinimalFileLinkMetadata.id.validator),
    ('path', MinimalFileLinkMetadata.path.validator),
    ('rev', MinimalFileLinkMetadata.rev.validator),
RelocationBatchArgBase.entries.validator = bv.List(RelocationPath_validator, min_items=1, max_items=1000)
RelocationBatchArgBase.autorename.validator = bv.Boolean()
RelocationBatchArgBase._all_field_names_ = set([
RelocationBatchArgBase._all_fields_ = [
    ('entries', RelocationBatchArgBase.entries.validator),
    ('autorename', RelocationBatchArgBase.autorename.validator),
MoveBatchArg.allow_ownership_transfer.validator = bv.Boolean()
MoveBatchArg._all_field_names_ = RelocationBatchArgBase._all_field_names_.union(set(['allow_ownership_transfer']))
MoveBatchArg._all_fields_ = RelocationBatchArgBase._all_fields_ + [('allow_ownership_transfer', MoveBatchArg.allow_ownership_transfer.validator)]
MoveIntoFamilyError._is_shared_folder_validator = bv.Void()
MoveIntoFamilyError._other_validator = bv.Void()
MoveIntoFamilyError._tagmap = {
    'is_shared_folder': MoveIntoFamilyError._is_shared_folder_validator,
    'other': MoveIntoFamilyError._other_validator,
MoveIntoFamilyError.is_shared_folder = MoveIntoFamilyError('is_shared_folder')
MoveIntoFamilyError.other = MoveIntoFamilyError('other')
MoveIntoVaultError._is_shared_folder_validator = bv.Void()
MoveIntoVaultError._other_validator = bv.Void()
MoveIntoVaultError._tagmap = {
    'is_shared_folder': MoveIntoVaultError._is_shared_folder_validator,
    'other': MoveIntoVaultError._other_validator,
MoveIntoVaultError.is_shared_folder = MoveIntoVaultError('is_shared_folder')
MoveIntoVaultError.other = MoveIntoVaultError('other')
PaperContentError._insufficient_permissions_validator = bv.Void()
PaperContentError._content_malformed_validator = bv.Void()
PaperContentError._doc_length_exceeded_validator = bv.Void()
PaperContentError._image_size_exceeded_validator = bv.Void()
PaperContentError._other_validator = bv.Void()
PaperContentError._tagmap = {
    'insufficient_permissions': PaperContentError._insufficient_permissions_validator,
    'content_malformed': PaperContentError._content_malformed_validator,
    'doc_length_exceeded': PaperContentError._doc_length_exceeded_validator,
    'image_size_exceeded': PaperContentError._image_size_exceeded_validator,
    'other': PaperContentError._other_validator,
PaperContentError.insufficient_permissions = PaperContentError('insufficient_permissions')
PaperContentError.content_malformed = PaperContentError('content_malformed')
PaperContentError.doc_length_exceeded = PaperContentError('doc_length_exceeded')
PaperContentError.image_size_exceeded = PaperContentError('image_size_exceeded')
PaperContentError.other = PaperContentError('other')
PaperCreateArg.path.validator = Path_validator
PaperCreateArg.import_format.validator = ImportFormat_validator
PaperCreateArg._all_field_names_ = set([
    'import_format',
PaperCreateArg._all_fields_ = [
    ('path', PaperCreateArg.path.validator),
    ('import_format', PaperCreateArg.import_format.validator),
PaperCreateError._invalid_path_validator = bv.Void()
PaperCreateError._email_unverified_validator = bv.Void()
PaperCreateError._invalid_file_extension_validator = bv.Void()
PaperCreateError._paper_disabled_validator = bv.Void()
PaperCreateError._tagmap = {
    'invalid_path': PaperCreateError._invalid_path_validator,
    'email_unverified': PaperCreateError._email_unverified_validator,
    'invalid_file_extension': PaperCreateError._invalid_file_extension_validator,
    'paper_disabled': PaperCreateError._paper_disabled_validator,
PaperCreateError._tagmap.update(PaperContentError._tagmap)
PaperCreateError.invalid_path = PaperCreateError('invalid_path')
PaperCreateError.email_unverified = PaperCreateError('email_unverified')
PaperCreateError.invalid_file_extension = PaperCreateError('invalid_file_extension')
PaperCreateError.paper_disabled = PaperCreateError('paper_disabled')
PaperCreateResult.url.validator = bv.String()
PaperCreateResult.result_path.validator = bv.String()
PaperCreateResult.file_id.validator = FileId_validator
PaperCreateResult.paper_revision.validator = bv.Int64()
PaperCreateResult._all_field_names_ = set([
    'result_path',
    'file_id',
PaperCreateResult._all_fields_ = [
    ('url', PaperCreateResult.url.validator),
    ('result_path', PaperCreateResult.result_path.validator),
    ('file_id', PaperCreateResult.file_id.validator),
    ('paper_revision', PaperCreateResult.paper_revision.validator),
PaperDocUpdatePolicy._update_validator = bv.Void()
PaperDocUpdatePolicy._overwrite_validator = bv.Void()
PaperDocUpdatePolicy._prepend_validator = bv.Void()
PaperDocUpdatePolicy._append_validator = bv.Void()
PaperDocUpdatePolicy._other_validator = bv.Void()
PaperDocUpdatePolicy._tagmap = {
    'update': PaperDocUpdatePolicy._update_validator,
    'overwrite': PaperDocUpdatePolicy._overwrite_validator,
    'prepend': PaperDocUpdatePolicy._prepend_validator,
    'append': PaperDocUpdatePolicy._append_validator,
    'other': PaperDocUpdatePolicy._other_validator,
PaperDocUpdatePolicy.update = PaperDocUpdatePolicy('update')
PaperDocUpdatePolicy.overwrite = PaperDocUpdatePolicy('overwrite')
PaperDocUpdatePolicy.prepend = PaperDocUpdatePolicy('prepend')
PaperDocUpdatePolicy.append = PaperDocUpdatePolicy('append')
PaperDocUpdatePolicy.other = PaperDocUpdatePolicy('other')
PaperUpdateArg.path.validator = WritePathOrId_validator
PaperUpdateArg.import_format.validator = ImportFormat_validator
PaperUpdateArg.doc_update_policy.validator = PaperDocUpdatePolicy_validator
PaperUpdateArg.paper_revision.validator = bv.Nullable(bv.Int64())
PaperUpdateArg._all_field_names_ = set([
    'doc_update_policy',
PaperUpdateArg._all_fields_ = [
    ('path', PaperUpdateArg.path.validator),
    ('import_format', PaperUpdateArg.import_format.validator),
    ('doc_update_policy', PaperUpdateArg.doc_update_policy.validator),
    ('paper_revision', PaperUpdateArg.paper_revision.validator),
PaperUpdateError._path_validator = LookupError_validator
PaperUpdateError._revision_mismatch_validator = bv.Void()
PaperUpdateError._doc_archived_validator = bv.Void()
PaperUpdateError._doc_deleted_validator = bv.Void()
PaperUpdateError._tagmap = {
    'path': PaperUpdateError._path_validator,
    'revision_mismatch': PaperUpdateError._revision_mismatch_validator,
    'doc_archived': PaperUpdateError._doc_archived_validator,
    'doc_deleted': PaperUpdateError._doc_deleted_validator,
PaperUpdateError._tagmap.update(PaperContentError._tagmap)
PaperUpdateError.revision_mismatch = PaperUpdateError('revision_mismatch')
PaperUpdateError.doc_archived = PaperUpdateError('doc_archived')
PaperUpdateError.doc_deleted = PaperUpdateError('doc_deleted')
PaperUpdateResult.paper_revision.validator = bv.Int64()
PaperUpdateResult._all_field_names_ = set(['paper_revision'])
PaperUpdateResult._all_fields_ = [('paper_revision', PaperUpdateResult.paper_revision.validator)]
PathOrLink._path_validator = ReadPath_validator
PathOrLink._link_validator = SharedLinkFileInfo_validator
PathOrLink._other_validator = bv.Void()
PathOrLink._tagmap = {
    'path': PathOrLink._path_validator,
    'link': PathOrLink._link_validator,
    'other': PathOrLink._other_validator,
PathOrLink.other = PathOrLink('other')
PathToTags.path.validator = Path_validator
PathToTags.tags.validator = bv.List(Tag_validator)
PathToTags._all_field_names_ = set([
    'tags',
PathToTags._all_fields_ = [
    ('path', PathToTags.path.validator),
    ('tags', PathToTags.tags.validator),
PhotoMetadata._field_names_ = set([])
PhotoMetadata._all_field_names_ = MediaMetadata._all_field_names_.union(PhotoMetadata._field_names_)
PhotoMetadata._fields_ = []
PhotoMetadata._all_fields_ = MediaMetadata._all_fields_ + PhotoMetadata._fields_
PreviewArg.path.validator = ReadPath_validator
PreviewArg.rev.validator = bv.Nullable(Rev_validator)
PreviewArg._all_field_names_ = set([
PreviewArg._all_fields_ = [
    ('path', PreviewArg.path.validator),
    ('rev', PreviewArg.rev.validator),
PreviewError._path_validator = LookupError_validator
PreviewError._in_progress_validator = bv.Void()
PreviewError._unsupported_extension_validator = bv.Void()
PreviewError._unsupported_content_validator = bv.Void()
PreviewError._tagmap = {
    'path': PreviewError._path_validator,
    'in_progress': PreviewError._in_progress_validator,
    'unsupported_extension': PreviewError._unsupported_extension_validator,
    'unsupported_content': PreviewError._unsupported_content_validator,
PreviewError.in_progress = PreviewError('in_progress')
PreviewError.unsupported_extension = PreviewError('unsupported_extension')
PreviewError.unsupported_content = PreviewError('unsupported_content')
PreviewResult.file_metadata.validator = bv.Nullable(FileMetadata_validator)
PreviewResult.link_metadata.validator = bv.Nullable(MinimalFileLinkMetadata_validator)
PreviewResult._all_field_names_ = set([
    'link_metadata',
PreviewResult._all_fields_ = [
    ('file_metadata', PreviewResult.file_metadata.validator),
    ('link_metadata', PreviewResult.link_metadata.validator),
RelocationPath.from_path.validator = WritePathOrId_validator
RelocationPath.to_path.validator = WritePathOrId_validator
RelocationPath._all_field_names_ = set([
    'from_path',
    'to_path',
RelocationPath._all_fields_ = [
    ('from_path', RelocationPath.from_path.validator),
    ('to_path', RelocationPath.to_path.validator),
RelocationArg.allow_shared_folder.validator = bv.Boolean()
RelocationArg.autorename.validator = bv.Boolean()
RelocationArg.allow_ownership_transfer.validator = bv.Boolean()
RelocationArg._all_field_names_ = RelocationPath._all_field_names_.union(set([
    'allow_shared_folder',
    'allow_ownership_transfer',
RelocationArg._all_fields_ = RelocationPath._all_fields_ + [
    ('allow_shared_folder', RelocationArg.allow_shared_folder.validator),
    ('autorename', RelocationArg.autorename.validator),
    ('allow_ownership_transfer', RelocationArg.allow_ownership_transfer.validator),
RelocationBatchArg.allow_shared_folder.validator = bv.Boolean()
RelocationBatchArg.allow_ownership_transfer.validator = bv.Boolean()
RelocationBatchArg._all_field_names_ = RelocationBatchArgBase._all_field_names_.union(set([
RelocationBatchArg._all_fields_ = RelocationBatchArgBase._all_fields_ + [
    ('allow_shared_folder', RelocationBatchArg.allow_shared_folder.validator),
    ('allow_ownership_transfer', RelocationBatchArg.allow_ownership_transfer.validator),
RelocationError._from_lookup_validator = LookupError_validator
RelocationError._from_write_validator = WriteError_validator
RelocationError._to_validator = WriteError_validator
RelocationError._cant_copy_shared_folder_validator = bv.Void()
RelocationError._cant_nest_shared_folder_validator = bv.Void()
RelocationError._cant_move_folder_into_itself_validator = bv.Void()
RelocationError._too_many_files_validator = bv.Void()
RelocationError._duplicated_or_nested_paths_validator = bv.Void()
RelocationError._cant_transfer_ownership_validator = bv.Void()
RelocationError._insufficient_quota_validator = bv.Void()
RelocationError._internal_error_validator = bv.Void()
RelocationError._cant_move_shared_folder_validator = bv.Void()
RelocationError._cant_move_into_vault_validator = MoveIntoVaultError_validator
RelocationError._cant_move_into_family_validator = MoveIntoFamilyError_validator
RelocationError._other_validator = bv.Void()
RelocationError._tagmap = {
    'from_lookup': RelocationError._from_lookup_validator,
    'from_write': RelocationError._from_write_validator,
    'to': RelocationError._to_validator,
    'cant_copy_shared_folder': RelocationError._cant_copy_shared_folder_validator,
    'cant_nest_shared_folder': RelocationError._cant_nest_shared_folder_validator,
    'cant_move_folder_into_itself': RelocationError._cant_move_folder_into_itself_validator,
    'too_many_files': RelocationError._too_many_files_validator,
    'duplicated_or_nested_paths': RelocationError._duplicated_or_nested_paths_validator,
    'cant_transfer_ownership': RelocationError._cant_transfer_ownership_validator,
    'insufficient_quota': RelocationError._insufficient_quota_validator,
    'internal_error': RelocationError._internal_error_validator,
    'cant_move_shared_folder': RelocationError._cant_move_shared_folder_validator,
    'cant_move_into_vault': RelocationError._cant_move_into_vault_validator,
    'cant_move_into_family': RelocationError._cant_move_into_family_validator,
    'other': RelocationError._other_validator,
RelocationError.cant_copy_shared_folder = RelocationError('cant_copy_shared_folder')
RelocationError.cant_nest_shared_folder = RelocationError('cant_nest_shared_folder')
RelocationError.cant_move_folder_into_itself = RelocationError('cant_move_folder_into_itself')
RelocationError.too_many_files = RelocationError('too_many_files')
RelocationError.duplicated_or_nested_paths = RelocationError('duplicated_or_nested_paths')
RelocationError.cant_transfer_ownership = RelocationError('cant_transfer_ownership')
RelocationError.insufficient_quota = RelocationError('insufficient_quota')
RelocationError.internal_error = RelocationError('internal_error')
RelocationError.cant_move_shared_folder = RelocationError('cant_move_shared_folder')
RelocationError.other = RelocationError('other')
RelocationBatchError._too_many_write_operations_validator = bv.Void()
RelocationBatchError._tagmap = {
    'too_many_write_operations': RelocationBatchError._too_many_write_operations_validator,
RelocationBatchError._tagmap.update(RelocationError._tagmap)
RelocationBatchError.too_many_write_operations = RelocationBatchError('too_many_write_operations')
RelocationBatchErrorEntry._relocation_error_validator = RelocationError_validator
RelocationBatchErrorEntry._internal_error_validator = bv.Void()
RelocationBatchErrorEntry._too_many_write_operations_validator = bv.Void()
RelocationBatchErrorEntry._other_validator = bv.Void()
RelocationBatchErrorEntry._tagmap = {
    'relocation_error': RelocationBatchErrorEntry._relocation_error_validator,
    'internal_error': RelocationBatchErrorEntry._internal_error_validator,
    'too_many_write_operations': RelocationBatchErrorEntry._too_many_write_operations_validator,
    'other': RelocationBatchErrorEntry._other_validator,
RelocationBatchErrorEntry.internal_error = RelocationBatchErrorEntry('internal_error')
RelocationBatchErrorEntry.too_many_write_operations = RelocationBatchErrorEntry('too_many_write_operations')
RelocationBatchErrorEntry.other = RelocationBatchErrorEntry('other')
RelocationBatchJobStatus._complete_validator = RelocationBatchResult_validator
RelocationBatchJobStatus._failed_validator = RelocationBatchError_validator
RelocationBatchJobStatus._tagmap = {
    'complete': RelocationBatchJobStatus._complete_validator,
    'failed': RelocationBatchJobStatus._failed_validator,
RelocationBatchJobStatus._tagmap.update(async_.PollResultBase._tagmap)
RelocationBatchLaunch._complete_validator = RelocationBatchResult_validator
RelocationBatchLaunch._other_validator = bv.Void()
RelocationBatchLaunch._tagmap = {
    'complete': RelocationBatchLaunch._complete_validator,
    'other': RelocationBatchLaunch._other_validator,
RelocationBatchLaunch._tagmap.update(async_.LaunchResultBase._tagmap)
RelocationBatchLaunch.other = RelocationBatchLaunch('other')
RelocationBatchResult.entries.validator = bv.List(RelocationBatchResultData_validator)
RelocationBatchResult._all_field_names_ = FileOpsResult._all_field_names_.union(set(['entries']))
RelocationBatchResult._all_fields_ = FileOpsResult._all_fields_ + [('entries', RelocationBatchResult.entries.validator)]
RelocationBatchResultData.metadata.validator = Metadata_validator
RelocationBatchResultData._all_field_names_ = set(['metadata'])
RelocationBatchResultData._all_fields_ = [('metadata', RelocationBatchResultData.metadata.validator)]
RelocationBatchResultEntry._success_validator = Metadata_validator
RelocationBatchResultEntry._failure_validator = RelocationBatchErrorEntry_validator
RelocationBatchResultEntry._other_validator = bv.Void()
RelocationBatchResultEntry._tagmap = {
    'success': RelocationBatchResultEntry._success_validator,
    'failure': RelocationBatchResultEntry._failure_validator,
    'other': RelocationBatchResultEntry._other_validator,
RelocationBatchResultEntry.other = RelocationBatchResultEntry('other')
RelocationBatchV2JobStatus._complete_validator = RelocationBatchV2Result_validator
RelocationBatchV2JobStatus._tagmap = {
    'complete': RelocationBatchV2JobStatus._complete_validator,
RelocationBatchV2JobStatus._tagmap.update(async_.PollResultBase._tagmap)
RelocationBatchV2Launch._complete_validator = RelocationBatchV2Result_validator
RelocationBatchV2Launch._tagmap = {
    'complete': RelocationBatchV2Launch._complete_validator,
RelocationBatchV2Launch._tagmap.update(async_.LaunchResultBase._tagmap)
RelocationBatchV2Result.entries.validator = bv.List(RelocationBatchResultEntry_validator)
RelocationBatchV2Result._all_field_names_ = FileOpsResult._all_field_names_.union(set(['entries']))
RelocationBatchV2Result._all_fields_ = FileOpsResult._all_fields_ + [('entries', RelocationBatchV2Result.entries.validator)]
RelocationResult.metadata.validator = Metadata_validator
RelocationResult._all_field_names_ = FileOpsResult._all_field_names_.union(set(['metadata']))
RelocationResult._all_fields_ = FileOpsResult._all_fields_ + [('metadata', RelocationResult.metadata.validator)]
RemoveTagArg.path.validator = Path_validator
RemoveTagArg.tag_text.validator = TagText_validator
RemoveTagArg._all_field_names_ = set([
RemoveTagArg._all_fields_ = [
    ('path', RemoveTagArg.path.validator),
    ('tag_text', RemoveTagArg.tag_text.validator),
RemoveTagError._tag_not_present_validator = bv.Void()
RemoveTagError._tagmap = {
    'tag_not_present': RemoveTagError._tag_not_present_validator,
RemoveTagError._tagmap.update(BaseTagError._tagmap)
RemoveTagError.tag_not_present = RemoveTagError('tag_not_present')
RestoreArg.path.validator = WritePath_validator
RestoreArg.rev.validator = Rev_validator
RestoreArg._all_field_names_ = set([
RestoreArg._all_fields_ = [
    ('path', RestoreArg.path.validator),
    ('rev', RestoreArg.rev.validator),
RestoreError._path_lookup_validator = LookupError_validator
RestoreError._path_write_validator = WriteError_validator
RestoreError._invalid_revision_validator = bv.Void()
RestoreError._in_progress_validator = bv.Void()
RestoreError._other_validator = bv.Void()
RestoreError._tagmap = {
    'path_lookup': RestoreError._path_lookup_validator,
    'path_write': RestoreError._path_write_validator,
    'invalid_revision': RestoreError._invalid_revision_validator,
    'in_progress': RestoreError._in_progress_validator,
    'other': RestoreError._other_validator,
RestoreError.invalid_revision = RestoreError('invalid_revision')
RestoreError.in_progress = RestoreError('in_progress')
RestoreError.other = RestoreError('other')
SaveCopyReferenceArg.copy_reference.validator = bv.String()
SaveCopyReferenceArg.path.validator = Path_validator
SaveCopyReferenceArg._all_field_names_ = set([
SaveCopyReferenceArg._all_fields_ = [
    ('copy_reference', SaveCopyReferenceArg.copy_reference.validator),
    ('path', SaveCopyReferenceArg.path.validator),
SaveCopyReferenceError._path_validator = WriteError_validator
SaveCopyReferenceError._invalid_copy_reference_validator = bv.Void()
SaveCopyReferenceError._no_permission_validator = bv.Void()
SaveCopyReferenceError._not_found_validator = bv.Void()
SaveCopyReferenceError._too_many_files_validator = bv.Void()
SaveCopyReferenceError._other_validator = bv.Void()
SaveCopyReferenceError._tagmap = {
    'path': SaveCopyReferenceError._path_validator,
    'invalid_copy_reference': SaveCopyReferenceError._invalid_copy_reference_validator,
    'no_permission': SaveCopyReferenceError._no_permission_validator,
    'not_found': SaveCopyReferenceError._not_found_validator,
    'too_many_files': SaveCopyReferenceError._too_many_files_validator,
    'other': SaveCopyReferenceError._other_validator,
SaveCopyReferenceError.invalid_copy_reference = SaveCopyReferenceError('invalid_copy_reference')
SaveCopyReferenceError.no_permission = SaveCopyReferenceError('no_permission')
SaveCopyReferenceError.not_found = SaveCopyReferenceError('not_found')
SaveCopyReferenceError.too_many_files = SaveCopyReferenceError('too_many_files')
SaveCopyReferenceError.other = SaveCopyReferenceError('other')
SaveCopyReferenceResult.metadata.validator = Metadata_validator
SaveCopyReferenceResult._all_field_names_ = set(['metadata'])
SaveCopyReferenceResult._all_fields_ = [('metadata', SaveCopyReferenceResult.metadata.validator)]
SaveUrlArg.path.validator = Path_validator
SaveUrlArg.url.validator = bv.String()
SaveUrlArg._all_field_names_ = set([
SaveUrlArg._all_fields_ = [
    ('path', SaveUrlArg.path.validator),
    ('url', SaveUrlArg.url.validator),
SaveUrlError._path_validator = WriteError_validator
SaveUrlError._download_failed_validator = bv.Void()
SaveUrlError._invalid_url_validator = bv.Void()
SaveUrlError._not_found_validator = bv.Void()
SaveUrlError._other_validator = bv.Void()
SaveUrlError._tagmap = {
    'path': SaveUrlError._path_validator,
    'download_failed': SaveUrlError._download_failed_validator,
    'invalid_url': SaveUrlError._invalid_url_validator,
    'not_found': SaveUrlError._not_found_validator,
    'other': SaveUrlError._other_validator,
SaveUrlError.download_failed = SaveUrlError('download_failed')
SaveUrlError.invalid_url = SaveUrlError('invalid_url')
SaveUrlError.not_found = SaveUrlError('not_found')
SaveUrlError.other = SaveUrlError('other')
SaveUrlJobStatus._complete_validator = FileMetadata_validator
SaveUrlJobStatus._failed_validator = SaveUrlError_validator
SaveUrlJobStatus._tagmap = {
    'complete': SaveUrlJobStatus._complete_validator,
    'failed': SaveUrlJobStatus._failed_validator,
SaveUrlJobStatus._tagmap.update(async_.PollResultBase._tagmap)
SaveUrlResult._complete_validator = FileMetadata_validator
SaveUrlResult._tagmap = {
    'complete': SaveUrlResult._complete_validator,
SaveUrlResult._tagmap.update(async_.LaunchResultBase._tagmap)
SearchArg.path.validator = PathROrId_validator
SearchArg.query.validator = bv.String(max_length=1000)
SearchArg.start.validator = bv.UInt64(max_value=9999)
SearchArg.max_results.validator = bv.UInt64(min_value=1, max_value=1000)
SearchArg.mode.validator = SearchMode_validator
SearchArg._all_field_names_ = set([
    'start',
    'max_results',
SearchArg._all_fields_ = [
    ('path', SearchArg.path.validator),
    ('query', SearchArg.query.validator),
    ('start', SearchArg.start.validator),
    ('max_results', SearchArg.max_results.validator),
    ('mode', SearchArg.mode.validator),
SearchError._path_validator = LookupError_validator
SearchError._invalid_argument_validator = bv.Nullable(bv.String())
SearchError._internal_error_validator = bv.Void()
SearchError._other_validator = bv.Void()
SearchError._tagmap = {
    'path': SearchError._path_validator,
    'invalid_argument': SearchError._invalid_argument_validator,
    'internal_error': SearchError._internal_error_validator,
    'other': SearchError._other_validator,
SearchError.internal_error = SearchError('internal_error')
SearchError.other = SearchError('other')
SearchMatch.match_type.validator = SearchMatchType_validator
SearchMatch.metadata.validator = Metadata_validator
SearchMatch._all_field_names_ = set([
    'match_type',
SearchMatch._all_fields_ = [
    ('match_type', SearchMatch.match_type.validator),
    ('metadata', SearchMatch.metadata.validator),
SearchMatchFieldOptions.include_highlights.validator = bv.Boolean()
SearchMatchFieldOptions._all_field_names_ = set(['include_highlights'])
SearchMatchFieldOptions._all_fields_ = [('include_highlights', SearchMatchFieldOptions.include_highlights.validator)]
SearchMatchType._filename_validator = bv.Void()
SearchMatchType._content_validator = bv.Void()
SearchMatchType._both_validator = bv.Void()
SearchMatchType._tagmap = {
    'filename': SearchMatchType._filename_validator,
    'content': SearchMatchType._content_validator,
    'both': SearchMatchType._both_validator,
SearchMatchType.filename = SearchMatchType('filename')
SearchMatchType.content = SearchMatchType('content')
SearchMatchType.both = SearchMatchType('both')
SearchMatchTypeV2._filename_validator = bv.Void()
SearchMatchTypeV2._file_content_validator = bv.Void()
SearchMatchTypeV2._filename_and_content_validator = bv.Void()
SearchMatchTypeV2._image_content_validator = bv.Void()
SearchMatchTypeV2._other_validator = bv.Void()
SearchMatchTypeV2._tagmap = {
    'filename': SearchMatchTypeV2._filename_validator,
    'file_content': SearchMatchTypeV2._file_content_validator,
    'filename_and_content': SearchMatchTypeV2._filename_and_content_validator,
    'image_content': SearchMatchTypeV2._image_content_validator,
    'other': SearchMatchTypeV2._other_validator,
SearchMatchTypeV2.filename = SearchMatchTypeV2('filename')
SearchMatchTypeV2.file_content = SearchMatchTypeV2('file_content')
SearchMatchTypeV2.filename_and_content = SearchMatchTypeV2('filename_and_content')
SearchMatchTypeV2.image_content = SearchMatchTypeV2('image_content')
SearchMatchTypeV2.other = SearchMatchTypeV2('other')
SearchMatchV2.metadata.validator = MetadataV2_validator
SearchMatchV2.match_type.validator = bv.Nullable(SearchMatchTypeV2_validator)
SearchMatchV2.highlight_spans.validator = bv.Nullable(bv.List(HighlightSpan_validator))
SearchMatchV2._all_field_names_ = set([
    'highlight_spans',
SearchMatchV2._all_fields_ = [
    ('metadata', SearchMatchV2.metadata.validator),
    ('match_type', SearchMatchV2.match_type.validator),
    ('highlight_spans', SearchMatchV2.highlight_spans.validator),
SearchMode._filename_validator = bv.Void()
SearchMode._filename_and_content_validator = bv.Void()
SearchMode._deleted_filename_validator = bv.Void()
SearchMode._tagmap = {
    'filename': SearchMode._filename_validator,
    'filename_and_content': SearchMode._filename_and_content_validator,
    'deleted_filename': SearchMode._deleted_filename_validator,
SearchMode.filename = SearchMode('filename')
SearchMode.filename_and_content = SearchMode('filename_and_content')
SearchMode.deleted_filename = SearchMode('deleted_filename')
SearchOptions.path.validator = bv.Nullable(PathROrId_validator)
SearchOptions.max_results.validator = bv.UInt64(min_value=1, max_value=1000)
SearchOptions.order_by.validator = bv.Nullable(SearchOrderBy_validator)
SearchOptions.file_status.validator = FileStatus_validator
SearchOptions.filename_only.validator = bv.Boolean()
SearchOptions.file_extensions.validator = bv.Nullable(bv.List(bv.String()))
SearchOptions.file_categories.validator = bv.Nullable(bv.List(FileCategory_validator))
SearchOptions.account_id.validator = bv.Nullable(users_common.AccountId_validator)
SearchOptions._all_field_names_ = set([
    'order_by',
    'file_status',
    'filename_only',
    'file_extensions',
    'file_categories',
    'account_id',
SearchOptions._all_fields_ = [
    ('path', SearchOptions.path.validator),
    ('max_results', SearchOptions.max_results.validator),
    ('order_by', SearchOptions.order_by.validator),
    ('file_status', SearchOptions.file_status.validator),
    ('filename_only', SearchOptions.filename_only.validator),
    ('file_extensions', SearchOptions.file_extensions.validator),
    ('file_categories', SearchOptions.file_categories.validator),
    ('account_id', SearchOptions.account_id.validator),
SearchOrderBy._relevance_validator = bv.Void()
SearchOrderBy._last_modified_time_validator = bv.Void()
SearchOrderBy._other_validator = bv.Void()
SearchOrderBy._tagmap = {
    'relevance': SearchOrderBy._relevance_validator,
    'last_modified_time': SearchOrderBy._last_modified_time_validator,
    'other': SearchOrderBy._other_validator,
SearchOrderBy.relevance = SearchOrderBy('relevance')
SearchOrderBy.last_modified_time = SearchOrderBy('last_modified_time')
SearchOrderBy.other = SearchOrderBy('other')
SearchResult.matches.validator = bv.List(SearchMatch_validator)
SearchResult.more.validator = bv.Boolean()
SearchResult.start.validator = bv.UInt64()
SearchResult._all_field_names_ = set([
    'more',
SearchResult._all_fields_ = [
    ('matches', SearchResult.matches.validator),
    ('more', SearchResult.more.validator),
    ('start', SearchResult.start.validator),
SearchV2Arg.query.validator = bv.String(max_length=1000)
SearchV2Arg.options.validator = bv.Nullable(SearchOptions_validator)
SearchV2Arg.match_field_options.validator = bv.Nullable(SearchMatchFieldOptions_validator)
SearchV2Arg.include_highlights.validator = bv.Nullable(bv.Boolean())
SearchV2Arg._all_field_names_ = set([
    'options',
    'match_field_options',
    'include_highlights',
SearchV2Arg._all_fields_ = [
    ('query', SearchV2Arg.query.validator),
    ('options', SearchV2Arg.options.validator),
    ('match_field_options', SearchV2Arg.match_field_options.validator),
    ('include_highlights', SearchV2Arg.include_highlights.validator),
SearchV2ContinueArg.cursor.validator = SearchV2Cursor_validator
SearchV2ContinueArg._all_field_names_ = set(['cursor'])
SearchV2ContinueArg._all_fields_ = [('cursor', SearchV2ContinueArg.cursor.validator)]
SearchV2Result.matches.validator = bv.List(SearchMatchV2_validator)
SearchV2Result.has_more.validator = bv.Boolean()
SearchV2Result.cursor.validator = bv.Nullable(SearchV2Cursor_validator)
SearchV2Result._all_field_names_ = set([
SearchV2Result._all_fields_ = [
    ('matches', SearchV2Result.matches.validator),
    ('has_more', SearchV2Result.has_more.validator),
    ('cursor', SearchV2Result.cursor.validator),
SharedLink.url.validator = SharedLinkUrl_validator
SharedLink.password.validator = bv.Nullable(bv.String())
SharedLink._all_field_names_ = set([
    'password',
SharedLink._all_fields_ = [
    ('url', SharedLink.url.validator),
    ('password', SharedLink.password.validator),
SharedLinkFileInfo.url.validator = bv.String()
SharedLinkFileInfo.path.validator = bv.Nullable(bv.String())
SharedLinkFileInfo.password.validator = bv.Nullable(bv.String())
SharedLinkFileInfo._all_field_names_ = set([
SharedLinkFileInfo._all_fields_ = [
    ('url', SharedLinkFileInfo.url.validator),
    ('path', SharedLinkFileInfo.path.validator),
    ('password', SharedLinkFileInfo.password.validator),
SingleUserLock.created.validator = common.DropboxTimestamp_validator
SingleUserLock.lock_holder_account_id.validator = users_common.AccountId_validator
SingleUserLock.lock_holder_team_id.validator = bv.Nullable(bv.String())
SingleUserLock._all_field_names_ = set([
    'lock_holder_account_id',
    'lock_holder_team_id',
SingleUserLock._all_fields_ = [
    ('created', SingleUserLock.created.validator),
    ('lock_holder_account_id', SingleUserLock.lock_holder_account_id.validator),
    ('lock_holder_team_id', SingleUserLock.lock_holder_team_id.validator),
SymlinkInfo.target.validator = bv.String()
SymlinkInfo._all_field_names_ = set(['target'])
SymlinkInfo._all_fields_ = [('target', SymlinkInfo.target.validator)]
SyncSetting._default_validator = bv.Void()
SyncSetting._not_synced_validator = bv.Void()
SyncSetting._not_synced_inactive_validator = bv.Void()
SyncSetting._other_validator = bv.Void()
SyncSetting._tagmap = {
    'default': SyncSetting._default_validator,
    'not_synced': SyncSetting._not_synced_validator,
    'not_synced_inactive': SyncSetting._not_synced_inactive_validator,
    'other': SyncSetting._other_validator,
SyncSetting.default = SyncSetting('default')
SyncSetting.not_synced = SyncSetting('not_synced')
SyncSetting.not_synced_inactive = SyncSetting('not_synced_inactive')
SyncSetting.other = SyncSetting('other')
SyncSettingArg._default_validator = bv.Void()
SyncSettingArg._not_synced_validator = bv.Void()
SyncSettingArg._other_validator = bv.Void()
SyncSettingArg._tagmap = {
    'default': SyncSettingArg._default_validator,
    'not_synced': SyncSettingArg._not_synced_validator,
    'other': SyncSettingArg._other_validator,
SyncSettingArg.default = SyncSettingArg('default')
SyncSettingArg.not_synced = SyncSettingArg('not_synced')
SyncSettingArg.other = SyncSettingArg('other')
SyncSettingsError._path_validator = LookupError_validator
SyncSettingsError._unsupported_combination_validator = bv.Void()
SyncSettingsError._unsupported_configuration_validator = bv.Void()
SyncSettingsError._other_validator = bv.Void()
SyncSettingsError._tagmap = {
    'path': SyncSettingsError._path_validator,
    'unsupported_combination': SyncSettingsError._unsupported_combination_validator,
    'unsupported_configuration': SyncSettingsError._unsupported_configuration_validator,
    'other': SyncSettingsError._other_validator,
SyncSettingsError.unsupported_combination = SyncSettingsError('unsupported_combination')
SyncSettingsError.unsupported_configuration = SyncSettingsError('unsupported_configuration')
SyncSettingsError.other = SyncSettingsError('other')
Tag._user_generated_tag_validator = UserGeneratedTag_validator
Tag._other_validator = bv.Void()
Tag._tagmap = {
    'user_generated_tag': Tag._user_generated_tag_validator,
    'other': Tag._other_validator,
Tag.other = Tag('other')
ThumbnailArg.path.validator = ReadPath_validator
ThumbnailArg.format.validator = ThumbnailFormat_validator
ThumbnailArg.size.validator = ThumbnailSize_validator
ThumbnailArg.mode.validator = ThumbnailMode_validator
ThumbnailArg._all_field_names_ = set([
    'format',
ThumbnailArg._all_fields_ = [
    ('path', ThumbnailArg.path.validator),
    ('format', ThumbnailArg.format.validator),
    ('size', ThumbnailArg.size.validator),
    ('mode', ThumbnailArg.mode.validator),
ThumbnailError._path_validator = LookupError_validator
ThumbnailError._unsupported_extension_validator = bv.Void()
ThumbnailError._unsupported_image_validator = bv.Void()
ThumbnailError._conversion_error_validator = bv.Void()
ThumbnailError._tagmap = {
    'path': ThumbnailError._path_validator,
    'unsupported_extension': ThumbnailError._unsupported_extension_validator,
    'unsupported_image': ThumbnailError._unsupported_image_validator,
    'conversion_error': ThumbnailError._conversion_error_validator,
ThumbnailError.unsupported_extension = ThumbnailError('unsupported_extension')
ThumbnailError.unsupported_image = ThumbnailError('unsupported_image')
ThumbnailError.conversion_error = ThumbnailError('conversion_error')
ThumbnailFormat._jpeg_validator = bv.Void()
ThumbnailFormat._png_validator = bv.Void()
ThumbnailFormat._tagmap = {
    'jpeg': ThumbnailFormat._jpeg_validator,
    'png': ThumbnailFormat._png_validator,
ThumbnailFormat.jpeg = ThumbnailFormat('jpeg')
ThumbnailFormat.png = ThumbnailFormat('png')
ThumbnailMode._strict_validator = bv.Void()
ThumbnailMode._bestfit_validator = bv.Void()
ThumbnailMode._fitone_bestfit_validator = bv.Void()
ThumbnailMode._tagmap = {
    'strict': ThumbnailMode._strict_validator,
    'bestfit': ThumbnailMode._bestfit_validator,
    'fitone_bestfit': ThumbnailMode._fitone_bestfit_validator,
ThumbnailMode.strict = ThumbnailMode('strict')
ThumbnailMode.bestfit = ThumbnailMode('bestfit')
ThumbnailMode.fitone_bestfit = ThumbnailMode('fitone_bestfit')
ThumbnailSize._w32h32_validator = bv.Void()
ThumbnailSize._w64h64_validator = bv.Void()
ThumbnailSize._w128h128_validator = bv.Void()
ThumbnailSize._w256h256_validator = bv.Void()
ThumbnailSize._w480h320_validator = bv.Void()
ThumbnailSize._w640h480_validator = bv.Void()
ThumbnailSize._w960h640_validator = bv.Void()
ThumbnailSize._w1024h768_validator = bv.Void()
ThumbnailSize._w2048h1536_validator = bv.Void()
ThumbnailSize._tagmap = {
    'w32h32': ThumbnailSize._w32h32_validator,
    'w64h64': ThumbnailSize._w64h64_validator,
    'w128h128': ThumbnailSize._w128h128_validator,
    'w256h256': ThumbnailSize._w256h256_validator,
    'w480h320': ThumbnailSize._w480h320_validator,
    'w640h480': ThumbnailSize._w640h480_validator,
    'w960h640': ThumbnailSize._w960h640_validator,
    'w1024h768': ThumbnailSize._w1024h768_validator,
    'w2048h1536': ThumbnailSize._w2048h1536_validator,
ThumbnailSize.w32h32 = ThumbnailSize('w32h32')
ThumbnailSize.w64h64 = ThumbnailSize('w64h64')
ThumbnailSize.w128h128 = ThumbnailSize('w128h128')
ThumbnailSize.w256h256 = ThumbnailSize('w256h256')
ThumbnailSize.w480h320 = ThumbnailSize('w480h320')
ThumbnailSize.w640h480 = ThumbnailSize('w640h480')
ThumbnailSize.w960h640 = ThumbnailSize('w960h640')
ThumbnailSize.w1024h768 = ThumbnailSize('w1024h768')
ThumbnailSize.w2048h1536 = ThumbnailSize('w2048h1536')
ThumbnailV2Arg.resource.validator = PathOrLink_validator
ThumbnailV2Arg.format.validator = ThumbnailFormat_validator
ThumbnailV2Arg.size.validator = ThumbnailSize_validator
ThumbnailV2Arg.mode.validator = ThumbnailMode_validator
ThumbnailV2Arg._all_field_names_ = set([
    'resource',
ThumbnailV2Arg._all_fields_ = [
    ('resource', ThumbnailV2Arg.resource.validator),
    ('format', ThumbnailV2Arg.format.validator),
    ('size', ThumbnailV2Arg.size.validator),
    ('mode', ThumbnailV2Arg.mode.validator),
ThumbnailV2Error._path_validator = LookupError_validator
ThumbnailV2Error._unsupported_extension_validator = bv.Void()
ThumbnailV2Error._unsupported_image_validator = bv.Void()
ThumbnailV2Error._conversion_error_validator = bv.Void()
ThumbnailV2Error._access_denied_validator = bv.Void()
ThumbnailV2Error._not_found_validator = bv.Void()
ThumbnailV2Error._other_validator = bv.Void()
ThumbnailV2Error._tagmap = {
    'path': ThumbnailV2Error._path_validator,
    'unsupported_extension': ThumbnailV2Error._unsupported_extension_validator,
    'unsupported_image': ThumbnailV2Error._unsupported_image_validator,
    'conversion_error': ThumbnailV2Error._conversion_error_validator,
    'access_denied': ThumbnailV2Error._access_denied_validator,
    'not_found': ThumbnailV2Error._not_found_validator,
    'other': ThumbnailV2Error._other_validator,
ThumbnailV2Error.unsupported_extension = ThumbnailV2Error('unsupported_extension')
ThumbnailV2Error.unsupported_image = ThumbnailV2Error('unsupported_image')
ThumbnailV2Error.conversion_error = ThumbnailV2Error('conversion_error')
ThumbnailV2Error.access_denied = ThumbnailV2Error('access_denied')
ThumbnailV2Error.not_found = ThumbnailV2Error('not_found')
ThumbnailV2Error.other = ThumbnailV2Error('other')
UnlockFileArg.path.validator = WritePathOrId_validator
UnlockFileArg._all_field_names_ = set(['path'])
UnlockFileArg._all_fields_ = [('path', UnlockFileArg.path.validator)]
UnlockFileBatchArg.entries.validator = bv.List(UnlockFileArg_validator)
UnlockFileBatchArg._all_field_names_ = set(['entries'])
UnlockFileBatchArg._all_fields_ = [('entries', UnlockFileBatchArg.entries.validator)]
UploadArg.content_hash.validator = bv.Nullable(Sha256HexHash_validator)
UploadArg._all_field_names_ = CommitInfo._all_field_names_.union(set(['content_hash']))
UploadArg._all_fields_ = CommitInfo._all_fields_ + [('content_hash', UploadArg.content_hash.validator)]
UploadError._path_validator = UploadWriteFailed_validator
UploadError._properties_error_validator = file_properties.InvalidPropertyGroupError_validator
UploadError._payload_too_large_validator = bv.Void()
UploadError._content_hash_mismatch_validator = bv.Void()
UploadError._other_validator = bv.Void()
UploadError._tagmap = {
    'path': UploadError._path_validator,
    'properties_error': UploadError._properties_error_validator,
    'payload_too_large': UploadError._payload_too_large_validator,
    'content_hash_mismatch': UploadError._content_hash_mismatch_validator,
    'other': UploadError._other_validator,
UploadError.payload_too_large = UploadError('payload_too_large')
UploadError.content_hash_mismatch = UploadError('content_hash_mismatch')
UploadError.other = UploadError('other')
UploadSessionAppendArg.cursor.validator = UploadSessionCursor_validator
UploadSessionAppendArg.close.validator = bv.Boolean()
UploadSessionAppendArg.content_hash.validator = bv.Nullable(Sha256HexHash_validator)
UploadSessionAppendArg._all_field_names_ = set([
    'close',
UploadSessionAppendArg._all_fields_ = [
    ('cursor', UploadSessionAppendArg.cursor.validator),
    ('close', UploadSessionAppendArg.close.validator),
    ('content_hash', UploadSessionAppendArg.content_hash.validator),
UploadSessionLookupError._not_found_validator = bv.Void()
UploadSessionLookupError._incorrect_offset_validator = UploadSessionOffsetError_validator
UploadSessionLookupError._closed_validator = bv.Void()
UploadSessionLookupError._not_closed_validator = bv.Void()
UploadSessionLookupError._too_large_validator = bv.Void()
UploadSessionLookupError._concurrent_session_invalid_offset_validator = bv.Void()
UploadSessionLookupError._concurrent_session_invalid_data_size_validator = bv.Void()
UploadSessionLookupError._payload_too_large_validator = bv.Void()
UploadSessionLookupError._other_validator = bv.Void()
UploadSessionLookupError._tagmap = {
    'not_found': UploadSessionLookupError._not_found_validator,
    'incorrect_offset': UploadSessionLookupError._incorrect_offset_validator,
    'closed': UploadSessionLookupError._closed_validator,
    'not_closed': UploadSessionLookupError._not_closed_validator,
    'too_large': UploadSessionLookupError._too_large_validator,
    'concurrent_session_invalid_offset': UploadSessionLookupError._concurrent_session_invalid_offset_validator,
    'concurrent_session_invalid_data_size': UploadSessionLookupError._concurrent_session_invalid_data_size_validator,
    'payload_too_large': UploadSessionLookupError._payload_too_large_validator,
    'other': UploadSessionLookupError._other_validator,
UploadSessionLookupError.not_found = UploadSessionLookupError('not_found')
UploadSessionLookupError.closed = UploadSessionLookupError('closed')
UploadSessionLookupError.not_closed = UploadSessionLookupError('not_closed')
UploadSessionLookupError.too_large = UploadSessionLookupError('too_large')
UploadSessionLookupError.concurrent_session_invalid_offset = UploadSessionLookupError('concurrent_session_invalid_offset')
UploadSessionLookupError.concurrent_session_invalid_data_size = UploadSessionLookupError('concurrent_session_invalid_data_size')
UploadSessionLookupError.payload_too_large = UploadSessionLookupError('payload_too_large')
UploadSessionLookupError.other = UploadSessionLookupError('other')
UploadSessionAppendError._content_hash_mismatch_validator = bv.Void()
UploadSessionAppendError._tagmap = {
    'content_hash_mismatch': UploadSessionAppendError._content_hash_mismatch_validator,
UploadSessionAppendError._tagmap.update(UploadSessionLookupError._tagmap)
UploadSessionAppendError.content_hash_mismatch = UploadSessionAppendError('content_hash_mismatch')
UploadSessionCursor.session_id.validator = bv.String()
UploadSessionCursor.offset.validator = bv.UInt64()
UploadSessionCursor._all_field_names_ = set([
    'session_id',
    'offset',
UploadSessionCursor._all_fields_ = [
    ('session_id', UploadSessionCursor.session_id.validator),
    ('offset', UploadSessionCursor.offset.validator),
UploadSessionFinishArg.cursor.validator = UploadSessionCursor_validator
UploadSessionFinishArg.commit.validator = CommitInfo_validator
UploadSessionFinishArg.content_hash.validator = bv.Nullable(Sha256HexHash_validator)
UploadSessionFinishArg._all_field_names_ = set([
    'commit',
UploadSessionFinishArg._all_fields_ = [
    ('cursor', UploadSessionFinishArg.cursor.validator),
    ('commit', UploadSessionFinishArg.commit.validator),
    ('content_hash', UploadSessionFinishArg.content_hash.validator),
UploadSessionFinishBatchArg.entries.validator = bv.List(UploadSessionFinishArg_validator, max_items=1000)
UploadSessionFinishBatchArg._all_field_names_ = set(['entries'])
UploadSessionFinishBatchArg._all_fields_ = [('entries', UploadSessionFinishBatchArg.entries.validator)]
UploadSessionFinishBatchJobStatus._complete_validator = UploadSessionFinishBatchResult_validator
UploadSessionFinishBatchJobStatus._tagmap = {
    'complete': UploadSessionFinishBatchJobStatus._complete_validator,
UploadSessionFinishBatchJobStatus._tagmap.update(async_.PollResultBase._tagmap)
UploadSessionFinishBatchLaunch._complete_validator = UploadSessionFinishBatchResult_validator
UploadSessionFinishBatchLaunch._other_validator = bv.Void()
UploadSessionFinishBatchLaunch._tagmap = {
    'complete': UploadSessionFinishBatchLaunch._complete_validator,
    'other': UploadSessionFinishBatchLaunch._other_validator,
UploadSessionFinishBatchLaunch._tagmap.update(async_.LaunchResultBase._tagmap)
UploadSessionFinishBatchLaunch.other = UploadSessionFinishBatchLaunch('other')
UploadSessionFinishBatchResult.entries.validator = bv.List(UploadSessionFinishBatchResultEntry_validator)
UploadSessionFinishBatchResult._all_field_names_ = set(['entries'])
UploadSessionFinishBatchResult._all_fields_ = [('entries', UploadSessionFinishBatchResult.entries.validator)]
UploadSessionFinishBatchResultEntry._success_validator = FileMetadata_validator
UploadSessionFinishBatchResultEntry._failure_validator = UploadSessionFinishError_validator
UploadSessionFinishBatchResultEntry._tagmap = {
    'success': UploadSessionFinishBatchResultEntry._success_validator,
    'failure': UploadSessionFinishBatchResultEntry._failure_validator,
UploadSessionFinishError._lookup_failed_validator = UploadSessionLookupError_validator
UploadSessionFinishError._path_validator = WriteError_validator
UploadSessionFinishError._properties_error_validator = file_properties.InvalidPropertyGroupError_validator
UploadSessionFinishError._too_many_shared_folder_targets_validator = bv.Void()
UploadSessionFinishError._too_many_write_operations_validator = bv.Void()
UploadSessionFinishError._concurrent_session_data_not_allowed_validator = bv.Void()
UploadSessionFinishError._concurrent_session_not_closed_validator = bv.Void()
UploadSessionFinishError._concurrent_session_missing_data_validator = bv.Void()
UploadSessionFinishError._payload_too_large_validator = bv.Void()
UploadSessionFinishError._content_hash_mismatch_validator = bv.Void()
UploadSessionFinishError._other_validator = bv.Void()
UploadSessionFinishError._tagmap = {
    'lookup_failed': UploadSessionFinishError._lookup_failed_validator,
    'path': UploadSessionFinishError._path_validator,
    'properties_error': UploadSessionFinishError._properties_error_validator,
    'too_many_shared_folder_targets': UploadSessionFinishError._too_many_shared_folder_targets_validator,
    'too_many_write_operations': UploadSessionFinishError._too_many_write_operations_validator,
    'concurrent_session_data_not_allowed': UploadSessionFinishError._concurrent_session_data_not_allowed_validator,
    'concurrent_session_not_closed': UploadSessionFinishError._concurrent_session_not_closed_validator,
    'concurrent_session_missing_data': UploadSessionFinishError._concurrent_session_missing_data_validator,
    'payload_too_large': UploadSessionFinishError._payload_too_large_validator,
    'content_hash_mismatch': UploadSessionFinishError._content_hash_mismatch_validator,
    'other': UploadSessionFinishError._other_validator,
UploadSessionFinishError.too_many_shared_folder_targets = UploadSessionFinishError('too_many_shared_folder_targets')
UploadSessionFinishError.too_many_write_operations = UploadSessionFinishError('too_many_write_operations')
UploadSessionFinishError.concurrent_session_data_not_allowed = UploadSessionFinishError('concurrent_session_data_not_allowed')
UploadSessionFinishError.concurrent_session_not_closed = UploadSessionFinishError('concurrent_session_not_closed')
UploadSessionFinishError.concurrent_session_missing_data = UploadSessionFinishError('concurrent_session_missing_data')
UploadSessionFinishError.payload_too_large = UploadSessionFinishError('payload_too_large')
UploadSessionFinishError.content_hash_mismatch = UploadSessionFinishError('content_hash_mismatch')
UploadSessionFinishError.other = UploadSessionFinishError('other')
UploadSessionOffsetError.correct_offset.validator = bv.UInt64()
UploadSessionOffsetError._all_field_names_ = set(['correct_offset'])
UploadSessionOffsetError._all_fields_ = [('correct_offset', UploadSessionOffsetError.correct_offset.validator)]
UploadSessionStartArg.close.validator = bv.Boolean()
UploadSessionStartArg.session_type.validator = bv.Nullable(UploadSessionType_validator)
UploadSessionStartArg.content_hash.validator = bv.Nullable(Sha256HexHash_validator)
UploadSessionStartArg._all_field_names_ = set([
    'session_type',
UploadSessionStartArg._all_fields_ = [
    ('close', UploadSessionStartArg.close.validator),
    ('session_type', UploadSessionStartArg.session_type.validator),
    ('content_hash', UploadSessionStartArg.content_hash.validator),
UploadSessionStartBatchArg.session_type.validator = bv.Nullable(UploadSessionType_validator)
UploadSessionStartBatchArg.num_sessions.validator = bv.UInt64(min_value=1, max_value=1000)
UploadSessionStartBatchArg._all_field_names_ = set([
    'num_sessions',
UploadSessionStartBatchArg._all_fields_ = [
    ('session_type', UploadSessionStartBatchArg.session_type.validator),
    ('num_sessions', UploadSessionStartBatchArg.num_sessions.validator),
UploadSessionStartBatchResult.session_ids.validator = bv.List(bv.String())
UploadSessionStartBatchResult._all_field_names_ = set(['session_ids'])
UploadSessionStartBatchResult._all_fields_ = [('session_ids', UploadSessionStartBatchResult.session_ids.validator)]
UploadSessionStartError._concurrent_session_data_not_allowed_validator = bv.Void()
UploadSessionStartError._concurrent_session_close_not_allowed_validator = bv.Void()
UploadSessionStartError._payload_too_large_validator = bv.Void()
UploadSessionStartError._content_hash_mismatch_validator = bv.Void()
UploadSessionStartError._other_validator = bv.Void()
UploadSessionStartError._tagmap = {
    'concurrent_session_data_not_allowed': UploadSessionStartError._concurrent_session_data_not_allowed_validator,
    'concurrent_session_close_not_allowed': UploadSessionStartError._concurrent_session_close_not_allowed_validator,
    'payload_too_large': UploadSessionStartError._payload_too_large_validator,
    'content_hash_mismatch': UploadSessionStartError._content_hash_mismatch_validator,
    'other': UploadSessionStartError._other_validator,
UploadSessionStartError.concurrent_session_data_not_allowed = UploadSessionStartError('concurrent_session_data_not_allowed')
UploadSessionStartError.concurrent_session_close_not_allowed = UploadSessionStartError('concurrent_session_close_not_allowed')
UploadSessionStartError.payload_too_large = UploadSessionStartError('payload_too_large')
UploadSessionStartError.content_hash_mismatch = UploadSessionStartError('content_hash_mismatch')
UploadSessionStartError.other = UploadSessionStartError('other')
UploadSessionStartResult.session_id.validator = bv.String()
UploadSessionStartResult._all_field_names_ = set(['session_id'])
UploadSessionStartResult._all_fields_ = [('session_id', UploadSessionStartResult.session_id.validator)]
UploadSessionType._sequential_validator = bv.Void()
UploadSessionType._concurrent_validator = bv.Void()
UploadSessionType._other_validator = bv.Void()
UploadSessionType._tagmap = {
    'sequential': UploadSessionType._sequential_validator,
    'concurrent': UploadSessionType._concurrent_validator,
    'other': UploadSessionType._other_validator,
UploadSessionType.sequential = UploadSessionType('sequential')
UploadSessionType.concurrent = UploadSessionType('concurrent')
UploadSessionType.other = UploadSessionType('other')
UploadWriteFailed.reason.validator = WriteError_validator
UploadWriteFailed.upload_session_id.validator = bv.String()
UploadWriteFailed._all_field_names_ = set([
    'upload_session_id',
UploadWriteFailed._all_fields_ = [
    ('reason', UploadWriteFailed.reason.validator),
    ('upload_session_id', UploadWriteFailed.upload_session_id.validator),
UserGeneratedTag.tag_text.validator = TagText_validator
UserGeneratedTag._all_field_names_ = set(['tag_text'])
UserGeneratedTag._all_fields_ = [('tag_text', UserGeneratedTag.tag_text.validator)]
VideoMetadata.duration.validator = bv.Nullable(bv.UInt64())
VideoMetadata._field_names_ = set(['duration'])
VideoMetadata._all_field_names_ = MediaMetadata._all_field_names_.union(VideoMetadata._field_names_)
VideoMetadata._fields_ = [('duration', VideoMetadata.duration.validator)]
VideoMetadata._all_fields_ = MediaMetadata._all_fields_ + VideoMetadata._fields_
WriteConflictError._file_validator = bv.Void()
WriteConflictError._folder_validator = bv.Void()
WriteConflictError._file_ancestor_validator = bv.Void()
WriteConflictError._other_validator = bv.Void()
WriteConflictError._tagmap = {
    'file': WriteConflictError._file_validator,
    'folder': WriteConflictError._folder_validator,
    'file_ancestor': WriteConflictError._file_ancestor_validator,
    'other': WriteConflictError._other_validator,
WriteConflictError.file = WriteConflictError('file')
WriteConflictError.folder = WriteConflictError('folder')
WriteConflictError.file_ancestor = WriteConflictError('file_ancestor')
WriteConflictError.other = WriteConflictError('other')
WriteError._malformed_path_validator = MalformedPathError_validator
WriteError._conflict_validator = WriteConflictError_validator
WriteError._no_write_permission_validator = bv.Void()
WriteError._insufficient_space_validator = bv.Void()
WriteError._disallowed_name_validator = bv.Void()
WriteError._team_folder_validator = bv.Void()
WriteError._operation_suppressed_validator = bv.Void()
WriteError._too_many_write_operations_validator = bv.Void()
WriteError._other_validator = bv.Void()
WriteError._tagmap = {
    'malformed_path': WriteError._malformed_path_validator,
    'conflict': WriteError._conflict_validator,
    'no_write_permission': WriteError._no_write_permission_validator,
    'insufficient_space': WriteError._insufficient_space_validator,
    'disallowed_name': WriteError._disallowed_name_validator,
    'team_folder': WriteError._team_folder_validator,
    'operation_suppressed': WriteError._operation_suppressed_validator,
    'too_many_write_operations': WriteError._too_many_write_operations_validator,
    'other': WriteError._other_validator,
WriteError.no_write_permission = WriteError('no_write_permission')
WriteError.insufficient_space = WriteError('insufficient_space')
WriteError.disallowed_name = WriteError('disallowed_name')
WriteError.team_folder = WriteError('team_folder')
WriteError.operation_suppressed = WriteError('operation_suppressed')
WriteError.too_many_write_operations = WriteError('too_many_write_operations')
WriteError.other = WriteError('other')
WriteMode._add_validator = bv.Void()
WriteMode._overwrite_validator = bv.Void()
WriteMode._update_validator = Rev_validator
WriteMode._tagmap = {
    'add': WriteMode._add_validator,
    'overwrite': WriteMode._overwrite_validator,
    'update': WriteMode._update_validator,
WriteMode.add = WriteMode('add')
WriteMode.overwrite = WriteMode('overwrite')
GetMetadataArg.include_media_info.default = False
GetMetadataArg.include_deleted.default = False
GetMetadataArg.include_has_explicit_shared_members.default = False
CommitInfo.mode.default = WriteMode.add
CommitInfo.autorename.default = False
CommitInfo.mute.default = False
CommitInfo.strict_conflict.default = False
CreateFolderArg.autorename.default = False
CreateFolderBatchArg.autorename.default = False
CreateFolderBatchArg.force_async.default = False
FileMetadata.is_downloadable.default = True
FolderSharingInfo.traverse_only.default = False
FolderSharingInfo.no_access.default = False
GetTemporaryUploadLinkArg.duration.default = 14400.0
ListFolderArg.recursive.default = False
ListFolderArg.include_media_info.default = False
ListFolderArg.include_deleted.default = False
ListFolderArg.include_has_explicit_shared_members.default = False
ListFolderArg.include_mounted_folders.default = True
ListFolderArg.include_non_downloadable_files.default = True
ListFolderLongpollArg.timeout.default = 30
ListRevisionsArg.mode.default = ListRevisionsMode.path
ListRevisionsArg.limit.default = 10
RelocationBatchArgBase.autorename.default = False
MoveBatchArg.allow_ownership_transfer.default = False
RelocationArg.allow_shared_folder.default = False
RelocationArg.autorename.default = False
RelocationArg.allow_ownership_transfer.default = False
RelocationBatchArg.allow_shared_folder.default = False
RelocationBatchArg.allow_ownership_transfer.default = False
SearchArg.start.default = 0
SearchArg.max_results.default = 100
SearchArg.mode.default = SearchMode.filename
SearchMatchFieldOptions.include_highlights.default = False
SearchOptions.max_results.default = 100
SearchOptions.file_status.default = FileStatus.active
SearchOptions.filename_only.default = False
ThumbnailArg.format.default = ThumbnailFormat.jpeg
ThumbnailArg.size.default = ThumbnailSize.w64h64
ThumbnailArg.mode.default = ThumbnailMode.strict
ThumbnailV2Arg.format.default = ThumbnailFormat.jpeg
ThumbnailV2Arg.size.default = ThumbnailSize.w64h64
ThumbnailV2Arg.mode.default = ThumbnailMode.strict
UploadSessionAppendArg.close.default = False
UploadSessionStartArg.close.default = False
alpha_get_metadata = bb.Route(
    'alpha/get_metadata',
    AlphaGetMetadataArg_validator,
    Metadata_validator,
    AlphaGetMetadataError_validator,
alpha_upload = bb.Route(
    'alpha/upload',
    UploadArg_validator,
    FileMetadata_validator,
    UploadError_validator,
     'host': 'content',
     'style': 'upload'},
copy_v2 = bb.Route(
    RelocationArg_validator,
    RelocationResult_validator,
    RelocationError_validator,
copy = bb.Route(
copy_batch_v2 = bb.Route(
    'copy_batch',
    CopyBatchArg_validator,
    RelocationBatchV2Launch_validator,
copy_batch = bb.Route(
    RelocationBatchArg_validator,
    RelocationBatchLaunch_validator,
copy_batch_check_v2 = bb.Route(
    'copy_batch/check',
    async_.PollArg_validator,
    RelocationBatchV2JobStatus_validator,
    async_.PollError_validator,
copy_batch_check = bb.Route(
    RelocationBatchJobStatus_validator,
copy_reference_get = bb.Route(
    'copy_reference/get',
    GetCopyReferenceArg_validator,
    GetCopyReferenceResult_validator,
    GetCopyReferenceError_validator,
copy_reference_save = bb.Route(
    'copy_reference/save',
    SaveCopyReferenceArg_validator,
    SaveCopyReferenceResult_validator,
    SaveCopyReferenceError_validator,
create_folder_v2 = bb.Route(
    'create_folder',
    CreateFolderArg_validator,
    CreateFolderResult_validator,
    CreateFolderError_validator,
create_folder = bb.Route(
    FolderMetadata_validator,
create_folder_batch = bb.Route(
    'create_folder_batch',
    CreateFolderBatchArg_validator,
    CreateFolderBatchLaunch_validator,
create_folder_batch_check = bb.Route(
    'create_folder_batch/check',
    CreateFolderBatchJobStatus_validator,
delete_v2 = bb.Route(
    DeleteArg_validator,
    DeleteResult_validator,
    DeleteError_validator,
delete_batch = bb.Route(
    'delete_batch',
    DeleteBatchArg_validator,
    DeleteBatchLaunch_validator,
delete_batch_check = bb.Route(
    'delete_batch/check',
    DeleteBatchJobStatus_validator,
download = bb.Route(
    'download',
    DownloadArg_validator,
    DownloadError_validator,
     'style': 'download'},
download_zip = bb.Route(
    'download_zip',
    DownloadZipArg_validator,
    DownloadZipResult_validator,
    DownloadZipError_validator,
export = bb.Route(
    'export',
    ExportArg_validator,
    ExportResult_validator,
    ExportError_validator,
get_file_lock_batch = bb.Route(
    'get_file_lock_batch',
    LockFileBatchArg_validator,
    LockFileBatchResult_validator,
    LockFileError_validator,
get_metadata = bb.Route(
    'get_metadata',
    GetMetadataArg_validator,
    GetMetadataError_validator,
get_preview = bb.Route(
    'get_preview',
    PreviewArg_validator,
    PreviewError_validator,
get_temporary_link = bb.Route(
    'get_temporary_link',
    GetTemporaryLinkArg_validator,
    GetTemporaryLinkResult_validator,
    GetTemporaryLinkError_validator,
get_temporary_upload_link = bb.Route(
    'get_temporary_upload_link',
    GetTemporaryUploadLinkArg_validator,
    GetTemporaryUploadLinkResult_validator,
get_thumbnail = bb.Route(
    'get_thumbnail',
    ThumbnailArg_validator,
    ThumbnailError_validator,
get_thumbnail_v2 = bb.Route(
    ThumbnailV2Arg_validator,
    PreviewResult_validator,
    ThumbnailV2Error_validator,
    {'auth': 'app, user',
get_thumbnail_batch = bb.Route(
    'get_thumbnail_batch',
    GetThumbnailBatchArg_validator,
    GetThumbnailBatchResult_validator,
    GetThumbnailBatchError_validator,
list_folder = bb.Route(
    'list_folder',
    ListFolderArg_validator,
    ListFolderResult_validator,
    ListFolderError_validator,
list_folder_continue = bb.Route(
    'list_folder/continue',
    ListFolderContinueArg_validator,
    ListFolderContinueError_validator,
list_folder_get_latest_cursor = bb.Route(
    'list_folder/get_latest_cursor',
    ListFolderGetLatestCursorResult_validator,
list_folder_longpoll = bb.Route(
    'list_folder/longpoll',
    ListFolderLongpollArg_validator,
    ListFolderLongpollResult_validator,
    ListFolderLongpollError_validator,
    {'auth': 'noauth',
     'host': 'notify',
list_revisions = bb.Route(
    'list_revisions',
    ListRevisionsArg_validator,
    ListRevisionsResult_validator,
    ListRevisionsError_validator,
lock_file_batch = bb.Route(
    'lock_file_batch',
move_v2 = bb.Route(
    'move',
move = bb.Route(
move_batch_v2 = bb.Route(
    'move_batch',
    MoveBatchArg_validator,
move_batch = bb.Route(
move_batch_check_v2 = bb.Route(
    'move_batch/check',
move_batch_check = bb.Route(
paper_create = bb.Route(
    'paper/create',
    PaperCreateArg_validator,
    PaperCreateResult_validator,
    PaperCreateError_validator,
paper_update = bb.Route(
    'paper/update',
    PaperUpdateArg_validator,
    PaperUpdateResult_validator,
    PaperUpdateError_validator,
permanently_delete = bb.Route(
    'permanently_delete',
    file_properties.AddPropertiesArg_validator,
    file_properties.AddPropertiesError_validator,
    file_properties.OverwritePropertyGroupArg_validator,
    file_properties.InvalidPropertyGroupError_validator,
    file_properties.RemovePropertiesArg_validator,
    file_properties.RemovePropertiesError_validator,
properties_template_get = bb.Route(
    'properties/template/get',
    file_properties.GetTemplateArg_validator,
    file_properties.GetTemplateResult_validator,
    file_properties.TemplateError_validator,
properties_template_list = bb.Route(
    'properties/template/list',
    file_properties.ListTemplateResult_validator,
    file_properties.UpdatePropertiesArg_validator,
    file_properties.UpdatePropertiesError_validator,
restore = bb.Route(
    'restore',
    RestoreArg_validator,
    RestoreError_validator,
save_url = bb.Route(
    'save_url',
    SaveUrlArg_validator,
    SaveUrlResult_validator,
    SaveUrlError_validator,
save_url_check_job_status = bb.Route(
    'save_url/check_job_status',
    SaveUrlJobStatus_validator,
search = bb.Route(
    'search',
    SearchArg_validator,
    SearchResult_validator,
    SearchError_validator,
search_v2 = bb.Route(
    SearchV2Arg_validator,
    SearchV2Result_validator,
search_continue_v2 = bb.Route(
    'search/continue',
    SearchV2ContinueArg_validator,
tags_add = bb.Route(
    'tags/add',
    AddTagArg_validator,
    AddTagError_validator,
tags_get = bb.Route(
    'tags/get',
    GetTagsArg_validator,
    GetTagsResult_validator,
    BaseTagError_validator,
tags_remove = bb.Route(
    'tags/remove',
    RemoveTagArg_validator,
    RemoveTagError_validator,
unlock_file_batch = bb.Route(
    'unlock_file_batch',
    UnlockFileBatchArg_validator,
upload = bb.Route(
    'upload',
upload_session_append_v2 = bb.Route(
    'upload_session/append',
    UploadSessionAppendArg_validator,
    UploadSessionAppendError_validator,
upload_session_append = bb.Route(
    UploadSessionCursor_validator,
upload_session_finish = bb.Route(
    'upload_session/finish',
    UploadSessionFinishArg_validator,
    UploadSessionFinishError_validator,
upload_session_finish_batch = bb.Route(
    'upload_session/finish_batch',
    UploadSessionFinishBatchArg_validator,
    UploadSessionFinishBatchLaunch_validator,
upload_session_finish_batch_v2 = bb.Route(
    UploadSessionFinishBatchResult_validator,
upload_session_finish_batch_check = bb.Route(
    'upload_session/finish_batch/check',
    UploadSessionFinishBatchJobStatus_validator,
upload_session_start = bb.Route(
    'upload_session/start',
    UploadSessionStartArg_validator,
    UploadSessionStartResult_validator,
    UploadSessionStartError_validator,
upload_session_start_batch = bb.Route(
    'upload_session/start_batch',
    UploadSessionStartBatchArg_validator,
    UploadSessionStartBatchResult_validator,
    'alpha/get_metadata': alpha_get_metadata,
    'alpha/upload': alpha_upload,
    'copy:2': copy_v2,
    'copy': copy,
    'copy_batch:2': copy_batch_v2,
    'copy_batch': copy_batch,
    'copy_batch/check:2': copy_batch_check_v2,
    'copy_batch/check': copy_batch_check,
    'copy_reference/get': copy_reference_get,
    'copy_reference/save': copy_reference_save,
    'create_folder:2': create_folder_v2,
    'create_folder': create_folder,
    'create_folder_batch': create_folder_batch,
    'create_folder_batch/check': create_folder_batch_check,
    'delete:2': delete_v2,
    'delete_batch': delete_batch,
    'delete_batch/check': delete_batch_check,
    'download': download,
    'download_zip': download_zip,
    'export': export,
    'get_file_lock_batch': get_file_lock_batch,
    'get_metadata': get_metadata,
    'get_preview': get_preview,
    'get_temporary_link': get_temporary_link,
    'get_temporary_upload_link': get_temporary_upload_link,
    'get_thumbnail': get_thumbnail,
    'get_thumbnail:2': get_thumbnail_v2,
    'get_thumbnail_batch': get_thumbnail_batch,
    'list_folder': list_folder,
    'list_folder/continue': list_folder_continue,
    'list_folder/get_latest_cursor': list_folder_get_latest_cursor,
    'list_folder/longpoll': list_folder_longpoll,
    'list_revisions': list_revisions,
    'lock_file_batch': lock_file_batch,
    'move:2': move_v2,
    'move': move,
    'move_batch:2': move_batch_v2,
    'move_batch': move_batch,
    'move_batch/check:2': move_batch_check_v2,
    'move_batch/check': move_batch_check,
    'paper/create': paper_create,
    'paper/update': paper_update,
    'permanently_delete': permanently_delete,
    'properties/template/get': properties_template_get,
    'properties/template/list': properties_template_list,
    'restore': restore,
    'save_url': save_url,
    'save_url/check_job_status': save_url_check_job_status,
    'search': search,
    'search:2': search_v2,
    'search/continue:2': search_continue_v2,
    'tags/add': tags_add,
    'tags/get': tags_get,
    'tags/remove': tags_remove,
    'unlock_file_batch': unlock_file_batch,
    'upload': upload,
    'upload_session/append:2': upload_session_append_v2,
    'upload_session/append': upload_session_append,
    'upload_session/finish': upload_session_finish,
    'upload_session/finish_batch': upload_session_finish_batch,
    'upload_session/finish_batch:2': upload_session_finish_batch_v2,
    'upload_session/finish_batch/check': upload_session_finish_batch_check,
    'upload_session/start': upload_session_start,
    'upload_session/start_batch': upload_session_start_batch,
from . import Error, Tags, register
@register(Tags.files)
def check_setting_file_upload_temp_dir(app_configs, **kwargs):
    setting = getattr(settings, "FILE_UPLOAD_TEMP_DIR", None)
    if setting and not Path(setting).is_dir():
            Error(
                f"The FILE_UPLOAD_TEMP_DIR setting refers to the nonexistent "
                f"directory '{setting}'.",
                id="files.E001",
from django.core.files.base import ContentFile, File
from django.core.files.images import ImageFile
from django.core.files.storage import Storage, default_storage
from django.db.models.fields import Field
from django.db.models.query_utils import DeferredAttribute
from django.db.models.utils import AltersData
class FieldFile(File, AltersData):
    def __init__(self, instance, field, name):
        super().__init__(None, name)
        self.instance = instance
        self.field = field
        self.storage = field.storage
        self._committed = True
        # Older code may be expecting FileField values to be simple strings.
        # By overriding the == operator, it can remain backwards compatibility.
        if hasattr(other, "name"):
        return self.name == other
    # The standard File contains most of the necessary properties, but
    # FieldFiles can be instantiated without a name, so that needs to
    # be checked for here.
    def _require_file(self):
        if not self:
                "The '%s' attribute has no file associated with it." % self.field.name
    def _get_file(self):
        self._require_file()
        if getattr(self, "_file", None) is None:
            self._file = self.storage.open(self.name, "rb")
        return self._file
    def _set_file(self, file):
        self._file = file
    def _del_file(self):
        del self._file
    file = property(_get_file, _set_file, _del_file)
        return self.storage.path(self.name)
        return self.storage.url(self.name)
        if not self._committed:
        return self.storage.size(self.name)
    def open(self, mode="rb"):
            self.file = self.storage.open(self.name, mode)
            self.file.open(mode)
    # open() doesn't alter the file's contents, but it does reset the pointer
    open.alters_data = True
    # In addition to the standard File API, FieldFiles have extra methods
    # to further manipulate the underlying file, as well as update the
    # associated model instance.
    def _set_instance_attribute(self, name, content):
        setattr(self.instance, self.field.attname, name)
    def save(self, name, content, save=True):
        name = self.field.generate_filename(self.instance, name)
        self.name = self.storage.save(name, content, max_length=self.field.max_length)
        self._set_instance_attribute(self.name, content)
        # Save the object because it has changed, unless save is False
        if save:
            self.instance.save()
    def delete(self, save=True):
        # Only close the file if it's already open, which we know by the
        # presence of self._file
        if hasattr(self, "_file"):
            del self.file
        self.storage.delete(self.name)
        self.name = None
        setattr(self.instance, self.field.attname, self.name)
        self._committed = False
        file = getattr(self, "_file", None)
        return file is None or file.closed
        # FieldFile needs access to its associated model field, an instance and
        # the file's name. Everything else will be restored later, by
        # FileDescriptor below.
            "closed": False,
            "_committed": True,
            "_file": None,
            "field": self.field,
        self.storage = self.field.storage
class FileDescriptor(DeferredAttribute):
    The descriptor for the file attribute on the model instance. Return a
    FieldFile when accessed so you can write code like::
        >>> from myapp.models import MyModel
        >>> instance = MyModel.objects.get(pk=1)
        >>> instance.file.size
    Assign a file object on assignment so you can do::
        >>> with open('/path/to/hello.world') as f:
        ...     instance.file = File(f)
        # This is slightly complicated, so worth an explanation.
        # instance.file needs to ultimately return some instance of `File`,
        # probably a subclass. Additionally, this returned object needs to have
        # the FieldFile API so that users can easily do things like
        # instance.file.path and have that delegated to the file storage
        # engine. Easy enough if we're strict about assignment in __set__, but
        # if you peek below you can see that we're not. So depending on the
        # current value of the field we have to dynamically construct some sort
        # of "thing" to return.
        # The instance dict contains whatever was originally assigned
        # in __set__.
        file = super().__get__(instance, cls)
        # If this value is a string (instance.file = "path/to/file") or None
        # then we simply wrap it with the appropriate attribute class according
        # to the file field. [This is FieldFile for FileFields and
        # ImageFieldFile for ImageFields; it's also conceivable that user
        # subclasses might also want to subclass the attribute class]. This
        # object understands how to convert a path to a file, and also how to
        # handle None.
        if isinstance(file, str) or file is None:
            attr = self.field.attr_class(instance, self.field, file)
            instance.__dict__[self.field.attname] = attr
        # If this value is a DatabaseDefault, initialize the attribute class
        # for this field with its db_default value.
        elif isinstance(file, DatabaseDefault):
            attr = self.field.attr_class(instance, self.field, self.field.db_default)
        # Other types of files may be assigned as well, but they need to have
        # the FieldFile interface added to them. Thus, we wrap any other type
        # of File inside a FieldFile (well, the field's attr_class, which is
        # usually FieldFile).
        elif isinstance(file, File) and not isinstance(file, FieldFile):
            file_copy = self.field.attr_class(instance, self.field, file.name)
            file_copy.file = file
            file_copy._committed = False
            instance.__dict__[self.field.attname] = file_copy
        # Finally, because of the (some would say boneheaded) way pickle works,
        # the underlying FieldFile might not actually itself have an associated
        # file. So we need to reset the details of the FieldFile in those
        # cases.
        elif isinstance(file, FieldFile) and not hasattr(file, "field"):
            file.instance = instance
            file.field = self.field
            file.storage = self.field.storage
        # Make sure that the instance is correct.
        elif isinstance(file, FieldFile) and instance is not file.instance:
        # That was fun, wasn't it?
        return instance.__dict__[self.field.attname]
    def __set__(self, instance, value):
        instance.__dict__[self.field.attname] = value
class FileField(Field):
    # The class to wrap instance attributes in. Accessing the file object off
    # the instance will always return an instance of attr_class.
    attr_class = FieldFile
    # The descriptor to use for accessing the attribute off of the class.
    descriptor_class = FileDescriptor
    description = _("File")
        self, verbose_name=None, name=None, upload_to="", storage=None, **kwargs
        self._primary_key_set_explicitly = "primary_key" in kwargs
        self.storage = storage if storage is not None else default_storage
        if callable(self.storage):
            # Hold a reference to the callable for deconstruct().
            self._storage_callable = self.storage
            self.storage = self.storage()
            if not isinstance(self.storage, Storage):
                    "%s.storage must be a subclass/instance of %s.%s"
                        Storage.__module__,
                        Storage.__qualname__,
        self.upload_to = upload_to
            *self._check_upload_to(),
        if self._primary_key_set_explicitly:
                    "'primary_key' is not a valid argument for a %s."
                    id="fields.E201",
    def _check_upload_to(self):
        if isinstance(self.upload_to, str) and self.upload_to.startswith("/"):
                    "%s's 'upload_to' argument must be a relative path, not an "
                    "absolute path." % self.__class__.__name__,
                    id="fields.E202",
                    hint="Remove the leading slash.",
        kwargs["upload_to"] = self.upload_to
        storage = getattr(self, "_storage_callable", self.storage)
        if storage is not default_storage:
            kwargs["storage"] = storage
        return "FileField"
        # Need to convert File objects provided via a form to string for
        # database insertion.
        file = super().pre_save(model_instance, add)
        if file.name is None and file._file is not None:
            exc = FieldError(
                f"File for {self.name} must have "
                "the name attribute specified to be saved."
            if isinstance(file._file, ContentFile):
                exc.add_note("Pass a 'name' argument to ContentFile.")
        if file and not file._committed:
            # Commit the file to storage prior to saving the model
            file.save(file.name, file.file, save=False)
    def generate_filename(self, instance, filename):
        Apply (if callable) or prepend (if a string) upload_to to the filename,
        then delegate further processing of the name to the storage backend.
        Until the storage layer, all file paths are expected to be Unix style
        (with forward slashes).
        if callable(self.upload_to):
            filename = self.upload_to(instance, filename)
            dirname = datetime.datetime.now().strftime(str(self.upload_to))
            filename = posixpath.join(dirname, filename)
        filename = validate_file_name(filename, allow_relative_path=True)
        return self.storage.generate_filename(filename)
        # Important: None means "no change", other false value means "clear"
        # This subtle distinction (rather than a more explicit marker) is
        # needed because we need to consume values that are also sane for a
        # regular (non Model-) Form to find in its cleaned_data dictionary.
            # This value will be converted to str and stored in the
            # database, so leaving False as-is is not acceptable.
            setattr(instance, self.name, data or "")
                "form_class": forms.FileField,
class ImageFileDescriptor(FileDescriptor):
    Just like the FileDescriptor, but for ImageFields. The only difference is
    assigning the width/height to the width_field/height_field, if appropriate.
        previous_file = instance.__dict__.get(self.field.attname)
        super().__set__(instance, value)
        # To prevent recalculating image dimensions when we are instantiating
        # an object from the database (bug #11084), only update dimensions if
        # the field had a value before this assignment. Since the default
        # value for FileField subclasses is an instance of field.attr_class,
        # previous_file will only be None when we are called from
        # Model.__init__(). The ImageField.update_dimension_fields method
        # hooked up to the post_init signal handles the Model.__init__() cases.
        # Assignment happening outside of Model.__init__() will trigger the
        # update right here.
        if previous_file is not None:
            self.field.update_dimension_fields(instance, force=True)
class ImageFieldFile(ImageFile, FieldFile):
        setattr(self.instance, self.field.attname, content)
        # Update the name in case generate_filename() or storage.save() changed
        # it, but bypass the descriptor to avoid re-reading the file.
        self.instance.__dict__[self.field.attname] = self.name
        # Clear the image dimensions cache
        if hasattr(self, "_dimensions_cache"):
            del self._dimensions_cache
        super().delete(save)
class ImageField(FileField):
    attr_class = ImageFieldFile
    descriptor_class = ImageFileDescriptor
    description = _("Image")
        width_field=None,
        height_field=None,
        self.width_field, self.height_field = width_field, height_field
            *self._check_image_library_installed(),
    def _check_image_library_installed(self):
            from PIL import Image  # NOQA
                    "Cannot use ImageField because Pillow is not installed.",
                        "Get Pillow at https://pypi.org/project/Pillow/ "
                        'or run command "python -m pip install Pillow".'
                    id="fields.E210",
        if self.width_field:
            kwargs["width_field"] = self.width_field
        if self.height_field:
            kwargs["height_field"] = self.height_field
        # Attach update_dimension_fields so that dimension fields declared
        # after their corresponding image field don't stay cleared by
        # Model.__init__, see bug #11196.
        # Only run post-initialization dimension update on non-abstract models
        # with width_field/height_field.
        if not cls._meta.abstract and (self.width_field or self.height_field):
            signals.post_init.connect(self.update_dimension_fields, sender=cls)
    def update_dimension_fields(self, instance, force=False, *args, **kwargs):
        Update field's width and height fields, if defined.
        This method is hooked up to model's post_init signal to update
        dimensions after instantiating a model instance. However, dimensions
        won't be updated if the dimensions fields are already populated. This
        avoids unnecessary recalculation when loading an object from the
        database.
        Dimensions can be forced to update with force=True, which is how
        ImageFileDescriptor.__set__ calls this method.
        # Nothing to update if the field doesn't have dimension fields or if
        # the field is deferred.
        has_dimension_fields = self.width_field or self.height_field
        if not has_dimension_fields or self.attname not in instance.__dict__:
        # getattr will call the ImageFileDescriptor's __get__ method, which
        # coerces the assigned value into an instance of self.attr_class
        # (ImageFieldFile in this case).
        file = getattr(instance, self.attname)
        # Nothing to update if we have no file and not being forced to update.
        if not file and not force:
        dimension_fields_filled = not (
            (self.width_field and not getattr(instance, self.width_field))
            or (self.height_field and not getattr(instance, self.height_field))
        # When both dimension fields have values, we are most likely loading
        # data from the database or updating an image field that already had
        # an image stored. In the first case, we don't want to update the
        # dimension fields because we are already getting their values from the
        # database. In the second case, we do want to update the dimensions
        # fields and will skip this return because force will be True since we
        # were called from ImageFileDescriptor.__set__.
        if dimension_fields_filled and not force:
        # file should be an instance of ImageFieldFile or should be None.
        if file:
            width = file.width
            height = file.height
            # No file, so clear dimensions fields.
            width = None
            height = None
        # Update the width and height fields.
            setattr(instance, self.width_field, width)
            setattr(instance, self.height_field, height)
                "form_class": forms.ImageField,
from .._utils import extract_files, maybe_transform, deepcopy_minimal, async_maybe_transform
            f"/files/{file_id}",
            f"/files/{file_id}/content",
from ...._utils import extract_files, maybe_transform, deepcopy_minimal, async_maybe_transform
            f"/containers/{container_id}/files",
            f"/containers/{container_id}/files/{file_id}",
            f"/vector_stores/{vector_store_id}/files",
            f"/vector_stores/{vector_store_id}/files/{file_id}",
            f"/vector_stores/{vector_store_id}/files/{file_id}/content",
from types import MappingProxyType
from typing import Any, Dict, List, Literal, Mapping, Set, Union
Base Enums/Consts
class FileType(Enum):
    AAC = "AAC"
    CSV = "CSV"
    DOC = "DOC"
    DOCX = "DOCX"
    FLAC = "FLAC"
    FLV = "FLV"
    GIF = "GIF"
    GOOGLE_DOC = "GOOGLE_DOC"
    GOOGLE_DRAWINGS = "GOOGLE_DRAWINGS"
    GOOGLE_SHEETS = "GOOGLE_SHEETS"
    GOOGLE_SLIDES = "GOOGLE_SLIDES"
    HEIC = "HEIC"
    HEIF = "HEIF"
    HTML = "HTML"
    JPEG = "JPEG"
    JSON = "JSON"
    M4A = "M4A"
    M4V = "M4V"
    MOV = "MOV"
    MP3 = "MP3"
    MP4 = "MP4"
    MPEG = "MPEG"
    MPEGPS = "MPEGPS"
    MPG = "MPG"
    MPA = "MPA"
    MPGA = "MPGA"
    OGG = "OGG"
    OPUS = "OPUS"
    PDF = "PDF"
    PCM = "PCM"
    PNG = "PNG"
    PPT = "PPT"
    PPTX = "PPTX"
    RTF = "RTF"
    THREE_GPP = "3GPP"
    TXT = "TXT"
    WAV = "WAV"
    WEBM = "WEBM"
    WEBP = "WEBP"
    WMV = "WMV"
    XLS = "XLS"
    XLSX = "XLSX"
FILE_EXTENSIONS: Mapping[FileType, List[str]] = MappingProxyType(
        FileType.AAC: ["aac"],
        FileType.CSV: ["csv"],
        FileType.DOC: ["doc"],
        FileType.DOCX: ["docx"],
        FileType.FLAC: ["flac"],
        FileType.FLV: ["flv"],
        FileType.GIF: ["gif"],
        FileType.GOOGLE_DOC: ["gdoc"],
        FileType.GOOGLE_DRAWINGS: ["gdraw"],
        FileType.GOOGLE_SHEETS: ["gsheet"],
        FileType.GOOGLE_SLIDES: ["gslides"],
        FileType.HEIC: ["heic"],
        FileType.HEIF: ["heif"],
        FileType.HTML: ["html", "htm"],
        FileType.JPEG: ["jpeg", "jpg"],
        FileType.JSON: ["json"],
        FileType.M4A: ["m4a"],
        FileType.M4V: ["m4v"],
        FileType.MOV: ["mov"],
        FileType.MP3: ["mp3"],
        FileType.MP4: ["mp4"],
        FileType.MPEG: ["mpeg"],
        FileType.MPEGPS: ["mpegps"],
        FileType.MPG: ["mpg"],
        FileType.MPA: ["mpa"],
        FileType.MPGA: ["mpga"],
        FileType.OGG: ["ogg"],
        FileType.OPUS: ["opus"],
        FileType.PDF: ["pdf"],
        FileType.PCM: ["pcm"],
        FileType.PNG: ["png"],
        FileType.PPT: ["ppt"],
        FileType.PPTX: ["pptx"],
        FileType.RTF: ["rtf"],
        FileType.THREE_GPP: ["3gpp"],
        FileType.TXT: ["txt"],
        FileType.WAV: ["wav"],
        FileType.WEBM: ["webm"],
        FileType.WEBP: ["webp"],
        FileType.WMV: ["wmv"],
        FileType.XLS: ["xls"],
        FileType.XLSX: ["xlsx"],
FILE_MIME_TYPES: Mapping[FileType, str] = MappingProxyType(
        FileType.AAC: "audio/aac",
        FileType.CSV: "text/csv",
        FileType.DOC: "application/msword",
        FileType.DOCX: "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        FileType.FLAC: "audio/flac",
        FileType.FLV: "video/x-flv",
        FileType.GIF: "image/gif",
        FileType.GOOGLE_DOC: "application/vnd.google-apps.document",
        FileType.GOOGLE_DRAWINGS: "application/vnd.google-apps.drawing",
        FileType.GOOGLE_SHEETS: "application/vnd.google-apps.spreadsheet",
        FileType.GOOGLE_SLIDES: "application/vnd.google-apps.presentation",
        FileType.HEIC: "image/heic",
        FileType.HEIF: "image/heif",
        FileType.HTML: "text/html",
        FileType.JPEG: "image/jpeg",
        FileType.JSON: "application/json",
        FileType.M4A: "audio/x-m4a",
        FileType.M4V: "video/x-m4v",
        FileType.MOV: "video/quicktime",
        FileType.MP3: "audio/mpeg",
        FileType.MP4: "video/mp4",
        FileType.MPEG: "video/mpeg",
        FileType.MPEGPS: "video/mpegps",
        FileType.MPG: "video/mpg",
        FileType.MPA: "audio/m4a",
        FileType.MPGA: "audio/mpga",
        FileType.OGG: "audio/ogg",
        FileType.OPUS: "audio/opus",
        FileType.PDF: "application/pdf",
        FileType.PCM: "audio/pcm",
        FileType.PNG: "image/png",
        FileType.PPT: "application/vnd.ms-powerpoint",
        FileType.PPTX: "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        FileType.RTF: "application/rtf",
        FileType.THREE_GPP: "video/3gpp",
        FileType.TXT: "text/plain",
        FileType.WAV: "audio/wav",
        FileType.WEBM: "video/webm",
        FileType.WEBP: "image/webp",
        FileType.WMV: "video/wmv",
        FileType.XLS: "application/vnd.ms-excel",
        FileType.XLSX: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
Util Functions
def get_file_extension_from_mime_type(mime_type: str) -> str:
    for file_type, mime in FILE_MIME_TYPES.items():
        if mime.lower() == mime_type.lower():
            return FILE_EXTENSIONS[file_type][0]
    raise ValueError(f"Unknown extension for mime type: {mime_type}")
def get_file_type_from_extension(extension: str) -> FileType:
    for file_type, extensions in FILE_EXTENSIONS.items():
        if extension.lower() in extensions:
            return file_type
    raise ValueError(f"Unknown file type for extension: {extension}")
def get_file_extension_for_file_type(file_type: FileType) -> str:
def get_file_mime_type_for_file_type(file_type: FileType) -> str:
def get_file_mime_type_from_extension(extension: str) -> str:
    file_type = get_file_type_from_extension(extension)
    return get_file_mime_type_for_file_type(file_type)
FileType Type Groupings (Videos, Images, etc)
# Images
IMAGE_FILE_TYPES = {
    FileType.PNG,
    FileType.JPEG,
    FileType.GIF,
    FileType.WEBP,
    FileType.HEIC,
    FileType.HEIF,
def is_image_file_type(file_type):
    return file_type in IMAGE_FILE_TYPES
# Videos
VIDEO_FILE_TYPES = {
    FileType.MOV,
    FileType.MP4,
    FileType.MPEG,
    FileType.M4V,
    FileType.FLV,
    FileType.MPEGPS,
    FileType.MPG,
    FileType.WEBM,
    FileType.WMV,
    FileType.THREE_GPP,
def is_video_file_type(file_type):
    return file_type in VIDEO_FILE_TYPES
# Audio
AUDIO_FILE_TYPES = {
    FileType.AAC,
    FileType.FLAC,
    FileType.MP3,
    FileType.MPA,
    FileType.MPGA,
    FileType.OPUS,
    FileType.PCM,
    FileType.WAV,
def is_audio_file_type(file_type):
    return file_type in AUDIO_FILE_TYPES
TEXT_FILE_TYPES = {FileType.CSV, FileType.HTML, FileType.RTF, FileType.TXT}
def is_text_file_type(file_type):
    return file_type in TEXT_FILE_TYPES
Other FileType Groupings
# Accepted file types for GEMINI 1.5 through Vertex AI
# https://cloud.google.com/vertex-ai/generative-ai/docs/multimodal/send-multimodal-prompts#gemini-send-multimodal-samples-images-nodejs
GEMINI_1_5_ACCEPTED_FILE_TYPES: Set[FileType] = {
    # Image
    # Video
    # PDF
    FileType.PDF,
    FileType.TXT,
def is_gemini_1_5_accepted_file_type(file_type: FileType) -> bool:
    return file_type in GEMINI_1_5_ACCEPTED_FILE_TYPES
Two-Step File Upload Types
class TwoStepFileUploadRequest(TypedDict):
    Request structure for two-step file upload process.
    Step 1: Initial request to get upload URL
    Step 2: Upload file content to the upload URL
    Used by providers like Manus and Google Cloud Storage.
    headers: Required[Dict[str, str]]
    data: Required[Union[str, bytes, Dict[str, Any]]]
class TwoStepFileUploadConfig(TypedDict, total=False):
    Configuration for two-step file upload process.
    Properties:
        initial_request: Request to create file record and get upload URL
        upload_request: Request to upload actual file content
        upload_url_location: Where to find upload URL ('headers' or 'body')
        upload_url_key: Key name for upload URL in response (default: 'upload_url')
    initial_request: Required[TwoStepFileUploadRequest]
    upload_request: Required[TwoStepFileUploadRequest]
    upload_url_location: Required[Literal["headers", "body"]]
    upload_url_key: str
