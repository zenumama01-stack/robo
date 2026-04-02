from .realtime_truncation_retention_ratio import RealtimeTruncationRetentionRatio
__all__ = ["RealtimeTruncation"]
RealtimeTruncation: TypeAlias = Union[Literal["auto", "disabled"], RealtimeTruncationRetentionRatio]
