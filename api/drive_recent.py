from ..collection_base import CollectionRequestBase
from ..options import *
class DriveRecentRequest(CollectionRequestBase):
        super(DriveRecentRequest, self).__init__(request_url, client, options)
        """Sends the GET request
            :class:`ItemsCollectionResponse<onedrivesdk.request.items_collection.ItemsCollectionResponse>`:
                The resulting collection page from the operation
        collection_response = ItemsCollectionResponse(json.loads(self.send().content))
    def get_next_page_request(collection_page, client, options):
        """Gets the DriveRecentRequest for the next page. Returns None if there is no next page
            :class:`DriveRecentRequest<onedrivesdk.request.drive_recent.DriveRecentRequest>`:
                The DriveRecentRequest
            return DriveRecentRequest(collection_page._next_page_link, client, options, token)
class DriveRecentRequestBuilder(RequestBuilderBase):
        super(DriveRecentRequestBuilder, self).__init__(request_url, client)
        """Builds the request for the DriveRecent
        req = DriveRecentRequest(self._request_url, self._client, options)
            The resulting ItemsCollectionResponse from the operation
from ..request.items_collection import ItemsCollectionResponse
        """Sends the GET request using an asyncio coroutine
        collection_response = yield from future
        return collection_response
