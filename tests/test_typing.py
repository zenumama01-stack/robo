from typing import Generic, TypeVar, cast
from openai._utils import extract_type_var_from_base
_T2 = TypeVar("_T2")
_T3 = TypeVar("_T3")
class BaseGeneric(Generic[_T]): ...
class SubclassGeneric(BaseGeneric[_T]): ...
class BaseGenericMultipleTypeArgs(Generic[_T, _T2, _T3]): ...
class SubclassGenericMultipleTypeArgs(BaseGenericMultipleTypeArgs[_T, _T2, _T3]): ...
class SubclassDifferentOrderGenericMultipleTypeArgs(BaseGenericMultipleTypeArgs[_T2, _T, _T3]): ...
def test_extract_type_var() -> None:
        extract_type_var_from_base(
            BaseGeneric[int],
            generic_bases=cast("tuple[type, ...]", (BaseGeneric,)),
        == int
def test_extract_type_var_generic_subclass() -> None:
            SubclassGeneric[int],
def test_extract_type_var_multiple() -> None:
    typ = BaseGenericMultipleTypeArgs[int, str, None]
    generic_bases = cast("tuple[type, ...]", (BaseGenericMultipleTypeArgs,))
    assert extract_type_var_from_base(typ, index=0, generic_bases=generic_bases) == int
    assert extract_type_var_from_base(typ, index=1, generic_bases=generic_bases) == str
    assert extract_type_var_from_base(typ, index=2, generic_bases=generic_bases) == type(None)
def test_extract_type_var_generic_subclass_multiple() -> None:
    typ = SubclassGenericMultipleTypeArgs[int, str, None]
def test_extract_type_var_generic_subclass_different_ordering_multiple() -> None:
    typ = SubclassDifferentOrderGenericMultipleTypeArgs[int, str, None]
from numpy.typing.mypy_plugin import _EXTENDED_PRECISION_LIST
# Only trigger a full `mypy` run if this environment variable is set
# Note that these tests tend to take over a minute even on a macOS M1 CPU,
# and more than that in CI.
RUN_MYPY = "NPY_RUN_MYPY_IN_TESTSUITE" in os.environ
if RUN_MYPY and RUN_MYPY not in ('0', '', 'false'):
    RUN_MYPY = True
# Skips all functions in this file
    not RUN_MYPY,
    reason="`NPY_RUN_MYPY_IN_TESTSUITE` not set"
    from mypy import api
    NO_MYPY = True
    NO_MYPY = False
    # We need this as annotation, but it's located in a private namespace.
    # As a compromise, do *not* import it during runtime
    from _pytest.mark.structures import ParameterSet
PASS_DIR = os.path.join(DATA_DIR, "pass")
FAIL_DIR = os.path.join(DATA_DIR, "fail")
REVEAL_DIR = os.path.join(DATA_DIR, "reveal")
MISC_DIR = os.path.join(DATA_DIR, "misc")
MYPY_INI = os.path.join(DATA_DIR, "mypy.ini")
CACHE_DIR = os.path.join(DATA_DIR, ".mypy_cache")
#: A dictionary with file names as keys and lists of the mypy stdout as values.
#: To-be populated by `run_mypy`.
OUTPUT_MYPY: defaultdict[str, list[str]] = defaultdict(list)
def _key_func(key: str) -> str:
    """Split at the first occurrence of the ``:`` character.
    Windows drive-letters (*e.g.* ``C:``) are ignored herein.
    drive, tail = os.path.splitdrive(key)
    return os.path.join(drive, tail.split(":", 1)[0])
def _strip_filename(msg: str) -> tuple[int, str]:
    """Strip the filename and line number from a mypy message."""
    _, tail = os.path.splitdrive(msg)
    _, lineno, msg = tail.split(":", 2)
    return int(lineno), msg.strip()
def strip_func(match: re.Match[str]) -> str:
    """`re.sub` helper function for stripping module names."""
    return match.groups()[1]
@pytest.fixture(scope="module", autouse=True)
def run_mypy() -> None:
    """Clears the cache and run mypy before running any of the typing tests.
    The mypy results are cached in `OUTPUT_MYPY` for further use.
    The cache refresh can be skipped using
    NUMPY_TYPING_TEST_CLEAR_CACHE=0 pytest numpy/typing/tests
        os.path.isdir(CACHE_DIR)
        and bool(os.environ.get("NUMPY_TYPING_TEST_CLEAR_CACHE", True))
        shutil.rmtree(CACHE_DIR)
    split_pattern = re.compile(r"(\s+)?\^(\~+)?")
    for directory in (PASS_DIR, REVEAL_DIR, FAIL_DIR, MISC_DIR):
        # Run mypy
        stdout, stderr, exit_code = api.run([
            "--config-file",
            MYPY_INI,
            "--cache-dir",
            CACHE_DIR,
            pytest.fail(f"Unexpected mypy standard error\n\n{stderr}")
        elif exit_code not in {0, 1}:
            pytest.fail(f"Unexpected mypy exit code: {exit_code}\n\n{stdout}")
        str_concat = ""
        for i in stdout.split("\n"):
            if "note:" in i:
                filename = _key_func(i)
            str_concat += f"{i}\n"
            if split_pattern.match(i) is not None:
                OUTPUT_MYPY[filename].append(str_concat)
def get_test_cases(directory: str) -> Iterator[ParameterSet]:
    for root, _, files in os.walk(directory):
        for fname in files:
            short_fname, ext = os.path.splitext(fname)
            if ext in (".pyi", ".py"):
                fullpath = os.path.join(root, fname)
                yield pytest.param(fullpath, id=short_fname)
@pytest.mark.skipif(NO_MYPY, reason="Mypy is not installed")
@pytest.mark.parametrize("path", get_test_cases(PASS_DIR))
def test_success(path) -> None:
    # Alias `OUTPUT_MYPY` so that it appears in the local namespace
    output_mypy = OUTPUT_MYPY
    if path in output_mypy:
        msg = "Unexpected mypy output\n\n"
        msg += "\n".join(_strip_filename(v)[1] for v in output_mypy[path])
@pytest.mark.parametrize("path", get_test_cases(FAIL_DIR))
def test_fail(path: str) -> None:
    with open(path) as fin:
        lines = fin.readlines()
    errors = defaultdict(lambda: "")
    assert path in output_mypy
    for error_line in output_mypy[path]:
        lineno, error_line = _strip_filename(error_line)
        errors[lineno] += f'{error_line}\n'
    for i, line in enumerate(lines):
        lineno = i + 1
            line.startswith('#')
            or (" E:" not in line and lineno not in errors)
        target_line = lines[lineno - 1]
        if "# E:" in target_line:
            expression, _, marker = target_line.partition("  # E: ")
            expected_error = errors[lineno].strip()
            marker = marker.strip()
            _test_fail(path, expression, marker, expected_error, lineno)
                f"Unexpected mypy output at line {lineno}\n\n{errors[lineno]}"
_FAIL_MSG1 = """Extra error at line {}
Expression: {}
Extra error: {!r}
_FAIL_MSG2 = """Error mismatch at line {}
Expected error: {}
Observed error: {!r}
def _test_fail(
    expression: str,
    error: str,
    expected_error: None | str,
    lineno: int,
    if expected_error is None:
        raise AssertionError(_FAIL_MSG1.format(lineno, expression, error))
    elif error not in expected_error:
        raise AssertionError(_FAIL_MSG2.format(
            lineno, expression, expected_error, error
_REVEAL_MSG = """Reveal mismatch at line {}
@pytest.mark.parametrize("path", get_test_cases(REVEAL_DIR))
def test_reveal(path: str) -> None:
    """Validate that mypy correctly infers the return-types of
    the expressions in `path`.
    if path not in output_mypy:
        raise AssertionError(_REVEAL_MSG.format(lineno, error_line))
def test_code_runs(path: str) -> None:
    """Validate that the code in `path` properly during runtime."""
    path_without_extension, _ = os.path.splitext(path)
    dirname, filename = path.split(os.sep)[-2:]
    spec = importlib.util.spec_from_file_location(
        f"{dirname}.{filename}", path
    assert spec is not None
    assert spec.loader is not None
    test_module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(test_module)
LINENO_MAPPING = {
    11: "uint128",
    12: "uint256",
    14: "int128",
    15: "int256",
    17: "float80",
    18: "float96",
    19: "float128",
    20: "float256",
    22: "complex160",
    23: "complex192",
    24: "complex256",
    25: "complex512",
def test_extended_precision() -> None:
    path = os.path.join(MISC_DIR, "extended_precision.pyi")
        expression_list = f.readlines()
    for _msg in output_mypy[path]:
        lineno, msg = _strip_filename(_msg)
        expression = expression_list[lineno - 1].rstrip("\n")
        if LINENO_MAPPING[lineno] in _EXTENDED_PRECISION_LIST:
            raise AssertionError(_REVEAL_MSG.format(lineno, msg))
        elif "error" not in msg:
            _test_fail(
                path, expression, msg, 'Expression is of type "Any"', lineno
