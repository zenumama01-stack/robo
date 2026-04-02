from ..shared_params.function_definition import FunctionDefinition
__all__ = ["FunctionToolParam"]
class FunctionToolParam(TypedDict, total=False):
    function: Required[FunctionDefinition]
    type: Required[Literal["function"]]
    parameters: Required[Optional[Dict[str, object]]]
    strict: Required[Optional[bool]]
