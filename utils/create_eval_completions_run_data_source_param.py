from ..graders.grader_inputs_param import GraderInputsParam
from ..responses.easy_input_message_param import EasyInputMessageParam
from ..responses.response_input_audio_param import ResponseInputAudioParam
from ..chat.chat_completion_function_tool_param import ChatCompletionFunctionToolParam
    "CreateEvalCompletionsRunDataSourceParam",
class SourceFileContentContent(TypedDict, total=False):
    item: Required[Dict[str, object]]
    sample: Dict[str, object]
class SourceFileContent(TypedDict, total=False):
    content: Required[Iterable[SourceFileContentContent]]
    type: Required[Literal["file_content"]]
class SourceFileID(TypedDict, total=False):
    type: Required[Literal["file_id"]]
class SourceStoredCompletions(TypedDict, total=False):
    created_after: Optional[int]
    created_before: Optional[int]
    limit: Optional[int]
    model: Optional[str]
Source: TypeAlias = Union[SourceFileContent, SourceFileID, SourceStoredCompletions]
class InputMessagesTemplateTemplateEvalItemContentOutputText(TypedDict, total=False):
class InputMessagesTemplateTemplateEvalItemContentInputImage(TypedDict, total=False):
class InputMessagesTemplateTemplateEvalItem(TypedDict, total=False):
    content: Required[InputMessagesTemplateTemplateEvalItemContent]
InputMessagesTemplateTemplate: TypeAlias = Union[EasyInputMessageParam, InputMessagesTemplateTemplateEvalItem]
class InputMessagesTemplate(TypedDict, total=False):
    template: Required[Iterable[InputMessagesTemplateTemplate]]
    type: Required[Literal["template"]]
class InputMessagesItemReference(TypedDict, total=False):
    item_reference: Required[str]
    type: Required[Literal["item_reference"]]
InputMessages: TypeAlias = Union[InputMessagesTemplate, InputMessagesItemReference]
class SamplingParams(TypedDict, total=False):
    max_completion_tokens: int
    response_format: SamplingParamsResponseFormat
    seed: int
    tools: Iterable[ChatCompletionFunctionToolParam]
    top_p: float
class CreateEvalCompletionsRunDataSourceParam(TypedDict, total=False):
    source: Required[Source]
    type: Required[Literal["completions"]]
    input_messages: InputMessages
    sampling_params: SamplingParams
