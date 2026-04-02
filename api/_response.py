from typing_extensions import Awaitable, ParamSpec, override, get_origin
from ._utils import is_given, extract_type_arg, is_annotated_type, is_type_alias_type, extract_type_var_from_base
from ._constants import RAW_RESPONSE_HEADER, OVERRIDE_CAST_TO_HEADER
from ._exceptions import OpenAIError, APIResponseValidationError
_APIResponseT = TypeVar("_APIResponseT", bound="APIResponse[Any]")
_AsyncAPIResponseT = TypeVar("_AsyncAPIResponseT", bound="AsyncAPIResponse[Any]")
class BaseAPIResponse(Generic[R]):
    _is_sse_stream: bool
        self._is_sse_stream = stream
        """Returns the httpx Request instance associated with the current response."""
        """Returns the URL for which the request was made."""
        """Whether or not the response body has been closed.
        If this is False then there is response data that has not been read yet.
        You must either fully consume the response body or call `.close()`
        before discarding the response to prevent resource leaks.
            f"<{self.__class__.__name__} [{self.status_code} {self.http_response.reason_phrase}] type={self._cast_to}>"
        if self._is_sse_stream:
        if cast_to == bytes:
            return cast(R, response.content)
        # handle the legacy binary response case
        if inspect.isclass(cast_to) and cast_to.__name__ == "HttpxBinaryResponseContent":
        if origin == APIResponse:
        if inspect.isclass(origin) and issubclass(origin, httpx.Response):
class APIResponse(BaseAPIResponse[R]):
        if not self._is_sse_stream:
            self.read()
        """Read and return the binary response content."""
            return self.http_response.read()
        except httpx.StreamConsumed as exc:
            # The default error raised by httpx isn't very
            # helpful in our case so we re-raise it with
            # a different error message.
            raise StreamAlreadyConsumed() from exc
        """Read and decode the response content into a string."""
    def json(self) -> object:
        """Read and decode the JSON response content."""
        return self.http_response.json()
        """Close the response and release the connection.
        Automatically called if the response body is read to completion.
        self.http_response.close()
        A byte-iterator over the decoded response content.
        This automatically handles gzip, deflate and brotli encoded responses.
        for chunk in self.http_response.iter_bytes(chunk_size):
            yield chunk
        """A str-iterator over the decoded response content
        that handles both gzip, deflate, etc but also detects the content's
        string encoding.
        for chunk in self.http_response.iter_text(chunk_size):
        """Like `iter_text()` but will only yield chunks for each line"""
        for chunk in self.http_response.iter_lines():
class AsyncAPIResponse(BaseAPIResponse[R]):
    async def parse(self, *, to: type[_T]) -> _T: ...
    async def parse(self) -> R: ...
    async def parse(self, *, to: type[_T] | None = None) -> R | _T:
            await self.read()
    async def read(self) -> bytes:
            return await self.http_response.aread()
            # the default error raised by httpx isn't very
            # a different error message
    async def text(self) -> str:
    async def json(self) -> object:
        await self.http_response.aclose()
    async def iter_bytes(self, chunk_size: int | None = None) -> AsyncIterator[bytes]:
        async for chunk in self.http_response.aiter_bytes(chunk_size):
    async def iter_text(self, chunk_size: int | None = None) -> AsyncIterator[str]:
        async for chunk in self.http_response.aiter_text(chunk_size):
    async def iter_lines(self) -> AsyncIterator[str]:
        async for chunk in self.http_response.aiter_lines():
class BinaryAPIResponse(APIResponse[bytes]):
    """Subclass of APIResponse providing helpers for dealing with binary data.
    Note: If you want to stream the response data instead of eagerly reading it
    the API request, e.g. `.with_streaming_response.get_binary_response()`
            for data in self.iter_bytes():
