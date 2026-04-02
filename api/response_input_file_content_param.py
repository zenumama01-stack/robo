__all__ = ["ResponseInputFileContentParam"]
class ResponseInputFileContentParam(TypedDict, total=False):
    type: Required[Literal["input_file"]]
    file_data: Optional[str]
    file_id: Optional[str]
    file_url: Optional[str]
    filename: Optional[str]
