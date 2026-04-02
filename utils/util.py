from lib.config import verbose_mode_print
ELECTRON_DIR = os.path.abspath(
  os.path.dirname(os.path.dirname(os.path.dirname(__file__)))
TS_NODE = os.path.join(ELECTRON_DIR, 'node_modules', '.bin', 'ts-node')
SRC_DIR = os.path.abspath(os.path.join(__file__, '..', '..', '..', '..'))
if sys.platform in ['win32', 'cygwin']:
  TS_NODE += '.cmd'
def scoped_cwd(path):
  cwd = os.getcwd()
  os.chdir(path)
    os.chdir(cwd)
def download(text, url, path):
  safe_mkdir(os.path.dirname(path))
  with open(path, 'wb') as local_file, urlopen(url) as web_file:
    print(f"Downloading {url} to {path}")
    info = web_file.info()
    if hasattr(info, 'getheader'):
      file_size = int(info.getheaders("Content-Length")[0])
      file_size = int(info.get("Content-Length")[0])
    downloaded_size = 0
    block_size = 4096
    ci = os.environ.get('CI') is not None
      buf = web_file.read(block_size)
      if not buf:
      downloaded_size += len(buf)
      local_file.write(buf)
      if not ci:
        percent = downloaded_size * 100. / file_size
        status = f"\r{text}  {downloaded_size:10d}  [{percent:3.1f}%]"
        print(status, end=' ')
    if ci:
      print(f"{text} done.")
      print()
def make_zip(zip_file_path, files, dirs):
  safe_unlink(zip_file_path)
    allfiles = files + dirs
    execute(['zip', '-r', '-y', '-9', zip_file_path] + allfiles)
    with zipfile.ZipFile(zip_file_path, "w",
                         zipfile.ZIP_DEFLATED,
                         allowZip64=True) as zip_file:
        zip_file.write(filename, filename, compress_type=zipfile.ZIP_DEFLATED, compresslevel=9)
      for dirname in dirs:
        for root, _, filenames in os.walk(dirname):
          for f in filenames:
            zip_file.write(os.path.join(root, f), compress_type=zipfile.ZIP_DEFLATED, compresslevel=9)
      zip_file.close()
def make_tar_xz(tar_file_path, files, dirs):
  safe_unlink(tar_file_path)
  execute(['tar', '-cJf', tar_file_path] + allfiles)
def rm_rf(path):
    shutil.rmtree(path)
def safe_unlink(path):
    os.unlink(path)
def safe_mkdir(path):
    if e.errno != errno.EEXIST:
def execute(argv, env=None, cwd=None):
  if env is None:
    env = os.environ
  verbose_mode_print(' '.join(argv))
    output = subprocess.check_output(argv, stderr=subprocess.STDOUT,
                                     env=env, cwd=cwd)
    verbose_mode_print(output.decode('utf-8').strip())
def get_electron_branding():
  SOURCE_ROOT = os.path.abspath(os.path.join(__file__, '..', '..', '..'))
  branding_file_path = os.path.join(
    SOURCE_ROOT, 'shell', 'app', 'BRANDING.json')
  with open(branding_file_path, encoding='utf-8') as file_in:
    return json.load(file_in)
cached_electron_version = None
def get_electron_version():
  global cached_electron_version
  if cached_electron_version is None:
    cached_electron_version = str.strip(execute([
      'node',
      'require("./script/lib/get-version").getElectronVersion()'
    ], cwd=ELECTRON_DIR).decode())
  return cached_electron_version
def store_artifact(prefix, key_prefix, files):
  # Azure Storage
  azput(prefix, key_prefix, files)
def azput(prefix, key_prefix, files):
  env = os.environ.copy()
  output = execute([
    os.path.join(os.path.dirname(__file__), 'azput.js'),
    '--prefix', prefix,
    '--key_prefix', key_prefix,
  ] + files, env)
def get_out_dir():
  out_dir = 'Default'
  override = os.environ.get('ELECTRON_OUT_DIR')
  if override is not None:
    out_dir = override
  return os.path.join(SRC_DIR, 'out', out_dir)
# NOTE: This path is not created by gn, it is used as a scratch zone by our
#       upload scripts
def get_dist_dir():
  return os.path.join(get_out_dir(), 'gen', 'electron_dist')
def get_electron_exec():
  out_dir = get_out_dir()
    return f'{out_dir}/Electron.app/Contents/MacOS/Electron'
    return f'{out_dir}/electron.exe'
    return f'{out_dir}/electron'
  raise Exception(
      f"get_electron_exec: unexpected platform '{sys.platform}'")
def get_buildtools_executable(name):
  buildtools = os.path.realpath(os.path.join(ELECTRON_DIR, '..', 'buildtools'))
    chromium_platform = 'mac_arm64' if platform.machine() == 'arm64' else 'mac'
  elif sys.platform in ['win32', 'cygwin']:
    chromium_platform = 'win'
  elif sys.platform in ['linux', 'linux2']:
    chromium_platform = 'linux64'
    raise Exception(f"Unsupported platform: {sys.platform}")
  if name == 'clang-format':
    chromium_platform += '-format'
  path = os.path.join(buildtools, chromium_platform, name)
    path += '.exe'
def get_depot_tools_executable(name):
  buildtools = os.path.realpath(
    os.path.join(ELECTRON_DIR, '..', 'third_party', 'depot_tools'))
  path = os.path.join(buildtools, name)
    path += '.bat'
import dis
def iterate_instructions(code_object):
    """Delivers the byte-code instructions as a continuous stream.
    Yields `dis.Instruction`. After each code-block (`co_code`), `None` is
    yielded to mark the end of the block and to interrupt the steam.
    # The arg extension the EXTENDED_ARG opcode represents is automatically handled by get_instructions() but the
    # instruction is left in. Get rid of it to make subsequent parsing easier/safer.
    yield from (i for i in dis.get_instructions(code_object) if i.opname != "EXTENDED_ARG")
    # For each constant in this code object that is itself a code object,
    # parse this constant in the same manner.
    for constant in code_object.co_consts:
        if inspect.iscode(constant):
            yield from iterate_instructions(constant)
# util.py
_bslash = chr(92)
class __config_flags:
    """Internal class for defining compatibility and debugging flags"""
    _all_names: List[str] = []
    _fixed_names: List[str] = []
    _type_desc = "configuration"
    def _set(cls, dname, value):
        if dname in cls._fixed_names:
                "{}.{} {} is {} and cannot be overridden".format(
                    dname,
                    cls._type_desc,
                    str(getattr(cls, dname)).upper(),
        if dname in cls._all_names:
            setattr(cls, dname, value)
            raise ValueError("no such {} {!r}".format(cls._type_desc, dname))
    enable = classmethod(lambda cls, name: cls._set(name, True))
    disable = classmethod(lambda cls, name: cls._set(name, False))
@lru_cache(maxsize=128)
def col(loc: int, strg: str) -> int:
    Returns current column within a string, counting newlines as line separators.
    The first column is number 1.
    Note: the default parsing behavior is to expand tabs in the input string
    before starting the parsing process.  See
    :class:`ParserElement.parseString` for more
    information on parsing strings containing ``<TAB>`` s, and suggested
    methods to maintain a consistent view of the parsed string, the parse
    location, and line and column positions within the parsed string.
    s = strg
    return 1 if 0 < loc < len(s) and s[loc - 1] == "\n" else loc - s.rfind("\n", 0, loc)
def lineno(loc: int, strg: str) -> int:
    """Returns current line number within a string, counting newlines as line separators.
    The first line is number 1.
    Note - the default parsing behavior is to expand tabs in the input string
    before starting the parsing process.  See :class:`ParserElement.parseString`
    for more information on parsing strings containing ``<TAB>`` s, and
    suggested methods to maintain a consistent view of the parsed string, the
    parse location, and line and column positions within the parsed string.
    return strg.count("\n", 0, loc) + 1
def line(loc: int, strg: str) -> str:
    Returns the line of text containing loc within a string, counting newlines as line separators.
    last_cr = strg.rfind("\n", 0, loc)
    next_cr = strg.find("\n", loc)
    return strg[last_cr + 1 : next_cr] if next_cr >= 0 else strg[last_cr + 1 :]
class _UnboundedCache:
        cache_get = cache.get
        self.not_in_cache = not_in_cache = object()
        def get(_, key):
            return cache_get(key, not_in_cache)
        def set_(_, key, value):
            cache[key] = value
        def clear(_):
        self.size = None
        self.get = types.MethodType(get, self)
        self.set = types.MethodType(set_, self)
        self.clear = types.MethodType(clear, self)
class _FifoCache:
    def __init__(self, size):
        cache = collections.OrderedDict()
            while len(cache) > size:
                cache.popitem(last=False)
class LRUMemo:
    A memoizing mapping that retains `capacity` deleted items
    The memo tracks retained items by their access order; once `capacity` items
    are retained, the least recently used item is discarded.
    def __init__(self, capacity):
        self._capacity = capacity
        self._active = {}
        self._memory = collections.OrderedDict()
            return self._active[key]
            self._memory.move_to_end(key)
            return self._memory[key]
        self._memory.pop(key, None)
        self._active[key] = value
            value = self._active.pop(key)
            while len(self._memory) >= self._capacity:
                self._memory.popitem(last=False)
            self._memory[key] = value
        self._active.clear()
        self._memory.clear()
class UnboundedMemo(dict):
    A memoizing mapping that retains all deleted items
def _escape_regex_range_chars(s: str) -> str:
    # escape these chars: ^-[]
    for c in r"\^-[]":
        s = s.replace(c, _bslash + c)
    s = s.replace("\n", r"\n")
    s = s.replace("\t", r"\t")
    return str(s)
def _collapse_string_to_ranges(
    s: Union[str, Iterable[str]], re_escape: bool = True
    def is_consecutive(c):
        c_int = ord(c)
        is_consecutive.prev, prev = c_int, is_consecutive.prev
        if c_int - prev > 1:
            is_consecutive.value = next(is_consecutive.counter)
        return is_consecutive.value
    is_consecutive.prev = 0
    is_consecutive.counter = itertools.count()
    is_consecutive.value = -1
    def escape_re_range_char(c):
        return "\\" + c if c in r"\^-][" else c
    def no_escape_re_range_char(c):
    if not re_escape:
        escape_re_range_char = no_escape_re_range_char
    s = "".join(sorted(set(s)))
    if len(s) > 3:
        for _, chars in itertools.groupby(s, key=is_consecutive):
            first = last = next(chars)
            last = collections.deque(
                itertools.chain(iter([last]), chars), maxlen=1
            ).pop()
            if first == last:
                ret.append(escape_re_range_char(first))
                sep = "" if ord(last) == ord(first) + 1 else "-"
                        escape_re_range_char(first), sep, escape_re_range_char(last)
        ret = [escape_re_range_char(c) for c in s]
    return "".join(ret)
def _flatten(ll: list) -> list:
    for i in ll:
        if isinstance(i, list):
            ret.extend(_flatten(i))
            ret.append(i)
"""distutils.util
Miscellaneous utility functions -- anything that doesn't fit into
one of the other *util.py modules.
import sysconfig
from distutils.errors import DistutilsPlatformError
from distutils.dep_util import newer
from distutils.spawn import spawn
from distutils.errors import DistutilsByteCompileError
def get_host_platform():
    Return a string that identifies the current platform. Use this
    function to distinguish platform-specific build directories and
    platform-specific built distributions.
    # This function initially exposed platforms as defined in Python 3.9
    # even with older Python versions when distutils was split out.
    # Now it delegates to stdlib sysconfig, but maintains compatibility.
    if sys.version_info < (3, 8):
            if '(arm)' in sys.version.lower():
                return 'win-arm32'
            if '(arm64)' in sys.version.lower():
                return 'win-arm64'
        if os.name == "posix" and hasattr(os, 'uname'):
            osname, host, release, version, machine = os.uname()
            if osname[:3] == "aix":
                from .py38compat import aix_platform
                return aix_platform(osname, version, release)
    return sysconfig.get_platform()
def get_platform():
        TARGET_TO_PLAT = {
            'x86': 'win32',
            'x64': 'win-amd64',
            'arm': 'win-arm32',
            'arm64': 'win-arm64',
        target = os.environ.get('VSCMD_ARG_TGT_ARCH')
        return TARGET_TO_PLAT.get(target) or get_host_platform()
    return get_host_platform()
    _syscfg_macosx_ver = None  # cache the version pulled from sysconfig
MACOSX_VERSION_VAR = 'MACOSX_DEPLOYMENT_TARGET'
def _clear_cached_macosx_ver():
    """For testing only. Do not call."""
    global _syscfg_macosx_ver
    _syscfg_macosx_ver = None
def get_macosx_target_ver_from_syscfg():
    """Get the version of macOS latched in the Python interpreter configuration.
    Returns the version as a string or None if can't obtain one. Cached."""
    if _syscfg_macosx_ver is None:
        from distutils import sysconfig
        ver = sysconfig.get_config_var(MACOSX_VERSION_VAR) or ''
        if ver:
            _syscfg_macosx_ver = ver
    return _syscfg_macosx_ver
def get_macosx_target_ver():
    """Return the version of macOS for which we are building.
    The target version defaults to the version in sysconfig latched at time
    the Python interpreter was built, unless overridden by an environment
    variable. If neither source has a value, then None is returned"""
    syscfg_ver = get_macosx_target_ver_from_syscfg()
    env_ver = os.environ.get(MACOSX_VERSION_VAR)
    if env_ver:
        # Validate overridden version against sysconfig version, if have both.
        # Ensure that the deployment target of the build process is not less
        # than 10.3 if the interpreter was built for 10.3 or later.  This
        # ensures extension modules are built with correct compatibility
        # values, specifically LDSHARED which can use
        # '-undefined dynamic_lookup' which only works on >= 10.3.
            syscfg_ver
            and split_version(syscfg_ver) >= [10, 3]
            and split_version(env_ver) < [10, 3]
            my_msg = (
                '$' + MACOSX_VERSION_VAR + ' mismatch: '
                'now "%s" but "%s" during configure; '
                'must use 10.3 or later' % (env_ver, syscfg_ver)
            raise DistutilsPlatformError(my_msg)
        return env_ver
    return syscfg_ver
def split_version(s):
    """Convert a dot-separated string into a list of numbers for comparisons"""
    return [int(n) for n in s.split('.')]
    """Return 'pathname' as a name that will work on the native filesystem,
    i.e. split it on '/' and put it back together again using the current
    directory separator.  Needed because filenames in the setup script are
    always supplied in Unix style, and have to be converted to the local
    convention before we can actually use them in the filesystem.  Raises
    ValueError on non-Unix-ish systems if 'pathname' either starts or
    ends with a slash.
        return pathname
    if not pathname:
    if pathname[0] == '/':
        raise ValueError("path '%s' cannot be absolute" % pathname)
    if pathname[-1] == '/':
        raise ValueError("path '%s' cannot end with '/'" % pathname)
    paths = pathname.split('/')
    while '.' in paths:
        paths.remove('.')
        return os.curdir
    return os.path.join(*paths)
# convert_path ()
def change_root(new_root, pathname):
    """Return 'pathname' with 'new_root' prepended.  If 'pathname' is
    relative, this is equivalent to "os.path.join(new_root,pathname)".
    Otherwise, it requires making 'pathname' relative and then joining the
    two, which is tricky on DOS/Windows and Mac OS.
        if not os.path.isabs(pathname):
            return os.path.join(new_root, pathname)
            return os.path.join(new_root, pathname[1:])
        (drive, path) = os.path.splitdrive(pathname)
        if path[0] == '\\':
            path = path[1:]
        return os.path.join(new_root, path)
    raise DistutilsPlatformError(f"nothing known about platform '{os.name}'")
_environ_checked = 0
def check_environ():
    """Ensure that 'os.environ' has all the environment variables we
    guarantee that users can use in config files, command-line options,
    etc.  Currently this includes:
      HOME - user's home directory (Unix only)
      PLAT - description of the current platform, including hardware
             and OS (see 'get_platform()')
    global _environ_checked
    if _environ_checked:
    if os.name == 'posix' and 'HOME' not in os.environ:
            os.environ['HOME'] = pwd.getpwuid(os.getuid())[5]
        except (ImportError, KeyError):
            # bpo-10496: if the current user identifier doesn't exist in the
            # password database, do nothing
    if 'PLAT' not in os.environ:
        os.environ['PLAT'] = get_platform()
    _environ_checked = 1
def subst_vars(s, local_vars):
    Perform variable substitution on 'string'.
    Variables are indicated by format-style braces ("{var}").
    Variable is substituted by the value found in the 'local_vars'
    dictionary or in 'os.environ' if it's not in 'local_vars'.
    'os.environ' is first checked/augmented to guarantee that it contains
    certain values: see 'check_environ()'.  Raise ValueError for any
    variables not found in either 'local_vars' or 'os.environ'.
    check_environ()
    lookup = dict(os.environ)
    lookup.update((name, str(value)) for name, value in local_vars.items())
        return _subst_compat(s).format_map(lookup)
    except KeyError as var:
        raise ValueError(f"invalid variable {var}")
def _subst_compat(s):
    Replace shell/Perl-style variable substitution with
    format-style. For compatibility.
    def _subst(match):
        return f'{{{match.group(1)}}}'
    repl = re.sub(r'\$([a-zA-Z_][a-zA-Z_0-9]*)', _subst, s)
    if repl != s:
            "shell/Perl-style substitions are deprecated",
    return repl
def grok_environment_error(exc, prefix="error: "):
    # Function kept for backward compatibility.
    # Used to try clever things with EnvironmentErrors,
    # but nowadays str(exception) produces good messages.
    return prefix + str(exc)
# Needed by 'split_quoted()'
_wordchars_re = _squote_re = _dquote_re = None
def _init_regex():
    global _wordchars_re, _squote_re, _dquote_re
    _wordchars_re = re.compile(r'[^\\\'\"%s ]*' % string.whitespace)
    _squote_re = re.compile(r"'(?:[^'\\]|\\.)*'")
    _dquote_re = re.compile(r'"(?:[^"\\]|\\.)*"')
def split_quoted(s):
    """Split a string up according to Unix shell-like rules for quotes and
    backslashes.  In short: words are delimited by spaces, as long as those
    spaces are not escaped by a backslash, or inside a quoted string.
    Single and double quotes are equivalent, and the quote characters can
    be backslash-escaped.  The backslash is stripped from any two-character
    escape sequence, leaving only the escaped character.  The quote
    characters are stripped from any quoted string.  Returns a list of
    words.
    # This is a nice algorithm for splitting up a single string, since it
    # doesn't require character-by-character examination.  It was a little
    # bit of a brain-bender to get it working right, though...
    if _wordchars_re is None:
        _init_regex()
    words = []
    while s:
        m = _wordchars_re.match(s, pos)
        end = m.end()
        if end == len(s):
            words.append(s[:end])
        if s[end] in string.whitespace:
            # unescaped, unquoted whitespace: now
            # we definitely have a word delimiter
            s = s[end:].lstrip()
        elif s[end] == '\\':
            # preserve whatever is being escaped;
            # will become part of the current word
            s = s[:end] + s[end + 1 :]
            pos = end + 1
            if s[end] == "'":  # slurp singly-quoted string
                m = _squote_re.match(s, end)
            elif s[end] == '"':  # slurp doubly-quoted string
                m = _dquote_re.match(s, end)
                raise RuntimeError("this can't happen (bad char '%c')" % s[end])
                raise ValueError("bad string (mismatched %s quotes?)" % s[end])
            (beg, end) = m.span()
            s = s[:beg] + s[beg + 1 : end - 1] + s[end:]
            pos = m.end() - 2
        if pos >= len(s):
            words.append(s)
    return words
# split_quoted ()
def execute(func, args, msg=None, verbose=0, dry_run=0):
    """Perform some action that affects the outside world (eg.  by
    writing to the filesystem).  Such actions are special because they
    are disabled by the 'dry_run' flag.  This method takes care of all
    that bureaucracy for you; all you have to do is supply the
    function to call and an argument tuple for it (to embody the
    "external action" being performed), and an optional message to
    print.
        msg = "%s%r" % (func.__name__, args)
        if msg[-2:] == ',)':  # correct for singleton tuple
            msg = msg[0:-2] + ')'
    log.info(msg)
    if not dry_run:
        func(*args)
def strtobool(val):
    """Convert a string representation of truth to true (1) or false (0).
    True values are 'y', 'yes', 't', 'true', 'on', and '1'; false values
    are 'n', 'no', 'f', 'false', 'off', and '0'.  Raises ValueError if
    'val' is anything else.
    val = val.lower()
    if val in ('y', 'yes', 't', 'true', 'on', '1'):
    elif val in ('n', 'no', 'f', 'false', 'off', '0'):
        raise ValueError("invalid truth value %r" % (val,))
def byte_compile(
    py_files,
    force=0,
    base_dir=None,
    verbose=1,
    dry_run=0,
    direct=None,
    """Byte-compile a collection of Python source files to .pyc
    files in a __pycache__ subdirectory.  'py_files' is a list
    of files to compile; any files that don't end in ".py" are silently
    skipped.  'optimize' must be one of the following:
      0 - don't optimize
      1 - normal optimization (like "python -O")
      2 - extra optimization (like "python -OO")
    If 'force' is true, all files are recompiled regardless of
    timestamps.
    The source filename encoded in each bytecode file defaults to the
    filenames listed in 'py_files'; you can modify these with 'prefix' and
    'basedir'.  'prefix' is a string that will be stripped off of each
    source filename, and 'base_dir' is a directory name that will be
    prepended (after 'prefix' is stripped).  You can supply either or both
    (or neither) of 'prefix' and 'base_dir', as you wish.
    If 'dry_run' is true, doesn't actually do anything that would
    affect the filesystem.
    Byte-compilation is either done directly in this interpreter process
    with the standard py_compile module, or indirectly by writing a
    temporary script and executing it.  Normally, you should let
    'byte_compile()' figure out to use direct compilation or not (see
    the source for details).  The 'direct' flag is used by the script
    generated in indirect mode; unless you know what you're doing, leave
    it set to None.
    # nothing is done if sys.dont_write_bytecode is True
    if sys.dont_write_bytecode:
        raise DistutilsByteCompileError('byte-compiling is disabled.')
    # First, if the caller didn't force us into direct or indirect mode,
    # figure out which mode we should be in.  We take a conservative
    # approach: choose direct mode *only* if the current interpreter is
    # in debug mode and optimize is 0.  If we're not in debug mode (-O
    # or -OO), we don't know which level of optimization this
    # interpreter is running with, so we can't do direct
    # byte-compilation and be certain that it's the right thing.  Thus,
    # always compile indirectly if the current interpreter is in either
    # optimize mode, or if either optimization level was requested by
    # the caller.
    if direct is None:
        direct = __debug__ and optimize == 0
    # "Indirect" byte-compilation: write a temporary script and then
    # run it with the appropriate flags.
    if not direct:
            from tempfile import mkstemp
            (script_fd, script_name) = mkstemp(".py")
            from tempfile import mktemp
            (script_fd, script_name) = None, mktemp(".py")
        log.info("writing byte-compilation script '%s'", script_name)
            if script_fd is not None:
                script = os.fdopen(script_fd, "w")
                script = open(script_name, "w")
            with script:
                script.write(
from distutils.util import byte_compile
                # XXX would be nice to write absolute filenames, just for
                # safety's sake (script should be more robust in the face of
                # chdir'ing before running it).  But this requires abspath'ing
                # 'prefix' as well, and that breaks the hack in build_lib's
                # 'byte_compile()' method that carefully tacks on a trailing
                # slash (os.sep really) to make sure the prefix here is "just
                # right".  This whole prefix business is rather delicate -- the
                # problem is that it's really a directory, but I'm treating it
                # as a dumb string, so trailing slashes and so forth matter.
                script.write(",\n".join(map(repr, py_files)) + "]\n")
byte_compile(files, optimize=%r, force=%r,
             prefix=%r, base_dir=%r,
             verbose=%r, dry_run=0,
             direct=1)
                    % (optimize, force, prefix, base_dir, verbose)
        cmd = [sys.executable]
        cmd.extend(subprocess._optim_args_from_interpreter_flags())
        cmd.append(script_name)
        spawn(cmd, dry_run=dry_run)
        execute(os.remove, (script_name,), "removing %s" % script_name, dry_run=dry_run)
    # "Direct" byte-compilation: use the py_compile module to compile
    # right here, right now.  Note that the script generated in indirect
    # mode simply calls 'byte_compile()' in direct mode, a weird sort of
    # cross-process recursion.  Hey, it works!
        from py_compile import compile
        for file in py_files:
            if file[-3:] != ".py":
                # This lets us be lazy and not filter filenames in
                # the "install_lib" command.
            # Terminology from the py_compile module:
            #   cfile - byte-compiled file
            #   dfile - purported source filename (same as 'file' by default)
            if optimize >= 0:
                opt = '' if optimize == 0 else optimize
                cfile = importlib.util.cache_from_source(file, optimization=opt)
                cfile = importlib.util.cache_from_source(file)
            dfile = file
                if file[: len(prefix)] != prefix:
                        "invalid prefix: filename %r doesn't start with %r"
                        % (file, prefix)
                dfile = dfile[len(prefix) :]
            if base_dir:
                dfile = os.path.join(base_dir, dfile)
            cfile_base = os.path.basename(cfile)
            if direct:
                if force or newer(file, cfile):
                    log.info("byte-compiling %s to %s", file, cfile_base)
                        compile(file, cfile, dfile)
                    log.debug("skipping byte-compilation of %s to %s", file, cfile_base)
def rfc822_escape(header):
    """Return a version of the string escaped for inclusion in an
    RFC-822 header, by ensuring there are 8 spaces space after each newline.
    lines = header.split('\n')
    sep = '\n' + 8 * ' '
    return sep.join(lines)
# Copyright (C) 2012-2023 The Python Software Foundation.
from glob import iglob as std_iglob
import py_compile
    ssl = None
    import dummy_threading as threading
from . import DistlibException
from .compat import (string_types, text_type, shutil, raw_input, StringIO, cache_from_source, urlopen, urljoin, httplib,
                     xmlrpclib, HTTPHandler, BaseConfigurator, valid_ident, Container, configparser, URLError, ZipFile,
                     fsdecode, unquote, urlparse)
# Requirement parsing code as per PEP 508
IDENTIFIER = re.compile(r'^([\w\.-]+)\s*')
VERSION_IDENTIFIER = re.compile(r'^([\w\.*+-]+)\s*')
COMPARE_OP = re.compile(r'^(<=?|>=?|={2,3}|[~!]=)\s*')
MARKER_OP = re.compile(r'^((<=?)|(>=?)|={2,3}|[~!]=|in|not\s+in)\s*')
OR = re.compile(r'^or\b\s*')
AND = re.compile(r'^and\b\s*')
NON_SPACE = re.compile(r'(\S+)\s*')
STRING_CHUNK = re.compile(r'([\s\w\.{}()*+#:;,/?!~`@$%^&=|<>\[\]-]+)')
def parse_marker(marker_string):
    Parse a marker string and return a dictionary containing a marker expression.
    The dictionary will contain keys "op", "lhs" and "rhs" for non-terminals in
    the expression grammar, or strings. A string contained in quotes is to be
    interpreted as a literal string, and a string not contained in quotes is a
    variable (such as os_name).
    def marker_var(remaining):
        # either identifier, or literal string
        m = IDENTIFIER.match(remaining)
            result = m.groups()[0]
            remaining = remaining[m.end():]
        elif not remaining:
            raise SyntaxError('unexpected end of input')
            q = remaining[0]
            if q not in '\'"':
                raise SyntaxError('invalid expression: %s' % remaining)
            oq = '\'"'.replace(q, '')
            remaining = remaining[1:]
            parts = [q]
            while remaining:
                # either a string chunk, or oq, or q to terminate
                if remaining[0] == q:
                elif remaining[0] == oq:
                    parts.append(oq)
                    m = STRING_CHUNK.match(remaining)
                        raise SyntaxError('error in string literal: %s' % remaining)
                    parts.append(m.groups()[0])
                s = ''.join(parts)
                raise SyntaxError('unterminated string: %s' % s)
            parts.append(q)
            result = ''.join(parts)
            remaining = remaining[1:].lstrip()  # skip past closing quote
        return result, remaining
    def marker_expr(remaining):
        if remaining and remaining[0] == '(':
            result, remaining = marker(remaining[1:].lstrip())
            if remaining[0] != ')':
                raise SyntaxError('unterminated parenthesis: %s' % remaining)
            remaining = remaining[1:].lstrip()
            lhs, remaining = marker_var(remaining)
                m = MARKER_OP.match(remaining)
                op = m.groups()[0]
                rhs, remaining = marker_var(remaining)
                lhs = {'op': op, 'lhs': lhs, 'rhs': rhs}
            result = lhs
    def marker_and(remaining):
        lhs, remaining = marker_expr(remaining)
            m = AND.match(remaining)
            rhs, remaining = marker_expr(remaining)
            lhs = {'op': 'and', 'lhs': lhs, 'rhs': rhs}
        return lhs, remaining
    def marker(remaining):
        lhs, remaining = marker_and(remaining)
            m = OR.match(remaining)
            rhs, remaining = marker_and(remaining)
            lhs = {'op': 'or', 'lhs': lhs, 'rhs': rhs}
    return marker(marker_string)
def parse_requirement(req):
    Parse a requirement passed in as a string. Return a Container
    whose attributes contain the various parts of the requirement.
    remaining = req.strip()
    if not remaining or remaining.startswith('#'):
        raise SyntaxError('name expected: %s' % remaining)
    distname = m.groups()[0]
    extras = mark_expr = versions = uri = None
    if remaining and remaining[0] == '[':
        i = remaining.find(']', 1)
            raise SyntaxError('unterminated extra: %s' % remaining)
        s = remaining[1:i]
        remaining = remaining[i + 1:].lstrip()
        extras = []
            m = IDENTIFIER.match(s)
                raise SyntaxError('malformed extra: %s' % s)
            extras.append(m.groups()[0])
            s = s[m.end():]
            if s[0] != ',':
                raise SyntaxError('comma expected in extras: %s' % s)
            s = s[1:].lstrip()
            extras = None
    if remaining:
        if remaining[0] == '@':
            # it's a URI
            m = NON_SPACE.match(remaining)
                raise SyntaxError('invalid URI: %s' % remaining)
            uri = m.groups()[0]
            t = urlparse(uri)
            # there are issues with Python and URL parsing, so this test
            # is a bit crude. See bpo-20271, bpo-23505. Python doesn't
            # always parse invalid URLs correctly - it should raise
            # exceptions for malformed URLs
            if not (t.scheme and t.netloc):
                raise SyntaxError('Invalid URL: %s' % uri)
            remaining = remaining[m.end():].lstrip()
            def get_versions(ver_remaining):
                Return a list of operator, version tuples if any are
                specified, else None.
                m = COMPARE_OP.match(ver_remaining)
                versions = None
                    versions = []
                        ver_remaining = ver_remaining[m.end():]
                        m = VERSION_IDENTIFIER.match(ver_remaining)
                            raise SyntaxError('invalid version: %s' % ver_remaining)
                        v = m.groups()[0]
                        versions.append((op, v))
                        if not ver_remaining or ver_remaining[0] != ',':
                        ver_remaining = ver_remaining[1:].lstrip()
                        # Some packages have a trailing comma which would break things
                        # See issue #148
                        if not ver_remaining:
                            raise SyntaxError('invalid constraint: %s' % ver_remaining)
                    if not versions:
                return versions, ver_remaining
            if remaining[0] != '(':
                versions, remaining = get_versions(remaining)
                i = remaining.find(')', 1)
                # As a special diversion from PEP 508, allow a version number
                # a.b.c in parentheses as a synonym for ~= a.b.c (because this
                # is allowed in earlier PEPs)
                if COMPARE_OP.match(s):
                    versions, _ = get_versions(s)
                    m = VERSION_IDENTIFIER.match(s)
                        raise SyntaxError('invalid constraint: %s' % s)
                    s = s[m.end():].lstrip()
                    if s:
                    versions = [('~=', v)]
        if remaining[0] != ';':
            raise SyntaxError('invalid requirement: %s' % remaining)
        mark_expr, remaining = parse_marker(remaining)
    if remaining and remaining[0] != '#':
        raise SyntaxError('unexpected trailing data: %s' % remaining)
        rs = distname
        rs = '%s %s' % (distname, ', '.join(['%s %s' % con for con in versions]))
    return Container(name=distname, extras=extras, constraints=versions, marker=mark_expr, url=uri, requirement=rs)
def get_resources_dests(resources_root, rules):
    """Find destinations for resources files"""
    def get_rel_path(root, path):
        # normalizes and returns a lstripped-/-separated path
        root = root.replace(os.path.sep, '/')
        path = path.replace(os.path.sep, '/')
        assert path.startswith(root)
        return path[len(root):].lstrip('/')
    destinations = {}
    for base, suffix, dest in rules:
        prefix = os.path.join(resources_root, base)
        for abs_base in iglob(prefix):
            abs_glob = os.path.join(abs_base, suffix)
            for abs_path in iglob(abs_glob):
                resource_file = get_rel_path(resources_root, abs_path)
                if dest is None:  # remove the entry if it was here
                    destinations.pop(resource_file, None)
                    rel_path = get_rel_path(abs_base, abs_path)
                    rel_dest = dest.replace(os.path.sep, '/').rstrip('/')
                    destinations[resource_file] = rel_dest + '/' + rel_path
    return destinations
def in_venv():
    if hasattr(sys, 'real_prefix'):
        # virtualenv venvs
        result = True
        # PEP 405 venvs
        result = sys.prefix != getattr(sys, 'base_prefix', sys.prefix)
def get_executable():
    # The __PYVENV_LAUNCHER__ dance is apparently no longer needed, as
    # changes to the stub launcher mean that sys.executable always points
    # to the stub on OS X
    #    if sys.platform == 'darwin' and ('__PYVENV_LAUNCHER__'
    #                                     in os.environ):
    #        result =  os.environ['__PYVENV_LAUNCHER__']
    #    else:
    #        result = sys.executable
    #    return result
    # Avoid normcasing: see issue #143
    # result = os.path.normcase(sys.executable)
    result = sys.executable
    if not isinstance(result, text_type):
        result = fsdecode(result)
def proceed(prompt, allowed_chars, error_prompt=None, default=None):
    p = prompt
        s = raw_input(p)
        if not s and default:
            s = default
            c = s[0].lower()
            if c in allowed_chars:
            if error_prompt:
                p = '%c: %s\n%s' % (c, error_prompt, prompt)
def extract_by_key(d, keys):
    if isinstance(keys, string_types):
        keys = keys.split()
        if key in d:
            result[key] = d[key]
def read_exports(stream):
        # needs to be a text stream
        stream = codecs.getreader('utf-8')(stream)
    # Try to load as JSON, falling back on legacy format
    data = stream.read()
    stream = StringIO(data)
        jdata = json.load(stream)
        result = jdata['extensions']['python.exports']['exports']
        for group, entries in result.items():
            for k, v in entries.items():
                s = '%s = %s' % (k, v)
                entry = get_export_entry(s)
                assert entry is not None
                entries[k] = entry
        stream.seek(0, 0)
    def read_stream(cp, stream):
        if hasattr(cp, 'read_file'):
            cp.read_file(stream)
            cp.readfp(stream)
    cp = configparser.ConfigParser()
        read_stream(cp, stream)
    except configparser.MissingSectionHeaderError:
        data = textwrap.dedent(data)
    for key in cp.sections():
        result[key] = entries = {}
        for name, value in cp.items(key):
            s = '%s = %s' % (name, value)
            # entry.dist = self
            entries[name] = entry
def write_exports(exports, stream):
        stream = codecs.getwriter('utf-8')(stream)
    for k, v in exports.items():
        # TODO check k, v for valid values
        cp.add_section(k)
        for entry in v.values():
            if entry.suffix is None:
                s = entry.prefix
                s = '%s:%s' % (entry.prefix, entry.suffix)
            if entry.flags:
                s = '%s [%s]' % (s, ', '.join(entry.flags))
            cp.set(k, entry.name, s)
    cp.write(stream)
def tempdir():
    td = tempfile.mkdtemp()
        yield td
        shutil.rmtree(td)
def chdir(d):
        os.chdir(d)
def socket_timeout(seconds=15):
    cto = socket.getdefaulttimeout()
        socket.setdefaulttimeout(seconds)
        socket.setdefaulttimeout(cto)
class cached_property(object):
    def __init__(self, func):
        # for attr in ('__name__', '__module__', '__doc__'):
        #     setattr(self, attr, getattr(func, attr, None))
    def __get__(self, obj, cls=None):
        value = self.func(obj)
        object.__setattr__(obj, self.func.__name__, value)
        # obj.__dict__[self.func.__name__] = value = self.func(obj)
    """Return 'pathname' as a name that will work on the native filesystem.
    The path is split on '/' and put back together again using the current
    while os.curdir in paths:
        paths.remove(os.curdir)
class FileOperator(object):
    def __init__(self, dry_run=False):
        self.dry_run = dry_run
        self.ensured = set()
        self._init_record()
    def _init_record(self):
        self.record = False
        self.files_written = set()
        self.dirs_created = set()
    def record_as_written(self, path):
        if self.record:
            self.files_written.add(path)
    def newer(self, source, target):
        """Tell if the target is newer than the source.
        Returns true if 'source' exists and is more recently modified than
        'target', or if 'source' exists and 'target' doesn't.
        Returns false if both exist and 'target' is the same age or younger
        than 'source'. Raise PackagingFileError if 'source' does not exist.
        Note that this test is not very accurate: files created in the same
        second will have the same "age".
        if not os.path.exists(source):
            raise DistlibException("file '%r' does not exist" % os.path.abspath(source))
        if not os.path.exists(target):
        return os.stat(source).st_mtime > os.stat(target).st_mtime
    def copy_file(self, infile, outfile, check=True):
        """Copy a file respecting dry-run and force flags.
        self.ensure_dir(os.path.dirname(outfile))
        logger.info('Copying %s to %s', infile, outfile)
        if not self.dry_run:
            if check:
                if os.path.islink(outfile):
                    msg = '%s is a symlink' % outfile
                elif os.path.exists(outfile) and not os.path.isfile(outfile):
                    msg = '%s is a non-regular file' % outfile
                raise ValueError(msg + ' which would be overwritten')
            shutil.copyfile(infile, outfile)
        self.record_as_written(outfile)
    def copy_stream(self, instream, outfile, encoding=None):
        assert not os.path.isdir(outfile)
        logger.info('Copying stream %s to %s', instream, outfile)
            if encoding is None:
                outstream = open(outfile, 'wb')
                outstream = codecs.open(outfile, 'w', encoding=encoding)
                shutil.copyfileobj(instream, outstream)
                outstream.close()
    def write_binary_file(self, path, data):
        self.ensure_dir(os.path.dirname(path))
            if os.path.exists(path):
        self.record_as_written(path)
    def write_text_file(self, path, data, encoding):
        self.write_binary_file(path, data.encode(encoding))
    def set_mode(self, bits, mask, files):
        if os.name == 'posix' or (os.name == 'java' and os._name == 'posix'):
            # Set the executable bits (owner, group, and world) on
            # all the files specified.
                if self.dry_run:
                    logger.info("changing mode of %s", f)
                    mode = (os.stat(f).st_mode | bits) & mask
                    logger.info("changing mode of %s to %o", f, mode)
                    os.chmod(f, mode)
    set_executable_mode = lambda s, f: s.set_mode(0o555, 0o7777, f)
    def ensure_dir(self, path):
        path = os.path.abspath(path)
        if path not in self.ensured and not os.path.exists(path):
            self.ensured.add(path)
            d, f = os.path.split(path)
            self.ensure_dir(d)
            logger.info('Creating %s' % path)
                os.mkdir(path)
                self.dirs_created.add(path)
    def byte_compile(self, path, optimize=False, force=False, prefix=None, hashed_invalidation=False):
        dpath = cache_from_source(path, not optimize)
        logger.info('Byte-compiling %s to %s', path, dpath)
            if force or self.newer(path, dpath):
                if not prefix:
                    diagpath = None
                    assert path.startswith(prefix)
                    diagpath = path[len(prefix):]
            compile_kwargs = {}
            if hashed_invalidation and hasattr(py_compile, 'PycInvalidationMode'):
                if not isinstance(hashed_invalidation, py_compile.PycInvalidationMode):
                    hashed_invalidation = py_compile.PycInvalidationMode.CHECKED_HASH
                compile_kwargs['invalidation_mode'] = hashed_invalidation
            py_compile.compile(path, dpath, diagpath, True, **compile_kwargs)  # raise error
        self.record_as_written(dpath)
        return dpath
    def ensure_removed(self, path):
            if os.path.isdir(path) and not os.path.islink(path):
                logger.debug('Removing directory tree at %s', path)
                    if path in self.dirs_created:
                        self.dirs_created.remove(path)
                if os.path.islink(path):
                    s = 'link'
                    s = 'file'
                logger.debug('Removing %s %s', s, path)
                    if path in self.files_written:
                        self.files_written.remove(path)
    def is_writable(self, path):
        result = False
        while not result:
                result = os.access(path, os.W_OK)
            parent = os.path.dirname(path)
            if parent == path:
            path = parent
        Commit recorded changes, turn off recording, return
        changes.
        assert self.record
        result = self.files_written, self.dirs_created
            for f in list(self.files_written):
                if os.path.exists(f):
                    os.remove(f)
            # dirs should all be empty now, except perhaps for
            # __pycache__ subdirs
            # reverse so that subdirs appear before their parents
            dirs = sorted(self.dirs_created, reverse=True)
            for d in dirs:
                flist = os.listdir(d)
                if flist:
                    assert flist == ['__pycache__']
                    sd = os.path.join(d, flist[0])
                    os.rmdir(sd)
                os.rmdir(d)  # should fail if non-empty
def resolve(module_name, dotted_path):
    if module_name in sys.modules:
        mod = sys.modules[module_name]
        mod = __import__(module_name)
    if dotted_path is None:
        result = mod
        parts = dotted_path.split('.')
        result = getattr(mod, parts.pop(0))
            result = getattr(result, p)
class ExportEntry(object):
    def __init__(self, name, prefix, suffix, flags):
        self.suffix = suffix
        return resolve(self.prefix, self.suffix)
    def __repr__(self):  # pragma: no cover
        return '<ExportEntry %s = %s:%s %s>' % (self.name, self.prefix, self.suffix, self.flags)
        if not isinstance(other, ExportEntry):
            result = (self.name == other.name and self.prefix == other.prefix and self.suffix == other.suffix and
                      self.flags == other.flags)
    __hash__ = object.__hash__
ENTRY_RE = re.compile(
    r'''(?P<name>([^\[]\S*))
                      \s*=\s*(?P<callable>(\w+)([:\.]\w+)*)
                      \s*(\[\s*(?P<flags>[\w-]+(=\w+)?(,\s*\w+(=\w+)?)*)\s*\])?
                      ''', re.VERBOSE)
def get_export_entry(specification):
    m = ENTRY_RE.search(specification)
        result = None
        if '[' in specification or ']' in specification:
            raise DistlibException("Invalid specification "
                                   "'%s'" % specification)
        d = m.groupdict()
        name = d['name']
        path = d['callable']
        colons = path.count(':')
        if colons == 0:
            prefix, suffix = path, None
            if colons != 1:
            prefix, suffix = path.split(':')
        flags = d['flags']
        if flags is None:
            flags = []
            flags = [f.strip() for f in flags.split(',')]
        result = ExportEntry(name, prefix, suffix, flags)
def get_cache_base(suffix=None):
    Return the default base location for distlib caches. If the directory does
    not exist, it is created. Use the suffix provided for the base directory,
    and default to '.distlib' if it isn't provided.
    On Windows, if LOCALAPPDATA is defined in the environment, then it is
    assumed to be a directory, and will be the parent directory of the result.
    On POSIX, and on Windows if LOCALAPPDATA is not defined, the user's home
    directory - using os.expanduser('~') - will be the parent directory of
    the result.
    The result is just the directory '.distlib' in the parent directory as
    determined above, or with the name specified with ``suffix``.
    if suffix is None:
        suffix = '.distlib'
    if os.name == 'nt' and 'LOCALAPPDATA' in os.environ:
        result = os.path.expandvars('$localappdata')
        # Assume posix, or old Windows
        result = os.path.expanduser('~')
    # we use 'isdir' instead of 'exists', because we want to
    # fail if there's a file with that name
    if os.path.isdir(result):
        usable = os.access(result, os.W_OK)
        if not usable:
            logger.warning('Directory exists but is not writable: %s', result)
            os.makedirs(result)
            usable = True
            logger.warning('Unable to create %s', result, exc_info=True)
            usable = False
        result = tempfile.mkdtemp()
        logger.warning('Default location unusable, using %s', result)
    return os.path.join(result, suffix)
def path_to_cache_dir(path, use_abspath=True):
    Convert an absolute path to a directory name for use in a cache.
    The algorithm used is:
    #. On Windows, any ``':'`` in the drive is replaced with ``'---'``.
    #. Any occurrence of ``os.sep`` is replaced with ``'--'``.
    #. ``'.cache'`` is appended.
    d, p = os.path.splitdrive(os.path.abspath(path) if use_abspath else path)
    if d:
        d = d.replace(':', '---')
    p = p.replace(os.sep, '--')
    return d + p + '.cache'
def ensure_slash(s):
    if not s.endswith('/'):
        return s + '/'
def parse_credentials(netloc):
    username = password = None
    if '@' in netloc:
        prefix, netloc = netloc.rsplit('@', 1)
        if ':' not in prefix:
            username = prefix
            username, password = prefix.split(':', 1)
        username = unquote(username)
    return username, password, netloc
def get_process_umask():
    result = os.umask(0o22)
    os.umask(result)
def is_string_sequence(seq):
    i = None
    for i, s in enumerate(seq):
        if not isinstance(s, string_types):
    assert i is not None
PROJECT_NAME_AND_VERSION = re.compile('([a-z0-9_]+([.-][a-z_][a-z0-9_]*)*)-'
                                      '([a-z0-9_.+-]+)', re.I)
PYTHON_VERSION = re.compile(r'-py(\d\.?\d?)')
def split_filename(filename, project_name=None):
    Extract name, version, python version from a filename (no extension)
    Return name, version, pyver or None
    pyver = None
    filename = unquote(filename).replace(' ', '-')
    m = PYTHON_VERSION.search(filename)
        pyver = m.group(1)
        filename = filename[:m.start()]
    if project_name and len(filename) > len(project_name) + 1:
        m = re.match(re.escape(project_name) + r'\b', filename)
            n = m.end()
            result = filename[:n], filename[n + 1:], pyver
        m = PROJECT_NAME_AND_VERSION.match(filename)
            result = m.group(1), m.group(3), pyver
# Allow spaces in name because of legacy dists like "Twisted Core"
NAME_VERSION_RE = re.compile(r'(?P<name>[\w .-]+)\s*'
                             r'\(\s*(?P<ver>[^\s)]+)\)$')
def parse_name_and_version(p):
    A utility method used to get name and version from a string.
    From e.g. a Provides-Dist value.
    :param p: A value in a form 'foo (1.0)'
    :return: The name and version as a tuple.
    m = NAME_VERSION_RE.match(p)
        raise DistlibException('Ill-formed name/version string: \'%s\'' % p)
    return d['name'].strip().lower(), d['ver']
def get_extras(requested, available):
    result = set()
    requested = set(requested or [])
    available = set(available or [])
    if '*' in requested:
        requested.remove('*')
        result |= available
    for r in requested:
        if r == '-':
            result.add(r)
        elif r.startswith('-'):
            unwanted = r[1:]
            if unwanted not in available:
                logger.warning('undeclared extra: %s' % unwanted)
            if unwanted in result:
                result.remove(unwanted)
            if r not in available:
                logger.warning('undeclared extra: %s' % r)
# Extended metadata functionality
def _get_external_data(url):
        # urlopen might fail if it runs into redirections,
        # because of Python issue #13696. Fixed in locators
        # using a custom redirect handler.
        resp = urlopen(url)
        headers = resp.info()
        ct = headers.get('Content-Type')
        if not ct.startswith('application/json'):
            logger.debug('Unexpected response for JSON request: %s', ct)
            reader = codecs.getreader('utf-8')(resp)
            # data = reader.read().decode('utf-8')
            # result = json.loads(data)
            result = json.load(reader)
        logger.exception('Failed to get external data for %s: %s', url, e)
_external_data_base_url = 'https://www.red-dove.com/pypi/projects/'
def get_project_data(name):
    url = '%s/%s/project.json' % (name[0].upper(), name)
    url = urljoin(_external_data_base_url, url)
    result = _get_external_data(url)
def get_package_data(name, version):
    url = '%s/%s/package-%s.json' % (name[0].upper(), name, version)
    return _get_external_data(url)
    A class implementing a cache for resources that need to live in the file system
    e.g. shared libraries. This class was moved from resources to here because it
    could be used by other modules, e.g. the wheel module.
    def __init__(self, base):
        Initialise an instance.
        :param base: The base directory where the cache should be located.
        if not os.path.isdir(base):  # pragma: no cover
            os.makedirs(base)
        if (os.stat(base).st_mode & 0o77) != 0:
            logger.warning('Directory \'%s\' is not private', base)
        self.base = os.path.abspath(os.path.normpath(base))
    def prefix_to_dir(self, prefix, use_abspath=True):
        Converts a resource prefix to a directory name in the cache.
        return path_to_cache_dir(prefix, use_abspath=use_abspath)
        Clear the cache.
        not_removed = []
        for fn in os.listdir(self.base):
            fn = os.path.join(self.base, fn)
                if os.path.islink(fn) or os.path.isfile(fn):
                    os.remove(fn)
                elif os.path.isdir(fn):
                    shutil.rmtree(fn)
                not_removed.append(fn)
        return not_removed
class EventMixin(object):
    A very simple publish/subscribe system.
        self._subscribers = {}
    def add(self, event, subscriber, append=True):
        Add a subscriber for an event.
        :param event: The name of an event.
        :param subscriber: The subscriber to be added (and called when the
                           event is published).
        :param append: Whether to append or prepend the subscriber to an
                       existing subscriber list for the event.
        subs = self._subscribers
        if event not in subs:
            subs[event] = deque([subscriber])
            sq = subs[event]
            if append:
                sq.append(subscriber)
                sq.appendleft(subscriber)
    def remove(self, event, subscriber):
        Remove a subscriber for an event.
        :param subscriber: The subscriber to be removed.
            raise ValueError('No subscribers: %r' % event)
        subs[event].remove(subscriber)
    def get_subscribers(self, event):
        Return an iterator for the subscribers for an event.
        :param event: The event to return subscribers for.
        return iter(self._subscribers.get(event, ()))
    def publish(self, event, *args, **kwargs):
        Publish a event and return a list of values returned by its
        :param event: The event to publish.
        :param args: The positional arguments to pass to the event's
        :param kwargs: The keyword arguments to pass to the event's
        for subscriber in self.get_subscribers(event):
                value = subscriber(event, *args, **kwargs)
                logger.exception('Exception during event publication')
            result.append(value)
        logger.debug('publish %s: args = %s, kwargs = %s, result = %s', event, args, kwargs, result)
# Simple sequencing
class Sequencer(object):
        self._preds = {}
        self._succs = {}
        self._nodes = set()  # nodes with no preds/succs
    def add_node(self, node):
        self._nodes.add(node)
    def remove_node(self, node, edges=False):
        if node in self._nodes:
            self._nodes.remove(node)
        if edges:
            for p in set(self._preds.get(node, ())):
                self.remove(p, node)
            for s in set(self._succs.get(node, ())):
                self.remove(node, s)
            # Remove empties
            for k, v in list(self._preds.items()):
                if not v:
                    del self._preds[k]
            for k, v in list(self._succs.items()):
                    del self._succs[k]
    def add(self, pred, succ):
        assert pred != succ
        self._preds.setdefault(succ, set()).add(pred)
        self._succs.setdefault(pred, set()).add(succ)
    def remove(self, pred, succ):
            preds = self._preds[succ]
            succs = self._succs[pred]
        except KeyError:  # pragma: no cover
            raise ValueError('%r not a successor of anything' % succ)
            preds.remove(pred)
            succs.remove(succ)
            raise ValueError('%r not a successor of %r' % (succ, pred))
    def is_step(self, step):
        return (step in self._preds or step in self._succs or step in self._nodes)
    def get_steps(self, final):
        if not self.is_step(final):
            raise ValueError('Unknown: %r' % final)
        todo = []
        todo.append(final)
            step = todo.pop(0)
            if step in seen:
                # if a step was already seen,
                # move it to the end (so it will appear earlier
                # when reversed on return) ... but not for the
                # final step, as that would be confusing for
                if step != final:
                    result.remove(step)
                    result.append(step)
                seen.add(step)
                preds = self._preds.get(step, ())
                todo.extend(preds)
        return reversed(result)
    def strong_connections(self):
        # http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
        index_counter = [0]
        stack = []
        lowlinks = {}
        index = {}
        graph = self._succs
        def strongconnect(node):
            # set the depth index for this node to the smallest unused index
            index[node] = index_counter[0]
            lowlinks[node] = index_counter[0]
            index_counter[0] += 1
            stack.append(node)
            # Consider successors
                successors = graph[node]
                successors = []
            for successor in successors:
                if successor not in lowlinks:
                    # Successor has not yet been visited
                    strongconnect(successor)
                    lowlinks[node] = min(lowlinks[node], lowlinks[successor])
                elif successor in stack:
                    # the successor is in the stack and hence in the current
                    # strongly connected component (SCC)
                    lowlinks[node] = min(lowlinks[node], index[successor])
            # If `node` is a root node, pop the stack and generate an SCC
            if lowlinks[node] == index[node]:
                connected_component = []
                    successor = stack.pop()
                    connected_component.append(successor)
                    if successor == node:
                component = tuple(connected_component)
                # storing the result
                result.append(component)
        for node in graph:
            if node not in lowlinks:
                strongconnect(node)
    def dot(self):
        result = ['digraph G {']
        for succ in self._preds:
            for pred in preds:
                result.append('  %s -> %s;' % (pred, succ))
        for node in self._nodes:
            result.append('  %s;' % node)
        result.append('}')
        return '\n'.join(result)
# Unarchiving functionality for zip, tar, tgz, tbz, whl
ARCHIVE_EXTENSIONS = ('.tar.gz', '.tar.bz2', '.tar', '.zip', '.tgz', '.tbz', '.whl')
def unarchive(archive_filename, dest_dir, format=None, check=True):
    def check_path(path):
        if not isinstance(path, text_type):
            path = path.decode('utf-8')
        p = os.path.abspath(os.path.join(dest_dir, path))
        if not p.startswith(dest_dir) or p[plen] != os.sep:
            raise ValueError('path outside destination: %r' % p)
    dest_dir = os.path.abspath(dest_dir)
    plen = len(dest_dir)
    archive = None
    if format is None:
        if archive_filename.endswith(('.zip', '.whl')):
            format = 'zip'
        elif archive_filename.endswith(('.tar.gz', '.tgz')):
            format = 'tgz'
            mode = 'r:gz'
        elif archive_filename.endswith(('.tar.bz2', '.tbz')):
            format = 'tbz'
            mode = 'r:bz2'
        elif archive_filename.endswith('.tar'):
            format = 'tar'
            mode = 'r'
            raise ValueError('Unknown format for %r' % archive_filename)
        if format == 'zip':
            archive = ZipFile(archive_filename, 'r')
                names = archive.namelist()
                    check_path(name)
            archive = tarfile.open(archive_filename, mode)
                names = archive.getnames()
        if format != 'zip' and sys.version_info[0] < 3:
            # See Python issue 17153. If the dest path contains Unicode,
            # tarfile extraction fails on Python 2.x if a member path name
            # contains non-ASCII characters - it leads to an implicit
            # bytes -> unicode conversion using ASCII to decode.
            for tarinfo in archive.getmembers():
                if not isinstance(tarinfo.name, text_type):
                    tarinfo.name = tarinfo.name.decode('utf-8')
        # Limit extraction of dangerous items, if this Python
        # allows it easily. If not, just trust the input.
        # See: https://docs.python.org/3/library/tarfile.html#extraction-filters
        def extraction_filter(member, path):
            """Run tarfile.tar_filter, but raise the expected ValueError"""
            # This is only called if the current Python has tarfile filters
                return tarfile.tar_filter(member, path)
            except tarfile.FilterError as exc:
                raise ValueError(str(exc))
        archive.extraction_filter = extraction_filter
        archive.extractall(dest_dir)
        if archive:
            archive.close()
def zip_dir(directory):
    """zip a directory tree into a BytesIO object"""
    result = io.BytesIO()
    dlen = len(directory)
    with ZipFile(result, "w") as zf:
        for root, dirs, files in os.walk(directory):
                full = os.path.join(root, name)
                rel = root[dlen:]
                dest = os.path.join(rel, name)
                zf.write(full, dest)
# Simple progress bar
UNITS = ('', 'K', 'M', 'G', 'T', 'P')
class Progress(object):
    unknown = 'UNKNOWN'
    def __init__(self, minval=0, maxval=100):
        assert maxval is None or maxval >= minval
        self.min = self.cur = minval
        self.max = maxval
        self.started = None
        self.elapsed = 0
        self.done = False
    def update(self, curval):
        assert self.min <= curval
        assert self.max is None or curval <= self.max
        self.cur = curval
        if self.started is None:
            self.started = now
            self.elapsed = now - self.started
    def increment(self, incr):
        assert incr >= 0
        self.update(self.cur + incr)
        self.update(self.min)
        if self.max is not None:
            self.update(self.max)
        self.done = True
    def maximum(self):
        return self.unknown if self.max is None else self.max
    def percentage(self):
        if self.done:
            result = '100 %'
        elif self.max is None:
            result = ' ?? %'
            v = 100.0 * (self.cur - self.min) / (self.max - self.min)
            result = '%3d %%' % v
    def format_duration(self, duration):
        if (duration <= 0) and self.max is None or self.cur == self.min:
            result = '??:??:??'
        # elif duration < 1:
        #     result = '--:--:--'
            result = time.strftime('%H:%M:%S', time.gmtime(duration))
    def ETA(self):
            prefix = 'Done'
            t = self.elapsed
            # import pdb; pdb.set_trace()
            prefix = 'ETA '
            if self.max is None:
                t = -1
            elif self.elapsed == 0 or (self.cur == self.min):
                t = 0
                t = float(self.max - self.min)
                t /= self.cur - self.min
                t = (t - 1) * self.elapsed
        return '%s: %s' % (prefix, self.format_duration(t))
    def speed(self):
        if self.elapsed == 0:
            result = 0.0
            result = (self.cur - self.min) / self.elapsed
        for unit in UNITS:
            if result < 1000:
            result /= 1000.0
        return '%d %sB/s' % (result, unit)
# Glob functionality
RICH_GLOB = re.compile(r'\{([^}]*)\}')
_CHECK_RECURSIVE_GLOB = re.compile(r'[^/\\,{]\*\*|\*\*[^/\\,}]')
_CHECK_MISMATCH_SET = re.compile(r'^[^{]*\}|\{[^}]*$')
def iglob(path_glob):
    """Extended globbing function that supports ** and {opt1,opt2,opt3}."""
    if _CHECK_RECURSIVE_GLOB.search(path_glob):
        msg = """invalid glob %r: recursive glob "**" must be used alone"""
        raise ValueError(msg % path_glob)
    if _CHECK_MISMATCH_SET.search(path_glob):
        msg = """invalid glob %r: mismatching set marker '{' or '}'"""
    return _iglob(path_glob)
def _iglob(path_glob):
    rich_path_glob = RICH_GLOB.split(path_glob, 1)
    if len(rich_path_glob) > 1:
        assert len(rich_path_glob) == 3, rich_path_glob
        prefix, set, suffix = rich_path_glob
        for item in set.split(','):
            for path in _iglob(''.join((prefix, item, suffix))):
        if '**' not in path_glob:
            for item in std_iglob(path_glob):
            prefix, radical = path_glob.split('**', 1)
                prefix = '.'
            if radical == '':
                radical = '*'
                # we support both
                radical = radical.lstrip('/')
                radical = radical.lstrip('\\')
            for path, dir, files in os.walk(prefix):
                for fn in _iglob(os.path.join(path, radical)):
if ssl:
    from .compat import (HTTPSHandler as BaseHTTPSHandler, match_hostname, CertificateError)
    # HTTPSConnection which verifies certificates/matches domains
    class HTTPSConnection(httplib.HTTPSConnection):
        ca_certs = None  # set this to the path to the certs file (.pem)
        check_domain = True  # only used if ca_certs is not None
        # noinspection PyPropertyAccess
            sock = socket.create_connection((self.host, self.port), self.timeout)
            if getattr(self, '_tunnel_host', False):
                self.sock = sock
                self._tunnel()
            context = ssl.SSLContext(ssl.PROTOCOL_SSLv23)
            if hasattr(ssl, 'OP_NO_SSLv2'):
                context.options |= ssl.OP_NO_SSLv2
            if getattr(self, 'cert_file', None):
                context.load_cert_chain(self.cert_file, self.key_file)
            if self.ca_certs:
                context.verify_mode = ssl.CERT_REQUIRED
                context.load_verify_locations(cafile=self.ca_certs)
                if getattr(ssl, 'HAS_SNI', False):
                    kwargs['server_hostname'] = self.host
            self.sock = context.wrap_socket(sock, **kwargs)
            if self.ca_certs and self.check_domain:
                    match_hostname(self.sock.getpeercert(), self.host)
                    logger.debug('Host verified: %s', self.host)
                except CertificateError:  # pragma: no cover
                    self.sock.shutdown(socket.SHUT_RDWR)
                    self.sock.close()
    class HTTPSHandler(BaseHTTPSHandler):
        def __init__(self, ca_certs, check_domain=True):
            BaseHTTPSHandler.__init__(self)
            self.ca_certs = ca_certs
            self.check_domain = check_domain
        def _conn_maker(self, *args, **kwargs):
            This is called to create a connection instance. Normally you'd
            pass a connection class to do_open, but it doesn't actually check for
            a class, and just expects a callable. As long as we behave just as a
            constructor would have, we should be OK. If it ever changes so that
            we *must* pass a class, we'll create an UnsafeHTTPSConnection class
            which just sets check_domain to False in the class definition, and
            choose which one to pass to do_open.
            result = HTTPSConnection(*args, **kwargs)
                result.ca_certs = self.ca_certs
                result.check_domain = self.check_domain
        def https_open(self, req):
                return self.do_open(self._conn_maker, req)
            except URLError as e:
                if 'certificate verify failed' in str(e.reason):
                    raise CertificateError('Unable to verify server certificate '
                                           'for %s' % req.host)
    # To prevent against mixing HTTP traffic with HTTPS (examples: A Man-In-The-
    # Middle proxy using HTTP listens on port 443, or an index mistakenly serves
    # HTML containing a http://xyz link when it should be https://xyz),
    # you can use the following handler class, which does not allow HTTP traffic.
    # It works by inheriting from HTTPHandler - so build_opener won't add a
    # handler for HTTP itself.
    class HTTPSOnlyHandler(HTTPSHandler, HTTPHandler):
        def http_open(self, req):
            raise URLError('Unexpected HTTP request on what should be a secure '
                           'connection: %s' % req)
# XML-RPC with timeouts
class Transport(xmlrpclib.Transport):
    def __init__(self, timeout, use_datetime=0):
        xmlrpclib.Transport.__init__(self, use_datetime)
    def make_connection(self, host):
        h, eh, x509 = self.get_host_info(host)
        if not self._connection or host != self._connection[0]:
            self._extra_headers = eh
            self._connection = host, httplib.HTTPConnection(h)
        return self._connection[1]
    class SafeTransport(xmlrpclib.SafeTransport):
            xmlrpclib.SafeTransport.__init__(self, use_datetime)
            h, eh, kwargs = self.get_host_info(host)
            kwargs['timeout'] = self.timeout
                self._connection = host, httplib.HTTPSConnection(h, None, **kwargs)
class ServerProxy(xmlrpclib.ServerProxy):
    def __init__(self, uri, **kwargs):
        self.timeout = timeout = kwargs.pop('timeout', None)
        # The above classes only come into play if a timeout
        # is specified
            # scheme = splittype(uri)  # deprecated as of Python 3.8
            scheme = urlparse(uri)[0]
            use_datetime = kwargs.get('use_datetime', 0)
            if scheme == 'https':
                tcls = SafeTransport
                tcls = Transport
            kwargs['transport'] = t = tcls(timeout, use_datetime=use_datetime)
            self.transport = t
        xmlrpclib.ServerProxy.__init__(self, uri, **kwargs)
# CSV functionality. This is provided because on 2.x, the csv module can't
# handle Unicode. However, we need to deal with Unicode in e.g. RECORD files.
def _csv_open(fn, mode, **kwargs):
    if sys.version_info[0] < 3:
        mode += 'b'
        kwargs['newline'] = ''
        # Python 3 determines encoding from locale. Force 'utf-8'
        # file encoding to match other forced utf-8 encoding
        kwargs['encoding'] = 'utf-8'
    return open(fn, mode, **kwargs)
class CSVBase(object):
        'delimiter': str(','),  # The strs are used because we need native
        'quotechar': str('"'),  # str in the csv API (2.x won't take
        'lineterminator': str('\n')  # Unicode)
    def __exit__(self, *exc_info):
class CSVReader(CSVBase):
        if 'stream' in kwargs:
            stream = kwargs['stream']
            self.stream = _csv_open(kwargs['path'], 'r')
        self.reader = csv.reader(self.stream, **self.defaults)
        result = next(self.reader)
            for i, item in enumerate(result):
                if not isinstance(item, text_type):
                    result[i] = item.decode('utf-8')
    __next__ = next
class CSVWriter(CSVBase):
    def __init__(self, fn, **kwargs):
        self.stream = _csv_open(fn, 'w')
        self.writer = csv.writer(self.stream, **self.defaults)
    def writerow(self, row):
            r = []
            for item in row:
                if isinstance(item, text_type):
                    item = item.encode('utf-8')
                r.append(item)
            row = r
        self.writer.writerow(row)
#   Configurator functionality
class Configurator(BaseConfigurator):
    value_converters = dict(BaseConfigurator.value_converters)
    value_converters['inc'] = 'inc_convert'
    def __init__(self, config, base=None):
        super(Configurator, self).__init__(config)
        self.base = base or os.getcwd()
    def configure_custom(self, config):
        def convert(o):
            if isinstance(o, (list, tuple)):
                result = type(o)([convert(i) for i in o])
            elif isinstance(o, dict):
                if '()' in o:
                    result = self.configure_custom(o)
                    for k in o:
                        result[k] = convert(o[k])
                result = self.convert(o)
        c = config.pop('()')
        if not callable(c):
            c = self.resolve(c)
        props = config.pop('.', None)
        # Check for valid identifiers
        args = config.pop('[]', ())
            args = tuple([convert(o) for o in args])
        items = [(k, convert(config[k])) for k in config if valid_ident(k)]
        kwargs = dict(items)
        result = c(*args, **kwargs)
        if props:
            for n, v in props.items():
                setattr(result, n, convert(v))
        result = self.config[key]
        if isinstance(result, dict) and '()' in result:
            self.config[key] = result = self.configure_custom(result)
    def inc_convert(self, value):
        """Default converter for the inc:// protocol."""
        if not os.path.isabs(value):
            value = os.path.join(self.base, value)
        with codecs.open(value, 'r', encoding='utf-8') as f:
            result = json.load(f)
class SubprocessMixin(object):
    Mixin for running subprocesses and capturing their output
    def __init__(self, verbose=False, progress=None):
        self.progress = progress
    def reader(self, stream, context):
        Read lines from a subprocess' output stream and either pass to a progress
        callable (if specified) or write progress information to sys.stderr.
        progress = self.progress
        verbose = self.verbose
            s = stream.readline()
            if progress is not None:
                progress(s, context)
                if not verbose:
                    sys.stderr.write('.')
                    sys.stderr.write(s.decode('utf-8'))
    def run_command(self, cmd, **kwargs):
        p = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, **kwargs)
        t1 = threading.Thread(target=self.reader, args=(p.stdout, 'stdout'))
        t1.start()
        t2 = threading.Thread(target=self.reader, args=(p.stderr, 'stderr'))
        t2.start()
        p.wait()
        t1.join()
        t2.join()
        if self.progress is not None:
            self.progress('done.', 'main')
        elif self.verbose:
            sys.stderr.write('done.\n')
def normalize_name(name):
    """Normalize a python package name a la PEP 503"""
    # https://www.python.org/dev/peps/pep-0503/#normalized-names
    return re.sub('[-_.]+', '-', name).lower()
# def _get_pypirc_command():
# """
# Get the distutils command for interacting with PyPI configurations.
# :return: the command.
# from distutils.core import Distribution
# from distutils.config import PyPIRCCommand
# d = Distribution()
# return PyPIRCCommand(d)
class PyPIRCFile(object):
    def __init__(self, fn=None, url=None):
        if fn is None:
            fn = os.path.join(os.path.expanduser('~'), '.pypirc')
        self.filename = fn
        if os.path.exists(self.filename):
            repository = self.url or self.DEFAULT_REPOSITORY
            config = configparser.RawConfigParser()
            config.read(self.filename)
                _servers = [server.strip() for server in index_servers.split('\n') if server.strip() != '']
                        result = {'server': server}
                        result['username'] = config.get(server, 'username')
                        for key, default in (('repository', self.DEFAULT_REPOSITORY), ('realm', self.DEFAULT_REALM),
                                             ('password', None)):
                                result[key] = config.get(server, key)
                                result[key] = default
                        if (server == 'pypi' and repository in (self.DEFAULT_REPOSITORY, 'pypi')):
                            result['repository'] = self.DEFAULT_REPOSITORY
                        elif (result['server'] != repository and result['repository'] != repository):
                    'realm': self.DEFAULT_REALM
    def update(self, username, password):
        fn = self.filename
        config.read(fn)
        if not config.has_section('pypi'):
            config.add_section('pypi')
        config.set('pypi', 'username', username)
        config.set('pypi', 'password', password)
        with open(fn, 'w') as f:
            config.write(f)
def _load_pypirc(index):
    Read the PyPI access configuration as supported by distutils.
    return PyPIRCFile(url=index.url).read()
def _store_pypirc(index):
    PyPIRCFile().update(index.username, index.password)
# get_platform()/get_host_platform() copied from Python 3.10.a0 source, with some minor
# tweaks
    """Return a string that identifies the current platform.  This is used mainly to
    distinguish platform-specific build directories and platform-specific built
    distributions.  Typically includes the OS name and version and the
    architecture (as supplied by 'os.uname()'), although the exact information
    included depends on the OS; eg. on Linux, the kernel version isn't
    particularly important.
    Examples of returned values:
       linux-i586
       linux-alpha (?)
       solaris-2.6-sun4u
    Windows will return one of:
       win-amd64 (64bit Windows on AMD64 (aka x86_64, Intel64, EM64T, etc)
       win32 (all others - specifically, sys.platform is returned)
    For other non-POSIX platforms, currently just returns 'sys.platform'.
        if 'amd64' in sys.version.lower():
            return 'win-amd64'
        return sys.platform
    # Set for cross builds explicitly
    if "_PYTHON_HOST_PLATFORM" in os.environ:
        return os.environ["_PYTHON_HOST_PLATFORM"]
    if os.name != 'posix' or not hasattr(os, 'uname'):
        # XXX what about the architecture? NT is Intel or Alpha,
        # Mac OS is M68k or PPC, etc.
    # Try to distinguish various flavours of Unix
    (osname, host, release, version, machine) = os.uname()
    # Convert the OS name to lowercase, remove '/' characters, and translate
    # spaces (for "Power Macintosh")
    osname = osname.lower().replace('/', '')
    machine = machine.replace(' ', '_').replace('/', '-')
    if osname[:5] == 'linux':
        # At least on Linux/Intel, 'machine' is the processor --
        # i386, etc.
        # XXX what about Alpha, SPARC, etc?
        return "%s-%s" % (osname, machine)
    elif osname[:5] == 'sunos':
        if release[0] >= '5':  # SunOS 5 == Solaris 2
            osname = 'solaris'
            release = '%d.%s' % (int(release[0]) - 3, release[2:])
            # We can't use 'platform.architecture()[0]' because a
            # bootstrap problem. We use a dict to get an error
            # if some suspicious happens.
            bitness = {2147483647: '32bit', 9223372036854775807: '64bit'}
            machine += '.%s' % bitness[sys.maxsize]
        # fall through to standard osname-release-machine representation
    elif osname[:3] == 'aix':
        from _aix_support import aix_platform
        return aix_platform()
    elif osname[:6] == 'cygwin':
        osname = 'cygwin'
        rel_re = re.compile(r'[\d.]+', re.ASCII)
        m = rel_re.match(release)
            release = m.group()
    elif osname[:6] == 'darwin':
        import _osx_support
        osname, release, machine = _osx_support.get_platform_osx(sysconfig.get_config_vars(), osname, release, machine)
    return '%s-%s-%s' % (osname, release, machine)
_TARGET_TO_PLAT = {
    cross_compilation_target = os.environ.get('VSCMD_ARG_TGT_ARCH')
    if cross_compilation_target not in _TARGET_TO_PLAT:
    return _TARGET_TO_PLAT[cross_compilation_target]
    pygments.util
    ~~~~~~~~~~~~~
    Utility functions.
from io import TextIOWrapper
split_path_re = re.compile(r'[/\\ ]')
doctype_lookup_re = re.compile(r'''
    <!DOCTYPE\s+(
     [a-zA-Z_][a-zA-Z0-9]*
     (?: \s+      # optional in HTML5
     [a-zA-Z_][a-zA-Z0-9]*\s+
     "[^"]*")?
     [^>]*>
''', re.DOTALL | re.MULTILINE | re.VERBOSE)
tag_re = re.compile(r'<(.+?)(\s.*?)?>.*?</.+?>',
                    re.IGNORECASE | re.DOTALL | re.MULTILINE)
xml_decl_re = re.compile(r'\s*<\?xml[^>]*\?>', re.I)
class ClassNotFound(ValueError):
    """Raised if one of the lookup functions didn't find a matching class."""
class OptionError(Exception):
    This exception will be raised by all option processing functions if
    the type or value of the argument is not correct.
def get_choice_opt(options, optname, allowed, default=None, normcase=False):
    If the key `optname` from the dictionary is not in the sequence
    `allowed`, raise an error, otherwise return it.
    string = options.get(optname, default)
    if normcase:
        string = string.lower()
    if string not in allowed:
        raise OptionError('Value for option {} must be one of {}'.format(optname, ', '.join(map(str, allowed))))
def get_bool_opt(options, optname, default=None):
    Intuitively, this is `options.get(optname, default)`, but restricted to
    Boolean value. The Booleans can be represented as string, in order to accept
    Boolean value from the command line arguments. If the key `optname` is
    present in the dictionary `options` and is not associated with a Boolean,
    raise an `OptionError`. If it is absent, `default` is returned instead.
    The valid string values for ``True`` are ``1``, ``yes``, ``true`` and
    ``on``, the ones for ``False`` are ``0``, ``no``, ``false`` and ``off``
    (matched case-insensitively).
    if isinstance(string, bool):
    elif isinstance(string, int):
        return bool(string)
    elif not isinstance(string, str):
        raise OptionError(f'Invalid type {string!r} for option {optname}; use '
                          '1/0, yes/no, true/false, on/off')
    elif string.lower() in ('1', 'yes', 'true', 'on'):
    elif string.lower() in ('0', 'no', 'false', 'off'):
        raise OptionError(f'Invalid value {string!r} for option {optname}; use '
def get_int_opt(options, optname, default=None):
    """As :func:`get_bool_opt`, but interpret the value as an integer."""
        return int(string)
        raise OptionError(f'Invalid type {string!r} for option {optname}; you '
                          'must give an integer value')
        raise OptionError(f'Invalid value {string!r} for option {optname}; you '
def get_list_opt(options, optname, default=None):
    If the key `optname` from the dictionary `options` is a string,
    split it at whitespace and return it. If it is already a list
    or a tuple, it is returned as a list.
    val = options.get(optname, default)
    elif isinstance(val, (list, tuple)):
        return list(val)
        raise OptionError(f'Invalid type {val!r} for option {optname}; you '
                          'must give a list value')
def docstring_headline(obj):
    if not obj.__doc__:
    res = []
    for line in obj.__doc__.strip().splitlines():
        if line.strip():
            res.append(" " + line.strip())
    return ''.join(res).lstrip()
def make_analysator(f):
    """Return a static text analyser function that returns float values."""
    def text_analyse(text):
            rv = f(text)
        if not rv:
            return min(1.0, max(0.0, float(rv)))
    text_analyse.__doc__ = f.__doc__
    return staticmethod(text_analyse)
def shebang_matches(text, regex):
    r"""Check if the given regular expression matches the last part of the
    shebang if one exists.
        >>> from pygments.util import shebang_matches
        >>> shebang_matches('#!/usr/bin/env python', r'python(2\.\d)?')
        >>> shebang_matches('#!/usr/bin/python2.4', r'python(2\.\d)?')
        >>> shebang_matches('#!/usr/bin/python-ruby', r'python(2\.\d)?')
        >>> shebang_matches('#!/usr/bin/python/ruby', r'python(2\.\d)?')
        >>> shebang_matches('#!/usr/bin/startsomethingwith python',
        ...                 r'python(2\.\d)?')
    It also checks for common windows executable file extensions::
        >>> shebang_matches('#!C:\\Python2.4\\Python.exe', r'python(2\.\d)?')
    Parameters (``'-f'`` or ``'--foo'`` are ignored so ``'perl'`` does
    the same as ``'perl -e'``)
    Note that this method automatically searches the whole string (eg:
    the regular expression is wrapped in ``'^$'``)
    index = text.find('\n')
    if index >= 0:
        first_line = text[:index].lower()
        first_line = text.lower()
    if first_line.startswith('#!'):
            found = [x for x in split_path_re.split(first_line[2:].strip())
                     if x and not x.startswith('-')][-1]
        regex = re.compile(rf'^{regex}(\.(exe|cmd|bat|bin))?$', re.IGNORECASE)
        if regex.search(found) is not None:
def doctype_matches(text, regex):
    """Check if the doctype matches a regular expression (if present).
    Note that this method only checks the first part of a DOCTYPE.
    eg: 'html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN"'
    m = doctype_lookup_re.search(text)
    doctype = m.group(1)
    return re.compile(regex, re.I).match(doctype.strip()) is not None
def html_doctype_matches(text):
    """Check if the file looks like it has a html doctype."""
    return doctype_matches(text, r'html')
_looks_like_xml_cache = {}
def looks_like_xml(text):
    """Check if a doctype exists or if we have some tags."""
    if xml_decl_re.match(text):
    key = hash(text)
        return _looks_like_xml_cache[key]
        rv = tag_re.search(text[:1000]) is not None
        _looks_like_xml_cache[key] = rv
def surrogatepair(c):
    """Given a unicode character code with length greater than 16 bits,
    return the two 16 bit surrogate pair.
    # From example D28 of:
    # http://www.unicode.org/book/ch03.pdf
    return (0xd7c0 + (c >> 10), (0xdc00 + (c & 0x3ff)))
def format_lines(var_name, seq, raw=False, indent_level=0):
    """Formats a sequence of strings for output."""
    base_indent = ' ' * indent_level * 4
    inner_indent = ' ' * (indent_level + 1) * 4
    lines.append(base_indent + var_name + ' = (')
    if raw:
        # These should be preformatted reprs of, say, tuples.
        for i in seq:
            lines.append(inner_indent + i + ',')
            # Force use of single quotes
            r = repr(i + '"')
            lines.append(inner_indent + r[:-2] + r[-1] + ',')
    lines.append(base_indent + ')')
    return '\n'.join(lines)
def duplicates_removed(it, already_seen=()):
    Returns a list with duplicates removed from the iterable `it`.
    Order is preserved.
    for i in it:
        if i in seen or i in already_seen:
        lst.append(i)
        seen.add(i)
class Future:
    """Generic class to defer some work.
    Handled specially in RegexLexerMeta, to support regex string construction at
    first use.
def guess_decode(text):
    """Decode *text* with guessed encoding.
    First try UTF-8; this should fail for non-UTF-8 encodings.
    Then try the preferred locale encoding.
    Fall back to latin-1, which always works.
        text = text.decode('utf-8')
        return text, 'utf-8'
            prefencoding = locale.getpreferredencoding()
            text = text.decode()
            return text, prefencoding
        except (UnicodeDecodeError, LookupError):
            text = text.decode('latin1')
            return text, 'latin1'
def guess_decode_from_terminal(text, term):
    """Decode *text* coming from terminal *term*.
    First try the terminal encoding, if given.
    Then try UTF-8.  Then try the preferred locale encoding.
    if getattr(term, 'encoding', None):
            text = text.decode(term.encoding)
            return text, term.encoding
    return guess_decode(text)
def terminal_encoding(term):
    """Return our best guess of encoding for the given *term*."""
        return term.encoding
    return locale.getpreferredencoding()
class UnclosingTextIOWrapper(TextIOWrapper):
    # Don't close underlying buffer on destruction.
        self.flush()
    from shutil import which  # Python >= 3.3
    import os, sys
    # This is copied from Python 3.4.1
    def which(cmd, mode=os.F_OK | os.X_OK, path=None):
        """Given a command, mode, and a PATH string, return the path which
        conforms to the given mode on the PATH, or None if there is no such
        `mode` defaults to os.F_OK | os.X_OK. `path` defaults to the result
        of os.environ.get("PATH"), or can be overridden with a custom search
        # Check that a given file can be accessed with the correct mode.
        # Additionally check that `file` is not a directory, as on Windows
        # directories pass the os.access check.
        def _access_check(fn, mode):
            return (os.path.exists(fn) and os.access(fn, mode)
                    and not os.path.isdir(fn))
        # If we're given a path with a directory part, look it up directly rather
        # than referring to PATH directories. This includes checking relative to the
        # current directory, e.g. ./script
        if os.path.dirname(cmd):
            if _access_check(cmd, mode):
            path = os.environ.get("PATH", os.defpath)
        path = path.split(os.pathsep)
            # The current directory takes precedence on Windows.
            if not os.curdir in path:
                path.insert(0, os.curdir)
            # PATHEXT is necessary to check on Windows.
            pathext = os.environ.get("PATHEXT", "").split(os.pathsep)
            # See if the given file matches any of the expected path extensions.
            # This will allow us to short circuit when given "python.exe".
            # If it does match, only test that one, otherwise we have to try
            # others.
            if any(cmd.lower().endswith(ext.lower()) for ext in pathext):
                files = [cmd]
                files = [cmd + ext for ext in pathext]
            # On other platforms you don't have things like PATHEXT to tell you
            # what file suffixes are executable, so just pass on cmd as-is.
        for dir in path:
            normdir = os.path.normcase(dir)
            if not normdir in seen:
                seen.add(normdir)
                for thefile in files:
                    name = os.path.join(dir, thefile)
                    if _access_check(name, mode):
class PtyProcessError(Exception):
    """Generic error class for this package."""
def to_bytes(
    x: str | bytes, encoding: str | None = None, errors: str | None = None
    if isinstance(x, bytes):
    elif not isinstance(x, str):
        raise TypeError(f"not expecting type {type(x).__name__}")
    if encoding or errors:
        return x.encode(encoding or "utf-8", errors=errors or "strict")
    return x.encode()
def to_str(
    elif not isinstance(x, bytes):
        return x.decode(encoding or "utf-8", errors=errors or "strict")
    return x.decode()
def reraise(
    tp: type[BaseException] | None,
    value: BaseException,
    tb: TracebackType | None = None,
) -> typing.NoReturn:
        if value.__traceback__ is not tb:
            raise value.with_traceback(tb)
        raise value
        value = None  # type: ignore[assignment]
        tb = None
"""Utility."""
from functools import wraps, lru_cache
from typing import Callable, Any
DEBUG = 0x00001
RE_PATTERN_LINE_SPLIT = re.compile(r'(?:\r\n|(?!\r\n)[\n\r])|$')
UC_A = ord('A')
UC_Z = ord('Z')
@lru_cache(maxsize=512)
def lower(string: str) -> str:
    """Lower."""
    new_string = []
    for c in string:
        o = ord(c)
        new_string.append(chr(o + 32) if UC_A <= o <= UC_Z else c)
    return ''.join(new_string)
class SelectorSyntaxError(Exception):
    """Syntax error in a CSS selector."""
    def __init__(self, msg: str, pattern: str | None = None, index: int | None = None) -> None:
        """Initialize."""
        self.line = None
        self.col = None
        self.context = None
        if pattern is not None and index is not None:
            # Format pattern to show line and column position
            self.context, self.line, self.col = get_pattern_context(pattern, index)
            msg = f'{msg}\n  line {self.line}:\n{self.context}'
def deprecated(message: str, stacklevel: int = 2) -> Callable[..., Any]:  # pragma: no cover
    Raise a `DeprecationWarning` when wrapped function/method is called.
        @deprecated("This method will be removed in version X; use Y instead.")
        def some_method()"
    def _wrapper(func: Callable[..., Any]) -> Callable[..., Any]:
        def _deprecated_func(*args: Any, **kwargs: Any) -> Any:
                f"'{func.__name__}' is deprecated. {message}",
                stacklevel=stacklevel
        return _deprecated_func
    return _wrapper
def warn_deprecated(message: str, stacklevel: int = 2) -> None:  # pragma: no cover
    """Warn deprecated."""
def get_pattern_context(pattern: str, index: int) -> tuple[str, int, int]:
    """Get the pattern context."""
    current_line = 1
    col = 1
    text = []  # type: list[str]
    line = 1
    offset = None  # type: int | None
    # Split pattern by newline and handle the text before the newline
    for m in RE_PATTERN_LINE_SPLIT.finditer(pattern):
        linetext = pattern[last:m.start(0)]
        if not len(m.group(0)) and not len(text):
            indent = ''
            offset = -1
            col = index - last + 1
        elif last <= index < m.end(0):
            indent = '--> '
            offset = (-1 if index > m.start(0) else 0) + 3
            indent = '    '
            offset = None
            # Regardless of whether we are presented with `\r\n`, `\r`, or `\n`,
            # we will render the output with just `\n`. We will still log the column
            # correctly though.
            text.append('\n')
        text.append(f'{indent}{linetext}')
            text.append(' ' * (col + offset) + '^')
            line = current_line
        current_line += 1
        last = m.end(0)
    return ''.join(text), line, col
"""Module containing a memory memory manager which provides a sliding window on a number of memory mapped files"""
from mmap import mmap, ACCESS_READ
from mmap import ALLOCATIONGRANULARITY
__all__ = ["align_to_mmap", "is_64_bit",
           "MapWindow", "MapRegion", "MapRegionList", "ALLOCATIONGRANULARITY"]
#{ Utilities
def align_to_mmap(num, round_up):
    Align the given integer number to the closest page offset, which usually is 4096 bytes.
    :param round_up: if True, the next higher multiple of page size is used, otherwise
        the lower page_size will be used (i.e. if True, 1 becomes 4096, otherwise it becomes 0)
    :return: num rounded to closest page"""
    res = (num // ALLOCATIONGRANULARITY) * ALLOCATIONGRANULARITY
    if round_up and (res != num):
        res += ALLOCATIONGRANULARITY
    # END handle size
def is_64_bit():
    """:return: True if the system is 64 bit. Otherwise it can be assumed to be 32 bit"""
    return sys.maxsize > (1 << 32) - 1
#}END utilities
#{ Utility Classes
class MapWindow:
    """Utility type which is used to snap windows towards each other, and to adjust their size"""
        'ofs',      # offset into the file in bytes
        'size'              # size of the window in bytes
    def __init__(self, offset, size):
        self.ofs = offset
        return "MapWindow(%i, %i)" % (self.ofs, self.size)
    def from_region(cls, region):
        """:return: new window from a region"""
        return cls(region._b, region.size())
    def ofs_end(self):
        return self.ofs + self.size
    def align(self):
        """Assures the previous window area is contained in the new one"""
        nofs = align_to_mmap(self.ofs, 0)
        self.size += self.ofs - nofs    # keep size constant
        self.ofs = nofs
        self.size = align_to_mmap(self.size, 1)
    def extend_left_to(self, window, max_size):
        """Adjust the offset to start where the given window on our left ends if possible,
        but don't make yourself larger than max_size.
        The resize will assure that the new window still contains the old window area"""
        rofs = self.ofs - window.ofs_end()
        nsize = rofs + self.size
        rofs -= nsize - min(nsize, max_size)
        self.ofs -= rofs
        self.size += rofs
    def extend_right_to(self, window, max_size):
        """Adjust the size to make our window end where the right window begins, but don't
        get larger than max_size"""
        self.size = min(self.size + (window.ofs - self.ofs_end()), max_size)
class MapRegion:
    """Defines a mapped region of memory, aligned to pagesizes
    **Note:** deallocates used region automatically on destruction"""
        '_b',   # beginning of mapping
        '_mf',  # mapped memory chunk (as returned by mmap)
        '_uc',  # total amount of usages
        '_size',  # cached size of our memory map
        '__weakref__'
    #{ Configuration
    #} END configuration
    def __init__(self, path_or_fd, ofs, size, flags=0):
        """Initialize a region, allocate the memory map
        :param path_or_fd: path to the file to map, or the opened file descriptor
        :param ofs: **aligned** offset into the file to be mapped
        :param size: if size is larger then the file on disk, the whole file will be
            allocated the the size automatically adjusted
        :param flags: additional flags to be given when opening the file.
        :raise Exception: if no memory can be allocated"""
        self._b = ofs
        self._uc = 0
        if isinstance(path_or_fd, int):
            fd = path_or_fd
            fd = os.open(path_or_fd, os.O_RDONLY | getattr(os, 'O_BINARY', 0) | flags)
        # END handle fd
            kwargs = dict(access=ACCESS_READ, offset=ofs)
            corrected_size = size
            sizeofs = ofs
            # have to correct size, otherwise (instead of the c version) it will
            # bark that the size is too large ... many extra file accesses because
            # if this ... argh !
            actual_size = min(os.fstat(fd).st_size - sizeofs, corrected_size)
            self._mf = mmap(fd, actual_size, **kwargs)
            # END handle memory mode
            self._size = len(self._mf)
            if isinstance(path_or_fd, str):
            # END only close it if we opened it
        # END close file handle
        # We assume the first one to use us keeps us around
        self.increment_client_count()
        return "MapRegion<%i, %i>" % (self._b, self.size())
    #{ Interface
    def buffer(self):
        """:return: a buffer containing the memory"""
        return self._mf
    def map(self):
        """:return: a memory map containing the memory"""
    def ofs_begin(self):
        """:return: absolute byte offset to the first byte of the mapping"""
        return self._b
        """:return: total size of the mapped region in bytes"""
        """:return: Absolute offset to one byte beyond the mapping into the file"""
        return self._b + self._size
    def includes_ofs(self, ofs):
        """:return: True if the given offset can be read in our mapped region"""
        return self._b <= ofs < self._b + self._size
    def client_count(self):
        """:return: number of clients currently using this region"""
        return self._uc
    def increment_client_count(self, ofs = 1):
        """Adjust the usage count by the given positive or negative offset.
        If usage count equals 0, we will auto-release our resources
        :return: True if we released resources, False otherwise. In the latter case, we can still be used"""
        self._uc += ofs
        assert self._uc > -1, "Increments must match decrements, usage counter negative: %i" % self._uc
        if self.client_count() == 0:
            self.release()
        # end handle release
        """Release all resources this instance might hold. Must only be called if there usage_count() is zero"""
        self._mf.close()
    #} END interface
class MapRegionList(list):
    """List of MapRegion instances associating a path with a list of regions."""
        '_path_or_fd',  # path or file descriptor which is mapped by all our regions
        '_file_size'    # total size of the file we map
    def __new__(cls, path):
    def __init__(self, path_or_fd):
        self._path_or_fd = path_or_fd
        self._file_size = None
    def path_or_fd(self):
        """:return: path or file descriptor we are attached to"""
        return self._path_or_fd
    def file_size(self):
        """:return: size of file we manager"""
        if self._file_size is None:
            if isinstance(self._path_or_fd, str):
                self._file_size = os.stat(self._path_or_fd).st_size
                self._file_size = os.fstat(self._path_or_fd).st_size
            # END handle path type
        # END update file size
        return self._file_size
#} END utility classes
This module provides utility methods for dealing with path-specs.
	Collection,
	Sequence)
from dataclasses import (
	dataclass)
	Union)  # Replaced by `X | Y` in 3.10.
	Pattern)
	AnyStr,  # Removed in 3.18.
	deprecated)  # Added in 3.13.
StrPath = Union[str, os.PathLike[str]]
TStrPath = TypeVar("TStrPath", bound=StrPath)
Type variable for :class:`str` or :class:`os.PathLike`.
NORMALIZE_PATH_SEPS = [
	__sep
	for __sep in [os.sep, os.altsep]
	if __sep and __sep != posixpath.sep
*NORMALIZE_PATH_SEPS* (:class:`list` of :class:`str`) contains the path
separators that need to be normalized to the POSIX separator for the current
operating system. The separators are determined by examining :data:`os.sep` and
:data:`os.altsep`.
_registered_patterns = {}
*_registered_patterns* (:class:`dict`) maps a name (:class:`str`) to the
registered pattern factory (:class:`~collections.abc.Callable`).
def append_dir_sep(path: pathlib.Path) -> str:
	Appends the path separator to the path if the path is a directory. This can be
	used to aid in distinguishing between directories and files on the file-system
	by relying on the presence of a trailing path separator.
	*path* (:class:`pathlib.Path`) is the path to use.
	Returns the path (:class:`str`).
	str_path = str(path)
	if path.is_dir():
		str_path += os.sep
	return str_path
def check_match_file(
	patterns: Iterable[tuple[int, Pattern]],
	file: str,
	is_reversed: Optional[bool] = None,
) -> tuple[Optional[bool], Optional[int]]:
	Check the file against the patterns.
	*patterns* (:class:`~collections.abc.Iterable`) yields each indexed pattern
	(:class:`tuple`) which contains the pattern index (:class:`int`) and actua
	pattern (:class:`.Pattern`).
	*file* (:class:`str`) is the normalized file path to be matched against
	*patterns*.
	*is_reversed* (:class:`bool` or :data:`None`) is whether the order of the
	patterns has been reversed. Default is :data:`None` for :data:`False`.
	Reversing the order of the patterns is an optimization.
	Returns a :class:`tuple` containing whether to include *file* (:class:`bool`
	or :data:`None`), and the index of the last matched pattern (:class:`int` or
	:data:`None`).
	if is_reversed:
		# Check patterns in reverse order. The first pattern that matches takes
		for index, pattern in patterns:
			if pattern.include is not None and pattern.match_file(file) is not None:
				return pattern.include, index
		# Check all patterns. The last pattern that matches takes precedence.
		out_include: Optional[bool] = None
		out_index: Optional[int] = None
				out_include = pattern.include
				out_index = index
		return out_include, out_index
def detailed_match_files(
	patterns: Iterable[Pattern],
	files: Iterable[str],
	all_matches: Optional[bool] = None,
) -> dict[str, 'MatchDetail']:
	Matches the files to the patterns, and returns which patterns matched the
	*patterns* (:class:`~collections.abc.Iterable` of :class:`.Pattern`) contains
	the patterns to use.
	*files* (:class:`~collections.abc.Iterable` of :class:`str`) contains the
	normalized file paths to be matched against *patterns*.
	*all_matches* (:class:`bool` or :data:`None`) is whether to return all matches
	patterns (:data:`True`), or only the last matched pattern (:data:`False`).
	Default is :data:`None` for :data:`False`.
	Returns the matched files (:class:`dict`) which maps each matched file
	(:class:`str`) to the patterns that matched in order (:class:`.MatchDetail`).
	all_files = files if isinstance(files, Collection) else list(files)
	return_files = {}
		if pattern.include is not None:
			result_files = pattern.match(all_files)  # TODO: Replace with `.match_file()`.
			if pattern.include:
				# Add files and record pattern.
				for result_file in result_files:
					if result_file in return_files:
						if all_matches:
							return_files[result_file].patterns.append(pattern)
							return_files[result_file].patterns[0] = pattern
						return_files[result_file] = MatchDetail([pattern])
				# Remove files.
				for file in result_files:
					del return_files[file]
	return return_files
def _filter_check_patterns(
) -> list[tuple[int, Pattern]]:
	Filters out null-patterns.
	the patterns.
	Returns a :class:`list` containing each indexed pattern (:class:`tuple`) which
	contains the pattern index (:class:`int`) and the actual pattern
	(:class:`.Pattern`).
		(__index, __pat)
		for __index, __pat in enumerate(patterns)
		if __pat.include is not None
def _is_iterable(value: Any) -> bool:
	Check whether the value is an iterable (excludes strings).
	*value* is the value to check,
	Returns whether *value* is an iterable (:class:`bool`).
	return isinstance(value, Iterable) and not isinstance(value, (str, bytes))
@deprecated((
	"pathspec.util.iter_tree() is deprecated. Use iter_tree_files() instead."
def iter_tree(root, on_error=None, follow_links=None):
	.. version-deprecated:: 0.10.0
		This is an alias for the :func:`.iter_tree_files` function.
	return iter_tree_files(root, on_error=on_error, follow_links=follow_links)
def iter_tree_entries(
	root: StrPath,
	on_error: Optional[Callable[[OSError], None]] = None,
	follow_links: Optional[bool] = None,
) -> Iterator['TreeEntry']:
	Walks the specified directory for all files and directories.
	*root* (:class:`str` or :class:`os.PathLike`) is the root directory to search.
	*on_error* (:class:`~collections.abc.Callable` or :data:`None`) optionally is
	the error handler for file-system exceptions. It will be called with the
	exception (:exc:`OSError`). Reraise the exception to abort the walk. Default
	is :data:`None` to ignore file-system exceptions.
	*follow_links* (:class:`bool` or :data:`None`) optionally is whether to walk
	symbolic links that resolve to directories. Default is :data:`None` for
	:data:`True`.
	Raises :exc:`.RecursionError` if recursion is detected.
	Returns an :class:`~collections.abc.Iterator` yielding each file or directory
	entry (:class:`.TreeEntry`) relative to *root*.
	if on_error is not None and not callable(on_error):
		raise TypeError(f"on_error:{on_error!r} is not callable.")
	if follow_links is None:
		follow_links = True
	yield from _iter_tree_entries_next(os.path.abspath(root), '', {}, on_error, follow_links)
def _iter_tree_entries_next(
	root_full: str,
	dir_rel: str,
	memo: dict[str, str],
	on_error: Callable[[OSError], None],
	follow_links: bool,
	Scan the directory for all descendant files.
	*root_full* (:class:`str`) the absolute path to the root directory.
	*dir_rel* (:class:`str`) the path to the directory to scan relative to
	*root_full*.
	*memo* (:class:`dict`) keeps track of ancestor directories encountered. Maps
	each ancestor real path (:class:`str`) to relative path (:class:`str`).
	the error handler for file-system exceptions.
	*follow_links* (:class:`bool`) is whether to walk symbolic links that resolve
	to directories.
	Yields each entry (:class:`.TreeEntry`).
	dir_full = os.path.join(root_full, dir_rel)
	dir_real = os.path.realpath(dir_full)
	# Remember each encountered ancestor directory and its canonical (real) path.
	# If a canonical path is encountered more than once, recursion has occurred.
	if dir_real not in memo:
		memo[dir_real] = dir_rel
		raise RecursionError(real_path=dir_real, first_path=memo[dir_real], second_path=dir_rel)
	with os.scandir(dir_full) as scan_iter:
		node_ent: os.DirEntry
		for node_ent in scan_iter:
			node_rel = os.path.join(dir_rel, node_ent.name)
			# Inspect child node.
				node_lstat = node_ent.stat(follow_symlinks=False)
				if on_error is not None:
					on_error(e)
			if node_ent.is_symlink():
				# Child node is a link, inspect the target node.
					node_stat = node_ent.stat()
				node_stat = node_lstat
			if node_ent.is_dir(follow_symlinks=follow_links):
				# Child node is a directory, recurse into it and yield its descendant
				# files.
				yield TreeEntry(node_ent.name, node_rel, node_lstat, node_stat)
				yield from _iter_tree_entries_next(root_full, node_rel, memo, on_error, follow_links)
			elif node_ent.is_file() or node_ent.is_symlink():
				# Child node is either a file or an unfollowed link, yield it.
	# NOTE: Make sure to remove the canonical (real) path of the directory from
	# the ancestors memo once we are done with it. This allows the same directory
	# to appear multiple times. If this is not done, the second occurrence of the
	# directory will be incorrectly interpreted as a recursion. See
	# <https://github.com/cpburnz/python-path-specification/pull/7>.
	del memo[dir_real]
def iter_tree_files(
) -> Iterator[str]:
	Walks the specified directory for all files.
	*root* (:class:`str` or :class:`os.PathLike`) is the root directory to search
	for files.
	Returns an :class:`~collections.abc.Iterator` yielding the path to each file
	(:class:`str`) relative to *root*.
	yield from _iter_tree_files_next(os.path.abspath(root), '', {}, on_error, follow_links)
def _iter_tree_files_next(
	Yields each file path (:class:`str`).
				yield from _iter_tree_files_next(root_full, node_rel, memo, on_error, follow_links)
			elif node_ent.is_file():
				# Child node is a file, yield it.
				yield node_rel
			elif not follow_links and node_ent.is_symlink():
				# Child node is an unfollowed link, yield it.
def lookup_pattern(name: str) -> Callable[[AnyStr], Pattern]:
	Lookups a registered pattern factory by name.
	*name* (:class:`str`) is the name of the pattern factory.
	Returns the registered pattern factory (:class:`~collections.abc.Callable`).
	If no pattern factory is registered, raises :exc:`KeyError`.
	return _registered_patterns[name]
def match_file(patterns: Iterable[Pattern], file: str) -> bool:
	Matches the file to the patterns.
	Returns :data:`True` if *file* matched; otherwise, :data:`False`.
	matched = False
			matched = pattern.include
	return matched
	"pathspec.util.match_files() is deprecated. Use match_file() with a loop for "
	"better results."
def match_files(
) -> set[str]:
		This function is no longer used. Use the :func:`.match_file` function with a
		loop for better results.
	Matches the files to the patterns.
	Returns the matched files (:class:`set` of :class:`str`).
	use_patterns = [__pat for __pat in patterns if __pat.include is not None]
	return_files = set()
		if match_file(use_patterns, file):
			return_files.add(file)
def normalize_file(
	file: StrPath,
	separators: Optional[Collection[str]] = None,
	Normalizes the file path to use the POSIX path separator (i.e., ``"/"``), and
	make the paths relative (remove leading ``"/"``).
	*file* (:class:`str` or :class:`os.PathLike`) is the file path.
	*separators* (:class:`~collections.abc.Collection` of :class:`str`; or
	:data:`None`) optionally contains the path separators to normalize. This does
	not need to include the POSIX path separator (``"/"``), but including it will
	not affect the results. Default is ``None`` for :data:`.NORMALIZE_PATH_SEPS`.
	To prevent normalization, pass an empty container (e.g., an empty tuple
	``()``).
	Returns the normalized file path (:class:`str`).
	# Normalize path separators.
	if separators is None:
		separators = NORMALIZE_PATH_SEPS
	# Convert path object to string.
	norm_file: str = os.fspath(file)
	for sep in separators:
		norm_file = norm_file.replace(sep, posixpath.sep)
	if norm_file.startswith('/'):
		# Make path relative.
		norm_file = norm_file[1:]
	elif norm_file.startswith('./'):
		# Remove current directory prefix.
		norm_file = norm_file[2:]
	return norm_file
	"pathspec.util.normalize_files() is deprecated. Use normalize_file() with a "
	"loop for better results."
def normalize_files(
	files: Iterable[StrPath],
) -> dict[str, list[StrPath]]:
		This function is no longer used. Use the :func:`.normalize_file` function
		with a loop for better results.
	Normalizes the file paths to use the POSIX path separator.
	*files* (:class:`~collections.abc.Iterable` of :class:`str` or
	:class:`os.PathLike`) contains the file paths to be normalized.
	:data:`None`) optionally contains the path separators to normalize. See
	:func:`.normalize_file` for more information.
	Returns a :class:`dict` mapping each normalized file path (:class:`str`) to
	the original file paths (:class:`list` of :class:`str` or
	:class:`os.PathLike`).
	norm_files = {}
		norm_file = normalize_file(path, separators=separators)
		if norm_file in norm_files:
			norm_files[norm_file].append(path)
			norm_files[norm_file] = [path]
	return norm_files
def register_pattern(
	pattern_factory: Callable[[AnyStr], Pattern],
	override: Optional[bool] = None,
	Registers the specified pattern factory.
	*name* (:class:`str`) is the name to register the pattern factory under.
	*pattern_factory* (:class:`~collections.abc.Callable`) is used to compile
	patterns. It must accept an uncompiled pattern (:class:`str`) and return the
	compiled pattern (:class:`.Pattern`).
	*override* (:class:`bool` or :data:`None`) optionally is whether to allow
	overriding an already registered pattern under the same name (:data:`True`),
	instead of raising an :exc:`.AlreadyRegisteredError` (:data:`False`). Default
	is :data:`None` for :data:`False`.
		raise TypeError(f"name:{name!r} is not a string.")
	if not callable(pattern_factory):
		raise TypeError(f"pattern_factory:{pattern_factory!r} is not callable.")
	if name in _registered_patterns and not override:
		raise AlreadyRegisteredError(name, _registered_patterns[name])
	_registered_patterns[name] = pattern_factory
class AlreadyRegisteredError(Exception):
	The :exc:`AlreadyRegisteredError` exception is raised when a pattern factory
	is registered under a name already in use.
		Initializes the :exc:`AlreadyRegisteredError` instance.
		*name* (:class:`str`) is the name of the registered pattern.
		*pattern_factory* (:class:`~collections.abc.Callable`) is the registered
		pattern factory.
		super().__init__(name, pattern_factory)
	def message(self) -> str:
			f"{self.name!r} is already registered for pattern factory="
			f"{self.pattern_factory!r}."
	def pattern_factory(self) -> Callable[[AnyStr], Pattern]:
class RecursionError(Exception):
	The :exc:`RecursionError` exception is raised when recursion is detected.
		real_path: str,
		first_path: str,
		second_path: str,
		Initializes the :exc:`RecursionError` instance.
		*real_path* (:class:`str`) is the real path that recursion was encountered
		*first_path* (:class:`str`) is the first path encountered for *real_path*.
		*second_path* (:class:`str`) is the second path encountered for *real_path*.
		super().__init__(real_path, first_path, second_path)
	def first_path(self) -> str:
		*first_path* (:class:`str`) is the first path encountered for
		:attr:`self.real_path <RecursionError.real_path>`.
			f"Real path {self.real_path!r} was encountered at {self.first_path!r} "
			f"and then {self.second_path!r}."
	def real_path(self) -> str:
		*real_path* (:class:`str`) is the real path that recursion was
		encountered on.
	def second_path(self) -> str:
		*second_path* (:class:`str`) is the second path encountered for
class CheckResult(Generic[TStrPath]):
	The :class:`CheckResult` class contains information about the file and which
	pattern matched it.
	# Make the class dict-less.
		'include',
		'index',
	file: TStrPath
	include: Optional[bool]
	*include* (:class:`bool` or :data:`None`) is whether to include or exclude the
	file. If :data:`None`, no pattern matched.
	index: Optional[int]
	*index* (:class:`int` or :data:`None`) is the index of the last pattern that
	matched. If :data:`None`, no pattern matched.
class MatchDetail(object):
	The :class:`.MatchDetail` class contains information about
	__slots__ = ('patterns',)
	def __init__(self, patterns: Sequence[Pattern]) -> None:
		Initialize the :class:`.MatchDetail` instance.
		*patterns* (:class:`~collections.abc.Sequence` of :class:`.Pattern`)
		contains the patterns that matched the file in the order they were encountered.
		self.patterns = patterns
		contains the patterns that matched the file in the order they were
class TreeEntry(object):
	The :class:`TreeEntry` class contains information about a file-system entry.
	__slots__ = ('_lstat', 'name', 'path', '_stat')
		lstat: os.stat_result,
		stat: os.stat_result,
		Initialize the :class:`TreeEntry` instance.
		*name* (:class:`str`) is the base name of the entry.
		*path* (:class:`str`) is the relative path of the entry.
		*lstat* (:class:`os.stat_result`) is the stat result of the direct entry.
		*stat* (:class:`os.stat_result`) is the stat result of the entry,
		potentially linked.
		self._lstat: os.stat_result = lstat
		*_lstat* (:class:`os.stat_result`) is the stat result of the direct entry.
		self.path: str = path
		*path* (:class:`str`) is the path of the entry.
		self._stat: os.stat_result = stat
		*_stat* (:class:`os.stat_result`) is the stat result of the linked entry.
	def is_dir(self, follow_links: Optional[bool] = None) -> bool:
		Get whether the entry is a directory.
		*follow_links* (:class:`bool` or :data:`None`) is whether to follow symbolic
		links. If this is :data:`True`, a symlink to a directory will result in
		:data:`True`. Default is :data:`None` for :data:`True`.
		Returns whether the entry is a directory (:class:`bool`).
		node_stat = self._stat if follow_links else self._lstat
		return stat.S_ISDIR(node_stat.st_mode)
	def is_file(self, follow_links: Optional[bool] = None) -> bool:
		Get whether the entry is a regular file.
		links. If this is :data:`True`, a symlink to a regular file will result in
		Returns whether the entry is a regular file (:class:`bool`).
		return stat.S_ISREG(node_stat.st_mode)
	def is_symlink(self) -> bool:
		Returns whether the entry is a symbolic link (:class:`bool`).
		return stat.S_ISLNK(self._lstat.st_mode)
	def stat(self, follow_links: Optional[bool] = None) -> os.stat_result:
		Get the cached stat result for the entry.
		links. If this is :data:`True`, the stat result of the linked file will be
		returned. Default is :data:`None` for :data:`True`.
		Returns that stat result (:class:`os.stat_result`).
		return self._stat if follow_links else self._lstat
Utility functions for
- building and importing modules on test time, using a temporary location
- detecting if compilers are present
- determining paths to tests
from numpy.compat import asstr
from numpy._utils import asunicode
from numpy.testing import temppath, IS_WASM
# Maintaining a temporary module directory
_module_dir = None
_module_num = 5403
if sys.platform == "cygwin":
    NUMPY_INSTALL_ROOT = Path(__file__).parent.parent.parent
    _module_list = list(NUMPY_INSTALL_ROOT.glob("**/*.dll"))
def _cleanup():
    global _module_dir
    if _module_dir is not None:
            sys.path.remove(_module_dir)
            shutil.rmtree(_module_dir)
def get_module_dir():
    if _module_dir is None:
        _module_dir = tempfile.mkdtemp()
        atexit.register(_cleanup)
        if _module_dir not in sys.path:
            sys.path.insert(0, _module_dir)
    return _module_dir
def get_temp_module_name():
    # Assume single-threaded, and the module dir usable only by this thread
    global _module_num
    get_module_dir()
    name = "_test_ext_module_%d" % _module_num
    _module_num += 1
    if name in sys.modules:
        # this should not be possible, but check anyway
        raise RuntimeError("Temporary module name already in use.")
def _memoize(func):
    def wrapper(*a, **kw):
        key = repr((a, kw))
        if key not in memo:
                memo[key] = func(*a, **kw)
                memo[key] = e
        ret = memo[key]
        if isinstance(ret, Exception):
            raise ret
    wrapper.__name__ = func.__name__
# Building modules
@_memoize
def build_module(source_files, options=[], skip=[], only=[], module_name=None):
    Compile and import a f2py module, built from the given files.
    code = f"import sys; sys.path = {sys.path!r}; import numpy.f2py; numpy.f2py.main()"
    d = get_module_dir()
    # Copy files
    dst_sources = []
    f2py_sources = []
    for fn in source_files:
        if not os.path.isfile(fn):
            raise RuntimeError("%s is not a file" % fn)
        dst = os.path.join(d, os.path.basename(fn))
        shutil.copyfile(fn, dst)
        dst_sources.append(dst)
        base, ext = os.path.splitext(dst)
        if ext in (".f90", ".f", ".c", ".pyf"):
            f2py_sources.append(dst)
    assert f2py_sources
    # Prepare options
    if module_name is None:
        module_name = get_temp_module_name()
    f2py_opts = ["-c", "-m", module_name] + options + f2py_sources
    if skip:
        f2py_opts += ["skip:"] + skip
        f2py_opts += ["only:"] + only
    # Build
        cmd = [sys.executable, "-c", code] + f2py_opts
        p = subprocess.Popen(cmd,
                             stderr=subprocess.STDOUT)
        out, err = p.communicate()
            raise RuntimeError("Running f2py failed: %s\n%s" %
                               (cmd[4:], asunicode(out)))
        # Partial cleanup
        for fn in dst_sources:
    # Rebase (Cygwin-only)
        # If someone starts deleting modules after import, this will
        # need to change to record how big each module is, rather than
        # relying on rebase being able to find that from the files.
        _module_list.extend(
            glob.glob(os.path.join(d, "{:s}*".format(module_name)))
            ["/usr/bin/rebase", "--database", "--oblivious", "--verbose"]
            + _module_list
    # Import
    return import_module(module_name)
def build_code(source_code,
               options=[],
               skip=[],
               only=[],
               suffix=None,
               module_name=None):
    Compile and import Fortran code using f2py.
        suffix = ".f"
    with temppath(suffix=suffix) as path:
        with open(path, "w") as f:
            f.write(source_code)
        return build_module([path],
                            only=only,
                            module_name=module_name)
# Check if compilers are available at all...
_compiler_status = None
def _get_compiler_status():
    global _compiler_status
    if _compiler_status is not None:
        return _compiler_status
    _compiler_status = (False, False, False)
    if IS_WASM:
        # Can't run compiler from inside WASM.
    # XXX: this is really ugly. But I don't know how to invoke Distutils
    #      in a safer way...
    code = textwrap.dedent(f"""\
        sys.path = {repr(sys.path)}
        def configuration(parent_name='',top_path=None):
            global config
            config = Configuration('', parent_name, top_path)
        config_cmd = config.get_config_cmd()
        have_c = config_cmd.try_compile('void foo() {{}}')
        print('COMPILERS:%%d,%%d,%%d' %% (have_c,
                                          config.have_f77c(),
                                          config.have_f90c()))
        sys.exit(99)
    code = code % dict(syspath=repr(sys.path))
        script = os.path.join(tmpdir, "setup.py")
        with open(script, "w") as f:
            f.write(code)
        cmd = [sys.executable, "setup.py", "config"]
                             cwd=tmpdir)
    m = re.search(br"COMPILERS:(\d+),(\d+),(\d+)", out)
        _compiler_status = (
            bool(int(m.group(1))),
            bool(int(m.group(2))),
            bool(int(m.group(3))),
    # Finished
def has_c_compiler():
    return _get_compiler_status()[0]
def has_f77_compiler():
    return _get_compiler_status()[1]
def has_f90_compiler():
    return _get_compiler_status()[2]
# Building with distutils
def build_module_distutils(source_files, config_code, module_name, **kw):
    Build a module via distutils and import it.
    # Build script
    config_code = textwrap.dedent(config_code).replace("\n", "\n    ")
    code = fr"""
    {config_code}
    script = os.path.join(d, get_temp_module_name() + ".py")
    dst_sources.append(script)
    with open(script, "wb") as f:
        f.write(code.encode('latin1'))
        cmd = [sys.executable, script, "build_ext", "-i"]
            raise RuntimeError("Running distutils build failed: %s\n%s" %
                               (cmd[4:], asstr(out)))
    return sys.modules[module_name]
# Unittest convenience
class F2PyTest:
    code = None
    sources = None
    skip = []
    only = []
    module = None
    def module_name(self):
        cls = type(self)
        return f'_{cls.__module__.rsplit(".",1)[-1]}_{cls.__name__}_ext_module'
    def setup_method(self):
            pytest.skip("Fails with MinGW64 Gfortran (Issue #9673)")
        if self.module is not None:
        # Check compiler availability first
        if not has_c_compiler():
            pytest.skip("No C compiler available")
        codes = []
        if self.sources:
            codes.extend(self.sources)
        if self.code is not None:
            codes.append(self.suffix)
        needs_f77 = False
        needs_f90 = False
        needs_pyf = False
        for fn in codes:
            if str(fn).endswith(".f"):
                needs_f77 = True
            elif str(fn).endswith(".f90"):
                needs_f90 = True
            elif str(fn).endswith(".pyf"):
                needs_pyf = True
        if needs_f77 and not has_f77_compiler():
            pytest.skip("No Fortran 77 compiler available")
        if needs_f90 and not has_f90_compiler():
            pytest.skip("No Fortran 90 compiler available")
        if needs_pyf and not (has_f90_compiler() or has_f77_compiler()):
            pytest.skip("No Fortran compiler available")
        # Build the module
            self.module = build_code(
                options=self.options,
                skip=self.skip,
                only=self.only,
                suffix=self.suffix,
                module_name=self.module_name,
        if self.sources is not None:
            self.module = build_module(
                self.sources,
# Helper functions
def getpath(*a):
    # Package root
    d = Path(numpy.f2py.__file__).parent.resolve()
    return d.joinpath(*a)
def switchdir(path):
    curpath = Path.cwd()
        os.chdir(curpath)
from importlib.machinery import ModuleSpec
from ..abc import ResourceReader, Traversable, TraversableResources
from . import _path
from . import zip as zip_
class Reader(ResourceReader):
    def get_resource_reader(self, package):
    def open_resource(self, path):
        if isinstance(self.file, Exception):
            raise self.file
        return self.file
    def resource_path(self, path_):
        self._path = path_
        if isinstance(self.path, Exception):
            raise self.path
    def is_resource(self, path_):
        def part(entry):
            return entry.split('/')
            len(parts) == 1 and parts[0] == path_ for parts in map(part, self._contents)
        yield from self._contents
def create_package_from_loader(loader, is_package=True):
    name = 'testingpackage'
    module = types.ModuleType(name)
    spec = ModuleSpec(name, loader, origin='does-not-exist', is_package=is_package)
    module.__spec__ = spec
    module.__loader__ = loader
def create_package(file=None, path=None, is_package=True, contents=()):
    return create_package_from_loader(
        Reader(file=file, path=path, _contents=contents),
        is_package,
class CommonTestsBase(metaclass=abc.ABCMeta):
    Tests shared by test_open, test_path, and test_read.
        Call the pertinent legacy API function (e.g. open_text, path)
        on package and path.
    def test_package_name(self):
        Passing in the package name should succeed.
        self.execute(self.data.__name__, 'utf-8.file')
    def test_package_object(self):
        Passing in the package itself should succeed.
        self.execute(self.data, 'utf-8.file')
    def test_string_path(self):
        Passing in a string for the path should succeed.
        path = 'utf-8.file'
        self.execute(self.data, path)
    def test_pathlib_path(self):
        Passing in a pathlib.PurePath object for the path should succeed.
        path = pathlib.PurePath('utf-8.file')
    def test_importing_module_as_side_effect(self):
        The anchor package can already be imported.
        del sys.modules[self.data.__name__]
    def test_missing_path(self):
        Attempting to open or read or request the path for a
        non-existent path should succeed if open_resource
        can return a viable data stream.
        bytes_data = io.BytesIO(b'Hello, world!')
        package = create_package(file=bytes_data, path=FileNotFoundError())
        self.execute(package, 'utf-8.file')
        self.assertEqual(package.__loader__._path, 'utf-8.file')
    def test_extant_path(self):
        # Attempting to open or read or request the path when the
        # path does exist should still succeed. Does not assert
        # anything about the result.
        # any path that exists
        path = __file__
        package = create_package(file=bytes_data, path=path)
    def test_useless_loader(self):
        package = create_package(file=FileNotFoundError(), path=FileNotFoundError())
fixtures = dict(
    data01={
        '__init__.py': '',
        'binary.file': bytes(range(4)),
        'utf-16.file': '\ufeffHello, UTF-16 world!\n'.encode('utf-16-le'),
        'utf-8.file': 'Hello, UTF-8 world!\n'.encode('utf-8'),
        'subdirectory': {
            'binary.file': bytes(range(4, 8)),
    data02={
        'one': {'__init__.py': '', 'resource1.txt': 'one resource'},
        'two': {'__init__.py': '', 'resource2.txt': 'two resource'},
        'subdirectory': {'subsubdir': {'resource.txt': 'a resource'}},
    namespacedata01={
            'binary.file': bytes(range(12, 16)),
class ModuleSetup:
        self.fixtures = contextlib.ExitStack()
        self.addCleanup(self.fixtures.close)
        self.fixtures.enter_context(import_helper.isolated_modules())
        self.data = self.load_fixture(self.MODULE)
    def load_fixture(self, module):
        self.tree_on_path({module: fixtures[module]})
class ZipSetup(ModuleSetup):
    MODULE = 'data01'
    def tree_on_path(self, spec):
        temp_dir = self.fixtures.enter_context(os_helper.temp_dir())
        modules = pathlib.Path(temp_dir) / 'zipped modules.zip'
        self.fixtures.enter_context(
            import_helper.DirsOnSysPath(str(zip_.make_zip_file(spec, modules)))
class DiskSetup(ModuleSetup):
        _path.build(spec, pathlib.Path(temp_dir))
        self.fixtures.enter_context(import_helper.DirsOnSysPath(temp_dir))
class MemorySetup(ModuleSetup):
    """Support loading a module in memory."""
        self.fixtures.enter_context(self.augment_sys_metapath(module))
    def augment_sys_metapath(self, module):
        finder_instance = self.MemoryFinder(module)
        sys.meta_path.append(finder_instance)
        sys.meta_path.remove(finder_instance)
    class MemoryFinder(importlib.abc.MetaPathFinder):
            self._module = module
            if fullname != self._module:
            return importlib.machinery.ModuleSpec(
                name=fullname,
                loader=MemorySetup.MemoryLoader(self._module),
                is_package=True,
    class MemoryLoader(importlib.abc.Loader):
        def get_resource_reader(self, fullname):
            return MemorySetup.MemoryTraversableResources(self._module, fullname)
    class MemoryTraversableResources(TraversableResources):
        def __init__(self, module, fullname):
            self._fullname = fullname
            return MemorySetup.MemoryTraversable(self._module, self._fullname)
    class MemoryTraversable(Traversable):
        """Implement only the abstract methods of `Traversable`.
        Besides `.__init__()`, no other methods may be implemented or overridden.
        This is critical for validating the concrete `Traversable` implementations.
        def _resolve(self):
            Fully traverse the `fixtures` dictionary.
            This should be wrapped in a `try/except KeyError`
            but it is not currently needed and lowers the code coverage numbers.
            path = pathlib.PurePosixPath(self._fullname)
            return functools.reduce(lambda d, p: d[p], path.parts, fixtures)
            directory = self._resolve()
            if not isinstance(directory, dict):
                # Filesystem openers raise OSError, and that exception is mirrored here.
                raise OSError(f"{self._fullname} is not a directory")
            for path in directory:
                yield MemorySetup.MemoryTraversable(
                    self._module, f"{self._fullname}/{path}"
        def is_dir(self) -> bool:
            return isinstance(self._resolve(), dict)
        def is_file(self) -> bool:
            return not self.is_dir()
        def open(self, mode='r', encoding=None, errors=None, *_, **__):
            contents = self._resolve()
            if isinstance(contents, dict):
                # Filesystem openers raise OSError when attempting to open a directory,
                # and that exception is mirrored here.
                raise OSError(f"{self._fullname} is a directory")
            if isinstance(contents, str):
                contents = contents.encode("utf-8")
            result = io.BytesIO(contents)
            if "b" in mode:
            return io.TextIOWrapper(result, encoding=encoding, errors=errors)
            return pathlib.PurePosixPath(self._fullname).name
class CommonTests(DiskSetup, CommonTestsBase):
from smmap import (
    StaticWindowMapManager,
    SlidingWindowMapManager,
    SlidingWindowMapBuffer
# initialize our global memory manager instance
# Use it to free cached (and unused) resources.
mman = SlidingWindowMapManager()
# END handle mman
    from struct import unpack_from
    from struct import unpack, calcsize
    __calcsize_cache = dict()
    def unpack_from(fmt, data, offset=0):
            size = __calcsize_cache[fmt]
            size = calcsize(fmt)
            __calcsize_cache[fmt] = size
        # END exception handling
        return unpack(fmt, data[offset: offset + size])
    # END own unpack_from implementation
#{ Aliases
hex_to_bin = binascii.a2b_hex
bin_to_hex = binascii.b2a_hex
# errors
ENOENT = errno.ENOENT
# os shortcuts
exists = os.path.exists
mkdir = os.mkdir
chmod = os.chmod
isdir = os.path.isdir
isfile = os.path.isfile
rename = os.rename
dirname = os.path.dirname
basename = os.path.basename
join = os.path.join
read = os.read
write = os.write
close = os.close
fsync = os.fsync
def _retry(func, *args, **kwargs):
    # Wrapper around functions, that are problematic on "Windows". Sometimes
    # the OS or someone else has still a handle to the file
        for _ in range(10):
def remove(*args, **kwargs):
    return _retry(os.remove, *args, **kwargs)
# Backwards compatibility imports
from gitdb.const import (
    NULL_BIN_SHA,
    NULL_HEX_SHA
#} END Aliases
#{ compatibility stuff ...
class _RandomAccessBytesIO:
    """Wrapper to provide required functionality in case memory maps cannot or may
    not be used. This is only really required in python 2.4"""
    __slots__ = '_sio'
    def __init__(self, buf=''):
        self._sio = BytesIO(buf)
        return getattr(self._sio, attr)
        return len(self.getvalue())
        return self.getvalue()[i]
    def __getslice__(self, start, end):
        return self.getvalue()[start:end]
def byte_ord(b):
    Return the integer representation of the byte string.  This supports Python
    3 byte arrays as well as standard strings.
        return ord(b)
#} END compatibility stuff ...
#{ Routines
def make_sha(source=b''):
    """A python2.4 workaround for the sha/hashlib module fiasco
    **Note** From the dulwich project """
        return hashlib.sha1(source)
        import sha
        sha1 = sha.sha(source)
        return sha1
def allocate_memory(size):
    """:return: a file-protocol accessible memory block of the given size"""
        return _RandomAccessBytesIO(b'')
    # END handle empty chunks gracefully
        return mmap.mmap(-1, size)  # read-write by default
        # setup real memory instead
        # this of course may fail if the amount of memory is not available in
        # one chunk - would only be the case in python 2.4, being more likely on
        # 32 bit systems.
        return _RandomAccessBytesIO(b"\0" * size)
    # END handle memory allocation
def file_contents_ro(fd, stream=False, allow_mmap=True):
    """:return: read-only contents of the file represented by the file descriptor fd
    :param fd: file descriptor opened for reading
    :param stream: if False, random access is provided, otherwise the stream interface
        is provided.
    :param allow_mmap: if True, its allowed to map the contents into memory, which
        allows large files to be handled and accessed efficiently. The file-descriptor
        will change its position if this is False"""
        if allow_mmap:
            # supports stream and random access
                return mmap.mmap(fd, 0, access=mmap.ACCESS_READ)
                # python 2.4 issue, 0 wants to be the actual size
                return mmap.mmap(fd, os.fstat(fd).st_size, access=mmap.ACCESS_READ)
            # END handle python 2.4
    # read manually
    contents = os.read(fd, os.fstat(fd).st_size)
    if stream:
        return _RandomAccessBytesIO(contents)
    return contents
def file_contents_ro_filepath(filepath, stream=False, allow_mmap=True, flags=0):
    """Get the file contents at filepath as fast as possible
    :return: random access compatible memory of the given filepath
    :param stream: see ``file_contents_ro``
    :param allow_mmap: see ``file_contents_ro``
    :param flags: additional flags to pass to os.open
    :raise OSError: If the file could not be opened
    **Note** for now we don't try to use O_NOATIME directly as the right value needs to be
    shared per database in fact. It only makes a real difference for loose object
    databases anyway, and they use it with the help of the ``flags`` parameter"""
    fd = os.open(filepath, os.O_RDONLY | getattr(os, 'O_BINARY', 0) | flags)
        return file_contents_ro(fd, stream, allow_mmap)
        close(fd)
    # END assure file is closed
def sliding_ro_buffer(filepath, flags=0):
    :return: a buffer compatible object which uses our mapped memory manager internally
        ready to read the whole given filepath"""
    return SlidingWindowMapBuffer(mman.make_cursor(filepath), flags=flags)
def to_hex_sha(sha):
    """:return: hexified version  of sha"""
    if len(sha) == 40:
        return sha
    return bin_to_hex(sha)
def to_bin_sha(sha):
    if len(sha) == 20:
    return hex_to_bin(sha)
#} END routines
class LazyMixin:
    Base class providing an interface to lazily retrieve attribute values upon
    first access. If slots are used, memory will only be reserved once the attribute
    is actually accessed and retrieved the first time. All future accesses will
    return the cached value as stored in the Instance's dict or slot.
        Whenever an attribute is requested that we do not know, we allow it
        to be created and set. Next time the same attribute is requested, it is simply
        returned from our dict/slots. """
        self._set_cache_(attr)
        # will raise in case the cache was not created
        return object.__getattribute__(self, attr)
    def _set_cache_(self, attr):
        This method should be overridden in the derived class.
        It should check whether the attribute named by attr can be created
        and cached. Do nothing if you do not know the attribute or call your subclass
        The derived class may create as many additional attributes as it deems
        necessary in case a git command returns more information than represented
        in the single attribute."""
class LockedFD:
    This class facilitates a safe read and write operation to a file on disk.
    If we write to 'file', we obtain a lock file at 'file.lock' and write to
    that instead. If we succeed, the lock file will be renamed to overwrite
    the original file.
    When reading, we obtain a lock file, but to prevent other writers from
    succeeding while we are reading the file.
    This type handles error correctly in that it will assure a consistent state
    on destruction.
    **note** with this setup, parallel reading is not possible"""
    __slots__ = ("_filepath", '_fd', '_write')
    def __init__(self, filepath):
        """Initialize an instance with the givne filepath"""
        self._filepath = filepath
        self._write = None          # if True, we write a file
        # will do nothing if the file descriptor is already closed
        if self._fd is not None:
            self.rollback()
    def _lockfilepath(self):
        return "%s.lock" % self._filepath
    def open(self, write=False, stream=False):
        Open the file descriptor for reading or writing, both in binary mode.
        :param write: if True, the file descriptor will be opened for writing. Other
            wise it will be opened read-only.
        :param stream: if True, the file descriptor will be wrapped into a simple stream
            object which supports only reading or writing
        :return: fd to read from or write to. It is still maintained by this instance
            and must not be closed directly
        :raise IOError: if the lock could not be retrieved
        :raise OSError: If the actual file could not be opened for reading
        **note** must only be called once"""
        if self._write is not None:
            raise AssertionError("Called %s multiple times" % self.open)
        self._write = write
        # try to open the lock file
        binary = getattr(os, 'O_BINARY', 0)
        lockmode = os.O_WRONLY | os.O_CREAT | os.O_EXCL | binary
            fd = os.open(self._lockfilepath(), lockmode, int("600", 8))
            if not write:
            # END handle file descriptor
            raise OSError("Lock at %r could not be obtained" % self._lockfilepath()) from e
        # END handle lock retrieval
        # open actual file if required
        if self._fd is None:
            # we could specify exclusive here, as we obtained the lock anyway
                self._fd = os.open(self._filepath, os.O_RDONLY | binary)
                # assure we release our lockfile
                remove(self._lockfilepath())
            # END handle lockfile
        # END open descriptor for reading
            # need delayed import
            from gitdb.stream import FDStream
            return FDStream(self._fd)
        # END handle stream
        """When done writing, call this function to commit your changes into the
        actual file.
        The file descriptor will be closed, and the lockfile handled.
        **Note** can be called multiple times"""
        self._end_writing(successful=True)
        """Abort your operation without any changes. The file descriptor will be
        closed, and the lock released.
        self._end_writing(successful=False)
    def _end_writing(self, successful=True):
        """Handle the lock according to the write mode """
        if self._write is None:
            raise AssertionError("Cannot end operation if it wasn't started yet")
        os.close(self._fd)
        lockfile = self._lockfilepath()
        if self._write and successful:
            # on windows, rename does not silently overwrite the existing one
                if isfile(self._filepath):
                    remove(self._filepath)
                # END remove if exists
            # END win32 special handling
            os.rename(lockfile, self._filepath)
            # assure others can at least read the file - the tmpfile left it at rw--
            # We may also write that file, on windows that boils down to a remove-
            # protection as well
            chmod(self._filepath, int("644", 8))
            # just delete the file so far, we failed
            remove(lockfile)
        # END successful handling
#} END utilities
    "stream_copy",
    "join_path",
    "to_native_path_linux",
    "join_path_native",
    "IndexFileSHA1Writer",
    "IterableObj",
    "IterableList",
    "get_user_id",
    "assure_directory_exists",
    "CallableRemoteProgress",
    "unbare_repo",
    "HIDE_WINDOWS_KNOWN_ERRORS",
    __all__.append("to_native_path_windows")
from urllib.parse import urlsplit, urlunsplit
# NOTE: Unused imports can be improved now that CI testing has fully resumed. Some of
# these be used indirectly through other GitPython modules, which avoids having to write
# gitdb all the time in their imports. They are not in __all__, at least currently,
# because they could be removed or changed at any time, and so should not be considered
# conceptually public to code outside GitPython. Linters of course do not like it.
    LazyMixin,  # noqa: F401
    LockedFD,  # noqa: F401
    bin_to_hex,  # noqa: F401
    file_contents_ro,  # noqa: F401
    file_contents_ro_filepath,  # noqa: F401
    hex_to_bin,  # noqa: F401
    make_sha,
    to_bin_sha,  # noqa: F401
    to_hex_sha,  # noqa: F401
# typing ---------------------------------------------------------
    AnyStr,
    BinaryIO,
    from git.cmd import Git
    from git.config import GitConfigParser, SectionConstraint
    from git.remote import Remote
from git.types import (
    Files_TD,
    Has_id_attribute,
    HSH_TD,
    PathLike,
    Total_TD,
# ---------------------------------------------------------------------
T_IterableObj = TypeVar("T_IterableObj", bound=Union["IterableObj", "Has_id_attribute"], covariant=True)
# So IterableList[Head] is subtype of IterableList[IterableObj].
def _read_env_flag(name: str, default: bool) -> bool:
    """Read a boolean flag from an environment variable.
        The flag, or the `default` value if absent or ambiguous.
        value = os.environ[name]
    _logger.warning(
        "The %s environment variable is deprecated. Its effect has never been documented and changes without warning.",
    adjusted_value = value.strip().lower()
    if adjusted_value in {"", "0", "false", "no"}:
    if adjusted_value in {"1", "true", "yes"}:
    _logger.warning("%s has unrecognized value %r, treating as %r.", name, value, default)
def _read_win_env_flag(name: str, default: bool) -> bool:
    """Read a boolean flag from an environment variable on Windows.
        On Windows, the flag, or the `default` value if absent or ambiguous.
        On all other operating systems, ``False``.
        This only accesses the environment on Windows.
    return sys.platform == "win32" and _read_env_flag(name, default)
#: We need an easy way to see if Appveyor TCs start failing,
#: so the errors marked with this var are considered "acknowledged" ones, awaiting remedy,
#: till then, we wish to hide them.
HIDE_WINDOWS_KNOWN_ERRORS = _read_win_env_flag("HIDE_WINDOWS_KNOWN_ERRORS", True)
HIDE_WINDOWS_FREEZE_ERRORS = _read_win_env_flag("HIDE_WINDOWS_FREEZE_ERRORS", True)
# { Utility Methods
def unbare_repo(func: Callable[..., T]) -> Callable[..., T]:
    """Methods with this decorator raise :exc:`~git.exc.InvalidGitRepositoryError` if
    they encounter a bare repository."""
    from .exc import InvalidGitRepositoryError
    def wrapper(self: "Remote", *args: Any, **kwargs: Any) -> T:
        if self.repo.bare:
            raise InvalidGitRepositoryError("Method '%s' cannot operate on bare repositories" % func.__name__)
        # END bare method
    # END wrapper
def cwd(new_dir: PathLike) -> Generator[PathLike, None, None]:
    """Context manager to temporarily change directory.
    This is similar to :func:`contextlib.chdir` introduced in Python 3.11, but the
    context manager object returned by a single call to this function is not reentrant.
    old_dir = os.getcwd()
    os.chdir(new_dir)
        yield new_dir
        os.chdir(old_dir)
def patch_env(name: str, value: str) -> Generator[None, None, None]:
    """Context manager to temporarily patch an environment variable."""
    old_value = os.getenv(name)
            del os.environ[name]
            os.environ[name] = old_value
def rmtree(path: PathLike) -> None:
    """Remove the given directory tree recursively.
        We use :func:`shutil.rmtree` but adjust its behaviour to see whether files that
        couldn't be deleted are read-only. Windows will not remove them in that case.
    def handler(function: Callable, path: PathLike, _excinfo: Any) -> None:
        """Callback for :func:`shutil.rmtree`.
        This works as either a ``onexc`` or ``onerror`` style callback.
        # Is the error an access error?
        os.chmod(path, stat.S_IWUSR)
            function(path)
        except PermissionError as ex:
            if HIDE_WINDOWS_KNOWN_ERRORS:
                raise SkipTest(f"FIXME: fails with: PermissionError\n  {ex}") from ex
    elif sys.version_info >= (3, 12):
        shutil.rmtree(path, onexc=handler)
        shutil.rmtree(path, onerror=handler)
def rmfile(path: PathLike) -> None:
    """Ensure file deleted also on *Windows* where read-only files need special
    treatment."""
    if osp.isfile(path):
            os.chmod(path, 0o777)
def stream_copy(source: BinaryIO, destination: BinaryIO, chunk_size: int = 512 * 1024) -> int:
    """Copy all data from the `source` stream into the `destination` stream in chunks
        Number of bytes written
    br = 0
        chunk = source.read(chunk_size)
        destination.write(chunk)
        br += len(chunk)
        if len(chunk) < chunk_size:
    # END reading output stream
    return br
def join_path(a: PathLike, *p: PathLike) -> PathLike:
    R"""Join path tokens together similar to osp.join, but always use ``/`` instead of
    possibly ``\`` on Windows."""
    path = os.fspath(a)
    for b in p:
        b = os.fspath(b)
        if not b:
        if b.startswith("/"):
            path += b[1:]
        elif path == "" or path.endswith("/"):
            path += b
            path += "/" + b
    # END for each path token to add
    def to_native_path_windows(path: PathLike) -> PathLike:
        path = os.fspath(path)
        return path.replace("/", "\\")
    def to_native_path_linux(path: PathLike) -> str:
        return path.replace("\\", "/")
    to_native_path = to_native_path_windows
    # No need for any work on Linux.
        return os.fspath(path)
    to_native_path = to_native_path_linux
def join_path_native(a: PathLike, *p: PathLike) -> PathLike:
    R"""Like :func:`join_path`, but makes sure an OS native path is returned.
    This is only needed to play it safe on Windows and to ensure nice paths that only
    use ``\``.
    return to_native_path(join_path(a, *p))
def assure_directory_exists(path: PathLike, is_file: bool = False) -> bool:
    """Make sure that the directory pointed to by path exists.
    :param is_file:
        If ``True``, `path` is assumed to be a file and handled correctly.
        Otherwise it must be a directory.
        ``True`` if the directory was created, ``False`` if it already existed.
    if is_file:
        path = osp.dirname(path)
    # END handle file
    if not osp.isdir(path):
def _get_exe_extensions() -> Sequence[str]:
    PATHEXT = os.environ.get("PATHEXT", None)
    if PATHEXT:
        return tuple(p.upper() for p in PATHEXT.split(os.pathsep))
        return (".BAT", ".COM", ".EXE")
def py_where(program: str, path: Optional[PathLike] = None) -> List[str]:
    """Perform a path search to assist :func:`is_cygwin_git`.
    This is not robust for general use. It is an implementation detail of
    :func:`is_cygwin_git`. When a search following all shell rules is needed,
    :func:`shutil.which` can be used instead.
        Neither this function nor :func:`shutil.which` will predict the effect of an
        executable search on a native Windows system due to a :class:`subprocess.Popen`
        call without ``shell=True``, because shell and non-shell executable search on
        Windows differ considerably.
    # From: http://stackoverflow.com/a/377028/548792
    winprog_exts = _get_exe_extensions()
    def is_exec(fpath: str) -> bool:
            osp.isfile(fpath)
            and os.access(fpath, os.X_OK)
                sys.platform != "win32" or not winprog_exts or any(fpath.upper().endswith(ext) for ext in winprog_exts)
    progs = []
        path = os.environ["PATH"]
    for folder in os.fspath(path).split(os.pathsep):
        folder = folder.strip('"')
        if folder:
            exe_path = osp.join(folder, program)
            for f in [exe_path] + ["%s%s" % (exe_path, e) for e in winprog_exts]:
                if is_exec(f):
                    progs.append(f)
    return progs
def _cygexpath(drive: Optional[str], path: str) -> str:
    if osp.isabs(path) and not drive:
        # Invoked from `cygpath()` directly with `D:Apps\123`?
        #  It's an error, leave it alone just slashes)
        p = path  # convert to str if AnyPath given
        p = path and osp.normpath(osp.expandvars(osp.expanduser(path)))
        if osp.isabs(p):
            if drive:
                # Confusing, maybe a remote system should expand vars.
                p = path
                p = cygpath(p)
        elif drive:
            p = "/proc/cygdrive/%s/%s" % (drive.lower(), p)
    p_str = os.fspath(p)  # ensure it is a str and not AnyPath
    return p_str.replace("\\", "/")
_cygpath_parsers: Tuple[Tuple[Pattern[str], Callable, bool], ...] = (
    # See: https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx
    # and: https://www.cygwin.com/cygwin-ug-net/using.html#unc-paths
        re.compile(r"\\\\\?\\UNC\\([^\\]+)\\([^\\]+)(?:\\(.*))?"),
        (lambda server, share, rest_path: "//%s/%s/%s" % (server, share, rest_path.replace("\\", "/"))),
    (re.compile(r"\\\\\?\\(\w):[/\\](.*)"), (_cygexpath), False),
    (re.compile(r"(\w):[/\\](.*)"), (_cygexpath), False),
    (re.compile(r"file:(.*)", re.I), (lambda rest_path: rest_path), True),
    (re.compile(r"(\w{2,}:.*)"), (lambda url: url), False),  # remote URL, do nothing
def cygpath(path: str) -> str:
    """Use :meth:`git.cmd.Git.polish_url` instead, that works on any environment."""
    path = os.fspath(path)  # Ensure is str and not AnyPath.
    # Fix to use Paths when 3.5 dropped. Or to be just str if only for URLs?
    if not path.startswith(("/cygdrive", "//", "/proc/cygdrive")):
        for regex, parser, recurse in _cygpath_parsers:
            match = regex.match(path)
                path = parser(*match.groups())
                if recurse:
                    path = cygpath(path)
            path = _cygexpath(None, path)
_decygpath_regex = re.compile(r"(?:/proc)?/cygdrive/(\w)(/.*)?")
def decygpath(path: PathLike) -> str:
    m = _decygpath_regex.match(path)
        drive, rest_path = m.groups()
        path = "%s:%s" % (drive.upper(), rest_path or "")
#: Store boolean flags denoting if a specific Git executable
#: is from a Cygwin installation (since `cache_lru()` unsupported on PY2).
_is_cygwin_cache: Dict[str, Optional[bool]] = {}
def _is_cygwin_git(git_executable: str) -> bool:
    is_cygwin = _is_cygwin_cache.get(git_executable)  # type: Optional[bool]
    if is_cygwin is None:
        is_cygwin = False
            git_dir = osp.dirname(git_executable)
            if not git_dir:
                res = py_where(git_executable)
                git_dir = osp.dirname(res[0]) if res else ""
            # Just a name given, not a real path.
            uname_cmd = osp.join(git_dir, "uname")
            if not (Path(uname_cmd).is_file() and os.access(uname_cmd, os.X_OK)):
                _logger.debug(f"Failed checking if running in CYGWIN: {uname_cmd} is not an executable")
                _is_cygwin_cache[git_executable] = is_cygwin
                return is_cygwin
            process = subprocess.Popen([uname_cmd], stdout=subprocess.PIPE, universal_newlines=True)
            uname_out, _ = process.communicate()
            # retcode = process.poll()
            is_cygwin = "CYGWIN" in uname_out
            _logger.debug("Failed checking if running in CYGWIN due to: %r", ex)
def is_cygwin_git(git_executable: None) -> Literal[False]: ...
def is_cygwin_git(git_executable: PathLike) -> bool: ...
def is_cygwin_git(git_executable: Union[None, PathLike]) -> bool:
    # TODO: when py3.7 support is dropped, use the new interpolation f"{variable=}"
    _logger.debug(f"sys.platform={sys.platform!r}, git_executable={git_executable!r}")
    if sys.platform != "cygwin":
    elif git_executable is None:
        return _is_cygwin_git(str(git_executable))
def get_user_id() -> str:
    """:return: String identifying the currently active system user as ``name@node``"""
    return "%s@%s" % (getpass.getuser(), platform.node())
def finalize_process(proc: Union[subprocess.Popen, "Git.AutoInterrupt"], **kwargs: Any) -> None:
    """Wait for the process (clone, fetch, pull or push) and handle its errors
    accordingly."""
    # TODO: No close proc-streams??
    proc.wait(**kwargs)
def expand_path(p: None, expand_vars: bool = ...) -> None: ...
def expand_path(p: PathLike, expand_vars: bool = ...) -> str:
    # TODO: Support for Python 3.5 has been dropped, so these overloads can be improved.
def expand_path(p: Union[None, PathLike], expand_vars: bool = True) -> Optional[PathLike]:
    if isinstance(p, Path):
        return p.resolve()
        p = osp.expanduser(p)  # type: ignore[arg-type]
        if expand_vars:
            p = osp.expandvars(p)
        return osp.normpath(osp.abspath(p))
def remove_password_if_present(cmdline: Sequence[str]) -> List[str]:
    """Parse any command line argument and if one of the elements is an URL with a
    username and/or password, replace them by stars (in-place).
    If nothing is found, this just returns the command line as-is.
    This should be used for every log line that print a command line, as well as
    exception messages.
    new_cmdline = []
    for index, to_parse in enumerate(cmdline):
        new_cmdline.append(to_parse)
            url = urlsplit(to_parse)
            # Remove password from the URL if present.
            if url.password is None and url.username is None:
            if url.password is not None:
                url = url._replace(netloc=url.netloc.replace(url.password, "*****"))
            if url.username is not None:
                url = url._replace(netloc=url.netloc.replace(url.username, "*****"))
            new_cmdline[index] = urlunsplit(url)
            # This is not a valid URL.
    return new_cmdline
# } END utilities
# { Classes
class RemoteProgress:
    """Handler providing an interface to parse progress information emitted by
    :manpage:`git-push(1)` and :manpage:`git-fetch(1)` and to dispatch callbacks
    allowing subclasses to react to the progress."""
    _num_op_codes: int = 9
        BEGIN,
        COUNTING,
        COMPRESSING,
        WRITING,
        RECEIVING,
        RESOLVING,
        FINDING_SOURCES,
        CHECKING_OUT,
    ) = [1 << x for x in range(_num_op_codes)]
    STAGE_MASK = BEGIN | END
    OP_MASK = ~STAGE_MASK
    DONE_TOKEN = "done."
    TOKEN_SEPARATOR = ", "
        "_cur_line",
        "_seen_ops",
        "error_lines",  # Lines that started with 'error:' or 'fatal:'.
        "other_lines",  # Lines not denoting progress (i.e.g. push-infos).
    re_op_absolute = re.compile(r"(remote: )?([\w\s]+):\s+()(\d+)()(.*)")
    re_op_relative = re.compile(r"(remote: )?([\w\s]+):\s+(\d+)% \((\d+)/(\d+)\)(.*)")
        self._seen_ops: List[int] = []
        self._cur_line: Optional[str] = None
        self.error_lines: List[str] = []
        self.other_lines: List[str] = []
    def _parse_progress_line(self, line: AnyStr) -> None:
        """Parse progress information from the given line as retrieved by
        :manpage:`git-push(1)` or :manpage:`git-fetch(1)`.
        - Lines that do not contain progress info are stored in :attr:`other_lines`.
        - Lines that seem to contain an error (i.e. start with ``error:`` or ``fatal:``)
          are stored in :attr:`error_lines`.
        # handle
        # Counting objects: 4, done.
        # Compressing objects:  50% (1/2)
        # Compressing objects: 100% (2/2)
        # Compressing objects: 100% (2/2), done.
        if isinstance(line, bytes):  # mypy argues about ternary assignment.
            line_str = line.decode("utf-8")
            line_str = line
        self._cur_line = line_str
        if self._cur_line.startswith(("error:", "fatal:")):
            self.error_lines.append(self._cur_line)
        cur_count, max_count = None, None
        match = self.re_op_relative.match(line_str)
            match = self.re_op_absolute.match(line_str)
            self.line_dropped(line_str)
            self.other_lines.append(line_str)
        # END could not get match
        op_code = 0
        _remote, op_name, _percent, cur_count, max_count, message = match.groups()
        # Get operation ID.
        if op_name == "Counting objects":
            op_code |= self.COUNTING
        elif op_name == "Compressing objects":
            op_code |= self.COMPRESSING
        elif op_name == "Writing objects":
            op_code |= self.WRITING
        elif op_name == "Receiving objects":
            op_code |= self.RECEIVING
        elif op_name == "Resolving deltas":
            op_code |= self.RESOLVING
        elif op_name == "Finding sources":
            op_code |= self.FINDING_SOURCES
        elif op_name == "Checking out files":
            op_code |= self.CHECKING_OUT
            # Note: On Windows it can happen that partial lines are sent.
            # Hence we get something like "CompreReceiving objects", which is
            # a blend of "Compressing objects" and "Receiving objects".
            # This can't really be prevented, so we drop the line verbosely
            # to make sure we get informed in case the process spits out new
            # commands at some point.
            # Note: Don't add this line to the other lines, as we have to silently
            # drop it.
        # END handle op code
        # Figure out stage.
        if op_code not in self._seen_ops:
            self._seen_ops.append(op_code)
            op_code |= self.BEGIN
        # END begin opcode
            message = ""
        # END message handling
        message = message.strip()
        if message.endswith(self.DONE_TOKEN):
            op_code |= self.END
            message = message[: -len(self.DONE_TOKEN)]
        # END end message handling
        message = message.strip(self.TOKEN_SEPARATOR)
        self.update(
            op_code,
            cur_count and float(cur_count),
            max_count and float(max_count),
    def new_message_handler(self) -> Callable[[str], None]:
            A progress handler suitable for :func:`~git.cmd.handle_process_output`,
            passing lines on to this progress handler in a suitable format.
        def handler(line: AnyStr) -> None:
            return self._parse_progress_line(line.rstrip())
        # END handler
    def line_dropped(self, line: str) -> None:
        """Called whenever a line could not be understood and was therefore dropped."""
        op_code: int,
        cur_count: Union[str, float],
        max_count: Union[str, float, None] = None,
        message: str = "",
        """Called whenever the progress changes.
        :param op_code:
            Integer allowing to be compared against Operation IDs and stage IDs.
            Stage IDs are :const:`BEGIN` and :const:`END`. :const:`BEGIN` will only be
            set once for each Operation ID as well as :const:`END`. It may be that
            :const:`BEGIN` and :const:`END` are set at once in case only one progress
            message was emitted due to the speed of the operation. Between
            :const:`BEGIN` and :const:`END`, none of these flags will be set.
            Operation IDs are all held within the :const:`OP_MASK`. Only one Operation
            ID will be active per call.
        :param cur_count:
            Current absolute count of items.
        :param max_count:
            The maximum count of items we expect. It may be ``None`` in case there is no
            maximum number of items or if it is (yet) unknown.
        :param message:
            In case of the :const:`WRITING` operation, it contains the amount of bytes
            transferred. It may possibly be used for other purposes as well.
            You may read the contents of the current line in
            :attr:`self._cur_line <_cur_line>`.
class CallableRemoteProgress(RemoteProgress):
    """A :class:`RemoteProgress` implementation forwarding updates to any callable.
        Like direct instances of :class:`RemoteProgress`, instances of this
        :class:`CallableRemoteProgress` class are not themselves directly callable.
        Rather, instances of this class wrap a callable and forward to it. This should
        therefore not be confused with :class:`git.types.CallableProgress`.
    __slots__ = ("_callable",)
    def __init__(self, fn: Callable) -> None:
        self._callable = fn
    def update(self, *args: Any, **kwargs: Any) -> None:
        self._callable(*args, **kwargs)
class Actor:
    """Actors hold information about a person acting on the repository. They can be
    committers and authors or anything with a name and an email as mentioned in the git
    log entries."""
    # PRECOMPILED REGEX
    name_only_regex = re.compile(r"<(.*)>")
    name_email_regex = re.compile(r"(.*) <(.*?)>")
    # ENVIRONMENT VARIABLES
    # These are read when creating new commits.
    env_author_name = "GIT_AUTHOR_NAME"
    env_author_email = "GIT_AUTHOR_EMAIL"
    env_committer_name = "GIT_COMMITTER_NAME"
    env_committer_email = "GIT_COMMITTER_EMAIL"
    # CONFIGURATION KEYS
    conf_name = "name"
    conf_email = "email"
    __slots__ = ("name", "email")
    def __init__(self, name: Optional[str], email: Optional[str]) -> None:
        return self.name == other.name and self.email == other.email
    def __ne__(self, other: Any) -> bool:
        return hash((self.name, self.email))
        return self.name if self.name else ""
        return '<git.Actor "%s <%s>">' % (self.name, self.email)
    def _from_string(cls, string: str) -> "Actor":
        """Create an :class:`Actor` from a string.
        :param string:
            The string, which is expected to be in regular git format::
                John Doe <jdoe@example.com>
            :class:`Actor`
        m = cls.name_email_regex.search(string)
            name, email = m.groups()
            return Actor(name, email)
            m = cls.name_only_regex.search(string)
                return Actor(m.group(1), None)
            # Assume the best and use the whole string as name.
            return Actor(string, None)
            # END special case name
        # END handle name/email matching
    def _main_actor(
        env_name: str,
        env_email: str,
        config_reader: Union[None, "GitConfigParser", "SectionConstraint"] = None,
    ) -> "Actor":
        actor = Actor("", "")
        user_id = None  # We use this to avoid multiple calls to getpass.getuser().
        def default_email() -> str:
            nonlocal user_id
            if not user_id:
                user_id = get_user_id()
        def default_name() -> str:
            return default_email().split("@")[0]
        for attr, evar, cvar, default in (
            ("name", env_name, cls.conf_name, default_name),
            ("email", env_email, cls.conf_email, default_email),
                val = os.environ[evar]
                setattr(actor, attr, val)
                if config_reader is not None:
                        val = config_reader.get("user", cvar)
                        val = default()
                # END config-reader handling
                if not getattr(actor, attr):
                    setattr(actor, attr, default())
            # END handle name
        # END for each item to retrieve
        return actor
    def committer(cls, config_reader: Union[None, "GitConfigParser", "SectionConstraint"] = None) -> "Actor":
            :class:`Actor` instance corresponding to the configured committer. It
            behaves similar to the git implementation, such that the environment will
            override configuration values of `config_reader`. If no value is set at all,
            it will be generated.
        :param config_reader:
            ConfigReader to use to retrieve the values from in case they are not set in
            the environment.
        return cls._main_actor(cls.env_committer_name, cls.env_committer_email, config_reader)
    def author(cls, config_reader: Union[None, "GitConfigParser", "SectionConstraint"] = None) -> "Actor":
        """Same as :meth:`committer`, but defines the main author. It may be specified
        in the environment, but defaults to the committer."""
        return cls._main_actor(cls.env_author_name, cls.env_author_email, config_reader)
class Stats:
    """Represents stat information as presented by git at the end of a merge. It is
    created from the output of a diff operation.
     c = Commit( sha1 )
     s = c.stats
     s.total         # full-stat-dict
     s.files         # dict( filepath : stat-dict )
    ``stat-dict``
    A dictionary with the following keys and values::
      deletions = number of deleted lines as int
      insertions = number of inserted lines as int
      lines = total number of lines changed as int, or deletions + insertions
      change_type = type of change as str, A|C|D|M|R|T|U|X|B
    ``full-stat-dict``
    In addition to the items in the stat-dict, it features additional information::
     files = number of changed files as int
    __slots__ = ("total", "files")
    def __init__(self, total: Total_TD, files: Dict[PathLike, Files_TD]) -> None:
    def _list_from_string(cls, repo: "Repo", text: str) -> "Stats":
        """Create a :class:`Stats` object from output retrieved by
        :manpage:`git-diff(1)`.
            :class:`git.Stats`
        hsh: HSH_TD = {
            "total": {"insertions": 0, "deletions": 0, "lines": 0, "files": 0},
            "files": {},
            (change_type, raw_insertions, raw_deletions, filename) = line.split("\t")
            insertions = raw_insertions != "-" and int(raw_insertions) or 0
            deletions = raw_deletions != "-" and int(raw_deletions) or 0
            hsh["total"]["insertions"] += insertions
            hsh["total"]["deletions"] += deletions
            hsh["total"]["lines"] += insertions + deletions
            hsh["total"]["files"] += 1
            files_dict: Files_TD = {
                "insertions": insertions,
                "deletions": deletions,
                "lines": insertions + deletions,
                "change_type": change_type,
            hsh["files"][filename.strip()] = files_dict
        return Stats(hsh["total"], hsh["files"])
class IndexFileSHA1Writer:
    """Wrapper around a file-like object that remembers the SHA1 of the data written to
    it. It will write a sha when the stream is closed or if asked for explicitly using
    :meth:`write_sha`.
    Only useful to the index file.
        Based on the dulwich project.
    __slots__ = ("f", "sha1")
    def __init__(self, f: IO) -> None:
        self.f = f
        self.sha1 = make_sha(b"")
    def write(self, data: AnyStr) -> int:
        self.sha1.update(data)
        return self.f.write(data)
    def write_sha(self) -> bytes:
        sha = self.sha1.digest()
        self.f.write(sha)
    def close(self) -> bytes:
        sha = self.write_sha()
        return self.f.tell()
class LockFile:
    """Provides methods to obtain, check for, and release a file based lock which
    should be used to handle concurrent access to the same file.
    As we are a utility class to be derived from, we only use protected methods.
    Locks will automatically be released on destruction.
    __slots__ = ("_file_path", "_owns_lock")
    def __init__(self, file_path: PathLike) -> None:
        self._file_path = file_path
        self._owns_lock = False
        self._release_lock()
    def _lock_file_path(self) -> str:
        """:return: Path to lockfile"""
        return "%s.lock" % (self._file_path)
    def _has_lock(self) -> bool:
            True if we have a lock and if the lockfile still exists
        :raise AssertionError:
            If our lock-file does not exist.
        return self._owns_lock
    def _obtain_lock_or_raise(self) -> None:
        """Create a lock file as flag for other instances, mark our instance as
        lock-holder.
            If a lock was already present or a lock file could not be written.
        if self._has_lock():
        lock_file = self._lock_file_path()
        if osp.isfile(lock_file):
                "Lock for file %r did already exist, delete %r in case the lock is illegal"
                % (self._file_path, lock_file)
            with open(lock_file, mode="w"):
            raise IOError(str(e)) from e
        self._owns_lock = True
    def _obtain_lock(self) -> None:
        """The default implementation will raise if a lock cannot be obtained.
        Subclasses may override this method to provide a different implementation.
        return self._obtain_lock_or_raise()
    def _release_lock(self) -> None:
        """Release our lock if we have one."""
        if not self._has_lock():
        # If someone removed our file beforehand, lets just flag this issue instead of
        # failing, to make it more usable.
        lfp = self._lock_file_path()
            rmfile(lfp)
class BlockingLockFile(LockFile):
    """The lock file will block until a lock could be obtained, or fail after a
    specified timeout.
        If the directory containing the lock was removed, an exception will be raised
        during the blocking period, preventing hangs as the lock can never be obtained.
    __slots__ = ("_check_interval", "_max_block_time")
        file_path: PathLike,
        check_interval_s: float = 0.3,
        max_block_time_s: int = sys.maxsize,
        """Configure the instance.
        :param check_interval_s:
            Period of time to sleep until the lock is checked the next time.
            By default, it waits a nearly unlimited time.
        :param max_block_time_s:
            Maximum amount of seconds we may lock.
        super().__init__(file_path)
        self._check_interval = check_interval_s
        self._max_block_time = max_block_time_s
        """This method blocks until it obtained the lock, or raises :exc:`IOError` if it
        ran out of time or if the parent directory was not available anymore.
        If this method returns, you are guaranteed to own the lock.
        starttime = time.time()
        maxtime = starttime + float(self._max_block_time)
                super()._obtain_lock()
                # synity check: if the directory leading to the lockfile is not
                # readable anymore, raise an exception
                curtime = time.time()
                if not osp.isdir(osp.dirname(self._lock_file_path())):
                    msg = "Directory containing the lockfile %r was not readable anymore after waiting %g seconds" % (
                        self._lock_file_path(),
                        curtime - starttime,
                    raise IOError(msg) from e
                # END handle missing directory
                if curtime >= maxtime:
                    msg = "Waited %g seconds for lock at %r" % (
                        maxtime - starttime,
                # END abort if we wait too long
                time.sleep(self._check_interval)
        # END endless loop
class IterableList(List[T_IterableObj]):  # type: ignore[type-var]
    """List of iterable objects allowing to query an object by id or by named index::
     heads = repo.heads
     heads.master
     heads['master']
     heads[0]
    Iterable parent objects:
    * :class:`Commit <git.objects.Commit>`
    * :class:`Submodule <git.objects.submodule.base.Submodule>`
    * :class:`Reference <git.refs.reference.Reference>`
    * :class:`FetchInfo <git.remote.FetchInfo>`
    * :class:`PushInfo <git.remote.PushInfo>`
    Iterable via inheritance:
    * :class:`Head <git.refs.head.Head>`
    * :class:`TagReference <git.refs.tag.TagReference>`
    * :class:`RemoteReference <git.refs.remote.RemoteReference>`
    This requires an ``id_attribute`` name to be set which will be queried from its
    contained items to have a means for comparison.
    A prefix can be specified which is to be used in case the id returned by the items
    always contains a prefix that does not matter to the user, so it can be left out.
    __slots__ = ("_id_attr", "_prefix")
    def __new__(cls, id_attr: str, prefix: str = "") -> "IterableList[T_IterableObj]":
    def __init__(self, id_attr: str, prefix: str = "") -> None:
        self._id_attr = id_attr
        self._prefix = prefix
    def __contains__(self, attr: object) -> bool:
        # First try identity match for performance.
            rval = list.__contains__(self, attr)
            if rval:
        # END handle match
        # Otherwise make a full name search.
            getattr(self, cast(str, attr))  # Use cast to silence mypy.
        # END handle membership
    def __getattr__(self, attr: str) -> T_IterableObj:
        attr = self._prefix + attr
        for item in self:
            if getattr(item, self._id_attr) == attr:
        # END for each item
        return list.__getattribute__(self, attr)
    def __getitem__(self, index: Union[SupportsIndex, int, slice, str]) -> T_IterableObj:  # type: ignore[override]
        if isinstance(index, int):
            return list.__getitem__(self, index)
        elif isinstance(index, slice):
            raise ValueError("Index should be an int or str")
                return getattr(self, cast(str, index))
                raise IndexError(f"No item found with id {self._prefix}{index}") from e
        # END handle getattr
    def __delitem__(self, index: Union[SupportsIndex, int, slice, str]) -> None:
        delindex = cast(int, index)
        if isinstance(index, str):
            delindex = -1
            name = self._prefix + index
            for i, item in enumerate(self):
                if getattr(item, self._id_attr) == name:
                    delindex = i
                # END search index
            if delindex == -1:
                raise IndexError("Item with name %s not found" % name)
        # END get index to delete
        list.__delitem__(self, delindex)
class IterableObj(Protocol):
    """Defines an interface for iterable items, so there is a uniform way to retrieve
    and iterate items within the git repository.
    Subclasses:
    * :class:`Remote <git.remote.Remote>`
    _id_attribute_: str
    def iter_items(cls, repo: "Repo", *args: Any, **kwargs: Any) -> Iterator[T_IterableObj]:
        # Return-typed to be compatible with subtypes e.g. Remote.
        """Find (all) items of this type.
        Subclasses can specify `args` and `kwargs` differently, and may use them for
        filtering. However, when the method is called with no additional positional or
        keyword arguments, subclasses are obliged to to yield all items.
            Iterator yielding Items
        raise NotImplementedError("To be implemented by Subclass")
    def list_items(cls, repo: "Repo", *args: Any, **kwargs: Any) -> IterableList[T_IterableObj]:
        """Find (all) items of this type and collect them into a list.
        For more information about the arguments, see :meth:`iter_items`.
            Favor the :meth:`iter_items` method as it will avoid eagerly collecting all
            items. When there are many items, that can slow performance and increase
            memory usage.
            list(Item,...) list of item instances
        out_list: IterableList = IterableList(cls._id_attribute_)
        out_list.extend(cls.iter_items(repo, *args, **kwargs))
        return out_list
class IterableClassWatcher(type):
    """Metaclass that issues :exc:`DeprecationWarning` when :class:`git.util.Iterable`
    is subclassed."""
    def __init__(cls, name: str, bases: Tuple, clsdict: Dict) -> None:
            if type(base) is IterableClassWatcher:
                    f"GitPython Iterable subclassed by {name}."
                    " Iterable is deprecated due to naming clash since v3.1.18"
                    " and will be removed in 4.0.0."
                    " Use IterableObj instead.",
class Iterable(metaclass=IterableClassWatcher):
    """Deprecated, use :class:`IterableObj` instead.
    Defines an interface for iterable items, so there is a uniform way to retrieve
    _id_attribute_ = "attribute that most suitably identifies your instance"
    def iter_items(cls, repo: "Repo", *args: Any, **kwargs: Any) -> Any:
        Find (all) items of this type.
        See :meth:`IterableObj.iter_items` for details on usage.
    def list_items(cls, repo: "Repo", *args: Any, **kwargs: Any) -> Any:
        Find (all) items of this type and collect them into a list.
        See :meth:`IterableObj.list_items` for details on usage.
        out_list: Any = IterableList(cls._id_attribute_)
# } END classes
"""Index utilities."""
__all__ = ["TemporaryFileSwap", "post_clear_cache", "default_index", "git_working_dir"]
# typing ----------------------------------------------------------------------
from typing import Any, Callable, TYPE_CHECKING, Optional, Type, cast
from git.types import Literal, PathLike, _T
    from git.index import IndexFile
# ---------------------------------------------------------------------------------
# { Aliases
pack = struct.pack
unpack = struct.unpack
# } END aliases
class TemporaryFileSwap:
    """Utility class moving a file to a temporary location within the same directory and
    moving it back on to where on object deletion."""
    __slots__ = ("file_path", "tmp_file_path")
        self.file_path = file_path
        dirname, basename = osp.split(file_path)
        fd, self.tmp_file_path = tempfile.mkstemp(prefix=basename, dir=dirname)
        with contextlib.suppress(OSError):  # It may be that the source does not exist.
            os.replace(self.file_path, self.tmp_file_path)
    def __enter__(self) -> "TemporaryFileSwap":
    ) -> Literal[False]:
        if osp.isfile(self.tmp_file_path):
            os.replace(self.tmp_file_path, self.file_path)
# { Decorators
def post_clear_cache(func: Callable[..., _T]) -> Callable[..., _T]:
    """Decorator for functions that alter the index using the git command.
    When a git command alters the index, this invalidates our possibly existing entries
    dictionary, which is why it must be deleted to allow it to be lazily reread later.
    def post_clear_cache_if_not_raised(self: "IndexFile", *args: Any, **kwargs: Any) -> _T:
        rval = func(self, *args, **kwargs)
    return post_clear_cache_if_not_raised
def default_index(func: Callable[..., _T]) -> Callable[..., _T]:
    """Decorator ensuring the wrapped method may only run if we are the default
    repository index.
    This is as we rely on git commands that operate on that index only.
    def check_default_index(self: "IndexFile", *args: Any, **kwargs: Any) -> _T:
                "Cannot call %r on indices that do not represent the default git index" % func.__name__
    return check_default_index
def git_working_dir(func: Callable[..., _T]) -> Callable[..., _T]:
    """Decorator which changes the current working dir to the one of the git
    repository in order to ensure relative paths are handled correctly."""
    def set_git_working_dir(self: "IndexFile", *args: Any, **kwargs: Any) -> _T:
        cur_wd = os.getcwd()
        os.chdir(cast(PathLike, self.repo.working_tree_dir))
            os.chdir(cur_wd)
        # END handle working dir
    return set_git_working_dir
# } END decorators
"""Utility functions for working with git objects."""
    "get_object_type_by_name",
    "parse_date",
    "parse_actor_and_date",
    "ProcessStreamAdapter",
    "Traversable",
    "altz_to_utctz_str",
    "utctz_to_altz",
    "verify_utctz",
    "tzoffset",
    "utc",
from string import digits
from git.util import Actor, IterableList, IterableObj
# typing ------------------------------------------------------------
    Deque,
from git.types import Has_id_attribute, Literal
    from git.types import Protocol, runtime_checkable
    from .tree import TraversedTreeTup, Tree
    Protocol = ABC
    def runtime_checkable(f):
class TraverseNT(NamedTuple):
    depth: int
    item: Union["Traversable", "Blob"]
    src: Union["Traversable", None]
T_TIobj = TypeVar("T_TIobj", bound="TraversableIterableObj")  # For TraversableIterableObj.traverse()
TraversedTup = Union[
    Tuple[Union["Traversable", None], "Traversable"],  # For Commit, Submodule.
    "TraversedTreeTup",  # For Tree.traverse().
# { Functions
def mode_str_to_int(modestr: Union[bytes, str]) -> int:
    """Convert mode bits from an octal mode string to an integer mode for git.
    :param modestr:
        String like ``755`` or ``644`` or ``100644`` - only the last 6 chars will be
        String identifying a mode compatible to the mode methods ids of the :mod:`stat`
        module regarding the rwx permissions for user, group and other, special flags
        and file system flags, such as whether it is a symlink.
    mode = 0
    for iteration, char in enumerate(reversed(modestr[-6:])):
        char = cast(Union[str, int], char)
        mode += int(char) << iteration * 3
    # END for each char
    return mode
def get_object_type_by_name(
    object_type_name: bytes,
) -> Union[Type["Commit"], Type["TagObject"], Type["Tree"], Type["Blob"]]:
    """Retrieve the Python class GitPython uses to represent a kind of Git object.
        A type suitable to handle the given as `object_type_name`.
        This type can be called create new instances.
    :param object_type_name:
        Member of :attr:`Object.TYPES <git.objects.base.Object.TYPES>`.
        If `object_type_name` is unknown.
    if object_type_name == b"commit":
        from . import commit
        return commit.Commit
    elif object_type_name == b"tag":
        from . import tag
        return tag.TagObject
    elif object_type_name == b"blob":
        from . import blob
        return blob.Blob
    elif object_type_name == b"tree":
        from . import tree
        return tree.Tree
        raise ValueError("Cannot handle unknown object type: %s" % object_type_name.decode())
def utctz_to_altz(utctz: str) -> int:
    """Convert a git timezone offset into a timezone offset west of UTC in seconds
    (compatible with :attr:`time.altzone`).
    :param utctz:
        git utc timezone string, e.g. +0200
    int_utctz = int(utctz)
    seconds = (abs(int_utctz) // 100) * 3600 + (abs(int_utctz) % 100) * 60
    return seconds if int_utctz < 0 else -seconds
def altz_to_utctz_str(altz: float) -> str:
    """Convert a timezone offset west of UTC in seconds into a Git timezone offset
    :param altz:
        Timezone offset in seconds west of UTC.
    hours = abs(altz) // 3600
    minutes = (abs(altz) % 3600) // 60
    sign = "-" if altz >= 60 else "+"
    return "{}{:02}{:02}".format(sign, hours, minutes)
def verify_utctz(offset: str) -> str:
        If `offset` is incorrect.
        `offset`
    fmt_exc = ValueError("Invalid timezone offset format: %s" % offset)
    if len(offset) != 5:
        raise fmt_exc
    if offset[0] not in "+-":
    if offset[1] not in digits or offset[2] not in digits or offset[3] not in digits or offset[4] not in digits:
    return offset
class tzoffset(tzinfo):
    def __init__(self, secs_west_of_utc: float, name: Union[None, str] = None) -> None:
        self._offset = timedelta(seconds=-secs_west_of_utc)
        self._name = name or "fixed"
    def __reduce__(self) -> Tuple[Type["tzoffset"], Tuple[float, str]]:
        return tzoffset, (-self._offset.total_seconds(), self._name)
    def utcoffset(self, dt: Union[datetime, None]) -> timedelta:
        return self._offset
    def tzname(self, dt: Union[datetime, None]) -> str:
    def dst(self, dt: Union[datetime, None]) -> timedelta:
utc = tzoffset(0, "UTC")
def from_timestamp(timestamp: float, tz_offset: float) -> datetime:
    """Convert a `timestamp` + `tz_offset` into an aware :class:`~datetime.datetime`
    utc_dt = datetime.fromtimestamp(timestamp, utc)
        local_dt = utc_dt.astimezone(tzoffset(tz_offset))
        return local_dt
        return utc_dt
def parse_date(string_date: Union[str, datetime]) -> Tuple[int, int]:
    """Parse the given date as one of the following:
        * Aware datetime instance
        * Git internal format: timestamp offset
        * :rfc:`2822`: ``Thu, 07 Apr 2005 22:13:13 +0200``
        * ISO 8601: ``2005-04-07T22:13:13`` - The ``T`` can be a space as well.
        Tuple(int(timestamp_UTC), int(offset)), both in seconds since epoch
        If the format could not be understood.
        Date can also be ``YYYY.MM.DD``, ``MM/DD/YYYY`` and ``DD.MM.YYYY``.
    if isinstance(string_date, datetime):
        if string_date.tzinfo:
            utcoffset = cast(timedelta, string_date.utcoffset())  # typeguard, if tzinfoand is not None
            offset = -int(utcoffset.total_seconds())
            return int(string_date.astimezone(utc).timestamp()), offset
            raise ValueError(f"string_date datetime object without tzinfo, {string_date}")
    # Git time
        if string_date.count(" ") == 1 and string_date.rfind(":") == -1:
            timestamp, offset_str = string_date.split()
            if timestamp.startswith("@"):
                timestamp = timestamp[1:]
            timestamp_int = int(timestamp)
            return timestamp_int, utctz_to_altz(verify_utctz(offset_str))
            offset_str = "+0000"  # Local time by default.
            if string_date[-5] in "-+":
                offset_str = verify_utctz(string_date[-5:])
                string_date = string_date[:-6]  # skip space as well
            # END split timezone info
            offset = utctz_to_altz(offset_str)
            # Now figure out the date and time portion - split time.
            date_formats = []
            splitter = -1
            if "," in string_date:
                date_formats.append("%a, %d %b %Y")
                splitter = string_date.rfind(" ")
                # ISO plus additional
                date_formats.append("%Y-%m-%d")
                date_formats.append("%Y.%m.%d")
                date_formats.append("%m/%d/%Y")
                date_formats.append("%d.%m.%Y")
                splitter = string_date.rfind("T")
                if splitter == -1:
                # END handle 'T' and ' '
            # END handle RFC or ISO
            assert splitter > -1
            # Split date and time.
            time_part = string_date[splitter + 1 :]  # Skip space.
            date_part = string_date[:splitter]
            # Parse time.
            tstruct = time.strptime(time_part, "%H:%M:%S")
            for fmt in date_formats:
                    dtstruct = time.strptime(date_part, fmt)
                    utctime = calendar.timegm(
                            dtstruct.tm_year,
                            dtstruct.tm_mon,
                            dtstruct.tm_mday,
                            tstruct.tm_hour,
                            tstruct.tm_min,
                            tstruct.tm_sec,
                            dtstruct.tm_wday,
                            dtstruct.tm_yday,
                            tstruct.tm_isdst,
                    return int(utctime), offset
            # END for each fmt
            # Still here ? fail.
            raise ValueError("no format matched")
        # END handle format
        raise ValueError(f"Unsupported date format or type: {string_date}, type={type(string_date)}") from e
# Precompiled regexes
_re_actor_epoch = re.compile(r"^.+? (.*) (\d+) ([+-]\d+).*$")
_re_only_actor = re.compile(r"^.+? (.*)$")
def parse_actor_and_date(line: str) -> Tuple[Actor, int, int]:
    """Parse out the actor (author or committer) info from a line like::
        author Tom Preston-Werner <tom@mojombo.com> 1191999972 -0700
        [Actor, int_seconds_since_epoch, int_timezone_offset]
    actor, epoch, offset = "", "0", "0"
    m = _re_actor_epoch.search(line)
        actor, epoch, offset = m.groups()
        m = _re_only_actor.search(line)
        actor = m.group(1) if m else line or ""
    return (Actor._from_string(actor), int(epoch), utctz_to_altz(offset))
# } END functions
class ProcessStreamAdapter:
    """Class wiring all calls to the contained Process instance.
    Use this type to hide the underlying process to provide access only to a specified
    stream. The process is usually wrapped into an :class:`~git.cmd.Git.AutoInterrupt`
    class to kill it if the instance goes out of scope.
    __slots__ = ("_proc", "_stream")
    def __init__(self, process: "Popen", stream_name: str) -> None:
        self._proc = process
        self._stream: StringIO = getattr(process, stream_name)  # guessed type
        return getattr(self._stream, attr)
class Traversable(Protocol):
    """Simple interface to perform depth-first or breadth-first traversals in one
    direction.
    Subclasses only need to implement one function.
    Instances of the subclass must be hashable.
    Defined subclasses:
    def _get_intermediate_items(cls, item: Any) -> Sequence["Traversable"]:
            Tuple of items connected to the given item.
            Must be implemented in subclass.
        class Commit::     (cls, Commit) -> Tuple[Commit, ...]
        class Submodule::  (cls, Submodule) -> Iterablelist[Submodule]
        class Tree::       (cls, Tree) -> Tuple[Tree, ...]
    def list_traverse(self, *args: Any, **kwargs: Any) -> Any:
        """Traverse self and collect all items found.
        Calling this directly on the abstract base class, including via a ``super()``
        proxy, is deprecated. Only overridden implementations should be called.
            "list_traverse() method should only be called from subclasses."
            " Calling from Traversable abstract class will raise NotImplementedError in 4.0.0."
            " The concrete subclasses in GitPython itself are 'Commit', 'RootModule', 'Submodule', and 'Tree'.",
        return self._list_traverse(*args, **kwargs)
    def _list_traverse(
        self, as_edge: bool = False, *args: Any, **kwargs: Any
    ) -> IterableList[Union["Commit", "Submodule", "Tree", "Blob"]]:
            :class:`~git.util.IterableList` with the results of the traversal as
            produced by :meth:`traverse`::
                Commit -> IterableList[Commit]
                Submodule ->  IterableList[Submodule]
                Tree -> IterableList[Union[Submodule, Tree, Blob]]
        # Commit and Submodule have id.__attribute__ as IterableObj.
        # Tree has id.__attribute__ inherited from IndexObject.
        if isinstance(self, Has_id_attribute):
            id = self._id_attribute_
            # Shouldn't reach here, unless Traversable subclass created with no
            # _id_attribute_.
            id = ""
            # Could add _id_attribute_ to Traversable, or make all Traversable also
            # Iterable?
        if not as_edge:
            out: IterableList[Union["Commit", "Submodule", "Tree", "Blob"]] = IterableList(id)
            out.extend(self.traverse(as_edge=as_edge, *args, **kwargs))  # noqa: B026
            # Overloads in subclasses (mypy doesn't allow typing self: subclass).
            # Union[IterableList['Commit'], IterableList['Submodule'], IterableList[Union['Submodule', 'Tree', 'Blob']]]
            # Raise DeprecationWarning, it doesn't make sense to use this.
            out_list: IterableList = IterableList(self.traverse(*args, **kwargs))
    def traverse(self, *args: Any, **kwargs: Any) -> Any:
        """Iterator yielding items found when traversing self.
            "traverse() method should only be called from subclasses."
        return self._traverse(*args, **kwargs)
    def _traverse(
        predicate: Callable[[Union["Traversable", "Blob", TraversedTup], int], bool] = lambda i, d: True,
        prune: Callable[[Union["Traversable", "Blob", TraversedTup], int], bool] = lambda i, d: False,
        depth: int = -1,
        branch_first: bool = True,
        visit_once: bool = True,
        ignore_self: int = 1,
        as_edge: bool = False,
    ) -> Union[Iterator[Union["Traversable", "Blob"]], Iterator[TraversedTup]]:
        """Iterator yielding items found when traversing `self`.
            A function ``f(i,d)`` that returns ``False`` if item i at depth ``d`` should
            not be included in the result.
        :param prune:
            A function ``f(i,d)`` that returns ``True`` if the search should stop at
            item ``i`` at depth ``d``. Item ``i`` will not be returned.
        :param depth:
            Defines at which level the iteration should not go deeper if -1. There is no
            limit if 0, you would effectively only get `self`, the root of the
            iteration. If 1, you would only get the first level of
            predecessors/successors.
        :param branch_first:
            If ``True``, items will be returned branch first, otherwise depth first.
        :param visit_once:
            If ``True``, items will only be returned once, although they might be
            encountered several times. Loops are prevented that way.
        :param ignore_self:
            If ``True``, `self` will be ignored and automatically pruned from the
            result. Otherwise it will be the first item to be returned. If `as_edge` is
            ``True``, the source of the first edge is ``None``.
        :param as_edge:
            If ``True``, return a pair of items, first being the source, second the
            destination, i.e. tuple(src, dest) with the edge spanning from source to
            Iterator yielding items found when traversing `self`::
                Commit -> Iterator[Union[Commit, Tuple[Commit, Commit]] Submodule ->
                Iterator[Submodule, Tuple[Submodule, Submodule]] Tree ->
                Iterator[Union[Blob, Tree, Submodule,
                                        Tuple[Union[Submodule, Tree], Union[Blob, Tree,
                                        Submodule]]]
                ignore_self=True is_edge=True -> Iterator[item] ignore_self=True
                is_edge=False --> Iterator[item] ignore_self=False is_edge=True ->
                Iterator[item] | Iterator[Tuple[src, item]] ignore_self=False
                is_edge=False -> Iterator[Tuple[src, item]]
        visited = set()
        stack: Deque[TraverseNT] = deque()
        stack.append(TraverseNT(0, self, None))  # self is always depth level 0.
        def addToStack(
            stack: Deque[TraverseNT],
            src_item: "Traversable",
            branch_first: bool,
            depth: int,
            lst = self._get_intermediate_items(item)
            if not lst:  # Empty list
            if branch_first:
                stack.extendleft(TraverseNT(depth, i, src_item) for i in lst)
                reviter = (TraverseNT(depth, lst[i], src_item) for i in range(len(lst) - 1, -1, -1))
                stack.extend(reviter)
        # END addToStack local method
            d, item, src = stack.pop()  # Depth of item, item, item_source
            if visit_once and item in visited:
            if visit_once:
                visited.add(item)
            rval: Union[TraversedTup, "Traversable", "Blob"]
            if as_edge:
                # If as_edge return (src, item) unless rrc is None
                # (e.g. for first item).
                rval = (src, item)
                rval = item
            if prune(rval, d):
            skipStartItem = ignore_self and (item is self)
            if not skipStartItem and predicate(rval, d):
                yield rval
            # Only continue to next level if this is appropriate!
            next_d = d + 1
            if depth > -1 and next_d > depth:
            addToStack(stack, item, branch_first, next_d)
        # END for each item on work stack
class Serializable(Protocol):
    """Defines methods to serialize and deserialize objects from and into a data
    stream."""
    # @abstractmethod
    def _serialize(self, stream: "BytesIO") -> "Serializable":
        """Serialize the data of this object into the given data stream.
            A serialized object would :meth:`_deserialize` into the same object.
        :param stream:
            A file-like object.
    def _deserialize(self, stream: "BytesIO") -> "Serializable":
        """Deserialize all information regarding this object from the stream.
class TraversableIterableObj(IterableObj, Traversable):
    TIobj_tuple = Tuple[Union[T_TIobj, None], T_TIobj]
    def list_traverse(self: T_TIobj, *args: Any, **kwargs: Any) -> IterableList[T_TIobj]:
        return super()._list_traverse(*args, **kwargs)
    def traverse(self: T_TIobj) -> Iterator[T_TIobj]: ...
    def traverse(
        self: T_TIobj,
        predicate: Callable[[Union[T_TIobj, Tuple[Union[T_TIobj, None], T_TIobj]], int], bool],
        prune: Callable[[Union[T_TIobj, Tuple[Union[T_TIobj, None], T_TIobj]], int], bool],
        visit_once: bool,
        ignore_self: Literal[True],
        as_edge: Literal[False],
    ) -> Iterator[T_TIobj]: ...
        ignore_self: Literal[False],
        as_edge: Literal[True],
    ) -> Iterator[Tuple[Union[T_TIobj, None], T_TIobj]]: ...
        predicate: Callable[[Union[T_TIobj, TIobj_tuple], int], bool],
        prune: Callable[[Union[T_TIobj, TIobj_tuple], int], bool],
    ) -> Iterator[Tuple[T_TIobj, T_TIobj]]: ...
        predicate: Callable[[Union[T_TIobj, TIobj_tuple], int], bool] = lambda i, d: True,
        prune: Callable[[Union[T_TIobj, TIobj_tuple], int], bool] = lambda i, d: False,
    ) -> Union[Iterator[T_TIobj], Iterator[Tuple[T_TIobj, T_TIobj]], Iterator[TIobj_tuple]]:
        """For documentation, see :meth:`Traversable._traverse`."""
        ## To typecheck instead of using cast:
        # import itertools
        # from git.types import TypeGuard
        # def is_commit_traversed(inp: Tuple) -> TypeGuard[Tuple[Iterator[Tuple['Commit', 'Commit']]]]:
        #     for x in inp[1]:
        #         if not isinstance(x, tuple) and len(x) != 2:
        #             if all(isinstance(inner, Commit) for inner in x):
        #                 continue
        #     return True
        # ret = super(Commit, self).traverse(predicate, prune, depth, branch_first, visit_once, ignore_self, as_edge)
        # ret_tup = itertools.tee(ret, 2)
        # assert is_commit_traversed(ret_tup), f"{[type(x) for x in list(ret_tup[0])]}"
        # return ret_tup[0]
            Union[Iterator[T_TIobj], Iterator[Tuple[Union[None, T_TIobj], T_TIobj]]],
            super()._traverse(
                predicate,  # type: ignore[arg-type]
                prune,  # type: ignore[arg-type]
                branch_first,
                visit_once,
                ignore_self,
                as_edge,
    "sm_section",
    "sm_name",
    "mkhead",
    "find_first_remote_branch",
    "SubmoduleConfigParser",
from git.config import GitConfigParser
from git.exc import InvalidGitRepositoryError
# typing -----------------------------------------------------------------------
from typing import Any, Sequence, TYPE_CHECKING, Union
    from weakref import ReferenceType
    from .base import Submodule
# { Utilities
def sm_section(name: str) -> str:
    """:return: Section title used in ``.gitmodules`` configuration file"""
    return f'submodule "{name}"'
def sm_name(section: str) -> str:
    """:return: Name of the submodule as parsed from the section name"""
    section = section.strip()
    return section[11:-1]
def mkhead(repo: "Repo", path: PathLike) -> "Head":
    """:return: New branch/head instance"""
    return git.Head(repo, git.Head.to_full_path(path))
def find_first_remote_branch(remotes: Sequence["Remote"], branch_name: str) -> "RemoteReference":
    """Find the remote branch matching the name of the given branch or raise
    :exc:`~git.exc.InvalidGitRepositoryError`."""
            return remote.refs[branch_name]
    # END for remote
    raise InvalidGitRepositoryError("Didn't find remote branch '%r' in any of the given remotes" % branch_name)
class SubmoduleConfigParser(GitConfigParser):
    """Catches calls to :meth:`~git.config.GitConfigParser.write`, and updates the
    ``.gitmodules`` blob in the index with the new data, if we have written into a
    Otherwise it would add the local file to the index to make it correspond with the
    working tree. Additionally, the cache must be cleared.
    Please note that no mutating method will work in bare mode.
        self._smref: Union["ReferenceType[Submodule]", None] = None
        self._index = None
        self._auto_write = True
    # { Interface
    def set_submodule(self, submodule: "Submodule") -> None:
        """Set this instance's submodule. It must be called before the first write
        operation begins."""
        self._smref = weakref.ref(submodule)
    def flush_to_index(self) -> None:
        """Flush changes in our configuration file to the index."""
        assert self._smref is not None
        # Should always have a file here.
        assert not isinstance(self._file_or_files, BytesIO)
        sm = self._smref()
        if sm is not None:
            index = self._index
            if index is None:
            # END handle index
            index.add([sm.k_modules_file], write=self._auto_write)
            sm._clear_cache()
        # END handle weakref
    # } END interface
    # { Overridden Methods
    def write(self) -> None:  # type: ignore[override]
        rval: None = super().write()
        self.flush_to_index()
    # END overridden methods
