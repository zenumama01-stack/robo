__all__ = ["ResponseOutputTextAnnotationAddedEvent"]
class ResponseOutputTextAnnotationAddedEvent(BaseModel):
    """Emitted when an annotation is added to output text content."""
    annotation: object
    """The annotation object being added. (See annotation schema for details.)"""
    annotation_index: int
    """The index of the annotation within the content part."""
    """The index of the content part within the output item."""
    """The unique identifier of the item to which the annotation is being added."""
    type: Literal["response.output_text.annotation.added"]
    """The type of the event. Always 'response.output_text.annotation.added'."""
