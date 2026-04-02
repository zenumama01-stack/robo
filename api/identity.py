from ..model.thumbnail_set import ThumbnailSet
class Identity(OneDriveObjectBase):
    def display_name(self):
        """Gets and sets the displayName
                The displayName
        if "displayName" in self._prop_dict:
            return self._prop_dict["displayName"]
    @display_name.setter
    def display_name(self, val):
        self._prop_dict["displayName"] = val
        """Gets and sets the id
    def thumbnails(self):
        Gets and sets the thumbnails
            :class:`ThumbnailSet<onedrivesdk.model.thumbnail_set.ThumbnailSet>`:
                The thumbnails
        if "thumbnails" in self._prop_dict:
            if isinstance(self._prop_dict["thumbnails"], OneDriveObjectBase):
                return self._prop_dict["thumbnails"]
                self._prop_dict["thumbnails"] = ThumbnailSet(self._prop_dict["thumbnails"])
    @thumbnails.setter
    def thumbnails(self, val):
        self._prop_dict["thumbnails"] = val
