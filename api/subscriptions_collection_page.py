class SubscriptionsCollectionPage(CollectionPageBase):
        """Get the Subscription at the index specified
            index (int): The index of the item to get from the SubscriptionsCollectionPage
            :class:`Subscription<onedrivesdk.model.subscription.Subscription>`:
                The Subscription at the index
        return Subscription(self._prop_list[index])
        """Get a generator of Subscription within the SubscriptionsCollectionPage
                The next Subscription in the collection
            yield Subscription(item)
