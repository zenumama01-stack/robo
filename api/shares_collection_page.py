from ..model.share import Share
class SharesCollectionPage(CollectionPageBase):
        """Get the Share at the index specified
            index (int): The index of the item to get from the SharesCollectionPage
            :class:`Share<onedrivesdk.model.share.Share>`:
                The Share at the index
        return Share(self._prop_list[index])
    def shares(self):
        """Get a generator of Share within the SharesCollectionPage
                The next Share in the collection
            yield Share(item)
