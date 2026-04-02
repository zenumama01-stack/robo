from .tag_request import TagRequest
class TagRequestBuilder(RequestBuilderBase):
        """Initialize the TagRequestBuilder
        super(TagRequestBuilder, self).__init__(request_url, client)
        """Builds the TagRequest
            :class:`TagRequest<onedrivesdk.request.tag_request.TagRequest>`:
                The TagRequest
        req = TagRequest(self._request_url, self._client, options)
                The updated Tag
        return self.request().update(tag)
        entity = yield from self.request().update_async(tag)
