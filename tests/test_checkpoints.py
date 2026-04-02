from openai.types.fine_tuning.jobs import FineTuningJobCheckpoint
class TestCheckpoints:
        checkpoint = client.fine_tuning.jobs.checkpoints.list(
        assert_matches_type(SyncCursorPage[FineTuningJobCheckpoint], checkpoint, path=["response"])
        response = client.fine_tuning.jobs.checkpoints.with_raw_response.list(
        checkpoint = response.parse()
        with client.fine_tuning.jobs.checkpoints.with_streaming_response.list(
            client.fine_tuning.jobs.checkpoints.with_raw_response.list(
class TestAsyncCheckpoints:
        checkpoint = await async_client.fine_tuning.jobs.checkpoints.list(
        assert_matches_type(AsyncCursorPage[FineTuningJobCheckpoint], checkpoint, path=["response"])
        response = await async_client.fine_tuning.jobs.checkpoints.with_raw_response.list(
        async with async_client.fine_tuning.jobs.checkpoints.with_streaming_response.list(
            checkpoint = await response.parse()
            await async_client.fine_tuning.jobs.checkpoints.with_raw_response.list(
