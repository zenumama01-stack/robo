from ..model.sharing_invitation import SharingInvitation
from ..model.sharing_link import SharingLink
class Permission(OneDriveObjectBase):
    def granted_to(self):
        Gets and sets the grantedTo
                The grantedTo
        if "grantedTo" in self._prop_dict:
            if isinstance(self._prop_dict["grantedTo"], OneDriveObjectBase):
                return self._prop_dict["grantedTo"]
                self._prop_dict["grantedTo"] = IdentitySet(self._prop_dict["grantedTo"])
    @granted_to.setter
    def granted_to(self, val):
        self._prop_dict["grantedTo"] = val
    def invitation(self):
        Gets and sets the invitation
            :class:`SharingInvitation<onedrivesdk.model.sharing_invitation.SharingInvitation>`:
                The invitation
        if "invitation" in self._prop_dict:
            if isinstance(self._prop_dict["invitation"], OneDriveObjectBase):
                return self._prop_dict["invitation"]
                self._prop_dict["invitation"] = SharingInvitation(self._prop_dict["invitation"])
    @invitation.setter
    def invitation(self, val):
        self._prop_dict["invitation"] = val
    def inherited_from(self):
        Gets and sets the inheritedFrom
                The inheritedFrom
        if "inheritedFrom" in self._prop_dict:
            if isinstance(self._prop_dict["inheritedFrom"], OneDriveObjectBase):
                return self._prop_dict["inheritedFrom"]
                self._prop_dict["inheritedFrom"] = ItemReference(self._prop_dict["inheritedFrom"])
    @inherited_from.setter
    def inherited_from(self, val):
        self._prop_dict["inheritedFrom"] = val
    def link(self):
        Gets and sets the link
            :class:`SharingLink<onedrivesdk.model.sharing_link.SharingLink>`:
                The link
        if "link" in self._prop_dict:
            if isinstance(self._prop_dict["link"], OneDriveObjectBase):
                return self._prop_dict["link"]
                self._prop_dict["link"] = SharingLink(self._prop_dict["link"])
    @link.setter
    def link(self, val):
        self._prop_dict["link"] = val
    def roles(self):
        Gets and sets the roles
                The roles
        if "roles" in self._prop_dict:
            return self._prop_dict["roles"]
    @roles.setter
    def roles(self, val):
        self._prop_dict["roles"] = val
    def share_id(self):
        Gets and sets the shareId
                The shareId
        if "shareId" in self._prop_dict:
            return self._prop_dict["shareId"]
    @share_id.setter
    def share_id(self, val):
        self._prop_dict["shareId"] = val
