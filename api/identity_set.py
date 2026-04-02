from ..model.identity import Identity
class IdentitySet(OneDriveObjectBase):
    def application(self):
        Gets and sets the application
            :class:`Identity<onedrivesdk.model.identity.Identity>`:
                The application
        if "application" in self._prop_dict:
            if isinstance(self._prop_dict["application"], OneDriveObjectBase):
                return self._prop_dict["application"]
                self._prop_dict["application"] = Identity(self._prop_dict["application"])
    @application.setter
    def application(self, val):
        self._prop_dict["application"] = val
    def device(self):
        Gets and sets the device
                The device
        if "device" in self._prop_dict:
            if isinstance(self._prop_dict["device"], OneDriveObjectBase):
                return self._prop_dict["device"]
                self._prop_dict["device"] = Identity(self._prop_dict["device"])
    @device.setter
    def device(self, val):
        self._prop_dict["device"] = val
    def user(self):
        Gets and sets the user
                The user
        if "user" in self._prop_dict:
            if isinstance(self._prop_dict["user"], OneDriveObjectBase):
                return self._prop_dict["user"]
                self._prop_dict["user"] = Identity(self._prop_dict["user"])
    @user.setter
    def user(self, val):
        self._prop_dict["user"] = val
