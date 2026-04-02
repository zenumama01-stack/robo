__all__ = ["RealtimeTruncationRetentionRatioParam", "TokenLimits"]
class TokenLimits(TypedDict, total=False):
    post_instructions: int
class RealtimeTruncationRetentionRatioParam(TypedDict, total=False):
    retention_ratio: Required[float]
    type: Required[Literal["retention_ratio"]]
    token_limits: TokenLimits
