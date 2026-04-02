class TestCalls:
        respx_mock.post("/realtime/calls").mock(return_value=httpx.Response(200, json={"foo": "bar"}))
        call = client.realtime.calls.create(
            sdp="sdp",
        assert isinstance(call, _legacy_response.HttpxBinaryResponseContent)
        assert call.json() == {"foo": "bar"}
                    "input": {
                        "format": {
                            "rate": 24000,
                            "type": "audio/pcm",
                        "noise_reduction": {"type": "near_field"},
                        "transcription": {
                            "language": "language",
                            "model": "string",
                            "prompt": "prompt",
                        "turn_detection": {
                            "type": "server_vad",
                            "create_response": True,
                            "idle_timeout_ms": 5000,
                            "interrupt_response": True,
                            "prefix_padding_ms": 0,
                            "silence_duration_ms": 0,
                            "threshold": 0,
                    "output": {
                        "speed": 0.25,
                "include": ["item.input_audio_transcription.logprobs"],
                "instructions": "instructions",
                "max_output_tokens": 0,
                "prompt": {
                "tool_choice": "none",
                "tools": [
                        "parameters": {},
                "tracing": "auto",
                "truncation": "auto",
        response = client.realtime.calls.with_raw_response.create(
        call = response.parse()
        assert_matches_type(_legacy_response.HttpxBinaryResponseContent, call, path=["response"])
        with client.realtime.calls.with_streaming_response.create(
            assert_matches_type(bytes, call, path=["response"])
    def test_method_accept(self, client: OpenAI) -> None:
        call = client.realtime.calls.accept(
            call_id="call_id",
            type="realtime",
        assert call is None
    def test_method_accept_with_all_params(self, client: OpenAI) -> None:
            include=["item.input_audio_transcription.logprobs"],
            output_modalities=["text"],
            tracing="auto",
    def test_raw_response_accept(self, client: OpenAI) -> None:
        response = client.realtime.calls.with_raw_response.accept(
    def test_streaming_response_accept(self, client: OpenAI) -> None:
        with client.realtime.calls.with_streaming_response.accept(
    def test_path_params_accept(self, client: OpenAI) -> None:
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `call_id` but received ''"):
            client.realtime.calls.with_raw_response.accept(
                call_id="",
    def test_method_hangup(self, client: OpenAI) -> None:
        call = client.realtime.calls.hangup(
            "call_id",
    def test_raw_response_hangup(self, client: OpenAI) -> None:
        response = client.realtime.calls.with_raw_response.hangup(
    def test_streaming_response_hangup(self, client: OpenAI) -> None:
        with client.realtime.calls.with_streaming_response.hangup(
    def test_path_params_hangup(self, client: OpenAI) -> None:
            client.realtime.calls.with_raw_response.hangup(
    def test_method_refer(self, client: OpenAI) -> None:
        call = client.realtime.calls.refer(
            target_uri="tel:+14155550123",
    def test_raw_response_refer(self, client: OpenAI) -> None:
        response = client.realtime.calls.with_raw_response.refer(
    def test_streaming_response_refer(self, client: OpenAI) -> None:
        with client.realtime.calls.with_streaming_response.refer(
    def test_path_params_refer(self, client: OpenAI) -> None:
            client.realtime.calls.with_raw_response.refer(
    def test_method_reject(self, client: OpenAI) -> None:
        call = client.realtime.calls.reject(
    def test_method_reject_with_all_params(self, client: OpenAI) -> None:
            status_code=486,
    def test_raw_response_reject(self, client: OpenAI) -> None:
        response = client.realtime.calls.with_raw_response.reject(
    def test_streaming_response_reject(self, client: OpenAI) -> None:
        with client.realtime.calls.with_streaming_response.reject(
    def test_path_params_reject(self, client: OpenAI) -> None:
            client.realtime.calls.with_raw_response.reject(
class TestAsyncCalls:
        call = await async_client.realtime.calls.create(
        response = await async_client.realtime.calls.with_raw_response.create(
        async with async_client.realtime.calls.with_streaming_response.create(
            call = await response.parse()
    async def test_method_accept(self, async_client: AsyncOpenAI) -> None:
        call = await async_client.realtime.calls.accept(
    async def test_method_accept_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_accept(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.realtime.calls.with_raw_response.accept(
    async def test_streaming_response_accept(self, async_client: AsyncOpenAI) -> None:
        async with async_client.realtime.calls.with_streaming_response.accept(
    async def test_path_params_accept(self, async_client: AsyncOpenAI) -> None:
            await async_client.realtime.calls.with_raw_response.accept(
    async def test_method_hangup(self, async_client: AsyncOpenAI) -> None:
        call = await async_client.realtime.calls.hangup(
    async def test_raw_response_hangup(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.realtime.calls.with_raw_response.hangup(
    async def test_streaming_response_hangup(self, async_client: AsyncOpenAI) -> None:
        async with async_client.realtime.calls.with_streaming_response.hangup(
    async def test_path_params_hangup(self, async_client: AsyncOpenAI) -> None:
            await async_client.realtime.calls.with_raw_response.hangup(
    async def test_method_refer(self, async_client: AsyncOpenAI) -> None:
        call = await async_client.realtime.calls.refer(
    async def test_raw_response_refer(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.realtime.calls.with_raw_response.refer(
    async def test_streaming_response_refer(self, async_client: AsyncOpenAI) -> None:
        async with async_client.realtime.calls.with_streaming_response.refer(
    async def test_path_params_refer(self, async_client: AsyncOpenAI) -> None:
            await async_client.realtime.calls.with_raw_response.refer(
    async def test_method_reject(self, async_client: AsyncOpenAI) -> None:
        call = await async_client.realtime.calls.reject(
    async def test_method_reject_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_reject(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.realtime.calls.with_raw_response.reject(
    async def test_streaming_response_reject(self, async_client: AsyncOpenAI) -> None:
        async with async_client.realtime.calls.with_streaming_response.reject(
    async def test_path_params_reject(self, async_client: AsyncOpenAI) -> None:
            await async_client.realtime.calls.with_raw_response.reject(
