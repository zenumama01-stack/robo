from typing_extensions import Required, TypedDict
from .moderation_model import ModerationModel
from .moderation_multi_modal_input_param import ModerationMultiModalInputParam
__all__ = ["ModerationCreateParams"]
class ModerationCreateParams(TypedDict, total=False):
    input: Required[Union[str, SequenceNotStr[str], Iterable[ModerationMultiModalInputParam]]]
    """Input (or inputs) to classify.
    Can be a single string, an array of strings, or an array of multi-modal input
    objects similar to other models.
    model: Union[str, ModerationModel]
    """The content moderation model you would like to use.
