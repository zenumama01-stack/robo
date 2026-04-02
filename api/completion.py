from .completion_usage import CompletionUsage
from .completion_choice import CompletionChoice
__all__ = ["Completion"]
class Completion(BaseModel):
    """Represents a completion response from the API.
    Note: both the streamed and non-streamed response objects share the same shape (unlike the chat endpoint).
    """A unique identifier for the completion."""
    choices: List[CompletionChoice]
    """The list of completion choices the model generated for the input prompt."""
    created: int
    """The Unix timestamp (in seconds) of when the completion was created."""
    """The model used for completion."""
    object: Literal["text_completion"]
    """The object type, which is always "text_completion" """
    system_fingerprint: Optional[str] = None
    """This fingerprint represents the backend configuration that the model runs with.
    Can be used in conjunction with the `seed` request parameter to understand when
    backend changes have been made that might impact determinism.
    usage: Optional[CompletionUsage] = None
    """Usage statistics for the completion request."""
from pip._internal.cli.status_codes import SUCCESS
from pip._internal.utils.misc import get_prog
BASE_COMPLETION = """
# pip {shell} completion start{script}# pip {shell} completion end
COMPLETION_SCRIPTS = {
    "bash": """
        _pip_completion()
            local IFS=$' \\t\\n'
            COMPREPLY=( $( COMP_WORDS="${{COMP_WORDS[*]}}" \\
                           COMP_CWORD=$COMP_CWORD \\
                           PIP_AUTO_COMPLETE=1 "$1" 2>/dev/null ) )
        complete -o default -F _pip_completion {prog}
    "zsh": """
        #compdef -P pip[0-9.]#
        __pip() {{
          compadd $( COMP_WORDS="$words[*]" \\
                     COMP_CWORD=$((CURRENT-1)) \\
                     PIP_AUTO_COMPLETE=1 $words[1] 2>/dev/null )
        if [[ $zsh_eval_context[-1] == loadautofunc ]]; then
          # autoload from fpath, call function directly
          __pip "$@"
          # eval/source/. command, register function for later
          compdef __pip -P 'pip[0-9.]#'
        fi
    "fish": """
        function __fish_complete_pip
            set -lx COMP_WORDS \\
                (commandline --current-process --tokenize --cut-at-cursor) \\
                (commandline --current-token --cut-at-cursor)
            set -lx COMP_CWORD (math (count $COMP_WORDS) - 1)
            set -lx PIP_AUTO_COMPLETE 1
            set -l completions
            if string match -q '2.*' $version
                set completions (eval $COMP_WORDS[1])
                set completions ($COMP_WORDS[1])
            string split \\  -- $completions
        complete -fa "(__fish_complete_pip)" -c {prog}
    "powershell": """
        if ((Test-Path Function:\\TabExpansion) -and -not `
            (Test-Path Function:\\_pip_completeBackup)) {{
            Rename-Item Function:\\TabExpansion _pip_completeBackup
        function TabExpansion($line, $lastWord) {{
            $lastBlock = [regex]::Split($line, '[|;]')[-1].TrimStart()
            if ($lastBlock.StartsWith("{prog} ")) {{
                $Env:COMP_WORDS=$lastBlock
                $Env:COMP_CWORD=$lastBlock.Split().Length - 1
                $Env:PIP_AUTO_COMPLETE=1
                (& {prog}).Split()
                Remove-Item Env:COMP_WORDS
                Remove-Item Env:COMP_CWORD
                Remove-Item Env:PIP_AUTO_COMPLETE
            elseif (Test-Path Function:\\_pip_completeBackup) {{
                # Fall back on existing tab expansion
                _pip_completeBackup $line $lastWord
class CompletionCommand(Command):
    """A helper command to be used for command completion."""
            "--bash",
            "-b",
            action="store_const",
            const="bash",
            dest="shell",
            help="Emit completion code for bash",
            "--zsh",
            "-z",
            const="zsh",
            help="Emit completion code for zsh",
            "--fish",
            const="fish",
            help="Emit completion code for fish",
            "--powershell",
            const="powershell",
            help="Emit completion code for powershell",
        """Prints the completion code of the given shell"""
        shells = COMPLETION_SCRIPTS.keys()
        shell_options = ["--" + shell for shell in sorted(shells)]
        if options.shell in shells:
            script = textwrap.dedent(
                COMPLETION_SCRIPTS.get(options.shell, "").format(prog=get_prog())
            print(BASE_COMPLETION.format(script=script, shell=options.shell))
                "ERROR: You must pass {}\n".format(" or ".join(shell_options))
Completer for a regular grammar.
from prompt_toolkit.completion import CompleteEvent, Completer, Completion
from .compiler import Match, _CompiledGrammar
    "GrammarCompleter",
class GrammarCompleter(Completer):
    Completer which can be used for autocompletion according to variables in
    the grammar. Each variable can have a different autocompleter.
    :param compiled_grammar: `GrammarCompleter` instance.
    :param completers: `dict` mapping variable names of the grammar to the
                       `Completer` instances to be used for each variable.
        self, compiled_grammar: _CompiledGrammar, completers: dict[str, Completer]
        self.compiled_grammar = compiled_grammar
        m = self.compiled_grammar.match_prefix(document.text_before_cursor)
            yield from self._remove_duplicates(
                self._get_completions_for_match(m, complete_event)
    def _get_completions_for_match(
        self, match: Match, complete_event: CompleteEvent
        Yield all the possible completions for this input string.
        (The completer assumes that the cursor position was at the end of the
        input string.)
        for match_variable in match.end_nodes():
            varname = match_variable.varname
            start = match_variable.start
            completer = self.completers.get(varname)
            if completer:
                text = match_variable.value
                # Unwrap text.
                unwrapped_text = self.compiled_grammar.unescape(varname, text)
                # Create a document, for the completions API (text/cursor_position)
                document = Document(unwrapped_text, len(unwrapped_text))
                # Call completer
                for completion in completer.get_completions(document, complete_event):
                    new_text = (
                        unwrapped_text[: len(text) + completion.start_position]
                        + completion.text
                    # Wrap again.
                    yield Completion(
                        text=self.compiled_grammar.escape(varname, new_text),
                        start_position=start - len(match.string),
                        display=completion.display,
                        display_meta=completion.display_meta,
    def _remove_duplicates(self, items: Iterable[Completion]) -> Iterable[Completion]:
        Remove duplicates, while keeping the order.
        (Sometimes we have duplicates, because the there several matches of the
        same grammar, each yielding similar completions.)
        def hash_completion(completion: Completion) -> tuple[str, int]:
            return completion.text, completion.start_position
        yielded_so_far: set[tuple[str, int]] = set()
        for completion in items:
            hash_value = hash_completion(completion)
            if hash_value not in yielded_so_far:
                yielded_so_far.add(hash_value)
Key binding handlers for displaying completions.
from prompt_toolkit.application.run_in_terminal import in_terminal
from prompt_toolkit.completion import (
from prompt_toolkit.formatted_text import StyleAndTextTuples
from prompt_toolkit.key_binding.key_bindings import KeyBindings
from prompt_toolkit.key_binding.key_processor import KeyPressEvent
    from prompt_toolkit.application import Application
    from prompt_toolkit.shortcuts import PromptSession
    "generate_completions",
    "display_completions_like_readline",
def generate_completions(event: E) -> None:
    Tab-completion: where the first tab completes the common suffix and the
    second tab lists all the completions.
    b = event.current_buffer
    # When already navigating through completions, select the next one.
    if b.complete_state:
        b.complete_next()
        b.start_completion(insert_common_part=True)
def display_completions_like_readline(event: E) -> None:
    Key binding handler for readline-style tab completion.
    This is meant to be as similar as possible to the way how readline displays
    completions.
    Generate the completions immediately (blocking) and display them above the
    prompt in columns.
        # Call this handler when 'Tab' has been pressed.
        key_bindings.add(Keys.ControlI)(display_completions_like_readline)
    # Request completions.
    if b.completer is None:
    complete_event = CompleteEvent(completion_requested=True)
    completions = list(b.completer.get_completions(b.document, complete_event))
    # Calculate the common suffix.
    common_suffix = get_common_complete_suffix(b.document, completions)
    # One completion: insert it.
    if len(completions) == 1:
        b.delete_before_cursor(-completions[0].start_position)
        b.insert_text(completions[0].text)
    # Multiple completions with common part.
    elif common_suffix:
        b.insert_text(common_suffix)
    # Otherwise: display all completions.
    elif completions:
        _display_completions_like_readline(event.app, completions)
def _display_completions_like_readline(
    app: Application[object], completions: list[Completion]
) -> asyncio.Task[None]:
    Display the list of completions in columns above the prompt.
    This will ask for a confirmation if there are too many completions to fit
    on a single page and provide a paginator to walk through them.
    from prompt_toolkit.shortcuts.prompt import create_confirm_session
    # Get terminal dimensions.
    term_size = app.output.get_size()
    term_width = term_size.columns
    term_height = term_size.rows
    # Calculate amount of required columns/rows for displaying the
    # completions. (Keep in mind that completions are displayed
    # alphabetically column-wise.)
    max_compl_width = min(
        term_width, max(get_cwidth(c.display_text) for c in completions) + 1
    column_count = max(1, term_width // max_compl_width)
    completions_per_page = column_count * (term_height - 1)
    page_count = int(math.ceil(len(completions) / float(completions_per_page)))
    # Note: math.ceil can return float on Python2.
    def display(page: int) -> None:
        # Display completions.
        page_completions = completions[
            page * completions_per_page : (page + 1) * completions_per_page
        page_row_count = int(math.ceil(len(page_completions) / float(column_count)))
        page_columns = [
            page_completions[i * page_row_count : (i + 1) * page_row_count]
            for i in range(column_count)
        result: StyleAndTextTuples = []
        for r in range(page_row_count):
            for c in range(column_count):
                    completion = page_columns[c][r]
                    style = "class:readline-like-completions.completion " + (
                        completion.style or ""
                    result.extend(to_formatted_text(completion.display, style=style))
                    # Add padding.
                    padding = max_compl_width - get_cwidth(completion.display_text)
                    result.append((completion.style, " " * padding))
            result.append(("", "\n"))
        app.print_text(to_formatted_text(result, "class:readline-like-completions"))
    # User interaction through an application generator function.
    async def run_compl() -> None:
        "Coroutine."
        async with in_terminal(render_cli_done=True):
            if len(completions) > completions_per_page:
                # Ask confirmation if it doesn't fit on the screen.
                confirm = await create_confirm_session(
                    f"Display all {len(completions)} possibilities?",
                ).prompt_async()
                if confirm:
                    # Display pages.
                    for page in range(page_count):
                        display(page)
                        if page != page_count - 1:
                            # Display --MORE-- and go to the next page.
                            show_more = await _create_more_session(
                                "--MORE--"
                            if not show_more:
                    app.output.flush()
                # Display all completions.
                display(0)
    return app.create_background_task(run_compl())
def _create_more_session(message: str = "--MORE--") -> PromptSession[bool]:
    Create a `PromptSession` object for displaying the "--MORE--".
    bindings = KeyBindings()
    @bindings.add(" ")
    @bindings.add("y")
    @bindings.add("Y")
    @bindings.add(Keys.ControlJ)
    @bindings.add(Keys.ControlM)
    @bindings.add(Keys.ControlI)  # Tab.
    def _yes(event: E) -> None:
        event.app.exit(result=True)
    @bindings.add("n")
    @bindings.add("N")
    @bindings.add("q")
    @bindings.add("Q")
    @bindings.add(Keys.ControlC)
    def _no(event: E) -> None:
        event.app.exit(result=False)
    @bindings.add(Keys.Any)
        "Disable inserting of text."
    return PromptSession(message, key_bindings=bindings, erase_when_done=True)
from ._completion_classes import completion_init
from ._completion_shared import Shells, _get_shell_name, get_completion_script, install
from .models import ParamMeta
from .params import Option
from .utils import get_params_from_function
_click_patched = False
def get_completion_inspect_parameters() -> tuple[ParamMeta, ParamMeta]:
    completion_init()
    test_disable_detection = os.getenv("_TYPER_COMPLETE_TEST_DISABLE_SHELL_DETECTION")
    if not test_disable_detection:
        parameters = get_params_from_function(_install_completion_placeholder_function)
        parameters = get_params_from_function(
            _install_completion_no_auto_placeholder_function
    install_param, show_param = parameters.values()
    return install_param, show_param
def install_callback(ctx: click.Context, param: click.Parameter, value: Any) -> Any:
        return value  # pragma: no cover
        shell, path = install(shell=value)
        shell, path = install()
    click.secho(f"{shell} completion installed in {path}", fg="green")
    click.echo("Completion will take effect once you restart the terminal")
def show_callback(ctx: click.Context, param: click.Parameter, value: Any) -> Any:
    prog_name = ctx.find_root().info_name
    assert prog_name
    complete_var = "_{}_COMPLETE".format(prog_name.replace("-", "_").upper())
    shell = ""
        shell = value
    elif not test_disable_detection:
        detected_shell = _get_shell_name()
        if detected_shell is not None:
            shell = detected_shell
    script_content = get_completion_script(
        prog_name=prog_name, complete_var=complete_var, shell=shell
    click.echo(script_content)
# Create a fake command function to extract the completion parameters
def _install_completion_placeholder_function(
    install_completion: bool = Option(
        "--install-completion",
        callback=install_callback,
        help="Install completion for the current shell.",
    show_completion: bool = Option(
        "--show-completion",
        callback=show_callback,
        help="Show completion for the current shell, to copy it or customize the installation.",
def _install_completion_no_auto_placeholder_function(
    install_completion: Shells = Option(
        help="Install completion for the specified shell.",
    show_completion: Shells = Option(
        help="Show completion for the specified shell, to copy it or customize the installation.",
# Re-implement Click's shell_complete to add error message with:
# Invalid completion instruction
# To use 7.x instruction style for compatibility
# And to add extra error messages, for compatibility with Typer in previous versions
# This is only called in new Command method, only used by Click 8.x+
def shell_complete(
    cli: click.Command,
    ctx_args: MutableMapping[str, Any],
    prog_name: str,
    complete_var: str,
    instruction: str,
    import click.shell_completion
    if "_" not in instruction:
        click.echo("Invalid completion instruction.", err=True)
    # Click 8 changed the order/style of shell instructions from e.g.
    # source_bash to bash_source
    # Typer override to preserve the old style for compatibility
    # Original in Click 8.x commented:
    # shell, _, instruction = instruction.partition("_")
    instruction, _, shell = instruction.partition("_")
    # Typer override end
    comp_cls = click.shell_completion.get_completion_class(shell)
    if comp_cls is None:
        click.echo(f"Shell {shell} not supported.", err=True)
    comp = comp_cls(cli, ctx_args, prog_name, complete_var)
    if instruction == "source":
        click.echo(comp.source())
    # Typer override to print the completion help msg with Rich
    if instruction == "complete":
        click.echo(comp.complete())
    click.echo(f'Completion instruction "{instruction}" not supported.', err=True)
from typing import Iterable, List, Optional, Union
    [Vision guide](https://platform.openai.com/docs/guides/vision/low-or-high-fidelity-image-understanding).
ChatCompletionContentPartParam = Union[
    ChatCompletionContentPartTextParam, ChatCompletionContentPartImageParam
class ChatCompletionMessageToolCallParam(TypedDict, total=False):
    content: Optional[str]
    tool_calls: Iterable[ChatCompletionMessageToolCallParam]
ChatCompletionMessageParam = Union[
class CompletionRequest(BaseModel):
    messages: List[ChatCompletionMessageParam] = []
    timeout: Optional[Union[float, int]] = None
    stop: Optional[dict] = None
    logit_bias: Optional[dict] = None
    deployment_id: Optional[str] = None
    functions: Optional[List[str]] = None
    function_call: Optional[str] = None
    base_url: Optional[str] = None
    model_list: Optional[List[str]] = None
