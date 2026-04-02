# Copyright 2016 Google Inc. All Rights Reserved.
"""Helpers for authentication using oauth2client or google-auth."""
    import google.auth
    import google.auth.credentials
    HAS_GOOGLE_AUTH = True
except ImportError:  # pragma: NO COVER
    HAS_GOOGLE_AUTH = False
    import google_auth_httplib2
    google_auth_httplib2 = None
    import oauth2client
    import oauth2client.client
    HAS_OAUTH2CLIENT = True
    HAS_OAUTH2CLIENT = False
def credentials_from_file(filename, scopes=None, quota_project_id=None):
    """Returns credentials loaded from a file."""
    if HAS_GOOGLE_AUTH:
        credentials, _ = google.auth.load_credentials_from_file(
            filename, scopes=scopes, quota_project_id=quota_project_id
        return credentials
        raise EnvironmentError(
            "client_options.credentials_file is only supported in google-auth."
def default_credentials(scopes=None, quota_project_id=None):
    """Returns Application Default Credentials."""
        credentials, _ = google.auth.default(
            scopes=scopes, quota_project_id=quota_project_id
    elif HAS_OAUTH2CLIENT:
        if scopes is not None or quota_project_id is not None:
                "client_options.scopes and client_options.quota_project_id are not supported in oauth2client."
                "Please install google-auth."
        return oauth2client.client.GoogleCredentials.get_application_default()
            "No authentication library is available. Please install either "
            "google-auth or oauth2client."
def with_scopes(credentials, scopes):
    """Scopes the credentials if necessary.
    if HAS_GOOGLE_AUTH and isinstance(credentials, google.auth.credentials.Credentials):
        return google.auth.credentials.with_scopes_if_required(credentials, scopes)
            if credentials.create_scoped_required():
                return credentials.create_scoped(scopes)
def authorized_http(credentials):
    """Returns an http client that is authorized with the given credentials.
        if google_auth_httplib2 is None:
                "Credentials from google.auth specified, but "
                "google-api-python-client is unable to use these credentials "
                "unless google-auth-httplib2 is installed. Please install "
                "google-auth-httplib2."
        return google_auth_httplib2.AuthorizedHttp(credentials, http=build_http())
        return credentials.authorize(build_http())
def refresh_credentials(credentials):
    # Refresh must use a new http instance, as the one associated with the
    # credentials could be a AuthorizedHttp or an oauth2client-decorated
    # Http instance which would cause a weird recursive loop of refreshing
    # and likely tear a hole in spacetime.
    refresh_http = httplib2.Http()
        request = google_auth_httplib2.Request(refresh_http)
        return credentials.refresh(request)
        return credentials.refresh(refresh_http)
def apply_credentials(credentials, headers):
    # oauth2client and google-auth have the same interface for this.
    if not is_valid(credentials):
        refresh_credentials(credentials)
    return credentials.apply(headers)
def is_valid(credentials):
        return credentials.valid
            credentials.access_token is not None
            and not credentials.access_token_expired
def get_credentials_from_http(http):
    if http is None:
    elif hasattr(http.request, "credentials"):
        return http.request.credentials
    elif hasattr(http, "credentials") and not isinstance(
        http.credentials, httplib2.Credentials
        return http.credentials
from urllib.request import parse_http_list
from ._exceptions import ProtocolError
from ._models import Cookies, Request, Response
from ._utils import to_bytes, to_str, unquote
if typing.TYPE_CHECKING:  # pragma: no cover
__all__ = ["Auth", "BasicAuth", "DigestAuth", "NetRCAuth"]
class Auth:
    Base class for all authentication schemes.
    To implement a custom authentication scheme, subclass `Auth` and override
    the `.auth_flow()` method.
    If the authentication scheme does I/O such as disk access or network calls, or uses
    synchronization primitives such as locks, you should override `.sync_auth_flow()`
    and/or `.async_auth_flow()` instead of `.auth_flow()` to provide specialized
    implementations that will be used by `Client` and `AsyncClient` respectively.
    requires_request_body = False
    requires_response_body = False
    def auth_flow(self, request: Request) -> typing.Generator[Request, Response, None]:
        Execute the authentication flow.
        To dispatch a request, `yield` it:
        yield request
        The client will `.send()` the response back into the flow generator. You can
        access it like so:
        response = yield request
        A `return` (or reaching the end of the generator) will result in the
        client returning the last response obtained from the server.
        You can dispatch as many requests as is necessary.
    def sync_auth_flow(
    ) -> typing.Generator[Request, Response, None]:
        Execute the authentication flow synchronously.
        By default, this defers to `.auth_flow()`. You should override this method
        when the authentication scheme does I/O and/or uses concurrency primitives.
        if self.requires_request_body:
            request.read()
        flow = self.auth_flow(request)
        request = next(flow)
            if self.requires_response_body:
                response.read()
                request = flow.send(response)
    async def async_auth_flow(
    ) -> typing.AsyncGenerator[Request, Response]:
        Execute the authentication flow asynchronously.
            await request.aread()
                await response.aread()
class FunctionAuth(Auth):
    Allows the 'auth' argument to be passed as a simple callable function,
    that takes the request, and returns a new, modified request.
    def __init__(self, func: typing.Callable[[Request], Request]) -> None:
        self._func = func
        yield self._func(request)
class BasicAuth(Auth):
    Allows the 'auth' argument to be passed as a (username, password) pair,
    and uses HTTP Basic authentication.
    def __init__(self, username: str | bytes, password: str | bytes) -> None:
        self._auth_header = self._build_auth_header(username, password)
        request.headers["Authorization"] = self._auth_header
    def _build_auth_header(self, username: str | bytes, password: str | bytes) -> str:
        userpass = b":".join((to_bytes(username), to_bytes(password)))
        token = b64encode(userpass).decode()
        return f"Basic {token}"
class NetRCAuth(Auth):
    Use a 'netrc' file to lookup basic auth credentials based on the url host.
    def __init__(self, file: str | None = None) -> None:
        # Lazily import 'netrc'.
        # There's no need for us to load this module unless 'NetRCAuth' is being used.
        self._netrc_info = netrc.netrc(file)
        auth_info = self._netrc_info.authenticators(request.url.host)
        if auth_info is None or not auth_info[2]:
            # The netrc file did not have authentication credentials for this host.
            # Build a basic auth header with credentials from the netrc file.
            request.headers["Authorization"] = self._build_auth_header(
                username=auth_info[0], password=auth_info[2]
class DigestAuth(Auth):
    _ALGORITHM_TO_HASH_FUNCTION: dict[str, typing.Callable[[bytes], _Hash]] = {
        "MD5": hashlib.md5,
        "MD5-SESS": hashlib.md5,
        "SHA": hashlib.sha1,
        "SHA-SESS": hashlib.sha1,
        "SHA-256": hashlib.sha256,
        "SHA-256-SESS": hashlib.sha256,
        "SHA-512": hashlib.sha512,
        "SHA-512-SESS": hashlib.sha512,
        self._username = to_bytes(username)
        self._password = to_bytes(password)
        self._last_challenge: _DigestAuthChallenge | None = None
        self._nonce_count = 1
        if self._last_challenge:
                request, self._last_challenge
        if response.status_code != 401 or "www-authenticate" not in response.headers:
            # If the response is not a 401 then we don't
            # need to build an authenticated request.
        for auth_header in response.headers.get_list("www-authenticate"):
            if auth_header.lower().startswith("digest "):
            # If the response does not include a 'WWW-Authenticate: Digest ...'
            # header, then we don't need to build an authenticated request.
        self._last_challenge = self._parse_challenge(request, response, auth_header)
            Cookies(response.cookies).set_cookie_header(request=request)
    def _parse_challenge(
        self, request: Request, response: Response, auth_header: str
    ) -> _DigestAuthChallenge:
        Returns a challenge from a Digest WWW-Authenticate header.
        These take the form of:
        `Digest realm="realm@host.com",qop="auth,auth-int",nonce="abc",opaque="xyz"`
        scheme, _, fields = auth_header.partition(" ")
        # This method should only ever have been called with a Digest auth header.
        assert scheme.lower() == "digest"
        header_dict: dict[str, str] = {}
        for field in parse_http_list(fields):
            key, value = field.strip().split("=", 1)
            header_dict[key] = unquote(value)
            realm = header_dict["realm"].encode()
            nonce = header_dict["nonce"].encode()
            algorithm = header_dict.get("algorithm", "MD5")
            opaque = header_dict["opaque"].encode() if "opaque" in header_dict else None
            qop = header_dict["qop"].encode() if "qop" in header_dict else None
            return _DigestAuthChallenge(
                realm=realm, nonce=nonce, algorithm=algorithm, opaque=opaque, qop=qop
            message = "Malformed Digest WWW-Authenticate header"
            raise ProtocolError(message, request=request) from exc
    def _build_auth_header(
        self, request: Request, challenge: _DigestAuthChallenge
        hash_func = self._ALGORITHM_TO_HASH_FUNCTION[challenge.algorithm.upper()]
        def digest(data: bytes) -> bytes:
            return hash_func(data).hexdigest().encode()
        A1 = b":".join((self._username, challenge.realm, self._password))
        path = request.url.raw_path
        A2 = b":".join((request.method.encode(), path))
        # TODO: implement auth-int
        HA2 = digest(A2)
        nc_value = b"%08x" % self._nonce_count
        cnonce = self._get_client_nonce(self._nonce_count, challenge.nonce)
        self._nonce_count += 1
        HA1 = digest(A1)
        if challenge.algorithm.lower().endswith("-sess"):
            HA1 = digest(b":".join((HA1, challenge.nonce, cnonce)))
        qop = self._resolve_qop(challenge.qop, request=request)
        if qop is None:
            # Following RFC 2069
            digest_data = [HA1, challenge.nonce, HA2]
            # Following RFC 2617/7616
            digest_data = [HA1, challenge.nonce, nc_value, cnonce, qop, HA2]
        format_args = {
            "username": self._username,
            "realm": challenge.realm,
            "nonce": challenge.nonce,
            "uri": path,
            "response": digest(b":".join(digest_data)),
            "algorithm": challenge.algorithm.encode(),
        if challenge.opaque:
            format_args["opaque"] = challenge.opaque
            format_args["qop"] = b"auth"
            format_args["nc"] = nc_value
            format_args["cnonce"] = cnonce
        return "Digest " + self._get_header_value(format_args)
    def _get_client_nonce(self, nonce_count: int, nonce: bytes) -> bytes:
        s = str(nonce_count).encode()
        s += nonce
        s += time.ctime().encode()
        return hashlib.sha1(s).hexdigest()[:16].encode()
    def _get_header_value(self, header_fields: dict[str, bytes]) -> str:
        NON_QUOTED_FIELDS = ("algorithm", "qop", "nc")
        QUOTED_TEMPLATE = '{}="{}"'
        NON_QUOTED_TEMPLATE = "{}={}"
        header_value = ""
        for i, (field, value) in enumerate(header_fields.items()):
            if i > 0:
                header_value += ", "
                QUOTED_TEMPLATE
                if field not in NON_QUOTED_FIELDS
                else NON_QUOTED_TEMPLATE
            header_value += template.format(field, to_str(value))
        return header_value
    def _resolve_qop(self, qop: bytes | None, request: Request) -> bytes | None:
        qops = re.split(b", ?", qop)
        if b"auth" in qops:
            return b"auth"
        if qops == [b"auth-int"]:
            raise NotImplementedError("Digest auth-int support is not yet implemented")
        message = f'Unexpected qop value "{qop!r}" in digest auth'
        raise ProtocolError(message, request=request)
class _DigestAuthChallenge(typing.NamedTuple):
    realm: bytes
    nonce: bytes
    algorithm: str
    opaque: bytes | None
    qop: bytes | None
# Copyright 2023 The HuggingFace Team. All rights reserved.
"""Contains a helper to get the token from machine (env variable, secret or config file)."""
from .. import constants
from ._runtime import is_colab_enterprise, is_google_colab
_IS_GOOGLE_COLAB_CHECKED = False
_GOOGLE_COLAB_SECRET_LOCK = Lock()
_GOOGLE_COLAB_SECRET: Optional[str] = None
def get_token() -> Optional[str]:
    Get token if user is logged in.
    Note: in most cases, you should use [`huggingface_hub.utils.build_hf_headers`] instead. This method is only useful
          if you want to retrieve the token for other purposes than sending an HTTP request.
    Token is retrieved in priority from the `HF_TOKEN` environment variable. Otherwise, we read the token file located
    in the Hugging Face home folder. Returns None if user is not logged in. To log in, use [`login`] or
    `hf auth login`.
        `str` or `None`: The token, `None` if it doesn't exist.
    return _get_token_from_google_colab() or _get_token_from_environment() or _get_token_from_file()
def _get_token_from_google_colab() -> Optional[str]:
    """Get token from Google Colab secrets vault using `google.colab.userdata.get(...)`.
    Token is read from the vault only once per session and then stored in a global variable to avoid re-requesting
    access to the vault.
    # If it's not a Google Colab or it's Colab Enterprise, fallback to environment variable or token file authentication
    if not is_google_colab() or is_colab_enterprise():
    # `google.colab.userdata` is not thread-safe
    # This can lead to a deadlock if multiple threads try to access it at the same time
    # (typically when using `snapshot_download`)
    # => use a lock
    # See https://github.com/huggingface/huggingface_hub/issues/1952 for more details.
    with _GOOGLE_COLAB_SECRET_LOCK:
        global _GOOGLE_COLAB_SECRET
        global _IS_GOOGLE_COLAB_CHECKED
        if _IS_GOOGLE_COLAB_CHECKED:  # request access only once
            return _GOOGLE_COLAB_SECRET
            from google.colab import userdata  # type: ignore
            from google.colab.errors import Error as ColabError  # type: ignore
            token = userdata.get("HF_TOKEN")
            _GOOGLE_COLAB_SECRET = _clean_token(token)
        except userdata.NotebookAccessError:
            # Means the user has a secret call `HF_TOKEN` and got a popup "please grand access to HF_TOKEN" and refused it
            # => warn user but ignore error => do not re-request access to user
                "\nAccess to the secret `HF_TOKEN` has not been granted on this notebook."
                "\nYou will not be requested again."
                "\nPlease restart the session if you want to be prompted again."
            _GOOGLE_COLAB_SECRET = None
        except userdata.SecretNotFoundError:
            # Means the user did not define a `HF_TOKEN` secret => warn
                "\nThe secret `HF_TOKEN` does not exist in your Colab secrets."
                "\nTo authenticate with the Hugging Face Hub, create a token in your settings tab "
                "(https://huggingface.co/settings/tokens), set it as secret in your Google Colab and restart your session."
                "\nYou will be able to reuse this secret in all of your notebooks."
                "\nPlease note that authentication is recommended but still optional to access public models or datasets."
        except ColabError as e:
            # Something happen but we don't know what => recommend to open a GitHub issue
                f"\nError while fetching `HF_TOKEN` secret value from your vault: '{str(e)}'."
                "\nYou are not authenticated with the Hugging Face Hub in this notebook."
                "\nIf the error persists, please let us know by opening an issue on GitHub "
                "(https://github.com/huggingface/huggingface_hub/issues/new)."
        _IS_GOOGLE_COLAB_CHECKED = True
def _get_token_from_environment() -> Optional[str]:
    # `HF_TOKEN` has priority (keep `HUGGING_FACE_HUB_TOKEN` for backward compatibility)
    return _clean_token(os.environ.get("HF_TOKEN") or os.environ.get("HUGGING_FACE_HUB_TOKEN"))
def _get_token_from_file() -> Optional[str]:
        return _clean_token(Path(constants.HF_TOKEN_PATH).read_text())
def get_stored_tokens() -> dict[str, str]:
    Returns the parsed INI file containing the access tokens.
    The file is located at `HF_STORED_TOKENS_PATH`, defaulting to `~/.cache/huggingface/stored_tokens`.
    If the file does not exist, an empty dictionary is returned.
    Returns: `dict[str, str]`
        Key is the token name and value is the token.
    tokens_path = Path(constants.HF_STORED_TOKENS_PATH)
    if not tokens_path.exists():
        stored_tokens = {}
    config = configparser.ConfigParser()
        config.read(tokens_path)
        stored_tokens = {token_name: config.get(token_name, "hf_token") for token_name in config.sections()}
    except configparser.Error as e:
        logger.error(f"Error parsing stored tokens file: {e}")
    return stored_tokens
def _save_stored_tokens(stored_tokens: dict[str, str]) -> None:
    Saves the given configuration to the stored tokens file.
        stored_tokens (`dict[str, str]`):
            The stored tokens to save. Key is the token name and value is the token.
    stored_tokens_path = Path(constants.HF_STORED_TOKENS_PATH)
    # Write the stored tokens into an INI file
    for token_name in sorted(stored_tokens.keys()):
        config.add_section(token_name)
        config.set(token_name, "hf_token", stored_tokens[token_name])
    stored_tokens_path.parent.mkdir(parents=True, exist_ok=True)
    with stored_tokens_path.open("w") as config_file:
        config.write(config_file)
def _get_token_by_name(token_name: str) -> Optional[str]:
    Get the token by name.
        token_name (`str`):
            The name of the token to get.
    stored_tokens = get_stored_tokens()
    if token_name not in stored_tokens:
    return _clean_token(stored_tokens[token_name])
def _save_token(token: str, token_name: str) -> None:
    Save the given token.
    If the stored tokens file does not exist, it will be created.
        token (`str`):
            The token to save.
            The name of the token.
    stored_tokens[token_name] = token
    _save_stored_tokens(stored_tokens)
    logger.info(f"The token `{token_name}` has been saved to {tokens_path}")
def _clean_token(token: Optional[str]) -> Optional[str]:
    """Clean token by removing trailing and leading spaces and newlines.
    If token is an empty string, return None.
    return token.replace("\r", "").replace("\n", "").strip() or None
