from .chat_model import ChatModel
__all__ = ["AllModels"]
AllModels: TypeAlias = Union[
    ChatModel,
