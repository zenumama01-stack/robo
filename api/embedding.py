__all__ = ["Embedding"]
class Embedding(BaseModel):
    """Represents an embedding vector returned by embedding endpoint."""
    embedding: List[float]
    """The embedding vector, which is a list of floats.
    The length of vector depends on the model as listed in the
    [embedding guide](https://platform.openai.com/docs/guides/embeddings).
    """The index of the embedding in the list of embeddings."""
    object: Literal["embedding"]
    """The object type, which is always "embedding"."""
Handles embedding calls to Bedrock's `/invoke` endpoint
from typing import Any, Callable, List, Optional, Tuple, Union, get_args
from litellm.constants import BEDROCK_EMBEDDING_PROVIDERS_LITERAL
from litellm.llms.cohere.embed.handler import embedding as cohere_embedding
    _get_httpx_client,
from litellm.types.llms.bedrock import (
    AmazonEmbeddingRequest,
    CohereEmbeddingRequest,
from litellm.types.utils import EmbeddingResponse, LlmProviders
from ..base_aws_llm import BaseAWSLLM
from ..common_utils import BedrockError
from .amazon_nova_transformation import AmazonNovaEmbeddingConfig
from .amazon_titan_g1_transformation import AmazonTitanG1Config
from .amazon_titan_multimodal_transformation import (
    AmazonTitanMultimodalEmbeddingG1Config,
from .amazon_titan_v2_transformation import AmazonTitanV2Config
from .cohere_transformation import BedrockCohereEmbeddingConfig
from .twelvelabs_marengo_transformation import TwelveLabsMarengoEmbeddingConfig
class BedrockEmbedding(BaseAWSLLM):
    def _load_credentials(
    ) -> Tuple[Any, str]:
            from botocore.credentials import Credentials
            raise ImportError("Missing boto3 to call bedrock. Run 'pip install boto3'.")
        ## CREDENTIALS ##
        # pop aws_secret_access_key, aws_access_key_id, aws_session_token, aws_region_name from kwargs, since completion calls fail with them
        aws_secret_access_key = optional_params.pop("aws_secret_access_key", None)
        aws_access_key_id = optional_params.pop("aws_access_key_id", None)
        aws_session_token = optional_params.pop("aws_session_token", None)
        aws_region_name = optional_params.pop("aws_region_name", None)
        aws_role_name = optional_params.pop("aws_role_name", None)
        aws_session_name = optional_params.pop("aws_session_name", None)
        aws_profile_name = optional_params.pop("aws_profile_name", None)
        aws_web_identity_token = optional_params.pop("aws_web_identity_token", None)
        aws_sts_endpoint = optional_params.pop("aws_sts_endpoint", None)
        ### SET REGION NAME ###
        if aws_region_name is None:
            # check env #
            litellm_aws_region_name = get_secret("AWS_REGION_NAME", None)
            if litellm_aws_region_name is not None and isinstance(
                litellm_aws_region_name, str
                aws_region_name = litellm_aws_region_name
            standard_aws_region_name = get_secret("AWS_REGION", None)
            if standard_aws_region_name is not None and isinstance(
                standard_aws_region_name, str
                aws_region_name = standard_aws_region_name
                aws_region_name = "us-west-2"
        credentials: Credentials = self.get_credentials(  # type: ignore
            aws_access_key_id=aws_access_key_id,
            aws_secret_access_key=aws_secret_access_key,
            aws_session_token=aws_session_token,
            aws_session_name=aws_session_name,
            aws_profile_name=aws_profile_name,
            aws_role_name=aws_role_name,
            aws_web_identity_token=aws_web_identity_token,
            aws_sts_endpoint=aws_sts_endpoint,
        return credentials, aws_region_name
    async def async_embeddings(self):
    def _make_sync_call(
        if client is None or not isinstance(client, HTTPHandler):
                    timeout = httpx.Timeout(timeout)
                _params["timeout"] = timeout
            client = _get_httpx_client(_params)  # type: ignore
            client = client
            response = client.post(url=api_base, headers=headers, data=json.dumps(data))  # type: ignore
        except httpx.HTTPStatusError as err:
            error_code = err.response.status_code
            raise BedrockError(status_code=error_code, message=err.response.text)
            raise BedrockError(status_code=408, message="Timeout error occurred.")
    async def _make_async_call(
        if client is None or not isinstance(client, AsyncHTTPHandler):
            client = get_async_httpx_client(
                params=_params, llm_provider=litellm.LlmProviders.BEDROCK
            response = await client.post(url=api_base, headers=headers, data=json.dumps(data))  # type: ignore
    def _transform_response(
        response_list: List[dict],
        provider: BEDROCK_EMBEDDING_PROVIDERS_LITERAL,
        is_async_invoke: Optional[bool] = False,
    ) -> Optional[EmbeddingResponse]:
        Transforms the response from the Bedrock embedding provider to the OpenAI format.
        returned_response: Optional[EmbeddingResponse] = None
        # Handle async invoke responses (single response with invocationArn)
            is_async_invoke
            and len(response_list) == 1
            and "invocationArn" in response_list[0]
            if provider == "twelvelabs":
                returned_response = (
                    TwelveLabsMarengoEmbeddingConfig()._transform_async_invoke_response(
                        response=response_list[0], model=model
            elif provider == "nova":
                    AmazonNovaEmbeddingConfig()._transform_async_invoke_response(
                # For other providers, create a generic async response
                invocation_arn = response_list[0].get("invocationArn", "")
                from litellm.types.utils import Embedding, Usage
                embedding = Embedding(
                    embedding=[],
                    object="embedding",  # Must be literal "embedding"
                usage = Usage(prompt_tokens=0, total_tokens=0)
                # Create hidden params with job ID
                from litellm.types.llms.base import HiddenParams
                hidden_params = HiddenParams()
                setattr(hidden_params, "_invocation_arn", invocation_arn)
                returned_response = EmbeddingResponse(
                    data=[embedding],
                    hidden_params=hidden_params,
            # Handle regular invoke responses
            if model == "amazon.titan-embed-image-v1":
                    AmazonTitanMultimodalEmbeddingG1Config()._transform_response(
                        response_list=response_list, model=model
            elif model == "amazon.titan-embed-text-v1":
                returned_response = AmazonTitanG1Config()._transform_response(
            elif model == "amazon.titan-embed-text-v2:0":
                returned_response = AmazonTitanV2Config()._transform_response(
            elif provider == "twelvelabs":
                    TwelveLabsMarengoEmbeddingConfig()._transform_response(
                returned_response = AmazonNovaEmbeddingConfig()._transform_response(
        ##########################################################
        # Validate returned response
        if returned_response is None:
                "Unable to map model response to known provider format. model={}".format(
        return returned_response
    def _single_func_embeddings(
        batch_data: List[dict],
        credentials: Any,
        extra_headers: Optional[dict],
        endpoint_url: str,
        aws_region_name: str,
        responses: List[dict] = []
        for data in batch_data:
            headers = {"Content-Type": "application/json"}
                headers = {"Content-Type": "application/json", **extra_headers}
            prepped = self.get_request_headers(  # type: ignore  # type: ignore
                endpoint_url=endpoint_url,
                input=data,
                api_key="",
                    "api_base": prepped.url,
                    "headers": prepped.headers,
            headers_for_request = dict(prepped.headers) if hasattr(prepped, 'headers') else {}
            response = self._make_sync_call(
                api_base=prepped.url,
                headers=headers_for_request,
            responses.append(response)
        return self._transform_response(
            response_list=responses,
            is_async_invoke=is_async_invoke,
    async def _async_single_func_embeddings(
            # Convert CaseInsensitiveDict to regular dict for httpx compatibility
            # This ensures custom headers are properly forwarded, especially with IAM roles and custom api_base
            response = await self._make_async_call(
        ## TRANSFORM RESPONSE ##
    def embeddings(  # noqa: PLR0915
        input: List[str],
        client: Optional[Union[HTTPHandler, AsyncHTTPHandler]],
        aembedding: Optional[bool],
        credentials, aws_region_name = self._load_credentials(optional_params)
        ### TRANSFORMATION ###
        unencoded_model_id = (
            optional_params.pop("model_id", None) or model
        )  # default to model if not passed
        modelId = urllib.parse.quote(unencoded_model_id, safe="")
        aws_region_name = self._get_aws_region_name(
            optional_params={"aws_region_name": aws_region_name},
            model_id=unencoded_model_id,
        # Check async invoke needs to be used
        has_async_invoke = "async_invoke/" in model
        if has_async_invoke:
            model = model.replace("async_invoke/", "", 1)
        provider = self.get_bedrock_embedding_provider(model)
                f"Unable to determine bedrock embedding provider for model: {model}. "
                f"Supported providers: {list(get_args(BEDROCK_EMBEDDING_PROVIDERS_LITERAL))}"
        inference_params = copy.deepcopy(optional_params)
        inference_params = {
            for k, v in inference_params.items()
            if k.lower() not in self.aws_authentication_params
        inference_params.pop(
            "user", None
        )  # make sure user is not passed in for bedrock call
        data: Optional[CohereEmbeddingRequest] = None
        batch_data: Optional[List] = None
            data = BedrockCohereEmbeddingConfig()._transform_request(
                model=model, input=input, inference_params=inference_params
        elif provider == "amazon" and model in [
            "amazon.titan-embed-text-v2:0",
            batch_data = []
            for i in input:
                    transformed_request: (
                        AmazonEmbeddingRequest
                    ) = AmazonTitanMultimodalEmbeddingG1Config()._transform_request(
                        input=i, inference_params=inference_params
                    transformed_request = AmazonTitanG1Config()._transform_request(
                    transformed_request = AmazonTitanV2Config()._transform_request(
                        "Unmapped model. Received={}. Expected={}".format(
                batch_data.append(transformed_request)
                twelvelabs_request = (
                    TwelveLabsMarengoEmbeddingConfig()._transform_request(
                        input=i,
                        inference_params=inference_params,
                        async_invoke_route=has_async_invoke,
                        model_id=modelId,
                        output_s3_uri=inference_params.get("output_s3_uri"),
                batch_data.append(twelvelabs_request)
                nova_request = AmazonNovaEmbeddingConfig()._transform_request(
                batch_data.append(nova_request)
        ### SET RUNTIME ENDPOINT ###
        endpoint_url, proxy_endpoint_url = self.get_runtime_endpoint(
            aws_bedrock_runtime_endpoint=optional_params.pop(
                "aws_bedrock_runtime_endpoint", None
            endpoint_url = f"{endpoint_url}/async-invoke"
            endpoint_url = f"{endpoint_url}/model/{modelId}/invoke"
        if batch_data is not None:
            if aembedding:
                return self._async_single_func_embeddings(  # type: ignore
                        if client is not None and isinstance(client, AsyncHTTPHandler)
                    batch_data=batch_data,
                    is_async_invoke=has_async_invoke,
            returned_response = self._single_func_embeddings(
                    if client is not None and isinstance(client, HTTPHandler)
                raise Exception("Unable to map Bedrock request to provider")
        elif data is None:
        prepped = self.get_request_headers(  # type: ignore
        ## ROUTING ##
        return cohere_embedding(
            data=data,  # type: ignore
            complete_api_base=prepped.url,
    async def _get_async_invoke_status(
        self, invocation_arn: str, aws_region_name: str, logging_obj=None, **kwargs
        Get the status of an async invoke job using the GetAsyncInvoke operation.
            invocation_arn: The invocation ARN from the async invoke response
            **kwargs: Additional parameters (credentials, etc.)
            dict: Status response from AWS Bedrock
        # Get AWS credentials using the same method as other Bedrock methods
        credentials, _ = self._load_credentials(kwargs)
        # Get the runtime endpoint
        endpoint_url, _ = self.get_runtime_endpoint(
            api_base=None,
            aws_bedrock_runtime_endpoint=kwargs.get("aws_bedrock_runtime_endpoint"),
        # Encode the ARN for use in URL path
        encoded_arn = quote(invocation_arn, safe="")
        status_url = f"{endpoint_url.rstrip('/')}/async-invoke/{encoded_arn}"
        # Prepare headers for GET request
        # Use AWSRequest directly for GET requests (get_request_headers hardcodes POST)
            from botocore.auth import SigV4Auth
            from botocore.awsrequest import AWSRequest
                "Missing boto3 to call bedrock. Run 'pip install boto3'."
        # Create AWSRequest with GET method and encoded URL
        request = AWSRequest(
            url=status_url,
            data=None,  # GET request, no body
        # Sign the request - SigV4Auth will create canonical string from request URL
        sigv4 = SigV4Auth(credentials, "bedrock", aws_region_name)
        sigv4.add_auth(request)
        # Prepare the request
        # LOGGING
        if logging_obj is not None:
            # Create custom curl command for GET request
            masked_headers = logging_obj._get_masked_headers(prepped.headers)
            formatted_headers = " ".join(
                [f"-H '{k}: {v}'" for k, v in masked_headers.items()]
            custom_curl = "\n\nGET Request Sent from LiteLLM:\n"
            custom_curl += "curl -X GET \\\n"
            custom_curl += f"{prepped.url} \\\n"
            custom_curl += f"{formatted_headers}\n"
                input=invocation_arn,
                    "complete_input_dict": {"invocation_arn": invocation_arn},
                    "request_str": custom_curl,  # Override with custom GET curl command
        # Make the GET request
        client = get_async_httpx_client(llm_provider=LlmProviders.BEDROCK)
        response = await client.get(
            url=prepped.url,
            headers=prepped.headers,
                    "complete_input_dict": {"invocation_arn": invocation_arn}
        # Parse response
                f"Failed to get async invoke status: {response.status_code} - {response.text}"
Calls handled in openai/
as mistral is an openai-compatible endpoint.
class EmbeddingRequest(BaseModel):
    input: List[str] = []
    timeout: int = 600
    api_type: Optional[str] = None
    caching: bool = False
    custom_llm_provider: Optional[Union[str, dict]] = None
    litellm_call_id: Optional[str] = None
    litellm_logging_obj: Optional[dict] = None
    logger_fn: Optional[str] = None
