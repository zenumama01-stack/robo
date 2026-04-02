from ...._utils import path_template, maybe_transform
from ....pagination import SyncConversationCursorPage, AsyncConversationCursorPage
from ...._base_client import AsyncPaginator, make_request_options
from ....types.beta.chatkit import thread_list_params, thread_list_items_params
from ....types.beta.chatkit.chatkit_thread import ChatKitThread
from ....types.beta.chatkit.thread_delete_response import ThreadDeleteResponse
from ....types.beta.chatkit.chatkit_thread_item_list import Data
__all__ = ["Threads", "AsyncThreads"]
class Threads(SyncAPIResource):
    def with_raw_response(self) -> ThreadsWithRawResponse:
        return ThreadsWithRawResponse(self)
    def with_streaming_response(self) -> ThreadsWithStreamingResponse:
        return ThreadsWithStreamingResponse(self)
        thread_id: str,
    ) -> ChatKitThread:
        Retrieve a ChatKit thread by its identifier.
        if not thread_id:
            raise ValueError(f"Expected a non-empty value for `thread_id` but received {thread_id!r}")
            path_template("/chatkit/threads/{thread_id}", thread_id=thread_id),
            cast_to=ChatKitThread,
    ) -> SyncConversationCursorPage[ChatKitThread]:
        List ChatKit threads with optional pagination and user filters.
          after: List items created after this thread item ID. Defaults to null for the first
              page.
          before: List items created before this thread item ID. Defaults to null for the newest
          limit: Maximum number of thread items to return. Defaults to 20.
          order: Sort order for results by creation time. Defaults to `desc`.
          user: Filter threads that belong to this user identifier. Defaults to null to return
              all users.
            "/chatkit/threads",
            page=SyncConversationCursorPage[ChatKitThread],
                    thread_list_params.ThreadListParams,
            model=ChatKitThread,
    ) -> ThreadDeleteResponse:
        Delete a ChatKit thread along with its items and stored attachments.
            cast_to=ThreadDeleteResponse,
    def list_items(
    ) -> SyncConversationCursorPage[Data]:
        List items that belong to a ChatKit thread.
            path_template("/chatkit/threads/{thread_id}/items", thread_id=thread_id),
            page=SyncConversationCursorPage[Data],
                    thread_list_items_params.ThreadListItemsParams,
            model=cast(Any, Data),  # Union types cannot be passed in as arguments in the type system
class AsyncThreads(AsyncAPIResource):
    def with_raw_response(self) -> AsyncThreadsWithRawResponse:
        return AsyncThreadsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncThreadsWithStreamingResponse:
        return AsyncThreadsWithStreamingResponse(self)
    ) -> AsyncPaginator[ChatKitThread, AsyncConversationCursorPage[ChatKitThread]]:
            page=AsyncConversationCursorPage[ChatKitThread],
    ) -> AsyncPaginator[Data, AsyncConversationCursorPage[Data]]:
            page=AsyncConversationCursorPage[Data],
