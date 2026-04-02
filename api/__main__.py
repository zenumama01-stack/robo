from . import displayMousePosition
displayMousePosition()from argparse import ArgumentParser, ArgumentTypeError, SUPPRESS
from enum import IntEnum
from typing import List, Optional
from . import (AbortDownloadException, BadCredentialsException, Instaloader, InstaloaderException,
               InvalidArgumentException, LoginException, Post, Profile, ProfileNotExistsException, StoryItem,
               TwoFactorAuthRequiredException, __version__, load_structure_from_file)
from .instaloader import (get_default_session_filename, get_default_stamps_filename)
from .instaloadercontext import default_user_agent
from .lateststamps import LatestStamps
    import browser_cookie3
    bc3_library = True
    bc3_library = False
class ExitCode(IntEnum):
    NON_FATAL_ERROR = 1
    INIT_FAILURE = 2
    LOGIN_FAILURE = 3
    DOWNLOAD_ABORTED = 4
    USER_ABORTED = 5
    UNEXPECTED_ERROR = 99
def usage_string():
    # NOTE: duplicated in README.rst and docs/index.rst
    argv0 = os.path.basename(sys.argv[0])
    argv0 = "instaloader" if argv0 == "__main__.py" else argv0
    return """
{0} [--comments] [--geotags]
{2:{1}} [--stories] [--highlights] [--tagged] [--reels] [--igtv]
{2:{1}} [--login YOUR-USERNAME] [--fast-update]
{2:{1}} profile | "#hashtag" | %%location_id | :stories | :feed | :saved
{0} --help""".format(argv0, len(argv0), '')
def http_status_code_list(code_list_str: str) -> List[int]:
    codes = [int(s) for s in code_list_str.split(',')]
    for code in codes:
        if not 100 <= code <= 599:
            raise ArgumentTypeError("Invalid HTTP status code: {}".format(code))
    return codes
def filterstr_to_filterfunc(filter_str: str, item_type: type):
    """Takes an --post-filter=... or --storyitem-filter=... filter
     specification and makes a filter_func Callable out of it."""
    # The filter_str is parsed, then all names occurring in its AST are replaced by loads to post.<name>. A
    # function Post->bool is returned which evaluates the filter with the post as 'post' in its namespace.
    class TransformFilterAst(ast.NodeTransformer):
        def visit_Name(self, node: ast.Name):
            if not isinstance(node.ctx, ast.Load):
                raise InvalidArgumentException("Invalid filter: Modifying variables ({}) not allowed.".format(node.id))
            if node.id == "datetime":
                return node
            if not hasattr(item_type, node.id):
                raise InvalidArgumentException("Invalid filter: {} not a {} attribute.".format(node.id,
                                                                                               item_type.__name__))
            new_node = ast.Attribute(ast.copy_location(ast.Name('item', ast.Load()), node), node.id,
                                     ast.copy_location(ast.Load(), node))
            return ast.copy_location(new_node, node)
    input_filename = '<command line filter parameter>'
    compiled_filter = compile(TransformFilterAst().visit(ast.parse(filter_str, filename=input_filename, mode='eval')),
                              filename=input_filename, mode='eval')
    def filterfunc(item) -> bool:
        # pylint:disable=eval-used
        return bool(eval(compiled_filter, {'item': item, 'datetime': datetime.datetime}))
    return filterfunc
