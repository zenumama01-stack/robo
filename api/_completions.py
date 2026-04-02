from typing import TYPE_CHECKING, Any, Iterable, cast
from typing_extensions import TypeVar, TypeGuard, assert_never
from .._tools import PydanticFunctionTool
from ..._types import Omit, omit
from ..._utils import is_dict, is_given
from ..._compat import PYDANTIC_V1, model_parse_json
from ..._models import construct_type_unchecked
from .._pydantic import is_basemodel_type, to_strict_json_schema, is_dataclass_like_type
from ...types.chat import (
    ParsedChoice,
    ChatCompletion,
    ParsedFunction,
    ParsedChatCompletion,
    ChatCompletionMessage,
    ParsedFunctionToolCall,
    ParsedChatCompletionMessage,
    ChatCompletionToolUnionParam,
    ChatCompletionFunctionToolParam,
    completion_create_params,
from ..._exceptions import LengthFinishReasonError, ContentFilterFinishReasonError
from ...types.shared_params import FunctionDefinition
from ...types.chat.completion_create_params import ResponseFormat as ResponseFormatParam
from ...types.chat.chat_completion_message_function_tool_call import Function
ResponseFormatT = TypeVar(
    "ResponseFormatT",
    # if it isn't given then we don't do any parsing
_default_response_format: None = None
log: logging.Logger = logging.getLogger("openai.lib.parsing")
def is_strict_chat_completion_tool_param(
    tool: ChatCompletionToolUnionParam,
) -> TypeGuard[ChatCompletionFunctionToolParam]:
    """Check if the given tool is a strict ChatCompletionFunctionToolParam."""
    if not tool["type"] == "function":
    if tool["function"].get("strict") is not True:
def select_strict_chat_completion_tools(
    tools: Iterable[ChatCompletionToolUnionParam] | Omit = omit,
) -> Iterable[ChatCompletionFunctionToolParam] | Omit:
    """Select only the strict ChatCompletionFunctionToolParams from the given tools."""
    if not is_given(tools):
        return omit
    return [t for t in tools if is_strict_chat_completion_tool_param(t)]
def validate_input_tools(
    for tool in tools:
        if tool["type"] != "function":
                f"Currently only `function` tool types support auto-parsing; Received `{tool['type']}`",
        strict = tool["function"].get("strict")
        if strict is not True:
                f"`{tool['function']['name']}` is not strict. Only `strict` function tools can be auto-parsed"
    return cast(Iterable[ChatCompletionFunctionToolParam], tools)
def parse_chat_completion(
    response_format: type[ResponseFormatT] | completion_create_params.ResponseFormat | Omit,
    input_tools: Iterable[ChatCompletionToolUnionParam] | Omit,
    chat_completion: ChatCompletion | ParsedChatCompletion[object],
) -> ParsedChatCompletion[ResponseFormatT]:
    if is_given(input_tools):
        input_tools = [t for t in input_tools]
        input_tools = []
    choices: list[ParsedChoice[ResponseFormatT]] = []
    for choice in chat_completion.choices:
        if choice.finish_reason == "length":
            raise LengthFinishReasonError(completion=chat_completion)
        if choice.finish_reason == "content_filter":
            raise ContentFilterFinishReasonError()
        message = choice.message
        tool_calls: list[ParsedFunctionToolCall] = []
        if message.tool_calls:
            for tool_call in message.tool_calls:
                if tool_call.type == "function":
                    tool_call_dict = tool_call.to_dict()
                    tool_calls.append(
                        construct_type_unchecked(
                            value={
                                **tool_call_dict,
                                "function": {
                                    **cast(Any, tool_call_dict["function"]),
                                    "parsed_arguments": parse_function_tool_arguments(
                                        input_tools=input_tools, function=tool_call.function
                            type_=ParsedFunctionToolCall,
                elif tool_call.type == "custom":
                    # warn user that custom tool calls are not callable here
                        "Custom tool calls are not callable. Ignoring tool call: %s - %s",
                        tool_call.id,
                        tool_call.custom.name,
                elif TYPE_CHECKING:  # type: ignore[unreachable]
                    assert_never(tool_call)
                    tool_calls.append(tool_call)
        choices.append(
                type_=ParsedChoice[ResponseFormatT],
                    **choice.to_dict(),
                    "message": {
                        **message.to_dict(),
                        "parsed": maybe_parse_content(
                            response_format=response_format,
                        "tool_calls": tool_calls if tool_calls else None,
    return construct_type_unchecked(
        type_=ParsedChatCompletion[ResponseFormatT],
            **chat_completion.to_dict(),
            "choices": choices,
def get_input_tool_by_name(
    *, input_tools: list[ChatCompletionToolUnionParam], name: str
) -> ChatCompletionFunctionToolParam | None:
    return next((t for t in input_tools if t["type"] == "function" and t.get("function", {}).get("name") == name), None)
def parse_function_tool_arguments(
    *, input_tools: list[ChatCompletionToolUnionParam], function: Function | ParsedFunction
) -> object | None:
    input_tool = get_input_tool_by_name(input_tools=input_tools, name=function.name)
    if not input_tool:
    input_fn = cast(object, input_tool.get("function"))
    if isinstance(input_fn, PydanticFunctionTool):
        return model_parse_json(input_fn.model, function.arguments)
    input_fn = cast(FunctionDefinition, input_fn)
    if not input_fn.get("strict"):
    return json.loads(function.arguments)  # type: ignore[no-any-return]
def maybe_parse_content(
    response_format: type[ResponseFormatT] | ResponseFormatParam | Omit,
    message: ChatCompletionMessage | ParsedChatCompletionMessage[object],
) -> ResponseFormatT | None:
    if has_rich_response_format(response_format) and message.content and not message.refusal:
        return _parse_content(response_format, message.content)
def has_parseable_input(
    response_format: type | ResponseFormatParam | Omit,
    input_tools: Iterable[ChatCompletionToolUnionParam] | Omit = omit,
) -> bool:
    if has_rich_response_format(response_format):
    for input_tool in input_tools or []:
        if is_parseable_tool(input_tool):
def has_rich_response_format(
) -> TypeGuard[type[ResponseFormatT]]:
    if not is_given(response_format):
    if is_response_format_param(response_format):
def is_response_format_param(response_format: object) -> TypeGuard[ResponseFormatParam]:
    return is_dict(response_format)
def is_parseable_tool(input_tool: ChatCompletionToolUnionParam) -> bool:
    if input_tool["type"] != "function":
    return cast(FunctionDefinition, input_fn).get("strict") or False
def _parse_content(response_format: type[ResponseFormatT], content: str) -> ResponseFormatT:
    if is_basemodel_type(response_format):
        return cast(ResponseFormatT, model_parse_json(response_format, content))
    if is_dataclass_like_type(response_format):
            raise TypeError(f"Non BaseModel types are only supported with Pydantic v2 - {response_format}")
        return pydantic.TypeAdapter(response_format).validate_json(content)
    raise TypeError(f"Unable to automatically parse response format type {response_format}")
def type_to_response_format_param(
    response_format: type | completion_create_params.ResponseFormat | Omit,
) -> ResponseFormatParam | Omit:
        return response_format
    # type checkers don't narrow the negation of a `TypeGuard` as it isn't
    # a safe default behaviour but we know that at this point the `response_format`
    # can only be a `type`
    response_format = cast(type, response_format)
    json_schema_type: type[pydantic.BaseModel] | pydantic.TypeAdapter[Any] | None = None
        name = response_format.__name__
        json_schema_type = response_format
    elif is_dataclass_like_type(response_format):
        json_schema_type = pydantic.TypeAdapter(response_format)
        raise TypeError(f"Unsupported response_format type - {response_format}")
        "type": "json_schema",
        "json_schema": {
            "schema": to_strict_json_schema(json_schema_type),
            "name": name,
from typing import TYPE_CHECKING, Any, Generic, Callable, Iterable, Awaitable, AsyncIterator, cast
from typing_extensions import Self, Iterator, assert_never
from jiter import from_json
from ._types import ParsedChoiceSnapshot, ParsedChatCompletionSnapshot, ParsedChatCompletionMessageSnapshot
    ChunkEvent,
    ContentDoneEvent,
    RefusalDoneEvent,
    ContentDeltaEvent,
    RefusalDeltaEvent,
    LogprobsContentDoneEvent,
    LogprobsRefusalDoneEvent,
    ChatCompletionStreamEvent,
    LogprobsContentDeltaEvent,
    LogprobsRefusalDeltaEvent,
    FunctionToolCallArgumentsDoneEvent,
    FunctionToolCallArgumentsDeltaEvent,
from .._deltas import accumulate_delta
from ...._types import Omit, IncEx, omit
from ...._utils import is_given, consume_sync_iterator, consume_async_iterator
from ...._compat import model_dump
from ...._models import build, construct_type
from ..._parsing import (
    ResponseFormatT,
    maybe_parse_content,
    parse_chat_completion,
    get_input_tool_by_name,
    parse_function_tool_arguments,
from ...._streaming import Stream, AsyncStream
from ....types.chat import ChatCompletionChunk, ParsedChatCompletion, ChatCompletionToolUnionParam
from ...._exceptions import LengthFinishReasonError, ContentFilterFinishReasonError
from ....types.chat.chat_completion import ChoiceLogprobs
from ....types.chat.chat_completion_chunk import Choice as ChoiceChunk
from ....types.chat.completion_create_params import ResponseFormat as ResponseFormatParam
class ChatCompletionStream(Generic[ResponseFormatT]):
    """Wrapper over the Chat Completions streaming API that adds helpful
    events such as `content.done`, supports automatically parsing
    responses & tool calls and accumulates a `ChatCompletion` object
    from each individual chunk.
    https://platform.openai.com/docs/api-reference/streaming
        raw_stream: Stream[ChatCompletionChunk],
        self._raw_stream = raw_stream
        self._response = raw_stream.response
        self._state = ChatCompletionStreamState(response_format=response_format, input_tools=input_tools)
    def __next__(self) -> ChatCompletionStreamEvent[ResponseFormatT]:
    def __iter__(self) -> Iterator[ChatCompletionStreamEvent[ResponseFormatT]]:
        self._response.close()
    def get_final_completion(self) -> ParsedChatCompletion[ResponseFormatT]:
        """Waits until the stream has been read to completion and returns
        the accumulated `ParsedChatCompletion` object.
        If you passed a class type to `.stream()`, the `completion.choices[0].message.parsed`
        property will be the content deserialised into that class, if there was any content returned
        return self._state.get_final_completion()
    def until_done(self) -> Self:
        """Blocks until the stream has been consumed."""
    def current_completion_snapshot(self) -> ParsedChatCompletionSnapshot:
        return self._state.current_completion_snapshot
    def __stream__(self) -> Iterator[ChatCompletionStreamEvent[ResponseFormatT]]:
        for sse_event in self._raw_stream:
            if not _is_valid_chat_completion_chunk_weak(sse_event):
            events_to_fire = self._state.handle_chunk(sse_event)
            for event in events_to_fire:
class ChatCompletionStreamManager(Generic[ResponseFormatT]):
    """Context manager over a `ChatCompletionStream` that is returned by `.stream()`.
    This context manager ensures the response cannot be leaked if you don't read
    the stream to completion.
    with client.chat.completions.stream(...) as stream:
        api_request: Callable[[], Stream[ChatCompletionChunk]],
        self.__stream: ChatCompletionStream[ResponseFormatT] | None = None
        self.__response_format = response_format
        self.__input_tools = input_tools
    def __enter__(self) -> ChatCompletionStream[ResponseFormatT]:
        raw_stream = self.__api_request()
        self.__stream = ChatCompletionStream(
            raw_stream=raw_stream,
            response_format=self.__response_format,
            input_tools=self.__input_tools,
        return self.__stream
class AsyncChatCompletionStream(Generic[ResponseFormatT]):
        raw_stream: AsyncStream[ChatCompletionChunk],
    async def __anext__(self) -> ChatCompletionStreamEvent[ResponseFormatT]:
    async def __aiter__(self) -> AsyncIterator[ChatCompletionStreamEvent[ResponseFormatT]]:
        await self._response.aclose()
    async def get_final_completion(self) -> ParsedChatCompletion[ResponseFormatT]:
    async def until_done(self) -> Self:
    async def __stream__(self) -> AsyncIterator[ChatCompletionStreamEvent[ResponseFormatT]]:
        async for sse_event in self._raw_stream:
class AsyncChatCompletionStreamManager(Generic[ResponseFormatT]):
    """Context manager over a `AsyncChatCompletionStream` that is returned by `.stream()`.
    async with client.chat.completions.stream(...) as stream:
        api_request: Awaitable[AsyncStream[ChatCompletionChunk]],
        self.__stream: AsyncChatCompletionStream[ResponseFormatT] | None = None
    async def __aenter__(self) -> AsyncChatCompletionStream[ResponseFormatT]:
        raw_stream = await self.__api_request
        self.__stream = AsyncChatCompletionStream(
class ChatCompletionStreamState(Generic[ResponseFormatT]):
    """Helper class for manually accumulating `ChatCompletionChunk`s into a final `ChatCompletion` object.
    This is useful in cases where you can't always use the `.stream()` method, e.g.
    from openai.lib.streaming.chat import ChatCompletionStreamState
    state = ChatCompletionStreamState()
    stream = client.chat.completions.create(..., stream=True)
    for chunk in response:
        state.handle_chunk(chunk)
        # can also access the accumulated `ChatCompletion` mid-stream
        state.current_completion_snapshot
    print(state.get_final_completion())
        response_format: type[ResponseFormatT] | ResponseFormatParam | Omit = omit,
        self.__current_completion_snapshot: ParsedChatCompletionSnapshot | None = None
        self.__choice_event_states: list[ChoiceEventState] = []
        self._input_tools = [tool for tool in input_tools] if is_given(input_tools) else []
        self._response_format = response_format
        self._rich_response_format: type | Omit = response_format if inspect.isclass(response_format) else omit
        """Parse the final completion object.
        Note this does not provide any guarantees that the stream has actually finished, you must
        only call this method when the stream is finished.
        return parse_chat_completion(
            chat_completion=self.current_completion_snapshot,
            response_format=self._rich_response_format,
            input_tools=self._input_tools,
        assert self.__current_completion_snapshot is not None
        return self.__current_completion_snapshot
    def handle_chunk(self, chunk: ChatCompletionChunk) -> Iterable[ChatCompletionStreamEvent[ResponseFormatT]]:
        """Accumulate a new chunk into the snapshot and returns an iterable of events to yield."""
        self.__current_completion_snapshot = self._accumulate_chunk(chunk)
        return self._build_events(
            chunk=chunk,
            completion_snapshot=self.__current_completion_snapshot,
    def _get_choice_state(self, choice: ChoiceChunk) -> ChoiceEventState:
            return self.__choice_event_states[choice.index]
            choice_state = ChoiceEventState(input_tools=self._input_tools)
            self.__choice_event_states.append(choice_state)
            return choice_state
    def _accumulate_chunk(self, chunk: ChatCompletionChunk) -> ParsedChatCompletionSnapshot:
        completion_snapshot = self.__current_completion_snapshot
        if completion_snapshot is None:
            return _convert_initial_chunk_into_snapshot(chunk)
                choice_snapshot = completion_snapshot.choices[choice.index]
                previous_tool_calls = choice_snapshot.message.tool_calls or []
                choice_snapshot.message = cast(
                    ParsedChatCompletionMessageSnapshot,
                        type_=ParsedChatCompletionMessageSnapshot,
                        value=accumulate_delta(
                                    choice_snapshot.message,
                                    # we don't want to serialise / deserialise our custom properties
                                    # as they won't appear in the delta and we don't want to have to
                                    # continuosly reparse the content
                                    exclude=cast(
                                        # cast required as mypy isn't smart enough to infer `True` here to `Literal[True]`
                                            "parsed": True,
                                            "tool_calls": {
                                                idx: {"function": {"parsed_arguments": True}}
                                                for idx, _ in enumerate(choice_snapshot.message.tool_calls or [])
                            cast("dict[object, object]", choice.delta.to_dict()),
                # ensure tools that have already been parsed are added back into the newly
                # constructed message snapshot
                for tool_index, prev_tool in enumerate(previous_tool_calls):
                    new_tool = (choice_snapshot.message.tool_calls or [])[tool_index]
                    if prev_tool.type == "function":
                        assert new_tool.type == "function"
                        new_tool.function.parsed_arguments = prev_tool.function.parsed_arguments
                        assert_never(prev_tool)
                choice_snapshot = cast(
                    ParsedChoiceSnapshot,
                        type_=ParsedChoiceSnapshot,
                            **choice.model_dump(exclude_unset=True, exclude={"delta"}),
                            "message": choice.delta.to_dict(),
                completion_snapshot.choices.append(choice_snapshot)
            if choice.finish_reason:
                choice_snapshot.finish_reason = choice.finish_reason
                if has_parseable_input(response_format=self._response_format, input_tools=self._input_tools):
                        # at the time of writing, `.usage` will always be `None` but
                        # we include it here in case that is changed in the future
                        raise LengthFinishReasonError(completion=completion_snapshot)
                choice_snapshot.message.content
                and not choice_snapshot.message.refusal
                and is_given(self._rich_response_format)
                # partial parsing fails on white-space
                and choice_snapshot.message.content.lstrip()
                choice_snapshot.message.parsed = from_json(
                    bytes(choice_snapshot.message.content, "utf-8"),
                    partial_mode=True,
            for tool_call_chunk in choice.delta.tool_calls or []:
                tool_call_snapshot = (choice_snapshot.message.tool_calls or [])[tool_call_chunk.index]
                if tool_call_snapshot.type == "function":
                    input_tool = get_input_tool_by_name(
                        input_tools=self._input_tools, name=tool_call_snapshot.function.name
                        input_tool
                        and input_tool.get("function", {}).get("strict")
                        and tool_call_snapshot.function.arguments
                        tool_call_snapshot.function.parsed_arguments = from_json(
                            bytes(tool_call_snapshot.function.arguments, "utf-8"),
                    assert_never(tool_call_snapshot)
            if choice.logprobs is not None:
                if choice_snapshot.logprobs is None:
                    choice_snapshot.logprobs = build(
                        ChoiceLogprobs,
                        content=choice.logprobs.content,
                        refusal=choice.logprobs.refusal,
                    if choice.logprobs.content:
                        if choice_snapshot.logprobs.content is None:
                            choice_snapshot.logprobs.content = []
                        choice_snapshot.logprobs.content.extend(choice.logprobs.content)
                    if choice.logprobs.refusal:
                        if choice_snapshot.logprobs.refusal is None:
                            choice_snapshot.logprobs.refusal = []
                        choice_snapshot.logprobs.refusal.extend(choice.logprobs.refusal)
        completion_snapshot.usage = chunk.usage
        completion_snapshot.system_fingerprint = chunk.system_fingerprint
        return completion_snapshot
    def _build_events(
        chunk: ChatCompletionChunk,
        completion_snapshot: ParsedChatCompletionSnapshot,
    ) -> list[ChatCompletionStreamEvent[ResponseFormatT]]:
        events_to_fire: list[ChatCompletionStreamEvent[ResponseFormatT]] = []
        events_to_fire.append(
            build(ChunkEvent, type="chunk", chunk=chunk, snapshot=completion_snapshot),
            choice_state = self._get_choice_state(choice)
            if choice.delta.content is not None and choice_snapshot.message.content is not None:
                    build(
                        type="content.delta",
                        delta=choice.delta.content,
                        snapshot=choice_snapshot.message.content,
                        parsed=choice_snapshot.message.parsed,
            if choice.delta.refusal is not None and choice_snapshot.message.refusal is not None:
                        type="refusal.delta",
                        delta=choice.delta.refusal,
                        snapshot=choice_snapshot.message.refusal,
            if choice.delta.tool_calls:
                tool_calls = choice_snapshot.message.tool_calls
                assert tool_calls is not None
                for tool_call_delta in choice.delta.tool_calls:
                    tool_call = tool_calls[tool_call_delta.index]
                        assert tool_call_delta.function is not None
                                type="tool_calls.function.arguments.delta",
                                name=tool_call.function.name,
                                index=tool_call_delta.index,
                                arguments=tool_call.function.arguments,
                                parsed_arguments=tool_call.function.parsed_arguments,
                                arguments_delta=tool_call_delta.function.arguments or "",
            if choice.logprobs is not None and choice_snapshot.logprobs is not None:
                if choice.logprobs.content and choice_snapshot.logprobs.content:
                            type="logprobs.content.delta",
                            snapshot=choice_snapshot.logprobs.content,
                if choice.logprobs.refusal and choice_snapshot.logprobs.refusal:
                            type="logprobs.refusal.delta",
                            snapshot=choice_snapshot.logprobs.refusal,
            events_to_fire.extend(
                choice_state.get_done_events(
                    choice_chunk=choice,
                    choice_snapshot=choice_snapshot,
                    response_format=self._response_format,
        return events_to_fire
class ChoiceEventState:
    def __init__(self, *, input_tools: list[ChatCompletionToolUnionParam]) -> None:
        self._input_tools = input_tools
        self._content_done = False
        self._refusal_done = False
        self._logprobs_content_done = False
        self._logprobs_refusal_done = False
        self._done_tool_calls: set[int] = set()
        self.__current_tool_call_index: int | None = None
    def get_done_events(
        choice_chunk: ChoiceChunk,
        choice_snapshot: ParsedChoiceSnapshot,
        if choice_snapshot.finish_reason:
                self._content_done_events(choice_snapshot=choice_snapshot, response_format=response_format)
                self.__current_tool_call_index is not None
                and self.__current_tool_call_index not in self._done_tool_calls
                self._add_tool_done_event(
                    events_to_fire=events_to_fire,
                    tool_index=self.__current_tool_call_index,
        for tool_call in choice_chunk.delta.tool_calls or []:
            if self.__current_tool_call_index != tool_call.index:
                if self.__current_tool_call_index is not None:
            self.__current_tool_call_index = tool_call.index
    def _content_done_events(
        if choice_snapshot.message.content and not self._content_done:
            self._content_done = True
            parsed = maybe_parse_content(
                message=choice_snapshot.message,
            # update the parsed content to now use the richer `response_format`
            # as opposed to the raw JSON-parsed object as the content is now
            # complete and can be fully validated.
            choice_snapshot.message.parsed = parsed
                    # we do this dance so that when the `ContentDoneEvent` instance
                    # is printed at runtime the class name will include the solved
                    # type variable, e.g. `ContentDoneEvent[MyModelType]`
                    cast(  # pyright: ignore[reportUnnecessaryCast]
                        "type[ContentDoneEvent[ResponseFormatT]]",
                        cast(Any, ContentDoneEvent),
                    type="content.done",
                    content=choice_snapshot.message.content,
                    parsed=parsed,
        if choice_snapshot.message.refusal is not None and not self._refusal_done:
            self._refusal_done = True
                build(RefusalDoneEvent, type="refusal.done", refusal=choice_snapshot.message.refusal),
            choice_snapshot.logprobs is not None
            and choice_snapshot.logprobs.content is not None
            and not self._logprobs_content_done
            self._logprobs_content_done = True
                build(LogprobsContentDoneEvent, type="logprobs.content.done", content=choice_snapshot.logprobs.content),
            and choice_snapshot.logprobs.refusal is not None
            and not self._logprobs_refusal_done
            self._logprobs_refusal_done = True
                build(LogprobsRefusalDoneEvent, type="logprobs.refusal.done", refusal=choice_snapshot.logprobs.refusal),
    def _add_tool_done_event(
        events_to_fire: list[ChatCompletionStreamEvent[ResponseFormatT]],
        tool_index: int,
        if tool_index in self._done_tool_calls:
        self._done_tool_calls.add(tool_index)
        assert choice_snapshot.message.tool_calls is not None
        tool_call_snapshot = choice_snapshot.message.tool_calls[tool_index]
            parsed_arguments = parse_function_tool_arguments(
                input_tools=self._input_tools, function=tool_call_snapshot.function
            # update the parsed content to potentially use a richer type
            tool_call_snapshot.function.parsed_arguments = parsed_arguments
                    type="tool_calls.function.arguments.done",
                    index=tool_index,
                    name=tool_call_snapshot.function.name,
                    arguments=tool_call_snapshot.function.arguments,
                    parsed_arguments=parsed_arguments,
def _convert_initial_chunk_into_snapshot(chunk: ChatCompletionChunk) -> ParsedChatCompletionSnapshot:
    data = chunk.to_dict()
    choices = cast("list[object]", data["choices"])
        choices[choice.index] = {
        ParsedChatCompletionSnapshot,
            type_=ParsedChatCompletionSnapshot,
                "system_fingerprint": None,
                **data,
                "object": "chat.completion",
def _is_valid_chat_completion_chunk_weak(sse_event: ChatCompletionChunk) -> bool:
    # Although the _raw_stream is always supposed to contain only objects adhering to ChatCompletionChunk schema,
    # this is broken by the Azure OpenAI in case of Asynchronous Filter enabled.
    # An easy filter is to check for the "object" property:
    # - should be "chat.completion.chunk" for a ChatCompletionChunk;
    # - is an empty string for Asynchronous Filter events.
    return sse_event.object == "chat.completion.chunk"  # type: ignore # pylance reports this as a useless check
                type_=cast(Any, ParsedChoice)[solve_response_format_t(response_format)],
        ParsedChatCompletion[ResponseFormatT],
            type_=cast(Any, ParsedChatCompletion)[solve_response_format_t(response_format)],
def solve_response_format_t(
) -> type[ResponseFormatT]:
    """Return the runtime type for the given response format.
    If no response format is given, or if we won't auto-parse the response format
    then we default to `None`.
    return cast("type[ResponseFormatT]", _default_response_format)
    solve_response_format_t,
                        cast(Any, ContentDoneEvent)[solve_response_format_t(response_format)],
