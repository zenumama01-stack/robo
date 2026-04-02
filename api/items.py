from typing import Any, List, Iterable, cast
from ...pagination import SyncConversationCursorPage, AsyncConversationCursorPage
from ...types.conversations import item_list_params, item_create_params, item_retrieve_params
from ...types.responses.response_includable import ResponseIncludable
from ...types.conversations.conversation_item import ConversationItem
from ...types.conversations.conversation_item_list import ConversationItemList
__all__ = ["Items", "AsyncItems"]
class Items(SyncAPIResource):
    def with_raw_response(self) -> ItemsWithRawResponse:
        return ItemsWithRawResponse(self)
    def with_streaming_response(self) -> ItemsWithStreamingResponse:
        return ItemsWithStreamingResponse(self)
        items: Iterable[ResponseInputItemParam],
        include: List[ResponseIncludable] | Omit = omit,
    ) -> ConversationItemList:
        Create items in a conversation with the given ID.
          items: The items to add to the conversation. You may add up to 20 items at a time.
          include: Additional fields to include in the response. See the `include` parameter for
              [listing Conversation items above](https://platform.openai.com/docs/api-reference/conversations/list-items#conversations_list_items-include)
            path_template("/conversations/{conversation_id}/items", conversation_id=conversation_id),
            body=maybe_transform({"items": items}, item_create_params.ItemCreateParams),
                query=maybe_transform({"include": include}, item_create_params.ItemCreateParams),
            cast_to=ConversationItemList,
        item_id: str,
    ) -> ConversationItem:
        Get a single item from a conversation with the given IDs.
        if not item_id:
            raise ValueError(f"Expected a non-empty value for `item_id` but received {item_id!r}")
            ConversationItem,
            self._get(
                    "/conversations/{conversation_id}/items/{item_id}", conversation_id=conversation_id, item_id=item_id
                    query=maybe_transform({"include": include}, item_retrieve_params.ItemRetrieveParams),
                cast_to=cast(Any, ConversationItem),  # Union types cannot be passed in as arguments in the type system
    ) -> SyncConversationCursorPage[ConversationItem]:
        List all items for a conversation with the given ID.
          after: An item ID to list items after, used in pagination.
          include: Specify additional output data to include in the model response. Currently
              supported values are:
              - `web_search_call.action.sources`: Include the sources of the web search tool
                call.
              - `code_interpreter_call.outputs`: Includes the outputs of python code execution
                in code interpreter tool call items.
              - `computer_call_output.output.image_url`: Include image urls from the computer
                call output.
              - `file_search_call.results`: Include the search results of the file search tool
              - `message.input_image.image_url`: Include image urls from the input message.
              - `message.output_text.logprobs`: Include logprobs with assistant messages.
              - `reasoning.encrypted_content`: Includes an encrypted version of reasoning
                tokens in reasoning item outputs. This enables reasoning items to be used in
                multi-turn conversations when using the Responses API statelessly (like when
                the `store` parameter is set to `false`, or when an organization is enrolled
                in the zero data retention program).
          order: The order to return the input items in. Default is `desc`.
              - `asc`: Return the input items in ascending order.
              - `desc`: Return the input items in descending order.
            page=SyncConversationCursorPage[ConversationItem],
                    item_list_params.ItemListParams,
            model=cast(Any, ConversationItem),  # Union types cannot be passed in as arguments in the type system
        Delete an item from a conversation with the given IDs.
class AsyncItems(AsyncAPIResource):
    def with_raw_response(self) -> AsyncItemsWithRawResponse:
        return AsyncItemsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncItemsWithStreamingResponse:
        return AsyncItemsWithStreamingResponse(self)
            body=await async_maybe_transform({"items": items}, item_create_params.ItemCreateParams),
                query=await async_maybe_transform({"include": include}, item_create_params.ItemCreateParams),
            await self._get(
                    query=await async_maybe_transform({"include": include}, item_retrieve_params.ItemRetrieveParams),
    ) -> AsyncPaginator[ConversationItem, AsyncConversationCursorPage[ConversationItem]]:
            page=AsyncConversationCursorPage[ConversationItem],
class ItemsWithRawResponse:
    def __init__(self, items: Items) -> None:
        self._items = items
            items.create,
            items.retrieve,
            items.list,
            items.delete,
class AsyncItemsWithRawResponse:
    def __init__(self, items: AsyncItems) -> None:
class ItemsWithStreamingResponse:
class AsyncItemsWithStreamingResponse:
from fastapi import APIRouter, Depends, HTTPException
from ..dependencies import get_token_header
router = APIRouter(
    prefix="/items",
    tags=["items"],
    responses={404: {"description": "Not found"}},
fake_items_db = {"plumbus": {"name": "Plumbus"}, "gun": {"name": "Portal Gun"}}
@router.get("/")
async def read_items():
    return fake_items_db
@router.get("/{item_id}")
async def read_item(item_id: str):
    if item_id not in fake_items_db:
    return {"name": fake_items_db[item_id]["name"], "item_id": item_id}
@router.put(
    "/{item_id}",
    tags=["custom"],
    responses={403: {"description": "Operation forbidden"}},
async def update_item(item_id: str):
    if item_id != "plumbus":
        raise HTTPException(
            status_code=403, detail="You can only update the item: plumbus"
    return {"item_id": item_id, "name": "The great Plumbus"}
            f"/conversations/{conversation_id}/items",
                f"/conversations/{conversation_id}/items/{item_id}",
