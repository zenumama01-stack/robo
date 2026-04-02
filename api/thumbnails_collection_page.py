class ThumbnailsCollectionPage(CollectionPageBase):
        """Get the ThumbnailSet at the index specified
            index (int): The index of the item to get from the ThumbnailsCollectionPage
                The ThumbnailSet at the index
        return ThumbnailSet(self._prop_list[index])
        """Get a generator of ThumbnailSet within the ThumbnailsCollectionPage
                The next ThumbnailSet in the collection
            yield ThumbnailSet(item)
