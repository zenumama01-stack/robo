from ..model.shares_collection_page import SharesCollectionPage
class SharesCollectionRequest(CollectionRequestBase):
        """Initialize the SharesCollectionRequest
            request_url (str): The url to perform the SharesCollectionRequest
        super(SharesCollectionRequest, self).__init__(request_url, client, options)
        """Gets the SharesCollectionPage
            :class:`SharesCollectionPage<onedrivesdk.model.shares_collection_page.SharesCollectionPage>`:
                The SharesCollectionPage
        collection_response = SharesCollectionResponse(json.loads(self.send().content))
        """Gets the SharesCollectionRequest for the next page. Returns None if there is no next page
            collection_page (:class:`SharesCollectionPage<onedrivesdk.model.shares_collection_page.SharesCollectionPage>`):
            :class:`SharesCollectionRequest<onedrivesdk.request.shares_collection.SharesCollectionRequest>`:
                The SharesCollectionRequest
            return SharesCollectionRequest(collection_page._next_page_link, client, options)
class SharesCollectionRequestBuilder(RequestBuilderBase):
        """Get the ShareRequestBuilder with the specified key
            key (str): The key to get a ShareRequestBuilder for
            :class:`ShareRequestBuilder<onedrivesdk.request.share_request_builder.ShareRequestBuilder>`:
                A ShareRequestBuilder for that key
        return ShareRequestBuilder(self.append_to_request_url(str(key)), self._client)
        """Builds the SharesCollectionRequest
        req = SharesCollectionRequest(self._request_url, self._client, options)
class SharesCollectionResponse(CollectionResponseBase):
            self._collection_page = SharesCollectionPage(self._prop_dict["value"])
from ..request.share_request_builder import ShareRequestBuilder
        """Gets the SharesCollectionPage in async
