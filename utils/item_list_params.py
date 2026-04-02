__all__ = ["ItemListParams"]
class ItemListParams(TypedDict, total=False):
    """An item ID to list items after, used in pagination."""
    """Specify additional output data to include in the model response.
    Currently supported values are:
    """The order to return the input items in. Default is `desc`.
