import os.path as osp
import pathlib
from importlib.resources import files
import pytest
from notebook.app import JupyterNotebookApp
pytest_plugins = ["jupyter_server.pytest_plugin"]
def mkdir(tmp_path, *parts):
    path = tmp_path.joinpath(*parts)
    if not path.exists():
        path.mkdir(parents=True)
app_settings_dir = pytest.fixture(lambda tmp_path: mkdir(tmp_path, "app_settings"))
user_settings_dir = pytest.fixture(lambda tmp_path: mkdir(tmp_path, "user_settings"))
schemas_dir = pytest.fixture(lambda tmp_path: mkdir(tmp_path, "schemas"))
workspaces_dir = pytest.fixture(lambda tmp_path: mkdir(tmp_path, "workspaces"))
labextensions_dir = pytest.fixture(lambda tmp_path: mkdir(tmp_path, "labextensions_dir"))
@pytest.fixture
def make_notebook_app(  # PLR0913
    jp_root_dir,
    jp_template_dir,
    app_settings_dir,
    user_settings_dir,
    schemas_dir,
    workspaces_dir,
    labextensions_dir,
    def _make_notebook_app(**kwargs):
        return JupyterNotebookApp(
            static_dir=str(jp_root_dir),
            templates_dir=str(jp_template_dir),
            app_url="/",
            app_settings_dir=str(app_settings_dir),
            user_settings_dir=str(user_settings_dir),
            schemas_dir=str(schemas_dir),
            workspaces_dir=str(workspaces_dir),
            extra_labextensions_path=[str(labextensions_dir)],
    # Copy the template files.
    for html_path in glob.glob(str(files("notebook.templates").joinpath("*.html"))):
        shutil.copy(html_path, jp_template_dir)
    # Create the index file.
    index = jp_template_dir.joinpath("index.html")
    index.write_text(
  <title>{{page_config['appName'] | e}}</title>
    {# Copy so we do not modify the page_config with updates. #}
    {% set page_config_full = page_config.copy() %}
    {# Set a dummy variable - we just want the side effect of the update. #}
    {% set _ = page_config_full.update(baseUrl=base_url, wsUrl=ws_url) %}
  <script src="{{page_config['fullStaticUrl'] | e}}/bundle.js" main="index"></script>
        window.history.replaceState({ }, '', parsedUrl.href);
    # Copy the schema files.
    test_data = str(files("jupyterlab_server.test_data")._paths[0])
    src = pathlib.PurePath(test_data, "schemas", "@jupyterlab")
    dst = pathlib.PurePath(str(schemas_dir), "@jupyterlab")
    if os.path.exists(dst):
        shutil.rmtree(dst)
    shutil.copytree(src, dst)
    # Create the federated extensions
    for name in ["apputils-extension", "codemirror-extension"]:
        target_name = name + "-federated"
        target = pathlib.PurePath(str(labextensions_dir), "@jupyterlab", target_name)
        src = pathlib.PurePath(test_data, "schemas", "@jupyterlab", name)
        dst = target / "schemas" / "@jupyterlab" / target_name
        if osp.exists(dst):
        with open(target / "package.orig.json", "w") as fid:
            data = dict(name=target_name, jupyterlab=dict(extension=True))
            json.dump(data, fid)
    # Copy the overrides file.
    src = pathlib.PurePath(test_data, "app-settings", "overrides.json")
    dst = pathlib.PurePath(str(app_settings_dir), "overrides.json")
        os.remove(dst)
    shutil.copyfile(src, dst)
    # Copy workspaces.
    ws_path = pathlib.PurePath(test_data, "workspaces")
    for item in os.listdir(ws_path):
        src = ws_path / item
        dst = pathlib.PurePath(str(workspaces_dir), item)
        shutil.copy(src, str(workspaces_dir))
    return _make_notebook_app
def notebookapp(jp_serverapp, make_notebook_app):
    app = make_notebook_app()
    app._link_jupyter_server_extension(jp_serverapp)
    app.initialize()
    return app
from typing import TYPE_CHECKING, Iterator, AsyncIterator
from pytest_asyncio import is_async_test
from openai import OpenAI, AsyncOpenAI, DefaultAioHttpClient
from openai._utils import is_dict
    from _pytest.fixtures import FixtureRequest  # pyright: ignore[reportPrivateImportUsage]
pytest.register_assert_rewrite("tests.utils")
logging.getLogger("openai").setLevel(logging.DEBUG)
# automatically add `pytest.mark.asyncio()` to all of our async tests
# so we don't have to add that boilerplate everywhere
def pytest_collection_modifyitems(items: list[pytest.Function]) -> None:
    pytest_asyncio_tests = (item for item in items if is_async_test(item))
    session_scope_marker = pytest.mark.asyncio(loop_scope="session")
    for async_test in pytest_asyncio_tests:
        async_test.add_marker(session_scope_marker, append=False)
    # We skip tests that use both the aiohttp client and respx_mock as respx_mock
    # doesn't support custom transports.
        if "async_client" not in item.fixturenames or "respx_mock" not in item.fixturenames:
        if not hasattr(item, "callspec"):
        async_client_param = item.callspec.params.get("async_client")
        if is_dict(async_client_param) and async_client_param.get("http_client") == "aiohttp":
            item.add_marker(pytest.mark.skip(reason="aiohttp client is not compatible with respx_mock"))
base_url = os.environ.get("TEST_API_BASE_URL", "http://127.0.0.1:4010")
api_key = "My API Key"
@pytest.fixture(scope="session")
def client(request: FixtureRequest) -> Iterator[OpenAI]:
    strict = getattr(request, "param", True)
    if not isinstance(strict, bool):
        raise TypeError(f"Unexpected fixture parameter type {type(strict)}, expected {bool}")
    with OpenAI(base_url=base_url, api_key=api_key, _strict_response_validation=strict) as client:
        yield client
async def async_client(request: FixtureRequest) -> AsyncIterator[AsyncOpenAI]:
    param = getattr(request, "param", True)
    # defaults
    strict = True
    http_client: None | httpx.AsyncClient = None
    if isinstance(param, bool):
        strict = param
    elif is_dict(param):
        strict = param.get("strict", True)
        assert isinstance(strict, bool)
        http_client_type = param.get("http_client", "httpx")
        if http_client_type == "aiohttp":
            http_client = DefaultAioHttpClient()
        raise TypeError(f"Unexpected fixture parameter type {type(param)}, expected bool or dict")
    async with AsyncOpenAI(
        base_url=base_url, api_key=api_key, _strict_response_validation=strict, http_client=http_client
    ) as client:
from yt_dlp.networking import RequestHandler
from yt_dlp.networking.common import _REQUEST_HANDLERS
from yt_dlp.utils._utils import _YDLLogger as FakeLogger
def handler(request):
    RH_KEY = getattr(request, 'param', None)
    if not RH_KEY:
    if inspect.isclass(RH_KEY) and issubclass(RH_KEY, RequestHandler):
        handler = RH_KEY
    elif RH_KEY in _REQUEST_HANDLERS:
        handler = _REQUEST_HANDLERS[RH_KEY]
        pytest.skip(f'{RH_KEY} request handler is not available')
    class HandlerWrapper(handler):
        RH_KEY = handler.RH_KEY
        def __init__(self, **kwargs):
            super().__init__(logger=FakeLogger, **kwargs)
    return HandlerWrapper
def skip_handler(request, handler):
    """usage: pytest.mark.skip_handler('my_handler', 'reason')"""
    for marker in request.node.iter_markers('skip_handler'):
        if marker.args[0] == handler.RH_KEY:
            pytest.skip(marker.args[1] if len(marker.args) > 1 else '')
def skip_handler_if(request, handler):
    """usage: pytest.mark.skip_handler_if('my_handler', lambda request: True, 'reason')"""
    for marker in request.node.iter_markers('skip_handler_if'):
        if marker.args[0] == handler.RH_KEY and marker.args[1](request):
            pytest.skip(marker.args[2] if len(marker.args) > 2 else '')
def skip_handlers_if(request, handler):
    """usage: pytest.mark.skip_handlers_if(lambda request, handler: True, 'reason')"""
    for marker in request.node.iter_markers('skip_handlers_if'):
        if handler and marker.args[0](request, handler):
def handler_flaky(request, handler):
    """Mark a certain handler as being flaky.
    This will skip the test if pytest does not get run using `--allow-flaky`
    usage:
        pytest.mark.handler_flaky('my_handler', os.name != 'nt', reason='reason')
    for marker in request.node.iter_markers(handler_flaky.__name__):
            marker.args[0] == handler.RH_KEY
            and (not marker.args[1:] or any(marker.args[1:]))
            and request.config.getoption('disallow_flaky')
            reason = marker.kwargs.get('reason')
            pytest.skip(f'flaky: {reason}' if reason else 'flaky')
def pytest_addoption(parser, pluginmanager):
    parser.addoption(
        '--disallow-flaky',
        help='disallow flaky tests from running.',
def pytest_configure(config):
    config.addinivalue_line(
        'markers', 'skip_handler(handler): skip test for the given handler',
        'markers', 'skip_handler_if(handler): skip test for the given handler if condition is true',
        'markers', 'skip_handlers_if(handler): skip test for handlers when the condition is true',
        'markers', 'handler_flaky(handler): mark handler as flaky if condition is true',
import yt_dlp.globals
from yt_dlp.extractor.common import InfoExtractor
_TESTDATA_PATH = pathlib.Path(__file__).parent.parent / 'testdata/sigs'
_player_re = re.compile(r'^.+/player/(?P<id>[a-zA-Z0-9_/.-]+)\.js$')
_player_id_trans = str.maketrans(dict.fromkeys('/.-', '_'))
def ie() -> InfoExtractor:
    runtime_names = yt_dlp.globals.supported_js_runtimes.value
    ydl = YoutubeDL({'js_runtimes': {key: {} for key in runtime_names}})
    ie = ydl.get_info_extractor('Youtube')
    def _load_player(video_id, player_url, fatal=True):
        match = _player_re.match(player_url)
        test_id = match.group('id').translate(_player_id_trans)
        cached_file = _TESTDATA_PATH / f'player-{test_id}.js'
        if cached_file.exists():
            return cached_file.read_text()
        if code := ie._download_webpage(player_url, video_id, fatal=fatal):
            _TESTDATA_PATH.mkdir(exist_ok=True, parents=True)
            cached_file.write_text(code)
    ie._load_player = _load_player
    return ie
class MockLogger:
    def trace(self, message: str):
        print(f'trace: {message}')
    def debug(self, message: str, *, once=False):
        print(f'debug: {message}')
    def info(self, message: str):
        print(f'info: {message}')
    def warning(self, message: str, *, once=False):
        print(f'warning: {message}')
    def error(self, message: str):
        print(f'error: {message}')
def logger():
    return MockLogger()
from yt_dlp.cookies import YoutubeDLCookieJar
from yt_dlp.extractor.youtube.pot._provider import IEContentProviderLogger
from yt_dlp.extractor.youtube.pot.provider import PoTokenRequest, PoTokenContext
from yt_dlp.utils.networking import HTTPHeaderDict
class MockLogger(IEContentProviderLogger):
    log_level = IEContentProviderLogger.LogLevel.TRACE
        super().__init__(*args, **kwargs)
        self.messages = collections.defaultdict(list)
        self.messages['trace'].append(message)
    def debug(self, message: str):
        self.messages['debug'].append(message)
        self.messages['info'].append(message)
        self.messages['warning'].append(message)
        self.messages['error'].append(message)
    ydl = YoutubeDL()
    return ydl.get_info_extractor('Youtube')
def logger() -> MockLogger:
def pot_request() -> PoTokenRequest:
    return PoTokenRequest(
        context=PoTokenContext.GVS,
        innertube_context={'client': {'clientName': 'WEB'}},
        innertube_host='youtube.com',
        session_index=None,
        player_url=None,
        is_authenticated=False,
        video_webpage=None,
        visitor_data='example-visitor-data',
        data_sync_id='example-data-sync-id',
        video_id='example-video-id',
        request_cookiejar=YoutubeDLCookieJar(),
        request_proxy=None,
        request_headers=HTTPHeaderDict(),
        request_timeout=None,
        request_source_address=None,
        request_verify_tls=True,
        bypass_cache=False,
from spotdl.utils import ffmpeg
from spotdl.utils.logging import init_logging
ORIGINAL_INITIALIZE = SpotifyClient.init
    "ad996353310b4ced82f5be1309b11b14", "2e5851cff3bc45f495cd7cfa40be1b48"
init_logging("MATCH")
class FakeProcess:
    """Instead of running ffmpeg, just fake it"""
    def __init__(self, *args):
        command = list(*args)
        self._input = Path(command[command.index("-i") + 1])
        self._output = Path(command[-1])
    def communicate(self):
        Ensure that the file has been download, and create empty output file,
        to avoid infinite loop.
        assert self._input.is_file()
        self._output.open("w").close()
        return (None, None)
    def wait(self):
    def returncode(self):
def new_initialize(
    auth_token=None,
    user_auth=False,
    cache_path=None,
    no_cache=True,
    headless=True,
    max_retries=3,
    use_cache_file=False,
    """This function allows calling `initialize()` multiple times"""
        return SpotifyClient()
        return ORIGINAL_INITIALIZE(
            client_id="ad996353310b4ced82f5be1309b11b14",
            client_secret="2e5851cff3bc45f495cd7cfa40be1b48",
            auth_token=auth_token,
            use_cache_file=use_cache_file,
def fake_create_subprocess_exec(*args, stdout=None, stderr=None, **kwargs):
    return FakeProcess(args)
def patch_dependencies(mocker, monkeypatch):
    This function is called before each test.
    monkeypatch.setattr(SpotifyClient, "init", new_initialize)
    monkeypatch.setattr(subprocess, "Popen", fake_create_subprocess_exec)
    monkeypatch.setattr(ffmpeg, "get_ffmpeg_version", lambda *_: (4.4, 2022))
    mocker.patch.object(Downloader, "download_song", autospec=True)
    mocker.patch.object(Downloader, "download_multiple_songs", autospec=True)
def clean_ansi_sequence(text):
    Remove ANSI escape sequences from text
        r"(?:\x1B[@-Z\\-_]|[\x80-\x9A\x9C-\x9F]|(?:\x1B\[|\x9B)[0-?]*[ -/]*[@-~])",
# Set a handler for the root-logger to inhibit 'basicConfig()' (called in PyInstaller.log) is setting up a stream
# handler writing to stderr. This avoids log messages to be written (and captured) twice: once on stderr and
# once by pytests's caplog.
logging.getLogger().addHandler(logging.NullHandler())
# psutil is used for process tree clean-up on time-out when running the test frozen application. If unavailable
# (for example, on cygwin), we fall back to trying to terminate only the main application process.
    import psutil  # noqa: E402
    psutil = None
import pytest  # noqa: E402
from PyInstaller import __main__ as pyi_main  # noqa: E402
from PyInstaller import configure  # noqa: E402
from PyInstaller.compat import is_cygwin, is_darwin, is_win  # noqa: E402
from PyInstaller.depend.analysis import initialize_modgraph  # noqa: E402
from PyInstaller.archive.readers import pkg_archive_contents  # noqa: E402
from PyInstaller.utils.tests import gen_sourcefile  # noqa: E402
from PyInstaller.utils.win32 import winutils  # noqa: E402
# Timeout for running the executable. If executable does not exit in this time, it is interpreted as a test failure.
_EXE_TIMEOUT = 3 * 60  # In sec.
# All currently supported platforms
SUPPORTED_OSES = {"darwin", "linux", "win32"}
# Have pyi_builder fixure clean-up the temporary directories of successful tests. Controlled by environment variable.
_PYI_BUILDER_CLEANUP = os.environ.get("PYI_BUILDER_CLEANUP", "1") == "1"
# Fixtures
# --------
def pytest_runtest_setup(item):
    Markers to skip tests based on the current platform.
    https://pytest.org/en/stable/example/markers.html#marking-platform-specific-tests-with-pytest
    Available markers: see pytest.ini markers
        - @pytest.mark.darwin (macOS)
        - @pytest.mark.linux (GNU/Linux)
        - @pytest.mark.win32 (Windows)
    supported_platforms = SUPPORTED_OSES.intersection(mark.name for mark in item.iter_markers())
    plat = sys.platform
    if plat == 'android':
        plat = 'linux'
    if supported_platforms and plat not in supported_platforms:
        pytest.skip(f"does not run on {plat}")
@pytest.hookimpl(tryfirst=True, hookwrapper=True)
def pytest_runtest_makereport(item, call):
    # Execute all other hooks to obtain the report object.
    outcome = yield
    rep = outcome.get_result()
    # Set a report attribute for each phase of a call, which can be "setup", "call", "teardown".
    setattr(item, f"rep_{rep.when}", rep)
# Return the base directory which contains the current test module.
def _get_base_dir(request):
    return request.path.resolve().parent  # pathlib.Path instance
# Directory with Python scripts for functional tests.
def _get_script_dir(request):
    return _get_base_dir(request) / 'scripts'
# Directory with testing modules used in some tests.
def _get_modules_dir(request):
    return _get_base_dir(request) / 'modules'
# Directory with .toc log files.
def _get_logs_dir(request):
    return _get_base_dir(request) / 'logs'
# Return the directory where data for tests is located.
def _get_data_dir(request):
    return _get_base_dir(request) / 'data'
# Directory with .spec files used in some tests.
def _get_spec_dir(request):
    return _get_base_dir(request) / 'specs'
def spec_dir(request):
    Return the directory where the test spec-files reside.
    return _get_spec_dir(request)
def script_dir(request):
    Return the directory where the test scripts reside.
    return _get_script_dir(request)
# A fixture that copies test's data directory into test's temporary directory. The data directory is assumed to be
# `data/{test-name}` found next to the .py file that contains test.
def data_dir(
    # The request object for this test. Used to infer name of the test and location of the source .py file.
    # See
    # https://pytest.org/latest/builtin.html#_pytest.python.FixtureRequest
    # https://pytest.org/latest/fixture.html#fixtures-can-introspect-the-requesting-test-context.
    # The tmp_path object for this test. See: https://pytest.org/latest/tmp_path.html.
    tmp_path
    # Strip the leading 'test_' from the test's name.
    test_name = request.function.__name__[5:]
    # Copy to data dir and return the path.
    source_data_dir = _get_data_dir(request) / test_name
    tmp_data_dir = tmp_path / 'data'
    # Copy the data.
    shutil.copytree(source_data_dir, tmp_data_dir)
    # Return the temporary data directory, so that the copied data can now be used.
    return tmp_data_dir
class AppBuilder:
    def __init__(self, tmp_path, request, bundle_mode):
        self._tmp_path = tmp_path
        self._mode = bundle_mode
        self._spec_dir = tmp_path
        self._dist_dir = tmp_path / 'dist'
        self._build_dir = tmp_path / 'build'
        self._is_spec = False
    def test_spec(self, specfile, *args, **kwargs):
        Test a Python script that is referenced in the supplied .spec file.
        __tracebackhide__ = True
        specfile = _get_spec_dir(self._request) / specfile
        # 'test_script' should handle .spec properly as script.
        self._is_spec = True
        return self.test_script(specfile, *args, **kwargs)
    def test_source(self, source, *args, **kwargs):
        Test a Python script given as source code.
        The source will be written into a file named like the test-function. This file will then be passed to
        `test_script`. If you need other related file, e.g., as `.toc`-file for testing the content, put it at at the
        normal place. Just mind to take the basnename from the test-function's name.
        :param script: Source code to create executable from. This will be saved into a temporary file which is then
                       passed on to `test_script`.
        :param test_id: Test-id for parametrized tests. If given, it will be appended to the script filename, separated
                        by two underscores.
        All other arguments are passed straight on to `test_script`.
        # For parametrized test append the test-id.
        scriptfile = gen_sourcefile(self._tmp_path, source, kwargs.setdefault('test_id'))
        del kwargs['test_id']
        return self.test_script(scriptfile, *args, **kwargs)
    def _display_message(self, step_name, message):
        # Print the given message to both stderr and stdout, and it with APP-BUILDER to make it clear where it
        # originates from.
        print(f'[APP-BUILDER:{step_name}] {message}', file=sys.stdout)
        print(f'[APP-BUILDER:{step_name}] {message}', file=sys.stderr)
    def test_script(
        self, script, pyi_args=None, app_name=None, app_args=None, runtime=None, run_from_path=False, **kwargs
        Main method to wrap all phases of testing a Python script.
        :param script: Name of script to create executable from.
        :param pyi_args: Additional arguments to pass to PyInstaller when creating executable.
        :param app_name: Name of the executable. This is equivalent to argument --name=APPNAME.
        :param app_args: Additional arguments to pass to
        :param runtime: Time in seconds how long to keep executable running.
        :param toc_log: List of modules that are expected to be bundled with the executable.
        # Skip interactive tests (the ones with `runtime` set) if `psutil` is unavailable, as we need it to properly
        # clean up the process tree.
        if runtime and psutil is None:
            pytest.skip('Interactive tests require psutil for proper cleanup.')
            pyi_args = []
        if app_args is None:
            app_args = []
        if app_name:
            if not self._is_spec:
                pyi_args.extend(['--name', app_name])
            # Derive name from script name.
            app_name = os.path.splitext(os.path.basename(script))[0]
        # Relative path means that a script from _script_dir is referenced.
        if not os.path.isabs(script):
            script = _get_script_dir(self._request) / script
        self.script = str(script)  # might be a pathlib.Path at this point!
        assert os.path.exists(self.script), f'Script {self.script!r} not found.'
        self._display_message('TEST-SCRIPT', 'Starting build...')
        if not self._test_building(args=pyi_args):
            pytest.fail(f'Building of {script} failed.')
        self._display_message('TEST-SCRIPT', 'Build finished, now running executable...')
        self._test_executables(app_name, args=app_args, runtime=runtime, run_from_path=run_from_path, **kwargs)
        self._display_message('TEST-SCRIPT', 'Running executable finished.')
    def _test_executables(self, name, args, runtime, run_from_path, **kwargs):
        Run created executable to make sure it works.
        Multipackage-tests generate more than one exe-file and all of them have to be run.
        :param args: CLI options to pass to the created executable.
        :param runtime: Time in seconds how long to keep the executable running.
        :return: Exit code of the executable.
        exes = self._find_executables(name)
        # Empty list means that PyInstaller probably failed to create any executable.
        assert exes != [], 'No executable file was found.'
        for exe in exes:
            # Try to find .toc log file. .toc log file has the same basename as exe file.
            toc_log = os.path.splitext(os.path.basename(exe))[0] + '.toc'
            toc_log = _get_logs_dir(self._request) / toc_log
            if toc_log.exists():
                if not self._examine_executable(exe, toc_log):
                    pytest.fail(f'Matching .toc of {exe} failed.')
            retcode = self._run_executable(exe, args, run_from_path, runtime)
            if retcode != kwargs.get('retcode', 0):
                pytest.fail(f'Running exe {exe} failed with return-code {retcode}.')
    def _find_executables(self, name):
        Search for all executables generated by the testcase.
        If the test-case is called e.g. 'test_multipackage1', this is searching for each of 'test_multipackage1.exe'
        and 'multipackage1_?.exe' in both one-file- and one-dir-mode.
        :param name: Name of the executable to look for.
        :return: List of executables
        exes = []
        onedir_pt = str(self._dist_dir / name / name)
        onefile_pt = str(self._dist_dir / name)
        patterns = [
            onedir_pt,
            onefile_pt,
            # Multipackage one-dir
            onedir_pt + '_?',
            # Multipackage one-file
            onefile_pt + '_?'
        # For Windows append .exe extension to patterns.
            patterns = [pt + '.exe' for pt in patterns]
        # For macOS append pattern for .app bundles.
            # e.g:  ./dist/name.app/Contents/MacOS/name
            app_bundle_pt = str(self._dist_dir / f'{name}.app' / 'Contents' / 'MacOS' / name)
            patterns.append(app_bundle_pt)
        # Apply file patterns.
        for pattern in patterns:
            for prog in glob.glob(pattern):
                if os.path.isfile(prog):
                    exes.append(prog)
        return exes
    def _run_executable(self, prog, args, run_from_path, runtime):
        Run executable created by PyInstaller.
        # Run the test in a clean environment to make sure they're really self-contained.
        prog_env = copy.deepcopy(os.environ)
        prog_env['PATH'] = ''
        del prog_env['PATH']
        # For Windows we need to keep minimal PATH for successful running of some tests.
            # Minimum Windows PATH is in most cases:   C:\Windows\system32;C:\Windows
            prog_env['PATH'] = os.pathsep.join(winutils.get_system_path())
        # Same for Cygwin - if /usr/bin is not in PATH, cygwin1.dll cannot be discovered.
        if is_cygwin:
            prog_env['PATH'] = os.pathsep.join(['/usr/local/bin', '/usr/bin'])
        # On macOS, we similarly set up minimal PATH with system directories, in case utilities from there are used by
        # tested python code (for example, matplotlib >= 3.9.0 uses `system_profiler` that is found in /usr/sbin).
            # The following paths are registered when application is launched via Finder, and are a subset of what is
            # typically available in the shell.
            prog_env['PATH'] = os.pathsep.join(['/usr/bin', '/bin', '/usr/sbin', '/sbin'])
        exe_path = prog
        if run_from_path:
            # Run executable in the temp directory. Add the directory containing the executable to $PATH. Basically,
            # pretend we are a shell executing the program from $PATH.
            prog_cwd = str(self._tmp_path)
            prog_name = os.path.basename(prog)
            prog_env['PATH'] = os.pathsep.join([prog_env.get('PATH', ''), os.path.dirname(prog)])
            # Run executable in the directory where it is.
            prog_cwd = os.path.dirname(prog)
            # The executable will be called with argv[0] as relative not absolute path.
            prog_name = os.path.join(os.curdir, os.path.basename(prog))
        args = [prog_name] + args
        # Using sys.stdout/sys.stderr for subprocess fixes printing messages in Windows command prompt. Py.test is then
        # able to collect stdout/sterr messages and display them if a test fails.
        return self._run_executable_(args, exe_path, prog_env, prog_cwd, runtime)
    def _run_executable_(self, args, exe_path, prog_env, prog_cwd, runtime):
        # Use psutil.Popen, if available; otherwise, fall back to subprocess.Popen
        popen_implementation = subprocess.Popen if psutil is None else psutil.Popen
        # Run the executable
        self._display_message('RUN-EXE', f'Running {exe_path!r}, args: {args!r}')
        process = popen_implementation(args, executable=exe_path, env=prog_env, cwd=prog_cwd)
        # Wait for the process to finish. If no run-time (= timeout) is specified, we expect the process to exit on
        # its own, and use global _EXE_TIMEOUT. If run-time is specified, we expect the application to be running
        # for at least the specified amount of time, which is useful in "interactive" test applications that are not
        # expected exit on their own.
            timeout = runtime if runtime else _EXE_TIMEOUT
            stdout, stderr = process.communicate(timeout=timeout)
            elapsed = time.time() - start_time
            retcode = process.returncode
            self._display_message(
                'RUN-EXE', f'Process exited on its own after {elapsed:.1f} seconds with return code {retcode}.'
        except (subprocess.TimeoutExpired) if psutil is None else (psutil.TimeoutExpired, subprocess.TimeoutExpired):
            if runtime:
                # When 'runtime' is set, the expired timeout is a good sign that the executable was running successfully
                # for the specified time.
                self._display_message('RUN-EXE', f'Process reached expected run-time of {runtime} seconds.')
                retcode = 0
                # Executable is still running and it is not interactive. Clean up the process tree, and fail the test.
                self._display_message('RUN-EXE', f'Timeout while running executable (timeout: {timeout} seconds)!')
                retcode = 1
            if psutil is None:
                # We are using subprocess.Popen(). Without psutil, we have no access to process tree; this poses a
                # problem for onefile builds, where we would need to first kill the child (main application) process,
                # and let the onefile parent perform its cleanup. As a best-effort approach, we can first call
                # process.terminate(); on POSIX systems, this sends SIGTERM to the parent process, and in most
                # situations, the bootloader will forward it to the child process. Then wait 5 seconds, and call
                # process.kill() if necessary. On Windows, however, both process.terminate() and process.kill() do
                # the same. Therefore, we should avoid running "interactive" tests with expected run-time if we do
                # not have psutil available.
                    self._display_message('RUN-EXE', 'Stopping the process using Popen.terminate()...')
                    process.terminate()
                    stdout, stderr = process.communicate(timeout=5)
                    self._display_message('RUN-EXE', 'Process stopped.')
                except subprocess.TimeoutExpired:
                    # Kill the process.
                        self._display_message('RUN-EXE', 'Stopping the process using Popen.kill()...')
                        process.kill()
                        # process.communicate() waits for end-of-file, which may never arrive if there is a child
                        # process still alive. Nothing we can really do about it here, so add a short timeout and
                        # display a warning.
                        stdout, stderr = process.communicate(timeout=1)
                        self._display_message('RUN-EXE', 'Failed to stop the process (or its child process(es))!')
                # We are using psutil.Popen(). First, force-kill all child processes; in onefile mode, this includes
                # the application process, whose termination should trigger cleanup and exit of the parent onefile
                self._display_message('RUN-EXE', 'Stopping child processes...')
                for child_process in list(process.children(recursive=True)):
                    with contextlib.suppress(psutil.NoSuchProcess):
                        self._display_message('RUN-EXE', f'Stopping child process {child_process.pid}...')
                        child_process.kill()
                # Give the main process 5 seconds to exit on its own (to accommodate cleanup in onefile mode).
                    self._display_message('RUN-EXE', f'Waiting for main process ({process.pid}) to stop...')
                    self._display_message('RUN-EXE', 'Process stopped on its own.')
                except (psutil.TimeoutExpired, subprocess.TimeoutExpired):
                    # End of the line - kill the main process.
                    # Try to retrieve stdout/stderr - but keep a short timeout, just in case...
                    except (psutil.TimeoutExpired, subprocess.TimeoutExpire):
        self._display_message('RUN-EXE', f'Done! Return code: {retcode}')
    def _test_building(self, args):
        Run building of test script.
        :param args: additional CLI options for PyInstaller.
        Return True if build succeeded False otherwise.
        if self._is_spec:
            default_args = [
                '--distpath', str(self._dist_dir),
                '--workpath', str(self._build_dir),
                '--log-level', 'INFO',
            ]  # yapf: disable
                '--debug=bootloader',
                '--noupx',
                '--specpath', str(self._spec_dir),
                '--path', str(_get_modules_dir(self._request)),
            # Choose bundle mode.
            if self._mode == 'onedir':
                default_args.append('--onedir')
            elif self._mode == 'onefile':
                default_args.append('--onefile')
            # if self._mode is None then just the spec file was supplied.
        pyi_args = [self.script, *default_args, *args]
        # TODO: fix return code in running PyInstaller programmatically.
        PYI_CONFIG = configure.get_config()
        # Override CACHEDIR for PyInstaller; relocate cache into `self._tmp_path`.
        PYI_CONFIG['cachedir'] = str(self._tmp_path)
        pyi_main.run(pyi_args, PYI_CONFIG)
        return retcode == 0
    def _examine_executable(self, exe, toc_log):
        Compare log files (now used mostly by multipackage test_name).
        :return: True if .toc files match
        self._display_message('EXAMINE-EXE', f'Matching against TOC log: {str(toc_log)!r}')
        fname_list = pkg_archive_contents(exe)
        with open(toc_log, 'r', encoding='utf-8') as f:
            pattern_list = eval(f.read())
        # Alphabetical order of patterns.
        pattern_list.sort()
        missing = []
        for pattern in pattern_list:
            for fname in fname_list:
                if re.match(pattern, fname):
                    self._display_message('EXAMINE-EXE', f'Entry found: {pattern!r} --> {fname!r}')
                # No matching entry found
                missing.append(pattern)
                self._display_message('EXAMINE-EXE', f'Entry MISSING: {pattern!r}')
        # We expect the missing list to be empty
        return not missing
# Scope 'session' should keep the object unchanged for whole tests. This fixture caches basic module graph dependencies
# that are same for every executable.
@pytest.fixture(scope='session')
def pyi_modgraph():
    # Explicitly set the log level since the plugin `pytest-catchlog` (un-) sets the root logger's level to NOTSET for
    # the setup phase, which will lead to TRACE messages been written out.
    import PyInstaller.log as logging
    logging.logger.setLevel(logging.DEBUG)
    initialize_modgraph()
# Run by default test as onedir and onefile.
@pytest.fixture(params=['onedir', 'onefile'])
def pyi_builder(tmp_path, monkeypatch, request, pyi_modgraph):
    # Save/restore environment variable PATH.
    monkeypatch.setenv('PATH', os.environ['PATH'])
    # PyInstaller or a test case might manipulate 'sys.path'. Reset it for every test.
    monkeypatch.syspath_prepend(None)
    # Set current working directory to
    monkeypatch.chdir(tmp_path)
    # Clean up configuration and force PyInstaller to do a clean configuration for another app/test. The value is same
    # as the original value.
    monkeypatch.setattr('PyInstaller.config.CONF', {'pathex': []})
    yield AppBuilder(tmp_path, request, request.param)
    # Clean up the temporary directory of a successful test
    if _PYI_BUILDER_CLEANUP and request.node.rep_setup.passed and request.node.rep_call.passed:
        if tmp_path.exists():
            shutil.rmtree(tmp_path, ignore_errors=True)
# Fixture for .spec based tests. With .spec it does not make sense to differentiate onefile/onedir mode.
def pyi_builder_spec(tmp_path, request, monkeypatch, pyi_modgraph):
    yield AppBuilder(tmp_path, request, None)
def pyi_windowed_builder(pyi_builder: AppBuilder):
    """A pyi_builder equivalent for testing --windowed applications."""
    # psutil.Popen() somehow bypasses an application's windowed/console mode so that any application built in
    # --windowed mode but invoked with psutil still receives valid std{in,out,err} handles and behaves exactly like
    # a console application. In short, testing windowed mode with psutil is a null test. We must instead use subprocess.
    def _run_executable_(args, exe_path, prog_env, prog_cwd, runtime):
        return subprocess.run([exe_path, *args], env=prog_env, cwd=prog_cwd, timeout=runtime).returncode
    pyi_builder._run_executable_ = _run_executable_
    yield pyi_builder
from PyInstaller.compat import is_win, is_darwin
# Bring all common fixtures into this file.
from PyInstaller.utils.conftest import *  # noqa: F401, F403
# A fixture that compiles a test shared library from `data/load_dll_using_ctypes/ctypes_dylib.c` in a sub-directory
# of the tmp_dir, and returns path to the compiled shared library.
# NOTE: for this fixture to be session-scoped, we need to define it here (as opposed to `PyInstaller.utils.confest`).
# This is because its data directory needs to be resolved based on this module's location. (And even if we were to use
# test-scoped fixture and infer location from `request.path`, the fixture would be valid only for test files from this
# directory).
def compiled_dylib(tmp_path_factory):
    # Copy the source to temporary directory.
    orig_source_dir = pathlib.Path(__file__).parent / 'data' / 'ctypes_dylib'
    tmp_source_dir = tmp_path_factory.mktemp('compiled_ctypes_dylib')
    shutil.copy2(orig_source_dir / 'ctypes_dylib.c', tmp_source_dir)
    # Compile shared library using `distuils.ccompiler` module. The code is loosely based on implementation of the
    # `distutils.command.build_ext` command module.
    def _compile_dylib(source_dir):
        # Until python 3.12, `distutils` was part of standard library. For newer python versions, `setuptools` provides
        # its vendored copy. If neither are available, skip the test.
            import distutils.ccompiler
            import distutils.sysconfig
            pytest.skip('distutils.ccompiler is not available')
        # Set up compiler
        compiler = distutils.ccompiler.new_compiler()
        distutils.sysconfig.customize_compiler(compiler)
        if hasattr(compiler, 'initialize'):  # Applicable to MSVCCompiler on Windows.
            compiler.initialize()
            # With MinGW compiler, the `customize_compiler()` call ends up changing `compiler.shared_lib_extension` into
            # ".pyd". Use ".dll" instead.
            suffix = '.dll'
            # On macOS, `compiler.shared_lib_extension` is ".so", but ".dylib" is more appropriate.
            suffix = '.dylib'
            suffix = compiler.shared_lib_extension
        # Change the current working directory to the directory that contains source files. Ideally, we could pass the
        # absolute path to sources to `compiler.compile()` and set its `output_dir` argument to the directory where
        # object files should be generated. However, in this case, the object files are put under output directory
        # *while retaining their original path component*. If `output_dir` is not specified, then the original absolute
        # source file paths seem to be turned into relative ones (e.g, on Linux, the leading / is stripped away).
        # NOTE: with python >= 3.11 we could use contextlib.chdir().
        old_cwd = pathlib.Path.cwd()
        os.chdir(source_dir)
            # Compile source .c file into object
                'ctypes_dylib.c',
            objects = compiler.compile(sources)
            # Link into shared library
            output_filename = f'ctypes_dylib{suffix}'
            compiler.link_shared_object(
                output_filename,
                target_lang='c',
                export_symbols=['add_twelve'],
            os.chdir(old_cwd)  # Restore old working directory.
        # Return path to compiled shared library
        return source_dir / output_filename
        return _compile_dylib(tmp_source_dir)
        pytest.skip(f"Could not compile test shared library: {e}")
# Expand sys.path with PyInstaller source.
_ROOT_DIR = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.append(_ROOT_DIR)
# Bring all fixtures into this file.
from PyInstaller.utils.conftest import *  # noqa
from typer.testing import CliRunner
skip_on_windows = pytest.mark.skipif(
    sys.platform == "win32", reason="Skipping on Windows"
THIS_DIR = Path(__file__).parent.resolve()
def pytest_collection_modifyitems(config, items: list[pytest.Item]) -> None:
    if sys.platform != "win32":
        item_path = Path(item.fspath).resolve()
        if item_path.is_relative_to(THIS_DIR):
            item.add_marker(skip_on_windows)
@pytest.fixture(name="runner")
def get_runner():
    runner = CliRunner()
    with runner.isolated_filesystem():
        yield runner
@pytest.fixture(name="root_dir")
def prepare_paths(runner):
    docs_dir = Path("docs")
    en_docs_dir = docs_dir / "en" / "docs"
    lang_docs_dir = docs_dir / "lang" / "docs"
    en_docs_dir.mkdir(parents=True, exist_ok=True)
    lang_docs_dir.mkdir(parents=True, exist_ok=True)
    yield Path.cwd()
def copy_test_files(root_dir: Path, request: pytest.FixtureRequest):
    en_file_path = Path(request.param[0])
    translation_file_path = Path(request.param[1])
    shutil.copy(str(en_file_path), str(root_dir / "docs" / "en" / "docs" / "doc.md"))
    shutil.copy(
        str(translation_file_path), str(root_dir / "docs" / "lang" / "docs" / "doc.md")
"""Configuration for unit tests."""
from collections.abc import Iterator, Sequence
from importlib import util
from blockbuster import BlockBuster, blockbuster_ctx
from pytest_mock import MockerFixture
def blockbuster() -> Iterator[BlockBuster]:
    with blockbuster_ctx("langchain_core") as bb:
        for func in ["os.stat", "os.path.abspath"]:
                bb.functions[func]
                .can_block_in("langchain_core/_api/internal.py", "is_caller_internal")
                .can_block_in("langchain_core/runnables/base.py", "__repr__")
        for func in ["os.stat", "io.TextIOWrapper.read"]:
            bb.functions[func].can_block_in(
                "langsmith/client.py", "_default_retry_config"
        for bb_function in bb.functions.values():
            bb_function.can_block_in(
                "freezegun/api.py", "_get_cached_module_attributes"
        yield bb
def pytest_addoption(parser: pytest.Parser) -> None:
    """Add custom command line options to pytest."""
        "--only-extended",
        help="Only run extended tests. Does not allow skipping any extended tests.",
        "--only-core",
        help="Only run core tests. Never runs any extended tests.",
def pytest_collection_modifyitems(
    config: pytest.Config, items: Sequence[pytest.Function]
    """Add implementations for handling custom markers.
    At the moment, this adds support for a custom `requires` marker.
    The `requires` marker is used to denote tests that require one or more packages
    to be installed to run. If the package is not installed, the test is skipped.
    The `requires` marker syntax is:
    @pytest.mark.requires("package1", "package2")
    def test_something(): ...
    # Mapping from the name of a package to whether it is installed or not.
    # Used to avoid repeated calls to `util.find_spec`
    required_pkgs_info: dict[str, bool] = {}
    only_extended = config.getoption("--only-extended") or False
    only_core = config.getoption("--only-core") or False
    if only_extended and only_core:
        msg = "Cannot specify both `--only-extended` and `--only-core`."
        requires_marker = item.get_closest_marker("requires")
        if requires_marker is not None:
            if only_core:
                item.add_marker(pytest.mark.skip(reason="Skipping not a core test."))
            # Iterate through the list of required packages
            required_pkgs = requires_marker.args
            for pkg in required_pkgs:
                # If we haven't yet checked whether the pkg is installed
                # let's check it and store the result.
                if pkg not in required_pkgs_info:
                        installed = util.find_spec(pkg) is not None
                        installed = False
                    required_pkgs_info[pkg] = installed
                if not required_pkgs_info[pkg]:
                    if only_extended:
                        pytest.fail(
                            f"Package `{pkg}` is not installed but is required for "
                            f"extended tests. Please install the given package and "
                            f"try again.",
                        # If the package is not installed, we immediately break
                        # and mark the test as skipped.
                        item.add_marker(
                            pytest.mark.skip(reason=f"Requires pkg: `{pkg}`")
        elif only_extended:
            item.add_marker(pytest.mark.skip(reason="Skipping not an extended test."))
def deterministic_uuids(mocker: MockerFixture) -> MockerFixture:
    side_effect = (
        UUID(f"00000000-0000-4000-8000-{i:012}", version=4) for i in range(10000)
    return mocker.patch("uuid.uuid4", side_effect=side_effect)
# Getting the absolute path of the current file's directory
ABS_PATH = Path(__file__).resolve().parent
# Getting the absolute path of the project's root directory
PROJECT_DIR = ABS_PATH.parent.parent
# Loading the .env file if it exists
def _load_env() -> None:
    dotenv_path = PROJECT_DIR / "tests" / "integration_tests" / ".env"
    if dotenv_path.exists():
        from dotenv import load_dotenv
        load_dotenv(dotenv_path)
_load_env()
@pytest.fixture(scope="module")
def test_dir() -> Path:
    return PROJECT_DIR / "tests" / "integration_tests"
# This fixture returns a string containing the path to the cassette directory for the
# current module
def vcr_cassette_dir(request: pytest.FixtureRequest) -> str:
    module = Path(request.module.__file__)
    return str(module.parent / "cassettes" / module.stem)
        "--community",
        dest="community",
        help="enable running unite tests that require community",
    only_extended = config.getoption("--only-extended", default=False)
    only_core = config.getoption("--only-core", default=False)
    if not config.getoption("--community", default=False):
        skip_community = pytest.mark.skip(reason="need --community option to run")
            if "community" in item.keywords:
                item.add_marker(skip_community)
                    required_pkgs_info[pkg] = util.find_spec(pkg) is not None
                            pytest.mark.skip(reason=f"Requires pkg: `{pkg}`"),
                pytest.mark.skip(reason="Skipping not an extended test."),
from langchain_tests.conftest import CustomPersister, CustomSerializer, base_vcr_config
from vcr import VCR
_EXTRA_HEADERS = [
    ("openai-organization", "PLACEHOLDER"),
    ("user-agent", "PLACEHOLDER"),
    ("x-openai-client-user-agent", "PLACEHOLDER"),
    with blockbuster_ctx() as bb:
def remove_request_headers(request: Any) -> Any:
    """Remove sensitive headers from the request."""
    for k in request.headers:
        request.headers[k] = "**REDACTED**"
    request.uri = "**REDACTED**"
def remove_response_headers(response: dict[str, Any]) -> dict[str, Any]:
    """Remove sensitive headers from the response."""
    for k in response["headers"]:
        response["headers"][k] = "**REDACTED**"
def vcr_config() -> dict[str, Any]:
    """Extend the default configuration coming from langchain_tests."""
    config = base_vcr_config()
    config["match_on"] = [m if m != "body" else "json_body" for m in config.get("match_on", [])]
    config.setdefault("filter_headers", []).extend(_EXTRA_HEADERS)
    config["before_record_request"] = remove_request_headers
    config["before_record_response"] = remove_response_headers
    config["serializer"] = "yaml.gz"
    config["path_transformer"] = VCR.ensure_suffix(".yaml.gz")
def _json_body_matcher(r1: Any, r2: Any) -> None:
    """Match request bodies as parsed JSON, ignoring key order."""
    b1 = r1.body or b""
    b2 = r2.body or b""
    if isinstance(b1, bytes):
        b1 = b1.decode("utf-8")
    if isinstance(b2, bytes):
        b2 = b2.decode("utf-8")
        j1 = json.loads(b1)
        j2 = json.loads(b2)
    except (json.JSONDecodeError, ValueError):
        assert b1 == b2, f"body mismatch (non-JSON):\n{b1}\n!=\n{b2}"
    assert j1 == j2, f"body mismatch:\n{j1}\n!=\n{j2}"
def pytest_recording_configure(config: dict[str, Any], vcr: VCR) -> None:  # noqa: ARG001
    vcr.register_persister(CustomPersister())
    vcr.register_serializer("yaml.gz", CustomSerializer())
    vcr.register_matcher("json_body", _json_body_matcher)
def pytest_collection_modifyitems(config: pytest.Config, items: Sequence[pytest.Function]) -> None:
from langgraph.checkpoint.base import BaseCheckpointSaver
from langgraph.store.base import BaseStore
from tests.unit_tests.agents.conftest_checkpointer import (
    _checkpointer_memory,
    _checkpointer_postgres,
    _checkpointer_postgres_aio,
    _checkpointer_postgres_aio_pipe,
    _checkpointer_postgres_aio_pool,
    _checkpointer_postgres_pipe,
    _checkpointer_postgres_pool,
    _checkpointer_sqlite,
    _checkpointer_sqlite_aio,
from tests.unit_tests.agents.conftest_store import (
    _store_memory,
    _store_postgres,
    _store_postgres_aio,
    _store_postgres_aio_pipe,
    _store_postgres_aio_pool,
    _store_postgres_pipe,
    _store_postgres_pool,
# Global variables for checkpointer and store configurations
FAST_MODE = os.getenv("LANGGRAPH_TEST_FAST", "true").lower() in {"true", "1", "yes"}
SYNC_CHECKPOINTER_PARAMS = (
    ["memory"]
    if FAST_MODE
        "memory",
        "sqlite",
        "postgres",
        "postgres_pipe",
        "postgres_pool",
ASYNC_CHECKPOINTER_PARAMS = (
        "sqlite_aio",
        "postgres_aio",
        "postgres_aio_pipe",
        "postgres_aio_pool",
SYNC_STORE_PARAMS = (
    ["in_memory"]
        "in_memory",
ASYNC_STORE_PARAMS = (
def anyio_backend() -> str:
    return "asyncio"
    side_effect = (UUID(f"00000000-0000-4000-8000-{i:012}", version=4) for i in range(10000))
# checkpointer fixtures
@pytest.fixture(
    params=SYNC_STORE_PARAMS,
def sync_store(request: pytest.FixtureRequest) -> Iterator[BaseStore | None]:
    store_name = request.param
    if store_name is None:
    elif store_name == "in_memory":
        with _store_memory() as store:
            yield store
    elif store_name == "postgres":
        with _store_postgres() as store:
    elif store_name == "postgres_pipe":
        with _store_postgres_pipe() as store:
    elif store_name == "postgres_pool":
        with _store_postgres_pool() as store:
        msg = f"Unknown store {store_name}"
    params=ASYNC_STORE_PARAMS,
async def async_store(request: pytest.FixtureRequest) -> AsyncIterator[BaseStore | None]:
    elif store_name == "postgres_aio":
        async with _store_postgres_aio() as store:
    elif store_name == "postgres_aio_pipe":
        async with _store_postgres_aio_pipe() as store:
    elif store_name == "postgres_aio_pool":
        async with _store_postgres_aio_pool() as store:
    params=SYNC_CHECKPOINTER_PARAMS,
def sync_checkpointer(
    request: pytest.FixtureRequest,
) -> Iterator[BaseCheckpointSaver[str]]:
    checkpointer_name = request.param
    if checkpointer_name == "memory":
        with _checkpointer_memory() as checkpointer:
            yield checkpointer
    elif checkpointer_name == "sqlite":
        with _checkpointer_sqlite() as checkpointer:
    elif checkpointer_name == "postgres":
        with _checkpointer_postgres() as checkpointer:
    elif checkpointer_name == "postgres_pipe":
        with _checkpointer_postgres_pipe() as checkpointer:
    elif checkpointer_name == "postgres_pool":
        with _checkpointer_postgres_pool() as checkpointer:
        msg = f"Unknown checkpointer: {checkpointer_name}"
    params=ASYNC_CHECKPOINTER_PARAMS,
async def async_checkpointer(
) -> AsyncIterator[BaseCheckpointSaver[str]]:
    elif checkpointer_name == "sqlite_aio":
        async with _checkpointer_sqlite_aio() as checkpointer:
    elif checkpointer_name == "postgres_aio":
        async with _checkpointer_postgres_aio() as checkpointer:
    elif checkpointer_name == "postgres_aio_pipe":
        async with _checkpointer_postgres_aio_pipe() as checkpointer:
    elif checkpointer_name == "postgres_aio_pool":
        async with _checkpointer_postgres_aio_pool() as checkpointer:
from vcr import VCR  # type: ignore[import-untyped]
def remove_response_headers(response: dict) -> dict:
def vcr_config() -> dict:
def pytest_recording_configure(config: dict, vcr: VCR) -> None:
    config["match_on"] = [
        m if m != "body" else "json_body" for m in config.get("match_on", [])
"""Conftest for OpenRouter tests."""
    """Redact all request headers to avoid leaking secrets."""
    """Redact all response headers."""
def pytest_recording_configure(config: dict, vcr: VCR) -> None:  # noqa: ARG001
    """Register custom VCR persister and serializer."""
from qdrant_client import QdrantClient
from tests.integration_tests.fixtures import qdrant_locations
def pytest_runtest_teardown() -> None:
    """Clean up all collections after the each test."""
    for location in qdrant_locations():
        client = QdrantClient(location=location, api_key=os.getenv("QDRANT_API_KEY"))
        collections = client.get_collections().collections
        for collection in collections:
            client.delete_collection(collection.name)
"""Pytest conftest."""
from langchain_core._api.deprecation import deprecated
from vcr.persisters.filesystem import CassetteNotFoundError
from vcr.request import Request
class CustomSerializer:
    """Custom serializer for VCR cassettes using YAML and gzip.
    We're using a custom serializer to avoid the default yaml serializer
    used by VCR, which is not designed to be safe for untrusted input.
    This step is an extra precaution necessary because the cassette files
    are in compressed YAML format, which makes it more difficult to inspect
    their contents during development or debugging.
    def serialize(cassette_dict: dict[str, Any]) -> bytes:
        """Convert cassette to YAML and compress it."""
        cassette_dict["requests"] = [
                "uri": request.uri,
                "body": request.body,
                "headers": {k: [v] for k, v in request.headers.items()},
            for request in cassette_dict["requests"]
        yml = yaml.safe_dump(cassette_dict)
        return gzip.compress(yml.encode("utf-8"))
    def deserialize(data: bytes) -> dict[str, Any]:
        """Decompress data and convert it from YAML."""
        decoded_yaml = gzip.decompress(data).decode("utf-8")
        cassette = cast("dict[str, Any]", yaml.safe_load(decoded_yaml))
        cassette["requests"] = [Request(**request) for request in cassette["requests"]]
        return cassette
class CustomPersister:
    """A custom persister for VCR that uses the `CustomSerializer`."""
    def load_cassette(
        cassette_path: str | PathLike[str],
        serializer: CustomSerializer,
    ) -> tuple[list[Any], list[Any]]:
        """Load a cassette from a file."""
        # If cassette path is already Path this is a no-op
        cassette_path = Path(cassette_path)
        if not cassette_path.is_file():
            msg = f"Cassette file {cassette_path} does not exist."
            raise CassetteNotFoundError(msg)
        with cassette_path.open(mode="rb") as f:
        deser = serializer.deserialize(data)
        return deser["requests"], deser["responses"]
    def save_cassette(
        cassette_dict: dict[str, Any],
        """Save a cassette to a file."""
        data = serializer.serialize(cassette_dict)
        # if cassette path is already Path this is no operation
        cassette_folder = cassette_path.parent
        if not cassette_folder.exists():
            cassette_folder.mkdir(parents=True)
        with cassette_path.open("wb") as f:
# A list of headers that should be filtered out of the cassettes.
# These are typically associated with sensitive information and should
# not be stored in cassettes.
_BASE_FILTER_HEADERS = [
    ("authorization", "PLACEHOLDER"),
    ("x-api-key", "PLACEHOLDER"),
    ("api-key", "PLACEHOLDER"),
def base_vcr_config() -> dict[str, Any]:
    """Return VCR configuration that every cassette will receive.
    (Anything permitted by `vcr.VCR(**kwargs)` can be put here.)
        "record_mode": "once",
        "filter_headers": _BASE_FILTER_HEADERS.copy(),
        "match_on": ["method", "uri", "body"],
        "allow_playback_repeats": True,
        "decode_compressed_response": True,
        "cassette_library_dir": "tests/cassettes",
        "path_transformer": VCR.ensure_suffix(".yaml"),
@deprecated("1.0.3", alternative="base_vcr_config", removal="2.0")
def _base_vcr_config() -> dict[str, Any]:
    return base_vcr_config()
    """VCR config fixture."""
                    except (ImportError, ValueError):
Pytest configuration and fixtures for the Numpy test suite.
import hypothesis
from numpy.core._multiarray_tests import get_fpu_mode
_old_fpu_mode = None
_collect_results = {}
# Use a known and persistent tmpdir for hypothesis' caches, which
# can be automatically cleared by the OS or user.
hypothesis.configuration.set_hypothesis_home_dir(
    os.path.join(tempfile.gettempdir(), ".hypothesis")
# We register two custom profiles for Numpy - for details see
# https://hypothesis.readthedocs.io/en/latest/settings.html
# The first is designed for our own CI runs; the latter also 
# forces determinism and is designed for use via np.test()
hypothesis.settings.register_profile(
    name="numpy-profile", deadline=None, print_blob=True,
    name="np.test() profile",
    deadline=None, print_blob=True, database=None, derandomize=True,
    suppress_health_check=list(hypothesis.HealthCheck),
# Note that the default profile is chosen based on the presence 
# of pytest.ini, but can be overridden by passing the 
# --hypothesis-profile=NAME argument to pytest.
_pytest_ini = os.path.join(os.path.dirname(__file__), "..", "pytest.ini")
hypothesis.settings.load_profile(
    "numpy-profile" if os.path.isfile(_pytest_ini) else "np.test() profile"
# The experimentalAPI is used in _umath_tests
os.environ["NUMPY_EXPERIMENTAL_DTYPE_API"] = "1"
    config.addinivalue_line("markers",
        "valgrind_error: Tests that are known to error under valgrind.")
        "leaks_references: Tests that are known to leak references.")
        "slow: Tests that are very slow.")
        "slow_pypy: Tests that are very slow on pypy.")
def pytest_addoption(parser):
    parser.addoption("--available-memory", action="store", default=None,
                     help=("Set amount of memory available for running the "
                           "test suite. This can result to tests requiring "
                           "especially large amounts of memory to be skipped. "
                           "Equivalent to setting environment variable "
                           "NPY_AVAILABLE_MEM. Default: determined"
                           "automatically."))
def pytest_sessionstart(session):
    available_mem = session.config.getoption('available_memory')
    if available_mem is not None:
        os.environ['NPY_AVAILABLE_MEM'] = available_mem
#FIXME when yield tests are gone.
@pytest.hookimpl()
def pytest_itemcollected(item):
    Check FPU precision mode was not changed during test collection.
    The clumsy way we do it here is mainly necessary because numpy
    still uses yield tests, which can execute code at test collection
    global _old_fpu_mode
    mode = get_fpu_mode()
    if _old_fpu_mode is None:
        _old_fpu_mode = mode
    elif mode != _old_fpu_mode:
        _collect_results[item] = (_old_fpu_mode, mode)
@pytest.fixture(scope="function", autouse=True)
def check_fpu_mode(request):
    Check FPU precision mode was not changed during the test.
    old_mode = get_fpu_mode()
    new_mode = get_fpu_mode()
    if old_mode != new_mode:
        raise AssertionError("FPU precision mode changed from {0:#x} to {1:#x}"
                             " during the test".format(old_mode, new_mode))
    collect_result = _collect_results.get(request.node)
    if collect_result is not None:
        old_mode, new_mode = collect_result
                             " when collecting the test".format(old_mode,
                                                                new_mode))
def add_np(doctest_namespace):
    doctest_namespace['np'] = numpy
def env_setup(monkeypatch):
    monkeypatch.setenv('PYTHONHASHSEED', '0')
@pytest.fixture(params=[True, False])
def weak_promotion(request):
    Fixture to ensure "legacy" promotion state or change it to use the new
    weak promotion (plus warning).  `old_promotion` should be used as a
    parameter in the function.
    state = numpy._get_promotion_state()
    if request.param:
        numpy._set_promotion_state("weak_and_warn")
        numpy._set_promotion_state("legacy")
    yield request.param
    numpy._set_promotion_state(state)
Testing
General guidelines for writing good tests:
- doctests always assume ``import networkx as nx`` so don't add that
- prefer pytest fixtures over classes with setup methods.
- use the ``@pytest.mark.parametrize``  decorator
- use ``pytest.importorskip`` for numpy, scipy, pandas, and matplotlib b/c of PyPy.
  and add the module to the relevant entries below.
from importlib.metadata import entry_points
import networkx
        "--runslow", action="store_true", default=False, help="run slow tests"
        "--backend",
        help="Run tests with a backend by auto-converting nx graphs to backend graphs",
        "--fallback-to-nx",
        help="Run nx function if a backend doesn't implement a dispatchable function"
        " (use with --backend)",
    config.addinivalue_line("markers", "slow: mark test as slow to run")
    backend = config.getoption("--backend")
        backend = os.environ.get("NETWORKX_TEST_BACKEND")
    # nx_loopback backend is only available when testing with a backend
    loopback_ep = entry_points(name="nx_loopback", group="networkx.backends")
    if not loopback_ep:
            "\n\n             WARNING: Mixed NetworkX configuration! \n\n"
            "        This environment has mixed configuration for networkx.\n"
            "        The test object nx_loopback is not configured correctly.\n"
            "        You should not be seeing this message.\n"
            "        Try `pip install -e .`, or change your PYTHONPATH\n"
            "        Make sure python finds the networkx repo you are testing\n\n"
    config.backend = backend
    if backend:
        # We will update `networkx.config.backend_priority` below in `*_modify_items`
        # to allow tests to get set up with normal networkx graphs.
        networkx.utils.backends.backends["nx_loopback"] = loopback_ep["nx_loopback"]
        networkx.utils.backends.backend_info["nx_loopback"] = {}
        networkx.config.backends = networkx.utils.Config(
            nx_loopback=networkx.utils.Config(),
            **networkx.config.backends,
        fallback_to_nx = config.getoption("--fallback-to-nx")
        if not fallback_to_nx:
            fallback_to_nx = os.environ.get("NETWORKX_FALLBACK_TO_NX")
        networkx.config.fallback_to_nx = bool(fallback_to_nx)
def pytest_collection_modifyitems(config, items):
    # Setting this to True here allows tests to be set up before dispatching
    # any function call to a backend.
    if config.backend:
        # Allow pluggable backends to add markers to tests (such as skip or xfail)
        # when running in auto-conversion test mode
        backend_name = config.backend
        if backend_name != "networkx":
            networkx.utils.backends._dispatchable._is_testing = True
            networkx.config.backend_priority.algos = [backend_name]
            networkx.config.backend_priority.generators = [backend_name]
            backend = networkx.utils.backends.backends[backend_name].load()
            if hasattr(backend, "on_start_tests"):
                getattr(backend, "on_start_tests")(items)
    if config.getoption("--runslow"):
        # --runslow given in cli: do not skip slow tests
    skip_slow = pytest.mark.skip(reason="need --runslow option to run")
        if "slow" in item.keywords:
            item.add_marker(skip_slow)
# TODO: The warnings below need to be dealt with, but for now we silence them.
def set_warnings():
        "ignore",
        category=FutureWarning,
        message="\n\nsingle_target_shortest_path_length",
        message="\n\nshortest_path",
        "ignore", category=DeprecationWarning, message="\n\nThe `normalized`"
        "ignore", category=DeprecationWarning, message="\n\nall_triplets"
        "ignore", category=DeprecationWarning, message="\n\nrandom_triad"
        "ignore", category=DeprecationWarning, message="minimal_d_separator"
        "ignore", category=DeprecationWarning, message="d_separated"
    warnings.filterwarnings("ignore", category=DeprecationWarning, message="\n\nk_core")
        "ignore", category=DeprecationWarning, message="\n\nk_shell"
        "ignore", category=DeprecationWarning, message="\n\nk_crust"
        "ignore", category=DeprecationWarning, message="\n\nk_corona"
        "ignore", category=DeprecationWarning, message="\n\ntotal_spanning_tree_weight"
        "ignore", category=DeprecationWarning, message=r"\n\nThe 'create=matrix'"
        "ignore", category=DeprecationWarning, message="\n\n`compute_v_structures"
        "ignore", category=DeprecationWarning, message="Keyword argument 'link'"
def add_nx(doctest_namespace):
    doctest_namespace["nx"] = networkx
# What dependencies are installed?
    has_numpy = True
    has_numpy = False
    import scipy
    has_scipy = True
    has_scipy = False
    import matplotlib
    has_matplotlib = True
    has_matplotlib = False
    has_pandas = True
    has_pandas = False
    import pygraphviz
    has_pygraphviz = True
    has_pygraphviz = False
    import pydot
    has_pydot = True
    has_pydot = False
    import sympy
    has_sympy = True
    has_sympy = False
# List of files that pytest should ignore
collect_ignore = []
needs_numpy = [
    "algorithms/approximation/traveling_salesman.py",
    "algorithms/centrality/current_flow_closeness.py",
    "algorithms/centrality/laplacian.py",
    "algorithms/node_classification.py",
    "algorithms/non_randomness.py",
    "algorithms/polynomials.py",
    "algorithms/shortest_paths/dense.py",
    "algorithms/tree/mst.py",
    "drawing/nx_latex.py",
    "generators/expanders.py",
    "linalg/bethehessianmatrix.py",
    "linalg/laplacianmatrix.py",
    "utils/misc.py",
needs_scipy = [
    "algorithms/assortativity/correlation.py",
    "algorithms/assortativity/mixing.py",
    "algorithms/assortativity/pairs.py",
    "algorithms/bipartite/matrix.py",
    "algorithms/bipartite/spectral.py",
    "algorithms/centrality/current_flow_betweenness.py",
    "algorithms/centrality/current_flow_betweenness_subset.py",
    "algorithms/centrality/eigenvector.py",
    "algorithms/centrality/katz.py",
    "algorithms/centrality/second_order.py",
    "algorithms/centrality/subgraph_alg.py",
    "algorithms/communicability_alg.py",
    "algorithms/community/divisive.py",
    "algorithms/distance_measures.py",
    "algorithms/link_analysis/hits_alg.py",
    "algorithms/link_analysis/pagerank_alg.py",
    "algorithms/similarity.py",
    "algorithms/walks.py",
    "convert_matrix.py",
    "drawing/layout.py",
    "drawing/nx_pylab.py",
    "generators/spectral_graph_forge.py",
    "linalg/algebraicconnectivity.py",
    "linalg/attrmatrix.py",
    "linalg/graphmatrix.py",
    "linalg/modularitymatrix.py",
    "linalg/spectrum.py",
    "utils/rcm.py",
needs_matplotlib = ["drawing/nx_pylab.py", "generators/classic.py"]
needs_pandas = ["convert_matrix.py"]
needs_pygraphviz = ["drawing/nx_agraph.py"]
needs_pydot = ["drawing/nx_pydot.py"]
needs_sympy = ["algorithms/polynomials.py"]
if not has_numpy:
    collect_ignore += needs_numpy
if not has_scipy:
    collect_ignore += needs_scipy
if not has_matplotlib:
    collect_ignore += needs_matplotlib
if not has_pandas:
    collect_ignore += needs_pandas
if not has_pygraphviz:
    collect_ignore += needs_pygraphviz
if not has_pydot:
    collect_ignore += needs_pydot
if not has_sympy:
    collect_ignore += needs_sympy
def m():
    Fixture providing a memory filesystem.
    m = fsspec.filesystem("memory")
    m.store.clear()
    m.pseudo_dirs.clear()
    m.pseudo_dirs.append("")
        yield m
class InstanceCacheInspector:
    Helper class to inspect instance caches of filesystem classes in tests.
    def clear(self) -> None:
        Clear instance caches of all currently imported filesystem classes.
        classes = deque([fsspec.spec.AbstractFileSystem])
        while classes:
            cls = classes.popleft()
            cls.clear_instance_cache()
            classes.extend(cls.__subclasses__())
    def gather_counts(self, *, omit_zero: bool = True) -> dict[str, int]:
        Gather counts of filesystem instances in the instance caches
        of all currently imported filesystem classes.
        omit_zero:
            Whether to omit instance types with no cached instances.
        out: dict[str, int] = {}
            count = len(cls._cache)  # there is no public interface for the cache
            # note: skip intermediate AbstractFileSystem subclasses
            #   if they proxy the protocol attribute via a property.
            if isinstance(cls.protocol, (Sequence, str)):
                key = cls.protocol if isinstance(cls.protocol, str) else cls.protocol[0]
                if count or not omit_zero:
                    out[key] = count
def instance_caches() -> Generator[InstanceCacheInspector, None, None]:
    Fixture to ensure empty filesystem instance caches before and after a test.
    Used by default for all tests.
    Clears caches of all imported filesystem classes.
    Can be used to write test assertions about instance caches.
        def test_something(instance_caches):
            # Test code here
            fsspec.open("file://abc")
            fsspec.open("memory://foo/bar")
            # Test assertion
            assert instance_caches.gather_counts() == {"file": 1, "memory": 1}
    instance_caches: An instance cache inspector for clearing and inspecting caches.
    ic = InstanceCacheInspector()
    ic.clear()
        yield ic
@pytest.fixture(scope="function")
def ftp_writable(tmpdir):
    Fixture providing a writable FTP filesystem.
    pytest.importorskip("pyftpdlib")
    d = str(tmpdir)
    with open(os.path.join(d, "out"), "wb") as f:
        f.write(b"hello" * 10000)
    P = subprocess.Popen(
        [sys.executable, "-m", "pyftpdlib", "-d", d, "-u", "user", "-P", "pass", "-w"]
        yield "localhost", 2121, "user", "pass"
        P.terminate()
        P.wait()
# Pytest customization
import numpy.testing as npt
from scipy._lib._fpumode import get_fpu_mode
from scipy._lib._testutils import FPUModeChangeWarning
from scipy._lib._array_api import SCIPY_ARRAY_API, SCIPY_DEVICE
from scipy._lib import _pep440
    from scipy_doctest.conftest import dt_config
    HAVE_SCPDT = True
    HAVE_SCPDT = False
    import pytest_run_parallel  # noqa:F401
    PARALLEL_RUN_AVAILABLE = True
    PARALLEL_RUN_AVAILABLE = False
        "xslow: mark test as extremely slow (not run unless explicitly requested)")
        "xfail_on_32bit: mark test as failing on 32-bit platforms")
        import pytest_timeout  # noqa:F401
            "markers", 'timeout: mark a test for a non-default timeout')
        # This is a more reliable test of whether pytest_fail_slow is installed
        # When I uninstalled it, `import pytest_fail_slow` didn't fail!
        from pytest_fail_slow import parse_duration  # type: ignore[import-not-found] # noqa:F401,E501
            "markers", 'fail_slow: mark a test for a non-default timeout failure')
        "skip_xp_backends(backends, reason=None, np_only=False, cpu_only=False, "
        "exceptions=None): "
        "mark the desired skip configuration for the `skip_xp_backends` fixture.")
        "xfail_xp_backends(backends, reason=None, np_only=False, cpu_only=False, "
        "mark the desired xfail configuration for the `xfail_xp_backends` fixture.")
    if not PARALLEL_RUN_AVAILABLE:
            'markers',
            'parallel_threads(n): run the given test function in parallel '
            'using `n` threads.')
            "markers",
            "thread_unsafe: mark the test function as single-threaded",
            "iterations(n): run the given test function `n` times in each thread",
    mark = item.get_closest_marker("xslow")
    if mark is not None:
            v = int(os.environ.get('SCIPY_XSLOW', '0'))
            v = False
            pytest.skip("very slow test; "
                        "set environment variable SCIPY_XSLOW=1 to run it")
    mark = item.get_closest_marker("xfail_on_32bit")
    if mark is not None and np.intp(0).itemsize < 8:
        pytest.xfail(f'Fails on our 32-bit test platform(s): {mark.args[0]}')
    # Older versions of threadpoolctl have an issue that may lead to this
    # warning being emitted, see gh-14441
    with npt.suppress_warnings() as sup:
        sup.filter(pytest.PytestUnraisableExceptionWarning)
            from threadpoolctl import threadpool_limits
            HAS_THREADPOOLCTL = True
        except Exception:  # observed in gh-14441: (ImportError, AttributeError)
            # Optional dependency only. All exceptions are caught, for robustness
            HAS_THREADPOOLCTL = False
        if HAS_THREADPOOLCTL:
            # Set the number of openmp threads based on the number of workers
            # xdist is using to prevent oversubscription. Simplified version of what
            # sklearn does (it can rely on threadpoolctl and its builtin OpenMP helper
            # functions)
                xdist_worker_count = int(os.environ['PYTEST_XDIST_WORKER_COUNT'])
                # raises when pytest-xdist is not installed
            if not os.getenv('OMP_NUM_THREADS'):
                max_openmp_threads = os.cpu_count() // 2  # use nr of physical cores
                threads_per_worker = max(max_openmp_threads // xdist_worker_count, 1)
                    threadpool_limits(threads_per_worker, user_api='blas')
                    # May raise AttributeError for older versions of OpenBLAS.
                    # Catch any error for robustness.
    Check FPU mode was not changed during the test.
        warnings.warn(f"FPU mode changed from {old_mode:#x} to {new_mode:#x} during "
                      "the test",
                      category=FPUModeChangeWarning, stacklevel=0)
    def num_parallel_threads():
# Array API backend handling
xp_available_backends = {'numpy': np}
if SCIPY_ARRAY_API and isinstance(SCIPY_ARRAY_API, str):
    # fill the dict of backends with available libraries
        import array_api_strict
        xp_available_backends.update({'array_api_strict': array_api_strict})
        if _pep440.parse(array_api_strict.__version__) < _pep440.Version('2.0'):
            raise ImportError("array-api-strict must be >= version 2.0")
        array_api_strict.set_array_api_strict_flags(
            api_version='2023.12'
        import torch  # type: ignore[import-not-found]
        xp_available_backends.update({'torch': torch})
        # can use `mps` or `cpu`
        torch.set_default_device(SCIPY_DEVICE)
        import cupy  # type: ignore[import-not-found]
        xp_available_backends.update({'cupy': cupy})
        import jax.numpy  # type: ignore[import-not-found]
        xp_available_backends.update({'jax.numpy': jax.numpy})
        jax.config.update("jax_enable_x64", True)
        jax.config.update("jax_default_device", jax.devices(SCIPY_DEVICE)[0])
    # by default, use all available backends
    if SCIPY_ARRAY_API.lower() not in ("1", "true"):
        SCIPY_ARRAY_API_ = json.loads(SCIPY_ARRAY_API)
        if 'all' in SCIPY_ARRAY_API_:
            pass  # same as True
            # only select a subset of backend by filtering out the dict
                xp_available_backends = {
                    backend: xp_available_backends[backend]
                    for backend in SCIPY_ARRAY_API_
                msg = f"'--array-api-backend' must be in {xp_available_backends.keys()}"
if 'cupy' in xp_available_backends:
    SCIPY_DEVICE = 'cuda'
array_api_compatible = pytest.mark.parametrize("xp", xp_available_backends.values())
skip_xp_invalid_arg = pytest.mark.skipif(SCIPY_ARRAY_API,
    reason = ('Test involves masked arrays, object arrays, or other types '
              'that are not valid input when `SCIPY_ARRAY_API` is used.'))
def _backends_kwargs_from_request(request, skip_or_xfail):
    """A helper for {skip,xfail}_xp_backends"""
    # do not allow multiple backends
    args_ = request.keywords[f'{skip_or_xfail}_xp_backends'].args
    if len(args_) > 1:
        # np_only / cpu_only has args=(), otherwise it's ('numpy',)
        # and we do not allow ('numpy', 'cupy')
        raise ValueError(f"multiple backends: {args_}")
    markers = list(request.node.iter_markers(f'{skip_or_xfail}_xp_backends'))
    for marker in markers:
        if marker.kwargs.get('np_only'):
            kwargs['np_only'] = True
            kwargs['exceptions'] = marker.kwargs.get('exceptions', [])
        elif marker.kwargs.get('cpu_only'):
            if not kwargs.get('np_only'):
                # if np_only is given, it is certainly cpu only
                kwargs['cpu_only'] = True
        # add backends, if any
        if len(marker.args) > 0:
            backend = marker.args[0]  # was a tuple, ('numpy',) etc
            backends.append(backend)
            kwargs.update(**{backend: marker.kwargs})
    return backends, kwargs
def skip_xp_backends(xp, request):
    """skip_xp_backends(backend=None, reason=None, np_only=False, cpu_only=False, exceptions=None)
    Skip a decorated test for the provided backend, or skip a category of backends.
    See ``skip_or_xfail_backends`` docstring for details. Note that, contrary to
    ``skip_or_xfail_backends``, the ``backend`` and ``reason`` arguments are optional
    single strings: this function only skips a single backend at a time.
    To skip multiple backends, provide multiple decorators.
    if "skip_xp_backends" not in request.keywords:
    backends, kwargs = _backends_kwargs_from_request(request, skip_or_xfail='skip')
    skip_or_xfail_xp_backends(xp, backends, kwargs, skip_or_xfail='skip')
def xfail_xp_backends(xp, request):
    """xfail_xp_backends(backend=None, reason=None, np_only=False, cpu_only=False, exceptions=None)
    xfail a decorated test for the provided backend, or xfail a category of backends.
    single strings: this function only xfails a single backend at a time.
    To xfail multiple backends, provide multiple decorators.
    if "xfail_xp_backends" not in request.keywords:
    backends, kwargs = _backends_kwargs_from_request(request, skip_or_xfail='xfail')
    skip_or_xfail_xp_backends(xp, backends, kwargs, skip_or_xfail='xfail')
def skip_or_xfail_xp_backends(xp, backends, kwargs, skip_or_xfail='skip'):
    Skip based on the ``skip_xp_backends`` or ``xfail_xp_backends`` marker.
    See the "Support for the array API standard" docs page for usage examples.
    backends : tuple
        Backends to skip/xfail, e.g. ``("array_api_strict", "torch")``.
        These are overriden when ``np_only`` is ``True``, and are not
        necessary to provide for non-CPU backends when ``cpu_only`` is ``True``.
        For a custom reason to apply, you should pass a dict ``{'reason': '...'}``
        to a keyword matching the name of the backend.
    reason : str, optional
        A reason for the skip/xfail in the case of ``np_only=True``.
        If unprovided, a default reason is used. Note that it is not possible
        to specify a custom reason with ``cpu_only``.
    np_only : bool, optional
        When ``True``, the test is skipped/xfailed for all backends other
        than the default NumPy backend. There is no need to provide
        any ``backends`` in this case. To specify a reason, pass a
        value to ``reason``. Default: ``False``.
    cpu_only : bool, optional
        When ``True``, the test is skipped/xfailed on non-CPU devices.
        There is no need to provide any ``backends`` in this case,
        but any ``backends`` will also be skipped on the CPU.
        Default: ``False``.
    exceptions : list, optional
        A list of exceptions for use with ``cpu_only`` or ``np_only``.
        This should be provided when delegation is implemented for some,
        but not all, non-CPU/non-NumPy backends.
    skip_or_xfail : str
        ``'skip'`` to skip, ``'xfail'`` to xfail.
    skip_or_xfail = getattr(pytest, skip_or_xfail)
    np_only = kwargs.get("np_only", False)
    cpu_only = kwargs.get("cpu_only", False)
    exceptions = kwargs.get("exceptions", [])
    if reasons := kwargs.get("reasons"):
        raise ValueError(f"provide a single `reason=` kwarg; got {reasons=} instead")
    # input validation
    if np_only and cpu_only:
        # np_only is a stricter subset of cpu_only
        cpu_only = False
    if exceptions and not (cpu_only or np_only):
        raise ValueError("`exceptions` is only valid alongside `cpu_only` or `np_only`")
    if np_only:
        reason = kwargs.get("reason", "do not run with non-NumPy backends.")
        if not isinstance(reason, str) and len(reason) > 1:
            raise ValueError("please provide a singleton `reason` "
                             "when using `np_only`")
        if xp.__name__ != 'numpy' and xp.__name__ not in exceptions:
            skip_or_xfail(reason=reason)
    if cpu_only:
        reason = ("no array-agnostic implementation or delegation available "
                  "for this backend and device")
        exceptions = [] if exceptions is None else exceptions
        if SCIPY_ARRAY_API and SCIPY_DEVICE != 'cpu':
            if xp.__name__ == 'cupy' and 'cupy' not in exceptions:
            elif xp.__name__ == 'torch' and 'torch' not in exceptions:
                if 'cpu' not in xp.empty(0).device.type:
            elif xp.__name__ == 'jax.numpy' and 'jax.numpy' not in exceptions:
                for d in xp.empty(0).devices():
                    if 'cpu' not in d.device_kind:
    if backends is not None:
        for i, backend in enumerate(backends):
            if xp.__name__ == backend:
                reason = kwargs[backend].get('reason')
                if not reason:
                    reason = f"do not run with array API backend: {backend}"
# Following the approach of NumPy's conftest.py...
# We register two custom profiles for SciPy - for details see
# forces determinism and is designed for use via scipy.test()
    name="nondeterministic", deadline=None, print_blob=True,
    name="deterministic",
# Profile is currently set by environment variable `SCIPY_HYPOTHESIS_PROFILE`
# In the future, it would be good to work the choice into dev.py.
SCIPY_HYPOTHESIS_PROFILE = os.environ.get("SCIPY_HYPOTHESIS_PROFILE",
                                          "deterministic")
hypothesis.settings.load_profile(SCIPY_HYPOTHESIS_PROFILE)
############################################################################
# doctesting stuff
if HAVE_SCPDT:
    # FIXME: populate the dict once
    def warnings_errors_and_rng(test=None):
        """Temporarily turn (almost) all warnings to errors.
        Filter out known warnings which we allow.
        known_warnings = dict()
        # these functions are known to emit "divide by zero" RuntimeWarnings
        divide_by_zero = [
            'scipy.linalg.norm', 'scipy.ndimage.center_of_mass',
        for name in divide_by_zero:
            known_warnings[name] = dict(category=RuntimeWarning,
                                        message='divide by zero')
        # Deprecated stuff in scipy.signal and elsewhere
        deprecated = [
            'scipy.signal.cwt', 'scipy.signal.morlet', 'scipy.signal.morlet2',
            'scipy.signal.ricker',
            'scipy.integrate.simpson',
            'scipy.interpolate.interp2d',
            'scipy.linalg.kron',
        for name in deprecated:
            known_warnings[name] = dict(category=DeprecationWarning)
        from scipy import integrate
        # the functions are known to emit IntegrationWarnings
        integration_w = ['scipy.special.ellip_normal',
                         'scipy.special.ellip_harm_2',
        for name in integration_w:
            known_warnings[name] = dict(category=integrate.IntegrationWarning,
                                        message='The occurrence of roundoff')
        # scipy.stats deliberately emits UserWarnings sometimes
        user_w = ['scipy.stats.anderson_ksamp', 'scipy.stats.kurtosistest',
                  'scipy.stats.normaltest', 'scipy.sparse.linalg.norm']
        for name in user_w:
            known_warnings[name] = dict(category=UserWarning)
        # additional one-off warnings to filter
        dct = {
            'scipy.sparse.linalg.norm':
                dict(category=UserWarning, message="Exited at iteration"),
            # tutorials
            'linalg.rst':
                dict(message='the matrix subclass is not',
                     category=PendingDeprecationWarning),
            'stats.rst':
                dict(message='The maximum number of subdivisions',
                     category=integrate.IntegrationWarning),
        known_warnings.update(dct)
        # these legitimately emit warnings in examples
        legit = set('scipy.signal.normalize')
        # Now, the meat of the matter: filter warnings,
        # also control the random seed for each doctest.
        # XXX: this matches the refguide-check behavior, but is a tad strange:
        # makes sure that the seed the old-fashioned np.random* methods is
        # *NOT* reproducible but the new-style `default_rng()` *IS* repoducible.
        # Should these two be either both repro or both not repro?
        from scipy._lib._util import _fixed_default_rng
        with _fixed_default_rng():
            np.random.seed(None)
                if test and test.name in known_warnings:
                    warnings.filterwarnings('ignore',
                                            **known_warnings[test.name])
                elif test and test.name in legit:
                    warnings.simplefilter('error', Warning)
    dt_config.user_context_mgr = warnings_errors_and_rng
    dt_config.skiplist = set([
        'scipy.linalg.LinAlgError',     # comes from numpy
        'scipy.fftpack.fftshift',       # fftpack stuff is also from numpy
        'scipy.fftpack.ifftshift',
        'scipy.fftpack.fftfreq',
        'scipy.special.sinc',           # sinc is from numpy
        'scipy.optimize.show_options',  # does not have much to doctest
        'scipy.signal.normalize',       # manipulates warnings (XXX temp skip)
        'scipy.sparse.linalg.norm',     # XXX temp skip
        # these below test things which inherit from np.ndarray
        # cross-ref https://github.com/numpy/numpy/issues/28019
        'scipy.io.matlab.MatlabObject.strides',
        'scipy.io.matlab.MatlabObject.dtype',
        'scipy.io.matlab.MatlabOpaque.dtype',
        'scipy.io.matlab.MatlabOpaque.strides',
        'scipy.io.matlab.MatlabFunction.strides',
        'scipy.io.matlab.MatlabFunction.dtype'
    # these are affected by NumPy 2.0 scalar repr: rely on string comparison
    if np.__version__ < "2":
        dt_config.skiplist.update(set([
            'scipy.io.hb_read',
            'scipy.io.hb_write',
            'scipy.sparse.csgraph.connected_components',
            'scipy.sparse.csgraph.depth_first_order',
            'scipy.sparse.csgraph.shortest_path',
            'scipy.sparse.csgraph.floyd_warshall',
            'scipy.sparse.csgraph.dijkstra',
            'scipy.sparse.csgraph.bellman_ford',
            'scipy.sparse.csgraph.johnson',
            'scipy.sparse.csgraph.yen',
            'scipy.sparse.csgraph.breadth_first_order',
            'scipy.sparse.csgraph.reverse_cuthill_mckee',
            'scipy.sparse.csgraph.structural_rank',
            'scipy.sparse.csgraph.construct_dist_matrix',
            'scipy.sparse.csgraph.reconstruct_path',
            'scipy.ndimage.value_indices',
            'scipy.stats.mstats.describe',
    # help pytest collection a bit: these names are either private
    # (distributions), or just do not need doctesting.
    dt_config.pytest_extra_ignore = [
        "scipy.stats.distributions",
        "scipy.optimize.cython_optimize",
        "scipy.test",
        "scipy.show_config",
        # equivalent to "pytest --ignore=path/to/file"
        "scipy/special/_precompute",
        "scipy/interpolate/_interpnd_info.py",
        "scipy/_lib/array_api_compat",
        "scipy/_lib/highs",
        "scipy/_lib/unuran",
        "scipy/_lib/_gcutils.py",
        "scipy/_lib/doccer.py",
        "scipy/_lib/_uarray",
    dt_config.pytest_extra_xfail = {
        # name: reason
        "ND_regular_grid.rst": "ReST parser limitation",
        "extrapolation_examples.rst": "ReST parser limitation",
        "sampling_pinv.rst": "__cinit__ unexpected argument",
        "sampling_srou.rst": "nan in scalar_power",
        "probability_distributions.rst": "integration warning",
    dt_config.pseudocode = set(['integrate.nquad(func,'])
    dt_config.local_resources = {
        'io.rst': [
            "octave_a.mat",
            "octave_cells.mat",
            "octave_struct.mat"
    dt_config.strict_check = True
