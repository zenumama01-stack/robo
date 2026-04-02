class ItemCreateLinkRequest(RequestBase):
    def __init__(self, request_url, client, options, type):
        super(ItemCreateLinkRequest, self).__init__(request_url, client, options)
        if type:
            self.body_options["type"] = type
        entity = Permission(json.loads(self.send(self.body_options).content))
class ItemCreateLinkRequestBuilder(RequestBuilderBase):
    def __init__(self, request_url, client, type):
        super(ItemCreateLinkRequestBuilder, self).__init__(request_url, client)
        self._method_options["type"] = type
        """Builds the request for the ItemCreateLink
            :class:`ItemCreateLinkRequest<onedrivesdk.request.item_create_link.ItemCreateLinkRequest>`:
        req = ItemCreateLinkRequest(self._request_url, self._client, options, self._method_options["type"])
            The resulting Permission from the operation
