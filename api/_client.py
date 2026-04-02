from typing import TYPE_CHECKING, Any, Mapping, Callable, Awaitable
from typing_extensions import Self, override
    Transport,
    ProxiesTypes,
from ._utils import (
    is_given,
    is_mapping,
    get_async_library,
from ._compat import cached_property
from ._models import FinalRequestOptions
from ._streaming import Stream as Stream, AsyncStream as AsyncStream
from ._exceptions import OpenAIError, APIStatusError
from ._base_client import (
    SyncAPIClient,
    AsyncAPIClient,
    from .resources import (
        beta,
        chat,
        audio,
        evals,
        files,
        skills,
        videos,
        batches,
        uploads,
        realtime,
        containers,
        completions,
        fine_tuning,
        moderations,
        vector_stores,
    from .resources.files import Files, AsyncFiles
    from .resources.images import Images, AsyncImages
    from .resources.models import Models, AsyncModels
    from .resources.videos import Videos, AsyncVideos
    from .resources.batches import Batches, AsyncBatches
    from .resources.beta.beta import Beta, AsyncBeta
    from .resources.chat.chat import Chat, AsyncChat
    from .resources.embeddings import Embeddings, AsyncEmbeddings
    from .resources.audio.audio import Audio, AsyncAudio
    from .resources.completions import Completions, AsyncCompletions
    from .resources.evals.evals import Evals, AsyncEvals
    from .resources.moderations import Moderations, AsyncModerations
    from .resources.skills.skills import Skills, AsyncSkills
    from .resources.uploads.uploads import Uploads, AsyncUploads
    from .resources.realtime.realtime import Realtime, AsyncRealtime
    from .resources.webhooks.webhooks import Webhooks, AsyncWebhooks
    from .resources.responses.responses import Responses, AsyncResponses
    from .resources.containers.containers import Containers, AsyncContainers
    from .resources.fine_tuning.fine_tuning import FineTuning, AsyncFineTuning
    from .resources.conversations.conversations import Conversations, AsyncConversations
    from .resources.vector_stores.vector_stores import VectorStores, AsyncVectorStores
__all__ = ["Timeout", "Transport", "ProxiesTypes", "RequestOptions", "OpenAI", "AsyncOpenAI", "Client", "AsyncClient"]
class OpenAI(SyncAPIClient):
    # client options
    api_key: str
    organization: str | None
    project: str | None
    webhook_secret: str | None
    websocket_base_url: str | httpx.URL | None
    """Base URL for WebSocket connections.
    If not specified, the default base URL will be used, with 'wss://' replacing the
    'http://' or 'https://' scheme. For example: 'http://example.com' becomes
    'wss://example.com'
        api_key: str | None | Callable[[], str] = None,
        organization: str | None = None,
        project: str | None = None,
        webhook_secret: str | None = None,
        base_url: str | httpx.URL | None = None,
        websocket_base_url: str | httpx.URL | None = None,
        default_headers: Mapping[str, str] | None = None,
        default_query: Mapping[str, object] | None = None,
        # Configure a custom httpx client.
        # We provide a `DefaultHttpxClient` class that you can pass to retain the default values we use for `limits`, `timeout` & `follow_redirects`.
        # See the [httpx documentation](https://www.python-httpx.org/api/#client) for more details.
        # Enable or disable schema validation for data returned by the API.
        # When enabled an error APIResponseValidationError is raised
        # if the API responds with invalid data for the expected schema.
        # This parameter may be removed or changed in the future.
        # If you rely on this feature, please open a GitHub issue
        # outlining your use-case to help us decide if it should be
        # part of our public interface in the future.
        _strict_response_validation: bool = False,
        """Construct a new synchronous OpenAI client instance.
        This automatically infers the following arguments from their corresponding environment variables if they are not provided:
        - `api_key` from `OPENAI_API_KEY`
        - `organization` from `OPENAI_ORG_ID`
        - `project` from `OPENAI_PROJECT_ID`
        - `webhook_secret` from `OPENAI_WEBHOOK_SECRET`
        if api_key is None:
            api_key = os.environ.get("OPENAI_API_KEY")
            raise OpenAIError(
                "The api_key client option must be set either by passing api_key to the client or by setting the OPENAI_API_KEY environment variable"
        if callable(api_key):
            self.api_key = ""
            self._api_key_provider: Callable[[], str] | None = api_key
            self.api_key = api_key
            self._api_key_provider = None
        if organization is None:
            organization = os.environ.get("OPENAI_ORG_ID")
        self.organization = organization
        if project is None:
            project = os.environ.get("OPENAI_PROJECT_ID")
        self.project = project
        if webhook_secret is None:
            webhook_secret = os.environ.get("OPENAI_WEBHOOK_SECRET")
        self.webhook_secret = webhook_secret
        self.websocket_base_url = websocket_base_url
        if base_url is None:
            base_url = os.environ.get("OPENAI_BASE_URL")
            base_url = f"https://api.openai.com/v1"
            version=__version__,
            custom_headers=default_headers,
            custom_query=default_query,
        self._default_stream_cls = Stream
    @cached_property
    def completions(self) -> Completions:
        Given a prompt, the model will return one or more predicted completions, and can also return the probabilities of alternative tokens at each position.
        from .resources.completions import Completions
        return Completions(self)
    def chat(self) -> Chat:
        from .resources.chat import Chat
        return Chat(self)
    def embeddings(self) -> Embeddings:
        Get a vector representation of a given input that can be easily consumed by machine learning models and algorithms.
        from .resources.embeddings import Embeddings
        return Embeddings(self)
    def files(self) -> Files:
        Files are used to upload documents that can be used with features like Assistants and Fine-tuning.
        from .resources.files import Files
        return Files(self)
    def images(self) -> Images:
        """Given a prompt and/or an input image, the model will generate a new image."""
        from .resources.images import Images
        return Images(self)
    def audio(self) -> Audio:
        from .resources.audio import Audio
        return Audio(self)
    def moderations(self) -> Moderations:
        Given text and/or image inputs, classifies if those inputs are potentially harmful.
        from .resources.moderations import Moderations
        return Moderations(self)
    def models(self) -> Models:
        """List and describe the various models available in the API."""
        from .resources.models import Models
        return Models(self)
    def fine_tuning(self) -> FineTuning:
        from .resources.fine_tuning import FineTuning
        return FineTuning(self)
    def vector_stores(self) -> VectorStores:
        from .resources.vector_stores import VectorStores
        return VectorStores(self)
    def webhooks(self) -> Webhooks:
        from .resources.webhooks import Webhooks
        return Webhooks(self)
    def beta(self) -> Beta:
        from .resources.beta import Beta
        return Beta(self)
    def batches(self) -> Batches:
        """Create large batches of API requests to run asynchronously."""
        from .resources.batches import Batches
        return Batches(self)
    def uploads(self) -> Uploads:
        """Use Uploads to upload large files in multiple parts."""
        from .resources.uploads import Uploads
        return Uploads(self)
    def responses(self) -> Responses:
        from .resources.responses import Responses
        return Responses(self)
    def realtime(self) -> Realtime:
        from .resources.realtime import Realtime
        return Realtime(self)
    def conversations(self) -> Conversations:
        """Manage conversations and conversation items."""
        from .resources.conversations import Conversations
        return Conversations(self)
    def evals(self) -> Evals:
        """Manage and run evals in the OpenAI platform."""
        from .resources.evals import Evals
        return Evals(self)
    def containers(self) -> Containers:
        from .resources.containers import Containers
        return Containers(self)
    def skills(self) -> Skills:
        from .resources.skills import Skills
        return Skills(self)
    def videos(self) -> Videos:
        from .resources.videos import Videos
        return Videos(self)
    def with_raw_response(self) -> OpenAIWithRawResponse:
        return OpenAIWithRawResponse(self)
    def with_streaming_response(self) -> OpenAIWithStreamedResponse:
        return OpenAIWithStreamedResponse(self)
        return Querystring(array_format="brackets")
    def _refresh_api_key(self) -> None:
        if self._api_key_provider:
            self.api_key = self._api_key_provider()
    def _prepare_options(self, options: FinalRequestOptions) -> FinalRequestOptions:
        self._refresh_api_key()
        return super()._prepare_options(options)
        api_key = self.api_key
        if not api_key:
            # if the api key is an empty string, encoding the header will fail
        return {"Authorization": f"Bearer {api_key}"}
            **super().default_headers,
            "X-Stainless-Async": "false",
            "OpenAI-Organization": self.organization if self.organization is not None else Omit(),
            "OpenAI-Project": self.project if self.project is not None else Omit(),
    def copy(
        api_key: str | Callable[[], str] | None = None,
        max_retries: int | NotGiven = not_given,
        set_default_headers: Mapping[str, str] | None = None,
        set_default_query: Mapping[str, object] | None = None,
        _extra_kwargs: Mapping[str, Any] = {},
    ) -> Self:
        Create a new client instance re-using the same options given to the current client with optional overriding.
        if default_headers is not None and set_default_headers is not None:
            raise ValueError("The `default_headers` and `set_default_headers` arguments are mutually exclusive")
        if default_query is not None and set_default_query is not None:
            raise ValueError("The `default_query` and `set_default_query` arguments are mutually exclusive")
        headers = self._custom_headers
        if default_headers is not None:
            headers = {**headers, **default_headers}
        elif set_default_headers is not None:
            headers = set_default_headers
        params = self._custom_query
        if default_query is not None:
            params = {**params, **default_query}
        elif set_default_query is not None:
            params = set_default_query
        http_client = http_client or self._client
        return self.__class__(
            api_key=api_key or self._api_key_provider or self.api_key,
            organization=organization or self.organization,
            project=project or self.project,
            webhook_secret=webhook_secret or self.webhook_secret,
            websocket_base_url=websocket_base_url or self.websocket_base_url,
            base_url=base_url or self.base_url,
            timeout=self.timeout if isinstance(timeout, NotGiven) else timeout,
            max_retries=max_retries if is_given(max_retries) else self.max_retries,
            default_headers=headers,
            default_query=params,
            **_extra_kwargs,
    # Alias for `copy` for nicer inline usage, e.g.
    # client.with_options(timeout=10).foo.create(...)
    with_options = copy
        data = body.get("error", body) if is_mapping(body) else body
            return _exceptions.BadRequestError(err_msg, response=response, body=data)
            return _exceptions.AuthenticationError(err_msg, response=response, body=data)
            return _exceptions.PermissionDeniedError(err_msg, response=response, body=data)
            return _exceptions.NotFoundError(err_msg, response=response, body=data)
            return _exceptions.ConflictError(err_msg, response=response, body=data)
        if response.status_code == 422:
            return _exceptions.UnprocessableEntityError(err_msg, response=response, body=data)
            return _exceptions.RateLimitError(err_msg, response=response, body=data)
            return _exceptions.InternalServerError(err_msg, response=response, body=data)
        return APIStatusError(err_msg, response=response, body=data)
class AsyncOpenAI(AsyncAPIClient):
        api_key: str | Callable[[], Awaitable[str]] | None = None,
        # We provide a `DefaultAsyncHttpxClient` class that you can pass to retain the default values we use for `limits`, `timeout` & `follow_redirects`.
        # See the [httpx documentation](https://www.python-httpx.org/api/#asyncclient) for more details.
        """Construct a new async AsyncOpenAI client instance.
            self._api_key_provider: Callable[[], Awaitable[str]] | None = api_key
        self._default_stream_cls = AsyncStream
    def completions(self) -> AsyncCompletions:
        from .resources.completions import AsyncCompletions
        return AsyncCompletions(self)
    def chat(self) -> AsyncChat:
        from .resources.chat import AsyncChat
        return AsyncChat(self)
    def embeddings(self) -> AsyncEmbeddings:
        from .resources.embeddings import AsyncEmbeddings
        return AsyncEmbeddings(self)
    def files(self) -> AsyncFiles:
        from .resources.files import AsyncFiles
        return AsyncFiles(self)
    def images(self) -> AsyncImages:
        from .resources.images import AsyncImages
        return AsyncImages(self)
    def audio(self) -> AsyncAudio:
        from .resources.audio import AsyncAudio
        return AsyncAudio(self)
    def moderations(self) -> AsyncModerations:
        from .resources.moderations import AsyncModerations
        return AsyncModerations(self)
    def models(self) -> AsyncModels:
        from .resources.models import AsyncModels
        return AsyncModels(self)
    def fine_tuning(self) -> AsyncFineTuning:
        from .resources.fine_tuning import AsyncFineTuning
        return AsyncFineTuning(self)
    def vector_stores(self) -> AsyncVectorStores:
        from .resources.vector_stores import AsyncVectorStores
        return AsyncVectorStores(self)
    def webhooks(self) -> AsyncWebhooks:
        from .resources.webhooks import AsyncWebhooks
        return AsyncWebhooks(self)
    def beta(self) -> AsyncBeta:
        from .resources.beta import AsyncBeta
        return AsyncBeta(self)
    def batches(self) -> AsyncBatches:
        from .resources.batches import AsyncBatches
        return AsyncBatches(self)
    def uploads(self) -> AsyncUploads:
        from .resources.uploads import AsyncUploads
        return AsyncUploads(self)
    def responses(self) -> AsyncResponses:
        from .resources.responses import AsyncResponses
        return AsyncResponses(self)
    def realtime(self) -> AsyncRealtime:
        from .resources.realtime import AsyncRealtime
        return AsyncRealtime(self)
    def conversations(self) -> AsyncConversations:
        from .resources.conversations import AsyncConversations
        return AsyncConversations(self)
    def evals(self) -> AsyncEvals:
        from .resources.evals import AsyncEvals
        return AsyncEvals(self)
    def containers(self) -> AsyncContainers:
        from .resources.containers import AsyncContainers
        return AsyncContainers(self)
    def skills(self) -> AsyncSkills:
        from .resources.skills import AsyncSkills
        return AsyncSkills(self)
    def videos(self) -> AsyncVideos:
        from .resources.videos import AsyncVideos
        return AsyncVideos(self)
    def with_raw_response(self) -> AsyncOpenAIWithRawResponse:
        return AsyncOpenAIWithRawResponse(self)
    def with_streaming_response(self) -> AsyncOpenAIWithStreamedResponse:
        return AsyncOpenAIWithStreamedResponse(self)
    async def _refresh_api_key(self) -> None:
            self.api_key = await self._api_key_provider()
    async def _prepare_options(self, options: FinalRequestOptions) -> FinalRequestOptions:
        await self._refresh_api_key()
        return await super()._prepare_options(options)
            "X-Stainless-Async": f"async:{get_async_library()}",
class OpenAIWithRawResponse:
    _client: OpenAI
    def __init__(self, client: OpenAI) -> None:
    def completions(self) -> completions.CompletionsWithRawResponse:
        from .resources.completions import CompletionsWithRawResponse
        return CompletionsWithRawResponse(self._client.completions)
    def chat(self) -> chat.ChatWithRawResponse:
        from .resources.chat import ChatWithRawResponse
        return ChatWithRawResponse(self._client.chat)
    def embeddings(self) -> embeddings.EmbeddingsWithRawResponse:
        from .resources.embeddings import EmbeddingsWithRawResponse
        return EmbeddingsWithRawResponse(self._client.embeddings)
    def files(self) -> files.FilesWithRawResponse:
        from .resources.files import FilesWithRawResponse
        return FilesWithRawResponse(self._client.files)
    def images(self) -> images.ImagesWithRawResponse:
        from .resources.images import ImagesWithRawResponse
        return ImagesWithRawResponse(self._client.images)
    def audio(self) -> audio.AudioWithRawResponse:
        from .resources.audio import AudioWithRawResponse
        return AudioWithRawResponse(self._client.audio)
    def moderations(self) -> moderations.ModerationsWithRawResponse:
        from .resources.moderations import ModerationsWithRawResponse
        return ModerationsWithRawResponse(self._client.moderations)
    def models(self) -> models.ModelsWithRawResponse:
        from .resources.models import ModelsWithRawResponse
        return ModelsWithRawResponse(self._client.models)
    def fine_tuning(self) -> fine_tuning.FineTuningWithRawResponse:
        from .resources.fine_tuning import FineTuningWithRawResponse
        return FineTuningWithRawResponse(self._client.fine_tuning)
    def vector_stores(self) -> vector_stores.VectorStoresWithRawResponse:
        from .resources.vector_stores import VectorStoresWithRawResponse
        return VectorStoresWithRawResponse(self._client.vector_stores)
    def beta(self) -> beta.BetaWithRawResponse:
        from .resources.beta import BetaWithRawResponse
        return BetaWithRawResponse(self._client.beta)
    def batches(self) -> batches.BatchesWithRawResponse:
        from .resources.batches import BatchesWithRawResponse
        return BatchesWithRawResponse(self._client.batches)
    def uploads(self) -> uploads.UploadsWithRawResponse:
        from .resources.uploads import UploadsWithRawResponse
        return UploadsWithRawResponse(self._client.uploads)
    def responses(self) -> responses.ResponsesWithRawResponse:
        from .resources.responses import ResponsesWithRawResponse
        return ResponsesWithRawResponse(self._client.responses)
    def realtime(self) -> realtime.RealtimeWithRawResponse:
        from .resources.realtime import RealtimeWithRawResponse
        return RealtimeWithRawResponse(self._client.realtime)
    def conversations(self) -> conversations.ConversationsWithRawResponse:
        from .resources.conversations import ConversationsWithRawResponse
        return ConversationsWithRawResponse(self._client.conversations)
    def evals(self) -> evals.EvalsWithRawResponse:
        from .resources.evals import EvalsWithRawResponse
        return EvalsWithRawResponse(self._client.evals)
    def containers(self) -> containers.ContainersWithRawResponse:
        from .resources.containers import ContainersWithRawResponse
        return ContainersWithRawResponse(self._client.containers)
    def skills(self) -> skills.SkillsWithRawResponse:
        from .resources.skills import SkillsWithRawResponse
        return SkillsWithRawResponse(self._client.skills)
    def videos(self) -> videos.VideosWithRawResponse:
        from .resources.videos import VideosWithRawResponse
        return VideosWithRawResponse(self._client.videos)
class AsyncOpenAIWithRawResponse:
    _client: AsyncOpenAI
    def __init__(self, client: AsyncOpenAI) -> None:
    def completions(self) -> completions.AsyncCompletionsWithRawResponse:
        from .resources.completions import AsyncCompletionsWithRawResponse
        return AsyncCompletionsWithRawResponse(self._client.completions)
    def chat(self) -> chat.AsyncChatWithRawResponse:
        from .resources.chat import AsyncChatWithRawResponse
        return AsyncChatWithRawResponse(self._client.chat)
    def embeddings(self) -> embeddings.AsyncEmbeddingsWithRawResponse:
        from .resources.embeddings import AsyncEmbeddingsWithRawResponse
        return AsyncEmbeddingsWithRawResponse(self._client.embeddings)
    def files(self) -> files.AsyncFilesWithRawResponse:
        from .resources.files import AsyncFilesWithRawResponse
        return AsyncFilesWithRawResponse(self._client.files)
    def images(self) -> images.AsyncImagesWithRawResponse:
        from .resources.images import AsyncImagesWithRawResponse
        return AsyncImagesWithRawResponse(self._client.images)
    def audio(self) -> audio.AsyncAudioWithRawResponse:
        from .resources.audio import AsyncAudioWithRawResponse
        return AsyncAudioWithRawResponse(self._client.audio)
    def moderations(self) -> moderations.AsyncModerationsWithRawResponse:
        from .resources.moderations import AsyncModerationsWithRawResponse
        return AsyncModerationsWithRawResponse(self._client.moderations)
    def models(self) -> models.AsyncModelsWithRawResponse:
        from .resources.models import AsyncModelsWithRawResponse
        return AsyncModelsWithRawResponse(self._client.models)
    def fine_tuning(self) -> fine_tuning.AsyncFineTuningWithRawResponse:
        from .resources.fine_tuning import AsyncFineTuningWithRawResponse
        return AsyncFineTuningWithRawResponse(self._client.fine_tuning)
    def vector_stores(self) -> vector_stores.AsyncVectorStoresWithRawResponse:
        from .resources.vector_stores import AsyncVectorStoresWithRawResponse
        return AsyncVectorStoresWithRawResponse(self._client.vector_stores)
    def beta(self) -> beta.AsyncBetaWithRawResponse:
        from .resources.beta import AsyncBetaWithRawResponse
        return AsyncBetaWithRawResponse(self._client.beta)
    def batches(self) -> batches.AsyncBatchesWithRawResponse:
        from .resources.batches import AsyncBatchesWithRawResponse
        return AsyncBatchesWithRawResponse(self._client.batches)
    def uploads(self) -> uploads.AsyncUploadsWithRawResponse:
        from .resources.uploads import AsyncUploadsWithRawResponse
        return AsyncUploadsWithRawResponse(self._client.uploads)
    def responses(self) -> responses.AsyncResponsesWithRawResponse:
        from .resources.responses import AsyncResponsesWithRawResponse
        return AsyncResponsesWithRawResponse(self._client.responses)
    def realtime(self) -> realtime.AsyncRealtimeWithRawResponse:
        from .resources.realtime import AsyncRealtimeWithRawResponse
        return AsyncRealtimeWithRawResponse(self._client.realtime)
    def conversations(self) -> conversations.AsyncConversationsWithRawResponse:
        from .resources.conversations import AsyncConversationsWithRawResponse
        return AsyncConversationsWithRawResponse(self._client.conversations)
    def evals(self) -> evals.AsyncEvalsWithRawResponse:
        from .resources.evals import AsyncEvalsWithRawResponse
        return AsyncEvalsWithRawResponse(self._client.evals)
    def containers(self) -> containers.AsyncContainersWithRawResponse:
        from .resources.containers import AsyncContainersWithRawResponse
        return AsyncContainersWithRawResponse(self._client.containers)
    def skills(self) -> skills.AsyncSkillsWithRawResponse:
        from .resources.skills import AsyncSkillsWithRawResponse
        return AsyncSkillsWithRawResponse(self._client.skills)
    def videos(self) -> videos.AsyncVideosWithRawResponse:
        from .resources.videos import AsyncVideosWithRawResponse
        return AsyncVideosWithRawResponse(self._client.videos)
class OpenAIWithStreamedResponse:
    def completions(self) -> completions.CompletionsWithStreamingResponse:
        from .resources.completions import CompletionsWithStreamingResponse
        return CompletionsWithStreamingResponse(self._client.completions)
    def chat(self) -> chat.ChatWithStreamingResponse:
        from .resources.chat import ChatWithStreamingResponse
        return ChatWithStreamingResponse(self._client.chat)
    def embeddings(self) -> embeddings.EmbeddingsWithStreamingResponse:
        from .resources.embeddings import EmbeddingsWithStreamingResponse
        return EmbeddingsWithStreamingResponse(self._client.embeddings)
    def files(self) -> files.FilesWithStreamingResponse:
        from .resources.files import FilesWithStreamingResponse
        return FilesWithStreamingResponse(self._client.files)
    def images(self) -> images.ImagesWithStreamingResponse:
        from .resources.images import ImagesWithStreamingResponse
        return ImagesWithStreamingResponse(self._client.images)
    def audio(self) -> audio.AudioWithStreamingResponse:
        from .resources.audio import AudioWithStreamingResponse
        return AudioWithStreamingResponse(self._client.audio)
    def moderations(self) -> moderations.ModerationsWithStreamingResponse:
        from .resources.moderations import ModerationsWithStreamingResponse
        return ModerationsWithStreamingResponse(self._client.moderations)
    def models(self) -> models.ModelsWithStreamingResponse:
        from .resources.models import ModelsWithStreamingResponse
        return ModelsWithStreamingResponse(self._client.models)
    def fine_tuning(self) -> fine_tuning.FineTuningWithStreamingResponse:
        from .resources.fine_tuning import FineTuningWithStreamingResponse
        return FineTuningWithStreamingResponse(self._client.fine_tuning)
    def vector_stores(self) -> vector_stores.VectorStoresWithStreamingResponse:
        from .resources.vector_stores import VectorStoresWithStreamingResponse
        return VectorStoresWithStreamingResponse(self._client.vector_stores)
    def beta(self) -> beta.BetaWithStreamingResponse:
        from .resources.beta import BetaWithStreamingResponse
        return BetaWithStreamingResponse(self._client.beta)
    def batches(self) -> batches.BatchesWithStreamingResponse:
        from .resources.batches import BatchesWithStreamingResponse
        return BatchesWithStreamingResponse(self._client.batches)
    def uploads(self) -> uploads.UploadsWithStreamingResponse:
        from .resources.uploads import UploadsWithStreamingResponse
        return UploadsWithStreamingResponse(self._client.uploads)
    def responses(self) -> responses.ResponsesWithStreamingResponse:
        from .resources.responses import ResponsesWithStreamingResponse
        return ResponsesWithStreamingResponse(self._client.responses)
    def realtime(self) -> realtime.RealtimeWithStreamingResponse:
        from .resources.realtime import RealtimeWithStreamingResponse
        return RealtimeWithStreamingResponse(self._client.realtime)
    def conversations(self) -> conversations.ConversationsWithStreamingResponse:
        from .resources.conversations import ConversationsWithStreamingResponse
        return ConversationsWithStreamingResponse(self._client.conversations)
    def evals(self) -> evals.EvalsWithStreamingResponse:
        from .resources.evals import EvalsWithStreamingResponse
        return EvalsWithStreamingResponse(self._client.evals)
    def containers(self) -> containers.ContainersWithStreamingResponse:
        from .resources.containers import ContainersWithStreamingResponse
        return ContainersWithStreamingResponse(self._client.containers)
    def skills(self) -> skills.SkillsWithStreamingResponse:
        from .resources.skills import SkillsWithStreamingResponse
        return SkillsWithStreamingResponse(self._client.skills)
    def videos(self) -> videos.VideosWithStreamingResponse:
        from .resources.videos import VideosWithStreamingResponse
        return VideosWithStreamingResponse(self._client.videos)
class AsyncOpenAIWithStreamedResponse:
    def completions(self) -> completions.AsyncCompletionsWithStreamingResponse:
        from .resources.completions import AsyncCompletionsWithStreamingResponse
        return AsyncCompletionsWithStreamingResponse(self._client.completions)
    def chat(self) -> chat.AsyncChatWithStreamingResponse:
        from .resources.chat import AsyncChatWithStreamingResponse
        return AsyncChatWithStreamingResponse(self._client.chat)
    def embeddings(self) -> embeddings.AsyncEmbeddingsWithStreamingResponse:
        from .resources.embeddings import AsyncEmbeddingsWithStreamingResponse
        return AsyncEmbeddingsWithStreamingResponse(self._client.embeddings)
    def files(self) -> files.AsyncFilesWithStreamingResponse:
        from .resources.files import AsyncFilesWithStreamingResponse
        return AsyncFilesWithStreamingResponse(self._client.files)
    def images(self) -> images.AsyncImagesWithStreamingResponse:
        from .resources.images import AsyncImagesWithStreamingResponse
        return AsyncImagesWithStreamingResponse(self._client.images)
    def audio(self) -> audio.AsyncAudioWithStreamingResponse:
        from .resources.audio import AsyncAudioWithStreamingResponse
        return AsyncAudioWithStreamingResponse(self._client.audio)
    def moderations(self) -> moderations.AsyncModerationsWithStreamingResponse:
        from .resources.moderations import AsyncModerationsWithStreamingResponse
        return AsyncModerationsWithStreamingResponse(self._client.moderations)
    def models(self) -> models.AsyncModelsWithStreamingResponse:
        from .resources.models import AsyncModelsWithStreamingResponse
        return AsyncModelsWithStreamingResponse(self._client.models)
    def fine_tuning(self) -> fine_tuning.AsyncFineTuningWithStreamingResponse:
        from .resources.fine_tuning import AsyncFineTuningWithStreamingResponse
        return AsyncFineTuningWithStreamingResponse(self._client.fine_tuning)
    def vector_stores(self) -> vector_stores.AsyncVectorStoresWithStreamingResponse:
        from .resources.vector_stores import AsyncVectorStoresWithStreamingResponse
        return AsyncVectorStoresWithStreamingResponse(self._client.vector_stores)
    def beta(self) -> beta.AsyncBetaWithStreamingResponse:
        from .resources.beta import AsyncBetaWithStreamingResponse
        return AsyncBetaWithStreamingResponse(self._client.beta)
    def batches(self) -> batches.AsyncBatchesWithStreamingResponse:
        from .resources.batches import AsyncBatchesWithStreamingResponse
        return AsyncBatchesWithStreamingResponse(self._client.batches)
    def uploads(self) -> uploads.AsyncUploadsWithStreamingResponse:
        from .resources.uploads import AsyncUploadsWithStreamingResponse
        return AsyncUploadsWithStreamingResponse(self._client.uploads)
    def responses(self) -> responses.AsyncResponsesWithStreamingResponse:
        from .resources.responses import AsyncResponsesWithStreamingResponse
        return AsyncResponsesWithStreamingResponse(self._client.responses)
    def realtime(self) -> realtime.AsyncRealtimeWithStreamingResponse:
        from .resources.realtime import AsyncRealtimeWithStreamingResponse
        return AsyncRealtimeWithStreamingResponse(self._client.realtime)
    def conversations(self) -> conversations.AsyncConversationsWithStreamingResponse:
        from .resources.conversations import AsyncConversationsWithStreamingResponse
        return AsyncConversationsWithStreamingResponse(self._client.conversations)
    def evals(self) -> evals.AsyncEvalsWithStreamingResponse:
        from .resources.evals import AsyncEvalsWithStreamingResponse
        return AsyncEvalsWithStreamingResponse(self._client.evals)
    def containers(self) -> containers.AsyncContainersWithStreamingResponse:
        from .resources.containers import AsyncContainersWithStreamingResponse
        return AsyncContainersWithStreamingResponse(self._client.containers)
    def skills(self) -> skills.AsyncSkillsWithStreamingResponse:
        from .resources.skills import AsyncSkillsWithStreamingResponse
        return AsyncSkillsWithStreamingResponse(self._client.skills)
    def videos(self) -> videos.AsyncVideosWithStreamingResponse:
        from .resources.videos import AsyncVideosWithStreamingResponse
        return AsyncVideosWithStreamingResponse(self._client.videos)
Client = OpenAI
AsyncClient = AsyncOpenAI
from contextlib import asynccontextmanager, contextmanager
from ._auth import Auth, BasicAuth, FunctionAuth
from ._config import (
    DEFAULT_LIMITS,
    DEFAULT_MAX_REDIRECTS,
    DEFAULT_TIMEOUT_CONFIG,
    Limits,
    Proxy,
from ._decoders import SUPPORTED_DECODERS
    request_context,
from ._models import Cookies, Headers, Request, Response
from ._status_codes import codes
from ._transports.base import AsyncBaseTransport, BaseTransport
from ._transports.default import AsyncHTTPTransport, HTTPTransport
    AsyncByteStream,
    AuthTypes,
    CertTypes,
    CookieTypes,
    HeaderTypes,
    ProxyTypes,
    QueryParamTypes,
    RequestContent,
    RequestData,
    RequestExtensions,
    SyncByteStream,
    TimeoutTypes,
from ._urls import URL, QueryParams
from ._utils import URLPattern, get_environment_proxies
    import ssl  # pragma: no cover
__all__ = ["USE_CLIENT_DEFAULT", "AsyncClient", "Client"]
# The type annotation for @classmethod and context managers here follows PEP 484
# https://www.python.org/dev/peps/pep-0484/#annotating-instance-and-class-methods
T = typing.TypeVar("T", bound="Client")
U = typing.TypeVar("U", bound="AsyncClient")
def _is_https_redirect(url: URL, location: URL) -> bool:
    Return 'True' if 'location' is a HTTPS upgrade of 'url'
    if url.host != location.host:
        url.scheme == "http"
        and _port_or_default(url) == 80
        and location.scheme == "https"
        and _port_or_default(location) == 443
def _port_or_default(url: URL) -> int | None:
    if url.port is not None:
        return url.port
    return {"http": 80, "https": 443}.get(url.scheme)
def _same_origin(url: URL, other: URL) -> bool:
    Return 'True' if the given URLs share the same origin.
        url.scheme == other.scheme
        and url.host == other.host
        and _port_or_default(url) == _port_or_default(other)
class UseClientDefault:
    For some parameters such as `auth=...` and `timeout=...` we need to be able
    to indicate the default "unset" state, in a way that is distinctly different
    to using `None`.
    The default "unset" state indicates that whatever default is set on the
    client should be used. This is different to setting `None`, which
    explicitly disables the parameter, possibly overriding a client default.
    For example we use `timeout=USE_CLIENT_DEFAULT` in the `request()` signature.
    Omitting the `timeout` parameter will send a request using whatever default
    timeout has been configured on the client. Including `timeout=None` will
    ensure no timeout is used.
    Note that user code shouldn't need to use the `USE_CLIENT_DEFAULT` constant,
    but it is used internally when a parameter is not included.
USE_CLIENT_DEFAULT = UseClientDefault()
logger = logging.getLogger("httpx")
USER_AGENT = f"python-httpx/{__version__}"
ACCEPT_ENCODING = ", ".join(
    [key for key in SUPPORTED_DECODERS.keys() if key != "identity"]
class ClientState(enum.Enum):
    # UNOPENED:
    #   The client has been instantiated, but has not been used to send a request,
    #   or been opened by entering the context of a `with` block.
    UNOPENED = 1
    # OPENED:
    #   The client has either sent a request, or is within a `with` block.
    OPENED = 2
    # CLOSED:
    #   The client has either exited the `with` block, or `close()` has
    #   been called explicitly.
    CLOSED = 3
class BoundSyncStream(SyncByteStream):
    A byte stream that is bound to a given response instance, and that
    ensures the `response.elapsed` is set once the response is closed.
        self, stream: SyncByteStream, response: Response, start: float
        self._response = response
        for chunk in self._stream:
        elapsed = time.perf_counter() - self._start
        self._response.elapsed = datetime.timedelta(seconds=elapsed)
        self._stream.close()
class BoundAsyncStream(AsyncByteStream):
    An async byte stream that is bound to a given response instance, and that
        self, stream: AsyncByteStream, response: Response, start: float
        async for chunk in self._stream:
        await self._stream.aclose()
EventHook = typing.Callable[..., typing.Any]
        auth: AuthTypes | None = None,
        params: QueryParamTypes | None = None,
        headers: HeaderTypes | None = None,
        cookies: CookieTypes | None = None,
        timeout: TimeoutTypes = DEFAULT_TIMEOUT_CONFIG,
        follow_redirects: bool = False,
        max_redirects: int = DEFAULT_MAX_REDIRECTS,
        event_hooks: None | (typing.Mapping[str, list[EventHook]]) = None,
        base_url: URL | str = "",
        trust_env: bool = True,
        default_encoding: str | typing.Callable[[bytes], str] = "utf-8",
        event_hooks = {} if event_hooks is None else event_hooks
        self._auth = self._build_auth(auth)
        self._params = QueryParams(params)
        self.headers = Headers(headers)
        self._cookies = Cookies(cookies)
        self._timeout = Timeout(timeout)
        self.follow_redirects = follow_redirects
        self.max_redirects = max_redirects
        self._event_hooks = {
            "request": list(event_hooks.get("request", [])),
            "response": list(event_hooks.get("response", [])),
        self._trust_env = trust_env
        self._default_encoding = default_encoding
        self._state = ClientState.UNOPENED
        Check if the client being closed
        return self._state == ClientState.CLOSED
    def trust_env(self) -> bool:
        return self._trust_env
    def _get_proxy_map(
        self, proxy: ProxyTypes | None, allow_env_proxies: bool
    ) -> dict[str, Proxy | None]:
        if proxy is None:
            if allow_env_proxies:
                    key: None if url is None else Proxy(url=url)
                    for key, url in get_environment_proxies().items()
            proxy = Proxy(url=proxy) if isinstance(proxy, (str, URL)) else proxy
            return {"all://": proxy}
    def timeout(self) -> Timeout:
        return self._timeout
    @timeout.setter
    def timeout(self, timeout: TimeoutTypes) -> None:
    def event_hooks(self) -> dict[str, list[EventHook]]:
        return self._event_hooks
    @event_hooks.setter
    def event_hooks(self, event_hooks: dict[str, list[EventHook]]) -> None:
    def auth(self) -> Auth | None:
        Authentication class used when none is passed at the request-level.
        See also [Authentication][0].
        [0]: /quickstart/#authentication
        return self._auth
    @auth.setter
    def auth(self, auth: AuthTypes) -> None:
        Base URL to use when sending requests with relative URLs.
        self._base_url = self._enforce_trailing_slash(URL(url))
    def headers(self) -> Headers:
        HTTP headers to include when sending requests.
    def headers(self, headers: HeaderTypes) -> None:
        client_headers = Headers(
                b"Accept": b"*/*",
                b"Accept-Encoding": ACCEPT_ENCODING.encode("ascii"),
                b"Connection": b"keep-alive",
                b"User-Agent": USER_AGENT.encode("ascii"),
        client_headers.update(headers)
        self._headers = client_headers
    def cookies(self) -> Cookies:
        Cookie values to include when sending requests.
        return self._cookies
    @cookies.setter
    def cookies(self, cookies: CookieTypes) -> None:
    def params(self) -> QueryParams:
        Query parameters to include in the URL when sending requests.
        return self._params
    @params.setter
    def params(self, params: QueryParamTypes) -> None:
    def build_request(
        url: URL | str,
        content: RequestContent | None = None,
        data: RequestData | None = None,
        timeout: TimeoutTypes | UseClientDefault = USE_CLIENT_DEFAULT,
        extensions: RequestExtensions | None = None,
    ) -> Request:
        Build and return a request instance.
        * The `params`, `headers` and `cookies` arguments
        are merged with any values set on the client.
        * The `url` argument is merged with any `base_url` set on the client.
        See also: [Request instances][0]
        [0]: /advanced/clients/#request-instances
        url = self._merge_url(url)
        headers = self._merge_headers(headers)
        cookies = self._merge_cookies(cookies)
        params = self._merge_queryparams(params)
        extensions = {} if extensions is None else extensions
        if "timeout" not in extensions:
            timeout = (
                self.timeout
                if isinstance(timeout, UseClientDefault)
                else Timeout(timeout)
            extensions = dict(**extensions, timeout=timeout.as_dict())
            extensions=extensions,
    def _merge_url(self, url: URL | str) -> URL:
            # To merge URLs we always append to the base URL. To get this
            # behaviour correct we always ensure the base URL ends in a '/'
            # separator, and strip any leading '/' from the merge URL.
            # So, eg...
            # >>> client = Client(base_url="https://www.example.com/subpath")
            # >>> client.base_url
            # URL('https://www.example.com/subpath/')
            # >>> client.build_request("GET", "/path").url
            # URL('https://www.example.com/subpath/path')
    def _merge_cookies(self, cookies: CookieTypes | None = None) -> CookieTypes | None:
        Merge a cookies argument together with any cookies on the client,
        to create the cookies used for the outgoing request.
        if cookies or self.cookies:
            merged_cookies = Cookies(self.cookies)
            merged_cookies.update(cookies)
            return merged_cookies
    def _merge_headers(self, headers: HeaderTypes | None = None) -> HeaderTypes | None:
        Merge a headers argument together with any headers on the client,
        to create the headers used for the outgoing request.
        merged_headers = Headers(self.headers)
        merged_headers.update(headers)
        return merged_headers
    def _merge_queryparams(
        self, params: QueryParamTypes | None = None
    ) -> QueryParamTypes | None:
        Merge a queryparams argument together with any queryparams on the client,
        to create the queryparams used for the outgoing request.
        if params or self.params:
            merged_queryparams = QueryParams(self.params)
            return merged_queryparams.merge(params)
    def _build_auth(self, auth: AuthTypes | None) -> Auth | None:
        elif isinstance(auth, tuple):
            return BasicAuth(username=auth[0], password=auth[1])
        elif isinstance(auth, Auth):
        elif callable(auth):
            return FunctionAuth(func=auth)
            raise TypeError(f'Invalid "auth" argument: {auth!r}')
    def _build_request_auth(
        request: Request,
        auth: AuthTypes | UseClientDefault | None = USE_CLIENT_DEFAULT,
    ) -> Auth:
        auth = (
            self._auth if isinstance(auth, UseClientDefault) else self._build_auth(auth)
        username, password = request.url.username, request.url.password
        if username or password:
            return BasicAuth(username=username, password=password)
        return Auth()
    def _build_redirect_request(self, request: Request, response: Response) -> Request:
        Given a request and a redirect response, return a new request that
        should be used to effect the redirect.
        method = self._redirect_method(request, response)
        url = self._redirect_url(request, response)
        headers = self._redirect_headers(request, url, method)
        stream = self._redirect_stream(request, method)
        cookies = Cookies(self.cookies)
            extensions=request.extensions,
    def _redirect_method(self, request: Request, response: Response) -> str:
        When being redirected we may want to change the method of the request
        method = request.method
        if response.status_code == codes.SEE_OTHER and method != "HEAD":
        # Turn 302s into GETs.
        if response.status_code == codes.FOUND and method != "HEAD":
        # If a POST is responded to with a 301, turn it into a GET.
        # This bizarre behaviour is explained in 'requests' issue 1704.
        if response.status_code == codes.MOVED_PERMANENTLY and method == "POST":
    def _redirect_url(self, request: Request, response: Response) -> URL:
        Return the URL for the redirect to follow.
        location = response.headers["Location"]
            url = URL(location)
        except InvalidURL as exc:
            raise RemoteProtocolError(
                f"Invalid URL in location header: {exc}.", request=request
        # Handle malformed 'Location' headers that are "absolute" form, have no host.
        # See: https://github.com/encode/httpx/issues/771
        if url.scheme and not url.host:
            url = url.copy_with(host=request.url.host)
        # Facilitate relative 'Location' headers, as allowed by RFC 7231.
        if url.is_relative_url:
            url = request.url.join(url)
        # Attach previous fragment if needed (RFC 7231 7.1.2)
        if request.url.fragment and not url.fragment:
            url = url.copy_with(fragment=request.url.fragment)
    def _redirect_headers(self, request: Request, url: URL, method: str) -> Headers:
        Return the headers that should be used for the redirect request.
        headers = Headers(request.headers)
        if not _same_origin(url, request.url):
            if not _is_https_redirect(request.url, url):
                # Strip Authorization headers when responses are redirected
                # away from the origin. (Except for direct HTTP to HTTPS redirects.)
                headers.pop("Authorization", None)
            # Update the Host header.
            headers["Host"] = url.netloc.decode("ascii")
        if method != request.method and method == "GET":
            # If we've switch to a 'GET' request, then strip any headers which
            # are only relevant to the request body.
            headers.pop("Content-Length", None)
            headers.pop("Transfer-Encoding", None)
        # We should use the client cookie store to determine any cookie header,
        # rather than whatever was on the original outgoing request.
    def _redirect_stream(
        self, request: Request, method: str
    ) -> SyncByteStream | AsyncByteStream | None:
        Return the body that should be used for the redirect request.
        return request.stream
    def _set_timeout(self, request: Request) -> None:
        if "timeout" not in request.extensions:
                if isinstance(self.timeout, UseClientDefault)
                else Timeout(self.timeout)
            request.extensions = dict(**request.extensions, timeout=timeout.as_dict())
    An HTTP client, with connection pooling, HTTP/2, redirects, cookie persistence, etc.
    It can be shared between threads.
    >>> client = httpx.Client()
    >>> response = client.get('https://example.org')
    **Parameters:**
    * **auth** - *(optional)* An authentication class to use when sending
    * **params** - *(optional)* Query parameters to include in request URLs, as
    a string, dictionary, or sequence of two-tuples.
    * **headers** - *(optional)* Dictionary of HTTP headers to include when
    sending requests.
    * **cookies** - *(optional)* Dictionary of Cookie items to include when
    * **verify** - *(optional)* Either `True` to use an SSL context with the
    default CA bundle, `False` to disable verification, or an instance of
    `ssl.SSLContext` to use a custom context.
    * **http2** - *(optional)* A boolean indicating if HTTP/2 support should be
    enabled. Defaults to `False`.
    * **proxy** - *(optional)* A proxy URL where all the traffic should be routed.
    * **timeout** - *(optional)* The timeout configuration to use when sending
    * **limits** - *(optional)* The limits configuration to use.
    * **max_redirects** - *(optional)* The maximum number of redirect responses
    that should be followed.
    * **base_url** - *(optional)* A URL to use as the base when building
    request URLs.
    * **transport** - *(optional)* A transport class to use for sending requests
    over the network.
    * **trust_env** - *(optional)* Enables or disables usage of environment
    variables for configuration.
    * **default_encoding** - *(optional)* The default encoding to use for decoding
    response text, if no charset information is included in a response Content-Type
    header. Set to a callable for automatic character set detection. Default: "utf-8".
        verify: ssl.SSLContext | str | bool = True,
        cert: CertTypes | None = None,
        http1: bool = True,
        http2: bool = False,
        proxy: ProxyTypes | None = None,
        mounts: None | (typing.Mapping[str, BaseTransport | None]) = None,
        limits: Limits = DEFAULT_LIMITS,
        transport: BaseTransport | None = None,
            follow_redirects=follow_redirects,
            max_redirects=max_redirects,
            event_hooks=event_hooks,
            trust_env=trust_env,
            default_encoding=default_encoding,
        if http2:
                import h2  # noqa
                    "Using http2=True, but the 'h2' package is not installed. "
                    "Make sure to install httpx using `pip install httpx[http2]`."
        allow_env_proxies = trust_env and transport is None
        proxy_map = self._get_proxy_map(proxy, allow_env_proxies)
        self._transport = self._init_transport(
            http1=http1,
            http2=http2,
            limits=limits,
            transport=transport,
        self._mounts: dict[URLPattern, BaseTransport | None] = {
            URLPattern(key): None
            if proxy is None
            else self._init_proxy_transport(
            for key, proxy in proxy_map.items()
        if mounts is not None:
            self._mounts.update(
                {URLPattern(key): transport for key, transport in mounts.items()}
        self._mounts = dict(sorted(self._mounts.items()))
    def _init_transport(
    ) -> BaseTransport:
        if transport is not None:
            return transport
        return HTTPTransport(
    def _init_proxy_transport(
        proxy: Proxy,
            proxy=proxy,
    def _transport_for_url(self, url: URL) -> BaseTransport:
        Returns the transport instance that should be used for a given URL.
        This will either be the standard connection pool, or a proxy.
        for pattern, transport in self._mounts.items():
            if pattern.matches(url):
                return self._transport if transport is None else transport
        return self._transport
        follow_redirects: bool | UseClientDefault = USE_CLIENT_DEFAULT,
        Build and send a request.
        Equivalent to:
        request = client.build_request(...)
        response = client.send(request, ...)
        See `Client.build_request()`, `Client.send()` and
        [Merging of configuration][0] for how the various parameters
        are merged with client-level configuration.
        [0]: /advanced/clients/#merging-of-configuration
        if cookies is not None:
                "Setting per-request cookies=<...> is being deprecated, because "
                "the expected behaviour on cookie persistence is ambiguous. Set "
                "cookies directly on the client instance instead."
            warnings.warn(message, DeprecationWarning, stacklevel=2)
        request = self.build_request(
        return self.send(request, auth=auth, follow_redirects=follow_redirects)
    ) -> typing.Iterator[Response]:
        Alternative to `httpx.request()` that streams the response body
        instead of loading it into memory at once.
        **Parameters**: See `httpx.request`.
        See also: [Streaming Responses][0]
        [0]: /quickstart#streaming-responses
        response = self.send(
            yield response
        Send a request.
        The request is sent as-is, unmodified.
        Typically you'll want to build one with `Client.build_request()`
        so that any client-level configuration is merged into the request,
        but passing an explicit `httpx.Request()` is supported as well.
        if self._state == ClientState.CLOSED:
            raise RuntimeError("Cannot send a request, as the client has been closed.")
        self._state = ClientState.OPENED
        follow_redirects = (
            self.follow_redirects
            if isinstance(follow_redirects, UseClientDefault)
            else follow_redirects
        self._set_timeout(request)
        auth = self._build_request_auth(request, auth)
        response = self._send_handling_auth(
            history=[],
    def _send_handling_auth(
        auth: Auth,
        follow_redirects: bool,
        history: list[Response],
        auth_flow = auth.sync_auth_flow(request)
            request = next(auth_flow)
                response = self._send_handling_redirects(
                        next_request = auth_flow.send(response)
                    response.history = list(history)
                    request = next_request
                    history.append(response)
            auth_flow.close()
    def _send_handling_redirects(
            if len(history) > self.max_redirects:
                    "Exceeded maximum allowed redirects.", request=request
            for hook in self._event_hooks["request"]:
                hook(request)
            response = self._send_single_request(request)
                for hook in self._event_hooks["response"]:
                    hook(response)
                if not response.has_redirect_location:
                request = self._build_redirect_request(request, response)
                history = history + [response]
                if follow_redirects:
                    response.next_request = request
    def _send_single_request(self, request: Request) -> Response:
        Sends a single request, without handling any redirections.
        transport = self._transport_for_url(request.url)
        start = time.perf_counter()
        if not isinstance(request.stream, SyncByteStream):
                "Attempted to send an async request with a sync Client instance."
        with request_context(request=request):
            response = transport.handle_request(request)
        assert isinstance(response.stream, SyncByteStream)
        response.stream = BoundSyncStream(
            response.stream, response=response, start=start
        self.cookies.extract_cookies(response)
        response.default_encoding = self._default_encoding
            'HTTP Request: %s %s "%s %d %s"',
            response.http_version,
        Send a `GET` request.
        auth: AuthTypes | UseClientDefault = USE_CLIENT_DEFAULT,
        Send an `OPTIONS` request.
        Send a `HEAD` request.
        Send a `POST` request.
        Send a `PUT` request.
        Send a `PATCH` request.
        Send a `DELETE` request.
        Close transport and proxies.
        if self._state != ClientState.CLOSED:
            self._state = ClientState.CLOSED
            self._transport.close()
            for transport in self._mounts.values():
                    transport.close()
    def __enter__(self: T) -> T:
        if self._state != ClientState.UNOPENED:
                ClientState.OPENED: "Cannot open a client instance more than once.",
                ClientState.CLOSED: (
                    "Cannot reopen a client instance, once it has been closed."
            }[self._state]
        self._transport.__enter__()
                transport.__enter__()
        exc_type: type[BaseException] | None = None,
        exc_value: BaseException | None = None,
        traceback: TracebackType | None = None,
        self._transport.__exit__(exc_type, exc_value, traceback)
                transport.__exit__(exc_type, exc_value, traceback)
class AsyncClient(BaseClient):
    An asynchronous HTTP client, with connection pooling, HTTP/2, redirects,
    cookie persistence, etc.
    It can be shared between tasks.
    >>> async with httpx.AsyncClient() as client:
    >>>     response = await client.get('https://example.org')
        mounts: None | (typing.Mapping[str, AsyncBaseTransport | None]) = None,
        transport: AsyncBaseTransport | None = None,
        self._mounts: dict[URLPattern, AsyncBaseTransport | None] = {
    ) -> AsyncBaseTransport:
        return AsyncHTTPTransport(
    def _transport_for_url(self, url: URL) -> AsyncBaseTransport:
        response = await client.send(request, ...)
        See `AsyncClient.build_request()`, `AsyncClient.send()`
        and [Merging of configuration][0] for how the various parameters
        if cookies is not None:  # pragma: no cover
        return await self.send(request, auth=auth, follow_redirects=follow_redirects)
    @asynccontextmanager
    async def stream(
    ) -> typing.AsyncIterator[Response]:
        response = await self.send(
    async def send(
        Typically you'll want to build one with `AsyncClient.build_request()`
        response = await self._send_handling_auth(
    async def _send_handling_auth(
        auth_flow = auth.async_auth_flow(request)
            request = await auth_flow.__anext__()
                response = await self._send_handling_redirects(
                        next_request = await auth_flow.asend(response)
            await auth_flow.aclose()
    async def _send_handling_redirects(
                await hook(request)
            response = await self._send_single_request(request)
                    await hook(response)
    async def _send_single_request(self, request: Request) -> Response:
        if not isinstance(request.stream, AsyncByteStream):
                "Attempted to send an sync request with an AsyncClient instance."
            response = await transport.handle_async_request(request)
        assert isinstance(response.stream, AsyncByteStream)
        response.stream = BoundAsyncStream(
        return await self.request(
            await self._transport.aclose()
            for proxy in self._mounts.values():
                    await proxy.aclose()
    async def __aenter__(self: U) -> U:
        await self._transport.__aenter__()
                await proxy.__aenter__()
        await self._transport.__aexit__(exc_type, exc_value, traceback)
                await proxy.__aexit__(exc_type, exc_value, traceback)
    from .resources.webhooks import Webhooks, AsyncWebhooks
# Related resources:
#    https://huggingface.co/tasks
#    https://huggingface.co/docs/huggingface.js/inference/README
#    https://github.com/huggingface/huggingface.js/tree/main/packages/inference/src
#    https://github.com/huggingface/text-generation-inference/tree/main/clients/python
#    https://github.com/huggingface/text-generation-inference/blob/main/clients/python/text_generation/client.py
#    https://huggingface.slack.com/archives/C03E4DQ9LAJ/p1680169099087869
#    https://github.com/huggingface/unity-api#tasks
# Some TODO:
# - add all tasks
# NOTE: the philosophy of this client is "let's make it as easy as possible to use it, even if less optimized". Some
# examples of how it translates:
# - Timeout / Server unavailable is handled by the client in a single "timeout" parameter.
# - Files can be provided as bytes, file paths, or URLs and the client will try to "guess" the type.
# - Images are parsed as PIL.Image for easier manipulation.
# - Provides a "recommended model" for each task => suboptimal but user-wise quicker to get a first script running.
# - Only the main parameters are publicly exposed. Power users can always read the docs for more options.
from contextlib import ExitStack
from typing import TYPE_CHECKING, Any, Iterable, Literal, Optional, Union, overload
from huggingface_hub import constants
from huggingface_hub.errors import BadRequestError, HfHubHTTPError, InferenceTimeoutError
from huggingface_hub.inference._common import (
    TASKS_EXPECTING_IMAGES,
    ContentT,
    RequestParameters,
    _b64_encode,
    _b64_to_image,
    _bytes_to_dict,
    _bytes_to_image,
    _bytes_to_list,
    _get_unsupported_text_generation_kwargs,
    _import_numpy,
    _set_unsupported_text_generation_kwargs,
    _stream_chat_completion_response,
    _stream_text_generation_response,
    raise_text_generation_error,
from huggingface_hub.inference._generated.types import (
    AudioClassificationOutputElement,
    AudioClassificationOutputTransform,
    AudioToAudioOutputElement,
    AutomaticSpeechRecognitionOutput,
    ChatCompletionInputGrammarType,
    ChatCompletionInputMessage,
    ChatCompletionInputStreamOptions,
    ChatCompletionInputTool,
    ChatCompletionInputToolChoiceClass,
    ChatCompletionInputToolChoiceEnum,
    ChatCompletionOutput,
    ChatCompletionStreamOutput,
    DocumentQuestionAnsweringOutputElement,
    FillMaskOutputElement,
    ImageClassificationOutputElement,
    ImageClassificationOutputTransform,
    ImageSegmentationOutputElement,
    ImageSegmentationSubtask,
    ImageToImageTargetSize,
    ImageToTextOutput,
    ImageToVideoTargetSize,
    ObjectDetectionOutputElement,
    Padding,
    QuestionAnsweringOutputElement,
    SummarizationOutput,
    SummarizationTruncationStrategy,
    TableQuestionAnsweringOutputElement,
    TextClassificationOutputElement,
    TextClassificationOutputTransform,
    TextGenerationInputGrammarType,
    TextGenerationOutput,
    TextGenerationStreamOutput,
    TextToSpeechEarlyStoppingEnum,
    TokenClassificationAggregationStrategy,
    TokenClassificationOutputElement,
    TranslationOutput,
    TranslationTruncationStrategy,
    VisualQuestionAnsweringOutputElement,
    ZeroShotClassificationOutputElement,
    ZeroShotImageClassificationOutputElement,
from huggingface_hub.inference._providers import PROVIDER_OR_POLICY_T, get_provider_helper
from huggingface_hub.utils import (
    build_hf_headers,
    get_session,
    hf_raise_for_status,
    validate_hf_hub_args,
from huggingface_hub.utils._auth import get_token
    from PIL.Image import Image
MODEL_KWARGS_NOT_USED_REGEX = re.compile(r"The following `model_kwargs` are not used by the model: \[(.*?)\]")
class InferenceClient:
    Initialize a new Inference Client.
    [`InferenceClient`] aims to provide a unified experience to perform inference. The client can be used
    seamlessly with either the (free) Inference API, self-hosted Inference Endpoints, or third-party Inference Providers.
        model (`str`, `optional`):
            The model to run inference with. Can be a model id hosted on the Hugging Face Hub, e.g. `meta-llama/Meta-Llama-3-8B-Instruct`
            or a URL to a deployed Inference Endpoint. Defaults to None, in which case a recommended model is
            automatically selected for the task.
            Note: for better compatibility with OpenAI's client, `model` has been aliased as `base_url`. Those 2
            arguments are mutually exclusive. If a URL is passed as `model` or `base_url` for chat completion, the `(/v1)/chat/completions` suffix path will be appended to the URL.
        provider (`str`, *optional*):
            Name of the provider to use for inference. Can be `"black-forest-labs"`, `"cerebras"`, `"clarifai"`, `"cohere"`, `"fal-ai"`, `"featherless-ai"`, `"fireworks-ai"`, `"groq"`, `"hf-inference"`, `"hyperbolic"`, `"nebius"`, `"novita"`, `"nscale"`, `"openai"`, `"ovhcloud"`, `"publicai"`, `"replicate"`, `"sambanova"`, `"scaleway"`, `"together"`, `"wavespeed"` or `"zai-org"`.
            Defaults to "auto" i.e. the first of the providers available for the model, sorted by the user's order in https://hf.co/settings/inference-providers.
            If model is a URL or `base_url` is passed, then `provider` is not used.
        token (`str`, *optional*):
            Hugging Face token. Will default to the locally saved token if not provided.
            Note: for better compatibility with OpenAI's client, `token` has been aliased as `api_key`. Those 2
            arguments are mutually exclusive and have the exact same behavior.
        timeout (`float`, `optional`):
            The maximum number of seconds to wait for a response from the server. Defaults to None, meaning it will loop until the server is available.
        headers (`dict[str, str]`, `optional`):
            Additional headers to send to the server. By default only the authorization and user-agent headers are sent.
            Values in this dictionary will override the default values.
        bill_to (`str`, `optional`):
            The billing account to use for the requests. By default the requests are billed on the user's account.
            Requests can only be billed to an organization the user is a member of, and which has subscribed to Enterprise Hub.
        cookies (`dict[str, str]`, `optional`):
            Additional cookies to send to the server.
        base_url (`str`, `optional`):
            Base URL to run inference. This is a duplicated argument from `model` to make [`InferenceClient`]
            follow the same pattern as `openai.OpenAI` client. Cannot be used if `model` is set. Defaults to None.
        api_key (`str`, `optional`):
            Token to use for authentication. This is a duplicated argument from `token` to make [`InferenceClient`]
            follow the same pattern as `openai.OpenAI` client. Cannot be used if `token` is set. Defaults to None.
    provider: Optional[PROVIDER_OR_POLICY_T]
    @validate_hf_hub_args
        model: Optional[str] = None,
        provider: Optional[PROVIDER_OR_POLICY_T] = None,
        token: Optional[str] = None,
        timeout: Optional[float] = None,
        headers: Optional[dict[str, str]] = None,
        cookies: Optional[dict[str, str]] = None,
        bill_to: Optional[str] = None,
        # OpenAI compatibility
        base_url: Optional[str] = None,
        api_key: Optional[str] = None,
        if model is not None and base_url is not None:
                "Received both `model` and `base_url` arguments. Please provide only one of them."
                " `base_url` is an alias for `model` to make the API compatible with OpenAI's client."
                " If using `base_url` for chat completion, the `/chat/completions` suffix path will be appended to the base url."
                " When passing a URL as `model`, the client will not append any suffix path to it."
        if token is not None and api_key is not None:
                "Received both `token` and `api_key` arguments. Please provide only one of them."
                " `api_key` is an alias for `token` to make the API compatible with OpenAI's client."
                " It has the exact same behavior as `token`."
        token = token if token is not None else api_key
        if isinstance(token, bool):
            # Legacy behavior: previously it was possible to pass `token=False` to disable authentication. This is not
            # supported anymore as authentication is required. Better to explicitly raise here rather than risking
            # sending the locally saved token without the user knowing about it.
            if token is False:
                    "Cannot use `token=False` to disable authentication as authentication is required to run Inference."
                "Using `token=True` to automatically use the locally saved token is deprecated and will be removed in a future release. "
                "Please use `token=None` instead (default).",
        self.model: Optional[str] = base_url or model
        self.token: Optional[str] = token
        self.headers = {**headers} if headers is not None else {}
        if bill_to is not None:
                constants.HUGGINGFACE_HEADER_X_BILL_TO in self.headers
                and self.headers[constants.HUGGINGFACE_HEADER_X_BILL_TO] != bill_to
                    f"Overriding existing '{self.headers[constants.HUGGINGFACE_HEADER_X_BILL_TO]}' value in headers with '{bill_to}'.",
            self.headers[constants.HUGGINGFACE_HEADER_X_BILL_TO] = bill_to
            if token is not None and not token.startswith("hf_"):
                    "You've provided an external provider's API key, so requests will be billed directly by the provider. "
                    "The `bill_to` parameter is only applicable for Hugging Face billing and will be ignored.",
        # Configure provider
        self.provider = provider  # type: ignore[assignment]
        self.exit_stack = ExitStack()
        return f"<InferenceClient(model='{self.model if self.model else ''}', timeout={self.timeout})>"
        self.exit_stack.close()
    def _inner_post(  # type: ignore[misc]
        self, request_parameters: RequestParameters, *, stream: Literal[False] = ...
    ) -> bytes: ...
        self, request_parameters: RequestParameters, *, stream: Literal[True] = ...
    ) -> Iterable[str]: ...
    def _inner_post(
        self, request_parameters: RequestParameters, *, stream: bool = False
    ) -> Union[bytes, Iterable[str]]: ...
    ) -> Union[bytes, Iterable[str]]:
        """Make a request to the inference server."""
        # TODO: this should be handled in provider helpers directly
        if request_parameters.task in TASKS_EXPECTING_IMAGES and "Accept" not in request_parameters.headers:
            request_parameters.headers["Accept"] = "image/png"
            response = self.exit_stack.enter_context(
                get_session().stream(
                    request_parameters.url,
                    json=request_parameters.json,
                    content=request_parameters.data,
                    headers=request_parameters.headers,
                return response.iter_lines()
                return response.read()
        except TimeoutError as error:
            # Convert any `TimeoutError` to a `InferenceTimeoutError`
            raise InferenceTimeoutError(f"Inference call timed out: {request_parameters.url}") from error  # type: ignore
        except HfHubHTTPError as error:
            if error.response.status_code == 422 and request_parameters.task != "unknown":
                msg = str(error.args[0])
                if len(error.response.text) > 0:
                    msg += f"{os.linesep}{error.response.text}{os.linesep}"
                error.args = (msg,) + error.args[1:]
    def audio_classification(
        audio: ContentT,
        top_k: Optional[int] = None,
        function_to_apply: Optional["AudioClassificationOutputTransform"] = None,
    ) -> list[AudioClassificationOutputElement]:
        Perform audio classification on the provided audio content.
            audio (Union[str, Path, bytes, BinaryIO]):
                The audio content to classify. It can be raw audio bytes, a local audio file, or a URL pointing to an
                audio file.
            model (`str`, *optional*):
                The model to use for audio classification. Can be a model ID hosted on the Hugging Face Hub
                or a URL to a deployed Inference Endpoint. If not provided, the default recommended model for
                audio classification will be used.
            top_k (`int`, *optional*):
                When specified, limits the output to the top K most probable classes.
            function_to_apply (`"AudioClassificationOutputTransform"`, *optional*):
                The function to apply to the model outputs in order to retrieve the scores.
            `list[AudioClassificationOutputElement]`: List of [`AudioClassificationOutputElement`] items containing the predicted labels and their confidence.
            [`InferenceTimeoutError`]:
                If the model is unavailable or the request times out.
            [`HfHubHTTPError`]:
                If the request fails with an HTTP error status code other than HTTP 503.
        >>> from huggingface_hub import InferenceClient
        >>> client = InferenceClient()
        >>> client.audio_classification("audio.flac")
            AudioClassificationOutputElement(score=0.4976358711719513, label='hap'),
            AudioClassificationOutputElement(score=0.3677836060523987, label='neu'),
        model_id = model or self.model
        provider_helper = get_provider_helper(self.provider, task="audio-classification", model=model_id)
        request_parameters = provider_helper.prepare_request(
            inputs=audio,
            parameters={"function_to_apply": function_to_apply, "top_k": top_k},
            model=model_id,
            api_key=self.token,
        response = self._inner_post(request_parameters)
        return AudioClassificationOutputElement.parse_obj_as_list(response)
    def audio_to_audio(
    ) -> list[AudioToAudioOutputElement]:
        Performs multiple tasks related to audio-to-audio depending on the model (eg: speech enhancement, source separation).
                The audio content for the model. It can be raw audio bytes, a local audio file, or a URL pointing to an
                The model can be any model which takes an audio file and returns another audio file. Can be a model ID hosted on the Hugging Face Hub
                audio_to_audio will be used.
            `list[AudioToAudioOutputElement]`: A list of [`AudioToAudioOutputElement`] items containing audios label, content-type, and audio content in blob.
            `InferenceTimeoutError`:
        >>> audio_output = client.audio_to_audio("audio.flac")
        >>> for i, item in enumerate(audio_output):
        >>>     with open(f"output_{i}.flac", "wb") as f:
                    f.write(item.blob)
        provider_helper = get_provider_helper(self.provider, task="audio-to-audio", model=model_id)
            parameters={},
        audio_output = AudioToAudioOutputElement.parse_obj_as_list(response)
        for item in audio_output:
            item.blob = base64.b64decode(item.blob)
        return audio_output
    def automatic_speech_recognition(
        extra_body: Optional[dict] = None,
    ) -> AutomaticSpeechRecognitionOutput:
        Perform automatic speech recognition (ASR or audio-to-text) on the given audio content.
                The content to transcribe. It can be raw audio bytes, local audio file, or a URL to an audio file.
                The model to use for ASR. Can be a model ID hosted on the Hugging Face Hub or a URL to a deployed
                Inference Endpoint. If not provided, the default recommended model for ASR will be used.
            extra_body (`dict`, *optional*):
                Additional provider-specific parameters to pass to the model. Refer to the provider's documentation
                for supported parameters.
            [`AutomaticSpeechRecognitionOutput`]: An item containing the transcribed text and optionally the timestamp chunks.
        >>> client.automatic_speech_recognition("hello_world.flac").text
        "hello world"
        provider_helper = get_provider_helper(self.provider, task="automatic-speech-recognition", model=model_id)
            parameters={**(extra_body or {})},
        response = provider_helper.get_response(response, request_params=request_parameters)
        return AutomaticSpeechRecognitionOutput.parse_obj_as_instance(response)
    def chat_completion(  # type: ignore
        messages: list[Union[dict, ChatCompletionInputMessage]],
        frequency_penalty: Optional[float] = None,
        logit_bias: Optional[list[float]] = None,
        logprobs: Optional[bool] = None,
        max_tokens: Optional[int] = None,
        n: Optional[int] = None,
        presence_penalty: Optional[float] = None,
        response_format: Optional[ChatCompletionInputGrammarType] = None,
        seed: Optional[int] = None,
        stop: Optional[list[str]] = None,
        stream_options: Optional[ChatCompletionInputStreamOptions] = None,
        temperature: Optional[float] = None,
        tool_choice: Optional[Union[ChatCompletionInputToolChoiceClass, "ChatCompletionInputToolChoiceEnum"]] = None,
        tool_prompt: Optional[str] = None,
        tools: Optional[list[ChatCompletionInputTool]] = None,
        top_logprobs: Optional[int] = None,
        top_p: Optional[float] = None,
    ) -> ChatCompletionOutput: ...
        stream: Literal[True] = True,
    ) -> Iterable[ChatCompletionStreamOutput]: ...
    def chat_completion(
    ) -> Union[ChatCompletionOutput, Iterable[ChatCompletionStreamOutput]]: ...
        # Parameters from ChatCompletionInput (handled manually)
    ) -> Union[ChatCompletionOutput, Iterable[ChatCompletionStreamOutput]]:
        A method for completing conversations using a specified language model.
        > [!TIP]
        > The `client.chat_completion` method is aliased as `client.chat.completions.create` for compatibility with OpenAI's client.
        > Inputs and outputs are strictly the same and using either syntax will yield the same results.
        > Check out the [Inference guide](https://huggingface.co/docs/huggingface_hub/guides/inference#openai-compatibility)
        > for more details about OpenAI's compatibility.
        > You can pass provider-specific parameters to the model by using the `extra_body` argument.
            messages (List of [`ChatCompletionInputMessage`]):
                Conversation history consisting of roles and content pairs.
                The model to use for chat-completion. Can be a model ID hosted on the Hugging Face Hub or a URL to a deployed
                Inference Endpoint. If not provided, the default recommended model for chat-based text-generation will be used.
                See https://huggingface.co/tasks/text-generation for more details.
                If `model` is a model ID, it is passed to the server as the `model` parameter. If you want to define a
                custom URL while setting `model` in the request payload, you must set `base_url` when initializing [`InferenceClient`].
            frequency_penalty (`float`, *optional*):
                Penalizes new tokens based on their existing frequency
                in the text so far. Range: [-2.0, 2.0]. Defaults to 0.0.
            logit_bias (`list[float]`, *optional*):
                Adjusts the likelihood of specific tokens appearing in the generated output.
            logprobs (`bool`, *optional*):
                Whether to return log probabilities of the output tokens or not. If true, returns the log
                probabilities of each output token returned in the content of message.
            max_tokens (`int`, *optional*):
                Maximum number of tokens allowed in the response. Defaults to 100.
            n (`int`, *optional*):
                The number of completions to generate for each prompt.
            presence_penalty (`float`, *optional*):
                Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the
                text so far, increasing the model's likelihood to talk about new topics.
            response_format ([`ChatCompletionInputGrammarType`], *optional*):
                Grammar constraints. Can be either a JSONSchema or a regex.
            seed (Optional[`int`], *optional*):
                Seed for reproducible control flow. Defaults to None.
            stop (`list[str]`, *optional*):
                Up to four strings which trigger the end of the response.
            stream (`bool`, *optional*):
                Enable realtime streaming of responses. Defaults to False.
            stream_options ([`ChatCompletionInputStreamOptions`], *optional*):
                Options for streaming completions.
            temperature (`float`, *optional*):
                Controls randomness of the generations. Lower values ensure
                less random completions. Range: [0, 2]. Defaults to 1.0.
            top_logprobs (`int`, *optional*):
                An integer between 0 and 5 specifying the number of most likely tokens to return at each token
                position, each with an associated log probability. logprobs must be set to true if this parameter is
            top_p (`float`, *optional*):
                Fraction of the most likely next words to sample from.
                Must be between 0 and 1. Defaults to 1.0.
            tool_choice ([`ChatCompletionInputToolChoiceClass`] or [`ChatCompletionInputToolChoiceEnum`], *optional*):
                The tool to use for the completion. Defaults to "auto".
            tool_prompt (`str`, *optional*):
                A prompt to be appended before the tools.
            tools (List of [`ChatCompletionInputTool`], *optional*):
                A list of tools the model may call. Currently, only functions are supported as a tool. Use this to
                provide a list of functions the model may generate JSON inputs for.
            [`ChatCompletionOutput`] or Iterable of [`ChatCompletionStreamOutput`]:
            Generated text returned from the server:
            - if `stream=False`, the generated text is returned as a [`ChatCompletionOutput`] (default).
            - if `stream=True`, the generated text is returned token by token as a sequence of [`ChatCompletionStreamOutput`].
        >>> messages = [{"role": "user", "content": "What is the capital of France?"}]
        >>> client = InferenceClient("meta-llama/Meta-Llama-3-8B-Instruct")
        >>> client.chat_completion(messages, max_tokens=100)
        ChatCompletionOutput(
                ChatCompletionOutputComplete(
                    finish_reason='eos_token',
                    message=ChatCompletionOutputMessage(
                        content='The capital of France is Paris.',
                    logprobs=None
            created=1719907176,
            id='',
            model='meta-llama/Meta-Llama-3-8B-Instruct',
            object='text_completion',
            system_fingerprint='2.0.4-sha-f426a33',
            usage=ChatCompletionOutputUsage(
                completion_tokens=8,
                prompt_tokens=17,
                total_tokens=25
        Example using streaming:
        >>> for token in client.chat_completion(messages, max_tokens=10, stream=True):
        ...     print(token)
        ChatCompletionStreamOutput(choices=[ChatCompletionStreamOutputChoice(delta=ChatCompletionStreamOutputDelta(content='The', role='assistant'), index=0, finish_reason=None)], created=1710498504)
        ChatCompletionStreamOutput(choices=[ChatCompletionStreamOutputChoice(delta=ChatCompletionStreamOutputDelta(content=' capital', role='assistant'), index=0, finish_reason=None)], created=1710498504)
        ChatCompletionStreamOutput(choices=[ChatCompletionStreamOutputChoice(delta=ChatCompletionStreamOutputDelta(content=' may', role='assistant'), index=0, finish_reason=None)], created=1710498504)
        Example using OpenAI's syntax:
        # instead of `from openai import OpenAI`
        from huggingface_hub import InferenceClient
        # instead of `client = OpenAI(...)`
        client = InferenceClient(
            base_url=...,
            api_key=...,
        output = client.chat.completions.create(
            model="meta-llama/Meta-Llama-3-8B-Instruct",
                {"role": "user", "content": "Count to 10"},
            max_tokens=1024,
        for chunk in output:
            print(chunk.choices[0].delta.content)
        Example using a third-party provider directly with extra (provider-specific) parameters. Usage will be billed on your Together AI account.
        >>> client = InferenceClient(
        ...     provider="together",  # Use Together AI provider
        ...     api_key="<together_api_key>",  # Pass your Together API key directly
        >>> client.chat_completion(
        ...     model="meta-llama/Meta-Llama-3-8B-Instruct",
        ...     messages=[{"role": "user", "content": "What is the capital of France?"}],
        ...     extra_body={"safety_model": "Meta-Llama/Llama-Guard-7b"},
        Example using a third-party provider through Hugging Face Routing. Usage will be billed on your Hugging Face account.
        ...     provider="sambanova",  # Use Sambanova provider
        ...     api_key="hf_...",  # Pass your HF token
        Example using Image + Text as input:
        # provide a remote URL
        >>> image_url ="https://cdn.britannica.com/61/93061-050-99147DCE/Statue-of-Liberty-Island-New-York-Bay.jpg"
        # or a base64-encoded image
        >>> image_path = "/path/to/image.jpeg"
        >>> with open(image_path, "rb") as f:
        ...     base64_image = base64.b64encode(f.read()).decode("utf-8")
        >>> image_url = f"data:image/jpeg;base64,{base64_image}"
        >>> client = InferenceClient("meta-llama/Llama-3.2-11B-Vision-Instruct")
        >>> output = client.chat.completions.create(
        ...     messages=[
        ...         {
        ...             "role": "user",
        ...             "content": [
        ...                 {
        ...                     "type": "image_url",
        ...                     "image_url": {"url": image_url},
        ...                 },
        ...                     "type": "text",
        ...                     "text": "Describe this image in one sentence.",
        ...             ],
        ...     ],
        >>> output
        The image depicts the iconic Statue of Liberty situated in New York Harbor, New York, on a clear day.
        Example using tools:
        >>> client = InferenceClient("meta-llama/Meta-Llama-3-70B-Instruct")
        >>> messages = [
        ...     {
        ...         "role": "system",
        ...         "content": "Don't make assumptions about what values to plug into functions. Ask for clarification if a user request is ambiguous.",
        ...         "role": "user",
        ...         "content": "What's the weather like the next 3 days in San Francisco, CA?",
        >>> tools = [
        ...         "type": "function",
        ...         "function": {
        ...             "name": "get_current_weather",
        ...             "description": "Get the current weather",
        ...             "parameters": {
        ...                 "type": "object",
        ...                 "properties": {
        ...                     "location": {
        ...                         "type": "string",
        ...                         "description": "The city and state, e.g. San Francisco, CA",
        ...                     },
        ...                     "format": {
        ...                         "enum": ["celsius", "fahrenheit"],
        ...                         "description": "The temperature unit to use. Infer this from the users location.",
        ...                 "required": ["location", "format"],
        ...             },
        ...             "name": "get_n_day_weather_forecast",
        ...             "description": "Get an N-day weather forecast",
        ...                     "num_days": {
        ...                         "type": "integer",
        ...                         "description": "The number of days to forecast",
        ...                 "required": ["location", "format", "num_days"],
        >>> response = client.chat_completion(
        ...     model="meta-llama/Meta-Llama-3-70B-Instruct",
        ...     messages=messages,
        ...     tools=tools,
        ...     tool_choice="auto",
        ...     max_tokens=500,
        >>> response.choices[0].message.tool_calls[0].function
        ChatCompletionOutputFunctionDefinition(
            arguments={
                'location': 'San Francisco, CA',
                'format': 'fahrenheit',
                'num_days': 3
            name='get_n_day_weather_forecast',
            description=None
        Example using response_format:
        ...         "content": "I saw a puppy a cat and a raccoon during my bike ride in the park. What did I see and when?",
        >>> response_format = {
        ...     "type": "json",
        ...     "value": {
        ...         "properties": {
        ...             "location": {"type": "string"},
        ...             "activity": {"type": "string"},
        ...             "animals_seen": {"type": "integer", "minimum": 1, "maximum": 5},
        ...             "animals": {"type": "array", "items": {"type": "string"}},
        ...         "required": ["location", "activity", "animals_seen", "animals"],
        ...     response_format=response_format,
        >>> response.choices[0].message.content
        '{\n\n"activity": "bike ride",\n"animals": ["puppy", "cat", "raccoon"],\n"animals_seen": 3,\n"location": "park"}'
        # Since `chat_completion(..., model=xxx)` is also a payload parameter for the server, we need to handle 'model' differently.
        # `self.model` takes precedence over 'model' argument for building URL.
        # `model` takes precedence for payload value.
        model_id_or_url = self.model or model
        payload_model = model or self.model
        # Get the provider helper
        provider_helper = get_provider_helper(
            self.provider,
            task="conversational",
            model=model_id_or_url
            if model_id_or_url is not None and model_id_or_url.startswith(("http://", "https://"))
            else payload_model,
        # Prepare the payload
        parameters = {
            "model": payload_model,
            "tool_prompt": tool_prompt,
            **(extra_body or {}),
            inputs=messages,
            parameters=parameters,
            model=model_id_or_url,
        data = self._inner_post(request_parameters, stream=stream)
            return _stream_chat_completion_response(data)  # type: ignore[arg-type]
        return ChatCompletionOutput.parse_obj_as_instance(data)  # type: ignore[arg-type]
    def document_question_answering(
        image: ContentT,
        doc_stride: Optional[int] = None,
        handle_impossible_answer: Optional[bool] = None,
        lang: Optional[str] = None,
        max_answer_len: Optional[int] = None,
        max_question_len: Optional[int] = None,
        max_seq_len: Optional[int] = None,
        word_boxes: Optional[list[Union[list[float], str]]] = None,
    ) -> list[DocumentQuestionAnsweringOutputElement]:
        Answer questions on document images.
            image (`Union[str, Path, bytes, BinaryIO]`):
                The input image for the context. It can be raw bytes, an image file, or a URL to an online image.
            question (`str`):
                Question to be answered.
                The model to use for the document question answering task. Can be a model ID hosted on the Hugging Face Hub or a URL to
                a deployed Inference Endpoint. If not provided, the default recommended document question answering model will be used.
            doc_stride (`int`, *optional*):
                If the words in the document are too long to fit with the question for the model, it will be split in
                several chunks with some overlap. This argument controls the size of that overlap.
            handle_impossible_answer (`bool`, *optional*):
                Whether to accept impossible as an answer
            lang (`str`, *optional*):
                Language to use while running OCR. Defaults to english.
            max_answer_len (`int`, *optional*):
                The maximum length of predicted answers (e.g., only answers with a shorter length are considered).
            max_question_len (`int`, *optional*):
                The maximum length of the question after tokenization. It will be truncated if needed.
            max_seq_len (`int`, *optional*):
                The maximum length of the total sentence (context + question) in tokens of each chunk passed to the
                model. The context will be split in several chunks (using doc_stride as overlap) if needed.
                The number of answers to return (will be chosen by order of likelihood). Can return less than top_k
                answers if there are not enough options available within the context.
            word_boxes (`list[Union[list[float], str`, *optional*):
                A list of words and bounding boxes (normalized 0->1000). If provided, the inference will skip the OCR
                step and use the provided bounding boxes instead.
            `list[DocumentQuestionAnsweringOutputElement]`: a list of [`DocumentQuestionAnsweringOutputElement`] items containing the predicted label, associated probability, word ids, and page number.
        >>> client.document_question_answering(image="https://huggingface.co/spaces/impira/docquery/resolve/2359223c1837a7587402bda0f2643382a6eefeab/invoice.png", question="What is the invoice number?")
        [DocumentQuestionAnsweringOutputElement(answer='us-001', end=16, score=0.9999666213989258, start=16)]
        provider_helper = get_provider_helper(self.provider, task="document-question-answering", model=model_id)
        inputs: dict[str, Any] = {"question": question, "image": _b64_encode(image)}
            parameters={
                "doc_stride": doc_stride,
                "handle_impossible_answer": handle_impossible_answer,
                "lang": lang,
                "max_answer_len": max_answer_len,
                "max_question_len": max_question_len,
                "max_seq_len": max_seq_len,
                "top_k": top_k,
                "word_boxes": word_boxes,
        return DocumentQuestionAnsweringOutputElement.parse_obj_as_list(response)
    def feature_extraction(
        normalize: Optional[bool] = None,
        prompt_name: Optional[str] = None,
        truncate: Optional[bool] = None,
        truncation_direction: Optional[Literal["left", "right"]] = None,
        dimensions: Optional[int] = None,
        encoding_format: Optional[Literal["float", "base64"]] = None,
    ) -> "np.ndarray":
        Generate embeddings for a given text.
            text (`str`):
                The text to embed.
                The model to use for the feature extraction task. Can be a model ID hosted on the Hugging Face Hub or a URL to
                a deployed Inference Endpoint. If not provided, the default recommended feature extraction model will be used.
            normalize (`bool`, *optional*):
                Whether to normalize the embeddings or not.
                Only available on server powered by Text-Embedding-Inference.
            prompt_name (`str`, *optional*):
                The name of the prompt that should be used by for encoding. If not set, no prompt will be applied.
                Must be a key in the `Sentence Transformers` configuration `prompts` dictionary.
                For example if ``prompt_name`` is "query" and the ``prompts`` is {"query": "query: ",...},
                then the sentence "What is the capital of France?" will be encoded as "query: What is the capital of France?"
                because the prompt text will be prepended before any text to encode.
            truncate (`bool`, *optional*):
                Whether to truncate the embeddings or not.
            truncation_direction (`Literal["left", "right"]`, *optional*):
                Which side of the input should be truncated when `truncate=True` is passed.
            dimensions (`int`, *optional*):
                Only available on OpenAI-compatible embedding endpoints.
            encoding_format (`Literal["float", "base64"]`, *optional*):
                The format of the output embeddings. Either "float" or "base64".
            `np.ndarray`: The embedding representing the input text as a float32 numpy array.
        >>> client.feature_extraction("Hi, who are you?")
        array([[ 2.424802  ,  2.93384   ,  1.1750331 , ...,  1.240499, -0.13776633, -0.7889173 ],
        [-0.42943227, -0.6364878 , -1.693462  , ...,  0.41978157, -2.4336355 ,  0.6162071 ],
        [ 0.28552425, -0.928395  , -1.2077185 , ...,  0.76810825, -2.1069427 ,  0.6236161 ]], dtype=float32)
        provider_helper = get_provider_helper(self.provider, task="feature-extraction", model=model_id)
            inputs=text,
                "normalize": normalize,
                "prompt_name": prompt_name,
                "truncate": truncate,
                "truncation_direction": truncation_direction,
        return np.array(provider_helper.get_response(response), dtype="float32")
    def fill_mask(
        targets: Optional[list[str]] = None,
    ) -> list[FillMaskOutputElement]:
        Fill in a hole with a missing word (token to be precise).
                a string to be filled from, must contain the [MASK] token (check model card for exact name of the mask).
                The model to use for the fill mask task. Can be a model ID hosted on the Hugging Face Hub or a URL to
                a deployed Inference Endpoint. If not provided, the default recommended fill mask model will be used.
            targets (`list[str`, *optional*):
                When passed, the model will limit the scores to the passed targets instead of looking up in the whole
                vocabulary. If the provided targets are not in the model vocab, they will be tokenized and the first
                resulting token will be used (with a warning, and that might be slower).
                When passed, overrides the number of predictions to return.
            `list[FillMaskOutputElement]`: a list of [`FillMaskOutputElement`] items containing the predicted label, associated
            probability, token reference, and completed text.
        >>> client.fill_mask("The goal of life is <mask>.")
            FillMaskOutputElement(score=0.06897063553333282, token=11098, token_str=' happiness', sequence='The goal of life is happiness.'),
            FillMaskOutputElement(score=0.06554922461509705, token=45075, token_str=' immortality', sequence='The goal of life is immortality.')
        provider_helper = get_provider_helper(self.provider, task="fill-mask", model=model_id)
            parameters={"targets": targets, "top_k": top_k},
        return FillMaskOutputElement.parse_obj_as_list(response)
    def image_classification(
        function_to_apply: Optional["ImageClassificationOutputTransform"] = None,
    ) -> list[ImageClassificationOutputElement]:
        Perform image classification on the given image using the specified model.
            image (`Union[str, Path, bytes, BinaryIO, PIL.Image.Image]`):
                The image to classify. It can be raw bytes, an image file, a URL to an online image, or a PIL Image.
                The model to use for image classification. Can be a model ID hosted on the Hugging Face Hub or a URL to a
                deployed Inference Endpoint. If not provided, the default recommended model for image classification will be used.
            function_to_apply (`"ImageClassificationOutputTransform"`, *optional*):
            `list[ImageClassificationOutputElement]`: a list of [`ImageClassificationOutputElement`] items containing the predicted label and associated probability.
        >>> client.image_classification("https://upload.wikimedia.org/wikipedia/commons/thumb/4/43/Cute_dog.jpg/320px-Cute_dog.jpg")
        [ImageClassificationOutputElement(label='Blenheim spaniel', score=0.9779096841812134), ...]
        provider_helper = get_provider_helper(self.provider, task="image-classification", model=model_id)
            inputs=image,
        return ImageClassificationOutputElement.parse_obj_as_list(response)
    def image_segmentation(
        mask_threshold: Optional[float] = None,
        overlap_mask_area_threshold: Optional[float] = None,
        subtask: Optional["ImageSegmentationSubtask"] = None,
        threshold: Optional[float] = None,
    ) -> list[ImageSegmentationOutputElement]:
        Perform image segmentation on the given image using the specified model.
        > [!WARNING]
        > You must have `PIL` installed if you want to work with images (`pip install Pillow`).
                The image to segment. It can be raw bytes, an image file, a URL to an online image, or a PIL Image.
                The model to use for image segmentation. Can be a model ID hosted on the Hugging Face Hub or a URL to a
                deployed Inference Endpoint. If not provided, the default recommended model for image segmentation will be used.
            mask_threshold (`float`, *optional*):
                Threshold to use when turning the predicted masks into binary values.
            overlap_mask_area_threshold (`float`, *optional*):
                Mask overlap threshold to eliminate small, disconnected segments.
            subtask (`"ImageSegmentationSubtask"`, *optional*):
                Segmentation task to be performed, depending on model capabilities.
            threshold (`float`, *optional*):
                Probability threshold to filter out predicted masks.
            `list[ImageSegmentationOutputElement]`: A list of [`ImageSegmentationOutputElement`] items containing the segmented masks and associated attributes.
        >>> client.image_segmentation("cat.jpg")
        [ImageSegmentationOutputElement(score=0.989008, label='LABEL_184', mask=<PIL.PngImagePlugin.PngImageFile image mode=L size=400x300 at 0x7FDD2B129CC0>), ...]
        provider_helper = get_provider_helper(self.provider, task="image-segmentation", model=model_id)
                "mask_threshold": mask_threshold,
                "overlap_mask_area_threshold": overlap_mask_area_threshold,
                "subtask": subtask,
                "threshold": threshold,
        response = provider_helper.get_response(response, request_parameters)
        output = ImageSegmentationOutputElement.parse_obj_as_list(response)
        for item in output:
            item.mask = _b64_to_image(item.mask)  # type: ignore [assignment]
    def image_to_image(
        prompt: Optional[str] = None,
        negative_prompt: Optional[str] = None,
        num_inference_steps: Optional[int] = None,
        guidance_scale: Optional[float] = None,
        target_size: Optional[ImageToImageTargetSize] = None,
    ) -> "Image":
        Perform image-to-image translation using a specified model.
                The input image for translation. It can be raw bytes, an image file, a URL to an online image, or a PIL Image.
            prompt (`str`, *optional*):
                The text prompt to guide the image generation.
            negative_prompt (`str`, *optional*):
                One prompt to guide what NOT to include in image generation.
            num_inference_steps (`int`, *optional*):
                For diffusion models. The number of denoising steps. More denoising steps usually lead to a higher
                quality image at the expense of slower inference.
            guidance_scale (`float`, *optional*):
                For diffusion models. A higher guidance scale value encourages the model to generate images closely
                linked to the text prompt at the expense of lower image quality.
                The model to use for inference. Can be a model ID hosted on the Hugging Face Hub or a URL to a deployed
                Inference Endpoint. This parameter overrides the model defined at the instance level. Defaults to None.
            target_size (`ImageToImageTargetSize`, *optional*):
                The size in pixels of the output image. This parameter is only supported by some providers and for
                specific models. It will be ignored when unsupported.
            `Image`: The translated image.
        >>> image = client.image_to_image("cat.jpg", prompt="turn the cat into a tiger")
        >>> image.save("tiger.jpg")
        provider_helper = get_provider_helper(self.provider, task="image-to-image", model=model_id)
                "negative_prompt": negative_prompt,
                "target_size": target_size,
                "num_inference_steps": num_inference_steps,
                "guidance_scale": guidance_scale,
        return _bytes_to_image(response)
    def image_to_video(
        num_frames: Optional[float] = None,
        target_size: Optional[ImageToVideoTargetSize] = None,
        Generate a video from an input image.
                The input image to generate a video from. It can be raw bytes, an image file, a URL to an online image, or a PIL Image.
                The text prompt to guide the video generation.
                One prompt to guide what NOT to include in video generation.
            num_frames (`float`, *optional*):
                The num_frames parameter determines how many video frames are generated.
                For diffusion models. A higher guidance scale value encourages the model to generate videos closely
            seed (`int`, *optional*):
                The seed to use for the video generation.
            target_size (`ImageToVideoTargetSize`, *optional*):
                The size in pixel of the output video frames.
                The number of denoising steps. More denoising steps usually lead to a higher quality video at the
                expense of slower inference.
                Seed for the random number generator.
            `bytes`: The generated video.
        >>> video = client.image_to_video("cat.jpg", model="Wan-AI/Wan2.2-I2V-A14B", prompt="turn the cat into a tiger")
        >>> with open("tiger.mp4", "wb") as f:
        ...     f.write(video)
        provider_helper = get_provider_helper(self.provider, task="image-to-video", model=model_id)
                "num_frames": num_frames,
    def image_to_text(self, image: ContentT, *, model: Optional[str] = None) -> ImageToTextOutput:
        Takes an input image and return text.
        Models can have very different outputs depending on your use case (image captioning, optical character recognition
        (OCR), Pix2Struct, etc.). Please have a look to the model card to learn more about a model's specificities.
                The input image to caption. It can be raw bytes, an image file, a URL to an online image, or a PIL Image.
            [`ImageToTextOutput`]: The generated text.
        >>> client.image_to_text("cat.jpg")
        'a cat standing in a grassy field '
        >>> client.image_to_text("https://upload.wikimedia.org/wikipedia/commons/thumb/4/43/Cute_dog.jpg/320px-Cute_dog.jpg")
        'a dog laying on the grass next to a flower pot '
        provider_helper = get_provider_helper(self.provider, task="image-to-text", model=model_id)
        output_list: list[ImageToTextOutput] = ImageToTextOutput.parse_obj_as_list(response)
        return output_list[0]
    def object_detection(
        self, image: ContentT, *, model: Optional[str] = None, threshold: Optional[float] = None
    ) -> list[ObjectDetectionOutputElement]:
        Perform object detection on the given image using the specified model.
                The image to detect objects on. It can be raw bytes, an image file, a URL to an online image, or a PIL Image.
                The model to use for object detection. Can be a model ID hosted on the Hugging Face Hub or a URL to a
                deployed Inference Endpoint. If not provided, the default recommended model for object detection (DETR) will be used.
                The probability necessary to make a prediction.
            `list[ObjectDetectionOutputElement]`: A list of [`ObjectDetectionOutputElement`] items containing the bounding boxes and associated attributes.
            `ValueError`:
                If the request output is not a List.
        >>> client.object_detection("people.jpg")
        [ObjectDetectionOutputElement(score=0.9486683011054993, label='person', box=ObjectDetectionBoundingBox(xmin=59, ymin=39, xmax=420, ymax=510)), ...]
        provider_helper = get_provider_helper(self.provider, task="object-detection", model=model_id)
            parameters={"threshold": threshold},
        return ObjectDetectionOutputElement.parse_obj_as_list(response)
    def question_answering(
        context: str,
        align_to_words: Optional[bool] = None,
    ) -> Union[QuestionAnsweringOutputElement, list[QuestionAnsweringOutputElement]]:
        Retrieve the answer to a question from a given text.
            context (`str`):
                The context of the question.
            model (`str`):
                The model to use for the question answering task. Can be a model ID hosted on the Hugging Face Hub or a URL to
                a deployed Inference Endpoint.
            align_to_words (`bool`, *optional*):
                Attempts to align the answer to real words. Improves quality on space separated languages. Might hurt
                on non-space-separated languages (like Japanese or Chinese)
                If the context is too long to fit with the question for the model, it will be split in several chunks
                with some overlap. This argument controls the size of that overlap.
                Whether to accept impossible as an answer.
                model. The context will be split in several chunks (using docStride as overlap) if needed.
                The number of answers to return (will be chosen by order of likelihood). Note that we return less than
                topk answers if there are not enough options available within the context.
            Union[`QuestionAnsweringOutputElement`, list[`QuestionAnsweringOutputElement`]]:
                When top_k is 1 or not provided, it returns a single `QuestionAnsweringOutputElement`.
                When top_k is greater than 1, it returns a list of `QuestionAnsweringOutputElement`.
        >>> client.question_answering(question="What's my name?", context="My name is Clara and I live in Berkeley.")
        QuestionAnsweringOutputElement(answer='Clara', end=16, score=0.9326565265655518, start=11)
        provider_helper = get_provider_helper(self.provider, task="question-answering", model=model_id)
            inputs={"question": question, "context": context},
                "align_to_words": align_to_words,
        # Parse the response as a single `QuestionAnsweringOutputElement` when top_k is 1 or not provided, or a list of `QuestionAnsweringOutputElement` to ensure backward compatibility.
        output = QuestionAnsweringOutputElement.parse_obj(response)
    def sentence_similarity(
        self, sentence: str, other_sentences: list[str], *, model: Optional[str] = None
    ) -> list[float]:
        Compute the semantic similarity between a sentence and a list of other sentences by comparing their embeddings.
            sentence (`str`):
                The main sentence to compare to others.
            other_sentences (`list[str]`):
                The list of sentences to compare to.
                The model to use for the sentence similarity task. Can be a model ID hosted on the Hugging Face Hub or a URL to
                a deployed Inference Endpoint. If not provided, the default recommended sentence similarity model will be used.
            `list[float]`: The embedding representing the input text.
        >>> client.sentence_similarity(
        ...     "Machine learning is so easy.",
        ...     other_sentences=[
        ...         "Deep learning is so straightforward.",
        ...         "This is so difficult, like rocket science.",
        ...         "I can't believe how much I struggled with this.",
        [0.7785726189613342, 0.45876261591911316, 0.2906220555305481]
        provider_helper = get_provider_helper(self.provider, task="sentence-similarity", model=model_id)
            inputs={"source_sentence": sentence, "sentences": other_sentences},
            extra_payload={},
        return _bytes_to_list(response)
    def summarization(
        clean_up_tokenization_spaces: Optional[bool] = None,
        generate_parameters: Optional[dict[str, Any]] = None,
        truncation: Optional["SummarizationTruncationStrategy"] = None,
    ) -> SummarizationOutput:
        Generate a summary of a given text using a specified model.
                The input text to summarize.
                Inference Endpoint. If not provided, the default recommended model for summarization will be used.
            clean_up_tokenization_spaces (`bool`, *optional*):
                Whether to clean up the potential extra spaces in the text output.
            generate_parameters (`dict[str, Any]`, *optional*):
                Additional parametrization of the text generation algorithm.
            truncation (`"SummarizationTruncationStrategy"`, *optional*):
                The truncation strategy to use.
            [`SummarizationOutput`]: The generated summary text.
        >>> client.summarization("The Eiffel tower...")
        SummarizationOutput(generated_text="The Eiffel tower is one of the most famous landmarks in the world....")
            "clean_up_tokenization_spaces": clean_up_tokenization_spaces,
            "generate_parameters": generate_parameters,
        provider_helper = get_provider_helper(self.provider, task="summarization", model=model_id)
        return SummarizationOutput.parse_obj_as_list(response)[0]
    def table_question_answering(
        table: dict[str, Any],
        padding: Optional["Padding"] = None,
        sequential: Optional[bool] = None,
        truncation: Optional[bool] = None,
    ) -> TableQuestionAnsweringOutputElement:
        Retrieve the answer to a question from information given in a table.
            table (`str`):
                A table of data represented as a dict of lists where entries are headers and the lists are all the
                values, all lists must have the same size.
            query (`str`):
                The query in plain text that you want to ask the table.
                The model to use for the table-question-answering task. Can be a model ID hosted on the Hugging Face
                Hub or a URL to a deployed Inference Endpoint.
            padding (`"Padding"`, *optional*):
                Activates and controls padding.
            sequential (`bool`, *optional*):
                Whether to do inference sequentially or as a batch. Batching is faster, but models like SQA require the
                inference to be done sequentially to extract relations within sequences, given their conversational
                nature.
            truncation (`bool`, *optional*):
                Activates and controls truncation.
            [`TableQuestionAnsweringOutputElement`]: a table question answering output containing the answer, coordinates, cells and the aggregator used.
        >>> query = "How many stars does the transformers repository have?"
        >>> table = {"Repository": ["Transformers", "Datasets", "Tokenizers"], "Stars": ["36542", "4512", "3934"]}
        >>> client.table_question_answering(table, query, model="google/tapas-base-finetuned-wtq")
        TableQuestionAnsweringOutputElement(answer='36542', coordinates=[[0, 1]], cells=['36542'], aggregator='AVERAGE')
        provider_helper = get_provider_helper(self.provider, task="table-question-answering", model=model_id)
            inputs={"query": query, "table": table},
            parameters={"model": model, "padding": padding, "sequential": sequential, "truncation": truncation},
        return TableQuestionAnsweringOutputElement.parse_obj_as_instance(response)
    def tabular_classification(self, table: dict[str, Any], *, model: Optional[str] = None) -> list[str]:
        Classifying a target category (a group) based on a set of attributes.
            table (`dict[str, Any]`):
                Set of attributes to classify.
                The model to use for the tabular classification task. Can be a model ID hosted on the Hugging Face Hub or a URL to
                a deployed Inference Endpoint. If not provided, the default recommended tabular classification model will be used.
            `List`: a list of labels, one per row in the initial table.
        >>> table = {
        ...     "fixed_acidity": ["7.4", "7.8", "10.3"],
        ...     "volatile_acidity": ["0.7", "0.88", "0.32"],
        ...     "citric_acid": ["0", "0", "0.45"],
        ...     "residual_sugar": ["1.9", "2.6", "6.4"],
        ...     "chlorides": ["0.076", "0.098", "0.073"],
        ...     "free_sulfur_dioxide": ["11", "25", "5"],
        ...     "total_sulfur_dioxide": ["34", "67", "13"],
        ...     "density": ["0.9978", "0.9968", "0.9976"],
        ...     "pH": ["3.51", "3.2", "3.23"],
        ...     "sulphates": ["0.56", "0.68", "0.82"],
        ...     "alcohol": ["9.4", "9.8", "12.6"],
        >>> client.tabular_classification(table=table, model="julien-c/wine-quality")
        ["5", "5", "5"]
        provider_helper = get_provider_helper(self.provider, task="tabular-classification", model=model_id)
            inputs=None,
            extra_payload={"table": table},
    def tabular_regression(self, table: dict[str, Any], *, model: Optional[str] = None) -> list[float]:
        Predicting a numerical target value given a set of attributes/features in a table.
                Set of attributes stored in a table. The attributes used to predict the target can be both numerical and categorical.
                The model to use for the tabular regression task. Can be a model ID hosted on the Hugging Face Hub or a URL to
                a deployed Inference Endpoint. If not provided, the default recommended tabular regression model will be used.
            `List`: a list of predicted numerical target values.
        ...     "Height": ["11.52", "12.48", "12.3778"],
        ...     "Length1": ["23.2", "24", "23.9"],
        ...     "Length2": ["25.4", "26.3", "26.5"],
        ...     "Length3": ["30", "31.2", "31.1"],
        ...     "Species": ["Bream", "Bream", "Bream"],
        ...     "Width": ["4.02", "4.3056", "4.6961"],
        >>> client.tabular_regression(table, model="scikit-learn/Fish-Weight")
        [110, 120, 130]
        provider_helper = get_provider_helper(self.provider, task="tabular-regression", model=model_id)
    def text_classification(
        function_to_apply: Optional["TextClassificationOutputTransform"] = None,
    ) -> list[TextClassificationOutputElement]:
        Perform text classification (e.g. sentiment-analysis) on the given text.
                A string to be classified.
                The model to use for the text classification task. Can be a model ID hosted on the Hugging Face Hub or a URL to
                a deployed Inference Endpoint. If not provided, the default recommended text classification model will be used.
            function_to_apply (`"TextClassificationOutputTransform"`, *optional*):
            `list[TextClassificationOutputElement]`: a list of [`TextClassificationOutputElement`] items containing the predicted label and associated probability.
        >>> client.text_classification("I like you")
            TextClassificationOutputElement(label='POSITIVE', score=0.9998695850372314),
            TextClassificationOutputElement(label='NEGATIVE', score=0.0001304351753788069),
        provider_helper = get_provider_helper(self.provider, task="text-classification", model=model_id)
                "function_to_apply": function_to_apply,
        return TextClassificationOutputElement.parse_obj_as_list(response)[0]  # type: ignore [return-value]
    def text_generation(
        details: Literal[True],
        # Parameters from `TextGenerationInputGenerateParameters` (maintained manually)
        adapter_id: Optional[str] = None,
        best_of: Optional[int] = None,
        decoder_input_details: Optional[bool] = None,
        do_sample: Optional[bool] = None,
        grammar: Optional[TextGenerationInputGrammarType] = None,
        max_new_tokens: Optional[int] = None,
        repetition_penalty: Optional[float] = None,
        return_full_text: Optional[bool] = None,
        stop_sequences: Optional[list[str]] = None,  # Deprecated, use `stop` instead
        top_n_tokens: Optional[int] = None,
        truncate: Optional[int] = None,
        typical_p: Optional[float] = None,
        watermark: Optional[bool] = None,
    ) -> Iterable[TextGenerationStreamOutput]: ...
        stream: Optional[Literal[False]] = None,
    ) -> TextGenerationOutput: ...
        details: Optional[Literal[False]] = None,
        return_full_text: Optional[bool] = None,  # Manual default value
        details: Optional[bool] = None,
        stream: Optional[bool] = None,
    ) -> Union[str, TextGenerationOutput, Iterable[str], Iterable[TextGenerationStreamOutput]]: ...
    ) -> Union[str, TextGenerationOutput, Iterable[str], Iterable[TextGenerationStreamOutput]]:
        Given a prompt, generate the following text.
        > If you want to generate a response from chat messages, you should use the [`InferenceClient.chat_completion`] method.
        > It accepts a list of messages instead of a single text prompt and handles the chat templating for you.
            prompt (`str`):
                Input text.
            details (`bool`, *optional*):
                By default, text_generation returns a string. Pass `details=True` if you want a detailed output (tokens,
                probabilities, seed, finish reason, etc.). Only available for models running on with the
                `text-generation-inference` backend.
                By default, text_generation returns the full generated text. Pass `stream=True` if you want a stream of
                tokens to be returned. Only available for models running on with the `text-generation-inference`
            adapter_id (`str`, *optional*):
                Lora adapter id.
            best_of (`int`, *optional*):
                Generate best_of sequences and return the one if the highest token logprobs.
            decoder_input_details (`bool`, *optional*):
                Return the decoder input token logprobs and ids. You must set `details=True` as well for it to be taken
                into account. Defaults to `False`.
            do_sample (`bool`, *optional*):
                Activate logits sampling
                Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in
                the text so far, decreasing the model's likelihood to repeat the same line verbatim.
            grammar ([`TextGenerationInputGrammarType`], *optional*):
            max_new_tokens (`int`, *optional*):
                Maximum number of generated tokens. Defaults to 100.
            repetition_penalty (`float`, *optional*):
                The parameter for repetition penalty. 1.0 means no penalty. See [this
                paper](https://arxiv.org/pdf/1909.05858.pdf) for more details.
            return_full_text (`bool`, *optional*):
                Whether to prepend the prompt to the generated text
                Random sampling seed
                Stop generating tokens if a member of `stop` is generated.
            stop_sequences (`list[str]`, *optional*):
                Deprecated argument. Use `stop` instead.
                The value used to module the logits distribution.
            top_n_tokens (`int`, *optional*):
                Return information about the `top_n_tokens` most likely tokens at each generation step, instead of
                just the sampled token.
            top_k (`int`, *optional`):
                The number of highest probability vocabulary tokens to keep for top-k-filtering.
            top_p (`float`, *optional`):
                If set to < 1, only the smallest set of most probable tokens with probabilities that add up to `top_p` or
                higher are kept for generation.
            truncate (`int`, *optional`):
                Truncate inputs tokens to the given size.
            typical_p (`float`, *optional`):
                Typical Decoding mass
                See [Typical Decoding for Natural Language Generation](https://arxiv.org/abs/2202.00666) for more information
            watermark (`bool`, *optional*):
                Watermarking with [A Watermark for Large Language Models](https://arxiv.org/abs/2301.10226)
            `Union[str, TextGenerationOutput, Iterable[str], Iterable[TextGenerationStreamOutput]]`:
            - if `stream=False` and `details=False`, the generated text is returned as a `str` (default)
            - if `stream=True` and `details=False`, the generated text is returned token by token as a `Iterable[str]`
            - if `stream=False` and `details=True`, the generated text is returned with more details as a [`~huggingface_hub.TextGenerationOutput`]
            - if `details=True` and `stream=True`, the generated text is returned token by token as a iterable of [`~huggingface_hub.TextGenerationStreamOutput`]
            `ValidationError`:
                If input values are not valid. No HTTP call is made to the server.
        # Case 1: generate text
        >>> client.text_generation("The huggingface_hub library is ", max_new_tokens=12)
        '100% open source and built to be easy to use.'
        # Case 2: iterate over the generated tokens. Useful for large generation.
        >>> for token in client.text_generation("The huggingface_hub library is ", max_new_tokens=12, stream=True):
        open
        built
        be
        easy
        use
        # Case 3: get more details about the generation process.
        >>> client.text_generation("The huggingface_hub library is ", max_new_tokens=12, details=True)
        TextGenerationOutput(
            generated_text='100% open source and built to be easy to use.',
            details=TextGenerationDetails(
                finish_reason='length',
                generated_tokens=12,
                seed=None,
                prefill=[
                    TextGenerationPrefillOutputToken(id=487, text='The', logprob=None),
                    TextGenerationPrefillOutputToken(id=53789, text=' hugging', logprob=-13.171875),
                    TextGenerationPrefillOutputToken(id=204, text=' ', logprob=-7.0390625)
                tokens=[
                    TokenElement(id=1425, text='100', logprob=-1.0175781, special=False),
                    TokenElement(id=16, text='%', logprob=-0.0463562, special=False),
                    TokenElement(id=25, text='.', logprob=-0.5703125, special=False)
                best_of_sequences=None
        # Case 4: iterate over the generated tokens with more details.
        # Last object is more complete, containing the full generated text and the finish reason.
        >>> for details in client.text_generation("The huggingface_hub library is ", max_new_tokens=12, details=True, stream=True):
        ...     print(details)
        TextGenerationStreamOutput(token=TokenElement(id=1425, text='100', logprob=-1.0175781, special=False), generated_text=None, details=None)
        TextGenerationStreamOutput(token=TokenElement(id=16, text='%', logprob=-0.0463562, special=False), generated_text=None, details=None)
        TextGenerationStreamOutput(token=TokenElement(id=1314, text=' open', logprob=-1.3359375, special=False), generated_text=None, details=None)
        TextGenerationStreamOutput(token=TokenElement(id=3178, text=' source', logprob=-0.28100586, special=False), generated_text=None, details=None)
        TextGenerationStreamOutput(token=TokenElement(id=273, text=' and', logprob=-0.5961914, special=False), generated_text=None, details=None)
        TextGenerationStreamOutput(token=TokenElement(id=3426, text=' built', logprob=-1.9423828, special=False), generated_text=None, details=None)
        TextGenerationStreamOutput(token=TokenElement(id=271, text=' to', logprob=-1.4121094, special=False), generated_text=None, details=None)
        TextGenerationStreamOutput(token=TokenElement(id=314, text=' be', logprob=-1.5224609, special=False), generated_text=None, details=None)
        TextGenerationStreamOutput(token=TokenElement(id=1833, text=' easy', logprob=-2.1132812, special=False), generated_text=None, details=None)
        TextGenerationStreamOutput(token=TokenElement(id=271, text=' to', logprob=-0.08520508, special=False), generated_text=None, details=None)
        TextGenerationStreamOutput(token=TokenElement(id=745, text=' use', logprob=-0.39453125, special=False), generated_text=None, details=None)
        TextGenerationStreamOutput(token=TokenElement(
            id=25,
            text='.',
            logprob=-0.5703125,
            special=False),
            details=TextGenerationStreamOutputStreamDetails(finish_reason='length', generated_tokens=12, seed=None)
        # Case 5: generate constrained output using grammar
        >>> response = client.text_generation(
        ...     prompt="I saw a puppy a cat and a raccoon during my bike ride in the park",
        ...     model="HuggingFaceH4/zephyr-orpo-141b-A35b-v0.1",
        ...     max_new_tokens=100,
        ...     repetition_penalty=1.3,
        ...     grammar={
        ...         "type": "json",
        ...         "value": {
        ...             "properties": {
        ...                 "location": {"type": "string"},
        ...                 "activity": {"type": "string"},
        ...                 "animals_seen": {"type": "integer", "minimum": 1, "maximum": 5},
        ...                 "animals": {"type": "array", "items": {"type": "string"}},
        ...             "required": ["location", "activity", "animals_seen", "animals"],
        >>> json.loads(response)
            "activity": "bike riding",
            "animals": ["puppy", "cat", "raccoon"],
            "animals_seen": 3,
            "location": "park"
        if decoder_input_details and not details:
                "`decoder_input_details=True` has been passed to the server but `details=False` is set meaning that"
                " the output from the server will be truncated."
            decoder_input_details = False
        if stop_sequences is not None:
                "`stop_sequences` is a deprecated argument for `text_generation` task"
                " and will be removed in version '0.28.0'. Use `stop` instead.",
                FutureWarning,
            stop = stop_sequences  # use deprecated arg if provided
        # Build payload
            "adapter_id": adapter_id,
            "decoder_input_details": decoder_input_details,
            "details": details,
            "do_sample": do_sample,
            "grammar": grammar,
            "max_new_tokens": max_new_tokens,
            "repetition_penalty": repetition_penalty,
            "return_full_text": return_full_text,
            "top_n_tokens": top_n_tokens,
            "typical_p": typical_p,
            "watermark": watermark,
        # Remove some parameters if not a TGI server
        unsupported_kwargs = _get_unsupported_text_generation_kwargs(model)
        if len(unsupported_kwargs) > 0:
            # The server does not support some parameters
            # => means it is not a TGI server
            # => remove unsupported parameters and warn the user
            ignored_parameters = []
            for key in unsupported_kwargs:
                if parameters.get(key):
                    ignored_parameters.append(key)
                parameters.pop(key, None)
            if len(ignored_parameters) > 0:
                    "API endpoint/model for text-generation is not served via TGI. Ignoring following parameters:"
                    f" {', '.join(ignored_parameters)}.",
            if details:
                    "API endpoint/model for text-generation is not served via TGI. Parameter `details=True` will"
                    " be ignored meaning only the generated text will be returned.",
                details = False
                    "API endpoint/model for text-generation is not served via TGI. Cannot return output as a stream."
                    " Please pass `stream=False` as input."
        provider_helper = get_provider_helper(self.provider, task="text-generation", model=model_id)
            inputs=prompt,
            extra_payload={"stream": stream},
        # Handle errors separately for more precise error messages
            bytes_output = self._inner_post(request_parameters, stream=stream or False)
            match = MODEL_KWARGS_NOT_USED_REGEX.search(str(e))
            if isinstance(e, BadRequestError) and match:
                unused_params = [kwarg.strip("' ") for kwarg in match.group(1).split(",")]
                _set_unsupported_text_generation_kwargs(model, unused_params)
                return self.text_generation(  # type: ignore
                    details=details,
                    adapter_id=adapter_id,
                    best_of=best_of,
                    decoder_input_details=decoder_input_details,
                    do_sample=do_sample,
                    grammar=grammar,
                    max_new_tokens=max_new_tokens,
                    repetition_penalty=repetition_penalty,
                    return_full_text=return_full_text,
                    top_k=top_k,
                    top_n_tokens=top_n_tokens,
                    truncate=truncate,
                    typical_p=typical_p,
                    watermark=watermark,
            raise_text_generation_error(e)
        # Parse output
            return _stream_text_generation_response(bytes_output, details)  # type: ignore
        data = _bytes_to_dict(bytes_output)  # type: ignore[arg-type]
        # Data can be a single element (dict) or an iterable of dicts where we select the first element of.
            data = data[0]
        response = provider_helper.get_response(data, request_parameters)
        return TextGenerationOutput.parse_obj_as_instance(response) if details else response["generated_text"]
    def text_to_image(
        height: Optional[int] = None,
        width: Optional[int] = None,
        scheduler: Optional[str] = None,
        extra_body: Optional[dict[str, Any]] = None,
        Generate an image based on a given text using a specified model.
                The prompt to generate an image from.
            height (`int`, *optional*):
                The height in pixels of the output image
            width (`int`, *optional*):
                The width in pixels of the output image
                The number of denoising steps. More denoising steps usually lead to a higher quality image at the
                A higher guidance scale value encourages the model to generate images closely linked to the text
                prompt, but values too high may cause saturation and other artifacts.
                Inference Endpoint. If not provided, the default recommended text-to-image model will be used.
            scheduler (`str`, *optional*):
                Override the scheduler with a compatible one.
            extra_body (`dict[str, Any]`, *optional*):
            `Image`: The generated image.
        >>> image = client.text_to_image("An astronaut riding a horse on the moon.")
        >>> image.save("astronaut.png")
        >>> image = client.text_to_image(
        ...     "An astronaut riding a horse on the moon.",
        ...     negative_prompt="low resolution, blurry",
        ...     model="stabilityai/stable-diffusion-2-1",
        >>> image.save("better_astronaut.png")
        Example using a third-party provider directly. Usage will be billed on your fal.ai account.
        ...     provider="fal-ai",  # Use fal.ai provider
        ...     api_key="fal-ai-api-key",  # Pass your fal.ai API key
        ...     "A majestic lion in a fantasy forest",
        ...     model="black-forest-labs/FLUX.1-schnell",
        >>> image.save("lion.png")
        ...     provider="replicate",  # Use replicate provider
        ...     model="black-forest-labs/FLUX.1-dev",
        Example using Replicate provider with extra parameters
        ...     extra_body={"output_quality": 100},
        provider_helper = get_provider_helper(self.provider, task="text-to-image", model=model_id)
                "height": height,
                "width": width,
                "scheduler": scheduler,
    def text_to_video(
        negative_prompt: Optional[list[str]] = None,
        Generate a video based on a given text.
                The prompt to generate a video from.
                Inference Endpoint. If not provided, the default recommended text-to-video model will be used.
                A higher guidance scale value encourages the model to generate videos closely linked to the text
            negative_prompt (`list[str]`, *optional*):
                One or several prompt to guide what NOT to include in video generation.
        ...     provider="fal-ai",  # Using fal.ai provider
        >>> video = client.text_to_video(
        ...     "A majestic lion running in a fantasy forest",
        ...     model="tencent/HunyuanVideo",
        >>> with open("lion.mp4", "wb") as file:
        ...     file.write(video)
        ...     provider="replicate",  # Using replicate provider
        ...     "A cat running in a park",
        ...     model="genmo/mochi-1-preview",
        >>> with open("cat.mp4", "wb") as file:
        provider_helper = get_provider_helper(self.provider, task="text-to-video", model=model_id)
    def text_to_speech(
        early_stopping: Optional[Union[bool, "TextToSpeechEarlyStoppingEnum"]] = None,
        epsilon_cutoff: Optional[float] = None,
        eta_cutoff: Optional[float] = None,
        max_length: Optional[int] = None,
        min_length: Optional[int] = None,
        min_new_tokens: Optional[int] = None,
        num_beam_groups: Optional[int] = None,
        num_beams: Optional[int] = None,
        penalty_alpha: Optional[float] = None,
        use_cache: Optional[bool] = None,
        Synthesize an audio of a voice pronouncing a given text.
                The text to synthesize.
                Inference Endpoint. If not provided, the default recommended text-to-speech model will be used.
                Whether to use sampling instead of greedy decoding when generating new tokens.
            early_stopping (`Union[bool, "TextToSpeechEarlyStoppingEnum"]`, *optional*):
                Controls the stopping condition for beam-based methods.
            epsilon_cutoff (`float`, *optional*):
                If set to float strictly between 0 and 1, only tokens with a conditional probability greater than
                epsilon_cutoff will be sampled. In the paper, suggested values range from 3e-4 to 9e-4, depending on
                the size of the model. See [Truncation Sampling as Language Model
                Desmoothing](https://hf.co/papers/2210.15191) for more details.
            eta_cutoff (`float`, *optional*):
                Eta sampling is a hybrid of locally typical sampling and epsilon sampling. If set to float strictly
                between 0 and 1, a token is only considered if it is greater than either eta_cutoff or sqrt(eta_cutoff)
                * exp(-entropy(softmax(next_token_logits))). The latter term is intuitively the expected next token
                probability, scaled by sqrt(eta_cutoff). In the paper, suggested values range from 3e-4 to 2e-3,
                depending on the size of the model. See [Truncation Sampling as Language Model
            max_length (`int`, *optional*):
                The maximum length (in tokens) of the generated text, including the input.
                The maximum number of tokens to generate. Takes precedence over max_length.
            min_length (`int`, *optional*):
                The minimum length (in tokens) of the generated text, including the input.
            min_new_tokens (`int`, *optional*):
                The minimum number of tokens to generate. Takes precedence over min_length.
            num_beam_groups (`int`, *optional*):
                Number of groups to divide num_beams into in order to ensure diversity among different groups of beams.
                See [this paper](https://hf.co/papers/1610.02424) for more details.
            num_beams (`int`, *optional*):
                Number of beams to use for beam search.
            penalty_alpha (`float`, *optional*):
                The value balances the model confidence and the degeneration penalty in contrastive search decoding.
                The value used to modulate the next token probabilities.
                If set to float < 1, only the smallest set of most probable tokens with probabilities that add up to
                top_p or higher are kept for generation.
            typical_p (`float`, *optional*):
                Local typicality measures how similar the conditional probability of predicting a target token next is
                to the expected conditional probability of predicting a random token next, given the partial text
                already generated. If set to float < 1, the smallest set of the most locally typical tokens with
                probabilities that add up to typical_p or higher are kept for generation. See [this
                paper](https://hf.co/papers/2202.00666) for more details.
            use_cache (`bool`, *optional*):
                Whether the model should use the past last key/values attentions to speed up decoding
            `bytes`: The generated audio.
        >>> from pathlib import Path
        >>> audio = client.text_to_speech("Hello world")
        >>> Path("hello_world.flac").write_bytes(audio)
        Example using a third-party provider directly. Usage will be billed on your Replicate account.
        ...     provider="replicate",
        ...     api_key="your-replicate-api-key",  # Pass your Replicate API key directly
        >>> audio = client.text_to_speech(
        ...     text="Hello world",
        ...     model="OuteAI/OuteTTS-0.3-500M",
        >>> audio =client.text_to_speech(
        ...     "Hello, my name is Kororo, an awesome text-to-speech model.",
        ...     model="hexgrad/Kokoro-82M",
        ...     extra_body={"voice": "af_nicole"},
        >>> Path("hello.flac").write_bytes(audio)
        Example music-gen using "YuE-s1-7B-anneal-en-cot" on fal.ai
        >>> lyrics = '''
        ... [verse]
        ... In the town where I was born
        ... Lived a man who sailed to sea
        ... And he told us of his life
        ... In the land of submarines
        ... So we sailed on to the sun
        ... 'Til we found a sea of green
        ... And we lived beneath the waves
        ... In our yellow submarine
        ... [chorus]
        ... We all live in a yellow submarine
        ... Yellow submarine, yellow submarine
        >>> genres = "pavarotti-style tenor voice"
        ...     provider="fal-ai",
        ...     model="m-a-p/YuE-s1-7B-anneal-en-cot",
        ...     api_key=...,
        >>> audio = client.text_to_speech(lyrics, extra_body={"genres": genres})
        >>> with open("output.mp3", "wb") as f:
        ...     f.write(audio)
        provider_helper = get_provider_helper(self.provider, task="text-to-speech", model=model_id)
                "early_stopping": early_stopping,
                "epsilon_cutoff": epsilon_cutoff,
                "eta_cutoff": eta_cutoff,
                "max_length": max_length,
                "min_length": min_length,
                "min_new_tokens": min_new_tokens,
                "num_beam_groups": num_beam_groups,
                "num_beams": num_beams,
                "penalty_alpha": penalty_alpha,
                "use_cache": use_cache,
        response = provider_helper.get_response(response)
    def token_classification(
        aggregation_strategy: Optional["TokenClassificationAggregationStrategy"] = None,
        ignore_labels: Optional[list[str]] = None,
        stride: Optional[int] = None,
    ) -> list[TokenClassificationOutputElement]:
        Perform token classification on the given text.
        Usually used for sentence parsing, either grammatical, or Named Entity Recognition (NER) to understand keywords contained within text.
                The model to use for the token classification task. Can be a model ID hosted on the Hugging Face Hub or a URL to
                a deployed Inference Endpoint. If not provided, the default recommended token classification model will be used.
            aggregation_strategy (`"TokenClassificationAggregationStrategy"`, *optional*):
                The strategy used to fuse tokens based on model predictions
            ignore_labels (`list[str`, *optional*):
                A list of labels to ignore
            stride (`int`, *optional*):
                The number of overlapping tokens between chunks when splitting the input text.
            `list[TokenClassificationOutputElement]`: List of [`TokenClassificationOutputElement`] items containing the entity group, confidence score, word, start and end index.
        >>> client.token_classification("My name is Sarah Jessica Parker but you can call me Jessica")
            TokenClassificationOutputElement(
                entity_group='PER',
                score=0.9971321225166321,
                word='Sarah Jessica Parker',
                start=11,
                end=31,
                score=0.9773476123809814,
                word='Jessica',
                start=52,
                end=59,
        provider_helper = get_provider_helper(self.provider, task="token-classification", model=model_id)
                "aggregation_strategy": aggregation_strategy,
                "ignore_labels": ignore_labels,
                "stride": stride,
        return TokenClassificationOutputElement.parse_obj_as_list(response)
    def translation(
        src_lang: Optional[str] = None,
        tgt_lang: Optional[str] = None,
        truncation: Optional["TranslationTruncationStrategy"] = None,
    ) -> TranslationOutput:
        Convert text from one language to another.
        Check out https://huggingface.co/tasks/translation for more information on how to choose the best model for
        your specific use case. Source and target languages usually depend on the model.
        However, it is possible to specify source and target languages for certain models. If you are working with one of these models,
        you can use `src_lang` and `tgt_lang` arguments to pass the relevant information.
                A string to be translated.
                The model to use for the translation task. Can be a model ID hosted on the Hugging Face Hub or a URL to
                a deployed Inference Endpoint. If not provided, the default recommended translation model will be used.
            src_lang (`str`, *optional*):
                The source language of the text. Required for models that can translate from multiple languages.
            tgt_lang (`str`, *optional*):
                Target language to translate to. Required for models that can translate to multiple languages.
            truncation (`"TranslationTruncationStrategy"`, *optional*):
            [`TranslationOutput`]: The generated translated text.
                If only one of the `src_lang` and `tgt_lang` arguments are provided.
        >>> client.translation("My name is Wolfgang and I live in Berlin")
        'Mein Name ist Wolfgang und ich lebe in Berlin.'
        >>> client.translation("My name is Wolfgang and I live in Berlin", model="Helsinki-NLP/opus-mt-en-fr")
        TranslationOutput(translation_text='Je m'appelle Wolfgang et je vis à Berlin.')
        Specifying languages:
        >>> client.translation("My name is Sarah Jessica Parker but you can call me Jessica", model="facebook/mbart-large-50-many-to-many-mmt", src_lang="en_XX", tgt_lang="fr_XX")
        "Mon nom est Sarah Jessica Parker mais vous pouvez m'appeler Jessica"
        # Throw error if only one of `src_lang` and `tgt_lang` was given
        if src_lang is not None and tgt_lang is None:
            raise ValueError("You cannot specify `src_lang` without specifying `tgt_lang`.")
        if src_lang is None and tgt_lang is not None:
            raise ValueError("You cannot specify `tgt_lang` without specifying `src_lang`.")
        provider_helper = get_provider_helper(self.provider, task="translation", model=model_id)
                "src_lang": src_lang,
                "tgt_lang": tgt_lang,
        return TranslationOutput.parse_obj_as_list(response)[0]
    def visual_question_answering(
    ) -> list[VisualQuestionAnsweringOutputElement]:
        Answering open-ended questions based on an image.
                The input image for the context. It can be raw bytes, an image file, a URL to an online image, or a PIL Image.
                The model to use for the visual question answering task. Can be a model ID hosted on the Hugging Face Hub or a URL to
                a deployed Inference Endpoint. If not provided, the default recommended visual question answering model will be used.
            `list[VisualQuestionAnsweringOutputElement]`: a list of [`VisualQuestionAnsweringOutputElement`] items containing the predicted label and associated probability.
        >>> client.visual_question_answering(
        ...     image="https://huggingface.co/datasets/mishig/sample_images/resolve/main/tiger.jpg",
        ...     question="What is the animal doing?"
            VisualQuestionAnsweringOutputElement(score=0.778609573841095, answer='laying down'),
            VisualQuestionAnsweringOutputElement(score=0.6957435607910156, answer='sitting'),
        provider_helper = get_provider_helper(self.provider, task="visual-question-answering", model=model_id)
            parameters={"top_k": top_k},
            extra_payload={"question": question, "image": _b64_encode(image)},
        return VisualQuestionAnsweringOutputElement.parse_obj_as_list(response)
    def zero_shot_classification(
        candidate_labels: list[str],
        multi_label: Optional[bool] = False,
        hypothesis_template: Optional[str] = None,
    ) -> list[ZeroShotClassificationOutputElement]:
        Provide as input a text and a set of candidate labels to classify the input text.
                The input text to classify.
            candidate_labels (`list[str]`):
                The set of possible class labels to classify the text into.
            labels (`list[str]`, *optional*):
                (deprecated) List of strings. Each string is the verbalization of a possible label for the input text.
            multi_label (`bool`, *optional*):
                Whether multiple candidate labels can be true. If false, the scores are normalized such that the sum of
                the label likelihoods for each sequence is 1. If true, the labels are considered independent and
                probabilities are normalized for each candidate.
            hypothesis_template (`str`, *optional*):
                The sentence used in conjunction with `candidate_labels` to attempt the text classification by
                replacing the placeholder with the candidate labels.
                Inference Endpoint. This parameter overrides the model defined at the instance level. If not provided, the default recommended zero-shot classification model will be used.
            `list[ZeroShotClassificationOutputElement]`: List of [`ZeroShotClassificationOutputElement`] items containing the predicted labels and their confidence.
        Example with `multi_label=False`:
        >>> text = (
        ...     "A new model offers an explanation for how the Galilean satellites formed around the solar system's"
        ...     "largest world. Konstantin Batygin did not set out to solve one of the solar system's most puzzling"
        ...     " mysteries when he went for a run up a hill in Nice, France."
        >>> labels = ["space & cosmos", "scientific discovery", "microbiology", "robots", "archeology"]
        >>> client.zero_shot_classification(text, labels)
            ZeroShotClassificationOutputElement(label='scientific discovery', score=0.7961668968200684),
            ZeroShotClassificationOutputElement(label='space & cosmos', score=0.18570658564567566),
            ZeroShotClassificationOutputElement(label='microbiology', score=0.00730885099619627),
            ZeroShotClassificationOutputElement(label='archeology', score=0.006258360575884581),
            ZeroShotClassificationOutputElement(label='robots', score=0.004559356719255447),
        >>> client.zero_shot_classification(text, labels, multi_label=True)
            ZeroShotClassificationOutputElement(label='scientific discovery', score=0.9829297661781311),
            ZeroShotClassificationOutputElement(label='space & cosmos', score=0.755190908908844),
            ZeroShotClassificationOutputElement(label='microbiology', score=0.0005462635890580714),
            ZeroShotClassificationOutputElement(label='archeology', score=0.00047131875180639327),
            ZeroShotClassificationOutputElement(label='robots', score=0.00030448526376858354),
        Example with `multi_label=True` and a custom `hypothesis_template`:
        >>> client.zero_shot_classification(
        ...    text="I really like our dinner and I'm very happy. I don't like the weather though.",
        ...    labels=["positive", "negative", "pessimistic", "optimistic"],
        ...    multi_label=True,
        ...    hypothesis_template="This text is {} towards the weather"
            ZeroShotClassificationOutputElement(label='negative', score=0.9231801629066467),
            ZeroShotClassificationOutputElement(label='pessimistic', score=0.8760990500450134),
            ZeroShotClassificationOutputElement(label='optimistic', score=0.0008674879791215062),
            ZeroShotClassificationOutputElement(label='positive', score=0.0005250611575320363)
        provider_helper = get_provider_helper(self.provider, task="zero-shot-classification", model=model_id)
                "candidate_labels": candidate_labels,
                "multi_label": multi_label,
                "hypothesis_template": hypothesis_template,
        output = _bytes_to_dict(response)
        return ZeroShotClassificationOutputElement.parse_obj_as_list(output)
    def zero_shot_image_classification(
        # deprecated argument
        labels: list[str] = None,  # type: ignore
    ) -> list[ZeroShotImageClassificationOutputElement]:
        Provide input image and text labels to predict text labels for the image.
                The candidate labels for this image
                (deprecated) List of string possible labels. There must be at least 2 labels.
                Inference Endpoint. This parameter overrides the model defined at the instance level. If not provided, the default recommended zero-shot image classification model will be used.
                The sentence used in conjunction with `candidate_labels` to attempt the image classification by
            `list[ZeroShotImageClassificationOutputElement]`: List of [`ZeroShotImageClassificationOutputElement`] items containing the predicted labels and their confidence.
        >>> client.zero_shot_image_classification(
        ...     "https://upload.wikimedia.org/wikipedia/commons/thumb/4/43/Cute_dog.jpg/320px-Cute_dog.jpg",
        ...     labels=["dog", "cat", "horse"],
        [ZeroShotImageClassificationOutputElement(label='dog', score=0.956),...]
        # Raise ValueError if input is less than 2 labels
        if len(candidate_labels) < 2:
            raise ValueError("You must specify at least 2 classes to compare.")
        provider_helper = get_provider_helper(self.provider, task="zero-shot-image-classification", model=model_id)
        return ZeroShotImageClassificationOutputElement.parse_obj_as_list(response)
    def get_endpoint_info(self, *, model: Optional[str] = None) -> dict[str, Any]:
        Get information about the deployed endpoint.
        This endpoint is only available on endpoints powered by Text-Generation-Inference (TGI) or Text-Embedding-Inference (TEI).
        Endpoints powered by `transformers` return an empty payload.
            `dict[str, Any]`: Information about the endpoint.
        >>> client.get_endpoint_info()
            'model_id': 'meta-llama/Meta-Llama-3-70B-Instruct',
            'model_sha': None,
            'model_dtype': 'torch.float16',
            'model_device_type': 'cuda',
            'model_pipeline_tag': None,
            'max_concurrent_requests': 128,
            'max_best_of': 2,
            'max_stop_sequences': 4,
            'max_input_length': 8191,
            'max_total_tokens': 8192,
            'waiting_served_ratio': 0.3,
            'max_batch_total_tokens': 1259392,
            'max_waiting_tokens': 20,
            'max_batch_size': None,
            'validation_workers': 32,
            'max_client_batch_size': 4,
            'version': '2.0.2',
            'sha': 'dccab72549635c7eb5ddb17f43f0b7cdff07c214',
            'docker_label': 'sha-dccab72'
        if self.provider != "hf-inference":
            raise ValueError(f"Getting endpoint info is not supported on '{self.provider}'.")
        model = model or self.model
            raise ValueError("Model id not provided.")
        if model.startswith(("http://", "https://")):
            url = model.rstrip("/") + "/info"
            url = f"{constants.INFERENCE_ENDPOINT}/models/{model}/info"
        response = get_session().get(url, headers=build_hf_headers(token=self.token))
        return response.json()
    def health_check(self, model: Optional[str] = None) -> bool:
        Check the health of the deployed endpoint.
        Health check is only available with Inference Endpoints powered by Text-Generation-Inference (TGI) or Text-Embedding-Inference (TEI).
                URL of the Inference Endpoint. This parameter overrides the model defined at the instance level. Defaults to None.
            `bool`: True if everything is working fine.
        >>> client = InferenceClient("https://jzgu0buei5.us-east-1.aws.endpoints.huggingface.cloud")
        >>> client.health_check()
            raise ValueError(f"Health check is not supported on '{self.provider}'.")
        if not model.startswith(("http://", "https://")):
            raise ValueError("Model must be an Inference Endpoint URL.")
        url = model.rstrip("/") + "/health"
        return response.status_code == 200
    def chat(self) -> "ProxyClientChat":
        return ProxyClientChat(self)
class _ProxyClient:
    """Proxy class to be able to call `client.chat.completion.create(...)` as OpenAI client."""
    def __init__(self, client: InferenceClient):
class ProxyClientChat(_ProxyClient):
    def completions(self) -> "ProxyClientChatCompletions":
        return ProxyClientChatCompletions(self._client)
class ProxyClientChatCompletions(_ProxyClient):
        return self._client.chat_completion
