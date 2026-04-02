from .file_path_delta_annotation import FilePathDeltaAnnotation
from .file_citation_delta_annotation import FileCitationDeltaAnnotation
__all__ = ["AnnotationDelta"]
AnnotationDelta: TypeAlias = Annotated[
    Union[FileCitationDeltaAnnotation, FilePathDeltaAnnotation], PropertyInfo(discriminator="type")
