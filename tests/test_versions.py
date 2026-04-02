from openai.types.skills import SkillVersion, DeletedSkillVersion
class TestVersions:
        version = client.skills.versions.create(
        assert_matches_type(SkillVersion, version, path=["response"])
            default=True,
        response = client.skills.versions.with_raw_response.create(
        version = response.parse()
        with client.skills.versions.with_streaming_response.create(
            client.skills.versions.with_raw_response.create(
        version = client.skills.versions.retrieve(
            version="version",
        response = client.skills.versions.with_raw_response.retrieve(
        with client.skills.versions.with_streaming_response.retrieve(
            client.skills.versions.with_raw_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `version` but received ''"):
                version="",
        version = client.skills.versions.list(
        assert_matches_type(SyncCursorPage[SkillVersion], version, path=["response"])
            after="skillver_123",
        response = client.skills.versions.with_raw_response.list(
        with client.skills.versions.with_streaming_response.list(
            client.skills.versions.with_raw_response.list(
        version = client.skills.versions.delete(
        assert_matches_type(DeletedSkillVersion, version, path=["response"])
        response = client.skills.versions.with_raw_response.delete(
        with client.skills.versions.with_streaming_response.delete(
            client.skills.versions.with_raw_response.delete(
class TestAsyncVersions:
        version = await async_client.skills.versions.create(
        response = await async_client.skills.versions.with_raw_response.create(
        async with async_client.skills.versions.with_streaming_response.create(
            version = await response.parse()
            await async_client.skills.versions.with_raw_response.create(
        version = await async_client.skills.versions.retrieve(
        response = await async_client.skills.versions.with_raw_response.retrieve(
        async with async_client.skills.versions.with_streaming_response.retrieve(
            await async_client.skills.versions.with_raw_response.retrieve(
        version = await async_client.skills.versions.list(
        assert_matches_type(AsyncCursorPage[SkillVersion], version, path=["response"])
        response = await async_client.skills.versions.with_raw_response.list(
        async with async_client.skills.versions.with_streaming_response.list(
            await async_client.skills.versions.with_raw_response.list(
        version = await async_client.skills.versions.delete(
        response = await async_client.skills.versions.with_raw_response.delete(
        async with async_client.skills.versions.with_streaming_response.delete(
            await async_client.skills.versions.with_raw_response.delete(
