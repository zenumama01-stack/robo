__all__ = ["FunctionToolCallDelta", "Function"]
class FunctionToolCallDelta(BaseModel):
    function: Optional[Function] = None
