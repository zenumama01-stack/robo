__all__ = ["FilePathAnnotation", "FilePath"]
class FilePath(BaseModel):
    """The ID of the file that was generated."""
class FilePathAnnotation(BaseModel):
    A URL for the file that's generated when the assistant used the `code_interpreter` tool to generate a file.
    file_path: FilePath
    type: Literal["file_path"]
    """Always `file_path`."""
