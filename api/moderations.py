from typing import Union, Iterable
from ..types import moderation_create_params
from .._utils import maybe_transform, async_maybe_transform
from ..types.moderation_model import ModerationModel
from ..types.moderation_create_response import ModerationCreateResponse
from ..types.moderation_multi_modal_input_param import ModerationMultiModalInputParam
__all__ = ["Moderations", "AsyncModerations"]
class Moderations(SyncAPIResource):
    def with_raw_response(self) -> ModerationsWithRawResponse:
        return ModerationsWithRawResponse(self)
    def with_streaming_response(self) -> ModerationsWithStreamingResponse:
        return ModerationsWithStreamingResponse(self)
        input: Union[str, SequenceNotStr[str], Iterable[ModerationMultiModalInputParam]],
        model: Union[str, ModerationModel] | Omit = omit,
    ) -> ModerationCreateResponse:
        """Classifies if text and/or image inputs are potentially harmful.
        Learn more in
        the [moderation guide](https://platform.openai.com/docs/guides/moderation).
          input: Input (or inputs) to classify. Can be a single string, an array of strings, or
              an array of multi-modal input objects similar to other models.
          model: The content moderation model you would like to use. Learn more in
              [the moderation guide](https://platform.openai.com/docs/guides/moderation), and
              learn about available models
              [here](https://platform.openai.com/docs/models#moderation).
            "/moderations",
                moderation_create_params.ModerationCreateParams,
            cast_to=ModerationCreateResponse,
class AsyncModerations(AsyncAPIResource):
    def with_raw_response(self) -> AsyncModerationsWithRawResponse:
        return AsyncModerationsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncModerationsWithStreamingResponse:
        return AsyncModerationsWithStreamingResponse(self)
class ModerationsWithRawResponse:
    def __init__(self, moderations: Moderations) -> None:
        self._moderations = moderations
            moderations.create,
class AsyncModerationsWithRawResponse:
    def __init__(self, moderations: AsyncModerations) -> None:
class ModerationsWithStreamingResponse:
class AsyncModerationsWithStreamingResponse:
OpenAI Moderation Guardrail Integration for LiteLLM
from litellm.integrations.custom_guardrail import (
    CustomGuardrail,
    log_guardrail_information,
from litellm.types.utils import GenericGuardrailAPIInputs
from .base import OpenAIGuardrailBase
    from litellm.proxy._types import UserAPIKeyAuth
    from litellm.types.llms.openai import OpenAIModerationResponse
    from litellm.types.proxy.guardrails.guardrail_hooks.base import GuardrailConfigModel
    from litellm.types.utils import ModelResponse, ModelResponseStream
class OpenAIModerationGuardrail(OpenAIGuardrailBase, CustomGuardrail):
    LiteLLM Built-in Guardrail for OpenAI Content Moderation.
    This guardrail scans prompts and responses using the OpenAI Moderation API to detect
    harmful content, including violence, hate, harassment, self-harm, sexual content, etc.
    Configuration:
        guardrail_name: Name of the guardrail instance
        api_key: OpenAI API key
        api_base: OpenAI API endpoint
        model: OpenAI moderation model to use
        default_on: Whether to enable by default
        model: Optional[Literal["omni-moderation-latest", "text-moderation-latest"]] = None,
        """Initialize OpenAI Moderation guardrail handler."""
        # Initialize parent CustomGuardrail
        supported_event_hooks = [
            GuardrailEventHooks.pre_call,
            GuardrailEventHooks.during_call,
            GuardrailEventHooks.post_call,
            supported_event_hooks=supported_event_hooks,
        self.async_handler = get_async_httpx_client(
            llm_provider=httpxSpecialProvider.GuardrailCallback
        # Store configuration
        self.api_key = api_key or self._get_api_key()
        self.api_base = api_base or "https://api.openai.com/v1"
        self.model: Literal["omni-moderation-latest", "text-moderation-latest"] = model or "omni-moderation-latest"
        if not self.api_key:
            raise ValueError("OpenAI Moderation: api_key is required. Set OPENAI_API_KEY environment variable or pass it in configuration.")
            f"Initialized OpenAI Moderation Guardrail: {guardrail_name} with model: {self.model}"
    def _get_api_key(self) -> Optional[str]:
        """Get API key from environment variables or litellm configuration"""
            os.environ.get("OPENAI_API_KEY")
    async def async_make_request(
        self, input_text: str
    ) -> "OpenAIModerationResponse":
        Make a request to the OpenAI Moderation API.
        request_body = {
            "input": input_text
            "OpenAI Moderation guard request: %s", request_body
        response = await self.async_handler.post(
            url=f"{self.api_base}/moderations",
                "Authorization": f"Bearer {self.api_key}",
            json=request_body,
            "OpenAI Moderation guard response: %s", response.json()
        if response.status_code != 200:
                status_code=response.status_code,
                    "error": "OpenAI Moderation API request failed",
                    "details": response.text,
        return OpenAIModerationResponse(**response.json())
    def _check_moderation_result(self, moderation_response: "OpenAIModerationResponse") -> None:
        Check if the moderation response indicates harmful content and raise exception if needed.
        if not moderation_response.results:
        result = moderation_response.results[0]
        if result.flagged:
            # Build detailed violation information
            violated_categories = []
            if result.categories:
                for category, is_violated in result.categories.items():
                    if is_violated:
                        violated_categories.append(category)
            violation_details = {
                "violated_categories": violated_categories,
                "category_scores": result.category_scores or {},
                "OpenAI Moderation: Content flagged for violations: %s", 
                violation_details
                    "error": "Violated OpenAI moderation policy",
                    "moderation_result": violation_details,
    async def apply_guardrail(
        inputs: GenericGuardrailAPIInputs,
        input_type: Literal["request", "response"],
        logging_obj: Optional["LiteLLMLoggingObj"] = None,
    ) -> GenericGuardrailAPIInputs:
        Apply OpenAI moderation guardrail using the unified guardrail interface.
        This method is called by the UnifiedLLMGuardrails system for all endpoint types
        (chat completions, embeddings, responses API, etc.).
            inputs: GenericGuardrailAPIInputs containing texts and/or structured_messages
            request_data: The original request data
            input_type: Whether this is a "request" (pre-call) or "response" (post-call)
            logging_obj: Optional logging object
            The inputs unchanged (moderation doesn't modify content, only blocks)
            HTTPException: If content violates moderation policy
        # Extract text to moderate from inputs
        text_to_moderate: Optional[str] = None
        # Prefer structured_messages if available (has role context)
        if structured_messages := inputs.get("structured_messages"):
            text_to_moderate = self.get_user_prompt(structured_messages)
        # Fall back to texts
        if not text_to_moderate:
            if texts := inputs.get("texts"):
                # Join all texts for moderation
                text_to_moderate = "\n".join(texts)
                "OpenAI Moderation: No text content to moderate in inputs"
        # Make moderation request
        moderation_response = await self.async_make_request(input_text=text_to_moderate)
        # Check if content is flagged and raise exception if needed
        self._check_moderation_result(moderation_response)
        # Moderation doesn't modify content, just blocks - return inputs unchanged
    @log_guardrail_information
        request_data: Dict[str, Any],
    ) -> AsyncGenerator["ModelResponseStream", None]:
        Process streaming response chunks for OpenAI moderation.
        Collects all chunks from the stream, assembles them into a complete response,
        and applies moderation check. If content violates moderation policy, raises HTTPException.
        from litellm.llms.base_llm.base_model_iterator import MockResponseIterator
        from litellm.main import stream_chunk_builder
        from litellm.types.utils import TextCompletionResponse
            "OpenAI Moderation: Running streaming response scan"
        # Collect all chunks to process them together
        all_chunks: List["ModelResponseStream"] = []
            all_chunks.append(chunk)
        # Assemble the complete response from chunks
        assembled_model_response: Optional[
            Union["ModelResponse", TextCompletionResponse]
        ] = stream_chunk_builder(
            chunks=all_chunks,
        if isinstance(assembled_model_response, (type(None), TextCompletionResponse)):
            # If we can't assemble a ModelResponse or it's a text completion, 
            # just yield the original chunks without moderation
                "OpenAI Moderation: Could not assemble ModelResponse from chunks, skipping moderation"
            for chunk in all_chunks:
        # Extract response text for moderation
        response_text = self._extract_response_text(assembled_model_response)
        if response_text:
                f"OpenAI Moderation: Streaming response text: {response_text[:100]}..."  # Log first 100 chars
            # Make moderation request - this will raise HTTPException if content is flagged
            moderation_response = await self.async_make_request(
                input_text=response_text,
        # If we reach here, content passed moderation - yield the original chunks
        mock_response = MockResponseIterator(
            model_response=assembled_model_response
        # Return the reconstructed stream
        async for chunk in mock_response:
    def _extract_response_text(self, response: "ModelResponse") -> Optional[str]:
        Extract text content from the model response for moderation.
        if not hasattr(response, 'choices') or not response.choices:
        response_texts = []
        for choice in response.choices:
                # Try to get content from message (chat completion)
                message = getattr(choice, 'message', None)
                    content = getattr(message, 'content', None)
                    if content and isinstance(content, str):
                        response_texts.append(content)
                # Try to get text (text completion)
                text = getattr(choice, 'text', None)
                if text and isinstance(text, str):
                    response_texts.append(text)
                # Try to get content from delta (streaming)
                delta = getattr(choice, 'delta', None)
                if delta:
                    content = getattr(delta, 'content', None)
                # Skip choices that don't have expected attributes
        return "\n".join(response_texts) if response_texts else None
    def get_config_model() -> Optional[Type["GuardrailConfigModel"]]:
        Get the config model for the OpenAI Moderation guardrail.
        from litellm.types.proxy.guardrails.guardrail_hooks.openai.openai_moderation import (
            OpenAIModerationGuardrailConfigModel,
        return OpenAIModerationGuardrailConfigModel
