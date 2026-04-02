from requests.auth import AuthBase, HTTPBasicAuth
from requests_oauthlib import OAuth1, OAuth1Session, OAuth2Session
from tweepy.errors import TweepyException
WARNING_MESSAGE = """Warning! Due to a Twitter API bug, signin_with_twitter
and access_type don't always play nice together. Details
https://dev.twitter.com/discussions/21281"""
class OAuth1UserHandler:
    """OAuth 1.0a User Context authentication handler
    .. versionchanged:: 4.5
        Renamed from :class:`OAuthHandler`
    def __init__(self, consumer_key, consumer_secret, access_token=None,
                 access_token_secret=None, callback=None):
        if not isinstance(consumer_key, (str, bytes)):
            raise TypeError("Consumer key must be string or bytes, not "
                            + type(consumer_key).__name__)
        if not isinstance(consumer_secret, (str, bytes)):
            raise TypeError("Consumer secret must be string or bytes, not "
                            + type(consumer_secret).__name__)
        self.consumer_key = consumer_key
        self.consumer_secret = consumer_secret
        self.access_token = access_token
        self.access_token_secret = access_token_secret
        self.callback = callback
        self.username = None
        self.request_token = {}
        self.oauth = OAuth1Session(consumer_key, client_secret=consumer_secret,
                                   callback_uri=self.callback)
    def apply_auth(self):
        return OAuth1(
            self.consumer_key, client_secret=self.consumer_secret,
            resource_owner_key=self.access_token,
            resource_owner_secret=self.access_token_secret, decoding=None
    def _get_oauth_url(self, endpoint):
        return 'https://api.twitter.com/oauth/' + endpoint
    def _get_request_token(self, access_type=None):
            url = self._get_oauth_url('request_token')
            if access_type:
                url += f'?x_auth_access_type={access_type}'
            return self.oauth.fetch_request_token(url)
            raise TweepyException(e)
    def get_authorization_url(self, signin_with_twitter=False,
                              access_type=None):
        """Get the authorization URL to redirect the user to"""
            if signin_with_twitter:
                url = self._get_oauth_url('authenticate')
                    log.warning(WARNING_MESSAGE)
                url = self._get_oauth_url('authorize')
            self.request_token = self._get_request_token(
                access_type=access_type
            return self.oauth.authorization_url(url)
    def get_access_token(self, verifier=None):
        """After user has authorized the app, get access token and secret with
        verifier
            url = self._get_oauth_url('access_token')
            self.oauth = OAuth1Session(
                resource_owner_key=self.request_token['oauth_token'],
                resource_owner_secret=self.request_token['oauth_token_secret'],
                verifier=verifier, callback_uri=self.callback
            resp = self.oauth.fetch_access_token(url)
            self.access_token = resp['oauth_token']
            self.access_token_secret = resp['oauth_token_secret']
            return self.access_token, self.access_token_secret
    def set_access_token(self, key, secret):
        .. deprecated:: 4.5
            Set through initialization instead.
        self.access_token = key
        self.access_token_secret = secret
class OAuthHandler(OAuth1UserHandler):
    """Alias for :class:`OAuth1UserHandler`
        Use :class:`OAuth1UserHandler` instead.
        warnings.warn(
            "OAuthHandler is deprecated; use OAuth1UserHandler instead.",
            DeprecationWarning
        super().__init__(consumer_key, consumer_secret, access_token, 
                         access_token_secret, callback)
class OAuth2AppHandler:
    """OAuth 2.0 Bearer Token (App-Only) using API / Consumer key and secret
    authentication handler
        Renamed from :class:`AppAuthHandler`
    def __init__(self, consumer_key, consumer_secret):
        self._bearer_token = ''
        resp = requests.post(
            'https://api.twitter.com/oauth2/token',
            auth=(self.consumer_key, self.consumer_secret),
            data={'grant_type': 'client_credentials'}
        data = resp.json()
        if data.get('token_type') != 'bearer':
            raise TweepyException('Expected token_type to equal "bearer", '
                                  f'but got {data.get("token_type")} instead')
        self._bearer_token = data['access_token']
        return OAuth2BearerHandler(self._bearer_token)
class AppAuthHandler(OAuth2AppHandler):
    """Alias for :class:`OAuth2AppHandler`
        Use :class:`OAuth2AppHandler` instead.
            "AppAuthHandler is deprecated; use OAuth2AppHandler instead.",
        super().__init__(consumer_key, consumer_secret)
class OAuth2BearerHandler(AuthBase):
    """OAuth 2.0 Bearer Token (App-Only) authentication handler
    .. versionadded:: 4.5
    def __init__(self, bearer_token):
        self.bearer_token = bearer_token
    def __call__(self, request):
        request.headers['Authorization'] = 'Bearer ' + self.bearer_token
        return request
class OAuth2UserHandler(OAuth2Session):
    """OAuth 2.0 Authorization Code Flow with PKCE (User Context)
    def __init__(self, *, client_id, redirect_uri, scope, client_secret=None):
        super().__init__(client_id, redirect_uri=redirect_uri, scope=scope)
        if client_secret is not None:
            self.auth = HTTPBasicAuth(client_id, client_secret)
            self.auth = None
    def get_authorization_url(self):
        authorization_url, state = self.authorization_url(
            "https://twitter.com/i/oauth2/authorize",
            code_challenge=self._client.create_code_challenge(
                self._client.create_code_verifier(128), "S256"
            ), code_challenge_method="S256"
        return authorization_url
    def fetch_token(self, authorization_response):
        """After user has authorized the app, fetch access token with
        authorization response URL
        return super().fetch_token(
            "https://api.twitter.com/2/oauth2/token",
            authorization_response=authorization_response,
            auth=self.auth,
            include_client_id=True,
            code_verifier=self._client.code_verifier
class AccessError(bb.Union):
    Error occurred because the account doesn't have permission to access the
    resource.
    :ivar InvalidAccountTypeError AccessError.invalid_account_type: Current
        account type cannot access the resource.
    :ivar PaperAccessError AccessError.paper_access_denied: Current account
        cannot access Paper.
    def invalid_account_type(cls, val):
        Create an instance of this class set to the ``invalid_account_type`` tag
        with value ``val``.
        :param InvalidAccountTypeError val:
        :rtype: AccessError
        return cls('invalid_account_type', val)
    def paper_access_denied(cls, val):
        Create an instance of this class set to the ``paper_access_denied`` tag
        :param PaperAccessError val:
        return cls('paper_access_denied', val)
    def is_invalid_account_type(self):
        Check if the union tag is ``invalid_account_type``.
        return self._tag == 'invalid_account_type'
    def is_paper_access_denied(self):
        Check if the union tag is ``paper_access_denied``.
        return self._tag == 'paper_access_denied'
    def get_invalid_account_type(self):
        Current account type cannot access the resource.
        Only call this if :meth:`is_invalid_account_type` is true.
        :rtype: InvalidAccountTypeError
        if not self.is_invalid_account_type():
            raise AttributeError("tag 'invalid_account_type' not set")
    def get_paper_access_denied(self):
        Current account cannot access Paper.
        Only call this if :meth:`is_paper_access_denied` is true.
        :rtype: PaperAccessError
        if not self.is_paper_access_denied():
            raise AttributeError("tag 'paper_access_denied' not set")
        super(AccessError, self)._process_custom_annotations(annotation_type, field_path, processor)
AccessError_validator = bv.Union(AccessError)
class AuthError(bb.Union):
    Errors occurred during authentication.
    :ivar auth.AuthError.invalid_access_token: The access token is invalid.
    :ivar auth.AuthError.invalid_select_user: The user specified in
        'Dropbox-API-Select-User' is no longer on the team.
    :ivar auth.AuthError.invalid_select_admin: The user specified in
        'Dropbox-API-Select-Admin' is not a Dropbox Business team admin.
    :ivar auth.AuthError.user_suspended: The user has been suspended.
    :ivar auth.AuthError.expired_access_token: The access token has expired.
    :ivar TokenScopeError AuthError.missing_scope: The access token does not
        have the required scope to access the route.
    :ivar auth.AuthError.route_access_denied: The route is not available to
        public.
    invalid_access_token = None
    invalid_select_user = None
    invalid_select_admin = None
    user_suspended = None
    expired_access_token = None
    route_access_denied = None
    def missing_scope(cls, val):
        Create an instance of this class set to the ``missing_scope`` tag with
        :param TokenScopeError val:
        :rtype: AuthError
        return cls('missing_scope', val)
    def is_invalid_access_token(self):
        Check if the union tag is ``invalid_access_token``.
        return self._tag == 'invalid_access_token'
    def is_invalid_select_user(self):
        Check if the union tag is ``invalid_select_user``.
        return self._tag == 'invalid_select_user'
    def is_invalid_select_admin(self):
        Check if the union tag is ``invalid_select_admin``.
        return self._tag == 'invalid_select_admin'
    def is_user_suspended(self):
        Check if the union tag is ``user_suspended``.
        return self._tag == 'user_suspended'
    def is_expired_access_token(self):
        Check if the union tag is ``expired_access_token``.
        return self._tag == 'expired_access_token'
    def is_missing_scope(self):
        Check if the union tag is ``missing_scope``.
        return self._tag == 'missing_scope'
    def is_route_access_denied(self):
        Check if the union tag is ``route_access_denied``.
        return self._tag == 'route_access_denied'
    def get_missing_scope(self):
        The access token does not have the required scope to access the route.
        Only call this if :meth:`is_missing_scope` is true.
        :rtype: TokenScopeError
        if not self.is_missing_scope():
            raise AttributeError("tag 'missing_scope' not set")
        super(AuthError, self)._process_custom_annotations(annotation_type, field_path, processor)
AuthError_validator = bv.Union(AuthError)
class InvalidAccountTypeError(bb.Union):
    :ivar auth.InvalidAccountTypeError.endpoint: Current account type doesn't
        have permission to access this route endpoint.
    :ivar auth.InvalidAccountTypeError.feature: Current account type doesn't
        have permission to access this feature.
    endpoint = None
    feature = None
    def is_endpoint(self):
        Check if the union tag is ``endpoint``.
        return self._tag == 'endpoint'
    def is_feature(self):
        Check if the union tag is ``feature``.
        return self._tag == 'feature'
        super(InvalidAccountTypeError, self)._process_custom_annotations(annotation_type, field_path, processor)
InvalidAccountTypeError_validator = bv.Union(InvalidAccountTypeError)
class PaperAccessError(bb.Union):
    :ivar auth.PaperAccessError.paper_disabled: Paper is disabled.
    :ivar auth.PaperAccessError.not_paper_user: The provided user has not used
        Paper yet.
    paper_disabled = None
    not_paper_user = None
    def is_paper_disabled(self):
        Check if the union tag is ``paper_disabled``.
        return self._tag == 'paper_disabled'
    def is_not_paper_user(self):
        Check if the union tag is ``not_paper_user``.
        return self._tag == 'not_paper_user'
        super(PaperAccessError, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperAccessError_validator = bv.Union(PaperAccessError)
class RateLimitError(bb.Struct):
    Error occurred because the app is being rate limited.
    :ivar auth.RateLimitError.reason: The reason why the app is being rate
        limited.
    :ivar auth.RateLimitError.retry_after: The number of seconds that the app
        should wait before making another request.
        '_reason_value',
        '_retry_after_value',
                 reason=None,
                 retry_after=None):
        self._reason_value = bb.NOT_SET
        self._retry_after_value = bb.NOT_SET
        if reason is not None:
        if retry_after is not None:
            self.retry_after = retry_after
    # Instance attribute type: RateLimitReason (validator is set below)
    reason = bb.Attribute("reason", user_defined=True)
    # Instance attribute type: int (validator is set below)
    retry_after = bb.Attribute("retry_after")
        super(RateLimitError, self)._process_custom_annotations(annotation_type, field_path, processor)
RateLimitError_validator = bv.Struct(RateLimitError)
class RateLimitReason(bb.Union):
    :ivar auth.RateLimitReason.too_many_requests: You are making too many
        requests in the past few minutes.
    :ivar auth.RateLimitReason.too_many_write_operations: There are currently
        too many write operations happening in the user's Dropbox.
    too_many_requests = None
    too_many_write_operations = None
    def is_too_many_requests(self):
        Check if the union tag is ``too_many_requests``.
        return self._tag == 'too_many_requests'
    def is_too_many_write_operations(self):
        Check if the union tag is ``too_many_write_operations``.
        return self._tag == 'too_many_write_operations'
        super(RateLimitReason, self)._process_custom_annotations(annotation_type, field_path, processor)
RateLimitReason_validator = bv.Union(RateLimitReason)
class TokenFromOAuth1Arg(bb.Struct):
    :ivar auth.TokenFromOAuth1Arg.oauth1_token: The supplied OAuth 1.0 access
        token.
    :ivar auth.TokenFromOAuth1Arg.oauth1_token_secret: The token secret
        associated with the supplied access token.
        '_oauth1_token_value',
        '_oauth1_token_secret_value',
                 oauth1_token=None,
                 oauth1_token_secret=None):
        self._oauth1_token_value = bb.NOT_SET
        self._oauth1_token_secret_value = bb.NOT_SET
        if oauth1_token is not None:
            self.oauth1_token = oauth1_token
        if oauth1_token_secret is not None:
            self.oauth1_token_secret = oauth1_token_secret
    oauth1_token = bb.Attribute("oauth1_token")
    oauth1_token_secret = bb.Attribute("oauth1_token_secret")
        super(TokenFromOAuth1Arg, self)._process_custom_annotations(annotation_type, field_path, processor)
TokenFromOAuth1Arg_validator = bv.Struct(TokenFromOAuth1Arg)
class TokenFromOAuth1Error(bb.Union):
    :ivar auth.TokenFromOAuth1Error.invalid_oauth1_token_info: Part or all of
        the OAuth 1.0 access token info is invalid.
    :ivar auth.TokenFromOAuth1Error.app_id_mismatch: The authorized app does not
        match the app associated with the supplied access token.
    invalid_oauth1_token_info = None
    app_id_mismatch = None
    def is_invalid_oauth1_token_info(self):
        Check if the union tag is ``invalid_oauth1_token_info``.
        return self._tag == 'invalid_oauth1_token_info'
    def is_app_id_mismatch(self):
        Check if the union tag is ``app_id_mismatch``.
        return self._tag == 'app_id_mismatch'
        super(TokenFromOAuth1Error, self)._process_custom_annotations(annotation_type, field_path, processor)
TokenFromOAuth1Error_validator = bv.Union(TokenFromOAuth1Error)
class TokenFromOAuth1Result(bb.Struct):
    :ivar auth.TokenFromOAuth1Result.oauth2_token: The OAuth 2.0 token generated
        from the supplied OAuth 1.0 token.
        '_oauth2_token_value',
                 oauth2_token=None):
        self._oauth2_token_value = bb.NOT_SET
        if oauth2_token is not None:
            self.oauth2_token = oauth2_token
    oauth2_token = bb.Attribute("oauth2_token")
        super(TokenFromOAuth1Result, self)._process_custom_annotations(annotation_type, field_path, processor)
TokenFromOAuth1Result_validator = bv.Struct(TokenFromOAuth1Result)
class TokenScopeError(bb.Struct):
    :ivar auth.TokenScopeError.required_scope: The required scope to access the
        route.
        '_required_scope_value',
                 required_scope=None):
        self._required_scope_value = bb.NOT_SET
        if required_scope is not None:
            self.required_scope = required_scope
    required_scope = bb.Attribute("required_scope")
        super(TokenScopeError, self)._process_custom_annotations(annotation_type, field_path, processor)
