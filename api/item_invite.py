class ItemInviteRequest(CollectionRequestBase):
    def __init__(self, request_url, client, options, recipients, require_sign_in=None, roles=None, send_invitation=None, message=None):
        super(ItemInviteRequest, self).__init__(request_url, client, options)
        if recipients:
            self.body_options["recipients"] = recipients
        if require_sign_in:
            self.body_options["requireSignIn"] = require_sign_in
        if roles:
            self.body_options["roles"] = roles
        if send_invitation:
            self.body_options["sendInvitation"] = send_invitation
        if message:
            self.body_options["message"] = message
        collection_response = ItemsCollectionResponse(json.loads(self.send(self.body_options).content))
class ItemInviteRequestBuilder(RequestBuilderBase):
    def __init__(self, request_url, client, recipients, require_sign_in=None, roles=None, send_invitation=None, message=None):
        super(ItemInviteRequestBuilder, self).__init__(request_url, client)
        self._method_options["recipients"] = recipients._prop_dict
        self._method_options["requireSignIn"] = require_sign_in
        self._method_options["roles"] = roles
        self._method_options["sendInvitation"] = send_invitation
        self._method_options["message"] = message
        """Builds the request for the ItemInvite
            :class:`ItemInviteRequest<onedrivesdk.request.item_invite.ItemInviteRequest>`:
        req = ItemInviteRequest(self._request_url, self._client, options, self._method_options["recipients"], require_sign_in=self._method_options["requireSignIn"], roles=self._method_options["roles"], send_invitation=self._method_options["sendInvitation"], message=self._method_options["message"])
        collection_page = yield from self.request().post_async()
