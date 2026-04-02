from openai import _compat
from openai._utils._json import openapi_dumps
class TestOpenapiDumps:
    def test_basic(self) -> None:
        data = {"key": "value", "number": 42}
        json_bytes = openapi_dumps(data)
        assert json_bytes == b'{"key":"value","number":42}'
    def test_datetime_serialization(self) -> None:
        dt = datetime.datetime(2023, 1, 1, 12, 0, 0)
        data = {"datetime": dt}
        assert json_bytes == b'{"datetime":"2023-01-01T12:00:00"}'
    def test_pydantic_model_serialization(self) -> None:
        class User(pydantic.BaseModel):
            first_name: str
            last_name: str
        model_instance = User(first_name="John", last_name="Kramer", age=83)
        data = {"model": model_instance}
        assert json_bytes == b'{"model":{"first_name":"John","last_name":"Kramer","age":83}}'
    def test_pydantic_model_with_default_values(self) -> None:
            role: str = "user"
            active: bool = True
            score: int = 0
        model_instance = User(name="Alice")
        assert json_bytes == b'{"model":{"name":"Alice"}}'
    def test_pydantic_model_with_default_values_overridden(self) -> None:
        model_instance = User(name="Bob", role="admin", active=False)
        assert json_bytes == b'{"model":{"name":"Bob","role":"admin","active":false}}'
    def test_pydantic_model_with_alias(self) -> None:
            first_name: str = pydantic.Field(alias="firstName")
            last_name: str = pydantic.Field(alias="lastName")
        model_instance = User(firstName="John", lastName="Doe")
        assert json_bytes == b'{"model":{"firstName":"John","lastName":"Doe"}}'
    def test_pydantic_model_with_alias_and_default(self) -> None:
            user_name: str = pydantic.Field(alias="userName")
            user_role: str = pydantic.Field(default="member", alias="userRole")
            is_active: bool = pydantic.Field(default=True, alias="isActive")
        model_instance = User(userName="charlie")
        assert json_bytes == b'{"model":{"userName":"charlie"}}'
        model_with_overrides = User(userName="diana", userRole="admin", isActive=False)
        data = {"model": model_with_overrides}
        assert json_bytes == b'{"model":{"userName":"diana","userRole":"admin","isActive":false}}'
    def test_pydantic_model_with_nested_models_and_defaults(self) -> None:
        class Address(pydantic.BaseModel):
            street: str
            city: str = "Unknown"
            address: Address
            verified: bool = False
        if _compat.PYDANTIC_V1:
            # to handle forward references in Pydantic v1
            User.update_forward_refs(**locals())  # type: ignore[reportDeprecated]
        address = Address(street="123 Main St")
        user = User(name="Diana", address=address)
        data = {"user": user}
        assert json_bytes == b'{"user":{"name":"Diana","address":{"street":"123 Main St"}}}'
        address_with_city = Address(street="456 Oak Ave", city="Boston")
        user_verified = User(name="Eve", address=address_with_city, verified=True)
        data = {"user": user_verified}
            json_bytes == b'{"user":{"name":"Eve","address":{"street":"456 Oak Ave","city":"Boston"},"verified":true}}'
    def test_pydantic_model_with_optional_fields(self) -> None:
            email: Union[str, None]
            phone: Union[str, None]
        model_with_none = User(name="Eve", email=None, phone=None)
        data = {"model": model_with_none}
        assert json_bytes == b'{"model":{"name":"Eve","email":null,"phone":null}}'
        model_with_values = User(name="Frank", email="frank@example.com", phone=None)
        data = {"model": model_with_values}
        assert json_bytes == b'{"model":{"name":"Frank","email":"frank@example.com","phone":null}}'
