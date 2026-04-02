    "ScoreModelGrader",
    "Input",
    "InputContent",
    "InputContentOutputText",
    "InputContentInputImage",
    """The sampling parameters for the model."""
    max_completions_tokens: Optional[int] = None
    """The maximum number of tokens the grader model may generate in its response."""
class ScoreModelGrader(BaseModel):
    """The input messages evaluated by the grader.
    Supports text, output text, input image, and input audio content blocks, and may
    include template strings.
    """The model to use for the evaluation."""
    type: Literal["score_model"]
    """The object type, which is always `score_model`."""
    range: Optional[List[float]] = None
    """The range of the score. Defaults to `[0, 1]`."""
