from typing import TypeVar, Callable, Awaitable
from typing_extensions import ParamSpec
import sniffio
import anyio.to_thread
T_Retval = TypeVar("T_Retval")
T_ParamSpec = ParamSpec("T_ParamSpec")
async def to_thread(
    func: Callable[T_ParamSpec, T_Retval], /, *args: T_ParamSpec.args, **kwargs: T_ParamSpec.kwargs
) -> T_Retval:
    if sniffio.current_async_library() == "asyncio":
        return await asyncio.to_thread(func, *args, **kwargs)
    return await anyio.to_thread.run_sync(
        functools.partial(func, *args, **kwargs),
# inspired by `asyncer`, https://github.com/tiangolo/asyncer
def asyncify(function: Callable[T_ParamSpec, T_Retval]) -> Callable[T_ParamSpec, Awaitable[T_Retval]]:
    Take a blocking function and create an async one that receives the same
    positional and keyword arguments.
    ```python
    def blocking_func(arg1, arg2, kwarg1=None):
        # blocking code
    result = asyncify(blocking_function)(arg1, arg2, kwarg1=value1)
    ## Arguments
    `function`: a blocking regular callable (e.g. a function)
    ## Return
    An async function that takes the same positional and keyword arguments as the
    original one, that when called runs the same original function in a thread worker
    and returns the result.
    async def wrapper(*args: T_ParamSpec.args, **kwargs: T_ParamSpec.kwargs) -> T_Retval:
        return await to_thread(function, *args, **kwargs)
from backoff._common import (_init_wait_gen, _maybe_call, _next_wait)
def _call_handlers(hdlrs, target, args, kwargs, tries, elapsed, **extra):
    details = {
        'target': target,
        'args': args,
        'kwargs': kwargs,
        'tries': tries,
        'elapsed': elapsed,
    details.update(extra)
    for hdlr in hdlrs:
        hdlr(details)
def retry_predicate(target, wait_gen, predicate,
                    max_tries, max_time, jitter,
                    on_success, on_backoff, on_giveup,
                    wait_gen_kwargs):
    @functools.wraps(target)
    def retry(*args, **kwargs):
        max_tries_value = _maybe_call(max_tries)
        max_time_value = _maybe_call(max_time)
        start = datetime.datetime.now()
        wait = _init_wait_gen(wait_gen, wait_gen_kwargs)
            elapsed = timedelta.total_seconds(datetime.datetime.now() - start)
                "target": target,
                "kwargs": kwargs,
                "tries": tries,
                "elapsed": elapsed,
            ret = target(*args, **kwargs)
            if predicate(ret):
                max_tries_exceeded = (tries == max_tries_value)
                max_time_exceeded = (max_time_value is not None and
                                     elapsed >= max_time_value)
                if max_tries_exceeded or max_time_exceeded:
                    _call_handlers(on_giveup, **details, value=ret)
                    seconds = _next_wait(wait, ret, jitter, elapsed,
                                         max_time_value)
                    _call_handlers(on_giveup, **details)
                _call_handlers(on_backoff, **details,
                               value=ret, wait=seconds)
                _call_handlers(on_success, **details, value=ret)
    return retry
def retry_exception(target, wait_gen, exception,
                    max_tries, max_time, jitter, giveup,
                    on_success, on_backoff, on_giveup, raise_on_giveup,
            except exception as e:
                if giveup(e) or max_tries_exceeded or max_time_exceeded:
                    _call_handlers(on_giveup, **details, exception=e)
                    if raise_on_giveup:
                    seconds = _next_wait(wait, e, jitter, elapsed,
                _call_handlers(on_backoff, **details, wait=seconds,
                               exception=e)
                _call_handlers(on_success, **details)
