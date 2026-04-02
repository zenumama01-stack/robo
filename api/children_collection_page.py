from ..collection_base import CollectionPageBase
from ..model.item import Item
class ChildrenCollectionPage(CollectionPageBase):
    def __getitem__(self, index):
        """Get the Item at the index specified
            index (int): The index of the item to get from the ChildrenCollectionPage
            :class:`Item<onedrivesdk.model.item.Item>`:
                The Item at the index
        return Item(self._prop_list[index])
    def children(self):
        """Get a generator of Item within the ChildrenCollectionPage
        Yields:
                The next Item in the collection
        for item in self._prop_list:
            yield Item(item)
