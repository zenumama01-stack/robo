from ....._types import Body, Omit, Query, Headers, NotGiven, omit, not_given
from ....._utils import path_template, maybe_transform, async_maybe_transform
from .....types.beta.threads.runs import step_list_params, step_retrieve_params
from .....types.beta.threads.runs.run_step import RunStep
__all__ = ["Steps", "AsyncSteps"]
class Steps(SyncAPIResource):
    def with_raw_response(self) -> StepsWithRawResponse:
        return StepsWithRawResponse(self)
    def with_streaming_response(self) -> StepsWithStreamingResponse:
        return StepsWithStreamingResponse(self)
        step_id: str,
    ) -> RunStep:
        Retrieves a run step.
        if not step_id:
            raise ValueError(f"Expected a non-empty value for `step_id` but received {step_id!r}")
            path_template(
                "/threads/{thread_id}/runs/{run_id}/steps/{step_id}",
                step_id=step_id,
                query=maybe_transform({"include": include}, step_retrieve_params.StepRetrieveParams),
            cast_to=RunStep,
    ) -> SyncCursorPage[RunStep]:
        Returns a list of run steps belonging to a run.
            path_template("/threads/{thread_id}/runs/{run_id}/steps", thread_id=thread_id, run_id=run_id),
            page=SyncCursorPage[RunStep],
                    step_list_params.StepListParams,
            model=RunStep,
class AsyncSteps(AsyncAPIResource):
    def with_raw_response(self) -> AsyncStepsWithRawResponse:
        return AsyncStepsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncStepsWithStreamingResponse:
        return AsyncStepsWithStreamingResponse(self)
                query=await async_maybe_transform({"include": include}, step_retrieve_params.StepRetrieveParams),
    ) -> AsyncPaginator[RunStep, AsyncCursorPage[RunStep]]:
            page=AsyncCursorPage[RunStep],
class StepsWithRawResponse:
    def __init__(self, steps: Steps) -> None:
        self._steps = steps
                steps.retrieve,  # pyright: ignore[reportDeprecated],
                steps.list,  # pyright: ignore[reportDeprecated],
class AsyncStepsWithRawResponse:
    def __init__(self, steps: AsyncSteps) -> None:
class StepsWithStreamingResponse:
class AsyncStepsWithStreamingResponse:
from ....._utils import maybe_transform, async_maybe_transform
            f"/threads/{thread_id}/runs/{run_id}/steps/{step_id}",
            f"/threads/{thread_id}/runs/{run_id}/steps",
