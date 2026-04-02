from typing import TYPE_CHECKING, Any
from .._utils import LazyProxy
from ._common import MissingDependencyError, format_instructions
    import numpy as numpy
NUMPY_INSTRUCTIONS = format_instructions(library="numpy", extra="voice_helpers")
class NumpyProxy(LazyProxy[Any]):
    def __load__(self) -> Any:
            import numpy
        except ImportError as err:
            raise MissingDependencyError(NUMPY_INSTRUCTIONS) from err
        return numpy
    numpy = NumpyProxy()
def has_numpy() -> bool:
        import numpy  # noqa: F401  # pyright: ignore[reportUnusedImport]
