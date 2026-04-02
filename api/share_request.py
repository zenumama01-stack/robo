class ShareRequest(RequestBase):
    """The type ShareRequest."""
        """Constructs a new ShareRequest.
            request_url (str): The url to perform the ShareRequest
        super(ShareRequest, self).__init__(request_url, client, options)
        """Deletes the specified Share."""
        """Gets the specified Share.
                The Share.
        entity = Share(json.loads(self.send().content))
    def update(self, share):
        """Updates the specified Share.
            share (:class:`Share<onedrivesdk.model.share.Share>`):
                The Share to update.
                The updated Share.
        entity = Share(json.loads(self.send(share).content))
        """Gets the specified Share in async.
    def update_async(self, share):
        """Updates the specified Share in async
                                                    share)