class AsyncBinaryAPIResponse(AsyncAPIResponse[bytes]):
    async def write_to_file(
            async for data in self.iter_bytes():
class StreamedBinaryAPIResponse(APIResponse[bytes]):
        """Streams the output to the given file.
            for data in self.iter_bytes(chunk_size):
class AsyncStreamedBinaryAPIResponse(AsyncAPIResponse[bytes]):
    async def stream_to_file(
            async for data in self.iter_bytes(chunk_size):
class StreamAlreadyConsumed(OpenAIError):
    Attempted to read or stream content, but the content has already
    been streamed.
    This can happen if you use a method like `.iter_lines()` and then attempt
    to read th entire response body afterwards, e.g.
    response = await client.post(...)
    async for line in response.iter_lines():
        ...  # do something with `line`
    content = await response.read()
    # ^ error
    If you want this behaviour you'll need to either manually accumulate the response
    content or call `await response.read()` before iterating over the stream.
        message = (
            "Attempted to read or stream some content, but the content has "
            "already been streamed. "
            "This could be due to attempting to stream the response "
            "content more than once."
            "\n\n"
            "You can fix this by manually accumulating the response content while streaming "
            "or by calling `.read()` before starting to stream."
class ResponseContextManager(Generic[_APIResponseT]):
    """Context manager for ensuring that a request is not made
    until it is entered and that the response will always be closed
    when the context manager exits
    def __init__(self, request_func: Callable[[], _APIResponseT]) -> None:
        self._request_func = request_func
        self.__response: _APIResponseT | None = None
    def __enter__(self) -> _APIResponseT:
        self.__response = self._request_func()
        return self.__response
        if self.__response is not None:
            self.__response.close()
class AsyncResponseContextManager(Generic[_AsyncAPIResponseT]):
    def __init__(self, api_request: Awaitable[_AsyncAPIResponseT]) -> None:
        self._api_request = api_request
        self.__response: _AsyncAPIResponseT | None = None
    async def __aenter__(self) -> _AsyncAPIResponseT:
        self.__response = await self._api_request
            await self.__response.close()
def to_streamed_response_wrapper(func: Callable[P, R]) -> Callable[P, ResponseContextManager[APIResponse[R]]]:
    to support streaming and returning the raw `APIResponse` object directly.
    def wrapped(*args: P.args, **kwargs: P.kwargs) -> ResponseContextManager[APIResponse[R]]:
        extra_headers[RAW_RESPONSE_HEADER] = "stream"
        make_request = functools.partial(func, *args, **kwargs)
        return ResponseContextManager(cast(Callable[[], APIResponse[R]], make_request))
def async_to_streamed_response_wrapper(
    func: Callable[P, Awaitable[R]],
) -> Callable[P, AsyncResponseContextManager[AsyncAPIResponse[R]]]:
    def wrapped(*args: P.args, **kwargs: P.kwargs) -> AsyncResponseContextManager[AsyncAPIResponse[R]]:
        make_request = func(*args, **kwargs)
        return AsyncResponseContextManager(cast(Awaitable[AsyncAPIResponse[R]], make_request))
def to_custom_streamed_response_wrapper(
    func: Callable[P, object],
    response_cls: type[_APIResponseT],
) -> Callable[P, ResponseContextManager[_APIResponseT]]:
    """Higher order function that takes one of our bound API methods and an `APIResponse` class
    and wraps the method to support streaming and returning the given response class directly.
    Note: the given `response_cls` *must* be concrete, e.g. `class BinaryAPIResponse(APIResponse[bytes])`
    def wrapped(*args: P.args, **kwargs: P.kwargs) -> ResponseContextManager[_APIResponseT]:
        extra_headers: dict[str, Any] = {**(cast(Any, kwargs.get("extra_headers")) or {})}
        extra_headers[OVERRIDE_CAST_TO_HEADER] = response_cls
        return ResponseContextManager(cast(Callable[[], _APIResponseT], make_request))
def async_to_custom_streamed_response_wrapper(
    func: Callable[P, Awaitable[object]],
    response_cls: type[_AsyncAPIResponseT],
) -> Callable[P, AsyncResponseContextManager[_AsyncAPIResponseT]]:
    def wrapped(*args: P.args, **kwargs: P.kwargs) -> AsyncResponseContextManager[_AsyncAPIResponseT]:
        return AsyncResponseContextManager(cast(Awaitable[_AsyncAPIResponseT], make_request))
def to_raw_response_wrapper(func: Callable[P, R]) -> Callable[P, APIResponse[R]]:
    def wrapped(*args: P.args, **kwargs: P.kwargs) -> APIResponse[R]:
        extra_headers[RAW_RESPONSE_HEADER] = "raw"
        return cast(APIResponse[R], func(*args, **kwargs))
def async_to_raw_response_wrapper(func: Callable[P, Awaitable[R]]) -> Callable[P, Awaitable[AsyncAPIResponse[R]]]:
    async def wrapped(*args: P.args, **kwargs: P.kwargs) -> AsyncAPIResponse[R]:
        return cast(AsyncAPIResponse[R], await func(*args, **kwargs))
def to_custom_raw_response_wrapper(
) -> Callable[P, _APIResponseT]:
    and wraps the method to support returning the given response class directly.
    def wrapped(*args: P.args, **kwargs: P.kwargs) -> _APIResponseT:
        return cast(_APIResponseT, func(*args, **kwargs))
def async_to_custom_raw_response_wrapper(
) -> Callable[P, Awaitable[_AsyncAPIResponseT]]:
    def wrapped(*args: P.args, **kwargs: P.kwargs) -> Awaitable[_AsyncAPIResponseT]:
        return cast(Awaitable[_AsyncAPIResponseT], func(*args, **kwargs))
def extract_response_type(typ: type[BaseAPIResponse[Any]]) -> type:
    """Given a type like `APIResponse[T]`, returns the generic type variable `T`.
    This also handles the case where a concrete subclass is given, e.g.
    class MyResponse(APIResponse[bytes]):
    extract_response_type(MyResponse) -> bytes
    return extract_type_var_from_base(
        typ,
        generic_bases=cast("tuple[type, ...]", (BaseAPIResponse, APIResponse, AsyncAPIResponse)),
        index=0,
