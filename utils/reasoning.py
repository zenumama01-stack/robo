from .reasoning_effort import ReasoningEffort
__all__ = ["Reasoning"]
class Reasoning(BaseModel):
    effort: Optional[ReasoningEffort] = None
    generate_summary: Optional[Literal["auto", "concise", "detailed"]] = None
    """**Deprecated:** use `summary` instead.
    A summary of the reasoning performed by the model. This can be useful for
    debugging and understanding the model's reasoning process. One of `auto`,
    `concise`, or `detailed`.
    summary: Optional[Literal["auto", "concise", "detailed"]] = None
    """A summary of the reasoning performed by the model.
    This can be useful for debugging and understanding the model's reasoning
    process. One of `auto`, `concise`, or `detailed`.
    `concise` is supported for `computer-use-preview` models and all reasoning
    models after `gpt-5`.
class Reasoning(TypedDict, total=False):
    effort: Optional[ReasoningEffort]
    generate_summary: Optional[Literal["auto", "concise", "detailed"]]
    summary: Optional[Literal["auto", "concise", "detailed"]]
