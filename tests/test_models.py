from typing import TYPE_CHECKING, Any, Dict, List, Union, Optional, cast
from typing_extensions import Literal, Annotated, TypeAliasType
from pydantic import Field
from openai._utils import PropertyInfo
from openai._compat import PYDANTIC_V1, parse_obj, model_dump, model_json
from openai._models import DISCRIMINATOR_CACHE, BaseModel, construct_type
class BasicModel(BaseModel):
@pytest.mark.parametrize("value", ["hello", 1], ids=["correct type", "mismatched"])
def test_basic(value: object) -> None:
    m = BasicModel.construct(foo=value)
    assert m.foo == value
def test_directly_nested_model() -> None:
    class NestedModel(BaseModel):
        nested: BasicModel
    m = NestedModel.construct(nested={"foo": "Foo!"})
    assert m.nested.foo == "Foo!"
    # mismatched types
    m = NestedModel.construct(nested="hello!")
    assert cast(Any, m.nested) == "hello!"
def test_optional_nested_model() -> None:
        nested: Optional[BasicModel]
    m1 = NestedModel.construct(nested=None)
    assert m1.nested is None
    m2 = NestedModel.construct(nested={"foo": "bar"})
    assert m2.nested is not None
    assert m2.nested.foo == "bar"
    m3 = NestedModel.construct(nested={"foo"})
    assert isinstance(cast(Any, m3.nested), set)
    assert cast(Any, m3.nested) == {"foo"}
def test_list_nested_model() -> None:
        nested: List[BasicModel]
    m = NestedModel.construct(nested=[{"foo": "bar"}, {"foo": "2"}])
    assert m.nested is not None
    assert isinstance(m.nested, list)
    assert len(m.nested) == 2
    assert m.nested[0].foo == "bar"
    assert m.nested[1].foo == "2"
    m = NestedModel.construct(nested=True)
    assert cast(Any, m.nested) is True
    m = NestedModel.construct(nested=[False])
    assert cast(Any, m.nested) == [False]
def test_optional_list_nested_model() -> None:
        nested: Optional[List[BasicModel]]
    m1 = NestedModel.construct(nested=[{"foo": "bar"}, {"foo": "2"}])
    assert m1.nested is not None
    assert isinstance(m1.nested, list)
    assert len(m1.nested) == 2
    assert m1.nested[0].foo == "bar"
    assert m1.nested[1].foo == "2"
    m2 = NestedModel.construct(nested=None)
    assert m2.nested is None
    m3 = NestedModel.construct(nested={1})
    assert cast(Any, m3.nested) == {1}
    m4 = NestedModel.construct(nested=[False])
    assert cast(Any, m4.nested) == [False]
def test_list_optional_items_nested_model() -> None:
        nested: List[Optional[BasicModel]]
    m = NestedModel.construct(nested=[None, {"foo": "bar"}])
    assert m.nested[0] is None
    assert m.nested[1] is not None
    assert m.nested[1].foo == "bar"
    m3 = NestedModel.construct(nested="foo")
    assert cast(Any, m3.nested) == "foo"
def test_list_mismatched_type() -> None:
        nested: List[str]
    m = NestedModel.construct(nested=False)
    assert cast(Any, m.nested) is False
def test_raw_dictionary() -> None:
        nested: Dict[str, str]
    m = NestedModel.construct(nested={"hello": "world"})
    assert m.nested == {"hello": "world"}
def test_nested_dictionary_model() -> None:
        nested: Dict[str, BasicModel]
    m = NestedModel.construct(nested={"hello": {"foo": "bar"}})
    assert isinstance(m.nested, dict)
    assert m.nested["hello"].foo == "bar"
    m = NestedModel.construct(nested={"hello": False})
    assert cast(Any, m.nested["hello"]) is False
def test_unknown_fields() -> None:
    m1 = BasicModel.construct(foo="foo", unknown=1)
    assert m1.foo == "foo"
    assert cast(Any, m1).unknown == 1
    m2 = BasicModel.construct(foo="foo", unknown={"foo_bar": True})
    assert m2.foo == "foo"
    assert cast(Any, m2).unknown == {"foo_bar": True}
    assert model_dump(m2) == {"foo": "foo", "unknown": {"foo_bar": True}}
def test_strict_validation_unknown_fields() -> None:
    model = parse_obj(Model, dict(foo="hello!", user="Robert"))
    assert model.foo == "hello!"
    assert cast(Any, model).user == "Robert"
    assert model_dump(model) == {"foo": "hello!", "user": "Robert"}
def test_aliases() -> None:
        my_field: int = Field(alias="myField")
    m = Model.construct(myField=1)
    assert m.my_field == 1
    m = Model.construct(myField={"hello": False})
    assert cast(Any, m.my_field) == {"hello": False}
def test_repr() -> None:
    model = BasicModel(foo="bar")
    assert str(model) == "BasicModel(foo='bar')"
    assert repr(model) == "BasicModel(foo='bar')"
def test_repr_nested_model() -> None:
    class Child(BaseModel):
        age: int
    class Parent(BaseModel):
        child: Child
    model = Parent(name="Robert", child=Child(name="Foo", age=5))
    assert str(model) == "Parent(name='Robert', child=Child(name='Foo', age=5))"
    assert repr(model) == "Parent(name='Robert', child=Child(name='Foo', age=5))"
def test_optional_list() -> None:
    class Submodel(BaseModel):
        items: Optional[List[Submodel]]
    m = Model.construct(items=None)
    assert m.items is None
    m = Model.construct(items=[])
    assert m.items == []
    m = Model.construct(items=[{"name": "Robert"}])
    assert m.items is not None
    assert len(m.items) == 1
    assert m.items[0].name == "Robert"
def test_nested_union_of_models() -> None:
    class Submodel1(BaseModel):
        bar: bool
    class Submodel2(BaseModel):
        thing: str
        foo: Union[Submodel1, Submodel2]
    m = Model.construct(foo={"thing": "hello"})
    assert isinstance(m.foo, Submodel2)
    assert m.foo.thing == "hello"
def test_nested_union_of_mixed_types() -> None:
        foo: Union[Submodel1, Literal[True], Literal["CARD_HOLDER"]]
    m = Model.construct(foo=True)
    assert m.foo is True
    m = Model.construct(foo="CARD_HOLDER")
    assert m.foo == "CARD_HOLDER"
    m = Model.construct(foo={"bar": False})
    assert isinstance(m.foo, Submodel1)
    assert m.foo.bar is False
def test_nested_union_multiple_variants() -> None:
    class Submodel3(BaseModel):
        foo: Union[Submodel1, Submodel2, None, Submodel3]
    m = Model.construct(foo=None)
    assert m.foo is None
    m = Model.construct()
    m = Model.construct(foo={"foo": "1"})
    assert isinstance(m.foo, Submodel3)
    assert m.foo.foo == 1
def test_nested_union_invalid_data() -> None:
        level: int
    assert cast(bool, m.foo) is True
    m = Model.construct(foo={"name": 3})
        assert m.foo.name == "3"
        assert m.foo.name == 3  # type: ignore
def test_list_of_unions() -> None:
        items: List[Union[Submodel1, Submodel2]]
    m = Model.construct(items=[{"level": 1}, {"name": "Robert"}])
    assert len(m.items) == 2
    assert isinstance(m.items[0], Submodel1)
    assert m.items[0].level == 1
    assert isinstance(m.items[1], Submodel2)
    assert m.items[1].name == "Robert"
    m = Model.construct(items=[{"level": -1}, 156])
    assert m.items[0].level == -1
    assert cast(Any, m.items[1]) == 156
def test_union_of_lists() -> None:
    class SubModel1(BaseModel):
    class SubModel2(BaseModel):
        items: Union[List[SubModel1], List[SubModel2]]
    # with one valid entry
    assert isinstance(m.items[0], SubModel2)
    # with two entries pointing to different types
    assert isinstance(m.items[0], SubModel1)
    assert isinstance(m.items[1], SubModel1)
    assert cast(Any, m.items[1]).name == "Robert"
    # with two entries pointing to *completely* different types
def test_dict_of_union() -> None:
        data: Dict[str, Union[SubModel1, SubModel2]]
    m = Model.construct(data={"hello": {"name": "there"}, "foo": {"foo": "bar"}})
    assert len(list(m.data.keys())) == 2
    assert isinstance(m.data["hello"], SubModel1)
    assert m.data["hello"].name == "there"
    assert isinstance(m.data["foo"], SubModel2)
    assert m.data["foo"].foo == "bar"
    # TODO: test mismatched type
def test_double_nested_union() -> None:
        bar: str
        data: Dict[str, List[Union[SubModel1, SubModel2]]]
    m = Model.construct(data={"foo": [{"bar": "baz"}, {"name": "Robert"}]})
    assert len(m.data["foo"]) == 2
    entry1 = m.data["foo"][0]
    assert isinstance(entry1, SubModel2)
    assert entry1.bar == "baz"
    entry2 = m.data["foo"][1]
    assert isinstance(entry2, SubModel1)
    assert entry2.name == "Robert"
def test_union_of_dict() -> None:
        data: Union[Dict[str, SubModel1], Dict[str, SubModel2]]
    assert isinstance(m.data["foo"], SubModel1)
    assert cast(Any, m.data["foo"]).foo == "bar"
def test_iso8601_datetime() -> None:
        created_at: datetime
    expected = datetime(2019, 12, 27, 18, 11, 19, 117000, tzinfo=timezone.utc)
        expected_json = '{"created_at": "2019-12-27T18:11:19.117000+00:00"}'
        expected_json = '{"created_at":"2019-12-27T18:11:19.117000Z"}'
    model = Model.construct(created_at="2019-12-27T18:11:19.117Z")
    assert model.created_at == expected
    assert model_json(model) == expected_json
    model = parse_obj(Model, dict(created_at="2019-12-27T18:11:19.117Z"))
def test_does_not_coerce_int() -> None:
    assert Model.construct(bar=1).bar == 1
    assert Model.construct(bar=10.9).bar == 10.9
    assert Model.construct(bar="19").bar == "19"  # type: ignore[comparison-overlap]
    assert Model.construct(bar=False).bar is False
def test_int_to_float_safe_conversion() -> None:
        float_field: float
    m = Model.construct(float_field=10)
    assert m.float_field == 10.0
    assert isinstance(m.float_field, float)
    m = Model.construct(float_field=10.12)
    assert m.float_field == 10.12
    # number too big
    m = Model.construct(float_field=2**53 + 1)
    assert m.float_field == 2**53 + 1
    assert isinstance(m.float_field, int)
def test_deprecated_alias() -> None:
        resource_id: str = Field(alias="model_id")
        def model_id(self) -> str:
            return self.resource_id
    m = Model.construct(model_id="id")
    assert m.model_id == "id"
    assert m.resource_id == "id"
    assert m.resource_id is m.model_id
    m = parse_obj(Model, {"model_id": "id"})
def test_omitted_fields() -> None:
        resource_id: Optional[str] = None
    assert m.resource_id is None
    assert "resource_id" not in m.model_fields_set
    m = Model.construct(resource_id=None)
    assert "resource_id" in m.model_fields_set
    m = Model.construct(resource_id="foo")
    assert m.resource_id == "foo"
def test_to_dict() -> None:
        foo: Optional[str] = Field(alias="FOO", default=None)
    m = Model(FOO="hello")
    assert m.to_dict() == {"FOO": "hello"}
    assert m.to_dict(use_api_names=False) == {"foo": "hello"}
    m2 = Model()
    assert m2.to_dict() == {}
    assert m2.to_dict(exclude_unset=False) == {"FOO": None}
    assert m2.to_dict(exclude_unset=False, exclude_none=True) == {}
    assert m2.to_dict(exclude_unset=False, exclude_defaults=True) == {}
    m3 = Model(FOO=None)
    assert m3.to_dict() == {"FOO": None}
    assert m3.to_dict(exclude_none=True) == {}
    assert m3.to_dict(exclude_defaults=True) == {}
    time_str = "2024-03-21T11:39:01.275859"
    m4 = Model2.construct(created_at=time_str)
    assert m4.to_dict(mode="python") == {"created_at": datetime.fromisoformat(time_str)}
    assert m4.to_dict(mode="json") == {"created_at": time_str}
        with pytest.raises(ValueError, match="warnings is only supported in Pydantic v2"):
            m.to_dict(warnings=False)
