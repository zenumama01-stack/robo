    "ResponseOutputTextParam",
class AnnotationFileCitation(TypedDict, total=False):
    index: Required[int]
    type: Required[Literal["file_citation"]]
class AnnotationURLCitation(TypedDict, total=False):
    end_index: Required[int]
    start_index: Required[int]
    title: Required[str]
    type: Required[Literal["url_citation"]]
class AnnotationContainerFileCitation(TypedDict, total=False):
    type: Required[Literal["container_file_citation"]]
class AnnotationFilePath(TypedDict, total=False):
    type: Required[Literal["file_path"]]
Annotation: TypeAlias = Union[
    AnnotationFileCitation, AnnotationURLCitation, AnnotationContainerFileCitation, AnnotationFilePath
class LogprobTopLogprob(TypedDict, total=False):
    token: Required[str]
    bytes: Required[Iterable[int]]
    logprob: Required[float]
class Logprob(TypedDict, total=False):
    top_logprobs: Required[Iterable[LogprobTopLogprob]]
class ResponseOutputTextParam(TypedDict, total=False):
    annotations: Required[Iterable[Annotation]]
    logprobs: Iterable[Logprob]
