from openai.types.responses.tool_param import ToolParam
from openai.types.responses.response_input_item_param import ResponseInputItemParam
    tools: List[ToolParam] = [
            "name": "get_current_weather",
            "description": "Get current weather in a given location",
            "parameters": {
                    "location": {
                        "description": "City and state, e.g. San Francisco, CA",
                    "unit": {
                        "enum": ["c", "f"],
                        "description": "Temperature unit to use",
                "required": ["location", "unit"],
                "additionalProperties": False,
            "strict": True,
    input_items: List[ResponseInputItemParam] = [
            "type": "message",
            "content": [{"type": "input_text", "text": "What's the weather in San Francisco today?"}],
    response = client.responses.input_tokens.count(
        model="gpt-5",
        instructions="You are a concise assistant.",
        input=input_items,
        tools=tools,
        tool_choice={"type": "function", "name": "get_current_weather"},
    print(f"input tokens: {response.input_tokens}")
