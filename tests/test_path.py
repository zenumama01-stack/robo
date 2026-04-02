from openai._utils._path import path_template
    "template, kwargs, expected",
        ("/v1/{id}", dict(id="abc"), "/v1/abc"),
        ("/v1/{a}/{b}", dict(a="x", b="y"), "/v1/x/y"),
        ("/v1/{a}{b}/path/{c}?val={d}#{e}", dict(a="x", b="y", c="z", d="u", e="v"), "/v1/xy/path/z?val=u#v"),
        ("/{w}/{w}", dict(w="echo"), "/echo/echo"),
        ("/v1/static", {}, "/v1/static"),
        ("", {}, ""),
        ("/v1/?q={n}&count=10", dict(n=42), "/v1/?q=42&count=10"),
        ("/v1/{v}", dict(v=None), "/v1/null"),
        ("/v1/{v}", dict(v=True), "/v1/true"),
        ("/v1/{v}", dict(v=False), "/v1/false"),
        ("/v1/{v}", dict(v=".hidden"), "/v1/.hidden"),  # dot prefix ok
        ("/v1/{v}", dict(v="file.txt"), "/v1/file.txt"),  # dot in middle ok
        ("/v1/{v}", dict(v="..."), "/v1/..."),  # triple dot ok
        ("/v1/{a}{b}", dict(a=".", b="txt"), "/v1/.txt"),  # dot var combining with adjacent to be ok
        ("/items?q={v}#{f}", dict(v=".", f=".."), "/items?q=.#.."),  # dots in query/fragment are fine
            "/v1/{a}?query={b}",
            dict(a="../../other/endpoint", b="a&bad=true"),
            "/v1/..%2F..%2Fother%2Fendpoint?query=a%26bad%3Dtrue",
        ("/v1/{val}", dict(val="a/b/c"), "/v1/a%2Fb%2Fc"),
        ("/v1/{val}", dict(val="a/b/c?query=value"), "/v1/a%2Fb%2Fc%3Fquery=value"),
        ("/v1/{val}", dict(val="a/b/c?query=value&bad=true"), "/v1/a%2Fb%2Fc%3Fquery=value&bad=true"),
        ("/v1/{val}", dict(val="%20"), "/v1/%2520"),  # escapes escape sequences in input
        # Query: slash and ? are safe, # is not
        ("/items?q={v}", dict(v="a/b"), "/items?q=a/b"),
        ("/items?q={v}", dict(v="a?b"), "/items?q=a?b"),
        ("/items?q={v}", dict(v="a#b"), "/items?q=a%23b"),
        ("/items?q={v}", dict(v="a b"), "/items?q=a%20b"),
        # Fragment: slash and ? are safe
        ("/docs#{v}", dict(v="a/b"), "/docs#a/b"),
        ("/docs#{v}", dict(v="a?b"), "/docs#a?b"),
        # Path: slash, ? and # are all encoded
        ("/v1/{v}", dict(v="a/b"), "/v1/a%2Fb"),
        ("/v1/{v}", dict(v="a?b"), "/v1/a%3Fb"),
        ("/v1/{v}", dict(v="a#b"), "/v1/a%23b"),
        # same var encoded differently by component
            "/v1/{v}?q={v}#{v}",
            dict(v="a/b?c#d"),
            "/v1/a%2Fb%3Fc%23d?q=a/b?c%23d#a/b?c%23d",
        ("/v1/{val}", dict(val="x?admin=true"), "/v1/x%3Fadmin=true"),  # query injection
        ("/v1/{val}", dict(val="x#admin"), "/v1/x%23admin"),  # fragment injection
def test_interpolation(template: str, kwargs: dict[str, Any], expected: str) -> None:
    assert path_template(template, **kwargs) == expected
