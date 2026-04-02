from openai import AzureOpenAI
# may change in the future
# https://learn.microsoft.com/en-us/azure/ai-services/openai/reference#rest-api-versioning
api_version = "2023-07-01-preview"
# gets the API Key from environment variable AZURE_OPENAI_API_KEY
client = AzureOpenAI(
    api_version=api_version,
    # https://learn.microsoft.com/en-us/azure/cognitive-services/openai/how-to/create-resource?pivots=web-portal#create-a-resource
    azure_endpoint="https://example-endpoint.openai.azure.com",
completion = client.chat.completions.create(
    model="deployment-name",  # e.g. gpt-35-instant
    messages=[
            "role": "user",
            "content": "How do I output all files in a directory using Python?",
print(completion.to_json())
deployment_client = AzureOpenAI(
    azure_endpoint="https://example-resource.azure.openai.com/",
    # Navigate to the Azure OpenAI Studio to deploy a model.
    azure_deployment="deployment-name",  # e.g. gpt-35-instant
completion = deployment_client.chat.completions.create(
    model="<ignored>",
from typing import Any, Union, Mapping, TypeVar, Callable, Awaitable, cast, overload
from .._types import NOT_GIVEN, Omit, Query, Timeout, NotGiven
from .._utils import is_given, is_mapping
from .._client import OpenAI, AsyncOpenAI
from .._compat import model_copy
from .._models import FinalRequestOptions
from .._streaming import Stream, AsyncStream
from .._base_client import DEFAULT_MAX_RETRIES, BaseClient
_deployments_endpoints = set(
        "/completions",
        "/chat/completions",
        "/embeddings",
        "/audio/transcriptions",
        "/audio/translations",
        "/audio/speech",
        "/images/generations",
        "/images/edits",
AzureADTokenProvider = Callable[[], str]
AsyncAzureADTokenProvider = Callable[[], "str | Awaitable[str]"]
# we need to use a sentinel API key value for Azure AD
# as we don't want to make the `api_key` in the main client Optional
# and Azure AD tokens may be retrieved on a per-request basis
API_KEY_SENTINEL = "".join(["<", "missing API key", ">"])
class MutuallyExclusiveAuthError(OpenAIError):
            "The `api_key`, `azure_ad_token` and `azure_ad_token_provider` arguments are mutually exclusive; Only one can be passed at a time"
class BaseAzureClient(BaseClient[_HttpxClientT, _DefaultStreamT]):
    _azure_endpoint: httpx.URL | None
    _azure_deployment: str | None
        if options.url in _deployments_endpoints and is_mapping(options.json_data):
            model = options.json_data.get("model")
            if model is not None and "/deployments" not in str(self.base_url.path):
                options.url = f"/deployments/{model}{options.url}"
        return super()._build_request(options, retries_taken=retries_taken)
    def _prepare_url(self, url: str) -> httpx.URL:
        """Adjust the URL if the client was configured with an Azure endpoint + deployment
        and the API feature being called is **not** a deployments-based endpoint
        (i.e. requires /deployments/deployment-name in the URL path).
        if self._azure_deployment and self._azure_endpoint and url not in _deployments_endpoints:
            merge_url = httpx.URL(url)
                merge_raw_path = (
                    self._azure_endpoint.raw_path.rstrip(b"/") + b"/openai/" + merge_url.raw_path.lstrip(b"/")
                return self._azure_endpoint.copy_with(raw_path=merge_raw_path)
        return super()._prepare_url(url)
class AzureOpenAI(BaseAzureClient[httpx.Client, Stream[Any]], OpenAI):
        azure_endpoint: str,
        azure_deployment: str | None = None,
        api_version: str | None = None,
        azure_ad_token: str | None = None,
        azure_ad_token_provider: AzureADTokenProvider | None = None,
        timeout: float | Timeout | None | NotGiven = NOT_GIVEN,
        base_url: str,
        azure_endpoint: str | None = None,
        base_url: str | None = None,
        """Construct a new synchronous azure openai client instance.
        - `api_key` from `AZURE_OPENAI_API_KEY`
        - `azure_ad_token` from `AZURE_OPENAI_AD_TOKEN`
        - `api_version` from `OPENAI_API_VERSION`
        - `azure_endpoint` from `AZURE_OPENAI_ENDPOINT`
            azure_endpoint: Your Azure endpoint, including the resource, e.g. `https://example-resource.azure.openai.com/`
            azure_ad_token: Your Azure Active Directory token, https://www.microsoft.com/en-us/security/business/identity-access/microsoft-entra-id
            azure_ad_token_provider: A function that returns an Azure Active Directory token, will be invoked on every request.
            azure_deployment: A model deployment, if given with `azure_endpoint`, sets the base client URL to include `/deployments/{azure_deployment}`.
                Not supported with Assistants APIs.
            api_key = os.environ.get("AZURE_OPENAI_API_KEY")
            azure_ad_token = os.environ.get("AZURE_OPENAI_AD_TOKEN")
        if api_key is None and azure_ad_token is None and azure_ad_token_provider is None:
                "Missing credentials. Please pass one of `api_key`, `azure_ad_token`, `azure_ad_token_provider`, or the `AZURE_OPENAI_API_KEY` or `AZURE_OPENAI_AD_TOKEN` environment variables."
            api_version = os.environ.get("OPENAI_API_VERSION")
            raise ValueError(
                "Must provide either the `api_version` argument or the `OPENAI_API_VERSION` environment variable"
        if default_query is None:
            default_query = {"api-version": api_version}
            default_query = {**default_query, "api-version": api_version}
                azure_endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT")
                    "Must provide one of the `base_url` or `azure_endpoint` arguments, or the `AZURE_OPENAI_ENDPOINT` environment variable"
            if azure_deployment is not None:
                base_url = f"{azure_endpoint.rstrip('/')}/openai/deployments/{azure_deployment}"
                base_url = f"{azure_endpoint.rstrip('/')}/openai"
            if azure_endpoint is not None:
                raise ValueError("base_url and azure_endpoint are mutually exclusive")
            # define a sentinel value to avoid any typing issues
            api_key = API_KEY_SENTINEL
            websocket_base_url=websocket_base_url,
        self._api_version = api_version
        self._azure_ad_token = azure_ad_token
        self._azure_ad_token_provider = azure_ad_token_provider
        self._azure_deployment = azure_deployment if azure_endpoint else None
        self._azure_endpoint = httpx.URL(azure_endpoint) if azure_endpoint else None
        max_retries: int | NotGiven = NOT_GIVEN,
        return super().copy(
            set_default_headers=set_default_headers,
            set_default_query=set_default_query,
            _extra_kwargs={
                "api_version": api_version or self._api_version,
                "azure_ad_token": azure_ad_token or self._azure_ad_token,
                "azure_ad_token_provider": azure_ad_token_provider or self._azure_ad_token_provider,
    def _get_azure_ad_token(self) -> str | None:
        if self._azure_ad_token is not None:
            return self._azure_ad_token
        provider = self._azure_ad_token_provider
        if provider is not None:
            token = provider()
            if not token or not isinstance(token, str):  # pyright: ignore[reportUnnecessaryIsInstance]
                    f"Expected `azure_ad_token_provider` argument to return a string but it returned {token}",
            return token
        headers: dict[str, str | Omit] = {**options.headers} if is_given(options.headers) else {}
        options = model_copy(options)
        azure_ad_token = self._get_azure_ad_token()
        if azure_ad_token is not None:
            if headers.get("Authorization") is None:
                headers["Authorization"] = f"Bearer {azure_ad_token}"
        elif self.api_key is not API_KEY_SENTINEL:
            if headers.get("api-key") is None:
                headers["api-key"] = self.api_key
            # should never be hit
            raise ValueError("Unable to handle auth")
    def _configure_realtime(self, model: str, extra_query: Query) -> tuple[httpx.URL, dict[str, str]]:
        auth_headers = {}
        query = {
            **extra_query,
            "api-version": self._api_version,
            "deployment": self._azure_deployment or model,
        if self.api_key and self.api_key != "<missing API key>":
            auth_headers = {"api-key": self.api_key}
            token = self._get_azure_ad_token()
            if token:
                auth_headers = {"Authorization": f"Bearer {token}"}
        if self.websocket_base_url is not None:
            base_url = httpx.URL(self.websocket_base_url)
            merge_raw_path = base_url.raw_path.rstrip(b"/") + b"/realtime"
            realtime_url = base_url.copy_with(raw_path=merge_raw_path)
            base_url = self._prepare_url("/realtime")
            realtime_url = base_url.copy_with(scheme="wss")
        url = realtime_url.copy_with(params={**query})
        return url, auth_headers
class AsyncAzureOpenAI(BaseAzureClient[httpx.AsyncClient, AsyncStream[Any]], AsyncOpenAI):
        azure_ad_token_provider: AsyncAzureADTokenProvider | None = None,
        """Construct a new asynchronous azure openai client instance.
    async def _get_azure_ad_token(self) -> str | None:
            if inspect.isawaitable(token):
                token = await token
            if not token or not isinstance(cast(Any, token), str):
            return str(token)
        azure_ad_token = await self._get_azure_ad_token()
    async def _configure_realtime(self, model: str, extra_query: Query) -> tuple[httpx.URL, dict[str, str]]:
            token = await self._get_azure_ad_token()
"""Azure OpenAI chat wrapper."""
from collections.abc import AsyncIterator, Awaitable, Callable, Iterator
from typing import TYPE_CHECKING, Any, Literal, TypeAlias, TypeVar
from langchain_core.language_models import LanguageModelInput
from langchain_core.language_models.chat_models import LangSmithParams
from langchain_core.outputs import ChatGenerationChunk, ChatResult
from langchain_core.utils import from_env, secret_from_env
from pydantic import BaseModel, Field, SecretStr, model_validator
from langchain_openai.chat_models.base import BaseChatOpenAI, _get_default_model_profile
    from langchain_core.language_models import ModelProfile
_BM = TypeVar("_BM", bound=BaseModel)
_DictOrPydanticClass: TypeAlias = dict[str, Any] | type[_BM] | type
_DictOrPydantic: TypeAlias = dict | _BM
def _is_pydantic_class(obj: Any) -> bool:
    return isinstance(obj, type) and is_basemodel_subclass(obj)
class AzureChatOpenAI(BaseChatOpenAI):
    r"""Azure OpenAI chat model integration.
        Head to the Azure [OpenAI quickstart guide](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/chatgpt-quickstart?tabs=keyless%2Ctypescript-keyless%2Cpython-new%2Ccommand-line&pivots=programming-language-python)
        to create your Azure OpenAI deployment.
        Then install `langchain-openai` and set environment variables
        `AZURE_OPENAI_API_KEY` and `AZURE_OPENAI_ENDPOINT`:
        pip install -U langchain-openai
        export AZURE_OPENAI_API_KEY="your-api-key"
        export AZURE_OPENAI_ENDPOINT="https://your-endpoint.openai.azure.com/"
        azure_deployment:
            Name of Azure OpenAI deployment to use.
        temperature:
            Sampling temperature.
        max_tokens:
            Max number of tokens to generate.
        logprobs:
            Whether to return logprobs.
        api_version:
            Azure OpenAI REST API version to use (distinct from the version of the
            underlying model). [See more on the different versions.](https://learn.microsoft.com/en-us/azure/ai-services/openai/reference#rest-api-versioning)
            Timeout for requests.
            Max number of retries.
        organization:
            OpenAI organization ID. If not passed in will be read from env
            var `OPENAI_ORG_ID`.
            The name of the underlying OpenAI model. Used for tracing and token
            counting. Does not affect completion. E.g. `'gpt-4'`, `'gpt-35-turbo'`, etc.
        model_version:
            The version of the underlying OpenAI model. Used for tracing and token
            counting. Does not affect completion. E.g., `'0125'`, `'0125-preview'`, etc.
        model = AzureChatOpenAI(
            azure_deployment="your-deployment",
            api_version="2024-05-01-preview",
            max_tokens=None,
            max_retries=2,
            # organization="...",
            # model="gpt-35-turbo",
            # model_version="0125",
        Any param which is not explicitly supported will be passed directly to the
        `openai.AzureOpenAI.chat.completions.create(...)` API every time to the model is
        invoked.
        AzureChatOpenAI(..., logprobs=True).invoke(...)
        # results in underlying API call of:
        openai.AzureOpenAI(..).chat.completions.create(..., logprobs=True)
        # which is also equivalent to:
        AzureChatOpenAI(...).invoke(..., logprobs=True)
    Invoke:
                "system",
                "You are a helpful translator. Translate the user sentence to French.",
            ("human", "I love programming."),
        model.invoke(messages)
            content="J'adore programmer.",
            usage_metadata={
                "input_tokens": 28,
                "output_tokens": 6,
                "total_tokens": 34,
            response_metadata={
                "token_usage": {
                    "completion_tokens": 6,
                    "prompt_tokens": 28,
                "model_name": "gpt-4",
                "system_fingerprint": "fp_7ec89fabc6",
                "prompt_filter_results": [
                        "prompt_index": 0,
                        "content_filter_results": {
                            "hate": {"filtered": False, "severity": "safe"},
                            "self_harm": {"filtered": False, "severity": "safe"},
                            "sexual": {"filtered": False, "severity": "safe"},
                            "violence": {"filtered": False, "severity": "safe"},
                "finish_reason": "stop",
                "logprobs": None,
            id="run-6d7a5282-0de0-4f27-9cc0-82a9db9a3ce9-0",
    Stream:
        for chunk in model.stream(messages):
            print(chunk.text, end="")
        AIMessageChunk(content="", id="run-a6f294d3-0700-4f6a-abc2-c6ef1178c37f")
        AIMessageChunk(content="J", id="run-a6f294d3-0700-4f6a-abc2-c6ef1178c37f")
        AIMessageChunk(content="'", id="run-a6f294d3-0700-4f6a-abc2-c6ef1178c37f")
        AIMessageChunk(content="ad", id="run-a6f294d3-0700-4f6a-abc2-c6ef1178c37f")
        AIMessageChunk(content="ore", id="run-a6f294d3-0700-4f6a-abc2-c6ef1178c37f")
        AIMessageChunk(content=" la", id="run-a6f294d3-0700-4f6a-abc2-c6ef1178c37f")
            content=" programm", id="run-a6f294d3-0700-4f6a-abc2-c6ef1178c37f"
        AIMessageChunk(content="ation", id="run-a6f294d3-0700-4f6a-abc2-c6ef1178c37f")
        AIMessageChunk(content=".", id="run-a6f294d3-0700-4f6a-abc2-c6ef1178c37f")
                "system_fingerprint": "fp_811936bd4f",
            id="run-a6f294d3-0700-4f6a-abc2-c6ef1178c37f",
        stream = model.stream(messages)
        full = next(stream)
            full += chunk
        full
            content="J'adore la programmation.",
            id="run-ba60e41c-9258-44b8-8f3a-2f10599643b3",
        await model.ainvoke(messages)
        # stream:
        # async for chunk in (await model.astream(messages))
        # batch:
        # await model.abatch([messages])
    Tool calling:
        model_with_tools = model.bind_tools([GetWeather, GetPopulation])
        ai_msg = model_with_tools.invoke(
        ai_msg.tool_calls
                "name": "GetWeather",
                "args": {"location": "Los Angeles, CA"},
                "id": "call_6XswGD5Pqk8Tt5atYr7tfenU",
                "args": {"location": "New York, NY"},
                "id": "call_ZVL15vA8Y7kXqOy3dtmQgeCi",
                "name": "GetPopulation",
                "id": "call_49CFW8zqC9W7mh7hbMLSIrXw",
                "id": "call_6ghfKxV264jEfe1mRIkS3PE7",
    Structured output:
            '''Joke to tell user.'''
            rating: int | None = Field(
                description="How funny the joke is, from 1 to 10"
        structured_model.invoke("Tell me a joke about cats")
        Joke(
            setup="Why was the cat sitting on the computer?",
            punchline="To keep an eye on the mouse!",
            rating=None,
        See `AzureChatOpenAI.with_structured_output()` for more.
    JSON mode:
        json_model = model.bind(response_format={"type": "json_object"})
        ai_msg = json_model.invoke(
            "Return a JSON object with key 'random_ints' and a value of 10 random ints in [0-99]"
        ai_msg.content
        '\\n{\\n  "random_ints": [23, 87, 45, 12, 78, 34, 56, 90, 11, 67]\\n}'
    Image input:
        image_url = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/dd/Gfp-wisconsin-madison-the-nature-boardwalk.jpg/2560px-Gfp-wisconsin-madison-the-nature-boardwalk.jpg"
        image_data = base64.b64encode(httpx.get(image_url).content).decode("utf-8")
                {"type": "text", "text": "describe the weather in this image"},
                    "image_url": {"url": f"data:image/jpeg;base64,{image_data}"},
        ai_msg = model.invoke([message])
        "The weather in the image appears to be quite pleasant. The sky is mostly clear"
    Token usage:
        ai_msg = model.invoke(messages)
        ai_msg.usage_metadata
        {"input_tokens": 28, "output_tokens": 5, "total_tokens": 33}
    Logprobs:
        logprobs_model = model.bind(logprobs=True)
        ai_msg = logprobs_model.invoke(messages)
        ai_msg.response_metadata["logprobs"]
                    "token": "J",
                    "bytes": [74],
                    "logprob": -4.9617593e-06,
                    "top_logprobs": [],
                    "token": "'adore",
                    "bytes": [39, 97, 100, 111, 114, 101],
                    "logprob": -0.25202933,
                    "token": " la",
                    "bytes": [32, 108, 97],
                    "logprob": -0.20141791,
                    "token": " programmation",
                    "bytes": [
                        32,
                        112,
                        114,
                        111,
                        103,
                        97,
                        109,
                        116,
                        105,
                        110,
                    "logprob": -1.9361265e-07,
                    "token": ".",
                    "bytes": [46],
                    "logprob": -1.2233183e-05,
    Response metadata
        ai_msg.response_metadata
            "model_name": "gpt-35-turbo",
    azure_endpoint: str | None = Field(
        default_factory=from_env("AZURE_OPENAI_ENDPOINT", default=None)
    """Your Azure endpoint, including the resource.
        Automatically inferred from env var `AZURE_OPENAI_ENDPOINT` if not provided.
        Example: `https://example-resource.azure.openai.com/`
    deployment_name: str | None = Field(default=None, alias="azure_deployment")
    """A model deployment.
        If given sets the base client URL to include `/deployments/{azure_deployment}`
            This means you won't be able to use non-deployment endpoints.
    openai_api_version: str | None = Field(
        alias="api_version",
        default_factory=from_env("OPENAI_API_VERSION", default=None),
    """Automatically inferred from env var `OPENAI_API_VERSION` if not provided."""
    # Check OPENAI_API_KEY for backwards compatibility.
    # TODO: Remove OPENAI_API_KEY support to avoid possible conflict when using
    # other forms of azure credentials.
    openai_api_key: SecretStr | None = Field(
            ["AZURE_OPENAI_API_KEY", "OPENAI_API_KEY"], default=None
    """Automatically inferred from env var `AZURE_OPENAI_API_KEY` if not provided."""
    azure_ad_token: SecretStr | None = Field(
        default_factory=secret_from_env("AZURE_OPENAI_AD_TOKEN", default=None)
    """Your Azure Active Directory token.
        Automatically inferred from env var `AZURE_OPENAI_AD_TOKEN` if not provided.
        For more, see [this page](https://www.microsoft.com/en-us/security/business/identity-access/microsoft-entra-id).
    azure_ad_token_provider: Callable[[], str] | None = None
    """A function that returns an Azure Active Directory token.
        Will be invoked on every sync request. For async requests,
        will be invoked if `azure_ad_async_token_provider` is not provided.
    azure_ad_async_token_provider: Callable[[], Awaitable[str]] | None = None
        Will be invoked on every async request.
    model_version: str = ""
    """The version of the model (e.g. `'0125'` for `'gpt-3.5-0125'`).
    Azure OpenAI doesn't return model version with the response by default so it must
    be manually specified if you want to use this information downstream, e.g. when
    calculating costs.
    When you specify the version, it will be appended to the model name in the
    response. Setting correct version will help you to calculate the cost properly.
    Model version is not validated, so make sure you set it correctly to get the
    correct cost.
    openai_api_type: str | None = Field(
        default_factory=from_env("OPENAI_API_TYPE", default="azure")
    """Legacy, for `openai<1.0.0` support."""
    validate_base_url: bool = True
    """If legacy arg `openai_api_base` is passed in, try to infer if it is a
        `base_url` or `azure_endpoint` and update client params accordingly.
    model_name: str | None = Field(default=None, alias="model")  # type: ignore[assignment]
    """Name of the deployed OpenAI model, e.g. `'gpt-4o'`, `'gpt-35-turbo'`, etc.
    Distinct from the Azure deployment name, which is set by the Azure user.
    Used for tracing and token counting.
        Does NOT affect completion.
    disabled_params: dict[str, Any] | None = Field(default=None)
    """Parameters of the OpenAI client or chat.completions endpoint that should be
    disabled for the given model.
    Should be specified as `{"param": None | ['val1', 'val2']}` where the key is the
    parameter and the value is either None, meaning that parameter should never be
    used, or it's a list of disabled values for the parameter.
    For example, older models may not support the `'parallel_tool_calls'` parameter at
    all, in which case `disabled_params={"parallel_tool_calls: None}` can ben passed
    If a parameter is disabled then it will not be used by default in any methods, e.g.
    in
    `langchain_openai.chat_models.azure.AzureChatOpenAI.with_structured_output`.
    However this does not prevent a user from directly passed in the parameter during
    invocation.
    By default, unless `model_name="gpt-4o"` is specified, then
    `'parallel_tools_calls'` will be disabled.
    max_tokens: int | None = Field(default=None, alias="max_completion_tokens")  # type: ignore[assignment]
    """Maximum number of tokens to generate."""
            `["langchain", "chat_models", "azure_openai"]`
        return ["langchain", "chat_models", "azure_openai"]
    def lc_secrets(self) -> dict[str, str]:
        """Get the mapping of secret environment variables."""
            "openai_api_key": "AZURE_OPENAI_API_KEY",
            "azure_ad_token": "AZURE_OPENAI_AD_TOKEN",
        """Check if the class is serializable in langchain."""
        if self.n is not None and self.n < 1:
            msg = "n must be at least 1."
        if self.n is not None and self.n > 1 and self.streaming:
            msg = "n must be 1 when streaming."
        if self.disabled_params is None:
            # As of 09-17-2024 'parallel_tool_calls' param is only supported for gpt-4o.
            if self.model_name and self.model_name == "gpt-4o":
                self.disabled_params = {"parallel_tool_calls": None}
        # Check OPENAI_ORGANIZATION for backwards compatibility.
        self.openai_organization = (
            self.openai_organization
            or os.getenv("OPENAI_ORG_ID")
            or os.getenv("OPENAI_ORGANIZATION")
        # Enable stream_usage by default if using default base URL and client
            getattr(self, key, None) is None
            for key in (
                "stream_usage",
                "openai_proxy",
                "openai_api_base",
                "base_url",
                "root_client",
                "async_client",
                "root_async_client",
                "http_client",
                "http_async_client",
            self.stream_usage = True
        # For backwards compatibility. Before openai v1, no distinction was made
        # between azure_endpoint and base_url (openai_api_base).
        openai_api_base = self.openai_api_base
        if openai_api_base and self.validate_base_url:
            if "/openai" not in openai_api_base:
                    "As of openai>=1.0.0, Azure endpoints should be specified via "
                    "the `azure_endpoint` param not `openai_api_base` "
                    "(or alias `base_url`)."
            if self.deployment_name:
                    "As of openai>=1.0.0, if `azure_deployment` (or alias "
                    "`deployment_name`) is specified then "
                    "`base_url` (or alias `openai_api_base`) should not be. "
                    "If specifying `azure_deployment`/`deployment_name` then use "
                    "`azure_endpoint` instead of `base_url`.\n\n"
                    "For example, you could specify:\n\n"
                    'azure_endpoint="https://xxx.openai.azure.com/", '
                    'azure_deployment="my-deployment"\n\n'
                    "Or you can equivalently specify:\n\n"
                    'base_url="https://xxx.openai.azure.com/openai/deployments/my-deployment"'
        client_params: dict = {
            "api_version": self.openai_api_version,
            "azure_endpoint": self.azure_endpoint,
            "azure_deployment": self.deployment_name,
            "api_key": (
                self.openai_api_key.get_secret_value() if self.openai_api_key else None
            "azure_ad_token": (
                self.azure_ad_token.get_secret_value() if self.azure_ad_token else None
            "azure_ad_token_provider": self.azure_ad_token_provider,
            "organization": self.openai_organization,
            "base_url": self.openai_api_base,
            "timeout": self.request_timeout,
            "default_headers": {
                "User-Agent": "langchain-partner-python-azure-openai",
                **(self.default_headers or {}),
            "default_query": self.default_query,
        if self.max_retries is not None:
            client_params["max_retries"] = self.max_retries
            sync_specific = {"http_client": self.http_client}
            self.root_client = openai.AzureOpenAI(**client_params, **sync_specific)  # type: ignore[arg-type]
            self.client = self.root_client.chat.completions
            async_specific = {"http_client": self.http_async_client}
            if self.azure_ad_async_token_provider:
                client_params["azure_ad_token_provider"] = (
                    self.azure_ad_async_token_provider
            self.root_async_client = openai.AsyncAzureOpenAI(
                **client_params,
                **async_specific,  # type: ignore[arg-type]
            self.async_client = self.root_async_client.chat.completions
    def _resolve_model_profile(self) -> ModelProfile | None:
        if self.deployment_name is not None:
            return _get_default_model_profile(self.deployment_name) or None
    def _identifying_params(self) -> dict[str, Any]:
            **super()._identifying_params,
        return "azure-openai-chat"
    def lc_attributes(self) -> dict[str, Any]:
        """Get the attributes relevant to tracing."""
            "openai_api_type": self.openai_api_type,
            "openai_api_version": self.openai_api_version,
        """Get the default parameters for calling Azure OpenAI API."""
        params = super()._default_params
        if "max_tokens" in params:
            params["max_completion_tokens"] = params.pop("max_tokens")
        self, stop: list[str] | None = None, **kwargs: Any
        """Get the parameters used to invoke the model."""
        params = super()._get_ls_params(stop=stop, **kwargs)
        params["ls_provider"] = "azure"
        if self.model_name:
            if self.model_version and self.model_version not in self.model_name:
                params["ls_model_name"] = (
                    self.model_name + "-" + self.model_version.lstrip("-")
                params["ls_model_name"] = self.model_name
        elif self.deployment_name:
            params["ls_model_name"] = self.deployment_name
    def _create_chat_result(
        response: dict | openai.BaseModel,
        generation_info: dict | None = None,
        chat_result = super()._create_chat_result(response, generation_info)
        if not isinstance(response, dict):
            response = response.model_dump()
        for res in response["choices"]:
            if res.get("finish_reason", None) == "content_filter":
                    "Azure has not provided the response due to a content filter "
                    "being triggered"
        if "model" in response:
            model = response["model"]
            if self.model_version:
                model = f"{model}-{self.model_version}"
            chat_result.llm_output = chat_result.llm_output or {}
            chat_result.llm_output["model_name"] = model
        if "prompt_filter_results" in response:
            chat_result.llm_output["prompt_filter_results"] = response[
                "prompt_filter_results"
        for chat_gen, response_choice in zip(
            chat_result.generations, response["choices"], strict=False
            chat_gen.generation_info = chat_gen.generation_info or {}
            chat_gen.generation_info["content_filter_results"] = response_choice.get(
                "content_filter_results", {}
        return chat_result
    def _get_request_payload(
        input_: LanguageModelInput,
        """Get the request payload, using deployment name for Azure Responses API."""
        payload = super()._get_request_payload(input_, stop=stop, **kwargs)
        # For Azure Responses API, use deployment name instead of model name
            self._use_responses_api(payload)
            and not payload.get("model")
            and self.deployment_name
            payload["model"] = self.deployment_name
    def _stream(self, *args: Any, **kwargs: Any) -> Iterator[ChatGenerationChunk]:
        """Route to Chat Completions or Responses API."""
        if self._use_responses_api({**kwargs, **self.model_kwargs}):
            return super()._stream_responses(*args, **kwargs)
        return super()._stream(*args, **kwargs)
    async def _astream(
    ) -> AsyncIterator[ChatGenerationChunk]:
            async for chunk in super()._astream_responses(*args, **kwargs):
            async for chunk in super()._astream(*args, **kwargs):
        schema: _DictOrPydanticClass | None = None,
        method: Literal["function_calling", "json_mode", "json_schema"] = "json_schema",
        include_raw: bool = False,
        strict: bool | None = None,
    ) -> Runnable[LanguageModelInput, _DictOrPydantic]:
        r"""Model wrapper that returns outputs formatted to match the given schema.
            schema: The output schema. Can be passed in as:
                - A JSON Schema,
                - A `TypedDict` class,
                - A Pydantic class,
                - Or an OpenAI function/tool schema.
                If `schema` is a Pydantic class then the model output will be a
                Pydantic instance of that class, and the model-generated fields will be
                validated by the Pydantic class. Otherwise the model output will be a
                dict and will not be validated.
                See `langchain_core.utils.function_calling.convert_to_openai_tool` for
                more on how to properly specify types and descriptions of schema fields
                when specifying a Pydantic or `TypedDict` class.
            method: The method for steering model generation, one of:
                - `'json_schema'`:
                    Uses OpenAI's [Structured Output API](https://platform.openai.com/docs/guides/structured-outputs).
                    Supported for `'gpt-4o-mini'`, `'gpt-4o-2024-08-06'`, `'o1'`, and later
                - `'function_calling'`:
                    Uses OpenAI's tool-calling (formerly called function calling)
                    [API](https://platform.openai.com/docs/guides/function-calling)
                - `'json_mode'`:
                    Uses OpenAI's [JSON mode](https://platform.openai.com/docs/guides/structured-outputs/json-mode).
                    Note that if using JSON mode then you must include instructions for
                    formatting the output into the desired schema into the model call
                Learn more about the differences between the methods and which models
                support which methods [here](https://platform.openai.com/docs/guides/structured-outputs/function-calling-vs-response-format).
            include_raw:
                If `False` then only the parsed structured output is returned.
                If an error occurs during model output parsing it will be raised.
                If `True` then both the raw model response (a `BaseMessage`) and the
                parsed model response will be returned.
                If an error occurs during output parsing it will be caught and returned
                as well.
                The final output is always a `dict` with keys `'raw'`, `'parsed'`, and
                `'parsing_error'`.
            strict:
                - True:
                    Model output is guaranteed to exactly match the schema.
                    The input schema will also be validated according to the [supported schemas](https://platform.openai.com/docs/guides/structured-outputs/supported-schemas?api-mode=responses#supported-schemas).
                - False:
                    Input schema will not be validated and model output will not be
                    validated.
                - None:
                    `strict` argument will not be passed to the model.
                If schema is specified via TypedDict or JSON schema, `strict` is not
                enabled by default. Pass `strict=True` to enable it.
                    `strict` can only be non-null if `method` is `'json_schema'`
                    or `'function_calling'`.
            kwargs: Additional keyword args are passed through to the model.
            A `Runnable` that takes same inputs as a
                `langchain_core.language_models.chat.BaseChatModel`. If `include_raw` is
                `False` and `schema` is a Pydantic class, `Runnable` outputs an instance
                of `schema` (i.e., a Pydantic object). Otherwise, if `include_raw` is
                `False` then `Runnable` outputs a `dict`.
                If `include_raw` is `True`, then `Runnable` outputs a `dict` with keys:
                - `'raw'`: `BaseMessage`
                - `'parsed'`: `None` if there was a parsing error, otherwise the type
                    depends on the `schema` as described above.
                - `'parsing_error'`: `BaseException | None`
        !!! warning "Behavior changed in `langchain-openai` 0.3.0"
            `method` default changed from "function_calling" to "json_schema".
        !!! warning "Behavior changed in `langchain-openai` 0.3.12"
            Support for `tools` added.
        !!! warning "Behavior changed in `langchain-openai` 0.3.21"
            Pass `kwargs` through to the model.
        ??? note "Example: `schema=Pydantic` class, `method='json_schema'`, `include_raw=False`, `strict=True`"
            Note, OpenAI has a number of restrictions on what types of schemas can be
            provided if `strict` = True. When using Pydantic, our model cannot
            specify any Field metadata (like min/max constraints) and fields cannot
            have default values.
            See all constraints [here](https://platform.openai.com/docs/guides/structured-outputs/supported-schemas).
            class AnswerWithJustification(BaseModel):
                '''An answer to the user question along with justification for the answer.'''
                answer: str
                justification: str | None = Field(
                    default=..., description="A justification for the answer."
                azure_deployment="...", model="gpt-4o", temperature=0
            structured_model = model.with_structured_output(AnswerWithJustification)
            structured_model.invoke(
                "What weighs more a pound of bricks or a pound of feathers"
            # -> AnswerWithJustification(
            #     answer='They weigh the same',
            #     justification='Both a pound of bricks and a pound of feathers weigh one pound. The weight is the same, but the volume or density of the objects may differ.'
        ??? note "Example: `schema=Pydantic` class, `method='function_calling'`, `include_raw=False`, `strict=False`"
            structured_model = model.with_structured_output(
                AnswerWithJustification, method="function_calling"
        ??? note "Example: `schema=Pydantic` class, `method='json_schema'`, `include_raw=True`"
                justification: str
                AnswerWithJustification, include_raw=True
            # -> {
            #     'raw': AIMessage(content='', additional_kwargs={'tool_calls': [{'id': 'call_Ao02pnFYXD6GN1yzc0uXPsvF', 'function': {'arguments': '{"answer":"They weigh the same.","justification":"Both a pound of bricks and a pound of feathers weigh one pound. The weight is the same, but the volume or density of the objects may differ."}', 'name': 'AnswerWithJustification'}, 'type': 'function'}]}),
            #     'parsed': AnswerWithJustification(answer='They weigh the same.', justification='Both a pound of bricks and a pound of feathers weigh one pound. The weight is the same, but the volume or density of the objects may differ.'),
            #     'parsing_error': None
        ??? note "Example: `schema=TypedDict` class, `method='json_schema'`, `include_raw=False`, `strict=False`"
            class AnswerWithJustification(TypedDict):
                justification: Annotated[
                    str | None, None, "A justification for the answer."
            #     'answer': 'They weigh the same',
            #     'justification': 'Both a pound of bricks and a pound of feathers weigh one pound. The weight is the same, but the volume and density of the two substances differ.'
        ??? note "Example: `schema=OpenAI` function schema, `method='json_schema'`, `include_raw=False`"
            oai_schema = {
                'name': 'AnswerWithJustification',
                'description': 'An answer to the user question along with justification for the answer.',
                'parameters': {
                    'type': 'object',
                        'answer': {'type': 'string'},
                        'justification': {'description': 'A justification for the answer.', 'type': 'string'}
                    'required': ['answer']
                    azure_deployment="...",
                structured_model = model.with_structured_output(oai_schema)
        ??? note "Example: `schema=Pydantic` class, `method='json_mode'`, `include_raw=True`"
                AnswerWithJustification, method="json_mode", include_raw=True
                "Answer the following question. "
                "Make sure to return a JSON blob with keys 'answer' and 'justification'.\\n\\n"
                "What's heavier a pound of bricks or a pound of feathers?"
            #     'raw': AIMessage(content='{\\n    "answer": "They are both the same weight.",\\n    "justification": "Both a pound of bricks and a pound of feathers weigh one pound. The difference lies in the volume and density of the materials, not the weight." \\n}'),
            #     'parsed': AnswerWithJustification(answer='They are both the same weight.', justification='Both a pound of bricks and a pound of feathers weigh one pound. The difference lies in the volume and density of the materials, not the weight.'),
        ??? note "Example: `schema=None`, `method='json_mode'`, `include_raw=True`"
                method="json_mode", include_raw=True
            #     'parsed': {
            #         'answer': 'They are both the same weight.',
            #         'justification': 'Both a pound of bricks and a pound of feathers weigh one pound. The difference lies in the volume and density of the materials, not the weight.'
            #     },
        return super().with_structured_output(
            schema, method=method, include_raw=include_raw, strict=strict, **kwargs
"""Azure OpenAI embeddings wrapper."""
from collections.abc import Awaitable, Callable
from pydantic import Field, SecretStr, model_validator
class AzureOpenAIEmbeddings(OpenAIEmbeddings):  # type: ignore[override]
    """AzureOpenAI embedding model integration.
        To access AzureOpenAI embedding models you'll need to create an Azure account,
        get an API key, and install the `langchain-openai` integration package.
        You'll need to have an Azure OpenAI instance deployed.
        You can deploy a version on Azure Portal following this
        [guide](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource?pivots=web-portal).
        Once you have your instance running, make sure you have the name of your
        instance and key. You can find the key in the Azure Portal,
        under the “Keys and Endpoint” section of your instance.
        pip install -U langchain_openai
        # Set up your environment variables (or pass them directly to the model)
        export AZURE_OPENAI_ENDPOINT="https://<your-endpoint>.openai.azure.com/"
        export AZURE_OPENAI_API_VERSION="2024-02-01"
            Name of `AzureOpenAI` model to use.
        dimensions:
            Number of dimensions for the embeddings. Can be specified only if the
            underlying model supports it.
        embeddings = AzureOpenAIEmbeddings(
            model="text-embedding-3-large"
            # dimensions: int | None = None, # Can specify dimensions with new text-embedding-3 models
            # azure_endpoint="https://<your-endpoint>.openai.azure.com/", If not provided, will read env variable AZURE_OPENAI_ENDPOINT
            # api_key=... # Can provide an API key directly. If missing read env variable AZURE_OPENAI_API_KEY
            # openai_api_version=..., # If not provided, will read env variable AZURE_OPENAI_API_VERSION
    deployment: str | None = Field(default=None, alias="azure_deployment")
        If given sets the base client URL to include `/deployments/{azure_deployment}`.
    # Check OPENAI_KEY for backwards compatibility.
        default_factory=from_env("OPENAI_API_VERSION", default="2023-05-15"),
    """Automatically inferred from env var `OPENAI_API_VERSION` if not provided.
    Set to `'2023-05-15'` by default if env variable `OPENAI_API_VERSION` is not
        [For more, see this page.](https://www.microsoft.com/en-us/security/business/identity-access/microsoft-entra-id)
    chunk_size: int = 2048
    """Maximum number of texts to embed in each batch"""
            # Only validate openai_api_base if azure_endpoint is not provided
            if not self.azure_endpoint and "/openai" not in openai_api_base:
                self.openai_api_base = cast(str, self.openai_api_base) + "/openai"
                    "(or alias `base_url`). "
            if self.deployment:
                    "As of openai>=1.0.0, if `deployment` (or alias "
                    "`azure_deployment`) is specified then "
                    "`openai_api_base` (or alias `base_url`) should not be. "
                    "Instead use `deployment` (or alias `azure_deployment`) "
                    "and `azure_endpoint`."
            "azure_deployment": self.deployment,
            "max_retries": self.max_retries,
            sync_specific: dict = {"http_client": self.http_client}
            self.client = openai.AzureOpenAI(
                **client_params,  # type: ignore[arg-type]
                **sync_specific,
            ).embeddings
            async_specific: dict = {"http_client": self.http_async_client}
            self.async_client = openai.AsyncAzureOpenAI(
                **async_specific,
"""Azure OpenAI large language models. Not to be confused with chat models."""
from collections.abc import Awaitable, Callable, Mapping
from langchain_core.language_models import LangSmithParams
from langchain_openai.llms.base import BaseOpenAI
class AzureOpenAI(BaseOpenAI):
    """Azure-specific OpenAI large language models.
        from langchain_openai import AzureOpenAI
        openai = AzureOpenAI(model_name="gpt-3.5-turbo-instruct")
        Example: `'https://example-resource.azure.openai.com/'`
        `For more, see this page <https://www.microsoft.com/en-us/security/business/identity-access/microsoft-entra-id>.`__
    """For backwards compatibility. If legacy val openai_api_base is passed in, try to
        infer if it is a base_url or azure_endpoint and update accordingly.
            `["langchain", "llms", "openai"]`
        return ["langchain", "llms", "openai"]
        """Mapping of secret keys to environment variables."""
        if self.n < 1:
        if self.streaming and self.n > 1:
            msg = "Cannot stream results when n > 1."
        if self.streaming and self.best_of > 1:
            msg = "Cannot stream results when best_of > 1."
                self.openai_api_base = (
                    cast(str, self.openai_api_base).rstrip("/") + "/openai"
                    "As of openai>=1.0.0, if `deployment_name` (or alias "
                    "Instead use `deployment_name` (or alias `azure_deployment`) "
                self.deployment_name = None
            "api_key": self.openai_api_key.get_secret_value()
            if self.openai_api_key
            else None,
            "azure_ad_token": self.azure_ad_token.get_secret_value()
            if self.azure_ad_token
                **sync_specific,  # type: ignore[arg-type]
            ).completions
            "deployment_name": self.deployment_name,
        openai_params = {"model": self.deployment_name}
        return {**openai_params, **super()._invocation_params}
        invocation_params = self._invocation_params
        if model_name := invocation_params.get("model"):
            params["ls_model_name"] = model_name
        """Return type of llm."""
        return "azure"
        """Attributes relevant to tracing."""
from typing import Any, Callable, Coroutine, Dict, List, Optional, Union
import httpx  # type: ignore
from openai import (
    AsyncAzureOpenAI,
    AsyncOpenAI,
    AzureOpenAI,
    OpenAI,
from litellm.constants import AZURE_OPERATION_POLLING_TIMEOUT, DEFAULT_MAX_RETRIES
from litellm.litellm_core_utils.logging_utils import track_llm_api_timing
    AsyncHTTPHandler,
    modify_url,
from ...types.llms.openai import HttpxBinaryResponseContent
from ..base import BaseLLM
from .common_utils import (
    AzureOpenAIError,
    BaseAzureLLM,
    get_azure_ad_token_from_oidc,
    process_azure_headers,
    select_azure_base_url_or_endpoint,
from .image_generation import get_azure_image_generation_config
class AzureOpenAIAssistantsAPIConfig:
    Reference: https://learn.microsoft.com/en-us/azure/ai-services/openai/assistants-reference-messages?tabs=python#create-message
    def get_supported_openai_create_message_params(self):
            "role",
    def map_openai_params_create_message_params(
        self, non_default_params: dict, optional_params: dict
        for param, value in non_default_params.items():
            if param == "role":
                optional_params["role"] = value
            if param == "metadata":
                optional_params["metadata"] = value
            elif param == "content":  # only string accepted
                    optional_params["content"] = value
                        message="Azure only accepts content as a string.",
                param == "attachments"
            ):  # this is a v2 param. Azure currently supports the old 'file_id's param
                file_ids: List[str] = []
                        if "file_id" in item:
                            file_ids.append(item["file_id"])
                            if litellm.drop_params is True:
                                    message="Azure doesn't support {}. To drop it from the call, set `litellm.drop_params = True.".format(
                        message="Invalid param. attachments should always be a list. Got={}, Expected=List. Raw value={}".format(
                            type(value), value
def _check_dynamic_azure_params(
    azure_client_params: dict,
    azure_client: Optional[Union[AzureOpenAI, AsyncAzureOpenAI]],
    Returns True if user passed in client params != initialized azure client
    Currently only implemented for api version
    if azure_client is None:
    dynamic_params = ["api_version"]
    for k, v in azure_client_params.items():
        if k in dynamic_params and k == "api_version":
            if v is not None and v != azure_client._custom_query["api-version"]:
class AzureChatCompletion(BaseAzureLLM, BaseLLM):
    def make_sync_azure_openai_chat_completion_request(
        azure_client: Union[AzureOpenAI, OpenAI],
        data: dict,
        Helper to:
        - call chat.completions.create.with_raw_response when litellm.return_response_headers is True
        - call chat.completions.create by default
            raw_response = azure_client.chat.completions.with_raw_response.create(
                **data, timeout=timeout
            headers = dict(raw_response.headers)
            return headers, response
    @track_llm_api_timing()
    async def make_azure_openai_chat_completion_request(
        azure_client: Union[AsyncAzureOpenAI, AsyncOpenAI],
        logging_obj: LiteLLMLoggingObj,
            raw_response = await azure_client.chat.completions.with_raw_response.create(
        except APITimeoutError as e:
            time_delta = round(end_time - start_time, 2)
            e.message += f" - timeout value={timeout}, time taken={time_delta} seconds"
    def completion(  # noqa: PLR0915
        model_response: ModelResponse,
        api_version: str,
        api_type: str,
        azure_ad_token_provider: Optional[Callable],
        dynamic_params: bool,
        print_verbose: Callable,
        logger_fn,
        acompletion: bool = False,
            if model is None or messages is None:
                raise AzureOpenAIError(
                    status_code=422, message="Missing model or messages"
            max_retries = optional_params.pop("max_retries", None)
                max_retries = DEFAULT_MAX_RETRIES
            json_mode: Optional[bool] = optional_params.pop("json_mode", False)
            ### CHECK IF CLOUDFLARE AI GATEWAY ###
            ### if so - set the model as part of the base url
            if api_base is not None and "gateway.ai.cloudflare.com" in api_base:
                client = self._init_azure_client_for_cloudflare_ai_gateway(
                data = {"model": None, "messages": messages, **optional_params}
                data = litellm.AzureOpenAIGPT5Config().transform_request(
                data = litellm.AzureOpenAIConfig().transform_request(
            if acompletion is True:
                    return self.async_streaming(
                    return self.acompletion(
                        convert_tool_call_to_json_mode=json_mode,
            elif "stream" in optional_params and optional_params["stream"] is True:
                return self.streaming(
                logging_obj.pre_call(
                        "headers": {
                            "azure_ad_token": azure_ad_token,
                        "complete_input_dict": data,
                if not isinstance(max_retries, int):
                        status_code=422, message="max retries must be an int"
                # init AzureOpenAI Client
                azure_client = self.get_azure_openai_client(
                    _is_async=False,
                if not isinstance(azure_client, (AzureOpenAI, OpenAI)):
                        message="azure_client is not an instance of AzureOpenAI or OpenAI",
                headers, response = self.make_sync_azure_openai_chat_completion_request(
                    azure_client=azure_client, data=data, timeout=timeout
                stringified_response = response.model_dump()
                logging_obj.post_call(
                    original_response=stringified_response,
                return convert_to_model_response_object(
                    response_object=stringified_response,
                    model_response_object=model_response,
                    _response_headers=headers,
        except AzureOpenAIError as e:
            status_code = getattr(e, "status_code", 500)
            error_headers = getattr(e, "headers", None)
            error_response = getattr(e, "response", None)
            error_body = getattr(e, "body", None)
            if error_headers is None and error_response:
                error_headers = getattr(error_response, "headers", None)
                status_code=status_code,
                message=str(e),
                headers=error_headers,
                body=error_body,
    async def acompletion(
        timeout: Any,
        azure_ad_token: Optional[str] = None,
        azure_ad_token_provider: Optional[Callable] = None,
        convert_tool_call_to_json_mode: Optional[bool] = None,
        client=None,  # this is the AsyncAzureOpenAI
        litellm_params: Optional[dict] = {},
            # setting Azure client
            if not isinstance(azure_client, (AsyncAzureOpenAI, AsyncOpenAI)):
                raise ValueError("Azure client is not an instance of AsyncAzureOpenAI or AsyncOpenAI")
                input=data["messages"],
                api_key=azure_client.api_key,
                    "acompletion": True,
            headers, response = await self.make_azure_openai_chat_completion_request(
                azure_client=azure_client,
            logging_obj.model_call_details["response_headers"] = headers
                additional_args={"complete_input_dict": data},
                hidden_params={"headers": headers},
                convert_tool_call_to_json_mode=convert_tool_call_to_json_mode,
        except asyncio.CancelledError as e:
            raise AzureOpenAIError(status_code=500, message=str(e))
            message = getattr(e, "message", str(e))
            body = getattr(e, "body", None)
            if hasattr(e, "status_code"):
                raise AzureOpenAIError(status_code=500, message=message, body=body)
    def streaming(
        logging_obj,
        azure_client_params = {
            "azure_endpoint": api_base,
            "azure_deployment": model,
            "http_client": litellm.client_session,
        azure_client_params = select_azure_base_url_or_endpoint(
            azure_client_params=azure_client_params
            azure_client_params["api_key"] = api_key
        elif azure_ad_token is not None:
            if azure_ad_token.startswith("oidc/"):
                azure_ad_token = get_azure_ad_token_from_oidc(azure_ad_token)
            azure_client_params["azure_ad_token"] = azure_ad_token
        elif azure_ad_token_provider is not None:
            azure_client_params["azure_ad_token_provider"] = azure_ad_token_provider
        streamwrapper = CustomStreamWrapper(
            custom_llm_provider="azure",
            stream_options=data.get("stream_options", None),
            _response_headers=process_azure_headers(headers),
        return streamwrapper
    async def async_streaming(
            # return response
            return streamwrapper  ## DO NOT make this into an async for ... loop, it will yield an async generator, which won't raise errors if the response fails
    async def aembedding(
        model_response: EmbeddingResponse,
        input: list,
            openai_aclient = self.get_azure_openai_client(
            if not isinstance(openai_aclient, (AsyncAzureOpenAI, AsyncOpenAI)):
            raw_response = await openai_aclient.embeddings.with_raw_response.create(
            # Convert json.JSONDecodeError to AzureOpenAIError for two critical reasons:
            # 1. ROUTER BEHAVIOR: The router relies on exception.status_code to determine cooldown logic:
            #    - JSONDecodeError has no status_code → router skips cooldown evaluation
            #    - AzureOpenAIError has status_code → router properly evaluates for cooldown
            # 2. CONNECTION CLEANUP: When response.parse() throws JSONDecodeError, the response
            #    body may not be fully consumed, preventing httpx from properly returning the
            #    connection to the pool. By catching the exception and accessing raw_response.status_code,
            #    we trigger httpx's internal cleanup logic. Without this:
            #    - parse() fails → JSONDecodeError bubbles up → httpx never knows response was acknowledged → connection leak
            #    This completely eliminates "Unclosed connection" warnings during high load.
            except json.JSONDecodeError as json_error:
                    status_code=raw_response.status_code or 500,
                    message=f"Failed to parse raw Azure embedding response: {str(json_error)}"
                ) from json_error
            embedding_response = convert_to_model_response_object(
                response_type="embedding",
            if not isinstance(embedding_response, EmbeddingResponse):
                    message="embedding_response is not an instance of EmbeddingResponse",
            return embedding_response
        aembedding=None,
        if self._client_session is None:
            self._client_session = self.create_client_session()
            data = {"model": model, "input": input, **optional_params}
                max_retries = litellm.DEFAULT_MAX_RETRIES
                    "headers": {"api_key": api_key, "azure_ad_token": azure_ad_token},
            if aembedding is True:
                return self.aembedding(
            raw_response = azure_client.embeddings.with_raw_response.create(**data, timeout=timeout)  # type: ignore
                additional_args={"complete_input_dict": data, "api_base": api_base},
            return convert_to_model_response_object(response_object=response.model_dump(), model_response_object=model_response, response_type="embedding", _response_headers=process_azure_headers(headers))  # type: ignore
            error_text = str(e)
                error_text = error_response.text
                status_code=status_code, message=error_text, headers=error_headers
    async def make_async_azure_httpx_request(
        client: Optional[AsyncHTTPHandler],
        timeout: Optional[Union[float, httpx.Timeout]],
        Implemented for azure dall-e-2 image gen calls
        Alternative to needing a custom transport implementation
            _params = {}
                if isinstance(timeout, float) or isinstance(timeout, int):
                    _httpx_timeout = httpx.Timeout(timeout)
                    _params["timeout"] = _httpx_timeout
                _params["timeout"] = httpx.Timeout(timeout=600.0, connect=5.0)
            async_handler = get_async_httpx_client(
                llm_provider=LlmProviders.AZURE,
                params=_params,
            async_handler = client  # type: ignore
            "images/generations" in api_base
            and api_version
            in [  # dall-e-3 starts from `2023-12-01-preview` so we should be able to avoid conflict
                "2023-06-01-preview",
                "2023-07-01-preview",
                "2023-08-01-preview",
                "2023-09-01-preview",
                "2023-10-01-preview",
        ):  # CREATE + POLL for azure dall-e-2 calls
            api_base = modify_url(
                original_url=api_base, new_path="/openai/images/generations:submit"
            data.pop(
                "model", None
            )  # REMOVE 'model' from dall-e-2 arg https://learn.microsoft.com/en-us/azure/ai-services/openai/reference#request-a-generated-image-dall-e-2-preview
            response = await async_handler.post(
                url=api_base,
                data=json.dumps(data),
            if "operation-location" in response.headers:
                operation_location_url = response.headers["operation-location"]
                raise AzureOpenAIError(status_code=500, message=response.text)
            response = await async_handler.get(
                url=operation_location_url,
            timeout_secs: int = AZURE_OPERATION_POLLING_TIMEOUT
            if "status" not in response.json():
                    "Expected 'status' in response. Got={}".format(response.json())
            while response.json()["status"] not in ["succeeded", "failed"]:
                if time.time() - start_time > timeout_secs:
                        status_code=408, message="Operation polling timed out."
                await asyncio.sleep(int(response.headers.get("retry-after") or 10))
            if response.json()["status"] == "failed":
                error_data = response.json()
                raise AzureOpenAIError(status_code=400, message=json.dumps(error_data))
            result = response.json()["result"]
            return httpx.Response(
                status_code=200,
                headers=response.headers,
                content=json.dumps(result).encode("utf-8"),
                request=httpx.Request(method="POST", url="https://api.openai.com/v1"),
        return await async_handler.post(
            json=data,
    def make_sync_azure_httpx_request(
        client: Optional[HTTPHandler],
            sync_handler = HTTPHandler(**_params, client=litellm.client_session)  # type: ignore
            sync_handler = client  # type: ignore
            response = sync_handler.post(
            response = sync_handler.get(
                time.sleep(int(response.headers.get("retry-after") or 10))
        return sync_handler.post(
    def create_azure_base_url(
        self, azure_client_params: dict, model: Optional[str]
            AzureFoundryFluxImageGenerationConfig,
        api_base: str = azure_client_params.get(
            "azure_endpoint", ""
        )  # "https://example-endpoint.openai.azure.com"
        if api_base.endswith("/"):
        api_version: str = azure_client_params.get("api_version", "")
            model = ""
        # Handle FLUX 2 models on Azure AI which use a different URL pattern
        # e.g., /providers/blackforestlabs/v1/flux-2-pro instead of /openai/deployments/{model}/images/generations
        if AzureFoundryFluxImageGenerationConfig.is_flux2_model(model):
            return AzureFoundryFluxImageGenerationConfig.get_flux2_image_generation_url(
        if "/openai/deployments/" in api_base:
            base_url_with_deployment = api_base
            base_url_with_deployment = api_base + "/openai/deployments/" + model
        base_url_with_deployment += "/images/generations"
        base_url_with_deployment += "?api-version=" + api_version
        return base_url_with_deployment
    async def aimage_generation(
        model_response: Optional[ImageResponse],
        response: Optional[dict] = None
            # response = await azure_client.images.generate(**data, timeout=timeout)
                "api_base", ""
            # Use the deployment name (model) for URL construction, not the base_model from data
            img_gen_api_base = self.create_azure_base_url(
                azure_client_params=azure_client_params, model=model or data.get("model", "")
                input=data["prompt"],
                    "api_base": img_gen_api_base,
            httpx_response: httpx.Response = await self.make_async_azure_httpx_request(
                api_base=img_gen_api_base,
            provider_config = get_azure_image_generation_config(
                data.get("model", "dall-e-2")
                return provider_config.transform_image_generation_response(
                    model=data.get("model", "dall-e-2"),
                    raw_response=httpx_response,
                    model_response=model_response or ImageResponse(),
                    request_data=data,
                    optional_params=data,
                    litellm_params=data,
                    encoding=litellm.encoding,
                response = httpx_response.json()
                stringified_response = response
                return convert_to_model_response_object(  # type: ignore
                    response_type="image_generation",
        model_response: Optional[ImageResponse] = None,
        aimg_generation=None,
            if model and len(model) > 0:
            ## BASE MODEL CHECK
                model_response is not None
                and litellm_params is not None
                and litellm_params.get("base_model", None) is not None
                model_response._hidden_params["model"] = litellm_params.get("base_model", None)
            # Azure image generation API doesn't support extra_body parameter
            extra_body = optional_params.pop("extra_body", {})
            flattened_params = {**optional_params, **extra_body}
            base_model = litellm_params.get("base_model", None) if litellm_params else None
            data = {"model": base_model or model, "prompt": prompt, **flattened_params}
            max_retries = data.pop("max_retries", 2)
            if api_key is None and azure_ad_token_provider is not None:
                azure_ad_token = azure_ad_token_provider()
                if azure_ad_token:
                    headers.pop("api-key", None)
            azure_client_params: Dict[str, Any] = self.initialize_azure_sdk_client(
                model_name=model or "",
                return self.aimage_generation(data=data, input=input, logging_obj=logging_obj, model_response=model_response, api_key=api_key, client=client, azure_client_params=azure_client_params, timeout=timeout, headers=headers, model=model)  # type: ignore
                azure_client_params=azure_client_params, model=model
            httpx_response: httpx.Response = self.make_sync_azure_httpx_request(
                api_version=api_version or "",
                api_key=api_key or "",
                input=prompt,
            return convert_to_model_response_object(response_object=response, model_response_object=model_response, response_type="image_generation")  # type: ignore
            error_code = getattr(e, "status_code", None)
            if error_code is not None:
                raise AzureOpenAIError(status_code=error_code, message=str(e))
    def audio_speech(
        voice: str,
        organization: Optional[str],
        max_retries = optional_params.pop("max_retries", 2)
        if aspeech is not None and aspeech is True:
            return self.async_audio_speech(
        azure_client: AzureOpenAI = self.get_azure_openai_client(
        response = azure_client.audio.speech.create(
            voice=voice,  # type: ignore
        return HttpxBinaryResponseContent(response=response.response)
    async def async_audio_speech(
        azure_client: AsyncAzureOpenAI = self.get_azure_openai_client(
        azure_response = await azure_client.audio.speech.create(
        return HttpxBinaryResponseContent(response=azure_response.response)
    def get_headers(
        input: Optional[list] = None,
        client_session = litellm.client_session or httpx.Client()
            ## build base url - assume api base includes resource name
            if not api_base.endswith("/"):
                api_base += "/"
            api_base += f"{model}"
                base_url=api_base,
                http_client=client_session,
            # cloudflare ai gateway, needs model=None
                azure_endpoint=api_base,
            # only run this check if it's not cloudflare ai gateway
            if model is None and mode != "image_generation":
                raise Exception("model is not set")
        completion = None
        if messages is None:
            messages = [{"role": "user", "content": "Hey"}]
            completion = client.chat.completions.with_raw_response.create(
                messages=messages,  # type: ignore
        response = {}
        if completion is None or not hasattr(completion, "headers"):
            raise Exception("invalid completion response")
            completion.headers.get("x-ratelimit-remaining-requests", None) is not None
        ):  # not provided for dall-e requests
            response["x-ratelimit-remaining-requests"] = completion.headers[
                "x-ratelimit-remaining-requests"
        if completion.headers.get("x-ratelimit-remaining-tokens", None) is not None:
            response["x-ratelimit-remaining-tokens"] = completion.headers[
                "x-ratelimit-remaining-tokens"
        if completion.headers.get("x-ms-region", None) is not None:
            response["x-ms-region"] = completion.headers["x-ms-region"]
API_VERSION_YEAR_SUPPORTED_RESPONSE_FORMAT = 2024
API_VERSION_MONTH_SUPPORTED_RESPONSE_FORMAT = 8
