from ..model.drive import Drive
class DrivesCollectionPage(CollectionPageBase):
        """Get the Drive at the index specified
            index (int): The index of the item to get from the DrivesCollectionPage
            :class:`Drive<onedrivesdk.model.drive.Drive>`:
                The Drive at the index
        return Drive(self._prop_list[index])
    def drives(self):
        """Get a generator of Drive within the DrivesCollectionPage
                The next Drive in the collection
            yield Drive(item)
