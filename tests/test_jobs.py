from openai.types.fine_tuning import (
class TestJobs:
        job = client.fine_tuning.jobs.create(
            model="gpt-4o-mini",
            training_file="file-abc123",
        assert_matches_type(FineTuningJob, job, path=["response"])
            hyperparameters={
                "batch_size": "auto",
                "learning_rate_multiplier": "auto",
                "n_epochs": "auto",
            integrations=[
                    "type": "wandb",
                    "wandb": {
                        "project": "my-wandb-project",
                        "entity": "entity",
                        "tags": ["custom-tag"],
            method={
                "type": "supervised",
                "dpo": {
                    "hyperparameters": {
                        "beta": "auto",
                "reinforcement": {
                    "grader": {
                        "input": "input",
                        "operation": "eq",
                        "reference": "reference",
                        "type": "string_check",
                        "compute_multiplier": "auto",
                        "eval_interval": "auto",
                        "eval_samples": "auto",
                        "reasoning_effort": "default",
                "supervised": {
            seed=42,
            suffix="x",
            validation_file="file-abc123",
        response = client.fine_tuning.jobs.with_raw_response.create(
        job = response.parse()
        with client.fine_tuning.jobs.with_streaming_response.create(
        job = client.fine_tuning.jobs.retrieve(
            "ft-AF1WoRqd3aJAHsqc9NY7iL8F",
        response = client.fine_tuning.jobs.with_raw_response.retrieve(
        with client.fine_tuning.jobs.with_streaming_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `fine_tuning_job_id` but received ''"):
            client.fine_tuning.jobs.with_raw_response.retrieve(
        job = client.fine_tuning.jobs.list()
        assert_matches_type(SyncCursorPage[FineTuningJob], job, path=["response"])
        job = client.fine_tuning.jobs.list(
        response = client.fine_tuning.jobs.with_raw_response.list()
        with client.fine_tuning.jobs.with_streaming_response.list() as response:
        job = client.fine_tuning.jobs.cancel(
        response = client.fine_tuning.jobs.with_raw_response.cancel(
        with client.fine_tuning.jobs.with_streaming_response.cancel(
            client.fine_tuning.jobs.with_raw_response.cancel(
    def test_method_list_events(self, client: OpenAI) -> None:
        job = client.fine_tuning.jobs.list_events(
        assert_matches_type(SyncCursorPage[FineTuningJobEvent], job, path=["response"])
    def test_method_list_events_with_all_params(self, client: OpenAI) -> None:
    def test_raw_response_list_events(self, client: OpenAI) -> None:
        response = client.fine_tuning.jobs.with_raw_response.list_events(
    def test_streaming_response_list_events(self, client: OpenAI) -> None:
        with client.fine_tuning.jobs.with_streaming_response.list_events(
    def test_path_params_list_events(self, client: OpenAI) -> None:
            client.fine_tuning.jobs.with_raw_response.list_events(
    def test_method_pause(self, client: OpenAI) -> None:
        job = client.fine_tuning.jobs.pause(
    def test_raw_response_pause(self, client: OpenAI) -> None:
        response = client.fine_tuning.jobs.with_raw_response.pause(
    def test_streaming_response_pause(self, client: OpenAI) -> None:
        with client.fine_tuning.jobs.with_streaming_response.pause(
    def test_path_params_pause(self, client: OpenAI) -> None:
            client.fine_tuning.jobs.with_raw_response.pause(
    def test_method_resume(self, client: OpenAI) -> None:
        job = client.fine_tuning.jobs.resume(
    def test_raw_response_resume(self, client: OpenAI) -> None:
        response = client.fine_tuning.jobs.with_raw_response.resume(
    def test_streaming_response_resume(self, client: OpenAI) -> None:
        with client.fine_tuning.jobs.with_streaming_response.resume(
    def test_path_params_resume(self, client: OpenAI) -> None:
            client.fine_tuning.jobs.with_raw_response.resume(
class TestAsyncJobs:
        job = await async_client.fine_tuning.jobs.create(
        response = await async_client.fine_tuning.jobs.with_raw_response.create(
        async with async_client.fine_tuning.jobs.with_streaming_response.create(
            job = await response.parse()
        job = await async_client.fine_tuning.jobs.retrieve(
        response = await async_client.fine_tuning.jobs.with_raw_response.retrieve(
        async with async_client.fine_tuning.jobs.with_streaming_response.retrieve(
            await async_client.fine_tuning.jobs.with_raw_response.retrieve(
        job = await async_client.fine_tuning.jobs.list()
        assert_matches_type(AsyncCursorPage[FineTuningJob], job, path=["response"])
        job = await async_client.fine_tuning.jobs.list(
        response = await async_client.fine_tuning.jobs.with_raw_response.list()
        async with async_client.fine_tuning.jobs.with_streaming_response.list() as response:
        job = await async_client.fine_tuning.jobs.cancel(
        response = await async_client.fine_tuning.jobs.with_raw_response.cancel(
        async with async_client.fine_tuning.jobs.with_streaming_response.cancel(
            await async_client.fine_tuning.jobs.with_raw_response.cancel(
    async def test_method_list_events(self, async_client: AsyncOpenAI) -> None:
        job = await async_client.fine_tuning.jobs.list_events(
        assert_matches_type(AsyncCursorPage[FineTuningJobEvent], job, path=["response"])
    async def test_method_list_events_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_list_events(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.fine_tuning.jobs.with_raw_response.list_events(
    async def test_streaming_response_list_events(self, async_client: AsyncOpenAI) -> None:
        async with async_client.fine_tuning.jobs.with_streaming_response.list_events(
    async def test_path_params_list_events(self, async_client: AsyncOpenAI) -> None:
            await async_client.fine_tuning.jobs.with_raw_response.list_events(
    async def test_method_pause(self, async_client: AsyncOpenAI) -> None:
        job = await async_client.fine_tuning.jobs.pause(
    async def test_raw_response_pause(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.fine_tuning.jobs.with_raw_response.pause(
    async def test_streaming_response_pause(self, async_client: AsyncOpenAI) -> None:
        async with async_client.fine_tuning.jobs.with_streaming_response.pause(
    async def test_path_params_pause(self, async_client: AsyncOpenAI) -> None:
            await async_client.fine_tuning.jobs.with_raw_response.pause(
    async def test_method_resume(self, async_client: AsyncOpenAI) -> None:
        job = await async_client.fine_tuning.jobs.resume(
    async def test_raw_response_resume(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.fine_tuning.jobs.with_raw_response.resume(
    async def test_streaming_response_resume(self, async_client: AsyncOpenAI) -> None:
        async with async_client.fine_tuning.jobs.with_streaming_response.resume(
    async def test_path_params_resume(self, async_client: AsyncOpenAI) -> None:
            await async_client.fine_tuning.jobs.with_raw_response.resume(
