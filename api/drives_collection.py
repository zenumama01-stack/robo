from ..model.drives_collection_page import DrivesCollectionPage
class DrivesCollectionRequest(CollectionRequestBase):
        """Initialize the DrivesCollectionRequest
            request_url (str): The url to perform the DrivesCollectionRequest
        super(DrivesCollectionRequest, self).__init__(request_url, client, options)
        """Gets the DrivesCollectionPage
            :class:`DrivesCollectionPage<onedrivesdk.model.drives_collection_page.DrivesCollectionPage>`:
                The DrivesCollectionPage
        collection_response = DrivesCollectionResponse(json.loads(self.send().content))
        """Gets the DrivesCollectionRequest for the next page. Returns None if there is no next page
            collection_page (:class:`DrivesCollectionPage<onedrivesdk.model.drives_collection_page.DrivesCollectionPage>`):
            :class:`DrivesCollectionRequest<onedrivesdk.request.drives_collection.DrivesCollectionRequest>`:
                The DrivesCollectionRequest
            return DrivesCollectionRequest(collection_page._next_page_link, client, options)
class DrivesCollectionRequestBuilder(RequestBuilderBase):
        """Get the DriveRequestBuilder with the specified key
            key (str): The key to get a DriveRequestBuilder for
            :class:`DriveRequestBuilder<onedrivesdk.request.drive_request_builder.DriveRequestBuilder>`:
                A DriveRequestBuilder for that key
        return DriveRequestBuilder(self.append_to_request_url(str(key)), self._client)
        """Builds the DrivesCollectionRequest
        req = DrivesCollectionRequest(self._request_url, self._client, options)
class DrivesCollectionResponse(CollectionResponseBase):
            self._collection_page = DrivesCollectionPage(self._prop_dict["value"])
        """Gets the DrivesCollectionPage in async
