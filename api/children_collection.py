from ..collection_base import CollectionRequestBase, CollectionResponseBase
class ChildrenCollectionRequest(CollectionRequestBase):
        """Initialize the ChildrenCollectionRequest
            request_url (str): The url to perform the ChildrenCollectionRequest
                on
            client (:class:`OneDriveClient<onedrivesdk.request.one_drive_client.OneDriveClient>`):
                The client which will be used for the request
                A list of options to pass into the request
        super(ChildrenCollectionRequest, self).__init__(request_url, client, options)
    def add(self, entity):
        """Add a Item to the collection
            entity (:class:`Item<onedrivesdk.model.item.Item>`):
                The Item that you would like to add to the collection
                The Item that you added, with additional data from OneDrive
        self.content_type = "application/json"
        entity = Item(json.loads(self.send(entity).content))
        """Gets the ChildrenCollectionPage
            :class:`ChildrenCollectionPage<onedrivesdk.model.children_collection_page.ChildrenCollectionPage>`:
                The ChildrenCollectionPage
        collection_response = ChildrenCollectionResponse(json.loads(self.send().content))
        return self._page_from_response(collection_response)
    def get_next_page_request(collection_page, client, options=None):
        """Gets the ChildrenCollectionRequest for the next page. Returns None if there is no next page
            collection_page (:class:`ChildrenCollectionPage<onedrivesdk.model.children_collection_page.ChildrenCollectionPage>`):
                The collection to get the next page for
                A list of options to pass into the request. Defaults to None.
            :class:`ChildrenCollectionRequest<onedrivesdk.request.children_collection.ChildrenCollectionRequest>`:
                The ChildrenCollectionRequest
        if collection_page._next_page_link:
            return ChildrenCollectionRequest(collection_page._next_page_link, client, options)
class ChildrenCollectionRequestBuilder(RequestBuilderBase):
        """Get the ItemRequestBuilder with the specified key
            key (str): The key to get a ItemRequestBuilder for
            :class:`ItemRequestBuilder<onedrivesdk.request.item_request_builder.ItemRequestBuilder>`:
                A ItemRequestBuilder for that key
        return ItemRequestBuilder(self.append_to_request_url(str(key)), self._client)
    def request(self, expand=None, select=None, top=None, order_by=None, options=None):
        """Builds the ChildrenCollectionRequest
        req = ChildrenCollectionRequest(self._request_url, self._client, options)
        req._set_query_options(expand=expand, select=select, top=top, order_by=order_by)
        return self.request().add(entity)
        return self.request().get()
class ChildrenCollectionResponse(CollectionResponseBase):
        """The collection page stored in the response JSON
                The collection page
        if self._collection_page:
            self._collection_page._prop_list = self._prop_dict["value"]
            self._collection_page = ChildrenCollectionPage(self._prop_dict["value"])
        return self._collection_page
    def add_async(self, entity):
        """Add a Item to the collection in async
                                                    self.add,
                                                    entity)
    def get_async(self):
        """Gets the ChildrenCollectionPage in async
                                                    self.get)
        collection_page = yield from future
        return collection_page
        entity = yield from self.request().add_async(entity)
        collection_page = yield from self.request().get_async()
