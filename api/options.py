class Option(object):
    def __init__(self, key, value):
        """Initialize the Option, used for passing
        options into requests
            key (str): The key for the option
            value (str): The value for the option
        self._key = key
        self._value = value
    def key(self):
        """Gets and sets the key of the :class:`Option`
            str: The key"""
        return self._key
    @key.setter
    def key(self, value):
        self._key = value
    def value(self):
        """Gets and sets the value of the :class:`Option`
            str: The value"""
    @value.setter
    def value(self, value):
class QueryOption(Option):
    """Option used for the query string of a request"""
class HeaderOption(Option):
    """Option used for the header of the request"""
import shlex
from .compat import compat_expanduser
from .cookies import SUPPORTED_BROWSERS, SUPPORTED_KEYRINGS
from .downloader.external import list_external_downloaders
    SponsorBlockPP,
from .postprocessor.modify_chapters import DEFAULT_SPONSORBLOCK_CHAPTER_TITLE
from .update import UPDATE_SOURCES, detect_variant, is_non_updateable
    OUTTMPL_TYPES,
    deprecation_warning,
    get_executable_path,
    get_system_config_dirs,
    get_user_config_dirs,
    orderedSet_from_options,
from .version import CHANNEL, __version__
def parseOpts(overrideArguments=None, ignore_config_files='if_override'):  # noqa: N803
    PACKAGE_NAME = 'yt-dlp'
    root = Config(create_parser())
    if ignore_config_files == 'if_override':
        ignore_config_files = overrideArguments is not None
    def read_config(*paths):
        path = os.path.join(*paths)
        conf = Config.read_file(path, default=None)
        if conf is not None:
            return conf, path
    def _load_from_config_dirs(config_dirs):
        for config_dir in config_dirs:
            head, tail = os.path.split(config_dir)
            assert tail == PACKAGE_NAME or config_dir == os.path.join(compat_expanduser('~'), f'.{PACKAGE_NAME}')
            yield read_config(head, f'{PACKAGE_NAME}.conf')
            if tail.startswith('.'):  # ~/.PACKAGE_NAME
                yield read_config(head, f'{PACKAGE_NAME}.conf.txt')
            yield read_config(config_dir, 'config')
            yield read_config(config_dir, 'config.txt')
    def add_config(label, path=None, func=None):
        """ Adds config and returns whether to continue """
        if root.parse_known_args()[0].ignoreconfig:
        elif func:
            assert path is None
            args, current_path = next(
                filter(None, _load_from_config_dirs(func(PACKAGE_NAME))), (None, None))
            current_path = os.path.join(path, 'yt-dlp.conf')
            args = Config.read_file(current_path, default=None)
        if args is not None:
            root.append_config(args, current_path, label=label)
    def load_configs():
        yield not ignore_config_files
        yield add_config('Portable', get_executable_path())
        yield add_config('Home', expand_path(root.parse_known_args()[0].paths.get('home', '')).strip())
        yield add_config('User', func=get_user_config_dirs)
        yield add_config('System', func=get_system_config_dirs)
    opts = optparse.Values({'verbose': True, 'print_help': False})
            if overrideArguments is not None:
                root.append_config(overrideArguments, label='Override')
                root.append_config(sys.argv[1:], label='Command-line')
            loaded_all_configs = all(load_configs())
            raise root.parser.error(err)
        if loaded_all_configs:
            # If ignoreconfig is found inside the system configuration file,
            # the user configuration is removed
                user_conf = next((i for i, conf in enumerate(root.configs) if conf.label == 'User'), None)
                if user_conf is not None:
                    root.configs.pop(user_conf)
            root.configs[0].load_configs()  # Resolve any aliases using --config-location
        opts, args = root.parse_args()
    except optparse.OptParseError:
        with contextlib.suppress(optparse.OptParseError):
            opts, _ = root.parse_known_args(strict=False)
    except (SystemExit, KeyboardInterrupt):
        opts.verbose = False
        verbose = opts.verbose and f'\n{root}'.replace('\n| ', '\n[debug] ')[1:]
            write_string(f'{verbose}\n')
        if opts.print_help:
                write_string('\n')
            root.parser.print_help()
    return root.parser, opts, args
class _YoutubeDLHelpFormatter(optparse.IndentedHelpFormatter):
        # No need to wrap help messages if we're on a wide console
        max_width = shutil.get_terminal_size().columns or 80
        # The % is chosen to get a pretty output in README.md
        super().__init__(width=max_width, max_help_position=int(0.45 * max_width))
    def format_option_strings(option):
        """ ('-o', '--option') -> -o, --format METAVAR """
        opts = join_nonempty(
            option._short_opts and option._short_opts[0],
            option._long_opts and option._long_opts[0],
            delim=', ')
        if option.takes_value():
            opts += f' {option.metavar}'
        return opts
_PRESET_ALIASES = {
    'mp3': ['-f', 'ba[acodec^=mp3]/ba/b', '-x', '--audio-format', 'mp3'],
    'aac': ['-f', 'ba[acodec^=aac]/ba[acodec^=mp4a.40.]/ba/b', '-x', '--audio-format', 'aac'],
    'mp4': ['--merge-output-format', 'mp4', '--remux-video', 'mp4', '-S', 'vcodec:h264,lang,quality,res,fps,hdr:12,acodec:aac'],
    'mkv': ['--merge-output-format', 'mkv', '--remux-video', 'mkv'],
    'sleep': ['--sleep-subtitles', '5', '--sleep-requests', '0.75', '--sleep-interval', '10', '--max-sleep-interval', '20'],
class _YoutubeDLOptionParser(optparse.OptionParser):
    # optparse is deprecated since Python 3.2. So assume a stable interface even for private methods
    ALIAS_DEST = '_triggered_aliases'
    ALIAS_TRIGGER_LIMIT = 100
            prog='yt-dlp' if detect_variant() == 'source' else None,
            usage='%prog [OPTIONS] URL [URL...]',
            epilog='See full documentation at  https://github.com/yt-dlp/yt-dlp#readme',
            formatter=_YoutubeDLHelpFormatter(),
            conflict_handler='resolve',
        self.set_default(self.ALIAS_DEST, collections.defaultdict(int))
    _UNKNOWN_OPTION = (optparse.BadOptionError, optparse.AmbiguousOptionError)
    _BAD_OPTION = optparse.OptionValueError
    def parse_known_args(self, args=None, values=None, strict=True):
        """Same as parse_args, but ignore unknown switches. Similar to argparse.parse_known_args"""
        self.rargs, self.largs = self._get_args(args), []
        self.values = values or self.get_default_values()
        while self.rargs:
            arg = self.rargs[0]
                if arg == '--':
                    del self.rargs[0]
                    break
                elif arg.startswith('--'):
                    self._process_long_opt(self.rargs, self.values)
                elif arg.startswith('-') and arg != '-':
                    self._process_short_opts(self.rargs, self.values)
                elif self.allow_interspersed_args:
                    self.largs.append(self.rargs.pop(0))
            except optparse.OptParseError as err:
                if isinstance(err, self._UNKNOWN_OPTION):
                    self.largs.append(err.opt_str)
                elif strict:
                    if isinstance(err, self._BAD_OPTION):
                        self.error(str(err))
        return self.check_values(self.values, self.largs)
    def _generate_error_message(self, msg):
        msg = f'{self.get_prog_name()}: error: {str(msg).strip()}\n'
        return f'{self.get_usage()}\n{msg}' if self.usage else msg
    def error(self, msg):
        raise optparse.OptParseError(self._generate_error_message(msg))
    def _get_args(self, args):
        return sys.argv[1:] if args is None else list(args)
    def _match_long_opt(self, opt):
        """Improve ambiguous argument resolution by comparing option objects instead of argument strings"""
            return super()._match_long_opt(opt)
        except optparse.AmbiguousOptionError as e:
            if len({self._long_opt[p] for p in e.possibilities}) == 1:
                return e.possibilities[0]
    def format_option_help(self, formatter=None):
        assert formatter, 'Formatter can not be None'
        formatted_help = super().format_option_help(formatter=formatter)
        formatter.indent()
        heading = formatter.format_heading('Preset Aliases')
        description = formatter.format_description(
            'Predefined aliases for convenience and ease of use. Note that future versions of yt-dlp '
            'may add or adjust presets, but the existing preset names will not be changed or removed')
        for name, args in _PRESET_ALIASES.items():
            option = optparse.Option('-t', help=shlex.join(args))
            formatter.option_strings[option] = f'-t {name}'
            result.append(formatter.format_option(option))
        formatter.dedent()
        help_lines = '\n'.join(result)
        return f'{formatted_help}\n{heading}{description}\n{help_lines}'
def create_parser():
    def _list_from_options_callback(option, opt_str, value, parser, append=True, delim=',', process=str.strip):
        # append can be True, False or -1 (prepend)
        current = list(getattr(parser.values, option.dest)) if append else []
        value = list(filter(None, [process(value)] if delim is None else map(process, value.split(delim))))
        setattr(
            parser.values, option.dest,
            current + value if append is True else value + current)
    def _set_from_options_callback(
            option, opt_str, value, parser, allowed_values, delim=',', aliases={},
            process=lambda x: x.lower().strip()):
        values = [process(value)] if delim is None else map(process, value.split(delim))
            requested = orderedSet_from_options(values, collections.ChainMap(aliases, {'all': allowed_values}),
                                                start=getattr(parser.values, option.dest))
            raise optparse.OptionValueError(f'wrong {option.metavar} for {opt_str}: {e.args[0]}')
        setattr(parser.values, option.dest, set(requested))
    def _dict_from_options_callback(
            option, opt_str, value, parser,
            allowed_keys=r'[\w-]+', delimiter=':', default_key=None, process=None, multiple_keys=True,
            process_key=str.lower, append=False):
        out_dict = dict(getattr(parser.values, option.dest))
        multiple_args = not isinstance(value, str)
        if multiple_keys:
            allowed_keys = fr'({allowed_keys})(,({allowed_keys}))*'
        mobj = re.match(
            fr'(?is)(?P<keys>{allowed_keys}){delimiter}(?P<val>.*)$',
            value[0] if multiple_args else value)
        if mobj is not None:
            keys, val = mobj.group('keys').split(','), mobj.group('val')
            if multiple_args:
                val = [val, *value[1:]]
        elif default_key is not None:
            keys, val = variadic(default_key), value
            raise optparse.OptionValueError(
                f'wrong {opt_str} formatting; it should be {option.metavar}, not "{value}"')
            keys = map(process_key, keys) if process_key else keys
            val = process(val) if process else val
            raise optparse.OptionValueError(f'wrong {opt_str} formatting; {err}')
            out_dict[key] = [*out_dict.get(key, []), val] if append else val
        setattr(parser.values, option.dest, out_dict)
    def when_prefix(default):
            'default': {},
            'type': 'str',
            'action': 'callback',
            'callback': _dict_from_options_callback,
            'callback_kwargs': {
                'allowed_keys': '|'.join(map(re.escape, POSTPROCESS_WHEN)),
                'default_key': default,
                'multiple_keys': False,
                'append': True,
    parser = _YoutubeDLOptionParser()
    alias_group = optparse.OptionGroup(parser, 'Aliases')
    Formatter = string.Formatter()
    def _create_alias(option, opt_str, value, parser):
        aliases, opts = value
            nargs = len({i if f == '' else f
                         for i, (_, f, _, _) in enumerate(Formatter.parse(opts)) if f is not None})
            opts.format(*map(str, range(nargs)))  # validate
            raise optparse.OptionValueError(f'wrong {opt_str} OPTIONS formatting; {err}')
        if alias_group not in parser.option_groups:
            parser.add_option_group(alias_group)
        aliases = (x if x.startswith('-') else f'--{x}' for x in map(str.strip, aliases.split(',')))
            args = [f'ARG{i}' for i in range(nargs)]
            alias_group.add_option(
                *aliases, nargs=nargs, dest=parser.ALIAS_DEST, type='str' if nargs else None,
                metavar=' '.join(args), help=opts.format(*args), action='callback',
                callback=_alias_callback, callback_kwargs={'opts': opts, 'nargs': nargs})
    def _alias_callback(option, opt_str, value, parser, opts, nargs):
        counter = getattr(parser.values, option.dest)
        counter[opt_str] += 1
        if counter[opt_str] > parser.ALIAS_TRIGGER_LIMIT:
            raise optparse.OptionValueError(f'Alias {opt_str} exceeded invocation limit')
        if nargs == 1:
            value = [value]
        assert (nargs == 0 and value is None) or len(value) == nargs
        parser.rargs[:0] = shlex.split(
            opts if value is None else opts.format(*map(shlex.quote, value)))
    def _preset_alias_callback(option, opt_str, value, parser):
        if not value:
        if value not in _PRESET_ALIASES:
            raise optparse.OptionValueError(f'Unknown preset alias: {value}')
        parser.rargs[:0] = _PRESET_ALIASES[value]
    general = optparse.OptionGroup(parser, 'General Options')
    general.add_option(
        '-h', '--help', dest='print_help', action='store_true',
        help='Print this help text and exit')
        '--version',
        action='version',
        help='Print program version and exit')
        '-U', '--update',
        action='store_const', dest='update_self', const=CHANNEL,
        help=format_field(
            is_non_updateable(), None, 'Check if updates are available. %s',
            default=f'Update this program to the latest {CHANNEL} version'))
        '--no-update',
        action='store_false', dest='update_self',
        help='Do not check for updates (default)')
        '--update-to',
        action='store', dest='update_self', metavar='[CHANNEL]@[TAG]',
            'Upgrade/downgrade to a specific version. CHANNEL can be a repository as well. '
            f'CHANNEL and TAG default to "{CHANNEL.partition("@")[0]}" and "latest" respectively if omitted; '
            f'See "UPDATE" for details. Supported channels: {", ".join(UPDATE_SOURCES)}'))
        '-i', '--ignore-errors',
        action='store_true', dest='ignoreerrors',
        help='Ignore download and postprocessing errors. The download will be considered successful even if the postprocessing fails')
        '--no-abort-on-error',
        action='store_const', dest='ignoreerrors', const='only_download',
        help='Continue with next video on download errors; e.g. to skip unavailable videos in a playlist (default)')
        '--abort-on-error', '--no-ignore-errors',
        action='store_false', dest='ignoreerrors',
        help='Abort downloading of further videos if an error occurs (Alias: --no-ignore-errors)')
        '--list-extractors',
        action='store_true', dest='list_extractors', default=False,
        help='List all supported extractors and exit')
        '--extractor-descriptions',
        action='store_true', dest='list_extractor_descriptions', default=False,
        help='Output descriptions of all supported extractors and exit')
        '--use-extractors', '--ies',
        action='callback', dest='allowed_extractors', metavar='NAMES', type='str',
        default=[], callback=_list_from_options_callback,
            'Extractor names to use separated by commas. '
            'You can also use regexes, "all", "default" and "end" (end URL matching); '
            'e.g. --ies "holodex.*,end,youtube". '
            'Prefix the name with a "-" to exclude it, e.g. --ies default,-generic. '
            'Use --list-extractors for a list of extractor names. (Alias: --ies)'))
        '--force-generic-extractor',
        action='store_true', dest='force_generic_extractor', default=False,
        help=optparse.SUPPRESS_HELP)
        '--default-search',
        dest='default_search', metavar='PREFIX',
            'Use this prefix for unqualified URLs. '
            'E.g. "gvsearch2:python" downloads two videos from google videos for the search term "python". '
            'Use the value "auto" to let yt-dlp guess ("auto_warning" to emit a warning when guessing). '
            '"error" just throws an error. The default value "fixup_error" repairs broken URLs, '
            'but emits an error if this is not possible instead of searching'))
        '--ignore-config', '--no-config',
        action='store_true', dest='ignoreconfig',
            'Don\'t load any more configuration files except those given to --config-locations. '
            'For backward compatibility, if this option is found inside the system configuration file, the user configuration is not loaded. '
            '(Alias: --no-config)'))
        '--no-config-locations',
        action='store_const', dest='config_locations', const=None,
            'Do not load any custom configuration files (default). When given inside a '
            'configuration file, ignore all previous --config-locations defined in the current file'))
        '--config-locations',
        dest='config_locations', metavar='PATH', action='append',
            'Location of the main configuration file; either the path to the config or its containing directory '
            '("-" for stdin). Can be used multiple times and inside other configuration files'))
        '--plugin-dirs',
        metavar='DIR',
        dest='plugin_dirs',
        action='callback',
        callback=_list_from_options_callback,
        type='str',
        callback_kwargs={'delim': None},
        default=['default'],
            'Path to an additional directory to search for plugins. '
            'This option can be used multiple times to add multiple directories. '
            'Use "default" to search the default plugin directories (default)'))
        '--no-plugin-dirs',
        dest='plugin_dirs', action='store_const', const=[],
        help='Clear plugin directories to search, including defaults and those provided by previous --plugin-dirs')
        '--js-runtimes',
        metavar='RUNTIME[:PATH]',
        dest='js_runtimes',
        default=['deno'],
            'Additional JavaScript runtime to enable, with an optional location for the runtime '
            '(either the path to the binary or its containing directory). '
            'This option can be used multiple times to enable multiple runtimes. '
            'Supported runtimes are (in order of priority, from highest to lowest): deno, node, quickjs, bun. '
            'Only "deno" is enabled by default. The highest priority runtime that is both enabled and '
            'available will be used. In order to use a lower priority runtime when "deno" is available, '
            '--no-js-runtimes needs to be passed before enabling other runtimes'))
        '--no-js-runtimes',
        dest='js_runtimes', action='store_const', const=[],
        help='Clear JavaScript runtimes to enable, including defaults and those provided by previous --js-runtimes')
        '--remote-components',
        metavar='COMPONENT',
        dest='remote_components',
            'Remote components to allow yt-dlp to fetch when required. '
            'This option is currently not needed if you are using an official executable '
            'or have the requisite version of the yt-dlp-ejs package installed. '
            'You can use this option multiple times to allow multiple components. '
            'Supported values: ejs:npm (external JavaScript components from npm), '
            'ejs:github (external JavaScript components from yt-dlp-ejs GitHub). '
            'By default, no remote components are allowed'))
        '--no-remote-components',
        dest='remote_components', action='store_const', const=[],
        help='Disallow fetching of all remote components, including any previously allowed by --remote-components or defaults.')
        '--flat-playlist',
        action='store_const', dest='extract_flat', const='in_playlist', default=False,
            'Do not extract a playlist\'s URL result entries; '
            'some entry metadata may be missing and downloading may be bypassed'))
        '--no-flat-playlist',
        action='store_false', dest='extract_flat',
        help='Fully extract the videos of a playlist (default)')
        '--live-from-start',
        action='store_true', dest='live_from_start',
        help='Download livestreams from the start. Currently experimental and only supported for YouTube, Twitch, and TVer')
        '--no-live-from-start',
        action='store_false', dest='live_from_start',
        help='Download livestreams from the current time (default)')
        '--wait-for-video',
        dest='wait_for_video', metavar='MIN[-MAX]', default=None,
            'Wait for scheduled streams to become available. '
            'Pass the minimum number of seconds (or range) to wait between retries'))
        '--no-wait-for-video',
        dest='wait_for_video', action='store_const', const=None,
        help='Do not wait for scheduled streams (default)')
        '--mark-watched',
        action='store_true', dest='mark_watched', default=False,
        help='Mark videos watched (even with --simulate)')
        '--no-mark-watched',
        action='store_false', dest='mark_watched',
        help='Do not mark videos watched (default)')
        '--no-colors', '--no-colours',
        action='store_const', dest='color', const={
            'stdout': 'no_color',
            'stderr': 'no_color',
        dest='color', metavar='[STREAM:]POLICY', default={}, type='str',
        action='callback', callback=_dict_from_options_callback,
        callback_kwargs={
            'allowed_keys': 'stdout|stderr',
            'default_key': ['stdout', 'stderr'],
            'process': str.strip,
        }, help=(
            'Whether to emit color codes in output, optionally prefixed by '
            'the STREAM (stdout or stderr) to apply the setting to. '
            'Can be one of "always", "auto" (default), "never", or '
            '"no_color" (use non color terminal sequences). '
            'Use "auto-tty" or "no_color-tty" to decide based on terminal support only. '
            'Can be used multiple times'))
        '--compat-options',
        metavar='OPTS', dest='compat_opts', default=set(), type='str',
        action='callback', callback=_set_from_options_callback,
            'allowed_values': {
                'filename', 'filename-sanitization', 'format-sort', 'abort-on-error', 'format-spec', 'no-playlist-metafiles',
                'multistreams', 'no-live-chat', 'playlist-index', 'list-formats', 'no-direct-merge', 'playlist-match-filter',
                'no-attach-info-json', 'embed-thumbnail-atomicparsley', 'no-external-downloader-progress',
                'embed-metadata', 'seperate-video-versions', 'no-clean-infojson', 'no-keep-subs', 'no-certifi',
                'no-youtube-channel-redirect', 'no-youtube-unavailable-videos', 'no-youtube-prefer-utc-upload-date',
                'prefer-legacy-http-handler', 'manifest-filesize-approx', 'allow-unsafe-ext', 'prefer-vp9-sort', 'mtime-by-default',
            }, 'aliases': {
                'youtube-dl': ['all', '-multistreams', '-playlist-match-filter', '-manifest-filesize-approx', '-allow-unsafe-ext', '-prefer-vp9-sort'],
                'youtube-dlc': ['all', '-no-youtube-channel-redirect', '-no-live-chat', '-playlist-match-filter', '-manifest-filesize-approx', '-allow-unsafe-ext', '-prefer-vp9-sort'],
                '2021': ['2022', 'no-certifi', 'filename-sanitization'],
                '2022': ['2023', 'no-external-downloader-progress', 'playlist-match-filter', 'prefer-legacy-http-handler', 'manifest-filesize-approx'],
                '2023': ['2024', 'prefer-vp9-sort'],
                '2024': ['2025', 'mtime-by-default'],
                '2025': [],
            'Options that can help keep compatibility with youtube-dl or youtube-dlc '
            'configurations by reverting some of the changes made in yt-dlp. '
            'See "Differences in default behavior" for details'))
        '--alias', metavar='ALIASES OPTIONS', dest='_', type='str', nargs=2,
        action='callback', callback=_create_alias,
            'Create aliases for an option string. Unless an alias starts with a dash "-", it is prefixed with "--". '
            'Arguments are parsed according to the Python string formatting mini-language. '
            'E.g. --alias get-audio,-X "-S aext:{0},abr -x --audio-format {0}" creates options '
            '"--get-audio" and "-X" that takes an argument (ARG0) and expands to '
            '"-S aext:ARG0,abr -x --audio-format ARG0". All defined aliases are listed in the --help output. '
            'Alias options can trigger more aliases; so be careful to avoid defining recursive options. '
            f'As a safety measure, each alias may be triggered a maximum of {_YoutubeDLOptionParser.ALIAS_TRIGGER_LIMIT} times. '
            'This option can be used multiple times'))
        '-t', '--preset-alias',
        metavar='PRESET', dest='_', type='str',
        action='callback', callback=_preset_alias_callback,
            'Applies a predefined set of options. e.g. --preset-alias mp3. '
            f'The following presets are available: {", ".join(_PRESET_ALIASES)}. '
            'See the "Preset Aliases" section at the end for more info. '
    network = optparse.OptionGroup(parser, 'Network Options')
    network.add_option(
        '--proxy', dest='proxy',
        default=None, metavar='URL',
            'Use the specified HTTP/HTTPS/SOCKS proxy. To enable SOCKS proxy, specify a proper scheme, '
            'e.g. socks5://user:pass@127.0.0.1:1080/. Pass in an empty string (--proxy "") for direct connection'))
        '--socket-timeout',
        dest='socket_timeout', type=float, default=None, metavar='SECONDS',
        help='Time to wait before giving up, in seconds')
        '--source-address',
        metavar='IP', dest='source_address', default=None,
        help='Client-side IP address to bind to',
        '--impersonate',
        metavar='CLIENT[:OS]', dest='impersonate', default=None,
            'Client to impersonate for requests. E.g. chrome, chrome-110, chrome:windows-10. '
            'Pass --impersonate="" to impersonate any client. Note that forcing impersonation '
            'for all requests may have a detrimental impact on download speed and stability'),
        '--list-impersonate-targets',
        dest='list_impersonate_targets', default=False, action='store_true',
        help='List available clients to impersonate.',
        '-4', '--force-ipv4',
        action='store_const', const='0.0.0.0', dest='source_address',
        help='Make all connections via IPv4',
        '-6', '--force-ipv6',
        action='store_const', const='::', dest='source_address',
        help='Make all connections via IPv6',
        '--enable-file-urls', action='store_true',
        dest='enable_file_urls', default=False,
        help='Enable file:// URLs. This is disabled by default for security reasons.',
    geo = optparse.OptionGroup(parser, 'Geo-restriction')
    geo.add_option(
        '--geo-verification-proxy',
        dest='geo_verification_proxy', default=None, metavar='URL',
            'Use this proxy to verify the IP address for some geo-restricted sites. '
            'The default proxy specified by --proxy (or none, if the option is not present) is used for the actual downloading'))
        '--xff', metavar='VALUE',
        dest='geo_bypass', default='default',
            'How to fake X-Forwarded-For HTTP header to try bypassing geographic restriction. '
            'One of "default" (only when known to be useful), "never", '
            'an IP block in CIDR notation, or a two-letter ISO 3166-2 country code'))
        '--geo-bypass',
        action='store_const', dest='geo_bypass', const='default',
        '--no-geo-bypass',
        action='store_const', dest='geo_bypass', const='never',
        '--geo-bypass-country', metavar='CODE', dest='geo_bypass',
        '--geo-bypass-ip-block', metavar='IP_BLOCK', dest='geo_bypass',
    selection = optparse.OptionGroup(parser, 'Video Selection')
    selection.add_option(
        '--playlist-start',
        dest='playliststart', metavar='NUMBER', default=1, type=int,
        '--playlist-end',
        dest='playlistend', metavar='NUMBER', default=None, type=int,
        '-I', '--playlist-items',
        dest='playlist_items', metavar='ITEM_SPEC', default=None,
            'Comma-separated playlist_index of the items to download. '
            'You can specify a range using "[START]:[STOP][:STEP]". For backward compatibility, START-STOP is also supported. '
            'Use negative indices to count from the right and negative STEP to download in reverse order. '
            'E.g. "-I 1:3,7,-5::2" used on a playlist of size 15 will download the items at index 1,2,3,7,11,13,15'))
        '--match-title',
        dest='matchtitle', metavar='REGEX',
        '--reject-title',
        dest='rejecttitle', metavar='REGEX',
        '--min-filesize',
        metavar='SIZE', dest='min_filesize', default=None,
        help='Abort download if filesize is smaller than SIZE, e.g. 50k or 44.6M')
        '--max-filesize',
        metavar='SIZE', dest='max_filesize', default=None,
        help='Abort download if filesize is larger than SIZE, e.g. 50k or 44.6M')
        '--date',
        metavar='DATE', dest='date', default=None,
            'Download only videos uploaded on this date. '
            'The date can be "YYYYMMDD" or in the format [now|today|yesterday][-N[day|week|month|year]]. '
            'E.g. "--date today-2weeks" downloads only videos uploaded on the same day two weeks ago'))
        '--datebefore',
        metavar='DATE', dest='datebefore', default=None,
            'Download only videos uploaded on or before this date. '
            'The date formats accepted are the same as --date'))
        '--dateafter',
        metavar='DATE', dest='dateafter', default=None,
            'Download only videos uploaded on or after this date. '
        '--min-views',
        metavar='COUNT', dest='min_views', default=None, type=int,
        '--max-views',
        metavar='COUNT', dest='max_views', default=None, type=int,
        '--match-filters',
        metavar='FILTER', dest='match_filter', action='append',
            'Generic video filter. Any "OUTPUT TEMPLATE" field can be compared with a '
            'number or a string using the operators defined in "Filtering Formats". '
            'You can also simply specify a field to match if the field is present, '
            'use "!field" to check if the field is not present, and "&" to check multiple conditions. '
            'Use a "\\" to escape "&" or quotes if needed. If used multiple times, '
            'the filter matches if at least one of the conditions is met. E.g. --match-filters '
            '!is_live --match-filters "like_count>?100 & description~=\'(?i)\\bcats \\& dogs\\b\'" '
            'matches only videos that are not live OR those that have a like count more than 100 '
            '(or the like field is not available) and also has a description '
            'that contains the phrase "cats & dogs" (caseless). '
            'Use "--match-filters -" to interactively ask whether to download each video'))
        '--no-match-filters',
        dest='match_filter', action='store_const', const=None,
        help='Do not use any --match-filters (default)')
        '--break-match-filters',
        metavar='FILTER', dest='breaking_match_filter', action='append',
        help='Same as "--match-filters" but stops the download process when a video is rejected')
        '--no-break-match-filters',
        dest='breaking_match_filter', action='store_const', const=None,
        help='Do not use any --break-match-filters (default)')
        '--no-playlist',
        action='store_true', dest='noplaylist', default=False,
        help='Download only the video, if the URL refers to a video and a playlist')
        '--yes-playlist',
        action='store_false', dest='noplaylist',
        help='Download the playlist, if the URL refers to a video and a playlist')
        '--age-limit',
        metavar='YEARS', dest='age_limit', default=None, type=int,
        help='Download only videos suitable for the given age')
        '--download-archive', metavar='FILE',
        dest='download_archive',
        help='Download only videos not listed in the archive file. Record the IDs of all downloaded videos in it')
        '--no-download-archive',
        dest='download_archive', action='store_const', const=None,
        help='Do not use archive file (default)')
        '--max-downloads',
        dest='max_downloads', metavar='NUMBER', type=int, default=None,
        help='Abort after downloading NUMBER files')
        '--break-on-existing',
        action='store_true', dest='break_on_existing', default=False,
        help='Stop the download process when encountering a file that is in the archive '
             'supplied with the --download-archive option')
        '--no-break-on-existing',
        action='store_false', dest='break_on_existing',
        help='Do not stop the download process when encountering a file that is in the archive (default)')
        '--break-on-reject',
        action='store_true', dest='break_on_reject', default=False,
        '--break-per-input',
        action='store_true', dest='break_per_url', default=False,
        help='Alters --max-downloads, --break-on-existing, --break-match-filters, and autonumber to reset per input URL')
        '--no-break-per-input',
        action='store_false', dest='break_per_url',
        help='--break-on-existing and similar options terminates the entire download queue')
        '--skip-playlist-after-errors', metavar='N',
        dest='skip_playlist_after_errors', default=None, type=int,
        help='Number of allowed failures until the rest of the playlist is skipped')
    authentication = optparse.OptionGroup(parser, 'Authentication Options')
    authentication.add_option(
        '-u', '--username',
        dest='username', metavar='USERNAME',
        help='Login with this account ID')
        '-p', '--password',
        dest='password', metavar='PASSWORD',
        help='Account password. If this option is left out, yt-dlp will ask interactively')
        '-2', '--twofactor',
        dest='twofactor', metavar='TWOFACTOR',
        help='Two-factor authentication code')
        '-n', '--netrc',
        action='store_true', dest='usenetrc', default=False,
        help='Use .netrc authentication data')
        '--netrc-location',
        dest='netrc_location', metavar='PATH',
        help='Location of .netrc authentication data; either the path or its containing directory. Defaults to ~/.netrc')
        '--netrc-cmd',
        dest='netrc_cmd', metavar='NETRC_CMD',
        help='Command to execute to get the credentials for an extractor.')
        '--video-password',
        dest='videopassword', metavar='PASSWORD',
        help='Video-specific password')
        '--ap-mso',
        dest='ap_mso', metavar='MSO',
        help='Adobe Pass multiple-system operator (TV provider) identifier, use --ap-list-mso for a list of available MSOs')
        '--ap-username',
        dest='ap_username', metavar='USERNAME',
        help='Multiple-system operator account login')
        '--ap-password',
        dest='ap_password', metavar='PASSWORD',
        help='Multiple-system operator account password. If this option is left out, yt-dlp will ask interactively')
        '--ap-list-mso',
        action='store_true', dest='ap_list_mso', default=False,
        help='List all supported multiple-system operators')
        '--client-certificate',
        dest='client_certificate', metavar='CERTFILE',
        help='Path to client certificate file in PEM format. May include the private key')
        '--client-certificate-key',
        dest='client_certificate_key', metavar='KEYFILE',
        help='Path to private key file for client certificate')
        '--client-certificate-password',
        dest='client_certificate_password', metavar='PASSWORD',
        help='Password for client certificate private key, if encrypted. '
             'If not provided, and the key is encrypted, yt-dlp will ask interactively')
    video_format = optparse.OptionGroup(parser, 'Video Format Options')
    video_format.add_option(
        '-f', '--format',
        action='store', dest='format', metavar='FORMAT', default=None,
        help='Video format code, see "FORMAT SELECTION" for more details')
        '-S', '--format-sort', metavar='SORTORDER',
        dest='format_sort', default=[], type='str', action='callback',
        callback=_list_from_options_callback, callback_kwargs={'append': -1},
        help='Sort the formats by the fields given, see "Sorting Formats" for more details')
        '--format-sort-reset',
        dest='format_sort', action='store_const', const=[],
        help='Disregard previous user specified sort order and reset to the default')
        '--format-sort-force', '--S-force',
        action='store_true', dest='format_sort_force', metavar='FORMAT', default=False,
            'Force user specified sort order to have precedence over all fields, '
            'see "Sorting Formats" for more details (Alias: --S-force)'))
        '--no-format-sort-force',
        action='store_false', dest='format_sort_force', metavar='FORMAT', default=False,
        help='Some fields have precedence over the user specified sort order (default)')
        '--video-multistreams',
        action='store_true', dest='allow_multiple_video_streams', default=None,
        help='Allow multiple video streams to be merged into a single file')
        '--no-video-multistreams',
        action='store_false', dest='allow_multiple_video_streams',
        help='Only one video stream is downloaded for each output file (default)')
        '--audio-multistreams',
        action='store_true', dest='allow_multiple_audio_streams', default=None,
        help='Allow multiple audio streams to be merged into a single file')
        '--no-audio-multistreams',
        action='store_false', dest='allow_multiple_audio_streams',
        help='Only one audio stream is downloaded for each output file (default)')
        '--all-formats',
        action='store_const', dest='format', const='all',
        '--prefer-free-formats',
        action='store_true', dest='prefer_free_formats', default=False,
            'Prefer video formats with free containers over non-free ones of the same quality. '
            'Use with "-S ext" to strictly prefer free containers irrespective of quality'))
        '--no-prefer-free-formats',
        action='store_false', dest='prefer_free_formats', default=False,
        help="Don't give any special preference to free containers (default)")
        '--check-formats',
        action='store_const', const='selected', dest='check_formats', default=None,
        help='Make sure formats are selected only from those that are actually downloadable')
        '--check-all-formats',
        action='store_true', dest='check_formats',
        help='Check all formats for whether they are actually downloadable')
        '--no-check-formats',
        action='store_false', dest='check_formats',
        help='Do not check that the formats are actually downloadable')
        '-F', '--list-formats',
        action='store_true', dest='listformats',
        help='List available formats of each video. Simulate unless --no-simulate is used')
        '--list-formats-as-table',
        action='store_true', dest='listformats_table', default=True,
        '--list-formats-old', '--no-list-formats-as-table',
        action='store_false', dest='listformats_table',
        '--merge-output-format',
        action='store', dest='merge_output_format', metavar='FORMAT', default=None,
            'Containers that may be used when merging formats, separated by "/", e.g. "mp4/mkv". '
            'Ignored if no merge is required. '
            f'(currently supported: {", ".join(sorted(FFmpegMergerPP.SUPPORTED_EXTS))})'))
        '--allow-unplayable-formats',
        action='store_true', dest='allow_unplayable_formats', default=False,
        '--no-allow-unplayable-formats',
        action='store_false', dest='allow_unplayable_formats',
    subtitles = optparse.OptionGroup(parser, 'Subtitle Options')
    subtitles.add_option(
        '--write-subs', '--write-srt',
        action='store_true', dest='writesubtitles', default=False,
        help='Write subtitle file')
        '--no-write-subs', '--no-write-srt',
        action='store_false', dest='writesubtitles',
        help='Do not write subtitle file (default)')
        '--write-auto-subs', '--write-automatic-subs',
        action='store_true', dest='writeautomaticsub', default=False,
        help='Write automatically generated subtitle file (Alias: --write-automatic-subs)')
        '--no-write-auto-subs', '--no-write-automatic-subs',
        action='store_false', dest='writeautomaticsub', default=False,
        help='Do not write auto-generated subtitles (default) (Alias: --no-write-automatic-subs)')
        '--all-subs',
        action='store_true', dest='allsubtitles', default=False,
        '--list-subs',
        action='store_true', dest='listsubtitles', default=False,
        help='List available subtitles of each video. Simulate unless --no-simulate is used')
        '--sub-format',
        action='store', dest='subtitlesformat', metavar='FORMAT', default='best',
        help='Subtitle format; accepts formats preference separated by "/", e.g. "srt" or "ass/srt/best"')
        '--sub-langs', '--srt-langs',
        action='callback', dest='subtitleslangs', metavar='LANGS', type='str',
            'Languages of the subtitles to download (can be regex) or "all" separated by commas, e.g. --sub-langs "en.*,ja" '
            '(where "en.*" is a regex pattern that matches "en" followed by 0 or more of any character). '
            'You can prefix the language code with a "-" to exclude it from the requested languages, e.g. --sub-langs all,-live_chat. '
            'Use --list-subs for a list of available language tags'))
    downloader = optparse.OptionGroup(parser, 'Download Options')
    downloader.add_option(
        '-N', '--concurrent-fragments',
        dest='concurrent_fragment_downloads', metavar='N', default=1, type=int,
        help='Number of fragments of a dash/hlsnative video that should be downloaded concurrently (default is %default)')
        '-r', '--limit-rate', '--rate-limit',
        dest='ratelimit', metavar='RATE',
        help='Maximum download rate in bytes per second, e.g. 50K or 4.2M')
        '--throttled-rate',
        dest='throttledratelimit', metavar='RATE',
        help='Minimum download rate in bytes per second below which throttling is assumed and the video data is re-extracted, e.g. 100K')
        '-R', '--retries',
        dest='retries', metavar='RETRIES', default=10,
        help='Number of retries (default is %default), or "infinite"')
        '--file-access-retries',
        dest='file_access_retries', metavar='RETRIES', default=3,
        help='Number of times to retry on file access error (default is %default), or "infinite"')
        '--fragment-retries',
        dest='fragment_retries', metavar='RETRIES', default=10,
        help='Number of retries for a fragment (default is %default), or "infinite" (DASH, hlsnative and ISM)')
        '--retry-sleep',
        dest='retry_sleep', metavar='[TYPE:]EXPR', default={}, type='str',
            'allowed_keys': 'http|fragment|file_access|extractor',
            'default_key': 'http',
            'Time to sleep between retries in seconds (optionally) prefixed by the type of retry '
            '(http (default), fragment, file_access, extractor) to apply the sleep to. '
            'EXPR can be a number, linear=START[:END[:STEP=1]] or exp=START[:END[:BASE=2]]. '
            'This option can be used multiple times to set the sleep for the different retry types, '
            'e.g. --retry-sleep linear=1::2 --retry-sleep fragment:exp=1:20'))
        '--skip-unavailable-fragments', '--no-abort-on-unavailable-fragments',
        action='store_true', dest='skip_unavailable_fragments', default=True,
        help='Skip unavailable fragments for DASH, hlsnative and ISM downloads (default) (Alias: --no-abort-on-unavailable-fragments)')
        '--abort-on-unavailable-fragments', '--no-skip-unavailable-fragments',
        action='store_false', dest='skip_unavailable_fragments',
        help='Abort download if a fragment is unavailable (Alias: --no-skip-unavailable-fragments)')
        '--keep-fragments',
        action='store_true', dest='keep_fragments', default=False,
        help='Keep downloaded fragments on disk after downloading is finished')
        '--no-keep-fragments',
        action='store_false', dest='keep_fragments',
        help='Delete downloaded fragments after downloading is finished (default)')
        '--buffer-size',
        dest='buffersize', metavar='SIZE', default='1024',
        help='Size of download buffer, e.g. 1024 or 16K (default is %default)')
        '--resize-buffer',
        action='store_false', dest='noresizebuffer',
        help='The buffer size is automatically resized from an initial value of --buffer-size (default)')
        '--no-resize-buffer',
        action='store_true', dest='noresizebuffer', default=False,
        help='Do not automatically adjust the buffer size')
        '--http-chunk-size',
        dest='http_chunk_size', metavar='SIZE', default=None,
            'Size of a chunk for chunk-based HTTP downloading, e.g. 10485760 or 10M (default is disabled). '
            'May be useful for bypassing bandwidth throttling imposed by a webserver (experimental)'))
        '--test',
        action='store_true', dest='test', default=False,
        '--playlist-reverse',
        action='store_true', dest='playlist_reverse',
        '--no-playlist-reverse',
        action='store_false', dest='playlist_reverse',
        '--playlist-random',
        action='store_true', dest='playlist_random',
        help='Download playlist videos in random order')
        '--lazy-playlist',
        action='store_true', dest='lazy_playlist',
        help='Process entries in the playlist as they are received. This disables n_entries, --playlist-random and --playlist-reverse')
        '--no-lazy-playlist',
        action='store_false', dest='lazy_playlist',
        help='Process videos in the playlist only after the entire playlist is parsed (default)')
        '--hls-prefer-native',
        dest='hls_prefer_native', action='store_true', default=None,
        '--hls-prefer-ffmpeg',
        dest='hls_prefer_native', action='store_false', default=None,
        '--hls-use-mpegts',
        dest='hls_use_mpegts', action='store_true', default=None,
            'Use the mpegts container for HLS videos; '
            'allowing some players to play the video while downloading, '
            'and reducing the chance of file corruption if download is interrupted. '
            'This is enabled by default for live streams'))
        '--no-hls-use-mpegts',
        dest='hls_use_mpegts', action='store_false',
            'Do not use the mpegts container for HLS videos. '
            'This is default when not downloading live streams'))
        '--download-sections',
        metavar='REGEX', dest='download_ranges', action='append',
            'Download only chapters that match the regular expression. '
            'A "*" prefix denotes time-range instead of chapter. Negative timestamps are calculated from the end. '
            '"*from-url" can be used to download between the "start_time" and "end_time" extracted from the URL. '
            'Needs ffmpeg. This option can be used multiple times to download multiple sections, '
            'e.g. --download-sections "*10:15-inf" --download-sections "intro"'))
        '--downloader', '--external-downloader',
        dest='external_downloader', metavar='[PROTO:]NAME', default={}, type='str',
            'allowed_keys': 'http|ftp|m3u8|dash|rtsp|rtmp|mms',
            'default_key': 'default',
            'Name or path of the external downloader to use (optionally) prefixed by '
            'the protocols (http, ftp, m3u8, dash, rstp, rtmp, mms) to use it for. '
            f'Currently supports native, {", ".join(sorted(list_external_downloaders()))}. '
            'You can use this option multiple times to set different downloaders for different protocols. '
            'E.g. --downloader aria2c --downloader "dash,m3u8:native" will use '
            'aria2c for http/ftp downloads, and the native downloader for dash/m3u8 downloads '
            '(Alias: --external-downloader)'))
        '--downloader-args', '--external-downloader-args',
        metavar='NAME:ARGS', dest='external_downloader_args', default={}, type='str',
            'allowed_keys': r'ffmpeg_[io]\d*|{}'.format('|'.join(map(re.escape, list_external_downloaders()))),
            'process': shlex.split,
            'Give these arguments to the external downloader. '
            'Specify the downloader name and the arguments separated by a colon ":". '
            'For ffmpeg, arguments can be passed to different positions using the same syntax as --postprocessor-args. '
            'You can use this option multiple times to give different arguments to different downloaders '
            '(Alias: --external-downloader-args)'))
    workarounds = optparse.OptionGroup(parser, 'Workarounds')
    workarounds.add_option(
        '--encoding',
        dest='encoding', metavar='ENCODING',
        help='Force the specified encoding (experimental)')
        '--legacy-server-connect',
        action='store_true', dest='legacy_server_connect', default=False,
        help='Explicitly allow HTTPS connection to servers that do not support RFC 5746 secure renegotiation')
        '--no-check-certificates',
        action='store_true', dest='no_check_certificate', default=False,
        help='Suppress HTTPS certificate validation')
        '--prefer-insecure', '--prefer-unsecure',
        action='store_true', dest='prefer_insecure',
        help='Use an unencrypted connection to retrieve information about the video (Currently supported only for YouTube)')
        '--user-agent',
        metavar='UA', dest='user_agent',
        '--referer',
        metavar='URL', dest='referer', default=None,
        '--add-headers',
        metavar='FIELD:VALUE', dest='headers', default={}, type='str',
        callback_kwargs={'multiple_keys': False},
        help='Specify a custom HTTP header and its value, separated by a colon ":". You can use this option multiple times',
        '--bidi-workaround',
        dest='bidi_workaround', action='store_true',
        help='Work around terminals that lack bidirectional text support. Requires bidiv or fribidi executable in PATH')
        '--sleep-requests', metavar='SECONDS',
        dest='sleep_interval_requests', type=float,
        help='Number of seconds to sleep between requests during data extraction')
        '--sleep-interval', '--min-sleep-interval', metavar='SECONDS',
        dest='sleep_interval', type=float,
            'Number of seconds to sleep before each download. '
            'This is the minimum time to sleep when used along with --max-sleep-interval '
            '(Alias: --min-sleep-interval)'))
        '--max-sleep-interval', metavar='SECONDS',
        dest='max_sleep_interval', type=float,
        help='Maximum number of seconds to sleep. Can only be used along with --min-sleep-interval')
        '--sleep-subtitles', metavar='SECONDS',
        dest='sleep_interval_subtitles', default=0, type=float,
        help='Number of seconds to sleep before each subtitle download')
    verbosity = optparse.OptionGroup(parser, 'Verbosity and Simulation Options')
    verbosity.add_option(
        '-q', '--quiet',
        action='store_true', dest='quiet', default=None,
        help='Activate quiet mode. If used with --verbose, print the log to stderr')
        '--no-quiet',
        action='store_false', dest='quiet',
        help='Deactivate quiet mode. (Default)')
        '--no-warnings',
        dest='no_warnings', action='store_true', default=False,
        help='Ignore warnings')
        '-s', '--simulate',
        action='store_true', dest='simulate', default=None,
        help='Do not download the video and do not write anything to disk')
        '--no-simulate',
        action='store_false', dest='simulate',
        help='Download the video even if printing/listing options are used')
        '--ignore-no-formats-error',
        action='store_true', dest='ignore_no_formats_error', default=False,
            'Ignore "No video formats" error. Useful for extracting metadata '
            'even if the videos are not actually available for download (experimental)'))
        '--no-ignore-no-formats-error',
        action='store_false', dest='ignore_no_formats_error',
        help='Throw error when no downloadable video formats are found (default)')
        '--skip-download', '--no-download',
        action='store_true', dest='skip_download', default=False,
        help='Do not download the video but write all related files (Alias: --no-download)')
        '-O', '--print',
        metavar='[WHEN:]TEMPLATE', dest='forceprint', **when_prefix('video'),
            'Field name or output template to print to screen, optionally prefixed with when to print it, separated by a ":". '
            'Supported values of "WHEN" are the same as that of --use-postprocessor (default: video). '
            'Implies --quiet. Implies --simulate unless --no-simulate or later stages of WHEN are used. '
        '--print-to-file',
        metavar='[WHEN:]TEMPLATE FILE', dest='print_to_file', nargs=2, **when_prefix('video'),
            'Append given template to the file. The values of WHEN and TEMPLATE are the same as that of --print. '
            'FILE uses the same syntax as the output template. This option can be used multiple times'))
        '-g', '--get-url',
        action='store_true', dest='geturl', default=False,
        '-e', '--get-title',
        action='store_true', dest='gettitle', default=False,
        '--get-id',
        action='store_true', dest='getid', default=False,
        '--get-thumbnail',
        action='store_true', dest='getthumbnail', default=False,
        '--get-description',
        action='store_true', dest='getdescription', default=False,
        '--get-duration',
        action='store_true', dest='getduration', default=False,
        '--get-filename',
        action='store_true', dest='getfilename', default=False,
        '--get-format',
        action='store_true', dest='getformat', default=False,
        '-j', '--dump-json',
        action='store_true', dest='dumpjson', default=False,
            'Quiet, but print JSON information for each video. Simulate unless --no-simulate is used. '
            'See "OUTPUT TEMPLATE" for a description of available keys'))
        '-J', '--dump-single-json',
        action='store_true', dest='dump_single_json', default=False,
            'Quiet, but print JSON information for each URL or infojson passed. Simulate unless --no-simulate is used. '
            'If the URL refers to a playlist, the whole playlist information is dumped in a single line'))
        '--print-json',
        action='store_true', dest='print_json', default=False,
        '--force-write-archive', '--force-write-download-archive', '--force-download-archive',
        action='store_true', dest='force_write_download_archive', default=False,
            'Force download archive entries to be written as far as no errors occur, '
            'even if -s or another simulation option is used (Alias: --force-download-archive)'))
        '--newline',
        action='store_true', dest='progress_with_newline', default=False,
        help='Output progress bar as new lines')
        '--no-progress',
        action='store_true', dest='noprogress', default=None,
        help='Do not print progress bar')
        '--progress',
        action='store_false', dest='noprogress',
        help='Show progress bar, even if in quiet mode')
        '--console-title',
        action='store_true', dest='consoletitle', default=False,
        help='Display progress in console titlebar')
        '--progress-template',
        metavar='[TYPES:]TEMPLATE', dest='progress_template', default={}, type='str',
            'allowed_keys': '(download|postprocess)(-title)?',
            'default_key': 'download',
            'Template for progress outputs, optionally prefixed with one of "download:" (default), '
            '"download-title:" (the console title), "postprocess:",  or "postprocess-title:". '
            'The video\'s fields are accessible under the "info" key and '
            'the progress attributes are accessible under "progress" key. E.g. '
            # TODO: Document the fields inside "progress"
            '--console-title --progress-template "download-title:%(info.id)s-%(progress.eta)s"'))
        '--progress-delta',
        metavar='SECONDS', action='store', dest='progress_delta', type=float, default=0,
        help='Time between progress output (default: 0)')
        '-v', '--verbose',
        action='store_true', dest='verbose', default=False,
        help='Print various debugging information')
        '--dump-pages',
        action='store_true', dest='dump_intermediate_pages', default=False,
        help='Print downloaded pages encoded using base64 to debug problems (very verbose)')
        '--write-pages',
        action='store_true', dest='write_pages', default=False,
        help='Write downloaded intermediary pages to files in the current directory to debug problems')
        '--load-pages',
        action='store_true', dest='load_pages', default=False,
        '--print-traffic',
        dest='debug_printtraffic', action='store_true', default=False,
        help='Display sent and read HTTP traffic')
    filesystem = optparse.OptionGroup(parser, 'Filesystem Options')
    filesystem.add_option(
        '-a', '--batch-file',
        dest='batchfile', metavar='FILE',
            'File containing URLs to download ("-" for stdin), one URL per line. '
            'Lines starting with "#", ";" or "]" are considered as comments and ignored'))
        '--no-batch-file',
        dest='batchfile', action='store_const', const=None,
        help='Do not read URLs from batch file (default)')
        '--id', default=False,
        action='store_true', dest='useid', help=optparse.SUPPRESS_HELP)
        '-P', '--paths',
        metavar='[TYPES:]PATH', dest='paths', default={}, type='str',
            'allowed_keys': 'home|temp|{}'.format('|'.join(map(re.escape, OUTTMPL_TYPES.keys()))),
            'default_key': 'home',
            'The paths where the files should be downloaded. '
            'Specify the type of file and the path separated by a colon ":". '
            'All the same TYPES as --output are supported. '
            'Additionally, you can also provide "home" (default) and "temp" paths. '
            'All intermediary files are first downloaded to the temp path and '
            'then the final files are moved over to the home path after download is finished. '
            'This option is ignored if --output is an absolute path'))
        '-o', '--output',
        metavar='[TYPES:]TEMPLATE', dest='outtmpl', default={}, type='str',
            'allowed_keys': '|'.join(map(re.escape, OUTTMPL_TYPES.keys())),
        }, help='Output filename template; see "OUTPUT TEMPLATE" for details')
        '--output-na-placeholder',
        dest='outtmpl_na_placeholder', metavar='TEXT', default='NA',
        help=('Placeholder for unavailable fields in --output (default: "%default")'))
        '--autonumber-size',
        dest='autonumber_size', metavar='NUMBER', type=int,
        '--autonumber-start',
        dest='autonumber_start', metavar='NUMBER', default=1, type=int,
        '--restrict-filenames',
        action='store_true', dest='restrictfilenames', default=False,
        help='Restrict filenames to only ASCII characters, and avoid "&" and spaces in filenames')
        '--no-restrict-filenames',
        action='store_false', dest='restrictfilenames',
        help='Allow Unicode characters, "&" and spaces in filenames (default)')
        '--windows-filenames',
        action='store_true', dest='windowsfilenames', default=None,
        help='Force filenames to be Windows-compatible')
        '--no-windows-filenames',
        action='store_false', dest='windowsfilenames',
        help='Sanitize filenames only minimally')
        '--trim-filenames', '--trim-file-names', metavar='LENGTH',
        dest='trim_file_name', default=0, type=int,
        help='Limit the filename length (excluding extension) to the specified number of characters')
        '-w', '--no-overwrites',
        action='store_false', dest='overwrites', default=None,
        help='Do not overwrite any files')
        '--force-overwrites', '--yes-overwrites',
        action='store_true', dest='overwrites',
        help='Overwrite all video and metadata files. This option includes --no-continue')
        '--no-force-overwrites',
        action='store_const', dest='overwrites', const=None,
        help='Do not overwrite the video, but overwrite related files (default)')
        '-c', '--continue',
        action='store_true', dest='continue_dl', default=True,
        help='Resume partially downloaded files/fragments (default)')
        '--no-continue',
        action='store_false', dest='continue_dl',
            'Do not resume partially downloaded fragments. '
            'If the file is not fragmented, restart download of the entire file'))
        '--part',
        action='store_false', dest='nopart', default=False,
        help='Use .part files instead of writing directly into output file (default)')
        '--no-part',
        action='store_true', dest='nopart',
        help='Do not use .part files - write directly into output file')
        '--mtime',
        action='store_true', dest='updatetime', default=None,
        help='Use the Last-modified header to set the file modification time')
        '--no-mtime',
        action='store_false', dest='updatetime',
        help='Do not use the Last-modified header to set the file modification time (default)')
        '--write-description',
        action='store_true', dest='writedescription', default=False,
        help='Write video description to a .description file')
        '--no-write-description',
        action='store_false', dest='writedescription',
        help='Do not write video description (default)')
        '--write-info-json',
        action='store_true', dest='writeinfojson', default=None,
        help='Write video metadata to a .info.json file (this may contain personal information)')
        '--no-write-info-json',
        action='store_false', dest='writeinfojson',
        help='Do not write video metadata (default)')
        '--write-playlist-metafiles',
        action='store_true', dest='allow_playlist_files', default=None,
            'Write playlist metadata in addition to the video metadata '
            'when using --write-info-json, --write-description etc. (default)'))
        '--no-write-playlist-metafiles',
        action='store_false', dest='allow_playlist_files',
        help='Do not write playlist metadata when using --write-info-json, --write-description etc.')
        '--clean-info-json', '--clean-infojson',
        action='store_true', dest='clean_infojson', default=None,
            'Remove some internal metadata such as filenames from the infojson (default)'))
        '--no-clean-info-json', '--no-clean-infojson',
        action='store_false', dest='clean_infojson',
        help='Write all fields to the infojson')
        '--write-comments', '--get-comments',
        action='store_true', dest='getcomments', default=False,
            'Retrieve video comments to be placed in the infojson. '
            'The comments are fetched even without this option if the extraction is known to be quick (Alias: --get-comments)'))
        '--no-write-comments', '--no-get-comments',
        action='store_false', dest='getcomments',
        help='Do not retrieve video comments unless the extraction is known to be quick (Alias: --no-get-comments)')
        '--load-info-json',
        dest='load_info_filename', metavar='FILE',
        help='JSON file containing the video information (created with the "--write-info-json" option)')
        '--cookies',
        dest='cookiefile', metavar='FILE',
        help='Netscape formatted file to read cookies from and dump cookie jar in')
        '--no-cookies',
        action='store_const', const=None, dest='cookiefile', metavar='FILE',
        help='Do not read/dump cookies from/to file (default)')
        '--cookies-from-browser',
        dest='cookiesfrombrowser', metavar='BROWSER[+KEYRING][:PROFILE][::CONTAINER]',
            'The name of the browser to load cookies from. '
            f'Currently supported browsers are: {", ".join(sorted(SUPPORTED_BROWSERS))}. '
            'Optionally, the KEYRING used for decrypting Chromium cookies on Linux, '
            'the name/path of the PROFILE to load cookies from, '
            'and the CONTAINER name (if Firefox) ("none" for no container) '
            'can be given with their respective separators. '
            'By default, all containers of the most recently accessed profile are used. '
            f'Currently supported keyrings are: {", ".join(map(str.lower, sorted(SUPPORTED_KEYRINGS)))}'))
        '--no-cookies-from-browser',
        action='store_const', const=None, dest='cookiesfrombrowser',
        help='Do not load cookies from browser (default)')
        '--cache-dir', dest='cachedir', default=None, metavar='DIR',
            'Location in the filesystem where yt-dlp can store some downloaded information '
            '(such as client ids and signatures) permanently. By default ${XDG_CACHE_HOME}/yt-dlp'))
        '--no-cache-dir', action='store_false', dest='cachedir',
        help='Disable filesystem caching')
        '--rm-cache-dir',
        action='store_true', dest='rm_cachedir',
        help='Delete all filesystem cache files')
    thumbnail = optparse.OptionGroup(parser, 'Thumbnail Options')
    thumbnail.add_option(
        '--write-thumbnail',
        action='callback', dest='writethumbnail', default=False,
        # Should override --no-write-thumbnail, but not --write-all-thumbnail
        callback=lambda option, _, __, parser: setattr(
            parser.values, option.dest, getattr(parser.values, option.dest) or True),
        help='Write thumbnail image to disk')
        '--no-write-thumbnail',
        action='store_false', dest='writethumbnail',
        help='Do not write thumbnail image to disk (default)')
        '--write-all-thumbnails',
        action='store_const', dest='writethumbnail', const='all',
        help='Write all thumbnail image formats to disk')
        '--list-thumbnails',
        action='store_true', dest='list_thumbnails', default=False,
        help='List available thumbnails of each video. Simulate unless --no-simulate is used')
    link = optparse.OptionGroup(parser, 'Internet Shortcut Options')
    link.add_option(
        '--write-link',
        action='store_true', dest='writelink', default=False,
        help='Write an internet shortcut file, depending on the current platform (.url, .webloc or .desktop). The URL may be cached by the OS')
        '--write-url-link',
        action='store_true', dest='writeurllink', default=False,
        help='Write a .url Windows internet shortcut. The OS caches the URL based on the file path')
        '--write-webloc-link',
        action='store_true', dest='writewebloclink', default=False,
        help='Write a .webloc macOS internet shortcut')
        '--write-desktop-link',
        action='store_true', dest='writedesktoplink', default=False,
        help='Write a .desktop Linux internet shortcut')
    postproc = optparse.OptionGroup(parser, 'Post-Processing Options')
    postproc.add_option(
        '-x', '--extract-audio',
        action='store_true', dest='extractaudio', default=False,
        help='Convert video files to audio-only files (requires ffmpeg and ffprobe)')
        '--audio-format', metavar='FORMAT', dest='audioformat', default='best',
            'Format to convert the audio to when -x is used. '
            f'(currently supported: best (default), {", ".join(sorted(FFmpegExtractAudioPP.SUPPORTED_EXTS))}). '
            'You can specify multiple rules using similar syntax as --remux-video'))
        '--audio-quality', metavar='QUALITY',
        dest='audioquality', default='5',
            'Specify ffmpeg audio quality to use when converting the audio with -x. '
            'Insert a value between 0 (best) and 10 (worst) for VBR or a specific bitrate like 128K (default %default)'))
        '--remux-video',
        metavar='FORMAT', dest='remuxvideo', default=None,
            'Remux the video into another container if necessary '
            f'(currently supported: {", ".join(FFmpegVideoRemuxerPP.SUPPORTED_EXTS)}). '
            'If the target container does not support the video/audio codec, remuxing will fail. You can specify multiple rules; '
            'e.g. "aac>m4a/mov>mp4/mkv" will remux aac to m4a, mov to mp4 and anything else to mkv'))
        '--recode-video',
        metavar='FORMAT', dest='recodevideo', default=None,
        help='Re-encode the video into another format if necessary. The syntax and supported formats are the same as --remux-video')
        '--postprocessor-args', '--ppa',
        metavar='NAME:ARGS', dest='postprocessor_args', default={}, type='str',
            'allowed_keys': r'\w+(?:\+\w+)?',
            'default_key': 'default-compat',
            'Give these arguments to the postprocessors. '
            'Specify the postprocessor/executable name and the arguments separated by a colon ":" '
            'to give the argument to the specified postprocessor/executable. Supported PP are: '
            'Merger, ModifyChapters, SplitChapters, ExtractAudio, VideoRemuxer, VideoConvertor, '
            'Metadata, EmbedSubtitle, EmbedThumbnail, SubtitlesConvertor, ThumbnailsConvertor, '
            'FixupStretched, FixupM4a, FixupM3u8, FixupTimestamp and FixupDuration. '
            'The supported executables are: AtomicParsley, FFmpeg and FFprobe. '
            'You can also specify "PP+EXE:ARGS" to give the arguments to the specified executable '
            'only when being used by the specified postprocessor. Additionally, for ffmpeg/ffprobe, '
            '"_i"/"_o" can be appended to the prefix optionally followed by a number to pass the argument '
            'before the specified input/output file, e.g. --ppa "Merger+ffmpeg_i1:-v quiet". '
            'You can use this option multiple times to give different arguments to different '
            'postprocessors. (Alias: --ppa)'))
        '-k', '--keep-video',
        action='store_true', dest='keepvideo', default=False,
        help='Keep the intermediate video file on disk after post-processing')
        '--no-keep-video',
        action='store_false', dest='keepvideo',
        help='Delete the intermediate video file after post-processing (default)')
        '--post-overwrites',
        action='store_false', dest='nopostoverwrites',
        help='Overwrite post-processed files (default)')
        '--no-post-overwrites',
        action='store_true', dest='nopostoverwrites', default=False,
        help='Do not overwrite post-processed files')
        '--embed-subs',
        action='store_true', dest='embedsubtitles', default=False,
        help='Embed subtitles in the video (only for mp4, webm and mkv videos)')
        '--no-embed-subs',
        action='store_false', dest='embedsubtitles',
        help='Do not embed subtitles (default)')
        '--embed-thumbnail',
        action='store_true', dest='embedthumbnail', default=False,
        help='Embed thumbnail in the video as cover art')
        '--no-embed-thumbnail',
        action='store_false', dest='embedthumbnail',
        help='Do not embed thumbnail (default)')
        '--embed-metadata', '--add-metadata',
        action='store_true', dest='addmetadata', default=False,
            'Embed metadata to the video file. Also embeds chapters/infojson if present '
            'unless --no-embed-chapters/--no-embed-info-json are used (Alias: --add-metadata)'))
        '--no-embed-metadata', '--no-add-metadata',
        action='store_false', dest='addmetadata',
        help='Do not add metadata to file (default) (Alias: --no-add-metadata)')
        '--embed-chapters', '--add-chapters',
        action='store_true', dest='addchapters', default=None,
        help='Add chapter markers to the video file (Alias: --add-chapters)')
        '--no-embed-chapters', '--no-add-chapters',
        action='store_false', dest='addchapters',
        help='Do not add chapter markers (default) (Alias: --no-add-chapters)')
        '--embed-info-json',
        action='store_true', dest='embed_infojson', default=None,
        help='Embed the infojson as an attachment to mkv/mka video files')
        '--no-embed-info-json',
        action='store_false', dest='embed_infojson',
        help='Do not embed the infojson as an attachment to the video file')
        '--metadata-from-title',
        metavar='FORMAT', dest='metafromtitle',
        '--parse-metadata',
        metavar='[WHEN:]FROM:TO', dest='parse_metadata', **when_prefix('pre_process'),
            'Parse additional metadata like title/artist from other fields; see "MODIFYING METADATA" for details. '
            'Supported values of "WHEN" are the same as that of --use-postprocessor (default: pre_process)'))
        '--replace-in-metadata',
        dest='parse_metadata', metavar='[WHEN:]FIELDS REGEX REPLACE', nargs=3, **when_prefix('pre_process'),
            'Replace text in a metadata field using the given regex. This option can be used multiple times. '
        '--xattrs', '--xattr',
        action='store_true', dest='xattrs', default=False,
        help='Write metadata to the video file\'s xattrs (using Dublin Core and XDG standards)')
        '--concat-playlist',
        metavar='POLICY', dest='concat_playlist', default='multi_video',
        choices=('never', 'always', 'multi_video'),
            'Concatenate videos in a playlist. One of "never", "always", or '
            '"multi_video" (default; only when the videos form a single show). '
            'All the video files must have the same codecs and number of streams to be concatenable. '
            'The "pl_video:" prefix can be used with "--paths" and "--output" to '
            'set the output filename for the concatenated files. See "OUTPUT TEMPLATE" for details'))
        '--fixup',
        metavar='POLICY', dest='fixup', default=None,
        choices=('never', 'ignore', 'warn', 'detect_or_warn', 'force'),
            'Automatically correct known faults of the file. '
            'One of never (do nothing), warn (only emit a warning), '
            'detect_or_warn (the default; fix the file if we can, warn otherwise), '
            'force (try fixing even if the file already exists)'))
        '--ffmpeg-location', metavar='PATH',
        dest='ffmpeg_location',
        help='Location of the ffmpeg binary; either the path to the binary or its containing directory')
        '--exec',
        metavar='[WHEN:]CMD', dest='exec_cmd', **when_prefix('after_move'),
            'Execute a command, optionally prefixed with when to execute it, separated by a ":". '
            'Supported values of "WHEN" are the same as that of --use-postprocessor (default: after_move). '
            'The same syntax as the output template can be used to pass any field as arguments to the command. '
            'If no fields are passed, %(filepath,_filename|)q is appended to the end of the command. '
        '--no-exec',
        action='store_const', dest='exec_cmd', const={},
        help='Remove any previously defined --exec')
        '--exec-before-download', metavar='CMD',
        action='append', dest='exec_before_dl_cmd',
        '--no-exec-before-download',
        action='store_const', dest='exec_before_dl_cmd', const=None,
        '--convert-subs', '--convert-sub', '--convert-subtitles',
        metavar='FORMAT', dest='convertsubtitles', default=None,
            'Convert the subtitles to another format '
            f'(currently supported: {", ".join(sorted(FFmpegSubtitlesConvertorPP.SUPPORTED_EXTS))}). '
            'Use "--convert-subs none" to disable conversion (default) (Alias: --convert-subtitles)'))
        '--convert-thumbnails',
        metavar='FORMAT', dest='convertthumbnails', default=None,
            'Convert the thumbnails to another format '
            f'(currently supported: {", ".join(sorted(FFmpegThumbnailsConvertorPP.SUPPORTED_EXTS))}). '
            'You can specify multiple rules using similar syntax as "--remux-video". '
            'Use "--convert-thumbnails none" to disable conversion (default)'))
        '--split-chapters', '--split-tracks',
        dest='split_chapters', action='store_true', default=False,
            'Split video into multiple files based on internal chapters. '
            'The "chapter:" prefix can be used with "--paths" and "--output" to '
            'set the output filename for the split files. See "OUTPUT TEMPLATE" for details'))
        '--no-split-chapters', '--no-split-tracks',
        dest='split_chapters', action='store_false',
        help='Do not split video based on chapters (default)')
        '--remove-chapters',
        metavar='REGEX', dest='remove_chapters', action='append',
            'Remove chapters whose title matches the given regular expression. '
            'The syntax is the same as --download-sections. This option can be used multiple times'))
        '--no-remove-chapters', dest='remove_chapters', action='store_const', const=None,
        help='Do not remove any chapters from the file (default)')
        '--force-keyframes-at-cuts',
        action='store_true', dest='force_keyframes_at_cuts', default=False,
            'Force keyframes at cuts when downloading/splitting/removing sections. '
            'This is slow due to needing a re-encode, but the resulting video may have fewer artifacts around the cuts'))
        '--no-force-keyframes-at-cuts',
        action='store_false', dest='force_keyframes_at_cuts',
        help='Do not force keyframes around the chapters when cutting/splitting (default)')
    _postprocessor_opts_parser = lambda key, val='': (
        *(item.split('=', 1) for item in (val.split(';') if val else [])),
        ('key', remove_end(key, 'PP')))
        '--use-postprocessor',
        metavar='NAME[:ARGS]', dest='add_postprocessors', default=[], type='str',
        action='callback', callback=_list_from_options_callback,
            'delim': None,
            'process': lambda val: dict(_postprocessor_opts_parser(*val.split(':', 1))),
            'The (case-sensitive) name of plugin postprocessors to be enabled, '
            'and (optionally) arguments to be passed to it, separated by a colon ":". '
            'ARGS are a semicolon ";" delimited list of NAME=VALUE. '
            'The "when" argument determines when the postprocessor is invoked. '
            'It can be one of "pre_process" (after video extraction), "after_filter" (after video passes filter), '
            '"video" (after --format; before --print/--output), "before_dl" (before each video download), '
            '"post_process" (after each video download; default), '
            '"after_move" (after moving the video file to its final location), '
            '"after_video" (after downloading and processing all formats of a video), '
            'or "playlist" (at end of playlist). '
            'This option can be used multiple times to add different postprocessors'))
    sponsorblock = optparse.OptionGroup(parser, 'SponsorBlock Options', description=(
        'Make chapter entries for, or remove various segments (sponsor, introductions, etc.) '
        'from downloaded YouTube videos using the SponsorBlock API (https://sponsor.ajay.app)'))
    sponsorblock.add_option(
        '--sponsorblock-mark', metavar='CATS',
        dest='sponsorblock_mark', default=set(), action='callback', type='str',
        callback=_set_from_options_callback, callback_kwargs={
            'allowed_values': SponsorBlockPP.CATEGORIES.keys(),
            'aliases': {'default': ['all']},
            'SponsorBlock categories to create chapters for, separated by commas. '
            f'Available categories are {", ".join(SponsorBlockPP.CATEGORIES.keys())}, all and default (=all). '
            'You can prefix the category with a "-" to exclude it. See [1] for descriptions of the categories. '
            'E.g. --sponsorblock-mark all,-preview [1] https://wiki.sponsor.ajay.app/w/Segment_Categories'))
        '--sponsorblock-remove', metavar='CATS',
        dest='sponsorblock_remove', default=set(), action='callback', type='str',
            'allowed_values': set(SponsorBlockPP.CATEGORIES.keys()) - set(SponsorBlockPP.NON_SKIPPABLE_CATEGORIES.keys()),
            # Note: From https://wiki.sponsor.ajay.app/w/Types:
            # The filler category is very aggressive.
            # It is strongly recommended to not use this in a client by default.
            'aliases': {'default': ['all', '-filler']},
            'SponsorBlock categories to be removed from the video file, separated by commas. '
            'If a category is present in both mark and remove, remove takes precedence. '
            'The syntax and available categories are the same as for --sponsorblock-mark '
            'except that "default" refers to "all,-filler" '
            f'and {", ".join(SponsorBlockPP.NON_SKIPPABLE_CATEGORIES.keys())} are not available'))
        '--sponsorblock-chapter-title', metavar='TEMPLATE',
        default=DEFAULT_SPONSORBLOCK_CHAPTER_TITLE, dest='sponsorblock_chapter_title',
            'An output template for the title of the SponsorBlock chapters created by --sponsorblock-mark. '
            'The only available fields are start_time, end_time, category, categories, name, category_names. '
            'Defaults to "%default"'))
        '--no-sponsorblock', default=False,
        action='store_true', dest='no_sponsorblock',
        help='Disable both --sponsorblock-mark and --sponsorblock-remove')
        '--sponsorblock-api', metavar='URL',
        default='https://sponsor.ajay.app', dest='sponsorblock_api',
        help='SponsorBlock API location, defaults to %default')
    extractor = optparse.OptionGroup(parser, 'Extractor Options')
    extractor.add_option(
        '--extractor-retries',
        dest='extractor_retries', metavar='RETRIES', default=3,
        help='Number of retries for known extractor errors (default is %default), or "infinite"')
        '--allow-dynamic-mpd', '--no-ignore-dynamic-mpd',
        action='store_true', dest='dynamic_mpd', default=True,
        help='Process dynamic DASH manifests (default) (Alias: --no-ignore-dynamic-mpd)')
        '--ignore-dynamic-mpd', '--no-allow-dynamic-mpd',
        action='store_false', dest='dynamic_mpd',
        help='Do not process dynamic DASH manifests (Alias: --no-allow-dynamic-mpd)')
        '--hls-split-discontinuity',
        dest='hls_split_discontinuity', action='store_true', default=False,
        help='Split HLS playlists to different formats at discontinuities such as ad breaks',
        '--no-hls-split-discontinuity',
        dest='hls_split_discontinuity', action='store_false',
        help='Do not split HLS playlists into different formats at discontinuities such as ad breaks (default)')
    _extractor_arg_parser = lambda key, vals='': (key.strip().lower().replace('-', '_'), [
        val.replace(r'\,', ',').strip() for val in re.split(r'(?<!\\),', vals)])
        '--extractor-args',
        metavar='IE_KEY:ARGS', dest='extractor_args', default={}, type='str',
            'process': lambda val: dict(
                _extractor_arg_parser(*arg.split('=', 1)) for arg in val.split(';')),
            'Pass ARGS arguments to the IE_KEY extractor. See "EXTRACTOR ARGUMENTS" for details. '
            'You can use this option multiple times to give arguments for different extractors'))
    def _deprecated_option_callback(option, opt_str, value, parser):
        current = getattr(parser.values, '_deprecated_options', [])
        parser.values._deprecated_options = [*current, opt_str]
    deprecated_switches = [
        '--xattr-set-filesize',
        '--dump-user-agent',
        '--youtube-include-dash-manifest',
        '--no-youtube-skip-dash-manifest',
        '--youtube-skip-dash-manifest',
        '--no-youtube-include-dash-manifest',
        '--youtube-include-hls-manifest',
        '--no-youtube-skip-hls-manifest',
        '--youtube-skip-hls-manifest',
        '--no-youtube-include-hls-manifest',
        '--youtube-print-sig-code',
        '--sponskrub',
        '--no-sponskrub',
        '--sponskrub-cut',
        '--no-sponskrub-cut',
        '--sponskrub-force',
        '--no-sponskrub-force',
        '--prefer-avconv',
        '--no-prefer-ffmpeg',
        '--prefer-ffmpeg',
        '--no-prefer-avconv',
        '-C',  # this needs to remain deprecated until at least 2028
        '--call-home',
        '--no-call-home',
        '--include-ads',
        '--no-include-ads',
        '--write-annotations',
        '--no-write-annotations',
    deprecated_arguments = [
        '--sponskrub-location',
        '--sponskrub-args',
        '--cn-verification-proxy',
    for opt in deprecated_switches:
            opt, action='callback', callback=_deprecated_option_callback,
    for opt in deprecated_arguments:
            metavar='ARG', dest='_', type='str',
    parser.add_option_group(general)
    parser.add_option_group(network)
    parser.add_option_group(geo)
    parser.add_option_group(selection)
    parser.add_option_group(downloader)
    parser.add_option_group(filesystem)
    parser.add_option_group(thumbnail)
    parser.add_option_group(link)
    parser.add_option_group(verbosity)
    parser.add_option_group(workarounds)
    parser.add_option_group(video_format)
    parser.add_option_group(subtitles)
    parser.add_option_group(authentication)
    parser.add_option_group(postproc)
    parser.add_option_group(sponsorblock)
    parser.add_option_group(extractor)
def _hide_login_info(opts):
    deprecation_warning(f'"{__name__}._hide_login_info" is deprecated and may be removed '
                        'in a future version. Use "yt_dlp.utils.Config.hide_login_info" instead')
    return Config.hide_login_info(opts)
This file contains types for spotdl/downloader/web modules.
Options types have all the fields marked as required.
Settings types have all the fields marked as optional.
from typing import List, Optional, Union
    "SpotifyOptions",
    "DownloaderOptions",
    "WebOptions",
    "SpotDLOptions",
    "SpotifyOptionalOptions",
    "DownloaderOptionalOptions",
    "WebOptionalOptions",
    "SpotDLOptionalOptions",
class SpotifyOptions(TypedDict):
    Options used for initializing the Spotify client.
    client_id: str
    auth_token: Optional[str]
    user_auth: bool
    headless: bool
    cache_path: str
    no_cache: bool
    use_cache_file: bool
class DownloaderOptions(TypedDict):
    Options used for initializing the Downloader.
    audio_providers: List[str]
    lyrics_providers: List[str]
    genius_token: str
    playlist_numbering: bool
    playlist_retain_track_cover: bool
    scan_for_songs: bool
    m3u: Optional[str]
    overwrite: str
    search_query: Optional[str]
    ffmpeg: str
    bitrate: Optional[Union[str, int]]
    ffmpeg_args: Optional[str]
    format: str
    save_file: Optional[str]
    filter_results: bool
    album_type: Optional[str]
    threads: int
    cookie_file: Optional[str]
    restrict: Optional[str]
    print_errors: bool
    sponsor_block: bool
    preload: bool
    archive: Optional[str]
    load_config: bool
    log_level: str
    simple_tui: bool
    fetch_albums: bool
    id3_separator: str
    ytm_data: bool
    add_unavailable: bool
    generate_lrc: bool
    force_update_metadata: bool
    only_verified_results: bool
    sync_without_deleting: bool
    max_filename_length: Optional[int]
    yt_dlp_args: Optional[str]
    detect_formats: Optional[List[str]]
    save_errors: Optional[str]
    ignore_albums: Optional[List[str]]
    proxy: Optional[str]
    skip_explicit: Optional[bool]
    log_format: Optional[str]
    redownload: Optional[bool]
    skip_album_art: Optional[bool]
    create_skip_file: Optional[bool]
    respect_skip_file: Optional[bool]
    sync_remove_lrc: Optional[bool]
class WebOptions(TypedDict):
    Options used for initializing the Web server.
    web_use_output_dir: bool
    port: int
    host: str
    keep_alive: bool
    enable_tls: bool
    key_file: Optional[str]
    cert_file: Optional[str]
    ca_file: Optional[str]
    allowed_origins: Optional[List[str]]
    keep_sessions: bool
    force_update_gui: bool
    web_gui_repo: Optional[str]
    web_gui_location: Optional[str]
class SpotDLOptions(SpotifyOptions, DownloaderOptions, WebOptions):
    Options used for initializing the SpotDL client.
class SpotifyOptionalOptions(TypedDict, total=False):
class DownloaderOptionalOptions(TypedDict, total=False):
class WebOptionalOptions(TypedDict, total=False):
    allowed_origins: Optional[str]
class SpotDLOptionalOptions(
    SpotifyOptionalOptions, DownloaderOptionalOptions, WebOptionalOptions
    This type is modified to not require all the fields.
import os, tempfile, optparse, sys, re
from waflib import Logs, Utils, Context, Errors
options = optparse.Values()
envvars = []
lockfile = os.environ.get('WAFLOCK', '.lock-waf_%s_build' % sys.platform)
class opt_parser(optparse.OptionParser):
    def __init__(self, ctx, allow_unknown=False):
        optparse.OptionParser.__init__(
            add_help_option=False,
            version='%s %s (%s)' % (Context.WAFNAME, Context.WAFVERSION, Context.WAFREVISION)
        self.formatter.width = Logs.get_term_cols()
        self.ctx = ctx
        self.allow_unknown = allow_unknown
    def _process_args(self, largs, rargs, values):
        while rargs:
                optparse.OptionParser._process_args(self, largs, rargs, values)
            except (optparse.BadOptionError, optparse.AmbiguousOptionError) as e:
                if self.allow_unknown:
                    largs.append(e.opt_str)
                    self.error(str(e))
    def _process_long_opt(self, rargs, values):
            back = [] + rargs
                optparse.OptionParser._process_long_opt(self, rargs, values)
            except optparse.BadOptionError:
                    rargs.pop()
                rargs.extend(back)
                rargs.pop(0)
    def print_usage(self, file=None):
        return self.print_help(file)
    def get_usage(self):
        cmds_str = {}
        for cls in Context.classes:
            if not cls.cmd or cls.cmd == 'options' or cls.cmd.startswith('_'):
            s = cls.__doc__ or ''
            cmds_str[cls.cmd] = s
        if Context.g_module:
            for (k, v) in Context.g_module.__dict__.items():
                if k in ('options', 'init', 'shutdown'):
                if type(v) is type(Context.create_context):
                    if v.__doc__ and not k.startswith('_'):
                        cmds_str[k] = v.__doc__
        just = 0
        for k in cmds_str:
            just = max(just, len(k))
        lst = ['  %s: %s' % (k.ljust(just), v) for (k, v) in cmds_str.items()]
        lst.sort()
        ret = '\n'.join(lst)
        return '''%s [commands] [options]
Main commands (example: ./%s build -j4)
''' % (Context.WAFNAME, Context.WAFNAME, ret)
class OptionsContext(Context.Context):
    cmd = 'options'
    fun = 'options'
        super(OptionsContext, self).__init__(**kw)
        self.parser = opt_parser(self)
        self.option_groups = {}
        jobs = self.jobs()
        p = self.add_option
        color = os.environ.get('NOCOLOR', '') and 'no' or 'auto'
        if os.environ.get('CLICOLOR', '') == '0':
            color = 'no'
        elif os.environ.get('CLICOLOR_FORCE', '') == '1':
            color = 'yes'
        p(
            dest='colors',
            default=color,
            action='store',
            help='whether to use colors (yes/no/auto) [default: auto]',
            choices=('yes', 'no', 'auto')
        p('-j', '--jobs', dest='jobs', default=jobs, type='int', help='amount of parallel jobs (%r)' % jobs)
        p('-k', '--keep', dest='keep', default=0, action='count', help='continue despite errors (-kk to try harder)')
            dest='verbose',
            action='count',
            help='verbosity level -v -vv or -vvv [default: 0]'
        p('--zones', dest='zones', default='', action='store', help='debugging zones (task_gen, deps, tasks, etc)')
        p('--profile', dest='profile', default=0, action='store_true', help=optparse.SUPPRESS_HELP)
        p('--pdb', dest='pdb', default=0, action='store_true', help=optparse.SUPPRESS_HELP)
        p('-h', '--help', dest='whelp', default=0, action='store_true', help="show this help message and exit")
        gr = self.add_option_group('Configuration options')
        self.option_groups['configure options'] = gr
        gr.add_option('-o', '--out', action='store', default='', help='build dir for the project', dest='out')
        gr.add_option('-t', '--top', action='store', default='', help='src dir for the project', dest='top')
        gr.add_option(
            '--no-lock-in-run',
            default=os.environ.get('NO_LOCK_IN_RUN', ''),
            help=optparse.SUPPRESS_HELP,
            dest='no_lock_in_run'
            '--no-lock-in-out',
            default=os.environ.get('NO_LOCK_IN_OUT', ''),
            dest='no_lock_in_out'
            '--no-lock-in-top',
            default=os.environ.get('NO_LOCK_IN_TOP', ''),
            dest='no_lock_in_top'
        default_prefix = getattr(Context.g_module, 'default_prefix', os.environ.get('PREFIX'))
        if not default_prefix:
            if Utils.unversioned_sys_platform() == 'win32':
                d = tempfile.gettempdir()
                default_prefix = d[0].upper() + d[1:]
                default_prefix = '/usr/local/'
            '--prefix',
            dest='prefix',
            default=default_prefix,
            help='installation prefix [default: %r]' % default_prefix
        gr.add_option('--bindir', dest='bindir', help='bindir')
        gr.add_option('--libdir', dest='libdir', help='libdir')
        gr = self.add_option_group('Build and installation options')
        self.option_groups['build and install options'] = gr
            dest='progress_bar',
            help='-p: progress bar; -pp: ide output'
            '--targets', dest='targets', default='', action='store', help='task generators, e.g. "target1,target2"'
        gr = self.add_option_group('Step options')
        self.option_groups['step options'] = gr
            '--files',
            dest='files',
            default='',
            help='files to process, by regexp, e.g. "*/main.c,*/test/main.o"'
        default_destdir = os.environ.get('DESTDIR', '')
        gr = self.add_option_group('Installation and uninstallation options')
        self.option_groups['install/uninstall options'] = gr
            '--destdir',
            help='installation root [default: %r]' % default_destdir,
            default=default_destdir,
            dest='destdir'
        gr.add_option('-f', '--force', dest='force', default=False, action='store_true', help='force file installation')
            '--distcheck-args', metavar='ARGS', help='arguments to pass to distcheck', default=None, action='store'
    def jobs(self):
        count = int(os.environ.get('JOBS', 0))
        if count < 1:
            if 'NUMBER_OF_PROCESSORS' in os.environ:
                count = int(os.environ.get('NUMBER_OF_PROCESSORS', 1))
                if hasattr(os, 'sysconf_names'):
                    if 'SC_NPROCESSORS_ONLN' in os.sysconf_names:
                        count = int(os.sysconf('SC_NPROCESSORS_ONLN'))
                    elif 'SC_NPROCESSORS_CONF' in os.sysconf_names:
                        count = int(os.sysconf('SC_NPROCESSORS_CONF'))
                if not count and os.name not in ('nt', 'java'):
                        tmp = self.cmd_and_log(['sysctl', '-n', 'hw.ncpu'], quiet=0)
                        if re.match('^[0-9]+$', tmp):
                            count = int(tmp)
        elif count > 1024:
            count = 1024
        return count
    def add_option(self, *k, **kw):
        return self.parser.add_option(*k, **kw)
    def add_option_group(self, *k, **kw):
            gr = self.option_groups[k[0]]
            gr = self.parser.add_option_group(*k, **kw)
        self.option_groups[k[0]] = gr
        return gr
    def get_option_group(self, opt_str):
            return self.option_groups[opt_str]
            for group in self.parser.option_groups:
                if group.title == opt_str:
                    return group
    def sanitize_path(self, path, cwd=None):
        if not cwd:
            cwd = Context.launch_dir
        p = os.path.expanduser(path)
        p = os.path.join(cwd, p)
        p = os.path.normpath(p)
        p = os.path.abspath(p)
        return p
    def parse_cmd_args(self, _args=None, cwd=None, allow_unknown=False):
        self.parser.allow_unknown = allow_unknown
        (options, leftover_args) = self.parser.parse_args(args=_args)
        for arg in leftover_args:
            if '=' in arg:
                envvars.append(arg)
            elif arg != 'options':
                commands.append(arg)
        if options.jobs < 1:
            options.jobs = 1
        for name in 'top out destdir prefix bindir libdir'.split():
            if getattr(options, name, None):
                path = self.sanitize_path(getattr(options, name), cwd)
                setattr(options, name, path)
        return options, commands, envvars
    def init_module_vars(self, arg_options, arg_commands, arg_envvars):
        options.__dict__.clear()
        del commands[:]
        del envvars[:]
        options.__dict__.update(arg_options.__dict__)
        commands.extend(arg_commands)
        envvars.extend(arg_envvars)
        for var in envvars:
            (name, value) = var.split('=', 1)
            os.environ[name.strip()] = value
    def init_logs(self, options, commands, envvars):
        Logs.verbose = options.verbose
        if options.verbose >= 1:
            self.load('errcheck')
        colors = {'yes': 2, 'auto': 1, 'no': 0}[options.colors]
        Logs.enable_colors(colors)
        if options.zones:
            Logs.zones = options.zones.split(',')
            if not Logs.verbose:
                Logs.verbose = 1
        elif Logs.verbose > 0:
            Logs.zones = ['runner']
        if Logs.verbose > 2:
            Logs.zones = ['*']
    def parse_args(self, _args=None):
        options, commands, envvars = self.parse_cmd_args()
        self.init_logs(options, commands, envvars)
        self.init_module_vars(options, commands, envvars)
        super(OptionsContext, self).execute()
        self.parse_args()
        Utils.alloc_process_pool(options.jobs)
from functools import partial, update_wrapper
from urllib.parse import parse_qsl
from urllib.parse import quote as urlquote
from urllib.parse import urlsplit
from django.contrib import messages
from django.contrib.admin import helpers, widgets
from django.contrib.admin.checks import (
    BaseModelAdminChecks,
    InlineModelAdminChecks,
    ModelAdminChecks,
from django.contrib.admin.exceptions import DisallowedModelAdminToField, NotRegistered
from django.contrib.admin.templatetags.admin_urls import add_preserved_filters
    NestedObjects,
    construct_change_message,
    display_for_value,
    get_deleted_objects,
    lookup_spawns_duplicates,
    model_format_dict,
    model_ngettext,
    unquote,
from django.contrib.admin.widgets import AutocompleteSelect, AutocompleteSelectMultiple
from django.contrib.auth import get_permission_codename
from django.core.exceptions import (
    FieldDoesNotExist,
    FieldError,
    PermissionDenied,
from django.core.paginator import Paginator
from django.db import models, router, transaction
from django.db.models.constants import LOOKUP_SEP
from django.forms.formsets import DELETION_FIELD_NAME, all_valid
from django.forms.models import (
    BaseInlineFormSet,
    inlineformset_factory,
    modelform_defines_fields,
    modelform_factory,
    modelformset_factory,
from django.forms.widgets import CheckboxSelectMultiple, SelectMultiple
from django.http import HttpResponseRedirect
from django.http.response import HttpResponseBase
from django.template.response import SimpleTemplateResponse, TemplateResponse
from django.urls import reverse
from django.utils.decorators import method_decorator
from django.utils.html import format_html
from django.utils.http import urlencode
from django.utils.text import (
    capfirst,
    format_lazy,
    get_text_list,
    smart_split,
    unescape_string_literal,
from django.utils.translation import gettext as _
from django.utils.translation import ngettext
from django.views.decorators.csrf import csrf_protect
from django.views.generic import RedirectView
IS_POPUP_VAR = "_popup"
SOURCE_MODEL_VAR = "_source_model"
TO_FIELD_VAR = "_to_field"
IS_FACETS_VAR = "_facets"
EMPTY_VALUE_STRING = "-"
class ShowFacets(enum.Enum):
    NEVER = "NEVER"
    ALLOW = "ALLOW"
    ALWAYS = "ALWAYS"
HORIZONTAL, VERTICAL = 1, 2
def get_content_type_for_model(obj):
    # Since this module gets imported in the application's root package,
    # it cannot import models from other applications at the module level.
    return ContentType.objects.get_for_model(obj, for_concrete_model=False)
def get_ul_class(radio_style):
    return "radiolist" if radio_style == VERTICAL else "radiolist inline"
class IncorrectLookupParameters(Exception):
# Defaults for formfield_overrides. ModelAdmin subclasses can change this
# by adding to ModelAdmin.formfield_overrides.
FORMFIELD_FOR_DBFIELD_DEFAULTS = {
    models.DateTimeField: {
        "form_class": forms.SplitDateTimeField,
        "widget": widgets.AdminSplitDateTime,
    models.DateField: {"widget": widgets.AdminDateWidget},
    models.TimeField: {"widget": widgets.AdminTimeWidget},
    models.TextField: {"widget": widgets.AdminTextareaWidget},
    models.URLField: {"widget": widgets.AdminURLFieldWidget},
    models.IntegerField: {"widget": widgets.AdminIntegerFieldWidget},
    models.BigIntegerField: {"widget": widgets.AdminBigIntegerFieldWidget},
    models.CharField: {"widget": widgets.AdminTextInputWidget},
    models.ImageField: {"widget": widgets.AdminFileWidget},
    models.FileField: {"widget": widgets.AdminFileWidget},
    models.EmailField: {"widget": widgets.AdminEmailInputWidget},
    models.UUIDField: {"widget": widgets.AdminUUIDInputWidget},
csrf_protect_m = method_decorator(csrf_protect)
class BaseModelAdmin(metaclass=forms.MediaDefiningClass):
    """Functionality common to both ModelAdmin and InlineAdmin."""
    autocomplete_fields = ()
    raw_id_fields = ()
    fields = None
    exclude = None
    fieldsets = None
    form = forms.ModelForm
    filter_vertical = ()
    filter_horizontal = ()
    radio_fields = {}
    formfield_overrides = {}
    ordering = None
    sortable_by = None
    view_on_site = True
    show_full_result_count = True
    checks_class = BaseModelAdminChecks
    def check(self, **kwargs):
        return self.checks_class().check(self, **kwargs)
        # Merge FORMFIELD_FOR_DBFIELD_DEFAULTS with the formfield_overrides
        # rather than simply overwriting.
        overrides = copy.deepcopy(FORMFIELD_FOR_DBFIELD_DEFAULTS)
        for k, v in self.formfield_overrides.items():
            overrides.setdefault(k, {}).update(v)
        self.formfield_overrides = overrides
    def formfield_for_dbfield(self, db_field, request, **kwargs):
        Hook for specifying the form Field instance for a given database Field
        If kwargs are given, they're passed to the form Field's constructor.
        # If the field specifies choices, we don't need to look for special
        # admin widgets - we just need to use a select widget of some kind.
        if db_field.choices:
            return self.formfield_for_choice_field(db_field, request, **kwargs)
        # ForeignKey or ManyToManyFields
        if isinstance(db_field, (models.ForeignKey, models.ManyToManyField)):
            # Combine the field kwargs with any options for
            # formfield_overrides. Make sure the passed in **kwargs override
            # anything in formfield_overrides because **kwargs is more
            # specific, and should always win.
            if db_field.__class__ in self.formfield_overrides:
                kwargs = {**self.formfield_overrides[db_field.__class__], **kwargs}
            # Get the correct formfield.
            if isinstance(db_field, models.ForeignKey):
                formfield = self.formfield_for_foreignkey(db_field, request, **kwargs)
            elif isinstance(db_field, models.ManyToManyField):
                formfield = self.formfield_for_manytomany(db_field, request, **kwargs)
            # For non-raw_id fields, wrap the widget with a wrapper that adds
            # extra HTML -- the "add other" interface -- to the end of the
            # rendered output. formfield can be None if it came from a
            # OneToOneField with parent_link=True or a M2M intermediary.
            if formfield and db_field.name not in self.raw_id_fields:
                    related_modeladmin = self.admin_site.get_model_admin(
                        db_field.remote_field.model
                except NotRegistered:
                    wrapper_kwargs = {}
                    wrapper_kwargs = {
                        "can_add_related": related_modeladmin.has_add_permission(
                        "can_change_related": related_modeladmin.has_change_permission(
                        "can_delete_related": related_modeladmin.has_delete_permission(
                        "can_view_related": related_modeladmin.has_view_permission(
                formfield.widget = widgets.RelatedFieldWidgetWrapper(
                    formfield.widget,
                    db_field.remote_field,
                    self.admin_site,
                    **wrapper_kwargs,
            return formfield
        # If we've got overrides for the formfield defined, use 'em. **kwargs
        # passed to formfield_for_dbfield override the defaults.
        for klass in db_field.__class__.mro():
            if klass in self.formfield_overrides:
                kwargs = {**copy.deepcopy(self.formfield_overrides[klass]), **kwargs}
                return db_field.formfield(**kwargs)
        # For any other type of field, just call its formfield() method.
    def formfield_for_choice_field(self, db_field, request, **kwargs):
        Get a form Field for a database Field that has declared choices.
        # If the field is named as a radio_field, use a RadioSelect
        if db_field.name in self.radio_fields:
            # Avoid stomping on custom widget/choices arguments.
            if "widget" not in kwargs:
                kwargs["widget"] = widgets.AdminRadioSelect(
                    attrs={
                        "class": get_ul_class(self.radio_fields[db_field.name]),
            if "choices" not in kwargs:
                kwargs["choices"] = db_field.get_choices(
                    include_blank=db_field.blank, blank_choice=[("", _("None"))]
    def get_field_queryset(self, db, db_field, request):
        If the ModelAdmin specifies ordering, the queryset should respect that
        ordering. Otherwise don't specify the queryset, let the field decide
        (return None in that case).
            related_admin = self.admin_site.get_model_admin(db_field.remote_field.model)
            ordering = related_admin.get_ordering(request)
            if ordering is not None and ordering != ():
                return db_field.remote_field.model._default_manager.using(db).order_by(
                    *ordering
    def formfield_for_foreignkey(self, db_field, request, **kwargs):
        Get a form Field for a ForeignKey.
        db = kwargs.get("using")
            if db_field.name in self.get_autocomplete_fields(request):
                kwargs["widget"] = AutocompleteSelect(
                    db_field, self.admin_site, using=db
            elif db_field.name in self.raw_id_fields:
                kwargs["widget"] = widgets.ForeignKeyRawIdWidget(
                    db_field.remote_field, self.admin_site, using=db
            elif db_field.name in self.radio_fields:
                kwargs["empty_label"] = (
                    kwargs.get("empty_label", _("None")) if db_field.blank else None
        if "queryset" not in kwargs:
            queryset = self.get_field_queryset(db, db_field, request)
            if queryset is not None:
                kwargs["queryset"] = queryset
    def formfield_for_manytomany(self, db_field, request, **kwargs):
        Get a form Field for a ManyToManyField.
        # If it uses an intermediary model that isn't auto created, don't show
        # a field in admin.
        if not db_field.remote_field.through._meta.auto_created:
            autocomplete_fields = self.get_autocomplete_fields(request)
            if db_field.name in autocomplete_fields:
                kwargs["widget"] = AutocompleteSelectMultiple(
                    db_field,
                    using=db,
                kwargs["widget"] = widgets.ManyToManyRawIdWidget(
            elif db_field.name in [*self.filter_vertical, *self.filter_horizontal]:
                kwargs["widget"] = widgets.FilteredSelectMultiple(
                    db_field.verbose_name, db_field.name in self.filter_vertical
        form_field = db_field.formfield(**kwargs)
            isinstance(form_field.widget, SelectMultiple)
            and form_field.widget.allow_multiple_selected
            and not isinstance(
                form_field.widget, (CheckboxSelectMultiple, AutocompleteSelectMultiple)
            msg = _(
                "Hold down “Control”, or “Command” on a Mac, to select more than one."
            help_text = form_field.help_text
            form_field.help_text = (
                format_lazy("{} {}", help_text, msg) if help_text else msg
        return form_field
    def get_autocomplete_fields(self, request):
        Return a list of ForeignKey and/or ManyToMany fields which should use
        an autocomplete widget.
        return self.autocomplete_fields
    def get_view_on_site_url(self, obj=None):
        if obj is None or not self.view_on_site:
        if callable(self.view_on_site):
            return self.view_on_site(obj)
        elif hasattr(obj, "get_absolute_url"):
            # use the ContentType lookup if view_on_site is True
            return reverse(
                "admin:view_on_site",
                kwargs={
                    "content_type_id": get_content_type_for_model(obj).pk,
                    "object_id": obj.pk,
                current_app=self.admin_site.name,
    def get_empty_value_display(self):
        Return the empty_value_display set on ModelAdmin or AdminSite.
            return mark_safe(self.empty_value_display)
            return mark_safe(self.admin_site.empty_value_display)
    def get_exclude(self, request, obj=None):
        Hook for specifying exclude.
        return self.exclude
    def get_fields(self, request, obj=None):
        Hook for specifying fields.
        if self.fields:
            return self.fields
        # _get_form_for_get_fields() is implemented in subclasses.
        form = self._get_form_for_get_fields(request, obj)
        return [*form.base_fields, *self.get_readonly_fields(request, obj)]
    def get_fieldsets(self, request, obj=None):
        Hook for specifying fieldsets.
        if self.fieldsets:
            return self.fieldsets
        return [(None, {"fields": self.get_fields(request, obj)})]
    def get_inlines(self, request, obj):
        """Hook for specifying custom inlines."""
        return self.inlines
    def get_ordering(self, request):
        Hook for specifying field ordering.
        return self.ordering or ()  # otherwise we might try to *None, which is bad ;)
    def get_readonly_fields(self, request, obj=None):
        Hook for specifying custom readonly fields.
        return self.readonly_fields
    def get_prepopulated_fields(self, request, obj=None):
        Hook for specifying custom prepopulated fields.
        return self.prepopulated_fields
    def get_queryset(self, request):
        Return a QuerySet of all model instances that can be edited by the
        admin site. This is used by changelist_view.
        qs = self.model._default_manager.get_queryset()
        # TODO: this should be handled by some parameter to the ChangeList.
        ordering = self.get_ordering(request)
        if ordering:
            qs = qs.order_by(*ordering)
        return qs
    def get_sortable_by(self, request):
        """Hook for specifying which fields can be sorted in the changelist."""
            self.sortable_by
            if self.sortable_by is not None
            else self.get_list_display(request)
    def lookup_allowed(self, lookup, value, request):
        from django.contrib.admin.filters import SimpleListFilter
        model = self.model
        # Check FKey lookups that are allowed, so that popups produced by
        # ForeignKeyRawIdWidget, on the basis of ForeignKey.limit_choices_to,
        # are allowed to work.
        for fk_lookup in model._meta.related_fkey_lookups:
            # As ``limit_choices_to`` can be a callable, invoke it here.
            if callable(fk_lookup):
                fk_lookup = fk_lookup()
            if (lookup, value) in widgets.url_params_from_lookup_dict(
                fk_lookup
            ).items():
        relation_parts = []
        prev_field = None
        parts = lookup.split(LOOKUP_SEP)
                field = model._meta.get_field(part)
            except FieldDoesNotExist:
                # Lookups on nonexistent fields are ok, since they're ignored
                # later.
            if not prev_field or (
                prev_field.is_relation
                and field not in model._meta.parents.values()
                and field is not model._meta.auto_field
                and (
                    model._meta.auto_field is None
                    or part not in getattr(prev_field, "to_fields", [])
                and (field.is_relation or not field.primary_key)
                relation_parts.append(part)
            if not getattr(field, "path_infos", None):
                # This is not a relational field, so further parts
                # must be transforms.
            prev_field = field
            model = field.path_infos[-1].to_opts.model
        if len(relation_parts) <= 1:
            # Either a local field filter, or no fields at all.
        valid_lookups = {self.date_hierarchy}
        for filter_item in self.get_list_filter(request):
            if isinstance(filter_item, type) and issubclass(
                filter_item, SimpleListFilter
                valid_lookups.add(filter_item.parameter_name)
            elif isinstance(filter_item, (list, tuple)):
                valid_lookups.add(filter_item[0])
                valid_lookups.add(filter_item)
        # Is it a valid relational lookup?
        return not {
            LOOKUP_SEP.join(relation_parts),
            LOOKUP_SEP.join([*relation_parts, part]),
        }.isdisjoint(valid_lookups)
    def to_field_allowed(self, request, to_field):
        Return True if the model associated with this admin should be
        allowed to be referenced by the specified field.
            field = self.opts.get_field(to_field)
        # Always allow referencing the primary key since it's already possible
        # to get this information from the change view URL.
        if field.primary_key:
        # Allow reverse relationships to models defining m2m fields if they
        # target the specified field.
        for many_to_many in self.opts.many_to_many:
            if many_to_many.m2m_target_field_name() == to_field:
        # Make sure at least one of the models registered for this site
        # references this field through a FK or a M2M relationship.
        registered_models = set()
        for model, admin in self.admin_site._registry.items():
            registered_models.add(model)
            for inline in admin.inlines:
                registered_models.add(inline.model)
        related_objects = (
            for f in self.opts.get_fields(include_hidden=True)
            if (f.auto_created and not f.concrete)
        for related_object in related_objects:
            related_model = related_object.related_model
            remote_field = related_object.field.remote_field
                any(issubclass(model, related_model) for model in registered_models)
                and hasattr(remote_field, "get_related_field")
                and remote_field.get_related_field() == field
    def has_add_permission(self, request):
        Return True if the given request has permission to add an object.
        Can be overridden by the user in subclasses.
        opts = self.opts
        codename = get_permission_codename("add", opts)
        return request.user.has_perm("%s.%s" % (opts.app_label, codename))
    def has_change_permission(self, request, obj=None):
        Return True if the given request has permission to change the given
        Django model instance, the default implementation doesn't examine the
        `obj` parameter.
        Can be overridden by the user in subclasses. In such case it should
        return True if the given request has permission to change the `obj`
        model instance. If `obj` is None, this should return True if the given
        request has permission to change *any* object of the given type.
        codename = get_permission_codename("change", opts)
    def has_delete_permission(self, request, obj=None):
        Return True if the given request has permission to delete the given
        return True if the given request has permission to delete the `obj`
        request has permission to delete *any* object of the given type.
        codename = get_permission_codename("delete", opts)
    def has_view_permission(self, request, obj=None):
        Return True if the given request has permission to view the given
        Django model instance. The default implementation doesn't examine the
        If overridden by the user in subclasses, it should return True if the
        given request has permission to view the `obj` model instance. If `obj`
        is None, it should return True if the request has permission to view
        any object of the given type.
        codename_view = get_permission_codename("view", opts)
        codename_change = get_permission_codename("change", opts)
        return request.user.has_perm(
            "%s.%s" % (opts.app_label, codename_view)
        ) or request.user.has_perm("%s.%s" % (opts.app_label, codename_change))
    def has_view_or_change_permission(self, request, obj=None):
        return self.has_view_permission(request, obj) or self.has_change_permission(
            request, obj
    def has_module_permission(self, request):
        Return True if the given request has any permission in the given
        app label.
        return True if the given request has permission to view the module on
        the admin index page and access the module's index page. Overriding it
        does not restrict access to the add, change or delete views. Use
        `ModelAdmin.has_(add|change|delete)_permission` for that.
        return request.user.has_module_perms(self.opts.app_label)
class ModelAdmin(BaseModelAdmin):
    """Encapsulate all admin options and functionality for a given model."""
    list_display = ("__str__",)
    list_display_links = ()
    list_filter = ()
    list_select_related = False
    list_per_page = 100
    list_max_show_all = 200
    list_editable = ()
    search_fields = ()
    search_help_text = None
    date_hierarchy = None
    save_as = False
    save_as_continue = True
    save_on_top = False
    paginator = Paginator
    preserve_filters = True
    show_facets = ShowFacets.ALLOW
    inlines = ()
    # Custom templates (designed to be over-ridden in subclasses)
    add_form_template = None
    change_form_template = None
    change_list_template = None
    delete_confirmation_template = None
    delete_selected_confirmation_template = None
    object_history_template = None
    popup_response_template = None
    # Actions
    actions = ()
    action_form = helpers.ActionForm
    actions_on_top = True
    actions_on_bottom = False
    actions_selection_counter = True
    checks_class = ModelAdminChecks
    def __init__(self, model, admin_site):
        self.opts = model._meta
        self.admin_site = admin_site
        return "%s.%s" % (self.opts.app_label, self.__class__.__name__)
            f"<{self.__class__.__qualname__}: model={self.model.__qualname__} "
            f"site={self.admin_site!r}>"
    def get_inline_instances(self, request, obj=None):
        inline_instances = []
        for inline_class in self.get_inlines(request, obj):
            inline = inline_class(self.model, self.admin_site)
            if request:
                if not (
                    inline.has_view_or_change_permission(request, obj)
                    or inline.has_add_permission(request, obj)
                    or inline.has_delete_permission(request, obj)
                if not inline.has_add_permission(request, obj):
                    inline.max_num = 0
            inline_instances.append(inline)
        return inline_instances
    def get_urls(self):
        from django.urls import path
        def wrap(view):
                return self.admin_site.admin_view(view)(*args, **kwargs)
            wrapper.model_admin = self
            return update_wrapper(wrapper, view)
        info = self.opts.app_label, self.opts.model_name
            path("", wrap(self.changelist_view), name="%s_%s_changelist" % info),
            path("add/", wrap(self.add_view), name="%s_%s_add" % info),
            path(
                "<path:object_id>/history/",
                wrap(self.history_view),
                name="%s_%s_history" % info,
                "<path:object_id>/delete/",
                wrap(self.delete_view),
                name="%s_%s_delete" % info,
                "<path:object_id>/change/",
                wrap(self.change_view),
                name="%s_%s_change" % info,
            # For backwards compatibility (was the change url before 1.9)
                "<path:object_id>/",
                wrap(
                    RedirectView.as_view(
                        pattern_name="%s:%s_%s_change" % (self.admin_site.name, *info)
    def urls(self):
        return self.get_urls()
        extra = "" if settings.DEBUG else ".min"
        js = [
            "vendor/jquery/jquery%s.js" % extra,
            "jquery.init.js",
            "core.js",
            "admin/RelatedObjectLookups.js",
            "actions.js",
            "urlify.js",
            "prepopulate.js",
            "vendor/xregexp/xregexp%s.js" % extra,
        return forms.Media(js=["admin/js/%s" % url for url in js])
    def get_model_perms(self, request):
        Return a dict of all perms for this model. This dict has the keys
        ``add``, ``change``, ``delete``, and ``view`` mapping to the True/False
        for each of those actions.
            "add": self.has_add_permission(request),
            "change": self.has_change_permission(request),
            "delete": self.has_delete_permission(request),
            "view": self.has_view_permission(request),
    def _get_form_for_get_fields(self, request, obj):
        return self.get_form(request, obj, fields=None)
    def get_form(self, request, obj=None, change=False, **kwargs):
        Return a Form class for use in the admin add view. This is used by
        add_view and change_view.
        if "fields" in kwargs:
            fields = kwargs.pop("fields")
            fields = flatten_fieldsets(self.get_fieldsets(request, obj))
        excluded = self.get_exclude(request, obj)
        exclude = [] if excluded is None else list(excluded)
        readonly_fields = self.get_readonly_fields(request, obj)
        exclude.extend(readonly_fields)
        # Exclude all fields if it's a change form and the user doesn't have
        # the change permission.
            change
            and hasattr(request, "user")
            and not self.has_change_permission(request, obj)
            exclude.extend(fields)
        if excluded is None and hasattr(self.form, "_meta") and self.form._meta.exclude:
            # Take the custom ModelForm's Meta.exclude into account only if the
            # ModelAdmin doesn't define its own.
            exclude.extend(self.form._meta.exclude)
        # if exclude is an empty list we pass None to be consistent with the
        # default on modelform_factory
        exclude = exclude or None
        # Remove declared form fields which are in readonly_fields.
        new_attrs = dict.fromkeys(
            f for f in readonly_fields if f in self.form.declared_fields
        form = type(self.form.__name__, (self.form,), new_attrs)
        defaults = {
            "form": form,
            "fields": fields,
            "exclude": exclude,
            "formfield_callback": partial(self.formfield_for_dbfield, request=request),
        if defaults["fields"] is None and not modelform_defines_fields(
            defaults["form"]
            defaults["fields"] = forms.ALL_FIELDS
            return modelform_factory(self.model, **defaults)
        except FieldError as e:
            raise FieldError(
                "%s. Check fields/fieldsets/exclude attributes of class %s."
                % (e, self.__class__.__name__)
    def get_changelist(self, request, **kwargs):
        Return the ChangeList class for use on the changelist page.
        from django.contrib.admin.views.main import ChangeList
        return ChangeList
    def get_changelist_instance(self, request):
        Return a `ChangeList` instance based on `request`. May raise
        `IncorrectLookupParameters`.
        list_display = self.get_list_display(request)
        list_display_links = self.get_list_display_links(request, list_display)
        # Add the action checkboxes if any actions are available.
        if self.get_actions(request):
            list_display = ["action_checkbox", *list_display]
        sortable_by = self.get_sortable_by(request)
        ChangeList = self.get_changelist(request)
        return ChangeList(
            self.model,
            list_display,
            list_display_links,
            self.get_list_filter(request),
            self.date_hierarchy,
            self.get_search_fields(request),
            self.get_list_select_related(request),
            self.list_per_page,
            self.list_max_show_all,
            self.list_editable,
            sortable_by,
            self.search_help_text,
    def get_object(self, request, object_id, from_field=None):
        Return an instance matching the field and value provided, the primary
        key is used if no field is provided. Return ``None`` if no match is
        found or the object_id fails validation.
        queryset = self.get_queryset(request)
        model = queryset.model
        field = (
            model._meta.pk if from_field is None else model._meta.get_field(from_field)
            object_id = field.to_python(object_id)
            return queryset.get(**{field.name: object_id})
        except (model.DoesNotExist, ValidationError, ValueError):
    def get_changelist_form(self, request, **kwargs):
        Return a Form class for use in the Formset on the changelist page.
        if defaults.get("fields") is None and not modelform_defines_fields(
            defaults.get("form")
    def get_changelist_formset(self, request, **kwargs):
        Return a FormSet class for use on the changelist page if list_editable
        is used.
        return modelformset_factory(
            self.get_changelist_form(request),
            extra=0,
            fields=self.list_editable,
            **defaults,
    def get_formsets_with_inlines(self, request, obj=None):
        Yield formsets and the corresponding inlines.
        for inline in self.get_inline_instances(request, obj):
            yield inline.get_formset(request, obj), inline
    def get_paginator(
        self, request, queryset, per_page, orphans=0, allow_empty_first_page=True
        return self.paginator(queryset, per_page, orphans, allow_empty_first_page)
    def log_addition(self, request, obj, message):
        Log that an object has been successfully added.
        The default implementation creates an admin LogEntry object.
        from django.contrib.admin.models import ADDITION, LogEntry
        return LogEntry.objects.log_actions(
            user_id=request.user.pk,
            queryset=[obj],
            action_flag=ADDITION,
            change_message=message,
            single_object=True,
    def log_change(self, request, obj, message):
        Log that an object has been successfully changed.
        from django.contrib.admin.models import CHANGE, LogEntry
            action_flag=CHANGE,
    def log_deletions(self, request, queryset):
        Log that objects will be deleted. Note that this method must be called
        before the deletion.
        The default implementation creates admin LogEntry objects.
        from django.contrib.admin.models import DELETION, LogEntry
            queryset=queryset,
            action_flag=DELETION,
    def action_checkbox(self, obj):
        A list_display column containing a checkbox widget.
        attrs = {
            "class": "action-select",
            "aria-label": format_html(
                _("Select this object for an action - {}"), str(obj)
        checkbox = forms.CheckboxInput(attrs, lambda value: False)
        return checkbox.render(helpers.ACTION_CHECKBOX_NAME, str(obj.pk))
    def _get_action_description(func, name):
            return func.short_description
            return capfirst(name.replace("_", " "))
    def _get_base_actions(self):
        """Return the list of actions, prior to any request-based filtering."""
        actions = []
        base_actions = (self.get_action(action) for action in self.actions or [])
        # get_action might have returned None, so filter any of those out.
        base_actions = [action for action in base_actions if action]
        base_action_names = {name for _, name, _ in base_actions}
        # Gather actions from the admin site first
        for name, func in self.admin_site.actions:
            if name in base_action_names:
            description = self._get_action_description(func, name)
            actions.append((func, name, description))
        # Add actions from this ModelAdmin.
        actions.extend(base_actions)
    def _filter_actions_by_permissions(self, request, actions):
        """Filter out any actions that the user doesn't have access to."""
        filtered_actions = []
            callable = action[0]
            if not hasattr(callable, "allowed_permissions"):
                filtered_actions.append(action)
            permission_checks = (
                getattr(self, "has_%s_permission" % permission)
                for permission in callable.allowed_permissions
            if any(has_permission(request) for has_permission in permission_checks):
        return filtered_actions
    def get_actions(self, request):
        Return a dictionary mapping the names of all actions for this
        ModelAdmin to a tuple of (callable, name, description) for each action.
        # If self.actions is set to None that means actions are disabled on
        # this page.
        if self.actions is None or IS_POPUP_VAR in request.GET:
        actions = self._filter_actions_by_permissions(request, self._get_base_actions())
        return {name: (func, name, desc) for func, name, desc in actions}
    def get_action_choices(self, request, default_choices=models.BLANK_CHOICE_DASH):
        Return a list of choices for use in a form object. Each choice is a
        tuple (name, description).
        choices = [*default_choices]
        for func, name, description in self.get_actions(request).values():
            choice = (name, description % model_format_dict(self.opts))
            choices.append(choice)
        return choices
    def get_action(self, action):
        Return a given action from a parameter, which can either be a callable,
        or the name of a method on the ModelAdmin. Return is a tuple of
        (callable, name, description).
        # If the action is a callable, just use it.
        if callable(action):
            func = action
            action = action.__name__
        # Next, look for a method. Grab it off self.__class__ to get an unbound
        # method instead of a bound one; this ensures that the calling
        # conventions are the same for functions and methods.
        elif hasattr(self.__class__, action):
            func = getattr(self.__class__, action)
        # Finally, look for a named method on the admin site
                func = self.admin_site.get_action(action)
        description = self._get_action_description(func, action)
        return func, action, description
    def get_list_display(self, request):
        Return a sequence containing the fields to be displayed on the
        changelist.
        return self.list_display
    def get_list_display_links(self, request, list_display):
        Return a sequence containing the fields to be displayed as links
        on the changelist. The list_display parameter is the list of fields
        returned by get_list_display().
            self.list_display_links
            or self.list_display_links is None
            or not list_display
            return self.list_display_links
            # Use only the first item in list_display as link
            return list(list_display)[:1]
    def get_list_filter(self, request):
        Return a sequence containing the fields to be displayed as filters in
        the right sidebar of the changelist page.
        return self.list_filter
    def get_list_select_related(self, request):
        Return a list of fields to add to the select_related() part of the
        changelist items query.
        return self.list_select_related
    def get_search_fields(self, request):
        Return a sequence containing the fields to be searched whenever
        somebody submits a search query.
        return self.search_fields
    def get_search_results(self, request, queryset, search_term):
        Return a tuple containing a queryset to implement the search
        and a boolean indicating if the results may contain duplicates.
        # Apply keyword searches.
        def construct_search(field_name):
            Return a tuple of (lookup, field_to_validate).
            field_to_validate is set for non-text exact lookups so that
            invalid search terms can be skipped (preserving index usage).
            if field_name.startswith("^"):
                return "%s__istartswith" % field_name.removeprefix("^"), None
            elif field_name.startswith("="):
                return "%s__iexact" % field_name.removeprefix("="), None
            elif field_name.startswith("@"):
                return "%s__search" % field_name.removeprefix("@"), None
            # Use field_name if it includes a lookup.
            opts = queryset.model._meta
            lookup_fields = field_name.split(LOOKUP_SEP)
            # Go through the fields, following all relations.
            for path_part in lookup_fields:
                if path_part == "pk":
                    path_part = opts.pk.name
                    field = opts.get_field(path_part)
                    # Use valid query lookups.
                    if prev_field and prev_field.get_lookup(path_part):
                        if path_part == "exact" and not isinstance(
                            prev_field, (models.CharField, models.TextField)
                            # Use prev_field to validate the search term.
                            return field_name, prev_field
                        return field_name, None
                    if hasattr(field, "path_infos"):
                        # Update opts to follow the relation.
                        opts = field.path_infos[-1].to_opts
            # Otherwise, use the field with icontains.
            return "%s__icontains" % field_name, None
        may_have_duplicates = False
        search_fields = self.get_search_fields(request)
        if search_fields and search_term:
            orm_lookups = []
            for field in search_fields:
                orm_lookups.append(construct_search(str(field)))
            term_queries = []
            for bit in smart_split(search_term):
                if bit.startswith(('"', "'")) and bit[0] == bit[-1]:
                    bit = unescape_string_literal(bit)
                # Build term lookups, skipping values invalid for their field.
                bit_lookups = []
                for orm_lookup, validate_field in orm_lookups:
                    if validate_field is not None:
                        formfield = validate_field.formfield()
                            if formfield is not None:
                                value = formfield.to_python(bit)
                                # Fields like AutoField lack a form field.
                                value = validate_field.to_python(bit)
                        except ValidationError:
                            # Skip this lookup for invalid values.
                        value = bit
                    bit_lookups.append((orm_lookup, value))
                if bit_lookups:
                    or_queries = models.Q.create(bit_lookups, connector=models.Q.OR)
                    term_queries.append(or_queries)
                    # No valid lookups: add a filter that returns nothing.
                    term_queries.append(models.Q(pk__in=[]))
            if term_queries:
                queryset = queryset.filter(models.Q.create(term_queries))
            may_have_duplicates |= any(
                lookup_spawns_duplicates(self.opts, search_spec)
                for search_spec, _ in orm_lookups
        return queryset, may_have_duplicates
    def get_preserved_filters(self, request):
        Return the preserved filters querystring.
        match = request.resolver_match
        if self.preserve_filters and match:
            current_url = "%s:%s" % (match.app_name, match.url_name)
            changelist_url = "admin:%s_%s_changelist" % (
                self.opts.app_label,
                self.opts.model_name,
            if current_url == changelist_url:
                preserved_filters = request.GET.urlencode()
                preserved_filters = request.GET.get("_changelist_filters")
            if preserved_filters:
                return urlencode({"_changelist_filters": preserved_filters})
    def construct_change_message(self, request, form, formsets, add=False):
        Construct a JSON structure describing changes from a changed object.
        return construct_change_message(form, formsets, add)
    def message_user(
        self, request, message, level=messages.INFO, extra_tags="", fail_silently=False
        Send a message to the user. The default implementation
        posts a message using the django.contrib.messages backend.
        Exposes almost the same API as messages.add_message(), but accepts the
        positional arguments in a different order to maintain backwards
        compatibility. For convenience, it accepts the `level` argument as
        a string rather than the usual level number.
        if not isinstance(level, int):
            # attempt to get the level if passed a string
                level = getattr(messages.constants, level.upper())
                levels = messages.constants.DEFAULT_TAGS.values()
                levels_repr = ", ".join("`%s`" % level for level in levels)
                    "Bad message level string: `%s`. Possible values are: %s"
                    % (level, levels_repr)
        messages.add_message(
            request, level, message, extra_tags=extra_tags, fail_silently=fail_silently
    def save_form(self, request, form, change):
        Given a ModelForm return an unsaved instance. ``change`` is True if
        the object is being changed, and False if it's being added.
        return form.save(commit=False)
    def save_model(self, request, obj, form, change):
        Given a model instance save it to the database.
        obj.save()
    def delete_model(self, request, obj):
        Given a model instance delete it from the database.
        obj.delete()
    def delete_queryset(self, request, queryset):
        """Given a queryset, delete it from the database."""
        queryset.delete()
    def save_formset(self, request, form, formset, change):
        Given an inline formset save it to the database.
        formset.save()
    def save_related(self, request, form, formsets, change):
        Given the ``HttpRequest``, the parent ``ModelForm`` instance, the
        list of inline formsets and a boolean value based on whether the
        parent is being added or changed, save the related objects to the
        database. Note that at this point save_form() and save_model() have
        already been called.
        form.save_m2m()
        for formset in formsets:
            self.save_formset(request, form, formset, change=change)
    def render_change_form(
        self, request, context, add=False, change=False, form_url="", obj=None
        app_label = self.opts.app_label
        preserved_filters = self.get_preserved_filters(request)
        form_url = add_preserved_filters(
            {"preserved_filters": preserved_filters, "opts": self.opts}, form_url
        view_on_site_url = self.get_view_on_site_url(obj)
        has_editable_inline_admin_formsets = False
        for inline in context["inline_admin_formsets"]:
                inline.has_add_permission
                or inline.has_change_permission
                or inline.has_delete_permission
                has_editable_inline_admin_formsets = True
        context.update(
                "add": add,
                "change": change,
                "has_view_permission": self.has_view_permission(request, obj),
                "has_add_permission": self.has_add_permission(request),
                "has_change_permission": self.has_change_permission(request, obj),
                "has_delete_permission": self.has_delete_permission(request, obj),
                "has_editable_inline_admin_formsets": (
                    has_editable_inline_admin_formsets
                "has_file_field": context["adminform"].form.is_multipart()
                or any(
                    admin_formset.formset.is_multipart()
                    for admin_formset in context["inline_admin_formsets"]
                "has_absolute_url": view_on_site_url is not None,
                "absolute_url": view_on_site_url,
                "form_url": form_url,
                "opts": self.opts,
                "content_type_id": get_content_type_for_model(self.model).pk,
                "save_as": self.save_as,
                "save_on_top": self.save_on_top,
                "to_field_var": TO_FIELD_VAR,
                "is_popup_var": IS_POPUP_VAR,
                "source_model_var": SOURCE_MODEL_VAR,
                "app_label": app_label,
        if add and self.add_form_template is not None:
            form_template = self.add_form_template
            form_template = self.change_form_template
        request.current_app = self.admin_site.name
        return TemplateResponse(
            form_template
            or [
                "admin/%s/%s/change_form.html" % (app_label, self.opts.model_name),
                "admin/%s/change_form.html" % app_label,
                "admin/change_form.html",
    def _get_preserved_qsl(self, request, preserved_filters):
        query_string = urlsplit(request.build_absolute_uri()).query
        return parse_qsl(query_string.replace(preserved_filters, ""))
    def response_add(self, request, obj, post_url_continue=None):
        Determine the HttpResponse for the add_view stage.
        opts = obj._meta
        preserved_qsl = self._get_preserved_qsl(request, preserved_filters)
        obj_url = reverse(
            "admin:%s_%s_change" % (opts.app_label, opts.model_name),
            args=(quote(obj.pk),),
        # Add a link to the object's change form if the user can edit the obj.
        obj_display = display_for_value(str(obj), EMPTY_VALUE_STRING)
        if self.has_change_permission(request, obj):
            obj_repr = format_html(
                '<a href="{}">{}</a>', urlquote(obj_url), obj_display
            obj_repr = obj_display
        msg_dict = {
            "name": opts.verbose_name,
            "obj": obj_repr,
        # Here, we distinguish between different save types by checking for
        # the presence of keys in request.POST.
        if IS_POPUP_VAR in request.POST:
            to_field = request.POST.get(TO_FIELD_VAR)
            if to_field:
                attr = str(to_field)
                attr = obj._meta.pk.attname
            value = obj.serializable_value(attr)
            popup_response = {
                "value": str(value),
                "obj": str(obj),
            # Find the optgroup for the new item, if available
            source_model_name = request.POST.get(SOURCE_MODEL_VAR)
            source_admin = None
            if source_model_name:
                app_label, model_name = source_model_name.split(".", 1)
                    source_model = apps.get_model(app_label, model_name)
                    msg = _('The app "%s" could not be found.') % source_model_name
                    self.message_user(request, msg, messages.ERROR)
                    source_admin = self.admin_site._registry.get(source_model)
            if source_admin:
                form = source_admin.get_form(request)()
                if self.opts.verbose_name_plural in form.fields:
                    field = form.fields[self.opts.verbose_name_plural]
                    for option_value, option_label in field.choices:
                        # Check if this is an optgroup (label is a sequence
                        # of choices rather than a single string value).
                        if isinstance(option_label, (list, tuple)):
                            # It's an optgroup:
                            # (group_name, [(value, label), ...])
                            optgroup_label = option_value
                            for choice_value, choice_display in option_label:
                                if choice_display == str(obj):
                                    popup_response["optgroup"] = str(optgroup_label)
            popup_response_data = json.dumps(popup_response)
                self.popup_response_template
                    "admin/%s/%s/popup_response.html"
                    % (opts.app_label, opts.model_name),
                    "admin/%s/popup_response.html" % opts.app_label,
                    "admin/popup_response.html",
                    "popup_response_data": popup_response_data,
        elif "_continue" in request.POST or (
            # Redirecting after "Save as new".
            "_saveasnew" in request.POST
            and self.save_as_continue
            and self.has_change_permission(request, obj)
            msg = _("The {name} “{obj}” was added successfully.")
                msg += " " + _("You may edit it again below.")
            self.message_user(request, format_html(msg, **msg_dict), messages.SUCCESS)
            if post_url_continue is None:
                post_url_continue = obj_url
            post_url_continue = add_preserved_filters(
                    "preserved_filters": preserved_filters,
                    "preserved_qsl": preserved_qsl,
                    "opts": opts,
                post_url_continue,
            return HttpResponseRedirect(post_url_continue)
        elif "_addanother" in request.POST:
            msg = format_html(
                _(
                    "The {name} “{obj}” was added successfully. You may add another "
                    "{name} below."
                **msg_dict,
            self.message_user(request, msg, messages.SUCCESS)
            redirect_url = request.path
            redirect_url = add_preserved_filters(
                redirect_url,
            return HttpResponseRedirect(redirect_url)
                _("The {name} “{obj}” was added successfully."), **msg_dict
            return self.response_post_save_add(request, obj)
    def response_change(self, request, obj):
        Determine the HttpResponse for the change_view stage.
            attr = str(to_field) if to_field else opts.pk.attname
            value = request.resolver_match.kwargs["object_id"]
            new_value = obj.serializable_value(attr)
            popup_response_data = json.dumps(
                    "action": "change",
                    "new_value": str(new_value),
            "obj": format_html(
                '<a href="{}">{}</a>', urlquote(request.path), obj_display
        if "_continue" in request.POST:
                    "The {name} “{obj}” was changed successfully. You may edit it "
                    "again below."
                    "The {name} “{obj}” was changed successfully. You may add another "
            redirect_url = reverse(
                "admin:%s_%s_add" % (opts.app_label, opts.model_name),
                _("The {name} “{obj}” was changed successfully."), **msg_dict
            return self.response_post_save_change(request, obj)
    def _response_post_save(self, request, obj):
        if self.has_view_or_change_permission(request):
            post_url = reverse(
                "admin:%s_%s_changelist" % (self.opts.app_label, self.opts.model_name),
            post_url = add_preserved_filters(
                {"preserved_filters": preserved_filters, "opts": self.opts}, post_url
            post_url = reverse("admin:index", current_app=self.admin_site.name)
        return HttpResponseRedirect(post_url)
    def response_post_save_add(self, request, obj):
        Figure out where to redirect after the 'Save' button has been pressed
        when adding a new object.
        return self._response_post_save(request, obj)
    def response_post_save_change(self, request, obj):
        when editing an existing object.
    def response_action(self, request, queryset):
        Handle an admin action. This is called if a request is POSTed to the
        changelist; it returns an HttpResponse if the action was handled, and
        None otherwise.
        # There can be multiple action forms on the page (at the top
        # and bottom of the change list, for example). Get the action
        # whose button was pushed.
            action_index = int(request.POST.get("index", 0))
            action_index = 0
        # Construct the action form.
        data = request.POST.copy()
        data.pop(helpers.ACTION_CHECKBOX_NAME, None)
        data.pop("index", None)
        # Use the action whose button was pushed
            data.update({"action": data.getlist("action")[action_index]})
            # If we didn't get an action from the chosen form that's invalid
            # POST data, so by deleting action it'll fail the validation check
            # below. So no need to do anything here
        action_form = self.action_form(data, auto_id=None)
        action_form.fields["action"].choices = self.get_action_choices(request)
        # If the form's valid we can handle the action.
        if action_form.is_valid():
            action = action_form.cleaned_data["action"]
            select_across = action_form.cleaned_data["select_across"]
            func = self.get_actions(request)[action][0]
            # Get the list of selected PKs. If nothing's selected, we can't
            # perform an action on it, so bail. Except we want to perform
            # the action explicitly on all objects.
            selected = request.POST.getlist(helpers.ACTION_CHECKBOX_NAME)
            if not selected and not select_across:
                # Something needs to be selected or nothing will happen.
                    "Items must be selected in order to perform "
                    "actions on them. No items have been changed."
                self.message_user(request, msg, messages.WARNING)
            if not select_across:
                # Perform the action only on the selected objects
                queryset = queryset.filter(pk__in=selected)
            response = func(self, request, queryset)
            # Actions may return an HttpResponse-like object, which will be
            # used as the response from the POST. If not, we'll be a good
            # little HTTP citizen and redirect back to the changelist page.
            if isinstance(response, HttpResponseBase):
                return HttpResponseRedirect(request.get_full_path())
            msg = _("No action selected.")
    def response_delete(self, request, obj_display, obj_id):
        Determine the HttpResponse for the delete_view stage.
                    "action": "delete",
                    "value": str(obj_id),
                    % (self.opts.app_label, self.opts.model_name),
                    "admin/%s/popup_response.html" % self.opts.app_label,
        self.message_user(
            _("The %(name)s “%(obj)s” was deleted successfully.")
                "name": self.opts.verbose_name,
                "obj": display_for_value(str(obj_display), EMPTY_VALUE_STRING),
            messages.SUCCESS,
        if self.has_change_permission(request, None):
    def render_delete_form(self, request, context):
            to_field_var=TO_FIELD_VAR,
            is_popup_var=IS_POPUP_VAR,
            media=self.media,
            self.delete_confirmation_template
                "admin/{}/{}/delete_confirmation.html".format(
                    app_label, self.opts.model_name
                "admin/{}/delete_confirmation.html".format(app_label),
                "admin/delete_confirmation.html",
    def get_inline_formsets(self, request, formsets, inline_instances, obj=None):
        # Edit permissions on parent model are required for editable inlines.
        can_edit_parent = (
            self.has_change_permission(request, obj)
            if obj
            else self.has_add_permission(request)
        inline_admin_formsets = []
        for inline, formset in zip(inline_instances, formsets):
            fieldsets = list(inline.get_fieldsets(request, obj))
            readonly = list(inline.get_readonly_fields(request, obj))
            if can_edit_parent:
                has_add_permission = inline.has_add_permission(request, obj)
                has_change_permission = inline.has_change_permission(request, obj)
                has_delete_permission = inline.has_delete_permission(request, obj)
                # Disable all edit-permissions, and override formset settings.
                has_add_permission = has_change_permission = has_delete_permission = (
                formset.extra = formset.max_num = 0
            has_view_permission = inline.has_view_permission(request, obj)
            prepopulated = dict(inline.get_prepopulated_fields(request, obj))
            inline_admin_formset = helpers.InlineAdminFormSet(
                prepopulated,
                readonly,
                model_admin=self,
                has_add_permission=has_add_permission,
                has_change_permission=has_change_permission,
                has_delete_permission=has_delete_permission,
                has_view_permission=has_view_permission,
            inline_admin_formsets.append(inline_admin_formset)
        return inline_admin_formsets
    def get_changeform_initial_data(self, request):
        Get the initial form data from the request's GET params.
        initial = dict(request.GET.items())
        for k in initial:
                f = self.opts.get_field(k)
            # We have to special-case M2Ms as a list of comma-separated PKs.
            if isinstance(f, models.ManyToManyField):
                initial[k] = initial[k].split(",")
        return initial
    def _get_obj_does_not_exist_redirect(self, request, opts, object_id):
        Create a message informing the user that the object doesn't exist
        and return a redirect to the admin index page.
        msg = _("%(name)s with ID “%(key)s” doesn’t exist. Perhaps it was deleted?") % {
            "key": unquote(object_id),
        url = reverse("admin:index", current_app=self.admin_site.name)
        return HttpResponseRedirect(url)
    @csrf_protect_m
    def changeform_view(self, request, object_id=None, form_url="", extra_context=None):
        if request.method in ("GET", "HEAD", "OPTIONS", "TRACE"):
            return self._changeform_view(request, object_id, form_url, extra_context)
        with transaction.atomic(using=router.db_for_write(self.model)):
    def _changeform_view(self, request, object_id, form_url, extra_context):
        to_field = request.POST.get(TO_FIELD_VAR, request.GET.get(TO_FIELD_VAR))
        if to_field and not self.to_field_allowed(request, to_field):
            raise DisallowedModelAdminToField(
                "The field %s cannot be referenced." % to_field
        if request.method == "POST" and "_saveasnew" in request.POST:
            object_id = None
        add = object_id is None
        if add:
            if not self.has_add_permission(request):
                raise PermissionDenied
            obj = None
            obj = self.get_object(request, unquote(object_id), to_field)
            if request.method == "POST":
                if not self.has_change_permission(request, obj):
                if not self.has_view_or_change_permission(request, obj):
                return self._get_obj_does_not_exist_redirect(
                    request, self.opts, object_id
        fieldsets = self.get_fieldsets(request, obj)
        ModelForm = self.get_form(
            request, obj, change=not add, fields=flatten_fieldsets(fieldsets)
            form = ModelForm(request.POST, request.FILES, instance=obj)
            formsets, inline_instances = self._create_formsets(
                form.instance,
                change=not add,
            form_validated = form.is_valid()
            if form_validated:
                new_object = self.save_form(request, form, change=not add)
                new_object = form.instance
            if all_valid(formsets) and form_validated:
                self.save_model(request, new_object, form, not add)
                self.save_related(request, form, formsets, not add)
                change_message = self.construct_change_message(
                    request, form, formsets, add
                    self.log_addition(request, new_object, change_message)
                    return self.response_add(request, new_object)
                    self.log_change(request, new_object, change_message)
                    return self.response_change(request, new_object)
                form_validated = False
                initial = self.get_changeform_initial_data(request)
                form = ModelForm(initial=initial)
                    request, form.instance, change=False
                form = ModelForm(instance=obj)
                    request, obj, change=True
        if not add and not self.has_change_permission(request, obj):
            readonly_fields = flatten_fieldsets(fieldsets)
        admin_form = helpers.AdminForm(
            list(fieldsets),
            # Clear prepopulated fields on a view-only form to avoid a crash.
                self.get_prepopulated_fields(request, obj)
                if add or self.has_change_permission(request, obj)
                else {}
            readonly_fields,
        media = self.media + admin_form.media
        inline_formsets = self.get_inline_formsets(
            request, formsets, inline_instances, obj
            media += inline_formset.media
            title = _("Add %s")
        elif self.has_change_permission(request, obj):
            title = _("Change %s")
            title = _("View %s")
            **self.admin_site.each_context(request),
            "title": title % self.opts.verbose_name,
            "subtitle": (
                display_for_value(str(obj), EMPTY_VALUE_STRING) if obj else None
            "adminform": admin_form,
            "object_id": object_id,
            "original": obj,
            "is_popup": IS_POPUP_VAR in request.POST or IS_POPUP_VAR in request.GET,
            "source_model": request.GET.get(SOURCE_MODEL_VAR),
            "to_field": to_field,
            "media": media,
            "inline_admin_formsets": inline_formsets,
            "errors": helpers.AdminErrorList(form, formsets),
            "preserved_filters": self.get_preserved_filters(request),
        # Hide the "Save" and "Save and continue" buttons if "Save as New" was
        # previously chosen to prevent the interface from getting confusing.
            request.method == "POST"
            and not form_validated
            and "_saveasnew" in request.POST
            context["show_save"] = False
            context["show_save_and_continue"] = False
            # Use the change template instead of the add template.
            add = False
        context.update(extra_context or {})
        return self.render_change_form(
            request, context, add=add, change=not add, obj=obj, form_url=form_url
    def add_view(self, request, form_url="", extra_context=None):
        return self.changeform_view(request, None, form_url, extra_context)
    def change_view(self, request, object_id, form_url="", extra_context=None):
        return self.changeform_view(request, object_id, form_url, extra_context)
    def _get_edited_object_pks(self, request, prefix):
        """Return POST data values of list_editable primary keys."""
        pk_pattern = re.compile(
            r"{}-\d+-{}$".format(re.escape(prefix), self.opts.pk.name)
        return [value for key, value in request.POST.items() if pk_pattern.match(key)]
    def _get_list_editable_queryset(self, request, prefix):
        Based on POST data, return a queryset of the objects that were edited
        via list_editable.
        object_pks = self._get_edited_object_pks(request, prefix)
        validate = queryset.model._meta.pk.to_python
            for pk in object_pks:
                validate(pk)
            # Disable the optimization if the POST data was tampered with.
            return queryset
        return queryset.filter(pk__in=object_pks)
    def changelist_view(self, request, extra_context=None):
        The 'change list' admin view for this model.
        from django.contrib.admin.views.main import ERROR_FLAG
        if not self.has_view_or_change_permission(request):
            cl = self.get_changelist_instance(request)
        except IncorrectLookupParameters:
            # Wacky lookup parameters were given, so redirect to the main
            # changelist page, without parameters, and pass an 'invalid=1'
            # parameter via the query string. If wacky parameters were given
            # and the 'invalid=1' parameter was already in the query string,
            # something is screwed up with the database, so display an error
            # page.
            if ERROR_FLAG in request.GET:
                return SimpleTemplateResponse(
                    "admin/invalid_setup.html",
                        "title": _("Database error"),
            return HttpResponseRedirect(request.path + "?" + ERROR_FLAG + "=1")
        # If the request was POSTed, this might be a bulk action or a bulk
        # edit. Try to look up an action or confirmation first, but if this
        # isn't an action the POST will fall through to the bulk edit check,
        action_failed = False
        actions = self.get_actions(request)
        # Actions with no confirmation
            actions
            and request.method == "POST"
            and "index" in request.POST
            and "_save" not in request.POST
            if selected:
                response = self.response_action(
                    request, queryset=cl.get_queryset(request)
                    action_failed = True
        # Actions with confirmation
            and helpers.ACTION_CHECKBOX_NAME in request.POST
            and "index" not in request.POST
        if action_failed:
            # Redirect back to the changelist page to avoid resubmitting the
            # form if the user refreshes the browser or uses the "No, take
            # me back" button on the action confirmation page.
        # Handle POSTed bulk-edit data.
        if request.method == "POST" and cl.list_editable and "_save" in request.POST:
            if not self.has_change_permission(request):
            FormSet = self.get_changelist_formset(request)
            modified_objects = self._get_list_editable_queryset(
                request, FormSet.get_default_prefix()
            cl.formset = FormSet(request.POST, request.FILES, queryset=modified_objects)
            if cl.formset.is_valid():
                changecount = 0
                    for form in cl.formset.forms:
                        if form.has_changed():
                            obj = self.save_form(request, form, change=True)
                            self.save_model(request, obj, form, change=True)
                            self.save_related(request, form, formsets=[], change=True)
                            change_msg = self.construct_change_message(
                                request, form, None
                            self.log_change(request, obj, change_msg)
                            changecount += 1
                if changecount:
                    msg = ngettext(
                        "%(count)s %(name)s was changed successfully.",
                        "%(count)s %(name)s were changed successfully.",
                        changecount,
                    ) % {
                        "count": changecount,
                        "name": model_ngettext(self.opts, changecount),
        # Handle GET -- construct a formset for display.
        elif cl.list_editable and self.has_change_permission(request):
            cl.formset = FormSet(queryset=cl.result_list)
        # Build the list of media to be used by the formset.
        if cl.formset:
            media = self.media + cl.formset.media
            media = self.media
        # Build the action form and populate it with available actions.
        if actions:
            action_form = self.action_form(auto_id=None)
            media += action_form.media
            action_form = None
        selection_note_all = ngettext(
            "%(total_count)s selected", "All %(total_count)s selected", cl.result_count
            "module_name": str(self.opts.verbose_name_plural),
            "selection_note": _("0 of %(cnt)s selected") % {"cnt": len(cl.result_list)},
            "selection_note_all": selection_note_all % {"total_count": cl.result_count},
            "title": cl.title,
            "subtitle": None,
            "is_popup": cl.is_popup,
            "to_field": cl.to_field,
            "cl": cl,
            "opts": cl.opts,
            "action_form": action_form,
            "actions_on_top": self.actions_on_top,
            "actions_on_bottom": self.actions_on_bottom,
            "actions_selection_counter": self.actions_selection_counter,
            **(extra_context or {}),
            self.change_list_template
                "admin/%s/%s/change_list.html" % (app_label, self.opts.model_name),
                "admin/%s/change_list.html" % app_label,
                "admin/change_list.html",
    def get_deleted_objects(self, objs, request):
        Hook for customizing the delete process for the delete view and the
        "delete selected" action.
        return get_deleted_objects(objs, request, self.admin_site)
    def delete_view(self, request, object_id, extra_context=None):
            return self._delete_view(request, object_id, extra_context)
    def _delete_view(self, request, object_id, extra_context):
        "The 'delete' admin view for this model."
        if not self.has_delete_permission(request, obj):
            return self._get_obj_does_not_exist_redirect(request, self.opts, object_id)
        # Populate deleted_objects, a data structure of all related objects
        # that will also be deleted.
            deleted_objects,
            model_count,
            perms_needed,
            protected,
        ) = self.get_deleted_objects([obj], request)
        if request.POST and not protected:  # The user has confirmed the deletion.
            if perms_needed:
            obj_display = str(obj)
            attr = str(to_field) if to_field else self.opts.pk.attname
            obj_id = obj.serializable_value(attr)
            self.log_deletions(request, [obj])
            self.delete_model(request, obj)
            return self.response_delete(request, obj_display, obj_id)
        object_name = str(self.opts.verbose_name)
        if perms_needed or protected:
            title = _("Cannot delete %(name)s") % {"name": object_name}
            title = _("Delete")
            "title": title,
            "object_name": object_name,
            "object": obj,
            "escaped_object": display_for_value(str(obj), EMPTY_VALUE_STRING),
            "deleted_objects": deleted_objects,
            "model_count": dict(model_count).items(),
            "perms_lacking": perms_needed,
            "protected": protected,
        return self.render_delete_form(request, context)
    def history_view(self, request, object_id, extra_context=None):
        "The 'history' admin view for this model."
        from django.contrib.admin.models import LogEntry
        from django.contrib.admin.views.main import PAGE_VAR
        # First check if the user can see this history.
        obj = self.get_object(request, unquote(object_id))
                request, model._meta, object_id
        # Then get the history for this object.
        action_list = (
            LogEntry.objects.filter(
                object_id=unquote(object_id),
                content_type=get_content_type_for_model(model),
            .select_related()
            .order_by("action_time")
        paginator = self.get_paginator(request, action_list, 100)
        page_number = request.GET.get(PAGE_VAR, 1)
        page_obj = paginator.get_page(page_number)
        page_range = paginator.get_elided_page_range(page_obj.number)
            "title": _("Change history: %s")
            % display_for_value(str(obj), EMPTY_VALUE_STRING),
            "action_list": page_obj,
            "page_range": page_range,
            "page_var": PAGE_VAR,
            "pagination_required": paginator.count > 100,
            "module_name": str(capfirst(self.opts.verbose_name_plural)),
            self.object_history_template
                "admin/%s/%s/object_history.html" % (app_label, self.opts.model_name),
                "admin/%s/object_history.html" % app_label,
                "admin/object_history.html",
    def get_formset_kwargs(self, request, obj, inline, prefix):
        formset_params = {
            "instance": obj,
            "prefix": prefix,
            "queryset": inline.get_queryset(request),
            formset_params.update(
                    "data": request.POST.copy(),
                    "files": request.FILES,
                    "save_as_new": "_saveasnew" in request.POST,
        return formset_params
    def _create_formsets(self, request, obj, change):
        "Helper function to generate formsets for add/change_view."
        formsets = []
        prefixes = {}
        get_formsets_args = [request]
        if change:
            get_formsets_args.append(obj)
        for FormSet, inline in self.get_formsets_with_inlines(*get_formsets_args):
            prefix = FormSet.get_default_prefix()
            prefixes[prefix] = prefixes.get(prefix, 0) + 1
            if prefixes[prefix] != 1 or not prefix:
                prefix = "%s-%s" % (prefix, prefixes[prefix])
            formset_params = self.get_formset_kwargs(request, obj, inline, prefix)
            formset = FormSet(**formset_params)
            def user_deleted_form(request, obj, formset, index, inline):
                """Return whether or not the user deleted the form."""
                    inline.has_delete_permission(request, obj)
                    and "{}-{}-DELETE".format(formset.prefix, index) in request.POST
            # Bypass validation of each view-only inline form (since the form's
            # data won't be in request.POST), unless the form was deleted.
            if not inline.has_change_permission(request, obj if change else None):
                for index, form in enumerate(formset.initial_forms):
                    if user_deleted_form(request, obj, formset, index, inline):
                    form._errors = {}
                    form.cleaned_data = form.initial
            formsets.append(formset)
        return formsets, inline_instances
class InlineModelAdmin(BaseModelAdmin):
    Options for inline editing of ``model`` instances.
    Provide ``fk_name`` to specify the attribute name of the ``ForeignKey``
    from ``model`` to its parent. This is required if ``model`` has more than
    one ``ForeignKey`` to its parent.
    model = None
    fk_name = None
    formset = BaseInlineFormSet
    extra = 3
    min_num = None
    max_num = None
    template = None
    verbose_name = None
    verbose_name_plural = None
    can_delete = True
    show_change_link = False
    checks_class = InlineModelAdminChecks
    classes = None
    def __init__(self, parent_model, admin_site):
        self.parent_model = parent_model
        self.opts = self.model._meta
        self.has_registered_model = admin_site.is_registered(self.model)
        if self.verbose_name_plural is None:
            if self.verbose_name is None:
                self.verbose_name_plural = self.opts.verbose_name_plural
                self.verbose_name_plural = format_lazy("{}s", self.verbose_name)
            self.verbose_name = self.opts.verbose_name
        js = ["vendor/jquery/jquery%s.js" % extra, "jquery.init.js", "inlines.js"]
        if self.filter_vertical or self.filter_horizontal:
            js.extend(["SelectBox.js", "SelectFilter2.js"])
    def get_extra(self, request, obj=None, **kwargs):
        """Hook for customizing the number of extra inline forms."""
        return self.extra
    def get_min_num(self, request, obj=None, **kwargs):
        """Hook for customizing the min number of inline forms."""
        return self.min_num
    def get_max_num(self, request, obj=None, **kwargs):
        """Hook for customizing the max number of extra inline forms."""
        return self.max_num
    def get_formset(self, request, obj=None, **kwargs):
        """Return a BaseInlineFormSet class for use in add/change views."""
        exclude.extend(self.get_readonly_fields(request, obj))
            # InlineModelAdmin doesn't define its own.
        # If exclude is an empty list we use None, since that's the actual
        can_delete = self.can_delete and self.has_delete_permission(request, obj)
            "form": self.form,
            "formset": self.formset,
            "fk_name": self.fk_name,
            "extra": self.get_extra(request, obj, **kwargs),
            "min_num": self.get_min_num(request, obj, **kwargs),
            "max_num": self.get_max_num(request, obj, **kwargs),
            "can_delete": can_delete,
        base_model_form = defaults["form"]
        can_change = self.has_change_permission(request, obj) if request else True
        can_add = self.has_add_permission(request, obj) if request else True
        class DeleteProtectedModelForm(base_model_form):
            def hand_clean_DELETE(self):
                We don't validate the 'DELETE' field itself because on
                templates it's not rendered using the field information, but
                just using a generic "deletion_field" of the InlineModelAdmin.
                if self.cleaned_data.get(DELETION_FIELD_NAME, False):
                    using = router.db_for_write(self._meta.model)
                    collector = NestedObjects(using=using)
                    if self.instance._state.adding:
                    collector.collect([self.instance])
                    if collector.protected:
                        objs = []
                        for p in collector.protected:
                            objs.append(
                                # Translators: Model verbose name and instance
                                # representation, suitable to be an item in a
                                _("%(class_name)s %(instance)s")
                                % {"class_name": p._meta.verbose_name, "instance": p}
                            "class_name": self._meta.model._meta.verbose_name,
                            "instance": self.instance,
                            "related_objects": get_text_list(objs, _("and")),
                            "Deleting %(class_name)s %(instance)s would require "
                            "deleting the following protected related objects: "
                            "%(related_objects)s"
                        raise ValidationError(
                            msg, code="deleting_protected", params=params
            def is_valid(self):
                result = super().is_valid()
                self.hand_clean_DELETE()
            def has_changed(self):
                # Protect against unauthorized edits.
                if not can_change and not self.instance._state.adding:
                if not can_add and self.instance._state.adding:
                return super().has_changed()
        defaults["form"] = DeleteProtectedModelForm
        return inlineformset_factory(self.parent_model, self.model, **defaults)
    def _get_form_for_get_fields(self, request, obj=None):
        return self.get_formset(request, obj, fields=None).form
        queryset = super().get_queryset(request)
            queryset = queryset.none()
    def _has_any_perms_for_target_model(self, request, perms):
        This method is called only when the ModelAdmin's model is for an
        ManyToManyField's implicit through model (if self.opts.auto_created).
        Return True if the user has any of the given permissions ('add',
        'change', etc.) for the model that points to the through model.
        # Find the target model of an auto-created many-to-many relationship.
        for field in opts.fields:
            if field.remote_field and field.remote_field.model != self.parent_model:
                opts = field.remote_field.model._meta
        return any(
            request.user.has_perm(
                "%s.%s" % (opts.app_label, get_permission_codename(perm, opts))
            for perm in perms
    def has_add_permission(self, request, obj):
        if self.opts.auto_created:
            # Auto-created intermediate models don't have their own
            # permissions. The user needs to have the change permission for the
            # related model in order to be able to do anything with the
            # intermediate model.
            return self._has_any_perms_for_target_model(request, ["change"])
        return super().has_add_permission(request)
            # Same comment as has_add_permission().
        return super().has_change_permission(request)
        return super().has_delete_permission(request, obj)
            # Same comment as has_add_permission(). The 'change' permission
            # also implies the 'view' permission.
            return self._has_any_perms_for_target_model(request, ["view", "change"])
        return super().has_view_permission(request)
class StackedInline(InlineModelAdmin):
    template = "admin/edit_inline/stacked.html"
class TabularInline(InlineModelAdmin):
    template = "admin/edit_inline/tabular.html"
from django.contrib.admin import ModelAdmin
from django.contrib.gis.db import models
from django.contrib.gis.forms import OSMWidget
class GeoModelAdminMixin:
    gis_widget = OSMWidget
    gis_widget_kwargs = {}
        if isinstance(db_field, models.GeometryField) and (
            db_field.dim < 3 or self.gis_widget.supports_3d
            kwargs["widget"] = self.gis_widget(**self.gis_widget_kwargs)
            return super().formfield_for_dbfield(db_field, request, **kwargs)
class GISModelAdmin(GeoModelAdminMixin, ModelAdmin):
import bisect
from django.core.exceptions import FieldDoesNotExist, ImproperlyConfigured
from django.core.signals import setting_changed
from django.db.models import (
    AutoField,
    CompositePrimaryKey,
    Manager,
    OrderWrt,
    UniqueConstraint,
from django.db.models.fields import composite
from django.db.models.query_utils import PathInfo
from django.utils.datastructures import ImmutableList, OrderedSet
from django.utils.text import camel_case_to_spaces, format_lazy
from django.utils.translation import override
PROXY_PARENTS = object()
EMPTY_RELATION_TREE = ()
IMMUTABLE_WARNING = (
    "The return type of '%s' should never be mutated. If you want to manipulate this "
    "list for your own use, make a copy first."
DEFAULT_NAMES = (
    "db_table",
    "db_table_comment",
    "unique_together",
    "order_with_respect_to",
    "db_tablespace",
    "abstract",
    "proxy",
    "swappable",
    "auto_created",
    "required_db_features",
    "required_db_vendor",
    "indexes",
    "constraints",
def normalize_together(option_together):
    option_together can be either a tuple of tuples, or a single
    tuple of two strings. Normalize it to a tuple of tuples, so that
    calling code can uniformly expect that.
        if not option_together:
        if not isinstance(option_together, (tuple, list)):
            raise TypeError
        first_element = option_together[0]
        if not isinstance(first_element, (tuple, list)):
            option_together = (option_together,)
        # Normalize everything to tuples
        return tuple(tuple(ot) for ot in option_together)
        # If the value of option_together isn't valid, return it
        # verbatim; this will be picked up by the check framework later.
        return option_together
def make_immutable_fields_list(name, data):
    return ImmutableList(data, warning=IMMUTABLE_WARNING % name)
    FORWARD_PROPERTIES = {
        "fields",
        "many_to_many",
        "concrete_fields",
        "local_concrete_fields",
        "_non_pk_concrete_field_names",
        "_reverse_one_to_one_field_names",
        "_forward_fields_map",
        "managers",
        "managers_map",
        "base_manager",
        "default_manager",
        "db_returning_fields",
        "_property_names",
        "pk_fields",
        "total_unique_constraints",
        "all_parents",
        "swapped",
        "verbose_name_raw",
    REVERSE_PROPERTIES = {"related_objects", "fields_map", "_relation_tree"}
    default_apps = apps
    def __init__(self, meta, app_label=None):
        self._get_fields_cache = {}
        self.local_fields = []
        self.local_many_to_many = []
        self.private_fields = []
        self.local_managers = []
        self.base_manager_name = None
        self.default_manager_name = None
        self.model_name = None
        self.verbose_name = None
        self.verbose_name_plural = None
        self.db_table = ""
        self.db_table_comment = ""
        self.ordering = []
        self._ordering_clash = False
        self.indexes = []
        self.constraints = []
        self.unique_together = []
        self.select_on_save = False
        self.default_permissions = ("add", "change", "delete", "view")
        self.permissions = []
        self.object_name = None
        self.get_latest_by = None
        self.order_with_respect_to = None
        self.db_tablespace = settings.DEFAULT_TABLESPACE
        self.required_db_features = []
        self.required_db_vendor = None
        self.meta = meta
        self.pk = None
        self.auto_field = None
        self.abstract = False
        self.managed = True
        self.proxy = False
        # For any class that is a proxy (including automatically created
        # classes for deferred object loading), proxy_for_model tells us
        # which class this model is proxying. Note that proxy_for_model
        # can create a chain of proxy models. For non-proxy models, the
        # variable is always None.
        self.proxy_for_model = None
        # For any non-abstract class, the concrete class is the model
        # in the end of the proxy_for_model chain. In particular, for
        # concrete models, the concrete_model is always the class itself.
        self.concrete_model = None
        self.swappable = None
        self.parents = {}
        self.auto_created = False
        # List of all lookups defined in ForeignKey 'limit_choices_to' options
        # from *other* models. Needed for some admin checks. Internal use only.
        self.related_fkey_lookups = []
        # A custom app registry to use, if you're making a separate model set.
        self.apps = self.default_apps
        self.default_related_name = None
    def label(self):
        return "%s.%s" % (self.app_label, self.object_name)
    def label_lower(self):
        return "%s.%s" % (self.app_label, self.model_name)
    def app_config(self):
        # Don't go through get_app_config to avoid triggering imports.
        return self.apps.app_configs.get(self.app_label)
    def contribute_to_class(self, cls, name):
        from django.db import connection
        cls._meta = self
        self.model = cls
        # First, construct the default values for these options.
        self.object_name = cls.__name__
        self.model_name = self.object_name.lower()
        self.verbose_name = camel_case_to_spaces(self.object_name)
        # Store the original user-defined values for each option,
        # for use when serializing the model definition
        self.original_attrs = {}
        # Next, apply any overridden values from 'class Meta'.
        if self.meta:
            meta_attrs = self.meta.__dict__.copy()
            for name in self.meta.__dict__:
                # Ignore any private attributes that Django doesn't care about.
                # NOTE: We can't modify a dictionary's contents while looping
                # over it, so we loop over the *original* dictionary instead.
                if name.startswith("_"):
                    del meta_attrs[name]
            for attr_name in DEFAULT_NAMES:
                if attr_name in meta_attrs:
                    setattr(self, attr_name, meta_attrs.pop(attr_name))
                    self.original_attrs[attr_name] = getattr(self, attr_name)
                elif hasattr(self.meta, attr_name):
                    setattr(self, attr_name, getattr(self.meta, attr_name))
            self.unique_together = normalize_together(self.unique_together)
            # App label/class name interpolation for names of constraints and
            if not self.abstract:
                self.constraints = self._format_names(self.constraints)
                self.indexes = self._format_names(self.indexes)
            # verbose_name_plural is a special case because it uses a 's'
            # by default.
            # order_with_respect_and ordering are mutually exclusive.
            self._ordering_clash = bool(self.ordering and self.order_with_respect_to)
            # Any leftover attributes must be invalid.
            if meta_attrs != {}:
                    "'class Meta' got invalid attribute(s): %s" % ",".join(meta_attrs)
        del self.meta
        # If the db_table wasn't provided, use the app_label + model_name.
        if not self.db_table:
            self.db_table = "%s_%s" % (self.app_label, self.model_name)
            self.db_table = truncate_name(
                self.db_table, connection.ops.max_name_length()
        if self.swappable:
            setting_changed.connect(self.setting_changed)
    def _format_names(self, objs):
        """App label/class name interpolation for object names."""
        names = {"app_label": self.app_label.lower(), "class": self.model_name}
        new_objs = []
            obj = obj.clone()
            obj.name %= names
            new_objs.append(obj)
        return new_objs
    def _get_default_pk_class(self):
        pk_class_path = getattr(
            self.app_config,
            "default_auto_field",
            settings.DEFAULT_AUTO_FIELD,
        if self.app_config and self.app_config._is_default_auto_field_overridden:
            app_config_class = type(self.app_config)
            source = (
                f"{app_config_class.__module__}."
                f"{app_config_class.__qualname__}.default_auto_field"
            source = "DEFAULT_AUTO_FIELD"
        if not pk_class_path:
            raise ImproperlyConfigured(f"{source} must not be empty.")
            pk_class = import_string(pk_class_path)
                f"{source} refers to the module '{pk_class_path}' that could "
                f"not be imported."
            raise ImproperlyConfigured(msg) from e
        if not issubclass(pk_class, AutoField):
                f"Primary key '{pk_class_path}' referred by {source} must "
                f"subclass AutoField."
        return pk_class
    def _prepare(self, model):
        if self.order_with_respect_to:
            # The app registry will not be ready at this point, so we cannot
            # use get_field().
            query = self.order_with_respect_to
                self.order_with_respect_to = next(
                    for f in self._get_fields(reverse=False)
                    if f.name == query or f.attname == query
                raise FieldDoesNotExist(
                    "%s has no field named '%s'" % (self.object_name, query)
            self.ordering = ("_order",)
                isinstance(field, OrderWrt) for field in model._meta.local_fields
                model.add_to_class("_order", OrderWrt())
        if self.pk is None:
            if self.parents:
                # Promote the first parent link in lieu of adding yet another
                field = next(iter(self.parents.values()))
                # Look for a local field with the same name as the
                # first parent link. If a local field has already been
                # created, use it instead of promoting the parent
                already_created = [
                    fld for fld in self.local_fields if fld.name == field.name
                if already_created:
                    field = already_created[0]
                field.primary_key = True
                self.setup_pk(field)
                pk_class = self._get_default_pk_class()
                auto = pk_class(verbose_name="ID", primary_key=True, auto_created=True)
                model.add_to_class("id", auto)
    def add_manager(self, manager):
        self.local_managers.append(manager)
        self._expire_cache()
    def add_field(self, field, private=False):
        # Insert the given field in the order in which it was created, using
        # the "creation_counter" attribute of the field.
        # Move many-to-many related fields from self.fields into
        # self.many_to_many.
        if private:
            self.private_fields.append(field)
        elif field.is_relation and field.many_to_many:
            bisect.insort(self.local_many_to_many, field)
            bisect.insort(self.local_fields, field)
        # If the field being added is a relation to another known field,
        # expire the cache on this field and the forward cache on the field
        # being referenced, because there will be new relationships in the
        # cache. Otherwise, expire the cache of references *to* this field.
        # The mechanism for getting at the related model is slightly odd -
        # ideally, we'd just ask for field.related_model. However,
        # related_model is a cached property, and all the models haven't been
        # loaded yet, so we need to make sure we don't cache a string
        # reference.
            and hasattr(field.remote_field, "model")
            and field.remote_field.model
                field.remote_field.model._meta._expire_cache(forward=False)
            self._expire_cache(reverse=False)
    def setup_pk(self, field):
        if not self.pk and field.primary_key:
            self.pk = field
            field.serialize = False
    def setup_proxy(self, target):
        Do the internal setup so that the current model is a proxy for
        "target".
        self.pk = target._meta.pk
        self.proxy_for_model = target
        self.db_table = target._meta.db_table
        return "<Options for %s>" % self.object_name
        return self.label_lower
    def can_migrate(self, connection):
        Return True if the model can/should be migrated on the `connection`.
        `connection` can be either a real connection or a connection alias.
        if self.proxy or self.swapped or not self.managed:
        if isinstance(connection, str):
            connection = connections[connection]
        if self.required_db_vendor:
            return self.required_db_vendor == connection.vendor
        if self.required_db_features:
                getattr(connection.features, feat, False)
                for feat in self.required_db_features
    def verbose_name_raw(self):
        """Return the untranslated verbose name."""
        if isinstance(self.verbose_name, str):
            return self.verbose_name
        with override(None):
            return str(self.verbose_name)
    def swapped(self):
        Has this model been swapped out for another? If so, return the model
        name of the replacement; otherwise, return None.
        For historical reasons, model name lookups using get_model() are
        case insensitive, so we make sure we are case insensitive here.
            swapped_for = getattr(settings, self.swappable, None)
            if swapped_for:
                    swapped_label, swapped_object = swapped_for.split(".")
                    # setting not in the format app_label.model_name
                    # raising ImproperlyConfigured here causes problems with
                    # test cleanup code - instead it is raised in
                    # get_user_model or as part of validation.
                    return swapped_for
                    "%s.%s" % (swapped_label, swapped_object.lower())
                    != self.label_lower
    def setting_changed(self, *, setting, **kwargs):
        if setting == self.swappable and "swapped" in self.__dict__:
            del self.swapped
    def managers(self):
        managers = []
        seen_managers = set()
        bases = (b for b in self.model.mro() if hasattr(b, "_meta"))
        for depth, base in enumerate(bases):
            for manager in base._meta.local_managers:
                if manager.name in seen_managers:
                manager = copy.copy(manager)
                manager.model = self.model
                seen_managers.add(manager.name)
                managers.append((depth, manager.creation_counter, manager))
        return make_immutable_fields_list(
            (m[2] for m in sorted(managers)),
    def managers_map(self):
        return {manager.name: manager for manager in self.managers}
    def base_manager(self):
        base_manager_name = self.base_manager_name
        if not base_manager_name:
            # Get the first parent's base_manager_name if there's one.
            for parent in self.model.mro()[1:]:
                if hasattr(parent, "_meta"):
                    if parent._base_manager.name != "_base_manager":
                        base_manager_name = parent._base_manager.name
        if base_manager_name:
                return self.managers_map[base_manager_name]
                    "%s has no manager named %r"
                        self.object_name,
                        base_manager_name,
        manager.name = "_base_manager"
        return manager
    def default_manager(self):
        default_manager_name = self.default_manager_name
        if not default_manager_name and not self.local_managers:
            # Get the first parent's default_manager_name if there's one.
                    default_manager_name = parent._meta.default_manager_name
        if default_manager_name:
                return self.managers_map[default_manager_name]
                        default_manager_name,
        if self.managers:
            return self.managers[0]
        Return a list of all forward fields on the model and its parents,
        excluding ManyToManyFields.
        Private API intended only to be used by Django itself; get_fields()
        combined with filtering of field properties is the public API for
        obtaining this field list.
        # For legacy reasons, the fields property should only contain forward
        # fields that are not private or with a m2m cardinality. Therefore we
        # pass these three filters as filters to the generator.
        # The third filter is a longwinded way of checking f.related_model - we
        # don't use that property directly because related_model is a cached
        # property, and all the models may not have been loaded yet; we don't
        # want to cache the string reference to the related_model.
        def is_not_an_m2m_field(f):
            return not (f.is_relation and f.many_to_many)
        def is_not_a_generic_relation(f):
            return not (f.is_relation and f.one_to_many)
        def is_not_a_generic_foreign_key(f):
                f.is_relation
                and f.many_to_one
                and not (hasattr(f.remote_field, "model") and f.remote_field.model)
                if is_not_an_m2m_field(f)
                and is_not_a_generic_relation(f)
                and is_not_a_generic_foreign_key(f)
    def concrete_fields(self):
        Return a list of all concrete fields on the model and its parents.
            "concrete_fields", (f for f in self.fields if f.concrete)
    def local_concrete_fields(self):
        Return a list of all concrete fields on the model.
            "local_concrete_fields", (f for f in self.local_fields if f.concrete)
    def many_to_many(self):
        Return a list of all many to many fields on the model and its parents.
        obtaining this list.
                if f.is_relation and f.many_to_many
    def related_objects(self):
        Return all related objects pointing to the current model. The related
        objects can come from a one-to-one, one-to-many, or many-to-many field
        relation type.
        all_related_fields = self._get_fields(
            forward=False, reverse=True, include_hidden=True
            "related_objects",
                for obj in all_related_fields
                if not obj.hidden or obj.field.many_to_many
    def _forward_fields_map(self):
        res = {}
        fields = self._get_fields(reverse=False)
            res[field.name] = field
            # Due to the way Django's internals work, get_field() should also
            # be able to fetch a field by attname. In the case of a concrete
            # field with relation, includes the *_id name too
                res[field.attname] = field
    def fields_map(self):
        fields = self._get_fields(forward=False, include_hidden=True)
    def get_field(self, field_name):
        Return a field instance given the name of a forward or reverse field.
            # In order to avoid premature loading of the relation tree
            # (expensive) we prefer checking if the field is a forward field.
            return self._forward_fields_map[field_name]
            # If the app registry is not ready, reverse fields are
            # unavailable, therefore we throw a FieldDoesNotExist exception.
            if not self.apps.models_ready:
                    "%s has no field named '%s'. The app cache isn't ready yet, "
                    "so if this is an auto-created related field, it won't "
                    "be available yet." % (self.object_name, field_name)
            # Retrieve field instance by name from cached or just-computed
            # field map.
            return self.fields_map[field_name]
                "%s has no field named '%s'" % (self.object_name, field_name)
    def get_base_chain(self, model):
        Return a list of parent classes leading to `model` (ordered from
        closest to most distant ancestor). This has to handle the case where
        `model` is a grandparent or even more distant relation.
        if not self.parents:
        if model in self.parents:
            return [model]
        for parent in self.parents:
            res = parent._meta.get_base_chain(model)
            if res:
                res.insert(0, parent)
    def all_parents(self):
        Return all the ancestors of this model as a tuple ordered by MRO.
        Useful for determining if something is an ancestor, regardless of
        lineage.
        result = OrderedSet(self.parents)
            for ancestor in parent._meta.all_parents:
                result.add(ancestor)
        return tuple(result)
    def get_parent_list(self):
        Return all the ancestors of this model as a list ordered by MRO.
        Backward compatibility method.
        return list(self.all_parents)
    def get_ancestor_link(self, ancestor):
        Return the field on the current model which points to the given
        "ancestor". This is possible an indirect link (a pointer to a parent
        model, which points, eventually, to the ancestor). Used when
        constructing table joins for model inheritance.
        Return None if the model isn't an ancestor of this one.
        if ancestor in self.parents:
            return self.parents[ancestor]
            # Tries to get a link field from the immediate parent
            parent_link = parent._meta.get_ancestor_link(ancestor)
            if parent_link:
                # In case of a proxied model, the first link
                # of the chain to the ancestor is that parent
                # links
                return self.parents[parent] or parent_link
    def get_path_to_parent(self, parent):
        Return a list of PathInfos containing the path from the current
        model to the parent model, or an empty list if parent is not a
        parent of the current model.
        if self.model is parent:
        # Skip the chain of proxy to the concrete proxied model.
        proxied_model = self.concrete_model
        path = []
        opts = self
        for int_model in self.get_base_chain(parent):
            if int_model is proxied_model:
                opts = int_model._meta
                final_field = opts.parents[int_model]
                targets = (final_field.remote_field.get_related_field(),)
                path.append(
                    PathInfo(
                        from_opts=final_field.model._meta,
                        to_opts=opts,
                        target_fields=targets,
                        join_field=final_field,
                        m2m=False,
                        direct=True,
                        filtered_relation=None,
    def get_path_from_parent(self, parent):
        Return a list of PathInfos containing the path from the parent
        model to the current model, or an empty list if parent is not a
        model = self.concrete_model
        # Get a reversed base chain including both the current and parent
        # models.
        chain = model._meta.get_base_chain(parent)
        chain.reverse()
        chain.append(model)
        # Construct a list of the PathInfos between models in chain.
        for i, ancestor in enumerate(chain[:-1]):
            child = chain[i + 1]
            link = child._meta.get_ancestor_link(ancestor)
            path.extend(link.reverse_path_infos)
    def _populate_directed_relation_graph(self):
        This method is used by each model to find its reverse objects. As this
        method is very expensive and is accessed frequently (it looks up every
        field in a model, in every app), it is computed on first access and
        then is set as a property on every model.
        related_objects_graph = defaultdict(list)
        all_models = self.apps.get_models(include_auto_created=True)
        for model in all_models:
            # Abstract model's fields are copied to child models, hence we will
            # see the fields from the child models.
            fields_with_relations = (
                for f in opts._get_fields(reverse=False, include_parents=False)
            for f in fields_with_relations:
                if not isinstance(f.remote_field.model, str):
                    remote_label = f.remote_field.model._meta.concrete_model._meta.label
                    related_objects_graph[remote_label].append(f)
            # Set the relation_tree using the internal __dict__. In this way
            # we avoid calling the cached property. In attribute lookup,
            # __dict__ takes precedence over a data descriptor (such as
            # @cached_property). This means that the _meta._relation_tree is
            # only called if related_objects is not in __dict__.
            related_objects = related_objects_graph[
                model._meta.concrete_model._meta.label
            model._meta.__dict__["_relation_tree"] = related_objects
        # It seems it is possible that self is not in all_models, so guard
        # against that with default for get().
        return self.__dict__.get("_relation_tree", EMPTY_RELATION_TREE)
    def _relation_tree(self):
        return self._populate_directed_relation_graph()
    def _expire_cache(self, forward=True, reverse=True):
        # This method is usually called by apps.cache_clear(), when the
        # registry is finalized, or when a new field is added.
        if forward:
            for cache_key in self.FORWARD_PROPERTIES:
                if cache_key in self.__dict__:
                    delattr(self, cache_key)
        if reverse and not self.abstract:
            for cache_key in self.REVERSE_PROPERTIES:
    def get_fields(self, include_parents=True, include_hidden=False):
        Return a list of fields associated to the model. By default, include
        forward and reverse fields, fields derived from inheritance, but not
        hidden fields. The returned fields can be changed using the parameters:
        - include_parents: include fields derived from inheritance
        - include_hidden:  include fields that have a related_name that
                           starts with a "+"
        if include_parents is False:
            include_parents = PROXY_PARENTS
        return self._get_fields(
            include_parents=include_parents, include_hidden=include_hidden
    def _get_fields(
        forward=True,
        include_parents=True,
        include_hidden=False,
        topmost_call=True,
        Internal helper function to return fields of the model.
        * If forward=True, then fields defined on this model are returned.
        * If reverse=True, then relations pointing to this model are returned.
        * If include_hidden=True, then fields with is_hidden=True are returned.
        * The include_parents argument toggles if fields from parent models
          should be included. It has three values: True, False, and
          PROXY_PARENTS. When set to PROXY_PARENTS, the call will return all
          fields defined for the current model or any of its parents in the
          parent chain to the model's concrete model.
        if include_parents not in (True, False, PROXY_PARENTS):
                "Invalid argument for include_parents: %s" % (include_parents,)
        # This helper function is used to allow recursion in ``get_fields()``
        # implementation and to provide a fast way for Django's internals to
        # access specific subsets of fields.
        # Creates a cache key composed of all arguments
        cache_key = (forward, reverse, include_parents, include_hidden, topmost_call)
            # In order to avoid list manipulation. Always return a shallow copy
            # of the results.
            return self._get_fields_cache[cache_key]
        # Recursively call _get_fields() on each parent, with the same
        # options provided in this call.
        if include_parents is not False:
            # In diamond inheritance it is possible that we see the same model
            # from two different routes. In that case, avoid adding fields from
            # the same parent again.
            parent_fields = set()
                    parent._meta.concrete_model != self.concrete_model
                    and include_parents == PROXY_PARENTS
                for obj in parent._meta._get_fields(
                    forward=forward,
                    reverse=reverse,
                    include_parents=include_parents,
                    include_hidden=include_hidden,
                    topmost_call=False,
                        not getattr(obj, "parent_link", False)
                        or obj.model == self.concrete_model
                    ) and obj not in parent_fields:
                        fields.append(obj)
                        parent_fields.add(obj)
        if reverse and not self.proxy:
            # Tree is computed once and cached until the app cache is expired.
            # It is composed of a list of fields pointing to the current model
            # from other models.
            all_fields = self._relation_tree
            for field in all_fields:
                # If hidden fields should be included or the relation is not
                # intentionally hidden, add to the fields dict.
                if include_hidden or not field.remote_field.hidden:
                    fields.append(field.remote_field)
            fields += self.local_fields
            fields += self.local_many_to_many
            # Private fields are recopied to each child model, and they get a
            # different model as field.model in each child. Hence we have to
            # add the private fields separately from the topmost call. If we
            # did this recursively similar to local_fields, we would get field
            # instances with field.model != self.model.
            if topmost_call:
                fields += self.private_fields
        # In order to avoid list manipulation. Always
        # return a shallow copy of the results
        fields = make_immutable_fields_list("get_fields()", fields)
        # Store result into cache for later access
        self._get_fields_cache[cache_key] = fields
    def total_unique_constraints(self):
        Return a list of total unique constraints. Useful for determining set
        of fields guaranteed to be unique for all rows.
            for constraint in self.constraints
                and constraint.condition is None
                and not constraint.contains_expressions
    def pk_fields(self):
        return composite.unnest([self.pk])
    def is_composite_pk(self):
        return isinstance(self.pk, CompositePrimaryKey)
    def _property_names(self):
        """Return a set of the names of the properties defined on the model."""
        names = set()
        for klass in self.model.__mro__:
            names |= {
                for name, value in klass.__dict__.items()
                if isinstance(value, property) and name not in seen
            seen |= set(klass.__dict__)
        return frozenset(names)
    def _non_pk_concrete_field_names(self):
        Return a set of the non-pk concrete field names defined on the model.
        names = []
        all_pk_fields = set(self.pk_fields)
        for parent in self.all_parents:
            all_pk_fields.update(parent._meta.pk_fields)
        for field in self.concrete_fields:
            if field not in all_pk_fields:
                names.append(field.name)
                if field.name != field.attname:
                    names.append(field.attname)
    def _reverse_one_to_one_field_names(self):
        Return a set of reverse one to one field names pointing to the current
        return frozenset(
            field.name for field in self.related_objects if field.one_to_one
    def db_returning_fields(self):
        Private API intended only to be used by Django itself.
        Fields to be returned after a database insert.
            for field in self._get_fields(
                forward=True, reverse=False, include_parents=PROXY_PARENTS
            if getattr(field, "db_returning", False)
"""Contains the logic for all of the default options for Flake8."""
from flake8 import defaults
from flake8.options.manager import OptionManager
def stage1_arg_parser() -> argparse.ArgumentParser:
    """Register the preliminary options on our OptionManager.
    The preliminary options include:
    - ``-v``/``--verbose``
    - ``--output-file``
    - ``--append-config``
    - ``--config``
    - ``--isolated``
    - ``--enable-extensions``
    parser = argparse.ArgumentParser(add_help=False)
        help="Print more information about what is happening in flake8. "
        "This option is repeatable and will increase verbosity each "
        "time it is repeated.",
        "--output-file", default=None, help="Redirect report to a file."
    # Config file options
        "--append-config",
        help="Provide extra config files to parse in addition to the files "
        "found by Flake8 by default. These files are the last ones read "
        "and so they take the highest precedence when multiple files "
        "provide the same option.",
        "--config",
        help="Path to the config file that will be the authoritative config "
        "source. This will cause Flake8 to ignore all other "
        "configuration files.",
        "--isolated",
        help="Ignore all configuration files.",
    # Plugin enablement options
        "--enable-extensions",
        help="Enable plugins and extensions that are otherwise disabled "
        "by default",
        "--require-plugins",
        help="Require specific plugins to be installed before running",
class JobsArgument:
    """Type callback for the --jobs argument."""
    def __init__(self, arg: str) -> None:
        """Parse and validate the --jobs argument.
        :param arg: The argument passed by argparse for validation
        self.is_auto = False
        self.n_jobs = -1
        if arg == "auto":
            self.is_auto = True
        elif arg.isdigit():
            self.n_jobs = int(arg)
            raise argparse.ArgumentTypeError(
                f"{arg!r} must be 'auto' or an integer.",
        """Representation for debugging."""
        return f"{type(self).__name__}({str(self)!r})"
        """Format our JobsArgument class."""
        return "auto" if self.is_auto else str(self.n_jobs)
def register_default_options(option_manager: OptionManager) -> None:
    """Register the default options on our OptionManager.
    The default options include:
    - ``-q``/``--quiet``
    - ``--color``
    - ``--count``
    - ``--exclude``
    - ``--extend-exclude``
    - ``--filename``
    - ``--format``
    - ``--hang-closing``
    - ``--ignore``
    - ``--extend-ignore``
    - ``--per-file-ignores``
    - ``--max-line-length``
    - ``--max-doc-length``
    - ``--indent-size``
    - ``--select``
    - ``--extend-select``
    - ``--disable-noqa``
    - ``--show-source``
    - ``--statistics``
    - ``--exit-zero``
    - ``-j``/``--jobs``
    - ``--tee``
    - ``--benchmark``
    - ``--bug-report``
    add_option = option_manager.add_option
    add_option(
        parse_from_config=True,
        help="Report only file names, or nothing. This option is repeatable.",
        "--color",
        choices=("auto", "always", "never"),
        default="auto",
        help="Whether to use color in output.  Defaults to `%(default)s`.",
        help="Print total number of errors to standard output after "
        "all other output.",
        "--exclude",
        metavar="patterns",
        default=",".join(defaults.EXCLUDE),
        comma_separated_list=True,
        normalize_paths=True,
        help="Comma-separated list of files or directories to exclude. "
        "(Default: %(default)s)",
        "--extend-exclude",
        help="Comma-separated list of files or directories to add to the list "
        "of excluded ones.",
        "--filename",
        default="*.py",
        help="Only check for filenames matching the patterns in this comma-"
        "separated list. (Default: %(default)s)",
        "--stdin-display-name",
        default="stdin",
        help="The name used when reporting errors from code passed via stdin. "
        "This is useful for editors piping the file contents to flake8. "
    # TODO(sigmavirus24): Figure out --first/--repeat
    # NOTE(sigmavirus24): We can't use choices for this option since users can
    # freely provide a format string and that will break if we restrict their
    # choices.
        metavar="format",
        default="default",
            f"Format errors according to the chosen formatter "
            f"({', '.join(sorted(option_manager.formatter_names))}) "
            f"or a format string containing %%-style "
            f"mapping keys (code, col, path, row, text). "
            f"For example, "
            f"``--format=pylint`` or ``--format='%%(path)s %%(code)s'``. "
            f"(Default: %(default)s)"
        "--hang-closing",
        help="Hang closing bracket instead of matching indentation of opening "
        "bracket's line.",
        "--ignore",
        metavar="errors",
            f"Comma-separated list of error codes to ignore (or skip). "
            f"For example, ``--ignore=E4,E51,W234``. "
            f"(Default: {','.join(defaults.IGNORE)})"
        "--extend-ignore",
        help="Comma-separated list of error codes to add to the list of "
        "ignored ones. For example, ``--extend-ignore=E4,E51,W234``.",
        "--per-file-ignores",
        help="A pairing of filenames and violation codes that defines which "
        "violations to ignore in a particular file. The filenames can be "
        "specified in a manner similar to the ``--exclude`` option and the "
        "violations work similarly to the ``--ignore`` and ``--select`` "
        "options.",
        "--max-line-length",
        metavar="n",
        default=defaults.MAX_LINE_LENGTH,
        help="Maximum allowed line length for the entirety of this run. "
        "--max-doc-length",
        help="Maximum allowed doc line length for the entirety of this run. "
        "--indent-size",
        default=defaults.INDENT_SIZE,
        help="Number of spaces used for indentation (Default: %(default)s)",
        "--select",
            "Limit the reported error codes to codes prefix-matched by this "
            "list.  "
            "You usually do not need to specify this option as the default "
            "includes all installed plugin codes.  "
            "For example, ``--select=E4,E51,W234``."
        "--extend-select",
            "Add additional error codes to the default ``--select``.  "
            "For example, ``--extend-select=E4,E51,W234``."
        "--disable-noqa",
        help='Disable the effect of "# noqa". This will report errors on '
        'lines with "# noqa" at the end.',
    # TODO(sigmavirus24): Decide what to do about --show-pep8
        help="Show the source generate each error or warning.",
        "--no-show-source",
        dest="show_source",
        parse_from_config=False,
        help="Negate --show-source",
        help="Count errors.",
    # Flake8 options
        "--exit-zero",
        help='Exit with status code "0" even if there are errors.',
        "-j",
        "--jobs",
        type=JobsArgument,
        help="Number of subprocesses to use to run checks in parallel. "
        'This is ignored on Windows. The default, "auto", will '
        "auto-detect the number of processors available to use. "
        "--tee",
        help="Write to stdout and output-file.",
    # Benchmarking
        "--benchmark",
        help="Print benchmark information about this run of Flake8",
    # Debugging
        "--bug-report",
        help="Print information necessary when preparing a bug report",
