from .jobs.jobs import (
from .alpha.alpha import (
from .checkpoints.checkpoints import (
__all__ = ["FineTuning", "AsyncFineTuning"]
class FineTuning(SyncAPIResource):
    def jobs(self) -> Jobs:
        """Manage fine-tuning jobs to tailor a model to your specific training data."""
        return Jobs(self._client)
    def checkpoints(self) -> Checkpoints:
        return Checkpoints(self._client)
    def alpha(self) -> Alpha:
        return Alpha(self._client)
    def with_raw_response(self) -> FineTuningWithRawResponse:
        return FineTuningWithRawResponse(self)
    def with_streaming_response(self) -> FineTuningWithStreamingResponse:
        return FineTuningWithStreamingResponse(self)
class AsyncFineTuning(AsyncAPIResource):
    def jobs(self) -> AsyncJobs:
        return AsyncJobs(self._client)
    def checkpoints(self) -> AsyncCheckpoints:
        return AsyncCheckpoints(self._client)
    def alpha(self) -> AsyncAlpha:
        return AsyncAlpha(self._client)
    def with_raw_response(self) -> AsyncFineTuningWithRawResponse:
        return AsyncFineTuningWithRawResponse(self)
    def with_streaming_response(self) -> AsyncFineTuningWithStreamingResponse:
        return AsyncFineTuningWithStreamingResponse(self)
class FineTuningWithRawResponse:
    def __init__(self, fine_tuning: FineTuning) -> None:
        self._fine_tuning = fine_tuning
    def jobs(self) -> JobsWithRawResponse:
        return JobsWithRawResponse(self._fine_tuning.jobs)
    def checkpoints(self) -> CheckpointsWithRawResponse:
        return CheckpointsWithRawResponse(self._fine_tuning.checkpoints)
    def alpha(self) -> AlphaWithRawResponse:
        return AlphaWithRawResponse(self._fine_tuning.alpha)
class AsyncFineTuningWithRawResponse:
    def __init__(self, fine_tuning: AsyncFineTuning) -> None:
    def jobs(self) -> AsyncJobsWithRawResponse:
        return AsyncJobsWithRawResponse(self._fine_tuning.jobs)
    def checkpoints(self) -> AsyncCheckpointsWithRawResponse:
        return AsyncCheckpointsWithRawResponse(self._fine_tuning.checkpoints)
    def alpha(self) -> AsyncAlphaWithRawResponse:
        return AsyncAlphaWithRawResponse(self._fine_tuning.alpha)
class FineTuningWithStreamingResponse:
    def jobs(self) -> JobsWithStreamingResponse:
        return JobsWithStreamingResponse(self._fine_tuning.jobs)
    def checkpoints(self) -> CheckpointsWithStreamingResponse:
        return CheckpointsWithStreamingResponse(self._fine_tuning.checkpoints)
    def alpha(self) -> AlphaWithStreamingResponse:
        return AlphaWithStreamingResponse(self._fine_tuning.alpha)
class AsyncFineTuningWithStreamingResponse:
    def jobs(self) -> AsyncJobsWithStreamingResponse:
        return AsyncJobsWithStreamingResponse(self._fine_tuning.jobs)
    def checkpoints(self) -> AsyncCheckpointsWithStreamingResponse:
        return AsyncCheckpointsWithStreamingResponse(self._fine_tuning.checkpoints)
    def alpha(self) -> AsyncAlphaWithStreamingResponse:
        return AsyncAlphaWithStreamingResponse(self._fine_tuning.alpha)
from openai.types.fine_tuning.fine_tuning_job import Hyperparameters
class OpenAIFineTuningHyperparameters(Hyperparameters):
