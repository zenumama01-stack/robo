class TestSpeech:
    def test_method_create(self, client: OpenAI, respx_mock: MockRouter) -> None:
        respx_mock.post("/audio/speech").mock(return_value=httpx.Response(200, json={"foo": "bar"}))
        speech = client.audio.speech.create(
            voice="string",
        assert isinstance(speech, _legacy_response.HttpxBinaryResponseContent)
        assert speech.json() == {"foo": "bar"}
    def test_method_create_with_all_params(self, client: OpenAI, respx_mock: MockRouter) -> None:
            response_format="mp3",
            speed=0.25,
            stream_format="sse",
    def test_raw_response_create(self, client: OpenAI, respx_mock: MockRouter) -> None:
        response = client.audio.speech.with_raw_response.create(
        speech = response.parse()
        assert_matches_type(_legacy_response.HttpxBinaryResponseContent, speech, path=["response"])
    def test_streaming_response_create(self, client: OpenAI, respx_mock: MockRouter) -> None:
        with client.audio.speech.with_streaming_response.create(
            assert_matches_type(bytes, speech, path=["response"])
class TestAsyncSpeech:
    async def test_method_create(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        speech = await async_client.audio.speech.create(
    async def test_method_create_with_all_params(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
    async def test_raw_response_create(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        response = await async_client.audio.speech.with_raw_response.create(
    async def test_streaming_response_create(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        async with async_client.audio.speech.with_streaming_response.create(
            speech = await response.parse()
