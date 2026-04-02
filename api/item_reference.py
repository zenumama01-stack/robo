class ItemReference(OneDriveObjectBase):
    def drive_id(self):
        """Gets and sets the driveId
                The driveId
        if "driveId" in self._prop_dict:
            return self._prop_dict["driveId"]
    @drive_id.setter
    def drive_id(self, val):
        self._prop_dict["driveId"] = val
    def path(self):
        """Gets and sets the path
                The path
        if "path" in self._prop_dict:
            return self._prop_dict["path"]
    @path.setter
    def path(self, val):
        self._prop_dict["path"] = val
