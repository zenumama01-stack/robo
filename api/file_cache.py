"""File based cache for the discovery document.
The cache is stored in a single file so that multiple processes can
share the same cache. It locks the file whenever accessing to the
file. When the cache content is corrupted, it will be initialized with
an empty cache.
from __future__ import division
    from oauth2client.contrib.locked_file import LockedFile
    # oauth2client < 2.0.0
        from oauth2client.locked_file import LockedFile
        # oauth2client > 4.0.0 or google-auth
            "file_cache is unavailable when using oauth2client >= 4.0.0 or google-auth"
FILENAME = "google-api-python-client-discovery-doc.cache"
def _to_timestamp(date):
        return (date - EPOCH).total_seconds()
        # The following is the equivalent of total_seconds() in Python2.6.
        # See also: https://docs.python.org/2/library/datetime.html
        delta = date - EPOCH
            delta.microseconds + (delta.seconds + delta.days * 24 * 3600) * 10**6
        ) / 10**6
def _read_or_initialize_cache(f):
    f.file_handle().seek(0)
        cache = json.load(f.file_handle())
        # This means it opens the file for the first time, or the cache is
        # corrupted, so initializing the file with an empty dict.
        cache = {}
        f.file_handle().truncate(0)
        json.dump(cache, f.file_handle())
    return cache
    """A file based cache for the discovery documents."""
        self._file = os.path.join(tempfile.gettempdir(), FILENAME)
        f = LockedFile(self._file, "a+", "r")
            f.open_and_lock()
            if f.is_locked():
                _read_or_initialize_cache(f)
            # If we can not obtain the lock, other process or thread must
            # have initialized the file.
            f.unlock_and_close()
        f = LockedFile(self._file, "r+", "r")
                cache = _read_or_initialize_cache(f)
                if url in cache:
                    content, t = cache.get(url, (None, 0))
                    if _to_timestamp(datetime.datetime.now()) < t + self._max_age:
                LOGGER.debug("Could not obtain a lock for the cache file.")
                cache[url] = (content, _to_timestamp(datetime.datetime.now()))
                # Remove stale cache.
                for k, (_, timestamp) in list(cache.items()):
                        _to_timestamp(datetime.datetime.now())
                        >= timestamp + self._max_age
                        del cache[k]
from typing import IO, TYPE_CHECKING
from pip._vendor.cachecontrol.cache import BaseCache, SeparateBodyBaseCache
    from filelock import BaseFileLock
class _FileCacheMixin:
    """Shared implementation for both FileCache variants."""
        directory: str | Path,
        forever: bool = False,
        filemode: int = 0o0600,
        dirmode: int = 0o0700,
        lock_class: type[BaseFileLock] | None = None,
            if lock_class is None:
                from filelock import FileLock
                lock_class = FileLock
            notice = dedent(
            NOTE: In order to use the FileCache you must have
            filelock installed. You can install it via pip:
              pip install cachecontrol[filecache]
            raise ImportError(notice)
        self.forever = forever
        self.filemode = filemode
        self.dirmode = dirmode
        self.lock_class = lock_class
    def encode(x: str) -> str:
        return hashlib.sha224(x.encode()).hexdigest()
    def _fn(self, name: str) -> str:
        # NOTE: This method should not change as some may depend on it.
        #       See: https://github.com/ionrock/cachecontrol/issues/63
        hashed = self.encode(name)
        name = self._fn(key)
            with open(name, "rb") as fh:
                return fh.read()
        self._write(name, value)
        Safely write the data to the given path.
        # Make sure the directory exists
        os.makedirs(dirname, self.dirmode, exist_ok=True)
        with self.lock_class(path + ".lock"):
            # Write our actual file
            (fd, name) = tempfile.mkstemp(dir=dirname)
                os.write(fd, data)
            os.chmod(name, self.filemode)
            os.replace(name, path)
    def _delete(self, key: str, suffix: str) -> None:
        name = self._fn(key) + suffix
        if not self.forever:
                os.remove(name)
class FileCache(_FileCacheMixin, BaseCache):
    Traditional FileCache: body is stored in memory, so not suitable for large
    downloads.
        self._delete(key, "")
class SeparateBodyFileCache(_FileCacheMixin, SeparateBodyBaseCache):
    Memory-efficient FileCache: body is stored in a separate file, reducing
    peak memory usage.
        name = self._fn(key) + ".body"
            return open(name, "rb")
        self._write(name, body)
        self._delete(key, ".body")
def url_to_file_path(url: str, filecache: FileCache) -> str:
    """Return the file cache path based on the URL.
    This does not ensure the file exists!
    key = CacheController.cache_url(url)
    return filecache._fn(key)
