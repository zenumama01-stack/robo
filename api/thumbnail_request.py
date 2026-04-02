class ThumbnailRequest(RequestBase):
    """The type ThumbnailRequest."""
        """Constructs a new ThumbnailRequest.
            request_url (str): The url to perform the ThumbnailRequest
        super(ThumbnailRequest, self).__init__(request_url, client, options)
        """Deletes the specified Thumbnail."""
        """Gets the specified Thumbnail.
                The Thumbnail.
        entity = Thumbnail(json.loads(self.send().content))
    def update(self, thumbnail):
        """Updates the specified Thumbnail.
            thumbnail (:class:`Thumbnail<onedrivesdk.model.thumbnail.Thumbnail>`):
                The Thumbnail to update.
                The updated Thumbnail.
        entity = Thumbnail(json.loads(self.send(thumbnail).content))
        """Gets the specified Thumbnail in async.
    def update_async(self, thumbnail):
        """Updates the specified Thumbnail in async
                                                    thumbnail)
