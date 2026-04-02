from typing import Union, Iterable, Optional
from ..._utils import path_template, maybe_transform, async_maybe_transform
from ...pagination import SyncCursorPage, AsyncCursorPage
from ...types.beta import (
    assistant_list_params,
    assistant_create_params,
    assistant_update_params,
from ..._base_client import AsyncPaginator, make_request_options
from ...types.beta.assistant import Assistant
from ...types.shared.chat_model import ChatModel
from ...types.beta.assistant_deleted import AssistantDeleted
from ...types.shared_params.metadata import Metadata
from ...types.shared.reasoning_effort import ReasoningEffort
from ...types.beta.assistant_tool_param import AssistantToolParam
from ...types.beta.assistant_response_format_option_param import AssistantResponseFormatOptionParam
__all__ = ["Assistants", "AsyncAssistants"]
class Assistants(SyncAPIResource):
    """Build Assistants that can call models and use tools."""
    def with_raw_response(self) -> AssistantsWithRawResponse:
        return AssistantsWithRawResponse(self)
    def with_streaming_response(self) -> AssistantsWithStreamingResponse:
        return AssistantsWithStreamingResponse(self)
    @typing_extensions.deprecated("deprecated")
        model: Union[str, ChatModel],
        description: Optional[str] | Omit = omit,
        instructions: Optional[str] | Omit = omit,
        name: Optional[str] | Omit = omit,
        reasoning_effort: Optional[ReasoningEffort] | Omit = omit,
        response_format: Optional[AssistantResponseFormatOptionParam] | Omit = omit,
        tool_resources: Optional[assistant_create_params.ToolResources] | Omit = omit,
        tools: Iterable[AssistantToolParam] | Omit = omit,
    ) -> Assistant:
        Create an assistant with a model and instructions.
          description: The description of the assistant. The maximum length is 512 characters.
          instructions: The system instructions that the assistant uses. The maximum length is 256,000
          name: The name of the assistant. The maximum length is 256 characters.
          reasoning_effort: Constrains effort on reasoning for
              [reasoning models](https://platform.openai.com/docs/guides/reasoning). Currently
              supported values are `none`, `minimal`, `low`, `medium`, `high`, and `xhigh`.
              Reducing reasoning effort can result in faster responses and fewer tokens used
              on reasoning in a response.
              - `gpt-5.1` defaults to `none`, which does not perform reasoning. The supported
                reasoning values for `gpt-5.1` are `none`, `low`, `medium`, and `high`. Tool
                calls are supported for all reasoning values in gpt-5.1.
              - All models before `gpt-5.1` default to `medium` reasoning effort, and do not
                support `none`.
              - The `gpt-5-pro` model defaults to (and only supports) `high` reasoning effort.
              - `xhigh` is supported for all models after `gpt-5.1-codex-max`.
          response_format: Specifies the format that the model must output. Compatible with
              [GPT-4o](https://platform.openai.com/docs/models#gpt-4o),
              [GPT-4 Turbo](https://platform.openai.com/docs/models#gpt-4-turbo-and-gpt-4),
              and all GPT-3.5 Turbo models since `gpt-3.5-turbo-1106`.
              Setting to `{ "type": "json_schema", "json_schema": {...} }` enables Structured
              Outputs which ensures the model will match your supplied JSON schema. Learn more
              in the
              [Structured Outputs guide](https://platform.openai.com/docs/guides/structured-outputs).
              Setting to `{ "type": "json_object" }` enables JSON mode, which ensures the
              message the model generates is valid JSON.
              **Important:** when using JSON mode, you **must** also instruct the model to
              produce JSON yourself via a system or user message. Without this, the model may
              generate an unending stream of whitespace until the generation reaches the token
              limit, resulting in a long-running and seemingly "stuck" request. Also note that
              the message content may be partially cut off if `finish_reason="length"`, which
              indicates the generation exceeded `max_tokens` or the conversation exceeded the
              max context length.
          tool_resources: A set of resources that are used by the assistant's tools. The resources are
              specific to the type of tool. For example, the `code_interpreter` tool requires
              a list of file IDs, while the `file_search` tool requires a list of vector store
              IDs.
          tools: A list of tool enabled on the assistant. There can be a maximum of 128 tools per
              assistant. Tools can be of types `code_interpreter`, `file_search`, or
              `function`.
              We generally recommend altering this or temperature but not both.
        extra_headers = {"OpenAI-Beta": "assistants=v2", **(extra_headers or {})}
            "/assistants",
                    "description": description,
                    "reasoning_effort": reasoning_effort,
                    "tool_resources": tool_resources,
                    "tools": tools,
                assistant_create_params.AssistantCreateParams,
            cast_to=Assistant,
        assistant_id: str,
        Retrieves an assistant.
        if not assistant_id:
            raise ValueError(f"Expected a non-empty value for `assistant_id` but received {assistant_id!r}")
            path_template("/assistants/{assistant_id}", assistant_id=assistant_id),
    def update(
        model: Union[
                "gpt-5",
                "gpt-5-mini",
                "gpt-5-nano",
                "gpt-5-2025-08-07",
                "gpt-5-mini-2025-08-07",
                "gpt-5-nano-2025-08-07",
                "gpt-4.1",
                "gpt-4.1-mini",
                "gpt-4.1-nano",
                "gpt-4.1-2025-04-14",
                "gpt-4.1-mini-2025-04-14",
                "gpt-4.1-nano-2025-04-14",
                "o3-mini",
                "o3-mini-2025-01-31",
                "o1",
                "o1-2024-12-17",
                "gpt-4o",
                "gpt-4o-2024-11-20",
                "gpt-4o-2024-08-06",
                "gpt-4o-2024-05-13",
                "gpt-4o-mini",
                "gpt-4o-mini-2024-07-18",
                "gpt-4.5-preview",
                "gpt-4.5-preview-2025-02-27",
                "gpt-4-turbo",
                "gpt-4-turbo-2024-04-09",
                "gpt-4-0125-preview",
                "gpt-4-turbo-preview",
                "gpt-4-1106-preview",
                "gpt-4-vision-preview",
                "gpt-4",
                "gpt-4-0314",
                "gpt-4-0613",
                "gpt-4-32k",
                "gpt-4-32k-0314",
                "gpt-4-32k-0613",
                "gpt-3.5-turbo",
                "gpt-3.5-turbo-16k",
                "gpt-3.5-turbo-0613",
                "gpt-3.5-turbo-1106",
                "gpt-3.5-turbo-0125",
                "gpt-3.5-turbo-16k-0613",
        tool_resources: Optional[assistant_update_params.ToolResources] | Omit = omit,
        """Modifies an assistant.
          description: The description of the assistant.
        The maximum length is 512 characters.
                assistant_update_params.AssistantUpdateParams,
        before: str | Omit = omit,
    ) -> SyncCursorPage[Assistant]:
        """Returns a list of assistants.
          before: A cursor for use in pagination. `before` is an object ID that defines your place
              starting with obj_foo, your subsequent call can include before=obj_foo in order
              to fetch the previous page of the list.
            page=SyncCursorPage[Assistant],
                        "before": before,
                    assistant_list_params.AssistantListParams,
            model=Assistant,
    ) -> AssistantDeleted:
        Delete an assistant.
            cast_to=AssistantDeleted,
class AsyncAssistants(AsyncAPIResource):
    def with_raw_response(self) -> AsyncAssistantsWithRawResponse:
        return AsyncAssistantsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncAssistantsWithStreamingResponse:
        return AsyncAssistantsWithStreamingResponse(self)
    async def update(
    ) -> AsyncPaginator[Assistant, AsyncCursorPage[Assistant]]:
            page=AsyncCursorPage[Assistant],
class AssistantsWithRawResponse:
    def __init__(self, assistants: Assistants) -> None:
        self._assistants = assistants
        self.create = (  # pyright: ignore[reportDeprecated]
                assistants.create,  # pyright: ignore[reportDeprecated],
        self.retrieve = (  # pyright: ignore[reportDeprecated]
                assistants.retrieve,  # pyright: ignore[reportDeprecated],
        self.update = (  # pyright: ignore[reportDeprecated]
                assistants.update,  # pyright: ignore[reportDeprecated],
        self.list = (  # pyright: ignore[reportDeprecated]
                assistants.list,  # pyright: ignore[reportDeprecated],
        self.delete = (  # pyright: ignore[reportDeprecated]
                assistants.delete,  # pyright: ignore[reportDeprecated],
class AsyncAssistantsWithRawResponse:
    def __init__(self, assistants: AsyncAssistants) -> None:
class AssistantsWithStreamingResponse:
class AsyncAssistantsWithStreamingResponse:
            f"/assistants/{assistant_id}",
from typing import Any, Coroutine, Dict, Iterable, Literal, Optional, Union
from openai import AsyncAzureOpenAI, AzureOpenAI
from ...types.llms.openai import (
    AssistantToolParam,
    AsyncCursorPage,
    OpenAICreateThreadParamsMessage,
    OpenAIMessage,
    SyncCursorPage,
from .common_utils import BaseAzureLLM
class AzureAssistantsAPI(BaseAzureLLM):
    def get_azure_client(
        api_version: Optional[str],
        azure_ad_token: Optional[str],
        max_retries: Optional[int],
        client: Optional[AzureOpenAI] = None,
        litellm_params: Optional[dict] = None,
    ) -> AzureOpenAI:
        if client is None:
            azure_client_params = self.initialize_azure_sdk_client(
                litellm_params=litellm_params or {},
                model_name="",
                is_async=False,
            azure_openai_client = AzureOpenAI(**azure_client_params)  # type: ignore
            azure_openai_client = client
        return azure_openai_client
    def async_get_azure_client(
        client: Optional[AsyncAzureOpenAI] = None,
    ) -> AsyncAzureOpenAI:
            azure_openai_client = AsyncAzureOpenAI(**azure_client_params)
            # azure_openai_client = AsyncAzureOpenAI(**data)  # type: ignore
    async def async_get_assistants(
        client: Optional[AsyncAzureOpenAI],
        azure_openai_client = self.async_get_azure_client(
        response = await azure_openai_client.beta.assistants.list()
        aget_assistants: Literal[True], 
    ) -> Coroutine[None, None, AsyncCursorPage[Assistant]]:
        client: Optional[AzureOpenAI],
        aget_assistants: Optional[Literal[False]], 
        aget_assistants=None,
        if aget_assistants is not None and aget_assistants is True:
            return self.async_get_assistants(
        azure_openai_client = self.get_azure_client(
        response = azure_openai_client.beta.assistants.list()
        message_data: dict,
        openai_client = self.async_get_azure_client(
        thread_message: OpenAIMessage = await openai_client.beta.threads.messages.create(  # type: ignore
            thread_id, **message_data  # type: ignore
        response_obj: Optional[OpenAIMessage] = None
        if getattr(thread_message, "status", None) is None:
            thread_message.status = "completed"
            response_obj = OpenAIMessage(**thread_message.dict())
        return response_obj
        a_add_message: Literal[True],
    ) -> Coroutine[None, None, OpenAIMessage]:
        a_add_message: Optional[Literal[False]],
        a_add_message: Optional[bool] = None,
        if a_add_message is not None and a_add_message is True:
            return self.a_add_message(
        openai_client = self.get_azure_client(
        thread_message: OpenAIMessage = openai_client.beta.threads.messages.create(  # type: ignore
    async def async_get_messages(
        response = await openai_client.beta.threads.messages.list(thread_id=thread_id)
        aget_messages: Literal[True],
    ) -> Coroutine[None, None, AsyncCursorPage[OpenAIMessage]]:
        aget_messages: Optional[Literal[False]],
        aget_messages=None,
        if aget_messages is not None and aget_messages is True:
            return self.async_get_messages(
        response = openai_client.beta.threads.messages.list(thread_id=thread_id)
    async def async_create_thread(
        messages: Optional[Iterable[OpenAICreateThreadParamsMessage]],
            data["messages"] = messages  # type: ignore
            data["metadata"] = metadata  # type: ignore
        message_thread = await openai_client.beta.threads.create(**data)  # type: ignore
        return Thread(**message_thread.dict())
        acreate_thread: Literal[True],
    ) -> Coroutine[None, None, Thread]:
        acreate_thread: Optional[Literal[False]],
        acreate_thread=None,
        from litellm.llms.openai.openai import OpenAIAssistantsAPI, MessageData
        # create thread
        message: MessageData = {"role": "user", "content": "Hey, how's it going?"}
        openai_api.create_thread(messages=[message])
        if acreate_thread is not None and acreate_thread is True:
            return self.async_create_thread(
        message_thread = azure_openai_client.beta.threads.create(**data)  # type: ignore
    async def async_get_thread(
        response = await openai_client.beta.threads.retrieve(thread_id=thread_id)
        return Thread(**response.dict())
        aget_thread: Literal[True],
        aget_thread: Optional[Literal[False]],
        aget_thread=None,
        if aget_thread is not None and aget_thread is True:
            return self.async_get_thread(
        response = openai_client.beta.threads.retrieve(thread_id=thread_id)
    # def delete_thread(self):
    #     pass
        additional_instructions: Optional[str],
        instructions: Optional[str],
        metadata: Optional[Dict],
        stream: Optional[bool],
        tools: Optional[Iterable[AssistantToolParam]],
        response = await openai_client.beta.threads.runs.create_and_poll(  # type: ignore
            metadata=metadata,  # type: ignore
    def async_run_thread_stream(
        client: AsyncAzureOpenAI,
        event_handler: Optional[AssistantEventHandler],
        data: Dict[str, Any] = {
            "thread_id": thread_id,
        if event_handler is not None:
            data["event_handler"] = event_handler
        return client.beta.threads.runs.stream(**data)  # type: ignore
        client: AzureOpenAI,
        arun_thread: Literal[True],
    ) -> Coroutine[None, None, Run]:
        arun_thread: Optional[Literal[False]],
        arun_thread=None,
        if arun_thread is not None and arun_thread is True:
            if stream is not None and stream is True:
                azure_client = self.async_get_azure_client(
                return self.async_run_thread_stream(
                    client=azure_client,
            return self.arun_thread(
            return self.run_thread_stream(
                client=openai_client,
        response = openai_client.beta.threads.runs.create_and_poll(  # type: ignore
    # Create Assistant
    async def async_create_assistants(
        create_assistant_data: dict,
        response = await azure_openai_client.beta.assistants.create(
            **create_assistant_data
        async_create_assistants=None,
        if async_create_assistants is not None and async_create_assistants is True:
            return self.async_create_assistants(
        response = azure_openai_client.beta.assistants.create(**create_assistant_data)
    # Delete Assistant
    async def async_delete_assistant(
        response = await azure_openai_client.beta.assistants.delete(
            assistant_id=assistant_id
        async_delete_assistants: Optional[bool] = None,
        if async_delete_assistants is not None and async_delete_assistants is True:
            return self.async_delete_assistant(
        response = azure_openai_client.beta.assistants.delete(assistant_id=assistant_id)
