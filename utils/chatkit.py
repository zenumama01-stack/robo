from ...._compat import cached_property
from ...._resource import SyncAPIResource, AsyncAPIResource
__all__ = ["ChatKit", "AsyncChatKit"]
class ChatKit(SyncAPIResource):
    def sessions(self) -> Sessions:
        return Sessions(self._client)
    def with_raw_response(self) -> ChatKitWithRawResponse:
        return ChatKitWithRawResponse(self)
    def with_streaming_response(self) -> ChatKitWithStreamingResponse:
        return ChatKitWithStreamingResponse(self)
class AsyncChatKit(AsyncAPIResource):
    def sessions(self) -> AsyncSessions:
        return AsyncSessions(self._client)
    def with_raw_response(self) -> AsyncChatKitWithRawResponse:
        return AsyncChatKitWithRawResponse(self)
    def with_streaming_response(self) -> AsyncChatKitWithStreamingResponse:
        return AsyncChatKitWithStreamingResponse(self)
class ChatKitWithRawResponse:
    def __init__(self, chatkit: ChatKit) -> None:
        self._chatkit = chatkit
    def sessions(self) -> SessionsWithRawResponse:
        return SessionsWithRawResponse(self._chatkit.sessions)
        return ThreadsWithRawResponse(self._chatkit.threads)
class AsyncChatKitWithRawResponse:
    def __init__(self, chatkit: AsyncChatKit) -> None:
    def sessions(self) -> AsyncSessionsWithRawResponse:
        return AsyncSessionsWithRawResponse(self._chatkit.sessions)
        return AsyncThreadsWithRawResponse(self._chatkit.threads)
class ChatKitWithStreamingResponse:
    def sessions(self) -> SessionsWithStreamingResponse:
        return SessionsWithStreamingResponse(self._chatkit.sessions)
        return ThreadsWithStreamingResponse(self._chatkit.threads)
class AsyncChatKitWithStreamingResponse:
    def sessions(self) -> AsyncSessionsWithStreamingResponse:
        return AsyncSessionsWithStreamingResponse(self._chatkit.sessions)
        return AsyncThreadsWithStreamingResponse(self._chatkit.threads)
