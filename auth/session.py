    "Session",
    "InputAudioNoiseReduction",
    "InputAudioTranscription",
    "Tool",
    "Tracing",
    "TracingTracingConfiguration",
    "TurnDetection",
class InputAudioNoiseReduction(BaseModel):
    type: Optional[Literal["near_field", "far_field"]] = None
    """Type of noise reduction.
    `near_field` is for close-talking microphones such as headphones, `far_field` is
    for far-field microphones such as laptop or conference room microphones.
class InputAudioTranscription(BaseModel):
    The model to use for transcription, current options are `gpt-4o-transcribe`,
    `gpt-4o-mini-transcribe`, and `whisper-1`.
    An optional text to guide the model's style or continue a previous audio
    segment. For `whisper-1`, the
    [prompt is a list of keywords](https://platform.openai.com/docs/guides/speech-to-text#prompting).
    For `gpt-4o-transcribe` models, the prompt is a free text string, for example
    "expect words related to technology".
class Tool(BaseModel):
class TracingTracingConfiguration(BaseModel):
    group_id: Optional[str] = None
    The group id to attach to this trace to enable filtering and grouping in the
    traces dashboard.
    metadata: Optional[object] = None
    The arbitrary metadata to attach to this trace to enable filtering in the traces
    dashboard.
    workflow_name: Optional[str] = None
    """The name of the workflow to attach to this trace.
    This is used to name the trace in the traces dashboard.
Tracing: TypeAlias = Union[Literal["auto"], TracingTracingConfiguration]
class TurnDetection(BaseModel):
    create_response: Optional[bool] = None
    Whether or not to automatically generate a response when a VAD stop event
    occurs.
    eagerness: Optional[Literal["low", "medium", "high", "auto"]] = None
    """Used only for `semantic_vad` mode.
    The eagerness of the model to respond. `low` will wait longer for the user to
    continue speaking, `high` will respond more quickly. `auto` is the default and
    is equivalent to `medium`.
    interrupt_response: Optional[bool] = None
    Whether or not to automatically interrupt any ongoing response with output to
    the default conversation (i.e. `conversation` of `auto`) when a VAD start event
    prefix_padding_ms: Optional[int] = None
    """Used only for `server_vad` mode.
    Amount of audio to include before the VAD detected speech (in milliseconds).
    Defaults to 300ms.
    silence_duration_ms: Optional[int] = None
    Duration of silence to detect speech stop (in milliseconds). Defaults to 500ms.
    With shorter values the model will respond more quickly, but may jump in on
    short pauses from the user.
    threshold: Optional[float] = None
    Activation threshold for VAD (0.0 to 1.0), this defaults to 0.5. A higher
    threshold will require louder audio to activate the model, and thus might
    perform better in noisy environments.
    type: Optional[Literal["server_vad", "semantic_vad"]] = None
    """Type of turn detection."""
