from openai.types import ImagesResponse
class TestImages:
    def test_method_create_variation(self, client: OpenAI) -> None:
        image = client.images.create_variation(
            image=b"Example data",
        assert_matches_type(ImagesResponse, image, path=["response"])
    def test_method_create_variation_with_all_params(self, client: OpenAI) -> None:
            response_format="url",
    def test_raw_response_create_variation(self, client: OpenAI) -> None:
        response = client.images.with_raw_response.create_variation(
        image = response.parse()
    def test_streaming_response_create_variation(self, client: OpenAI) -> None:
        with client.images.with_streaming_response.create_variation(
    def test_method_edit_overload_1(self, client: OpenAI) -> None:
        image = client.images.edit(
            prompt="A cute baby sea otter wearing a beret",
    def test_method_edit_with_all_params_overload_1(self, client: OpenAI) -> None:
            background="transparent",
            input_fidelity="high",
            mask=b"Example data",
            output_compression=100,
            output_format="png",
            partial_images=1,
            quality="high",
    def test_raw_response_edit_overload_1(self, client: OpenAI) -> None:
        response = client.images.with_raw_response.edit(
    def test_streaming_response_edit_overload_1(self, client: OpenAI) -> None:
        with client.images.with_streaming_response.edit(
    def test_method_edit_overload_2(self, client: OpenAI) -> None:
        image_stream = client.images.edit(
        image_stream.response.close()
    def test_method_edit_with_all_params_overload_2(self, client: OpenAI) -> None:
    def test_raw_response_edit_overload_2(self, client: OpenAI) -> None:
    def test_streaming_response_edit_overload_2(self, client: OpenAI) -> None:
    def test_method_generate_overload_1(self, client: OpenAI) -> None:
        image = client.images.generate(
    def test_method_generate_with_all_params_overload_1(self, client: OpenAI) -> None:
            moderation="low",
            quality="medium",
            style="vivid",
    def test_raw_response_generate_overload_1(self, client: OpenAI) -> None:
        response = client.images.with_raw_response.generate(
    def test_streaming_response_generate_overload_1(self, client: OpenAI) -> None:
        with client.images.with_streaming_response.generate(
    def test_method_generate_overload_2(self, client: OpenAI) -> None:
        image_stream = client.images.generate(
    def test_method_generate_with_all_params_overload_2(self, client: OpenAI) -> None:
    def test_raw_response_generate_overload_2(self, client: OpenAI) -> None:
    def test_streaming_response_generate_overload_2(self, client: OpenAI) -> None:
class TestAsyncImages:
    async def test_method_create_variation(self, async_client: AsyncOpenAI) -> None:
        image = await async_client.images.create_variation(
    async def test_method_create_variation_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_create_variation(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.images.with_raw_response.create_variation(
    async def test_streaming_response_create_variation(self, async_client: AsyncOpenAI) -> None:
        async with async_client.images.with_streaming_response.create_variation(
            image = await response.parse()
    async def test_method_edit_overload_1(self, async_client: AsyncOpenAI) -> None:
        image = await async_client.images.edit(
    async def test_method_edit_with_all_params_overload_1(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_edit_overload_1(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.images.with_raw_response.edit(
    async def test_streaming_response_edit_overload_1(self, async_client: AsyncOpenAI) -> None:
        async with async_client.images.with_streaming_response.edit(
    async def test_method_edit_overload_2(self, async_client: AsyncOpenAI) -> None:
        image_stream = await async_client.images.edit(
        await image_stream.response.aclose()
    async def test_method_edit_with_all_params_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_edit_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_streaming_response_edit_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_method_generate_overload_1(self, async_client: AsyncOpenAI) -> None:
        image = await async_client.images.generate(
    async def test_method_generate_with_all_params_overload_1(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_generate_overload_1(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.images.with_raw_response.generate(
    async def test_streaming_response_generate_overload_1(self, async_client: AsyncOpenAI) -> None:
        async with async_client.images.with_streaming_response.generate(
    async def test_method_generate_overload_2(self, async_client: AsyncOpenAI) -> None:
        image_stream = await async_client.images.generate(
    async def test_method_generate_with_all_params_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_generate_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_streaming_response_generate_overload_2(self, async_client: AsyncOpenAI) -> None:
