from ..model.item_reference import ItemReference
from ..model.audio import Audio
from ..model.deleted import Deleted
from ..model.file import File
from ..model.file_system_info import FileSystemInfo
from ..model.folder import Folder
from ..model.image import Image
from ..model.location import Location
from ..model.open_with_set import OpenWithSet
from ..model.photo import Photo
from ..model.search_result import SearchResult
from ..model.shared import Shared
from ..model.special_folder import SpecialFolder
from ..model.video import Video
from ..model.permission import Permission
from ..model.subscription import Subscription
from ..model.tag import Tag
class Item(OneDriveObjectBase):
    def created_by(self):
        Gets and sets the createdBy
                The createdBy
        if "createdBy" in self._prop_dict:
            if isinstance(self._prop_dict["createdBy"], OneDriveObjectBase):
                return self._prop_dict["createdBy"]
                self._prop_dict["createdBy"] = IdentitySet(self._prop_dict["createdBy"])
    @created_by.setter
    def created_by(self, val):
        self._prop_dict["createdBy"] = val
        Gets and sets the createdDateTime
    def c_tag(self):
        Gets and sets the cTag
                The cTag
        if "cTag" in self._prop_dict:
            return self._prop_dict["cTag"]
    @c_tag.setter
    def c_tag(self, val):
        self._prop_dict["cTag"] = val
    def description(self):
        Gets and sets the description
                The description
        if "description" in self._prop_dict:
            return self._prop_dict["description"]
    @description.setter
    def description(self, val):
        self._prop_dict["description"] = val
    def e_tag(self):
        Gets and sets the eTag
                The eTag
        if "eTag" in self._prop_dict:
            return self._prop_dict["eTag"]
    @e_tag.setter
    def e_tag(self, val):
        self._prop_dict["eTag"] = val
    def last_modified_by(self):
        Gets and sets the lastModifiedBy
                The lastModifiedBy
        if "lastModifiedBy" in self._prop_dict:
            if isinstance(self._prop_dict["lastModifiedBy"], OneDriveObjectBase):
                return self._prop_dict["lastModifiedBy"]
                self._prop_dict["lastModifiedBy"] = IdentitySet(self._prop_dict["lastModifiedBy"])
    @last_modified_by.setter
    def last_modified_by(self, val):
        self._prop_dict["lastModifiedBy"] = val
        Gets and sets the lastModifiedDateTime
        Gets and sets the name
    def parent_reference(self):
        Gets and sets the parentReference
            :class:`ItemReference<onedrivesdk.model.item_reference.ItemReference>`:
                The parentReference
        if "parentReference" in self._prop_dict:
            if isinstance(self._prop_dict["parentReference"], OneDriveObjectBase):
                return self._prop_dict["parentReference"]
                self._prop_dict["parentReference"] = ItemReference(self._prop_dict["parentReference"])
    @parent_reference.setter
    def parent_reference(self, val):
        self._prop_dict["parentReference"] = val
        Gets and sets the size
                The size
        if "size" in self._prop_dict:
            return self._prop_dict["size"]
    @size.setter
    def size(self, val):
        self._prop_dict["size"] = val
    def web_url(self):
        Gets and sets the webUrl
                The webUrl
        if "webUrl" in self._prop_dict:
            return self._prop_dict["webUrl"]
    @web_url.setter
    def web_url(self, val):
        self._prop_dict["webUrl"] = val
    def audio(self):
        Gets and sets the audio
            :class:`Audio<onedrivesdk.model.audio.Audio>`:
        if "audio" in self._prop_dict:
            if isinstance(self._prop_dict["audio"], OneDriveObjectBase):
                return self._prop_dict["audio"]
                self._prop_dict["audio"] = Audio(self._prop_dict["audio"])
    @audio.setter
    def audio(self, val):
        self._prop_dict["audio"] = val
    def deleted(self):
        Gets and sets the deleted
            :class:`Deleted<onedrivesdk.model.deleted.Deleted>`:
                The deleted
        if "deleted" in self._prop_dict:
            if isinstance(self._prop_dict["deleted"], OneDriveObjectBase):
                return self._prop_dict["deleted"]
                self._prop_dict["deleted"] = Deleted(self._prop_dict["deleted"])
    @deleted.setter
    def deleted(self, val):
        self._prop_dict["deleted"] = val
    def file(self):
        Gets and sets the file
            :class:`File<onedrivesdk.model.file.File>`:
                The file
        if "file" in self._prop_dict:
            if isinstance(self._prop_dict["file"], OneDriveObjectBase):
                return self._prop_dict["file"]
                self._prop_dict["file"] = File(self._prop_dict["file"])
    @file.setter
    def file(self, val):
        self._prop_dict["file"] = val
    def file_system_info(self):
        Gets and sets the fileSystemInfo
            :class:`FileSystemInfo<onedrivesdk.model.file_system_info.FileSystemInfo>`:
                The fileSystemInfo
        if "fileSystemInfo" in self._prop_dict:
            if isinstance(self._prop_dict["fileSystemInfo"], OneDriveObjectBase):
                return self._prop_dict["fileSystemInfo"]
                self._prop_dict["fileSystemInfo"] = FileSystemInfo(self._prop_dict["fileSystemInfo"])
    @file_system_info.setter
    def file_system_info(self, val):
        self._prop_dict["fileSystemInfo"] = val
    def folder(self):
        Gets and sets the folder
            :class:`Folder<onedrivesdk.model.folder.Folder>`:
                The folder
        if "folder" in self._prop_dict:
            if isinstance(self._prop_dict["folder"], OneDriveObjectBase):
                return self._prop_dict["folder"]
                self._prop_dict["folder"] = Folder(self._prop_dict["folder"])
    @folder.setter
    def folder(self, val):
        self._prop_dict["folder"] = val
    def image(self):
        Gets and sets the image
            :class:`Image<onedrivesdk.model.image.Image>`:
                The image
        if "image" in self._prop_dict:
            if isinstance(self._prop_dict["image"], OneDriveObjectBase):
                return self._prop_dict["image"]
                self._prop_dict["image"] = Image(self._prop_dict["image"])
    @image.setter
    def image(self, val):
        self._prop_dict["image"] = val
    def location(self):
        Gets and sets the location
            :class:`Location<onedrivesdk.model.location.Location>`:
                The location
        if "location" in self._prop_dict:
            if isinstance(self._prop_dict["location"], OneDriveObjectBase):
                return self._prop_dict["location"]
                self._prop_dict["location"] = Location(self._prop_dict["location"])
    @location.setter
    def location(self, val):
        self._prop_dict["location"] = val
    def open_with(self):
        Gets and sets the openWith
            :class:`OpenWithSet<onedrivesdk.model.open_with_set.OpenWithSet>`:
                The openWith
        if "openWith" in self._prop_dict:
            if isinstance(self._prop_dict["openWith"], OneDriveObjectBase):
                return self._prop_dict["openWith"]
                self._prop_dict["openWith"] = OpenWithSet(self._prop_dict["openWith"])
    @open_with.setter
    def open_with(self, val):
        self._prop_dict["openWith"] = val
    def photo(self):
        Gets and sets the photo
            :class:`Photo<onedrivesdk.model.photo.Photo>`:
                The photo
        if "photo" in self._prop_dict:
            if isinstance(self._prop_dict["photo"], OneDriveObjectBase):
                return self._prop_dict["photo"]
                self._prop_dict["photo"] = Photo(self._prop_dict["photo"])
    @photo.setter
    def photo(self, val):
        self._prop_dict["photo"] = val
    def remote_item(self):
        Gets and sets the remoteItem
                The remoteItem
        if "remoteItem" in self._prop_dict:
            if isinstance(self._prop_dict["remoteItem"], OneDriveObjectBase):
                return self._prop_dict["remoteItem"]
                self._prop_dict["remoteItem"] = Item(self._prop_dict["remoteItem"])
    @remote_item.setter
    def remote_item(self, val):
        self._prop_dict["remoteItem"] = val
    def search_result(self):
        Gets and sets the searchResult
            :class:`SearchResult<onedrivesdk.model.search_result.SearchResult>`:
                The searchResult
        if "searchResult" in self._prop_dict:
            if isinstance(self._prop_dict["searchResult"], OneDriveObjectBase):
                return self._prop_dict["searchResult"]
                self._prop_dict["searchResult"] = SearchResult(self._prop_dict["searchResult"])
    @search_result.setter
    def search_result(self, val):
        self._prop_dict["searchResult"] = val
        Gets and sets the shared
            :class:`Shared<onedrivesdk.model.shared.Shared>`:
            if isinstance(self._prop_dict["shared"], OneDriveObjectBase):
                return self._prop_dict["shared"]
                self._prop_dict["shared"] = Shared(self._prop_dict["shared"])
    @shared.setter
    def shared(self, val):
        self._prop_dict["shared"] = val
    def special_folder(self):
        Gets and sets the specialFolder
            :class:`SpecialFolder<onedrivesdk.model.special_folder.SpecialFolder>`:
                The specialFolder
        if "specialFolder" in self._prop_dict:
            if isinstance(self._prop_dict["specialFolder"], OneDriveObjectBase):
                return self._prop_dict["specialFolder"]
                self._prop_dict["specialFolder"] = SpecialFolder(self._prop_dict["specialFolder"])
    @special_folder.setter
    def special_folder(self, val):
        self._prop_dict["specialFolder"] = val
    def video(self):
        Gets and sets the video
            :class:`Video<onedrivesdk.model.video.Video>`:
                The video
        if "video" in self._prop_dict:
            if isinstance(self._prop_dict["video"], OneDriveObjectBase):
                return self._prop_dict["video"]
                self._prop_dict["video"] = Video(self._prop_dict["video"])
    @video.setter
    def video(self, val):
        self._prop_dict["video"] = val
    def permissions(self):
        """Gets and sets the permissions
            :class:`PermissionsCollectionPage<onedrivesdk.request.permissions_collection.PermissionsCollectionPage>`:
                The permissions
        if "permissions" in self._prop_dict:
            return PermissionsCollectionPage(self._prop_dict["permissions"])
    def subscriptions(self):
        """Gets and sets the subscriptions
            :class:`SubscriptionsCollectionPage<onedrivesdk.request.subscriptions_collection.SubscriptionsCollectionPage>`:
                The subscriptions
        if "subscriptions" in self._prop_dict:
            return SubscriptionsCollectionPage(self._prop_dict["subscriptions"])
    def versions(self):
        """Gets and sets the versions
            :class:`VersionsCollectionPage<onedrivesdk.request.versions_collection.VersionsCollectionPage>`:
                The versions
        if "versions" in self._prop_dict:
            return VersionsCollectionPage(self._prop_dict["versions"])
        """Gets and sets the children
            :class:`ChildrenCollectionPage<onedrivesdk.request.children_collection.ChildrenCollectionPage>`:
                The children
        if "children" in self._prop_dict:
            return ChildrenCollectionPage(self._prop_dict["children"])
    def tags(self):
        """Gets and sets the tags
            :class:`TagsCollectionPage<onedrivesdk.request.tags_collection.TagsCollectionPage>`:
                The tags
        if "tags" in self._prop_dict:
            return TagsCollectionPage(self._prop_dict["tags"])
        """Gets and sets the thumbnails
            :class:`ThumbnailsCollectionPage<onedrivesdk.request.thumbnails_collection.ThumbnailsCollectionPage>`:
            return ThumbnailsCollectionPage(self._prop_dict["thumbnails"])
from ..model.permissions_collection_page import PermissionsCollectionPage
from ..model.subscriptions_collection_page import SubscriptionsCollectionPage
from ..model.versions_collection_page import VersionsCollectionPage
from ..model.children_collection_page import ChildrenCollectionPage
from ..model.tags_collection_page import TagsCollectionPage
from ..model.thumbnails_collection_page import ThumbnailsCollectionPage
