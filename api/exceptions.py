class InstaloaderException(Exception):
    """Base exception for this script.
    :note: This exception should not be raised directly."""
class QueryReturnedBadRequestException(InstaloaderException):
class QueryReturnedForbiddenException(InstaloaderException):
class ProfileNotExistsException(InstaloaderException):
class ProfileHasNoPicsException(InstaloaderException):
    .. deprecated:: 4.2.2
       Not raised anymore.
class PrivateProfileNotFollowedException(InstaloaderException):
class LoginRequiredException(InstaloaderException):
class LoginException(InstaloaderException):
class TwoFactorAuthRequiredException(LoginException):
class InvalidArgumentException(InstaloaderException):
class BadResponseException(InstaloaderException):
class BadCredentialsException(LoginException):
class ConnectionException(InstaloaderException):
class PostChangedException(InstaloaderException):
    """.. versionadded:: 4.2.2"""
class QueryReturnedNotFoundException(ConnectionException):
class TooManyRequestsException(ConnectionException):
class IPhoneSupportDisabledException(InstaloaderException):
class AbortDownloadException(Exception):
    Exception that is not catched in the error catchers inside the download loop and so aborts the
    download loop.
    This exception is not a subclass of ``InstaloaderException``.
class DropboxException(Exception):
    """All errors related to making an API request extend this."""
    def __init__(self, request_id, *args, **kwargs):
        # A request_id can be shared with Dropbox Support to pinpoint the exact
        # request that returns an error.
        super(DropboxException, self).__init__(request_id, *args, **kwargs)
        return repr(self)
class ApiError(DropboxException):
    """Errors produced by the Dropbox API."""
    def __init__(self, request_id, error, user_message_text, user_message_locale):
        :param (str) request_id: A request_id can be shared with Dropbox
            Support to pinpoint the exact request that returns an error.
        :param error: An instance of the error data type for the route.
        :param (str) user_message_text: A human-readable message that can be
            displayed to the end user. Is None, if unavailable.
        :param (str) user_message_locale: The locale of ``user_message_text``,
            if present.
        super(ApiError, self).__init__(request_id, error)
        self.error = error
        self.user_message_text = user_message_text
        self.user_message_locale = user_message_locale
        return 'ApiError({!r}, {})'.format(self.request_id, self.error)
class HttpError(DropboxException):
    """Errors produced at the HTTP layer."""
    def __init__(self, request_id, status_code, body):
        super(HttpError, self).__init__(request_id, status_code, body)
        self.status_code = status_code
        return 'HttpError({!r}, {}, {!r})'.format(self.request_id,
            self.status_code, self.body)
class PathRootError(HttpError):
    """Error caused by an invalid path root."""
    def __init__(self, request_id, error=None):
        super(PathRootError, self).__init__(request_id, 422, None)
        return 'PathRootError({!r}, {!r})'.format(self.request_id, self.error)
class BadInputError(HttpError):
    """Errors due to bad input parameters to an API Operation."""
    def __init__(self, request_id, message):
        super(BadInputError, self).__init__(request_id, 400, message)
        return 'BadInputError({!r}, {!r})'.format(self.request_id, self.message)
class AuthError(HttpError):
    """Errors due to invalid authentication credentials."""
    def __init__(self, request_id, error):
        super(AuthError, self).__init__(request_id, 401, None)
        return 'AuthError({!r}, {!r})'.format(self.request_id, self.error)
class RateLimitError(HttpError):
    """Error caused by rate limiting."""
    def __init__(self, request_id, error=None, backoff=None):
        super(RateLimitError, self).__init__(request_id, 429, None)
        self.backoff = backoff
        return 'RateLimitError({!r}, {!r}, {!r})'.format(
            self.request_id, self.error, self.backoff)
class InternalServerError(HttpError):
    """Errors due to a problem on Dropbox."""
        return 'InternalServerError({!r}, {}, {!r})'.format(
            self.request_id, self.status_code, self.body)
from ..utils import YoutubeDLError
    from .common import RequestHandler, Response
class RequestError(YoutubeDLError):
        msg: str | None = None,
        cause: Exception | str | None = None,
        handler: RequestHandler = None,
        self.cause = cause
        if not msg and cause:
            msg = str(cause)
class UnsupportedRequest(RequestError):
    """raised when a handler cannot handle a request"""
class NoSupportingHandlers(RequestError):
    """raised when no handlers can support a request for various reasons"""
    def __init__(self, unsupported_errors: list[UnsupportedRequest], unexpected_errors: list[Exception]):
        self.unsupported_errors = unsupported_errors or []
        self.unexpected_errors = unexpected_errors or []
        # Print a quick summary of the errors
        err_handler_map = {}
        for err in unsupported_errors:
            err_handler_map.setdefault(err.msg, []).append(err.handler.RH_NAME)
        reason_str = ', '.join([f'{msg} ({", ".join(handlers)})' for msg, handlers in err_handler_map.items()])
        if unexpected_errors:
            reason_str = ' + '.join(filter(None, [reason_str, f'{len(unexpected_errors)} unexpected error(s)']))
        err_str = 'Unable to handle request'
        if reason_str:
            err_str += f': {reason_str}'
        super().__init__(msg=err_str)
class TransportError(RequestError):
    """Network related errors"""
class HTTPError(RequestError):
    def __init__(self, response: Response, redirect_loop=False):
        self.status = response.status
        self.reason = response.reason
        self.redirect_loop = redirect_loop
        msg = f'HTTP Error {response.status}: {response.reason}'
        if redirect_loop:
            msg += ' (redirect loop detected)'
        super().__init__(msg=msg)
        return f'<HTTPError {self.status}: {self.reason}>'
class IncompleteRead(TransportError):
    def __init__(self, partial: int, expected: int | None = None, **kwargs):
        self.partial = partial
        self.expected = expected
        msg = f'{partial} bytes read'
        if expected is not None:
            msg += f', {expected} more expected'
        super().__init__(msg=msg, **kwargs)
        return f'<IncompleteRead: {self.msg}>'
class SSLError(TransportError):
class CertificateVerifyError(SSLError):
    """Raised when certificate validated has failed"""
class ProxyError(TransportError):
network_exceptions = (HTTPError, TransportError)
class ExecCommandFailed(SystemExit):
class HookError(Exception):
    Base class for hook related errors.
