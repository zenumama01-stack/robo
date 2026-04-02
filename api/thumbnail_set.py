from ..model.thumbnail import Thumbnail
class ThumbnailSet(OneDriveObjectBase):
    def large(self):
        Gets and sets the large
            :class:`Thumbnail<onedrivesdk.model.thumbnail.Thumbnail>`:
                The large
        if "large" in self._prop_dict:
            if isinstance(self._prop_dict["large"], OneDriveObjectBase):
                return self._prop_dict["large"]
                self._prop_dict["large"] = Thumbnail(self._prop_dict["large"])
    @large.setter
    def large(self, val):
        self._prop_dict["large"] = val
    def medium(self):
        Gets and sets the medium
                The medium
        if "medium" in self._prop_dict:
            if isinstance(self._prop_dict["medium"], OneDriveObjectBase):
                return self._prop_dict["medium"]
                self._prop_dict["medium"] = Thumbnail(self._prop_dict["medium"])
    @medium.setter
    def medium(self, val):
        self._prop_dict["medium"] = val
    def small(self):
        Gets and sets the small
                The small
        if "small" in self._prop_dict:
            if isinstance(self._prop_dict["small"], OneDriveObjectBase):
                return self._prop_dict["small"]
                self._prop_dict["small"] = Thumbnail(self._prop_dict["small"])
    @small.setter
    def small(self, val):
        self._prop_dict["small"] = val
    def source(self):
        Gets and sets the source
                The source
        if "source" in self._prop_dict:
            if isinstance(self._prop_dict["source"], OneDriveObjectBase):
                return self._prop_dict["source"]
                self._prop_dict["source"] = Thumbnail(self._prop_dict["source"])
    @source.setter
    def source(self, val):
        self._prop_dict["source"] = val
