    ConversationItemList,
class TestItems:
        item = client.conversations.items.create(
        assert_matches_type(ConversationItemList, item, path=["response"])
        response = client.conversations.items.with_raw_response.create(
        item = response.parse()
        with client.conversations.items.with_streaming_response.create(
            client.conversations.items.with_raw_response.create(
        item = client.conversations.items.retrieve(
            item_id="msg_abc",
        assert_matches_type(ConversationItem, item, path=["response"])
        response = client.conversations.items.with_raw_response.retrieve(
        with client.conversations.items.with_streaming_response.retrieve(
            client.conversations.items.with_raw_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `item_id` but received ''"):
                item_id="",
        item = client.conversations.items.list(
        assert_matches_type(SyncConversationCursorPage[ConversationItem], item, path=["response"])
        response = client.conversations.items.with_raw_response.list(
        with client.conversations.items.with_streaming_response.list(
            client.conversations.items.with_raw_response.list(
        item = client.conversations.items.delete(
        assert_matches_type(Conversation, item, path=["response"])
        response = client.conversations.items.with_raw_response.delete(
        with client.conversations.items.with_streaming_response.delete(
            client.conversations.items.with_raw_response.delete(
class TestAsyncItems:
        item = await async_client.conversations.items.create(
        response = await async_client.conversations.items.with_raw_response.create(
        async with async_client.conversations.items.with_streaming_response.create(
            item = await response.parse()
            await async_client.conversations.items.with_raw_response.create(
        item = await async_client.conversations.items.retrieve(
        response = await async_client.conversations.items.with_raw_response.retrieve(
        async with async_client.conversations.items.with_streaming_response.retrieve(
            await async_client.conversations.items.with_raw_response.retrieve(
        item = await async_client.conversations.items.list(
        assert_matches_type(AsyncConversationCursorPage[ConversationItem], item, path=["response"])
        response = await async_client.conversations.items.with_raw_response.list(
        async with async_client.conversations.items.with_streaming_response.list(
            await async_client.conversations.items.with_raw_response.list(
        item = await async_client.conversations.items.delete(
        response = await async_client.conversations.items.with_raw_response.delete(
        async with async_client.conversations.items.with_streaming_response.delete(
            await async_client.conversations.items.with_raw_response.delete(
