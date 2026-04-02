__all__ = ["TextSimilarityGrader"]
class TextSimilarityGrader(BaseModel):
    evaluation_metric: Literal[
        "cosine",
        "fuzzy_match",
        "bleu",
        "gleu",
        "meteor",
        "rouge_1",
        "rouge_2",
        "rouge_3",
        "rouge_4",
        "rouge_5",
        "rouge_l",
    """The evaluation metric to use.
    One of `cosine`, `fuzzy_match`, `bleu`, `gleu`, `meteor`, `rouge_1`, `rouge_2`,
    `rouge_3`, `rouge_4`, `rouge_5`, or `rouge_l`.
    """The text being graded."""
    """The text being graded against."""
    type: Literal["text_similarity"]
    """The type of grader."""
