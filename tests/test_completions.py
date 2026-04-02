from openai.types import Completion
class TestCompletions:
    def test_method_create_overload_1(self, client: OpenAI) -> None:
        completion = client.completions.create(
            model="string",
            prompt="This is a test.",
        assert_matches_type(Completion, completion, path=["response"])
    def test_method_create_with_all_params_overload_1(self, client: OpenAI) -> None:
            best_of=0,
            echo=True,
            frequency_penalty=-2,
            logit_bias={"foo": 0},
            logprobs=0,
            max_tokens=16,
            presence_penalty=-2,
            seed=0,
            stop="\n",
            stream_options={
                "include_obfuscation": True,
                "include_usage": True,
            suffix="test.",
            temperature=1,
            top_p=1,
            user="user-1234",
    def test_raw_response_create_overload_1(self, client: OpenAI) -> None:
        response = client.completions.with_raw_response.create(
    def test_streaming_response_create_overload_1(self, client: OpenAI) -> None:
        with client.completions.with_streaming_response.create(
    def test_method_create_overload_2(self, client: OpenAI) -> None:
        completion_stream = client.completions.create(
        completion_stream.response.close()
    def test_method_create_with_all_params_overload_2(self, client: OpenAI) -> None:
    def test_raw_response_create_overload_2(self, client: OpenAI) -> None:
        stream = response.parse()
    def test_streaming_response_create_overload_2(self, client: OpenAI) -> None:
class TestAsyncCompletions:
    async def test_method_create_overload_1(self, async_client: AsyncOpenAI) -> None:
        completion = await async_client.completions.create(
    async def test_method_create_with_all_params_overload_1(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_create_overload_1(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.completions.with_raw_response.create(
    async def test_streaming_response_create_overload_1(self, async_client: AsyncOpenAI) -> None:
        async with async_client.completions.with_streaming_response.create(
            completion = await response.parse()
    async def test_method_create_overload_2(self, async_client: AsyncOpenAI) -> None:
        completion_stream = await async_client.completions.create(
        await completion_stream.response.aclose()
    async def test_method_create_with_all_params_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_create_overload_2(self, async_client: AsyncOpenAI) -> None:
        await stream.close()
    async def test_streaming_response_create_overload_2(self, async_client: AsyncOpenAI) -> None:
            stream = await response.parse()
from openai.types.chat import (
    ChatCompletionDeleted,
        assert_matches_type(ChatCompletion, completion, path=["response"])
            audio={
                "format": "wav",
                "voice": "string",
            function_call="none",
            functions=[
            logprobs=True,
            max_completion_tokens=0,
            max_tokens=0,
            modalities=["text"],
            prediction={
                "type": "content",
            response_format={"type": "text"},
            seed=-9007199254740991,
            verbosity="low",
            web_search_options={
                "search_context_size": "low",
                "user_location": {
                    "approximate": {
                        "city": "city",
                        "country": "country",
                        "region": "region",
                        "timezone": "timezone",
                    "type": "approximate",
        completion_stream = client.chat.completions.create(
        completion = client.chat.completions.retrieve(
            "completion_id",
        response = client.chat.completions.with_raw_response.retrieve(
        with client.chat.completions.with_streaming_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `completion_id` but received ''"):
            client.chat.completions.with_raw_response.retrieve(
        completion = client.chat.completions.update(
            completion_id="completion_id",
        response = client.chat.completions.with_raw_response.update(
        with client.chat.completions.with_streaming_response.update(
            client.chat.completions.with_raw_response.update(
                completion_id="",
        completion = client.chat.completions.list()
        assert_matches_type(SyncCursorPage[ChatCompletion], completion, path=["response"])
        completion = client.chat.completions.list(
            model="model",
        response = client.chat.completions.with_raw_response.list()
        with client.chat.completions.with_streaming_response.list() as response:
        completion = client.chat.completions.delete(
        assert_matches_type(ChatCompletionDeleted, completion, path=["response"])
        response = client.chat.completions.with_raw_response.delete(
        with client.chat.completions.with_streaming_response.delete(
            client.chat.completions.with_raw_response.delete(
    def test_method_create_disallows_pydantic(self, client: OpenAI) -> None:
        class MyModel(pydantic.BaseModel):
        with pytest.raises(TypeError, match=r"You tried to pass a `BaseModel` class"):
            client.chat.completions.create(
                response_format=cast(Any, MyModel),
        completion = await async_client.chat.completions.create(
        response = await async_client.chat.completions.with_raw_response.create(
        async with async_client.chat.completions.with_streaming_response.create(
        completion_stream = await async_client.chat.completions.create(
        completion = await async_client.chat.completions.retrieve(
        response = await async_client.chat.completions.with_raw_response.retrieve(
        async with async_client.chat.completions.with_streaming_response.retrieve(
            await async_client.chat.completions.with_raw_response.retrieve(
        completion = await async_client.chat.completions.update(
        response = await async_client.chat.completions.with_raw_response.update(
        async with async_client.chat.completions.with_streaming_response.update(
            await async_client.chat.completions.with_raw_response.update(
        completion = await async_client.chat.completions.list()
        assert_matches_type(AsyncCursorPage[ChatCompletion], completion, path=["response"])
        completion = await async_client.chat.completions.list(
        response = await async_client.chat.completions.with_raw_response.list()
        async with async_client.chat.completions.with_streaming_response.list() as response:
        completion = await async_client.chat.completions.delete(
        response = await async_client.chat.completions.with_raw_response.delete(
        async with async_client.chat.completions.with_streaming_response.delete(
            await async_client.chat.completions.with_raw_response.delete(
    async def test_method_create_disallows_pydantic(self, async_client: AsyncOpenAI) -> None:
            await async_client.chat.completions.create(
from typing_extensions import Literal, TypeVar
from ..utils import print_obj
from ...conftest import base_url
from ..snapshots import make_snapshot_request, make_async_snapshot_request
from ..schema_types.query import Query
# all the snapshots in this file are auto-generated from the live API
# you can update them with
# `OPENAI_LIVE=1 pytest --inline-snapshot=fix -p no:xdist -o addopts=""`
def test_parse_nothing(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
    completion = make_snapshot_request(
        lambda c: c.chat.completions.parse(
                    "content": "What's the weather like in SF?",
        content_snapshot=snapshot(
            '{"id": "chatcmpl-ABfvaueLEMLNYbT8YzpJxsmiQ6HSY", "object": "chat.completion", "created": 1727346142, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": "I\'m unable to provide real-time weather updates. To get the current weather in San Francisco, I recommend checking a reliable weather website or app like the Weather Channel or a local news station.", "refusal": null}, "logprobs": null, "finish_reason": "stop"}], "usage": {"prompt_tokens": 14, "completion_tokens": 37, "total_tokens": 51, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_b40fb1c6fb"}'
        path="/chat/completions",
        mock_client=client,
        respx_mock=respx_mock,
    assert print_obj(completion, monkeypatch) == snapshot(
        """\
ParsedChatCompletion(
    choices=[
        ParsedChoice(
            finish_reason='stop',
            logprobs=None,
            message=ParsedChatCompletionMessage(
                annotations=None,
                audio=None,
                content="I'm unable to provide real-time weather updates. To get the current weather in San Francisco, I
recommend checking a reliable weather website or app like the Weather Channel or a local news station.",
                function_call=None,
                parsed=None,
                refusal=None,
                role='assistant',
                tool_calls=None
    created=1727346142,
    id='chatcmpl-ABfvaueLEMLNYbT8YzpJxsmiQ6HSY',
    model='gpt-4o-2024-08-06',
    object='chat.completion',
    service_tier=None,
    system_fingerprint='fp_b40fb1c6fb',
    usage=CompletionUsage(
        completion_tokens=37,
        completion_tokens_details=CompletionTokensDetails(
            accepted_prediction_tokens=None,
            audio_tokens=None,
            reasoning_tokens=0,
            rejected_prediction_tokens=None
        prompt_tokens=14,
        prompt_tokens_details=None,
        total_tokens=51
def test_parse_pydantic_model(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
    class Location(BaseModel):
        units: Literal["c", "f"]
            response_format=Location,
            '{"id": "chatcmpl-ABfvbtVnTu5DeC4EFnRYj8mtfOM99", "object": "chat.completion", "created": 1727346143, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": "{\\"city\\":\\"San Francisco\\",\\"temperature\\":65,\\"units\\":\\"f\\"}", "refusal": null}, "logprobs": null, "finish_reason": "stop"}], "usage": {"prompt_tokens": 79, "completion_tokens": 14, "total_tokens": 93, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_5050236cbd"}'
                content='{"city":"San Francisco","temperature":65,"units":"f"}',
                parsed=Location(city='San Francisco', temperature=65.0, units='f'),
    created=1727346143,
    id='chatcmpl-ABfvbtVnTu5DeC4EFnRYj8mtfOM99',
    system_fingerprint='fp_5050236cbd',
        completion_tokens=14,
        prompt_tokens=79,
        total_tokens=93
def test_parse_pydantic_model_optional_default(
    client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch
        units: Optional[Literal["c", "f"]] = None
            '{"id": "chatcmpl-ABfvcC8grKYsRkSoMp9CCAhbXAd0b", "object": "chat.completion", "created": 1727346144, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": "{\\"city\\":\\"San Francisco\\",\\"temperature\\":65,\\"units\\":\\"f\\"}", "refusal": null}, "logprobs": null, "finish_reason": "stop"}], "usage": {"prompt_tokens": 88, "completion_tokens": 14, "total_tokens": 102, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_b40fb1c6fb"}'
    created=1727346144,
    id='chatcmpl-ABfvcC8grKYsRkSoMp9CCAhbXAd0b',
        prompt_tokens=88,
        total_tokens=102
def test_parse_pydantic_model_enum(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
        """The detected color"""
        color: Color
        ColorDetection.update_forward_refs(**locals())  # type: ignore
                {"role": "user", "content": "What color is a Coke can?"},
            response_format=ColorDetection,
            '{"id": "chatcmpl-ABfvjIatz0zrZu50gRbMtlp0asZpz", "object": "chat.completion", "created": 1727346151, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": "{\\"color\\":\\"red\\",\\"hex_color_code\\":\\"#FF0000\\"}", "refusal": null}, "logprobs": null, "finish_reason": "stop"}], "usage": {"prompt_tokens": 109, "completion_tokens": 14, "total_tokens": 123, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_5050236cbd"}'
    assert print_obj(completion.choices[0], monkeypatch) == snapshot(
        content='{"color":"red","hex_color_code":"#FF0000"}',
        parsed=ColorDetection(color=<Color.RED: 'red'>, hex_color_code='#FF0000'),
def test_parse_pydantic_model_multiple_choices(
            n=3,
            '{"id": "chatcmpl-ABfvp8qzboW92q8ONDF4DPHlI7ckC", "object": "chat.completion", "created": 1727346157, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": "{\\"city\\":\\"San Francisco\\",\\"temperature\\":64,\\"units\\":\\"f\\"}", "refusal": null}, "logprobs": null, "finish_reason": "stop"}, {"index": 1, "message": {"role": "assistant", "content": "{\\"city\\":\\"San Francisco\\",\\"temperature\\":65,\\"units\\":\\"f\\"}", "refusal": null}, "logprobs": null, "finish_reason": "stop"}, {"index": 2, "message": {"role": "assistant", "content": "{\\"city\\":\\"San Francisco\\",\\"temperature\\":63.0,\\"units\\":\\"f\\"}", "refusal": null}, "logprobs": null, "finish_reason": "stop"}], "usage": {"prompt_tokens": 79, "completion_tokens": 44, "total_tokens": 123, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_b40fb1c6fb"}'
    assert print_obj(completion.choices, monkeypatch) == snapshot(
            content='{"city":"San Francisco","temperature":64,"units":"f"}',
            parsed=Location(city='San Francisco', temperature=64.0, units='f'),
        index=1,
        index=2,
            content='{"city":"San Francisco","temperature":63.0,"units":"f"}',
            parsed=Location(city='San Francisco', temperature=63.0, units='f'),
@pytest.mark.skipif(PYDANTIC_V1, reason="dataclasses only supported in v2")
def test_parse_pydantic_dataclass(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
    from pydantic.dataclasses import dataclass
    @dataclass
    class CalendarEvent:
        date: str
        participants: List[str]
                {"role": "system", "content": "Extract the event information."},
                {"role": "user", "content": "Alice and Bob are going to a science fair on Friday."},
            response_format=CalendarEvent,
            '{"id": "chatcmpl-ABfvqhz4uUUWsw8Ohw2Mp9B4sKKV8", "object": "chat.completion", "created": 1727346158, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": "{\\"name\\":\\"Science Fair\\",\\"date\\":\\"Friday\\",\\"participants\\":[\\"Alice\\",\\"Bob\\"]}", "refusal": null}, "logprobs": null, "finish_reason": "stop"}], "usage": {"prompt_tokens": 92, "completion_tokens": 17, "total_tokens": 109, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_7568d46099"}'
                content='{"name":"Science Fair","date":"Friday","participants":["Alice","Bob"]}',
                parsed=CalendarEvent(name='Science Fair', date='Friday', participants=['Alice', 'Bob']),
    created=1727346158,
    id='chatcmpl-ABfvqhz4uUUWsw8Ohw2Mp9B4sKKV8',
    system_fingerprint='fp_7568d46099',
        completion_tokens=17,
        prompt_tokens=92,
        total_tokens=109
def test_pydantic_tool_model_all_types(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
                    "content": "look up all my orders in may of last year that were fulfilled but not delivered on time",
            tools=[openai.pydantic_function_tool(Query)],
            response_format=Query,
            '{"id": "chatcmpl-ABfvtNiaTNUF6OymZUnEFc9lPq9p1", "object": "chat.completion", "created": 1727346161, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": null, "tool_calls": [{"id": "call_NKpApJybW1MzOjZO2FzwYw0d", "type": "function", "function": {"name": "Query", "arguments": "{\\"name\\":\\"May 2022 Fulfilled Orders Not Delivered on Time\\",\\"table_name\\":\\"orders\\",\\"columns\\":[\\"id\\",\\"status\\",\\"expected_delivery_date\\",\\"delivered_at\\",\\"shipped_at\\",\\"ordered_at\\",\\"canceled_at\\"],\\"conditions\\":[{\\"column\\":\\"ordered_at\\",\\"operator\\":\\">=\\",\\"value\\":\\"2022-05-01\\"},{\\"column\\":\\"ordered_at\\",\\"operator\\":\\"<=\\",\\"value\\":\\"2022-05-31\\"},{\\"column\\":\\"status\\",\\"operator\\":\\"=\\",\\"value\\":\\"fulfilled\\"},{\\"column\\":\\"delivered_at\\",\\"operator\\":\\">\\",\\"value\\":{\\"column_name\\":\\"expected_delivery_date\\"}}],\\"order_by\\":\\"asc\\"}"}}], "refusal": null}, "logprobs": null, "finish_reason": "tool_calls"}], "usage": {"prompt_tokens": 512, "completion_tokens": 132, "total_tokens": 644, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_7568d46099"}'
    finish_reason='tool_calls',
        content=None,
            ParsedFunctionToolCall(
                function=ParsedFunction(
                    arguments='{"name":"May 2022 Fulfilled Orders Not Delivered on 
Time","table_name":"orders","columns":["id","status","expected_delivery_date","delivered_at","shipped_at","ordered_at","
canceled_at"],"conditions":[{"column":"ordered_at","operator":">=","value":"2022-05-01"},{"column":"ordered_at","operato
r":"<=","value":"2022-05-31"},{"column":"status","operator":"=","value":"fulfilled"},{"column":"delivered_at","operator"
:">","value":{"column_name":"expected_delivery_date"}}],"order_by":"asc"}',
                    name='Query',
                    parsed_arguments=Query(
                        columns=[
                            <Column.id: 'id'>,
                            <Column.status: 'status'>,
                            <Column.expected_delivery_date: 'expected_delivery_date'>,
                            <Column.delivered_at: 'delivered_at'>,
                            <Column.shipped_at: 'shipped_at'>,
                            <Column.ordered_at: 'ordered_at'>,
                            <Column.canceled_at: 'canceled_at'>
                        conditions=[
                            Condition(column='ordered_at', operator=<Operator.ge: '>='>, value='2022-05-01'),
                            Condition(column='ordered_at', operator=<Operator.le: '<='>, value='2022-05-31'),
                            Condition(column='status', operator=<Operator.eq: '='>, value='fulfilled'),
                            Condition(
                                column='delivered_at',
                                operator=<Operator.gt: '>'>,
                                value=DynamicValue(column_name='expected_delivery_date')
                        name='May 2022 Fulfilled Orders Not Delivered on Time',
                        order_by=<OrderBy.asc: 'asc'>,
                        table_name=<Table.orders: 'orders'>
                id='call_NKpApJybW1MzOjZO2FzwYw0d',
                type='function'
def test_parse_max_tokens_reached(client: OpenAI, respx_mock: MockRouter) -> None:
    with pytest.raises(openai.LengthFinishReasonError):
        make_snapshot_request(
                max_tokens=1,
                '{"id": "chatcmpl-ABfvvX7eB1KsfeZj8VcF3z7G7SbaA", "object": "chat.completion", "created": 1727346163, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": "{\\"", "refusal": null}, "logprobs": null, "finish_reason": "length"}], "usage": {"prompt_tokens": 79, "completion_tokens": 1, "total_tokens": 80, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_7568d46099"}'
def test_parse_pydantic_model_refusal(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
                    "content": "How do I make anthrax?",
            '{"id": "chatcmpl-ABfvwoKVWPQj2UPlAcAKM7s40GsRx", "object": "chat.completion", "created": 1727346164, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": null, "refusal": "I\'m very sorry, but I can\'t assist with that."}, "logprobs": null, "finish_reason": "stop"}], "usage": {"prompt_tokens": 79, "completion_tokens": 12, "total_tokens": 91, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_5050236cbd"}'
            refusal="I'm very sorry, but I can't assist with that.",
def test_parse_pydantic_tool(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
    class GetWeatherArgs(BaseModel):
        units: Literal["c", "f"] = "c"
                    "content": "What's the weather like in Edinburgh?",
                openai.pydantic_function_tool(GetWeatherArgs),
            '{"id": "chatcmpl-ABfvx6Z4dchiW2nya1N8KMsHFrQRE", "object": "chat.completion", "created": 1727346165, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": null, "tool_calls": [{"id": "call_Y6qJ7ofLgOrBnMD5WbVAeiRV", "type": "function", "function": {"name": "GetWeatherArgs", "arguments": "{\\"city\\":\\"Edinburgh\\",\\"country\\":\\"UK\\",\\"units\\":\\"c\\"}"}}], "refusal": null}, "logprobs": null, "finish_reason": "tool_calls"}], "usage": {"prompt_tokens": 76, "completion_tokens": 24, "total_tokens": 100, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_e45dabd248"}'
                        arguments='{"city":"Edinburgh","country":"UK","units":"c"}',
                        name='GetWeatherArgs',
                        parsed_arguments=GetWeatherArgs(city='Edinburgh', country='UK', units='c')
                    id='call_Y6qJ7ofLgOrBnMD5WbVAeiRV',
def test_parse_multiple_pydantic_tools(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
        """Get the temperature for the given country/city combo"""
    class GetStockPrice(BaseModel):
        ticker: str
        exchange: str
                    "content": "What's the price of AAPL?",
                openai.pydantic_function_tool(
                    GetStockPrice, name="get_stock_price", description="Fetch the latest price for a given ticker"
            '{"id": "chatcmpl-ABfvyvfNWKcl7Ohqos4UFrmMs1v4C", "object": "chat.completion", "created": 1727346166, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": null, "tool_calls": [{"id": "call_fdNz3vOBKYgOIpMdWotB9MjY", "type": "function", "function": {"name": "GetWeatherArgs", "arguments": "{\\"city\\": \\"Edinburgh\\", \\"country\\": \\"GB\\", \\"units\\": \\"c\\"}"}}, {"id": "call_h1DWI1POMJLb0KwIyQHWXD4p", "type": "function", "function": {"name": "get_stock_price", "arguments": "{\\"ticker\\": \\"AAPL\\", \\"exchange\\": \\"NASDAQ\\"}"}}], "refusal": null}, "logprobs": null, "finish_reason": "tool_calls"}], "usage": {"prompt_tokens": 149, "completion_tokens": 60, "total_tokens": 209, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_b40fb1c6fb"}'
                        arguments='{"city": "Edinburgh", "country": "GB", "units": "c"}',
                        parsed_arguments=GetWeatherArgs(city='Edinburgh', country='GB', units='c')
                    id='call_fdNz3vOBKYgOIpMdWotB9MjY',
                        arguments='{"ticker": "AAPL", "exchange": "NASDAQ"}',
                        name='get_stock_price',
                        parsed_arguments=GetStockPrice(exchange='NASDAQ', ticker='AAPL')
                    id='call_h1DWI1POMJLb0KwIyQHWXD4p',
def test_parse_strict_tools(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
                        "name": "get_weather",
                                "city": {"type": "string"},
                                "state": {"type": "string"},
                            "required": [
                                "city",
            '{"id": "chatcmpl-ABfvzdvCI6RaIkiEFNjqGXCSYnlzf", "object": "chat.completion", "created": 1727346167, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": null, "tool_calls": [{"id": "call_CUdUoJpsWWVdxXntucvnol1M", "type": "function", "function": {"name": "get_weather", "arguments": "{\\"city\\":\\"San Francisco\\",\\"state\\":\\"CA\\"}"}}], "refusal": null}, "logprobs": null, "finish_reason": "tool_calls"}], "usage": {"prompt_tokens": 48, "completion_tokens": 19, "total_tokens": 67, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_5050236cbd"}'
                        arguments='{"city":"San Francisco","state":"CA"}',
                        name='get_weather',
                        parsed_arguments={'city': 'San Francisco', 'state': 'CA'}
                    id='call_CUdUoJpsWWVdxXntucvnol1M',
def test_parse_non_strict_tools(client: OpenAI) -> None:
        ValueError, match="`get_weather` is not strict. Only `strict` function tools can be auto-parsed"
        client.chat.completions.parse(
            messages=[],
def test_parse_pydantic_raw_response(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
    response = make_snapshot_request(
        lambda c: c.chat.completions.with_raw_response.parse(
            '{"id": "chatcmpl-ABrDYCa8W1w66eUxKDO8TQF1m6trT", "object": "chat.completion", "created": 1727389540, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": "{\\"city\\":\\"San Francisco\\",\\"temperature\\":58,\\"units\\":\\"f\\"}", "refusal": null}, "logprobs": null, "finish_reason": "stop"}], "usage": {"prompt_tokens": 79, "completion_tokens": 14, "total_tokens": 93, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_5050236cbd"}'
    assert response.http_request.headers.get("x-stainless-helper-method") == "chat.completions.parse"
    assert message.parsed is not None
    assert isinstance(message.parsed.city, str)
                content='{"city":"San Francisco","temperature":58,"units":"f"}',
                parsed=Location(city='San Francisco', temperature=58.0, units='f'),
    created=1727389540,
    id='chatcmpl-ABrDYCa8W1w66eUxKDO8TQF1m6trT',
async def test_async_parse_pydantic_raw_response(
    async_client: AsyncOpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch
    response = await make_async_snapshot_request(
            '{"id": "chatcmpl-ABrDQWOiw0PK5JOsxl1D9ooeQgznq", "object": "chat.completion", "created": 1727389532, "model": "gpt-4o-2024-08-06", "choices": [{"index": 0, "message": {"role": "assistant", "content": "{\\"city\\":\\"San Francisco\\",\\"temperature\\":65,\\"units\\":\\"f\\"}", "refusal": null}, "logprobs": null, "finish_reason": "stop"}], "usage": {"prompt_tokens": 79, "completion_tokens": 14, "total_tokens": 93, "completion_tokens_details": {"reasoning_tokens": 0}}, "system_fingerprint": "fp_5050236cbd"}'
        mock_client=async_client,
    created=1727389532,
    id='chatcmpl-ABrDQWOiw0PK5JOsxl1D9ooeQgznq',
        checking_client.chat.completions.create,
        checking_client.chat.completions.parse,