def test_missing_kwarg_raises_key_error() -> None:
    with pytest.raises(KeyError, match="org_id"):
        path_template("/v1/{org_id}")
    "template, kwargs",
        ("{a}/path", dict(a=".")),
        ("{a}/path", dict(a="..")),
        ("/v1/{a}", dict(a=".")),
        ("/v1/{a}", dict(a="..")),
        ("/v1/{a}/path", dict(a=".")),
        ("/v1/{a}/path", dict(a="..")),
        ("/v1/{a}{b}", dict(a=".", b=".")),  # adjacent vars → ".."
        ("/v1/{a}.", dict(a=".")),  # var + static → ".."
        ("/v1/{a}{b}", dict(a="", b=".")),  # empty + dot → "."
        ("/v1/%2e/{x}", dict(x="ok")),  # encoded dot in static text
        ("/v1/%2e./{x}", dict(x="ok")),  # mixed encoded ".." in static
        ("/v1/.%2E/{x}", dict(x="ok")),  # mixed encoded ".." in static
        ("/v1/{v}?q=1", dict(v="..")),
        ("/v1/{v}#frag", dict(v="..")),
def test_dot_segment_rejected(template: str, kwargs: dict[str, Any]) -> None:
    with pytest.raises(ValueError, match="dot-segment"):
        path_template(template, **kwargs)
from fastapi.testclient import TestClient
from .main import app
client = TestClient(app)
def test_text_get():
    response = client.get("/text")
    assert response.status_code == 200, response.text
    assert response.json() == "Hello World"
def test_nonexistent():
    response = client.get("/nonexistent")
    assert response.status_code == 404, response.text
    assert response.json() == {"detail": "Not Found"}
def test_path_foobar():
    response = client.get("/path/foobar")
    assert response.json() == "foobar"
def test_path_str_foobar():
    response = client.get("/path/str/foobar")
def test_path_str_42():
    response = client.get("/path/str/42")
    assert response.json() == "42"
def test_path_str_True():
    response = client.get("/path/str/True")
    assert response.json() == "True"
