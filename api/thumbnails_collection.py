class ThumbnailsCollectionRequest(CollectionRequestBase):
        """Initialize the ThumbnailsCollectionRequest
            request_url (str): The url to perform the ThumbnailsCollectionRequest
        super(ThumbnailsCollectionRequest, self).__init__(request_url, client, options)
        """Gets the ThumbnailsCollectionPage
            :class:`ThumbnailsCollectionPage<onedrivesdk.model.thumbnails_collection_page.ThumbnailsCollectionPage>`:
                The ThumbnailsCollectionPage
        collection_response = ThumbnailsCollectionResponse(json.loads(self.send().content))
        """Gets the ThumbnailsCollectionRequest for the next page. Returns None if there is no next page
            collection_page (:class:`ThumbnailsCollectionPage<onedrivesdk.model.thumbnails_collection_page.ThumbnailsCollectionPage>`):
            :class:`ThumbnailsCollectionRequest<onedrivesdk.request.thumbnails_collection.ThumbnailsCollectionRequest>`:
                The ThumbnailsCollectionRequest
            return ThumbnailsCollectionRequest(collection_page._next_page_link, client, options)
class ThumbnailsCollectionRequestBuilder(RequestBuilderBase):
        """Get the ThumbnailSetRequestBuilder with the specified key
            key (str): The key to get a ThumbnailSetRequestBuilder for
            :class:`ThumbnailSetRequestBuilder<onedrivesdk.request.thumbnail_set_request_builder.ThumbnailSetRequestBuilder>`:
                A ThumbnailSetRequestBuilder for that key
        return ThumbnailSetRequestBuilder(self.append_to_request_url(str(key)), self._client)
        """Builds the ThumbnailsCollectionRequest
        req = ThumbnailsCollectionRequest(self._request_url, self._client, options)
class ThumbnailsCollectionResponse(CollectionResponseBase):
            self._collection_page = ThumbnailsCollectionPage(self._prop_dict["value"])
from ..request.thumbnail_set_request_builder import ThumbnailSetRequestBuilder
        """Gets the ThumbnailsCollectionPage in async
