from . import web_search_tool
from .computer_tool import ComputerTool
from .namespace_tool import NamespaceTool
from .web_search_tool import WebSearchTool
from .apply_patch_tool import ApplyPatchTool
from .tool_search_tool import ToolSearchTool
from .function_shell_tool import FunctionShellTool
from .web_search_preview_tool import WebSearchPreviewTool
from .computer_use_preview_tool import ComputerUsePreviewTool
    "WebSearchTool",
    "CodeInterpreterContainer",
    "CodeInterpreterContainerCodeInterpreterToolAuto",
    "CodeInterpreterContainerCodeInterpreterToolAutoNetworkPolicy",
    "ImageGeneration",
    "ImageGenerationInputImageMask",
    "LocalShell",
WebSearchToolFilters = web_search_tool.Filters
WebSearchToolUserLocation = web_search_tool.UserLocation
CodeInterpreterContainerCodeInterpreterToolAutoNetworkPolicy: TypeAlias = Annotated[
class CodeInterpreterContainerCodeInterpreterToolAuto(BaseModel):
    """Configuration for a code interpreter container.
    Optionally specify the IDs of the files to run the code on.
    type: Literal["auto"]
    """The memory limit for the code interpreter container."""
    network_policy: Optional[CodeInterpreterContainerCodeInterpreterToolAutoNetworkPolicy] = None
CodeInterpreterContainer: TypeAlias = Union[str, CodeInterpreterContainerCodeInterpreterToolAuto]
    """A tool that runs Python code to help generate a response to a prompt."""
    container: CodeInterpreterContainer
    """The code interpreter container.
    Can be a container ID or an object that specifies uploaded file IDs to make
    available to your code, along with an optional `memory_limit` setting.
    """The type of the code interpreter tool. Always `code_interpreter`."""
class ImageGenerationInputImageMask(BaseModel):
    """Optional mask for inpainting.
    Contains `image_url`
    (string, optional) and `file_id` (string, optional).
    """File ID for the mask image."""
    """Base64-encoded mask image."""
class ImageGeneration(BaseModel):
    """A tool that generates images using the GPT image models."""
    type: Literal["image_generation"]
    """The type of the image generation tool. Always `image_generation`."""
    action: Optional[Literal["generate", "edit", "auto"]] = None
    """Whether to generate a new image or edit an existing image. Default: `auto`."""
    background: Optional[Literal["transparent", "opaque", "auto"]] = None
    """Background type for the generated image.
    One of `transparent`, `opaque`, or `auto`. Default: `auto`.
    input_fidelity: Optional[Literal["high", "low"]] = None
    input_image_mask: Optional[ImageGenerationInputImageMask] = None
    Contains `image_url` (string, optional) and `file_id` (string, optional).
    model: Union[str, Literal["gpt-image-1", "gpt-image-1-mini", "gpt-image-1.5"], None] = None
    """The image generation model to use. Default: `gpt-image-1`."""
    moderation: Optional[Literal["auto", "low"]] = None
    """Moderation level for the generated image. Default: `auto`."""
    output_compression: Optional[int] = None
    """Compression level for the output image. Default: 100."""
    """The output format of the generated image.
    One of `png`, `webp`, or `jpeg`. Default: `png`.
    partial_images: Optional[int] = None
    Number of partial images to generate in streaming mode, from 0 (default value)
    to 3.
    quality: Optional[Literal["low", "medium", "high", "auto"]] = None
    """The quality of the generated image.
    One of `low`, `medium`, `high`, or `auto`. Default: `auto`.
    size: Optional[Literal["1024x1024", "1024x1536", "1536x1024", "auto"]] = None
    """The size of the generated image.
    One of `1024x1024`, `1024x1536`, `1536x1024`, or `auto`. Default: `auto`.
class LocalShell(BaseModel):
    """A tool that allows the model to execute shell commands in a local environment."""
    type: Literal["local_shell"]
    """The type of the local shell tool. Always `local_shell`."""
Tool: TypeAlias = Annotated[
        FunctionTool,
        FileSearchTool,
        ComputerTool,
        ComputerUsePreviewTool,
        WebSearchTool,
        Mcp,
        CodeInterpreter,
        ImageGeneration,
        LocalShell,
        FunctionShellTool,
        CustomTool,
        NamespaceTool,
        ToolSearchTool,
        WebSearchPreviewTool,
        ApplyPatchTool,
"""Messages for tools."""
from typing import Any, Literal, cast, overload
from pydantic import Field, model_validator
from typing_extensions import NotRequired, TypedDict, override
from langchain_core.messages.base import BaseMessage, BaseMessageChunk, merge_content
from langchain_core.messages.content import InvalidToolCall
from langchain_core.utils._merge import merge_dicts, merge_obj
class ToolOutputMixin:
    """Mixin for objects that tools can return directly.
    If a custom BaseTool is invoked with a `ToolCall` and the output of custom code is
    not an instance of `ToolOutputMixin`, the output will automatically be coerced to
    a string and wrapped in a `ToolMessage`.
class ToolMessage(BaseMessage, ToolOutputMixin):
    """Message for passing the result of executing a tool back to a model.
    `ToolMessage` objects contain the result of a tool invocation. Typically, the result
    is encoded inside the `content` field.
    `tool_call_id` is used to associate the tool call request with the tool call
    response. Useful in situations where a chat model is able to request multiple tool
    calls in parallel.
        A `ToolMessage` representing a result of `42` from a tool call with id
        from langchain_core.messages import ToolMessage
        ToolMessage(content="42", tool_call_id="call_Jja7J89XsjrOLA5r!MEOW!SL")
        A `ToolMessage` where only part of the tool output is sent to the model
        and the full output is passed in to artifact.
        tool_output = {
            "stdout": "From the graph we can see that the correlation between "
            "x and y is ...",
            "stderr": None,
            "artifacts": {"type": "image", "base64_data": "/9j/4gIcSU..."},
        ToolMessage(
            content=tool_output["stdout"],
            artifact=tool_output,
            tool_call_id="call_Jja7J89XsjrOLA5r!MEOW!SL",
    type: Literal["tool"] = "tool"
    """The type of the message (used for serialization)."""
    artifact: Any = None
    """Artifact of the Tool execution which is not meant to be sent to the model.
    Should only be specified if it is different from the message content, e.g. if only
    a subset of the full tool output is being passed as message content but the full
    output is needed in other parts of the code.
    status: Literal["success", "error"] = "success"
    """Status of the tool invocation."""
    additional_kwargs: dict = Field(default_factory=dict, repr=False)
    """Currently inherited from `BaseMessage`, but not used."""
    response_metadata: dict = Field(default_factory=dict, repr=False)
    def coerce_args(cls, values: dict) -> dict:
        """Coerce the model arguments to the correct types.
            values: The model arguments.
        content = values["content"]
        if isinstance(content, tuple):
            content = list(content)
        if not isinstance(content, (str, list)):
                values["content"] = str(content)
                    "ToolMessage content should be a string or a list of string/dicts. "
                    f"Received:\n\n{content=}\n\n which could not be coerced into a "
                    "string."
            values["content"] = []
            for i, x in enumerate(content):
                if not isinstance(x, (str, dict)):
                        values["content"].append(str(x))
                            "ToolMessage content should be a string or a list of "
                            "string/dicts. Received a list but "
                            f"element ToolMessage.content[{i}] is not a dict and could "
                            f"not be coerced to a string.:\n\n{x}"
                    values["content"].append(x)
        tool_call_id = values["tool_call_id"]
        if isinstance(tool_call_id, (UUID, int, float)):
            values["tool_call_id"] = str(tool_call_id)
        """Initialize a `ToolMessage`.
            **kwargs: Additional fields.
                content=cast("str | list[str | dict]", content_blocks),
class ToolMessageChunk(ToolMessage, BaseMessageChunk):
    """Tool Message chunk."""
    type: Literal["ToolMessageChunk"] = "ToolMessageChunk"  # type: ignore[assignment]
        if isinstance(other, ToolMessageChunk):
            if self.tool_call_id != other.tool_call_id:
                msg = "Cannot concatenate ToolMessageChunks with different names."
                tool_call_id=self.tool_call_id,
                artifact=merge_obj(self.artifact, other.artifact),
                status=_merge_status(self.status, other.status),
        This represents a request to call the tool named `'foo'` with arguments
        `{"a": 1}` and an identifier of `'123'`.
        `tool_call` may also be used as a factory to create a `ToolCall`. Benefits
    """The arguments to the tool call as a dictionary."""
    type: NotRequired[Literal["tool_call"]]
def tool_call(
    id: str | None,
    """Create a tool call.
        args: The arguments to the tool call as a dictionary.
        id: An identifier associated with the tool call.
        The created tool call.
    return ToolCall(name=name, args=args, id=id, type="tool_call")
    When merging `ToolCallChunk` objects (e.g., via `AIMessageChunk.__add__`), all
    string attributes are concatenated. Chunks are only merged if their values of
    `index` are equal and not `None`.
    """The arguments to the tool call as a JSON-parseable string."""
    index: int | None
    """The index of the tool call in a sequence.
    Used for merging chunks.
    type: NotRequired[Literal["tool_call_chunk"]]
def tool_call_chunk(
    args: str | None = None,
    index: int | None = None,
) -> ToolCallChunk:
    """Create a tool call chunk.
        args: The arguments to the tool call as a JSON string.
        index: The index of the tool call in a sequence.
        The created tool call chunk.
    return ToolCallChunk(
        name=name, args=args, id=id, index=index, type="tool_call_chunk"
def invalid_tool_call(
    error: str | None = None,
) -> InvalidToolCall:
    """Create an invalid tool call.
        error: An error message associated with the tool call.
        The created invalid tool call.
    return InvalidToolCall(
        name=name, args=args, id=id, error=error, type="invalid_tool_call"
def default_tool_parser(
    raw_tool_calls: list[dict],
) -> tuple[list[ToolCall], list[InvalidToolCall]]:
    """Best-effort parsing of tools.
        raw_tool_calls: List of raw tool call dicts to parse.
        A list of tool calls and invalid tool calls.
    tool_calls = []
    invalid_tool_calls = []
    for raw_tool_call in raw_tool_calls:
        if "function" not in raw_tool_call:
        function_name = raw_tool_call["function"]["name"]
            function_args = json.loads(raw_tool_call["function"]["arguments"])
            parsed = tool_call(
                name=function_name or "",
                args=function_args or {},
                id=raw_tool_call.get("id"),
            tool_calls.append(parsed)
            invalid_tool_calls.append(
                invalid_tool_call(
                    name=function_name,
                    args=raw_tool_call["function"]["arguments"],
                    error=None,
    return tool_calls, invalid_tool_calls
def default_tool_chunk_parser(raw_tool_calls: list[dict]) -> list[ToolCallChunk]:
    """Best-effort parsing of tool chunks.
        List of parsed ToolCallChunk objects.
    tool_call_chunks = []
    for tool_call in raw_tool_calls:
        if "function" not in tool_call:
            function_args = None
            function_name = None
            function_args = tool_call["function"]["arguments"]
            function_name = tool_call["function"]["name"]
        parsed = tool_call_chunk(
            args=function_args,
            id=tool_call.get("id"),
            index=tool_call.get("index"),
        tool_call_chunks.append(parsed)
    return tool_call_chunks
def _merge_status(
    left: Literal["success", "error"], right: Literal["success", "error"]
) -> Literal["success", "error"]:
    return "error" if "error" in {left, right} else "success"
from langchain_classic.tools.retriever import create_retriever_tool
__all__ = ["create_retriever_tool"]
    from langchain_community.agent_toolkits.nla.tool import NLATool
DEPRECATED_LOOKUP = {"NLATool": "langchain_community.agent_toolkits.nla.tool"}
    "NLATool",
    from langchain_community.tools import ArxivQueryRun
    from langchain_community.tools.arxiv.tool import ArxivInput
    "ArxivInput": "langchain_community.tools.arxiv.tool",
    "ArxivQueryRun": "langchain_community.tools",
    "ArxivInput",
    from langchain_community.tools import BearlyInterpreterTool
    from langchain_community.tools.bearly.tool import (
        BearlyInterpreterToolArguments,
        FileInfo,
    "BearlyInterpreterToolArguments": "langchain_community.tools.bearly.tool",
    "FileInfo": "langchain_community.tools.bearly.tool",
    "BearlyInterpreterTool": "langchain_community.tools",
    "BearlyInterpreterToolArguments",
    "FileInfo",
    from langchain_community.tools import BraveSearch
DEPRECATED_LOOKUP = {"BraveSearch": "langchain_community.tools"}
    from langchain_community.tools.clickup.tool import ClickupAction
DEPRECATED_LOOKUP = {"ClickupAction": "langchain_community.tools.clickup.tool"}
    "ClickupAction",
    from langchain_community.tools import DuckDuckGoSearchResults, DuckDuckGoSearchRun
    from langchain_community.tools.ddg_search.tool import DDGInput, DuckDuckGoSearchTool
    "DDGInput": "langchain_community.tools.ddg_search.tool",
    "DuckDuckGoSearchRun": "langchain_community.tools",
    "DuckDuckGoSearchResults": "langchain_community.tools",
    "DuckDuckGoSearchTool": "langchain_community.tools.ddg_search.tool",
    "DDGInput",
    "DuckDuckGoSearchTool",
    from langchain_community.tools import E2BDataAnalysisTool
    from langchain_community.tools.e2b_data_analysis.tool import (
        E2BDataAnalysisToolArguments,
        UploadedFile,
    "UploadedFile": "langchain_community.tools.e2b_data_analysis.tool",
    "E2BDataAnalysisToolArguments": "langchain_community.tools.e2b_data_analysis.tool",
    "E2BDataAnalysisTool": "langchain_community.tools",
    "E2BDataAnalysisToolArguments",
    "UploadedFile",
    from langchain_community.tools.github.tool import GitHubAction
DEPRECATED_LOOKUP = {"GitHubAction": "langchain_community.tools.github.tool"}
    "GitHubAction",
    from langchain_community.tools.gitlab.tool import GitLabAction
DEPRECATED_LOOKUP = {"GitLabAction": "langchain_community.tools.gitlab.tool"}
    "GitLabAction",
    from langchain_community.tools.google_places.tool import GooglePlacesSchema
    "GooglePlacesSchema": "langchain_community.tools.google_places.tool",
    "GooglePlacesTool": "langchain_community.tools",
    "GooglePlacesSchema",
    from langchain_community.tools import BaseGraphQLTool
DEPRECATED_LOOKUP = {"BaseGraphQLTool": "langchain_community.tools"}
    from langchain_community.tools import StdInInquireTool
DEPRECATED_LOOKUP = {"StdInInquireTool": "langchain_community.tools"}
"""This module provides dynamic access to deprecated Jira tools.
When attributes like `JiraAction` are accessed, they are redirected to their new
locations in `langchain_community.tools`. This ensures backward compatibility
while warning developers about deprecation.
    JiraAction (deprecated): Dynamically loaded from langchain_community.tools.
    from langchain_community.tools import JiraAction
DEPRECATED_LOOKUP = {"JiraAction": "langchain_community.tools"}
    """Dynamically retrieve attributes from the updated module path.
        name: The name of the attribute to import.
        The resolved attribute from the updated path.
"""This module provides dynamic access to deprecated JSON tools in LangChain.
It ensures backward compatibility by forwarding references such as
`JsonGetValueTool`, `JsonListKeysTool`, and `JsonSpec` to their updated
locations within the `langchain_community.tools` namespace.
This setup allows legacy code to continue working while guiding developers
toward using the updated module paths.
    from langchain_community.tools import JsonGetValueTool, JsonListKeysTool
    from langchain_community.tools.json.tool import JsonSpec
    "JsonSpec": "langchain_community.tools.json.tool",
    "JsonListKeysTool": "langchain_community.tools",
    "JsonGetValueTool": "langchain_community.tools",
    This method is used to resolve deprecated attribute imports
    at runtime and forward them to their new locations.
        The resolved attribute from the appropriate updated module.
    "JsonSpec",
    from langchain_community.tools.memorize.tool import Memorize, TrainableLLM
    "TrainableLLM": "langchain_community.tools.memorize.tool",
    "Memorize": "langchain_community.tools.memorize.tool",
    "TrainableLLM",
    from langchain_community.tools import MerriamWebsterQueryRun
DEPRECATED_LOOKUP = {"MerriamWebsterQueryRun": "langchain_community.tools"}
    from langchain_community.tools import NasaAction
DEPRECATED_LOOKUP = {"NasaAction": "langchain_community.tools"}
    from langchain_community.tools.nuclia.tool import NUASchema, NucliaUnderstandingAPI
    "NUASchema": "langchain_community.tools.nuclia.tool",
    "NucliaUnderstandingAPI": "langchain_community.tools.nuclia.tool",
    "NUASchema",
        InfoPowerBITool,
        ListPowerBITool,
        QueryPowerBITool,
    "QueryPowerBITool": "langchain_community.tools",
    "InfoPowerBITool": "langchain_community.tools",
    "ListPowerBITool": "langchain_community.tools",
    from langchain_community.tools import PubmedQueryRun
DEPRECATED_LOOKUP = {"PubmedQueryRun": "langchain_community.tools"}
    from langchain_community.tools import RedditSearchRun, RedditSearchSchema
    "RedditSearchSchema": "langchain_community.tools",
    "RedditSearchRun": "langchain_community.tools",
    "RedditSearchSchema",
        BaseRequestsTool,
        RequestsDeleteTool,
        RequestsGetTool,
        RequestsPatchTool,
        RequestsPostTool,
        RequestsPutTool,
    "BaseRequestsTool": "langchain_community.tools",
    "RequestsGetTool": "langchain_community.tools",
    "RequestsPostTool": "langchain_community.tools",
    "RequestsPatchTool": "langchain_community.tools",
    "RequestsPutTool": "langchain_community.tools",
    "RequestsDeleteTool": "langchain_community.tools",
    from langchain_community.tools import SceneXplainTool
    from langchain_community.tools.scenexplain.tool import SceneXplainInput
    "SceneXplainInput": "langchain_community.tools.scenexplain.tool",
    "SceneXplainTool": "langchain_community.tools",
    "SceneXplainInput",
    from langchain_community.tools import SearxSearchResults, SearxSearchRun
    "SearxSearchRun": "langchain_community.tools",
    "SearxSearchResults": "langchain_community.tools",
    from langchain_community.tools.shell.tool import ShellInput
    "ShellInput": "langchain_community.tools.shell.tool",
    "ShellTool": "langchain_community.tools",
    "ShellInput",
    from langchain_community.tools import SleepTool
    from langchain_community.tools.sleep.tool import SleepInput
    "SleepInput": "langchain_community.tools.sleep.tool",
    "SleepTool": "langchain_community.tools",
    "SleepInput",
        BaseSparkSQLTool,
        InfoSparkSQLTool,
        ListSparkSQLTool,
        QueryCheckerTool,
        QuerySparkSQLTool,
    "BaseSparkSQLTool": "langchain_community.tools",
    "QuerySparkSQLTool": "langchain_community.tools",
    "InfoSparkSQLTool": "langchain_community.tools",
    "ListSparkSQLTool": "langchain_community.tools",
    "QueryCheckerTool": "langchain_community.tools",
        BaseSQLDatabaseTool,
        InfoSQLDatabaseTool,
        ListSQLDatabaseTool,
        QuerySQLCheckerTool,
        QuerySQLDataBaseTool,
    "BaseSQLDatabaseTool": "langchain_community.tools",
    "QuerySQLDataBaseTool": "langchain_community.tools",
    "InfoSQLDatabaseTool": "langchain_community.tools",
    "ListSQLDatabaseTool": "langchain_community.tools",
    "QuerySQLCheckerTool": "langchain_community.tools",
    from langchain_community.tools import StackExchangeTool
DEPRECATED_LOOKUP = {"StackExchangeTool": "langchain_community.tools"}
    from langchain_community.tools import SteamWebAPIQueryRun
DEPRECATED_LOOKUP = {"SteamWebAPIQueryRun": "langchain_community.tools"}
    from langchain_community.tools.steamship_image_generation.tool import ModelName
    "ModelName": "langchain_community.tools.steamship_image_generation.tool",
    "SteamshipImageGenerationTool": "langchain_community.tools",
    "ModelName",
        TavilyInput,
    "TavilyInput": "langchain_community.tools.tavily_search.tool",
    "TavilyInput",
        VectorStoreQATool,
        VectorStoreQAWithSourcesTool,
    "VectorStoreQATool": "langchain_community.tools",
    "VectorStoreQAWithSourcesTool": "langchain_community.tools",
    from langchain_community.tools import WikipediaQueryRun
DEPRECATED_LOOKUP = {"WikipediaQueryRun": "langchain_community.tools"}
"""This module provides dynamic access to deprecated Zapier tools in LangChain.
It supports backward compatibility by forwarding references such as
`ZapierNLAListActions` and `ZapierNLARunAction` to their updated locations
in the `langchain_community.tools` package.
Developers using older import paths will continue to function, while LangChain
internally redirects access to the newer, supported module structure.
"""A tool to parse and pretty-print JSON5.
    $ echo '{foo:"bar"}' | python -m json5
        foo: 'bar',
    $ echo '{foo:"bar"}' | python -m json5 --as-json
import json5
from json5.host import Host
from json5.version import __version__
QUOTE_STYLES = {q.value: q for q in json5.QuoteStyle}
def main(argv=None, host=None):
    host = host or Host()
    args = _parse_args(host, argv)
    if args.version:
        host.print(__version__)
    if args.cmd:
        inp = args.cmd
    elif args.file == '-':
        inp = host.stdin.read()
        inp = host.read_text_file(args.file)
    if args.indent == 'None':
        args.indent = None
            args.indent = int(args.indent)
    if args.as_json:
        args.quote_keys = True
        args.trailing_commas = False
        args.quote_style = json5.QuoteStyle.ALWAYS_DOUBLE.value
    obj = json5.loads(inp, strict=args.strict)
    s = json5.dumps(
        indent=args.indent,
        quote_keys=args.quote_keys,
        trailing_commas=args.trailing_commas,
        quote_style=QUOTE_STYLES[args.quote_style],
    host.print(s)
class _HostedArgumentParser(argparse.ArgumentParser):
    """An argument parser that plays nicely w/ host objects."""
    def __init__(self, host, **kwargs):
    def exit(self, status=0, message=None):
            self._print_message(message, self.host.stderr)
        sys.exit(status)
        self.host.print(f'usage: {self.usage}', end='', file=self.host.stderr)
        self.host.print('    -h/--help for help\n', file=self.host.stderr)
        self.exit(2, f'error: {message}\n')
    def print_help(self, file=None):
        self.host.print(self.format_help(), file=file)
def _parse_args(host, argv):
    usage = 'json5 [options] [FILE]\n'
    parser = _HostedArgumentParser(
        prog='json5',
        usage=usage,
        description=__doc__,
        '-V',
        help=f'show JSON5 library version ({__version__})',
        metavar='STR',
        dest='cmd',
        help='inline json5 string to read instead of reading from a file',
        '--as-json',
        dest='as_json',
        action='store_const',
        const=True,
        help='output as JSON (same as --quote-keys --no-trailing-commas)',
        '--indent',
        dest='indent',
        default=4,
        help='amount to indent each line (default is 4 spaces)',
        '--quote-keys',
        help='quote all object keys',
        '--no-quote-keys',
        action='store_false',
        dest='quote_keys',
        help="don't quote object keys that are identifiers"
        ' (this is the default)',
        '--trailing-commas',
        help='add commas after the last item in multi-line '
        'objects and arrays (this is the default)',
        '--no-trailing-commas',
        dest='trailing_commas',
        help='do not add commas after the last item in '
        'multi-line lists and objects',
        '--strict',
        help='Do not allow control characters (\\x00-\\x1f) in strings '
        '(default)',
        '--no-strict',
        dest='strict',
        help='Allow control characters (\\x00-\\x1f) in strings',
        '--quote-style',
        default='always_double',
        choices=QUOTE_STYLES.keys(),
        help='Controls how strings are encoded. By default they are always '
        'double-quoted ("always_double")',
        metavar='FILE',
        nargs='?',
        default='-',
        help='optional file to read JSON5 document from; if '
        'not specified or "-", will read from stdin '
        'instead',
    return parser.parse_args(argv)
