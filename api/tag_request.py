class TagRequest(RequestBase):
    """The type TagRequest."""
        """Constructs a new TagRequest.
            request_url (str): The url to perform the TagRequest
        super(TagRequest, self).__init__(request_url, client, options)
        """Deletes the specified Tag."""
        """Gets the specified Tag.
                The Tag.
        entity = Tag(json.loads(self.send().content))
    def update(self, tag):
        """Updates the specified Tag.
            tag (:class:`Tag<onedrivesdk.model.tag.Tag>`):
                The Tag to update.
                The updated Tag.
        entity = Tag(json.loads(self.send(tag).content))
        """Gets the specified Tag in async.
    def update_async(self, tag):
        """Updates the specified Tag in async
                                                    tag)
