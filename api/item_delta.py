class ItemDeltaRequest(CollectionRequestBase):
    def __init__(self, request_url, client, options, token=None):
        super(ItemDeltaRequest, self).__init__(request_url, client, options)
            self._query_options["token"] = token
            :class:`ItemDeltaCollectionResponse<onedrivesdk.request.item_delta_collection.ItemDeltaCollectionResponse>`:
        collection_response = ItemDeltaCollectionResponse(json.loads(self.send().content))
    def get_next_page_request(collection_page, client, options, token=None):
        """Gets the ItemDeltaRequest for the next page. Returns None if there is no next page
            :class:`ItemDeltaRequest<onedrivesdk.request.item_delta.ItemDeltaRequest>`:
                The ItemDeltaRequest
            return ItemDeltaRequest(collection_page._next_page_link, client, options, token)
class ItemDeltaRequestBuilder(RequestBuilderBase):
    def __init__(self, request_url, client, token=None):
        super(ItemDeltaRequestBuilder, self).__init__(request_url, client)
        self._method_options["token"] = token
        """Builds the request for the ItemDelta
        req = ItemDeltaRequest(self._request_url, self._client, options, token=self._method_options["token"])
            The resulting ItemDeltaCollectionResponse from the operation
from ..request.item_delta_collection import ItemDeltaCollectionResponse
