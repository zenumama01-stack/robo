from .._types import SequenceNotStr
from .chat.chat_completion_stream_options_param import ChatCompletionStreamOptionsParam
__all__ = ["CompletionCreateParamsBase", "CompletionCreateParamsNonStreaming", "CompletionCreateParamsStreaming"]
class CompletionCreateParamsBase(TypedDict, total=False):
    model: Required[Union[str, Literal["gpt-3.5-turbo-instruct", "davinci-002", "babbage-002"]]]
    """ID of the model to use.
    You can use the
    prompt: Required[Union[str, SequenceNotStr[str], Iterable[int], Iterable[Iterable[int]], None]]
    The prompt(s) to generate completions for, encoded as a string, array of
    best_of: Optional[int]
    Generates `best_of` completions server-side and returns the "best" (the one with
    echo: Optional[bool]
    """Echo back the prompt in addition to the completion"""
    frequency_penalty: Optional[float]
    """Number between -2.0 and 2.0.
    Positive values penalize new tokens based on their existing frequency in the
    text so far, decreasing the model's likelihood to repeat the same line verbatim.
    logit_bias: Optional[Dict[str, int]]
    """Modify the likelihood of specified tokens appearing in the completion.
    logprobs: Optional[int]
    Include the log probabilities on the `logprobs` most likely output tokens, as
    max_tokens: Optional[int]
    The maximum number of [tokens](/tokenizer) that can be generated in the
    n: Optional[int]
    """How many completions to generate for each prompt.
    presence_penalty: Optional[float]
    Positive values penalize new tokens based on whether they appear in the text so
    far, increasing the model's likelihood to talk about new topics.
    seed: Optional[int]
    If specified, our system will make a best effort to sample deterministically,
    stop: Union[Optional[str], SequenceNotStr[str], None]
    """Not supported with latest reasoning models `o3` and `o4-mini`.
    stream_options: Optional[ChatCompletionStreamOptionsParam]
    """Options for streaming response. Only set this when you set `stream: true`."""
    suffix: Optional[str]
    """The suffix that comes after a completion of inserted text.
    temperature: Optional[float]
    """What sampling temperature to use, between 0 and 2.
    Higher values like 0.8 will make the output more random, while lower values like
    0.2 will make it more focused and deterministic.
    top_p: Optional[float]
    An alternative to sampling with temperature, called nucleus sampling, where the
    user: str
    A unique identifier representing your end-user, which can help OpenAI to monitor
class CompletionCreateParamsNonStreaming(CompletionCreateParamsBase, total=False):
    stream: Optional[Literal[False]]
    """Whether to stream back partial progress.
    If set, tokens will be sent as data-only
class CompletionCreateParamsStreaming(CompletionCreateParamsBase):
    stream: Required[Literal[True]]