class Session(BaseModel):
    """Unique identifier for the session that looks like `sess_1234567890abcdef`."""
    input_audio_format: Optional[Literal["pcm16", "g711_ulaw", "g711_alaw"]] = None
    """The format of input audio.
    Options are `pcm16`, `g711_ulaw`, or `g711_alaw`. For `pcm16`, input audio must
    be 16-bit PCM at a 24kHz sample rate, single channel (mono), and little-endian
    byte order.
    input_audio_noise_reduction: Optional[InputAudioNoiseReduction] = None
    """Configuration for input audio noise reduction.
    This can be set to `null` to turn off. Noise reduction filters audio added to
    the input audio buffer before it is sent to VAD and the model. Filtering the
    audio can improve VAD and turn detection accuracy (reducing false positives) and
    model performance by improving perception of the input audio.
    input_audio_transcription: Optional[InputAudioTranscription] = None
    Configuration for input audio transcription, defaults to off and can be set to
    model: Optional[
    ] = None
    """The Realtime model used for this session."""
    """The format of output audio.
    Options are `pcm16`, `g711_ulaw`, or `g711_alaw`. For `pcm16`, output audio is
    sampled at a rate of 24kHz.
    speed: Optional[float] = None
    """The speed of the model's spoken response.
    1.0 is the default speed. 0.25 is the minimum speed. 1.5 is the maximum speed.
    This value can only be changed in between model turns, not while a response is
    in progress.
    """Sampling temperature for the model, limited to [0.6, 1.2].
    For audio models a temperature of 0.8 is highly recommended for best
    performance.
    Options are `auto`, `none`, `required`, or specify a function.
    tools: Optional[List[Tool]] = None
    tracing: Optional[Tracing] = None
    """Configuration options for tracing.
    Set to null to disable tracing. Once tracing is enabled for a session, the
    configuration cannot be modified.
    turn_detection: Optional[TurnDetection] = None
    """Configuration for turn detection, ether Server VAD or Semantic VAD.
    This can be set to `null` to turn off, in which case the client must manually
    trigger model response. Server VAD means that the model will detect the start
    and end of speech based on audio volume and respond at the end of user speech.
    Semantic VAD is more advanced and uses a turn detection model (in conjunction
    with VAD) to semantically estimate whether the user has finished speaking, then
    dynamically sets a timeout based on this probability. For example, if user audio
    trails off with "uhhm", the model will score a low probability of turn end and
    wait longer for the user to continue speaking. This can be useful for more
    natural conversations, but may have a higher latency.
from requests.adapters import HTTPAdapter
from urllib3.poolmanager import PoolManager
API_DOMAIN = os.environ.get('DROPBOX_API_DOMAIN',
    os.environ.get('DROPBOX_DOMAIN', '.dropboxapi.com'))
WEB_DOMAIN = os.environ.get('DROPBOX_WEB_DOMAIN',
    os.environ.get('DROPBOX_DOMAIN', '.dropbox.com'))
# Default short hostname for RPC-style routes.
HOST_API = 'api'
# Default short hostname for upload and download-style routes.
HOST_CONTENT = 'content'
# Default short hostname for longpoll routes.
HOST_NOTIFY = 'notify'
# Default short hostname for the Drobox website.
HOST_WWW = 'www'
API_HOST = os.environ.get('DROPBOX_API_HOST', HOST_API + API_DOMAIN)
API_CONTENT_HOST = os.environ.get('DROPBOX_API_CONTENT_HOST', HOST_CONTENT + API_DOMAIN)
API_NOTIFICATION_HOST = os.environ.get('DROPBOX_API_NOTIFY_HOST', HOST_NOTIFY + API_DOMAIN)
WEB_HOST = os.environ.get('DROPBOX_WEB_HOST', HOST_WWW + WEB_DOMAIN)
# This is the default longest time we'll block on receiving data from the server
DEFAULT_TIMEOUT = 100
# TODO(kelkabany): We probably only want to instantiate this once so that even
# if multiple Dropbox objects are instantiated, they all share the same pool.
class _SSLAdapter(HTTPAdapter):
    _ca_certs = None
    def __init__(self, *args, **kwargs):
        self._ca_certs = kwargs.pop("ca_certs", None)
        super(_SSLAdapter, self).__init__(*args, **kwargs)
    def init_poolmanager(self, connections, maxsize, block=False, **_):
        self.poolmanager = PoolManager(
            num_pools=connections,
            block=block,
            cert_reqs=ssl.CERT_REQUIRED,
            ca_certs=self._ca_certs,
def pinned_session(pool_maxsize=8, ca_certs=None):
    # always verify, use cert bundle if provided
    _session = requests.session()
    # requests
    if ca_certs is not None:
        _session.verify = ca_certs
        _session.verify = True
    # urllib3 within requests
    http_adapter = _SSLAdapter(pool_connections=4, pool_maxsize=pool_maxsize, ca_certs=ca_certs)
    _session.mount('https://', http_adapter)
    return _session
SSLError = requests.exceptions.SSLError  # raised on verification errors
from .session_base import SessionBase
from time import time
class Session(SessionBase):
                 token_type,
                 scope_string,
                 auth_server_url,
                 refresh_token=None,
                 client_secret=None):
        self.token_type = token_type
        self._expires_at = time() + int(expires_in)
        self.scope = scope_string.split(" ")
        self.client_id = client_id
        self.auth_server_url = auth_server_url
        self.client_secret = client_secret
        """Whether or not the session has expired
            bool: True if the session has expired, otherwise false
        # Add a 10 second buffer in case the token is just about to expire
        return self._expires_at < time() - 10
    def refresh_session(self, expires_in, scope_string, access_token, refresh_token):
        """Save the current session.
        IMPORTANT: This implementation should only be used for debugging.
        For real applications, the Session object should be subclassed and
        both save_session() and load_session() should be overwritten using
        the client system's correct mechanism (keychain, database, etc.).
        Remember, the access_token should be treated the same as a password.
            save_session_kwargs (dicr): To be used by implementation
            of save_session, however save_session wants to use them. The
            default implementation (this one) takes a relative or absolute
            file path for pickle save location, under the name "path"
        path = "session.pickle"
        if "path" in save_session_kwargs:
            path = save_session_kwargs["path"]
        with open(path, "wb") as session_file:
            # pickle.HIGHEST_PROTOCOL is binary format. Good perf.
            pickle.dump(self, session_file, pickle.HIGHEST_PROTOCOL)
    def load_session(**load_session_kwargs):
            load_session_kwargs (dict): To be used by implementation
            of load_session, however load_session wants to use them. The
            :class:`Session`: The loaded session
        if "path" in load_session_kwargs:
            path = load_session_kwargs["path"]
        with open(path, "rb") as session_file:
            return pickle.load(session_file)
from django.contrib.messages.storage.base import BaseStorage
from django.contrib.messages.storage.cookie import MessageDecoder, MessageEncoder
class SessionStorage(BaseStorage):
    Store messages in the session (that is, django.contrib.sessions).
    session_key = "_messages"
        if not hasattr(request, "session"):
                "The session-based temporary message storage requires session "
                "middleware to be installed, and come before the message "
                "middleware in the MIDDLEWARE list."
        super().__init__(request, *args, **kwargs)
        Retrieve a list of messages from the request's session. This storage
        always stores everything it is given, so return True for the
        all_retrieved flag.
            self.deserialize_messages(self.request.session.get(self.session_key)),
        Store a list of messages to the request's session.
        if messages:
            self.request.session[self.session_key] = self.serialize_messages(messages)
            self.request.session.pop(self.session_key, None)
    def serialize_messages(self, messages):
        encoder = MessageEncoder()
        return encoder.encode(messages)
    def deserialize_messages(self, data):
        if data and isinstance(data, str):
            return json.loads(data, cls=MessageDecoder)
from django.contrib.sessions.backends.base import SessionBase
    A simple cookie-based session storage implementation.
    The session key is actually the session data, pickled and encoded.
    This means that saving the session will change the session key.
        self._session_key = self.encode({})
        self._session_key = self.encode(self._session)
            return self.decode(self.session_key)
"""PipSession and supporting code, containing all pip-specific
network request configuration and behavior.
import ipaddress
from collections.abc import Generator, Mapping, Sequence
from pip._vendor import requests, urllib3
from pip._vendor.cachecontrol import CacheControlAdapter as _BaseCacheControlAdapter
from pip._vendor.requests.adapters import DEFAULT_POOLBLOCK, BaseAdapter
from pip._vendor.requests.adapters import HTTPAdapter as _BaseHTTPAdapter
from pip._vendor.requests.models import PreparedRequest, Response
from pip._vendor.requests.structures import CaseInsensitiveDict
from pip._vendor.urllib3.connectionpool import ConnectionPool
from pip._vendor.urllib3.exceptions import InsecureRequestWarning
from pip import __version__
from pip._internal.network.auth import MultiDomainBasicAuth
from pip._internal.network.cache import SafeFileCache
# Import ssl from compat so the initial import occurs in only one place.
from pip._internal.utils.compat import has_tls
from pip._internal.utils.glibc import libc_ver
from pip._internal.utils.misc import build_url_from_netloc, parse_netloc
    from ssl import SSLContext
    from pip._vendor.urllib3 import ProxyManager
    from pip._vendor.urllib3.poolmanager import PoolManager
