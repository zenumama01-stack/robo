from typing import TYPE_CHECKING, Optional, cast
from openai.types.completion import Completion
from .._utils import get_client
from ..._types import Omittable, omit
from ..._utils import is_given
from .._errors import CLIError
from ..._streaming import Stream
    sub = subparser.add_parser("completions.create")
    sub.add_argument(
        "--model",
        help="The model to use",
    sub.add_argument("-p", "--prompt", help="An optional prompt to complete from")
    sub.add_argument("--stream", help="Stream tokens as they're ready.", action="store_true")
    sub.add_argument("-M", "--max-tokens", help="The maximum number of tokens to generate", type=int)
        "--temperature",
        help="""What sampling temperature to use. Higher values means the model will take more risks. Try 0.9 for more creative applications, and 0 (argmax sampling) for ones with a well-defined answer.
Mutually exclusive with `top_p`.""",
        type=float,
        "-P",
        "--top_p",
        help="""An alternative to sampling with temperature, called nucleus sampling, where the considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10%% probability mass are considered.
            Mutually exclusive with `temperature`.""",
        "-n",
        "--n",
        help="How many sub-completions to generate for each prompt.",
        "--logprobs",
        help="Include the log probabilities on the `logprobs` most likely tokens, as well the chosen tokens. So for example, if `logprobs` is 10, the API will return a list of the 10 most likely tokens. If `logprobs` is 0, only the chosen tokens will have logprobs returned.",
        "--best_of",
        help="Generates `best_of` completions server-side and returns the 'best' (the one with the highest log probability per token). Results cannot be streamed.",
        "--echo",
        help="Echo back the prompt in addition to the completion",
        "--frequency_penalty",
        help="Positive values penalize new tokens based on their existing frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.",
        "--presence_penalty",
        help="Positive values penalize new tokens based on whether they appear in the text so far, increasing the model's likelihood to talk about new topics.",
    sub.add_argument("--suffix", help="The suffix that comes after a completion of inserted text.")
    sub.add_argument("--stop", help="A stop sequence at which to stop generating tokens.")
        "--user",
        help="A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.",
    # TODO: add support for logit_bias
    sub.set_defaults(func=CLICompletions.create, args_model=CLICompletionCreateArgs)
class CLICompletionCreateArgs(BaseModel):
    stream: bool = False
    n: Omittable[int] = omit
    stop: Omittable[str] = omit
    user: Omittable[str] = omit
    echo: Omittable[bool] = omit
    suffix: Omittable[str] = omit
    best_of: Omittable[int] = omit
    top_p: Omittable[float] = omit
    logprobs: Omittable[int] = omit
    max_tokens: Omittable[int] = omit
    temperature: Omittable[float] = omit
    presence_penalty: Omittable[float] = omit
    frequency_penalty: Omittable[float] = omit
class CLICompletions:
    def create(args: CLICompletionCreateArgs) -> None:
        if is_given(args.n) and args.n > 1 and args.stream:
            raise CLIError("Can't stream completions with n>1 with the current CLI")
        make_request = partial(
            get_client().completions.create,
            n=args.n,
            echo=args.echo,
            stop=args.stop,
            user=args.user,
            top_p=args.top_p,
            prompt=args.prompt,
            suffix=args.suffix,
            best_of=args.best_of,
            logprobs=args.logprobs,
            max_tokens=args.max_tokens,
            temperature=args.temperature,
            presence_penalty=args.presence_penalty,
            frequency_penalty=args.frequency_penalty,
        if args.stream:
            return CLICompletions._stream_create(
                # mypy doesn't understand the `partial` function but pyright does
                cast(Stream[Completion], make_request(stream=True))  # pyright: ignore[reportUnnecessaryCast]
        return CLICompletions._create(make_request())
    def _create(completion: Completion) -> None:
        should_print_header = len(completion.choices) > 1
        for choice in completion.choices:
            if should_print_header:
                sys.stdout.write("===== Completion {} =====\n".format(choice.index))
            sys.stdout.write(choice.text)
            if should_print_header or not choice.text.endswith("\n"):
    def _stream_create(stream: Stream[Completion]) -> None:
        for completion in stream:
            for choice in sorted(completion.choices, key=lambda c: c.index):
                    sys.stdout.write("===== Chat Completion {} =====\n".format(choice.index))
from typing import TYPE_CHECKING, List, Optional, cast
from typing_extensions import Literal, NamedTuple
from ..._utils import get_client
from ..._models import BaseModel
from ...._streaming import Stream
from ....types.chat import (
    ChatCompletionRole,
    ChatCompletionChunk,
    CompletionCreateParams,
from ....types.chat.completion_create_params import (
    CompletionCreateParamsStreaming,
    CompletionCreateParamsNonStreaming,
    sub = subparser.add_parser("chat.completions.create")
    sub._action_groups.pop()
    req = sub.add_argument_group("required arguments")
    opt = sub.add_argument_group("optional arguments")
    req.add_argument(
        "-g",
        "--message",
        action="append",
        nargs=2,
        metavar=("ROLE", "CONTENT"),
        help="A message in `{role} {content}` format. Use this argument multiple times to add multiple messages.",
        help="The model to use.",
    opt.add_argument(
        help="How many completions to generate for the conversation.",
    opt.add_argument("-M", "--max-tokens", help="The maximum number of tokens to generate.", type=int)
        "--stop",
        help="A stop sequence at which to stop generating tokens for the message.",
    opt.add_argument("--stream", help="Stream messages as they're ready.", action="store_true")
    sub.set_defaults(func=CLIChatCompletion.create, args_model=CLIChatCompletionCreateArgs)
class CLIMessage(NamedTuple):
    role: ChatCompletionRole
    content: str
class CLIChatCompletionCreateArgs(BaseModel):
    message: List[CLIMessage]
    n: Optional[int] = None
    max_tokens: Optional[int] = None
    top_p: Optional[float] = None
    stop: Optional[str] = None
class CLIChatCompletion:
    def create(args: CLIChatCompletionCreateArgs) -> None:
        params: CompletionCreateParams = {
            "model": args.model,
            "messages": [
                {"role": cast(Literal["user"], message.role), "content": message.content} for message in args.message
            # type checkers are not good at inferring union types so we have to set stream afterwards
            "stream": False,
        if args.temperature is not None:
            params["temperature"] = args.temperature
        if args.stop is not None:
            params["stop"] = args.stop
        if args.top_p is not None:
            params["top_p"] = args.top_p
        if args.n is not None:
            params["n"] = args.n
            params["stream"] = args.stream  # type: ignore
        if args.max_tokens is not None:
            params["max_tokens"] = args.max_tokens
            return CLIChatCompletion._stream_create(cast(CompletionCreateParamsStreaming, params))
        return CLIChatCompletion._create(cast(CompletionCreateParamsNonStreaming, params))
    def _create(params: CompletionCreateParamsNonStreaming) -> None:
        completion = get_client().chat.completions.create(**params)
            content = choice.message.content if choice.message.content is not None else "None"
            sys.stdout.write(content)
            if should_print_header or not content.endswith("\n"):
    def _stream_create(params: CompletionCreateParamsStreaming) -> None:
        # cast is required for mypy
        stream = cast(  # pyright: ignore[reportUnnecessaryCast]
            Stream[ChatCompletionChunk], get_client().chat.completions.create(**params)
            should_print_header = len(chunk.choices) > 1
            for choice in chunk.choices:
                content = choice.delta.content or ""
from typing import Dict, Union, Iterable, Optional
from typing_extensions import Literal, overload
from ..types import completion_create_params
from .._types import Body, Omit, Query, Headers, NotGiven, SequenceNotStr, omit, not_given
from .._utils import required_args, maybe_transform, async_maybe_transform
from .._base_client import (
    make_request_options,
from ..types.completion import Completion
from ..types.chat.chat_completion_stream_options_param import ChatCompletionStreamOptionsParam
__all__ = ["Completions", "AsyncCompletions"]
class Completions(SyncAPIResource):
    def with_raw_response(self) -> CompletionsWithRawResponse:
        return CompletionsWithRawResponse(self)
    def with_streaming_response(self) -> CompletionsWithStreamingResponse:
        return CompletionsWithStreamingResponse(self)
        model: Union[str, Literal["gpt-3.5-turbo-instruct", "davinci-002", "babbage-002"]],
        prompt: Union[str, SequenceNotStr[str], Iterable[int], Iterable[Iterable[int]], None],
        best_of: Optional[int] | Omit = omit,
        echo: Optional[bool] | Omit = omit,
        frequency_penalty: Optional[float] | Omit = omit,
        logit_bias: Optional[Dict[str, int]] | Omit = omit,
        logprobs: Optional[int] | Omit = omit,
        max_tokens: Optional[int] | Omit = omit,
        n: Optional[int] | Omit = omit,
        presence_penalty: Optional[float] | Omit = omit,
        seed: Optional[int] | Omit = omit,
        stop: Union[Optional[str], SequenceNotStr[str], None] | Omit = omit,
        stream: Optional[Literal[False]] | Omit = omit,
        stream_options: Optional[ChatCompletionStreamOptionsParam] | Omit = omit,
        suffix: Optional[str] | Omit = omit,
        temperature: Optional[float] | Omit = omit,
        top_p: Optional[float] | Omit = omit,
        user: str | Omit = omit,
    ) -> Completion:
        Creates a completion for the provided prompt and parameters.
        Returns a completion object, or a sequence of completion objects if the request
        is streamed.
          model: ID of the model to use. You can use the
              [List models](https://platform.openai.com/docs/api-reference/models/list) API to
              see all of your available models, or see our
              [Model overview](https://platform.openai.com/docs/models) for descriptions of
              them.
          prompt: The prompt(s) to generate completions for, encoded as a string, array of
              strings, array of tokens, or array of token arrays.
              Note that <|endoftext|> is the document separator that the model sees during
              training, so if a prompt is not specified the model will generate as if from the
              beginning of a new document.
          best_of: Generates `best_of` completions server-side and returns the "best" (the one with
              the highest log probability per token). Results cannot be streamed.
              When used with `n`, `best_of` controls the number of candidate completions and
              `n` specifies how many to return – `best_of` must be greater than `n`.
              **Note:** Because this parameter generates many completions, it can quickly
              consume your token quota. Use carefully and ensure that you have reasonable
              settings for `max_tokens` and `stop`.
          echo: Echo back the prompt in addition to the completion
          frequency_penalty: Number between -2.0 and 2.0. Positive values penalize new tokens based on their
              existing frequency in the text so far, decreasing the model's likelihood to
              repeat the same line verbatim.
              [See more information about frequency and presence penalties.](https://platform.openai.com/docs/guides/text-generation)
          logit_bias: Modify the likelihood of specified tokens appearing in the completion.
              Accepts a JSON object that maps tokens (specified by their token ID in the GPT
              tokenizer) to an associated bias value from -100 to 100. You can use this
              [tokenizer tool](/tokenizer?view=bpe) to convert text to token IDs.
              Mathematically, the bias is added to the logits generated by the model prior to
              sampling. The exact effect will vary per model, but values between -1 and 1
              should decrease or increase likelihood of selection; values like -100 or 100
              should result in a ban or exclusive selection of the relevant token.
              As an example, you can pass `{"50256": -100}` to prevent the <|endoftext|> token
              from being generated.
          logprobs: Include the log probabilities on the `logprobs` most likely output tokens, as
              well the chosen tokens. For example, if `logprobs` is 5, the API will return a
              list of the 5 most likely tokens. The API will always return the `logprob` of
              the sampled token, so there may be up to `logprobs+1` elements in the response.
              The maximum value for `logprobs` is 5.
          max_tokens: The maximum number of [tokens](/tokenizer) that can be generated in the
              completion.
              The token count of your prompt plus `max_tokens` cannot exceed the model's
              context length.
              [Example Python code](https://cookbook.openai.com/examples/how_to_count_tokens_with_tiktoken)
              for counting tokens.
          n: How many completions to generate for each prompt.
          presence_penalty: Number between -2.0 and 2.0. Positive values penalize new tokens based on
              whether they appear in the text so far, increasing the model's likelihood to
              talk about new topics.
          seed: If specified, our system will make a best effort to sample deterministically,
              such that repeated requests with the same `seed` and parameters should return
              the same result.
              Determinism is not guaranteed, and you should refer to the `system_fingerprint`
              response parameter to monitor changes in the backend.
          stop: Not supported with latest reasoning models `o3` and `o4-mini`.
              Up to 4 sequences where the API will stop generating further tokens. The
              returned text will not contain the stop sequence.
          stream: Whether to stream back partial progress. If set, tokens will be sent as
              data-only
              [server-sent events](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events#Event_stream_format)
              as they become available, with the stream terminated by a `data: [DONE]`
              message.
              [Example Python code](https://cookbook.openai.com/examples/how_to_stream_completions).
          stream_options: Options for streaming response. Only set this when you set `stream: true`.
          suffix: The suffix that comes after a completion of inserted text.
              This parameter is only supported for `gpt-3.5-turbo-instruct`.
          temperature: What sampling temperature to use, between 0 and 2. Higher values like 0.8 will
              make the output more random, while lower values like 0.2 will make it more
              focused and deterministic.
              We generally recommend altering this or `top_p` but not both.
          top_p: An alternative to sampling with temperature, called nucleus sampling, where the
              model considers the results of the tokens with top_p probability mass. So 0.1
              means only the tokens comprising the top 10% probability mass are considered.
              We generally recommend altering this or `temperature` but not both.
          user: A unique identifier representing your end-user, which can help OpenAI to monitor
              and detect abuse.
              [Learn more](https://platform.openai.com/docs/guides/safety-best-practices#end-user-ids).
    ) -> Stream[Completion]:
    ) -> Completion | Stream[Completion]:
    @required_args(["model", "prompt"], ["model", "prompt", "stream"])
        stream: Optional[Literal[False]] | Literal[True] | Omit = omit,
                    "model": model,
                    "prompt": prompt,
                    "best_of": best_of,
                    "echo": echo,
                    "frequency_penalty": frequency_penalty,
                    "logit_bias": logit_bias,
                    "logprobs": logprobs,
                    "max_tokens": max_tokens,
                    "n": n,
                    "presence_penalty": presence_penalty,
                    "seed": seed,
                    "stop": stop,
                    "stream": stream,
                    "stream_options": stream_options,
                    "suffix": suffix,
                    "temperature": temperature,
                    "top_p": top_p,
                    "user": user,
                completion_create_params.CompletionCreateParamsStreaming
                if stream
                else completion_create_params.CompletionCreateParamsNonStreaming,
            cast_to=Completion,
            stream=stream or False,
            stream_cls=Stream[Completion],
class AsyncCompletions(AsyncAPIResource):
    def with_raw_response(self) -> AsyncCompletionsWithRawResponse:
        return AsyncCompletionsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncCompletionsWithStreamingResponse:
        return AsyncCompletionsWithStreamingResponse(self)
    ) -> AsyncStream[Completion]:
    ) -> Completion | AsyncStream[Completion]:
            stream_cls=AsyncStream[Completion],
class CompletionsWithRawResponse:
    def __init__(self, completions: Completions) -> None:
        self._completions = completions
            completions.create,
class AsyncCompletionsWithRawResponse:
    def __init__(self, completions: AsyncCompletions) -> None:
class CompletionsWithStreamingResponse:
class AsyncCompletionsWithStreamingResponse:
from typing import Dict, List, Type, Union, Iterable, Optional, cast
from ...._types import Body, Omit, Query, Headers, NotGiven, SequenceNotStr, omit, not_given
    ChatCompletionAudioParam,
    completion_list_params,
    completion_update_params,
from ....lib._parsing import (
    validate_input_tools as _validate_input_tools,
    parse_chat_completion as _parse_chat_completion,
    type_to_response_format_param as _type_to_response_format,
from ....lib.streaming.chat import ChatCompletionStreamManager, AsyncChatCompletionStreamManager
from ....types.chat.chat_completion import ChatCompletion
from ....types.shared.reasoning_effort import ReasoningEffort
from ....types.chat.chat_completion_chunk import ChatCompletionChunk
from ....types.chat.parsed_chat_completion import ParsedChatCompletion
from ....types.chat.chat_completion_deleted import ChatCompletionDeleted
from ....types.chat.chat_completion_audio_param import ChatCompletionAudioParam
from ....types.chat.chat_completion_message_param import ChatCompletionMessageParam
from ....types.chat.chat_completion_tool_union_param import ChatCompletionToolUnionParam
from ....types.chat.chat_completion_stream_options_param import ChatCompletionStreamOptionsParam
from ....types.chat.chat_completion_prediction_content_param import ChatCompletionPredictionContentParam
from ....types.chat.chat_completion_tool_choice_option_param import ChatCompletionToolChoiceOptionParam
    def parse(
        messages: Iterable[ChatCompletionMessageParam],
        audio: Optional[ChatCompletionAudioParam] | Omit = omit,
        response_format: type[ResponseFormatT] | Omit = omit,
        function_call: completion_create_params.FunctionCall | Omit = omit,
        functions: Iterable[completion_create_params.Function] | Omit = omit,
        logprobs: Optional[bool] | Omit = omit,
        modalities: Optional[List[Literal["text", "audio"]]] | Omit = omit,
        prediction: Optional[ChatCompletionPredictionContentParam] | Omit = omit,
        prompt_cache_key: str | Omit = omit,
        prompt_cache_retention: Optional[Literal["in-memory", "24h"]] | Omit = omit,
        safety_identifier: str | Omit = omit,
        service_tier: Optional[Literal["auto", "default", "flex", "scale", "priority"]] | Omit = omit,
        store: Optional[bool] | Omit = omit,
        tool_choice: ChatCompletionToolChoiceOptionParam | Omit = omit,
        top_logprobs: Optional[int] | Omit = omit,
        verbosity: Optional[Literal["low", "medium", "high"]] | Omit = omit,
        web_search_options: completion_create_params.WebSearchOptions | Omit = omit,
        """Wrapper over the `client.chat.completions.create()` method that provides richer integrations with Python specific types
        & returns a `ParsedChatCompletion` object, which is a subclass of the standard `ChatCompletion` class.
        You can pass a pydantic model to this method and it will automatically convert the model
        into a JSON schema, send it to the API and parse the response content back into the given model.
        This method will also automatically parse `function` tool calls if:
        - You use the `openai.pydantic_function_tool()` helper method
        - You mark your tool schema with `"strict": True`
            print(message.parsed.steps)
        chat_completion_tools = _validate_input_tools(tools)
            "X-Stainless-Helper-Method": "chat.completions.parse",
        def parser(raw_completion: ChatCompletion) -> ParsedChatCompletion[ResponseFormatT]:
            return _parse_chat_completion(
                chat_completion=raw_completion,
                input_tools=chat_completion_tools,
                    "audio": audio,
                    "function_call": function_call,
                    "functions": functions,
                    "prediction": prediction,
                    "prompt_cache_key": prompt_cache_key,
                    "prompt_cache_retention": prompt_cache_retention,
                    "response_format": _type_to_response_format(response_format),
                    "safety_identifier": safety_identifier,
                    "service_tier": service_tier,
                    "store": store,
                    "top_logprobs": top_logprobs,
                    "verbosity": verbosity,
                    "web_search_options": web_search_options,
                completion_create_params.CompletionCreateParams,
            # we turn the `ChatCompletion` instance into a `ParsedChatCompletion`
            # in the `parser` function above
            cast_to=cast(Type[ParsedChatCompletion[ResponseFormatT]], ChatCompletion),
        response_format: completion_create_params.ResponseFormat | Omit = omit,
    ) -> ChatCompletion:
        **Starting a new project?** We recommend trying
        [Responses](https://platform.openai.com/docs/api-reference/responses) to take
        advantage of the latest OpenAI platform features. Compare
        [Chat Completions with Responses](https://platform.openai.com/docs/guides/responses-vs-chat-completions?api-mode=responses).
        Creates a model response for the given chat conversation. Learn more in the
        [text generation](https://platform.openai.com/docs/guides/text-generation),
        [vision](https://platform.openai.com/docs/guides/vision), and
        [audio](https://platform.openai.com/docs/guides/audio) guides.
        Parameter support can differ depending on the model used to generate the
        response, particularly for newer reasoning models. Parameters that are only
        supported for reasoning models are noted below. For the current state of
        unsupported parameters in reasoning models,
        [refer to the reasoning guide](https://platform.openai.com/docs/guides/reasoning).
        Returns a chat completion object, or a streamed sequence of chat completion
        chunk objects if the request is streamed.
          messages: A list of messages comprising the conversation so far. Depending on the
              [model](https://platform.openai.com/docs/models) you use, different message
              types (modalities) are supported, like
              [text](https://platform.openai.com/docs/guides/text-generation),
              [images](https://platform.openai.com/docs/guides/vision), and
              [audio](https://platform.openai.com/docs/guides/audio).
          model: Model ID used to generate the response, like `gpt-4o` or `o3`. OpenAI offers a
              wide range of models with different capabilities, performance characteristics,
              and price points. Refer to the
              [model guide](https://platform.openai.com/docs/models) to browse and compare
              available models.
          audio: Parameters for audio output. Required when audio output is requested with
              `modalities: ["audio"]`.
              [Learn more](https://platform.openai.com/docs/guides/audio).
          function_call: Deprecated in favor of `tool_choice`.
              Controls which (if any) function is called by the model.
              `none` means the model will not call a function and instead generates a message.
              `auto` means the model can pick between generating a message or calling a
              Specifying a particular function via `{"name": "my_function"}` forces the model
              to call that function.
              `none` is the default when no functions are present. `auto` is the default if
              functions are present.
          functions: Deprecated in favor of `tools`.
              A list of functions the model may generate JSON inputs for.
              Accepts a JSON object that maps tokens (specified by their token ID in the
              tokenizer) to an associated bias value from -100 to 100. Mathematically, the
              bias is added to the logits generated by the model prior to sampling. The exact
              effect will vary per model, but values between -1 and 1 should decrease or
              increase likelihood of selection; values like -100 or 100 should result in a ban
              or exclusive selection of the relevant token.
          logprobs: Whether to return log probabilities of the output tokens or not. If true,
              returns the log probabilities of each output token returned in the `content` of
              `message`.
          max_completion_tokens: An upper bound for the number of tokens that can be generated for a completion,
              including visible output tokens and
              [reasoning tokens](https://platform.openai.com/docs/guides/reasoning).
          max_tokens: The maximum number of [tokens](/tokenizer) that can be generated in the chat
              completion. This value can be used to control
              [costs](https://openai.com/api/pricing/) for text generated via API.
              This value is now deprecated in favor of `max_completion_tokens`, and is not
              compatible with
              [o-series models](https://platform.openai.com/docs/guides/reasoning).
          modalities: Output types that you would like the model to generate. Most models are capable
              of generating text, which is the default:
              `["text"]`
              The `gpt-4o-audio-preview` model can also be used to
              [generate audio](https://platform.openai.com/docs/guides/audio). To request that
              this model generate both text and audio responses, you can use:
              `["text", "audio"]`
          n: How many chat completion choices to generate for each input message. Note that
              you will be charged based on the number of generated tokens across all of the
              choices. Keep `n` as `1` to minimize costs.
          prediction: Static predicted output content, such as the content of a text file that is
              being regenerated.
          prompt_cache_key: Used by OpenAI to cache responses for similar requests to optimize your cache
              hit rates. Replaces the `user` field.
              [Learn more](https://platform.openai.com/docs/guides/prompt-caching).
          prompt_cache_retention: The retention policy for the prompt cache. Set to `24h` to enable extended
              prompt caching, which keeps cached prefixes active for longer, up to a maximum
              of 24 hours.
              [Learn more](https://platform.openai.com/docs/guides/prompt-caching#prompt-cache-retention).
          response_format: An object specifying the format that the model must output.
              Setting to `{ "type": "json_object" }` enables the older JSON mode, which
              ensures the message the model generates is valid JSON. Using `json_schema` is
              preferred for models that support it.
          safety_identifier: A stable identifier used to help detect users of your application that may be
              violating OpenAI's usage policies. The IDs should be a string that uniquely
              identifies each user, with a maximum length of 64 characters. We recommend
              hashing their username or email address, in order to avoid sending us any
              identifying information.
              [Learn more](https://platform.openai.com/docs/guides/safety-best-practices#safety-identifiers).
          seed: This feature is in Beta. If specified, our system will make a best effort to
              sample deterministically, such that repeated requests with the same `seed` and
              parameters should return the same result. Determinism is not guaranteed, and you
              should refer to the `system_fingerprint` response parameter to monitor changes
              in the backend.
          service_tier: Specifies the processing type used for serving the request.
              - If set to 'auto', then the request will be processed with the service tier
                configured in the Project settings. Unless otherwise configured, the Project
                will use 'default'.
              - If set to 'default', then the request will be processed with the standard
                pricing and performance for the selected model.
              - If set to '[flex](https://platform.openai.com/docs/guides/flex-processing)' or
                '[priority](https://openai.com/api-priority-processing/)', then the request
                will be processed with the corresponding service tier.
              - When not set, the default behavior is 'auto'.
              When the `service_tier` parameter is set, the response body will include the
              `service_tier` value based on the processing mode actually used to serve the
              request. This response value may be different from the value set in the
          store: Whether or not to store the output of this chat completion request for use in
              our [model distillation](https://platform.openai.com/docs/guides/distillation)
              or [evals](https://platform.openai.com/docs/guides/evals) products.
              Supports text and image inputs. Note: image inputs over 8MB will be dropped.
              [Streaming section below](https://platform.openai.com/docs/api-reference/chat/streaming)
              for more information, along with the
              [streaming responses](https://platform.openai.com/docs/guides/streaming-responses)
              guide for more information on how to handle the streaming events.
              focused and deterministic. We generally recommend altering this or `top_p` but
              not both.
              not call any tool and instead generates a message. `auto` means the model can
              pick between generating a message or calling one or more tools. `required` means
              the model must call one or more tools. Specifying a particular tool via
              `none` is the default when no tools are present. `auto` is the default if tools
              are present.
          tools: A list of tools the model may call. You can provide either
              [custom tools](https://platform.openai.com/docs/guides/function-calling#custom-tools)
              or [function tools](https://platform.openai.com/docs/guides/function-calling).
          top_logprobs: An integer between 0 and 20 specifying the number of most likely tokens to
              return at each token position, each with an associated log probability.
              `logprobs` must be set to `true` if this parameter is used.
          user: This field is being replaced by `safety_identifier` and `prompt_cache_key`. Use
              `prompt_cache_key` instead to maintain caching optimizations. A stable
              identifier for your end-users. Used to boost cache hit rates by better bucketing
              similar requests and to help OpenAI detect and prevent abuse.
          verbosity: Constrains the verbosity of the model's response. Lower values will result in
              more concise responses, while higher values will result in more verbose
              responses. Currently supported values are `low`, `medium`, and `high`.
          web_search_options: This tool searches the web for relevant results to use in a response. Learn more
              about the
              [web search tool](https://platform.openai.com/docs/guides/tools-web-search?api-mode=chat).
    ) -> Stream[ChatCompletionChunk]:
    ) -> ChatCompletion | Stream[ChatCompletionChunk]:
    @required_args(["messages", "model"], ["messages", "model", "stream"])
        validate_response_format(response_format)
            cast_to=ChatCompletion,
            stream_cls=Stream[ChatCompletionChunk],
        completion_id: str,
        """Get a stored chat completion.
        Only Chat Completions that have been created with
        the `store` parameter set to `true` will be returned.
        if not completion_id:
            raise ValueError(f"Expected a non-empty value for `completion_id` but received {completion_id!r}")
            path_template("/chat/completions/{completion_id}", completion_id=completion_id),
        metadata: Optional[Metadata],
        """Modify a stored chat completion.
        Only Chat Completions that have been created
        with the `store` parameter set to `true` can be modified. Currently, the only
        supported modification is to update the `metadata` field.
            body=maybe_transform({"metadata": metadata}, completion_update_params.CompletionUpdateParams),
        model: str | Omit = omit,
    ) -> SyncCursorPage[ChatCompletion]:
        """List stored Chat Completions.
        Only Chat Completions that have been stored with
          after: Identifier for the last chat completion from the previous pagination request.
          limit: Number of Chat Completions to retrieve.
          metadata:
              A list of metadata keys to filter the Chat Completions by. Example:
              `metadata[key1]=value1&metadata[key2]=value2`
          model: The model used to generate the Chat Completions.
          order: Sort order for Chat Completions by timestamp. Use `asc` for ascending order or
              `desc` for descending order. Defaults to `asc`.
            page=SyncCursorPage[ChatCompletion],
                    completion_list_params.CompletionListParams,
            model=ChatCompletion,
    ) -> ChatCompletionDeleted:
        """Delete a stored chat completion.
        with the `store` parameter set to `true` can be deleted.
            cast_to=ChatCompletionDeleted,
        response_format: completion_create_params.ResponseFormat | type[ResponseFormatT] | Omit = omit,
    ) -> ChatCompletionStreamManager[ResponseFormatT]:
        """Wrapper over the `client.chat.completions.create(stream=True)` method that provides a more granular event API
        and automatic accumulation of each delta.
        This also supports all of the parsing utilities that `.parse()` does.
        Unlike `.create(stream=True)`, the `.stream()` method requires usage within a context manager to prevent accidental leakage of the response:
            messages=[...],
        When the context manager is entered, a `ChatCompletionStream` instance is returned which, like `.create(stream=True)` is an iterator. The full list of events that are yielded by the iterator are outlined in [these docs](https://github.com/openai/openai-python/blob/main/helpers.md#chat-completions-events).
        When the context manager exits, the response will be closed, however the `stream` instance is still available outside
        the context manager.
            "X-Stainless-Helper-Method": "chat.completions.stream",
        api_request: partial[Stream[ChatCompletionChunk]] = partial(
            self.create,
            messages=messages,
            audio=audio,
            response_format=_type_to_response_format(response_format),
            frequency_penalty=frequency_penalty,
            function_call=function_call,
            functions=functions,
            logit_bias=logit_bias,
            logprobs=logprobs,
            max_tokens=max_tokens,
            modalities=modalities,
            n=n,
            prediction=prediction,
            presence_penalty=presence_penalty,
            prompt_cache_key=prompt_cache_key,
            prompt_cache_retention=prompt_cache_retention,
            safety_identifier=safety_identifier,
            seed=seed,
            service_tier=service_tier,
            store=store,
            stop=stop,
            stream_options=stream_options,
            top_logprobs=top_logprobs,
            user=user,
            verbosity=verbosity,
            web_search_options=web_search_options,
        return ChatCompletionStreamManager(
            api_request,
            input_tools=tools,
    async def parse(
        completion = await client.chat.completions.parse(
        _validate_input_tools(tools)
    ) -> AsyncStream[ChatCompletionChunk]:
    ) -> ChatCompletion | AsyncStream[ChatCompletionChunk]:
            stream_cls=AsyncStream[ChatCompletionChunk],
            body=await async_maybe_transform({"metadata": metadata}, completion_update_params.CompletionUpdateParams),
    ) -> AsyncPaginator[ChatCompletion, AsyncCursorPage[ChatCompletion]]:
            page=AsyncCursorPage[ChatCompletion],
    ) -> AsyncChatCompletionStreamManager[ResponseFormatT]:
        async with client.chat.completions.stream(
        When the context manager is entered, an `AsyncChatCompletionStream` instance is returned which, like `.create(stream=True)` is an async iterator. The full list of events that are yielded by the iterator are outlined in [these docs](https://github.com/openai/openai-python/blob/main/helpers.md#chat-completions-events).
        api_request = self.create(
        return AsyncChatCompletionStreamManager(
        self.parse = _legacy_response.to_raw_response_wrapper(
            completions.parse,
            completions.retrieve,
        self.update = _legacy_response.to_raw_response_wrapper(
            completions.update,
            completions.list,
            completions.delete,
        return MessagesWithRawResponse(self._completions.messages)
        self.parse = _legacy_response.async_to_raw_response_wrapper(
        self.update = _legacy_response.async_to_raw_response_wrapper(
        return AsyncMessagesWithRawResponse(self._completions.messages)
        self.parse = to_streamed_response_wrapper(
        self.update = to_streamed_response_wrapper(
        return MessagesWithStreamingResponse(self._completions.messages)
        self.parse = async_to_streamed_response_wrapper(
        self.update = async_to_streamed_response_wrapper(
        return AsyncMessagesWithStreamingResponse(self._completions.messages)
def validate_response_format(response_format: object) -> None:
    if inspect.isclass(response_format) and issubclass(response_format, pydantic.BaseModel):
            "You tried to pass a `BaseModel` class to `chat.completions.create()`; You must use `chat.completions.parse()` instead"
              identifies each user. We recommend hashing their username or email address, in
              order to avoid sending us any identifying information.
            f"/chat/completions/{completion_id}",
