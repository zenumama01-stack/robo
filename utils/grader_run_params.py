from ...graders.multi_grader_param import MultiGraderParam
from ...graders.python_grader_param import PythonGraderParam
from ...graders.score_model_grader_param import ScoreModelGraderParam
from ...graders.string_check_grader_param import StringCheckGraderParam
from ...graders.text_similarity_grader_param import TextSimilarityGraderParam
__all__ = ["GraderRunParams", "Grader"]
class GraderRunParams(TypedDict, total=False):
    model_sample: Required[str]
    """The model sample to be evaluated.
    This value will be used to populate the `sample` namespace. See
    item: object
    """The dataset item provided to the grader.
    This will be used to populate the `item` namespace. See
