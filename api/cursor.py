from math import inf
from tweepy.parsers import ModelParser, RawParser
class Cursor:
    """:class:`Cursor` can be used to paginate for any :class:`API` methods that
    support pagination
        :class:`API` method to paginate for
        Positional arguments to pass to ``method``
    kwargs
        Keyword arguments to pass to ``method``
    def __init__(self, method, *args, **kwargs):
        if hasattr(method, 'pagination_mode'):
            if method.pagination_mode == 'cursor':
                self.iterator = CursorIterator(method, *args, **kwargs)
            elif method.pagination_mode == 'dm_cursor':
                self.iterator = DMCursorIterator(method, *args, **kwargs)
            elif method.pagination_mode == 'id':
                self.iterator = IdIterator(method, *args, **kwargs)
            elif method.pagination_mode == "next":
                self.iterator = NextIterator(method, *args, **kwargs)
            elif method.pagination_mode == 'page':
                self.iterator = PageIterator(method, *args, **kwargs)
                raise TweepyException('Invalid pagination mode.')
            raise TweepyException('This method does not perform pagination')
    def pages(self, limit=inf):
        """Retrieve the page for each request
            Maximum number of pages to iterate over
        CursorIterator or DMCursorIterator or IdIterator or NextIterator or \
        PageIterator
            Iterator to iterate through pages
        self.iterator.limit = limit
        return self.iterator
    def items(self, limit=inf):
        """Retrieve the items in each page/request
            Maximum number of items to iterate over
        ItemIterator
            Iterator to iterate through items
        iterator = ItemIterator(self.iterator)
        iterator.limit = limit
        return iterator
class BaseIterator:
        self.method = method
        self.args = args
        self.kwargs = kwargs
        self.limit = inf
    def __next__(self):
        return self.next()
    def next(self):
    def prev(self):
    def __iter__(self):
class CursorIterator(BaseIterator):
        BaseIterator.__init__(self, method, *args, **kwargs)
        start_cursor = self.kwargs.pop('cursor', None)
        self.next_cursor = start_cursor or -1
        self.prev_cursor = start_cursor or 0
        self.num_tweets = 0
        if self.next_cursor == 0 or self.num_tweets >= self.limit:
            raise StopIteration
        data, cursors = self.method(cursor=self.next_cursor,
                                    *self.args,
                                    **self.kwargs)
        self.prev_cursor, self.next_cursor = cursors
        if len(data) == 0:
        self.num_tweets += 1
        if self.prev_cursor == 0:
            raise TweepyException('Can not page back more, at first page')
        data, self.next_cursor, self.prev_cursor = self.method(cursor=self.prev_cursor,
        self.num_tweets -= 1
class DMCursorIterator(BaseIterator):
        self.next_cursor = self.kwargs.pop('cursor', None)
        self.page_count = 0
        if self.next_cursor == -1 or self.page_count >= self.limit:
        data = self.method(cursor=self.next_cursor, return_cursors=True, *self.args, **self.kwargs)
        self.page_count += 1
        if isinstance(data, tuple):
            data, self.next_cursor = data
            self.next_cursor = -1
        raise TweepyException('This method does not allow backwards pagination')
class IdIterator(BaseIterator):
        self.max_id = self.kwargs.pop('max_id', None)
        self.results = []
        self.model_results = []
        self.index = 0
        """Fetch a set of items with IDs less than current set."""
        if self.num_tweets >= self.limit:
        if self.index >= len(self.results) - 1:
            data = self.method(max_id=self.max_id, parser=RawParser(), *self.args, **self.kwargs)
            model = ModelParser().parse(
                data, api = self.method.__self__,
                payload_list=self.method.payload_list,
                payload_type=self.method.payload_type
            result = self.method.__self__.parser.parse(
            if len(self.results) != 0:
                self.index += 1
            self.results.append(result)
            self.model_results.append(model)
            result = self.results[self.index]
            model = self.model_results[self.index]
        if len(result) == 0:
        # TODO: Make this not dependent on the parser making max_id and
        # since_id available
        self.max_id = model.max_id
        """Fetch a set of items with IDs greater than current set."""
        self.index -= 1
        if self.index < 0:
            # There's no way to fetch a set of tweets directly 'above' the
            # current set
        data = self.results[self.index]
        self.max_id = self.model_results[self.index].max_id
class PageIterator(BaseIterator):
        self.current_page = 1
        # Keep track of previous page of items to handle Twitter API issue with
        # duplicate pages
        # https://twittercommunity.com/t/odd-pagination-behavior-with-get-users-search/148502
        # https://github.com/tweepy/tweepy/issues/1465
        # https://github.com/tweepy/tweepy/issues/958
        self.previous_items = []
        if self.current_page > self.limit:
        items = self.method(page=self.current_page, *self.args, **self.kwargs)
        if len(items) == 0:
        for item in items:
            if item in self.previous_items:
        self.current_page += 1
        self.previous_items = items
        return items
        if self.current_page == 1:
        self.current_page -= 1
        return self.method(page=self.current_page, *self.args, **self.kwargs)
class NextIterator(BaseIterator):
        self.next_token = self.kwargs.pop('next', None)
        if self.next_token == -1 or self.page_count >= self.limit:
        data = self.method(next=self.next_token, return_cursors=True, *self.args, **self.kwargs)
            data, self.next_token = data
            self.next_token = -1
class ItemIterator(BaseIterator):
    def __init__(self, page_iterator):
        self.page_iterator = page_iterator
        self.current_page = None
        self.page_index = -1
        if self.current_page is None or self.page_index == len(self.current_page) - 1:
            # Reached end of current page, get the next page...
            self.current_page = next(self.page_iterator)
            while len(self.current_page) == 0:
        self.page_index += 1
        return self.current_page[self.page_index]
        if self.current_page is None:
            raise TweepyException('Can not go back more, at first page')
        if self.page_index == 0:
            # At the beginning of the current page, move to next...
            self.current_page = self.page_iterator.prev()
            self.page_index = len(self.current_page)
                raise TweepyException('No more items')
        self.page_index -= 1
