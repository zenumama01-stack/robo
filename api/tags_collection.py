class TagsCollectionRequest(CollectionRequestBase):
        """Initialize the TagsCollectionRequest
            request_url (str): The url to perform the TagsCollectionRequest
        super(TagsCollectionRequest, self).__init__(request_url, client, options)
        """Gets the TagsCollectionPage
            :class:`TagsCollectionPage<onedrivesdk.model.tags_collection_page.TagsCollectionPage>`:
                The TagsCollectionPage
        collection_response = TagsCollectionResponse(json.loads(self.send().content))
        """Gets the TagsCollectionRequest for the next page. Returns None if there is no next page
            collection_page (:class:`TagsCollectionPage<onedrivesdk.model.tags_collection_page.TagsCollectionPage>`):
            :class:`TagsCollectionRequest<onedrivesdk.request.tags_collection.TagsCollectionRequest>`:
                The TagsCollectionRequest
            return TagsCollectionRequest(collection_page._next_page_link, client, options)
class TagsCollectionRequestBuilder(RequestBuilderBase):
        """Get the TagRequestBuilder with the specified key
            key (str): The key to get a TagRequestBuilder for
            :class:`TagRequestBuilder<onedrivesdk.request.tag_request_builder.TagRequestBuilder>`:
                A TagRequestBuilder for that key
        return TagRequestBuilder(self.append_to_request_url(str(key)), self._client)
        """Builds the TagsCollectionRequest
        req = TagsCollectionRequest(self._request_url, self._client, options)
class TagsCollectionResponse(CollectionResponseBase):
            self._collection_page = TagsCollectionPage(self._prop_dict["value"])
from ..request.tag_request_builder import TagRequestBuilder
        """Gets the TagsCollectionPage in async
