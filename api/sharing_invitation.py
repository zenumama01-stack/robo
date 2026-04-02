class SharingInvitation(OneDriveObjectBase):
    def invited_by(self):
        Gets and sets the invitedBy
                The invitedBy
        if "invitedBy" in self._prop_dict:
            if isinstance(self._prop_dict["invitedBy"], OneDriveObjectBase):
                return self._prop_dict["invitedBy"]
                self._prop_dict["invitedBy"] = IdentitySet(self._prop_dict["invitedBy"])
    @invited_by.setter
    def invited_by(self, val):
        self._prop_dict["invitedBy"] = val
    def sign_in_required(self):
        """Gets and sets the signInRequired
                The signInRequired
        if "signInRequired" in self._prop_dict:
            return self._prop_dict["signInRequired"]
    @sign_in_required.setter
    def sign_in_required(self, val):
        self._prop_dict["signInRequired"] = val
    def send_invitation_status(self):
        """Gets and sets the sendInvitationStatus
                The sendInvitationStatus
        if "sendInvitationStatus" in self._prop_dict:
            return self._prop_dict["sendInvitationStatus"]
    @send_invitation_status.setter
    def send_invitation_status(self, val):
        self._prop_dict["sendInvitationStatus"] = val
    def invite_error_resolve_url(self):
        """Gets and sets the inviteErrorResolveUrl
                The inviteErrorResolveUrl
        if "inviteErrorResolveUrl" in self._prop_dict:
            return self._prop_dict["inviteErrorResolveUrl"]
    @invite_error_resolve_url.setter
    def invite_error_resolve_url(self, val):
        self._prop_dict["inviteErrorResolveUrl"] = val
