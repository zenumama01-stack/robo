__all__ = ["ToolChoiceTypes"]
class ToolChoiceTypes(BaseModel):
    Indicates that the model should use a built-in tool to generate a response.
    [Learn more about built-in tools](https://platform.openai.com/docs/guides/tools).
    type: Literal[
        "file_search",
        "web_search_preview",
        "computer",
        "computer_use_preview",
        "computer_use",
        "web_search_preview_2025_03_11",
        "image_generation",
        "code_interpreter",
    """The type of hosted tool the model should to use.
    Allowed values are:
    - `file_search`
    - `web_search_preview`
    - `computer`
    - `computer_use_preview`
    - `computer_use`
    - `code_interpreter`
    - `image_generation`
