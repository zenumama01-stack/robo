from .response_format_text_json_schema_config_param import ResponseFormatTextJSONSchemaConfigParam
__all__ = ["ResponseFormatTextConfigParam"]
ResponseFormatTextConfigParam: TypeAlias = Union[
    ResponseFormatText, ResponseFormatTextJSONSchemaConfigParam, ResponseFormatJSONObject
