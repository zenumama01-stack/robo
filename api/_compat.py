from typing import TYPE_CHECKING, Any, Union, Generic, TypeVar, Callable, cast, overload
from datetime import date, datetime
from typing_extensions import Self, Literal, TypedDict
from pydantic.fields import FieldInfo
from ._types import IncEx, StrBytesIntFloat
_ModelT = TypeVar("_ModelT", bound=pydantic.BaseModel)
# --------------- Pydantic v2, v3 compatibility ---------------
# Pyright incorrectly reports some of our functions as overriding a method when they don't
# pyright: reportIncompatibleMethodOverride=false
PYDANTIC_V1 = pydantic.VERSION.startswith("1.")
    def parse_date(value: date | StrBytesIntFloat) -> date:  # noqa: ARG001
    def parse_datetime(value: Union[datetime, StrBytesIntFloat]) -> datetime:  # noqa: ARG001
    def get_args(t: type[Any]) -> tuple[Any, ...]:  # noqa: ARG001
    def is_union(tp: type[Any] | None) -> bool:  # noqa: ARG001
    def get_origin(t: type[Any]) -> type[Any] | None:  # noqa: ARG001
    def is_literal_type(type_: type[Any]) -> bool:  # noqa: ARG001
    def is_typeddict(type_: type[Any]) -> bool:  # noqa: ARG001
    # v1 re-exports
    if PYDANTIC_V1:
        from pydantic.typing import (
            get_args as get_args,
            is_union as is_union,
            get_origin as get_origin,
            is_typeddict as is_typeddict,
            is_literal_type as is_literal_type,
        from pydantic.datetime_parse import parse_date as parse_date, parse_datetime as parse_datetime
            parse_date as parse_date,
            parse_datetime as parse_datetime,
# refactored config
    from pydantic import ConfigDict as ConfigDict
        # TODO: provide an error message here?
        ConfigDict = None
# renamed methods / properties
def parse_obj(model: type[_ModelT], value: object) -> _ModelT:
        return cast(_ModelT, model.parse_obj(value))  # pyright: ignore[reportDeprecated, reportUnnecessaryCast]
        return model.model_validate(value)
def field_is_required(field: FieldInfo) -> bool:
        return field.required  # type: ignore
    return field.is_required()
def field_get_default(field: FieldInfo) -> Any:
    value = field.get_default()
    from pydantic_core import PydanticUndefined
    if value == PydanticUndefined:
def field_outer_type(field: FieldInfo) -> Any:
        return field.outer_type_  # type: ignore
    return field.annotation
def get_model_config(model: type[pydantic.BaseModel]) -> Any:
        return model.__config__  # type: ignore
    return model.model_config
def get_model_fields(model: type[pydantic.BaseModel]) -> dict[str, FieldInfo]:
        return model.__fields__  # type: ignore
    return model.model_fields
def model_copy(model: _ModelT, *, deep: bool = False) -> _ModelT:
        return model.copy(deep=deep)  # type: ignore
    return model.model_copy(deep=deep)
def model_json(model: pydantic.BaseModel, *, indent: int | None = None) -> str:
        return model.json(indent=indent)  # type: ignore
    return model.model_dump_json(indent=indent)
class _ModelDumpKwargs(TypedDict, total=False):
    by_alias: bool
def model_dump(
    model: pydantic.BaseModel,
    exclude: IncEx | None = None,
    exclude_unset: bool = False,
    exclude_defaults: bool = False,
    warnings: bool = True,
    mode: Literal["json", "python"] = "python",
    by_alias: bool | None = None,
) -> dict[str, Any]:
    if (not PYDANTIC_V1) or hasattr(model, "model_dump"):
        kwargs: _ModelDumpKwargs = {}
        if by_alias is not None:
            kwargs["by_alias"] = by_alias
        return model.model_dump(
            mode=mode,
            exclude=exclude,
            exclude_unset=exclude_unset,
            exclude_defaults=exclude_defaults,
            # warnings are not supported in Pydantic v1
            warnings=True if PYDANTIC_V1 else warnings,
        "dict[str, Any]",
        model.dict(  # pyright: ignore[reportDeprecated, reportUnnecessaryCast]
            exclude=exclude, exclude_unset=exclude_unset, exclude_defaults=exclude_defaults, by_alias=bool(by_alias)
def model_parse(model: type[_ModelT], data: Any) -> _ModelT:
        return model.parse_obj(data)  # pyright: ignore[reportDeprecated]
    return model.model_validate(data)
def model_parse_json(model: type[_ModelT], data: str | bytes) -> _ModelT:
        return model.parse_raw(data)  # pyright: ignore[reportDeprecated]
    return model.model_validate_json(data)
def model_json_schema(model: type[_ModelT]) -> dict[str, Any]:
        return model.schema()  # pyright: ignore[reportDeprecated]
    return model.model_json_schema()
# generic models
    class GenericModel(pydantic.BaseModel): ...
        import pydantic.generics
        class GenericModel(pydantic.generics.GenericModel, pydantic.BaseModel): ...
        # there no longer needs to be a distinction in v2 but
        # we still have to create our own subclass to avoid
        # inconsistent MRO ordering errors
# cached properties
    cached_property = property
    # we define a separate type (copied from typeshed)
    # that represents that `cached_property` is `set`able
    # at runtime, which differs from `@property`.
    # this is a separate type as editors likely special case
    # `@property` and we don't want to cause issues just to have
    # more helpful internal types.
    class typed_cached_property(Generic[_T]):
        func: Callable[[Any], _T]
        attrname: str | None
        def __init__(self, func: Callable[[Any], _T]) -> None: ...
        def __get__(self, instance: None, owner: type[Any] | None = None) -> Self: ...
        def __get__(self, instance: object, owner: type[Any] | None = None) -> _T: ...
        def __get__(self, instance: object, owner: type[Any] | None = None) -> _T | Self:
        def __set_name__(self, owner: type[Any], name: str) -> None: ...
        # __set__ is not defined at runtime, but @cached_property is designed to be settable
        def __set__(self, instance: object, value: _T) -> None: ...
    from functools import cached_property as cached_property
    typed_cached_property = cached_property
import typing_extensions
from typing import Any, Type, Union, Literal, Optional
from typing_extensions import get_args as _get_args, get_origin as _get_origin
from .._types import StrBytesIntFloat
from ._datetime_parse import parse_date as _parse_date, parse_datetime as _parse_datetime
_LITERAL_TYPES = {Literal, typing_extensions.Literal}
def get_args(tp: type[Any]) -> tuple[Any, ...]:
    return _get_args(tp)
def get_origin(tp: type[Any]) -> type[Any] | None:
    return _get_origin(tp)
def is_union(tp: Optional[Type[Any]]) -> bool:
    if sys.version_info < (3, 10):
        return tp is Union  # type: ignore[comparison-overlap]
        import types
        return tp is Union or tp is types.UnionType  # type: ignore[comparison-overlap]
def is_typeddict(tp: Type[Any]) -> bool:
    return typing_extensions.is_typeddict(tp)
def is_literal_type(tp: Type[Any]) -> bool:
    return get_origin(tp) in _LITERAL_TYPES
def parse_date(value: Union[date, StrBytesIntFloat]) -> date:
    return _parse_date(value)
def parse_datetime(value: Union[datetime, StrBytesIntFloat]) -> datetime:
    return _parse_datetime(value)
"""Compatibility helpers for Pydantic v1/v2 with langsmith `Run` objects.
    The generic helpers (`pydantic_to_dict`, `pydantic_copy`) detect Pydanti version
    based on the langsmith `Run` model. They're intended for langsmith objects (`Run`,
    `Example`) which migrate together.
For general Pydantic v1/v2 handling, see `langchain_core.utils.pydantic`.
# Detect Pydantic version once at import time based on Run model
_RUN_IS_PYDANTIC_V2 = hasattr(Run, "model_dump")
def run_to_dict(run: Run, **kwargs: Any) -> dict[str, Any]:
    """Convert run to dict, compatible with both Pydantic v1 and v2.
        run: The run to convert.
        **kwargs: Additional arguments passed to `model_dump`/`dict`.
        Dictionary representation of the run.
    if _RUN_IS_PYDANTIC_V2:
        return run.model_dump(**kwargs)
    return run.dict(**kwargs)  # type: ignore[deprecated]
def run_copy(run: Run, **kwargs: Any) -> Run:
    """Copy run, compatible with both Pydantic v1 and v2.
        run: The run to copy.
        **kwargs: Additional arguments passed to `model_copy`/`copy`.
        A copy of the run.
        return run.model_copy(**kwargs)
    return run.copy(**kwargs)  # type: ignore[deprecated]
def run_construct(**kwargs: Any) -> Run:
    """Construct run without validation, compatible with both Pydantic v1 and v2.
        **kwargs: Fields to set on the run.
        A new `Run` instance constructed without validation.
        return Run.model_construct(**kwargs)
    return Run.construct(**kwargs)  # type: ignore[deprecated]
def pydantic_to_dict(obj: Any, **kwargs: Any) -> dict[str, Any]:
    """Convert any Pydantic model to dict, compatible with both v1 and v2.
        obj: The Pydantic model to convert.
        Dictionary representation of the model.
        return obj.model_dump(**kwargs)  # type: ignore[no-any-return]
    return obj.dict(**kwargs)  # type: ignore[no-any-return]
def pydantic_copy(obj: T, **kwargs: Any) -> T:
    """Copy any Pydantic model, compatible with both v1 and v2.
        obj: The Pydantic model to copy.
        A copy of the model.
        return obj.model_copy(**kwargs)  # type: ignore[attr-defined,no-any-return]
    return obj.copy(**kwargs)  # type: ignore[attr-defined,no-any-return]
def _convert_annotation_from_v1(annotation: types.Annotation) -> dict[str, Any]:
    """Convert LangChain annotation format to Anthropic's native citation format."""
    if annotation["type"] == "non_standard_annotation":
        return annotation["value"]
    if annotation["type"] == "citation":
        if "url" in annotation:
            # web_search_result_location
            out: dict[str, Any] = {}
            if cited_text := annotation.get("cited_text"):
                out["cited_text"] = cited_text
            if "encrypted_index" in annotation.get("extras", {}):
                out["encrypted_index"] = annotation.get("extras", {})["encrypted_index"]
            if "title" in annotation:
                out["title"] = annotation["title"]
            out["type"] = "web_search_result_location"
            out["url"] = annotation.get("url")
            for key, value in annotation.get("extras", {}).items():
                if key not in out:
                    out[key] = value
        if "start_char_index" in annotation.get("extras", {}):
            # char_location
            out = {"type": "char_location"}
            for field in ["cited_text"]:
                if value := annotation.get(field):
                    out[field] = value
            if title := annotation.get("title"):
                out["document_title"] = title
            out = {k: out[k] for k in sorted(out)}
        if "search_result_index" in annotation.get("extras", {}):
            # search_result_location
            out = {"type": "search_result_location"}
            for field in ["cited_text", "title"]:
        if "start_block_index" in annotation.get("extras", {}):
            # content_block_location
            if "document_index" in annotation.get("extras", {}):
                out["document_index"] = annotation.get("extras", {})["document_index"]
                out["document_title"] = annotation["title"]
            out["type"] = "content_block_location"
        if "start_page_number" in annotation.get("extras", {}):
            # page_location
            out = {"type": "page_location"}
        return cast(dict[str, Any], annotation)
def _convert_from_v1_to_anthropic(
    content: list[types.ContentBlock],
    tool_calls: list[types.ToolCall],
    model_provider: str | None,
    new_content: list = []
    for block in content:
        if block["type"] == "text":
            if model_provider == "anthropic" and "annotations" in block:
                new_block: dict[str, Any] = {"type": "text"}
                new_block["citations"] = [
                    _convert_annotation_from_v1(a) for a in block["annotations"]
                if "text" in block:
                    new_block["text"] = block["text"]
                new_block = {"text": block.get("text", ""), "type": "text"}
            new_content.append(new_block)
        elif block["type"] == "tool_call":
            tool_use_block = {
                "name": block.get("name", ""),
                "input": block.get("args", {}),
                "id": block.get("id", ""),
            if "caller" in block.get("extras", {}):
                tool_use_block["caller"] = block["extras"]["caller"]
            new_content.append(tool_use_block)
        elif block["type"] == "tool_call_chunk":
            if isinstance(block["args"], str):
                    input_ = json.loads(block["args"] or "{}")
                    input_ = {}
                input_ = block.get("args") or {}
            new_content.append(
                    "input": input_,
        elif block["type"] == "reasoning" and model_provider == "anthropic":
            new_block = {}
            if "reasoning" in block:
                new_block["thinking"] = block["reasoning"]
            new_block["type"] = "thinking"
            if signature := block.get("extras", {}).get("signature"):
                new_block["signature"] = signature
        elif block["type"] == "server_tool_call" and model_provider == "anthropic":
            if "id" in block:
                new_block["id"] = block["id"]
            new_block["input"] = block.get("args", {})
            if partial_json := block.get("extras", {}).get("partial_json"):
                new_block["input"] = {}
                new_block["partial_json"] = partial_json
            if block.get("name") == "code_interpreter":
                new_block["name"] = "code_execution"
            elif block.get("name") == "remote_mcp":
                if "tool_name" in block.get("extras", {}):
                    new_block["name"] = block["extras"]["tool_name"]
                if "server_name" in block.get("extras", {}):
                    new_block["server_name"] = block["extras"]["server_name"]
                new_block["name"] = block.get("name", "")
            if block.get("name") == "remote_mcp":
                new_block["type"] = "mcp_tool_use"
                new_block["type"] = "server_tool_use"
        elif block["type"] == "server_tool_result" and model_provider == "anthropic":
            if "output" in block:
                new_block["content"] = block["output"]
            server_tool_result_type = block.get("extras", {}).get("block_type", "")
            if server_tool_result_type == "mcp_tool_result":
                new_block["is_error"] = block.get("status") == "error"
            if "tool_call_id" in block:
                new_block["tool_use_id"] = block["tool_call_id"]
            new_block["type"] = server_tool_result_type
            block["type"] == "non_standard"
            and "value" in block
            and model_provider == "anthropic"
            new_content.append(block["value"])
            new_content.append(block)
    return new_content
"""Converts between AIMessage output formats, governed by `output_version`."""
def _convert_from_v1_to_chat_completions(message: AIMessage) -> AIMessage:
    """Convert a v1 message to the Chat Completions format."""
            if isinstance(block, dict):
                block_type = block.get("type")
                    # Strip annotations
                    new_content.append({"type": "text", "text": block["text"]})
                elif block_type in ("reasoning", "tool_call"):
        return message.model_copy(update={"content": new_content})
def _convert_from_v1_to_groq(
) -> tuple[list[dict[str, Any] | str], dict]:
    new_additional_kwargs: dict = {}
    for i, block in enumerate(content):
            new_content.append({"text": block.get("text", ""), "type": "text"})
            block["type"] == "reasoning"
            and (reasoning := block.get("reasoning"))
            and model_provider == "groq"
            new_additional_kwargs["reasoning_content"] = reasoning
        elif block["type"] == "server_tool_call" and model_provider == "groq":
            if "args" in block:
                new_block["arguments"] = json.dumps(block["args"])
            if idx := block.get("extras", {}).get("index"):
                new_block["index"] = idx
            if block.get("name") == "web_search":
                new_block["type"] = "search"
            elif block.get("name") == "code_interpreter":
                new_block["type"] = "python"
                new_block["type"] = ""
            if i < len(content) - 1 and content[i + 1]["type"] == "server_tool_result":
                result = cast("types.ServerToolResult", content[i + 1])
                for k, v in result.get("extras", {}).items():
                    new_block[k] = v  # noqa: PERF403
                if "output" in result:
                    new_block["output"] = result["output"]
                if "executed_tools" not in new_additional_kwargs:
                    new_additional_kwargs["executed_tools"] = []
                new_additional_kwargs["executed_tools"].append(new_block)
        elif block["type"] == "server_tool_result":
    # For consistency with v0 payloads, we cast single text blocks to str
        len(new_content) == 1
        and isinstance(new_content[0], dict)
        and new_content[0].get("type") == "text"
        and (text_content := new_content[0].get("text"))
        and isinstance(text_content, str)
        return text_content, new_additional_kwargs
    return new_content, new_additional_kwargs
"""Derivations of standard content blocks from mistral content."""
from langchain_core.messages.block_translators import register_translator
def _convert_from_v1_to_mistral(
            and isinstance(reasoning, str)
            and model_provider == "mistralai"
                    "thinking": [{"type": "text", "text": reasoning}],
def _convert_to_v1_from_mistral(message: AIMessage) -> list[types.ContentBlock]:
    """Convert mistral message content to v1 format."""
        content_blocks: list[types.ContentBlock] = [
            {"type": "text", "text": message.content}
        content_blocks = []
                content_blocks.append({"type": "text", "text": block})
                if block.get("type") == "text" and isinstance(block.get("text"), str):
                    text_block: types.TextContentBlock = {
                        "text": block["text"],
                    if "index" in block:
                        text_block["index"] = block["index"]
                    content_blocks.append(text_block)
                elif block.get("type") == "thinking" and isinstance(
                    block.get("thinking"), list
                    for sub_block in block["thinking"]:
                            isinstance(sub_block, dict)
                            and sub_block.get("type") == "text"
                            reasoning_block: types.ReasoningContentBlock = {
                                "reasoning": sub_block.get("text", ""),
                                reasoning_block["index"] = block["index"]
                            content_blocks.append(reasoning_block)
                    non_standard_block: types.NonStandardContentBlock = {
                        "type": "non_standard",
                        "value": block,
                    content_blocks.append(non_standard_block)
        len(content_blocks) == 1
        and content_blocks[0].get("type") == "text"
        and content_blocks[0].get("text") == ""
        content_blocks.append(
                "args": tool_call["args"],
                "id": tool_call.get("id"),
    return content_blocks
def translate_content(message: AIMessage) -> list[types.ContentBlock]:
    """Derive standard content blocks from a message with mistral content."""
    return _convert_to_v1_from_mistral(message)
def translate_content_chunk(message: AIMessageChunk) -> list[types.ContentBlock]:
    """Derive standard content blocks from a message chunk with mistral content."""
register_translator("mistralai", translate_content, translate_content_chunk)
"""Go from v1 content blocks to Ollama SDK format."""
def _convert_from_v1_to_ollama(
    model_provider: str | None,  # noqa: ARG001
    """Convert v1 content blocks to Ollama format.
        content: List of v1 `ContentBlock` objects.
        model_provider: The model provider name that generated the v1 content.
        List of content blocks in Ollama format.
        if not isinstance(block, dict) or "type" not in block:
        block_dict = dict(block)  # (For typing)
        # TextContentBlock
        if block_dict["type"] == "text":
            # Note: this drops all other fields/extras
            new_content.append({"type": "text", "text": block_dict["text"]})
        # ReasoningContentBlock
        # Ollama doesn't take reasoning back in
        # In the future, could consider coercing into text as an option?
        # e.g.:
        # if block_dict["type"] == "reasoning":
        #     # Attempt to preserve content in text form
        #     new_content.append({"text": str(block_dict["reasoning"])})
        # ImageContentBlock
        if block_dict["type"] == "image":
            # Already handled in _get_image_from_data_content_block
            new_content.append(block_dict)
        # TODO: AudioContentBlock once models support
        # TODO: FileContentBlock once models support
        # ToolCall -> ???
        # if block_dict["type"] == "tool_call":
        #     function_call = {}
        #     new_content.append(function_call)
        # ToolCallChunk -> ???
        # elif block_dict["type"] == "tool_call_chunk":
        # NonStandardContentBlock
        if block_dict["type"] == "non_standard":
            # Attempt to preserve content in text form
                {"type": "text", "text": str(block_dict.get("value", ""))}
"""Converts between AIMessage output formats, governed by `output_version`.
`output_version` is an attribute on ChatOpenAI.
Supported values are `None`, `'v0'`, and `'responses/v1'`.
`'v0'` corresponds to the format as of `ChatOpenAI` v0.3. For the Responses API, it
stores reasoning and tool outputs in `AIMessage.additional_kwargs`:
        {"type": "text", "text": "Hello, world!", "annotations": [{"type": "foo"}]}
        "reasoning": {
            "id": "rs_123",
            "summary": [{"type": "summary_text", "text": "Reasoning summary"}],
        "tool_outputs": [
                "type": "web_search_call",
                "id": "websearch_123",
        "refusal": "I cannot assist with that.",
    response_metadata={"id": "resp_123"},
    id="msg_123",
`'responses/v1'` is only applicable to the Responses API. It retains information
about response item sequencing and accommodates multiple reasoning items by
representing these items in the content sequence:
            "annotations": [{"type": "foo"}],
            "id": "msg_123",
        {"type": "refusal", "refusal": "I cannot assist with that."},
        {"type": "web_search_call", "id": "websearch_123", "status": "completed"},
    id="resp_123",
There are other, small improvements as well-- e.g., we store message IDs on text
content blocks, rather than on the AIMessage.id, which now stores the response ID.
For backwards compatibility, this module provides functions to convert between the
formats. The functions are used internally by ChatOpenAI.
from collections.abc import Iterable, Iterator
from langchain_core.messages import AIMessage, is_data_content_block
_FUNCTION_CALL_IDS_MAP_KEY = "__openai_function_call_ids__"
# v0.3 / Responses
def _convert_to_v03_ai_message(
    message: AIMessage, has_reasoning: bool = False
) -> AIMessage:
    """Mutate an `AIMessage` to the old-style v0.3 format."""
        new_content: list[dict | str] = []
                if block.get("type") == "reasoning":
                    # Store a reasoning item in additional_kwargs (overwriting as in
                    # v0.3)
                    _ = block.pop("index", None)
                    if has_reasoning:
                        _ = block.pop("id", None)
                        _ = block.pop("type", None)
                    message.additional_kwargs["reasoning"] = block
                elif block.get("type") in (
                    "web_search_call",
                    "file_search_call",
                    "computer_call",
                    "code_interpreter_call",
                    "mcp_call",
                    "mcp_list_tools",
                    "mcp_approval_request",
                    "image_generation_call",
                    "tool_search_call",
                    "tool_search_output",
                    # Store built-in tool calls in additional_kwargs
                    if "tool_outputs" not in message.additional_kwargs:
                        message.additional_kwargs["tool_outputs"] = []
                    message.additional_kwargs["tool_outputs"].append(block)
                elif block.get("type") == "function_call":
                    # Store function call item IDs in additional_kwargs, otherwise
                    # discard function call items.
                    if _FUNCTION_CALL_IDS_MAP_KEY not in message.additional_kwargs:
                        message.additional_kwargs[_FUNCTION_CALL_IDS_MAP_KEY] = {}
                    if (call_id := block.get("call_id")) and (
                        function_call_id := block.get("id")
                        message.additional_kwargs[_FUNCTION_CALL_IDS_MAP_KEY][
                            call_id
                        ] = function_call_id
                elif (block.get("type") == "refusal") and (
                    refusal := block.get("refusal")
                    # Store a refusal item in additional_kwargs (overwriting as in
                    message.additional_kwargs["refusal"] = refusal
                    # Store a message item ID on AIMessage.id
                        message.id = block["id"]
                    new_content.append({k: v for k, v in block.items() if k != "id"})
                    set(block.keys()) == {"id", "index"}
                    and isinstance(block["id"], str)
                    and block["id"].startswith("msg_")
                    # Drop message IDs in streaming case
                    new_content.append({"index": block["index"]})
        message.content = new_content
        if isinstance(message.id, str) and message.id.startswith("resp_"):
            message.id = None
# v1 / Chat Completions
# v1 / Responses
    """Convert a v1 `Annotation` to the v0.3 format (for Responses API)."""
        new_ann: dict[str, Any] = {}
        for field in ("end_index", "start_index"):
            if field in annotation:
                new_ann[field] = annotation[field]
            # URL citation
                new_ann["title"] = annotation["title"]
            new_ann["type"] = "url_citation"
            new_ann["url"] = annotation["url"]
            if extra_fields := annotation.get("extras"):
                new_ann.update(dict(extra_fields.items()))
            # Document citation
            new_ann["type"] = "file_citation"
                new_ann["filename"] = annotation["title"]
        return new_ann
    return dict(annotation)
def _implode_reasoning_blocks(blocks: list[dict[str, Any]]) -> Iterable[dict[str, Any]]:
    n = len(blocks)
    while i < n:
        block = blocks[i]
        # Skip non-reasoning blocks or blocks already in Responses format
        if block.get("type") != "reasoning" or "summary" in block:
            yield dict(block)
        elif "reasoning" not in block and "summary" not in block:
            # {"type": "reasoning", "id": "rs_..."}
            oai_format = {**block, "summary": []}
            if "extras" in oai_format:
                oai_format.update(oai_format.pop("extras"))
            oai_format["type"] = oai_format.pop("type", "reasoning")
            if "encrypted_content" in oai_format:
                oai_format["encrypted_content"] = oai_format.pop("encrypted_content")
            yield oai_format
        summary: list[dict[str, str]] = [
            {"type": "summary_text", "text": block.get("reasoning", "")}
        # 'common' is every field except the exploded 'reasoning'
        common = {k: v for k, v in block.items() if k != "reasoning"}
        if "extras" in common:
            common.update(common.pop("extras"))
            next_ = blocks[i]
            if next_.get("type") == "reasoning" and "reasoning" in next_:
                summary.append(
                    {"type": "summary_text", "text": next_.get("reasoning", "")}
        merged = dict(common)
        merged["summary"] = summary
        merged["type"] = merged.pop("type", "reasoning")
        yield merged
def _consolidate_calls(items: Iterable[dict[str, Any]]) -> Iterator[dict[str, Any]]:
    """Generator that walks through *items* and, whenever it meets the pair.
        {"type": "server_tool_call", "name": "web_search", "id": X, ...}
        {"type": "server_tool_result", "id": X}
    merges them into
        {"id": X,
         "output": ...,
         "status": ...,
         "type": "web_search_call"}
    keeping every other element untouched.
    items = iter(items)  # make sure we have a true iterator
    for current in items:
        # Only a call can start a pair worth collapsing
        if current.get("type") != "server_tool_call":
            yield current
            nxt = next(items)  # look-ahead one element
        except StopIteration:  # no “result” - just yield the call back
        # If this really is the matching “result” - collapse
        if nxt.get("type") == "server_tool_result" and nxt.get(
            "tool_call_id"
        ) == current.get("id"):
            if current.get("name") == "web_search":
                collapsed = {"id": current["id"]}
                if "args" in current:
                    # N.B. as of 2025-09-17 OpenAI raises BadRequestError if sources
                    # are passed back in
                    collapsed["action"] = current["args"]
                if status := nxt.get("status"):
                    if status == "success":
                        collapsed["status"] = "completed"
                    elif status == "error":
                        collapsed["status"] = "failed"
                elif nxt.get("extras", {}).get("status"):
                    collapsed["status"] = nxt["extras"]["status"]
                collapsed["type"] = "web_search_call"
            if current.get("name") == "file_search":
                if "args" in current and "queries" in current["args"]:
                    collapsed["queries"] = current["args"]["queries"]
                if "output" in nxt:
                    collapsed["results"] = nxt["output"]
                collapsed["type"] = "file_search_call"
            elif current.get("name") == "code_interpreter":
                if "args" in current and "code" in current["args"]:
                    collapsed["code"] = current["args"]["code"]
                for key in ("container_id",):
                    if key in current:
                        collapsed[key] = current[key]
                    elif key in current.get("extras", {}):
                        collapsed[key] = current["extras"][key]
                    collapsed["outputs"] = nxt["output"]
                collapsed["type"] = "code_interpreter_call"
            elif current.get("name") == "remote_mcp":
                    collapsed["arguments"] = json.dumps(
                        current["args"], separators=(",", ":")
                elif "arguments" in current.get("extras", {}):
                    collapsed["arguments"] = current["extras"]["arguments"]
                if tool_name := current.get("extras", {}).get("tool_name"):
                    collapsed["name"] = tool_name
                if server_label := current.get("extras", {}).get("server_label"):
                    collapsed["server_label"] = server_label
                collapsed["type"] = "mcp_call"
                if approval_id := current.get("extras", {}).get("approval_request_id"):
                    collapsed["approval_request_id"] = approval_id
                if error := nxt.get("extras", {}).get("error"):
                    collapsed["error"] = error
                    collapsed["output"] = nxt["output"]
                for k, v in current.get("extras", {}).items():
                    if k not in ("server_label", "arguments", "tool_name", "error"):
                        collapsed[k] = v
            elif current.get("name") == "mcp_list_tools":
                    collapsed["tools"] = nxt["output"]
                collapsed["type"] = "mcp_list_tools"
                    if k not in ("server_label", "error"):
            yield collapsed
            # Not a matching pair - emit both, in original order
            yield nxt
def _convert_from_v1_to_responses(
    content: list[types.ContentBlock], tool_calls: list[types.ToolCall]
        if block["type"] == "text" and "annotations" in block:
            # Need a copy because we're changing the annotations list
            new_block = dict(block)
            new_block["annotations"] = [
            new_block = {"type": "function_call", "call_id": block["id"]}
            if "extras" in block and "item_id" in block["extras"]:
                new_block["id"] = block["extras"]["item_id"]
            if "name" in block:
                new_block["name"] = block["name"]
            if "extras" in block and "arguments" in block["extras"]:
                new_block["arguments"] = block["extras"]["arguments"]
            if any(key not in new_block for key in ("name", "arguments")):
                matching_tool_calls = [
                    call for call in tool_calls if call["id"] == block["id"]
                if matching_tool_calls:
                    tool_call = matching_tool_calls[0]
                    if "name" not in new_block:
                        new_block["name"] = tool_call["name"]
                    if "arguments" not in new_block:
                        new_block["arguments"] = json.dumps(
                            tool_call["args"], separators=(",", ":")
            if "extras" in block:
                for extra_key in ("status", "namespace"):
                    if extra_key in block["extras"]:
                        new_block[extra_key] = block["extras"][extra_key]
        elif block["type"] == "server_tool_call" and block.get("name") == "tool_search":
            extras = block.get("extras", {})
            new_block = {"id": block["id"]}
            status = extras.get("status")
                new_block["status"] = status
            new_block["type"] = "tool_search_call"
                new_block["arguments"] = block["args"]
            execution = extras.get("execution")
            if execution:
                new_block["execution"] = execution
            block["type"] == "server_tool_result"
            and block.get("extras", {}).get("name") == "tool_search"
            new_block = {"id": block.get("tool_call_id", "")}
            status = block.get("status")
                new_block["status"] = "completed"
                new_block["status"] = "failed"
            elif status:
            new_block["type"] = "tool_search_output"
            new_block["execution"] = "server"
            output: dict = block.get("output", {})
            if isinstance(output, dict) and "tools" in output:
                new_block["tools"] = output["tools"]
            is_data_content_block(cast(dict, block))
            and block["type"] == "image"
            and "base64" in block
            and isinstance(block.get("id"), str)
            and block["id"].startswith("ig_")
            new_block = {"type": "image_generation_call", "result": block["base64"]}
            for extra_key in ("id", "status"):
                if extra_key in block:
                    new_block[extra_key] = block[extra_key]  # type: ignore[literal-required]
                elif extra_key in block.get("extras", {}):
        elif block["type"] == "non_standard" and "value" in block:
    new_content = list(_implode_reasoning_blocks(new_content))
    return list(_consolidate_calls(new_content))
if sys.version_info >= (3, 10):
    from zipfile import Path as ZipPath  # type: ignore
    from ..zipp import Path as ZipPath  # type: ignore
    from typing import runtime_checkable  # type: ignore
    def runtime_checkable(cls):  # type: ignore
    from typing import Protocol  # type: ignore
    Protocol = abc.ABC  # type: ignore
class TraversableResourcesLoader:
    Adapt loaders to provide TraversableResources and other
    Used primarily for Python 3.9 and earlier where the native
    loaders do not yet implement TraversableResources.
    def __init__(self, spec):
        self.spec = spec
        return self.spec.origin
    def get_resource_reader(self, name):
        from . import readers, _adapters
        def _zip_reader(spec):
            with suppress(AttributeError):
                return readers.ZipReader(spec.loader, spec.name)
        def _namespace_reader(spec):
            with suppress(AttributeError, ValueError):
                return readers.NamespaceReader(spec.submodule_search_locations)
        def _available_reader(spec):
                return spec.loader.get_resource_reader(spec.name)
        def _native_reader(spec):
            reader = _available_reader(spec)
            return reader if hasattr(reader, 'files') else None
        def _file_reader(spec):
                path = pathlib.Path(self.path)
            if path.exists():
                return readers.FileReader(self)
            # native reader if it supplies 'files'
            _native_reader(self.spec)
            # local ZipReader if a zip module
            _zip_reader(self.spec)
            # local NamespaceReader if a namespace module
            _namespace_reader(self.spec)
            # local FileReader
            _file_reader(self.spec)
            # fallback - adapt the spec ResourceReader to TraversableReader
            or _adapters.CompatibilityFiles(self.spec)
def wrap_spec(package):
    Construct a package spec with traversable compatibility
    on the spec/loader/reader.
    Supersedes _adapters.wrap_spec to use TraversableResourcesLoader
    from above for older Python compatibility (<3.10).
    from . import _adapters
    return _adapters.SpecLoaderAdapter(package.__spec__, TraversableResourcesLoader)
__all__ = ['install', 'NullFinder', 'Protocol']
    from typing import Protocol
    from ..typing_extensions import Protocol  # type: ignore
def install(cls):
    Class decorator for installation on sys.meta_path.
    Adds the backport DistributionFinder to sys.meta_path and
    attempts to disable the finder functionality of the stdlib
    DistributionFinder.
    sys.meta_path.append(cls())
    disable_stdlib_finder()
def disable_stdlib_finder():
    Give the backport primacy for discovering path-based distributions
    by monkey-patching the stdlib O_O.
    See #91 for more background for rationale on this sketchy
    def matches(finder):
        return getattr(
            finder, '__module__', None
        ) == '_frozen_importlib_external' and hasattr(finder, 'find_distributions')
    for finder in filter(matches, sys.meta_path):  # pragma: nocover
        del finder.find_distributions
class NullFinder:
    A "Finder" (aka "MetaClassFinder") that never finds any modules,
    but may find distributions.
    def find_spec(*args, **kwargs):
    # In Python 2, the import system requires finders
    # to have a find_module() method, but this usage
    # is deprecated in Python 3 in favor of find_spec().
    # For the purposes of this finder (i.e. being present
    # on sys.meta_path but having no other import
    # system functionality), the two methods are identical.
    find_module = find_spec
def pypy_partial(val):
    Adjust for variable stacklevel on partial under PyPy.
    Workaround for #327.
    is_pypy = platform.python_implementation() == 'PyPy'
    return val + is_pypy
from typing import Any, Protocol, cast
class BadMetadata(ValueError):
    def __init__(self, dist: importlib.metadata.Distribution, *, reason: str) -> None:
        return f"Bad metadata in {self.dist} ({self.reason})"
class BasePath(Protocol):
    """A protocol that various path objects conform.
    This exists because importlib.metadata uses both ``pathlib.Path`` and
    ``zipfile.Path``, and we need a common base for type hints (Union does not
    work well since ``zipfile.Path`` is too new for our linter setup).
    This does not mean to be exhaustive, but only contains things that present
    in both classes *that we need*.
    def parent(self) -> BasePath:
def get_info_location(d: importlib.metadata.Distribution) -> BasePath | None:
    """Find the path to the distribution's metadata directory.
    HACK: This relies on importlib.metadata's private ``_path`` attribute. Not
    all distributions exist on disk, so importlib.metadata is correct to not
    expose the attribute as public. But pip's code base is old and not as clean,
    so we do this to avoid having to rewrite too many things. Hopefully we can
    eliminate this some day.
    return getattr(d, "_path", None)
def parse_name_and_version_from_info_directory(
    dist: importlib.metadata.Distribution,
) -> tuple[str | None, str | None]:
    """Get a name and version from the metadata directory name.
    This is much faster than reading distribution metadata.
    info_location = get_info_location(dist)
    if info_location is None:
    stem, suffix = os.path.splitext(info_location.name)
    if suffix == ".dist-info":
        name, sep, version = stem.partition("-")
        if sep:
            return name, version
    if suffix == ".egg-info":
        name = stem.split("-", 1)[0]
        return name, None
def get_dist_canonical_name(dist: importlib.metadata.Distribution) -> NormalizedName:
    """Get the distribution's normalized name.
    The ``name`` attribute is only available in Python 3.10 or later. We are
    targeting exactly that, but Mypy does not know this.
    if name := parse_name_and_version_from_info_directory(dist)[0]:
        return canonicalize_name(name)
    name = cast(Any, dist).name
    if not isinstance(name, str):
        raise BadMetadata(dist, reason="invalid metadata entry 'name'")
NO_EXTENSIONS = bool(os.environ.get("MULTIDICT_NO_EXTENSIONS"))
PYPY = platform.python_implementation() == "PyPy"
USE_EXTENSIONS = not NO_EXTENSIONS and not PYPY
if USE_EXTENSIONS:
        from . import _multidict  # type: ignore[attr-defined]  # noqa: F401
        # FIXME: Refactor for coverage. See #837.
        USE_EXTENSIONS = False
from collections.abc import Mapping, Sequence  # noqa: F401
from typing import _GenericAlias
PY_3_10_PLUS = sys.version_info[:2] >= (3, 10)
PY_3_11_PLUS = sys.version_info[:2] >= (3, 11)
PY_3_12_PLUS = sys.version_info[:2] >= (3, 12)
PY_3_13_PLUS = sys.version_info[:2] >= (3, 13)
PY_3_14_PLUS = sys.version_info[:2] >= (3, 14)
if PY_3_14_PLUS:
    import annotationlib
    # We request forward-ref annotations to not break in the presence of
    # forward references.
    def _get_annotations(cls):
        return annotationlib.get_annotations(
            cls, format=annotationlib.Format.FORWARDREF
        Get annotations for *cls*.
        return cls.__dict__.get("__annotations__", {})
class _AnnotationExtractor:
    Extract type annotations from a callable, returning None whenever there
    is none.
    __slots__ = ["sig"]
    def __init__(self, callable):
            self.sig = inspect.signature(callable)
        except (ValueError, TypeError):  # inspect failed
            self.sig = None
    def get_first_param_type(self):
        Return the type annotation of the first argument if it's not empty.
        if not self.sig:
        params = list(self.sig.parameters.values())
        if params and params[0].annotation is not inspect.Parameter.empty:
            return params[0].annotation
    def get_return_type(self):
        Return the return type if it's not empty.
            self.sig
            and self.sig.return_annotation is not inspect.Signature.empty
            return self.sig.return_annotation
# Thread-local global to track attrs instances which are already being repr'd.
# This is needed because there is no other (thread-safe) way to pass info
# about the instances that are already being repr'd through the call stack
# in order to ensure we don't perform infinite recursion.
# For instance, if an instance contains a dict which contains that instance,
# we need to know that we're already repr'ing the outside instance from within
# the dict's repr() call.
# This lives here rather than in _make.py so that the functions in _make.py
# don't have a direct reference to the thread-local in their globals dict.
# If they have such a reference, it breaks cloudpickle.
repr_context = threading.local()
def get_generic_base(cl):
    """If this is a generic class (A[str]), return the generic base for it."""
    if cl.__class__ is _GenericAlias:
        return cl.__origin__
__all__ = ['install', 'NullFinder']
    A "Finder" (aka "MetaPathFinder") that never finds any modules,
import tokenize
if sys.version_info >= (3, 12):  # pragma: >=3.12 cover
    FSTRING_START = tokenize.FSTRING_START
    FSTRING_MIDDLE = tokenize.FSTRING_MIDDLE
    FSTRING_END = tokenize.FSTRING_END
else:  # pragma: <3.12 cover
    FSTRING_START = FSTRING_MIDDLE = FSTRING_END = -1
if sys.version_info >= (3, 14):  # pragma: >=3.14 cover
    TSTRING_START = tokenize.TSTRING_START
    TSTRING_MIDDLE = tokenize.TSTRING_MIDDLE
    TSTRING_END = tokenize.TSTRING_END
else:  # pragma: <3.14 cover
    TSTRING_START = TSTRING_MIDDLE = TSTRING_END = -1
from weakref import WeakKeyDictionary
CYGWIN = sys.platform.startswith("cygwin")
WIN = sys.platform.startswith("win")
auto_wrap_for_ansi: t.Callable[[t.TextIO], t.TextIO] | None = None
_ansi_re = re.compile(r"\033\[[;?0-9]*[a-zA-Z]")
def _make_text_stream(
    stream: t.BinaryIO,
    errors: str | None,
    force_readable: bool = False,
    force_writable: bool = False,
) -> t.TextIO:
        encoding = get_best_encoding(stream)
    if errors is None:
        errors = "replace"
    return _NonClosingTextIOWrapper(
        line_buffering=True,
        force_readable=force_readable,
        force_writable=force_writable,
def is_ascii_encoding(encoding: str) -> bool:
    """Checks if a given encoding is ascii."""
        return codecs.lookup(encoding).name == "ascii"
def get_best_encoding(stream: t.IO[t.Any]) -> str:
    """Returns the default stream encoding if not found."""
    rv = getattr(stream, "encoding", None) or sys.getdefaultencoding()
    if is_ascii_encoding(rv):
class _NonClosingTextIOWrapper(io.TextIOWrapper):
        **extra: t.Any,
        self._stream = stream = t.cast(
            t.BinaryIO, _FixupStream(stream, force_readable, force_writable)
        super().__init__(stream, encoding, errors, **extra)
    def isatty(self) -> bool:
        # https://bitbucket.org/pypy/pypy/issue/1803
        return self._stream.isatty()
class _FixupStream:
    """The new io interface needs more from streams than streams
    traditionally implement.  As such, this fix-up code is necessary in
    some circumstances.
    The forcing of readable and writable flags are there because some tools
    put badly patched objects on sys (one such offender are certain version
    of jupyter notebook).
        self._force_readable = force_readable
        self._force_writable = force_writable
    def __getattr__(self, name: str) -> t.Any:
        return getattr(self._stream, name)
    def read1(self, size: int) -> bytes:
        f = getattr(self._stream, "read1", None)
            return t.cast(bytes, f(size))
        return self._stream.read(size)
        if self._force_readable:
        x = getattr(self._stream, "readable", None)
        if x is not None:
            return t.cast(bool, x())
            self._stream.read(0)
    def writable(self) -> bool:
        if self._force_writable:
        x = getattr(self._stream, "writable", None)
            self._stream.write(b"")
    def seekable(self) -> bool:
        x = getattr(self._stream, "seekable", None)
            self._stream.seek(self._stream.tell())
def _is_binary_reader(stream: t.IO[t.Any], default: bool = False) -> bool:
        return isinstance(stream.read(0), bytes)
        # This happens in some cases where the stream was already
        # closed.  In this case, we assume the default.
def _is_binary_writer(stream: t.IO[t.Any], default: bool = False) -> bool:
        stream.write(b"")
            stream.write("")
def _find_binary_reader(stream: t.IO[t.Any]) -> t.BinaryIO | None:
    # We need to figure out if the given stream is already binary.
    # This can happen because the official docs recommend detaching
    # the streams to get binary streams.  Some code might do this, so
    # we need to deal with this case explicitly.
    if _is_binary_reader(stream, False):
        return t.cast(t.BinaryIO, stream)
    buf = getattr(stream, "buffer", None)
    # Same situation here; this time we assume that the buffer is
    # actually binary in case it's closed.
    if buf is not None and _is_binary_reader(buf, True):
        return t.cast(t.BinaryIO, buf)
def _find_binary_writer(stream: t.IO[t.Any]) -> t.BinaryIO | None:
    if _is_binary_writer(stream, False):
    if buf is not None and _is_binary_writer(buf, True):
def _stream_is_misconfigured(stream: t.TextIO) -> bool:
    """A stream is misconfigured if its encoding is ASCII."""
    # If the stream does not have an encoding set, we assume it's set
    # to ASCII.  This appears to happen in certain unittest
    # environments.  It's not quite clear what the correct behavior is
    # but this at least will force Click to recover somehow.
    return is_ascii_encoding(getattr(stream, "encoding", None) or "ascii")
def _is_compat_stream_attr(stream: t.TextIO, attr: str, value: str | None) -> bool:
    """A stream attribute is compatible if it is equal to the
    desired value or the desired value is unset and the attribute
    has a value.
    stream_value = getattr(stream, attr, None)
    return stream_value == value or (value is None and stream_value is not None)
def _is_compatible_text_stream(
    stream: t.TextIO, encoding: str | None, errors: str | None
    """Check if a stream's encoding and errors attributes are
    compatible with the desired values.
    return _is_compat_stream_attr(
        stream, "encoding", encoding
    ) and _is_compat_stream_attr(stream, "errors", errors)
def _force_correct_text_stream(
    text_stream: t.IO[t.Any],
    is_binary: t.Callable[[t.IO[t.Any], bool], bool],
    find_binary: t.Callable[[t.IO[t.Any]], t.BinaryIO | None],
    if is_binary(text_stream, False):
        binary_reader = t.cast(t.BinaryIO, text_stream)
        text_stream = t.cast(t.TextIO, text_stream)
        # If the stream looks compatible, and won't default to a
        # misconfigured ascii encoding, return it as-is.
        if _is_compatible_text_stream(text_stream, encoding, errors) and not (
            encoding is None and _stream_is_misconfigured(text_stream)
            return text_stream
        # Otherwise, get the underlying binary reader.
        possible_binary_reader = find_binary(text_stream)
        # If that's not possible, silently use the original reader
        # and get mojibake instead of exceptions.
        if possible_binary_reader is None:
        binary_reader = possible_binary_reader
    # Default errors to replace instead of strict in order to get
    # something that works.
    # Wrap the binary stream in a text stream with the correct
    # encoding parameters.
    return _make_text_stream(
        binary_reader,
def _force_correct_text_reader(
    text_reader: t.IO[t.Any],
    return _force_correct_text_stream(
        text_reader,
        _is_binary_reader,
        _find_binary_reader,
def _force_correct_text_writer(
    text_writer: t.IO[t.Any],
        text_writer,
        _is_binary_writer,
        _find_binary_writer,
def get_binary_stdin() -> t.BinaryIO:
    reader = _find_binary_reader(sys.stdin)
        raise RuntimeError("Was not able to determine binary stream for sys.stdin.")
    return reader
def get_binary_stdout() -> t.BinaryIO:
    writer = _find_binary_writer(sys.stdout)
    if writer is None:
        raise RuntimeError("Was not able to determine binary stream for sys.stdout.")
    return writer
def get_binary_stderr() -> t.BinaryIO:
    writer = _find_binary_writer(sys.stderr)
        raise RuntimeError("Was not able to determine binary stream for sys.stderr.")
def get_text_stdin(encoding: str | None = None, errors: str | None = None) -> t.TextIO:
    rv = _get_windows_console_stream(sys.stdin, encoding, errors)
    if rv is not None:
    return _force_correct_text_reader(sys.stdin, encoding, errors, force_readable=True)
def get_text_stdout(encoding: str | None = None, errors: str | None = None) -> t.TextIO:
    rv = _get_windows_console_stream(sys.stdout, encoding, errors)
    return _force_correct_text_writer(sys.stdout, encoding, errors, force_writable=True)
def get_text_stderr(encoding: str | None = None, errors: str | None = None) -> t.TextIO:
    rv = _get_windows_console_stream(sys.stderr, encoding, errors)
    return _force_correct_text_writer(sys.stderr, encoding, errors, force_writable=True)
def _wrap_io_open(
    file: str | os.PathLike[str] | int,
) -> t.IO[t.Any]:
    """Handles not passing ``encoding`` and ``errors`` in binary mode."""
        return open(file, mode)
    return open(file, mode, encoding=encoding, errors=errors)
def open_stream(
    filename: str | os.PathLike[str],
    errors: str | None = "strict",
    atomic: bool = False,
) -> tuple[t.IO[t.Any], bool]:
    binary = "b" in mode
    filename = os.fspath(filename)
    # Standard streams first. These are simple because they ignore the
    # atomic flag. Use fsdecode to handle Path("-").
    if os.fsdecode(filename) == "-":
        if any(m in mode for m in ["w", "a", "x"]):
            if binary:
                return get_binary_stdout(), False
            return get_text_stdout(encoding=encoding, errors=errors), False
            return get_binary_stdin(), False
        return get_text_stdin(encoding=encoding, errors=errors), False
    # Non-atomic writes directly go out through the regular open functions.
    if not atomic:
        return _wrap_io_open(filename, mode, encoding, errors), True
    # Some usability stuff for atomic writes
    if "a" in mode:
            "Appending to an existing file is not supported, because that"
            " would involve an expensive `copy`-operation to a temporary"
            " file. Open the file in normal `w`-mode and copy explicitly"
            " if that's what you're after."
    if "x" in mode:
        raise ValueError("Use the `overwrite`-parameter instead.")
    if "w" not in mode:
        raise ValueError("Atomic writes only make sense with `w`-mode.")
    # Atomic writes are more complicated.  They work by opening a file
    # as a proxy in the same folder and then using the fdopen
    # functionality to wrap it in a Python file.  Then we wrap it in an
    # atomic file that moves the file over on close.
        perm: int | None = os.stat(filename).st_mode
        perm = None
    flags = os.O_RDWR | os.O_CREAT | os.O_EXCL
        flags |= getattr(os, "O_BINARY", 0)
        tmp_filename = os.path.join(
            os.path.dirname(filename),
            f".__atomic-write{random.randrange(1 << 32):08x}",
            fd = os.open(tmp_filename, flags, 0o666 if perm is None else perm)
            if e.errno == errno.EEXIST or (
                os.name == "nt"
                and e.errno == errno.EACCES
                and os.path.isdir(e.filename)
                and os.access(e.filename, os.W_OK)
    if perm is not None:
        os.chmod(tmp_filename, perm)  # in case perm includes bits in umask
    f = _wrap_io_open(fd, mode, encoding, errors)
    af = _AtomicFile(f, tmp_filename, os.path.realpath(filename))
    return t.cast(t.IO[t.Any], af), True
class _AtomicFile:
    def __init__(self, f: t.IO[t.Any], tmp_filename: str, real_filename: str) -> None:
        self._f = f
        self._tmp_filename = tmp_filename
        self._real_filename = real_filename
        return self._real_filename
    def close(self, delete: bool = False) -> None:
        self._f.close()
        os.replace(self._tmp_filename, self._real_filename)
        return getattr(self._f, name)
    def __enter__(self) -> _AtomicFile:
        exc_value: BaseException | None,
        tb: TracebackType | None,
        self.close(delete=exc_type is not None)
        return repr(self._f)
def strip_ansi(value: str) -> str:
    return _ansi_re.sub("", value)
def _is_jupyter_kernel_output(stream: t.IO[t.Any]) -> bool:
    while isinstance(stream, (_FixupStream, _NonClosingTextIOWrapper)):
        stream = stream._stream
    return stream.__class__.__module__.startswith("ipykernel.")
def should_strip_ansi(
    stream: t.IO[t.Any] | None = None, color: bool | None = None
            stream = sys.stdin
        return not isatty(stream) and not _is_jupyter_kernel_output(stream)
    return not color
# On Windows, wrap the output streams with colorama to support ANSI
# color codes.
# NOTE: double check is needed so mypy does not analyze this on Linux
if sys.platform.startswith("win") and WIN:
    from ._winconsole import _get_windows_console_stream
    def _get_argv_encoding() -> str:
    _ansi_stream_wrappers: cabc.MutableMapping[t.TextIO, t.TextIO] = WeakKeyDictionary()
    def auto_wrap_for_ansi(stream: t.TextIO, color: bool | None = None) -> t.TextIO:
        """Support ANSI color and style codes on Windows by wrapping a
        stream with colorama.
            cached = _ansi_stream_wrappers.get(stream)
            cached = None
        strip = should_strip_ansi(stream, color)
        ansi_wrapper = colorama.AnsiToWin32(stream, strip=strip)
        rv = t.cast(t.TextIO, ansi_wrapper.stream)
        _write = rv.write
        def _safe_write(s: str) -> int:
                return _write(s)
                ansi_wrapper.reset_all()
        rv.write = _safe_write  # type: ignore[method-assign]
            _ansi_stream_wrappers[stream] = rv
        return getattr(sys.stdin, "encoding", None) or sys.getfilesystemencoding()
    def _get_windows_console_stream(
        f: t.TextIO, encoding: str | None, errors: str | None
    ) -> t.TextIO | None:
def term_len(x: str) -> int:
    return len(strip_ansi(x))
def isatty(stream: t.IO[t.Any]) -> bool:
def _make_cached_stream_func(
    src_func: t.Callable[[], t.TextIO | None],
    wrapper_func: t.Callable[[], t.TextIO],
) -> t.Callable[[], t.TextIO | None]:
    cache: cabc.MutableMapping[t.TextIO, t.TextIO] = WeakKeyDictionary()
    def func() -> t.TextIO | None:
        stream = src_func()
            rv = cache.get(stream)
            rv = None
        rv = wrapper_func()
            cache[stream] = rv
_default_text_stdin = _make_cached_stream_func(lambda: sys.stdin, get_text_stdin)
_default_text_stdout = _make_cached_stream_func(lambda: sys.stdout, get_text_stdout)
_default_text_stderr = _make_cached_stream_func(lambda: sys.stderr, get_text_stderr)
binary_streams: cabc.Mapping[str, t.Callable[[], t.BinaryIO]] = {
    "stdin": get_binary_stdin,
    "stdout": get_binary_stdout,
    "stderr": get_binary_stderr,
text_streams: cabc.Mapping[str, t.Callable[[str | None, str | None], t.TextIO]] = {
    "stdin": get_text_stdin,
    "stdout": get_text_stdout,
    "stderr": get_text_stderr,
from typing_extensions import Self, Literal
        return tp is Union or tp is types.UnionType
