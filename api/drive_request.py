class DriveRequest(RequestBase):
    """The type DriveRequest."""
        """Constructs a new DriveRequest.
            request_url (str): The url to perform the DriveRequest
        super(DriveRequest, self).__init__(request_url, client, options)
        """Deletes the specified Drive."""
        self.method = "DELETE"
        self.send()
        """Gets the specified Drive.
                The Drive.
        entity = Drive(json.loads(self.send().content))
        self._initialize_collection_properties(entity)
    def update(self, drive):
        """Updates the specified Drive.
            drive (:class:`Drive<onedrivesdk.model.drive.Drive>`):
                The Drive to update.
                The updated Drive.
        self.method = "PATCH"
        entity = Drive(json.loads(self.send(drive).content))
    def _initialize_collection_properties(self, value):
        if value and value._prop_dict:
            if value.items:
                if "items@odata.nextLink" in value._prop_dict:
                    next_page_link = value._prop_dict["items@odata.nextLink"]
                    value.items._next_page_link = next_page_link
            if value.shared:
                if "shared@odata.nextLink" in value._prop_dict:
                    next_page_link = value._prop_dict["shared@odata.nextLink"]
                    value.shared._next_page_link = next_page_link
            if value.special:
                if "special@odata.nextLink" in value._prop_dict:
                    next_page_link = value._prop_dict["special@odata.nextLink"]
                    value.special._next_page_link = next_page_link
    def delete_async(self):
                                                    self.delete)
        """Gets the specified Drive in async.
    def update_async(self, drive):
        """Updates the specified Drive in async
                                                    self.update,
                                                    drive)
