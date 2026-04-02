from typing import Dict, Union, Iterable
    "CreateEvalJSONLRunDataSourceParam",
Source: TypeAlias = Union[SourceFileContent, SourceFileID]
class CreateEvalJSONLRunDataSourceParam(TypedDict, total=False):
    type: Required[Literal["jsonl"]]
