from typing import List, Union, Iterable, Optional
from ..... import _legacy_response
from ....._types import NOT_GIVEN, Body, Omit, Query, Headers, NotGiven, omit, not_given
from ....._utils import (
    path_template,
    required_args,
from ....._compat import cached_property
from ....._resource import SyncAPIResource, AsyncAPIResource
from ....._response import to_streamed_response_wrapper, async_to_streamed_response_wrapper
from ....._streaming import Stream, AsyncStream
from .....pagination import SyncCursorPage, AsyncCursorPage
from ....._base_client import AsyncPaginator, make_request_options
from .....lib.streaming import (
from .....types.beta.threads import (
    run_list_params,
    run_create_params,
    run_update_params,
    run_submit_tool_outputs_params,
from .....types.beta.threads.run import Run
from .....types.shared.chat_model import ChatModel
from .....types.shared_params.metadata import Metadata
from .....types.shared.reasoning_effort import ReasoningEffort
from .....types.beta.assistant_tool_param import AssistantToolParam
from .....types.beta.assistant_stream_event import AssistantStreamEvent
from .....types.beta.threads.runs.run_step_include import RunStepInclude
from .....types.beta.assistant_tool_choice_option_param import AssistantToolChoiceOptionParam
from .....types.beta.assistant_response_format_option_param import AssistantResponseFormatOptionParam
__all__ = ["Runs", "AsyncRuns"]
class Runs(SyncAPIResource):
    def steps(self) -> Steps:
        return Steps(self._client)
    def with_raw_response(self) -> RunsWithRawResponse:
        return RunsWithRawResponse(self)
    def with_streaming_response(self) -> RunsWithStreamingResponse:
        return RunsWithStreamingResponse(self)
        include: List[RunStepInclude] | Omit = omit,
        additional_instructions: Optional[str] | Omit = omit,
        additional_messages: Optional[Iterable[run_create_params.AdditionalMessage]] | Omit = omit,
        truncation_strategy: Optional[run_create_params.TruncationStrategy] | Omit = omit,
        Create a run.
          include: A list of additional fields to include in the response. Currently the only
              supported value is `step_details.tool_calls[*].file_search.results[*].content`
              to fetch the file search result content.
              [file search tool documentation](https://platform.openai.com/docs/assistants/tools/file-search#customizing-file-search-settings)
          additional_instructions: Appends additional instructions at the end of the instructions for the run. This
              is useful for modifying the behavior on a per-run basis without overriding other
              instructions.
          additional_messages: Adds additional messages to the thread before creating the run.
          instructions: Overrides the
              [instructions](https://platform.openai.com/docs/api-reference/assistants/createAssistant)
              of the assistant. This is useful for modifying the behavior on a per-run basis.
            path_template("/threads/{thread_id}/runs", thread_id=thread_id),
                    "additional_instructions": additional_instructions,
                    "additional_messages": additional_messages,
                run_create_params.RunCreateParamsStreaming if stream else run_create_params.RunCreateParamsNonStreaming,
                query=maybe_transform({"include": include}, run_create_params.RunCreateParams),
        run_id: str,
        Retrieves a run.
        if not run_id:
            raise ValueError(f"Expected a non-empty value for `run_id` but received {run_id!r}")
            path_template("/threads/{thread_id}/runs/{run_id}", thread_id=thread_id, run_id=run_id),
        Modifies a run.
            body=maybe_transform({"metadata": metadata}, run_update_params.RunUpdateParams),
    ) -> SyncCursorPage[Run]:
        Returns a list of runs belonging to a thread.
            page=SyncCursorPage[Run],
                    run_list_params.RunListParams,
            model=Run,
        Cancels a run that is `in_progress`.
            path_template("/threads/{thread_id}/runs/{run_id}/cancel", thread_id=thread_id, run_id=run_id),
        A helper to create a run an poll for a terminal state. More information on Run
        lifecycles can be found here:
        run = self.create(  # pyright: ignore[reportDeprecated]
            thread_id=thread_id,
            additional_instructions=additional_instructions,
            additional_messages=additional_messages,
            reasoning_effort=reasoning_effort,
            # We assume we are not streaming when polling
        return self.poll(  # pyright: ignore[reportDeprecated]
            run.id,
    @typing_extensions.deprecated("use `stream` instead")
    def create_and_stream(
        """Create a Run stream"""
            "X-Stainless-Stream-Helper": "threads.runs.create_and_stream",
                run_create_params.RunCreateParams,
        A helper to poll a run status until it reaches a terminal state. More
        information on Run lifecycles can be found here:
        extra_headers = {"X-Stainless-Poll-Helper": "true", **(extra_headers or {})}
            extra_headers["X-Stainless-Custom-Poll-Interval"] = str(poll_interval_ms)
        terminal_states = {"requires_action", "cancelled", "completed", "failed", "expired", "incomplete"}
            response = self.with_raw_response.retrieve(  # pyright: ignore[reportDeprecated]
                run_id=run_id,
            run = response.parse()
            # Return if we reached a terminal state
            if run.status in terminal_states:
                return run
    def stream(
    def submit_tool_outputs(
        tool_outputs: Iterable[run_submit_tool_outputs_params.ToolOutput],
        When a run has the `status: "requires_action"` and `required_action.type` is
        `submit_tool_outputs`, this endpoint can be used to submit the outputs from the
        tool calls once they're all completed. All outputs must be submitted in a single
          tool_outputs: A list of tools for which the outputs are being submitted.
    @required_args(["thread_id", "tool_outputs"], ["thread_id", "stream", "tool_outputs"])
            path_template("/threads/{thread_id}/runs/{run_id}/submit_tool_outputs", thread_id=thread_id, run_id=run_id),
                    "tool_outputs": tool_outputs,
                run_submit_tool_outputs_params.RunSubmitToolOutputsParamsStreaming
                else run_submit_tool_outputs_params.RunSubmitToolOutputsParamsNonStreaming,
    def submit_tool_outputs_and_poll(
        A helper to submit a tool output to a run and poll for a terminal run state.
        run = self.submit_tool_outputs(  # pyright: ignore[reportDeprecated]
            tool_outputs=tool_outputs,
            run_id=run.id,
    def submit_tool_outputs_stream(
        Submit the tool outputs from a previous run and stream the run to a terminal
        state. More information on Run lifecycles can be found here:
            "X-Stainless-Stream-Helper": "threads.runs.submit_tool_outputs_stream",
        request = partial(
                run_submit_tool_outputs_params.RunSubmitToolOutputsParams,
        return AssistantStreamManager(request, event_handler=event_handler or AssistantEventHandler())
class AsyncRuns(AsyncAPIResource):
    def steps(self) -> AsyncSteps:
        return AsyncSteps(self._client)
    def with_raw_response(self) -> AsyncRunsWithRawResponse:
        return AsyncRunsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncRunsWithStreamingResponse:
        return AsyncRunsWithStreamingResponse(self)
                query=await async_maybe_transform({"include": include}, run_create_params.RunCreateParams),
            body=await async_maybe_transform({"metadata": metadata}, run_update_params.RunUpdateParams),
    ) -> AsyncPaginator[Run, AsyncCursorPage[Run]]:
            page=AsyncCursorPage[Run],
        run = await self.create(  # pyright: ignore[reportDeprecated]
        return await self.poll(  # pyright: ignore[reportDeprecated]
            response = await self.with_raw_response.retrieve(  # pyright: ignore[reportDeprecated]
    async def submit_tool_outputs(
    async def submit_tool_outputs_and_poll(
        run = await self.submit_tool_outputs(  # pyright: ignore[reportDeprecated]
class RunsWithRawResponse:
    def __init__(self, runs: Runs) -> None:
        self._runs = runs
                runs.create,  # pyright: ignore[reportDeprecated],
                runs.retrieve,  # pyright: ignore[reportDeprecated],
                runs.update,  # pyright: ignore[reportDeprecated],
                runs.list,  # pyright: ignore[reportDeprecated],
        self.cancel = (  # pyright: ignore[reportDeprecated]
                runs.cancel,  # pyright: ignore[reportDeprecated],
        self.submit_tool_outputs = (  # pyright: ignore[reportDeprecated]
                runs.submit_tool_outputs,  # pyright: ignore[reportDeprecated],
    def steps(self) -> StepsWithRawResponse:
        return StepsWithRawResponse(self._runs.steps)
class AsyncRunsWithRawResponse:
    def __init__(self, runs: AsyncRuns) -> None:
    def steps(self) -> AsyncStepsWithRawResponse:
        return AsyncStepsWithRawResponse(self._runs.steps)
class RunsWithStreamingResponse:
    def steps(self) -> StepsWithStreamingResponse:
        return StepsWithStreamingResponse(self._runs.steps)
class AsyncRunsWithStreamingResponse:
    def steps(self) -> AsyncStepsWithStreamingResponse:
        return AsyncStepsWithStreamingResponse(self._runs.steps)
from ....types.evals import run_list_params, run_create_params
from ....types.evals.run_list_response import RunListResponse
from ....types.evals.run_cancel_response import RunCancelResponse
from ....types.evals.run_create_response import RunCreateResponse
from ....types.evals.run_delete_response import RunDeleteResponse
from ....types.evals.run_retrieve_response import RunRetrieveResponse
    def output_items(self) -> OutputItems:
        return OutputItems(self._client)
        data_source: run_create_params.DataSource,
    ) -> RunCreateResponse:
        Kicks off a new run for a given evaluation, specifying the data source, and what
        model configuration to use to test. The datasource will be validated against the
        schema specified in the config of the evaluation.
          data_source: Details about the run's data source.
          name: The name of the run.
            path_template("/evals/{eval_id}/runs", eval_id=eval_id),
                    "data_source": data_source,
            cast_to=RunCreateResponse,
    ) -> RunRetrieveResponse:
        Get an evaluation run by ID.
            path_template("/evals/{eval_id}/runs/{run_id}", eval_id=eval_id, run_id=run_id),
            cast_to=RunRetrieveResponse,
        status: Literal["queued", "in_progress", "completed", "canceled", "failed"] | Omit = omit,
    ) -> SyncCursorPage[RunListResponse]:
        Get a list of runs for an evaluation.
          after: Identifier for the last run from the previous pagination request.
          limit: Number of runs to retrieve.
          order: Sort order for runs by timestamp. Use `asc` for ascending order or `desc` for
              descending order. Defaults to `asc`.
          status: Filter runs by status. One of `queued` | `in_progress` | `failed` | `completed`
              | `canceled`.
            page=SyncCursorPage[RunListResponse],
            model=RunListResponse,
    ) -> RunDeleteResponse:
        Delete an eval run.
            cast_to=RunDeleteResponse,
    ) -> RunCancelResponse:
        Cancel an ongoing evaluation run.
            cast_to=RunCancelResponse,
    def output_items(self) -> AsyncOutputItems:
        return AsyncOutputItems(self._client)
    ) -> AsyncPaginator[RunListResponse, AsyncCursorPage[RunListResponse]]:
            page=AsyncCursorPage[RunListResponse],
            runs.create,
            runs.retrieve,
            runs.list,
            runs.delete,
            runs.cancel,
    def output_items(self) -> OutputItemsWithRawResponse:
        return OutputItemsWithRawResponse(self._runs.output_items)
    def output_items(self) -> AsyncOutputItemsWithRawResponse:
        return AsyncOutputItemsWithRawResponse(self._runs.output_items)
    def output_items(self) -> OutputItemsWithStreamingResponse:
        return OutputItemsWithStreamingResponse(self._runs.output_items)
    def output_items(self) -> AsyncOutputItemsWithStreamingResponse:
        return AsyncOutputItemsWithStreamingResponse(self._runs.output_items)
            f"/threads/{thread_id}/runs",
            f"/threads/{thread_id}/runs/{run_id}",
            f"/threads/{thread_id}/runs/{run_id}/cancel",
            f"/threads/{thread_id}/runs/{run_id}/submit_tool_outputs",
            f"/evals/{eval_id}/runs",
            f"/evals/{eval_id}/runs/{run_id}",
