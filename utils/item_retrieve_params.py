__all__ = ["ItemRetrieveParams"]
class ItemRetrieveParams(TypedDict, total=False):
    conversation_id: Required[str]
