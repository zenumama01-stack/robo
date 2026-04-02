from .embedding_model import EmbeddingModel
__all__ = ["EmbeddingCreateParams"]
class EmbeddingCreateParams(TypedDict, total=False):
    input: Required[Union[str, SequenceNotStr[str], Iterable[int], Iterable[Iterable[int]]]]
    """Input text to embed, encoded as a string or array of tokens.
    To embed multiple inputs in a single request, pass an array of strings or array
    of token arrays. The input must not exceed the max input tokens for the model
    (8192 tokens for all embedding models), cannot be an empty string, and any array
    must be 2048 dimensions or less.
    model: Required[Union[str, EmbeddingModel]]
    dimensions: int
    """The number of dimensions the resulting output embeddings should have.
    Only supported in `text-embedding-3` and later models.
    encoding_format: Literal["float", "base64"]
    """The format to return the embeddings in.
    Can be either `float` or [`base64`](https://pypi.org/project/pybase64/).
