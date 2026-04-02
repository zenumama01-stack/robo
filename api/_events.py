from typing import List, Union, Generic, Optional
from ._types import ParsedChatCompletionSnapshot
from ...._models import BaseModel, GenericModel
from ..._parsing import ResponseFormatT
from ....types.chat import ChatCompletionChunk, ChatCompletionTokenLogprob
class ChunkEvent(BaseModel):
    type: Literal["chunk"]
    chunk: ChatCompletionChunk
    snapshot: ParsedChatCompletionSnapshot
class ContentDeltaEvent(BaseModel):
    """This event is yielded for every chunk with `choice.delta.content` data."""
    type: Literal["content.delta"]
    delta: str
    snapshot: str
    parsed: Optional[object] = None
class ContentDoneEvent(GenericModel, Generic[ResponseFormatT]):
    type: Literal["content.done"]
    parsed: Optional[ResponseFormatT] = None
class RefusalDeltaEvent(BaseModel):
    type: Literal["refusal.delta"]
class RefusalDoneEvent(BaseModel):
    type: Literal["refusal.done"]
    refusal: str
class FunctionToolCallArgumentsDeltaEvent(BaseModel):
    type: Literal["tool_calls.function.arguments.delta"]
    index: int
    arguments: str
    """Accumulated raw JSON string"""
    parsed_arguments: object
    """The parsed arguments so far"""
    arguments_delta: str
    """The JSON string delta"""
class FunctionToolCallArgumentsDoneEvent(BaseModel):
    type: Literal["tool_calls.function.arguments.done"]
    """The parsed arguments"""
class LogprobsContentDeltaEvent(BaseModel):
    type: Literal["logprobs.content.delta"]
    content: List[ChatCompletionTokenLogprob]
    snapshot: List[ChatCompletionTokenLogprob]
class LogprobsContentDoneEvent(BaseModel):
    type: Literal["logprobs.content.done"]
class LogprobsRefusalDeltaEvent(BaseModel):
    type: Literal["logprobs.refusal.delta"]
    refusal: List[ChatCompletionTokenLogprob]
class LogprobsRefusalDoneEvent(BaseModel):
    type: Literal["logprobs.refusal.done"]
