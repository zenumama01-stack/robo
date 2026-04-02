from .._exceptions import OpenAIError
INSTRUCTIONS = """
OpenAI error:
    missing `{library}`
This feature requires additional dependencies:
    $ pip install openai[{extra}]
def format_instructions(*, library: str, extra: str) -> str:
    return INSTRUCTIONS.format(library=library, extra=extra)
class MissingDependencyError(OpenAIError):
from .abc import ResourceReader, Traversable
from ._compat import wrap_spec
Package = Union[types.ModuleType, str]
def files(package):
    # type: (Package) -> Traversable
    Get a Traversable resource from a package
    return from_package(get_package(package))
def get_resource_reader(package):
    # type: (types.ModuleType) -> Optional[ResourceReader]
    Return the package's loader if it's a ResourceReader.
    # We can't use
    # a issubclass() check here because apparently abc.'s __subclasscheck__()
    # hook wants to create a weak reference to the object, but
    # zipimport.zipimporter does not support weak references, resulting in a
    # TypeError.  That seems terrible.
    spec = package.__spec__
    reader = getattr(spec.loader, 'get_resource_reader', None)  # type: ignore
    if reader is None:
    return reader(spec.name)  # type: ignore
def resolve(cand):
    # type: (Package) -> types.ModuleType
    return cand if isinstance(cand, types.ModuleType) else importlib.import_module(cand)
def get_package(package):
    """Take a package name or module object and return the module.
    Raise an exception if the resolved module is not a package.
    resolved = resolve(package)
    if wrap_spec(resolved).submodule_search_locations is None:
        raise TypeError(f'{package!r} is not a package')
def from_package(package):
    Return a Traversable object for the given package.
    spec = wrap_spec(package)
    reader = spec.loader.get_resource_reader(spec.name)
    return reader.files()
def _tempfile(reader, suffix=''):
    # Not using tempfile.NamedTemporaryFile as it leads to deeper 'try'
    # blocks due to the need to close the temporary file to work on Windows
    # properly.
    fd, raw_path = tempfile.mkstemp(suffix=suffix)
            os.write(fd, reader())
        del reader
        yield pathlib.Path(raw_path)
            os.remove(raw_path)
@functools.singledispatch
def as_file(path):
    Given a Traversable object, return that object as a
    path on the local file system in a context manager.
    return _tempfile(path.read_bytes, suffix=path.name)
@as_file.register(pathlib.Path)
def _(path):
    Degenerate behavior for pathlib.Path objects.
