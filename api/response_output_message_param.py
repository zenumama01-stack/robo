from .response_output_text_param import ResponseOutputTextParam
from .response_output_refusal_param import ResponseOutputRefusalParam
__all__ = ["ResponseOutputMessageParam", "Content"]
Content: TypeAlias = Union[ResponseOutputTextParam, ResponseOutputRefusalParam]
class ResponseOutputMessageParam(TypedDict, total=False):
