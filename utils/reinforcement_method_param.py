from ..graders.multi_grader_param import MultiGraderParam
from ..graders.python_grader_param import PythonGraderParam
from ..graders.score_model_grader_param import ScoreModelGraderParam
from ..graders.string_check_grader_param import StringCheckGraderParam
from .reinforcement_hyperparameters_param import ReinforcementHyperparametersParam
from ..graders.text_similarity_grader_param import TextSimilarityGraderParam
__all__ = ["ReinforcementMethodParam", "Grader"]
Grader: TypeAlias = Union[
    StringCheckGraderParam, TextSimilarityGraderParam, PythonGraderParam, ScoreModelGraderParam, MultiGraderParam
class ReinforcementMethodParam(TypedDict, total=False):
    grader: Required[Grader]
    hyperparameters: ReinforcementHyperparametersParam
