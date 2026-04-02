from .conversation_item_with_reference import ConversationItemWithReference
__all__ = ["ResponseCreateEvent", "Response", "ResponseTool"]
class ResponseTool(BaseModel):
    The description of the function, including guidance on when and how to call it,
    and guidance about what to tell the user when calling (if anything).
    """The name of the function."""
    parameters: Optional[object] = None
    """Parameters of the function in JSON Schema."""
    type: Optional[Literal["function"]] = None
    """The type of the tool, i.e. `function`."""
class Response(BaseModel):
    conversation: Union[str, Literal["auto", "none"], None] = None
    """Controls which conversation the response is added to.
    Currently supports `auto` and `none`, with `auto` as the default value. The
    `auto` value means that the contents of the response will be added to the
    default conversation. Set this to `none` to create an out-of-band response which
    will not add items to default conversation.
    input: Optional[List[ConversationItemWithReference]] = None
    """Input items to include in the prompt for the model.
    Using this field creates a new context for this Response instead of using the
    default conversation. An empty array `[]` will clear the context for this
    Response. Note that this can include references to items from the default
    """The default system instructions (i.e.
    system message) prepended to model calls. This field allows the client to guide
    the model on desired responses. The model can be instructed on response content
    and format, (e.g. "be extremely succinct", "act friendly", "here are examples of
    good responses") and on audio behavior (e.g. "talk quickly", "inject emotion
    into your voice", "laugh frequently"). The instructions are not guaranteed to be
    followed by the model, but they provide guidance to the model on the desired
    behavior.
    max_response_output_tokens: Union[int, Literal["inf"], None] = None
    """The set of modalities the model can respond with.
    To disable audio, set this to ["text"].
    tool_choice: Optional[str] = None
    """How the model chooses tools.
    Options are `auto`, `none`, `required`, or specify a function, like
    `{"type": "function", "function": {"name": "my_function"}}`.
    tools: Optional[List[ResponseTool]] = None
    """Tools (functions) available to the model."""
    """The voice the model uses to respond.
    Voice cannot be changed during the session once the model has responded with
    audio at least once. Current voice options are `alloy`, `ash`, `ballad`,
    `coral`, `echo`, `sage`, `shimmer`, and `verse`.
class ResponseCreateEvent(BaseModel):
    type: Literal["response.create"]
    """The event type, must be `response.create`."""
    response: Optional[Response] = None
    """Create a new Realtime response with these parameters"""
from .realtime_response_create_params import RealtimeResponseCreateParams
__all__ = ["ResponseCreateEvent"]
    response: Optional[RealtimeResponseCreateParams] = None
