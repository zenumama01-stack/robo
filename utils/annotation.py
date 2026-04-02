from .file_path_annotation import FilePathAnnotation
from .file_citation_annotation import FileCitationAnnotation
__all__ = ["Annotation"]
Annotation: TypeAlias = Annotated[Union[FileCitationAnnotation, FilePathAnnotation], PropertyInfo(discriminator="type")]
