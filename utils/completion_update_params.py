__all__ = ["CompletionUpdateParams"]
class CompletionUpdateParams(TypedDict, total=False):
    metadata: Required[Optional[Metadata]]
