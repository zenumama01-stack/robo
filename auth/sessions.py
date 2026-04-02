from .... import _legacy_response
from ...._types import Body, Omit, Query, Headers, NotGiven, omit, not_given
from ...._utils import path_template, maybe_transform, async_maybe_transform
from ...._response import to_streamed_response_wrapper, async_to_streamed_response_wrapper
from ...._base_client import make_request_options
from ....types.beta.chatkit import (
    ChatSessionWorkflowParam,
    ChatSessionRateLimitsParam,
    ChatSessionExpiresAfterParam,
    ChatSessionChatKitConfigurationParam,
    session_create_params,
from ....types.beta.chatkit.chat_session import ChatSession
from ....types.beta.chatkit.chat_session_workflow_param import ChatSessionWorkflowParam
from ....types.beta.chatkit.chat_session_rate_limits_param import ChatSessionRateLimitsParam
from ....types.beta.chatkit.chat_session_expires_after_param import ChatSessionExpiresAfterParam
from ....types.beta.chatkit.chat_session_chatkit_configuration_param import ChatSessionChatKitConfigurationParam
__all__ = ["Sessions", "AsyncSessions"]
class Sessions(SyncAPIResource):
    def with_raw_response(self) -> SessionsWithRawResponse:
        return SessionsWithRawResponse(self)
    def with_streaming_response(self) -> SessionsWithStreamingResponse:
        return SessionsWithStreamingResponse(self)
        user: str,
        workflow: ChatSessionWorkflowParam,
        chatkit_configuration: ChatSessionChatKitConfigurationParam | Omit = omit,
        expires_after: ChatSessionExpiresAfterParam | Omit = omit,
        rate_limits: ChatSessionRateLimitsParam | Omit = omit,
    ) -> ChatSession:
        Create a ChatKit session.
          user: A free-form string that identifies your end user; ensures this Session can
              access other objects that have the same `user` scope.
          workflow: Workflow that powers the session.
          chatkit_configuration: Optional overrides for ChatKit runtime configuration features
          expires_after: Optional override for session expiration timing in seconds from creation.
              Defaults to 10 minutes.
          rate_limits: Optional override for per-minute request limits. When omitted, defaults to 10.
        extra_headers = {"OpenAI-Beta": "chatkit_beta=v1", **(extra_headers or {})}
            "/chatkit/sessions",
                    "workflow": workflow,
                    "chatkit_configuration": chatkit_configuration,
                    "rate_limits": rate_limits,
                session_create_params.SessionCreateParams,
            cast_to=ChatSession,
        session_id: str,
        Cancel an active ChatKit session and return its most recent metadata.
        Cancelling prevents new requests from using the issued client secret.
        if not session_id:
            raise ValueError(f"Expected a non-empty value for `session_id` but received {session_id!r}")
            path_template("/chatkit/sessions/{session_id}/cancel", session_id=session_id),
class AsyncSessions(AsyncAPIResource):
    def with_raw_response(self) -> AsyncSessionsWithRawResponse:
        return AsyncSessionsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncSessionsWithStreamingResponse:
        return AsyncSessionsWithStreamingResponse(self)
class SessionsWithRawResponse:
    def __init__(self, sessions: Sessions) -> None:
        self._sessions = sessions
            sessions.create,
            sessions.cancel,
class AsyncSessionsWithRawResponse:
    def __init__(self, sessions: AsyncSessions) -> None:
