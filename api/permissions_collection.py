class PermissionsCollectionRequest(CollectionRequestBase):
        """Initialize the PermissionsCollectionRequest
            request_url (str): The url to perform the PermissionsCollectionRequest
        super(PermissionsCollectionRequest, self).__init__(request_url, client, options)
        """Gets the PermissionsCollectionPage
            :class:`PermissionsCollectionPage<onedrivesdk.model.permissions_collection_page.PermissionsCollectionPage>`:
                The PermissionsCollectionPage
        collection_response = PermissionsCollectionResponse(json.loads(self.send().content))
        """Gets the PermissionsCollectionRequest for the next page. Returns None if there is no next page
            collection_page (:class:`PermissionsCollectionPage<onedrivesdk.model.permissions_collection_page.PermissionsCollectionPage>`):
            :class:`PermissionsCollectionRequest<onedrivesdk.request.permissions_collection.PermissionsCollectionRequest>`:
                The PermissionsCollectionRequest
            return PermissionsCollectionRequest(collection_page._next_page_link, client, options)
class PermissionsCollectionRequestBuilder(RequestBuilderBase):
        """Get the PermissionRequestBuilder with the specified key
            key (str): The key to get a PermissionRequestBuilder for
            :class:`PermissionRequestBuilder<onedrivesdk.request.permission_request_builder.PermissionRequestBuilder>`:
                A PermissionRequestBuilder for that key
        return PermissionRequestBuilder(self.append_to_request_url(str(key)), self._client)
        """Builds the PermissionsCollectionRequest
        req = PermissionsCollectionRequest(self._request_url, self._client, options)
class PermissionsCollectionResponse(CollectionResponseBase):
            self._collection_page = PermissionsCollectionPage(self._prop_dict["value"])
from ..request.permission_request_builder import PermissionRequestBuilder
        """Gets the PermissionsCollectionPage in async
