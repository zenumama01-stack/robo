__all__ = ["ResponseFormatTextGrammar"]
class ResponseFormatTextGrammar(BaseModel):
    A custom grammar for the model to follow when generating text.
    Learn more in the [custom grammars guide](https://platform.openai.com/docs/guides/custom-grammars).
    grammar: str
    """The custom grammar for the model to follow."""
    """The type of response format being defined. Always `grammar`."""
