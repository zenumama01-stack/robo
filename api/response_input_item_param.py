from .easy_input_message_param import EasyInputMessageParam
from .response_output_message_param import ResponseOutputMessageParam
from .response_reasoning_item_param import ResponseReasoningItemParam
from .response_custom_tool_call_param import ResponseCustomToolCallParam
from .response_computer_tool_call_param import ResponseComputerToolCallParam
from .response_function_tool_call_param import ResponseFunctionToolCallParam
from .response_function_web_search_param import ResponseFunctionWebSearchParam
from .response_compaction_item_param_param import ResponseCompactionItemParamParam
from .response_file_search_tool_call_param import ResponseFileSearchToolCallParam
from .response_custom_tool_call_output_param import ResponseCustomToolCallOutputParam
from .response_code_interpreter_tool_call_param import ResponseCodeInterpreterToolCallParam
from .response_tool_search_output_item_param_param import ResponseToolSearchOutputItemParamParam
from .response_function_call_output_item_list_param import ResponseFunctionCallOutputItemListParam
from .response_function_shell_call_output_content_param import ResponseFunctionShellCallOutputContentParam
from .response_computer_tool_call_output_screenshot_param import ResponseComputerToolCallOutputScreenshotParam
    "ResponseInputItemParam",
    content: Required[ResponseInputMessageContentListParam]
    role: Required[Literal["user", "system", "developer"]]
class ComputerCallOutputAcknowledgedSafetyCheck(TypedDict, total=False):
class ComputerCallOutput(TypedDict, total=False):
    output: Required[ResponseComputerToolCallOutputScreenshotParam]
    type: Required[Literal["computer_call_output"]]
    acknowledged_safety_checks: Optional[Iterable[ComputerCallOutputAcknowledgedSafetyCheck]]
    status: Optional[Literal["in_progress", "completed", "incomplete"]]
class FunctionCallOutput(TypedDict, total=False):
    output: Required[Union[str, ResponseFunctionCallOutputItemListParam]]
class ToolSearchCall(TypedDict, total=False):
    arguments: Required[object]
    type: Required[Literal["tool_search_call"]]
    call_id: Optional[str]
    execution: Literal["server", "client"]
class ImageGenerationCall(TypedDict, total=False):
    result: Required[Optional[str]]
    status: Required[Literal["in_progress", "completed", "generating", "failed"]]
    type: Required[Literal["image_generation_call"]]
class LocalShellCallAction(TypedDict, total=False):
    command: Required[SequenceNotStr[str]]
    env: Required[Dict[str, str]]
    type: Required[Literal["exec"]]
    timeout_ms: Optional[int]
    user: Optional[str]
    working_directory: Optional[str]
class LocalShellCall(TypedDict, total=False):
    action: Required[LocalShellCallAction]
    type: Required[Literal["local_shell_call"]]
class LocalShellCallOutput(TypedDict, total=False):
    type: Required[Literal["local_shell_call_output"]]
class ShellCallAction(TypedDict, total=False):
    commands: Required[SequenceNotStr[str]]
    max_output_length: Optional[int]
ShellCallEnvironment: TypeAlias = Union[LocalEnvironmentParam, ContainerReferenceParam]
class ShellCall(TypedDict, total=False):
    action: Required[ShellCallAction]
    type: Required[Literal["shell_call"]]
    environment: Optional[ShellCallEnvironment]
class ShellCallOutput(TypedDict, total=False):
    output: Required[Iterable[ResponseFunctionShellCallOutputContentParam]]
    type: Required[Literal["shell_call_output"]]
class ApplyPatchCallOperationCreateFile(TypedDict, total=False):
    diff: Required[str]
    type: Required[Literal["create_file"]]
class ApplyPatchCallOperationDeleteFile(TypedDict, total=False):
    type: Required[Literal["delete_file"]]
class ApplyPatchCallOperationUpdateFile(TypedDict, total=False):
    type: Required[Literal["update_file"]]
ApplyPatchCallOperation: TypeAlias = Union[
    ApplyPatchCallOperationCreateFile, ApplyPatchCallOperationDeleteFile, ApplyPatchCallOperationUpdateFile
class ApplyPatchCall(TypedDict, total=False):
    operation: Required[ApplyPatchCallOperation]
    status: Required[Literal["in_progress", "completed"]]
    type: Required[Literal["apply_patch_call"]]
class ApplyPatchCallOutput(TypedDict, total=False):
    status: Required[Literal["completed", "failed"]]
    type: Required[Literal["apply_patch_call_output"]]
class McpListToolsTool(TypedDict, total=False):
class McpListTools(TypedDict, total=False):
    tools: Required[Iterable[McpListToolsTool]]
    error: Optional[str]
class McpApprovalRequest(TypedDict, total=False):
class McpApprovalResponse(TypedDict, total=False):
class McpCall(TypedDict, total=False):
    status: Literal["in_progress", "completed", "incomplete", "calling", "failed"]
class ItemReference(TypedDict, total=False):
    type: Optional[Literal["item_reference"]]
ResponseInputItemParam: TypeAlias = Union[
    EasyInputMessageParam,
    ResponseOutputMessageParam,
    ResponseFileSearchToolCallParam,
    ResponseComputerToolCallParam,
    ResponseFunctionWebSearchParam,
    ResponseFunctionToolCallParam,
    ResponseToolSearchOutputItemParamParam,
    ResponseReasoningItemParam,
    ResponseCompactionItemParamParam,
    ResponseCodeInterpreterToolCallParam,
    ResponseCustomToolCallOutputParam,
    ResponseCustomToolCallParam,
