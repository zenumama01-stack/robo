import zipfile
EXTENSIONS_TO_SKIP = [
  '.pdb',
  '.mojom.js',
  '.mojom-lite.js',
  '.info',
  '.m.js',
  # These are only needed for Chromium tests we don't run. Listed in
  # 'extensions' because the mksnapshot zip has these under a subdirectory, and
  # the PATHS_TO_SKIP is checked with |startswith|.
  'dbgcore.dll',
  'dbghelp.dll',
PATHS_TO_SKIP = [
  # Skip because it is an output of //ui/gl that we don't need.
  'angledata',
  # Skip because these are outputs that we don't need.
  './libVkICD_mock_',
  './VkICD_mock_',
  # Skip because its an output of create_bundle from
  # //build/config/mac/rules.gni that we don't need
  'Electron.dSYM',
  # Refs https://chromium-review.googlesource.com/c/angle/angle/+/2425197.
  # Remove this when Angle themselves remove the file:
  # https://issuetracker.google.com/issues/168736059
  'gen/angle/angle_commit.h',
  # //chrome/browser:resources depends on this via
  # //chrome/browser/resources/ssl/ssl_error_assistant, but we don't need to
  # ship it.
  'pyproto',
  'resources/inspector',
  'gen/third_party/devtools-frontend/src',
  'gen/ui/webui',
  # Skip because these get zipped separately in script/zip-symbols.py
  'debug',
def skip_path(dep, dist_zip, target_cpu):
  # Skip specific paths and extensions as well as the following special case:
  # snapshot_blob.bin is a dependency of mksnapshot.zip because
  # v8_context_generator needs it, but this file does not get generated for arm
  # and arm 64 binaries of mksnapshot since they are built on x64 hardware.
  # Consumers of arm and arm64 mksnapshot can generate snapshot_blob.bin
  # themselves by running mksnapshot.
  should_skip = (
    any(dep.startswith(path) for path in PATHS_TO_SKIP) or
    any(dep.endswith(ext) for ext in EXTENSIONS_TO_SKIP) or
      "arm" in target_cpu
      and dist_zip == "mksnapshot.zip"
      and dep == "snapshot_blob.bin"
  if should_skip and os.environ.get('ELECTRON_DEBUG_ZIP_SKIP') == '1':
    print("Skipping {}".format(dep))
  return should_skip
def execute(argv):
    output = subprocess.check_output(argv, stderr=subprocess.STDOUT)
    return output
    print(e.output)
    raise e
  dist_zip, runtime_deps, target_cpu, _, flatten_val, flatten_relative_to = argv
  should_flatten = flatten_val == "true"
  dist_files = set()
  with open(runtime_deps) as f:
    for dep in f.readlines():
      dep = dep.strip()
      if not skip_path(dep, dist_zip, target_cpu):
        dist_files.add(dep)
  # On Linux, filter out any files which have a .stripped companion
  if sys.platform == 'linux':
    dist_files = {
      dep for dep in dist_files if f"{dep.removeprefix('./')}.stripped" not in dist_files
  if sys.platform == 'darwin' and not should_flatten:
    execute(['zip', '-r', '-y', dist_zip] + list(dist_files))
    with zipfile.ZipFile(
      dist_zip, 'w', zipfile.ZIP_DEFLATED, allowZip64=True
    ) as z:
      for dep in dist_files:
        if os.path.isdir(dep):
          for root, _, files in os.walk(dep):
            for filename in files:
              z.write(os.path.join(root, filename))
          basename = os.path.basename(dep)
          dirname = os.path.dirname(dep)
          arcname = (
            os.path.join(dirname, 'chrome-sandbox')
            if basename.removesuffix('.stripped') == 'chrome_sandbox'
            else dep
          name_to_write = arcname
          # On Linux, strip the .stripped suffix from the name before zipping
            name_to_write = name_to_write.removesuffix('.stripped')
          if should_flatten:
            if flatten_relative_to:
              if name_to_write.startswith(flatten_relative_to):
                name_to_write = name_to_write[len(flatten_relative_to):]
                name_to_write = os.path.basename(arcname)
          z.write(
            dep,
            name_to_write,
  sys.exit(main(sys.argv[1:]))
Generate zip test data files.
import zipp
def make_zip_file(tree, dst):
    Zip the files in tree into a new zipfile at dst.
    with zipfile.ZipFile(dst, 'w') as zf:
        for name, contents in walk(tree):
            zf.writestr(name, contents)
        zipp.CompleteDirs.inject(zf)
    return dst
def walk(tree, prefix=''):
    for name, contents in tree.items():
            yield from walk(contents, prefix=f'{prefix}{name}/')
            yield f'{prefix}{name}', contents
class ZipFileSystem(AbstractArchiveFileSystem):
    """Read/Write contents of ZIP archive as a file-system
    Keeps file object open while instance lives.
    This class is pickleable, but not necessarily thread-safe
    protocol = "zip"
        mode="r",
        compression=zipfile.ZIP_STORED,
        allowZip64=True,
        compresslevel=None,
        fo: str or file-like
            Contains ZIP, and must exist. If a str, will fetch file using
            :meth:`~fsspec.open_files`, which must return one file exactly.
        mode: str
            Accept: "r", "w", "a"
        target_protocol: str (optional)
            If ``fo`` is a string, this value can be used to override the
            FS protocol inferred from a URL
        target_options: dict (optional)
            Kwargs passed when instantiating the target FS, if ``fo`` is
            a string.
        compression, allowZip64, compresslevel: passed to ZipFile
            Only relevant when creating a ZIP
        super().__init__(self, **kwargs)
        if mode not in set("rwa"):
            raise ValueError(f"mode '{mode}' no understood")
        if isinstance(fo, (str, os.PathLike)):
            if mode == "a":
                m = "r+b"
                m = mode + "b"
            fo = fsspec.open(
                fo, mode=m, protocol=target_protocol, **(target_options or {})
        self.force_zip_64 = allowZip64
        self.of = fo
        self.fo = fo.__enter__()  # the whole instance is a context
        self.zip = zipfile.ZipFile(
            self.fo,
            compression=compression,
            allowZip64=allowZip64,
            compresslevel=compresslevel,
        # zip file paths are always relative to the archive root
        return super()._strip_protocol(path).lstrip("/")
        if hasattr(self, "zip"):
            del self.zip
        """Commits any write changes to the file. Done on ``del`` too."""
        self.zip.close()
        if self.dir_cache is None or self.mode in set("wa"):
            # when writing, dir_cache is always in the ZipFile's attributes,
            # not read from the file.
            files = self.zip.infolist()
                dirname.rstrip("/"): {
                    "name": dirname.rstrip("/"),
                    "size": 0,
                    "type": "directory",
                for dirname in self._all_dirnames(self.zip.namelist())
            for z in files:
                f = {s: getattr(z, s, None) for s in zipfile.ZipInfo.__slots__}
                f.update(
                        "name": z.filename.rstrip("/"),
                        "size": z.file_size,
                        "type": ("directory" if z.is_dir() else "file"),
                self.dir_cache[f["name"]] = f
    def pipe_file(self, path, value, **kwargs):
        # override upstream, because we know the exact file size in this case
        self.zip.writestr(path, value, **kwargs)
        if "r" in mode and self.mode in set("wa"):
            if self.exists(path):
                raise OSError("ZipFS can only be open for reading or writing, not both")
        if "r" in self.mode and "w" in mode:
        out = self.zip.open(path, mode.strip("b"), force_zip64=self.force_zip_64)
        if "r" in mode:
            info = self.info(path)
            out.size = info["size"]
            out.name = info["name"]
    def find(self, path, maxdepth=None, withdirs=False, detail=False, **kwargs):
        def to_parts(_path: str):
            return list(filter(None, _path.replace("\\", "/").split("/")))
            path = str(path)
        # Remove the leading slash, as the zip file paths are always
        # given without a leading slash
        path = path.lstrip("/")
        path_parts = to_parts(path)
        path_depth = len(path_parts)
        self._get_dirs()
        # To match posix find, if an exact file name is given, we should
        # return only that file
        if path in self.dir_cache and self.dir_cache[path]["type"] == "file":
            result[path] = self.dir_cache[path]
            return result if detail else [path]
        for file_path, file_info in self.dir_cache.items():
            if len(file_parts := to_parts(file_path)) < path_depth or any(
                a != b for a, b in zip(path_parts, file_parts)
                # skip parent folders and mismatching paths
            if file_info["type"] == "directory":
                if withdirs and file_path not in result:
                    result[file_path.strip("/")] = file_info
            if file_path not in result:
                result[file_path] = file_info if detail else None
        if maxdepth:
                k: v for k, v in result.items() if k.count("/") < maxdepth + path_depth
        return result if detail else sorted(result)
