class Thumbnail(OneDriveObjectBase):
    def url(self):
        """Gets and sets the url
                The url
        if "url" in self._prop_dict:
            return self._prop_dict["url"]
    @url.setter
    def url(self, val):
        self._prop_dict["url"] = val
