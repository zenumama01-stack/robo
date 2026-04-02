from typing import TYPE_CHECKING, Union, Mapping, cast
from ..._types import Body, Omit, Query, Headers, NotGiven, FileTypes, omit, not_given
from ..._utils import extract_files, maybe_transform, deepcopy_minimal, async_maybe_transform
from ...types.audio import translation_create_params
from ...types.audio.translation import Translation
from ...types.audio.translation_verbose import TranslationVerbose
__all__ = ["Translations", "AsyncTranslations"]
class Translations(SyncAPIResource):
    def with_raw_response(self) -> TranslationsWithRawResponse:
        return TranslationsWithRawResponse(self)
    def with_streaming_response(self) -> TranslationsWithStreamingResponse:
        return TranslationsWithStreamingResponse(self)
    ) -> Translation: ...
    ) -> TranslationVerbose: ...
        response_format: Union[Literal["json", "text", "srt", "verbose_json", "vtt"], Omit] = omit,
    ) -> Translation | TranslationVerbose | str:
        Translates audio into English.
          file: The audio file object (not file name) translate, in one of these formats: flac,
              mp3, mp4, mpeg, mpga, m4a, ogg, wav, or webm.
          model: ID of the model to use. Only `whisper-1` (which is powered by our open source
              Whisper V2 model) is currently available.
              should be in English.
              `verbose_json`, or `vtt`.
            body=maybe_transform(body, translation_create_params.TranslationCreateParams),
class AsyncTranslations(AsyncAPIResource):
    def with_raw_response(self) -> AsyncTranslationsWithRawResponse:
        return AsyncTranslationsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncTranslationsWithStreamingResponse:
        return AsyncTranslationsWithStreamingResponse(self)
            body=await async_maybe_transform(body, translation_create_params.TranslationCreateParams),
class TranslationsWithRawResponse:
    def __init__(self, translations: Translations) -> None:
        self._translations = translations
            translations.create,
class AsyncTranslationsWithRawResponse:
    def __init__(self, translations: AsyncTranslations) -> None:
class TranslationsWithStreamingResponse:
class AsyncTranslationsWithStreamingResponse:
) -> type[Translation | TranslationVerbose | str]:
        return Translation
        return TranslationVerbose
    elif TYPE_CHECKING and response_format != "diarized_json":  # type: ignore[unreachable]
        log.warning("Unexpected audio response format: %s", response_format)
