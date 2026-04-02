from ..graders.grader_inputs import GraderInputs
from ..responses.easy_input_message import EasyInputMessage
from ..responses.response_input_audio import ResponseInputAudio
from ..chat.chat_completion_function_tool import ChatCompletionFunctionTool
    "CreateEvalCompletionsRunDataSource",
    "Source",
    "SourceFileContent",
    "SourceFileContentContent",
    "SourceFileID",
    "SourceStoredCompletions",
    "InputMessages",
    "InputMessagesTemplate",
    "InputMessagesTemplateTemplate",
    "InputMessagesTemplateTemplateEvalItem",
    "InputMessagesTemplateTemplateEvalItemContent",
    "InputMessagesTemplateTemplateEvalItemContentOutputText",
    "InputMessagesTemplateTemplateEvalItemContentInputImage",
    "InputMessagesItemReference",
    "SamplingParams",
    "SamplingParamsResponseFormat",
class SourceFileContentContent(BaseModel):
    item: Dict[str, object]
    sample: Optional[Dict[str, object]] = None
class SourceFileContent(BaseModel):
    content: List[SourceFileContentContent]
    """The content of the jsonl file."""
    type: Literal["file_content"]
    """The type of jsonl source. Always `file_content`."""
class SourceFileID(BaseModel):
    """The identifier of the file."""
    type: Literal["file_id"]
    """The type of jsonl source. Always `file_id`."""
class SourceStoredCompletions(BaseModel):
    """A StoredCompletionsRunDataSource configuration describing a set of filters"""
    """The type of source. Always `stored_completions`."""
    created_after: Optional[int] = None
    """An optional Unix timestamp to filter items created after this time."""
    created_before: Optional[int] = None
    """An optional Unix timestamp to filter items created before this time."""
    """An optional maximum number of items to return."""
    """An optional model to filter by (e.g., 'gpt-4o')."""
Source: TypeAlias = Annotated[
    Union[SourceFileContent, SourceFileID, SourceStoredCompletions], PropertyInfo(discriminator="type")
class InputMessagesTemplateTemplateEvalItemContentOutputText(BaseModel):
class InputMessagesTemplateTemplateEvalItemContentInputImage(BaseModel):
    type: Literal["input_image"]
    detail: Optional[str] = None
InputMessagesTemplateTemplateEvalItemContent: TypeAlias = Union[
    InputMessagesTemplateTemplateEvalItemContentOutputText,
    InputMessagesTemplateTemplateEvalItemContentInputImage,
    ResponseInputAudio,
    GraderInputs,
class InputMessagesTemplateTemplateEvalItem(BaseModel):
    content: InputMessagesTemplateTemplateEvalItemContent
    role: Literal["user", "assistant", "system", "developer"]
    type: Optional[Literal["message"]] = None
InputMessagesTemplateTemplate: TypeAlias = Union[EasyInputMessage, InputMessagesTemplateTemplateEvalItem]
class InputMessagesTemplate(BaseModel):
    template: List[InputMessagesTemplateTemplate]
    type: Literal["template"]
    """The type of input messages. Always `template`."""
class InputMessagesItemReference(BaseModel):
    item_reference: str
    """A reference to a variable in the `item` namespace. Ie, "item.input_trajectory" """
    type: Literal["item_reference"]
    """The type of input messages. Always `item_reference`."""
InputMessages: TypeAlias = Annotated[
    Union[InputMessagesTemplate, InputMessagesItemReference], PropertyInfo(discriminator="type")
SamplingParamsResponseFormat: TypeAlias = Union[ResponseFormatText, ResponseFormatJSONSchema, ResponseFormatJSONObject]
class SamplingParams(BaseModel):
    """The maximum number of tokens in the generated output."""
    reasoning_effort: Optional[ReasoningEffort] = None
    response_format: Optional[SamplingParamsResponseFormat] = None
    seed: Optional[int] = None
    """A seed value to initialize the randomness, during sampling."""
    """A higher temperature increases randomness in the outputs."""
    tools: Optional[List[ChatCompletionFunctionTool]] = None
    Currently, only functions are supported as a tool. Use this to provide a list of
    functions the model may generate JSON inputs for. A max of 128 functions are
    supported.
    """An alternative to temperature for nucleus sampling; 1.0 includes all tokens."""
class CreateEvalCompletionsRunDataSource(BaseModel):
    """A CompletionsRunDataSource object describing a model sampling configuration."""
    source: Source
    """Determines what populates the `item` namespace in this run's data source."""
    type: Literal["completions"]
    """The type of run data source. Always `completions`."""
    input_messages: Optional[InputMessages] = None
    """Used when sampling from a model.
    Dictates the structure of the messages passed into the model. Can either be a
    reference to a prebuilt trajectory (ie, `item.input_trajectory`), or a template
    with variable references to the `item` namespace.
    """The name of the model to use for generating completions (e.g. "o3-mini")."""
    sampling_params: Optional[SamplingParams] = None
