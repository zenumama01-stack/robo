__all__ = ["AssistantUpdateParams", "ToolResources", "ToolResourcesCodeInterpreter", "ToolResourcesFileSearch"]
class AssistantUpdateParams(TypedDict, total=False):
    Overrides the list of
    [file](https://platform.openai.com/docs/api-reference/files) IDs made available
    to the `code_interpreter` tool. There can be a maximum of 20 files associated
    with the tool.
    Overrides the
