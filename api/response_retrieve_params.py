__all__ = ["ResponseRetrieveParamsBase", "ResponseRetrieveParamsNonStreaming", "ResponseRetrieveParamsStreaming"]
class ResponseRetrieveParamsBase(TypedDict, total=False):
    starting_after: int
    """The sequence number of the event after which to start streaming."""
class ResponseRetrieveParamsNonStreaming(ResponseRetrieveParamsBase, total=False):
    stream: Literal[False]
class ResponseRetrieveParamsStreaming(ResponseRetrieveParamsBase):
ResponseRetrieveParams = Union[ResponseRetrieveParamsNonStreaming, ResponseRetrieveParamsStreaming]
