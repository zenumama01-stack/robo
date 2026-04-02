    "ChatCompletionCustomToolParam",
    "Custom",
    "CustomFormat",
    "CustomFormatText",
    "CustomFormatGrammar",
    "CustomFormatGrammarGrammar",
class CustomFormatText(TypedDict, total=False):
    """Unconstrained free-form text."""
    """Unconstrained text format. Always `text`."""
class CustomFormatGrammarGrammar(TypedDict, total=False):
    """Your chosen grammar."""
    definition: Required[str]
    """The grammar definition."""
    syntax: Required[Literal["lark", "regex"]]
    """The syntax of the grammar definition. One of `lark` or `regex`."""
class CustomFormatGrammar(TypedDict, total=False):
    """A grammar defined by the user."""
    grammar: Required[CustomFormatGrammarGrammar]
    type: Required[Literal["grammar"]]
    """Grammar format. Always `grammar`."""
CustomFormat: TypeAlias = Union[CustomFormatText, CustomFormatGrammar]
class Custom(TypedDict, total=False):
    """Properties of the custom tool."""
    """The name of the custom tool, used to identify it in tool calls."""
    """Optional description of the custom tool, used to provide more context."""
    format: CustomFormat
    """The input format for the custom tool. Default is unconstrained text."""
class ChatCompletionCustomToolParam(TypedDict, total=False):
    """A custom tool that processes input using a specified format."""
    custom: Required[Custom]
    """The type of the custom tool. Always `custom`."""
