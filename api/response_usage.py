__all__ = ["ResponseUsage", "InputTokensDetails", "OutputTokensDetails"]
class ResponseUsage(BaseModel):
    Represents token usage details including input tokens, output tokens,
    a breakdown of output tokens, and the total tokens used.
