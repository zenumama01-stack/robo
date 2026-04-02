from typing import Any, Dict, List, Union, TypeVar, Iterable, Optional, cast
from typing_extensions import Required, Annotated, TypedDict
from openai._types import Base64FileInput, omit, not_given
from openai._utils import (
    transform as _transform,
    async_transform as _async_transform,
from openai._compat import PYDANTIC_V1
from openai._models import BaseModel
SAMPLE_FILE_PATH = pathlib.Path(__file__).parent.joinpath("sample_file.txt")
async def transform(
    use_async: bool,
    if use_async:
        return await _async_transform(data, expected_type=expected_type)
    return _transform(data, expected_type=expected_type)
parametrize = pytest.mark.parametrize("use_async", [False, True], ids=["sync", "async"])
class Foo1(TypedDict):
    foo_bar: Annotated[str, PropertyInfo(alias="fooBar")]
@parametrize
async def test_top_level_alias(use_async: bool) -> None:
    assert await transform({"foo_bar": "hello"}, expected_type=Foo1, use_async=use_async) == {"fooBar": "hello"}
class Foo2(TypedDict):
    bar: Bar2
class Bar2(TypedDict):
    this_thing: Annotated[int, PropertyInfo(alias="this__thing")]
    baz: Annotated[Baz2, PropertyInfo(alias="Baz")]
class Baz2(TypedDict):
    my_baz: Annotated[str, PropertyInfo(alias="myBaz")]
async def test_recursive_typeddict(use_async: bool) -> None:
    assert await transform({"bar": {"this_thing": 1}}, Foo2, use_async) == {"bar": {"this__thing": 1}}
    assert await transform({"bar": {"baz": {"my_baz": "foo"}}}, Foo2, use_async) == {"bar": {"Baz": {"myBaz": "foo"}}}
class Foo3(TypedDict):
    things: List[Bar3]
class Bar3(TypedDict):
    my_field: Annotated[str, PropertyInfo(alias="myField")]
async def test_list_of_typeddict(use_async: bool) -> None:
    result = await transform({"things": [{"my_field": "foo"}, {"my_field": "foo2"}]}, Foo3, use_async)
    assert result == {"things": [{"myField": "foo"}, {"myField": "foo2"}]}
class Foo4(TypedDict):
    foo: Union[Bar4, Baz4]
class Bar4(TypedDict):
class Baz4(TypedDict):
    foo_baz: Annotated[str, PropertyInfo(alias="fooBaz")]
async def test_union_of_typeddict(use_async: bool) -> None:
    assert await transform({"foo": {"foo_bar": "bar"}}, Foo4, use_async) == {"foo": {"fooBar": "bar"}}
    assert await transform({"foo": {"foo_baz": "baz"}}, Foo4, use_async) == {"foo": {"fooBaz": "baz"}}
    assert await transform({"foo": {"foo_baz": "baz", "foo_bar": "bar"}}, Foo4, use_async) == {
        "foo": {"fooBaz": "baz", "fooBar": "bar"}
class Foo5(TypedDict):
    foo: Annotated[Union[Bar4, List[Baz4]], PropertyInfo(alias="FOO")]
class Bar5(TypedDict):
class Baz5(TypedDict):
async def test_union_of_list(use_async: bool) -> None:
    assert await transform({"foo": {"foo_bar": "bar"}}, Foo5, use_async) == {"FOO": {"fooBar": "bar"}}
    assert await transform(
            "foo": [
                {"foo_baz": "baz"},
        Foo5,
        use_async,
    ) == {"FOO": [{"fooBaz": "baz"}, {"fooBaz": "baz"}]}
class Foo6(TypedDict):
    bar: Annotated[str, PropertyInfo(alias="Bar")]
async def test_includes_unknown_keys(use_async: bool) -> None:
    assert await transform({"bar": "bar", "baz_": {"FOO": 1}}, Foo6, use_async) == {
        "Bar": "bar",
        "baz_": {"FOO": 1},
class Foo7(TypedDict):
    bar: Annotated[List[Bar7], PropertyInfo(alias="bAr")]
    foo: Bar7
class Bar7(TypedDict):
async def test_ignores_invalid_input(use_async: bool) -> None:
    assert await transform({"bar": "<foo>"}, Foo7, use_async) == {"bAr": "<foo>"}
    assert await transform({"foo": "<foo>"}, Foo7, use_async) == {"foo": "<foo>"}
class DatetimeDict(TypedDict, total=False):
    foo: Annotated[datetime, PropertyInfo(format="iso8601")]
    bar: Annotated[Optional[datetime], PropertyInfo(format="iso8601")]
    required: Required[Annotated[Optional[datetime], PropertyInfo(format="iso8601")]]
    list_: Required[Annotated[Optional[List[datetime]], PropertyInfo(format="iso8601")]]
    union: Annotated[Union[int, datetime], PropertyInfo(format="iso8601")]
class DateDict(TypedDict, total=False):
    foo: Annotated[date, PropertyInfo(format="iso8601")]
class DatetimeModel(BaseModel):
    foo: datetime
class DateModel(BaseModel):
    foo: Optional[date]
async def test_iso8601_format(use_async: bool) -> None:
    dt = datetime.fromisoformat("2023-02-23T14:16:36.337692+00:00")
    tz = "+00:00" if PYDANTIC_V1 else "Z"
    assert await transform({"foo": dt}, DatetimeDict, use_async) == {"foo": "2023-02-23T14:16:36.337692+00:00"}  # type: ignore[comparison-overlap]
    assert await transform(DatetimeModel(foo=dt), Any, use_async) == {"foo": "2023-02-23T14:16:36.337692" + tz}  # type: ignore[comparison-overlap]
    dt = dt.replace(tzinfo=None)
    assert await transform({"foo": dt}, DatetimeDict, use_async) == {"foo": "2023-02-23T14:16:36.337692"}  # type: ignore[comparison-overlap]
    assert await transform(DatetimeModel(foo=dt), Any, use_async) == {"foo": "2023-02-23T14:16:36.337692"}  # type: ignore[comparison-overlap]
    assert await transform({"foo": None}, DateDict, use_async) == {"foo": None}  # type: ignore[comparison-overlap]
    assert await transform(DateModel(foo=None), Any, use_async) == {"foo": None}  # type: ignore
    assert await transform({"foo": date.fromisoformat("2023-02-23")}, DateDict, use_async) == {"foo": "2023-02-23"}  # type: ignore[comparison-overlap]
    assert await transform(DateModel(foo=date.fromisoformat("2023-02-23")), DateDict, use_async) == {
        "foo": "2023-02-23"
    }  # type: ignore[comparison-overlap]
async def test_optional_iso8601_format(use_async: bool) -> None:
    assert await transform({"bar": dt}, DatetimeDict, use_async) == {"bar": "2023-02-23T14:16:36.337692+00:00"}  # type: ignore[comparison-overlap]
    assert await transform({"bar": None}, DatetimeDict, use_async) == {"bar": None}
async def test_required_iso8601_format(use_async: bool) -> None:
    assert await transform({"required": dt}, DatetimeDict, use_async) == {
        "required": "2023-02-23T14:16:36.337692+00:00"
    assert await transform({"required": None}, DatetimeDict, use_async) == {"required": None}
async def test_union_datetime(use_async: bool) -> None:
    assert await transform({"union": dt}, DatetimeDict, use_async) == {  # type: ignore[comparison-overlap]
        "union": "2023-02-23T14:16:36.337692+00:00"
    assert await transform({"union": "foo"}, DatetimeDict, use_async) == {"union": "foo"}
async def test_nested_list_iso6801_format(use_async: bool) -> None:
    dt1 = datetime.fromisoformat("2023-02-23T14:16:36.337692+00:00")
    dt2 = parse_datetime("2022-01-15T06:34:23Z")
    assert await transform({"list_": [dt1, dt2]}, DatetimeDict, use_async) == {  # type: ignore[comparison-overlap]
        "list_": ["2023-02-23T14:16:36.337692+00:00", "2022-01-15T06:34:23+00:00"]
async def test_datetime_custom_format(use_async: bool) -> None:
    dt = parse_datetime("2022-01-15T06:34:23Z")
    result = await transform(dt, Annotated[datetime, PropertyInfo(format="custom", format_template="%H")], use_async)
    assert result == "06"  # type: ignore[comparison-overlap]
class DateDictWithRequiredAlias(TypedDict, total=False):
    required_prop: Required[Annotated[date, PropertyInfo(format="iso8601", alias="prop")]]
async def test_datetime_with_alias(use_async: bool) -> None:
    assert await transform({"required_prop": None}, DateDictWithRequiredAlias, use_async) == {"prop": None}  # type: ignore[comparison-overlap]
        {"required_prop": date.fromisoformat("2023-02-23")}, DateDictWithRequiredAlias, use_async
    ) == {"prop": "2023-02-23"}  # type: ignore[comparison-overlap]
async def test_pydantic_model_to_dictionary(use_async: bool) -> None:
    assert cast(Any, await transform(MyModel(foo="hi!"), Any, use_async)) == {"foo": "hi!"}
    assert cast(Any, await transform(MyModel.construct(foo="hi!"), Any, use_async)) == {"foo": "hi!"}
async def test_pydantic_empty_model(use_async: bool) -> None:
    assert cast(Any, await transform(MyModel.construct(), Any, use_async)) == {}
async def test_pydantic_unknown_field(use_async: bool) -> None:
    assert cast(Any, await transform(MyModel.construct(my_untyped_field=True), Any, use_async)) == {
        "my_untyped_field": True
async def test_pydantic_mismatched_types(use_async: bool) -> None:
    model = MyModel.construct(foo=True)
        params = await transform(model, Any, use_async)
        with pytest.warns(UserWarning):
    assert cast(Any, params) == {"foo": True}
async def test_pydantic_mismatched_object_type(use_async: bool) -> None:
    model = MyModel.construct(foo=MyModel.construct(hello="world"))
    assert cast(Any, params) == {"foo": {"hello": "world"}}
class ModelNestedObjects(BaseModel):
    nested: MyModel
async def test_pydantic_nested_objects(use_async: bool) -> None:
    model = ModelNestedObjects.construct(nested={"foo": "stainless"})
    assert isinstance(model.nested, MyModel)
    assert cast(Any, await transform(model, Any, use_async)) == {"nested": {"foo": "stainless"}}
class ModelWithDefaultField(BaseModel):
    with_none_default: Union[str, None] = None
    with_str_default: str = "foo"
async def test_pydantic_default_field(use_async: bool) -> None:
    # should be excluded when defaults are used
    model = ModelWithDefaultField.construct()
    assert model.with_none_default is None
    assert model.with_str_default == "foo"
    assert cast(Any, await transform(model, Any, use_async)) == {}
    # should be included when the default value is explicitly given
    model = ModelWithDefaultField.construct(with_none_default=None, with_str_default="foo")
    assert cast(Any, await transform(model, Any, use_async)) == {"with_none_default": None, "with_str_default": "foo"}
    # should be included when a non-default value is explicitly given
    model = ModelWithDefaultField.construct(with_none_default="bar", with_str_default="baz")
    assert model.with_none_default == "bar"
    assert model.with_str_default == "baz"
    assert cast(Any, await transform(model, Any, use_async)) == {"with_none_default": "bar", "with_str_default": "baz"}
class TypedDictIterableUnion(TypedDict):
    foo: Annotated[Union[Bar8, Iterable[Baz8]], PropertyInfo(alias="FOO")]
class Bar8(TypedDict):
class Baz8(TypedDict):
async def test_iterable_of_dictionaries(use_async: bool) -> None:
    assert await transform({"foo": [{"foo_baz": "bar"}]}, TypedDictIterableUnion, use_async) == {
        "FOO": [{"fooBaz": "bar"}]
    assert cast(Any, await transform({"foo": ({"foo_baz": "bar"},)}, TypedDictIterableUnion, use_async)) == {
    def my_iter() -> Iterable[Baz8]:
        yield {"foo_baz": "hello"}
        yield {"foo_baz": "world"}
    assert await transform({"foo": my_iter()}, TypedDictIterableUnion, use_async) == {
        "FOO": [{"fooBaz": "hello"}, {"fooBaz": "world"}]
async def test_dictionary_items(use_async: bool) -> None:
    class DictItems(TypedDict):
    assert await transform({"foo": {"foo_baz": "bar"}}, Dict[str, DictItems], use_async) == {"foo": {"fooBaz": "bar"}}
class TypedDictIterableUnionStr(TypedDict):
    foo: Annotated[Union[str, Iterable[Baz8]], PropertyInfo(alias="FOO")]
async def test_iterable_union_str(use_async: bool) -> None:
    assert await transform({"foo": "bar"}, TypedDictIterableUnionStr, use_async) == {"FOO": "bar"}
    assert cast(Any, await transform(iter([{"foo_baz": "bar"}]), Union[str, Iterable[Baz8]], use_async)) == [
        {"fooBaz": "bar"}
class TypedDictBase64Input(TypedDict):
    foo: Annotated[Union[str, Base64FileInput], PropertyInfo(format="base64")]
async def test_base64_file_input(use_async: bool) -> None:
    # strings are left as-is
    assert await transform({"foo": "bar"}, TypedDictBase64Input, use_async) == {"foo": "bar"}
    # pathlib.Path is automatically converted to base64
    assert await transform({"foo": SAMPLE_FILE_PATH}, TypedDictBase64Input, use_async) == {
        "foo": "SGVsbG8sIHdvcmxkIQo="
    # io instances are automatically converted to base64
    assert await transform({"foo": io.StringIO("Hello, world!")}, TypedDictBase64Input, use_async) == {
        "foo": "SGVsbG8sIHdvcmxkIQ=="
    assert await transform({"foo": io.BytesIO(b"Hello, world!")}, TypedDictBase64Input, use_async) == {
async def test_transform_skipping(use_async: bool) -> None:
    # lists of ints are left as-is
    data = [1, 2, 3]
    assert await transform(data, List[int], use_async) is data
    # iterables of ints are converted to a list
    data = iter([1, 2, 3])
    assert await transform(data, Iterable[int], use_async) == [1, 2, 3]
async def test_strips_notgiven(use_async: bool) -> None:
    assert await transform({"foo_bar": "bar"}, Foo1, use_async) == {"fooBar": "bar"}
    assert await transform({"foo_bar": not_given}, Foo1, use_async) == {}
async def test_strips_omit(use_async: bool) -> None:
    assert await transform({"foo_bar": omit}, Foo1, use_async) == {}
"""Test transform chain."""
from langchain_classic.chains.transform import TransformChain
def dummy_transform(inputs: dict[str, str]) -> dict[str, str]:
    """Transform a dummy input for tests."""
    outputs = inputs
    outputs["greeting"] = f"{inputs['first_name']} {inputs['last_name']} says hello"
    del outputs["first_name"]
    del outputs["last_name"]
def test_transform_chain() -> None:
    """Test basic transform chain."""
    transform_chain = TransformChain(
        input_variables=["first_name", "last_name"],
        output_variables=["greeting"],
        transform=dummy_transform,
    input_dict = {"first_name": "Leroy", "last_name": "Jenkins"}
    response = transform_chain(input_dict)
    expected_response = {"greeting": "Leroy Jenkins says hello"}
    assert response == expected_response
def test_transform_chain_bad_inputs() -> None:
    input_dict = {"name": "Leroy", "last_name": "Jenkins"}
        ValueError, match=re.escape("Missing some input keys: {'first_name'}")
        _ = transform_chain(input_dict)
