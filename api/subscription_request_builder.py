from .subscription_request import SubscriptionRequest
class SubscriptionRequestBuilder(RequestBuilderBase):
        """Initialize the SubscriptionRequestBuilder
        super(SubscriptionRequestBuilder, self).__init__(request_url, client)
        """Builds the SubscriptionRequest
            :class:`SubscriptionRequest<onedrivesdk.request.subscription_request.SubscriptionRequest>`:
                The SubscriptionRequest
        req = SubscriptionRequest(self._request_url, self._client, options)
                The updated Subscription
        return self.request().update(subscription)
        entity = yield from self.request().update_async(subscription)
