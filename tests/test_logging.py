from openai._utils import SensitiveHeadersFilter
def logger_with_filter() -> logging.Logger:
    logger = logging.getLogger("test_logger")
def test_keys_redacted(logger_with_filter: logging.Logger, caplog: pytest.LogCaptureFixture) -> None:
        logger_with_filter.debug(
                "method": "post",
                "url": "chat/completions",
                "headers": {"api-key": "12345", "Authorization": "Bearer token"},
    log_record = cast(Dict[str, Any], caplog.records[0].args)
    assert log_record["method"] == "post"
    assert log_record["url"] == "chat/completions"
    assert log_record["headers"]["api-key"] == "<redacted>"
    assert log_record["headers"]["Authorization"] == "<redacted>"
        caplog.messages[0]
        == "Request options: {'method': 'post', 'url': 'chat/completions', 'headers': {'api-key': '<redacted>', 'Authorization': '<redacted>'}}"
def test_keys_redacted_case_insensitive(logger_with_filter: logging.Logger, caplog: pytest.LogCaptureFixture) -> None:
                "headers": {"Api-key": "12345", "authorization": "Bearer token"},
    assert log_record["headers"]["Api-key"] == "<redacted>"
    assert log_record["headers"]["authorization"] == "<redacted>"
        == "Request options: {'method': 'post', 'url': 'chat/completions', 'headers': {'Api-key': '<redacted>', 'authorization': '<redacted>'}}"
def test_no_headers(logger_with_filter: logging.Logger, caplog: pytest.LogCaptureFixture) -> None:
            {"method": "post", "url": "chat/completions"},
    assert "api-key" not in log_record
    assert "Authorization" not in log_record
    assert caplog.messages[0] == "Request options: {'method': 'post', 'url': 'chat/completions'}"
def test_headers_without_sensitive_info(logger_with_filter: logging.Logger, caplog: pytest.LogCaptureFixture) -> None:
                "headers": {"custom": "value"},
    assert log_record["headers"] == {"custom": "value"}
        == "Request options: {'method': 'post', 'url': 'chat/completions', 'headers': {'custom': 'value'}}"
def test_standard_debug_msg(logger_with_filter: logging.Logger, caplog: pytest.LogCaptureFixture) -> None:
        logger_with_filter.debug("Sending HTTP Request: %s %s", "POST", "chat/completions")
    assert caplog.messages[0] == "Sending HTTP Request: POST chat/completions"
from logging import LogRecord
from spotdl.utils.logging import DEBUG, NOTSET, SpotdlFormatter
def test_spotdl_formatter_format():
    # cf. https://rich.readthedocs.io/en/stable/markup.html#escaping
    formatter = SpotdlFormatter()
    input_output_map = {
        ("[as it is, infinite]", DEBUG): "[blue]\\[as it is, infinite]",
        ("[effluvium]", NOTSET): "\\[effluvium]",
        ("DRIP", DEBUG): "[blue]DRIP",
        ("FOREIGN TONGUES", NOTSET): "FOREIGN TONGUES",
    for (msg, level), escaped_msg in input_output_map.items():
            formatter.format(
                LogRecord("spotdl", level, "", 0, msg, None, None, None, None)
            == escaped_msg
class SchemaLoggerTests(TestCase):
    def test_extra_args(self):
        editor = connection.schema_editor(collect_sql=True)
        sql = "SELECT * FROM foo WHERE id in (%s, %s)"
        params = [42, 1337]
        with self.assertLogs("django.db.backends.schema", "DEBUG") as cm:
            editor.execute(sql, params)
        if connection.features.schema_editor_uses_clientside_param_binding:
            sql = "SELECT * FROM foo WHERE id in (42, 1337)"
        self.assertEqual(cm.records[0].sql, sql)
        self.assertEqual(cm.records[0].params, params)
        self.assertEqual(cm.records[0].getMessage(), f"{sql}; (params {params})")
from django.template import Engine, Variable, VariableDoesNotExist
class VariableResolveLoggingTests(SimpleTestCase):
    loglevel = logging.DEBUG
    def test_log_on_variable_does_not_exist_silent(self):
        class TestObject:
            class SilentDoesNotExist(Exception):
            def template_name(self):
                return "template_name"
            def template(self):
                return Engine().from_string("")
            def article(self):
                raise TestObject.SilentDoesNotExist("Attribute does not exist.")
                return (attr for attr in dir(TestObject) if attr[:2] != "__")
            def __getitem__(self, item):
                return self.__dict__[item]
        with self.assertLogs("django.template", self.loglevel) as cm:
            Variable("article").resolve(TestObject())
        self.assertEqual(len(cm.records), 1)
        log_record = cm.records[0]
            log_record.getMessage(),
            "Exception while resolving variable 'article' in template 'template_name'.",
        self.assertIsNotNone(log_record.exc_info)
        raised_exception = log_record.exc_info[1]
        self.assertEqual(str(raised_exception), "Attribute does not exist.")
    def test_log_on_variable_does_not_exist_not_silent(self):
            with self.assertRaises(VariableDoesNotExist):
                Variable("article.author").resolve({"article": {"section": "News"}})
            "Exception while resolving variable 'author' in template 'unknown'.",
            str(raised_exception),
            "Failed lookup for key [author] in {'section': 'News'}",
    def test_no_log_when_variable_exists(self):
        with self.assertNoLogs("django.template", self.loglevel):
            Variable("article.section").resolve({"article": {"section": "News"}})
from langchain_classic.callbacks.tracers import LoggingCallbackHandler
def test_logging(
    caplog: pytest.LogCaptureFixture,
    capsys: pytest.CaptureFixture[str],
    # Set up a Logger and a handler so we can check the Logger's handlers work too
    logger = logging.getLogger("test_logging")
    logger.addHandler(logging.StreamHandler(sys.stdout))
    handler = LoggingCallbackHandler(logger, extra={"test": "test_extra"})
    handler.on_text("test", run_id=uuid.uuid4())
    # Assert logging actually took place
    assert len(caplog.record_tuples) == 1
    record = caplog.records[0]
    assert record.name == logger.name
    assert record.levelno == logging.INFO
        record.msg == "\x1b[36;1m\x1b[1;3m[text]\x1b[0m \x1b[1mNew text:\x1b[0m\ntest"
    # Check the extra shows up
    assert record.test == "test_extra"  # type: ignore[attr-defined]
    # Assert log handlers worked
    cap_result = capsys.readouterr()
        cap_result.out
        == "\x1b[36;1m\x1b[1;3m[text]\x1b[0m \x1b[1mNew text:\x1b[0m\ntest\n"
