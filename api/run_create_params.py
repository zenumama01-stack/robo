from ...shared.chat_model import ChatModel
from ..assistant_tool_param import AssistantToolParam
from .runs.run_step_include import RunStepInclude
from ...shared.reasoning_effort import ReasoningEffort
from ..assistant_tool_choice_option_param import AssistantToolChoiceOptionParam
from ..assistant_response_format_option_param import AssistantResponseFormatOptionParam
    "RunCreateParamsBase",
    "AdditionalMessage",
    "AdditionalMessageAttachment",
    "AdditionalMessageAttachmentTool",
    "AdditionalMessageAttachmentToolFileSearch",
    "RunCreateParamsNonStreaming",
    "RunCreateParamsStreaming",
class RunCreateParamsBase(TypedDict, total=False):
    include: List[RunStepInclude]
    """A list of additional fields to include in the response.
    Currently the only supported value is
    `step_details.tool_calls[*].file_search.results[*].content` to fetch the file
    search result content.
    additional_instructions: Optional[str]
    """Appends additional instructions at the end of the instructions for the run.
    This is useful for modifying the behavior on a per-run basis without overriding
    other instructions.
    additional_messages: Optional[Iterable[AdditionalMessage]]
    """Adds additional messages to the thread before creating the run."""
class AdditionalMessageAttachmentToolFileSearch(TypedDict, total=False):
AdditionalMessageAttachmentTool: TypeAlias = Union[CodeInterpreterToolParam, AdditionalMessageAttachmentToolFileSearch]
class AdditionalMessageAttachment(TypedDict, total=False):
    tools: Iterable[AdditionalMessageAttachmentTool]
class AdditionalMessage(TypedDict, total=False):
    attachments: Optional[Iterable[AdditionalMessageAttachment]]
class RunCreateParamsNonStreaming(RunCreateParamsBase, total=False):
class RunCreateParamsStreaming(RunCreateParamsBase):
RunCreateParams = Union[RunCreateParamsNonStreaming, RunCreateParamsStreaming]
from ..responses.tool_param import ToolParam
from .create_eval_jsonl_run_data_source_param import CreateEvalJSONLRunDataSourceParam
from ..responses.response_format_text_config_param import ResponseFormatTextConfigParam
from .create_eval_completions_run_data_source_param import CreateEvalCompletionsRunDataSourceParam
    "RunCreateParams",
    "DataSourceCreateEvalResponsesRunDataSource",
    "DataSourceCreateEvalResponsesRunDataSourceSource",
    "DataSourceCreateEvalResponsesRunDataSourceSourceFileContent",
    "DataSourceCreateEvalResponsesRunDataSourceSourceFileContentContent",
    "DataSourceCreateEvalResponsesRunDataSourceSourceFileID",
    "DataSourceCreateEvalResponsesRunDataSourceSourceResponses",
    "DataSourceCreateEvalResponsesRunDataSourceInputMessages",
    "DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplate",
    "DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplate",
    "DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateChatMessage",
    "DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItem",
    "DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItemContent",
    "DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItemContentOutputText",
    "DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItemContentInputImage",
    "DataSourceCreateEvalResponsesRunDataSourceInputMessagesItemReference",
    "DataSourceCreateEvalResponsesRunDataSourceSamplingParams",
    "DataSourceCreateEvalResponsesRunDataSourceSamplingParamsText",
class RunCreateParams(TypedDict, total=False):
    data_source: Required[DataSource]
    """Details about the run's data source."""
    """The name of the run."""
class DataSourceCreateEvalResponsesRunDataSourceSourceFileContentContent(TypedDict, total=False):
class DataSourceCreateEvalResponsesRunDataSourceSourceFileContent(TypedDict, total=False):
    content: Required[Iterable[DataSourceCreateEvalResponsesRunDataSourceSourceFileContentContent]]
class DataSourceCreateEvalResponsesRunDataSourceSourceFileID(TypedDict, total=False):
class DataSourceCreateEvalResponsesRunDataSourceSourceResponses(TypedDict, total=False):
    type: Required[Literal["responses"]]
    instructions_search: Optional[str]
    metadata: Optional[object]
    tools: Optional[SequenceNotStr[str]]
    users: Optional[SequenceNotStr[str]]
DataSourceCreateEvalResponsesRunDataSourceSource: TypeAlias = Union[
    DataSourceCreateEvalResponsesRunDataSourceSourceFileContent,
    DataSourceCreateEvalResponsesRunDataSourceSourceFileID,
    DataSourceCreateEvalResponsesRunDataSourceSourceResponses,
class DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateChatMessage(TypedDict, total=False):
class DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItemContentOutputText(
    TypedDict, total=False
class DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItemContentInputImage(
DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItemContent: TypeAlias = Union[
    DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItemContentOutputText,
    DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItemContentInputImage,
class DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItem(TypedDict, total=False):
    content: Required[DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItemContent]
DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplate: TypeAlias = Union[
    DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateChatMessage,
    DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplateEvalItem,
class DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplate(TypedDict, total=False):
    template: Required[Iterable[DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplateTemplate]]
class DataSourceCreateEvalResponsesRunDataSourceInputMessagesItemReference(TypedDict, total=False):
DataSourceCreateEvalResponsesRunDataSourceInputMessages: TypeAlias = Union[
    DataSourceCreateEvalResponsesRunDataSourceInputMessagesTemplate,
    DataSourceCreateEvalResponsesRunDataSourceInputMessagesItemReference,
class DataSourceCreateEvalResponsesRunDataSourceSamplingParamsText(TypedDict, total=False):
    format: ResponseFormatTextConfigParam
class DataSourceCreateEvalResponsesRunDataSourceSamplingParams(TypedDict, total=False):
    text: DataSourceCreateEvalResponsesRunDataSourceSamplingParamsText
    tools: Iterable[ToolParam]
class DataSourceCreateEvalResponsesRunDataSource(TypedDict, total=False):
    source: Required[DataSourceCreateEvalResponsesRunDataSourceSource]
    input_messages: DataSourceCreateEvalResponsesRunDataSourceInputMessages
    sampling_params: DataSourceCreateEvalResponsesRunDataSourceSamplingParams
DataSource: TypeAlias = Union[
    CreateEvalJSONLRunDataSourceParam,
    CreateEvalCompletionsRunDataSourceParam,
    DataSourceCreateEvalResponsesRunDataSource,
