__all__ = ["FileUpdateParams"]
class FileUpdateParams(TypedDict, total=False):
    attributes: Required[Optional[Dict[str, Union[str, float, bool]]]]
