__all__ = ["ChatCompletionFunctionCallOptionParam"]
class ChatCompletionFunctionCallOptionParam(TypedDict, total=False):
    Specifying a particular function via `{"name": "my_function"}` forces the model to call that function.
