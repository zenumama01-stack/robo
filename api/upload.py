import mmap
from struct import Struct
from zipfile import ZipFile
from lib.config import PLATFORM, get_target_arch, \
                       get_zip_name, get_tar_name, set_verbose_mode, \
                       is_verbose_mode, get_platform_key, \
                       verbose_mode_print
from lib.util import get_electron_branding, execute, get_electron_version, \
                     store_artifact, get_electron_exec, get_out_dir, \
                     SRC_DIR, ELECTRON_DIR, TS_NODE
ELECTRON_VERSION = 'v' + get_electron_version()
DIST_NAME = get_zip_name(PROJECT_NAME, ELECTRON_VERSION)
SYMBOLS_NAME = get_zip_name(PROJECT_NAME, ELECTRON_VERSION, 'symbols')
# Use tar.xz compression for dsym files due to size
DSYM_NAME = get_tar_name(PROJECT_NAME, ELECTRON_VERSION, 'dsym')
DSYM_SNAPSHOT_NAME = get_zip_name(PROJECT_NAME, ELECTRON_VERSION,
                                  'dsym-snapshot')
PDB_NAME = get_zip_name(PROJECT_NAME, ELECTRON_VERSION, 'pdb')
DEBUG_NAME = get_zip_name(PROJECT_NAME, ELECTRON_VERSION, 'debug')
TOOLCHAIN_PROFILE_NAME = get_zip_name(PROJECT_NAME, ELECTRON_VERSION,
                                      'toolchain-profile')
