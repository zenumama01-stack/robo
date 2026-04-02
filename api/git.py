"""Git helper functions.
Everything here should be project agnostic: it shouldn't rely on project's
structure, or make assumptions about the passed arguments or calls' outcomes.
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
sys.path.append(SCRIPT_DIR)
from patches import PATCH_FILENAME_PREFIX, is_patch_location_line
UPSTREAM_HEAD='refs/patches/upstream-head'
def is_repo_root(path):
  path_exists = os.path.exists(path)
  if not path_exists:
  git_folder_path = os.path.join(path, '.git')
  git_folder_exists = os.path.exists(git_folder_path)
  return git_folder_exists
def get_repo_root(path):
  """Finds a closest ancestor folder which is a repo root."""
  norm_path = os.path.normpath(path)
  norm_path_exists = os.path.exists(norm_path)
  if not norm_path_exists:
  if is_repo_root(norm_path):
    return norm_path
  parent_path = os.path.dirname(norm_path)
  # Check if we're in the root folder already.
  if parent_path == norm_path:
  return get_repo_root(parent_path)
def am(repo, patch_data, threeway=False, directory=None, exclude=None,
    committer_name=None, committer_email=None, keep_cr=True,
    output_prefix=None):
  # --keep-non-patch prevents stripping leading bracketed strings on the subject line
  args = ['--keep-non-patch']
  if threeway:
    args += ['--3way']
  if directory is not None:
    args += ['--directory', directory]
  if exclude is not None:
    for path_pattern in exclude:
      args += ['--exclude', path_pattern]
  if keep_cr is True:
    # Keep the CR of CRLF in case any patches target files with Windows line
    # endings.
    args += ['--keep-cr']
  root_args = ['-C', repo]
  if committer_name is not None:
    root_args += ['-c', 'user.name=' + committer_name]
  if committer_email is not None:
    root_args += ['-c', 'user.email=' + committer_email]
  root_args += ['-c', 'commit.gpgsign=false']
  # git am rewrites the index 2-3x per patch. In large repos (Chromium's
  # index is ~70MB / ~500K files) this dominates wall time. skipHash
  # avoids recomputing the trailing SHA over the full index on every
  # write, and index v4 roughly halves the on-disk size via path prefix
  # compression. Also skip per-object fsync and auto-gc since a crashed
  # apply is simply re-run from a clean reset.
  root_args += [
    '-c', 'index.skipHash=true',
    '-c', 'index.version=4',
    '-c', 'core.fsync=none',
    '-c', 'gc.auto=0',
  command = ['git'] + root_args + ['am'] + args
  popen_kwargs = {'stdin': subprocess.PIPE}
  if output_prefix is not None:
    popen_kwargs['stdout'] = subprocess.PIPE
    popen_kwargs['stderr'] = subprocess.STDOUT
  with subprocess.Popen(command, **popen_kwargs) as proc:
    def feed_stdin():
      proc.stdin.write(patch_data.encode('utf-8'))
      proc.stdin.close()
      writer = threading.Thread(target=feed_stdin)
      writer.start()
      for line in proc.stdout:
          sys.stdout.write(
            f'{output_prefix}{line.decode("utf-8", "replace")}')
          sys.stdout.flush()
        except BrokenPipeError:
      writer.join()
      feed_stdin()
    if proc.returncode != 0:
      raise RuntimeError(f"Command {command} returned {proc.returncode}")
def import_patches(repo, ref=UPSTREAM_HEAD, **kwargs):
  """same as am(), but we save the upstream HEAD so we can refer to it when we
  later export patches"""
  update_ref(repo=repo, ref=ref, newvalue='HEAD')
  # Upgrade to index v4 before applying so every intermediate index write
  # during am benefits from path-prefix compression (roughly halves index
  # size in large repos).
  subprocess.call(
    ['git', '-C', repo, 'update-index', '--index-version', '4'],
    stderr=subprocess.DEVNULL)
  am(repo=repo, **kwargs)
def update_ref(repo, ref, newvalue):
  args = ['git', '-C', repo, 'update-ref', ref, newvalue]
  return subprocess.check_call(args)
def get_commit_for_ref(repo, ref):
  args = ['git', '-C', repo, 'rev-parse', '--verify', ref]
  return subprocess.check_output(args).decode('utf-8').strip()
def get_commit_count(repo, commit_range):
  args = ['git', '-C', repo, 'rev-list', '--count', commit_range]
  return int(subprocess.check_output(args).decode('utf-8').strip())
def guess_base_commit(repo, ref):
  """Guess which commit the patches might be based on"""
    upstream_head = get_commit_for_ref(repo, ref)
    num_commits = get_commit_count(repo, upstream_head + '..')
    return [upstream_head, num_commits]
    args = [
      'git',
      '-C',
      repo,
      'describe',
      '--tags',
    return subprocess.check_output(args).decode('utf-8').rsplit('-', 2)[0:2]
def format_patch(repo, since):
    'core.attributesfile='
    + os.path.join(
        os.path.dirname(os.path.realpath(__file__)),
        'electron.gitattributes',
    # Pin rename/copy detection to git's default so that patch output is
    # deterministic regardless of local or system-level diff.renames config
    # (e.g. 'copies', which would encode similar new files as copies).
    'diff.renames=true',
    # Ensure it is not possible to match anything
    # Disabled for now as we have consistent chunk headers
    # '-c',
    # 'diff.electron.xfuncname=$^',
    'format-patch',
    '--keep-subject',
    '--no-stat',
    '--stdout',
    # Per RFC 3676 the signature is separated from the body by a line with
    # '-- ' on it. If the signature option is omitted the signature defaults
    # to the Git version number.
    '--no-signature',
    # The name of the parent commit object isn't useful information in this
    # context, so zero it out to avoid needless patch-file churn.
    '--zero-commit',
    # Some versions of git print out different numbers of characters in the
    # 'index' line of patches, so pass --full-index to get consistent
    # behaviour.
    '--full-index',
    since
  return subprocess.check_output(args).decode('utf-8')
def split_patches(patch_data):
  """Split a concatenated series of patches into N separate patches"""
  patches = []
  patch_start = re.compile('^From [0-9a-f]+ ')
  # Keep line endings in case any patches target files with CRLF.
  keep_line_endings = True
  for line in patch_data.splitlines(keep_line_endings):
    if patch_start.match(line):
      patches.append([])
    patches[-1].append(line)
  return patches
def filter_patches(patches, key):
  """Return patches that include the specified key"""
  if key is None:
  matches = []
  for patch in patches:
    if any(key in line for line in patch):
      matches.append(patch)
  return matches
def munge_subject_to_filename(subject):
  """Derive a suitable filename from a commit's subject"""
  if subject.endswith('.patch'):
    subject = subject[:-6]
  return re.sub(r'[^A-Za-z0-9-]+', '_', subject).strip('_').lower() + '.patch'
