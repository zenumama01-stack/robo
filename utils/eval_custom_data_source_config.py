from typing import Dict
__all__ = ["EvalCustomDataSourceConfig"]
class EvalCustomDataSourceConfig(BaseModel):
    A CustomDataSourceConfig which specifies the schema of your `item` and optionally `sample` namespaces.
    The response schema defines the shape of the data that will be:
    type: Literal["custom"]
