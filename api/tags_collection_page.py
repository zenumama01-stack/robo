class TagsCollectionPage(CollectionPageBase):
        """Get the Tag at the index specified
            index (int): The index of the item to get from the TagsCollectionPage
            :class:`Tag<onedrivesdk.model.tag.Tag>`:
                The Tag at the index
        return Tag(self._prop_list[index])
        """Get a generator of Tag within the TagsCollectionPage
                The next Tag in the collection
            yield Tag(item)
