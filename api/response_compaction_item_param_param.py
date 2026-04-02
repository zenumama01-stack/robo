__all__ = ["ResponseCompactionItemParamParam"]
class ResponseCompactionItemParamParam(TypedDict, total=False):
    encrypted_content: Required[str]
    type: Required[Literal["compaction"]]
