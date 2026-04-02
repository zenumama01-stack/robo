from dirty_equals import IsDict, IsList, IsBytes, IsTuple
from openai._files import to_httpx_files, async_to_httpx_files
readme_path = Path(__file__).parent.parent.joinpath("README.md")
def test_pathlib_includes_file_name() -> None:
    result = to_httpx_files({"file": readme_path})
    assert result == IsDict({"file": IsTuple("README.md", IsBytes())})
def test_tuple_input() -> None:
    result = to_httpx_files([("file", readme_path)])
    assert result == IsList(IsTuple("file", IsTuple("README.md", IsBytes())))
async def test_async_pathlib_includes_file_name() -> None:
    result = await async_to_httpx_files({"file": readme_path})
async def test_async_supports_anyio_path() -> None:
    result = await async_to_httpx_files({"file": anyio.Path(readme_path)})
async def test_async_tuple_input() -> None:
    result = await async_to_httpx_files([("file", readme_path)])
def test_string_not_allowed() -> None:
    with pytest.raises(TypeError, match="Expected file types input to be a FileContent type or to be a tuple"):
        to_httpx_files(
                "file": "foo",  # type: ignore
import openai._legacy_response as _legacy_response
from openai.types import FileObject, FileDeleted
# pyright: reportDeprecated=false
class TestFiles:
        file = client.files.create(
            file=b"Example data",
        assert_matches_type(FileObject, file, path=["response"])
        response = client.files.with_raw_response.create(
        with client.files.with_streaming_response.create(
        file = client.files.retrieve(
        response = client.files.with_raw_response.retrieve(
        with client.files.with_streaming_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `file_id` but received ''"):
            client.files.with_raw_response.retrieve(
        file = client.files.list()
        assert_matches_type(SyncCursorPage[FileObject], file, path=["response"])
        file = client.files.list(
            purpose="purpose",
        response = client.files.with_raw_response.list()
        with client.files.with_streaming_response.list() as response:
        file = client.files.delete(
        assert_matches_type(FileDeleted, file, path=["response"])
        response = client.files.with_raw_response.delete(
        with client.files.with_streaming_response.delete(
            client.files.with_raw_response.delete(
    def test_method_content(self, client: OpenAI, respx_mock: MockRouter) -> None:
        respx_mock.get("/files/string/content").mock(return_value=httpx.Response(200, json={"foo": "bar"}))
        file = client.files.content(
        assert isinstance(file, _legacy_response.HttpxBinaryResponseContent)
        assert file.json() == {"foo": "bar"}
    def test_raw_response_content(self, client: OpenAI, respx_mock: MockRouter) -> None:
        response = client.files.with_raw_response.content(
        assert_matches_type(_legacy_response.HttpxBinaryResponseContent, file, path=["response"])
    def test_streaming_response_content(self, client: OpenAI, respx_mock: MockRouter) -> None:
        with client.files.with_streaming_response.content(
            assert_matches_type(bytes, file, path=["response"])
    def test_path_params_content(self, client: OpenAI) -> None:
            client.files.with_raw_response.content(
    def test_method_retrieve_content(self, client: OpenAI) -> None:
        with pytest.warns(DeprecationWarning):
            file = client.files.retrieve_content(
        assert_matches_type(str, file, path=["response"])
    def test_raw_response_retrieve_content(self, client: OpenAI) -> None:
            response = client.files.with_raw_response.retrieve_content(
    def test_streaming_response_retrieve_content(self, client: OpenAI) -> None:
            with client.files.with_streaming_response.retrieve_content(
    def test_path_params_retrieve_content(self, client: OpenAI) -> None:
                client.files.with_raw_response.retrieve_content(
class TestAsyncFiles:
        file = await async_client.files.create(
        response = await async_client.files.with_raw_response.create(
        async with async_client.files.with_streaming_response.create(
            file = await response.parse()
        file = await async_client.files.retrieve(
        response = await async_client.files.with_raw_response.retrieve(
        async with async_client.files.with_streaming_response.retrieve(
            await async_client.files.with_raw_response.retrieve(
        file = await async_client.files.list()
        assert_matches_type(AsyncCursorPage[FileObject], file, path=["response"])
        file = await async_client.files.list(
        response = await async_client.files.with_raw_response.list()
        async with async_client.files.with_streaming_response.list() as response:
        file = await async_client.files.delete(
        response = await async_client.files.with_raw_response.delete(
        async with async_client.files.with_streaming_response.delete(
            await async_client.files.with_raw_response.delete(
    async def test_method_content(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        file = await async_client.files.content(
    async def test_raw_response_content(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        response = await async_client.files.with_raw_response.content(
    async def test_streaming_response_content(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        async with async_client.files.with_streaming_response.content(
    async def test_path_params_content(self, async_client: AsyncOpenAI) -> None:
            await async_client.files.with_raw_response.content(
    async def test_method_retrieve_content(self, async_client: AsyncOpenAI) -> None:
            file = await async_client.files.retrieve_content(
    async def test_raw_response_retrieve_content(self, async_client: AsyncOpenAI) -> None:
            response = await async_client.files.with_raw_response.retrieve_content(
    async def test_streaming_response_retrieve_content(self, async_client: AsyncOpenAI) -> None:
            async with async_client.files.with_streaming_response.retrieve_content(
    async def test_path_params_retrieve_content(self, async_client: AsyncOpenAI) -> None:
                await async_client.files.with_raw_response.retrieve_content(
from openai.types.containers import (
    FileListResponse,
    FileCreateResponse,
    FileRetrieveResponse,
        file = client.containers.files.create(
            container_id="container_id",
        assert_matches_type(FileCreateResponse, file, path=["response"])
            file_id="file_id",
        response = client.containers.files.with_raw_response.create(
        with client.containers.files.with_streaming_response.create(
            client.containers.files.with_raw_response.create(
                container_id="",
        file = client.containers.files.retrieve(
        assert_matches_type(FileRetrieveResponse, file, path=["response"])
        response = client.containers.files.with_raw_response.retrieve(
        with client.containers.files.with_streaming_response.retrieve(
            client.containers.files.with_raw_response.retrieve(
                file_id="",
        file = client.containers.files.list(
        assert_matches_type(SyncCursorPage[FileListResponse], file, path=["response"])
        response = client.containers.files.with_raw_response.list(
        with client.containers.files.with_streaming_response.list(
            client.containers.files.with_raw_response.list(
        file = client.containers.files.delete(
        assert file is None
        response = client.containers.files.with_raw_response.delete(
        with client.containers.files.with_streaming_response.delete(
            client.containers.files.with_raw_response.delete(
        file = await async_client.containers.files.create(
        response = await async_client.containers.files.with_raw_response.create(
        async with async_client.containers.files.with_streaming_response.create(
            await async_client.containers.files.with_raw_response.create(
        file = await async_client.containers.files.retrieve(
        response = await async_client.containers.files.with_raw_response.retrieve(
        async with async_client.containers.files.with_streaming_response.retrieve(
            await async_client.containers.files.with_raw_response.retrieve(
        file = await async_client.containers.files.list(
        assert_matches_type(AsyncCursorPage[FileListResponse], file, path=["response"])
        response = await async_client.containers.files.with_raw_response.list(
        async with async_client.containers.files.with_streaming_response.list(
            await async_client.containers.files.with_raw_response.list(
        file = await async_client.containers.files.delete(
        response = await async_client.containers.files.with_raw_response.delete(
        async with async_client.containers.files.with_streaming_response.delete(
            await async_client.containers.files.with_raw_response.delete(
    FileContentResponse,
    VectorStoreFileDeleted,
        file = client.vector_stores.files.create(
        assert_matches_type(VectorStoreFile, file, path=["response"])
        response = client.vector_stores.files.with_raw_response.create(
        with client.vector_stores.files.with_streaming_response.create(
            client.vector_stores.files.with_raw_response.create(
        file = client.vector_stores.files.retrieve(
            file_id="file-abc123",
        response = client.vector_stores.files.with_raw_response.retrieve(
        with client.vector_stores.files.with_streaming_response.retrieve(
            client.vector_stores.files.with_raw_response.retrieve(
        file = client.vector_stores.files.update(
        response = client.vector_stores.files.with_raw_response.update(
        with client.vector_stores.files.with_streaming_response.update(
            client.vector_stores.files.with_raw_response.update(
        file = client.vector_stores.files.list(
        assert_matches_type(SyncCursorPage[VectorStoreFile], file, path=["response"])
        response = client.vector_stores.files.with_raw_response.list(
        with client.vector_stores.files.with_streaming_response.list(
            client.vector_stores.files.with_raw_response.list(
        file = client.vector_stores.files.delete(
        assert_matches_type(VectorStoreFileDeleted, file, path=["response"])
        response = client.vector_stores.files.with_raw_response.delete(
        with client.vector_stores.files.with_streaming_response.delete(
            client.vector_stores.files.with_raw_response.delete(
    def test_method_content(self, client: OpenAI) -> None:
        file = client.vector_stores.files.content(
        assert_matches_type(SyncPage[FileContentResponse], file, path=["response"])
    def test_raw_response_content(self, client: OpenAI) -> None:
        response = client.vector_stores.files.with_raw_response.content(
    def test_streaming_response_content(self, client: OpenAI) -> None:
        with client.vector_stores.files.with_streaming_response.content(
            client.vector_stores.files.with_raw_response.content(
        file = await async_client.vector_stores.files.create(
        response = await async_client.vector_stores.files.with_raw_response.create(
        async with async_client.vector_stores.files.with_streaming_response.create(
            await async_client.vector_stores.files.with_raw_response.create(
        file = await async_client.vector_stores.files.retrieve(
        response = await async_client.vector_stores.files.with_raw_response.retrieve(
        async with async_client.vector_stores.files.with_streaming_response.retrieve(
            await async_client.vector_stores.files.with_raw_response.retrieve(
        file = await async_client.vector_stores.files.update(
        response = await async_client.vector_stores.files.with_raw_response.update(
        async with async_client.vector_stores.files.with_streaming_response.update(
            await async_client.vector_stores.files.with_raw_response.update(
        file = await async_client.vector_stores.files.list(
        assert_matches_type(AsyncCursorPage[VectorStoreFile], file, path=["response"])
        response = await async_client.vector_stores.files.with_raw_response.list(
        async with async_client.vector_stores.files.with_streaming_response.list(
            await async_client.vector_stores.files.with_raw_response.list(
        file = await async_client.vector_stores.files.delete(
        response = await async_client.vector_stores.files.with_raw_response.delete(
        async with async_client.vector_stores.files.with_streaming_response.delete(
            await async_client.vector_stores.files.with_raw_response.delete(
    async def test_method_content(self, async_client: AsyncOpenAI) -> None:
        file = await async_client.vector_stores.files.content(
        assert_matches_type(AsyncPage[FileContentResponse], file, path=["response"])
    async def test_raw_response_content(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.vector_stores.files.with_raw_response.content(
    async def test_streaming_response_content(self, async_client: AsyncOpenAI) -> None:
        async with async_client.vector_stores.files.with_streaming_response.content(
            await async_client.vector_stores.files.with_raw_response.content(
        checking_client.vector_stores.files.create,
        checking_client.vector_stores.files.create_and_poll,
def test_upload_and_poll_method_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
        checking_client.vector_stores.files.upload_and_poll,
        exclude_params={"file_id", "extra_headers", "extra_query", "extra_body", "timeout"},
from django.core.checks import Error
from django.core.checks.files import check_setting_file_upload_temp_dir
class FilesCheckTests(SimpleTestCase):
    def test_file_upload_temp_dir(self):
            Path.cwd(),
            str(Path.cwd()),
        for setting in tests:
            with self.subTest(setting), self.settings(FILE_UPLOAD_TEMP_DIR=setting):
                self.assertEqual(check_setting_file_upload_temp_dir(None), [])
    def test_file_upload_temp_dir_nonexistent(self):
        for setting in ["nonexistent", Path("nonexistent")]:
                    check_setting_file_upload_temp_dir(None),
                            "The FILE_UPLOAD_TEMP_DIR setting refers to the "
                            "nonexistent directory 'nonexistent'.",
import importlib_resources as resources
from ..abc import Traversable
from .compat.py39 import import_helper, os_helper
        warnings.simplefilter('default', category=DeprecationWarning)
class FilesTests:
    def test_read_bytes(self):
        files = resources.files(self.data)
        actual = files.joinpath('utf-8.file').read_bytes()
        assert actual == b'Hello, UTF-8 world!\n'
    def test_read_text(self):
        actual = files.joinpath('utf-8.file').read_text(encoding='utf-8')
        assert actual == 'Hello, UTF-8 world!\n'
    def test_traversable(self):
        assert isinstance(resources.files(self.data), Traversable)
    def test_joinpath_with_multiple_args(self):
        binfile = files.joinpath('subdirectory', 'binary.file')
        self.assertTrue(binfile.is_file())
    def test_old_parameter(self):
        Files used to take a 'package' parameter. Make sure anyone
        passing by name is still supported.
            resources.files(package=self.data)
class OpenDiskTests(FilesTests, util.DiskSetup, unittest.TestCase):
class OpenZipTests(FilesTests, util.ZipSetup, unittest.TestCase):
class OpenNamespaceTests(FilesTests, util.DiskSetup, unittest.TestCase):
    MODULE = 'namespacedata01'
    def test_non_paths_in_dunder_path(self):
        Non-path items in a namespace package's ``__path__`` are ignored.
        As reported in python/importlib_resources#311, some tools
        like Setuptools, when creating editable packages, will inject
        non-paths into a namespace package's ``__path__``, a
        sentinel like
        ``__editable__.sample_namespace-1.0.finder.__path_hook__``
        to cause the ``PathEntryFinder`` to be called when searching
        for packages. In that case, resources should still be loadable.
        import namespacedata01  # type: ignore[import-not-found]
        namespacedata01.__path__.append(
            '__editable__.sample_namespace-1.0.finder.__path_hook__'
        resources.files(namespacedata01)
class OpenNamespaceZipTests(FilesTests, util.ZipSetup, unittest.TestCase):
    ZIP_MODULE = 'namespacedata01'
class DirectSpec:
    Override behavior of ModuleSetup to write a full spec directly.
    MODULE = 'unused'
    def load_fixture(self, name):
        self.tree_on_path(self.spec)
class ModulesFiles:
    spec = {
        'mod.py': '',
        'res.txt': 'resources are the best',
    def test_module_resources(self):
        A module can have resources found adjacent to the module.
        import mod  # type: ignore[import-not-found]
        actual = resources.files(mod).joinpath('res.txt').read_text(encoding='utf-8')
        assert actual == self.spec['res.txt']
class ModuleFilesDiskTests(DirectSpec, util.DiskSetup, ModulesFiles, unittest.TestCase):
class ModuleFilesZipTests(DirectSpec, util.ZipSetup, ModulesFiles, unittest.TestCase):
class ImplicitContextFiles:
    set_val = textwrap.dedent(
        f"""
        import {resources.__name__} as res
        val = res.files().joinpath('res.txt').read_text(encoding='utf-8')
        'somepkg': {
            '__init__.py': set_val,
            'submod.py': set_val,
        'frozenpkg': {
            '__init__.py': set_val.replace(resources.__name__, 'c_resources'),
    def test_implicit_files_package(self):
        Without any parameter, files() will infer the location as the caller.
        assert importlib.import_module('somepkg').val == 'resources are the best'
    def test_implicit_files_submodule(self):
        assert importlib.import_module('somepkg.submod').val == 'resources are the best'
    def _compile_importlib(self):
        Make a compiled-only copy of the importlib resources package.
        Currently only code is copied, as importlib resources doesn't itself
        have any resources.
        bin_site = self.fixtures.enter_context(os_helper.temp_dir())
        c_resources = pathlib.Path(bin_site, 'c_resources')
        sources = pathlib.Path(resources.__file__).parent
        for source_path in sources.glob('**/*.py'):
            c_path = c_resources.joinpath(source_path.relative_to(sources)).with_suffix(
                '.pyc'
            py_compile.compile(source_path, c_path)
        self.fixtures.enter_context(import_helper.DirsOnSysPath(bin_site))
    def test_implicit_files_with_compiled_importlib(self):
        Caller detection works for compiled-only resources module.
        python/cpython#123085
        self._compile_importlib()
        assert importlib.import_module('frozenpkg').val == 'resources are the best'
class ImplicitContextFilesDiskTests(
    DirectSpec, util.DiskSetup, ImplicitContextFiles, unittest.TestCase
class ImplicitContextFilesZipTests(
    DirectSpec, util.ZipSetup, ImplicitContextFiles, unittest.TestCase
