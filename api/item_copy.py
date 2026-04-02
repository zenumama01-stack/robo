class ItemCopyRequest(RequestBase):
    def __init__(self, request_url, client, options, name=None, parent_reference=None):
        super(ItemCopyRequest, self).__init__(request_url, client, options)
        self.body_options={}
            self.body_options["name"] = name
        if parent_reference:
            self.body_options["parentReference"] = parent_reference
    def body_options(self):
        return self._body_options
    @body_options.setter
    def body_options(self, value):
        self._body_options=value
            :class:`AsyncOperationMonitor<onedrivesdk.async_operation_monitor.AsyncOperationMonitor>`:
        self.append_option(HeaderOption("Prefer", "respond-async"))
        response = self.send(self.body_options)
        entity = AsyncOperationMonitor(response.headers["Location"], self._client, None)
class ItemCopyRequestBuilder(RequestBuilderBase):
    def __init__(self, request_url, client, name=None, parent_reference=None):
        super(ItemCopyRequestBuilder, self).__init__(request_url, client)
        self._method_options["name"] = name
        self._method_options["parentReference"] = parent_reference._prop_dict
    def request(self, options=None):
        """Builds the request for the ItemCopy
            :class:`ItemCopyRequest<onedrivesdk.request.item_copy.ItemCopyRequest>`:
        req = ItemCopyRequest(self._request_url, self._client, options, name=self._method_options["name"], parent_reference=self._method_options["parentReference"])
            The resulting Item from the operation
        return self.request().post()
        entity = yield from self.request().post_async()
