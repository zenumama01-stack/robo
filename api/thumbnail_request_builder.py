from .thumbnail_request import ThumbnailRequest
from ..request.thumbnail_content_request import ThumbnailContentRequestBuilder
class ThumbnailRequestBuilder(RequestBuilderBase):
        """Initialize the ThumbnailRequestBuilder
        super(ThumbnailRequestBuilder, self).__init__(request_url, client)
        """Builds the ThumbnailRequest
            :class:`ThumbnailRequest<onedrivesdk.request.thumbnail_request.ThumbnailRequest>`:
                The ThumbnailRequest
        req = ThumbnailRequest(self._request_url, self._client, options)
                The updated Thumbnail
        return self.request().update(thumbnail)
        """The content for the ThumbnailRequestBuilder
            :class:`ThumbnailContentRequestBuilder<onedrivesdk.request.thumbnail_content.ThumbnailContentRequestBuilder>`:
                A request builder created from the ThumbnailRequestBuilder
        return ThumbnailContentRequestBuilder(self.append_to_request_url("content"), self._client)
        entity = yield from self.request().update_async(thumbnail)
