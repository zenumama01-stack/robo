from openai.types.evals.runs import OutputItemListResponse, OutputItemRetrieveResponse
class TestOutputItems:
        output_item = client.evals.runs.output_items.retrieve(
            output_item_id="output_item_id",
        assert_matches_type(OutputItemRetrieveResponse, output_item, path=["response"])
        response = client.evals.runs.output_items.with_raw_response.retrieve(
        output_item = response.parse()
        with client.evals.runs.output_items.with_streaming_response.retrieve(
            client.evals.runs.output_items.with_raw_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `output_item_id` but received ''"):
                output_item_id="",
        output_item = client.evals.runs.output_items.list(
        assert_matches_type(SyncCursorPage[OutputItemListResponse], output_item, path=["response"])
            status="fail",
        response = client.evals.runs.output_items.with_raw_response.list(
        with client.evals.runs.output_items.with_streaming_response.list(
            client.evals.runs.output_items.with_raw_response.list(
class TestAsyncOutputItems:
        output_item = await async_client.evals.runs.output_items.retrieve(
        response = await async_client.evals.runs.output_items.with_raw_response.retrieve(
        async with async_client.evals.runs.output_items.with_streaming_response.retrieve(
            output_item = await response.parse()
            await async_client.evals.runs.output_items.with_raw_response.retrieve(
        output_item = await async_client.evals.runs.output_items.list(
        assert_matches_type(AsyncCursorPage[OutputItemListResponse], output_item, path=["response"])
        response = await async_client.evals.runs.output_items.with_raw_response.list(
        async with async_client.evals.runs.output_items.with_streaming_response.list(
            await async_client.evals.runs.output_items.with_raw_response.list(
