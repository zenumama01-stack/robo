from .eval_api_error import EvalAPIError
from ..responses.tool import Tool
from .create_eval_jsonl_run_data_source import CreateEvalJSONLRunDataSource
from ..responses.response_format_text_config import ResponseFormatTextConfig
from .create_eval_completions_run_data_source import CreateEvalCompletionsRunDataSource
    "RunCancelResponse",
    "DataSource",
    "DataSourceResponses",
    "DataSourceResponsesSource",
    "DataSourceResponsesSourceFileContent",
    "DataSourceResponsesSourceFileContentContent",
    "DataSourceResponsesSourceFileID",
    "DataSourceResponsesSourceResponses",
    "DataSourceResponsesInputMessages",
    "DataSourceResponsesInputMessagesTemplate",
    "DataSourceResponsesInputMessagesTemplateTemplate",
    "DataSourceResponsesInputMessagesTemplateTemplateChatMessage",
    "DataSourceResponsesInputMessagesTemplateTemplateEvalItem",
    "DataSourceResponsesInputMessagesTemplateTemplateEvalItemContent",
    "DataSourceResponsesInputMessagesTemplateTemplateEvalItemContentOutputText",
    "DataSourceResponsesInputMessagesTemplateTemplateEvalItemContentInputImage",
    "DataSourceResponsesInputMessagesItemReference",
    "DataSourceResponsesSamplingParams",
    "DataSourceResponsesSamplingParamsText",
    "PerModelUsage",
    "PerTestingCriteriaResult",
    "ResultCounts",
class DataSourceResponsesSourceFileContentContent(BaseModel):
class DataSourceResponsesSourceFileContent(BaseModel):
    content: List[DataSourceResponsesSourceFileContentContent]
class DataSourceResponsesSourceFileID(BaseModel):
class DataSourceResponsesSourceResponses(BaseModel):
    """A EvalResponsesSource object describing a run data source configuration."""
    type: Literal["responses"]
    """The type of run data source. Always `responses`."""
    """Only include items created after this timestamp (inclusive).
    This is a query parameter used to select responses.
    """Only include items created before this timestamp (inclusive).
    instructions_search: Optional[str] = None
    """Optional string to search the 'instructions' field.
    """Metadata filter for the responses.
    """The name of the model to find responses for.
    """Sampling temperature. This is a query parameter used to select responses."""
    tools: Optional[List[str]] = None
    """List of tool names. This is a query parameter used to select responses."""
    """Nucleus sampling parameter. This is a query parameter used to select responses."""
    users: Optional[List[str]] = None
    """List of user identifiers. This is a query parameter used to select responses."""
DataSourceResponsesSource: TypeAlias = Annotated[
    Union[DataSourceResponsesSourceFileContent, DataSourceResponsesSourceFileID, DataSourceResponsesSourceResponses],
class DataSourceResponsesInputMessagesTemplateTemplateChatMessage(BaseModel):
    role: str
class DataSourceResponsesInputMessagesTemplateTemplateEvalItemContentOutputText(BaseModel):
class DataSourceResponsesInputMessagesTemplateTemplateEvalItemContentInputImage(BaseModel):
DataSourceResponsesInputMessagesTemplateTemplateEvalItemContent: TypeAlias = Union[
    DataSourceResponsesInputMessagesTemplateTemplateEvalItemContentOutputText,
    DataSourceResponsesInputMessagesTemplateTemplateEvalItemContentInputImage,
class DataSourceResponsesInputMessagesTemplateTemplateEvalItem(BaseModel):
    content: DataSourceResponsesInputMessagesTemplateTemplateEvalItemContent
DataSourceResponsesInputMessagesTemplateTemplate: TypeAlias = Union[
    DataSourceResponsesInputMessagesTemplateTemplateChatMessage,
    DataSourceResponsesInputMessagesTemplateTemplateEvalItem,
class DataSourceResponsesInputMessagesTemplate(BaseModel):
    template: List[DataSourceResponsesInputMessagesTemplateTemplate]
class DataSourceResponsesInputMessagesItemReference(BaseModel):
    """A reference to a variable in the `item` namespace. Ie, "item.name" """
DataSourceResponsesInputMessages: TypeAlias = Annotated[
    Union[DataSourceResponsesInputMessagesTemplate, DataSourceResponsesInputMessagesItemReference],
class DataSourceResponsesSamplingParamsText(BaseModel):
    """Configuration options for a text response from the model.
    Can be plain
    text or structured JSON data. Learn more:
    format: Optional[ResponseFormatTextConfig] = None
    Configuring `{ "type": "json_schema" }` enables Structured Outputs, which
    ensures the model will match your supplied JSON schema. Learn more in the
    The default format is `{ "type": "text" }` with no additional options.
    **Not recommended for gpt-4o and newer models:**
class DataSourceResponsesSamplingParams(BaseModel):
    text: Optional[DataSourceResponsesSamplingParamsText] = None
    Can be plain text or structured JSON data. Learn more:
    """An array of tools the model may call while generating a response.
    You can specify which tool to use by setting the `tool_choice` parameter.
    The two categories of tools you can provide the model are:
      the model to call your own code. Learn more about
class DataSourceResponses(BaseModel):
    """A ResponsesRunDataSource object describing a model sampling configuration."""
    source: DataSourceResponsesSource
    input_messages: Optional[DataSourceResponsesInputMessages] = None
    sampling_params: Optional[DataSourceResponsesSamplingParams] = None
DataSource: TypeAlias = Annotated[
    Union[CreateEvalJSONLRunDataSource, CreateEvalCompletionsRunDataSource, DataSourceResponses],
class PerModelUsage(BaseModel):
    """The number of tokens retrieved from cache."""
    """The number of completion tokens generated."""
    invocation_count: int
    """The number of invocations."""
    run_model_name: str = FieldInfo(alias="model_name")
    """The name of the model."""
    """The number of prompt tokens used."""
class PerTestingCriteriaResult(BaseModel):
    """Number of tests failed for this criteria."""
    passed: int
    """Number of tests passed for this criteria."""
    testing_criteria: str
    """A description of the testing criteria."""
class ResultCounts(BaseModel):
    """Counters summarizing the outcomes of the evaluation run."""
    errored: int
    """Number of output items that resulted in an error."""
    """Number of output items that failed to pass the evaluation."""
    """Number of output items that passed the evaluation."""
    """Total number of executed output items."""
class RunCancelResponse(BaseModel):
    """A schema representing an evaluation run."""
    """Unique identifier for the evaluation run."""
    """Unix timestamp (in seconds) when the evaluation run was created."""
    data_source: DataSource
    """Information about the run's data source."""
    error: EvalAPIError
    """The identifier of the associated evaluation."""
    """The model that is evaluated, if applicable."""
    """The name of the evaluation run."""
    object: Literal["eval.run"]
    """The type of the object. Always "eval.run"."""
    per_model_usage: List[PerModelUsage]
    """Usage statistics for each model during the evaluation run."""
    per_testing_criteria_results: List[PerTestingCriteriaResult]
    """Results per testing criteria applied during the evaluation run."""
    report_url: str
    """The URL to the rendered evaluation run report on the UI dashboard."""
    result_counts: ResultCounts
    """The status of the evaluation run."""