class ThreadsWithRawResponse:
    def __init__(self, threads: Threads) -> None:
        self._threads = threads
            threads.retrieve,
            threads.list,
            threads.delete,
        self.list_items = _legacy_response.to_raw_response_wrapper(
            threads.list_items,
class AsyncThreadsWithRawResponse:
    def __init__(self, threads: AsyncThreads) -> None:
        self.list_items = _legacy_response.async_to_raw_response_wrapper(
class ThreadsWithStreamingResponse:
        self.list_items = to_streamed_response_wrapper(
class AsyncThreadsWithStreamingResponse:
        self.list_items = async_to_streamed_response_wrapper(
from ...._types import NOT_GIVEN, Body, Omit, Query, Headers, NotGiven, omit, not_given
from ...._utils import path_template, required_args, maybe_transform, async_maybe_transform
from .runs.runs import (
from ....types.beta import (
    thread_create_params,
    thread_update_params,
    thread_create_and_run_params,
from ....lib.streaming import (
    AssistantEventHandler,
    AssistantEventHandlerT,
    AssistantStreamManager,
    AsyncAssistantEventHandler,
    AsyncAssistantEventHandlerT,
    AsyncAssistantStreamManager,
from ....types.beta.thread import Thread
from ....types.beta.threads.run import Run
from ....types.shared.chat_model import ChatModel
from ....types.beta.thread_deleted import ThreadDeleted
from ....types.beta.assistant_tool_param import AssistantToolParam
from ....types.beta.assistant_stream_event import AssistantStreamEvent
from ....types.beta.assistant_tool_choice_option_param import AssistantToolChoiceOptionParam
from ....types.beta.assistant_response_format_option_param import AssistantResponseFormatOptionParam
    def runs(self) -> Runs:
        return Runs(self._client)
    def messages(self) -> Messages:
        return Messages(self._client)
        messages: Iterable[thread_create_params.Message] | Omit = omit,
        tool_resources: Optional[thread_create_params.ToolResources] | Omit = omit,
    ) -> Thread:
        Create a thread.
          messages: A list of [messages](https://platform.openai.com/docs/api-reference/messages) to
              start the thread with.
          tool_resources: A set of resources that are made available to the assistant's tools in this
              thread. The resources are specific to the type of tool. For example, the
              `code_interpreter` tool requires a list of file IDs, while the `file_search`
              tool requires a list of vector store IDs.
            "/threads",
                    "messages": messages,
                thread_create_params.ThreadCreateParams,
            cast_to=Thread,
        Retrieves a thread.
            path_template("/threads/{thread_id}", thread_id=thread_id),
        tool_resources: Optional[thread_update_params.ToolResources] | Omit = omit,
        Modifies a thread.
                thread_update_params.ThreadUpdateParams,
    ) -> ThreadDeleted:
        Delete a thread.
            cast_to=ThreadDeleted,
    def create_and_run(
        max_completion_tokens: Optional[int] | Omit = omit,
        max_prompt_tokens: Optional[int] | Omit = omit,
        model: Union[str, ChatModel, None] | Omit = omit,
        parallel_tool_calls: bool | Omit = omit,
        thread: thread_create_and_run_params.Thread | Omit = omit,
        tool_choice: Optional[AssistantToolChoiceOptionParam] | Omit = omit,
        tool_resources: Optional[thread_create_and_run_params.ToolResources] | Omit = omit,
        tools: Optional[Iterable[AssistantToolParam]] | Omit = omit,
        truncation_strategy: Optional[thread_create_and_run_params.TruncationStrategy] | Omit = omit,
    ) -> Run:
        Create a thread and run it in one request.
          assistant_id: The ID of the
              [assistant](https://platform.openai.com/docs/api-reference/assistants) to use to
              execute this run.
          instructions: Override the default system message of the assistant. This is useful for
              modifying the behavior on a per-run basis.
          max_completion_tokens: The maximum number of completion tokens that may be used over the course of the
              run. The run will make a best effort to use only the number of completion tokens
              specified, across multiple turns of the run. If the run exceeds the number of
              completion tokens specified, the run will end with status `incomplete`. See
              `incomplete_details` for more info.
          max_prompt_tokens: The maximum number of prompt tokens that may be used over the course of the run.
              The run will make a best effort to use only the number of prompt tokens
              prompt tokens specified, the run will end with status `incomplete`. See
          model: The ID of the [Model](https://platform.openai.com/docs/api-reference/models) to
              be used to execute this run. If a value is provided here, it will override the
              model associated with the assistant. If not, the model associated with the
              assistant will be used.
          parallel_tool_calls: Whether to enable
              [parallel function calling](https://platform.openai.com/docs/guides/function-calling#configuring-parallel-function-calling)
              during tool use.
          stream: If `true`, returns a stream of events that happen during the Run as server-sent
              events, terminating when the Run enters a terminal state with a `data: [DONE]`
          thread: Options to create a new thread. If no thread is provided when running a request,
              an empty thread will be created.
          tool_choice: Controls which (if any) tool is called by the model. `none` means the model will
              not call any tools and instead generates a message. `auto` is the default value
              and means the model can pick between generating a message or calling one or more
              tools. `required` means the model must call one or more tools before responding
              to the user. Specifying a particular tool like `{"type": "file_search"}` or
              `{"type": "function", "function": {"name": "my_function"}}` forces the model to
              call that tool.
          tools: Override the tools the assistant can use for this run. This is useful for
          truncation_strategy: Controls for how a thread will be truncated prior to the run. Use this to
              control the initial context window of the run.
    ) -> Stream[AssistantStreamEvent]:
    ) -> Run | Stream[AssistantStreamEvent]:
    @required_args(["assistant_id"], ["assistant_id", "stream"])
            "/threads/runs",
                    "assistant_id": assistant_id,
                    "max_completion_tokens": max_completion_tokens,
                    "max_prompt_tokens": max_prompt_tokens,
                    "parallel_tool_calls": parallel_tool_calls,
                    "thread": thread,
                    "truncation_strategy": truncation_strategy,
                thread_create_and_run_params.ThreadCreateAndRunParamsStreaming
                else thread_create_and_run_params.ThreadCreateAndRunParamsNonStreaming,
                synthesize_event_and_data=True,
            cast_to=Run,
            stream_cls=Stream[AssistantStreamEvent],
    def create_and_run_poll(
        A helper to create a thread, start a run and then poll for a terminal state.
        More information on Run lifecycles can be found here:
        https://platform.openai.com/docs/assistants/how-it-works/runs-and-run-steps
        run = self.create_and_run(  # pyright: ignore[reportDeprecated]
            assistant_id=assistant_id,
            instructions=instructions,
            max_completion_tokens=max_completion_tokens,
            max_prompt_tokens=max_prompt_tokens,
            metadata=metadata,
            parallel_tool_calls=parallel_tool_calls,
            temperature=temperature,
            stream=False,
            thread=thread,
            tool_resources=tool_resources,
            truncation_strategy=truncation_strategy,
            top_p=top_p,
        return self.runs.poll(run.id, run.thread_id, extra_headers, extra_query, extra_body, timeout, poll_interval_ms)  # pyright: ignore[reportDeprecated]
    def create_and_run_stream(
    ) -> AssistantStreamManager[AssistantEventHandler]:
        """Create a thread and stream the run back"""
    ) -> AssistantStreamManager[AssistantEventHandlerT]:
        event_handler: AssistantEventHandlerT | None = None,
    ) -> AssistantStreamManager[AssistantEventHandler] | AssistantStreamManager[AssistantEventHandlerT]:
        extra_headers = {
            "OpenAI-Beta": "assistants=v2",
            "X-Stainless-Stream-Helper": "threads.create_and_run_stream",
            "X-Stainless-Custom-Event-Handler": "true" if event_handler else "false",
            **(extra_headers or {}),
            self._post,
                    "stream": True,
                thread_create_and_run_params.ThreadCreateAndRunParams,
        return AssistantStreamManager(make_request, event_handler=event_handler or AssistantEventHandler())
    def runs(self) -> AsyncRuns:
        return AsyncRuns(self._client)
    def messages(self) -> AsyncMessages:
        return AsyncMessages(self._client)
    async def create_and_run(
    ) -> AsyncStream[AssistantStreamEvent]:
    ) -> Run | AsyncStream[AssistantStreamEvent]:
            stream_cls=AsyncStream[AssistantStreamEvent],
    async def create_and_run_poll(
        run = await self.create_and_run(  # pyright: ignore[reportDeprecated]
        return await self.runs.poll(  # pyright: ignore[reportDeprecated]
            run.id, run.thread_id, extra_headers, extra_query, extra_body, timeout, poll_interval_ms
    ) -> AsyncAssistantStreamManager[AsyncAssistantEventHandler]:
    ) -> AsyncAssistantStreamManager[AsyncAssistantEventHandlerT]:
        event_handler: AsyncAssistantEventHandlerT | None = None,
    ) -> (
        AsyncAssistantStreamManager[AsyncAssistantEventHandler]
        | AsyncAssistantStreamManager[AsyncAssistantEventHandlerT]
        request = self._post(
        return AsyncAssistantStreamManager(request, event_handler=event_handler or AsyncAssistantEventHandler())
                threads.create,  # pyright: ignore[reportDeprecated],
                threads.retrieve,  # pyright: ignore[reportDeprecated],
                threads.update,  # pyright: ignore[reportDeprecated],
                threads.delete,  # pyright: ignore[reportDeprecated],
        self.create_and_run = (  # pyright: ignore[reportDeprecated]
                threads.create_and_run,  # pyright: ignore[reportDeprecated],
    def runs(self) -> RunsWithRawResponse:
        return RunsWithRawResponse(self._threads.runs)
    def messages(self) -> MessagesWithRawResponse:
        return MessagesWithRawResponse(self._threads.messages)
    def runs(self) -> AsyncRunsWithRawResponse:
        return AsyncRunsWithRawResponse(self._threads.runs)
    def messages(self) -> AsyncMessagesWithRawResponse:
        return AsyncMessagesWithRawResponse(self._threads.messages)
    def runs(self) -> RunsWithStreamingResponse:
        return RunsWithStreamingResponse(self._threads.runs)
    def messages(self) -> MessagesWithStreamingResponse:
        return MessagesWithStreamingResponse(self._threads.messages)
    def runs(self) -> AsyncRunsWithStreamingResponse:
        return AsyncRunsWithStreamingResponse(self._threads.runs)
    def messages(self) -> AsyncMessagesWithStreamingResponse:
        return AsyncMessagesWithStreamingResponse(self._threads.messages)
from ...._utils import maybe_transform
        Retrieve a ChatKit thread
            f"/chatkit/threads/{thread_id}",
        List ChatKit threads
        Delete a ChatKit thread
        List ChatKit thread items
            f"/chatkit/threads/{thread_id}/items",
from ...._utils import required_args, maybe_transform, async_maybe_transform
            f"/threads/{thread_id}",
