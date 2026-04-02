__all__ = ["Checkpoints", "AsyncCheckpoints"]
class Checkpoints(SyncAPIResource):
    def permissions(self) -> Permissions:
        return Permissions(self._client)
    def with_raw_response(self) -> CheckpointsWithRawResponse:
        return CheckpointsWithRawResponse(self)
    def with_streaming_response(self) -> CheckpointsWithStreamingResponse:
        return CheckpointsWithStreamingResponse(self)
class AsyncCheckpoints(AsyncAPIResource):
    def permissions(self) -> AsyncPermissions:
        return AsyncPermissions(self._client)
    def with_raw_response(self) -> AsyncCheckpointsWithRawResponse:
        return AsyncCheckpointsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncCheckpointsWithStreamingResponse:
        return AsyncCheckpointsWithStreamingResponse(self)
class CheckpointsWithRawResponse:
    def __init__(self, checkpoints: Checkpoints) -> None:
        self._checkpoints = checkpoints
    def permissions(self) -> PermissionsWithRawResponse:
        return PermissionsWithRawResponse(self._checkpoints.permissions)
class AsyncCheckpointsWithRawResponse:
    def __init__(self, checkpoints: AsyncCheckpoints) -> None:
    def permissions(self) -> AsyncPermissionsWithRawResponse:
        return AsyncPermissionsWithRawResponse(self._checkpoints.permissions)
class CheckpointsWithStreamingResponse:
    def permissions(self) -> PermissionsWithStreamingResponse:
        return PermissionsWithStreamingResponse(self._checkpoints.permissions)
class AsyncCheckpointsWithStreamingResponse:
    def permissions(self) -> AsyncPermissionsWithStreamingResponse:
        return AsyncPermissionsWithStreamingResponse(self._checkpoints.permissions)
from ....types.fine_tuning.jobs import checkpoint_list_params
from ....types.fine_tuning.jobs.fine_tuning_job_checkpoint import FineTuningJobCheckpoint
        fine_tuning_job_id: str,
    ) -> SyncCursorPage[FineTuningJobCheckpoint]:
        List checkpoints for a fine-tuning job.
          after: Identifier for the last checkpoint ID from the previous pagination request.
          limit: Number of checkpoints to retrieve.
        if not fine_tuning_job_id:
            raise ValueError(f"Expected a non-empty value for `fine_tuning_job_id` but received {fine_tuning_job_id!r}")
            path_template("/fine_tuning/jobs/{fine_tuning_job_id}/checkpoints", fine_tuning_job_id=fine_tuning_job_id),
            page=SyncCursorPage[FineTuningJobCheckpoint],
                    checkpoint_list_params.CheckpointListParams,
            model=FineTuningJobCheckpoint,
    ) -> AsyncPaginator[FineTuningJobCheckpoint, AsyncCursorPage[FineTuningJobCheckpoint]]:
            page=AsyncCursorPage[FineTuningJobCheckpoint],
            checkpoints.list,
            f"/fine_tuning/jobs/{fine_tuning_job_id}/checkpoints",
