class ItemsCollectionRequest(CollectionRequestBase):
        """Initialize the ItemsCollectionRequest
            request_url (str): The url to perform the ItemsCollectionRequest
        super(ItemsCollectionRequest, self).__init__(request_url, client, options)
        """Gets the ItemsCollectionPage
            :class:`ItemsCollectionPage<onedrivesdk.model.items_collection_page.ItemsCollectionPage>`:
                The ItemsCollectionPage
        """Gets the ItemsCollectionRequest for the next page. Returns None if there is no next page
            collection_page (:class:`ItemsCollectionPage<onedrivesdk.model.items_collection_page.ItemsCollectionPage>`):
            :class:`ItemsCollectionRequest<onedrivesdk.request.items_collection.ItemsCollectionRequest>`:
                The ItemsCollectionRequest
            return ItemsCollectionRequest(collection_page._next_page_link, client, options)
class ItemsCollectionRequestBuilder(RequestBuilderBase):
        """Builds the ItemsCollectionRequest
        req = ItemsCollectionRequest(self._request_url, self._client, options)
class ItemsCollectionResponse(CollectionResponseBase):
            self._collection_page = ItemsCollectionPage(self._prop_dict["value"])
        """Gets the ItemsCollectionPage in async
