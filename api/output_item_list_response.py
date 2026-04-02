from typing import TYPE_CHECKING, Dict, List, Optional
from ..eval_api_error import EvalAPIError
__all__ = ["OutputItemListResponse", "Result", "Sample", "SampleInput", "SampleOutput", "SampleUsage"]
class Result(BaseModel):
    """A single grader result for an evaluation run output item."""
    passed: bool
    """Whether the grader considered the output a pass."""
    """The numeric score produced by the grader."""
    """Optional sample or intermediate data produced by the grader."""
    """The grader type (for example, "string-check-grader")."""
        # Some versions of Pydantic <2.8.0 have a bug and don’t allow assigning a
        # value to this field, so for compatibility we avoid doing it at runtime.
        __pydantic_extra__: Dict[str, object] = FieldInfo(init=False)  # pyright: ignore[reportIncompatibleVariableOverride]
        # Stub to indicate that arbitrary properties are accepted.
        # To access properties that are not valid identifiers you can use `getattr`, e.g.
        # `getattr(obj, '$type')`
        def __getattr__(self, attr: str) -> object: ...
        __pydantic_extra__: Dict[str, object]
class SampleInput(BaseModel):
    """An input message."""
    """The role of the message sender (e.g., system, user, developer)."""
class SampleOutput(BaseModel):
    role: Optional[str] = None
class SampleUsage(BaseModel):
    """Token usage details for the sample."""
class Sample(BaseModel):
    """A sample containing the input and output of the evaluation run."""
    finish_reason: str
    """The reason why the sample generation was finished."""
    input: List[SampleInput]
    """An array of input messages."""
    """The maximum number of tokens allowed for completion."""
    """The model used for generating the sample."""
    output: List[SampleOutput]
    """An array of output messages."""
    """The seed used for generating the sample."""
    """The sampling temperature used."""
    """The top_p value used for sampling."""
    usage: SampleUsage
class OutputItemListResponse(BaseModel):
    """A schema representing an evaluation run output item."""
    """Unique identifier for the evaluation run output item."""
    datasource_item: Dict[str, object]
    """Details of the input data source item."""
    datasource_item_id: int
    """The identifier for the data source item."""
    """The identifier of the evaluation group."""
    object: Literal["eval.run.output_item"]
    """The type of the object. Always "eval.run.output_item"."""
    results: List[Result]
    """A list of grader results for this output item."""
    """The identifier of the evaluation run associated with this output item."""
    sample: Sample
