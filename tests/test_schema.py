"""Unit tests for googleapiclient.schema."""
LOAD_FEED = """{
      "longVal": 42,
      "kind": "zoo#loadValue",
      "enumVal": "A String",
      "anyVal": "", # Anything will do.
      "nullVal": None,
      "stringVal": "A String",
      "doubleVal": 3.14,
      "booleanVal": True or False, # True or False.
  "kind": "zoo#loadFeed",
class SchemasTest(unittest.TestCase):
        f = open(datafile("zoo.json"))
        discovery = f.read()
        self.sc = Schemas(discovery)
    def test_basic_formatting(self):
            sorted(LOAD_FEED.splitlines()),
            sorted(self.sc.prettyPrintByName("LoadFeed").splitlines()),
    def test_empty_edge_case(self):
        self.assertTrue("Unknown type" in self.sc.prettyPrintSchema({}))
    def test_simple_object(self):
        self.assertEqual({}, eval(self.sc.prettyPrintSchema({"type": "object"})))
    def test_string(self):
            type(""), type(eval(self.sc.prettyPrintSchema({"type": "string"})))
    def test_integer(self):
            type(20), type(eval(self.sc.prettyPrintSchema({"type": "integer"})))
    def test_number(self):
            type(1.2), type(eval(self.sc.prettyPrintSchema({"type": "number"})))
    def test_boolean(self):
            type(True), type(eval(self.sc.prettyPrintSchema({"type": "boolean"})))
    def test_string_default(self):
            "foo", eval(self.sc.prettyPrintSchema({"type": "string", "default": "foo"}))
    def test_integer_default(self):
            20, eval(self.sc.prettyPrintSchema({"type": "integer", "default": 20}))
    def test_number_default(self):
            1.2, eval(self.sc.prettyPrintSchema({"type": "number", "default": 1.2}))
    def test_boolean_default(self):
            eval(self.sc.prettyPrintSchema({"type": "boolean", "default": False})),
    def test_null(self):
        self.assertEqual(None, eval(self.sc.prettyPrintSchema({"type": "null"})))
    def test_any(self):
        self.assertEqual("", eval(self.sc.prettyPrintSchema({"type": "any"})))
    def test_array(self):
            [{}],
            eval(
                self.sc.prettyPrintSchema(
                    {"type": "array", "items": {"type": "object"}}
    def test_nested_references(self):
        feed = {
                    "photo": {
                        "hash": "A String",
                        "hashAlgorithm": "A String",
                        "filename": "A String",
                        "type": "A String",
                        "size": 42,
                    "kind": "zoo#animal",
                    "etag": "A String",
                    "name": "A String",
            "kind": "zoo#animalFeed",
        self.assertEqual(feed, eval(self.sc.prettyPrintByName("AnimalFeed")))
    def test_additional_properties(self):
        items = {
            "animals": {
                "a_key": {
            "kind": "zoo#animalMap",
        self.assertEqual(items, eval(self.sc.prettyPrintByName("AnimalMap")))
    def test_unknown_name(self):
        self.assertRaises(KeyError, self.sc.prettyPrintByName, "UknownSchemaThing")
class SchemaEditorTests(SimpleTestCase):
    def test_effective_default_callable(self):
        SchemaEditor.effective_default() shouldn't call callable defaults.
        class MyStr(str):
            def __call__(self):
        class MyCharField(models.CharField):
        field = MyCharField(max_length=1, default=MyStr)
        self.assertEqual(BaseDatabaseSchemaEditor._effective_default(field), MyStr)
from django.test import TestCase
@unittest.skipUnless(connection.vendor == "mysql", "MySQL tests")
class SchemaEditorTests(TestCase):
    def test_quote_value(self):
        import MySQLdb
        editor = connection.schema_editor()
        tested_values = [
            ("string", "'string'"),
            ("¿Tú hablas inglés?", "'¿Tú hablas inglés?'"),
            (b"bytes", b"'bytes'"),
            (42, "42"),
            (1.754, "1.754e0" if MySQLdb.version_info >= (1, 3, 14) else "1.754"),
            (False, b"0" if MySQLdb.version_info >= (1, 4, 0) else "0"),
        for value, expected in tested_values:
            with self.subTest(value=value):
                self.assertEqual(editor.quote_value(value), expected)
"""Test formatting functionality."""
from langchain_core.language_models.base import _get_token_ids_default_method
class TestTokenCountingWithGPT2Tokenizer:
    def test_tokenization(self) -> None:
        # Check that the tokenization is consistent with the GPT-2 tokenizer
        assert _get_token_ids_default_method("This is a test") == [1212, 318, 257, 1332]
    def test_empty_token(self) -> None:
        assert len(_get_token_ids_default_method("")) == 0
    def test_multiple_tokens(self) -> None:
        assert len(_get_token_ids_default_method("a b c")) == 3
    def test_special_tokens(self) -> None:
        # test for consistency when the default tokenizer is changed
        assert len(_get_token_ids_default_method("a:b_c d")) == 6
from langchain_core.outputs import ChatGeneration, ChatGenerationChunk, Generation
from langchain_core.prompt_values import ChatPromptValueConcrete, StringPromptValue
from pydantic import RootModel, ValidationError
@pytest.mark.xfail(reason="TODO: FIX BEFORE 0.3 RELEASE")
def test_serialization_of_wellknown_objects() -> None:
    """Test that pydantic is able to serialize and deserialize well known objects."""
    well_known_lc_object = RootModel[
        Document
        | HumanMessage
        | SystemMessage
        | ChatMessage
        | FunctionMessage
        | FunctionMessageChunk
        | AIMessage
        | HumanMessageChunk
        | SystemMessageChunk
        | ChatMessageChunk
        | AIMessageChunk
        | StringPromptValue
        | ChatPromptValueConcrete
        | AgentFinish
        | AgentAction
        | AgentActionMessageLog
        | ChatGeneration
        | Generation
        | ChatGenerationChunk,
    lc_objects = [
        HumanMessage(content="human"),
        HumanMessageChunk(content="human"),
        AIMessage(content="ai"),
        AIMessageChunk(content="ai"),
        SystemMessage(content="sys"),
        SystemMessageChunk(content="sys"),
        FunctionMessage(
            name="func",
            content="func",
        FunctionMessageChunk(
        ChatMessage(
            role="human",
            content="human",
        ChatMessageChunk(
        StringPromptValue(text="hello"),
        ChatPromptValueConcrete(messages=[AIMessage(content="foo")]),
        ChatPromptValueConcrete(messages=[HumanMessage(content="human")]),
        ChatPromptValueConcrete(
            messages=[ToolMessage(content="foo", tool_call_id="bar")],
        ChatPromptValueConcrete(messages=[SystemMessage(content="foo")]),
        Document(page_content="hello"),
        AgentFinish(return_values={}, log=""),
        AgentAction(tool="tool", tool_input="input", log=""),
        AgentActionMessageLog(
            tool="tool",
            tool_input="input",
            message_log=[HumanMessage(content="human")],
        Generation(
            text="hello",
            generation_info={"info": "info"},
        ChatGeneration(
            message=HumanMessage(content="human"),
        ChatGenerationChunk(
            message=HumanMessageChunk(content="cat"),
    for lc_object in lc_objects:
        d = lc_object.model_dump()
        assert "type" in d, f"Missing key `type` for {type(lc_object)}"
        obj1 = well_known_lc_object.model_validate(d)
        assert type(obj1.root) is type(lc_object), f"failed for {type(lc_object)}"
    with pytest.raises((TypeError, ValidationError)):
        # Make sure that specifically validation error is raised
        well_known_lc_object.model_validate({})
