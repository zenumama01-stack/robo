from .chatkit_widget_item import ChatKitWidgetItem
from .chatkit_thread_user_message_item import ChatKitThreadUserMessageItem
from .chatkit_thread_assistant_message_item import ChatKitThreadAssistantMessageItem
    "ChatKitThreadItemList",
    "Data",
    "DataChatKitClientToolCall",
    "DataChatKitTask",
    "DataChatKitTaskGroup",
    "DataChatKitTaskGroupTask",
class DataChatKitClientToolCall(BaseModel):
    """Record of a client side tool invocation initiated by the assistant."""
    """JSON-encoded arguments that were sent to the tool."""
    """Identifier for the client tool call."""
    """Tool name that was invoked."""
    output: Optional[str] = None
    """JSON-encoded output captured from the tool.
    Defaults to null while execution is in progress.
    status: Literal["in_progress", "completed"]
    """Execution status for the tool call."""
    type: Literal["chatkit.client_tool_call"]
    """Type discriminator that is always `chatkit.client_tool_call`."""
class DataChatKitTask(BaseModel):
    """Task emitted by the workflow to show progress and status updates."""
    heading: Optional[str] = None
    """Optional heading for the task. Defaults to null when not provided."""
    summary: Optional[str] = None
    """Optional summary that describes the task. Defaults to null when omitted."""
    task_type: Literal["custom", "thought"]
    """Subtype for the task."""
    type: Literal["chatkit.task"]
    """Type discriminator that is always `chatkit.task`."""
class DataChatKitTaskGroupTask(BaseModel):
    """Task entry that appears within a TaskGroup."""
    """Optional heading for the grouped task. Defaults to null when not provided."""
    """Optional summary that describes the grouped task.
    Defaults to null when omitted.
    type: Literal["custom", "thought"]
    """Subtype for the grouped task."""
class DataChatKitTaskGroup(BaseModel):
    """Collection of workflow tasks grouped together in the thread."""
    tasks: List[DataChatKitTaskGroupTask]
    """Tasks included in the group."""
    type: Literal["chatkit.task_group"]
    """Type discriminator that is always `chatkit.task_group`."""
Data: TypeAlias = Annotated[
        ChatKitThreadUserMessageItem,
        ChatKitThreadAssistantMessageItem,
        ChatKitWidgetItem,
        DataChatKitClientToolCall,
        DataChatKitTask,
        DataChatKitTaskGroup,
class ChatKitThreadItemList(BaseModel):
    """A paginated list of thread items rendered for the ChatKit API."""
    data: List[Data]
