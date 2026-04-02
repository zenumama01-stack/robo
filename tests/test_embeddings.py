from openai.types import CreateEmbeddingResponse
class TestEmbeddings:
        embedding = client.embeddings.create(
            input="The quick brown fox jumped over the lazy dog",
            model="text-embedding-3-small",
        assert_matches_type(CreateEmbeddingResponse, embedding, path=["response"])
            dimensions=1,
            encoding_format="float",
        response = client.embeddings.with_raw_response.create(
        embedding = response.parse()
        with client.embeddings.with_streaming_response.create(
class TestAsyncEmbeddings:
        embedding = await async_client.embeddings.create(
        response = await async_client.embeddings.with_raw_response.create(
        async with async_client.embeddings.with_streaming_response.create(
            embedding = await response.parse()
from langchain_classic.schema.embeddings import __all__
EXPECTED_ALL = ["Embeddings"]
def test_all_imports() -> None:
    assert set(__all__) == set(EXPECTED_ALL)
"""Test Fireworks embeddings."""
def test_langchain_fireworks_embedding_documents() -> None:
    """Test Fireworks hosted embeddings."""
    documents = ["foo bar"]
    embedding = FireworksEmbeddings(model="nomic-ai/nomic-embed-text-v1.5")
    output = embedding.embed_documents(documents)
    assert len(output) == 1
    assert len(output[0]) > 0
def test_langchain_fireworks_embedding_query() -> None:
    document = "foo bar"
    output = embedding.embed_query(document)
    assert len(output) > 0
"""Test embedding model integration."""
def test_initialization() -> None:
    """Test embedding model initialization."""
    FireworksEmbeddings(model="nomic-ai/nomic-embed-text-v1.5")
"""Test MistralAI Embedding."""
import tenacity
def test_mistralai_embedding_documents() -> None:
    """Test MistralAI embeddings for documents."""
    documents = ["foo bar", "test document"]
    embedding = MistralAIEmbeddings()
    assert len(output) == 2
    assert len(output[0]) == 1024
def test_mistralai_embedding_query() -> None:
    """Test MistralAI embeddings for query."""
    assert len(output) == 1024
async def test_mistralai_embedding_documents_async() -> None:
    output = await embedding.aembed_documents(documents)
async def test_mistralai_embedding_documents_tenacity_error_async() -> None:
    embedding = MistralAIEmbeddings(max_retries=0)
    mock_response = httpx.Response(
        status_code=429,
        request=httpx.Request("POST", url=embedding.async_client.base_url),
        patch.object(embedding.async_client, "post", return_value=mock_response),
        pytest.raises(tenacity.RetryError),
        await embedding.aembed_documents(documents)
async def test_mistralai_embedding_documents_http_error_async() -> None:
    embedding = MistralAIEmbeddings(max_retries=None)
        status_code=400,
        pytest.raises(httpx.HTTPStatusError),
async def test_mistralai_embedding_query_async() -> None:
    output = await embedding.aembed_query(document)
def test_mistralai_embedding_documents_long() -> None:
    documents = ["foo bar " * 1000, "test document " * 1000] * 5
    assert len(output) == 10
def test_mistralai_embed_query_character() -> None:
    document = "😳"
from unittest.mock import MagicMock
from langchain_mistralai.embeddings import (
    DummyTokenizer,
    _is_retryable_error,
os.environ["MISTRAL_API_KEY"] = "foo"
def test_mistral_init() -> None:
    for model in [
        MistralAIEmbeddings(model="mistral-embed", mistral_api_key="test"),  # type: ignore[call-arg]
        MistralAIEmbeddings(model="mistral-embed", api_key="test"),  # type: ignore[arg-type]
        assert model.model == "mistral-embed"
        assert cast("SecretStr", model.mistral_api_key).get_secret_value() == "test"
def test_is_retryable_error_timeout() -> None:
    """Test that timeout exceptions are retryable."""
    exc = httpx.TimeoutException("timeout")
    assert _is_retryable_error(exc) is True
def test_is_retryable_error_rate_limit() -> None:
    """Test that 429 errors are retryable."""
    response = MagicMock()
    response.status_code = 429
    exc = httpx.HTTPStatusError("rate limit", request=MagicMock(), response=response)
def test_is_retryable_error_server_error() -> None:
    """Test that 5xx errors are retryable."""
    for status_code in [500, 502, 503, 504]:
        response.status_code = status_code
        exc = httpx.HTTPStatusError(
            "server error", request=MagicMock(), response=response
def test_is_retryable_error_bad_request_not_retryable() -> None:
    """Test that 400 errors are NOT retryable."""
    response.status_code = 400
    exc = httpx.HTTPStatusError("bad request", request=MagicMock(), response=response)
    assert _is_retryable_error(exc) is False
def test_is_retryable_error_other_4xx_not_retryable() -> None:
    """Test that other 4xx errors are NOT retryable."""
    for status_code in [401, 403, 404, 422]:
            "client error", request=MagicMock(), response=response
def test_is_retryable_error_other_exceptions() -> None:
    """Test that other exceptions are not retryable."""
    assert _is_retryable_error(ValueError("test")) is False
    assert _is_retryable_error(RuntimeError("test")) is False
def test_dummy_tokenizer() -> None:
    """Test that DummyTokenizer returns character lists."""
    tokenizer = DummyTokenizer()
    result = tokenizer.encode_batch(["hello", "world"])
    assert result == [["h", "e", "l", "l", "o"], ["w", "o", "r", "l", "d"]]
"""Test Nomic embeddings."""
def test_langchain_nomic_embedding_documents() -> None:
    """Test nomic embeddings."""
    embedding = NomicEmbeddings(model="nomic-embed-text-v1")
def test_langchain_nomic_embedding_query() -> None:
def test_langchain_nomic_embedding_dimensionality() -> None:
    embedding = NomicEmbeddings(model="nomic-embed-text-v1.5", dimensionality=256)
    assert len(output[0]) == 256
    NomicEmbeddings(model="nomic-embed-text-v1")
"""Test Ollama embeddings."""
from langchain_tests.integration_tests import EmbeddingsIntegrationTests
MODEL_NAME = os.environ.get("OLLAMA_TEST_MODEL", "llama3.1")
class TestOllamaEmbeddings(EmbeddingsIntegrationTests):
    def embeddings_class(self) -> type[OllamaEmbeddings]:
        return OllamaEmbeddings
    def embedding_model_params(self) -> dict:
        return {"model": MODEL_NAME}
from unittest.mock import MagicMock, Mock, patch
    OllamaEmbeddings(model=MODEL_NAME, keep_alive=1)
@patch("langchain_ollama.embeddings.validate_model")
def test_validate_model_on_init(mock_validate_model: Any) -> None:
    """Test that the model is validated on initialization when requested."""
    OllamaEmbeddings(model=MODEL_NAME, validate_model_on_init=True)
    mock_validate_model.assert_called_once()
    mock_validate_model.reset_mock()
    OllamaEmbeddings(model=MODEL_NAME, validate_model_on_init=False)
    mock_validate_model.assert_not_called()
    OllamaEmbeddings(model=MODEL_NAME)
def test_embed_documents_passes_options(mock_client_class: Any) -> None:
    """Test that `embed_documents()` passes options, including `num_gpu`."""
    mock_client = Mock()
    mock_client_class.return_value = mock_client
    mock_client.embed.return_value = {"embeddings": [[0.1, 0.2, 0.3]]}
    embeddings = OllamaEmbeddings(model=MODEL_NAME, num_gpu=4, temperature=0.5)
    result = embeddings.embed_documents(["test text"])
    assert result == [[0.1, 0.2, 0.3]]
    # Check that embed was called with correct arguments
    mock_client.embed.assert_called_once()
    call_args = mock_client.embed.call_args
    # Verify the keyword arguments
    assert "options" in call_args.kwargs
    assert "keep_alive" in call_args.kwargs
    # Verify options contain num_gpu and temperature
    options = call_args.kwargs["options"]
    assert options["num_gpu"] == 4
    assert options["temperature"] == 0.5
def test_embed_documents_raises_when_client_none() -> None:
    """Test that embed_documents raises RuntimeError when client is None."""
    with patch("langchain_ollama.embeddings.Client") as mock_client_class:
        mock_client_class.return_value = MagicMock()
        embeddings = OllamaEmbeddings(model="test-model")
        embeddings._client = None  # type: ignore[assignment]
        with pytest.raises(RuntimeError, match="sync client is not initialized"):
            embeddings.embed_documents(["test"])
async def test_aembed_documents_raises_when_client_none() -> None:
    """Test that aembed_documents raises RuntimeError when async client is None."""
    with patch("langchain_ollama.embeddings.AsyncClient") as mock_client_class:
        embeddings._async_client = None  # type: ignore[assignment]
        with pytest.raises(RuntimeError, match="async client is not initialized"):
            await embeddings.aembed_documents(["test"])
from langchain_core.embeddings import DeterministicFakeEmbedding, Embeddings
class TestFakeEmbeddingsUnit(EmbeddingsUnitTests):
        return DeterministicFakeEmbedding
        return {"size": 6}  # embedding dimension
class TestFakeEmbeddingsIntegration(EmbeddingsIntegrationTests):
        return {"size": 6}
