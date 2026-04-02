__all__ = ["GraderInputs", "GraderInputItem", "GraderInputItemOutputText", "GraderInputItemInputImage"]
class GraderInputItemOutputText(BaseModel):
class GraderInputItemInputImage(BaseModel):
GraderInputItem: TypeAlias = Union[
    str, ResponseInputText, GraderInputItemOutputText, GraderInputItemInputImage, ResponseInputAudio
GraderInputs: TypeAlias = List[GraderInputItem]
