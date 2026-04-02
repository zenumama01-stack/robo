from openai.types import Skill, DeletedSkill
class TestSkills:
        skill = client.skills.create()
        assert_matches_type(Skill, skill, path=["response"])
        skill = client.skills.create(
            files=[b"Example data"],
        response = client.skills.with_raw_response.create()
        skill = response.parse()
        with client.skills.with_streaming_response.create() as response:
        skill = client.skills.retrieve(
            "skill_123",
        response = client.skills.with_raw_response.retrieve(
        with client.skills.with_streaming_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `skill_id` but received ''"):
            client.skills.with_raw_response.retrieve(
        skill = client.skills.update(
            skill_id="skill_123",
            default_version="default_version",
        response = client.skills.with_raw_response.update(
        with client.skills.with_streaming_response.update(
            client.skills.with_raw_response.update(
                skill_id="",
        skill = client.skills.list()
        assert_matches_type(SyncCursorPage[Skill], skill, path=["response"])
        skill = client.skills.list(
        response = client.skills.with_raw_response.list()
        with client.skills.with_streaming_response.list() as response:
        skill = client.skills.delete(
        assert_matches_type(DeletedSkill, skill, path=["response"])
        response = client.skills.with_raw_response.delete(
        with client.skills.with_streaming_response.delete(
            client.skills.with_raw_response.delete(
class TestAsyncSkills:
        skill = await async_client.skills.create()
        skill = await async_client.skills.create(
        response = await async_client.skills.with_raw_response.create()
        async with async_client.skills.with_streaming_response.create() as response:
            skill = await response.parse()
        skill = await async_client.skills.retrieve(
        response = await async_client.skills.with_raw_response.retrieve(
        async with async_client.skills.with_streaming_response.retrieve(
            await async_client.skills.with_raw_response.retrieve(
        skill = await async_client.skills.update(
        response = await async_client.skills.with_raw_response.update(
        async with async_client.skills.with_streaming_response.update(
            await async_client.skills.with_raw_response.update(
        skill = await async_client.skills.list()
        assert_matches_type(AsyncCursorPage[Skill], skill, path=["response"])
        skill = await async_client.skills.list(
        response = await async_client.skills.with_raw_response.list()
        async with async_client.skills.with_streaming_response.list() as response:
        skill = await async_client.skills.delete(
        response = await async_client.skills.with_raw_response.delete(
        async with async_client.skills.with_streaming_response.delete(
            await async_client.skills.with_raw_response.delete(
