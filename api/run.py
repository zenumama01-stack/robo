from .run_status import RunStatus
from ..assistant_tool import AssistantTool
from ..assistant_tool_choice_option import AssistantToolChoiceOption
from ..assistant_response_format_option import AssistantResponseFormatOption
from .required_action_function_tool_call import RequiredActionFunctionToolCall
    "Run",
    "LastError",
    "RequiredAction",
    "RequiredActionSubmitToolOutputs",
    """Details on why the run is incomplete.
    Will be `null` if the run is not incomplete.
    reason: Optional[Literal["max_completion_tokens", "max_prompt_tokens"]] = None
    """The reason why the run is incomplete.
    This will point to which specific token limit was reached over the course of the
    run.
class LastError(BaseModel):
    """The last error associated with this run. Will be `null` if there are no errors."""
    code: Literal["server_error", "rate_limit_exceeded", "invalid_prompt"]
    """One of `server_error`, `rate_limit_exceeded`, or `invalid_prompt`."""
    """A human-readable description of the error."""
class RequiredActionSubmitToolOutputs(BaseModel):
    """Details on the tool outputs needed for this run to continue."""
    tool_calls: List[RequiredActionFunctionToolCall]
    """A list of the relevant tool calls."""
class RequiredAction(BaseModel):
    """Details on the action required to continue the run.
    Will be `null` if no action is required.
    submit_tool_outputs: RequiredActionSubmitToolOutputs
    type: Literal["submit_tool_outputs"]
    """For now, this is always `submit_tool_outputs`."""
class TruncationStrategy(BaseModel):
    type: Literal["auto", "last_messages"]
    last_messages: Optional[int] = None
    """Usage statistics related to the run.
    This value will be `null` if the run is not in a terminal state (i.e. `in_progress`, `queued`, etc.).
    """Number of completion tokens used over the course of the run."""
    """Number of prompt tokens used over the course of the run."""
    """Total number of tokens used (prompt + completion)."""