def test_forwards_compat_model_dump_method() -> None:
    assert m.model_dump() == {"foo": "hello"}
    assert m.model_dump(include={"bar"}) == {}
    assert m.model_dump(exclude={"foo"}) == {}
    assert m.model_dump(by_alias=True) == {"FOO": "hello"}
    assert m2.model_dump() == {"foo": None}
    assert m2.model_dump(exclude_unset=True) == {}
    assert m2.model_dump(exclude_none=True) == {}
    assert m2.model_dump(exclude_defaults=True) == {}
    assert m3.model_dump() == {"foo": None}
    assert m3.model_dump(exclude_none=True) == {}
        with pytest.raises(ValueError, match="round_trip is only supported in Pydantic v2"):
            m.model_dump(round_trip=True)
            m.model_dump(warnings=False)
def test_compat_method_no_error_for_warnings() -> None:
        foo: Optional[str]
    m = Model(foo="hello")
    assert isinstance(model_dump(m, warnings=False), dict)
def test_to_json() -> None:
    assert json.loads(m.to_json()) == {"FOO": "hello"}
    assert json.loads(m.to_json(use_api_names=False)) == {"foo": "hello"}
        assert m.to_json(indent=None) == '{"FOO": "hello"}'
        assert m.to_json(indent=None) == '{"FOO":"hello"}'
    assert json.loads(m2.to_json()) == {}
    assert json.loads(m2.to_json(exclude_unset=False)) == {"FOO": None}
    assert json.loads(m2.to_json(exclude_unset=False, exclude_none=True)) == {}
    assert json.loads(m2.to_json(exclude_unset=False, exclude_defaults=True)) == {}
    assert json.loads(m3.to_json()) == {"FOO": None}
    assert json.loads(m3.to_json(exclude_none=True)) == {}
            m.to_json(warnings=False)
def test_forwards_compat_model_dump_json_method() -> None:
    assert json.loads(m.model_dump_json()) == {"foo": "hello"}
    assert json.loads(m.model_dump_json(include={"bar"})) == {}
    assert json.loads(m.model_dump_json(include={"foo"})) == {"foo": "hello"}
    assert json.loads(m.model_dump_json(by_alias=True)) == {"FOO": "hello"}
    assert m.model_dump_json(indent=2) == '{\n  "foo": "hello"\n}'
    assert json.loads(m2.model_dump_json()) == {"foo": None}
    assert json.loads(m2.model_dump_json(exclude_unset=True)) == {}
    assert json.loads(m2.model_dump_json(exclude_none=True)) == {}
    assert json.loads(m2.model_dump_json(exclude_defaults=True)) == {}
    assert json.loads(m3.model_dump_json()) == {"foo": None}
    assert json.loads(m3.model_dump_json(exclude_none=True)) == {}
            m.model_dump_json(round_trip=True)
            m.model_dump_json(warnings=False)
def test_type_compat() -> None:
    # our model type can be assigned to Pydantic's model type
    def takes_pydantic(model: pydantic.BaseModel) -> None:  # noqa: ARG001
    class OurModel(BaseModel):
        foo: Optional[str] = None
    takes_pydantic(OurModel())
