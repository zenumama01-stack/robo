from ..shared_params.custom_tool_input_format import CustomToolInputFormat
__all__ = ["CustomToolParam"]
class CustomToolParam(TypedDict, total=False):
    format: CustomToolInputFormat
