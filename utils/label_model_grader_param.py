from .grader_inputs_param import GraderInputsParam
__all__ = ["LabelModelGraderParam", "Input", "InputContent", "InputContentOutputText", "InputContentInputImage"]
class InputContentOutputText(TypedDict, total=False):
class InputContentInputImage(TypedDict, total=False):
    InputContentOutputText,
    InputContentInputImage,
class Input(TypedDict, total=False):
    content: Required[InputContent]
class LabelModelGraderParam(TypedDict, total=False):
    input: Required[Iterable[Input]]
