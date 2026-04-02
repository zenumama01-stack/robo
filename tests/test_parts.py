from openai.types.uploads import UploadPart
class TestParts:
        part = client.uploads.parts.create(
            data=b"Example data",
        assert_matches_type(UploadPart, part, path=["response"])
        response = client.uploads.parts.with_raw_response.create(
        part = response.parse()
        with client.uploads.parts.with_streaming_response.create(
            client.uploads.parts.with_raw_response.create(
class TestAsyncParts:
        part = await async_client.uploads.parts.create(
        response = await async_client.uploads.parts.with_raw_response.create(
        async with async_client.uploads.parts.with_streaming_response.create(
            part = await response.parse()
            await async_client.uploads.parts.with_raw_response.create(
