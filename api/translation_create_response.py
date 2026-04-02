from .translation import Translation
from .translation_verbose import TranslationVerbose
__all__ = ["TranslationCreateResponse"]
TranslationCreateResponse: TypeAlias = Union[Translation, TranslationVerbose]
