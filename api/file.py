from ..model.hashes import Hashes
class File(OneDriveObjectBase):
    def hashes(self):
        Gets and sets the hashes
            :class:`Hashes<onedrivesdk.model.hashes.Hashes>`:
                The hashes
        if "hashes" in self._prop_dict:
            if isinstance(self._prop_dict["hashes"], OneDriveObjectBase):
                return self._prop_dict["hashes"]
                self._prop_dict["hashes"] = Hashes(self._prop_dict["hashes"])
    @hashes.setter
    def hashes(self, val):
        self._prop_dict["hashes"] = val
    def mime_type(self):
        """Gets and sets the mimeType
                The mimeType
        if "mimeType" in self._prop_dict:
            return self._prop_dict["mimeType"]
    @mime_type.setter
    def mime_type(self, val):
        self._prop_dict["mimeType"] = val
from django.contrib.sessions.backends.base import (
    VALID_KEY_CHARS,
    CreateError,
    SessionBase,
    UpdateError,
from django.contrib.sessions.exceptions import InvalidSessionKey
from django.core.exceptions import ImproperlyConfigured, SuspiciousOperation
    Implement a file based session store.
        self.storage_path = self._get_storage_path()
        self.file_prefix = settings.SESSION_COOKIE_NAME
    def _get_storage_path(cls):
            return cls._storage_path
            storage_path = (
                getattr(settings, "SESSION_FILE_PATH", None) or tempfile.gettempdir()
            # Make sure the storage path is valid.
            if not os.path.isdir(storage_path):
                    "The session storage path %r doesn't exist. Please set your"
                    " SESSION_FILE_PATH setting to an existing directory in which"
                    " Django can store session data." % storage_path
            cls._storage_path = storage_path
            return storage_path
    def _key_to_file(self, session_key=None):
        Get the file associated with this session key.
            session_key = self._get_or_create_session_key()
        # Make sure we're not vulnerable to directory traversal. Session keys
        # should always be md5s, so they should never contain directory
        # components.
        if not set(session_key).issubset(VALID_KEY_CHARS):
            raise InvalidSessionKey("Invalid characters in session key")
        return os.path.join(self.storage_path, self.file_prefix + session_key)
    def _last_modification(self):
        Return the modification time of the file storing the session's content.
        modification = os.stat(self._key_to_file()).st_mtime
        tz = datetime.UTC if settings.USE_TZ else None
        return datetime.datetime.fromtimestamp(modification, tz=tz)
    def _expiry_date(self, session_data):
        Return the expiry time of the file storing the session's content.
        return session_data.get("_session_expiry") or (
            self._last_modification()
            + datetime.timedelta(seconds=self.get_session_cookie_age())
        session_data = {}
            with open(self._key_to_file(), encoding="ascii") as session_file:
                file_data = session_file.read()
            # Don't fail if there is no data in the session file.
            # We may have opened the empty placeholder file.
            if file_data:
                    session_data = self.decode(file_data)
                except (EOFError, SuspiciousOperation) as e:
                    if isinstance(e, SuspiciousOperation):
                        logger = logging.getLogger(
                            "django.security.%s" % e.__class__.__name__
                        logger.warning(str(e))
                # Remove expired sessions.
                expiry_age = self.get_expiry_age(expiry=self._expiry_date(session_data))
                if expiry_age <= 0:
        except (OSError, SuspiciousOperation):
        return self.load()
        # Get the session data now, before we start messing
        # with the file it is stored within.
        session_data = self._get_session(no_load=must_create)
        session_file_name = self._key_to_file()
            # Make sure the file exists. If it does not already exist, an
            # empty placeholder file is created.
            flags = os.O_WRONLY | getattr(os, "O_BINARY", 0)
                flags |= os.O_EXCL | os.O_CREAT
            fd = os.open(session_file_name, flags)
            os.close(fd)
            if not must_create:
        # Write the session file without interfering with other threads
        # or processes. By writing to an atomically generated temporary
        # file and then using the atomic os.rename() to make the complete
        # file visible, we avoid having to lock the session file, while
        # still maintaining its integrity.
        # Note: Locking the session file was explored, but rejected in part
        # because in order to be atomic and cross-platform, it required a
        # long-lived lock file for each session, doubling the number of
        # files in the session storage directory at any given time. This
        # rename solution is cleaner and avoids any additional overhead
        # when reading the session data, which is the more common case
        # unless SESSION_SAVE_EVERY_REQUEST = True.
        # See ticket #8616.
        dir, prefix = os.path.split(session_file_name)
            output_file_fd, output_file_name = tempfile.mkstemp(
                dir=dir, prefix=prefix + "_out_"
            renamed = False
                    os.write(output_file_fd, self.encode(session_data).encode())
                    os.close(output_file_fd)
                # This will atomically rename the file (os.rename) if the OS
                # supports it. Otherwise this will result in a shutil.copy2
                # and os.unlink (for example on Windows). See #9084.
                shutil.move(output_file_name, session_file_name)
                renamed = True
                if not renamed:
                    os.unlink(output_file_name)
        except (EOFError, OSError):
        return self.save(must_create=must_create)
        return os.path.exists(self._key_to_file(session_key))
        return self.exists(session_key)
            os.unlink(self._key_to_file(session_key))
        return self.delete(session_key=session_key)
        storage_path = cls._get_storage_path()
        file_prefix = settings.SESSION_COOKIE_NAME
        for session_file in os.listdir(storage_path):
            if not session_file.startswith(file_prefix):
            session_key = session_file.removeprefix(file_prefix)
            session = cls(session_key)
            # When an expired session is loaded, its file is removed, and a
            # new file is immediately created. Prevent this by disabling
            # the create() method.
            session.create = lambda: None
            session.load()
        cls.clear_expired()
"""Callback handler that writes to a file."""
from typing import TYPE_CHECKING, Any, TextIO, cast
from langchain_core._api import warn_deprecated
from langchain_core.callbacks import BaseCallbackHandler
from langchain_core.utils.input import print_text
_GLOBAL_DEPRECATION_WARNED = False
class FileCallbackHandler(BaseCallbackHandler):
    """Callback handler that writes to a file.
    This handler supports both context manager usage (recommended) and direct
    instantiation (deprecated) for backwards compatibility.
        Using as a context manager (recommended):
        with FileCallbackHandler("output.txt") as handler:
            # Use handler with your chain/agent
            chain.invoke(inputs, config={"callbacks": [handler]})
        Direct instantiation (deprecated):
        handler = FileCallbackHandler("output.txt")
        # File remains open until handler is garbage collected
            handler.close()  # Explicit cleanup recommended
        filename: The file path to write to.
        mode: The file open mode. Defaults to `'a'` (append).
        color: Default color for text output.
        When not used as a context manager, a deprecation warning will be issued on
        first use. The file will be opened immediately in `__init__` and closed in
        `__del__` or when `close()` is called explicitly.
        self, filename: str, mode: str = "a", color: str | None = None
        """Initialize the file callback handler.
            filename: Path to the output file.
            mode: File open mode (e.g., `'w'`, `'a'`, `'x'`). Defaults to `'a'`.
            color: Default text color for output.
        self._file_opened_in_context = False
        self.file: TextIO = cast(
            "TextIO",
            # Open the file in the specified mode with UTF-8 encoding.
            Path(self.filename).open(self.mode, encoding="utf-8"),  # noqa: SIM115
        """Enter the context manager.
            The `FileCallbackHandler` instance.
            The file is already opened in `__init__`, so this just marks that the
            handler is being used as a context manager.
        self._file_opened_in_context = True
        exc_val: BaseException | None,
        exc_tb: object,
        """Exit the context manager and close the file.
            exc_type: Exception type if an exception occurred.
            exc_val: Exception value if an exception occurred.
            exc_tb: Exception traceback if an exception occurred.
        """Destructor to cleanup when done."""
        """Close the file if it's open.
        This method is safe to call multiple times and will only close
        the file if it's currently open.
        if hasattr(self, "file") and self.file and not self.file.closed:
    def _write(
        color: str | None = None,
        end: str = "",
        """Write text to the file with deprecation warning if needed.
            text: The text to write to the file.
            color: Optional color for the text. Defaults to `self.color`.
            end: String appended after the text.
            file: Optional file to write to. Defaults to `self.file`.
            RuntimeError: If the file is closed or not available.
        global _GLOBAL_DEPRECATION_WARNED  # noqa: PLW0603
        if not self._file_opened_in_context and not _GLOBAL_DEPRECATION_WARNED:
            warn_deprecated(
                since="0.3.67",
                pending=True,
                message=(
                    "Using FileCallbackHandler without a context manager is "
                    "deprecated. Use 'with FileCallbackHandler(...) as "
                    "handler:' instead."
            _GLOBAL_DEPRECATION_WARNED = True
        if not hasattr(self, "file") or self.file is None or self.file.closed:
            msg = "File is not open. Use FileCallbackHandler as a context manager."
            raise RuntimeError(msg)
        print_text(text, file=self.file, color=color, end=end)
        self, serialized: dict[str, Any], inputs: dict[str, Any], **kwargs: Any
        """Print that we are entering a chain.
            serialized: The serialized chain information.
            inputs: The inputs to the chain.
            **kwargs: Additional keyword arguments that may contain `'name'`.
        name = (
            kwargs.get("name")
            or serialized.get("name", serialized.get("id", ["<unknown>"])[-1])
            or "<unknown>"
        self._write(f"\n\n> Entering new {name} chain...", end="\n")
    def on_chain_end(self, outputs: dict[str, Any], **kwargs: Any) -> None:
        """Print that we finished a chain.
        self._write("\n> Finished chain.", end="\n")
        self, action: AgentAction, color: str | None = None, **kwargs: Any
        """Handle agent action by writing the action log.
            action: The agent action containing the log to write.
            color: Color override for this specific output.
                If `None`, uses `self.color`.
        self._write(action.log, color=color or self.color)
        output: str,
        observation_prefix: str | None = None,
        llm_prefix: str | None = None,
        """Handle tool end by writing the output with optional prefixes.
            output: The tool output to write.
            observation_prefix: Optional prefix to write before the output.
            llm_prefix: Optional prefix to write after the output.
        if observation_prefix is not None:
            self._write(f"\n{observation_prefix}")
        self._write(output)
        if llm_prefix is not None:
            self._write(f"\n{llm_prefix}")
        self, text: str, color: str | None = None, end: str = "", **kwargs: Any
        """Handle text output.
            text: The text to write.
        self._write(text, color=color or self.color, end=end)
        self, finish: AgentFinish, color: str | None = None, **kwargs: Any
        """Handle agent finish by writing the finish log.
            finish: The agent finish object containing the log to write.
        self._write(finish.log, color=color or self.color, end="\n")
__all__ = ["FileCallbackHandler"]
    from langchain_community.chat_message_histories import FileChatMessageHistory
    "FileReadStream",
    "FileStreamAttribute",
    "FileWriteStream",
from collections.abc import Callable, Mapping
from io import SEEK_SET, UnsupportedOperation
from typing import Any, BinaryIO, cast
from .. import (
    BrokenResourceError,
    ClosedResourceError,
    EndOfStream,
    TypedAttributeSet,
    to_thread,
    typed_attribute,
from ..abc import ByteReceiveStream, ByteSendStream
class FileStreamAttribute(TypedAttributeSet):
    #: the open file descriptor
    file: BinaryIO = typed_attribute()
    #: the path of the file on the file system, if available (file must be a real file)
    path: Path = typed_attribute()
    #: the file number, if available (file must be a real file or a TTY)
    fileno: int = typed_attribute()
class _BaseFileStream:
    def __init__(self, file: BinaryIO):
        await to_thread.run_sync(self._file.close)
    def extra_attributes(self) -> Mapping[Any, Callable[[], Any]]:
        attributes: dict[Any, Callable[[], Any]] = {
            FileStreamAttribute.file: lambda: self._file,
        if hasattr(self._file, "name"):
            attributes[FileStreamAttribute.path] = lambda: Path(self._file.name)
            self._file.fileno()
        except UnsupportedOperation:
            attributes[FileStreamAttribute.fileno] = lambda: self._file.fileno()
class FileReadStream(_BaseFileStream, ByteReceiveStream):
    A byte stream that reads from a file in the file system.
    :param file: a file that has been opened for reading in binary mode
    async def from_path(cls, path: str | PathLike[str]) -> FileReadStream:
        Create a file read stream by opening the given file.
        :param path: path of the file to read from
        file = await to_thread.run_sync(Path(path).open, "rb")
        return cls(cast(BinaryIO, file))
            data = await to_thread.run_sync(self._file.read, max_bytes)
            raise ClosedResourceError from None
            raise BrokenResourceError from exc
            raise EndOfStream
    async def seek(self, position: int, whence: int = SEEK_SET) -> int:
        Seek the file to the given position.
        .. seealso:: :meth:`io.IOBase.seek`
        .. note:: Not all file descriptors are seekable.
        :param position: position to seek the file to
        :param whence: controls how ``position`` is interpreted
        :return: the new absolute position
        :raises OSError: if the file is not seekable
        return await to_thread.run_sync(self._file.seek, position, whence)
    async def tell(self) -> int:
        Return the current stream position.
        :return: the current absolute position
        return await to_thread.run_sync(self._file.tell)
class FileWriteStream(_BaseFileStream, ByteSendStream):
    A byte stream that writes to a file in the file system.
    :param file: a file that has been opened for writing in binary mode
    async def from_path(
        cls, path: str | PathLike[str], append: bool = False
    ) -> FileWriteStream:
        Create a file write stream by opening the given file for writing.
        :param path: path of the file to write to
        :param append: if ``True``, open the file for appending; if ``False``, any
            existing file at the given path will be truncated
        mode = "ab" if append else "wb"
        file = await to_thread.run_sync(Path(path).open, mode)
            await to_thread.run_sync(self._file.write, item)
