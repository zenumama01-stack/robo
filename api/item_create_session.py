class ItemCreateSessionRequest(RequestBase):
    def __init__(self, request_url, client, options, item=None):
        super(ItemCreateSessionRequest, self).__init__(request_url, client, options)
        if item:
            self.body_options["item"] = item
        entity = UploadSession(json.loads(self.send(self.body_options).content))
class ItemCreateSessionRequestBuilder(RequestBuilderBase):
    def __init__(self, request_url, client, item=None):
        super(ItemCreateSessionRequestBuilder, self).__init__(request_url, client)
        self._method_options["item"] = item._prop_dict
        """Builds the request for the ItemCreateSession
            :class:`ItemCreateSessionRequest<onedrivesdk.request.item_create_session.ItemCreateSessionRequest>`:
        req = ItemCreateSessionRequest(self._request_url, self._client, options, item=self._method_options["item"])
