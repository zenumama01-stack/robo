from typing import Dict, Optional
__all__ = ["EvalStoredCompletionsDataSourceConfig"]
class EvalStoredCompletionsDataSourceConfig(BaseModel):
    type: Literal["stored_completions"]
