class SubscriptionsCollectionRequest(CollectionRequestBase):
        """Initialize the SubscriptionsCollectionRequest
            request_url (str): The url to perform the SubscriptionsCollectionRequest
        super(SubscriptionsCollectionRequest, self).__init__(request_url, client, options)
        """Gets the SubscriptionsCollectionPage
            :class:`SubscriptionsCollectionPage<onedrivesdk.model.subscriptions_collection_page.SubscriptionsCollectionPage>`:
                The SubscriptionsCollectionPage
        collection_response = SubscriptionsCollectionResponse(json.loads(self.send().content))
        """Gets the SubscriptionsCollectionRequest for the next page. Returns None if there is no next page
            collection_page (:class:`SubscriptionsCollectionPage<onedrivesdk.model.subscriptions_collection_page.SubscriptionsCollectionPage>`):
            :class:`SubscriptionsCollectionRequest<onedrivesdk.request.subscriptions_collection.SubscriptionsCollectionRequest>`:
                The SubscriptionsCollectionRequest
            return SubscriptionsCollectionRequest(collection_page._next_page_link, client, options)
class SubscriptionsCollectionRequestBuilder(RequestBuilderBase):
        """Get the SubscriptionRequestBuilder with the specified key
            key (str): The key to get a SubscriptionRequestBuilder for
            :class:`SubscriptionRequestBuilder<onedrivesdk.request.subscription_request_builder.SubscriptionRequestBuilder>`:
                A SubscriptionRequestBuilder for that key
        return SubscriptionRequestBuilder(self.append_to_request_url(str(key)), self._client)
        """Builds the SubscriptionsCollectionRequest
        req = SubscriptionsCollectionRequest(self._request_url, self._client, options)
class SubscriptionsCollectionResponse(CollectionResponseBase):
            self._collection_page = SubscriptionsCollectionPage(self._prop_dict["value"])
from ..request.subscription_request_builder import SubscriptionRequestBuilder
        """Gets the SubscriptionsCollectionPage in async
