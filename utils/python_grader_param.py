__all__ = ["PythonGraderParam"]
class PythonGraderParam(TypedDict, total=False):
    source: Required[str]
    type: Required[Literal["python"]]
    image_tag: str
