class Folder(OneDriveObjectBase):
    def child_count(self):
        """Gets and sets the childCount
                The childCount
        if "childCount" in self._prop_dict:
            return self._prop_dict["childCount"]
    @child_count.setter
    def child_count(self, val):
        self._prop_dict["childCount"] = val
