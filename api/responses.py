from copy import copy
from typing import TYPE_CHECKING, Any, List, Type, Union, Iterable, Iterator, Optional, AsyncIterator, cast
from ..._types import NOT_GIVEN, Body, Omit, Query, Headers, NoneType, NotGiven, omit, not_given
from ..._utils import is_given, path_template, maybe_transform, strip_not_given, async_maybe_transform
from ...lib._tools import PydanticFunctionTool, ResponsesPydanticFunctionTool
from ..._base_client import _merge_mappings, make_request_options
    response_create_params,
    response_compact_params,
    response_retrieve_params,
    responses_client_event_param,
from ...lib._parsing._responses import (
    TextFormatT,
    parse_response,
    type_to_text_format_param as _type_to_text_format_param,
from ...types.responses.response import Response
from ...types.responses.tool_param import ToolParam, ParseableToolParam
from ...types.responses.parsed_response import ParsedResponse
from ...lib.streaming.responses._responses import ResponseStreamManager, AsyncResponseStreamManager
from ...types.responses.compacted_response import CompactedResponse
from ...types.shared_params.responses_model import ResponsesModel
from ...types.responses.response_input_param import ResponseInputParam
from ...types.responses.response_stream_event import ResponseStreamEvent
from ...types.responses.responses_client_event import ResponsesClientEvent
from ...types.responses.responses_server_event import ResponsesServerEvent
from ...types.responses.response_text_config_param import ResponseTextConfigParam
from ...types.responses.responses_client_event_param import ResponsesClientEventParam
__all__ = ["Responses", "AsyncResponses"]
class Responses(SyncAPIResource):
    def input_items(self) -> InputItems:
        return InputItems(self._client)
    def input_tokens(self) -> InputTokens:
        return InputTokens(self._client)
    def with_raw_response(self) -> ResponsesWithRawResponse:
        return ResponsesWithRawResponse(self)
    def with_streaming_response(self) -> ResponsesWithStreamingResponse:
        return ResponsesWithStreamingResponse(self)
        background: Optional[bool] | Omit = omit,
        context_management: Optional[Iterable[response_create_params.ContextManagement]] | Omit = omit,
        conversation: Optional[response_create_params.Conversation] | Omit = omit,
        include: Optional[List[ResponseIncludable]] | Omit = omit,
        input: Union[str, ResponseInputParam] | Omit = omit,
        max_output_tokens: Optional[int] | Omit = omit,
        max_tool_calls: Optional[int] | Omit = omit,
        model: ResponsesModel | Omit = omit,
        stream_options: Optional[response_create_params.StreamOptions] | Omit = omit,
        text: ResponseTextConfigParam | Omit = omit,
        tool_choice: response_create_params.ToolChoice | Omit = omit,
        tools: Iterable[ToolParam] | Omit = omit,
        truncation: Optional[Literal["auto", "disabled"]] | Omit = omit,
    ) -> Response:
        """Creates a model response.
        Provide
        [text](https://platform.openai.com/docs/guides/text) or
        [image](https://platform.openai.com/docs/guides/images) inputs to generate
        [JSON](https://platform.openai.com/docs/guides/structured-outputs) outputs. Have
        the model call your own
        [custom code](https://platform.openai.com/docs/guides/function-calling) or use
        built-in [tools](https://platform.openai.com/docs/guides/tools) like
        [web search](https://platform.openai.com/docs/guides/tools-web-search) or
        [file search](https://platform.openai.com/docs/guides/tools-file-search) to use
        your own data as input for the model's response.
          background: Whether to run the model response in the background.
              [Learn more](https://platform.openai.com/docs/guides/background).
          context_management: Context management configuration for this request.
          input: Text, image, or file inputs to the model, used to generate a response.
              Learn more:
              - [Image inputs](https://platform.openai.com/docs/guides/images)
              - [File inputs](https://platform.openai.com/docs/guides/pdf-files)
              - [Conversation state](https://platform.openai.com/docs/guides/conversation-state)
              - [Function calling](https://platform.openai.com/docs/guides/function-calling)
          instructions: A system (or developer) message inserted into the model's context.
              When using along with `previous_response_id`, the instructions from a previous
              response will not be carried over to the next response. This makes it simple to
              swap out system (or developer) messages in new responses.
          max_output_tokens: An upper bound for the number of tokens that can be generated for a response,
          max_tool_calls: The maximum number of total calls to built-in tools that can be processed in a
              response. This maximum number applies across all built-in tool calls, not per
              individual tool. Any further attempts to call a tool by the model will be
              ignored.
          reasoning: **gpt-5 and o-series models only**
              Configuration options for
          store: Whether to store the generated model response for later retrieval via API.
              [Streaming section below](https://platform.openai.com/docs/api-reference/responses-streaming)
          stream_options: Options for streaming responses. Only set this when you set `stream: true`.
          tool_choice: How the model should select which tool (or tools) to use when generating a
              response. See the `tools` parameter to see how to specify which tools the model
              can call.
              We support the following categories of tools:
              - **Built-in tools**: Tools that are provided by OpenAI that extend the model's
                capabilities, like
                [file search](https://platform.openai.com/docs/guides/tools-file-search).
                Learn more about
                [built-in tools](https://platform.openai.com/docs/guides/tools).
              - **MCP Tools**: Integrations with third-party systems via custom MCP servers or
                predefined connectors such as Google Drive and SharePoint. Learn more about
                [MCP Tools](https://platform.openai.com/docs/guides/tools-connectors-mcp).
              - **Function calls (custom tools)**: Functions that are defined by you, enabling
                the model to call your own code with strongly typed arguments and outputs.
                [function calling](https://platform.openai.com/docs/guides/function-calling).
                You can also use custom tools to call your own code.
          truncation: The truncation strategy to use for the model response.
              - `auto`: If the input to this Response exceeds the model's context window size,
                the model will truncate the response to fit the context window by dropping
                items from the beginning of the conversation.
              - `disabled` (default): If the input size will exceed the context window size
                for a model, the request will fail with a 400 error.
    ) -> Stream[ResponseStreamEvent]:
    ) -> Response | Stream[ResponseStreamEvent]:
            "/responses",
                    "context_management": context_management,
                    "max_tool_calls": max_tool_calls,
                response_create_params.ResponseCreateParamsStreaming
                else response_create_params.ResponseCreateParamsNonStreaming,
            cast_to=Response,
            stream_cls=Stream[ResponseStreamEvent],
        text_format: type[TextFormatT] | Omit = omit,
        starting_after: int | Omit = omit,
        tools: Iterable[ParseableToolParam] | Omit = omit,
    ) -> ResponseStreamManager[TextFormatT]: ...
        input: Union[str, ResponseInputParam],
        model: ResponsesModel,
        response_id: str | Omit = omit,
    ) -> ResponseStreamManager[TextFormatT]:
        new_response_args = {
        new_response_args_names = [k for k, v in new_response_args.items() if is_given(v)]
        if (is_given(response_id) or is_given(starting_after)) and len(new_response_args_names) > 0:
                "Cannot provide both response_id/starting_after can't be provided together with "
                + ", ".join(new_response_args_names)
        tools = _make_tools(tools)
        if len(new_response_args_names) > 0:
            if not is_given(input):
                raise ValueError("input must be provided when creating a new response")
            if not is_given(model):
                raise ValueError("model must be provided when creating a new response")
            if is_given(text_format):
                if not text:
                    text = {}
                if "format" in text:
                    raise TypeError("Cannot mix and match text.format with text_format")
                text = copy(text)
                text["format"] = _type_to_text_format_param(text_format)
            api_request: partial[Stream[ResponseStreamEvent]] = partial(
                input=input,
                context_management=context_management,
                conversation=conversation,
                max_output_tokens=max_output_tokens,
                max_tool_calls=max_tool_calls,
                text=text,
                reasoning=reasoning,
                truncation=truncation,
                background=background,
            return ResponseStreamManager(api_request, text_format=text_format, input_tools=tools, starting_after=None)
            if not is_given(response_id):
                raise ValueError("id must be provided when streaming an existing response")
            return ResponseStreamManager(
                lambda: self.retrieve(
                    include=include or [],
                    starting_after=omit,
                text_format=text_format,
                starting_after=starting_after if is_given(starting_after) else None,
        def parser(raw_response: Response) -> ParsedResponse[TextFormatT]:
            return parse_response(
                response=raw_response,
                response_create_params.ResponseCreateParams,
            # we turn the `Response` instance into a `ParsedResponse`
            cast_to=cast(Type[ParsedResponse[TextFormatT]], Response),
        include_obfuscation: bool | Omit = omit,
        stream: Literal[False] | Omit = omit,
    ) -> Response: ...
    ) -> Stream[ResponseStreamEvent]: ...
    ) -> Response | Stream[ResponseStreamEvent]: ...
        Retrieves a model response with the given ID.
          include_obfuscation: When true, stream obfuscation will be enabled. Stream obfuscation adds random
              characters to an `obfuscation` field on streaming delta events to normalize
              payload sizes as a mitigation to certain side-channel attacks. These obfuscation
              fields are included by default, but add a small amount of overhead to the data
              stream. You can set `include_obfuscation` to false to optimize for bandwidth if
              you trust the network links between your application and the OpenAI API.
          starting_after: The sequence number of the event after which to start streaming.
        stream: Literal[False] | Literal[True] | Omit = omit,
            path_template("/responses/{response_id}", response_id=response_id),
                        "include_obfuscation": include_obfuscation,
                        "starting_after": starting_after,
                    response_retrieve_params.ResponseRetrieveParams,
        Deletes a model response with the given ID.
        """Cancels a model response with the given ID.
        Only responses created with the
        `background` parameter set to `true` can be cancelled.
            path_template("/responses/{response_id}/cancel", response_id=response_id),
    def compact(
                "gpt-5.4",
                "gpt-5.4-mini",
                "gpt-5.4-nano",
                "gpt-5.4-mini-2026-03-17",
                "gpt-5.4-nano-2026-03-17",
                "gpt-5.3-chat-latest",
                "gpt-5.2",
                "gpt-5.2-2025-12-11",
                "gpt-5.2-chat-latest",
                "gpt-5.2-pro",
                "gpt-5.2-pro-2025-12-11",
                "gpt-5.1",
                "gpt-5.1-2025-11-13",
                "gpt-5.1-codex",
                "gpt-5.1-mini",
                "gpt-5.1-chat-latest",
                "gpt-5-chat-latest",
                "o4-mini",
                "o4-mini-2025-04-16",
                "o3",
                "o3-2025-04-16",
                "o1-preview",
                "o1-preview-2024-09-12",
                "o1-mini",
                "o1-mini-2024-09-12",
                "gpt-4o-audio-preview",
                "gpt-4o-audio-preview-2024-10-01",
                "gpt-4o-audio-preview-2024-12-17",
                "gpt-4o-audio-preview-2025-06-03",
                "gpt-4o-mini-audio-preview",
                "gpt-4o-mini-audio-preview-2024-12-17",
                "gpt-4o-search-preview",
                "gpt-4o-mini-search-preview",
                "gpt-4o-search-preview-2025-03-11",
                "gpt-4o-mini-search-preview-2025-03-11",
                "chatgpt-4o-latest",
                "codex-mini-latest",
                "gpt-3.5-turbo-0301",
                "o1-pro",
                "o1-pro-2025-03-19",
                "o3-pro",
                "o3-pro-2025-06-10",
                "o3-deep-research",
                "o3-deep-research-2025-06-26",
                "o4-mini-deep-research",
                "o4-mini-deep-research-2025-06-26",
                "computer-use-preview",
                "computer-use-preview-2025-03-11",
                "gpt-5-codex",
                "gpt-5-pro",
                "gpt-5-pro-2025-10-06",
                "gpt-5.1-codex-max",
        prompt_cache_key: Optional[str] | Omit = omit,
    ) -> CompactedResponse:
        """Compact a conversation.
        Returns a compacted response object.
        Learn when and how to compact long-running conversations in the
        [conversation state guide](https://platform.openai.com/docs/guides/conversation-state#managing-the-context-window).
        For ZDR-compatible compaction details, see
        [Compaction (advanced)](https://platform.openai.com/docs/guides/conversation-state#compaction-advanced).
          model: Model ID used to generate the response, like `gpt-5` or `o3`. OpenAI offers a
          prompt_cache_key: A key to use when reading from or writing to the prompt cache.
            "/responses/compact",
                response_compact_params.ResponseCompactParams,
            cast_to=CompactedResponse,
    ) -> ResponsesConnectionManager:
        """Connect to a persistent Responses API WebSocket.
        Send `response.create` events and receive response stream events over the socket.
        return ResponsesConnectionManager(
class AsyncResponses(AsyncAPIResource):
    def input_items(self) -> AsyncInputItems:
        return AsyncInputItems(self._client)
    def input_tokens(self) -> AsyncInputTokens:
        return AsyncInputTokens(self._client)
    def with_raw_response(self) -> AsyncResponsesWithRawResponse:
        return AsyncResponsesWithRawResponse(self)
    def with_streaming_response(self) -> AsyncResponsesWithStreamingResponse:
        return AsyncResponsesWithStreamingResponse(self)
    ) -> AsyncStream[ResponseStreamEvent]:
    ) -> Response | AsyncStream[ResponseStreamEvent]:
            stream_cls=AsyncStream[ResponseStreamEvent],
    ) -> AsyncResponseStreamManager[TextFormatT]: ...
    ) -> AsyncResponseStreamManager[TextFormatT]:
            if isinstance(input, NotGiven):
            return AsyncResponseStreamManager(
                starting_after=None,
            if isinstance(response_id, Omit):
                raise ValueError("response_id must be provided when streaming an existing response")
            api_request = self.retrieve(
                response_id,
    ) -> AsyncStream[ResponseStreamEvent]: ...
    ) -> Response | AsyncStream[ResponseStreamEvent]: ...
    async def compact(
    ) -> AsyncResponsesConnectionManager:
        return AsyncResponsesConnectionManager(
class ResponsesWithRawResponse:
    def __init__(self, responses: Responses) -> None:
        self._responses = responses
            responses.create,
            responses.retrieve,
            responses.delete,
            responses.cancel,
        self.compact = _legacy_response.to_raw_response_wrapper(
            responses.compact,
            responses.parse,
    def input_items(self) -> InputItemsWithRawResponse:
        return InputItemsWithRawResponse(self._responses.input_items)
    def input_tokens(self) -> InputTokensWithRawResponse:
        return InputTokensWithRawResponse(self._responses.input_tokens)
class AsyncResponsesWithRawResponse:
    def __init__(self, responses: AsyncResponses) -> None:
        self.compact = _legacy_response.async_to_raw_response_wrapper(
    def input_items(self) -> AsyncInputItemsWithRawResponse:
        return AsyncInputItemsWithRawResponse(self._responses.input_items)
    def input_tokens(self) -> AsyncInputTokensWithRawResponse:
        return AsyncInputTokensWithRawResponse(self._responses.input_tokens)
class ResponsesWithStreamingResponse:
        self.compact = to_streamed_response_wrapper(
    def input_items(self) -> InputItemsWithStreamingResponse:
        return InputItemsWithStreamingResponse(self._responses.input_items)
    def input_tokens(self) -> InputTokensWithStreamingResponse:
        return InputTokensWithStreamingResponse(self._responses.input_tokens)
class AsyncResponsesWithStreamingResponse:
        self.compact = async_to_streamed_response_wrapper(
    def input_items(self) -> AsyncInputItemsWithStreamingResponse:
        return AsyncInputItemsWithStreamingResponse(self._responses.input_items)
    def input_tokens(self) -> AsyncInputTokensWithStreamingResponse:
        return AsyncInputTokensWithStreamingResponse(self._responses.input_tokens)
def _make_tools(tools: Iterable[ParseableToolParam] | Omit) -> List[ToolParam] | Omit:
    converted_tools: List[ToolParam] = []
            converted_tools.append(tool)
        if "function" not in tool:
            # standard Responses API case
        function = cast(Any, tool)["function"]  # pyright: ignore[reportUnnecessaryCast]
        if not isinstance(function, PydanticFunctionTool):
                "Expected Chat Completions function tool shape to be created using `openai.pydantic_function_tool()`"
        assert "parameters" in function
        new_tool = ResponsesPydanticFunctionTool(
                "name": function["name"],
                "description": function.get("description"),
                "parameters": function["parameters"],
                "strict": function.get("strict") or False,
            function.model,
        converted_tools.append(new_tool.cast())
    return converted_tools
class AsyncResponsesConnection:
    """Represents a live WebSocket connection to the Responses API"""
    response: AsyncResponsesResponseResource
        self.response = AsyncResponsesResponseResource(self)
    async def __aiter__(self) -> AsyncIterator[ResponsesServerEvent]:
    async def recv(self) -> ResponsesServerEvent:
        Receive the next message from the connection and parses it into a `ResponsesServerEvent` object.
        If you want to parse the message into a `ResponsesServerEvent` object like `.recv()` does,
    async def send(self, event: ResponsesClientEvent | ResponsesClientEventParam) -> None:
            else json.dumps(await async_maybe_transform(event, ResponsesClientEventParam))
    def parse_event(self, data: str | bytes) -> ResponsesServerEvent:
        Converts a raw `str` or `bytes` message into a `ResponsesServerEvent` object.
            ResponsesServerEvent,
            construct_type_unchecked(value=json.loads(data), type_=cast(Any, ResponsesServerEvent)),
class AsyncResponsesConnectionManager:
    Context manager over a `AsyncResponsesConnection` that is returned by `responses.connect()`
    connection = await client.responses.connect(...).enter()
        self.__connection: AsyncResponsesConnection | None = None
    async def __aenter__(self) -> AsyncResponsesConnection:
                **self.__extra_query,
        self.__connection = AsyncResponsesConnection(
                        **self.__client.auth_headers,
        merge_raw_path = base_url.raw_path.rstrip(b"/") + b"/responses"
class ResponsesConnection:
    response: ResponsesResponseResource
        self.response = ResponsesResponseResource(self)
    def __iter__(self) -> Iterator[ResponsesServerEvent]:
    def recv(self) -> ResponsesServerEvent:
    def send(self, event: ResponsesClientEvent | ResponsesClientEventParam) -> None:
            else json.dumps(maybe_transform(event, ResponsesClientEventParam))
class ResponsesConnectionManager:
    Context manager over a `ResponsesConnection` that is returned by `responses.connect()`
    connection = client.responses.connect(...).enter()
        self.__connection: ResponsesConnection | None = None
    def __enter__(self) -> ResponsesConnection:
        self.__connection = ResponsesConnection(
class BaseResponsesConnectionResource:
    def __init__(self, connection: ResponsesConnection) -> None:
class ResponsesResponseResource(BaseResponsesConnectionResource):
        context_management: Optional[Iterable[responses_client_event_param.ContextManagement]] | Omit = omit,
        conversation: Optional[responses_client_event_param.Conversation] | Omit = omit,
        stream: Optional[bool] | Omit = omit,
        stream_options: Optional[responses_client_event_param.StreamOptions] | Omit = omit,
        tool_choice: responses_client_event_param.ToolChoice | Omit = omit,
                ResponsesClientEventParam,
                        "type": "response.create",
class BaseAsyncResponsesConnectionResource:
    def __init__(self, connection: AsyncResponsesConnection) -> None:
class AsyncResponsesResponseResource(BaseAsyncResponsesConnectionResource):
from fastapi.exceptions import FastAPIDeprecationWarning
from fastapi.sse import EventSourceResponse as EventSourceResponse  # noqa
from starlette.responses import FileResponse as FileResponse  # noqa
from starlette.responses import HTMLResponse as HTMLResponse  # noqa
from starlette.responses import JSONResponse as JSONResponse  # noqa
from starlette.responses import PlainTextResponse as PlainTextResponse  # noqa
from starlette.responses import RedirectResponse as RedirectResponse  # noqa
from starlette.responses import Response as Response  # noqa
from starlette.responses import StreamingResponse as StreamingResponse  # noqa
from typing_extensions import deprecated
    import ujson
except ImportError:  # pragma: nocover
    ujson = None  # type: ignore
    import orjson
    orjson = None  # type: ignore
    "UJSONResponse is deprecated, FastAPI now serializes data directly to JSON "
    "bytes via Pydantic when a return type or response model is set, which is "
    "faster and doesn't need a custom response class. Read more in the FastAPI "
    "docs: https://fastapi.tiangolo.com/advanced/custom-response/#orjson-or-response-model "
    "and https://fastapi.tiangolo.com/tutorial/response-model/",
    category=FastAPIDeprecationWarning,
class UJSONResponse(JSONResponse):
    """JSON response using the ujson library to serialize data to JSON.
    **Deprecated**: `UJSONResponse` is deprecated. FastAPI now serializes data
    directly to JSON bytes via Pydantic when a return type or response model is
    set, which is faster and doesn't need a custom response class.
    Read more in the
    [FastAPI docs for Custom Response](https://fastapi.tiangolo.com/advanced/custom-response/#orjson-or-response-model)
    and the
    [FastAPI docs for Response Model](https://fastapi.tiangolo.com/tutorial/response-model/).
    **Note**: `ujson` is not included with FastAPI and must be installed
    separately, e.g. `pip install ujson`.
    def render(self, content: Any) -> bytes:
        assert ujson is not None, "ujson must be installed to use UJSONResponse"
        return ujson.dumps(content, ensure_ascii=False).encode("utf-8")
    "ORJSONResponse is deprecated, FastAPI now serializes data directly to JSON "
class ORJSONResponse(JSONResponse):
    """JSON response using the orjson library to serialize data to JSON.
    **Deprecated**: `ORJSONResponse` is deprecated. FastAPI now serializes data
    **Note**: `orjson` is not included with FastAPI and must be installed
    separately, e.g. `pip install orjson`.
        assert orjson is not None, "orjson must be installed to use ORJSONResponse"
        return orjson.dumps(
            content, option=orjson.OPT_NON_STR_KEYS | orjson.OPT_SERIALIZE_NUMPY
from collections.abc import AsyncIterable, Awaitable, Callable, Iterable, Mapping, Sequence
from email.utils import format_datetime, formatdate
from mimetypes import guess_type
from secrets import token_hex
from starlette._utils import collapse_excgroups
from starlette.background import BackgroundTask
from starlette.concurrency import iterate_in_threadpool
from starlette.datastructures import URL, Headers, MutableHeaders
from starlette.requests import ClientDisconnect
from starlette.types import Receive, Scope, Send
    media_type = None
        content: Any = None,
        status_code: int = 200,
        headers: Mapping[str, str] | None = None,
        media_type: str | None = None,
        background: BackgroundTask | None = None,
        if media_type is not None:
            self.media_type = media_type
        self.background = background
        self.body = self.render(content)
        self.init_headers(headers)
    def render(self, content: Any) -> bytes | memoryview:
        if isinstance(content, bytes | memoryview):
        return content.encode(self.charset)  # type: ignore
    def init_headers(self, headers: Mapping[str, str] | None = None) -> None:
            raw_headers: list[tuple[bytes, bytes]] = []
            populate_content_length = True
            populate_content_type = True
            raw_headers = [(k.lower().encode("latin-1"), v.encode("latin-1")) for k, v in headers.items()]
            keys = [h[0] for h in raw_headers]
            populate_content_length = b"content-length" not in keys
            populate_content_type = b"content-type" not in keys
        body = getattr(self, "body", None)
            body is not None
            and populate_content_length
            and not (self.status_code < 200 or self.status_code in (204, 304))
            content_length = str(len(body))
            raw_headers.append((b"content-length", content_length.encode("latin-1")))
        content_type = self.media_type
        if content_type is not None and populate_content_type:
            if content_type.startswith("text/") and "charset=" not in content_type.lower():
                content_type += "; charset=" + self.charset
            raw_headers.append((b"content-type", content_type.encode("latin-1")))
        self.raw_headers = raw_headers
    def headers(self) -> MutableHeaders:
        if not hasattr(self, "_headers"):
            self._headers = MutableHeaders(raw=self.raw_headers)
        value: str = "",
        max_age: int | None = None,
        expires: datetime | str | int | None = None,
        path: str | None = "/",
        domain: str | None = None,
        secure: bool = False,
        httponly: bool = False,
        samesite: Literal["lax", "strict", "none"] | None = "lax",
        partitioned: bool = False,
        cookie: http.cookies.BaseCookie[str] = http.cookies.SimpleCookie()
        cookie[key] = value
            cookie[key]["max-age"] = max_age
            if isinstance(expires, datetime):
                cookie[key]["expires"] = format_datetime(expires, usegmt=True)
                cookie[key]["expires"] = expires
            cookie[key]["path"] = path
            cookie[key]["domain"] = domain
            cookie[key]["secure"] = True
            cookie[key]["httponly"] = True
        if samesite is not None:
            assert samesite.lower() in [
                "strict",
                "lax",
                "none",
            ], "samesite must be either 'strict', 'lax' or 'none'"
            cookie[key]["samesite"] = samesite
        if partitioned:
            if sys.version_info < (3, 14):
                raise ValueError("Partitioned cookies are only supported in Python 3.14 and above.")  # pragma: no cover
            cookie[key]["partitioned"] = True  # pragma: no cover
        cookie_val = cookie.output(header="").strip()
        self.raw_headers.append((b"set-cookie", cookie_val.encode("latin-1")))
    def delete_cookie(
        path: str = "/",
            expires=0,
            httponly=httponly,
    async def __call__(self, scope: Scope, receive: Receive, send: Send) -> None:
        prefix = "websocket." if scope["type"] == "websocket" else ""
        await send(
                "type": prefix + "http.response.start",
                "status": self.status_code,
                "headers": self.raw_headers,
        await send({"type": prefix + "http.response.body", "body": self.body})
        if self.background is not None:
            await self.background()
class HTMLResponse(Response):
    media_type = "text/html"
class PlainTextResponse(Response):
    media_type = "text/plain"
class JSONResponse(Response):
    media_type = "application/json"
        super().__init__(content, status_code, headers, media_type, background)
            indent=None,
        ).encode("utf-8")
class RedirectResponse(Response):
        url: str | URL,
        status_code: int = 307,
        super().__init__(content=b"", status_code=status_code, headers=headers, background=background)
        self.headers["location"] = quote(str(url), safe=":/%#?=@[]!$&'()*+,;")
Content = str | bytes | memoryview
SyncContentStream = Iterable[Content]
AsyncContentStream = AsyncIterable[Content]
ContentStream = AsyncContentStream | SyncContentStream
class StreamingResponse(Response):
    body_iterator: AsyncContentStream
        content: ContentStream,
        if isinstance(content, AsyncIterable):
            self.body_iterator = content
            self.body_iterator = iterate_in_threadpool(content)
        self.media_type = self.media_type if media_type is None else media_type
    async def listen_for_disconnect(self, receive: Receive) -> None:
            message = await receive()
            if message["type"] == "http.disconnect":
    async def stream_response(self, send: Send) -> None:
                "type": "http.response.start",
        async for chunk in self.body_iterator:
            if not isinstance(chunk, bytes | memoryview):
                chunk = chunk.encode(self.charset)
            await send({"type": "http.response.body", "body": chunk, "more_body": True})
        await send({"type": "http.response.body", "body": b"", "more_body": False})
        spec_version = tuple(map(int, scope.get("asgi", {}).get("spec_version", "2.0").split(".")))
        if spec_version >= (2, 4):
                await self.stream_response(send)
                raise ClientDisconnect()
            with collapse_excgroups():
                async with anyio.create_task_group() as task_group:
                    async def wrap(func: Callable[[], Awaitable[None]]) -> None:
                        await func()
                        task_group.cancel_scope.cancel()
                    task_group.start_soon(wrap, partial(self.stream_response, send))
                    await wrap(partial(self.listen_for_disconnect, receive))
class MalformedRangeHeader(Exception):
    def __init__(self, content: str = "Malformed range header.") -> None:
class RangeNotSatisfiable(Exception):
    def __init__(self, max_size: int) -> None:
        self.max_size = max_size
class FileResponse(Response):
    chunk_size = 64 * 1024
        path: str | os.PathLike[str],
        stat_result: os.stat_result | None = None,
        content_disposition_type: str = "attachment",
        if method is not None:
                "The 'method' parameter is not used, and it will be removed.",
        if media_type is None:
            media_type = guess_type(filename or path)[0] or "text/plain"
        self.headers.setdefault("accept-ranges", "bytes")
        if self.filename is not None:
            content_disposition_filename = quote(self.filename)
            if content_disposition_filename != self.filename:
                content_disposition = f"{content_disposition_type}; filename*=utf-8''{content_disposition_filename}"
                content_disposition = f'{content_disposition_type}; filename="{self.filename}"'
            self.headers.setdefault("content-disposition", content_disposition)
        self.stat_result = stat_result
        if stat_result is not None:
            self.set_stat_headers(stat_result)
    def set_stat_headers(self, stat_result: os.stat_result) -> None:
        content_length = str(stat_result.st_size)
        last_modified = formatdate(stat_result.st_mtime, usegmt=True)
        etag_base = str(stat_result.st_mtime) + "-" + str(stat_result.st_size)
        etag = f'"{hashlib.md5(etag_base.encode(), usedforsecurity=False).hexdigest()}"'
        self.headers.setdefault("content-length", content_length)
        self.headers.setdefault("last-modified", last_modified)
        self.headers.setdefault("etag", etag)
        send_header_only: bool = scope["method"].upper() == "HEAD"
        send_pathsend: bool = "http.response.pathsend" in scope.get("extensions", {})
        if self.stat_result is None:
                stat_result = await anyio.to_thread.run_sync(os.stat, self.path)
                raise RuntimeError(f"File at path {self.path} does not exist.")
                mode = stat_result.st_mode
                if not stat.S_ISREG(mode):
                    raise RuntimeError(f"File at path {self.path} is not a file.")
            stat_result = self.stat_result
        headers = Headers(scope=scope)
        http_range = headers.get("range")
        http_if_range = headers.get("if-range")
        if http_range is None or (http_if_range is not None and not self._should_use_range(http_if_range)):
            await self._handle_simple(send, send_header_only, send_pathsend)
                ranges = self._parse_range_header(http_range, stat_result.st_size)
            except MalformedRangeHeader as exc:
                return await PlainTextResponse(exc.content, status_code=400)(scope, receive, send)
            except RangeNotSatisfiable as exc:
                response = PlainTextResponse(status_code=416, headers={"Content-Range": f"*/{exc.max_size}"})
                return await response(scope, receive, send)
            if len(ranges) == 1:
                start, end = ranges[0]
                await self._handle_single_range(send, start, end, stat_result.st_size, send_header_only)
                await self._handle_multiple_ranges(send, ranges, stat_result.st_size, send_header_only)
    async def _handle_simple(self, send: Send, send_header_only: bool, send_pathsend: bool) -> None:
        await send({"type": "http.response.start", "status": self.status_code, "headers": self.raw_headers})
        if send_header_only:
        elif send_pathsend:
            await send({"type": "http.response.pathsend", "path": str(self.path)})
            async with await anyio.open_file(self.path, mode="rb") as file:
                more_body = True
                while more_body:
                    chunk = await file.read(self.chunk_size)
                    more_body = len(chunk) == self.chunk_size
                    await send({"type": "http.response.body", "body": chunk, "more_body": more_body})
    async def _handle_single_range(
        self, send: Send, start: int, end: int, file_size: int, send_header_only: bool
        self.headers["content-range"] = f"bytes {start}-{end - 1}/{file_size}"
        self.headers["content-length"] = str(end - start)
        await send({"type": "http.response.start", "status": 206, "headers": self.raw_headers})
                await file.seek(start)
                    chunk = await file.read(min(self.chunk_size, end - start))
                    start += len(chunk)
                    more_body = len(chunk) == self.chunk_size and start < end
    async def _handle_multiple_ranges(
        send: Send,
        ranges: list[tuple[int, int]],
        file_size: int,
        send_header_only: bool,
        # In firefox and chrome, they use boundary with 95-96 bits entropy (that's roughly 13 bytes).
        boundary = token_hex(13)
        content_length, header_generator = self.generate_multipart(
            ranges, boundary, file_size, self.headers["content-type"]
        self.headers["content-range"] = f"multipart/byteranges; boundary={boundary}"
        self.headers["content-length"] = str(content_length)
                for start, end in ranges:
                    await send({"type": "http.response.body", "body": header_generator(start, end), "more_body": True})
                    while start < end:
                    await send({"type": "http.response.body", "body": b"\n", "more_body": True})
                        "type": "http.response.body",
                        "body": f"\n--{boundary}--\n".encode("latin-1"),
                        "more_body": False,
    def _should_use_range(self, http_if_range: str) -> bool:
        return http_if_range == self.headers["last-modified"] or http_if_range == self.headers["etag"]
    def _parse_range_header(cls, http_range: str, file_size: int) -> list[tuple[int, int]]:
        ranges: list[tuple[int, int]] = []
            units, range_ = http_range.split("=", 1)
            raise MalformedRangeHeader()
        units = units.strip().lower()
        if units != "bytes":
            raise MalformedRangeHeader("Only support bytes range")
        ranges = cls._parse_ranges(range_, file_size)
        if len(ranges) == 0:
            raise MalformedRangeHeader("Range header: range must be requested")
        if any(not (0 <= start < file_size) for start, _ in ranges):
            raise RangeNotSatisfiable(file_size)
        if any(start > end for start, end in ranges):
            raise MalformedRangeHeader("Range header: start must be less than end")
            return ranges
        # Merge ranges
        result: list[tuple[int, int]] = []
            for p in range(len(result)):
                p_start, p_end = result[p]
                if start > p_end:
                elif end < p_start:
                    result.insert(p, (start, end))  # THIS IS NOT REACHED!
                    result[p] = (min(start, p_start), max(end, p_end))
                result.append((start, end))
    def _parse_ranges(cls, range_: str, file_size: int) -> list[tuple[int, int]]:
        for part in range_.split(","):
            part = part.strip()
            # If the range is empty or a single dash, we ignore it.
            if not part or part == "-":
            # If the range is not in the format "start-end", we ignore it.
            if "-" not in part:
            start_str, end_str = part.split("-", 1)
            start_str = start_str.strip()
            end_str = end_str.strip()
                start = int(start_str) if start_str else file_size - int(end_str)
                end = int(end_str) + 1 if start_str and end_str and int(end_str) < file_size else file_size
                ranges.append((start, end))
                # If the range is not numeric, we ignore it.
    def generate_multipart(
        ranges: Sequence[tuple[int, int]],
        boundary: str,
        max_size: int,
        content_type: str,
    ) -> tuple[int, Callable[[int, int], bytes]]:
        Multipart response headers generator.
        --{boundary}\n
        Content-Type: {content_type}\n
        Content-Range: bytes {start}-{end-1}/{max_size}\n
        \n
        ..........content...........\n
        --{boundary}--\n
        boundary_len = len(boundary)
        static_header_part_len = 44 + boundary_len + len(content_type) + len(str(max_size))
        content_length = sum(
            (len(str(start)) + len(str(end - 1)) + static_header_part_len)  # Headers
            + (end - start)  # Content
            for start, end in ranges
        ) + (
            5 + boundary_len  # --boundary--\n
            content_length,
            lambda start, end: (
                f"--{boundary}\nContent-Type: {content_type}\nContent-Range: bytes {start}-{end - 1}/{max_size}\n\n"
            ).encode("latin-1"),
from typing import Any, List, Type, Union, Iterable, Optional, cast
from ..._utils import is_given, maybe_transform, async_maybe_transform
            f"/responses/{response_id}",
            f"/responses/{response_id}/cancel",
        Compact conversation
    JSON response using the high-performance ujson library to serialize data to JSON.
    [FastAPI docs for Custom Response - HTML, Stream, File, others](https://fastapi.tiangolo.com/advanced/custom-response/).
    JSON response using the high-performance orjson library to serialize data to JSON.
