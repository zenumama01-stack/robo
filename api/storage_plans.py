class StoragePlans(OneDriveObjectBase):
    def upgrade_available(self):
        """Gets and sets the upgradeAvailable
                The upgradeAvailable
        if "upgradeAvailable" in self._prop_dict:
            return self._prop_dict["upgradeAvailable"]
    @upgrade_available.setter
    def upgrade_available(self, val):
        self._prop_dict["upgradeAvailable"] = val
