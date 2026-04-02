from ..request.one_drive_client import OneDriveClient
def item(self, drive=None, id=None, path=None):
    """Gets an item given the specified (optional) drive,
    (optional) id, and (optional) path
        drive (str): The drive that you want to use
        id (str): The id of the item to request
        path (str): The path of the item to request
            A request builder for an item given a path or id
    if id is None and path is None:
        raise ValueError("Either 'path' or 'id' must be specified")
    elif id is not None and path is not None:
        raise ValueError("Only one of either 'path' or 'id' can be specified")
    drive_builder = self.drives[drive] if drive else self.drive
        return drive_builder.item_by_path(path)
    elif id:
        return drive_builder.items[id]
def drive(self):
    """Gets the user's default drive
        :class:`DriveRequestBuilder<onedrivesdk.requests.drive_request_builder.DriveRequestBuilder>`:
            User's default drive
    return DriveRequestBuilder(self.base_url + "drive", self)
OneDriveClient.item = item
OneDriveClient.drive = drive
