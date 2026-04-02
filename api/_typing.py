import typing
from typing import Any, TypeVar, Iterable, cast
from collections import abc as _c_abc
    TypeIs,
    Annotated,
from ._utils import lru_cache
from .._types import InheritsGeneric
from ._compat import is_union as _is_union
def is_annotated_type(typ: type) -> bool:
    return get_origin(typ) == Annotated
def is_list_type(typ: type) -> bool:
    return (get_origin(typ) or typ) == list
def is_sequence_type(typ: type) -> bool:
    return origin == typing_extensions.Sequence or origin == typing.Sequence or origin == _c_abc.Sequence
def is_iterable_type(typ: type) -> bool:
    """If the given type is `typing.Iterable[T]`"""
    return origin == Iterable or origin == _c_abc.Iterable
def is_union_type(typ: type) -> bool:
    return _is_union(get_origin(typ))
def is_required_type(typ: type) -> bool:
    return get_origin(typ) == Required
def is_typevar(typ: type) -> bool:
    # type ignore is required because type checkers
    # think this expression will always return False
    return type(typ) == TypeVar  # type: ignore
_TYPE_ALIAS_TYPES: tuple[type[typing_extensions.TypeAliasType], ...] = (typing_extensions.TypeAliasType,)
if sys.version_info >= (3, 12):
    _TYPE_ALIAS_TYPES = (*_TYPE_ALIAS_TYPES, typing.TypeAliasType)
def is_type_alias_type(tp: Any, /) -> TypeIs[typing_extensions.TypeAliasType]:
    """Return whether the provided argument is an instance of `TypeAliasType`.
    type Int = int
    is_type_alias_type(Int)
    # > True
    Str = TypeAliasType("Str", str)
    is_type_alias_type(Str)
    return isinstance(tp, _TYPE_ALIAS_TYPES)
# Extracts T from Annotated[T, ...] or from Required[Annotated[T, ...]]
def strip_annotated_type(typ: type) -> type:
    if is_required_type(typ) or is_annotated_type(typ):
        return strip_annotated_type(cast(type, get_args(typ)[0]))
    return typ
def extract_type_arg(typ: type, index: int) -> type:
    args = get_args(typ)
        return cast(type, args[index])
        raise RuntimeError(f"Expected type {typ} to have a type argument at index {index} but it did not") from err
def extract_type_var_from_base(
    typ: type,
    generic_bases: tuple[type, ...],
    index: int,
    """Given a type like `Foo[T]`, returns the generic type variable `T`.
    class MyResponse(Foo[bytes]):
    extract_type_var(MyResponse, bases=(Foo,), index=0) -> bytes
    And where a generic subclass is given:
    _T = TypeVar('_T')
    class MyResponse(Foo[_T]):
    extract_type_var(MyResponse[bytes], bases=(Foo,), index=0) -> bytes
    cls = cast(object, get_origin(typ) or typ)
    if cls in generic_bases:  # pyright: ignore[reportUnnecessaryContains]
        # we're given the class directly
        return extract_type_arg(typ, index)
    # if a subclass is given
    # ---
    # this is needed as __orig_bases__ is not present in the typeshed stubs
    # because it is intended to be for internal use only, however there does
    # not seem to be a way to resolve generic TypeVars for inherited subclasses
    # without using it.
    if isinstance(cls, InheritsGeneric):
        target_base_class: Any | None = None
        for base in cls.__orig_bases__:
            if base.__origin__ in generic_bases:
                target_base_class = base
        if target_base_class is None:
                "Could not find the generic base class;\n"
                "This should never happen;\n"
                f"Does {cls} inherit from one of {generic_bases} ?"
        extracted = extract_type_arg(target_base_class, index)
        if is_typevar(extracted):
            # If the extracted type argument is itself a type variable
            # then that means the subclass itself is generic, so we have
            # to resolve the type argument from the class itself, not
            # the base class.
            # Note: if there is more than 1 type argument, the subclass could
            # change the ordering of the type arguments, this is not currently
            # supported.
        return extracted
    raise RuntimeError(failure_message or f"Could not resolve inner type variable at index {index} for {typ}")