def test_annotated_types() -> None:
    m = construct_type(
        value={"value": "foo"},
        type_=cast(Any, Annotated[Model, "random metadata"]),
    assert isinstance(m, Model)
    assert m.value == "foo"
def test_discriminated_unions_invalid_data() -> None:
    class A(BaseModel):
        type: Literal["a"]
    class B(BaseModel):
        type: Literal["b"]
        data: int
        value={"type": "b", "data": "foo"},
        type_=cast(Any, Annotated[Union[A, B], PropertyInfo(discriminator="type")]),
    assert isinstance(m, B)
    assert m.type == "b"
    assert m.data == "foo"  # type: ignore[comparison-overlap]
        value={"type": "a", "data": 100},
    assert isinstance(m, A)
    assert m.type == "a"
        # pydantic v1 automatically converts inputs to strings
        # if the expected type is a str
        assert m.data == "100"
        assert m.data == 100  # type: ignore[comparison-overlap]
def test_discriminated_unions_unknown_variant() -> None:
        value={"type": "c", "data": None, "new_thing": "bar"},
    # just chooses the first variant
    assert m.type == "c"  # type: ignore[comparison-overlap]
    assert m.data == None  # type: ignore[unreachable]
    assert m.new_thing == "bar"
def test_discriminated_unions_invalid_data_nested_unions() -> None:
    class C(BaseModel):
        type: Literal["c"]
        data: bool
        type_=cast(Any, Annotated[Union[Union[A, B], C], PropertyInfo(discriminator="type")]),
        value={"type": "c", "data": "foo"},
    assert isinstance(m, C)
    assert m.type == "c"
def test_discriminated_unions_with_aliases_invalid_data() -> None:
        foo_type: Literal["a"] = Field(alias="type")
        foo_type: Literal["b"] = Field(alias="type")
        type_=cast(Any, Annotated[Union[A, B], PropertyInfo(discriminator="foo_type")]),
    assert m.foo_type == "b"
    assert m.foo_type == "a"
def test_discriminated_unions_overlapping_discriminators_invalid_data() -> None:
        value={"type": "a", "data": "foo"},
def test_discriminated_unions_invalid_data_uses_cache() -> None:
    UnionType = cast(Any, Union[A, B])
    assert not DISCRIMINATOR_CACHE.get(UnionType)
        value={"type": "b", "data": "foo"}, type_=cast(Any, Annotated[UnionType, PropertyInfo(discriminator="type")])
    discriminator = DISCRIMINATOR_CACHE.get(UnionType)
    assert discriminator is not None
    # if the discriminator details object stays the same between invocations then
    # we hit the cache
    assert DISCRIMINATOR_CACHE.get(UnionType) is discriminator
@pytest.mark.skipif(PYDANTIC_V1, reason="TypeAliasType is not supported in Pydantic v1")
def test_type_alias_type() -> None:
    Alias = TypeAliasType("Alias", str)  # pyright: ignore
        alias: Alias
        union: Union[int, Alias]
    m = construct_type(value={"alias": "foo", "union": "bar"}, type_=Model)
    assert isinstance(m.alias, str)
    assert m.alias == "foo"
    assert isinstance(m.union, str)
    assert m.union == "bar"
def test_field_named_cls() -> None:
        cls: str
    m = construct_type(value={"cls": "foo"}, type_=Model)
    assert isinstance(m.cls, str)
def test_discriminated_union_case() -> None:
        data: List[Union[A, object]]
    class ModelA(BaseModel):
        type: Literal["modelA"]
    class ModelB(BaseModel):
        type: Literal["modelB"]
        required: str
        data: Union[A, B]
    # when constructing ModelA | ModelB, value data doesn't match ModelB exactly - missing `required`
        value={"type": "modelB", "data": {"type": "a", "data": True}},
        type_=cast(Any, Annotated[Union[ModelA, ModelB], PropertyInfo(discriminator="type")]),
    assert isinstance(m, ModelB)
def test_nested_discriminated_union() -> None:
    class InnerType1(BaseModel):
        type: Literal["type_1"]
    class InnerModel(BaseModel):
        inner_value: str
    class InnerType2(BaseModel):
        type: Literal["type_2"]
        some_inner_model: InnerModel
    class Type1(BaseModel):
        base_type: Literal["base_type_1"]
        value: Annotated[
                InnerType1,
                InnerType2,
    class Type2(BaseModel):
        base_type: Literal["base_type_2"]
    T = Annotated[
            Type1,
            Type2,
        PropertyInfo(discriminator="base_type"),
    model = construct_type(
        type_=T,
            "base_type": "base_type_1",
            "value": {
                "type": "type_2",
    assert isinstance(model, Type1)
    assert isinstance(model.value, InnerType2)
@pytest.mark.skipif(PYDANTIC_V1, reason="this is only supported in pydantic v2 for now")
def test_extra_properties() -> None:
    class Item(BaseModel):
        prop: int
        __pydantic_extra__: Dict[str, Item] = Field(init=False)  # pyright: ignore[reportIncompatibleVariableOverride]
        other: str
            def __getattr__(self, attr: str) -> Item: ...
        type_=Model,
            "a": {"prop": 1},
            "other": "foo",
    assert isinstance(model, Model)
    assert model.a.prop == 1
    assert isinstance(model.a, Item)
    assert model.other == "foo"
from openai.types import Model, ModelDeleted
from openai.pagination import SyncPage, AsyncPage
class TestModels:
        model = client.models.retrieve(
        assert_matches_type(Model, model, path=["response"])
        response = client.models.with_raw_response.retrieve(
        model = response.parse()
        with client.models.with_streaming_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `model` but received ''"):
            client.models.with_raw_response.retrieve(
        model = client.models.list()
        assert_matches_type(SyncPage[Model], model, path=["response"])
        response = client.models.with_raw_response.list()
        with client.models.with_streaming_response.list() as response:
        model = client.models.delete(
            "ft:gpt-4o-mini:acemeco:suffix:abc123",
        assert_matches_type(ModelDeleted, model, path=["response"])
        response = client.models.with_raw_response.delete(
        with client.models.with_streaming_response.delete(
            client.models.with_raw_response.delete(
class TestAsyncModels:
        model = await async_client.models.retrieve(
        response = await async_client.models.with_raw_response.retrieve(
        async with async_client.models.with_streaming_response.retrieve(
            model = await response.parse()
            await async_client.models.with_raw_response.retrieve(
        model = await async_client.models.list()
        assert_matches_type(AsyncPage[Model], model, path=["response"])
        response = await async_client.models.with_raw_response.list()
        async with async_client.models.with_streaming_response.list() as response:
        model = await async_client.models.delete(
        response = await async_client.models.with_raw_response.delete(
        async with async_client.models.with_streaming_response.delete(
            await async_client.models.with_raw_response.delete(
from onedrivesdk.model.item import Item
from onedrivesdk.model.item_reference import ItemReference
class TestModels(unittest.TestCase):
    name = "test1"
    id = "thisisa!test"
    def test_serialization(self):
        Test the serialization of the dict-backed models, seeing that
        the correct objects are returned when called
        ref = ItemReference();
        ref._prop_dict = {"id": self.id}
        response = {"name":self.name, "folder":{}, "parentReference":ref._prop_dict, "lastModifiedDateTime": "2015-07-09T22:22:53.993000Z"}
        item = Item();
        item._prop_dict = response
        assert isinstance(item.folder, Folder)
        assert item.name == self.name
        assert isinstance(item.parent_reference, ItemReference)
        assert item.parent_reference.id == self.id
        assert isinstance(item.last_modified_date_time, datetime)
        assert item.last_modified_date_time.isoformat()+"Z" == response["lastModifiedDateTime"]
    def test_serialization_different_datetime(self):
        the correct objects are returned when called. Specifically,
        ensure that the datetime can be parsed correctly when the format
        does not fit exactly what we always return
        response = {"name":self.name, "folder":{}, "parentReference":ref._prop_dict, "lastModifiedDateTime": "2015-07-09T22:22:53.99Z"}
        assert item.last_modified_date_time.isoformat()+"Z" == "2015-07-09T22:22:53.990000Z"
        response = {"name":self.name, "folder":{}, "parentReference":ref._prop_dict, "lastModifiedDateTime": "2015-07-09T22:22:53Z"}
        assert item.last_modified_date_time.isoformat()+"Z" == "2015-07-09T22:22:53Z"
        timenow = datetime.now()
        item.last_modified_date_time = timenow
        assert item.last_modified_date_time.isoformat() == timenow.isoformat()
        assert item._prop_dict['lastModifiedDateTime'] == timenow.isoformat()+"Z"
        timenow = timenow.replace(microsecond=235)
        timenow = timenow.replace(microsecond=0)
        assert item._prop_dict['lastModifiedDateTime'] == timenow.isoformat()+".0Z"
from django.conf.global_settings import PASSWORD_HASHERS
from django.contrib.auth.backends import ModelBackend
from django.contrib.auth.base_user import AbstractBaseUser
from django.contrib.auth.hashers import get_hasher
from django.contrib.auth.models import (
    AnonymousUser,
    UserManager,
from django.db import connection, migrations
from django.db.models.signals import post_save
from django.test import SimpleTestCase, TestCase, TransactionTestCase, override_settings
from .models import CustomEmailField, IntegerUsernameUser
class NaturalKeysTestCase(TestCase):
    def test_user_natural_key(self):
        staff_user = User.objects.create_user(username="staff")
        self.assertEqual(User.objects.get_by_natural_key("staff"), staff_user)
        self.assertEqual(staff_user.natural_key(), ("staff",))
    async def test_auser_natural_key(self):
        staff_user = await User.objects.acreate_user(username="staff")
        self.assertEqual(await User.objects.aget_by_natural_key("staff"), staff_user)
    def test_group_natural_key(self):
        users_group = Group.objects.create(name="users")
        self.assertEqual(Group.objects.get_by_natural_key("users"), users_group)
    async def test_agroup_natural_key(self):
        users_group = await Group.objects.acreate(name="users")
        self.assertEqual(await Group.objects.aget_by_natural_key("users"), users_group)
class LoadDataWithoutNaturalKeysTestCase(TestCase):
    fixtures = ["regular.json"]
    def test_user_is_created_and_added_to_group(self):
        user = User.objects.get(username="my_username")
        group = Group.objects.get(name="my_group")
        self.assertEqual(group, user.groups.get())
class LoadDataWithNaturalKeysTestCase(TestCase):
    fixtures = ["natural.json"]
class LoadDataWithNaturalKeysAndMultipleDatabasesTestCase(TestCase):
    databases = {"default", "other"}
    def test_load_data_with_user_permissions(self):
        # Create test contenttypes for both databases
        default_objects = [
            ContentType.objects.db_manager("default").create(
                model="examplemodela",
                app_label="app_a",
                model="examplemodelb",
                app_label="app_b",
        other_objects = [
            ContentType.objects.db_manager("other").create(
        # Now we create the test UserPermission
        Permission.objects.db_manager("default").create(
            name="Can delete example model b",
            codename="delete_examplemodelb",
            content_type=default_objects[1],
        Permission.objects.db_manager("other").create(
            content_type=other_objects[0],
        perm_default = Permission.objects.get_by_natural_key(
            "delete_examplemodelb",
            "app_b",
            "examplemodelb",
        perm_other = Permission.objects.db_manager("other").get_by_natural_key(
        self.assertEqual(perm_default.content_type_id, default_objects[1].id)
        self.assertEqual(perm_other.content_type_id, other_objects[0].id)
class UserManagerTestCase(TransactionTestCase):
    available_apps = [
        "auth_tests",
        "django.contrib.auth",
        "django.contrib.contenttypes",
    def test_create_user(self):
        email_lowercase = "normal@normal.com"
        user = User.objects.create_user("user", email_lowercase)
        self.assertEqual(user.email, email_lowercase)
        self.assertEqual(user.username, "user")
        self.assertFalse(user.has_usable_password())
    def test_create_user_email_domain_normalize_rfc3696(self):
        # According to RFC 3696 Section 3 the "@" symbol can be part of the
        # local part of an email address.
        returned = UserManager.normalize_email(r"Abc\@DEF@EXAMPLE.com")
        self.assertEqual(returned, r"Abc\@DEF@example.com")
    def test_create_user_email_domain_normalize(self):
        returned = UserManager.normalize_email("normal@DOMAIN.COM")
        self.assertEqual(returned, "normal@domain.com")
    def test_create_user_email_domain_normalize_with_whitespace(self):
        returned = UserManager.normalize_email(r"email\ with_whitespace@D.COM")
        self.assertEqual(returned, r"email\ with_whitespace@d.com")
    def test_empty_username(self):
        with self.assertRaisesMessage(ValueError, "The given username must be set"):
            User.objects.create_user(username="")
    def test_create_user_is_staff(self):
        email = "normal@normal.com"
        user = User.objects.create_user("user", email, is_staff=True)
        self.assertEqual(user.email, email)
        self.assertTrue(user.is_staff)
    def test_create_super_user_raises_error_on_false_is_superuser(self):
        with self.assertRaisesMessage(
            ValueError, "Superuser must have is_superuser=True."
            User.objects.create_superuser(
                username="test",
                email="test@test.com",
                password="test",
                is_superuser=False,
    async def test_acreate_super_user_raises_error_on_false_is_superuser(self):
            await User.objects.acreate_superuser(
    def test_create_superuser_raises_error_on_false_is_staff(self):
        with self.assertRaisesMessage(ValueError, "Superuser must have is_staff=True."):
                is_staff=False,
    async def test_acreate_superuser_raises_error_on_false_is_staff(self):
    def test_runpython_manager_methods(self):
        def forwards(apps, schema_editor):
            UserModel = apps.get_model("auth", "User")
            user = UserModel.objects.create_user("user1", password="secure")
            self.assertIsInstance(user, UserModel)
        operation = migrations.RunPython(forwards, migrations.RunPython.noop)
        project_state = ProjectState()
        project_state.add_model(ModelState.from_model(User))
        project_state.add_model(ModelState.from_model(Group))
        project_state.add_model(ModelState.from_model(Permission))
        project_state.add_model(ModelState.from_model(ContentType))
        new_state = project_state.clone()
            operation.state_forwards("test_manager_methods", new_state)
            operation.database_forwards(
                "test_manager_methods",
                editor,
                project_state,
                new_state,
        user = User.objects.get(username="user1")
        self.assertTrue(user.check_password("secure"))
class AbstractBaseUserTests(SimpleTestCase):
    def test_has_usable_password(self):
        Passwords are usable even if they don't correspond to a hasher in
        settings.PASSWORD_HASHERS.
        self.assertIs(User(password="some-gibbberish").has_usable_password(), True)
    def test_normalize_username(self):
        self.assertEqual(IntegerUsernameUser().normalize_username(123), 123)
    def test_clean_normalize_username(self):
        # The normalization happens in AbstractBaseUser.clean()
        ohm_username = "iamtheΩ"  # U+2126 OHM SIGN
        for model in ("auth.User", "auth_tests.CustomUser"):
            with self.subTest(model=model), self.settings(AUTH_USER_MODEL=model):
                User = get_user_model()
                user = User(**{User.USERNAME_FIELD: ohm_username, "password": "foo"})
                user.clean()
                username = user.get_username()
                self.assertNotEqual(username, ohm_username)
                    username, "iamtheΩ"
                )  # U+03A9 GREEK CAPITAL LETTER OMEGA
    def test_default_email(self):
        self.assertEqual(AbstractBaseUser.get_email_field_name(), "email")
    def test_custom_email(self):
        user = CustomEmailField()
        self.assertEqual(user.get_email_field_name(), "email_address")
class AbstractUserTestCase(TestCase):
    def test_email_user(self):
        # valid send_mail parameters
            "fail_silently": False,
            "auth_user": None,
            "auth_password": None,
            "connection": None,
            "html_message": None,
        user = User(email="foo@bar.com")
        user.email_user(
            subject="Subject here",
            message="This is a message",
            from_email="from@domain.com",
        self.assertEqual(len(mail.outbox), 1)
        message = mail.outbox[0]
        self.assertEqual(message.subject, "Subject here")
        self.assertEqual(message.body, "This is a message")
        self.assertEqual(message.from_email, "from@domain.com")
        self.assertEqual(message.to, [user.email])
    def test_last_login_default(self):
        user1 = User.objects.create(username="user1")
        self.assertIsNone(user1.last_login)
        user2 = User.objects.create_user(username="user2")
        self.assertIsNone(user2.last_login)
    def test_user_clean_normalize_email(self):
        user = User(username="user", password="foo", email="foo@BAR.com")
        self.assertEqual(user.email, "foo@bar.com")
    def test_user_double_save(self):
        Calling user.save() twice should trigger password_changed() once.
        user = User.objects.create_user(username="user", password="foo")
        user.set_password("bar")
            "django.contrib.auth.password_validation.password_changed"
        ) as pw_changed:
            user.save()
            self.assertEqual(pw_changed.call_count, 1)
    @override_settings(PASSWORD_HASHERS=PASSWORD_HASHERS)
    def test_check_password_upgrade(self):
        password_changed() shouldn't be called if User.check_password()
        triggers a hash iteration upgrade.
        initial_password = user.password
        self.assertTrue(user.check_password("foo"))
        hasher = get_hasher("default")
        self.assertEqual("pbkdf2_sha256", hasher.algorithm)
        old_iterations = hasher.iterations
            # Upgrade the password iterations
            hasher.iterations = old_iterations + 1
                user.check_password("foo")
                self.assertEqual(pw_changed.call_count, 0)
            self.assertNotEqual(initial_password, user.password)
            hasher.iterations = old_iterations
    async def test_acheck_password_upgrade(self):
        user = await User.objects.acreate_user(username="user", password="foo")
        self.assertIs(await user.acheck_password("foo"), True)
            # Upgrade the password iterations.
class CustomModelBackend(ModelBackend):
        if obj is not None and obj.username == "charliebrown":
            return User.objects.filter(pk=obj.pk)
        return User.objects.filter(username__startswith="charlie")
class UserWithPermTestCase(TestCase):
    def setUpTestData(cls):
        content_type = ContentType.objects.get_for_model(Group)
        cls.permission = Permission.objects.create(
            name="test",
            codename="test",
        # User with permission.
        cls.user1 = User.objects.create_user("user 1", "foo@example.com")
        cls.user1.user_permissions.add(cls.permission)
        # User with group permission.
        group1 = Group.objects.create(name="group 1")
        group1.permissions.add(cls.permission)
        group2 = Group.objects.create(name="group 2")
        group2.permissions.add(cls.permission)
        cls.user2 = User.objects.create_user("user 2", "bar@example.com")
        cls.user2.groups.add(group1, group2)
        # Users without permissions.
        cls.user_charlie = User.objects.create_user("charlie", "charlie@example.com")
        cls.user_charlie_b = User.objects.create_user(
            "charliebrown", "charlie@brown.com"
        # Superuser.
        cls.superuser = User.objects.create_superuser(
            "superuser",
            "superuser@example.com",
            "superpassword",
        # Inactive user with permission.
        cls.inactive_user = User.objects.create_user(
            "inactive_user",
            "baz@example.com",
            is_active=False,
        cls.inactive_user.user_permissions.add(cls.permission)
    def test_invalid_permission_name(self):
        msg = "Permission name should be in the form app_label.permission_codename."
        for perm in ("nodots", "too.many.dots", "...", ""):
            with self.subTest(perm), self.assertRaisesMessage(ValueError, msg):
                User.objects.with_perm(perm)
    def test_invalid_permission_type(self):
        msg = "The `perm` argument must be a string or a permission instance."
        for perm in (b"auth.test", object(), None):
            with self.subTest(perm), self.assertRaisesMessage(TypeError, msg):
    def test_invalid_backend_type(self):
        msg = "backend must be a dotted import path string (got %r)."
        for backend in (b"auth_tests.CustomModelBackend", object()):
            with self.subTest(backend):
                with self.assertRaisesMessage(TypeError, msg % backend):
                    User.objects.with_perm("auth.test", backend=backend)
        active_users = [self.user1, self.user2]
            ({}, [*active_users, self.superuser]),
            ({"obj": self.user1}, []),
            # Only inactive users.
            ({"is_active": False}, [self.inactive_user]),
            # All users.
            ({"is_active": None}, [*active_users, self.superuser, self.inactive_user]),
            # Exclude superusers.
            ({"include_superusers": False}, active_users),
                {"include_superusers": False, "is_active": False},
                [self.inactive_user],
                {"include_superusers": False, "is_active": None},
                [*active_users, self.inactive_user],
        for kwargs, expected_users in tests:
            for perm in ("auth.test", self.permission):
                with self.subTest(perm=perm, **kwargs):
                    self.assertCountEqual(
                        User.objects.with_perm(perm, **kwargs),
                        expected_users,
    @override_settings(
        AUTHENTICATION_BACKENDS=["django.contrib.auth.backends.BaseBackend"]
    def test_backend_without_with_perm(self):
        self.assertSequenceEqual(User.objects.with_perm("auth.test"), [])
    def test_nonexistent_permission(self):
        self.assertSequenceEqual(User.objects.with_perm("auth.perm"), [self.superuser])
    def test_nonexistent_backend(self):
        with self.assertRaises(ImportError):
            User.objects.with_perm(
                "auth.test",
                backend="invalid.backend.CustomModelBackend",
    def test_invalid_backend_submodule(self):
                backend="json.tool",
        AUTHENTICATION_BACKENDS=["auth_tests.test_models.CustomModelBackend"]
    def test_custom_backend(self):
            with self.subTest(perm):
                    User.objects.with_perm(perm),
                    [self.user_charlie, self.user_charlie_b],
    def test_custom_backend_pass_obj(self):
                self.assertSequenceEqual(
                    User.objects.with_perm(perm, obj=self.user_charlie_b),
                    [self.user_charlie_b],
        AUTHENTICATION_BACKENDS=[
            "auth_tests.test_models.CustomModelBackend",
            "django.contrib.auth.backends.ModelBackend",
    def test_multiple_backends(self):
        with self.assertRaisesMessage(ValueError, msg):
            User.objects.with_perm("auth.test")
        backend = "auth_tests.test_models.CustomModelBackend"
            User.objects.with_perm("auth.test", backend=backend),
class IsActiveTestCase(TestCase):
    Tests the behavior of the guaranteed is_active attribute
    def test_builtin_user_isactive(self):
        user = User.objects.create(username="foo", email="foo@bar.com")
        # is_active is true by default
        self.assertIs(user.is_active, True)
        user.is_active = False
        user_fetched = User.objects.get(pk=user.pk)
        # the is_active flag is saved
        self.assertFalse(user_fetched.is_active)
    @override_settings(AUTH_USER_MODEL="auth_tests.IsActiveTestUser1")
    def test_is_active_field_default(self):
        tests that the default value for is_active is provided
        UserModel = get_user_model()
        user = UserModel(username="foo")
        # you can set the attribute - but it will not save
        # there should be no problem saving - but the attribute is not saved
        user_fetched = UserModel._default_manager.get(pk=user.pk)
        # the attribute is always true for newly retrieved instance
        self.assertIs(user_fetched.is_active, True)
class TestCreateSuperUserSignals(TestCase):
    Simple test case for ticket #20541
    def post_save_listener(self, *args, **kwargs):
        self.signals_count += 1
        self.signals_count = 0
        post_save.connect(self.post_save_listener, sender=User)
        self.addCleanup(post_save.disconnect, self.post_save_listener, sender=User)
        User.objects.create_user("JohnDoe")
        self.assertEqual(self.signals_count, 1)
    def test_create_superuser(self):
        User.objects.create_superuser("JohnDoe", "mail@example.com", "1")
class AnonymousUserTests(SimpleTestCase):
    no_repr_msg = "Django doesn't provide a DB representation for AnonymousUser."
        self.user = AnonymousUser()
    def test_properties(self):
        self.assertIsNone(self.user.pk)
        self.assertEqual(self.user.username, "")
        self.assertEqual(self.user.get_username(), "")
        self.assertIs(self.user.is_anonymous, True)
        self.assertIs(self.user.is_authenticated, False)
        self.assertIs(self.user.is_staff, False)
        self.assertIs(self.user.is_active, False)
        self.assertIs(self.user.is_superuser, False)
        self.assertEqual(self.user.groups.count(), 0)
        self.assertEqual(self.user.user_permissions.count(), 0)
        self.assertEqual(self.user.get_user_permissions(), set())
        self.assertEqual(self.user.get_group_permissions(), set())
    async def test_properties_async_versions(self):
        self.assertEqual(await self.user.groups.acount(), 0)
        self.assertEqual(await self.user.user_permissions.acount(), 0)
        self.assertEqual(await self.user.aget_user_permissions(), set())
        self.assertEqual(await self.user.aget_group_permissions(), set())
    def test_str(self):
        self.assertEqual(str(self.user), "AnonymousUser")
    def test_eq(self):
        self.assertEqual(self.user, AnonymousUser())
        self.assertNotEqual(self.user, User("super", "super@example.com", "super"))
    def test_hash(self):
        self.assertEqual(hash(self.user), 1)
    def test_int(self):
            "Cannot cast AnonymousUser to int. Are you trying to use it in "
            "place of User?"
        with self.assertRaisesMessage(TypeError, msg):
            int(self.user)
    def test_delete(self):
        with self.assertRaisesMessage(NotImplementedError, self.no_repr_msg):
            self.user.delete()
    def test_save(self):
            self.user.save()
    def test_set_password(self):
            self.user.set_password("password")
    def test_check_password(self):
            self.user.check_password("password")
class GroupTests(SimpleTestCase):
        g = Group(name="Users")
        self.assertEqual(str(g), "Users")
class PermissionTests(TestCase):
        p = Permission.objects.get(codename="view_customemailfield")
            str(p), "Auth_Tests | custom email field | Can view custom email field"
from .models import Comment, Tenant, Token, User
class CompositePKModelsTests(TestCase):
        cls.tenant_1 = Tenant.objects.create()
        cls.tenant_2 = Tenant.objects.create()
        cls.user_1 = User.objects.create(
            tenant=cls.tenant_1,
            id=1,
            email="user0001@example.com",
        cls.user_2 = User.objects.create(
            id=2,
            email="user0002@example.com",
        cls.user_3 = User.objects.create(
            tenant=cls.tenant_2,
            id=3,
            email="user0003@example.com",
        cls.comment_1 = Comment.objects.create(id=1, user=cls.user_1)
        cls.comment_2 = Comment.objects.create(id=2, user=cls.user_1)
        cls.comment_3 = Comment.objects.create(id=3, user=cls.user_2)
        cls.comment_4 = Comment.objects.create(id=4, user=cls.user_3)
    def test_fields(self):
        # tenant_1
            self.tenant_1.user_set.order_by("pk"),
            [self.user_1, self.user_2],
            self.tenant_1.comments.order_by("pk"),
            [self.comment_1, self.comment_2, self.comment_3],
        # tenant_2
        self.assertSequenceEqual(self.tenant_2.user_set.order_by("pk"), [self.user_3])
            self.tenant_2.comments.order_by("pk"), [self.comment_4]
        # user_1
        self.assertEqual(self.user_1.id, 1)
        self.assertEqual(self.user_1.tenant_id, self.tenant_1.id)
        self.assertEqual(self.user_1.tenant, self.tenant_1)
        self.assertEqual(self.user_1.pk, (self.tenant_1.id, self.user_1.id))
            self.user_1.comments.order_by("pk"), [self.comment_1, self.comment_2]
        # user_2
        self.assertEqual(self.user_2.id, 2)
        self.assertEqual(self.user_2.tenant_id, self.tenant_1.id)
        self.assertEqual(self.user_2.tenant, self.tenant_1)
        self.assertEqual(self.user_2.pk, (self.tenant_1.id, self.user_2.id))
        self.assertSequenceEqual(self.user_2.comments.order_by("pk"), [self.comment_3])
        # comment_1
        self.assertEqual(self.comment_1.id, 1)
        self.assertEqual(self.comment_1.user_id, self.user_1.id)
        self.assertEqual(self.comment_1.user, self.user_1)
        self.assertEqual(self.comment_1.tenant_id, self.tenant_1.id)
        self.assertEqual(self.comment_1.tenant, self.tenant_1)
        self.assertEqual(self.comment_1.pk, (self.tenant_1.id, self.user_1.id))
    def test_full_clean_success(self):
        test_cases = (
            # 1, 1234, {}
            ({"tenant": self.tenant_1, "id": 1234}, {}),
            ({"tenant_id": self.tenant_1.id, "id": 1234}, {}),
            ({"pk": (self.tenant_1.id, 1234)}, {}),
            # 1, 1, {"id"}
            ({"tenant": self.tenant_1, "id": 1}, {"id"}),
            ({"tenant_id": self.tenant_1.id, "id": 1}, {"id"}),
            ({"pk": (self.tenant_1.id, 1)}, {"id"}),
            # 1, 1, {"tenant", "id"}
            ({"tenant": self.tenant_1, "id": 1}, {"tenant", "id"}),
            ({"tenant_id": self.tenant_1.id, "id": 1}, {"tenant", "id"}),
            ({"pk": (self.tenant_1.id, 1)}, {"tenant", "id"}),
        for kwargs, exclude in test_cases:
            with self.subTest(kwargs):
                kwargs["email"] = "user0004@example.com"
                User(**kwargs).full_clean(exclude=exclude)
    def test_full_clean_failure(self):
        e_tenant_and_id = "User with this Tenant and Id already exists."
        e_id = "User with this Id already exists."
            # 1, 1, {}
            ({"tenant": self.tenant_1, "id": 1}, {}, (e_tenant_and_id, e_id)),
            ({"tenant_id": self.tenant_1.id, "id": 1}, {}, (e_tenant_and_id, e_id)),
            ({"pk": (self.tenant_1.id, 1)}, {}, (e_tenant_and_id, e_id)),
            # 2, 1, {}
            ({"tenant": self.tenant_2, "id": 1}, {}, (e_id,)),
            ({"tenant_id": self.tenant_2.id, "id": 1}, {}, (e_id,)),
            ({"pk": (self.tenant_2.id, 1)}, {}, (e_id,)),
            # 1, 1, {"tenant"}
            ({"tenant": self.tenant_1, "id": 1}, {"tenant"}, (e_id,)),
            ({"tenant_id": self.tenant_1.id, "id": 1}, {"tenant"}, (e_id,)),
            ({"pk": (self.tenant_1.id, 1)}, {"tenant"}, (e_id,)),
        for kwargs, exclude, messages in test_cases:
                with self.assertRaises(ValidationError) as ctx:
                self.assertSequenceEqual(ctx.exception.messages, messages)
    def test_full_clean_update(self):
        with self.assertNumQueries(1):
            self.comment_1.full_clean()
    def test_field_conflicts(self):
            ({"pk": (1, 1), "id": 2}, (1, 1)),
            ({"id": 2, "pk": (1, 1)}, (1, 1)),
            ({"pk": (1, 1), "tenant_id": 2}, (1, 1)),
            ({"tenant_id": 2, "pk": (1, 1)}, (1, 1)),
            ({"pk": (2, 2), "tenant_id": 3, "id": 4}, (2, 2)),
            ({"tenant_id": 3, "id": 4, "pk": (2, 2)}, (2, 2)),
        for kwargs, pk in test_cases:
            with self.subTest(kwargs=kwargs):
                user = User(**kwargs)
                self.assertEqual(user.pk, pk)
    def test_validate_unique(self):
        user = User.objects.get(pk=self.user_1.pk)
        user.id = None
            user.validate_unique()
            ctx.exception.messages, ("User with this Email already exists.",)
    def test_permissions(self):
        token = ContentType.objects.get_for_model(Token)
        user = ContentType.objects.get_for_model(User)
        comment = ContentType.objects.get_for_model(Comment)
        self.assertEqual(4, token.permission_set.count())
        self.assertEqual(4, user.permission_set.count())
        self.assertEqual(4, comment.permission_set.count())
from django.contrib.contenttypes.models import ContentType, ContentTypeManager
from django.contrib.contenttypes.prefetch import GenericPrefetch
from django.test import TestCase, override_settings
from django.test.utils import isolate_apps
from .models import Author, ConcreteModel, FooWithUrl, ProxyModel
class ContentTypesTests(TestCase):
        self.addCleanup(ContentType.objects.clear_cache)
    def test_lookup_cache(self):
        The content type cache (see ContentTypeManager) works correctly.
        Lookups for a particular content type -- by model, ID, or natural key
        -- should hit the database only on the first lookup.
        # At this point, a lookup for a ContentType should hit the DB
            ContentType.objects.get_for_model(ContentType)
        # A second hit, though, won't hit the DB, nor will a lookup by ID
        # or natural key
        with self.assertNumQueries(0):
            ct = ContentType.objects.get_for_model(ContentType)
            ContentType.objects.get_for_id(ct.id)
            ContentType.objects.get_by_natural_key("contenttypes", "contenttype")
        # Once we clear the cache, another lookup will again hit the DB
        # The same should happen with a lookup by natural key
        # And a second hit shouldn't hit the DB
    def test_get_for_models_creation(self):
        ContentType.objects.all().delete()
        with self.assertNumQueries(4):
            cts = ContentType.objects.get_for_models(
                ContentType, FooWithUrl, ProxyModel, ConcreteModel
            cts,
                ContentType: ContentType.objects.get_for_model(ContentType),
                FooWithUrl: ContentType.objects.get_for_model(FooWithUrl),
                ProxyModel: ContentType.objects.get_for_model(ProxyModel),
                ConcreteModel: ContentType.objects.get_for_model(ConcreteModel),
    def test_get_for_models_empty_cache(self):
        # Empty cache.
    def test_get_for_models_partial_cache(self):
        # Partial cache
            cts = ContentType.objects.get_for_models(ContentType, FooWithUrl)
    def test_get_for_models_migrations(self):
        state = ProjectState.from_apps(apps.get_app_config("contenttypes"))
        ContentType = state.apps.get_model("contenttypes", "ContentType")
        cts = ContentType.objects.get_for_models(ContentType)
            cts, {ContentType: ContentType.objects.get_for_model(ContentType)}
    @isolate_apps("contenttypes_tests")
    def test_get_for_models_migrations_create_model(self):
                app_label = "contenttypes_tests"
        state.add_model(ModelState.from_model(Foo))
        cts = ContentType.objects.get_for_models(FooWithUrl, Foo)
                Foo: ContentType.objects.get_for_model(Foo),
    def test_get_for_models_full_cache(self):
        # Full cache
        ContentType.objects.get_for_model(FooWithUrl)
    def test_get_for_model_create_contenttype(self):
        ContentTypeManager.get_for_model() creates the corresponding content
        type if it doesn't exist in the database.
        class ModelCreatedOnTheFly(models.Model):
            name = models.CharField()
        ct = ContentType.objects.get_for_model(ModelCreatedOnTheFly)
        self.assertEqual(ct.app_label, "contenttypes_tests")
        self.assertEqual(ct.model, "modelcreatedonthefly")
        self.assertEqual(str(ct), "contenttypes_tests | modelcreatedonthefly")
    def test_get_for_concrete_model(self):
        Make sure the `for_concrete_model` kwarg correctly works
        with concrete, proxy and deferred models
        concrete_model_ct = ContentType.objects.get_for_model(ConcreteModel)
            concrete_model_ct, ContentType.objects.get_for_model(ProxyModel)
            concrete_model_ct,
            ContentType.objects.get_for_model(ConcreteModel, for_concrete_model=False),
        proxy_model_ct = ContentType.objects.get_for_model(
            ProxyModel, for_concrete_model=False
        self.assertNotEqual(concrete_model_ct, proxy_model_ct)
        # Make sure deferred model are correctly handled
        ConcreteModel.objects.create(name="Concrete")
        DeferredConcreteModel = ConcreteModel.objects.only("pk").get().__class__
        DeferredProxyModel = ProxyModel.objects.only("pk").get().__class__
            concrete_model_ct, ContentType.objects.get_for_model(DeferredConcreteModel)
            ContentType.objects.get_for_model(
                DeferredConcreteModel, for_concrete_model=False
            concrete_model_ct, ContentType.objects.get_for_model(DeferredProxyModel)
            proxy_model_ct,
                DeferredProxyModel, for_concrete_model=False
    def test_get_for_concrete_models(self):
        Make sure the `for_concrete_models` kwarg correctly works
        with concrete, proxy and deferred models.
        cts = ContentType.objects.get_for_models(ConcreteModel, ProxyModel)
                ConcreteModel: concrete_model_ct,
                ProxyModel: concrete_model_ct,
            ConcreteModel, ProxyModel, for_concrete_models=False
                ProxyModel: proxy_model_ct,
            DeferredConcreteModel, DeferredProxyModel
                DeferredConcreteModel: concrete_model_ct,
                DeferredProxyModel: concrete_model_ct,
            DeferredConcreteModel, DeferredProxyModel, for_concrete_models=False
                DeferredProxyModel: proxy_model_ct,
    def test_cache_not_shared_between_managers(self):
        other_manager = ContentTypeManager()
        other_manager.model = ContentType
            other_manager.get_for_model(ContentType)
    def test_missing_model(self):
        Displaying content types in admin (or anywhere) doesn't break on
        leftover content type records in the DB for which no model is defined
        anymore.
        ct = ContentType.objects.create(
            app_label="contenttypes",
            model="OldModel",
        self.assertEqual(str(ct), "contenttypes | OldModel")
        self.assertIsNone(ct.model_class())
        # Stale ContentTypes can be fetched like any other object.
        ct_fetched = ContentType.objects.get_for_id(ct.pk)
        self.assertIsNone(ct_fetched.model_class())
    def test_missing_model_with_existing_model_name(self):
        anymore, even if a model with the same name exists in another app.
        # Create a stale ContentType that matches the name of an existing
        ContentType.objects.create(app_label="contenttypes", model="author")
        # get_for_models() should work as expected for existing models.
        cts = ContentType.objects.get_for_models(ContentType, Author)
                Author: ContentType.objects.get_for_model(Author),
        ct = ContentType.objects.get(app_label="contenttypes_tests", model="site")
        self.assertEqual(str(ct), "Contenttypes_Tests | site")
    def test_str_auth(self):
        ct = ContentType.objects.get(app_label="auth", model="group")
        self.assertEqual(str(ct), "Authentication and Authorization | group")
    def test_name(self):
        self.assertEqual(ct.name, "site")
    def test_app_labeled_name(self):
        self.assertEqual(ct.app_labeled_name, "Contenttypes_Tests | site")
    def test_name_unknown_model(self):
        ct = ContentType(app_label="contenttypes_tests", model="unknown")
        self.assertEqual(ct.name, "unknown")
    def test_app_labeled_name_unknown_model(self):
        self.assertEqual(ct.app_labeled_name, "contenttypes_tests | unknown")
class TestRouter:
    def db_for_read(self, model, **hints):
        return "other"
    def db_for_write(self, model, **hints):
        return "default"
@override_settings(DATABASE_ROUTERS=[TestRouter()])
class ContentTypesMultidbTests(TestCase):
    def test_multidb(self):
        When using multiple databases, ContentType.objects.get_for_model() uses
        db_for_read().
        with (
            self.assertNumQueries(0, using="default"),
            self.assertNumQueries(1, using="other"),
            ContentType.objects.get_for_model(Author)
class GenericPrefetchTests(TestCase):
    def test_querysets_required(self):
            "GenericPrefetch.__init__() missing 1 required "
            "positional argument: 'querysets'"
            GenericPrefetch("question")
    def test_values_queryset(self):
        msg = "Prefetch querysets cannot use raw(), values(), and values_list()."
            GenericPrefetch("question", [Author.objects.values("pk")])
            GenericPrefetch("question", [Author.objects.values_list("pk")])
    def test_raw_queryset(self):
            GenericPrefetch("question", [Author.objects.raw("select pk from author")])
from django.contrib.flatpages.models import FlatPage
from django.test import SimpleTestCase, override_settings
from django.test.utils import override_script_prefix
class FlatpageModelTests(SimpleTestCase):
        self.page = FlatPage(title="Café!", url="/café/")
    def test_get_absolute_url_urlencodes(self):
        self.assertEqual(self.page.get_absolute_url(), "/caf%C3%A9/")
    @override_script_prefix("/prefix/")
    def test_get_absolute_url_honors_script_prefix(self):
        self.assertEqual(self.page.get_absolute_url(), "/prefix/caf%C3%A9/")
        self.assertEqual(str(self.page), "/café/ -- Café!")
    @override_settings(ROOT_URLCONF="flatpages_tests.urls")
    def test_get_absolute_url_include(self):
        self.assertEqual(self.page.get_absolute_url(), "/flatpage_root/caf%C3%A9/")
    @override_settings(ROOT_URLCONF="flatpages_tests.no_slash_urls")
    def test_get_absolute_url_include_no_slash(self):
        self.assertEqual(self.page.get_absolute_url(), "/flatpagecaf%C3%A9/")
    @override_settings(ROOT_URLCONF="flatpages_tests.absolute_urls")
    def test_get_absolute_url_with_hardcoded_url(self):
        fp = FlatPage(title="Test", url="/hardcoded/")
        self.assertEqual(fp.get_absolute_url(), "/flatpage/")
from django.core.checks import Error, Warning
from django.core.checks.model_checks import _check_lazy_references
from django.db import connection, connections, models
from django.db.models.functions import Abs, Lower, Round
from django.db.models.signals import post_init
from django.test import SimpleTestCase, TestCase, skipUnlessDBFeature
from django.test.utils import isolate_apps, override_settings, register_lookup
class EmptyRouter:
def get_max_column_name_length():
    for db in ("default", "other"):
        if max_name_length is not None and not connection.features.truncates_names:
            if allowed_len is None or max_name_length < allowed_len:
    return (allowed_len, db_alias)
@isolate_apps("invalid_models_tests")
class UniqueTogetherTests(SimpleTestCase):
    def test_non_iterable(self):
        class Model(models.Model):
                unique_together = 42
            Model.check(),
                    obj=Model,
    def test_list_containing_non_iterable(self):
            one = models.IntegerField()
            two = models.IntegerField()
                unique_together = [("a", "b"), 42]
    def test_non_list(self):
                unique_together = "not-a-list"
    def test_valid_model(self):
                # unique_together can be a simple tuple
                unique_together = ("one", "two")
        self.assertEqual(Model.check(), [])
    def test_pointing_to_missing_field(self):
                unique_together = [["missing_field"]]
                    "'unique_together' refers to the nonexistent field "
                    "'missing_field'.",
    def test_pointing_to_m2m(self):
            m2m = models.ManyToManyField("self")
                unique_together = [["m2m"]]
                    "'unique_together' refers to a ManyToManyField 'm2m', but "
                    "ManyToManyFields are not permitted in 'unique_together'.",
    def test_pointing_to_fk(self):
            foo_1 = models.ForeignKey(
                Foo, on_delete=models.CASCADE, related_name="bar_1"
            foo_2 = models.ForeignKey(
                Foo, on_delete=models.CASCADE, related_name="bar_2"
                unique_together = [["foo_1_id", "foo_2"]]
        self.assertEqual(Bar.check(), [])
    def test_pointing_to_composite_primary_key(self):
            pk = models.CompositePrimaryKey("version", "name")
            version = models.IntegerField()
                unique_together = [["pk"]]
                    "'unique_together' refers to a CompositePrimaryKey 'pk', but "
                    "CompositePrimaryKeys are not permitted in 'unique_together'.",
    def test_pointing_to_foreign_object(self):
        class Reference(models.Model):
            reference_id = models.IntegerField(unique=True)
        class ReferenceTab(models.Model):
            reference_id = models.IntegerField()
            reference = models.ForeignObject(
                Reference,
                from_fields=["reference_id"],
                to_fields=["reference_id"],
                unique_together = [["reference"]]
        self.assertEqual(ReferenceTab.check(), [])
    def test_pointing_to_foreign_object_multi_column(self):
            code = models.CharField(max_length=1)
        class ReferenceTabMultiple(models.Model):
                from_fields=["reference_id", "code"],
                to_fields=["reference_id", "code"],
            ReferenceTabMultiple.check(),
                    "'unique_together' refers to a ForeignObject 'reference' with "
                    "multiple 'from_fields', which is not supported for that option.",
                    obj=ReferenceTabMultiple,
class IndexesTests(TestCase):
                indexes = [models.Index(fields=["missing_field"], name="name")]
            Model.check(databases=self.databases),
                    "'indexes' refers to the nonexistent field 'missing_field'.",
    def test_pointing_to_desc_field(self):
                indexes = [models.Index(fields=["-name"], name="index_name")]
        self.assertEqual(Model.check(databases=self.databases), [])
    def test_pointing_to_m2m_field(self):
                indexes = [models.Index(fields=["m2m"], name="name")]
                    "'indexes' refers to a ManyToManyField 'm2m', but "
                    "ManyToManyFields are not permitted in 'indexes'.",
    def test_pointing_to_non_local_field(self):
        class Bar(Foo):
            field2 = models.IntegerField()
                indexes = [models.Index(fields=["field2", "field1"], name="name")]
            Bar.check(databases=self.databases),
                    "'indexes' refers to field 'field1' which is not local to "
                    "model 'Bar'.",
                    obj=Bar,
                    models.Index(fields=["foo_1_id", "foo_2"], name="index_name")
        self.assertEqual(Bar.check(databases=self.databases), [])
                indexes = [models.Index(fields=["pk", "name"], name="name")]
                    "'indexes' refers to a CompositePrimaryKey 'pk', but "
                    "CompositePrimaryKeys are not permitted in 'indexes'.",
    def test_name_constraints(self):
                    models.Index(fields=["id"], name="_index_name"),
                    models.Index(fields=["id"], name="5index_name"),
                    "The index name '%sindex_name' cannot start with an "
                    "underscore or a number." % prefix,
                    id="models.E033",
                for prefix in ("_", "5")
    def test_max_name_length(self):
        index_name = "x" * 31
                indexes = [models.Index(fields=["id"], name=index_name)]
                    "The index name '%s' cannot be longer than 30 characters."
                    % index_name,
                    id="models.E034",
    def test_index_with_condition(self):
                    models.Index(
                        fields=["age"],
                        name="index_age_gte_10",
                        condition=models.Q(age__gte=10),
        errors = Model.check(databases=self.databases)
            if connection.features.supports_partial_indexes
            else [
                Warning(
                    "%s does not support indexes with conditions."
                    % connection.display_name,
                        "Conditions will be ignored. Silence this warning if you "
                        "don't care about it."
                    id="models.W037",
        self.assertEqual(errors, expected)
    def test_index_with_condition_required_db_features(self):
    def test_index_with_include(self):
                        name="index_age_include_id",
                        include=["id"],
            if connection.features.supports_covering_indexes
                    "%s does not support indexes with non-key columns."
                        "Non-key columns will be ignored. Silence this warning if "
                        "you don't care about it."
                    id="models.W040",
    def test_index_with_include_required_db_features(self):
                required_db_features = {"supports_covering_indexes"}
    @skipUnlessDBFeature("supports_covering_indexes")
    def test_index_include_pointing_to_missing_field(self):
                    models.Index(fields=["id"], include=["missing_field"], name="name"),
    def test_index_include_pointing_to_m2m_field(self):
                indexes = [models.Index(fields=["id"], include=["m2m"], name="name")]
    def test_index_include_pointing_to_non_local_field(self):
                    models.Index(fields=["field2"], include=["field1"], name="name"),
            Child.check(databases=self.databases),
                    "model 'Child'.",
                    obj=Child,
    def test_index_include_pointing_to_fk(self):
        class Target(models.Model):
            fk_1 = models.ForeignKey(Target, models.CASCADE, related_name="target_1")
            fk_2 = models.ForeignKey(Target, models.CASCADE, related_name="target_2")
                        fields=["id"],
                        include=["fk_1_id", "fk_2"],
    def test_index_include_pointing_to_composite_primary_key(self):
                indexes = [models.Index(fields=["name"], include=["pk"], name="name")]
    def test_func_index(self):
                indexes = [models.Index(Lower("name"), name="index_lower_name")]
        warn = Warning(
            "%s does not support indexes on expressions." % connection.display_name,
                "An index won't be created. Silence this warning if you don't "
                "care about it."
            id="models.W043",
        expected = [] if connection.features.supports_expression_indexes else [warn]
        self.assertEqual(Model.check(databases=self.databases), expected)
    def test_func_index_required_db_features(self):
    @skipUnlessDBFeature("supports_expression_indexes")
    def test_func_index_complex_expression_custom_lookup(self):
            height = models.IntegerField()
            weight = models.IntegerField()
                        models.F("height")
                        / (models.F("weight__abs") + models.Value(5)),
        with register_lookup(models.IntegerField, Abs):
    def test_func_index_pointing_to_missing_field(self):
                indexes = [models.Index(Lower("missing_field").desc(), name="name")]
    def test_func_index_pointing_to_missing_field_nested(self):
                    models.Index(Abs(Round("missing_field")), name="name"),
    def test_func_index_pointing_to_m2m_field(self):
                indexes = [models.Index(Lower("m2m"), name="name")]
    def test_func_index_pointing_to_non_local_field(self):
            field1 = models.CharField(max_length=15)
                indexes = [models.Index(Lower("field1"), name="name")]
    def test_func_index_pointing_to_fk(self):
            foo_1 = models.ForeignKey(Foo, models.CASCADE, related_name="bar_1")
            foo_2 = models.ForeignKey(Foo, models.CASCADE, related_name="bar_2")
                    models.Index(Lower("foo_1_id"), Lower("foo_2"), name="index_name"),
    def test_func_index_pointing_to_composite_primary_key(self):
                indexes = [models.Index(Abs("pk"), name="name")]
class FieldNamesTests(TestCase):
    def test_ending_with_underscore(self):
            field_ = models.CharField(max_length=10)
            m2m_ = models.ManyToManyField("self")
                    obj=Model._meta.get_field("field_"),
                    obj=Model._meta.get_field("m2m_"),
    max_column_name_length, column_limit_db_alias = get_max_column_name_length()
        max_column_name_length is None,
        "The database doesn't have a column name length limit.",
    def test_M2M_long_column_name(self):
        #13711 -- Model check for long M2M column names when database has
        column name length limits.
        # A model with very long name which will be used to set relations to.
        class VeryLongModelNamezzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz(
            models.Model
            title = models.CharField(max_length=11)
        # Main model for which checks will be performed.
        class ModelWithLongField(models.Model):
            m2m_field = models.ManyToManyField(
                VeryLongModelNamezzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz,
                related_name="rn1",
            m2m_field2 = models.ManyToManyField(
                related_name="rn2",
                through="m2msimple",
            m2m_field3 = models.ManyToManyField(
                related_name="rn3",
                through="m2mcomplex",
                related_name="rn4",
        # Models used for setting `through` in M2M field.
        class m2msimple(models.Model):
            id2 = models.ForeignKey(ModelWithLongField, models.CASCADE)
        class m2mcomplex(models.Model):
        long_field_name = "a" * (self.max_column_name_length + 1)
        models.ForeignKey(
        ).contribute_to_class(m2msimple, long_field_name)
            db_column=long_field_name,
        ).contribute_to_class(m2mcomplex, long_field_name)
        errors = ModelWithLongField.check(databases=("default", "other"))
        # First error because of M2M field set on the model with long name.
        m2m_long_name = (
            "verylongmodelnamezzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz_id"
        if self.max_column_name_length > len(m2m_long_name):
            # Some databases support names longer than the test name.
            expected = []
            expected = [
                    'Autogenerated column name too long for M2M field "%s". '
                        m2m_long_name,
                        self.max_column_name_length,
                        self.column_limit_db_alias,
                    hint="Use 'through' to create a separate model for "
                    "M2M and then set column_name using 'db_column'.",
                    obj=ModelWithLongField,
        # Second error because the FK specified in the `through` model
        # `m2msimple` has auto-generated name longer than allowed.
        # There will be no check errors in the other M2M because it
        # specifies db_column for the FK in `through` model even if the actual
        # name is longer than the limits of the database.
        expected.append(
                'Autogenerated column name too long for M2M field "%s_id". '
                    long_field_name,
        # Check for long column names is called only for specified database
        # aliases.
        self.assertEqual(ModelWithLongField.check(databases=None), [])
    def test_local_field_long_column_name(self):
        #13711 -- Model check for long column names
        when database does not support long names.
        long_field_name2 = "b" * (self.max_column_name_length + 1)
        models.CharField(max_length=11).contribute_to_class(
            ModelWithLongField, long_field_name
        models.CharField(max_length=11, db_column="vlmn").contribute_to_class(
            ModelWithLongField, long_field_name2
            ModelWithLongField.check(databases=("default", "other")),
    def test_including_separator(self):
            some__field = models.IntegerField()
                    'Field names must not contain "__".',
                    obj=Model._meta.get_field("some__field"),
    def test_pk(self):
            pk = models.IntegerField()
                    obj=Model._meta.get_field("pk"),
    def test_db_column_clash(self):
            foo = models.IntegerField()
            bar = models.IntegerField(db_column="foo")
                    "Field 'bar' has column name 'foo' that is used by "
                    "another field.",
class ShadowingFieldsTests(SimpleTestCase):
    def test_field_name_clash_with_child_accessor(self):
            child = models.CharField(max_length=100)
            Child.check(),
                    "The field 'child' clashes with the field "
                    "'child' from model 'invalid_models_tests.parent'.",
                    obj=Child._meta.get_field("child"),
    def test_field_name_clash_with_m2m_through(self):
            clash_id = models.IntegerField()
            clash = models.ForeignKey("Child", models.CASCADE)
            parents = models.ManyToManyField(
                to=Parent,
                through="Through",
                through_fields=["parent", "model"],
        class Through(models.Model):
            parent = models.ForeignKey(Parent, models.CASCADE)
            model = models.ForeignKey(Model, models.CASCADE)
                    "The field 'clash' clashes with the field 'clash_id' from "
                    "model 'invalid_models_tests.parent'.",
                    obj=Child._meta.get_field("clash"),
    def test_multiinheritance_clash(self):
        class Mother(models.Model):
            clash = models.IntegerField()
        class Father(models.Model):
        class Child(Mother, Father):
            # Here we have two clashed: id (automatic field) and clash, because
            # both parents define these fields.
                    "The field 'id' from parent model "
                    "'invalid_models_tests.mother' clashes with the field 'id' "
                    "from parent model 'invalid_models_tests.father'.",
                    "The field 'clash' from parent model "
                    "'invalid_models_tests.mother' clashes with the field 'clash' "
    def test_inheritance_clash(self):
            f_id = models.IntegerField()
            # This field doesn't result in a clash.
            # This field clashes with parent "f_id" field.
            f = models.ForeignKey(Target, models.CASCADE)
                    "The field 'f' clashes with the field 'f_id' "
                    "from model 'invalid_models_tests.parent'.",
                    obj=Child._meta.get_field("f"),
    def test_multigeneration_inheritance(self):
        class GrandParent(models.Model):
        class Parent(GrandParent):
        class GrandChild(Child):
            GrandChild.check(),
                    "The field 'clash' clashes with the field 'clash' "
                    "from model 'invalid_models_tests.grandparent'.",
                    obj=GrandChild._meta.get_field("clash"),
    def test_diamond_mti_common_parent(self):
        class MTICommonParentModel(Child, GrandParent):
            MTICommonParentModel.check(),
                    "The field 'grandparent_ptr' clashes with the field "
                    "'grandparent_ptr' from model 'invalid_models_tests.parent'.",
                    obj=MTICommonParentModel,
    def test_id_clash(self):
            fk = models.ForeignKey(Target, models.CASCADE)
            fk_id = models.IntegerField()
                    "The field 'fk_id' clashes with the field 'fk' from model "
                    "'invalid_models_tests.model'.",
                    obj=Model._meta.get_field("fk_id"),
class OtherModelTests(SimpleTestCase):
    def test_unique_primary_key(self):
        invalid_id = models.IntegerField(primary_key=False)
            id = invalid_id
                    "'id' can only be used as a field name if the field also sets "
    def test_ordering_non_iterable(self):
                ordering = "missing_field"
                    "'ordering' must be a tuple or list "
                    "(even if you want to order by only one field).",
    def test_just_ordering_no_errors(self):
                ordering = ["order"]
    def test_just_order_with_respect_to_no_errors(self):
            question = models.ForeignKey(Question, models.CASCADE)
        self.assertEqual(Answer.check(), [])
    def test_ordering_with_order_with_respect_to(self):
            Answer.check(),
                    obj=Answer,
    def test_non_valid(self):
        class RelationModel(models.Model):
            relation = models.ManyToManyField(RelationModel)
                ordering = ["relation"]
                    "'ordering' refers to the nonexistent field, related field, "
                    "or lookup 'relation'.",
    def test_ordering_pointing_to_missing_field(self):
                ordering = ("missing_field",)
                    "or lookup 'missing_field'.",
    def test_ordering_pointing_to_missing_foreignkey_field(self):
            missing_fk_field = models.IntegerField()
                ordering = ("missing_fk_field_id",)
                    "or lookup 'missing_fk_field_id'.",
    def test_ordering_pointing_to_missing_related_field(self):
            test = models.IntegerField()
                ordering = ("missing_related__id",)
                    "or lookup 'missing_related__id'.",
    def test_ordering_pointing_to_missing_related_model_field(self):
                ordering = ("parent__missing_field",)
                    "or lookup 'parent__missing_field'.",
    def test_ordering_pointing_to_non_related_field(self):
            parent = models.IntegerField()
    def test_ordering_pointing_to_two_related_model_field(self):
        class Parent2(models.Model):
        class Parent1(models.Model):
            parent2 = models.ForeignKey(Parent2, models.CASCADE)
            parent1 = models.ForeignKey(Parent1, models.CASCADE)
                ordering = ("parent1__parent2__missing_field",)
                    "or lookup 'parent1__parent2__missing_field'.",
    def test_ordering_pointing_multiple_times_to_model_fields(self):
            field1 = models.CharField(max_length=100)
            field2 = models.CharField(max_length=100)
                ordering = ("parent__field1__field2",)
                    "or lookup 'parent__field1__field2'.",
    def test_ordering_allows_registered_lookups(self):
            test = models.CharField(max_length=100)
                ordering = ("test__lower",)
        with register_lookup(models.CharField, Lower):
    def test_ordering_pointing_to_lookup_not_transform(self):
                ordering = ("test__isnull",)
    def test_ordering_pointing_to_related_model_pk(self):
                ordering = ("parent__pk",)
        self.assertEqual(Child.check(), [])
    def test_ordering_pointing_to_foreignkey_field(self):
                ordering = ("parent_id",)
        self.assertFalse(Child.check())
    def test_name_beginning_with_underscore(self):
        class _Model(models.Model):
            _Model.check(),
                    "The model name '_Model' cannot start or end with an underscore "
                    "as it collides with the query lookup syntax.",
                    obj=_Model,
    def test_name_ending_with_underscore(self):
        class Model_(models.Model):
            Model_.check(),
                    "The model name 'Model_' cannot start or end with an underscore "
                    obj=Model_,
    def test_name_contains_double_underscores(self):
        class Test__Model(models.Model):
            Test__Model.check(),
                    "The model name 'Test__Model' cannot contain double underscores "
                    obj=Test__Model,
    def test_property_and_related_field_accessor_clash(self):
            fk = models.ForeignKey("self", models.CASCADE)
        # Override related field accessor.
        Model.fk_id = property(lambda self: "ERROR")
                    "The property 'fk_id' clashes with a related field accessor.",
    def test_inherited_overriden_property_no_clash(self):
        class Cheese:
            def filling_id(self):
        class Sandwich(Cheese, models.Model):
            filling = models.ForeignKey("self", models.CASCADE)
        self.assertEqual(Sandwich.check(), [])
    def test_single_primary_key(self):
            foo = models.IntegerField(primary_key=True)
            bar = models.IntegerField(primary_key=True)
    @override_settings(TEST_SWAPPED_MODEL_BAD_VALUE="not-a-model")
    def test_swappable_missing_app_name(self):
                swappable = "TEST_SWAPPED_MODEL_BAD_VALUE"
                    "'TEST_SWAPPED_MODEL_BAD_VALUE' is not of the form "
                    "'app_label.app_name'.",
    @override_settings(TEST_SWAPPED_MODEL_BAD_MODEL="not_an_app.Target")
    def test_swappable_missing_app(self):
                swappable = "TEST_SWAPPED_MODEL_BAD_MODEL"
                    "'TEST_SWAPPED_MODEL_BAD_MODEL' references 'not_an_app.Target', "
                    "which has not been installed, or is abstract.",
    def test_two_m2m_through_same_relationship(self):
            primary = models.ManyToManyField(
                Person, through="Membership", related_name="primary"
            secondary = models.ManyToManyField(
                Person, through="Membership", related_name="secondary"
            Group.check(),
                    "The model has two identical many-to-many relations through "
                    "the intermediate model 'invalid_models_tests.Membership'.",
                    obj=Group,
    def test_two_m2m_through_same_model_with_different_through_fields(self):
        class ShippingMethod(models.Model):
            to_countries = models.ManyToManyField(
                through="ShippingMethodPrice",
                through_fields=("method", "to_country"),
            from_countries = models.ManyToManyField(
                through_fields=("method", "from_country"),
        class ShippingMethodPrice(models.Model):
            method = models.ForeignKey(ShippingMethod, models.CASCADE)
            to_country = models.ForeignKey(Country, models.CASCADE)
            from_country = models.ForeignKey(Country, models.CASCADE)
        self.assertEqual(ShippingMethod.check(), [])
    def test_onetoone_with_parent_model(self):
        class ParkingLot(Place):
            other_place = models.OneToOneField(
                Place, models.CASCADE, related_name="other_parking"
        self.assertEqual(ParkingLot.check(), [])
    def test_onetoone_with_explicit_parent_link_parent_model(self):
            place = models.OneToOneField(
                Place, models.CASCADE, parent_link=True, primary_key=True
    def test_m2m_table_name_clash(self):
            bar = models.ManyToManyField("Bar", db_table="myapp_bar")
                db_table = "myapp_foo"
                db_table = "myapp_bar"
            Foo.check(),
                    "The field's intermediary table 'myapp_bar' clashes with the "
                    "table name of 'invalid_models_tests.Bar'.",
                    obj=Foo._meta.get_field("bar"),
                    id="fields.E340",
        DATABASE_ROUTERS=["invalid_models_tests.test_models.EmptyRouter"]
    def test_m2m_table_name_clash_database_routers_installed(self):
                        "You have configured settings.DATABASE_ROUTERS. Verify "
                        "that the table of 'invalid_models_tests.Bar' is "
                        "correctly routed to a separate database."
                    id="fields.W344",
    def test_m2m_field_table_name_clash(self):
            foos = models.ManyToManyField(Foo, db_table="clash")
        class Baz(models.Model):
            Bar.check() + Baz.check(),
                    "The field's intermediary table 'clash' clashes with the "
                    "table name of 'invalid_models_tests.Baz.foos'.",
                    obj=Bar._meta.get_field("foos"),
                    "table name of 'invalid_models_tests.Bar.foos'.",
                    obj=Baz._meta.get_field("foos"),
    def test_m2m_field_table_name_clash_database_routers_installed(self):
                    "table name of 'invalid_models_tests.%s.foos'." % clashing_model,
                    obj=model_cls._meta.get_field("foos"),
                        "that the table of 'invalid_models_tests.%s.foos' is "
                        "correctly routed to a separate database." % clashing_model
                for model_cls, clashing_model in [(Bar, "Baz"), (Baz, "Bar")]
    def test_m2m_autogenerated_table_name_clash(self):
                db_table = "bar_foos"
            # The autogenerated `db_table` will be bar_foos.
            foos = models.ManyToManyField(Foo)
                db_table = "bar"
            Bar.check(),
                    "The field's intermediary table 'bar_foos' clashes with the "
                    "table name of 'invalid_models_tests.Foo'.",
    def test_m2m_autogenerated_table_name_clash_database_routers_installed(self):
            # The autogenerated db_table is bar_foos.
                        "that the table of 'invalid_models_tests.Foo' is "
    def test_m2m_unmanaged_shadow_models_not_checked(self):
        class A1(models.Model):
        class C1(models.Model):
            mm_a = models.ManyToManyField(A1, db_table="d1")
        # Unmanaged models that shadow the above models. Reused table names
        # shouldn't be flagged by any checks.
        class A2(models.Model):
        class C2(models.Model):
            mm_a = models.ManyToManyField(A2, through="Intermediate")
        class Intermediate(models.Model):
            a2 = models.ForeignKey(A2, models.CASCADE, db_column="a1_id")
            c2 = models.ForeignKey(C2, models.CASCADE, db_column="c1_id")
                db_table = "d1"
        self.assertEqual(C1.check(), [])
        self.assertEqual(C2.check(), [])
    def test_m2m_to_concrete_and_proxy_allowed(self):
            a = models.ForeignKey("A", models.CASCADE)
            c = models.ForeignKey("C", models.CASCADE)
        class ThroughProxy(Through):
            mm_a = models.ManyToManyField(A, through=Through)
            mm_aproxy = models.ManyToManyField(
                A, through=ThroughProxy, related_name="proxied_m2m"
        self.assertEqual(C.check(), [])
    @isolate_apps("django.contrib.auth", kwarg_name="apps")
    def test_lazy_reference_checks(self, apps):
        class DummyModel(models.Model):
            author = models.ForeignKey("Author", models.CASCADE)
                app_label = "invalid_models_tests"
        class DummyClass:
            def __call__(self, **kwargs):
            def dummy_method(self):
        def dummy_function(*args, **kwargs):
        apps.lazy_model_operation(dummy_function, ("auth", "imaginarymodel"))
        apps.lazy_model_operation(dummy_function, ("fanciful_app", "imaginarymodel"))
        post_init.connect(dummy_function, sender="missing-app.Model", apps=apps)
        post_init.connect(DummyClass(), sender="missing-app.Model", apps=apps)
        post_init.connect(
            DummyClass().dummy_method, sender="missing-app.Model", apps=apps
            _check_lazy_references(apps),
                    "%r contains a lazy reference to auth.imaginarymodel, "
                    "but app 'auth' doesn't provide model 'imaginarymodel'."
                    % dummy_function,
                    obj=dummy_function,
                    id="models.E022",
                    "%r contains a lazy reference to fanciful_app.imaginarymodel, "
                    "but app 'fanciful_app' isn't installed." % dummy_function,
                    "An instance of class 'DummyClass' was connected to "
                    "the 'post_init' signal with a lazy reference to the sender "
                    "'missing-app.model', but app 'missing-app' isn't installed.",
                    hint=None,
                    obj="invalid_models_tests.test_models",
                    id="signals.E001",
                    "Bound method 'DummyClass.dummy_method' was connected to the "
                    "'post_init' signal with a lazy reference to the sender "
                    "The field invalid_models_tests.DummyModel.author was declared "
                    "with a lazy reference to 'invalid_models_tests.author', but app "
                    "'invalid_models_tests' isn't installed.",
                    obj=DummyModel.author.field,
                    id="fields.E307",
                    "The function 'dummy_function' was connected to the 'post_init' "
                    "signal with a lazy reference to the sender "
class DbTableCommentTests(TestCase):
    def test_db_table_comment(self):
                db_table_comment = "Table comment"
            if connection.features.supports_comments
                    f"{connection.display_name} does not support comments on tables "
                    f"(db_table_comment).",
    def test_db_table_comment_required_db_features(self):
class MultipleAutoFieldsTests(TestCase):
    def test_multiple_autofields(self):
            "Model invalid_models_tests.MultipleAutoFields can't have more "
            "than one auto-generated field."
            class MultipleAutoFields(models.Model):
                auto1 = models.AutoField(primary_key=True)
                auto2 = models.AutoField(primary_key=True)
class JSONFieldTests(TestCase):
    @skipUnlessDBFeature("supports_json_field")
    def test_ordering_pointing_to_json_field_value(self):
            field = models.JSONField()
                ordering = ["field__value"]
    def test_check_jsonfield(self):
        error = Error(
            "%s does not support JSONFields." % connection.display_name,
            id="fields.E180",
        expected = [] if connection.features.supports_json_field else [error]
    def test_check_jsonfield_required_db_features(self):
class ConstraintsTests(TestCase):
    def test_check_constraints(self):
                        condition=models.Q(age__gte=18), name="is_adult"
            "%s does not support check constraints." % connection.display_name,
                "A constraint won't be created. Silence this warning if you "
            id="models.W027",
            [] if connection.features.supports_table_check_constraints else [warn]
        self.assertCountEqual(errors, expected)
    def test_check_constraints_required_db_features(self):
                required_db_features = {"supports_table_check_constraints"}
    def test_check_constraint_pointing_to_missing_field(self):
                        condition=models.Q(missing_field=2),
                        "'constraints' refers to the nonexistent field "
                if connection.features.supports_table_check_constraints
                else []
    @skipUnlessDBFeature("supports_table_check_constraints")
    def test_check_constraint_pointing_to_reverse_fk(self):
            parent = models.ForeignKey("self", models.CASCADE, related_name="parents")
                    models.CheckConstraint(name="name", condition=models.Q(parents=3)),
                    "'constraints' refers to the nonexistent field 'parents'.",
    def test_check_constraint_pointing_to_reverse_o2o(self):
            parent = models.OneToOneField("self", models.CASCADE)
                        condition=models.Q(model__isnull=True),
                    "'constraints' refers to the nonexistent field 'model'.",
    def test_check_constraint_pointing_to_m2m_field(self):
                    models.CheckConstraint(name="name", condition=models.Q(m2m=2)),
                    "'constraints' refers to a ManyToManyField 'm2m', but "
                    "ManyToManyFields are not permitted in 'constraints'.",
    def test_check_constraint_pointing_to_fk(self):
                        condition=models.Q(fk_1_id=2) | models.Q(fk_2=2),
    def test_check_constraint_pointing_to_pk(self):
            age = models.SmallIntegerField()
                        condition=models.Q(pk__gt=5) & models.Q(age__gt=models.F("pk")),
    def test_check_constraint_pointing_to_non_local_field(self):
                    models.CheckConstraint(name="name", condition=models.Q(field1=1)),
                    "'constraints' refers to field 'field1' which is not local to "
    def test_check_constraint_pointing_to_joined_fields(self):
            field1 = models.PositiveSmallIntegerField()
            field2 = models.PositiveSmallIntegerField()
            field3 = models.PositiveSmallIntegerField()
            previous = models.OneToOneField("self", models.CASCADE, related_name="next")
                        name="name1",
                            field1__lt=models.F("parent__field1")
                            + models.F("parent__field2")
                        name="name2", condition=models.Q(name=Lower("parent__name"))
                        name="name3",
                        condition=models.Q(parent__field3=models.F("field1")),
                        name="name4",
                        condition=models.Q(name=Lower("previous__name")),
        joined_fields = [
            "parent__field1",
            "parent__field2",
            "parent__field3",
            "parent__name",
            "previous__name",
        expected_errors = [
                "'constraints' refers to the joined field '%s'." % field_name,
                id="models.E041",
            for field_name in joined_fields
        self.assertCountEqual(errors, expected_errors)
    def test_check_constraint_pointing_to_joined_fields_complex_check(self):
            name = models.PositiveSmallIntegerField()
                                models.Q(name="test")
                                & models.Q(field1__lt=models.F("parent__field1"))
                                models.Q(name__startswith=Lower("parent__name"))
                                & models.Q(
                                    field1__gte=(
                                        models.F("parent__field1")
                        | (models.Q(name="test1")),
        joined_fields = ["parent__field1", "parent__field2", "parent__name"]
    def test_check_constraint_raw_sql_check(self):
                        condition=models.Q(id__gt=0), name="q_check"
                        condition=models.ExpressionWrapper(
                            models.Q(price__gt=20),
                            output_field=models.BooleanField(),
                        name="expression_wrapper_check",
                        condition=models.expressions.RawSQL(
                            "id = 0",
                        name="raw_sql_check",
                            models.ExpressionWrapper(
                                models.Q(
                                    models.expressions.RawSQL(
                        name="nested_raw_sql_check",
        expected_warnings = (
                    "Check constraint 'raw_sql_check' contains RawSQL() expression and "
                    "won't be validated during the model full_clean().",
                    hint="Silence this warning if you don't care about it.",
                    id="models.W045",
                    "Check constraint 'nested_raw_sql_check' contains RawSQL() "
                    "expression and won't be validated during the model full_clean().",
        self.assertEqual(Model.check(databases=self.databases), expected_warnings)
    def test_check_constraint_pointing_to_composite_primary_key(self):
                        condition=models.Q(pk__gt=(7, "focal")),
                    "'constraints' refers to a CompositePrimaryKey 'pk', but "
                    "CompositePrimaryKeys are not permitted in 'constraints'.",
    def test_unique_constraint_with_condition(self):
                        name="unique_age_gte_100",
                        condition=models.Q(age__gte=100),
                    "%s does not support unique constraints with conditions."
                        "A constraint won't be created. Silence this warning if "
                    id="models.W036",
    def test_unique_constraint_with_condition_required_db_features(self):
    def test_unique_constraint_condition_pointing_to_missing_field(self):
    def test_unique_constraint_condition_pointing_to_joined_fields(self):
                        condition=models.Q(parent__age__lt=2),
                        "'constraints' refers to the joined field 'parent__age__lt'.",
    def test_unique_constraint_pointing_to_reverse_o2o(self):
                        fields=["parent"],
    def test_deferrable_unique_constraint(self):
                        name="unique_age_deferrable",
            if connection.features.supports_deferrable_unique_constraints
                    "%s does not support deferrable unique constraints."
                    id="models.W038",
    def test_deferrable_unique_constraint_required_db_features(self):
                required_db_features = {"supports_deferrable_unique_constraints"}
    def test_unique_constraint_pointing_to_missing_field(self):
                    models.UniqueConstraint(fields=["missing_field"], name="name")
                    "'constraints' refers to the nonexistent field 'missing_field'.",
    def test_unique_constraint_pointing_to_m2m_field(self):
                constraints = [models.UniqueConstraint(fields=["m2m"], name="name")]
    def test_unique_constraint_pointing_to_non_local_field(self):
                    models.UniqueConstraint(fields=["field2", "field1"], name="name"),
    def test_unique_constraint_pointing_to_fk(self):
                    models.UniqueConstraint(fields=["fk_1_id", "fk_2"], name="name"),
    def test_unique_constraint_pointing_to_composite_primary_key(self):
                constraints = [models.UniqueConstraint(fields=["pk"], name="name")]
    def test_unique_constraint_with_include(self):
                        name="unique_age_include_id",
                    "%s does not support unique constraints with non-key columns."
                    id="models.W039",
    def test_unique_constraint_with_include_required_db_features(self):
    def test_unique_constraint_include_pointing_to_missing_field(self):
                        include=["missing_field"],
    def test_unique_constraint_include_pointing_to_m2m_field(self):
                        include=["m2m"],
    def test_unique_constraint_include_pointing_to_non_local_field(self):
                        fields=["field2"],
                        include=["field1"],
    def test_unique_constraint_include_pointing_to_fk(self):
    def test_unique_constraint_include_pointing_to_composite_primary_key(self):
                        fields=["version"],
                        include=["pk"],
    def test_func_unique_constraint(self):
                    models.UniqueConstraint(Lower("name"), name="lower_name_uq"),
            "%s does not support unique constraints on expressions."
            id="models.W044",
    def test_func_unique_constraint_required_db_features(self):
                    models.UniqueConstraint(Lower("name"), name="lower_name_unq"),
    def test_unique_constraint_nulls_distinct(self):
                        name="name_uq_distinct_null",
                        nulls_distinct=True,
            f"{connection.display_name} does not support "
            "UniqueConstraint.nulls_distinct.",
                "A constraint won't be created. Silence this warning if you don't care "
                "about it."
            id="models.W047",
            if connection.features.supports_nulls_distinct_unique_constraints
            else [warn]
    def test_unique_constraint_nulls_distinct_required_db_features(self):
                required_db_features = {"supports_nulls_distinct_unique_constraints"}
    def test_func_unique_constraint_expression_custom_lookup(self):
    def test_func_unique_constraint_pointing_to_missing_field(self):
                    models.UniqueConstraint(Lower("missing_field").desc(), name="name"),
    def test_func_unique_constraint_pointing_to_missing_field_nested(self):
                    models.UniqueConstraint(Abs(Round("missing_field")), name="name"),
    def test_func_unique_constraint_pointing_to_m2m_field(self):
                constraints = [models.UniqueConstraint(Lower("m2m"), name="name")]
    def test_func_unique_constraint_pointing_to_non_local_field(self):
                constraints = [models.UniqueConstraint(Lower("field1"), name="name")]
    def test_func_unique_constraint_pointing_to_fk(self):
            id = models.CharField(primary_key=True, max_length=255)
                        Lower("foo_1_id"),
                        Lower("foo_2"),
    def test_func_unique_constraint_pointing_composite_primary_key(self):
                constraints = [models.UniqueConstraint(Abs("pk"), name="name")]
class RelatedFieldTests(SimpleTestCase):
    def test_on_delete_python_db_variants(self):
            artist = models.ForeignKey(Artist, models.CASCADE)
            album = models.ForeignKey(Album, models.RESTRICT)
            artist = models.ForeignKey(Artist, models.DB_CASCADE)
            Song.check(databases=self.databases),
                    "The model cannot have related fields with both database-level and "
                    "Python-level on_delete variants.",
                    obj=Song,
    def test_on_delete_python_db_variants_auto_created(self):
        class SharedModel(models.Model):
        class Child(SharedModel):
            parent = models.ForeignKey(Parent, on_delete=models.DB_CASCADE)
    def test_on_delete_db_do_nothing(self):
            album = models.ForeignKey(Album, models.DO_NOTHING)
        self.assertEqual(Song.check(databases=self.databases), [])
#!/usr/bin/env python3`
from tests import get_tests_data_path, get_tests_output_path, run_cli
from TTS.tts.utils.languages import LanguageManager
from TTS.tts.utils.speakers import SpeakerManager
MODELS_WITH_SEP_TESTS = [
    "tts_models/multilingual/multi-dataset/bark",
    "tts_models/en/multi-dataset/tortoise-v2",
    "tts_models/multilingual/multi-dataset/xtts_v1.1",
    "tts_models/multilingual/multi-dataset/xtts_v2",
def run_models(offset=0, step=1):
    """Check if all the models are downloadable and tts models run correctly."""
    print(" > Run synthesizer with all the models.")
    output_path = os.path.join(get_tests_output_path(), "output.wav")
    manager = ModelManager(output_prefix=get_tests_output_path(), progress_bar=False)
    model_names = [name for name in manager.list_models() if name not in MODELS_WITH_SEP_TESTS]
    print("Model names:", model_names)
    for model_name in model_names[offset::step]:
        print(f"\n > Run - {model_name}")
        model_path, _, _ = manager.download_model(model_name)
            local_download_dir = os.path.dirname(model_path)
            # download and run the model
            speaker_files = glob.glob(local_download_dir + "/speaker*")
            language_files = glob.glob(local_download_dir + "/language*")
            language_id = ""
            if len(speaker_files) > 0:
                # multi-speaker model
                if "speaker_ids" in speaker_files[0]:
                    speaker_manager = SpeakerManager(speaker_id_file_path=speaker_files[0])
                elif "speakers" in speaker_files[0]:
                    speaker_manager = SpeakerManager(d_vectors_file_path=speaker_files[0])
                # multi-lingual model - Assuming multi-lingual models are also multi-speaker
                if len(language_files) > 0 and "language_ids" in language_files[0]:
                    language_manager = LanguageManager(language_ids_file_path=language_files[0])
                    language_id = language_manager.language_names[0]
                speaker_id = list(speaker_manager.name_to_id.keys())[0]
                run_cli(
                    f"tts --model_name  {model_name} "
                    f'--text "This is an example." --out_path "{output_path}" --speaker_idx "{speaker_id}" --language_idx "{language_id}" --progress_bar False'
                # single-speaker model
                    f'--text "This is an example." --out_path "{output_path}" --progress_bar False'
            # remove downloaded models
            shutil.rmtree(local_download_dir)
            shutil.rmtree(get_user_data_dir("tts"))
            speaker_wav = os.path.join(get_tests_data_path(), "ljspeech", "wavs", "LJ001-0001.wav")
            reference_wav = os.path.join(get_tests_data_path(), "ljspeech", "wavs", "LJ001-0032.wav")
                f'--out_path "{output_path}" --source_wav "{speaker_wav}" --target_wav "{reference_wav}" --progress_bar False'
            # only download the model
            manager.download_model(model_name)
        print(f" | > OK: {model_name}")
def test_xtts():
    """XTTS is too big to run on github actions. We need to test it locally"""
    use_gpu = torch.cuda.is_available()
    if use_gpu:
            "yes | "
            f"tts --model_name  tts_models/multilingual/multi-dataset/xtts_v1.1 "
            f'--text "This is an example." --out_path "{output_path}" --progress_bar False --use_cuda True '
            f'--speaker_wav "{speaker_wav}" --language_idx "en"'
            f'--text "This is an example." --out_path "{output_path}" --progress_bar False '
def test_xtts_streaming():
    """Testing the new inference_stream method"""
    from TTS.tts.models.xtts import Xtts
    speaker_wav = [os.path.join(get_tests_data_path(), "ljspeech", "wavs", "LJ001-0001.wav")]
    speaker_wav_2 = os.path.join(get_tests_data_path(), "ljspeech", "wavs", "LJ001-0002.wav")
    speaker_wav.append(speaker_wav_2)
    model_path = os.path.join(get_user_data_dir("tts"), "tts_models--multilingual--multi-dataset--xtts_v1.1")
    config = XttsConfig()
    config.load_json(os.path.join(model_path, "config.json"))
    model = Xtts.init_from_config(config)
    model.load_checkpoint(config, checkpoint_dir=model_path)
    model.to(torch.device("cuda" if torch.cuda.is_available() else "cpu"))
    print("Computing speaker latents...")
    gpt_cond_latent, speaker_embedding = model.get_conditioning_latents(audio_path=speaker_wav)
    print("Inference...")
    chunks = model.inference_stream(
        "It took me quite a long time to develop a voice and now that I have it I am not going to be silent.",
        "en",
        gpt_cond_latent,
        speaker_embedding,
    wav_chuncks = []
    for i, chunk in enumerate(chunks):
            assert chunk.shape[-1] > 5000
        wav_chuncks.append(chunk)
    assert len(wav_chuncks) > 1
def test_xtts_v2():
            f"tts --model_name  tts_models/multilingual/multi-dataset/xtts_v2 "
            f'--speaker_wav "{speaker_wav}" "{speaker_wav_2}"  --language_idx "en"'
            f'--speaker_wav "{speaker_wav}" "{speaker_wav_2}" --language_idx "en"'
def test_xtts_v2_streaming():
    model_path = os.path.join(get_user_data_dir("tts"), "tts_models--multilingual--multi-dataset--xtts_v2")
    normal_len = sum([len(chunk) for chunk in wav_chuncks])
        speed=1.5,
    fast_len = sum([len(chunk) for chunk in wav_chuncks])
        speed=0.66,
    slow_len = sum([len(chunk) for chunk in wav_chuncks])
    assert slow_len > normal_len
    assert normal_len > fast_len
def test_tortoise():
            f" tts --model_name  tts_models/en/multi-dataset/tortoise-v2 "
            f'--text "This is an example." --out_path "{output_path}" --progress_bar False --use_cuda True'
def test_bark():
    """Bark is too big to run on github actions. We need to test it locally"""
            f" tts --model_name  tts_models/multilingual/multi-dataset/bark "
def test_voice_conversion():
    print(" > Run voice conversion inference using YourTTS model.")
    model_name = "tts_models/multilingual/multi-dataset/your_tts"
    language_id = "en"
        f"tts --model_name  {model_name}"
        f" --out_path {output_path} --speaker_wav {speaker_wav} --reference_wav {reference_wav} --language_idx {language_id} --progress_bar False"
These are used to split tests into different actions on Github.
def test_models_offset_0_step_3():
    run_models(offset=0, step=3)
def test_models_offset_1_step_3():
    run_models(offset=1, step=3)
def test_models_offset_2_step_3():
    run_models(offset=2, step=3)
