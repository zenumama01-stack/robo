class ThumbnailContentRequest(RequestBase):
        """Initialize the ThumbnailContentRequest
            request_url (str): The url to perform the ThumbnailContentRequest
        super(ThumbnailContentRequest, self).__init__(request_url, client, options)
        """Downloads the specified Thumbnail.
                The path where the Thumbnail should be downloaded to
class ThumbnailContentRequestBuilder(RequestBuilderBase):
        """Initialize the ThumbnailContentRequestBuilder
                the ThumbnailContentRequestBuilder at
                ThumbnailContentRequestBuilder
        super(ThumbnailContentRequestBuilder, self).__init__(request_url, client)
        """Builds the ThumbnailContentRequest
            :class:`ThumbnailContentRequest<onedrivesdk.request.thumbnail_content.ThumbnailContentRequest>`:
                The ThumbnailContentRequest
        return ThumbnailContentRequest(self._request_url, self._client, None)        Downloads the specified Thumbnail in async.
        return ThumbnailContentRequest(self._request_url, self._client, None)        Downloads the specified Thumbnail in async.
        return ThumbnailContentRequest(self._request_url, self._client, None)        Downloads the specified Thumbnail in async.
