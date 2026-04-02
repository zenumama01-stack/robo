from ..model.storage_plans import StoragePlans
class Quota(OneDriveObjectBase):
        """Gets and sets the deleted
    def remaining(self):
        """Gets and sets the remaining
                The remaining
        if "remaining" in self._prop_dict:
            return self._prop_dict["remaining"]
    @remaining.setter
    def remaining(self, val):
        self._prop_dict["remaining"] = val
    def total(self):
        """Gets and sets the total
                The total
        if "total" in self._prop_dict:
            return self._prop_dict["total"]
    @total.setter
    def total(self, val):
        self._prop_dict["total"] = val
    def used(self):
        """Gets and sets the used
                The used
        if "used" in self._prop_dict:
            return self._prop_dict["used"]
    @used.setter
    def used(self, val):
        self._prop_dict["used"] = val
    def storage_plans(self):
        Gets and sets the storagePlans
            :class:`StoragePlans<onedrivesdk.model.storage_plans.StoragePlans>`:
                The storagePlans
        if "storagePlans" in self._prop_dict:
            if isinstance(self._prop_dict["storagePlans"], OneDriveObjectBase):
                return self._prop_dict["storagePlans"]
                self._prop_dict["storagePlans"] = StoragePlans(self._prop_dict["storagePlans"])
    @storage_plans.setter
    def storage_plans(self, val):
        self._prop_dict["storagePlans"] = val
