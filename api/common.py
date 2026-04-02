class PathRoot(bb.Union):
    :ivar common.PathRoot.home: Paths are relative to the authenticating user's
        home namespace, whether or not that user belongs to a team.
    :ivar str common.PathRoot.root: Paths are relative to the authenticating
        user's root namespace (This results in
        :field:`PathRootError.invalid_root` if the user's root namespace has
        changed.).
    :ivar str common.PathRoot.namespace_id: Paths are relative to given
        namespace id (This results in :field:`PathRootError.no_permission` if
        you don't have access to this namespace.).
    home = None
    def root(cls, val):
        Create an instance of this class set to the ``root`` tag with value
        ``val``.
        :rtype: PathRoot
        return cls('root', val)
    def namespace_id(cls, val):
        Create an instance of this class set to the ``namespace_id`` tag with
        return cls('namespace_id', val)
    def is_home(self):
        Check if the union tag is ``home``.
        return self._tag == 'home'
    def is_root(self):
        Check if the union tag is ``root``.
        return self._tag == 'root'
    def is_namespace_id(self):
        Check if the union tag is ``namespace_id``.
        return self._tag == 'namespace_id'
    def get_root(self):
        Paths are relative to the authenticating user's root namespace (This
        results in ``PathRootError.invalid_root`` if the user's root namespace
        has changed.).
        Only call this if :meth:`is_root` is true.
        if not self.is_root():
            raise AttributeError("tag 'root' not set")
    def get_namespace_id(self):
        Paths are relative to given namespace id (This results in
        ``PathRootError.no_permission`` if you don't have access to this
        namespace.).
        Only call this if :meth:`is_namespace_id` is true.
        if not self.is_namespace_id():
            raise AttributeError("tag 'namespace_id' not set")
        super(PathRoot, self)._process_custom_annotations(annotation_type, field_path, processor)
PathRoot_validator = bv.Union(PathRoot)
class PathRootError(bb.Union):
    :ivar RootInfo PathRootError.invalid_root: The root namespace id in
        Dropbox-API-Path-Root header is not valid. The value of this error is
        the user's latest root info.
    :ivar common.PathRootError.no_permission: You don't have permission to
        access the namespace id in Dropbox-API-Path-Root  header.
    no_permission = None
    def invalid_root(cls, val):
        Create an instance of this class set to the ``invalid_root`` tag with
        :param RootInfo val:
        :rtype: PathRootError
        return cls('invalid_root', val)
    def is_invalid_root(self):
        Check if the union tag is ``invalid_root``.
        return self._tag == 'invalid_root'
    def is_no_permission(self):
        Check if the union tag is ``no_permission``.
        return self._tag == 'no_permission'
    def get_invalid_root(self):
        The root namespace id in Dropbox-API-Path-Root header is not valid. The
        value of this error is the user's latest root info.
        Only call this if :meth:`is_invalid_root` is true.
        :rtype: RootInfo
        if not self.is_invalid_root():
            raise AttributeError("tag 'invalid_root' not set")
        super(PathRootError, self)._process_custom_annotations(annotation_type, field_path, processor)
