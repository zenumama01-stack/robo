__all__ = ["ChatCompletionPredictionContentParam"]
class ChatCompletionPredictionContentParam(TypedDict, total=False):
    Static predicted output content, such as the content of a text file that is
    The content that should be matched when generating a model response. If
    generated tokens would match this content, the entire model response can be
    returned much more quickly.
    type: Required[Literal["content"]]
    """The type of the predicted content you want to provide.
    This type is currently always `content`.
