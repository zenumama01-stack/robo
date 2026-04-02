    "GraderInputsParam",
    "GraderInputsParamItem",
    "GraderInputsParamItemOutputText",
    "GraderInputsParamItemInputImage",
class GraderInputsParamItemOutputText(TypedDict, total=False):
class GraderInputsParamItemInputImage(TypedDict, total=False):
GraderInputsParamItem: TypeAlias = Union[
    GraderInputsParamItemOutputText,
    GraderInputsParamItemInputImage,
GraderInputsParam: TypeAlias = List[GraderInputsParamItem]
