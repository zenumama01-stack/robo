id = None
with client.responses.create(
    input="solve 8x + 31 = 2",
    background=True,
        if event.type == "response.created":
            id = event.response.id
        if "output_text" in event.type:
            rich.print(event)
        if event.sequence_number == 10:
print("Interrupted. Continuing...")
assert id is not None
with client.responses.retrieve(
    response_id=id,
    starting_after=10,
from collections.abc import Callable
from typing import Annotated, Any
from annotated_doc import Doc
from starlette.background import BackgroundTasks as StarletteBackgroundTasks
class BackgroundTasks(StarletteBackgroundTasks):
    A collection of background tasks that will be called after a response has been
    sent to the client.
    Read more about it in the
    [FastAPI docs for Background Tasks](https://fastapi.tiangolo.com/tutorial/background-tasks/).
    ## Example
    from fastapi import BackgroundTasks, FastAPI
    def write_notification(email: str, message=""):
        with open("log.txt", mode="w") as email_file:
            content = f"notification for {email}: {message}"
            email_file.write(content)
    @app.post("/send-notification/{email}")
    async def send_notification(email: str, background_tasks: BackgroundTasks):
        background_tasks.add_task(write_notification, email, message="some notification")
        return {"message": "Notification sent in the background"}
    def add_task(
        func: Annotated[
            Callable[P, Any],
            Doc(
                The function to call after the response is sent.
                It can be a regular `def` function or an `async def` function.
        Add a function to be called in the background after the response is sent.
        return super().add_task(func, *args, **kwargs)
class BackgroundTask:
    def __init__(self, func: Callable[P, Any], *args: P.args, **kwargs: P.kwargs) -> None:
        self.is_async = is_async_callable(func)
    async def __call__(self) -> None:
            await self.func(*self.args, **self.kwargs)
            await run_in_threadpool(self.func, *self.args, **self.kwargs)
class BackgroundTasks(BackgroundTask):
    def __init__(self, tasks: Sequence[BackgroundTask] | None = None):
        self.tasks = list(tasks) if tasks else []
    def add_task(self, func: Callable[P, Any], *args: P.args, **kwargs: P.kwargs) -> None:
        task = BackgroundTask(func, *args, **kwargs)
        self.tasks.append(task)
        for task in self.tasks:
            await task()
from typing import Annotated, Any, Callable
