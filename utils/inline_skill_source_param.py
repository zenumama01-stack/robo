__all__ = ["InlineSkillSourceParam"]
class InlineSkillSourceParam(TypedDict, total=False):
    media_type: Required[Literal["application/zip"]]
    type: Required[Literal["base64"]]
