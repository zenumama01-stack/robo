from .grader_inputs import GraderInputs
__all__ = ["LabelModelGrader", "Input", "InputContent", "InputContentOutputText", "InputContentInputImage"]
class InputContentOutputText(BaseModel):
class InputContentInputImage(BaseModel):
InputContent: TypeAlias = Union[
    str, ResponseInputText, InputContentOutputText, InputContentInputImage, ResponseInputAudio, GraderInputs
class Input(BaseModel):
    content: InputContent
class LabelModelGrader(BaseModel):
    input: List[Input]
    labels: List[str]
    """The labels to assign to each item in the evaluation."""
    passing_labels: List[str]
    type: Literal["label_model"]
