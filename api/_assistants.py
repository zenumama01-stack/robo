from typing import TYPE_CHECKING, Any, Generic, TypeVar, Callable, Iterable, Iterator, cast
from typing_extensions import Awaitable, AsyncIterable, AsyncIterator, assert_never
from ..._utils import is_dict, is_list, consume_sync_iterator, consume_async_iterator
from ..._compat import model_dump
from ..._models import construct_type
from ..._streaming import Stream, AsyncStream
from ...types.beta import AssistantStreamEvent
from ...types.beta.threads import (
    Run,
    ImageFile,
    TextDelta,
    MessageDelta,
    MessageContent,
    MessageContentDelta,
from ...types.beta.threads.runs import RunStep, ToolCall, RunStepDelta, ToolCallDelta
class AssistantEventHandler:
    text_deltas: Iterable[str]
    """Iterator over just the text deltas in the stream.
    This corresponds to the `thread.message.delta` event
    in the API.
    for text in stream.text_deltas:
        print(text, end="", flush=True)
        self._current_event: AssistantStreamEvent | None = None
        self._current_message_content_index: int | None = None
        self._current_message_content: MessageContent | None = None
        self._current_tool_call_index: int | None = None
        self._current_tool_call: ToolCall | None = None
        self.__current_run_step_id: str | None = None
        self.__current_run: Run | None = None
        self.__run_step_snapshots: dict[str, RunStep] = {}
        self.__message_snapshots: dict[str, Message] = {}
        self.__current_message_snapshot: Message | None = None
        self.text_deltas = self.__text_deltas__()
        self.__stream: Stream[AssistantStreamEvent] | None = None
    def _init(self, stream: Stream[AssistantStreamEvent]) -> None:
        if self.__stream:
                "A single event handler cannot be shared between multiple streams; You will need to construct a new event handler instance"
        self.__stream = stream
    def __next__(self) -> AssistantStreamEvent:
    def __iter__(self) -> Iterator[AssistantStreamEvent]:
    def current_event(self) -> AssistantStreamEvent | None:
        return self._current_event
    def current_run(self) -> Run | None:
        return self.__current_run
    def current_run_step_snapshot(self) -> RunStep | None:
        if not self.__current_run_step_id:
        return self.__run_step_snapshots[self.__current_run_step_id]
    def current_message_snapshot(self) -> Message | None:
        return self.__current_message_snapshot
        Automatically called when the context manager exits.
            self.__stream.close()
    def until_done(self) -> None:
        """Waits until the stream has been consumed"""
        consume_sync_iterator(self)
    def get_final_run(self) -> Run:
        """Wait for the stream to finish and returns the completed Run object"""
        self.until_done()
        if not self.__current_run:
            raise RuntimeError("No final run object found")
    def get_final_run_steps(self) -> list[RunStep]:
        """Wait for the stream to finish and returns the steps taken in this run"""
        if not self.__run_step_snapshots:
            raise RuntimeError("No run steps found")
        return [step for step in self.__run_step_snapshots.values()]
    def get_final_messages(self) -> list[Message]:
        """Wait for the stream to finish and returns the messages emitted in this run"""
        if not self.__message_snapshots:
            raise RuntimeError("No messages found")
        return [message for message in self.__message_snapshots.values()]
    def __text_deltas__(self) -> Iterator[str]:
        for event in self:
            if event.event != "thread.message.delta":
            for content_delta in event.data.delta.content or []:
                if content_delta.type == "text" and content_delta.text and content_delta.text.value:
                    yield content_delta.text.value
    # event handlers
    def on_end(self) -> None:
        """Fires when the stream has finished.
        This happens if the stream is read to completion
        or if an exception occurs during iteration.
    def on_event(self, event: AssistantStreamEvent) -> None:
        """Callback that is fired for every Server-Sent-Event"""
    def on_run_step_created(self, run_step: RunStep) -> None:
        """Callback that is fired when a run step is created"""
    def on_run_step_delta(self, delta: RunStepDelta, snapshot: RunStep) -> None:
        """Callback that is fired whenever a run step delta is returned from the API
        The first argument is just the delta as sent by the API and the second argument
        is the accumulated snapshot of the run step. For example, a tool calls event may
        look like this:
        # delta
        tool_calls=[
            RunStepDeltaToolCallsCodeInterpreter(
                type='code_interpreter',
                id=None,
                code_interpreter=CodeInterpreter(input=' sympy', outputs=None)
        # snapshot
            CodeToolCall(
                id='call_wKayJlcYV12NiadiZuJXxcfx',
                code_interpreter=CodeInterpreter(input='from sympy', outputs=[]),
                index=0
    def on_run_step_done(self, run_step: RunStep) -> None:
        """Callback that is fired when a run step is completed"""
    def on_tool_call_created(self, tool_call: ToolCall) -> None:
        """Callback that is fired when a tool call is created"""
    def on_tool_call_delta(self, delta: ToolCallDelta, snapshot: ToolCall) -> None:
        """Callback that is fired when a tool call delta is encountered"""
    def on_tool_call_done(self, tool_call: ToolCall) -> None:
    def on_exception(self, exception: Exception) -> None:
        """Fired whenever an exception happens during streaming"""
    def on_timeout(self) -> None:
        """Fires if the request times out"""
    def on_message_created(self, message: Message) -> None:
        """Callback that is fired when a message is created"""
    def on_message_delta(self, delta: MessageDelta, snapshot: Message) -> None:
        """Callback that is fired whenever a message delta is returned from the API
        is the accumulated snapshot of the message. For example, a text content event may
        MessageDeltaText(
            type='text',
            text=Text(
                value=' Jane'
        MessageContentText(
                value='Certainly, Jane'
    def on_message_done(self, message: Message) -> None:
        """Callback that is fired when a message is completed"""
    def on_text_created(self, text: Text) -> None:
        """Callback that is fired when a text content block is created"""
    def on_text_delta(self, delta: TextDelta, snapshot: Text) -> None:
        """Callback that is fired whenever a text content delta is returned
        by the API.
        is the accumulated snapshot of the text. For example:
        on_text_delta(TextDelta(value="The"), Text(value="The")),
        on_text_delta(TextDelta(value=" solution"), Text(value="The solution")),
        on_text_delta(TextDelta(value=" to"), Text(value="The solution to")),
        on_text_delta(TextDelta(value=" the"), Text(value="The solution to the")),
        on_text_delta(TextDelta(value=" equation"), Text(value="The solution to the equation")),
    def on_text_done(self, text: Text) -> None:
        """Callback that is fired when a text content block is finished"""
    def on_image_file_done(self, image_file: ImageFile) -> None:
        """Callback that is fired when an image file block is finished"""
    def _emit_sse_event(self, event: AssistantStreamEvent) -> None:
        self._current_event = event
        self.on_event(event)
        self.__current_message_snapshot, new_content = accumulate_event(
            event=event,
            current_message_snapshot=self.__current_message_snapshot,
        if self.__current_message_snapshot is not None:
            self.__message_snapshots[self.__current_message_snapshot.id] = self.__current_message_snapshot
        accumulate_run_step(
            run_step_snapshots=self.__run_step_snapshots,
        for content_delta in new_content:
            assert self.__current_message_snapshot is not None
            block = self.__current_message_snapshot.content[content_delta.index]
            if block.type == "text":
                self.on_text_created(block.text)
            event.event == "thread.run.completed"
            or event.event == "thread.run.cancelled"
            or event.event == "thread.run.expired"
            or event.event == "thread.run.failed"
            or event.event == "thread.run.requires_action"
            or event.event == "thread.run.incomplete"
            self.__current_run = event.data
            if self._current_tool_call:
                self.on_tool_call_done(self._current_tool_call)
            event.event == "thread.run.created"
            or event.event == "thread.run.in_progress"
            or event.event == "thread.run.cancelling"
            or event.event == "thread.run.queued"
        elif event.event == "thread.message.created":
            self.on_message_created(event.data)
        elif event.event == "thread.message.delta":
            snapshot = self.__current_message_snapshot
            assert snapshot is not None
            message_delta = event.data.delta
            if message_delta.content is not None:
                for content_delta in message_delta.content:
                    if content_delta.type == "text" and content_delta.text:
                        snapshot_content = snapshot.content[content_delta.index]
                        assert snapshot_content.type == "text"
                        self.on_text_delta(content_delta.text, snapshot_content.text)
                    # If the delta is for a new message content:
                    # - emit on_text_done/on_image_file_done for the previous message content
                    # - emit on_text_created/on_image_created for the new message content
                    if content_delta.index != self._current_message_content_index:
                        if self._current_message_content is not None:
                            if self._current_message_content.type == "text":
                                self.on_text_done(self._current_message_content.text)
                            elif self._current_message_content.type == "image_file":
                                self.on_image_file_done(self._current_message_content.image_file)
                        self._current_message_content_index = content_delta.index
                        self._current_message_content = snapshot.content[content_delta.index]
                    # Update the current_message_content (delta event is correctly emitted already)
            self.on_message_delta(event.data.delta, snapshot)
        elif event.event == "thread.message.completed" or event.event == "thread.message.incomplete":
            self.__current_message_snapshot = event.data
            self.__message_snapshots[event.data.id] = event.data
            if self._current_message_content_index is not None:
                content = event.data.content[self._current_message_content_index]
                if content.type == "text":
                    self.on_text_done(content.text)
                elif content.type == "image_file":
                    self.on_image_file_done(content.image_file)
            self.on_message_done(event.data)
        elif event.event == "thread.run.step.created":
            self.__current_run_step_id = event.data.id
            self.on_run_step_created(event.data)
        elif event.event == "thread.run.step.in_progress":
        elif event.event == "thread.run.step.delta":
            step_snapshot = self.__run_step_snapshots[event.data.id]
            run_step_delta = event.data.delta
                run_step_delta.step_details
                and run_step_delta.step_details.type == "tool_calls"
                and run_step_delta.step_details.tool_calls is not None
                assert step_snapshot.step_details.type == "tool_calls"
                for tool_call_delta in run_step_delta.step_details.tool_calls:
                    if tool_call_delta.index == self._current_tool_call_index:
                        self.on_tool_call_delta(
                            tool_call_delta,
                            step_snapshot.step_details.tool_calls[tool_call_delta.index],
                    # If the delta is for a new tool call:
                    # - emit on_tool_call_done for the previous tool_call
                    # - emit on_tool_call_created for the new tool_call
                    if tool_call_delta.index != self._current_tool_call_index:
                        if self._current_tool_call is not None:
                        self._current_tool_call_index = tool_call_delta.index
                        self._current_tool_call = step_snapshot.step_details.tool_calls[tool_call_delta.index]
                        self.on_tool_call_created(self._current_tool_call)
                    # Update the current_tool_call (delta event is correctly emitted already)
            self.on_run_step_delta(
                event.data.delta,
                step_snapshot,
            event.event == "thread.run.step.completed"
            or event.event == "thread.run.step.cancelled"
            or event.event == "thread.run.step.expired"
            or event.event == "thread.run.step.failed"
            self.on_run_step_done(event.data)
            self.__current_run_step_id = None
        elif event.event == "thread.created" or event.event == "thread.message.in_progress" or event.event == "error":
            # currently no special handling
            # we only want to error at build-time
            if TYPE_CHECKING:  # type: ignore[unreachable]
                assert_never(event)
        self._current_event = None
    def __stream__(self) -> Iterator[AssistantStreamEvent]:
        stream = self.__stream
        if not stream:
            raise RuntimeError("Stream has not been started yet")
                self._emit_sse_event(event)
                yield event
        except (httpx.TimeoutException, asyncio.TimeoutError) as exc:
            self.on_timeout()
            self.on_end()
AssistantEventHandlerT = TypeVar("AssistantEventHandlerT", bound=AssistantEventHandler)
class AssistantStreamManager(Generic[AssistantEventHandlerT]):
    """Wrapper over AssistantStreamEventHandler that is returned by `.stream()`
    so that a context manager can be used.
    with client.threads.create_and_run_stream(...) as stream:
        api_request: Callable[[], Stream[AssistantStreamEvent]],
        event_handler: AssistantEventHandlerT,
        self.__event_handler = event_handler
        self.__api_request = api_request
    def __enter__(self) -> AssistantEventHandlerT:
        self.__stream = self.__api_request()
        self.__event_handler._init(self.__stream)
        return self.__event_handler
        if self.__stream is not None:
class AsyncAssistantEventHandler:
    text_deltas: AsyncIterable[str]
    async for text in stream.text_deltas:
        self.__stream: AsyncStream[AssistantStreamEvent] | None = None
    def _init(self, stream: AsyncStream[AssistantStreamEvent]) -> None:
    async def __anext__(self) -> AssistantStreamEvent:
    async def __aiter__(self) -> AsyncIterator[AssistantStreamEvent]:
            await self.__stream.close()
    async def until_done(self) -> None:
        await consume_async_iterator(self)
    async def get_final_run(self) -> Run:
        await self.until_done()
    async def get_final_run_steps(self) -> list[RunStep]:
    async def get_final_messages(self) -> list[Message]:
    async def __text_deltas__(self) -> AsyncIterator[str]:
        async for event in self:
    async def on_end(self) -> None:
    async def on_event(self, event: AssistantStreamEvent) -> None:
    async def on_run_step_created(self, run_step: RunStep) -> None:
    async def on_run_step_delta(self, delta: RunStepDelta, snapshot: RunStep) -> None:
    async def on_run_step_done(self, run_step: RunStep) -> None:
    async def on_tool_call_created(self, tool_call: ToolCall) -> None:
    async def on_tool_call_delta(self, delta: ToolCallDelta, snapshot: ToolCall) -> None:
    async def on_tool_call_done(self, tool_call: ToolCall) -> None:
    async def on_exception(self, exception: Exception) -> None:
    async def on_timeout(self) -> None:
    async def on_message_created(self, message: Message) -> None:
    async def on_message_delta(self, delta: MessageDelta, snapshot: Message) -> None:
    async def on_message_done(self, message: Message) -> None:
    async def on_text_created(self, text: Text) -> None:
    async def on_text_delta(self, delta: TextDelta, snapshot: Text) -> None:
        on_text_delta(TextDelta(value=" equation"), Text(value="The solution to the equivalent")),
    async def on_text_done(self, text: Text) -> None:
    async def on_image_file_done(self, image_file: ImageFile) -> None:
    async def _emit_sse_event(self, event: AssistantStreamEvent) -> None:
        await self.on_event(event)
                await self.on_text_created(block.text)
                await self.on_tool_call_done(self._current_tool_call)
            await self.on_message_created(event.data)
                        await self.on_text_delta(content_delta.text, snapshot_content.text)
                                await self.on_text_done(self._current_message_content.text)
                                await self.on_image_file_done(self._current_message_content.image_file)
            await self.on_message_delta(event.data.delta, snapshot)
                    await self.on_text_done(content.text)
                    await self.on_image_file_done(content.image_file)
            await self.on_message_done(event.data)
            await self.on_run_step_created(event.data)
                        await self.on_tool_call_delta(
                        await self.on_tool_call_created(self._current_tool_call)
            await self.on_run_step_delta(
            await self.on_run_step_done(event.data)
    async def __stream__(self) -> AsyncIterator[AssistantStreamEvent]:
                await self._emit_sse_event(event)
            await self.on_timeout()
            await self.on_exception(exc)
            await self.on_end()
AsyncAssistantEventHandlerT = TypeVar("AsyncAssistantEventHandlerT", bound=AsyncAssistantEventHandler)
class AsyncAssistantStreamManager(Generic[AsyncAssistantEventHandlerT]):
    """Wrapper over AsyncAssistantStreamEventHandler that is returned by `.stream()`
    so that an async context manager can be used without `await`ing the
    original client call.
    async with client.threads.create_and_run_stream(...) as stream:
        api_request: Awaitable[AsyncStream[AssistantStreamEvent]],
        event_handler: AsyncAssistantEventHandlerT,
    async def __aenter__(self) -> AsyncAssistantEventHandlerT:
        self.__stream = await self.__api_request
def accumulate_run_step(
    event: AssistantStreamEvent,
    run_step_snapshots: dict[str, RunStep],
    if event.event == "thread.run.step.created":
        run_step_snapshots[event.data.id] = event.data
    if event.event == "thread.run.step.delta":
        data = event.data
        snapshot = run_step_snapshots[data.id]
        if data.delta:
            merged = accumulate_delta(
                cast(
                    "dict[object, object]",
                    model_dump(snapshot, exclude_unset=True, warnings=False),
                    model_dump(data.delta, exclude_unset=True, warnings=False),
            run_step_snapshots[snapshot.id] = cast(RunStep, construct_type(type_=RunStep, value=merged))
def accumulate_event(
    current_message_snapshot: Message | None,
) -> tuple[Message | None, list[MessageContentDelta]]:
    """Returns a tuple of message snapshot and newly created text message deltas"""
    if event.event == "thread.message.created":
        return event.data, []
    new_content: list[MessageContentDelta] = []
        return current_message_snapshot, []
    if not current_message_snapshot:
        raise RuntimeError("Encountered a message delta with no previous snapshot")
    if data.delta.content:
        for content_delta in data.delta.content:
                block = current_message_snapshot.content[content_delta.index]
                current_message_snapshot.content.insert(
                    content_delta.index,
                        construct_type(
                            # mypy doesn't allow Content for some reason
                            type_=cast(Any, MessageContent),
                            value=model_dump(content_delta, exclude_unset=True, warnings=False),
                new_content.append(content_delta)
                        model_dump(block, exclude_unset=True, warnings=False),
                        model_dump(content_delta, exclude_unset=True, warnings=False),
                current_message_snapshot.content[content_delta.index] = cast(
                        value=merged,
    return current_message_snapshot, new_content
def accumulate_delta(acc: dict[object, object], delta: dict[object, object]) -> dict[object, object]:
    for key, delta_value in delta.items():
        if key not in acc:
            acc[key] = delta_value
        acc_value = acc[key]
        if acc_value is None:
        # the `index` property is used in arrays of objects so it should
        # not be accumulated like other values e.g.
        # [{'foo': 'bar', 'index': 0}]
        # the same applies to `type` properties as they're used for
        # discriminated unions
        if key == "index" or key == "type":
        if isinstance(acc_value, str) and isinstance(delta_value, str):
            acc_value += delta_value
        elif isinstance(acc_value, (int, float)) and isinstance(delta_value, (int, float)):
        elif is_dict(acc_value) and is_dict(delta_value):
            acc_value = accumulate_delta(acc_value, delta_value)
        elif is_list(acc_value) and is_list(delta_value):
            # for lists of non-dictionary items we'll only ever get new entries
            # in the array, existing entries will never be changed
            if all(isinstance(x, (str, int, float)) for x in acc_value):
                acc_value.extend(delta_value)
            for delta_entry in delta_value:
                if not is_dict(delta_entry):
                    raise TypeError(f"Unexpected list delta entry is not a dictionary: {delta_entry}")
                    index = delta_entry["index"]
                    raise RuntimeError(f"Expected list delta entry to have an `index` key; {delta_entry}") from exc
                if not isinstance(index, int):
                    raise TypeError(f"Unexpected, list delta entry `index` value is not an integer; {index}")
                    acc_entry = acc_value[index]
                    acc_value.insert(index, delta_entry)
                    if not is_dict(acc_entry):
                        raise TypeError("not handled yet")
                    acc_value[index] = accumulate_delta(acc_entry, delta_entry)
        acc[key] = acc_value
    return acc