"""Common objects shared by __init__.py and _ps*.py modules.
Note: this module is imported by setup.py, so it should not import
psutil or third-party modules.
from socket import AF_INET
from socket import SOCK_DGRAM
from socket import SOCK_STREAM
    from socket import AF_INET6
    AF_INET6 = None
    from socket import AF_UNIX
    AF_UNIX = None
PSUTIL_DEBUG = bool(os.getenv('PSUTIL_DEBUG'))
_DEFAULT = object()
    # OS constants
    'FREEBSD', 'BSD', 'LINUX', 'NETBSD', 'OPENBSD', 'MACOS', 'OSX', 'POSIX',
    'SUNOS', 'WINDOWS',
    # connection constants
    'CONN_CLOSE', 'CONN_CLOSE_WAIT', 'CONN_CLOSING', 'CONN_ESTABLISHED',
    'CONN_FIN_WAIT1', 'CONN_FIN_WAIT2', 'CONN_LAST_ACK', 'CONN_LISTEN',
    'CONN_NONE', 'CONN_SYN_RECV', 'CONN_SYN_SENT', 'CONN_TIME_WAIT',
    # net constants
    'NIC_DUPLEX_FULL', 'NIC_DUPLEX_HALF', 'NIC_DUPLEX_UNKNOWN',  # noqa: F822
    # process status constants
    'STATUS_DEAD', 'STATUS_DISK_SLEEP', 'STATUS_IDLE', 'STATUS_LOCKED',
    'STATUS_RUNNING', 'STATUS_SLEEPING', 'STATUS_STOPPED', 'STATUS_SUSPENDED',
    'STATUS_TRACING_STOP', 'STATUS_WAITING', 'STATUS_WAKE_KILL',
    'STATUS_WAKING', 'STATUS_ZOMBIE', 'STATUS_PARKED',
    # other constants
    'ENCODING', 'ENCODING_ERRS', 'AF_INET6',
    # utility functions
    'conn_tmap', 'deprecated_method', 'isfile_strict', 'memoize',
    'parse_environ_block', 'path_exists_strict', 'usage_percent',
    'supports_ipv6', 'sockfam_to_enum', 'socktype_to_enum', "wrap_numbers",
    'open_text', 'open_binary', 'cat', 'bcat',
    'bytes2human', 'conn_to_ntuple', 'debug',
    # shell utils
    'hilite', 'term_supports_colors', 'print_color',
# ===================================================================
# --- OS constants
POSIX = os.name == "posix"
WINDOWS = os.name == "nt"
LINUX = sys.platform.startswith("linux")
MACOS = sys.platform.startswith("darwin")
OSX = MACOS  # deprecated alias
FREEBSD = sys.platform.startswith(("freebsd", "midnightbsd"))
OPENBSD = sys.platform.startswith("openbsd")
NETBSD = sys.platform.startswith("netbsd")
BSD = FREEBSD or OPENBSD or NETBSD
SUNOS = sys.platform.startswith(("sunos", "solaris"))
AIX = sys.platform.startswith("aix")
# --- API constants
# Process.status()
STATUS_RUNNING = "running"
STATUS_SLEEPING = "sleeping"
STATUS_DISK_SLEEP = "disk-sleep"
STATUS_STOPPED = "stopped"
STATUS_TRACING_STOP = "tracing-stop"
STATUS_ZOMBIE = "zombie"
STATUS_DEAD = "dead"
STATUS_WAKE_KILL = "wake-kill"
STATUS_WAKING = "waking"
STATUS_IDLE = "idle"  # Linux, macOS, FreeBSD
STATUS_LOCKED = "locked"  # FreeBSD
STATUS_WAITING = "waiting"  # FreeBSD
STATUS_SUSPENDED = "suspended"  # NetBSD
STATUS_PARKED = "parked"  # Linux
# Process.net_connections() and psutil.net_connections()
CONN_ESTABLISHED = "ESTABLISHED"
CONN_SYN_SENT = "SYN_SENT"
CONN_SYN_RECV = "SYN_RECV"
CONN_FIN_WAIT1 = "FIN_WAIT1"
CONN_FIN_WAIT2 = "FIN_WAIT2"
CONN_TIME_WAIT = "TIME_WAIT"
CONN_CLOSE = "CLOSE"
CONN_CLOSE_WAIT = "CLOSE_WAIT"
CONN_LAST_ACK = "LAST_ACK"
CONN_LISTEN = "LISTEN"
CONN_CLOSING = "CLOSING"
CONN_NONE = "NONE"
# net_if_stats()
class NicDuplex(enum.IntEnum):
    NIC_DUPLEX_FULL = 2
    NIC_DUPLEX_HALF = 1
    NIC_DUPLEX_UNKNOWN = 0
globals().update(NicDuplex.__members__)
# sensors_battery()
class BatteryTime(enum.IntEnum):
    POWER_TIME_UNKNOWN = -1
    POWER_TIME_UNLIMITED = -2
globals().update(BatteryTime.__members__)
# --- others
ENCODING = sys.getfilesystemencoding()
ENCODING_ERRS = sys.getfilesystemencodeerrors()
# --- Process.net_connections() 'kind' parameter mapping
conn_tmap = {
    "all": ([AF_INET, AF_INET6, AF_UNIX], [SOCK_STREAM, SOCK_DGRAM]),
    "tcp": ([AF_INET, AF_INET6], [SOCK_STREAM]),
    "tcp4": ([AF_INET], [SOCK_STREAM]),
    "udp": ([AF_INET, AF_INET6], [SOCK_DGRAM]),
    "udp4": ([AF_INET], [SOCK_DGRAM]),
    "inet": ([AF_INET, AF_INET6], [SOCK_STREAM, SOCK_DGRAM]),
    "inet4": ([AF_INET], [SOCK_STREAM, SOCK_DGRAM]),
    "inet6": ([AF_INET6], [SOCK_STREAM, SOCK_DGRAM]),
if AF_INET6 is not None:
    conn_tmap.update({
        "tcp6": ([AF_INET6], [SOCK_STREAM]),
        "udp6": ([AF_INET6], [SOCK_DGRAM]),
if AF_UNIX is not None and not SUNOS:
    conn_tmap.update({"unix": ([AF_UNIX], [SOCK_STREAM, SOCK_DGRAM])})
# --- Exceptions
    """Base exception class. All other psutil exceptions inherit
    from this one.
    __module__ = 'psutil'
    def _infodict(self, attrs):
        for name in attrs:
            value = getattr(self, name, None)
            if value or (name == "pid" and value == 0):
                info[name] = value
        # invoked on `raise Error`
        info = self._infodict(("pid", "ppid", "name"))
            details = "({})".format(
                ", ".join([f"{k}={v!r}" for k, v in info.items()])
            details = None
        return " ".join([x for x in (getattr(self, "msg", ""), details) if x])
        # invoked on `repr(Error)`
        info = self._infodict(("pid", "ppid", "name", "seconds", "msg"))
        details = ", ".join([f"{k}={v!r}" for k, v in info.items()])
        return f"psutil.{self.__class__.__name__}({details})"
class NoSuchProcess(Error):
    """Exception raised when a process with a certain PID doesn't
    or no longer exists.
    def __init__(self, pid, name=None, msg=None):
        Error.__init__(self)
        self.pid = pid
        self.msg = msg or "process no longer exists"
        return (self.__class__, (self.pid, self.name, self.msg))
class ZombieProcess(NoSuchProcess):
    """Exception raised when querying a zombie process. This is
    raised on macOS, BSD and Solaris only, and not always: depending
    on the query the OS may be able to succeed anyway.
    On Linux all zombie processes are querable (hence this is never
    raised). Windows doesn't have zombie processes.
    def __init__(self, pid, name=None, ppid=None, msg=None):
        NoSuchProcess.__init__(self, pid, name, msg)
        self.ppid = ppid
        self.msg = msg or "PID still exists but it's a zombie"
        return (self.__class__, (self.pid, self.name, self.ppid, self.msg))
class AccessDenied(Error):
    """Exception raised when permission to perform an action is denied."""
    def __init__(self, pid=None, name=None, msg=None):
        self.msg = msg or ""
class TimeoutExpired(Error):
    """Raised on Process.wait(timeout) if timeout expires and process
    is still alive.
    def __init__(self, seconds, pid=None, name=None):
        self.seconds = seconds
        self.msg = f"timeout after {seconds} seconds"
        return (self.__class__, (self.seconds, self.pid, self.name))
# --- utils
def usage_percent(used, total, round_=None):
    """Calculate percentage usage of 'used' against 'total'."""
        ret = (float(used) / total) * 100
        if round_ is not None:
            ret = round(ret, round_)
def memoize(fun):
    """A simple memoize decorator for functions supporting (hashable)
    positional arguments.
    It also provides a cache_clear() function for clearing the cache:
    >>> @memoize
    ... def foo()
    ...     return 1
    >>> foo()
    >>> foo.cache_clear()
    It supports:
     - functions
     - classes (acts as a @singleton)
     - staticmethods
     - classmethods
    It does NOT support:
     - methods
    @functools.wraps(fun)
        key = (args, frozenset(sorted(kwargs.items())))
                ret = cache[key] = fun(*args, **kwargs)
                raise err from None
    def cache_clear():
        """Clear cache."""
    wrapper.cache_clear = cache_clear
def memoize_when_activated(fun):
    """A memoize decorator which is disabled by default. It can be
    activated and deactivated on request.
    For efficiency reasons it can be used only against class methods
    accepting no arguments.
    >>> class Foo:
    ...     @memoize
    ...     def foo()
    ...         print(1)
    >>> f = Foo()
    >>> # deactivated (default)
    >>> # activated
    >>> foo.cache_activate(self)
    def wrapper(self):
            # case 1: we previously entered oneshot() ctx
            ret = self._cache[fun]
            # case 2: we never entered oneshot() ctx
                return fun(self)
            # case 3: we entered oneshot() ctx but there's no cache
            # for this entry yet
                ret = fun(self)
                self._cache[fun] = ret
                # multi-threading race condition, see:
                # https://github.com/giampaolo/psutil/issues/1948
    def cache_activate(proc):
        """Activate cache. Expects a Process instance. Cache will be
        stored as a "_cache" instance attribute.
        proc._cache = {}
    def cache_deactivate(proc):
        """Deactivate and clear cache."""
            del proc._cache
    wrapper.cache_activate = cache_activate
    wrapper.cache_deactivate = cache_deactivate
def isfile_strict(path):
    """Same as os.path.isfile() but does not swallow EACCES / EPERM
    exceptions, see:
    http://mail.python.org/pipermail/python-dev/2012-June/120787.html.
    except PermissionError:
        return stat.S_ISREG(st.st_mode)
def path_exists_strict(path):
    """Same as os.path.exists() but does not swallow EACCES / EPERM
    exceptions. See:
        os.stat(path)
def supports_ipv6():
    """Return True if IPv6 is supported on this platform."""
    if not socket.has_ipv6 or AF_INET6 is None:
        with socket.socket(AF_INET6, socket.SOCK_STREAM) as sock:
            sock.bind(("::1", 0))
def parse_environ_block(data):
    """Parse a C environ block of environment variables into a dictionary."""
    # The block is usually raw data from the target process.  It might contain
    # trailing garbage and lines that do not look like assignments.
    # localize global variable to speed up access.
    WINDOWS_ = WINDOWS
        next_pos = data.find("\0", pos)
        # nul byte at the beginning or double nul byte means finish
        if next_pos <= pos:
        # there might not be an equals sign
        equal_pos = data.find("=", pos, next_pos)
        if equal_pos > pos:
            key = data[pos:equal_pos]
            value = data[equal_pos + 1 : next_pos]
            # Windows expects environment variables to be uppercase only
            if WINDOWS_:
                key = key.upper()
            ret[key] = value
        pos = next_pos + 1
def sockfam_to_enum(num):
    """Convert a numeric socket family value to an IntEnum member.
    If it's not a known member, return the numeric value itself.
        return socket.AddressFamily(num)
        return num
def socktype_to_enum(num):
    """Convert a numeric socket type value to an IntEnum member.
        return socket.SocketKind(num)
def conn_to_ntuple(fd, fam, type_, laddr, raddr, status, status_map, pid=None):
    """Convert a raw connection tuple to a proper ntuple."""
    from . import _ntuples as ntp
    if fam in {socket.AF_INET, AF_INET6}:
        if laddr:
            laddr = ntp.addr(*laddr)
        if raddr:
            raddr = ntp.addr(*raddr)
    if type_ == socket.SOCK_STREAM and fam in {AF_INET, AF_INET6}:
        status = status_map.get(status, CONN_NONE)
        status = CONN_NONE  # ignore whatever C returned to us
    fam = sockfam_to_enum(fam)
    type_ = socktype_to_enum(type_)
        return ntp.pconn(fd, fam, type_, laddr, raddr, status)
        return ntp.sconn(fd, fam, type_, laddr, raddr, status, pid)
def broadcast_addr(addr):
    """Given the address ntuple returned by ``net_if_addrs()``
    calculates the broadcast address.
    if not addr.address or not addr.netmask:
    if addr.family == socket.AF_INET:
            ipaddress.IPv4Network(
                f"{addr.address}/{addr.netmask}", strict=False
            ).broadcast_address
    if addr.family == socket.AF_INET6:
            ipaddress.IPv6Network(
def deprecated_method(replacement):
    """A decorator which can be used to mark a method as deprecated
    'replcement' is the method name which will be called instead.
    def outer(fun):
            f"{fun.__name__}() is deprecated and will be removed; use"
            f" {replacement}() instead"
        if fun.__doc__ is None:
            fun.__doc__ = msg
        def inner(self, *args, **kwargs):
            warnings.warn(msg, category=DeprecationWarning, stacklevel=2)
            return getattr(self, replacement)(*args, **kwargs)
class _WrapNumbers:
    """Watches numbers so that they don't overflow and wrap
    (reset to zero).
        self.cache = {}
        self.reminders = {}
        self.reminder_keys = {}
    def _add_dict(self, input_dict, name):
        assert name not in self.cache
        assert name not in self.reminders
        assert name not in self.reminder_keys
        self.cache[name] = input_dict
        self.reminders[name] = collections.defaultdict(int)
        self.reminder_keys[name] = collections.defaultdict(set)
    def _remove_dead_reminders(self, input_dict, name):
        """In case the number of keys changed between calls (e.g. a
        disk disappears) this removes the entry from self.reminders.
        old_dict = self.cache[name]
        gone_keys = set(old_dict.keys()) - set(input_dict.keys())
        for gone_key in gone_keys:
            for remkey in self.reminder_keys[name][gone_key]:
                del self.reminders[name][remkey]
            del self.reminder_keys[name][gone_key]
    def run(self, input_dict, name):
        """Cache dict and sum numbers which overflow and wrap.
        Return an updated copy of `input_dict`.
        if name not in self.cache:
            # This was the first call.
            self._add_dict(input_dict, name)
            return input_dict
        self._remove_dead_reminders(input_dict, name)
        new_dict = {}
        for key in input_dict:
            input_tuple = input_dict[key]
                old_tuple = old_dict[key]
                # The input dict has a new key (e.g. a new disk or NIC)
                # which didn't exist in the previous call.
                new_dict[key] = input_tuple
            bits = []
            for i in range(len(input_tuple)):
                input_value = input_tuple[i]
                old_value = old_tuple[i]
                remkey = (key, i)
                if input_value < old_value:
                    # it wrapped!
                    self.reminders[name][remkey] += old_value
                    self.reminder_keys[name][key].add(remkey)
                bits.append(input_value + self.reminders[name][remkey])
            new_dict[key] = tuple(bits)
        return new_dict
    def cache_clear(self, name=None):
        """Clear the internal cache, optionally only for function 'name'."""
                self.cache.clear()
                self.reminders.clear()
                self.reminder_keys.clear()
                self.cache.pop(name, None)
                self.reminders.pop(name, None)
                self.reminder_keys.pop(name, None)
    def cache_info(self):
        """Return internal cache dicts as a tuple of 3 elements."""
            return (self.cache, self.reminders, self.reminder_keys)
def wrap_numbers(input_dict, name):
    """Given an `input_dict` and a function `name`, adjust the numbers
    which "wrap" (restart from zero) across different calls by adding
    "old value" to "new value" and return an updated dict.
    with _wn.lock:
        return _wn.run(input_dict, name)
_wn = _WrapNumbers()
wrap_numbers.cache_clear = _wn.cache_clear
wrap_numbers.cache_info = _wn.cache_info
# The read buffer size for open() builtin. This (also) dictates how
# much data we read(2) when iterating over file lines as in:
#   >>> with open(file) as f:
#   ...    for line in f:
#   ...        ...
# Default per-line buffer size for binary files is 1K. For text files
# is 8K. We use a bigger buffer (32K) in order to have more consistent
# results when reading /proc pseudo files on Linux, see:
# https://github.com/giampaolo/psutil/issues/2050
# https://github.com/giampaolo/psutil/issues/708
FILE_READ_BUFFER_SIZE = 32 * 1024
def open_binary(fname):
    return open(fname, "rb", buffering=FILE_READ_BUFFER_SIZE)
def open_text(fname):
    """Open a file in text mode by using the proper FS encoding and
    en/decoding error handlers.
    # See:
    # https://github.com/giampaolo/psutil/issues/675
    # https://github.com/giampaolo/psutil/pull/733
    fobj = open(  # noqa: SIM115
        buffering=FILE_READ_BUFFER_SIZE,
        encoding=ENCODING,
        errors=ENCODING_ERRS,
        # Dictates per-line read(2) buffer size. Defaults is 8k. See:
        # https://github.com/giampaolo/psutil/issues/2050#issuecomment-1013387546
        fobj._CHUNK_SIZE = FILE_READ_BUFFER_SIZE
        fobj.close()
    return fobj
def cat(fname, fallback=_DEFAULT, _open=open_text):
    """Read entire file content and return it as a string. File is
    opened in text mode. If specified, `fallback` is the value
    returned in case of error, either if the file does not exist or
    it can't be read().
    if fallback is _DEFAULT:
        with _open(fname) as f:
def bcat(fname, fallback=_DEFAULT):
    """Same as above but opens file in binary mode."""
    return cat(fname, fallback=fallback, _open=open_binary)
def bytes2human(n, format="%(value).1f%(symbol)s"):
    """Used by various scripts. See: https://code.activestate.com/recipes/578019-bytes-to-human-human-to-bytes-converter/?in=user-4178764.
    >>> bytes2human(10000)
    '9.8K'
    >>> bytes2human(100001221)
    '95.4M'
    symbols = ('B', 'K', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y')
    prefix = {}
    for i, s in enumerate(symbols[1:]):
        prefix[s] = 1 << (i + 1) * 10
    for symbol in reversed(symbols[1:]):
        if abs(n) >= prefix[symbol]:
            value = float(n) / prefix[symbol]
            return format % locals()
    return format % dict(symbol=symbols[0], value=n)
def get_procfs_path():
    """Return updated psutil.PROCFS_PATH constant."""
    return sys.modules['psutil'].PROCFS_PATH
def decode(s):
    return s.decode(encoding=ENCODING, errors=ENCODING_ERRS)
# --- shell utils
@memoize
def term_supports_colors(file=sys.stdout):  # pragma: no cover
    if not hasattr(file, "isatty") or not file.isatty():
        file.fileno()
def hilite(s, color=None, bold=False):  # pragma: no cover
    """Return an highlighted version of 'string'."""
    if not term_supports_colors():
    attr = []
    colors = dict(
        blue='34',
        brown='33',
        darkgrey='30',
        green='32',
        grey='37',
        lightblue='36',
        red='91',
        violet='35',
        yellow='93',
    colors[None] = '29'
        color = colors[color]
        msg = f"invalid color {color!r}; choose amongst {list(colors.keys())}"
    attr.append(color)
        attr.append('1')
    return f"\x1b[{';'.join(attr)}m{s}\x1b[0m"
def print_color(
    s, color=None, bold=False, file=sys.stdout
):  # pragma: no cover
    """Print a colorized version of string."""
        print(s, file=file)
    elif POSIX:
        print(hilite(s, color, bold), file=file)
        DEFAULT_COLOR = 7
        GetStdHandle = ctypes.windll.Kernel32.GetStdHandle
        SetConsoleTextAttribute = (
            ctypes.windll.Kernel32.SetConsoleTextAttribute
        colors = dict(green=2, red=4, brown=6, yellow=6)
        colors[None] = DEFAULT_COLOR
                f"invalid color {color!r}; choose between"
                f" {list(colors.keys())!r}"
        if bold and color <= 7:
            color += 8
        handle_id = -12 if file is sys.stderr else -11
        GetStdHandle.restype = ctypes.c_ulong
        handle = GetStdHandle(handle_id)
        SetConsoleTextAttribute(handle, color)
            SetConsoleTextAttribute(handle, DEFAULT_COLOR)
def debug(msg):
    """If PSUTIL_DEBUG env var is set, print a debug message to stderr."""
    if PSUTIL_DEBUG:
        fname, lineno, _, _lines, _index = inspect.getframeinfo(
            inspect.currentframe().f_back
        if isinstance(msg, Exception):
            if isinstance(msg, OSError):
                # ...because str(exc) may contain info about the file name
                msg = f"ignoring {msg}"
                msg = f"ignoring {msg!r}"
        print(  # noqa: T201
            f"psutil-debug [{fname}:{lineno}]> {msg}", file=sys.stderr
from typing import Optional, Union, cast
Anchor = Package
def package_to_anchor(func):
    Replace 'package' parameter as 'anchor' and warn about the change.
    Other errors should fall through.
    >>> files('a', 'b')
    TypeError: files() takes from 0 to 1 positional arguments but 2 were given
    Remove this compatibility in Python 3.14.
    undefined = object()
    def wrapper(anchor=undefined, package=undefined):
        if package is not undefined:
            if anchor is not undefined:
                return func(anchor, package)
                "First parameter to files is renamed to 'anchor'",
            return func(package)
        elif anchor is undefined:
        return func(anchor)
@package_to_anchor
def files(anchor: Optional[Anchor] = None) -> Traversable:
    Get a Traversable resource for an anchor.
    return from_package(resolve(anchor))
def get_resource_reader(package: types.ModuleType) -> Optional[ResourceReader]:
    reader = getattr(spec.loader, 'get_resource_reader', None)  # type: ignore[union-attr]
    return reader(spec.name)  # type: ignore[union-attr]
def resolve(cand: Optional[Anchor]) -> types.ModuleType:
    return cast(types.ModuleType, cand)
@resolve.register
def _(cand: str) -> types.ModuleType:
    return importlib.import_module(cand)
def _(cand: None) -> types.ModuleType:
    return resolve(_infer_caller().f_globals['__name__'])
def _infer_caller():
    Walk the stack and find the frame of the first caller not in this module.
    def is_this_file(frame_info):
        return frame_info.filename == stack[0].filename
    def is_wrapper(frame_info):
        return frame_info.function == 'wrapper'
    stack = inspect.stack()
    not_this_file = itertools.filterfalse(is_this_file, stack)
    # also exclude 'wrapper' due to singledispatch in the call stack
    callers = itertools.filterfalse(is_wrapper, not_this_file)
    return next(callers).frame
def from_package(package: types.ModuleType):
    # deferred for performance (python/cpython#109829)
    from .future.adapters import wrap_spec
def _tempfile(
    reader,
    suffix='',
    # gh-93353: Keep a reference to call os.remove() in late Python
    # finalization.
    _os_remove=os.remove,
            _os_remove(raw_path)
def _temp_file(path):
def _is_present_dir(path: Traversable) -> bool:
    Some Traversables implement ``is_dir()`` to raise an
    exception (i.e. ``FileNotFoundError``) when the
    directory doesn't exist. This function wraps that call
    to always return a boolean and only return True
    if there's a dir and it exists.
    with contextlib.suppress(FileNotFoundError):
        return path.is_dir()
    return _temp_dir(path) if _is_present_dir(path) else _temp_file(path)
def _temp_path(dir: tempfile.TemporaryDirectory):
    Wrap tempfile.TemporaryDirectory to return a pathlib object.
    with dir as result:
        yield pathlib.Path(result)
def _temp_dir(path):
    Given a traversable dir, recursively replicate the whole tree
    to the file system in a context manager.
    assert path.is_dir()
    with _temp_path(tempfile.TemporaryDirectory()) as temp_dir:
        yield _write_contents(temp_dir, path)
def _write_contents(target, source):
    child = target.joinpath(source.name)
    if source.is_dir():
        child.mkdir()
        for item in source.iterdir():
            _write_contents(child, item)
        child.write_bytes(source.read_bytes())
# Use module-specific logger with a default null handler.
_logger = logging.getLogger('backoff')
_logger.addHandler(logging.NullHandler())  # pragma: no cover
_logger.setLevel(logging.INFO)
# Evaluate arg that can be either a fixed value or a callable.
def _maybe_call(f, *args, **kwargs):
    if callable(f):
def _init_wait_gen(wait_gen, wait_gen_kwargs):
    kwargs = {k: _maybe_call(v) for k, v in wait_gen_kwargs.items()}
    initialized = wait_gen(**kwargs)
    initialized.send(None)  # Initialize with an empty send
    return initialized
def _next_wait(wait, send_value, jitter, elapsed, max_time):
    value = wait.send(send_value)
        if jitter is not None:
            seconds = jitter(value)
            seconds = value
            "Nullary jitter function signature is deprecated. Use "
            "unary signature accepting a wait value in seconds and "
            "returning a jittered version of it.",
        seconds = value + jitter()
    # don't sleep longer than remaining allotted max_time
    if max_time is not None:
        seconds = min(seconds, max_time - elapsed)
    return seconds
def _prepare_logger(logger):
    if isinstance(logger, str):
        logger = logging.getLogger(logger)
# Configure handler list with user specified handler and optionally
# with a default handler bound to the specified logger.
def _config_handlers(
    user_handlers, *, default_handler=None, logger=None, log_level=None
    handlers = []
    if logger is not None:
        assert log_level is not None, "Log level is not specified"
        # bind the specified logger to the default log handler
        log_handler = functools.partial(
            default_handler, logger=logger, log_level=log_level
        handlers.append(log_handler)
    if user_handlers is None:
        return handlers
    # user specified handlers can either be an iterable of handlers
    # or a single handler. either way append them to the list.
    if hasattr(user_handlers, '__iter__'):
        # add all handlers in the iterable
        handlers += list(user_handlers)
        # append a single handler
        handlers.append(user_handlers)
# Default backoff handler
def _log_backoff(details, logger, log_level):
    msg = "Backing off %s(...) for %.1fs (%s)"
    log_args = [details['target'].__name__, details['wait']]
    exc_typ, exc, _ = sys.exc_info()
    if exc is not None:
        exc_fmt = traceback.format_exception_only(exc_typ, exc)[-1]
        log_args.append(exc_fmt.rstrip("\n"))
        log_args.append(details['value'])
    logger.log(log_level, msg, *log_args)
# Default giveup handler
def _log_giveup(details, logger, log_level):
    msg = "Giving up %s(...) after %d tries (%s)"
    log_args = [details['target'].__name__, details['tries']]
ConfidenceInterval = namedtuple("ConfidenceInterval", ["low", "high"])
ConfidenceInterval. __doc__ = "Class for confidence intervals."
Common code used in multiple modules.
class weekday(object):
    __slots__ = ["weekday", "n"]
    def __init__(self, weekday, n=None):
        self.weekday = weekday
        self.n = n
    def __call__(self, n):
        if n == self.n:
            return self.__class__(self.weekday, n)
            if self.weekday != other.weekday or self.n != other.n:
        return hash((
          self.weekday,
        return not (self == other)
        s = ("MO", "TU", "WE", "TH", "FR", "SA", "SU")[self.weekday]
        if not self.n:
            return "%s(%+d)" % (s, self.n)
# vim:ts=4:sw=4:et
from six import PY2
from datetime import datetime, timedelta, tzinfo
ZERO = timedelta(0)
__all__ = ['tzname_in_python2', 'enfold']
def tzname_in_python2(namefunc):
    """Change unicode output into bytestrings in Python 2
    tzname() API changed in Python 3. It used to return bytes, but was changed
    to unicode strings
    if PY2:
        @wraps(namefunc)
        def adjust_encoding(*args, **kwargs):
            name = namefunc(*args, **kwargs)
                name = name.encode()
        return adjust_encoding
        return namefunc
# The following is adapted from Alexander Belopolsky's tz library
# https://github.com/abalkin/tz
if hasattr(datetime, 'fold'):
    # This is the pre-python 3.6 fold situation
    def enfold(dt, fold=1):
        Provides a unified interface for assigning the ``fold`` attribute to
        datetimes both before and after the implementation of PEP-495.
        :param fold:
            The value for the ``fold`` attribute in the returned datetime. This
            should be either 0 or 1.
            Returns an object for which ``getattr(dt, 'fold', 0)`` returns
            ``fold`` for all versions of Python. In versions prior to
            Python 3.6, this is a ``_DatetimeWithFold`` object, which is a
            subclass of :py:class:`datetime.datetime` with the ``fold``
            attribute added, if ``fold`` is 1.
        .. versionadded:: 2.6.0
        return dt.replace(fold=fold)
    class _DatetimeWithFold(datetime):
        This is a class designed to provide a PEP 495-compliant interface for
        Python versions before 3.6. It is used only for dates in a fold, so
        the ``fold`` attribute is fixed at ``1``.
        def replace(self, *args, **kwargs):
            Return a datetime with the same attributes, except for those
            attributes given new values by whichever keyword arguments are
            specified. Note that tzinfo=None can be specified to create a naive
            datetime from an aware datetime with no conversion of date and time
            This is reimplemented in ``_DatetimeWithFold`` because pypy3 will
            return a ``datetime.datetime`` even if ``fold`` is unchanged.
            argnames = (
                'year', 'month', 'day', 'hour', 'minute', 'second',
                'microsecond', 'tzinfo'
            for arg, argname in zip(args, argnames):
                if argname in kwargs:
                    raise TypeError('Duplicate argument: {}'.format(argname))
                kwargs[argname] = arg
            for argname in argnames:
                if argname not in kwargs:
                    kwargs[argname] = getattr(self, argname)
            dt_class = self.__class__ if kwargs.get('fold', 1) else datetime
            return dt_class(**kwargs)
        def fold(self):
        if getattr(dt, 'fold', 0) == fold:
        args = dt.timetuple()[:6]
        args += (dt.microsecond, dt.tzinfo)
        if fold:
            return _DatetimeWithFold(*args)
            return datetime(*args)
def _validate_fromutc_inputs(f):
    The CPython version of ``fromutc`` checks that the input is a ``datetime``
    object and that ``self`` is attached as its ``tzinfo``.
    def fromutc(self, dt):
        if not isinstance(dt, datetime):
            raise TypeError("fromutc() requires a datetime argument")
        if dt.tzinfo is not self:
            raise ValueError("dt.tzinfo is not self")
        return f(self, dt)
    return fromutc
class _tzinfo(tzinfo):
    Base class for all ``dateutil`` ``tzinfo`` objects.
    def is_ambiguous(self, dt):
        Whether or not the "wall time" of a given datetime is ambiguous in this
            A :py:class:`datetime.datetime`, naive or time zone aware.
            Returns ``True`` if ambiguous, ``False`` otherwise.
        dt = dt.replace(tzinfo=self)
        wall_0 = enfold(dt, fold=0)
        wall_1 = enfold(dt, fold=1)
        same_offset = wall_0.utcoffset() == wall_1.utcoffset()
        same_dt = wall_0.replace(tzinfo=None) == wall_1.replace(tzinfo=None)
        return same_dt and not same_offset
    def _fold_status(self, dt_utc, dt_wall):
        Determine the fold status of a "wall" datetime, given a representation
        of the same datetime as a (naive) UTC datetime. This is calculated based
        on the assumption that ``dt.utcoffset() - dt.dst()`` is constant for all
        datetimes, and that this offset is the actual number of hours separating
        ``dt_utc`` and ``dt_wall``.
        :param dt_utc:
            Representation of the datetime as UTC
        :param dt_wall:
            Representation of the datetime as "wall time". This parameter must
            either have a `fold` attribute or have a fold-naive
            :class:`datetime.tzinfo` attached, otherwise the calculation may
            fail.
        if self.is_ambiguous(dt_wall):
            delta_wall = dt_wall - dt_utc
            _fold = int(delta_wall == (dt_utc.utcoffset() - dt_utc.dst()))
            _fold = 0
        return _fold
    def _fold(self, dt):
        return getattr(dt, 'fold', 0)
    def _fromutc(self, dt):
        Given a timezone-aware datetime in a given timezone, calculates a
        timezone-aware datetime in a new timezone.
        Since this is the one time that we *know* we have an unambiguous
        datetime object, we take this opportunity to determine whether the
        datetime is ambiguous and in a "fold" state (e.g. if it's the first
        occurrence, chronologically, of the ambiguous datetime).
            A timezone-aware :class:`datetime.datetime` object.
        # Re-implement the algorithm from Python's datetime.py
        dtoff = dt.utcoffset()
        if dtoff is None:
            raise ValueError("fromutc() requires a non-None utcoffset() "
                             "result")
        # The original datetime.py code assumes that `dst()` defaults to
        # zero during ambiguous times. PEP 495 inverts this presumption, so
        # for pre-PEP 495 versions of python, we need to tweak the algorithm.
        dtdst = dt.dst()
        if dtdst is None:
            raise ValueError("fromutc() requires a non-None dst() result")
        delta = dtoff - dtdst
        dt += delta
        # Set fold=1 so we can default to being in the fold for
        # ambiguous dates.
        dtdst = enfold(dt, fold=1).dst()
            raise ValueError("fromutc(): dt.dst gave inconsistent "
                             "results; cannot convert")
        return dt + dtdst
    @_validate_fromutc_inputs
        dt_wall = self._fromutc(dt)
        # Calculate the fold status given the two datetimes.
        _fold = self._fold_status(dt, dt_wall)
        # Set the default fold value for ambiguous dates
        return enfold(dt_wall, fold=_fold)
class tzrangebase(_tzinfo):
    This is an abstract base class for time zones represented by an annual
    transition into and out of DST. Child classes should implement the following
    methods:
        * ``__init__(self, *args, **kwargs)``
        * ``transitions(self, year)`` - this is expected to return a tuple of
          datetimes representing the DST on and off transitions in standard
    A fully initialized ``tzrangebase`` subclass should also provide the
    following attributes:
        * ``hasdst``: Boolean whether or not the zone uses DST.
        * ``_dst_offset`` / ``_std_offset``: :class:`datetime.timedelta` objects
          representing the respective UTC offsets.
        * ``_dst_abbr`` / ``_std_abbr``: Strings representing the timezone short
          abbreviations in DST and STD, respectively.
        * ``_hasdst``: Whether or not the zone has DST.
        raise NotImplementedError('tzrangebase is an abstract base class')
    def utcoffset(self, dt):
        isdst = self._isdst(dt)
        if isdst is None:
        elif isdst:
            return self._dst_offset
            return self._std_offset
    def dst(self, dt):
            return self._dst_base_offset
            return ZERO
    @tzname_in_python2
    def tzname(self, dt):
        if self._isdst(dt):
            return self._dst_abbr
            return self._std_abbr
        """ Given a datetime in UTC, return local time """
        # Get transitions - if there are none, fixed offset
        transitions = self.transitions(dt.year)
        if transitions is None:
            return dt + self.utcoffset(dt)
        # Get the transition times in UTC
        dston, dstoff = transitions
        dston -= self._std_offset
        dstoff -= self._std_offset
        utc_transitions = (dston, dstoff)
        dt_utc = dt.replace(tzinfo=None)
        isdst = self._naive_isdst(dt_utc, utc_transitions)
        if isdst:
            dt_wall = dt + self._dst_offset
            dt_wall = dt + self._std_offset
        _fold = int(not isdst and self.is_ambiguous(dt_wall))
        if not self.hasdst:
        start, end = self.transitions(dt.year)
        return (end <= dt < end + self._dst_base_offset)
    def _isdst(self, dt):
        elif dt is None:
        isdst = self._naive_isdst(dt, transitions)
        # Handle ambiguous dates
        if not isdst and self.is_ambiguous(dt):
            return not self._fold(dt)
            return isdst
    def _naive_isdst(self, dt, transitions):
        if dston < dstoff:
            isdst = dston <= dt < dstoff
            isdst = not dstoff <= dt < dston
    def _dst_base_offset(self):
        return self._dst_offset - self._std_offset
    __hash__ = None
        return "%s(...)" % self.__class__.__name__
    __reduce__ = object.__reduce__
"""Contains utilities used by both the sync and async inference clients."""
from typing import TYPE_CHECKING, Any, AsyncIterable, BinaryIO, Iterable, Literal, NoReturn, Optional, Union, overload
    GenerationError,
    IncompleteGenerationError,
    OverloadedError,
    TextGenerationError,
from ..utils import get_session, is_numpy_available, is_pillow_available
from ._generated.types import ChatCompletionStreamOutput, TextGenerationStreamOutput
# TYPES
UrlT = str
PathT = Union[str, Path]
ContentT = Union[bytes, BinaryIO, PathT, UrlT, "Image", bytearray, memoryview]
# Use to set an Accept: image/png header
TASKS_EXPECTING_IMAGES = {"text-to-image", "image-to-image"}
class RequestParameters:
    task: str
    json: Optional[Union[str, dict, list]]
    data: Optional[bytes]
    headers: dict[str, Any]
class MimeBytes(bytes):
    A bytes object with a mime type.
    To be returned by `_prepare_payload_open_as_mime_bytes` in subclasses.
        >>> b = MimeBytes(b"hello", "text/plain")
        >>> isinstance(b, bytes)
        >>> b.mime_type
        'text/plain'
    mime_type: Optional[str]
    def __new__(cls, data: bytes, mime_type: Optional[str] = None):
        obj = super().__new__(cls, data)
        obj.mime_type = mime_type
        if isinstance(data, MimeBytes) and mime_type is None:
            obj.mime_type = data.mime_type
## IMPORT UTILS
def _import_numpy():
    """Make sure `numpy` is installed on the machine."""
    if not is_numpy_available():
        raise ImportError("Please install numpy to use deal with embeddings (`pip install numpy`).")
def _import_pil_image():
    """Make sure `PIL` is installed on the machine."""
    if not is_pillow_available():
            "Please install Pillow to use deal with images (`pip install Pillow`). If you don't want the image to be"
            " post-processed, use `client.post(...)` and get the raw response from the server."
    return Image
## ENCODING / DECODING UTILS
def _open_as_mime_bytes(content: ContentT) -> MimeBytes: ...  # means "if input is not None, output is not None"
def _open_as_mime_bytes(content: Literal[None]) -> Literal[None]: ...  # means "if input is None, output is None"
def _open_as_mime_bytes(content: Optional[ContentT]) -> Optional[MimeBytes]:
    """Open `content` as a binary file, either from a URL, a local path, raw bytes, or a PIL Image.
    Do nothing if `content` is None.
    # If content is None, yield None
    # If content is bytes, return it
        return MimeBytes(content)
    # If content is raw binary data (bytearray, memoryview)
    if isinstance(content, (bytearray, memoryview)):
        return MimeBytes(bytes(content))
    # If content is a binary file-like object
    if hasattr(content, "read"):  # duck-typing instead of isinstance(content, BinaryIO)
        logger.debug("Reading content from BinaryIO")
        data = content.read()
        mime_type = mimetypes.guess_type(str(content.name))[0] if hasattr(content, "name") else None
            raise TypeError("Expected binary stream (bytes), but got text stream")
        return MimeBytes(data, mime_type=mime_type)
    # If content is a string => must be either a URL or a path
        if content.startswith("https://") or content.startswith("http://"):
            logger.debug(f"Downloading content from {content}")
            response = get_session().get(content)
            mime_type = response.headers.get("Content-Type")
            if mime_type is None:
                mime_type = mimetypes.guess_type(content)[0]
            return MimeBytes(response.content, mime_type=mime_type)
        content = Path(content)
        if not content.exists():
            raise FileNotFoundError(
                f"File not found at {content}. If `data` is a string, it must either be a URL or a path to a local"
                " file. To pass raw content, please encode it as bytes first."
    # If content is a Path => open it
    if isinstance(content, Path):
        logger.debug(f"Opening content from {content}")
        return MimeBytes(content.read_bytes(), mime_type=mimetypes.guess_type(content)[0])
    # If content is a PIL Image => convert to bytes
    if is_pillow_available():
        if isinstance(content, Image.Image):
            logger.debug("Converting PIL Image to bytes")
            format = content.format or "PNG"
            content.save(buffer, format=format)
            return MimeBytes(buffer.getvalue(), mime_type=f"image/{format.lower()}")
    # If nothing matched, raise error
        f"Unsupported content type: {type(content)}. "
        "Expected one of: bytes, bytearray, BinaryIO, memoryview, Path, str (URL or file path), or PIL.Image.Image."
def _b64_encode(content: ContentT) -> str:
    """Encode a raw file (image, audio) into base64. Can be bytes, an opened file, a path or a URL."""
    raw_bytes = _open_as_mime_bytes(content)
    return base64.b64encode(raw_bytes).decode()
def _as_url(content: ContentT, default_mime_type: str) -> str:
    if isinstance(content, str) and content.startswith(("http://", "https://", "data:")):
    # Convert content to bytes
    # Get MIME type
    mime_type = raw_bytes.mime_type or default_mime_type
    # Encode content to base64
    encoded_data = base64.b64encode(raw_bytes).decode()
    # Build data URL
    return f"data:{mime_type};base64,{encoded_data}"
def _b64_to_image(encoded_image: str) -> "Image":
    """Parse a base64-encoded string into a PIL Image."""
    Image = _import_pil_image()
    return Image.open(io.BytesIO(base64.b64decode(encoded_image)))
def _bytes_to_list(content: bytes) -> list:
    """Parse bytes from a Response object into a Python list.
    Expects the response body to be JSON-encoded data.
    NOTE: This is exactly the same implementation as `_bytes_to_dict` and will not complain if the returned data is a
    dictionary. The only advantage of having both is to help the user (and mypy) understand what kind of data to expect.
    return json.loads(content.decode())
def _bytes_to_dict(content: bytes) -> dict:
    """Parse bytes from a Response object into a Python dictionary.
    NOTE: This is exactly the same implementation as `_bytes_to_list` and will not complain if the returned data is a
    list. The only advantage of having both is to help the user (and mypy) understand what kind of data to expect.
def _bytes_to_image(content: bytes) -> "Image":
    """Parse bytes from a Response object into a PIL Image.
    Expects the response body to be raw bytes. To deal with b64 encoded images, use `_b64_to_image` instead.
    return Image.open(io.BytesIO(content))
def _as_dict(response: Union[bytes, dict]) -> dict:
    return json.loads(response) if isinstance(response, bytes) else response
## STREAMING UTILS
def _stream_text_generation_response(
    output_lines: Iterable[str], details: bool
) -> Union[Iterable[str], Iterable[TextGenerationStreamOutput]]:
    """Used in `InferenceClient.text_generation`."""
    # Parse ServerSentEvents
    for line in output_lines:
            output = _format_text_generation_stream_output(line, details)
async def _async_stream_text_generation_response(
    output_lines: AsyncIterable[str], details: bool
) -> Union[AsyncIterable[str], AsyncIterable[TextGenerationStreamOutput]]:
    """Used in `AsyncInferenceClient.text_generation`."""
    async for line in output_lines:
def _format_text_generation_stream_output(
    line: str, details: bool
) -> Optional[Union[str, TextGenerationStreamOutput]]:
    if not line.startswith("data:"):
        return None  # empty line
    if line.strip() == "data: [DONE]":
        raise StopIteration("[DONE] signal received.")
    # Decode payload
    payload = line.lstrip("data:").rstrip("/n")
    json_payload = json.loads(payload)
    # Either an error as being returned
    if json_payload.get("error") is not None:
        raise _parse_text_generation_error(json_payload["error"], json_payload.get("error_type"))
    # Or parse token payload
    output = TextGenerationStreamOutput.parse_obj_as_instance(json_payload)
    return output.token.text if not details else output
def _stream_chat_completion_response(
    lines: Iterable[str],
) -> Iterable[ChatCompletionStreamOutput]:
    """Used in `InferenceClient.chat_completion` if model is served with TGI."""
            output = _format_chat_completion_stream_output(line)
async def _async_stream_chat_completion_response(
    lines: AsyncIterable[str],
) -> AsyncIterable[ChatCompletionStreamOutput]:
    """Used in `AsyncInferenceClient.chat_completion`."""
    async for line in lines:
def _format_chat_completion_stream_output(
    line: str,
) -> Optional[ChatCompletionStreamOutput]:
    json_payload = json.loads(line.lstrip("data:").strip())
    return ChatCompletionStreamOutput.parse_obj_as_instance(json_payload)
async def _async_yield_from(client: httpx.AsyncClient, response: httpx.Response) -> AsyncIterable[str]:
    async for line in response.aiter_lines():
        yield line.strip()
# "TGI servers" are servers running with the `text-generation-inference` backend.
# This backend is the go-to solution to run large language models at scale. However,
# for some smaller models (e.g. "gpt2") the default `transformers` + `api-inference`
# solution is still in use.
# Both approaches have very similar APIs, but not exactly the same. What we do first in
# the `text_generation` method is to assume the model is served via TGI. If we realize
# it's not the case (i.e. we receive an HTTP 400 Bad Request), we fall back to the
# default API with a warning message. When that's the case, We remember the unsupported
# attributes for this model in the `_UNSUPPORTED_TEXT_GENERATION_KWARGS` global variable.
# In addition, TGI servers have a built-in API route for chat-completion, which is not
# available on the default API. We use this route to provide a more consistent behavior
# when available.
# For more details, see https://github.com/huggingface/text-generation-inference and
# https://huggingface.co/docs/api-inference/detailed_parameters#text-generation-task.
_UNSUPPORTED_TEXT_GENERATION_KWARGS: dict[Optional[str], list[str]] = {}
def _set_unsupported_text_generation_kwargs(model: Optional[str], unsupported_kwargs: list[str]) -> None:
    _UNSUPPORTED_TEXT_GENERATION_KWARGS.setdefault(model, []).extend(unsupported_kwargs)
def _get_unsupported_text_generation_kwargs(model: Optional[str]) -> list[str]:
    return _UNSUPPORTED_TEXT_GENERATION_KWARGS.get(model, [])
# Text-generation errors are parsed separately to handle as much as possible the errors returned by the text generation
# inference project (https://github.com/huggingface/text-generation-inference).
def raise_text_generation_error(http_error: HfHubHTTPError) -> NoReturn:
    Try to parse text-generation-inference error message and raise HTTPError in any case.
        error (`HTTPError`):
            The HTTPError that have been raised.
    # Try to parse a Text Generation Inference error
    if http_error.response is None:
        raise http_error
        # Hacky way to retrieve payload in case of aiohttp error
        payload = getattr(http_error, "response_error_payload", None) or http_error.response.json()
        error = payload.get("error")
        error_type = payload.get("error_type")
    except Exception:  # no payload
    # If error_type => more information than `hf_raise_for_status`
        exception = _parse_text_generation_error(error, error_type)
        raise exception from http_error
    # Otherwise, fallback to default error
def _parse_text_generation_error(error: Optional[str], error_type: Optional[str]) -> TextGenerationError:
    if error_type == "generation":
        return GenerationError(error)  # type: ignore
    if error_type == "incomplete_generation":
        return IncompleteGenerationError(error)  # type: ignore
    if error_type == "overloaded":
        return OverloadedError(error)  # type: ignore
    if error_type == "validation":
        return ValidationError(error)  # type: ignore
    return UnknownError(error)  # type: ignore
from typing import Any, Optional, Union, overload
from huggingface_hub.hf_api import InferenceProviderMapping
from huggingface_hub.inference._common import MimeBytes, RequestParameters
from huggingface_hub.inference._generated.types.chat_completion import ChatCompletionInputMessage
from huggingface_hub.utils import build_hf_headers, get_token, logging
# Dev purposes only.
# If you want to try to run inference for a new model locally before it's registered on huggingface.co
# for a given Inference Provider, you can add it to the following dictionary.
HARDCODED_MODEL_INFERENCE_MAPPING: dict[str, dict[str, InferenceProviderMapping]] = {
    # "HF model ID" => InferenceProviderMapping object initialized with "Model ID on Inference Provider's side"
    # "Qwen/Qwen2.5-Coder-32B-Instruct": InferenceProviderMapping(hf_model_id="Qwen/Qwen2.5-Coder-32B-Instruct",
    #                                    provider_id="Qwen2.5-Coder-32B-Instruct",
    #                                    task="conversational",
    #                                    status="live")
    "cerebras": {},
    "cohere": {},
    "clarifai": {},
    "fal-ai": {},
    "fireworks-ai": {},
    "groq": {},
    "hf-inference": {},
    "hyperbolic": {},
    "nebius": {},
    "nscale": {},
    "ovhcloud": {},
    "replicate": {},
    "sambanova": {},
    "scaleway": {},
    "together": {},
    "wavespeed": {},
    "zai-org": {},
def filter_none(obj: dict[str, Any]) -> dict[str, Any]: ...
def filter_none(obj: list[Any]) -> list[Any]: ...
def filter_none(obj: Union[dict[str, Any], list[Any]]) -> Union[dict[str, Any], list[Any]]:
        cleaned: dict[str, Any] = {}
        for k, v in obj.items():
            if isinstance(v, (dict, list)):
                v = filter_none(v)
            cleaned[k] = v
        return cleaned
    if isinstance(obj, list):
        return [filter_none(v) if isinstance(v, (dict, list)) else v for v in obj]
    raise ValueError(f"Expected dict or list, got {type(obj)}")
class TaskProviderHelper:
    """Base class for task-specific provider helpers."""
    def __init__(self, provider: str, base_url: str, task: str) -> None:
        self.provider = provider
        self.task = task
    def prepare_request(
        inputs: Any,
        parameters: dict[str, Any],
        headers: dict,
        model: Optional[str],
        api_key: Optional[str],
        extra_payload: Optional[dict[str, Any]] = None,
    ) -> RequestParameters:
        Prepare the request to be sent to the provider.
        Each step (api_key, model, headers, url, payload) can be customized in subclasses.
        # api_key from user, or local token, or raise error
        api_key = self._prepare_api_key(api_key)
        # mapped model from HF model ID
        provider_mapping_info = self._prepare_mapping_info(model)
        # default HF headers + user headers (to customize in subclasses)
        headers = self._prepare_headers(headers, api_key)
        # routed URL if HF token, or direct URL (to customize in '_prepare_route' in subclasses)
        url = self._prepare_url(api_key, provider_mapping_info.provider_id)
        # prepare payload (to customize in subclasses)
        payload = self._prepare_payload_as_dict(inputs, parameters, provider_mapping_info=provider_mapping_info)
            payload = recursive_merge(payload, filter_none(extra_payload or {}))
        # body data (to customize in subclasses)
        data = self._prepare_payload_as_bytes(inputs, parameters, provider_mapping_info, extra_payload)
        # check if both payload and data are set and return
        if payload is not None and data is not None:
            raise ValueError("Both payload and data cannot be set in the same request.")
        if payload is None and data is None:
            raise ValueError("Either payload or data must be set in the request.")
        # normalize headers to lowercase and add content-type if not present
        normalized_headers = self._normalize_headers(headers, payload, data)
        return RequestParameters(
            task=self.task,
            model=provider_mapping_info.provider_id,
            json=payload,
            headers=normalized_headers,
    def get_response(
        response: Union[bytes, dict],
        request_params: Optional[RequestParameters] = None,
        Return the response in the expected format.
        Override this method in subclasses for customized response handling."""
    def _prepare_api_key(self, api_key: Optional[str]) -> str:
        """Return the API key to use for the request.
        Usually not overwritten in subclasses."""
            api_key = get_token()
                f"You must provide an api_key to work with {self.provider} API or log in with `hf auth login`."
    def _prepare_mapping_info(self, model: Optional[str]) -> InferenceProviderMapping:
        """Return the mapped model ID to use for the request.
            raise ValueError(f"Please provide an HF model ID supported by {self.provider}.")
        # hardcoded mapping for local testing
        if HARDCODED_MODEL_INFERENCE_MAPPING.get(self.provider, {}).get(model):
            return HARDCODED_MODEL_INFERENCE_MAPPING[self.provider][model]
        provider_mapping = None
        for mapping in _fetch_inference_provider_mapping(model):
            if mapping.provider == self.provider:
                provider_mapping = mapping
        if provider_mapping is None:
            raise ValueError(f"Model {model} is not supported by provider {self.provider}.")
        if provider_mapping.task != self.task:
                f"Model {model} is not supported for task {self.task} and provider {self.provider}. "
                f"Supported task: {provider_mapping.task}."
        if provider_mapping.status == "staging":
                f"Model {model} is in staging mode for provider {self.provider}. Meant for test purposes only."
        if provider_mapping.status == "error":
                f"Our latest automated health check on model '{model}' for provider '{self.provider}' did not complete successfully.  "
                "Inference call might fail."
        return provider_mapping
    def _normalize_headers(
        self, headers: dict[str, Any], payload: Optional[dict[str, Any]], data: Optional[MimeBytes]
        """Normalize the headers to use for the request.
        Override this method in subclasses for customized headers.
        normalized_headers = {key.lower(): value for key, value in headers.items() if value is not None}
        if normalized_headers.get("content-type") is None:
            if data is not None and data.mime_type is not None:
                normalized_headers["content-type"] = data.mime_type
            elif payload is not None:
                normalized_headers["content-type"] = "application/json"
        return normalized_headers
    def _prepare_headers(self, headers: dict, api_key: str) -> dict[str, Any]:
        """Return the headers to use for the request.
        return {**build_hf_headers(token=api_key), **headers}
    def _prepare_url(self, api_key: str, mapped_model: str) -> str:
        """Return the URL to use for the request.
        base_url = self._prepare_base_url(api_key)
        route = self._prepare_route(mapped_model, api_key)
        return f"{base_url.rstrip('/')}/{route.lstrip('/')}"
    def _prepare_base_url(self, api_key: str) -> str:
        """Return the base URL to use for the request.
        # Route to the proxy if the api_key is a HF TOKEN
        if api_key.startswith("hf_"):
            logger.info(f"Calling '{self.provider}' provider through Hugging Face router.")
            return constants.INFERENCE_PROXY_TEMPLATE.format(provider=self.provider)
            logger.info(f"Calling '{self.provider}' provider directly.")
            return self.base_url
    def _prepare_route(self, mapped_model: str, api_key: str) -> str:
        """Return the route to use for the request.
        Override this method in subclasses for customized routes.
    def _prepare_payload_as_dict(
        self, inputs: Any, parameters: dict, provider_mapping_info: InferenceProviderMapping
    ) -> Optional[dict]:
        """Return the payload to use for the request, as a dict.
        Override this method in subclasses for customized payloads.
        Only one of `_prepare_payload_as_dict` and `_prepare_payload_as_bytes` should return a value.
    def _prepare_payload_as_bytes(
        parameters: dict,
        provider_mapping_info: InferenceProviderMapping,
        extra_payload: Optional[dict],
    ) -> Optional[MimeBytes]:
        """Return the body to use for the request, as bytes.
        Override this method in subclasses for customized body data.
class BaseConversationalTask(TaskProviderHelper):
    Base class for conversational (chat completion) tasks.
    The schema follows the OpenAI API format defined here: https://platform.openai.com/docs/api-reference/chat
    def __init__(self, provider: str, base_url: str):
        super().__init__(provider=provider, base_url=base_url, task="conversational")
        return "/v1/chat/completions"
        inputs: list[Union[dict, ChatCompletionInputMessage]],
        return filter_none({"messages": inputs, **parameters, "model": provider_mapping_info.provider_id})
class AutoRouterConversationalTask(BaseConversationalTask):
    Auto-router for conversational tasks.
    We let the Hugging Face router select the best provider for the model, based on availability and user preferences.
    This is a special case since the selection is done server-side (avoid 1 API call to fetch provider mapping).
        super().__init__(provider="auto", base_url="https://router.huggingface.co")
        if not api_key.startswith("hf_"):
            raise ValueError("Cannot select auto-router when using non-Hugging Face API key.")
            return self.base_url  # No `/auto` suffix in the URL
        In auto-router, we don't need to fetch provider mapping info.
        We just return a dummy mapping info with provider_id set to the HF model ID.
            raise ValueError("Please provide an HF model ID.")
        return InferenceProviderMapping(
            provider="auto",
            hf_model_id=model,
            providerId=model,
            status="live",
class BaseTextGenerationTask(TaskProviderHelper):
    Base class for text-generation (completion) tasks.
    The schema follows the OpenAI API format defined here: https://platform.openai.com/docs/api-reference/completions
        super().__init__(provider=provider, base_url=base_url, task="text-generation")
        return "/v1/completions"
        return filter_none({"prompt": inputs, **parameters, "model": provider_mapping_info.provider_id})
def _fetch_inference_provider_mapping(model: str) -> list["InferenceProviderMapping"]:
    Fetch provider mappings for a model from the Hub.
    from huggingface_hub.hf_api import HfApi
    info = HfApi().model_info(model, expand=["inferenceProviderMapping"])
    provider_mapping = info.inference_provider_mapping
        raise ValueError(f"No provider mapping found for model {model}")
def recursive_merge(dict1: dict, dict2: dict) -> dict:
        **dict1,
            key: recursive_merge(dict1[key], value)
            if (key in dict1 and isinstance(dict1[key], dict) and isinstance(value, dict))
            else value
            for key, value in dict2.items()
