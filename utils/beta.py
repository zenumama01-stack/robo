from .chatkit.chatkit import (
from .threads.threads import (
from ...resources.chat import Chat, AsyncChat
from .realtime.realtime import (
    Realtime,
    AsyncRealtime,
__all__ = ["Beta", "AsyncBeta"]
class Beta(SyncAPIResource):
        return Chat(self._client)
        return Realtime(self._client)
    def chatkit(self) -> ChatKit:
        return ChatKit(self._client)
    def assistants(self) -> Assistants:
        return Assistants(self._client)
    def threads(self) -> Threads:
        return Threads(self._client)
    def with_raw_response(self) -> BetaWithRawResponse:
        return BetaWithRawResponse(self)
    def with_streaming_response(self) -> BetaWithStreamingResponse:
        return BetaWithStreamingResponse(self)
class AsyncBeta(AsyncAPIResource):
        return AsyncChat(self._client)
        return AsyncRealtime(self._client)
    def chatkit(self) -> AsyncChatKit:
        return AsyncChatKit(self._client)
    def assistants(self) -> AsyncAssistants:
        return AsyncAssistants(self._client)
    def threads(self) -> AsyncThreads:
        return AsyncThreads(self._client)
    def with_raw_response(self) -> AsyncBetaWithRawResponse:
        return AsyncBetaWithRawResponse(self)
    def with_streaming_response(self) -> AsyncBetaWithStreamingResponse:
        return AsyncBetaWithStreamingResponse(self)
class BetaWithRawResponse:
    def __init__(self, beta: Beta) -> None:
        self._beta = beta
    def chatkit(self) -> ChatKitWithRawResponse:
        return ChatKitWithRawResponse(self._beta.chatkit)
    def assistants(self) -> AssistantsWithRawResponse:
        return AssistantsWithRawResponse(self._beta.assistants)
    def threads(self) -> ThreadsWithRawResponse:
        return ThreadsWithRawResponse(self._beta.threads)
class AsyncBetaWithRawResponse:
    def __init__(self, beta: AsyncBeta) -> None:
    def chatkit(self) -> AsyncChatKitWithRawResponse:
        return AsyncChatKitWithRawResponse(self._beta.chatkit)
    def assistants(self) -> AsyncAssistantsWithRawResponse:
        return AsyncAssistantsWithRawResponse(self._beta.assistants)
    def threads(self) -> AsyncThreadsWithRawResponse:
        return AsyncThreadsWithRawResponse(self._beta.threads)
class BetaWithStreamingResponse:
    def chatkit(self) -> ChatKitWithStreamingResponse:
        return ChatKitWithStreamingResponse(self._beta.chatkit)
    def assistants(self) -> AssistantsWithStreamingResponse:
        return AssistantsWithStreamingResponse(self._beta.assistants)
    def threads(self) -> ThreadsWithStreamingResponse:
        return ThreadsWithStreamingResponse(self._beta.threads)
class AsyncBetaWithStreamingResponse:
    def chatkit(self) -> AsyncChatKitWithStreamingResponse:
        return AsyncChatKitWithStreamingResponse(self._beta.chatkit)
    def assistants(self) -> AsyncAssistantsWithStreamingResponse:
        return AsyncAssistantsWithStreamingResponse(self._beta.assistants)
    def threads(self) -> AsyncThreadsWithStreamingResponse:
        return AsyncThreadsWithStreamingResponse(self._beta.threads)