def test_path_int_foobar():
    response = client.get("/path/int/foobar")
    assert response.status_code == 422
    assert response.json() == {
        "detail": [
                "type": "int_parsing",
                "loc": ["path", "item_id"],
                "msg": "Input should be a valid integer, unable to parse string as an integer",
                "input": "foobar",
def test_path_int_True():
    response = client.get("/path/int/True")
                "input": "True",
def test_path_int_42():
    response = client.get("/path/int/42")
    assert response.json() == 42
def test_path_int_42_5():
    response = client.get("/path/int/42.5")
                "input": "42.5",
def test_path_float_foobar():
    response = client.get("/path/float/foobar")
                "type": "float_parsing",
                "msg": "Input should be a valid number, unable to parse string as a number",
def test_path_float_True():
    response = client.get("/path/float/True")
def test_path_float_42():
    response = client.get("/path/float/42")
def test_path_float_42_5():
    response = client.get("/path/float/42.5")
    assert response.json() == 42.5
def test_path_bool_foobar():
    response = client.get("/path/bool/foobar")
                "type": "bool_parsing",
                "msg": "Input should be a valid boolean, unable to interpret input",
def test_path_bool_True():
    response = client.get("/path/bool/True")
    assert response.json() is True
def test_path_bool_42():
    response = client.get("/path/bool/42")
                "input": "42",
def test_path_bool_42_5():
    response = client.get("/path/bool/42.5")
def test_path_bool_1():
    response = client.get("/path/bool/1")
def test_path_bool_0():
    response = client.get("/path/bool/0")
    assert response.json() is False
def test_path_bool_true():
    response = client.get("/path/bool/true")
def test_path_bool_False():
    response = client.get("/path/bool/False")
def test_path_bool_false():
    response = client.get("/path/bool/false")
def test_path_param_foo():
    response = client.get("/path/param/foo")
    assert response.json() == "foo"
def test_path_param_minlength_foo():
    response = client.get("/path/param-minlength/foo")
def test_path_param_minlength_fo():
    response = client.get("/path/param-minlength/fo")
                "type": "string_too_short",
                "msg": "String should have at least 3 characters",
                "input": "fo",
                "ctx": {"min_length": 3},
def test_path_param_maxlength_foo():
    response = client.get("/path/param-maxlength/foo")
def test_path_param_maxlength_foobar():
    response = client.get("/path/param-maxlength/foobar")
                "type": "string_too_long",
                "msg": "String should have at most 3 characters",
                "ctx": {"max_length": 3},
def test_path_param_min_maxlength_foo():
    response = client.get("/path/param-min_maxlength/foo")
def test_path_param_min_maxlength_foobar():
    response = client.get("/path/param-min_maxlength/foobar")
def test_path_param_min_maxlength_f():
    response = client.get("/path/param-min_maxlength/f")
                "msg": "String should have at least 2 characters",
                "input": "f",
                "ctx": {"min_length": 2},
def test_path_param_gt_42():
    response = client.get("/path/param-gt/42")
def test_path_param_gt_2():
    response = client.get("/path/param-gt/2")
                "type": "greater_than",
                "msg": "Input should be greater than 3",
                "input": "2",
                "ctx": {"gt": 3.0},
def test_path_param_gt0_0_05():
    response = client.get("/path/param-gt0/0.05")
    assert response.json() == 0.05
def test_path_param_gt0_0():
    response = client.get("/path/param-gt0/0")
                "msg": "Input should be greater than 0",
                "input": "0",
                "ctx": {"gt": 0.0},
def test_path_param_ge_42():
    response = client.get("/path/param-ge/42")
def test_path_param_ge_3():
    response = client.get("/path/param-ge/3")
    assert response.json() == 3
def test_path_param_ge_2():
    response = client.get("/path/param-ge/2")
                "type": "greater_than_equal",
                "msg": "Input should be greater than or equal to 3",
                "ctx": {"ge": 3.0},
def test_path_param_lt_42():
    response = client.get("/path/param-lt/42")
                "type": "less_than",
                "msg": "Input should be less than 3",
                "ctx": {"lt": 3.0},
def test_path_param_lt_2():
    response = client.get("/path/param-lt/2")
    assert response.json() == 2
def test_path_param_lt0__1():
    response = client.get("/path/param-lt0/-1")
    assert response.json() == -1
def test_path_param_lt0_0():
    response = client.get("/path/param-lt0/0")
                "msg": "Input should be less than 0",
                "ctx": {"lt": 0.0},
def test_path_param_le_42():
    response = client.get("/path/param-le/42")
                "type": "less_than_equal",
                "msg": "Input should be less than or equal to 3",
                "ctx": {"le": 3.0},
def test_path_param_le_3():
    response = client.get("/path/param-le/3")
def test_path_param_le_2():
    response = client.get("/path/param-le/2")
def test_path_param_lt_gt_2():
    response = client.get("/path/param-lt-gt/2")
def test_path_param_lt_gt_4():
    response = client.get("/path/param-lt-gt/4")
                "input": "4",
def test_path_param_lt_gt_0():
    response = client.get("/path/param-lt-gt/0")
                "msg": "Input should be greater than 1",
                "ctx": {"gt": 1.0},
def test_path_param_le_ge_2():
    response = client.get("/path/param-le-ge/2")
def test_path_param_le_ge_1():
    response = client.get("/path/param-le-ge/1")
def test_path_param_le_ge_3():
    response = client.get("/path/param-le-ge/3")
def test_path_param_le_ge_4():
    response = client.get("/path/param-le-ge/4")
def test_path_param_lt_int_2():
    response = client.get("/path/param-lt-int/2")
def test_path_param_lt_int_42():
    response = client.get("/path/param-lt-int/42")
                "ctx": {"lt": 3},
def test_path_param_lt_int_2_7():
    response = client.get("/path/param-lt-int/2.7")
                "input": "2.7",
def test_path_param_gt_int_42():
    response = client.get("/path/param-gt-int/42")
def test_path_param_gt_int_2():
    response = client.get("/path/param-gt-int/2")
                "ctx": {"gt": 3},
def test_path_param_gt_int_2_7():
    response = client.get("/path/param-gt-int/2.7")
def test_path_param_le_int_42():
    response = client.get("/path/param-le-int/42")
                "ctx": {"le": 3},
def test_path_param_le_int_3():
    response = client.get("/path/param-le-int/3")
def test_path_param_le_int_2():
    response = client.get("/path/param-le-int/2")
def test_path_param_le_int_2_7():
    response = client.get("/path/param-le-int/2.7")
def test_path_param_ge_int_42():
    response = client.get("/path/param-ge-int/42")
def test_path_param_ge_int_3():
    response = client.get("/path/param-ge-int/3")
def test_path_param_ge_int_2():
    response = client.get("/path/param-ge-int/2")
                "ctx": {"ge": 3},
def test_path_param_ge_int_2_7():
    response = client.get("/path/param-ge-int/2.7")
def test_path_param_lt_gt_int_2():
    response = client.get("/path/param-lt-gt-int/2")
def test_path_param_lt_gt_int_4():
    response = client.get("/path/param-lt-gt-int/4")
def test_path_param_lt_gt_int_0():
    response = client.get("/path/param-lt-gt-int/0")
                "ctx": {"gt": 1},
def test_path_param_lt_gt_int_2_7():
    response = client.get("/path/param-lt-gt-int/2.7")
def test_path_param_le_ge_int_2():
    response = client.get("/path/param-le-ge-int/2")
def test_path_param_le_ge_int_1():
    response = client.get("/path/param-le-ge-int/1")
    assert response.json() == 1
def test_path_param_le_ge_int_3():
    response = client.get("/path/param-le-ge-int/3")
def test_path_param_le_ge_int_4():
    response = client.get("/path/param-le-ge-int/4")
def test_path_param_le_ge_int_2_7():
    response = client.get("/path/param-le-ge-int/2.7")
from langchain_core._api import path
HERE = Path(__file__).parent
ROOT = HERE.parent.parent.parent
def test_as_import_path() -> None:
    """Test that the path is converted to a LangChain import path."""
    # Verify that default paths are correct
    # if editable install, check directory structure
    if path.PACKAGE_DIR == ROOT / "langchain_core":
        assert path.PACKAGE_DIR == ROOT / "langchain_core"
    # Verify that as import path works correctly
    assert path.as_import_path(HERE, relative_to=ROOT) == "tests.unit_tests._api"
        path.as_import_path(__file__, relative_to=ROOT)
        == "tests.unit_tests._api.test_path"
        path.as_import_path(__file__, suffix="create_agent", relative_to=ROOT)
        == "tests.unit_tests._api.test_path.create_agent"
class CommonTests(util.CommonTests, unittest.TestCase):
    def execute(self, package, path):
        with resources.as_file(resources.files(package).joinpath(path)):
class PathTests:
    def test_reading(self):
        Path should be readable and a pathlib.Path instance.
        target = resources.files(self.data) / 'utf-8.file'
        with resources.as_file(target) as path:
            self.assertIsInstance(path, pathlib.Path)
            self.assertTrue(path.name.endswith("utf-8.file"), repr(path))
            self.assertEqual('Hello, UTF-8 world!\n', path.read_text(encoding='utf-8'))
class PathDiskTests(PathTests, util.DiskSetup, unittest.TestCase):
    def test_natural_path(self):
        Guarantee the internal implementation detail that
        file-system-backed resources do not get the tempdir
        treatment.
            assert 'data' in str(path)
class PathMemoryTests(PathTests, unittest.TestCase):
        file = io.BytesIO(b'Hello, UTF-8 world!\n')
        self.addCleanup(file.close)
        self.data = util.create_package(
            file=file, path=FileNotFoundError("package exists only in memory")
        self.data.__spec__.origin = None
        self.data.__spec__.has_location = False
class PathZipTests(PathTests, util.ZipSetup, unittest.TestCase):
    def test_remove_in_context_manager(self):
        It is not an error if the file that was temporarily stashed on the
        file system is removed inside the `with` stanza.
            path.unlink()
