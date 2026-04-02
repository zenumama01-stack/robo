from ..collection_base import CollectionRequestBase, CollectionResponseBase, CollectionPageBase
class EffectiveRolesCollectionRequest(CollectionRequestBase):
        """Initialize the EffectiveRolesCollectionRequest
            request_url (str): The url to perform the EffectiveRolesCollectionRequest
        super(EffectiveRolesCollectionRequest, self).__init__(request_url, client, options)
        """Gets the EffectiveRolesCollectionPage
            :class:`EffectiveRolesCollectionPage<onedrivesdk.request.effective_roles_collection.EffectiveRolesCollectionPage>`:
                The EffectiveRolesCollectionPage
        collection_response = EffectiveRolesCollectionResponse(json.loads(self.send().content))
class EffectiveRolesCollectionRequestBuilder(RequestBuilderBase):
        """Get the strRequestBuilder with the specified key
            key (str): The key to get a strRequestBuilder for
            :class:`strRequestBuilder<onedrivesdk.request.str_request_builder.strRequestBuilder>`:
                A strRequestBuilder for that key
        return strRequestBuilder(self.append_to_request_url(str(key)), self._client)
        """Builds the EffectiveRolesCollectionRequest
            :class:`EffectiveRolesCollectionRequest<onedrivesdk.request.effective_roles_collection.EffectiveRolesCollectionRequest>`:
                The EffectiveRolesCollectionRequest
        req = EffectiveRolesCollectionRequest(self._request_url, self._client, options)
class EffectiveRolesCollectionResponse(CollectionResponseBase):
            self._collection_page = EffectiveRolesCollectionPage(self._prop_dict["value"])
class EffectiveRolesCollectionPage(CollectionPageBase):
        """Get the str at the index specified
            index (int): The index of the item to get from the EffectiveRolesCollectionPage
            :class:`str<onedrivesdk.model.str.str>`:
                The str at the index
        return str(self._prop_list[index])
        """Get a generator of str within the EffectiveRolesCollectionPage
                The next str in the collection
            yield str(item)
    def _init_next_page_request(self, next_page_link, client, options):
        """Initialize the next page request for the EffectiveRolesCollectionPage
            next_page_link (str): The URL for the next page request
                to be sent to
            client (:class:`OneDriveClient<onedrivesdk.model.one_drive_client.OneDriveClient>`:
                The client to be used for the request
            options (list of :class:`Option<onedrivesdk.options.Option>`:
                A list of options
        self._next_page_request = EffectiveRolesCollectionRequest(next_page_link, client, options)
from ..request.str_request_builder import strRequestBuilder
        """Gets the EffectiveRolesCollectionPage in async