ChatCompletionStreamEvent = Union[
    ContentDoneEvent[ResponseFormatT],
from typing_extensions import Union, Generic, TypeVar, Annotated, TypeAlias
from ...._utils import PropertyInfo
from ...._compat import GenericModel
from ....types.responses import (
    ResponseErrorEvent,
    ResponseQueuedEvent,
    ResponseCreatedEvent,
    ResponseTextDoneEvent as RawResponseTextDoneEvent,
    ResponseAudioDoneEvent,
    ResponseCompletedEvent as RawResponseCompletedEvent,
    ResponseTextDeltaEvent as RawResponseTextDeltaEvent,
    ResponseAudioDeltaEvent,
    ResponseInProgressEvent,
    ResponseRefusalDoneEvent,
    ResponseRefusalDeltaEvent,
    ResponseMcpCallFailedEvent,
    ResponseOutputItemDoneEvent,
    ResponseContentPartDoneEvent,
    ResponseOutputItemAddedEvent,
    ResponseContentPartAddedEvent,
    ResponseMcpCallCompletedEvent,
    ResponseMcpCallInProgressEvent,
    ResponseMcpListToolsFailedEvent,
    ResponseAudioTranscriptDoneEvent,
    ResponseAudioTranscriptDeltaEvent,
    ResponseMcpCallArgumentsDoneEvent,
    ResponseImageGenCallCompletedEvent,
    ResponseMcpCallArgumentsDeltaEvent,
    ResponseMcpListToolsCompletedEvent,
    ResponseImageGenCallGeneratingEvent,
    ResponseImageGenCallInProgressEvent,
    ResponseMcpListToolsInProgressEvent,
    ResponseWebSearchCallCompletedEvent,
    ResponseWebSearchCallSearchingEvent,
    ResponseCustomToolCallInputDoneEvent,
    ResponseFileSearchCallCompletedEvent,
    ResponseFileSearchCallSearchingEvent,
    ResponseWebSearchCallInProgressEvent,
    ResponseCustomToolCallInputDeltaEvent,
    ResponseFileSearchCallInProgressEvent,
    ResponseImageGenCallPartialImageEvent,
    ResponseReasoningSummaryPartDoneEvent,
    ResponseReasoningSummaryTextDoneEvent,
    ResponseFunctionCallArgumentsDoneEvent,
    ResponseOutputTextAnnotationAddedEvent,
    ResponseReasoningSummaryPartAddedEvent,
    ResponseReasoningSummaryTextDeltaEvent,
    ResponseFunctionCallArgumentsDeltaEvent as RawResponseFunctionCallArgumentsDeltaEvent,
    ResponseCodeInterpreterCallCodeDoneEvent,
    ResponseCodeInterpreterCallCodeDeltaEvent,
    ResponseCodeInterpreterCallCompletedEvent,
    ResponseCodeInterpreterCallInProgressEvent,
    ResponseCodeInterpreterCallInterpretingEvent,
from ....types.responses.response_reasoning_text_done_event import ResponseReasoningTextDoneEvent
from ....types.responses.response_reasoning_text_delta_event import ResponseReasoningTextDeltaEvent
class ResponseTextDeltaEvent(RawResponseTextDeltaEvent):
class ResponseTextDoneEvent(RawResponseTextDoneEvent, GenericModel, Generic[TextFormatT]):
    parsed: Optional[TextFormatT] = None
class ResponseFunctionCallArgumentsDeltaEvent(RawResponseFunctionCallArgumentsDeltaEvent):
class ResponseCompletedEvent(RawResponseCompletedEvent, GenericModel, Generic[TextFormatT]):
    response: ParsedResponse[TextFormatT]  # type: ignore[assignment]
ResponseStreamEvent: TypeAlias = Annotated[
    Union[
        # wrappers with snapshots added on
        ResponseTextDeltaEvent,
        ResponseTextDoneEvent[TextFormatT],
        ResponseFunctionCallArgumentsDeltaEvent,
        ResponseCompletedEvent[TextFormatT],
        # the same as the non-accumulated API
        ResponseTextDoneEvent,
        ResponseReasoningTextDeltaEvent,
        ResponseReasoningTextDoneEvent,
    PropertyInfo(discriminator="type"),
# High level events that make up HTTP/1.1 conversations. Loosely inspired by
# the corresponding events in hyper-h2:
#     http://python-hyper.org/h2/en/stable/api.html#events
# Don't subclass these. Stuff will break.
from typing import List, Tuple, Union
from ._abnf import method, request_target
from ._headers import Headers, normalize_and_validate
from ._util import bytesify, LocalProtocolError, validate
# Everything in __all__ gets re-exported as part of the h11 public API.
method_re = re.compile(method.encode("ascii"))
request_target_re = re.compile(request_target.encode("ascii"))
class Event(ABC):
    Base class for h11 events.
@dataclass(init=False, frozen=True)
class Request(Event):
    """The beginning of an HTTP request.
    Fields:
    .. attribute:: method
       An HTTP method, e.g. ``b"GET"`` or ``b"POST"``. Always a byte
       string. :term:`Bytes-like objects <bytes-like object>` and native
       strings containing only ascii characters will be automatically
       converted to byte strings.
    .. attribute:: target
       The target of an HTTP request, e.g. ``b"/index.html"``, or one of the
       more exotic formats described in `RFC 7320, section 5.3
       <https://tools.ietf.org/html/rfc7230#section-5.3>`_. Always a byte
    .. attribute:: headers
       Request headers, represented as a list of (name, value) pairs. See
       :ref:`the header normalization rules <headers-format>` for details.
    .. attribute:: http_version
       The HTTP protocol version, represented as a byte string like
       ``b"1.1"``. See :ref:`the HTTP version normalization rules
       <http_version-format>` for details.
    __slots__ = ("method", "headers", "target", "http_version")
    method: bytes
    target: bytes
    http_version: bytes
        method: Union[bytes, str],
        headers: Union[Headers, List[Tuple[bytes, bytes]], List[Tuple[str, str]]],
        target: Union[bytes, str],
        http_version: Union[bytes, str] = b"1.1",
        _parsed: bool = False,
        if isinstance(headers, Headers):
            object.__setattr__(self, "headers", headers)
            object.__setattr__(
                self, "headers", normalize_and_validate(headers, _parsed=_parsed)
        if not _parsed:
            object.__setattr__(self, "method", bytesify(method))
            object.__setattr__(self, "target", bytesify(target))
            object.__setattr__(self, "http_version", bytesify(http_version))
            object.__setattr__(self, "method", method)
            object.__setattr__(self, "target", target)
            object.__setattr__(self, "http_version", http_version)
        # "A server MUST respond with a 400 (Bad Request) status code to any
        # HTTP/1.1 request message that lacks a Host header field and to any
        # request message that contains more than one Host header field or a
        # Host header field with an invalid field-value."
        # -- https://tools.ietf.org/html/rfc7230#section-5.4
        host_count = 0
        for name, value in self.headers:
            if name == b"host":
                host_count += 1
        if self.http_version == b"1.1" and host_count == 0:
            raise LocalProtocolError("Missing mandatory Host: header")
        if host_count > 1:
            raise LocalProtocolError("Found multiple Host: headers")
        validate(method_re, self.method, "Illegal method characters")
        validate(request_target_re, self.target, "Illegal target characters")
    # This is an unhashable type.
    __hash__ = None  # type: ignore
class _ResponseBase(Event):
    __slots__ = ("headers", "http_version", "reason", "status_code")
    reason: bytes
        status_code: int,
        reason: Union[bytes, str] = b"",
            object.__setattr__(self, "reason", bytesify(reason))
            if not isinstance(status_code, int):
                raise LocalProtocolError("status code must be integer")
            # Because IntEnum objects are instances of int, but aren't
            # duck-compatible (sigh), see gh-72.
            object.__setattr__(self, "status_code", int(status_code))
            object.__setattr__(self, "reason", reason)
            object.__setattr__(self, "status_code", status_code)
        self.__post_init__()
    def __post_init__(self) -> None:
class InformationalResponse(_ResponseBase):
    """An HTTP informational response.
    .. attribute:: status_code
       The status code of this response, as an integer. For an
       :class:`InformationalResponse`, this is always in the range [100,
       200).
       :ref:`the header normalization rules <headers-format>` for
    .. attribute:: reason
       The reason phrase of this response, as a byte string. For example:
       ``b"OK"``, or ``b"Not Found"``.
        if not (100 <= self.status_code < 200):
            raise LocalProtocolError(
                "InformationalResponse status_code should be in range "
                "[100, 200), not {}".format(self.status_code)
class Response(_ResponseBase):
    """The beginning of an HTTP response.
       :class:`Response`, this is always in the range [200,
       1000).
        if not (200 <= self.status_code < 1000):
                "Response status_code should be in range [200, 1000), not {}".format(
                    self.status_code
class Data(Event):
    """Part of an HTTP message body.
    .. attribute:: data
       A :term:`bytes-like object` containing part of a message body. Or, if
       using the ``combine=False`` argument to :meth:`Connection.send`, then
       any object that your socket writing code knows what to do with, and for
       which calling :func:`len` returns the number of bytes that will be
       written -- see :ref:`sendfile` for details.
    .. attribute:: chunk_start
       A marker that indicates whether this data object is from the start of a
       chunked transfer encoding chunk. This field is ignored when when a Data
       event is provided to :meth:`Connection.send`: it is only valid on
       events emitted from :meth:`Connection.next_event`. You probably
       shouldn't use this attribute at all; see
       :ref:`chunk-delimiters-are-bad` for details.
    .. attribute:: chunk_end
       A marker that indicates whether this data object is the last for a
       given chunked transfer encoding chunk. This field is ignored when when
       a Data event is provided to :meth:`Connection.send`: it is only valid
       on events emitted from :meth:`Connection.next_event`. You probably
    __slots__ = ("data", "chunk_start", "chunk_end")
    data: bytes
    chunk_start: bool
    chunk_end: bool
        self, data: bytes, chunk_start: bool = False, chunk_end: bool = False
        object.__setattr__(self, "data", data)
        object.__setattr__(self, "chunk_start", chunk_start)
        object.__setattr__(self, "chunk_end", chunk_end)
# XX FIXME: "A recipient MUST ignore (or consider as an error) any fields that
# are forbidden to be sent in a trailer, since processing them as if they were
# present in the header section might bypass external security filters."
# https://svn.tools.ietf.org/svn/wg/httpbis/specs/rfc7230.html#chunked.trailer.part
# Unfortunately, the list of forbidden fields is long and vague :-/
class EndOfMessage(Event):
    """The end of an HTTP message.
       Default value: ``[]``
       Any trailing headers attached to this message, represented as a list of
       (name, value) pairs. See :ref:`the header normalization rules
       <headers-format>` for details.
       Must be empty unless ``Transfer-Encoding: chunked`` is in use.
    __slots__ = ("headers",)
        headers: Union[
            Headers, List[Tuple[bytes, bytes]], List[Tuple[str, str]], None
            headers = Headers([])
        elif not isinstance(headers, Headers):
            headers = normalize_and_validate(headers, _parsed=_parsed)
class ConnectionClosed(Event):
    """This event indicates that the sender has closed their outgoing
    Note that this does not necessarily mean that they can't *receive* further
    data, because TCP connections are composed to two one-way channels which
    can be closed independently. See :ref:`closing` for details.
    No fields.
