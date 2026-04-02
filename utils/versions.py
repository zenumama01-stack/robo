from ...._types import (
from ....types.skills import version_list_params, version_create_params
from ....types.skills.skill_version import SkillVersion
from ....types.skills.deleted_skill_version import DeletedSkillVersion
__all__ = ["Versions", "AsyncVersions"]
class Versions(SyncAPIResource):
    def with_raw_response(self) -> VersionsWithRawResponse:
        return VersionsWithRawResponse(self)
    def with_streaming_response(self) -> VersionsWithStreamingResponse:
        return VersionsWithStreamingResponse(self)
        default: bool | Omit = omit,
    ) -> SkillVersion:
        Create a new immutable skill version.
          default: Whether to set this version as the default.
                "default": default,
                "files": files,
            path_template("/skills/{skill_id}/versions", skill_id=skill_id),
            body=maybe_transform(body, version_create_params.VersionCreateParams),
            cast_to=SkillVersion,
        Get a specific skill version.
          version: The version number to retrieve.
            path_template("/skills/{skill_id}/versions/{version}", skill_id=skill_id, version=version),
    ) -> SyncCursorPage[SkillVersion]:
        List skill versions for a skill.
          after: The skill version ID to start after.
          limit: Number of versions to retrieve.
          order: Sort order of results by version number.
            page=SyncCursorPage[SkillVersion],
                    version_list_params.VersionListParams,
            model=SkillVersion,
    ) -> DeletedSkillVersion:
        Delete a skill version.
            cast_to=DeletedSkillVersion,
class AsyncVersions(AsyncAPIResource):
    def with_raw_response(self) -> AsyncVersionsWithRawResponse:
        return AsyncVersionsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncVersionsWithStreamingResponse:
        return AsyncVersionsWithStreamingResponse(self)
            body=await async_maybe_transform(body, version_create_params.VersionCreateParams),
    ) -> AsyncPaginator[SkillVersion, AsyncCursorPage[SkillVersion]]:
            page=AsyncCursorPage[SkillVersion],
class VersionsWithRawResponse:
    def __init__(self, versions: Versions) -> None:
        self._versions = versions
            versions.create,
            versions.retrieve,
            versions.list,
            versions.delete,
        return ContentWithRawResponse(self._versions.content)
class AsyncVersionsWithRawResponse:
    def __init__(self, versions: AsyncVersions) -> None:
        return AsyncContentWithRawResponse(self._versions.content)
class VersionsWithStreamingResponse:
        return ContentWithStreamingResponse(self._versions.content)
class AsyncVersionsWithStreamingResponse:
        return AsyncContentWithStreamingResponse(self._versions.content)
def _get_sys_info():
    Get useful system information.
        Useful system information.
        "python": sys.version.replace(os.linesep, " "),
        "executable": sys.executable,
        "machine": platform.platform(),
def _get_deps_info():
    Get the versions of the dependencies.
        Versions of the dependencies.
    deps = ["cobyqa", "numpy", "scipy", "setuptools", "pip"]
    deps_info = {}
    for module in deps:
            deps_info[module] = version(module)
            deps_info[module] = None
    return deps_info
def show_versions():
    Display useful system and dependencies information.
    When reporting issues, please include this information.
    print("System settings")
    sys_info = _get_sys_info()
            f"{k:>{max(map(len, sys_info.keys())) + 1}}: {v}"
            for k, v in sys_info.items()
    print("Python dependencies")
    print("-------------------")
    deps_info = _get_deps_info()
            f"{k:>{max(map(len, deps_info.keys())) + 1}}: {v}"
            for k, v in deps_info.items()
        Create Skill Version
            f"/skills/{skill_id}/versions",
        Get Skill Version
            f"/skills/{skill_id}/versions/{version}",
        List Skill Versions
        Delete Skill Version
