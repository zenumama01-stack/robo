class TweepyClientTests(unittest.TestCase):
        self.client = tweepy.Client(
    @tape.use_cassette("test_client_bookmarks.yaml")
    def test_bookmarks(self):
        self.client.bookmark(tweet_id)
        self.client.get_bookmarks()
        self.client.remove_bookmark(tweet_id)
    @tape.use_cassette("test_client_hide_and_unhide_reply.yaml")
    def test_hide_and_unhide_reply(self):
        self.client.hide_reply(reply_id)
        self.client.unhide_reply(reply_id)
    @tape.use_cassette("test_client_like_and_unlike.yaml")
    def test_like_and_unlike(self):
        self.client.like(tweet_id)
        self.client.unlike(tweet_id)
    @tape.use_cassette("test_client_get_liking_users.yaml")
    def test_get_liking_users(self):
        self.client.get_liking_users(tweet_id)
    @tape.use_cassette("test_client_get_liked_tweets.yaml")
    def test_get_liked_tweets(self):
        self.client.get_liked_tweets(user_id)
    @tape.use_cassette("test_client_create_and_delete_tweet.yaml")
    def test_create_and_delete_tweet(self):
        response = self.client.create_tweet(text="Test Tweet")
        self.client.delete_tweet(tweet_id)
    @tape.use_cassette("test_client_get_quote_tweets.yaml")
    def test_get_quote_tweets(self):
        self.client.get_quote_tweets(tweet_id)
    @tape.use_cassette("test_client_retweet_and_unretweet.yaml")
    def test_retweet_and_unretweet(self):
        self.client.retweet(tweet_id)
        self.client.unretweet(tweet_id)
    @tape.use_cassette("test_client_get_retweeters.yaml")
    def test_get_retweeters(self):
        self.client.get_retweeters(tweet_id)
    @tape.use_cassette("test_client_search_all_tweets.yaml")
    def test_search_all_tweets(self):
        self.client.search_all_tweets("Tweepy")
    @tape.use_cassette("test_client_search_recent_tweets.yaml")
    def test_search_recent_tweets(self):
        self.client.search_recent_tweets("Tweepy")
    @tape.use_cassette("test_client_get_users_mentions.yaml")
    def test_get_users_mentions(self):
        self.client.get_users_mentions(user_id)
    @tape.use_cassette("test_client_get_home_timeline.yaml")
    def test_get_home_timeline(self):
        self.client.get_home_timeline()
    @tape.use_cassette("test_client_get_users_tweets.yaml")
    def test_get_users_tweets(self):
        self.client.get_users_tweets(user_id)
    @tape.use_cassette("test_client_get_all_tweets_count.yaml")
    def test_get_all_tweets_count(self):
        self.client.get_all_tweets_count("Tweepy")
    @tape.use_cassette("test_client_get_recent_tweets_count.yaml")
    def test_get_recent_tweets_count(self):
        self.client.get_recent_tweets_count("Tweepy")
    @tape.use_cassette("test_client_get_tweet.yaml")
    def test_get_tweet(self):
        self.client.get_tweet(tweet_id)
    @tape.use_cassette("test_client_get_tweets.yaml")
    def test_get_tweets(self):
        self.client.get_tweets(tweet_ids)
    # TODO: Test Client.get_blocked
    @tape.use_cassette("test_client_follow_and_unfollow_user.yaml")
    def test_follow_and_unfollow_user(self):
        self.client.follow_user(user_id)
        self.client.unfollow_user(user_id)
    @tape.use_cassette("test_client_get_users_followers.yaml")
    def test_get_users_followers(self):
        self.client.get_users_followers(user_id)
    @tape.use_cassette("test_client_get_users_following.yaml")
    def test_get_users_following(self):
        self.client.get_users_following(user_id)
    @tape.use_cassette("test_client_mute_get_muted_and_unmute.yaml")
    def test_mute_get_muted_and_unmute(self):
        self.client.mute(user_id)
        self.client.get_muted()
        self.client.unmute(user_id)
    @tape.use_cassette("test_client_get_user.yaml")
    def test_get_user(self):
        self.client.get_user(username="Twitter")
    @tape.use_cassette("test_client_get_users.yaml")
    def test_get_users(self):
        self.client.get_users(usernames=["Twitter", "TwitterDev"])
    @tape.use_cassette("test_client_get_me.yaml")
    def test_get_me(self):
        self.client.get_me()
    @tape.use_cassette("test_client_search_spaces.yaml")
    def test_search_spaces(self):
        self.client.search_spaces("Twitter")
    @tape.use_cassette("test_client_get_spaces.yaml")
    def test_get_spaces(self):
        space_ids = ["1YpKkzBgBlVxj", "1OwGWzarWnNKQ"]
        # Space ID for @TwitterSpaces Twitter Spaces community gathering + Q&A
        # https://twitter.com/TwitterSpaces/status/1436382283347283969
        # Space ID for @NASA #NASAWebb Space Telescope 101 and Q&A
        # https://twitter.com/NASA/status/1442961745098653701
        user_ids = [1065249714214457345, 2328002822]
        # User IDs for @TwitterSpaces and @TwitterWomen
        self.client.get_spaces(ids=space_ids)
        self.client.get_spaces(user_ids=user_ids)
    @tape.use_cassette("test_client_get_space.yaml")
    def test_get_space(self):
        space_id = "1YpKkzBgBlVxj"
        self.client.get_space(space_id)
    # TODO: Test Client.get_space_buyers
    # TODO: Test Client.get_space_tweets
    @tape.use_cassette("test_manage_and_lookup_direct_messages.yaml")
    def test_manage_and_lookup_direct_messages(self):
        response = self.client.create_direct_message(
        self.client.create_direct_message(
        self.client.create_direct_message_conversation(
        self.client.get_dm_events()
        self.client.get_dm_events(dm_conversation_id=dm_conversation_id)
        self.client.get_dm_events(participant_id=user_ids[1])
    @tape.use_cassette("test_client_get_list_tweets.yaml")
    def test_get_list_tweets(self):
        self.client.get_list_tweets(list_id)
    @tape.use_cassette("test_client_follow_and_unfollow_list.yaml")
    def test_follow_and_unfollow_list(self):
        self.client.follow_list(list_id)
        self.client.unfollow_list(list_id)
    @tape.use_cassette("test_client_get_list_followers.yaml")
    def test_get_list_followers(self):
        self.client.get_list_followers(list_id)
    @tape.use_cassette("test_client_get_followed_lists.yaml")
    def test_get_followed_lists(self):
        user_id = 372575989  # User ID for @TwitterNews
        self.client.get_followed_lists(user_id)
    @tape.use_cassette("test_client_get_list.yaml")
    def test_get_list(self):
        self.client.get_list(list_id)
    @tape.use_cassette("test_client_get_owned_lists.yaml")
    def test_get_owned_lists(self):
        self.client.get_owned_lists(user_id)
    @tape.use_cassette("test_client_get_list_members.yaml")
    def test_get_list_members(self):
        self.client.get_list_members(list_id)
    @tape.use_cassette("test_client_get_list_memberships.yaml")
    def test_get_list_memberships(self):
        self.client.get_list_memberships(user_id)
    @tape.use_cassette("test_client_manage_and_get_pinned_lists.yaml")
    def test_manage_and_get_pinned_lists(self):
        response = self.client.create_list("Test List", private=True)
        self.client.add_list_member(list_id, user_id)
        self.client.pin_list(list_id)
        self.client.get_pinned_lists()
        self.client.remove_list_member(list_id, user_id)
        self.client.unpin_list(list_id)
        self.client.update_list(list_id, description="Test List Description")
        self.client.delete_list(list_id)
        "test_client_create_and_get_compliance_job_and_jobs.yaml"
    def test_create_and_get_compliance_job_and_jobs(self):
        response = self.client.create_compliance_job("tweets")
        self.client.get_compliance_job(job_id)
        self.client.get_compliance_jobs("tweets")
    @tape.use_cassette("test_client_create_tweet_with_community.yaml")
    def test_create_tweet_with_community(self):
        response = self.client.create_tweet(
            text="Test tweet in community",
            community_id="123456789"
        assert response.data["text"] == "Test tweet in community"
        assert "community_id" in response.data
import gc
import dataclasses
import tracemalloc
from typing import Any, Union, TypeVar, Callable, Iterable, Iterator, Optional, Protocol, Coroutine, cast
from unittest import mock
from typing_extensions import Literal, AsyncIterator, override
from respx import MockRouter
from pydantic import ValidationError
from openai import OpenAI, AsyncOpenAI, APIResponseValidationError
from openai._types import Omit
from openai._utils import asyncify
from openai._models import BaseModel, FinalRequestOptions
from openai._streaming import Stream, AsyncStream
from openai._exceptions import OpenAIError, APIStatusError, APITimeoutError, APIResponseValidationError
from openai._base_client import (
    HTTPX_DEFAULT_TIMEOUT,
    BaseClient,
    DefaultHttpxClient,
    DefaultAsyncHttpxClient,
    get_platform,
from .utils import update_env
class MockRequestCall(Protocol):
def _get_params(client: BaseClient[Any, Any]) -> dict[str, str]:
    request = client._build_request(FinalRequestOptions(method="get", url="/foo"))
    url = httpx.URL(request.url)
    return dict(url.params)
def _low_retry_timeout(*_args: Any, **_kwargs: Any) -> float:
    return 0.1
def mirror_request_content(request: httpx.Request) -> httpx.Response:
    return httpx.Response(200, content=request.content)
# note: we can't use the httpx.MockTransport class as it consumes the request
#       body itself, which means we can't test that the body is read lazily
class MockTransport(httpx.BaseTransport, httpx.AsyncBaseTransport):
        handler: Callable[[httpx.Request], httpx.Response]
        | Callable[[httpx.Request], Coroutine[Any, Any, httpx.Response]],
        self.handler = handler
    def handle_request(
        request: httpx.Request,
    ) -> httpx.Response:
        assert not inspect.iscoroutinefunction(self.handler), "handler must not be a coroutine function"
        assert inspect.isfunction(self.handler), "handler must be a function"
        return self.handler(request)
    async def handle_async_request(
        assert inspect.iscoroutinefunction(self.handler), "handler must be a coroutine function"
        return await self.handler(request)
@dataclasses.dataclass
class Counter:
    value: int = 0
def _make_sync_iterator(iterable: Iterable[T], counter: Optional[Counter] = None) -> Iterator[T]:
    for item in iterable:
        if counter:
            counter.value += 1
async def _make_async_iterator(iterable: Iterable[T], counter: Optional[Counter] = None) -> AsyncIterator[T]:
def _get_open_connections(client: OpenAI | AsyncOpenAI) -> int:
    transport = client._client._transport
    assert isinstance(transport, httpx.HTTPTransport) or isinstance(transport, httpx.AsyncHTTPTransport)
    pool = transport._pool
    return len(pool._requests)
class TestOpenAI:
    @pytest.mark.respx(base_url=base_url)
    def test_raw_response(self, respx_mock: MockRouter, client: OpenAI) -> None:
        respx_mock.post("/foo").mock(return_value=httpx.Response(200, json={"foo": "bar"}))
        response = client.post("/foo", cast_to=httpx.Response)
        assert response.status_code == 200
        assert isinstance(response, httpx.Response)
        assert response.json() == {"foo": "bar"}
    def test_raw_response_for_binary(self, respx_mock: MockRouter, client: OpenAI) -> None:
        respx_mock.post("/foo").mock(
            return_value=httpx.Response(200, headers={"Content-Type": "application/binary"}, content='{"foo": "bar"}')
    def test_copy(self, client: OpenAI) -> None:
        copied = client.copy()
        assert id(copied) != id(client)
        copied = client.copy(api_key="another My API Key")
        assert copied.api_key == "another My API Key"
        assert client.api_key == "My API Key"
    def test_copy_default_options(self, client: OpenAI) -> None:
        # options that have a default are overridden correctly
        copied = client.copy(max_retries=7)
        assert copied.max_retries == 7
        assert client.max_retries == 2
        copied2 = copied.copy(max_retries=6)
        assert copied2.max_retries == 6
        # timeout
        assert isinstance(client.timeout, httpx.Timeout)
        copied = client.copy(timeout=None)
        assert copied.timeout is None
    def test_copy_default_headers(self) -> None:
        client = OpenAI(
            base_url=base_url, api_key=api_key, _strict_response_validation=True, default_headers={"X-Foo": "bar"}
        assert client.default_headers["X-Foo"] == "bar"
        # does not override the already given value when not specified
        assert copied.default_headers["X-Foo"] == "bar"
        # merges already given headers
        copied = client.copy(default_headers={"X-Bar": "stainless"})
        assert copied.default_headers["X-Bar"] == "stainless"
        # uses new values for any already given headers
        copied = client.copy(default_headers={"X-Foo": "stainless"})
        assert copied.default_headers["X-Foo"] == "stainless"
        # set_default_headers
        # completely overrides already set values
        copied = client.copy(set_default_headers={})
        assert copied.default_headers.get("X-Foo") is None
        copied = client.copy(set_default_headers={"X-Bar": "Robert"})
        assert copied.default_headers["X-Bar"] == "Robert"
        with pytest.raises(
            ValueError,
            match="`default_headers` and `set_default_headers` arguments are mutually exclusive",
            client.copy(set_default_headers={}, default_headers={"X-Foo": "Bar"})
        client.close()
    def test_copy_default_query(self) -> None:
            base_url=base_url, api_key=api_key, _strict_response_validation=True, default_query={"foo": "bar"}
        assert _get_params(client)["foo"] == "bar"
        assert _get_params(copied)["foo"] == "bar"
        # merges already given params
        copied = client.copy(default_query={"bar": "stainless"})
        params = _get_params(copied)
        assert params["foo"] == "bar"
        assert params["bar"] == "stainless"
        copied = client.copy(default_query={"foo": "stainless"})
        assert _get_params(copied)["foo"] == "stainless"
        # set_default_query
        copied = client.copy(set_default_query={})
        assert _get_params(copied) == {}
        copied = client.copy(set_default_query={"bar": "Robert"})
        assert _get_params(copied)["bar"] == "Robert"
            # TODO: update
            match="`default_query` and `set_default_query` arguments are mutually exclusive",
            client.copy(set_default_query={}, default_query={"foo": "Bar"})
    def test_copy_signature(self, client: OpenAI) -> None:
        # ensure the same parameters that can be passed to the client are defined in the `.copy()` method
        init_signature = inspect.signature(
            # mypy doesn't like that we access the `__init__` property.
            client.__init__,  # type: ignore[misc]
        copy_signature = inspect.signature(client.copy)
        exclude_params = {"transport", "proxies", "_strict_response_validation"}
        for name in init_signature.parameters.keys():
            copy_param = copy_signature.parameters.get(name)
            assert copy_param is not None, f"copy() signature is missing the {name} param"
    @pytest.mark.skipif(sys.version_info >= (3, 10), reason="fails because of a memory leak that started from 3.12")
    def test_copy_build_request(self, client: OpenAI) -> None:
        options = FinalRequestOptions(method="get", url="/foo")
        def build_request(options: FinalRequestOptions) -> None:
            client_copy = client.copy()
            client_copy._build_request(options)
        # ensure that the machinery is warmed up before tracing starts.
        build_request(options)
        gc.collect()
        tracemalloc.start(1000)
        snapshot_before = tracemalloc.take_snapshot()
        ITERATIONS = 10
        for _ in range(ITERATIONS):
        snapshot_after = tracemalloc.take_snapshot()
        tracemalloc.stop()
        def add_leak(leaks: list[tracemalloc.StatisticDiff], diff: tracemalloc.StatisticDiff) -> None:
            if diff.count == 0:
                # Avoid false positives by considering only leaks (i.e. allocations that persist).
            if diff.count % ITERATIONS != 0:
                # Avoid false positives by considering only leaks that appear per iteration.
            for frame in diff.traceback:
                if any(
                    frame.filename.endswith(fragment)
                    for fragment in [
                        # to_raw_response_wrapper leaks through the @functools.wraps() decorator.
                        # removing the decorator fixes the leak for reasons we don't understand.
                        "openai/_legacy_response.py",
                        "openai/_response.py",
                        # pydantic.BaseModel.model_dump || pydantic.BaseModel.dict leak memory for some reason.
                        "openai/_compat.py",
                        # Standard library leaks we don't care about.
                        "/logging/__init__.py",
            leaks.append(diff)
        leaks: list[tracemalloc.StatisticDiff] = []
        for diff in snapshot_after.compare_to(snapshot_before, "traceback"):
            add_leak(leaks, diff)
        if leaks:
            for leak in leaks:
                print("MEMORY LEAK:", leak)
                for frame in leak.traceback:
                    print(frame)
            raise AssertionError()
    def test_request_timeout(self, client: OpenAI) -> None:
        timeout = httpx.Timeout(**request.extensions["timeout"])  # type: ignore
        assert timeout == DEFAULT_TIMEOUT
        request = client._build_request(FinalRequestOptions(method="get", url="/foo", timeout=httpx.Timeout(100.0)))
        assert timeout == httpx.Timeout(100.0)
    def test_client_timeout_option(self) -> None:
        client = OpenAI(base_url=base_url, api_key=api_key, _strict_response_validation=True, timeout=httpx.Timeout(0))
        assert timeout == httpx.Timeout(0)
    def test_http_client_timeout_option(self) -> None:
        # custom timeout given to the httpx client should be used
        with httpx.Client(timeout=None) as http_client:
                base_url=base_url, api_key=api_key, _strict_response_validation=True, http_client=http_client
            assert timeout == httpx.Timeout(None)
        # no timeout given to the httpx client should not use the httpx default
        with httpx.Client() as http_client:
        # explicitly passing the default timeout currently results in it being ignored
        with httpx.Client(timeout=HTTPX_DEFAULT_TIMEOUT) as http_client:
            assert timeout == DEFAULT_TIMEOUT  # our default
    async def test_invalid_http_client(self) -> None:
        with pytest.raises(TypeError, match="Invalid `http_client` arg"):
            async with httpx.AsyncClient() as http_client:
                OpenAI(
                    _strict_response_validation=True,
                    http_client=cast(Any, http_client),
    def test_default_headers_option(self) -> None:
        test_client = OpenAI(
        request = test_client._build_request(FinalRequestOptions(method="get", url="/foo"))
        assert request.headers.get("x-foo") == "bar"
        assert request.headers.get("x-stainless-lang") == "python"
        test_client2 = OpenAI(
            default_headers={
                "X-Foo": "stainless",
                "X-Stainless-Lang": "my-overriding-header",
        request = test_client2._build_request(FinalRequestOptions(method="get", url="/foo"))
        assert request.headers.get("x-foo") == "stainless"
        assert request.headers.get("x-stainless-lang") == "my-overriding-header"
        test_client.close()
        test_client2.close()
    def test_validate_headers(self) -> None:
        client = OpenAI(base_url=base_url, api_key=api_key, _strict_response_validation=True)
        options = client._prepare_options(FinalRequestOptions(method="get", url="/foo"))
        request = client._build_request(options)
        assert request.headers.get("Authorization") == f"Bearer {api_key}"
        with pytest.raises(OpenAIError):
            with update_env(**{"OPENAI_API_KEY": Omit()}):
                client2 = OpenAI(base_url=base_url, api_key=None, _strict_response_validation=True)
            _ = client2
    def test_default_query_option(self) -> None:
            base_url=base_url, api_key=api_key, _strict_response_validation=True, default_query={"query_param": "bar"}
        assert dict(url.params) == {"query_param": "bar"}
        request = client._build_request(
            FinalRequestOptions(
                method="get",
                url="/foo",
                params={"foo": "baz", "query_param": "overridden"},
        assert dict(url.params) == {"foo": "baz", "query_param": "overridden"}
    def test_request_extra_json(self, client: OpenAI) -> None:
                json_data={"foo": "bar"},
                extra_json={"baz": False},
        data = json.loads(request.content.decode("utf-8"))
        assert data == {"foo": "bar", "baz": False}
        assert data == {"baz": False}
        # `extra_json` takes priority over `json_data` when keys clash
                json_data={"foo": "bar", "baz": True},
                extra_json={"baz": None},
        assert data == {"foo": "bar", "baz": None}
    def test_request_extra_headers(self, client: OpenAI) -> None:
                **make_request_options(extra_headers={"X-Foo": "Foo"}),
        assert request.headers.get("X-Foo") == "Foo"
        # `extra_headers` takes priority over `default_headers` when keys clash
        request = client.with_options(default_headers={"X-Bar": "true"})._build_request(
                **make_request_options(
                    extra_headers={"X-Bar": "false"},
        assert request.headers.get("X-Bar") == "false"
    def test_request_extra_query(self, client: OpenAI) -> None:
                    extra_query={"my_query_param": "Foo"},
        params = dict(request.url.params)
        assert params == {"my_query_param": "Foo"}
        # if both `query` and `extra_query` are given, they are merged
                    query={"bar": "1"},
                    extra_query={"foo": "2"},
        assert params == {"bar": "1", "foo": "2"}
        # `extra_query` takes priority over `query` when keys clash
                    query={"foo": "1"},
        assert params == {"foo": "2"}
    def test_multipart_repeating_array(self, client: OpenAI) -> None:
            FinalRequestOptions.construct(
                headers={"Content-Type": "multipart/form-data; boundary=6b7ba517decee4a450543ea6ae821c82"},
                json_data={"array": ["foo", "bar"]},
                files=[("foo.txt", b"hello world")],
        assert request.read().split(b"\r\n") == [
            b"--6b7ba517decee4a450543ea6ae821c82",
            b'Content-Disposition: form-data; name="array[]"',
            b"",
            b"foo",
            b"bar",
            b'Content-Disposition: form-data; name="foo.txt"; filename="upload"',
            b"Content-Type: application/octet-stream",
            b"hello world",
            b"--6b7ba517decee4a450543ea6ae821c82--",
    def test_binary_content_upload(self, respx_mock: MockRouter, client: OpenAI) -> None:
        respx_mock.post("/upload").mock(side_effect=mirror_request_content)
        file_content = b"Hello, this is a test file."
        response = client.post(
            "/upload",
            content=file_content,
            cast_to=httpx.Response,
            options={"headers": {"Content-Type": "application/octet-stream"}},
        assert response.request.headers["Content-Type"] == "application/octet-stream"
        assert response.content == file_content
    def test_binary_content_upload_with_iterator(self) -> None:
        counter = Counter()
        iterator = _make_sync_iterator([file_content], counter=counter)
        def mock_handler(request: httpx.Request) -> httpx.Response:
            assert counter.value == 0, "the request body should not have been read"
            return httpx.Response(200, content=request.read())
        with OpenAI(
            http_client=httpx.Client(transport=MockTransport(handler=mock_handler)),
                content=iterator,
            assert counter.value == 1
    def test_binary_content_upload_with_body_is_deprecated(self, respx_mock: MockRouter, client: OpenAI) -> None:
        with pytest.deprecated_call(
            match="Passing raw bytes as `body` is deprecated and will be removed in a future version. Please pass raw bytes via the `content` parameter instead."
                body=file_content,
    def test_basic_union_response(self, respx_mock: MockRouter, client: OpenAI) -> None:
        class Model1(BaseModel):
        class Model2(BaseModel):
        respx_mock.get("/foo").mock(return_value=httpx.Response(200, json={"foo": "bar"}))
        response = client.get("/foo", cast_to=cast(Any, Union[Model1, Model2]))
        assert isinstance(response, Model2)
        assert response.foo == "bar"
    def test_union_response_different_types(self, respx_mock: MockRouter, client: OpenAI) -> None:
        """Union of objects with the same field name using a different type"""
            foo: int
        respx_mock.get("/foo").mock(return_value=httpx.Response(200, json={"foo": 1}))
        assert isinstance(response, Model1)
        assert response.foo == 1
    def test_non_application_json_content_type_for_json_data(self, respx_mock: MockRouter, client: OpenAI) -> None:
        Response that sets Content-Type to something other than application/json but returns json data
        respx_mock.get("/foo").mock(
            return_value=httpx.Response(
                200,
                content=json.dumps({"foo": 2}),
                headers={"Content-Type": "application/text"},
        response = client.get("/foo", cast_to=Model)
        assert isinstance(response, Model)
        assert response.foo == 2
    def test_base_url_setter(self) -> None:
        client = OpenAI(base_url="https://example.com/from_init", api_key=api_key, _strict_response_validation=True)
        assert client.base_url == "https://example.com/from_init/"
        client.base_url = "https://example.com/from_setter"  # type: ignore[assignment]
        assert client.base_url == "https://example.com/from_setter/"
    def test_base_url_env(self) -> None:
        with update_env(OPENAI_BASE_URL="http://localhost:5000/from/env"):
            client = OpenAI(api_key=api_key, _strict_response_validation=True)
            assert client.base_url == "http://localhost:5000/from/env/"
    @pytest.mark.parametrize(
        "client",
            OpenAI(base_url="http://localhost:5000/custom/path/", api_key=api_key, _strict_response_validation=True),
                base_url="http://localhost:5000/custom/path/",
                http_client=httpx.Client(),
        ids=["standard", "custom http client"],
    def test_base_url_trailing_slash(self, client: OpenAI) -> None:
        assert request.url == "http://localhost:5000/custom/path/foo"
    def test_base_url_no_trailing_slash(self, client: OpenAI) -> None:
    def test_absolute_request_url(self, client: OpenAI) -> None:
                url="https://myapi.com/foo",
        assert request.url == "https://myapi.com/foo"
    def test_copied_client_does_not_close_http(self) -> None:
        test_client = OpenAI(base_url=base_url, api_key=api_key, _strict_response_validation=True)
        assert not test_client.is_closed()
        copied = test_client.copy()
        assert copied is not test_client
        del copied
    def test_client_context_manager(self) -> None:
        with test_client as c2:
            assert c2 is test_client
            assert not c2.is_closed()
        assert test_client.is_closed()
    def test_client_response_validation_error(self, respx_mock: MockRouter, client: OpenAI) -> None:
        respx_mock.get("/foo").mock(return_value=httpx.Response(200, json={"foo": {"invalid": True}}))
        with pytest.raises(APIResponseValidationError) as exc:
            client.get("/foo", cast_to=Model)
        assert isinstance(exc.value.__cause__, ValidationError)
    def test_client_max_retries_validation(self) -> None:
        with pytest.raises(TypeError, match=r"max_retries cannot be None"):
            OpenAI(base_url=base_url, api_key=api_key, _strict_response_validation=True, max_retries=cast(Any, None))
    def test_default_stream_cls(self, respx_mock: MockRouter, client: OpenAI) -> None:
        stream = client.post("/foo", cast_to=Model, stream=True, stream_cls=Stream[Model])
        assert isinstance(stream, Stream)
        stream.response.close()
    def test_received_text_for_expected_json(self, respx_mock: MockRouter) -> None:
        respx_mock.get("/foo").mock(return_value=httpx.Response(200, text="my-custom-format"))
        strict_client = OpenAI(base_url=base_url, api_key=api_key, _strict_response_validation=True)
        with pytest.raises(APIResponseValidationError):
            strict_client.get("/foo", cast_to=Model)
        non_strict_client = OpenAI(base_url=base_url, api_key=api_key, _strict_response_validation=False)
        response = non_strict_client.get("/foo", cast_to=Model)
        assert isinstance(response, str)  # type: ignore[unreachable]
        strict_client.close()
        non_strict_client.close()
        "remaining_retries,retry_after,timeout",
            [3, "20", 20],
            [3, "0", 0.5],
            [3, "-10", 0.5],
            [3, "60", 60],
            [3, "61", 0.5],
            [3, "Fri, 29 Sep 2023 16:26:57 GMT", 20],
            [3, "Fri, 29 Sep 2023 16:26:37 GMT", 0.5],
            [3, "Fri, 29 Sep 2023 16:26:27 GMT", 0.5],
            [3, "Fri, 29 Sep 2023 16:27:37 GMT", 60],
            [3, "Fri, 29 Sep 2023 16:27:38 GMT", 0.5],
            [3, "99999999999999999999999999999999999", 0.5],
            [3, "Zun, 29 Sep 2023 16:26:27 GMT", 0.5],
            [3, "", 0.5],
            [2, "", 0.5 * 2.0],
            [1, "", 0.5 * 4.0],
            [-1100, "", 8],  # test large number potentially overflowing
    @mock.patch("time.time", mock.MagicMock(return_value=1696004797))
    def test_parse_retry_after_header(
        self, remaining_retries: int, retry_after: str, timeout: float, client: OpenAI
        headers = httpx.Headers({"retry-after": retry_after})
        options = FinalRequestOptions(method="get", url="/foo", max_retries=3)
        calculated = client._calculate_retry_timeout(remaining_retries, options, headers)
        assert calculated == pytest.approx(timeout, 0.5 * 0.875)  # pyright: ignore[reportUnknownMemberType]
    @mock.patch("openai._base_client.BaseClient._calculate_retry_timeout", _low_retry_timeout)
    def test_retrying_timeout_errors_doesnt_leak(self, respx_mock: MockRouter, client: OpenAI) -> None:
        respx_mock.post("/chat/completions").mock(side_effect=httpx.TimeoutException("Test timeout error"))
        with pytest.raises(APITimeoutError):
            client.chat.completions.with_streaming_response.create(
                        "content": "string",
                        "role": "developer",
                model="gpt-5.4",
            ).__enter__()
        assert _get_open_connections(client) == 0
    def test_retrying_status_errors_doesnt_leak(self, respx_mock: MockRouter, client: OpenAI) -> None:
        respx_mock.post("/chat/completions").mock(return_value=httpx.Response(500))
        with pytest.raises(APIStatusError):
    @pytest.mark.parametrize("failures_before_success", [0, 2, 4])
    @pytest.mark.parametrize("failure_mode", ["status", "exception"])
    def test_retries_taken(
        failures_before_success: int,
        failure_mode: Literal["status", "exception"],
        respx_mock: MockRouter,
        client = client.with_options(max_retries=4)
        nb_retries = 0
        def retry_handler(_request: httpx.Request) -> httpx.Response:
            nonlocal nb_retries
            if nb_retries < failures_before_success:
                nb_retries += 1
                if failure_mode == "exception":
                    raise RuntimeError("oops")
                return httpx.Response(500)
            return httpx.Response(200)
        respx_mock.post("/chat/completions").mock(side_effect=retry_handler)
        assert response.retries_taken == failures_before_success
        assert int(response.http_request.headers.get("x-stainless-retry-count")) == failures_before_success
    def test_omit_retry_count_header(
        self, client: OpenAI, failures_before_success: int, respx_mock: MockRouter
            extra_headers={"x-stainless-retry-count": Omit()},
        assert len(response.http_request.headers.get_list("x-stainless-retry-count")) == 0
    def test_overwrite_retry_count_header(
            extra_headers={"x-stainless-retry-count": "42"},
        assert response.http_request.headers.get("x-stainless-retry-count") == "42"
    def test_retries_taken_new_response_class(
        with client.chat.completions.with_streaming_response.create(
    def test_proxy_environment_variables(self, monkeypatch: pytest.MonkeyPatch) -> None:
        # Test that the proxy environment variables are set correctly
        monkeypatch.setenv("HTTPS_PROXY", "https://example.org")
        # Delete in case our environment has any proxy env vars set
        monkeypatch.delenv("HTTP_PROXY", raising=False)
        monkeypatch.delenv("ALL_PROXY", raising=False)
        monkeypatch.delenv("NO_PROXY", raising=False)
        monkeypatch.delenv("http_proxy", raising=False)
        monkeypatch.delenv("https_proxy", raising=False)
        monkeypatch.delenv("all_proxy", raising=False)
        monkeypatch.delenv("no_proxy", raising=False)
        client = DefaultHttpxClient()
        mounts = tuple(client._mounts.items())
        assert len(mounts) == 1
        assert mounts[0][0].pattern == "https://"
    @pytest.mark.filterwarnings("ignore:.*deprecated.*:DeprecationWarning")
    def test_default_client_creation(self) -> None:
        # Ensure that the client can be initialized without any exceptions
        DefaultHttpxClient(
            verify=True,
            cert=None,
            trust_env=True,
            http1=True,
            http2=False,
            limits=httpx.Limits(max_connections=100, max_keepalive_connections=20),
    def test_follow_redirects(self, respx_mock: MockRouter, client: OpenAI) -> None:
        # Test that the default follow_redirects=True allows following redirects
        respx_mock.post("/redirect").mock(
            return_value=httpx.Response(302, headers={"Location": f"{base_url}/redirected"})
        respx_mock.get("/redirected").mock(return_value=httpx.Response(200, json={"status": "ok"}))
        response = client.post("/redirect", body={"key": "value"}, cast_to=httpx.Response)
        assert response.json() == {"status": "ok"}
    def test_follow_redirects_disabled(self, respx_mock: MockRouter, client: OpenAI) -> None:
        # Test that follow_redirects=False prevents following redirects
        with pytest.raises(APIStatusError) as exc_info:
            client.post("/redirect", body={"key": "value"}, options={"follow_redirects": False}, cast_to=httpx.Response)
        assert exc_info.value.response.status_code == 302
        assert exc_info.value.response.headers["Location"] == f"{base_url}/redirected"
    def test_api_key_before_after_refresh_provider(self) -> None:
        client = OpenAI(base_url=base_url, api_key=lambda: "test_bearer_token")
        assert client.api_key == ""
        assert "Authorization" not in client.auth_headers
        client._refresh_api_key()
        assert client.api_key == "test_bearer_token"
        assert client.auth_headers.get("Authorization") == "Bearer test_bearer_token"
    def test_api_key_before_after_refresh_str(self) -> None:
        client = OpenAI(base_url=base_url, api_key="test_api_key")
        assert client.auth_headers.get("Authorization") == "Bearer test_api_key"
    @pytest.mark.respx()
    def test_api_key_refresh_on_retry(self, respx_mock: MockRouter) -> None:
        respx_mock.post(base_url + "/chat/completions").mock(
            side_effect=[
                httpx.Response(500, json={"error": "server error"}),
                httpx.Response(200, json={"foo": "bar"}),
        counter = 0
        def token_provider() -> str:
            nonlocal counter
            counter += 1
            if counter == 1:
                return "first"
            return "second"
        client = OpenAI(base_url=base_url, api_key=token_provider)
        client.chat.completions.create(messages=[], model="gpt-4")
        calls = cast("list[MockRequestCall]", respx_mock.calls)
        assert len(calls) == 2
        assert calls[0].request.headers.get("Authorization") == "Bearer first"
        assert calls[1].request.headers.get("Authorization") == "Bearer second"
    def test_copy_auth(self) -> None:
        client = OpenAI(base_url=base_url, api_key=lambda: "test_bearer_token_1").copy(
            api_key=lambda: "test_bearer_token_2"
        assert client.auth_headers == {"Authorization": "Bearer test_bearer_token_2"}
class TestAsyncOpenAI:
    async def test_raw_response(self, respx_mock: MockRouter, async_client: AsyncOpenAI) -> None:
        response = await async_client.post("/foo", cast_to=httpx.Response)
    async def test_raw_response_for_binary(self, respx_mock: MockRouter, async_client: AsyncOpenAI) -> None:
    def test_copy(self, async_client: AsyncOpenAI) -> None:
        copied = async_client.copy()
        assert id(copied) != id(async_client)
        copied = async_client.copy(api_key="another My API Key")
        assert async_client.api_key == "My API Key"
    def test_copy_default_options(self, async_client: AsyncOpenAI) -> None:
        copied = async_client.copy(max_retries=7)
        assert async_client.max_retries == 2
        assert isinstance(async_client.timeout, httpx.Timeout)
        copied = async_client.copy(timeout=None)
    async def test_copy_default_headers(self) -> None:
        client = AsyncOpenAI(
        await client.close()
    async def test_copy_default_query(self) -> None:
    def test_copy_signature(self, async_client: AsyncOpenAI) -> None:
            async_client.__init__,  # type: ignore[misc]
        copy_signature = inspect.signature(async_client.copy)
    def test_copy_build_request(self, async_client: AsyncOpenAI) -> None:
            client_copy = async_client.copy()
    async def test_request_timeout(self, async_client: AsyncOpenAI) -> None:
        request = async_client._build_request(FinalRequestOptions(method="get", url="/foo"))
        request = async_client._build_request(
            FinalRequestOptions(method="get", url="/foo", timeout=httpx.Timeout(100.0))
    async def test_client_timeout_option(self) -> None:
            base_url=base_url, api_key=api_key, _strict_response_validation=True, timeout=httpx.Timeout(0)
    async def test_http_client_timeout_option(self) -> None:
        async with httpx.AsyncClient(timeout=None) as http_client:
        async with httpx.AsyncClient(timeout=HTTPX_DEFAULT_TIMEOUT) as http_client:
    def test_invalid_http_client(self) -> None:
                AsyncOpenAI(
    async def test_default_headers_option(self) -> None:
        test_client = AsyncOpenAI(
        test_client2 = AsyncOpenAI(
        await test_client.close()
        await test_client2.close()
    async def test_validate_headers(self) -> None:
        client = AsyncOpenAI(base_url=base_url, api_key=api_key, _strict_response_validation=True)
        options = await client._prepare_options(FinalRequestOptions(method="get", url="/foo"))
                client2 = AsyncOpenAI(base_url=base_url, api_key=None, _strict_response_validation=True)
    async def test_default_query_option(self) -> None:
    def test_multipart_repeating_array(self, async_client: AsyncOpenAI) -> None:
    async def test_binary_content_upload(self, respx_mock: MockRouter, async_client: AsyncOpenAI) -> None:
        response = await async_client.post(
    async def test_binary_content_upload_with_asynciterator(self) -> None:
        iterator = _make_async_iterator([file_content], counter=counter)
        async def mock_handler(request: httpx.Request) -> httpx.Response:
            return httpx.Response(200, content=await request.aread())
            http_client=httpx.AsyncClient(transport=MockTransport(handler=mock_handler)),
            response = await client.post(
    async def test_binary_content_upload_with_body_is_deprecated(
        self, respx_mock: MockRouter, async_client: AsyncOpenAI
    async def test_basic_union_response(self, respx_mock: MockRouter, async_client: AsyncOpenAI) -> None:
        response = await async_client.get("/foo", cast_to=cast(Any, Union[Model1, Model2]))
    async def test_union_response_different_types(self, respx_mock: MockRouter, async_client: AsyncOpenAI) -> None:
    async def test_non_application_json_content_type_for_json_data(
        response = await async_client.get("/foo", cast_to=Model)
    async def test_base_url_setter(self) -> None:
            base_url="https://example.com/from_init", api_key=api_key, _strict_response_validation=True
    async def test_base_url_env(self) -> None:
            client = AsyncOpenAI(api_key=api_key, _strict_response_validation=True)
                base_url="http://localhost:5000/custom/path/", api_key=api_key, _strict_response_validation=True
                http_client=httpx.AsyncClient(),
    async def test_base_url_trailing_slash(self, client: AsyncOpenAI) -> None:
    async def test_base_url_no_trailing_slash(self, client: AsyncOpenAI) -> None:
    async def test_absolute_request_url(self, client: AsyncOpenAI) -> None:
    async def test_copied_client_does_not_close_http(self) -> None:
        test_client = AsyncOpenAI(base_url=base_url, api_key=api_key, _strict_response_validation=True)
        await asyncio.sleep(0.2)
    async def test_client_context_manager(self) -> None:
        async with test_client as c2:
    async def test_client_response_validation_error(self, respx_mock: MockRouter, async_client: AsyncOpenAI) -> None:
            await async_client.get("/foo", cast_to=Model)
    async def test_client_max_retries_validation(self) -> None:
                base_url=base_url, api_key=api_key, _strict_response_validation=True, max_retries=cast(Any, None)
    async def test_default_stream_cls(self, respx_mock: MockRouter, async_client: AsyncOpenAI) -> None:
        stream = await async_client.post("/foo", cast_to=Model, stream=True, stream_cls=AsyncStream[Model])
        assert isinstance(stream, AsyncStream)
        await stream.response.aclose()
    async def test_received_text_for_expected_json(self, respx_mock: MockRouter) -> None:
        strict_client = AsyncOpenAI(base_url=base_url, api_key=api_key, _strict_response_validation=True)
            await strict_client.get("/foo", cast_to=Model)
        non_strict_client = AsyncOpenAI(base_url=base_url, api_key=api_key, _strict_response_validation=False)
        response = await non_strict_client.get("/foo", cast_to=Model)
        await strict_client.close()
        await non_strict_client.close()
    async def test_parse_retry_after_header(
        self, remaining_retries: int, retry_after: str, timeout: float, async_client: AsyncOpenAI
        calculated = async_client._calculate_retry_timeout(remaining_retries, options, headers)
    async def test_retrying_timeout_errors_doesnt_leak(self, respx_mock: MockRouter, async_client: AsyncOpenAI) -> None:
            await async_client.chat.completions.with_streaming_response.create(
            ).__aenter__()
        assert _get_open_connections(async_client) == 0
    async def test_retrying_status_errors_doesnt_leak(self, respx_mock: MockRouter, async_client: AsyncOpenAI) -> None:
    async def test_retries_taken(
        async_client: AsyncOpenAI,
        client = async_client.with_options(max_retries=4)
        response = await client.chat.completions.with_raw_response.create(
    async def test_omit_retry_count_header(
        self, async_client: AsyncOpenAI, failures_before_success: int, respx_mock: MockRouter
    async def test_overwrite_retry_count_header(
    async def test_retries_taken_new_response_class(
        async with client.chat.completions.with_streaming_response.create(
    async def test_get_platform(self) -> None:
        platform = await asyncify(get_platform)()
        assert isinstance(platform, (str, OtherPlatform))
    async def test_proxy_environment_variables(self, monkeypatch: pytest.MonkeyPatch) -> None:
        client = DefaultAsyncHttpxClient()
    async def test_default_client_creation(self) -> None:
        DefaultAsyncHttpxClient(
    async def test_follow_redirects(self, respx_mock: MockRouter, async_client: AsyncOpenAI) -> None:
        response = await async_client.post("/redirect", body={"key": "value"}, cast_to=httpx.Response)
    async def test_follow_redirects_disabled(self, respx_mock: MockRouter, async_client: AsyncOpenAI) -> None:
            await async_client.post(
                "/redirect", body={"key": "value"}, options={"follow_redirects": False}, cast_to=httpx.Response
    @pytest.mark.asyncio
    async def test_api_key_before_after_refresh_provider(self) -> None:
        async def mock_api_key_provider():
            return "test_bearer_token"
        client = AsyncOpenAI(base_url=base_url, api_key=mock_api_key_provider)
        await client._refresh_api_key()
    async def test_api_key_before_after_refresh_str(self) -> None:
        client = AsyncOpenAI(base_url=base_url, api_key="test_api_key")
    async def test_bearer_token_refresh_async(self, respx_mock: MockRouter) -> None:
        async def token_provider() -> str:
        client = AsyncOpenAI(base_url=base_url, api_key=token_provider)
        await client.chat.completions.create(messages=[], model="gpt-4")
    async def test_copy_auth(self) -> None:
        async def token_provider_1() -> str:
            return "test_bearer_token_1"
        async def token_provider_2() -> str:
            return "test_bearer_token_2"
        client = AsyncOpenAI(base_url=base_url, api_key=token_provider_1).copy(api_key=token_provider_2)
class SimpleDatabaseClientTests(SimpleTestCase):
        self.client = BaseDatabaseClient(connection=connection)
    def test_settings_to_cmd_args_env(self):
        with self.assertRaisesMessage(NotImplementedError, msg):
            self.client.settings_to_cmd_args_env(None, None)
    def test_runshell_use_environ(self):
        for env in [None, {}]:
            with self.subTest(env=env):
                with mock.patch("subprocess.run") as run:
                    with mock.patch.object(
                        BaseDatabaseClient,
                        "settings_to_cmd_args_env",
                        return_value=([], env),
                        self.client.runshell(None)
                    run.assert_called_once_with([], env=None, check=True)
from posthog.contexts import get_context_session_id, new_context, set_context_session
from posthog.request import APIError, GetResponse
from posthog.test.test_utils import FAKE_TEST_API_KEY
from posthog.types import FeatureFlag, LegacyFlagMetadata
from posthog.contexts import tag
class TestClient(unittest.TestCase):
        # This ensures no real HTTP POST requests are made
        cls.client_post_patcher = mock.patch("posthog.client.batch_post")
        cls.consumer_post_patcher = mock.patch("posthog.consumer.batch_post")
        cls.client_post_patcher.start()
        cls.consumer_post_patcher.start()
        cls.client_post_patcher.stop()
        cls.consumer_post_patcher.stop()
    def set_fail(self, e, batch):
        """Mark the failure handler"""
        print("FAIL", e, batch)  # noqa: T201
        self.failed = True
        self.failed = False
        self.client = Client(FAKE_TEST_API_KEY, on_error=self.set_fail)
    def test_requires_api_key(self):
        self.assertRaises(TypeError, Client)
    def test_empty_flush(self):
        self.client.flush()
    def test_basic_capture(self):
        with mock.patch("posthog.client.batch_post") as mock_post:
            client = Client(FAKE_TEST_API_KEY, on_error=self.set_fail, sync_mode=True)
            msg_uuid = client.capture("python test event", distinct_id="distinct_id")
            self.assertIsNotNone(msg_uuid)
            self.assertFalse(self.failed)
            # Get the enqueued message from the mock
            mock_post.assert_called_once()
            batch_data = mock_post.call_args[1]["batch"]
            msg = batch_data[0]
            self.assertEqual(msg["event"], "python test event")
            self.assertTrue(isinstance(msg["timestamp"], str))
            self.assertIsNotNone(msg.get("uuid"))
            self.assertEqual(msg["distinct_id"], "distinct_id")
            self.assertEqual(msg["properties"]["$lib"], "posthog-python")
            self.assertEqual(msg["properties"]["$lib_version"], VERSION)
            # these will change between platforms so just asssert on presence here
            assert msg["properties"]["$python_runtime"] == mock.ANY
            assert msg["properties"]["$python_version"] == mock.ANY
            assert msg["properties"]["$os"] == mock.ANY
            assert msg["properties"]["$os_version"] == mock.ANY
    def test_basic_capture_with_uuid(self):
            uuid = str(uuid4())
            msg_uuid = client.capture(
                "python test event", distinct_id="distinct_id", uuid=uuid
            self.assertEqual(msg_uuid, uuid)
            self.assertEqual(msg["uuid"], uuid)
    def test_basic_capture_with_project_api_key(self):
                project_api_key=FAKE_TEST_API_KEY,
                on_error=self.set_fail,
                sync_mode=True,
    def test_basic_super_properties(self):
                FAKE_TEST_API_KEY,
                super_properties={"source": "repo-name"},
            # Check the enqueued message
            self.assertEqual(msg["properties"]["source"], "repo-name")
    def test_basic_capture_exception(self):
        with mock.patch.object(Client, "capture", return_value=None) as patch_capture:
            client = self.client
            exception = Exception("test exception")
            client.capture_exception(exception, distinct_id="distinct_id")
            self.assertTrue(patch_capture.called)
            capture_call = patch_capture.call_args
            self.assertEqual(capture_call[0][0], "$exception")
            self.assertEqual(capture_call[1]["distinct_id"], "distinct_id")
    def test_basic_capture_exception_with_distinct_id(self):
    def test_basic_capture_exception_with_correct_host_generation(self):
                FAKE_TEST_API_KEY, on_error=self.set_fail, host="https://aloha.com"
            call = patch_capture.call_args
            self.assertEqual(call[0][0], "$exception")
            self.assertEqual(call[1]["distinct_id"], "distinct_id")
    def test_basic_capture_exception_with_correct_host_generation_for_server_hosts(
                host="https://app.posthog.com",
    def test_basic_capture_exception_with_no_exception_given(self):
                raise Exception("test exception")
                client.capture_exception(None, distinct_id="distinct_id")
            print(capture_call)
                capture_call[1]["properties"]["$exception_list"][0]["mechanism"][
                "generic",
                    "handled"
                capture_call[1]["properties"]["$exception_list"][0]["module"], None
                capture_call[1]["properties"]["$exception_list"][0]["type"], "Exception"
                capture_call[1]["properties"]["$exception_list"][0]["value"],
                "test exception",
                capture_call[1]["properties"]["$exception_list"][0]["stacktrace"][
                "raw",
                    "frames"
                ][0]["filename"],
                "posthog/test/test_client.py",
                ][0]["function"],
                "test_basic_capture_exception_with_no_exception_given",
                ][0]["module"],
                "posthog.test.test_client",
                ][0]["in_app"],
    def test_basic_capture_exception_with_no_exception_happening(self):
            with self.assertLogs("posthog", level="WARNING") as logs:
                client.capture_exception(None)
                self.assertFalse(patch_capture.called)
                    logs.output[0],
                    "WARNING:posthog:No exception information available",
    def test_capture_exception_logs_when_enabled(self):
        client = Client(FAKE_TEST_API_KEY, log_captured_exceptions=True)
        with self.assertLogs("posthog", level="ERROR") as logs:
            client.capture_exception(
                Exception("test exception"), distinct_id="distinct_id"
                logs.output[0], "ERROR:posthog:test exception\nNoneType: None"
    @mock.patch("posthog.client.flags")
    def test_basic_capture_with_feature_flags(self, patch_flags):
        patch_flags.return_value = {"featureFlags": {"beta-feature": "random-variant"}}
                personal_api_key=FAKE_TEST_API_KEY,
                "python test event", distinct_id="distinct_id", send_feature_flags=True
                msg["properties"]["$feature/beta-feature"], "random-variant"
                msg["properties"]["$active_feature_flags"], ["beta-feature"]
            self.assertEqual(patch_flags.call_count, 1)
    def test_basic_capture_with_locally_evaluated_feature_flags(self, patch_flags):
        multivariate_flag = {
            "id": 1,
            "name": "Beta Feature",
            "key": "beta-feature-local",
            "active": True,
            "rollout_percentage": 100,
            "filters": {
                "groups": [
                                "key": "email",
                                "type": "person",
                                "value": "test@posthog.com",
                                "operator": "exact",
                        "rollout_percentage": 50,
                "multivariate": {
                    "variants": [
                            "key": "first-variant",
                            "name": "First Variant",
                            "key": "second-variant",
                            "name": "Second Variant",
                            "rollout_percentage": 25,
                            "key": "third-variant",
                            "name": "Third Variant",
                "payloads": {
                    "first-variant": "some-payload",
                    "third-variant": {"a": "json"},
        basic_flag = {
            "key": "person-flag",
                                "key": "region",
                                "value": ["USA"],
                "payloads": {"true": 300},
        false_flag = {
            "key": "false-flag",
                        "rollout_percentage": 0,
            client.feature_flags = [multivariate_flag, basic_flag, false_flag]
                msg["properties"]["$feature/beta-feature-local"], "third-variant"
            self.assertEqual(msg["properties"]["$feature/false-flag"], False)
                msg["properties"]["$active_feature_flags"], ["beta-feature-local"]
            assert "$feature/beta-feature" not in msg["properties"]
            self.assertEqual(patch_flags.call_count, 0)
        # test that flags are not evaluated without local evaluation
            client.feature_flags = []
            assert "$feature/beta-feature-local" not in msg["properties"]
            assert "$feature/false-flag" not in msg["properties"]
            assert "$active_feature_flags" not in msg["properties"]
    @mock.patch("posthog.client.get")
    def test_load_feature_flags_quota_limited(self, patch_get):
        mock_response = {
            "type": "quota_limited",
            "detail": "You have exceeded your feature flag request quota",
            "code": "payment_required",
        patch_get.side_effect = APIError(402, mock_response["detail"])
        client = Client(FAKE_TEST_API_KEY, personal_api_key="test")
            client._load_feature_flags()
            self.assertEqual(client.feature_flags, [])
            self.assertEqual(client.feature_flags_by_key, {})
            self.assertEqual(client.group_type_mapping, {})
            self.assertEqual(client.cohorts, {})
            self.assertIn("PostHog feature flags quota limited", logs.output[0])
    def test_load_feature_flags_unauthorized(self, patch_get):
        patch_get.side_effect = APIError(401, "Unauthorized")
            self.assertIn("please set a valid personal_api_key", logs.output[0])
    def test_dont_override_capture_with_local_flags(self, patch_flags):
            client.feature_flags = [multivariate_flag, basic_flag]
                "python test event",
                distinct_id="distinct_id",
                properties={"$feature/beta-feature-local": "my-custom-variant"},
                send_feature_flags=True,
                msg["properties"]["$feature/beta-feature-local"], "my-custom-variant"
            assert "$feature/person-flag" not in msg["properties"]
    def test_basic_capture_with_feature_flags_returns_active_only(self, patch_flags):
        patch_flags.return_value = {
            "featureFlags": {
                "beta-feature": "random-variant",
                "alpha-feature": True,
                "off-feature": False,
            self.assertTrue(msg["properties"]["$geoip_disable"])
            self.assertEqual(msg["properties"]["$feature/alpha-feature"], True)
                msg["properties"]["$active_feature_flags"],
                ["beta-feature", "alpha-feature"],
            patch_flags.assert_called_with(
                "random_key",
                "https://us.i.posthog.com",
                timeout=3,
                groups={},
                person_properties={},
                group_properties={},
                geoip_disable=True,
                device_id=None,
    def test_basic_capture_with_feature_flags_and_disable_geoip_returns_correctly(
        self, patch_flags
                feature_flags_request_timeout_seconds=12,
                disable_geoip=False,
            self.assertTrue("$geoip_disable" not in msg["properties"])
                timeout=12,
                geoip_disable=False,
    def test_basic_capture_with_feature_flags_switched_off_doesnt_send_them(
                "python test event", distinct_id="distinct_id", send_feature_flags=False
            self.assertTrue("$feature/beta-feature" not in msg["properties"])
            self.assertTrue("$active_feature_flags" not in msg["properties"])
    def test_capture_with_send_feature_flags_false_and_local_evaluation_doesnt_send_flags(
        """Test that send_feature_flags=False with local evaluation enabled does NOT send flags"""
        patch_flags.return_value = {"featureFlags": {"beta-feature": "remote-variant"}}
        simple_flag = {
            "id": 2,
            "name": "Simple Flag",
            "key": "simple-flag",
            client.feature_flags = [multivariate_flag, simple_flag]
                send_feature_flags=False,
            # CRITICAL: Verify local flags are NOT included in the event
            self.assertNotIn("$feature/beta-feature-local", msg["properties"])
            self.assertNotIn("$feature/simple-flag", msg["properties"])
            self.assertNotIn("$active_feature_flags", msg["properties"])
            # CRITICAL: Verify the /flags API was NOT called
    def test_capture_with_send_feature_flags_true_and_local_evaluation_uses_local_flags(
        """Test that send_feature_flags=True with local evaluation enabled uses local flags without API call"""
        patch_flags.return_value = {"featureFlags": {"remote-flag": "remote-variant"}}
            # Verify local flags are included in the event
            self.assertIn("$feature/beta-feature-local", msg["properties"])
            self.assertIn("$feature/simple-flag", msg["properties"])
            self.assertEqual(msg["properties"]["$feature/simple-flag"], True)
            # Verify active feature flags are set correctly
            active_flags = msg["properties"]["$active_feature_flags"]
            self.assertIn("beta-feature-local", active_flags)
            self.assertIn("simple-flag", active_flags)
            # The remote flag should NOT be included since we used local evaluation
            self.assertNotIn("$feature/remote-flag", msg["properties"])
    def test_capture_with_send_feature_flags_options_only_evaluate_locally_true(
        """Test that SendFeatureFlagsOptions with only_evaluate_locally=True uses local evaluation"""
            # Set up local flags
            client.feature_flags = [
                    "key": "local-flag",
                                "properties": [{"key": "region", "value": "US"}],
            send_options = {
                "only_evaluate_locally": True,
                "person_properties": {"region": "US"},
                "test event", distinct_id="distinct_id", send_feature_flags=send_options
            # Verify flags() was not called (no remote evaluation)
            patch_flags.assert_not_called()
            # Check the message includes the local flag
            self.assertEqual(msg["properties"]["$feature/local-flag"], True)
            self.assertEqual(msg["properties"]["$active_feature_flags"], ["local-flag"])
    def test_capture_with_send_feature_flags_options_only_evaluate_locally_false(
        """Test that SendFeatureFlagsOptions with only_evaluate_locally=False forces remote evaluation"""
        patch_flags.return_value = {"featureFlags": {"remote-flag": "remote-value"}}
                "only_evaluate_locally": False,
                "person_properties": {"plan": "premium"},
                "group_properties": {"company": {"type": "enterprise"}},
                "test event",
                groups={"company": "acme"},
                send_feature_flags=send_options,
            # Verify flags() was called with the correct properties
            patch_flags.assert_called_once()
            call_args = patch_flags.call_args[1]
            self.assertEqual(call_args["person_properties"], {"plan": "premium"})
                call_args["group_properties"], {"company": {"type": "enterprise"}}
            # Check the message includes the remote flag
            self.assertEqual(msg["properties"]["$feature/remote-flag"], "remote-value")
    def test_capture_with_send_feature_flags_options_default_behavior(
        """Test that SendFeatureFlagsOptions without only_evaluate_locally defaults to remote evaluation"""
        patch_flags.return_value = {"featureFlags": {"default-flag": "default-value"}}
                "person_properties": {"subscription": "pro"},
            # Verify flags() was called (default to remote evaluation)
            self.assertEqual(call_args["person_properties"], {"subscription": "pro"})
            # Check the message includes the flag
                msg["properties"]["$feature/default-flag"], "default-value"
    def test_capture_exception_with_send_feature_flags_options(self, patch_flags):
        """Test that capture_exception also supports SendFeatureFlagsOptions"""
        patch_flags.return_value = {"featureFlags": {"exception-flag": True}}
                "person_properties": {"user_type": "admin"},
                raise ValueError("Test exception")
                msg_uuid = client.capture_exception(
                    e, distinct_id="distinct_id", send_feature_flags=send_options
            self.assertEqual(call_args["person_properties"], {"user_type": "admin"})
            self.assertEqual(msg["event"], "$exception")
            self.assertEqual(msg["properties"]["$feature/exception-flag"], True)
    def test_stringifies_distinct_id(self):
        # A large number that loses precision in node:
        # node -e "console.log(157963456373623802 + 1)" > 157963456373623800
                "python test event", distinct_id=157963456373623802
            self.assertEqual(msg["distinct_id"], "157963456373623802")
    def test_advanced_capture(self):
                properties={"property": "value"},
                timestamp=datetime(2014, 9, 3),
                uuid="new-uuid",
            self.assertEqual(msg_uuid, "new-uuid")
            self.assertEqual(msg["timestamp"], "2014-09-03T00:00:00+00:00")
            self.assertEqual(msg["properties"]["property"], "value")
            self.assertEqual(msg["uuid"], "new-uuid")
            self.assertTrue("$groups" not in msg["properties"])
    def test_groups_capture(self):
                "test_event",
                groups={"company": "id:5", "instance": "app.posthog.com"},
                msg["properties"]["$groups"],
                {"company": "id:5", "instance": "app.posthog.com"},
    def test_basic_set(self):
            msg_uuid = client.set(
                distinct_id="distinct_id", properties={"trait": "value"}
            self.assertEqual(msg["$set"]["trait"], "value")
    def test_advanced_set(self):
                properties={"trait": "value"},
    def test_basic_set_once(self):
            msg_uuid = client.set_once(
            self.assertEqual(msg["$set_once"]["trait"], "value")
    def test_advanced_set_once(self):
    def test_basic_group_identify(self):
            msg_uuid = client.group_identify("organization", "id:5")
            self.assertEqual(msg["event"], "$groupidentify")
                msg["properties"],
                    "$group_type": "organization",
                    "$group_key": "id:5",
                    "$group_set": {},
                    "$lib": "posthog-python",
                    "$lib_version": VERSION,
                    "$geoip_disable": True,
    def test_basic_group_identify_with_distinct_id(self):
            msg_uuid = client.group_identify(
                "organization", "id:5", distinct_id="distinct_id"
    def test_advanced_group_identify(self):
                "organization",
                "id:5",
                {"trait": "value"},
                    "$group_set": {"trait": "value"},
    def test_advanced_group_identify_with_distinct_id(self):
    def test_basic_alias(self):
            msg_uuid = client.alias("previousId", "distinct_id")
            self.assertEqual(msg["properties"]["distinct_id"], "previousId")
            self.assertEqual(msg["properties"]["alias"], "distinct_id")
            # test_name, session_id, additional_properties, expected_properties
            ("basic_session_id", "test-session-123", {}, {}),
                "session_id_with_other_properties",
                "test-session-456",
                    "custom_prop": "custom_value",
                    "$process_person_profile": False,
                    "$current_url": "https://example.com",
            ("session_id_uuid_format", str(uuid4()), {}, {}),
            ("session_id_numeric_string", "1234567890", {}, {}),
            ("session_id_empty_string", "", {}, {}),
            ("session_id_with_special_chars", "session-123_test.id", {}, {}),
    def test_capture_with_session_id_variations(
        self, test_name, session_id, additional_properties, expected_properties
            properties = {"$session_id": session_id, **additional_properties}
                "python test event", distinct_id="distinct_id", properties=properties
            self.assertEqual(msg["properties"]["$session_id"], session_id)
            # Check additional expected properties
            for key, value in expected_properties.items():
                self.assertEqual(msg["properties"][key], value)
    def test_session_id_preserved_with_groups(self):
            session_id = "group-session-101"
                properties={"$session_id": session_id},
    def test_session_id_with_anonymous_event(self):
            session_id = "anonymous-session-202"
                "anonymous_event",
                    "$session_id": session_id,
            self.assertEqual(msg["properties"]["$process_person_profile"], False)
            # test_name, event_name, session_id, additional_properties, expected_additional_properties
                "screen_event",
                "$screen",
                "special-session-505",
                {"$screen_name": "HomeScreen"},
                "survey_event",
                "survey sent",
                "survey-session-606",
                    "$survey_id": "survey_123",
                    "$survey_questions": [
                        {"id": "q1", "question": "How likely are you to recommend us?"}
                {"$survey_id": "survey_123"},
                "complex_properties_event",
                "complex_event",
                "mixed-session-707",
                    "$current_url": "https://example.com/page",
                    "$process_person_profile": True,
                    "custom_property": "custom_value",
                    "numeric_property": 42,
                    "boolean_property": True,
                "csp_violation",
                "$csp_violation",
                "csp-session-789",
                    "$csp_version": "1.0",
                    "$raw_user_agent": "Mozilla/5.0 Test Agent",
                    "$csp_document_url": "https://example.com/page",
                    "$csp_blocked_url": "https://malicious.com/script.js",
                    "$csp_violated_directive": "script-src",
    def test_session_id_with_different_event_types(
        test_name,
        event_name,
        additional_properties,
        expected_additional_properties,
                event_name, distinct_id="distinct_id", properties=properties
            self.assertEqual(msg["event"], event_name)
            for key, value in expected_additional_properties.items():
            # Verify system properties are still added
            # test_name, super_properties, event_session_id, expected_session_id, expected_super_props
                "super_properties_override_session_id",
                {"$session_id": "super-session", "source": "test"},
                "event-session-808",
                "super-session",
                {"source": "test"},
                "no_super_properties_conflict",
                {"source": "test", "version": "1.0"},
                "event-session-909",
                "empty_super_properties",
                "event-session-111",
                "super_properties_with_other_dollar_props",
                {"$current_url": "https://super.com", "source": "test"},
                "event-session-222",
    def test_session_id_with_super_properties_variations(
        super_properties,
        event_session_id,
        expected_session_id,
        expected_super_props,
                FAKE_TEST_API_KEY, super_properties=super_properties, sync_mode=True
                properties={"$session_id": event_session_id},
            self.assertEqual(msg["properties"]["$session_id"], expected_session_id)
            # Check expected super properties are present
            for key, value in expected_super_props.items():
    def test_flush(self):
        # set up the consumer with more requests than a single batch will allow
        for i in range(1000):
            client.capture(
                "event", distinct_id="distinct_id", properties={"trait": "value"}
        # We can't reliably assert that the queue is non-empty here; that's
        # a race condition. We do our best to load it up though.
        client.flush()
        # Make sure that the client queue is empty after flushing
        self.assertTrue(client.queue.empty())
    def test_shutdown(self):
                "test event", distinct_id="distinct_id", properties={"trait": "value"}
        client.shutdown()
        # we expect two things after shutdown:
        # 1. client queue is empty
        # 2. consumer thread has stopped
        for consumer in client.consumers:
            self.assertFalse(consumer.is_alive())
    def test_synchronous(self):
            client = Client(FAKE_TEST_API_KEY, sync_mode=True)
            msg_uuid = client.capture("test event", distinct_id="distinct_id")
            self.assertFalse(client.consumers)
            # Verify the message was sent immediately
    def test_overflow(self):
        client = Client(FAKE_TEST_API_KEY, max_queue_size=1)
        # Ensure consumer thread is no longer uploading
        client.join()
            client.capture("test event", distinct_id="distinct_id")
        # Make sure we are informed that the queue is at capacity
        self.assertIsNone(msg_uuid)
        Client(six.u("unicode_key"))
    def test_numeric_distinct_id(self):
        self.client.capture("python event", distinct_id=1234)
    def test_debug(self):
        Client("bad_key", debug=True)
    def test_gzip(self):
        client = Client(FAKE_TEST_API_KEY, on_error=self.fail, gzip=True)
    def test_user_defined_flush_at(self):
            FAKE_TEST_API_KEY, on_error=self.fail, flush_at=10, flush_interval=3
        def mock_post_fn(*args, **kwargs):
            self.assertEqual(len(kwargs["batch"]), 10)
        # the post function should be called 2 times, with a batch size of 10
        # each time.
            "posthog.consumer.batch_post", side_effect=mock_post_fn
        ) as mock_post:
            for _ in range(20):
            self.assertEqual(mock_post.call_count, 2)
    def test_user_defined_timeout(self):
        client = Client(FAKE_TEST_API_KEY, timeout=10)
            self.assertEqual(consumer.timeout, 10)
    def test_default_timeout_15(self):
        client = Client(FAKE_TEST_API_KEY)
            self.assertEqual(consumer.timeout, 15)
    def test_disabled(self):
        client = Client(FAKE_TEST_API_KEY, on_error=self.set_fail, disabled=True)
    def test_disabled_with_feature_flags(self, patch_flags):
        response = client.get_feature_flag("beta-feature", "12345")
        self.assertIsNone(response)
        response = client.feature_enabled("beta-feature", "12345")
        response = client.get_all_flags("12345")
        response = client.get_feature_flag_payload("key", "12345")
        response = client.get_all_flags_and_payloads("12345")
        self.assertEqual(response, {"featureFlags": None, "featureFlagPayloads": None})
        # no capture calls
    def test_enabled_to_disabled(self):
            client.disabled = True
    def test_disable_geoip_default_on_events(self):
            capture_msg = batch_data[0]
            self.assertEqual(capture_msg["properties"]["$geoip_disable"], True)
    def test_disable_geoip_override_on_events(self):
                properties={"a": "b", "c": "d"},
                "event",
            # Check both calls were made
            # Check set event
            set_batch = mock_post.call_args_list[0][1]["batch"]
            capture_msg = set_batch[0]
            # Check page event
            page_batch = mock_post.call_args_list[1][1]["batch"]
            identify_msg = page_batch[0]
            self.assertEqual("$geoip_disable" not in identify_msg["properties"], True)
    def test_disable_geoip_method_overrides_init_on_events(self):
                "python test event", distinct_id="distinct_id", disable_geoip=False
    def test_disable_geoip_default_on_decide(self, patch_flags):
        client = Client(FAKE_TEST_API_KEY, on_error=self.set_fail, disable_geoip=False)
        client.get_feature_flag("random_key", "some_id", disable_geoip=True)
            distinct_id="some_id",
            person_properties={"distinct_id": "some_id"},
            flag_keys_to_evaluate=["random_key"],
        patch_flags.reset_mock()
        client.feature_enabled(
            "random_key", "feature_enabled_distinct_id", disable_geoip=True
            distinct_id="feature_enabled_distinct_id",
            person_properties={"distinct_id": "feature_enabled_distinct_id"},
        client.get_all_flags_and_payloads("all_flags_payloads_id")
            distinct_id="all_flags_payloads_id",
            person_properties={"distinct_id": "all_flags_payloads_id"},
    @mock.patch("posthog.client.Poller")
    def test_call_identify_fails(self, patch_get, patch_poller):
        def raise_effect():
            raise Exception("http exception")
        patch_get.return_value.raiseError.side_effect = raise_effect
        client.feature_flags = [{"key": "example"}]
        self.assertFalse(client.feature_enabled("example", "distinct_id"))
    def test_default_properties_get_added_properly(self, patch_flags):
            host="http://app2.posthog.com",
        client.get_feature_flag(
            "some_id",
            person_properties={"x1": "y1"},
            group_properties={"company": {"x": "y"}},
            "http://app2.posthog.com",
            person_properties={"distinct_id": "some_id", "x1": "y1"},
            group_properties={
                "company": {"$group_key": "id:5", "x": "y"},
                "instance": {"$group_key": "app.posthog.com"},
            person_properties={"distinct_id": "override"},
                "company": {
                    "$group_key": "group_override",
                "company": {"$group_key": "group_override"},
        # test nones
        client.get_all_flags_and_payloads(
            "some_id", groups={}, person_properties=None, group_properties=None
            # method, method_args, expected_person_props, expected_flag_keys
                ["random_key", "some_id"],
                {"distinct_id": "some_id"},
                ["random_key"],
                ["some_id"],
            ("get_all_flags", ["some_id"], {"distinct_id": "some_id"}, None),
            ("get_flags_decision", ["some_id"], {}, None),
    def test_device_id_is_passed_to_flags_request(
        method_args,
        expected_person_props,
        expected_flag_keys,
        patch_flags,
        """Test that device_id is properly passed to the flags request when provided."""
        client = Client(FAKE_TEST_API_KEY, on_error=self.set_fail)
        getattr(client, method)(*method_args, device_id="test-device-123")
        expected_call = {
            "distinct_id": "some_id",
            "groups": {},
            "person_properties": expected_person_props,
            "group_properties": {},
            "geoip_disable": True,
            "device_id": "test-device-123",
        if expected_flag_keys:
            expected_call["flag_keys_to_evaluate"] = expected_flag_keys
            "random_key", "https://us.i.posthog.com", timeout=3, **expected_call
    def test_device_id_from_context_is_used_in_flags_request(self, patch_flags):
        """Test that device_id from context is used in flags request when not explicitly provided."""
        from posthog.contexts import new_context, set_context_device_id
        # Test that device_id from context is used
            set_context_device_id("context-device-id")
            client.get_feature_flag("random_key", "some_id")
                device_id="context-device-id",
        # Test that explicit device_id overrides context
                "random_key", "some_id", device_id="explicit-device-id"
                device_id="explicit-device-id",
            # name, sys_platform, version_info, expected_runtime, expected_version, expected_os, expected_os_version, platform_method, platform_return, distro_info
                "macOS",
                "darwin",
                (3, 8, 10),
                "MockPython",
                "3.8.10",
                "Mac OS X",
                "10.15.7",
                "mac_ver",
                ("10.15.7", "", ""),
                "win32",
                "10",
                "win32_ver",
                ("10", "", "", ""),
                "linux",
                "20.04",
                {"version": "20.04"},
    def test_mock_system_context(
        _name,
        sys_platform,
        version_info,
        expected_runtime,
        expected_version,
        expected_os,
        expected_os_version,
        platform_method,
        platform_return,
        distro_info,
        """Test that we can mock platform and sys for testing system_context"""
        with mock.patch("posthog.utils.platform") as mock_platform:
            with mock.patch("posthog.utils.sys") as mock_sys:
                # Set up common mocks
                mock_platform.python_implementation.return_value = expected_runtime
                mock_sys.version_info = version_info
                mock_sys.platform = sys_platform
                # Set up platform-specific mocks
                if platform_method:
                    getattr(
                        mock_platform, platform_method
                    ).return_value = platform_return
                # Special handling for Linux which uses distro module
                if sys_platform == "linux":
                    # Directly patch the get_os_info function to return our expected values
                        "posthog.utils.get_os_info",
                        return_value=(expected_os, expected_os_version),
                        from posthog.utils import system_context
                        context = system_context()
                    # Get system context for non-Linux platforms
                # Verify results
                expected_context = {
                    "$python_runtime": expected_runtime,
                    "$python_version": expected_version,
                    "$os": expected_os,
                    "$os_version": expected_os_version,
                assert context == expected_context
    def test_get_decide_returns_normalized_decide_response(self, patch_flags):
            "featureFlagPayloads": {"beta-feature": '{"some": "data"}'},
            "errorsWhileComputingFlags": False,
            "requestId": "test-id",
        distinct_id = "test_distinct_id"
        groups = {"test_group_type": "test_group_id"}
        person_properties = {"test_property": "test_value"}
        response = client.get_flags_decision(distinct_id, groups, person_properties)
        assert response == {
            "flags": {
                "beta-feature": FeatureFlag(
                    key="beta-feature",
                    enabled=True,
                    variant="random-variant",
                    metadata=LegacyFlagMetadata(
                        payload='{"some": "data"}',
                "alpha-feature": FeatureFlag(
                    key="alpha-feature",
                    variant=None,
                        payload=None,
                "off-feature": FeatureFlag(
                    key="off-feature",
                    enabled=False,
    def test_set_context_session_with_capture(self):
                set_context_session("context-session-123")
                    properties={"custom_prop": "value"},
                    msg["properties"]["$session_id"], "context-session-123"
    def test_set_context_session_with_page_explicit_properties(self):
                set_context_session("page-explicit-session-789")
                    "$session_id": get_context_session_id(),
                    "page_type": "landing",
                    "$page", distinct_id="distinct_id", properties=properties
                    msg["properties"]["$session_id"], "page-explicit-session-789"
    def test_set_context_session_override_in_capture(self):
        """Test that explicit session ID overrides context session ID in capture"""
        from posthog.contexts import new_context, set_context_session
                set_context_session("context-session-override")
                        "$session_id": "explicit-session-override",
                        "custom_prop": "value",
                    msg["properties"]["$session_id"], "explicit-session-override"
    def test_enable_local_evaluation_false_disables_poller(
        self, patch_get, patch_poller
        """Test that when enable_local_evaluation=False, the poller is not started"""
        patch_get.return_value = GetResponse(
            data={
                "flags": [
                        "key": "beta-feature",
                "group_type_mapping": {},
                "cohorts": {},
            etag='"test-etag"',
            personal_api_key="test-personal-key",
            enable_local_evaluation=False,
        # Load feature flags should not start the poller
        client.load_feature_flags()
        # Assert that the poller was not created/started
        patch_poller.assert_not_called()
        # But the feature flags should still be loaded
        patch_get.assert_called_once()
        self.assertEqual(len(client.feature_flags), 1)
        self.assertEqual(client.feature_flags[0]["key"], "beta-feature")
    def test_enable_local_evaluation_true_starts_poller(self, patch_get, patch_poller):
        """Test that when enable_local_evaluation=True (default), the poller is started"""
        # Load feature flags should start the poller
        # Assert that the poller was created and started
        patch_poller.assert_called_once()
    @mock.patch("posthog.client.remote_config")
    def test_get_remote_config_payload_works_without_poller(self, patch_remote_config):
        """Test that get_remote_config_payload works without local evaluation enabled"""
        patch_remote_config.return_value = {"test": "payload"}
        # Should work without poller
        result = client.get_remote_config_payload("test-flag")
        self.assertEqual(result, {"test": "payload"})
        patch_remote_config.assert_called_once_with(
            "test-personal-key",
            client.host,
            "test-flag",
            timeout=client.feature_flags_request_timeout_seconds,
    def test_get_remote_config_payload_requires_personal_api_key(self):
        """Test that get_remote_config_payload requires personal API key"""
        self.assertIsNone(result)
    def test_parse_send_feature_flags_method(self):
        """Test the _parse_send_feature_flags helper method"""
        # Test boolean True
        result = client._parse_send_feature_flags(True)
        self.assertEqual(result, expected)
        # Test boolean False
        result = client._parse_send_feature_flags(False)
            "should_send": False,
        # Test options dict with all fields
        result = client._parse_send_feature_flags(options)
        # Test options dict with partial fields
        options = {"person_properties": {"user_id": "123"}}
            "person_properties": {"user_id": "123"},
        # Test empty dict
        result = client._parse_send_feature_flags({})
        # Test invalid types
        with self.assertRaises(TypeError) as cm:
            client._parse_send_feature_flags("invalid")
        self.assertIn("Invalid type for send_feature_flags", str(cm.exception))
            client._parse_send_feature_flags(123)
            client._parse_send_feature_flags(None)
    def test_capture_with_send_feature_flags_flag_keys_filter(self, patch_flags):
        """Test that SendFeatureFlagsOptions with flag_keys_filter only evaluates specified flags"""
        # When flag_keys_to_evaluate is provided, the API should only return the requested flags
                "flag1": "value1",
                "flag3": "value3",
                "flag_keys_filter": ["flag1", "flag3"],
            # Verify flags() was called with flag_keys_to_evaluate
            self.assertEqual(call_args["flag_keys_to_evaluate"], ["flag1", "flag3"])
            # Check the message includes only the filtered flags
            self.assertEqual(msg["properties"]["$feature/flag1"], "value1")
            self.assertEqual(msg["properties"]["$feature/flag3"], "value3")
            # flag2 should not be included since it wasn't requested
            self.assertNotIn("$feature/flag2", msg["properties"])
    @mock.patch("posthog.client.batch_post")
    def test_get_feature_flag_result_with_empty_string_payload(self, patch_batch_post):
        """Test that get_feature_flag_result returns a FeatureFlagResult when payload is empty string"""
            personal_api_key="test_personal_api_key",
        # Set up local evaluation with a flag that has empty string payload
                "name": "Test flag",
                "key": "test-flag",
                "is_simple_flag": False,
                "rollout_percentage": None,
                            "variant": "empty-variant",
                                "key": "empty-variant",
                                "name": "Empty Variant",
                    "payloads": {"empty-variant": ""},  # Empty string payload
        # Test get_feature_flag_result
        result = client.get_feature_flag_result(
            "test-flag", "test-user", only_evaluate_locally=True
        # Should return a FeatureFlagResult, not None
        self.assertIsNotNone(result)
        self.assertEqual(result.key, "test-flag")
        self.assertEqual(result.get_value(), "empty-variant")
        self.assertEqual(result.payload, "")  # Should be empty string, not None
    def test_get_all_flags_and_payloads_with_empty_string(self, patch_batch_post):
        """Test that get_all_flags_and_payloads includes flags with empty string payloads"""
        # Set up multiple flags with different payload types
                "name": "Flag with empty payload",
                "key": "empty-payload-flag",
                    "groups": [{"properties": [], "variant": "variant1"}],
                        "variants": [{"key": "variant1", "rollout_percentage": 100}]
                    "payloads": {"variant1": ""},  # Empty string
                "name": "Flag with normal payload",
                "key": "normal-payload-flag",
                    "groups": [{"properties": [], "variant": "variant2"}],
                        "variants": [{"key": "variant2", "rollout_percentage": 100}]
                    "payloads": {"variant2": "normal payload"},
        result = client.get_all_flags_and_payloads(
            "test-user", only_evaluate_locally=True
        # Check that both flags are included
        self.assertEqual(result["featureFlags"]["empty-payload-flag"], "variant1")
        self.assertEqual(result["featureFlags"]["normal-payload-flag"], "variant2")
        # Check that empty string payload is included (not filtered out)
        self.assertIn("empty-payload-flag", result["featureFlagPayloads"])
        self.assertEqual(result["featureFlagPayloads"]["empty-payload-flag"], "")
            result["featureFlagPayloads"]["normal-payload-flag"], "normal payload"
    def test_context_tags_added(self):
                tag("random_tag", 12345)
                client.capture("python test event", distinct_id="distinct_id")
            self.assertEqual(msg["properties"]["$context_tags"], ["random_tag"])
        "posthog.client.Client._enqueue", side_effect=Exception("Unexpected error")
    def test_methods_handle_exceptions(self, mock_enqueue):
        """Test that all decorated methods handle exceptions gracefully."""
        client = Client("test-key")
            ("capture", ["test_event"], {}),
            ("set", [], {"distinct_id": "some-id", "properties": {"a": "b"}}),
            ("set_once", [], {"distinct_id": "some-id", "properties": {"a": "b"}}),
            ("group_identify", ["group-type", "group-key"], {}),
            ("alias", ["some-id", "new-id"], {}),
        for method_name, args, kwargs in test_cases:
            with self.subTest(method=method_name):
                method = getattr(client, method_name)
                result = method(*args, **kwargs)
                self.assertEqual(result, None)
        "posthog.client.Client._enqueue", side_effect=Exception("Expected error")
    def test_debug_flag_re_raises_exceptions(self, mock_enqueue):
        """Test that methods re-raise exceptions when debug=True."""
        client = Client("test-key", debug=True)
                with self.assertRaises(Exception) as cm:
                    method(*args, **kwargs)
                self.assertEqual(str(cm.exception), "Expected error")
