__all__ = ["ResponseFormatTextJSONSchemaConfig"]
class ResponseFormatTextJSONSchemaConfig(BaseModel):
    """JSON Schema response format.
    Used to generate structured JSON responses.
    Learn more about [Structured Outputs](https://platform.openai.com/docs/guides/structured-outputs).
    """The name of the response format.
    The schema for the response format, described as a JSON Schema object. Learn how
    to build JSON schemas [here](https://json-schema.org/).
    type: Literal["json_schema"]
    """The type of response format being defined. Always `json_schema`."""
    A description of what the response format is for, used by the model to determine
    how to respond in the format.
    Whether to enable strict schema adherence when generating the output. If set to
    true, the model will always follow the exact schema defined in the `schema`
    field. Only a subset of JSON Schema is supported when `strict` is `true`. To
    learn more, read the
