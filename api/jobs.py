from ..._utils import get_client, print_model
from ...._types import Omittable, omit
from ...._utils import is_given
from ....pagination import SyncCursorPage
from ....types.fine_tuning import (
    FineTuningJob,
    FineTuningJobEvent,
    sub = subparser.add_parser("fine_tuning.jobs.create")
        help="The model to fine-tune.",
        "-F",
        "--training-file",
        help="The training file to fine-tune the model on.",
        "-H",
        "--hyperparameters",
        help="JSON string of hyperparameters to use for fine-tuning.",
        "-s",
        "--suffix",
        help="A suffix to add to the fine-tuned model name.",
        "--validation-file",
        help="The validation file to use for fine-tuning.",
    sub.set_defaults(func=CLIFineTuningJobs.create, args_model=CLIFineTuningJobsCreateArgs)
    sub = subparser.add_parser("fine_tuning.jobs.retrieve")
        "-i",
        "--id",
        help="The ID of the fine-tuning job to retrieve.",
    sub.set_defaults(func=CLIFineTuningJobs.retrieve, args_model=CLIFineTuningJobsRetrieveArgs)
    sub = subparser.add_parser("fine_tuning.jobs.list")
        "-a",
        "--after",
        help="Identifier for the last job from the previous pagination request. If provided, only jobs created after this job will be returned.",
        "--limit",
        help="Number of fine-tuning jobs to retrieve.",
    sub.set_defaults(func=CLIFineTuningJobs.list, args_model=CLIFineTuningJobsListArgs)
    sub = subparser.add_parser("fine_tuning.jobs.cancel")
        help="The ID of the fine-tuning job to cancel.",
    sub.set_defaults(func=CLIFineTuningJobs.cancel, args_model=CLIFineTuningJobsCancelArgs)
    sub = subparser.add_parser("fine_tuning.jobs.list_events")
        help="The ID of the fine-tuning job to list events for.",
        help="Identifier for the last event from the previous pagination request. If provided, only events created after this event will be returned.",
        help="Number of fine-tuning job events to retrieve.",
    sub.set_defaults(func=CLIFineTuningJobs.list_events, args_model=CLIFineTuningJobsListEventsArgs)
class CLIFineTuningJobsCreateArgs(BaseModel):
    training_file: str
    hyperparameters: Omittable[str] = omit
    validation_file: Omittable[str] = omit
class CLIFineTuningJobsRetrieveArgs(BaseModel):
class CLIFineTuningJobsListArgs(BaseModel):
    after: Omittable[str] = omit
    limit: Omittable[int] = omit
class CLIFineTuningJobsCancelArgs(BaseModel):
class CLIFineTuningJobsListEventsArgs(BaseModel):
class CLIFineTuningJobs:
    def create(args: CLIFineTuningJobsCreateArgs) -> None:
        hyperparameters = json.loads(str(args.hyperparameters)) if is_given(args.hyperparameters) else omit
        fine_tuning_job: FineTuningJob = get_client().fine_tuning.jobs.create(
            training_file=args.training_file,
            hyperparameters=hyperparameters,
            validation_file=args.validation_file,
        print_model(fine_tuning_job)
    def retrieve(args: CLIFineTuningJobsRetrieveArgs) -> None:
        fine_tuning_job: FineTuningJob = get_client().fine_tuning.jobs.retrieve(fine_tuning_job_id=args.id)
    def list(args: CLIFineTuningJobsListArgs) -> None:
        fine_tuning_jobs: SyncCursorPage[FineTuningJob] = get_client().fine_tuning.jobs.list(
            after=args.after or omit, limit=args.limit or omit
        print_model(fine_tuning_jobs)
    def cancel(args: CLIFineTuningJobsCancelArgs) -> None:
        fine_tuning_job: FineTuningJob = get_client().fine_tuning.jobs.cancel(fine_tuning_job_id=args.id)
    def list_events(args: CLIFineTuningJobsListEventsArgs) -> None:
        fine_tuning_job_events: SyncCursorPage[FineTuningJobEvent] = get_client().fine_tuning.jobs.list_events(
            fine_tuning_job_id=args.id,
            after=args.after or omit,
            limit=args.limit or omit,
        print_model(fine_tuning_job_events)
from ....types.fine_tuning import job_list_params, job_create_params, job_list_events_params
from ....types.fine_tuning.fine_tuning_job import FineTuningJob
from ....types.fine_tuning.fine_tuning_job_event import FineTuningJobEvent
__all__ = ["Jobs", "AsyncJobs"]
class Jobs(SyncAPIResource):
    def with_raw_response(self) -> JobsWithRawResponse:
        return JobsWithRawResponse(self)
    def with_streaming_response(self) -> JobsWithStreamingResponse:
        return JobsWithStreamingResponse(self)
        model: Union[str, Literal["babbage-002", "davinci-002", "gpt-3.5-turbo", "gpt-4o-mini"]],
        training_file: str,
        hyperparameters: job_create_params.Hyperparameters | Omit = omit,
        integrations: Optional[Iterable[job_create_params.Integration]] | Omit = omit,
        method: job_create_params.Method | Omit = omit,
        validation_file: Optional[str] | Omit = omit,
    ) -> FineTuningJob:
        Creates a fine-tuning job which begins the process of creating a new model from
        a given dataset.
        Response includes details of the enqueued job including job status and the name
        of the fine-tuned models once complete.
        [Learn more about fine-tuning](https://platform.openai.com/docs/guides/model-optimization)
          model: The name of the model to fine-tune. You can select one of the
              [supported models](https://platform.openai.com/docs/guides/fine-tuning#which-models-can-be-fine-tuned).
          training_file: The ID of an uploaded file that contains training data.
              Your dataset must be formatted as a JSONL file. Additionally, you must upload
              your file with the purpose `fine-tune`.
              The contents of the file should differ depending on if the model uses the
              [chat](https://platform.openai.com/docs/api-reference/fine-tuning/chat-input),
              format, or if the fine-tuning method uses the
              [preference](https://platform.openai.com/docs/api-reference/fine-tuning/preference-input)
              [fine-tuning guide](https://platform.openai.com/docs/guides/model-optimization)
              for more details.
          hyperparameters: The hyperparameters used for the fine-tuning job. This value is now deprecated
              in favor of `method`, and should be passed in under the `method` parameter.
          integrations: A list of integrations to enable for your fine-tuning job.
          method: The method used for fine-tuning.
          seed: The seed controls the reproducibility of the job. Passing in the same seed and
              job parameters should produce the same results, but may differ in rare cases. If
              a seed is not specified, one will be generated for you.
          suffix: A string of up to 64 characters that will be added to your fine-tuned model
              name.
              For example, a `suffix` of "custom-model-name" would produce a model name like
              `ft:gpt-4o-mini:openai:custom-model-name:7p4lURel`.
          validation_file: The ID of an uploaded file that contains validation data.
              If you provide this file, the data is used to generate validation metrics
              periodically during fine-tuning. These metrics can be viewed in the fine-tuning
              results file. The same data should not be present in both train and validation
              files.
              Your dataset must be formatted as a JSONL file. You must upload your file with
              the purpose `fine-tune`.
            "/fine_tuning/jobs",
                    "training_file": training_file,
                    "hyperparameters": hyperparameters,
                    "integrations": integrations,
                    "method": method,
                    "validation_file": validation_file,
                job_create_params.JobCreateParams,
            cast_to=FineTuningJob,
        Get info about a fine-tuning job.
            path_template("/fine_tuning/jobs/{fine_tuning_job_id}", fine_tuning_job_id=fine_tuning_job_id),
        metadata: Optional[Dict[str, str]] | Omit = omit,
    ) -> SyncCursorPage[FineTuningJob]:
        List your organization's fine-tuning jobs
          after: Identifier for the last job from the previous pagination request.
          limit: Number of fine-tuning jobs to retrieve.
          metadata: Optional metadata filter. To filter, use the syntax `metadata[k]=v`.
              Alternatively, set `metadata=null` to indicate no metadata.
            page=SyncCursorPage[FineTuningJob],
                    job_list_params.JobListParams,
            model=FineTuningJob,
        Immediately cancel a fine-tune job.
            path_template("/fine_tuning/jobs/{fine_tuning_job_id}/cancel", fine_tuning_job_id=fine_tuning_job_id),
    def list_events(
    ) -> SyncCursorPage[FineTuningJobEvent]:
        Get status updates for a fine-tuning job.
          after: Identifier for the last event from the previous pagination request.
          limit: Number of events to retrieve.
            path_template("/fine_tuning/jobs/{fine_tuning_job_id}/events", fine_tuning_job_id=fine_tuning_job_id),
            page=SyncCursorPage[FineTuningJobEvent],
                    job_list_events_params.JobListEventsParams,
            model=FineTuningJobEvent,
    def pause(
        Pause a fine-tune job.
            path_template("/fine_tuning/jobs/{fine_tuning_job_id}/pause", fine_tuning_job_id=fine_tuning_job_id),
    def resume(
        Resume a fine-tune job.
            path_template("/fine_tuning/jobs/{fine_tuning_job_id}/resume", fine_tuning_job_id=fine_tuning_job_id),
class AsyncJobs(AsyncAPIResource):
    def with_raw_response(self) -> AsyncJobsWithRawResponse:
        return AsyncJobsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncJobsWithStreamingResponse:
        return AsyncJobsWithStreamingResponse(self)
    ) -> AsyncPaginator[FineTuningJob, AsyncCursorPage[FineTuningJob]]:
            page=AsyncCursorPage[FineTuningJob],
    ) -> AsyncPaginator[FineTuningJobEvent, AsyncCursorPage[FineTuningJobEvent]]:
            page=AsyncCursorPage[FineTuningJobEvent],
    async def pause(
    async def resume(
class JobsWithRawResponse:
    def __init__(self, jobs: Jobs) -> None:
        self._jobs = jobs
            jobs.create,
            jobs.retrieve,
            jobs.list,
            jobs.cancel,
        self.list_events = _legacy_response.to_raw_response_wrapper(
            jobs.list_events,
        self.pause = _legacy_response.to_raw_response_wrapper(
            jobs.pause,
        self.resume = _legacy_response.to_raw_response_wrapper(
            jobs.resume,
        return CheckpointsWithRawResponse(self._jobs.checkpoints)
class AsyncJobsWithRawResponse:
    def __init__(self, jobs: AsyncJobs) -> None:
        self.list_events = _legacy_response.async_to_raw_response_wrapper(
        self.pause = _legacy_response.async_to_raw_response_wrapper(
        self.resume = _legacy_response.async_to_raw_response_wrapper(
        return AsyncCheckpointsWithRawResponse(self._jobs.checkpoints)
class JobsWithStreamingResponse:
        self.list_events = to_streamed_response_wrapper(
        self.pause = to_streamed_response_wrapper(
        self.resume = to_streamed_response_wrapper(
        return CheckpointsWithStreamingResponse(self._jobs.checkpoints)
class AsyncJobsWithStreamingResponse:
        self.list_events = async_to_streamed_response_wrapper(
        self.pause = async_to_streamed_response_wrapper(
        self.resume = async_to_streamed_response_wrapper(
        return AsyncCheckpointsWithStreamingResponse(self._jobs.checkpoints)
            f"/fine_tuning/jobs/{fine_tuning_job_id}",
            f"/fine_tuning/jobs/{fine_tuning_job_id}/cancel",
            f"/fine_tuning/jobs/{fine_tuning_job_id}/events",
            f"/fine_tuning/jobs/{fine_tuning_job_id}/pause",
            f"/fine_tuning/jobs/{fine_tuning_job_id}/resume",
"""Contains commands to interact with jobs on the Hugging Face Hub.
    # run a job
    hf jobs run <image> <command>
    # List running or completed jobs
    hf jobs ps [-a] [-f key=value] [--format TEMPLATE]
    # Stream logs from a job
    hf jobs logs <job-id>
    # Stream resources usage stats and metrics from a job
    hf jobs stats <job-id>
    # Inspect detailed information about a job
    hf jobs inspect <job-id>
    # Cancel a running job
    hf jobs cancel <job-id>
    # List available hardware options
    hf jobs hardware
    # Run a UV script
    hf jobs uv run <script>
    # Schedule a job
    hf jobs scheduled run <schedule> <image> <command>
    # List scheduled jobs
    hf jobs scheduled ps [-a] [-f key=value] [--format TEMPLATE]
    # Inspect a scheduled job
    hf jobs scheduled inspect <scheduled_job_id>
    # Suspend a scheduled job
    hf jobs scheduled suspend <scheduled_job_id>
    # Resume a scheduled job
    hf jobs scheduled resume <scheduled_job_id>
    # Delete a scheduled job
    hf jobs scheduled delete <scheduled_job_id>
import multiprocessing.pool
from dataclasses import asdict
from fnmatch import fnmatch
from queue import Empty, Queue
from typing import Annotated, Any, Callable, Dict, Iterable, Optional, TypeVar, Union
from huggingface_hub import SpaceHardware, get_token
from huggingface_hub.errors import CLIError, HfHubHTTPError
from huggingface_hub.utils import logging
from huggingface_hub.utils._cache_manager import _format_size
from huggingface_hub.utils._dotenv import load_dotenv
from ._cli_utils import TokenOpt, get_hf_api, typer_factory
SUGGESTED_FLAVORS = [item.value for item in SpaceHardware if item.value != "zero-a10g"]
STATS_UPDATE_MIN_INTERVAL = 0.1  # we set a limit here since there is one update per second per job
# Common job-related options
ImageArg = Annotated[
        help="The Docker image to use.",
ImageOpt = Annotated[
        help="Use a custom Docker image with `uv` installed.",
FlavorOpt = Annotated[
    Optional[SpaceHardware],
        help="Flavor for the hardware, as in HF Spaces. Run 'hf jobs hardware' to list available flavors. Defaults to `cpu-basic`.",
EnvOpt = Annotated[
        "--env",
        help="Set environment variables. E.g. --env ENV=value",
SecretsOpt = Annotated[
        "--secrets",
        help="Set secret environment variables. E.g. --secrets SECRET=value or `--secrets HF_TOKEN` to pass your Hugging Face token.",
LabelsOpt = Annotated[
        "--label",
        help="Set labels. E.g. --label KEY=VALUE or --label LABEL",
EnvFileOpt = Annotated[
        "--env-file",
        help="Read in a file of environment variables.",
SecretsFileOpt = Annotated[
        help="Read in a file of secret environment variables.",
TimeoutOpt = Annotated[
        help="Max duration: int/float with s (seconds, default), m (minutes), h (hours) or d (days).",
DetachOpt = Annotated[
        "--detach",
        help="Run the Job in the background and print the Job ID.",
NamespaceOpt = Annotated[
        help="The namespace where the job will be running. Defaults to the current user's namespace.",
WithOpt = Annotated[
        "--with",
        help="Run with the given packages installed",
PythonOpt = Annotated[
        "--python",
        help="The Python interpreter to use for the run environment",
SuspendOpt = Annotated[
    Optional[bool],
        help="Suspend (pause) the scheduled Job",
ConcurrencyOpt = Annotated[
        help="Allow multiple instances of this Job to run concurrently",
ScheduleArg = Annotated[
        help="One of annually, yearly, monthly, weekly, daily, hourly, or a CRON schedule expression.",
ScriptArg = Annotated[
        help="UV script to run (local file or URL)",
ScriptArgsArg = Annotated[
        help="Arguments for the script",
CommandArg = Annotated[
        help="The command to run.",
JobIdArg = Annotated[
        help="Job ID",
JobIdsArg = Annotated[
        help="Job IDs",
ScheduledJobIdArg = Annotated[
        help="Scheduled Job ID",
jobs_cli = typer_factory(help="Run and manage Jobs on the Hub.")
@jobs_cli.command(
    "run",
    context_settings={"ignore_unknown_options": True},
        "hf jobs run python:3.12 python -c 'print(\"Hello!\")'",
        "hf jobs run -e FOO=foo python:3.12 python script.py",
        "hf jobs run --secrets HF_TOKEN python:3.12 python script.py",
def jobs_run(
    image: ImageArg,
    command: CommandArg,
    env: EnvOpt = None,
    secrets: SecretsOpt = None,
    label: LabelsOpt = None,
    env_file: EnvFileOpt = None,
    secrets_file: SecretsFileOpt = None,
    flavor: FlavorOpt = None,
    timeout: TimeoutOpt = None,
    detach: DetachOpt = False,
    namespace: NamespaceOpt = None,
    """Run a Job."""
    env_map: dict[str, Optional[str]] = {}
    if env_file:
        env_map.update(load_dotenv(Path(env_file).read_text(), environ=os.environ.copy()))
    for env_value in env or []:
        env_map.update(load_dotenv(env_value, environ=os.environ.copy()))
    secrets_map: dict[str, Optional[str]] = {}
    extended_environ = _get_extended_environ()
    if secrets_file:
        secrets_map.update(load_dotenv(Path(secrets_file).read_text(), environ=extended_environ))
    for secret in secrets or []:
        secrets_map.update(load_dotenv(secret, environ=extended_environ))
    job = api.run_job(
        image=image,
        command=command,
        env=env_map,
        secrets=secrets_map,
        labels=_parse_labels_map(label),
        flavor=flavor,
    # Always print the job ID to the user
    print(f"Job started with ID: {job.id}")
    print(f"View at: {job.url}")
    if detach:
    # Now let's stream the logs
    for log in api.fetch_job_logs(job_id=job.id, namespace=job.owner.name):
        print(log)
@jobs_cli.command("logs", examples=["hf jobs logs <job_id>"])
def jobs_logs(
    job_id: JobIdArg,
    """Fetch the logs of a Job"""
        for log in api.fetch_job_logs(job_id=job_id, namespace=namespace):
        status = e.response.status_code if e.response is not None else None
        if status == 404:
            raise CLIError("Job not found. Please check the job ID.") from e
        elif status == 403:
            raise CLIError("Access denied. You may not have permission to view this job.") from e
            raise CLIError(f"Failed to fetch job logs: {e}") from e
def _matches_filters(job_properties: dict[str, str], filters: list[tuple[str, str, str]]) -> bool:
    """Check if scheduled job matches all specified filters."""
    for key, op_str, pattern in filters:
        value = job_properties.get(key)
            if op_str == "!=":
        match = fnmatch(value.lower(), pattern.lower())
        if (op_str == "=" and not match) or (op_str == "!=" and match):
def _print_output(
    rows: list[list[Union[str, int]]], headers: list[str], aliases: list[str], fmt: Optional[str]
    """Print output according to the chosen format."""
    if fmt:
        # Use custom template if provided
        template = fmt
            line = template
            for i, field in enumerate(aliases):
                placeholder = f"{{{{.{field}}}}}"
                if placeholder in line:
                    line = line.replace(placeholder, str(row[i]))
        # Default tabular format
        print(_tabulate(rows, headers=headers))
def _clear_line(n: int) -> None:
    LINE_UP = "\033[1A"
    LINE_CLEAR = "\x1b[2K"
    for i in range(n):
        print(LINE_UP, end=LINE_CLEAR)
def _get_jobs_stats_rows(
    job_id: str, metrics_stream: Iterable[dict[str, Any]], table_headers: list[str]
) -> Iterable[tuple[bool, str, list[list[Union[str, int]]]]]:
    for metrics in metrics_stream:
        row = [
            job_id,
            f"{metrics['cpu_usage_pct']}%",
            round(metrics["cpu_millicores"] / 1000.0, 1),
            f"{round(100 * metrics['memory_used_bytes'] / metrics['memory_total_bytes'], 2)}%",
            f"{_format_size(metrics['memory_used_bytes'])}B / {_format_size(metrics['memory_total_bytes'])}B",
            f"{_format_size(metrics['rx_bps'])}bps / {_format_size(metrics['tx_bps'])}bps",
        if metrics["gpus"] and isinstance(metrics["gpus"], dict):
            rows = [row] + [[""] * len(row)] * (len(metrics["gpus"]) - 1)
            for row, gpu_id in zip(rows, sorted(metrics["gpus"])):
                gpu = metrics["gpus"][gpu_id]
                row += [
                    f"{gpu['utilization']}%",
                    f"{round(100 * gpu['memory_used_bytes'] / gpu['memory_total_bytes'], 2)}%",
                    f"{_format_size(gpu['memory_used_bytes'])}B / {_format_size(gpu['memory_total_bytes'])}B",
            row += ["N/A"] * (len(table_headers) - len(row))
            rows = [row]
        yield False, job_id, rows
    yield True, job_id, []
@jobs_cli.command("stats", examples=["hf jobs stats <job_id>"])
def jobs_stats(
    job_ids: JobIdsArg = None,
    """Fetch the resource usage statistics and metrics of Jobs"""
    if namespace is None:
        namespace = api.whoami()["name"]
    if job_ids is None:
        job_ids = [
            job.id
            for job in api.list_jobs(namespace=namespace)
            if (job.status.stage if job.status else "UNKNOWN") in ("RUNNING", "UPDATING")
    if len(job_ids) == 0:
        print("No running jobs found")
    table_headers = [
        "JOB ID",
        "CPU %",
        "NUM CPU",
        "MEM %",
        "MEM USAGE",
        "NET I/O",
        "GPU UTIL %",
        "GPU MEM %",
        "GPU MEM USAGE",
    headers_aliases = [
        "cpu_usage_pct",
        "cpu_millicores",
        "memory_used_bytes_pct",
        "memory_used_bytes_and_total_bytes",
        "rx_bps_and_tx_bps",
        "gpu_utilization",
        "gpu_memory_used_bytes_pct",
        "gpu_memory_used_bytes_and_total_bytes",
        with multiprocessing.pool.ThreadPool(len(job_ids)) as pool:
            rows_per_job_id: dict[str, list[list[Union[str, int]]]] = {}
            for job_id in job_ids:
                row: list[Union[str, int]] = [job_id]
                row += ["-- / --" if ("/" in header or "USAGE" in header) else "--" for header in table_headers[1:]]
                rows_per_job_id[job_id] = [row]
            last_update_time = time.time()
            total_rows = [row for job_id in rows_per_job_id for row in rows_per_job_id[job_id]]
            _print_output(total_rows, table_headers, headers_aliases, None)
            kwargs_list = [
                    "job_id": job_id,
                    "metrics_stream": api.fetch_job_metrics(job_id=job_id, namespace=namespace),
                    "table_headers": table_headers,
                for job_id in job_ids
            for done, job_id, rows in iflatmap_unordered(pool, _get_jobs_stats_rows, kwargs_list=kwargs_list):
                if done:
                    rows_per_job_id.pop(job_id, None)
                    rows_per_job_id[job_id] = rows
                if now - last_update_time >= STATS_UPDATE_MIN_INTERVAL:
                    _clear_line(2 + len(total_rows))
                    last_update_time = now
            raise CLIError(f"Failed to fetch job stats: {e}") from e
@jobs_cli.command("ps", examples=["hf jobs ps", "hf jobs ps -a"])
def jobs_ps(
    all: Annotated[
            "--all",
            help="Show all Jobs (default shows just running)",
            help="Filter output based on conditions provided (format: key=value)",
            help="Format output using a custom template",
    """List Jobs."""
    # Fetch jobs data
    jobs = api.list_jobs(namespace=namespace)
    # Define table headers
    table_headers = ["JOB ID", "IMAGE/SPACE", "COMMAND", "CREATED", "STATUS"]
    headers_aliases = ["id", "image", "command", "created", "status"]
    rows: list[list[Union[str, int]]] = []
    filters: list[tuple[str, str, str]] = []
    labels_filters: list[tuple[str, str, str]] = []
    for f in filter or []:
        if f.startswith("label!=") or f.startswith("label="):
            if f.startswith("label!="):
                label_part = f[len("label!=") :]
                if "=" in label_part:
                        f"Warning: Ignoring invalid label filter format 'label!={label_part}'. Use label!=key format."
                label_key, op, label_value = label_part, "!=", "*"
                label_part = f[len("label=") :]
                    label_key, label_value = label_part.split("=", 1)
                    label_key, label_value = label_part, "*"
                # Negate predicate in case of key!=value
                if label_key.endswith("!"):
                    op = "!="
                    label_key = label_key[:-1]
                    op = "="
            labels_filters.append((label_key.lower(), op, label_value.lower()))
        elif "=" in f:
            key, value = f.split("=", 1)
            if key.endswith("!"):
                key = key[:-1]
            filters.append((key.lower(), op, value.lower()))
            print(f"Warning: Ignoring invalid filter format '{f}'. Use key=value format.")
    # Process jobs data
    for job in jobs:
        # Extract job data for filtering
        status = job.status.stage if job.status else "UNKNOWN"
        if not all and status not in ("RUNNING", "UPDATING"):
            # Skip job if not all jobs should be shown and status doesn't match criteria
        # Extract job data for output
        job_id = job.id
        # Extract image or space information
        image_or_space = job.docker_image or "N/A"
        # Extract and format command
        cmd = job.command or []
        command_str = " ".join(cmd) if cmd else "N/A"
        # Extract creation time
        created_at = job.created_at.strftime("%Y-%m-%d %H:%M:%S") if job.created_at else "N/A"
        # Create a dict with all job properties for filtering
        props = {"id": job_id, "image": image_or_space, "status": status.lower(), "command": command_str}
        if not _matches_filters(props, filters):
        if not _matches_filters(job.labels or {}, labels_filters):
        # Create row
        rows.append([job_id, image_or_space, command_str, created_at, status])
    # Handle empty results
    if not rows:
        filters_msg = f" matching filters: {', '.join([f'{k}{o}{v}' for k, o, v in filters])}" if filters else ""
        print(f"No jobs found{filters_msg}")
    # Apply custom format if provided or use default tabular format
    _print_output(rows, table_headers, headers_aliases, format)
@jobs_cli.command("hardware", examples=["hf jobs hardware"])
def jobs_hardware() -> None:
    """List available hardware options for Jobs"""
    api = get_hf_api()
    hardware_list = api.list_jobs_hardware()
    table_headers = ["NAME", "PRETTY NAME", "CPU", "RAM", "ACCELERATOR", "COST/MIN", "COST/HOUR"]
    headers_aliases = ["name", "prettyName", "cpu", "ram", "accelerator", "costMin", "costHour"]
    for hw in hardware_list:
        accelerator_info = "N/A"
        if hw.accelerator:
            accelerator_info = f"{hw.accelerator.quantity}x {hw.accelerator.model} ({hw.accelerator.vram})"
        cost_min = f"${hw.unit_cost_usd:.4f}" if hw.unit_cost_usd is not None else "N/A"
        cost_hour = f"${hw.unit_cost_usd * 60:.2f}" if hw.unit_cost_usd is not None else "N/A"
        rows.append([hw.name, hw.pretty_name or "N/A", hw.cpu, hw.ram, accelerator_info, cost_min, cost_hour])
        print("No hardware options found")
    _print_output(rows, table_headers, headers_aliases, None)
@jobs_cli.command("inspect", examples=["hf jobs inspect <job_id>"])
def jobs_inspect(
    job_ids: Annotated[
            help="The jobs to inspect",
    """Display detailed information on one or more Jobs"""
        jobs = [api.inspect_job(job_id=job_id, namespace=namespace) for job_id in job_ids]
        print(json.dumps([asdict(job) for job in jobs], indent=4, default=str))
            raise CLIError(f"Failed to inspect job: {e}") from e
@jobs_cli.command("cancel", examples=["hf jobs cancel <job_id>"])
def jobs_cancel(
    """Cancel a Job"""
        api.cancel_job(job_id=job_id, namespace=namespace)
            raise CLIError("Access denied. You may not have permission to cancel this job.") from e
            raise CLIError(f"Failed to cancel job: {e}") from e
uv_app = typer_factory(help="Run UV scripts (Python with inline dependencies) on HF infrastructure.")
jobs_cli.add_typer(uv_app, name="uv")
@uv_app.command(
        "hf jobs uv run my_script.py",
        "hf jobs uv run ml_training.py --flavor a10g-small",
        "hf jobs uv run --with transformers train.py",
def jobs_uv_run(
    script: ScriptArg,
    script_args: ScriptArgsArg = None,
    image: ImageOpt = None,
    with_: WithOpt = None,
    python: PythonOpt = None,
    """Run a UV script (local file or URL) on HF infrastructure"""
    job = api.run_uv_job(
        script=script,
        script_args=script_args or [],
        dependencies=with_,
        python=python,
        flavor=flavor,  # type: ignore[arg-type]
scheduled_app = typer_factory(help="Create and manage scheduled Jobs on the Hub.")
jobs_cli.add_typer(scheduled_app, name="scheduled")
@scheduled_app.command(
    examples=['hf jobs scheduled run "0 0 * * *" python:3.12 python script.py'],
def scheduled_run(
    schedule: ScheduleArg,
    suspend: SuspendOpt = None,
    concurrency: ConcurrencyOpt = None,
    """Schedule a Job."""
    scheduled_job = api.create_scheduled_job(
        schedule=schedule,
        suspend=suspend,
        concurrency=concurrency,
    print(f"Scheduled Job created with ID: {scheduled_job.id}")
@scheduled_app.command("ps", examples=["hf jobs scheduled ps"])
def scheduled_ps(
            help="Show all scheduled Jobs (default hides suspended)",
    """List scheduled Jobs"""
    scheduled_jobs = api.list_scheduled_jobs(namespace=namespace)
    table_headers = ["ID", "SCHEDULE", "IMAGE/SPACE", "COMMAND", "LAST RUN", "NEXT RUN", "SUSPEND"]
    headers_aliases = ["id", "schedule", "image", "command", "last", "next", "suspend"]
        if "=" in f:
    for scheduled_job in scheduled_jobs:
        suspend = scheduled_job.suspend or False
        if not all and suspend:
        sj_id = scheduled_job.id
        schedule = scheduled_job.schedule or "N/A"
        image_or_space = scheduled_job.job_spec.docker_image or "N/A"
        cmd = scheduled_job.job_spec.command or []
        last_job_at = (
            scheduled_job.status.last_job.at.strftime("%Y-%m-%d %H:%M:%S") if scheduled_job.status.last_job else "N/A"
        next_job_run_at = (
            scheduled_job.status.next_job_run_at.strftime("%Y-%m-%d %H:%M:%S")
            if scheduled_job.status.next_job_run_at
            else "N/A"
        props = {"id": sj_id, "image": image_or_space, "suspend": str(suspend), "command": command_str}
        rows.append([sj_id, schedule, image_or_space, command_str, last_job_at, next_job_run_at, suspend])
        print(f"No scheduled jobs found{filters_msg}")
@scheduled_app.command("inspect", examples=["hf jobs scheduled inspect <id>"])
def scheduled_inspect(
    scheduled_job_ids: Annotated[
            help="The scheduled jobs to inspect",
    """Display detailed information on one or more scheduled Jobs"""
    scheduled_jobs = [
        api.inspect_scheduled_job(scheduled_job_id=scheduled_job_id, namespace=namespace)
        for scheduled_job_id in scheduled_job_ids
    print(json.dumps([asdict(scheduled_job) for scheduled_job in scheduled_jobs], indent=4, default=str))
@scheduled_app.command("delete", examples=["hf jobs scheduled delete <id>"])
def scheduled_delete(
    scheduled_job_id: ScheduledJobIdArg,
    """Delete a scheduled Job."""
    api.delete_scheduled_job(scheduled_job_id=scheduled_job_id, namespace=namespace)
@scheduled_app.command("suspend", examples=["hf jobs scheduled suspend <id>"])
def scheduled_suspend(
    """Suspend (pause) a scheduled Job."""
    api.suspend_scheduled_job(scheduled_job_id=scheduled_job_id, namespace=namespace)
@scheduled_app.command("resume", examples=["hf jobs scheduled resume <id>"])
def scheduled_resume(
    """Resume (unpause) a scheduled Job."""
    api.resume_scheduled_job(scheduled_job_id=scheduled_job_id, namespace=namespace)
scheduled_uv_app = typer_factory(help="Schedule UV scripts on HF infrastructure.")
scheduled_app.add_typer(scheduled_uv_app, name="uv")
@scheduled_uv_app.command(
        'hf jobs scheduled uv run "0 0 * * *" script.py',
        'hf jobs scheduled uv run "0 0 * * *" script.py --with pandas',
def scheduled_uv_run(
    job = api.create_scheduled_uv_job(
    print(f"Scheduled Job created with ID: {job.id}")
### UTILS
def _parse_labels_map(labels: Optional[list[str]]) -> Optional[dict[str, str]]:
    """Parse label key-value pairs from CLI arguments.
        labels: List of label strings in KEY=VALUE format. If KEY only, then VALUE is set to empty string.
        Dictionary mapping label keys to values, or None if no labels provided.
    if not labels:
    labels_map: dict[str, str] = {}
    for label_var in labels:
        key, value = label_var.split("=", 1) if "=" in label_var else (label_var, "")
        labels_map[key] = value
    return labels_map
def _tabulate(rows: list[list[Union[str, int]]], headers: list[str]) -> str:
    Inspired by:
    - stackoverflow.com/a/8356620/593036
    - stackoverflow.com/questions/9535954/printing-lists-as-tabular-data
    col_widths = [max(len(str(x)) for x in col) for col in zip(*rows, headers)]
    terminal_width = max(os.get_terminal_size().columns, len(headers) * 12)
    while len(headers) + sum(col_widths) > terminal_width:
        col_to_minimize = col_widths.index(max(col_widths))
        col_widths[col_to_minimize] //= 2
        if len(headers) + sum(col_widths) <= terminal_width:
            col_widths[col_to_minimize] = terminal_width - sum(col_widths) - len(headers) + col_widths[col_to_minimize]
    row_format = ("{{:{}}} " * len(headers)).format(*col_widths)
    lines.append(row_format.format(*headers))
    lines.append(row_format.format(*["-" * w for w in col_widths]))
        row_format_args = [
            str(x)[: col_width - 3] + "..." if len(str(x)) > col_width else str(x)
            for x, col_width in zip(row, col_widths)
        lines.append(row_format.format(*row_format_args))
def _get_extended_environ() -> Dict[str, str]:
    extended_environ = os.environ.copy()
    if (token := get_token()) is not None:
        extended_environ["HF_TOKEN"] = token
    return extended_environ
def _write_generator_to_queue(queue: Queue[T], func: Callable[..., Iterable[T]], kwargs: dict) -> None:
    for result in func(**kwargs):
        queue.put(result)
def iflatmap_unordered(
    pool: multiprocessing.pool.ThreadPool,
    func: Callable[..., Iterable[T]],
    kwargs_list: list[dict],
) -> Iterable[T]:
    Takes a function that returns an iterable of items, and run it in parallel using threads to return the flattened iterable of items as they arrive.
    This is inspired by those three `map()` variants, and is the mix of all three:
    * `imap()`: like `map()` but returns an iterable instead of a list of results
    * `imap_unordered()`: like `imap()` but the output is sorted by time of arrival
    * `flatmap()`: like `map()` but given a function which returns a list, `flatmap()` returns the flattened list that is the concatenation of all the output lists
    queue: Queue[T] = Queue()
    async_results = [pool.apply_async(_write_generator_to_queue, (queue, func, kwargs)) for kwargs in kwargs_list]
                yield queue.get(timeout=0.05)
            except Empty:
                if all(async_result.ready() for async_result in async_results) and queue.empty():
        # we get the result in case there's an error to raise
            [async_result.get(timeout=0.05) for async_result in async_results]
        except multiprocessing.TimeoutError:
