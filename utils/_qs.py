from typing import Any, List, Tuple, Union, Mapping, TypeVar
from urllib.parse import parse_qs, urlencode
from typing_extensions import Literal, get_args
from ._types import NotGiven, not_given
from ._utils import flatten
ArrayFormat = Literal["comma", "repeat", "indices", "brackets"]
NestedFormat = Literal["dots", "brackets"]
PrimitiveData = Union[str, int, float, bool, None]
# this should be Data = Union[PrimitiveData, "List[Data]", "Tuple[Data]", "Mapping[str, Data]"]
# https://github.com/microsoft/pyright/issues/3555
Data = Union[PrimitiveData, List[Any], Tuple[Any], "Mapping[str, Any]"]
Params = Mapping[str, Data]
class Querystring:
    array_format: ArrayFormat
    nested_format: NestedFormat
        array_format: ArrayFormat = "repeat",
        nested_format: NestedFormat = "brackets",
        self.array_format = array_format
        self.nested_format = nested_format
    def parse(self, query: str) -> Mapping[str, object]:
        # Note: custom format syntax is not supported yet
        return parse_qs(query)
    def stringify(
        params: Params,
        array_format: ArrayFormat | NotGiven = not_given,
        nested_format: NestedFormat | NotGiven = not_given,
        return urlencode(
            self.stringify_items(
                array_format=array_format,
                nested_format=nested_format,
    def stringify_items(
    ) -> list[tuple[str, str]]:
        opts = Options(
            qs=self,
        return flatten([self._stringify_item(key, value, opts) for key, value in params.items()])
    def _stringify_item(
        key: str,
        value: Data,
        opts: Options,
        if isinstance(value, Mapping):
            items: list[tuple[str, str]] = []
            nested_format = opts.nested_format
            for subkey, subvalue in value.items():
                items.extend(
                    self._stringify_item(
                        # TODO: error if unknown format
                        f"{key}.{subkey}" if nested_format == "dots" else f"{key}[{subkey}]",
                        subvalue,
                        opts,
        if isinstance(value, (list, tuple)):
            array_format = opts.array_format
            if array_format == "comma":
                        ",".join(self._primitive_value_to_str(item) for item in value if item is not None),
            elif array_format == "repeat":
                items = []
                for item in value:
                    items.extend(self._stringify_item(key, item, opts))
            elif array_format == "indices":
                raise NotImplementedError("The array indices format is not supported yet")
            elif array_format == "brackets":
                key = key + "[]"
                raise NotImplementedError(
                    f"Unknown array_format value: {array_format}, choose from {', '.join(get_args(ArrayFormat))}"
        serialised = self._primitive_value_to_str(value)
        if not serialised:
        return [(key, serialised)]
    def _primitive_value_to_str(self, value: PrimitiveData) -> str:
        # copied from httpx
        if value is True:
            return "true"
        elif value is False:
            return "false"
        elif value is None:
        return str(value)
_qs = Querystring()
parse = _qs.parse
stringify = _qs.stringify
stringify_items = _qs.stringify_items
class Options:
        qs: Querystring = _qs,
        self.array_format = qs.array_format if isinstance(array_format, NotGiven) else array_format
        self.nested_format = qs.nested_format if isinstance(nested_format, NotGiven) else nested_format
