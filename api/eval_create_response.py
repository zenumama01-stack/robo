from typing import Dict, List, Union, Optional
from typing_extensions import Literal, Annotated, TypeAlias
from pydantic import Field as FieldInfo
from .._utils import PropertyInfo
from .graders.python_grader import PythonGrader
from .graders.label_model_grader import LabelModelGrader
from .graders.score_model_grader import ScoreModelGrader
from .graders.string_check_grader import StringCheckGrader
from .eval_custom_data_source_config import EvalCustomDataSourceConfig
from .graders.text_similarity_grader import TextSimilarityGrader
from .eval_stored_completions_data_source_config import EvalStoredCompletionsDataSourceConfig
    "EvalCreateResponse",
    "TestingCriterionEvalGraderTextSimilarity",
    "TestingCriterionEvalGraderPython",
    "TestingCriterionEvalGraderScoreModel",
class DataSourceConfigLogs(BaseModel):
    A LogsDataSourceConfig which specifies the metadata property of your logs query.
    The schema returned by this data source config is used to defined what variables are available in your evals.
    `item` and `sample` are both defined when using this data source config.
    schema_: Dict[str, object] = FieldInfo(alias="schema")
    The json schema for the run data source items. Learn how to build JSON schemas
    [here](https://json-schema.org/).
    type: Literal["logs"]
DataSourceConfig: TypeAlias = Annotated[
    Union[EvalCustomDataSourceConfig, DataSourceConfigLogs, EvalStoredCompletionsDataSourceConfig],
class TestingCriterionEvalGraderTextSimilarity(TextSimilarityGrader):
    __test__ = False
class TestingCriterionEvalGraderPython(PythonGrader):
    pass_threshold: Optional[float] = None
class TestingCriterionEvalGraderScoreModel(ScoreModelGrader):
    LabelModelGrader,
    StringCheckGrader,
    TestingCriterionEvalGraderTextSimilarity,
    TestingCriterionEvalGraderPython,
    TestingCriterionEvalGraderScoreModel,
class EvalCreateResponse(BaseModel):
    An Eval object with a data source config and testing criteria.
    An Eval represents a task to be done for your LLM integration.
    Like:
     - Improve the quality of my chatbot
     - See how well my chatbot handles customer support
     - Check if o4-mini is better at my usecase than gpt-4o
    """Unique identifier for the evaluation."""
    """The Unix timestamp (in seconds) for when the eval was created."""
    data_source_config: DataSourceConfig
    """Configuration of data sources used in runs of the evaluation."""
    object: Literal["eval"]
    """The object type."""
    testing_criteria: List[TestingCriterion]
    """A list of testing criteria."""
