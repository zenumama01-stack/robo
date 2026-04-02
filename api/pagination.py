from tweepy.client import Response
class Paginator:
    """Paginator( \
        self, method, *args, limit=inf, pagination_token=None, **kwargs \
    :class:`Paginator` can be used to paginate for any :class:`Client`
    methods that support pagination
        When the returned response from the method being passed is of type
        :class:`requests.Response`, it will be deserialized in order to parse
        the pagination tokens, likely negating any potential performance
        benefits from using a :class:`requests.Response` return type.
        :class:`Client` method to paginate for
        Maximum number of requests to make to the API
    pagination_token
        Pagination token to start pagination with
        return PaginationIterator(self.method, *self.args, **self.kwargs)
    def __reversed__(self):
        return PaginationIterator(self.method, *self.args, reverse=True,
    def flatten(self, limit=inf):
        """Flatten paginated data
            Maximum number of results to yield
        if limit <= 0:
        count = 0
        for response in PaginationIterator(
            self.method, *self.args, **self.kwargs
            if isinstance(response, Response):
                response_data = response.data or []
            elif isinstance(response, dict):
                response_data = response.get("data", [])
                raise RuntimeError(
                    f"Paginator.flatten does not support the {type(response)} "
                    f"return type for {self.method.__qualname__}"
            for data in response_data:
                yield data
                if count == limit:
class PaginationIterator:
    def __init__(self, method, *args, limit=inf, pagination_token=None,
                 reverse=False, **kwargs):
        self.limit = limit
        self.reverse = reverse
        if reverse:
            self.previous_token = pagination_token
            self.next_token = None
            self.previous_token = None
            self.next_token = pagination_token
        self.count = 0
        if self.reverse:
            pagination_token = self.previous_token
            pagination_token = self.next_token
        if self.count >= self.limit or self.count and pagination_token is None:
        # https://twittercommunity.com/t/why-does-timeline-use-pagination-token-while-search-uses-next-token/150963
        if self.method.__name__ in (
            "search_all_tweets", "search_recent_tweets",
            "get_all_tweets_count"
            self.kwargs["next_token"] = pagination_token
            self.kwargs["pagination_token"] = pagination_token
        response = self.method(*self.args, **self.kwargs)
            meta = response.meta
        elif isinstance(response, requests.Response):
            meta = response.json().get("meta", {})
                f"Unknown {type(response)} return type for "
                f"{self.method.__qualname__}"
        self.previous_token = meta.get("previous_token")
        self.next_token = meta.get("next_token")
        self.count += 1
class AsyncPaginator:
    """AsyncPaginator( \
    :class:`AsyncPaginator` can be used to paginate for any
    :class:`AsyncClient` methods that support pagination
        :class:`aiohttp.ClientResponse`, it will be deserialized in order to
        parse the pagination tokens, likely negating any potential performance
        benefits from using a :class:`aiohttp.ClientResponse` return type.
    .. versionadded:: 4.11
        :class:`AsyncClient` method to paginate for
    def __aiter__(self):
        return AsyncPaginationIterator(self.method, *self.args, **self.kwargs)
        return AsyncPaginationIterator(
            self.method, *self.args, reverse=True, **self.kwargs
    async def flatten(self, limit=inf):
        async for response in AsyncPaginationIterator(
                    "AsyncPaginator.flatten does not support the "
                    f"{type(response)} return type for "
class AsyncPaginationIterator:
        self, method, *args, limit=inf, pagination_token=None, reverse=False,
    async def __anext__(self):
            raise StopAsyncIteration
        response = await self.method(*self.args, **self.kwargs)
        elif isinstance(response, aiohttp.ClientResponse):
            meta = (await response.json()).get("meta", {})
from typing import Any, List, Generic, TypeVar, Optional, cast
from typing_extensions import Protocol, override, runtime_checkable
from ._base_client import BasePage, PageInfo, BaseSyncPage, BaseAsyncPage
    "SyncPage",
    "AsyncPage",
    "SyncCursorPage",
    "AsyncCursorPage",
    "SyncConversationCursorPage",
    "AsyncConversationCursorPage",
class CursorPageItem(Protocol):
    id: Optional[str]
class SyncPage(BaseSyncPage[_T], BasePage[_T], Generic[_T]):
    """Note: no pagination actually occurs yet, this is for forwards-compatibility."""
    data: List[_T]
    object: str
    def _get_page_items(self) -> List[_T]:
        data = self.data
        if not data:
    def next_page_info(self) -> None:
        This page represents a response that isn't actually paginated at the API level
        so there will never be a next page.
class AsyncPage(BaseAsyncPage[_T], BasePage[_T], Generic[_T]):
class SyncCursorPage(BaseSyncPage[_T], BasePage[_T], Generic[_T]):
    has_more: Optional[bool] = None
        has_more = self.has_more
        if has_more is not None and has_more is False:
        return super().has_next_page()
    def next_page_info(self) -> Optional[PageInfo]:
        item = cast(Any, data[-1])
        if not isinstance(item, CursorPageItem) or item.id is None:
            # TODO emit warning log
        return PageInfo(params={"after": item.id})
class AsyncCursorPage(BaseAsyncPage[_T], BasePage[_T], Generic[_T]):
class SyncConversationCursorPage(BaseSyncPage[_T], BasePage[_T], Generic[_T]):
    last_id: Optional[str] = None
        last_id = self.last_id
        if not last_id:
        return PageInfo(params={"after": last_id})
class AsyncConversationCursorPage(BaseAsyncPage[_T], BasePage[_T], Generic[_T]):
"""Queries with paginated results against the shopping search API"""
    input = raw_input
    """Get and print a the entire paginated feed of public products in the United
    States.
    Pagination is controlled with the "startIndex" parameter passed to the list
    method of the resource.
    # The first request contains the information we need for the total items, and
    # page size, as well as returning the first page of results.
    itemsPerPage = response["itemsPerPage"]
    totalItems = response["totalItems"]
    for i in range(1, totalItems, itemsPerPage):
            "About to display results from %s to %s, y/(n)? " % (i, i + itemsPerPage)
        if answer.strip().lower().startswith("n"):
            # Stop if the user has had enough
            # Fetch this series of results
                source="public", country="US", q="digital camera", startIndex=i
