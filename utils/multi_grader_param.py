from .python_grader_param import PythonGraderParam
from .label_model_grader_param import LabelModelGraderParam
from .score_model_grader_param import ScoreModelGraderParam
from .string_check_grader_param import StringCheckGraderParam
from .text_similarity_grader_param import TextSimilarityGraderParam
__all__ = ["MultiGraderParam", "Graders"]
Graders: TypeAlias = Union[
    StringCheckGraderParam, TextSimilarityGraderParam, PythonGraderParam, ScoreModelGraderParam, LabelModelGraderParam
class MultiGraderParam(TypedDict, total=False):
    calculate_output: Required[str]
    graders: Required[Graders]
    type: Required[Literal["multi"]]
