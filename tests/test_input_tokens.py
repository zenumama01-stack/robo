from openai.types.responses import InputTokenCountResponse
class TestInputTokens:
    def test_method_count(self, client: OpenAI) -> None:
        input_token = client.responses.input_tokens.count()
        assert_matches_type(InputTokenCountResponse, input_token, path=["response"])
    def test_method_count_with_all_params(self, client: OpenAI) -> None:
        input_token = client.responses.input_tokens.count(
    def test_raw_response_count(self, client: OpenAI) -> None:
        response = client.responses.input_tokens.with_raw_response.count()
        input_token = response.parse()
    def test_streaming_response_count(self, client: OpenAI) -> None:
        with client.responses.input_tokens.with_streaming_response.count() as response:
class TestAsyncInputTokens:
    async def test_method_count(self, async_client: AsyncOpenAI) -> None:
        input_token = await async_client.responses.input_tokens.count()
    async def test_method_count_with_all_params(self, async_client: AsyncOpenAI) -> None:
        input_token = await async_client.responses.input_tokens.count(
    async def test_raw_response_count(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.responses.input_tokens.with_raw_response.count()
    async def test_streaming_response_count(self, async_client: AsyncOpenAI) -> None:
        async with async_client.responses.input_tokens.with_streaming_response.count() as response:
            input_token = await response.parse()
