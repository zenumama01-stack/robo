    EvalListResponse,
    EvalCreateResponse,
    EvalDeleteResponse,
    EvalUpdateResponse,
    EvalRetrieveResponse,
class TestEvals:
        eval = client.evals.create(
            data_source_config={
                "item_schema": {"foo": "bar"},
                "type": "custom",
            testing_criteria=[
                    "input": [
                            "content": "content",
                            "role": "role",
                    "labels": ["string"],
                    "model": "model",
                    "name": "name",
                    "passing_labels": ["string"],
                    "type": "label_model",
        assert_matches_type(EvalCreateResponse, eval, path=["response"])
                "include_sample_schema": True,
        response = client.evals.with_raw_response.create(
        eval = response.parse()
        with client.evals.with_streaming_response.create(
        eval = client.evals.retrieve(
            "eval_id",
        assert_matches_type(EvalRetrieveResponse, eval, path=["response"])
        response = client.evals.with_raw_response.retrieve(
        with client.evals.with_streaming_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `eval_id` but received ''"):
            client.evals.with_raw_response.retrieve(
        eval = client.evals.update(
            eval_id="eval_id",
        assert_matches_type(EvalUpdateResponse, eval, path=["response"])
    def test_method_update_with_all_params(self, client: OpenAI) -> None:
        response = client.evals.with_raw_response.update(
        with client.evals.with_streaming_response.update(
            client.evals.with_raw_response.update(
                eval_id="",
        eval = client.evals.list()
        assert_matches_type(SyncCursorPage[EvalListResponse], eval, path=["response"])
        eval = client.evals.list(
            order_by="created_at",
        response = client.evals.with_raw_response.list()
        with client.evals.with_streaming_response.list() as response:
        eval = client.evals.delete(
        assert_matches_type(EvalDeleteResponse, eval, path=["response"])
        response = client.evals.with_raw_response.delete(
        with client.evals.with_streaming_response.delete(
            client.evals.with_raw_response.delete(
class TestAsyncEvals:
        eval = await async_client.evals.create(
        response = await async_client.evals.with_raw_response.create(
        async with async_client.evals.with_streaming_response.create(
            eval = await response.parse()
        eval = await async_client.evals.retrieve(
        response = await async_client.evals.with_raw_response.retrieve(
        async with async_client.evals.with_streaming_response.retrieve(
            await async_client.evals.with_raw_response.retrieve(
        eval = await async_client.evals.update(
    async def test_method_update_with_all_params(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.evals.with_raw_response.update(
        async with async_client.evals.with_streaming_response.update(
            await async_client.evals.with_raw_response.update(
        eval = await async_client.evals.list()
        assert_matches_type(AsyncCursorPage[EvalListResponse], eval, path=["response"])
        eval = await async_client.evals.list(
        response = await async_client.evals.with_raw_response.list()
        async with async_client.evals.with_streaming_response.list() as response:
        eval = await async_client.evals.delete(
        response = await async_client.evals.with_raw_response.delete(
        async with async_client.evals.with_streaming_response.delete(
            await async_client.evals.with_raw_response.delete(
