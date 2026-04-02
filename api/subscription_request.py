class SubscriptionRequest(RequestBase):
    """The type SubscriptionRequest."""
        """Constructs a new SubscriptionRequest.
            request_url (str): The url to perform the SubscriptionRequest
        super(SubscriptionRequest, self).__init__(request_url, client, options)
        """Deletes the specified Subscription."""
        """Gets the specified Subscription.
                The Subscription.
        entity = Subscription(json.loads(self.send().content))
    def update(self, subscription):
        """Updates the specified Subscription.
            subscription (:class:`Subscription<onedrivesdk.model.subscription.Subscription>`):
                The Subscription to update.
                The updated Subscription.
        entity = Subscription(json.loads(self.send(subscription).content))
        """Gets the specified Subscription in async.
    def update_async(self, subscription):
        """Updates the specified Subscription in async
                                                    subscription)
