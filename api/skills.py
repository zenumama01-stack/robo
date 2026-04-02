from typing import Union, Mapping, cast
from ...types import skill_list_params, skill_create_params, skill_update_params
from ..._utils import extract_files, path_template, maybe_transform, deepcopy_minimal, async_maybe_transform
from ...types.skill import Skill
from .versions.versions import (
from ...types.deleted_skill import DeletedSkill
__all__ = ["Skills", "AsyncSkills"]
class Skills(SyncAPIResource):
    def versions(self) -> Versions:
        return Versions(self._client)
    def with_raw_response(self) -> SkillsWithRawResponse:
        return SkillsWithRawResponse(self)
    def with_streaming_response(self) -> SkillsWithStreamingResponse:
        return SkillsWithStreamingResponse(self)
        files: Union[SequenceNotStr[FileTypes], FileTypes] | Omit = omit,
    ) -> Skill:
        Create a new skill.
          files: Skill files to upload (directory upload) or a single zip file.
        body = deepcopy_minimal({"files": files})
        extracted_files = extract_files(cast(Mapping[str, object], body), paths=[["files", "<array>"], ["files"]])
        if extracted_files:
            "/skills",
            body=maybe_transform(body, skill_create_params.SkillCreateParams),
            files=extracted_files,
            cast_to=Skill,
        Get a skill by its ID.
            path_template("/skills/{skill_id}", skill_id=skill_id),
        default_version: str,
        Update the default version pointer for a skill.
          default_version: The skill version number to set as default.
            body=maybe_transform({"default_version": default_version}, skill_update_params.SkillUpdateParams),
    ) -> SyncCursorPage[Skill]:
        List all skills for the current project.
            page=SyncCursorPage[Skill],
                    skill_list_params.SkillListParams,
            model=Skill,
    ) -> DeletedSkill:
        Delete a skill by its ID.
            cast_to=DeletedSkill,
class AsyncSkills(AsyncAPIResource):
    def versions(self) -> AsyncVersions:
        return AsyncVersions(self._client)
    def with_raw_response(self) -> AsyncSkillsWithRawResponse:
        return AsyncSkillsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncSkillsWithStreamingResponse:
        return AsyncSkillsWithStreamingResponse(self)
            body=await async_maybe_transform(body, skill_create_params.SkillCreateParams),
                {"default_version": default_version}, skill_update_params.SkillUpdateParams
    ) -> AsyncPaginator[Skill, AsyncCursorPage[Skill]]:
            page=AsyncCursorPage[Skill],
class SkillsWithRawResponse:
    def __init__(self, skills: Skills) -> None:
        self._skills = skills
            skills.create,
            skills.retrieve,
            skills.update,
            skills.list,
            skills.delete,
        return ContentWithRawResponse(self._skills.content)
    def versions(self) -> VersionsWithRawResponse:
        return VersionsWithRawResponse(self._skills.versions)
class AsyncSkillsWithRawResponse:
    def __init__(self, skills: AsyncSkills) -> None:
        return AsyncContentWithRawResponse(self._skills.content)
    def versions(self) -> AsyncVersionsWithRawResponse:
        return AsyncVersionsWithRawResponse(self._skills.versions)
class SkillsWithStreamingResponse:
        return ContentWithStreamingResponse(self._skills.content)
    def versions(self) -> VersionsWithStreamingResponse:
        return VersionsWithStreamingResponse(self._skills.versions)
class AsyncSkillsWithStreamingResponse:
        return AsyncContentWithStreamingResponse(self._skills.content)
    def versions(self) -> AsyncVersionsWithStreamingResponse:
        return AsyncVersionsWithStreamingResponse(self._skills.versions)
        Create Skill
        Get Skill
            f"/skills/{skill_id}",
        Update Skill Default Version
        List Skills
        Delete Skill
"""Contains commands to manage skills for AI assistants.
    # install the hf-cli skill for Claude (project-level, in current directory)
    hf skills add --claude
    # install for multiple assistants (project-level)
    hf skills add --claude --codex --opencode
    # install globally (user-level)
    hf skills add --claude --global
    # install to a custom directory
    hf skills add --dest=~/my-skills
    # overwrite an existing skill
    hf skills add --claude --force
from huggingface_hub.utils import get_session
from ._cli_utils import typer_factory
DEFAULT_SKILL_ID = "hf-cli"
_GITHUB_RAW_BASE = "https://raw.githubusercontent.com/huggingface/huggingface_hub/main/docs/source/en"
_SKILL_MD_URL = f"{_GITHUB_RAW_BASE}/guides/cli.md"
_REFERENCE_URL = f"{_GITHUB_RAW_BASE}/package_reference/cli.md"
_SKILL_YAML_PREFIX = """\
name: hf-cli
description: >
  Hugging Face Hub CLI (`hf`) for downloading, uploading, and managing
  repositories, models, datasets, and Spaces on the Hugging Face Hub.
