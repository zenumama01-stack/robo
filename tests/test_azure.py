from typing import Union, cast
from typing_extensions import Literal, Protocol
from openai._utils import SensitiveHeadersFilter, is_dict
from openai._models import FinalRequestOptions
from openai.lib.azure import AzureOpenAI, AsyncAzureOpenAI
Client = Union[AzureOpenAI, AsyncAzureOpenAI]
sync_client = AzureOpenAI(
    api_version="2023-07-01",
    api_key="example API key",
    azure_endpoint="https://example-resource.azure.openai.com",
async_client = AsyncAzureOpenAI(
@pytest.mark.parametrize("client", [sync_client, async_client])
def test_implicit_deployment_path(client: Client) -> None:
    req = client._build_request(
            url="/chat/completions",
            json_data={"model": "my-deployment-model"},
        req.url
        == "https://example-resource.azure.openai.com/openai/deployments/my-deployment-model/chat/completions?api-version=2023-07-01"
    "client,method",
        (sync_client, "copy"),
        (sync_client, "with_options"),
        (async_client, "copy"),
        (async_client, "with_options"),
def test_client_copying(client: Client, method: Literal["copy", "with_options"]) -> None:
    if method == "copy":
        copied = client.with_options()
    assert copied._custom_query == {"api-version": "2023-07-01"}
    [sync_client, async_client],
def test_client_copying_override_options(client: Client) -> None:
    copied = client.copy(
        api_version="2022-05-01",
    assert copied._custom_query == {"api-version": "2022-05-01"}
def test_client_token_provider_refresh_sync(respx_mock: MockRouter) -> None:
    respx_mock.post(
        "https://example-resource.azure.openai.com/openai/deployments/gpt-4/chat/completions?api-version=2024-02-01"
    ).mock(
        api_version="2024-02-01",
async def test_client_token_provider_refresh_async(respx_mock: MockRouter) -> None:
class TestAzureLogging:
    def logger_with_filter(self) -> logging.Logger:
        logger = logging.getLogger("openai")
        logger.addFilter(SensitiveHeadersFilter())
        return logger
    def test_azure_api_key_redacted(self, respx_mock: MockRouter, caplog: pytest.LogCaptureFixture) -> None:
            "https://example-resource.azure.openai.com/openai/deployments/gpt-4/chat/completions?api-version=2024-06-01"
        ).mock(return_value=httpx.Response(200, json={"model": "gpt-4"}))
            api_version="2024-06-01",
            api_key="example_api_key",
        with caplog.at_level(logging.DEBUG):
        for record in caplog.records:
            if is_dict(record.args) and record.args.get("headers") and is_dict(record.args["headers"]):
                assert record.args["headers"]["api-key"] == "<redacted>"
    def test_azure_bearer_token_redacted(self, respx_mock: MockRouter, caplog: pytest.LogCaptureFixture) -> None:
            azure_ad_token="example_token",
                assert record.args["headers"]["Authorization"] == "<redacted>"
    async def test_azure_api_key_redacted_async(self, respx_mock: MockRouter, caplog: pytest.LogCaptureFixture) -> None:
    async def test_azure_bearer_token_redacted_async(
        self, respx_mock: MockRouter, caplog: pytest.LogCaptureFixture
    "client,base_url,api,json_data,expected",
        # Deployment-based endpoints
        # AzureOpenAI: No deployment specified
            AzureOpenAI(
            "https://example-resource.azure.openai.com/openai/",
            {"model": "deployment-body"},
            "https://example-resource.azure.openai.com/openai/deployments/deployment-body/chat/completions?api-version=2024-02-01",
        # AzureOpenAI: Deployment specified
                azure_deployment="deployment-client",
            "https://example-resource.azure.openai.com/openai/deployments/deployment-client/",
            "https://example-resource.azure.openai.com/openai/deployments/deployment-client/chat/completions?api-version=2024-02-01",
        # AzureOpenAI: "deployments" in the DNS name
                azure_endpoint="https://deployments.example-resource.azure.openai.com",
            "https://deployments.example-resource.azure.openai.com/openai/",
            "https://deployments.example-resource.azure.openai.com/openai/deployments/deployment-body/chat/completions?api-version=2024-02-01",
        # AzureOpenAI: Deployment called deployments
                azure_deployment="deployments",
            "https://example-resource.azure.openai.com/openai/deployments/deployments/",
            "https://example-resource.azure.openai.com/openai/deployments/deployments/chat/completions?api-version=2024-02-01",
        # AzureOpenAI: base_url and azure_deployment specified; ignored b/c not supported
            AzureOpenAI(  # type: ignore
                base_url="https://example.azure-api.net/PTU/",
            "https://example.azure-api.net/PTU/",
            "https://example.azure-api.net/PTU/deployments/deployment-body/chat/completions?api-version=2024-02-01",
        # AsyncAzureOpenAI: No deployment specified
            AsyncAzureOpenAI(
        # AsyncAzureOpenAI: Deployment specified
        # AsyncAzureOpenAI: "deployments" in the DNS name
        # AsyncAzureOpenAI: Deployment called deployments
        # AsyncAzureOpenAI: base_url and azure_deployment specified; azure_deployment ignored b/c not supported
            AsyncAzureOpenAI(  # type: ignore
def test_prepare_url_deployment_endpoint(
    client: Client, base_url: str, api: str, json_data: dict[str, str], expected: str
            url=api,
            json_data=json_data,
    assert req.url == expected
    assert client.base_url == base_url
        # Non-deployment endpoints
            "https://example-resource.azure.openai.com/openai/models?api-version=2024-02-01",
            "https://example-resource.azure.openai.com/openai/assistants?api-version=2024-02-01",
            "https://deployments.example-resource.azure.openai.com/openai/models?api-version=2024-02-01",
        # AzureOpenAI: Deployment called "deployments"
        # AzureOpenAI: base_url and azure_deployment specified; azure_deployment ignored b/c not supported
            "https://example.azure-api.net/PTU/models?api-version=2024-02-01",
        # AsyncAzureOpenAI: Deployment called "deployments"
def test_prepare_url_nondeployment_endpoint(
    "client,base_url,json_data,expected",
        # Realtime endpoint
            "wss://example-resource.azure.openai.com/openai/realtime?api-version=2024-02-01&deployment=deployment-body",
            "wss://example-resource.azure.openai.com/openai/realtime?api-version=2024-02-01&deployment=deployment-client",
                azure_endpoint="https://deployments.azure.openai.com",
            "https://deployments.azure.openai.com/openai/",
            "wss://deployments.azure.openai.com/openai/realtime?api-version=2024-02-01&deployment=deployment-body",
            "wss://example-resource.azure.openai.com/openai/realtime?api-version=2024-02-01&deployment=deployments",
                azure_deployment="my-deployment",
            "wss://example.azure-api.net/PTU/realtime?api-version=2024-02-01&deployment=deployment-body",
        # AzureOpenAI: websocket_base_url specified
                websocket_base_url="wss://example-resource.azure.openai.com/base",
            "wss://example-resource.azure.openai.com/base/realtime?api-version=2024-02-01&deployment=deployment-body",
def test_prepare_url_realtime(client: AzureOpenAI, base_url: str, json_data: dict[str, str], expected: str) -> None:
    url, _ = client._configure_realtime(json_data["model"], {})
    assert str(url) == expected
        # AsyncAzureOpenAI: websocket_base_url specified
async def test_prepare_url_realtime_async(
    client: AsyncAzureOpenAI, base_url: str, json_data: dict[str, str], expected: str
    url, _ = await client._configure_realtime(json_data["model"], {})
def test_client_sets_base_url(client: Client) -> None:
    assert client.base_url == "https://example-resource.azure.openai.com/openai/deployments/my-deployment/"
    # (not recommended) user sets base_url to target different deployment
    client.base_url = "https://example-resource.azure.openai.com/openai/deployments/different-deployment/"
            json_data={"model": "placeholder"},
        == "https://example-resource.azure.openai.com/openai/deployments/different-deployment/chat/completions?api-version=2024-02-01"
            url="/models",
            json_data={},
    assert req.url == "https://example-resource.azure.openai.com/openai/models?api-version=2024-02-01"
    # (not recommended) user sets base_url to remove deployment
    client.base_url = "https://example-resource.azure.openai.com/openai/"
            json_data={"model": "deployment"},
        == "https://example-resource.azure.openai.com/openai/deployments/deployment/chat/completions?api-version=2024-02-01"
"""Test AzureChatOpenAI wrapper."""
from langchain_core.outputs import ChatGeneration, ChatResult, LLMResult
from tests.unit_tests.fake.callbacks import FakeCallbackHandler
OPENAI_API_VERSION = os.environ.get("AZURE_OPENAI_API_VERSION", "")
OPENAI_API_BASE = os.environ.get("AZURE_OPENAI_API_BASE", "")
OPENAI_API_KEY = os.environ.get("AZURE_OPENAI_API_KEY", "")
DEPLOYMENT_NAME = os.environ.get(
    "AZURE_OPENAI_DEPLOYMENT_NAME",
    os.environ.get("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME", ""),
def _get_llm(**kwargs: Any) -> AzureChatOpenAI:
    return AzureChatOpenAI(  # type: ignore[call-arg, call-arg, call-arg]
        deployment_name=DEPLOYMENT_NAME,
        openai_api_version=OPENAI_API_VERSION,
        azure_endpoint=OPENAI_API_BASE,
        openai_api_key=OPENAI_API_KEY,
@pytest.mark.scheduled
def llm() -> AzureChatOpenAI:
    return _get_llm(max_tokens=50)
def test_chat_openai(llm: AzureChatOpenAI) -> None:
    message = HumanMessage(content="Hello")
    response = llm.invoke([message])
    assert isinstance(response, BaseMessage)
    assert isinstance(response.content, str)
def test_chat_openai_generate() -> None:
    """Test AzureChatOpenAI wrapper with generate."""
    chat = _get_llm(max_tokens=10, n=2)
    response = chat.generate([[message], [message]])
    assert isinstance(response, LLMResult)
    assert len(response.generations) == 2
    for generations in response.generations:
        assert len(generations) == 2
        for generation in generations:
            assert isinstance(generation, ChatGeneration)
            assert isinstance(generation.text, str)
            assert generation.text == generation.message.content
def test_chat_openai_multiple_completions() -> None:
    """Test AzureChatOpenAI wrapper with multiple completions."""
    chat = _get_llm(max_tokens=10, n=5)
    response = chat._generate([message])
    assert isinstance(response, ChatResult)
    assert len(response.generations) == 5
    for generation in response.generations:
        assert isinstance(generation.message, BaseMessage)
        assert isinstance(generation.message.content, str)
def test_chat_openai_streaming() -> None:
    """Test that streaming correctly invokes on_llm_new_token callback."""
    callback_handler = FakeCallbackHandler()
    callback_manager = CallbackManager([callback_handler])
    chat = _get_llm(
        streaming=True,
        callbacks=callback_manager,
    response = chat.invoke([message])
    assert callback_handler.llm_streams > 0
def test_chat_openai_streaming_generation_info() -> None:
    """Test that generation info is preserved when streaming."""
    class _FakeCallback(FakeCallbackHandler):
        saved_things: dict = {}
        def on_llm_end(self, *args: Any, **kwargs: Any) -> Any:
            # Save the generation
            self.saved_things["generation"] = args[0]
    callback = _FakeCallback()
    callback_manager = CallbackManager([callback])
    chat = _get_llm(max_tokens=2, temperature=0, callbacks=callback_manager)
    list(chat.stream("hi"))
    generation = callback.saved_things["generation"]
    # `Hello!` is two tokens, assert that is what is returned
    assert generation.generations[0][0].text == "Hello!"
async def test_async_chat_openai() -> None:
    """Test async generation."""
    response = await chat.agenerate([[message], [message]])
async def test_async_chat_openai_streaming() -> None:
        assert len(generations) == 1
def test_openai_streaming(llm: AzureChatOpenAI) -> None:
    """Test streaming tokens from OpenAI."""
    full: BaseMessageChunk | None = None
    for chunk in llm.stream("I'm Pickle Rick"):
        assert isinstance(chunk.content, str)
        full = chunk if full is None else full + chunk
    assert isinstance(full, AIMessageChunk)
    assert full.response_metadata.get("model_name") is not None
async def test_openai_astream(llm: AzureChatOpenAI) -> None:
    async for chunk in llm.astream("I'm Pickle Rick"):
async def test_openai_abatch(llm: AzureChatOpenAI) -> None:
    """Test streaming tokens from AzureChatOpenAI."""
    result = await llm.abatch(["I'm Pickle Rick", "I'm not Pickle Rick"])
    for token in result:
        assert isinstance(token.content, str)
async def test_openai_abatch_tags(llm: AzureChatOpenAI) -> None:
    """Test batch tokens from AzureChatOpenAI."""
    result = await llm.abatch(
        ["I'm Pickle Rick", "I'm not Pickle Rick"], config={"tags": ["foo"]}
def test_openai_batch(llm: AzureChatOpenAI) -> None:
    result = llm.batch(["I'm Pickle Rick", "I'm not Pickle Rick"])
async def test_openai_ainvoke(llm: AzureChatOpenAI) -> None:
    """Test invoke tokens from AzureChatOpenAI."""
    result = await llm.ainvoke("I'm Pickle Rick", config={"tags": ["foo"]})
    assert isinstance(result.content, str)
    assert result.response_metadata.get("model_name") is not None
def test_openai_invoke(llm: AzureChatOpenAI) -> None:
    result = llm.invoke("I'm Pickle Rick", config={"tags": ["foo"]})
def test_json_mode(llm: AzureChatOpenAI) -> None:
    response = llm.invoke(
        "Return this as json: {'a': 1}", response_format={"type": "json_object"}
    assert json.loads(response.content) == {"a": 1}
    # Test streaming
    for chunk in llm.stream(
    assert isinstance(full.content, str)
    assert json.loads(full.content) == {"a": 1}
async def test_json_mode_async(llm: AzureChatOpenAI) -> None:
    response = await llm.ainvoke(
    async for chunk in llm.astream(
def test_stream_response_format(llm: AzureChatOpenAI) -> None:
    chunks = []
    for chunk in llm.stream("how are ya", response_format=Foo):
    assert len(chunks) > 1
    parsed = full.additional_kwargs["parsed"]
    assert isinstance(parsed, Foo)
    parsed_content = json.loads(full.content)
    assert parsed.response == parsed_content["response"]
async def test_astream_response_format(llm: AzureChatOpenAI) -> None:
    async for chunk in llm.astream("how are ya", response_format=Foo):
"""Test azure openai embeddings."""
    os.environ.get("AZURE_OPENAI_EMBEDDINGS_DEPLOYMENT_NAME", ""),
print
def _get_embeddings(**kwargs: Any) -> AzureOpenAIEmbeddings:
    return AzureOpenAIEmbeddings(  # type: ignore[call-arg]
        azure_deployment=DEPLOYMENT_NAME,
        api_version=OPENAI_API_VERSION,
def test_azure_openai_embedding_documents() -> None:
    """Test openai embeddings."""
    embedding = _get_embeddings()
    assert len(output[0]) == 1536
def test_azure_openai_embedding_documents_multiple() -> None:
    documents = ["foo bar", "bar foo", "foo"]
    embedding = _get_embeddings(chunk_size=2)
    embedding.embedding_ctx_length = 8191
    assert embedding.chunk_size == 2
    assert len(output) == 3
    assert len(output[1]) == 1536
    assert len(output[2]) == 1536
def test_azure_openai_embedding_documents_chunk_size() -> None:
    documents = ["foo bar"] * 20
    # Max 2048 chunks per batch on Azure OpenAI embeddings
    assert embedding.chunk_size == 2048
    assert len(output) == 20
    assert all(len(out) == 1536 for out in output)
async def test_azure_openai_embedding_documents_async_multiple() -> None:
def test_azure_openai_embedding_query() -> None:
    assert len(output) == 1536
async def test_azure_openai_embedding_async_query() -> None:
def test_azure_openai_embedding_with_empty_string() -> None:
    """Test openai embeddings with empty string."""
    document = ["", "abc"]
    output = embedding.embed_documents(document)
        openai.AzureOpenAI(
            api_key=OPENAI_API_KEY,
        )  # type: ignore
        .embeddings.create(input="", model="text-embedding-ada-002")
        .data[0]
        .embedding
    assert np.allclose(output[0], expected_output, atol=0.001)
def test_embed_documents_normalized() -> None:
    output = _get_embeddings().embed_documents(["foo walked to the market"])
    assert np.isclose(np.linalg.norm(output[0]), 1.0)
def test_embed_query_normalized() -> None:
    output = _get_embeddings().embed_query("foo walked to the market")
    assert np.isclose(np.linalg.norm(output), 1.0)
"""Test AzureOpenAI wrapper."""
    os.environ.get("AZURE_OPENAI_LLM_DEPLOYMENT_NAME", ""),
pytestmark = pytest.mark.skipif(
    reason=(
        "This entire module is skipped as all Azure OpenAI models supporting text "
        "completions are retired. See: "
        "https://learn.microsoft.com/en-us/azure/ai-foundry/openai/concepts/legacy-models"
def _get_llm(**kwargs: Any) -> AzureOpenAI:
    return AzureOpenAI(  # type: ignore[call-arg, call-arg, call-arg]
def llm() -> AzureOpenAI:
    return _get_llm(max_tokens=10)
def test_openai_call(llm: AzureOpenAI) -> None:
    """Test valid call to openai."""
    output = llm.invoke("Say something nice:")
def test_openai_streaming(llm: AzureOpenAI) -> None:
    """Test streaming tokens from AzureOpenAI."""
    generator = llm.stream("I'm Pickle Rick")
    assert isinstance(generator, Generator)
    full_response = ""
    for token in generator:
        assert isinstance(token, str)
        full_response += token
    assert full_response
async def test_openai_astream(llm: AzureOpenAI) -> None:
    async for token in llm.astream("I'm Pickle Rick"):
async def test_openai_abatch(llm: AzureOpenAI) -> None:
async def test_openai_abatch_tags(llm: AzureOpenAI) -> None:
def test_openai_batch(llm: AzureOpenAI) -> None:
async def test_openai_ainvoke(llm: AzureOpenAI) -> None:
    assert isinstance(result, str)
def test_openai_invoke(llm: AzureOpenAI) -> None:
def test_openai_multiple_prompts(llm: AzureOpenAI) -> None:
    """Test completion with multiple prompts."""
    output = llm.generate(["I'm Pickle Rick", "I'm Pickle Rick"])
    assert isinstance(output, LLMResult)
    assert isinstance(output.generations, list)
    assert len(output.generations) == 2
def test_openai_streaming_best_of_error() -> None:
    """Test validation for streaming fails if best_of is not 1."""
        _get_llm(best_of=2, streaming=True)
def test_openai_streaming_n_error() -> None:
    """Test validation for streaming fails if n is not 1."""
        _get_llm(n=2, streaming=True)
def test_openai_streaming_multiple_prompts_error() -> None:
    """Test validation for streaming fails if multiple prompts are given."""
        _get_llm(streaming=True).generate(["I'm Pickle Rick", "I'm Pickle Rick"])
def test_openai_streaming_call() -> None:
    llm = _get_llm(max_tokens=10, streaming=True)
    output = llm.invoke("Say foo:")
def test_openai_streaming_callback() -> None:
    llm = _get_llm(
    llm.invoke("Write me a sentence with 100 words.")
    assert callback_handler.llm_streams < 15
async def test_openai_async_generate() -> None:
    llm = _get_llm(max_tokens=10)
    output = await llm.agenerate(["Hello, how are you?"])
async def test_openai_async_streaming_callback() -> None:
    result = await llm.agenerate(["Write me a sentence with 100 words."])
    assert isinstance(result, LLMResult)
"""Test Azure OpenAI Chat API wrapper."""
def test_initialize_azure_openai() -> None:
    llm = AzureChatOpenAI(  # type: ignore[call-arg]
        azure_deployment="35-turbo-dev",
        openai_api_version="2023-05-15",
        azure_endpoint="my-base-url",
    assert llm.deployment_name == "35-turbo-dev"
    assert llm.openai_api_version == "2023-05-15"
    assert llm.azure_endpoint == "my-base-url"
def test_initialize_more() -> None:
        api_key="xyz",  # type: ignore[arg-type]
        model="gpt-35-turbo",
        model_version="0125",
    assert llm.openai_api_key is not None
    assert llm.openai_api_key.get_secret_value() == "xyz"
    assert llm.temperature == 0
    assert llm.stream_usage
    ls_params = llm._get_ls_params()
    assert ls_params.get("ls_provider") == "azure"
    assert ls_params.get("ls_model_name") == "gpt-35-turbo-0125"
def test_initialize_azure_openai_with_openai_api_base_set() -> None:
    with mock.patch.dict(os.environ, {"OPENAI_API_BASE": "https://api.openai.com"}):
        llm = AzureChatOpenAI(  # type: ignore[call-arg, call-arg]
            openai_api_base=None,
        assert ls_params["ls_provider"] == "azure"
        assert ls_params["ls_model_name"] == "35-turbo-dev"
def test_structured_output_old_model() -> None:
    class Output(TypedDict):
        """output."""
    with pytest.warns(match="Cannot use method='json_schema'"):
        ).with_structured_output(Output)
    # assert tool calling was used instead of json_schema
    assert "tools" in llm.steps[0].kwargs  # type: ignore
    assert "response_format" not in llm.steps[0].kwargs  # type: ignore
def test_max_completion_tokens_in_payload() -> None:
    llm = AzureChatOpenAI(
        azure_deployment="o1-mini",
        api_version="2024-12-01-preview",
        model_kwargs={"max_completion_tokens": 300},
    messages = [HumanMessage("Hello")]
    payload = llm._get_request_payload(messages)
    assert payload == {
        "messages": [{"content": "Hello", "role": "user"}],
        "model": None,
def test_responses_api_uses_deployment_name() -> None:
    """Test that Azure deployment name is used for Responses API."""
        azure_deployment="your_deployment",
        api_version="2025-04-01-preview",
        azure_endpoint="your_endpoint",
        api_key=SecretStr("your_api_key"),
        # Force Responses API usage by including a Responses-only parameter
    # For Responses API, the model field should be the deployment name
    assert payload["model"] == "your_deployment"
    assert "input" in payload  # Responses API uses 'input' instead of 'messages'
def test_chat_completions_api_uses_model_name() -> None:
    """Test that regular Chat Completions API still uses model name."""
        model="gpt-5",  # This is the OpenAI model name
        # No Responses-only parameters, so Chat Completions API will be used
    # For Chat Completions API, the model field should still be None/model_name
    # Azure Chat Completions uses deployment in the URL, not in the model field
    assert payload["model"] == "gpt-5"
    assert "messages" in payload  # Chat Completions API uses 'messages'
    assert "input" not in payload
def test_max_completion_tokens_parameter() -> None:
    """Test that max_completion_tokens can be used as a direct parameter."""
        azure_deployment="gpt-5",
        max_completion_tokens=1500,
    # Should use max_completion_tokens instead of max_tokens
    assert "max_completion_tokens" in payload
    assert payload["max_completion_tokens"] == 1500
    assert "max_tokens" not in payload
def test_max_tokens_converted_to_max_completion_tokens() -> None:
    """Test that max_tokens is converted to max_completion_tokens."""
        max_tokens=1000,  # type: ignore[call-arg]
    # max_tokens should be converted to max_completion_tokens
    assert payload["max_completion_tokens"] == 1000
def test_azure_model_param(monkeypatch: Any) -> None:
    monkeypatch.delenv("OPENAI_API_BASE", raising=False)
    llm = AzureOpenAI(
        openai_api_key="secret-api-key",  # type: ignore[call-arg]
        azure_endpoint="endpoint",
        api_version="version",
        azure_deployment="gpt-35-turbo-instruct",
    # Test standard tracing params
    assert ls_params == {
        "ls_provider": "azure",
        "ls_model_type": "llm",
        "ls_model_name": "gpt-35-turbo-instruct",
        "ls_temperature": 0.7,
        "ls_max_tokens": 256,
