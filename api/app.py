"""Jupyter notebook application."""
import typing as t
from jupyter_client.utils import ensure_async  # type:ignore[attr-defined]
from jupyter_core.application import base_aliases
from jupyter_core.paths import jupyter_config_dir
from jupyter_server.base.handlers import JupyterHandler
from jupyter_server.extension.handler import (
    ExtensionHandlerJinjaMixin,
    ExtensionHandlerMixin,
from jupyter_server.serverapp import flags
from jupyter_server.utils import url_escape, url_is_absolute
from jupyter_server.utils import url_path_join as ujoin
from jupyterlab.commands import (  # type:ignore[import-untyped]
    get_app_dir,
    get_user_settings_dir,
    get_workspaces_dir,
from jupyterlab_server import LabServerApp
from jupyterlab_server.config import (  # type:ignore[attr-defined]
    LabConfig,
    get_page_config,
    recursive_update,
from jupyterlab_server.handlers import _camelCase, is_url
from notebook_shim.shim import NotebookConfigShimMixin  # type:ignore[import-untyped]
from tornado import web
from traitlets import Bool, Unicode, default
from traitlets.config.loader import Config
from ._version import __version__
HERE = Path(__file__).parent.resolve()
Flags = dict[str | tuple[str, ...], tuple[dict[str, t.Any] | Config, str]]
app_dir = Path(get_app_dir())
# mypy: disable-error-code="no-untyped-call"
class NotebookBaseHandler(ExtensionHandlerJinjaMixin, ExtensionHandlerMixin, JupyterHandler):
    """The base notebook API handler."""
    def custom_css(self) -> t.Any:
        return self.settings.get("custom_css", True)
    def get_page_config(self) -> dict[str, t.Any]:
        """Get the page config."""
        config = LabConfig()
        app: JupyterNotebookApp = self.extensionapp  # type:ignore[assignment]
        base_url = self.settings.get("base_url", "/")
        page_config_data = self.settings.setdefault("page_config_data", {})
        page_config = {
            **page_config_data,
            "appVersion": version,
            "baseUrl": self.base_url,
            "terminalsAvailable": self.settings.get("terminals_available", False),
            "token": self.settings["token"],
            "fullStaticUrl": ujoin(self.base_url, "static", self.name),
            "frontendUrl": ujoin(self.base_url, "/"),
            "exposeAppInBrowser": app.expose_app_in_browser,
        server_root = self.settings.get("server_root_dir", "")
        server_root = server_root.replace(os.sep, "/")
        server_root = os.path.normpath(Path(server_root).expanduser())
            # Remove the server_root from pref dir
            if self.serverapp.preferred_dir != server_root:
                page_config["preferredPath"] = "/" + os.path.relpath(
                    self.serverapp.preferred_dir, server_root
                page_config["preferredPath"] = "/"
        mathjax_config = self.settings.get("mathjax_config", "TeX-AMS_HTML-full,Safe")
        # TODO Remove CDN usage.
        mathjax_url = self.settings.get(
            "mathjax_url",
            "https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.7/MathJax.js",
        if not url_is_absolute(mathjax_url) and not mathjax_url.startswith(self.base_url):
            mathjax_url = ujoin(self.base_url, mathjax_url)
        page_config.setdefault("mathjaxConfig", mathjax_config)
        page_config.setdefault("fullMathjaxUrl", mathjax_url)
        page_config.setdefault("jupyterConfigDir", jupyter_config_dir())
        # Put all our config in page_config
        for name in config.trait_names():
            page_config[_camelCase(name)] = getattr(app, name)
        # Add full versions of all the urls
            if not name.endswith("_url"):
            full_name = _camelCase("full_" + name)
            full_url = getattr(app, name)
            if not is_url(full_url):
                # Relative URL will be prefixed with base_url
                full_url = ujoin(base_url, full_url)
            page_config[full_name] = full_url
        labextensions_path = app.extra_labextensions_path + app.labextensions_path
        recursive_update(
            page_config,
            get_page_config(
                labextensions_path,
                logger=self.log,
        # modify page config with custom hook
        page_config_hook = self.settings.get("page_config_hook", None)
        if page_config_hook:
            page_config = page_config_hook(self, page_config)
        return page_config
class TreeHandler(NotebookBaseHandler):
    """A tree page handler."""
    @web.authenticated
    async def get(self, path: str = "") -> None:
        Display appropriate page for given path.
        - A directory listing is shown if path is a directory
        - Redirected to notebook page if path is a notebook
        - Render the raw file if path is any other file
        path = path.strip("/")
        cm = self.contents_manager
        if await ensure_async(cm.dir_exists(path=path)):
            if await ensure_async(cm.is_hidden(path)) and not cm.allow_hidden:
                self.log.info("Refusing to serve hidden directory, via 404 Error")
                raise web.HTTPError(404)
            # Set treePath for routing to the directory
            page_config = self.get_page_config()
            page_config["treePath"] = path
            tpl = self.render_template("tree.html", page_config=page_config)
            return self.write(tpl)
        if await ensure_async(cm.file_exists(path)):
            # it's not a directory, we have redirecting to do
            model = await ensure_async(cm.get(path, content=False))
            if model["type"] == "notebook":
                url = ujoin(self.base_url, "notebooks", url_escape(path))
                # Return raw content if file is not a notebook
                url = ujoin(self.base_url, "files", url_escape(path))
            self.log.debug("Redirecting %s to %s", self.request.path, url)
            self.redirect(url)
class ConsoleHandler(NotebookBaseHandler):
    """A console page handler."""
    def get(self, path: str | None = None) -> t.Any:  # noqa: ARG002
        """Get the console page."""
        tpl = self.render_template("consoles.html", page_config=self.get_page_config())
class TerminalHandler(NotebookBaseHandler):
    """A terminal page handler."""
        """Get the terminal page."""
        tpl = self.render_template("terminals.html", page_config=self.get_page_config())
class FileHandler(NotebookBaseHandler):
    """A file page handler."""
        """Get the file page."""
        tpl = self.render_template("edit.html", page_config=self.get_page_config())
class NotebookHandler(NotebookBaseHandler):
    """A notebook page handler."""
    async def get(self, path: str = "") -> t.Any:
        """Get the notebook page. Redirect if it's a directory."""
            url = ujoin(self.base_url, "tree", url_escape(path))
            self.log.debug("Redirecting %s to %s since path is a directory", self.request.path, url)
        tpl = self.render_template("notebooks.html", page_config=self.get_page_config())
class CustomCssHandler(NotebookBaseHandler):
    """A custom CSS handler."""
    def get(self) -> t.Any:
        """Get the custom css file."""
        self.set_header("Content-Type", "text/css")
        custom_css_file = f"{page_config['jupyterConfigDir']}/custom/custom.css"
        if not Path(custom_css_file).is_file():
            static_path_root = re.match("^(.*?)static", page_config["staticDir"])
            if static_path_root is not None:
                custom_dir = static_path_root.groups()[0]
                custom_css_file = f"{custom_dir}custom/custom.css"
        with Path(custom_css_file).open() as css_f:
            return self.write(css_f.read())
aliases = dict(base_aliases)
class JupyterNotebookApp(NotebookConfigShimMixin, LabServerApp):  # type:ignore[misc]
    """The notebook server extension app."""
    name = "notebook"
    app_name = "Jupyter Notebook"
    description = "Jupyter Notebook - A web-based notebook environment for interactive computing"
    version = version
    app_version = Unicode(version, help="The version of the application.")
    extension_url = "/"
    default_url = Unicode("/tree", config=True, help="The default URL to redirect to from `/`")
    file_url_prefix = "/tree"
    load_other_extensions = True
    app_dir = app_dir
    subcommands: dict[str, t.Any] = {}
    expose_app_in_browser = Bool(
        False,
        config=True,
        help="Whether to expose the global app instance to browser via window.jupyterapp",
    custom_css = Bool(
        True,
        help="""Whether custom CSS is loaded on the page.
        Defaults to True and custom CSS is loaded.
    flags: Flags = flags  # type:ignore[assignment]
    flags["expose-app-in-browser"] = (
        {"JupyterNotebookApp": {"expose_app_in_browser": True}},
        "Expose the global app instance to browser via window.jupyterapp.",
    flags["custom-css"] = (
        {"JupyterNotebookApp": {"custom_css": True}},
        "Load custom CSS in template html files. Default is True",
    @default("static_dir")
    def _default_static_dir(self) -> str:
        return str(HERE / "static")
    @default("templates_dir")
    def _default_templates_dir(self) -> str:
        return str(HERE / "templates")
    @default("app_settings_dir")
    def _default_app_settings_dir(self) -> str:
        return str(app_dir / "settings")
    @default("schemas_dir")
    def _default_schemas_dir(self) -> str:
        return str(app_dir / "schemas")
    @default("themes_dir")
    def _default_themes_dir(self) -> str:
        return str(app_dir / "themes")
    @default("user_settings_dir")
    def _default_user_settings_dir(self) -> str:
        return t.cast(str, get_user_settings_dir())
    @default("workspaces_dir")
    def _default_workspaces_dir(self) -> str:
        return t.cast(str, get_workspaces_dir())
    def _prepare_templates(self) -> None:
        super(LabServerApp, self)._prepare_templates()
        self.jinja2_env.globals.update(custom_css=self.custom_css)  # type:ignore[has-type]
    def server_extension_is_enabled(self, extension: str) -> bool:
        """Check if server extension is enabled."""
        if self.serverapp is None:
            extension_enabled = (
                self.serverapp.extension_manager.extensions[extension].enabled is True
        except (AttributeError, KeyError, TypeError):
            extension_enabled = False
        return extension_enabled
    def initialize_handlers(self) -> None:
        """Initialize handlers."""
        assert self.serverapp is not None  # noqa: S101
        page_config = self.serverapp.web_app.settings.setdefault("page_config_data", {})
        nbclassic_enabled = self.server_extension_is_enabled("nbclassic")
        page_config["nbclassic_enabled"] = nbclassic_enabled
        # If running under JupyterHub, add more metadata.
        if "hub_prefix" in self.serverapp.tornado_settings:
            tornado_settings = self.serverapp.tornado_settings
            hub_prefix = tornado_settings["hub_prefix"]
            page_config["hubPrefix"] = hub_prefix
            page_config["hubHost"] = tornado_settings["hub_host"]
            page_config["hubUser"] = tornado_settings["user"]
            page_config["shareUrl"] = ujoin(hub_prefix, "user-redirect")
            # Assume the server_name property indicates running JupyterHub 1.0.
            if hasattr(self.serverapp, "server_name"):
                page_config["hubServerName"] = self.serverapp.server_name
            # avoid setting API token in page config
            # $JUPYTERHUB_API_TOKEN identifies the server, not the client
            # but at least make sure we don't use the token
            # if the serverapp set one
            page_config["token"] = ""
        self.handlers.append(("/tree(.*)", TreeHandler))
        self.handlers.append(("/notebooks(.*)", NotebookHandler))
        self.handlers.append(("/edit(.*)", FileHandler))
        self.handlers.append(("/consoles/(.*)", ConsoleHandler))
        self.handlers.append(("/terminals/(.*)", TerminalHandler))
        self.handlers.append(("/custom/custom.css", CustomCssHandler))
        super().initialize_handlers()
    def initialize(self, argv: list[str] | None = None) -> None:  # noqa: ARG002
        """Subclass because the ExtensionApp.initialize() method does not take arguments"""
        super().initialize()
main = launch_new_instance = JupyterNotebookApp.launch_instance
class RecursiveItem(BaseModel):
    sub_items: list["RecursiveItem"] = []
class RecursiveSubitemInSubmodel(BaseModel):
    sub_items2: list["RecursiveItemViaSubmodel"] = []
class RecursiveItemViaSubmodel(BaseModel):
    sub_items1: list[RecursiveSubitemInSubmodel] = []
RecursiveItem.model_rebuild()
RecursiveSubitemInSubmodel.model_rebuild()
RecursiveItemViaSubmodel.model_rebuild()
@app.get("/items/recursive", response_model=RecursiveItem)
def get_recursive():
    return {"name": "item", "sub_items": [{"name": "subitem", "sub_items": []}]}
@app.get("/items/recursive-submodel", response_model=RecursiveItemViaSubmodel)
def get_recursive_submodel():
        "name": "item",
        "sub_items1": [
                "name": "subitem",
                "sub_items2": [
                        "name": "subsubitem",
                        "sub_items1": [{"name": "subsubsubitem", "sub_items2": []}],
    from langchain_community.tools import AINAppOps
    from langchain_community.tools.ainetwork.app import AppOperationType, AppSchema
    "AppOperationType": "langchain_community.tools.ainetwork.app",
    "AppSchema": "langchain_community.tools.ainetwork.app",
    "AINAppOps": "langchain_community.tools",
    "AppOperationType",
    "AppSchema",
Filters that accept a `Application` as argument.
from prompt_toolkit.application.current import get_app
from prompt_toolkit.cache import memoized
from prompt_toolkit.enums import EditingMode
from .base import Condition
    from prompt_toolkit.layout.layout import FocusableElement
    "has_suggestion",
# NOTE: `has_focus` below should *not* be `memoized`. It can reference any user
#       control. For instance, if we would continuously create new
#       `PromptSession` instances, then previous instances won't be released,
#       because this memoize (which caches results in the global scope) will
#       still refer to each instance.
def has_focus(value: FocusableElement) -> Condition:
    Enable when this buffer has the focus.
    from prompt_toolkit.buffer import Buffer
    from prompt_toolkit.layout import walk
    from prompt_toolkit.layout.containers import Window, to_container
    from prompt_toolkit.layout.controls import UIControl
        def test() -> bool:
            return get_app().current_buffer.name == value
    elif isinstance(value, Buffer):
            return get_app().current_buffer == value
    elif isinstance(value, UIControl):
            return get_app().layout.current_control == value
        value = to_container(value)
        if isinstance(value, Window):
                return get_app().layout.current_window == value
                # Consider focused when any window inside this container is
                # focused.
                current_window = get_app().layout.current_window
                for c in walk(value):
                    if isinstance(c, Window) and c == current_window:
    @Condition
    def has_focus_filter() -> bool:
        return test()
    return has_focus_filter
def buffer_has_focus() -> bool:
    Enabled when the currently focused control is a `BufferControl`.
    return get_app().layout.buffer_has_focus
def has_selection() -> bool:
    Enable when the current buffer has a selection.
    return bool(get_app().current_buffer.selection_state)
def has_suggestion() -> bool:
    Enable when the current buffer has a suggestion.
    buffer = get_app().current_buffer
    return buffer.suggestion is not None and buffer.suggestion.text != ""
def has_completions() -> bool:
    Enable when the current buffer has completions.
    state = get_app().current_buffer.complete_state
    return state is not None and len(state.completions) > 0
def completion_is_selected() -> bool:
    True when the user selected a completion.
    complete_state = get_app().current_buffer.complete_state
    return complete_state is not None and complete_state.current_completion is not None
def is_read_only() -> bool:
    True when the current buffer is read only.
    return get_app().current_buffer.read_only()
def is_multiline() -> bool:
    True when the current buffer has been marked as multiline.
    return get_app().current_buffer.multiline()
def has_validation_error() -> bool:
    "Current buffer has validation error."
    return get_app().current_buffer.validation_error is not None
def has_arg() -> bool:
    "Enable when the input processor has an 'arg'."
    return get_app().key_processor.arg is not None
def is_done() -> bool:
    True when the CLI is returning, aborting or exiting.
    return get_app().is_done
def renderer_height_is_known() -> bool:
    Only True when the renderer knows it's real height.
    (On VT100 terminals, we have to wait for a CPR response, before we can be
    sure of the available height between the cursor position and the bottom of
    the terminal. And usually it's nicer to wait with drawing bottom toolbars
    until we receive the height, in order to avoid flickering -- first drawing
    somewhere in the middle, and then again at the bottom.)
    return get_app().renderer.height_is_known
@memoized()
def in_editing_mode(editing_mode: EditingMode) -> Condition:
    Check whether a given editing mode is active. (Vi or Emacs.)
    def in_editing_mode_filter() -> bool:
        return get_app().editing_mode == editing_mode
    return in_editing_mode_filter
def in_paste_mode() -> bool:
    return get_app().paste_mode()
def vi_mode() -> bool:
    return get_app().editing_mode == EditingMode.VI
def vi_navigation_mode() -> bool:
    Active when the set for Vi navigation key bindings are active.
    from prompt_toolkit.key_binding.vi_state import InputMode
    app = get_app()
        app.editing_mode != EditingMode.VI
        or app.vi_state.operator_func
        or app.vi_state.waiting_for_digraph
        or app.current_buffer.selection_state
        app.vi_state.input_mode == InputMode.NAVIGATION
        or app.vi_state.temporary_navigation_mode
        or app.current_buffer.read_only()
def vi_insert_mode() -> bool:
    return app.vi_state.input_mode == InputMode.INSERT
def vi_insert_multiple_mode() -> bool:
    return app.vi_state.input_mode == InputMode.INSERT_MULTIPLE
def vi_replace_mode() -> bool:
    return app.vi_state.input_mode == InputMode.REPLACE
def vi_replace_single_mode() -> bool:
    return app.vi_state.input_mode == InputMode.REPLACE_SINGLE
def vi_selection_mode() -> bool:
    if app.editing_mode != EditingMode.VI:
    return bool(app.current_buffer.selection_state)
def vi_waiting_for_text_object_mode() -> bool:
    return app.vi_state.operator_func is not None
def vi_digraph_mode() -> bool:
    return app.vi_state.waiting_for_digraph
def vi_recording_macro() -> bool:
    "When recording a Vi macro."
    return app.vi_state.recording_register is not None
def emacs_mode() -> bool:
    "When the Emacs bindings are active."
    return get_app().editing_mode == EditingMode.EMACS
def emacs_insert_mode() -> bool:
        app.editing_mode != EditingMode.EMACS
def emacs_selection_mode() -> bool:
    return bool(
        app.editing_mode == EditingMode.EMACS and app.current_buffer.selection_state
def shift_selection_mode() -> bool:
        app.current_buffer.selection_state
        and app.current_buffer.selection_state.shift_mode
def is_searching() -> bool:
    "When we are searching."
    return app.layout.is_searching
def control_is_searchable() -> bool:
    "When the current UIControl is searchable."
    from prompt_toolkit.layout.controls import BufferControl
    control = get_app().layout.current_control
        isinstance(control, BufferControl) and control.search_buffer_control is not None
def vi_search_direction_reversed() -> bool:
    "When the '/' and '?' key bindings for Vi-style searching have been reversed."
    return get_app().reverse_vi_search_direction()
