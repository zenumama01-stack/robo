from typing import TYPE_CHECKING, Any, Optional, cast
from typing_extensions import Literal
from ._utils import is_dict
from ._models import construct_type
    from .types.chat import ChatCompletion
class OpenAIError(Exception):
class APIError(OpenAIError):
    message: str
    request: httpx.Request
    body: object | None
    """The API response body.
    If the API responded with a valid JSON structure then this property will be the
    decoded result.
    If it isn't a valid JSON structure then this will be the raw response.
    If there was no response associated with this error then it will be `None`.
    code: Optional[str] = None
    param: Optional[str] = None
    type: Optional[str]
    def __init__(self, message: str, request: httpx.Request, *, body: object | None) -> None:
        self.request = request
        self.message = message
        self.body = body
        if is_dict(body):
            self.code = cast(Any, construct_type(type_=Optional[str], value=body.get("code")))
            self.param = cast(Any, construct_type(type_=Optional[str], value=body.get("param")))
            self.type = cast(Any, construct_type(type_=str, value=body.get("type")))
            self.code = None
            self.param = None
            self.type = None
class APIResponseValidationError(APIError):
    response: httpx.Response
    status_code: int
    def __init__(self, response: httpx.Response, body: object | None, *, message: str | None = None) -> None:
        super().__init__(message or "Data returned by API invalid for expected schema.", response.request, body=body)
        self.status_code = response.status_code
class APIStatusError(APIError):
    """Raised when an API response has a status code of 4xx or 5xx."""
    request_id: str | None
    def __init__(self, message: str, *, response: httpx.Response, body: object | None) -> None:
        super().__init__(message, response.request, body=body)
        self.request_id = response.headers.get("x-request-id")
class APIConnectionError(APIError):
    def __init__(self, *, message: str = "Connection error.", request: httpx.Request) -> None:
        super().__init__(message, request, body=None)
class APITimeoutError(APIConnectionError):
    def __init__(self, request: httpx.Request) -> None:
        super().__init__(message="Request timed out.", request=request)
class BadRequestError(APIStatusError):
    status_code: Literal[400] = 400  # pyright: ignore[reportIncompatibleVariableOverride]
class AuthenticationError(APIStatusError):
    status_code: Literal[401] = 401  # pyright: ignore[reportIncompatibleVariableOverride]
class PermissionDeniedError(APIStatusError):
    status_code: Literal[403] = 403  # pyright: ignore[reportIncompatibleVariableOverride]
class NotFoundError(APIStatusError):
    status_code: Literal[404] = 404  # pyright: ignore[reportIncompatibleVariableOverride]
class ConflictError(APIStatusError):
    status_code: Literal[409] = 409  # pyright: ignore[reportIncompatibleVariableOverride]
class UnprocessableEntityError(APIStatusError):
    status_code: Literal[422] = 422  # pyright: ignore[reportIncompatibleVariableOverride]
class RateLimitError(APIStatusError):
    status_code: Literal[429] = 429  # pyright: ignore[reportIncompatibleVariableOverride]
class InternalServerError(APIStatusError):
class LengthFinishReasonError(OpenAIError):
    completion: ChatCompletion
    """The completion that caused this error.
    Note: this will *not* be a complete `ChatCompletion` object when streaming as `usage`
          will not be included.
    def __init__(self, *, completion: ChatCompletion) -> None:
        msg = "Could not parse response content as the length limit was reached"
        if completion.usage:
            msg += f" - {completion.usage}"
        super().__init__(msg)
        self.completion = completion
class ContentFilterFinishReasonError(OpenAIError):
            f"Could not parse response content as the request was rejected by the content filter",
class InvalidWebhookSignatureError(ValueError):
    """Raised when a webhook signature is invalid, meaning the computed signature does not match the expected signature."""
Various richly-typed exceptions, that also help us deal with string formatting
in python where it's easier.
By putting the formatting in `__str__`, we also avoid paying the cost for
users who silence the exceptions.
from .._utils import set_module
def _unpack_tuple(tup):
    if len(tup) == 1:
        return tup[0]
        return tup
def _display_as_base(cls):
    A decorator that makes an exception class look like its base.
    We use this to hide subclasses that are implementation details - the user
    should catch the base type, which is what the traceback will show them.
    Classes decorated with this decorator are subject to removal without a
    deprecation warning.
    assert issubclass(cls, Exception)
    cls.__name__ = cls.__base__.__name__
