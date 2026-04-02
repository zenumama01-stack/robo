class VersionsCollectionRequest(CollectionRequestBase):
        """Initialize the VersionsCollectionRequest
            request_url (str): The url to perform the VersionsCollectionRequest
        super(VersionsCollectionRequest, self).__init__(request_url, client, options)
        """Gets the VersionsCollectionPage
            :class:`VersionsCollectionPage<onedrivesdk.model.versions_collection_page.VersionsCollectionPage>`:
                The VersionsCollectionPage
        collection_response = VersionsCollectionResponse(json.loads(self.send().content))
        """Gets the VersionsCollectionRequest for the next page. Returns None if there is no next page
            collection_page (:class:`VersionsCollectionPage<onedrivesdk.model.versions_collection_page.VersionsCollectionPage>`):
            :class:`VersionsCollectionRequest<onedrivesdk.request.versions_collection.VersionsCollectionRequest>`:
                The VersionsCollectionRequest
            return VersionsCollectionRequest(collection_page._next_page_link, client, options)
class VersionsCollectionRequestBuilder(RequestBuilderBase):
        """Builds the VersionsCollectionRequest
        req = VersionsCollectionRequest(self._request_url, self._client, options)
class VersionsCollectionResponse(CollectionResponseBase):
            self._collection_page = VersionsCollectionPage(self._prop_dict["value"])
        """Gets the VersionsCollectionPage in async
