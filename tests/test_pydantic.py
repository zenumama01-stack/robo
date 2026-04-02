from pydantic import Field, BaseModel
from inline_snapshot import snapshot
from openai.lib._pydantic import to_strict_json_schema
from .schema_types.query import Query
def test_most_types() -> None:
        assert openai.pydantic_function_tool(Query)["function"] == snapshot(
                "name": "Query",
                    "$defs": {
                        "Column": {
                            "enum": [
                                "status",
                                "expected_delivery_date",
                                "delivered_at",
                                "shipped_at",
                                "ordered_at",
                                "canceled_at",
                            "title": "Column",
                        "Condition": {
                                "column": {"title": "Column", "type": "string"},
                                "operator": {"$ref": "#/$defs/Operator"},
                                    "anyOf": [
                                        {"type": "string"},
                                        {"type": "integer"},
                                        {"$ref": "#/$defs/DynamicValue"},
                                    "title": "Value",
                            "required": ["column", "operator", "value"],
                            "title": "Condition",
                        "DynamicValue": {
                            "properties": {"column_name": {"title": "Column Name", "type": "string"}},
                            "required": ["column_name"],
                            "title": "DynamicValue",
                        "Operator": {"enum": ["=", ">", "<", "<=", ">=", "!="], "title": "Operator", "type": "string"},
                        "OrderBy": {"enum": ["asc", "desc"], "title": "OrderBy", "type": "string"},
                        "Table": {"enum": ["orders", "customers", "products"], "title": "Table", "type": "string"},
                        "name": {"anyOf": [{"type": "string"}, {"type": "null"}], "title": "Name"},
                        "table_name": {"$ref": "#/$defs/Table"},
                        "columns": {
                            "items": {"$ref": "#/$defs/Column"},
                            "title": "Columns",
                        "conditions": {
                            "items": {"$ref": "#/$defs/Condition"},
                            "title": "Conditions",
                        "order_by": {"$ref": "#/$defs/OrderBy"},
                    "required": ["name", "table_name", "columns", "conditions", "order_by"],
                    "title": "Query",
                        "name": {"title": "Name", "type": "string"},
                        "table_name": {"$ref": "#/definitions/Table"},
                        "columns": {"type": "array", "items": {"$ref": "#/definitions/Column"}},
                            "items": {"$ref": "#/definitions/Condition"},
                        "order_by": {"$ref": "#/definitions/OrderBy"},
                    "definitions": {
                        "Table": {
                            "title": "Table",
                            "description": "An enumeration.",
                            "enum": ["orders", "customers", "products"],
                        "Operator": {
                            "title": "Operator",
                            "enum": ["=", ">", "<", "<=", ">=", "!="],
                                "operator": {"$ref": "#/definitions/Operator"},
                                        {"$ref": "#/definitions/DynamicValue"},
                        "OrderBy": {
                            "title": "OrderBy",
                            "enum": ["asc", "desc"],
class Color(Enum):
    RED = "red"
    BLUE = "blue"
    GREEN = "green"
class ColorDetection(BaseModel):
    color: Color = Field(description="The detected color")
    hex_color_code: str = Field(description="The hex color code of the detected color")
def test_enums() -> None:
        assert openai.pydantic_function_tool(ColorDetection)["function"] == snapshot(
                "name": "ColorDetection",
                    "$defs": {"Color": {"enum": ["red", "blue", "green"], "title": "Color", "type": "string"}},
                        "color": {
                            "description": "The detected color",
                            "enum": ["red", "blue", "green"],
                            "title": "Color",
                        "hex_color_code": {
                            "description": "The hex color code of the detected color",
                            "title": "Hex Color Code",
                    "required": ["color", "hex_color_code"],
                    "title": "ColorDetection",
                        "Color": {"title": "Color", "description": "An enumeration.", "enum": ["red", "blue", "green"]}
class Star(BaseModel):
    name: str = Field(description="The name of the star.")
class Galaxy(BaseModel):
    name: str = Field(description="The name of the galaxy.")
    largest_star: Star = Field(description="The largest star in the galaxy.")
class Universe(BaseModel):
    name: str = Field(description="The name of the universe.")
    galaxy: Galaxy = Field(description="A galaxy in the universe.")
def test_nested_inline_ref_expansion() -> None:
        assert to_strict_json_schema(Universe) == snapshot(
                "title": "Universe",
                    "Star": {
                        "title": "Star",
                            "name": {
                                "title": "Name",
                                "description": "The name of the star.",
                        "required": ["name"],
                    "Galaxy": {
                        "title": "Galaxy",
                                "description": "The name of the galaxy.",
                            "largest_star": {
                                "description": "The largest star in the galaxy.",
                        "required": ["name", "largest_star"],
                        "description": "The name of the universe.",
                    "galaxy": {
                        "description": "A galaxy in the universe.",
                "required": ["name", "galaxy"],
                            "name": {"title": "Name", "description": "The name of the star.", "type": "string"}
                            "name": {"title": "Name", "description": "The name of the galaxy.", "type": "string"},
                                "title": "Largest Star",
"""Test for some custom pydantic decorators."""
from pydantic import BaseModel, ConfigDict, Field
    _create_subset_model_v2,
    create_model_v2,
    is_basemodel_instance,
def test_pre_init_decorator() -> None:
        x: int = 5
        @pre_init
        def validator(cls, v: dict[str, Any]) -> dict[str, Any]:
            v["y"] = v["x"] + 1
    # Type ignore initialization b/c y is marked as required
    foo = Foo()  # type: ignore[call-arg]
    assert foo.y == 6
    foo = Foo(x=10)  # type: ignore[call-arg]
    assert foo.y == 11
def test_pre_init_decorator_with_more_defaults() -> None:
        a: int = 1
        b: int | None = None
        c: int = Field(default=2)
        d: int = Field(default_factory=lambda: 3)
            assert v["a"] == 1
            assert v["b"] is None
            assert v["c"] == 2
            assert v["d"] == 3
    # Try to create an instance of Foo
    Foo()
def test_with_aliases() -> None:
        x: int = Field(default=1, alias="y")
        z: int
            populate_by_name=True,
            v["z"] = v["x"]
    # Based on defaults
    # z is required
    assert foo.x == 1
    assert foo.z == 1
    # Based on field name
    foo = Foo(x=2)  # type: ignore[call-arg]
    assert foo.x == 2
    assert foo.z == 2
    # Based on alias
    foo = Foo(y=2)  # type: ignore[call-arg]
def test_is_basemodel_subclass() -> None:
    """Test pydantic."""
    assert is_basemodel_subclass(BaseModel)
    assert is_basemodel_subclass(BaseModelV1)
def test_is_basemodel_instance() -> None:
    assert is_basemodel_instance(Foo(x=5))
    class Bar(BaseModelV1):
    assert is_basemodel_instance(Bar(x=5))
def test_with_field_metadata() -> None:
    """Test pydantic with field metadata."""
        x: list[int] = Field(
            description="List of integers", min_length=10, max_length=15
    subset_model = _create_subset_model_v2("Foo", Foo, ["x"])
    assert subset_model.model_json_schema() == {
            "x": {
                "description": "List of integers",
                "items": {"type": "integer"},
                "maxItems": 15,
                "minItems": 10,
                "title": "X",
        "required": ["x"],
        "title": "Foo",
def test_fields_pydantic_v2_proper() -> None:
    fields = get_fields(Foo)
    assert fields == {"x": Foo.model_fields["x"]}
    sys.version_info >= (3, 14),
    reason="pydantic.v1 namespace not supported with Python 3.14+",
def test_fields_pydantic_v1_from_2() -> None:
    class Foo(BaseModelV1):
    assert fields == {"x": Foo.__fields__["x"]}
def test_create_model_v2() -> None:
    """Test that create model v2 works as expected."""
    with warnings.catch_warnings(record=True) as record:
        warnings.simplefilter("always")  # Cause all warnings to always be triggered
        foo = create_model_v2("Foo", field_definitions={"a": (int, None)})
        foo.model_json_schema()
    assert list(record) == []
    # schema is used by pydantic, but OK to re-use
        foo = create_model_v2("Foo", field_definitions={"schema": (int, None)})
    # From protected namespaces, but definitely OK to use.
        foo = create_model_v2("Foo", field_definitions={"model_id": (int, None)})
        # Verify that we can use non-English characters
        field_name = "もしもし"
        foo = create_model_v2("Foo", field_definitions={field_name: (int, None)})
def test_create_subset_model_v2_preserves_default_factory() -> None:
    """Fields with default_factory should not be marked as required."""
    class Original(BaseModel):
        required_field: str
        names: list[str] = Field(default_factory=list, description="Some names")
        mapping: dict[str, int] = Field(default_factory=dict, description="A mapping")
    subset = _create_subset_model_v2(
        "Subset",
        Original,
        ["required_field", "names", "mapping"],
    schema = subset.model_json_schema()
    assert schema.get("required") == ["required_field"]
    assert "names" not in schema.get("required", [])
    assert "mapping" not in schema.get("required", [])
