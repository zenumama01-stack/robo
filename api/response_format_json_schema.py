__all__ = ["ResponseFormatJSONSchema", "JSONSchema"]
class JSONSchema(BaseModel):
    """Structured Outputs configuration options, including a JSON Schema."""
    schema_: Optional[Dict[str, object]] = FieldInfo(alias="schema", default=None)
class ResponseFormatJSONSchema(BaseModel):
    json_schema: JSONSchema
class JSONSchema(TypedDict, total=False):
    schema: Dict[str, object]
class ResponseFormatJSONSchema(TypedDict, total=False):
    json_schema: Required[JSONSchema]
