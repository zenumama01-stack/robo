from openai.types.responses import ResponseItem
class TestInputItems:
        input_item = client.responses.input_items.list(
            response_id="response_id",
        assert_matches_type(SyncCursorPage[ResponseItem], input_item, path=["response"])
        response = client.responses.input_items.with_raw_response.list(
        input_item = response.parse()
        with client.responses.input_items.with_streaming_response.list(
            client.responses.input_items.with_raw_response.list(
class TestAsyncInputItems:
        input_item = await async_client.responses.input_items.list(
        assert_matches_type(AsyncCursorPage[ResponseItem], input_item, path=["response"])
        response = await async_client.responses.input_items.with_raw_response.list(
        async with async_client.responses.input_items.with_streaming_response.list(
            input_item = await response.parse()
            await async_client.responses.input_items.with_raw_response.list(