SecureOrigin = tuple[str, str, Optional[Union[int, str]]]
# Ignore warning raised when using --trusted-host.
warnings.filterwarnings("ignore", category=InsecureRequestWarning)
SECURE_ORIGINS: list[SecureOrigin] = [
    # protocol, hostname, port
    # Taken from Chrome's list of secure origins (See: http://bit.ly/1qrySKC)
    ("https", "*", "*"),
    ("*", "localhost", "*"),
    ("*", "127.0.0.0/8", "*"),
    ("*", "::1/128", "*"),
    ("file", "*", None),
    # ssh is always secure.
    ("ssh", "*", "*"),
# These are environment variables present when running under various
# CI systems.  For each variable, some CI systems that use the variable
# are indicated.  The collection was chosen so that for each of a number
# of popular systems, at least one of the environment variables is used.
# This list is used to provide some indication of and lower bound for
# CI traffic to PyPI.  Thus, it is okay if the list is not comprehensive.
# For more background, see: https://github.com/pypa/pip/issues/5499
CI_ENVIRONMENT_VARIABLES = (
    # Azure Pipelines
    "BUILD_BUILDID",
    # Jenkins
    "BUILD_ID",
    # AppVeyor, CircleCI, Codeship, Gitlab CI, Shippable, Travis CI
    "CI",
    # Explicit environment variable.
    "PIP_IS_CI",
def looks_like_ci() -> bool:
    Return whether it looks like pip is running under CI.
    # We don't use the method of checking for a tty (e.g. using isatty())
    # because some CI systems mimic a tty (e.g. Travis CI).  Thus that
    # method doesn't provide definitive information in either direction.
    return any(name in os.environ for name in CI_ENVIRONMENT_VARIABLES)
def user_agent() -> str:
    Return a string representing the user agent.
    data: dict[str, Any] = {
        "installer": {"name": "pip", "version": __version__},
        "python": platform.python_version(),
        "implementation": {
            "name": platform.python_implementation(),
    if data["implementation"]["name"] == "CPython":
        data["implementation"]["version"] = platform.python_version()
    elif data["implementation"]["name"] == "PyPy":
        pypy_version_info = sys.pypy_version_info  # type: ignore
        if pypy_version_info.releaselevel == "final":
            pypy_version_info = pypy_version_info[:3]
        data["implementation"]["version"] = ".".join(
            [str(x) for x in pypy_version_info]
    elif data["implementation"]["name"] == "Jython":
        # Complete Guess
    elif data["implementation"]["name"] == "IronPython":
    if sys.platform.startswith("linux"):
        from pip._vendor import distro
        linux_distribution = distro.name(), distro.version(), distro.codename()
        distro_infos: dict[str, Any] = dict(
                lambda x: x[1],
                zip(["name", "version", "id"], linux_distribution),
        libc = dict(
                zip(["lib", "version"], libc_ver()),
        if libc:
            distro_infos["libc"] = libc
        if distro_infos:
            data["distro"] = distro_infos
    if sys.platform.startswith("darwin") and platform.mac_ver()[0]:
        data["distro"] = {"name": "macOS", "version": platform.mac_ver()[0]}
    if platform.system():
        data.setdefault("system", {})["name"] = platform.system()
    if platform.release():
        data.setdefault("system", {})["release"] = platform.release()
    if platform.machine():
        data["cpu"] = platform.machine()
    if has_tls():
        import _ssl as ssl
        data["openssl_version"] = ssl.OPENSSL_VERSION
    setuptools_dist = get_default_environment().get_distribution("setuptools")
    if setuptools_dist is not None:
        data["setuptools_version"] = str(setuptools_dist.version)
    if shutil.which("rustc") is not None:
        # If for any reason `rustc --version` fails, silently ignore it
            rustc_output = subprocess.check_output(
                ["rustc", "--version"], stderr=subprocess.STDOUT, timeout=0.5
            if rustc_output.startswith(b"rustc "):
                # The format of `rustc --version` is:
                # `b'rustc 1.52.1 (9bc8c42bb 2021-05-09)\n'`
                # We extract just the middle (1.52.1) part
                data["rustc_version"] = rustc_output.split(b" ")[1].decode()
    # Use None rather than False so as not to give the impression that
    # pip knows it is not being run under CI.  Rather, it is a null or
    # inconclusive result.  Also, we include some value rather than no
    # value to make it easier to know that the check has been run.
    data["ci"] = True if looks_like_ci() else None
    user_data = os.environ.get("PIP_USER_AGENT_USER_DATA")
    if user_data is not None:
        data["user_data"] = user_data
    return "{data[installer][name]}/{data[installer][version]} {json}".format(
        json=json.dumps(data, separators=(",", ":"), sort_keys=True),
class LocalFSAdapter(BaseAdapter):
    def send(
        request: PreparedRequest,
        timeout: float | tuple[float, float] | tuple[float, None] | None = None,
        verify: bool | str = True,
        cert: bytes | str | tuple[bytes | str, bytes | str] | None = None,
        proxies: Mapping[str, str] | None = None,
        assert request.url is not None
        pathname = url_to_path(request.url)
        resp = Response()
        resp.status_code = 200
        resp.url = request.url
            stats = os.stat(pathname)
            # format the exception raised as a io.BytesIO object,
            # to return a better error message:
            resp.status_code = 404
            resp.reason = type(exc).__name__
            resp.raw = io.BytesIO(f"{resp.reason}: {exc}".encode())
            modified = email.utils.formatdate(stats.st_mtime, usegmt=True)
            content_type = mimetypes.guess_type(pathname)[0] or "text/plain"
            resp.headers = CaseInsensitiveDict(
                    "Content-Type": content_type,
                    "Content-Length": str(stats.st_size),
                    "Last-Modified": modified,
            resp.raw = open(pathname, "rb")
            resp.close = resp.raw.close  # type: ignore[method-assign]
class _SSLContextAdapterMixin:
    """Mixin to add the ``ssl_context`` constructor argument to HTTP adapters.
    The additional argument is forwarded directly to the pool manager. This allows us
    to dynamically decide what SSL store to use at runtime, which is used to implement
    the optional ``truststore`` backend.
        ssl_context: SSLContext | None = None,
        self._ssl_context = ssl_context
    def init_poolmanager(
        connections: int,
        maxsize: int,
        block: bool = DEFAULT_POOLBLOCK,
        **pool_kwargs: Any,
    ) -> PoolManager:
        if self._ssl_context is not None:
            pool_kwargs.setdefault("ssl_context", self._ssl_context)
        return super().init_poolmanager(  # type: ignore[misc, no-any-return]
            connections=connections,
            **pool_kwargs,
    def proxy_manager_for(self, proxy: str, **proxy_kwargs: Any) -> ProxyManager:
        # Proxy manager replaces the pool manager, so inject our SSL
        # context here too. https://github.com/pypa/pip/issues/13288
            proxy_kwargs.setdefault("ssl_context", self._ssl_context)
        return super().proxy_manager_for(proxy, **proxy_kwargs)  # type: ignore[misc, no-any-return]
class HTTPAdapter(_SSLContextAdapterMixin, _BaseHTTPAdapter):
class CacheControlAdapter(_SSLContextAdapterMixin, _BaseCacheControlAdapter):
class InsecureHTTPAdapter(HTTPAdapter):
    def cert_verify(
        conn: ConnectionPool,
        verify: bool | str,
        cert: str | tuple[str, str] | None,
        super().cert_verify(conn=conn, url=url, verify=False, cert=cert)
class InsecureCacheControlAdapter(CacheControlAdapter):
class PipSession(requests.Session):
    timeout: int | None = None
        retries: int = 0,
        resume_retries: int = 0,
        cache: str | None = None,
        trusted_hosts: Sequence[str] = (),
        :param trusted_hosts: Domains not to emit warnings for when not using
            HTTPS.
        # Namespace the attribute with "pip_" just in case to prevent
        # possible conflicts with the base class.
        self.pip_trusted_origins: list[tuple[str, int | None]] = []
        self.pip_proxy = None
        # Attach our User Agent to the request
        self.headers["User-Agent"] = user_agent()
        # Attach our Authentication handler to the session
        self.auth: MultiDomainBasicAuth = MultiDomainBasicAuth(index_urls=index_urls)
        # Create our urllib3.Retry instance which will allow us to customize
        # how we handle retries.
        retries = urllib3.Retry(
            # Set the total number of retries that a particular request can
            # have.
            total=retries,
            # A 503 error from PyPI typically means that the Fastly -> Origin
            # connection got interrupted in some way. A 503 error in general
            # is typically considered a transient error so we'll go ahead and
            # retry it.
            # A 500 may indicate transient error in Amazon S3
            # A 502 may be a transient error from a CDN like CloudFlare or CloudFront
            # A 520 or 527 - may indicate transient error in CloudFlare
            status_forcelist=[500, 502, 503, 520, 527],
            # Add a small amount of back off between failed requests in
            # order to prevent hammering the service.
            backoff_factor=0.25,
        self.resume_retries = resume_retries
        # Our Insecure HTTPAdapter disables HTTPS validation. It does not
        # support caching so we'll use it for all http:// URLs.
        # If caching is disabled, we will also use it for
        # https:// hosts that we've marked as ignoring
        # TLS errors for (trusted-hosts).
        insecure_adapter = InsecureHTTPAdapter(max_retries=retries)
        # We want to _only_ cache responses on securely fetched origins or when
        # the host is specified as trusted. We do this because
        # we can't validate the response of an insecurely/untrusted fetched
        # origin, and we don't want someone to be able to poison the cache and
        # require manual eviction from the cache to fix it.
        self._trusted_host_adapter: InsecureCacheControlAdapter | InsecureHTTPAdapter
            secure_adapter: _BaseHTTPAdapter = CacheControlAdapter(
                cache=SafeFileCache(cache),
                max_retries=retries,
                ssl_context=ssl_context,
            self._trusted_host_adapter = InsecureCacheControlAdapter(
            secure_adapter = HTTPAdapter(max_retries=retries, ssl_context=ssl_context)
            self._trusted_host_adapter = insecure_adapter
        self.mount("https://", secure_adapter)
        self.mount("http://", insecure_adapter)
        # Enable file:// urls
        self.mount("file://", LocalFSAdapter())
        for host in trusted_hosts:
            self.add_trusted_host(host, suppress_logging=True)
    def update_index_urls(self, new_index_urls: list[str]) -> None:
        :param new_index_urls: New index urls to update the authentication
            handler with.
        self.auth.index_urls = new_index_urls
    def add_trusted_host(
        self, host: str, source: str | None = None, suppress_logging: bool = False
        :param host: It is okay to provide a host that has previously been
        :param source: An optional source string, for logging where the host
            string came from.
        if not suppress_logging:
            msg = f"adding trusted host: {host!r}"
            if source is not None:
                msg += f" (from {source})"
            logger.info(msg)
        parsed_host, parsed_port = parse_netloc(host)
        if parsed_host is None:
            raise ValueError(f"Trusted host URL must include a host part: {host!r}")
        if (parsed_host, parsed_port) not in self.pip_trusted_origins:
            self.pip_trusted_origins.append((parsed_host, parsed_port))
        self.mount(
            build_url_from_netloc(host, scheme="http") + "/", self._trusted_host_adapter
        self.mount(build_url_from_netloc(host) + "/", self._trusted_host_adapter)
        if not parsed_port:
                build_url_from_netloc(host, scheme="http") + ":",
                self._trusted_host_adapter,
            # Mount wildcard ports for the same host.
            self.mount(build_url_from_netloc(host) + ":", self._trusted_host_adapter)
    def iter_secure_origins(self) -> Generator[SecureOrigin, None, None]:
        yield from SECURE_ORIGINS
        for host, port in self.pip_trusted_origins:
            yield ("*", host, "*" if port is None else port)
    def is_secure_origin(self, location: Link) -> bool:
        # Determine if this url used a secure transport mechanism
        parsed = urllib.parse.urlparse(str(location))
        origin_protocol, origin_host, origin_port = (
            parsed.scheme,
            parsed.hostname,
            parsed.port,
        # The protocol to use to see if the protocol matches.
        # Don't count the repository type as part of the protocol: in
        # cases such as "git+ssh", only use "ssh". (I.e., Only verify against
        # the last scheme.)
        origin_protocol = origin_protocol.rsplit("+", 1)[-1]
        # Determine if our origin is a secure origin by looking through our
        # hardcoded list of secure origins, as well as any additional ones
        # configured on this PackageFinder instance.
        for secure_origin in self.iter_secure_origins():
            secure_protocol, secure_host, secure_port = secure_origin
            if origin_protocol != secure_protocol and secure_protocol != "*":
                addr = ipaddress.ip_address(origin_host or "")
                network = ipaddress.ip_network(secure_host)
                # We don't have both a valid address or a valid network, so
                # we'll check this origin against hostnames.
                    origin_host
                    and origin_host.lower() != secure_host.lower()
                    and secure_host != "*"
                # We have a valid address and network, so see if the address
                # is contained within the network.
                if addr not in network:
            # Check to see if the port matches.
                origin_port != secure_port
                and secure_port != "*"
                and secure_port is not None
            # If we've gotten here, then this origin matches the current
            # secure origin and we should return True
        # If we've gotten to this point, then the origin isn't secure and we
        # will not accept it as a valid location to search. We will however
        # log a warning that we are ignoring it.
            "The repository located at %s is not a trusted or secure host and "
            "is being ignored. If this repository is available via HTTPS we "
            "recommend you use HTTPS instead, otherwise you may silence "
            "this warning and allow it anyway with '--trusted-host %s'.",
            origin_host,
    def request(self, method: str, url: str, *args: Any, **kwargs: Any) -> Response:  # type: ignore[override]
        # Allow setting a default timeout on a session
        kwargs.setdefault("timeout", self.timeout)
        # Allow setting a default proxies on a session
        kwargs.setdefault("proxies", self.proxies)
        # Dispatch the actual request
        return super().request(method, url, *args, **kwargs)