CXX_OBJECTS_NAME = get_zip_name(PROJECT_NAME, ELECTRON_VERSION,
                                      'libcxx_objects')
  set_verbose_mode(args.verbose)
  if args.upload_to_storage:
    utcnow = datetime.datetime.utcnow()
    args.upload_timestamp = utcnow.strftime('%Y%m%d')
  build_version = get_electron_build_version()
  if not ELECTRON_VERSION.startswith(build_version):
    errmsg = f"Tag ({ELECTRON_VERSION}) should match build ({build_version})\n"
    sys.stderr.write(errmsg)
    sys.stderr.flush()
    return 1
  tag_exists = False
  release = get_release(args.version)
  if not release['draft']:
    tag_exists = True
  if not args.upload_to_storage:
    assert release['exists'], \
          'Release does not exist; cannot upload to GitHub!'
    assert tag_exists == args.overwrite, \
          'You have to pass --overwrite to overwrite a published release'
  # Upload Electron files.
  # Rename dist.zip to  get_zip_name('electron', version, suffix='')
  electron_zip = os.path.join(OUT_DIR, DIST_NAME)
  shutil.copy2(os.path.join(OUT_DIR, 'dist.zip'), electron_zip)
  upload_electron(release, electron_zip, args)
  symbols_zip = os.path.join(OUT_DIR, SYMBOLS_NAME)
  shutil.copy2(os.path.join(OUT_DIR, 'symbols.zip'), symbols_zip)
  upload_electron(release, symbols_zip, args)
    if get_platform_key() == 'darwin' and get_target_arch() == 'x64':
      api_path = os.path.join(ELECTRON_DIR, 'electron-api.json')
      upload_electron(release, api_path, args)
      ts_defs_path = os.path.join(ELECTRON_DIR, 'electron.d.ts')
      upload_electron(release, ts_defs_path, args)
    dsym_zip = os.path.join(OUT_DIR, DSYM_NAME)
    shutil.copy2(os.path.join(OUT_DIR, 'dsym.tar.xz'), dsym_zip)
    upload_electron(release, dsym_zip, args)
    dsym_snapshot_zip = os.path.join(OUT_DIR, DSYM_SNAPSHOT_NAME)
    shutil.copy2(os.path.join(OUT_DIR, 'dsym-snapshot.zip'), dsym_snapshot_zip)
    upload_electron(release, dsym_snapshot_zip, args)
    pdb_zip = os.path.join(OUT_DIR, PDB_NAME)
    shutil.copy2(os.path.join(OUT_DIR, 'pdb.zip'), pdb_zip)
    upload_electron(release, pdb_zip, args)
    debug_zip = os.path.join(OUT_DIR, DEBUG_NAME)
    shutil.copy2(os.path.join(OUT_DIR, 'debug.zip'), debug_zip)
    upload_electron(release, debug_zip, args)
    # Upload libcxx_objects.zip for linux only
    libcxx_objects = get_zip_name('libcxx-objects', ELECTRON_VERSION)
    libcxx_objects_zip = os.path.join(OUT_DIR, libcxx_objects)
    shutil.copy2(os.path.join(OUT_DIR, 'libcxx_objects.zip'),
        libcxx_objects_zip)
    upload_electron(release, libcxx_objects_zip, args)
    # Upload headers.zip and abi_headers.zip as non-platform specific
    if get_target_arch() == "x64":
      cxx_headers_zip = os.path.join(OUT_DIR, 'libcxx_headers.zip')
      upload_electron(release, cxx_headers_zip, args)
      abi_headers_zip = os.path.join(OUT_DIR, 'libcxxabi_headers.zip')
      upload_electron(release, abi_headers_zip, args)
  # Upload free version of ffmpeg.
  ffmpeg = get_zip_name('ffmpeg', ELECTRON_VERSION)
  ffmpeg_zip = os.path.join(OUT_DIR, ffmpeg)
  ffmpeg_build_path = os.path.join(SRC_DIR, 'out', 'ffmpeg', 'ffmpeg.zip')
  shutil.copy2(ffmpeg_build_path, ffmpeg_zip)
  upload_electron(release, ffmpeg_zip, args)
  chromedriver = get_zip_name('chromedriver', ELECTRON_VERSION)
  chromedriver_zip = os.path.join(OUT_DIR, chromedriver)
  shutil.copy2(os.path.join(OUT_DIR, 'chromedriver.zip'), chromedriver_zip)
  upload_electron(release, chromedriver_zip, args)
  mksnapshot = get_zip_name('mksnapshot', ELECTRON_VERSION)
  mksnapshot_zip = os.path.join(OUT_DIR, mksnapshot)
  if get_target_arch().startswith('arm') and PLATFORM != 'darwin':
    # Upload the x64 binary for arm/arm64 mksnapshot
    mksnapshot = get_zip_name('mksnapshot', ELECTRON_VERSION, 'x64')
  shutil.copy2(os.path.join(OUT_DIR, 'mksnapshot.zip'), mksnapshot_zip)
  upload_electron(release, mksnapshot_zip, args)
  if PLATFORM == 'linux' and get_target_arch() == 'x64':
    # Upload the hunspell dictionaries only from the linux x64 build
    hunspell_dictionaries_zip = os.path.join(
      OUT_DIR, 'hunspell_dictionaries.zip')
    upload_electron(release, hunspell_dictionaries_zip, args)
  if not tag_exists and not args.upload_to_storage:
    # Upload symbols to symbol server.
    run_python_upload_script('upload-symbols.py')
      run_python_upload_script('upload-node-headers.py', '-v', args.version)
    toolchain_profile_zip = os.path.join(OUT_DIR, TOOLCHAIN_PROFILE_NAME)
    with ZipFile(toolchain_profile_zip, 'w') as myzip:
      myzip.write(
        os.path.join(OUT_DIR, 'windows_toolchain_profile.json'),
        'toolchain_profile.json')
    upload_electron(release, toolchain_profile_zip, args)
  parser = argparse.ArgumentParser(description='upload distribution file')
                      default=ELECTRON_VERSION)
  parser.add_argument('-o', '--overwrite',
                      help='Overwrite a published release',
  parser.add_argument('-p', '--publish-release',
                      help='Publish the release',
  parser.add_argument('-s', '--upload_to_storage',
                      help='Upload assets to azure bucket',
                      dest='upload_to_storage',
                      default=False,
  parser.add_argument('--verbose',
                      help='Mooooorreee logs')
def run_python_upload_script(script, *args):
  script_path = os.path.join(
    ELECTRON_DIR, 'script', 'release', 'uploaders', script)
  print(execute([sys.executable, script_path] + list(args)))
def get_electron_build_version():
  if get_target_arch().startswith('arm') or 'CI' in os.environ:
    # In CI we just build as told.
    return ELECTRON_VERSION
  electron = get_electron_exec()
  return subprocess.check_output([electron, '--version']).strip()
class NonZipFileError(ValueError):
  """Raised when a given file does not appear to be a zip"""
def zero_zip_date_time(fname):
  """ Wrap strip-zip zero_zip_date_time within a file opening operation """
    with open(fname, 'r+b') as f:
      _zero_zip_date_time(f)
    raise NonZipFileError(fname)
def _zero_zip_date_time(zip_):
  def purify_extra_data(mm, offset, length, compressed_size=0):
    extra_header_struct = Struct("<HH")
    # 0. id
    # 1. length
    STRIPZIP_OPTION_HEADER = 0xFFFF
    EXTENDED_TIME_DATA = 0x5455
    # Some sort of extended time data, see
    # ftp://ftp.info-zip.org/pub/infozip/src/zip30.zip ./proginfo/extrafld.txt
    # fallthrough
    UNIX_EXTRA_DATA = 0x7875
    # Unix extra data; UID / GID stuff, see
    ZIP64_EXTRA_HEADER = 0x0001
    zip64_extra_struct = Struct("<HHQQ")
    # ZIP64.
    # When a ZIP64 extra field is present this 8byte length
    # will override the 4byte length defined in canonical zips.
    # This is in the form:
    # - 0x0001 (header_id)
    # - 0x0010 [16] (header_length)
    # - ... (8byte uncompressed_length)
    # - ... (8byte compressed_length)
    mlen = offset + length
    while offset < mlen:
      values = list(extra_header_struct.unpack_from(mm, offset))
      _, header_length = values
      extra_struct = Struct("<HH" + "B" * header_length)
      values = list(extra_struct.unpack_from(mm, offset))
      header_id, header_length = values[:2]
      if header_id in (EXTENDED_TIME_DATA, UNIX_EXTRA_DATA):
        values[0] = STRIPZIP_OPTION_HEADER
        for i in range(2, len(values)):
          values[i] = 0xff
        extra_struct.pack_into(mm, offset, *values)
      if header_id == ZIP64_EXTRA_HEADER:
        assert header_length == 16
        values = list(zip64_extra_struct.unpack_from(mm, offset))
        header_id, header_length, _, compressed_size = values
      offset += extra_header_struct.size + header_length
    return compressed_size
  FILE_HEADER_SIGNATURE = 0x04034b50
  CENDIR_HEADER_SIGNATURE = 0x02014b50
  archive_size = os.fstat(zip_.fileno()).st_size
  signature_struct = Struct("<L")
  local_file_header_struct = Struct("<LHHHHHLLLHH")
  # 0. L signature
  # 1. H version_needed
  # 2. H gp_bits
  # 3. H compression_method
  # 4. H last_mod_time
  # 5. H last_mod_date
  # 6. L crc32
  # 7. L compressed_size
  # 8. L uncompressed_size
  # 9. H name_length
  # 10. H extra_field_length
  central_directory_header_struct = Struct("<LHHHHHHLLLHHHHHLL")
  # 1. H version_made_by
  # 2. H version_needed
  # 3. H gp_bits
  # 4. H compression_method
  # 5. H last_mod_time
  # 6. H last_mod_date
  # 7. L crc32
  # 8. L compressed_size
  # 9. L uncompressed_size
  # 10. H file_name_length
  # 11. H extra_field_length
  # 12. H file_comment_length
  # 13. H disk_number_start
  # 14. H internal_attr
  # 15. L external_attr
  # 16. L rel_offset_local_header
  offset = 0
  mm = mmap.mmap(zip_.fileno(), 0)
  while offset < archive_size:
    if signature_struct.unpack_from(mm, offset) != (FILE_HEADER_SIGNATURE,):
    values = list(local_file_header_struct.unpack_from(mm, offset))
    compressed_size, _, name_length, extra_field_length = values[7:11]
    # reset last_mod_time
    values[4] = 0
    # reset last_mod_date
    values[5] = 0x21
    local_file_header_struct.pack_into(mm, offset, *values)
    offset += local_file_header_struct.size + name_length
    if extra_field_length != 0:
      compressed_size = purify_extra_data(mm, offset, extra_field_length,
                                          compressed_size)
    offset += compressed_size + extra_field_length
    if signature_struct.unpack_from(mm, offset) != (CENDIR_HEADER_SIGNATURE,):
    values = list(central_directory_header_struct.unpack_from(mm, offset))
    file_name_length, extra_field_length, file_comment_length = values[10:13]
    values[5] = 0
    values[6] = 0x21
    central_directory_header_struct.pack_into(mm, offset, *values)
    offset += central_directory_header_struct.size
    offset += file_name_length + extra_field_length + file_comment_length
      purify_extra_data(mm, offset - extra_field_length, extra_field_length)
  if offset == 0:
    raise NonZipFileError(zip_.name)
def upload_electron(release, file_path, args):
  filename = os.path.basename(file_path)
  # Strip zip non determinism before upload, in-place operation
    zero_zip_date_time(file_path)
  except NonZipFileError:
  # if upload_to_storage is set, skip github upload.
  # todo (vertedinde): migrate this variable to upload_to_storage
    key_prefix = f'release-builds/{args.version}_{args.upload_timestamp}'
    store_artifact(os.path.dirname(file_path), key_prefix, [file_path])
    upload_sha256_checksum(args.version, file_path, key_prefix)
  # Upload the file.
  upload_io_to_github(release, filename, file_path, args.version)
  # Upload the checksum file.
  upload_sha256_checksum(args.version, file_path)
def upload_io_to_github(release, filename, filepath, version):
  print(f'Uploading {filename} to GitHub')
    ELECTRON_DIR, 'script', 'release', 'uploaders', 'upload-to-github.ts')
  with subprocess.Popen([TS_NODE, script_path, filepath,
                         filename, str(release['id']), version],
                        stderr=subprocess.STDOUT) as upload_process:
    if is_verbose_mode():
      for c in iter(lambda: upload_process.stdout.read(1), b""):
        sys.stdout.buffer.write(c)
    upload_process.wait()
    if upload_process.returncode != 0:
      sys.exit(upload_process.returncode)
  if "GITHUB_OUTPUT" in os.environ:
    output_path = os.environ["GITHUB_OUTPUT"]
    with open(output_path, "r+", encoding='utf-8') as github_output:
      if len(github_output.readlines()) > 0:
        github_output.write(",")
        github_output.write('UPLOADED_PATHS=')
      github_output.write(filepath)
def upload_sha256_checksum(version, file_path, key_prefix=None):
  checksum_path = f'{file_path}.sha256sum'
  if key_prefix is None:
    key_prefix = f'checksums-scratchpad/{version}'
  sha256 = hashlib.sha256()
  with open(file_path, 'rb') as f:
    sha256.update(f.read())
  with open(checksum_path, 'w', encoding='utf-8') as checksum:
    checksum.write(f'{sha256.hexdigest()} *{filename}')
  store_artifact(os.path.dirname(checksum_path), key_prefix, [checksum_path])
def get_release(version):
    ELECTRON_DIR, 'script', 'release', 'find-github-release.ts')
  # Strip warnings from stdout to ensure the only output is the desired object
  release_env = os.environ.copy()
  release_env['NODE_NO_WARNINGS'] = '1'
  release_info = execute([TS_NODE, script_path, version], release_env)
  verbose_mode_print(f'Release info for version: {version}:\n')
  verbose_mode_print(release_info)
  release = json.loads(release_info)
  return release
from .file_object import FileObject
__all__ = ["Upload"]
class Upload(BaseModel):
    """The Upload object can accept byte chunks in the form of Parts."""
    """The Upload unique identifier, which can be referenced in API endpoints."""
    """The intended number of bytes to be uploaded."""
    """The Unix timestamp (in seconds) for when the Upload was created."""
    expires_at: int
    """The Unix timestamp (in seconds) for when the Upload will expire."""
    """The name of the file to be uploaded."""
    object: Literal["upload"]
    """The object type, which is always "upload"."""
    [Please refer here](https://platform.openai.com/docs/api-reference/files/object#files/object-purpose)
    for acceptable values.
    status: Literal["pending", "completed", "cancelled", "expired"]
    """The status of the Upload."""
    file: Optional[FileObject] = None
distutils.command.upload
Implements the Distutils 'upload' subcommand (upload package to a package
index).
from base64 import standard_b64encode
from urllib.request import urlopen, Request, HTTPError
from distutils.errors import DistutilsError, DistutilsOptionError
from distutils.core import PyPIRCCommand
# PyPI Warehouse supports MD5, SHA256, and Blake2 (blake2-256)
# https://bugs.python.org/issue40698
_FILE_CONTENT_DIGESTS = {
    "md5_digest": getattr(hashlib, "md5", None),
    "sha256_digest": getattr(hashlib, "sha256", None),
    "blake2_256_digest": getattr(hashlib, "blake2b", None),
class upload(PyPIRCCommand):
    description = "upload binary package to PyPI"
    user_options = PyPIRCCommand.user_options + [
        ('sign', 's', 'sign files to upload using gpg'),
        ('identity=', 'i', 'GPG identity used to sign files'),
    boolean_options = PyPIRCCommand.boolean_options + ['sign']
        PyPIRCCommand.initialize_options(self)
        self.username = ''
        self.password = ''
        self.sign = False
        self.identity = None
        PyPIRCCommand.finalize_options(self)
        if self.identity and not self.sign:
            raise DistutilsOptionError("Must use --sign for --identity to have meaning")
        config = self._read_pypirc()
        if config != {}:
            self.username = config['username']
            self.password = config['password']
            self.repository = config['repository']
            self.realm = config['realm']
        # getting the password from the distribution
        # if previously set by the register command
        if not self.password and self.distribution.password:
            self.password = self.distribution.password
        if not self.distribution.dist_files:
                "Must create and upload files in one command "
                "(e.g. setup.py sdist upload)"
            raise DistutilsOptionError(msg)
        for command, pyversion, filename in self.distribution.dist_files:
            self.upload_file(command, pyversion, filename)
    def upload_file(self, command, pyversion, filename):
        # Makes sure the repository URL is compliant
        schema, netloc, url, params, query, fragments = urlparse(self.repository)
        if params or query or fragments:
            raise AssertionError("Incompatible url %s" % self.repository)
        if schema not in ('http', 'https'):
            raise AssertionError("unsupported schema " + schema)
        # Sign if requested
        if self.sign:
            gpg_args = ["gpg", "--detach-sign", "-a", filename]
            if self.identity:
                gpg_args[2:2] = ["--local-user", self.identity]
            spawn(gpg_args, dry_run=self.dry_run)
        # Fill in the data - send all the meta-data in case we need to
        # register a new release
        f = open(filename, 'rb')
        meta = self.distribution.metadata
            # action
            ':action': 'file_upload',
            'protocol_version': '1',
            # identify release
            'name': meta.get_name(),
            'version': meta.get_version(),
            # file content
            'content': (os.path.basename(filename), content),
            'filetype': command,
            'pyversion': pyversion,
            # additional meta-data
            'metadata_version': '1.0',
            'summary': meta.get_description(),
            'home_page': meta.get_url(),
            'author': meta.get_contact(),
            'author_email': meta.get_contact_email(),
            'license': meta.get_licence(),
            'description': meta.get_long_description(),
            'keywords': meta.get_keywords(),
            'platform': meta.get_platforms(),
            'classifiers': meta.get_classifiers(),
            'download_url': meta.get_download_url(),
            # PEP 314
            'provides': meta.get_provides(),
            'requires': meta.get_requires(),
            'obsoletes': meta.get_obsoletes(),
        data['comment'] = ''
        # file content digests
        for digest_name, digest_cons in _FILE_CONTENT_DIGESTS.items():
            if digest_cons is None:
                data[digest_name] = digest_cons(content).hexdigest()
                # hash digest not available or blocked by security policy
            with open(filename + ".asc", "rb") as f:
                data['gpg_signature'] = (os.path.basename(filename) + ".asc", f.read())
        # set up the authentication
        user_pass = (self.username + ":" + self.password).encode('ascii')
        # The exact encoding of the authentication string is debated.
        # Anyway PyPI only accepts ascii for both username or password.
        auth = "Basic " + standard_b64encode(user_pass).decode('ascii')
        # Build up the MIME payload for the POST data
        boundary = '--------------GHSKFJDLGDS7543FJKLFHRE75642756743254'
        sep_boundary = b'\r\n--' + boundary.encode('ascii')
        end_boundary = sep_boundary + b'--\r\n'
        body = io.BytesIO()
            title = '\r\nContent-Disposition: form-data; name="%s"' % key
            # handle multiple entries for the same name
            for value in value:
                if type(value) is tuple:
                    title += '; filename="%s"' % value[0]
                    value = value[1]
                    value = str(value).encode('utf-8')
                body.write(sep_boundary)
                body.write(title.encode('utf-8'))
                body.write(b"\r\n\r\n")
                body.write(value)
        body.write(end_boundary)
        body = body.getvalue()
        msg = "Submitting %s to %s" % (filename, self.repository)
        self.announce(msg, log.INFO)
        # build the Request
            'Content-type': 'multipart/form-data; boundary=%s' % boundary,
            'Content-length': str(len(body)),
            'Authorization': auth,
        request = Request(self.repository, data=body, headers=headers)
        # send the data
            result = urlopen(request)
            status = result.getcode()
            reason = result.msg
            status = e.code
            reason = e.msg
            self.announce(str(e), log.ERROR)
        if status == 200:
            self.announce('Server response (%s): %s' % (status, reason), log.INFO)
            if self.show_response:
                text = self._read_pypi_response(result)
                msg = '\n'.join(('-' * 75, text, '-' * 75))
            msg = 'Upload failed (%s): %s' % (status, reason)
            self.announce(msg, log.ERROR)
            raise DistutilsError(msg)
from distutils.command import upload as orig
from setuptools.errors import RemovedCommandError
class upload(orig.upload):
    """Formerly used to upload packages to PyPI."""
            "The upload command has been removed, use twine to upload "
            + "instead (https://pypi.org/p/twine)"
        self.announce("ERROR: " + msg, log.ERROR)
        raise RemovedCommandError(msg)
# Copyright 2023-present, the HuggingFace Inc. team.
"""Contains command to upload a repo or file with the CLI.
    # Upload file (implicit)
    hf upload my-cool-model ./my-cool-model.safetensors
    # Upload file (explicit)
    hf upload my-cool-model ./my-cool-model.safetensors  model.safetensors
    # Upload directory (implicit). If `my-cool-model/` is a directory it will be uploaded, otherwise an exception is raised.
    hf upload my-cool-model
    # Upload directory (explicit)
    hf upload my-cool-model ./models/my-cool-model .
    # Upload filtered directory (example: tensorboard logs except for the last run)
    hf upload my-cool-model ./model/training /logs --include "*.tfevents.*" --exclude "*20230905*"
    # Upload with wildcard
    hf upload my-cool-model "./model/training/*.safetensors"
    # Upload private dataset
    hf upload Wauplin/my-cool-dataset ./data . --repo-type=dataset --private
    # Upload with token
    hf upload Wauplin/my-cool-model --token=hf_****
    # Sync local Space with Hub (upload new files, delete removed files)
    hf upload Wauplin/space-example --repo-type=space --exclude="/logs/*" --delete="*" --commit-message="Sync local Space with Hub"
    # Schedule commits every 30 minutes
    hf upload Wauplin/my-cool-model --every=30
from huggingface_hub import logging
from huggingface_hub._commit_scheduler import CommitScheduler
from huggingface_hub.errors import RevisionNotFoundError
from huggingface_hub.utils import disable_progress_bars, enable_progress_bars
    PrivateOpt,
    RepoType,
UPLOAD_EXAMPLES = [
    "hf upload my-cool-model . .",
    "hf upload Wauplin/my-cool-model ./models/model.safetensors",
    "hf upload Wauplin/my-cool-dataset ./data /train --repo-type=dataset",
    'hf upload Wauplin/my-cool-model ./models . --commit-message="Epoch 34/50" --commit-description="Val accuracy: 68%"',
    "hf upload bigcode/the-stack . . --repo-type dataset --create-pr",
    local_path: Annotated[
            help="Local path to the file or folder to upload. Wildcard patterns are supported. Defaults to current directory.",
    path_in_repo: Annotated[
            help="Path of the file or folder in the repo. Defaults to the relative path of the file or folder.",
    repo_type: RepoTypeOpt = RepoType.model,
    private: PrivateOpt = None,
    include: Annotated[
            help="Glob patterns to match files to upload.",
    exclude: Annotated[
            help="Glob patterns to exclude from files to upload.",
    delete: Annotated[
            help="Glob patterns for file to be deleted from the repo while committing.",
    commit_message: Annotated[
            help="The summary / title / first line of the generated commit.",
    commit_description: Annotated[
            help="The description of the generated commit.",
    create_pr: Annotated[
            help="Whether to upload content as a new Pull Request.",
    every: Annotated[
            help="f set, a background job is scheduled to create commits every `every` minutes.",
            help="Disable progress bars and warnings; print only the returned path.",
    """Upload a file or a folder to the Hub. Recommended for single-commit uploads."""
    if every is not None and every <= 0:
        raise typer.BadParameter("--every must be a positive value", param_hint="every")
    repo_type_str = repo_type.value
    # Resolve local_path and path_in_repo based on implicit/explicit rules
    resolved_local_path, resolved_path_in_repo, resolved_include = _resolve_upload_paths(
        repo_id=repo_id, local_path=local_path, path_in_repo=path_in_repo, include=include
    def run_upload() -> str:
        if os.path.isfile(resolved_local_path):
            if resolved_include is not None and len(resolved_include) > 0 and isinstance(resolved_include, list):
                warnings.warn("Ignoring --include since a single file is uploaded.")
            if exclude is not None and len(exclude) > 0:
                warnings.warn("Ignoring --exclude since a single file is uploaded.")
            if delete is not None and len(delete) > 0:
                warnings.warn("Ignoring --delete since a single file is uploaded.")
        # Schedule commits if `every` is set
        if every is not None:
                # If file => watch entire folder + use allow_patterns
                folder_path = os.path.dirname(resolved_local_path)
                pi = (
                    resolved_path_in_repo[: -len(resolved_local_path)]
                    if resolved_path_in_repo.endswith(resolved_local_path)
                    else resolved_path_in_repo
                allow_patterns = [resolved_local_path]
                ignore_patterns: Optional[list[str]] = []
                folder_path = resolved_local_path
                pi = resolved_path_in_repo
                allow_patterns = (
                    resolved_include or []
                    if isinstance(resolved_include, list)
                    else [resolved_include]
                    if isinstance(resolved_include, str)
                ignore_patterns = exclude or []
                    warnings.warn("Ignoring --delete when uploading with scheduled commits.")
            scheduler = CommitScheduler(
                folder_path=folder_path,
                repo_type=repo_type_str,
                allow_patterns=allow_patterns,
                path_in_repo=pi,
                every=every,
                hf_api=api,
            print(f"Scheduling commits every {every} minutes to {scheduler.repo_id}.")
                    time.sleep(100)
                scheduler.stop()
                return "Stopped scheduled commits."
        # Otherwise, create repo and proceed with the upload
        if not os.path.isfile(resolved_local_path) and not os.path.isdir(resolved_local_path):
            raise FileNotFoundError(f"No such file or directory: '{resolved_local_path}'.")
        created = api.create_repo(
            exist_ok=True,
            space_sdk="gradio" if repo_type_str == "space" else None,
            # ^ We don't want it to fail when uploading to a Space => let's set Gradio by default.
            # ^ I'd rather not add CLI args to set it explicitly as we already have `hf repo create` for that.
        ).repo_id
        # Check if branch already exists and if not, create it
        if revision is not None and not create_pr:
                api.repo_info(repo_id=created, repo_type=repo_type_str, revision=revision)
            except RevisionNotFoundError:
                logger.info(f"Branch '{revision}' not found. Creating it...")
                api.create_branch(repo_id=created, repo_type=repo_type_str, branch=revision, exist_ok=True)
                # ^ `exist_ok=True` to avoid race concurrency issues
        # File-based upload
            return api.upload_file(
                path_or_fileobj=resolved_local_path,
                path_in_repo=resolved_path_in_repo,
                repo_id=created,
                commit_message=commit_message,
                commit_description=commit_description,
                create_pr=create_pr,
        # Folder-based upload
        return api.upload_folder(
            folder_path=resolved_local_path,
            allow_patterns=(
                resolved_include
            ignore_patterns=exclude,
            delete_patterns=delete,
        disable_progress_bars()
            print(run_upload())
        enable_progress_bars()
        logging.set_verbosity_warning()
def _resolve_upload_paths(
    *, repo_id: str, local_path: Optional[str], path_in_repo: Optional[str], include: Optional[list[str]]
) -> tuple[str, str, Optional[list[str]]]:
    repo_name = repo_id.split("/")[-1]
    resolved_include = include
    if local_path is not None and any(c in local_path for c in ["*", "?", "["]):
            raise ValueError("Cannot set --include when local_path contains a wildcard.")
        if path_in_repo is not None and path_in_repo != ".":
            raise ValueError("Cannot set path_in_repo when local_path contains a wildcard.")
        return ".", local_path, ["."]  # will be adjusted below; placeholder for type
    if local_path is None and os.path.isfile(repo_name):
        return repo_name, repo_name, resolved_include
    if local_path is None and os.path.isdir(repo_name):
        return repo_name, ".", resolved_include
    if local_path is None:
        raise ValueError(f"'{repo_name}' is not a local file or folder. Please set local_path explicitly.")
    if path_in_repo is None and os.path.isfile(local_path):
        return local_path, os.path.basename(local_path), resolved_include
    if path_in_repo is None:
        return local_path, ".", resolved_include
    return local_path, path_in_repo, resolved_include