def get_file_name(patch):
  """Return the name of the file to which the patch should be written"""
  file_name = None
  for line in patch:
    if line.startswith(PATCH_FILENAME_PREFIX):
      file_name = line[len(PATCH_FILENAME_PREFIX):]
  # If no patch-filename header, munge the subject.
  if not file_name:
      if line.startswith('Subject: '):
        file_name = munge_subject_to_filename(line[len('Subject: '):])
  return file_name.rstrip('\n')
def join_patch(patch):
  """Joins and formats patch contents"""
  return ''.join(remove_patch_location(patch)).rstrip('\n') + '\n'
def remove_patch_location(patch):
  """Strip out the patch location lines from a patch's message body"""
  force_keep_next_line = False
  n = len(patch)
  for i, l in enumerate(patch):
    skip_line = is_patch_location_line(l)
    skip_next = i < n - 1 and is_patch_location_line(patch[i + 1])
    if not force_keep_next_line and (
      skip_line or (skip_next and len(l.rstrip()) == 0)
    ):
      pass  # drop this line
      yield l
    force_keep_next_line = l.startswith('Subject: ')
def export_patches(repo, out_dir,
                   patch_range=None, ref=UPSTREAM_HEAD,
                   dry_run=False, grep=None):
    sys.stderr.write(
      f"Skipping patches in {repo} because it does not exist.\n"
  if patch_range is None:
    patch_range, n_patches = guess_base_commit(repo, ref)
    msg = f"Exporting {n_patches} patches in {repo} since {patch_range[0:7]}\n"
    sys.stderr.write(msg)
  patch_data = format_patch(repo, patch_range)
  patches = split_patches(patch_data)
  if grep:
    olen = len(patches)
    patches = filter_patches(patches, grep)
    sys.stderr.write(f"Exporting {len(patches)} of {olen} patches\n")
    os.mkdir(out_dir)
  except OSError:
  if dry_run:
    # If we're doing a dry run, iterate through each patch and see if the newly
    # exported patch differs from what exists. Report number of mismatched
    # patches and fail if there's more than one.
    bad_patches = []
      filename = get_file_name(patch)
      filepath = posixpath.join(out_dir, filename)
      with io.open(filepath, 'rb') as inp:
        existing_patch = str(inp.read(), 'utf-8')
      formatted_patch = join_patch(patch)
      if formatted_patch != existing_patch:
        bad_patches.append(filename)
    if len(bad_patches) > 0:
        "Patches in {} not up to date: {} patches need update\n-- {}\n".format(
          out_dir, len(bad_patches), "\n-- ".join(bad_patches)
    # Remove old patches so that deleted commits are correctly reflected in the
    # patch files (as a removed file)
    for p in os.listdir(out_dir):
      if p.endswith('.patch'):
        os.remove(posixpath.join(out_dir, p))
    with io.open(
      posixpath.join(out_dir, '.patches'),
      'w',
      newline='\n',
    ) as pl:
        file_path = posixpath.join(out_dir, filename)
        # Write in binary mode to retain mixed line endings on write.
          file_path, 'wb'
        ) as f:
          f.write(formatted_patch.encode('utf-8'))
        pl.write(filename + '\n')
    from langchain_community.document_loaders import GitLoader
DEPRECATED_LOOKUP = {"GitLoader": "langchain_community.document_loaders"}
from dataclasses import replace
from pip._internal.exceptions import BadCommand, InstallationError
from pip._internal.utils.misc import HiddenText, display_path, hide_url
from pip._internal.utils.subprocess import make_command
from pip._internal.vcs.versioncontrol import (
    AuthInfo,
    RevOptions,
    VersionControl,
    find_path_to_project_root_from_repo_root,
urlsplit = urllib.parse.urlsplit
urlunsplit = urllib.parse.urlunsplit
GIT_VERSION_REGEX = re.compile(
    r"^git version "  # Prefix.
    r"(\d+)"  # Major.
    r"\.(\d+)"  # Dot, minor.
    r"(?:\.(\d+))?"  # Optional dot, patch.
    r".*$"  # Suffix, including any pre- and post-release segments we don't care about.
HASH_REGEX = re.compile("^[a-fA-F0-9]{40}$")
# SCP (Secure copy protocol) shorthand. e.g. 'git@example.com:foo/bar.git'
SCP_REGEX = re.compile(
    r"""^
    # Optional user, e.g. 'git@'
    (\w+@)?
    # Server, e.g. 'github.com'.
    ([^/:]+):
    # The server-side path. e.g. 'user/project.git'. Must start with an
    # alphanumeric character so as not to be confusable with a Windows paths
    # like 'C:/foo/bar' or 'C:\foo\bar'.
    (\w[^:]*)
    $""",
def looks_like_hash(sha: str) -> bool:
    return bool(HASH_REGEX.match(sha))
class Git(VersionControl):
    name = "git"
    dirname = ".git"
    repo_name = "clone"
    schemes = (
        "git+http",
        "git+https",
        "git+ssh",
        "git+git",
        "git+file",
    # Prevent the user's environment variables from interfering with pip:
    # https://github.com/pypa/pip/issues/1130
    unset_environ = ("GIT_DIR", "GIT_WORK_TREE")
    default_arg_rev = "HEAD"
    def get_base_rev_args(rev: str) -> list[str]:
        return [rev]
    def run_command(cls, *args: Any, **kwargs: Any) -> str:
        if os.environ.get("PIP_NO_INPUT"):
            extra_environ = kwargs.get("extra_environ", {})
            extra_environ["GIT_TERMINAL_PROMPT"] = "0"
            extra_environ["GIT_SSH_COMMAND"] = "ssh -oBatchMode=yes"
            kwargs["extra_environ"] = extra_environ
        return super().run_command(*args, **kwargs)
    def is_immutable_rev_checkout(self, url: str, dest: str) -> bool:
        _, rev_options = self.get_url_rev_options(hide_url(url))
        if not rev_options.rev:
        if not self.is_commit_id_equal(dest, rev_options.rev):
            # the current commit is different from rev,
            # which means rev was something else than a commit hash
        # return False in the rare case rev is both a commit hash
        # and a tag or a branch; we don't want to cache in that case
        # because that branch/tag could point to something else in the future
        is_tag_or_branch = bool(self.get_revision_sha(dest, rev_options.rev)[0])
        return not is_tag_or_branch
    def get_git_version(self) -> tuple[int, ...]:
        version = self.run_command(
            ["version"],
            command_desc="git version",
            show_stdout=False,
            stdout_only=True,
        match = GIT_VERSION_REGEX.match(version)
            logger.warning("Can't parse git version: %s", version)
        return (int(match.group(1)), int(match.group(2)))
    def get_current_branch(cls, location: str) -> str | None:
        Return the current branch, or None if HEAD isn't at a branch
        (e.g. detached HEAD).
        # git-symbolic-ref exits with empty stdout if "HEAD" is a detached
        # HEAD rather than a symbolic ref.  In addition, the -q causes the
        # command to exit with status code 1 instead of 128 in this case
        # and to suppress the message to stderr.
        args = ["symbolic-ref", "-q", "HEAD"]
        output = cls.run_command(
            extra_ok_returncodes=(1,),
            cwd=location,
        ref = output.strip()
        if ref.startswith("refs/heads/"):
            return ref[len("refs/heads/") :]
    def get_revision_sha(cls, dest: str, rev: str) -> tuple[str | None, bool]:
        Return (sha_or_none, is_branch), where sha_or_none is a commit hash
        if the revision names a remote branch or tag, otherwise None.
          dest: the repository directory.
          rev: the revision name.
        # Pass rev to pre-filter the list.
            ["show-ref", rev],
            cwd=dest,
            on_returncode="ignore",
        refs = {}
        # NOTE: We do not use splitlines here since that would split on other
        #       unicode separators, which can be maliciously used to install a
        #       different revision.
        for line in output.strip().split("\n"):
            line = line.rstrip("\r")
                ref_sha, ref_name = line.split(" ", maxsplit=2)
                # Include the offending line to simplify troubleshooting if
                # this error ever occurs.
                raise ValueError(f"unexpected show-ref line: {line!r}")
            refs[ref_name] = ref_sha
        branch_ref = f"refs/remotes/origin/{rev}"
        tag_ref = f"refs/tags/{rev}"
        sha = refs.get(branch_ref)
        if sha is not None:
            return (sha, True)
        sha = refs.get(tag_ref)
        return (sha, False)
    def _should_fetch(cls, dest: str, rev: str) -> bool:
        Return true if rev is a ref or is a commit that we don't have locally.
        Branches and tags are not considered in this method because they are
        assumed to be always available locally (which is a normal outcome of
        ``git clone`` and ``git fetch --tags``).
        if rev.startswith("refs/"):
            # Always fetch remote refs.
        if not looks_like_hash(rev):
            # Git fetch would fail with abbreviated commits.
        if cls.has_commit(dest, rev):
            # Don't fetch if we have the commit locally.
    def resolve_revision(
        cls, dest: str, url: HiddenText, rev_options: RevOptions
    ) -> RevOptions:
        Resolve a revision to a new RevOptions object with the SHA1 of the
        branch, tag, or ref if found.
          rev_options: a RevOptions object.
        rev = rev_options.arg_rev
        # The arg_rev property's implementation for Git ensures that the
        # rev return value is always non-None.
        assert rev is not None
        sha, is_branch = cls.get_revision_sha(dest, rev)
            rev_options = rev_options.make_new(sha)
            rev_options = replace(rev_options, branch_name=(rev if is_branch else None))
            return rev_options
        # Do not show a warning for the common case of something that has
        # the form of a Git commit hash.
                "Did not find branch or tag '%s', assuming revision or ref.",
                rev,
        if not cls._should_fetch(dest, rev):
        # fetch the requested revision
        cls.run_command(
            make_command("fetch", "-q", url, rev_options.to_args()),
        # Change the revision to the SHA of the ref we fetched
        sha = cls.get_revision(dest, rev="FETCH_HEAD")
    def is_commit_id_equal(cls, dest: str, name: str | None) -> bool:
        Return whether the current commit hash equals the given name.
          name: a string name.
            # Then avoid an unnecessary subprocess call.
        return cls.get_revision(dest) == name
    def fetch_new(
        self, dest: str, url: HiddenText, rev_options: RevOptions, verbosity: int
        rev_display = rev_options.to_display()
        logger.info("Cloning %s%s to %s", url, rev_display, display_path(dest))
        if verbosity <= 0:
            flags: tuple[str, ...] = ("--quiet",)
        elif verbosity == 1:
            flags = ()
            flags = ("--verbose", "--progress")
        if self.get_git_version() >= (2, 17):
            # Git added support for partial clone in 2.17
            # https://git-scm.com/docs/partial-clone
            # Speeds up cloning by functioning without a complete copy of repository
            self.run_command(
                make_command(
                    "clone",
                    "--filter=blob:none",
                    *flags,
                    dest,
            self.run_command(make_command("clone", *flags, url, dest))
        if rev_options.rev:
            # Then a specific revision was requested.
            rev_options = self.resolve_revision(dest, url, rev_options)
            branch_name = getattr(rev_options, "branch_name", None)
            logger.debug("Rev options %s, branch_name %s", rev_options, branch_name)
            if branch_name is None:
                # Only do a checkout if the current commit id doesn't match
                # the requested revision.
                    cmd_args = make_command(
                        "checkout",
                        rev_options.to_args(),
                    self.run_command(cmd_args, cwd=dest)
            elif self.get_current_branch(dest) != branch_name:
                # Then a specific branch was requested, and that branch
                # is not yet checked out.
                track_branch = f"origin/{branch_name}"
                cmd_args = [
                    branch_name,
                    "--track",
                    track_branch,
            sha = self.get_revision(dest)
        logger.info("Resolved %s to commit %s", url, rev_options.rev)
        #: repo may contain submodules
        self.update_submodules(dest, verbosity=verbosity)
    def switch(
        dest: str,
        url: HiddenText,
        rev_options: RevOptions,
        verbosity: int = 0,
            make_command("config", "remote.origin.url", url),
        extra_flags = []
            extra_flags.append("-q")
        cmd_args = make_command("checkout", *extra_flags, rev_options.to_args())
        # First fetch changes from the default remote
        if self.get_git_version() >= (1, 9):
            # fetch tags in addition to everything else
            self.run_command(["fetch", "--tags", *extra_flags], cwd=dest)
            self.run_command(["fetch", *extra_flags], cwd=dest)
        # Then reset to wanted revision (maybe even origin/master)
            "reset",
            "--hard",
            *extra_flags,
        #: update submodules
    def get_remote_url(cls, location: str) -> str:
        Return URL of the first remote encountered.
        Raises RemoteNotFoundError if the repository does not have a remote
        url configured.
        # We need to pass 1 for extra_ok_returncodes since the command
        # exits with return code 1 if there are no matching lines.
        stdout = cls.run_command(
            ["config", "--get-regexp", r"remote\..*\.url"],
        remotes = stdout.splitlines()
            found_remote = remotes[0]
            raise RemoteNotFoundError
        for remote in remotes:
            if remote.startswith("remote.origin.url "):
                found_remote = remote
        url = found_remote.split(" ")[1]
        return cls._git_remote_to_pip_url(url.strip())
    def _git_remote_to_pip_url(url: str) -> str:
        Convert a remote url from what git uses to what pip accepts.
        There are 3 legal forms **url** may take:
            1. A fully qualified url: ssh://git@example.com/foo/bar.git
            2. A local project.git folder: /path/to/bare/repository.git
            3. SCP shorthand for form 1: git@example.com:foo/bar.git
        Form 1 is output as-is. Form 2 must be converted to URI and form 3 must
        be converted to form 1.
        See the corresponding test test_git_remote_url_to_pip() for examples of
        sample inputs/outputs.
        if re.match(r"\w+://", url):
            # This is already valid. Pass it though as-is.
        if os.path.exists(url):
            # A local bare remote (git clone --mirror).
            # Needs a file:// prefix.
            return pathlib.PurePath(url).as_uri()
        scp_match = SCP_REGEX.match(url)
        if scp_match:
            # Add an ssh:// prefix and replace the ':' with a '/'.
            return scp_match.expand(r"ssh://\1\2/\3")
        # Otherwise, bail out.
        raise RemoteNotValidError(url)
    def has_commit(cls, location: str, rev: str) -> bool:
        Check if rev is a commit that is available in the local repository.
                ["rev-parse", "-q", "--verify", "sha^" + rev],
                log_failed_cmd=False,
        except InstallationError:
    def get_revision(cls, location: str, rev: str | None = None) -> str:
        if rev is None:
            rev = "HEAD"
        current_rev = cls.run_command(
            ["rev-parse", rev],
        return current_rev.strip()
    def get_subdirectory(cls, location: str) -> str | None:
        Return the path to Python project root, relative to the repo root.
        Return None if the project root is in the repo root.
        # find the repo root
        git_dir = cls.run_command(
            ["rev-parse", "--git-dir"],
        if not os.path.isabs(git_dir):
            git_dir = os.path.join(location, git_dir)
        repo_root = os.path.abspath(os.path.join(git_dir, ".."))
        return find_path_to_project_root_from_repo_root(location, repo_root)
    def get_url_rev_and_auth(cls, url: str) -> tuple[str, str | None, AuthInfo]:
        Prefixes stub URLs like 'user@hostname:user/repo.git' with 'ssh://'.
        That's required because although they use SSH they sometimes don't
        work with a ssh:// scheme (e.g. GitHub). But we need a scheme for
        parsing. Hence we remove it again afterwards and return it as a stub.
        # Works around an apparent Git bug
        # (see https://article.gmane.org/gmane.comp.version-control.git/146500)
        scheme, netloc, path, query, fragment = urlsplit(url)
        if scheme.endswith("file"):
            initial_slashes = path[: -len(path.lstrip("/"))]
            newpath = initial_slashes + urllib.request.url2pathname(path).replace(
                "\\", "/"
            ).lstrip("/")
            after_plus = scheme.find("+") + 1
            url = scheme[:after_plus] + urlunsplit(
                (scheme[after_plus:], netloc, newpath, query, fragment),
        if "://" not in url:
            assert "file:" not in url
            url = url.replace("git+", "git+ssh://")
            url, rev, user_pass = super().get_url_rev_and_auth(url)
            url = url.replace("ssh://", "")
        return url, rev, user_pass
    def update_submodules(cls, location: str, verbosity: int = 0) -> None:
        argv = ["submodule", "update", "--init", "--recursive"]
            argv.append("-q")
        if not os.path.exists(os.path.join(location, ".gitmodules")):
    def get_repository_root(cls, location: str) -> str | None:
        loc = super().get_repository_root(location)
        if loc:
            return loc
            r = cls.run_command(
                ["rev-parse", "--show-toplevel"],
                on_returncode="raise",
        except BadCommand:
                "could not determine if %s is under git control "
                "because git is not available",
        return os.path.normpath(r.rstrip("\r\n"))
    def should_add_vcs_url_prefix(repo_url: str) -> bool:
        """In either https or ssh form, requirements must be prefixed with git+."""
vcs.register(Git)
import pygit2
from .memory import MemoryFile
class GitFileSystem(AbstractFileSystem):
    """Browse the files of a local git repo at any hash/tag/branch
    (experimental backend)
    root_marker = ""
    cachable = True
    def __init__(self, path=None, fo=None, ref=None, **kwargs):
        path: str (optional)
            Local location of the repo (uses current directory if not given).
            May be deprecated in favour of ``fo``. When used with a higher
            level function such as fsspec.open(), may be of the form
            "git://[path-to-repo[:]][ref@]path/to/file" (but the actual
            file path should not contain "@" or ":").
        fo: str (optional)
            Same as ``path``, but passed as part of a chained URL. This one
            takes precedence if both are given.
        ref: str (optional)
            Reference to work with, could be a hash, tag or branch name. Defaults
            to current working tree. Note that ``ls`` and ``open`` also take hash,
            so this becomes the default for those operations
        self.repo = pygit2.Repository(fo or path or os.getcwd())
        self.ref = ref or "master"
    def _strip_protocol(cls, path):
        path = super()._strip_protocol(path).lstrip("/")
        if ":" in path:
            path = path.split(":", 1)[1]
        if "@" in path:
            path = path.split("@", 1)[1]
        return path.lstrip("/")
    def _path_to_object(self, path, ref):
        comm, ref = self.repo.resolve_refish(ref or self.ref)
        parts = path.split("/")
        tree = comm.tree
            if part and isinstance(tree, pygit2.Tree):
                if part not in tree:
                    raise FileNotFoundError(path)
                tree = tree[part]
        return tree
    def _get_kwargs_from_urls(path):
        path = path.removeprefix("git://")
            out["path"], path = path.split(":", 1)
            out["ref"], path = path.split("@", 1)
    def _object_to_info(obj, path=None):
        # obj.name and obj.filemode are None for the root tree!
        is_dir = isinstance(obj, pygit2.Tree)
            "type": "directory" if is_dir else "file",
            "name": (
                "/".join([path, obj.name or ""]).lstrip("/") if path else obj.name
            "hex": str(obj.id),
            "mode": "100644" if obj.filemode is None else f"{obj.filemode:o}",
            "size": 0 if is_dir else obj.size,
    def ls(self, path, detail=True, ref=None, **kwargs):
        tree = self._path_to_object(self._strip_protocol(path), ref)
            GitFileSystem._object_to_info(obj, path)
            if detail
            else GitFileSystem._object_to_info(obj, path)["name"]
            for obj in (tree if isinstance(tree, pygit2.Tree) else [tree])
    def info(self, path, ref=None, **kwargs):
        return GitFileSystem._object_to_info(tree, path)
    def ukey(self, path, ref=None):
        return self.info(path, ref=ref)["hex"]
    def _open(
        mode="rb",
        block_size=None,
        autocommit=True,
        cache_options=None,
        ref=None,
        obj = self._path_to_object(path, ref or self.ref)
        return MemoryFile(data=obj.data)
from gitdb.db.base import (
    CompoundDB,
    ObjectDBW,
    FileDBBase
from gitdb.db.loose import LooseObjectDB
from gitdb.db.pack import PackedDB
from gitdb.db.ref import ReferenceDB
from gitdb.exc import InvalidDBRoot
__all__ = ('GitDB', )
class GitDB(FileDBBase, ObjectDBW, CompoundDB):
    """A git-style object database, which contains all objects in the 'objects'
    subdirectory
    ``IMPORTANT``: The usage of this implementation is highly discouraged as it fails to release file-handles.
    This can be a problem with long-running processes and/or big repositories.
    # Configuration
    PackDBCls = PackedDB
    LooseDBCls = LooseObjectDB
    ReferenceDBCls = ReferenceDB
    # Directories
    packs_dir = 'pack'
    loose_dir = ''
    alternates_dir = os.path.join('info', 'alternates')
        """Initialize ourselves on a git objects directory"""
        super().__init__(root_path)
        if attr == '_dbs' or attr == '_loose_db':
            loose_db = None
            for subpath, dbcls in ((self.packs_dir, self.PackDBCls),
                                   (self.loose_dir, self.LooseDBCls),
                                   (self.alternates_dir, self.ReferenceDBCls)):
                path = self.db_path(subpath)
                    self._dbs.append(dbcls(path))
                    if dbcls is self.LooseDBCls:
                        loose_db = self._dbs[-1]
                    # END remember loose db
                # END check path exists
            # END for each db type
            # should have at least one subdb
            if not self._dbs:
                raise InvalidDBRoot(self.root_path())
            # END handle error
            # we the first one should have the store method
            assert loose_db is not None and hasattr(loose_db, 'store'), "First database needs store functionality"
            # finally set the value
            self._loose_db = loose_db
        # END handle attrs
    #{ ObjectDBW interface
        return self._loose_db.store(istream)
        return self._loose_db.ostream()
    def set_ostream(self, ostream):
        return self._loose_db.set_ostream(ostream)
    #} END objectdbw interface
