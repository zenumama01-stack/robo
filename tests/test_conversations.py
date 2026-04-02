from openai.types.conversations import (
    Conversation,
    ConversationDeletedResource,
class TestConversations:
        conversation = client.conversations.create()
        assert_matches_type(Conversation, conversation, path=["response"])
        conversation = client.conversations.create(
            items=[
                    "phase": "commentary",
        response = client.conversations.with_raw_response.create()
        conversation = response.parse()
        with client.conversations.with_streaming_response.create() as response:
        conversation = client.conversations.retrieve(
            "conv_123",
        response = client.conversations.with_raw_response.retrieve(
        with client.conversations.with_streaming_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `conversation_id` but received ''"):
            client.conversations.with_raw_response.retrieve(
    def test_method_update(self, client: OpenAI) -> None:
        conversation = client.conversations.update(
            conversation_id="conv_123",
    def test_raw_response_update(self, client: OpenAI) -> None:
        response = client.conversations.with_raw_response.update(
    def test_streaming_response_update(self, client: OpenAI) -> None:
        with client.conversations.with_streaming_response.update(
    def test_path_params_update(self, client: OpenAI) -> None:
            client.conversations.with_raw_response.update(
                conversation_id="",
        conversation = client.conversations.delete(
        assert_matches_type(ConversationDeletedResource, conversation, path=["response"])
        response = client.conversations.with_raw_response.delete(
        with client.conversations.with_streaming_response.delete(
            client.conversations.with_raw_response.delete(
class TestAsyncConversations:
        conversation = await async_client.conversations.create()
        conversation = await async_client.conversations.create(
        response = await async_client.conversations.with_raw_response.create()
        async with async_client.conversations.with_streaming_response.create() as response:
            conversation = await response.parse()
        conversation = await async_client.conversations.retrieve(
        response = await async_client.conversations.with_raw_response.retrieve(
        async with async_client.conversations.with_streaming_response.retrieve(
            await async_client.conversations.with_raw_response.retrieve(
    async def test_method_update(self, async_client: AsyncOpenAI) -> None:
        conversation = await async_client.conversations.update(
    async def test_raw_response_update(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.conversations.with_raw_response.update(
    async def test_streaming_response_update(self, async_client: AsyncOpenAI) -> None:
        async with async_client.conversations.with_streaming_response.update(
    async def test_path_params_update(self, async_client: AsyncOpenAI) -> None:
            await async_client.conversations.with_raw_response.update(
        conversation = await async_client.conversations.delete(
        response = await async_client.conversations.with_raw_response.delete(
        async with async_client.conversations.with_streaming_response.delete(
            await async_client.conversations.with_raw_response.delete(
