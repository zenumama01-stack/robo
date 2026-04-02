from openai._utils import assert_signatures_in_sync
    CompactedResponse,
class TestResponses:
        response = client.responses.create()
        assert_matches_type(Response, response, path=["response"])
        response = client.responses.create(
            context_management=[
                    "type": "type",
                    "compact_threshold": 1000,
            conversation="string",
            include=["file_search_call.results"],
            input="string",
            instructions="instructions",
            max_output_tokens=0,
            max_tool_calls=0,
            model="gpt-5.1",
            previous_response_id="previous_response_id",
            prompt={
                "id": "id",
                "variables": {"foo": "string"},
            prompt_cache_key="prompt-cache-key-1234",
            prompt_cache_retention="in-memory",
            reasoning={
                "effort": "none",
                "generate_summary": "auto",
                "summary": "auto",
            safety_identifier="safety-identifier-1234",
            service_tier="auto",
            store=True,
            stream_options={"include_obfuscation": True},
            text={
                "format": {"type": "text"},
                "verbosity": "low",
            tool_choice="none",
                    "parameters": {"foo": "bar"},
                    "defer_loading": True,
                    "description": "description",
            top_logprobs=0,
            truncation="auto",
        http_response = client.responses.with_raw_response.create()
        assert http_response.is_closed is True
        assert http_response.http_request.headers.get("X-Stainless-Lang") == "python"
        response = http_response.parse()
        with client.responses.with_streaming_response.create() as http_response:
            assert not http_response.is_closed
        assert cast(Any, http_response.is_closed) is True
        response_stream = client.responses.create(
        response_stream.response.close()
        response = client.responses.with_raw_response.create(
        with client.responses.with_streaming_response.create(
    def test_method_retrieve_overload_1(self, client: OpenAI) -> None:
        response = client.responses.retrieve(
            response_id="resp_677efb5139a88190b512bc3fef8e535d",
    def test_method_retrieve_with_all_params_overload_1(self, client: OpenAI) -> None:
            include_obfuscation=True,
            starting_after=0,
    def test_raw_response_retrieve_overload_1(self, client: OpenAI) -> None:
        http_response = client.responses.with_raw_response.retrieve(
    def test_streaming_response_retrieve_overload_1(self, client: OpenAI) -> None:
        with client.responses.with_streaming_response.retrieve(
        ) as http_response:
    def test_path_params_retrieve_overload_1(self, client: OpenAI) -> None:
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `response_id` but received ''"):
            client.responses.with_raw_response.retrieve(
                response_id="",
    def test_method_retrieve_overload_2(self, client: OpenAI) -> None:
        response_stream = client.responses.retrieve(
    def test_method_retrieve_with_all_params_overload_2(self, client: OpenAI) -> None:
    def test_raw_response_retrieve_overload_2(self, client: OpenAI) -> None:
        response = client.responses.with_raw_response.retrieve(
    def test_streaming_response_retrieve_overload_2(self, client: OpenAI) -> None:
    def test_path_params_retrieve_overload_2(self, client: OpenAI) -> None:
        response = client.responses.delete(
            "resp_677efb5139a88190b512bc3fef8e535d",
        assert response is None
        http_response = client.responses.with_raw_response.delete(
        with client.responses.with_streaming_response.delete(
            client.responses.with_raw_response.delete(
        response = client.responses.cancel(
        http_response = client.responses.with_raw_response.cancel(
        with client.responses.with_streaming_response.cancel(
            client.responses.with_raw_response.cancel(
    def test_method_compact(self, client: OpenAI) -> None:
        response = client.responses.compact(
        assert_matches_type(CompactedResponse, response, path=["response"])
    def test_method_compact_with_all_params(self, client: OpenAI) -> None:
            previous_response_id="resp_123",
            prompt_cache_key="prompt_cache_key",
    def test_raw_response_compact(self, client: OpenAI) -> None:
        http_response = client.responses.with_raw_response.compact(
    def test_streaming_response_compact(self, client: OpenAI) -> None:
        with client.responses.with_streaming_response.compact(
def test_parse_method_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
    checking_client: OpenAI | AsyncOpenAI = client if sync else async_client
    assert_signatures_in_sync(
        checking_client.responses.create,
        checking_client.responses.parse,
        exclude_params={"stream", "tools"},
class TestAsyncResponses:
        response = await async_client.responses.create()
        response = await async_client.responses.create(
        http_response = await async_client.responses.with_raw_response.create()
        async with async_client.responses.with_streaming_response.create() as http_response:
            response = await http_response.parse()
        response_stream = await async_client.responses.create(
        await response_stream.response.aclose()
        response = await async_client.responses.with_raw_response.create(
        async with async_client.responses.with_streaming_response.create(
    async def test_method_retrieve_overload_1(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.responses.retrieve(
    async def test_method_retrieve_with_all_params_overload_1(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_retrieve_overload_1(self, async_client: AsyncOpenAI) -> None:
        http_response = await async_client.responses.with_raw_response.retrieve(
    async def test_streaming_response_retrieve_overload_1(self, async_client: AsyncOpenAI) -> None:
        async with async_client.responses.with_streaming_response.retrieve(
    async def test_path_params_retrieve_overload_1(self, async_client: AsyncOpenAI) -> None:
            await async_client.responses.with_raw_response.retrieve(
    async def test_method_retrieve_overload_2(self, async_client: AsyncOpenAI) -> None:
        response_stream = await async_client.responses.retrieve(
    async def test_method_retrieve_with_all_params_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_retrieve_overload_2(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.responses.with_raw_response.retrieve(
    async def test_streaming_response_retrieve_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_path_params_retrieve_overload_2(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.responses.delete(
        http_response = await async_client.responses.with_raw_response.delete(
        async with async_client.responses.with_streaming_response.delete(
            await async_client.responses.with_raw_response.delete(
        response = await async_client.responses.cancel(
        http_response = await async_client.responses.with_raw_response.cancel(
        async with async_client.responses.with_streaming_response.cancel(
            await async_client.responses.with_raw_response.cancel(
    async def test_method_compact(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.responses.compact(
    async def test_method_compact_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_compact(self, async_client: AsyncOpenAI) -> None:
        http_response = await async_client.responses.with_raw_response.compact(
    async def test_streaming_response_compact(self, async_client: AsyncOpenAI) -> None:
        async with async_client.responses.with_streaming_response.compact(
from ..snapshots import make_snapshot_request
def test_output_text(client: OpenAI, respx_mock: MockRouter) -> None:
        lambda c: c.responses.create(
            input="What's the weather like in SF?",
            '{"id": "resp_689a0b2545288193953c892439b42e2800b2e36c65a1fd4b", "object": "response", "created_at": 1754925861, "status": "completed", "background": false, "error": null, "incomplete_details": null, "instructions": null, "max_output_tokens": null, "max_tool_calls": null, "model": "gpt-4o-mini-2024-07-18", "output": [{"id": "msg_689a0b2637b08193ac478e568f49e3f900b2e36c65a1fd4b", "type": "message", "status": "completed", "content": [{"type": "output_text", "annotations": [], "logprobs": [], "text": "I can\'t provide real-time updates, but you can easily check the current weather in San Francisco using a weather website or app. Typically, San Francisco has cool, foggy summers and mild winters, so it\'s good to be prepared for variable weather!"}], "role": "assistant"}], "parallel_tool_calls": true, "previous_response_id": null, "prompt_cache_key": null, "reasoning": {"effort": null, "summary": null}, "safety_identifier": null, "service_tier": "default", "store": true, "temperature": 1.0, "text": {"format": {"type": "text"}, "verbosity": "medium"}, "tool_choice": "auto", "tools": [], "top_logprobs": 0, "top_p": 1.0, "truncation": "disabled", "usage": {"input_tokens": 14, "input_tokens_details": {"cached_tokens": 0}, "output_tokens": 50, "output_tokens_details": {"reasoning_tokens": 0}, "total_tokens": 64}, "user": null, "metadata": {}}'
        path="/responses",
    assert response.output_text == snapshot(
        "I can't provide real-time updates, but you can easily check the current weather in San Francisco using a weather website or app. Typically, San Francisco has cool, foggy summers and mild winters, so it's good to be prepared for variable weather!"
def test_stream_method_definition_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
        checking_client.responses.stream,
def test_parse_method_definition_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
        exclude_params={"tools"},
"""Unit tests for langchain.agents.structured_output module."""
from langchain.agents.structured_output import (
    OutputToolBinding,
    ProviderStrategy,
    ProviderStrategyBinding,
    ToolStrategy,
    _SchemaSpec,
class _TestModel(BaseModel):
    """A test model for structured output."""
    email: str = "default@example.com"
    """Custom model with a custom docstring."""
    value: float
class EmptyDocModel(BaseModel):
    # No custom docstring, should have no description in tool
class TestToolStrategy:
    """Test ToolStrategy dataclass."""
    def test_basic_creation(self) -> None:
        """Test basic ToolStrategy creation."""
        strategy = ToolStrategy(schema=_TestModel)
        assert strategy.schema == _TestModel
        assert strategy.tool_message_content is None
        assert len(strategy.schema_specs) == 1
        assert strategy.schema_specs[0].schema == _TestModel
    def test_multiple_schemas(self) -> None:
        """Test ToolStrategy with multiple schemas."""
        strategy = ToolStrategy(schema=_TestModel | CustomModel)
        assert len(strategy.schema_specs) == 2
        assert strategy.schema_specs[1].schema == CustomModel
    def test_schema_with_tool_message_content(self) -> None:
        """Test ToolStrategy with tool message content."""
        strategy = ToolStrategy(schema=_TestModel, tool_message_content="custom message")
        assert strategy.tool_message_content == "custom message"
class TestProviderStrategy:
    """Test ProviderStrategy dataclass."""
        """Test basic ProviderStrategy creation."""
        strategy = ProviderStrategy(schema=_TestModel)
        assert strategy.schema_spec.schema == _TestModel
        assert strategy.schema_spec.strict is None
    def test_strict(self) -> None:
        """Test ProviderStrategy creation with strict=True."""
        strategy = ProviderStrategy(schema=_TestModel, strict=True)
        assert strategy.schema_spec.strict is True
    def test_to_model_kwargs(self) -> None:
        strategy_default = ProviderStrategy(schema=_TestModel)
        assert strategy_default.to_model_kwargs() == {
            "response_format": {
                    "name": "_TestModel",
                    "schema": {
                        "description": "A test model for structured output.",
                            "age": {"title": "Age", "type": "integer"},
                            "email": {
                                "default": "default@example.com",
                                "title": "Email",
                        "required": ["name", "age"],
                        "title": "_TestModel",
    def test_to_model_kwargs_strict(self) -> None:
        strategy_default = ProviderStrategy(schema=_TestModel, strict=True)
class TestOutputToolBinding:
    """Test OutputToolBinding dataclass and its methods."""
    def test_from_schema_spec_basic(self) -> None:
        """Test basic OutputToolBinding creation from SchemaSpec."""
        schema_spec = _SchemaSpec(schema=_TestModel)
        tool_binding = OutputToolBinding.from_schema_spec(schema_spec)
        assert tool_binding.schema == _TestModel
        assert tool_binding.schema_kind == "pydantic"
        assert tool_binding.tool is not None
        assert tool_binding.tool.name == "_TestModel"
    def test_from_schema_spec_with_custom_name(self) -> None:
        """Test OutputToolBinding creation with custom name."""
        schema_spec = _SchemaSpec(schema=_TestModel, name="custom_tool_name")
        assert tool_binding.tool.name == "custom_tool_name"
    def test_from_schema_spec_with_custom_description(self) -> None:
        """Test OutputToolBinding creation with custom description."""
        schema_spec = _SchemaSpec(schema=_TestModel, description="Custom tool description")
        assert tool_binding.tool.description == "Custom tool description"
    def test_from_schema_spec_with_model_docstring(self) -> None:
        """Test OutputToolBinding creation using model docstring as description."""
        schema_spec = _SchemaSpec(schema=CustomModel)
        assert tool_binding.tool.description == "Custom model with a custom docstring."
    def test_from_schema_spec_empty_docstring(self) -> None:
        """Test OutputToolBinding creation with model that has default docstring."""
        # Create a model with the same docstring as BaseModel
        class DefaultDocModel(BaseModel):
            # This should have the same docstring as BaseModel
        schema_spec = _SchemaSpec(schema=DefaultDocModel)
        # Should use empty description when model has default BaseModel docstring
        assert not tool_binding.tool.description
    def test_parse_payload_pydantic_success(self) -> None:
        """Test successful parsing for Pydantic model."""
        tool_args = {"name": "John", "age": 30}
        result = tool_binding.parse(tool_args)
        assert isinstance(result, _TestModel)
        assert result.name == "John"
        assert result.age == 30
        assert result.email == "default@example.com"  # default value
    def test_parse_payload_pydantic_validation_error(self) -> None:
        """Test parsing failure for invalid Pydantic data."""
        # Missing required field 'name'
        tool_args = {"age": 30}
        with pytest.raises(ValueError, match="Failed to parse data to _TestModel"):
            tool_binding.parse(tool_args)
class TestProviderStrategyBinding:
    """Test ProviderStrategyBinding dataclass and its methods."""
        """Test basic ProviderStrategyBinding creation from SchemaSpec."""
        tool_binding = ProviderStrategyBinding.from_schema_spec(schema_spec)
        message = AIMessage(content='{"name": "John", "age": 30}')
        result = tool_binding.parse(message)
        message = AIMessage(content='{"age": 30}')
            tool_binding.parse(message)
    def test_parse_payload_pydantic_json_error(self) -> None:
        """Test parsing failure for invalid JSON data."""
        message = AIMessage(content="invalid json")
            match="Native structured output expected valid JSON for _TestModel, but parsing failed",
    def test_parse_content_list(self) -> None:
        """Test successful parsing for Pydantic model with content as list."""
            content=['{"name":', {"content": ' "John",'}, {"type": "text", "text": ' "age": 30}'}]
class TestEdgeCases:
    """Test edge cases and error conditions."""
    def test_single_schema(self) -> None:
        """Test ToolStrategy with a single schema creates one schema spec."""
        strategy = ToolStrategy(EmptyDocModel)
    def test_empty_docstring_model(self) -> None:
        """Test that models without explicit docstrings have empty tool descriptions."""
        binding = OutputToolBinding.from_schema_spec(_SchemaSpec(EmptyDocModel))
        assert binding.tool.name == "EmptyDocModel"
        assert not binding.tool.description