class SessionsWithStreamingResponse:
class AsyncSessionsWithStreamingResponse:
from typing import List, Union, Iterable
from ...._types import NOT_GIVEN, Body, Query, Headers, NotGiven
from ...._utils import maybe_transform, async_maybe_transform
from ....types.beta.realtime import session_create_params
from ....types.beta.realtime.session_create_response import SessionCreateResponse
        client_secret: session_create_params.ClientSecret | NotGiven = NOT_GIVEN,
        input_audio_format: Literal["pcm16", "g711_ulaw", "g711_alaw"] | NotGiven = NOT_GIVEN,
        input_audio_noise_reduction: session_create_params.InputAudioNoiseReduction | NotGiven = NOT_GIVEN,
        input_audio_transcription: session_create_params.InputAudioTranscription | NotGiven = NOT_GIVEN,
        instructions: str | NotGiven = NOT_GIVEN,
        max_response_output_tokens: Union[int, Literal["inf"]] | NotGiven = NOT_GIVEN,
        modalities: List[Literal["text", "audio"]] | NotGiven = NOT_GIVEN,
        model: Literal[
            "gpt-realtime",
            "gpt-realtime-2025-08-28",
            "gpt-4o-realtime-preview",
            "gpt-4o-realtime-preview-2024-10-01",
            "gpt-4o-realtime-preview-2024-12-17",
            "gpt-4o-realtime-preview-2025-06-03",
            "gpt-4o-mini-realtime-preview",
            "gpt-4o-mini-realtime-preview-2024-12-17",
        | NotGiven = NOT_GIVEN,
        output_audio_format: Literal["pcm16", "g711_ulaw", "g711_alaw"] | NotGiven = NOT_GIVEN,
        speed: float | NotGiven = NOT_GIVEN,
        temperature: float | NotGiven = NOT_GIVEN,
        tool_choice: str | NotGiven = NOT_GIVEN,
        tools: Iterable[session_create_params.Tool] | NotGiven = NOT_GIVEN,
        tracing: session_create_params.Tracing | NotGiven = NOT_GIVEN,
        turn_detection: session_create_params.TurnDetection | NotGiven = NOT_GIVEN,
        voice: Union[str, Literal["alloy", "ash", "ballad", "coral", "echo", "sage", "shimmer", "verse"]]
        timeout: float | httpx.Timeout | None | NotGiven = NOT_GIVEN,
    ) -> SessionCreateResponse:
        Create an ephemeral API token for use in client-side applications with the
        Realtime API. Can be configured with the same session parameters as the
        `session.update` client event.
        It responds with a session object, plus a `client_secret` key which contains a
        usable ephemeral API token that can be used to authenticate browser clients for
        the Realtime API.
          client_secret: Configuration options for the generated client secret.
          input_audio_format: The format of input audio. Options are `pcm16`, `g711_ulaw`, or `g711_alaw`. For
              `pcm16`, input audio must be 16-bit PCM at a 24kHz sample rate, single channel
              (mono), and little-endian byte order.
          input_audio_noise_reduction: Configuration for input audio noise reduction. This can be set to `null` to turn
              off. Noise reduction filters audio added to the input audio buffer before it is
              sent to VAD and the model. Filtering the audio can improve VAD and turn
              detection accuracy (reducing false positives) and model performance by improving
              perception of the input audio.
          input_audio_transcription: Configuration for input audio transcription, defaults to off and can be set to
              `null` to turn off once on. Input audio transcription is not native to the
              model, since the model consumes audio directly. Transcription runs
              asynchronously through
              [the /audio/transcriptions endpoint](https://platform.openai.com/docs/api-reference/audio/createTranscription)
              and should be treated as guidance of input audio content rather than precisely
              what the model heard. The client can optionally set the language and prompt for
              transcription, these offer additional guidance to the transcription service.
          instructions: The default system instructions (i.e. system message) prepended to model calls.
              This field allows the client to guide the model on desired responses. The model
              can be instructed on response content and format, (e.g. "be extremely succinct",
              "act friendly", "here are examples of good responses") and on audio behavior
              (e.g. "talk quickly", "inject emotion into your voice", "laugh frequently"). The
              instructions are not guaranteed to be followed by the model, but they provide
              guidance to the model on the desired behavior.
              Note that the server sets default instructions which will be used if this field
              is not set and are visible in the `session.created` event at the start of the
              session.
          max_response_output_tokens: Maximum number of output tokens for a single assistant response, inclusive of
              tool calls. Provide an integer between 1 and 4096 to limit output tokens, or
              `inf` for the maximum available tokens for a given model. Defaults to `inf`.
          modalities: The set of modalities the model can respond with. To disable audio, set this to
              ["text"].
          model: The Realtime model used for this session.
          output_audio_format: The format of output audio. Options are `pcm16`, `g711_ulaw`, or `g711_alaw`.
              For `pcm16`, output audio is sampled at a rate of 24kHz.
          speed: The speed of the model's spoken response. 1.0 is the default speed. 0.25 is the
              minimum speed. 1.5 is the maximum speed. This value can only be changed in
              between model turns, not while a response is in progress.
          temperature: Sampling temperature for the model, limited to [0.6, 1.2]. For audio models a
              temperature of 0.8 is highly recommended for best performance.
          tool_choice: How the model chooses tools. Options are `auto`, `none`, `required`, or specify
              a function.
          tools: Tools (functions) available to the model.
          tracing: Configuration options for tracing. Set to null to disable tracing. Once tracing
              is enabled for a session, the configuration cannot be modified.
              `auto` will create a trace for the session with default values for the workflow
              name, group id, and metadata.
          turn_detection: Configuration for turn detection, ether Server VAD or Semantic VAD. This can be
              set to `null` to turn off, in which case the client must manually trigger model
              response. Server VAD means that the model will detect the start and end of
              speech based on audio volume and respond at the end of user speech. Semantic VAD
              is more advanced and uses a turn detection model (in conjunction with VAD) to
              semantically estimate whether the user has finished speaking, then dynamically
              sets a timeout based on this probability. For example, if user audio trails off
              with "uhhm", the model will score a low probability of turn end and wait longer
              for the user to continue speaking. This can be useful for more natural
              conversations, but may have a higher latency.
          voice: The voice the model uses to respond. Voice cannot be changed during the session
              once the model has responded with audio at least once. Current voice options are
              `alloy`, `ash`, `ballad`, `coral`, `echo`, `sage`, `shimmer`, and `verse`.
            "/realtime/sessions",
                    "client_secret": client_secret,
                    "input_audio_format": input_audio_format,
                    "input_audio_noise_reduction": input_audio_noise_reduction,
                    "input_audio_transcription": input_audio_transcription,
                    "max_response_output_tokens": max_response_output_tokens,
                    "modalities": modalities,
                    "output_audio_format": output_audio_format,
                    "tool_choice": tool_choice,
                    "tracing": tracing,
                    "turn_detection": turn_detection,
            cast_to=SessionCreateResponse,
from django.core.checks import Tags, Warning, register
def add_session_cookie_message(message):
    return message + (
        " Using a secure-only session cookie makes it more difficult for "
        "network traffic sniffers to hijack user sessions."
W010 = Warning(
    add_session_cookie_message(
        "You have 'django.contrib.sessions' in your INSTALLED_APPS, "
        "but you have not set SESSION_COOKIE_SECURE to True."
    id="security.W010",
W011 = Warning(
        "You have 'django.contrib.sessions.middleware.SessionMiddleware' "
        "in your MIDDLEWARE, but you have not set "
        "SESSION_COOKIE_SECURE to True."
    id="security.W011",
W012 = Warning(
    add_session_cookie_message("SESSION_COOKIE_SECURE is not set to True."),
    id="security.W012",
def add_httponly_message(message):
        " Using an HttpOnly session cookie makes it more difficult for "
        "cross-site scripting attacks to hijack user sessions."
W013 = Warning(
    add_httponly_message(
        "but you have not set SESSION_COOKIE_HTTPONLY to True.",
    id="security.W013",
W014 = Warning(
        "SESSION_COOKIE_HTTPONLY to True."
    id="security.W014",
W015 = Warning(
    add_httponly_message("SESSION_COOKIE_HTTPONLY is not set to True."),
    id="security.W015",
def check_session_cookie_secure(app_configs, **kwargs):
    if settings.SESSION_COOKIE_SECURE is True:
    if _session_app():
        errors.append(W010)
    if _session_middleware():
        errors.append(W011)
    if len(errors) > 1:
        errors = [W012]
def check_session_cookie_httponly(app_configs, **kwargs):
    if settings.SESSION_COOKIE_HTTPONLY is True:
        errors.append(W013)
        errors.append(W014)
        errors = [W015]
def _session_middleware():
    return "django.contrib.sessions.middleware.SessionMiddleware" in settings.MIDDLEWARE
def _session_app():
    return "django.contrib.sessions" in settings.INSTALLED_APPS
requests.sessions
This module provides a Session object to manage and persist settings across
requests (cookies, auth, proxies).
from datetime import timedelta
from .adapters import HTTPAdapter
from .auth import _basic_auth_str
from .compat import Mapping, cookielib, urljoin, urlparse
from .cookies import (
    RequestsCookieJar,
    cookiejar_from_dict,
    extract_cookies_to_jar,
    merge_cookies,
    InvalidSchema,
from .hooks import default_hooks, dispatch_hook
# formerly defined here, reexposed here for backward compatibility
from .models import (  # noqa: F401
    DEFAULT_REDIRECT_LIMIT,
    REDIRECT_STATI,
    PreparedRequest,
from .utils import (  # noqa: F401
    DEFAULT_PORTS,
    default_headers,
    get_environ_proxies,
    get_netrc_auth,
    resolve_proxies,
    rewind_body,
    should_bypass_proxies,
# Preferred clock, based on which one is more accurate on a given system.
    preferred_clock = time.perf_counter
    preferred_clock = time.time
def merge_setting(request_setting, session_setting, dict_class=OrderedDict):
    """Determines appropriate setting for a given request, taking into account
    the explicit setting on that request, and the setting in the session. If a
    setting is a dictionary, they will be merged together using `dict_class`
    if session_setting is None:
        return request_setting
    if request_setting is None:
        return session_setting
    # Bypass if not a dictionary (e.g. verify)
        isinstance(session_setting, Mapping) and isinstance(request_setting, Mapping)
    merged_setting = dict_class(to_key_val_list(session_setting))
    merged_setting.update(to_key_val_list(request_setting))
    # Remove keys that are set to None. Extract keys first to avoid altering
    # the dictionary during iteration.
    none_keys = [k for (k, v) in merged_setting.items() if v is None]
    for key in none_keys:
        del merged_setting[key]
    return merged_setting
def merge_hooks(request_hooks, session_hooks, dict_class=OrderedDict):
    """Properly merges both requests and session hooks.
    This is necessary because when request_hooks == {'response': []}, the
    merge breaks Session hooks entirely.
    if session_hooks is None or session_hooks.get("response") == []:
        return request_hooks
    if request_hooks is None or request_hooks.get("response") == []:
        return session_hooks
    return merge_setting(request_hooks, session_hooks, dict_class)
class SessionRedirectMixin:
    def get_redirect_target(self, resp):
        """Receives a Response. Returns a redirect URI or ``None``"""
        # Due to the nature of how requests processes redirects this method will
        # be called at least once upon the original response and at least twice
        # on each subsequent redirect response (if any).
        # If a custom mixin is used to handle this logic, it may be advantageous
        # to cache the redirect location onto the response object as a private
        if resp.is_redirect:
            location = resp.headers["location"]
            # Currently the underlying http module on py3 decode headers
            # in latin1, but empirical evidence suggests that latin1 is very
            # rarely used with non-ASCII characters in HTTP headers.
            # It is more likely to get UTF8 header rather than latin1.
            # This causes incorrect handling of UTF8 encoded location headers.
            # To solve this, we re-encode the location in latin1.
            location = location.encode("latin1")
            return to_native_string(location, "utf8")
    def should_strip_auth(self, old_url, new_url):
        """Decide whether Authorization header should be removed when redirecting"""
        old_parsed = urlparse(old_url)
        new_parsed = urlparse(new_url)
        if old_parsed.hostname != new_parsed.hostname:
        # Special case: allow http -> https redirect when using the standard
        # ports. This isn't specified by RFC 7235, but is kept to avoid
        # breaking backwards compatibility with older versions of requests
        # that allowed any redirects on the same host.
            old_parsed.scheme == "http"
            and old_parsed.port in (80, None)
            and new_parsed.scheme == "https"
            and new_parsed.port in (443, None)
        # Handle default port usage corresponding to scheme.
        changed_port = old_parsed.port != new_parsed.port
        changed_scheme = old_parsed.scheme != new_parsed.scheme
        default_port = (DEFAULT_PORTS.get(old_parsed.scheme, None), None)
            not changed_scheme
            and old_parsed.port in default_port
            and new_parsed.port in default_port
        # Standard case: root URI must match
        return changed_port or changed_scheme
    def resolve_redirects(
        resp,
        proxies=None,
        yield_requests=False,
        **adapter_kwargs,
        """Receives a Response. Returns a generator of Responses or Requests."""
        hist = []  # keep track of history
        url = self.get_redirect_target(resp)
        previous_fragment = urlparse(req.url).fragment
        while url:
            prepared_request = req.copy()
            # Update history and keep track of redirects.
            # resp.history must ignore the original request in this loop
            hist.append(resp)
            resp.history = hist[1:]
                resp.content  # Consume socket so it can be released
            except (ChunkedEncodingError, ContentDecodingError, RuntimeError):
                resp.raw.read(decode_content=False)
            if len(resp.history) >= self.max_redirects:
                raise TooManyRedirects(
                    f"Exceeded {self.max_redirects} redirects.", response=resp
            # Release the connection back into the pool.
            resp.close()
            # Handle redirection without scheme (see: RFC 1808 Section 4)
                parsed_rurl = urlparse(resp.url)
                url = ":".join([to_native_string(parsed_rurl.scheme), url])
            # Normalize url case and attach previous fragment if needed (RFC 7231 7.1.2)
            if parsed.fragment == "" and previous_fragment:
                parsed = parsed._replace(fragment=previous_fragment)
            elif parsed.fragment:
                previous_fragment = parsed.fragment
            url = parsed.geturl()
            # Facilitate relative 'location' headers, as allowed by RFC 7231.
            # (e.g. '/path/to/resource' instead of 'http://domain.tld/path/to/resource')
            # Compliant with RFC3986, we percent encode the url.
            if not parsed.netloc:
                url = urljoin(resp.url, requote_uri(url))
                url = requote_uri(url)
            prepared_request.url = to_native_string(url)
            self.rebuild_method(prepared_request, resp)
            # https://github.com/psf/requests/issues/1084
            if resp.status_code not in (
                codes.temporary_redirect,
                # https://github.com/psf/requests/issues/3490
                purged_headers = ("Content-Length", "Content-Type", "Transfer-Encoding")
                for header in purged_headers:
                    prepared_request.headers.pop(header, None)
                prepared_request.body = None
            headers = prepared_request.headers
            headers.pop("Cookie", None)
            # Extract any cookies sent on the response to the cookiejar
            # in the new request. Because we've mutated our copied prepared
            # request, use the old one that we haven't yet touched.
            extract_cookies_to_jar(prepared_request._cookies, req, resp.raw)
            merge_cookies(prepared_request._cookies, self.cookies)
            prepared_request.prepare_cookies(prepared_request._cookies)
            # Rebuild auth and proxy information.
            proxies = self.rebuild_proxies(prepared_request, proxies)
            self.rebuild_auth(prepared_request, resp)
            # A failed tell() sets `_body_position` to `object()`. This non-None
            # value ensures `rewindable` will be True, allowing us to raise an
            # UnrewindableBodyError, instead of hanging the connection.
            rewindable = prepared_request._body_position is not None and (
                "Content-Length" in headers or "Transfer-Encoding" in headers
            # Attempt to rewind consumed file-like object.
            if rewindable:
                rewind_body(prepared_request)
            # Override the original request.
            req = prepared_request
            if yield_requests:
                resp = self.send(
                    verify=verify,
                    cert=cert,
                    proxies=proxies,
                    allow_redirects=False,
                extract_cookies_to_jar(self.cookies, prepared_request, resp.raw)
                # extract redirect url, if any, for the next loop
                yield resp
    def rebuild_auth(self, prepared_request, response):
        """When being redirected we may want to strip authentication from the
        request to avoid leaking credentials. This method intelligently removes
        and reapplies authentication where possible to avoid credential loss.
        url = prepared_request.url
        if "Authorization" in headers and self.should_strip_auth(
            response.request.url, url
            # If we get redirected to a new host, we should strip out any
            # authentication headers.
            del headers["Authorization"]
        # .netrc might have more auth for us on our new host.
        new_auth = get_netrc_auth(url) if self.trust_env else None
        if new_auth is not None:
            prepared_request.prepare_auth(new_auth)
    def rebuild_proxies(self, prepared_request, proxies):
        """This method re-evaluates the proxy configuration by considering the
        environment variables. If we are redirected to a URL covered by
        NO_PROXY, we strip the proxy configuration. Otherwise, we set missing
        proxy keys for this URL (in case they were stripped by a previous
        redirect).
        This method also replaces the Proxy-Authorization header where
        :rtype: dict
        scheme = urlparse(prepared_request.url).scheme
        new_proxies = resolve_proxies(prepared_request, proxies, self.trust_env)
        if "Proxy-Authorization" in headers:
            del headers["Proxy-Authorization"]
            username, password = get_auth_from_url(new_proxies[scheme])
        # urllib3 handles proxy authorization for us in the standard adapter.
        # Avoid appending this to TLS tunneled requests where it may be leaked.
        if not scheme.startswith("https") and username and password:
            headers["Proxy-Authorization"] = _basic_auth_str(username, password)
        return new_proxies
    def rebuild_method(self, prepared_request, response):
        """When being redirected we may want to change the method of the request
        based on certain specs or browser behavior.
        method = prepared_request.method
        # https://tools.ietf.org/html/rfc7231#section-6.4.4
        if response.status_code == codes.see_other and method != "HEAD":
        # Do what the browsers do, despite standards...
        # First, turn 302s into GETs.
        if response.status_code == codes.found and method != "HEAD":
        # Second, if a POST is responded to with a 301, turn it into a GET.
        # This bizarre behaviour is explained in Issue 1704.
        if response.status_code == codes.moved and method == "POST":
        prepared_request.method = method
class Session(SessionRedirectMixin):
    """A Requests session.
    Provides cookie persistence, connection-pooling, and configuration.
    Basic Usage::
      >>> s.get('https://httpbin.org/get')
    Or as a context manager::
      >>> with requests.Session() as s:
      ...     s.get('https://httpbin.org/get')
        "auth",
        "proxies",
        "hooks",
        "params",
        "verify",
        "cert",
        "adapters",
        "stream",
        "trust_env",
        "max_redirects",
        #: A case-insensitive dictionary of headers to be sent on each
        #: :class:`Request <Request>` sent from this
        #: :class:`Session <Session>`.
        self.headers = default_headers()
        #: Default Authentication tuple or object to attach to
        #: :class:`Request <Request>`.
        #: Dictionary mapping protocol or protocol and host to the URL of the proxy
        #: (e.g. {'http': 'foo.bar:3128', 'http://host.name': 'foo.bar:4012'}) to
        #: be used on each :class:`Request <Request>`.
        self.proxies = {}
        #: Event-handling hooks.
        #: Dictionary of querystring data to attach to each
        #: :class:`Request <Request>`. The dictionary values may be lists for
        #: representing multivalued query parameters.
        self.params = {}
        #: Stream response content default.
        self.stream = False
        #: SSL Verification default.
        #: Defaults to `True`, requiring requests to verify the TLS certificate at the
        #: remote end.
        #: If verify is set to `False`, requests will accept any TLS certificate
        #: presented by the server, and will ignore hostname mismatches and/or
        #: expired certificates, which will make your application vulnerable to
        #: man-in-the-middle (MitM) attacks.
        #: Only set this to `False` for testing.
        self.verify = True
        #: SSL client certificate default, if String, path to ssl client
        #: cert file (.pem). If Tuple, ('cert', 'key') pair.
        self.cert = None
        #: Maximum number of redirects allowed. If the request exceeds this
        #: limit, a :class:`TooManyRedirects` exception is raised.
        #: This defaults to requests.models.DEFAULT_REDIRECT_LIMIT, which is
        #: 30.
        self.max_redirects = DEFAULT_REDIRECT_LIMIT
        #: Trust environment settings for proxy configuration, default
        #: authentication and similar.
        self.trust_env = True
        #: A CookieJar containing all currently outstanding cookies set on this
        #: session. By default it is a
        #: :class:`RequestsCookieJar <requests.cookies.RequestsCookieJar>`, but
        #: may be any other ``cookielib.CookieJar`` compatible object.
        # Default connection adapters.
        self.adapters = OrderedDict()
        self.mount("https://", HTTPAdapter())
        self.mount("http://", HTTPAdapter())
    def prepare_request(self, request):
        """Constructs a :class:`PreparedRequest <PreparedRequest>` for
        transmission and returns it. The :class:`PreparedRequest` has settings
        merged from the :class:`Request <Request>` instance and those of the
        :class:`Session`.
        :param request: :class:`Request` instance to prepare with this
            session's settings.
        :rtype: requests.PreparedRequest
        cookies = request.cookies or {}
        # Bootstrap CookieJar.
        if not isinstance(cookies, cookielib.CookieJar):
            cookies = cookiejar_from_dict(cookies)
        # Merge with session cookies
        merged_cookies = merge_cookies(
            merge_cookies(RequestsCookieJar(), self.cookies), cookies
        # Set environment's basic authentication if not explicitly set.
        auth = request.auth
        if self.trust_env and not auth and not self.auth:
            auth = get_netrc_auth(request.url)
            method=request.method.upper(),
            url=request.url,
            files=request.files,
            data=request.data,
            json=request.json,
            headers=merge_setting(
                request.headers, self.headers, dict_class=CaseInsensitiveDict
            params=merge_setting(request.params, self.params),
            auth=merge_setting(auth, self.auth),
            cookies=merged_cookies,
            hooks=merge_hooks(request.hooks, self.hooks),
        allow_redirects=True,
        verify=None,
        """Constructs a :class:`Request <Request>`, prepares it and sends it.
        Returns :class:`Response <Response>` object.
        :param method: method for the new :class:`Request` object.
        :param params: (optional) Dictionary or bytes to be sent in the query
            string for the :class:`Request`.
        :param json: (optional) json to send in the body of the
            :class:`Request`.
        :param headers: (optional) Dictionary of HTTP Headers to send with the
        :param cookies: (optional) Dict or CookieJar object to send with the
        :param files: (optional) Dictionary of ``'filename': file-like-objects``
            for multipart encoding upload.
        :param auth: (optional) Auth tuple or callable to enable
            Basic/Digest/Custom HTTP Auth.
        :param timeout: (optional) How many seconds to wait for the server to send
            data before giving up, as a float, or a :ref:`(connect timeout,
            read timeout) <timeouts>` tuple.
        :param allow_redirects: (optional) Set to True by default.
        :param proxies: (optional) Dictionary mapping protocol or protocol and
            hostname to the URL of the proxy.
        :param hooks: (optional) Dictionary mapping hook name to one event or
            list of events, event must be callable.
        :param stream: (optional) whether to immediately download the response
            content. Defaults to ``False``.
            to a CA bundle to use. Defaults to ``True``. When set to
            ``False``, requests will accept any TLS certificate presented by
            the server, and will ignore hostname mismatches and/or expired
            certificates, which will make your application vulnerable to
            man-in-the-middle (MitM) attacks. Setting verify to ``False``
            may be useful during local development or testing.
        :param cert: (optional) if String, path to ssl client cert file (.pem).
            If Tuple, ('cert', 'key') pair.
        # Create the Request.
        req = Request(
            method=method.upper(),
            url=url,
            data=data or {},
            json=json,
            params=params or {},
            auth=auth,
            cookies=cookies,
            hooks=hooks,
        prep = self.prepare_request(req)
        proxies = proxies or {}
        settings = self.merge_environment_settings(
            prep.url, proxies, stream, verify, cert
        # Send the request.
        send_kwargs = {
            "timeout": timeout,
            "allow_redirects": allow_redirects,
        send_kwargs.update(settings)
        resp = self.send(prep, **send_kwargs)
    def get(self, url, **kwargs):
        r"""Sends a GET request. Returns :class:`Response` object.
        kwargs.setdefault("allow_redirects", True)
        return self.request("GET", url, **kwargs)
    def options(self, url, **kwargs):
        r"""Sends a OPTIONS request. Returns :class:`Response` object.
        return self.request("OPTIONS", url, **kwargs)
    def head(self, url, **kwargs):
        r"""Sends a HEAD request. Returns :class:`Response` object.
        return self.request("HEAD", url, **kwargs)
    def post(self, url, data=None, json=None, **kwargs):
        r"""Sends a POST request. Returns :class:`Response` object.
        :param json: (optional) json to send in the body of the :class:`Request`.
        return self.request("POST", url, data=data, json=json, **kwargs)
    def put(self, url, data=None, **kwargs):
        r"""Sends a PUT request. Returns :class:`Response` object.
        return self.request("PUT", url, data=data, **kwargs)
    def patch(self, url, data=None, **kwargs):
        r"""Sends a PATCH request. Returns :class:`Response` object.
        return self.request("PATCH", url, data=data, **kwargs)
    def delete(self, url, **kwargs):
        r"""Sends a DELETE request. Returns :class:`Response` object.
        return self.request("DELETE", url, **kwargs)
    def send(self, request, **kwargs):
        """Send a given PreparedRequest.
        # Set defaults that the hooks can utilize to ensure they always have
        # the correct parameters to reproduce the previous request.
        kwargs.setdefault("stream", self.stream)
        kwargs.setdefault("verify", self.verify)
        kwargs.setdefault("cert", self.cert)
        if "proxies" not in kwargs:
            kwargs["proxies"] = resolve_proxies(request, self.proxies, self.trust_env)
        # It's possible that users might accidentally send a Request object.
        # Guard against that specific failure case.
        if isinstance(request, Request):
            raise ValueError("You can only send PreparedRequests.")
        # Set up variables needed for resolve_redirects and dispatching of hooks
        allow_redirects = kwargs.pop("allow_redirects", True)
        stream = kwargs.get("stream")
        hooks = request.hooks
        # Get the appropriate adapter to use
        adapter = self.get_adapter(url=request.url)
        # Start time (approximately) of the request
        start = preferred_clock()
        # Send the request
        r = adapter.send(request, **kwargs)
        # Total elapsed time of the request (approximately)
        elapsed = preferred_clock() - start
        r.elapsed = timedelta(seconds=elapsed)
        # Response manipulation hooks
        r = dispatch_hook("response", hooks, r, **kwargs)
        # Persist cookies
        if r.history:
            # If the hooks create history then we want those cookies too
            for resp in r.history:
                extract_cookies_to_jar(self.cookies, resp.request, resp.raw)
        extract_cookies_to_jar(self.cookies, request, r.raw)
        # Resolve redirects if allowed.
        if allow_redirects:
            # Redirect resolving generator.
            gen = self.resolve_redirects(r, request, **kwargs)
            history = [resp for resp in gen]
            history = []
        # Shuffle things around if there's history.
        if history:
            # Insert the first (original) request at the start
            history.insert(0, r)
            # Get the last request made
            r = history.pop()
            r.history = history
        # If redirects aren't being followed, store the response on the Request for Response.next().
        if not allow_redirects:
                r._next = next(
                    self.resolve_redirects(r, request, yield_requests=True, **kwargs)
    def merge_environment_settings(self, url, proxies, stream, verify, cert):
        Check the environment and merge it with some settings.
        # Gather clues from the surrounding environment.
        if self.trust_env:
            # Set environment's proxies.
            no_proxy = proxies.get("no_proxy") if proxies is not None else None
            env_proxies = get_environ_proxies(url, no_proxy=no_proxy)
            for k, v in env_proxies.items():
                proxies.setdefault(k, v)
            # Look for requests environment configuration
            # and be compatible with cURL.
            if verify is True or verify is None:
                verify = (
                    os.environ.get("REQUESTS_CA_BUNDLE")
                    or os.environ.get("CURL_CA_BUNDLE")
                    or verify
        # Merge all the kwargs.
        proxies = merge_setting(proxies, self.proxies)
        stream = merge_setting(stream, self.stream)
        verify = merge_setting(verify, self.verify)
        cert = merge_setting(cert, self.cert)
        return {"proxies": proxies, "stream": stream, "verify": verify, "cert": cert}
    def get_adapter(self, url):
        Returns the appropriate connection adapter for the given URL.
        :rtype: requests.adapters.BaseAdapter
        for prefix, adapter in self.adapters.items():
            if url.lower().startswith(prefix.lower()):
                return adapter
        # Nothing matches :-/
        raise InvalidSchema(f"No connection adapters were found for {url!r}")
        """Closes all adapters and as such the session"""
        for v in self.adapters.values():
            v.close()
    def mount(self, prefix, adapter):
        """Registers a connection adapter to a prefix.
        Adapters are sorted in descending order by prefix length.
        self.adapters[prefix] = adapter
        keys_to_move = [k for k in self.adapters if len(k) < len(prefix)]
        for key in keys_to_move:
            self.adapters[key] = self.adapters.pop(key)
        state = {attr: getattr(self, attr, None) for attr in self.__attrs__}
            setattr(self, attr, value)
def session():
    Returns a :class:`Session` for context-management.
    .. deprecated:: 1.0.0
        This method has been deprecated since version 1.0.0 and is only kept for
        backwards compatibility. New code should use :class:`~requests.sessions.Session`
        to create a session. This may be removed at a future date.
    :rtype: Session
    return Session()
import itsdangerous
from itsdangerous.exc import BadSignature
from starlette.datastructures import MutableHeaders, Secret
class SessionMiddleware:
        secret_key: str | Secret,
        session_cookie: str = "session",
        max_age: int | None = 14 * 24 * 60 * 60,  # 14 days, in seconds
        same_site: Literal["lax", "strict", "none"] = "lax",
        https_only: bool = False,
        self.signer = itsdangerous.TimestampSigner(str(secret_key))
        self.session_cookie = session_cookie
        self.max_age = max_age
        self.security_flags = "httponly; samesite=" + same_site
        if https_only:  # Secure flag can be used with HTTPS only
            self.security_flags += "; secure"
            self.security_flags += f"; domain={domain}"
        if scope["type"] not in ("http", "websocket"):  # pragma: no cover
        connection = HTTPConnection(scope)
        initial_session_was_empty = True
        if self.session_cookie in connection.cookies:
            data = connection.cookies[self.session_cookie].encode("utf-8")
                data = self.signer.unsign(data, max_age=self.max_age)
                scope["session"] = json.loads(b64decode(data))
                initial_session_was_empty = False
            except BadSignature:
                scope["session"] = {}
        async def send_wrapper(message: Message) -> None:
                if scope["session"]:
                    # We have session data to persist.
                    data = b64encode(json.dumps(scope["session"]).encode("utf-8"))
                    data = self.signer.sign(data)
                    headers = MutableHeaders(scope=message)
                    header_value = "{session_cookie}={data}; path={path}; {max_age}{security_flags}".format(
                        session_cookie=self.session_cookie,
                        data=data.decode("utf-8"),
                        path=self.path,
                        max_age=f"Max-Age={self.max_age}; " if self.max_age else "",
                        security_flags=self.security_flags,
                    headers.append("Set-Cookie", header_value)
                elif not initial_session_was_empty:
                    # The session has been cleared.
                    header_value = "{session_cookie}={data}; path={path}; {expires}{security_flags}".format(
                        data="null",
                        expires="expires=Thu, 01 Jan 1970 00:00:00 GMT; ",
        await self.app(scope, receive, send_wrapper)
        Create a ChatKit session
        Cancel a ChatKit session
            f"/chatkit/sessions/{session_id}/cancel",
