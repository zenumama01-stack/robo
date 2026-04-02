from . import web_search_tool_param
from ..chat import ChatCompletionFunctionToolParam
from .computer_tool_param import ComputerToolParam
from .namespace_tool_param import NamespaceToolParam
from .web_search_tool_param import WebSearchToolParam
from .apply_patch_tool_param import ApplyPatchToolParam
from .tool_search_tool_param import ToolSearchToolParam
from .function_shell_tool_param import FunctionShellToolParam
from .web_search_preview_tool_param import WebSearchPreviewToolParam
from .computer_use_preview_tool_param import ComputerUsePreviewToolParam
    "ToolParam",
WebSearchTool = web_search_tool_param.WebSearchToolParam
WebSearchToolFilters = web_search_tool_param.Filters
WebSearchToolUserLocation = web_search_tool_param.UserLocation
CodeInterpreterContainerCodeInterpreterToolAutoNetworkPolicy: TypeAlias = Union[
    ContainerNetworkPolicyDisabledParam, ContainerNetworkPolicyAllowlistParam
class CodeInterpreterContainerCodeInterpreterToolAuto(TypedDict, total=False):
    network_policy: CodeInterpreterContainerCodeInterpreterToolAutoNetworkPolicy
class CodeInterpreter(TypedDict, total=False):
    container: Required[CodeInterpreterContainer]
class ImageGenerationInputImageMask(TypedDict, total=False):
class ImageGeneration(TypedDict, total=False):
    type: Required[Literal["image_generation"]]
    action: Literal["generate", "edit", "auto"]
    input_image_mask: ImageGenerationInputImageMask
    model: Union[str, Literal["gpt-image-1", "gpt-image-1-mini", "gpt-image-1.5"]]
    moderation: Literal["auto", "low"]
    output_compression: int
    partial_images: int
class LocalShell(TypedDict, total=False):
    type: Required[Literal["local_shell"]]
ToolParam: TypeAlias = Union[
    FileSearchToolParam,
    ComputerToolParam,
    ComputerUsePreviewToolParam,
    WebSearchToolParam,
    FunctionShellToolParam,
    CustomToolParam,
    NamespaceToolParam,
    ToolSearchToolParam,
    WebSearchPreviewToolParam,
    ApplyPatchToolParam,
ParseableToolParam: TypeAlias = Union[ToolParam, ChatCompletionFunctionToolParam]
