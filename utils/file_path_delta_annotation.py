__all__ = ["FilePathDeltaAnnotation", "FilePath"]
class FilePathDeltaAnnotation(BaseModel):
    file_path: Optional[FilePath] = None
