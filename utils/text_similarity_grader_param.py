__all__ = ["TextSimilarityGraderParam"]
class TextSimilarityGraderParam(TypedDict, total=False):
    evaluation_metric: Required[
    type: Required[Literal["text_similarity"]]
