class SpecialCollectionRequest(CollectionRequestBase):
        """Initialize the SpecialCollectionRequest
            request_url (str): The url to perform the SpecialCollectionRequest
        super(SpecialCollectionRequest, self).__init__(request_url, client, options)
        """Gets the SpecialCollectionPage
            :class:`SpecialCollectionPage<onedrivesdk.model.special_collection_page.SpecialCollectionPage>`:
                The SpecialCollectionPage
        collection_response = SpecialCollectionResponse(json.loads(self.send().content))
        """Gets the SpecialCollectionRequest for the next page. Returns None if there is no next page
            collection_page (:class:`SpecialCollectionPage<onedrivesdk.model.special_collection_page.SpecialCollectionPage>`):
            :class:`SpecialCollectionRequest<onedrivesdk.request.special_collection.SpecialCollectionRequest>`:
                The SpecialCollectionRequest
            return SpecialCollectionRequest(collection_page._next_page_link, client, options)
class SpecialCollectionRequestBuilder(RequestBuilderBase):
        """Builds the SpecialCollectionRequest
        req = SpecialCollectionRequest(self._request_url, self._client, options)
class SpecialCollectionResponse(CollectionResponseBase):
            self._collection_page = SpecialCollectionPage(self._prop_dict["value"])
        """Gets the SpecialCollectionPage in async