CompletionCreateParams = Union[CompletionCreateParamsNonStreaming, CompletionCreateParamsStreaming]
from typing import Dict, List, Union, Iterable, Optional
from .chat_completion_audio_param import ChatCompletionAudioParam
from .chat_completion_message_param import ChatCompletionMessageParam
from .chat_completion_tool_union_param import ChatCompletionToolUnionParam
from ..shared_params.function_parameters import FunctionParameters
from .chat_completion_stream_options_param import ChatCompletionStreamOptionsParam
from .chat_completion_prediction_content_param import ChatCompletionPredictionContentParam
from .chat_completion_tool_choice_option_param import ChatCompletionToolChoiceOptionParam
from .chat_completion_function_call_option_param import ChatCompletionFunctionCallOptionParam
    "CompletionCreateParamsBase",
    "FunctionCall",
    "ResponseFormat",
    "WebSearchOptions",
    "WebSearchOptionsUserLocation",
    "WebSearchOptionsUserLocationApproximate",
    "CompletionCreateParamsNonStreaming",
    "CompletionCreateParamsStreaming",
    messages: Required[Iterable[ChatCompletionMessageParam]]
    """A list of messages comprising the conversation so far.
    Depending on the [model](https://platform.openai.com/docs/models) you use,
    different message types (modalities) are supported, like
    """Model ID used to generate the response, like `gpt-4o` or `o3`.
    audio: Optional[ChatCompletionAudioParam]
    Required when audio output is requested with `modalities: ["audio"]`.
    function_call: FunctionCall
    """Deprecated in favor of `tool_choice`.
    functions: Iterable[Function]
    """Deprecated in favor of `tools`.
    logprobs: Optional[bool]
    """Whether to return log probabilities of the output tokens or not.
    If true, returns the log probabilities of each output token returned in the
    `content` of `message`.
    An upper bound for the number of tokens that can be generated for a completion,
    The maximum number of [tokens](/tokenizer) that can be generated in the chat
    modalities: Optional[List[Literal["text", "audio"]]]
    Output types that you would like the model to generate. Most models are capable
    """How many chat completion choices to generate for each input message.
    Note that you will be charged based on the number of generated tokens across all
    of the choices. Keep `n` as `1` to minimize costs.
    prediction: Optional[ChatCompletionPredictionContentParam]
    prompt_cache_key: str
    Used by OpenAI to cache responses for similar requests to optimize your cache
    prompt_cache_retention: Optional[Literal["in-memory", "24h"]]
    """The retention policy for the prompt cache.
    Set to `24h` to enable extended prompt caching, which keeps cached prefixes
    active for longer, up to a maximum of 24 hours.
    response_format: ResponseFormat
    """An object specifying the format that the model must output.
    safety_identifier: str
    A stable identifier used to help detect users of your application that may be
    This feature is in Beta. If specified, our system will make a best effort to
    service_tier: Optional[Literal["auto", "default", "flex", "scale", "priority"]]
    store: Optional[bool]
    Whether or not to store the output of this chat completion request for use in
    0.2 will make it more focused and deterministic. We generally recommend altering
    this or `top_p` but not both.
    tool_choice: ChatCompletionToolChoiceOptionParam
    tools: Iterable[ChatCompletionToolUnionParam]
    """A list of tools the model may call.
    You can provide either
    top_logprobs: Optional[int]
    An integer between 0 and 20 specifying the number of most likely tokens to
    """This field is being replaced by `safety_identifier` and `prompt_cache_key`.
    Use `prompt_cache_key` instead to maintain caching optimizations. A stable
    verbosity: Optional[Literal["low", "medium", "high"]]
    """Constrains the verbosity of the model's response.
    Lower values will result in more concise responses, while higher values will
    result in more verbose responses. Currently supported values are `low`,
    `medium`, and `high`.
    web_search_options: WebSearchOptions
    This tool searches the web for relevant results to use in a response. Learn more
FunctionCall: TypeAlias = Union[Literal["none", "auto"], ChatCompletionFunctionCallOptionParam]
    """The name of the function to be called.
    Must be a-z, A-Z, 0-9, or contain underscores and dashes, with a maximum length
    of 64.
    A description of what the function does, used by the model to choose when and
    how to call the function.
    parameters: FunctionParameters
    """The parameters the functions accepts, described as a JSON Schema object.
    See the [guide](https://platform.openai.com/docs/guides/function-calling) for
    examples, and the
    [JSON Schema reference](https://json-schema.org/understanding-json-schema/) for
    documentation about the format.
    Omitting `parameters` defines a function with an empty parameter list.
ResponseFormat: TypeAlias = Union[ResponseFormatText, ResponseFormatJSONSchema, ResponseFormatJSONObject]
class WebSearchOptionsUserLocationApproximate(TypedDict, total=False):
    """Approximate location parameters for the search."""
    """Free text input for the city of the user, e.g. `San Francisco`."""
    The two-letter [ISO country code](https://en.wikipedia.org/wiki/ISO_3166-1) of
    the user, e.g. `US`.
    region: str
    """Free text input for the region of the user, e.g. `California`."""
    timezone: str
    The [IANA timezone](https://timeapi.io/documentation/iana-timezones) of the
    user, e.g. `America/Los_Angeles`.
class WebSearchOptionsUserLocation(TypedDict, total=False):
    approximate: Required[WebSearchOptionsUserLocationApproximate]
    type: Required[Literal["approximate"]]
    """The type of location approximation. Always `approximate`."""
class WebSearchOptions(TypedDict, total=False):
    This tool searches the web for relevant results to use in a response.
    Learn more about the [web search tool](https://platform.openai.com/docs/guides/tools-web-search?api-mode=chat).
    search_context_size: Literal["low", "medium", "high"]
    High level guidance for the amount of context window space to use for the
    search. One of `low`, `medium`, or `high`. `medium` is the default.
    user_location: Optional[WebSearchOptionsUserLocation]