from django.core.serializers.base import DeserializationError
from django.test import SimpleTestCase, TestCase, TransactionTestCase
from django.utils.translation import gettext_lazy, override
from .models import Score
from .tests import SerializersTestBase, SerializersTransactionTestBase
class JsonSerializerTestCase(SerializersTestBase, TestCase):
    serializer_name = "json"
    pkless_str = """[
        "pk": null,
        "model": "serializers.category",
        "fields": {"name": "Reference"}
        "fields": {"name": "Non-fiction"}
    }]"""
    mapping_ordering_str = """[
  "model": "serializers.article",
  "pk": %(article_pk)s,
  "fields": {
    "author": %(author_pk)s,
    "headline": "Poker has no place on ESPN",
    "pub_date": "2006-06-16T11:00:00",
    "categories": [
      %(first_category_pk)s,
      %(second_category_pk)s
    "meta_data": [],
    "topics": []
    def _validate_output(serial_str):
            json.loads(serial_str)
    def _get_pk_values(serial_str):
        serial_list = json.loads(serial_str)
        return [obj_dict["pk"] for obj_dict in serial_list]
    def _get_field_values(serial_str, field_name):
            obj_dict["fields"][field_name]
            for obj_dict in serial_list
            if field_name in obj_dict["fields"]
    def test_indentation_whitespace(self):
        s = serializers.json.Serializer()
        json_data = s.serialize([Score(score=5.0), Score(score=6.0)], indent=2)
        for line in json_data.splitlines():
            if re.search(r".+,\s*$", line):
                self.assertEqual(line, line.rstrip())
    @isolate_apps("serializers")
    def test_custom_encoder(self):
        class ScoreDecimal(models.Model):
            score = models.DecimalField()
        class CustomJSONEncoder(json.JSONEncoder):
                if isinstance(o, decimal.Decimal):
                    return str(o)
        json_data = s.serialize(
            [ScoreDecimal(score=decimal.Decimal(1.0))], cls=CustomJSONEncoder
        self.assertIn('"fields": {"score": "1"}', json_data)
    def test_json_deserializer_exception(self):
        with self.assertRaises(DeserializationError):
            for obj in serializers.deserialize("json", """[{"pk":1}"""):
    def test_helpful_error_message_invalid_pk(self):
        If there is an invalid primary key, the error message should contain
        the model associated with it.
        test_string = """[{
            "pk": "badpk",
            "model": "serializers.player",
                "name": "Bob",
                "rank": 1,
                "team": "Team"
            DeserializationError, "(serializers.player:pk=badpk)"
            list(serializers.deserialize("json", test_string))
    def test_helpful_error_message_invalid_field(self):
        If there is an invalid field value, the error message should contain
            "pk": "1",
                "rank": "invalidint",
        expected = "(serializers.player:pk=1) field_value was 'invalidint'"
        with self.assertRaisesMessage(DeserializationError, expected):
    def test_helpful_error_message_for_foreign_keys(self):
        Invalid foreign keys with a natural key should throw a helpful error
        message, such as what the failing key is.
            "pk": 1,
                "name": "Unknown foreign key",
                "meta_data": [
                    "doesnotexist",
                    "metadata"
        key = ["doesnotexist", "metadata"]
        expected = "(serializers.category:pk=1) field_value was '%r'" % key
    def test_helpful_error_message_for_many2many_non_natural(self):
        Invalid many-to-many keys should throw a helpful error message.
                "author": 1,
                "headline": "Unknown many to many",
                "pub_date": "2014-09-15T10:35:00",
                "categories": [1, "doesnotexist"]
            "model": "serializers.author",
                "name": "Agnes"
                "name": "Reference"
        expected = "(serializers.article:pk=1) field_value was 'doesnotexist'"
    def test_helpful_error_message_for_many2many_natural1(self):
        This tests the code path where one of a list of natural keys is
            "model": "serializers.categorymetadata",
                "kind": "author",
                "name": "meta1",
                "value": "Agnes"
                    ["author", "meta1"],
                    ["doesnotexist", "meta1"],
                    ["author", "meta1"]
        key = ["doesnotexist", "meta1"]
        expected = "(serializers.article:pk=1) field_value was '%r'" % key
            for obj in serializers.deserialize("json", test_string):
    def test_helpful_error_message_for_many2many_natural2(self):
        Invalid many-to-many keys should throw a helpful error message. This
        tests the code path where a natural many-to-many key has only a single
                "meta_data": [1, "doesnotexist"]
            for obj in serializers.deserialize("json", test_string, ignore=False):
    def test_helpful_error_message_for_many2many_not_iterable(self):
        Not iterable many-to-many field value throws a helpful error message.
            "model": "serializers.m2mdata",
            "fields": {"data": null}
        expected = "(serializers.m2mdata:pk=1) field_value was 'None'"
            next(serializers.deserialize("json", test_string, ignore=False))
class JsonSerializerTransactionTestCase(
    SerializersTransactionTestBase, TransactionTestCase
    fwd_ref_str = """[
            "headline": "Forward references pose no problem",
            "pub_date": "2006-06-16T15:00:00",
            "categories": [1],
            "author": 1
class DjangoJSONEncoderTests(SimpleTestCase):
    def test_lazy_string_encoding(self):
            json.dumps({"lang": gettext_lazy("French")}, cls=DjangoJSONEncoder),
            '{"lang": "French"}',
        with override("fr"):
                '{"lang": "Fran\\u00e7ais"}',
    def test_timedelta(self):
        duration = datetime.timedelta(days=1, hours=2, seconds=3)
            json.dumps({"duration": duration}, cls=DjangoJSONEncoder),
            '{"duration": "P1DT02H00M03S"}',
        duration = datetime.timedelta(0)
            '{"duration": "P0DT00H00M00S"}',
from collections import UserList, defaultdict
class JSONNormalizeTestCase(SimpleTestCase):
    def test_converts_json_types(self):
        for test_case, expected in [
            (None, "null"),
            (True, "true"),
            (False, "false"),
            (2, "2"),
            (3.0, "3.0"),
            (1e23 + 1, "1e+23"),
            ("1", '"1"'),
            (b"hello", '"hello"'),
            ([], "[]"),
            (UserList([1, 2]), "[1, 2]"),
            ({}, "{}"),
            ({1: "a"}, '{"1": "a"}'),
            ({"foo": (1, 2, 3)}, '{"foo": [1, 2, 3]}'),
            (defaultdict(list), "{}"),
            (float("nan"), "NaN"),
            (float("inf"), "Infinity"),
            (float("-inf"), "-Infinity"),
            with self.subTest(test_case):
                normalized = normalize_json(test_case)
                # Ensure that the normalized result is serializable.
                self.assertEqual(json.dumps(normalized), expected)
    def test_bytes_decode_error(self):
        with self.assertRaisesMessage(ValueError, "Unsupported value"):
            normalize_json(b"\xff")
    def test_encode_error(self):
        for test_case in [self, any, object(), datetime.now(), set(), Decimal("3.42")]:
                self.subTest(test_case),
                self.assertRaisesMessage(TypeError, "Unsupported type"),
                normalize_json(test_case)
@override_settings(ROOT_URLCONF="view_tests.generic_urls")
class JsonResponseTests(SimpleTestCase):
    def test_json_response(self):
        response = self.client.get("/json/response/")
            json.loads(response.text),
                "a": [1, 2, 3],
                "foo": {"bar": "baz"},
                "timestamp": "2013-05-19T20:00:00",
                "value": "3.14",
from langchain_core.exceptions import OutputParserException
from langchain_core.utils.function_calling import convert_to_openai_function
from langchain_core.utils.json import (
    parse_and_check_json_markdown,
    parse_json_markdown,
    parse_partial_json,
from tests.unit_tests.pydantic_utils import _schema
GOOD_JSON = """```json
    "foo": "bar"
```"""
JSON_WITH_NEW_LINES = """
JSON_WITH_NEW_LINES_INSIDE = """```json
JSON_WITH_NEW_LINES_EVERYWHERE = """
TICKS_WITH_NEW_LINES_EVERYWHERE = """
JSON_WITH_MARKDOWN_CODE_BLOCK = """```json
    "foo": "```bar```"
JSON_WITH_PART_MARKDOWN_CODE_BLOCK = """
{\"valid_json\": "hey ```print(hello world!)``` hey"}
JSON_WITH_MARKDOWN_CODE_BLOCK_AND_NEWLINES = """```json
    "action": "Final Answer",
    "action_input": "```bar\n<div id=\\"1\\" class=\\"value\\">\n\ttext\n</div>```"
JSON_WITH_PYTHON_DICT = """```json
    "action_input": {"foo": "bar", "bar": "foo"}
JSON_WITH_ESCAPED_DOUBLE_QUOTES_IN_NESTED_JSON = """```json
    "action_input": "{\\"foo\\": \\"bar\\", \\"bar\\": \\"foo\\"}"
NO_TICKS = """{
NO_TICKS_WHITE_SPACE = """
TEXT_BEFORE = """Thought: I need to use the search tool
Action:
TEXT_AFTER = """```
This should do the trick"""
TEXT_BEFORE_AND_AFTER = """Action: Testing
WITHOUT_END_BRACKET = """Here is a response formatted as schema:
WITH_END_BRACKET = """Here is a response formatted as schema:
WITH_END_TICK = """Here is a response formatted as schema:
WITH_END_TEXT = """Here is a response formatted as schema:
This should do the trick
    GOOD_JSON,
    JSON_WITH_NEW_LINES,
    JSON_WITH_NEW_LINES_INSIDE,
    JSON_WITH_NEW_LINES_EVERYWHERE,
    TICKS_WITH_NEW_LINES_EVERYWHERE,
    NO_TICKS,
    NO_TICKS_WHITE_SPACE,
    TEXT_BEFORE,
    TEXT_AFTER,
    TEXT_BEFORE_AND_AFTER,
    WITHOUT_END_BRACKET,
    WITH_END_BRACKET,
    WITH_END_TICK,
    WITH_END_TEXT,
@pytest.mark.parametrize("json_string", TEST_CASES)
def test_parse_json(json_string: str) -> None:
    parsed = parse_json_markdown(json_string)
    assert parsed == {"foo": "bar"}
def test_parse_json_with_code_blocks() -> None:
    parsed = parse_json_markdown(JSON_WITH_MARKDOWN_CODE_BLOCK)
    assert parsed == {"foo": "```bar```"}
def test_parse_json_with_part_code_blocks() -> None:
    parsed = parse_json_markdown(JSON_WITH_PART_MARKDOWN_CODE_BLOCK)
    assert parsed == {"valid_json": "hey ```print(hello world!)``` hey"}
def test_parse_json_with_code_blocks_and_newlines() -> None:
    parsed = parse_json_markdown(JSON_WITH_MARKDOWN_CODE_BLOCK_AND_NEWLINES)
        "action_input": '```bar\n<div id="1" class="value">\n\ttext\n</div>```',
def test_parse_non_dict_json_output() -> None:
    text = "```json\n1\n```"
    with pytest.raises(OutputParserException) as exc_info:
        parse_and_check_json_markdown(text, expected_keys=["foo"])
    assert "Expected JSON object (dict)" in str(exc_info.value)
TEST_CASES_ESCAPED_QUOTES = [
    JSON_WITH_ESCAPED_DOUBLE_QUOTES_IN_NESTED_JSON,
@pytest.mark.parametrize("json_string", TEST_CASES_ESCAPED_QUOTES)
def test_parse_nested_json_with_escaped_quotes(json_string: str) -> None:
        "action_input": '{"foo": "bar", "bar": "foo"}',
def test_parse_json_with_python_dict() -> None:
    parsed = parse_json_markdown(JSON_WITH_PYTHON_DICT)
        "action_input": {"foo": "bar", "bar": "foo"},
TEST_CASES_PARTIAL = [
    ('{"foo": "bar", "bar": "foo"}', '{"foo": "bar", "bar": "foo"}'),
    ('{"foo": "bar", "bar": "foo', '{"foo": "bar", "bar": "foo"}'),
    ('{"foo": "bar", "bar": "foo}', '{"foo": "bar", "bar": "foo}"}'),
    ('{"foo": "bar", "bar": "foo[', '{"foo": "bar", "bar": "foo["}'),
    ('{"foo": "bar", "bar": "foo\\"', '{"foo": "bar", "bar": "foo\\""}'),
    ('{"foo": "bar", "bar":', '{"foo": "bar"}'),
    ('{"foo": "bar", "bar"', '{"foo": "bar"}'),
    ('{"foo": "bar", ', '{"foo": "bar"}'),
    ('{"foo":"bar\\', '{"foo": "bar"}'),
@pytest.mark.parametrize("json_strings", TEST_CASES_PARTIAL)
def test_parse_partial_json(json_strings: tuple[str, str]) -> None:
    case, expected = json_strings
    parsed = parse_partial_json(case)
    assert parsed == json.loads(expected)
STREAMED_TOKENS = """
 "
setup
":
Why
 did
 the
 bears
 band
 called
 Bears
 ?
,
punchline
Because
 they
 wanted
 play
 bear
 -y
 good
 music
 !
audience
Haha
So
 funny
""".splitlines()
EXPECTED_STREAMED_JSON = [
    {"setup": ""},
    {"setup": "Why"},
    {"setup": "Why did"},
    {"setup": "Why did the"},
    {"setup": "Why did the bears"},
    {"setup": "Why did the bears start"},
    {"setup": "Why did the bears start a"},
    {"setup": "Why did the bears start a band"},
    {"setup": "Why did the bears start a band called"},
    {"setup": "Why did the bears start a band called Bears"},
    {"setup": "Why did the bears start a band called Bears Bears"},
    {"setup": "Why did the bears start a band called Bears Bears Bears"},
    {"setup": "Why did the bears start a band called Bears Bears Bears ?"},
        "setup": "Why did the bears start a band called Bears Bears Bears ?",
        "punchline": "",
        "punchline": "Because",
        "punchline": "Because they",
        "punchline": "Because they wanted",
        "punchline": "Because they wanted to",
        "punchline": "Because they wanted to play",
        "punchline": "Because they wanted to play bear",
        "punchline": "Because they wanted to play bear -y",
        "punchline": "Because they wanted to play bear -y good",
        "punchline": "Because they wanted to play bear -y good music",
        "punchline": "Because they wanted to play bear -y good music !",
        "audience": [],
        "audience": [""],
        "audience": ["Haha"],
        "audience": ["Haha", ""],
        "audience": ["Haha", "So"],
        "audience": ["Haha", "So funny"],
EXPECTED_STREAMED_JSON_DIFF = [
    [{"op": "replace", "path": "", "value": {}}],
    [{"op": "add", "path": "/setup", "value": ""}],
    [{"op": "replace", "path": "/setup", "value": "Why"}],
    [{"op": "replace", "path": "/setup", "value": "Why did"}],
    [{"op": "replace", "path": "/setup", "value": "Why did the"}],
    [{"op": "replace", "path": "/setup", "value": "Why did the bears"}],
    [{"op": "replace", "path": "/setup", "value": "Why did the bears start"}],
    [{"op": "replace", "path": "/setup", "value": "Why did the bears start a"}],
    [{"op": "replace", "path": "/setup", "value": "Why did the bears start a band"}],
            "op": "replace",
            "path": "/setup",
            "value": "Why did the bears start a band called",
            "value": "Why did the bears start a band called Bears",
            "value": "Why did the bears start a band called Bears Bears",
            "value": "Why did the bears start a band called Bears Bears Bears",
            "value": "Why did the bears start a band called Bears Bears Bears ?",
    [{"op": "add", "path": "/punchline", "value": ""}],
    [{"op": "replace", "path": "/punchline", "value": "Because"}],
    [{"op": "replace", "path": "/punchline", "value": "Because they"}],
    [{"op": "replace", "path": "/punchline", "value": "Because they wanted"}],
    [{"op": "replace", "path": "/punchline", "value": "Because they wanted to"}],
    [{"op": "replace", "path": "/punchline", "value": "Because they wanted to play"}],
            "path": "/punchline",
            "value": "Because they wanted to play bear",
            "value": "Because they wanted to play bear -y",
            "value": "Because they wanted to play bear -y good",
            "value": "Because they wanted to play bear -y good music",
            "value": "Because they wanted to play bear -y good music !",
    [{"op": "add", "path": "/audience", "value": []}],
    [{"op": "add", "path": "/audience/0", "value": ""}],
    [{"op": "replace", "path": "/audience/0", "value": "Haha"}],
    [{"op": "add", "path": "/audience/1", "value": ""}],
    [{"op": "replace", "path": "/audience/1", "value": "So"}],
    [{"op": "replace", "path": "/audience/1", "value": "So funny"}],
def test_partial_text_json_output_parser() -> None:
    def input_iter(_: Any) -> Iterator[str]:
        yield from STREAMED_TOKENS
    chain = input_iter | SimpleJsonOutputParser()
    assert list(chain.stream(None)) == EXPECTED_STREAMED_JSON
def test_partial_text_json_output_parser_diff() -> None:
    chain = input_iter | SimpleJsonOutputParser(diff=True)
    assert list(chain.stream(None)) == EXPECTED_STREAMED_JSON_DIFF
async def test_partial_text_json_output_parser_async() -> None:
    async def input_iter(_: Any) -> AsyncIterator[str]:
        for token in STREAMED_TOKENS:
    assert [p async for p in chain.astream(None)] == EXPECTED_STREAMED_JSON
async def test_partial_text_json_output_parser_diff_async() -> None:
    assert [p async for p in chain.astream(None)] == EXPECTED_STREAMED_JSON_DIFF
def test_raises_error() -> None:
    parser = SimpleJsonOutputParser()
    with pytest.raises(OutputParserException):
        parser.invoke("hi")
# A test fixture for an output which contains
# json within a code block
TOKENS_WITH_JSON_CODE_BLOCK = [
    " France",
    "\n\n```",
    "\n{",
    "\n ",
    ' "',
    '":',
    "France",
    '",',
    " \n ",
    "population",
    "size",
    " 67",
    "39",
    "15",
    "82",
    "\n}",
    "\n```",
    "\n\nI",
    " looked",
    " up",
def test_partial_text_json_output_parser_with_json_code_block() -> None:
    """Test json parser works correctly when the response contains a json code-block."""
        yield from TOKENS_WITH_JSON_CODE_BLOCK
    assert list(chain.stream(None)) == [
        {"country_name": ""},
        {"country_name": "France"},
        {"country_name": "France", "population_size": 67},
        {"country_name": "France", "population_size": 6739},
        {"country_name": "France", "population_size": 673915},
        {"country_name": "France", "population_size": 67391582},
def test_base_model_schema_consistency() -> None:
    class Joke(BaseModel):
        setup: str
        punchline: str
    initial_joke_schema = dict(_schema(Joke).items())
    SimpleJsonOutputParser(pydantic_object=Joke)
    openai_func = convert_to_openai_function(Joke)
    retrieved_joke_schema = dict(_schema(Joke).items())
    assert initial_joke_schema == retrieved_joke_schema
    assert openai_func.get("name", None) is not None
def test_unicode_handling() -> None:
    """Tests if the JsonOutputParser is able to process unicodes."""
        title: str = Field(description="科学文章的标题")
    parser = SimpleJsonOutputParser(pydantic_object=Sample)
    format_instructions = parser.get_format_instructions()
    assert "科学文章的标题" in format_instructions, (
        "Unicode characters should not be escaped"
def test_tool_usage() -> None:
    parser = JSONAgentOutputParser()
    _input = """    ```
  "action": "search",
  "action_input": "2+2"
    output = parser.invoke(_input)
    expected_output = AgentAction(tool="search", tool_input="2+2", log=_input)
    assert output == expected_output
def test_finish() -> None:
    _input = """```
  "action_input": "4"
    expected_output = AgentFinish(return_values={"output": "4"}, log=_input)
from langchain_core.messages import AIMessageChunk
from langchain_core.output_parsers.openai_functions import JsonOutputFunctionsParser
    "action_input": "```bar\n<div id="1" class=\"value\">\n\ttext\n</div>```"
JSON_WITH_UNESCAPED_QUOTES_IN_NESTED_JSON = """```json
    "action_input": "{"foo": "bar", "bar": "foo"}"
JSON_WITH_ESCAPED_QUOTES_IN_NESTED_JSON = """```json
    "action_input": "{\"foo\": \"bar\", \"bar\": \"foo\"}"
    JSON_WITH_UNESCAPED_QUOTES_IN_NESTED_JSON,
    JSON_WITH_ESCAPED_QUOTES_IN_NESTED_JSON,
def test_partial_functions_json_output_parser() -> None:
    def input_iter(_: Any) -> Iterator[AIMessageChunk]:
            yield AIMessageChunk(
                additional_kwargs={"function_call": {"arguments": token}},
    chain = input_iter | JsonOutputFunctionsParser()
def test_partial_functions_json_output_parser_diff() -> None:
    chain = input_iter | JsonOutputFunctionsParser(diff=True)
async def test_partial_functions_json_output_parser_async() -> None:
    async def input_iter(_: Any) -> AsyncIterator[AIMessageChunk]:
async def test_partial_functions_json_output_parser_diff_async() -> None:
