from .response_format_text_json_schema_config import ResponseFormatTextJSONSchemaConfig
__all__ = ["ResponseFormatTextConfig"]
ResponseFormatTextConfig: TypeAlias = Annotated[
    Union[ResponseFormatText, ResponseFormatTextJSONSchemaConfig, ResponseFormatJSONObject],
