__all__ = ["ResponsePromptParam", "Variables"]
Variables: TypeAlias = Union[str, ResponseInputTextParam, ResponseInputImageParam, ResponseInputFileParam]
class ResponsePromptParam(TypedDict, total=False):
    variables: Optional[Dict[str, Variables]]
    version: Optional[str]
