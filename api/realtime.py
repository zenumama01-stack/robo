    """The following example demonstrates how to configure OpenAI to use the Realtime API.
        model="gpt-realtime",
from typing import TYPE_CHECKING, Any, Iterator, cast
from typing_extensions import AsyncIterator
from ...._types import NOT_GIVEN, Query, Headers, NotGiven
from ...._utils import (
    is_azure_client,
    maybe_transform,
    async_maybe_transform,
    is_async_azure_client,
from ...._models import construct_type_unchecked
from ...._exceptions import OpenAIError
from ...._base_client import _merge_mappings
from ....types.beta.realtime import (
    session_update_event_param,
    response_create_event_param,
    transcription_session_update_param,
from ....types.websocket_connection_options import WebsocketConnectionOptions
from ....types.beta.realtime.realtime_client_event import RealtimeClientEvent
from ....types.beta.realtime.realtime_server_event import RealtimeServerEvent
from ....types.beta.realtime.conversation_item_param import ConversationItemParam
from ....types.beta.realtime.realtime_client_event_param import RealtimeClientEventParam
    from websockets.sync.client import ClientConnection as WebsocketConnection
    from websockets.asyncio.client import ClientConnection as AsyncWebsocketConnection
    from ...._client import OpenAI, AsyncOpenAI
__all__ = ["Realtime", "AsyncRealtime"]
class Realtime(SyncAPIResource):
    def transcription_sessions(self) -> TranscriptionSessions:
        return TranscriptionSessions(self._client)
    def with_raw_response(self) -> RealtimeWithRawResponse:
        return RealtimeWithRawResponse(self)
    def with_streaming_response(self) -> RealtimeWithStreamingResponse:
        return RealtimeWithStreamingResponse(self)
    def connect(
        extra_query: Query = {},
        extra_headers: Headers = {},
        websocket_connection_options: WebsocketConnectionOptions = {},
    ) -> RealtimeConnectionManager:
        The Realtime API enables you to build low-latency, multi-modal conversational experiences. It currently supports text and audio as both input and output, as well as function calling.
        Some notable benefits of the API include:
        - Native speech-to-speech: Skipping an intermediate text format means low latency and nuanced output.
        - Natural, steerable voices: The models have natural inflection and can laugh, whisper, and adhere to tone direction.
        - Simultaneous multimodal output: Text is useful for moderation; faster-than-realtime audio ensures stable playback.
        The Realtime API is a stateful, event-based API that communicates over a WebSocket.
        return RealtimeConnectionManager(
            websocket_connection_options=websocket_connection_options,
class AsyncRealtime(AsyncAPIResource):
    def transcription_sessions(self) -> AsyncTranscriptionSessions:
        return AsyncTranscriptionSessions(self._client)
    def with_raw_response(self) -> AsyncRealtimeWithRawResponse:
        return AsyncRealtimeWithRawResponse(self)
    def with_streaming_response(self) -> AsyncRealtimeWithStreamingResponse:
        return AsyncRealtimeWithStreamingResponse(self)
    ) -> AsyncRealtimeConnectionManager:
        return AsyncRealtimeConnectionManager(
class RealtimeWithRawResponse:
    def __init__(self, realtime: Realtime) -> None:
        self._realtime = realtime
        return SessionsWithRawResponse(self._realtime.sessions)
    def transcription_sessions(self) -> TranscriptionSessionsWithRawResponse:
        return TranscriptionSessionsWithRawResponse(self._realtime.transcription_sessions)
class AsyncRealtimeWithRawResponse:
    def __init__(self, realtime: AsyncRealtime) -> None:
        return AsyncSessionsWithRawResponse(self._realtime.sessions)
    def transcription_sessions(self) -> AsyncTranscriptionSessionsWithRawResponse:
        return AsyncTranscriptionSessionsWithRawResponse(self._realtime.transcription_sessions)
class RealtimeWithStreamingResponse:
        return SessionsWithStreamingResponse(self._realtime.sessions)
    def transcription_sessions(self) -> TranscriptionSessionsWithStreamingResponse:
        return TranscriptionSessionsWithStreamingResponse(self._realtime.transcription_sessions)
class AsyncRealtimeWithStreamingResponse:
        return AsyncSessionsWithStreamingResponse(self._realtime.sessions)
    def transcription_sessions(self) -> AsyncTranscriptionSessionsWithStreamingResponse:
        return AsyncTranscriptionSessionsWithStreamingResponse(self._realtime.transcription_sessions)
class AsyncRealtimeConnection:
    """Represents a live websocket connection to the Realtime API"""
    session: AsyncRealtimeSessionResource
    response: AsyncRealtimeResponseResource
    input_audio_buffer: AsyncRealtimeInputAudioBufferResource
    conversation: AsyncRealtimeConversationResource
    output_audio_buffer: AsyncRealtimeOutputAudioBufferResource
    transcription_session: AsyncRealtimeTranscriptionSessionResource
    _connection: AsyncWebsocketConnection
    def __init__(self, connection: AsyncWebsocketConnection) -> None:
        self._connection = connection
        self.session = AsyncRealtimeSessionResource(self)
        self.response = AsyncRealtimeResponseResource(self)
        self.input_audio_buffer = AsyncRealtimeInputAudioBufferResource(self)
        self.conversation = AsyncRealtimeConversationResource(self)
        self.output_audio_buffer = AsyncRealtimeOutputAudioBufferResource(self)
        self.transcription_session = AsyncRealtimeTranscriptionSessionResource(self)
    async def __aiter__(self) -> AsyncIterator[RealtimeServerEvent]:
        An infinite-iterator that will continue to yield events until
        the connection is closed.
        from websockets.exceptions import ConnectionClosedOK
                yield await self.recv()
        except ConnectionClosedOK:
    async def recv(self) -> RealtimeServerEvent:
        Receive the next message from the connection and parses it into a `RealtimeServerEvent` object.
        Canceling this method is safe. There's no risk of losing data.
        return self.parse_event(await self.recv_bytes())
    async def recv_bytes(self) -> bytes:
        """Receive the next message from the connection as raw bytes.
        If you want to parse the message into a `RealtimeServerEvent` object like `.recv()` does,
        then you can call `.parse_event(data)`.
        message = await self._connection.recv(decode=False)
        log.debug(f"Received websocket message: %s", message)
        return message
    async def send(self, event: RealtimeClientEvent | RealtimeClientEventParam) -> None:
        data = (
            event.to_json(use_api_names=True, exclude_defaults=True, exclude_unset=True)
            if isinstance(event, BaseModel)
            else json.dumps(await async_maybe_transform(event, RealtimeClientEventParam))
        await self._connection.send(data)
    async def close(self, *, code: int = 1000, reason: str = "") -> None:
        await self._connection.close(code=code, reason=reason)
    def parse_event(self, data: str | bytes) -> RealtimeServerEvent:
        Converts a raw `str` or `bytes` message into a `RealtimeServerEvent` object.
        This is helpful if you're using `.recv_bytes()`.
            RealtimeServerEvent, construct_type_unchecked(value=json.loads(data), type_=cast(Any, RealtimeServerEvent))
class AsyncRealtimeConnectionManager:
    Context manager over a `AsyncRealtimeConnection` that is returned by `beta.realtime.connect()`
    This context manager ensures that the connection will be closed when it exits.
    Note that if your application doesn't work well with the context manager approach then you
    can call the `.enter()` method directly to initiate a connection.
    **Warning**: You must remember to close the connection with `.close()`.
    connection = await client.beta.realtime.connect(...).enter()
    await connection.close()
        extra_query: Query,
        extra_headers: Headers,
        websocket_connection_options: WebsocketConnectionOptions,
        self.__client = client
        self.__model = model
        self.__connection: AsyncRealtimeConnection | None = None
        self.__extra_query = extra_query
        self.__extra_headers = extra_headers
        self.__websocket_connection_options = websocket_connection_options
    async def __aenter__(self) -> AsyncRealtimeConnection:
        👋 If your application doesn't work well with the context manager approach then you
        can call this method directly to initiate a connection.
            from websockets.asyncio.client import connect
        except ImportError as exc:
            raise OpenAIError("You need to install `openai[realtime]` to use this method") from exc
        extra_query = self.__extra_query
        await self.__client._refresh_api_key()
        auth_headers = self.__client.auth_headers
        if is_async_azure_client(self.__client):
            url, auth_headers = await self.__client._configure_realtime(self.__model, extra_query)
            url = self._prepare_url().copy_with(
                params={
                    **self.__client.base_url.params,
                    "model": self.__model,
        log.debug("Connecting to %s", url)
        if self.__websocket_connection_options:
            log.debug("Connection options: %s", self.__websocket_connection_options)
        self.__connection = AsyncRealtimeConnection(
            await connect(
                str(url),
                user_agent_header=self.__client.user_agent,
                additional_headers=_merge_mappings(
                        **auth_headers,
                        "OpenAI-Beta": "realtime=v1",
                    self.__extra_headers,
                **self.__websocket_connection_options,
        return self.__connection
    enter = __aenter__
    def _prepare_url(self) -> httpx.URL:
        if self.__client.websocket_base_url is not None:
            base_url = httpx.URL(self.__client.websocket_base_url)
            base_url = self.__client._base_url.copy_with(scheme="wss")
        return base_url.copy_with(raw_path=merge_raw_path)
        self, exc_type: type[BaseException] | None, exc: BaseException | None, exc_tb: TracebackType | None
        if self.__connection is not None:
            await self.__connection.close()
class RealtimeConnection:
    session: RealtimeSessionResource
    response: RealtimeResponseResource
    input_audio_buffer: RealtimeInputAudioBufferResource
    conversation: RealtimeConversationResource
    output_audio_buffer: RealtimeOutputAudioBufferResource
    transcription_session: RealtimeTranscriptionSessionResource
    _connection: WebsocketConnection
    def __init__(self, connection: WebsocketConnection) -> None:
        self.session = RealtimeSessionResource(self)
        self.response = RealtimeResponseResource(self)
        self.input_audio_buffer = RealtimeInputAudioBufferResource(self)
        self.conversation = RealtimeConversationResource(self)
        self.output_audio_buffer = RealtimeOutputAudioBufferResource(self)
        self.transcription_session = RealtimeTranscriptionSessionResource(self)
    def __iter__(self) -> Iterator[RealtimeServerEvent]:
                yield self.recv()
    def recv(self) -> RealtimeServerEvent:
        return self.parse_event(self.recv_bytes())
    def recv_bytes(self) -> bytes:
        message = self._connection.recv(decode=False)
    def send(self, event: RealtimeClientEvent | RealtimeClientEventParam) -> None:
            else json.dumps(maybe_transform(event, RealtimeClientEventParam))
        self._connection.send(data)
    def close(self, *, code: int = 1000, reason: str = "") -> None:
        self._connection.close(code=code, reason=reason)
class RealtimeConnectionManager:
    Context manager over a `RealtimeConnection` that is returned by `beta.realtime.connect()`
    connection = client.beta.realtime.connect(...).enter()
        self.__connection: RealtimeConnection | None = None
    def __enter__(self) -> RealtimeConnection:
            from websockets.sync.client import connect
        self.__client._refresh_api_key()
        if is_azure_client(self.__client):
            url, auth_headers = self.__client._configure_realtime(self.__model, extra_query)
        self.__connection = RealtimeConnection(
            connect(
    enter = __enter__
            self.__connection.close()
class BaseRealtimeConnectionResource:
    def __init__(self, connection: RealtimeConnection) -> None:
class RealtimeSessionResource(BaseRealtimeConnectionResource):
    def update(self, *, session: session_update_event_param.Session, event_id: str | NotGiven = NOT_GIVEN) -> None:
        Send this event to update the session’s default configuration.
        The client may send this event at any time to update any field,
        except for `voice`. However, note that once a session has been
        initialized with a particular `model`, it can’t be changed to
        another model using `session.update`.
        When the server receives a `session.update`, it will respond
        with a `session.updated` event showing the full, effective configuration.
        Only the fields that are present are updated. To clear a field like
        `instructions`, pass an empty string.
        self._connection.send(
                RealtimeClientEventParam,
                strip_not_given({"type": "session.update", "session": session, "event_id": event_id}),
class RealtimeResponseResource(BaseRealtimeConnectionResource):
        event_id: str | NotGiven = NOT_GIVEN,
        response: response_create_event_param.Response | NotGiven = NOT_GIVEN,
        This event instructs the server to create a Response, which means triggering
        model inference. When in Server VAD mode, the server will create Responses
        automatically.
        A Response will include at least one Item, and may have two, in which case
        the second will be a function call. These Items will be appended to the
        conversation history.
        The server will respond with a `response.created` event, events for Items
        and content created, and finally a `response.done` event to indicate the
        Response is complete.
        The `response.create` event includes inference configuration like
        `instructions`, and `temperature`. These fields will override the Session's
        configuration for this Response only.
                strip_not_given({"type": "response.create", "event_id": event_id, "response": response}),
    def cancel(self, *, event_id: str | NotGiven = NOT_GIVEN, response_id: str | NotGiven = NOT_GIVEN) -> None:
        """Send this event to cancel an in-progress response.
        The server will respond
        with a `response.done` event with a status of `response.status=cancelled`. If
        there is no response to cancel, the server will respond with an error.
                strip_not_given({"type": "response.cancel", "event_id": event_id, "response_id": response_id}),
class RealtimeInputAudioBufferResource(BaseRealtimeConnectionResource):
    def clear(self, *, event_id: str | NotGiven = NOT_GIVEN) -> None:
        """Send this event to clear the audio bytes in the buffer.
        The server will
        respond with an `input_audio_buffer.cleared` event.
            cast(RealtimeClientEventParam, strip_not_given({"type": "input_audio_buffer.clear", "event_id": event_id}))
    def commit(self, *, event_id: str | NotGiven = NOT_GIVEN) -> None:
        Send this event to commit the user input audio buffer, which will create a
        new user message item in the conversation. This event will produce an error
        if the input audio buffer is empty. When in Server VAD mode, the client does
        not need to send this event, the server will commit the audio buffer
        Committing the input audio buffer will trigger input audio transcription
        (if enabled in session configuration), but it will not create a response
        from the model. The server will respond with an `input_audio_buffer.committed`
        event.
            cast(RealtimeClientEventParam, strip_not_given({"type": "input_audio_buffer.commit", "event_id": event_id}))
    def append(self, *, audio: str, event_id: str | NotGiven = NOT_GIVEN) -> None:
        """Send this event to append audio bytes to the input audio buffer.
        The audio
        buffer is temporary storage you can write to and later commit. In Server VAD
        mode, the audio buffer is used to detect speech and the server will decide
        when to commit. When Server VAD is disabled, you must commit the audio buffer
        manually.
        The client may choose how much audio to place in each event up to a maximum
        of 15 MiB, for example streaming smaller chunks from the client may allow the
        VAD to be more responsive. Unlike made other client events, the server will
        not send a confirmation response to this event.
                strip_not_given({"type": "input_audio_buffer.append", "audio": audio, "event_id": event_id}),
class RealtimeConversationResource(BaseRealtimeConnectionResource):
    def item(self) -> RealtimeConversationItemResource:
        return RealtimeConversationItemResource(self._connection)
class RealtimeConversationItemResource(BaseRealtimeConnectionResource):
    def delete(self, *, item_id: str, event_id: str | NotGiven = NOT_GIVEN) -> None:
        """Send this event when you want to remove any item from the conversation
        history.
        The server will respond with a `conversation.item.deleted` event,
        unless the item does not exist in the conversation history, in which case the
        server will respond with an error.
                strip_not_given({"type": "conversation.item.delete", "item_id": item_id, "event_id": event_id}),
        item: ConversationItemParam,
        previous_item_id: str | NotGiven = NOT_GIVEN,
        Add a new Item to the Conversation's context, including messages, function
        calls, and function call responses. This event can be used both to populate a
        "history" of the conversation and to add new items mid-stream, but has the
        current limitation that it cannot populate assistant audio messages.
        If successful, the server will respond with a `conversation.item.created`
        event, otherwise an `error` event will be sent.
                strip_not_given(
                        "type": "conversation.item.create",
                        "item": item,
                        "event_id": event_id,
                        "previous_item_id": previous_item_id,
    def truncate(
        self, *, audio_end_ms: int, content_index: int, item_id: str, event_id: str | NotGiven = NOT_GIVEN
        """Send this event to truncate a previous assistant message’s audio.
        The server
        will produce audio faster than realtime, so this event is useful when the user
        interrupts to truncate audio that has already been sent to the client but not
        yet played. This will synchronize the server's understanding of the audio with
        the client's playback.
        Truncating audio will delete the server-side text transcript to ensure there
        is not text in the context that hasn't been heard by the user.
        If successful, the server will respond with a `conversation.item.truncated`
                        "type": "conversation.item.truncate",
                        "audio_end_ms": audio_end_ms,
                        "content_index": content_index,
                        "item_id": item_id,
    def retrieve(self, *, item_id: str, event_id: str | NotGiven = NOT_GIVEN) -> None:
        Send this event when you want to retrieve the server's representation of a specific item in the conversation history. This is useful, for example, to inspect user audio after noise cancellation and VAD.
        The server will respond with a `conversation.item.retrieved` event,
                strip_not_given({"type": "conversation.item.retrieve", "item_id": item_id, "event_id": event_id}),
class RealtimeOutputAudioBufferResource(BaseRealtimeConnectionResource):
        """**WebRTC Only:** Emit to cut off the current audio response.
        This will trigger the server to
        stop generating audio and emit a `output_audio_buffer.cleared` event. This
        event should be preceded by a `response.cancel` client event to stop the
        generation of the current response.
        [Learn more](https://platform.openai.com/docs/guides/realtime-conversations#client-and-server-events-for-audio-in-webrtc).
            cast(RealtimeClientEventParam, strip_not_given({"type": "output_audio_buffer.clear", "event_id": event_id}))
class RealtimeTranscriptionSessionResource(BaseRealtimeConnectionResource):
        self, *, session: transcription_session_update_param.Session, event_id: str | NotGiven = NOT_GIVEN
        """Send this event to update a transcription session."""
                strip_not_given({"type": "transcription_session.update", "session": session, "event_id": event_id}),
class BaseAsyncRealtimeConnectionResource:
    def __init__(self, connection: AsyncRealtimeConnection) -> None:
class AsyncRealtimeSessionResource(BaseAsyncRealtimeConnectionResource):
        self, *, session: session_update_event_param.Session, event_id: str | NotGiven = NOT_GIVEN
        await self._connection.send(
class AsyncRealtimeResponseResource(BaseAsyncRealtimeConnectionResource):
    async def cancel(self, *, event_id: str | NotGiven = NOT_GIVEN, response_id: str | NotGiven = NOT_GIVEN) -> None:
class AsyncRealtimeInputAudioBufferResource(BaseAsyncRealtimeConnectionResource):
    async def clear(self, *, event_id: str | NotGiven = NOT_GIVEN) -> None:
    async def commit(self, *, event_id: str | NotGiven = NOT_GIVEN) -> None:
    async def append(self, *, audio: str, event_id: str | NotGiven = NOT_GIVEN) -> None:
class AsyncRealtimeConversationResource(BaseAsyncRealtimeConnectionResource):
    def item(self) -> AsyncRealtimeConversationItemResource:
        return AsyncRealtimeConversationItemResource(self._connection)
class AsyncRealtimeConversationItemResource(BaseAsyncRealtimeConnectionResource):
    async def delete(self, *, item_id: str, event_id: str | NotGiven = NOT_GIVEN) -> None:
    async def truncate(
    async def retrieve(self, *, item_id: str, event_id: str | NotGiven = NOT_GIVEN) -> None:
class AsyncRealtimeOutputAudioBufferResource(BaseAsyncRealtimeConnectionResource):
class AsyncRealtimeTranscriptionSessionResource(BaseAsyncRealtimeConnectionResource):
from ..._types import Omit, Query, Headers, omit
from ..._utils import (
from ..._exceptions import OpenAIError
from ..._base_client import _merge_mappings
from ...types.realtime import session_update_event_param
from ...types.websocket_connection_options import WebSocketConnectionOptions
from ...types.realtime.realtime_client_event import RealtimeClientEvent
from ...types.realtime.realtime_server_event import RealtimeServerEvent
from ...types.realtime.conversation_item_param import ConversationItemParam
from ...types.realtime.realtime_client_event_param import RealtimeClientEventParam
from ...types.realtime.realtime_response_create_params_param import RealtimeResponseCreateParamsParam
    from websockets.sync.client import ClientConnection as WebSocketConnection
    from websockets.asyncio.client import ClientConnection as AsyncWebSocketConnection
    from ..._client import OpenAI, AsyncOpenAI
    def client_secrets(self) -> ClientSecrets:
        return ClientSecrets(self._client)
    def calls(self) -> Calls:
        from ...lib._realtime import _Calls
        return _Calls(self._client)
        call_id: str | Omit = omit,
        websocket_connection_options: WebSocketConnectionOptions = {},
            call_id=call_id,
    def client_secrets(self) -> AsyncClientSecrets:
        return AsyncClientSecrets(self._client)
    def calls(self) -> AsyncCalls:
        from ...lib._realtime import _AsyncCalls
        return _AsyncCalls(self._client)
    def client_secrets(self) -> ClientSecretsWithRawResponse:
        return ClientSecretsWithRawResponse(self._realtime.client_secrets)
    def calls(self) -> CallsWithRawResponse:
        return CallsWithRawResponse(self._realtime.calls)
    def client_secrets(self) -> AsyncClientSecretsWithRawResponse:
        return AsyncClientSecretsWithRawResponse(self._realtime.client_secrets)
    def calls(self) -> AsyncCallsWithRawResponse:
        return AsyncCallsWithRawResponse(self._realtime.calls)
    def client_secrets(self) -> ClientSecretsWithStreamingResponse:
        return ClientSecretsWithStreamingResponse(self._realtime.client_secrets)
    def calls(self) -> CallsWithStreamingResponse:
        return CallsWithStreamingResponse(self._realtime.calls)
    def client_secrets(self) -> AsyncClientSecretsWithStreamingResponse:
        return AsyncClientSecretsWithStreamingResponse(self._realtime.client_secrets)
    def calls(self) -> AsyncCallsWithStreamingResponse:
        return AsyncCallsWithStreamingResponse(self._realtime.calls)
    """Represents a live WebSocket connection to the Realtime API"""
    _connection: AsyncWebSocketConnection
    def __init__(self, connection: AsyncWebSocketConnection) -> None:
        log.debug(f"Received WebSocket message: %s", message)
    Context manager over a `AsyncRealtimeConnection` that is returned by `realtime.connect()`
    connection = await client.realtime.connect(...).enter()
        websocket_connection_options: WebSocketConnectionOptions,
        self.__call_id = call_id
        if self.__call_id is not omit:
            extra_query = {**extra_query, "call_id": self.__call_id}
            model = self.__model
                raise OpenAIError("`model` is required for Azure Realtime API")
                url, auth_headers = await self.__client._configure_realtime(model, extra_query)
                    **({"model": self.__model} if self.__model is not omit else {}),
            scheme = self.__client._base_url.scheme
            ws_scheme = "ws" if scheme == "http" else "wss"
            base_url = self.__client._base_url.copy_with(scheme=ws_scheme)
    _connection: WebSocketConnection
    def __init__(self, connection: WebSocketConnection) -> None:
    Context manager over a `RealtimeConnection` that is returned by `realtime.connect()`
    connection = client.realtime.connect(...).enter()
                url, auth_headers = self.__client._configure_realtime(model, extra_query)
    def update(self, *, session: session_update_event_param.Session, event_id: str | Omit = omit) -> None:
        Send this event to update the session’s configuration.
        The client may send this event at any time to update any field
        except for `voice` and `model`. `voice` can be updated only if there have been no other audio outputs yet.
        Only the fields that are present in the `session.update` are updated. To clear a field like
        `instructions`, pass an empty string. To clear a field like `tools`, pass an empty array.
        To clear a field like `turn_detection`, pass `null`.
    def create(self, *, event_id: str | Omit = omit, response: RealtimeResponseCreateParamsParam | Omit = omit) -> None:
        conversation history by default.
        `instructions` and `tools`. If these are set, they will override the Session's
        Responses can be created out-of-band of the default Conversation, meaning that they can
        have arbitrary input, and it's possible to disable writing the output to the Conversation.
        Only one Response can write to the default Conversation at a time, but otherwise multiple
        Responses can be created in parallel. The `metadata` field is a good way to disambiguate
        multiple simultaneous Responses.
        Clients can set `conversation` to `none` to create a Response that does not write to the default
        Conversation. Arbitrary input can be provided with the `input` field, which is an array accepting
        raw Items and references to existing Items.
    def cancel(self, *, event_id: str | Omit = omit, response_id: str | Omit = omit) -> None:
        there is no response to cancel, the server will respond with an error. It's safe
        to call `response.cancel` even if no response is in progress, an error will be
        returned the session will remain unaffected.
    def clear(self, *, event_id: str | Omit = omit) -> None:
    def commit(self, *, event_id: str | Omit = omit) -> None:
        Send this event to commit the user input audio buffer, which will create a  new user message item in the conversation. This event will produce an error  if the input audio buffer is empty. When in Server VAD mode, the client does  not need to send this event, the server will commit the audio buffer  automatically.
        Committing the input audio buffer will trigger input audio transcription  (if enabled in session configuration), but it will not create a response  from the model. The server will respond with an `input_audio_buffer.committed` event.
    def append(self, *, audio: str, event_id: str | Omit = omit) -> None:
        buffer is temporary storage you can write to and later commit. A "commit" will create a new
        user message item in the conversation history from the buffer content and clear the buffer.
        Input audio transcription (if enabled) will be generated when the buffer is committed.
        If VAD is enabled the audio buffer is used to detect speech and the server will decide
        manually. Input audio noise reduction operates on writes to the audio buffer.
        VAD to be more responsive. Unlike most other client events, the server will
    def delete(self, *, item_id: str, event_id: str | Omit = omit) -> None:
        self, *, item: ConversationItemParam, event_id: str | Omit = omit, previous_item_id: str | Omit = omit
    def truncate(self, *, audio_end_ms: int, content_index: int, item_id: str, event_id: str | Omit = omit) -> None:
    def retrieve(self, *, item_id: str, event_id: str | Omit = omit) -> None:
        """**WebRTC/SIP Only:** Emit to cut off the current audio response.
    async def update(self, *, session: session_update_event_param.Session, event_id: str | Omit = omit) -> None:
        self, *, event_id: str | Omit = omit, response: RealtimeResponseCreateParamsParam | Omit = omit
    async def cancel(self, *, event_id: str | Omit = omit, response_id: str | Omit = omit) -> None:
    async def clear(self, *, event_id: str | Omit = omit) -> None:
    async def commit(self, *, event_id: str | Omit = omit) -> None:
    async def append(self, *, audio: str, event_id: str | Omit = omit) -> None:
    async def delete(self, *, item_id: str, event_id: str | Omit = omit) -> None:
        self, *, audio_end_ms: int, content_index: int, item_id: str, event_id: str | Omit = omit
    async def retrieve(self, *, item_id: str, event_id: str | Omit = omit) -> None:
from ...types.websocket_connection_options import WebsocketConnectionOptions
from typing import List, Literal, Optional, Union
from .llms.openai import (
    OpenAIRealtimeEvents,
    OpenAIRealtimeOutputItemDone,
    OpenAIRealtimeResponseDelta,
ALL_DELTA_TYPES = Literal["text", "audio"]
class RealtimeResponseTransformInput(TypedDict):
    session_configuration_request: Optional[str]
    current_output_item_id: Optional[
    ]  # used to check if this is a new content.delta or a continuation of a previous content.delta
    current_response_id: Optional[
    current_delta_chunks: Optional[List[OpenAIRealtimeResponseDelta]]
    current_item_chunks: Optional[List[OpenAIRealtimeOutputItemDone]]
    current_conversation_id: Optional[str]
    current_delta_type: Optional[ALL_DELTA_TYPES]
class RealtimeResponseTypedDict(TypedDict):
    response: Union[OpenAIRealtimeEvents, List[OpenAIRealtimeEvents]]
    current_output_item_id: Optional[str]
    current_response_id: Optional[str]
class RealtimeModalityResponseTransformOutput(TypedDict):
    returned_message: List[OpenAIRealtimeEvents]
class RealtimeQueryParams(TypedDict, total=False):
    intent: Optional[str]
    # Add more fields as needed
