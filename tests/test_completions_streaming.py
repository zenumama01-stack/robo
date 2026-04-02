from typing import Any, Generic, Callable, Iterator, cast, overload
from inline_snapshot import (
    external,
    snapshot,
    outsource,  # pyright: ignore[reportUnknownVariableType]
    get_snapshot_value,
from openai._utils import consume_sync_iterator, assert_signatures_in_sync
from openai._compat import model_copy
from openai.types.chat import ChatCompletionChunk
from openai.lib.streaming.chat import (
    ChatCompletionStream,
    ChatCompletionStreamState,
    ChatCompletionStreamManager,
from openai.lib._parsing._completions import ResponseFormatT
    listener = _make_stream_snapshot_request(
        lambda c: c.chat.completions.stream(
        content_snapshot=snapshot(external("e2aad469b71d*.bin")),
    assert print_obj(listener.stream.get_final_completion().choices, monkeypatch) == snapshot(
recommend checking a reliable weather website or a weather app.",
    assert print_obj(listener.get_event_by_type("content.done"), monkeypatch) == snapshot(
ContentDoneEvent(
    content="I'm unable to provide real-time weather updates. To get the current weather in San Francisco, I recommend 
checking a reliable weather website or a weather app.",
    type='content.done'
    done_snapshots: list[ParsedChatCompletionSnapshot] = []
    def on_event(stream: ChatCompletionStream[Location], event: ChatCompletionStreamEvent[Location]) -> None:
        if event.type == "content.done":
            done_snapshots.append(model_copy(stream.current_completion_snapshot, deep=True))
        content_snapshot=snapshot(external("7e5ea4d12e7c*.bin")),
        on_event=on_event,
    assert len(done_snapshots) == 1
    assert isinstance(done_snapshots[0].choices[0].message.parsed, Location)
    for event in reversed(listener.events):
            data = cast(Any, event.parsed)
            assert isinstance(data["city"], str), data
            assert isinstance(data["temperature"], (int, float)), data
            assert isinstance(data["units"], str), data
        rich.print(listener.events)
        raise AssertionError("Did not find a `content.delta` event")
    assert print_obj(listener.stream.get_final_completion(), monkeypatch) == snapshot(
                content='{"city":"San Francisco","temperature":61,"units":"f"}',
                parsed=Location(city='San Francisco', temperature=61.0, units='f'),
    created=1727346169,
    id='chatcmpl-ABfw1e5abtU8OwGr15vOreYVb2MiF',
        content_snapshot=snapshot(external("a491adda08c3*.bin")),
    assert [e.type for e in listener.events] == snapshot(
            "chunk",
            "content.delta",
            "content.done",
            content='{"city":"San Francisco","temperature":59,"units":"f"}',
            parsed=Location(city='San Francisco', temperature=59.0, units='f'),
        _make_stream_snapshot_request(
            content_snapshot=snapshot(external("4cc50a6135d2*.bin")),
        content_snapshot=snapshot(external("173417d55340*.bin")),
    assert print_obj(listener.get_event_by_type("refusal.done"), monkeypatch) == snapshot("""\
RefusalDoneEvent(refusal="I'm sorry, I can't assist with that request.", type='refusal.done')
            refusal="I'm sorry, I can't assist with that request.",
def test_content_logprobs_events(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
                    "content": "Say foo",
        content_snapshot=snapshot(external("83b060bae42e*.bin")),
    assert print_obj([e for e in listener.events if e.type.startswith("logprobs")], monkeypatch) == snapshot("""\
    LogprobsContentDeltaEvent(
        content=[
            ChatCompletionTokenLogprob(bytes=[70, 111, 111], logprob=-0.0025094282, token='Foo', top_logprobs=[])
        snapshot=[
        type='logprobs.content.delta'
        content=[ChatCompletionTokenLogprob(bytes=[33], logprob=-0.26638845, token='!', top_logprobs=[])],
            ChatCompletionTokenLogprob(bytes=[70, 111, 111], logprob=-0.0025094282, token='Foo', top_logprobs=[]),
            ChatCompletionTokenLogprob(bytes=[33], logprob=-0.26638845, token='!', top_logprobs=[])
    LogprobsContentDoneEvent(
        type='logprobs.content.done'
    assert print_obj(listener.stream.get_final_completion().choices, monkeypatch) == snapshot("""\
        logprobs=ChoiceLogprobs(
            refusal=None
            content='Foo!',
def test_refusal_logprobs_events(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
        content_snapshot=snapshot(external("569c877e6942*.bin")),
    assert print_obj([e.type for e in listener.events if e.type.startswith("logprobs")], monkeypatch) == snapshot("""\
    'logprobs.refusal.delta',
    'logprobs.refusal.done'
            refusal=[
                ChatCompletionTokenLogprob(bytes=[73, 39, 109], logprob=-0.0012038043, token="I'm", top_logprobs=[]),
                ChatCompletionTokenLogprob(
                    bytes=[32, 118, 101, 114, 121],
                    logprob=-0.8438816,
                    token=' very',
                    top_logprobs=[]
                    bytes=[32, 115, 111, 114, 114, 121],
                    logprob=-3.4121115e-06,
                    token=' sorry',
                ChatCompletionTokenLogprob(bytes=[44], logprob=-3.3809047e-05, token=',', top_logprobs=[]),
                    bytes=[32, 98, 117, 116],
                    logprob=-0.038048144,
                    token=' but',
                ChatCompletionTokenLogprob(bytes=[32, 73], logprob=-0.0016109125, token=' I', top_logprobs=[]),
                    bytes=[32, 99, 97, 110, 39, 116],
                    logprob=-0.0073532974,
                    token=" can't",
                    bytes=[32, 97, 115, 115, 105, 115, 116],
                    logprob=-0.0020837625,
                    token=' assist',
                    bytes=[32, 119, 105, 116, 104],
                    logprob=-0.00318354,
                    token=' with',
                    bytes=[32, 116, 104, 97, 116],
                    logprob=-0.0017186158,
                    token=' that',
                ChatCompletionTokenLogprob(bytes=[46], logprob=-0.57687104, token='.', top_logprobs=[])
        content_snapshot=snapshot(external("c6aa7e397b71*.bin")),
    assert print_obj(listener.stream.current_completion_snapshot.choices, monkeypatch) == snapshot(
                    id='call_c91SqDXlYFuETYv8mUHzz6pp',
        content_snapshot=snapshot(external("f82268f2fefd*.bin")),
                    id='call_JMW1whyEaYG438VE1OIflxA2',
                    id='call_DNYTawLBoN8fj3KN6qU9N1Ou',
    completion = listener.stream.get_final_completion()
    assert print_obj(completion.choices[0].message.tool_calls, monkeypatch) == snapshot(
        content_snapshot=snapshot(external("a247c49c5fcd*.bin")),
                    id='call_CTf1nWJLqSeRgDqaCG27xZ74',
def test_non_pydantic_response_format(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
                    "content": "What's the weather like in SF? Give me any JSON back",
            response_format={"type": "json_object"},
        content_snapshot=snapshot(external("d61558011839*.bin")),
            content='\\n  {\\n    "location": "San Francisco, CA",\\n    "weather": {\\n      "temperature": "18°C",\\n      
"condition": "Partly Cloudy",\\n      "humidity": "72%",\\n      "windSpeed": "15 km/h",\\n      "windDirection": "NW"\\n   
},\\n    "forecast": [\\n      {\\n        "day": "Monday",\\n        "high": "20°C",\\n        "low": "14°C",\\n        
"condition": "Sunny"\\n      },\\n      {\\n        "day": "Tuesday",\\n        "high": "19°C",\\n        "low": "15°C",\\n   
"condition": "Mostly Cloudy"\\n      },\\n      {\\n        "day": "Wednesday",\\n        "high": "18°C",\\n        "low": 
"14°C",\\n        "condition": "Cloudy"\\n      }\\n    ]\\n  }\\n',
def test_allows_non_strict_tools_but_no_parsing(
            messages=[{"role": "user", "content": "what's the weather in NYC?"}],
                        "parameters": {"type": "object", "properties": {"city": {"type": "string"}}},
        content_snapshot=snapshot(external("2018feb66ae1*.bin")),
    assert print_obj(listener.get_event_by_type("tool_calls.function.arguments.done"), monkeypatch) == snapshot("""\
FunctionToolCallArgumentsDoneEvent(
    arguments='{"city":"New York City"}',
    parsed_arguments=None,
    type='tool_calls.function.arguments.done'
                        parsed_arguments=None
                    id='call_4XzlGBLtUe9dy3GVNV4jhq7h',
def test_chat_completion_state_helper(client: OpenAI, respx_mock: MockRouter, monkeypatch: pytest.MonkeyPatch) -> None:
    def streamer(client: OpenAI) -> Iterator[ChatCompletionChunk]:
    _make_raw_stream_snapshot_request(
    assert print_obj(state.get_final_completion().choices, monkeypatch) == snapshot(
def test_stream_method_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
        checking_client.chat.completions.stream,
class StreamListener(Generic[ResponseFormatT]):
    def __init__(self, stream: ChatCompletionStream[ResponseFormatT]) -> None:
        self.stream = stream
        self.events: list[ChatCompletionStreamEvent[ResponseFormatT]] = []
        for event in self.stream:
            self.events.append(event)
    def get_event_by_type(self, event_type: Literal["content.done"]) -> ContentDoneEvent[ResponseFormatT] | None: ...
    def get_event_by_type(self, event_type: str) -> ChatCompletionStreamEvent[ResponseFormatT] | None: ...
    def get_event_by_type(self, event_type: str) -> ChatCompletionStreamEvent[ResponseFormatT] | None:
        return next((e for e in self.events if e.type == event_type), None)
def _make_stream_snapshot_request(
    func: Callable[[OpenAI], ChatCompletionStreamManager[ResponseFormatT]],
    on_event: Callable[[ChatCompletionStream[ResponseFormatT], ChatCompletionStreamEvent[ResponseFormatT]], Any]
    | None = None,
) -> StreamListener[ResponseFormatT]:
            assert outsource(response.read()) == content_snapshot
        respx_mock.post("/chat/completions").mock(
                headers={"content-type": "text/event-stream"},
    with func(client) as stream:
        listener = StreamListener(stream)
        for event in listener:
            if on_event:
                on_event(stream, event)
    return listener
def _make_raw_stream_snapshot_request(
    func: Callable[[OpenAI], Iterator[ChatCompletionChunk]],
    stream = func(client)
    consume_sync_iterator(stream)
