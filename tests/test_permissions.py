from openai.pagination import SyncPage, AsyncPage, SyncConversationCursorPage, AsyncConversationCursorPage
from openai.types.fine_tuning.checkpoints import (
    PermissionListResponse,
    PermissionCreateResponse,
    PermissionDeleteResponse,
    PermissionRetrieveResponse,
class TestPermissions:
        permission = client.fine_tuning.checkpoints.permissions.create(
            fine_tuned_model_checkpoint="ft:gpt-4o-mini-2024-07-18:org:weather:B7R9VjQd",
            project_ids=["string"],
        assert_matches_type(SyncPage[PermissionCreateResponse], permission, path=["response"])
        response = client.fine_tuning.checkpoints.permissions.with_raw_response.create(
        permission = response.parse()
        with client.fine_tuning.checkpoints.permissions.with_streaming_response.create(
            ValueError, match=r"Expected a non-empty value for `fine_tuned_model_checkpoint` but received ''"
            client.fine_tuning.checkpoints.permissions.with_raw_response.create(
                fine_tuned_model_checkpoint="",
            permission = client.fine_tuning.checkpoints.permissions.retrieve(
                fine_tuned_model_checkpoint="ft-AF1WoRqd3aJAHsqc9NY7iL8F",
        assert_matches_type(PermissionRetrieveResponse, permission, path=["response"])
                order="ascending",
                project_id="project_id",
            response = client.fine_tuning.checkpoints.permissions.with_raw_response.retrieve(
            with client.fine_tuning.checkpoints.permissions.with_streaming_response.retrieve(
                client.fine_tuning.checkpoints.permissions.with_raw_response.retrieve(
        permission = client.fine_tuning.checkpoints.permissions.list(
        assert_matches_type(SyncConversationCursorPage[PermissionListResponse], permission, path=["response"])
        response = client.fine_tuning.checkpoints.permissions.with_raw_response.list(
        with client.fine_tuning.checkpoints.permissions.with_streaming_response.list(
            client.fine_tuning.checkpoints.permissions.with_raw_response.list(
        permission = client.fine_tuning.checkpoints.permissions.delete(
            permission_id="cp_zc4Q7MP6XxulcVzj4MZdwsAB",
        assert_matches_type(PermissionDeleteResponse, permission, path=["response"])
        response = client.fine_tuning.checkpoints.permissions.with_raw_response.delete(
        with client.fine_tuning.checkpoints.permissions.with_streaming_response.delete(
            client.fine_tuning.checkpoints.permissions.with_raw_response.delete(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `permission_id` but received ''"):
                permission_id="",
class TestAsyncPermissions:
        permission = await async_client.fine_tuning.checkpoints.permissions.create(
        assert_matches_type(AsyncPage[PermissionCreateResponse], permission, path=["response"])
        response = await async_client.fine_tuning.checkpoints.permissions.with_raw_response.create(
        async with async_client.fine_tuning.checkpoints.permissions.with_streaming_response.create(
            permission = await response.parse()
            await async_client.fine_tuning.checkpoints.permissions.with_raw_response.create(
            permission = await async_client.fine_tuning.checkpoints.permissions.retrieve(
            response = await async_client.fine_tuning.checkpoints.permissions.with_raw_response.retrieve(
            async with async_client.fine_tuning.checkpoints.permissions.with_streaming_response.retrieve(
                await async_client.fine_tuning.checkpoints.permissions.with_raw_response.retrieve(
        permission = await async_client.fine_tuning.checkpoints.permissions.list(
        assert_matches_type(AsyncConversationCursorPage[PermissionListResponse], permission, path=["response"])
        response = await async_client.fine_tuning.checkpoints.permissions.with_raw_response.list(
        async with async_client.fine_tuning.checkpoints.permissions.with_streaming_response.list(
            await async_client.fine_tuning.checkpoints.permissions.with_raw_response.list(
        permission = await async_client.fine_tuning.checkpoints.permissions.delete(
        response = await async_client.fine_tuning.checkpoints.permissions.with_raw_response.delete(
        async with async_client.fine_tuning.checkpoints.permissions.with_streaming_response.delete(
            await async_client.fine_tuning.checkpoints.permissions.with_raw_response.delete(
