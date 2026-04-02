from openai.types.audio import TranslationCreateResponse
class TestTranslations:
        translation = client.audio.translations.create(
        assert_matches_type(TranslationCreateResponse, translation, path=["response"])
        response = client.audio.translations.with_raw_response.create(
        translation = response.parse()
        with client.audio.translations.with_streaming_response.create(
class TestAsyncTranslations:
        translation = await async_client.audio.translations.create(
        response = await async_client.audio.translations.with_raw_response.create(
        async with async_client.audio.translations.with_streaming_response.create(
            translation = await response.parse()
