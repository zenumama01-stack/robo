    from ._client import OpenAI, AsyncOpenAI
class SyncAPIResource:
        self._get = client.get
        self._post = client.post
        self._patch = client.patch
        self._put = client.put
        self._delete = client.delete
        self._get_api_list = client.get_api_list
    def _sleep(self, seconds: float) -> None:
class AsyncAPIResource:
    async def _sleep(self, seconds: float) -> None:
        await anyio.sleep(seconds)
