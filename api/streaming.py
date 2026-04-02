# Appengine users: https://developers.google.com/appengine/docs/python/sockets/#making_httplib_use_sockets
import ssl
from threading import Thread
from time import sleep
from typing import NamedTuple
import urllib3
from tweepy.client import BaseClient, Response
StreamResponse = namedtuple(
    "StreamResponse", ("data", "includes", "errors", "matching_rules")
class BaseStream:
    def __init__(self, *, chunk_size=512, daemon=False, max_retries=inf,
                 proxy=None, verify=True):
        self.chunk_size = chunk_size
        self.daemon = daemon
        self.max_retries = max_retries
        self.proxies = {"https": proxy} if proxy else {}
        self.verify = verify
        self.running = False
        self.thread = None
    def _connect(
        self, method, url, auth=None, params=None, headers=None, body=None,
        timeout=21
        self.running = True
        error_count = 0
        # https://developer.twitter.com/en/docs/twitter-api/v1/tweets/filter-realtime/guides/connecting
        # https://developer.twitter.com/en/docs/twitter-api/tweets/filtered-stream/integrate/handling-disconnections
        # https://developer.twitter.com/en/docs/twitter-api/tweets/volume-streams/integrate/handling-disconnections
        network_error_wait = 0
        network_error_wait_step = 0.25
        network_error_wait_max = 16
        http_error_wait = http_error_wait_start = 5
        http_error_wait_max = 320
        http_429_error_wait_start = 60
        self.session.headers["User-Agent"] = self.user_agent
            while self.running and error_count <= self.max_retries:
                        method, url, params=params, headers=headers, data=body,
                        timeout=timeout, stream=True, auth=auth,
                        verify=self.verify, proxies=self.proxies
                    ) as resp:
                        if resp.status_code == 200:
                            http_error_wait = http_error_wait_start
                            self.on_connect()
                            if not self.running:
                            for line in resp.iter_lines(
                                chunk_size=self.chunk_size
                                if line:
                                    self.on_data(line)
                                    self.on_keep_alive()
                            if resp.raw.closed:
                                self.on_closed(resp)
                            self.on_request_error(resp.status_code)
                            # The error text is logged here instead of in
                            # on_request_error to keep on_request_error
                            # backwards-compatible. In a future version, the
                            # Response should be passed to on_request_error.
                            log.error(
                                "HTTP error response text: %s", resp.text
                            error_count += 1
                            if resp.status_code in (420, 429):
                                if http_error_wait < http_429_error_wait_start:
                                    http_error_wait = http_429_error_wait_start
                            sleep(http_error_wait)
                            http_error_wait *= 2
                            if http_error_wait > http_error_wait_max:
                                http_error_wait = http_error_wait_max
                except (requests.ConnectionError, requests.Timeout,
                        requests.exceptions.ChunkedEncodingError,
                        ssl.SSLError, urllib3.exceptions.ReadTimeoutError,
                        urllib3.exceptions.ProtocolError) as exc:
                    # This is still necessary, as a SSLError can actually be
                    # thrown when using Requests
                    # If it's not time out treat it like any other exception
                    if isinstance(exc, ssl.SSLError):
                        if not (exc.args and "timed out" in str(exc.args[0])):
                    self.on_connection_error()
                    # on_connection_error to keep on_connection_error
                    # backwards-compatible. In a future version, the error
                    # should be passed to on_connection_error.
                        "Connection error: %s",
                        "".join(
                            traceback.format_exception_only(type(exc), exc)
                        ).rstrip()
                    sleep(network_error_wait)
                    network_error_wait += network_error_wait_step
                    if network_error_wait > network_error_wait_max:
                        network_error_wait = network_error_wait_max
        except Exception as exc:
            self.on_exception(exc)
            self.on_disconnect()
    def _threaded_connect(self, *args, **kwargs):
        self.thread = Thread(target=self._connect, name="Tweepy Stream",
                             args=args, kwargs=kwargs, daemon=self.daemon)
        self.thread.start()
        return self.thread
    def disconnect(self):
        """Disconnect the stream"""
    def on_closed(self, response):
        """This is called when the stream has been closed by Twitter.
        response : requests.Response
            The Response from Twitter
        log.error("Stream connection closed by Twitter")
    def on_connect(self):
        """This is called after successfully connecting to the streaming API.
        log.info("Stream connected")
    def on_connection_error(self):
        """This is called when the stream connection errors or times out."""
        log.error("Stream connection has errored or timed out")
    def on_disconnect(self):
        """This is called when the stream has disconnected."""
        log.info("Stream disconnected")
    def on_exception(self, exception):
        """This is called when an unhandled exception occurs.
        exception : Exception
            The unhandled exception
        log.exception("Stream encountered an exception")
    def on_keep_alive(self):
        """This is called when a keep-alive signal is received."""
        log.debug("Received keep-alive signal")
    def on_request_error(self, status_code):
        """This is called when a non-200 HTTP status code is encountered.
        status_code : int
            The HTTP status code encountered
        log.error("Stream encountered HTTP error: %d", status_code)
class StreamingClient(BaseClient, BaseStream):
    """Filter and sample realtime Tweets with Twitter API v2
    bearer_token : str
        Twitter API Bearer Token
        Whether or not to wait before retrying when a rate limit is
        encountered. This applies to requests besides those that connect to a
        stream (see ``max_retries``).
    chunk_size : int
        The default socket.read size. Default to 512, less than half the size
        of a Tweet so that it reads Tweets with the minimal latency of 2 reads
        per Tweet. Values higher than ~1kb will increase latency by waiting for
        more data to arrive but may also increase throughput by doing fewer
        socket read calls.
    daemon : bool
        Whether or not to use a daemon thread when using a thread to run the
        stream
    max_retries : int
        Max number of times to retry connecting the stream
    proxy : str | None
        URL of the proxy to use when connecting to the stream
    verify : bool | str
        Either a boolean, in which case it controls whether to verify the
        server’s TLS certificate, or a string, in which case it must be a path
        to a CA bundle to use.
    running : bool
        Whether there's currently a stream running
    session : :class:`requests.Session`
        Requests Session used to connect to the stream
    thread : :class:`threading.Thread` | None
        Thread used to run the stream
        User agent used when connecting to the stream
    def __init__(self, bearer_token, *, return_type=Response,
                 wait_on_rate_limit=False, **kwargs):
        """__init__( \
            bearer_token, *, return_type=Response, wait_on_rate_limit=False, \
            chunk_size=512, daemon=False, max_retries=inf, proxy=None, \
            verify=True \
        BaseClient.__init__(self, bearer_token, return_type=return_type,
                            wait_on_rate_limit=wait_on_rate_limit)
        BaseStream.__init__(self, **kwargs)
    def _connect(self, method, endpoint, **kwargs):
        self.session.headers["Authorization"] = f"Bearer {self.bearer_token}"
        url = f"https://api.twitter.com/2/tweets/{endpoint}/stream"
        super()._connect(method, url, **kwargs)
        if data_type is StreamRule:
                rules = []
                for rule in data:
                    if "tag" in rule:
                        rules.append(StreamRule(
                            value=rule["value"], id=rule["id"], tag=rule["tag"]
                        rules.append(StreamRule(value=rule["value"],
                                                id=rule["id"]))
                return rules
                if "tag" in data:
                    return StreamRule(value=data["value"], id=data["id"],
                                      tag=data["tag"])
                    return StreamRule(value=data["value"], id=data["id"])
            return super()._process_data(data, data_type=data_type)
    def add_rules(self, add, **params):
        """add_rules(add, *, dry_run)
        Add rules to filtered stream.
        add : list[StreamRule] | StreamRule
            Specifies the operation you want to perform on the rules.
        dry_run : bool
            Set to true to test the syntax of your rule without submitting it.
            This is useful if you want to check the syntax of a rule before
            removing one or more of your existing rules.
        https://developer.twitter.com/en/docs/twitter-api/tweets/filtered-stream/api-reference/post-tweets-search-stream-rules
        json = {"add": []}
        if isinstance(add, StreamRule):
            add = (add,)
        for rule in add:
            if rule.tag is not None:
                json["add"].append({"value": rule.value, "tag": rule.tag})
                json["add"].append({"value": rule.value})
            "POST", f"/2/tweets/search/stream/rules", params=params,
            endpoint_parameters=("dry_run",), json=json, data_type=StreamRule
    def delete_rules(self, ids, **params):
        """delete_rules(ids, *, dry_run)
        Delete rules from filtered stream.
        ids : int | str | list[int | str | StreamRule] | StreamRule
            Array of rule IDs, each one representing a rule already active in
            your stream. IDs must be submitted as strings.
        json = {"delete": {"ids": []}}
        if isinstance(ids, (int, str, StreamRule)):
            ids = (ids,)
        for id in ids:
            if isinstance(id, StreamRule):
                json["delete"]["ids"].append(str(id.id))
                json["delete"]["ids"].append(str(id))
    def filter(self, *, threaded=False, **params):
        """filter( \
            *, backfill_minutes=None, expansions=None, media_fields=None, \
            place_fields=None, poll_fields=None, tweet_fields=None, \
            user_fields=None, threaded=False \
        Streams Tweets in real-time based on a specific set of filter rules.
        If you are using the academic research product track, you can connect
        up to two `redundant connections <filter redundant connections_>`_ to
        maximize your streaming up-time.
        backfill_minutes : int | None
            By passing this parameter, you can request up to five (5) minutes
            worth of streaming data that you might have missed during a
            disconnection to be delivered to you upon reconnection. The
            backfilled Tweets will automatically flow through the reconnected
            stream, with older Tweets generally being delivered before any
            newly matching Tweets. You must include a whole number between 1
            and 5 as the value to this parameter.
            This feature will deliver duplicate Tweets, meaning that if you
            were disconnected for 90 seconds, and you requested two minutes of
            backfill, you will receive 30 seconds worth of duplicate Tweets.
            Due to this, you should make sure your system is tolerant of
            duplicate data.
            This feature is currently only available to the Academic Research
            product track.
        expansions : list[str] | str
        media_fields : list[str] | str
        place_fields : list[str] | str
        poll_fields : list[str] | str
        tweet_fields : list[str] | str
        user_fields : list[str] | str
        threaded : bool
            Whether or not to use a thread to run the stream
        TweepyException
            When the stream is already connected
        threading.Thread | None
            The thread if ``threaded`` is set to ``True``, else ``None``
        https://developer.twitter.com/en/docs/twitter-api/tweets/filtered-stream/api-reference/get-tweets-search-stream
        .. _filter redundant connections: https://developer.twitter.com/en/docs/twitter-api/tweets/filtered-stream/integrate/recovery-and-redundancy-features
        .. _Tweet cap: https://developer.twitter.com/en/docs/twitter-api/tweet-caps
        if self.running:
            raise TweepyException("Stream is already connected")
        method = "GET"
        endpoint = "search"
        params = self._process_params(
            params, endpoint_parameters=(
                "backfill_minutes", "expansions", "media.fields",
                "place.fields", "poll.fields", "tweet.fields", "user.fields"
        if threaded:
            return self._threaded_connect(method, endpoint, params=params)
            self._connect(method, endpoint, params=params)
    def get_rules(self, **params):
        """get_rules(*, ids)
        Return a list of rules currently active on the streaming endpoint,
        either as a list or individually.
        ids : list[str] | str
            Comma-separated list of rule IDs. If omitted, all rules are
        https://developer.twitter.com/en/docs/twitter-api/tweets/filtered-stream/api-reference/get-tweets-search-stream-rules
            "GET", f"/2/tweets/search/stream/rules", params=params,
            endpoint_parameters=("ids",), data_type=StreamRule
    def sample(self, *, threaded=False, **params):
        """sample( \
        Streams about 1% of all Tweets in real-time.
        up to two `redundant connections <sample redundant connections_>`_ to
        https://developer.twitter.com/en/docs/twitter-api/tweets/volume-streams/api-reference/get-tweets-sample-stream
        .. _sample redundant connections: https://developer.twitter.com/en/docs/twitter-api/tweets/volume-streams/integrate/recovery-and-redundancy-features
        endpoint = "sample"
    def on_data(self, raw_data):
        """This is called when raw data is received from the stream.
        This method handles sending the data to other methods.
        raw_data : JSON
            The raw data from the stream
        https://developer.twitter.com/en/docs/twitter-api/tweets/filtered-stream/integrate/consuming-streaming-data
        data = json.loads(raw_data)
        tweet = None
        includes = {}
        errors = []
        matching_rules = []
        if "data" in data:
            tweet = Tweet(data["data"])
            self.on_tweet(tweet)
        if "includes" in data:
            includes = self._process_includes(data["includes"])
            self.on_includes(includes)
        if "errors" in data:
            errors = data["errors"]
            self.on_errors(errors)
        if "matching_rules" in data:
            matching_rules = [
                StreamRule(id=rule["id"], tag=rule["tag"])
                for rule in data["matching_rules"]
            self.on_matching_rules(matching_rules)
        self.on_response(
            StreamResponse(tweet, includes, errors, matching_rules)
    def on_tweet(self, tweet):
        """This is called when a Tweet is received.
        tweet : Tweet
            The Tweet received
    def on_includes(self, includes):
        """This is called when includes are received.
        includes : dict
            The includes received
    def on_errors(self, errors):
        """This is called when errors are received.
        errors : dict
            The errors received
        log.error("Received errors: %s", errors)
    def on_matching_rules(self, matching_rules):
        """This is called when matching rules are received.
        matching_rules : list[StreamRule]
            The matching rules received
    def on_response(self, response):
        """This is called when a response is received.
        response : StreamResponse
            The response received
        log.debug("Received response: %s", response)
class StreamRule(NamedTuple):
    """Rule for filtered stream
    value : str | None
        The rule text. If you are using a `Standard Project`_ at the Basic
        `access level`_, you can use the basic set of `operators`_, can submit
        up to 25 concurrent rules, and can submit rules up to 512 characters
        access level, you can use all available operators, can submit up to
        1,000 concurrent rules, and can submit rules up to 1,024 characters
        long.
    tag : str | None
        The tag label. This is a free-form text you can use to identify the
        rules that matched a specific Tweet in the streaming response. Tags can
        be the same across rules.
    id : str | None
        Unique identifier of this rule. This is returned as a string.
    .. _access level: https://developer.twitter.com/en/products/twitter-api/early-access/guide#na_1
    value: str = None
    tag: str = None
    id: str = None
from tweepy.asynchronous.client import AsyncBaseClient
from tweepy.streaming import StreamResponse, StreamRule
class AsyncBaseStream:
    def __init__(self, *, max_retries=inf, proxy=None):
        self.proxy = proxy
        self.task = None
    async def _connect(
        self, method, url, params=None, headers=None, body=None,
        oauth_client=None, timeout=21
        if self.session is None or self.session.closed:
            self.session = aiohttp.ClientSession(
                connector=aiohttp.TCPConnector(enable_cleanup_closed=True),
                timeout=aiohttp.ClientTimeout(sock_read=timeout)
            while error_count <= self.max_retries:
                    if oauth_client is not None:
                            url, http_method=method, headers=headers, body=body
                    async with self.session.request(
                        proxy=self.proxy
                        if resp.status == 200:
                            await self.on_connect()
                            async for line in resp.content:
                                line = line.strip()
                                    await self.on_data(line)
                                    await self.on_keep_alive()
                            await self.on_closed(resp)
                            await self.on_request_error(resp.status)
                            # ClientResponse should be passed to
                            # on_request_error.
                            response_text = await resp.text()
                                "HTTP error response text: %s", response_text
                            if resp.status in (420, 429):
                            await asyncio.sleep(http_error_wait)
                            if resp.status != 420:
                except (aiohttp.ClientConnectionError,
                        aiohttp.ClientPayloadError) as e:
                    await self.on_connection_error()
                            traceback.format_exception_only(type(e), e)
                    await asyncio.sleep(network_error_wait)
        except asyncio.CancelledError:
            await self.on_exception(e)
            await self.session.close()
            await self.on_disconnect()
        if self.task is not None:
            self.task.cancel()
    async def on_closed(self, resp):
        """|coroutine|
        This is called when the stream has been closed by Twitter.
        response : aiohttp.ClientResponse
            The response from Twitter
    async def on_connect(self):
        This is called after successfully connecting to the streaming API.
    async def on_connection_error(self):
        This is called when the stream connection errors or times out.
    async def on_disconnect(self):
        This is called when the stream has disconnected.
    async def on_exception(self, exception):
        This is called when an unhandled exception occurs.
    async def on_keep_alive(self):
        This is called when a keep-alive signal is received.
    async def on_request_error(self, status_code):
        This is called when a non-200 HTTP status code is encountered.
        log.error("Stream encountered HTTP Error: %d", status_code)
class AsyncStreamingClient(AsyncBaseClient, AsyncBaseStream):
    """Stream realtime Tweets asynchronously with Twitter API v2
    max_retries: int | None
        Number of times to attempt to (re)connect the stream.
    session : aiohttp.ClientSession | None
        Aiohttp client session used to connect to the API
    task : asyncio.Task | None
        The task running the stream
        User agent used when connecting to the API
            max_retries=inf, proxy=None \
        AsyncBaseClient.__init__(self, bearer_token, return_type=return_type,
        AsyncBaseStream.__init__(self, **kwargs)
    async def _connect(self, method, endpoint, **kwargs):
        headers = {"Authorization": f"Bearer {self.bearer_token}"}
        await super()._connect(method, url, headers=headers, **kwargs)
    async def add_rules(self, add, **params):
        |coroutine|
    async def delete_rules(self, ids, **params):
    def filter(self, **params):
        asyncio.Task
        if self.task is not None and not self.task.done():
        self.task = asyncio.create_task(
            self._connect("GET", endpoint, params=params)
        # Use name parameter when support for Python 3.7 is dropped
        return self.task
    async def get_rules(self, **params):
    def sample(self, **params):
    async def on_data(self, raw_data):
        This is called when raw data is received from the stream.
            await self.on_tweet(tweet)
            await self.on_includes(includes)
            await self.on_errors(errors)
            await self.on_matching_rules(matching_rules)
        await self.on_response(
    async def on_tweet(self, tweet):
        This is called when a Tweet is received.
    async def on_includes(self, includes):
        This is called when includes are received.
    async def on_errors(self, errors):
        This is called when errors are received.
    async def on_matching_rules(self, matching_rules):
        This is called when matching rules are received.
    async def on_response(self, response):
        This is called when a response is received.
from openai import OpenAI, AsyncOpenAI
# This script assumes you have the OPENAI_API_KEY environment variable set to a valid OpenAI API key.
# You can run this script from the root directory like so:
# `python examples/streaming.py`
    response = client.completions.create(
        prompt="1,2,3,",
        max_tokens=5,
        temperature=0,
    # You can manually control iteration over the response
    first = next(response)
    print(f"got response data: {first.to_json()}")
    # Or you could automatically iterate through all of data.
    # Note that the for loop will not exit until *all* of the data has been processed.
    for data in response:
        print(data.to_json())
    response = await client.completions.create(
    # You can manually control iteration over the response.
    # In Python 3.10+ you can also use the `await anext(response)` builtin instead
    first = await response.__anext__()
    async for data in response:
GigaChat Streaming Response Handler
from litellm.types.llms.openai import ChatCompletionToolCallChunk, ChatCompletionToolCallFunctionChunk
from litellm.types.utils import GenericStreamingChunk
class GigaChatModelResponseIterator:
    """Iterator for GigaChat streaming responses."""
        streaming_response: Any,
        sync_stream: bool,
        json_mode: Optional[bool] = False,
        self.streaming_response = streaming_response
        self.response_iterator = self.streaming_response
        self.json_mode = json_mode
    def chunk_parser(self, chunk: dict) -> GenericStreamingChunk:
        """Parse a single streaming chunk from GigaChat."""
        text = ""
        tool_use: Optional[ChatCompletionToolCallChunk] = None
        is_finished = False
        choices = chunk.get("choices", [])
        if not choices:
            return GenericStreamingChunk(
                text="",
                tool_use=None,
                is_finished=False,
                finish_reason="",
                usage=None,
        delta = choice.get("delta", {})
        finish_reason = choice.get("finish_reason")
        # Extract text content
        text = delta.get("content", "") or ""
        # Handle function_call in stream
        if finish_reason == "function_call" and delta.get("function_call"):
            func_call = delta["function_call"]
            args = func_call.get("arguments", {})
            if isinstance(args, dict):
                args = json.dumps(args, ensure_ascii=False)
            tool_use = ChatCompletionToolCallChunk(
                id=f"call_{uuid.uuid4().hex[:24]}",
                function=ChatCompletionToolCallFunctionChunk(
                    name=func_call.get("name", ""),
            finish_reason = "tool_calls"
        if finish_reason is not None:
            is_finished = True
            tool_use=tool_use,
            is_finished=is_finished,
            finish_reason=finish_reason or "",
            index=choice.get("index", 0),
    def __next__(self) -> GenericStreamingChunk:
            chunk = self.response_iterator.__next__()
            if isinstance(chunk, str):
                # Parse SSE format: data: {...}
                if chunk.startswith("data: "):
                    chunk = chunk[6:]
                if chunk.strip() == "[DONE]":
                    chunk = json.loads(chunk)
            return self.chunk_parser(chunk)
    async def __anext__(self) -> GenericStreamingChunk:
            chunk = await self.response_iterator.__anext__()
                # Parse SSE format
