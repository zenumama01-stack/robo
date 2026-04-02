__all__ = ["PythonGrader"]
class PythonGrader(BaseModel):
    """The source code of the python script."""
    type: Literal["python"]
    """The object type, which is always `python`."""
    image_tag: Optional[str] = None
    """The image tag to use for the python script."""
