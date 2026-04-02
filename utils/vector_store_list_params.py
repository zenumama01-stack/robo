__all__ = ["VectorStoreListParams"]
class VectorStoreListParams(TypedDict, total=False):
    before: str
    `before` is an object ID that defines your place in the list. For instance, if
    you make a list request and receive 100 objects, starting with obj_foo, your
    subsequent call can include before=obj_foo in order to fetch the previous page
    of the list.
