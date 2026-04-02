import tweepy
# Your app's API/consumer key and secret can be found under the Consumer Keys
# section of the Keys and Tokens tab of your app, under the
# Twitter Developer Portal Projects & Apps page at
# https://developer.twitter.com/en/portal/projects-and-apps
consumer_key = ""
consumer_secret = ""
# Your account's (the app owner's account's) access token and secret for your
# app can be found under the Authentication Tokens section of the
# Keys and Tokens tab of your app, under the
access_token = ""
access_token_secret = ""
auth = tweepy.OAuth1UserHandler(
    consumer_key, consumer_secret, access_token, access_token_secret
api = tweepy.API(auth)
# If the authentication was successful, this should print the
# screen name / username of the account
print(api.verify_credentials().screen_name)
# Your app's bearer token can be found under the Authentication Tokens section
# of the Keys and Tokens tab of your app, under the
bearer_token = ""
# You can authenticate as your app with just your bearer token
client = tweepy.Client(bearer_token=bearer_token)
# You can provide the consumer key and secret with the access token and access
# token secret to authenticate as a user
client = tweepy.Client(
    consumer_key=consumer_key, consumer_secret=consumer_secret,
    access_token=access_token, access_token_secret=access_token_secret
from typing import Any, ParamSpec
from starlette._utils import is_async_callable
from starlette.exceptions import HTTPException
from starlette.responses import RedirectResponse
def has_required_scope(conn: HTTPConnection, scopes: Sequence[str]) -> bool:
    for scope in scopes:
        if scope not in conn.auth.scopes:
def requires(
    scopes: str | Sequence[str],
    status_code: int = 403,
    redirect: str | None = None,
) -> Callable[[Callable[_P, Any]], Callable[_P, Any]]:
    scopes_list = [scopes] if isinstance(scopes, str) else list(scopes)
    def decorator(
        func: Callable[_P, Any],
    ) -> Callable[_P, Any]:
        for idx, parameter in enumerate(sig.parameters.values()):
            if parameter.name == "request" or parameter.name == "websocket":
                type_ = parameter.name
            raise Exception(f'No "request" or "websocket" argument on function "{func}"')
        if type_ == "websocket":
            # Handle websocket functions. (Always async)
            async def websocket_wrapper(*args: _P.args, **kwargs: _P.kwargs) -> None:
                websocket = kwargs.get("websocket", args[idx] if idx < len(args) else None)
                assert isinstance(websocket, WebSocket)
                if not has_required_scope(websocket, scopes_list):
                    await websocket.close()
                    await func(*args, **kwargs)
            return websocket_wrapper
        elif is_async_callable(func):
            # Handle async request/response functions.
            async def async_wrapper(*args: _P.args, **kwargs: _P.kwargs) -> Any:
                request = kwargs.get("request", args[idx] if idx < len(args) else None)
                if not has_required_scope(request, scopes_list):
                    if redirect is not None:
                        orig_request_qparam = urlencode({"next": str(request.url)})
                        next_url = f"{request.url_for(redirect)}?{orig_request_qparam}"
                        return RedirectResponse(url=next_url, status_code=303)
                    raise HTTPException(status_code=status_code)
            return async_wrapper
            # Handle sync request/response functions.
            def sync_wrapper(*args: _P.args, **kwargs: _P.kwargs) -> Any:
            return sync_wrapper
class AuthenticationError(Exception):
class AuthenticationBackend:
    async def authenticate(self, conn: HTTPConnection) -> tuple[AuthCredentials, BaseUser] | None:
        raise NotImplementedError()  # pragma: no cover
class AuthCredentials:
    def __init__(self, scopes: Sequence[str] | None = None):
        self.scopes = [] if scopes is None else list(scopes)
class BaseUser:
    def is_authenticated(self) -> bool:
    def display_name(self) -> str:
    def identity(self) -> str:
class SimpleUser(BaseUser):
    def __init__(self, username: str) -> None:
class UnauthenticatedUser(BaseUser):
from starlette.authentication import (
    AuthCredentials,
    AuthenticationBackend,
    UnauthenticatedUser,
from starlette.requests import HTTPConnection
from starlette.responses import PlainTextResponse, Response
from starlette.types import ASGIApp, Receive, Scope, Send
class AuthenticationMiddleware:
        app: ASGIApp,
        backend: AuthenticationBackend,
        on_error: Callable[[HTTPConnection, AuthenticationError], Response] | None = None,
        self.app = app
        self.on_error: Callable[[HTTPConnection, AuthenticationError], Response] = (
            on_error if on_error is not None else self.default_on_error
        if scope["type"] not in ["http", "websocket"]:
            await self.app(scope, receive, send)
        conn = HTTPConnection(scope)
            auth_result = await self.backend.authenticate(conn)
        except AuthenticationError as exc:
            response = self.on_error(conn, exc)
            if scope["type"] == "websocket":
                await send({"type": "websocket.close", "code": 1000})
                await response(scope, receive, send)
        if auth_result is None:
            auth_result = AuthCredentials(), UnauthenticatedUser()
        scope["auth"], scope["user"] = auth_result
    def default_on_error(conn: HTTPConnection, exc: Exception) -> Response:
        return PlainTextResponse(str(exc), status_code=400)
