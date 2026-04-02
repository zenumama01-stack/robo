from .item_request import ItemRequest
from ..request.item_create_session import ItemCreateSessionRequestBuilder
from ..request.item_copy import ItemCopyRequestBuilder
from ..request.item_create_link import ItemCreateLinkRequestBuilder
from ..request.item_invite import ItemInviteRequestBuilder
from ..request.item_delta import ItemDeltaRequestBuilder
from ..request.item_search import ItemSearchRequestBuilder
from ..request.item_content_request import ItemContentRequestBuilder
class ItemRequestBuilder(RequestBuilderBase):
        """Initialize the ItemRequestBuilder
        super(ItemRequestBuilder, self).__init__(request_url, client)
        """Builds the ItemRequest
            :class:`ItemRequest<onedrivesdk.request.item_request.ItemRequest>`:
                The ItemRequest
        req = ItemRequest(self._request_url, self._client, options)
                The updated Item
        return self.request().update(item)
    def upload(self, local_path):
            The created entity.
    def download(self, local_path):
        """Downloads the specified entity.
            local_path (str): The path where the entity should be
                downloaded to
        return self.content.request().download(local_path)
        """The permissions for the ItemRequestBuilder
            :class:`PermissionsCollectionRequestBuilder<onedrivesdk.request.permissions_collection.PermissionsCollectionRequestBuilder>`:
                A request builder created from the ItemRequestBuilder
        return PermissionsCollectionRequestBuilder(self.append_to_request_url("permissions"), self._client)
        """The subscriptions for the ItemRequestBuilder
            :class:`SubscriptionsCollectionRequestBuilder<onedrivesdk.request.subscriptions_collection.SubscriptionsCollectionRequestBuilder>`:
        return SubscriptionsCollectionRequestBuilder(self.append_to_request_url("subscriptions"), self._client)
        """The versions for the ItemRequestBuilder
            :class:`VersionsCollectionRequestBuilder<onedrivesdk.request.versions_collection.VersionsCollectionRequestBuilder>`:
        return VersionsCollectionRequestBuilder(self.append_to_request_url("versions"), self._client)
        """The children for the ItemRequestBuilder
            :class:`ChildrenCollectionRequestBuilder<onedrivesdk.request.children_collection.ChildrenCollectionRequestBuilder>`:
        return ChildrenCollectionRequestBuilder(self.append_to_request_url("children"), self._client)
        """The tags for the ItemRequestBuilder
            :class:`TagsCollectionRequestBuilder<onedrivesdk.request.tags_collection.TagsCollectionRequestBuilder>`:
        return TagsCollectionRequestBuilder(self.append_to_request_url("tags"), self._client)
        """The thumbnails for the ItemRequestBuilder
            :class:`ThumbnailsCollectionRequestBuilder<onedrivesdk.request.thumbnails_collection.ThumbnailsCollectionRequestBuilder>`:
        return ThumbnailsCollectionRequestBuilder(self.append_to_request_url("thumbnails"), self._client)
        """The content for the ItemRequestBuilder
            :class:`ItemContentRequestBuilder<onedrivesdk.request.item_content.ItemContentRequestBuilder>`:
        return ItemContentRequestBuilder(self.append_to_request_url("content"), self._client)
    def create_session(self, item=None):
        """Executes the createSession method
            item (:class:`ChunkedUploadSessionDescriptor<onedrivesdk.model.chunked_upload_session_descriptor.ChunkedUploadSessionDescriptor>`):
                Defaults to None, The item to use in the method request
            :class:`ItemCreateSessionRequestBuilder<onedrivesdk.request.item_create_session.ItemCreateSessionRequestBuilder>`:
                A ItemCreateSessionRequestBuilder for the method
        return ItemCreateSessionRequestBuilder(self.append_to_request_url("upload.createSession"), self._client, item=item)
    def copy(self, name=None, parent_reference=None):
        """Executes the copy method
            name (str):
                The name to use in the method request          
            parent_reference (:class:`ItemReference<onedrivesdk.model.item_reference.ItemReference>`):
                Defaults to None, The parent_reference to use in the method request
            :class:`ItemCopyRequestBuilder<onedrivesdk.request.item_copy.ItemCopyRequestBuilder>`:
                A ItemCopyRequestBuilder for the method
        return ItemCopyRequestBuilder(self.append_to_request_url("action.copy"), self._client, name=name, parent_reference=parent_reference)
    def create_link(self, type):
        """Executes the createLink method
            type (str):
                The type to use in the method request          
            :class:`ItemCreateLinkRequestBuilder<onedrivesdk.request.item_create_link.ItemCreateLinkRequestBuilder>`:
                A ItemCreateLinkRequestBuilder for the method
        return ItemCreateLinkRequestBuilder(self.append_to_request_url("action.createLink"), self._client, type)
    def invite(self, recipients, require_sign_in=None, roles=None, send_invitation=None, message=None):
        """Executes the invite method
            require_sign_in (bool):
                The require_sign_in to use in the method request          
            roles (str):
                The roles to use in the method request          
            recipients (:class:`Recipients<onedrivesdk.model.recipients.Recipients>`):
                The recipients to use in the method request
            send_invitation (bool):
                The send_invitation to use in the method request          
            message (str):
                The message to use in the method request          
            :class:`ItemInviteRequestBuilder<onedrivesdk.request.item_invite.ItemInviteRequestBuilder>`:
                A ItemInviteRequestBuilder for the method
        return ItemInviteRequestBuilder(self.append_to_request_url("action.invite"), self._client, recipients, require_sign_in=require_sign_in, roles=roles, send_invitation=send_invitation, message=message)
    def delta(self, token=None):
        """Executes the delta method
            token (str):
                The token to use in the method request          
            :class:`ItemDeltaRequestBuilder<onedrivesdk.request.item_delta.ItemDeltaRequestBuilder>`:
                A ItemDeltaRequestBuilder for the method
        return ItemDeltaRequestBuilder(self.append_to_request_url("view.delta"), self._client, token=token)
    def search(self, q=None):
        """Executes the search method
            q (str):
                The q to use in the method request          
            :class:`ItemSearchRequestBuilder<onedrivesdk.request.item_search.ItemSearchRequestBuilder>`:
                A ItemSearchRequestBuilder for the method
        return ItemSearchRequestBuilder(self.append_to_request_url("view.search"), self._client, q=q)
from ..request.permissions_collection import PermissionsCollectionRequestBuilder
from ..request.subscriptions_collection import SubscriptionsCollectionRequestBuilder
from ..request.versions_collection import VersionsCollectionRequestBuilder
from ..request.children_collection import ChildrenCollectionRequestBuilder
from ..request.tags_collection import TagsCollectionRequestBuilder
from ..request.thumbnails_collection import ThumbnailsCollectionRequestBuilder
        entity = yield from self.request().update_async(item)
    def upload_async(self, local_path):
        Returns: The created entity.
        entity = yield from self.content.request().upload_async(local_path)
    def download_async(self, local_path):
        """Downloads the specified entity in async.
        entity = yield from self.content.request().download_async(local_path)
