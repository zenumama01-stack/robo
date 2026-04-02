__all__ = ["ResponseFormatTextJSONSchemaConfigParam"]
class ResponseFormatTextJSONSchemaConfigParam(TypedDict, total=False):
    schema: Required[Dict[str, object]]
    type: Required[Literal["json_schema"]]
