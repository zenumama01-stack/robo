class TweepyException(Exception):
    """Base exception for Tweepy
class HTTPException(TweepyException):
    """HTTPException()
    Exception raised when an HTTP request fails
    .. versionchanged:: 4.10
        ``response`` attribute can be an instance of
        :class:`aiohttp.ClientResponse`
    response : requests.Response | aiohttp.ClientResponse
        Requests Response from the Twitter API
    api_errors : list[dict[str, int | str]]
        The errors the Twitter API responded with, if any
    api_codes : list[int]
        The error codes the Twitter API responded with, if any
    api_messages : list[str]
        The error messages the Twitter API responded with, if any
    def __init__(self, response, *, response_json=None):
        self.response = response
        self.api_errors = []
        self.api_codes = []
        self.api_messages = []
            status_code = response.status_code
            # response is an instance of aiohttp.ClientResponse
            status_code = response.status
        if response_json is None:
                response_json = response.json()
            except requests.JSONDecodeError:
                super().__init__(f"{status_code} {response.reason}")
        errors = response_json.get("errors", [])
        # Use := when support for Python 3.7 is dropped
        if "error" in response_json:
            errors.append(response_json["error"])
        error_text = ""
        for error in errors:
            self.api_errors.append(error)
            if isinstance(error, str):
                self.api_messages.append(error)
                error_text += '\n' + error
            if "code" in error:
                self.api_codes.append(error["code"])
            if "message" in error:
                self.api_messages.append(error["message"])
            if "code" in error and "message" in error:
                error_text += f"\n{error['code']} - {error['message']}"
            elif "message" in error:
                error_text += '\n' + error["message"]
        if not error_text and "detail" in response_json:
            self.api_messages.append(response_json["detail"])
            error_text = '\n' + response_json["detail"]
        super().__init__(
            f"{status_code} {response.reason}{error_text}"
class BadRequest(HTTPException):
    """BadRequest()
    Exception raised for a 400 HTTP status code
class Unauthorized(HTTPException):
    """Unauthorized()
    Exception raised for a 401 HTTP status code
class Forbidden(HTTPException):
    """Forbidden()
    Exception raised for a 403 HTTP status code
class NotFound(HTTPException):
    """NotFound()
    Exception raised for a 404 HTTP status code
class TooManyRequests(HTTPException):
    """TooManyRequests()
    Exception raised for a 429 HTTP status code
    reset_time : int | None
        Unix timestamp when the rate limit resets, if available
    def __init__(self, response, *, response_json=None, reset_time=None):
        super().__init__(response, response_json=response_json)
        self.reset_time = reset_time
class TwitterServerError(HTTPException):
    """TwitterServerError()
    Exception raised for a 5xx HTTP status code
"""Errors for the library.
All exceptions defined by the library
should be defined in this file.
class Error(Exception):
    """Base error for this module."""
class HttpError(Error):
    """HTTP data was invalid or unexpected."""
    @util.positional(3)
    def __init__(self, resp, content, uri=None):
        self.resp = resp
        if not isinstance(content, bytes):
            raise TypeError("HTTP content should be bytes")
        self.content = content
        self.uri = uri
        self.error_details = ""
        self.reason = self._get_reason()
    def status_code(self):
        """Return the HTTP status code from the response content."""
        return self.resp.status
    def _get_reason(self):
        """Calculate the reason for the error from the response content."""
        reason = self.resp.reason
                data = json.loads(self.content.decode("utf-8"))
                # In case it is not json
                data = self.content.decode("utf-8")
                reason = data["error"]["message"]
                error_detail_keyword = next(
                        kw
                        for kw in ["detail", "details", "errors", "message"]
                        if kw in data["error"]
                if error_detail_keyword:
                    self.error_details = data["error"][error_detail_keyword]
            elif isinstance(data, list) and len(data) > 0:
                first_error = data[0]
                reason = first_error["error"]["message"]
                if "details" in first_error["error"]:
                    self.error_details = first_error["error"]["details"]
                self.error_details = data
        except (ValueError, KeyError, TypeError):
        if reason is None:
            reason = ""
        return reason.strip()
        if self.error_details:
            return '<HttpError %s when requesting %s returned "%s". Details: "%s">' % (
                self.resp.status,
                self.uri,
                self.reason,
                self.error_details,
        elif self.uri:
            return '<HttpError %s when requesting %s returned "%s">' % (
            return '<HttpError %s "%s">' % (self.resp.status, self.reason)
    __str__ = __repr__
class InvalidJsonError(Error):
    """The JSON returned could not be parsed."""
class UnknownFileType(Error):
    """File type unknown or unexpected."""
class UnknownLinkType(Error):
    """Link type unknown or unexpected."""
class UnknownApiNameOrVersion(Error):
    """No API with that name and version exists."""
class UnacceptableMimeTypeError(Error):
    """That is an unacceptable mimetype for this operation."""
class MediaUploadSizeError(Error):
    """Media is larger than the method can accept."""
class ResumableUploadError(HttpError):
    """Error occurred during resumable upload."""
class InvalidChunkSizeError(Error):
    """The given chunksize is not valid."""
class InvalidNotificationError(Error):
    """The channel Notification is invalid."""
class BatchError(HttpError):
    """Error occurred during batch operations."""
    def __init__(self, reason, resp=None, content=None):
        self.reason = reason
        if getattr(self.resp, "status", None) is None:
            return '<BatchError "%s">' % (self.reason)
            return '<BatchError %s "%s">' % (self.resp.status, self.reason)
class UnexpectedMethodError(Error):
    """Exception raised by RequestMockBuilder on unexpected calls."""
    @util.positional(1)
    def __init__(self, methodId=None):
        """Constructor for an UnexpectedMethodError."""
        super(UnexpectedMethodError, self).__init__(
            "Received unexpected call %s" % methodId
class UnexpectedBodyError(Error):
    """Exception raised by RequestMockBuilder on unexpected bodies."""
    def __init__(self, expected, provided):
        super(UnexpectedBodyError, self).__init__(
            "Expected: [%s] - Provided: [%s]" % (expected, provided)
import traceback, sys
class WafError(Exception):
    def __init__(self, msg='', ex=None):
        Exception.__init__(self)
        assert not isinstance(msg, Exception)
        self.stack = []
        if ex:
            if not msg:
                self.msg = str(ex)
            if isinstance(ex, WafError):
                self.stack = ex.stack
                self.stack = traceback.extract_tb(sys.exc_info()[2])
        self.stack += traceback.extract_stack()[:-1]
        self.verbose_msg = ''.join(traceback.format_list(self.stack))
        return str(self.msg)
class BuildError(WafError):
    def __init__(self, error_tasks=[]):
        self.tasks = error_tasks
        WafError.__init__(self, self.format_error())
    def format_error(self):
        lst = ['Build failed']
        for tsk in self.tasks:
            txt = tsk.format_error()
            if txt:
                lst.append(txt)
        return '\n'.join(lst)
class ConfigurationError(WafError):
class TaskRescan(WafError):
class TaskNotReady(WafError):
"""setuptools.errors
Provides exceptions used by setuptools modules.
from distutils import errors as _distutils_errors
# Re-export errors from distutils to facilitate the migration to PEP632
ByteCompileError = _distutils_errors.DistutilsByteCompileError
CCompilerError = _distutils_errors.CCompilerError
ClassError = _distutils_errors.DistutilsClassError
CompileError = _distutils_errors.CompileError
ExecError = _distutils_errors.DistutilsExecError
FileError = _distutils_errors.DistutilsFileError
InternalError = _distutils_errors.DistutilsInternalError
LibError = _distutils_errors.LibError
LinkError = _distutils_errors.LinkError
ModuleError = _distutils_errors.DistutilsModuleError
OptionError = _distutils_errors.DistutilsOptionError
PlatformError = _distutils_errors.DistutilsPlatformError
PreprocessError = _distutils_errors.PreprocessError
SetupError = _distutils_errors.DistutilsSetupError
TemplateError = _distutils_errors.DistutilsTemplateError
UnknownFileError = _distutils_errors.UnknownFileError
# The root error class in the hierarchy
BaseError = _distutils_errors.DistutilsError
class RemovedCommandError(BaseError, RuntimeError):
    """Error used for commands that have been removed in setuptools.
    Since ``setuptools`` is built on ``distutils``, simply removing a command
    from ``setuptools`` will make the behavior fall back to ``distutils``; this
    error is raised if a command exists in ``distutils`` but has been actively
    removed in ``setuptools``.
class PackageDiscoveryError(BaseError, RuntimeError):
    """Impossible to perform automatic discovery of packages and/or modules.
    The current project layout or given discovery options can lead to problems when
    scanning the project directory.
    Setuptools might also refuse to complete auto-discovery if an error prone condition
    is detected (e.g. when a project is organised as a flat-layout but contains
    multiple directories that can be taken as top-level packages inside a single
    distribution [*]_). In these situations the users are encouraged to be explicit
    about which packages to include or to make the discovery parameters more specific.
    .. [*] Since multi-package distributions are uncommon it is very likely that the
       developers did not intend for all the directories to be packaged, and are just
       leaving auxiliary code in the repository top-level, such as maintenance-related
       scripts.
"""distutils.errors
Provides exceptions used by the Distutils modules.  Note that Distutils
modules may raise standard exceptions; in particular, SystemExit is
usually raised for errors that are obviously the end-user's fault
(eg. bad command-line arguments).
This module is safe to use in "from ... import *" mode; it only exports
symbols whose names start with "Distutils" and end with "Error"."""
class DistutilsError(Exception):
    """The root of all Distutils evil."""
class DistutilsModuleError(DistutilsError):
    """Unable to load an expected module, or to find an expected class
    within some module (in particular, command modules and classes)."""
class DistutilsClassError(DistutilsError):
    """Some command class (or possibly distribution class, if anyone
    feels a need to subclass Distribution) is found not to be holding
    up its end of the bargain, ie. implementing some part of the
    "command "interface."""
class DistutilsGetoptError(DistutilsError):
    """The option table provided to 'fancy_getopt()' is bogus."""
class DistutilsArgError(DistutilsError):
    """Raised by fancy_getopt in response to getopt.error -- ie. an
    error in the command line usage."""
class DistutilsFileError(DistutilsError):
    """Any problems in the filesystem: expected file not found, etc.
    Typically this is for problems that we detect before OSError
    could be raised."""
class DistutilsOptionError(DistutilsError):
    """Syntactic/semantic errors in command options, such as use of
    mutually conflicting options, or inconsistent options,
    badly-spelled values, etc.  No distinction is made between option
    values originating in the setup script, the command line, config
    files, or what-have-you -- but if we *know* something originated in
    the setup script, we'll raise DistutilsSetupError instead."""
class DistutilsSetupError(DistutilsError):
    """For errors that can be definitely blamed on the setup script,
    such as invalid keyword arguments to 'setup()'."""
class DistutilsPlatformError(DistutilsError):
    """We don't know how to do something on the current platform (but
    we do know how to do it on some platform) -- eg. trying to compile
    C files on a platform not supported by a CCompiler subclass."""
class DistutilsExecError(DistutilsError):
    """Any problems executing an external program (such as the C
    compiler, when compiling C files)."""
class DistutilsInternalError(DistutilsError):
    """Internal inconsistencies or impossibilities (obviously, this
    should never be seen if the code is working!)."""
class DistutilsTemplateError(DistutilsError):
    """Syntax error in a file list template."""
class DistutilsByteCompileError(DistutilsError):
    """Byte compile error."""
# Exception classes used by the CCompiler implementation classes
class CCompilerError(Exception):
    """Some compile/link operation failed."""
class PreprocessError(CCompilerError):
    """Failure to preprocess one or more C/C++ files."""
class CompileError(CCompilerError):
    """Failure to compile one or more C/C++ source files."""
class LibError(CCompilerError):
    """Failure to create a static library from one or more C/C++ object
    files."""
class LinkError(CCompilerError):
    """Failure to link one or more C/C++ object files into an executable
    or shared library file."""
class UnknownFileError(CCompilerError):
    """Attempt to process an unknown file type."""
class ConsoleError(Exception):
    """An error in console operation."""
class StyleError(Exception):
    """An error in styles."""
class StyleSyntaxError(ConsoleError):
    """Style was badly formatted."""
class MissingStyle(StyleError):
    """No such style."""
class StyleStackError(ConsoleError):
    """Style stack is invalid."""
class NotRenderableError(ConsoleError):
    """Object is not renderable."""
class MarkupError(ConsoleError):
    """Markup was badly formatted."""
class LiveError(ConsoleError):
    """Error related to Live display."""
class NoAltScreen(ConsoleError):
    """Alt screen mode was required."""
from starlette.responses import HTMLResponse, PlainTextResponse, Response
from starlette.types import ASGIApp, ExceptionHandler, Message, Receive, Scope, Send
STYLES = """
    color: #211c1c;
.traceback-container {
    border: 1px solid #038BB8;
.traceback-title {
    background-color: #038BB8;
    color: lemonchiffon;
.frame-line {
.frame-filename {
.center-line {
    color: #f9f6e1;
    padding: 5px 0px 5px 5px;
.lineno {
.frame-title {
    font-weight: unset;
    padding: 10px 10px 10px 10px;
    background-color: #E4F4FD;
    color: #191f21;
    border: 1px solid #c7dce8;
.collapse-btn {
    padding: 0px 5px 1px 5px;
    border: solid 1px #96aebb;
.collapsed {
.source-code {
  font-family: courier;
  font-size: small;
JS = """
    function collapse(element){
        const frameId = element.getAttribute("data-frame-id");
        const frame = document.getElementById(frameId);
        if (frame.classList.contains("collapsed")){
            element.innerHTML = "&#8210;";
            frame.classList.remove("collapsed");
            element.innerHTML = "+";
            frame.classList.add("collapsed");
TEMPLATE = """
        <style type='text/css'>
            {styles}
        <title>Starlette Debugger</title>
        <h1>500 Server Error</h1>
        <h2>{error}</h2>
        <div class="traceback-container">
            <p class="traceback-title">Traceback</p>
            <div>{exc_html}</div>
        {js}
FRAME_TEMPLATE = """
    <p class="frame-title">File <span class="frame-filename">{frame_filename}</span>,
    line <i>{frame_lineno}</i>,
    in <b>{frame_name}</b>
    <span class="collapse-btn" data-frame-id="{frame_filename}-{frame_lineno}" onclick="collapse(this)">{collapse_button}</span>
    <div id="{frame_filename}-{frame_lineno}" class="source-code {collapsed}">{code_context}</div>
LINE = """
<p><span class="frame-line">
<span class="lineno">{lineno}.</span> {line}</span></p>
CENTER_LINE = """
<p class="center-line"><span class="frame-line center-line">
class ServerErrorMiddleware:
    Handles returning 500 responses when a server error occurs.
    If 'debug' is set, then traceback responses will be returned,
    otherwise the designated 'handler' will be called.
    This middleware class should generally be used to wrap *everything*
    else up, so that unhandled exceptions anywhere in the stack
    always result in an appropriate 500 response.
        handler: ExceptionHandler | None = None,
        debug: bool = False,
        self.debug = debug
        response_started = False
        async def _send(message: Message) -> None:
            nonlocal response_started, send
            if message["type"] == "http.response.start":
                response_started = True
            await send(message)
            await self.app(scope, receive, _send)
            request = Request(scope)
                # In debug mode, return traceback responses.
                response = self.debug_response(request, exc)
            elif self.handler is None:
                # Use our default 500 error handler.
                response = self.error_response(request, exc)
                # Use an installed 500 error handler.
                if is_async_callable(self.handler):
                    response = await self.handler(request, exc)
                    response = await run_in_threadpool(self.handler, request, exc)
            if not response_started:
            # We always continue to raise the exception.
            # This allows servers to log the error, or allows test clients
            # to optionally raise the error within the test case.
    def format_line(self, index: int, line: str, frame_lineno: int, frame_index: int) -> str:
        values = {
            # HTML escape - line could contain < or >
            "line": html.escape(line).replace(" ", "&nbsp"),
            "lineno": (frame_lineno - frame_index) + index,
        if index != frame_index:
            return LINE.format(**values)
        return CENTER_LINE.format(**values)
    def generate_frame_html(self, frame: inspect.FrameInfo, is_collapsed: bool) -> str:
        code_context = "".join(
            self.format_line(
                frame.lineno,
                frame.index,  # type: ignore[arg-type]
            for index, line in enumerate(frame.code_context or [])
            # HTML escape - filename could contain < or >, especially if it's a virtual
            # file e.g. <stdin> in the REPL
            "frame_filename": html.escape(frame.filename),
            "frame_lineno": frame.lineno,
            # HTML escape - if you try very hard it's possible to name a function with <
            # or >
            "frame_name": html.escape(frame.function),
            "code_context": code_context,
            "collapsed": "collapsed" if is_collapsed else "",
            "collapse_button": "+" if is_collapsed else "&#8210;",
        return FRAME_TEMPLATE.format(**values)
    def generate_html(self, exc: Exception, limit: int = 7) -> str:
        traceback_obj = traceback.TracebackException.from_exception(exc, capture_locals=True)
        exc_html = ""
        is_collapsed = False
        exc_traceback = exc.__traceback__
        if exc_traceback is not None:
            frames = inspect.getinnerframes(exc_traceback, limit)
            for frame in reversed(frames):
                exc_html += self.generate_frame_html(frame, is_collapsed)
                is_collapsed = True
            exc_type_str = traceback_obj.exc_type_str
            exc_type_str = traceback_obj.exc_type.__name__
        # escape error class and text
        error = f"{html.escape(exc_type_str)}: {html.escape(str(traceback_obj))}"
        return TEMPLATE.format(styles=STYLES, js=JS, error=error, exc_html=exc_html)
    def generate_plain_text(self, exc: Exception) -> str:
        return "".join(traceback.format_exception(type(exc), exc, exc.__traceback__))
    def debug_response(self, request: Request, exc: Exception) -> Response:
        accept = request.headers.get("accept", "")
        if "text/html" in accept:
            content = self.generate_html(exc)
            return HTMLResponse(content, status_code=500)
        content = self.generate_plain_text(exc)
        return PlainTextResponse(content, status_code=500)
    def error_response(self, request: Request, exc: Exception) -> Response:
        return PlainTextResponse("Internal Server Error", status_code=500)
"""Pydantic-specific errors."""
from typing import Any, ClassVar, Literal
from typing_inspection.introspection import Qualifier
from pydantic._internal import _repr
from .version import version_short
# We use this URL to allow for future flexibility about how we host the docs, while allowing for Pydantic
# code in the while with "old" URLs to still work.
# 'u' refers to "user errors" - e.g. errors caused by developers using pydantic, as opposed to validation errors.
DEV_ERROR_DOCS_URL = f'https://errors.pydantic.dev/{version_short()}/u/'
PydanticErrorCodes = Literal[
    'class-not-fully-defined',
    'custom-json-schema',
    'decorator-missing-field',
    'discriminator-no-field',
    'discriminator-alias-type',
    'discriminator-needs-literal',
    'discriminator-alias',
    'discriminator-validator',
    'callable-discriminator-no-tag',
    'typed-dict-version',
    'model-field-overridden',
    'model-field-missing-annotation',
    'config-both',
    'removed-kwargs',
    'circular-reference-schema',
    'invalid-for-json-schema',
    'json-schema-already-used',
    'base-model-instantiated',
    'undefined-annotation',
    'schema-for-unknown-type',
    'import-error',
    'create-model-field-definitions',
    'validator-no-fields',
    'validator-invalid-fields',
    'validator-instance-method',
    'validator-input-type',
    'root-validator-pre-skip',
    'model-serializer-instance-method',
    'validator-field-config-info',
    'validator-v1-signature',
    'validator-signature',
    'field-serializer-signature',
    'model-serializer-signature',
    'multiple-field-serializers',
    'invalid-annotated-type',
    'type-adapter-config-unused',
    'root-model-extra',
    'unevaluable-type-annotation',
    'dataclass-init-false-extra-allow',
    'clashing-init-and-init-var',
    'model-config-invalid-field-name',
    'with-config-on-model',
    'dataclass-on-model',
    'validate-call-type',
    'unpack-typed-dict',
    'overlapping-unpack-typed-dict',
    'invalid-self-type',
    'validate-by-alias-and-name-false',
class PydanticErrorMixin:
    """A mixin class for common functionality shared by all Pydantic-specific errors.
        message: A message describing the error.
        code: An optional error code from PydanticErrorCodes enum.
    def __init__(self, message: str, *, code: PydanticErrorCodes | None) -> None:
        if self.code is None:
            return f'{self.message}\n\nFor further information visit {DEV_ERROR_DOCS_URL}{self.code}'
class PydanticUserError(PydanticErrorMixin, TypeError):
    """An error raised due to incorrect use of Pydantic."""
class PydanticUndefinedAnnotation(PydanticErrorMixin, NameError):
    """A subclass of `NameError` raised when handling undefined annotations during `CoreSchema` generation.
        name: Name of the error.
        message: Description of the error.
    def __init__(self, name: str, message: str) -> None:
        super().__init__(message=message, code='undefined-annotation')
    def from_name_error(cls, name_error: NameError) -> Self:
        """Convert a `NameError` to a `PydanticUndefinedAnnotation` error.
            name_error: `NameError` to be converted.
            Converted `PydanticUndefinedAnnotation` error.
            name = name_error.name  # type: ignore  # python > 3.10
            name = re.search(r".*'(.+?)'", str(name_error)).group(1)  # type: ignore[union-attr]
        return cls(name=name, message=str(name_error))
class PydanticImportError(PydanticErrorMixin, ImportError):
    """An error raised when an import fails due to module changes between V1 and V2.
        super().__init__(message, code='import-error')
class PydanticSchemaGenerationError(PydanticUserError):
    """An error raised during failures to generate a `CoreSchema` for some type.
        super().__init__(message, code='schema-for-unknown-type')
class PydanticInvalidForJsonSchema(PydanticUserError):
    """An error raised during failures to generate a JSON schema for some `CoreSchema`.
        super().__init__(message, code='invalid-for-json-schema')
class PydanticForbiddenQualifier(PydanticUserError):
    """An error raised if a forbidden type qualifier is found in a type annotation."""
    _qualifier_repr_map: ClassVar[dict[Qualifier, str]] = {
        'required': 'typing.Required',
        'not_required': 'typing.NotRequired',
        'read_only': 'typing.ReadOnly',
        'class_var': 'typing.ClassVar',
        'init_var': 'dataclasses.InitVar',
        'final': 'typing.Final',
    def __init__(self, qualifier: Qualifier, annotation: Any) -> None:
                f'The annotation {_repr.display_as_type(annotation)!r} contains the {self._qualifier_repr_map[qualifier]!r} '
                f'type qualifier, which is invalid in the context it is defined.'
            code=None,
from typing import TYPE_CHECKING, Any, Callable, Sequence, Set, Tuple, Type, Union
from pydantic.v1.typing import display_as_type
    from pydantic.v1.typing import DictStrAny
# explicitly state exports to avoid "from pydantic.v1.errors import *" also importing Decimal, Path etc.
    'PydanticTypeError',
    'PydanticValueError',
    'ConfigError',
    'MissingError',
    'ExtraError',
    'NoneIsNotAllowedError',
    'NoneIsAllowedError',
    'WrongConstantError',
    'NotNoneError',
    'BoolError',
    'BytesError',
    'DictError',
    'EmailError',
    'UrlError',
    'UrlSchemeError',
    'UrlSchemePermittedError',
    'UrlUserInfoError',
    'UrlHostError',
    'UrlHostTldError',
    'UrlPortError',
    'UrlExtraError',
    'EnumError',
    'IntEnumError',
    'EnumMemberError',
    'IntegerError',
    'FloatError',
    'PathError',
    'PathNotExistsError',
    'PathNotAFileError',
    'PathNotADirectoryError',
    'PyObjectError',
    'SequenceError',
    'ListError',
    'SetError',
    'FrozenSetError',
    'TupleError',
    'TupleLengthError',
    'ListMinLengthError',
    'ListMaxLengthError',
    'ListUniqueItemsError',
    'SetMinLengthError',
    'SetMaxLengthError',
    'FrozenSetMinLengthError',
    'FrozenSetMaxLengthError',
    'AnyStrMinLengthError',
    'AnyStrMaxLengthError',
    'StrError',
    'StrRegexError',
    'NumberNotGtError',
    'NumberNotGeError',
    'NumberNotLtError',
    'NumberNotLeError',
    'NumberNotMultipleError',
    'DecimalError',
    'DecimalIsNotFiniteError',
    'DecimalMaxDigitsError',
    'DecimalMaxPlacesError',
    'DecimalWholeDigitsError',
    'DateTimeError',
    'DateError',
    'DateNotInThePastError',
    'DateNotInTheFutureError',
    'TimeError',
    'DurationError',
    'HashableError',
    'UUIDError',
    'UUIDVersionError',
    'ArbitraryTypeError',
    'ClassError',
    'SubclassError',
    'JsonError',
    'JsonTypeError',
    'PatternError',
    'DataclassTypeError',
    'CallableError',
    'IPvAnyAddressError',
    'IPvAnyInterfaceError',
    'IPvAnyNetworkError',
    'IPv4AddressError',
    'IPv6AddressError',
    'IPv4NetworkError',
    'IPv6NetworkError',
    'IPv4InterfaceError',
    'IPv6InterfaceError',
    'ColorError',
    'StrictBoolError',
    'NotDigitError',
    'LuhnValidationError',
    'InvalidLengthForBrand',
    'InvalidByteSize',
    'InvalidByteSizeUnit',
    'MissingDiscriminator',
    'InvalidDiscriminator',
def cls_kwargs(cls: Type['PydanticErrorMixin'], ctx: 'DictStrAny') -> 'PydanticErrorMixin':
    For built-in exceptions like ValueError or TypeError, we need to implement
    __reduce__ to override the default behaviour (instead of __getstate__/__setstate__)
    By default pickle protocol 2 calls `cls.__new__(cls, *args)`.
    Since we only use kwargs, we need a little constructor to change that.
    Note: the callable can't be a lambda as pickle looks in the namespace to find it
    return cls(**ctx)
    msg_template: str
    def __init__(self, **ctx: Any) -> None:
        self.__dict__ = ctx
        return self.msg_template.format(**self.__dict__)
    def __reduce__(self) -> Tuple[Callable[..., 'PydanticErrorMixin'], Tuple[Type['PydanticErrorMixin'], 'DictStrAny']]:
        return cls_kwargs, (self.__class__, self.__dict__)
class PydanticTypeError(PydanticErrorMixin, TypeError):
class PydanticValueError(PydanticErrorMixin, ValueError):
class ConfigError(RuntimeError):
class MissingError(PydanticValueError):
    msg_template = 'field required'
class ExtraError(PydanticValueError):
    msg_template = 'extra fields not permitted'
class NoneIsNotAllowedError(PydanticTypeError):
    code = 'none.not_allowed'
    msg_template = 'none is not an allowed value'
class NoneIsAllowedError(PydanticTypeError):
    code = 'none.allowed'
    msg_template = 'value is not none'
class WrongConstantError(PydanticValueError):
    code = 'const'
        permitted = ', '.join(repr(v) for v in self.permitted)  # type: ignore
        return f'unexpected value; permitted: {permitted}'
class NotNoneError(PydanticTypeError):
    code = 'not_none'
    msg_template = 'value is not None'
class BoolError(PydanticTypeError):
    msg_template = 'value could not be parsed to a boolean'
class BytesError(PydanticTypeError):
    msg_template = 'byte type expected'
class DictError(PydanticTypeError):
    msg_template = 'value is not a valid dict'
class EmailError(PydanticValueError):
    msg_template = 'value is not a valid email address'
class UrlError(PydanticValueError):
    code = 'url'
class UrlSchemeError(UrlError):
    code = 'url.scheme'
    msg_template = 'invalid or missing URL scheme'
class UrlSchemePermittedError(UrlError):
    msg_template = 'URL scheme not permitted'
    def __init__(self, allowed_schemes: Set[str]):
        super().__init__(allowed_schemes=allowed_schemes)
class UrlUserInfoError(UrlError):
    code = 'url.userinfo'
    msg_template = 'userinfo required in URL but missing'
class UrlHostError(UrlError):
    code = 'url.host'
    msg_template = 'URL host invalid'
class UrlHostTldError(UrlError):
    msg_template = 'URL host invalid, top level domain required'
class UrlPortError(UrlError):
    code = 'url.port'
    msg_template = 'URL port invalid, port cannot exceed 65535'
class UrlExtraError(UrlError):
    code = 'url.extra'
    msg_template = 'URL invalid, extra characters found after valid URL: {extra!r}'
class EnumMemberError(PydanticTypeError):
    code = 'enum'
        permitted = ', '.join(repr(v.value) for v in self.enum_values)  # type: ignore
        return f'value is not a valid enumeration member; permitted: {permitted}'
class IntegerError(PydanticTypeError):
    msg_template = 'value is not a valid integer'
class FloatError(PydanticTypeError):
    msg_template = 'value is not a valid float'
class PathError(PydanticTypeError):
    msg_template = 'value is not a valid path'
class _PathValueError(PydanticValueError):
    def __init__(self, *, path: Path) -> None:
        super().__init__(path=str(path))
class PathNotExistsError(_PathValueError):
    code = 'path.not_exists'
    msg_template = 'file or directory at path "{path}" does not exist'
class PathNotAFileError(_PathValueError):
    code = 'path.not_a_file'
    msg_template = 'path "{path}" does not point to a file'
class PathNotADirectoryError(_PathValueError):
    code = 'path.not_a_directory'
    msg_template = 'path "{path}" does not point to a directory'
class PyObjectError(PydanticTypeError):
    msg_template = 'ensure this value contains valid import path or valid callable: {error_message}'
class SequenceError(PydanticTypeError):
    msg_template = 'value is not a valid sequence'
class IterableError(PydanticTypeError):
    msg_template = 'value is not a valid iterable'
class ListError(PydanticTypeError):
    msg_template = 'value is not a valid list'
class SetError(PydanticTypeError):
    msg_template = 'value is not a valid set'
class FrozenSetError(PydanticTypeError):
    msg_template = 'value is not a valid frozenset'
class DequeError(PydanticTypeError):
    msg_template = 'value is not a valid deque'
class TupleError(PydanticTypeError):
    msg_template = 'value is not a valid tuple'
class TupleLengthError(PydanticValueError):
    code = 'tuple.length'
    msg_template = 'wrong tuple length {actual_length}, expected {expected_length}'
    def __init__(self, *, actual_length: int, expected_length: int) -> None:
        super().__init__(actual_length=actual_length, expected_length=expected_length)
class ListMinLengthError(PydanticValueError):
    code = 'list.min_items'
    msg_template = 'ensure this value has at least {limit_value} items'
    def __init__(self, *, limit_value: int) -> None:
        super().__init__(limit_value=limit_value)
class ListMaxLengthError(PydanticValueError):
    code = 'list.max_items'
    msg_template = 'ensure this value has at most {limit_value} items'
class ListUniqueItemsError(PydanticValueError):
    code = 'list.unique_items'
    msg_template = 'the list has duplicated items'
class SetMinLengthError(PydanticValueError):
    code = 'set.min_items'
class SetMaxLengthError(PydanticValueError):
    code = 'set.max_items'
class FrozenSetMinLengthError(PydanticValueError):
    code = 'frozenset.min_items'
class FrozenSetMaxLengthError(PydanticValueError):
    code = 'frozenset.max_items'
class AnyStrMinLengthError(PydanticValueError):
    code = 'any_str.min_length'
    msg_template = 'ensure this value has at least {limit_value} characters'
class AnyStrMaxLengthError(PydanticValueError):
    code = 'any_str.max_length'
    msg_template = 'ensure this value has at most {limit_value} characters'
class StrError(PydanticTypeError):
    msg_template = 'str type expected'
class StrRegexError(PydanticValueError):
    code = 'str.regex'
    msg_template = 'string does not match regex "{pattern}"'
    def __init__(self, *, pattern: str) -> None:
        super().__init__(pattern=pattern)
class _NumberBoundError(PydanticValueError):
    def __init__(self, *, limit_value: Union[int, float, Decimal]) -> None:
class NumberNotGtError(_NumberBoundError):
    code = 'number.not_gt'
    msg_template = 'ensure this value is greater than {limit_value}'
class NumberNotGeError(_NumberBoundError):
    code = 'number.not_ge'
    msg_template = 'ensure this value is greater than or equal to {limit_value}'
class NumberNotLtError(_NumberBoundError):
    code = 'number.not_lt'
    msg_template = 'ensure this value is less than {limit_value}'
class NumberNotLeError(_NumberBoundError):
    code = 'number.not_le'
    msg_template = 'ensure this value is less than or equal to {limit_value}'
class NumberNotFiniteError(PydanticValueError):
    code = 'number.not_finite_number'
    msg_template = 'ensure this value is a finite number'
class NumberNotMultipleError(PydanticValueError):
    code = 'number.not_multiple'
    msg_template = 'ensure this value is a multiple of {multiple_of}'
    def __init__(self, *, multiple_of: Union[int, float, Decimal]) -> None:
        super().__init__(multiple_of=multiple_of)
class DecimalError(PydanticTypeError):
    msg_template = 'value is not a valid decimal'
class DecimalIsNotFiniteError(PydanticValueError):
    code = 'decimal.not_finite'
class DecimalMaxDigitsError(PydanticValueError):
    code = 'decimal.max_digits'
    msg_template = 'ensure that there are no more than {max_digits} digits in total'
    def __init__(self, *, max_digits: int) -> None:
        super().__init__(max_digits=max_digits)
class DecimalMaxPlacesError(PydanticValueError):
    code = 'decimal.max_places'
    msg_template = 'ensure that there are no more than {decimal_places} decimal places'
    def __init__(self, *, decimal_places: int) -> None:
        super().__init__(decimal_places=decimal_places)
class DecimalWholeDigitsError(PydanticValueError):
    code = 'decimal.whole_digits'
    msg_template = 'ensure that there are no more than {whole_digits} digits before the decimal point'
    def __init__(self, *, whole_digits: int) -> None:
        super().__init__(whole_digits=whole_digits)
class DateTimeError(PydanticValueError):
    msg_template = 'invalid datetime format'
class DateError(PydanticValueError):
    msg_template = 'invalid date format'
class DateNotInThePastError(PydanticValueError):
    code = 'date.not_in_the_past'
    msg_template = 'date is not in the past'
class DateNotInTheFutureError(PydanticValueError):
    code = 'date.not_in_the_future'
    msg_template = 'date is not in the future'
class TimeError(PydanticValueError):
    msg_template = 'invalid time format'
class DurationError(PydanticValueError):
    msg_template = 'invalid duration format'
class HashableError(PydanticTypeError):
    msg_template = 'value is not a valid hashable'
class UUIDError(PydanticTypeError):
    msg_template = 'value is not a valid uuid'
class UUIDVersionError(PydanticValueError):
    code = 'uuid.version'
    msg_template = 'uuid version {required_version} expected'
    def __init__(self, *, required_version: int) -> None:
        super().__init__(required_version=required_version)
class ArbitraryTypeError(PydanticTypeError):
    code = 'arbitrary_type'
    msg_template = 'instance of {expected_arbitrary_type} expected'
    def __init__(self, *, expected_arbitrary_type: Type[Any]) -> None:
        super().__init__(expected_arbitrary_type=display_as_type(expected_arbitrary_type))
class ClassError(PydanticTypeError):
    code = 'class'
    msg_template = 'a class is expected'
class SubclassError(PydanticTypeError):
    code = 'subclass'
    msg_template = 'subclass of {expected_class} expected'
    def __init__(self, *, expected_class: Type[Any]) -> None:
        super().__init__(expected_class=display_as_type(expected_class))
class JsonError(PydanticValueError):
    msg_template = 'Invalid JSON'
class JsonTypeError(PydanticTypeError):
    code = 'json'
    msg_template = 'JSON object must be str, bytes or bytearray'
class PatternError(PydanticValueError):
    code = 'regex_pattern'
    msg_template = 'Invalid regular expression'
class DataclassTypeError(PydanticTypeError):
    code = 'dataclass'
    msg_template = 'instance of {class_name}, tuple or dict expected'
class CallableError(PydanticTypeError):
    msg_template = '{value} is not callable'
class EnumError(PydanticTypeError):
    code = 'enum_instance'
    msg_template = '{value} is not a valid Enum instance'
class IntEnumError(PydanticTypeError):
    code = 'int_enum_instance'
    msg_template = '{value} is not a valid IntEnum instance'
class IPvAnyAddressError(PydanticValueError):
    msg_template = 'value is not a valid IPv4 or IPv6 address'
class IPvAnyInterfaceError(PydanticValueError):
    msg_template = 'value is not a valid IPv4 or IPv6 interface'
class IPvAnyNetworkError(PydanticValueError):
    msg_template = 'value is not a valid IPv4 or IPv6 network'
class IPv4AddressError(PydanticValueError):
    msg_template = 'value is not a valid IPv4 address'
class IPv6AddressError(PydanticValueError):
    msg_template = 'value is not a valid IPv6 address'
class IPv4NetworkError(PydanticValueError):
    msg_template = 'value is not a valid IPv4 network'
class IPv6NetworkError(PydanticValueError):
    msg_template = 'value is not a valid IPv6 network'
class IPv4InterfaceError(PydanticValueError):
    msg_template = 'value is not a valid IPv4 interface'
class IPv6InterfaceError(PydanticValueError):
    msg_template = 'value is not a valid IPv6 interface'
class ColorError(PydanticValueError):
    msg_template = 'value is not a valid color: {reason}'
class StrictBoolError(PydanticValueError):
    msg_template = 'value is not a valid boolean'
class NotDigitError(PydanticValueError):
    code = 'payment_card_number.digits'
    msg_template = 'card number is not all digits'
class LuhnValidationError(PydanticValueError):
    code = 'payment_card_number.luhn_check'
    msg_template = 'card number is not luhn valid'
class InvalidLengthForBrand(PydanticValueError):
    code = 'payment_card_number.invalid_length_for_brand'
    msg_template = 'Length for a {brand} card must be {required_length}'
class InvalidByteSize(PydanticValueError):
    msg_template = 'could not parse value and unit from byte string'
class InvalidByteSizeUnit(PydanticValueError):
    msg_template = 'could not interpret byte unit: {unit}'
class MissingDiscriminator(PydanticValueError):
    code = 'discriminated_union.missing_discriminator'
    msg_template = 'Discriminator {discriminator_key!r} is missing in value'
class InvalidDiscriminator(PydanticValueError):
    code = 'discriminated_union.invalid_discriminator'
    msg_template = (
        'No match for discriminator {discriminator_key!r} and value {discriminator_value!r} '
        '(allowed values: {allowed_values})'
    def __init__(self, *, discriminator_key: str, discriminator_value: Any, allowed_values: Sequence[Any]) -> None:
            discriminator_key=discriminator_key,
            discriminator_value=discriminator_value,
            allowed_values=', '.join(map(repr, allowed_values)),
"""Contains all custom errors."""
from httpx import HTTPError, Response
# CACHE ERRORS
class CacheNotFound(Exception):
    """Exception thrown when the Huggingface cache is not found."""
    cache_dir: Union[str, Path]
    def __init__(self, msg: str, cache_dir: Union[str, Path], *args, **kwargs):
        super().__init__(msg, *args, **kwargs)
class CorruptedCacheException(Exception):
    """Exception for any unexpected structure in the Huggingface cache-system."""
# HEADERS ERRORS
class LocalTokenNotFoundError(EnvironmentError):
    """Raised if local token is required but not found."""
# HTTP ERRORS
class OfflineModeIsEnabled(ConnectionError):
    """Raised when a request is made but `HF_HUB_OFFLINE=1` is set as environment variable."""
class HfHubHTTPError(HTTPError, OSError):
    HTTPError to inherit from for any custom HTTP Error raised in HF Hub.
    Any HTTPError is converted at least into a `HfHubHTTPError`. If some information is
    sent back by the server, it will be added to the error message.
    Added details:
    - Request ID sourced from headers in order of precedence: "X-Request-Id", "X-Amzn-Trace-Id", "X-Amz-Cf-Id".
    - Server error message from the header "X-Error-Message".
    - Server error message if we can found one in the response body.
        from huggingface_hub.utils import get_session, hf_raise_for_status, HfHubHTTPError
        response = get_session().post(...)
            hf_raise_for_status(response)
        except HfHubHTTPError as e:
            print(str(e)) # formatted message
            e.request_id, e.server_message # details returned by server
            # Complete the error message with additional information once it's raised
            e.append_to_message("\n`create_commit` expects the repository to exist.")
        server_message: Optional[str] = None,
        self.request_id = (
            response.headers.get("x-request-id")
            or response.headers.get("X-Amzn-Trace-Id")
            or response.headers.get("x-amz-cf-id")
        self.server_message = server_message
        self.request = response.request
    def append_to_message(self, additional_message: str) -> None:
        """Append additional information to the `HfHubHTTPError` initial message."""
        self.args = (self.args[0] + additional_message,) + self.args[1:]
    def _reconstruct_hf_hub_http_error(
        cls, message: str, response: Response, server_message: Optional[str]
    ) -> "HfHubHTTPError":
        return cls(message, response=response, server_message=server_message)
    def __reduce_ex__(self, protocol):
        """Fix pickling of Exception subclass with kwargs. We need to override __reduce_ex__ of the parent class"""
        return (self.__class__._reconstruct_hf_hub_http_error, (str(self), self.response, self.server_message))
# INFERENCE CLIENT ERRORS
class InferenceTimeoutError(HTTPError, TimeoutError):
    """Error raised when a model is unavailable or the request times out."""
# INFERENCE ENDPOINT ERRORS
class InferenceEndpointError(Exception):
    """Generic exception when dealing with Inference Endpoints."""
class InferenceEndpointTimeoutError(InferenceEndpointError, TimeoutError):
    """Exception for timeouts while waiting for Inference Endpoint."""
# SAFETENSORS ERRORS
class SafetensorsParsingError(Exception):
    """Raised when failing to parse a safetensors file metadata.
    This can be the case if the file is not a safetensors file or does not respect the specification.
class NotASafetensorsRepoError(Exception):
    """Raised when a repo is not a Safetensors repo i.e. doesn't have either a `model.safetensors` or a
    `model.safetensors.index.json` file.
# TEXT GENERATION ERRORS
class TextGenerationError(HTTPError):
    """Generic error raised if text-generation went wrong."""
# Text Generation Inference Errors
class ValidationError(TextGenerationError):
    """Server-side validation error."""
class GenerationError(TextGenerationError):
class OverloadedError(TextGenerationError):
class IncompleteGenerationError(TextGenerationError):
class UnknownError(TextGenerationError):
# VALIDATION ERRORS
class HFValidationError(ValueError):
    """Generic exception thrown by `huggingface_hub` validators.
    Inherits from [`ValueError`](https://docs.python.org/3/library/exceptions.html#ValueError).
# FILE METADATA ERRORS
class DryRunError(OSError):
    """Error triggered when a dry run is requested but cannot be performed (e.g. invalid repo)."""
class FileMetadataError(OSError):
    """Error triggered when the metadata of a file on the Hub cannot be retrieved (missing ETag or commit_hash).
    Inherits from `OSError` for backward compatibility.
# REPOSITORY ERRORS
class RepositoryNotFoundError(HfHubHTTPError):
    Raised when trying to access a hf.co URL with an invalid repository name, or
    with a private repo name the user does not have access to.
    >>> from huggingface_hub import model_info
    >>> model_info("<non_existent_repository>")
    (...)
    huggingface_hub.errors.RepositoryNotFoundError: 401 Client Error. (Request ID: PvMw_VjBMjVdMz53WKIzP)
    Repository Not Found for url: https://huggingface.co/api/models/%3Cnon_existent_repository%3E.
    Please make sure you specified the correct `repo_id` and `repo_type`.
    If the repo is private, make sure you are authenticated.
    Invalid username or password.
class GatedRepoError(RepositoryNotFoundError):
    Raised when trying to access a gated repository for which the user is not on the
    authorized list.
    Note: derives from `RepositoryNotFoundError` to ensure backward compatibility.
    >>> model_info("<gated_repository>")
    huggingface_hub.errors.GatedRepoError: 403 Client Error. (Request ID: ViT1Bf7O_026LGSQuVqfa)
    Cannot access gated repo for url https://huggingface.co/api/models/ardent-figment/gated-model.
    Access to model ardent-figment/gated-model is restricted and you are not in the authorized list.
    Visit https://huggingface.co/ardent-figment/gated-model to ask for access.
class DisabledRepoError(HfHubHTTPError):
    Raised when trying to access a repository that has been disabled by its author.
    >>> from huggingface_hub import dataset_info
    >>> dataset_info("laion/laion-art")
    huggingface_hub.errors.DisabledRepoError: 403 Client Error. (Request ID: Root=1-659fc3fa-3031673e0f92c71a2260dbe2;bc6f4dfb-b30a-4862-af0a-5cfe827610d8)
    Cannot access repository for url https://huggingface.co/api/datasets/laion/laion-art.
    Access to this resource is disabled.
# REVISION ERROR
class RevisionNotFoundError(HfHubHTTPError):
    Raised when trying to access a hf.co URL with a valid repository but an invalid
    revision.
    >>> from huggingface_hub import hf_hub_download
    >>> hf_hub_download('bert-base-cased', 'config.json', revision='<non-existent-revision>')
    huggingface_hub.errors.RevisionNotFoundError: 404 Client Error. (Request ID: Mwhe_c3Kt650GcdKEFomX)
    Revision Not Found for url: https://huggingface.co/bert-base-cased/resolve/%3Cnon-existent-revision%3E/config.json.
# ENTRY ERRORS
class EntryNotFoundError(Exception):
    Raised when entry not found, either locally or remotely.
    >>> hf_hub_download('bert-base-cased', '<non-existent-file>')
    huggingface_hub.errors.RemoteEntryNotFoundError (...)
    >>> hf_hub_download('bert-base-cased', '<non-existent-file>', local_files_only=True)
    huggingface_hub.utils.errors.LocalEntryNotFoundError (...)
class RemoteEntryNotFoundError(HfHubHTTPError, EntryNotFoundError):
    Raised when trying to access a hf.co URL with a valid repository and revision
    but an invalid filename.
    huggingface_hub.errors.EntryNotFoundError: 404 Client Error. (Request ID: 53pNl6M0MxsnG5Sw8JA6x)
    Entry Not Found for url: https://huggingface.co/bert-base-cased/resolve/main/%3Cnon-existent-file%3E.
class LocalEntryNotFoundError(FileNotFoundError, EntryNotFoundError):
    Raised when trying to access a file or snapshot that is not on the disk when network is
    disabled or unavailable (connection issue). The entry may exist on the Hub.
    >>> hf_hub_download('bert-base-cased', '<non-cached-file>',  local_files_only=True)
    huggingface_hub.errors.LocalEntryNotFoundError: Cannot find the requested files in the disk cache and outgoing traffic has been disabled. To enable hf.co look-ups and downloads online, set 'local_files_only' to False.
    def __init__(self, message: str):
# REQUEST ERROR
class BadRequestError(HfHubHTTPError, ValueError):
    Raised by `hf_raise_for_status` when the server returns a HTTP 400 error.
    >>> resp = httpx.post("hf.co/api/check", ...)
    >>> hf_raise_for_status(resp, endpoint_name="check")
    huggingface_hub.errors.BadRequestError: Bad request for check endpoint: {details} (Request ID: XXX)
# DDUF file format ERROR
class DDUFError(Exception):
    """Base exception for errors related to the DDUF format."""
class DDUFCorruptedFileError(DDUFError):
    """Exception thrown when the DDUF file is corrupted."""
class DDUFExportError(DDUFError):
    """Base exception for errors during DDUF export."""
class DDUFInvalidEntryNameError(DDUFExportError):
    """Exception thrown when the entry name is invalid."""
# STRICT DATACLASSES ERRORS
class StrictDataclassError(Exception):
    """Base exception for strict dataclasses."""
class StrictDataclassDefinitionError(StrictDataclassError):
    """Exception thrown when a strict dataclass is defined incorrectly."""
class StrictDataclassFieldValidationError(StrictDataclassError):
    """Exception thrown when a strict dataclass fails validation for a given field."""
    def __init__(self, field: str, cause: Exception):
        error_message = f"Validation error for field '{field}':"
        error_message += f"\n    {cause.__class__.__name__}: {cause}"
        super().__init__(error_message)
class StrictDataclassClassValidationError(StrictDataclassError):
    """Exception thrown when a strict dataclass fails validation on a class validator."""
    def __init__(self, validator: str, cause: Exception):
        error_message = f"Class validation error for validator '{validator}':"
# XET ERRORS
class XetError(Exception):
    """Base exception for errors related to Xet Storage."""
class XetAuthorizationError(XetError):
    """Exception thrown when the user does not have the right authorization to use Xet Storage."""
class XetRefreshTokenError(XetError):
    """Exception thrown when the refresh token is invalid."""
class XetDownloadError(Exception):
    """Exception thrown when the download from Xet Storage fails."""
# CLI ERRORS
class CLIError(Exception):
    """CLI error with clean message (no traceback by default)."""
