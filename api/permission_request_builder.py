from .permission_request import PermissionRequest
class PermissionRequestBuilder(RequestBuilderBase):
        """Initialize the PermissionRequestBuilder
        super(PermissionRequestBuilder, self).__init__(request_url, client)
        """Builds the PermissionRequest
            :class:`PermissionRequest<onedrivesdk.request.permission_request.PermissionRequest>`:
                The PermissionRequest
        req = PermissionRequest(self._request_url, self._client, options)
                The updated Permission
        return self.request().update(permission)
        entity = yield from self.request().update_async(permission)
