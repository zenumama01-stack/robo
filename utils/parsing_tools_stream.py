class GetWeather(BaseModel):
    city: str
    country: str
            "content": "What's the weather like in SF and New York?",
        # because we're using `.parse_stream()`, the returned tool calls
        # will be automatically deserialized into this `GetWeather` type
        openai.pydantic_function_tool(GetWeather, name="get_weather"),
    parallel_tool_calls=True,
        if event.type == "tool_calls.function.arguments.delta" or event.type == "tool_calls.function.arguments.done":
            rich.get_console().print(event, width=80)
print("----\n")
