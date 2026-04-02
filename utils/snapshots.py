from typing import Any, Callable, Awaitable
from typing_extensions import TypeVar
from inline_snapshot import get_snapshot_value
def make_snapshot_request(
    func: Callable[[OpenAI], _T],
    content_snapshot: Any,
    mock_client: OpenAI,
    live = os.environ.get("OPENAI_LIVE") == "1"
    if live:
        def _on_response(response: httpx.Response) -> None:
            # update the content snapshot
            assert json.dumps(json.loads(response.read())) == content_snapshot
        respx_mock.stop()
            http_client=httpx.Client(
                event_hooks={
                    "response": [_on_response],
        respx_mock.post(path).mock(
                content=get_snapshot_value(content_snapshot),
                headers={"content-type": "application/json"},
        client = mock_client
    result = func(client)
async def make_async_snapshot_request(
    func: Callable[[AsyncOpenAI], Awaitable[_T]],
    mock_client: AsyncOpenAI,
        async def _on_response(response: httpx.Response) -> None:
            assert json.dumps(json.loads(await response.aread())) == content_snapshot
            http_client=httpx.AsyncClient(
    result = await func(client)
