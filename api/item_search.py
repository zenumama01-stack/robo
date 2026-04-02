class ItemSearchRequest(CollectionRequestBase):
    def __init__(self, request_url, client, options, q=None):
        super(ItemSearchRequest, self).__init__(request_url, client, options)
        if q:
            self._query_options["q"] = q
    def get_next_page_request(collection_page, client, options, q=None):
        """Gets the ItemSearchRequest for the next page. Returns None if there is no next page
            :class:`ItemSearchRequest<onedrivesdk.request.item_search.ItemSearchRequest>`:
                The ItemSearchRequest
            return ItemSearchRequest(collection_page._next_page_link, client, options, token)
class ItemSearchRequestBuilder(RequestBuilderBase):
    def __init__(self, request_url, client, q=None):
        super(ItemSearchRequestBuilder, self).__init__(request_url, client)
        self._method_options["q"] = q
        """Builds the request for the ItemSearch
        req = ItemSearchRequest(self._request_url, self._client, options, q=self._method_options["q"])
