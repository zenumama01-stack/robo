from .share_request import ShareRequest
class ShareRequestBuilder(RequestBuilderBase):
        """Initialize the ShareRequestBuilder
        super(ShareRequestBuilder, self).__init__(request_url, client)
        """Builds the ShareRequest
            :class:`ShareRequest<onedrivesdk.request.share_request.ShareRequest>`:
                The ShareRequest
        req = ShareRequest(self._request_url, self._client, options)
                The updated Share
        return self.request().update(share)
        """The items for the ShareRequestBuilder
                A request builder created from the ShareRequestBuilder
        entity = yield from self.request().update_async(share)
