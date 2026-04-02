from typing import overload
from typing_extensions import TypeGuard
    FileTypes,
    FileContent,
    HttpxFileTypes,
    Base64FileInput,
    HttpxFileContent,
from ._utils import is_tuple_t, is_mapping_t, is_sequence_t
def is_base64_file_input(obj: object) -> TypeGuard[Base64FileInput]:
    return isinstance(obj, io.IOBase) or isinstance(obj, os.PathLike)
def is_file_content(obj: object) -> TypeGuard[FileContent]:
        isinstance(obj, bytes) or isinstance(obj, tuple) or isinstance(obj, io.IOBase) or isinstance(obj, os.PathLike)
def assert_is_file_content(obj: object, *, key: str | None = None) -> None:
    if not is_file_content(obj):
        prefix = f"Expected entry at `{key}`" if key is not None else f"Expected file input `{obj!r}`"
            f"{prefix} to be bytes, an io.IOBase instance, PathLike or a tuple but received {type(obj)} instead. See https://github.com/openai/openai-python/tree/main#file-uploads"
def to_httpx_files(files: None) -> None: ...
def to_httpx_files(files: RequestFiles) -> HttpxRequestFiles: ...
def to_httpx_files(files: RequestFiles | None) -> HttpxRequestFiles | None:
    if files is None:
    if is_mapping_t(files):
        files = {key: _transform_file(file) for key, file in files.items()}
    elif is_sequence_t(files):
        files = [(key, _transform_file(file)) for key, file in files]
        raise TypeError(f"Unexpected file type input {type(files)}, expected mapping or sequence")
def _transform_file(file: FileTypes) -> HttpxFileTypes:
    if is_file_content(file):
        if isinstance(file, os.PathLike):
            path = pathlib.Path(file)
            return (path.name, path.read_bytes())
        return file
    if is_tuple_t(file):
        return (file[0], read_file_content(file[1]), *file[2:])
    raise TypeError(f"Expected file types input to be a FileContent type or to be a tuple")
def read_file_content(file: FileContent) -> HttpxFileContent:
        return pathlib.Path(file).read_bytes()
async def async_to_httpx_files(files: None) -> None: ...
async def async_to_httpx_files(files: RequestFiles) -> HttpxRequestFiles: ...
async def async_to_httpx_files(files: RequestFiles | None) -> HttpxRequestFiles | None:
        files = {key: await _async_transform_file(file) for key, file in files.items()}
        files = [(key, await _async_transform_file(file)) for key, file in files]
        raise TypeError("Unexpected file type input {type(files)}, expected mapping or sequence")
async def _async_transform_file(file: FileTypes) -> HttpxFileTypes:
            path = anyio.Path(file)
            return (path.name, await path.read_bytes())
        return (file[0], await async_read_file_content(file[1]), *file[2:])
async def async_read_file_content(file: FileContent) -> HttpxFileContent:
        return await anyio.Path(file).read_bytes()
