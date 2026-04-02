from ._utils import Colors, organization_info
from .._exceptions import APIError, OpenAIError
class CLIError(OpenAIError): ...
class SilentCLIError(CLIError): ...
def display_error(err: CLIError | APIError | pydantic.ValidationError) -> None:
    if isinstance(err, SilentCLIError):
    sys.stderr.write("{}{}Error:{} {}\n".format(organization_info(), Colors.FAIL, Colors.ENDC, err))
# Copyright 2026 The HuggingFace Team. All rights reserved.
"""CLI error handling utilities."""
from huggingface_hub.errors import (
    GatedRepoError,
    HfHubHTTPError,
    LocalTokenNotFoundError,
    RemoteEntryNotFoundError,
    RepositoryNotFoundError,
    RevisionNotFoundError,
CLI_ERROR_MAPPINGS: dict[type[Exception], Callable[[Exception], str]] = {
    RepositoryNotFoundError: lambda e: (
        "Repository not found. Check the `repo_id` and `repo_type` parameters. If the repo is private, make sure you are authenticated."
    RevisionNotFoundError: lambda e: "Revision not found. Check the `revision` parameter.",
    GatedRepoError: lambda e: "Access denied. This repository requires approval.",
    LocalTokenNotFoundError: lambda e: "Not logged in. Run 'hf auth login' first.",
    RemoteEntryNotFoundError: lambda e: "File not found in repository.",
    HfHubHTTPError: lambda e: str(e),
def format_known_exception(e: Exception) -> Optional[str]:
    for exc_type, formatter in CLI_ERROR_MAPPINGS.items():
        if isinstance(e, exc_type):
            return formatter(e)
