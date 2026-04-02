from typing import TYPE_CHECKING, List, Iterable, cast
from typing_extensions import TypeVar, assert_never
from .._tools import ResponsesPydanticFunctionTool
from ..._types import Omit
from .._pydantic import is_basemodel_type, is_dataclass_like_type
from ._completions import type_to_response_format_param
from ...types.responses import (
    ToolParam,
    ParsedContent,
    ParsedResponse,
    ParsedResponseOutputItem,
    ParsedResponseOutputText,
    ResponseFunctionToolCall,
    ParsedResponseOutputMessage,
    ResponseFormatTextConfigParam,
    ParsedResponseFunctionToolCall,
from ...types.chat.completion_create_params import ResponseFormat
TextFormatT = TypeVar(
    "TextFormatT",
def type_to_text_format_param(type_: type) -> ResponseFormatTextConfigParam:
    response_format_dict = type_to_response_format_param(type_)
    assert is_given(response_format_dict)
    response_format_dict = cast(ResponseFormat, response_format_dict)  # pyright: ignore[reportUnnecessaryCast]
    assert response_format_dict["type"] == "json_schema"
    assert "schema" in response_format_dict["json_schema"]
        "name": response_format_dict["json_schema"]["name"],
        "schema": response_format_dict["json_schema"]["schema"],
def parse_response(
    text_format: type[TextFormatT] | Omit,
    input_tools: Iterable[ToolParam] | Omit | None,
    response: Response | ParsedResponse[object],
) -> ParsedResponse[TextFormatT]:
    output_list: List[ParsedResponseOutputItem[TextFormatT]] = []
    for output in response.output:
        if output.type == "message":
            content_list: List[ParsedContent[TextFormatT]] = []
                    content_list.append(item)
                content_list.append(
                        type_=ParsedResponseOutputText[TextFormatT],
                            **item.to_dict(),
                            "parsed": parse_text(item.text, text_format=text_format),
            output_list.append(
                    type_=ParsedResponseOutputMessage[TextFormatT],
                        **output.to_dict(),
                        "content": content_list,
        elif output.type == "function_call":
                    type_=ParsedResponseFunctionToolCall,
                            input_tools=input_tools, function_call=output
        elif (
            output.type == "computer_call"
            or output.type == "file_search_call"
            or output.type == "web_search_call"
            or output.type == "tool_search_call"
            or output.type == "tool_search_output"
            or output.type == "reasoning"
            or output.type == "compaction"
            or output.type == "mcp_call"
            or output.type == "mcp_approval_request"
            or output.type == "mcp_approval_response"
            or output.type == "image_generation_call"
            or output.type == "code_interpreter_call"
            or output.type == "local_shell_call"
            or output.type == "local_shell_call_output"
            or output.type == "shell_call"
            or output.type == "shell_call_output"
            or output.type == "apply_patch_call"
            or output.type == "apply_patch_call_output"
            or output.type == "mcp_list_tools"
            or output.type == "exec"
            or output.type == "custom_tool_call"
            or output.type == "function_call_output"
            or output.type == "computer_call_output"
            or output.type == "custom_tool_call_output"
            output_list.append(output)
        elif TYPE_CHECKING:  # type: ignore
            assert_never(output)
        type_=ParsedResponse[TextFormatT],
            **response.to_dict(),
            "output": output_list,
def parse_text(text: str, text_format: type[TextFormatT] | Omit) -> TextFormatT | None:
    if not is_given(text_format):
    if is_basemodel_type(text_format):
        return cast(TextFormatT, model_parse_json(text_format, text))
    if is_dataclass_like_type(text_format):
            raise TypeError(f"Non BaseModel types are only supported with Pydantic v2 - {text_format}")
        return pydantic.TypeAdapter(text_format).validate_json(text)
    raise TypeError(f"Unable to automatically parse response format type {text_format}")
def get_input_tool_by_name(*, input_tools: Iterable[ToolParam], name: str) -> FunctionToolParam | None:
    for tool in input_tools:
        if tool["type"] == "function" and tool.get("name") == name:
            return tool
    function_call: ParsedResponseFunctionToolCall | ResponseFunctionToolCall,
    if input_tools is None or not is_given(input_tools):
    input_tool = get_input_tool_by_name(input_tools=input_tools, name=function_call.name)
    tool = cast(object, input_tool)
    if isinstance(tool, ResponsesPydanticFunctionTool):
        return model_parse_json(tool.model, function_call.arguments)
    if not input_tool.get("strict"):
    return json.loads(function_call.arguments)
from typing import Any, List, Generic, Iterable, Awaitable, cast
from typing_extensions import Self, Callable, Iterator, AsyncIterator
from ._types import ParsedResponseSnapshot
    ResponseStreamEvent,
from ...._types import Omit, omit
from ...._models import build, construct_type_unchecked
from ....types.responses import ParsedResponse, ResponseStreamEvent as RawResponseStreamEvent
from ..._parsing._responses import TextFormatT, parse_text, parse_response
from ....types.responses.tool_param import ToolParam
from ....types.responses.parsed_response import (
class ResponseStream(Generic[TextFormatT]):
        raw_stream: Stream[RawResponseStreamEvent],
        input_tools: Iterable[ToolParam] | Omit,
        starting_after: int | None,
        self._state = ResponseStreamState(text_format=text_format, input_tools=input_tools)
        self._starting_after = starting_after
    def __next__(self) -> ResponseStreamEvent[TextFormatT]:
    def __iter__(self) -> Iterator[ResponseStreamEvent[TextFormatT]]:
    def __stream__(self) -> Iterator[ResponseStreamEvent[TextFormatT]]:
            events_to_fire = self._state.handle_event(sse_event)
                if self._starting_after is None or event.sequence_number > self._starting_after:
    def get_final_response(self) -> ParsedResponse[TextFormatT]:
        the accumulated `ParsedResponse` object.
        response = self._state._completed_response
        if not response:
            raise RuntimeError("Didn't receive a `response.completed` event.")
class ResponseStreamManager(Generic[TextFormatT]):
        api_request: Callable[[], Stream[RawResponseStreamEvent]],
        self.__stream: ResponseStream[TextFormatT] | None = None
        self.__text_format = text_format
        self.__starting_after = starting_after
    def __enter__(self) -> ResponseStream[TextFormatT]:
        self.__stream = ResponseStream(
            text_format=self.__text_format,
            starting_after=self.__starting_after,
class AsyncResponseStream(Generic[TextFormatT]):
        raw_stream: AsyncStream[RawResponseStreamEvent],
    async def __anext__(self) -> ResponseStreamEvent[TextFormatT]:
    async def __aiter__(self) -> AsyncIterator[ResponseStreamEvent[TextFormatT]]:
    async def __stream__(self) -> AsyncIterator[ResponseStreamEvent[TextFormatT]]:
    async def get_final_response(self) -> ParsedResponse[TextFormatT]:
class AsyncResponseStreamManager(Generic[TextFormatT]):
        api_request: Awaitable[AsyncStream[RawResponseStreamEvent]],
        self.__stream: AsyncResponseStream[TextFormatT] | None = None
    async def __aenter__(self) -> AsyncResponseStream[TextFormatT]:
        self.__stream = AsyncResponseStream(
class ResponseStreamState(Generic[TextFormatT]):
        self.__current_snapshot: ParsedResponseSnapshot | None = None
        self._completed_response: ParsedResponse[TextFormatT] | None = None
        self._text_format = text_format
        self._rich_text_format: type | Omit = text_format if inspect.isclass(text_format) else omit
    def handle_event(self, event: RawResponseStreamEvent) -> List[ResponseStreamEvent[TextFormatT]]:
        self.__current_snapshot = snapshot = self.accumulate_event(event)
        events: List[ResponseStreamEvent[TextFormatT]] = []
            output = snapshot.output[event.output_index]
            assert output.type == "message"
            content = output.content[event.content_index]
            assert content.type == "output_text"
            events.append(
                    content_index=event.content_index,
                    delta=event.delta,
                    item_id=event.item_id,
                    output_index=event.output_index,
                    sequence_number=event.sequence_number,
                    logprobs=event.logprobs,
                    type="response.output_text.delta",
                    snapshot=content.text,
                    type="response.output_text.done",
                    text=event.text,
                    parsed=parse_text(event.text, text_format=self._text_format),
        elif event.type == "response.function_call_arguments.delta":
            assert output.type == "function_call"
                    type="response.function_call_arguments.delta",
                    snapshot=output.arguments,
        elif event.type == "response.completed":
            response = self._completed_response
            assert response is not None
                    type="response.completed",
            events.append(event)
    def accumulate_event(self, event: RawResponseStreamEvent) -> ParsedResponseSnapshot:
        snapshot = self.__current_snapshot
        if snapshot is None:
            return self._create_initial_response(event)
        if event.type == "response.output_item.added":
            if event.item.type == "function_call":
                snapshot.output.append(
                        type_=cast(Any, ParsedResponseFunctionToolCall), value=event.item.to_dict()
            elif event.item.type == "message":
                    construct_type_unchecked(type_=cast(Any, ParsedResponseOutputMessage), value=event.item.to_dict())
                snapshot.output.append(event.item)
        elif event.type == "response.content_part.added":
                output.content.append(
                    construct_type_unchecked(type_=cast(Any, ParsedContent), value=event.part.to_dict())
        elif event.type == "response.output_text.delta":
                content.text += event.delta
            if output.type == "function_call":
                output.arguments += event.delta
            self._completed_response = parse_response(
                text_format=self._text_format,
                response=event.response,
        return snapshot
    def _create_initial_response(self, event: RawResponseStreamEvent) -> ParsedResponseSnapshot:
        if event.type != "response.created":
            raise RuntimeError(f"Expected to have received `response.created` before `{event.type}`")
        return construct_type_unchecked(type_=ParsedResponseSnapshot, value=event.response.to_dict())
from typing import TYPE_CHECKING, Any, List, Iterable, cast
from ._completions import solve_response_format_t, type_to_response_format_param
    solved_t = solve_response_format_t(text_format)
                        type_=cast(Any, ParsedResponseOutputText)[solved_t],
                    type_=cast(Any, ParsedResponseOutputMessage)[solved_t],
        ParsedResponse[TextFormatT],
            type_=cast(Any, ParsedResponse)[solved_t],
