from .response_input_file_param import ResponseInputFileParam
from .response_input_text_param import ResponseInputTextParam
from .response_input_image_param import ResponseInputImageParam
__all__ = ["ResponseCustomToolCallOutputParam", "OutputOutputContentList"]
OutputOutputContentList: TypeAlias = Union[ResponseInputTextParam, ResponseInputImageParam, ResponseInputFileParam]
class ResponseCustomToolCallOutputParam(TypedDict, total=False):
    output: Required[Union[str, Iterable[OutputOutputContentList]]]
    type: Required[Literal["custom_tool_call_output"]]
