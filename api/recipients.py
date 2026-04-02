class Recipients(OneDriveObjectBase):
    def email(self):
        """Gets and sets the email
                The email
        if "email" in self._prop_dict:
            return self._prop_dict["email"]
    @email.setter
    def email(self, val):
        self._prop_dict["email"] = val
    def alias(self):
        """Gets and sets the alias
                The alias
        if "alias" in self._prop_dict:
            return self._prop_dict["alias"]
    @alias.setter
    def alias(self, val):
        self._prop_dict["alias"] = val
    def object_id(self):
        """Gets and sets the objectId
                The objectId
        if "objectId" in self._prop_dict:
            return self._prop_dict["objectId"]
    @object_id.setter
    def object_id(self, val):
        self._prop_dict["objectId"] = val
    def permission_identity_type_input(self):
        """Gets and sets the permissionIdentityTypeInput
                The permissionIdentityTypeInput
        if "permissionIdentityTypeInput" in self._prop_dict:
            return self._prop_dict["permissionIdentityTypeInput"]
    @permission_identity_type_input.setter
    def permission_identity_type_input(self, val):
        self._prop_dict["permissionIdentityTypeInput"] = val
