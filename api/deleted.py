class Deleted(OneDriveObjectBase):
    def state(self):
        """Gets and sets the state
                The state
        if "state" in self._prop_dict:
            return self._prop_dict["state"]
    @state.setter
    def state(self, val):
        self._prop_dict["state"] = val
