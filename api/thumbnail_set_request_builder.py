from .thumbnail_set_request import ThumbnailSetRequest
from ..request.thumbnail_request_builder import ThumbnailRequestBuilder
class ThumbnailSetRequestBuilder(RequestBuilderBase):
        """Initialize the ThumbnailSetRequestBuilder
        super(ThumbnailSetRequestBuilder, self).__init__(request_url, client)
        """Builds the ThumbnailSetRequest
            :class:`ThumbnailSetRequest<onedrivesdk.request.thumbnail_set_request.ThumbnailSetRequest>`:
                The ThumbnailSetRequest
        req = ThumbnailSetRequest(self._request_url, self._client, options)
                The updated ThumbnailSet
        return self.request().update(thumbnail_set)
        """The large for the ThumbnailSetRequestBuilder
            :class:`ThumbnailRequestBuilder<onedrivesdk.request.thumbnail_request.ThumbnailRequestBuilder>`:
                A request builder created from the ThumbnailSetRequestBuilder
        return ThumbnailRequestBuilder(self.append_to_request_url("large"), self._client)
        """The medium for the ThumbnailSetRequestBuilder
        return ThumbnailRequestBuilder(self.append_to_request_url("medium"), self._client)
        """The small for the ThumbnailSetRequestBuilder
        return ThumbnailRequestBuilder(self.append_to_request_url("small"), self._client)
        """The source for the ThumbnailSetRequestBuilder
        return ThumbnailRequestBuilder(self.append_to_request_url("source"), self._client)
        entity = yield from self.request().update_async(thumbnail_set)
