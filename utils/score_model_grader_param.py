    "ScoreModelGraderParam",
    max_completions_tokens: Optional[int]
class ScoreModelGraderParam(TypedDict, total=False):
    type: Required[Literal["score_model"]]
    range: Iterable[float]
