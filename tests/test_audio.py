from typing import get_args
from tests.utils import evaluate_forwardref
from openai._compat import is_literal_type
from openai._utils._typing import is_union_type
from openai.types.audio_response_format import AudioResponseFormat
def test_translation_create_overloads_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
    fn = checking_client.audio.translations.create
    overload_response_formats: set[str] = set()
    for i, overload in enumerate(typing_extensions.get_overloads(fn)):
            fn,
            exclude_params={"response_format", "stream"},
            description=f" for overload {i}",
        sig = inspect.signature(overload)
        typ = evaluate_forwardref(
            sig.parameters["response_format"].annotation,
            globalns=sys.modules[fn.__module__].__dict__,
        if is_union_type(typ):
            for arg in get_args(typ):
                if not is_literal_type(arg):
                overload_response_formats.update(get_args(arg))
        elif is_literal_type(typ):
            overload_response_formats.update(get_args(typ))
    # 'diarized_json' applies only to transcriptions, not translations.
    src_response_formats: set[str] = set(get_args(AudioResponseFormat)) - {"diarized_json"}
    diff = src_response_formats.difference(overload_response_formats)
    assert len(diff) == 0, f"some response format options don't have overloads"
def test_transcription_create_overloads_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
    fn = checking_client.audio.transcriptions.create
        exclude_params = {"response_format", "stream"}
        # known_speaker_names and known_speaker_references are only supported by diarized_json
        if not (is_literal_type(typ) and set(get_args(typ)) == {"diarized_json"}):
            exclude_params.update({"known_speaker_names", "known_speaker_references"})
        # diarized_json does not support these parameters
        if is_literal_type(typ) and set(get_args(typ)) == {"diarized_json"}:
            exclude_params.update({"include", "prompt", "timestamp_granularities"})
            exclude_params=exclude_params,
    src_response_formats: set[str] = set(get_args(AudioResponseFormat))
