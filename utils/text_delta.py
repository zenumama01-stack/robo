from .annotation_delta import AnnotationDelta
__all__ = ["TextDelta"]
class TextDelta(BaseModel):
    annotations: Optional[List[AnnotationDelta]] = None
    value: Optional[str] = None
