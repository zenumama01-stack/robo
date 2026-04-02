PLATFORM = {
  'cygwin': 'win32',
  'msys': 'win32',
  'darwin': 'darwin',
  'linux': 'linux',
  'linux2': 'linux',
  'win32': 'win32',
  'win': 'win32',
}[sys.platform]
verbose_mode = False
def get_platform_key():
  if 'MAS_BUILD' in os.environ:
    return 'mas'
  return PLATFORM
def get_target_arch():
  arch = os.environ.get('TARGET_ARCH')
  if arch is None:
    return 'x64'
  if arch == 'x86':
    return 'ia32'
  return arch
def set_verbose_mode(mode):
  print('Running in verbose mode')
  global verbose_mode
  verbose_mode = mode
def is_verbose_mode():
  return verbose_mode
def verbose_mode_print(output):
  if verbose_mode:
    print(output)
def get_zip_name(name, version, suffix=''):
  arch = get_target_arch()
  if arch == 'arm':
    arch += 'v7l'
  zip_name = f'{name}-{version}-{get_platform_key()}-{arch}'
  if suffix:
    zip_name += '-' + suffix
  return zip_name + '.zip'
def get_tar_name(name, version, suffix=''):
  return zip_name + '.tar.xz'
import vcr
from tweepy.api import API
from tweepy.auth import OAuth1UserHandler
user_id = os.environ.get('TWITTER_USER_ID', '1072250532645998596')
username = os.environ.get('TWITTER_USERNAME', 'TweepyDev')
bearer_token = os.environ.get('BEARER_TOKEN', '')
consumer_key = os.environ.get('CONSUMER_KEY', '')
consumer_secret = os.environ.get('CONSUMER_SECRET', '')
access_token = os.environ.get('ACCESS_KEY', '')
access_token_secret = os.environ.get('ACCESS_SECRET', '')
use_replay = os.environ.get('USE_REPLAY', True)
tape = vcr.VCR(
    cassette_library_dir='cassettes',
    filter_headers=['Authorization'],
    # Either use existing cassettes, or never use recordings:
    record_mode='none' if use_replay else 'all',
class TweepyTestCase(unittest.TestCase):
        self.auth = create_auth()
        self.api = API(self.auth)
        self.api.retry_count = 2
        self.api.retry_delay = 0 if use_replay else 5
def create_auth():
    auth = OAuth1UserHandler(
    return auth
Module related to managing reading and writing to the config file.
Default config - spotdl.utils.config.DEFAULT_CONFIG
from argparse import Namespace
from typing import Any, Dict, Tuple, Union
from spotdl.types.options import (
    DownloaderOptions,
    SpotDLOptions,
    SpotifyOptions,
    WebOptions,
    "ConfigError",
    "get_spotdl_path",
    "get_config_file",
    "get_cache_path",
    "get_temp_path",
    "get_errors_path",
    "get_web_ui_path",
    "get_config",
    "create_settings_type",
    "create_settings",
    "SPOTIFY_OPTIONS",
    "DOWNLOADER_OPTIONS",
    "WEB_OPTIONS",
    "DEFAULT_CONFIG",
class ConfigError(Exception):
    Base class for all exceptions related to config.
def get_spotdl_path() -> Path:
    Get the path to the spotdl folder, following XDG standards on Linux.
    ~/.config/spotdl/ is used if it exists, else ~/.spotdl if it exists.
    If the spotdl directory does not exist, it will be created
    - The path to the spotdl folder.
    # For Linux systems, we follow the XDG Base Directory Specification
        # Define the new, correct XDG config path (~/.config/spotdl)
        xdg_config_path = Path.home() / ".config" / "spotdl"
        # Define the old path (~/.spotdl) for backward compatibility
        old_spotdl_path = Path.home() / ".spotdl"
        # Scenario 1: The user already has the new XDG config folder. Use it.
        if xdg_config_path.exists():
            return xdg_config_path
        # Scenario 2: The user is an existing user with only the old folder. Use the old one.
        if old_spotdl_path.exists():
            return old_spotdl_path
        # Scenario 3: The user is brand new. Create and use the new XDG path.
        os.makedirs(xdg_config_path, exist_ok=True)
    # For non-Linux systems (like Windows), use the default ~/.spotdl path
    spotdl_path = Path.home() / ".spotdl"
    os.makedirs(spotdl_path, exist_ok=True)
    return spotdl_path
def get_config_file() -> Path:
    Get config file path
    - The path to the config file.
    return get_spotdl_path() / "config.json"
def get_cache_path() -> Path:
    Get the path to the cache folder.
    - The path to the spotipy cache file.
    return get_spotdl_path() / ".spotipy"
def get_spotify_cache_path() -> Path:
    Get the path to the spotify cache folder.
    return get_spotdl_path() / ".spotify_cache"
def get_temp_path() -> Path:
    Get the path to the temp folder.
    - The path to the temp folder.
    temp_path = get_spotdl_path() / "temp"
    if not temp_path.exists():
        os.mkdir(temp_path)
    return temp_path
def get_errors_path() -> Path:
    Get the path to the errors folder.
    - The path to the errors folder.
    - If the errors directory does not exist, it will be created.
    errors_path = get_spotdl_path() / "errors"
    if not errors_path.exists():
        os.mkdir(errors_path)
    return errors_path
def get_web_ui_path() -> Path:
    Get the path to the web-ui folder.
    - The path to the web-ui folder.
    - If the web-ui directory does not exist, it will be created.
    web_ui_path = get_spotdl_path() / "web-ui"
    if not web_ui_path.exists():
        os.mkdir(web_ui_path)
    return web_ui_path
def get_config() -> Dict[str, Any]:
    Get the config.
    - The dictionary with the config.
    ### Errors
    - ConfigError: If the config file does not exist.
    config_path = get_config_file()
    if not config_path.exists():
        raise ConfigError(
            "Config file not found."
            "Please run `spotdl --generate-config` to create a config file."
    with open(config_path, "r", encoding="utf-8") as config_file:
        return json.load(config_file)
def create_settings_type(
    arguments: Namespace,
    config: Dict[str, Any],
    default: Union[SpotifyOptions, DownloaderOptions, WebOptions],
) -> Dict[str, Any]:
    Create settings dict
    Argument value has always the priority, then the config file
    value, and if neither are set, use default value
    - arguments: Namespace from argparse
    - default: dict
    - settings: dict
    for key, default_value in default.items():
        argument_val = arguments.__dict__.get(key)
        config_val = config.get(key)
        if argument_val is not None:
            settings[key] = argument_val
        elif config_val is not None:
            settings[key] = config_val
            settings[key] = default_value
    return settings
def create_settings(
) -> Tuple[SpotifyOptions, DownloaderOptions, WebOptions]:
    Create settings dicts for Spotify, Downloader and Web
    based on the arguments and config file (if enabled)
    - spotify_options: SpotifyOptions
    - downloader_options: DownloaderOptions
    - web_options: WebOptions
    # Get the config file
    # It will automatically load if the `load_config` is set to True
    # in the config file
    config = {}
    if arguments.config or (
        get_config_file().exists() and get_config().get("load_config")
        config = get_config()
    # Type: ignore because of the issues below
    # https://github.com/python/mypy/issues/8890
    # https://github.com/python/mypy/issues/5382
    spotify_options = SpotifyOptions(
        **create_settings_type(arguments, config, SPOTIFY_OPTIONS)  # type: ignore
    downloader_options = DownloaderOptions(
        **create_settings_type(arguments, config, DOWNLOADER_OPTIONS)  # type: ignore
    web_options = WebOptions(**create_settings_type(arguments, config, WEB_OPTIONS))  # type: ignore
    return spotify_options, downloader_options, web_options
def modernize_settings(options: DownloaderOptions):
    """Handle deprecated values in config file.
    - options: DownloaderOptions to modernize
    warning_msg = "Deprecated '%s' value found for '%s' setting in config file. Using '%s' instead."
    # Respect backward compatibility with old boolean --restrict flag
    if options["restrict"] is True:
        logger.warning(warning_msg, True, "restrict", "strict")
        options["restrict"] = "strict"
class GlobalConfig:
    Class to store global configuration
    parameters: Dict[str, Any] = {}
    def set_parameter(cls, key, value):
        Set a parameter for the download config
        cls.parameters[key] = value
    def get_parameter(cls, key):
        Get a parameter from the download config
        return cls.parameters.get(key, None)
SPOTIFY_OPTIONS: SpotifyOptions = {
    "client_id": "5f573c9620494bae87890c0f08a60293",
    "client_secret": "212476d9b0f3472eaa762d90b19b0ba8",
    "auth_token": None,
    "user_auth": False,
    "headless": False,
    "cache_path": str(get_cache_path()),
    "no_cache": False,
    "max_retries": 3,
    "use_cache_file": False,
DOWNLOADER_OPTIONS: DownloaderOptions = {
    "audio_providers": ["youtube-music"],
    "lyrics_providers": ["genius", "azlyrics", "musixmatch"],
    "genius_token": "alXXDbPZtK1m2RrZ8I4k2Hn8Ahsd0Gh_o076HYvcdlBvmc0ULL1H8Z8xRlew5qaG",
    "playlist_numbering": False,
    "playlist_retain_track_cover": False,
    "scan_for_songs": False,
    "m3u": None,
    "output": "{artists} - {title}.{output-ext}",
    "overwrite": "skip",
    "search_query": None,
    "ffmpeg": "ffmpeg",
    "bitrate": "128k",
    "ffmpeg_args": None,
    "format": "mp3",
    "save_file": None,
    "filter_results": True,
    "album_type": None,
    "threads": 4,
    "cookie_file": None,
    "restrict": None,
    "print_errors": False,
    "sponsor_block": False,
    "preload": False,
    "archive": None,
    "load_config": True,
    "log_level": "INFO",
    "simple_tui": False,
    "fetch_albums": False,
    "id3_separator": "/",
    "ytm_data": False,
    "add_unavailable": False,
    "generate_lrc": False,
    "force_update_metadata": False,
    "only_verified_results": False,
    "sync_without_deleting": False,
    "max_filename_length": None,
    "yt_dlp_args": None,
    "detect_formats": None,
    "save_errors": None,
    "ignore_albums": None,
    "proxy": None,
    "skip_explicit": False,
    "log_format": None,
    "redownload": False,
    "skip_album_art": False,
    "create_skip_file": False,
    "respect_skip_file": False,
    "sync_remove_lrc": False,
WEB_OPTIONS: WebOptions = {
    "web_use_output_dir": False,
    "port": 8800,
    "host": "localhost",
    "keep_alive": False,
    "enable_tls": False,
    "key_file": None,
    "cert_file": None,
    "ca_file": None,
    "allowed_origins": None,
    "keep_sessions": False,
    "force_update_gui": False,
    "web_gui_repo": None,
    "web_gui_location": None,
# Type: ignore because of the issues above
DEFAULT_CONFIG: SpotDLOptions = {
    **SPOTIFY_OPTIONS,  # type: ignore
    **DOWNLOADER_OPTIONS,  # type: ignore
    **WEB_OPTIONS,  # type: ignore
This module holds run-time PyInstaller configuration.
Variable CONF is a dict() with all configuration options that are necessary for the build phase. Build phase is done by
passing .spec file to exec() function. CONF variable is the only way how to pass arguments to exec() and how to avoid
using 'global' variables.
NOTE: Having 'global' variables does not play well with the test suite because it does not provide isolated environments
for tests. Some tests might fail in this case.
NOTE: The 'CONF' dict() is cleaned after building phase to not interfere with any other possible test.
To pass any arguments to build phase, just do:
    from PyInstaller.config import CONF
    CONF['my_var_name'] = my_value
And to use this variable in the build phase:
    foo = CONF['my_var_name']
This is the list of known variables. (Please update it if necessary.)
cachedir
hiddenimports
noconfirm
pathex
ui_admin
ui_access
upx_available
upx_dir
workpath
tests_modgraph  - cached PyiModuleGraph object to speed up tests
code_cache - dictionary associating `Analysis.pure` list instances with code cache dictionaries. Used by PYZ writer.
# NOTE: Do not import other PyInstaller modules here. Just define constants here.
CONF = {
    # Unit tests require this key to exist.
    'pathex': [],
from pydantic_settings import BaseSettings
class Settings(BaseSettings):
    app_name: str = "Awesome API"
    admin_email: str
    items_per_user: int = 50
settings = Settings()
from pydantic_settings import BaseSettings, SettingsConfigDict
    model_config = SettingsConfigDict(env_file=".env")
from importlib import import_module
from django.core.exceptions import ImproperlyConfigured
from django.utils.functional import cached_property
from django.utils.module_loading import import_string, module_has_submodule
APPS_MODULE_NAME = "apps"
MODELS_MODULE_NAME = "models"
class AppConfig:
    """Class representing a Django application and its configuration."""
    def __init__(self, app_name, app_module):
        # Full Python path to the application e.g. 'django.contrib.admin'.
        self.name = app_name
        # Root module for the application e.g. <module 'django.contrib.admin'
        # from 'django/contrib/admin/__init__.py'>.
        self.module = app_module
        # Reference to the Apps registry that holds this AppConfig. Set by the
        # registry when it registers the AppConfig instance.
        self.apps = None
        # The following attributes could be defined at the class level in a
        # subclass, hence the test-and-set pattern.
        # Last component of the Python path to the application e.g. 'admin'.
        # This value must be unique across a Django project.
        if not hasattr(self, "label"):
            self.label = app_name.rpartition(".")[2]
        if not self.label.isidentifier():
            raise ImproperlyConfigured(
                "The app label '%s' is not a valid Python identifier." % self.label
        # Human-readable name for the application e.g. "Admin".
        if not hasattr(self, "verbose_name"):
            self.verbose_name = self.label.title()
        # Filesystem path to the application directory e.g.
        # '/path/to/django/contrib/admin'.
        if not hasattr(self, "path"):
            self.path = self._path_from_module(app_module)
        # Module containing models e.g. <module 'django.contrib.admin.models'
        # from 'django/contrib/admin/models.py'>. Set by import_models().
        # None if the application doesn't have a models module.
        self.models_module = None
        # Mapping of lowercase model names to model classes. Initially set to
        # None to prevent accidental access before import_models() runs.
        self.models = None
        return "<%s: %s>" % (self.__class__.__name__, self.label)
    def default_auto_field(self):
        return settings.DEFAULT_AUTO_FIELD
    def _is_default_auto_field_overridden(self):
        return self.__class__.default_auto_field is not AppConfig.default_auto_field
    def _path_from_module(self, module):
        """Attempt to determine app's filesystem path from its module."""
        # See #21874 for extended discussion of the behavior of this method in
        # various cases.
        # Convert to list because __path__ may not support indexing.
        paths = list(getattr(module, "__path__", []))
        if len(paths) != 1:
            filename = getattr(module, "__file__", None)
                paths = [os.path.dirname(filename)]
                # For unknown reasons, sometimes the list returned by __path__
                # contains duplicates that must be removed (#25246).
                paths = list(set(paths))
        if len(paths) > 1:
                "The app module %r has multiple filesystem locations (%r); "
                "you must configure this app with an AppConfig subclass "
                "with a 'path' class attribute." % (module, paths)
        elif not paths:
                "The app module %r has no filesystem location, "
                "with a 'path' class attribute." % module
        return paths[0]
    def create(cls, entry):
        Factory that creates an app config from an entry in INSTALLED_APPS.
        # create() eventually returns app_config_class(app_name, app_module).
        app_config_class = None
        app_name = None
        app_module = None
        # If import_module succeeds, entry points to the app module.
            app_module = import_module(entry)
            # If app_module has an apps submodule that defines a single
            # AppConfig subclass, use it automatically.
            # To prevent this, an AppConfig subclass can declare a class
            # variable default = False.
            # If the apps module defines more than one AppConfig subclass,
            # the default one can declare default = True.
            if module_has_submodule(app_module, APPS_MODULE_NAME):
                mod_path = "%s.%s" % (entry, APPS_MODULE_NAME)
                mod = import_module(mod_path)
                # Check if there's exactly one AppConfig candidate,
                # excluding those that explicitly define default = False.
                app_configs = [
                    (name, candidate)
                    for name, candidate in inspect.getmembers(mod, inspect.isclass)
                        issubclass(candidate, cls)
                        and candidate is not cls
                        and getattr(candidate, "default", True)
                if len(app_configs) == 1:
                    app_config_class = app_configs[0][1]
                    # Check if there's exactly one AppConfig subclass,
                    # among those that explicitly define default = True.
                        for name, candidate in app_configs
                        if getattr(candidate, "default", False)
                    if len(app_configs) > 1:
                        candidates = [repr(name) for name, _ in app_configs]
                            "%r declares more than one default AppConfig: "
                            "%s." % (mod_path, ", ".join(candidates))
                    elif len(app_configs) == 1:
            # Use the default app config class if we didn't find anything.
            if app_config_class is None:
                app_config_class = cls
                app_name = entry
        # If import_string succeeds, entry is an app config class.
                app_config_class = import_string(entry)
        # If both import_module and import_string failed, it means that entry
        # doesn't have a valid value.
        if app_module is None and app_config_class is None:
            # If the last component of entry starts with an uppercase letter,
            # then it was likely intended to be an app config class; if not,
            # an app module. Provide a nice error message in both cases.
            mod_path, _, cls_name = entry.rpartition(".")
            if mod_path and cls_name[0].isupper():
                # We could simply re-trigger the string import exception, but
                # we're going the extra mile and providing a better error
                # message for typos in INSTALLED_APPS.
                # This may raise ImportError, which is the best exception
                # possible if the module at mod_path cannot be imported.
                candidates = [
                    repr(name)
                    if issubclass(candidate, cls) and candidate is not cls
                msg = "Module '%s' does not contain a '%s' class." % (
                    mod_path,
                    cls_name,
                if candidates:
                    msg += " Choices are: %s." % ", ".join(candidates)
                raise ImportError(msg)
                # Re-trigger the module import exception.
                import_module(entry)
        # Check for obvious errors. (This check prevents duck typing, but
        # it could be removed if it became a problem in practice.)
        if not issubclass(app_config_class, AppConfig):
            raise ImproperlyConfigured("'%s' isn't a subclass of AppConfig." % entry)
        # Obtain app name here rather than in AppClass.__init__ to keep
        # all error checking for entries in INSTALLED_APPS in one place.
        if app_name is None:
                app_name = app_config_class.name
                raise ImproperlyConfigured("'%s' must supply a name attribute." % entry)
        # Ensure app_name points to a valid module.
            app_module = import_module(app_name)
                "Cannot import '%s'. Check that '%s.%s.name' is correct."
                    app_name,
                    app_config_class.__module__,
                    app_config_class.__qualname__,
        # Entry is a path to an app config class.
        return app_config_class(app_name, app_module)
    def get_model(self, model_name, require_ready=True):
        Return the model with the given case-insensitive model_name.
        Raise LookupError if no model exists with this name.
        if require_ready:
            self.apps.check_models_ready()
            self.apps.check_apps_ready()
            return self.models[model_name.lower()]
            raise LookupError(
                "App '%s' doesn't have a '%s' model." % (self.label, model_name)
    def get_models(self, include_auto_created=False, include_swapped=False):
        Return an iterable of models.
        By default, the following models aren't included:
        - auto-created models for many-to-many relations without
          an explicit intermediate table,
        - models that have been swapped out.
        Set the corresponding keyword argument to True to include such models.
        Keyword arguments aren't documented; they're a private API.
        for model in self.models.values():
            if model._meta.auto_created and not include_auto_created:
            if model._meta.swapped and not include_swapped:
            yield model
    def import_models(self):
        # Dictionary of models for this app, primarily maintained in the
        # 'all_models' attribute of the Apps this AppConfig is attached to.
        self.models = self.apps.all_models[self.label]
        if module_has_submodule(self.module, MODELS_MODULE_NAME):
            models_module_name = "%s.%s" % (self.name, MODELS_MODULE_NAME)
            self.models_module = import_module(models_module_name)
    def ready(self):
        Override this method in subclasses to run code when Django starts.
"""Configuration utilities for `Runnable` objects."""
# Cannot move uuid to TYPE_CHECKING as RunnableConfig is used in Pydantic models
import uuid  # noqa: TC003
from collections.abc import Awaitable, Callable, Generator, Iterable, Iterator, Sequence
from concurrent.futures import Executor, Future, ThreadPoolExecutor
from contextvars import Context, ContextVar, Token, copy_context
    from langchain_core.callbacks.base import BaseCallbackManager, Callbacks
    # Pydantic validates through typed dicts, but
    # the callbacks need forward refs updated
    Callbacks = list | Any | None
class EmptyDict(TypedDict, total=False):
    """Empty dict type."""
class RunnableConfig(TypedDict, total=False):
    """Configuration for a `Runnable`.
    !!! note Custom values
        The `TypedDict` has `total=False` set intentionally to:
        - Allow partial configs to be created and merged together via `merge_configs`
        - Support config propagation from parent to child runnables via
            `var_child_runnable_config` (a `ContextVar` that automatically passes
            config down the call stack without explicit parameter passing), where
            configs are merged rather than replaced
            # Parent sets tags
            chain.invoke(input, config={"tags": ["parent"]})
            # Child automatically inherits and can add:
            # ensure_config({"tags": ["child"]}) -> {"tags": ["parent", "child"]}
    tags: list[str]
    """Tags for this call and any sub-calls (e.g. a Chain calling an LLM).
    You can use these to filter calls.
    metadata: dict[str, Any]
    """Metadata for this call and any sub-calls (e.g. a Chain calling an LLM).
    Keys should be strings, values should be JSON-serializable.
    callbacks: Callbacks
    """Callbacks for this call and any sub-calls (e.g. a Chain calling an LLM).
    Tags are passed to all callbacks, metadata is passed to handle*Start callbacks.
    run_name: str
    """Name for the tracer run for this call.
    Defaults to the name of the class."""
    max_concurrency: int | None
    """Maximum number of parallel calls to make.
    If not provided, defaults to `ThreadPoolExecutor`'s default.
    recursion_limit: int
    """Maximum number of times a call can recurse.
    If not provided, defaults to `25`.
    configurable: dict[str, Any]
    """Runtime values for attributes previously made configurable on this `Runnable`,
    or sub-`Runnable` objects, through `configurable_fields` or
    `configurable_alternatives`.
    Check `output_schema` for a description of the attributes that have been made
    configurable.
    run_id: uuid.UUID | None
    """Unique identifier for the tracer run for this call.
    If not provided, a new UUID will be generated.
CONFIG_KEYS = [
    "tags",
    "metadata",
    "callbacks",
    "run_name",
    "max_concurrency",
    "recursion_limit",
    "configurable",
    "run_id",
COPIABLE_KEYS = [
DEFAULT_RECURSION_LIMIT = 25
var_child_runnable_config: ContextVar[RunnableConfig | None] = ContextVar(
    "child_runnable_config", default=None
# This is imported and used in langgraph, so don't break.
def _set_config_context(
) -> tuple[Token[RunnableConfig | None], dict[str, Any] | None]:
    """Set the child Runnable config + tracing context.
        config: The config to set.
        The token to reset the config and the previous tracing context.
    # Deferred to avoid importing langsmith at module level (~132ms).
    from langsmith.run_helpers import (  # noqa: PLC0415
        _set_tracing_context,
        get_tracing_context,
    from langchain_core.tracers.langchain import LangChainTracer  # noqa: PLC0415
    config_token = var_child_runnable_config.set(config)
    current_context = None
        (callbacks := config.get("callbacks"))
            parent_run_id := getattr(callbacks, "parent_run_id", None)
        )  # Is callback manager
            tracer := next(
                    handler
                    for handler in getattr(callbacks, "handlers", [])
                    if isinstance(handler, LangChainTracer)
        and (run := tracer.run_map.get(str(parent_run_id)))
        current_context = get_tracing_context()
        _set_tracing_context({"parent": run})
    return config_token, current_context
def set_config_context(config: RunnableConfig) -> Generator[Context, None, None]:
        The config context.
    from langsmith.run_helpers import _set_tracing_context  # noqa: PLC0415
    ctx = copy_context()
    config_token, _ = ctx.run(_set_config_context, config)
        yield ctx
        ctx.run(var_child_runnable_config.reset, config_token)
        ctx.run(
                "parent": None,
                "project_name": None,
                "tags": None,
                "metadata": None,
                "enabled": None,
                "client": None,
def ensure_config(config: RunnableConfig | None = None) -> RunnableConfig:
    """Ensure that a config is a dict with all keys present.
        config: The config to ensure.
        The ensured config.
    empty = RunnableConfig(
        tags=[],
        metadata={},
        callbacks=None,
        recursion_limit=DEFAULT_RECURSION_LIMIT,
        configurable={},
    if var_config := var_child_runnable_config.get():
        empty.update(
                    k: v.copy() if k in COPIABLE_KEYS else v  # type: ignore[attr-defined]
                    for k, v in var_config.items()
                    if v is not None
    if config is not None:
                    for k, v in config.items()
                    if v is not None and k in CONFIG_KEYS
        for k, v in config.items():
            if k not in CONFIG_KEYS and v is not None:
                empty["configurable"][k] = v
    for key, value in empty.get("configurable", {}).items():
            not key.startswith("__")
            and isinstance(value, (str, int, float, bool))
            and key not in empty["metadata"]
            and key != "api_key"
            empty["metadata"][key] = value
    return empty
def get_config_list(
    config: RunnableConfig | Sequence[RunnableConfig] | None, length: int
) -> list[RunnableConfig]:
    """Get a list of configs from a single config or a list of configs.
     It is useful for subclasses overriding batch() or abatch().
        config: The config or list of configs.
        length: The length of the list.
        The list of configs.
        ValueError: If the length of the list is not equal to the length of the inputs.
    if length < 0:
        msg = f"length must be >= 0, but got {length}"
    if isinstance(config, Sequence) and len(config) != length:
            f"config must be a list of the same length as inputs, "
            f"but got {len(config)} configs for {length} inputs"
        return list(map(ensure_config, config))
    if length > 1 and isinstance(config, dict) and config.get("run_id") is not None:
            "Provided run_id be used only for the first element of the batch.",
            category=RuntimeWarning,
        subsequent = cast(
            "RunnableConfig", {k: v for k, v in config.items() if k != "run_id"}
            ensure_config(subsequent) if i else ensure_config(config)
            for i in range(length)
    return [ensure_config(config) for i in range(length)]
def patch_config(
    callbacks: BaseCallbackManager | None = None,
    recursion_limit: int | None = None,
    max_concurrency: int | None = None,
    run_name: str | None = None,
    configurable: dict[str, Any] | None = None,
) -> RunnableConfig:
    """Patch a config with new values.
        config: The config to patch.
        callbacks: The callbacks to set.
        recursion_limit: The recursion limit to set.
        max_concurrency: The max concurrency to set.
        run_name: The run name to set.
        configurable: The configurable to set.
        The patched config.
    if callbacks is not None:
        # If we're replacing callbacks, we need to unset run_name
        # As that should apply only to the same run as the original callbacks
        config["callbacks"] = callbacks
        if "run_name" in config:
            del config["run_name"]
        if "run_id" in config:
            del config["run_id"]
    if recursion_limit is not None:
        config["recursion_limit"] = recursion_limit
    if max_concurrency is not None:
        config["max_concurrency"] = max_concurrency
    if run_name is not None:
        config["run_name"] = run_name
    if configurable is not None:
        config["configurable"] = {**config.get("configurable", {}), **configurable}
    return config
def merge_configs(*configs: RunnableConfig | None) -> RunnableConfig:
    """Merge multiple configs into one.
        *configs: The configs to merge.
        The merged config.
    base: RunnableConfig = {}
    # Even though the keys aren't literals, this is correct
    # because both dicts are the same type
    for config in (ensure_config(c) for c in configs if c is not None):
        for key in config:
            if key == "metadata":
                base["metadata"] = {
                    **base.get("metadata", {}),
                    **(config.get("metadata") or {}),
            elif key == "tags":
                base["tags"] = sorted(
                    set(base.get("tags", []) + (config.get("tags") or [])),
            elif key == "configurable":
                base["configurable"] = {
                    **base.get("configurable", {}),
                    **(config.get("configurable") or {}),
            elif key == "callbacks":
                base_callbacks = base.get("callbacks")
                these_callbacks = config["callbacks"]
                # callbacks can be either None, list[handler] or manager
                # so merging two callbacks values has 6 cases
                if isinstance(these_callbacks, list):
                    if base_callbacks is None:
                        base["callbacks"] = these_callbacks.copy()
                    elif isinstance(base_callbacks, list):
                        base["callbacks"] = base_callbacks + these_callbacks
                        # base_callbacks is a manager
                        mngr = base_callbacks.copy()
                        for callback in these_callbacks:
                            mngr.add_handler(callback, inherit=True)
                        base["callbacks"] = mngr
                elif these_callbacks is not None:
                    # these_callbacks is a manager
                        mngr = these_callbacks.copy()
                        for callback in base_callbacks:
                        # base_callbacks is also a manager
                        base["callbacks"] = base_callbacks.merge(these_callbacks)
            elif key == "recursion_limit":
                if config["recursion_limit"] != DEFAULT_RECURSION_LIMIT:
                    base["recursion_limit"] = config["recursion_limit"]
            elif key in COPIABLE_KEYS and config[key] is not None:  # type: ignore[literal-required]
                base[key] = config[key].copy()  # type: ignore[literal-required]
                base[key] = config[key] or base.get(key)  # type: ignore[literal-required]
    return base
def call_func_with_variable_args(
    run_manager: CallbackManagerForChainRun | None = None,
    """Call function that may optionally accept a run_manager and/or config.
        func: The function to call.
        input: The input to the function.
        config: The config to pass to the function.
        run_manager: The run manager to pass to the function.
        **kwargs: The keyword arguments to pass to the function.
        The output of the function.
        if run_manager is not None:
            kwargs["config"] = patch_config(config, callbacks=run_manager.get_child())
            kwargs["config"] = config
    if run_manager is not None and accepts_run_manager(func):
    return func(input, **kwargs)  # type: ignore[call-arg]
def acall_func_with_variable_args(
    run_manager: AsyncCallbackManagerForChainRun | None = None,
) -> Awaitable[Output]:
    """Async call function that may optionally accept a run_manager and/or config.
def get_callback_manager_for_config(config: RunnableConfig) -> CallbackManager:
    """Get a callback manager for a config.
        config: The config.
        The callback manager.
    return CallbackManager.configure(
def get_async_callback_manager_for_config(
) -> AsyncCallbackManager:
    """Get an async callback manager for a config.
        The async callback manager.
    return AsyncCallbackManager.configure(
class ContextThreadPoolExecutor(ThreadPoolExecutor):
    """ThreadPoolExecutor that copies the context to the child thread."""
    def submit(  # type: ignore[override]
        func: Callable[P, T],
    ) -> Future[T]:
        """Submit a function to the executor.
            func: The function to submit.
            *args: The positional arguments to the function.
            **kwargs: The keyword arguments to the function.
            The future for the function.
        return super().submit(
            cast("Callable[..., T]", partial(copy_context().run, func, *args, **kwargs))
    def map(
        fn: Callable[..., T],
        *iterables: Iterable[Any],
        """Map a function to multiple iterables.
            fn: The function to map.
            *iterables: The iterables to map over.
            timeout: The timeout for the map.
            chunksize: The chunksize for the map.
            The iterator for the mapped function.
        contexts = [copy_context() for _ in range(len(iterables[0]))]  # type: ignore[arg-type]
        def _wrapped_fn(*args: Any) -> T:
            return contexts.pop().run(fn, *args)
        return super().map(
            _wrapped_fn,
            *iterables,
def get_executor_for_config(
) -> Generator[Executor, None, None]:
    """Get an executor for a config.
        The executor.
    config = config or {}
    with ContextThreadPoolExecutor(
        max_workers=config.get("max_concurrency")
        yield executor
async def run_in_executor(
    executor_or_config: Executor | RunnableConfig | None,
    """Run a function in an executor.
        executor_or_config: The executor or config to run in.
        func: The function.
    def wrapper() -> T:
        except StopIteration as exc:
            # StopIteration can't be set on an asyncio.Future
            # it raises a TypeError and leaves the Future pending forever
            # so we need to convert it to a RuntimeError
            raise RuntimeError from exc
    if executor_or_config is None or isinstance(executor_or_config, dict):
        # Use default executor with context copied from current context
        return await asyncio.get_running_loop().run_in_executor(
            cast("Callable[..., T]", partial(copy_context().run, wrapper)),
    return await asyncio.get_running_loop().run_in_executor(executor_or_config, wrapper)
    EmptyDict,
    "EmptyDict",
    "acall_func_with_variable_args",
    "call_func_with_variable_args",
    "get_async_callback_manager_for_config",
    "get_callback_manager_for_config",
    "get_executor_for_config",
    "merge_configs",
"""Configuration for run evaluators."""
from langsmith import RunEvaluator
from langsmith.evaluation.evaluator import EvaluationResult, EvaluationResults
from langsmith.schemas import Example, Run
from langchain_classic.evaluation.criteria.eval_chain import CRITERIA_TYPE
    EmbeddingDistance as EmbeddingDistanceEnum,
from langchain_classic.evaluation.schema import EvaluatorType, StringEvaluator
    StringDistance as StringDistanceEnum,
RUN_EVALUATOR_LIKE = Callable[
    [Run, Example | None],
    EvaluationResult | EvaluationResults | dict,
BATCH_EVALUATOR_LIKE = Callable[
    [Sequence[Run], Sequence[Example] | None],
class EvalConfig(BaseModel):
    """Configuration for a given run evaluator.
        evaluator_type: The type of evaluator to use.
    evaluator_type: EvaluatorType
    def get_kwargs(self) -> dict[str, Any]:
        """Get the keyword arguments for the `load_evaluator` call.
            The keyword arguments for the `load_evaluator` call.
        for field, val in self:
            if field == "evaluator_type" or val is None:
            kwargs[field] = val
class SingleKeyEvalConfig(EvalConfig):
    """Configuration for a run evaluator that only requires a single key."""
    reference_key: str | None = None
    """The key in the dataset run to use as the reference string.
    If not provided, we will attempt to infer automatically."""
    prediction_key: str | None = None
    """The key from the traced run's outputs dictionary to use to
    represent the prediction. If not provided, it will be inferred
    automatically."""
    input_key: str | None = None
    """The key from the traced run's inputs dictionary to use to represent the
    input. If not provided, it will be inferred automatically."""
        kwargs = super().get_kwargs()
        # Filer out the keys that are not needed for the evaluator.
        for key in ["reference_key", "prediction_key", "input_key"]:
            kwargs.pop(key, None)
CUSTOM_EVALUATOR_TYPE = RUN_EVALUATOR_LIKE | RunEvaluator | StringEvaluator
SINGLE_EVAL_CONFIG_TYPE = EvaluatorType | str | EvalConfig
class RunEvalConfig(BaseModel):
    """Configuration for a run evaluation."""
    evaluators: list[SINGLE_EVAL_CONFIG_TYPE | CUSTOM_EVALUATOR_TYPE] = Field(
    """Configurations for which evaluators to apply to the dataset run.
    Each can be the string of an
    `EvaluatorType <langchain.evaluation.schema.EvaluatorType>`, such
    as `EvaluatorType.QA`, the evaluator type string ("qa"), or a configuration for a
    given evaluator
    (e.g.,
    `RunEvalConfig.QA <langchain.smith.evaluation.config.RunEvalConfig.QA>`)."""
    custom_evaluators: list[CUSTOM_EVALUATOR_TYPE] | None = None
    """Custom evaluators to apply to the dataset run."""
    batch_evaluators: list[BATCH_EVALUATOR_LIKE] | None = None
    """Evaluators that run on an aggregate/batch level.
    These generate one or more metrics that are assigned to the full test run.
    As a result, they are not associated with individual traces.
    eval_llm: BaseLanguageModel | None = None
    """The language model to pass to any evaluators that require one."""
    class Criteria(SingleKeyEvalConfig):
        """Configuration for a reference-free criteria evaluator.
            criteria: The criteria to evaluate.
            llm: The language model to use for the evaluation chain.
        criteria: CRITERIA_TYPE | None = None
        evaluator_type: EvaluatorType = EvaluatorType.CRITERIA
    class LabeledCriteria(SingleKeyEvalConfig):
        """Configuration for a labeled (with references) criteria evaluator.
        evaluator_type: EvaluatorType = EvaluatorType.LABELED_CRITERIA
    class EmbeddingDistance(SingleKeyEvalConfig):
        """Configuration for an embedding distance evaluator.
            embeddings: The embeddings to use for computing the distance.
            distance_metric: The distance metric to use for computing the distance.
        evaluator_type: EvaluatorType = EvaluatorType.EMBEDDING_DISTANCE
        embeddings: Embeddings | None = None
        distance_metric: EmbeddingDistanceEnum | None = None
    class StringDistance(SingleKeyEvalConfig):
        """Configuration for a string distance evaluator.
            distance: The string distance metric to use (`damerau_levenshtein`,
                `levenshtein`, `jaro`, or `jaro_winkler`).
            normalize_score: Whether to normalize the distance to between 0 and 1.
                Applies only to the Levenshtein and Damerau-Levenshtein distances.
        evaluator_type: EvaluatorType = EvaluatorType.STRING_DISTANCE
        distance: StringDistanceEnum | None = None
        normalize_score: bool = True
    class QA(SingleKeyEvalConfig):
        """Configuration for a QA evaluator.
            prompt: The prompt template to use for generating the question.
        evaluator_type: EvaluatorType = EvaluatorType.QA
        prompt: BasePromptTemplate | None = None
    class ContextQA(SingleKeyEvalConfig):
        """Configuration for a context-based QA evaluator.
        evaluator_type: EvaluatorType = EvaluatorType.CONTEXT_QA
    class CoTQA(SingleKeyEvalConfig):
    class JsonValidity(SingleKeyEvalConfig):
        """Configuration for a json validity evaluator."""
        evaluator_type: EvaluatorType = EvaluatorType.JSON_VALIDITY
    class JsonEqualityEvaluator(EvalConfig):
        """Configuration for a json equality evaluator."""
        evaluator_type: EvaluatorType = EvaluatorType.JSON_EQUALITY
    class ExactMatch(SingleKeyEvalConfig):
        """Configuration for an exact match string evaluator.
        evaluator_type: EvaluatorType = EvaluatorType.EXACT_MATCH
        ignore_case: bool = False
        ignore_punctuation: bool = False
        ignore_numbers: bool = False
    class RegexMatch(SingleKeyEvalConfig):
        """Configuration for a regex match string evaluator.
            flags: The flags to pass to the regex. Example: `re.IGNORECASE`.
        evaluator_type: EvaluatorType = EvaluatorType.REGEX_MATCH
        flags: int = 0
    class ScoreString(SingleKeyEvalConfig):
        """Configuration for a score string evaluator.
        This is like the criteria evaluator but it is configured by
        default to return a score on the scale from 1-10.
        It is recommended to normalize these scores
        by setting `normalize_by` to 10.
            normalize_by: If you want to normalize the score, the denominator to use.
                If not provided, the score will be between 1 and 10.
            prompt: The prompt template to use for evaluation.
        evaluator_type: EvaluatorType = EvaluatorType.SCORE_STRING
        normalize_by: float | None = None
    class LabeledScoreString(ScoreString):
        """Configuration for a labeled score string evaluator."""
        evaluator_type: EvaluatorType = EvaluatorType.LABELED_SCORE_STRING
"""distutils.pypirc
Provides the PyPIRCCommand class, the base class for the command classes
that uses .pypirc in the distutils.command package.
from configparser import RawConfigParser
from distutils.cmd import Command
DEFAULT_PYPIRC = """\
[distutils]
index-servers =
    pypi
[pypi]
username:%s
password:%s
class PyPIRCCommand(Command):
    """Base command that knows how to handle the .pypirc file"""
    DEFAULT_REPOSITORY = 'https://upload.pypi.org/legacy/'
    DEFAULT_REALM = 'pypi'
    repository = None
    realm = None
    user_options = [
        ('repository=', 'r', "url of repository [default: %s]" % DEFAULT_REPOSITORY),
        ('show-response', None, 'display full response text from server'),
    boolean_options = ['show-response']
    def _get_rc_file(self):
        """Returns rc file path."""
        return os.path.join(os.path.expanduser('~'), '.pypirc')
    def _store_pypirc(self, username, password):
        """Creates a default .pypirc file."""
        rc = self._get_rc_file()
        with os.fdopen(os.open(rc, os.O_CREAT | os.O_WRONLY, 0o600), 'w') as f:
            f.write(DEFAULT_PYPIRC % (username, password))
    def _read_pypirc(self):
        """Reads the .pypirc file."""
        if os.path.exists(rc):
            self.announce('Using PyPI login from %s' % rc)
            repository = self.repository or self.DEFAULT_REPOSITORY
            config = RawConfigParser()
            config.read(rc)
            sections = config.sections()
            if 'distutils' in sections:
                # let's get the list of servers
                index_servers = config.get('distutils', 'index-servers')
                _servers = [
                    server.strip()
                    for server in index_servers.split('\n')
                    if server.strip() != ''
                if _servers == []:
                    # nothing set, let's try to get the default pypi
                    if 'pypi' in sections:
                        _servers = ['pypi']
                        # the file is not properly defined, returning
                        # an empty dict
                for server in _servers:
                    current = {'server': server}
                    current['username'] = config.get(server, 'username')
                    # optional params
                    for key, default in (
                        ('repository', self.DEFAULT_REPOSITORY),
                        ('realm', self.DEFAULT_REALM),
                        ('password', None),
                        if config.has_option(server, key):
                            current[key] = config.get(server, key)
                            current[key] = default
                    # work around people having "repository" for the "pypi"
                    # section of their config set to the HTTP (rather than
                    # HTTPS) URL
                    if server == 'pypi' and repository in (
                        self.DEFAULT_REPOSITORY,
                        'pypi',
                        current['repository'] = self.DEFAULT_REPOSITORY
                        current['server'] == repository
                        or current['repository'] == repository
            elif 'server-login' in sections:
                # old format
                server = 'server-login'
                if config.has_option(server, 'repository'):
                    repository = config.get(server, 'repository')
                    repository = self.DEFAULT_REPOSITORY
                    'username': config.get(server, 'username'),
                    'password': config.get(server, 'password'),
                    'repository': repository,
                    'server': server,
                    'realm': self.DEFAULT_REALM,
    def _read_pypi_response(self, response):
        """Read and decode a PyPI HTTP response."""
        import cgi
        content_type = response.getheader('content-type', 'text/plain')
        encoding = cgi.parse_header(content_type)[1].get('charset', 'ascii')
        return response.read().decode(encoding)
    def initialize_options(self):
        """Initialize options."""
        self.repository = None
        self.realm = None
        self.show_response = 0
        """Finalizes options."""
        if self.repository is None:
            self.repository = self.DEFAULT_REPOSITORY
        if self.realm is None:
            self.realm = self.DEFAULT_REALM
"""distutils.command.config
Implements the Distutils 'config' command, a (mostly) empty command class
that exists mainly to be sub-classed by specific module distributions and
applications.  The idea is that while every "config" command is different,
at least they're all named the same, and users always see "config" in the
list of standard commands.  Also, this is a good place to put common
configure-like tasks: "try to compile this C code", or "figure out where
this header file lives".
import os, re
from distutils.errors import DistutilsExecError
from distutils.sysconfig import customize_compiler
LANG_EXT = {"c": ".c", "c++": ".cxx"}
class config(Command):
    description = "prepare to build"
        ('compiler=', None, "specify the compiler type"),
        ('cc=', None, "specify the compiler executable"),
        ('include-dirs=', 'I', "list of directories to search for header files"),
        ('define=', 'D', "C preprocessor macros to define"),
        ('undef=', 'U', "C preprocessor macros to undefine"),
        ('libraries=', 'l', "external C libraries to link with"),
        ('library-dirs=', 'L', "directories to search for external C libraries"),
        ('noisy', None, "show every action (compile, link, run, ...) taken"),
            'dump-source',
            "dump generated source files before attempting to compile them",
    # The three standard command methods: since the "config" command
    # does nothing by default, these are empty.
        self.cc = None
        self.include_dirs = None
        self.libraries = None
        self.library_dirs = None
        # maximal output for now
        self.noisy = 1
        self.dump_source = 1
        # list of temporary files generated along-the-way that we have
        # to clean at some point
        self.temp_files = []
        if self.include_dirs is None:
            self.include_dirs = self.distribution.include_dirs or []
        elif isinstance(self.include_dirs, str):
            self.include_dirs = self.include_dirs.split(os.pathsep)
        if self.libraries is None:
            self.libraries = []
        elif isinstance(self.libraries, str):
            self.libraries = [self.libraries]
        if self.library_dirs is None:
            self.library_dirs = []
        elif isinstance(self.library_dirs, str):
            self.library_dirs = self.library_dirs.split(os.pathsep)
    # Utility methods for actual "config" commands.  The interfaces are
    # loosely based on Autoconf macros of similar names.  Sub-classes
    # may use these freely.
    def _check_compiler(self):
        """Check that 'self.compiler' really is a CCompiler object;
        if not, make it one.
        # We do this late, and only on-demand, because this is an expensive
        from distutils.ccompiler import CCompiler, new_compiler
        if not isinstance(self.compiler, CCompiler):
            self.compiler = new_compiler(
                compiler=self.compiler, dry_run=self.dry_run, force=1
            customize_compiler(self.compiler)
            if self.include_dirs:
                self.compiler.set_include_dirs(self.include_dirs)
            if self.libraries:
                self.compiler.set_libraries(self.libraries)
            if self.library_dirs:
                self.compiler.set_library_dirs(self.library_dirs)
    def _gen_temp_sourcefile(self, body, headers, lang):
        filename = "_configtest" + LANG_EXT[lang]
        with open(filename, "w") as file:
                    file.write("#include <%s>\n" % header)
                file.write("\n")
            file.write(body)
            if body[-1] != "\n":
    def _preprocess(self, body, headers, include_dirs, lang):
        src = self._gen_temp_sourcefile(body, headers, lang)
        out = "_configtest.i"
        self.temp_files.extend([src, out])
        self.compiler.preprocess(src, out, include_dirs=include_dirs)
        return (src, out)
    def _compile(self, body, headers, include_dirs, lang):
        if self.dump_source:
            dump_file(src, "compiling '%s':" % src)
        (obj,) = self.compiler.object_filenames([src])
        self.temp_files.extend([src, obj])
        self.compiler.compile([src], include_dirs=include_dirs)
        return (src, obj)
    def _link(self, body, headers, include_dirs, libraries, library_dirs, lang):
        (src, obj) = self._compile(body, headers, include_dirs, lang)
        prog = os.path.splitext(os.path.basename(src))[0]
        self.compiler.link_executable(
            [obj],
            prog,
            library_dirs=library_dirs,
            target_lang=lang,
        if self.compiler.exe_extension is not None:
            prog = prog + self.compiler.exe_extension
        self.temp_files.append(prog)
        return (src, obj, prog)
    def _clean(self, *filenames):
        if not filenames:
            filenames = self.temp_files
        log.info("removing: %s", ' '.join(filenames))
        for filename in filenames:
    # XXX these ignore the dry-run flag: what to do, what to do? even if
    # you want a dry-run build, you still need some sort of configuration
    # info.  My inclination is to make it up to the real config command to
    # consult 'dry_run', and assume a default (minimal) configuration if
    # true.  The problem with trying to do it here is that you'd have to
    # return either true or false from all the 'try' methods, neither of
    # which is correct.
    # XXX need access to the header search path and maybe default macros.
    def try_cpp(self, body=None, headers=None, include_dirs=None, lang="c"):
        """Construct a source file from 'body' (a string containing lines
        of C/C++ code) and 'headers' (a list of header files to include)
        and run it through the preprocessor.  Return true if the
        preprocessor succeeded, false if there were any errors.
        ('body' probably isn't of much use, but what the heck.)
        from distutils.ccompiler import CompileError
        self._check_compiler()
        ok = True
            self._preprocess(body, headers, include_dirs, lang)
        except CompileError:
        self._clean()
        return ok
    def search_cpp(self, pattern, body=None, headers=None, include_dirs=None, lang="c"):
        """Construct a source file (just like 'try_cpp()'), run it through
        the preprocessor, and return true if any line of the output matches
        'pattern'.  'pattern' should either be a compiled regex object or a
        string containing a regex.  If both 'body' and 'headers' are None,
        preprocesses an empty file -- which can be useful to determine the
        symbols the preprocessor and compiler set by default.
        src, out = self._preprocess(body, headers, include_dirs, lang)
        if isinstance(pattern, str):
            pattern = re.compile(pattern)
        with open(out) as file:
            match = False
                line = file.readline()
                if line == '':
                if pattern.search(line):
                    match = True
    def try_compile(self, body, headers=None, include_dirs=None, lang="c"):
        """Try to compile a source file built from 'body' and 'headers'.
        Return true on success, false otherwise.
            self._compile(body, headers, include_dirs, lang)
        log.info(ok and "success!" or "failure.")
    def try_link(
        include_dirs=None,
        libraries=None,
        library_dirs=None,
        lang="c",
        """Try to compile and link a source file, built from 'body' and
        'headers', to executable form.  Return true on success, false
        otherwise.
        from distutils.ccompiler import CompileError, LinkError
            self._link(body, headers, include_dirs, libraries, library_dirs, lang)
        except (CompileError, LinkError):
    def try_run(
        """Try to compile, link to an executable, and run a program
        built from 'body' and 'headers'.  Return true on success, false
            src, obj, exe = self._link(
                body, headers, include_dirs, libraries, library_dirs, lang
            self.spawn([exe])
        except (CompileError, LinkError, DistutilsExecError):
    # -- High-level methods --------------------------------------------
    # (these are the ones that are actually likely to be useful
    # when implementing a real-world config command!)
    def check_func(
        decl=0,
        call=0,
        """Determine if function 'func' is available by constructing a
        source file that refers to 'func', and compiles and links it.
        If everything succeeds, returns true; otherwise returns false.
        The constructed source file starts out by including the header
        files listed in 'headers'.  If 'decl' is true, it then declares
        'func' (as "int func()"); you probably shouldn't supply 'headers'
        and set 'decl' true in the same call, or you might get errors about
        a conflicting declarations for 'func'.  Finally, the constructed
        'main()' function either references 'func' or (if 'call' is true)
        calls it.  'libraries' and 'library_dirs' are used when
        linking.
        body = []
        if decl:
            body.append("int %s ();" % func)
        body.append("int main () {")
        if call:
            body.append("  %s();" % func)
            body.append("  %s;" % func)
        body.append("}")
        body = "\n".join(body) + "\n"
        return self.try_link(body, headers, include_dirs, libraries, library_dirs)
    def check_lib(
        other_libraries=[],
        """Determine if 'library' is available to be linked against,
        without actually checking that any particular symbols are provided
        by it.  'headers' will be used in constructing the source file to
        be compiled, but the only effect of this is to check if all the
        header files listed are available.  Any libraries listed in
        'other_libraries' will be included in the link, in case 'library'
        has symbols that depend on other libraries.
        return self.try_link(
            "int main (void) { }",
            include_dirs,
            [library] + other_libraries,
            library_dirs,
    def check_header(self, header, include_dirs=None, library_dirs=None, lang="c"):
        """Determine if the system header file named by 'header_file'
        exists and can be found by the preprocessor; return true if so,
        false otherwise.
        return self.try_cpp(
            body="/* No body */", headers=[header], include_dirs=include_dirs
def dump_file(filename, head=None):
    """Dumps a file content into log.info.
    If head is not None, will be dumped before the file content.
    if head is None:
        log.info('%s', filename)
        log.info(head)
    file = open(filename)
        log.info(file.read())
# Added Fortran compiler support to config. Currently useful only for
# try_compile call. try_run works but is untested for most of Fortran
# compilers (they must define linker_exe first).
# Pearu Peterson
from distutils.command.config import config as old_config
from distutils.command.config import LANG_EXT
from distutils.file_util import copy_file
import distutils
from numpy.distutils.exec_command import filepath_from_subprocess_output
from numpy.distutils.mingw32ccompiler import generate_manifest
from numpy.distutils.command.autodist import (check_gcc_function_attribute,
                                              check_gcc_function_attribute_with_intrinsics,
                                              check_gcc_variable_attribute,
                                              check_gcc_version_at_least,
                                              check_inline,
                                              check_restrict,
                                              check_compiler_gcc)
LANG_EXT['f77'] = '.f'
LANG_EXT['f90'] = '.f90'
class config(old_config):
    old_config.user_options += [
        ('fcompiler=', None, "specify the Fortran compiler type"),
        old_config.initialize_options(self)
    def _check_compiler (self):
        old_config._check_compiler(self)
        from numpy.distutils.fcompiler import FCompiler, new_fcompiler
        if sys.platform == 'win32' and (self.compiler.compiler_type in
                                        ('msvc', 'intelw', 'intelemw')):
            # XXX: hack to circumvent a python 2.6 bug with msvc9compiler:
            # initialize call query_vcvarsall, which throws an IOError, and
            # causes an error along the way without much information. We try to
            # catch it here, hoping it is early enough, and print a helpful
            # message instead of Error: None.
            if not self.compiler.initialized:
                    self.compiler.initialize()
                except IOError as e:
                    msg = textwrap.dedent("""\
                        Could not initialize compiler instance: do you have Visual Studio
                        installed?  If you are trying to build with MinGW, please use "python setup.py
                        build -c mingw32" instead.  If you have Visual Studio installed, check it is
                        correctly installed, and the right version (VS 2015 as of this writing).
                        Original exception was: %s, and the Compiler class was %s
                        ============================================================================""") \
                        % (e, self.compiler.__class__.__name__)
                    print(textwrap.dedent("""\
                        ============================================================================"""))
                    raise distutils.errors.DistutilsPlatformError(msg) from e
            # After MSVC is initialized, add an explicit /MANIFEST to linker
            # flags.  See issues gh-4245 and gh-4101 for details.  Also
            # relevant are issues 4431 and 16296 on the Python bug tracker.
            from distutils import msvc9compiler
            if msvc9compiler.get_build_version() >= 10:
                for ldflags in [self.compiler.ldflags_shared,
                                self.compiler.ldflags_shared_debug]:
                    if '/MANIFEST' not in ldflags:
                        ldflags.append('/MANIFEST')
        if not isinstance(self.fcompiler, FCompiler):
            self.fcompiler = new_fcompiler(compiler=self.fcompiler,
                                           dry_run=self.dry_run, force=1,
                                           c_compiler=self.compiler)
            if self.fcompiler is not None:
                self.fcompiler.customize(self.distribution)
                if self.fcompiler.get_version():
                    self.fcompiler.customize_cmd(self)
                    self.fcompiler.show_customization()
    def _wrap_method(self, mth, lang, args):
        save_compiler = self.compiler
        if lang in ['f77', 'f90']:
            self.compiler = self.fcompiler
        if self.compiler is None:
            raise CompileError('%s compiler is not set' % (lang,))
            ret = mth(*((self,)+args))
        except (DistutilsExecError, CompileError) as e:
            self.compiler = save_compiler
            raise CompileError from e
    def _compile (self, body, headers, include_dirs, lang):
        src, obj = self._wrap_method(old_config._compile, lang,
                                     (body, headers, include_dirs, lang))
        # _compile in unixcompiler.py sometimes creates .d dependency files.
        # Clean them up.
        self.temp_files.append(obj + '.d')
        return src, obj
    def _link (self, body,
               headers, include_dirs,
               libraries, library_dirs, lang):
        if self.compiler.compiler_type=='msvc':
            libraries = (libraries or [])[:]
            library_dirs = (library_dirs or [])[:]
                lang = 'c' # always use system linker when using MSVC compiler
                if self.fcompiler:
                    for d in self.fcompiler.library_dirs or []:
                        # correct path when compiling in Cygwin but with
                        # normal Win Python
                        if d.startswith('/usr/lib'):
                                d = subprocess.check_output(['cygpath',
                                                             '-w', d])
                            except (OSError, subprocess.CalledProcessError):
                                d = filepath_from_subprocess_output(d)
                        library_dirs.append(d)
                    for libname in self.fcompiler.libraries or []:
                        if libname not in libraries:
                            libraries.append(libname)
            for libname in libraries:
                if libname.startswith('msvc'): continue
                fileexists = False
                for libdir in library_dirs or []:
                    libfile = os.path.join(libdir, '%s.lib' % (libname))
                    if os.path.isfile(libfile):
                        fileexists = True
                if fileexists: continue
                # make g77-compiled static libs available to MSVC
                for libdir in library_dirs:
                    libfile = os.path.join(libdir, 'lib%s.a' % (libname))
                        # copy libname.a file to name.lib so that MSVC linker
                        # can find it
                        libfile2 = os.path.join(libdir, '%s.lib' % (libname))
                        copy_file(libfile, libfile2)
                        self.temp_files.append(libfile2)
                log.warn('could not find library %r in directories %s' \
                         % (libname, library_dirs))
        elif self.compiler.compiler_type == 'mingw32':
            generate_manifest(self)
        return self._wrap_method(old_config._link, lang,
                                 (body, headers, include_dirs,
                                  libraries, library_dirs, lang))
    def check_header(self, header, include_dirs=None, library_dirs=None, lang='c'):
        return self.try_compile(
                "/* we need a dummy line to make distutils happy */",
                [header], include_dirs)
    def check_decl(self, symbol,
                   headers=None, include_dirs=None):
        body = textwrap.dedent("""
            int main(void)
            #ifndef %s
                (void) %s;
            }""") % (symbol, symbol)
        return self.try_compile(body, headers, include_dirs)
    def check_macro_true(self, symbol,
            #if %s
            #error false or undefined macro
            }""") % (symbol,)
    def check_type(self, type_name, headers=None, include_dirs=None,
            library_dirs=None):
        """Check type availability. Return True if the type can be compiled,
        False otherwise"""
        # First check the type can be compiled
        body = textwrap.dedent(r"""
            int main(void) {
              if ((%(name)s *) 0)
              if (sizeof (%(name)s))
            """) % {'name': type_name}
        st = False
                self._compile(body % {'type': type_name},
                        headers, include_dirs, 'c')
                st = True
            except distutils.errors.CompileError:
        return st
    def check_type_size(self, type_name, headers=None, include_dirs=None, library_dirs=None, expected=None):
        """Check size of a given type."""
            typedef %(type)s npy_check_sizeof_type;
            int main (void)
                static int test_array [1 - 2 * !(((long) (sizeof (npy_check_sizeof_type))) >= 0)];
                test_array [0] = 0
        if expected:
                    static int test_array [1 - 2 * !(((long) (sizeof (npy_check_sizeof_type))) == %(size)s)];
            for size in expected:
                    self._compile(body % {'type': type_name, 'size': size},
        # this fails to *compile* if size > sizeof(type)
                static int test_array [1 - 2 * !(((long) (sizeof (npy_check_sizeof_type))) <= %(size)s)];
        # The principle is simple: we first find low and high bounds of size
        # for the type, where low/high are looked up on a log scale. Then, we
        # do a binary search to find the exact size between low and high
        low = 0
        mid = 0
                self._compile(body % {'type': type_name, 'size': mid},
                #log.info("failure to test for bound %d" % mid)
                low = mid + 1
                mid = 2 * mid + 1
        high = mid
        # Binary search:
        while low != high:
            mid = (high - low) // 2 + low
        return low
    def check_func(self, func,
                   headers=None, include_dirs=None,
                   libraries=None, library_dirs=None,
                   decl=False, call=False, call_args=None):
        # clean up distutils's config a bit: add void to main(), and
        # return a value.
            if type(decl) == str:
                body.append(decl)
                body.append("int %s (void);" % func)
        # Handle MSVC intrinsics: force MS compiler to make a function call.
        # Useful to test for some functions when built with optimization on, to
        # avoid build error because the intrinsic and our 'fake' test
        # declaration do not match.
        body.append("#ifdef _MSC_VER")
        body.append("#pragma function(%s)" % func)
        body.append("#endif")
        body.append("int main (void) {")
            if call_args is None:
                call_args = ''
            body.append("  %s(%s);" % (func, call_args))
        body.append("  return 0;")
        body = '\n'.join(body) + "\n"
        return self.try_link(body, headers, include_dirs,
                             libraries, library_dirs)
    def check_funcs_once(self, funcs,
        """Check a list of functions at once.
        This is useful to speed up things, since all the functions in the funcs
        list will be put in one compilation unit.
        Arguments
        funcs : seq
            list of functions to test
        include_dirs : seq
            list of header paths
        libraries : seq
            list of libraries to link the code snippet to
        library_dirs : seq
            list of library paths
        decl : dict
            for every (key, value), the declaration in the value will be
            used for function in key. If a function is not in the
            dictionary, no declaration will be used.
        call : dict
            for every item (f, value), if the value is True, a call will be
            done to the function f.
            for f, v in decl.items():
                if v:
                    body.append("int %s (void);" % f)
        # Handle MS intrinsics. See check_func for more info.
        for func in funcs:
                if f in call and call[f]:
                    if not (call_args and f in call_args and call_args[f]):
                        args = ''
                        args = call_args[f]
                    body.append("  %s(%s);" % (f, args))
                    body.append("  %s;" % f)
    def check_inline(self):
        """Return the inline keyword recognized by the compiler, empty string
        otherwise."""
        return check_inline(self)
    def check_restrict(self):
        """Return the restrict keyword recognized by the compiler, empty string
        return check_restrict(self)
    def check_compiler_gcc(self):
        """Return True if the C compiler is gcc"""
        return check_compiler_gcc(self)
    def check_gcc_function_attribute(self, attribute, name):
        return check_gcc_function_attribute(self, attribute, name)
    def check_gcc_function_attribute_with_intrinsics(self, attribute, name,
                                                     code, include):
        return check_gcc_function_attribute_with_intrinsics(self, attribute,
                                                            name, code, include)
    def check_gcc_variable_attribute(self, attribute):
        return check_gcc_variable_attribute(self, attribute)
    def check_gcc_version_at_least(self, major, minor=0, patchlevel=0):
        """Return True if the GCC version is greater than or equal to the
        specified version."""
        return check_gcc_version_at_least(self, major, minor, patchlevel)
    def get_output(self, body, headers=None, include_dirs=None,
                   lang="c", use_tee=None):
        built from 'body' and 'headers'. Returns the exit status code
        of the program and its output.
        # 2008-11-16, RemoveMe
        warnings.warn("\n+++++++++++++++++++++++++++++++++++++++++++++++++\n"
                      "Usage of get_output is deprecated: please do not \n"
                      "use it anymore, and avoid configuration checks \n"
                      "involving running executable on the target machine.\n"
                      "+++++++++++++++++++++++++++++++++++++++++++++++++\n",
                      DeprecationWarning, stacklevel=2)
        exitcode, output = 255, ''
            grabber = GrabStdout()
                src, obj, exe = self._link(body, headers, include_dirs,
                                           libraries, library_dirs, lang)
                grabber.restore()
                output = grabber.data
            exe = os.path.join('.', exe)
                # specify cwd arg for consistency with
                # historic usage pattern of exec_command()
                # also, note that exe appears to be a string,
                # which exec_command() handled, but we now
                # use a list for check_output() -- this assumes
                # that exe is always a single command
                output = subprocess.check_output([exe], cwd='.')
            except subprocess.CalledProcessError as exc:
                exitstatus = exc.returncode
                output = ''
                # preserve the EnvironmentError exit status
                # used historically in exec_command()
                exitstatus = 127
                output = filepath_from_subprocess_output(output)
            if hasattr(os, 'WEXITSTATUS'):
                exitcode = os.WEXITSTATUS(exitstatus)
                if os.WIFSIGNALED(exitstatus):
                    sig = os.WTERMSIG(exitstatus)
                    log.error('subprocess exited with signal %d' % (sig,))
                    if sig == signal.SIGINT:
                        # control-C
                        raise KeyboardInterrupt
                exitcode = exitstatus
            log.info("success!")
            log.info("failure.")
        return exitcode, output
class GrabStdout:
        self.sys_stdout = sys.stdout
        self.data = ''
        sys.stdout = self
    def write (self, data):
        self.sys_stdout.write(data)
        self.data += data
    def flush (self):
        self.sys_stdout.flush()
        sys.stdout = self.sys_stdout
conf: dict[str, dict[str, Any]] = {}
default_conf_dir = os.path.join(os.path.expanduser("~"), ".config/fsspec")
conf_dir = os.environ.get("FSSPEC_CONFIG_DIR", default_conf_dir)
def set_conf_env(conf_dict, envdict=os.environ):
    """Set config values from environment variables
    Looks for variables of the form ``FSSPEC_<protocol>`` and
    ``FSSPEC_<protocol>_<kwarg>``. For ``FSSPEC_<protocol>`` the value is parsed
    as a json dictionary and used to ``update`` the config of the
    corresponding protocol. For ``FSSPEC_<protocol>_<kwarg>`` there is no
    attempt to convert the string value, but the kwarg keys will be lower-cased.
    The ``FSSPEC_<protocol>_<kwarg>`` variables are applied after the
    ``FSSPEC_<protocol>`` ones.
    conf_dict : dict(str, dict)
        This dict will be mutated
    envdict : dict-like(str, str)
        Source for the values - usually the real environment
    kwarg_keys = []
    for key in envdict:
        if key.startswith("FSSPEC_") and len(key) > 7 and key[7] != "_":
            if key.count("_") > 1:
                kwarg_keys.append(key)
                value = json.loads(envdict[key])
            except json.decoder.JSONDecodeError as ex:
                    f"Ignoring environment variable {key} due to a parse failure: {ex}"
                    _, proto = key.split("_", 1)
                    conf_dict.setdefault(proto.lower(), {}).update(value)
                        f"Ignoring environment variable {key} due to not being a dict:"
                        f" {type(value)}"
        elif key.startswith("FSSPEC"):
                f"Ignoring environment variable {key} due to having an unexpected name"
    for key in kwarg_keys:
        _, proto, kwarg = key.split("_", 2)
        conf_dict.setdefault(proto.lower(), {})[kwarg.lower()] = envdict[key]
def set_conf_files(cdir, conf_dict):
    """Set config values from files
    Scans for INI and JSON files in the given dictionary, and uses their
    contents to set the config. In case of repeated values, later values
    win.
    In the case of INI files, all values are strings, and these will not
    be converted.
    cdir : str
        Directory to search
    if not os.path.isdir(cdir):
    allfiles = sorted(os.listdir(cdir))
    for fn in allfiles:
        if fn.endswith(".ini"):
            ini = configparser.ConfigParser()
            ini.read(os.path.join(cdir, fn))
            for key in ini:
                if key == "DEFAULT":
                conf_dict.setdefault(key, {}).update(dict(ini[key]))
        if fn.endswith(".json"):
            with open(os.path.join(cdir, fn)) as f:
                js = json.load(f)
            for key in js:
                conf_dict.setdefault(key, {}).update(dict(js[key]))
def apply_config(cls, kwargs, conf_dict=None):
    """Supply default values for kwargs when instantiating class
    Augments the passed kwargs, by finding entries in the config dict
    which match the classes ``.protocol`` attribute (one or more str)
    cls : file system implementation
    kwargs : dict
    conf_dict : dict of dict
        Typically this is the global configuration
    dict : the modified set of kwargs
    if conf_dict is None:
        conf_dict = conf
    protos = cls.protocol if isinstance(cls.protocol, (tuple, list)) else [cls.protocol]
    kw = {}
    for proto in protos:
        # default kwargs from the current state of the config
        if proto in conf_dict:
            kw.update(conf_dict[proto])
    # explicit kwargs always win
    kw.update(**kwargs)
    kwargs = kw
set_conf_files(conf_dir, conf)
set_conf_env(conf)
"""Config handling logic for Flake8."""
from flake8.defaults import VALID_CODE_PREFIX
def _stat_key(s: str) -> tuple[int, int]:
    # same as what's used by samefile / samestat
    st = os.stat(s)
    return st.st_ino, st.st_dev
def _find_config_file(path: str) -> str | None:
    # on windows if the homedir isn't detected this returns back `~`
    home = os.path.expanduser("~")
        home_stat = _stat_key(home) if home != "~" else None
    except OSError:  # FileNotFoundError / PermissionError / etc.
        home_stat = None
    dir_stat = _stat_key(path)
        for candidate in ("setup.cfg", "tox.ini", ".flake8"):
            cfg = configparser.RawConfigParser()
            cfg_path = os.path.join(path, candidate)
                cfg.read(cfg_path, encoding="UTF-8")
            except (UnicodeDecodeError, configparser.ParsingError) as e:
                LOG.warning("ignoring unparseable config %s: %s", cfg_path, e)
                # only consider it a config if it contains flake8 sections
                if "flake8" in cfg or "flake8:local-plugins" in cfg:
                    return cfg_path
        new_path = os.path.dirname(path)
        new_dir_stat = _stat_key(new_path)
        if new_dir_stat == dir_stat or new_dir_stat == home_stat:
            path = new_path
            dir_stat = new_dir_stat
    # did not find any configuration file
def load_config(
    config: str | None,
    extra: list[str],
) -> tuple[configparser.RawConfigParser, str]:
    """Load the configuration given the user options.
    - in ``isolated`` mode, return an empty configuration
    - if a config file is given in ``config`` use that, otherwise attempt to
      discover a configuration using ``tox.ini`` / ``setup.cfg`` / ``.flake8``
    - finally, load any ``extra`` configuration files
    pwd = os.path.abspath(".")
    if isolated:
        return configparser.RawConfigParser(), pwd
    if config is None:
        config = _find_config_file(pwd)
        if not cfg.read(config, encoding="UTF-8"):
            raise exceptions.ExecutionError(
                f"The specified config file does not exist: {config}"
        cfg_dir = os.path.dirname(config)
        cfg_dir = pwd
    # TODO: remove this and replace it with configuration modifying plugins
    # read the additional configs afterwards
    for filename in extra:
        if not cfg.read(filename, encoding="UTF-8"):
                f"The specified config file does not exist: {filename}"
    return cfg, cfg_dir
def parse_config(
    option_manager: OptionManager,
    cfg: configparser.RawConfigParser,
    cfg_dir: str,
    """Parse and normalize the typed configuration options."""
    if "flake8" not in cfg:
    for option_name in cfg["flake8"]:
        option = option_manager.config_options_dict.get(option_name)
        if option is None:
            LOG.debug('Option "%s" is not registered. Ignoring.', option_name)
        # Use the appropriate method to parse the config value
        value: Any
        if option.type is int or option.action == "count":
            value = cfg.getint("flake8", option_name)
        elif option.action in {"store_true", "store_false"}:
            value = cfg.getboolean("flake8", option_name)
            value = cfg.get("flake8", option_name)
        LOG.debug('Option "%s" returned value: %r', option_name, value)
        final_value = option.normalize(value, cfg_dir)
        if option_name in {"ignore", "extend-ignore"}:
            for error_code in final_value:
                if not VALID_CODE_PREFIX.match(error_code):
                        f"Error code {error_code!r} "
                        f"supplied to {option_name!r} option "
                        f"does not match {VALID_CODE_PREFIX.pattern!r}"
        assert option.config_name is not None
        config_dict[option.config_name] = final_value
    return config_dict
from collections.abc import Callable, Iterator, Mapping, MutableMapping
from typing import Any, TypeVar, overload
class undefined:
class EnvironError(Exception):
class Environ(MutableMapping[str, str]):
    def __init__(self, environ: MutableMapping[str, str] = os.environ):
        self._environ = environ
        self._has_been_read: set[str] = set()
    def __getitem__(self, key: str) -> str:
        self._has_been_read.add(key)
        return self._environ.__getitem__(key)
    def __setitem__(self, key: str, value: str) -> None:
        if key in self._has_been_read:
            raise EnvironError(f"Attempting to set environ['{key}'], but the value has already been read.")
        self._environ.__setitem__(key, value)
            raise EnvironError(f"Attempting to delete environ['{key}'], but the value has already been read.")
        self._environ.__delitem__(key)
        return iter(self._environ)
        return len(self._environ)
environ = Environ()
        env_file: str | Path | None = None,
        environ: Mapping[str, str] = environ,
        env_prefix: str = "",
        self.environ = environ
        self.env_prefix = env_prefix
        self.file_values: dict[str, str] = {}
        if env_file is not None:
            if not os.path.isfile(env_file):
                warnings.warn(f"Config file '{env_file}' not found.")
                self.file_values = self._read_file(env_file, encoding)
    def __call__(self, key: str, *, default: None) -> str | None: ...
    def __call__(self, key: str, cast: type[T], default: T = ...) -> T: ...
    def __call__(self, key: str, cast: type[str] = ..., default: str = ...) -> str: ...
        cast: Callable[[Any], T] = ...,
        default: Any = ...,
    ) -> T: ...
    def __call__(self, key: str, cast: type[str] = ..., default: T = ...) -> T | str: ...
        cast: Callable[[Any], Any] | None = None,
        default: Any = undefined,
        return self.get(key, cast, default)
        key = self.env_prefix + key
        if key in self.environ:
            value = self.environ[key]
            return self._perform_cast(key, value, cast)
        if key in self.file_values:
            value = self.file_values[key]
        if default is not undefined:
            return self._perform_cast(key, default, cast)
        raise KeyError(f"Config '{key}' is missing, and has no default.")
    def _read_file(self, file_name: str | Path, encoding: str) -> dict[str, str]:
        file_values: dict[str, str] = {}
        with open(file_name, encoding=encoding) as input_file:
            for line in input_file.readlines():
                if "=" in line and not line.startswith("#"):
                    key, value = line.split("=", 1)
                    key = key.strip()
                    value = value.strip().strip("\"'")
                    file_values[key] = value
        return file_values
    def _perform_cast(
        if cast is None or value is None:
        elif cast is bool and isinstance(value, str):
            mapping = {"true": True, "1": True, "false": False, "0": False}
            if value not in mapping:
                raise ValueError(f"Config '{key}' has value '{value}'. Not a valid bool.")
            return mapping[value]
            return cast(value)
            raise ValueError(f"Config '{key}' has value '{value}'. Not a valid {cast.__name__}.")
"""Configuration for Pydantic models."""
from __future__ import annotations as _annotations
from typing import TYPE_CHECKING, Any, Callable, Literal, TypeVar, Union, cast, overload
from typing_extensions import TypeAlias, TypedDict, Unpack, deprecated
from .aliases import AliasGenerator
from .errors import PydanticUserError
from .warnings import PydanticDeprecatedSince211
    from ._internal._generate_schema import GenerateSchema as _GenerateSchema
    from .fields import ComputedFieldInfo, FieldInfo
__all__ = ('ConfigDict', 'with_config')
JsonValue: TypeAlias = Union[int, float, str, bool, None, list['JsonValue'], 'JsonDict']
JsonDict: TypeAlias = dict[str, JsonValue]
JsonEncoder = Callable[[Any], Any]
JsonSchemaExtraCallable: TypeAlias = Union[
    Callable[[JsonDict], None],
    Callable[[JsonDict, type[Any]], None],
ExtraValues = Literal['allow', 'ignore', 'forbid']
class ConfigDict(TypedDict, total=False):
    """A TypedDict for configuring Pydantic behaviour."""
    title: str | None
    """The title for the generated JSON schema, defaults to the model's name"""
    model_title_generator: Callable[[type], str] | None
    """A callable that takes a model class and returns the title for it. Defaults to `None`."""
    field_title_generator: Callable[[str, FieldInfo | ComputedFieldInfo], str] | None
    """A callable that takes a field's name and info and returns title for it. Defaults to `None`."""
    str_to_lower: bool
    """Whether to convert all characters to lowercase for str types. Defaults to `False`."""
    str_to_upper: bool
    """Whether to convert all characters to uppercase for str types. Defaults to `False`."""
    str_strip_whitespace: bool
    """Whether to strip leading and trailing whitespace for str types."""
    str_min_length: int
    """The minimum length for str types. Defaults to `None`."""
    str_max_length: int | None
    """The maximum length for str types. Defaults to `None`."""
    extra: ExtraValues | None
    Whether to ignore, allow, or forbid extra data during model initialization. Defaults to `'ignore'`.
    Three configuration values are available:
    - `'ignore'`: Providing extra data is ignored (the default):
      class User(BaseModel):
          model_config = ConfigDict(extra='ignore')  # (1)!
      user = User(name='John Doe', age=20)  # (2)!
      print(user)
      #> name='John Doe'
        1. This is the default behaviour.
        2. The `age` argument is ignored.
    - `'forbid'`: Providing extra data is not permitted, and a [`ValidationError`][pydantic_core.ValidationError]
      will be raised if this is the case:
      from pydantic import BaseModel, ConfigDict, ValidationError
          model_config = ConfigDict(extra='forbid')
          Model(x=1, y='a')
      except ValidationError as exc:
          print(exc)
          1 validation error for Model
          y
            Extra inputs are not permitted [type=extra_forbidden, input_value='a', input_type=str]
    - `'allow'`: Providing extra data is allowed and stored in the `__pydantic_extra__` dictionary attribute:
          model_config = ConfigDict(extra='allow')
      m = Model(x=1, y='a')
      assert m.__pydantic_extra__ == {'y': 'a'}
      By default, no validation will be applied to these extra items, but you can set a type for the values by overriding
      the type annotation for `__pydantic_extra__`:
      from pydantic import BaseModel, ConfigDict, Field, ValidationError
          __pydantic_extra__: dict[str, int] = Field(init=False)  # (1)!
            Input should be a valid integer, unable to parse string as an integer [type=int_parsing, input_value='a', input_type=str]
      m = Model(x=1, y='2')
      assert m.x == 1
      assert m.y == 2
      assert m.model_dump() == {'x': 1, 'y': 2}
      assert m.__pydantic_extra__ == {'y': 2}
        1. The `= Field(init=False)` does not have any effect at runtime, but prevents the `__pydantic_extra__` field from
           being included as a parameter to the model's `__init__` method by type checkers.
    As well as specifying an `extra` configuration value on the model, you can also provide it as an argument to the validation methods.
    This will override any `extra` configuration value set on the model:
        model_config = ConfigDict(extra="allow")
        # Override model config and forbid extra fields just this time
        Model.model_validate({"x": 1, "y": 2}, extra="forbid")
          Extra inputs are not permitted [type=extra_forbidden, input_value=2, input_type=int]
    frozen: bool
    Whether models are faux-immutable, i.e. whether `__setattr__` is allowed, and also generates
    a `__hash__()` method for the model. This makes instances of the model potentially hashable if all the
    attributes are hashable. Defaults to `False`.
        On V1, the inverse of this setting was called `allow_mutation`, and was `True` by default.
    populate_by_name: bool
    Whether an aliased field may be populated by its name as given by the model
    attribute, as well as the alias. Defaults to `False`.
        `populate_by_name` usage is not recommended in v2.11+ and will be deprecated in v3.
        Instead, you should use the [`validate_by_name`][pydantic.config.ConfigDict.validate_by_name] configuration setting.
        When `validate_by_name=True` and `validate_by_alias=True`, this is strictly equivalent to the
        previous behavior of `populate_by_name=True`.
        In v2.11, we also introduced a [`validate_by_alias`][pydantic.config.ConfigDict.validate_by_alias] setting that introduces more fine grained
        control for validation behavior.
        Here's how you might go about using the new settings to achieve the same behavior:
            model_config = ConfigDict(validate_by_name=True, validate_by_alias=True)
            my_field: str = Field(alias='my_alias')  # (1)!
        m = Model(my_alias='foo')  # (2)!
        print(m)
        #> my_field='foo'
        m = Model(my_field='foo')  # (3)!
        1. The field `'my_field'` has an alias `'my_alias'`.
        2. The model is populated by the alias `'my_alias'`.
        3. The model is populated by the attribute name `'my_field'`.
    use_enum_values: bool
    Whether to populate models with the `value` property of enums, rather than the raw enum.
    This may be useful if you want to serialize `model.model_dump()` later. Defaults to `False`.
        If you have an `Optional[Enum]` value that you set a default for, you need to use `validate_default=True`
        for said Field to ensure that the `use_enum_values` flag takes effect on the default, as extracting an
        enum's value occurs during validation, not serialization.
    class SomeEnum(Enum):
        FOO = 'foo'
        BAR = 'bar'
        BAZ = 'baz'
    class SomeModel(BaseModel):
        model_config = ConfigDict(use_enum_values=True)
        some_enum: SomeEnum
        another_enum: Optional[SomeEnum] = Field(
            default=SomeEnum.FOO, validate_default=True
    model1 = SomeModel(some_enum=SomeEnum.BAR)
    print(model1.model_dump())
    #> {'some_enum': 'bar', 'another_enum': 'foo'}
    model2 = SomeModel(some_enum=SomeEnum.BAR, another_enum=SomeEnum.BAZ)
    print(model2.model_dump())
    #> {'some_enum': 'bar', 'another_enum': 'baz'}
    validate_assignment: bool
    Whether to validate the data when the model is changed. Defaults to `False`.
    The default behavior of Pydantic is to validate the data when the model is created.
    In case the user changes the data after the model is created, the model is _not_ revalidated.
    user = User(name='John Doe')  # (1)!
    user.name = 123  # (1)!
    #> name=123
    1. The validation happens only when the model is created.
    2. The validation does not happen when the data is changed.
    In case you want to revalidate the model when the data is changed, you can use `validate_assignment=True`:
    from pydantic import BaseModel, ValidationError
    class User(BaseModel, validate_assignment=True):  # (1)!
    user = User(name='John Doe')  # (2)!
        user.name = 123  # (3)!
        1 validation error for User
          Input should be a valid string [type=string_type, input_value=123, input_type=int]
    1. You can either use class keyword arguments, or `model_config` to set `validate_assignment=True`.
    2. The validation happens when the model is created.
    3. The validation _also_ happens when the data is changed.
    arbitrary_types_allowed: bool
    Whether arbitrary types are allowed for field types. Defaults to `False`.
    # This is not a pydantic model, it's an arbitrary class
    class Pet:
        def __init__(self, name: str):
        model_config = ConfigDict(arbitrary_types_allowed=True)
        pet: Pet
        owner: str
    pet = Pet(name='Hedwig')
    # A simple check of instance type is used to validate the data
    model = Model(owner='Harry', pet=pet)
    print(model)
    #> pet=<__main__.Pet object at 0x0123456789ab> owner='Harry'
    print(model.pet)
    #> <__main__.Pet object at 0x0123456789ab>
    print(model.pet.name)
    #> Hedwig
    print(type(model.pet))
    #> <class '__main__.Pet'>
        # If the value is not an instance of the type, it's invalid
        Model(owner='Harry', pet='Hedwig')
        pet
          Input should be an instance of Pet [type=is_instance_of, input_value='Hedwig', input_type=str]
    # Nothing in the instance of the arbitrary type is checked
    # Here name probably should have been a str, but it's not validated
    pet2 = Pet(name=42)
    model2 = Model(owner='Harry', pet=pet2)
    print(model2)
    print(model2.pet)
    print(model2.pet.name)
    #> 42
    print(type(model2.pet))
    from_attributes: bool
    Whether to build models and look up discriminators of tagged unions using python object attributes.
    loc_by_alias: bool
    """Whether to use the actual key provided in the data (e.g. alias) for error `loc`s rather than the field's name. Defaults to `True`."""
    alias_generator: Callable[[str], str] | AliasGenerator | None
    A callable that takes a field name and returns an alias for it
    or an instance of [`AliasGenerator`][pydantic.aliases.AliasGenerator]. Defaults to `None`.
    When using a callable, the alias generator is used for both validation and serialization.
    If you want to use different alias generators for validation and serialization, you can use
    [`AliasGenerator`][pydantic.aliases.AliasGenerator] instead.
    If data source field names do not match your code style (e.g. CamelCase fields),
    you can automatically generate aliases using `alias_generator`. Here's an example with
    a basic callable:
    from pydantic.alias_generators import to_pascal
    class Voice(BaseModel):
        model_config = ConfigDict(alias_generator=to_pascal)
        language_code: str
    voice = Voice(Name='Filiz', LanguageCode='tr-TR')
    print(voice.language_code)
    #> tr-TR
    print(voice.model_dump(by_alias=True))
    #> {'Name': 'Filiz', 'LanguageCode': 'tr-TR'}
    [`AliasGenerator`][pydantic.aliases.AliasGenerator].
    from pydantic import AliasGenerator, BaseModel, ConfigDict
    from pydantic.alias_generators import to_camel, to_pascal
    class Athlete(BaseModel):
        sport: str
            alias_generator=AliasGenerator(
                validation_alias=to_camel,
                serialization_alias=to_pascal,
    athlete = Athlete(firstName='John', lastName='Doe', sport='track')
    print(athlete.model_dump(by_alias=True))
    #> {'FirstName': 'John', 'LastName': 'Doe', 'Sport': 'track'}
        Pydantic offers three built-in alias generators: [`to_pascal`][pydantic.alias_generators.to_pascal],
        [`to_camel`][pydantic.alias_generators.to_camel], and [`to_snake`][pydantic.alias_generators.to_snake].
    ignored_types: tuple[type, ...]
    """A tuple of types that may occur as values of class attributes without annotations. This is
    typically used for custom descriptors (classes that behave like `property`). If an attribute is set on a
    class without an annotation and has a type that is not in this tuple (or otherwise recognized by
    _pydantic_), an error will be raised. Defaults to `()`.
    allow_inf_nan: bool
    """Whether to allow infinity (`+inf` an `-inf`) and NaN values to float and decimal fields. Defaults to `True`."""
    json_schema_extra: JsonDict | JsonSchemaExtraCallable | None
    """A dict or callable to provide extra JSON schema properties. Defaults to `None`."""
    json_encoders: dict[type[object], JsonEncoder] | None
    A `dict` of custom JSON encoders for specific types. Defaults to `None`.
    /// version-deprecated | v2
    This configuration option is a carryover from v1. We originally planned to remove it in v2 but didn't have a 1:1 replacement
    so we are keeping it for now. It is still deprecated and will likely be removed in the future.
    # new in V2
    strict: bool
    Whether strict validation is applied to all fields on the model.
    By default, Pydantic attempts to coerce values to the correct type, when possible.
    There are situations in which you may want to disable this behavior, and instead raise an error if a value's type
    does not match the field's type annotation.
    To configure strict mode for all fields on a model, you can set `strict=True` on the model.
        model_config = ConfigDict(strict=True)
    See [Strict Mode](../concepts/strict_mode.md) for more details.
    See the [Conversion Table](../concepts/conversion_table.md) for more details on how Pydantic converts data in both
    strict and lax modes.
    /// version-added | v2
    # whether instances of models and dataclasses (including subclass instances) should re-validate, default 'never'
    revalidate_instances: Literal['always', 'never', 'subclass-instances']
    When and how to revalidate models and dataclasses during validation. Can be one of:
    - `'never'`: will *not* revalidate models and dataclasses during validation
    - `'always'`: will revalidate models and dataclasses during validation
    - `'subclass-instances'`: will revalidate models and dataclasses during validation if the instance is a
        subclass of the model or dataclass
    The default is `'never'` (no revalidation).
    This configuration only affects *the current model* it is applied on, and does *not* populate to the models
    referenced in fields.
    class User(BaseModel, revalidate_instances='never'):  # (1)!
    class Transaction(BaseModel):
        user: User
    my_user = User(name='John')
    t = Transaction(user=my_user)
    my_user.name = 1  # (2)!
    t = Transaction(user=my_user)  # (3)!
    print(t)
    #> user=User(name=1)
    1. This is the default behavior.
    2. The assignment is *not* validated, unless you set [`validate_assignment`][pydantic.ConfigDict.validate_assignment] in the configuration.
    3. Since `revalidate_instances` is set to `'never'`, the user instance is not revalidated.
    Here is an example demonstrating the behavior of `'subclass-instances'`:
    class User(BaseModel, revalidate_instances='subclass-instances'):
    class SubUser(User):
    my_user.name = 1  # (1)!
    t = Transaction(user=my_user)  # (2)!
    my_sub_user = SubUser(name='John', age=20)
    t = Transaction(user=my_sub_user)
    print(t)  # (3)!
    #> user=User(name='John')
    1. The assignment is *not* validated, unless you set [`validate_assignment`][pydantic.ConfigDict.validate_assignment] in the configuration.
    2. Because `my_user` is a "direct" instance of `User`, it is *not* being revalidated. It would have been the case if
      `revalidate_instances` was set to `'always'`.
    3. Because `my_sub_user` is an instance of a `User` subclass, it is being revalidated. In this case, Pydantic coerces `my_sub_user` to the defined
       `User` class defined on `Transaction`. If one of its fields had an invalid value, a validation error would have been raised.
    ser_json_timedelta: Literal['iso8601', 'float']
    The format of JSON serialized timedeltas. Accepts the string values of `'iso8601'` and
    `'float'`. Defaults to `'iso8601'`.
    - `'iso8601'` will serialize timedeltas to [ISO 8601 text format](https://en.wikipedia.org/wiki/ISO_8601#Durations).
    - `'float'` will serialize timedeltas to the total number of seconds.
    /// version-changed | v2.12
    It is now recommended to use the [`ser_json_temporal`][pydantic.config.ConfigDict.ser_json_temporal]
    setting. `ser_json_timedelta` will be deprecated in v3.
    ser_json_temporal: Literal['iso8601', 'seconds', 'milliseconds']
    The format of JSON serialized temporal types from the [`datetime`][] module. This includes:
    - [`datetime.datetime`][]
    - [`datetime.date`][]
    - [`datetime.time`][]
    - [`datetime.timedelta`][]
    Can be one of:
    - `'iso8601'` will serialize date-like types to [ISO 8601 text format](https://en.wikipedia.org/wiki/ISO_8601#Durations).
    - `'milliseconds'` will serialize date-like types to a floating point number of milliseconds since the epoch.
    - `'seconds'` will serialize date-like types to a floating point number of seconds since the epoch.
    Defaults to `'iso8601'`.
    /// version-added | v2.12
    This setting replaces [`ser_json_timedelta`][pydantic.config.ConfigDict.ser_json_timedelta],
    which will be deprecated in v3. `ser_json_temporal` adds more configurability for the other temporal types.
    val_temporal_unit: Literal['seconds', 'milliseconds', 'infer']
    The unit to assume for validating numeric input for datetime-like types ([`datetime.datetime`][] and [`datetime.date`][]). Can be one of:
    - `'seconds'` will validate date or time numeric inputs as seconds since the [epoch].
    - `'milliseconds'` will validate date or time numeric inputs as milliseconds since the [epoch].
    - `'infer'` will infer the unit from the string numeric input on unix time as:
        * seconds since the [epoch] if $-2^{10} <= v <= 2^{10}$
        * milliseconds since the [epoch] (if $v < -2^{10}$ or $v > 2^{10}$).
    Defaults to `'infer'`.
    [epoch]: https://en.wikipedia.org/wiki/Unix_time
    ser_json_bytes: Literal['utf8', 'base64', 'hex']
    The encoding of JSON serialized bytes. Defaults to `'utf8'`.
    Set equal to `val_json_bytes` to get back an equal value after serialization round trip.
    - `'utf8'` will serialize bytes to UTF-8 strings.
    - `'base64'` will serialize bytes to URL safe base64 strings.
    - `'hex'` will serialize bytes to hexadecimal strings.
    val_json_bytes: Literal['utf8', 'base64', 'hex']
    The encoding of JSON serialized bytes to decode. Defaults to `'utf8'`.
    Set equal to `ser_json_bytes` to get back an equal value after serialization round trip.
    - `'utf8'` will deserialize UTF-8 strings to bytes.
    - `'base64'` will deserialize URL safe base64 strings to bytes.
    - `'hex'` will deserialize hexadecimal strings to bytes.
    ser_json_inf_nan: Literal['null', 'constants', 'strings']
    The encoding of JSON serialized infinity and NaN float values. Defaults to `'null'`.
    - `'null'` will serialize infinity and NaN values as `null`.
    - `'constants'` will serialize infinity and NaN values as `Infinity` and `NaN`.
    - `'strings'` will serialize infinity as string `"Infinity"` and NaN as string `"NaN"`.
    # whether to validate default values during validation, default False
    validate_default: bool
    """Whether to validate default values during validation. Defaults to `False`."""
    validate_return: bool
    """Whether to validate the return value from call validators. Defaults to `False`."""
    protected_namespaces: tuple[str | Pattern[str], ...]
    A tuple of strings and/or regex patterns that prevent models from having fields with names that conflict with its existing members/methods.
    Strings are matched on a prefix basis. For instance, with `'dog'`, having a field named `'dog_name'` will be disallowed.
    Regex patterns are matched on the entire field name. For instance, with the pattern `'^dog$'`, having a field named `'dog'` will be disallowed,
    but `'dog_name'` will be accepted.
    Defaults to `('model_validate', 'model_dump')`. This default is used to prevent collisions with the existing (and possibly future)
    [validation](../concepts/models.md#validating-data) and [serialization](../concepts/serialization.md#serializing-data) methods.
    warnings.filterwarnings('error')  # Raise warnings as errors
            model_dump_something: str
    except UserWarning as e:
        Field 'model_dump_something' in 'Model' conflicts with protected namespace 'model_dump'.
        You may be able to solve this by setting the 'protected_namespaces' configuration to ('model_validate',).
    You can customize this behavior using the `protected_namespaces` setting:
    ```python {test="skip"}
    with warnings.catch_warnings(record=True) as caught_warnings:
        warnings.simplefilter('always')  # Catch all warnings
            safe_field: str
            also_protect_field: str
            protect_this: str
                protected_namespaces=(
                    'protect_me_',
                    'also_protect_',
                    re.compile('^protect_this$'),
    for warning in caught_warnings:
        print(f'{warning.message}')
        Field 'also_protect_field' in 'Model' conflicts with protected namespace 'also_protect_'.
        You may be able to solve this by setting the 'protected_namespaces' configuration to ('protect_me_', re.compile('^protect_this$'))`.
        Field 'protect_this' in 'Model' conflicts with protected namespace 're.compile('^protect_this$')'.
        You may be able to solve this by setting the 'protected_namespaces' configuration to ('protect_me_', 'also_protect_')`.
    While Pydantic will only emit a warning when an item is in a protected namespace but does not actually have a collision,
    an error _is_ raised if there is an actual collision with an existing attribute:
            model_validate: str
            model_config = ConfigDict(protected_namespaces=('model_',))
        Field 'model_validate' conflicts with member <bound method BaseModel.model_validate of <class 'pydantic.main.BaseModel'>> of protected namespace 'model_'.
    /// version-changed | v2.10
    The default protected namespaces was changed from `('model_',)` to `('model_validate', 'model_dump')`, to allow
    for fields like `model_id`, `model_name` to be used.
    hide_input_in_errors: bool
    Whether to hide inputs when printing errors. Defaults to `False`.
    Pydantic shows the input value and type when it raises `ValidationError` during the validation.
        Model(a=123)
    You can hide the input value and type by setting the `hide_input_in_errors` config to `True`.
        model_config = ConfigDict(hide_input_in_errors=True)
          Input should be a valid string [type=string_type]
    defer_build: bool
    Whether to defer model validator and serializer construction until the first model validation. Defaults to False.
    This can be useful to avoid the overhead of building models which are only
    used nested within other models, or when you want to manually define type namespace via
    [`Model.model_rebuild(_types_namespace=...)`][pydantic.BaseModel.model_rebuild].
    The setting also applies to [Pydantic dataclasses](../concepts/dataclasses.md) and [type adapters](../concepts/type_adapter.md).
    plugin_settings: dict[str, object] | None
    """A `dict` of settings for plugins. Defaults to `None`."""
    schema_generator: type[_GenerateSchema] | None
    The `GenerateSchema` class to use during core schema generation.
    /// version-deprecated | v2.10
    The `GenerateSchema` class is private and highly subject to change.
    json_schema_serialization_defaults_required: bool
    Whether fields with default values should be marked as required in the serialization schema. Defaults to `False`.
    This ensures that the serialization schema will reflect the fact a field with a default will always be present
    when serializing the model, even though it is not required for validation.
    However, there are scenarios where this may be undesirable — in particular, if you want to share the schema
    between validation and serialization, and don't mind fields with defaults being marked as not required during
    serialization. See [#7209](https://github.com/pydantic/pydantic/issues/7209) for more details.
        a: str = 'a'
        model_config = ConfigDict(json_schema_serialization_defaults_required=True)
    print(Model.model_json_schema(mode='validation'))
        'properties': {'a': {'default': 'a', 'title': 'A', 'type': 'string'}},
        'title': 'Model',
    print(Model.model_json_schema(mode='serialization'))
        'required': ['a'],
    /// version-added | v2.4
    json_schema_mode_override: Literal['validation', 'serialization', None]
    If not `None`, the specified mode will be used to generate the JSON schema regardless of what `mode` was passed to
    the function call. Defaults to `None`.
    This provides a way to force the JSON schema generation to reflect a specific mode, e.g., to always use the
    validation schema.
    It can be useful when using frameworks (such as FastAPI) that may generate different schemas for validation
    and serialization that must both be referenced from the same schema; when this happens, we automatically append
    `-Input` to the definition reference for the validation schema and `-Output` to the definition reference for the
    serialization schema. By specifying a `json_schema_mode_override` though, this prevents the conflict between
    the validation and serialization schemas (since both will use the specified schema), and so prevents the suffixes
    from being added to the definition references.
    from pydantic import BaseModel, ConfigDict, Json
        a: Json[int]  # requires a string to validate, but will dump an int
        'properties': {'a': {'title': 'A', 'type': 'integer'}},
    class ForceInputModel(Model):
        # the following ensures that even with mode='serialization', we
        # will get the schema that would be generated for validation.
        model_config = ConfigDict(json_schema_mode_override='validation')
    print(ForceInputModel.model_json_schema(mode='serialization'))
            'a': {
                'contentMediaType': 'application/json',
                'contentSchema': {'type': 'integer'},
                'title': 'A',
                'type': 'string',
        'title': 'ForceInputModel',
    coerce_numbers_to_str: bool
    If `True`, enables automatic coercion of any `Number` type to `str` in "lax" (non-strict) mode. Defaults to `False`.
    Pydantic doesn't allow number types (`int`, `float`, `Decimal`) to be coerced as type `str` by default.
        print(Model(value=42))
          Input should be a valid string [type=string_type, input_value=42, input_type=int]
        model_config = ConfigDict(coerce_numbers_to_str=True)
    repr(Model(value=42).value)
    #> "42"
    repr(Model(value=42.13).value)
    #> "42.13"
    repr(Model(value=Decimal('42.13')).value)
    regex_engine: Literal['rust-regex', 'python-re']
    The regex engine to be used for pattern validation.
    Defaults to `'rust-regex'`.
    - `'rust-regex'` uses the [`regex`](https://docs.rs/regex) Rust crate,
      which is non-backtracking and therefore more DDoS resistant, but does not support all regex features.
    - `'python-re'` use the [`re`][] module, which supports all regex features, but may be slower.
        If you use a compiled regex pattern, the `'python-re'` engine will be used regardless of this setting.
        This is so that flags such as [`re.IGNORECASE`][] are respected.
        model_config = ConfigDict(regex_engine='python-re')
        value: str = Field(pattern=r'^abc(?=def)')
    print(Model(value='abcdef').value)
    #> abcdef
        print(Model(value='abxyzcdef'))
          String should match pattern '^abc(?=def)' [type=string_pattern_mismatch, input_value='abxyzcdef', input_type=str]
    /// version-added | v2.5
    validation_error_cause: bool
    If `True`, Python exceptions that were part of a validation failure will be shown as an exception group as a cause. Can be useful for debugging. Defaults to `False`.
        Python 3.10 and older don't support exception groups natively. <=3.10, backport must be installed: `pip install exceptiongroup`.
        The structure of validation errors are likely to change in future Pydantic versions. Pydantic offers no guarantees about their structure. Should be used for visual traceback debugging only.
    use_attribute_docstrings: bool
    Whether docstrings of attributes (bare string literals immediately following the attribute declaration)
    should be used for field descriptions. Defaults to `False`.
        model_config = ConfigDict(use_attribute_docstrings=True)
        x: str
        Example of an attribute docstring
        y: int = Field(description="Description in Field")
        Description in Field overrides attribute docstring
    print(Model.model_fields["x"].description)
    # > Example of an attribute docstring
    print(Model.model_fields["y"].description)
    # > Description in Field
    This requires the source code of the class to be available at runtime.
    !!! warning "Usage with `TypedDict` and stdlib dataclasses"
        Due to current limitations, attribute docstrings detection may not work as expected when using
        [`TypedDict`][typing.TypedDict] and stdlib dataclasses, in particular when:
        - inheritance is being used.
        - multiple classes have the same name in the same source file (unless Python 3.13 or greater is used).
    /// version-added | v2.7
    cache_strings: bool | Literal['all', 'keys', 'none']
    Whether to cache strings to avoid constructing new Python objects. Defaults to True.
    Enabling this setting should significantly improve validation performance while increasing memory usage slightly.
    - `True` or `'all'` (the default): cache all strings
    - `'keys'`: cache only dictionary keys
    - `False` or `'none'`: no caching
        `True` or `'all'` is required to cache strings during general validation because
        validators don't know if they're in a key or a value.
    !!! tip
        If repeated strings are rare, it's recommended to use `'keys'` or `'none'` to reduce memory usage,
        as the performance difference is minimal if repeated strings are rare.
    validate_by_alias: bool
    Whether an aliased field may be populated by its alias. Defaults to `True`.
    Here's an example of disabling validation by alias:
        model_config = ConfigDict(validate_by_name=True, validate_by_alias=False)
        my_field: str = Field(validation_alias='my_alias')  # (1)!
    m = Model(my_field='foo')  # (2)!
    2. The model can only be populated by the attribute name `'my_field'`.
        You cannot set both `validate_by_alias` and `validate_by_name` to `False`.
        This would make it impossible to populate an attribute.
        See [usage errors](../errors/usage_errors.md#validate-by-alias-and-name-false) for an example.
        If you set `validate_by_alias` to `False`, under the hood, Pydantic dynamically sets
        `validate_by_name` to `True` to ensure that validation can still occur.
    /// version-added | v2.11
    This setting was introduced in conjunction with [`validate_by_name`][pydantic.ConfigDict.validate_by_name]
    to empower users with more fine grained validation control.
    validate_by_name: bool
    attribute. Defaults to `False`.
    This setting was introduced in conjunction with [`validate_by_alias`][pydantic.ConfigDict.validate_by_alias]
    to empower users with more fine grained validation control. It is an alternative to [`populate_by_name`][pydantic.ConfigDict.populate_by_name],
    that enables validation by name **and** by alias.
    serialize_by_alias: bool
    Whether an aliased field should be serialized by its alias. Defaults to `False`.
    Note: In v2.11, `serialize_by_alias` was introduced to address the
    [popular request](https://github.com/pydantic/pydantic/issues/8379)
    for consistency with alias behavior for validation and serialization settings.
    In v3, the default value is expected to change to `True` for consistency with the validation default.
        model_config = ConfigDict(serialize_by_alias=True)
        my_field: str = Field(serialization_alias='my_alias')  # (1)!
    m = Model(my_field='foo')
    print(m.model_dump())  # (2)!
    #> {'my_alias': 'foo'}
    2. The model is serialized using the alias `'my_alias'` for the `'my_field'` attribute.
    This setting was introduced to address the [popular request](https://github.com/pydantic/pydantic/issues/8379)
    for consistency with alias behavior for validation and serialization.
    url_preserve_empty_path: bool
    Whether to preserve empty URL paths when validating values for a URL type. Defaults to `False`.
    from pydantic import AnyUrl, BaseModel, ConfigDict
        model_config = ConfigDict(url_preserve_empty_path=True)
    m = Model(url='http://example.com')
    print(m.url)
    #> http://example.com
_TypeT = TypeVar('_TypeT', bound=type)
@deprecated('Passing `config` as a keyword argument is deprecated. Pass `config` as a positional argument instead.')
def with_config(*, config: ConfigDict) -> Callable[[_TypeT], _TypeT]: ...
def with_config(config: ConfigDict, /) -> Callable[[_TypeT], _TypeT]: ...
def with_config(**config: Unpack[ConfigDict]) -> Callable[[_TypeT], _TypeT]: ...
def with_config(config: ConfigDict | None = None, /, **kwargs: Any) -> Callable[[_TypeT], _TypeT]:
    """!!! abstract "Usage Documentation"
        [Configuration with other types](../concepts/config.md#configuration-on-other-supported-types)
    A convenience decorator to set a [Pydantic configuration](config.md) on a `TypedDict` or a `dataclass` from the standard library.
    Although the configuration can be set using the `__pydantic_config__` attribute, it does not play well with type checkers,
    especially with `TypedDict`.
    !!! example "Usage"
        from pydantic import ConfigDict, TypeAdapter, with_config
        @with_config(ConfigDict(str_to_lower=True))
        class TD(TypedDict):
        ta = TypeAdapter(TD)
        print(ta.validate_python({'x': 'ABC'}))
        #> {'x': 'abc'}
    /// deprecated-removed | v2.11 v3
    Passing `config` as a keyword argument.
    /// version-changed | v2.11
    Keyword arguments can be provided directly instead of a config dictionary.
    if config is not None and kwargs:
        raise ValueError('Cannot specify both `config` and keyword arguments')
    if len(kwargs) == 1 and (kwargs_conf := kwargs.get('config')) is not None:
            'Passing `config` as a keyword argument is deprecated. Pass `config` as a positional argument instead',
            category=PydanticDeprecatedSince211,
        final_config = cast(ConfigDict, kwargs_conf)
        final_config = config if config is not None else cast(ConfigDict, kwargs)
    def inner(class_: _TypeT, /) -> _TypeT:
        # Ideally, we would check for `class_` to either be a `TypedDict` or a stdlib dataclass.
        # However, the `@with_config` decorator can be applied *after* `@dataclass`. To avoid
        # common mistakes, we at least check for `class_` to not be a Pydantic model.
        from ._internal._utils import is_model_class
        if is_model_class(class_):
            raise PydanticUserError(
                f'Cannot use `with_config` on {class_.__name__} as it is a Pydantic model',
                code='with-config-on-model',
        class_.__pydantic_config__ = final_config
        return class_
__getattr__ = getattr_migration(__name__)
from .._internal import _config
from ..warnings import PydanticDeprecatedSince20
    # See PyCharm issues https://youtrack.jetbrains.com/issue/PY-21915
    # and https://youtrack.jetbrains.com/issue/PY-51428
    DeprecationWarning = PydanticDeprecatedSince20
__all__ = 'BaseConfig', 'Extra'
class _ConfigMetaclass(type):
            obj = _config.config_defaults[item]
            warnings.warn(_config.DEPRECATION_MESSAGE, DeprecationWarning)
            raise AttributeError(f"type object '{self.__name__}' has no attribute {exc}") from exc
@deprecated('BaseConfig is deprecated. Use the `pydantic.ConfigDict` instead.', category=PydanticDeprecatedSince20)
class BaseConfig(metaclass=_ConfigMetaclass):
    """This class is only retained for backwards compatibility.
    !!! Warning "Deprecated"
        BaseConfig is deprecated. Use the [`pydantic.ConfigDict`][pydantic.ConfigDict] instead.
            obj = super().__getattribute__(item)
                return getattr(type(self), item)
                # re-raising changes the displayed text to reflect that `self` is not a type
                raise AttributeError(str(exc)) from exc
class _ExtraMeta(type):
    def __getattribute__(self, __name: str) -> Any:
        # The @deprecated decorator accesses other attributes, so we only emit a warning for the expected ones
        if __name in {'allow', 'ignore', 'forbid'}:
                "`pydantic.config.Extra` is deprecated, use literal values instead (e.g. `extra='allow'`)",
        return super().__getattribute__(__name)
    "Extra is deprecated. Use literal values instead (e.g. `extra='allow'`)", category=PydanticDeprecatedSince20
class Extra(metaclass=_ExtraMeta):
    allow: Literal['allow'] = 'allow'
    ignore: Literal['ignore'] = 'ignore'
    forbid: Literal['forbid'] = 'forbid'
from typing import TYPE_CHECKING, Any, Callable, Dict, ForwardRef, Optional, Tuple, Type, Union
from pydantic.v1.typing import AnyArgTCallable, AnyCallable
from pydantic.v1.utils import GetterDict
from pydantic.v1.version import compiled
    from pydantic.v1.fields import ModelField
    from pydantic.v1.main import BaseModel
    ConfigType = Type['BaseConfig']
    class SchemaExtraCallable(Protocol):
        def __call__(self, schema: Dict[str, Any]) -> None:
        def __call__(self, schema: Dict[str, Any], model_class: Type[BaseModel]) -> None:
    SchemaExtraCallable = Callable[..., None]
__all__ = 'BaseConfig', 'ConfigDict', 'get_config', 'Extra', 'inherit_config', 'prepare_config'
class Extra(str, Enum):
    allow = 'allow'
    ignore = 'ignore'
    forbid = 'forbid'
# https://github.com/cython/cython/issues/4003
# Fixed in Cython 3 and Pydantic v1 won't support Cython 3.
# Pydantic v2 doesn't depend on Cython at all.
if not compiled:
        title: Optional[str]
        anystr_lower: bool
        anystr_strip_whitespace: bool
        min_anystr_length: int
        max_anystr_length: Optional[int]
        validate_all: bool
        extra: Extra
        allow_mutation: bool
        fields: Dict[str, Union[str, Dict[str, str]]]
        error_msg_templates: Dict[str, str]
        orm_mode: bool
        getter_dict: Type[GetterDict]
        alias_generator: Optional[Callable[[str], str]]
        keep_untouched: Tuple[type, ...]
        schema_extra: Union[Dict[str, object], 'SchemaExtraCallable']
        json_loads: Callable[[str], object]
        json_dumps: AnyArgTCallable[str]
        json_encoders: Dict[Type[object], AnyCallable]
        underscore_attrs_are_private: bool
        copy_on_model_validation: Literal['none', 'deep', 'shallow']
        # whether dataclass `__post_init__` should be run after validation
        post_init_call: Literal['before_validation', 'after_validation']
    ConfigDict = dict  # type: ignore
class BaseConfig:
    anystr_lower: bool = False
    anystr_upper: bool = False
    anystr_strip_whitespace: bool = False
    min_anystr_length: int = 0
    max_anystr_length: Optional[int] = None
    validate_all: bool = False
    extra: Extra = Extra.ignore
    allow_mutation: bool = True
    frozen: bool = False
    allow_population_by_field_name: bool = False
    use_enum_values: bool = False
    fields: Dict[str, Union[str, Dict[str, str]]] = {}
    validate_assignment: bool = False
    error_msg_templates: Dict[str, str] = {}
    arbitrary_types_allowed: bool = False
    orm_mode: bool = False
    getter_dict: Type[GetterDict] = GetterDict
    alias_generator: Optional[Callable[[str], str]] = None
    keep_untouched: Tuple[type, ...] = ()
    schema_extra: Union[Dict[str, Any], 'SchemaExtraCallable'] = {}
    json_loads: Callable[[str], Any] = json.loads
    json_dumps: Callable[..., str] = json.dumps
    json_encoders: Dict[Union[Type[Any], str, ForwardRef], AnyCallable] = {}
    underscore_attrs_are_private: bool = False
    allow_inf_nan: bool = True
    # whether inherited models as fields should be reconstructed as base model,
    # and whether such a copy should be shallow or deep
    copy_on_model_validation: Literal['none', 'deep', 'shallow'] = 'shallow'
    # whether `Union` should check all allowed types before even trying to coerce
    smart_union: bool = False
    # whether dataclass `__post_init__` should be run before or after validation
    post_init_call: Literal['before_validation', 'after_validation'] = 'before_validation'
    def get_field_info(cls, name: str) -> Dict[str, Any]:
        Get properties of FieldInfo from the `fields` property of the config class.
        fields_value = cls.fields.get(name)
        if isinstance(fields_value, str):
            field_info: Dict[str, Any] = {'alias': fields_value}
        elif isinstance(fields_value, dict):
            field_info = fields_value
            field_info = {}
        if 'alias' in field_info:
            field_info.setdefault('alias_priority', 2)
        if field_info.get('alias_priority', 0) <= 1 and cls.alias_generator:
            alias = cls.alias_generator(name)
            if not isinstance(alias, str):
                raise TypeError(f'Config.alias_generator must return str, not {alias.__class__}')
            field_info.update(alias=alias, alias_priority=1)
        return field_info
    def prepare_field(cls, field: 'ModelField') -> None:
        Optional hook to check or modify fields during model creation.
def get_config(config: Union[ConfigDict, Type[object], None]) -> Type[BaseConfig]:
        return BaseConfig
        config_dict = (
            if isinstance(config, dict)
            else {k: getattr(config, k) for k in dir(config) if not k.startswith('__')}
        class Config(BaseConfig):
        for k, v in config_dict.items():
            setattr(Config, k, v)
        return Config
def inherit_config(self_config: 'ConfigType', parent_config: 'ConfigType', **namespace: Any) -> 'ConfigType':
    if not self_config:
        base_classes: Tuple['ConfigType', ...] = (parent_config,)
    elif self_config == parent_config:
        base_classes = (self_config,)
        base_classes = self_config, parent_config
    namespace['json_encoders'] = {
        **getattr(parent_config, 'json_encoders', {}),
        **getattr(self_config, 'json_encoders', {}),
        **namespace.get('json_encoders', {}),
    return type('Config', base_classes, namespace)
def prepare_config(config: Type[BaseConfig], cls_name: str) -> None:
    if not isinstance(config.extra, Extra):
            config.extra = Extra(config.extra)
            raise ValueError(f'"{cls_name}": {config.extra} is not a valid value for "extra"')
"""Parser for reading and writing configuration files."""
__all__ = ["GitConfigParser", "SectionConstraint"]
import configparser as cp
from io import BufferedReader, IOBase
from git.compat import defenc, force_text
from git.util import LockFile
# typing-------------------------------------------------------
from git.types import Lit_config_levels, ConfigLevels_Tup, PathLike, assert_never, _T
    from git.repo.base import Repo
T_ConfigParser = TypeVar("T_ConfigParser", bound="GitConfigParser")
T_OMD_value = TypeVar("T_OMD_value", str, bytes, int, float, bool)
if sys.version_info[:3] < (3, 7, 2):
    # typing.Ordereddict not added until Python 3.7.2.
    OrderedDict_OMD = OrderedDict
    from typing import OrderedDict
    OrderedDict_OMD = OrderedDict[str, List[T_OMD_value]]  # type: ignore[assignment, misc]
# -------------------------------------------------------------
CONFIG_LEVELS: ConfigLevels_Tup = ("system", "user", "global", "repository")
"""The configuration level of a configuration file."""
CONDITIONAL_INCLUDE_REGEXP = re.compile(r"(?<=includeIf )\"(gitdir|gitdir/i|onbranch|hasconfig:remote\.\*\.url):(.+)\"")
"""Section pattern to detect conditional includes.
See: https://git-scm.com/docs/git-config#_conditional_includes
class MetaParserBuilder(abc.ABCMeta):  # noqa: B024
    """Utility class wrapping base-class methods into decorators that assure read-only
    properties."""
    def __new__(cls, name: str, bases: Tuple, clsdict: Dict[str, Any]) -> "MetaParserBuilder":
        """Equip all base-class methods with a needs_values decorator, and all non-const
        methods with a :func:`set_dirty_and_flush_changes` decorator in addition to
        that.
        kmm = "_mutating_methods_"
        if kmm in clsdict:
            mutating_methods = clsdict[kmm]
                methods = (t for t in inspect.getmembers(base, inspect.isroutine) if not t[0].startswith("_"))
                for method_name, method in methods:
                    if method_name in clsdict:
                    method_with_values = needs_values(method)
                    if method_name in mutating_methods:
                        method_with_values = set_dirty_and_flush_changes(method_with_values)
                    # END mutating methods handling
                    clsdict[method_name] = method_with_values
                # END for each name/method pair
            # END for each base
        # END if mutating methods configuration is set
        new_type = super().__new__(cls, name, bases, clsdict)
def needs_values(func: Callable[..., _T]) -> Callable[..., _T]:
    """Return a method for ensuring we read values (on demand) before we try to access
    them."""
    def assure_data_present(self: "GitConfigParser", *args: Any, **kwargs: Any) -> _T:
    # END wrapper method
    return assure_data_present
def set_dirty_and_flush_changes(non_const_func: Callable[..., _T]) -> Callable[..., _T]:
    """Return a method that checks whether given non constant function may be called.
    If so, the instance will be set dirty. Additionally, we flush the changes right to
    disk.
    def flush_changes(self: "GitConfigParser", *args: Any, **kwargs: Any) -> _T:
        rval = non_const_func(self, *args, **kwargs)
        self._dirty = True
        self.write()
        return rval
    flush_changes.__name__ = non_const_func.__name__
    return flush_changes
class SectionConstraint(Generic[T_ConfigParser]):
    """Constrains a ConfigParser to only option commands which are constrained to
    always use the section we have been initialized with.
    It supports all ConfigParser methods that operate on an option.
        If used as a context manager, will release the wrapped ConfigParser.
    __slots__ = ("_config", "_section_name")
    _valid_attrs_ = (
        "get_value",
        "set_value",
        "getint",
        "getfloat",
        "getboolean",
        "has_option",
        "remove_section",
        "remove_option",
    def __init__(self, config: T_ConfigParser, section: str) -> None:
        self._config = config
        self._section_name = section
        # Yes, for some reason, we have to call it explicitly for it to work in PY3 !
        # Apparently __del__ doesn't get call anymore if refcount becomes 0
        # Ridiculous ... .
        self._config.release()
    def __getattr__(self, attr: str) -> Any:
        if attr in self._valid_attrs_:
            return lambda *args, **kwargs: self._call_config(attr, *args, **kwargs)
    def _call_config(self, method: str, *args: Any, **kwargs: Any) -> Any:
        """Call the configuration at the given method which must take a section name as
        first argument."""
        return getattr(self._config, method)(self._section_name, *args, **kwargs)
    def config(self) -> T_ConfigParser:
        """return: ConfigParser instance we constrain"""
        return self._config
        """Equivalent to :meth:`GitConfigParser.release`, which is called on our
        underlying parser instance."""
        return self._config.release()
    def __enter__(self) -> "SectionConstraint[T_ConfigParser]":
        self._config.__enter__()
    def __exit__(self, exception_type: str, exception_value: str, traceback: str) -> None:
        self._config.__exit__(exception_type, exception_value, traceback)
class _OMD(OrderedDict_OMD):
    """Ordered multi-dict."""
    def __setitem__(self, key: str, value: _T) -> None:
        super().__setitem__(key, [value])
    def add(self, key: str, value: Any) -> None:
        super().__getitem__(key).append(value)
    def setall(self, key: str, values: List[_T]) -> None:
        super().__setitem__(key, values)
        return super().__getitem__(key)[-1]
    def getlast(self, key: str) -> Any:
    def setlast(self, key: str, value: Any) -> None:
        prior = super().__getitem__(key)
        prior[-1] = value
    def get(self, key: str, default: Union[_T, None] = None) -> Union[_T, None]:
        return super().get(key, [default])[-1]
    def getall(self, key: str) -> List[_T]:
    def items(self) -> List[Tuple[str, _T]]:  # type: ignore[override]
        """List of (key, last value for key)."""
        return [(k, self[k]) for k in self]
    def items_all(self) -> List[Tuple[str, List[_T]]]:
        """List of (key, list of values for key)."""
        return [(k, self.getall(k)) for k in self]
def get_config_path(config_level: Lit_config_levels) -> str:
    # We do not support an absolute path of the gitconfig on Windows.
    # Use the global config instead.
    if sys.platform == "win32" and config_level == "system":
        config_level = "global"
    if config_level == "system":
        return "/etc/gitconfig"
    elif config_level == "user":
        config_home = os.environ.get("XDG_CONFIG_HOME") or osp.join(os.environ.get("HOME", "~"), ".config")
        return osp.normpath(osp.expanduser(osp.join(config_home, "git", "config")))
    elif config_level == "global":
        return osp.normpath(osp.expanduser("~/.gitconfig"))
    elif config_level == "repository":
        raise ValueError("No repo to get repository configuration from. Use Repo._get_config_path")
        # Should not reach here. Will raise ValueError if does. Static typing will warn
        # about missing elifs.
        assert_never(  # type: ignore[unreachable]
            config_level,
            ValueError(f"Invalid configuration level: {config_level!r}"),
class GitConfigParser(cp.RawConfigParser, metaclass=MetaParserBuilder):
    """Implements specifics required to read git style configuration files.
    This variation behaves much like the :manpage:`git-config(1)` command, such that the
    configuration will be read on demand based on the filepath given during
    initialization.
    The changes will automatically be written once the instance goes out of scope, but
    can be triggered manually as well.
    The configuration file will be locked if you intend to change values preventing
    other instances to write concurrently.
        The config is case-sensitive even when queried, hence section and option names
        must match perfectly.
        If used as a context manager, this will release the locked file.
    # { Configuration
    t_lock = LockFile
    """The lock type determines the type of lock to use in new configuration readers.
    They must be compatible to the :class:`~git.util.LockFile` interface.
    A suitable alternative would be the :class:`~git.util.BlockingLockFile`.
    re_comment = re.compile(r"^\s*[#;]")
    # } END configuration
    optvalueonly_source = r"\s*(?P<option>[^:=\s][^:=]*)"
    OPTVALUEONLY = re.compile(optvalueonly_source)
    OPTCRE = re.compile(optvalueonly_source + r"\s*(?P<vi>[:=])\s*" + r"(?P<value>.*)$")
    del optvalueonly_source
    _mutating_methods_ = ("add_section", "remove_section", "remove_option", "set")
    """Names of :class:`~configparser.RawConfigParser` methods able to change the
    instance."""
        file_or_files: Union[None, PathLike, "BytesIO", Sequence[Union[PathLike, "BytesIO"]]] = None,
        read_only: bool = True,
        merge_includes: bool = True,
        config_level: Union[Lit_config_levels, None] = None,
        repo: Union["Repo", None] = None,
        """Initialize a configuration reader to read the given `file_or_files` and to
        possibly allow changes to it by setting `read_only` False.
        :param file_or_files:
            A file path or file object, or a sequence of possibly more than one of them.
        :param read_only:
            If ``True``, the ConfigParser may only read the data, but not change it.
            If ``False``, only a single file path or file object may be given. We will
            write back the changes when they happen, or when the ConfigParser is
            released. This will not happen if other configuration files have been
        :param merge_includes:
            If ``True``, we will read files mentioned in ``[include]`` sections and
            merge their contents into ours. This makes it impossible to write back an
            individual configuration file. Thus, if you want to modify a single
            configuration file, turn this off to leave the original dataset unaltered
            when reading it.
        :param repo:
            Reference to repository to use if ``[includeIf]`` sections are found in
            configuration files.
        cp.RawConfigParser.__init__(self, dict_type=_OMD)
        self._dict: Callable[..., _OMD]
        self._defaults: _OMD
        self._sections: _OMD
        # Used in Python 3. Needs to stay in sync with sections for underlying
        # implementation to work.
        if not hasattr(self, "_proxies"):
            self._proxies = self._dict()
        if file_or_files is not None:
            self._file_or_files: Union[PathLike, "BytesIO", Sequence[Union[PathLike, "BytesIO"]]] = file_or_files
            if config_level is None:
                if read_only:
                    self._file_or_files = [
                        get_config_path(cast(Lit_config_levels, f)) for f in CONFIG_LEVELS if f != "repository"
                    raise ValueError("No configuration level or configuration files specified")
                self._file_or_files = [get_config_path(config_level)]
        self._read_only = read_only
        self._dirty = False
        self._is_initialized = False
        self._merge_includes = merge_includes
        self._repo = repo
        self._lock: Union["LockFile", None] = None
        self._acquire_lock()
    def _acquire_lock(self) -> None:
        if not self._read_only:
            if not self._lock:
                if isinstance(self._file_or_files, (str, os.PathLike)):
                    file_or_files = self._file_or_files
                elif isinstance(self._file_or_files, (tuple, list, Sequence)):
                        "Write-ConfigParsers can operate on a single file only, multiple files have been passed"
                    file_or_files = self._file_or_files.name
                # END get filename from handle/stream
                # Initialize lock base - we want to write.
                self._lock = self.t_lock(file_or_files)
            # END lock check
            self._lock._obtain_lock()
        # END read-only check
        """Write pending changes if required and release locks."""
        # NOTE: Only consistent in Python 2.
    def __enter__(self) -> "GitConfigParser":
    def __exit__(self, *args: Any) -> None:
        """Flush changes and release the configuration write lock. This instance must
        not be used anymore afterwards.
        In Python 3, it's required to explicitly release locks and flush changes, as
        ``__del__`` is not called deterministically anymore.
        # Checking for the lock here makes sure we do not raise during write()
        # in case an invalid parser was created who could not get a lock.
        if self.read_only or (self._lock and not self._lock._has_lock()):
        except IOError:
            _logger.error("Exception during destruction of GitConfigParser", exc_info=True)
            # This happens in Python 3... and usually means that some state cannot be
            # written as the sections dict cannot be iterated. This usually happens when
            # the interpreter is shutting down. Can it be fixed?
            if self._lock is not None:
                self._lock._release_lock()
    def optionxform(self, optionstr: str) -> str:
        """Do not transform options in any way when writing."""
        return optionstr
    def _read(self, fp: Union[BufferedReader, IO[bytes]], fpname: str) -> None:
        """Originally a direct copy of the Python 2.4 version of
        :meth:`RawConfigParser._read <configparser.RawConfigParser._read>`, to ensure it
        uses ordered dicts.
        The ordering bug was fixed in Python 2.4, and dict itself keeps ordering since
        Python 3.7. This has some other changes, especially that it ignores initial
        whitespace, since git uses tabs. (Big comments are removed to be more compact.)
        cursect = None  # None, or a dictionary.
        optname = None
        lineno = 0
        is_multi_line = False
        e = None  # None, or an exception.
        def string_decode(v: str) -> str:
            if v and v.endswith("\\"):
                v = v[:-1]
            # END cut trailing escapes to prevent decode error
            return v.encode(defenc).decode("unicode_escape")
        # END string_decode
            # We assume to read binary!
            line = fp.readline().decode(defenc)
            lineno = lineno + 1
            # Comment or blank line?
            if line.strip() == "" or self.re_comment.match(line):
            if line.split(None, 1)[0].lower() == "rem" and line[0] in "rR":
                # No leading whitespace.
            # Is it a section header?
            mo = self.SECTCRE.match(line.strip())
            if not is_multi_line and mo:
                sectname: str = mo.group("header").strip()
                if sectname in self._sections:
                    cursect = self._sections[sectname]
                elif sectname == cp.DEFAULTSECT:
                    cursect = self._defaults
                    cursect = self._dict((("__name__", sectname),))
                    self._sections[sectname] = cursect
                    self._proxies[sectname] = None
                # So sections can't start with a continuation line.
            # No section header in the file?
            elif cursect is None:
                raise cp.MissingSectionHeaderError(fpname, lineno, line)
            # An option line?
            elif not is_multi_line:
                mo = self.OPTCRE.match(line)
                if mo:
                    # We might just have handled the last line, which could contain a quotation we want to remove.
                    optname, vi, optval = mo.group("option", "vi", "value")
                    optname = self.optionxform(optname.rstrip())
                    if vi in ("=", ":") and ";" in optval and not optval.strip().startswith('"'):
                        pos = optval.find(";")
                        if pos != -1 and optval[pos - 1].isspace():
                            optval = optval[:pos]
                    optval = optval.strip()
                    if len(optval) < 2 or optval[0] != '"':
                        # Does not open quoting.
                    elif optval[-1] != '"':
                        # Opens quoting and does not close: appears to start multi-line quoting.
                        is_multi_line = True
                        optval = string_decode(optval[1:])
                    elif optval.find("\\", 1, -1) == -1 and optval.find('"', 1, -1) == -1:
                        # Opens and closes quoting. Single line, and all we need is quote removal.
                        optval = optval[1:-1]
                    # TODO: Handle other quoted content, especially well-formed backslash escapes.
                    # Preserves multiple values for duplicate optnames.
                    cursect.add(optname, optval)
                    # Check if it's an option with no value - it's just ignored by git.
                    if not self.OPTVALUEONLY.match(line):
                        if not e:
                            e = cp.ParsingError(fpname)
                        e.append(lineno, repr(line))
                if line.endswith('"'):
                    line = line[:-1]
                # END handle quotations
                optval = cursect.getlast(optname)
                cursect.setlast(optname, optval + string_decode(line))
            # END parse section or option
        # END while reading
        # If any parsing errors occurred, raise an exception.
        if e:
    def _has_includes(self) -> Union[bool, int]:
        return self._merge_includes and len(self._included_paths())
    def _included_paths(self) -> List[Tuple[str, str]]:
        """List all paths that must be included to configuration.
            The list of paths, where each path is a tuple of (option, value).
        for section in self.sections():
            if section == "include":
                paths += self.items(section)
            match = CONDITIONAL_INCLUDE_REGEXP.search(section)
            if match is None or self._repo is None:
            keyword = match.group(1)
            value = match.group(2).strip()
            if keyword in ["gitdir", "gitdir/i"]:
                value = osp.expanduser(value)
                if not any(value.startswith(s) for s in ["./", "/"]):
                    value = "**/" + value
                if value.endswith("/"):
                    value += "**"
                # Ensure that glob is always case insensitive if required.
                if keyword.endswith("/i"):
                    value = re.sub(
                        r"[a-zA-Z]",
                        lambda m: f"[{m.group().lower()!r}{m.group().upper()!r}]",
                if self._repo.git_dir:
                    if fnmatch.fnmatchcase(os.fspath(self._repo.git_dir), value):
            elif keyword == "onbranch":
                    branch_name = self._repo.active_branch.name
                    # Ignore section if active branch cannot be retrieved.
                if fnmatch.fnmatchcase(branch_name, value):
            elif keyword == "hasconfig:remote.*.url":
                for remote in self._repo.remotes:
                    if fnmatch.fnmatchcase(remote.url, value):
    def read(self) -> None:  # type: ignore[override]
        """Read the data stored in the files we have been initialized with.
        This will ignore files that cannot be read, possibly leaving an empty
        :raise IOError:
            If a file cannot be handled.
        if self._is_initialized:
        self._is_initialized = True
        files_to_read: List[Union[PathLike, IO]] = [""]
            # For str or Path, as str is a type of Sequence.
            files_to_read = [self._file_or_files]
        elif not isinstance(self._file_or_files, (tuple, list, Sequence)):
            # Could merge with above isinstance once runtime type known.
        else:  # For lists or tuples.
            files_to_read = list(self._file_or_files)
        # END ensure we have a copy of the paths to handle
        seen = set(files_to_read)
        num_read_include_files = 0
        while files_to_read:
            file_path = files_to_read.pop(0)
            file_ok = False
            if hasattr(file_path, "seek"):
                # Must be a file-object.
                # TODO: Replace cast with assert to narrow type, once sure.
                file_path = cast(IO[bytes], file_path)
                self._read(file_path, file_path.name)
                    with open(file_path, "rb") as fp:
                        file_ok = True
                        self._read(fp, fp.name)
            # Read includes and append those that we didn't handle yet. We expect all
            # paths to be normalized and absolute (and will ensure that is the case).
            if self._has_includes():
                for _, include_path in self._included_paths():
                    if include_path.startswith("~"):
                        include_path = osp.expanduser(include_path)
                    if not osp.isabs(include_path):
                        if not file_ok:
                        # END ignore relative paths if we don't know the configuration file path
                        file_path = cast(PathLike, file_path)
                        assert osp.isabs(file_path), "Need absolute paths to be sure our cycle checks will work"
                        include_path = osp.join(osp.dirname(file_path), include_path)
                    # END make include path absolute
                    include_path = osp.normpath(include_path)
                    if include_path in seen or not os.access(include_path, os.R_OK):
                    seen.add(include_path)
                    # Insert included file to the top to be considered first.
                    files_to_read.insert(0, include_path)
                    num_read_include_files += 1
                # END each include path in configuration file
            # END handle includes
        # END for each file object to read
        # If there was no file included, we can safely write back (potentially) the
        # configuration file without altering its meaning.
        if num_read_include_files == 0:
            self._merge_includes = False
    def _write(self, fp: IO) -> None:
        """Write an .ini-format representation of the configuration state in
        git compatible format."""
        def write_section(name: str, section_dict: _OMD) -> None:
            fp.write(("[%s]\n" % name).encode(defenc))
            values: Sequence[str]  # Runtime only gets str in tests, but should be whatever _OMD stores.
            v: str
            for key, values in section_dict.items_all():
                if key == "__name__":
                    fp.write(("\t%s = %s\n" % (key, self._value_to_string(v).replace("\n", "\n\t"))).encode(defenc))
                # END if key is not __name__
        # END section writing
        if self._defaults:
            write_section(cp.DEFAULTSECT, self._defaults)
        value: _OMD
        for name, value in self._sections.items():
            write_section(name, value)
    def items(self, section_name: str) -> List[Tuple[str, str]]:  # type: ignore[override]
        """:return: list((option, value), ...) pairs of all items in the given section"""
        return [(k, v) for k, v in super().items(section_name) if k != "__name__"]
    def items_all(self, section_name: str) -> List[Tuple[str, List[str]]]:
        """:return: list((option, [values...]), ...) pairs of all items in the given section"""
        rv = _OMD(self._defaults)
        for k, vs in self._sections[section_name].items_all():
            if k == "__name__":
            if k in rv and rv.getall(k) == vs:
                rv.add(k, v)
        return rv.items_all()
    @needs_values
    def write(self) -> None:
        """Write changes to our file, if there are changes at all.
            If this is a read-only writer instance or if we could not obtain a file
        self._assure_writable("write")
        if not self._dirty:
        if isinstance(self._file_or_files, (list, tuple)):
                "Cannot write back if there is not exactly a single file to write to, have %i files"
                % len(self._file_or_files)
        # END assert multiple files
                "Skipping write-back of configuration file as include files were merged in."
                + "Set merge_includes=False to prevent this."
        # END stop if we have include files
        fp = self._file_or_files
        # We have a physical file on disk, so get a lock.
        is_file_lock = isinstance(fp, (str, os.PathLike, IOBase))  # TODO: Use PathLike (having dropped 3.5).
        if is_file_lock and self._lock is not None:  # Else raise error?
        if not hasattr(fp, "seek"):
            fp = cast(PathLike, fp)
            with open(fp, "wb") as fp_open:
                self._write(fp_open)
            fp = cast("BytesIO", fp)
            # Make sure we do not overwrite into an existing file.
            if hasattr(fp, "truncate"):
                fp.truncate()
            self._write(fp)
    def _assure_writable(self, method_name: str) -> None:
        if self.read_only:
            raise IOError("Cannot execute non-constant method %s.%s" % (self, method_name))
    def add_section(self, section: "cp._SectionName") -> None:
        """Assures added options will stay in order."""
        return super().add_section(section)
    def read_only(self) -> bool:
        """:return: ``True`` if this instance may change the configuration file"""
        return self._read_only
    # FIXME: Figure out if default or return type can really include bool.
    def get_value(
        section: str,
        option: str,
        default: Union[int, float, str, bool, None] = None,
    ) -> Union[int, float, str, bool]:
        """Get an option's value.
        If multiple values are specified for this option in the section, the last one
        specified is returned.
            If not ``None``, the given default value will be returned in case the option
            did not exist.
            A properly typed value, either int, float or string
        :raise TypeError:
            In case the value could not be understood.
            Otherwise the exceptions known to the ConfigParser will be raised.
            valuestr = self.get(section, option)
            if default is not None:
        return self._string_to_value(valuestr)
    def get_values(
    ) -> List[Union[int, float, str, bool]]:
        """Get an option's values.
        If multiple values are specified for this option in the section, all are
            If not ``None``, a list containing the given default value will be returned
            in case the option did not exist.
            A list of properly typed values, either int, float or string
            self.sections()
            lst = self._sections[section].getall(option)
                return [default]
        return [self._string_to_value(valuestr) for valuestr in lst]
    def _string_to_value(self, valuestr: str) -> Union[int, float, str, bool]:
        types = (int, float)
        for numtype in types:
                val = numtype(valuestr)
                # truncated value ?
                if val != float(valuestr):
        # END for each numeric type
        # Try boolean values as git uses them.
        vl = valuestr.lower()
        if vl == "false":
        if vl == "true":
        if not isinstance(valuestr, str):
                "Invalid value type: only int, long, float and str are allowed",
                valuestr,
        return valuestr
    def _value_to_string(self, value: Union[str, bytes, int, float, bool]) -> str:
        if isinstance(value, (int, float, bool)):
        return force_text(value)
    @set_dirty_and_flush_changes
    def set_value(self, section: str, option: str, value: Union[str, bytes, int, float, bool]) -> "GitConfigParser":
        """Set the given option in section to the given value.
        This will create the section if required, and will not throw as opposed to the
        default ConfigParser ``set`` method.
        :param section:
            Name of the section in which the option resides or should reside.
        :param option:
            Name of the options whose value to set.
            Value to set the option to. It must be a string or convertible to a string.
            This instance
        if not self.has_section(section):
            self.add_section(section)
        self.set(section, option, self._value_to_string(value))
    def add_value(self, section: str, option: str, value: Union[str, bytes, int, float, bool]) -> "GitConfigParser":
        """Add a value for the given option in section.
        default ConfigParser ``set`` method. The value becomes the new value of the
        option as returned by :meth:`get_value`, and appends to the list of values
        returned by :meth:`get_values`.
            Name of the option.
            Value to add to option. It must be a string or convertible to a string.
        self._sections[section].add(option, self._value_to_string(value))
    def rename_section(self, section: str, new_name: str) -> "GitConfigParser":
        """Rename the given section to `new_name`.
        :raise ValueError:
            If:
            * `section` doesn't exist.
            * A section with `new_name` does already exist.
            raise ValueError("Source section '%s' doesn't exist" % section)
        if self.has_section(new_name):
            raise ValueError("Destination section '%s' already exists" % new_name)
        super().add_section(new_name)
        new_section = self._sections[new_name]
        for k, vs in self.items_all(section):
            new_section.setall(k, vs)
        # END for each value to copy
        # This call writes back the changes, which is why we don't have the respective
        # decorator.
        self.remove_section(section)
Pydantic AI provider configuration.
class PydanticAIProviderConfig(BaseA2AProviderConfig):
    Provider configuration for Pydantic AI agents.
    This config provides fake streaming by converting non-streaming responses into streaming chunks.
        """Handle non-streaming request to Pydantic AI agent."""
        return await PydanticAIHandler.handle_non_streaming(
            request_id=request_id,
            timeout=kwargs.get("timeout", 60.0),
        """Handle streaming request with fake streaming."""
        async for chunk in PydanticAIHandler.handle_streaming(
            chunk_size=kwargs.get("chunk_size", 50),
            delay_ms=kwargs.get("delay_ms", 10),