class ImportErrorWhenRunningHook(HookError):
            "ERROR: Failed to import module {0} required by hook for module {1}. Please check whether module {0} "
            "actually exists and whether the hook is compatible with your version of {1}: You might want to read more "
            "about hooks in the manual and provide a pull-request to improve PyInstaller.".format(
                self.args[0], self.args[1]
class RemovedCipherFeatureError(SystemExit):
    def __init__(self, message):
            f"ERROR: Bytecode encryption was removed in PyInstaller v6.0. {message}"
            " For the rationale and alternatives see https://github.com/pyinstaller/pyinstaller/pull/6999"
class RemovedExternalManifestError(SystemExit):
        super().__init__(f"ERROR: Support for external executable manifest was removed in PyInstaller v6.0. {message}")
class RemovedWinSideBySideSupportError(SystemExit):
            f"ERROR: Support for collecting and processing WinSxS assemblies was removed in PyInstaller v6.0. {message}"
class PythonLibraryNotFoundError(SystemExit):
        super().__init__(f"ERROR: {message}")
class InvalidSrcDestTupleError(SystemExit):
    def __init__(self, src_dest, message):
        super().__init__(f"ERROR: Invalid (SRC, DEST_DIR) tuple: {src_dest!r}. {message}")
class ImportlibMetadataError(SystemExit):
            "ERROR: PyInstaller requires importlib.metadata from python >= 3.10 stdlib or importlib_metadata from "
            "importlib-metadata >= 4.6"
from collections.abc import Mapping, Sequence
from typing import Annotated, Any, TypedDict
from pydantic import BaseModel, create_model
from starlette.exceptions import HTTPException as StarletteHTTPException
from starlette.exceptions import WebSocketException as StarletteWebSocketException
class EndpointContext(TypedDict, total=False):
    function: str
    line: int
class HTTPException(StarletteHTTPException):
    An HTTP exception you can raise in your own code to show errors to the client.
    This is for client errors, invalid authentication, invalid data, etc. Not for server
    errors in your code.
    [FastAPI docs for Handling Errors](https://fastapi.tiangolo.com/tutorial/handling-errors/).
    from fastapi import FastAPI, HTTPException
    items = {"foo": "The Foo Wrestlers"}
    @app.get("/items/{item_id}")
        if item_id not in items:
        return {"item": items[item_id]}
        status_code: Annotated[
            int,
                HTTP status code to send to the client.
                [FastAPI docs for Handling Errors](https://fastapi.tiangolo.com/tutorial/handling-errors/#use-httpexception)
        detail: Annotated[
                Any data to be sent to the client in the `detail` key of the JSON
        headers: Annotated[
            Mapping[str, str] | None,
                Any headers to send to the client in the response.
                [FastAPI docs for Handling Errors](https://fastapi.tiangolo.com/tutorial/handling-errors/#add-custom-headers)
        super().__init__(status_code=status_code, detail=detail, headers=headers)
class WebSocketException(StarletteWebSocketException):
    A WebSocket exception you can raise in your own code to show errors to the client.
    [FastAPI docs for WebSockets](https://fastapi.tiangolo.com/advanced/websockets/).
    from fastapi import (
        Cookie,
        FastAPI,
        WebSocket,
        WebSocketException,
    @app.websocket("/items/{item_id}/ws")
    async def websocket_endpoint(
        websocket: WebSocket,
        session: Annotated[str | None, Cookie()] = None,
        if session is None:
            raise WebSocketException(code=status.WS_1008_POLICY_VIOLATION)
        await websocket.accept()
            data = await websocket.receive_text()
            await websocket.send_text(f"Session cookie is: {session}")
            await websocket.send_text(f"Message text was: {data}, for item ID: {item_id}")
        code: Annotated[
                A closing code from the
                [valid codes defined in the specification](https://datatracker.ietf.org/doc/html/rfc6455#section-7.4.1).
        reason: Annotated[
            str | None,
                The reason to close the WebSocket connection.
                It is UTF-8-encoded data. The interpretation of the reason is up to the
                application, it is not specified by the WebSocket specification.
                It could contain text that could be human-readable or interpretable
                by the client code, etc.
        super().__init__(code=code, reason=reason)
RequestErrorModel: type[BaseModel] = create_model("Request")
WebSocketErrorModel: type[BaseModel] = create_model("WebSocket")
class FastAPIError(RuntimeError):
    A generic, FastAPI-specific error.
class DependencyScopeError(FastAPIError):
    A dependency declared that it depends on another dependency with an invalid
    (narrower) scope.
class ValidationException(Exception):
        errors: Sequence[Any],
        endpoint_ctx: EndpointContext | None = None,
        self._errors = errors
        self.endpoint_ctx = endpoint_ctx
        ctx = endpoint_ctx or {}
        self.endpoint_function = ctx.get("function")
        self.endpoint_path = ctx.get("path")
        self.endpoint_file = ctx.get("file")
        self.endpoint_line = ctx.get("line")
    def errors(self) -> Sequence[Any]:
        return self._errors
    def _format_endpoint_context(self) -> str:
        if not (self.endpoint_file and self.endpoint_line and self.endpoint_function):
            if self.endpoint_path:
                return f"\n  Endpoint: {self.endpoint_path}"
        context = f'\n  File "{self.endpoint_file}", line {self.endpoint_line}, in {self.endpoint_function}'
            context += f"\n    {self.endpoint_path}"
        message = f"{len(self._errors)} validation error{'s' if len(self._errors) != 1 else ''}:\n"
        for err in self._errors:
            message += f"  {err}\n"
        message += self._format_endpoint_context()
        return message.rstrip()
class RequestValidationError(ValidationException):
        body: Any = None,
        super().__init__(errors, endpoint_ctx=endpoint_ctx)
class WebSocketRequestValidationError(ValidationException):
class ResponseValidationError(ValidationException):
class PydanticV1NotSupportedError(FastAPIError):
    A pydantic.v1 model is used, which is no longer supported.
class FastAPIDeprecationWarning(UserWarning):
    A custom deprecation warning as DeprecationWarning is ignored
    Ref: https://sethmlarson.dev/deprecations-via-warnings-dont-work-for-python-libraries
from django.core.exceptions import SuspiciousOperation
class DisallowedModelAdminLookup(SuspiciousOperation):
    """Invalid filter was passed to admin view via URL querystring"""
class DisallowedModelAdminToField(SuspiciousOperation):
    """Invalid to_field was passed to admin view via URL query string"""
class AlreadyRegistered(Exception):
    """The model is already registered."""
class NotRegistered(Exception):
    """The model is not registered."""
from django.core.exceptions import BadRequest, SuspiciousOperation
class InvalidSessionKey(SuspiciousOperation):
    """Invalid characters in session key"""
class SuspiciousSession(SuspiciousOperation):
    """The session may be tampered with"""
class SessionInterrupted(BadRequest):
    """The session was interrupted."""
Global Django exception classes.
class FieldDoesNotExist(Exception):
    """The requested model field does not exist"""
class AppRegistryNotReady(Exception):
    """The django.apps registry is not populated yet"""
class ObjectDoesNotExist(Exception):
    """The requested object does not exist"""
    silent_variable_failure = True
class ObjectNotUpdated(Exception):
    """The updated object no longer exists."""
class MultipleObjectsReturned(Exception):
    """The query returned multiple objects when only one was expected."""
class SuspiciousOperation(Exception):
    """The user did something suspicious"""
class SuspiciousMultipartForm(SuspiciousOperation):
    """Suspect MIME request in multipart form data"""
class SuspiciousFileOperation(SuspiciousOperation):
    """A Suspicious filesystem operation was attempted"""
class DisallowedHost(SuspiciousOperation):
    """HTTP_HOST header contains invalid value"""
class DisallowedRedirect(SuspiciousOperation):
    """Redirect was too long or scheme was not in allowed list."""
class TooManyFieldsSent(SuspiciousOperation):
    The number of fields in a GET or POST request exceeded
    settings.DATA_UPLOAD_MAX_NUMBER_FIELDS.
class TooManyFilesSent(SuspiciousOperation):
    settings.DATA_UPLOAD_MAX_NUMBER_FILES.
class RequestDataTooBig(SuspiciousOperation):
    The size of the request (excluding any file uploads) exceeded
    settings.DATA_UPLOAD_MAX_MEMORY_SIZE.
class RequestAborted(Exception):
    """The request was closed before it was completed, or timed out."""
class BadRequest(Exception):
    """The request is malformed and cannot be processed."""
class PermissionDenied(Exception):
    """The user did not have permission to do that"""
class ViewDoesNotExist(Exception):
    """The requested view does not exist"""
class MiddlewareNotUsed(Exception):
    """This middleware is not used in this server configuration"""
class ImproperlyConfigured(Exception):
    """Django is somehow improperly configured"""
class FieldError(Exception):
    """Some kind of problem with a model field."""
class FieldFetchBlocked(FieldError):
    """On-demand fetching of a model field blocked."""
NON_FIELD_ERRORS = "__all__"
class ValidationError(Exception):
    """An error while validating data."""
    def __init__(self, message, code=None, params=None):
        The `message` argument can be a single error, a list of errors, or a
        dictionary that maps field names to lists of errors. What we define as
        an "error" can be either a simple string or an instance of
        ValidationError with its message attribute set, and what we define as
        list or dictionary can be an actual `list` or `dict` or an instance
        of ValidationError with its `error_list` or `error_dict` attribute set.
        super().__init__(message, code, params)
        if isinstance(message, ValidationError):
            if hasattr(message, "error_dict"):
                message = message.error_dict
            elif not hasattr(message, "message"):
                message = message.error_list
                message, code, params = message.message, message.code, message.params
        if isinstance(message, dict):
            self.error_dict = {}
            for field, messages in message.items():
                if not isinstance(messages, ValidationError):
                    messages = ValidationError(messages)
                self.error_dict[field] = messages.error_list
        elif isinstance(message, list):
            self.error_list = []
            for message in message:
                # Normalize plain strings to instances of ValidationError.
                if not isinstance(message, ValidationError):
                    message = ValidationError(message)
                    self.error_list.extend(sum(message.error_dict.values(), []))
                    self.error_list.extend(message.error_list)
            self.error_list = [self]
    def message_dict(self):
        # Trigger an AttributeError if this ValidationError
        # doesn't have an error_dict.
        getattr(self, "error_dict")
        return dict(self)
    def messages(self):
        if hasattr(self, "error_dict"):
            return sum(dict(self).values(), [])
        return list(self)
    def update_error_dict(self, error_dict):
            for field, error_list in self.error_dict.items():
                error_dict.setdefault(field, []).extend(error_list)
            error_dict.setdefault(NON_FIELD_ERRORS, []).extend(self.error_list)
        return error_dict
            for field, errors in self.error_dict.items():
                yield field, list(ValidationError(errors))
            for error in self.error_list:
                message = error.message
                if error.params:
                    message %= error.params
                yield str(message)
            return repr(dict(self))
        return repr(list(self))
        return "ValidationError(%s)" % self
        if not isinstance(other, ValidationError):
        return hash(self) == hash(other)
        if hasattr(self, "message"):
            return hash(
                    self.message,
                    self.code,
                    make_hashable(self.params),
            return hash(make_hashable(self.error_dict))
        return hash(tuple(sorted(self.error_list, key=operator.attrgetter("message"))))
class EmptyResultSet(Exception):
    """A database query predicate is impossible."""
class FullResultSet(Exception):
    """A database query predicate is matches everything."""
class SynchronousOnlyOperation(Exception):
    """The user tried to call a sync-only function from an async context."""
class AmbiguityError(Exception):
    """More than one migration matches a name prefix."""
class BadMigrationError(Exception):
    """There's a bad migration (unreadable/bad format/etc.)."""
class CircularDependencyError(Exception):
    """There's an impossible-to-resolve circular dependency."""
class InconsistentMigrationHistory(Exception):
    """An applied migration has some of its dependencies not applied."""
class InvalidBasesError(ValueError):
    """A model's base classes can't be resolved."""
class IrreversibleError(RuntimeError):
    """An irreversible migration is about to be reversed."""
class NodeNotFoundError(LookupError):
    """An attempt on a node is made that is not available in the graph."""
    def __init__(self, message, node, origin=None):
        self.node = node
        return self.message
        return "NodeNotFoundError(%r)" % (self.node,)
class MigrationSchemaMissing(DatabaseError):
class InvalidMigrationPlan(ValueError):
class TaskException(Exception):
    """Base class for task-related exceptions. Do not raise directly."""
class InvalidTask(TaskException):
    """The provided Task is invalid."""
class InvalidTaskBackend(ImproperlyConfigured):
    """The provided Task backend is invalid."""
class TaskResultDoesNotExist(TaskException):
    """The requested TaskResult does not exist."""
class TaskResultMismatch(TaskException):
    """The requested TaskResult is invalid."""
This module contains generic exceptions used by template backends. Although,
due to historical reasons, the Django template language also internally uses
these exceptions, other exceptions specific to the DTL should not be added
here.
class TemplateDoesNotExist(Exception):
    The exception used when a template does not exist. Optional arguments:
    backend
        The template backend class used when raising this exception.
    tried
        A list of sources that were tried when finding the template. This
        is formatted as a list of tuples containing (origin, status), where
        origin is an Origin object or duck type and status is a string with the
        reason the template wasn't found.
    chain
        A list of intermediate TemplateDoesNotExist exceptions. This is used to
        encapsulate multiple exceptions when loading templates from multiple
        engines.
    def __init__(self, msg, tried=None, backend=None, chain=None):
        self.backend = backend
        if tried is None:
            tried = []
        self.tried = tried
        if chain is None:
            chain = []
        self.chain = chain
class TemplateSyntaxError(Exception):
    The exception used for syntax errors during parsing or rendering.
from django.http import Http404
class Resolver404(Http404):
class NoReverseMatch(Exception):
"""Custom **exceptions** for LangChain."""
class LangChainException(Exception):  # noqa: N818
    """General LangChain exception."""
class TracerException(LangChainException):
    """Base class for exceptions in tracers module."""
class OutputParserException(ValueError, LangChainException):  # noqa: N818
    """Exception that output parsers should raise to signify a parsing error.
    This exists to differentiate parsing errors from other code or execution errors
    that also may arise inside the output parser.
    `OutputParserException` will be available to catch and handle in ways to fix the
    parsing error, while other errors will be raised.
        error: Any,
        observation: str | None = None,
        llm_output: str | None = None,
        send_to_llm: bool = False,  # noqa: FBT001,FBT002
        """Create an `OutputParserException`.
            error: The error that's being re-raised or an error message.
            observation: String explanation of error which can be passed to a model to
                try and remediate the issue.
            llm_output: String model output which is error-ing.
            send_to_llm: Whether to send the observation and llm_output back to an Agent
                after an `OutputParserException` has been raised.
                This gives the underlying model driving the agent the context that the
                previous output was improperly structured, in the hopes that it will
                update the output to the correct format.
            ValueError: If `send_to_llm` is `True` but either observation or
                `llm_output` are not provided.
            error = create_message(
                message=error, error_code=ErrorCode.OUTPUT_PARSING_FAILURE
        super().__init__(error)
        if send_to_llm and (observation is None or llm_output is None):
                "Arguments 'observation' & 'llm_output'"
                " are required if 'send_to_llm' is True"
        self.observation = observation
        self.llm_output = llm_output
        self.send_to_llm = send_to_llm
class ContextOverflowError(LangChainException):
    """Exception raised when input exceeds the model's context limit.
    This exception is raised by chat models when the input tokens exceed
    the maximum context window supported by the model.
class ErrorCode(Enum):
    """Error codes."""
    INVALID_PROMPT_INPUT = "INVALID_PROMPT_INPUT"
    INVALID_TOOL_RESULTS = "INVALID_TOOL_RESULTS"  # Used in JS; not Py (yet)
    MESSAGE_COERCION_FAILURE = "MESSAGE_COERCION_FAILURE"
    MODEL_AUTHENTICATION = "MODEL_AUTHENTICATION"  # Used in JS; not Py (yet)
    MODEL_NOT_FOUND = "MODEL_NOT_FOUND"  # Used in JS; not Py (yet)
    MODEL_RATE_LIMIT = "MODEL_RATE_LIMIT"  # Used in JS; not Py (yet)
    OUTPUT_PARSING_FAILURE = "OUTPUT_PARSING_FAILURE"
def create_message(*, message: str, error_code: ErrorCode) -> str:
    """Create a message with a link to the LangChain troubleshooting guide.
        message: The message to display.
        error_code: The error code to display.
        The full message with the troubleshooting link.
        create_message(
            message="Failed to parse output",
            error_code=ErrorCode.OUTPUT_PARSING_FAILURE,
        "Failed to parse output. For troubleshooting, visit: ..."
        f"{message}\n"
        "For troubleshooting, visit: https://docs.langchain.com/oss/python/langchain"
        f"/errors/{error_code.value} "
__all__ = ["LangChainException"]
from langchain_core.stores import InvalidKeyException
__all__ = ["InvalidKeyException"]
# exceptions.py
from .util import col, line, lineno, _collapse_string_to_ranges
from .unicode import pyparsing_unicode as ppu
class ExceptionWordUnicode(ppu.Latin1, ppu.LatinA, ppu.LatinB, ppu.Greek, ppu.Cyrillic):
_extract_alphanums = _collapse_string_to_ranges(ExceptionWordUnicode.alphanums)
_exception_word_extractor = re.compile("([" + _extract_alphanums + "]{1,16})|.")
class ParseBaseException(Exception):
    """base exception class for all parsing runtime exceptions"""
    # Performance tuning: we construct a *lot* of these, so keep this
    # constructor as small and fast as possible
        pstr: str,
        loc: int = 0,
        msg: Optional[str] = None,
        elem=None,
        self.loc = loc
            self.msg = pstr
            self.pstr = ""
            self.pstr = pstr
        self.parser_element = self.parserElement = elem
        self.args = (pstr, loc, msg)
    def explain_exception(exc, depth=16):
        Method to take an exception and translate the Python internal traceback into a list
        of the pyparsing expressions that caused the exception to be raised.
        - exc - exception raised during parsing (need not be a ParseException, in support
          of Python exceptions that might be raised in a parse action)
        - depth (default=16) - number of levels back in the stack trace to list expression
          and function names; if None, the full stack trace names will be listed; if 0, only
          the failing input line, marker, and exception string will be shown
        Returns a multi-line string listing the ParserElements and/or function names in the
        exception's stack trace.
        from .core import ParserElement
        if depth is None:
            depth = sys.getrecursionlimit()
        if isinstance(exc, ParseBaseException):
            ret.append(exc.line)
            ret.append(" " * (exc.column - 1) + "^")
        ret.append("{}: {}".format(type(exc).__name__, exc))
        if depth > 0:
            callers = inspect.getinnerframes(exc.__traceback__, context=depth)
            for i, ff in enumerate(callers[-depth:]):
                frm = ff[0]
                f_self = frm.f_locals.get("self", None)
                if isinstance(f_self, ParserElement):
                    if frm.f_code.co_name not in ("parseImpl", "_parseNoCache"):
                    if id(f_self) in seen:
                    seen.add(id(f_self))
                    self_type = type(f_self)
                    ret.append(
                        "{}.{} - {}".format(
                            self_type.__module__, self_type.__name__, f_self
                elif f_self is not None:
                    ret.append("{}.{}".format(self_type.__module__, self_type.__name__))
                    code = frm.f_code
                    if code.co_name in ("wrapper", "<module>"):
                    ret.append("{}".format(code.co_name))
                depth -= 1
                if not depth:
        return "\n".join(ret)
    def _from_exception(cls, pe):
        internal factory method to simplify creating one type of ParseException
        from another - avoids having __init__ signature conflicts among subclasses
        return cls(pe.pstr, pe.loc, pe.msg, pe.parserElement)
    def line(self) -> str:
        Return the line of text where the exception occurred.
        return line(self.loc, self.pstr)
    def lineno(self) -> int:
        Return the 1-based line number of text where the exception occurred.
        return lineno(self.loc, self.pstr)
    def col(self) -> int:
        Return the 1-based column on the line of text where the exception occurred.
        return col(self.loc, self.pstr)
    def column(self) -> int:
        if self.pstr:
            if self.loc >= len(self.pstr):
                foundstr = ", found end of text"
                # pull out next word at error location
                found_match = _exception_word_extractor.match(self.pstr, self.loc)
                if found_match is not None:
                    found = found_match.group(0)
                    found = self.pstr[self.loc : self.loc + 1]
                foundstr = (", found %r" % found).replace(r"\\", "\\")
            foundstr = ""
        return "{}{}  (at char {}), (line:{}, col:{})".format(
            self.msg, foundstr, self.loc, self.lineno, self.column
    def mark_input_line(self, marker_string: str = None, *, markerString=">!<") -> str:
        Extracts the exception line from the input string, and marks
        the location of the exception with a special symbol.
        markerString = marker_string if marker_string is not None else markerString
        line_str = self.line
        line_column = self.column - 1
        if markerString:
            line_str = "".join(
                (line_str[:line_column], markerString, line_str[line_column:])
        return line_str.strip()
    def explain(self, depth=16) -> str:
        Method to translate the Python internal traceback into a list
            expr = pp.Word(pp.nums) * 3
                expr.parse_string("123 456 A789")
            except pp.ParseException as pe:
                print(pe.explain(depth=0))
            123 456 A789
                    ^
            ParseException: Expected W:(0-9), found 'A'  (at char 8), (line:1, col:9)
        Note: the diagnostic output will include string representations of the expressions
        that failed to parse. These representations will be more helpful if you use `set_name` to
        give identifiable names to your expressions. Otherwise they will use the default string
        forms, which may be cryptic to read.
        Note: pyparsing's default truncation of exception tracebacks may also truncate the
        stack of expressions that are displayed in the ``explain`` output. To get the full listing
        of parser expressions, you may have to set ``ParserElement.verbose_stacktrace = True``
        return self.explain_exception(self, depth)
    markInputline = mark_input_line
class ParseException(ParseBaseException):
    Exception thrown when a parse expression doesn't match the input string
            Word(nums).set_name("integer").parse_string("ABC")
        except ParseException as pe:
            print(pe)
            print("column: {}".format(pe.column))
       Expected integer (at char 0), (line:1, col:1)
        column: 1
class ParseFatalException(ParseBaseException):
    User-throwable exception thrown when inconsistent parse content
    is found; stops all parsing immediately
class ParseSyntaxException(ParseFatalException):
    Just like :class:`ParseFatalException`, but thrown internally
    when an :class:`ErrorStop<And._ErrorStop>` ('-' operator) indicates
    that parsing is to stop immediately because an unbacktrackable
    syntax error has been found.
class RecursiveGrammarException(Exception):
    Exception thrown by :class:`ParserElement.validate` if the
    grammar could be left-recursive; parser may need to enable
    left recursion using :class:`ParserElement.enable_left_recursion<ParserElement.enable_left_recursion>`
    def __init__(self, parseElementList):
        self.parseElementTrace = parseElementList
        return "RecursiveGrammarException: {}".format(self.parseElementTrace)
"""Exceptions used throughout package.
This module MUST NOT try to import from anything within `pip._internal` to
operate. This is expected to be importable from any/all files within the
subpackage and, thus, should not depend on them.
from itertools import chain, groupby, repeat
from typing import TYPE_CHECKING, Literal
from pip._vendor.packaging.requirements import InvalidRequirement
from pip._vendor.packaging.version import InvalidVersion
from pip._vendor.rich.console import Console, ConsoleOptions, RenderResult
from pip._vendor.rich.markup import escape
from pip._vendor.rich.text import Text
    from hashlib import _Hash
    from pip._vendor.requests.models import PreparedRequest, Request, Response
    from pip._internal.metadata import BaseDistribution
    from pip._internal.network.download import _FileDownload
    from pip._internal.req.req_install import InstallRequirement
# Scaffolding
def _is_kebab_case(s: str) -> bool:
    return re.match(r"^[a-z]+(-[a-z]+)*$", s) is not None
def _prefix_with_indent(
    s: Text | str,
    console: Console,
    prefix: str,
    indent: str,
) -> Text:
    if isinstance(s, Text):
        text = s
        text = console.render_str(s)
    return console.render_str(prefix, overflow="ignore") + console.render_str(
        f"\n{indent}", overflow="ignore"
    ).join(text.split(allow_blank=True))
class PipError(Exception):
    """The base pip error."""
class DiagnosticPipError(PipError):
    """An error, that presents diagnostic information to the user.
    This contains a bunch of logic, to enable pretty presentation of our error
    messages. Each error gets a unique reference. Each error can also include
    additional context, a hint and/or a note -- which are presented with the
    main error message in a consistent style.
    This is adapted from the error output styling in `sphinx-theme-builder`.
        kind: Literal["error", "warning"] = "error",
        message: str | Text,
        context: str | Text | None,
        hint_stmt: str | Text | None,
        note_stmt: str | Text | None = None,
        link: str | None = None,
        # Ensure a proper reference is provided.
        if reference is None:
            assert hasattr(self, "reference"), "error reference not provided!"
            reference = self.reference
        assert _is_kebab_case(reference), "error reference must be kebab-case!"
        self.kind = kind
        self.reference = reference
        self.note_stmt = note_stmt
        self.hint_stmt = hint_stmt
        super().__init__(f"<{self.__class__.__name__}: {self.reference}>")
            f"<{self.__class__.__name__}("
            f"reference={self.reference!r}, "
            f"message={self.message!r}, "
            f"context={self.context!r}, "
            f"note_stmt={self.note_stmt!r}, "
            f"hint_stmt={self.hint_stmt!r}"
            ")>"
    def __rich_console__(
        options: ConsoleOptions,
    ) -> RenderResult:
        colour = "red" if self.kind == "error" else "yellow"
        yield f"[{colour} bold]{self.kind}[/]: [bold]{self.reference}[/]"
        if not options.ascii_only:
            # Present the main message, with relevant context indented.
            if self.context is not None:
                yield _prefix_with_indent(
                    prefix=f"[{colour}]×[/] ",
                    indent=f"[{colour}]│[/] ",
                    prefix=f"[{colour}]╰─>[/] ",
                    indent=f"[{colour}]   [/] ",
                    prefix="[red]×[/] ",
                    indent="  ",
            yield self.message
                yield self.context
        if self.note_stmt is not None or self.hint_stmt is not None:
        if self.note_stmt is not None:
                self.note_stmt,
                prefix="[magenta bold]note[/]: ",
                indent="      ",
        if self.hint_stmt is not None:
                self.hint_stmt,
                prefix="[cyan bold]hint[/]: ",
        if self.link is not None:
            yield f"Link: {self.link}"
# Actual Errors
class ConfigurationError(PipError):
    """General exception in configuration"""
class InstallationError(PipError):
    """General exception during installation"""
class FailedToPrepareCandidate(InstallationError):
    """Raised when we fail to prepare a candidate (i.e. fetch and generate metadata).
    This is intentionally not a diagnostic error, since the output will be presented
    above this error, when this occurs. This should instead present information to the
        self, *, package_name: str, requirement_chain: str, failed_step: str
        super().__init__(f"Failed to build '{package_name}' when {failed_step.lower()}")
        self.package_name = package_name
        self.requirement_chain = requirement_chain
        self.failed_step = failed_step
class MissingPyProjectBuildRequires(DiagnosticPipError):
    """Raised when pyproject.toml has `build-system`, but no `build-system.requires`."""
    reference = "missing-pyproject-build-system-requires"
    def __init__(self, *, package: str) -> None:
            message=f"Can not process {escape(package)}",
            context=Text(
                "This package has an invalid pyproject.toml file.\n"
                "The [build-system] table is missing the mandatory `requires` key."
            note_stmt="This is an issue with the package mentioned above, not pip.",
            hint_stmt=Text("See PEP 518 for the detailed specification."),
class InvalidPyProjectBuildRequires(DiagnosticPipError):
    """Raised when pyproject.toml an invalid `build-system.requires`."""
    reference = "invalid-pyproject-build-system-requires"
    def __init__(self, *, package: str, reason: str) -> None:
                "This package has an invalid `build-system.requires` key in "
                f"pyproject.toml.\n{reason}"
class NoneMetadataError(PipError):
    """Raised when accessing a Distribution's "METADATA" or "PKG-INFO".
    This signifies an inconsistency, when the Distribution claims to have
    the metadata file (if not, raise ``FileNotFoundError`` instead), but is
    not actually able to produce its content. This may be due to permission
    errors.
        dist: BaseDistribution,
        metadata_name: str,
        :param dist: A Distribution object.
        :param metadata_name: The name of the metadata being accessed
            (can be "METADATA" or "PKG-INFO").
        self.metadata_name = metadata_name
        # Use `dist` in the error message because its stringification
        # includes more information, like the version and location.
        return f"None {self.metadata_name} metadata found for distribution: {self.dist}"
class UserInstallationInvalid(InstallationError):
    """A --user install is requested on an environment without user site."""
        return "User base directory is not specified"
class InvalidSchemeCombination(InstallationError):
        before = ", ".join(str(a) for a in self.args[:-1])
        return f"Cannot set {before} and {self.args[-1]} together"
class DistributionNotFound(InstallationError):
    """Raised when a distribution cannot be found to satisfy a requirement"""
class RequirementsFileParseError(InstallationError):
    """Raised when a general error occurs parsing a requirements file line."""
class BestVersionAlreadyInstalled(PipError):
    """Raised when the most up-to-date version of a package is already
    installed."""
class BadCommand(PipError):
    """Raised when virtualenv or a command is not found"""
class CommandError(PipError):
    """Raised when there is an error in command-line arguments"""
class PreviousBuildDirError(PipError):
    """Raised when there's a previous conflicting build directory"""
class NetworkConnectionError(PipError):
    """HTTP connection error"""
        error_msg: str,
        request: Request | PreparedRequest | None = None,
        Initialize NetworkConnectionError with  `request` and `response`
        self.error_msg = error_msg
            self.response is not None
            and not self.request
            and hasattr(response, "request")
            self.request = self.response.request
        super().__init__(error_msg, response, request)
        return str(self.error_msg)
class InvalidWheelFilename(InstallationError):
    """Invalid wheel filename."""
class UnsupportedWheel(InstallationError):
    """Unsupported wheel."""
class InvalidWheel(InstallationError):
    """Invalid (e.g. corrupt) wheel."""
    def __init__(self, location: str, name: str):
        return f"Wheel '{self.name}' located at {self.location} is invalid."
class MetadataInconsistent(InstallationError):
    """Built metadata contains inconsistent information.
    This is raised when the metadata contains values (e.g. name and version)
    that do not match the information previously obtained from sdist filename,
    user-supplied ``#egg=`` value, or an install requirement name.
        self, ireq: InstallRequirement, field: str, f_val: str, m_val: str
        self.ireq = ireq
        self.f_val = f_val
        self.m_val = m_val
            f"Requested {self.ireq} has inconsistent {self.field}: "
            f"expected {self.f_val!r}, but metadata has {self.m_val!r}"
class MetadataInvalid(InstallationError):
    """Metadata is invalid."""
    def __init__(self, ireq: InstallRequirement, error: str) -> None:
        return f"Requested {self.ireq} has invalid metadata: {self.error}"
class InstallationSubprocessError(DiagnosticPipError, InstallationError):
    """A subprocess call failed."""
    reference = "subprocess-exited-with-error"
        command_description: str,
        exit_code: int,
        output_lines: list[str] | None,
        if output_lines is None:
            output_prompt = Text("No available output.")
            output_prompt = (
                Text.from_markup(f"[red][{len(output_lines)} lines of output][/]\n")
                + Text("".join(output_lines))
                + Text.from_markup(R"[red]\[end of output][/]")
                f"[green]{escape(command_description)}[/] did not run successfully.\n"
                f"exit code: {exit_code}"
            context=output_prompt,
            hint_stmt=None,
            note_stmt=(
                "This error originates from a subprocess, and is likely not a "
                "problem with pip."
        self.command_description = command_description
        self.exit_code = exit_code
        return f"{self.command_description} exited with {self.exit_code}"
class MetadataGenerationFailed(DiagnosticPipError, InstallationError):
    reference = "metadata-generation-failed"
        package_details: str,
            message="Encountered error while generating package metadata.",
            context=escape(package_details),
            hint_stmt="See above for details.",
        return "metadata generation failed"
class HashErrors(InstallationError):
    """Multiple HashError instances rolled into one for reporting"""
        self.errors: list[HashError] = []
    def append(self, error: HashError) -> None:
        self.errors.append(error)
        self.errors.sort(key=lambda e: e.order)
        for cls, errors_of_cls in groupby(self.errors, lambda e: e.__class__):
            lines.append(cls.head)
            lines.extend(e.body() for e in errors_of_cls)
        if lines:
            return "\n".join(lines)
        return bool(self.errors)
class HashError(InstallationError):
    A failure to verify a package against known-good hashes
    :cvar order: An int sorting hash exception classes by difficulty of
        recovery (lower being harder), so the user doesn't bother fretting
        about unpinned packages when he has deeper issues, like VCS
        dependencies, to deal with. Also keeps error reports in a
        deterministic order.
    :cvar head: A section heading for display above potentially many
        exceptions of this kind
    :ivar req: The InstallRequirement that triggered this error. This is
        pasted on after the exception is instantiated, because it's not
        typically available earlier.
    req: InstallRequirement | None = None
    head = ""
    order: int = -1
    def body(self) -> str:
        """Return a summary of me for display under the heading.
        This default implementation simply prints a description of the
        triggering requirement.
        :param req: The InstallRequirement that provoked this error, with
            its link already populated by the resolver's _populate_link().
        return f"    {self._requirement_name()}"
        return f"{self.head}\n{self.body()}"
    def _requirement_name(self) -> str:
        """Return a description of the requirement that triggered me.
        This default implementation returns long description of the req, with
        line numbers
        return str(self.req) if self.req else "unknown package"
class VcsHashUnsupported(HashError):
    """A hash was provided for a version-control-system-based requirement, but
    we don't have a method for hashing those."""
    order = 0
    head = (
        "Can't verify hashes for these requirements because we don't "
        "have a way to hash version control repositories:"
class DirectoryUrlHashUnsupported(HashError):
    order = 1
        "Can't verify hashes for these file:// requirements because they "
        "point to directories:"
class HashMissing(HashError):
    """A hash was needed for a requirement but is absent."""
    order = 2
        "Hashes are required in --require-hashes mode, but they are "
        "missing from some requirements. Here is a list of those "
        "requirements along with the hashes their downloaded archives "
        "actually had. Add lines like these to your requirements files to "
        "prevent tampering. (If you did not enable --require-hashes "
        "manually, note that it turns on automatically when any package "
        "has a hash.)"
    def __init__(self, gotten_hash: str) -> None:
        :param gotten_hash: The hash of the (possibly malicious) archive we
            just downloaded
        self.gotten_hash = gotten_hash
        # Dodge circular import.
        from pip._internal.utils.hashes import FAVORITE_HASH
        package = None
        if self.req:
            # In the case of URL-based requirements, display the original URL
            # seen in the requirements file rather than the package name,
            # so the output can be directly copied into the requirements file.
            package = (
                self.req.original_link
                if self.req.is_direct
                # In case someone feeds something downright stupid
                # to InstallRequirement's constructor.
                else getattr(self.req, "req", None)
        return "    {} --hash={}:{}".format(
            package or "unknown package", FAVORITE_HASH, self.gotten_hash
class HashUnpinned(HashError):
    """A requirement had a hash specified but was not pinned to a specific
    version."""
    order = 3
        "In --require-hashes mode, all requirements must have their "
        "versions pinned with ==. These do not:"
class HashMismatch(HashError):
    Distribution file hash values don't match.
    :ivar package_name: The name of the package that triggered the hash
        mismatch. Feel free to write to this after the exception is raise to
        improve its error message.
    order = 4
        "THESE PACKAGES DO NOT MATCH THE HASHES FROM THE REQUIREMENTS "
        "FILE. If you have updated the package versions, please update "
        "the hashes. Otherwise, examine the package contents carefully; "
        "someone may have tampered with them."
    def __init__(self, allowed: dict[str, list[str]], gots: dict[str, _Hash]) -> None:
        :param allowed: A dict of algorithm names pointing to lists of allowed
            hex digests
        :param gots: A dict of algorithm names pointing to hashes we
            actually got from the files under suspicion
        self.gots = gots
        return f"    {self._requirement_name()}:\n{self._hash_comparison()}"
    def _hash_comparison(self) -> str:
        Return a comparison of actual and expected hash values.
               Expected sha256 abcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcde
                            or 123451234512345123451234512345123451234512345
                    Got        bcdefbcdefbcdefbcdefbcdefbcdefbcdefbcdefbcdef
        def hash_then_or(hash_name: str) -> chain[str]:
            # For now, all the decent hashes have 6-char names, so we can get
            # away with hard-coding space literals.
            return chain([hash_name], repeat("    or"))
        lines: list[str] = []
        for hash_name, expecteds in self.allowed.items():
            prefix = hash_then_or(hash_name)
            lines.extend((f"        Expected {next(prefix)} {e}") for e in expecteds)
            lines.append(
                f"             Got        {self.gots[hash_name].hexdigest()}\n"
class UnsupportedPythonVersion(InstallationError):
    """Unsupported python version according to Requires-Python package
    metadata."""
class ConfigurationFileCouldNotBeLoaded(ConfigurationError):
    """When there are errors while loading a configuration file"""
        reason: str = "could not be loaded",
        fname: str | None = None,
        error: configparser.Error | None = None,
        self.fname = fname
        if self.fname is not None:
            message_part = f" in {self.fname}."
            assert self.error is not None
            message_part = f".\n{self.error}\n"
        return f"Configuration file {self.reason}{message_part}"
_DEFAULT_EXTERNALLY_MANAGED_ERROR = f"""\
The Python environment under {sys.prefix} is managed externally, and may not be
manipulated by the user. Please use specific tooling from the distributor of
the Python installation to interact with this environment instead.
class ExternallyManagedEnvironment(DiagnosticPipError):
    """The current environment is externally managed.
    This is raised when the current environment is externally managed, as
    defined by `PEP 668`_. The ``EXTERNALLY-MANAGED`` configuration is checked
    and displayed when the error is bubbled up to the user.
    :param error: The error message read from ``EXTERNALLY-MANAGED``.
    reference = "externally-managed-environment"
    def __init__(self, error: str | None) -> None:
            context = Text(_DEFAULT_EXTERNALLY_MANAGED_ERROR)
            context = Text(error)
            message="This environment is externally managed",
                "If you believe this is a mistake, please contact your "
                "Python installation or OS distribution provider. "
                "You can override this, at the risk of breaking your Python "
                "installation or OS, by passing --break-system-packages."
            hint_stmt=Text("See PEP 668 for the detailed specification."),
    def _iter_externally_managed_error_keys() -> Iterator[str]:
        # LC_MESSAGES is in POSIX, but not the C standard. The most common
        # platform that does not implement this category is Windows, where
        # using other categories for console message localization is equally
        # unreliable, so we fall back to the locale-less vendor message. This
        # can always be re-evaluated when a vendor proposes a new alternative.
            category = locale.LC_MESSAGES
            lang: str | None = None
            lang, _ = locale.getlocale(category)
        if lang is not None:
            yield f"Error-{lang}"
            for sep in ("-", "_"):
                before, found, _ = lang.partition(sep)
                yield f"Error-{before}"
        yield "Error"
    def from_config(
        config: pathlib.Path | str,
    ) -> ExternallyManagedEnvironment:
        parser = configparser.ConfigParser(interpolation=None)
            parser.read(config, encoding="utf-8")
            section = parser["externally-managed"]
            for key in cls._iter_externally_managed_error_keys():
                with contextlib.suppress(KeyError):
                    return cls(section[key])
        except (OSError, UnicodeDecodeError, configparser.ParsingError):
            from pip._internal.utils._log import VERBOSE
            exc_info = logger.isEnabledFor(VERBOSE)
            logger.warning("Failed to read %s", config, exc_info=exc_info)
        return cls(None)
class UninstallMissingRecord(DiagnosticPipError):
    reference = "uninstall-no-record-file"
    def __init__(self, *, distribution: BaseDistribution) -> None:
        installer = distribution.installer
        if not installer or installer == "pip":
            dep = f"{distribution.raw_name}=={distribution.version}"
            hint = Text.assemble(
                "You might be able to recover from this via: ",
                (f"pip install --force-reinstall --no-deps {dep}", "green"),
            hint = Text(
                f"The package was installed by {installer}. "
                "You should check if it can uninstall the package."
            message=Text(f"Cannot uninstall {distribution}"),
            context=(
                "The package's contents are unknown: "
                f"no RECORD file was found for {distribution.raw_name}."
            hint_stmt=hint,
class LegacyDistutilsInstall(DiagnosticPipError):
    reference = "uninstall-distutils-installed-package"
                "It is a distutils installed project and thus we cannot accurately "
                "determine which files belong to it which would lead to only a partial "
                "uninstall."
class InvalidInstalledPackage(DiagnosticPipError):
    reference = "invalid-installed-package"
        invalid_exc: InvalidRequirement | InvalidVersion,
        installed_location = dist.installed_location
        if isinstance(invalid_exc, InvalidRequirement):
            invalid_type = "requirement"
            invalid_type = "version"
            message=Text(
                f"Cannot process installed package {dist} "
                + (f"in {installed_location!r} " if installed_location else "")
                + f"because it has an invalid {invalid_type}:\n{invalid_exc.args[0]}"
                "Starting with pip 24.1, packages with invalid "
                f"{invalid_type}s can not be processed."
            hint_stmt="To proceed this package must be uninstalled.",
class IncompleteDownloadError(DiagnosticPipError):
    """Raised when the downloader receives fewer bytes than advertised
    in the Content-Length header."""
    reference = "incomplete-download"
    def __init__(self, download: _FileDownload) -> None:
        from pip._internal.utils.misc import format_size
        assert download.size is not None
        download_status = (
            f"{format_size(download.bytes_received)}/{format_size(download.size)}"
        if download.reattempts:
            retry_status = f"after {download.reattempts + 1} attempts "
            hint = "Use --resume-retries to configure resume attempt limit."
            # Download retrying is not enabled.
            retry_status = ""
            hint = "Consider using --resume-retries to enable download resumption."
        message = Text(
            f"Download failed {retry_status}because not enough bytes "
            f"were received ({download_status})"
            context=f"URL: {download.link.redacted_url}",
            note_stmt="This is an issue with network connectivity, not pip.",
class ResolutionTooDeepError(DiagnosticPipError):
    """Raised when the dependency resolver exceeds the maximum recursion depth."""
    reference = "resolution-too-deep"
            message="Dependency resolution exceeded maximum depth",
                "Pip cannot resolve the current dependencies as the dependency graph "
                "is too complex for pip to solve efficiently."
            hint_stmt=(
                "Try adding lower bounds to constrain your dependencies, "
                "for example: 'package>=2.0.0' instead of just 'package'. "
            link="https://pip.pypa.io/en/stable/topics/dependency-resolution/#handling-resolution-too-deep-errors",
class InstallWheelBuildError(DiagnosticPipError):
    reference = "failed-wheel-build-for-install"
    def __init__(self, failed: list[InstallRequirement]) -> None:
                "Failed to build installable wheels for some "
                "pyproject.toml based projects"
            context=", ".join(r.name for r in failed),  # type: ignore
class InvalidEggFragment(DiagnosticPipError):
    reference = "invalid-egg-fragment"
    def __init__(self, link: Link, fragment: str) -> None:
        hint = ""
        if ">" in fragment or "=" in fragment or "<" in fragment:
            hint = (
                "Version specifiers are silently ignored for URL references. "
                "Remove them. "
        if "[" in fragment and "]" in fragment:
            hint += "Try using the Direct URL requirement syntax: 'name[extra] @ URL'"
        if not hint:
            hint = "Egg fragments can only be a valid project name."
            message=f"The '{escape(fragment)}' egg fragment is invalid",
            context=f"from '{escape(str(link))}'",
            hint_stmt=escape(hint),
class BuildDependencyInstallError(DiagnosticPipError):
    """Raised when build dependencies cannot be installed."""
    reference = "failed-build-dependency-install"
        req: InstallRequirement | None,
        build_reqs: Iterable[str],
        cause: Exception,
        log_lines: list[str] | None,
        if isinstance(cause, PipError):
            note = "This is likely not a problem with pip."
            note = (
                "pip crashed unexpectedly. Please file an issue on pip's issue "
                "tracker: https://github.com/pypa/pip/issues/new"
        if log_lines is None:
            # No logs are available, they must have been printed earlier.
            context = Text("See above for more details.")
                log_lines.append(f"ERROR: {cause}")
                # Split rendered error into real lines without trailing newlines.
                log_lines.extend(
                    "".join(traceback.format_exception(cause)).splitlines()
            context = Text.assemble(
                f"Installing {' '.join(build_reqs)}\n",
                (f"[{len(log_lines)} lines of output]\n", "red"),
                "\n".join(log_lines),
                ("\n[end of output]", "red"),
        message = Text("Cannot install build dependencies", "green")
        if req:
            message += Text(f" for {req}")
            message=message, context=context, hint_stmt=None, note_stmt=note
class UnpackException(Exception):
    """Base class for some exceptions raised while unpacking.
    NOTE: unpack may raise exception other than subclass of
    UnpackException.  If you want to catch all error, catch
    Exception instead.
class BufferFull(UnpackException):
class OutOfData(UnpackException):
class FormatError(ValueError, UnpackException):
    """Invalid msgpack format"""
class StackError(ValueError, UnpackException):
    """Too nested"""
# Deprecated.  Use ValueError instead
UnpackValueError = ValueError
class ExtraData(UnpackValueError):
    """ExtraData is raised when there is trailing data.
    This exception is raised while only one-shot (not streaming)
    unpack.
    def __init__(self, unpacked, extra):
        self.unpacked = unpacked
        return "unpack(b) received extra data."
# Deprecated.  Use Exception instead to catch all exception during packing.
PackException = Exception
PackValueError = ValueError
PackOverflowError = OverflowError
requests.exceptions
This module contains the set of Requests' exceptions.
from pip._vendor.urllib3.exceptions import HTTPError as BaseHTTPError
from .compat import JSONDecodeError as CompatJSONDecodeError
class RequestException(IOError):
    """There was an ambiguous exception that occurred while handling your
        """Initialize RequestException with `request` and `response` objects."""
        response = kwargs.pop("response", None)
        self.request = kwargs.pop("request", None)
        if response is not None and not self.request and hasattr(response, "request"):
class InvalidJSONError(RequestException):
    """A JSON error occurred."""
class JSONDecodeError(InvalidJSONError, CompatJSONDecodeError):
    """Couldn't decode the text into json"""
        Construct the JSONDecodeError instance first with all
        args. Then use it's args to construct the IOError so that
        the json specific args aren't used as IOError specific args
        and the error message from JSONDecodeError is preserved.
        CompatJSONDecodeError.__init__(self, *args)
        InvalidJSONError.__init__(self, *self.args, **kwargs)
        The __reduce__ method called when pickling the object must
        be the one from the JSONDecodeError (be it json/simplejson)
        as it expects all the arguments for instantiation, not just
        one like the IOError, and the MRO would by default call the
        __reduce__ method from the IOError due to the inheritance order.
        return CompatJSONDecodeError.__reduce__(self)
class HTTPError(RequestException):
    """An HTTP error occurred."""
class ConnectionError(RequestException):
    """A Connection error occurred."""
class ProxyError(ConnectionError):
    """A proxy error occurred."""
class SSLError(ConnectionError):
    """An SSL error occurred."""
class Timeout(RequestException):
    """The request timed out.
    Catching this error will catch both
    :exc:`~requests.exceptions.ConnectTimeout` and
    :exc:`~requests.exceptions.ReadTimeout` errors.
class ConnectTimeout(ConnectionError, Timeout):
    """The request timed out while trying to connect to the remote server.
    Requests that produced this error are safe to retry.
class ReadTimeout(Timeout):
    """The server did not send any data in the allotted amount of time."""
class URLRequired(RequestException):
    """A valid URL is required to make a request."""
class TooManyRedirects(RequestException):
    """Too many redirects."""
class MissingSchema(RequestException, ValueError):
    """The URL scheme (e.g. http or https) is missing."""
class InvalidSchema(RequestException, ValueError):
    """The URL scheme provided is either invalid or unsupported."""
class InvalidURL(RequestException, ValueError):
    """The URL provided was somehow invalid."""
class InvalidHeader(RequestException, ValueError):
    """The header value provided was somehow invalid."""
class InvalidProxyURL(InvalidURL):
    """The proxy URL provided is invalid."""
class ChunkedEncodingError(RequestException):
    """The server declared chunked encoding but sent an invalid chunk."""
class ContentDecodingError(RequestException, BaseHTTPError):
    """Failed to decode response content."""
class StreamConsumedError(RequestException, TypeError):
    """The content for this response was already consumed."""
class RetryError(RequestException):
    """Custom retries logic failed"""
class UnrewindableBodyError(RequestException):
    """Requests encountered an error when trying to rewind a body."""
class RequestsWarning(Warning):
    """Base warning for Requests."""
class FileModeWarning(RequestsWarning, DeprecationWarning):
    """A file was opened in text mode, but Requests determined its binary length."""
class RequestsDependencyWarning(RequestsWarning):
    """An imported dependency doesn't match the expected version range."""
from typing import TYPE_CHECKING, Collection, Generic
from ..structs import CT, RT, RequirementInformation
class ResolverException(Exception):
    """A base class for all exceptions raised by this module.
    Exceptions derived by this class should all be handled in this module. Any
    bubbling pass the resolver should be treated as a bug.
class RequirementsConflicted(ResolverException, Generic[RT, CT]):
    def __init__(self, criterion: Criterion[RT, CT]) -> None:
        super().__init__(criterion)
        self.criterion = criterion
        return "Requirements conflict: {}".format(
            ", ".join(repr(r) for r in self.criterion.iter_requirement()),
class InconsistentCandidate(ResolverException, Generic[RT, CT]):
    def __init__(self, candidate: CT, criterion: Criterion[RT, CT]):
        super().__init__(candidate, criterion)
        self.candidate = candidate
        return "Provided candidate {!r} does not satisfy {}".format(
            self.candidate,
class ResolutionError(ResolverException):
class ResolutionImpossible(ResolutionError, Generic[RT, CT]):
    def __init__(self, causes: Collection[RequirementInformation[RT, CT]]):
        super().__init__(causes)
        # causes is a list of RequirementInformation objects
        self.causes = causes
class ResolutionTooDeep(ResolutionError):
    def __init__(self, round_count: int) -> None:
        super().__init__(round_count)
        self.round_count = round_count
from .packages.six.moves.http_client import IncompleteRead as httplib_IncompleteRead
# Base Exceptions
class HTTPError(Exception):
    """Base exception used by this module."""
class HTTPWarning(Warning):
    """Base warning used by this module."""
class PoolError(HTTPError):
    """Base exception for errors caused within a pool."""
    def __init__(self, pool, message):
        self.pool = pool
        HTTPError.__init__(self, "%s: %s" % (pool, message))
        # For pickling purposes.
        return self.__class__, (None, None)
class RequestError(PoolError):
    """Base exception for PoolErrors that have associated URLs."""
    def __init__(self, pool, url, message):
        PoolError.__init__(self, pool, message)
        return self.__class__, (None, self.url, None)
class SSLError(HTTPError):
    """Raised when SSL certificate fails in an HTTPS connection."""
class ProxyError(HTTPError):
    """Raised when the connection to a proxy fails."""
    def __init__(self, message, error, *args):
        super(ProxyError, self).__init__(message, error, *args)
        self.original_error = error
class DecodeError(HTTPError):
    """Raised when automatic decoding based on Content-Type fails."""
class ProtocolError(HTTPError):
    """Raised when something unexpected happens mid-request/response."""
#: Renamed to ProtocolError but aliased for backwards compatibility.
ConnectionError = ProtocolError
# Leaf Exceptions
class MaxRetryError(RequestError):
    """Raised when the maximum number of retries is exceeded.
    :param pool: The connection pool
    :type pool: :class:`~urllib3.connectionpool.HTTPConnectionPool`
    :param string url: The requested Url
    :param exceptions.Exception reason: The underlying error
    def __init__(self, pool, url, reason=None):
        message = "Max retries exceeded with url: %s (Caused by %r)" % (url, reason)
        RequestError.__init__(self, pool, url, message)
class HostChangedError(RequestError):
    """Raised when an existing pool gets a request for a foreign host."""
    def __init__(self, pool, url, retries=3):
        message = "Tried to open a foreign host with url: %s" % url
        self.retries = retries
class TimeoutStateError(HTTPError):
    """Raised when passing an invalid state to a timeout"""
class TimeoutError(HTTPError):
    """Raised when a socket timeout error occurs.
    Catching this error will catch both :exc:`ReadTimeoutErrors
    <ReadTimeoutError>` and :exc:`ConnectTimeoutErrors <ConnectTimeoutError>`.
class ReadTimeoutError(TimeoutError, RequestError):
    """Raised when a socket timeout occurs while receiving data from a server"""
# This timeout error does not have a URL attached and needs to inherit from the
# base HTTPError
class ConnectTimeoutError(TimeoutError):
    """Raised when a socket timeout occurs while connecting to a server"""
class NewConnectionError(ConnectTimeoutError, PoolError):
    """Raised when we fail to establish a new connection. Usually ECONNREFUSED."""
class EmptyPoolError(PoolError):
    """Raised when a pool runs out of connections and no more are allowed."""
class ClosedPoolError(PoolError):
    """Raised when a request enters a pool after the pool has been closed."""
class LocationValueError(ValueError, HTTPError):
    """Raised when there is something wrong with a given URL input."""
class LocationParseError(LocationValueError):
    """Raised when get_host or similar fails to parse the URL input."""
    def __init__(self, location):
        message = "Failed to parse: %s" % location
        HTTPError.__init__(self, message)
class URLSchemeUnknown(LocationValueError):
    """Raised when a URL input has an unsupported scheme."""
    def __init__(self, scheme):
        message = "Not supported URL scheme %s" % scheme
        super(URLSchemeUnknown, self).__init__(message)
        self.scheme = scheme
class ResponseError(HTTPError):
    """Used as a container for an error reason supplied in a MaxRetryError."""
    GENERIC_ERROR = "too many error responses"
    SPECIFIC_ERROR = "too many {status_code} error responses"
class SecurityWarning(HTTPWarning):
    """Warned when performing security reducing actions"""
class SubjectAltNameWarning(SecurityWarning):
    """Warned when connecting to a host with a certificate missing a SAN."""
class InsecureRequestWarning(SecurityWarning):
    """Warned when making an unverified HTTPS request."""
class SystemTimeWarning(SecurityWarning):
    """Warned when system time is suspected to be wrong"""
class InsecurePlatformWarning(SecurityWarning):
    """Warned when certain TLS/SSL configuration is not available on a platform."""
class SNIMissingWarning(HTTPWarning):
    """Warned when making a HTTPS request without SNI available."""
class DependencyWarning(HTTPWarning):
    Warned when an attempt is made to import a module with missing optional
    dependencies.
class ResponseNotChunked(ProtocolError, ValueError):
    """Response needs to be chunked in order to read it as chunks."""
class BodyNotHttplibCompatible(HTTPError):
    Body should be :class:`http.client.HTTPResponse` like
    (have an fp attribute which returns raw chunks) for read_chunked().
class IncompleteRead(HTTPError, httplib_IncompleteRead):
    Response length doesn't match expected Content-Length
    Subclass of :class:`http.client.IncompleteRead` to allow int value
    for ``partial`` to avoid creating large objects on streamed reads.
    def __init__(self, partial, expected):
        super(IncompleteRead, self).__init__(partial, expected)
        return "IncompleteRead(%i bytes read, %i more expected)" % (
            self.partial,
            self.expected,
class InvalidChunkLength(HTTPError, httplib_IncompleteRead):
    """Invalid chunk length in a chunked response."""
    def __init__(self, response, length):
        super(InvalidChunkLength, self).__init__(
            response.tell(), response.length_remaining
        return "InvalidChunkLength(got length %r, %i bytes read)" % (
            self.length,
class InvalidHeader(HTTPError):
    """The header provided was somehow invalid."""
class ProxySchemeUnknown(AssertionError, URLSchemeUnknown):
    """ProxyManager does not support the supplied scheme"""
    # TODO(t-8ch): Stop inheriting from AssertionError in v2.0.
        # 'localhost' is here because our URL parser parses
        # localhost:8080 -> scheme=localhost, remove if we fix this.
        if scheme == "localhost":
            scheme = None
            message = "Proxy URL had no scheme, should start with http:// or https://"
                "Proxy URL had unsupported scheme %s, should use http:// or https://"
                % scheme
        super(ProxySchemeUnknown, self).__init__(message)
class ProxySchemeUnsupported(ValueError):
    """Fetching HTTPS resources through HTTPS proxies is unsupported"""
class HeaderParsingError(HTTPError):
    """Raised by assert_header_parsing, but we convert it to a log.warning statement."""
    def __init__(self, defects, unparsed_data):
        message = "%s, unparsed data: %r" % (defects or "Unknown", unparsed_data)
        super(HeaderParsingError, self).__init__(message)
class UnrewindableBodyError(HTTPError):
    """urllib3 encountered an error when trying to rewind a body"""
class PydubException(Exception):
    Base class for any Pydub exception
class TooManyMissingFrames(PydubException):
class InvalidDuration(PydubException):
class InvalidTag(PydubException):
class InvalidID3TagVersion(PydubException):
class CouldntDecodeError(PydubException):
class CouldntEncodeError(PydubException):
class MissingAudioParameter(PydubException):
from email.errors import MessageDefect
from http.client import IncompleteRead as httplib_IncompleteRead
    from .connection import HTTPConnection
    from .connectionpool import ConnectionPool
_TYPE_REDUCE_RESULT = tuple[typing.Callable[..., object], tuple[object, ...]]
    def __init__(self, pool: ConnectionPool, message: str) -> None:
        self._message = message
        super().__init__(f"{pool}: {message}")
    def __reduce__(self) -> _TYPE_REDUCE_RESULT:
        return self.__class__, (None, self._message)
    def __init__(self, pool: ConnectionPool, url: str | None, message: str) -> None:
        super().__init__(pool, message)
        return self.__class__, (None, self.url, self._message)
    # The original error is also available as __cause__.
    original_error: Exception
    def __init__(self, message: str, error: Exception) -> None:
        super().__init__(message, error)
    :param str url: The requested Url
    :param reason: The underlying error
    :type reason: :class:`Exception`
        self, pool: ConnectionPool, url: str | None, reason: Exception | None = None
        message = f"Max retries exceeded with url: {url} (Caused by {reason!r})"
        super().__init__(pool, url, message)
        return self.__class__, (None, self.url, self.reason)
        self, pool: ConnectionPool, url: str, retries: Retry | int = 3
        message = f"Tried to open a foreign host with url: {url}"
class NewConnectionError(ConnectTimeoutError, HTTPError):
    def __init__(self, conn: HTTPConnection, message: str) -> None:
        self.conn = conn
        super().__init__(f"{conn}: {message}")
    def pool(self) -> HTTPConnection:
            "The 'pool' property is deprecated and will be removed "
            "in urllib3 v2.1.0. Use 'conn' instead.",
        return self.conn
class NameResolutionError(NewConnectionError):
    """Raised when host name resolution fails."""
    def __init__(self, host: str, conn: HTTPConnection, reason: socket.gaierror):
        message = f"Failed to resolve '{host}' ({reason})"
        self._host = host
        self._reason = reason
        super().__init__(conn, message)
        return self.__class__, (self._host, None, self._reason)
class FullPoolError(PoolError):
    """Raised when we try to add a connection to a full pool in blocking mode."""
        message = f"Failed to parse: {location}"
    def __init__(self, scheme: str):
        message = f"Not supported URL scheme {scheme}"
class NotOpenSSLWarning(SecurityWarning):
    """Warned when using unsupported SSL library"""
    partial: int  # type: ignore[assignment]
    expected: int
    def __init__(self, partial: int, expected: int) -> None:
    def __init__(self, response: HTTPResponse, length: bytes) -> None:
        self.partial: int = response.tell()  # type: ignore[assignment]
        self.expected: int | None = response.length_remaining
    def __init__(self, scheme: str | None) -> None:
            message = f"Proxy URL had unsupported scheme {scheme}, should use http:// or https://"
        self, defects: list[MessageDefect], unparsed_data: bytes | str | None
        message = f"{defects or 'Unknown'}, unparsed data: {unparsed_data!r}"
class SOCKSError(Exception):
    """Generic exception for when something goes wrong"""
class ProtocolError(SOCKSError):
"""Exception classes used by Pexpect"""
class ExceptionPexpect(Exception):
    '''Base class for all exceptions raised by this module.
    def __init__(self, value):
        super(ExceptionPexpect, self).__init__(value)
    def get_trace(self):
        '''This returns an abbreviated stack trace with lines that only concern
        the caller. In other words, the stack trace inside the Pexpect module
        is not included. '''
        tblist = traceback.extract_tb(sys.exc_info()[2])
        tblist = [item for item in tblist if ('pexpect/__init__' not in item[0])
                                           and ('pexpect/expect' not in item[0])]
        tblist = traceback.format_list(tblist)
        return ''.join(tblist)
class EOF(ExceptionPexpect):
    '''Raised when EOF is read from a child.
    This usually means the child has exited.'''
class TIMEOUT(ExceptionPexpect):
    '''Raised when a read time exceeds the timeout. '''
Exceptions and Warnings (:mod:`numpy.exceptions`)
=================================================
General exceptions used by NumPy.  Note that some exceptions may be module
specific, such as linear algebra errors.
.. versionadded:: NumPy 1.25
    The exceptions module is new in NumPy 1.25.  Older exceptions remain
    available through the main NumPy namespace for compatibility.
.. currentmodule:: numpy.exceptions
Warnings
.. autosummary::
   :toctree: generated/
   ComplexWarning             Given when converting complex to real.
   VisibleDeprecationWarning  Same as a DeprecationWarning, but more visible.
Exceptions
    AxisError          Given when an axis was invalid.
    DTypePromotionError   Given when no common dtype could be found.
    TooHardError       Error specific to `numpy.shares_memory`.
    "ComplexWarning", "VisibleDeprecationWarning", "ModuleDeprecationWarning",
    "TooHardError", "AxisError", "DTypePromotionError"]
# Disallow reloading this module so as to preserve the identities of the
# classes defined here.
if '_is_loaded' in globals():
    raise RuntimeError('Reloading numpy._globals is not allowed')
_is_loaded = True
class ComplexWarning(RuntimeWarning):
    The warning raised when casting a complex dtype to a real dtype.
    As implemented, casting a complex number to a real discards its imaginary
    part, but this behavior may not be what the user actually wants.
class ModuleDeprecationWarning(DeprecationWarning):
    """Module deprecation warning.
        This warning should not be used, since nose testing is not relevant
    The nose tester turns ordinary Deprecation warnings into test failures.
    That makes it hard to deprecate whole modules, because they get
    imported by default. So this is a special Deprecation warning that the
    nose tester will let pass without making tests fail.
class VisibleDeprecationWarning(UserWarning):
    """Visible deprecation warning.
    By default, python will not show deprecation warnings, so this class
    can be used when a very visible warning is helpful, for example because
    the usage is most likely a user bug.
# Exception used in shares_memory()
class TooHardError(RuntimeError):
    """max_work was exceeded.
    This is raised whenever the maximum number of candidate solutions
    to consider specified by the ``max_work`` parameter is exceeded.
    Assigning a finite number to max_work may have caused the operation
    to fail.
class AxisError(ValueError, IndexError):
    """Axis supplied was invalid.
    This is raised whenever an ``axis`` parameter is specified that is larger
    than the number of array dimensions.
    For compatibility with code written against older numpy versions, which
    raised a mixture of `ValueError` and `IndexError` for this situation, this
    exception subclasses both to ensure that ``except ValueError`` and
    ``except IndexError`` statements continue to catch `AxisError`.
    .. versionadded:: 1.13
    axis : int or str
        The out of bounds axis or a custom exception message.
        If an axis is provided, then `ndim` should be specified as well.
    ndim : int, optional
        The number of array dimensions.
    msg_prefix : str, optional
        A prefix for the exception message.
    axis : int, optional
        The out of bounds axis or ``None`` if a custom exception
        message was provided. This should be the axis as passed by
        the user, before any normalization to resolve negative indices.
        .. versionadded:: 1.22
        The number of array dimensions or ``None`` if a custom exception
        message was provided.
    >>> array_1d = np.arange(10)
    >>> np.cumsum(array_1d, axis=1)
    numpy.exceptions.AxisError: axis 1 is out of bounds for array of dimension 1
    Negative axes are preserved:
    >>> np.cumsum(array_1d, axis=-2)
    numpy.exceptions.AxisError: axis -2 is out of bounds for array of dimension 1
    The class constructor generally takes the axis and arrays'
    dimensionality as arguments:
    >>> print(np.AxisError(2, 1, msg_prefix='error'))
    error: axis 2 is out of bounds for array of dimension 1
    Alternatively, a custom exception message can be passed:
    >>> print(np.AxisError('Custom error message'))
    Custom error message
    __slots__ = ("axis", "ndim", "_msg")
    def __init__(self, axis, ndim=None, msg_prefix=None):
        if ndim is msg_prefix is None:
            # single-argument form: directly set the error message
            self._msg = axis
            self.axis = None
            self.ndim = None
            self._msg = msg_prefix
            self.axis = axis
            self.ndim = ndim
        axis = self.axis
        ndim = self.ndim
        if axis is ndim is None:
            return self._msg
            msg = f"axis {axis} is out of bounds for array of dimension {ndim}"
            if self._msg is not None:
                msg = f"{self._msg}: {msg}"
class DTypePromotionError(TypeError):
    """Multiple DTypes could not be converted to a common one.
    This exception derives from ``TypeError`` and is raised whenever dtypes
    cannot be converted to a single common one.  This can be because they
    are of a different category/class or incompatible instances of the same
    one (see Examples).
    Notes
    Many functions will use promotion to find the correct result and
    implementation.  For these functions the error will typically be chained
    with a more specific error indicating that no implementation was found
    for the input dtypes.
    Typically promotion should be considered "invalid" between the dtypes of
    two arrays when `arr1 == arr2` can safely return all ``False`` because the
    dtypes are fundamentally different.
    Datetimes and complex numbers are incompatible classes and cannot be
    promoted:
    >>> np.result_type(np.dtype("M8[s]"), np.complex128)
    DTypePromotionError: The DType <class 'numpy.dtype[datetime64]'> could not
    be promoted by <class 'numpy.dtype[complex128]'>. This means that no common
    DType exists for the given inputs. For example they cannot be stored in a
    single array unless the dtype is `object`. The full list of DTypes is:
    (<class 'numpy.dtype[datetime64]'>, <class 'numpy.dtype[complex128]'>)
    For example for structured dtypes, the structure can mismatch and the
    same ``DTypePromotionError`` is given when two structured dtypes with
    a mismatch in their number of fields is given:
    >>> dtype1 = np.dtype([("field1", np.float64), ("field2", np.int64)])
    >>> dtype2 = np.dtype([("field1", np.float64)])
    >>> np.promote_types(dtype1, dtype2)
    DTypePromotionError: field names `('field1', 'field2')` and `('field1',)`
    mismatch.
class MSLexError(ValueError):
    """Class for mslex errors"""
fsspec user-defined exception classes
class BlocksizeMismatchError(ValueError):
    Raised when a cached file is opened with a different blocksize than it was
    written with
class FSTimeoutError(asyncio.TimeoutError):
    Raised when a fsspec function timed out occurs
from typing import ClassVar
class FrozenError(AttributeError):
    A frozen/immutable instance or attribute have been attempted to be
    modified.
    It mirrors the behavior of ``namedtuples`` by using the same error message
    and subclassing `AttributeError`.
    .. versionadded:: 20.1.0
    msg = "can't set attribute"
    args: ClassVar[tuple[str]] = [msg]
class FrozenInstanceError(FrozenError):
    A frozen instance has been attempted to be modified.
    .. versionadded:: 16.1.0
class FrozenAttributeError(FrozenError):
    A frozen attribute has been attempted to be modified.
class AttrsAttributeNotFoundError(ValueError):
    An *attrs* function couldn't find an attribute that the user asked for.
    .. versionadded:: 16.2.0
class NotAnAttrsClassError(ValueError):
    A non-*attrs* class has been passed into an *attrs* function.
class DefaultAlreadySetError(RuntimeError):
    A default has been set when defining the field and is attempted to be reset
    using the decorator.
    .. versionadded:: 17.1.0
class UnannotatedAttributeError(RuntimeError):
    A class with ``auto_attribs=True`` has a field without a type annotation.
    .. versionadded:: 17.3.0
class PythonTooOldError(RuntimeError):
    It was attempted to use an *attrs* feature that requires a newer Python
    .. versionadded:: 18.2.0
class NotCallableError(TypeError):
    A field requiring a callable has been set with a value that is not
    callable.
    .. versionadded:: 19.2.0
    def __init__(self, msg, value):
        super(TypeError, self).__init__(msg, value)
from attr.exceptions import *  # noqa: F403
class MaxEvalError(Exception):
    Exception raised when the maximum number of evaluations is reached.
class TargetSuccess(Exception):
    Exception raised when the target value is reached.
class CallbackSuccess(StopIteration):
    Exception raised when the callback function raises a ``StopIteration``.
class FeasibleSuccess(Exception):
    Exception raised when a feasible point of a feasible problem is found.
from urllib3.exceptions import HTTPError as BaseHTTPError
Errors, oh no!
import attrs
from referencing._attrs import frozen
    from referencing import Resource
    from referencing.typing import URI
@frozen
class NoSuchResource(KeyError):
    The given URI is not present in a registry.
    Unlike most exceptions, this class *is* intended to be publicly
    instantiable and *is* part of the public API of the package.
    ref: URI
        return attrs.astuple(self) == attrs.astuple(other)
        return hash(attrs.astuple(self))
class NoInternalID(Exception):
    A resource has no internal ID, but one is needed.
    E.g. in modern JSON Schema drafts, this is the :kw:`$id` keyword.
    One might be needed if a resource was to-be added to a registry but no
    other URI is available, and the resource doesn't declare its canonical URI.
    resource: Resource[Any]
class Unretrievable(KeyError):
    The given URI is not present in a registry, and retrieving it failed.
class CannotDetermineSpecification(Exception):
    Attempting to detect the appropriate `Specification` failed.
    This happens if no discernible information is found in the contents of the
    new resource which would help identify it.
    contents: Any
@attrs.frozen  # Because here we allow subclassing below.
class Unresolvable(Exception):
    A reference was unresolvable.
class PointerToNowhere(Unresolvable):
    A JSON Pointer leads to a part of a document that does not exist.
        msg = f"{self.ref!r} does not exist within {self.resource.contents!r}"
        if self.ref == "/":
                ". The pointer '/' is a valid JSON Pointer but it points to "
                "an empty string property ''. If you intended to point "
                "to the entire resource, you should use '#'."
class NoSuchAnchor(Unresolvable):
    An anchor does not exist within a particular resource.
    anchor: str
            f"{self.anchor!r} does not exist within {self.resource.contents!r}"
class InvalidAnchor(Unresolvable):
    An anchor which could never exist in a resource was dereferenced.
    It is somehow syntactically invalid.
            f"'#{self.anchor}' is not a valid anchor, neither as a "
            "plain name anchor nor as a JSON Pointer. You may have intended "
            f"to use '#/{self.anchor}', as the slash is required *before each "
            "segment* of a JSON pointer."
    from .runtime import Undefined
class TemplateError(Exception):
    """Baseclass for all template errors."""
    def __init__(self, message: t.Optional[str] = None) -> None:
    def message(self) -> t.Optional[str]:
        return self.args[0] if self.args else None
class TemplateNotFound(IOError, LookupError, TemplateError):
    """Raised if a template does not exist.
    .. versionchanged:: 2.11
        If the given name is :class:`Undefined` and no message was
        provided, an :exc:`UndefinedError` is raised.
    # Silence the Python warning about message being deprecated since
    # it's not valid here.
    message: t.Optional[str] = None
        name: t.Optional[t.Union[str, "Undefined"]],
        message: t.Optional[str] = None,
        IOError.__init__(self, name)
        if message is None:
            if isinstance(name, Undefined):
                name._fail_with_undefined_error()
            message = name
        self.templates = [name]
class TemplatesNotFound(TemplateNotFound):
    """Like :class:`TemplateNotFound` but raised if multiple templates
    are selected.  This is a subclass of :class:`TemplateNotFound`
    exception, so just catching the base exception will catch both.
        If a name in the list of names is :class:`Undefined`, a message
        about it being undefined is shown rather than the empty string.
        names: t.Sequence[t.Union[str, "Undefined"]] = (),
                    parts.append(name._undefined_message)
                    parts.append(name)
            parts_str = ", ".join(map(str, parts))
            message = f"none of the templates given were found: {parts_str}"
        super().__init__(names[-1] if names else None, message)
        self.templates = list(names)
class TemplateSyntaxError(TemplateError):
    """Raised to tell the user that there is a problem with the template."""
        name: t.Optional[str] = None,
        filename: t.Optional[str] = None,
        self.source: t.Optional[str] = None
        # this is set to True if the debug.translate_syntax_error
        # function translated the syntax error into a new traceback
        self.translated = False
        # for translated errors we only return the message
        if self.translated:
            return t.cast(str, self.message)
        # otherwise attach some stuff
        location = f"line {self.lineno}"
        name = self.filename or self.name
            location = f'File "{name}", {location}'
        lines = [t.cast(str, self.message), "  " + location]
        # if the source is set, add the line to the output
        if self.source is not None:
                line = self.source.splitlines()[self.lineno - 1]
                lines.append("    " + line.strip())
    def __reduce__(self):  # type: ignore
        # https://bugs.python.org/issue1692335 Exceptions that take
        # multiple required arguments have problems with pickling.
        # Without this, raises TypeError: __init__() missing 1 required
        # positional argument: 'lineno'
        return self.__class__, (self.message, self.lineno, self.name, self.filename)
class TemplateAssertionError(TemplateSyntaxError):
    """Like a template syntax error, but covers cases where something in the
    template caused an error at compile time that wasn't necessarily caused
    by a syntax error.  However it's a direct subclass of
    :exc:`TemplateSyntaxError` and has the same attributes.
class TemplateRuntimeError(TemplateError):
    """A generic runtime error in the template engine.  Under some situations
    Jinja may raise this exception.
class UndefinedError(TemplateRuntimeError):
    """Raised if a template tries to operate on :class:`Undefined`."""
class SecurityError(TemplateRuntimeError):
    """Raised if a template tries to do something insecure if the
    sandbox is enabled.
class FilterArgumentError(TemplateRuntimeError):
    """This error is raised if a filter was called with inappropriate
    arguments
"""Exception classes for all of Flake8."""
class Flake8Exception(Exception):
    """Plain Flake8 exception."""
class EarlyQuit(Flake8Exception):
    """Except raised when encountering a KeyboardInterrupt."""
class ExecutionError(Flake8Exception):
    """Exception raised during execution of Flake8."""
class FailedToLoadPlugin(Flake8Exception):
    """Exception raised when a plugin fails to load."""
    FORMAT = 'Flake8 failed to load plugin "%(name)s" due to %(exc)s.'
    def __init__(self, plugin_name: str, exception: Exception) -> None:
        """Initialize our FailedToLoadPlugin exception."""
        self.plugin_name = plugin_name
        self.original_exception = exception
        super().__init__(plugin_name, exception)
        """Format our exception message."""
        return self.FORMAT % {
            "name": self.plugin_name,
            "exc": self.original_exception,
class PluginRequestedUnknownParameters(Flake8Exception):
    """The plugin requested unknown parameters."""
    FORMAT = '"%(name)s" requested unknown parameters causing %(exc)s'
        """Pop certain keyword arguments for initialization."""
class PluginExecutionFailed(Flake8Exception):
    """The plugin failed during execution."""
    FORMAT = '{fname}: "{plugin}" failed during execution due to {exc!r}'
        plugin_name: str,
        exception: Exception,
        """Utilize keyword arguments for message generation."""
        super().__init__(filename, plugin_name, exception)
        return self.FORMAT.format(
            fname=self.filename,
            plugin=self.plugin_name,
            exc=self.original_exception,
from gettext import gettext as _
from gettext import ngettext
from ._compat import get_text_stderr
from .globals import resolve_color_default
from .utils import echo
from .utils import format_filename
    from .core import Command
    from .core import Context
    from .core import Parameter
def _join_param_hints(param_hint: cabc.Sequence[str] | str | None) -> str | None:
    if param_hint is not None and not isinstance(param_hint, str):
        return " / ".join(repr(x) for x in param_hint)
    return param_hint
class ClickException(Exception):
    """An exception that Click can handle and show to the user."""
    #: The exit code for this exception.
    exit_code = 1
    def __init__(self, message: str) -> None:
        # The context will be removed by the time we print the message, so cache
        # the color settings here to be used later on (in `show`)
        self.show_color: bool | None = resolve_color_default()
    def format_message(self) -> str:
    def show(self, file: t.IO[t.Any] | None = None) -> None:
        if file is None:
            file = get_text_stderr()
        echo(
            _("Error: {message}").format(message=self.format_message()),
            color=self.show_color,
class UsageError(ClickException):
    """An internal exception that signals a usage error.  This typically
    aborts any further handling.
    :param message: the error message to display.
    :param ctx: optionally the context that caused this error.  Click will
                fill in the context automatically in some situations.
    exit_code = 2
    def __init__(self, message: str, ctx: Context | None = None) -> None:
        self.cmd: Command | None = self.ctx.command if self.ctx else None
        color = None
            self.ctx is not None
            and self.ctx.command.get_help_option(self.ctx) is not None
            hint = _("Try '{command} {option}' for help.").format(
                command=self.ctx.command_path, option=self.ctx.help_option_names[0]
            hint = f"{hint}\n"
        if self.ctx is not None:
            color = self.ctx.color
            echo(f"{self.ctx.get_usage()}\n{hint}", file=file, color=color)
            color=color,
class BadParameter(UsageError):
    """An exception that formats out a standardized error message for a
    bad parameter.  This is useful when thrown from a callback or type as
    Click will attach contextual information to it (for instance, which
    parameter it is).
    .. versionadded:: 2.0
    :param param: the parameter object that caused this error.  This can
                  be left out, and Click will attach this info itself
                  if possible.
    :param param_hint: a string that shows up as parameter name.  This
                       can be used as alternative to `param` in cases
                       where custom validation should happen.  If it is
                       a string it's used as such, if it's a list then
                       each item is quoted and separated.
        ctx: Context | None = None,
        param: Parameter | None = None,
        param_hint: cabc.Sequence[str] | str | None = None,
        super().__init__(message, ctx)
        self.param = param
        self.param_hint = param_hint
        if self.param_hint is not None:
            param_hint = self.param_hint
        elif self.param is not None:
            param_hint = self.param.get_error_hint(self.ctx)  # type: ignore
            return _("Invalid value: {message}").format(message=self.message)
        return _("Invalid value for {param_hint}: {message}").format(
            param_hint=_join_param_hints(param_hint), message=self.message
class MissingParameter(BadParameter):
    """Raised if click required an option or argument but it was not
    provided when invoking the script.
    :param param_type: a string that indicates the type of the parameter.
                       The default is to inherit the parameter type from
                       the given `param`.  Valid values are ``'parameter'``,
                       ``'option'`` or ``'argument'``.
        message: str | None = None,
        param_type: str | None = None,
        super().__init__(message or "", ctx, param, param_hint)
        self.param_type = param_type
            param_hint: cabc.Sequence[str] | str | None = self.param_hint
            param_hint = None
        param_hint = _join_param_hints(param_hint)
        param_hint = f" {param_hint}" if param_hint else ""
        param_type = self.param_type
        if param_type is None and self.param is not None:
            param_type = self.param.param_type_name
        msg = self.message
        if self.param is not None:
            msg_extra = self.param.type.get_missing_message(
                param=self.param, ctx=self.ctx
            if msg_extra:
                    msg += f". {msg_extra}"
                    msg = msg_extra
        msg = f" {msg}" if msg else ""
        # Translate param_type for known types.
        if param_type == "argument":
            missing = _("Missing argument")
        elif param_type == "option":
            missing = _("Missing option")
        elif param_type == "parameter":
            missing = _("Missing parameter")
            missing = _("Missing {param_type}").format(param_type=param_type)
        return f"{missing}{param_hint}.{msg}"
        if not self.message:
            param_name = self.param.name if self.param else None
            return _("Missing parameter: {param_name}").format(param_name=param_name)
class NoSuchOption(UsageError):
    """Raised if click attempted to handle an option that does not
        option_name: str,
        possibilities: cabc.Sequence[str] | None = None,
            message = _("No such option: {name}").format(name=option_name)
        self.option_name = option_name
        self.possibilities = possibilities
        if not self.possibilities:
        possibility_str = ", ".join(sorted(self.possibilities))
        suggest = ngettext(
            "Did you mean {possibility}?",
            "(Possible options: {possibilities})",
            len(self.possibilities),
        ).format(possibility=possibility_str, possibilities=possibility_str)
        return f"{self.message} {suggest}"
class BadOptionUsage(UsageError):
    """Raised if an option is generally supplied but the use of the option
    was incorrect.  This is for instance raised if the number of arguments
    for an option is not correct.
    :param option_name: the name of the option being used incorrectly.
        self, option_name: str, message: str, ctx: Context | None = None
class BadArgumentUsage(UsageError):
    """Raised if an argument is generally supplied but the use of the argument
    was incorrect.  This is for instance raised if the number of values
    for an argument is not correct.
    .. versionadded:: 6.0
class NoArgsIsHelpError(UsageError):
    def __init__(self, ctx: Context) -> None:
        self.ctx: Context
        super().__init__(ctx.get_help(), ctx=ctx)
        echo(self.format_message(), file=file, err=True, color=self.ctx.color)
class FileError(ClickException):
    """Raised if a file cannot be opened."""
    def __init__(self, filename: str, hint: str | None = None) -> None:
        if hint is None:
            hint = _("unknown error")
        super().__init__(hint)
        self.ui_filename: str = format_filename(filename)
        return _("Could not open file {filename!r}: {message}").format(
            filename=self.ui_filename, message=self.message
class Abort(RuntimeError):
    """An internal signalling exception that signals Click to abort."""
class Exit(RuntimeError):
    """An exception that indicates that the application should exit with some
    status code.
    :param code: the status code to exit with.
    __slots__ = ("exit_code",)
    def __init__(self, code: int = 0) -> None:
        self.exit_code: int = code
"""Exceptions defined by Beautiful Soup itself."""
class StopParsing(Exception):
    """Exception raised by a TreeBuilder if it's unable to continue parsing."""
class FeatureNotFound(ValueError):
    """Exception raised by the BeautifulSoup constructor if no parser with the
    requested features is found.
class ParserRejectedMarkup(Exception):
    """An Exception to be raised when the underlying parser simply
    refuses to parse the given markup.
    def __init__(self, message_or_exception: Union[str, Exception]):
        """Explain why the parser rejected the given markup, either
        with a textual explanation or another exception.
        if isinstance(message_or_exception, Exception):
            e = message_or_exception
            message_or_exception = "%s: %s" % (e.__class__.__name__, str(e))
        super(ParserRejectedMarkup, self).__init__(message_or_exception)
class HTTPException(Exception):
    def __init__(self, status_code: int, detail: str | None = None, headers: Mapping[str, str] | None = None) -> None:
        if detail is None:
            detail = http.HTTPStatus(status_code).phrase
        self.detail = detail
        return f"{self.status_code}: {self.detail}"
        class_name = self.__class__.__name__
        return f"{class_name}(status_code={self.status_code!r}, detail={self.detail!r})"
class WebSocketException(Exception):
    def __init__(self, code: int, reason: str | None = None) -> None:
        self.reason = reason or ""
        return f"{self.code}: {self.reason}"
        return f"{class_name}(code={self.code!r}, reason={self.reason!r})"
from starlette._exception_handler import (
    ExceptionHandlers,
    StatusHandlers,
    wrap_app_handling_exceptions,
from starlette.exceptions import HTTPException, WebSocketException
from starlette.types import ASGIApp, ExceptionHandler, Receive, Scope, Send
class ExceptionMiddleware:
        handlers: Mapping[Any, ExceptionHandler] | None = None,
        self.debug = debug  # TODO: We ought to handle 404 cases if debug is set.
        self._status_handlers: StatusHandlers = {}
        self._exception_handlers: ExceptionHandlers = {
            HTTPException: self.http_exception,
            WebSocketException: self.websocket_exception,
        if handlers is not None:  # pragma: no branch
            for key, value in handlers.items():
                self.add_exception_handler(key, value)
    def add_exception_handler(
        exc_class_or_status_code: int | type[Exception],
        handler: ExceptionHandler,
        if isinstance(exc_class_or_status_code, int):
            self._status_handlers[exc_class_or_status_code] = handler
            assert issubclass(exc_class_or_status_code, Exception)
            self._exception_handlers[exc_class_or_status_code] = handler
        if scope["type"] not in ("http", "websocket"):
        scope["starlette.exception_handlers"] = (
            self._exception_handlers,
            self._status_handlers,
        conn: Request | WebSocket
        if scope["type"] == "http":
            conn = Request(scope, receive, send)
            conn = WebSocket(scope, receive, send)
        await wrap_app_handling_exceptions(self.app, conn)(scope, receive, send)
    async def http_exception(self, request: Request, exc: Exception) -> Response:
        assert isinstance(exc, HTTPException)
        if exc.status_code in {204, 304}:
            return Response(status_code=exc.status_code, headers=exc.headers)
        return PlainTextResponse(exc.detail, status_code=exc.status_code, headers=exc.headers)
    async def websocket_exception(self, websocket: WebSocket, exc: Exception) -> None:
        assert isinstance(exc, WebSocketException)
        await websocket.close(code=exc.code, reason=exc.reason)  # pragma: no cover
Validation errors, and some surrounding helpers.
from collections import defaultdict, deque
from textwrap import dedent, indent
from typing import TYPE_CHECKING, Any, ClassVar
import heapq
from attrs import define
from referencing.exceptions import Unresolvable as _Unresolvable
from jsonschema import _utils
    from collections.abc import Iterable, Mapping, MutableMapping, Sequence
    from jsonschema import _types
WEAK_MATCHES: frozenset[str] = frozenset(["anyOf", "oneOf"])
STRONG_MATCHES: frozenset[str] = frozenset()
_JSON_PATH_COMPATIBLE_PROPERTY_PATTERN = re.compile("^[a-zA-Z][a-zA-Z0-9_]*$")
_unset = _utils.Unset()
def _pretty(thing: Any, prefix: str):
    Format something for an error message as prettily as we currently can.
    return indent(pformat(thing, width=72, sort_dicts=False), prefix).lstrip()
    if name == "RefResolutionError":
class _Error(Exception):
    _word_for_schema_in_error_message: ClassVar[str]
    _word_for_instance_in_error_message: ClassVar[str]
        validator: str = _unset,  # type: ignore[assignment]
        path: Iterable[str | int] = (),
        cause: Exception | None = None,
        context=(),
        validator_value: Any = _unset,
        instance: Any = _unset,
        schema: Mapping[str, Any] | bool = _unset,  # type: ignore[assignment]
        schema_path: Iterable[str | int] = (),
        parent: _Error | None = None,
        type_checker: _types.TypeChecker = _unset,  # type: ignore[assignment]
            cause,
            validator_value,
            schema_path,
        self.path = self.relative_path = deque(path)
        self.schema_path = self.relative_schema_path = deque(schema_path)
        self.context = list(context)
        self.cause = self.__cause__ = cause
        self.validator_value = validator_value
        self._type_checker = type_checker
        for error in context:
            error.parent = self
        return f"<{self.__class__.__name__}: {self.message!r}>"
        essential_for_verbose = (
            self.validator, self.validator_value, self.instance, self.schema,
        if any(m is _unset for m in essential_for_verbose):
        schema_path = _utils.format_as_index(
            container=self._word_for_schema_in_error_message,
            indices=list(self.relative_schema_path)[:-1],
        instance_path = _utils.format_as_index(
            container=self._word_for_instance_in_error_message,
            indices=self.relative_path,
        prefix = 16 * " "
            f"""\
            {self.message}
            Failed validating {self.validator!r} in {schema_path}:
                {_pretty(self.schema, prefix=prefix)}
            On {instance_path}:
                {_pretty(self.instance, prefix=prefix)}
            """.rstrip(),
    def create_from(cls, other: _Error):
        return cls(**other._contents())
    def absolute_path(self) -> Sequence[str | int]:
        parent = self.parent
            return self.relative_path
        path = deque(self.relative_path)
        path.extendleft(reversed(parent.absolute_path))
    def absolute_schema_path(self) -> Sequence[str | int]:
            return self.relative_schema_path
        path = deque(self.relative_schema_path)
        path.extendleft(reversed(parent.absolute_schema_path))
    def json_path(self) -> str:
        path = "$"
        for elem in self.absolute_path:
            if isinstance(elem, int):
                path += "[" + str(elem) + "]"
            elif _JSON_PATH_COMPATIBLE_PROPERTY_PATTERN.match(elem):
                path += "." + elem
                escaped_elem = elem.replace("\\", "\\\\").replace("'", r"\'")
                path += "['" + escaped_elem + "']"
    def _set(
        type_checker: _types.TypeChecker | None = None,
        if type_checker is not None and self._type_checker is _unset:
            if getattr(self, k) is _unset:
    def _contents(self):
        attrs = (
            "message", "cause", "context", "validator", "validator_value",
            "path", "schema_path", "instance", "schema", "parent",
        return {attr: getattr(self, attr) for attr in attrs}
    def _matches_type(self) -> bool:
            # We ignore this as we want to simply crash if this happens
            expected = self.schema["type"]  # type: ignore[index]
            return self._type_checker.is_type(self.instance, expected)
            self._type_checker.is_type(self.instance, expected_type)
            for expected_type in expected
class ValidationError(_Error):
    An instance was invalid under a provided schema.
    _word_for_schema_in_error_message = "schema"
    _word_for_instance_in_error_message = "instance"
class SchemaError(_Error):
    A schema was invalid under its corresponding metaschema.
    _word_for_schema_in_error_message = "metaschema"
    _word_for_instance_in_error_message = "schema"
@define(slots=False)
class _RefResolutionError(Exception):  # noqa: PLW1641
    A ref could not be resolved.
    _DEPRECATION_MESSAGE = (
        "jsonschema.exceptions.RefResolutionError is deprecated as of version "
        "4.18.0. If you wish to catch potential reference resolution errors, "
        "directly catch referencing.exceptions.Unresolvable."
    _cause: Exception
            return NotImplemented  # pragma: no cover -- uncovered but deprecated  # noqa: E501
        return self._cause == other._cause
        return str(self._cause)
class _WrappedReferencingError(_RefResolutionError, _Unresolvable):  # pragma: no cover -- partially uncovered but to be removed  # noqa: E501
    def __init__(self, cause: _Unresolvable):
        object.__setattr__(self, "_wrapped", cause)
        if other.__class__ is self.__class__:
            return self._wrapped == other._wrapped
        elif other.__class__ is self._wrapped.__class__:
            return self._wrapped == other
        return getattr(self._wrapped, attr)
        return hash(self._wrapped)
        return f"<WrappedReferencingError {self._wrapped!r}>"
        return f"{self._wrapped.__class__.__name__}: {self._wrapped}"
class UndefinedTypeCheck(Exception):
    A type checker was asked to check a type it did not have registered.
    def __init__(self, type: str) -> None:
        return f"Type {self.type!r} is unknown to this type checker"
class UnknownType(Exception):
    A validator was asked to validate an instance against an unknown type.
    def __init__(self, type, instance, schema):
            Unknown type {self.type!r} for validator with schema:
            While checking instance:
class FormatError(Exception):
    Validating a format failed.
    def __init__(self, message, cause=None):
        super().__init__(message, cause)
class ErrorTree:
    ErrorTrees make it easier to check which validations failed.
    _instance = _unset
    def __init__(self, errors: Iterable[ValidationError] = ()):
        self.errors: MutableMapping[str, ValidationError] = {}
        self._contents: Mapping[str, ErrorTree] = defaultdict(self.__class__)
            container = self
            for element in error.path:
                container = container[element]
            container.errors[error.validator] = error
            container._instance = error.instance
    def __contains__(self, index: str | int):
        Check whether ``instance[index]`` has any errors.
        return index in self._contents
        Retrieve the child tree one level down at the given ``index``.
        If the index is not in the instance that this tree corresponds
        to and is not known by this tree, whatever error would be raised
        by ``instance.__getitem__`` will be propagated (usually this is
        some subclass of `LookupError`.
        if self._instance is not _unset and index not in self:
            self._instance[index]
        return self._contents[index]
    def __setitem__(self, index: str | int, value: ErrorTree):
        Add an error to the tree at the given ``index``.
        .. deprecated:: v4.20.0
            Setting items on an `ErrorTree` is deprecated without replacement.
            To populate a tree, provide all of its sub-errors when you
            construct the tree.
            "ErrorTree.__setitem__ is deprecated without replacement.",
        self._contents[index] = value  # type: ignore[index]
        Iterate (non-recursively) over the indices in the instance with errors.
        return iter(self._contents)
        Return the `total_errors`.
        return self.total_errors
        total = len(self)
        errors = "error" if total == 1 else "errors"
        return f"<{self.__class__.__name__} ({total} total {errors})>"
    def total_errors(self):
        The total number of errors in the entire tree, including children.
        child_errors = sum(len(tree) for _, tree in self._contents.items())
        return len(self.errors) + child_errors
def by_relevance(weak=WEAK_MATCHES, strong=STRONG_MATCHES):
    Create a key function that can be used to sort errors by relevance.
        weak (set):
            a collection of validation keywords to consider to be
            "weak".  If there are two errors at the same level of the
            instance and one is in the set of weak validation keywords,
            the other error will take priority. By default, :kw:`anyOf`
            and :kw:`oneOf` are considered weak keywords and will be
            superseded by other same-level validation errors.
        strong (set):
            "strong"
    def relevance(error):
        validator = error.validator
        return (                        # prefer errors which are ...
            -len(error.path),           # 'deeper' and thereby more specific
            error.path,                 # earlier (for sibling errors)
            validator not in weak,      # for a non-low-priority keyword
            validator in strong,        # for a high priority keyword
            not error._matches_type(),  # at least match the instance's type
        )                               # otherwise we'll treat them the same
    return relevance
relevance = by_relevance()
A key function (e.g. to use with `sorted`) which sorts errors by relevance.
    sorted(validator.iter_errors(12), key=jsonschema.exceptions.relevance)
def best_match(errors, key=relevance):
    Try to find an error that appears to be the best match among given errors.
    In general, errors that are higher up in the instance (i.e. for which
    `ValidationError.path` is shorter) are considered better matches,
    since they indicate "more" is wrong with the instance.
    If the resulting match is either :kw:`oneOf` or :kw:`anyOf`, the
    *opposite* assumption is made -- i.e. the deepest error is picked,
    since these keywords only need to match once, and any other errors
    may not be relevant.
        errors (collections.abc.Iterable):
            the errors to select from. Do not provide a mixture of
            errors from different validation attempts (i.e. from
            different instances or schemas), since it won't produce
            sensical output.
        key (collections.abc.Callable):
            the key to use when sorting errors. See `relevance` and
            transitively `by_relevance` for more details (the default is
            to sort with the defaults of that function). Changing the
            default is only useful if you want to change the function
            that rates errors but still want the error context descent
            done by this function.
        the best matching error, or ``None`` if the iterable was empty
        This function is a heuristic. Its return value may change for a given
        set of inputs from version to version if better heuristics are added.
    best = max(errors, key=key, default=None)
    if best is None:
    while best.context:
        # Calculate the minimum via nsmallest, because we don't recurse if
        # all nested errors have the same relevance (i.e. if min == max == all)
        smallest = heapq.nsmallest(2, best.context, key=key)
        if len(smallest) == 2 and key(smallest[0]) == key(smallest[1]):  # noqa: PLR2004
            return best
        best = smallest[0]
from typing import Annotated, Any, Optional, TypedDict, Union
            Optional[Mapping[str, str]],
        endpoint_ctx: Optional[EndpointContext] = None,
# +-----------------------------------------------+
# |                                               |
# |           Give Feedback / Get Help            |
# | https://github.com/BerriAI/litellm/issues/new |
#  Thank you users! We ❤️ you! - Krrish & Ishaan
## LiteLLM versions of the OpenAI Exception Types
from litellm.types.utils import LiteLLMCommonStrings
_MINIMAL_ERROR_RESPONSE: Optional[httpx.Response] = None
def _get_minimal_error_response() -> httpx.Response:
    """Get a cached minimal httpx.Response object for error cases."""
    global _MINIMAL_ERROR_RESPONSE
    if _MINIMAL_ERROR_RESPONSE is None:
        _MINIMAL_ERROR_RESPONSE = httpx.Response(
            request=httpx.Request(
                method="GET", url="https://litellm.ai"
    return _MINIMAL_ERROR_RESPONSE
class AuthenticationError(openai.AuthenticationError):  # type: ignore
        llm_provider,
        response: Optional[httpx.Response] = None,
        litellm_debug_info: Optional[str] = None,
        max_retries: Optional[int] = None,
        num_retries: Optional[int] = None,
        self.status_code = 401
        self.message = "litellm.AuthenticationError: {}".format(message)
        self.llm_provider = llm_provider
        self.litellm_debug_info = litellm_debug_info
        self.num_retries = num_retries
        self.response = response or httpx.Response(
            status_code=self.status_code,
            ),  # mock request object
            self.message, response=self.response, body=None
        )  # Call the base class constructor with the parameters it needs
        _message = self.message
        if self.num_retries:
            _message += f" LiteLLM Retried: {self.num_retries} times"
        if self.max_retries:
            _message += f", LiteLLM Max Retries: {self.max_retries}"
        return _message
# raise when invalid models passed, example gpt-8
class NotFoundError(openai.NotFoundError):  # type: ignore
        self.status_code = 404
        self.message = "litellm.NotFoundError: {}".format(message)
class BadRequestError(openai.BadRequestError):  # type: ignore
        body: Optional[dict] = None,
        self.status_code = 400
        self.message = "litellm.BadRequestError: {}".format(message)
        # Use response if it's a valid httpx.Response with a request, otherwise use minimal error response
        # Note: We check _request (not .request property) to avoid RuntimeError when _request is None
            response is not None
            and isinstance(response, httpx.Response)
            and hasattr(response, "_request")
            and getattr(response, "_request", None) is not None
            self.response = _get_minimal_error_response()
            self.message, response=self.response, body=body
class ImageFetchError(BadRequestError):
        llm_provider=None,
            llm_provider=llm_provider,
            litellm_debug_info=litellm_debug_info,
class UnprocessableEntityError(openai.UnprocessableEntityError):  # type: ignore
        self.status_code = 422
        self.message = "litellm.UnprocessableEntityError: {}".format(message)
            self.message, response=response, body=None
class Timeout(openai.APITimeoutError):  # type: ignore
        headers: Optional[dict] = None,
        exception_status_code: Optional[int] = None,
        request = httpx.Request(
            url="https://api.openai.com/v1",
            request=request
        self.status_code = exception_status_code or 408
        self.message = "litellm.Timeout: {}".format(message)
    # custom function to convert to str
class PermissionDeniedError(openai.PermissionDeniedError):  # type:ignore
        self.status_code = 403
        self.message = "litellm.PermissionDeniedError: {}".format(message)
class RateLimitError(openai.RateLimitError):  # type: ignore
        self.status_code = 429
        self.message = "litellm.RateLimitError: {}".format(message)
        _response_headers = (
            getattr(response, "headers", None) if response is not None else None
        self.response = httpx.Response(
            headers=_response_headers,
                url=" https://cloud.google.com/vertex-ai/",
        self.code = "429"
        self.type = "throttling_error"
# sub class of rate limit error - meant to give more granularity for error handling context window exceeded errors
class ContextWindowExceededError(BadRequestError):  # type: ignore
            model=self.model,  # type: ignore
            llm_provider=self.llm_provider,  # type: ignore
            litellm_debug_info=self.litellm_debug_info,
        # set after, to make it clear the raised error is a context window exceeded error
        self.message = "litellm.ContextWindowExceededError: {}".format(self.message)
# sub class of bad request error - meant to help us catch guardrails-related errors on proxy.
class RejectedRequestError(BadRequestError):  # type: ignore
        request_data: dict,
        self.message = "litellm.RejectedRequestError: {}".format(message)
        self.request_data = request_data
        request = httpx.Request(method="POST", url="https://api.openai.com/v1")
        response = httpx.Response(status_code=400, request=request)
            message=self.message,
class ContentPolicyViolationError(BadRequestError):  # type: ignore
    #  Error code: 400 - {'error': {'code': 'content_policy_violation', 'message': 'Your request was rejected as a result of our safety system. Image descriptions generated from your prompt may contain text that is not allowed by our safety system. If you believe this was done in error, your request may succeed if retried, or by adjusting your prompt.', 'param': None, 'type': 'invalid_request_error'}}
        provider_specific_fields: Optional[dict] = None,
        self.message = "litellm.ContentPolicyViolationError: {}".format(message)
        self.provider_specific_fields = provider_specific_fields
        return self._transform_error_to_string()
    def _transform_error_to_string(self) -> str:
        Transform the error to a string
class ServiceUnavailableError(openai.APIStatusError):  # type: ignore
        self.status_code = 503
        self.message = "litellm.ServiceUnavailableError: {}".format(message)
class BadGatewayError(openai.APIStatusError):  # type: ignore
        self.status_code = 502
        self.message = "litellm.BadGatewayError: {}".format(message)
class InternalServerError(openai.InternalServerError):  # type: ignore
        self.status_code = 500
        self.message = "litellm.InternalServerError: {}".format(message)
# raise this when the API returns an invalid response object - https://github.com/openai/openai-python/blob/1be14ee34a0f8e42d3f9aa5451aa4cb161f1781f/openai/api_requestor.py#L401
class APIError(openai.APIError):  # type: ignore
        request: Optional[httpx.Request] = None,
        self.message = "litellm.APIError: {}".format(message)
        super().__init__(self.message, request=request, body=None)  # type: ignore
# raised if an invalid request (not get, delete, put, post) is made
class APIConnectionError(openai.APIConnectionError):  # type: ignore
        self.message = "litellm.APIConnectionError: {}".format(message)
        self.request = httpx.Request(method="POST", url="https://api.openai.com/v1")
        super().__init__(message=self.message, request=self.request)
class APIResponseValidationError(openai.APIResponseValidationError):  # type: ignore
        self.message = "litellm.APIResponseValidationError: {}".format(message)
        response = httpx.Response(status_code=500, request=request)
        super().__init__(response=response, body=None, message=message)
class JSONSchemaValidationError(APIResponseValidationError):
        self, model: str, llm_provider: str, raw_response: str, schema: str
        self.raw_response = raw_response
        message = "litellm.JSONSchemaValidationError: model={}, returned an invalid response={}, for schema={}.\nAccess raw response with `e.raw_response`".format(
            model, raw_response, schema
        super().__init__(model=model, message=message, llm_provider=llm_provider)
class OpenAIError(openai.OpenAIError):  # type: ignore
    def __init__(self, original_exception=None):
        self.llm_provider = "openai"
class UnsupportedParamsError(BadRequestError):
        llm_provider: Optional[str] = None,
        status_code: int = 400,
        self.message = "litellm.UnsupportedParamsError: {}".format(message)
        response = response or httpx.Response(
LITELLM_EXCEPTION_TYPES = [
    RejectedRequestError,
class BudgetExceededError(Exception):
        self, current_cost: float, max_budget: float, message: Optional[str] = None
        self.current_cost = current_cost
        self.max_budget = max_budget
            or f"Budget has been exceeded! Current cost: {current_cost}, Max budget: {max_budget}"
## DEPRECATED ##
class InvalidRequestError(openai.BadRequestError):  # type: ignore
    def __init__(self, message, model, llm_provider):
            message=self.message, response=self.response, body=None
class MockException(openai.APIError):
    # used for testing
        self.message = "litellm.MockException: {}".format(message)
class LiteLLMUnknownProvider(BadRequestError):
    def __init__(self, model: str, custom_llm_provider: Optional[str] = None):
        self.message = LiteLLMCommonStrings.llm_provider_not_provided.value.format(
            model=model, custom_llm_provider=custom_llm_provider
            self.message, model=model, llm_provider=custom_llm_provider, response=None
class GuardrailRaisedException(Exception):
        guardrail_name: Optional[str] = None,
        should_wrap_with_default_message: bool = True,
        default_message = f"Guardrail raised an exception, Guardrail: {guardrail_name}, Message: {message}"
        self.guardrail_name = guardrail_name
        self.message = default_message if should_wrap_with_default_message else message
        super().__init__(self.message)
class BlockedPiiEntityError(Exception):
        entity_type: str,
        Raised when a blocked entity is detected by a guardrail.
        self.entity_type = entity_type
        self.message = f"Blocked entity detected: {entity_type} by Guardrail: {guardrail_name}. This entity is not allowed to be used in this request."
class MidStreamFallbackError(ServiceUnavailableError):  # type: ignore
        llm_provider: str,
        original_exception: Optional[Exception] = None,
        generated_content: str = "",
        is_pre_first_chunk: bool = False,
        self.status_code = 503  # Service Unavailable
        self.message = f"litellm.MidStreamFallbackError: {message}"
        self.original_exception = original_exception
        self.generated_content = generated_content
        self.is_pre_first_chunk = is_pre_first_chunk
        # Create a response if one wasn't provided
                    url=f"https://{llm_provider}.com/v1/",
        # Call the parent constructor
            response=self.response,
            max_retries=self.max_retries,
            num_retries=self.num_retries,
        if self.original_exception:
            _message += f" Original exception: {type(self.original_exception).__name__}: {str(self.original_exception)}"
class GuardrailInterventionNormalStringError(
    Exception
):  # custom exception to raise when a guardrail intervenes, but we want to return a normal string to the user
A2A Protocol Exceptions.
Custom exception types for A2A protocol operations, following LiteLLM's exception pattern.
class A2AError(Exception):
    Base exception for A2A protocol errors.
    Follows the same pattern as LiteLLM's main exceptions.
        status_code: int = 500,
        llm_provider: str = "a2a_agent",
        self.message = f"litellm.A2AError: {message}"
            request=httpx.Request(method="POST", url="https://litellm.ai"),
class A2AConnectionError(A2AError):
    Raised when connection to an A2A agent fails.
    This typically occurs when:
    - The agent is unreachable
    - The agent card contains a localhost/internal URL
    - Network issues prevent connection
        url: Optional[str] = None,
            status_code=503,
            llm_provider="a2a_agent",
class A2AAgentCardError(A2AError):
    Raised when there's an issue with the agent card.
    This includes:
    - Failed to fetch agent card
    - Invalid agent card format
    - Missing required fields
            status_code=404,
class A2ALocalhostURLError(A2AConnectionError):
    Raised when an agent card contains a localhost/internal URL.
    Many A2A agents are deployed with agent cards that contain internal URLs
    like "http://0.0.0.0:8001/" or "http://localhost:8000/". This error
    indicates that the URL needs to be corrected and the request should be retried.
        localhost_url: The localhost/internal URL found in the agent card
        base_url: The public base URL that should be used instead
        original_error: The original connection error that was raised
        localhost_url: str,
        original_error: Optional[Exception] = None,
        self.localhost_url = localhost_url
        self.original_error = original_error
            f"Agent card contains localhost/internal URL '{localhost_url}'. "
            f"Retrying with base URL '{base_url}'."
            url=localhost_url,
"""Anthropic error format type definitions."""
# Known Anthropic error types
# Source: https://docs.anthropic.com/en/api/errors
AnthropicErrorType = Literal[
    "invalid_request_error",
    "authentication_error",
    "permission_error",
    "not_found_error",
    "request_too_large",
    "rate_limit_error",
    "api_error",
    "overloaded_error",
class AnthropicErrorDetail(TypedDict):
    """Inner error detail in Anthropic format."""
    type: AnthropicErrorType
class AnthropicErrorResponse(TypedDict, total=False):
    Anthropic-formatted error response.
    Format:
        "type": "error",
        "error": {"type": "...", "message": "..."},
        "request_id": "req_..."  # optional
    type: Required[Literal["error"]]
    error: Required[AnthropicErrorDetail]
    request_id: str
class UnauthorizedError(Exception):
    """Exception raised when the API returns a 401 Unauthorized response."""
    def __init__(self, orig_exception: Union[requests.exceptions.HTTPError, str]):
        self.orig_exception = orig_exception
        super().__init__(str(orig_exception))
class NotFoundError(Exception):
    """Exception raised when the API returns a 404 Not Found response or indicates a resource was not found."""
from aider.dump import dump  # noqa: F401
class ExInfo:
    retry: bool
EXCEPTIONS = [
    ExInfo("APIConnectionError", True, None),
    ExInfo("APIError", True, None),
    ExInfo("APIResponseValidationError", True, None),
    ExInfo(
        "The API provider is not able to authenticate you. Check your API key.",
    ExInfo("AzureOpenAIError", True, None),
    ExInfo("BadGatewayError", True, "The API provider's servers are down or overloaded."),
    ExInfo("BadRequestError", False, None),
    ExInfo("BudgetExceededError", True, None),
        "ContentPolicyViolationError",
        "The API provider has refused the request due to a safety policy about the content.",
    ExInfo("ContextWindowExceededError", False, None),  # special case handled in base_coder
    ExInfo("ImageFetchError", False, "The API provider was unable to fetch one or more images."),
    ExInfo("InternalServerError", True, "The API provider's servers are down or overloaded."),
    ExInfo("InvalidRequestError", True, None),
    ExInfo("JSONSchemaValidationError", True, None),
    ExInfo("NotFoundError", False, None),
    ExInfo("OpenAIError", True, None),
        "The API provider has rate limited you. Try again later or check your quotas.",
    ExInfo("RouterRateLimitError", True, None),
    ExInfo("ServiceUnavailableError", True, "The API provider's servers are down or overloaded."),
    ExInfo("UnprocessableEntityError", True, None),
    ExInfo("UnsupportedParamsError", True, None),
        "The API provider timed out without returning a response. They may be down or overloaded.",
class LiteLLMExceptions:
    exceptions = dict()
    exception_info = {exi.name: exi for exi in EXCEPTIONS}
        self._load()
    def _load(self, strict=False):
        for var in dir(litellm):
            # Filter by BaseException because instances of non-exception classes cannot be caught.
            # `litellm.ErrorEventError` is an example of a regular class which just happens to end
            # with `Error`.
            if var.endswith("Error") and issubclass(getattr(litellm, var), BaseException):
                if var not in self.exception_info:
                    raise ValueError(f"{var} is in litellm but not in aider's exceptions list")
        for var in self.exception_info:
            ex = getattr(litellm, var)
            self.exceptions[ex] = self.exception_info[var]
    def exceptions_tuple(self):
        return tuple(self.exceptions)
    def get_ex_info(self, ex):
        """Return the ExInfo for a given exception instance"""
        if ex.__class__ is litellm.APIConnectionError:
            if "boto3" in str(ex):
                return ExInfo("APIConnectionError", False, "You need to: pip install boto3")
            if "OpenrouterException" in str(ex) and "'choices'" in str(ex):
                return ExInfo(
                        "OpenRouter or the upstream API provider is down, overloaded or rate"
                        " limiting your requests."
        # Check for specific non-retryable APIError cases like insufficient credits
        if ex.__class__ is litellm.APIError:
            err_str = str(ex).lower()
            if "insufficient credits" in err_str and '"code":402' in err_str:
                    "Insufficient credits with the API provider. Please add credits.",
            # Fall through to default APIError handling if not the specific credits error
        return self.exceptions.get(ex.__class__, ExInfo(None, None, None))
