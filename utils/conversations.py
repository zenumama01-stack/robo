from typing import Iterable, Optional
from ...types.conversations import conversation_create_params, conversation_update_params
from ...types.conversations.conversation import Conversation
from ...types.responses.response_input_item_param import ResponseInputItemParam
from ...types.conversations.conversation_deleted_resource import ConversationDeletedResource
__all__ = ["Conversations", "AsyncConversations"]
class Conversations(SyncAPIResource):
    def items(self) -> Items:
        return Items(self._client)
    def with_raw_response(self) -> ConversationsWithRawResponse:
        return ConversationsWithRawResponse(self)
    def with_streaming_response(self) -> ConversationsWithStreamingResponse:
        return ConversationsWithStreamingResponse(self)
        items: Optional[Iterable[ResponseInputItemParam]] | Omit = omit,
    ) -> Conversation:
        Create a conversation.
          items: Initial items to include in the conversation context. You may add up to 20 items
              at a time.
            "/conversations",
                    "items": items,
                conversation_create_params.ConversationCreateParams,
            cast_to=Conversation,
        conversation_id: str,
        Get a conversation
        if not conversation_id:
            raise ValueError(f"Expected a non-empty value for `conversation_id` but received {conversation_id!r}")
            path_template("/conversations/{conversation_id}", conversation_id=conversation_id),
        Update a conversation
            body=maybe_transform({"metadata": metadata}, conversation_update_params.ConversationUpdateParams),
    ) -> ConversationDeletedResource:
        """Delete a conversation.
        Items in the conversation will not be deleted.
            cast_to=ConversationDeletedResource,
class AsyncConversations(AsyncAPIResource):
    def items(self) -> AsyncItems:
        return AsyncItems(self._client)
    def with_raw_response(self) -> AsyncConversationsWithRawResponse:
        return AsyncConversationsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncConversationsWithStreamingResponse:
        return AsyncConversationsWithStreamingResponse(self)
                {"metadata": metadata}, conversation_update_params.ConversationUpdateParams
class ConversationsWithRawResponse:
    def __init__(self, conversations: Conversations) -> None:
        self._conversations = conversations
            conversations.create,
            conversations.retrieve,
            conversations.update,
            conversations.delete,
    def items(self) -> ItemsWithRawResponse:
        return ItemsWithRawResponse(self._conversations.items)
class AsyncConversationsWithRawResponse:
    def __init__(self, conversations: AsyncConversations) -> None:
    def items(self) -> AsyncItemsWithRawResponse:
        return AsyncItemsWithRawResponse(self._conversations.items)
class ConversationsWithStreamingResponse:
    def items(self) -> ItemsWithStreamingResponse:
        return ItemsWithStreamingResponse(self._conversations.items)
class AsyncConversationsWithStreamingResponse:
    def items(self) -> AsyncItemsWithStreamingResponse:
        return AsyncItemsWithStreamingResponse(self._conversations.items)
            f"/conversations/{conversation_id}",