class UFuncTypeError(TypeError):
    """ Base class for all ufunc exceptions """
    def __init__(self, ufunc):
        self.ufunc = ufunc
@_display_as_base
class _UFuncNoLoopError(UFuncTypeError):
    """ Thrown when a ufunc loop cannot be found """
    def __init__(self, ufunc, dtypes):
        super().__init__(ufunc)
        self.dtypes = tuple(dtypes)
            "ufunc {!r} did not contain a loop with signature matching types "
            "{!r} -> {!r}"
        ).format(
            self.ufunc.__name__,
            _unpack_tuple(self.dtypes[:self.ufunc.nin]),
            _unpack_tuple(self.dtypes[self.ufunc.nin:])
class _UFuncBinaryResolutionError(_UFuncNoLoopError):
    """ Thrown when a binary resolution fails """
        super().__init__(ufunc, dtypes)
        assert len(self.dtypes) == 2
            "ufunc {!r} cannot use operands with types {!r} and {!r}"
            self.ufunc.__name__, *self.dtypes
class _UFuncCastingError(UFuncTypeError):
    def __init__(self, ufunc, casting, from_, to):
        self.casting = casting
        self.from_ = from_
        self.to = to
class _UFuncInputCastingError(_UFuncCastingError):
    """ Thrown when a ufunc input cannot be casted """
    def __init__(self, ufunc, casting, from_, to, i):
        super().__init__(ufunc, casting, from_, to)
        self.in_i = i
        # only show the number if more than one input exists
        i_str = "{} ".format(self.in_i) if self.ufunc.nin != 1 else ""
            "Cannot cast ufunc {!r} input {}from {!r} to {!r} with casting "
            "rule {!r}"
            self.ufunc.__name__, i_str, self.from_, self.to, self.casting
class _UFuncOutputCastingError(_UFuncCastingError):
    """ Thrown when a ufunc output cannot be casted """
        self.out_i = i
        # only show the number if more than one output exists
        i_str = "{} ".format(self.out_i) if self.ufunc.nout != 1 else ""
            "Cannot cast ufunc {!r} output {}from {!r} to {!r} with casting "
class _ArrayMemoryError(MemoryError):
    """ Thrown when an array cannot be allocated"""
    def __init__(self, shape, dtype):
        self.shape = shape
    def _total_size(self):
        num_bytes = self.dtype.itemsize
        for dim in self.shape:
            num_bytes *= dim
        return num_bytes
    def _size_to_string(num_bytes):
        """ Convert a number of bytes into a binary size string """
        # https://en.wikipedia.org/wiki/Binary_prefix
        LOG2_STEP = 10
        STEP = 1024
        units = ['bytes', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB']
        unit_i = max(num_bytes.bit_length() - 1, 1) // LOG2_STEP
        unit_val = 1 << (unit_i * LOG2_STEP)
        n_units = num_bytes / unit_val
        del unit_val
        # ensure we pick a unit that is correct after rounding
        if round(n_units) == STEP:
            unit_i += 1
            n_units /= STEP
        # deal with sizes so large that we don't have units for them
        if unit_i >= len(units):
            new_unit_i = len(units) - 1
            n_units *= 1 << ((unit_i - new_unit_i) * LOG2_STEP)
            unit_i = new_unit_i
        unit_name = units[unit_i]
        # format with a sensible number of digits
        if unit_i == 0:
            # no decimal point on bytes
            return '{:.0f} {}'.format(n_units, unit_name)
        elif round(n_units) < 1000:
            # 3 significant figures, if none are dropped to the left of the .
            return '{:#.3g} {}'.format(n_units, unit_name)
            # just give all the digits otherwise
            return '{:#.0f} {}'.format(n_units, unit_name)
        size_str = self._size_to_string(self._total_size)
            "Unable to allocate {} for an array with shape {} and data type {}"
            .format(size_str, self.shape, self.dtype)
ExceptionMapping = typing.Mapping[typing.Type[Exception], typing.Type[Exception]]
def map_exceptions(map: ExceptionMapping) -> typing.Iterator[None]:
    except Exception as exc:  # noqa: PIE786
        for from_exc, to_exc in map.items():
            if isinstance(exc, from_exc):
                raise to_exc(exc) from exc
        raise  # pragma: nocover
class ConnectionNotAvailable(Exception):
class ProxyError(Exception):
class UnsupportedProtocol(Exception):
class ProtocolError(Exception):
class RemoteProtocolError(ProtocolError):
class LocalProtocolError(ProtocolError):
# Timeout errors
class TimeoutException(Exception):
class PoolTimeout(TimeoutException):
class ConnectTimeout(TimeoutException):
class ReadTimeout(TimeoutException):
class WriteTimeout(TimeoutException):
# Network errors
class NetworkError(Exception):
class ConnectError(NetworkError):
class ReadError(NetworkError):
class WriteError(NetworkError):
    from exceptiongroup import BaseExceptionGroup
class BrokenResourceError(Exception):
    Raised when trying to use a resource that has been rendered unusable due to external
    causes (e.g. a send stream whose peer has disconnected).
class BrokenWorkerProcess(Exception):
    Raised by :meth:`~anyio.to_process.run_sync` if the worker process terminates abruptly or
    otherwise misbehaves.
class BrokenWorkerInterpreter(Exception):
    Raised by :meth:`~anyio.to_interpreter.run_sync` if an unexpected exception is
    raised in the subinterpreter.
    def __init__(self, excinfo: Any):
        # This was adapted from concurrent.futures.interpreter.ExecutionFailed
        msg = excinfo.formatted
            if excinfo.type and excinfo.msg:
                msg = f"{excinfo.type.__name__}: {excinfo.msg}"
                msg = excinfo.type.__name__ or excinfo.msg
        self.excinfo = excinfo
            formatted = self.excinfo.errdisplay
            return dedent(
                {super().__str__()}
                Uncaught in the interpreter:
                {formatted}
                """.strip()
class BusyResourceError(Exception):
    Raised when two tasks are trying to read from or write to the same resource
    concurrently.
    def __init__(self, action: str):
        super().__init__(f"Another task is already {action} this resource")
class ClosedResourceError(Exception):
    """Raised when trying to use a resource that has been closed."""
class ConnectionFailed(OSError):
    Raised when a connection attempt fails.
    .. note:: This class inherits from :exc:`OSError` for backwards compatibility.
def iterate_exceptions(
    exception: BaseException,
) -> Generator[BaseException, None, None]:
    if isinstance(exception, BaseExceptionGroup):
        for exc in exception.exceptions:
            yield from iterate_exceptions(exc)
        yield exception
class DelimiterNotFound(Exception):
    Raised during
    :meth:`~anyio.streams.buffered.BufferedByteReceiveStream.receive_until` if the
    maximum number of bytes has been read without the delimiter being found.
    def __init__(self, max_bytes: int) -> None:
            f"The delimiter was not found among the first {max_bytes} bytes"
class EndOfStream(Exception):
    Raised when trying to read from a stream that has been closed from the other end.
class IncompleteRead(Exception):
    :meth:`~anyio.streams.buffered.BufferedByteReceiveStream.receive_exactly` or
    connection is closed before the requested amount of bytes has been read.
            "The stream was closed before the read operation could be completed"
class TypedAttributeLookupError(LookupError):
    Raised by :meth:`~anyio.TypedAttributeProvider.extra` when the given typed attribute
    is not found and no default value has been given.
class WouldBlock(Exception):
    """Raised by ``X_nowait`` functions if ``X()`` would block."""
class NoEventLoopError(RuntimeError):
    Raised by several functions that require an event loop to be running in the current
    thread when there is no running event loop.
    This is also raised by :func:`.from_thread.run` and :func:`.from_thread.run_sync`
    if not calling from an AnyIO worker thread, and no ``token`` was passed.
class RunFinishedError(RuntimeError):
    Raised by :func:`.from_thread.run` and :func:`.from_thread.run_sync` if the event
    loop associated with the explicitly passed token has already finished.
            "The event loop associated with the given token has already finished"
Our exception hierarchy:
* HTTPError
  x RequestError
    + TransportError
      - TimeoutException
        · ConnectTimeout
        · ReadTimeout
        · WriteTimeout
        · PoolTimeout
      - NetworkError
        · ConnectError
        · ReadError
        · WriteError
        · CloseError
      - ProtocolError
        · LocalProtocolError
        · RemoteProtocolError
      - ProxyError
      - UnsupportedProtocol
    + DecodingError
    + TooManyRedirects
  x HTTPStatusError
* InvalidURL
* CookieConflict
* StreamError
  x StreamConsumed
  x StreamClosed
  x ResponseNotRead
  x RequestNotRead
    from ._models import Request, Response  # pragma: no cover
    Base class for `RequestError` and `HTTPStatusError`.
    Useful for `try...except` blocks when issuing a request,
    and then calling `.raise_for_status()`.
        response = httpx.get("https://www.example.com")
    except httpx.HTTPError as exc:
        print(f"HTTP Exception for {exc.request.url} - {exc}")
        self._request: Request | None = None
    def request(self) -> Request:
        if self._request is None:
            raise RuntimeError("The .request property has not been set.")
        return self._request
    @request.setter
    def request(self, request: Request) -> None:
class RequestError(HTTPError):
    Base class for all exceptions that may occur when issuing a `.request()`.
    def __init__(self, message: str, *, request: Request | None = None) -> None:
        # At the point an exception is raised we won't typically have a request
        # instance to associate it with.
        # The 'request_context' context manager is used within the Client and
        # Response methods in order to ensure that any raised exceptions
        # have a `.request` property set on them.
    Base class for all exceptions that occur at the level of the Transport API.
# Timeout exceptions...
class TimeoutException(TransportError):
    The base class for timeout errors.
    An operation has timed out.
    Timed out while connecting to the host.
    Timed out while receiving data from the host.
    Timed out while sending data to the host.
    Timed out waiting to acquire a connection from the pool.
# Core networking exceptions...
class NetworkError(TransportError):
    The base class for network-related errors.
    An error occurred while interacting with the network.
    Failed to receive data from the network.
    Failed to send data through the network.
    Failed to establish a connection.
class CloseError(NetworkError):
    Failed to close a connection.
# Other transport exceptions...
    An error occurred while establishing a proxy connection.
class UnsupportedProtocol(TransportError):
    Attempted to make a request to an unsupported protocol.
    For example issuing a request to `ftp://www.example.com`.
class ProtocolError(TransportError):
    The protocol was violated.
    A protocol was violated by the client.
    For example if the user instantiated a `Request` instance explicitly,
    failed to include the mandatory `Host:` header, and then issued it directly
    using `client.send()`.
    The protocol was violated by the server.
    For example, returning malformed HTTP.
# Other request exceptions...
class DecodingError(RequestError):
    Decoding of the response failed, due to a malformed encoding.
class TooManyRedirects(RequestError):
    Too many redirects.
# Client errors
class HTTPStatusError(HTTPError):
    The response had an error HTTP status of 4xx or 5xx.
    May be raised when calling `response.raise_for_status()`
    def __init__(self, message: str, *, request: Request, response: Response) -> None:
class InvalidURL(Exception):
    URL is improperly formed or cannot be parsed.
class CookieConflict(Exception):
    Attempted to lookup a cookie by name, but multiple cookies existed.
    Can occur when calling `response.cookies.get(...)`.
# Stream exceptions...
# These may occur as the result of a programming error, by accessing
# the request/response stream in an invalid manner.
class StreamError(RuntimeError):
    The base class for stream exceptions.
    The developer made an error in accessing the request stream in
    an invalid way.
class StreamConsumed(StreamError):
            "already been streamed. For requests, this could be due to passing "
            "a generator as request content, and then receiving a redirect "
            "response or a secondary request as part of an authentication flow."
            "For responses, this could be due to attempting to stream the response "
class StreamClosed(StreamError):
    Attempted to read or stream response content, but the request has been
            "Attempted to read or stream content, but the stream has " "been closed."
class ResponseNotRead(StreamError):
    Attempted to access streaming response content, without having called `read()`.
            "Attempted to access streaming response content,"
            " without having called `read()`."
class RequestNotRead(StreamError):
    Attempted to access streaming request content, without having called `read()`.
            "Attempted to access streaming request content,"
def request_context(
    request: Request | None = None,
) -> typing.Iterator[None]:
    A context manager that can be used to attach the given request context
    to any `RequestError` exceptions that are raised within the block.
    except RequestError as exc:
            exc.request = request
