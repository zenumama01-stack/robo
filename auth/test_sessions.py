from openai.types.beta.chatkit import (
    ChatSession,
class TestSessions:
        session = client.beta.chatkit.sessions.create(
            user="x",
            workflow={"id": "id"},
        assert_matches_type(ChatSession, session, path=["response"])
            workflow={
                "state_variables": {"foo": "string"},
                "tracing": {"enabled": True},
            chatkit_configuration={
                "automatic_thread_titling": {"enabled": True},
                "file_upload": {
                    "enabled": True,
                    "max_file_size": 1,
                    "max_files": 1,
                "history": {
                    "recent_threads": 1,
                "seconds": 1,
            rate_limits={"max_requests_per_1_minute": 1},
        response = client.beta.chatkit.sessions.with_raw_response.create(
        session = response.parse()
        with client.beta.chatkit.sessions.with_streaming_response.create(
        session = client.beta.chatkit.sessions.cancel(
            "cksess_123",
        response = client.beta.chatkit.sessions.with_raw_response.cancel(
        with client.beta.chatkit.sessions.with_streaming_response.cancel(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `session_id` but received ''"):
            client.beta.chatkit.sessions.with_raw_response.cancel(
class TestAsyncSessions:
        session = await async_client.beta.chatkit.sessions.create(
        response = await async_client.beta.chatkit.sessions.with_raw_response.create(
        async with async_client.beta.chatkit.sessions.with_streaming_response.create(
            session = await response.parse()
        session = await async_client.beta.chatkit.sessions.cancel(
        response = await async_client.beta.chatkit.sessions.with_raw_response.cancel(
        async with async_client.beta.chatkit.sessions.with_streaming_response.cancel(
            await async_client.beta.chatkit.sessions.with_raw_response.cancel(