from typing import Any, Protocol, TypeVar
    from numbers import _IntegralLike as IntegralLike
        NumpyArray = npt.NDArray[Any]
    from types import CapsuleType
    CapsuleType = object
    from collections.abc import Buffer
    Buffer = Any
_Ink = float | tuple[int, ...] | str
Coords = Sequence[float] | Sequence[Sequence[float]]
class SupportsRead(Protocol[_T_co]):
    def read(self, length: int = ..., /) -> _T_co: ...
StrOrBytesPath = str | bytes | os.PathLike[str] | os.PathLike[bytes]
__all__ = ["Buffer", "IntegralLike", "StrOrBytesPath", "SupportsRead"]
This module provides stubs for type hints not supported by all relevant Python
NOTICE: This project should have zero required dependencies which means it
cannot simply require :module:`typing_extensions`, and I do not want to maintain
a vendored copy of :module:`typing_extensions`.
	Callable,  # Replaced by `collections.abc.Callable` in 3.9.2.
	Optional,  # Replaced by `X | None` in 3.10.
	TypeVar)
	from typing import AnyStr  # Removed in 3.18.
	AnyStr = TypeVar('AnyStr', str, bytes)
	from typing import Never  # Added in 3.11.
	from typing import NoReturn as Never
F = TypeVar('F', bound=Callable[..., Any])
	from warnings import deprecated  # Added in 3.13.
		def deprecated(
			message: str,
			/, *,
			category: Optional[type[Warning]] = DeprecationWarning,
			stacklevel: int = 1,
		) -> Callable[[F], F]:
			def decorator(f: F) -> F:
				def wrapper(*a, **k):
					warnings.warn(message, category=category, stacklevel=stacklevel+1)
					return f(*a, **k)
	from typing import override  # Added in 3.12.
		def override(f: F) -> F:
def assert_unreachable(message: str) -> Never:
	The code path is unreachable. Raises an :class:`AssertionError`.
	*message* (:class:`str`) is the error message.
	raise AssertionError(message)
This file defines the types for type annotations.
These names aren't part of the module namespace, but they are used in the
annotations in the function signatures. The functions in the module are only
valid for inputs that match the given type annotations.
    "Array",
    "Device",
    "Dtype",
    "SupportsDLPack",
    "SupportsBufferProtocol",
    "PyCapsule",
