class ThumbnailSetRequest(RequestBase):
    """The type ThumbnailSetRequest."""
        """Constructs a new ThumbnailSetRequest.
            request_url (str): The url to perform the ThumbnailSetRequest
        super(ThumbnailSetRequest, self).__init__(request_url, client, options)
        """Deletes the specified ThumbnailSet."""
        """Gets the specified ThumbnailSet.
                The ThumbnailSet.
        entity = ThumbnailSet(json.loads(self.send().content))
    def update(self, thumbnail_set):
        """Updates the specified ThumbnailSet.
            thumbnail_set (:class:`ThumbnailSet<onedrivesdk.model.thumbnail_set.ThumbnailSet>`):
                The ThumbnailSet to update.
                The updated ThumbnailSet.
        entity = ThumbnailSet(json.loads(self.send(thumbnail_set).content))
        """Gets the specified ThumbnailSet in async.
    def update_async(self, thumbnail_set):
        """Updates the specified ThumbnailSet in async
                                                    thumbnail_set)
