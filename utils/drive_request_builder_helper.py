from ..request.drive_request_builder import DriveRequestBuilder
from ..request.item_request_builder import ItemRequestBuilder
def item_by_path(self, path):
    """Get an item by path in OneDrive
    Args
        path (str): The path to the requested item
        :class:`ItemRequestBuilder<onedrivesdk.requests.item_request_builder.ItemRequestBuilder>`:
            A request builder for an item given a path
    #strip any leading '/'
    path = str(path)[1:] if str(path)[0] == "/" else str(path)
    return ItemRequestBuilder(self.append_to_request_url("root:/"+str(path)+":"), self._client)
DriveRequestBuilder.item_by_path = item_by_path