from ._array_object import Array
from numpy import (
    dtype,
    int8,
    int16,
    int32,
    int64,
    uint8,
    uint16,
    uint32,
    uint64,
    float32,
    float64,
class NestedSequence(Protocol[_T_co]):
    def __getitem__(self, key: int, /) -> _T_co | NestedSequence[_T_co]: ...
    def __len__(self, /) -> int: ...
Device = Literal["cpu"]
Dtype = dtype[Union[
]]
    from collections.abc import Buffer as SupportsBufferProtocol
    SupportsBufferProtocol = Any
PyCapsule = Any
class SupportsDLPack(Protocol):
    def __dlpack__(self, /, *, stream: None = ...) -> PyCapsule: ...
from typing import (Any, Callable, Coroutine, Dict, Generator, Sequence, Tuple,
                    TypeVar, Union)
if sys.version_info >= (3, 8):  # pragma: no cover
    from typing import TypedDict
    # use typing_extensions if installed but don't require it
        class TypedDict(dict):
                return super().__init_subclass__()
class _Details(TypedDict):
    target: Callable[..., Any]
    args: Tuple[Any, ...]
    kwargs: Dict[str, Any]
    tries: int
    elapsed: float
class Details(_Details, total=False):
    wait: float  # present in the on_backoff handler case for either decorator
    value: Any  # present in the on_predicate decorator case
_CallableT = TypeVar('_CallableT', bound=Callable[..., Any])
_Handler = Union[
    Callable[[Details], None],
    Callable[[Details], Coroutine[Any, Any, None]],
_Jitterer = Callable[[float], float]
_MaybeCallable = Union[T, Callable[[], T]]
_MaybeLogger = Union[str, logging.Logger, None]
_MaybeSequence = Union[T, Sequence[T]]
_Predicate = Callable[[T], bool]
_WaitGenerator = Callable[..., Generator[float, None, None]]
    "NestedSequence",
Array = Any
Device = Any
    "ndarray",
from cupy import (
    ndarray,
from cupy.cuda.device import Device
if TYPE_CHECKING or sys.version_info >= (3, 9):
    Dtype = dtype
Array = Any  # To be changed to a Protocol later (see array-api#589)
__all__ = ["Array", "ModuleType"]
# Custom type aliases used throughout Beautiful Soup to improve readability.
# Notes on improvements to the type system in newer versions of Python
# that can be used once Beautiful Soup drops support for older
# versions:
# * ClassVar can be put on class variables now.
# * In 3.10, x|y is an accepted shorthand for Union[x,y].
# * In 3.10, TypeAlias gains capabilities that can be used to
#   improve the tree matching types (I don't remember what, exactly).
# * In 3.9 it's possible to specialize the re.Match type,
#   e.g. re.Match[str]. In 3.8 there's a typing.re namespace for this,
#   but it's removed in 3.12, so to support the widest possible set of
#   versions I'm not using it.
    from bs4.element import (
        AttributeValueList,
        NamespacedAttribute,
class _RegularExpressionProtocol(Protocol):
    """A protocol object which can accept either Python's built-in
    `re.Pattern` objects, or the similar ``Regex`` objects defined by the
    third-party ``regex`` package.
        self, string: str, pos: int = ..., endpos: int = ...
    ) -> Optional[Any]: ...
    def pattern(self) -> str: ...
# Aliases for markup in various stages of processing.
#: The rawest form of markup: either a string, bytestring, or an open filehandle.
_IncomingMarkup: TypeAlias = Union[str, bytes, IO[str], IO[bytes]]
#: Markup that is in memory but has (potentially) yet to be converted
#: to Unicode.
_RawMarkup: TypeAlias = Union[str, bytes]
# Aliases for character encodings
#: A data encoding.
_Encoding: TypeAlias = str
#: One or more data encodings.
_Encodings: TypeAlias = Iterable[_Encoding]
# Aliases for XML namespaces
#: The prefix for an XML namespace.
_NamespacePrefix: TypeAlias = str
#: The URL of an XML namespace
_NamespaceURL: TypeAlias = str
#: A mapping of prefixes to namespace URLs.
_NamespaceMapping: TypeAlias = Dict[_NamespacePrefix, _NamespaceURL]
#: A mapping of namespace URLs to prefixes
_InvertedNamespaceMapping: TypeAlias = Dict[_NamespaceURL, _NamespacePrefix]
# Aliases for the attribute values associated with HTML/XML tags.
#: The value associated with an HTML or XML attribute. This is the
#: relatively unprocessed value Beautiful Soup expects to come from a
#: `TreeBuilder`.
_RawAttributeValue: TypeAlias = str
#: A dictionary of names to `_RawAttributeValue` objects. This is how
#: Beautiful Soup expects a `TreeBuilder` to represent a tag's
#: attribute values.
_RawAttributeValues: TypeAlias = (
    "Mapping[Union[str, NamespacedAttribute], _RawAttributeValue]"
#: An attribute value in its final form, as stored in the
# `Tag` class, after it has been processed and (in some cases)
# split into a list of strings.
_AttributeValue: TypeAlias = Union[str, "AttributeValueList"]
#: A dictionary of names to :py:data:`_AttributeValue` objects. This is what
#: a tag's attributes look like after processing.
_AttributeValues: TypeAlias = Dict[str, _AttributeValue]
#: The methods that deal with turning :py:data:`_RawAttributeValue` into
#: :py:data:`_AttributeValue` may be called several times, even after the values
#: are already processed (e.g. when cloning a tag), so they need to
#: be able to acommodate both possibilities.
_RawOrProcessedAttributeValues: TypeAlias = Union[_RawAttributeValues, _AttributeValues]
#: A number of tree manipulation methods can take either a `PageElement` or a
#: normal Python string (which will be converted to a `NavigableString`).
_InsertableElement: TypeAlias = Union["PageElement", str]
# Aliases to represent the many possibilities for matching bits of a
# parse tree.
# This is very complicated because we're applying a formal type system
# to some very DWIM code. The types we end up with will be the types
# of the arguments to the SoupStrainer constructor and (more
# familiarly to Beautiful Soup users) the find* methods.
#: A function that takes a PageElement and returns a yes-or-no answer.
_PageElementMatchFunction: TypeAlias = Callable[["PageElement"], bool]
#: A function that takes the raw parsed ingredients of a markup tag
#: and returns a yes-or-no answer.
#  Not necessary at the moment.
# _AllowTagCreationFunction:TypeAlias = Callable[[Optional[str], str, Optional[_RawAttributeValues]], bool]
#: A function that takes the raw parsed ingredients of a markup string node
# _AllowStringCreationFunction:TypeAlias = Callable[[Optional[str]], bool]
#: A function that takes a `Tag` and returns a yes-or-no answer.
#: A `TagNameMatchRule` expects this kind of function, if you're
#: going to pass it a function.
_TagMatchFunction: TypeAlias = Callable[["Tag"], bool]
#: A function that takes a string (or None) and returns a yes-or-no
#: answer. An `AttributeValueMatchRule` expects this kind of function, if
#: you're going to pass it a function.
_NullableStringMatchFunction: TypeAlias = Callable[[Optional[str]], bool]
#: A function that takes a string and returns a yes-or-no answer.  A
# `StringMatchRule` expects this kind of function, if you're going to
# pass it a function.
_StringMatchFunction: TypeAlias = Callable[[str], bool]
#: Either a tag name, an attribute value or a string can be matched
#: against a string, bytestring, regular expression, or a boolean.
_BaseStrainable: TypeAlias = Union[str, bytes, Pattern[str], bool]
#: A tag can be matched either with the `_BaseStrainable` options, or
#: using a function that takes the `Tag` as its sole argument.
_BaseStrainableElement: TypeAlias = Union[_BaseStrainable, _TagMatchFunction]
#: A tag's attribute value can be matched either with the
#: `_BaseStrainable` options, or using a function that takes that
#: value as its sole argument.
_BaseStrainableAttribute: TypeAlias = Union[_BaseStrainable, _NullableStringMatchFunction]
#: A tag can be matched using either a single criterion or a list of
#: criteria.
_StrainableElement: TypeAlias = Union[
    _BaseStrainableElement, Iterable[_BaseStrainableElement]
#: An attribute value can be matched using either a single criterion
#: or a list of criteria.
_StrainableAttribute: TypeAlias = Union[
    _BaseStrainableAttribute, Iterable[_BaseStrainableAttribute]
#: An string can be matched using the same techniques as
#: an attribute value.
_StrainableString: TypeAlias = _StrainableAttribute
#: A dictionary may be used to match against multiple attribute vlaues at once.
_StrainableAttributes: TypeAlias = Dict[str, _StrainableAttribute]
#: Many Beautiful soup methods return a PageElement or an ResultSet of
#: PageElements. A PageElement is either a Tag or a NavigableString.
#: These convenience aliases make it easier for IDE users to see which methods
#: are available on the objects they're dealing with.
_OneElement: TypeAlias = Union["PageElement", "Tag", "NavigableString"]
_AtMostOneElement: TypeAlias = Optional[_OneElement]
_AtMostOneTag: TypeAlias = Optional["Tag"]
_AtMostOneNavigableString: TypeAlias = Optional["NavigableString"]
_QueryResults: TypeAlias = "ResultSet[_OneElement]"
_SomeTags: TypeAlias = "ResultSet[Tag]"
_SomeNavigableStrings: TypeAlias = "ResultSet[NavigableString]"
# Copied from pydantic 1.9.2 (the latest version to support python 3.6.)
# https://github.com/pydantic/pydantic/blob/v1.9.2/pydantic/typing.py
# Reduced drastically to only include Typer-specific 3.9+ functionality
    def is_union(tp: Optional[type[Any]]) -> bool:
        return tp is Union
        return tp is Union or tp is types.UnionType  # noqa: E721
    "is_none_type",
    "is_callable_type",
    "is_literal_type",
    "all_literal_values",
    "is_union",
    "Annotated",
    "get_args",
    "get_origin",
    "get_type_hints",
NoneType = None.__class__
NONE_TYPES: tuple[Any, Any, Any] = (None, NoneType, Literal[None])
def is_none_type(type_: Any) -> bool:
    for none_type in NONE_TYPES:
        if type_ is none_type:
def is_callable_type(type_: type[Any]) -> bool:
    return type_ is Callable or get_origin(type_) is Callable
def is_literal_type(type_: type[Any]) -> bool:
    return get_origin(type_) is Literal
def literal_values(type_: type[Any]) -> tuple[Any, ...]:
    return get_args(type_)
def all_literal_values(type_: type[Any]) -> tuple[Any, ...]:
    This method is used to retrieve all Literal values as
    Literal can be used recursively (see https://www.python.org/dev/peps/pep-0586)
    e.g. `Literal[Literal[Literal[1, 2, 3], "foo"], 5, None]`
    if not is_literal_type(type_):
        return (type_,)
    values = literal_values(type_)
    return tuple(x for value in values for x in all_literal_values(value))
Some (initially private) typing helpers for jsonschema's types.
from typing import Any, Protocol
import referencing.jsonschema
class SchemaKeywordValidator(Protocol):
        validator: Validator,
        instance: Any,
        schema: referencing.jsonschema.Schema,
id_of = Callable[[referencing.jsonschema.Schema], str | None]
ApplicableValidators = Callable[
    [referencing.jsonschema.Schema],
    Iterable[tuple[str, Any]],
# Copyright 2022-present, the HuggingFace Inc. team.
"""Handle typing imports based on system compatibility."""
from typing import Any, Callable, Literal, Optional, Type, TypeVar, Union, get_args, get_origin
UNION_TYPES: list[Any] = [Union]
    from types import UnionType
    UNION_TYPES += [UnionType]
HTTP_METHOD_T = Literal["GET", "OPTIONS", "HEAD", "POST", "PUT", "PATCH", "DELETE"]
# type hint meaning "function signature not changed by decorator"
CallableT = TypeVar("CallableT", bound=Callable)
_JSON_SERIALIZABLE_TYPES = (int, float, str, bool, type(None))
def is_jsonable(obj: Any, _visited: Optional[set[int]] = None) -> bool:
    """Check if an object is JSON serializable.
    This is a weak check, as it does not check for the actual JSON serialization, but only for the types of the object.
    It works correctly for basic use cases but do not guarantee an exhaustive check.
    Object is considered to be recursively json serializable if:
    - it is an instance of int, float, str, bool, or NoneType
    - it is a list or tuple and all its items are json serializable
    - it is a dict and all its keys are strings and all its values are json serializable
    Uses a visited set to avoid infinite recursion on circular references. If object has already been visited, it is
    considered not json serializable.
    # Initialize visited set to track object ids and detect circular references
    if _visited is None:
        _visited = set()
    # Detect circular reference
    obj_id = id(obj)
    if obj_id in _visited:
    # Add current object to visited before recursive checks
    _visited.add(obj_id)
        if isinstance(obj, _JSON_SERIALIZABLE_TYPES):
        if isinstance(obj, (list, tuple)):
            return all(is_jsonable(item, _visited) for item in obj)
                isinstance(key, _JSON_SERIALIZABLE_TYPES) and is_jsonable(value, _visited)
                for key, value in obj.items()
        if hasattr(obj, "__json__"):
        # Remove the object id from visited to avoid side‑effects for other branches
        _visited.discard(obj_id)
def is_simple_optional_type(type_: Type) -> bool:
    """Check if a type is optional, i.e. Optional[Type] or Union[Type, None] or Type | None, where Type is a non-composite type."""
    if get_origin(type_) in UNION_TYPES:
        union_args = get_args(type_)
        if len(union_args) == 2 and type(None) in union_args:
def unwrap_simple_optional_type(optional_type: Type) -> Type:
    """Unwraps a simple optional type, i.e. returns Type from Optional[Type]."""
    for arg in get_args(optional_type):
        if arg is not type(None):
            return arg
    raise ValueError(f"'{optional_type}' is not an optional type")