The Hugging Face Hub CLI tool `hf` is available. IMPORTANT: The `hf` command replaces the deprecated `huggingface_cli` command.
Use `hf --help` to view available functions. Note that auth commands are now all under `hf auth` e.g. `hf auth whoami`.
CENTRAL_LOCAL = Path(".agents/skills")
CENTRAL_GLOBAL = Path("~/.agents/skills")
GLOBAL_TARGETS = {
    "codex": Path("~/.codex/skills"),
    "claude": Path("~/.claude/skills"),
    "opencode": Path("~/.config/opencode/skills"),
LOCAL_TARGETS = {
    "codex": Path(".codex/skills"),
    "claude": Path(".claude/skills"),
    "opencode": Path(".opencode/skills"),
skills_cli = typer_factory(help="Manage skills for AI assistants.")
def _download(url: str) -> str:
    """Download text content from a URL."""
    response = get_session().get(url)
    return response.text
def _remove_existing(path: Path, force: bool) -> None:
    """Remove existing file/directory/symlink if force is True, otherwise raise an error."""
    if not (path.exists() or path.is_symlink()):
        raise SystemExit(f"Skill already exists at {path}.\nRe-run with --force to overwrite.")
    if path.is_dir() and not path.is_symlink():
def _install_to(skills_dir: Path, force: bool) -> Path:
    """Download and install the skill files into a skills directory. Returns the installed path."""
    skills_dir = skills_dir.expanduser().resolve()
    skills_dir.mkdir(parents=True, exist_ok=True)
    dest = skills_dir / DEFAULT_SKILL_ID
    _remove_existing(dest, force)
    dest.mkdir()
    # SKILL.md – the main guide, prefixed with YAML metadata
    skill_content = _download(_SKILL_MD_URL)
    (dest / "SKILL.md").write_text(_SKILL_YAML_PREFIX + skill_content, encoding="utf-8")
    # references/cli.md – the full CLI reference
    ref_dir = dest / "references"
    ref_dir.mkdir()
    ref_content = _download(_REFERENCE_URL)
    (ref_dir / "cli.md").write_text(ref_content, encoding="utf-8")
def _create_symlink(agent_skills_dir: Path, central_skill_path: Path, force: bool) -> Path:
    """Create a relative symlink from agent directory to the central skill location."""
    agent_skills_dir = agent_skills_dir.expanduser().resolve()
    agent_skills_dir.mkdir(parents=True, exist_ok=True)
    link_path = agent_skills_dir / DEFAULT_SKILL_ID
    _remove_existing(link_path, force)
    link_path.symlink_to(os.path.relpath(central_skill_path, agent_skills_dir))
    return link_path
@skills_cli.command(
        "hf skills add --claude",
        "hf skills add --claude --global",
        "hf skills add --codex --opencode",
def skills_add(
    claude: Annotated[bool, typer.Option("--claude", help="Install for Claude.")] = False,
    codex: Annotated[bool, typer.Option("--codex", help="Install for Codex.")] = False,
    opencode: Annotated[bool, typer.Option("--opencode", help="Install for OpenCode.")] = False,
    global_: Annotated[
            "--global",
            help="Install globally (user-level) instead of in the current project directory.",
    dest: Annotated[
        Optional[Path],
            help="Install into a custom destination (path to skills directory).",
    force: Annotated[
            help="Overwrite existing skills in the destination.",
    """Download a skill and install it for an AI assistant."""
    if not (claude or codex or opencode or dest):
        raise CLIError("Pick a destination via --claude, --codex, --opencode, or --dest.")
    if dest:
        if claude or codex or opencode or global_:
            print("--dest cannot be combined with --claude, --codex, --opencode, or --global.")
            raise typer.Exit(code=1)
        skill_dest = _install_to(dest, force)
        print(f"Installed '{DEFAULT_SKILL_ID}' to {skill_dest}")
    targets_dict = GLOBAL_TARGETS if global_ else LOCAL_TARGETS
    agent_targets: list[Path] = []
    if claude:
        agent_targets.append(targets_dict["claude"])
    if codex:
        agent_targets.append(targets_dict["codex"])
    if opencode:
        agent_targets.append(targets_dict["opencode"])
    central_path = CENTRAL_GLOBAL if global_ else CENTRAL_LOCAL
    central_skill_path = _install_to(central_path, force)
    print(f"Installed '{DEFAULT_SKILL_ID}' to central location: {central_skill_path}")
    for agent_target in agent_targets:
        link_path = _create_symlink(agent_target, central_skill_path, force)
        print(f"Created symlink: {link_path}")
