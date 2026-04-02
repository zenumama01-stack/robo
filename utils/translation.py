__all__ = ["Translation"]
class Translation(BaseModel):
from django.utils.translation import get_supported_language_variant
from django.utils.translation.trans_real import language_code_re
E001 = Error(
    "You have provided an invalid value for the LANGUAGE_CODE setting: {!r}.",
    id="translation.E001",
E002 = Error(
    "You have provided an invalid language code in the LANGUAGES setting: {!r}.",
    id="translation.E002",
E003 = Error(
    "You have provided an invalid language code in the LANGUAGES_BIDI setting: {!r}.",
    id="translation.E003",
E004 = Error(
    "You have provided a value for the LANGUAGE_CODE setting that is not in "
    "the LANGUAGES setting.",
    id="translation.E004",
@register(Tags.translation)
def check_setting_language_code(app_configs, **kwargs):
    """Error if LANGUAGE_CODE setting is invalid."""
    tag = settings.LANGUAGE_CODE
    if not isinstance(tag, str) or not language_code_re.match(tag):
        return [Error(E001.msg.format(tag), id=E001.id)]
def check_setting_languages(app_configs, **kwargs):
    """Error if LANGUAGES setting is invalid."""
        Error(E002.msg.format(tag), id=E002.id)
        for tag, _ in settings.LANGUAGES
        if not isinstance(tag, str) or not language_code_re.match(tag)
def check_setting_languages_bidi(app_configs, **kwargs):
    """Error if LANGUAGES_BIDI setting is invalid."""
        Error(E003.msg.format(tag), id=E003.id)
        for tag in settings.LANGUAGES_BIDI
def check_language_settings_consistent(app_configs, **kwargs):
    """Error if language settings are not consistent with each other."""
        get_supported_language_variant(settings.LANGUAGE_CODE)
        return [E004]
from typing import Any, Literal, Optional
TranslationTruncationStrategy = Literal["do_not_truncate", "longest_first", "only_first", "only_second"]
class TranslationParameters(BaseInferenceType):
    """Additional inference parameters for Translation"""
    clean_up_tokenization_spaces: Optional[bool] = None
    """Whether to clean up the potential extra spaces in the text output."""
    generate_parameters: Optional[dict[str, Any]] = None
    """Additional parametrization of the text generation algorithm."""
    src_lang: Optional[str] = None
    """The source language of the text. Required for models that can translate from multiple
    tgt_lang: Optional[str] = None
    """Target language to translate to. Required for models that can translate to multiple
    truncation: Optional["TranslationTruncationStrategy"] = None
    """The truncation strategy to use."""
class TranslationInput(BaseInferenceType):
    """Inputs for Translation inference"""
    """The text to translate."""
    parameters: Optional[TranslationParameters] = None
class TranslationOutput(BaseInferenceType):
    """Outputs of inference for the Translation task"""
    translation_text: str
