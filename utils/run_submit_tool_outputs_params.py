    "RunSubmitToolOutputsParamsBase",
    "ToolOutput",
    "RunSubmitToolOutputsParamsNonStreaming",
    "RunSubmitToolOutputsParamsStreaming",
class RunSubmitToolOutputsParamsBase(TypedDict, total=False):
    tool_outputs: Required[Iterable[ToolOutput]]
    """A list of tools for which the outputs are being submitted."""
class ToolOutput(TypedDict, total=False):
    """The output of the tool call to be submitted to continue the run."""
    tool_call_id: str
    The ID of the tool call in the `required_action` object within the run object
    the output is being submitted for.
class RunSubmitToolOutputsParamsNonStreaming(RunSubmitToolOutputsParamsBase, total=False):
class RunSubmitToolOutputsParamsStreaming(RunSubmitToolOutputsParamsBase):
RunSubmitToolOutputsParams = Union[RunSubmitToolOutputsParamsNonStreaming, RunSubmitToolOutputsParamsStreaming]
