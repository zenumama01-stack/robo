import array
from typing import Union, Iterable, cast
from ..types import embedding_create_params
from .._utils import is_given, maybe_transform
from .._extras import numpy as np, has_numpy
from .._base_client import make_request_options
from ..types.embedding_model import EmbeddingModel
from ..types.create_embedding_response import CreateEmbeddingResponse
__all__ = ["Embeddings", "AsyncEmbeddings"]
class Embeddings(SyncAPIResource):
    def with_raw_response(self) -> EmbeddingsWithRawResponse:
        return EmbeddingsWithRawResponse(self)
    def with_streaming_response(self) -> EmbeddingsWithStreamingResponse:
        return EmbeddingsWithStreamingResponse(self)
        input: Union[str, SequenceNotStr[str], Iterable[int], Iterable[Iterable[int]]],
        model: Union[str, EmbeddingModel],
        dimensions: int | Omit = omit,
        encoding_format: Literal["float", "base64"] | Omit = omit,
    ) -> CreateEmbeddingResponse:
        Creates an embedding vector representing the input text.
          input: Input text to embed, encoded as a string or array of tokens. To embed multiple
              inputs in a single request, pass an array of strings or array of token arrays.
              The input must not exceed the max input tokens for the model (8192 tokens for
              all embedding models), cannot be an empty string, and any array must be 2048
              dimensions or less.
              for counting tokens. In addition to the per-input token limit, all embedding
              models enforce a maximum of 300,000 tokens summed across all inputs in a single
          dimensions: The number of dimensions the resulting output embeddings should have. Only
              supported in `text-embedding-3` and later models.
          encoding_format: The format to return the embeddings in. Can be either `float` or
              [`base64`](https://pypi.org/project/pybase64/).
            "input": input,
            "dimensions": dimensions,
            "encoding_format": encoding_format,
        if not is_given(encoding_format):
            params["encoding_format"] = "base64"
        def parser(obj: CreateEmbeddingResponse) -> CreateEmbeddingResponse:
            if is_given(encoding_format):
                # don't modify the response object if a user explicitly asked for a format
            if not obj.data:
                raise ValueError("No embedding data received")
            for embedding in obj.data:
                data = cast(object, embedding.embedding)
                if not isinstance(data, str):
                if not has_numpy():
                    # use array for base64 optimisation
                    embedding.embedding = array.array("f", base64.b64decode(data)).tolist()
                    embedding.embedding = np.frombuffer(  # type: ignore[no-untyped-call]
                        base64.b64decode(data), dtype="float32"
                    ).tolist()
            body=maybe_transform(params, embedding_create_params.EmbeddingCreateParams),
                post_parser=parser,
            cast_to=CreateEmbeddingResponse,
class AsyncEmbeddings(AsyncAPIResource):
    def with_raw_response(self) -> AsyncEmbeddingsWithRawResponse:
        return AsyncEmbeddingsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncEmbeddingsWithStreamingResponse:
        return AsyncEmbeddingsWithStreamingResponse(self)
class EmbeddingsWithRawResponse:
    def __init__(self, embeddings: Embeddings) -> None:
        self._embeddings = embeddings
            embeddings.create,
class AsyncEmbeddingsWithRawResponse:
    def __init__(self, embeddings: AsyncEmbeddings) -> None:
class EmbeddingsWithStreamingResponse:
class AsyncEmbeddingsWithStreamingResponse:
"""**Embeddings** interface."""
from langchain_core.runnables.config import run_in_executor
class Embeddings(ABC):
    """Interface for embedding models.
    This is an interface meant for implementing text embedding models.
    Text embedding models are used to map text to a vector (a point in n-dimensional
    space).
    Texts that are similar will usually be mapped to points that are close to each
    other in this space. The exact details of what's considered "similar" and how
    "distance" is measured in this space are dependent on the specific embedding model.
    This abstraction contains a method for embedding a list of documents and a method
    for embedding a query text. The embedding of a query text is expected to be a single
    vector, while the embedding of a list of documents is expected to be a list of
    vectors.
    Usually the query embedding is identical to the document embedding, but the
    abstraction allows treating them independently.
    In addition to the synchronous methods, this interface also provides asynchronous
    versions of the methods.
    By default, the asynchronous methods are implemented using the synchronous methods;
    however, implementations may choose to override the asynchronous methods with
    an async native implementation for performance reasons.
    def embed_documents(self, texts: list[str]) -> list[list[float]]:
        """Embed search docs.
            texts: List of text to embed.
            List of embeddings.
    def embed_query(self, text: str) -> list[float]:
        """Embed query text.
            text: Text to embed.
            Embedding.
    async def aembed_documents(self, texts: list[str]) -> list[list[float]]:
        """Asynchronous Embed search docs.
        return await run_in_executor(None, self.embed_documents, texts)
    async def aembed_query(self, text: str) -> list[float]:
        """Asynchronous Embed query text.
        return await run_in_executor(None, self.embed_query, text)
__all__ = ["Embeddings"]
from langchain_core.utils import secret_from_env
from pydantic import BaseModel, ConfigDict, Field, SecretStr, model_validator
class FireworksEmbeddings(BaseModel, Embeddings):
    """Fireworks embedding model integration.
    Setup:
        Install `langchain_fireworks` and set environment variable
        `FIREWORKS_API_KEY`.
        pip install -U langchain_fireworks
        export FIREWORKS_API_KEY="your-api-key"
    Key init args — completion params:
            Name of Fireworks model to use.
    Key init args — client params:
        fireworks_api_key:
            Fireworks API key.
    See full list of supported init args and their descriptions in the params section.
    Instantiate:
        from langchain_fireworks import FireworksEmbeddings
        model = FireworksEmbeddings(
            model="nomic-ai/nomic-embed-text-v1.5"
            # Use FIREWORKS_API_KEY env var or pass it in directly
            # fireworks_api_key="..."
    Embed multiple texts:
        vectors = embeddings.embed_documents(["hello", "goodbye"])
        # Showing only the first 3 coordinates
        print(len(vectors))
        print(vectors[0][:3])
        [-0.024603435769677162, -0.007543657906353474, 0.0039630369283258915]
    Embed single text:
        input_text = "The meaning of life is 42"
        vector = embeddings.embed_query("hello")
        print(vector[:3])
    client: OpenAI = Field(default=None, exclude=True)  # type: ignore[assignment]
    fireworks_api_key: SecretStr = Field(
        alias="api_key",
        default_factory=secret_from_env(
            "FIREWORKS_API_KEY",
    """Fireworks API key.
    Automatically read from env variable `FIREWORKS_API_KEY` if not provided.
    model: str = "nomic-ai/nomic-embed-text-v1.5"
    def validate_environment(self) -> Self:
        """Validate environment variables."""
        self.client = OpenAI(
            api_key=self.fireworks_api_key.get_secret_value(),
            base_url="https://api.fireworks.ai/inference/v1",
        """Embed search docs."""
            i.embedding
            for i in self.client.embeddings.create(input=texts, model=self.model).data
        """Embed query text."""
        return self.embed_documents([text])[0]
from collections.abc import Callable, Iterable
from httpx import Response
    SecretStr,
from tenacity import retry, retry_if_exception, stop_after_attempt, wait_fixed
from tokenizers import Tokenizer  # type: ignore[import]
MAX_TOKENS = 16_000
"""A batching parameter for the Mistral API. This is NOT the maximum number of tokens
accepted by the embedding model for each document/chunk, but rather the maximum number
of tokens that can be sent in a single request to the Mistral API (across multiple
documents/chunks)"""
def _is_retryable_error(exception: BaseException) -> bool:
    """Determine if an exception should trigger a retry.
    Only retries on:
    - Timeout exceptions
    - 429 (rate limit) errors
    - 5xx (server) errors
    Does NOT retry on 400 (bad request) or other 4xx client errors.
    if isinstance(exception, httpx.TimeoutException):
    if isinstance(exception, httpx.HTTPStatusError):
        status_code = exception.response.status_code
        # Retry on rate limit (429) or server errors (5xx)
        return status_code == 429 or status_code >= 500
class DummyTokenizer:
    """Dummy tokenizer for when tokenizer cannot be accessed (e.g., via Huggingface)."""
    def encode_batch(texts: list[str]) -> list[list[str]]:
        return [list(text) for text in texts]
class MistralAIEmbeddings(BaseModel, Embeddings):
    """MistralAI embedding model integration.
        Install `langchain_mistralai` and set environment variable
        `MISTRAL_API_KEY`.
        pip install -U langchain_mistralai
        export MISTRAL_API_KEY="your-api-key"
            Name of `MistralAI` model to use.
        api_key:
            The API key for the MistralAI API. If not provided, it will be read from the
            environment variable `MISTRAL_API_KEY`.
        max_concurrent_requests: int
        max_retries:
            The number of times to retry a request if it fails.
        timeout:
            The number of seconds to wait for a response before timing out.
        wait_time:
            The number of seconds to wait before retrying a request in case of 429
        max_concurrent_requests:
            The maximum number of concurrent requests to make to the Mistral API.
        from __module_name__ import MistralAIEmbeddings
        embed = MistralAIEmbeddings(
            model="mistral-embed",
            # api_key="...",
            # other params...
        vector = embed.embed_query(input_text)
    Embed multiple text:
        input_texts = ["Document 1...", "Document 2..."]
        vectors = embed.embed_documents(input_texts)
        # The first 3 coordinates for the first vector
    Async:
        vector = await embed.aembed_query(input_text)
        # multiple:
        # await embed.aembed_documents(input_texts)
        [-0.009100092574954033, 0.005071679595857859, -0.0029193938244134188]
    # The type for client and async_client is ignored because the type is not
    # an Optional after the model is initialized and the model_validator
    # is run.
    client: httpx.Client = Field(default=None)  # type: ignore[assignment]
    async_client: httpx.AsyncClient = Field(  # type: ignore[assignment]
        default=None
    mistral_api_key: SecretStr = Field(
        default_factory=secret_from_env("MISTRAL_API_KEY", default=""),
    endpoint: str = "https://api.mistral.ai/v1/"
    max_retries: int | None = 5
    timeout: int = 120
    wait_time: int | None = 30
    max_concurrent_requests: int = 64
    tokenizer: Tokenizer = Field(default=None)
    model: str = "mistral-embed"
        """Validate configuration."""
        api_key_str = self.mistral_api_key.get_secret_value()
        # TODO: handle retries
        if not self.client:
            self.client = httpx.Client(
                base_url=self.endpoint,
                headers={
                    "Authorization": f"Bearer {api_key_str}",
                timeout=self.timeout,
        # TODO: handle retries and max_concurrency
        if not self.async_client:
            self.async_client = httpx.AsyncClient(
        if self.tokenizer is None:
                self.tokenizer = Tokenizer.from_pretrained(
                    "mistralai/Mixtral-8x7B-v0.1"
            except OSError:  # huggingface_hub GatedRepoError
                    "Could not download mistral tokenizer from Huggingface for "
                    "calculating batch sizes. Set a Huggingface token via the "
                    "HF_TOKEN environment variable to download the real tokenizer. "
                    "Falling back to a dummy tokenizer that uses `len()`.",
                self.tokenizer = DummyTokenizer()
    def _get_batches(self, texts: list[str]) -> Iterable[list[str]]:
        """Split list of texts into batches of less than 16k tokens for Mistral API."""
        batch: list[str] = []
        batch_tokens = 0
        text_token_lengths = [
            len(encoded) for encoded in self.tokenizer.encode_batch(texts)
        for text, text_tokens in zip(texts, text_token_lengths, strict=False):
            if batch_tokens + text_tokens > MAX_TOKENS:
                if len(batch) > 0:
                    # edge case where first batch exceeds max tokens
                    # should not yield an empty batch.
                batch = [text]
                batch_tokens = text_tokens
                batch.append(text)
                batch_tokens += text_tokens
    def _retry(self, func: Callable) -> Callable:
        if self.max_retries is None or self.wait_time is None:
        return retry(
            retry=retry_if_exception(_is_retryable_error),
            wait=wait_fixed(self.wait_time),
            stop=stop_after_attempt(self.max_retries),
        )(func)
        """Embed a list of document texts.
            texts: The list of texts to embed.
            List of embeddings, one for each text.
            batch_responses = []
            @self._retry
            def _embed_batch(batch: list[str]) -> Response:
                response = self.client.post(
                    url="/embeddings",
                    json={
                        "model": self.model,
                        "input": batch,
            batch_responses = [
                _embed_batch(batch) for batch in self._get_batches(texts)
                list(map(float, embedding_obj["embedding"]))
                for response in batch_responses
                for embedding_obj in response.json()["data"]
            logger.exception("An error occurred with MistralAI")
            async def _aembed_batch(batch: list[str]) -> Response:
                response = await self.async_client.post(
            batch_responses = await asyncio.gather(
                *[_aembed_batch(batch) for batch in self._get_batches(texts)]
        """Embed a single query text.
            Embedding for the text.
        return (await self.aembed_documents([text]))[0]
from typing import Literal, overload
import nomic  # type: ignore[import]
from nomic import embed
class NomicEmbeddings(Embeddings):
    """`NomicEmbeddings` embedding model.
        from langchain_nomic import NomicEmbeddings
        model = NomicEmbeddings()
        nomic_api_key: str | None = ...,
        dimensionality: int | None = ...,
        inference_mode: Literal["remote"] = ...,
        inference_mode: Literal["local", "dynamic"],
        device: str | None = ...,
        inference_mode: str,
        nomic_api_key: str | None = None,
        dimensionality: int | None = None,
        inference_mode: str = "remote",
        device: str | None = None,
        vision_model: str | None = None,
        """Initialize `NomicEmbeddings` model.
            model: Model name
            nomic_api_key: Optionally, set the Nomic API key. Uses the `NOMIC_API_KEY`
                environment variable by default.
            dimensionality: The embedding dimension, for use with Matryoshka-capable
                models. Defaults to full-size.
            inference_mode: How to generate embeddings. One of `'remote'`, `'local'`
                (Embed4All), or `'dynamic'` (automatic).
            device: The device to use for local embeddings. Choices include
                `'cpu'`, `'gpu'`, `'nvidia'`, `'amd'`, or a specific device
                name. See the docstring for `GPT4All.__init__` for more info.
                Typically defaults to `'cpu'`.
                    Do not use on macOS.
            vision_model: The vision model to use for image embeddings.
        _api_key = nomic_api_key or os.environ.get("NOMIC_API_KEY")
        if _api_key:
            nomic.login(_api_key)
        self.dimensionality = dimensionality
        self.inference_mode = inference_mode
        self.device = device
        self.vision_model = vision_model
    def embed(self, texts: list[str], *, task_type: str) -> list[list[float]]:
        """Embed texts.
            texts: List of texts to embed
            task_type: The task type to use when embedding. One of `'search_query'`,
                `'search_document'`, `'classification'`, `'clustering'`
        output = embed.text(
            texts=texts,
            task_type=task_type,
            dimensionality=self.dimensionality,
            inference_mode=self.inference_mode,
            device=self.device,
        return output["embeddings"]
            texts: List of texts to embed as documents
        return self.embed(
            task_type="search_document",
            text: Query text
            texts=[text],
            task_type="search_query",
        )[0]
    def embed_image(self, uris: list[str]) -> list[list[float]]:
        """Embed images.
            uris: List of image URIs to embed
        return embed.image(
            images=uris,
            model=self.vision_model,
        )["embeddings"]
"""Ollama embeddings models."""
from ollama import AsyncClient, Client
from pydantic import BaseModel, ConfigDict, PrivateAttr, model_validator
from langchain_ollama._utils import (
    merge_auth_headers,
    parse_url_with_auth,
    validate_model,
class OllamaEmbeddings(BaseModel, Embeddings):
    """Ollama embedding model integration.
    Set up a local Ollama instance:
        [Install the Ollama package](https://github.com/ollama/ollama) and set up a
        local Ollama instance.
        You will need to choose a model to serve.
        You can view a list of available models via [the model library](https://ollama.com/library).
        To fetch a model from the Ollama model library use `ollama pull <name-of-model>`.
        For example, to pull the llama3 model:
        ollama pull llama3
        This will download the default tagged version of the model.
        Typically, the default points to the latest, smallest sized-parameter model.
        * On Mac, the models will be downloaded to `~/.ollama/models`
        * On Linux (or WSL), the models will be stored at `/usr/share/ollama/.ollama/models`
        You can specify the exact version of the model of interest
        as such `ollama pull vicuna:13b-v1.5-16k-q4_0`.
        To view pulled models:
        ollama list
        To start serving:
        ollama serve
        View the Ollama documentation for more commands.
        ollama help
    Install the `langchain-ollama` integration package:
        pip install -U langchain_ollama
            Name of Ollama model to use.
        base_url: str | None
            Base url the model is hosted under.
        embed = OllamaEmbeddings(model="llama3")
    """Model name to use."""
    validate_model_on_init: bool = False
    """Whether to validate the model exists in ollama locally on initialization.
    !!! version-added "Added in `langchain-ollama` 0.3.4"
    base_url: str | None = None
    """Base url the model is hosted under.
    If none, defaults to the Ollama client default.
    Supports `userinfo` auth in the format `http://username:password@localhost:11434`.
    Useful if your Ollama server is behind a proxy.
        `userinfo` is not secure and should only be used for local testing or
        in secure environments. Avoid using it in production or over unsecured
        networks.
        If using `userinfo`, ensure that the Ollama server is configured to
        accept and validate these credentials.
        `userinfo` headers are passed to both sync and async clients.
    client_kwargs: dict | None = {}
    """Additional kwargs to pass to the httpx clients. Pass headers in here.
    These arguments are passed to both synchronous and async clients.
    Use `sync_client_kwargs` and `async_client_kwargs` to pass different arguments
    to synchronous and asynchronous clients.
    async_client_kwargs: dict | None = {}
    """Additional kwargs to merge with `client_kwargs` before passing to httpx client.
    These are clients unique to the async client; for shared args use `client_kwargs`.
    For a full list of the params, see the [httpx documentation](https://www.python-httpx.org/api/#asyncclient).
    sync_client_kwargs: dict | None = {}
    These are clients unique to the sync client; for shared args use `client_kwargs`.
    For a full list of the params, see the [httpx documentation](https://www.python-httpx.org/api/#client).
    _client: Client | None = PrivateAttr(default=None)
    """The client to use for making requests."""
    _async_client: AsyncClient | None = PrivateAttr(default=None)
    """The async client to use for making requests."""
    mirostat: int | None = None
    """Enable Mirostat sampling for controlling perplexity.
    (default: `0`, `0` = disabled, `1` = Mirostat, `2` = Mirostat 2.0)"""
    mirostat_eta: float | None = None
    """Influences how quickly the algorithm responds to feedback
    from the generated text. A lower learning rate will result in
    slower adjustments, while a higher learning rate will make
    the algorithm more responsive. (Default: `0.1`)"""
    mirostat_tau: float | None = None
    """Controls the balance between coherence and diversity
    of the output. A lower value will result in more focused and
    coherent text. (Default: `5.0`)"""
    num_ctx: int | None = None
    """Sets the size of the context window used to generate the
    next token. (Default: `2048`)	"""
    num_gpu: int | None = None
    """The number of GPUs to use. On macOS it defaults to `1` to
    enable metal support, `0` to disable."""
    keep_alive: int | None = None
    """Controls how long the model will stay loaded into memory
    following the request (default: `5m`)
    num_thread: int | None = None
    """Sets the number of threads to use during computation.
    By default, Ollama will detect this for optimal performance.
    It is recommended to set this value to the number of physical
    CPU cores your system has (as opposed to the logical number of cores)."""
    repeat_last_n: int | None = None
    """Sets how far back for the model to look back to prevent
    repetition. (Default: `64`, `0` = disabled, `-1` = `num_ctx`)"""
    repeat_penalty: float | None = None
    """Sets how strongly to penalize repetitions. A higher value (e.g., `1.5`)
    will penalize repetitions more strongly, while a lower value (e.g., `0.9`)
    will be more lenient. (Default: `1.1`)"""
    temperature: float | None = None
    """The temperature of the model. Increasing the temperature will
    make the model answer more creatively. (Default: `0.8`)"""
    stop: list[str] | None = None
    """Sets the stop tokens to use."""
    tfs_z: float | None = None
    """Tail free sampling is used to reduce the impact of less probable
    tokens from the output. A higher value (e.g., `2.0`) will reduce the
    impact more, while a value of `1.0` disables this setting. (default: `1`)"""
    top_k: int | None = None
    """Reduces the probability of generating nonsense. A higher value (e.g. `100`)
    will give more diverse answers, while a lower value (e.g. `10`)
    will be more conservative. (Default: `40`)"""
    top_p: float | None = None
    """Works together with top-k. A higher value (e.g., `0.95`) will lead
    to more diverse text, while a lower value (e.g., `0.5`) will
    generate more focused and conservative text. (Default: `0.9`)"""
    def _default_params(self) -> dict[str, Any]:
        """Get the default parameters for calling Ollama."""
            "mirostat": self.mirostat,
            "mirostat_eta": self.mirostat_eta,
            "mirostat_tau": self.mirostat_tau,
            "num_ctx": self.num_ctx,
            "num_gpu": self.num_gpu,
            "num_thread": self.num_thread,
            "repeat_last_n": self.repeat_last_n,
            "repeat_penalty": self.repeat_penalty,
            "temperature": self.temperature,
            "stop": self.stop,
            "tfs_z": self.tfs_z,
            "top_k": self.top_k,
            "top_p": self.top_p,
    def _set_clients(self) -> Self:
        """Set clients to use for Ollama."""
        client_kwargs = self.client_kwargs or {}
        cleaned_url, auth_headers = parse_url_with_auth(self.base_url)
        merge_auth_headers(client_kwargs, auth_headers)
        sync_client_kwargs = client_kwargs
        if self.sync_client_kwargs:
            sync_client_kwargs = {**sync_client_kwargs, **self.sync_client_kwargs}
        async_client_kwargs = client_kwargs
        if self.async_client_kwargs:
            async_client_kwargs = {**async_client_kwargs, **self.async_client_kwargs}
        self._client = Client(host=cleaned_url, **sync_client_kwargs)
        self._async_client = AsyncClient(host=cleaned_url, **async_client_kwargs)
        if self.validate_model_on_init:
            validate_model(self._client, self.model)
        if not self._client:
                "Ollama sync client is not initialized. "
                "Make sure the model was properly constructed."
        return self._client.embed(
            self.model, texts, options=self._default_params, keep_alive=self.keep_alive
        if not self._async_client:
                "Ollama async client is not initialized. "
            await self._async_client.embed(
                options=self._default_params,
                keep_alive=self.keep_alive,
"""Integration tests for embeddings."""
from langchain_tests.unit_tests.embeddings import EmbeddingsTests
class EmbeddingsIntegrationTests(EmbeddingsTests):
    """Base class for embeddings integration tests.
    Test subclasses must implement the `embeddings_class` property to specify the
    embeddings model to be tested. You can also override the
    `embedding_model_params` property to specify initialization parameters.
    from typing import Type
    from my_package.embeddings import MyEmbeddingsModel
    class TestMyEmbeddingsModelIntegration(EmbeddingsIntegrationTests):
        def embeddings_class(self) -> Type[MyEmbeddingsModel]:
            # Return the embeddings model class to test here
            return MyEmbeddingsModel
            # Return initialization parameters for the model.
            return {"model": "model-001"}
        API references for individual test methods include troubleshooting tips.
    def test_embed_query(self, model: Embeddings) -> None:
        """Test embedding a string query.
        ??? note "Troubleshooting"
            If this test fails, check that:
            1. The model will generate a list of floats when calling `.embed_query`
                on a string.
            2. The length of the list is consistent across different inputs.
        embedding_1 = model.embed_query("foo")
        assert isinstance(embedding_1, list)
        assert isinstance(embedding_1[0], float)
        embedding_2 = model.embed_query("bar")
        assert len(embedding_1) > 0
        assert len(embedding_1) == len(embedding_2)
    def test_embed_documents(self, model: Embeddings) -> None:
        """Test embedding a list of strings.
            1. The model will generate a list of lists of floats when calling
                `embed_documents` on a list of strings.
            2. The length of each list is the same.
        documents = ["foo", "bar", "baz"]
        embeddings = model.embed_documents(documents)
        assert len(embeddings) == len(documents)
        assert all(isinstance(embedding, list) for embedding in embeddings)
        assert all(isinstance(embedding[0], float) for embedding in embeddings)
        assert len(embeddings[0]) > 0
        assert all(len(embedding) == len(embeddings[0]) for embedding in embeddings)
    async def test_aembed_query(self, model: Embeddings) -> None:
        """Test embedding a string query async.
            1. The model will generate a list of floats when calling `aembed_query`
        embedding_1 = await model.aembed_query("foo")
        embedding_2 = await model.aembed_query("bar")
    async def test_aembed_documents(self, model: Embeddings) -> None:
        """Test embedding a list of strings async.
                `aembed_documents` on a list of strings.
        embeddings = await model.aembed_documents(documents)
"""Embeddings unit tests."""
class EmbeddingsTests(BaseStandardTests):
    """Embeddings tests base class."""
    def embeddings_class(self) -> type[Embeddings]:
        """Embeddings class."""
    def embedding_model_params(self) -> dict[str, Any]:
        """Embeddings model parameters."""
    def model(self) -> Embeddings:
        """Embeddings model fixture."""
        return self.embeddings_class(**self.embedding_model_params)
class EmbeddingsUnitTests(EmbeddingsTests):
    """Base class for embeddings unit tests.
    from langchain_tests.unit_tests import EmbeddingsUnitTests
    class TestMyEmbeddingsModelUnit(EmbeddingsUnitTests):
    Testing initialization from environment variables
        Overriding the `init_from_env_params` property will enable additional tests
        for initialization from environment variables. See below for details.
        ??? note "`init_from_env_params`"
            This property is used in unit tests to test initialization from
            environment variables. It should return a tuple of three dictionaries
            that specify the environment variables, additional initialization args,
            and expected instance attributes to check.
            Defaults to empty dicts. If not overridden, the test is skipped.
            def init_from_env_params(self) -> Tuple[dict, dict, dict]:
                        "MY_API_KEY": "api_key",
                        "model": "model-001",
                        "my_api_key": "api_key",
    def test_init(self) -> None:
        """Test model initialization.
            If this test fails, ensure that `embedding_model_params` is specified
            and the model can be initialized from those params.
        model = self.embeddings_class(**self.embedding_model_params)
        assert model is not None
    def init_from_env_params(
    ) -> tuple[dict[str, str], dict[str, Any], dict[str, Any]]:
        """Init from env params.
        This property is used in unit tests to test initialization from environment
        variables. It should return a tuple of three dictionaries that specify the
        environment variables, additional initialization args, and expected instance
        attributes to check.
        return {}, {}, {}
    def test_init_from_env(self) -> None:
        """Test initialization from environment variables.
        Relies on the `init_from_env_params` property.
        Test is skipped if that property is not set.
            If this test fails, ensure that `init_from_env_params` is specified
            correctly and that model parameters are properly set from environment
            variables during initialization.
        env_params, embeddings_params, expected_attrs = self.init_from_env_params
        if env_params:
            with mock.patch.dict(os.environ, env_params):
                model = self.embeddings_class(**embeddings_params)
            for k, expected in expected_attrs.items():
                actual = getattr(model, k)
                if isinstance(actual, SecretStr):
                    actual = actual.get_secret_value()