TokenScopeError_validator = bv.Struct(TokenScopeError)
AccessError._invalid_account_type_validator = InvalidAccountTypeError_validator
AccessError._paper_access_denied_validator = PaperAccessError_validator
AccessError._other_validator = bv.Void()
AccessError._tagmap = {
    'invalid_account_type': AccessError._invalid_account_type_validator,
    'paper_access_denied': AccessError._paper_access_denied_validator,
    'other': AccessError._other_validator,
AccessError.other = AccessError('other')
AuthError._invalid_access_token_validator = bv.Void()
AuthError._invalid_select_user_validator = bv.Void()
AuthError._invalid_select_admin_validator = bv.Void()
AuthError._user_suspended_validator = bv.Void()
AuthError._expired_access_token_validator = bv.Void()
AuthError._missing_scope_validator = TokenScopeError_validator
AuthError._route_access_denied_validator = bv.Void()
AuthError._other_validator = bv.Void()
AuthError._tagmap = {
    'invalid_access_token': AuthError._invalid_access_token_validator,
    'invalid_select_user': AuthError._invalid_select_user_validator,
    'invalid_select_admin': AuthError._invalid_select_admin_validator,
    'user_suspended': AuthError._user_suspended_validator,
    'expired_access_token': AuthError._expired_access_token_validator,
    'missing_scope': AuthError._missing_scope_validator,
    'route_access_denied': AuthError._route_access_denied_validator,
    'other': AuthError._other_validator,
AuthError.invalid_access_token = AuthError('invalid_access_token')
AuthError.invalid_select_user = AuthError('invalid_select_user')
AuthError.invalid_select_admin = AuthError('invalid_select_admin')
AuthError.user_suspended = AuthError('user_suspended')
AuthError.expired_access_token = AuthError('expired_access_token')
AuthError.route_access_denied = AuthError('route_access_denied')
AuthError.other = AuthError('other')
InvalidAccountTypeError._endpoint_validator = bv.Void()
InvalidAccountTypeError._feature_validator = bv.Void()
InvalidAccountTypeError._other_validator = bv.Void()
InvalidAccountTypeError._tagmap = {
    'endpoint': InvalidAccountTypeError._endpoint_validator,
    'feature': InvalidAccountTypeError._feature_validator,
    'other': InvalidAccountTypeError._other_validator,
InvalidAccountTypeError.endpoint = InvalidAccountTypeError('endpoint')
InvalidAccountTypeError.feature = InvalidAccountTypeError('feature')
InvalidAccountTypeError.other = InvalidAccountTypeError('other')
PaperAccessError._paper_disabled_validator = bv.Void()
PaperAccessError._not_paper_user_validator = bv.Void()
PaperAccessError._other_validator = bv.Void()
PaperAccessError._tagmap = {
    'paper_disabled': PaperAccessError._paper_disabled_validator,
    'not_paper_user': PaperAccessError._not_paper_user_validator,
    'other': PaperAccessError._other_validator,
PaperAccessError.paper_disabled = PaperAccessError('paper_disabled')
PaperAccessError.not_paper_user = PaperAccessError('not_paper_user')
PaperAccessError.other = PaperAccessError('other')
RateLimitError.reason.validator = RateLimitReason_validator
RateLimitError.retry_after.validator = bv.UInt64()
RateLimitError._all_field_names_ = set([
    'reason',
    'retry_after',
RateLimitError._all_fields_ = [
    ('reason', RateLimitError.reason.validator),
    ('retry_after', RateLimitError.retry_after.validator),
RateLimitReason._too_many_requests_validator = bv.Void()
RateLimitReason._too_many_write_operations_validator = bv.Void()
RateLimitReason._other_validator = bv.Void()
RateLimitReason._tagmap = {
    'too_many_requests': RateLimitReason._too_many_requests_validator,
    'too_many_write_operations': RateLimitReason._too_many_write_operations_validator,
    'other': RateLimitReason._other_validator,
RateLimitReason.too_many_requests = RateLimitReason('too_many_requests')
RateLimitReason.too_many_write_operations = RateLimitReason('too_many_write_operations')
RateLimitReason.other = RateLimitReason('other')
TokenFromOAuth1Arg.oauth1_token.validator = bv.String(min_length=1)
TokenFromOAuth1Arg.oauth1_token_secret.validator = bv.String(min_length=1)
TokenFromOAuth1Arg._all_field_names_ = set([
    'oauth1_token',
    'oauth1_token_secret',
TokenFromOAuth1Arg._all_fields_ = [
    ('oauth1_token', TokenFromOAuth1Arg.oauth1_token.validator),
    ('oauth1_token_secret', TokenFromOAuth1Arg.oauth1_token_secret.validator),
TokenFromOAuth1Error._invalid_oauth1_token_info_validator = bv.Void()
TokenFromOAuth1Error._app_id_mismatch_validator = bv.Void()
TokenFromOAuth1Error._other_validator = bv.Void()
TokenFromOAuth1Error._tagmap = {
    'invalid_oauth1_token_info': TokenFromOAuth1Error._invalid_oauth1_token_info_validator,
    'app_id_mismatch': TokenFromOAuth1Error._app_id_mismatch_validator,
    'other': TokenFromOAuth1Error._other_validator,
TokenFromOAuth1Error.invalid_oauth1_token_info = TokenFromOAuth1Error('invalid_oauth1_token_info')
TokenFromOAuth1Error.app_id_mismatch = TokenFromOAuth1Error('app_id_mismatch')
TokenFromOAuth1Error.other = TokenFromOAuth1Error('other')
TokenFromOAuth1Result.oauth2_token.validator = bv.String(min_length=1)
TokenFromOAuth1Result._all_field_names_ = set(['oauth2_token'])
TokenFromOAuth1Result._all_fields_ = [('oauth2_token', TokenFromOAuth1Result.oauth2_token.validator)]
TokenScopeError.required_scope.validator = bv.String()
TokenScopeError._all_field_names_ = set(['required_scope'])
TokenScopeError._all_fields_ = [('required_scope', TokenScopeError.required_scope.validator)]
RateLimitError.retry_after.default = 1
token_from_oauth1 = bb.Route(
    'token/from_oauth1',
    TokenFromOAuth1Arg_validator,
    TokenFromOAuth1Result_validator,
    TokenFromOAuth1Error_validator,
    {'auth': 'app',
token_revoke = bb.Route(
    'token/revoke',
    bv.Void(),
    'token/from_oauth1': token_from_oauth1,
    'token/revoke': token_revoke,
from django.contrib.auth.hashers import UNUSABLE_PASSWORD_PREFIX, identify_hasher
from django.template import Library
from django.utils.html import format_html, format_html_join
register = Library()
@register.simple_tag
def render_password_as_hash(value):
    if not value or value.startswith(UNUSABLE_PASSWORD_PREFIX):
        return format_html("<p><strong>{}</strong></p>", gettext("No password set."))
        hasher = identify_hasher(value)
        hashed_summary = hasher.safe_summary(value)
            "<p><strong>{}</strong></p>",
            gettext("Invalid password format or unknown hashing algorithm."),
    items = [(gettext(key), val) for key, val in hashed_summary.items()]
        "<p>{}</p>",
        format_html_join(" ", "<strong>{}</strong>: <bdi>{}</bdi>", items),
"""Network Authentication Helpers
Contains interface (MultiDomainBasicAuth) and associated glue code for
providing credentials in the context of network requests.
from os.path import commonprefix
from pip._vendor.requests.auth import AuthBase, HTTPBasicAuth
from pip._vendor.requests.utils import get_netrc_auth
from pip._internal.utils.misc import (
    ask,
    ask_input,
    ask_password,
    remove_auth_from_url,
    split_auth_netloc_from_url,
from pip._internal.vcs.versioncontrol import AuthInfo
    from pip._vendor.requests import PreparedRequest
    from pip._vendor.requests.models import Response
KEYRING_DISABLED = False
class Credentials(NamedTuple):
    username: str
    password: str
class KeyRingBaseProvider(ABC):
    """Keyring base provider interface"""
    has_keyring: bool
    def get_auth_info(self, url: str, username: str | None) -> AuthInfo | None: ...
    def save_auth_info(self, url: str, username: str, password: str) -> None: ...
class KeyRingNullProvider(KeyRingBaseProvider):
    """Keyring null provider"""
    has_keyring = False
    def get_auth_info(self, url: str, username: str | None) -> AuthInfo | None:
    def save_auth_info(self, url: str, username: str, password: str) -> None:
class KeyRingPythonProvider(KeyRingBaseProvider):
    """Keyring interface which uses locally imported `keyring`"""
    has_keyring = True
        import keyring
        self.keyring = keyring
        # Support keyring's get_credential interface which supports getting
        # credentials without a username. This is only available for
        # keyring>=15.2.0.
        if hasattr(self.keyring, "get_credential"):
            logger.debug("Getting credentials from keyring for %s", url)
            cred = self.keyring.get_credential(url, username)
            if cred is not None:
                return cred.username, cred.password
            logger.debug("Getting password from keyring for %s", url)
            password = self.keyring.get_password(url, username)
        self.keyring.set_password(url, username, password)
class KeyRingCliProvider(KeyRingBaseProvider):
    """Provider which uses `keyring` cli
    Instead of calling the keyring package installed alongside pip
    we call keyring on the command line which will enable pip to
    use which ever installation of keyring is available first in
    PATH.
    def __init__(self, cmd: str) -> None:
        self.keyring = cmd
        # This is the default implementation of keyring.get_credential
        # https://github.com/jaraco/keyring/blob/97689324abcf01bd1793d49063e7ca01e03d7d07/keyring/backend.py#L134-L139
            password = self._get_password(url, username)
        return self._set_password(url, username, password)
    def _get_password(self, service_name: str, username: str) -> str | None:
        """Mirror the implementation of keyring.get_password using cli"""
        if self.keyring is None:
        cmd = [self.keyring, "get", service_name, username]
        env["PYTHONIOENCODING"] = "utf-8"
        res = subprocess.run(
            env=env,
        if res.returncode:
        return res.stdout.decode("utf-8").strip(os.linesep)
    def _set_password(self, service_name: str, username: str, password: str) -> None:
        """Mirror the implementation of keyring.set_password using cli"""
            [self.keyring, "set", service_name, username],
            input=f"{password}{os.linesep}".encode(),
def get_keyring_provider(provider: str) -> KeyRingBaseProvider:
    logger.verbose("Keyring provider requested: %s", provider)
    # keyring has previously failed and been disabled
    if KEYRING_DISABLED:
        provider = "disabled"
    if provider in ["import", "auto"]:
            impl = KeyRingPythonProvider()
            logger.verbose("Keyring provider set: import")
            return impl
            # In the event of an unexpected exception
            # we should warn the user
            msg = "Installed copy of keyring fails with exception %s"
            if provider == "auto":
                msg = msg + ", trying to find a keyring executable as a fallback"
            logger.warning(msg, exc, exc_info=logger.isEnabledFor(logging.DEBUG))
    if provider in ["subprocess", "auto"]:
        cli = shutil.which("keyring")
        if cli and cli.startswith(sysconfig.get_path("scripts")):
            # all code within this function is stolen from shutil.which implementation
            @typing.no_type_check
            def PATH_as_shutil_which_determines_it() -> str:
                path = os.environ.get("PATH", None)
                        path = os.confstr("CS_PATH")
                        # os.confstr() or CS_PATH is not available
                        path = os.defpath
                # bpo-35755: Don't use os.defpath if the PATH environment variable is
                # set to an empty string
            scripts = Path(sysconfig.get_path("scripts"))
            paths = []
            for path in PATH_as_shutil_which_determines_it().split(os.pathsep):
                p = Path(path)
                    if not p.samefile(scripts):
                        paths.append(path)
            path = os.pathsep.join(paths)
            cli = shutil.which("keyring", path=path)
        if cli:
            logger.verbose("Keyring provider set: subprocess with executable %s", cli)
            return KeyRingCliProvider(cli)
    logger.verbose("Keyring provider set: disabled")
    return KeyRingNullProvider()
class MultiDomainBasicAuth(AuthBase):
        prompting: bool = True,
        index_urls: list[str] | None = None,
        keyring_provider: str = "auto",
        self.prompting = prompting
        self.index_urls = index_urls
        self.keyring_provider = keyring_provider
        self.passwords: dict[str, AuthInfo] = {}
        # When the user is prompted to enter credentials and keyring is
        # available, we will offer to save them. If the user accepts,
        # this value is set to the credentials they entered. After the
        # request authenticates, the caller should call
        # ``save_credentials`` to save these.
        self._credentials_to_save: Credentials | None = None
    def keyring_provider(self) -> KeyRingBaseProvider:
        return get_keyring_provider(self._keyring_provider)
    @keyring_provider.setter
    def keyring_provider(self, provider: str) -> None:
        # The free function get_keyring_provider has been decorated with
        # functools.cache. If an exception occurs in get_keyring_auth that
        # cache will be cleared and keyring disabled, take that into account
        # if you want to remove this indirection.
        self._keyring_provider = provider
    def use_keyring(self) -> bool:
        # We won't use keyring when --no-input is passed unless
        # a specific provider is requested because it might require
        # user interaction
        return self.prompting or self._keyring_provider not in ["auto", "disabled"]
    def _get_keyring_auth(
        username: str | None,
    ) -> AuthInfo | None:
        """Return the tuple auth for a given url from keyring."""
        # Do nothing if no url was provided
            return self.keyring_provider.get_auth_info(url, username)
            # Log the full exception (with stacktrace) at debug, so it'll only
            # show up when running in verbose mode.
            logger.debug("Keyring is skipped due to an exception", exc_info=True)
            # Always log a shortened version of the exception.
                "Keyring is skipped due to an exception: %s",
                str(exc),
            global KEYRING_DISABLED
            KEYRING_DISABLED = True
            get_keyring_provider.cache_clear()
    def _get_index_url(self, url: str) -> str | None:
        """Return the original index URL matching the requested URL.
        Cached or dynamically generated credentials may work against
        the original index URL rather than just the netloc.
        The provided url should have had its username and password
        removed already. If the original index url had credentials then
        they will be included in the return value.
        Returns None if no matching index was found, or if --no-index
        was specified by the user.
        if not url or not self.index_urls:
        url = remove_auth_from_url(url).rstrip("/") + "/"
        parsed_url = urllib.parse.urlsplit(url)
        for index in self.index_urls:
            index = index.rstrip("/") + "/"
            parsed_index = urllib.parse.urlsplit(remove_auth_from_url(index))
            if parsed_url == parsed_index:
                return index
            if parsed_url.netloc != parsed_index.netloc:
            candidate = urllib.parse.urlsplit(index)
            candidates.append(candidate)
        candidates.sort(
            key=lambda candidate: commonprefix(
                    parsed_url.path,
                    candidate.path,
            ).rfind("/"),
        return urllib.parse.urlunsplit(candidates[0])
    def _get_new_credentials(
        original_url: str,
        allow_netrc: bool = True,
        allow_keyring: bool = False,
    ) -> AuthInfo:
        """Find and return credentials for the specified URL."""
        # Split the credentials and netloc from the url.
        url, netloc, url_user_password = split_auth_netloc_from_url(
            original_url,
        # Start with the credentials embedded in the url
        username, password = url_user_password
        if username is not None and password is not None:
            logger.debug("Found credentials in url for %s", netloc)
            return url_user_password
        # Find a matching index url for this request
        index_url = self._get_index_url(url)
        if index_url:
            # Split the credentials from the url.
            index_info = split_auth_netloc_from_url(index_url)
            if index_info:
                index_url, _, index_url_user_password = index_info
                logger.debug("Found index url %s", index_url)
        # If an index URL was found, try its embedded credentials
        if index_url and index_url_user_password[0] is not None:
            username, password = index_url_user_password
                logger.debug("Found credentials in index url for %s", netloc)
                return index_url_user_password
        # Get creds from netrc if we still don't have them
        if allow_netrc:
            netrc_auth = get_netrc_auth(original_url)
            if netrc_auth:
                logger.debug("Found credentials in netrc for %s", netloc)
                return netrc_auth
        # If we don't have a password and keyring is available, use it.
        if allow_keyring:
            # The index url is more specific than the netloc, so try it first
            kr_auth = (
                self._get_keyring_auth(index_url, username) or
                self._get_keyring_auth(netloc, username)
            if kr_auth:
                logger.debug("Found credentials in keyring for %s", netloc)
                return kr_auth
    def _get_url_and_credentials(
        self, original_url: str
    ) -> tuple[str, str | None, str | None]:
        """Return the credentials to use for the provided URL.
        If allowed, netrc and keyring may be used to obtain the
        correct credentials.
        Returns (url_without_credentials, username, password). Note
        that even if the original URL contains credentials, this
        function may return a different username and password.
        url, netloc, _ = split_auth_netloc_from_url(original_url)
        # Try to get credentials from original url
        username, password = self._get_new_credentials(original_url)
        # If credentials not found, use any stored credentials for this netloc.
        # Do this if either the username or the password is missing.
        # This accounts for the situation in which the user has specified
        # the username in the index url, but the password comes from keyring.
        if (username is None or password is None) and netloc in self.passwords:
            un, pw = self.passwords[netloc]
            # It is possible that the cached credentials are for a different username,
            # in which case the cache should be ignored.
            if username is None or username == un:
                username, password = un, pw
        if username is not None or password is not None:
            # Convert the username and password if they're None, so that
            # this netloc will show up as "cached" in the conditional above.
            # Further, HTTPBasicAuth doesn't accept None, so it makes sense to
            # cache the value that is going to be used.
            username = username or ""
            password = password or ""
            # Store any acquired credentials.
            self.passwords[netloc] = (username, password)
            # Credentials were found
            (username is not None and password is not None)
            # Credentials were not found
            or (username is None and password is None)
        ), f"Could not load credentials from url: {original_url}"
        return url, username, password
    def __call__(self, req: PreparedRequest) -> PreparedRequest:
        # Get credentials for this request
        assert req.url is not None
        url, username, password = self._get_url_and_credentials(req.url)
        # Set the url of the request to the url without any credentials
        req.url = url
            # Send the basic auth with this request
            req = HTTPBasicAuth(username, password)(req)
        # Attach a hook to handle 401 responses
        req.register_hook("response", self.handle_401)
    # Factored out to allow for easy patching in tests
    def _prompt_for_password(self, netloc: str) -> tuple[str | None, str | None, bool]:
        username = ask_input(f"User for {netloc}: ") if self.prompting else None
            return None, None, False
        if self.use_keyring:
            auth = self._get_keyring_auth(netloc, username)
            if auth and auth[0] is not None and auth[1] is not None:
                return auth[0], auth[1], False
        password = ask_password("Password: ")
        return username, password, True
    def _should_save_password_to_keyring(self) -> bool:
            not self.prompting
            or not self.use_keyring
            or not self.keyring_provider.has_keyring
        return ask("Save credentials to keyring [y/N]: ", ["y", "n"]) == "y"
    def handle_401(self, resp: Response, **kwargs: Any) -> Response:
        # We only care about 401 responses, anything else we want to just
        #   pass through the actual response
        if resp.status_code != 401:
        username, password = None, None
        # Query the keyring for credentials:
            username, password = self._get_new_credentials(
                resp.url,
                allow_netrc=False,
                allow_keyring=True,
        # We are not able to prompt the user so simply return the response
        if not self.prompting and not username and not password:
        parsed = urllib.parse.urlparse(resp.url)
        # Prompt the user for a new username and password
        save = False
        if not username and not password:
            username, password, save = self._prompt_for_password(parsed.netloc)
        # Store the new username and password to use for future requests
        self._credentials_to_save = None
            self.passwords[parsed.netloc] = (username, password)
            # Prompt to save the password to keyring
            if save and self._should_save_password_to_keyring():
                self._credentials_to_save = Credentials(
                    url=parsed.netloc,
                    username=username,
                    password=password,
        # Consume content and release the original connection to allow our new
        #   request to reuse the same one.
        # The result of the assignment isn't used, it's just needed to consume
        # the content.
        _ = resp.content
        resp.raw.release_conn()
        # Add our new username and password to the request
        req = HTTPBasicAuth(username or "", password or "")(resp.request)
        req.register_hook("response", self.warn_on_401)
        # On successful request, save the credentials that were used to
        # keyring. (Note that if the user responded "no" above, this member
        # is not set and nothing will be saved.)
        if self._credentials_to_save:
            req.register_hook("response", self.save_credentials)
        # Send our new request
        new_resp = resp.connection.send(req, **kwargs)
        new_resp.history.append(resp)
        return new_resp
    def warn_on_401(self, resp: Response, **kwargs: Any) -> None:
        """Response callback to warn about incorrect credentials."""
                "401 Error, Credentials not correct for %s",
                resp.request.url,
    def save_credentials(self, resp: Response, **kwargs: Any) -> None:
        """Response callback to save credentials on success."""
            self.keyring_provider.has_keyring
        ), "should never reach here without keyring"
        creds = self._credentials_to_save
        if creds and resp.status_code < 400:
                logger.info("Saving credentials to keyring")
                self.keyring_provider.save_auth_info(
                    creds.url, creds.username, creds.password
                logger.exception("Failed to save credentials")
requests.auth
This module contains the authentication handlers for Requests.
from base64 import b64encode
from ._internal_utils import to_native_string
from .compat import basestring, str, urlparse
from .cookies import extract_cookies_to_jar
from .utils import parse_dict_header
CONTENT_TYPE_FORM_URLENCODED = "application/x-www-form-urlencoded"
CONTENT_TYPE_MULTI_PART = "multipart/form-data"
def _basic_auth_str(username, password):
    """Returns a Basic Auth string."""
    # "I want us to put a big-ol' comment on top of it that
    # says that this behaviour is dumb but we need to preserve
    # it because people are relying on it."
    #    - Lukasa
    # These are here solely to maintain backwards compatibility
    # for things like ints. This will be removed in 3.0.0.
    if not isinstance(username, basestring):
            "Non-string usernames will no longer be supported in Requests "
            "3.0.0. Please convert the object you've passed in ({!r}) to "
            "a string or bytes object in the near future to avoid "
            "problems.".format(username),
        username = str(username)
    if not isinstance(password, basestring):
            "Non-string passwords will no longer be supported in Requests "
            "problems.".format(type(password)),
        password = str(password)
    # -- End Removal --
    if isinstance(username, str):
        username = username.encode("latin1")
    if isinstance(password, str):
        password = password.encode("latin1")
    authstr = "Basic " + to_native_string(
        b64encode(b":".join((username, password))).strip()
    return authstr
class AuthBase:
    """Base class that all auth implementations derive from"""
    def __call__(self, r):
        raise NotImplementedError("Auth hooks must be callable.")
class HTTPBasicAuth(AuthBase):
    """Attaches HTTP Basic Authentication to the given Request object."""
    def __init__(self, username, password):
                self.username == getattr(other, "username", None),
                self.password == getattr(other, "password", None),
        r.headers["Authorization"] = _basic_auth_str(self.username, self.password)
class HTTPProxyAuth(HTTPBasicAuth):
    """Attaches HTTP Proxy Authentication to a given Request object."""
        r.headers["Proxy-Authorization"] = _basic_auth_str(self.username, self.password)
class HTTPDigestAuth(AuthBase):
    """Attaches HTTP Digest Authentication to the given Request object."""
        # Keep state in per-thread local storage
        self._thread_local = threading.local()
    def init_per_thread_state(self):
        # Ensure state is initialized just once per-thread
        if not hasattr(self._thread_local, "init"):
            self._thread_local.init = True
            self._thread_local.last_nonce = ""
            self._thread_local.nonce_count = 0
            self._thread_local.chal = {}
            self._thread_local.pos = None
            self._thread_local.num_401_calls = None
    def build_digest_header(self, method, url):
        realm = self._thread_local.chal["realm"]
        nonce = self._thread_local.chal["nonce"]
        qop = self._thread_local.chal.get("qop")
        algorithm = self._thread_local.chal.get("algorithm")
        opaque = self._thread_local.chal.get("opaque")
        hash_utf8 = None
        if algorithm is None:
            _algorithm = "MD5"
            _algorithm = algorithm.upper()
        # lambdas assume digest modules are imported at the top level
        if _algorithm == "MD5" or _algorithm == "MD5-SESS":
            def md5_utf8(x):
                if isinstance(x, str):
                return hashlib.md5(x).hexdigest()
            hash_utf8 = md5_utf8
        elif _algorithm == "SHA":
            def sha_utf8(x):
                return hashlib.sha1(x).hexdigest()
            hash_utf8 = sha_utf8
        elif _algorithm == "SHA-256":
            def sha256_utf8(x):
                return hashlib.sha256(x).hexdigest()
            hash_utf8 = sha256_utf8
        elif _algorithm == "SHA-512":
            def sha512_utf8(x):
                return hashlib.sha512(x).hexdigest()
            hash_utf8 = sha512_utf8
        KD = lambda s, d: hash_utf8(f"{s}:{d}")  # noqa:E731
        if hash_utf8 is None:
        # XXX not implemented yet
        entdig = None
        p_parsed = urlparse(url)
        #: path is request-uri defined in RFC 2616 which should not be empty
        path = p_parsed.path or "/"
        if p_parsed.query:
            path += f"?{p_parsed.query}"
        A1 = f"{self.username}:{realm}:{self.password}"
        A2 = f"{method}:{path}"
        HA1 = hash_utf8(A1)
        HA2 = hash_utf8(A2)
        if nonce == self._thread_local.last_nonce:
            self._thread_local.nonce_count += 1
            self._thread_local.nonce_count = 1
        ncvalue = f"{self._thread_local.nonce_count:08x}"
        s = str(self._thread_local.nonce_count).encode("utf-8")
        s += nonce.encode("utf-8")
        s += time.ctime().encode("utf-8")
        s += os.urandom(8)
        cnonce = hashlib.sha1(s).hexdigest()[:16]
        if _algorithm == "MD5-SESS":
            HA1 = hash_utf8(f"{HA1}:{nonce}:{cnonce}")
        if not qop:
            respdig = KD(HA1, f"{nonce}:{HA2}")
        elif qop == "auth" or "auth" in qop.split(","):
            noncebit = f"{nonce}:{ncvalue}:{cnonce}:auth:{HA2}"
            respdig = KD(HA1, noncebit)
            # XXX handle auth-int.
        self._thread_local.last_nonce = nonce
        # XXX should the partial digests be encoded too?
        base = (
            f'username="{self.username}", realm="{realm}", nonce="{nonce}", '
            f'uri="{path}", response="{respdig}"'
        if opaque:
            base += f', opaque="{opaque}"'
        if algorithm:
            base += f', algorithm="{algorithm}"'
        if entdig:
            base += f', digest="{entdig}"'
        if qop:
            base += f', qop="auth", nc={ncvalue}, cnonce="{cnonce}"'
        return f"Digest {base}"
    def handle_redirect(self, r, **kwargs):
        """Reset num_401_calls counter on redirects."""
        if r.is_redirect:
            self._thread_local.num_401_calls = 1
    def handle_401(self, r, **kwargs):
        Takes the given response and tries digest-auth, if needed.
        # If response is not 4xx, do not auth
        # See https://github.com/psf/requests/issues/3772
        if not 400 <= r.status_code < 500:
        if self._thread_local.pos is not None:
            # Rewind the file position indicator of the body to where
            # it was to resend the request.
            r.request.body.seek(self._thread_local.pos)
        s_auth = r.headers.get("www-authenticate", "")
        if "digest" in s_auth.lower() and self._thread_local.num_401_calls < 2:
            self._thread_local.num_401_calls += 1
            pat = re.compile(r"digest ", flags=re.IGNORECASE)
            self._thread_local.chal = parse_dict_header(pat.sub("", s_auth, count=1))
            # Consume content and release the original connection
            # to allow our new request to reuse the same one.
            r.content
            r.close()
            prep = r.request.copy()
            extract_cookies_to_jar(prep._cookies, r.request, r.raw)
            prep.prepare_cookies(prep._cookies)
            prep.headers["Authorization"] = self.build_digest_header(
                prep.method, prep.url
            _r = r.connection.send(prep, **kwargs)
            _r.history.append(r)
            _r.request = prep
            return _r
        # Initialize per-thread state, if needed
        self.init_per_thread_state()
        # If we have a saved nonce, skip the 401
        if self._thread_local.last_nonce:
            r.headers["Authorization"] = self.build_digest_header(r.method, r.url)
            self._thread_local.pos = r.body.tell()
            # In the case of HTTPDigestAuth being reused and the body of
            # the previous request was a file-like object, pos has the
            # file position of the previous body. Ensure it's set to
            # None.
        r.register_hook("response", self.handle_401)
        r.register_hook("response", self.handle_redirect)
"""Contains commands to authenticate to the Hugging Face Hub and interact with your repositories.
    # login and save token locally.
    hf auth login --token=hf_*** --add-to-git-credential
    # switch between tokens
    hf auth switch
    # list all tokens
    hf auth list
    # logout from all tokens
    hf auth logout
    # check which account you are logged in as
    hf auth whoami
from huggingface_hub.constants import ENDPOINT
from huggingface_hub.hf_api import whoami
from .._login import auth_list, auth_switch, login, logout
from ..utils import ANSI, get_stored_tokens, get_token, logging
from ._cli_utils import TokenOpt, typer_factory
logger = logging.get_logger(__name__)
auth_cli = typer_factory(help="Manage authentication (login, logout, etc.).")
@auth_cli.command(
    examples=[
        "hf auth login",
        "hf auth login --token $HF_TOKEN",
        "hf auth login --token $HF_TOKEN --add-to-git-credential",
def auth_login(
    token: TokenOpt = None,
    add_to_git_credential: Annotated[
        typer.Option(
            help="Save to git credential helper. Useful only if you plan to run git commands directly.",
    """Login using a token from huggingface.co/settings/tokens."""
    login(token=token, add_to_git_credential=add_to_git_credential)
    examples=["hf auth logout", "hf auth logout --token-name my-token"],
def auth_logout(
    token_name: Annotated[
        typer.Option(help="Name of token to logout"),
    """Logout from a specific token."""
    logout(token_name=token_name)
def _select_token_name() -> Optional[str]:
    token_names = list(get_stored_tokens().keys())
    if not token_names:
        logger.error("No stored tokens found. Please login first.")
    print("Available stored tokens:")
    for i, token_name in enumerate(token_names, 1):
        print(f"{i}. {token_name}")
            choice = input("Enter the number of the token to switch to (or 'q' to quit): ")
            if choice.lower() == "q":
            index = int(choice) - 1
            if 0 <= index < len(token_names):
                return token_names[index]
                print("Invalid selection. Please try again.")
            print("Invalid input. Please enter a number or 'q' to quit.")
    examples=["hf auth switch", "hf auth switch --token-name my-token"],
def auth_switch_cmd(
            help="Name of the token to switch to",
    """Switch between access tokens."""
    if token_name is None:
        token_name = _select_token_name()
        print("No token name provided. Aborting.")
        raise typer.Exit()
    auth_switch(token_name, add_to_git_credential=add_to_git_credential)
@auth_cli.command("list", examples=["hf auth list"])
def auth_list_cmd() -> None:
    """List all stored access tokens."""
    auth_list()
@auth_cli.command("whoami", examples=["hf auth whoami"])
def auth_whoami() -> None:
    """Find out which huggingface.co account you are logged in as."""
    token = get_token()
    if token is None:
        print("Not logged in")
    info = whoami(token)
    print(ANSI.bold("user: "), info["name"])
    orgs = [org["name"] for org in info["orgs"]]
    if orgs:
        print(ANSI.bold("orgs: "), ",".join(orgs))
    if ENDPOINT != "https://huggingface.co":
        print(f"Authenticated through private endpoint: {ENDPOINT}")
from litellm.constants import CLI_JWT_EXPIRATION_HOURS
# Token storage utilities
def get_token_file_path() -> str:
    """Get the path to store the authentication token"""
    config_dir = home_dir / ".litellm"
    config_dir.mkdir(exist_ok=True)
    return str(config_dir / "token.json")
def save_token(token_data: Dict[str, Any]) -> None:
    """Save token data to file"""
    token_file = get_token_file_path()
    with open(token_file, "w") as f:
        json.dump(token_data, f, indent=2)
    # Set file permissions to be readable only by owner
    os.chmod(token_file, 0o600)
def load_token() -> Optional[Dict[str, Any]]:
    """Load token data from file"""
    if not os.path.exists(token_file):
        with open(token_file, "r") as f:
    except (json.JSONDecodeError, IOError):
def clear_token() -> None:
    """Clear stored token"""
    if os.path.exists(token_file):
        os.remove(token_file)
def get_stored_api_key() -> Optional[str]:
    """Get the stored API key from token file"""
    # Use the SDK-level utility
    return get_litellm_gateway_api_key()
# Team selection utilities
def display_teams_table(teams: List[Dict[str, Any]]) -> None:
    """Display teams in a formatted table"""
    if not teams:
        console.print("❌ No teams found for your user.")
    table = Table(title="Available Teams")
    table.add_column("Index", style="cyan", no_wrap=True)
    table.add_column("Team Alias", style="magenta")
    table.add_column("Team ID", style="green")
    table.add_column("Models", style="yellow")
    table.add_column("Max Budget", style="blue")
    for i, team in enumerate(teams):
        team_alias = team.get("team_alias") or "N/A"
        team_id = team.get("team_id", "N/A")
        models = team.get("models", [])
        max_budget = team.get("max_budget")
        # Format models list
        if models:
            if len(models) > 3:
                models_str = ", ".join(models[:3]) + f" (+{len(models) - 3} more)"
                models_str = ", ".join(models)
            models_str = "All models"
        # Format budget
        budget_str = f"${max_budget}" if max_budget else "Unlimited"
        table.add_row(str(i + 1), team_alias, team_id, models_str, budget_str)
def get_key_input():
    """Get a single key input from the user (cross-platform)"""
            key = msvcrt.getch()
            if key == b"\xe0":  # Arrow keys on Windows
                if key == b"H":  # Up arrow
                    return "up"
                elif key == b"P":  # Down arrow
                    return "down"
            elif key == b"\r":  # Enter key
                return "enter"
            elif key == b"\x1b":  # Escape key
                return "escape"
            elif key == b"q":
                return "quit"
            import termios
            import tty
            fd = sys.stdin.fileno()
            old_settings = termios.tcgetattr(fd)
                tty.setraw(sys.stdin.fileno())
                key = sys.stdin.read(1)
                if key == "\x1b":  # Escape sequence
                    key += sys.stdin.read(2)
                    if key == "\x1b[A":  # Up arrow
                    elif key == "\x1b[B":  # Down arrow
                    elif key == "\x1b":  # Just escape
                elif key == "\r" or key == "\n":  # Enter key
                elif key == "q":
                termios.tcsetattr(fd, termios.TCSADRAIN, old_settings)
        # Fallback to simple input if termios/msvcrt not available
def display_interactive_team_selection(
    teams: List[Dict[str, Any]], selected_index: int = 0
    """Display teams with one highlighted for selection"""
    # Clear the screen using Rich's method
    console.clear()
    console.print("🎯 Select a Team (Use ↑↓ arrows, Enter to select, 'q' to skip):\n")
        # Highlight the selected item
        if i == selected_index:
            console.print(f"➤ [bold cyan]{team_alias}[/bold cyan] ({team_id})")
            console.print(f"   Models: [yellow]{models_str}[/yellow]")
            console.print(f"   Budget: [blue]{budget_str}[/blue]\n")
            console.print(f"  [dim]{team_alias}[/dim] ({team_id})")
            console.print(f"   Models: [dim]{models_str}[/dim]")
            console.print(f"   Budget: [dim]{budget_str}[/dim]\n")
def prompt_team_selection(teams: List[Dict[str, Any]]) -> Optional[Dict[str, Any]]:
    """Interactive team selection with arrow keys"""
    selected_index = 0
        # Check if we can use interactive mode
        if not sys.stdin.isatty():
            # Fallback to simple selection for non-interactive environments
            return prompt_team_selection_fallback(teams)
            display_interactive_team_selection(teams, selected_index)
            key = get_key_input()
            if key == "up":
                selected_index = (selected_index - 1) % len(teams)
            elif key == "down":
                selected_index = (selected_index + 1) % len(teams)
            elif key == "enter":
                selected_team = teams[selected_index]
                # Clear screen and show selection
                click.echo(
                    f"✅ Selected team: {selected_team.get('team_alias', 'N/A')} ({selected_team.get('team_id')})"
                return selected_team
            elif key == "quit" or key == "escape":
                # Clear screen
                click.echo("ℹ️ Team selection skipped.")
            elif key is None:
                # If we can't get key input, fall back to simple selection
        click.echo("\n❌ Team selection cancelled.")
        # If interactive mode fails, fall back to simple selection
def prompt_team_selection_fallback(
    teams: List[Dict[str, Any]]
) -> Optional[Dict[str, Any]]:
    """Fallback team selection for non-interactive environments"""
            choice = click.prompt(
                "\nSelect a team by entering the index number (or 'skip' to continue without a team)",
            if choice.lower() == "skip":
            if 0 <= index < len(teams):
                selected_team = teams[index]
                    f"\n✅ Selected team: {selected_team.get('team_alias', 'N/A')} ({selected_team.get('team_id')})"
                    f"❌ Invalid selection. Please enter a number between 1 and {len(teams)}"
            click.echo("❌ Invalid input. Please enter a number or 'skip'")
# Polling-based authentication - no local server needed
def _poll_for_ready_data(
    total_timeout: int = 300,
    poll_interval: int = 2,
    request_timeout: int = 10,
    pending_message: Optional[str] = None,
    pending_log_every: int = 10,
    other_status_message: Optional[str] = None,
    other_status_log_every: int = 10,
    http_error_log_every: int = 10,
    connection_error_log_every: int = 10,
    for attempt in range(total_timeout // poll_interval):
            response = requests.get(url, timeout=request_timeout)
                status = data.get("status")
                if status == "ready":
                if status == "pending":
                        pending_message
                        and pending_log_every > 0
                        and attempt % pending_log_every == 0
                        click.echo(pending_message)
                    other_status_message
                    and other_status_log_every > 0
                    and attempt % other_status_log_every == 0
                    click.echo(other_status_message)
            elif http_error_log_every > 0 and attempt % http_error_log_every == 0:
                click.echo(f"Polling error: HTTP {response.status_code}")
        except requests.RequestException as e:
                connection_error_log_every > 0
                and attempt % connection_error_log_every == 0
                click.echo(f"Connection error (will retry): {e}")
        time.sleep(poll_interval)
def _normalize_teams(teams, team_details):
    """If team_details are a
        teams (_type_): _description_
        team_details (_type_): _description_
        _type_: _description_
    if isinstance(team_details, list) and team_details:
            {"team_id": i.get("team_id") or i.get("id"), "team_alias": i.get("team_alias")}
            for i in team_details
            if isinstance(i, dict) and (i.get("team_id") or i.get("id"))
    if isinstance(teams, list):
        return [{"team_id": str(t), "team_alias": None} for t in teams]
def _poll_for_authentication(base_url: str, key_id: str) -> Optional[dict]:
    Poll the server for authentication completion and handle team selection.
        Dictionary with authentication data if successful, None otherwise
    poll_url = f"{base_url}/sso/cli/poll/{key_id}"
    data = _poll_for_ready_data(
        poll_url,
        pending_message="Still waiting for authentication...",
    if data.get("requires_team_selection"):
        teams = data.get("teams", [])
        team_details = data.get("team_details")
        user_id = data.get("user_id")
        normalized_teams: List[Dict[str, Any]] = _normalize_teams(teams, team_details)
        if not normalized_teams:
            click.echo("⚠️ No teams available for selection.")
        # User has multiple teams - let them select
        jwt_with_team = _handle_team_selection_during_polling(
            key_id=key_id,
            teams=normalized_teams,
        # Use the team-specific JWT if selection succeeded
        if jwt_with_team:
                "api_key": jwt_with_team,
                "user_id": user_id,
                "teams": teams,
                "team_id": None,  # Set by server in JWT
        click.echo("❌ Team selection cancelled or JWT generation failed.")
    # JWT is ready (single team or team already selected)
    api_key = data.get("key")
    team_id = data.get("team_id")
    # Show which team was assigned
    if team_id and len(teams) == 1:
        click.echo(f"\n✅ Automatically assigned to team: {team_id}")
            "team_id": team_id,
def _handle_team_selection_during_polling(
    base_url: str, key_id: str, teams: List[Dict[str, Any]]
    Handle team selection and re-poll with selected team_id.
        teams: List of team IDs (strings)
        The JWT token with the selected team, or None if selection was skipped
            "ℹ️ No teams found. You can create or join teams using the web interface."
    click.echo("\n" + "=" * 60)
    click.echo("📋 Select a team for your CLI session...")
    team_id = _render_and_prompt_for_team_selection(teams)
    if not team_id:
        click.echo("ℹ️ No team selected.")
    click.echo(f"\n🔄 Generating JWT for team: {team_id}")
    poll_url = f"{base_url}/sso/cli/poll/{key_id}?team_id={team_id}"
        pending_message="Still waiting for team authentication...",
        other_status_message="Waiting for team authentication to complete...",
        http_error_log_every=10,
    jwt_token = data.get("key")
    if jwt_token:
        click.echo(f"✅ Successfully generated JWT for team: {team_id}")
        return jwt_token
def _render_and_prompt_for_team_selection(teams: List[Dict[str, Any]]) -> Optional[str]:
    """Render teams table and prompt user for a team selection.
    Returns the selected team_id as a string, or None if selection was
    cancelled or skipped without any teams available.
    # Display teams as a simple list, but prefer showing aliases where
    # available while still keeping the underlying IDs intact.
    table.add_column("Team Name", style="magenta")
        team_id = str(team.get("team_id"))
        team_alias = team.get("team_alias") or team_id
        table.add_row(str(i + 1), team_alias, team_id)
    # Simple selection
                "\nSelect a team by entering the index number (or 'skip' to use first team)",
                # Default to the first team's ID if the user skips an
                # explicit selection.
                if teams:
                    first_team = teams[0]
                    return str(first_team.get("team_id"))
                team_id = str(selected_team.get("team_id"))
                team_alias = selected_team.get("team_alias") or team_id
                click.echo(f"\n✅ Selected team: {team_alias} ({team_id})")
                return team_id
@click.command(name="login")
def login(ctx: click.Context):
    """Login to LiteLLM proxy using SSO authentication"""
    from litellm.constants import LITELLM_CLI_SOURCE_IDENTIFIER
    from litellm.proxy.client.cli.interface import show_commands
    base_url = ctx.obj["base_url"]
    # Check if we have an existing key to regenerate
    existing_key = get_stored_api_key()
    # Generate unique key ID for this login session
    key_id = f"sk-{str(uuid.uuid4())}"
        # Construct SSO login URL with CLI source and pre-generated key
        sso_url = f"{base_url}/sso/key/generate?source={LITELLM_CLI_SOURCE_IDENTIFIER}&key={key_id}"
        # If we have an existing key, include it as a parameter to the login endpoint
        # The server will encode it in the OAuth state parameter for the SSO flow
        if existing_key:
            sso_url += f"&existing_key={existing_key}"
        click.echo(f"Opening browser to: {sso_url}")
        click.echo("Please complete the SSO authentication in your browser...")
        click.echo(f"Session ID: {key_id}")
        # Open browser
        webbrowser.open(sso_url)
        # Poll for authentication completion
        click.echo("Waiting for authentication...")
        auth_result = _poll_for_authentication(base_url=base_url, key_id=key_id)
        if auth_result:
            api_key = auth_result["api_key"]
            user_id = auth_result["user_id"]
            # Save token data (simplified for CLI - we just need the key)
            save_token(
                    "key": api_key,
                    "user_id": user_id or "cli-user",
                    "user_email": "unknown",
                    "user_role": "cli",
                    "auth_header_name": "Authorization",
                    "jwt_token": "",
                    "timestamp": time.time(),
            click.echo("\n✅ Login successful!")
            click.echo(f"JWT Token: {api_key[:20]}...")
            click.echo("You can now use the CLI without specifying --api-key")
            # Show available commands after successful login
            show_commands()
            click.echo("❌ Authentication timed out. Please try again.")
        click.echo("\n❌ Authentication cancelled by user.")
        click.echo(f"❌ Authentication failed: {e}")
@click.command(name="logout")
def logout():
    """Logout and clear stored authentication"""
    clear_token()
    click.echo("✅ Logged out successfully. Authentication token cleared.")
@click.command(name="whoami")
def whoami():
    """Show current authentication status"""
    token_data = load_token()
    if not token_data:
        click.echo("❌ Not authenticated. Run 'litellm-proxy login' to authenticate.")
    click.echo("✅ Authenticated")
    click.echo(f"User Email: {token_data.get('user_email', 'Unknown')}")
    click.echo(f"User ID: {token_data.get('user_id', 'Unknown')}")
    click.echo(f"User Role: {token_data.get('user_role', 'Unknown')}")
    # Check if token is still valid (basic timestamp check)
    timestamp = token_data.get("timestamp", 0)
    age_hours = (time.time() - timestamp) / 3600
    click.echo(f"Token age: {age_hours:.1f} hours")
    if age_hours > CLI_JWT_EXPIRATION_HOURS:
        click.echo(f"⚠️ Warning: Token is more than {CLI_JWT_EXPIRATION_HOURS} hours old and may have expired.")
# Export functions for use by other CLI commands
__all__ = ["login", "logout", "whoami", "prompt_team_selection"]
# Export individual commands instead of grouping them
# login, logout, and whoami will be added as top-level commands
