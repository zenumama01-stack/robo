from ..model.identity_set import IdentitySet
from ..model.quota import Quota
from ..model.status import Status
class Drive(OneDriveObjectBase):
    def id(self):
        Gets and sets the id
                The id
        if "id" in self._prop_dict:
            return self._prop_dict["id"]
    @id.setter
    def id(self, val):
        self._prop_dict["id"] = val
    def drive_type(self):
        Gets and sets the driveType
                The driveType
        if "driveType" in self._prop_dict:
            return self._prop_dict["driveType"]
    @drive_type.setter
    def drive_type(self, val):
        self._prop_dict["driveType"] = val
    def owner(self):
        Gets and sets the owner
            :class:`IdentitySet<onedrivesdk.model.identity_set.IdentitySet>`:
                The owner
        if "owner" in self._prop_dict:
            if isinstance(self._prop_dict["owner"], OneDriveObjectBase):
                return self._prop_dict["owner"]
            else :
                self._prop_dict["owner"] = IdentitySet(self._prop_dict["owner"])
    @owner.setter
    def owner(self, val):
        self._prop_dict["owner"] = val
    def quota(self):
        Gets and sets the quota
            :class:`Quota<onedrivesdk.model.quota.Quota>`:
                The quota
        if "quota" in self._prop_dict:
            if isinstance(self._prop_dict["quota"], OneDriveObjectBase):
                return self._prop_dict["quota"]
                self._prop_dict["quota"] = Quota(self._prop_dict["quota"])
    @quota.setter
    def quota(self, val):
        self._prop_dict["quota"] = val
        Gets and sets the status
            :class:`Status<onedrivesdk.model.status.Status>`:
            if isinstance(self._prop_dict["status"], OneDriveObjectBase):
                self._prop_dict["status"] = Status(self._prop_dict["status"])
    def items(self):
        """Gets and sets the items
            :class:`ItemsCollectionPage<onedrivesdk.request.items_collection.ItemsCollectionPage>`:
                The items
        if "items" in self._prop_dict:
            return ItemsCollectionPage(self._prop_dict["items"])
    def shared(self):
        """Gets and sets the shared
            :class:`SharedCollectionPage<onedrivesdk.request.shared_collection.SharedCollectionPage>`:
                The shared
        if "shared" in self._prop_dict:
            return SharedCollectionPage(self._prop_dict["shared"])
    def special(self):
        """Gets and sets the special
            :class:`SpecialCollectionPage<onedrivesdk.request.special_collection.SpecialCollectionPage>`:
                The special
        if "special" in self._prop_dict:
            return SpecialCollectionPage(self._prop_dict["special"])
from ..model.items_collection_page import ItemsCollectionPage
from ..model.shared_collection_page import SharedCollectionPage
from ..model.special_collection_page import SpecialCollectionPage
