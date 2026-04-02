from ..shared.custom_tool_input_format import CustomToolInputFormat
__all__ = ["CustomTool"]
class CustomTool(BaseModel):
    """A custom tool that processes input using a specified format.
    Learn more about   [custom tools](https://platform.openai.com/docs/guides/function-calling#custom-tools)
    """Whether this tool should be deferred and discovered via tool search."""
    format: Optional[CustomToolInputFormat] = None
"""Custom tool decorator for OpenAI custom tools."""
def _make_wrapped_func(func: Callable[..., str]) -> Callable[..., list[dict[str, Any]]]:
    def wrapped(x: str) -> list[dict[str, Any]]:
        return [{"type": "custom_tool_call_output", "output": func(x)}]
def _make_wrapped_coroutine(
    coroutine: Callable[..., Awaitable[str]],
) -> Callable[..., Awaitable[list[dict[str, Any]]]]:
    async def wrapped(*args: Any, **kwargs: Any) -> list[dict[str, Any]]:
        result = await coroutine(*args, **kwargs)
        return [{"type": "custom_tool_call_output", "output": result}]
def custom_tool(*args: Any, **kwargs: Any) -> Any:
    """Decorator to create an OpenAI custom tool.
    Custom tools allow for tools with (potentially long) freeform string inputs.
    See below for an example using LangGraph:
    @custom_tool
    def execute_code(code: str) -> str:
        \"\"\"Execute python code.\"\"\"
        return "27"
    model = ChatOpenAI(model="gpt-5", output_version="responses/v1")
    agent = create_react_agent(model, [execute_code])
    input_message = {"role": "user", "content": "Use the tool to calculate 3^3."}
        {"messages": [input_message]},
    You can also specify a format for a corresponding context-free grammar using the
    `format` kwarg:
    from langchain_openai import ChatOpenAI, custom_tool
    grammar = \"\"\"
    start: expr
    expr: term (SP ADD SP term)* -> add
    | term
    term: factor (SP MUL SP factor)* -> mul
    | factor
    factor: INT
    SP: " "
    ADD: "+"
    MUL: "*"
    %import common.INT
    format = {"type": "grammar", "syntax": "lark", "definition": grammar}
    # highlight-next-line
    @custom_tool(format=format)
    def do_math(input_string: str) -> str:
        \"\"\"Do a mathematical operation.\"\"\"
    agent = create_react_agent(model, [do_math])
    def decorator(func: Callable[..., Any]) -> Any:
        metadata = {"type": "custom_tool"}
        if "format" in kwargs:
            metadata["format"] = kwargs.pop("format")
        tool_obj = tool(infer_schema=False, **kwargs)(func)
        tool_obj.metadata = metadata
        tool_obj.description = func.__doc__
        if inspect.iscoroutinefunction(func):
            tool_obj.coroutine = _make_wrapped_coroutine(func)
            tool_obj.func = _make_wrapped_func(func)
        return tool_obj
    if args and callable(args[0]) and not kwargs:
        return decorator(args[0])
