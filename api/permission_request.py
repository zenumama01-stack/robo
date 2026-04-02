class PermissionRequest(RequestBase):
    """The type PermissionRequest."""
        """Constructs a new PermissionRequest.
            request_url (str): The url to perform the PermissionRequest
        super(PermissionRequest, self).__init__(request_url, client, options)
        """Deletes the specified Permission."""
        """Gets the specified Permission.
                The Permission.
        entity = Permission(json.loads(self.send().content))
    def update(self, permission):
        """Updates the specified Permission.
            permission (:class:`Permission<onedrivesdk.model.permission.Permission>`):
                The Permission to update.
                The updated Permission.
        entity = Permission(json.loads(self.send(permission).content))
        """Gets the specified Permission in async.
    def update_async(self, permission):
        """Updates the specified Permission in async
                                                    permission)