class Run(BaseModel):
    Represents an execution run on a [thread](https://platform.openai.com/docs/api-reference/threads).
    assistant_id: str
    [assistant](https://platform.openai.com/docs/api-reference/assistants) used for
    execution of this run.
    """The Unix timestamp (in seconds) for when the run was cancelled."""
    """The Unix timestamp (in seconds) for when the run was completed."""
    """The Unix timestamp (in seconds) for when the run was created."""
    """The Unix timestamp (in seconds) for when the run will expire."""
    """The Unix timestamp (in seconds) for when the run failed."""
    The instructions that the
    this run.
    last_error: Optional[LastError] = None
    max_completion_tokens: Optional[int] = None
    The maximum number of completion tokens specified to have been used over the
    course of the run.
    max_prompt_tokens: Optional[int] = None
    The maximum number of prompt tokens specified to have been used over the course
    of the run.
    The model that the
    object: Literal["thread.run"]
    """The object type, which is always `thread.run`."""
    required_action: Optional[RequiredAction] = None
    started_at: Optional[int] = None
    """The Unix timestamp (in seconds) for when the run was started."""
    status: RunStatus
    The status of the run, which can be either `queued`, `in_progress`,
    `requires_action`, `cancelling`, `cancelled`, `failed`, `completed`,
    `incomplete`, or `expired`.
    The ID of the [thread](https://platform.openai.com/docs/api-reference/threads)
    that was executed on as a part of this run.
    tool_choice: Optional[AssistantToolChoiceOption] = None
    The list of tools that the
    truncation_strategy: Optional[TruncationStrategy] = None
    This value will be `null` if the run is not in a terminal state (i.e.
    `in_progress`, `queued`, etc.).
    """The sampling temperature used for this run. If not set, defaults to 1."""
    """The nucleus sampling value used for this run. If not set, defaults to 1."""
from .exceptions import EOF, TIMEOUT
from .pty_spawn import spawn
def run(command, timeout=30, withexitstatus=False, events=None,
        extra_args=None, logfile=None, cwd=None, env=None, **kwargs):
    This function runs the given command; waits for it to finish; then
    returns all output as a string. STDERR is included in output. If the full
    path to the command is not given then the path is searched.
    Note that lines are terminated by CR/LF (\\r\\n) combination even on
    UNIX-like systems because this is the standard for pseudottys. If you set
    'withexitstatus' to true, then run will return a tuple of (command_output,
    exitstatus). If 'withexitstatus' is false then this returns just
    command_output.
    The run() function can often be used instead of creating a spawn instance.
    For example, the following code uses spawn::
        from pexpect import *
        child = spawn('scp foo user@example.com:.')
        child.expect('(?i)password')
    The previous code can be replace with the following::
        run('scp foo user@example.com:.', events={'(?i)password': mypassword})
    **Examples**
    Start the apache daemon on the local machine::
        run("/usr/local/apache/bin/apachectl start")
    Check in a file using SVN::
        run("svn ci -m 'automatic commit' my_file.py")
    Run a command and capture exit status::
        (command_output, exitstatus) = run('ls -l /bin', withexitstatus=1)
    The following will run SSH and execute 'ls -l' on the remote machine. The
    password 'secret' will be sent if the '(?i)password' pattern is ever seen::
        run("ssh username@machine.example.com 'ls -l'",
            events={'(?i)password':'secret\\n'})
    This will start mencoder to rip a video from DVD. This will also display
    progress ticks every 5 seconds as it runs. For example::
        def print_ticks(d):
            print d['event_count'],
        run("mencoder dvd://1 -o video.avi -oac copy -ovc copy",
            events={TIMEOUT:print_ticks}, timeout=5)
    The 'events' argument should be either a dictionary or a tuple list that
    contains patterns and responses. Whenever one of the patterns is seen
    in the command output, run() will send the associated response string.
    So, run() in the above example can be also written as::
            events=[(TIMEOUT,print_ticks)], timeout=5)
    Use a tuple list for events if the command output requires a delicate
    control over what pattern should be matched, since the tuple list is passed
    to pexpect() as its pattern list, with the order of patterns preserved.
    Note that you should put newlines in your string if Enter is necessary.
    Like the example above, the responses may also contain a callback, either
    a function or method.  It should accept a dictionary value as an argument.
    The dictionary contains all the locals from the run() function, so you can
    access the child spawn object or any other variable defined in run()
    (event_count, child, and extra_args are the most useful). A callback may
    return True to stop the current run process.  Otherwise run() continues
    until the next event. A callback may also return a string which will be
    sent to the child. 'extra_args' is not used by directly run(). It provides
    a way to pass data to a callback function through run() through the locals
    dictionary passed to a callback.
    Like :class:`spawn`, passing *encoding* will make it work with unicode
    instead of bytes. You can pass *codec_errors* to control how errors in
    encoding and decoding are handled.
    if timeout == -1:
        child = spawn(command, maxread=2000, logfile=logfile, cwd=cwd, env=env,
                        **kwargs)
        child = spawn(command, timeout=timeout, maxread=2000, logfile=logfile,
                cwd=cwd, env=env, **kwargs)
    if isinstance(events, list):
        patterns= [x for x,y in events]
        responses = [y for x,y in events]
    elif isinstance(events, dict):
        patterns = list(events.keys())
        responses = list(events.values())
        # This assumes EOF or TIMEOUT will eventually cause run to terminate.
        patterns = None
        responses = None
    child_result_list = []
    event_count = 0
            index = child.expect(patterns)
            if isinstance(child.after, child.allowed_string_types):
                child_result_list.append(child.before + child.after)
                # child.after may have been a TIMEOUT or EOF,
                # which we don't want appended to the list.
                child_result_list.append(child.before)
            if isinstance(responses[index], child.allowed_string_types):
                child.send(responses[index])
            elif (isinstance(responses[index], types.FunctionType) or
                  isinstance(responses[index], types.MethodType)):
                callback_result = responses[index](locals())
                if isinstance(callback_result, child.allowed_string_types):
                    child.send(callback_result)
                elif callback_result:
                raise TypeError("parameter `event' at index {index} must be "
                                "a string, method, or function: {value!r}"
                                .format(index=index, value=responses[index]))
            event_count = event_count + 1
        except TIMEOUT:
        except EOF:
    child_result = child.string_type().join(child_result_list)
    if withexitstatus:
        child.close()
        return (child_result, child.exitstatus)
        return child_result
def runu(command, timeout=30, withexitstatus=False, events=None,
    """Deprecated: pass encoding to run() instead.
    return run(command, timeout=timeout, withexitstatus=withexitstatus,
                events=events, extra_args=extra_args, logfile=logfile, cwd=cwd,
                env=env, **kwargs)
from multiprocessing import get_context
from multiprocessing.context import SpawnProcess
from typing import TYPE_CHECKING, Any, Callable, Dict, Generator, List, Optional, Set, Tuple, Union
from .main import Change, FileChange, awatch, watch
__all__ = 'run_process', 'arun_process', 'detect_target_type', 'import_string'
def run_process(
    target: Union[str, Callable[..., Any]],
    args: Tuple[Any, ...] = (),
    kwargs: Optional[Dict[str, Any]] = None,
    target_type: "Literal['function', 'command', 'auto']" = 'auto',
    callback: Optional[Callable[[Set[FileChange]], None]] = None,
    grace_period: float = 0,
    sigint_timeout: int = 5,
    sigkill_timeout: int = 1,
    ignore_permission_denied: bool = False,
    Run a process and restart it upon file changes.
    `run_process` can work in two ways:
    * Using `multiprocessing.Process` † to run a python function
    * Or, using `subprocess.Popen` to run a command
        **†** technically `multiprocessing.get_context('spawn').Process` to avoid forking and improve
        code reload/import.
    Internally, `run_process` uses [`watch`][watchfiles.watch] with `raise_interrupt=False` so the function
    exits cleanly upon `Ctrl+C`.
        *paths: matches the same argument of [`watch`][watchfiles.watch]
        target: function or command to run
        args: arguments to pass to `target`, only used if `target` is a function
        kwargs: keyword arguments to pass to `target`, only used if `target` is a function
        target_type: type of target. Can be `'function'`, `'command'`, or `'auto'` in which case
            [`detect_target_type`][watchfiles.run.detect_target_type] is used to determine the type.
        callback: function to call on each reload, the function should accept a set of changes as the sole argument
        watch_filter: matches the same argument of [`watch`][watchfiles.watch]
        grace_period: number of seconds after the process is started before watching for changes
        debounce: matches the same argument of [`watch`][watchfiles.watch]
        step: matches the same argument of [`watch`][watchfiles.watch]
        debug: matches the same argument of [`watch`][watchfiles.watch]
        sigint_timeout: the number of seconds to wait after sending sigint before sending sigkill
        sigkill_timeout: the number of seconds to wait after sending sigkill before raising an exception
        recursive: matches the same argument of [`watch`][watchfiles.watch]
        number of times the function was reloaded.
    ```py title="Example of run_process running a function"
    from watchfiles import run_process
    def callback(changes):
        print('changes detected:', changes)
    def foobar(a, b):
        print('foobar called with:', a, b)
        run_process('./path/to/dir', target=foobar, args=(1, 2), callback=callback)
    As well as using a `callback` function, changes can be accessed from within the target function,
    using the `WATCHFILES_CHANGES` environment variable.
    ```py title="Example of run_process accessing changes"
    def foobar(a, b, c):
        # changes will be an empty list "[]" the first time the function is called
        changes = os.getenv('WATCHFILES_CHANGES')
        changes = json.loads(changes)
        print('foobar called due to changes:', changes)
        run_process('./path/to/dir', target=foobar, args=(1, 2, 3))
    Again with the target as `command`, `WATCHFILES_CHANGES` can be used
    to access changes.
    ```bash title="example.sh"
    echo "changers: ${WATCHFILES_CHANGES}"
    ```py title="Example of run_process running a command"
        run_process('.', target='./example.sh')
    if target_type == 'auto':
        target_type = detect_target_type(target)
    logger.debug('running "%s" as %s', target, target_type)
    catch_sigterm()
    process = start_process(target, target_type, args, kwargs)
    reloads = 0
    if grace_period:
        logger.debug('sleeping for %s seconds before watching for changes', grace_period)
        sleep(grace_period)
        for changes in watch(
            *paths,
            watch_filter=watch_filter,
            debounce=debounce,
            step=step,
            debug=debug,
            raise_interrupt=False,
            recursive=recursive,
            ignore_permission_denied=ignore_permission_denied,
            callback and callback(changes)
            process.stop(sigint_timeout=sigint_timeout, sigkill_timeout=sigkill_timeout)
            process = start_process(target, target_type, args, kwargs, changes)
            reloads += 1
        process.stop()
    return reloads
async def arun_process(
    callback: Optional[Callable[[Set[FileChange]], Any]] = None,
    Async equivalent of [`run_process`][watchfiles.run_process], all arguments match those of `run_process` except
    `callback` which can be a coroutine.
    Starting and stopping the process and watching for changes is done in a separate thread.
    As with `run_process`, internally `arun_process` uses [`awatch`][watchfiles.awatch], however `KeyboardInterrupt`
    cannot be caught and suppressed in `awatch` so these errors need to be caught separately, see below.
    ```py title="Example of arun_process usage"
    from watchfiles import arun_process
    async def callback(changes):
        await asyncio.sleep(0.1)
        await arun_process('.', target=foobar, args=(1, 2), callback=callback)
    process = await anyio.to_thread.run_sync(start_process, target, target_type, args, kwargs)
        await anyio.sleep(grace_period)
    async for changes in awatch(
            r = callback(changes)
            if inspect.isawaitable(r):
                await r
        await anyio.to_thread.run_sync(process.stop)
        process = await anyio.to_thread.run_sync(start_process, target, target_type, args, kwargs, changes)
# Use spawn context to make sure code run in subprocess
# does not reuse imported modules in main process/context
spawn_context = get_context('spawn')
def split_cmd(cmd: str) -> List[str]:
    posix = platform.uname().system.lower() != 'windows'
    return shlex.split(cmd, posix=posix)
def start_process(
    target_type: "Literal['function', 'command']",
    args: Tuple[Any, ...],
    kwargs: Optional[Dict[str, Any]],
    changes: Optional[Set[FileChange]] = None,
) -> 'CombinedProcess':
    if changes is None:
        changes_env_var = '[]'
        changes_env_var = json.dumps([[c.raw_str(), p] for c, p in changes])
    os.environ['WATCHFILES_CHANGES'] = changes_env_var
    process: Union[SpawnProcess, subprocess.Popen[bytes]]
    if target_type == 'function':
            args = target, get_tty_path(), args, kwargs
            target_ = run_function
            target_ = target
        process = spawn_context.Process(target=target_, args=args, kwargs=kwargs)
        process.start()
            logger.warning('ignoring args and kwargs for "command" target')
        assert isinstance(target, str), 'target must be a string to run as a command'
        popen_args = split_cmd(target)
        process = subprocess.Popen(popen_args)
    return CombinedProcess(process)
def detect_target_type(target: Union[str, Callable[..., Any]]) -> "Literal['function', 'command']":
    Used by [`run_process`][watchfiles.run_process], [`arun_process`][watchfiles.arun_process]
    and indirectly the CLI to determine the target type with `target_type` is `auto`.
    Detects the target type - either `function` or `command`. This method is only called with `target_type='auto'`.
    The following logic is employed:
    * If `target` is not a string, it is assumed to be a function
    * If `target` ends with `.py` or `.sh`, it is assumed to be a command
    * Otherwise, the target is assumed to be a function if it matches the regex `[a-zA-Z0-9_]+(\\.[a-zA-Z0-9_]+)+`
    If this logic does not work for you, specify the target type explicitly using the `target_type` function argument
    or `--target-type` command line argument.
        target: The target value
        either `'function'` or `'command'`
    if not isinstance(target, str):
        return 'function'
    elif target.endswith(('.py', '.sh')):
        return 'command'
    elif re.fullmatch(r'[a-zA-Z0-9_]+(\.[a-zA-Z0-9_]+)+', target):
class CombinedProcess:
    def __init__(self, p: 'Union[SpawnProcess, subprocess.Popen[bytes]]'):
        self._p = p
        assert self.pid is not None, 'process not yet spawned'
    def stop(self, sigint_timeout: int = 5, sigkill_timeout: int = 1) -> None:
        os.environ.pop('WATCHFILES_CHANGES', None)
        if self.is_alive():
            logger.debug('stopping process...')
            os.kill(self.pid, signal.SIGINT)
                self.join(sigint_timeout)
                # Capture this exception to allow the self.exitcode to be reached.
                # This will allow the SIGKILL to be sent, otherwise it is swallowed up.
                logger.warning('SIGINT timed out after %r seconds', sigint_timeout)
            if self.exitcode is None:
                logger.warning('process has not terminated, sending SIGKILL')
                os.kill(self.pid, signal.SIGKILL)
                self.join(sigkill_timeout)
                logger.debug('process stopped')
            logger.warning('process already dead, exit code: %d', self.exitcode)
    def is_alive(self) -> bool:
        if isinstance(self._p, SpawnProcess):
            return self._p.is_alive()
            return self._p.poll() is None
    def pid(self) -> int:
        # we check the process has always been spawned when CombinedProcess is initialised
        return self._p.pid  # type: ignore[return-value]
    def join(self, timeout: int) -> None:
            self._p.join(timeout)
            self._p.wait(timeout)
    def exitcode(self) -> Optional[int]:
            return self._p.exitcode
            return self._p.returncode
def run_function(function: str, tty_path: Optional[str], args: Tuple[Any, ...], kwargs: Dict[str, Any]) -> None:
    with set_tty(tty_path):
        func = import_string(function)
def import_string(dotted_path: str) -> Any:
    Stolen approximately from django. Import a dotted module path and return the attribute/class designated by the
    last name in the path. Raise ImportError if the import fails.
        module_path, class_name = dotted_path.strip(' ').rsplit('.', 1)
        raise ImportError(f'"{dotted_path}" doesn\'t look like a module path') from e
    module = import_module(module_path)
        return getattr(module, class_name)
        raise ImportError(f'Module "{module_path}" does not define a "{class_name}" attribute') from e
def get_tty_path() -> Optional[str]:  # pragma: no cover
    Return the path to the current TTY, if any.
    Virtually impossible to test in pytest, hence no cover.
        return os.ttyname(sys.stdin.fileno())
        # fileno() always fails with pytest
        return '/dev/tty'
        # on Windows. No idea of a better solution
def set_tty(tty_path: Optional[str]) -> Generator[None, None, None]:
    if tty_path:
            with open(tty_path) as tty:  # pragma: no cover
                sys.stdin = tty
            # eg. "No such device or address: '/dev/tty'", see https://github.com/samuelcolvin/watchfiles/issues/40
        # currently on windows tty_path is None and there's nothing we can do here
def raise_keyboard_interrupt(signum: int, _frame: Any) -> None:  # pragma: no cover
    logger.warning('received signal %s, raising KeyboardInterrupt', signal.Signals(signum))
def catch_sigterm() -> None:
    Catch SIGTERM and raise KeyboardInterrupt instead. This means watchfiles will stop quickly
    on `docker compose stop` and other cases where SIGTERM is sent.
    Without this the watchfiles process will be killed while a running process will continue uninterrupted.
    logger.debug('registering handler for SIGTERM on watchfiles process %d', os.getpid())
    signal.signal(signal.SIGTERM, raise_keyboard_interrupt)
