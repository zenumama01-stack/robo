from typing import Dict, List
__all__ = ["ToolChoiceAllowed"]
class ToolChoiceAllowed(BaseModel):
    mode: Literal["auto", "required"]
    tools: List[Dict[str, object]]
    For the Responses API, the list of tool definitions might look like:
      { "type": "function", "name": "get_weather" },
      { "type": "mcp", "server_label": "deepwiki" },
      { "type": "image_generation" }
    type: Literal["allowed_tools"]
