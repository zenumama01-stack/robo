from ..graders.multi_grader import MultiGrader
from ..graders.python_grader import PythonGrader
from ..graders.score_model_grader import ScoreModelGrader
from ..graders.string_check_grader import StringCheckGrader
from .reinforcement_hyperparameters import ReinforcementHyperparameters
from ..graders.text_similarity_grader import TextSimilarityGrader
__all__ = ["ReinforcementMethod", "Grader"]
Grader: TypeAlias = Union[StringCheckGrader, TextSimilarityGrader, PythonGrader, ScoreModelGrader, MultiGrader]
class ReinforcementMethod(BaseModel):
    grader: Grader
    """The grader used for the fine-tuning job."""
    hyperparameters: Optional[ReinforcementHyperparameters] = None
