class SharedCollectionRequest(CollectionRequestBase):
        """Initialize the SharedCollectionRequest
            request_url (str): The url to perform the SharedCollectionRequest
        super(SharedCollectionRequest, self).__init__(request_url, client, options)
        """Gets the SharedCollectionPage
            :class:`SharedCollectionPage<onedrivesdk.model.shared_collection_page.SharedCollectionPage>`:
                The SharedCollectionPage
        collection_response = SharedCollectionResponse(json.loads(self.send().content))
        """Gets the SharedCollectionRequest for the next page. Returns None if there is no next page
            collection_page (:class:`SharedCollectionPage<onedrivesdk.model.shared_collection_page.SharedCollectionPage>`):
            :class:`SharedCollectionRequest<onedrivesdk.request.shared_collection.SharedCollectionRequest>`:
                The SharedCollectionRequest
            return SharedCollectionRequest(collection_page._next_page_link, client, options)
class SharedCollectionRequestBuilder(RequestBuilderBase):
        """Builds the SharedCollectionRequest
        req = SharedCollectionRequest(self._request_url, self._client, options)
class SharedCollectionResponse(CollectionResponseBase):
            self._collection_page = SharedCollectionPage(self._prop_dict["value"])
        """Gets the SharedCollectionPage in async