PathRootError_validator = bv.Union(PathRootError)
class RootInfo(bb.Struct):
    Information about current user's root.
    :ivar common.RootInfo.root_namespace_id: The namespace ID for user's root
        namespace. It will be the namespace ID of the shared team root if the
        user is member of a team with a separate team root. Otherwise it will be
        same as ``RootInfo.home_namespace_id``.
    :ivar common.RootInfo.home_namespace_id: The namespace ID for user's home
        namespace.
        '_root_namespace_id_value',
        '_home_namespace_id_value',
                 root_namespace_id=None,
                 home_namespace_id=None):
        self._root_namespace_id_value = bb.NOT_SET
        self._home_namespace_id_value = bb.NOT_SET
        if root_namespace_id is not None:
            self.root_namespace_id = root_namespace_id
        if home_namespace_id is not None:
            self.home_namespace_id = home_namespace_id
    root_namespace_id = bb.Attribute("root_namespace_id")
    home_namespace_id = bb.Attribute("home_namespace_id")
        super(RootInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
RootInfo_validator = bv.StructTree(RootInfo)
class TeamRootInfo(RootInfo):
    Root info when user is member of a team with a separate root namespace ID.
    :ivar common.TeamRootInfo.home_path: The path for user's home directory
        under the shared team root.
        '_home_path_value',
                 home_namespace_id=None,
                 home_path=None):
        super(TeamRootInfo, self).__init__(root_namespace_id,
                                           home_namespace_id)
        self._home_path_value = bb.NOT_SET
        if home_path is not None:
            self.home_path = home_path
    home_path = bb.Attribute("home_path")
        super(TeamRootInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamRootInfo_validator = bv.Struct(TeamRootInfo)
class UserRootInfo(RootInfo):
    Root info when user is not member of a team or the user is a member of a
    team and the team does not have a separate root namespace.
        super(UserRootInfo, self).__init__(root_namespace_id,
        super(UserRootInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
UserRootInfo_validator = bv.Struct(UserRootInfo)
Date_validator = bv.Timestamp('%Y-%m-%d')
DisplayName_validator = bv.String(pattern='[^/:?*<>"|]*')
DisplayNameLegacy_validator = bv.String()
DropboxTimestamp_validator = bv.Timestamp('%Y-%m-%dT%H:%M:%SZ')
EmailAddress_validator = bv.String(max_length=255, pattern="^['#&A-Za-z0-9._%+-]+@[A-Za-z0-9-][A-Za-z0-9.-]*\\.[A-Za-z]{2,15}$")
# A ISO639-1 code.
LanguageCode_validator = bv.String(min_length=2)
NamePart_validator = bv.String(min_length=1, max_length=100, pattern='[^/:?*<>"|]*')
NamespaceId_validator = bv.String(pattern='[-_0-9a-zA-Z:]+')
OptionalNamePart_validator = bv.String(max_length=100, pattern='[^/:?*<>"|]*')
SessionId_validator = bv.String()
SharedFolderId_validator = NamespaceId_validator
PathRoot._home_validator = bv.Void()
PathRoot._root_validator = NamespaceId_validator
PathRoot._namespace_id_validator = NamespaceId_validator
PathRoot._other_validator = bv.Void()
PathRoot._tagmap = {
    'home': PathRoot._home_validator,
    'root': PathRoot._root_validator,
    'namespace_id': PathRoot._namespace_id_validator,
    'other': PathRoot._other_validator,
PathRoot.home = PathRoot('home')
PathRoot.other = PathRoot('other')
PathRootError._invalid_root_validator = RootInfo_validator
PathRootError._no_permission_validator = bv.Void()
PathRootError._other_validator = bv.Void()
PathRootError._tagmap = {
    'invalid_root': PathRootError._invalid_root_validator,
    'no_permission': PathRootError._no_permission_validator,
    'other': PathRootError._other_validator,
PathRootError.no_permission = PathRootError('no_permission')
PathRootError.other = PathRootError('other')
RootInfo.root_namespace_id.validator = NamespaceId_validator
RootInfo.home_namespace_id.validator = NamespaceId_validator
RootInfo._field_names_ = set([
    'root_namespace_id',
    'home_namespace_id',
RootInfo._all_field_names_ = RootInfo._field_names_
RootInfo._fields_ = [
    ('root_namespace_id', RootInfo.root_namespace_id.validator),
    ('home_namespace_id', RootInfo.home_namespace_id.validator),
RootInfo._all_fields_ = RootInfo._fields_
RootInfo._tag_to_subtype_ = {
    ('team',): TeamRootInfo_validator,
    ('user',): UserRootInfo_validator,
RootInfo._pytype_to_tag_and_subtype_ = {
    TeamRootInfo: (('team',), TeamRootInfo_validator),
    UserRootInfo: (('user',), UserRootInfo_validator),
RootInfo._is_catch_all_ = True
TeamRootInfo.home_path.validator = bv.String()
TeamRootInfo._field_names_ = set(['home_path'])
TeamRootInfo._all_field_names_ = RootInfo._all_field_names_.union(TeamRootInfo._field_names_)
TeamRootInfo._fields_ = [('home_path', TeamRootInfo.home_path.validator)]
TeamRootInfo._all_fields_ = RootInfo._all_fields_ + TeamRootInfo._fields_
UserRootInfo._field_names_ = set([])
UserRootInfo._all_field_names_ = RootInfo._all_field_names_.union(UserRootInfo._field_names_)
UserRootInfo._fields_ = []
UserRootInfo._all_fields_ = RootInfo._all_fields_ + UserRootInfo._fields_
from ..minicurses import (
    BreaklineStatusPrinter,
    MultilineLogger,
    MultilinePrinter,
    QuietMultilinePrinter,
from ..utils import (
    IDENTITY,
    LockingUnsupportedError,
    RetryManager,
    classproperty,
    sanitize_open,
    timetuple_from_msec,
from ..utils._utils import _ProgressState
class FileDownloader:
    """File Downloader class.
    File downloader objects are the ones responsible of downloading the
    actual video file and writing it to disk.
    File downloaders accept a lot of parameters. In order not to saturate
    the object constructor with arguments, it receives a dictionary of
    options instead.
    Available options:
    verbose:            Print additional info to stdout.
    quiet:              Do not print messages to stdout.
    ratelimit:          Download speed limit, in bytes/sec.
    throttledratelimit: Assume the download is being throttled below this speed (bytes/sec)
    retries:            Number of times to retry for expected network errors.
                        Default is 0 for API, but 10 for CLI
    file_access_retries:   Number of times to retry on file access error (default: 3)
    buffersize:         Size of download buffer in bytes.
    noresizebuffer:     Do not automatically resize the download buffer.
    continuedl:         Try to continue downloads if possible.
    noprogress:         Do not print the progress bar.
    nopart:             Do not use temporary .part files.
    updatetime:         Use the Last-modified header to set output file timestamps.
    test:               Download only first bytes to test the downloader.
    min_filesize:       Skip files smaller than this size
    max_filesize:       Skip files larger than this size
    progress_delta:     The minimum time between progress output, in seconds
    external_downloader_args:  A dictionary of downloader keys (in lower case)
                        and a list of additional command-line arguments for the
                        executable. Use 'default' as the name for arguments to be
                        passed to all downloaders. For compatibility with youtube-dl,
                        a single list of args can also be used
    hls_use_mpegts:     Use the mpegts container for HLS videos.
    http_chunk_size:    Size of a chunk for chunk-based HTTP downloading. May be
                        useful for bypassing bandwidth throttling imposed by
                        a webserver (experimental)
    progress_template:  See YoutubeDL.py
    retry_sleep_functions: See YoutubeDL.py
    Subclasses of this one must re-define the real_download method.
    _TEST_FILE_SIZE = 10241
    def __init__(self, ydl, params):
        """Create a FileDownloader object with the given options."""
        self._set_ydl(ydl)
        self._progress_hooks = []
        self._prepare_multiline_status()
        self.add_progress_hook(self.report_progress)
        if self.params.get('progress_delta'):
            self._progress_delta_lock = threading.Lock()
            self._progress_delta_time = time.monotonic()
    def _set_ydl(self, ydl):
        self.ydl = ydl
        for func in (
            'deprecation_warning',
            'deprecated_feature',
            'report_error',
            'report_file_already_downloaded',
            'report_warning',
            'to_console_title',
            'to_stderr',
            'trouble',
            'write_debug',
            if not hasattr(self, func):
                setattr(self, func, getattr(ydl, func))
    def to_screen(self, *args, **kargs):
        self.ydl.to_screen(*args, quiet=self.params.get('quiet'), **kargs)
    __to_screen = to_screen
    @classproperty
    def FD_NAME(cls):
        return re.sub(r'(?<=[a-z])(?=[A-Z])', '_', cls.__name__[:-2]).lower()
    def format_seconds(seconds):
        if seconds is None:
            return ' Unknown'
        time = timetuple_from_msec(seconds * 1000)
        if time.hours > 99:
            return '--:--:--'
        return '%02d:%02d:%02d' % time[:-1]
    def format_eta(cls, seconds):
        return f'{remove_start(cls.format_seconds(seconds), "00:"):>8s}'
    def calc_percent(byte_counter, data_len):
        if data_len is None:
        return float(byte_counter) / float(data_len) * 100.0
    def format_percent(percent):
        return '  N/A%' if percent is None else f'{percent:>5.1f}%'
    def calc_eta(cls, start_or_rate, now_or_remaining, total=NO_DEFAULT, current=NO_DEFAULT):
        if total is NO_DEFAULT:
            rate, remaining = start_or_rate, now_or_remaining
            if None in (rate, remaining):
            return int(float(remaining) / rate)
        start, now = start_or_rate, now_or_remaining
        if total is None:
        if now is None:
            now = time.time()
        rate = cls.calc_speed(start, now, current)
        return rate and int((float(total) - float(current)) / rate)
    def calc_speed(start, now, bytes):
        dif = now - start
        if bytes == 0 or dif < 0.001:  # One millisecond
        return float(bytes) / dif
    def format_speed(speed):
        return ' Unknown B/s' if speed is None else f'{format_bytes(speed):>10s}/s'
    def format_retries(retries):
        return 'inf' if retries == float('inf') else int(retries)
    def filesize_or_none(unencoded_filename):
        if os.path.isfile(unencoded_filename):
            return os.path.getsize(unencoded_filename)
    def best_block_size(elapsed_time, bytes):
        new_min = max(bytes / 2.0, 1.0)
        new_max = min(max(bytes * 2.0, 1.0), 4194304)  # Do not surpass 4 MB
        if elapsed_time < 0.001:
            return int(new_max)
        rate = bytes / elapsed_time
        if rate > new_max:
        if rate < new_min:
            return int(new_min)
        return int(rate)
    def parse_bytes(bytestr):
        """Parse a string indicating a byte quantity into an integer."""
        deprecation_warning('yt_dlp.FileDownloader.parse_bytes is deprecated and '
                            'may be removed in the future. Use yt_dlp.utils.parse_bytes instead')
        return parse_bytes(bytestr)
    def slow_down(self, start_time, now, byte_counter):
        """Sleep if the download speed is over the rate limit."""
        rate_limit = self.params.get('ratelimit')
        if rate_limit is None or byte_counter == 0:
        elapsed = now - start_time
        if elapsed <= 0.0:
        speed = float(byte_counter) / elapsed
        if speed > rate_limit:
            sleep_time = float(byte_counter) / rate_limit - elapsed
    def temp_name(self, filename):
        """Returns a temporary filename for the given filename."""
        if self.params.get('nopart', False) or filename == '-' or \
                (os.path.exists(filename) and not os.path.isfile(filename)):
        return filename + '.part'
    def undo_temp_name(self, filename):
        if filename.endswith('.part'):
            return filename[:-len('.part')]
    def ytdl_filename(self, filename):
        return filename + '.ytdl'
    def wrap_file_access(action, *, fatal=False):
        def error_callback(err, count, retries, *, fd):
            return RetryManager.report_retry(
                err, count, retries, info=fd.__to_screen,
                warn=lambda e: (time.sleep(0.01), fd.to_screen(f'[download] Unable to {action} file: {e}')),
                error=None if fatal else lambda e: fd.report_error(f'Unable to {action} file: {e}'),
                sleep_func=fd.params.get('retry_sleep_functions', {}).get('file_access'))
        def wrapper(self, func, *args, **kwargs):
            for retry in RetryManager(self.params.get('file_access_retries', 3), error_callback, fd=self):
                    return func(self, *args, **kwargs)
                except OSError as err:
                    if err.errno in (errno.EACCES, errno.EINVAL):
                        retry.error = err
                    retry.error_callback(err, 1, 0)
        return functools.partial(functools.partialmethod, wrapper)
    @wrap_file_access('open', fatal=True)
    def sanitize_open(self, filename, open_mode):
        f, filename = sanitize_open(filename, open_mode)
        if not getattr(f, 'locked', None):
            self.write_debug(f'{LockingUnsupportedError.msg}. Proceeding without locking', only_once=True)
        return f, filename
    @wrap_file_access('remove')
    def try_remove(self, filename):
        if os.path.isfile(filename):
    @wrap_file_access('rename')
    def try_rename(self, old_filename, new_filename):
        if old_filename == new_filename:
        os.replace(old_filename, new_filename)
    def try_utime(self, filename, last_modified_hdr):
        """Try to set the last-modified time of the given file."""
        if last_modified_hdr is None:
        if not os.path.isfile(filename):
        timestr = last_modified_hdr
        if timestr is None:
        filetime = timeconvert(timestr)
        if filetime is None:
            return filetime
        # Ignore obviously invalid dates
        if filetime == 0:
        with contextlib.suppress(Exception):
            os.utime(filename, (time.time(), filetime))
    def report_destination(self, filename):
        """Report destination filename."""
        self.to_screen('[download] Destination: ' + filename)
    def _prepare_multiline_status(self, lines=1):
        if self.params.get('noprogress'):
            self._multiline = QuietMultilinePrinter()
        elif self.ydl.params.get('logger'):
            self._multiline = MultilineLogger(self.ydl.params['logger'], lines)
        elif self.params.get('progress_with_newline'):
            self._multiline = BreaklineStatusPrinter(self.ydl._out_files.out, lines)
            self._multiline = MultilinePrinter(self.ydl._out_files.out, lines, not self.params.get('quiet'))
        self._multiline.allow_colors = self.ydl._allow_colors.out and self.ydl._allow_colors.out != 'no_color'
        self._multiline._HAVE_FULLCAP = self.ydl._allow_colors.out
    def _finish_multiline_status(self):
        self._multiline.end()
    ProgressStyles = Namespace(
        downloaded_bytes='light blue',
        percent='light blue',
        eta='yellow',
        speed='green',
        elapsed='bold white',
        total_bytes='',
        total_bytes_estimate='',
    def _report_progress_status(self, s, default_template):
        for name, style in self.ProgressStyles.items_:
            name = f'_{name}_str'
            if name not in s:
            s[name] = self._format_progress(s[name], style)
        s['_default_template'] = default_template % s
        progress_dict = s.copy()
        progress_dict.pop('info_dict')
        progress_dict = {'info': s['info_dict'], 'progress': progress_dict}
        progress_template = self.params.get('progress_template', {})
        self._multiline.print_at_line(self.ydl.evaluate_outtmpl(
            progress_template.get('download') or '[download] %(progress._default_template)s',
            progress_dict), s.get('progress_idx') or 0)
        self.to_console_title(self.ydl.evaluate_outtmpl(
            progress_template.get('download-title') or 'yt-dlp %(progress._default_template)s',
            progress_dict), _ProgressState.from_dict(s), s.get('_percent'))
    def _format_progress(self, *args, **kwargs):
        return self.ydl._format_text(
            self._multiline.stream, self._multiline.allow_colors, *args, **kwargs)
    def report_progress(self, s):
        def with_fields(*tups, default=''):
            for *fields, tmpl in tups:
                if all(s.get(f) is not None for f in fields):
                    return tmpl
        _format_bytes = lambda k: f'{format_bytes(s.get(k)):>10s}'
        if s['status'] == 'finished':
                self.to_screen('[download] Download completed')
            speed = try_call(lambda: s['total_bytes'] / s['elapsed'])
            s.update({
                'speed': speed,
                '_speed_str': self.format_speed(speed).strip(),
                '_total_bytes_str': _format_bytes('total_bytes'),
                '_elapsed_str': self.format_seconds(s.get('elapsed')),
                '_percent': 100.0,
                '_percent_str': self.format_percent(100),
            self._report_progress_status(s, join_nonempty(
                '100%%',
                with_fields(('total_bytes', 'of %(_total_bytes_str)s')),
                with_fields(('elapsed', 'in %(_elapsed_str)s')),
                with_fields(('speed', 'at %(_speed_str)s')),
                delim=' '))
        if s['status'] != 'downloading':
        if update_delta := self.params.get('progress_delta'):
            with self._progress_delta_lock:
                if time.monotonic() < self._progress_delta_time:
                self._progress_delta_time += update_delta
        progress = try_call(
            lambda: 100 * s['downloaded_bytes'] / s['total_bytes'],
            lambda: 100 * s['downloaded_bytes'] / s['total_bytes_estimate'],
            lambda: s['downloaded_bytes'] == 0 and 0)
            '_eta_str': self.format_eta(s.get('eta')).strip(),
            '_speed_str': self.format_speed(s.get('speed')),
            '_percent': progress,
            '_percent_str': self.format_percent(progress),
            '_total_bytes_estimate_str': _format_bytes('total_bytes_estimate'),
            '_downloaded_bytes_str': _format_bytes('downloaded_bytes'),
        msg_template = with_fields(
            ('total_bytes', '%(_percent_str)s of %(_total_bytes_str)s at %(_speed_str)s ETA %(_eta_str)s'),
            ('total_bytes_estimate', '%(_percent_str)s of ~%(_total_bytes_estimate_str)s at %(_speed_str)s ETA %(_eta_str)s'),
            ('downloaded_bytes', 'elapsed', '%(_downloaded_bytes_str)s at %(_speed_str)s (%(_elapsed_str)s)'),
            ('downloaded_bytes', '%(_downloaded_bytes_str)s at %(_speed_str)s'),
            default='%(_percent_str)s at %(_speed_str)s ETA %(_eta_str)s')
        msg_template += with_fields(
            ('fragment_index', 'fragment_count', ' (frag %(fragment_index)s/%(fragment_count)s)'),
            ('fragment_index', ' (frag %(fragment_index)s)'))
        self._report_progress_status(s, msg_template)
    def report_resuming_byte(self, resume_len):
        """Report attempt to resume at given byte."""
        self.to_screen(f'[download] Resuming download at byte {resume_len}')
    def report_retry(self, err, count, retries, frag_index=NO_DEFAULT, fatal=True):
        """Report retry"""
        is_frag = False if frag_index is NO_DEFAULT else 'fragment'
        RetryManager.report_retry(
            err, count, retries, info=self.__to_screen,
            warn=lambda msg: self.__to_screen(f'[download] Got error: {msg}'),
            error=IDENTITY if not fatal else lambda e: self.report_error(f'\r[download] Got error: {e}'),
            sleep_func=self.params.get('retry_sleep_functions', {}).get(is_frag or 'http'),
            suffix=f'fragment{"s" if frag_index is None else f" {frag_index}"}' if is_frag else None)
    def report_unable_to_resume(self):
        """Report it was impossible to resume download."""
        self.to_screen('[download] Unable to resume')
    def supports_manifest(manifest):
        """ Whether the downloader can download the fragments from the manifest.
        Redefine in subclasses if needed. """
    def download(self, filename, info_dict, subtitle=False):
        """Download to a filename using the info from info_dict
        Return True on success and False otherwise
        nooverwrites_and_exists = (
            not self.params.get('overwrites', True)
            and os.path.exists(filename)
        if not hasattr(filename, 'write'):
            continuedl_and_exists = (
                self.params.get('continuedl', True)
                and os.path.isfile(filename)
                and not self.params.get('nopart', False)
            # Check file already present
            if filename != '-' and (nooverwrites_and_exists or continuedl_and_exists):
                self.report_file_already_downloaded(filename)
                self._hook_progress({
                    'filename': filename,
                    'status': 'finished',
                    'total_bytes': os.path.getsize(filename),
                }, info_dict)
                self._finish_multiline_status()
                return True, False
        sleep_note = ''
        if subtitle:
            sleep_interval = self.params.get('sleep_interval_subtitles') or 0
            min_sleep_interval = self.params.get('sleep_interval') or 0
            max_sleep_interval = self.params.get('max_sleep_interval') or 0
            requested_formats = info_dict.get('requested_formats') or [info_dict]
            if available_at := max(f.get('available_at') or 0 for f in requested_formats):
                forced_sleep_interval = available_at - int(time.time())
                if forced_sleep_interval > min_sleep_interval:
                    sleep_note = 'as required by the site'
                    min_sleep_interval = forced_sleep_interval
                if forced_sleep_interval > max_sleep_interval:
                    max_sleep_interval = forced_sleep_interval
            sleep_interval = random.uniform(
                min_sleep_interval, max_sleep_interval or min_sleep_interval)
        if sleep_interval > 0:
            self.to_screen(f'[download] Sleeping {sleep_interval:.2f} seconds {sleep_note}...')
            time.sleep(sleep_interval)
        ret = self.real_download(filename, info_dict)
        return ret, True
    def real_download(self, filename, info_dict):
        """Real download process. Redefine in subclasses."""
        raise NotImplementedError('This method must be implemented by subclasses')
    def _hook_progress(self, status, info_dict):
        # Ideally we want to make a copy of the dict, but that is too slow
        status['info_dict'] = info_dict
        # youtube-dl passes the same status object to all the hooks.
        # Some third party scripts seems to be relying on this.
        # So keep this behavior if possible
        for ph in self._progress_hooks:
            ph(status)
    def add_progress_hook(self, ph):
        # See YoutubeDl.py (search for progress_hooks) for a description of
        # this interface
        self._progress_hooks.append(ph)
    def _debug_cmd(self, args, exe=None):
        if not self.params.get('verbose', False):
        if exe is None:
            exe = os.path.basename(args[0])
        self.write_debug(f'{exe} command line: {shell_quote(args)}')
    def _get_impersonate_target(self, info_dict):
        impersonate = info_dict.get('impersonate')
        if impersonate is None:
        available_target, requested_targets = self.ydl._parse_impersonate_targets(impersonate)
        if available_target:
            return available_target
        elif requested_targets:
            self.report_warning(self.ydl._unavailable_targets_message(requested_targets))
import http.cookiejar
import http.cookies
import netrc
import urllib.request
from ..compat import (
    compat_expanduser,
    urllib_req_to_req,
from ..cookies import LenientSimpleCookie
from ..downloader.f4m import get_base_url, remove_encrypted_media
from ..downloader.hls import HlsFD
from ..globals import plugin_ies_overrides
from ..networking import HEADRequest, Request
    IncompleteRead,
    network_exceptions,
    JSON_LD_RE,
    GeoRestrictedError,
    ISO639Utils,
    LenientJSONDecoder,
    RegexNotFoundError,
    UnsupportedError,
    bug_reports_message,
    dict_get,
    encode_data_uri,
    filter_dict,
    netrc_from_content,
    parse_m3u8_attributes,
    qualities,
    truncate_string,
    try_get,
    urlhandle_detect_ext,
from ..utils._utils import _request_dump_filename
from ..utils.jslib import devalue
class InfoExtractor:
    """Information Extractor class.
    Information extractors are the classes that, given a URL, extract
    information about the video (or videos) the URL refers to. This
    information includes the real video URL, the video title, author and
    others. The information is stored in a dictionary which is then
    passed to the YoutubeDL. The YoutubeDL processes this
    information possibly downloading the video to the file system, among
    other possible outcomes.
    The type field determines the type of the result.
    By far the most common value (and the default if _type is missing) is
    "video", which indicates a single video.
    For a video, the dictionaries must include the following fields:
    id:             Video identifier.
    title:          Video title, unescaped. Set to an empty string if video has
                    no title as opposed to "None" which signifies that the
                    extractor failed to obtain a title
    Additionally, it must contain either a formats entry or a url one:
    formats:        A list of dictionaries for each format available, ordered
                    from worst to best quality.
                    Potential fields:
                    * url        The mandatory URL representing the media:
                                   for plain file media - HTTP URL of this file,
                                   for RTMP - RTMP URL,
                                   for HLS - URL of the M3U8 media playlist,
                                   for HDS - URL of the F4M manifest,
                                   for DASH
                                     - HTTP URL to plain file media (in case of
                                       unfragmented media)
                                     - URL of the MPD manifest or base URL
                                       representing the media if MPD manifest
                                       is parsed from a string (in case of
                                       fragmented media)
                                   for MSS - URL of the ISM manifest.
                    * request_data  Data to send in POST request to the URL
                    * manifest_url
                                 The URL of the manifest file in case of
                                 fragmented media:
                                   for HLS - URL of the M3U8 master playlist,
                                   for DASH - URL of the MPD manifest,
                    * manifest_stream_number  (For internal use only)
                                 The index of the stream in the manifest file
                    * ext        Will be calculated from URL if missing
                    * format     A human-readable description of the format
                                 ("mp4 container with h264/opus").
                                 Calculated from the format_id, width, height.
                                 and format_note fields if missing.
                    * format_id  A short description of the format
                                 ("mp4_h264_opus" or "19").
                                Technically optional, but strongly recommended.
                    * format_note Additional info about the format
                                 ("3D" or "DASH video")
                    * width      Width of the video, if known
                    * height     Height of the video, if known
                    * aspect_ratio  Aspect ratio of the video, if known
                                 Automatically calculated from width and height
                    * resolution Textual description of width and height
                    * dynamic_range The dynamic range of the video. One of:
                                 "SDR" (None), "HDR10", "HDR10+, "HDR12", "HLG, "DV"
                    * tbr        Average bitrate of audio and video in kbps (1000 bits/sec)
                    * abr        Average audio bitrate in kbps (1000 bits/sec)
                    * acodec     Name of the audio codec in use
                    * asr        Audio sampling rate in Hertz
                    * audio_channels  Number of audio channels
                    * vbr        Average video bitrate in kbps (1000 bits/sec)
                    * fps        Frame rate
                    * vcodec     Name of the video codec in use
                    * container  Name of the container format
                    * filesize   The number of bytes, if known in advance
                    * filesize_approx  An estimate for the number of bytes
                    * player_url SWF Player URL (used for rtmpdump).
                    * protocol   The protocol that will be used for the actual
                                 download, lower-case. One of "http", "https" or
                                 one of the protocols defined in downloader.PROTOCOL_MAP
                    * fragment_base_url
                                 Base URL for fragments. Each fragment's path
                                 value (if present) will be relative to
                                 this URL.
                    * fragments  A list of fragments of a fragmented media.
                                 Each fragment entry must contain either an url
                                 or a path. If an url is present it should be
                                 considered by a client. Otherwise both path and
                                 fragment_base_url must be present. Here is
                                 the list of all potential fields:
                                 * "url" - fragment's URL
                                 * "path" - fragment's path relative to
                                            fragment_base_url
                                 * "duration" (optional, int or float)
                                 * "filesize" (optional, int)
                    * hls_media_playlist_data
                                 The M3U8 media playlist data as a string.
                                 Only use if the data must be modified during extraction and
                                 the native HLS downloader should bypass requesting the URL.
                                 Does not apply if ffmpeg is used as external downloader
                    * is_from_start  Is a live format that can be downloaded
                                from the start. Boolean
                    * preference Order number of this format. If this field is
                                 present and not None, the formats get sorted
                                 by this field, regardless of all other values.
                                 -1 for default (order by other properties),
                                 -2 or smaller for less than default.
                                 < -1000 to hide the format (if there is
                                    another one which is strictly better)
                    * language   Language code, e.g. "de" or "en-US".
                    * language_preference  Is this in the language mentioned in
                                 the URL?
                                 10 if it's what the URL is about,
                                 -1 for default (don't know),
                                 -10 otherwise, other values reserved for now.
                    * quality    Order number of the video quality of this
                                 format, irrespective of the file format.
                    * source_preference  Order number for this video source
                                  (quality takes higher priority)
                    * http_headers  A dictionary of additional HTTP headers
                                 to add to the request.
                    * stretched_ratio  If given and not 1, indicates that the
                                 video's pixels are not square.
                                 width : height ratio as float.
                    * no_resume  The server does not support resuming the
                                 (HTTP or RTMP) download. Boolean.
                    * has_drm    True if the format has DRM and cannot be downloaded.
                                 'maybe' if the format may have DRM and has to be tested before download.
                    * extra_param_to_segment_url  A query string to append to each
                                 fragment's URL, or to update each existing query string
                                 with. If it is an HLS stream with an AES-128 decryption key,
                                 the query parameters will be passed to the key URI as well,
                                 unless there is an `extra_param_to_key_url` given,
                                 or unless an external key URI is provided via `hls_aes`.
                                 Only applied by the native HLS/DASH downloaders.
                    * extra_param_to_key_url  A query string to append to the URL
                                 of the format's HLS AES-128 decryption key.
                                 Only applied by the native HLS downloader.
                    * hls_aes    A dictionary of HLS AES-128 decryption information
                                 used by the native HLS downloader to override the
                                 values in the media playlist when an '#EXT-X-KEY' tag
                                 is present in the playlist:
                                 * uri  The URI from which the key will be downloaded
                                 * key  The key (as hex) used to decrypt fragments.
                                        If `key` is given, any key URI will be ignored
                                 * iv   The IV (as hex) used to decrypt fragments
                    * impersonate  Impersonate target(s). Can be any of the following entities:
                                * an instance of yt_dlp.networking.impersonate.ImpersonateTarget
                                * a string in the format of CLIENT[:OS]
                                * a list or a tuple of CLIENT[:OS] strings or ImpersonateTarget instances
                                * a boolean value; True means any impersonate target is sufficient
                    * available_at  Unix timestamp of when a format will be available to download
                    * downloader_options  A dictionary of downloader options
                                 (For internal use only)
                                 * http_chunk_size Chunk size for HTTP downloads
                                 * ffmpeg_args     Extra arguments for ffmpeg downloader (input)
                                 * ffmpeg_args_out Extra arguments for ffmpeg downloader (output)
                                 * ws              (NiconicoLiveFD only) WebSocketResponse
                                 * ws_url          (NiconicoLiveFD only) Websockets URL
                                 * max_quality     (NiconicoLiveFD only) Max stream quality string
                    * is_dash_periods  Whether the format is a result of merging
                                 multiple DASH periods.
                    RTMP formats can also have the additional fields: page_url,
                    app, play_path, tc_url, flash_version, rtmp_live, rtmp_conn,
                    rtmp_protocol, rtmp_real_time
    url:            Final video URL.
    ext:            Video filename extension.
    format:         The video format, defaults to ext (used for --get-format)
    player_url:     SWF Player URL (used for rtmpdump).
    The following fields are optional:
    direct:         True if a direct video file was given (must only be set by GenericIE)
    alt_title:      A secondary title of the video.
    display_id:     An alternative identifier for the video, not necessarily
                    unique, but available before title. Typically, id is
                    something like "4234987", title "Dancing naked mole rats",
                    and display_id "dancing-naked-mole-rats"
    thumbnails:     A list of dictionaries, with the following entries:
                        * "id" (optional, string) - Thumbnail format ID
                        * "url"
                        * "ext" (optional, string) - actual image extension if not given in URL
                        * "preference" (optional, int) - quality of the image
                        * "width" (optional, int)
                        * "height" (optional, int)
                        * "resolution" (optional, string "{width}x{height}",
                                        deprecated)
                        * "http_headers" (dict) - HTTP headers for the request
    thumbnail:      Full URL to a video thumbnail image.
    description:    Full video description.
    uploader:       Full name of the video uploader.
    license:        License name the video is licensed under.
    creators:       List of creators of the video.
    timestamp:      UNIX timestamp of the moment the video was uploaded
    upload_date:    Video upload date in UTC (YYYYMMDD).
                    If not explicitly set, calculated from timestamp
    release_timestamp: UNIX timestamp of the moment the video was released.
                    If it is not clear whether to use timestamp or this, use the former
    release_date:   The date (YYYYMMDD) when the video was released in UTC.
                    If not explicitly set, calculated from release_timestamp
    release_year:   Year (YYYY) as integer when the video or album was released.
                    To be used if no exact release date is known.
                    If not explicitly set, calculated from release_date.
    modified_timestamp: UNIX timestamp of the moment the video was last modified.
    modified_date:   The date (YYYYMMDD) when the video was last modified in UTC.
                    If not explicitly set, calculated from modified_timestamp
    uploader_id:    Nickname or id of the video uploader.
    uploader_url:   Full URL to a personal webpage of the video uploader.
    channel:        Full name of the channel the video is uploaded on.
                    Note that channel fields may or may not repeat uploader
                    fields. This depends on a particular extractor.
    channel_id:     Id of the channel.
    channel_url:    Full URL to a channel webpage.
    channel_follower_count: Number of followers of the channel.
    channel_is_verified: Whether the channel is verified on the platform.
    location:       Physical location where the video was filmed.
    subtitles:      The available subtitles as a dictionary in the format
                    {tag: subformats}. "tag" is usually a language code, and
                    "subformats" is a list sorted from lower to higher
                    preference, each element is a dictionary with the "ext"
                    entry and one of:
                        * "data": The subtitles file contents
                        * "url": A URL pointing to the subtitles file
                    It can optionally also have:
                        * "name": Name or description of the subtitles
                        * "http_headers": A dictionary of additional HTTP headers
                        * "impersonate": Impersonate target(s); same as the "formats" field
                    "ext" will be calculated from URL if missing
    automatic_captions: Like 'subtitles'; contains automatically generated
                    captions instead of normal subtitles
    duration:       Length of the video in seconds, as an integer or float.
    view_count:     How many users have watched the video on the platform.
    concurrent_view_count: How many users are currently watching the video on the platform.
    save_count:     Number of times the video has been saved or bookmarked
    like_count:     Number of positive ratings of the video
    dislike_count:  Number of negative ratings of the video
    repost_count:   Number of reposts of the video
    average_rating: Average rating given by users, the scale used depends on the webpage
    comment_count:  Number of comments on the video
    comments:       A list of comments, each with one or more of the following
                    properties (all but one of text or html optional):
                        * "author" - human-readable name of the comment author
                        * "author_id" - user ID of the comment author
                        * "author_thumbnail" - The thumbnail of the comment author
                        * "author_url" - The url to the comment author's page
                        * "author_is_verified" - Whether the author is verified
                                                 on the platform
                        * "author_is_uploader" - Whether the comment is made by
                                                 the video uploader
                        * "id" - Comment ID
                        * "html" - Comment as HTML
                        * "text" - Plain text of the comment
                        * "timestamp" - UNIX timestamp of comment
                        * "parent" - ID of the comment this one is replying to.
                                     Set to "root" to indicate that this is a
                                     comment to the original video.
                        * "like_count" - Number of positive ratings of the comment
                        * "dislike_count" - Number of negative ratings of the comment
                        * "is_favorited" - Whether the comment is marked as
                                           favorite by the video uploader
                        * "is_pinned" - Whether the comment is pinned to
                                        the top of the comments
    age_limit:      Age restriction for the video, as an integer (years)
    webpage_url:    The URL to the video webpage, if given to yt-dlp it
                    should allow to get the same result again. (It will be set
                    by YoutubeDL if it's missing)
    categories:     A list of categories that the video falls in, for example
                    ["Sports", "Berlin"]
    tags:           A list of tags assigned to the video, e.g. ["sweden", "pop music"]
    cast:           A list of the video cast
    is_live:        True, False, or None (=unknown). Whether this video is a
                    live stream that goes on instead of a fixed-length video.
    was_live:       True, False, or None (=unknown). Whether this video was
                    originally a live stream.
    live_status:    None (=unknown), 'is_live', 'is_upcoming', 'was_live', 'not_live',
                    or 'post_live' (was live, but VOD is not yet processed)
                    If absent, automatically set from is_live, was_live
    start_time:     Time in seconds where the reproduction should start, as
                    specified in the URL.
    end_time:       Time in seconds where the reproduction should end, as
    chapters:       A list of dictionaries, with the following entries:
                        * "start_time" - The start time of the chapter in seconds
                        * "end_time" - The end time of the chapter in seconds
                                       (optional: core code can determine this value from
                                       the next chapter's start_time or the video's duration)
                        * "title" (optional, string)
    heatmap:        A list of dictionaries, with the following entries:
                        * "start_time" - The start time of the data point in seconds
                        * "end_time" - The end time of the data point in seconds
                        * "value" - The normalized value of the data point (float between 0 and 1)
    playable_in_embed: Whether this video is allowed to play in embedded
                    players on other sites. Can be True (=always allowed),
                    False (=never allowed), None (=unknown), or a string
                    specifying the criteria for embedability; e.g. 'whitelist'
    availability:   Under what condition the video is available. One of
                    'private', 'premium_only', 'subscriber_only', 'needs_auth',
                    'unlisted' or 'public'. Use 'InfoExtractor._availability'
                    to set it
    media_type:     The type of media as classified by the site, e.g. "episode", "clip", "trailer"
    _old_archive_ids: A list of old archive ids needed for backward
                   compatibility. Use yt_dlp.utils.make_archive_id to generate ids
    _format_sort_fields: A list of fields to use for sorting formats
    __post_extractor: A function to be called just before the metadata is
                    written to either disk, logger or console. The function
                    must return a dict which will be added to the info_dict.
                    This is useful for additional information that is
                    time-consuming to extract. Note that the fields thus
                    extracted will not be available to output template and
                    match_filter. So, only "comments" and "comment_count" are
                    currently allowed to be extracted via this method.
    The following fields should only be used when the video belongs to some logical
    chapter or section:
    chapter:        Name or title of the chapter the video belongs to.
    chapter_number: Number of the chapter the video belongs to, as an integer.
    chapter_id:     Id of the chapter the video belongs to, as a unicode string.
    The following fields should only be used when the video is an episode of some
    series, programme or podcast:
    series:         Title of the series or programme the video episode belongs to.
    series_id:      Id of the series or programme the video episode belongs to, as a unicode string.
    season:         Title of the season the video episode belongs to.
    season_number:  Number of the season the video episode belongs to, as an integer.
    season_id:      Id of the season the video episode belongs to, as a unicode string.
    episode:        Title of the video episode. Unlike mandatory video title field,
                    this field should denote the exact title of the video episode
                    without any kind of decoration.
    episode_number: Number of the video episode within a season, as an integer.
    episode_id:     Id of the video episode, as a unicode string.
    The following fields should only be used when the media is a track or a part of
    a music album:
    track:          Title of the track.
    track_number:   Number of the track within an album or a disc, as an integer.
    track_id:       Id of the track (useful in case of custom indexing, e.g. 6.iii),
                    as a unicode string.
    artists:        List of artists of the track.
    composers:      List of composers of the piece.
    genres:         List of genres of the track.
    album:          Title of the album the track belongs to.
    album_type:     Type of the album (e.g. "Demo", "Full-length", "Split", "Compilation", etc).
    album_artists:  List of all artists appeared on the album.
                    E.g. ["Ash Borer", "Fell Voices"] or ["Various Artists"].
                    Useful for splits and compilations.
    disc_number:    Number of the disc or other physical medium the track belongs to,
                    as an integer.
    The following fields should only be set for clips that should be cut from the original video:
    section_start:  Start time of the section in seconds
    section_end:    End time of the section in seconds
    The following fields should only be set for storyboards:
    rows:           Number of rows in each storyboard fragment, as an integer
    columns:        Number of columns in each storyboard fragment, as an integer
    The following fields are deprecated and should not be set by new code:
    composer:       Use "composers" instead.
                    Composer(s) of the piece, comma-separated.
    artist:         Use "artists" instead.
                    Artist(s) of the track, comma-separated.
    genre:          Use "genres" instead.
                    Genre(s) of the track, comma-separated.
    album_artist:   Use "album_artists" instead.
                    All artists appeared on the album, comma-separated.
    creator:        Use "creators" instead.
                    The creator of the video.
    Unless mentioned otherwise, the fields should be Unicode strings.
    Unless mentioned otherwise, None is equivalent to absence of information.
    _type "playlist" indicates multiple videos.
    There must be a key "entries", which is a list, an iterable, or a PagedList
    object, each element of which is a valid dictionary by this specification.
    Additionally, playlists can have "id", "title", and any other relevant
    attributes with the same semantics as videos (see above).
    It can also have the following optional fields:
    playlist_count: The total number of videos in a playlist. If not given,
                    YoutubeDL tries to calculate it from "entries"
    _type "multi_video" indicates that there are multiple videos that
    form a single show, for examples multiple acts of an opera or TV episode.
    It must have an entries key like a playlist and contain all the keys
    required for a video at the same time.
    _type "url" indicates that the video must be extracted from another
    location, possibly by a different extractor. Its only required key is:
    "url" - the next URL to extract.
    The key "ie_key" can be set to the class name (minus the trailing "IE",
    e.g. "Youtube") if the extractor class is known in advance.
    Additionally, the dictionary may have any properties of the resolved entity
    known in advance, for example "title" if the title of the referred video is
    known ahead of time.
    _type "url_transparent" entities have the same specification as "url", but
    indicate that the given additional information is more precise than the one
    associated with the resolved URL.
    This is useful when a site employs a video service that hosts the video and
    its technical metadata, but that video service does not embed a useful
    title, description etc.
    Subclasses of this should also be added to the list of extractors and
    should define _VALID_URL as a regexp or a Sequence of regexps, and
    re-define the _real_extract() and (optionally) _real_initialize() methods.
    Subclasses may also override suitable() if necessary, but ensure the function
    signature is preserved and that this function imports everything it needs
    (except other extractors), so that lazy_extractors works correctly.
    Subclasses can define a list of _EMBED_REGEX, which will be searched for in
    the HTML of Generic webpages. It may also override _extract_embed_urls
    or _extract_from_webpage as necessary. While these are normally classmethods,
    _extract_from_webpage is allowed to be an instance method.
    _extract_from_webpage may raise self.StopExtraction to stop further
    processing of the webpage and obtain exclusive rights to it. This is useful
    when the extractor cannot reliably be matched using just the URL,
    e.g. invidious/peertube instances
    Embed-only extractors can be defined by setting _VALID_URL = False.
    To support username + password (or netrc) login, the extractor must define a
    _NETRC_MACHINE and re-define _perform_login(username, password) and
    (optionally) _initialize_pre_login() methods. The _perform_login method will
    be called between _initialize_pre_login and _real_initialize if credentials
    are passed by the user. In cases where it is necessary to have the login
    process as part of the extraction rather than initialization, _perform_login
    can be left undefined.
    _GEO_BYPASS attribute may be set to False in order to disable
    geo restriction bypass mechanisms for a particular extractor.
    Though it won't disable explicit geo restriction bypass based on
    country code provided with geo_bypass_country.
    _GEO_COUNTRIES attribute may contain a list of presumably geo unrestricted
    countries for this extractor. One of these countries will be used by
    geo restriction bypass mechanism right away in order to bypass
    geo restriction, of course, if the mechanism is not disabled.
    _GEO_IP_BLOCKS attribute may contain a list of presumably geo unrestricted
    IP blocks in CIDR notation for this extractor. One of these IP blocks
    will be used by geo restriction bypass mechanism similarly
    to _GEO_COUNTRIES.
    The _ENABLED attribute should be set to False for IEs that
    are disabled by default and must be explicitly enabled.
    The _WORKING attribute should be set to False for broken IEs
    in order to warn the users and skip the tests.
    _ready = False
    _downloader = None
    _x_forwarded_for_ip = None
    _GEO_BYPASS = True
    _GEO_COUNTRIES = None
    _GEO_IP_BLOCKS = None
    _WORKING = True
    _ENABLED = True
    _NETRC_MACHINE = None
    IE_DESC = None
    SEARCH_KEY = None
    _VALID_URL = None
    _EMBED_REGEX = []
    def _login_hint(self, method=NO_DEFAULT, netrc=None):
        password_hint = f'--username and --password, --netrc-cmd, or --netrc ({netrc or self._NETRC_MACHINE}) to provide account credentials'
        cookies_hint = 'See  https://github.com/yt-dlp/yt-dlp/wiki/FAQ#how-do-i-pass-cookies-to-yt-dlp  for how to manually pass cookies'
            None: '',
            'any': f'Use --cookies, --cookies-from-browser, {password_hint}. {cookies_hint}',
            'password': f'Use {password_hint}',
            'cookies': f'Use --cookies-from-browser or --cookies for the authentication. {cookies_hint}',
            'session_cookies': f'Use --cookies for the authentication (--cookies-from-browser might not work). {cookies_hint}',
        }[method if method is not NO_DEFAULT else 'any' if self.supports_login() else 'cookies']
    def __init__(self, downloader=None):
        """Constructor. Receives an optional downloader (a YoutubeDL instance).
        If a downloader is not passed during initialization,
        it must be set using "set_downloader()" before "extract()" is called"""
        self._ready = False
        self._x_forwarded_for_ip = None
        self._printed_messages = set()
        self.set_downloader(downloader)
    def _match_valid_url(cls, url):
        if cls._VALID_URL is False:
        # This does not use has/getattr intentionally - we want to know whether
        # we have cached the regexp for *this* class, whereas getattr would also
        # match the superclass
        if '_VALID_URL_RE' not in cls.__dict__:
            cls._VALID_URL_RE = tuple(map(re.compile, variadic(cls._VALID_URL)))
        return next(filter(None, (regex.match(url) for regex in cls._VALID_URL_RE)), None)
    def suitable(cls, url):
        """Receives a URL and returns True if suitable for this IE."""
        # This function must import everything it needs (except other extractors),
        # so that lazy_extractors works correctly
        return cls._match_valid_url(url) is not None
    def _match_id(cls, url):
        return cls._match_valid_url(url).group('id')
    def get_temp_id(cls, url):
            return cls._match_id(url)
        except (IndexError, AttributeError):
    def working(cls):
        """Getter method for _WORKING."""
        return cls._WORKING
    def supports_login(cls):
        return bool(cls._NETRC_MACHINE)
    def initialize(self):
        """Initializes an instance (authentication, etc)."""
        self._initialize_geo_bypass({
            'countries': self._GEO_COUNTRIES,
            'ip_blocks': self._GEO_IP_BLOCKS,
        if not self._ready:
            self._initialize_pre_login()
            if self.supports_login():
                # try login only if it would actually do anything
                if type(self)._perform_login is not InfoExtractor._perform_login:
                    username, password = self._get_login_info()
                    if username:
                        self._perform_login(username, password)
            elif self.get_param('username') and False not in (self.IE_DESC, self._NETRC_MACHINE):
                self.report_warning(f'Login with password is not supported for this website. {self._login_hint("cookies")}')
            self._real_initialize()
            self._ready = True
    def _initialize_geo_bypass(self, geo_bypass_context):
        Initialize geo restriction bypass mechanism.
        This method is used to initialize geo bypass mechanism based on faking
        X-Forwarded-For HTTP header. A random country from provided country list
        is selected and a random IP belonging to this country is generated. This
        IP will be passed as X-Forwarded-For HTTP header in all subsequent
        HTTP requests.
        This method will be used for initial geo bypass mechanism initialization
        during the instance initialization with _GEO_COUNTRIES and
        _GEO_IP_BLOCKS.
        You may also manually call it from extractor's code if geo bypass
        information is not available beforehand (e.g. obtained during
        extraction) or due to some other reason. In this case you should pass
        this information in geo bypass context passed as first argument. It may
        contain following fields:
        countries:  List of geo unrestricted countries (similar
                    to _GEO_COUNTRIES)
        ip_blocks:  List of geo unrestricted IP blocks in CIDR notation
                    (similar to _GEO_IP_BLOCKS)
        if not self._x_forwarded_for_ip:
            # Geo bypass mechanism is explicitly disabled by user
            if not self.get_param('geo_bypass', True):
            if not geo_bypass_context:
                geo_bypass_context = {}
            # Backward compatibility: previously _initialize_geo_bypass
            # expected a list of countries, some 3rd party code may still use
            # it this way
            if isinstance(geo_bypass_context, (list, tuple)):
                geo_bypass_context = {
                    'countries': geo_bypass_context,
            # The whole point of geo bypass mechanism is to fake IP
            # as X-Forwarded-For HTTP header based on some IP block or
            # country code.
            # Path 1: bypassing based on IP block in CIDR notation
            # Explicit IP block specified by user, use it right away
            # regardless of whether extractor is geo bypassable or not
            ip_block = self.get_param('geo_bypass_ip_block', None)
            # Otherwise use random IP block from geo bypass context but only
            # if extractor is known as geo bypassable
            if not ip_block:
                ip_blocks = geo_bypass_context.get('ip_blocks')
                if self._GEO_BYPASS and ip_blocks:
                    ip_block = random.choice(ip_blocks)
            if ip_block:
                self._x_forwarded_for_ip = GeoUtils.random_ipv4(ip_block)
                self.write_debug(f'Using fake IP {self._x_forwarded_for_ip} as X-Forwarded-For')
            # Path 2: bypassing based on country code
            # Explicit country code specified by user, use it right away
            country = self.get_param('geo_bypass_country', None)
            # Otherwise use random country code from geo bypass context but
            # only if extractor is known as geo bypassable
            if not country:
                countries = geo_bypass_context.get('countries')
                if self._GEO_BYPASS and countries:
                    country = random.choice(countries)
            if country:
                self._x_forwarded_for_ip = GeoUtils.random_ipv4(country)
                self._downloader.write_debug(
                    f'Using fake IP {self._x_forwarded_for_ip} ({country.upper()}) as X-Forwarded-For')
    def extract(self, url):
        """Extracts URL information and returns it in list of dicts."""
            for _ in range(2):
                    self.initialize()
                    self.to_screen('Extracting URL: %s' % (
                        url if self.get_param('verbose') else truncate_string(url, 100, 20)))
                    ie_result = self._real_extract(url)
                    if ie_result is None:
                    if self._x_forwarded_for_ip:
                        ie_result['__x_forwarded_for_ip'] = self._x_forwarded_for_ip
                    subtitles = ie_result.get('subtitles') or {}
                    if 'no-live-chat' in self.get_param('compat_opts'):
                        for lang in ('live_chat', 'comments', 'danmaku'):
                            subtitles.pop(lang, None)
                    return ie_result
                except GeoRestrictedError as e:
                    if self.__maybe_fake_ip_and_retry(e.countries):
        except UnsupportedError:
            e.video_id = e.video_id or self.get_temp_id(url)
            e.ie = e.ie or self.IE_NAME
            e.traceback = e.traceback or sys.exc_info()[2]
        except IncompleteRead as e:
            raise ExtractorError('A network error has occurred.', cause=e, expected=True, video_id=self.get_temp_id(url))
        except (KeyError, StopIteration) as e:
            raise ExtractorError('An extractor error has occurred.', cause=e, video_id=self.get_temp_id(url))
    def __maybe_fake_ip_and_retry(self, countries):
        if (not self.get_param('geo_bypass_country', None)
                and self._GEO_BYPASS
                and self.get_param('geo_bypass', True)
                and not self._x_forwarded_for_ip
                and countries):
            country_code = random.choice(countries)
            self._x_forwarded_for_ip = GeoUtils.random_ipv4(country_code)
                self.report_warning(
                    'Video is geo restricted. Retrying extraction with fake IP '
                    f'{self._x_forwarded_for_ip} ({country_code.upper()}) as X-Forwarded-For.')
    def set_downloader(self, downloader):
        """Sets a YoutubeDL instance as the downloader for this IE."""
        self._downloader = downloader
    def cache(self):
        return self._downloader.cache
    def cookiejar(self):
        return self._downloader.cookiejar
    def _initialize_pre_login(self):
        """ Initialization before login. Redefine in subclasses."""
    def _perform_login(self, username, password):
        """ Login with username and password. Redefine in subclasses."""
    def _real_initialize(self):
        """Real initialization process. Redefine in subclasses."""
        """Real extraction process. Redefine in subclasses."""
    def ie_key(cls):
        """A string for getting the InfoExtractor with get_info_extractor"""
        return cls.__name__[:-2]
    def IE_NAME(cls):
    def __can_accept_status_code(err, expected_status):
        assert isinstance(err, HTTPError)
        if expected_status is None:
        elif callable(expected_status):
            return expected_status(err.status) is True
            return err.status in variadic(expected_status)
    def _create_request(self, url_or_request, data=None, headers=None, query=None, extensions=None):
        if isinstance(url_or_request, urllib.request.Request):
            self._downloader.deprecation_warning(
                'Passing a urllib.request.Request to _create_request() is deprecated. '
                'Use yt_dlp.networking.common.Request instead.')
            url_or_request = urllib_req_to_req(url_or_request)
        elif not isinstance(url_or_request, Request):
            url_or_request = Request(url_or_request)
        url_or_request.update(data=data, headers=headers, query=query, extensions=extensions)
        return url_or_request
    def _request_webpage(self, url_or_request, video_id, note=None, errnote=None, fatal=True, data=None,
                         headers=None, query=None, expected_status=None, impersonate=None, require_impersonation=False):
        Return the response handle.
        See _download_webpage docstring for arguments specification.
        if not self._downloader._first_webpage_request:
            sleep_interval = self.get_param('sleep_interval_requests') or 0
                self.to_screen(f'Sleeping {sleep_interval} seconds ...')
            self._downloader._first_webpage_request = False
        if note is None:
            self.report_download_webpage(video_id)
        elif note is not False:
            if video_id is None:
                self.to_screen(str(note))
                self.to_screen(f'{video_id}: {note}')
        # Some sites check X-Forwarded-For HTTP header in order to figure out
        # the origin of the client behind proxy. This allows bypassing geo
        # restriction by faking this header's value to IP that belongs to some
        # geo unrestricted country. We will do so once we encounter any
        # geo restriction error.
            headers = (headers or {}).copy()
            headers.setdefault('X-Forwarded-For', self._x_forwarded_for_ip)
        extensions = {}
        available_target, requested_targets = self._downloader._parse_impersonate_targets(impersonate)
            extensions['impersonate'] = available_target
            msg = 'The extractor is attempting impersonation'
            if require_impersonation:
                raise ExtractorError(
                    self._downloader._unavailable_targets_message(requested_targets, note=msg, is_error=True),
                    expected=True)
                self._downloader._unavailable_targets_message(requested_targets, note=msg), only_once=True)
            return self._downloader.urlopen(self._create_request(url_or_request, data, headers, query, extensions))
        except network_exceptions as err:
            if isinstance(err, HTTPError):
                if self.__can_accept_status_code(err, expected_status):
                    return err.response
            if errnote is False:
            if errnote is None:
                errnote = 'Unable to download webpage'
            errmsg = f'{errnote}: {err}'
            if fatal:
                raise ExtractorError(errmsg, cause=err)
                self.report_warning(errmsg)
    def _download_webpage_handle(self, url_or_request, video_id, note=None, errnote=None, fatal=True,
                                 encoding=None, data=None, headers={}, query={}, expected_status=None,
                                 impersonate=None, require_impersonation=False):
        Return a tuple (page content as string, URL handle).
        url_or_request -- plain text URL as a string or
            a yt_dlp.networking.Request object
        video_id -- Video/playlist/item identifier (string)
        Keyword arguments:
        note -- note printed before downloading (string)
        errnote -- note printed in case of an error (string)
        fatal -- flag denoting whether error should be considered fatal,
            i.e. whether it should cause ExtractionError to be raised,
            otherwise a warning will be reported and extraction continued
        encoding -- encoding for a page content decoding, guessed automatically
            when not explicitly specified
        data -- POST data (bytes)
        headers -- HTTP headers (dict)
        query -- URL query (dict)
        expected_status -- allows to accept failed HTTP requests (non 2xx
            status code) by explicitly specifying a set of accepted status
            codes. Can be any of the following entities:
                - an integer type specifying an exact failed status code to
                  accept
                - a list or a tuple of integer types specifying a list of
                  failed status codes to accept
                - a callable accepting an actual failed status code and
                  returning True if it should be accepted
            Note that this argument does not affect success status codes (2xx)
            which are always accepted.
        impersonate -- the impersonate target. Can be any of the following entities:
                - an instance of yt_dlp.networking.impersonate.ImpersonateTarget
                - a string in the format of CLIENT[:OS]
                - a list or a tuple of CLIENT[:OS] strings or ImpersonateTarget instances
                - a boolean value; True means any impersonate target is sufficient
        require_impersonation -- flag to toggle whether the request should raise an error
            if impersonation is not possible (bool, default: False)
        # Strip hashes from the URL (#1038)
        if isinstance(url_or_request, str):
            url_or_request = url_or_request.partition('#')[0]
        urlh = self._request_webpage(url_or_request, video_id, note, errnote, fatal, data=data,
                                     headers=headers, query=query, expected_status=expected_status,
                                     impersonate=impersonate, require_impersonation=require_impersonation)
        if urlh is False:
            assert not fatal
        content = self._webpage_read_content(urlh, url_or_request, video_id, note, errnote, fatal,
                                             encoding=encoding, data=data)
        if content is False:
        return (content, urlh)
    def _guess_encoding_from_content(content_type, webpage_bytes):
        m = re.match(r'[a-zA-Z0-9_.-]+/[a-zA-Z0-9_.-]+\s*;\s*charset=(.+)', content_type)
            encoding = m.group(1)
            m = re.search(br'<meta[^>]+charset=[\'"]?([^\'")]+)[ /\'">]',
                          webpage_bytes[:1024])
                encoding = m.group(1).decode('ascii')
            elif webpage_bytes.startswith(b'\xff\xfe'):
                encoding = 'utf-16'
                encoding = 'utf-8'
        return encoding
    def __check_blocked(self, content):
        first_block = content[:512]
        if ('<title>Access to this site is blocked</title>' in content
                and 'Websense' in first_block):
            msg = 'Access to this webpage has been blocked by Websense filtering software in your network.'
            blocked_iframe = self._html_search_regex(
                r'<iframe src="([^"]+)"', content,
                'Websense information URL', default=None)
            if blocked_iframe:
                msg += f' Visit {blocked_iframe} for more details'
            raise ExtractorError(msg, expected=True)
        if '<title>The URL you requested has been blocked</title>' in first_block:
            msg = (
                'Access to this webpage has been blocked by Indian censorship. '
                'Use a VPN or proxy server (with --proxy) to route around it.')
            block_msg = self._html_search_regex(
                r'</h1><p>(.*?)</p>',
                content, 'block message', default=None)
            if block_msg:
                msg += ' (Message: "{}")'.format(block_msg.replace('\n', ' '))
        if ('<title>TTK :: Доступ к ресурсу ограничен</title>' in content
                and 'blocklist.rkn.gov.ru' in content):
                'Access to this webpage has been blocked by decision of the Russian government. '
                'Visit http://blocklist.rkn.gov.ru/ for a block reason.',
    def __decode_webpage(self, webpage_bytes, encoding, headers):
        if not encoding:
            encoding = self._guess_encoding_from_content(headers.get('Content-Type', ''), webpage_bytes)
            return webpage_bytes.decode(encoding, 'replace')
        except LookupError:
            return webpage_bytes.decode('utf-8', 'replace')
    def _webpage_read_content(self, urlh, url_or_request, video_id, note=None, errnote=None, fatal=True,
                              prefix=None, encoding=None, data=None):
            webpage_bytes = urlh.read()
            errmsg = f'{video_id}: Error reading response: {err.msg}'
        if prefix is not None:
            webpage_bytes = prefix + webpage_bytes
        if self.get_param('dump_intermediate_pages', False):
            self.to_screen('Dumping request to ' + urlh.url)
            dump = base64.b64encode(webpage_bytes).decode('ascii')
            self._downloader.to_screen(dump)
        if self.get_param('write_pages'):
            if isinstance(url_or_request, Request):
                data = self._create_request(url_or_request, data).data
            filename = _request_dump_filename(
                urlh.url, video_id, data,
                trim_length=self.get_param('trim_file_name'))
            self.to_screen(f'Saving request to {filename}')
            with open(filename, 'wb') as outf:
                outf.write(webpage_bytes)
        content = self.__decode_webpage(webpage_bytes, encoding, urlh.headers)
        self.__check_blocked(content)
    def __print_error(self, errnote, fatal, video_id, err):
            raise ExtractorError(f'{video_id}: {errnote}', cause=err)
        elif errnote:
            self.report_warning(f'{video_id}: {errnote}: {err}')
    def _parse_xml(self, xml_string, video_id, transform_source=None, fatal=True, errnote=None):
        if transform_source:
            xml_string = transform_source(xml_string)
            return compat_etree_fromstring(xml_string.encode())
        except xml.etree.ElementTree.ParseError as ve:
            self.__print_error('Failed to parse XML' if errnote is None else errnote, fatal, video_id, ve)
    def _parse_json(self, json_string, video_id, transform_source=None, fatal=True, errnote=None, **parser_kwargs):
            return json.loads(
                json_string, cls=LenientJSONDecoder, strict=False, transform_source=transform_source, **parser_kwargs)
            self.__print_error('Failed to parse JSON' if errnote is None else errnote, fatal, video_id, ve)
    def _parse_socket_response_as_json(self, data, *args, **kwargs):
        return self._parse_json(data[data.find('{'):data.rfind('}') + 1], *args, **kwargs)
    def __create_download_methods(name, parser, note, errnote, return_value):
        def parse(ie, content, *args, errnote=errnote, **kwargs):
                kwargs['errnote'] = errnote
            # parser is fetched by name so subclasses can override it
            return getattr(ie, parser)(content, *args, **kwargs)
        def download_handle(self, url_or_request, video_id, note=note, errnote=errnote, transform_source=None,
                            fatal=True, encoding=None, data=None, headers={}, query={}, expected_status=None,
            res = self._download_webpage_handle(
                url_or_request, video_id, note=note, errnote=errnote, fatal=fatal, encoding=encoding,
                data=data, headers=headers, query=query, expected_status=expected_status,
            if res is False:
            content, urlh = res
            return parse(self, content, video_id, transform_source=transform_source, fatal=fatal, errnote=errnote), urlh
        def download_content(self, url_or_request, video_id, note=note, errnote=errnote, transform_source=None,
            if self.get_param('load_pages'):
                url_or_request = self._create_request(url_or_request, data, headers, query)
                    url_or_request.url, video_id, url_or_request.data,
                self.to_screen(f'Loading request from {filename}')
                    with open(filename, 'rb') as dumpf:
                        webpage_bytes = dumpf.read()
                    self.report_warning(f'Unable to load request from disk: {e}')
                    content = self.__decode_webpage(webpage_bytes, encoding, url_or_request.headers)
                    return parse(self, content, video_id, transform_source=transform_source, fatal=fatal, errnote=errnote)
            kwargs = {
                'note': note,
                'errnote': errnote,
                'transform_source': transform_source,
                'fatal': fatal,
                'encoding': encoding,
                'data': data,
                'headers': headers,
                'query': query,
                'expected_status': expected_status,
                'impersonate': impersonate,
                'require_impersonation': require_impersonation,
                kwargs.pop('transform_source')
            # The method is fetched by name so subclasses can override _download_..._handle
            res = getattr(self, download_handle.__name__)(url_or_request, video_id, **kwargs)
            return res if res is False else res[0]
        def impersonate(func, name, return_value):
            func.__name__, func.__qualname__ = name, f'InfoExtractor.{name}'
            func.__doc__ = f'''
                @param transform_source     Apply this transformation before parsing
                @returns                    {return_value}
                See _download_webpage_handle docstring for other arguments specification
        impersonate(download_handle, f'_download_{name}_handle', f'({return_value}, URL handle)')
        impersonate(download_content, f'_download_{name}', f'{return_value}')
        return download_handle, download_content
    _download_xml_handle, _download_xml = __create_download_methods(
        'xml', '_parse_xml', 'Downloading XML', 'Unable to download XML', 'xml as an xml.etree.ElementTree.Element')
    _download_json_handle, _download_json = __create_download_methods(
        'json', '_parse_json', 'Downloading JSON metadata', 'Unable to download JSON metadata', 'JSON object as a dict')
    _download_socket_json_handle, _download_socket_json = __create_download_methods(
        'socket_json', '_parse_socket_response_as_json', 'Polling socket', 'Unable to poll socket', 'JSON object as a dict')
    __download_webpage = __create_download_methods('webpage', None, None, None, 'data of the page as a string')[1]
    def _download_webpage(
            self, url_or_request, video_id, note=None, errnote=None,
            fatal=True, tries=1, timeout=NO_DEFAULT, *args, **kwargs):
        Return the data of the page as a string.
        tries -- number of tries
        timeout -- sleep interval between tries
        See _download_webpage_handle docstring for other arguments specification.
        R''' # NB: These are unused; should they be deprecated?
        if tries != 1:
            self._downloader.deprecation_warning('tries argument is deprecated in InfoExtractor._download_webpage')
        if timeout is NO_DEFAULT:
            timeout = 5
            self._downloader.deprecation_warning('timeout argument is deprecated in InfoExtractor._download_webpage')
        try_count = 0
                return self.__download_webpage(url_or_request, video_id, note, errnote, None, fatal, *args, **kwargs)
                try_count += 1
                if try_count >= tries:
                self._sleep(timeout, video_id)
    def report_warning(self, msg, video_id=None, *args, only_once=False, **kwargs):
        idstr = format_field(video_id, None, '%s: ')
        msg = f'[{self.IE_NAME}] {idstr}{msg}'
        if only_once:
            if f'WARNING: {msg}' in self._printed_messages:
            self._printed_messages.add(f'WARNING: {msg}')
        self._downloader.report_warning(msg, *args, **kwargs)
    def to_screen(self, msg, *args, **kwargs):
        """Print msg to screen, prefixing it with '[ie_name]'"""
        self._downloader.to_screen(f'[{self.IE_NAME}] {msg}', *args, **kwargs)
    def write_debug(self, msg, *args, **kwargs):
        self._downloader.write_debug(f'[{self.IE_NAME}] {msg}', *args, **kwargs)
    def get_param(self, name, default=None, *args, **kwargs):
        if self._downloader:
            return self._downloader.params.get(name, default, *args, **kwargs)
    def report_drm(self, video_id, partial=NO_DEFAULT):
        if partial is not NO_DEFAULT:
            self._downloader.deprecation_warning('InfoExtractor.report_drm no longer accepts the argument partial')
        self.raise_no_formats('This video is DRM protected', expected=True, video_id=video_id)
    def report_extraction(self, id_or_name):
        """Report information extraction."""
        self.to_screen(f'{id_or_name}: Extracting information')
    def report_download_webpage(self, video_id):
        """Report webpage download."""
        self.to_screen(f'{video_id}: Downloading webpage')
    def report_age_confirmation(self):
        """Report attempt to confirm age."""
        self.to_screen('Confirming age')
    def report_login(self):
        """Report attempt to log in."""
        self.to_screen('Logging in')
    def raise_login_required(
            self, msg='This video is only available for registered users',
            metadata_available=False, method=NO_DEFAULT):
        if metadata_available and (
                self.get_param('ignore_no_formats_error') or self.get_param('wait_for_video')):
            self.report_warning(msg)
        msg += format_field(self._login_hint(method), None, '. %s')
    def raise_geo_restricted(
            self, msg='This video is not available from your location due to geo restriction',
            countries=None, metadata_available=False):
            raise GeoRestrictedError(msg, countries=countries)
    def raise_no_formats(self, msg, expected=False, video_id=None):
        if expected and (
            self.report_warning(msg, video_id)
        elif isinstance(msg, ExtractorError):
            raise msg
            raise ExtractorError(msg, expected=expected, video_id=video_id)
    # Methods for following #608
    def url_result(url, ie=None, video_id=None, video_title=None, *, url_transparent=False, **kwargs):
        """Returns a URL that points to a page that should be processed"""
        if ie is not None:
            kwargs['ie_key'] = ie if isinstance(ie, str) else ie.ie_key()
        if video_id is not None:
            kwargs['id'] = video_id
        if video_title is not None:
            kwargs['title'] = video_title
            '_type': 'url_transparent' if url_transparent else 'url',
            'url': url,
    def playlist_from_matches(cls, matches, playlist_id=None, playlist_title=None,
                              getter=IDENTITY, ie=None, video_kwargs=None, **kwargs):
        return cls.playlist_result(
            (cls.url_result(m, ie, **(video_kwargs or {})) for m in orderedSet(map(getter, matches), lazy=True)),
            playlist_id, playlist_title, **kwargs)
    def playlist_result(entries, playlist_id=None, playlist_title=None, playlist_description=None, *, multi_video=False, **kwargs):
        """Returns a playlist"""
        if playlist_id:
            kwargs['id'] = playlist_id
        if playlist_title:
            kwargs['title'] = playlist_title
        if playlist_description is not None:
            kwargs['description'] = playlist_description
            '_type': 'multi_video' if multi_video else 'playlist',
            'entries': entries,
    def _search_regex(self, pattern, string, name, default=NO_DEFAULT, fatal=True, flags=0, group=None):
        Perform a regex search on the given string, using a single or a list of
        patterns returning the first matching group.
        In case of failure return a default value or raise a WARNING or a
        RegexNotFoundError, depending on fatal, specifying the field name.
        if string is None:
            mobj = None
        elif isinstance(pattern, (str, re.Pattern)):
            mobj = re.search(pattern, string, flags)
            for p in pattern:
                mobj = re.search(p, string, flags)
                if mobj:
        _name = self._downloader._format_err(name, self._downloader.Styles.EMPHASIS)
            if group is None:
                # return the first matching group
                return next(g for g in mobj.groups() if g is not None)
            elif isinstance(group, (list, tuple)):
                return tuple(mobj.group(g) for g in group)
                return mobj.group(group)
        elif default is not NO_DEFAULT:
        elif fatal:
            raise RegexNotFoundError(f'Unable to extract {_name}')
            self.report_warning(f'unable to extract {_name}' + bug_reports_message())
    def _search_json(self, start_pattern, string, name, video_id, *, end_pattern='',
                     contains_pattern=r'{(?s:.+)}', fatal=True, default=NO_DEFAULT, **kwargs):
        """Searches string for the JSON object specified by start_pattern"""
        # NB: end_pattern is only used to reduce the size of the initial match
            default, has_default = {}, False
            fatal, has_default = False, True
        json_string = self._search_regex(
            rf'(?:{start_pattern})\s*(?P<json>{contains_pattern})\s*(?:{end_pattern})',
            string, name, group='json', fatal=fatal, default=None if has_default else NO_DEFAULT)
        if not json_string:
            return self._parse_json(json_string, video_id, ignore_extra=True, **kwargs)
                    f'Unable to extract {_name} - Failed to parse JSON', cause=e.cause, video_id=video_id)
            elif not has_default:
                    f'Unable to extract {_name} - Failed to parse JSON: {e}', video_id=video_id)
    def _html_search_regex(self, pattern, string, name, default=NO_DEFAULT, fatal=True, flags=0, group=None):
        Like _search_regex, but strips HTML tags and unescapes entities.
        res = self._search_regex(pattern, string, name, default, fatal, flags, group)
        if isinstance(res, tuple):
            return tuple(map(clean_html, res))
        return clean_html(res)
    def _get_netrc_login_info(self, netrc_machine=None):
        netrc_machine = netrc_machine or self._NETRC_MACHINE
        if not netrc_machine:
            raise ExtractorError(f'Missing netrc_machine and {type(self).__name__}._NETRC_MACHINE')
        ALLOWED = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_'
        if netrc_machine.startswith(('-', '_')) or not all(c in ALLOWED for c in netrc_machine):
            raise ExtractorError(f'Invalid netrc machine: {netrc_machine!r}', expected=True)
        cmd = self.get_param('netrc_cmd')
        if cmd:
            cmd = cmd.replace('{}', netrc_machine)
            self.to_screen(f'Executing command: {cmd}')
            stdout, _, ret = Popen.run(cmd, text=True, shell=True, stdout=subprocess.PIPE)
            if ret != 0:
                raise OSError(f'Command returned error code {ret}')
            info = netrc_from_content(stdout).authenticators(netrc_machine)
        elif self.get_param('usenetrc', False):
            netrc_file = compat_expanduser(self.get_param('netrc_location') or '~')
            if os.path.isdir(netrc_file):
                netrc_file = os.path.join(netrc_file, '.netrc')
            info = netrc.netrc(netrc_file).authenticators(netrc_machine)
            return None, None
            self.to_screen(f'No authenticators for {netrc_machine}')
        self.write_debug(f'Using netrc for {netrc_machine} authentication')
        # compat: <=py3.10: netrc cannot parse tokens as empty strings, will return `""` instead
        # Ref: https://github.com/yt-dlp/yt-dlp/issues/11413
        #      https://github.com/python/cpython/commit/15409c720be0503131713e3d3abc1acd0da07378
        if sys.version_info < (3, 11):
            return tuple(x if x != '""' else '' for x in info[::2])
        return info[0], info[2]
    def _get_login_info(self, username_option='username', password_option='password', netrc_machine=None):
        Get the login info as (username, password)
        First look for the manually specified credentials using username_option
        and password_option as keys in params dictionary. If no such credentials
        are available try the netrc_cmd if it is defined or look in the
        netrc file using the netrc_machine or _NETRC_MACHINE value.
        If there's no info available, return (None, None)
        username = self.get_param(username_option)
            password = self.get_param(password_option)
                username, password = self._get_netrc_login_info(netrc_machine)
            except (OSError, netrc.NetrcParseError) as err:
                self.report_warning(f'Failed to parse .netrc: {err}')
        return username, password
    def _get_tfa_info(self, note='two-factor verification code'):
        Get the two-factor authentication info
        TODO - asking the user will be required for sms/phone verify
        currently just uses the command line option
        If there's no info available, return None
        tfa = self.get_param('twofactor')
        if tfa is not None:
            return tfa
        return getpass.getpass(f'Type {note} and press [Return]: ')
    # Helper functions for extracting OpenGraph info
    def _og_regexes(prop):
        content_re = r'content=(?:"([^"]+?)"|\'([^\']+?)\'|\s*([^\s"\'=<>`]+?)(?=\s|/?>))'
        property_re = r'(?:name|property)=(?:\'og{sep}{prop}\'|"og{sep}{prop}"|\s*og{sep}{prop}\b)'.format(
            prop=re.escape(prop), sep='(?:&#x3A;|[:-])')
        template = r'<meta[^>]+?%s[^>]+?%s'
            template % (property_re, content_re),
            template % (content_re, property_re),
    def _meta_regex(prop):
        return rf'''(?isx)<meta
                    (?=[^>]+(?:itemprop|name|property|id|http-equiv)=(["\']?){re.escape(prop)}\1)
                    [^>]+?content=(["\'])(?P<content>.*?)\2'''
    def _og_search_property(self, prop, html, name=None, **kargs):
        prop = variadic(prop)
        if name is None:
            name = f'OpenGraph {prop[0]}'
        og_regexes = []
        for p in prop:
            og_regexes.extend(self._og_regexes(p))
        escaped = self._search_regex(og_regexes, html, name, flags=re.DOTALL, **kargs)
        if escaped is None:
        return unescapeHTML(escaped)
    def _og_search_thumbnail(self, html, **kargs):
        return self._og_search_property('image', html, 'thumbnail URL', fatal=False, **kargs)
    def _og_search_description(self, html, **kargs):
        return self._og_search_property('description', html, fatal=False, **kargs)
    def _og_search_title(self, html, *, fatal=False, **kargs):
        return self._og_search_property('title', html, fatal=fatal, **kargs)
    def _og_search_video_url(self, html, name='video url', secure=True, **kargs):
        regexes = self._og_regexes('video') + self._og_regexes('video:url')
        if secure:
            regexes = self._og_regexes('video:secure_url') + regexes
        return self._html_search_regex(regexes, html, name, **kargs)
    def _og_search_url(self, html, **kargs):
        return self._og_search_property('url', html, **kargs)
    def _html_extract_title(self, html, name='title', *, fatal=False, **kwargs):
        return self._html_search_regex(r'(?s)<title\b[^>]*>([^<]+)</title>', html, name, fatal=fatal, **kwargs)
    def _html_search_meta(self, name, html, display_name=None, fatal=False, **kwargs):
        name = variadic(name)
        if display_name is None:
            display_name = name[0]
        return self._html_search_regex(
            [self._meta_regex(n) for n in name],
            html, display_name, fatal=fatal, group='content', **kwargs)
    def _dc_search_uploader(self, html):
        return self._html_search_meta('dc.creator', html, 'uploader')
    def _rta_search(html):
        # See http://www.rtalabel.org/index.php?content=howtofaq#single
        if re.search(r'(?ix)<meta\s+name="rating"\s+'
                     r'     content="RTA-5042-1996-1400-1577-RTA"',
                     html):
            return 18
        # And then there are the jokers who advertise that they use RTA, but actually don't.
        AGE_LIMIT_MARKERS = [
            r'Proudly Labeled <a href="http://www\.rtalabel\.org/" title="Restricted to Adults">RTA</a>',
            r'>[^<]*you acknowledge you are at least (\d+) years old',
            r'>\s*(?:18\s+U(?:\.S\.C\.|SC)\s+)?(?:§+\s*)?2257\b',
        age_limit = None
        for marker in AGE_LIMIT_MARKERS:
            mobj = re.search(marker, html)
                age_limit = max(age_limit or 0, int(traverse_obj(mobj, 1, default=18)))
        return age_limit
    def _media_rating_search(self, html):
        # See http://www.tjg-designs.com/WP/metadata-code-examples-adding-metadata-to-your-web-pages/
        rating = self._html_search_meta('rating', html)
        if not rating:
        RATING_TABLE = {
            'safe for kids': 0,
            'general': 8,
            '14 years': 14,
            'mature': 17,
            'restricted': 19,
        return RATING_TABLE.get(rating.lower())
    def _family_friendly_search(self, html):
        # See http://schema.org/VideoObject
        family_friendly = self._html_search_meta(
            'isFamilyFriendly', html, default=None)
        if not family_friendly:
            '1': 0,
            'true': 0,
            '0': 18,
            'false': 18,
        return RATING_TABLE.get(family_friendly.lower())
    def _twitter_search_player(self, html):
        return self._html_search_meta('twitter:player', html,
                                      'twitter card player')
    def _yield_json_ld(self, html, video_id, *, fatal=True, default=NO_DEFAULT):
        """Yield all json ld objects in the html"""
        if default is not NO_DEFAULT:
            fatal = False
        if not fatal and not isinstance(html, str):
        for mobj in re.finditer(JSON_LD_RE, html):
            json_ld_item = self._parse_json(
                mobj.group('json_ld'), video_id, fatal=fatal,
                errnote=False if default is not NO_DEFAULT else None)
            for json_ld in variadic(json_ld_item):
                if isinstance(json_ld, dict):
                    yield json_ld
    def _search_json_ld(self, html, video_id, expected_type=None, *, fatal=True, default=NO_DEFAULT):
        """Search for a video in any json ld in the html"""
        info = self._json_ld(
            list(self._yield_json_ld(html, video_id, fatal=fatal, default=default)),
            video_id, fatal=fatal, expected_type=expected_type)
        if info:
            return info
            raise RegexNotFoundError('Unable to extract JSON-LD')
            self.report_warning(f'unable to extract JSON-LD {bug_reports_message()}')
    def _json_ld(self, json_ld, video_id, fatal=True, expected_type=None):
        if isinstance(json_ld, str):
            json_ld = self._parse_json(json_ld, video_id, fatal=fatal)
        if not json_ld:
        info = {}
        INTERACTION_TYPE_MAP = {
            'CommentAction': 'comment',
            'AgreeAction': 'like',
            'DisagreeAction': 'dislike',
            'LikeAction': 'like',
            'DislikeAction': 'dislike',
            'ListenAction': 'view',
            'WatchAction': 'view',
            'ViewAction': 'view',
        def is_type(e, *expected_types):
            type_ = variadic(traverse_obj(e, '@type'))
            return any(x in type_ for x in expected_types)
        def extract_interaction_type(e):
            interaction_type = e.get('interactionType')
            if isinstance(interaction_type, dict):
                interaction_type = interaction_type.get('@type')
            return str_or_none(interaction_type)
        def extract_interaction_statistic(e):
            interaction_statistic = e.get('interactionStatistic')
            if isinstance(interaction_statistic, dict):
                interaction_statistic = [interaction_statistic]
            if not isinstance(interaction_statistic, list):
            for is_e in interaction_statistic:
                if not is_type(is_e, 'InteractionCounter'):
                interaction_type = extract_interaction_type(is_e)
                if not interaction_type:
                # For interaction count some sites provide string instead of
                # an integer (as per spec) with non digit characters (e.g. ",")
                # so extracting count with more relaxed str_to_int
                interaction_count = str_to_int(is_e.get('userInteractionCount'))
                if interaction_count is None:
                count_kind = INTERACTION_TYPE_MAP.get(interaction_type.split('/')[-1])
                if not count_kind:
                count_key = f'{count_kind}_count'
                if info.get(count_key) is not None:
                info[count_key] = interaction_count
        def extract_chapter_information(e):
            chapters = [{
                'title': part.get('name'),
                'start_time': part.get('startOffset'),
                'end_time': part.get('endOffset'),
            } for part in variadic(e.get('hasPart') or []) if part.get('@type') == 'Clip']
            for idx, (last_c, current_c, next_c) in enumerate(zip(
                    [{'end_time': 0}, *chapters], chapters, chapters[1:], strict=False)):
                current_c['end_time'] = current_c['end_time'] or next_c['start_time']
                current_c['start_time'] = current_c['start_time'] or last_c['end_time']
                if None in current_c.values():
                    self.report_warning(f'Chapter {idx} contains broken data. Not extracting chapters')
            if chapters:
                chapters[-1]['end_time'] = chapters[-1]['end_time'] or info['duration']
                info['chapters'] = chapters
        def extract_video_object(e):
            author = e.get('author')
            info.update({
                'url': url_or_none(e.get('contentUrl')),
                'ext': mimetype2ext(e.get('encodingFormat')),
                'title': unescapeHTML(e.get('name')),
                'description': unescapeHTML(e.get('description')),
                'thumbnails': traverse_obj(e, (('thumbnailUrl', 'thumbnailURL', 'thumbnail_url'), (None, ...), {
                    'url': ({str}, {unescapeHTML}, {self._proto_relative_url}, {url_or_none}),
                'duration': parse_duration(e.get('duration')),
                'timestamp': unified_timestamp(e.get('uploadDate')),
                # author can be an instance of 'Organization' or 'Person' types.
                # both types can have 'name' property(inherited from 'Thing' type). [1]
                # however some websites are using 'Text' type instead.
                # 1. https://schema.org/VideoObject
                'uploader': author.get('name') if isinstance(author, dict) else author if isinstance(author, str) else None,
                'artist': traverse_obj(e, ('byArtist', 'name'), expected_type=str),
                'filesize': int_or_none(float_or_none(e.get('contentSize'))),
                'tbr': int_or_none(e.get('bitrate')),
                'width': int_or_none(e.get('width')),
                'height': int_or_none(e.get('height')),
                'view_count': int_or_none(e.get('interactionCount')),
                'tags': try_call(lambda: e.get('keywords').split(',')),
            if is_type(e, 'AudioObject'):
                    'abr': int_or_none(e.get('bitrate')),
            extract_interaction_statistic(e)
            extract_chapter_information(e)
        def traverse_json_ld(json_ld, at_top_level=True):
            for e in variadic(json_ld):
                if not isinstance(e, dict):
                if at_top_level and '@context' not in e:
                if at_top_level and set(e.keys()) == {'@context', '@graph'}:
                    traverse_json_ld(e['@graph'], at_top_level=False)
                if expected_type is not None and not is_type(e, expected_type):
                rating = traverse_obj(e, ('aggregateRating', 'ratingValue'), expected_type=float_or_none)
                if rating is not None:
                    info['average_rating'] = rating
                if is_type(e, 'TVEpisode', 'Episode', 'PodcastEpisode'):
                    episode_name = unescapeHTML(e.get('name'))
                        'episode': episode_name,
                        'episode_number': int_or_none(e.get('episodeNumber')),
                    if not info.get('title') and episode_name:
                        info['title'] = episode_name
                    part_of_season = e.get('partOfSeason')
                    if is_type(part_of_season, 'TVSeason', 'Season', 'CreativeWorkSeason'):
                            'season': unescapeHTML(part_of_season.get('name')),
                            'season_number': int_or_none(part_of_season.get('seasonNumber')),
                    part_of_series = e.get('partOfSeries') or e.get('partOfTVSeries')
                    if is_type(part_of_series, 'TVSeries', 'Series', 'CreativeWorkSeries'):
                        info['series'] = unescapeHTML(part_of_series.get('name'))
                elif is_type(e, 'Movie'):
                        'timestamp': unified_timestamp(e.get('dateCreated')),
                elif is_type(e, 'Article', 'NewsArticle'):
                        'timestamp': parse_iso8601(e.get('datePublished')),
                        'title': unescapeHTML(e.get('headline')),
                        'description': unescapeHTML(e.get('articleBody') or e.get('description')),
                    if is_type(traverse_obj(e, ('video', 0)), 'VideoObject'):
                        extract_video_object(e['video'][0])
                    elif is_type(traverse_obj(e, ('subjectOf', 0)), 'VideoObject'):
                        extract_video_object(e['subjectOf'][0])
                elif is_type(e, 'VideoObject', 'AudioObject'):
                    extract_video_object(e)
                    if expected_type is None:
                video = e.get('video')
                if is_type(video, 'VideoObject'):
                    extract_video_object(video)
        traverse_json_ld(json_ld)
        return filter_dict(info)
    def _search_nextjs_data(self, webpage, video_id, *, fatal=True, default=NO_DEFAULT, **kw):
        if default == '{}':
            self._downloader.deprecation_warning('using `default=\'{}\'` is deprecated, use `default={}` instead')
            default = {}
        return self._search_json(
            r'<script[^>]+id=[\'"]__NEXT_DATA__[\'"][^>]*>', webpage, 'next.js data',
            video_id, end_pattern='</script>', fatal=fatal, default=default, **kw)
    def _search_nextjs_v13_data(self, webpage, video_id, fatal=True):
        """Parses Next.js app router flight data that was introduced in Next.js v13"""
        nextjs_data = {}
        if not fatal and not isinstance(webpage, str):
            return nextjs_data
        def flatten(flight_data):
            if not isinstance(flight_data, list):
            if len(flight_data) == 4 and flight_data[0] == '$':
                _, name, _, data = flight_data
                if not isinstance(data, dict):
                children = data.pop('children', None)
                if data and isinstance(name, str) and re.fullmatch(r'\$L[0-9a-f]+', name):
                    # It is useful hydration JSON data
                    nextjs_data[name[2:]] = data
                flatten(children)
            for f in flight_data:
                flatten(f)
        flight_text = ''
        # The pattern for the surrounding JS/tag should be strict as it's a hardcoded string in the next.js source
        # Ref: https://github.com/vercel/next.js/blob/5a4a08fdc/packages/next/src/server/app-render/use-flight-response.tsx#L189
        for flight_segment in re.findall(r'<script\b[^>]*>self\.__next_f\.push\((\[.+?\])\)</script>', webpage):
            segment = self._parse_json(flight_segment, video_id, fatal=fatal, errnote=None if fatal else False)
            # Some earlier versions of next.js "optimized" away this array structure; this is unsupported
            # Ref: https://github.com/vercel/next.js/commit/0123a9d5c9a9a77a86f135b7ae30b46ca986d761
            if not isinstance(segment, list) or len(segment) != 2:
                self.write_debug(
                    f'{video_id}: Unsupported next.js flight data structure detected', only_once=True)
            # Only use the relevant payload type (1 == data)
            # Ref: https://github.com/vercel/next.js/blob/5a4a08fdc/packages/next/src/server/app-render/use-flight-response.tsx#L11-L14
            payload_type, chunk = segment
            if payload_type == 1:
                flight_text += chunk
        for f in flight_text.splitlines():
            prefix, _, body = f.lstrip().partition(':')
            if not re.fullmatch(r'[0-9a-f]+', prefix):
            # The body still isn't guaranteed to be valid JSON, so parsing should always be non-fatal
            if body.startswith('[') and body.endswith(']'):
                flatten(self._parse_json(body, video_id, fatal=False, errnote=False))
            elif body.startswith('{') and body.endswith('}'):
                data = self._parse_json(body, video_id, fatal=False, errnote=False)
                if data is not None:
                    nextjs_data[prefix] = data
    def _search_nuxt_data(self, webpage, video_id, context_name='__NUXT__', *, fatal=True, traverse=('data', 0)):
        """Parses Nuxt.js metadata. This works as long as the function __NUXT__ invokes is a pure function"""
        rectx = re.escape(context_name)
        FUNCTION_RE = r'\(function\((?P<arg_keys>.*?)\){.*?\breturn\s+(?P<js>{.*?})\s*;?\s*}\((?P<arg_vals>.*?)\)'
        js, arg_keys, arg_vals = self._search_regex(
            (rf'<script>\s*window\.{rectx}={FUNCTION_RE}\s*\)\s*;?\s*</script>', rf'{rectx}\(.*?{FUNCTION_RE}'),
            webpage, context_name, group=('js', 'arg_keys', 'arg_vals'),
            default=NO_DEFAULT if fatal else (None, None, None))
        if js is None:
        args = dict(zip(arg_keys.split(','), map(json.dumps, self._parse_json(
            f'[{arg_vals}]', video_id, transform_source=js_to_json, fatal=fatal) or ()), strict=True))
        ret = self._parse_json(js, video_id, transform_source=functools.partial(js_to_json, vars=args), fatal=fatal)
        return traverse_obj(ret, traverse) or {}
    def _resolve_nuxt_array(self, array, video_id, *, fatal=True, default=NO_DEFAULT):
        """Resolves Nuxt rich JSON payload arrays"""
        # Ref: https://github.com/nuxt/nuxt/commit/9e503be0f2a24f4df72a3ccab2db4d3e63511f57
        #      https://github.com/nuxt/nuxt/pull/19205
        if not isinstance(array, list) or not array:
            error_msg = 'Unable to resolve Nuxt JSON data: invalid input'
                raise ExtractorError(error_msg, video_id=video_id)
            elif default is NO_DEFAULT:
                self.report_warning(error_msg, video_id=video_id)
            return {} if default is NO_DEFAULT else default
        def indirect_reviver(data):
        def json_reviver(data):
            return json.loads(data)
        gen = devalue.parse_iter(array, revivers={
            'NuxtError': indirect_reviver,
            'EmptyShallowRef': json_reviver,
            'EmptyRef': json_reviver,
            'ShallowRef': indirect_reviver,
            'ShallowReactive': indirect_reviver,
            'Ref': indirect_reviver,
            'Reactive': indirect_reviver,
                error_msg = f'Error resolving Nuxt JSON: {gen.send(None)}'
                    self.report_warning(error_msg, video_id=video_id, only_once=True)
                    self.write_debug(f'{video_id}: {error_msg}', only_once=True)
            except StopIteration as error:
                return error.value or ({} if default is NO_DEFAULT else default)
    def _search_nuxt_json(self, webpage, video_id, *, fatal=True, default=NO_DEFAULT):
        """Parses metadata from Nuxt rich JSON payloads embedded in HTML"""
        passed_default = default is not NO_DEFAULT
        array = self._search_json(
            r'<script\b[^>]+\bid="__NUXT_DATA__"[^>]*>', webpage,
            'Nuxt JSON data', video_id, contains_pattern=r'\[(?s:.+)\]',
            fatal=fatal, default=NO_DEFAULT if not passed_default else None)
        if not array:
            return default if passed_default else {}
        return self._resolve_nuxt_array(array, video_id, fatal=fatal, default=default)
    def _hidden_inputs(html):
        html = re.sub(r'<!--(?:(?!<!--).)*-->', '', html)
        hidden_inputs = {}
        for input_el in re.findall(r'(?i)(<input[^>]+>)', html):
            attrs = extract_attributes(input_el)
            if not input_el:
            if attrs.get('type') not in ('hidden', 'submit'):
            name = attrs.get('name') or attrs.get('id')
            value = attrs.get('value')
            if name and value is not None:
                hidden_inputs[name] = value
        return hidden_inputs
    def _form_hidden_inputs(self, form_id, html):
        form = self._search_regex(
            rf'(?is)<form[^>]+?id=(["\']){form_id}\1[^>]*>(?P<form>.+?)</form>',
            html, f'{form_id} form', group='form')
        return self._hidden_inputs(form)
    @classproperty(cache=True)
    def FormatSort(cls):
        class FormatSort(FormatSorter):
            def __init__(ie, *args, **kwargs):
                super().__init__(ie._downloader, *args, **kwargs)
        deprecation_warning(
            'yt_dlp.InfoExtractor.FormatSort is deprecated and may be removed in the future. '
            'Use yt_dlp.utils.FormatSorter instead')
        return FormatSort
    def _sort_formats(self, formats, field_preference=[]):
        if not field_preference:
                'yt_dlp.InfoExtractor._sort_formats is deprecated and is no longer required')
            'yt_dlp.InfoExtractor._sort_formats is deprecated and no longer works as expected. '
            'Return _format_sort_fields in the info_dict instead')
        if formats:
            formats[0]['__sort_fields'] = field_preference
    def _check_formats(self, formats, video_id):
            formats[:] = filter(
                lambda f: self._is_valid_url(
                    f['url'], video_id,
                    item='{} video format'.format(f.get('format_id')) if f.get('format_id') else 'video'),
                formats)
    def _remove_duplicate_formats(formats):
        seen_urls = set()
        seen_fragment_urls = set()
        unique_formats = []
        for f in formats:
            fragments = f.get('fragments')
            if callable(fragments):
                unique_formats.append(f)
            elif fragments:
                fragment_urls = frozenset(
                    fragment.get('url') or urljoin(f['fragment_base_url'], fragment['path'])
                    for fragment in fragments)
                if fragment_urls not in seen_fragment_urls:
                    seen_fragment_urls.add(fragment_urls)
            elif f['url'] not in seen_urls:
                seen_urls.add(f['url'])
        formats[:] = unique_formats
    def _is_valid_url(self, url, video_id, item='video', headers={}):
        url = self._proto_relative_url(url, scheme='http:')
        # For now assume non HTTP(S) URLs always valid
        if not url.startswith(('http://', 'https://')):
            self._request_webpage(url, video_id, f'Checking {item} URL', headers=headers)
                f'{video_id}: {item} URL is invalid, skipping: {e.cause!s}')
    def http_scheme(self):
        """ Either "http:" or "https:", depending on the user's preferences """
            'http:'
            if self.get_param('prefer_insecure', False)
            else 'https:')
    def _proto_relative_url(self, url, scheme=None):
        scheme = scheme or self.http_scheme()
        assert scheme.endswith(':')
        return sanitize_url(url, scheme=scheme[:-1])
    def _sleep(self, timeout, video_id, msg_template=None):
        if msg_template is None:
            msg_template = '%(video_id)s: Waiting for %(timeout)s seconds'
        msg = msg_template % {'video_id': video_id, 'timeout': timeout}
        self.to_screen(msg)
    def _extract_f4m_formats(self, manifest_url, video_id, preference=None, quality=None, f4m_id=None,
                             transform_source=lambda s: fix_xml_ampersands(s).strip(),
                             fatal=True, m3u8_id=None, data=None, headers={}, query={}):
        if self.get_param('ignore_no_formats_error'):
        res = self._download_xml_handle(
            manifest_url, video_id, 'Downloading f4m manifest',
            'Unable to download f4m manifest',
            # Some manifests may be malformed, e.g. prosiebensat1 generated manifests
            # (see https://github.com/ytdl-org/youtube-dl/issues/6215#issuecomment-121704244)
            transform_source=transform_source,
            fatal=fatal, data=data, headers=headers, query=query)
        manifest, urlh = res
        manifest_url = urlh.url
        return self._parse_f4m_formats(
            manifest, manifest_url, video_id, preference=preference, quality=quality, f4m_id=f4m_id,
            transform_source=transform_source, fatal=fatal, m3u8_id=m3u8_id)
    def _parse_f4m_formats(self, manifest, manifest_url, video_id, preference=None, quality=None, f4m_id=None,
                           fatal=True, m3u8_id=None):
        if not isinstance(manifest, xml.etree.ElementTree.Element) and not fatal:
        # currently yt-dlp cannot decode the playerVerificationChallenge as Akamai uses Adobe Alchemy
        akamai_pv = manifest.find('{http://ns.adobe.com/f4m/1.0}pv-2.0')
        if akamai_pv is not None and ';' in akamai_pv.text:
            player_verification_challenge = akamai_pv.text.split(';')[0]
            if player_verification_challenge.strip() != '':
        formats = []
        manifest_version = '1.0'
        media_nodes = manifest.findall('{http://ns.adobe.com/f4m/1.0}media')
        if not media_nodes:
            manifest_version = '2.0'
            media_nodes = manifest.findall('{http://ns.adobe.com/f4m/2.0}media')
        # Remove unsupported DRM protected media from final formats
        # rendition (see https://github.com/ytdl-org/youtube-dl/issues/8573).
        media_nodes = remove_encrypted_media(media_nodes)
            return formats
        manifest_base_url = get_base_url(manifest)
        bootstrap_info = xpath_element(
            manifest, ['{http://ns.adobe.com/f4m/1.0}bootstrapInfo', '{http://ns.adobe.com/f4m/2.0}bootstrapInfo'],
            'bootstrap info', default=None)
        vcodec = None
        mime_type = xpath_text(
            manifest, ['{http://ns.adobe.com/f4m/1.0}mimeType', '{http://ns.adobe.com/f4m/2.0}mimeType'],
            'base URL', default=None)
        if mime_type and mime_type.startswith('audio/'):
            vcodec = 'none'
        for i, media_el in enumerate(media_nodes):
            tbr = int_or_none(media_el.attrib.get('bitrate'))
            width = int_or_none(media_el.attrib.get('width'))
            height = int_or_none(media_el.attrib.get('height'))
            format_id = join_nonempty(f4m_id, tbr or i)
            # If <bootstrapInfo> is present, the specified f4m is a
            # stream-level manifest, and only set-level manifests may refer to
            # external resources.  See section 11.4 and section 4 of F4M spec
            if bootstrap_info is None:
                media_url = None
                # @href is introduced in 2.0, see section 11.6 of F4M spec
                if manifest_version == '2.0':
                    media_url = media_el.attrib.get('href')
                if media_url is None:
                    media_url = media_el.attrib.get('url')
                if not media_url:
                manifest_url = (
                    media_url if media_url.startswith(('http://', 'https://'))
                    else ((manifest_base_url or '/'.join(manifest_url.split('/')[:-1])) + '/' + media_url))
                # If media_url is itself a f4m manifest do the recursive extraction
                # since bitrates in parent manifest (this one) and media_url manifest
                # may differ leading to inability to resolve the format by requested
                # bitrate in f4m downloader
                ext = determine_ext(manifest_url)
                if ext == 'f4m':
                    f4m_formats = self._extract_f4m_formats(
                        manifest_url, video_id, preference=preference, quality=quality, f4m_id=f4m_id,
                        transform_source=transform_source, fatal=fatal)
                    # Sometimes stream-level manifest contains single media entry that
                    # does not contain any quality metadata (e.g. http://matchtv.ru/#live-player).
                    # At the same time parent's media entry in set-level manifest may
                    # contain it. We will copy it from parent in such cases.
                    if len(f4m_formats) == 1:
                        f = f4m_formats[0]
                        f.update({
                            'tbr': f.get('tbr') or tbr,
                            'width': f.get('width') or width,
                            'height': f.get('height') or height,
                            'format_id': f.get('format_id') if not tbr else format_id,
                            'vcodec': vcodec,
                    formats.extend(f4m_formats)
                elif ext == 'm3u8':
                    formats.extend(self._extract_m3u8_formats(
                        manifest_url, video_id, 'mp4', preference=preference,
                        quality=quality, m3u8_id=m3u8_id, fatal=fatal))
                'format_id': format_id,
                'url': manifest_url,
                'manifest_url': manifest_url,
                'ext': 'flv' if bootstrap_info is not None else None,
                'protocol': 'f4m',
                'tbr': tbr,
                'width': width,
                'height': height,
                'preference': preference,
                'quality': quality,
    def _m3u8_meta_format(self, m3u8_url, ext=None, preference=None, quality=None, m3u8_id=None):
            'format_id': join_nonempty(m3u8_id, 'meta'),
            'url': m3u8_url,
            'ext': ext,
            'protocol': 'm3u8',
            'preference': preference - 100 if preference else -100,
            'resolution': 'multiple',
            'format_note': 'Quality selection URL',
    def _report_ignoring_subs(self, name):
        self.report_warning(bug_reports_message(
            f'Ignoring subtitle tracks found in the {name} manifest; '
            'if any subtitle tracks are missing,',
        ), only_once=True)
    def _extract_m3u8_formats(self, *args, **kwargs):
        fmts, subs = self._extract_m3u8_formats_and_subtitles(*args, **kwargs)
        if subs:
            self._report_ignoring_subs('HLS')
        return fmts
    def _extract_m3u8_formats_and_subtitles(
            self, m3u8_url, video_id, ext=None, entry_protocol='m3u8_native',
            preference=None, quality=None, m3u8_id=None, note=None,
            errnote=None, fatal=True, live=False, data=None, headers={},
            query={}):
        if not m3u8_url:
            if errnote is not False:
                errnote = errnote or 'Failed to obtain m3u8 URL'
                    raise ExtractorError(errnote, video_id=video_id)
                self.report_warning(f'{errnote}{bug_reports_message()}')
            return [], {}
            note = 'Downloading m3u8 information'
            errnote = 'Failed to download m3u8 information'
        response = self._request_webpage(
            m3u8_url, video_id, note=note, errnote=errnote,
        if response is False:
        with contextlib.closing(response):
            prefix = response.read(512)
            if not prefix.startswith(b'#EXTM3U'):
                msg = 'Response data has no m3u header'
                    raise ExtractorError(msg, video_id=video_id)
                self.report_warning(f'{msg}{bug_reports_message()}', video_id=video_id)
            content = self._webpage_read_content(
                response, m3u8_url, video_id, note=note, errnote=errnote,
                fatal=fatal, prefix=prefix, data=data)
        return self._parse_m3u8_formats_and_subtitles(
            content, response.url, ext=ext, entry_protocol=entry_protocol,
            preference=preference, quality=quality, m3u8_id=m3u8_id,
            note=note, errnote=errnote, fatal=fatal, live=live, data=data,
            headers=headers, query=query, video_id=video_id)
    def _parse_m3u8_formats_and_subtitles(
            self, m3u8_doc, m3u8_url=None, ext=None, entry_protocol='m3u8_native',
            preference=None, quality=None, m3u8_id=None, live=False, note=None,
            errnote=None, fatal=True, data=None, headers={}, query={},
            video_id=None):
        has_drm = HlsFD._has_drm(m3u8_doc)
        def format_url(url):
            return url if re.match(r'https?://', url) else urllib.parse.urljoin(m3u8_url, url)
        if self.get_param('hls_split_discontinuity', False):
            def _extract_m3u8_playlist_indices(manifest_url=None, m3u8_doc=None):
                if not m3u8_doc:
                    if not manifest_url:
                    m3u8_doc = self._download_webpage(
                        manifest_url, video_id, fatal=fatal, data=data, headers=headers,
                        note=False, errnote='Failed to download m3u8 playlist information')
                    if m3u8_doc is False:
                return range(1 + sum(line.startswith('#EXT-X-DISCONTINUITY') for line in m3u8_doc.splitlines()))
            def _extract_m3u8_playlist_indices(*args, **kwargs):
                return [None]
        # References:
        # 1. https://tools.ietf.org/html/draft-pantos-http-live-streaming-21
        # 2. https://github.com/ytdl-org/youtube-dl/issues/12211
        # 3. https://github.com/ytdl-org/youtube-dl/issues/18923
        # We should try extracting formats only from master playlists [1, 4.3.4],
        # i.e. playlists that describe available qualities. On the other hand
        # media playlists [1, 4.3.3] should be returned as is since they contain
        # just the media without qualities renditions.
        # Fortunately, master playlist can be easily distinguished from media
        # playlist based on particular tags availability. As of [1, 4.3.3, 4.3.4]
        # master playlist tags MUST NOT appear in a media playlist and vice versa.
        # As of [1, 4.3.3.1] #EXT-X-TARGETDURATION tag is REQUIRED for every
        # media playlist and MUST NOT appear in master playlist thus we can
        # clearly detect media playlist with this criterion.
        if '#EXT-X-TARGETDURATION' in m3u8_doc:  # media playlist, return as is
                'format_id': join_nonempty(m3u8_id, idx),
                'format_index': idx,
                'url': m3u8_url or encode_data_uri(m3u8_doc.encode(), 'application/x-mpegurl'),
                'protocol': entry_protocol,
                'has_drm': has_drm,
            } for idx in _extract_m3u8_playlist_indices(m3u8_doc=m3u8_doc)]
            return formats, subtitles
        groups = {}
        last_stream_inf = {}
        def extract_media(x_media_line):
            media = parse_m3u8_attributes(x_media_line)
            # As per [1, 4.3.4.1] TYPE, GROUP-ID and NAME are REQUIRED
            media_type, group_id, name = media.get('TYPE'), media.get('GROUP-ID'), media.get('NAME')
            if not (media_type and group_id and name):
            groups.setdefault(group_id, []).append(media)
            # <https://tools.ietf.org/html/rfc8216#section-4.3.4.1>
            if media_type == 'SUBTITLES':
                # According to RFC 8216 §4.3.4.2.1, URI is REQUIRED in the
                # EXT-X-MEDIA tag if the media type is SUBTITLES.
                # However, lack of URI has been spotted in the wild.
                # e.g. NebulaIE; see https://github.com/yt-dlp/yt-dlp/issues/339
                if not media.get('URI'):
                url = format_url(media['URI'])
                sub_info = {
                    'ext': determine_ext(url),
                if sub_info['ext'] == 'm3u8':
                    # Per RFC 8216 §3.1, the only possible subtitle format m3u8
                    # files may contain is WebVTT:
                    # <https://tools.ietf.org/html/rfc8216#section-3.1>
                    sub_info['ext'] = 'vtt'
                    sub_info['protocol'] = 'm3u8_native'
                lang = media.get('LANGUAGE') or 'und'
                subtitles.setdefault(lang, []).append(sub_info)
            if media_type not in ('VIDEO', 'AUDIO'):
            media_url = media.get('URI')
            if media_url:
                manifest_url = format_url(media_url)
                is_audio = media_type == 'AUDIO'
                is_alternate = media.get('DEFAULT') == 'NO' or media.get('AUTOSELECT') == 'NO'
                formats.extend({
                    'format_id': join_nonempty(m3u8_id, group_id, name, idx),
                    'format_note': name,
                    'manifest_url': m3u8_url,
                    'language': media.get('LANGUAGE'),
                    'vcodec': 'none' if is_audio else None,
                    # Alternate audio formats (e.g. audio description) should be deprioritized
                    'source_preference': -2 if is_audio and is_alternate else None,
                    # Save this to assign source_preference based on associated video stream
                    '_audio_group_id': group_id if is_audio and not is_alternate else None,
                } for idx in _extract_m3u8_playlist_indices(manifest_url))
        def build_stream_name():
            # Despite specification does not mention NAME attribute for
            # EXT-X-STREAM-INF tag it still sometimes may be present (see [1]
            # or vidio test in TestInfoExtractor.test_parse_m3u8_formats)
            # 1. http://www.vidio.com/watch/165683-dj_ambred-booyah-live-2015
            stream_name = last_stream_inf.get('NAME')
            if stream_name:
                return stream_name
            # If there is no NAME in EXT-X-STREAM-INF it will be obtained
            # from corresponding rendition group
            stream_group_id = last_stream_inf.get('VIDEO')
            if not stream_group_id:
            stream_group = groups.get(stream_group_id)
            if not stream_group:
                return stream_group_id
            rendition = stream_group[0]
            return rendition.get('NAME') or stream_group_id
        # parse EXT-X-MEDIA tags before EXT-X-STREAM-INF in order to have the
        # chance to detect video only formats when EXT-X-STREAM-INF tags
        # precede EXT-X-MEDIA tags in HLS manifest such as [3].
        for line in m3u8_doc.splitlines():
            if line.startswith('#EXT-X-MEDIA:'):
                extract_media(line)
            if line.startswith('#EXT-X-STREAM-INF:'):
                last_stream_inf = parse_m3u8_attributes(line)
            elif line.startswith('#') or not line.strip():
                tbr = float_or_none(
                    last_stream_inf.get('AVERAGE-BANDWIDTH')
                    or last_stream_inf.get('BANDWIDTH'), scale=1000)
                manifest_url = format_url(line.strip())
                for idx in _extract_m3u8_playlist_indices(manifest_url):
                    format_id = [m3u8_id, None, idx]
                    # Bandwidth of live streams may differ over time thus making
                    # format_id unpredictable. So it's better to keep provided
                    # format_id intact.
                    if not live:
                        stream_name = build_stream_name()
                        format_id[1] = stream_name or '%d' % (tbr or len(formats))
                    f = {
                        'format_id': join_nonempty(*format_id),
                        'fps': float_or_none(last_stream_inf.get('FRAME-RATE')),
                    # YouTube-specific
                    if yt_audio_content_id := last_stream_inf.get('YT-EXT-AUDIO-CONTENT-ID'):
                        f['language'] = yt_audio_content_id.split('.')[0]
                    resolution = last_stream_inf.get('RESOLUTION')
                    if resolution:
                        mobj = re.search(r'(?P<width>\d+)[xX](?P<height>\d+)', resolution)
                            f['width'] = int(mobj.group('width'))
                            f['height'] = int(mobj.group('height'))
                    # Unified Streaming Platform
                    mobj = re.search(
                        r'audio.*?(?:%3D|=)(\d+)(?:-video.*?(?:%3D|=)(\d+))?', f['url'])
                        abr, vbr = mobj.groups()
                        abr, vbr = float_or_none(abr, 1000), float_or_none(vbr, 1000)
                            'vbr': vbr,
                            'abr': abr,
                    codecs = parse_codecs(last_stream_inf.get('CODECS'))
                    f.update(codecs)
                    audio_group_id = last_stream_inf.get('AUDIO')
                    # As per [1, 4.3.4.1.1] any EXT-X-STREAM-INF tag which
                    # references a rendition group MUST have a CODECS attribute.
                    # However, this is not always respected. E.g. [2]
                    # contains EXT-X-STREAM-INF tag which references AUDIO
                    # rendition group but does not have CODECS and despite
                    # referencing an audio group it represents a complete
                    # (with audio and video) format. So, for such cases we will
                    # ignore references to rendition groups and treat them
                    # as complete formats.
                    if audio_group_id and codecs and f.get('vcodec') != 'none':
                        # Save this to determine quality of audio formats that only have a GROUP-ID
                        f['_audio_group_id'] = audio_group_id
                        audio_group = groups.get(audio_group_id)
                        if audio_group and audio_group[0].get('URI'):
                            # TODO: update acodec for audio only formats with
                            # the same GROUP-ID
                            f['acodec'] = 'none'
                    if not f.get('ext'):
                        f['ext'] = 'm4a' if f.get('vcodec') == 'none' else 'mp4'
                    formats.append(f)
                    # for DailyMotion
                    progressive_uri = last_stream_inf.get('PROGRESSIVE-URI')
                    if progressive_uri:
                        http_f = f.copy()
                        del http_f['manifest_url']
                        http_f.update({
                            'format_id': f['format_id'].replace('hls-', 'http-'),
                            'protocol': 'http',
                            'url': progressive_uri,
                        formats.append(http_f)
        # Some audio-only formats only have a GROUP-ID without any other quality/bitrate/codec info
        # Each audio GROUP-ID corresponds with one or more video formats' AUDIO attribute
        # For sorting purposes, set source_preference based on the quality of the video formats they are grouped with
        # See https://github.com/yt-dlp/yt-dlp/issues/11178
        audio_groups_by_quality = orderedSet(f['_audio_group_id'] for f in sorted(
            traverse_obj(formats, lambda _, v: v.get('vcodec') != 'none' and v['_audio_group_id']),
            key=lambda x: (x.get('tbr') or 0, x.get('width') or 0)))
        audio_quality_map = {
            audio_groups_by_quality[0]: 'low',
            audio_groups_by_quality[-1]: 'high',
        } if len(audio_groups_by_quality) > 1 else None
        audio_preference = qualities(audio_groups_by_quality)
        for fmt in formats:
            audio_group_id = fmt.pop('_audio_group_id', None)
            if not audio_quality_map or not audio_group_id or fmt.get('vcodec') != 'none':
            # Use source_preference since quality and preference are set by params
            fmt['source_preference'] = audio_preference(audio_group_id)
            fmt['format_note'] = join_nonempty(
                fmt.get('format_note'), audio_quality_map.get(audio_group_id), delim=', ')
    def _extract_m3u8_vod_duration(
            self, m3u8_vod_url, video_id, note=None, errnote=None, data=None, headers={}, query={}):
        m3u8_vod = self._download_webpage(
            m3u8_vod_url, video_id,
            note='Downloading m3u8 VOD manifest' if note is None else note,
            errnote='Failed to download VOD manifest' if errnote is None else errnote,
            fatal=False, data=data, headers=headers, query=query)
        return self._parse_m3u8_vod_duration(m3u8_vod or '', video_id)
    def _parse_m3u8_vod_duration(self, m3u8_vod, video_id):
        if '#EXT-X-ENDLIST' not in m3u8_vod:
        return int(sum(
            float(line[len('#EXTINF:'):].split(',')[0])
            for line in m3u8_vod.splitlines() if line.startswith('#EXTINF:'))) or None
    def _extract_mpd_vod_duration(
            self, mpd_url, video_id, note=None, errnote=None, data=None, headers={}, query={}):
        mpd_doc = self._download_xml(
            mpd_url, video_id,
            note='Downloading MPD VOD manifest' if note is None else note,
        if not isinstance(mpd_doc, xml.etree.ElementTree.Element):
        return int_or_none(parse_duration(mpd_doc.get('mediaPresentationDuration')))
    def _xpath_ns(path, namespace=None):
        if not namespace:
        for c in path.split('/'):
            if not c or c == '.':
                out.append(c)
                out.append(f'{{{namespace}}}{c}')
        return '/'.join(out)
    def _extract_smil_formats_and_subtitles(self, smil_url, video_id, fatal=True, f4m_params=None, transform_source=None):
        res = self._download_smil(smil_url, video_id, fatal=fatal, transform_source=transform_source)
        smil, urlh = res
        return self._parse_smil_formats_and_subtitles(smil, urlh.url, video_id, f4m_params=f4m_params,
                                                      namespace=self._parse_smil_namespace(smil))
    def _extract_smil_formats(self, *args, **kwargs):
        fmts, subs = self._extract_smil_formats_and_subtitles(*args, **kwargs)
            self._report_ignoring_subs('SMIL')
    def _extract_smil_info(self, smil_url, video_id, fatal=True, f4m_params=None):
        res = self._download_smil(smil_url, video_id, fatal=fatal)
        smil_url = urlh.url
        return self._parse_smil(smil, smil_url, video_id, f4m_params=f4m_params)
    def _download_smil(self, smil_url, video_id, fatal=True, transform_source=None):
        return self._download_xml_handle(
            smil_url, video_id, 'Downloading SMIL file',
            'Unable to download SMIL file', fatal=fatal, transform_source=transform_source)
    def _parse_smil(self, smil, smil_url, video_id, f4m_params=None):
        namespace = self._parse_smil_namespace(smil)
        formats, subtitles = self._parse_smil_formats_and_subtitles(
            smil, smil_url, video_id, namespace=namespace, f4m_params=f4m_params)
        video_id = os.path.splitext(url_basename(smil_url))[0]
        title = None
        description = None
        upload_date = None
        for meta in smil.findall(self._xpath_ns('./head/meta', namespace)):
            name = meta.attrib.get('name')
            content = meta.attrib.get('content')
            if not name or not content:
            if not title and name == 'title':
                title = content
            elif not description and name in ('description', 'abstract'):
                description = content
            elif not upload_date and name == 'date':
                upload_date = unified_strdate(content)
        thumbnails = [{
            'id': image.get('type'),
            'url': image.get('src'),
            'width': int_or_none(image.get('width')),
            'height': int_or_none(image.get('height')),
        } for image in smil.findall(self._xpath_ns('.//image', namespace)) if image.get('src')]
            'title': title or video_id,
            'description': description,
            'upload_date': upload_date,
            'thumbnails': thumbnails,
    def _parse_smil_namespace(self, smil):
        return self._search_regex(
            r'(?i)^{([^}]+)?}smil$', smil.tag, 'namespace', default=None)
    def _parse_smil_formats(self, *args, **kwargs):
        fmts, subs = self._parse_smil_formats_and_subtitles(*args, **kwargs)
    def _parse_smil_formats_and_subtitles(
            self, smil, smil_url, video_id, namespace=None, f4m_params=None, transform_rtmp_url=None):
        base = smil_url
            b = meta.get('base') or meta.get('httpBase')
            if b:
                base = b
        rtmp_count = 0
        http_count = 0
        m3u8_count = 0
        imgs_count = 0
        srcs = set()
        media = itertools.chain.from_iterable(
            smil.findall(self._xpath_ns(arg, namespace))
            for arg in ['.//video', './/audio', './/media'])
        for medium in media:
            src = medium.get('src')
            if not src or src in srcs:
            srcs.add(src)
            bitrate = float_or_none(medium.get('system-bitrate') or medium.get('systemBitrate'), 1000)
            filesize = int_or_none(medium.get('size') or medium.get('fileSize'))
            width = int_or_none(medium.get('width'))
            height = int_or_none(medium.get('height'))
            proto = medium.get('proto')
            ext = medium.get('ext')
            src_ext = determine_ext(src, default_ext=None) or ext or urlhandle_detect_ext(
                self._request_webpage(HEADRequest(src), video_id, note='Requesting extension info', fatal=False))
            streamer = medium.get('streamer') or base
            if proto == 'rtmp' or streamer.startswith('rtmp'):
                rtmp_count += 1
                    'url': streamer,
                    'play_path': src,
                    'ext': 'flv',
                    'format_id': 'rtmp-%d' % (rtmp_count if bitrate is None else bitrate),
                    'tbr': bitrate,
                    'filesize': filesize,
                if transform_rtmp_url:
                    streamer, src = transform_rtmp_url(streamer, src)
                    formats[-1].update({
            src_url = src if src.startswith('http') else urllib.parse.urljoin(f'{base}/', src)
            src_url = src_url.strip()
            if proto == 'm3u8' or src_ext == 'm3u8':
                m3u8_formats, m3u8_subs = self._extract_m3u8_formats_and_subtitles(
                    src_url, video_id, ext or 'mp4', m3u8_id='hls', fatal=False)
                self._merge_subtitles(m3u8_subs, target=subtitles)
                if len(m3u8_formats) == 1:
                    m3u8_count += 1
                    m3u8_formats[0].update({
                        'format_id': 'hls-%d' % (m3u8_count if bitrate is None else bitrate),
                formats.extend(m3u8_formats)
            elif src_ext == 'f4m':
                f4m_url = src_url
                if not f4m_params:
                    f4m_params = {
                        'hdcore': '3.2.0',
                        'plugin': 'flowplayer-3.2.0.1',
                f4m_url += '&' if '?' in f4m_url else '?'
                f4m_url += urllib.parse.urlencode(f4m_params)
                formats.extend(self._extract_f4m_formats(f4m_url, video_id, f4m_id='hds', fatal=False))
            elif src_ext == 'mpd':
                mpd_formats, mpd_subs = self._extract_mpd_formats_and_subtitles(
                    src_url, video_id, mpd_id='dash', fatal=False)
                formats.extend(mpd_formats)
                self._merge_subtitles(mpd_subs, target=subtitles)
            elif re.search(r'\.ism/[Mm]anifest', src_url):
                ism_formats, ism_subs = self._extract_ism_formats_and_subtitles(
                    src_url, video_id, ism_id='mss', fatal=False)
                formats.extend(ism_formats)
                self._merge_subtitles(ism_subs, target=subtitles)
            elif src_url.startswith('http') and self._is_valid_url(src, video_id):
                http_count += 1
                    'url': src_url,
                    'ext': ext or src_ext or 'flv',
                    'format_id': 'http-%d' % (bitrate or http_count),
        for medium in smil.findall(self._xpath_ns('.//imagestream', namespace)):
            imgs_count += 1
                'format_id': f'imagestream-{imgs_count}',
                'url': src,
                'ext': mimetype2ext(medium.get('type')),
                'width': int_or_none(medium.get('width')),
                'height': int_or_none(medium.get('height')),
                'format_note': 'SMIL storyboards',
        smil_subs = self._parse_smil_subtitles(smil, namespace=namespace)
        self._merge_subtitles(smil_subs, target=subtitles)
    def _parse_smil_subtitles(self, smil, namespace=None, subtitles_lang='en'):
        urls = []
        subtitles = {}
        for textstream in smil.findall(self._xpath_ns('.//textstream', namespace)):
            src = textstream.get('src')
            if not src or src in urls:
            urls.append(src)
            ext = textstream.get('ext') or mimetype2ext(textstream.get('type')) or determine_ext(src)
            lang = textstream.get('systemLanguage') or textstream.get('systemLanguageName') or textstream.get('lang') or subtitles_lang
            subtitles.setdefault(lang, []).append({
        return subtitles
    def _extract_xspf_playlist(self, xspf_url, playlist_id, fatal=True):
            xspf_url, playlist_id, 'Downloading xpsf playlist',
            'Unable to download xspf manifest', fatal=fatal)
        xspf, urlh = res
        xspf_url = urlh.url
        return self._parse_xspf(
            xspf, playlist_id, xspf_url=xspf_url,
            xspf_base_url=base_url(xspf_url))
    def _parse_xspf(self, xspf_doc, playlist_id, xspf_url=None, xspf_base_url=None):
        NS_MAP = {
            'xspf': 'http://xspf.org/ns/0/',
            's1': 'http://static.streamone.nl/player/ns/0',
        entries = []
        for track in xspf_doc.findall(xpath_with_ns('./xspf:trackList/xspf:track', NS_MAP)):
            title = xpath_text(
                track, xpath_with_ns('./xspf:title', NS_MAP), 'title', default=playlist_id)
            description = xpath_text(
                track, xpath_with_ns('./xspf:annotation', NS_MAP), 'description')
            thumbnail = xpath_text(
                track, xpath_with_ns('./xspf:image', NS_MAP), 'thumbnail')
            duration = float_or_none(
                xpath_text(track, xpath_with_ns('./xspf:duration', NS_MAP), 'duration'), 1000)
            for location in track.findall(xpath_with_ns('./xspf:location', NS_MAP)):
                format_url = urljoin(xspf_base_url, location.text)
                if not format_url:
                    'url': format_url,
                    'manifest_url': xspf_url,
                    'format_id': location.get(xpath_with_ns('s1:label', NS_MAP)),
                    'width': int_or_none(location.get(xpath_with_ns('s1:width', NS_MAP))),
                    'height': int_or_none(location.get(xpath_with_ns('s1:height', NS_MAP))),
            entries.append({
                'id': playlist_id,
                'thumbnail': thumbnail,
                'duration': duration,
    def _extract_mpd_formats(self, *args, **kwargs):
        fmts, subs = self._extract_mpd_formats_and_subtitles(*args, **kwargs)
            self._report_ignoring_subs('DASH')
    def _extract_mpd_formats_and_subtitles(self, *args, **kwargs):
        periods = self._extract_mpd_periods(*args, **kwargs)
        return self._merge_mpd_periods(periods)
    def _extract_mpd_periods(
            self, mpd_url, video_id, mpd_id=None, note=None, errnote=None,
            fatal=True, data=None, headers={}, query={}):
            note='Downloading MPD manifest' if note is None else note,
            errnote='Failed to download MPD manifest' if errnote is None else errnote,
        mpd_doc, urlh = res
        if mpd_doc is None:
        # We could have been redirected to a new url when we retrieved our mpd file.
        mpd_url = urlh.url
        mpd_base_url = base_url(mpd_url)
        return self._parse_mpd_periods(mpd_doc, mpd_id, mpd_base_url, mpd_url)
    def _parse_mpd_formats(self, *args, **kwargs):
        fmts, subs = self._parse_mpd_formats_and_subtitles(*args, **kwargs)
    def _parse_mpd_formats_and_subtitles(self, *args, **kwargs):
        periods = self._parse_mpd_periods(*args, **kwargs)
    def _merge_mpd_periods(self, periods):
        Combine all formats and subtitles from an MPD manifest into a single list,
        by concatenate streams with similar formats.
        formats, subtitles = {}, {}
        for period in periods:
            for f in period['formats']:
                assert 'is_dash_periods' not in f, 'format already processed'
                f['is_dash_periods'] = True
                format_key = tuple(v for k, v in f.items() if k not in (
                    ('format_id', 'fragments', 'manifest_stream_number')))
                if format_key not in formats:
                    formats[format_key] = f
                elif 'fragments' in f:
                    formats[format_key].setdefault('fragments', []).extend(f['fragments'])
            if subtitles and period['subtitles']:
                    'Found subtitles in multiple periods in the DASH manifest; '
                    'if part of the subtitles are missing,',
            for sub_lang, sub_info in period['subtitles'].items():
                subtitles.setdefault(sub_lang, []).extend(sub_info)
        return list(formats.values()), subtitles
    def _parse_mpd_periods(self, mpd_doc, mpd_id=None, mpd_base_url='', mpd_url=None):
        Parse formats from MPD manifest.
        References:
         1. MPEG-DASH Standard, ISO/IEC 23009-1:2014(E),
            http://standards.iso.org/ittf/PubliclyAvailableStandards/c065274_ISO_IEC_23009-1_2014.zip
         2. https://en.wikipedia.org/wiki/Dynamic_Adaptive_Streaming_over_HTTP
        if not self.get_param('dynamic_mpd', True):
            if mpd_doc.get('type') == 'dynamic':
        namespace = self._search_regex(r'(?i)^{([^}]+)?}MPD$', mpd_doc.tag, 'namespace', default=None)
        def _add_ns(path):
            return self._xpath_ns(path, namespace)
        def is_drm_protected(element):
            return element.find(_add_ns('ContentProtection')) is not None
        def extract_multisegment_info(element, ms_parent_info):
            ms_info = ms_parent_info.copy()
            # As per [1, 5.3.9.2.2] SegmentList and SegmentTemplate share some
            # common attributes and elements.  We will only extract relevant
            # for us.
            def extract_common(source):
                segment_timeline = source.find(_add_ns('SegmentTimeline'))
                if segment_timeline is not None:
                    s_e = segment_timeline.findall(_add_ns('S'))
                    if s_e:
                        ms_info['total_number'] = 0
                        ms_info['s'] = []
                        for s in s_e:
                            r = int(s.get('r', 0))
                            ms_info['total_number'] += 1 + r
                            ms_info['s'].append({
                                't': int(s.get('t', 0)),
                                # @d is mandatory (see [1, 5.3.9.6.2, Table 17, page 60])
                                'd': int(s.attrib['d']),
                                'r': r,
                start_number = source.get('startNumber')
                if start_number:
                    ms_info['start_number'] = int(start_number)
                timescale = source.get('timescale')
                if timescale:
                    ms_info['timescale'] = int(timescale)
                segment_duration = source.get('duration')
                if segment_duration:
                    ms_info['segment_duration'] = float(segment_duration)
            def extract_Initialization(source):
                initialization = source.find(_add_ns('Initialization'))
                if initialization is not None:
                    ms_info['initialization_url'] = initialization.attrib['sourceURL']
            segment_list = element.find(_add_ns('SegmentList'))
            if segment_list is not None:
                extract_common(segment_list)
                extract_Initialization(segment_list)
                segment_urls_e = segment_list.findall(_add_ns('SegmentURL'))
                if segment_urls_e:
                    ms_info['segment_urls'] = [segment.attrib['media'] for segment in segment_urls_e]
                segment_template = element.find(_add_ns('SegmentTemplate'))
                if segment_template is not None:
                    extract_common(segment_template)
                    media = segment_template.get('media')
                    if media:
                        ms_info['media'] = media
                    initialization = segment_template.get('initialization')
                    if initialization:
                        ms_info['initialization'] = initialization
                        extract_Initialization(segment_template)
            return ms_info
        mpd_duration = parse_duration(mpd_doc.get('mediaPresentationDuration'))
        stream_numbers = collections.defaultdict(int)
        for period_idx, period in enumerate(mpd_doc.findall(_add_ns('Period'))):
            period_entry = {
                'id': period.get('id', f'period-{period_idx}'),
                'formats': [],
                'subtitles': collections.defaultdict(list),
            period_duration = parse_duration(period.get('duration')) or mpd_duration
            period_ms_info = extract_multisegment_info(period, {
                'start_number': 1,
                'timescale': 1,
            for adaptation_set in period.findall(_add_ns('AdaptationSet')):
                adaption_set_ms_info = extract_multisegment_info(adaptation_set, period_ms_info)
                for representation in adaptation_set.findall(_add_ns('Representation')):
                    representation_attrib = adaptation_set.attrib.copy()
                    representation_attrib.update(representation.attrib)
                    # According to [1, 5.3.7.2, Table 9, page 41], @mimeType is mandatory
                    mime_type = representation_attrib['mimeType']
                    content_type = representation_attrib.get('contentType', mime_type.split('/')[0])
                    codec_str = representation_attrib.get('codecs', '')
                    # Some kind of binary subtitle found in some youtube livestreams
                    if mime_type == 'application/x-rawcc':
                        codecs = {'scodec': codec_str}
                        codecs = parse_codecs(codec_str)
                    if content_type not in ('video', 'audio', 'text'):
                        if mime_type in ('image/avif', 'image/jpeg'):
                            content_type = mime_type
                        elif codecs.get('vcodec', 'none') != 'none':
                            content_type = 'video'
                        elif codecs.get('acodec', 'none') != 'none':
                            content_type = 'audio'
                        elif codecs.get('scodec', 'none') != 'none':
                            content_type = 'text'
                        elif mimetype2ext(mime_type) in ('tt', 'dfxp', 'ttml', 'xml', 'json'):
                            self.report_warning(f'Unknown MIME type {mime_type} in DASH manifest')
                    base_url = ''
                    for element in (representation, adaptation_set, period, mpd_doc):
                        base_url_e = element.find(_add_ns('BaseURL'))
                        if try_call(lambda: base_url_e.text) is not None:
                            base_url = base_url_e.text + base_url
                            if re.match(r'https?://', base_url):
                    if mpd_base_url and base_url.startswith('/'):
                        base_url = urllib.parse.urljoin(mpd_base_url, base_url)
                    elif mpd_base_url and not re.match(r'https?://', base_url):
                        if not mpd_base_url.endswith('/'):
                            mpd_base_url += '/'
                        base_url = mpd_base_url + base_url
                    representation_id = representation_attrib.get('id')
                    lang = representation_attrib.get('lang')
                    url_el = representation.find(_add_ns('BaseURL'))
                    filesize = int_or_none(url_el.attrib.get('{http://youtube.com/yt/2012/10/10}contentLength') if url_el is not None else None)
                    bandwidth = int_or_none(representation_attrib.get('bandwidth'))
                    if representation_id is not None:
                        format_id = representation_id
                        format_id = content_type
                    if mpd_id:
                        format_id = mpd_id + '-' + format_id
                    if content_type in ('video', 'audio'):
                            'manifest_url': mpd_url,
                            'ext': mimetype2ext(mime_type),
                            'width': int_or_none(representation_attrib.get('width')),
                            'height': int_or_none(representation_attrib.get('height')),
                            'tbr': float_or_none(bandwidth, 1000),
                            'asr': int_or_none(representation_attrib.get('audioSamplingRate')),
                            'fps': int_or_none(representation_attrib.get('frameRate')),
                            'language': lang if lang not in ('mul', 'und', 'zxx', 'mis') else None,
                            'format_note': f'DASH {content_type}',
                            'container': mimetype2ext(mime_type) + '_dash',
                            **codecs,
                    elif content_type == 'text':
                    elif content_type in ('image/avif', 'image/jpeg'):
                        # See test case in VikiIE
                        # https://www.viki.com/videos/1175236v-choosing-spouse-by-lottery-episode-1
                            'ext': 'mhtml',
                            'format_note': f'DASH storyboards ({mimetype2ext(mime_type)})',
                    if is_drm_protected(adaptation_set) or is_drm_protected(representation):
                        f['has_drm'] = True
                    representation_ms_info = extract_multisegment_info(representation, adaption_set_ms_info)
                    def prepare_template(template_name, identifiers):
                        tmpl = representation_ms_info[template_name]
                            tmpl = tmpl.replace('$RepresentationID$', representation_id)
                        # First of, % characters outside $...$ templates
                        # must be escaped by doubling for proper processing
                        # by % operator string formatting used further (see
                        # https://github.com/ytdl-org/youtube-dl/issues/16867).
                        t = ''
                        in_template = False
                        for c in tmpl:
                            t += c
                            if c == '$':
                                in_template = not in_template
                            elif c == '%' and not in_template:
                        # Next, $...$ templates are translated to their
                        # %(...) counterparts to be used with % operator
                        t = re.sub(r'\$({})\$'.format('|'.join(identifiers)), r'%(\1)d', t)
                        t = re.sub(r'\$({})%([^$]+)\$'.format('|'.join(identifiers)), r'%(\1)\2', t)
                        t.replace('$$', '$')
                        return t
                    # @initialization is a regular template like @media one
                    # so it should be handled just the same way (see
                    # https://github.com/ytdl-org/youtube-dl/issues/11605)
                    if 'initialization' in representation_ms_info:
                        initialization_template = prepare_template(
                            'initialization',
                            # As per [1, 5.3.9.4.2, Table 15, page 54] $Number$ and
                            # $Time$ shall not be included for @initialization thus
                            # only $Bandwidth$ remains
                            ('Bandwidth', ))
                        representation_ms_info['initialization_url'] = initialization_template % {
                            'Bandwidth': bandwidth,
                    def location_key(location):
                        return 'url' if re.match(r'https?://', location) else 'path'
                    if 'segment_urls' not in representation_ms_info and 'media' in representation_ms_info:
                        media_template = prepare_template('media', ('Number', 'Bandwidth', 'Time'))
                        media_location_key = location_key(media_template)
                        # As per [1, 5.3.9.4.4, Table 16, page 55] $Number$ and $Time$
                        # can't be used at the same time
                        if '%(Number' in media_template and 's' not in representation_ms_info:
                            segment_duration = None
                            if 'total_number' not in representation_ms_info and 'segment_duration' in representation_ms_info:
                                segment_duration = float_or_none(representation_ms_info['segment_duration'], representation_ms_info['timescale'])
                                representation_ms_info['total_number'] = math.ceil(float_or_none(period_duration, segment_duration, default=0))
                            representation_ms_info['fragments'] = [{
                                media_location_key: media_template % {
                                    'Number': segment_number,
                                'duration': segment_duration,
                            } for segment_number in range(
                                representation_ms_info['start_number'],
                                representation_ms_info['total_number'] + representation_ms_info['start_number'])]
                            # $Number*$ or $Time$ in media template with S list available
                            # Example $Number*$: http://www.svtplay.se/klipp/9023742/stopptid-om-bjorn-borg
                            representation_ms_info['fragments'] = []
                            segment_time = 0
                            segment_d = None
                            segment_number = representation_ms_info['start_number']
                            def add_segment_url():
                                segment_url = media_template % {
                                    'Time': segment_time,
                                representation_ms_info['fragments'].append({
                                    media_location_key: segment_url,
                                    'duration': float_or_none(segment_d, representation_ms_info['timescale']),
                            for s in representation_ms_info['s']:
                                segment_time = s.get('t') or segment_time
                                segment_d = s['d']
                                add_segment_url()
                                segment_number += 1
                                for _ in range(s.get('r', 0)):
                                    segment_time += segment_d
                    elif 'segment_urls' in representation_ms_info and 's' in representation_ms_info:
                        # No media template,
                        # e.g. https://www.youtube.com/watch?v=iXZV5uAYMJI
                        # or any YouTube dashsegments video
                        fragments = []
                        segment_index = 0
                        timescale = representation_ms_info['timescale']
                            duration = float_or_none(s['d'], timescale)
                            for _ in range(s.get('r', 0) + 1):
                                segment_uri = representation_ms_info['segment_urls'][segment_index]
                                fragments.append({
                                    location_key(segment_uri): segment_uri,
                                segment_index += 1
                        representation_ms_info['fragments'] = fragments
                    elif 'segment_urls' in representation_ms_info:
                        # Segment URLs with no SegmentTimeline
                        # E.g. https://www.seznam.cz/zpravy/clanek/cesko-zasahne-vitr-o-sile-vichrice-muze-byt-i-zivotu-nebezpecny-39091
                        # https://github.com/ytdl-org/youtube-dl/pull/14844
                        segment_duration = float_or_none(
                            representation_ms_info['segment_duration'],
                            representation_ms_info['timescale']) if 'segment_duration' in representation_ms_info else None
                        for segment_url in representation_ms_info['segment_urls']:
                            fragment = {
                                location_key(segment_url): segment_url,
                                fragment['duration'] = segment_duration
                            fragments.append(fragment)
                    # If there is a fragments key available then we correctly recognized fragmented media.
                    # Otherwise we will assume unfragmented media with direct access. Technically, such
                    # assumption is not necessarily correct since we may simply have no support for
                    # some forms of fragmented media renditions yet, but for now we'll use this fallback.
                    if 'fragments' in representation_ms_info:
                            # NB: mpd_url may be empty when MPD manifest is parsed from a string
                            'url': mpd_url or base_url,
                            'fragment_base_url': base_url,
                            'fragments': [],
                            'protocol': 'mhtml' if mime_type in ('image/avif', 'image/jpeg') else 'http_dash_segments',
                        if 'initialization_url' in representation_ms_info:
                            initialization_url = representation_ms_info['initialization_url']
                            if not f.get('url'):
                                f['url'] = initialization_url
                            f['fragments'].append({location_key(initialization_url): initialization_url})
                        f['fragments'].extend(representation_ms_info['fragments'])
                        if not period_duration:
                            period_duration = try_get(
                                representation_ms_info,
                                lambda r: sum(frag['duration'] for frag in r['fragments']), float)
                        # Assuming direct URL to unfragmented media.
                        f['url'] = base_url
                    if content_type in ('video', 'audio', 'image/avif', 'image/jpeg'):
                        f['manifest_stream_number'] = stream_numbers[f['url']]
                        stream_numbers[f['url']] += 1
                        period_entry['formats'].append(f)
                        period_entry['subtitles'][lang or 'und'].append(f)
            yield period_entry
    def _extract_ism_formats(self, *args, **kwargs):
        fmts, subs = self._extract_ism_formats_and_subtitles(*args, **kwargs)
            self._report_ignoring_subs('ISM')
    def _extract_ism_formats_and_subtitles(self, ism_url, video_id, ism_id=None, note=None, errnote=None, fatal=True, data=None, headers={}, query={}):
            ism_url, video_id,
            note='Downloading ISM manifest' if note is None else note,
            errnote='Failed to download ISM manifest' if errnote is None else errnote,
        ism_doc, urlh = res
        if ism_doc is None:
        return self._parse_ism_formats_and_subtitles(ism_doc, urlh.url, ism_id)
    def _parse_ism_formats_and_subtitles(self, ism_doc, ism_url, ism_id=None):
        Parse formats from ISM manifest.
         1. [MS-SSTR]: Smooth Streaming Protocol,
            https://msdn.microsoft.com/en-us/library/ff469518.aspx
        if ism_doc.get('IsLive') == 'TRUE':
        duration = int(ism_doc.attrib['Duration'])
        timescale = int_or_none(ism_doc.get('TimeScale')) or 10000000
        for stream in ism_doc.findall('StreamIndex'):
            stream_type = stream.get('Type')
            if stream_type not in ('video', 'audio', 'text'):
            url_pattern = stream.attrib['Url']
            stream_timescale = int_or_none(stream.get('TimeScale')) or timescale
            stream_name = stream.get('Name')
            # IsmFD expects ISO 639 Set 2 language codes (3-character length)
            # See: https://github.com/yt-dlp/yt-dlp/issues/11356
            stream_language = stream.get('Language') or 'und'
            if len(stream_language) != 3:
                stream_language = ISO639Utils.short2long(stream_language) or 'und'
            for track in stream.findall('QualityLevel'):
                KNOWN_TAGS = {'255': 'AACL', '65534': 'EC-3'}
                fourcc = track.get('FourCC') or KNOWN_TAGS.get(track.get('AudioTag'))
                # TODO: add support for WVC1 and WMAP
                if fourcc not in ('H264', 'AVC1', 'AACL', 'TTML', 'EC-3'):
                    self.report_warning(f'{fourcc} is not a supported codec')
                tbr = int(track.attrib['Bitrate']) // 1000
                # [1] does not mention Width and Height attributes. However,
                # they're often present while MaxWidth and MaxHeight are
                # missing, so should be used as fallbacks
                width = int_or_none(track.get('MaxWidth') or track.get('Width'))
                height = int_or_none(track.get('MaxHeight') or track.get('Height'))
                sampling_rate = int_or_none(track.get('SamplingRate'))
                track_url_pattern = re.sub(r'{[Bb]itrate}', track.attrib['Bitrate'], url_pattern)
                track_url_pattern = urllib.parse.urljoin(ism_url, track_url_pattern)
                fragment_ctx = {
                    'time': 0,
                stream_fragments = stream.findall('c')
                for stream_fragment_index, stream_fragment in enumerate(stream_fragments):
                    fragment_ctx['time'] = int_or_none(stream_fragment.get('t')) or fragment_ctx['time']
                    fragment_repeat = int_or_none(stream_fragment.get('r')) or 1
                    fragment_ctx['duration'] = int_or_none(stream_fragment.get('d'))
                    if not fragment_ctx['duration']:
                            next_fragment_time = int(stream_fragment[stream_fragment_index + 1].attrib['t'])
                            next_fragment_time = duration
                        fragment_ctx['duration'] = (next_fragment_time - fragment_ctx['time']) / fragment_repeat
                    for _ in range(fragment_repeat):
                            'url': re.sub(r'{start[ _]time}', str(fragment_ctx['time']), track_url_pattern),
                            'duration': fragment_ctx['duration'] / stream_timescale,
                        fragment_ctx['time'] += fragment_ctx['duration']
                if stream_type == 'text':
                    subtitles.setdefault(stream_language, []).append({
                        'ext': 'ismt',
                        'protocol': 'ism',
                        'url': ism_url,
                        'manifest_url': ism_url,
                        'fragments': fragments,
                        '_download_params': {
                            'stream_type': stream_type,
                            'timescale': stream_timescale,
                            'fourcc': fourcc,
                            'language': stream_language,
                            'codec_private_data': track.get('CodecPrivateData'),
                elif stream_type in ('video', 'audio'):
                        'format_id': join_nonempty(ism_id, stream_name, tbr),
                        'ext': 'ismv' if stream_type == 'video' else 'isma',
                        'asr': sampling_rate,
                        'vcodec': 'none' if stream_type == 'audio' else fourcc,
                        'acodec': 'none' if stream_type == 'video' else fourcc,
                        'has_drm': ism_doc.find('Protection') is not None,
                        'audio_channels': int_or_none(track.get('Channels')),
                            'width': width or 0,
                            'height': height or 0,
                            'sampling_rate': sampling_rate,
                            'channels': int_or_none(track.get('Channels', 2)),
                            'bits_per_sample': int_or_none(track.get('BitsPerSample', 16)),
                            'nal_unit_length_field': int_or_none(track.get('NALUnitLengthField', 4)),
    def _parse_html5_media_entries(self, base_url, webpage, video_id, m3u8_id=None, m3u8_entry_protocol='m3u8_native', mpd_id=None, preference=None, quality=None, _headers=None):
        def absolute_url(item_url):
            return urljoin(base_url, item_url)
        def parse_content_type(content_type):
            if not content_type:
            ctr = re.search(r'(?P<mimetype>[^/]+/[^;]+)(?:;\s*codecs="?(?P<codecs>[^"]+))?', content_type)
            if ctr:
                mimetype, codecs = ctr.groups()
                f = parse_codecs(codecs)
                f['ext'] = mimetype2ext(mimetype)
                return f
        def _media_formats(src, cur_media_type, type_info=None):
            type_info = type_info or {}
            full_url = absolute_url(src)
            ext = type_info.get('ext') or determine_ext(full_url)
            if ext == 'm3u8':
                is_plain_url = False
                formats = self._extract_m3u8_formats(
                    full_url, video_id, ext='mp4',
                    entry_protocol=m3u8_entry_protocol, m3u8_id=m3u8_id,
                    preference=preference, quality=quality, fatal=False, headers=_headers)
            elif ext == 'mpd':
                formats = self._extract_mpd_formats(
                    full_url, video_id, mpd_id=mpd_id, fatal=False, headers=_headers)
                is_plain_url = True
                    'url': full_url,
                    'vcodec': 'none' if cur_media_type == 'audio' else None,
            return is_plain_url, formats
        # amp-video and amp-audio are very similar to their HTML5 counterparts
        # so we will include them right here (see
        # https://www.ampproject.org/docs/reference/components/amp-video)
        # For dl8-* tags see https://delight-vr.com/documentation/dl8-video/
        _MEDIA_TAG_NAME_RE = r'(?:(?:amp|dl8(?:-live)?)-)?(video|audio)'
        media_tags = [(media_tag, media_tag_name, media_type, '')
                      for media_tag, media_tag_name, media_type
                      in re.findall(rf'(?s)(<({_MEDIA_TAG_NAME_RE})[^>]*/>)', webpage)]
        media_tags.extend(re.findall(
            # We only allow video|audio followed by a whitespace or '>'.
            # Allowing more characters may end up in significant slow down (see
            # https://github.com/ytdl-org/youtube-dl/issues/11979,
            # e.g. http://www.porntrex.com/maps/videositemap.xml).
            rf'(?s)(<(?P<tag>{_MEDIA_TAG_NAME_RE})(?:\s+[^>]*)?>)(.*?)</(?P=tag)>', webpage))
        for media_tag, _, media_type, media_content in media_tags:
            media_info = {
                'subtitles': {},
            media_attributes = extract_attributes(media_tag)
            src = strip_or_none(dict_get(media_attributes, ('src', 'data-video-src', 'data-src', 'data-source')))
                f = parse_content_type(media_attributes.get('type'))
                _, formats = _media_formats(src, media_type, f)
                media_info['formats'].extend(formats)
            media_info['thumbnail'] = absolute_url(media_attributes.get('poster'))
            if media_content:
                for source_tag in re.findall(r'<source[^>]+>', media_content):
                    s_attr = extract_attributes(source_tag)
                    # data-video-src and data-src are non standard but seen
                    # several times in the wild
                    src = strip_or_none(dict_get(s_attr, ('src', 'data-video-src', 'data-src', 'data-source')))
                    if not src:
                    f = parse_content_type(s_attr.get('type'))
                    is_plain_url, formats = _media_formats(src, media_type, f)
                    if is_plain_url:
                        # width, height, res, label and title attributes are
                        # all not standard but seen several times in the wild
                        labels = [
                            s_attr.get(lbl)
                            for lbl in ('label', 'title')
                            if str_or_none(s_attr.get(lbl))
                        width = int_or_none(s_attr.get('width'))
                        height = (int_or_none(s_attr.get('height'))
                                  or int_or_none(s_attr.get('res')))
                        if not width or not height:
                            for lbl in labels:
                                resolution = parse_resolution(lbl)
                                if not resolution:
                                width = width or resolution.get('width')
                                height = height or resolution.get('height')
                            tbr = parse_bitrate(lbl)
                            if tbr:
                            tbr = None
                            'format_id': s_attr.get('label') or s_attr.get('title'),
                        f.update(formats[0])
                        media_info['formats'].append(f)
                for track_tag in re.findall(r'<track[^>]+>', media_content):
                    track_attributes = extract_attributes(track_tag)
                    kind = track_attributes.get('kind')
                    if not kind or kind in ('subtitles', 'captions'):
                        src = strip_or_none(track_attributes.get('src'))
                        lang = track_attributes.get('srclang') or track_attributes.get('lang') or track_attributes.get('label')
                        media_info['subtitles'].setdefault(lang, []).append({
                            'url': absolute_url(src),
            for f in media_info['formats']:
                f.setdefault('http_headers', {})['Referer'] = base_url
                if _headers:
                    f['http_headers'].update(_headers)
            if media_info['formats'] or media_info['subtitles']:
                entries.append(media_info)
    def _extract_akamai_formats(self, *args, **kwargs):
        fmts, subs = self._extract_akamai_formats_and_subtitles(*args, **kwargs)
            self._report_ignoring_subs('akamai')
    def _extract_akamai_formats_and_subtitles(self, manifest_url, video_id, hosts={}):
        signed = 'hdnea=' in manifest_url
        if not signed:
            # https://learn.akamai.com/en-us/webhelp/media-services-on-demand/stream-packaging-user-guide/GUID-BE6C0F73-1E06-483B-B0EA-57984B91B7F9.html
            manifest_url = re.sub(
                r'(?:b=[\d,-]+|(?:__a__|attributes)=off|__b__=\d+)&?',
                '', manifest_url).strip('?')
        hdcore_sign = 'hdcore=3.7.0'
        f4m_url = re.sub(r'(https?://[^/]+)/i/', r'\1/z/', manifest_url).replace('/master.m3u8', '/manifest.f4m')
        hds_host = hosts.get('hds')
        if hds_host:
            f4m_url = re.sub(r'(https?://)[^/]+', r'\1' + hds_host, f4m_url)
        if 'hdcore=' not in f4m_url:
            f4m_url += ('&' if '?' in f4m_url else '?') + hdcore_sign
            f4m_url, video_id, f4m_id='hds', fatal=False)
        for entry in f4m_formats:
            entry.update({'extra_param_to_segment_url': hdcore_sign})
        m3u8_url = re.sub(r'(https?://[^/]+)/z/', r'\1/i/', manifest_url).replace('/manifest.f4m', '/master.m3u8')
        hls_host = hosts.get('hls')
        if hls_host:
            m3u8_url = re.sub(r'(https?://)[^/]+', r'\1' + hls_host, m3u8_url)
        m3u8_formats, m3u8_subtitles = self._extract_m3u8_formats_and_subtitles(
            m3u8_url, video_id, 'mp4', 'm3u8_native',
            m3u8_id='hls', fatal=False)
        subtitles = self._merge_subtitles(subtitles, m3u8_subtitles)
        http_host = hosts.get('http')
        if http_host and m3u8_formats and not signed:
            REPL_REGEX = r'https?://[^/]+/i/([^,]+),([^/]+),([^/]+)\.csmil/.+'
            qualities = re.match(REPL_REGEX, m3u8_url).group(2).split(',')
            qualities_length = len(qualities)
            if len(m3u8_formats) in (qualities_length, qualities_length + 1):
                for f in m3u8_formats:
                    if f['vcodec'] != 'none':
                        for protocol in ('http', 'https'):
                            http_url = re.sub(
                                REPL_REGEX, protocol + fr'://{http_host}/\g<1>{qualities[i]}\3', f['url'])
                                'format_id': http_f['format_id'].replace('hls-', protocol + '-'),
                                'url': http_url,
                                'protocol': protocol,
    def _extract_wowza_formats(self, url, video_id, m3u8_entry_protocol='m3u8_native', skip_protocols=[]):
        query = urllib.parse.urlparse(url).query
        url = re.sub(r'/(?:manifest|playlist|jwplayer)\.(?:m3u8|f4m|mpd|smil)', '', url)
            r'(?:(?:http|rtmp|rtsp)(?P<s>s)?:)?(?P<url>//[^?]+)', url)
        url_base = mobj.group('url')
        http_base_url = '{}{}:{}'.format('http', mobj.group('s') or '', url_base)
        def manifest_url(manifest):
            m_url = f'{http_base_url}/{manifest}'
                m_url += f'?{query}'
            return m_url
        if 'm3u8' not in skip_protocols:
                manifest_url('playlist.m3u8'), video_id, 'mp4',
                m3u8_entry_protocol, m3u8_id='hls', fatal=False))
        if 'f4m' not in skip_protocols:
            formats.extend(self._extract_f4m_formats(
                manifest_url('manifest.f4m'),
                video_id, f4m_id='hds', fatal=False))
        if 'dash' not in skip_protocols:
            formats.extend(self._extract_mpd_formats(
                manifest_url('manifest.mpd'),
                video_id, mpd_id='dash', fatal=False))
        if re.search(r'(?:/smil:|\.smil)', url_base):
            if 'smil' not in skip_protocols:
                rtmp_formats = self._extract_smil_formats(
                    manifest_url('jwplayer.smil'),
                    video_id, fatal=False)
                for rtmp_format in rtmp_formats:
                    rtsp_format = rtmp_format.copy()
                    rtsp_format['url'] = '{}/{}'.format(rtmp_format['url'], rtmp_format['play_path'])
                    del rtsp_format['play_path']
                    del rtsp_format['ext']
                    rtsp_format.update({
                        'url': rtsp_format['url'].replace('rtmp://', 'rtsp://'),
                        'format_id': rtmp_format['format_id'].replace('rtmp', 'rtsp'),
                        'protocol': 'rtsp',
                    formats.extend([rtmp_format, rtsp_format])
            for protocol in ('rtmp', 'rtsp'):
                if protocol not in skip_protocols:
                        'url': f'{protocol}:{url_base}',
                        'format_id': protocol,
    def _find_jwplayer_data(self, webpage, video_id=None, transform_source=js_to_json):
            r'''(?<!-)\bjwplayer\s*\(\s*(?P<q>'|")(?!(?P=q)).+(?P=q)\s*\)(?:(?!</script>).)*?\.\s*(?:setup\s*\(|(?P<load>load)\s*\(\s*\[)''',
            webpage, 'JWPlayer data', video_id,
            # must be a {...} or sequence, ending
            contains_pattern=r'\{(?s:.*)}(?(load)(?:\s*,\s*\{(?s:.*)})*)', end_pattern=r'(?(load)\]|\))',
            transform_source=transform_source, default=None)
    def _extract_jwplayer_data(self, webpage, video_id, *args, transform_source=js_to_json, **kwargs):
        jwplayer_data = self._find_jwplayer_data(
            webpage, video_id, transform_source=transform_source)
        return self._parse_jwplayer_data(
            jwplayer_data, video_id, *args, **kwargs)
    def _parse_jwplayer_data(self, jwplayer_data, video_id=None, require_title=True,
                             m3u8_id=None, mpd_id=None, rtmp_params=None, base_url=None):
        if not isinstance(jwplayer_data, dict):
        playlist_items = jwplayer_data.get('playlist')
        # JWPlayer backward compatibility: single playlist item/flattened playlists
        # https://github.com/jwplayer/jwplayer/blob/v7.7.0/src/js/playlist/playlist.js#L10
        # https://github.com/jwplayer/jwplayer/blob/v7.4.3/src/js/api/config.js#L81-L96
        if not isinstance(playlist_items, list):
            playlist_items = (playlist_items or jwplayer_data, )
        for video_data in playlist_items:
            if not isinstance(video_data, dict):
            # JWPlayer backward compatibility: flattened sources
            # https://github.com/jwplayer/jwplayer/blob/v7.4.3/src/js/playlist/item.js#L29-L35
            if 'sources' not in video_data:
                video_data['sources'] = [video_data]
            this_video_id = video_id or video_data['mediaid']
            formats = self._parse_jwplayer_formats(
                video_data['sources'], video_id=this_video_id, m3u8_id=m3u8_id,
                mpd_id=mpd_id, rtmp_params=rtmp_params, base_url=base_url)
            for track in traverse_obj(video_data, (
                    'tracks', lambda _, v: v['kind'].lower() in ('captions', 'subtitles'))):
                track_url = urljoin(base_url, track.get('file'))
                if not track_url:
                subtitles.setdefault(track.get('label') or 'en', []).append({
                    'url': self._proto_relative_url(track_url),
            entry = {
                'id': this_video_id,
                'title': unescapeHTML(video_data['title'] if require_title else video_data.get('title')),
                'description': clean_html(video_data.get('description')),
                'thumbnail': urljoin(base_url, self._proto_relative_url(video_data.get('image'))),
                'timestamp': int_or_none(video_data.get('pubdate')),
                'duration': float_or_none(jwplayer_data.get('duration') or video_data.get('duration')),
                'alt_title': clean_html(video_data.get('subtitle')),  # attributes used e.g. by Tele5 ...
                'genre': clean_html(video_data.get('genre')),
                'channel': clean_html(dict_get(video_data, ('category', 'channel'))),
                'season_number': int_or_none(video_data.get('season')),
                'episode_number': int_or_none(video_data.get('episode')),
                'release_year': int_or_none(video_data.get('releasedate')),
                'age_limit': int_or_none(video_data.get('age_restriction')),
            # https://github.com/jwplayer/jwplayer/blob/master/src/js/utils/validator.js#L32
            if len(formats) == 1 and re.search(r'^(?:http|//).*(?:youtube\.com|youtu\.be)/.+', formats[0]['url']):
                entry.update({
                    '_type': 'url_transparent',
                    'url': formats[0]['url'],
                entry['formats'] = formats
            entries.append(entry)
        if len(entries) == 1:
            return entries[0]
            return self.playlist_result(entries)
    def _parse_jwplayer_formats(self, jwplayer_sources_data, video_id=None,
        urls = set()
        for source in jwplayer_sources_data:
            if not isinstance(source, dict):
            source_url = urljoin(
                base_url, self._proto_relative_url(source.get('file')))
            if not source_url or source_url in urls:
            urls.add(source_url)
            source_type = source.get('type') or ''
            ext = determine_ext(source_url, default_ext=mimetype2ext(source_type))
            if source_type == 'hls' or ext == 'm3u8' or 'format=m3u8-aapl' in source_url:
                    source_url, video_id, 'mp4', entry_protocol='m3u8_native',
                    m3u8_id=m3u8_id, fatal=False))
            elif source_type == 'dash' or ext == 'mpd' or 'format=mpd-time-csf' in source_url:
                    source_url, video_id, mpd_id=mpd_id, fatal=False))
            elif ext == 'smil':
                formats.extend(self._extract_smil_formats(
                    source_url, video_id, fatal=False))
            # https://github.com/jwplayer/jwplayer/blob/master/src/js/providers/default.js#L67
            elif source_type.startswith('audio') or ext in (
                    'oga', 'aac', 'mp3', 'mpeg', 'vorbis'):
                    'url': source_url,
                format_id = str_or_none(source.get('label'))
                height = int_or_none(source.get('height'))
                if height is None and format_id:
                    # Often no height is provided but there is a label in
                    # format like "1080p", "720p SD", or 1080.
                    height = parse_resolution(format_id).get('height')
                a_format = {
                    'width': int_or_none(source.get('width')),
                    'tbr': int_or_none(source.get('bitrate'), scale=1000),
                    'filesize': int_or_none(source.get('filesize')),
                if source_url.startswith('rtmp'):
                    a_format['ext'] = 'flv'
                    # See com/longtailvideo/jwplayer/media/RTMPMediaProvider.as
                    # of jwplayer.flash.swf
                    rtmp_url_parts = re.split(
                        r'((?:mp4|mp3|flv):)', source_url, maxsplit=1)
                    if len(rtmp_url_parts) == 3:
                        rtmp_url, prefix, play_path = rtmp_url_parts
                        a_format.update({
                            'url': rtmp_url,
                            'play_path': prefix + play_path,
                    if rtmp_params:
                        a_format.update(rtmp_params)
                formats.append(a_format)
    def _live_title(self, name):
        self._downloader.deprecation_warning('yt_dlp.InfoExtractor._live_title is deprecated and does not work as expected')
    def _int(self, v, name, fatal=False, **kwargs):
        res = int_or_none(v, **kwargs)
            msg = f'Failed to extract {name}: Could not parse value {v!r}'
                raise ExtractorError(msg)
    def _float(self, v, name, fatal=False, **kwargs):
        res = float_or_none(v, **kwargs)
    def _set_cookie(self, domain, name, value, expire_time=None, port=None,
                    path='/', secure=False, discard=False, rest={}, **kwargs):
        cookie = http.cookiejar.Cookie(
            0, name, value, port, port is not None, domain, True,
            domain.startswith('.'), path, True, secure, expire_time,
            discard, None, None, rest)
        self.cookiejar.set_cookie(cookie)
    def _get_cookies(self, url):
        """ Return a http.cookies.SimpleCookie with the cookies for the url """
        return LenientSimpleCookie(self._downloader.cookiejar.get_cookie_header(url))
    def _apply_first_set_cookie_header(self, url_handle, cookie):
        Apply first Set-Cookie header instead of the last. Experimental.
        Some sites (e.g. [1-3]) may serve two cookies under the same name
        in Set-Cookie header and expect the first (old) one to be set rather
        than second (new). However, as of RFC6265 the newer one cookie
        should be set into cookie store what actually happens.
        We will workaround this issue by resetting the cookie to
        the first one manually.
        1. https://new.vk.com/
        2. https://github.com/ytdl-org/youtube-dl/issues/9841#issuecomment-227871201
        3. https://learning.oreilly.com/
        for header, cookies in url_handle.headers.items():
            if header.lower() != 'set-cookie':
            cookies = cookies.encode('iso-8859-1').decode('utf-8')
            cookie_value = re.search(
                rf'{cookie}=(.+?);.*?\b[Dd]omain=(.+?)(?:[,;]|$)', cookies)
            if cookie_value:
                value, domain = cookie_value.groups()
                self._set_cookie(domain, cookie, value)
    def get_testcases(cls, include_onlymatching=False):
        # Do not look in super classes
        t = vars(cls).get('_TEST')
        if t:
            assert not hasattr(cls, '_TESTS'), f'{cls.ie_key()}IE has _TEST and _TESTS'
            tests = [t]
            tests = vars(cls).get('_TESTS', [])
        for t in tests:
            if not include_onlymatching and t.get('only_matching', False):
            t['name'] = cls.ie_key()
            yield t
        if getattr(cls, '__wrapped__', None):
            yield from cls.__wrapped__.get_testcases(include_onlymatching)
    def get_webpage_testcases(cls):
        tests = vars(cls).get('_WEBPAGE_TESTS', [])
            yield from cls.__wrapped__.get_webpage_testcases()
    def age_limit(cls):
        """Get age limit from the testcases"""
        return max(traverse_obj(
            (*cls.get_testcases(include_onlymatching=False), *cls.get_webpage_testcases()),
            (..., (('playlist', 0), None), 'info_dict', 'age_limit')) or [0])
    def _RETURN_TYPE(cls):
        """What the extractor returns: "video", "playlist", "any", or None (Unknown)"""
        tests = tuple(cls.get_testcases(include_onlymatching=False))
        if not tests:
        elif not any(k.startswith('playlist') for test in tests for k in test):
            return 'video'
        elif all(any(k.startswith('playlist') for k in test) for test in tests):
            return 'playlist'
        return 'any'
    def is_single_video(cls, url):
        """Returns whether the URL is of a single video, None if unknown"""
        if cls.suitable(url):
            return {'video': True, 'playlist': False}.get(cls._RETURN_TYPE)
    def is_suitable(cls, age_limit):
        """Test whether the extractor is generally suitable for the given age limit"""
        return not age_restricted(cls.age_limit, age_limit)
    def description(cls, *, markdown=True, search_examples=None):
        """Description of the extractor"""
        desc = ''
        if cls._NETRC_MACHINE:
            if markdown:
                desc += f' [*{cls._NETRC_MACHINE}*](## "netrc machine")'
                desc += f' [{cls._NETRC_MACHINE}]'
        if cls.IE_DESC is False:
            desc += ' [HIDDEN]'
        elif cls.IE_DESC:
            desc += f' {cls.IE_DESC}'
        if cls.SEARCH_KEY:
            desc += f'{";" if cls.IE_DESC else ""} "{cls.SEARCH_KEY}:" prefix'
            if search_examples:
                _COUNTS = ('', '5', '10', 'all')
                desc += f' (e.g. "{cls.SEARCH_KEY}{random.choice(_COUNTS)}:{random.choice(search_examples)}")'
        if not cls.working():
            desc += ' (**Currently broken**)' if markdown else ' (Currently broken)'
        # Escape emojis. Ref: https://github.com/github/markup/issues/1153
        name = (' - **{}**'.format(re.sub(r':(\w+:)', ':\u200B\\g<1>', cls.IE_NAME))) if markdown else cls.IE_NAME
        return f'{name}:{desc}' if desc else name
    def extract_subtitles(self, *args, **kwargs):
        if (self.get_param('writesubtitles', False)
                or self.get_param('listsubtitles')):
            return self._get_subtitles(*args, **kwargs)
    def _get_subtitles(self, *args, **kwargs):
    class CommentsDisabled(Exception):
        """Raise in _get_comments if comments are disabled for the video"""
    def extract_comments(self, *args, **kwargs):
        if not self.get_param('getcomments'):
        generator = self._get_comments(*args, **kwargs)
        def extractor():
            comments = []
            interrupted = True
                    comments.append(next(generator))
                interrupted = False
                self.to_screen('Interrupted by user')
            except self.CommentsDisabled:
                return {'comments': None, 'comment_count': None}
                if self.get_param('ignoreerrors') is not True:
                self._downloader.report_error(e)
            comment_count = len(comments)
            self.to_screen(f'Extracted {comment_count} comments')
                'comments': comments,
                'comment_count': None if interrupted else comment_count,
        return extractor
    def _get_comments(self, *args, **kwargs):
    def _merge_subtitle_items(subtitle_list1, subtitle_list2):
        """ Merge subtitle items for one language. Items with duplicated URLs/data
        will be dropped. """
        list1_data = {(item.get('url'), item.get('data')) for item in subtitle_list1}
        ret = list(subtitle_list1)
        ret.extend(item for item in subtitle_list2 if (item.get('url'), item.get('data')) not in list1_data)
    def _merge_subtitles(cls, *dicts, target=None):
        """ Merge subtitle dictionaries, language by language. """
        if target is None:
            target = {}
        for d in filter(None, dicts):
            for lang, subs in d.items():
                target[lang] = cls._merge_subtitle_items(target.get(lang, []), subs)
        return target
    def extract_automatic_captions(self, *args, **kwargs):
        if (self.get_param('writeautomaticsub', False)
            return self._get_automatic_captions(*args, **kwargs)
    def _get_automatic_captions(self, *args, **kwargs):
    @functools.cached_property
    def _cookies_passed(self):
        """Whether cookies have been passed to YoutubeDL"""
        return self.get_param('cookiefile') is not None or self.get_param('cookiesfrombrowser') is not None
    def mark_watched(self, *args, **kwargs):
        if not self.get_param('mark_watched', False):
        if (self.supports_login() and self._get_login_info()[0] is not None) or self._cookies_passed:
            self._mark_watched(*args, **kwargs)
    def _mark_watched(self, *args, **kwargs):
    def geo_verification_headers(self):
        geo_verification_proxy = self.get_param('geo_verification_proxy')
        if geo_verification_proxy:
            headers['Ytdl-request-proxy'] = geo_verification_proxy
    def _generic_id(url):
        return urllib.parse.unquote(os.path.splitext(url.rstrip('/').split('/')[-1])[0])
    def _generic_title(self, url='', webpage='', *, default=None):
        return (self._og_search_title(webpage, default=None)
                or self._html_extract_title(webpage, default=None)
                or urllib.parse.unquote(os.path.splitext(url_basename(url))[0])
                or default)
    def _extract_chapters_helper(self, chapter_list, start_function, title_function, duration, strict=True):
        if not duration:
        chapter_list = [{
            'start_time': start_function(chapter),
            'title': title_function(chapter),
        } for chapter in chapter_list or []]
        if strict:
            warn = self.report_warning
            warn = self.write_debug
            chapter_list.sort(key=lambda c: c['start_time'] or 0)
        chapters = [{'start_time': 0}]
        for idx, chapter in enumerate(chapter_list):
            if chapter['start_time'] is None:
                warn(f'Incomplete chapter {idx}')
            elif chapters[-1]['start_time'] <= chapter['start_time'] <= duration:
                chapters.append(chapter)
            elif chapter not in chapters:
                issue = (f'{chapter["start_time"]} > {duration}' if chapter['start_time'] > duration
                         else f'{chapter["start_time"]} < {chapters[-1]["start_time"]}')
                warn(f'Invalid start time ({issue}) for chapter "{chapter["title"]}"')
        return chapters[1:]
    def _extract_chapters_from_description(self, description, duration):
        duration_re = r'(?:\d+:)?\d{1,2}:\d{2}'
        sep_re = r'(?m)^\s*(%s)\b\W*\s(%s)\s*$'
        return self._extract_chapters_helper(
            re.findall(sep_re % (duration_re, r'.+?'), description or ''),
            start_function=lambda x: parse_duration(x[0]), title_function=lambda x: x[1],
            duration=duration, strict=False) or self._extract_chapters_helper(
            re.findall(sep_re % (r'.+?', duration_re), description or ''),
            start_function=lambda x: parse_duration(x[1]), title_function=lambda x: x[0],
            duration=duration, strict=False)
    def _availability(is_private=None, needs_premium=None, needs_subscription=None, needs_auth=None, is_unlisted=None):
        all_known = all(
            x is not None for x in
            (is_private, needs_premium, needs_subscription, needs_auth, is_unlisted))
            'private' if is_private
            else 'premium_only' if needs_premium
            else 'subscriber_only' if needs_subscription
            else 'needs_auth' if needs_auth
            else 'unlisted' if is_unlisted
            else 'public' if all_known
    def _configuration_arg(self, key, default=NO_DEFAULT, *, ie_key=None, casesense=False):
        @returns            A list of values for the extractor argument given by "key"
                            or "default" if no such key is present
        @param default      The default value to return when the key is not present (default: [])
        @param casesense    When false, the values are converted to lower case
        ie_key = ie_key if isinstance(ie_key, str) else (ie_key or self).ie_key()
        val = traverse_obj(self._downloader.params, ('extractor_args', ie_key.lower(), key))
            return [] if default is NO_DEFAULT else default
        return list(val) if casesense else [x.lower() for x in val]
    def _yes_playlist(self, playlist_id, video_id, smuggled_data=None, *, playlist_label='playlist', video_label='video'):
        if not playlist_id or not video_id:
            return not video_id
        no_playlist = (smuggled_data or {}).get('force_noplaylist')
        if no_playlist is not None:
            return not no_playlist
        video_id = '' if video_id is True else f' {video_id}'
        playlist_id = '' if playlist_id is True else f' {playlist_id}'
        if self.get_param('noplaylist'):
            self.to_screen(f'Downloading just the {video_label}{video_id} because of --no-playlist')
        self.to_screen(f'Downloading {playlist_label}{playlist_id} - add --no-playlist to download just the {video_label}{video_id}')
    def _error_or_warning(self, err, _count=None, _retries=0, *, fatal=True):
            err, _count or int(fatal), _retries,
            info=self.to_screen, warn=self.report_warning, error=None if fatal else self.report_warning,
            sleep_func=self.get_param('retry_sleep_functions', {}).get('extractor'))
    def RetryManager(self, **kwargs):
        return RetryManager(self.get_param('extractor_retries', 3), self._error_or_warning, **kwargs)
    def _extract_generic_embeds(self, url, *args, info_dict={}, note='Extracting generic embeds', **kwargs):
        display_id = traverse_obj(info_dict, 'display_id', 'id')
        self.to_screen(f'{format_field(display_id, None, "%s: ")}{note}')
        return self._downloader.get_info_extractor('Generic')._extract_embeds(
            smuggle_url(url, {'block_ies': [self.ie_key()]}), *args, **kwargs)
    def extract_from_webpage(cls, ydl, url, webpage):
        ie = (cls if isinstance(cls._extract_from_webpage, types.MethodType)
              else ydl.get_info_extractor(cls.ie_key()))
        for info in ie._extract_from_webpage(url, webpage) or []:
            # url = None since we do not want to set (webpage/original)_url
            ydl.add_default_extra_info(info, ie, None)
            yield info
    def _extract_from_webpage(cls, url, webpage):
        for embed_url in orderedSet(
                cls._extract_embed_urls(url, webpage) or [], lazy=True):
            yield cls.url_result(embed_url, None if cls._VALID_URL is False else cls)
    def _extract_embed_urls(cls, url, webpage):
        """@returns all the embed urls on the webpage"""
        if '_EMBED_URL_RE' not in cls.__dict__:
            assert isinstance(cls._EMBED_REGEX, (list, tuple))
            for idx, regex in enumerate(cls._EMBED_REGEX):
                assert regex.count('(?P<url>') == 1, \
                    f'{cls.__name__}._EMBED_REGEX[{idx}] must have exactly 1 url group\n\t{regex}'
            cls._EMBED_URL_RE = tuple(map(re.compile, cls._EMBED_REGEX))
        for regex in cls._EMBED_URL_RE:
            for mobj in regex.finditer(webpage):
                embed_url = urllib.parse.urljoin(url, unescapeHTML(mobj.group('url')))
                if cls._VALID_URL is False or cls.suitable(embed_url):
                    yield embed_url
    class StopExtraction(Exception):
    def _extract_url(cls, webpage):  # TODO: Remove
        """Only for compatibility with some older extractors"""
        return next(iter(cls._extract_embed_urls(None, webpage) or []), None)
    def __init_subclass__(cls, *, plugin_name=None, **kwargs):
        if plugin_name:
            mro = inspect.getmro(cls)
            next_mro_class = super_class = mro[mro.index(cls) + 1]
            while getattr(super_class, '__wrapped__', None):
                super_class = super_class.__wrapped__
            if not any(override.PLUGIN_NAME == plugin_name for override in plugin_ies_overrides.value[super_class]):
                cls.__wrapped__ = next_mro_class
                cls.PLUGIN_NAME, cls.ie_key = plugin_name, next_mro_class.ie_key
                cls.IE_NAME = f'{next_mro_class.IE_NAME}+{plugin_name}'
                setattr(sys.modules[super_class.__module__], super_class.__name__, cls)
                plugin_ies_overrides.value[super_class].append(cls)
        return super().__init_subclass__(**kwargs)
class SearchInfoExtractor(InfoExtractor):
    Base class for paged search queries extractors.
    They accept URLs in the format _SEARCH_KEY(|all|[0-9]):{query}
    Instances should define _SEARCH_KEY and optionally _MAX_RESULTS
    _MAX_RESULTS = float('inf')
    _RETURN_TYPE = 'playlist'
    def _VALID_URL(cls):
        return rf'{cls._SEARCH_KEY}(?P<prefix>|[1-9][0-9]*|all):(?P<query>[\s\S]+)'
    def _real_extract(self, query):
        prefix, query = self._match_valid_url(query).group('prefix', 'query')
        if prefix == '':
            return self._get_n_results(query, 1)
        elif prefix == 'all':
            return self._get_n_results(query, self._MAX_RESULTS)
            n = int(prefix)
            if n <= 0:
                raise ExtractorError(f'invalid download number {n} for query "{query}"')
            elif n > self._MAX_RESULTS:
                self.report_warning('%s returns max %i results (you requested %i)' % (self._SEARCH_KEY, self._MAX_RESULTS, n))
                n = self._MAX_RESULTS
            return self._get_n_results(query, n)
    def _get_n_results(self, query, n):
        """Get a specified number of results for a query.
        Either this function or _search_results must be overridden by subclasses """
        return self.playlist_result(
            itertools.islice(self._search_results(query), 0, None if n == float('inf') else n),
            query, query)
    def _search_results(self, query):
        """Returns an iterator of search results"""
    def SEARCH_KEY(cls):
        return cls._SEARCH_KEY
class UnsupportedURLIE(InfoExtractor):
    _VALID_URL = '.*'
    _ENABLED = False
    IE_DESC = False
        raise UnsupportedError(url)
import urllib.response
from collections.abc import Iterable, Mapping
from email.message import Message
from http import HTTPStatus
from types import NoneType
from ._helper import make_ssl_context, wrap_request_errors
from .exceptions import (
    NoSupportingHandlers,
    RequestError,
    UnsupportedRequest,
from ..cookies import YoutubeDLCookieJar
    error_to_str,
from ..utils.networking import HTTPHeaderDict, normalize_url
DEFAULT_TIMEOUT = 20
def register_preference(*handlers: type[RequestHandler]):
    assert all(issubclass(handler, RequestHandler) for handler in handlers)
    def outer(preference: Preference):
        @functools.wraps(preference)
        def inner(handler, *args, **kwargs):
            if not handlers or isinstance(handler, handlers):
                return preference(handler, *args, **kwargs)
        _RH_PREFERENCES.add(inner)
    return outer
class RequestDirector:
    """RequestDirector class
    Helper class that, when given a request, forward it to a RequestHandler that supports it.
    Preference functions in the form of func(handler, request) -> int
    can be registered into the `preferences` set. These are used to sort handlers
    in order of preference.
    @param logger: Logger instance.
    @param verbose: Print debug request information to stdout.
    def __init__(self, logger, verbose=False):
        self.handlers: dict[str, RequestHandler] = {}
        self.preferences: set[Preference] = set()
        self.logger = logger  # TODO(Grub4k): default logger
        self.verbose = verbose
        for handler in self.handlers.values():
            handler.close()
        self.handlers.clear()
    def add_handler(self, handler: RequestHandler):
        """Add a handler. If a handler of the same RH_KEY exists, it will overwrite it"""
        assert isinstance(handler, RequestHandler), 'handler must be a RequestHandler'
        self.handlers[handler.RH_KEY] = handler
    def _get_handlers(self, request: Request) -> list[RequestHandler]:
        """Sorts handlers by preference, given a request"""
        preferences = {
            rh: sum(pref(rh, request) for pref in self.preferences)
            for rh in self.handlers.values()
        self._print_verbose('Handler preferences for this request: {}'.format(', '.join(
            f'{rh.RH_NAME}={pref}' for rh, pref in preferences.items())))
        return sorted(self.handlers.values(), key=preferences.get, reverse=True)
    def _print_verbose(self, msg):
        if self.verbose:
            self.logger.stdout(f'director: {msg}')
    def send(self, request: Request) -> Response:
        Passes a request onto a suitable RequestHandler
        if not self.handlers:
            raise RequestError('No request handlers configured')
        assert isinstance(request, Request)
        unexpected_errors = []
        unsupported_errors = []
        for handler in self._get_handlers(request):
            self._print_verbose(f'Checking if "{handler.RH_NAME}" supports this request.')
                handler.validate(request)
            except UnsupportedRequest as e:
                self._print_verbose(
                    f'"{handler.RH_NAME}" cannot handle this request (reason: {error_to_str(e)})')
                unsupported_errors.append(e)
            self._print_verbose(f'Sending request via "{handler.RH_NAME}"')
                response = handler.send(request)
            except RequestError:
                self.logger.error(
                    f'[{handler.RH_NAME}] Unexpected error: {error_to_str(e)}{bug_reports_message()}',
                    is_error=False)
                unexpected_errors.append(e)
            assert isinstance(response, Response)
        raise NoSupportingHandlers(unsupported_errors, unexpected_errors)
_REQUEST_HANDLERS = {}
def register_rh(handler):
    """Register a RequestHandler class"""
    assert issubclass(handler, RequestHandler), f'{handler} must be a subclass of RequestHandler'
    assert handler.RH_KEY not in _REQUEST_HANDLERS, f'RequestHandler {handler.RH_KEY} already registered'
    _REQUEST_HANDLERS[handler.RH_KEY] = handler
    return handler
class Features(enum.Enum):
    ALL_PROXY = enum.auto()
    NO_PROXY = enum.auto()
class RequestHandler(abc.ABC):
    """Request Handler class
    Request handlers are class that, given a Request,
    process the request from start to finish and return a Response.
    Concrete subclasses need to redefine the _send(request) method,
    which handles the underlying request logic and returns a Response.
    RH_NAME class variable may contain a display name for the RequestHandler.
    By default, this is generated from the class name.
    The concrete request handler MUST have "RH" as the suffix in the class name.
    All exceptions raised by a RequestHandler should be an instance of RequestError.
    Any other exception raised will be treated as a handler issue.
    If a Request is not supported by the handler, an UnsupportedRequest
    should be raised with a reason.
    By default, some checks are done on the request in _validate() based on the following class variables:
    - `_SUPPORTED_URL_SCHEMES`: a tuple of supported url schemes.
        Any Request with an url scheme not in this list will raise an UnsupportedRequest.
    - `_SUPPORTED_PROXY_SCHEMES`: a tuple of support proxy url schemes. Any Request that contains
        a proxy url with an url scheme not in this list will raise an UnsupportedRequest.
    - `_SUPPORTED_FEATURES`: a tuple of supported features, as defined in Features enum.
    The above may be set to None to disable the checks.
    Parameters:
    @param logger: logger instance
    @param headers: HTTP Headers to include when sending requests.
    @param cookiejar: Cookiejar to use for requests.
    @param timeout: Socket timeout to use when sending requests.
    @param proxies: Proxies to use for sending requests.
    @param source_address: Client-side IP address to bind to for requests.
    @param verbose: Print debug request and traffic information to stdout.
    @param prefer_system_certs: Whether to prefer system certificates over other means (e.g. certifi).
    @param client_cert: SSL client certificate configuration.
            dict with {client_certificate, client_certificate_key, client_certificate_password}
    @param verify: Verify SSL certificates
    @param legacy_ssl_support: Enable legacy SSL options such as legacy server connect and older cipher support.
    Some configuration options may be available for individual Requests too. In this case,
    either the Request configuration option takes precedence or they are merged.
    Requests may have additional optional parameters defined as extensions.
     RequestHandler subclasses may choose to support custom extensions.
    If an extension is supported, subclasses should extend _check_extensions(extensions)
    to pop and validate the extension.
    - Extensions left in `extensions` are treated as unsupported and UnsupportedRequest will be raised.
    The following extensions are defined for RequestHandler:
    - `cookiejar`: Cookiejar to use for this request.
    - `timeout`: socket timeout to use for this request.
    - `legacy_ssl`: Enable legacy SSL options for this request. See legacy_ssl_support.
    - `keep_header_casing`: Keep the casing of headers when sending the request.
    To enable these, add extensions.pop('<extension>', None) to _check_extensions
    Apart from the url protocol, proxies dict may contain the following keys:
    - `all`: proxy to use for all protocols. Used as a fallback if no proxy is set for a specific protocol.
    - `no`: comma seperated list of hostnames (optionally with port) to not use a proxy for.
    Note: a RequestHandler may not support these, as defined in `_SUPPORTED_FEATURES`.
    _SUPPORTED_URL_SCHEMES = ()
    _SUPPORTED_PROXY_SCHEMES = ()
    _SUPPORTED_FEATURES = ()
        self, *,
        logger,  # TODO(Grub4k): default logger
        headers: HTTPHeaderDict = None,
        cookiejar: YoutubeDLCookieJar = None,
        timeout: float | int | None = None,
        proxies: dict | None = None,
        source_address: str | None = None,
        verbose: bool = False,
        prefer_system_certs: bool = False,
        client_cert: dict[str, str | None] | None = None,
        verify: bool = True,
        legacy_ssl_support: bool = False,
        **_,
        self._logger = logger
        self.cookiejar = cookiejar if cookiejar is not None else YoutubeDLCookieJar()
        self.timeout = float(timeout or DEFAULT_TIMEOUT)
        self.proxies = proxies or {}
        self.source_address = source_address
        self.prefer_system_certs = prefer_system_certs
        self._client_cert = client_cert or {}
        self.legacy_ssl_support = legacy_ssl_support
    def _make_sslcontext(self, legacy_ssl_support=None):
        return make_ssl_context(
            verify=self.verify,
            legacy_support=legacy_ssl_support if legacy_ssl_support is not None else self.legacy_ssl_support,
            use_certifi=not self.prefer_system_certs,
            **self._client_cert,
    def _merge_headers(self, request_headers):
        return HTTPHeaderDict(self.headers, request_headers)
    def _prepare_headers(self, request: Request, headers: HTTPHeaderDict) -> None:  # noqa: B027
        """Additional operations to prepare headers before building. To be extended by subclasses.
        @param request: Request object
        @param headers: Merged headers to prepare
    def _get_headers(self, request: Request) -> dict[str, str]:
        Get headers for external use.
        Subclasses may define a _prepare_headers method to modify headers after merge but before building.
        headers = self._merge_headers(request.headers)
        self._prepare_headers(request, headers)
        if request.extensions.get('keep_header_casing'):
            return headers.sensitive()
        return dict(headers)
    def _calculate_timeout(self, request):
        return float(request.extensions.get('timeout') or self.timeout)
    def _get_cookiejar(self, request):
        cookiejar = request.extensions.get('cookiejar')
        return self.cookiejar if cookiejar is None else cookiejar
    def _get_proxies(self, request):
        return (request.proxies or self.proxies).copy()
    def _check_url_scheme(self, request: Request):
        scheme = urllib.parse.urlparse(request.url).scheme.lower()
        if self._SUPPORTED_URL_SCHEMES is not None and scheme not in self._SUPPORTED_URL_SCHEMES:
            raise UnsupportedRequest(f'Unsupported url scheme: "{scheme}"')
        return scheme  # for further processing
    def _check_proxies(self, proxies):
        for proxy_key, proxy_url in proxies.items():
            if proxy_url is None:
            if proxy_key == 'no':
                if self._SUPPORTED_FEATURES is not None and Features.NO_PROXY not in self._SUPPORTED_FEATURES:
                    raise UnsupportedRequest('"no" proxy is not supported')
                proxy_key == 'all'
                and self._SUPPORTED_FEATURES is not None
                and Features.ALL_PROXY not in self._SUPPORTED_FEATURES
                raise UnsupportedRequest('"all" proxy is not supported')
            # Unlikely this handler will use this proxy, so ignore.
            # This is to allow a case where a proxy may be set for a protocol
            # for one handler in which such protocol (and proxy) is not supported by another handler.
            if self._SUPPORTED_URL_SCHEMES is not None and proxy_key not in (*self._SUPPORTED_URL_SCHEMES, 'all'):
            if self._SUPPORTED_PROXY_SCHEMES is None:
                # Skip proxy scheme checks
                if urllib.request._parse_proxy(proxy_url)[0] is None:
                    # Scheme-less proxies are not supported
                    raise UnsupportedRequest(f'Proxy "{proxy_url}" missing scheme')
                # parse_proxy may raise on some invalid proxy urls such as "/a/b/c"
                raise UnsupportedRequest(f'Invalid proxy url "{proxy_url}": {e}')
            scheme = urllib.parse.urlparse(proxy_url).scheme.lower()
            if scheme not in self._SUPPORTED_PROXY_SCHEMES:
                raise UnsupportedRequest(f'Unsupported proxy type: "{scheme}"')
    def _check_extensions(self, extensions):
        """Check extensions for unsupported extensions. Subclasses should extend this."""
        assert isinstance(extensions.get('cookiejar'), (YoutubeDLCookieJar, NoneType))
        assert isinstance(extensions.get('timeout'), (float, int, NoneType))
        assert isinstance(extensions.get('legacy_ssl'), (bool, NoneType))
        assert isinstance(extensions.get('keep_header_casing'), (bool, NoneType))
    def _validate(self, request):
        self._check_url_scheme(request)
        self._check_proxies(request.proxies or self.proxies)
        extensions = request.extensions.copy()
        self._check_extensions(extensions)
        if extensions:
            # TODO: add support for optional extensions
            raise UnsupportedRequest(f'Unsupported extensions: {", ".join(extensions.keys())}')
    @wrap_request_errors
    def validate(self, request: Request):
        if not isinstance(request, Request):
            raise TypeError('Expected an instance of Request')
        self._validate(request)
        return self._send(request)
    def _send(self, request: Request):
        """Handle a request from start to finish. Redefine in subclasses."""
    def close(self):  # noqa: B027
    def RH_NAME(cls):
    def RH_KEY(cls):
        assert cls.__name__.endswith('RH'), 'RequestHandler class names must end with "RH"'
class Request:
    Represents a request to be made.
    Partially backwards-compatible with urllib.request.Request.
    @param url: url to send. Will be sanitized.
    @param data: payload data to send. Must be bytes, iterable of bytes, a file-like object or None
    @param headers: headers to send.
    @param proxies: proxy dict mapping of proto:proxy to use for the request and any redirects.
    @param query: URL query parameters to update the url with.
    @param method: HTTP method to use. If no method specified, will use POST if payload data is present else GET
    @param extensions: Dictionary of Request extensions to add, as supported by handlers.
            url: str,
            data: RequestData = None,
            headers: typing.Mapping | None = None,
            query: dict | None = None,
            method: str | None = None,
            extensions: dict | None = None,
        self._headers = HTTPHeaderDict()
        self._data = None
            url = update_url_query(url, query)
        if headers:
        self.data = data  # note: must be done after setting headers
        self.extensions = extensions or {}
        return self._url
    def url(self, url):
        if not isinstance(url, str):
            raise TypeError('url must be a string')
        elif url.startswith('//'):
            url = 'http:' + url
        self._url = normalize_url(url)
        return self._method or ('POST' if self.data is not None else 'GET')
    def method(self, method):
        if method is None:
            self._method = None
        elif isinstance(method, str):
            self._method = method.upper()
            raise TypeError('method must be a string')
    def data(self):
    @data.setter
    def data(self, data: RequestData):
        # Try catch some common mistakes
        if data is not None and (
            not isinstance(data, (bytes, io.IOBase, Iterable)) or isinstance(data, (str, Mapping))
            raise TypeError('data must be bytes, iterable of bytes, or a file-like object')
        if data == self._data and self._data is None:
            self.headers.pop('Content-Length', None)
        # https://docs.python.org/3/library/urllib.request.html#urllib.request.Request.data
        if data != self._data:
        if self._data is None:
            self.headers.pop('Content-Type', None)
        if 'Content-Type' not in self.headers and self._data is not None:
            self.headers['Content-Type'] = 'application/x-www-form-urlencoded'
    def headers(self) -> HTTPHeaderDict:
    @headers.setter
    def headers(self, new_headers: Mapping):
        """Replaces headers of the request. If not a HTTPHeaderDict, it will be converted to one."""
        if isinstance(new_headers, HTTPHeaderDict):
            self._headers = new_headers
        elif isinstance(new_headers, Mapping):
            self._headers = HTTPHeaderDict(new_headers)
            raise TypeError('headers must be a mapping')
    def update(self, url=None, data=None, headers=None, query=None, extensions=None):
        self.data = data if data is not None else self.data
        self.headers.update(headers or {})
        self.extensions.update(extensions or {})
        self.url = update_url_query(url or self.url, query or {})
    def copy(self):
            url=self.url,
            headers=copy.deepcopy(self.headers),
            proxies=copy.deepcopy(self.proxies),
            data=self._data,
            extensions=copy.copy(self.extensions),
            method=self._method,
HEADRequest = functools.partial(Request, method='HEAD')
PATCHRequest = functools.partial(Request, method='PATCH')
PUTRequest = functools.partial(Request, method='PUT')
class Response(io.IOBase):
    Base class for HTTP response adapters.
    By default, it provides a basic wrapper for a file-like response object.
    Interface partially backwards-compatible with addinfourl and http.client.HTTPResponse.
    @param fp: Original, file-like, response.
    @param url: URL that this is a response of.
    @param headers: response headers.
    @param status: Response HTTP status code. Default is 200 OK.
    @param reason: HTTP status reason. Will use built-in reasons based on status code if not provided.
    @param extensions: Dictionary of handler-specific response extensions.
            fp: io.IOBase,
            headers: Mapping[str, str],
            status: int = 200,
            reason: str | None = None,
        self.fp = fp
        self.headers = Message()
        for name, value in headers.items():
            self.headers.add_header(name, value)
            self.reason = reason or HTTPStatus(status).phrase
            self.reason = None
        return self.fp.readable()
    def read(self, amt: int | None = None) -> bytes:
        # Expected errors raised here should be of type RequestError or subclasses.
        # Subclasses should redefine this method with more precise error handling.
            res = self.fp.read(amt)
            if self.fp.closed:
            raise TransportError(cause=e) from e
        if not self.fp.closed:
            self.fp.close()
        return super().close()
    def get_header(self, name, default=None):
        """Get header for name.
        If there are multiple matching headers, return all seperated by comma."""
        headers = self.headers.get_all(name)
        if not headers:
        if name.title() == 'Set-Cookie':
            # Special case, only get the first one
            # https://www.rfc-editor.org/rfc/rfc9110.html#section-5.3-4.1
            return headers[0]
        return ', '.join(headers)
    # The following methods are for compatability reasons and are deprecated
        deprecation_warning('Response.code is deprecated, use Response.status', stacklevel=2)
        return self.status
    def getcode(self):
        deprecation_warning('Response.getcode() is deprecated, use Response.status', stacklevel=2)
    def geturl(self):
        deprecation_warning('Response.geturl() is deprecated, use Response.url', stacklevel=2)
        return self.url
    def info(self):
        deprecation_warning('Response.info() is deprecated, use Response.headers', stacklevel=2)
        return self.headers
    def getheader(self, name, default=None):
        deprecation_warning('Response.getheader() is deprecated, use Response.get_header', stacklevel=2)
        return self.get_header(name, default)
    RequestData = bytes | Iterable[bytes] | typing.IO | None
    Preference = typing.Callable[[RequestHandler, Request], int]
_RH_PREFERENCES: set[Preference] = set()
from ..networking.exceptions import HTTPError, network_exceptions
    PostProcessingError,
    _configuration_args,
class PostProcessorMetaClass(type):
    def run_wrapper(func):
        def run(self, info, *args, **kwargs):
            info_copy = self._copy_infodict(info)
            self._hook_progress({'status': 'started'}, info_copy)
            ret = func(self, info, *args, **kwargs)
            if ret is not None:
                _, info = ret
            self._hook_progress({'status': 'finished'}, info_copy)
    def __new__(cls, name, bases, attrs):
        if 'run' in attrs:
            attrs['run'] = cls.run_wrapper(attrs['run'])
        return type.__new__(cls, name, bases, attrs)
class PostProcessor(metaclass=PostProcessorMetaClass):
    """Post Processor class.
    PostProcessor objects can be added to downloaders with their
    add_post_processor() method. When the downloader has finished a
    successful download, it will take its internal chain of PostProcessors
    and start calling the run() method on each one of them, first with
    an initial argument and then with the returned value of the previous
    PostProcessor.
    PostProcessor objects follow a "mutual registration" process similar
    to InfoExtractor objects.
    Optionally PostProcessor can use a list of additional command-line arguments
    with self._configuration_args.
        self.PP_NAME = self.pp_key()
    def pp_key(cls):
        name = cls.__name__[:-2]
        return name[6:] if name[:6].lower() == 'ffmpeg' else name
    def to_screen(self, text, prefix=True, *args, **kwargs):
            tag = f'[{self.PP_NAME}] ' if prefix else ''
            return self._downloader.to_screen(f'{tag}{text}', *args, **kwargs)
    def report_warning(self, text, *args, **kwargs):
            return self._downloader.report_warning(text, *args, **kwargs)
    def deprecation_warning(self, msg):
        warn = getattr(self._downloader, 'deprecation_warning', deprecation_warning)
        return warn(msg, stacklevel=1)
    def deprecated_feature(self, msg):
            return self._downloader.deprecated_feature(msg)
        return deprecation_warning(msg, stacklevel=1)
    def report_error(self, text, *args, **kwargs):
        self.deprecation_warning('"yt_dlp.postprocessor.PostProcessor.report_error" is deprecated. '
                                 'raise "yt_dlp.utils.PostProcessingError" instead')
            return self._downloader.report_error(text, *args, **kwargs)
    def write_debug(self, text, *args, **kwargs):
            return self._downloader.write_debug(text, *args, **kwargs)
    def _delete_downloaded_files(self, *files_to_delete, **kwargs):
            return self._downloader._delete_downloaded_files(*files_to_delete, **kwargs)
        for filename in set(filter(None, files_to_delete)):
        """Sets the downloader for this PP."""
        for ph in getattr(downloader, '_postprocessor_hooks', []):
            self.add_progress_hook(ph)
    def _copy_infodict(self, info_dict):
        return getattr(self._downloader, '_copy_infodict', dict)(info_dict)
    def _restrict_to(*, video=True, audio=True, images=True, simulated=True):
        allowed = {'video': video, 'audio': audio, 'images': images}
        def decorator(func):
            def wrapper(self, info):
                if not simulated and (self.get_param('simulate') or self.get_param('skip_download')):
                    return [], info
                format_type = (
                    'video' if info.get('vcodec') != 'none'
                    else 'audio' if info.get('acodec') != 'none'
                    else 'images')
                if allowed[format_type]:
                    return func(self, info)
                    self.to_screen(f'Skipping {format_type}')
    def run(self, information):
        """Run the PostProcessor.
        The "information" argument is a dictionary like the ones
        composed by InfoExtractors. The only difference is that this
        one has an extra field called "filepath" that points to the
        downloaded file.
        This method returns a tuple, the first element is a list of the files
        that can be deleted, and the second of which is the updated
        In addition, this method may raise a PostProcessingError
        exception if post processing fails.
        return [], information  # by default, keep file and do nothing
    def try_utime(self, path, atime, mtime, errnote='Cannot update utime of file'):
            os.utime(path, (atime, mtime))
            self.report_warning(errnote)
    def _configuration_args(self, exe, *args, **kwargs):
        return _configuration_args(
            self.pp_key(), self.get_param('postprocessor_args'), exe, *args, **kwargs)
        if not self._progress_hooks:
        status.update({
            'info_dict': info_dict,
            'postprocessor': self.pp_key(),
        # See YoutubeDl.py (search for postprocessor_hooks) for a description of this interface
        s['_default_template'] = '%(postprocessor)s %(status)s' % s  # noqa: UP031
        if not self._downloader:
        progress_template = self.get_param('progress_template', {})
        tmpl = progress_template.get('postprocess')
        if tmpl:
            self._downloader.to_screen(
                self._downloader.evaluate_outtmpl(tmpl, progress_dict), quiet=False)
        self._downloader.to_console_title(self._downloader.evaluate_outtmpl(
            progress_template.get('postprocess-title') or 'yt-dlp %(progress._default_template)s',
    def _retry_download(self, err, count, retries):
        # While this is not an extractor, it behaves similar to one and
        # so obey extractor_retries and "--retry-sleep extractor"
        RetryManager.report_retry(err, count, retries, info=self.to_screen, warn=self.report_warning,
    def _download_json(self, url, *, expected_http_errors=(404,)):
        self.write_debug(f'{self.PP_NAME} query: {url}')
        for retry in RetryManager(self.get_param('extractor_retries', 3), self._retry_download):
                rsp = self._downloader.urlopen(Request(url))
            except network_exceptions as e:
                if isinstance(e, HTTPError) and e.status in expected_http_errors:
                retry.error = PostProcessingError(f'Unable to communicate with {self.PP_NAME} API: {e}')
        return json.loads(rsp.read().decode(rsp.headers.get_param('charset') or 'utf-8'))
class AudioConversionError(PostProcessingError):  # Deprecated
from django.core.mail import mail_managers
from django.http import HttpResponsePermanentRedirect
from django.urls import is_valid_path
from django.utils.http import escape_leading_slashes
class CommonMiddleware(MiddlewareMixin):
    "Common" middleware for taking care of some basic operations:
        - Forbid access to User-Agents in settings.DISALLOWED_USER_AGENTS
        - URL rewriting: Based on the APPEND_SLASH and PREPEND_WWW settings,
          append missing slashes and/or prepends missing "www."s.
            - If APPEND_SLASH is set and the initial URL doesn't end with a
              slash, and it is not found in urlpatterns, form a new URL by
              appending a slash at the end. If this new URL is found in
              urlpatterns, return an HTTP redirect to this new URL; otherwise
              process the initial URL as usual.
          This behavior can be customized by subclassing CommonMiddleware and
          overriding the response_redirect_class attribute.
    response_redirect_class = HttpResponsePermanentRedirect
        Check for denied User-Agents and rewrite the URL based on
        settings.APPEND_SLASH and settings.PREPEND_WWW
        # Check for denied User-Agents
        user_agent = request.META.get("HTTP_USER_AGENT")
            for user_agent_regex in settings.DISALLOWED_USER_AGENTS:
                if user_agent_regex.search(user_agent):
                    raise PermissionDenied("Forbidden user agent")
        # Check for a redirect based on settings.PREPEND_WWW
        if settings.PREPEND_WWW and host and not host.startswith("www."):
            # Check if we also need to append a slash so we can do it all
            # with a single redirect. (This check may be somewhat expensive,
            # so we only do it if we already know we're sending a redirect,
            # or in process_response if we get a 404.)
            if self.should_redirect_with_slash(request):
                path = self.get_full_path_with_slash(request)
                path = request.get_full_path()
            return self.response_redirect_class(f"{request.scheme}://www.{host}{path}")
    def should_redirect_with_slash(self, request):
        Return True if settings.APPEND_SLASH is True and appending a slash to
        the request path turns an invalid path into a valid one.
        if settings.APPEND_SLASH and not request.path_info.endswith("/"):
            urlconf = getattr(request, "urlconf", None)
            if not is_valid_path(request.path_info, urlconf):
                match = is_valid_path("%s/" % request.path_info, urlconf)
                    view = match.func
                    return getattr(view, "should_append_slash", True)
    def get_full_path_with_slash(self, request):
        Return the full path of the request with a trailing slash appended.
        Raise a RuntimeError if settings.DEBUG is True and request.method is
        DELETE, POST, PUT, or PATCH.
        new_path = request.get_full_path(force_append_slash=True)
        # Prevent construction of scheme relative urls.
        new_path = escape_leading_slashes(new_path)
        if settings.DEBUG and request.method in ("DELETE", "POST", "PUT", "PATCH"):
                "You called this URL via %(method)s, but the URL doesn't end "
                "in a slash and you have APPEND_SLASH set. Django can't "
                "redirect to the slash URL while maintaining %(method)s data. "
                "Change your form to point to %(url)s (note the trailing "
                "slash), or set APPEND_SLASH=False in your Django settings."
                    "method": request.method,
                    "url": request.get_host() + new_path,
        return new_path
        When the status code of the response is 404, it may redirect to a path
        with an appended slash if should_redirect_with_slash() returns True.
        # If the given URL is "Not Found", then check if we should redirect to
        # a path with a slash appended.
        if response.status_code == 404 and self.should_redirect_with_slash(request):
            response = self.response_redirect_class(
                self.get_full_path_with_slash(request)
        # Add the Content-Length header to non-streaming responses if not
        # already set.
        if not response.streaming and not response.has_header("Content-Length"):
            response.headers["Content-Length"] = str(len(response.content))
class BrokenLinkEmailsMiddleware(MiddlewareMixin):
        """Send broken link emails for relevant 404 NOT FOUND responses."""
        if response.status_code == 404 and not settings.DEBUG:
            domain = request.get_host()
            referer = request.META.get("HTTP_REFERER", "")
            if not self.is_ignorable_request(request, path, domain, referer):
                ua = request.META.get("HTTP_USER_AGENT", "<none>")
                ip = request.META.get("REMOTE_ADDR", "<none>")
                mail_managers(
                    "Broken %slink on %s"
                            "INTERNAL "
                            if self.is_internal_request(domain, referer)
                    "Referrer: %s\nRequested URL: %s\nUser agent: %s\n"
                    "IP address: %s\n" % (referer, path, ua, ip),
                    fail_silently=True,
    def is_internal_request(self, domain, referer):
        Return True if the referring URL is the same domain as the current
        # Different subdomains are treated as different domains.
        return bool(re.match("^https?://%s/" % re.escape(domain), referer))
    def is_ignorable_request(self, request, uri, domain, referer):
        Return True if the given request *shouldn't* notify the site managers
        according to project settings or in situations outlined by the inline
        comments.
        # The referer is empty.
        if not referer:
        # APPEND_SLASH is enabled and the referer is equal to the current URL
        # without a trailing slash indicating an internal redirect.
        if settings.APPEND_SLASH and uri.endswith("/") and referer == uri[:-1]:
        # A '?' in referer is identified as a search engine source.
        if not self.is_internal_request(domain, referer) and "?" in referer:
        # The referer is equal to the current URL, ignoring the scheme (assumed
        # to be a poorly implemented bot).
        parsed_referer = urlsplit(referer)
        if parsed_referer.netloc in ["", domain] and parsed_referer.path == uri:
        return any(pattern.search(uri) for pattern in settings.IGNORABLE_404_URLS)
def no_append_slash(view_func):
    Mark a view function as excluded from CommonMiddleware's APPEND_SLASH
    redirection.
    # view_func.should_append_slash = False would also work, but decorators are
    # nicer if they don't have side effects, so return a new function.
            return await view_func(request, *args, **kwargs)
            return view_func(request, *args, **kwargs)
    _view_wrapper.should_append_slash = False
import requests  # type: ignore[import-untyped]
from langchain_qdrant import SparseEmbeddings, SparseVector
def qdrant_running_locally() -> bool:
    """Check if Qdrant is running at http://localhost:6333."""
        response = requests.get("http://localhost:6333", timeout=10.0)
        return response_json.get("title") == "qdrant - vector search engine"
    except (requests.exceptions.ConnectionError, requests.exceptions.Timeout):
def assert_documents_equals(actual: list[Document], expected: list[Document]) -> None:  # type: ignore[no-untyped-def]
    assert len(actual) == len(expected)
    for actual_doc, expected_doc in zip(actual, expected, strict=False):
        assert actual_doc.page_content == expected_doc.page_content
        assert "_id" in actual_doc.metadata
        assert "_collection_name" in actual_doc.metadata
        actual_doc.metadata.pop("_id")
        actual_doc.metadata.pop("_collection_name")
        assert actual_doc.metadata == expected_doc.metadata
class ConsistentFakeEmbeddings(Embeddings):
    """Fake embeddings which remember all the texts seen so far to return consistent
    vectors for the same texts.
    def __init__(self, dimensionality: int = 10) -> None:
        self.known_texts: list[str] = []
        """Return consistent embeddings for each text seen so far."""
        out_vectors = []
        for text in texts:
            if text not in self.known_texts:
                self.known_texts.append(text)
            vector = [1.0] * (self.dimensionality - 1) + [
                float(self.known_texts.index(text))
            out_vectors.append(vector)
        return out_vectors
        """Return consistent embeddings for the text, if seen before, or a constant
        one if the text is unknown.
class ConsistentFakeSparseEmbeddings(SparseEmbeddings):
    """Fake sparse embeddings which remembers all the texts seen so far
    "to return consistent vectors for the same texts.
    def __init__(self, dimensionality: int = 25) -> None:
    def embed_documents(self, texts: list[str]) -> list[SparseVector]:
            index = self.known_texts.index(text)
            indices = [i + index for i in range(self.dimensionality)]
            values = [1.0] * (self.dimensionality - 1) + [float(index)]
            out_vectors.append(SparseVector(indices=indices, values=values))
    def embed_query(self, text: str) -> SparseVector:
# common.py
from .helpers import delimited_list, any_open_tag, any_close_tag
# some other useful expressions - using lower-case class name since we are really using this as a namespace
class pyparsing_common:
    """Here are some common low-level expressions that may be useful in
    jump-starting parser development:
    - numeric forms (:class:`integers<integer>`, :class:`reals<real>`,
      :class:`scientific notation<sci_real>`)
    - common :class:`programming identifiers<identifier>`
    - network addresses (:class:`MAC<mac_address>`,
      :class:`IPv4<ipv4_address>`, :class:`IPv6<ipv6_address>`)
    - ISO8601 :class:`dates<iso8601_date>` and
      :class:`datetime<iso8601_datetime>`
    - :class:`UUID<uuid>`
    - :class:`comma-separated list<comma_separated_list>`
    - :class:`url`
    Parse actions:
    - :class:`convertToInteger`
    - :class:`convertToFloat`
    - :class:`convertToDate`
    - :class:`convertToDatetime`
    - :class:`stripHTMLTags`
    - :class:`upcaseTokens`
    - :class:`downcaseTokens`
        pyparsing_common.number.runTests('''
            # any int or real number, returned as the appropriate type
            100
            -100
            +100
            3.14159
            6.02e23
            1e-12
        pyparsing_common.fnumber.runTests('''
            # any int or real number, returned as float
        pyparsing_common.hex_integer.runTests('''
            # hex numbers
            FF
        pyparsing_common.fraction.runTests('''
            # fractions
            1/2
            -3/4
        pyparsing_common.mixed_integer.runTests('''
            # mixed fractions
            1-3/4
        pyparsing_common.uuid.setParseAction(tokenMap(uuid.UUID))
        pyparsing_common.uuid.runTests('''
            12345678-1234-5678-1234-567812345678
    prints::
        [100]
        [-100]
        [3.14159]
        [6.02e+23]
        [1e-12]
        [100.0]
        [-100.0]
        [256]
        [255]
        [0.5]
        [-0.75]
        [1]
        [1.75]
        [UUID('12345678-1234-5678-1234-567812345678')]
    convert_to_integer = token_map(int)
    Parse action for converting parsed integers to Python int
    convert_to_float = token_map(float)
    Parse action for converting parsed numbers to Python float
    integer = Word(nums).set_name("integer").set_parse_action(convert_to_integer)
    """expression that parses an unsigned integer, returns an int"""
    hex_integer = (
        Word(hexnums).set_name("hex integer").set_parse_action(token_map(int, 16))
    """expression that parses a hexadecimal integer, returns an int"""
    signed_integer = (
        Regex(r"[+-]?\d+")
        .set_name("signed integer")
        .set_parse_action(convert_to_integer)
    """expression that parses an integer with optional leading sign, returns an int"""
    fraction = (
        signed_integer().set_parse_action(convert_to_float)
        + "/"
        + signed_integer().set_parse_action(convert_to_float)
    ).set_name("fraction")
    """fractional expression of an integer divided by an integer, returns a float"""
    fraction.add_parse_action(lambda tt: tt[0] / tt[-1])
    mixed_integer = (
        fraction | signed_integer + Opt(Opt("-").suppress() + fraction)
    ).set_name("fraction or mixed integer-fraction")
    """mixed integer of the form 'integer - fraction', with optional leading integer, returns float"""
    mixed_integer.add_parse_action(sum)
    real = (
        Regex(r"[+-]?(?:\d+\.\d*|\.\d+)")
        .set_name("real number")
        .set_parse_action(convert_to_float)
    """expression that parses a floating point number and returns a float"""
    sci_real = (
        Regex(r"[+-]?(?:\d+(?:[eE][+-]?\d+)|(?:\d+\.\d*|\.\d+)(?:[eE][+-]?\d+)?)")
        .set_name("real number with scientific notation")
    """expression that parses a floating point number with optional
    scientific notation and returns a float"""
    # streamlining this expression makes the docs nicer-looking
    number = (sci_real | real | signed_integer).setName("number").streamline()
    """any numeric expression, returns the corresponding Python type"""
    fnumber = (
        Regex(r"[+-]?\d+\.?\d*([eE][+-]?\d+)?")
        .set_name("fnumber")
    """any int or real number, returned as float"""
    identifier = Word(identchars, identbodychars).set_name("identifier")
    """typical code identifier (leading alpha or '_', followed by 0 or more alphas, nums, or '_')"""
    ipv4_address = Regex(
        r"(25[0-5]|2[0-4][0-9]|1?[0-9]{1,2})(\.(25[0-5]|2[0-4][0-9]|1?[0-9]{1,2})){3}"
    ).set_name("IPv4 address")
    "IPv4 address (``0.0.0.0 - 255.255.255.255``)"
    _ipv6_part = Regex(r"[0-9a-fA-F]{1,4}").set_name("hex_integer")
    _full_ipv6_address = (_ipv6_part + (":" + _ipv6_part) * 7).set_name(
        "full IPv6 address"
    _short_ipv6_address = (
        Opt(_ipv6_part + (":" + _ipv6_part) * (0, 6))
        + "::"
        + Opt(_ipv6_part + (":" + _ipv6_part) * (0, 6))
    ).set_name("short IPv6 address")
    _short_ipv6_address.add_condition(
        lambda t: sum(1 for tt in t if pyparsing_common._ipv6_part.matches(tt)) < 8
    _mixed_ipv6_address = ("::ffff:" + ipv4_address).set_name("mixed IPv6 address")
    ipv6_address = Combine(
        (_full_ipv6_address | _mixed_ipv6_address | _short_ipv6_address).set_name(
            "IPv6 address"
    ).set_name("IPv6 address")
    "IPv6 address (long, short, or mixed form)"
    mac_address = Regex(
        r"[0-9a-fA-F]{2}([:.-])[0-9a-fA-F]{2}(?:\1[0-9a-fA-F]{2}){4}"
    ).set_name("MAC address")
    "MAC address xx:xx:xx:xx:xx (may also have '-' or '.' delimiters)"
    def convert_to_date(fmt: str = "%Y-%m-%d"):
        Helper to create a parse action for converting parsed date string to Python datetime.date
        Params -
        - fmt - format to be passed to datetime.strptime (default= ``"%Y-%m-%d"``)
            date_expr = pyparsing_common.iso8601_date.copy()
            date_expr.setParseAction(pyparsing_common.convertToDate())
            print(date_expr.parseString("1999-12-31"))
            [datetime.date(1999, 12, 31)]
        def cvt_fn(ss, ll, tt):
                return datetime.strptime(tt[0], fmt).date()
                raise ParseException(ss, ll, str(ve))
        return cvt_fn
    def convert_to_datetime(fmt: str = "%Y-%m-%dT%H:%M:%S.%f"):
        """Helper to create a parse action for converting parsed
        datetime string to Python datetime.datetime
        - fmt - format to be passed to datetime.strptime (default= ``"%Y-%m-%dT%H:%M:%S.%f"``)
            dt_expr = pyparsing_common.iso8601_datetime.copy()
            dt_expr.setParseAction(pyparsing_common.convertToDatetime())
            print(dt_expr.parseString("1999-12-31T23:59:59.999"))
            [datetime.datetime(1999, 12, 31, 23, 59, 59, 999000)]
        def cvt_fn(s, l, t):
                return datetime.strptime(t[0], fmt)
                raise ParseException(s, l, str(ve))
    iso8601_date = Regex(
        r"(?P<year>\d{4})(?:-(?P<month>\d\d)(?:-(?P<day>\d\d))?)?"
    ).set_name("ISO8601 date")
    "ISO8601 date (``yyyy-mm-dd``)"
    iso8601_datetime = Regex(
        r"(?P<year>\d{4})-(?P<month>\d\d)-(?P<day>\d\d)[T ](?P<hour>\d\d):(?P<minute>\d\d)(:(?P<second>\d\d(\.\d*)?)?)?(?P<tz>Z|[+-]\d\d:?\d\d)?"
    ).set_name("ISO8601 datetime")
    "ISO8601 datetime (``yyyy-mm-ddThh:mm:ss.s(Z|+-00:00)``) - trailing seconds, milliseconds, and timezone optional; accepts separating ``'T'`` or ``' '``"
    uuid = Regex(r"[0-9a-fA-F]{8}(-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12}").set_name("UUID")
    "UUID (``xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx``)"
    _html_stripper = any_open_tag.suppress() | any_close_tag.suppress()
    def strip_html_tags(s: str, l: int, tokens: ParseResults):
        """Parse action to remove HTML tags from web page HTML source
            # strip HTML links from normal text
            text = '<td>More info at the <a href="https://github.com/pyparsing/pyparsing/wiki">pyparsing</a> wiki page</td>'
            td, td_end = makeHTMLTags("TD")
            table_text = td + SkipTo(td_end).setParseAction(pyparsing_common.stripHTMLTags)("body") + td_end
            print(table_text.parseString(text).body)
        Prints::
            More info at the pyparsing wiki page
        return pyparsing_common._html_stripper.transform_string(tokens[0])
    _commasepitem = (
        Combine(
            OneOrMore(
                ~Literal(",")
                + ~LineEnd()
                + Word(printables, exclude_chars=",")
                + Opt(White(" \t") + ~FollowedBy(LineEnd() | ","))
        .streamline()
        .set_name("commaItem")
    comma_separated_list = delimited_list(
        Opt(quoted_string.copy() | _commasepitem, default="")
    ).set_name("comma separated list")
    """Predefined expression of 1 or more printable words or quoted strings, separated by commas."""
    upcase_tokens = staticmethod(token_map(lambda t: t.upper()))
    """Parse action to convert tokens to upper case."""
    downcase_tokens = staticmethod(token_map(lambda t: t.lower()))
    """Parse action to convert tokens to lower case."""
    # fmt: off
    url = Regex(
        # https://mathiasbynens.be/demo/url-regex
        # https://gist.github.com/dperini/729294
        r"^" +
        # protocol identifier (optional)
        # short syntax // still required
        r"(?:(?:(?P<scheme>https?|ftp):)?\/\/)" +
        # user:pass BasicAuth (optional)
        r"(?:(?P<auth>\S+(?::\S*)?)@)?" +
        r"(?P<host>" +
        # IP address exclusion
        # private & local networks
        r"(?!(?:10|127)(?:\.\d{1,3}){3})" +
        r"(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})" +
        r"(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})" +
        # IP address dotted notation octets
        # excludes loopback network 0.0.0.0
        # excludes reserved space >= 224.0.0.0
        # excludes network & broadcast addresses
        # (first & last IP address of each class)
        r"(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])" +
        r"(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}" +
        r"(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))" +
        r"|" +
        # host & domain names, may end with dot
        # can be replaced by a shortest alternative
        # (?![-_])(?:[-\w\u00a1-\uffff]{0,63}[^-_]\.)+
        r"(?:" +
        r"[a-z0-9\u00a1-\uffff]" +
        r"[a-z0-9\u00a1-\uffff_-]{0,62}" +
        r")?" +
        r"[a-z0-9\u00a1-\uffff]\." +
        r")+" +
        # TLD identifier name, may end with dot
        r"(?:[a-z\u00a1-\uffff]{2,}\.?)" +
        r")" +
        # port number (optional)
        r"(:(?P<port>\d{2,5}))?" +
        # resource path (optional)
        r"(?P<path>\/[^?# ]*)?" +
        # query string (optional)
        r"(\?(?P<query>[^#]*))?" +
        # fragment (optional)
        r"(#(?P<fragment>\S*))?" +
        r"$"
    ).set_name("url")
    # fmt: on
    convertToInteger = convert_to_integer
    convertToFloat = convert_to_float
    convertToDate = convert_to_date
    convertToDatetime = convert_to_datetime
    stripHTMLTags = strip_html_tags
    upcaseTokens = upcase_tokens
    downcaseTokens = downcase_tokens
_builtin_exprs = [
    v for v in vars(pyparsing_common).values() if isinstance(v, ParserElement)
GLOB_EDGE_CASES_TESTS = {
    "argnames": ("path", "recursive", "maxdepth", "expected"),
    "argvalues": [
        ("fil?1", False, None, ["file1"]),
        ("fil?1", True, None, ["file1"]),
        ("file[1-2]", False, None, ["file1", "file2"]),
        ("file[1-2]", True, None, ["file1", "file2"]),
        ("*", False, None, ["file1", "file2"]),
                "file1",
                "file2",
                "subdir0/subfile1",
                "subdir0/subfile2",
                "subdir0/nesteddir/nestedfile",
                "subdir1/subfile1",
                "subdir1/subfile2",
                "subdir1/nesteddir/nestedfile",
        ("*", True, 1, ["file1", "file2"]),
        ("*1", False, None, ["file1"]),
            "*1",
        ("*1", True, 2, ["file1", "subdir1/subfile1", "subdir1/subfile2"]),
            "**",
        ("**", True, 1, ["file1", "file2"]),
        ("**/*1", False, None, ["file1", "subdir0/subfile1", "subdir1/subfile1"]),
            "**/*1",
        ("**/*1", True, 1, ["file1"]),
            ["file1", "subdir0/subfile1", "subdir1/subfile1", "subdir1/subfile2"],
        ("**/*1", False, 2, ["file1", "subdir0/subfile1", "subdir1/subfile1"]),
        ("**/subdir0", False, None, []),
        ("**/subdir0", True, None, ["subfile1", "subfile2", "nesteddir/nestedfile"]),
        ("**/subdir0/nested*", False, 2, []),
        ("**/subdir0/nested*", True, 2, ["nestedfile"]),
        ("subdir[1-2]", False, None, []),
        ("subdir[1-2]", True, None, ["subfile1", "subfile2", "nesteddir/nestedfile"]),
        ("subdir[1-2]", True, 2, ["subfile1", "subfile2"]),
        ("subdir[0-1]", False, None, []),
            "subdir[0-1]",
            "subdir[0-1]/*fil[e]*",
from scipy.sparse import find, coo_matrix
EPS = np.finfo(float).eps
def validate_first_step(first_step, t0, t_bound):
    """Assert that first_step is valid and return it."""
    if first_step <= 0:
        raise ValueError("`first_step` must be positive.")
    if first_step > np.abs(t_bound - t0):
        raise ValueError("`first_step` exceeds bounds.")
    return first_step
def validate_max_step(max_step):
    """Assert that max_Step is valid and return it."""
    if max_step <= 0:
        raise ValueError("`max_step` must be positive.")
    return max_step
def warn_extraneous(extraneous):
    """Display a warning for extraneous keyword arguments.
    The initializer of each solver class is expected to collect keyword
    arguments that it doesn't understand and warn about them. This function
    prints a warning for each key in the supplied dictionary.
    extraneous : dict
        Extraneous keyword arguments
    if extraneous:
        warn("The following arguments have no effect for a chosen solver: "
             f"{', '.join(f'`{x}`' for x in extraneous)}.",
             stacklevel=3)
def validate_tol(rtol, atol, n):
    """Validate tolerance values."""
    if np.any(rtol < 100 * EPS):
        warn("At least one element of `rtol` is too small. "
             f"Setting `rtol = np.maximum(rtol, {100 * EPS})`.",
        rtol = np.maximum(rtol, 100 * EPS)
    atol = np.asarray(atol)
    if atol.ndim > 0 and atol.shape != (n,):
        raise ValueError("`atol` has wrong shape.")
    if np.any(atol < 0):
        raise ValueError("`atol` must be positive.")
    return rtol, atol
def norm(x):
    """Compute RMS norm."""
    return np.linalg.norm(x) / x.size ** 0.5
def select_initial_step(fun, t0, y0, t_bound,
                        max_step, f0, direction, order, rtol, atol):
    """Empirically select a good initial step.
    The algorithm is described in [1]_.
        Right-hand side of the system.
        Initial value of the independent variable.
    y0 : ndarray, shape (n,)
        Initial value of the dependent variable.
        End-point of integration interval; used to ensure that t0+step<=tbound
        and that fun is only evaluated in the interval [t0,tbound]
    max_step : float
        Maximum allowable step size.
    f0 : ndarray, shape (n,)
        Initial value of the derivative, i.e., ``fun(t0, y0)``.
        Integration direction.
    order : float
        Error estimator order. It means that the error controlled by the
        algorithm is proportional to ``step_size ** (order + 1)`.
    rtol : float
        Desired relative tolerance.
    atol : float
        Desired absolute tolerance.
    h_abs : float
        Absolute value of the suggested initial step.
    .. [1] E. Hairer, S. P. Norsett G. Wanner, "Solving Ordinary Differential
           Equations I: Nonstiff Problems", Sec. II.4.
    if y0.size == 0:
        return np.inf
    interval_length = abs(t_bound - t0)
    if interval_length == 0.0:
    scale = atol + np.abs(y0) * rtol
    d0 = norm(y0 / scale)
    d1 = norm(f0 / scale)
    if d0 < 1e-5 or d1 < 1e-5:
        h0 = 1e-6
        h0 = 0.01 * d0 / d1
    # Check t0+h0*direction doesn't take us beyond t_bound
    h0 = min(h0, interval_length)
    y1 = y0 + h0 * direction * f0
    f1 = fun(t0 + h0 * direction, y1)
    d2 = norm((f1 - f0) / scale) / h0
    if d1 <= 1e-15 and d2 <= 1e-15:
        h1 = max(1e-6, h0 * 1e-3)
        h1 = (0.01 / max(d1, d2)) ** (1 / (order + 1))
    return min(100 * h0, h1, interval_length, max_step)
class OdeSolution:
    """Continuous ODE solution.
    It is organized as a collection of `DenseOutput` objects which represent
    local interpolants. It provides an algorithm to select a right interpolant
    for each given point.
    The interpolants cover the range between `t_min` and `t_max` (see
    Attributes below). Evaluation outside this interval is not forbidden, but
    the accuracy is not guaranteed.
    When evaluating at a breakpoint (one of the values in `ts`) a segment with
    the lower index is selected.
    ts : array_like, shape (n_segments + 1,)
        Time instants between which local interpolants are defined. Must
        be strictly increasing or decreasing (zero segment with two points is
        also allowed).
    interpolants : list of DenseOutput with n_segments elements
        Local interpolants. An i-th interpolant is assumed to be defined
        between ``ts[i]`` and ``ts[i + 1]``.
    alt_segment : boolean
        Requests the alternative interpolant segment selection scheme. At each
        solver integration point, two interpolant segments are available. The
        default (False) and alternative (True) behaviours select the segment
        for which the requested time corresponded to ``t`` and ``t_old``,
        respectively. This functionality is only relevant for testing the
        interpolants' accuracy: different integrators use different
        construction strategies.
    def __init__(self, ts, interpolants, alt_segment=False):
        ts = np.asarray(ts)
        d = np.diff(ts)
        # The first case covers integration on zero segment.
        if not ((ts.size == 2 and ts[0] == ts[-1])
                or np.all(d > 0) or np.all(d < 0)):
            raise ValueError("`ts` must be strictly increasing or decreasing.")
        self.n_segments = len(interpolants)
        if ts.shape != (self.n_segments + 1,):
            raise ValueError("Numbers of time stamps and interpolants "
                             "don't match.")
        self.ts = ts
        self.interpolants = interpolants
        if ts[-1] >= ts[0]:
            self.t_min = ts[0]
            self.t_max = ts[-1]
            self.ascending = True
            self.side = "right" if alt_segment else "left"
            self.ts_sorted = ts
            self.t_min = ts[-1]
            self.t_max = ts[0]
            self.ascending = False
            self.side = "left" if alt_segment else "right"
            self.ts_sorted = ts[::-1]
    def _call_single(self, t):
        # Here we preserve a certain symmetry that when t is in self.ts,
        # if alt_segment=False, then we prioritize a segment with a lower
        # index.
        ind = np.searchsorted(self.ts_sorted, t, side=self.side)
        segment = min(max(ind - 1, 0), self.n_segments - 1)
        if not self.ascending:
            segment = self.n_segments - 1 - segment
        return self.interpolants[segment](t)
        """Evaluate the solution.
            Points to evaluate at.
        y : ndarray, shape (n_states,) or (n_states, n_points)
            Computed values. Shape depends on whether `t` is a scalar or a
            return self._call_single(t)
        order = np.argsort(t)
        reverse = np.empty_like(order)
        reverse[order] = np.arange(order.shape[0])
        t_sorted = t[order]
        # See comment in self._call_single.
        segments = np.searchsorted(self.ts_sorted, t_sorted, side=self.side)
        segments -= 1
        segments[segments < 0] = 0
        segments[segments > self.n_segments - 1] = self.n_segments - 1
            segments = self.n_segments - 1 - segments
        ys = []
        group_start = 0
        for segment, group in groupby(segments):
            group_end = group_start + len(list(group))
            y = self.interpolants[segment](t_sorted[group_start:group_end])
            ys.append(y)
            group_start = group_end
        ys = np.hstack(ys)
        ys = ys[:, reverse]
        return ys
NUM_JAC_DIFF_REJECT = EPS ** 0.875
NUM_JAC_DIFF_SMALL = EPS ** 0.75
NUM_JAC_DIFF_BIG = EPS ** 0.25
NUM_JAC_MIN_FACTOR = 1e3 * EPS
NUM_JAC_FACTOR_INCREASE = 10
NUM_JAC_FACTOR_DECREASE = 0.1
def num_jac(fun, t, y, f, threshold, factor, sparsity=None):
    """Finite differences Jacobian approximation tailored for ODE solvers.
    This function computes finite difference approximation to the Jacobian
    matrix of `fun` with respect to `y` using forward differences.
    The Jacobian matrix has shape (n, n) and its element (i, j) is equal to
    ``d f_i / d y_j``.
    A special feature of this function is the ability to correct the step
    size from iteration to iteration. The main idea is to keep the finite
    difference significantly separated from its round-off error which
    approximately equals ``EPS * np.abs(f)``. It reduces a possibility of a
    huge error and assures that the estimated derivative are reasonably close
    to the true values (i.e., the finite difference approximation is at least
    qualitatively reflects the structure of the true Jacobian).
        Right-hand side of the system implemented in a vectorized fashion.
    y : ndarray, shape (n,)
    f : ndarray, shape (n,)
        Value of the right hand side at (t, y).
    threshold : float
        Threshold for `y` value used for computing the step size as
        ``factor * np.maximum(np.abs(y), threshold)``. Typically, the value of
        absolute tolerance (atol) for a solver should be passed as `threshold`.
    factor : ndarray with shape (n,) or None
        Factor to use for computing the step size. Pass None for the very
        evaluation, then use the value returned from this function.
    sparsity : tuple (structure, groups) or None
        Sparsity structure of the Jacobian, `structure` must be csc_matrix.
    J : ndarray or csc_matrix, shape (n, n)
        Jacobian matrix.
    factor : ndarray, shape (n,)
        Suggested `factor` for the next evaluation.
    n = y.shape[0]
        return np.empty((0, 0)), factor
    if factor is None:
        factor = np.full(n, EPS ** 0.5)
        factor = factor.copy()
    # Direct the step as ODE dictates, hoping that such a step won't lead to
    # a problematic region. For complex ODEs it makes sense to use the real
    # part of f as we use steps along real axis.
    f_sign = 2 * (np.real(f) >= 0).astype(float) - 1
    y_scale = f_sign * np.maximum(threshold, np.abs(y))
    h = (y + factor * y_scale) - y
    # Make sure that the step is not 0 to start with. Not likely it will be
    # executed often.
    for i in np.nonzero(h == 0)[0]:
        while h[i] == 0:
            factor[i] *= 10
            h[i] = (y[i] + factor[i] * y_scale[i]) - y[i]
    if sparsity is None:
        return _dense_num_jac(fun, t, y, f, h, factor, y_scale)
        structure, groups = sparsity
        return _sparse_num_jac(fun, t, y, f, h, factor, y_scale,
                               structure, groups)
def _dense_num_jac(fun, t, y, f, h, factor, y_scale):
    h_vecs = np.diag(h)
    f_new = fun(t, y[:, None] + h_vecs)
    diff = f_new - f[:, None]
    max_ind = np.argmax(np.abs(diff), axis=0)
    r = np.arange(n)
    max_diff = np.abs(diff[max_ind, r])
    scale = np.maximum(np.abs(f[max_ind]), np.abs(f_new[max_ind, r]))
    diff_too_small = max_diff < NUM_JAC_DIFF_REJECT * scale
    if np.any(diff_too_small):
        ind, = np.nonzero(diff_too_small)
        new_factor = NUM_JAC_FACTOR_INCREASE * factor[ind]
        h_new = (y[ind] + new_factor * y_scale[ind]) - y[ind]
        h_vecs[ind, ind] = h_new
        f_new = fun(t, y[:, None] + h_vecs[:, ind])
        diff_new = f_new - f[:, None]
        max_ind = np.argmax(np.abs(diff_new), axis=0)
        r = np.arange(ind.shape[0])
        max_diff_new = np.abs(diff_new[max_ind, r])
        scale_new = np.maximum(np.abs(f[max_ind]), np.abs(f_new[max_ind, r]))
        update = max_diff[ind] * scale_new < max_diff_new * scale[ind]
        if np.any(update):
            update, = np.nonzero(update)
            update_ind = ind[update]
            factor[update_ind] = new_factor[update]
            h[update_ind] = h_new[update]
            diff[:, update_ind] = diff_new[:, update]
            scale[update_ind] = scale_new[update]
            max_diff[update_ind] = max_diff_new[update]
    diff /= h
    factor[max_diff < NUM_JAC_DIFF_SMALL * scale] *= NUM_JAC_FACTOR_INCREASE
    factor[max_diff > NUM_JAC_DIFF_BIG * scale] *= NUM_JAC_FACTOR_DECREASE
    factor = np.maximum(factor, NUM_JAC_MIN_FACTOR)
    return diff, factor
def _sparse_num_jac(fun, t, y, f, h, factor, y_scale, structure, groups):
    n_groups = np.max(groups) + 1
    h_vecs = np.empty((n_groups, n))
    for group in range(n_groups):
        e = np.equal(group, groups)
        h_vecs[group] = h * e
    h_vecs = h_vecs.T
    df = f_new - f[:, None]
    i, j, _ = find(structure)
    diff = coo_matrix((df[i, groups[j]], (i, j)), shape=(n, n)).tocsc()
    max_ind = np.array(abs(diff).argmax(axis=0)).ravel()
    max_diff = np.asarray(np.abs(diff[max_ind, r])).ravel()
    scale = np.maximum(np.abs(f[max_ind]),
                       np.abs(f_new[max_ind, groups[r]]))
        h_new_all = np.zeros(n)
        h_new_all[ind] = h_new
        groups_unique = np.unique(groups[ind])
        groups_map = np.empty(n_groups, dtype=int)
        h_vecs = np.empty((groups_unique.shape[0], n))
        for k, group in enumerate(groups_unique):
            h_vecs[k] = h_new_all * e
            groups_map[group] = k
        i, j, _ = find(structure[:, ind])
        diff_new = coo_matrix((df[i, groups_map[groups[ind[j]]]],
                               (i, j)), shape=(n, ind.shape[0])).tocsc()
        max_ind_new = np.array(abs(diff_new).argmax(axis=0)).ravel()
        max_diff_new = np.asarray(np.abs(diff_new[max_ind_new, r])).ravel()
        scale_new = np.maximum(
            np.abs(f[max_ind_new]),
            np.abs(f_new[max_ind_new, groups_map[groups[ind]]]))
    diff.data /= np.repeat(h, np.diff(diff.indptr))
    "scipy.misc.common is deprecated and will be removed in 2.0.0",
"""Functions used by least-squares algorithms."""
from math import copysign
from numpy.linalg import norm
from scipy.linalg import cho_factor, cho_solve, LinAlgError
from scipy.sparse import issparse
from scipy.sparse.linalg import LinearOperator, aslinearoperator
# Functions related to a trust-region problem.
def intersect_trust_region(x, s, Delta):
    """Find the intersection of a line with the boundary of a trust region.
    This function solves the quadratic equation with respect to t
    ||(x + s*t)||**2 = Delta**2.
    t_neg, t_pos : tuple of float
        Negative and positive roots.
        If `s` is zero or `x` is not within the trust region.
    a = np.dot(s, s)
    if a == 0:
        raise ValueError("`s` is zero.")
    b = np.dot(x, s)
    c = np.dot(x, x) - Delta**2
    if c > 0:
        raise ValueError("`x` is not within the trust region.")
    d = np.sqrt(b*b - a*c)  # Root from one fourth of the discriminant.
    # Computations below avoid loss of significance, see "Numerical Recipes".
    q = -(b + copysign(d, b))
    t1 = q / a
    t2 = c / q
    if t1 < t2:
        return t1, t2
        return t2, t1
def solve_lsq_trust_region(n, m, uf, s, V, Delta, initial_alpha=None,
                           rtol=0.01, max_iter=10):
    """Solve a trust-region problem arising in least-squares minimization.
    This function implements a method described by J. J. More [1]_ and used
    in MINPACK, but it relies on a single SVD of Jacobian instead of series
    of Cholesky decompositions. Before running this function, compute:
    ``U, s, VT = svd(J, full_matrices=False)``.
        Number of variables.
    m : int
        Number of residuals.
    uf : ndarray
        Computed as U.T.dot(f).
    s : ndarray
        Singular values of J.
    V : ndarray
        Transpose of VT.
    Delta : float
        Radius of a trust region.
    initial_alpha : float, optional
        Initial guess for alpha, which might be available from a previous
        iteration. If None, determined automatically.
        Stopping tolerance for the root-finding procedure. Namely, the
        solution ``p`` will satisfy ``abs(norm(p) - Delta) < rtol * Delta``.
    max_iter : int, optional
        Maximum allowed number of iterations for the root-finding procedure.
    p : ndarray, shape (n,)
        Found solution of a trust-region problem.
    alpha : float
        Positive value such that (J.T*J + alpha*I)*p = -J.T*f.
        Sometimes called Levenberg-Marquardt parameter.
    n_iter : int
        Number of iterations made by root-finding procedure. Zero means
        that Gauss-Newton step was selected as the solution.
    .. [1] More, J. J., "The Levenberg-Marquardt Algorithm: Implementation
           and Theory," Numerical Analysis, ed. G. A. Watson, Lecture Notes
           in Mathematics 630, Springer Verlag, pp. 105-116, 1977.
    def phi_and_derivative(alpha, suf, s, Delta):
        """Function of which to find zero.
        It is defined as "norm of regularized (by alpha) least-squares
        solution minus `Delta`". Refer to [1]_.
        denom = s**2 + alpha
        p_norm = norm(suf / denom)
        phi = p_norm - Delta
        phi_prime = -np.sum(suf ** 2 / denom**3) / p_norm
        return phi, phi_prime
    suf = s * uf
    # Check if J has full rank and try Gauss-Newton step.
    if m >= n:
        threshold = EPS * m * s[0]
        full_rank = s[-1] > threshold
        full_rank = False
    if full_rank:
        p = -V.dot(uf / s)
        if norm(p) <= Delta:
            return p, 0.0, 0
    alpha_upper = norm(suf) / Delta
        phi, phi_prime = phi_and_derivative(0.0, suf, s, Delta)
        alpha_lower = -phi / phi_prime
        alpha_lower = 0.0
    if initial_alpha is None or not full_rank and initial_alpha == 0:
        alpha = max(0.001 * alpha_upper, (alpha_lower * alpha_upper)**0.5)
        alpha = initial_alpha
    for it in range(max_iter):
        if alpha < alpha_lower or alpha > alpha_upper:
        phi, phi_prime = phi_and_derivative(alpha, suf, s, Delta)
        if phi < 0:
            alpha_upper = alpha
        ratio = phi / phi_prime
        alpha_lower = max(alpha_lower, alpha - ratio)
        alpha -= (phi + Delta) * ratio / Delta
        if np.abs(phi) < rtol * Delta:
    p = -V.dot(suf / (s**2 + alpha))
    # Make the norm of p equal to Delta, p is changed only slightly during
    # this. It is done to prevent p lie outside the trust region (which can
    # cause problems later).
    p *= Delta / norm(p)
    return p, alpha, it + 1
def solve_trust_region_2d(B, g, Delta):
    """Solve a general trust-region problem in 2 dimensions.
    The problem is reformulated as a 4th order algebraic equation,
    the solution of which is found by numpy.roots.
    B : ndarray, shape (2, 2)
        Symmetric matrix, defines a quadratic term of the function.
    g : ndarray, shape (2,)
        Defines a linear term of the function.
    p : ndarray, shape (2,)
        Found solution.
    newton_step : bool
        Whether the returned solution is the Newton step which lies within
        the trust region.
        R, lower = cho_factor(B)
        p = -cho_solve((R, lower), g)
        if np.dot(p, p) <= Delta**2:
            return p, True
    except LinAlgError:
    a = B[0, 0] * Delta**2
    b = B[0, 1] * Delta**2
    c = B[1, 1] * Delta**2
    d = g[0] * Delta
    f = g[1] * Delta
    coeffs = np.array(
        [-b + d, 2 * (a - c + f), 6 * b, 2 * (-a + c + f), -b - d])
    t = np.roots(coeffs)  # Can handle leading zeros.
    t = np.real(t[np.isreal(t)])
    p = Delta * np.vstack((2 * t / (1 + t**2), (1 - t**2) / (1 + t**2)))
    value = 0.5 * np.sum(p * B.dot(p), axis=0) + np.dot(g, p)
    i = np.argmin(value)
    p = p[:, i]
    return p, False
def update_tr_radius(Delta, actual_reduction, predicted_reduction,
                     step_norm, bound_hit):
    """Update the radius of a trust region based on the cost reduction.
        New radius.
    ratio : float
        Ratio between actual and predicted reductions.
    if predicted_reduction > 0:
        ratio = actual_reduction / predicted_reduction
    elif predicted_reduction == actual_reduction == 0:
        ratio = 1
        ratio = 0
    if ratio < 0.25:
        Delta = 0.25 * step_norm
    elif ratio > 0.75 and bound_hit:
        Delta *= 2.0
    return Delta, ratio
# Construction and minimization of quadratic functions.
def build_quadratic_1d(J, g, s, diag=None, s0=None):
    """Parameterize a multivariate quadratic function along a line.
    The resulting univariate quadratic function is given as follows::
        f(t) = 0.5 * (s0 + s*t).T * (J.T*J + diag) * (s0 + s*t) +
               g.T * (s0 + s*t)
    J : ndarray, sparse matrix or LinearOperator shape (m, n)
        Jacobian matrix, affects the quadratic term.
    g : ndarray, shape (n,)
        Gradient, defines the linear term.
    s : ndarray, shape (n,)
        Direction vector of a line.
    diag : None or ndarray with shape (n,), optional
        Addition diagonal part, affects the quadratic term.
        If None, assumed to be 0.
    s0 : None or ndarray with shape (n,), optional
        Initial point. If None, assumed to be 0.
    a : float
        Coefficient for t**2.
    b : float
        Coefficient for t.
    c : float
        Free term. Returned only if `s0` is provided.
    v = J.dot(s)
    a = np.dot(v, v)
    if diag is not None:
        a += np.dot(s * diag, s)
    a *= 0.5
    b = np.dot(g, s)
    if s0 is not None:
        u = J.dot(s0)
        b += np.dot(u, v)
        c = 0.5 * np.dot(u, u) + np.dot(g, s0)
            b += np.dot(s0 * diag, s)
            c += 0.5 * np.dot(s0 * diag, s0)
        return a, b, c
        return a, b
def minimize_quadratic_1d(a, b, lb, ub, c=0):
    """Minimize a 1-D quadratic function subject to bounds.
    The free term `c` is 0 by default. Bounds must be finite.
        Minimum point.
    y : float
        Minimum value.
    t = [lb, ub]
    if a != 0:
        extremum = -0.5 * b / a
        if lb < extremum < ub:
            t.append(extremum)
    y = t * (a * t + b) + c
    min_index = np.argmin(y)
    return t[min_index], y[min_index]
def evaluate_quadratic(J, g, s, diag=None):
    """Compute values of a quadratic function arising in least squares.
    The function is 0.5 * s.T * (J.T * J + diag) * s + g.T * s.
    J : ndarray, sparse matrix or LinearOperator, shape (m, n)
    s : ndarray, shape (k, n) or (n,)
        Array containing steps as rows.
    diag : ndarray, shape (n,), optional
    values : ndarray with shape (k,) or float
        Values of the function. If `s` was 2-D, then ndarray is
        returned, otherwise, float is returned.
    if s.ndim == 1:
        Js = J.dot(s)
        q = np.dot(Js, Js)
            q += np.dot(s * diag, s)
        Js = J.dot(s.T)
        q = np.sum(Js**2, axis=0)
            q += np.sum(diag * s**2, axis=1)
    l = np.dot(s, g)
    return 0.5 * q + l
# Utility functions to work with bound constraints.
def in_bounds(x, lb, ub):
    """Check if a point lies within bounds."""
    return np.all((x >= lb) & (x <= ub))
def step_size_to_bound(x, s, lb, ub):
    """Compute a min_step size required to reach a bound.
    The function computes a positive scalar t, such that x + s * t is on
    the bound.
    step : float
        Computed step. Non-negative value.
    hits : ndarray of int with shape of x
        Each element indicates whether a corresponding variable reaches the
        bound:
             *  0 - the bound was not hit.
             * -1 - the lower bound was hit.
             *  1 - the upper bound was hit.
    non_zero = np.nonzero(s)
    s_non_zero = s[non_zero]
    steps = np.empty_like(x)
    steps.fill(np.inf)
    with np.errstate(over='ignore'):
        steps[non_zero] = np.maximum((lb - x)[non_zero] / s_non_zero,
                                     (ub - x)[non_zero] / s_non_zero)
    min_step = np.min(steps)
    return min_step, np.equal(steps, min_step) * np.sign(s).astype(int)
def find_active_constraints(x, lb, ub, rtol=1e-10):
    """Determine which constraints are active in a given point.
    The threshold is computed using `rtol` and the absolute value of the
    closest bound.
    active : ndarray of int with shape of x
        Each component shows whether the corresponding constraint is active:
             *  0 - a constraint is not active.
             * -1 - a lower bound is active.
             *  1 - a upper bound is active.
    active = np.zeros_like(x, dtype=int)
    if rtol == 0:
        active[x <= lb] = -1
        active[x >= ub] = 1
        return active
    lower_dist = x - lb
    upper_dist = ub - x
    lower_threshold = rtol * np.maximum(1, np.abs(lb))
    upper_threshold = rtol * np.maximum(1, np.abs(ub))
    lower_active = (np.isfinite(lb) &
                    (lower_dist <= np.minimum(upper_dist, lower_threshold)))
    active[lower_active] = -1
    upper_active = (np.isfinite(ub) &
                    (upper_dist <= np.minimum(lower_dist, upper_threshold)))
    active[upper_active] = 1
def make_strictly_feasible(x, lb, ub, rstep=1e-10):
    """Shift a point to the interior of a feasible region.
    Each element of the returned vector is at least at a relative distance
    `rstep` from the closest bound. If ``rstep=0`` then `np.nextafter` is used.
    x_new = x.copy()
    active = find_active_constraints(x, lb, ub, rstep)
    lower_mask = np.equal(active, -1)
    upper_mask = np.equal(active, 1)
    if rstep == 0:
        x_new[lower_mask] = np.nextafter(lb[lower_mask], ub[lower_mask])
        x_new[upper_mask] = np.nextafter(ub[upper_mask], lb[upper_mask])
        x_new[lower_mask] = (lb[lower_mask] +
                             rstep * np.maximum(1, np.abs(lb[lower_mask])))
        x_new[upper_mask] = (ub[upper_mask] -
                             rstep * np.maximum(1, np.abs(ub[upper_mask])))
    tight_bounds = (x_new < lb) | (x_new > ub)
    x_new[tight_bounds] = 0.5 * (lb[tight_bounds] + ub[tight_bounds])
    return x_new
def CL_scaling_vector(x, g, lb, ub):
    """Compute Coleman-Li scaling vector and its derivatives.
    Components of a vector v are defined as follows::
               | ub[i] - x[i], if g[i] < 0 and ub[i] < np.inf
        v[i] = | x[i] - lb[i], if g[i] > 0 and lb[i] > -np.inf
               | 1,           otherwise
    According to this definition v[i] >= 0 for all i. It differs from the
    definition in paper [1]_ (eq. (2.2)), where the absolute value of v is
    used. Both definitions are equivalent down the line.
    Derivatives of v with respect to x take value 1, -1 or 0 depending on a
    v : ndarray with shape of x
        Scaling vector.
    dv : ndarray with shape of x
        Derivatives of v[i] with respect to x[i], diagonal elements of v's
        Jacobian.
    .. [1] M.A. Branch, T.F. Coleman, and Y. Li, "A Subspace, Interior,
           and Conjugate Gradient Method for Large-Scale Bound-Constrained
           Minimization Problems," SIAM Journal on Scientific Computing,
           Vol. 21, Number 1, pp 1-23, 1999.
    v = np.ones_like(x)
    dv = np.zeros_like(x)
    mask = (g < 0) & np.isfinite(ub)
    v[mask] = ub[mask] - x[mask]
    dv[mask] = -1
    mask = (g > 0) & np.isfinite(lb)
    v[mask] = x[mask] - lb[mask]
    dv[mask] = 1
    return v, dv
def reflective_transformation(y, lb, ub):
    """Compute reflective transformation and its gradient."""
    if in_bounds(y, lb, ub):
        return y, np.ones_like(y)
    lb_finite = np.isfinite(lb)
    ub_finite = np.isfinite(ub)
    x = y.copy()
    g_negative = np.zeros_like(y, dtype=bool)
    mask = lb_finite & ~ub_finite
    x[mask] = np.maximum(y[mask], 2 * lb[mask] - y[mask])
    g_negative[mask] = y[mask] < lb[mask]
    mask = ~lb_finite & ub_finite
    x[mask] = np.minimum(y[mask], 2 * ub[mask] - y[mask])
    g_negative[mask] = y[mask] > ub[mask]
    mask = lb_finite & ub_finite
    d = ub - lb
    t = np.remainder(y[mask] - lb[mask], 2 * d[mask])
    x[mask] = lb[mask] + np.minimum(t, 2 * d[mask] - t)
    g_negative[mask] = t > d[mask]
    g = np.ones_like(y)
    g[g_negative] = -1
    return x, g
# Functions to display algorithm's progress.
def print_header_nonlinear():
    print("{:^15}{:^15}{:^15}{:^15}{:^15}{:^15}"
          .format("Iteration", "Total nfev", "Cost", "Cost reduction",
                  "Step norm", "Optimality"))
def print_iteration_nonlinear(iteration, nfev, cost, cost_reduction,
                              step_norm, optimality):
    if cost_reduction is None:
        cost_reduction = " " * 15
        cost_reduction = f"{cost_reduction:^15.2e}"
    if step_norm is None:
        step_norm = " " * 15
        step_norm = f"{step_norm:^15.2e}"
    print(f"{iteration:^15}{nfev:^15}{cost:^15.4e}{cost_reduction}{step_norm}{optimality:^15.2e}")
def print_header_linear():
    print("{:^15}{:^15}{:^15}{:^15}{:^15}"
          .format("Iteration", "Cost", "Cost reduction", "Step norm",
                  "Optimality"))
def print_iteration_linear(iteration, cost, cost_reduction, step_norm,
                           optimality):
    print(f"{iteration:^15}{cost:^15.4e}{cost_reduction}{step_norm}{optimality:^15.2e}")
# Simple helper functions.
def compute_grad(J, f):
    """Compute gradient of the least-squares cost function."""
    if isinstance(J, LinearOperator):
        return J.rmatvec(f)
        return J.T.dot(f)
def compute_jac_scale(J, scale_inv_old=None):
    """Compute variables scale based on the Jacobian matrix."""
    if issparse(J):
        scale_inv = np.asarray(J.power(2).sum(axis=0)).ravel()**0.5
        scale_inv = np.sum(J**2, axis=0)**0.5
    if scale_inv_old is None:
        scale_inv[scale_inv == 0] = 1
        scale_inv = np.maximum(scale_inv, scale_inv_old)
    return 1 / scale_inv, scale_inv
def left_multiplied_operator(J, d):
    """Return diag(d) J as LinearOperator."""
    J = aslinearoperator(J)
    def matvec(x):
        return d * J.matvec(x)
    def matmat(X):
        return d[:, np.newaxis] * J.matmat(X)
    def rmatvec(x):
        return J.rmatvec(x.ravel() * d)
    return LinearOperator(J.shape, matvec=matvec, matmat=matmat,
                          rmatvec=rmatvec)
def right_multiplied_operator(J, d):
    """Return J diag(d) as LinearOperator."""
        return J.matvec(np.ravel(x) * d)
        return J.matmat(X * d[:, np.newaxis])
        return d * J.rmatvec(x)
def regularized_lsq_operator(J, diag):
    """Return a matrix arising in regularized least squares as LinearOperator.
    The matrix is
        [ J ]
        [ D ]
    where D is diagonal matrix with elements from `diag`.
    m, n = J.shape
        return np.hstack((J.matvec(x), diag * x))
        x1 = x[:m]
        x2 = x[m:]
        return J.rmatvec(x1) + diag * x2
    return LinearOperator((m + n, n), matvec=matvec, rmatvec=rmatvec)
def right_multiply(J, d, copy=True):
    """Compute J diag(d).
    If `copy` is False, `J` is modified in place (unless being LinearOperator).
    if copy and not isinstance(J, LinearOperator):
        J = J.copy()
        J.data *= d.take(J.indices, mode='clip')  # scikit-learn recipe.
    elif isinstance(J, LinearOperator):
        J = right_multiplied_operator(J, d)
        J *= d
    return J
def left_multiply(J, d, copy=True):
    """Compute diag(d) J.
        J.data *= np.repeat(d, np.diff(J.indptr))  # scikit-learn recipe.
        J = left_multiplied_operator(J, d)
        J *= d[:, np.newaxis]
def check_termination(dF, F, dx_norm, x_norm, ratio, ftol, xtol):
    """Check termination condition for nonlinear least squares."""
    ftol_satisfied = dF < ftol * F and ratio > 0.25
    xtol_satisfied = dx_norm < xtol * (xtol + x_norm)
    if ftol_satisfied and xtol_satisfied:
        return 4
    elif ftol_satisfied:
    elif xtol_satisfied:
def scale_for_robust_loss_function(J, f, rho):
    """Scale Jacobian and residuals for a robust loss function.
    Arrays are modified in place.
    J_scale = rho[1] + 2 * rho[2] * f**2
    J_scale[J_scale < EPS] = EPS
    J_scale **= 0.5
    f *= rho[1] / J_scale
    return left_multiply(J, J_scale, copy=False), f
