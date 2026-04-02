from .drive_request import DriveRequest
from ..request.drive_recent import DriveRecentRequestBuilder
class DriveRequestBuilder(RequestBuilderBase):
        """Initialize the DriveRequestBuilder
        super(DriveRequestBuilder, self).__init__(request_url, client)
    def request(self, expand=None, select=None, options=None):
        """Builds the DriveRequest
            :class:`DriveRequest<onedrivesdk.request.drive_request.DriveRequest>`:
                The DriveRequest
        req = DriveRequest(self._request_url, self._client, options)
        req._set_query_options(expand=expand, select=select)
        self.request().delete()
                The updated Drive
        return self.request().update(drive)
        """The items for the DriveRequestBuilder
            :class:`ItemsCollectionRequestBuilder<onedrivesdk.request.items_collection.ItemsCollectionRequestBuilder>`:
                A request builder created from the DriveRequestBuilder
        return ItemsCollectionRequestBuilder(self.append_to_request_url("items"), self._client)
        """The shared for the DriveRequestBuilder
            :class:`SharedCollectionRequestBuilder<onedrivesdk.request.shared_collection.SharedCollectionRequestBuilder>`:
        return SharedCollectionRequestBuilder(self.append_to_request_url("shared"), self._client)
        """The special for the DriveRequestBuilder
            :class:`SpecialCollectionRequestBuilder<onedrivesdk.request.special_collection.SpecialCollectionRequestBuilder>`:
        return SpecialCollectionRequestBuilder(self.append_to_request_url("special"), self._client)
    def recent(self):
        """Executes the recent method
            :class:`DriveRecentRequestBuilder<onedrivesdk.request.drive_recent.DriveRecentRequestBuilder>`:
                A DriveRecentRequestBuilder for the method
        return DriveRecentRequestBuilder(self.append_to_request_url("recent"), self._client)
from ..request.items_collection import ItemsCollectionRequestBuilder
from ..request.shared_collection import SharedCollectionRequestBuilder
from ..request.special_collection import SpecialCollectionRequestBuilder
        yield from self.request().delete_async()
        entity = yield from self.request().get_async()
        entity = yield from self.request().update_async(drive)
