from openai.types.beta.threads import (
    MessageDeleted,
class TestMessages:
            message = client.beta.threads.messages.create(
                content="string",
                role="user",
        assert_matches_type(Message, message, path=["response"])
                attachments=[
            response = client.beta.threads.messages.with_raw_response.create(
        message = response.parse()
            with client.beta.threads.messages.with_streaming_response.create(
    def test_path_params_create(self, client: OpenAI) -> None:
                client.beta.threads.messages.with_raw_response.create(
            message = client.beta.threads.messages.retrieve(
                message_id="message_id",
            response = client.beta.threads.messages.with_raw_response.retrieve(
            with client.beta.threads.messages.with_streaming_response.retrieve(
                client.beta.threads.messages.with_raw_response.retrieve(
            with pytest.raises(ValueError, match=r"Expected a non-empty value for `message_id` but received ''"):
                    message_id="",
            message = client.beta.threads.messages.update(
            response = client.beta.threads.messages.with_raw_response.update(
            with client.beta.threads.messages.with_streaming_response.update(
                client.beta.threads.messages.with_raw_response.update(
            message = client.beta.threads.messages.list(
        assert_matches_type(SyncCursorPage[Message], message, path=["response"])
                run_id="run_id",
            response = client.beta.threads.messages.with_raw_response.list(
            with client.beta.threads.messages.with_streaming_response.list(
    def test_path_params_list(self, client: OpenAI) -> None:
                client.beta.threads.messages.with_raw_response.list(
            message = client.beta.threads.messages.delete(
        assert_matches_type(MessageDeleted, message, path=["response"])
            response = client.beta.threads.messages.with_raw_response.delete(
            with client.beta.threads.messages.with_streaming_response.delete(
                client.beta.threads.messages.with_raw_response.delete(
class TestAsyncMessages:
            message = await async_client.beta.threads.messages.create(
            response = await async_client.beta.threads.messages.with_raw_response.create(
            async with async_client.beta.threads.messages.with_streaming_response.create(
                message = await response.parse()
    async def test_path_params_create(self, async_client: AsyncOpenAI) -> None:
                await async_client.beta.threads.messages.with_raw_response.create(
            message = await async_client.beta.threads.messages.retrieve(
            response = await async_client.beta.threads.messages.with_raw_response.retrieve(
            async with async_client.beta.threads.messages.with_streaming_response.retrieve(
                await async_client.beta.threads.messages.with_raw_response.retrieve(
            message = await async_client.beta.threads.messages.update(
            response = await async_client.beta.threads.messages.with_raw_response.update(
            async with async_client.beta.threads.messages.with_streaming_response.update(
                await async_client.beta.threads.messages.with_raw_response.update(
            message = await async_client.beta.threads.messages.list(
        assert_matches_type(AsyncCursorPage[Message], message, path=["response"])
            response = await async_client.beta.threads.messages.with_raw_response.list(
            async with async_client.beta.threads.messages.with_streaming_response.list(
    async def test_path_params_list(self, async_client: AsyncOpenAI) -> None:
                await async_client.beta.threads.messages.with_raw_response.list(
            message = await async_client.beta.threads.messages.delete(
            response = await async_client.beta.threads.messages.with_raw_response.delete(
            async with async_client.beta.threads.messages.with_streaming_response.delete(
                await async_client.beta.threads.messages.with_raw_response.delete(
from openai.types.chat import ChatCompletionStoreMessage
        message = client.chat.completions.messages.list(
        assert_matches_type(SyncCursorPage[ChatCompletionStoreMessage], message, path=["response"])
        response = client.chat.completions.messages.with_raw_response.list(
        with client.chat.completions.messages.with_streaming_response.list(
            client.chat.completions.messages.with_raw_response.list(
        message = await async_client.chat.completions.messages.list(
        assert_matches_type(AsyncCursorPage[ChatCompletionStoreMessage], message, path=["response"])
        response = await async_client.chat.completions.messages.with_raw_response.list(
        async with async_client.chat.completions.messages.with_streaming_response.list(
            await async_client.chat.completions.messages.with_raw_response.list(
from langchain_core.load import dumpd, load
    ChatMessageChunk,
    FunctionMessage,
    FunctionMessageChunk,
    HumanMessageChunk,
    RemoveMessage,
from langchain_core.messages.content import KNOWN_BLOCK_TYPES, ContentBlock
from langchain_core.messages.tool import invalid_tool_call as create_invalid_tool_call
from langchain_core.messages.tool import tool_call as create_tool_call
from langchain_core.messages.tool import tool_call_chunk as create_tool_call_chunk
from langchain_core.utils._merge import merge_lists
def test_message_init() -> None:
    for doc in [
        BaseMessage(type="foo", content="bar"),
        BaseMessage(type="foo", content="bar", id=None),
        BaseMessage(type="foo", content="bar", id="1"),
        BaseMessage(type="foo", content="bar", id=1),
        assert isinstance(doc, BaseMessage)
def test_message_chunks() -> None:
    assert AIMessageChunk(content="I am", id="ai3") + AIMessageChunk(
        content=" indeed."
    ) == AIMessageChunk(content="I am indeed.", id="ai3"), (
        "MessageChunk + MessageChunk should be a MessageChunk"
    assert AIMessageChunk(content="I am", id="ai2") + HumanMessageChunk(
        content=" indeed.", id="human1"
    ) == AIMessageChunk(content="I am indeed.", id="ai2"), (
        "MessageChunk + MessageChunk should be a MessageChunk "
        "of same class as the left side"
    assert AIMessageChunk(
        content="", additional_kwargs={"foo": "bar"}
    ) + AIMessageChunk(content="", additional_kwargs={"baz": "foo"}) == AIMessageChunk(
        content="", additional_kwargs={"foo": "bar", "baz": "foo"}
        "with merged additional_kwargs"
        content="", additional_kwargs={"function_call": {"name": "web_search"}}
    ) + AIMessageChunk(
        content="", additional_kwargs={"function_call": {"arguments": None}}
        content="", additional_kwargs={"function_call": {"arguments": "{\n"}}
        content="",
        additional_kwargs={"function_call": {"arguments": '  "query": "turtles"\n}'}},
    ) == AIMessageChunk(
        additional_kwargs={
            "function_call": {
                "name": "web_search",
                "arguments": '{\n  "query": "turtles"\n}',
    # Test tool calls
        AIMessageChunk(
            tool_call_chunks=[
                create_tool_call_chunk(name="tool1", args="", id="1", index=0)
        + AIMessageChunk(
                create_tool_call_chunk(
                    name=None, args='{"arg1": "val', id=None, index=0
                create_tool_call_chunk(name=None, args='ue}"', id=None, index=0)
                name="tool1", args='{"arg1": "value}"', id="1", index=0
                create_tool_call_chunk(name=None, args='{"arg1": "val', id="", index=0)
                create_tool_call_chunk(name=None, args='ue"}', id="", index=0)
                name="tool1", args='{"arg1": "value"}', id="1", index=0
                create_tool_call_chunk(name="tool1", args="a", id=None, index=1)
        # Don't merge if `index` field does not match.
            create_tool_call_chunk(name="tool1", args="", id="1", index=0),
            create_tool_call_chunk(name="tool1", args="a", id=None, index=1),
    ai_msg_chunk = AIMessageChunk(content="")
    tool_calls_msg_chunk = AIMessageChunk(
    assert ai_msg_chunk + tool_calls_msg_chunk == tool_calls_msg_chunk
    assert tool_calls_msg_chunk + ai_msg_chunk == tool_calls_msg_chunk
    ai_msg_chunk = AIMessageChunk(
    assert ai_msg_chunk.tool_calls == [create_tool_call(name="tool1", args={}, id="1")]
    # Test token usage
    left = AIMessageChunk(
        usage_metadata={"input_tokens": 1, "output_tokens": 2, "total_tokens": 3},
    right = AIMessageChunk(
        usage_metadata={"input_tokens": 4, "output_tokens": 5, "total_tokens": 9},
    assert left + right == AIMessageChunk(
        usage_metadata={"input_tokens": 5, "output_tokens": 7, "total_tokens": 12},
    assert AIMessageChunk(content="") + left == left
    assert right + AIMessageChunk(content="") == right
    default_id = "lc_run--abc123"
    meaningful_id = "msg_def456"
    # Test ID order of precedence
    null_id_chunk = AIMessageChunk(content="", id=None)
    default_id_chunk = AIMessageChunk(
        content="", id=default_id
    )  # LangChain-assigned run ID
    provider_chunk = AIMessageChunk(
        content="", id=meaningful_id
    )  # provided ID (either by user or provider)
    assert (null_id_chunk + default_id_chunk).id == default_id
    assert (null_id_chunk + provider_chunk).id == meaningful_id
    # Provider assigned IDs have highest precedence
    assert (default_id_chunk + provider_chunk).id == meaningful_id
def test_chat_message_chunks() -> None:
    assert ChatMessageChunk(role="User", content="I am", id="ai4") + ChatMessageChunk(
        role="User", content=" indeed."
    ) == ChatMessageChunk(id="ai4", role="User", content="I am indeed."), (
        "ChatMessageChunk + ChatMessageChunk should be a ChatMessageChunk"
        ValueError, match="Cannot concatenate ChatMessageChunks with different roles"
        ChatMessageChunk(role="User", content="I am") + ChatMessageChunk(
            role="Assistant", content=" indeed."
    assert ChatMessageChunk(role="User", content="I am") + AIMessageChunk(
    ) == ChatMessageChunk(role="User", content="I am indeed."), (
        "ChatMessageChunk + other MessageChunk should be a ChatMessageChunk "
        "with the left side's role"
    assert AIMessageChunk(content="I am") + ChatMessageChunk(
    ) == AIMessageChunk(content="I am indeed."), (
        "Other MessageChunk + ChatMessageChunk should be a MessageChunk "
        "as the left side"
def test_complex_ai_message_chunks() -> None:
    assert AIMessageChunk(content=["I am"], id="ai4") + AIMessageChunk(
        content=[" indeed."]
    ) == AIMessageChunk(id="ai4", content=["I am", " indeed."]), (
        "Content concatenation with arrays of strings should naively combine"
    assert AIMessageChunk(content=[{"index": 0, "text": "I am"}]) + AIMessageChunk(
    ) == AIMessageChunk(content=[{"index": 0, "text": "I am"}, " indeed."]), (
        "Concatenating mixed content arrays should naively combine them"
        content=[{"index": 0, "text": " indeed."}]
    ) == AIMessageChunk(content=[{"index": 0, "text": "I am indeed."}]), (
        "Concatenating when both content arrays are dicts with the same index "
        "should merge"
        content=[{"text": " indeed."}]
    ) == AIMessageChunk(content=[{"index": 0, "text": "I am"}, {"text": " indeed."}]), (
        "Concatenating when one chunk is missing an index should not merge or throw"
        content=[{"index": 2, "text": " indeed."}]
        content=[{"index": 0, "text": "I am"}, {"index": 2, "text": " indeed."}]
        "Concatenating when both content arrays are dicts with a gap between indexes "
        "should not result in a holey array"
        content=[{"index": 1, "text": " indeed."}]
        content=[{"index": 0, "text": "I am"}, {"index": 1, "text": " indeed."}]
        "Concatenating when both content arrays are dicts with separate indexes "
        "should not merge"
        content=[{"index": 0, "text": "I am", "type": "text_block"}]
        content=[{"index": 0, "text": " indeed.", "type": "text_block"}]
        content=[{"index": 0, "text": "I am indeed.", "type": "text_block"}]
        "Concatenating when both content arrays are dicts with the same index and type "
        content=[{"index": 0, "text": " indeed.", "type": "text_block_delta"}]
        "and different types should merge without updating type"
        content="", response_metadata={"extra": "value"}
        content=[{"index": 0, "text": "I am", "type": "text_block"}],
        response_metadata={"extra": "value"},
        "Concatenating when one content is an array and one is an empty string "
        "should not add a new item, but should concat other fields"
def test_function_message_chunks() -> None:
    assert FunctionMessageChunk(
        name="hello", content="I am", id="ai5"
    ) + FunctionMessageChunk(name="hello", content=" indeed.") == FunctionMessageChunk(
        id="ai5", name="hello", content="I am indeed."
    ), "FunctionMessageChunk + FunctionMessageChunk should be a FunctionMessageChunk"
        match="Cannot concatenate FunctionMessageChunks with different names",
        FunctionMessageChunk(name="hello", content="I am") + FunctionMessageChunk(
            name="bye", content=" indeed."
def test_ai_message_chunks() -> None:
    assert AIMessageChunk(content="I am") + AIMessageChunk(
        "AIMessageChunk + AIMessageChunk should be a AIMessageChunk"
class TestGetBufferString:
    _HUMAN_MSG = HumanMessage(content="human")
    _AI_MSG = AIMessage(content="ai")
    def test_empty_input(self) -> None:
        assert not get_buffer_string([])
    def test_valid_single_message(self) -> None:
        expected_output = "Human: human"
        assert get_buffer_string([self._HUMAN_MSG]) == expected_output
    def test_custom_human_prefix(self) -> None:
        expected_output = "H: human"
        assert get_buffer_string([self._HUMAN_MSG], human_prefix="H") == expected_output
    def test_custom_ai_prefix(self) -> None:
        expected_output = "A: ai"
        assert get_buffer_string([self._AI_MSG], ai_prefix="A") == expected_output
    def test_multiple_msg(self) -> None:
        msgs = [
            self._HUMAN_MSG,
            self._AI_MSG,
            SystemMessage(content="system"),
            FunctionMessage(name="func", content="function"),
            ToolMessage(tool_call_id="tool_id", content="tool"),
            ChatMessage(role="Chat", content="chat"),
            AIMessage(content="tool"),
        expected_output = (
            "Human: human\n"
            "AI: ai\n"
            "System: system\n"
            "Function: function\n"
            "Tool: tool\n"
            "Chat: chat\n"
            "AI: tool"
        assert get_buffer_string(msgs) == expected_output
    def test_custom_message_separator(self) -> None:
        expected_output = "Human: human\n\nAI: ai"
        assert get_buffer_string(msgs, message_separator="\n\n") == expected_output
def test_multiple_msg() -> None:
    human_msg = HumanMessage(content="human", additional_kwargs={"key": "value"})
    ai_msg = AIMessage(content="ai")
    sys_msg = SystemMessage(content="sys")
        human_msg,
        ai_msg,
        sys_msg,
    assert messages_from_dict(messages_to_dict(msgs)) == msgs
    # Test with tool calls
            tool_calls=[create_tool_call(name="a", args={"b": 1}, id=None)],
            tool_calls=[create_tool_call(name="c", args={"c": 2}, id=None)],
def test_multiple_msg_with_name() -> None:
    human_msg = HumanMessage(
        content="human", additional_kwargs={"key": "value"}, name="human erick"
    ai_msg = AIMessage(content="ai", name="ai erick")
    sys_msg = SystemMessage(content="sys", name="sys erick")
def test_message_chunk_to_message() -> None:
    assert message_chunk_to_message(
        AIMessageChunk(content="I am", additional_kwargs={"foo": "bar"})
    ) == AIMessage(content="I am", additional_kwargs={"foo": "bar"})
    assert message_chunk_to_message(HumanMessageChunk(content="I am")) == HumanMessage(
        content="I am"
        ChatMessageChunk(role="User", content="I am")
    ) == ChatMessage(role="User", content="I am")
        FunctionMessageChunk(name="hello", content="I am")
    ) == FunctionMessage(name="hello", content="I am")
    chunk = AIMessageChunk(
        content="I am",
            create_tool_call_chunk(name="tool1", args='{"a": 1}', id="1", index=0),
            create_tool_call_chunk(name="tool2", args='{"b": ', id="2", index=0),
            create_tool_call_chunk(name="tool3", args=None, id="3", index=0),
            create_tool_call_chunk(name="tool4", args="abc", id="4", index=0),
    expected = AIMessage(
            create_tool_call(name="tool1", args={"a": 1}, id="1"),
            create_tool_call(name="tool2", args={}, id="2"),
            create_tool_call(name="tool3", args={}, id="3"),
        invalid_tool_calls=[
            create_invalid_tool_call(name="tool4", args="abc", id="4", error=None),
    assert message_chunk_to_message(chunk) == expected
    assert AIMessage(**expected.model_dump()) == expected
    assert AIMessageChunk(**chunk.model_dump()) == chunk
def test_tool_calls_merge() -> None:
    chunks: list[dict] = [
        {"content": ""},
            "content": "",
            "additional_kwargs": {
                "tool_calls": [
                        "index": 0,
                        "id": "call_CwGAsESnXehQEjiAIWzinlva",
                        "function": {"arguments": "", "name": "person"},
                        "id": None,
                        "function": {"arguments": '{"na', "name": None},
                        "type": None,
                        "function": {"arguments": 'me": ', "name": None},
                        "function": {"arguments": '"jane"', "name": None},
                        "function": {"arguments": ', "a', "name": None},
                        "function": {"arguments": 'ge": ', "name": None},
                        "function": {"arguments": "2}", "name": None},
                        "index": 1,
                        "id": "call_zXSIylHvc5x3JUAPcHZR5GZI",
                        "function": {"arguments": '"bob",', "name": None},
                        "function": {"arguments": ' "ag', "name": None},
                        "function": {"arguments": 'e": 3', "name": None},
                        "function": {"arguments": "}", "name": None},
    final: BaseMessageChunk | None = None
    for chunk in chunks:
        msg = AIMessageChunk(**chunk)
        final = msg if final is None else final + msg
    assert final == AIMessageChunk(
                        "arguments": '{"name": "jane", "age": 2}',
                        "name": "person",
                        "arguments": '{"name": "bob", "age": 3}',
                "args": '{"name": "jane", "age": 2}',
                "type": "tool_call_chunk",
                "args": '{"name": "bob", "age": 3}',
                "args": {"name": "jane", "age": 2},
                "args": {"name": "bob", "age": 3},
def test_convert_to_messages() -> None:
    # dicts
    actual = convert_to_messages(
            {"role": "system", "content": "You are a helpful assistant."},
            {"role": "user", "content": "Hello!"},
            {"role": "ai", "content": "Hi!", "id": "ai1"},
            {"type": "human", "content": "Hello!", "name": "Jane", "id": "human1"},
                "role": "assistant",
                "content": "Hi!",
                "name": "JaneBot",
                "function_call": {"name": "greet", "arguments": '{"name": "Jane"}'},
            {"role": "function", "name": "greet", "content": "Hi!"},
                        "name": "greet",
                        "args": {"name": "Jane"},
                        "id": "tool_id",
            {"role": "tool", "tool_call_id": "tool_id", "content": "Hi!"},
                "role": "tool",
                "tool_call_id": "tool_id2",
                "content": "Bye!",
                "artifact": {"foo": 123},
                "status": "success",
            {"role": "remove", "id": "message_to_remove", "content": ""},
                "content": "Now the turn for Larry to ask a question about the book!",
                "additional_kwargs": {"metadata": {"speaker_name": "Presenter"}},
                "response_metadata": {},
                "type": "human",
                "name": None,
        SystemMessage(content="You are a helpful assistant."),
        HumanMessage(content="Hello!"),
        AIMessage(content="Hi!", id="ai1"),
        HumanMessage(content="Hello!", name="Jane", id="human1"),
            content="Hi!",
            name="JaneBot",
                "function_call": {"name": "greet", "arguments": '{"name": "Jane"}'}
        FunctionMessage(name="greet", content="Hi!"),
                create_tool_call(name="greet", args={"name": "Jane"}, id="tool_id")
        ToolMessage(tool_call_id="tool_id", content="Hi!"),
            tool_call_id="tool_id2",
            content="Bye!",
            artifact={"foo": 123},
            status="success",
        RemoveMessage(id="message_to_remove"),
            content="Now the turn for Larry to ask a question about the book!",
            additional_kwargs={"metadata": {"speaker_name": "Presenter"}},
            response_metadata={},
            id="1",
    assert expected == actual
    assert convert_to_messages(
            "hello!",
            ("ai", "Hi!"),
            ["assistant", "Hi!"],
    ) == [
        HumanMessage(content="hello!"),
        AIMessage(content="Hi!"),
    "message_class",
def test_message_name(message_class: type) -> None:
    msg = message_class(content="foo", name="bar")
    assert msg.name == "bar"
    msg2 = message_class(content="foo", name=None)
    assert msg2.name is None
    msg3 = message_class(content="foo")
    assert msg3.name is None
    [FunctionMessage, FunctionMessageChunk],
def test_message_name_function(message_class: type) -> None:
    # functionmessage doesn't support name=None
    msg = message_class(name="foo", content="bar")
    assert msg.name == "foo"
    [ChatMessage, ChatMessageChunk],
def test_message_name_chat(message_class: type) -> None:
    msg = message_class(content="foo", role="user", name="bar")
    msg2 = message_class(content="foo", role="user", name=None)
    msg3 = message_class(content="foo", role="user")
def test_merge_tool_calls() -> None:
    tool_call_1 = create_tool_call_chunk(name="tool1", args="", id="1", index=0)
    tool_call_2 = create_tool_call_chunk(
    tool_call_3 = create_tool_call_chunk(name=None, args='ue}"', id=None, index=0)
    merged = merge_lists([tool_call_1], [tool_call_2])
    assert merged is not None
    assert merged == [
            "name": "tool1",
            "args": '{"arg1": "val',
    merged = merge_lists(merged, [tool_call_3])
            "args": '{"arg1": "value}"',
    left = create_tool_call_chunk(
        name="tool1", args='{"arg1": "value1"}', id="1", index=None
    right = create_tool_call_chunk(
        name="tool2", args='{"arg2": "value2"}', id="1", index=None
    merged = merge_lists([left], [right])
    assert len(merged) == 2
        name="tool1", args='{"arg1": "value1"}', id=None, index=None
        name="tool1", args='{"arg2": "value2"}', id=None, index=None
        name="tool1", args='{"arg1": "value1"}', id="1", index=0
        name="tool2", args='{"arg2": "value2"}', id=None, index=1
def test_merge_tool_calls_parallel_same_index() -> None:
    """Test parallel tool calls with same index but different IDs."""
    # Two parallel tool calls with the same index but different IDs
        name="read_file", args='{"path": "foo.txt"}', id="tooluse_ABC", index=0
        name="search_text", args='{"query": "bar"}', id="tooluse_DEF", index=0
    assert merged[0]["name"] == "read_file"
    assert merged[0]["id"] == "tooluse_ABC"
    assert merged[1]["name"] == "search_text"
    assert merged[1]["id"] == "tooluse_DEF"
    # Streaming continuation: same index, id=None on continuation chunk
    # should still merge correctly with the original chunk
    first = create_tool_call_chunk(name="tool1", args="", id="id1", index=0)
    continuation = create_tool_call_chunk(
        name=None, args='{"key": "value"}', id=None, index=0
    merged = merge_lists([first], [continuation])
    assert len(merged) == 1
    assert merged[0]["name"] == "tool1"
    assert merged[0]["args"] == '{"key": "value"}'
    assert merged[0]["id"] == "id1"
    # Three parallel tool calls all with the same index
    tc1 = create_tool_call_chunk(name="tool_a", args="{}", id="id_a", index=0)
    tc2 = create_tool_call_chunk(name="tool_b", args="{}", id="id_b", index=0)
    tc3 = create_tool_call_chunk(name="tool_c", args="{}", id="id_c", index=0)
    merged = merge_lists([tc1], [tc2], [tc3])
    assert len(merged) == 3
    assert [m["name"] for m in merged] == ["tool_a", "tool_b", "tool_c"]
    assert [m["id"] for m in merged] == ["id_a", "id_b", "id_c"]
def test_tool_message_serdes() -> None:
    message = ToolMessage(
        "foo", artifact={"bar": {"baz": 123}}, tool_call_id="1", status="error"
    ser_message = {
        "lc": 1,
        "type": "constructor",
        "id": ["langchain", "schema", "messages", "ToolMessage"],
        "kwargs": {
            "content": "foo",
            "type": "tool",
            "tool_call_id": "1",
            "artifact": {"bar": {"baz": 123}},
            "status": "error",
    assert dumpd(message) == ser_message
    assert load(dumpd(message), allowed_objects=[ToolMessage]) == message
class BadObject:
def test_tool_message_ser_non_serializable() -> None:
    bad_obj = BadObject()
    message = ToolMessage("foo", artifact=bad_obj, tool_call_id="1")
            "artifact": {
                "type": "not_implemented",
                "id": ["tests", "unit_tests", "test_messages", "BadObject"],
                "repr": repr(bad_obj),
    with pytest.raises(NotImplementedError):
        load(dumpd(message), allowed_objects=[ToolMessage])
def test_tool_message_to_dict() -> None:
    message = ToolMessage("foo", artifact={"bar": {"baz": 123}}, tool_call_id="1")
    expected = {
            "additional_kwargs": {},
    actual = message_to_dict(message)
    assert actual == expected
def test_tool_message_repr() -> None:
        "ToolMessage(content='foo', tool_call_id='1', artifact={'bar': {'baz': 123}})"
    actual = repr(message)
def test_tool_message_str() -> None:
    expected = "content='foo' tool_call_id='1' artifact={'bar': {'baz': 123}}"
    actual = str(message)
    ("first", "others", "expected"),
        ("", [""], ""),
        ("", [[]], [""]),
        ([], [""], []),
        ([], [[]], []),
        ("foo", [""], "foo"),
        ("foo", [[]], ["foo"]),
        (["foo"], [""], ["foo"]),
        (["foo"], [[]], ["foo"]),
        ("foo", ["bar"], "foobar"),
        ("foo", [["bar"]], ["foo", "bar"]),
        (["foo"], ["bar"], ["foobar"]),
        (["foo"], [["bar"]], ["foo", "bar"]),
            [{"text": "foo"}],
            [[{"index": 0, "text": "bar"}]],
            [{"text": "foo"}, {"index": 0, "text": "bar"}],
def test_merge_content(first: list | str, others: list, expected: list | str) -> None:
    actual = merge_content(first, *others)
def test_tool_message_content() -> None:
    ToolMessage("foo", tool_call_id="1")
    ToolMessage(["foo"], tool_call_id="1")
    ToolMessage([{"foo": "bar"}], tool_call_id="1")
    # Ignoring since we're testing that tuples get converted to lists in `coerce_args`
    assert ToolMessage(("a", "b", "c"), tool_call_id="1").content == ["a", "b", "c"]  # type: ignore[call-overload]
    assert ToolMessage(5, tool_call_id="1").content == "5"  # type: ignore[call-overload]
    assert ToolMessage(5.1, tool_call_id="1").content == "5.1"  # type: ignore[call-overload]
    assert ToolMessage({"foo": "bar"}, tool_call_id="1").content == "{'foo': 'bar'}"  # type: ignore[call-overload]
        ToolMessage(Document("foo"), tool_call_id="1").content == "page_content='foo'"  # type: ignore[call-overload]
def test_tool_message_tool_call_id() -> None:
    ToolMessage("foo", tool_call_id=uuid.uuid4())
    ToolMessage("foo", tool_call_id=1)
    ToolMessage("foo", tool_call_id=1.0)
def test_message_text() -> None:
    # partitions:
    # message types: [ai], [human], [system], [tool]
    # content types: [str], [list[str]], [list[dict]], [list[str | dict]]
    # content: [empty], [single element], [multiple elements]
    # content dict types: [text], [not text], [no type]
    assert HumanMessage(content="foo").text == "foo"
    assert AIMessage(content=[]).text == ""
    assert AIMessage(content=["foo", "bar"]).text == "foobar"
                {"type": "text", "text": "<thinking>thinking...</thinking>"},
                    "type": "tool_use",
                    "id": "toolu_01A09q90qw90lq917835lq9",
                    "input": {"location": "San Francisco, CA"},
        ).text
        == "<thinking>thinking...</thinking>"
        SystemMessage(content=[{"type": "text", "text": "foo"}, "bar"]).text == "foobar"
                {"type": "text", "text": "15 degrees"},
                    "type": "image",
                        "type": "base64",
                        "media_type": "image/jpeg",
            tool_call_id="1",
        == "15 degrees"
        AIMessage(content=[{"text": "hi there"}, "hi"]).text == "hi"
    )  # missing type: text
    assert AIMessage(content=[{"type": "nottext", "text": "hi"}]).text == ""
            content="", tool_calls=[create_tool_call(name="a", args={"b": 1}, id=None)]
        == ""
def test_is_data_content_block() -> None:
    # Test all DataContentBlock types with various data fields
    # Image blocks
    assert is_data_content_block({"type": "image", "url": "https://..."})
    assert is_data_content_block(
            "base64": "<base64 data>",
    # Video blocks
    assert is_data_content_block({"type": "video", "url": "https://video.mp4"})
            "type": "video",
            "base64": "<base64 video>",
            "mime_type": "video/mp4",
    assert is_data_content_block({"type": "video", "file_id": "vid_123"})
    # Audio blocks
    assert is_data_content_block({"type": "audio", "url": "https://audio.mp3"})
            "type": "audio",
            "base64": "<base64 audio>",
            "mime_type": "audio/mp3",
    assert is_data_content_block({"type": "audio", "file_id": "aud_123"})
    # Plain text blocks
    assert is_data_content_block({"type": "text-plain", "text": "document content"})
    assert is_data_content_block({"type": "text-plain", "url": "https://doc.txt"})
    assert is_data_content_block({"type": "text-plain", "file_id": "txt_123"})
    # File blocks
    assert is_data_content_block({"type": "file", "url": "https://file.pdf"})
            "type": "file",
            "base64": "<base64 file>",
            "mime_type": "application/pdf",
    assert is_data_content_block({"type": "file", "file_id": "file_123"})
    # Blocks with additional metadata (should still be valid)
            "cache_control": {"type": "ephemeral"},
            "metadata": {"cache_control": {"type": "ephemeral"}},
            "extras": "hi",
    # Invalid cases - wrong type
    assert not is_data_content_block({"type": "text", "text": "foo"})
    assert not is_data_content_block(
            "image_url": {"url": "https://..."},
        }  # This is OpenAI Chat Completions
    assert not is_data_content_block({"type": "tool_call", "name": "func", "args": {}})
    assert not is_data_content_block({"type": "invalid", "url": "something"})
    # Invalid cases - valid type but no data or `source_type` fields
    assert not is_data_content_block({"type": "image"})
    assert not is_data_content_block({"type": "video", "mime_type": "video/mp4"})
    assert not is_data_content_block({"type": "audio", "extras": {"key": "value"}})
    # Invalid cases - valid type but wrong data field name
    assert not is_data_content_block({"type": "image", "source": "<base64 data>"})
    assert not is_data_content_block({"type": "video", "data": "video_data"})
    # Edge cases - empty or missing values
    assert not is_data_content_block({})
    assert not is_data_content_block({"url": "https://..."})  # missing type
def test_convert_to_openai_image_block() -> None:
    for input_block in [
            "url": "https://...",
            "source_type": "url",
        result = convert_to_openai_image_block(input_block)
        assert result == expected
            "data": "<base64 data>",
                "url": "data:image/jpeg;base64,<base64 data>",
def test_known_block_types() -> None:
        bt
        for bt in get_args(ContentBlock)
        for bt in get_args(bt.__annotations__["type"])
    # Normalize any Literal[...] types in block types to their string values.
    # This ensures all entries are plain strings, not Literal objects.
        t
        if isinstance(t, str)
        else t.__args__[0]
        if hasattr(t, "__args__") and len(t.__args__) == 1
        else t
        for t in expected
    assert expected == KNOWN_BLOCK_TYPES
def test_typed_init() -> None:
    ai_message = AIMessage(content_blocks=[{"type": "text", "text": "Hello"}])
    assert ai_message.content == [{"type": "text", "text": "Hello"}]
    assert ai_message.content_blocks == ai_message.content
    human_message = HumanMessage(content_blocks=[{"type": "text", "text": "Hello"}])
    assert human_message.content == [{"type": "text", "text": "Hello"}]
    assert human_message.content_blocks == human_message.content
    system_message = SystemMessage(content_blocks=[{"type": "text", "text": "Hello"}])
    assert system_message.content == [{"type": "text", "text": "Hello"}]
    assert system_message.content_blocks == system_message.content
        content_blocks=[{"type": "text", "text": "Hello"}],
        tool_call_id="abc123",
    assert tool_message.content == [{"type": "text", "text": "Hello"}]
    assert tool_message.content_blocks == tool_message.content
    for message_class in [AIMessage, HumanMessage, SystemMessage]:
        message = message_class("Hello")
        assert message.content == "Hello"
        assert message.content_blocks == [{"type": "text", "text": "Hello"}]
        message = message_class(content="Hello")
    # Test we get type errors for malformed blocks (type checker will complain if
    # below type-ignores are unused).
    _ = AIMessage(content_blocks=[{"type": "text", "bad": "Hello"}])  # type: ignore[list-item]
    _ = HumanMessage(content_blocks=[{"type": "text", "bad": "Hello"}])  # type: ignore[list-item]
    _ = SystemMessage(content_blocks=[{"type": "text", "bad": "Hello"}])  # type: ignore[list-item]
    _ = ToolMessage(
        content_blocks=[{"type": "text", "bad": "Hello"}],  # type: ignore[list-item]
def test_text_accessor() -> None:
    """Test that `message.text` property and `.text()` method return the same value."""
    human_msg = HumanMessage(content="Hello world")
    assert human_msg.text == "Hello world"
    assert str(human_msg.text) == str(human_msg.text)
    system_msg = SystemMessage(content="You are a helpful assistant")
    assert system_msg.text == "You are a helpful assistant"
    assert str(system_msg.text) == str(system_msg.text)
    ai_msg = AIMessage(content="I can help you with that")
    assert ai_msg.text == "I can help you with that"
    assert str(ai_msg.text) == str(ai_msg.text)
    tool_msg = ToolMessage(content="Task completed", tool_call_id="tool_1")
    assert tool_msg.text == "Task completed"
    assert str(tool_msg.text) == str(tool_msg.text)
    complex_msg = HumanMessage(
        content=[{"type": "text", "text": "Hello "}, {"type": "text", "text": "world"}]
    assert complex_msg.text == "Hello world"
    assert str(complex_msg.text) == str(complex_msg.text)
    mixed_msg = AIMessage(
            {"type": "text", "text": "The answer is "},
            {"type": "tool_use", "name": "calculate", "input": {"x": 2}, "id": "1"},
            {"type": "text", "text": "42"},
    assert mixed_msg.text == "The answer is 42"
    assert str(mixed_msg.text) == str(mixed_msg.text)
    empty_msg = HumanMessage(content=[])
    assert empty_msg.text == ""
    assert str(empty_msg.text) == str(empty_msg.text)
from langchain_classic.schema.messages import __all__
EXPECTED_ALL = [