def get_cookies_from_instagram(domain, browser, cookie_file='', cookie_name=''):
    supported_browsers = {
        "brave": browser_cookie3.brave,
        "chrome": browser_cookie3.chrome,
        "chromium": browser_cookie3.chromium,
        "edge": browser_cookie3.edge,
        "firefox": browser_cookie3.firefox,
        "librewolf": browser_cookie3.librewolf,
        "opera": browser_cookie3.opera,
        "opera_gx": browser_cookie3.opera_gx,
        "safari": browser_cookie3.safari,
        "vivaldi": browser_cookie3.vivaldi,
    if browser not in supported_browsers:
        raise InvalidArgumentException("Loading cookies from the specified browser failed\n"
                                       "Supported browsers are Brave, Chrome, Chromium, Edge, Firefox, LibreWolf, "
                                       "Opera, Opera_GX, Safari and Vivaldi")
    cookies = {}
    browser_cookies = list(supported_browsers[browser](cookie_file=cookie_file))
    for cookie in browser_cookies:
        if domain in cookie.domain:
            cookies[cookie.name] = cookie.value
    if cookies:
        print(f"Cookies loaded successfully from {browser}")
        raise LoginException(f"No cookies found for Instagram in {browser}, "
                             f"Are you logged in successfully in {browser}?")
    if cookie_name:
        return cookies.get(cookie_name, {})
        return cookies
def import_session(browser, instaloader, cookiefile):
    cookie = get_cookies_from_instagram('instagram', browser, cookiefile)
    if cookie is not None:
        instaloader.context.update_cookies(cookie)
            raise LoginException(f"Not logged in. Are you logged in successfully in {browser}?")
        print(f"{username} has been successfully logged in.")
        print(f"Next time use --login={username} to reuse the same session.")
def _main(instaloader: Instaloader, targetlist: List[str],
          username: Optional[str] = None, password: Optional[str] = None,
          sessionfile: Optional[str] = None,
          download_profile_pic: bool = True, download_posts=True,
          download_stories: bool = False,
          download_highlights: bool = False,
          download_tagged: bool = False,
          download_reels: bool = False,
          download_igtv: bool = False,
          fast_update: bool = False,
          latest_stamps_file: Optional[str] = None,
          max_count: Optional[int] = None, post_filter_str: Optional[str] = None,
          storyitem_filter_str: Optional[str] = None,
          browser: Optional[str] = None,
          cookiefile: Optional[str] = None) -> ExitCode:
    """Download set of profiles, hashtags etc. and handle logging in and session files if desired."""
    # Parse and generate filter function
    post_filter = None
    if post_filter_str is not None:
        post_filter = filterstr_to_filterfunc(post_filter_str, Post)
        instaloader.context.log('Only download posts with property "{}".'.format(post_filter_str))
    storyitem_filter = None
    if storyitem_filter_str is not None:
        storyitem_filter = filterstr_to_filterfunc(storyitem_filter_str, StoryItem)
        instaloader.context.log('Only download storyitems with property "{}".'.format(storyitem_filter_str))
    latest_stamps = None
    if latest_stamps_file is not None:
        latest_stamps = LatestStamps(latest_stamps_file)
        instaloader.context.log(f"Using latest stamps from {latest_stamps_file}.")
    # load cookies if browser is not None
    if browser and bc3_library:
        import_session(browser.lower(), instaloader, cookiefile)
    elif browser and not bc3_library:
        raise InvalidArgumentException("browser_cookie3 library is needed to load cookies from browsers")
    # Login, if desired
    if username is not None:
        if not re.match(r"^[A-Za-z0-9._]+$", username):
            instaloader.context.error("Warning: Parameter \"{}\" for --login is not a valid username.".format(username))
            instaloader.load_session_from_file(username, sessionfile)
        except FileNotFoundError as err:
            if sessionfile is not None:
                print(err, file=sys.stderr)
            instaloader.context.log("Session file does not exist yet - Logging in.")
        if not instaloader.context.is_logged_in or username != instaloader.test_login():
            if password is not None:
                    instaloader.login(username, password)
                except TwoFactorAuthRequiredException:
                    # https://github.com/instaloader/instaloader/issues/1217
                    instaloader.context.error("Warning: There have been reports of 2FA currently not working. "
                                              "Consider importing session cookies from your browser with "
                                              "--load-cookies.")
                            code = input("Enter 2FA verification code: ")
                            instaloader.two_factor_login(code)
                        except BadCredentialsException as err:
                    instaloader.interactive_login(username)
                    print("\nInterrupted by user.", file=sys.stderr)
                    return ExitCode.USER_ABORTED
        instaloader.context.log("Logged in as %s." % username)
    # since 4.2.9 login is required for geotags
    if instaloader.download_geotags and not instaloader.context.is_logged_in:
        instaloader.context.error("Warning: Login is required to download geotags of posts.")
    # Try block for KeyboardInterrupt (save session on ^C)
    profiles = set()
    anonymous_retry_profiles = set()
    exit_code = ExitCode.SUCCESS
        # Generate set of profiles, already downloading non-profile targets
        for target in targetlist:
            if (target.endswith('.json') or target.endswith('.json.xz')) and os.path.isfile(target):
                with instaloader.context.error_catcher(target):
                    structure = load_structure_from_file(instaloader.context, target)
                    if isinstance(structure, Post):
                        if post_filter is not None and not post_filter(structure):
                            instaloader.context.log("<{} ({}) skipped>".format(structure, target), flush=True)
                        instaloader.context.log("Downloading {} ({})".format(structure, target))
                        instaloader.download_post(structure, os.path.dirname(target))
                    elif isinstance(structure, StoryItem):
                        if storyitem_filter is not None and not storyitem_filter(structure):
                        instaloader.context.log("Attempting to download {} ({})".format(structure, target))
                        instaloader.download_storyitem(structure, os.path.dirname(target))
                    elif isinstance(structure, Profile):
                        raise InvalidArgumentException("Profile JSON are ignored. Pass \"{}\" to download that profile"
                                                       .format(structure.username))
                        raise InvalidArgumentException("{} JSON file not supported as target"
                                                       .format(structure.__class__.__name__))
            # strip '/' characters to be more shell-autocompletion-friendly
            target = target.rstrip('/')
                if re.match(r"^@[A-Za-z0-9._]+$", target):
                    instaloader.context.log("Retrieving followees of %s..." % target[1:])
                    profile = Profile.from_username(instaloader.context, target[1:])
                    for followee in profile.get_followees():
                        instaloader.save_profile_id(followee)
                        profiles.add(followee)
                elif re.match(r"^#\w+$", target):
                    instaloader.download_hashtag(hashtag=target[1:], max_count=max_count, fast_update=fast_update,
                                                 post_filter=post_filter,
                                                 profile_pic=download_profile_pic, posts=download_posts)
                elif re.match(r"^-[A-Za-z0-9-_]+$", target):
                    instaloader.download_post(Post.from_shortcode(instaloader.context, target[1:]), target)
                elif re.match(r"^%[0-9]+$", target):
                    instaloader.download_location(location=target[1:], max_count=max_count, fast_update=fast_update,
                                                  post_filter=post_filter)
                elif target == ":feed":
                    instaloader.download_feed_posts(fast_update=fast_update, max_count=max_count,
                elif target == ":stories":
                    instaloader.download_stories(fast_update=fast_update, storyitem_filter=storyitem_filter)
                elif target == ":saved":
                    instaloader.download_saved_posts(fast_update=fast_update, max_count=max_count,
                elif re.match(r"^[A-Za-z0-9._]+$", target):
                    download_profile_content = download_posts or download_tagged or download_reels or download_igtv
                        profile = instaloader.check_profile_id(target, latest_stamps)
                        if instaloader.context.is_logged_in and profile.has_blocked_viewer:
                            if download_profile_pic or (
                                download_profile_content and not profile.is_private
                                raise ProfileNotExistsException("{} blocked you; But we download her anonymously."
                                                                .format(target))
                                instaloader.context.error("{} blocked you.".format(target))
                            profiles.add(profile)
                    except ProfileNotExistsException as err:
                        # Not only our profile.has_blocked_viewer condition raises ProfileNotExistsException,
                        # check_profile_id() also does, since access to blocked profile may be responded with 404.
                        if instaloader.context.is_logged_in and (download_profile_pic or download_profile_content):
                            instaloader.context.log(err)
                            instaloader.context.log("Trying again anonymously, helps in case you are just blocked.")
                            with instaloader.anonymous_copy() as anonymous_loader:
                                with instaloader.context.error_catcher():
                                    anonymous_retry_profiles.add(anonymous_loader.check_profile_id(target,
                                                                                                   latest_stamps))
                                    instaloader.context.error("Warning: {} will be downloaded anonymously (\"{}\")."
                                                              .format(target, err))
                    target_type = {
                        '#': 'hashtag',
                        '%': 'location',
                        '-': 'shortcode',
                    }.get(target[0], 'username')
                    raise ProfileNotExistsException('Invalid {} {}'.format(target_type, target))
        if len(profiles) > 1:
            instaloader.context.log("Downloading {} profiles: {}".format(len(profiles),
                                                                         ' '.join([p.username for p in profiles])))
        if instaloader.context.iphone_support and profiles and (download_profile_pic or download_posts) and \
           not instaloader.context.is_logged_in:
            instaloader.context.log("Hint: Login to download higher-quality versions of pictures.")
        instaloader.download_profiles(
            profiles,
            download_profile_pic,
            download_posts,
            download_tagged,
            download_igtv,
            download_highlights,
            download_stories,
            fast_update,
            post_filter,
            storyitem_filter,
            latest_stamps=latest_stamps,
            reels=download_reels,
        if anonymous_retry_profiles:
            instaloader.context.log("Downloading anonymously: {}"
                                    .format(' '.join([p.username for p in anonymous_retry_profiles])))
                anonymous_loader.download_profiles(
                    anonymous_retry_profiles,
                    fast_update=fast_update,
                    reels=download_reels
        exit_code = ExitCode.USER_ABORTED
    except AbortDownloadException as exc:
        print("\nDownload aborted: {}.".format(exc), file=sys.stderr)
        exit_code = ExitCode.DOWNLOAD_ABORTED
    # Save session if it is useful
    if instaloader.context.is_logged_in:
    # User might be confused if Instaloader does nothing
    if not targetlist:
            # Instaloader did at least save a session file
            instaloader.context.log("No targets were specified, thus nothing has been downloaded.")
            # Instaloader did not do anything
            instaloader.context.log("usage:" + usage_string())
            exit_code = ExitCode.INIT_FAILURE
    return exit_code
    parser = ArgumentParser(description=__doc__, add_help=False, usage=usage_string(),
                            epilog="The complete documentation can be found at "
                                   "https://instaloader.github.io/.",
                            fromfile_prefix_chars='+')
    g_targets = parser.add_argument_group("What to Download",
                                          "Specify a list of targets. For each of these, Instaloader creates a folder "
                                          "and downloads all posts. The following targets are supported:")
    g_targets.add_argument('profile', nargs='*',
                           help="Download profile. If an already-downloaded profile has been renamed, Instaloader "
                                "automatically finds it by its unique ID and renames the folder likewise.")
    g_targets.add_argument('_at_profile', nargs='*', metavar="@profile",
                           help="Download all followees of profile. Requires login. "
                                "Consider using :feed rather than @yourself.")
    g_targets.add_argument('_hashtag', nargs='*', metavar='"#hashtag"', help="Download #hashtag.")
    g_targets.add_argument('_location', nargs='*', metavar='%location_id',
                           help="Download %%location_id. Requires login.")
    g_targets.add_argument('_feed', nargs='*', metavar=":feed",
                           help="Download pictures from your feed. Requires login.")
    g_targets.add_argument('_stories', nargs='*', metavar=":stories",
                           help="Download the stories of your followees. Requires login.")
    g_targets.add_argument('_saved', nargs='*', metavar=":saved",
                           help="Download the posts that you marked as saved. Requires login.")
    g_targets.add_argument('_singlepost', nargs='*', metavar="-- -shortcode",
                           help="Download the post with the given shortcode")
    g_targets.add_argument('_json', nargs='*', metavar="filename.json[.xz]",
                           help="Re-Download the given object.")
    g_targets.add_argument('_fromfile', nargs='*', metavar="+args.txt",
                           help="Read targets (and options) from given textfile.")
    g_post = parser.add_argument_group("What to Download of each Post")
    g_prof = parser.add_argument_group("What to Download of each Profile")
    g_prof.add_argument('-P', '--profile-pic-only', action='store_true',
                        help=SUPPRESS)
    g_prof.add_argument('--no-posts', action='store_true',
                        help="Do not download regular posts.")
    g_prof.add_argument('--no-profile-pic', action='store_true',
                        help='Do not download profile picture.')
    g_post.add_argument('--slide', action='store',
                        help='Set what image/interval of a sidecar you want to download.')
    g_post.add_argument('--no-pictures', action='store_true',
                        help='Do not download post pictures. Cannot be used together with --fast-update. '
                             'Implies --no-video-thumbnails, does not imply --no-videos.')
    g_post.add_argument('-V', '--no-videos', action='store_true',
                        help='Do not download videos.')
    g_post.add_argument('--no-video-thumbnails', action='store_true',
                        help='Do not download thumbnails of videos.')
    g_post.add_argument('-G', '--geotags', action='store_true',
                        help='Download geotags when available. Geotags are stored as a '
                             'text file with the location\'s name and a Google Maps link. '
                             'This requires an additional request to the Instagram '
                             'server for each picture. Requires login.')
    g_post.add_argument('-C', '--comments', action='store_true',
                        help='Download and update comments for each post. '
                             'server for each post, which is why it is disabled by default. Requires login.')
    g_post.add_argument('--no-captions', action='store_true',
                        help='Do not create txt files.')
    g_post.add_argument('--post-metadata-txt', action='append',
                        help='Template to write in txt file for each Post.')
    g_post.add_argument('--storyitem-metadata-txt', action='append',
                        help='Template to write in txt file for each StoryItem.')
    g_post.add_argument('--no-metadata-json', action='store_true',
                        help='Do not create a JSON file containing the metadata of each post.')
    g_post.add_argument('--metadata-json', action='store_true',
    g_post.add_argument('--no-compress-json', action='store_true',
                        help='Do not xz compress JSON files, rather create pretty formatted JSONs.')
    g_prof.add_argument('-s', '--stories', action='store_true',
                        help='Also download stories of each profile that is downloaded. Requires login.')
    g_prof.add_argument('--stories-only', action='store_true',
    g_prof.add_argument('--highlights', action='store_true',
                        help='Also download highlights of each profile that is downloaded. Requires login.')
    g_prof.add_argument('--tagged', action='store_true',
                        help='Also download posts where each profile is tagged.')
    g_prof.add_argument('--reels', action='store_true',
                        help='Also download Reels videos.')
    g_prof.add_argument('--igtv', action='store_true',
                        help='Also download IGTV videos.')
    g_cond = parser.add_argument_group("Which Posts to Download")
    g_cond.add_argument('-F', '--fast-update', action='store_true',
                        help='For each target, stop when encountering the first already-downloaded picture. This '
                             'flag is recommended when you use Instaloader to update your personal Instagram archive.')
    g_cond.add_argument('--latest-stamps', nargs='?', metavar='STAMPSFILE', const=get_default_stamps_filename(),
                        help='Store the timestamps of latest media scraped for each profile. This allows updating '
                             'your personal Instagram archive even if you delete the destination directories. '
                             'If STAMPSFILE is not provided, defaults to ' + get_default_stamps_filename())
    g_cond.add_argument('--post-filter', '--only-if', metavar='filter',
                        help='Expression that, if given, must evaluate to True for each post to be downloaded. Must be '
                             'a syntactically valid python expression. Variables are evaluated to '
                             'instaloader.Post attributes. Example: --post-filter=viewer_has_liked.')
    g_cond.add_argument('--storyitem-filter', metavar='filter',
                        help='Expression that, if given, must evaluate to True for each storyitem to be downloaded. '
                             'Must be a syntactically valid python expression. Variables are evaluated to '
                             'instaloader.StoryItem attributes.')
    g_cond.add_argument('-c', '--count',
                        help='Do not attempt to download more than COUNT posts. '
                             'Applies to #hashtag, %%location_id, :feed, and :saved.')
    g_login = parser.add_argument_group('Login (Download Private Profiles)',
                                        'Instaloader can login to Instagram. This allows downloading private profiles. '
                                        'To login, pass the --login option. Your session cookie (not your password!) '
                                        'will be saved to a local file to be reused next time you want Instaloader '
                                        'to login. Instead of --login, the --load-cookies option can be used to '
                                        'import a session from a browser.')
    g_login.add_argument('-l', '--login', metavar='YOUR-USERNAME',
                         help='Login name (profile name) for your Instagram account.')
    g_login.add_argument('-b', '--load-cookies', metavar='BROWSER-NAME',
                         help='Browser name to load cookies from Instagram')
    g_login.add_argument('-B', '--cookiefile', metavar='COOKIE-FILE',
                         help='Cookie file of a profile to load cookies')
    g_login.add_argument('-f', '--sessionfile',
                         help='Path for loading and storing session key file. '
                              'Defaults to ' + get_default_session_filename("<login_name>"))
    g_login.add_argument('-p', '--password', metavar='YOUR-PASSWORD',
                         help='Password for your Instagram account. Without this option, '
                              'you\'ll be prompted for your password interactively if '
                              'there is not yet a valid session file.')
    g_how = parser.add_argument_group('How to Download')
    g_how.add_argument('--dirname-pattern',
                       help='Name of directory where to store posts. {profile} is replaced by the profile name, '
                            '{target} is replaced by the target you specified, i.e. either :feed, #hashtag or the '
                            'profile name. Defaults to \'{target}\'.')
    g_how.add_argument('--filename-pattern',
                       help='Prefix of filenames for posts and stories, relative to the directory given with '
                            '--dirname-pattern. {profile} is replaced by the profile name,'
                            '{target} is replaced by the target you specified, i.e. either :feed'
                            '#hashtag or the profile name. Defaults to \'{date_utc}_UTC\'')
    g_how.add_argument('--title-pattern',
                       help='Prefix of filenames for profile pics, hashtag profile pics, and highlight covers. '
                            'Defaults to \'{date_utc}_UTC_{typename}\' if --dirname-pattern contains \'{target}\' '
                            'or \'{dirname}\', or if --dirname-pattern is not specified. Otherwise defaults to '
                            '\'{target}_{date_utc}_UTC_{typename}\'.')
    g_how.add_argument('--resume-prefix', metavar='PREFIX',
                       help='Prefix for filenames that are used to save the information to resume an interrupted '
                            'download.')
    g_how.add_argument('--sanitize-paths', action='store_true',
                       help='Sanitize paths so that the resulting file and directory names are valid on both '
                            'Windows and Unix.')
    g_how.add_argument('--no-resume', action='store_true',
                       help='Do not resume a previously-aborted download iteration, and do not save such information '
                            'when interrupted.')
    g_how.add_argument('--use-aged-resume-files', action='store_true', help=SUPPRESS)
    g_how.add_argument('--user-agent',
                       help='User Agent to use for HTTP requests. Defaults to \'{}\'.'.format(default_user_agent()))
    g_how.add_argument('-S', '--no-sleep', action='store_true', help=SUPPRESS)
    g_how.add_argument('--max-connection-attempts', metavar='N', type=int, default=3,
                       help='Maximum number of connection attempts until a request is aborted. Defaults to 3. If a '
                            'connection fails, it can be manually skipped by hitting CTRL+C. Set this to 0 to retry '
                            'infinitely.')
    g_how.add_argument('--commit-mode', action='store_true', help=SUPPRESS)
    g_how.add_argument('--request-timeout', metavar='N', type=float, default=300.0,
                       help='Seconds to wait before timing out a connection request. Defaults to 300.')
    g_how.add_argument('--abort-on', type=http_status_code_list, metavar="STATUS_CODES",
                       help='Comma-separated list of HTTP status codes that cause Instaloader to abort, bypassing all '
                            'retry logic.')
    g_how.add_argument('--no-iphone', action='store_true',
                        help='Do not attempt to download iPhone version of images and videos.')
    g_misc = parser.add_argument_group('Miscellaneous Options')
    g_misc.add_argument('-q', '--quiet', action='store_true',
                        help='Disable user interaction, i.e. do not print messages (except errors) and fail '
                             'if login credentials are needed but not given. This makes Instaloader suitable as a '
                             'cron job.')
    g_misc.add_argument('-h', '--help', action='help', help='Show this help message and exit.')
    g_misc.add_argument('--version', action='version', help='Show version number and exit.',
                        version=__version__)
        if (args.login is None and args.load_cookies is None) and (args.stories or args.stories_only):
            print("Login is required to download stories.", file=sys.stderr)
            args.stories = False
            if args.stories_only:
                raise InvalidArgumentException()
        if ':feed-all' in args.profile or ':feed-liked' in args.profile:
            raise InvalidArgumentException(":feed-all and :feed-liked were removed. Use :feed as target and "
                                           "eventually --post-filter=viewer_has_liked.")
        post_metadata_txt_pattern = '\n'.join(args.post_metadata_txt) if args.post_metadata_txt else None
        storyitem_metadata_txt_pattern = '\n'.join(args.storyitem_metadata_txt) if args.storyitem_metadata_txt else None
        if args.no_captions:
            if not (post_metadata_txt_pattern or storyitem_metadata_txt_pattern):
                post_metadata_txt_pattern = ''
                storyitem_metadata_txt_pattern = ''
                raise InvalidArgumentException("--no-captions and --post-metadata-txt or --storyitem-metadata-txt "
                                               "given; That contradicts.")
        if args.no_resume and args.resume_prefix:
            raise InvalidArgumentException("--no-resume and --resume-prefix given; That contradicts.")
        resume_prefix = (args.resume_prefix if args.resume_prefix else 'iterator') if not args.no_resume else None
        if args.no_pictures and args.fast_update:
            raise InvalidArgumentException('--no-pictures and --fast-update cannot be used together.')
        if args.login and args.load_cookies:
            raise InvalidArgumentException('--load-cookies and --login cannot be used together.')
        # Determine what to download
        download_profile_pic = not args.no_profile_pic or args.profile_pic_only
        download_posts = not (args.no_posts or args.stories_only or args.profile_pic_only)
        download_stories = args.stories or args.stories_only
        loader = Instaloader(sleep=not args.no_sleep, quiet=args.quiet, user_agent=args.user_agent,
                             dirname_pattern=args.dirname_pattern, filename_pattern=args.filename_pattern,
                             download_pictures=not args.no_pictures,
                             download_videos=not args.no_videos, download_video_thumbnails=not args.no_video_thumbnails,
                             download_geotags=args.geotags,
                             download_comments=args.comments, save_metadata=not args.no_metadata_json,
                             compress_json=not args.no_compress_json,
                             post_metadata_txt_pattern=post_metadata_txt_pattern,
                             storyitem_metadata_txt_pattern=storyitem_metadata_txt_pattern,
                             max_connection_attempts=args.max_connection_attempts,
                             request_timeout=args.request_timeout,
                             resume_prefix=resume_prefix,
                             check_resume_bbd=not args.use_aged_resume_files,
                             slide=args.slide,
                             fatal_status_codes=args.abort_on,
                             iphone_support=not args.no_iphone,
                             title_pattern=args.title_pattern,
                             sanitize_paths=args.sanitize_paths)
        exit_code = _main(loader,
                          args.profile,
                          username=args.login.lower() if args.login is not None else None,
                          password=args.password,
                          sessionfile=args.sessionfile,
                          download_profile_pic=download_profile_pic,
                          download_posts=download_posts,
                          download_stories=download_stories,
                          download_highlights=args.highlights,
                          download_tagged=args.tagged,
                          download_reels=args.reels,
                          download_igtv=args.igtv,
                          fast_update=args.fast_update,
                          latest_stamps_file=args.latest_stamps,
                          max_count=int(args.count) if args.count is not None else None,
                          post_filter_str=args.post_filter,
                          storyitem_filter_str=args.storyitem_filter,
                          browser=args.load_cookies,
                          cookiefile=args.cookiefile)
        loader.close()
        if loader.has_stored_errors:
            exit_code = ExitCode.NON_FATAL_ERROR
    except InvalidArgumentException as err:
    except LoginException as err:
        exit_code = ExitCode.LOGIN_FAILURE
    except InstaloaderException as err:
        print("Fatal error: %s" % err)
        exit_code = ExitCode.UNEXPECTED_ERROR
    sys.exit(exit_code)
"""CLI entry point for notebook."""
from notebook.app import main
sys.exit(main())  # type:ignore[no-untyped-call]
from .cli import main
displayMousePosition()from argparse import ArgumentParser, ArgumentTypeError, SUPPRESS
# Execute with
# $ python3 -m yt_dlp
if __package__ is None and not getattr(sys, 'frozen', False):
    # direct call of __main__.py
    path = os.path.realpath(os.path.abspath(__file__))
    sys.path.insert(0, os.path.dirname(os.path.dirname(path)))
import yt_dlp
    yt_dlp.main()
Main module for spotdl. Exports version and main function.
    console_entry_point()
# Copyright (c) 2013-2023, PyInstaller Development Team.
Main command-line interface to PyInstaller.
from PyInstaller import __version__
from PyInstaller import log as logging
# Note: do not import anything else until compat.check_requirements function is run!
    from argcomplete import autocomplete
    def autocomplete(parser):
# Taken from https://stackoverflow.com/a/22157136 to format args more flexibly: any help text which beings with ``R|``
# will have all newlines preserved; the help text will be line wrapped. See
# https://docs.python.org/3/library/argparse.html#formatter-class.
# This is used by the ``--debug`` option.
class _SmartFormatter(argparse.HelpFormatter):
    def _split_lines(self, text, width):
        if text.startswith('R|'):
            # The underlying implementation of ``RawTextHelpFormatter._split_lines`` invokes this; mimic it.
            return text[2:].splitlines()
            # Invoke the usual formatter.
            return super()._split_lines(text, width)
def run_makespec(filenames, **opts):
    # Split pathex by using the path separator
    temppaths = opts['pathex'][:]
    pathex = opts['pathex'] = []
    for p in temppaths:
        pathex.extend(p.split(os.pathsep))
    import PyInstaller.building.makespec
    spec_file = PyInstaller.building.makespec.main(filenames, **opts)
    logger.info('wrote %s' % spec_file)
    return spec_file
def run_build(pyi_config, spec_file, **kwargs):
    import PyInstaller.building.build_main
    PyInstaller.building.build_main.main(pyi_config, spec_file, **kwargs)
def __add_options(parser):
        help='Show program version info and exit.',
class _PyiArgumentParser(argparse.ArgumentParser):
        self._pyi_action_groups = defaultdict(list)
    def _add_options(self, __add_options: callable, name: str = ""):
        Mutate self with the given callable, storing any new actions added in a named group
        n_actions_before = len(getattr(self, "_actions", []))
        __add_options(self)  # preserves old behavior
        new_actions = getattr(self, "_actions", [])[n_actions_before:]
        self._pyi_action_groups[name].extend(new_actions)
    def _option_name(self, action):
        Get the option name(s) associated with an action
        For options that define both short and long names, this function will
        return the long names joined by "/"
        longnames = [name for name in action.option_strings if name.startswith("--")]
        if longnames:
            name = "/".join(longnames)
            name = action.option_strings[0]
    def _forbid_options(self, args: argparse.Namespace, group: str, errmsg: str = ""):
        """Forbid options from a named action group"""
        options = defaultdict(str)
        for action in self._pyi_action_groups[group]:
            dest = action.dest
            name = self._option_name(action)
            if getattr(args, dest) is not self.get_default(dest):
                if dest in options:
                    options[dest] += "/"
                options[dest] += name
        # if any options from the forbidden group are not the default values,
        # the user must have passed them in, so issue an error report
        if options:
            sep = "\n  "
            bad = sep.join(options.values())
            if errmsg:
                errmsg = "\n" + errmsg
            raise SystemExit(f"ERROR: option(s) not allowed:{sep}{bad}{errmsg}")
def generate_parser() -> _PyiArgumentParser:
    Build an argparse parser for PyInstaller's main CLI.
    import PyInstaller.log
    parser = _PyiArgumentParser(formatter_class=_SmartFormatter)
    parser.prog = "pyinstaller"
    parser._add_options(__add_options)
    parser._add_options(PyInstaller.building.makespec.__add_options, name="makespec")
    parser._add_options(PyInstaller.building.build_main.__add_options, name="build_main")
    parser._add_options(PyInstaller.log.__add_options, name="log")
        'filenames',
        metavar='scriptname',
        nargs='+',
        help="Name of scriptfiles to be processed or exactly one .spec file. If a .spec file is specified, most "
        "options are unnecessary and are ignored.",
def run(pyi_args: list | None = None, pyi_config: dict | None = None):
    pyi_args     allows running PyInstaller programmatically without a subprocess
    pyi_config   allows checking configuration once when running multiple tests
    compat.check_requirements()
    check_unsafe_privileges()
    old_sys_argv = sys.argv
        parser = generate_parser()
        autocomplete(parser)
        if pyi_args is None:
            pyi_args = sys.argv[1:]
            index = pyi_args.index("--")
            index = len(pyi_args)
        args = parser.parse_args(pyi_args[:index])
        spec_args = pyi_args[index + 1:]
        PyInstaller.log.__process_options(parser, args)
        # Print PyInstaller version, Python version, and platform as the first line to stdout. This helps us identify
        # PyInstaller, Python, and platform version when users report issues.
            from _pyinstaller_hooks_contrib import __version__ as contrib_hooks_version
            contrib_hooks_version = 'unknown'
        logger.info('PyInstaller: %s, contrib hooks: %s', __version__, contrib_hooks_version)
        logger.info('Python: %s%s', platform.python_version(), " (conda)" if compat.is_conda else "")
        logger.info('Platform: %s', platform.platform())
        logger.info('Python environment: %s', sys.prefix)
        # Skip creating .spec when .spec file is supplied.
        if args.filenames[0].endswith('.spec'):
            parser._forbid_options(
                args, group="makespec", errmsg="makespec options not valid when a .spec file is given"
            spec_file = args.filenames[0]
            # Ensure that the given script files exist, before trying to generate the .spec file.
            # This prevents us from overwriting an existing (and customized) .spec file if user makes a typo in the
            # .spec file's suffix when trying to  build it, for example, `pyinstaller program.cpes` (see #8276).
            # It also prevents creation of a .spec file when `pyinstaller program.py` is accidentally ran from a
            # directory that does not contain the script (for example, due to failing to change the directory prior
            # to running the command).
                    raise SystemExit(f"ERROR: Script file {filename!r} does not exist.")
            spec_file = run_makespec(**vars(args))
        sys.argv = [spec_file, *spec_args]
        run_build(pyi_config, spec_file, **vars(args))
        raise SystemExit("Aborted by user request.")
    except RecursionError:
        from PyInstaller import _recursion_too_deep_message
        _recursion_too_deep_message.raise_with_msg()
        sys.argv = old_sys_argv
def _console_script_run():
    # Python prepends the main script's parent directory to sys.path. When PyInstaller is ran via the usual
    # `pyinstaller` CLI entry point, this directory is $pythonprefix/bin which should not be in sys.path.
    if os.path.basename(sys.path[0]) in ("bin", "Scripts"):
        sys.path.pop(0)
    run()
def check_unsafe_privileges():
    Forbid dangerous usage of PyInstaller with escalated privileges
    if compat.is_win and not compat.is_win_wine:
        # Discourage (with the intention to eventually block) people using *run as admin* with PyInstaller.
        # There are 4 cases, block case 3 but be careful not to also block case 2.
        #   1. User has no admin access: TokenElevationTypeDefault
        #   2. User is an admin/UAC disabled (common on CI/VMs): TokenElevationTypeDefault
        #   3. User has used *run as administrator* to elevate: TokenElevationTypeFull
        #   4. User can escalate but hasn't: TokenElevationTypeLimited
        # https://techcommunity.microsoft.com/t5/windows-blog-archive/how-to-determine-if-a-user-is-a-member-of-the-administrators/ba-p/228476
        advapi32 = ctypes.CDLL("Advapi32.dll")
        kernel32 = ctypes.CDLL("kernel32.dll")
        kernel32.GetCurrentProcess.restype = ctypes.c_void_p
        process = kernel32.GetCurrentProcess()
        token = ctypes.c_void_p()
            TOKEN_QUERY = 8
            assert advapi32.OpenProcessToken(ctypes.c_void_p(process), TOKEN_QUERY, ctypes.byref(token))
            elevation_type = ctypes.c_int()
            TokenElevationType = 18
            assert advapi32.GetTokenInformation(
                token, TokenElevationType, ctypes.byref(elevation_type), ctypes.sizeof(elevation_type),
                ctypes.byref(ctypes.c_int())
            kernel32.CloseHandle(token)
        if elevation_type.value == 2:  # TokenElevationTypeFull
            logger.log(
                logging.DEPRECATION,
                "Running PyInstaller as admin is not necessary nor sensible. Run PyInstaller from a non-administrator "
                "terminal. PyInstaller 7.0 will block this."
    elif compat.is_darwin or compat.is_linux:
        # Discourage (with the intention to eventually block) people using *sudo* with PyInstaller.
        # Again there are 4 cases, block only case 4.
        #   1. Non-root: os.getuid() != 0
        #   2. Logged in as root (usually a VM): os.getlogin() == "root", os.getuid() == 0
        #   3. No named users (e.g. most Docker containers): os.getlogin() fails
        #   4. Regular user using escalation: os.getlogin() != "root", os.getuid() == 0
            user = os.getlogin()
            user = ""
        if os.getuid() == 0 and user and user != "root":
                "Running PyInstaller as root is not necessary nor sensible. Do not use PyInstaller with sudo. "
                "PyInstaller 7.0 will block this."
    if compat.is_win:
        # Do not let people run PyInstaller from admin cmd's default working directory (C:\Windows\system32)
        cwd = pathlib.Path.cwd()
            win_dir = compat.win32api.GetWindowsDirectory()
            win_dir = None
        win_dir = None if win_dir is None else pathlib.Path(win_dir).resolve()
        inside_win_dir = cwd == win_dir or win_dir in cwd.parents
        # The only exception to the above is if user's home directory is also located under %WINDIR%, which happens
        # when PyInstaller is ran under SYSTEM user.
        if inside_win_dir:
            home_dir = pathlib.Path.home().resolve()
            if cwd == home_dir or home_dir in cwd.parents:
                inside_win_dir = False
            raise SystemExit(
                f"ERROR: Do not run pyinstaller from {cwd}. cd to where your code is and run pyinstaller from there. "
                "Hint: You can open a terminal where your code is by going to the parent folder in Windows file "
                "explorer and typing cmd into the address bar."
from .modulegraph import ModuleGraph
def parse_arguments():
        conflict_handler='resolve', prog='%s -mmodulegraph' % (
            os.path.basename(sys.executable)))
        '-d', action='count', dest='debug', default=1,
        help='Increase debug level')
        '-q', action='store_const', dest='debug', const=0,
        help='Clear debug level')
        '-m', '--modules', action='store_true',
        dest='domods', default=False,
        help='arguments are module names, not script files')
        '-x', metavar='NAME', action='append', dest='excludes',
        default=[], help='Add NAME to the excludes list')
        '-p', action='append', metavar='PATH', dest='addpath', default=[],
        help='Add PATH to the module search path')
        '-g', '--dot', action='store_const', dest='output', const='dot',
        help='Output a .dot graph')
        '-h', '--html', action='store_const',
        dest='output', const='html', help='Output a HTML file')
        'scripts', metavar='SCRIPT', nargs='+', help='scripts to analyse')
def create_graph(scripts, domods, debuglevel, excludes, path_extras):
    # Set the path based on sys.path and the script directory
    path = sys.path[:]
    if domods:
        del path[0]
        path[0] = os.path.dirname(scripts[0])
    path = path_extras + path
    if debuglevel > 1:
        print("path:", file=sys.stderr)
        for item in path:
            print("   ", repr(item), file=sys.stderr)
    # Create the module finder and turn its crank
    mf = ModuleGraph(path, excludes=excludes, debug=debuglevel)
    for arg in scripts:
            if arg[-2:] == '.*':
                mf.import_hook(arg[:-2], None, ["*"])
                mf.import_hook(arg)
            mf.add_script(arg)
    return mf
def output_graph(output_format, mf):
    if output_format == 'dot':
        mf.graphreport()
    elif output_format == 'html':
        mf.create_xref()
        mf.report()
    opts = parse_arguments()
    mf = create_graph(
        opts.scripts, opts.domods, opts.debug,
        opts.excludes, opts.addpath)
    output_graph(opts.output, mf)
if __name__ == '__main__':  # pragma: no cover
        print("\n[interrupt]")
from fastapi.cli import main
Invokes django-admin when the django module is run as a script.
Example: python -m django check
from django.core import management
    management.execute_from_command_line()
# Remove '' and current working directory from the first entry
# of sys.path, if present to avoid using current directory
# in pip commands check, freeze, install, list and show,
# when invoked as python -m pip <command>
if sys.path[0] in ("", os.getcwd()):
# If we are running from a wheel, add the wheel to sys.path
# This allows the usage python pip-*.whl/pip install pip-*.whl
if __package__ == "":
    # __file__ is pip-*.whl/pip/__main__.py
    # first dirname call strips of '/__main__.py', second strips off '/pip'
    # Resulting path is the name of the wheel itself
    # Add that to sys.path so we can import pip
    path = os.path.dirname(os.path.dirname(__file__))
    sys.path.insert(0, path)
    from pip._internal.cli.main import main as _main
    sys.exit(_main())
from pip._vendor.certifi import contents, where
parser.add_argument("-c", "--contents", action="store_true")
if args.contents:
    print(contents())
    print(where())
from ._implementation import resolve
from ._toml_compat import tomllib
    if tomllib is None:
            "Usage error: dependency-groups CLI requires tomli or Python 3.11+",
        raise SystemExit(2)
        description=(
            "A dependency-groups CLI. Prints out a resolved group, newline-delimited."
        "GROUP_NAME", nargs="*", help="The dependency group(s) to resolve."
        "--pyproject-file",
        default="pyproject.toml",
        help="The pyproject.toml file. Defaults to trying in the current directory.",
        "--output",
        help="An output file. Defaults to stdout.",
        "--list",
        help="List the available dependency groups",
    with open(args.pyproject_file, "rb") as fp:
        pyproject = tomllib.load(fp)
    dependency_groups_raw = pyproject.get("dependency-groups", {})
    if args.list:
        print(*dependency_groups_raw.keys())
    if not args.GROUP_NAME:
        print("A GROUP_NAME is required", file=sys.stderr)
        raise SystemExit(3)
    content = "\n".join(resolve(dependency_groups_raw, *args.GROUP_NAME))
    if args.output is None or args.output == "-":
        print(content)
        with open(args.output, "w", encoding="utf-8") as fp:
            print(content, file=fp)
from .distro import main
"""Main entry point."""
from pip._vendor.platformdirs import PlatformDirs, __version__
PROPS = (
    """Run the main entry point."""
    app_name = "MyApp"
    app_author = "MyCompany"
    print(f"-- platformdirs {__version__} --")  # noqa: T201
    print("-- app dirs (with optional 'version')")  # noqa: T201
    dirs = PlatformDirs(app_name, app_author, version="1.0")
    for prop in PROPS:
        print(f"{prop}: {getattr(dirs, prop)}")  # noqa: T201
    print("\n-- app dirs (without optional 'version')")  # noqa: T201
    dirs = PlatformDirs(app_name, app_author)
    print("\n-- app dirs (without optional 'appauthor')")  # noqa: T201
    dirs = PlatformDirs(app_name)
    print("\n-- app dirs (with disabled 'appauthor')")  # noqa: T201
    dirs = PlatformDirs(app_name, appauthor=False)
    pygments.__main__
    ~~~~~~~~~~~~~~~~~
    Main entry point for ``python -m pygments``.
from pip._vendor.pygments.cmdline import main
import colorsys
from time import process_time
from pip._vendor.rich import box
from pip._vendor.rich.color import Color
from pip._vendor.rich.console import Console, ConsoleOptions, Group, RenderableType, RenderResult
from pip._vendor.rich.markdown import Markdown
from pip._vendor.rich.measure import Measurement
from pip._vendor.rich.pretty import Pretty
from pip._vendor.rich.segment import Segment
from pip._vendor.rich.style import Style
from pip._vendor.rich.syntax import Syntax
from pip._vendor.rich.table import Table
class ColorBox:
        self, console: Console, options: ConsoleOptions
        for y in range(0, 5):
            for x in range(options.max_width):
                h = x / options.max_width
                l = 0.1 + ((y / 5) * 0.7)
                r1, g1, b1 = colorsys.hls_to_rgb(h, l, 1.0)
                r2, g2, b2 = colorsys.hls_to_rgb(h, l + 0.7 / 10, 1.0)
                bgcolor = Color.from_rgb(r1 * 255, g1 * 255, b1 * 255)
                color = Color.from_rgb(r2 * 255, g2 * 255, b2 * 255)
                yield Segment("▄", Style(color=color, bgcolor=bgcolor))
            yield Segment.line()
    def __rich_measure__(
        self, console: "Console", options: ConsoleOptions
    ) -> Measurement:
        return Measurement(1, options.max_width)
def make_test_card() -> Table:
    """Get a renderable that demonstrates a number of features."""
    table = Table.grid(padding=1, pad_edge=True)
    table.title = "Rich features"
    table.add_column("Feature", no_wrap=True, justify="center", style="bold red")
    table.add_column("Demonstration")
    color_table = Table(
        box=None,
        expand=False,
        show_header=False,
        show_edge=False,
        pad_edge=False,
    color_table.add_row(
            "✓ [bold green]4-bit color[/]\n"
            "✓ [bold blue]8-bit color[/]\n"
            "✓ [bold magenta]Truecolor (16.7 million)[/]\n"
            "✓ [bold yellow]Dumb terminals[/]\n"
            "✓ [bold cyan]Automatic color conversion"
        ColorBox(),
    table.add_row("Colors", color_table)
    table.add_row(
        "Styles",
        "All ansi styles: [bold]bold[/], [dim]dim[/], [italic]italic[/italic], [underline]underline[/], [strike]strikethrough[/], [reverse]reverse[/], and even [blink]blink[/].",
    lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque in metus sed sapien ultricies pretium a at justo. Maecenas luctus velit et auctor maximus."
    lorem_table = Table.grid(padding=1, collapse_padding=True)
    lorem_table.pad_edge = False
    lorem_table.add_row(
        Text(lorem, justify="left", style="green"),
        Text(lorem, justify="center", style="yellow"),
        Text(lorem, justify="right", style="blue"),
        Text(lorem, justify="full", style="red"),
        "Text",
            Text.from_markup(
                """Word wrap text. Justify [green]left[/], [yellow]center[/], [blue]right[/] or [red]full[/].\n"""
            lorem_table,
    def comparison(renderable1: RenderableType, renderable2: RenderableType) -> Table:
        table = Table(show_header=False, pad_edge=False, box=None, expand=True)
        table.add_column("1", ratio=1)
        table.add_column("2", ratio=1)
        table.add_row(renderable1, renderable2)
        "Asian\nlanguage\nsupport",
        ":flag_for_china:  该库支持中文，日文和韩文文本！\n:flag_for_japan:  ライブラリは中国語、日本語、韓国語のテキストをサポートしています\n:flag_for_south_korea:  이 라이브러리는 중국어, 일본어 및 한국어 텍스트를 지원합니다",
    markup_example = (
        "[bold magenta]Rich[/] supports a simple [i]bbcode[/i]-like [b]markup[/b] for [yellow]color[/], [underline]style[/], and emoji! "
        ":+1: :apple: :ant: :bear: :baguette_bread: :bus: "
    table.add_row("Markup", markup_example)
    example_table = Table(
        show_header=True,
        row_styles=["none", "dim"],
        box=box.SIMPLE,
    example_table.add_column("[green]Date", style="green", no_wrap=True)
    example_table.add_column("[blue]Title", style="blue")
    example_table.add_column(
        "[cyan]Production Budget",
        style="cyan",
        justify="right",
        no_wrap=True,
        "[magenta]Box Office",
        style="magenta",
    example_table.add_row(
        "Dec 20, 2019",
        "Star Wars: The Rise of Skywalker",
        "$275,000,000",
        "$375,126,118",
        "May 25, 2018",
        "[b]Solo[/]: A Star Wars Story",
        "$393,151,347",
        "Dec 15, 2017",
        "Star Wars Ep. VIII: The Last Jedi",
        "$262,000,000",
        "[bold]$1,332,539,889[/bold]",
        "May 19, 1999",
        "Star Wars Ep. [b]I[/b]: [i]The phantom Menace",
        "$115,000,000",
        "$1,027,044,677",
    table.add_row("Tables", example_table)
    code = '''\
def iter_last(values: Iterable[T]) -> Iterable[Tuple[bool, T]]:
    """Iterate and generate a tuple with a flag for last value."""
    iter_values = iter(values)
        previous_value = next(iter_values)
    for value in iter_values:
        yield False, previous_value
        previous_value = value
    yield True, previous_value'''
    pretty_data = {
            3.1427,
                "Paul Atreides",
                "Vladimir Harkonnen",
                "Thufir Hawat",
        "atomic": (False, True, None),
        "Syntax\nhighlighting\n&\npretty\nprinting",
        comparison(
            Syntax(code, "python3", line_numbers=True, indent_guides=True),
            Pretty(pretty_data, indent_guides=True),
    markdown_example = """\
# Markdown
Supports much of the *markdown* __syntax__!
- Headers
- Basic formatting: **bold**, *italic*, `code`
- Block quotes
- Lists, and more...
        "Markdown", comparison("[cyan]" + markdown_example, Markdown(markdown_example))
        "+more!",
        """Progress bars, columns, styled logging handler, tracebacks, etc...""",
    from pip._vendor.rich.panel import Panel
    console = Console(
        file=io.StringIO(),
        force_terminal=True,
    test_card = make_test_card()
    # Print once to warm cache
    start = process_time()
    console.print(test_card)
    pre_cache_taken = round((process_time() - start) * 1000.0, 1)
    console.file = io.StringIO()
    taken = round((process_time() - start) * 1000.0, 1)
    c = Console(record=True)
    c.print(test_card)
    console = Console()
    console.print(f"[dim]rendered in [not dim]{pre_cache_taken}ms[/] (cold cache)")
    console.print(f"[dim]rendered in [not dim]{taken}ms[/] (warm cache)")
    console.print()
    console.print(
        Panel.fit(
            "[b magenta]Hope you enjoy using Rich![/]\n\n"
            "Please consider sponsoring me if you get value from my work.\n\n"
            "Even the price of a ☕ can brighten my day!\n\n"
            "https://github.com/sponsors/willmcgugan",
            border_style="red",
            title="Help ensure Rich is maintained",
if len(sys.argv) > 1 and sys.argv[1] in ('-c', '--copy'):
    if len(sys.argv) > 2:
        pyperclip.copy(sys.argv[2])
        pyperclip.copy(sys.stdin.read())
elif len(sys.argv) > 1 and sys.argv[1] in ('-p', '--paste'):
    sys.stdout.write(pyperclip.paste())
    print('Usage: python -m pyperclip [-c | --copy] [text_to_copy] | [-p | --paste]')
    print('If a text_to_copy argument is provided, it is copied to the')
    print('clipboard. Otherwise, the stdin stream is copied to the')
    print('clipboard. (If reading this in from the keyboard, press')
    print('CTRL-Z on Windows or CTRL-D on Linux/macOS to stop.')
    print('When pasting, the clipboard will be written to stdout.')from .main import main
    sys.exit(main(sys.argv[1:]) or 0)
"""Entry point for cli, enables execution with `python -m dotenv`"""
from .cli import cli
    cli()
import pygments.cmdline
    sys.exit(pygments.cmdline.main(sys.argv))
from pyflakes.api import main
# python -m pyflakes
    main(prog='pyflakes')
from .features import pilinfo
pilinfo(supported_formats="--report" not in sys.argv)
# https://web.archive.org/web/20140822061353/http://cens.ioc.ee/projects/f2py2e
from numpy.f2py.f2py2e import main
# pragma: no cover
from json5.tool import main
from .cli import cli_detect
    cli_detect()
from os.path import abspath, basename, dirname, join, realpath
from unicodedata import unidata_version
import charset_normalizer.md as md_module
from charset_normalizer import from_fp
from charset_normalizer.models import CliDetectionResult
from charset_normalizer.version import __version__
def query_yes_no(question: str, default: str = "yes") -> bool:
    """Ask a yes/no question via input() and return their answer.
    "question" is a string that is presented to the user.
    "default" is the presumed answer if the user just hits <Enter>.
        It must be "yes" (the default), "no" or None (meaning
        an answer is required of the user).
    The "answer" return value is True for "yes" or False for "no".
    Credit goes to (c) https://stackoverflow.com/questions/3041986/apt-command-line-interface-like-yes-no-input
    valid = {"yes": True, "y": True, "ye": True, "no": False, "n": False}
        prompt = " [y/n] "
    elif default == "yes":
        prompt = " [Y/n] "
    elif default == "no":
        prompt = " [y/N] "
        raise ValueError("invalid default answer: '%s'" % default)
        sys.stdout.write(question + prompt)
        choice = input().lower()
        if default is not None and choice == "":
            return valid[default]
        elif choice in valid:
            return valid[choice]
            sys.stdout.write("Please respond with 'yes' or 'no' (or 'y' or 'n').\n")
class FileType:
    """Factory for creating file object types
    Instances of FileType are typically passed as type= arguments to the
    ArgumentParser add_argument() method.
    Keyword Arguments:
        - mode -- A string indicating how the file is to be opened. Accepts the
            same values as the builtin open() function.
        - bufsize -- The file's desired buffer size. Accepts the same values as
            the builtin open() function.
        - encoding -- The file's encoding. Accepts the same values as the
            builtin open() function.
        - errors -- A string indicating how encoding and decoding errors are to
            be handled. Accepts the same value as the builtin open() function.
    Backported from CPython 3.12
        mode: str = "r",
        bufsize: int = -1,
        encoding: str | None = None,
        errors: str | None = None,
        self._bufsize = bufsize
        self._encoding = encoding
    def __call__(self, string: str) -> typing.IO:  # type: ignore[type-arg]
        # the special argument "-" means sys.std{in,out}
        if string == "-":
            if "r" in self._mode:
                return sys.stdin.buffer if "b" in self._mode else sys.stdin
            elif any(c in self._mode for c in "wax"):
                return sys.stdout.buffer if "b" in self._mode else sys.stdout
                msg = f'argument "-" with mode {self._mode}'
        # all other arguments are used as file names
            return open(string, self._mode, self._bufsize, self._encoding, self._errors)
            message = f"can't open '{string}': {e}"
            raise argparse.ArgumentTypeError(message)
        args = self._mode, self._bufsize
        kwargs = [("encoding", self._encoding), ("errors", self._errors)]
        args_str = ", ".join(
            [repr(arg) for arg in args if arg != -1]
            + [f"{kw}={arg!r}" for kw, arg in kwargs if arg is not None]
        return f"{type(self).__name__}({args_str})"
def cli_detect(argv: list[str] | None = None) -> int:
    CLI assistant using ARGV and ArgumentParser
    :param argv:
    :return: 0 if everything is fine, anything else equal trouble
        description="The Real First Universal Charset Detector. "
        "Discover originating encoding used on text file. "
        "Normalize text to unicode."
        "files", type=FileType("rb"), nargs="+", help="File(s) to be analysed"
        dest="verbose",
        help="Display complementary information about file if any. "
        "Stdout will contain logs about the detection process.",
        "--with-alternative",
        dest="alternatives",
        help="Output complementary possibilities if any. Top-level JSON WILL be a list.",
        "--normalize",
        dest="normalize",
        help="Permit to normalize input file. If not set, program does not write anything.",
        "--minimal",
        dest="minimal",
        help="Only output the charset detected to STDOUT. Disabling JSON output.",
        "-r",
        "--replace",
        dest="replace",
        help="Replace file when trying to normalize it instead of creating a new one.",
        "--force",
        dest="force",
        help="Replace file without asking if you are sure, use this flag with caution.",
        "--no-preemptive",
        dest="no_preemptive",
        help="Disable looking at a charset declaration to hint the detector.",
        "--threshold",
        default=0.2,
        dest="threshold",
        help="Define a custom maximum amount of noise allowed in decoded content. 0. <= noise <= 1.",
        version="Charset-Normalizer {} - Python {} - Unicode {} - SpeedUp {}".format(
            python_version(),
            unidata_version,
            "OFF" if md_module.__file__.lower().endswith(".py") else "ON",
        help="Show version information and exit.",
    if args.replace is True and args.normalize is False:
        if args.files:
            for my_file in args.files:
                my_file.close()
        print("Use --replace in addition of --normalize only.", file=sys.stderr)
    if args.force is True and args.replace is False:
        print("Use --force in addition of --replace only.", file=sys.stderr)
    if args.threshold < 0.0 or args.threshold > 1.0:
        print("--threshold VALUE should be between 0. AND 1.", file=sys.stderr)
    x_ = []
        matches = from_fp(
            my_file,
            threshold=args.threshold,
            explain=args.verbose,
            preemptive_behaviour=args.no_preemptive is False,
        best_guess = matches.best()
        if best_guess is None:
                'Unable to identify originating encoding for "{}". {}'.format(
                    my_file.name,
                        "Maybe try increasing maximum amount of chaos."
                        if args.threshold < 1.0
            x_.append(
                CliDetectionResult(
                    abspath(my_file.name),
                    1.0,
                    best_guess.encoding,
                    best_guess.encoding_aliases,
                        cp
                        for cp in best_guess.could_be_from_charset
                        if cp != best_guess.encoding
                    best_guess.language,
                    best_guess.alphabets,
                    best_guess.bom,
                    best_guess.percent_chaos,
                    best_guess.percent_coherence,
            if len(matches) > 1 and args.alternatives:
                for el in matches:
                    if el != best_guess:
                                el.encoding,
                                el.encoding_aliases,
                                    for cp in el.could_be_from_charset
                                    if cp != el.encoding
                                el.language,
                                el.alphabets,
                                el.bom,
                                el.percent_chaos,
                                el.percent_coherence,
            if args.normalize is True:
                if best_guess.encoding.startswith("utf") is True:
                        '"{}" file does not need to be normalized, as it already came from unicode.'.format(
                            my_file.name
                    if my_file.closed is False:
                dir_path = dirname(realpath(my_file.name))
                file_name = basename(realpath(my_file.name))
                o_: list[str] = file_name.split(".")
                if args.replace is False:
                    o_.insert(-1, best_guess.encoding)
                    args.force is False
                    and query_yes_no(
                        'Are you sure to normalize "{}" by replacing it ?'.format(
                        "no",
                    is False
                    x_[0].unicode_path = join(dir_path, ".".join(o_))
                    with open(x_[0].unicode_path, "wb") as fp:
                        fp.write(best_guess.output())
                    print(str(e), file=sys.stderr)
    if args.minimal is False:
            dumps(
                [el.__dict__ for el in x_] if len(x_) > 1 else x_[0].__dict__,
                ensure_ascii=True,
                indent=4,
                        el.encoding or "undefined"
                        for el in x_
                        if el.path == abspath(my_file.name)
from certifi import contents, where
"""Module allowing for ``python -m flake8 ...``."""
from flake8.main.cli import main
from rich import box
from rich.color import Color
from rich.console import Console, ConsoleOptions, Group, RenderableType, RenderResult
from rich.markdown import Markdown
from rich.measure import Measurement
from rich.pretty import Pretty
from rich.segment import Segment
from rich.syntax import Syntax
from rich.table import Table
    from rich.panel import Panel
        Panel(
            "Consider sponsoring to ensure this project is maintained.\n\n"
            "[cyan]https://github.com/sponsors/willmcgugan[/cyan]",
            border_style="green",
            padding=(1, 2),
The jsonschema CLI is now deprecated in favor of check-jsonschema.
from jsonschema.cli import main
displayMousePosition()from argparse import ArgumentParser, ArgumentTypeError, SUPPRESS
