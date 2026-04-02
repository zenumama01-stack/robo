    "ResponseOutputText",
    "AnnotationFileCitation",
    "AnnotationURLCitation",
    "AnnotationContainerFileCitation",
    "AnnotationFilePath",
    "LogprobTopLogprob",
class AnnotationFileCitation(BaseModel):
    """A citation to a file."""
    """The ID of the file."""
    """The filename of the file cited."""
    """The index of the file in the list of files."""
    """The type of the file citation. Always `file_citation`."""
    """A citation for a web resource used to generate a model response."""
class AnnotationContainerFileCitation(BaseModel):
    """A citation for a container file used to generate a model response."""
    """The ID of the container file."""
    """The index of the last character of the container file citation in the message."""
    """The filename of the container file cited."""
    """The index of the first character of the container file citation in the message."""
    type: Literal["container_file_citation"]
    """The type of the container file citation. Always `container_file_citation`."""
class AnnotationFilePath(BaseModel):
    """A path to a file."""
    """The type of the file path. Always `file_path`."""
Annotation: TypeAlias = Annotated[
    Union[AnnotationFileCitation, AnnotationURLCitation, AnnotationContainerFileCitation, AnnotationFilePath],
class LogprobTopLogprob(BaseModel):
    """The top log probability of a token."""
    """The log probability of a token."""
    top_logprobs: List[LogprobTopLogprob]
class ResponseOutputText(BaseModel):
    """The annotations of the text output."""
