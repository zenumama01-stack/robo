__all__ = ["ResponseFormatJSONObject"]
class ResponseFormatJSONObject(BaseModel):
    """JSON object response format.
    An older method of generating JSON responses.
    Using `json_schema` is recommended for models that support it. Note that the
    model will not generate JSON without a system or user message instructing it
    to do so.
    type: Literal["json_object"]
    """The type of response format being defined. Always `json_object`."""
class ResponseFormatJSONObject(TypedDict, total=False):
    type: Required[Literal["json_object"]]
