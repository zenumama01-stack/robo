__all__ = ["MessageUpdateParams"]
class MessageUpdateParams(TypedDict, total=False):
    thread_id: Required[str]
