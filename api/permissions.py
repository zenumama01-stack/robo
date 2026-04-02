from ....pagination import SyncPage, AsyncPage, SyncConversationCursorPage, AsyncConversationCursorPage
from ....types.fine_tuning.checkpoints import (
    permission_list_params,
    permission_create_params,
    permission_retrieve_params,
from ....types.fine_tuning.checkpoints.permission_list_response import PermissionListResponse
from ....types.fine_tuning.checkpoints.permission_create_response import PermissionCreateResponse
from ....types.fine_tuning.checkpoints.permission_delete_response import PermissionDeleteResponse
from ....types.fine_tuning.checkpoints.permission_retrieve_response import PermissionRetrieveResponse
__all__ = ["Permissions", "AsyncPermissions"]
class Permissions(SyncAPIResource):
    def with_raw_response(self) -> PermissionsWithRawResponse:
        return PermissionsWithRawResponse(self)
    def with_streaming_response(self) -> PermissionsWithStreamingResponse:
        return PermissionsWithStreamingResponse(self)
        fine_tuned_model_checkpoint: str,
        project_ids: SequenceNotStr[str],
    ) -> SyncPage[PermissionCreateResponse]:
        **NOTE:** Calling this endpoint requires an [admin API key](../admin-api-keys).
        This enables organization owners to share fine-tuned models with other projects
        in their organization.
          project_ids: The project identifiers to grant access to.
        if not fine_tuned_model_checkpoint:
                f"Expected a non-empty value for `fine_tuned_model_checkpoint` but received {fine_tuned_model_checkpoint!r}"
                "/fine_tuning/checkpoints/{fine_tuned_model_checkpoint}/permissions",
                fine_tuned_model_checkpoint=fine_tuned_model_checkpoint,
            page=SyncPage[PermissionCreateResponse],
            body=maybe_transform({"project_ids": project_ids}, permission_create_params.PermissionCreateParams),
            model=PermissionCreateResponse,
            method="post",
    @typing_extensions.deprecated("Retrieve is deprecated. Please swap to the paginated list method instead.")
        order: Literal["ascending", "descending"] | Omit = omit,
        project_id: str | Omit = omit,
    ) -> PermissionRetrieveResponse:
        **NOTE:** This endpoint requires an [admin API key](../admin-api-keys).
        Organization owners can use this endpoint to view all permissions for a
        fine-tuned model checkpoint.
          after: Identifier for the last permission ID from the previous pagination request.
          limit: Number of permissions to retrieve.
          order: The order in which to retrieve permissions.
          project_id: The ID of the project to get permissions for.
                        "project_id": project_id,
                    permission_retrieve_params.PermissionRetrieveParams,
            cast_to=PermissionRetrieveResponse,
    ) -> SyncConversationCursorPage[PermissionListResponse]:
            page=SyncConversationCursorPage[PermissionListResponse],
                    permission_list_params.PermissionListParams,
            model=PermissionListResponse,
        permission_id: str,
    ) -> PermissionDeleteResponse:
        Organization owners can use this endpoint to delete a permission for a
        if not permission_id:
            raise ValueError(f"Expected a non-empty value for `permission_id` but received {permission_id!r}")
                "/fine_tuning/checkpoints/{fine_tuned_model_checkpoint}/permissions/{permission_id}",
                permission_id=permission_id,
            cast_to=PermissionDeleteResponse,
class AsyncPermissions(AsyncAPIResource):
    def with_raw_response(self) -> AsyncPermissionsWithRawResponse:
        return AsyncPermissionsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncPermissionsWithStreamingResponse:
        return AsyncPermissionsWithStreamingResponse(self)
    ) -> AsyncPaginator[PermissionCreateResponse, AsyncPage[PermissionCreateResponse]]:
            page=AsyncPage[PermissionCreateResponse],
    ) -> AsyncPaginator[PermissionListResponse, AsyncConversationCursorPage[PermissionListResponse]]:
            page=AsyncConversationCursorPage[PermissionListResponse],
class PermissionsWithRawResponse:
    def __init__(self, permissions: Permissions) -> None:
        self._permissions = permissions
            permissions.create,
                permissions.retrieve,  # pyright: ignore[reportDeprecated],
            permissions.list,
            permissions.delete,
class AsyncPermissionsWithRawResponse:
    def __init__(self, permissions: AsyncPermissions) -> None:
class PermissionsWithStreamingResponse:
class AsyncPermissionsWithStreamingResponse:
from ....pagination import SyncPage, AsyncPage
from ....types.fine_tuning.checkpoints import permission_create_params, permission_retrieve_params
            f"/fine_tuning/checkpoints/{fine_tuned_model_checkpoint}/permissions",
            f"/fine_tuning/checkpoints/{fine_tuned_model_checkpoint}/permissions/{permission_id}",
            permissions.retrieve,
