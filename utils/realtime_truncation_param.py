from .realtime_truncation_retention_ratio_param import RealtimeTruncationRetentionRatioParam
__all__ = ["RealtimeTruncationParam"]
RealtimeTruncationParam: TypeAlias = Union[Literal["auto", "disabled"], RealtimeTruncationRetentionRatioParam]
