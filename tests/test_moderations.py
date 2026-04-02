from openai.types import ModerationCreateResponse
class TestModerations:
        moderation = client.moderations.create(
            input="I want to kill them.",
        assert_matches_type(ModerationCreateResponse, moderation, path=["response"])
        response = client.moderations.with_raw_response.create(
        moderation = response.parse()
        with client.moderations.with_streaming_response.create(
class TestAsyncModerations:
        moderation = await async_client.moderations.create(
        response = await async_client.moderations.with_raw_response.create(
        async with async_client.moderations.with_streaming_response.create(
            moderation = await response.parse()
