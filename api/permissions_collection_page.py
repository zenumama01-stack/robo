class PermissionsCollectionPage(CollectionPageBase):
        """Get the Permission at the index specified
            index (int): The index of the item to get from the PermissionsCollectionPage
            :class:`Permission<onedrivesdk.model.permission.Permission>`:
                The Permission at the index
        return Permission(self._prop_list[index])
        """Get a generator of Permission within the PermissionsCollectionPage
                The next Permission in the collection
            yield Permission(item)
