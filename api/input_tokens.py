from ...types.responses import input_token_count_params
from ...types.responses.tool_param import ToolParam
from ...types.shared_params.reasoning import Reasoning
from ...types.responses.input_token_count_response import InputTokenCountResponse
__all__ = ["InputTokens", "AsyncInputTokens"]
class InputTokens(SyncAPIResource):
    def with_raw_response(self) -> InputTokensWithRawResponse:
        return InputTokensWithRawResponse(self)
    def with_streaming_response(self) -> InputTokensWithStreamingResponse:
        return InputTokensWithStreamingResponse(self)
    def count(
        conversation: Optional[input_token_count_params.Conversation] | Omit = omit,
        input: Union[str, Iterable[ResponseInputItemParam], None] | Omit = omit,
        model: Optional[str] | Omit = omit,
        parallel_tool_calls: Optional[bool] | Omit = omit,
        previous_response_id: Optional[str] | Omit = omit,
        reasoning: Optional[Reasoning] | Omit = omit,
        text: Optional[input_token_count_params.Text] | Omit = omit,
        tool_choice: Optional[input_token_count_params.ToolChoice] | Omit = omit,
        tools: Optional[Iterable[ToolParam]] | Omit = omit,
        truncation: Literal["auto", "disabled"] | Omit = omit,
    ) -> InputTokenCountResponse:
        Returns input token counts of the request.
        Returns an object with `object` set to `response.input_tokens` and an
        `input_tokens` count.
          conversation: The conversation that this response belongs to. Items from this conversation are
              prepended to `input_items` for this response request. Input items and output
              items from this response are automatically added to this conversation after this
              response completes.
          input: Text, image, or file inputs to the model, used to generate a response
          instructions: A system (or developer) message inserted into the model's context. When used
              along with `previous_response_id`, the instructions from a previous response
              will not be carried over to the next response. This makes it simple to swap out
              system (or developer) messages in new responses.
          parallel_tool_calls: Whether to allow the model to run tool calls in parallel.
          previous_response_id: The unique ID of the previous response to the model. Use this to create
              multi-turn conversations. Learn more about
              [conversation state](https://platform.openai.com/docs/guides/conversation-state).
              Cannot be used in conjunction with `conversation`.
          reasoning: **gpt-5 and o-series models only** Configuration options for
              [reasoning models](https://platform.openai.com/docs/guides/reasoning).
          text: Configuration options for a text response from the model. Can be plain text or
              structured JSON data. Learn more:
              - [Text inputs and outputs](https://platform.openai.com/docs/guides/text)
              - [Structured Outputs](https://platform.openai.com/docs/guides/structured-outputs)
          tool_choice: Controls which tool the model should use, if any.
          tools: An array of tools the model may call while generating a response. You can
              specify which tool to use by setting the `tool_choice` parameter.
          truncation: The truncation strategy to use for the model response. - `auto`: If the input to
              this Response exceeds the model's context window size, the model will truncate
              the response to fit the context window by dropping items from the beginning of
              the conversation. - `disabled` (default): If the input size will exceed the
              context window size for a model, the request will fail with a 400 error.
            "/responses/input_tokens",
                    "conversation": conversation,
                    "previous_response_id": previous_response_id,
                    "reasoning": reasoning,
                    "text": text,
                input_token_count_params.InputTokenCountParams,
            cast_to=InputTokenCountResponse,
class AsyncInputTokens(AsyncAPIResource):
    def with_raw_response(self) -> AsyncInputTokensWithRawResponse:
        return AsyncInputTokensWithRawResponse(self)
    def with_streaming_response(self) -> AsyncInputTokensWithStreamingResponse:
        return AsyncInputTokensWithStreamingResponse(self)
    async def count(
class InputTokensWithRawResponse:
    def __init__(self, input_tokens: InputTokens) -> None:
        self._input_tokens = input_tokens
        self.count = _legacy_response.to_raw_response_wrapper(
            input_tokens.count,
class AsyncInputTokensWithRawResponse:
    def __init__(self, input_tokens: AsyncInputTokens) -> None:
        self.count = _legacy_response.async_to_raw_response_wrapper(
class InputTokensWithStreamingResponse:
        self.count = to_streamed_response_wrapper(
class AsyncInputTokensWithStreamingResponse:
        self.count = async_to_streamed_response_wrapper(
        Get input token counts
