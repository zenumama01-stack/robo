This namespace contains endpoints and data types for creating and managing shared links and shared folders.
class AccessInheritance(bb.Union):
    Information about the inheritance policy of a shared folder.
    :ivar sharing.AccessInheritance.inherit: The shared folder inherits its
        members from the parent folder.
    :ivar sharing.AccessInheritance.no_inherit: The shared folder does not
        inherit its members from the parent folder.
    inherit = None
    no_inherit = None
    def is_inherit(self):
        Check if the union tag is ``inherit``.
        return self._tag == 'inherit'
    def is_no_inherit(self):
        Check if the union tag is ``no_inherit``.
        return self._tag == 'no_inherit'
        super(AccessInheritance, self)._process_custom_annotations(annotation_type, field_path, processor)
AccessInheritance_validator = bv.Union(AccessInheritance)
class AccessLevel(bb.Union):
    Defines the access levels for collaborators.
    :ivar sharing.AccessLevel.owner: The collaborator is the owner of the shared
        folder. Owners can view and edit the shared folder as well as set the
        folder's policies using
        :meth:`dropbox.dropbox_client.Dropbox.sharing_update_folder_policy`.
    :ivar sharing.AccessLevel.editor: The collaborator can both view and edit
    :ivar sharing.AccessLevel.viewer: The collaborator can only view the shared
    :ivar sharing.AccessLevel.viewer_no_comment: The collaborator can only view
        the shared folder and does not have any access to comments.
    :ivar sharing.AccessLevel.traverse: The collaborator can only view the
        shared folder that they have access to.
    :ivar sharing.AccessLevel.no_access: If there is a Righteous Link on the
        folder which grants access and the user has visited such link, they are
        allowed to perform certain action (i.e. add themselves to the folder)
        via the link access even though the user themselves are not a member on
        the shared folder yet.
    owner = None
    editor = None
    viewer = None
    viewer_no_comment = None
    traverse = None
    no_access = None
    def is_owner(self):
        Check if the union tag is ``owner``.
        return self._tag == 'owner'
    def is_editor(self):
        Check if the union tag is ``editor``.
        return self._tag == 'editor'
    def is_viewer(self):
        Check if the union tag is ``viewer``.
        return self._tag == 'viewer'
    def is_viewer_no_comment(self):
        Check if the union tag is ``viewer_no_comment``.
        return self._tag == 'viewer_no_comment'
    def is_traverse(self):
        Check if the union tag is ``traverse``.
        return self._tag == 'traverse'
    def is_no_access(self):
        Check if the union tag is ``no_access``.
        return self._tag == 'no_access'
        super(AccessLevel, self)._process_custom_annotations(annotation_type, field_path, processor)
AccessLevel_validator = bv.Union(AccessLevel)
class AclUpdatePolicy(bb.Union):
    Who can change a shared folder's access control list (ACL). In other words,
    who can add, remove, or change the privileges of members.
    :ivar sharing.AclUpdatePolicy.owner: Only the owner can update the ACL.
    :ivar sharing.AclUpdatePolicy.editors: Any editor can update the ACL. This
        may be further restricted to editors on the same team.
    editors = None
    def is_editors(self):
        Check if the union tag is ``editors``.
        return self._tag == 'editors'
        super(AclUpdatePolicy, self)._process_custom_annotations(annotation_type, field_path, processor)
AclUpdatePolicy_validator = bv.Union(AclUpdatePolicy)
class AddFileMemberArgs(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.sharing_add_file_member`.
    :ivar sharing.AddFileMemberArgs.file: File to which to add members.
    :ivar sharing.AddFileMemberArgs.members: Members to add. Note that even an
        email address is given, this may result in a user being directly added
        to the membership if that email is the user's main account email.
    :ivar sharing.AddFileMemberArgs.custom_message: Message to send to added
        members in their invitation.
    :ivar sharing.AddFileMemberArgs.quiet: Whether added members should be
        notified via email and device notifications of their invitation.
    :ivar sharing.AddFileMemberArgs.access_level: AccessLevel union object,
        describing what access level we want to give new members.
    :ivar sharing.AddFileMemberArgs.add_message_as_comment: If the custom
        message should be added as a comment on the file.
        '_file_value',
        '_access_level_value',
        '_add_message_as_comment_value',
                 file=None,
                 quiet=None,
                 access_level=None,
                 add_message_as_comment=None):
        self._file_value = bb.NOT_SET
        self._access_level_value = bb.NOT_SET
        self._add_message_as_comment_value = bb.NOT_SET
            self.file = file
        if access_level is not None:
            self.access_level = access_level
        if add_message_as_comment is not None:
            self.add_message_as_comment = add_message_as_comment
    file = bb.Attribute("file")
    # Instance attribute type: list of [MemberSelector] (validator is set below)
    # Instance attribute type: AccessLevel (validator is set below)
    access_level = bb.Attribute("access_level", user_defined=True)
    add_message_as_comment = bb.Attribute("add_message_as_comment")
        super(AddFileMemberArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
AddFileMemberArgs_validator = bv.Struct(AddFileMemberArgs)
class AddFileMemberError(bb.Union):
    Errors for :meth:`dropbox.dropbox_client.Dropbox.sharing_add_file_member`.
    :ivar sharing.AddFileMemberError.rate_limit: The user has reached the rate
        limit for invitations.
    :ivar sharing.AddFileMemberError.invalid_comment: The custom message did not
        pass comment permissions checks.
    invalid_comment = None
    def user_error(cls, val):
        Create an instance of this class set to the ``user_error`` tag with
        :param SharingUserError val:
        :rtype: AddFileMemberError
        return cls('user_error', val)
    def access_error(cls, val):
        Create an instance of this class set to the ``access_error`` tag with
        :param SharingFileAccessError val:
        return cls('access_error', val)
    def is_user_error(self):
        Check if the union tag is ``user_error``.
        return self._tag == 'user_error'
    def is_access_error(self):
        Check if the union tag is ``access_error``.
        return self._tag == 'access_error'
    def is_invalid_comment(self):
        Check if the union tag is ``invalid_comment``.
        return self._tag == 'invalid_comment'
    def get_user_error(self):
        Only call this if :meth:`is_user_error` is true.
        :rtype: SharingUserError
        if not self.is_user_error():
            raise AttributeError("tag 'user_error' not set")
    def get_access_error(self):
        Only call this if :meth:`is_access_error` is true.
        :rtype: SharingFileAccessError
        if not self.is_access_error():
            raise AttributeError("tag 'access_error' not set")
        super(AddFileMemberError, self)._process_custom_annotations(annotation_type, field_path, processor)
AddFileMemberError_validator = bv.Union(AddFileMemberError)
class AddFolderMemberArg(bb.Struct):
    :ivar sharing.AddFolderMemberArg.shared_folder_id: The ID for the shared
    :ivar sharing.AddFolderMemberArg.members: The intended list of members to
        add.  Added members will receive invites to join the shared folder.
    :ivar sharing.AddFolderMemberArg.quiet: Whether added members should be
        notified via email and device notifications of their invite.
    :ivar sharing.AddFolderMemberArg.custom_message: Optional message to display
        to added members in their invitation.
    shared_folder_id = bb.Attribute("shared_folder_id")
        super(AddFolderMemberArg, self)._process_custom_annotations(annotation_type, field_path, processor)
AddFolderMemberArg_validator = bv.Struct(AddFolderMemberArg)
class AddFolderMemberError(bb.Union):
    :ivar SharedFolderAccessError AddFolderMemberError.access_error: Unable to
        access shared folder.
    :ivar sharing.AddFolderMemberError.email_unverified: This user's email
    :ivar sharing.AddFolderMemberError.banned_member: The current user has been
        banned.
    :ivar AddMemberSelectorError AddFolderMemberError.bad_member:
        ``AddFolderMemberArg.members`` contains a bad invitation recipient.
    :ivar sharing.AddFolderMemberError.cant_share_outside_team: Your team policy
        does not allow sharing outside of the team.
    :ivar int sharing.AddFolderMemberError.too_many_members: The value is the
        member limit that was reached.
    :ivar int sharing.AddFolderMemberError.too_many_pending_invites: The value
        is the pending invite limit that was reached.
    :ivar sharing.AddFolderMemberError.rate_limit: The current user has hit the
        limit of invites they can send per day. Try again in 24 hours.
    :ivar sharing.AddFolderMemberError.too_many_invitees: The current user is
        trying to share with too many people at once.
    :ivar sharing.AddFolderMemberError.insufficient_plan: The current user's
        account doesn't support this action. An example of this is when adding a
        read-only member. This action can only be performed by users that have
        upgraded to a Pro or Business plan.
    :ivar sharing.AddFolderMemberError.team_folder: This action cannot be
        performed on a team shared folder.
    :ivar sharing.AddFolderMemberError.no_permission: The current user does not
        have permission to perform this action.
    :ivar sharing.AddFolderMemberError.invalid_shared_folder: Invalid shared
        folder error will be returned as an access_error.
    banned_member = None
    cant_share_outside_team = None
    too_many_invitees = None
    insufficient_plan = None
    invalid_shared_folder = None
        :param SharedFolderAccessError val:
        :rtype: AddFolderMemberError
    def bad_member(cls, val):
        Create an instance of this class set to the ``bad_member`` tag with
        :param AddMemberSelectorError val:
        return cls('bad_member', val)
    def too_many_members(cls, val):
        Create an instance of this class set to the ``too_many_members`` tag
        :param int val:
        return cls('too_many_members', val)
    def too_many_pending_invites(cls, val):
        Create an instance of this class set to the ``too_many_pending_invites``
        return cls('too_many_pending_invites', val)
    def is_banned_member(self):
        Check if the union tag is ``banned_member``.
        return self._tag == 'banned_member'
    def is_bad_member(self):
        Check if the union tag is ``bad_member``.
        return self._tag == 'bad_member'
    def is_cant_share_outside_team(self):
        Check if the union tag is ``cant_share_outside_team``.
        return self._tag == 'cant_share_outside_team'
    def is_too_many_members(self):
        Check if the union tag is ``too_many_members``.
        return self._tag == 'too_many_members'
    def is_too_many_pending_invites(self):
        Check if the union tag is ``too_many_pending_invites``.
        return self._tag == 'too_many_pending_invites'
    def is_too_many_invitees(self):
        Check if the union tag is ``too_many_invitees``.
        return self._tag == 'too_many_invitees'
    def is_insufficient_plan(self):
        Check if the union tag is ``insufficient_plan``.
        return self._tag == 'insufficient_plan'
    def is_invalid_shared_folder(self):
        Check if the union tag is ``invalid_shared_folder``.
        return self._tag == 'invalid_shared_folder'
        Unable to access shared folder.
        :rtype: SharedFolderAccessError
    def get_bad_member(self):
        Only call this if :meth:`is_bad_member` is true.
        :rtype: AddMemberSelectorError
        if not self.is_bad_member():
            raise AttributeError("tag 'bad_member' not set")
    def get_too_many_members(self):
        The value is the member limit that was reached.
        Only call this if :meth:`is_too_many_members` is true.
        :rtype: int
        if not self.is_too_many_members():
            raise AttributeError("tag 'too_many_members' not set")
    def get_too_many_pending_invites(self):
        The value is the pending invite limit that was reached.
        Only call this if :meth:`is_too_many_pending_invites` is true.
        if not self.is_too_many_pending_invites():
            raise AttributeError("tag 'too_many_pending_invites' not set")
        super(AddFolderMemberError, self)._process_custom_annotations(annotation_type, field_path, processor)
AddFolderMemberError_validator = bv.Union(AddFolderMemberError)
    The member and type of access the member should have when added to a shared
    :ivar sharing.AddMember.member: The member to add to the shared folder.
    :ivar sharing.AddMember.access_level: The access level to grant ``member``
        to the shared folder.  ``AccessLevel.owner`` is disallowed.
                 access_level=None):
    # Instance attribute type: MemberSelector (validator is set below)
class AddMemberSelectorError(bb.Union):
    :ivar sharing.AddMemberSelectorError.automatic_group: Automatically created
        groups can only be added to team folders.
    :ivar str sharing.AddMemberSelectorError.invalid_dropbox_id: The value is
        the ID that could not be identified.
    :ivar str sharing.AddMemberSelectorError.invalid_email: The value is the
        e-email address that is malformed.
    :ivar str sharing.AddMemberSelectorError.unverified_dropbox_id: The value is
        the ID of the Dropbox user with an unverified email address. Invite
        unverified users by email address instead of by their Dropbox ID.
    :ivar sharing.AddMemberSelectorError.group_deleted: At least one of the
        specified groups in ``AddFolderMemberArg.members`` is deleted.
    :ivar sharing.AddMemberSelectorError.group_not_on_team: Sharing to a group
        that is not on the current user's team.
    automatic_group = None
    group_deleted = None
    group_not_on_team = None
    def invalid_dropbox_id(cls, val):
        Create an instance of this class set to the ``invalid_dropbox_id`` tag
        return cls('invalid_dropbox_id', val)
    def invalid_email(cls, val):
        Create an instance of this class set to the ``invalid_email`` tag with
        return cls('invalid_email', val)
    def unverified_dropbox_id(cls, val):
        Create an instance of this class set to the ``unverified_dropbox_id``
        return cls('unverified_dropbox_id', val)
    def is_automatic_group(self):
        Check if the union tag is ``automatic_group``.
        return self._tag == 'automatic_group'
    def is_invalid_dropbox_id(self):
        Check if the union tag is ``invalid_dropbox_id``.
        return self._tag == 'invalid_dropbox_id'
    def is_invalid_email(self):
        Check if the union tag is ``invalid_email``.
        return self._tag == 'invalid_email'
    def is_unverified_dropbox_id(self):
        Check if the union tag is ``unverified_dropbox_id``.
        return self._tag == 'unverified_dropbox_id'
    def is_group_deleted(self):
        Check if the union tag is ``group_deleted``.
        return self._tag == 'group_deleted'
    def is_group_not_on_team(self):
        Check if the union tag is ``group_not_on_team``.
        return self._tag == 'group_not_on_team'
    def get_invalid_dropbox_id(self):
        The value is the ID that could not be identified.
        Only call this if :meth:`is_invalid_dropbox_id` is true.
        if not self.is_invalid_dropbox_id():
            raise AttributeError("tag 'invalid_dropbox_id' not set")
    def get_invalid_email(self):
        The value is the e-email address that is malformed.
        Only call this if :meth:`is_invalid_email` is true.
        if not self.is_invalid_email():
            raise AttributeError("tag 'invalid_email' not set")
    def get_unverified_dropbox_id(self):
        The value is the ID of the Dropbox user with an unverified email
        address. Invite unverified users by email address instead of by their
        Dropbox ID.
        Only call this if :meth:`is_unverified_dropbox_id` is true.
        if not self.is_unverified_dropbox_id():
            raise AttributeError("tag 'unverified_dropbox_id' not set")
        super(AddMemberSelectorError, self)._process_custom_annotations(annotation_type, field_path, processor)
AddMemberSelectorError_validator = bv.Union(AddMemberSelectorError)
class RequestedVisibility(bb.Union):
    The access permission that can be requested by the caller for the shared
    link. Note that the final resolved visibility of the shared link takes into
    account other aspects, such as team and shared folder settings. Check the
    :class:`ResolvedVisibility` for more info on the possible resolved
    visibility values of shared links.
    :ivar sharing.RequestedVisibility.public: Anyone who has received the link
        can access it. No login required.
    :ivar sharing.RequestedVisibility.team_only: Only members of the same team
        can access the link. Login is required.
    :ivar sharing.RequestedVisibility.password: A link-specific password is
        required to access the link. Login is not required.
    public = None
    team_only = None
    def is_public(self):
        Check if the union tag is ``public``.
        return self._tag == 'public'
    def is_team_only(self):
        Check if the union tag is ``team_only``.
        return self._tag == 'team_only'
    def is_password(self):
        Check if the union tag is ``password``.
        return self._tag == 'password'
        super(RequestedVisibility, self)._process_custom_annotations(annotation_type, field_path, processor)
RequestedVisibility_validator = bv.Union(RequestedVisibility)
class ResolvedVisibility(RequestedVisibility):
    The actual access permissions values of shared links after taking into
    account user preferences and the team and shared folder settings. Check the
    :class:`RequestedVisibility` for more info on the possible visibility values
    that can be set by the shared link's owner.
    :ivar sharing.ResolvedVisibility.team_and_password: Only members of the same
        team who have the link-specific password can access the link. Login is
        required.
    :ivar sharing.ResolvedVisibility.shared_folder_only: Only members of the
        shared folder containing the linked file can access the link. Login is
    :ivar sharing.ResolvedVisibility.no_one: The link merely points the user to
        the content, and does not grant any additional rights. Existing members
        of the content who use this link can only access the content with their
        pre-existing access rights. Either on the file directly, or inherited
        from a parent folder.
    :ivar sharing.ResolvedVisibility.only_you: Only the current user can view
        this link.
    team_and_password = None
    shared_folder_only = None
    no_one = None
    only_you = None
    def is_team_and_password(self):
        Check if the union tag is ``team_and_password``.
        return self._tag == 'team_and_password'
    def is_shared_folder_only(self):
        Check if the union tag is ``shared_folder_only``.
        return self._tag == 'shared_folder_only'
    def is_no_one(self):
        Check if the union tag is ``no_one``.
        return self._tag == 'no_one'
    def is_only_you(self):
        Check if the union tag is ``only_you``.
        return self._tag == 'only_you'
        super(ResolvedVisibility, self)._process_custom_annotations(annotation_type, field_path, processor)
ResolvedVisibility_validator = bv.Union(ResolvedVisibility)
class AlphaResolvedVisibility(ResolvedVisibility):
    check documentation for ResolvedVisibility.
        super(AlphaResolvedVisibility, self)._process_custom_annotations(annotation_type, field_path, processor)
AlphaResolvedVisibility_validator = bv.Union(AlphaResolvedVisibility)
class AudienceExceptionContentInfo(bb.Struct):
    Information about the content that has a link audience different than that
    of this folder.
    :ivar sharing.AudienceExceptionContentInfo.name: The name of the content,
        which is either a file or a folder.
        super(AudienceExceptionContentInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
AudienceExceptionContentInfo_validator = bv.Struct(AudienceExceptionContentInfo)
class AudienceExceptions(bb.Struct):
    The total count and truncated list of information of content inside this
    folder that has a different audience than the link on this folder. This is
    only returned for folders.
    :ivar sharing.AudienceExceptions.exceptions: A truncated list of some of the
        content that is an exception. The length of this list could be smaller
        than the count since it is only a sample but will not be empty as long
        as count is not 0.
        '_count_value',
        '_exceptions_value',
                 count=None,
                 exceptions=None):
        self._count_value = bb.NOT_SET
        self._exceptions_value = bb.NOT_SET
        if count is not None:
            self.count = count
        if exceptions is not None:
            self.exceptions = exceptions
    count = bb.Attribute("count")
    # Instance attribute type: list of [AudienceExceptionContentInfo] (validator is set below)
    exceptions = bb.Attribute("exceptions")
        super(AudienceExceptions, self)._process_custom_annotations(annotation_type, field_path, processor)
AudienceExceptions_validator = bv.Struct(AudienceExceptions)
class AudienceRestrictingSharedFolder(bb.Struct):
    Information about the shared folder that prevents the link audience for this
    link from being more restrictive.
    :ivar sharing.AudienceRestrictingSharedFolder.shared_folder_id: The ID of
    :ivar sharing.AudienceRestrictingSharedFolder.name: The name of the shared
    :ivar sharing.AudienceRestrictingSharedFolder.audience: The link audience of
        '_audience_value',
                 audience=None):
        self._audience_value = bb.NOT_SET
        if audience is not None:
            self.audience = audience
    # Instance attribute type: LinkAudience (validator is set below)
    audience = bb.Attribute("audience", user_defined=True)
        super(AudienceRestrictingSharedFolder, self)._process_custom_annotations(annotation_type, field_path, processor)
AudienceRestrictingSharedFolder_validator = bv.Struct(AudienceRestrictingSharedFolder)
class LinkMetadata(bb.Struct):
    Metadata for a shared link. This can be either a :class:`PathLinkMetadata`
    or :class:`CollectionLinkMetadata`.
    :ivar sharing.LinkMetadata.url: URL of the shared link.
    :ivar sharing.LinkMetadata.visibility: Who can access the link.
    :ivar sharing.LinkMetadata.expires: Expiration time, if set. By default the
        link won't expire.
        '_visibility_value',
                 visibility=None,
        self._visibility_value = bb.NOT_SET
        if visibility is not None:
            self.visibility = visibility
    # Instance attribute type: Visibility (validator is set below)
    visibility = bb.Attribute("visibility", user_defined=True)
    expires = bb.Attribute("expires", nullable=True)
        super(LinkMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
LinkMetadata_validator = bv.StructTree(LinkMetadata)
class CollectionLinkMetadata(LinkMetadata):
    Metadata for a collection-based shared link.
        super(CollectionLinkMetadata, self).__init__(url,
                                                     expires)
        super(CollectionLinkMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
CollectionLinkMetadata_validator = bv.Struct(CollectionLinkMetadata)
class CreateSharedLinkArg(bb.Struct):
    :ivar sharing.CreateSharedLinkArg.path: The path to share.
    :ivar sharing.CreateSharedLinkArg.pending_upload: If it's okay to share a
        path that does not yet exist, set this to either
        ``PendingUploadMode.file`` or ``PendingUploadMode.folder`` to indicate
        whether to assume it's a file or folder.
        '_short_url_value',
        '_pending_upload_value',
                 short_url=None,
        self._short_url_value = bb.NOT_SET
        self._pending_upload_value = bb.NOT_SET
        if short_url is not None:
            self.short_url = short_url
        if pending_upload is not None:
            self.pending_upload = pending_upload
    short_url = bb.Attribute("short_url")
    # Instance attribute type: PendingUploadMode (validator is set below)
    pending_upload = bb.Attribute("pending_upload", nullable=True, user_defined=True)
        super(CreateSharedLinkArg, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateSharedLinkArg_validator = bv.Struct(CreateSharedLinkArg)
class CreateSharedLinkError(bb.Union):
        :param files.LookupError val:
        :rtype: CreateSharedLinkError
        :rtype: files.LookupError
        super(CreateSharedLinkError, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateSharedLinkError_validator = bv.Union(CreateSharedLinkError)
class CreateSharedLinkWithSettingsArg(bb.Struct):
    :ivar sharing.CreateSharedLinkWithSettingsArg.path: The path to be shared by
    :ivar sharing.CreateSharedLinkWithSettingsArg.settings: The requested
        settings for the newly created shared link.
        '_settings_value',
        self._settings_value = bb.NOT_SET
        if settings is not None:
            self.settings = settings
    # Instance attribute type: SharedLinkSettings (validator is set below)
    settings = bb.Attribute("settings", nullable=True, user_defined=True)
        super(CreateSharedLinkWithSettingsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateSharedLinkWithSettingsArg_validator = bv.Struct(CreateSharedLinkWithSettingsArg)
class CreateSharedLinkWithSettingsError(bb.Union):
    :ivar sharing.CreateSharedLinkWithSettingsError.email_not_verified: This
        user's email address is not verified. This functionality is only
        available on accounts with a verified email address. Users can verify
        their email address `here <https://www.dropbox.com/help/317>`_.
    :ivar Optional[SharedLinkAlreadyExistsMetadata]
        sharing.CreateSharedLinkWithSettingsError.shared_link_already_exists:
        The shared link already exists. You can call :route:`list_shared_links`
        to get the  existing link, or use the provided metadata if it is
    :ivar SharedLinkSettingsError
        CreateSharedLinkWithSettingsError.settings_error: There is an error with
        the given settings.
    :ivar sharing.CreateSharedLinkWithSettingsError.access_denied: The user is
        not allowed to create a shared link to the specified file. For  example,
        this can occur if the file is restricted or if the user's links are
        `banned <https://help.dropbox.com/files-folders/share/banned-links>`_.
        :rtype: CreateSharedLinkWithSettingsError
    def shared_link_already_exists(cls, val):
        Create an instance of this class set to the
        ``shared_link_already_exists`` tag with value ``val``.
        :param SharedLinkAlreadyExistsMetadata val:
        return cls('shared_link_already_exists', val)
    def settings_error(cls, val):
        Create an instance of this class set to the ``settings_error`` tag with
        :param SharedLinkSettingsError val:
        return cls('settings_error', val)
    def is_shared_link_already_exists(self):
        Check if the union tag is ``shared_link_already_exists``.
        return self._tag == 'shared_link_already_exists'
    def is_settings_error(self):
        Check if the union tag is ``settings_error``.
        return self._tag == 'settings_error'
    def get_shared_link_already_exists(self):
        The shared link already exists. You can call
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_shared_links` to get
        the  existing link, or use the provided metadata if it is returned.
        Only call this if :meth:`is_shared_link_already_exists` is true.
        :rtype: SharedLinkAlreadyExistsMetadata
        if not self.is_shared_link_already_exists():
            raise AttributeError("tag 'shared_link_already_exists' not set")
    def get_settings_error(self):
        There is an error with the given settings.
        Only call this if :meth:`is_settings_error` is true.
        :rtype: SharedLinkSettingsError
        if not self.is_settings_error():
            raise AttributeError("tag 'settings_error' not set")
        super(CreateSharedLinkWithSettingsError, self)._process_custom_annotations(annotation_type, field_path, processor)
CreateSharedLinkWithSettingsError_validator = bv.Union(CreateSharedLinkWithSettingsError)
class SharedContentLinkMetadataBase(bb.Struct):
    :ivar sharing.SharedContentLinkMetadataBase.access_level: The access level
        on the link for this file.
    :ivar sharing.SharedContentLinkMetadataBase.audience_options: The audience
        options that are available for the content. Some audience options may be
        unavailable. For example, team_only may be unavailable if the content is
        not owned by a user on a team. The 'default' audience option is always
        available if the user can modify link settings.
    :ivar
        sharing.SharedContentLinkMetadataBase.audience_restricting_shared_folder:
        The shared folder that prevents the link audience for this link from
        being more restrictive.
    :ivar sharing.SharedContentLinkMetadataBase.current_audience: The current
        audience of the link.
    :ivar sharing.SharedContentLinkMetadataBase.expiry: Whether the link has an
        expiry set on it. A link with an expiry will have its  audience changed
        to members when the expiry is reached.
    :ivar sharing.SharedContentLinkMetadataBase.link_permissions: A list of
        permissions for actions you can perform on the link.
    :ivar sharing.SharedContentLinkMetadataBase.password_protected: Whether the
        link is protected by a password.
        '_audience_options_value',
        '_audience_restricting_shared_folder_value',
        '_current_audience_value',
        '_expiry_value',
        '_link_permissions_value',
        '_password_protected_value',
                 audience_options=None,
                 current_audience=None,
                 link_permissions=None,
                 password_protected=None,
                 audience_restricting_shared_folder=None,
                 expiry=None):
        self._audience_options_value = bb.NOT_SET
        self._audience_restricting_shared_folder_value = bb.NOT_SET
        self._current_audience_value = bb.NOT_SET
        self._expiry_value = bb.NOT_SET
        self._link_permissions_value = bb.NOT_SET
        self._password_protected_value = bb.NOT_SET
        if audience_options is not None:
            self.audience_options = audience_options
        if audience_restricting_shared_folder is not None:
            self.audience_restricting_shared_folder = audience_restricting_shared_folder
        if current_audience is not None:
            self.current_audience = current_audience
        if expiry is not None:
            self.expiry = expiry
        if link_permissions is not None:
            self.link_permissions = link_permissions
        if password_protected is not None:
            self.password_protected = password_protected
    access_level = bb.Attribute("access_level", nullable=True, user_defined=True)
    # Instance attribute type: list of [LinkAudience] (validator is set below)
    audience_options = bb.Attribute("audience_options")
    # Instance attribute type: AudienceRestrictingSharedFolder (validator is set below)
    audience_restricting_shared_folder = bb.Attribute("audience_restricting_shared_folder", nullable=True, user_defined=True)
    current_audience = bb.Attribute("current_audience", user_defined=True)
    expiry = bb.Attribute("expiry", nullable=True)
    # Instance attribute type: list of [LinkPermission] (validator is set below)
    link_permissions = bb.Attribute("link_permissions")
    password_protected = bb.Attribute("password_protected")
        super(SharedContentLinkMetadataBase, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedContentLinkMetadataBase_validator = bv.Struct(SharedContentLinkMetadataBase)
class ExpectedSharedContentLinkMetadata(SharedContentLinkMetadataBase):
    The expected metadata of a shared link for a file or folder when a link is
    first created for the content. Absent if the link already exists.
        super(ExpectedSharedContentLinkMetadata, self).__init__(audience_options,
                                                                current_audience,
                                                                link_permissions,
                                                                password_protected,
                                                                audience_restricting_shared_folder,
                                                                expiry)
        super(ExpectedSharedContentLinkMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
ExpectedSharedContentLinkMetadata_validator = bv.Struct(ExpectedSharedContentLinkMetadata)
class FileAction(bb.Union):
    Sharing actions that may be taken on files.
    :ivar sharing.FileAction.disable_viewer_info: Disable viewer information on
        the file.
    :ivar sharing.FileAction.edit_contents: Change or edit contents of the file.
    :ivar sharing.FileAction.enable_viewer_info: Enable viewer information on
    :ivar sharing.FileAction.invite_viewer: Add a member with view permissions.
    :ivar sharing.FileAction.invite_viewer_no_comment: Add a member with view
        permissions but no comment permissions.
    :ivar sharing.FileAction.invite_editor: Add a member with edit permissions.
    :ivar sharing.FileAction.unshare: Stop sharing this file.
    :ivar sharing.FileAction.relinquish_membership: Relinquish one's own
        membership to the file.
    :ivar sharing.FileAction.share_link: Use create_view_link and
        create_edit_link instead.
    :ivar sharing.FileAction.create_link: Use create_view_link and
    :ivar sharing.FileAction.create_view_link: Create a shared link to a file
        that only allows users to view the content.
    :ivar sharing.FileAction.create_edit_link: Create a shared link to a file
        that allows users to edit the content.
    disable_viewer_info = None
    edit_contents = None
    enable_viewer_info = None
    invite_viewer = None
    invite_viewer_no_comment = None
    invite_editor = None
    unshare = None
    relinquish_membership = None
    share_link = None
    create_link = None
    create_view_link = None
    create_edit_link = None
    def is_disable_viewer_info(self):
        Check if the union tag is ``disable_viewer_info``.
        return self._tag == 'disable_viewer_info'
    def is_edit_contents(self):
        Check if the union tag is ``edit_contents``.
        return self._tag == 'edit_contents'
    def is_enable_viewer_info(self):
        Check if the union tag is ``enable_viewer_info``.
        return self._tag == 'enable_viewer_info'
    def is_invite_viewer(self):
        Check if the union tag is ``invite_viewer``.
        return self._tag == 'invite_viewer'
    def is_invite_viewer_no_comment(self):
        Check if the union tag is ``invite_viewer_no_comment``.
        return self._tag == 'invite_viewer_no_comment'
    def is_invite_editor(self):
        Check if the union tag is ``invite_editor``.
        return self._tag == 'invite_editor'
    def is_unshare(self):
        Check if the union tag is ``unshare``.
        return self._tag == 'unshare'
    def is_relinquish_membership(self):
        Check if the union tag is ``relinquish_membership``.
        return self._tag == 'relinquish_membership'
    def is_share_link(self):
        Check if the union tag is ``share_link``.
        return self._tag == 'share_link'
    def is_create_link(self):
        Check if the union tag is ``create_link``.
        return self._tag == 'create_link'
    def is_create_view_link(self):
        Check if the union tag is ``create_view_link``.
        return self._tag == 'create_view_link'
    def is_create_edit_link(self):
        Check if the union tag is ``create_edit_link``.
        return self._tag == 'create_edit_link'
        super(FileAction, self)._process_custom_annotations(annotation_type, field_path, processor)
FileAction_validator = bv.Union(FileAction)
class FileErrorResult(bb.Union):
    :ivar str sharing.FileErrorResult.file_not_found_error: File specified by id
        was not found.
    :ivar str sharing.FileErrorResult.invalid_file_action_error: User does not
        have permission to take the specified action on the file.
    :ivar str sharing.FileErrorResult.permission_denied_error: User does not
        have permission to access file specified by file.Id.
    def file_not_found_error(cls, val):
        Create an instance of this class set to the ``file_not_found_error`` tag
        :rtype: FileErrorResult
        return cls('file_not_found_error', val)
    def invalid_file_action_error(cls, val):
        ``invalid_file_action_error`` tag with value ``val``.
        return cls('invalid_file_action_error', val)
    def permission_denied_error(cls, val):
        Create an instance of this class set to the ``permission_denied_error``
        return cls('permission_denied_error', val)
    def is_file_not_found_error(self):
        Check if the union tag is ``file_not_found_error``.
        return self._tag == 'file_not_found_error'
    def is_invalid_file_action_error(self):
        Check if the union tag is ``invalid_file_action_error``.
        return self._tag == 'invalid_file_action_error'
    def is_permission_denied_error(self):
        Check if the union tag is ``permission_denied_error``.
        return self._tag == 'permission_denied_error'
    def get_file_not_found_error(self):
        File specified by id was not found.
        Only call this if :meth:`is_file_not_found_error` is true.
        if not self.is_file_not_found_error():
            raise AttributeError("tag 'file_not_found_error' not set")
    def get_invalid_file_action_error(self):
        User does not have permission to take the specified action on the file.
        Only call this if :meth:`is_invalid_file_action_error` is true.
        if not self.is_invalid_file_action_error():
            raise AttributeError("tag 'invalid_file_action_error' not set")
    def get_permission_denied_error(self):
        User does not have permission to access file specified by file.Id.
        Only call this if :meth:`is_permission_denied_error` is true.
        if not self.is_permission_denied_error():
            raise AttributeError("tag 'permission_denied_error' not set")
        super(FileErrorResult, self)._process_custom_annotations(annotation_type, field_path, processor)
FileErrorResult_validator = bv.Union(FileErrorResult)
class SharedLinkMetadata(bb.Struct):
    The metadata of a shared link.
    :ivar sharing.SharedLinkMetadata.url: URL of the shared link.
    :ivar sharing.SharedLinkMetadata.id: A unique identifier for the linked
    :ivar sharing.SharedLinkMetadata.name: The linked file name (including
    :ivar sharing.SharedLinkMetadata.expires: Expiration time, if set. By
        default the link won't expire.
    :ivar sharing.SharedLinkMetadata.path_lower: The lowercased full path in the
        user's Dropbox. This always starts with a slash. This field will only be
        present only if the linked file is in the authenticated user's  dropbox.
    :ivar sharing.SharedLinkMetadata.link_permissions: The link's access
    :ivar sharing.SharedLinkMetadata.team_member_info: The team membership
        information of the link's owner.  This field will only be present  if
        the link's owner is a team member.
    :ivar sharing.SharedLinkMetadata.content_owner_team_info: The team
        information of the content's owner. This field will only be present if
        the content's owner is a team member and the content's owner team is
        different from the link's owner team.
        '_team_member_info_value',
        '_content_owner_team_info_value',
                 expires=None,
                 team_member_info=None,
                 content_owner_team_info=None):
        self._team_member_info_value = bb.NOT_SET
        self._content_owner_team_info_value = bb.NOT_SET
        if team_member_info is not None:
            self.team_member_info = team_member_info
        if content_owner_team_info is not None:
            self.content_owner_team_info = content_owner_team_info
    # Instance attribute type: LinkPermissions (validator is set below)
    link_permissions = bb.Attribute("link_permissions", user_defined=True)
    # Instance attribute type: TeamMemberInfo (validator is set below)
    team_member_info = bb.Attribute("team_member_info", nullable=True, user_defined=True)
    # Instance attribute type: users.Team (validator is set below)
    content_owner_team_info = bb.Attribute("content_owner_team_info", nullable=True)
        super(SharedLinkMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedLinkMetadata_validator = bv.StructTree(SharedLinkMetadata)
class FileLinkMetadata(SharedLinkMetadata):
    The metadata of a file shared link.
    :ivar sharing.FileLinkMetadata.client_modified: The modification time set by
        the desktop client when the file was added to Dropbox. Since this time
        is not verified (the Dropbox server stores whatever the desktop client
        sends up), this should only be used for display purposes (such as
        sorting) and not, for example, to determine if a file has changed or
        not.
    :ivar sharing.FileLinkMetadata.server_modified: The last time the file was
    :ivar sharing.FileLinkMetadata.rev: A unique identifier for the current
    :ivar sharing.FileLinkMetadata.size: The file size in bytes.
        super(FileLinkMetadata, self).__init__(url,
                                               expires,
                                               team_member_info,
                                               content_owner_team_info)
        super(FileLinkMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
FileLinkMetadata_validator = bv.Struct(FileLinkMetadata)
class FileMemberActionError(bb.Union):
    :ivar sharing.FileMemberActionError.invalid_member: Specified member was not
    :ivar sharing.FileMemberActionError.no_permission: User does not have
        permission to perform this action on this member.
    :ivar SharingFileAccessError FileMemberActionError.access_error: Specified
        file was invalid or user does not have access.
    :ivar MemberAccessLevelResult FileMemberActionError.no_explicit_access: The
        action cannot be completed because the target member does not have
        explicit access to the file. The return value is the access that the
        member has to the file from a parent folder.
    invalid_member = None
        :rtype: FileMemberActionError
    def no_explicit_access(cls, val):
        Create an instance of this class set to the ``no_explicit_access`` tag
        :param MemberAccessLevelResult val:
        return cls('no_explicit_access', val)
    def is_invalid_member(self):
        Check if the union tag is ``invalid_member``.
        return self._tag == 'invalid_member'
    def is_no_explicit_access(self):
        Check if the union tag is ``no_explicit_access``.
        return self._tag == 'no_explicit_access'
        Specified file was invalid or user does not have access.
    def get_no_explicit_access(self):
        The action cannot be completed because the target member does not have
        Only call this if :meth:`is_no_explicit_access` is true.
        :rtype: MemberAccessLevelResult
        if not self.is_no_explicit_access():
            raise AttributeError("tag 'no_explicit_access' not set")
        super(FileMemberActionError, self)._process_custom_annotations(annotation_type, field_path, processor)
FileMemberActionError_validator = bv.Union(FileMemberActionError)
class FileMemberActionIndividualResult(bb.Union):
    :ivar Optional[AccessLevel]
        sharing.FileMemberActionIndividualResult.success: Part of the response
        for both add_file_member and remove_file_member_v1 (deprecated). For
        add_file_member, indicates giving access was successful and at what
        AccessLevel. For remove_file_member_v1, indicates member was
        successfully removed from the file. If AccessLevel is given, the member
        still has access via a parent shared folder.
    :ivar FileMemberActionError FileMemberActionIndividualResult.member_error:
        User was not able to perform this action.
        :param AccessLevel val:
        :rtype: FileMemberActionIndividualResult
    def member_error(cls, val):
        Create an instance of this class set to the ``member_error`` tag with
        :param FileMemberActionError val:
        return cls('member_error', val)
    def is_member_error(self):
        Check if the union tag is ``member_error``.
        return self._tag == 'member_error'
        Part of the response for both add_file_member and remove_file_member_v1
        (deprecated). For add_file_member, indicates giving access was
        successful and at what AccessLevel. For remove_file_member_v1, indicates
        member was successfully removed from the file. If AccessLevel is given,
        the member still has access via a parent shared folder.
        :rtype: AccessLevel
    def get_member_error(self):
        Only call this if :meth:`is_member_error` is true.
        if not self.is_member_error():
            raise AttributeError("tag 'member_error' not set")
        super(FileMemberActionIndividualResult, self)._process_custom_annotations(annotation_type, field_path, processor)
FileMemberActionIndividualResult_validator = bv.Union(FileMemberActionIndividualResult)
class FileMemberActionResult(bb.Struct):
    :ivar sharing.FileMemberActionResult.member: One of specified input members.
    :ivar sharing.FileMemberActionResult.result: The outcome of the action on
    :ivar sharing.FileMemberActionResult.sckey_sha1: The SHA-1 encrypted shared
        content key.
    :ivar sharing.FileMemberActionResult.invitation_signature: The sharing
        sender-recipient invitation signatures for the input member_id. A
        member_id can be a group and thus have multiple users and multiple
        invitation signatures.
        '_sckey_sha1_value',
        '_invitation_signature_value',
                 result=None,
                 sckey_sha1=None,
                 invitation_signature=None):
        self._sckey_sha1_value = bb.NOT_SET
        self._invitation_signature_value = bb.NOT_SET
        if sckey_sha1 is not None:
            self.sckey_sha1 = sckey_sha1
        if invitation_signature is not None:
            self.invitation_signature = invitation_signature
    # Instance attribute type: FileMemberActionIndividualResult (validator is set below)
    sckey_sha1 = bb.Attribute("sckey_sha1", nullable=True)
    invitation_signature = bb.Attribute("invitation_signature", nullable=True)
        super(FileMemberActionResult, self)._process_custom_annotations(annotation_type, field_path, processor)
FileMemberActionResult_validator = bv.Struct(FileMemberActionResult)
class FileMemberRemoveActionResult(bb.Union):
    :ivar MemberAccessLevelResult FileMemberRemoveActionResult.success: Member
        was successfully removed from this file.
    :ivar FileMemberActionError FileMemberRemoveActionResult.member_error: User
        was not able to remove this member.
        :rtype: FileMemberRemoveActionResult
        Member was successfully removed from this file.
        User was not able to remove this member.
        super(FileMemberRemoveActionResult, self)._process_custom_annotations(annotation_type, field_path, processor)
FileMemberRemoveActionResult_validator = bv.Union(FileMemberRemoveActionResult)
class FilePermission(bb.Struct):
    Whether the user is allowed to take the sharing action on the file.
    :ivar sharing.FilePermission.action: The action that the user may wish to
        take on the file.
    :ivar sharing.FilePermission.allow: True if the user is allowed to take the
        action.
    :ivar sharing.FilePermission.reason: The reason why the user is denied the
        permission. Not present if the action is allowed.
        '_action_value',
        '_allow_value',
                 action=None,
                 allow=None,
                 reason=None):
        self._action_value = bb.NOT_SET
        self._allow_value = bb.NOT_SET
        if action is not None:
            self.action = action
        if allow is not None:
            self.allow = allow
    # Instance attribute type: FileAction (validator is set below)
    action = bb.Attribute("action", user_defined=True)
    allow = bb.Attribute("allow")
    # Instance attribute type: PermissionDeniedReason (validator is set below)
    reason = bb.Attribute("reason", nullable=True, user_defined=True)
        super(FilePermission, self)._process_custom_annotations(annotation_type, field_path, processor)
FilePermission_validator = bv.Struct(FilePermission)
class FolderAction(bb.Union):
    Actions that may be taken on shared folders.
    :ivar sharing.FolderAction.change_options: Change folder options, such as
        who can be invited to join the folder.
    :ivar sharing.FolderAction.disable_viewer_info: Disable viewer information
        for this folder.
    :ivar sharing.FolderAction.edit_contents: Change or edit contents of the
    :ivar sharing.FolderAction.enable_viewer_info: Enable viewer information on
    :ivar sharing.FolderAction.invite_editor: Invite a user or group to join the
        folder with read and write permission.
    :ivar sharing.FolderAction.invite_viewer: Invite a user or group to join the
        folder with read permission.
    :ivar sharing.FolderAction.invite_viewer_no_comment: Invite a user or group
        to join the folder with read permission but no comment permissions.
    :ivar sharing.FolderAction.relinquish_membership: Relinquish one's own
        membership in the folder.
    :ivar sharing.FolderAction.unmount: Unmount the folder.
    :ivar sharing.FolderAction.unshare: Stop sharing this folder.
    :ivar sharing.FolderAction.leave_a_copy: Keep a copy of the contents upon
        leaving or being kicked from the folder.
    :ivar sharing.FolderAction.share_link: Use create_link instead.
    :ivar sharing.FolderAction.create_link: Create a shared link for folder.
    :ivar sharing.FolderAction.set_access_inheritance: Set whether the folder
        inherits permissions from its parent.
    change_options = None
    unmount = None
    leave_a_copy = None
    set_access_inheritance = None
    def is_change_options(self):
        Check if the union tag is ``change_options``.
        return self._tag == 'change_options'
    def is_unmount(self):
        Check if the union tag is ``unmount``.
        return self._tag == 'unmount'
    def is_leave_a_copy(self):
        Check if the union tag is ``leave_a_copy``.
        return self._tag == 'leave_a_copy'
    def is_set_access_inheritance(self):
        Check if the union tag is ``set_access_inheritance``.
        return self._tag == 'set_access_inheritance'
        super(FolderAction, self)._process_custom_annotations(annotation_type, field_path, processor)
FolderAction_validator = bv.Union(FolderAction)
class FolderLinkMetadata(SharedLinkMetadata):
    The metadata of a folder shared link.
        super(FolderLinkMetadata, self).__init__(url,
        super(FolderLinkMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
FolderLinkMetadata_validator = bv.Struct(FolderLinkMetadata)
class FolderPermission(bb.Struct):
    Whether the user is allowed to take the action on the shared folder.
    :ivar sharing.FolderPermission.action: The action that the user may wish to
        take on the folder.
    :ivar sharing.FolderPermission.allow: True if the user is allowed to take
        the action.
    :ivar sharing.FolderPermission.reason: The reason why the user is denied the
        permission. Not present if the action is allowed, or if no reason is
    # Instance attribute type: FolderAction (validator is set below)
        super(FolderPermission, self)._process_custom_annotations(annotation_type, field_path, processor)
FolderPermission_validator = bv.Struct(FolderPermission)
class FolderPolicy(bb.Struct):
    A set of policies governing membership and privileges for a shared folder.
    :ivar sharing.FolderPolicy.member_policy: Who can be a member of this shared
        folder, as set on the folder itself. The effective policy may differ
        from this value if the team-wide policy is more restrictive. Present
        only if the folder is owned by a team.
    :ivar sharing.FolderPolicy.resolved_member_policy: Who can be a member of
        this shared folder, taking into account both the folder and the
        team-wide policy. This value may differ from that of member_policy if
        the team-wide policy is more restrictive than the folder policy. Present
    :ivar sharing.FolderPolicy.acl_update_policy: Who can add and remove members
        from this shared folder.
    :ivar sharing.FolderPolicy.shared_link_policy: Who links can be shared with.
    :ivar sharing.FolderPolicy.viewer_info_policy: Who can enable/disable viewer
        info for this shared folder.
        '_member_policy_value',
        '_resolved_member_policy_value',
        '_acl_update_policy_value',
        '_shared_link_policy_value',
        '_viewer_info_policy_value',
                 resolved_member_policy=None,
                 viewer_info_policy=None):
        self._member_policy_value = bb.NOT_SET
        self._resolved_member_policy_value = bb.NOT_SET
        self._acl_update_policy_value = bb.NOT_SET
        self._shared_link_policy_value = bb.NOT_SET
        self._viewer_info_policy_value = bb.NOT_SET
        if member_policy is not None:
            self.member_policy = member_policy
        if resolved_member_policy is not None:
            self.resolved_member_policy = resolved_member_policy
        if acl_update_policy is not None:
            self.acl_update_policy = acl_update_policy
        if shared_link_policy is not None:
            self.shared_link_policy = shared_link_policy
        if viewer_info_policy is not None:
            self.viewer_info_policy = viewer_info_policy
    # Instance attribute type: MemberPolicy (validator is set below)
    member_policy = bb.Attribute("member_policy", nullable=True, user_defined=True)
    resolved_member_policy = bb.Attribute("resolved_member_policy", nullable=True, user_defined=True)
    # Instance attribute type: AclUpdatePolicy (validator is set below)
    acl_update_policy = bb.Attribute("acl_update_policy", user_defined=True)
    # Instance attribute type: SharedLinkPolicy (validator is set below)
    shared_link_policy = bb.Attribute("shared_link_policy", user_defined=True)
    # Instance attribute type: ViewerInfoPolicy (validator is set below)
    viewer_info_policy = bb.Attribute("viewer_info_policy", nullable=True, user_defined=True)
        super(FolderPolicy, self)._process_custom_annotations(annotation_type, field_path, processor)
FolderPolicy_validator = bv.Struct(FolderPolicy)
class GetFileMetadataArg(bb.Struct):
    Arguments of
    :meth:`dropbox.dropbox_client.Dropbox.sharing_get_file_metadata`.
    :ivar sharing.GetFileMetadataArg.file: The file to query.
    :ivar sharing.GetFileMetadataArg.actions: A list of `FileAction`s
        corresponding to `FilePermission`s that should appear in the  response's
        ``SharedFileMetadata.permissions`` field describing the actions the
        authenticated user can perform on the file.
        '_actions_value',
        self._actions_value = bb.NOT_SET
        if actions is not None:
            self.actions = actions
    # Instance attribute type: list of [FileAction] (validator is set below)
    actions = bb.Attribute("actions", nullable=True)
        super(GetFileMetadataArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetFileMetadataArg_validator = bv.Struct(GetFileMetadataArg)
class GetFileMetadataBatchArg(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.sharing_get_file_metadata_batch`.
    :ivar sharing.GetFileMetadataBatchArg.files: The files to query.
    :ivar sharing.GetFileMetadataBatchArg.actions: A list of `FileAction`s
        '_files_value',
                 files=None,
        self._files_value = bb.NOT_SET
        if files is not None:
            self.files = files
    files = bb.Attribute("files")
        super(GetFileMetadataBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetFileMetadataBatchArg_validator = bv.Struct(GetFileMetadataBatchArg)
class GetFileMetadataBatchResult(bb.Struct):
    Per file results of
    :ivar sharing.GetFileMetadataBatchResult.file: This is the input file
        identifier corresponding to one of ``GetFileMetadataBatchArg.files``.
    :ivar sharing.GetFileMetadataBatchResult.result: The result for this
        particular file.
    # Instance attribute type: GetFileMetadataIndividualResult (validator is set below)
        super(GetFileMetadataBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
GetFileMetadataBatchResult_validator = bv.Struct(GetFileMetadataBatchResult)
class GetFileMetadataError(bb.Union):
    Error result for
        :rtype: GetFileMetadataError
        super(GetFileMetadataError, self)._process_custom_annotations(annotation_type, field_path, processor)
GetFileMetadataError_validator = bv.Union(GetFileMetadataError)
class GetFileMetadataIndividualResult(bb.Union):
    :ivar SharedFileMetadata GetFileMetadataIndividualResult.metadata: The
        result for this file if it was successful.
    :ivar SharingFileAccessError GetFileMetadataIndividualResult.access_error:
        :param SharedFileMetadata val:
        :rtype: GetFileMetadataIndividualResult
        The result for this file if it was successful.
        :rtype: SharedFileMetadata
        super(GetFileMetadataIndividualResult, self)._process_custom_annotations(annotation_type, field_path, processor)
GetFileMetadataIndividualResult_validator = bv.Union(GetFileMetadataIndividualResult)
class GetMetadataArgs(bb.Struct):
    :ivar sharing.GetMetadataArgs.shared_folder_id: The ID for the shared
    :ivar sharing.GetMetadataArgs.actions: A list of `FolderAction`s
        corresponding to `FolderPermission`s that should appear in the
        response's ``SharedFolderMetadata.permissions`` field describing the
        actions the  authenticated user can perform on the folder.
    # Instance attribute type: list of [FolderAction] (validator is set below)
        super(GetMetadataArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
GetMetadataArgs_validator = bv.Struct(GetMetadataArgs)
class SharedLinkError(bb.Union):
    :ivar sharing.SharedLinkError.shared_link_not_found: The shared link wasn't
    :ivar sharing.SharedLinkError.shared_link_access_denied: The caller is not
        allowed to access this shared link.
    :ivar sharing.SharedLinkError.unsupported_link_type: This type of link is
        not supported; use :meth:`dropbox.dropbox_client.Dropbox.sharing_files`
    shared_link_not_found = None
    shared_link_access_denied = None
    unsupported_link_type = None
    def is_shared_link_not_found(self):
        Check if the union tag is ``shared_link_not_found``.
        return self._tag == 'shared_link_not_found'
    def is_shared_link_access_denied(self):
        Check if the union tag is ``shared_link_access_denied``.
        return self._tag == 'shared_link_access_denied'
    def is_unsupported_link_type(self):
        Check if the union tag is ``unsupported_link_type``.
        return self._tag == 'unsupported_link_type'
        super(SharedLinkError, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedLinkError_validator = bv.Union(SharedLinkError)
class GetSharedLinkFileError(SharedLinkError):
    :ivar sharing.GetSharedLinkFileError.shared_link_is_directory: Directories
        cannot be retrieved by this endpoint.
    shared_link_is_directory = None
    def is_shared_link_is_directory(self):
        Check if the union tag is ``shared_link_is_directory``.
        return self._tag == 'shared_link_is_directory'
        super(GetSharedLinkFileError, self)._process_custom_annotations(annotation_type, field_path, processor)
GetSharedLinkFileError_validator = bv.Union(GetSharedLinkFileError)
class GetSharedLinkMetadataArg(bb.Struct):
    :ivar sharing.GetSharedLinkMetadataArg.url: URL of the shared link.
    :ivar sharing.GetSharedLinkMetadataArg.path: If the shared link is to a
        folder, this parameter can be used to retrieve the metadata for a
        specific file or sub-folder in this folder. A relative path should be
    :ivar sharing.GetSharedLinkMetadataArg.link_password: If the shared link has
        a password, this parameter can be used.
        '_link_password_value',
        self._link_password_value = bb.NOT_SET
        if link_password is not None:
            self.link_password = link_password
    link_password = bb.Attribute("link_password", nullable=True)
        super(GetSharedLinkMetadataArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetSharedLinkMetadataArg_validator = bv.Struct(GetSharedLinkMetadataArg)
class GetSharedLinksArg(bb.Struct):
    :ivar sharing.GetSharedLinksArg.path: See
        :meth:`dropbox.dropbox_client.Dropbox.sharing_get_shared_links`
        super(GetSharedLinksArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetSharedLinksArg_validator = bv.Struct(GetSharedLinksArg)
class GetSharedLinksError(bb.Union):
        :rtype: GetSharedLinksError
        super(GetSharedLinksError, self)._process_custom_annotations(annotation_type, field_path, processor)
GetSharedLinksError_validator = bv.Union(GetSharedLinksError)
class GetSharedLinksResult(bb.Struct):
    :ivar sharing.GetSharedLinksResult.links: Shared links applicable to the
        path argument.
        '_links_value',
                 links=None):
        self._links_value = bb.NOT_SET
        if links is not None:
            self.links = links
    # Instance attribute type: list of [LinkMetadata] (validator is set below)
    links = bb.Attribute("links")
        super(GetSharedLinksResult, self)._process_custom_annotations(annotation_type, field_path, processor)
GetSharedLinksResult_validator = bv.Struct(GetSharedLinksResult)
class GroupInfo(team_common.GroupSummary):
    The information about a group. Groups is a way to manage a list of users
    who need same access permission to the shared folder.
    :ivar sharing.GroupInfo.group_type: The type of group.
    :ivar sharing.GroupInfo.is_member: If the current user is a member of the
    :ivar sharing.GroupInfo.is_owner: If the current user is an owner of the
    :ivar sharing.GroupInfo.same_team: If the group is owned by the current
        user's team.
        '_group_type_value',
        '_is_member_value',
        '_is_owner_value',
        '_same_team_value',
                 group_name=None,
                 group_id=None,
                 group_management_type=None,
                 group_type=None,
                 is_member=None,
                 is_owner=None,
                 same_team=None,
                 member_count=None):
        super(GroupInfo, self).__init__(group_name,
                                        group_id,
                                        group_management_type,
                                        member_count)
        self._group_type_value = bb.NOT_SET
        self._is_member_value = bb.NOT_SET
        self._is_owner_value = bb.NOT_SET
        self._same_team_value = bb.NOT_SET
        if group_type is not None:
            self.group_type = group_type
        if is_member is not None:
            self.is_member = is_member
        if is_owner is not None:
            self.is_owner = is_owner
        if same_team is not None:
            self.same_team = same_team
    # Instance attribute type: team_common.GroupType (validator is set below)
    group_type = bb.Attribute("group_type", user_defined=True)
    is_member = bb.Attribute("is_member")
    is_owner = bb.Attribute("is_owner")
    same_team = bb.Attribute("same_team")
        super(GroupInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupInfo_validator = bv.Struct(GroupInfo)
class MembershipInfo(bb.Struct):
    The information about a member of the shared content.
    :ivar sharing.MembershipInfo.access_type: The access type for this member.
        It contains inherited access type from parent folder, and acquired
        access type from this folder.
    :ivar sharing.MembershipInfo.permissions: The permissions that requesting
        user has on this member. The set of permissions corresponds to the
        MemberActions in the request.
    :ivar sharing.MembershipInfo.initials: Never set.
    :ivar sharing.MembershipInfo.is_inherited: True if the member has access
        '_access_type_value',
        '_permissions_value',
        '_initials_value',
        '_is_inherited_value',
                 access_type=None,
                 permissions=None,
                 initials=None,
                 is_inherited=None):
        self._access_type_value = bb.NOT_SET
        self._permissions_value = bb.NOT_SET
        self._initials_value = bb.NOT_SET
        self._is_inherited_value = bb.NOT_SET
        if access_type is not None:
            self.access_type = access_type
        if permissions is not None:
            self.permissions = permissions
        if initials is not None:
            self.initials = initials
        if is_inherited is not None:
            self.is_inherited = is_inherited
    access_type = bb.Attribute("access_type", user_defined=True)
    # Instance attribute type: list of [MemberPermission] (validator is set below)
    permissions = bb.Attribute("permissions", nullable=True)
    initials = bb.Attribute("initials", nullable=True)
    is_inherited = bb.Attribute("is_inherited")
        super(MembershipInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
MembershipInfo_validator = bv.Struct(MembershipInfo)
class GroupMembershipInfo(MembershipInfo):
    The information about a group member of the shared content.
    :ivar sharing.GroupMembershipInfo.group: The information about the
        membership group.
        '_group_value',
                 group=None,
        super(GroupMembershipInfo, self).__init__(access_type,
                                                  permissions,
                                                  initials,
                                                  is_inherited)
        self._group_value = bb.NOT_SET
        if group is not None:
            self.group = group
    # Instance attribute type: GroupInfo (validator is set below)
    group = bb.Attribute("group", user_defined=True)
        super(GroupMembershipInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMembershipInfo_validator = bv.Struct(GroupMembershipInfo)
class InsufficientPlan(bb.Struct):
    :ivar sharing.InsufficientPlan.message: A message to tell the user to
        upgrade in order to support expected action.
    :ivar sharing.InsufficientPlan.upsell_url: A URL to send the user to in
        order to obtain the account type they need, e.g. upgrading. Absent if
        there is no action the user can take to upgrade.
        '_message_value',
        '_upsell_url_value',
                 message=None,
                 upsell_url=None):
        self._message_value = bb.NOT_SET
        self._upsell_url_value = bb.NOT_SET
        if message is not None:
        if upsell_url is not None:
            self.upsell_url = upsell_url
    message = bb.Attribute("message")
    upsell_url = bb.Attribute("upsell_url", nullable=True)
        super(InsufficientPlan, self)._process_custom_annotations(annotation_type, field_path, processor)
InsufficientPlan_validator = bv.Struct(InsufficientPlan)
class InsufficientQuotaAmounts(bb.Struct):
    :ivar sharing.InsufficientQuotaAmounts.space_needed: The amount of space
        needed to add the item (the size of the item).
    :ivar sharing.InsufficientQuotaAmounts.space_shortage: The amount of extra
        space needed to add the item.
    :ivar sharing.InsufficientQuotaAmounts.space_left: The amount of space left
        in the user's Dropbox, less than space_needed.
        '_space_needed_value',
        '_space_shortage_value',
        '_space_left_value',
                 space_needed=None,
                 space_shortage=None,
                 space_left=None):
        self._space_needed_value = bb.NOT_SET
        self._space_shortage_value = bb.NOT_SET
        self._space_left_value = bb.NOT_SET
        if space_needed is not None:
            self.space_needed = space_needed
        if space_shortage is not None:
            self.space_shortage = space_shortage
        if space_left is not None:
            self.space_left = space_left
    space_needed = bb.Attribute("space_needed")
    space_shortage = bb.Attribute("space_shortage")
    space_left = bb.Attribute("space_left")
        super(InsufficientQuotaAmounts, self)._process_custom_annotations(annotation_type, field_path, processor)
InsufficientQuotaAmounts_validator = bv.Struct(InsufficientQuotaAmounts)
class InviteeInfo(bb.Union):
    Information about the recipient of a shared content invitation.
    :ivar str sharing.InviteeInfo.email: Email address of invited user.
    def email(cls, val):
        Create an instance of this class set to the ``email`` tag with value
        :rtype: InviteeInfo
        return cls('email', val)
    def is_email(self):
        Check if the union tag is ``email``.
        return self._tag == 'email'
    def get_email(self):
        Email address of invited user.
        Only call this if :meth:`is_email` is true.
        if not self.is_email():
            raise AttributeError("tag 'email' not set")
        super(InviteeInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
InviteeInfo_validator = bv.Union(InviteeInfo)
class InviteeMembershipInfo(MembershipInfo):
    Information about an invited member of a shared content.
    :ivar sharing.InviteeMembershipInfo.invitee: Recipient of the invitation.
    :ivar sharing.InviteeMembershipInfo.user: The user this invitation is tied
        to, if available.
                 is_inherited=None,
                 user=None):
        super(InviteeMembershipInfo, self).__init__(access_type,
    # Instance attribute type: InviteeInfo (validator is set below)
    # Instance attribute type: UserInfo (validator is set below)
    user = bb.Attribute("user", nullable=True, user_defined=True)
        super(InviteeMembershipInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
InviteeMembershipInfo_validator = bv.Struct(InviteeMembershipInfo)
class JobError(bb.Union):
    Error occurred while performing an asynchronous job from
    :meth:`dropbox.dropbox_client.Dropbox.sharing_unshare_folder` or
    :meth:`dropbox.dropbox_client.Dropbox.sharing_remove_folder_member`.
    :ivar UnshareFolderError JobError.unshare_folder_error: Error occurred while
        performing :meth:`dropbox.dropbox_client.Dropbox.sharing_unshare_folder`
    :ivar RemoveFolderMemberError JobError.remove_folder_member_error: Error
        occurred while performing
        :meth:`dropbox.dropbox_client.Dropbox.sharing_remove_folder_member`
    :ivar RelinquishFolderMembershipError
        JobError.relinquish_folder_membership_error: Error occurred while
        performing
        :meth:`dropbox.dropbox_client.Dropbox.sharing_relinquish_folder_membership`
    def unshare_folder_error(cls, val):
        Create an instance of this class set to the ``unshare_folder_error`` tag
        :param UnshareFolderError val:
        :rtype: JobError
        return cls('unshare_folder_error', val)
    def remove_folder_member_error(cls, val):
        ``remove_folder_member_error`` tag with value ``val``.
        :param RemoveFolderMemberError val:
        return cls('remove_folder_member_error', val)
    def relinquish_folder_membership_error(cls, val):
        ``relinquish_folder_membership_error`` tag with value ``val``.
        :param RelinquishFolderMembershipError val:
        return cls('relinquish_folder_membership_error', val)
    def is_unshare_folder_error(self):
        Check if the union tag is ``unshare_folder_error``.
        return self._tag == 'unshare_folder_error'
    def is_remove_folder_member_error(self):
        Check if the union tag is ``remove_folder_member_error``.
        return self._tag == 'remove_folder_member_error'
    def is_relinquish_folder_membership_error(self):
        Check if the union tag is ``relinquish_folder_membership_error``.
        return self._tag == 'relinquish_folder_membership_error'
    def get_unshare_folder_error(self):
        Error occurred while performing
        :meth:`dropbox.dropbox_client.Dropbox.sharing_unshare_folder` action.
        Only call this if :meth:`is_unshare_folder_error` is true.
        :rtype: UnshareFolderError
        if not self.is_unshare_folder_error():
            raise AttributeError("tag 'unshare_folder_error' not set")
    def get_remove_folder_member_error(self):
        Only call this if :meth:`is_remove_folder_member_error` is true.
        :rtype: RemoveFolderMemberError
        if not self.is_remove_folder_member_error():
            raise AttributeError("tag 'remove_folder_member_error' not set")
    def get_relinquish_folder_membership_error(self):
        Only call this if :meth:`is_relinquish_folder_membership_error` is true.
        :rtype: RelinquishFolderMembershipError
        if not self.is_relinquish_folder_membership_error():
            raise AttributeError("tag 'relinquish_folder_membership_error' not set")
        super(JobError, self)._process_custom_annotations(annotation_type, field_path, processor)
JobError_validator = bv.Union(JobError)
class JobStatus(async_.PollResultBase):
    :ivar sharing.JobStatus.complete: The asynchronous job has finished.
    :ivar JobError JobStatus.failed: The asynchronous job returned an error.
        :param JobError val:
        :rtype: JobStatus
        The asynchronous job returned an error.
        super(JobStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
JobStatus_validator = bv.Union(JobStatus)
class LinkAccessLevel(bb.Union):
    :ivar sharing.LinkAccessLevel.viewer: Users who use the link can view and
        comment on the content.
    :ivar sharing.LinkAccessLevel.editor: Users who use the link can edit, view
        and comment on the content.
        super(LinkAccessLevel, self)._process_custom_annotations(annotation_type, field_path, processor)
LinkAccessLevel_validator = bv.Union(LinkAccessLevel)
class LinkAction(bb.Union):
    Actions that can be performed on a link.
    :ivar sharing.LinkAction.change_access_level: Change the access level of the
    :ivar sharing.LinkAction.change_audience: Change the audience of the link.
    :ivar sharing.LinkAction.remove_expiry: Remove the expiry date of the link.
    :ivar sharing.LinkAction.remove_password: Remove the password of the link.
    :ivar sharing.LinkAction.set_expiry: Create or modify the expiry date of the
    :ivar sharing.LinkAction.set_password: Create or modify the password of the
    change_access_level = None
    change_audience = None
    remove_expiry = None
    remove_password = None
    set_expiry = None
    set_password = None
    def is_change_access_level(self):
        Check if the union tag is ``change_access_level``.
        return self._tag == 'change_access_level'
    def is_change_audience(self):
        Check if the union tag is ``change_audience``.
        return self._tag == 'change_audience'
    def is_remove_expiry(self):
        Check if the union tag is ``remove_expiry``.
        return self._tag == 'remove_expiry'
    def is_remove_password(self):
        Check if the union tag is ``remove_password``.
        return self._tag == 'remove_password'
    def is_set_expiry(self):
        Check if the union tag is ``set_expiry``.
        return self._tag == 'set_expiry'
    def is_set_password(self):
        Check if the union tag is ``set_password``.
        return self._tag == 'set_password'
        super(LinkAction, self)._process_custom_annotations(annotation_type, field_path, processor)
LinkAction_validator = bv.Union(LinkAction)
class LinkAudience(bb.Union):
    :ivar sharing.LinkAudience.public: Link is accessible by anyone.
    :ivar sharing.LinkAudience.team: Link is accessible only by team members.
    :ivar sharing.LinkAudience.no_one: The link can be used by no one. The link
        merely points the user to the content, and does not grant additional
        rights to the user. Members of the content who use this link can only
        access the content with their pre-existing access rights.
    :ivar sharing.LinkAudience.password: Use `require_password` instead. A
        link-specific password is required to access the link. Login is not
    :ivar sharing.LinkAudience.members: Link is accessible only by members of
        the content.
    members = None
    def is_members(self):
        Check if the union tag is ``members``.
        return self._tag == 'members'
        super(LinkAudience, self)._process_custom_annotations(annotation_type, field_path, processor)
LinkAudience_validator = bv.Union(LinkAudience)
class VisibilityPolicyDisallowedReason(bb.Union):
    :ivar sharing.VisibilityPolicyDisallowedReason.delete_and_recreate: The user
        needs to delete and recreate the link to change the visibility policy.
    :ivar sharing.VisibilityPolicyDisallowedReason.restricted_by_shared_folder:
        The parent shared folder restricts sharing of links outside the shared
        folder. To change the visibility policy, remove the restriction from the
        parent shared folder.
    :ivar sharing.VisibilityPolicyDisallowedReason.restricted_by_team: The team
        policy prevents links being shared outside the team.
    :ivar sharing.VisibilityPolicyDisallowedReason.user_not_on_team: The user
        needs to be on a team to set this policy.
    :ivar sharing.VisibilityPolicyDisallowedReason.user_account_type: The user
        is a basic user or is on a limited team.
    :ivar sharing.VisibilityPolicyDisallowedReason.permission_denied: The user
        does not have permission.
    delete_and_recreate = None
    restricted_by_shared_folder = None
    restricted_by_team = None
    user_not_on_team = None
    user_account_type = None
    permission_denied = None
    def is_delete_and_recreate(self):
        Check if the union tag is ``delete_and_recreate``.
        return self._tag == 'delete_and_recreate'
    def is_restricted_by_shared_folder(self):
        Check if the union tag is ``restricted_by_shared_folder``.
        return self._tag == 'restricted_by_shared_folder'
    def is_restricted_by_team(self):
        Check if the union tag is ``restricted_by_team``.
        return self._tag == 'restricted_by_team'
    def is_user_not_on_team(self):
        Check if the union tag is ``user_not_on_team``.
        return self._tag == 'user_not_on_team'
    def is_user_account_type(self):
        Check if the union tag is ``user_account_type``.
        return self._tag == 'user_account_type'
    def is_permission_denied(self):
        Check if the union tag is ``permission_denied``.
        return self._tag == 'permission_denied'
        super(VisibilityPolicyDisallowedReason, self)._process_custom_annotations(annotation_type, field_path, processor)
VisibilityPolicyDisallowedReason_validator = bv.Union(VisibilityPolicyDisallowedReason)
class LinkAudienceDisallowedReason(VisibilityPolicyDisallowedReason):
    check documentation for VisibilityPolicyDisallowedReason.
        super(LinkAudienceDisallowedReason, self)._process_custom_annotations(annotation_type, field_path, processor)
LinkAudienceDisallowedReason_validator = bv.Union(LinkAudienceDisallowedReason)
class LinkAudienceOption(bb.Struct):
    :ivar sharing.LinkAudienceOption.audience: Specifies who can access the
    :ivar sharing.LinkAudienceOption.allowed: Whether the user calling this API
        can select this audience option.
    :ivar sharing.LinkAudienceOption.disallowed_reason: If ``allowed`` is
        ``False``, this will provide the reason that the user is not permitted
        to set the visibility to this policy.
        '_allowed_value',
        '_disallowed_reason_value',
                 audience=None,
                 allowed=None,
                 disallowed_reason=None):
        self._allowed_value = bb.NOT_SET
        self._disallowed_reason_value = bb.NOT_SET
        if allowed is not None:
            self.allowed = allowed
        if disallowed_reason is not None:
            self.disallowed_reason = disallowed_reason
    allowed = bb.Attribute("allowed")
    # Instance attribute type: LinkAudienceDisallowedReason (validator is set below)
    disallowed_reason = bb.Attribute("disallowed_reason", nullable=True, user_defined=True)
        super(LinkAudienceOption, self)._process_custom_annotations(annotation_type, field_path, processor)
LinkAudienceOption_validator = bv.Struct(LinkAudienceOption)
class LinkExpiry(bb.Union):
    :ivar sharing.LinkExpiry.remove_expiry: Remove the currently set expiry for
        the link.
    :ivar datetime.datetime sharing.LinkExpiry.set_expiry: Set a new expiry or
        change an existing expiry.
    def set_expiry(cls, val):
        Create an instance of this class set to the ``set_expiry`` tag with
        :param datetime.datetime val:
        :rtype: LinkExpiry
        return cls('set_expiry', val)
    def get_set_expiry(self):
        Set a new expiry or change an existing expiry.
        Only call this if :meth:`is_set_expiry` is true.
        :rtype: datetime.datetime
        if not self.is_set_expiry():
            raise AttributeError("tag 'set_expiry' not set")
        super(LinkExpiry, self)._process_custom_annotations(annotation_type, field_path, processor)
LinkExpiry_validator = bv.Union(LinkExpiry)
class LinkPassword(bb.Union):
    :ivar sharing.LinkPassword.remove_password: Remove the currently set
        password for the link.
    :ivar str sharing.LinkPassword.set_password: Set a new password or change an
        existing password.
    def set_password(cls, val):
        Create an instance of this class set to the ``set_password`` tag with
        :rtype: LinkPassword
        return cls('set_password', val)
    def get_set_password(self):
        Set a new password or change an existing password.
        Only call this if :meth:`is_set_password` is true.
        if not self.is_set_password():
            raise AttributeError("tag 'set_password' not set")
        super(LinkPassword, self)._process_custom_annotations(annotation_type, field_path, processor)
LinkPassword_validator = bv.Union(LinkPassword)
class LinkPermission(bb.Struct):
    Permissions for actions that can be performed on a link.
    # Instance attribute type: LinkAction (validator is set below)
        super(LinkPermission, self)._process_custom_annotations(annotation_type, field_path, processor)
LinkPermission_validator = bv.Struct(LinkPermission)
class LinkPermissions(bb.Struct):
    :ivar sharing.LinkPermissions.resolved_visibility: The current visibility of
        the link after considering the shared links policies of the the team (in
        case the link's owner is part of a team) and the shared folder (in case
        the linked file is part of a shared folder). This field is shown only if
        the caller has access to this info (the link's owner always has access
        to this data). For some links, an effective_audience value is returned
    :ivar sharing.LinkPermissions.requested_visibility: The shared link's
        requested visibility. This can be overridden by the team and shared
        folder policies. The final visibility, after considering these policies,
        can be found in ``resolved_visibility``. This is shown only if the
        caller is the link's owner and resolved_visibility is returned instead
        of effective_audience.
    :ivar sharing.LinkPermissions.can_revoke: Whether the caller can revoke the
        shared link.
    :ivar sharing.LinkPermissions.revoke_failure_reason: The failure reason for
        revoking the link. This field will only be present if the ``can_revoke``
        is ``False``.
    :ivar sharing.LinkPermissions.effective_audience: The type of audience who
        can benefit from the access level specified by the `link_access_level`
        field.
    :ivar sharing.LinkPermissions.link_access_level: The access level that the
        link will grant to its users. A link can grant additional rights to a
        user beyond their current access level. For example, if a user was
        invited as a viewer to a file, and then opens a link with
        `link_access_level` set to `editor`, then they will gain editor
        privileges. The `link_access_level` is a property of the link, and does
        not depend on who is calling this API. In particular,
        `link_access_level` does not take into account the API caller's current
        permissions to the content.
    :ivar sharing.LinkPermissions.visibility_policies: A list of policies that
        the user might be able to set for the visibility.
    :ivar sharing.LinkPermissions.can_set_expiry: Whether the user can set the
        expiry settings of the link. This refers to the ability to create a new
        expiry and modify an existing expiry.
    :ivar sharing.LinkPermissions.can_remove_expiry: Whether the user can remove
        the expiry of the link.
    :ivar sharing.LinkPermissions.allow_download: Whether the link can be
        downloaded or not.
    :ivar sharing.LinkPermissions.can_allow_download: Whether the user can allow
        downloads via the link. This refers to the ability to remove a
        no-download restriction on the link.
    :ivar sharing.LinkPermissions.can_disallow_download: Whether the user can
        disallow downloads via the link. This refers to the ability to impose a
    :ivar sharing.LinkPermissions.allow_comments: Whether comments are enabled
        for the linked file. This takes the team commenting policy into account.
    :ivar sharing.LinkPermissions.team_restricts_comments: Whether the team has
        disabled commenting globally.
    :ivar sharing.LinkPermissions.audience_options: A list of link audience
        options the user might be able to set as the new audience.
    :ivar sharing.LinkPermissions.can_set_password: Whether the user can set a
    :ivar sharing.LinkPermissions.can_remove_password: Whether the user can
        remove the password of the link.
    :ivar sharing.LinkPermissions.require_password: Whether the user is required
        to provide a password to view the link.
    :ivar sharing.LinkPermissions.can_use_extended_sharing_controls: Whether the
        user can use extended sharing controls, based on their account type.
        '_resolved_visibility_value',
        '_requested_visibility_value',
        '_can_revoke_value',
        '_revoke_failure_reason_value',
        '_effective_audience_value',
        '_link_access_level_value',
        '_visibility_policies_value',
        '_can_set_expiry_value',
        '_can_remove_expiry_value',
        '_allow_download_value',
        '_can_allow_download_value',
        '_can_disallow_download_value',
        '_allow_comments_value',
        '_team_restricts_comments_value',
        '_can_set_password_value',
        '_can_remove_password_value',
        '_require_password_value',
        '_can_use_extended_sharing_controls_value',
                 can_revoke=None,
                 visibility_policies=None,
                 can_set_expiry=None,
                 can_remove_expiry=None,
                 allow_download=None,
                 can_allow_download=None,
                 can_disallow_download=None,
                 allow_comments=None,
                 team_restricts_comments=None,
                 resolved_visibility=None,
                 requested_visibility=None,
                 revoke_failure_reason=None,
                 effective_audience=None,
                 link_access_level=None,
                 can_set_password=None,
                 can_remove_password=None,
                 require_password=None,
                 can_use_extended_sharing_controls=None):
        self._resolved_visibility_value = bb.NOT_SET
        self._requested_visibility_value = bb.NOT_SET
        self._can_revoke_value = bb.NOT_SET
        self._revoke_failure_reason_value = bb.NOT_SET
        self._effective_audience_value = bb.NOT_SET
        self._link_access_level_value = bb.NOT_SET
        self._visibility_policies_value = bb.NOT_SET
        self._can_set_expiry_value = bb.NOT_SET
        self._can_remove_expiry_value = bb.NOT_SET
        self._allow_download_value = bb.NOT_SET
        self._can_allow_download_value = bb.NOT_SET
        self._can_disallow_download_value = bb.NOT_SET
        self._allow_comments_value = bb.NOT_SET
        self._team_restricts_comments_value = bb.NOT_SET
        self._can_set_password_value = bb.NOT_SET
        self._can_remove_password_value = bb.NOT_SET
        self._require_password_value = bb.NOT_SET
        self._can_use_extended_sharing_controls_value = bb.NOT_SET
        if resolved_visibility is not None:
            self.resolved_visibility = resolved_visibility
        if requested_visibility is not None:
            self.requested_visibility = requested_visibility
        if can_revoke is not None:
            self.can_revoke = can_revoke
        if revoke_failure_reason is not None:
            self.revoke_failure_reason = revoke_failure_reason
        if effective_audience is not None:
            self.effective_audience = effective_audience
        if link_access_level is not None:
            self.link_access_level = link_access_level
        if visibility_policies is not None:
            self.visibility_policies = visibility_policies
        if can_set_expiry is not None:
            self.can_set_expiry = can_set_expiry
        if can_remove_expiry is not None:
            self.can_remove_expiry = can_remove_expiry
        if allow_download is not None:
            self.allow_download = allow_download
        if can_allow_download is not None:
            self.can_allow_download = can_allow_download
        if can_disallow_download is not None:
            self.can_disallow_download = can_disallow_download
        if allow_comments is not None:
            self.allow_comments = allow_comments
        if team_restricts_comments is not None:
            self.team_restricts_comments = team_restricts_comments
        if can_set_password is not None:
            self.can_set_password = can_set_password
        if can_remove_password is not None:
            self.can_remove_password = can_remove_password
        if require_password is not None:
            self.require_password = require_password
        if can_use_extended_sharing_controls is not None:
            self.can_use_extended_sharing_controls = can_use_extended_sharing_controls
    # Instance attribute type: ResolvedVisibility (validator is set below)
    resolved_visibility = bb.Attribute("resolved_visibility", nullable=True, user_defined=True)
    # Instance attribute type: RequestedVisibility (validator is set below)
    requested_visibility = bb.Attribute("requested_visibility", nullable=True, user_defined=True)
    can_revoke = bb.Attribute("can_revoke")
    # Instance attribute type: SharedLinkAccessFailureReason (validator is set below)
    revoke_failure_reason = bb.Attribute("revoke_failure_reason", nullable=True, user_defined=True)
    effective_audience = bb.Attribute("effective_audience", nullable=True, user_defined=True)
    # Instance attribute type: LinkAccessLevel (validator is set below)
    link_access_level = bb.Attribute("link_access_level", nullable=True, user_defined=True)
    # Instance attribute type: list of [VisibilityPolicy] (validator is set below)
    visibility_policies = bb.Attribute("visibility_policies")
    can_set_expiry = bb.Attribute("can_set_expiry")
    can_remove_expiry = bb.Attribute("can_remove_expiry")
    allow_download = bb.Attribute("allow_download")
    can_allow_download = bb.Attribute("can_allow_download")
    can_disallow_download = bb.Attribute("can_disallow_download")
    allow_comments = bb.Attribute("allow_comments")
    team_restricts_comments = bb.Attribute("team_restricts_comments")
    # Instance attribute type: list of [LinkAudienceOption] (validator is set below)
    audience_options = bb.Attribute("audience_options", nullable=True)
    can_set_password = bb.Attribute("can_set_password", nullable=True)
    can_remove_password = bb.Attribute("can_remove_password", nullable=True)
    require_password = bb.Attribute("require_password", nullable=True)
    can_use_extended_sharing_controls = bb.Attribute("can_use_extended_sharing_controls", nullable=True)
        super(LinkPermissions, self)._process_custom_annotations(annotation_type, field_path, processor)
LinkPermissions_validator = bv.Struct(LinkPermissions)
class LinkSettings(bb.Struct):
    Settings that apply to a link.
    :ivar sharing.LinkSettings.access_level: The access level on the link for
        this file. Currently, it only accepts 'viewer' and 'viewer_no_comment'.
    :ivar sharing.LinkSettings.audience: The type of audience on the link for
        this file.
    :ivar sharing.LinkSettings.expiry: An expiry timestamp to set on a link.
    :ivar sharing.LinkSettings.password: The password for the link.
                 expiry=None,
    audience = bb.Attribute("audience", nullable=True, user_defined=True)
    # Instance attribute type: LinkExpiry (validator is set below)
    expiry = bb.Attribute("expiry", nullable=True, user_defined=True)
    # Instance attribute type: LinkPassword (validator is set below)
    password = bb.Attribute("password", nullable=True, user_defined=True)
        super(LinkSettings, self)._process_custom_annotations(annotation_type, field_path, processor)
LinkSettings_validator = bv.Struct(LinkSettings)
class ListFileMembersArg(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.sharing_list_file_members`.
    :ivar sharing.ListFileMembersArg.file: The file for which you want to see
    :ivar sharing.ListFileMembersArg.actions: The actions for which to return
        permissions on a member.
    :ivar sharing.ListFileMembersArg.include_inherited: Whether to include
        members who only have access from a parent shared folder.
    :ivar sharing.ListFileMembersArg.limit: Number of members to return max per
        query. Defaults to 100 if no limit is specified.
        '_include_inherited_value',
                 include_inherited=None,
        self._include_inherited_value = bb.NOT_SET
        if include_inherited is not None:
            self.include_inherited = include_inherited
    # Instance attribute type: list of [MemberAction] (validator is set below)
    include_inherited = bb.Attribute("include_inherited")
        super(ListFileMembersArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFileMembersArg_validator = bv.Struct(ListFileMembersArg)
class ListFileMembersBatchArg(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.sharing_list_file_members_batch`.
    :ivar sharing.ListFileMembersBatchArg.files: Files for which to return
    :ivar sharing.ListFileMembersBatchArg.limit: Number of members to return max
        per query. Defaults to 10 if no limit is specified.
        super(ListFileMembersBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFileMembersBatchArg_validator = bv.Struct(ListFileMembersBatchArg)
class ListFileMembersBatchResult(bb.Struct):
    Per-file result for
    :ivar sharing.ListFileMembersBatchResult.file: This is the input file
        identifier, whether an ID or a path.
    :ivar sharing.ListFileMembersBatchResult.result: The result for this
    # Instance attribute type: ListFileMembersIndividualResult (validator is set below)
        super(ListFileMembersBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFileMembersBatchResult_validator = bv.Struct(ListFileMembersBatchResult)
class ListFileMembersContinueArg(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.sharing_list_file_members_continue`.
    :ivar sharing.ListFileMembersContinueArg.cursor: The cursor returned by your
        last call to
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_file_members`,
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_file_members_continue`,
        super(ListFileMembersContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFileMembersContinueArg_validator = bv.Struct(ListFileMembersContinueArg)
class ListFileMembersContinueError(bb.Union):
    Error for
    :ivar sharing.ListFileMembersContinueError.invalid_cursor:
        ``ListFileMembersContinueArg.cursor`` is invalid.
        :rtype: ListFileMembersContinueError
        super(ListFileMembersContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFileMembersContinueError_validator = bv.Union(ListFileMembersContinueError)
class ListFileMembersCountResult(bb.Struct):
    :ivar sharing.ListFileMembersCountResult.members: A list of members on this
    :ivar sharing.ListFileMembersCountResult.member_count: The number of members
        on this file. This does not include inherited members.
        '_member_count_value',
        self._member_count_value = bb.NOT_SET
        if member_count is not None:
            self.member_count = member_count
    # Instance attribute type: SharedFileMembers (validator is set below)
    members = bb.Attribute("members", user_defined=True)
    member_count = bb.Attribute("member_count")
        super(ListFileMembersCountResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFileMembersCountResult_validator = bv.Struct(ListFileMembersCountResult)
class ListFileMembersError(bb.Union):
    Error for :meth:`dropbox.dropbox_client.Dropbox.sharing_list_file_members`.
        :rtype: ListFileMembersError
        super(ListFileMembersError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFileMembersError_validator = bv.Union(ListFileMembersError)
class ListFileMembersIndividualResult(bb.Union):
    :ivar ListFileMembersCountResult ListFileMembersIndividualResult.result: The
        results of the query for this file if it was successful.
    :ivar SharingFileAccessError ListFileMembersIndividualResult.access_error:
        The result of the query for this file if it was an error.
    def result(cls, val):
        Create an instance of this class set to the ``result`` tag with value
        :param ListFileMembersCountResult val:
        :rtype: ListFileMembersIndividualResult
        return cls('result', val)
    def is_result(self):
        Check if the union tag is ``result``.
        return self._tag == 'result'
    def get_result(self):
        The results of the query for this file if it was successful.
        Only call this if :meth:`is_result` is true.
        :rtype: ListFileMembersCountResult
        if not self.is_result():
            raise AttributeError("tag 'result' not set")
        super(ListFileMembersIndividualResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFileMembersIndividualResult_validator = bv.Union(ListFileMembersIndividualResult)
class ListFilesArg(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.sharing_list_received_files`.
    :ivar sharing.ListFilesArg.limit: Number of files to return max per query.
        Defaults to 100 if no limit is specified.
    :ivar sharing.ListFilesArg.actions: A list of `FileAction`s corresponding to
        `FilePermission`s that should appear in the  response's
        super(ListFilesArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFilesArg_validator = bv.Struct(ListFilesArg)
class ListFilesContinueArg(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.sharing_list_received_files_continue`.
    :ivar sharing.ListFilesContinueArg.cursor: Cursor in
        ``ListFilesResult.cursor``.
        super(ListFilesContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFilesContinueArg_validator = bv.Struct(ListFilesContinueArg)
class ListFilesContinueError(bb.Union):
    Error results for
    :ivar SharingUserError ListFilesContinueError.user_error: User account had a
        problem.
    :ivar sharing.ListFilesContinueError.invalid_cursor:
        ``ListFilesContinueArg.cursor`` is invalid.
        :rtype: ListFilesContinueError
        User account had a problem.
        super(ListFilesContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFilesContinueError_validator = bv.Union(ListFilesContinueError)
class ListFilesResult(bb.Struct):
    Success results for
    :ivar sharing.ListFilesResult.entries: Information about the files shared
        with current user.
    :ivar sharing.ListFilesResult.cursor: Cursor used to obtain additional
        shared files.
    # Instance attribute type: list of [SharedFileMetadata] (validator is set below)
        super(ListFilesResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFilesResult_validator = bv.Struct(ListFilesResult)
class ListFolderMembersCursorArg(bb.Struct):
    :ivar sharing.ListFolderMembersCursorArg.actions: This is a list indicating
        whether each returned member will include a boolean value
        ``MemberPermission.allow`` that describes whether the current user can
        perform the MemberAction on the member.
    :ivar sharing.ListFolderMembersCursorArg.limit: The maximum number of
        results that include members, groups and invitees to return per request.
        super(ListFolderMembersCursorArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderMembersCursorArg_validator = bv.Struct(ListFolderMembersCursorArg)
class ListFolderMembersArgs(ListFolderMembersCursorArg):
    :ivar sharing.ListFolderMembersArgs.shared_folder_id: The ID for the shared
        super(ListFolderMembersArgs, self).__init__(actions,
        super(ListFolderMembersArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderMembersArgs_validator = bv.Struct(ListFolderMembersArgs)
class ListFolderMembersContinueArg(bb.Struct):
    :ivar sharing.ListFolderMembersContinueArg.cursor: The cursor returned by
        your last call to
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_folder_members` or
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_folder_members_continue`.
        super(ListFolderMembersContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderMembersContinueArg_validator = bv.Struct(ListFolderMembersContinueArg)
class ListFolderMembersContinueError(bb.Union):
    :ivar sharing.ListFolderMembersContinueError.invalid_cursor:
        ``ListFolderMembersContinueArg.cursor`` is invalid.
        :rtype: ListFolderMembersContinueError
        super(ListFolderMembersContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFolderMembersContinueError_validator = bv.Union(ListFolderMembersContinueError)
class ListFoldersArgs(bb.Struct):
    :ivar sharing.ListFoldersArgs.limit: The maximum number of results to return
    :ivar sharing.ListFoldersArgs.actions: A list of `FolderAction`s
        super(ListFoldersArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFoldersArgs_validator = bv.Struct(ListFoldersArgs)
class ListFoldersContinueArg(bb.Struct):
    :ivar sharing.ListFoldersContinueArg.cursor: The cursor returned by the
        previous API call specified in the endpoint description.
        super(ListFoldersContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFoldersContinueArg_validator = bv.Struct(ListFoldersContinueArg)
class ListFoldersContinueError(bb.Union):
    :ivar sharing.ListFoldersContinueError.invalid_cursor:
        ``ListFoldersContinueArg.cursor`` is invalid.
        super(ListFoldersContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFoldersContinueError_validator = bv.Union(ListFoldersContinueError)
class ListFoldersResult(bb.Struct):
    Result for :meth:`dropbox.dropbox_client.Dropbox.sharing_list_folders` or
    :meth:`dropbox.dropbox_client.Dropbox.sharing_list_mountable_folders`,
    depending on which endpoint was requested. Unmounted shared folders can be
    identified by the absence of ``SharedFolderMetadata.path_lower``.
    :ivar sharing.ListFoldersResult.entries: List of all shared folders the
        authenticated user has access to.
    :ivar sharing.ListFoldersResult.cursor: Present if there are additional
        shared folders that have not been returned yet. Pass the cursor into the
        corresponding continue endpoint (either
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_folders_continue` or
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_mountable_folders_continue`)
        to list additional folders.
    # Instance attribute type: list of [SharedFolderMetadata] (validator is set below)
        super(ListFoldersResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListFoldersResult_validator = bv.Struct(ListFoldersResult)
class ListSharedLinksArg(bb.Struct):
    :ivar sharing.ListSharedLinksArg.path: See
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_shared_links`
    :ivar sharing.ListSharedLinksArg.cursor: The cursor returned by your last
        call to
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_shared_links`.
    :ivar sharing.ListSharedLinksArg.direct_only: See
        '_direct_only_value',
        self._direct_only_value = bb.NOT_SET
        if direct_only is not None:
            self.direct_only = direct_only
    direct_only = bb.Attribute("direct_only", nullable=True)
        super(ListSharedLinksArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListSharedLinksArg_validator = bv.Struct(ListSharedLinksArg)
class ListSharedLinksError(bb.Union):
    :ivar sharing.ListSharedLinksError.reset: Indicates that the cursor has been
        invalidated. Call
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_shared_links` to
        obtain a new cursor.
        :rtype: ListSharedLinksError
        super(ListSharedLinksError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListSharedLinksError_validator = bv.Union(ListSharedLinksError)
class ListSharedLinksResult(bb.Struct):
    :ivar sharing.ListSharedLinksResult.links: Shared links applicable to the
    :ivar sharing.ListSharedLinksResult.has_more: Is true if there are
        additional shared links that have not been returned yet. Pass the cursor
        into :meth:`dropbox.dropbox_client.Dropbox.sharing_list_shared_links` to
        retrieve them.
    :ivar sharing.ListSharedLinksResult.cursor: Pass the cursor into
        obtain the additional links. Cursor is returned only if no path is
        given.
                 links=None,
    # Instance attribute type: list of [SharedLinkMetadata] (validator is set below)
        super(ListSharedLinksResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListSharedLinksResult_validator = bv.Struct(ListSharedLinksResult)
class MemberAccessLevelResult(bb.Struct):
    Contains information about a member's access level to content after an
    operation.
    :ivar sharing.MemberAccessLevelResult.access_level: The member still has
        this level of access to the content through a parent folder.
    :ivar sharing.MemberAccessLevelResult.warning: A localized string with
        additional information about why the user has this access level to the
    :ivar sharing.MemberAccessLevelResult.access_details: The parent folders
        that a member has access to. The field is present if the user has access
        to the first parent folder where the member gains access.
        '_warning_value',
        '_access_details_value',
                 warning=None,
                 access_details=None):
        self._warning_value = bb.NOT_SET
        self._access_details_value = bb.NOT_SET
        if warning is not None:
            self.warning = warning
        if access_details is not None:
            self.access_details = access_details
    warning = bb.Attribute("warning", nullable=True)
    # Instance attribute type: list of [ParentFolderAccessInfo] (validator is set below)
    access_details = bb.Attribute("access_details", nullable=True)
        super(MemberAccessLevelResult, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberAccessLevelResult_validator = bv.Struct(MemberAccessLevelResult)
class MemberAction(bb.Union):
    Actions that may be taken on members of a shared folder.
    :ivar sharing.MemberAction.leave_a_copy: Allow the member to keep a copy of
        the folder when removing.
    :ivar sharing.MemberAction.make_editor: Make the member an editor of the
    :ivar sharing.MemberAction.make_owner: Make the member an owner of the
    :ivar sharing.MemberAction.make_viewer: Make the member a viewer of the
    :ivar sharing.MemberAction.make_viewer_no_comment: Make the member a viewer
        of the folder without commenting permissions.
    :ivar sharing.MemberAction.remove: Remove the member from the folder.
    make_editor = None
    make_owner = None
    make_viewer = None
    make_viewer_no_comment = None
    remove = None
    def is_make_editor(self):
        Check if the union tag is ``make_editor``.
        return self._tag == 'make_editor'
    def is_make_owner(self):
        Check if the union tag is ``make_owner``.
        return self._tag == 'make_owner'
    def is_make_viewer(self):
        Check if the union tag is ``make_viewer``.
        return self._tag == 'make_viewer'
    def is_make_viewer_no_comment(self):
        Check if the union tag is ``make_viewer_no_comment``.
        return self._tag == 'make_viewer_no_comment'
    def is_remove(self):
        Check if the union tag is ``remove``.
        return self._tag == 'remove'
        super(MemberAction, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberAction_validator = bv.Union(MemberAction)
class MemberPermission(bb.Struct):
    Whether the user is allowed to take the action on the associated member.
    :ivar sharing.MemberPermission.action: The action that the user may wish to
        take on the member.
    :ivar sharing.MemberPermission.allow: True if the user is allowed to take
    :ivar sharing.MemberPermission.reason: The reason why the user is denied the
    # Instance attribute type: MemberAction (validator is set below)
        super(MemberPermission, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberPermission_validator = bv.Struct(MemberPermission)
class MemberPolicy(bb.Union):
    Policy governing who can be a member of a shared folder. Only applicable to
    folders owned by a user on a team.
    :ivar sharing.MemberPolicy.team: Only a teammate can become a member.
    :ivar sharing.MemberPolicy.anyone: Anyone can become a member.
    anyone = None
    def is_anyone(self):
        Check if the union tag is ``anyone``.
        return self._tag == 'anyone'
        super(MemberPolicy, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberPolicy_validator = bv.Union(MemberPolicy)
class MemberSelector(bb.Union):
    Includes different ways to identify a member of a shared folder.
    :ivar str sharing.MemberSelector.dropbox_id: Dropbox account, team member,
        or group ID of member.
    :ivar str sharing.MemberSelector.email: Email address of member.
    def dropbox_id(cls, val):
        Create an instance of this class set to the ``dropbox_id`` tag with
        :rtype: MemberSelector
        return cls('dropbox_id', val)
    def is_dropbox_id(self):
        Check if the union tag is ``dropbox_id``.
        return self._tag == 'dropbox_id'
    def get_dropbox_id(self):
        Dropbox account, team member, or group ID of member.
        Only call this if :meth:`is_dropbox_id` is true.
        if not self.is_dropbox_id():
            raise AttributeError("tag 'dropbox_id' not set")
        Email address of member.
        super(MemberSelector, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberSelector_validator = bv.Union(MemberSelector)
class ModifySharedLinkSettingsArgs(bb.Struct):
    :ivar sharing.ModifySharedLinkSettingsArgs.url: URL of the shared link to
        change its settings.
    :ivar sharing.ModifySharedLinkSettingsArgs.settings: Set of settings for the
    :ivar sharing.ModifySharedLinkSettingsArgs.remove_expiration: If set to
        true, removes the expiration of the shared link.
        '_remove_expiration_value',
                 settings=None,
                 remove_expiration=None):
        self._remove_expiration_value = bb.NOT_SET
        if remove_expiration is not None:
            self.remove_expiration = remove_expiration
    settings = bb.Attribute("settings", user_defined=True)
    remove_expiration = bb.Attribute("remove_expiration")
        super(ModifySharedLinkSettingsArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
ModifySharedLinkSettingsArgs_validator = bv.Struct(ModifySharedLinkSettingsArgs)
class ModifySharedLinkSettingsError(SharedLinkError):
    :ivar SharedLinkSettingsError ModifySharedLinkSettingsError.settings_error:
    :ivar sharing.ModifySharedLinkSettingsError.email_not_verified: This user's
        email address is not verified. This functionality is only available on
        :rtype: ModifySharedLinkSettingsError
        super(ModifySharedLinkSettingsError, self)._process_custom_annotations(annotation_type, field_path, processor)
ModifySharedLinkSettingsError_validator = bv.Union(ModifySharedLinkSettingsError)
class MountFolderArg(bb.Struct):
    :ivar sharing.MountFolderArg.shared_folder_id: The ID of the shared folder
        to mount.
                 shared_folder_id=None):
        super(MountFolderArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MountFolderArg_validator = bv.Struct(MountFolderArg)
class MountFolderError(bb.Union):
    :ivar sharing.MountFolderError.inside_shared_folder: Mounting would cause a
        shared folder to be inside another, which is disallowed.
    :ivar InsufficientQuotaAmounts MountFolderError.insufficient_quota: The
        current user does not have enough space to mount the shared folder.
    :ivar sharing.MountFolderError.already_mounted: The shared folder is already
        mounted.
    :ivar sharing.MountFolderError.no_permission: The current user does not have
        permission to perform this action.
    :ivar sharing.MountFolderError.not_mountable: The shared folder is not
        mountable. One example where this can occur is when the shared folder
        belongs within a team folder in the user's Dropbox.
    inside_shared_folder = None
    already_mounted = None
    not_mountable = None
        :rtype: MountFolderError
    def insufficient_quota(cls, val):
        Create an instance of this class set to the ``insufficient_quota`` tag
        :param InsufficientQuotaAmounts val:
        return cls('insufficient_quota', val)
    def is_inside_shared_folder(self):
        Check if the union tag is ``inside_shared_folder``.
        return self._tag == 'inside_shared_folder'
    def is_already_mounted(self):
        Check if the union tag is ``already_mounted``.
        return self._tag == 'already_mounted'
    def is_not_mountable(self):
        Check if the union tag is ``not_mountable``.
        return self._tag == 'not_mountable'
    def get_insufficient_quota(self):
        The current user does not have enough space to mount the shared folder.
        Only call this if :meth:`is_insufficient_quota` is true.
        :rtype: InsufficientQuotaAmounts
        if not self.is_insufficient_quota():
            raise AttributeError("tag 'insufficient_quota' not set")
        super(MountFolderError, self)._process_custom_annotations(annotation_type, field_path, processor)
MountFolderError_validator = bv.Union(MountFolderError)
class ParentFolderAccessInfo(bb.Struct):
    Contains information about a parent folder that a member has access to.
    :ivar sharing.ParentFolderAccessInfo.folder_name: Display name for the
    :ivar sharing.ParentFolderAccessInfo.shared_folder_id: The identifier of the
    :ivar sharing.ParentFolderAccessInfo.permissions: The user's permissions for
        the parent shared folder.
    :ivar sharing.ParentFolderAccessInfo.path: The full path to the parent
        shared folder relative to the acting user's root.
        '_folder_name_value',
                 folder_name=None,
        self._folder_name_value = bb.NOT_SET
        if folder_name is not None:
            self.folder_name = folder_name
    folder_name = bb.Attribute("folder_name")
    permissions = bb.Attribute("permissions")
        super(ParentFolderAccessInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
ParentFolderAccessInfo_validator = bv.Struct(ParentFolderAccessInfo)
class PathLinkMetadata(LinkMetadata):
    Metadata for a path-based shared link.
    :ivar sharing.PathLinkMetadata.path: Path in user's Dropbox.
        super(PathLinkMetadata, self).__init__(url,
        super(PathLinkMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
PathLinkMetadata_validator = bv.Struct(PathLinkMetadata)
class PendingUploadMode(bb.Union):
    Flag to indicate pending upload default (for linking to not-yet-existing
    paths).
    :ivar sharing.PendingUploadMode.file: Assume pending uploads are files.
    :ivar sharing.PendingUploadMode.folder: Assume pending uploads are folders.
        super(PendingUploadMode, self)._process_custom_annotations(annotation_type, field_path, processor)
PendingUploadMode_validator = bv.Union(PendingUploadMode)
class PermissionDeniedReason(bb.Union):
    Possible reasons the user is denied a permission.
    :ivar sharing.PermissionDeniedReason.user_not_same_team_as_owner: User is
        not on the same team as the folder owner.
    :ivar sharing.PermissionDeniedReason.user_not_allowed_by_owner: User is
        prohibited by the owner from taking the action.
    :ivar sharing.PermissionDeniedReason.target_is_indirect_member: Target is
        indirectly a member of the folder, for example by being part of a group.
    :ivar sharing.PermissionDeniedReason.target_is_owner: Target is the owner of
    :ivar sharing.PermissionDeniedReason.target_is_self: Target is the user
        itself.
    :ivar sharing.PermissionDeniedReason.target_not_active: Target is not an
        active member of the team.
    :ivar sharing.PermissionDeniedReason.folder_is_limited_team_folder: Folder
        is team folder for a limited team.
    :ivar sharing.PermissionDeniedReason.owner_not_on_team: The content owner
        needs to be on a Dropbox team to perform this action.
    :ivar sharing.PermissionDeniedReason.permission_denied: The user does not
        have permission to perform this action on the link.
    :ivar sharing.PermissionDeniedReason.restricted_by_team: The user's team
        policy prevents performing this action on the link.
    :ivar sharing.PermissionDeniedReason.user_account_type: The user's account
        type does not support this action.
    :ivar sharing.PermissionDeniedReason.user_not_on_team: The user needs to be
        on a Dropbox team to perform this action.
    :ivar sharing.PermissionDeniedReason.folder_is_inside_shared_folder: Folder
        is inside of another shared folder.
    :ivar sharing.PermissionDeniedReason.restricted_by_parent_folder: Policy
        cannot be changed due to restrictions from parent folder.
    user_not_same_team_as_owner = None
    user_not_allowed_by_owner = None
    target_is_indirect_member = None
    target_is_owner = None
    target_is_self = None
    target_not_active = None
    folder_is_limited_team_folder = None
    owner_not_on_team = None
    folder_is_inside_shared_folder = None
    restricted_by_parent_folder = None
    def insufficient_plan(cls, val):
        Create an instance of this class set to the ``insufficient_plan`` tag
        :param InsufficientPlan val:
        :rtype: PermissionDeniedReason
        return cls('insufficient_plan', val)
    def is_user_not_same_team_as_owner(self):
        Check if the union tag is ``user_not_same_team_as_owner``.
        return self._tag == 'user_not_same_team_as_owner'
    def is_user_not_allowed_by_owner(self):
        Check if the union tag is ``user_not_allowed_by_owner``.
        return self._tag == 'user_not_allowed_by_owner'
    def is_target_is_indirect_member(self):
        Check if the union tag is ``target_is_indirect_member``.
        return self._tag == 'target_is_indirect_member'
    def is_target_is_owner(self):
        Check if the union tag is ``target_is_owner``.
        return self._tag == 'target_is_owner'
    def is_target_is_self(self):
        Check if the union tag is ``target_is_self``.
        return self._tag == 'target_is_self'
    def is_target_not_active(self):
        Check if the union tag is ``target_not_active``.
        return self._tag == 'target_not_active'
    def is_folder_is_limited_team_folder(self):
        Check if the union tag is ``folder_is_limited_team_folder``.
        return self._tag == 'folder_is_limited_team_folder'
    def is_owner_not_on_team(self):
        Check if the union tag is ``owner_not_on_team``.
        return self._tag == 'owner_not_on_team'
    def is_folder_is_inside_shared_folder(self):
        Check if the union tag is ``folder_is_inside_shared_folder``.
        return self._tag == 'folder_is_inside_shared_folder'
    def is_restricted_by_parent_folder(self):
        Check if the union tag is ``restricted_by_parent_folder``.
        return self._tag == 'restricted_by_parent_folder'
    def get_insufficient_plan(self):
        Only call this if :meth:`is_insufficient_plan` is true.
        :rtype: InsufficientPlan
        if not self.is_insufficient_plan():
            raise AttributeError("tag 'insufficient_plan' not set")
        super(PermissionDeniedReason, self)._process_custom_annotations(annotation_type, field_path, processor)
PermissionDeniedReason_validator = bv.Union(PermissionDeniedReason)
class RelinquishFileMembershipArg(bb.Struct):
    :ivar sharing.RelinquishFileMembershipArg.file: The path or id for the file.
                 file=None):
        super(RelinquishFileMembershipArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RelinquishFileMembershipArg_validator = bv.Struct(RelinquishFileMembershipArg)
class RelinquishFileMembershipError(bb.Union):
    :ivar sharing.RelinquishFileMembershipError.group_access: The current user
        has access to the shared file via a group.  You can't relinquish
        membership to a file shared via groups.
    :ivar sharing.RelinquishFileMembershipError.no_permission: The current user
        does not have permission to perform this action.
    group_access = None
        :rtype: RelinquishFileMembershipError
    def is_group_access(self):
        Check if the union tag is ``group_access``.
        return self._tag == 'group_access'
        super(RelinquishFileMembershipError, self)._process_custom_annotations(annotation_type, field_path, processor)
RelinquishFileMembershipError_validator = bv.Union(RelinquishFileMembershipError)
class RelinquishFolderMembershipArg(bb.Struct):
    :ivar sharing.RelinquishFolderMembershipArg.shared_folder_id: The ID for the
    :ivar sharing.RelinquishFolderMembershipArg.leave_a_copy: Keep a copy of the
        folder's contents upon relinquishing membership. This must be set to
        false when the folder is within a team folder or another shared folder.
        '_leave_a_copy_value',
                 leave_a_copy=None):
        self._leave_a_copy_value = bb.NOT_SET
        if leave_a_copy is not None:
            self.leave_a_copy = leave_a_copy
    leave_a_copy = bb.Attribute("leave_a_copy")
        super(RelinquishFolderMembershipArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RelinquishFolderMembershipArg_validator = bv.Struct(RelinquishFolderMembershipArg)
class RelinquishFolderMembershipError(bb.Union):
    :ivar sharing.RelinquishFolderMembershipError.folder_owner: The current user
        is the owner of the shared folder. Owners cannot relinquish membership
        to their own folders. Try unsharing or transferring ownership first.
    :ivar sharing.RelinquishFolderMembershipError.mounted: The shared folder is
        currently mounted.  Unmount the shared folder before relinquishing
        membership.
    :ivar sharing.RelinquishFolderMembershipError.group_access: The current user
        has access to the shared folder via a group.  You can't relinquish
        membership to folders shared via groups.
    :ivar sharing.RelinquishFolderMembershipError.team_folder: This action
        cannot be performed on a team shared folder.
    :ivar sharing.RelinquishFolderMembershipError.no_permission: The current
        user does not have permission to perform this action.
    :ivar sharing.RelinquishFolderMembershipError.no_explicit_access: The
        current user only has inherited access to the shared folder.  You can't
        relinquish inherited membership to folders.
    folder_owner = None
    mounted = None
    no_explicit_access = None
    def is_folder_owner(self):
        Check if the union tag is ``folder_owner``.
        return self._tag == 'folder_owner'
    def is_mounted(self):
        Check if the union tag is ``mounted``.
        return self._tag == 'mounted'
        super(RelinquishFolderMembershipError, self)._process_custom_annotations(annotation_type, field_path, processor)
RelinquishFolderMembershipError_validator = bv.Union(RelinquishFolderMembershipError)
class RemoveFileMemberArg(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.sharing_remove_file_member_2`.
    :ivar sharing.RemoveFileMemberArg.file: File from which to remove members.
    :ivar sharing.RemoveFileMemberArg.member: Member to remove from this file.
        Note that even if an email is specified, it may result in the removal of
        a user (not an invitee) if the user's main account corresponds to that
        email address.
        super(RemoveFileMemberArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RemoveFileMemberArg_validator = bv.Struct(RemoveFileMemberArg)
class RemoveFileMemberError(bb.Union):
    Errors for
    :ivar MemberAccessLevelResult RemoveFileMemberError.no_explicit_access: This
        member does not have explicit access to the file and therefore cannot be
        removed. The return value is the access that a user might have to the
        file from a parent folder.
        :rtype: RemoveFileMemberError
        This member does not have explicit access to the file and therefore
        cannot be removed. The return value is the access that a user might have
        to the file from a parent folder.
        super(RemoveFileMemberError, self)._process_custom_annotations(annotation_type, field_path, processor)
RemoveFileMemberError_validator = bv.Union(RemoveFileMemberError)
class RemoveFolderMemberArg(bb.Struct):
    :ivar sharing.RemoveFolderMemberArg.shared_folder_id: The ID for the shared
    :ivar sharing.RemoveFolderMemberArg.member: The member to remove from the
    :ivar sharing.RemoveFolderMemberArg.leave_a_copy: If true, the removed user
        will keep their copy of the folder after it's unshared, assuming it was
        mounted. Otherwise, it will be removed from their Dropbox. This must be
        set to false when removing a group, or when the folder is within a team
        super(RemoveFolderMemberArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RemoveFolderMemberArg_validator = bv.Struct(RemoveFolderMemberArg)
class RemoveFolderMemberError(bb.Union):
    :ivar sharing.RemoveFolderMemberError.folder_owner: The target user is the
        owner of the shared folder. You can't remove this user until ownership
        has been transferred to another member.
    :ivar sharing.RemoveFolderMemberError.group_access: The target user has
        access to the shared folder via a group.
    :ivar sharing.RemoveFolderMemberError.team_folder: This action cannot be
    :ivar sharing.RemoveFolderMemberError.no_permission: The current user does
        not have permission to perform this action.
    :ivar sharing.RemoveFolderMemberError.too_many_files: This shared folder has
        too many files for leaving a copy. You can still remove this user
        without leaving a copy.
        :param SharedFolderMemberError val:
        :rtype: SharedFolderMemberError
        super(RemoveFolderMemberError, self)._process_custom_annotations(annotation_type, field_path, processor)
RemoveFolderMemberError_validator = bv.Union(RemoveFolderMemberError)
class RemoveMemberJobStatus(async_.PollResultBase):
    :ivar MemberAccessLevelResult RemoveMemberJobStatus.complete: Removing the
        folder member has finished. The value is information about whether the
        member has another form of access.
        :rtype: RemoveMemberJobStatus
        Removing the folder member has finished. The value is information about
        whether the member has another form of access.
        super(RemoveMemberJobStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
RemoveMemberJobStatus_validator = bv.Union(RemoveMemberJobStatus)
class RequestedLinkAccessLevel(bb.Union):
    :ivar sharing.RequestedLinkAccessLevel.viewer: Users who use the link can
        view and comment on the content.
    :ivar sharing.RequestedLinkAccessLevel.editor: Users who use the link can
        edit, view and comment on the content. Note not all file types support
        edit links yet.
    :ivar sharing.RequestedLinkAccessLevel.max: Request for the maximum access
        level you can set the link to.
    :ivar sharing.RequestedLinkAccessLevel.default: Request for the default
        access level the user has set.
    max = None
    def is_max(self):
        Check if the union tag is ``max``.
        return self._tag == 'max'
        super(RequestedLinkAccessLevel, self)._process_custom_annotations(annotation_type, field_path, processor)
RequestedLinkAccessLevel_validator = bv.Union(RequestedLinkAccessLevel)
class RevokeSharedLinkArg(bb.Struct):
    :ivar sharing.RevokeSharedLinkArg.url: URL of the shared link.
        super(RevokeSharedLinkArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeSharedLinkArg_validator = bv.Struct(RevokeSharedLinkArg)
class RevokeSharedLinkError(SharedLinkError):
    :ivar sharing.RevokeSharedLinkError.shared_link_malformed: Shared link is
        malformed.
    shared_link_malformed = None
    def is_shared_link_malformed(self):
        Check if the union tag is ``shared_link_malformed``.
        return self._tag == 'shared_link_malformed'
        super(RevokeSharedLinkError, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeSharedLinkError_validator = bv.Union(RevokeSharedLinkError)
class SetAccessInheritanceArg(bb.Struct):
    :ivar sharing.SetAccessInheritanceArg.access_inheritance: The access
        inheritance settings for the folder.
    :ivar sharing.SetAccessInheritanceArg.shared_folder_id: The ID for the
        '_access_inheritance_value',
                 access_inheritance=None):
        self._access_inheritance_value = bb.NOT_SET
        if access_inheritance is not None:
            self.access_inheritance = access_inheritance
    # Instance attribute type: AccessInheritance (validator is set below)
    access_inheritance = bb.Attribute("access_inheritance", user_defined=True)
        super(SetAccessInheritanceArg, self)._process_custom_annotations(annotation_type, field_path, processor)
SetAccessInheritanceArg_validator = bv.Struct(SetAccessInheritanceArg)
class SetAccessInheritanceError(bb.Union):
    :ivar SharedFolderAccessError SetAccessInheritanceError.access_error: Unable
        to access shared folder.
    :ivar sharing.SetAccessInheritanceError.no_permission: The current user does
        :rtype: SetAccessInheritanceError
        super(SetAccessInheritanceError, self)._process_custom_annotations(annotation_type, field_path, processor)
SetAccessInheritanceError_validator = bv.Union(SetAccessInheritanceError)
class ShareFolderArgBase(bb.Struct):
    :ivar sharing.ShareFolderArgBase.acl_update_policy: Who can add and remove
        members of this shared folder.
    :ivar sharing.ShareFolderArgBase.force_async: Whether to force the share to
    :ivar sharing.ShareFolderArgBase.member_policy: Who can be a member of this
        shared folder. Only applicable if the current user is on a team.
    :ivar sharing.ShareFolderArgBase.path: The path or the file id to the folder
        to share. If it does not exist, then a new one is created.
    :ivar sharing.ShareFolderArgBase.shared_link_policy: The policy to apply to
        shared links created for content inside this shared folder.  The current
        user must be on a team to set this policy to
        ``SharedLinkPolicy.members``.
    :ivar sharing.ShareFolderArgBase.viewer_info_policy: Who can enable/disable
        viewer info for this shared folder.
    :ivar sharing.ShareFolderArgBase.access_inheritance: The access inheritance
        settings for the folder.
                 force_async=None,
    acl_update_policy = bb.Attribute("acl_update_policy", nullable=True, user_defined=True)
    shared_link_policy = bb.Attribute("shared_link_policy", nullable=True, user_defined=True)
        super(ShareFolderArgBase, self)._process_custom_annotations(annotation_type, field_path, processor)
ShareFolderArgBase_validator = bv.Struct(ShareFolderArgBase)
class ShareFolderArg(ShareFolderArgBase):
    :ivar sharing.ShareFolderArg.actions: A list of `FolderAction`s
    :ivar sharing.ShareFolderArg.link_settings: Settings on the link for this
        '_link_settings_value',
                 access_inheritance=None,
        super(ShareFolderArg, self).__init__(path,
        self._link_settings_value = bb.NOT_SET
        if link_settings is not None:
            self.link_settings = link_settings
    # Instance attribute type: LinkSettings (validator is set below)
    link_settings = bb.Attribute("link_settings", nullable=True, user_defined=True)
        super(ShareFolderArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ShareFolderArg_validator = bv.Struct(ShareFolderArg)
class ShareFolderErrorBase(bb.Union):
    :ivar sharing.ShareFolderErrorBase.email_unverified: This user's email
    :ivar SharePathError ShareFolderErrorBase.bad_path: ``ShareFolderArg.path``
    :ivar sharing.ShareFolderErrorBase.team_policy_disallows_member_policy: Team
        policy is more restrictive than ``ShareFolderArg.member_policy``.
    :ivar sharing.ShareFolderErrorBase.disallowed_shared_link_policy: The
        current user's account is not allowed to select the specified
        ``ShareFolderArg.shared_link_policy``.
    team_policy_disallows_member_policy = None
    disallowed_shared_link_policy = None
    def bad_path(cls, val):
        Create an instance of this class set to the ``bad_path`` tag with value
        :param SharePathError val:
        :rtype: ShareFolderErrorBase
        return cls('bad_path', val)
    def is_bad_path(self):
        Check if the union tag is ``bad_path``.
        return self._tag == 'bad_path'
    def is_team_policy_disallows_member_policy(self):
        Check if the union tag is ``team_policy_disallows_member_policy``.
        return self._tag == 'team_policy_disallows_member_policy'
    def is_disallowed_shared_link_policy(self):
        Check if the union tag is ``disallowed_shared_link_policy``.
        return self._tag == 'disallowed_shared_link_policy'
    def get_bad_path(self):
        ``ShareFolderArg.path`` is invalid.
        Only call this if :meth:`is_bad_path` is true.
        :rtype: SharePathError
        if not self.is_bad_path():
            raise AttributeError("tag 'bad_path' not set")
        super(ShareFolderErrorBase, self)._process_custom_annotations(annotation_type, field_path, processor)
ShareFolderErrorBase_validator = bv.Union(ShareFolderErrorBase)
class ShareFolderError(ShareFolderErrorBase):
    :ivar sharing.ShareFolderError.no_permission: The current user does not have
        super(ShareFolderError, self)._process_custom_annotations(annotation_type, field_path, processor)
ShareFolderError_validator = bv.Union(ShareFolderError)
class ShareFolderJobStatus(async_.PollResultBase):
    :ivar SharedFolderMetadata ShareFolderJobStatus.complete: The share job has
        finished. The value is the metadata for the folder.
        :param SharedFolderMetadata val:
        :rtype: ShareFolderJobStatus
        :param ShareFolderError val:
        The share job has finished. The value is the metadata for the folder.
        :rtype: SharedFolderMetadata
        :rtype: ShareFolderError
        super(ShareFolderJobStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
ShareFolderJobStatus_validator = bv.Union(ShareFolderJobStatus)
class ShareFolderLaunch(async_.LaunchResultBase):
        :rtype: ShareFolderLaunch
        super(ShareFolderLaunch, self)._process_custom_annotations(annotation_type, field_path, processor)
ShareFolderLaunch_validator = bv.Union(ShareFolderLaunch)
class SharePathError(bb.Union):
    :ivar sharing.SharePathError.is_file: A file is at the specified path.
    :ivar sharing.SharePathError.inside_shared_folder: We do not support sharing
        a folder inside a shared folder.
    :ivar sharing.SharePathError.contains_shared_folder: We do not support
        shared folders that contain shared folders.
    :ivar sharing.SharePathError.contains_app_folder: We do not support shared
        folders that contain app folders.
    :ivar sharing.SharePathError.contains_team_folder: We do not support shared
        folders that contain team folders.
    :ivar sharing.SharePathError.is_app_folder: We do not support sharing an app
    :ivar sharing.SharePathError.inside_app_folder: We do not support sharing a
        folder inside an app folder.
    :ivar sharing.SharePathError.is_public_folder: A public folder can't be
        shared this way. Use a public link instead.
    :ivar sharing.SharePathError.inside_public_folder: A folder inside a public
        folder can't be shared this way. Use a public link instead.
    :ivar SharedFolderMetadata SharePathError.already_shared: Folder is already
        shared. Contains metadata about the existing shared folder.
    :ivar sharing.SharePathError.invalid_path: Path is not valid.
    :ivar sharing.SharePathError.is_osx_package: We do not support sharing a Mac
        OS X package.
    :ivar sharing.SharePathError.inside_osx_package: We do not support sharing a
        folder inside a Mac OS X package.
    :ivar sharing.SharePathError.is_vault: We do not support sharing the Vault
    :ivar sharing.SharePathError.is_vault_locked: We do not support sharing a
        folder inside a locked Vault.
    :ivar sharing.SharePathError.is_family: We do not support sharing the Family
    is_file = None
    contains_shared_folder = None
    contains_app_folder = None
    contains_team_folder = None
    is_app_folder = None
    inside_app_folder = None
    is_public_folder = None
    inside_public_folder = None
    is_osx_package = None
    inside_osx_package = None
    is_vault = None
    is_vault_locked = None
    is_family = None
    def already_shared(cls, val):
        Create an instance of this class set to the ``already_shared`` tag with
        return cls('already_shared', val)
    def is_is_file(self):
        Check if the union tag is ``is_file``.
        return self._tag == 'is_file'
    def is_contains_shared_folder(self):
        Check if the union tag is ``contains_shared_folder``.
        return self._tag == 'contains_shared_folder'
    def is_contains_app_folder(self):
        Check if the union tag is ``contains_app_folder``.
        return self._tag == 'contains_app_folder'
    def is_contains_team_folder(self):
        Check if the union tag is ``contains_team_folder``.
        return self._tag == 'contains_team_folder'
    def is_is_app_folder(self):
        Check if the union tag is ``is_app_folder``.
        return self._tag == 'is_app_folder'
    def is_inside_app_folder(self):
        Check if the union tag is ``inside_app_folder``.
        return self._tag == 'inside_app_folder'
    def is_is_public_folder(self):
        Check if the union tag is ``is_public_folder``.
        return self._tag == 'is_public_folder'
    def is_inside_public_folder(self):
        Check if the union tag is ``inside_public_folder``.
        return self._tag == 'inside_public_folder'
    def is_already_shared(self):
        Check if the union tag is ``already_shared``.
        return self._tag == 'already_shared'
    def is_is_osx_package(self):
        Check if the union tag is ``is_osx_package``.
        return self._tag == 'is_osx_package'
    def is_inside_osx_package(self):
        Check if the union tag is ``inside_osx_package``.
        return self._tag == 'inside_osx_package'
    def is_is_vault(self):
        Check if the union tag is ``is_vault``.
        return self._tag == 'is_vault'
    def is_is_vault_locked(self):
        Check if the union tag is ``is_vault_locked``.
        return self._tag == 'is_vault_locked'
    def is_is_family(self):
        Check if the union tag is ``is_family``.
        return self._tag == 'is_family'
    def get_already_shared(self):
        Folder is already shared. Contains metadata about the existing shared
        Only call this if :meth:`is_already_shared` is true.
        if not self.is_already_shared():
            raise AttributeError("tag 'already_shared' not set")
        super(SharePathError, self)._process_custom_annotations(annotation_type, field_path, processor)
SharePathError_validator = bv.Union(SharePathError)
class SharedContentLinkMetadata(SharedContentLinkMetadataBase):
    Metadata of a shared link for a file or folder.
    :ivar sharing.SharedContentLinkMetadata.audience_exceptions: The content
        inside this folder with link audience different than this folder's. This
        is only returned when an endpoint that returns metadata for a single
        shared folder is called, e.g. /get_folder_metadata.
    :ivar sharing.SharedContentLinkMetadata.url: The URL of the link.
        '_audience_exceptions_value',
                 audience_exceptions=None):
        super(SharedContentLinkMetadata, self).__init__(audience_options,
        self._audience_exceptions_value = bb.NOT_SET
        if audience_exceptions is not None:
            self.audience_exceptions = audience_exceptions
    # Instance attribute type: AudienceExceptions (validator is set below)
    audience_exceptions = bb.Attribute("audience_exceptions", nullable=True, user_defined=True)
        super(SharedContentLinkMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedContentLinkMetadata_validator = bv.Struct(SharedContentLinkMetadata)
class SharedFileMembers(bb.Struct):
    Shared file user, group, and invitee membership. Used for the results of
    :meth:`dropbox.dropbox_client.Dropbox.sharing_list_file_members` and
    and used as part of the results for
    :ivar sharing.SharedFileMembers.users: The list of user members of the
        shared file.
    :ivar sharing.SharedFileMembers.groups: The list of group members of the
    :ivar sharing.SharedFileMembers.invitees: The list of invited members of a
        file, but have not logged in and claimed this.
    :ivar sharing.SharedFileMembers.cursor: Present if there are additional
        shared file members that have not been returned yet. Pass the cursor
        into
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_file_members_continue`
        to list additional members.
        '_groups_value',
                 groups=None,
        self._groups_value = bb.NOT_SET
        if groups is not None:
            self.groups = groups
    # Instance attribute type: list of [UserFileMembershipInfo] (validator is set below)
    # Instance attribute type: list of [GroupMembershipInfo] (validator is set below)
    groups = bb.Attribute("groups")
    # Instance attribute type: list of [InviteeMembershipInfo] (validator is set below)
        super(SharedFileMembers, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedFileMembers_validator = bv.Struct(SharedFileMembers)
class SharedFileMetadata(bb.Struct):
    Properties of the shared file.
    :ivar sharing.SharedFileMetadata.access_type: The current user's access
        level for this shared file.
    :ivar sharing.SharedFileMetadata.id: The ID of the file.
    :ivar sharing.SharedFileMetadata.expected_link_metadata: The expected
        metadata of the link associated for the file when it is first shared.
        Absent if the link already exists. This is for an unreleased feature so
        it may not be returned yet.
    :ivar sharing.SharedFileMetadata.link_metadata: The metadata of the link
        associated for the file. This is for an unreleased feature so it may not
        be returned yet.
    :ivar sharing.SharedFileMetadata.name: The name of this file.
    :ivar sharing.SharedFileMetadata.owner_display_names: The display names of
        the users that own the file. If the file is part of a team folder, the
        display names of the team admins are also included. Absent if the owner
        display names cannot be fetched.
    :ivar sharing.SharedFileMetadata.owner_team: The team that owns the file.
        This field is not present if the file is not owned by a team.
    :ivar sharing.SharedFileMetadata.parent_shared_folder_id: The ID of the
        parent shared folder. This field is present only if the file is
        contained within a shared folder.
    :ivar sharing.SharedFileMetadata.path_display: The cased path to be used for
        display purposes only. In rare instances the casing will not correctly
        match the user's filesystem, but this behavior will match the path
        provided in the Core API v1. Absent for unmounted files.
    :ivar sharing.SharedFileMetadata.path_lower: The lower-case full path of
        this file. Absent for unmounted files.
    :ivar sharing.SharedFileMetadata.permissions: The sharing permissions that
        requesting user has on this file. This corresponds to the entries given
        in ``GetFileMetadataBatchArg.actions`` or
        ``GetFileMetadataArg.actions``.
    :ivar sharing.SharedFileMetadata.policy: Policies governing this shared
    :ivar sharing.SharedFileMetadata.preview_url: URL for displaying a web
        preview of the shared file.
    :ivar sharing.SharedFileMetadata.time_invited: Timestamp indicating when the
        current user was invited to this shared file. If the user was not
        invited to the shared file, the timestamp will indicate when the user
        was invited to the parent shared folder. This value may be absent.
        '_expected_link_metadata_value',
        '_owner_display_names_value',
        '_owner_team_value',
        '_policy_value',
        '_time_invited_value',
                 policy=None,
                 expected_link_metadata=None,
                 link_metadata=None,
                 owner_display_names=None,
                 owner_team=None,
                 time_invited=None):
        self._expected_link_metadata_value = bb.NOT_SET
        self._owner_display_names_value = bb.NOT_SET
        self._owner_team_value = bb.NOT_SET
        self._policy_value = bb.NOT_SET
        self._time_invited_value = bb.NOT_SET
        if expected_link_metadata is not None:
            self.expected_link_metadata = expected_link_metadata
        if owner_display_names is not None:
            self.owner_display_names = owner_display_names
        if owner_team is not None:
            self.owner_team = owner_team
        if policy is not None:
            self.policy = policy
        if time_invited is not None:
            self.time_invited = time_invited
    access_type = bb.Attribute("access_type", nullable=True, user_defined=True)
    # Instance attribute type: ExpectedSharedContentLinkMetadata (validator is set below)
    expected_link_metadata = bb.Attribute("expected_link_metadata", nullable=True, user_defined=True)
    # Instance attribute type: SharedContentLinkMetadata (validator is set below)
    owner_display_names = bb.Attribute("owner_display_names", nullable=True)
    owner_team = bb.Attribute("owner_team", nullable=True, user_defined=True)
    # Instance attribute type: list of [FilePermission] (validator is set below)
    # Instance attribute type: FolderPolicy (validator is set below)
    policy = bb.Attribute("policy", user_defined=True)
    preview_url = bb.Attribute("preview_url")
    time_invited = bb.Attribute("time_invited", nullable=True)
        super(SharedFileMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedFileMetadata_validator = bv.Struct(SharedFileMetadata)
class SharedFolderAccessError(bb.Union):
    There is an error accessing the shared folder.
    :ivar sharing.SharedFolderAccessError.invalid_id: This shared folder ID is
    :ivar sharing.SharedFolderAccessError.not_a_member: The user is not a member
        of the shared folder thus cannot access it.
    :ivar sharing.SharedFolderAccessError.invalid_member: The user does not
        exist or their account is disabled.
    :ivar sharing.SharedFolderAccessError.email_unverified: Never set.
    :ivar sharing.SharedFolderAccessError.unmounted: The shared folder is
        unmounted.
    invalid_id = None
    not_a_member = None
    unmounted = None
    def is_invalid_id(self):
        Check if the union tag is ``invalid_id``.
        return self._tag == 'invalid_id'
    def is_not_a_member(self):
        Check if the union tag is ``not_a_member``.
        return self._tag == 'not_a_member'
    def is_unmounted(self):
        Check if the union tag is ``unmounted``.
        return self._tag == 'unmounted'
        super(SharedFolderAccessError, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedFolderAccessError_validator = bv.Union(SharedFolderAccessError)
class SharedFolderMemberError(bb.Union):
    :ivar sharing.SharedFolderMemberError.invalid_dropbox_id: The target
        dropbox_id is invalid.
    :ivar sharing.SharedFolderMemberError.not_a_member: The target dropbox_id is
        not a member of the shared folder.
    :ivar MemberAccessLevelResult SharedFolderMemberError.no_explicit_access:
        The target member only has inherited access to the shared folder.
    invalid_dropbox_id = None
        super(SharedFolderMemberError, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedFolderMemberError_validator = bv.Union(SharedFolderMemberError)
class SharedFolderMembers(bb.Struct):
    Shared folder user and group membership.
    :ivar sharing.SharedFolderMembers.users: The list of user members of the
    :ivar sharing.SharedFolderMembers.groups: The list of group members of the
    :ivar sharing.SharedFolderMembers.invitees: The list of invitees to the
    :ivar sharing.SharedFolderMembers.cursor: Present if there are additional
        shared folder members that have not been returned yet. Pass the cursor
        :meth:`dropbox.dropbox_client.Dropbox.sharing_list_folder_members_continue`
    # Instance attribute type: list of [UserMembershipInfo] (validator is set below)
        super(SharedFolderMembers, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedFolderMembers_validator = bv.Struct(SharedFolderMembers)
class SharedFolderMetadataBase(bb.Struct):
    Properties of the shared folder.
    :ivar sharing.SharedFolderMetadataBase.access_type: The current user's
        access level for this shared folder.
    :ivar sharing.SharedFolderMetadataBase.is_inside_team_folder: Whether this
        folder is inside of a team folder.
    :ivar sharing.SharedFolderMetadataBase.is_team_folder: Whether this folder
        is a `team folder <https://www.dropbox.com/en/help/986>`_.
    :ivar sharing.SharedFolderMetadataBase.owner_display_names: The display
        names of the users that own the folder. If the folder is part of a team
        folder, the display names of the team admins are also included. Absent
        if the owner display names cannot be fetched.
    :ivar sharing.SharedFolderMetadataBase.owner_team: The team that owns the
        folder. This field is not present if the folder is not owned by a team.
    :ivar sharing.SharedFolderMetadataBase.parent_shared_folder_id: The ID of
        the parent shared folder. This field is present only if the folder is
        contained within another shared folder.
    :ivar sharing.SharedFolderMetadataBase.path_display: The full path of this
        shared folder. Absent for unmounted folders.
    :ivar sharing.SharedFolderMetadataBase.path_lower: The lower-cased full path
        of this shared folder. Absent for unmounted folders.
    :ivar sharing.SharedFolderMetadataBase.parent_folder_name: Display name for
        the parent folder.
        '_is_inside_team_folder_value',
        '_parent_folder_name_value',
                 is_inside_team_folder=None,
                 is_team_folder=None,
                 parent_folder_name=None):
        self._is_inside_team_folder_value = bb.NOT_SET
        self._parent_folder_name_value = bb.NOT_SET
        if is_inside_team_folder is not None:
            self.is_inside_team_folder = is_inside_team_folder
        if parent_folder_name is not None:
            self.parent_folder_name = parent_folder_name
    is_inside_team_folder = bb.Attribute("is_inside_team_folder")
    is_team_folder = bb.Attribute("is_team_folder")
    parent_folder_name = bb.Attribute("parent_folder_name", nullable=True)
        super(SharedFolderMetadataBase, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedFolderMetadataBase_validator = bv.Struct(SharedFolderMetadataBase)
class SharedFolderMetadata(SharedFolderMetadataBase):
    The metadata which includes basic information about the shared folder.
    :ivar sharing.SharedFolderMetadata.link_metadata: The metadata of the shared
        content link to this shared folder. Absent if there is no link on the
        folder. This is for an unreleased feature so it may not be returned yet.
    :ivar sharing.SharedFolderMetadata.name: The name of the this shared folder.
    :ivar sharing.SharedFolderMetadata.permissions: Actions the current user may
        perform on the folder and its contents. The set of permissions
        corresponds to the FolderActions in the request.
    :ivar sharing.SharedFolderMetadata.policy: Policies governing this shared
    :ivar sharing.SharedFolderMetadata.preview_url: URL for displaying a web
        preview of the shared folder.
    :ivar sharing.SharedFolderMetadata.shared_folder_id: The ID of the shared
    :ivar sharing.SharedFolderMetadata.time_invited: Timestamp indicating when
        the current user was invited to this shared folder.
    :ivar sharing.SharedFolderMetadata.access_inheritance: Whether the folder
        inherits its members from its parent.
                 time_invited=None,
                 parent_folder_name=None,
        super(SharedFolderMetadata, self).__init__(access_type,
                                                   is_inside_team_folder,
                                                   is_team_folder,
                                                   owner_display_names,
                                                   owner_team,
                                                   parent_folder_name)
    # Instance attribute type: list of [FolderPermission] (validator is set below)
    time_invited = bb.Attribute("time_invited")
        super(SharedFolderMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedFolderMetadata_validator = bv.Struct(SharedFolderMetadata)
class SharedLinkAccessFailureReason(bb.Union):
    :ivar sharing.SharedLinkAccessFailureReason.login_required: User is not
        logged in.
    :ivar sharing.SharedLinkAccessFailureReason.email_verify_required: This
    :ivar sharing.SharedLinkAccessFailureReason.password_required: The link is
        password protected.
    :ivar sharing.SharedLinkAccessFailureReason.team_only: Access is allowed for
        team members only.
    :ivar sharing.SharedLinkAccessFailureReason.owner_only: Access is allowed
        for the shared link's owner only.
    login_required = None
    email_verify_required = None
    password_required = None
    owner_only = None
    def is_login_required(self):
        Check if the union tag is ``login_required``.
        return self._tag == 'login_required'
    def is_email_verify_required(self):
        Check if the union tag is ``email_verify_required``.
        return self._tag == 'email_verify_required'
    def is_password_required(self):
        Check if the union tag is ``password_required``.
        return self._tag == 'password_required'
    def is_owner_only(self):
        Check if the union tag is ``owner_only``.
        return self._tag == 'owner_only'
        super(SharedLinkAccessFailureReason, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedLinkAccessFailureReason_validator = bv.Union(SharedLinkAccessFailureReason)
class SharedLinkAlreadyExistsMetadata(bb.Union):
    :ivar SharedLinkMetadata SharedLinkAlreadyExistsMetadata.metadata: Metadata
        of the shared link that already exists.
        :param SharedLinkMetadata val:
        Metadata of the shared link that already exists.
        :rtype: SharedLinkMetadata
        super(SharedLinkAlreadyExistsMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedLinkAlreadyExistsMetadata_validator = bv.Union(SharedLinkAlreadyExistsMetadata)
class SharedLinkPolicy(bb.Union):
    Who can view shared links in this folder.
    :ivar sharing.SharedLinkPolicy.anyone: Links can be shared with anyone.
    :ivar sharing.SharedLinkPolicy.team: Links can be shared with anyone on the
        same team as the owner.
    :ivar sharing.SharedLinkPolicy.members: Links can only be shared among
        members of the shared folder.
        super(SharedLinkPolicy, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedLinkPolicy_validator = bv.Union(SharedLinkPolicy)
class SharedLinkSettings(bb.Struct):
    :ivar sharing.SharedLinkSettings.require_password: Boolean flag to enable or
        disable password protection.
    :ivar sharing.SharedLinkSettings.link_password: If ``require_password`` is
        true, this is needed to specify the password to access the link.
    :ivar sharing.SharedLinkSettings.expires: Expiration time of the shared
        link. By default the link won't expire.
    :ivar sharing.SharedLinkSettings.audience: The new audience who can benefit
        from the access level specified by the link's access level specified in
        the `link_access_level` field of `LinkPermissions`. This is used in
        conjunction with team policies and shared folder policies to determine
        the final effective audience type in the `effective_audience` field of
        `LinkPermissions.
    :ivar sharing.SharedLinkSettings.access: Requested access level you want the
        audience to gain from this link. Note, modifying access level for an
        existing link is not supported.
    :ivar sharing.SharedLinkSettings.requested_visibility: Use ``audience``
        instead.  The requested access for this shared link.
    :ivar sharing.SharedLinkSettings.allow_download: Boolean flag to allow or
        not download capabilities for shared links.
        '_access_value',
                 link_password=None,
                 access=None,
                 allow_download=None):
        self._access_value = bb.NOT_SET
        if access is not None:
            self.access = access
    # Instance attribute type: RequestedLinkAccessLevel (validator is set below)
    access = bb.Attribute("access", nullable=True, user_defined=True)
    allow_download = bb.Attribute("allow_download", nullable=True)
        super(SharedLinkSettings, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedLinkSettings_validator = bv.Struct(SharedLinkSettings)
class SharedLinkSettingsError(bb.Union):
    :ivar sharing.SharedLinkSettingsError.invalid_settings: The given settings
        are invalid (for example, all attributes of the
        :class:`SharedLinkSettings` are empty, the requested visibility is
        ``RequestedVisibility.password`` but the
        ``SharedLinkSettings.link_password`` is missing,
        ``SharedLinkSettings.expires`` is set to the past, etc.).
    :ivar sharing.SharedLinkSettingsError.not_authorized: User is not allowed to
        modify the settings of this link. Note that basic users can only set
        ``RequestedVisibility.public`` as the
        ``SharedLinkSettings.requested_visibility`` and cannot set
        ``SharedLinkSettings.expires``.
    invalid_settings = None
    not_authorized = None
    def is_invalid_settings(self):
        Check if the union tag is ``invalid_settings``.
        return self._tag == 'invalid_settings'
    def is_not_authorized(self):
        Check if the union tag is ``not_authorized``.
        return self._tag == 'not_authorized'
        super(SharedLinkSettingsError, self)._process_custom_annotations(annotation_type, field_path, processor)
SharedLinkSettingsError_validator = bv.Union(SharedLinkSettingsError)
class SharingFileAccessError(bb.Union):
    User could not access this file.
    :ivar sharing.SharingFileAccessError.no_permission: Current user does not
        have sufficient privileges to perform the desired action.
    :ivar sharing.SharingFileAccessError.invalid_file: File specified was not
    :ivar sharing.SharingFileAccessError.is_folder: A folder can't be shared
        this way. Use folder sharing or a shared link instead.
    :ivar sharing.SharingFileAccessError.inside_public_folder: A file inside a
        public folder can't be shared this way. Use a public link instead.
    :ivar sharing.SharingFileAccessError.inside_osx_package: A Mac OS X package
        can't be shared this way. Use a shared link instead.
    invalid_file = None
    is_folder = None
    def is_invalid_file(self):
        Check if the union tag is ``invalid_file``.
        return self._tag == 'invalid_file'
    def is_is_folder(self):
        Check if the union tag is ``is_folder``.
        return self._tag == 'is_folder'
        super(SharingFileAccessError, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingFileAccessError_validator = bv.Union(SharingFileAccessError)
class SharingUserError(bb.Union):
    User account had a problem preventing this action.
    :ivar sharing.SharingUserError.email_unverified: This user's email address
        is not verified. This functionality is only available on accounts with a
        verified email address. Users can verify their email address `here
        <https://www.dropbox.com/help/317>`_.
        super(SharingUserError, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingUserError_validator = bv.Union(SharingUserError)
class TeamMemberInfo(bb.Struct):
    Information about a team member.
    :ivar sharing.TeamMemberInfo.team_info: Information about the member's team.
    :ivar sharing.TeamMemberInfo.display_name: The display name of the user.
    :ivar sharing.TeamMemberInfo.member_id: ID of user as a member of a team.
        This field will only be present if the member is in the same team as
        current user.
        '_team_info_value',
        '_display_name_value',
        '_member_id_value',
                 team_info=None,
                 display_name=None,
                 member_id=None):
        self._team_info_value = bb.NOT_SET
        self._display_name_value = bb.NOT_SET
        self._member_id_value = bb.NOT_SET
        if team_info is not None:
            self.team_info = team_info
        if display_name is not None:
            self.display_name = display_name
        if member_id is not None:
            self.member_id = member_id
    team_info = bb.Attribute("team_info")
    display_name = bb.Attribute("display_name")
    member_id = bb.Attribute("member_id", nullable=True)
        super(TeamMemberInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamMemberInfo_validator = bv.Struct(TeamMemberInfo)
class TransferFolderArg(bb.Struct):
    :ivar sharing.TransferFolderArg.shared_folder_id: The ID for the shared
    :ivar sharing.TransferFolderArg.to_dropbox_id: A account or team member ID
        to transfer ownership to.
        '_to_dropbox_id_value',
                 to_dropbox_id=None):
        self._to_dropbox_id_value = bb.NOT_SET
        if to_dropbox_id is not None:
            self.to_dropbox_id = to_dropbox_id
    to_dropbox_id = bb.Attribute("to_dropbox_id")
        super(TransferFolderArg, self)._process_custom_annotations(annotation_type, field_path, processor)
TransferFolderArg_validator = bv.Struct(TransferFolderArg)
class TransferFolderError(bb.Union):
    :ivar sharing.TransferFolderError.invalid_dropbox_id:
        ``TransferFolderArg.to_dropbox_id`` is invalid.
    :ivar sharing.TransferFolderError.new_owner_not_a_member: The new designated
        owner is not currently a member of the shared folder.
    :ivar sharing.TransferFolderError.new_owner_unmounted: The new designated
        owner has not added the folder to their Dropbox.
    :ivar sharing.TransferFolderError.new_owner_email_unverified: The new
        designated owner's email address is not verified. This functionality is
        only available on accounts with a verified email address. Users can
        verify their email address `here <https://www.dropbox.com/help/317>`_.
    :ivar sharing.TransferFolderError.team_folder: This action cannot be
    :ivar sharing.TransferFolderError.no_permission: The current user does not
    new_owner_not_a_member = None
    new_owner_unmounted = None
    new_owner_email_unverified = None
        :rtype: TransferFolderError
    def is_new_owner_not_a_member(self):
        Check if the union tag is ``new_owner_not_a_member``.
        return self._tag == 'new_owner_not_a_member'
    def is_new_owner_unmounted(self):
        Check if the union tag is ``new_owner_unmounted``.
        return self._tag == 'new_owner_unmounted'
    def is_new_owner_email_unverified(self):
        Check if the union tag is ``new_owner_email_unverified``.
        return self._tag == 'new_owner_email_unverified'
        super(TransferFolderError, self)._process_custom_annotations(annotation_type, field_path, processor)
TransferFolderError_validator = bv.Union(TransferFolderError)
class UnmountFolderArg(bb.Struct):
    :ivar sharing.UnmountFolderArg.shared_folder_id: The ID for the shared
        super(UnmountFolderArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UnmountFolderArg_validator = bv.Struct(UnmountFolderArg)
class UnmountFolderError(bb.Union):
    :ivar sharing.UnmountFolderError.no_permission: The current user does not
    :ivar sharing.UnmountFolderError.not_unmountable: The shared folder can't be
        unmounted. One example where this can occur is when the shared folder's
        parent folder is also a shared folder that resides in the current user's
    not_unmountable = None
        :rtype: UnmountFolderError
    def is_not_unmountable(self):
        Check if the union tag is ``not_unmountable``.
        return self._tag == 'not_unmountable'
        super(UnmountFolderError, self)._process_custom_annotations(annotation_type, field_path, processor)
UnmountFolderError_validator = bv.Union(UnmountFolderError)
class UnshareFileArg(bb.Struct):
    Arguments for :meth:`dropbox.dropbox_client.Dropbox.sharing_unshare_file`.
    :ivar sharing.UnshareFileArg.file: The file to unshare.
        super(UnshareFileArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UnshareFileArg_validator = bv.Struct(UnshareFileArg)
class UnshareFileError(bb.Union):
    :meth:`dropbox.dropbox_client.Dropbox.sharing_unshare_file`.
        :rtype: UnshareFileError
        super(UnshareFileError, self)._process_custom_annotations(annotation_type, field_path, processor)
UnshareFileError_validator = bv.Union(UnshareFileError)
class UnshareFolderArg(bb.Struct):
    :ivar sharing.UnshareFolderArg.shared_folder_id: The ID for the shared
    :ivar sharing.UnshareFolderArg.leave_a_copy: If true, members of this shared
        folder will get a copy of this folder after it's unshared. Otherwise, it
        will be removed from their Dropbox. The current user, who is an owner,
        will always retain their copy.
        super(UnshareFolderArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UnshareFolderArg_validator = bv.Struct(UnshareFolderArg)
class UnshareFolderError(bb.Union):
    :ivar sharing.UnshareFolderError.team_folder: This action cannot be
    :ivar sharing.UnshareFolderError.no_permission: The current user does not
    :ivar sharing.UnshareFolderError.too_many_files: This shared folder has too
        many files to be unshared.
        super(UnshareFolderError, self)._process_custom_annotations(annotation_type, field_path, processor)
UnshareFolderError_validator = bv.Union(UnshareFolderError)
class UpdateFileMemberArgs(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.sharing_update_file_member`.
    :ivar sharing.UpdateFileMemberArgs.file: File for which we are changing a
        member's access.
    :ivar sharing.UpdateFileMemberArgs.member: The member whose access we are
        changing.
    :ivar sharing.UpdateFileMemberArgs.access_level: The new access level for
        the member.
        super(UpdateFileMemberArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
UpdateFileMemberArgs_validator = bv.Struct(UpdateFileMemberArgs)
class UpdateFolderMemberArg(bb.Struct):
    :ivar sharing.UpdateFolderMemberArg.shared_folder_id: The ID for the shared
    :ivar sharing.UpdateFolderMemberArg.member: The member of the shared folder
        to update.  Only the ``MemberSelector.dropbox_id`` may be set at this
    :ivar sharing.UpdateFolderMemberArg.access_level: The new access level for
        ``member``. ``AccessLevel.owner`` is disallowed.
        super(UpdateFolderMemberArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UpdateFolderMemberArg_validator = bv.Struct(UpdateFolderMemberArg)
class UpdateFolderMemberError(bb.Union):
    :ivar AddFolderMemberError UpdateFolderMemberError.no_explicit_access: If
        updating the access type required the member to be added to the shared
        folder and there was an error when adding the member.
    :ivar sharing.UpdateFolderMemberError.insufficient_plan: The current user's
        account doesn't support this action. An example of this is when
        downgrading a member from editor to viewer. This action can only be
        performed by users that have upgraded to a Pro or Business plan.
    :ivar sharing.UpdateFolderMemberError.no_permission: The current user does
        :rtype: UpdateFolderMemberError
        :param AddFolderMemberError val:
        If updating the access type required the member to be added to the
        shared folder and there was an error when adding the member.
        super(UpdateFolderMemberError, self)._process_custom_annotations(annotation_type, field_path, processor)
UpdateFolderMemberError_validator = bv.Union(UpdateFolderMemberError)
class UpdateFolderPolicyArg(bb.Struct):
    If any of the policies are unset, then they retain their current setting.
    :ivar sharing.UpdateFolderPolicyArg.shared_folder_id: The ID for the shared
    :ivar sharing.UpdateFolderPolicyArg.member_policy: Who can be a member of
        this shared folder. Only applicable if the current user is on a team.
    :ivar sharing.UpdateFolderPolicyArg.acl_update_policy: Who can add and
        remove members of this shared folder.
    :ivar sharing.UpdateFolderPolicyArg.viewer_info_policy: Who can
        enable/disable viewer info for this shared folder.
    :ivar sharing.UpdateFolderPolicyArg.shared_link_policy: The policy to apply
        to shared links created for content inside this shared folder. The
        current user must be on a team to set this policy to
    :ivar sharing.UpdateFolderPolicyArg.link_settings: Settings on the link for
        this folder.
    :ivar sharing.UpdateFolderPolicyArg.actions: A list of `FolderAction`s
        super(UpdateFolderPolicyArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UpdateFolderPolicyArg_validator = bv.Struct(UpdateFolderPolicyArg)
class UpdateFolderPolicyError(bb.Union):
    :ivar sharing.UpdateFolderPolicyError.not_on_team:
        ``UpdateFolderPolicyArg.member_policy`` was set even though user is not
        on a team.
    :ivar sharing.UpdateFolderPolicyError.team_policy_disallows_member_policy:
        Team policy is more restrictive than ``ShareFolderArg.member_policy``.
    :ivar sharing.UpdateFolderPolicyError.disallowed_shared_link_policy: The
        current account is not allowed to select the specified
    :ivar sharing.UpdateFolderPolicyError.no_permission: The current user does
    :ivar sharing.UpdateFolderPolicyError.team_folder: This action cannot be
    not_on_team = None
        :rtype: UpdateFolderPolicyError
    def is_not_on_team(self):
        Check if the union tag is ``not_on_team``.
        return self._tag == 'not_on_team'
        super(UpdateFolderPolicyError, self)._process_custom_annotations(annotation_type, field_path, processor)
UpdateFolderPolicyError_validator = bv.Union(UpdateFolderPolicyError)
class UserMembershipInfo(MembershipInfo):
    The information about a user member of the shared content.
    :ivar sharing.UserMembershipInfo.user: The account information for the
        membership user.
        super(UserMembershipInfo, self).__init__(access_type,
        super(UserMembershipInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
UserMembershipInfo_validator = bv.Struct(UserMembershipInfo)
class UserFileMembershipInfo(UserMembershipInfo):
    The information about a user member of the shared content with an appended
    last seen timestamp.
    :ivar sharing.UserFileMembershipInfo.time_last_seen: The UTC timestamp of
        when the user has last seen the content. Only populated if the user has
        seen the content and the caller has a plan that includes viewer history.
    :ivar sharing.UserFileMembershipInfo.platform_type: The platform on which
        the user has last seen the content, or unknown.
        '_time_last_seen_value',
        '_platform_type_value',
                 time_last_seen=None,
                 platform_type=None):
        super(UserFileMembershipInfo, self).__init__(access_type,
        self._time_last_seen_value = bb.NOT_SET
        self._platform_type_value = bb.NOT_SET
        if time_last_seen is not None:
            self.time_last_seen = time_last_seen
        if platform_type is not None:
            self.platform_type = platform_type
    time_last_seen = bb.Attribute("time_last_seen", nullable=True)
    # Instance attribute type: seen_state.PlatformType (validator is set below)
    platform_type = bb.Attribute("platform_type", nullable=True, user_defined=True)
        super(UserFileMembershipInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
UserFileMembershipInfo_validator = bv.Struct(UserFileMembershipInfo)
class UserInfo(bb.Struct):
    Basic information about a user. Use
    :meth:`dropbox.dropbox_client.Dropbox.sharing_users_account` and
    :meth:`dropbox.dropbox_client.Dropbox.sharing_users_account_batch` to obtain
    more detailed information.
    :ivar sharing.UserInfo.account_id: The account ID of the user.
    :ivar sharing.UserInfo.email: Email address of user.
    :ivar sharing.UserInfo.display_name: The display name of the user.
    :ivar sharing.UserInfo.same_team: If the user is in the same team as current
    :ivar sharing.UserInfo.team_member_id: The team member ID of the shared
        folder member. Only present if ``same_team`` is true.
        '_team_member_id_value',
                 team_member_id=None):
        self._team_member_id_value = bb.NOT_SET
        if team_member_id is not None:
            self.team_member_id = team_member_id
    account_id = bb.Attribute("account_id")
    team_member_id = bb.Attribute("team_member_id", nullable=True)
        super(UserInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
UserInfo_validator = bv.Struct(UserInfo)
class ViewerInfoPolicy(bb.Union):
    :ivar sharing.ViewerInfoPolicy.enabled: Viewer information is available on
    :ivar sharing.ViewerInfoPolicy.disabled: Viewer information is disabled on
    enabled = None
    def is_enabled(self):
        Check if the union tag is ``enabled``.
        return self._tag == 'enabled'
        super(ViewerInfoPolicy, self)._process_custom_annotations(annotation_type, field_path, processor)
ViewerInfoPolicy_validator = bv.Union(ViewerInfoPolicy)
class Visibility(bb.Union):
    Who can access a shared link. The most open visibility is ``public``. The
    default depends on many aspects, such as team and user preferences and
    shared folder settings.
    :ivar sharing.Visibility.public: Anyone who has received the link can access
        it. No login required.
    :ivar sharing.Visibility.team_only: Only members of the same team can access
        the link. Login is required.
    :ivar sharing.Visibility.password: A link-specific password is required to
        access the link. Login is not required.
    :ivar sharing.Visibility.team_and_password: Only members of the same team
        who have the link-specific password can access the link.
    :ivar sharing.Visibility.shared_folder_only: Only members of the shared
        folder containing the linked file can access the link. Login is
        super(Visibility, self)._process_custom_annotations(annotation_type, field_path, processor)
Visibility_validator = bv.Union(Visibility)
class VisibilityPolicy(bb.Struct):
    :ivar sharing.VisibilityPolicy.policy: This is the value to submit when
        saving the visibility setting.
    :ivar sharing.VisibilityPolicy.resolved_policy: This is what the effective
        policy would be, if you selected this option. The resolved policy is
        obtained after considering external effects such as shared folder
        settings and team policy. This value is guaranteed to be provided.
    :ivar sharing.VisibilityPolicy.allowed: Whether the user is permitted to set
        the visibility to this policy.
    :ivar sharing.VisibilityPolicy.disallowed_reason: If ``allowed`` is
        '_resolved_policy_value',
                 resolved_policy=None,
        self._resolved_policy_value = bb.NOT_SET
        if resolved_policy is not None:
            self.resolved_policy = resolved_policy
    # Instance attribute type: AlphaResolvedVisibility (validator is set below)
    resolved_policy = bb.Attribute("resolved_policy", user_defined=True)
    # Instance attribute type: VisibilityPolicyDisallowedReason (validator is set below)
        super(VisibilityPolicy, self)._process_custom_annotations(annotation_type, field_path, processor)
VisibilityPolicy_validator = bv.Struct(VisibilityPolicy)
DropboxId_validator = bv.String(min_length=1)
GetSharedLinkFileArg_validator = GetSharedLinkMetadataArg_validator
GetSharedLinkFileArg = GetSharedLinkMetadataArg
Id_validator = files.Id_validator
Path_validator = files.Path_validator
PathOrId_validator = bv.String(min_length=1, pattern='((/|id:).*|nspath:[0-9]+:.*)|ns:[0-9]+(/.*)?')
ReadPath_validator = files.ReadPath_validator
Rev_validator = files.Rev_validator
TeamInfo_validator = users.Team_validator
TeamInfo = users.Team
AccessInheritance._inherit_validator = bv.Void()
AccessInheritance._no_inherit_validator = bv.Void()
AccessInheritance._other_validator = bv.Void()
AccessInheritance._tagmap = {
    'inherit': AccessInheritance._inherit_validator,
    'no_inherit': AccessInheritance._no_inherit_validator,
    'other': AccessInheritance._other_validator,
AccessInheritance.inherit = AccessInheritance('inherit')
AccessInheritance.no_inherit = AccessInheritance('no_inherit')
AccessInheritance.other = AccessInheritance('other')
AccessLevel._owner_validator = bv.Void()
AccessLevel._editor_validator = bv.Void()
AccessLevel._viewer_validator = bv.Void()
AccessLevel._viewer_no_comment_validator = bv.Void()
AccessLevel._traverse_validator = bv.Void()
AccessLevel._no_access_validator = bv.Void()
AccessLevel._other_validator = bv.Void()
AccessLevel._tagmap = {
    'owner': AccessLevel._owner_validator,
    'editor': AccessLevel._editor_validator,
    'viewer': AccessLevel._viewer_validator,
    'viewer_no_comment': AccessLevel._viewer_no_comment_validator,
    'traverse': AccessLevel._traverse_validator,
    'no_access': AccessLevel._no_access_validator,
    'other': AccessLevel._other_validator,
AccessLevel.owner = AccessLevel('owner')
AccessLevel.editor = AccessLevel('editor')
AccessLevel.viewer = AccessLevel('viewer')
AccessLevel.viewer_no_comment = AccessLevel('viewer_no_comment')
AccessLevel.traverse = AccessLevel('traverse')
AccessLevel.no_access = AccessLevel('no_access')
AccessLevel.other = AccessLevel('other')
AclUpdatePolicy._owner_validator = bv.Void()
AclUpdatePolicy._editors_validator = bv.Void()
AclUpdatePolicy._other_validator = bv.Void()
AclUpdatePolicy._tagmap = {
    'owner': AclUpdatePolicy._owner_validator,
    'editors': AclUpdatePolicy._editors_validator,
    'other': AclUpdatePolicy._other_validator,
AclUpdatePolicy.owner = AclUpdatePolicy('owner')
AclUpdatePolicy.editors = AclUpdatePolicy('editors')
AclUpdatePolicy.other = AclUpdatePolicy('other')
AddFileMemberArgs.file.validator = PathOrId_validator
AddFileMemberArgs.members.validator = bv.List(MemberSelector_validator)
AddFileMemberArgs.custom_message.validator = bv.Nullable(bv.String())
AddFileMemberArgs.quiet.validator = bv.Boolean()
AddFileMemberArgs.access_level.validator = AccessLevel_validator
AddFileMemberArgs.add_message_as_comment.validator = bv.Boolean()
AddFileMemberArgs._all_field_names_ = set([
    'file',
    'access_level',
    'add_message_as_comment',
AddFileMemberArgs._all_fields_ = [
    ('file', AddFileMemberArgs.file.validator),
    ('members', AddFileMemberArgs.members.validator),
    ('custom_message', AddFileMemberArgs.custom_message.validator),
    ('quiet', AddFileMemberArgs.quiet.validator),
    ('access_level', AddFileMemberArgs.access_level.validator),
    ('add_message_as_comment', AddFileMemberArgs.add_message_as_comment.validator),
AddFileMemberError._user_error_validator = SharingUserError_validator
AddFileMemberError._access_error_validator = SharingFileAccessError_validator
AddFileMemberError._rate_limit_validator = bv.Void()
AddFileMemberError._invalid_comment_validator = bv.Void()
AddFileMemberError._other_validator = bv.Void()
AddFileMemberError._tagmap = {
    'user_error': AddFileMemberError._user_error_validator,
    'access_error': AddFileMemberError._access_error_validator,
    'rate_limit': AddFileMemberError._rate_limit_validator,
    'invalid_comment': AddFileMemberError._invalid_comment_validator,
    'other': AddFileMemberError._other_validator,
AddFileMemberError.rate_limit = AddFileMemberError('rate_limit')
AddFileMemberError.invalid_comment = AddFileMemberError('invalid_comment')
AddFileMemberError.other = AddFileMemberError('other')
AddFolderMemberArg.shared_folder_id.validator = common.SharedFolderId_validator
AddFolderMemberArg.members.validator = bv.List(AddMember_validator)
AddFolderMemberArg.quiet.validator = bv.Boolean()
AddFolderMemberArg.custom_message.validator = bv.Nullable(bv.String(min_length=1))
AddFolderMemberArg._all_field_names_ = set([
AddFolderMemberArg._all_fields_ = [
    ('shared_folder_id', AddFolderMemberArg.shared_folder_id.validator),
    ('members', AddFolderMemberArg.members.validator),
    ('quiet', AddFolderMemberArg.quiet.validator),
    ('custom_message', AddFolderMemberArg.custom_message.validator),
AddFolderMemberError._access_error_validator = SharedFolderAccessError_validator
AddFolderMemberError._email_unverified_validator = bv.Void()
AddFolderMemberError._banned_member_validator = bv.Void()
AddFolderMemberError._bad_member_validator = AddMemberSelectorError_validator
AddFolderMemberError._cant_share_outside_team_validator = bv.Void()
AddFolderMemberError._too_many_members_validator = bv.UInt64()
AddFolderMemberError._too_many_pending_invites_validator = bv.UInt64()
AddFolderMemberError._rate_limit_validator = bv.Void()
AddFolderMemberError._too_many_invitees_validator = bv.Void()
AddFolderMemberError._insufficient_plan_validator = bv.Void()
AddFolderMemberError._team_folder_validator = bv.Void()
AddFolderMemberError._no_permission_validator = bv.Void()
AddFolderMemberError._invalid_shared_folder_validator = bv.Void()
AddFolderMemberError._other_validator = bv.Void()
AddFolderMemberError._tagmap = {
    'access_error': AddFolderMemberError._access_error_validator,
    'email_unverified': AddFolderMemberError._email_unverified_validator,
    'banned_member': AddFolderMemberError._banned_member_validator,
    'bad_member': AddFolderMemberError._bad_member_validator,
    'cant_share_outside_team': AddFolderMemberError._cant_share_outside_team_validator,
    'too_many_members': AddFolderMemberError._too_many_members_validator,
    'too_many_pending_invites': AddFolderMemberError._too_many_pending_invites_validator,
    'rate_limit': AddFolderMemberError._rate_limit_validator,
    'too_many_invitees': AddFolderMemberError._too_many_invitees_validator,
    'insufficient_plan': AddFolderMemberError._insufficient_plan_validator,
    'team_folder': AddFolderMemberError._team_folder_validator,
    'no_permission': AddFolderMemberError._no_permission_validator,
    'invalid_shared_folder': AddFolderMemberError._invalid_shared_folder_validator,
    'other': AddFolderMemberError._other_validator,
AddFolderMemberError.email_unverified = AddFolderMemberError('email_unverified')
AddFolderMemberError.banned_member = AddFolderMemberError('banned_member')
AddFolderMemberError.cant_share_outside_team = AddFolderMemberError('cant_share_outside_team')
AddFolderMemberError.rate_limit = AddFolderMemberError('rate_limit')
AddFolderMemberError.too_many_invitees = AddFolderMemberError('too_many_invitees')
AddFolderMemberError.insufficient_plan = AddFolderMemberError('insufficient_plan')
AddFolderMemberError.team_folder = AddFolderMemberError('team_folder')
AddFolderMemberError.no_permission = AddFolderMemberError('no_permission')
AddFolderMemberError.invalid_shared_folder = AddFolderMemberError('invalid_shared_folder')
AddFolderMemberError.other = AddFolderMemberError('other')
AddMember.member.validator = MemberSelector_validator
AddMember.access_level.validator = AccessLevel_validator
    ('access_level', AddMember.access_level.validator),
AddMemberSelectorError._automatic_group_validator = bv.Void()
AddMemberSelectorError._invalid_dropbox_id_validator = DropboxId_validator
AddMemberSelectorError._invalid_email_validator = common.EmailAddress_validator
AddMemberSelectorError._unverified_dropbox_id_validator = DropboxId_validator
AddMemberSelectorError._group_deleted_validator = bv.Void()
AddMemberSelectorError._group_not_on_team_validator = bv.Void()
AddMemberSelectorError._other_validator = bv.Void()
AddMemberSelectorError._tagmap = {
    'automatic_group': AddMemberSelectorError._automatic_group_validator,
    'invalid_dropbox_id': AddMemberSelectorError._invalid_dropbox_id_validator,
    'invalid_email': AddMemberSelectorError._invalid_email_validator,
    'unverified_dropbox_id': AddMemberSelectorError._unverified_dropbox_id_validator,
    'group_deleted': AddMemberSelectorError._group_deleted_validator,
    'group_not_on_team': AddMemberSelectorError._group_not_on_team_validator,
    'other': AddMemberSelectorError._other_validator,
AddMemberSelectorError.automatic_group = AddMemberSelectorError('automatic_group')
AddMemberSelectorError.group_deleted = AddMemberSelectorError('group_deleted')
AddMemberSelectorError.group_not_on_team = AddMemberSelectorError('group_not_on_team')
AddMemberSelectorError.other = AddMemberSelectorError('other')
RequestedVisibility._public_validator = bv.Void()
RequestedVisibility._team_only_validator = bv.Void()
RequestedVisibility._password_validator = bv.Void()
RequestedVisibility._tagmap = {
    'public': RequestedVisibility._public_validator,
    'team_only': RequestedVisibility._team_only_validator,
    'password': RequestedVisibility._password_validator,
RequestedVisibility.public = RequestedVisibility('public')
RequestedVisibility.team_only = RequestedVisibility('team_only')
RequestedVisibility.password = RequestedVisibility('password')
ResolvedVisibility._team_and_password_validator = bv.Void()
ResolvedVisibility._shared_folder_only_validator = bv.Void()
ResolvedVisibility._no_one_validator = bv.Void()
ResolvedVisibility._only_you_validator = bv.Void()
ResolvedVisibility._other_validator = bv.Void()
ResolvedVisibility._tagmap = {
    'team_and_password': ResolvedVisibility._team_and_password_validator,
    'shared_folder_only': ResolvedVisibility._shared_folder_only_validator,
    'no_one': ResolvedVisibility._no_one_validator,
    'only_you': ResolvedVisibility._only_you_validator,
    'other': ResolvedVisibility._other_validator,
ResolvedVisibility._tagmap.update(RequestedVisibility._tagmap)
ResolvedVisibility.team_and_password = ResolvedVisibility('team_and_password')
ResolvedVisibility.shared_folder_only = ResolvedVisibility('shared_folder_only')
ResolvedVisibility.no_one = ResolvedVisibility('no_one')
ResolvedVisibility.only_you = ResolvedVisibility('only_you')
ResolvedVisibility.other = ResolvedVisibility('other')
AlphaResolvedVisibility._tagmap = {
AlphaResolvedVisibility._tagmap.update(ResolvedVisibility._tagmap)
AudienceExceptionContentInfo.name.validator = bv.String()
AudienceExceptionContentInfo._all_field_names_ = set(['name'])
AudienceExceptionContentInfo._all_fields_ = [('name', AudienceExceptionContentInfo.name.validator)]
AudienceExceptions.count.validator = bv.UInt32()
AudienceExceptions.exceptions.validator = bv.List(AudienceExceptionContentInfo_validator)
AudienceExceptions._all_field_names_ = set([
    'exceptions',
AudienceExceptions._all_fields_ = [
    ('count', AudienceExceptions.count.validator),
    ('exceptions', AudienceExceptions.exceptions.validator),
AudienceRestrictingSharedFolder.shared_folder_id.validator = common.SharedFolderId_validator
AudienceRestrictingSharedFolder.name.validator = bv.String()
AudienceRestrictingSharedFolder.audience.validator = LinkAudience_validator
AudienceRestrictingSharedFolder._all_field_names_ = set([
    'audience',
AudienceRestrictingSharedFolder._all_fields_ = [
    ('shared_folder_id', AudienceRestrictingSharedFolder.shared_folder_id.validator),
    ('name', AudienceRestrictingSharedFolder.name.validator),
    ('audience', AudienceRestrictingSharedFolder.audience.validator),
LinkMetadata.url.validator = bv.String()
LinkMetadata.visibility.validator = Visibility_validator
LinkMetadata.expires.validator = bv.Nullable(common.DropboxTimestamp_validator)
LinkMetadata._field_names_ = set([
    'visibility',
LinkMetadata._all_field_names_ = LinkMetadata._field_names_
LinkMetadata._fields_ = [
    ('url', LinkMetadata.url.validator),
    ('visibility', LinkMetadata.visibility.validator),
    ('expires', LinkMetadata.expires.validator),
LinkMetadata._all_fields_ = LinkMetadata._fields_
LinkMetadata._tag_to_subtype_ = {
    ('path',): PathLinkMetadata_validator,
    ('collection',): CollectionLinkMetadata_validator,
LinkMetadata._pytype_to_tag_and_subtype_ = {
    PathLinkMetadata: (('path',), PathLinkMetadata_validator),
    CollectionLinkMetadata: (('collection',), CollectionLinkMetadata_validator),
LinkMetadata._is_catch_all_ = True
CollectionLinkMetadata._field_names_ = set([])
CollectionLinkMetadata._all_field_names_ = LinkMetadata._all_field_names_.union(CollectionLinkMetadata._field_names_)
CollectionLinkMetadata._fields_ = []
CollectionLinkMetadata._all_fields_ = LinkMetadata._all_fields_ + CollectionLinkMetadata._fields_
CreateSharedLinkArg.path.validator = bv.String()
CreateSharedLinkArg.short_url.validator = bv.Boolean()
CreateSharedLinkArg.pending_upload.validator = bv.Nullable(PendingUploadMode_validator)
CreateSharedLinkArg._all_field_names_ = set([
    'short_url',
    'pending_upload',
CreateSharedLinkArg._all_fields_ = [
    ('path', CreateSharedLinkArg.path.validator),
    ('short_url', CreateSharedLinkArg.short_url.validator),
    ('pending_upload', CreateSharedLinkArg.pending_upload.validator),
CreateSharedLinkError._path_validator = files.LookupError_validator
CreateSharedLinkError._other_validator = bv.Void()
CreateSharedLinkError._tagmap = {
    'path': CreateSharedLinkError._path_validator,
    'other': CreateSharedLinkError._other_validator,
CreateSharedLinkError.other = CreateSharedLinkError('other')
CreateSharedLinkWithSettingsArg.path.validator = ReadPath_validator
CreateSharedLinkWithSettingsArg.settings.validator = bv.Nullable(SharedLinkSettings_validator)
CreateSharedLinkWithSettingsArg._all_field_names_ = set([
    'settings',
CreateSharedLinkWithSettingsArg._all_fields_ = [
    ('path', CreateSharedLinkWithSettingsArg.path.validator),
    ('settings', CreateSharedLinkWithSettingsArg.settings.validator),
CreateSharedLinkWithSettingsError._path_validator = files.LookupError_validator
CreateSharedLinkWithSettingsError._email_not_verified_validator = bv.Void()
CreateSharedLinkWithSettingsError._shared_link_already_exists_validator = bv.Nullable(SharedLinkAlreadyExistsMetadata_validator)
CreateSharedLinkWithSettingsError._settings_error_validator = SharedLinkSettingsError_validator
CreateSharedLinkWithSettingsError._access_denied_validator = bv.Void()
CreateSharedLinkWithSettingsError._tagmap = {
    'path': CreateSharedLinkWithSettingsError._path_validator,
    'email_not_verified': CreateSharedLinkWithSettingsError._email_not_verified_validator,
    'shared_link_already_exists': CreateSharedLinkWithSettingsError._shared_link_already_exists_validator,
    'settings_error': CreateSharedLinkWithSettingsError._settings_error_validator,
    'access_denied': CreateSharedLinkWithSettingsError._access_denied_validator,
CreateSharedLinkWithSettingsError.email_not_verified = CreateSharedLinkWithSettingsError('email_not_verified')
CreateSharedLinkWithSettingsError.access_denied = CreateSharedLinkWithSettingsError('access_denied')
SharedContentLinkMetadataBase.access_level.validator = bv.Nullable(AccessLevel_validator)
SharedContentLinkMetadataBase.audience_options.validator = bv.List(LinkAudience_validator)
SharedContentLinkMetadataBase.audience_restricting_shared_folder.validator = bv.Nullable(AudienceRestrictingSharedFolder_validator)
SharedContentLinkMetadataBase.current_audience.validator = LinkAudience_validator
SharedContentLinkMetadataBase.expiry.validator = bv.Nullable(common.DropboxTimestamp_validator)
SharedContentLinkMetadataBase.link_permissions.validator = bv.List(LinkPermission_validator)
SharedContentLinkMetadataBase.password_protected.validator = bv.Boolean()
SharedContentLinkMetadataBase._all_field_names_ = set([
    'audience_options',
    'audience_restricting_shared_folder',
    'current_audience',
    'expiry',
    'link_permissions',
    'password_protected',
SharedContentLinkMetadataBase._all_fields_ = [
    ('access_level', SharedContentLinkMetadataBase.access_level.validator),
    ('audience_options', SharedContentLinkMetadataBase.audience_options.validator),
    ('audience_restricting_shared_folder', SharedContentLinkMetadataBase.audience_restricting_shared_folder.validator),
    ('current_audience', SharedContentLinkMetadataBase.current_audience.validator),
    ('expiry', SharedContentLinkMetadataBase.expiry.validator),
    ('link_permissions', SharedContentLinkMetadataBase.link_permissions.validator),
    ('password_protected', SharedContentLinkMetadataBase.password_protected.validator),
ExpectedSharedContentLinkMetadata._all_field_names_ = SharedContentLinkMetadataBase._all_field_names_.union(set([]))
ExpectedSharedContentLinkMetadata._all_fields_ = SharedContentLinkMetadataBase._all_fields_ + []
FileAction._disable_viewer_info_validator = bv.Void()
FileAction._edit_contents_validator = bv.Void()
FileAction._enable_viewer_info_validator = bv.Void()
FileAction._invite_viewer_validator = bv.Void()
FileAction._invite_viewer_no_comment_validator = bv.Void()
FileAction._invite_editor_validator = bv.Void()
FileAction._unshare_validator = bv.Void()
FileAction._relinquish_membership_validator = bv.Void()
FileAction._share_link_validator = bv.Void()
FileAction._create_link_validator = bv.Void()
FileAction._create_view_link_validator = bv.Void()
FileAction._create_edit_link_validator = bv.Void()
FileAction._other_validator = bv.Void()
FileAction._tagmap = {
    'disable_viewer_info': FileAction._disable_viewer_info_validator,
    'edit_contents': FileAction._edit_contents_validator,
    'enable_viewer_info': FileAction._enable_viewer_info_validator,
    'invite_viewer': FileAction._invite_viewer_validator,
    'invite_viewer_no_comment': FileAction._invite_viewer_no_comment_validator,
    'invite_editor': FileAction._invite_editor_validator,
    'unshare': FileAction._unshare_validator,
    'relinquish_membership': FileAction._relinquish_membership_validator,
    'share_link': FileAction._share_link_validator,
    'create_link': FileAction._create_link_validator,
    'create_view_link': FileAction._create_view_link_validator,
    'create_edit_link': FileAction._create_edit_link_validator,
    'other': FileAction._other_validator,
FileAction.disable_viewer_info = FileAction('disable_viewer_info')
FileAction.edit_contents = FileAction('edit_contents')
FileAction.enable_viewer_info = FileAction('enable_viewer_info')
FileAction.invite_viewer = FileAction('invite_viewer')
FileAction.invite_viewer_no_comment = FileAction('invite_viewer_no_comment')
FileAction.invite_editor = FileAction('invite_editor')
FileAction.unshare = FileAction('unshare')
FileAction.relinquish_membership = FileAction('relinquish_membership')
FileAction.share_link = FileAction('share_link')
FileAction.create_link = FileAction('create_link')
FileAction.create_view_link = FileAction('create_view_link')
FileAction.create_edit_link = FileAction('create_edit_link')
FileAction.other = FileAction('other')
FileErrorResult._file_not_found_error_validator = files.Id_validator
FileErrorResult._invalid_file_action_error_validator = files.Id_validator
FileErrorResult._permission_denied_error_validator = files.Id_validator
FileErrorResult._other_validator = bv.Void()
FileErrorResult._tagmap = {
    'file_not_found_error': FileErrorResult._file_not_found_error_validator,
    'invalid_file_action_error': FileErrorResult._invalid_file_action_error_validator,
    'permission_denied_error': FileErrorResult._permission_denied_error_validator,
    'other': FileErrorResult._other_validator,
FileErrorResult.other = FileErrorResult('other')
SharedLinkMetadata.url.validator = bv.String()
SharedLinkMetadata.id.validator = bv.Nullable(Id_validator)
SharedLinkMetadata.name.validator = bv.String()
SharedLinkMetadata.expires.validator = bv.Nullable(common.DropboxTimestamp_validator)
SharedLinkMetadata.path_lower.validator = bv.Nullable(bv.String())
SharedLinkMetadata.link_permissions.validator = LinkPermissions_validator
SharedLinkMetadata.team_member_info.validator = bv.Nullable(TeamMemberInfo_validator)
SharedLinkMetadata.content_owner_team_info.validator = bv.Nullable(TeamInfo_validator)
SharedLinkMetadata._field_names_ = set([
    'team_member_info',
    'content_owner_team_info',
SharedLinkMetadata._all_field_names_ = SharedLinkMetadata._field_names_
SharedLinkMetadata._fields_ = [
    ('url', SharedLinkMetadata.url.validator),
    ('id', SharedLinkMetadata.id.validator),
    ('name', SharedLinkMetadata.name.validator),
    ('expires', SharedLinkMetadata.expires.validator),
    ('path_lower', SharedLinkMetadata.path_lower.validator),
    ('link_permissions', SharedLinkMetadata.link_permissions.validator),
    ('team_member_info', SharedLinkMetadata.team_member_info.validator),
    ('content_owner_team_info', SharedLinkMetadata.content_owner_team_info.validator),
SharedLinkMetadata._all_fields_ = SharedLinkMetadata._fields_
SharedLinkMetadata._tag_to_subtype_ = {
    ('file',): FileLinkMetadata_validator,
    ('folder',): FolderLinkMetadata_validator,
SharedLinkMetadata._pytype_to_tag_and_subtype_ = {
    FileLinkMetadata: (('file',), FileLinkMetadata_validator),
    FolderLinkMetadata: (('folder',), FolderLinkMetadata_validator),
SharedLinkMetadata._is_catch_all_ = True
FileLinkMetadata.client_modified.validator = common.DropboxTimestamp_validator
FileLinkMetadata.server_modified.validator = common.DropboxTimestamp_validator
FileLinkMetadata.rev.validator = Rev_validator
FileLinkMetadata.size.validator = bv.UInt64()
FileLinkMetadata._field_names_ = set([
FileLinkMetadata._all_field_names_ = SharedLinkMetadata._all_field_names_.union(FileLinkMetadata._field_names_)
FileLinkMetadata._fields_ = [
    ('client_modified', FileLinkMetadata.client_modified.validator),
    ('server_modified', FileLinkMetadata.server_modified.validator),
    ('rev', FileLinkMetadata.rev.validator),
    ('size', FileLinkMetadata.size.validator),
FileLinkMetadata._all_fields_ = SharedLinkMetadata._all_fields_ + FileLinkMetadata._fields_
FileMemberActionError._invalid_member_validator = bv.Void()
FileMemberActionError._no_permission_validator = bv.Void()
FileMemberActionError._access_error_validator = SharingFileAccessError_validator
FileMemberActionError._no_explicit_access_validator = MemberAccessLevelResult_validator
FileMemberActionError._other_validator = bv.Void()
FileMemberActionError._tagmap = {
    'invalid_member': FileMemberActionError._invalid_member_validator,
    'no_permission': FileMemberActionError._no_permission_validator,
    'access_error': FileMemberActionError._access_error_validator,
    'no_explicit_access': FileMemberActionError._no_explicit_access_validator,
    'other': FileMemberActionError._other_validator,
FileMemberActionError.invalid_member = FileMemberActionError('invalid_member')
FileMemberActionError.no_permission = FileMemberActionError('no_permission')
FileMemberActionError.other = FileMemberActionError('other')
FileMemberActionIndividualResult._success_validator = bv.Nullable(AccessLevel_validator)
FileMemberActionIndividualResult._member_error_validator = FileMemberActionError_validator
FileMemberActionIndividualResult._tagmap = {
    'success': FileMemberActionIndividualResult._success_validator,
    'member_error': FileMemberActionIndividualResult._member_error_validator,
FileMemberActionResult.member.validator = MemberSelector_validator
FileMemberActionResult.result.validator = FileMemberActionIndividualResult_validator
FileMemberActionResult.sckey_sha1.validator = bv.Nullable(bv.String())
FileMemberActionResult.invitation_signature.validator = bv.Nullable(bv.List(bv.String()))
FileMemberActionResult._all_field_names_ = set([
    'sckey_sha1',
    'invitation_signature',
FileMemberActionResult._all_fields_ = [
    ('member', FileMemberActionResult.member.validator),
    ('result', FileMemberActionResult.result.validator),
    ('sckey_sha1', FileMemberActionResult.sckey_sha1.validator),
    ('invitation_signature', FileMemberActionResult.invitation_signature.validator),
FileMemberRemoveActionResult._success_validator = MemberAccessLevelResult_validator
FileMemberRemoveActionResult._member_error_validator = FileMemberActionError_validator
FileMemberRemoveActionResult._other_validator = bv.Void()
FileMemberRemoveActionResult._tagmap = {
    'success': FileMemberRemoveActionResult._success_validator,
    'member_error': FileMemberRemoveActionResult._member_error_validator,
    'other': FileMemberRemoveActionResult._other_validator,
FileMemberRemoveActionResult.other = FileMemberRemoveActionResult('other')
FilePermission.action.validator = FileAction_validator
FilePermission.allow.validator = bv.Boolean()
FilePermission.reason.validator = bv.Nullable(PermissionDeniedReason_validator)
FilePermission._all_field_names_ = set([
    'action',
    'allow',
FilePermission._all_fields_ = [
    ('action', FilePermission.action.validator),
    ('allow', FilePermission.allow.validator),
    ('reason', FilePermission.reason.validator),
FolderAction._change_options_validator = bv.Void()
FolderAction._disable_viewer_info_validator = bv.Void()
FolderAction._edit_contents_validator = bv.Void()
FolderAction._enable_viewer_info_validator = bv.Void()
FolderAction._invite_editor_validator = bv.Void()
FolderAction._invite_viewer_validator = bv.Void()
FolderAction._invite_viewer_no_comment_validator = bv.Void()
FolderAction._relinquish_membership_validator = bv.Void()
FolderAction._unmount_validator = bv.Void()
FolderAction._unshare_validator = bv.Void()
FolderAction._leave_a_copy_validator = bv.Void()
FolderAction._share_link_validator = bv.Void()
FolderAction._create_link_validator = bv.Void()
FolderAction._set_access_inheritance_validator = bv.Void()
FolderAction._other_validator = bv.Void()
FolderAction._tagmap = {
    'change_options': FolderAction._change_options_validator,
    'disable_viewer_info': FolderAction._disable_viewer_info_validator,
    'edit_contents': FolderAction._edit_contents_validator,
    'enable_viewer_info': FolderAction._enable_viewer_info_validator,
    'invite_editor': FolderAction._invite_editor_validator,
    'invite_viewer': FolderAction._invite_viewer_validator,
    'invite_viewer_no_comment': FolderAction._invite_viewer_no_comment_validator,
    'relinquish_membership': FolderAction._relinquish_membership_validator,
    'unmount': FolderAction._unmount_validator,
    'unshare': FolderAction._unshare_validator,
    'leave_a_copy': FolderAction._leave_a_copy_validator,
    'share_link': FolderAction._share_link_validator,
    'create_link': FolderAction._create_link_validator,
    'set_access_inheritance': FolderAction._set_access_inheritance_validator,
    'other': FolderAction._other_validator,
FolderAction.change_options = FolderAction('change_options')
FolderAction.disable_viewer_info = FolderAction('disable_viewer_info')
FolderAction.edit_contents = FolderAction('edit_contents')
FolderAction.enable_viewer_info = FolderAction('enable_viewer_info')
FolderAction.invite_editor = FolderAction('invite_editor')
FolderAction.invite_viewer = FolderAction('invite_viewer')
FolderAction.invite_viewer_no_comment = FolderAction('invite_viewer_no_comment')
FolderAction.relinquish_membership = FolderAction('relinquish_membership')
FolderAction.unmount = FolderAction('unmount')
FolderAction.unshare = FolderAction('unshare')
FolderAction.leave_a_copy = FolderAction('leave_a_copy')
FolderAction.share_link = FolderAction('share_link')
FolderAction.create_link = FolderAction('create_link')
FolderAction.set_access_inheritance = FolderAction('set_access_inheritance')
FolderAction.other = FolderAction('other')
FolderLinkMetadata._field_names_ = set([])
FolderLinkMetadata._all_field_names_ = SharedLinkMetadata._all_field_names_.union(FolderLinkMetadata._field_names_)
FolderLinkMetadata._fields_ = []
FolderLinkMetadata._all_fields_ = SharedLinkMetadata._all_fields_ + FolderLinkMetadata._fields_
FolderPermission.action.validator = FolderAction_validator
FolderPermission.allow.validator = bv.Boolean()
FolderPermission.reason.validator = bv.Nullable(PermissionDeniedReason_validator)
FolderPermission._all_field_names_ = set([
FolderPermission._all_fields_ = [
    ('action', FolderPermission.action.validator),
    ('allow', FolderPermission.allow.validator),
    ('reason', FolderPermission.reason.validator),
FolderPolicy.member_policy.validator = bv.Nullable(MemberPolicy_validator)
FolderPolicy.resolved_member_policy.validator = bv.Nullable(MemberPolicy_validator)
FolderPolicy.acl_update_policy.validator = AclUpdatePolicy_validator
FolderPolicy.shared_link_policy.validator = SharedLinkPolicy_validator
FolderPolicy.viewer_info_policy.validator = bv.Nullable(ViewerInfoPolicy_validator)
FolderPolicy._all_field_names_ = set([
    'member_policy',
    'resolved_member_policy',
    'acl_update_policy',
    'shared_link_policy',
    'viewer_info_policy',
FolderPolicy._all_fields_ = [
    ('member_policy', FolderPolicy.member_policy.validator),
    ('resolved_member_policy', FolderPolicy.resolved_member_policy.validator),
    ('acl_update_policy', FolderPolicy.acl_update_policy.validator),
    ('shared_link_policy', FolderPolicy.shared_link_policy.validator),
    ('viewer_info_policy', FolderPolicy.viewer_info_policy.validator),
GetFileMetadataArg.file.validator = PathOrId_validator
GetFileMetadataArg.actions.validator = bv.Nullable(bv.List(FileAction_validator))
GetFileMetadataArg._all_field_names_ = set([
    'actions',
GetFileMetadataArg._all_fields_ = [
    ('file', GetFileMetadataArg.file.validator),
    ('actions', GetFileMetadataArg.actions.validator),
GetFileMetadataBatchArg.files.validator = bv.List(PathOrId_validator, max_items=100)
GetFileMetadataBatchArg.actions.validator = bv.Nullable(bv.List(FileAction_validator))
GetFileMetadataBatchArg._all_field_names_ = set([
GetFileMetadataBatchArg._all_fields_ = [
    ('files', GetFileMetadataBatchArg.files.validator),
    ('actions', GetFileMetadataBatchArg.actions.validator),
GetFileMetadataBatchResult.file.validator = PathOrId_validator
GetFileMetadataBatchResult.result.validator = GetFileMetadataIndividualResult_validator
GetFileMetadataBatchResult._all_field_names_ = set([
GetFileMetadataBatchResult._all_fields_ = [
    ('file', GetFileMetadataBatchResult.file.validator),
    ('result', GetFileMetadataBatchResult.result.validator),
GetFileMetadataError._user_error_validator = SharingUserError_validator
GetFileMetadataError._access_error_validator = SharingFileAccessError_validator
GetFileMetadataError._other_validator = bv.Void()
GetFileMetadataError._tagmap = {
    'user_error': GetFileMetadataError._user_error_validator,
    'access_error': GetFileMetadataError._access_error_validator,
    'other': GetFileMetadataError._other_validator,
GetFileMetadataError.other = GetFileMetadataError('other')
GetFileMetadataIndividualResult._metadata_validator = SharedFileMetadata_validator
GetFileMetadataIndividualResult._access_error_validator = SharingFileAccessError_validator
GetFileMetadataIndividualResult._other_validator = bv.Void()
GetFileMetadataIndividualResult._tagmap = {
    'metadata': GetFileMetadataIndividualResult._metadata_validator,
    'access_error': GetFileMetadataIndividualResult._access_error_validator,
    'other': GetFileMetadataIndividualResult._other_validator,
GetFileMetadataIndividualResult.other = GetFileMetadataIndividualResult('other')
GetMetadataArgs.shared_folder_id.validator = common.SharedFolderId_validator
GetMetadataArgs.actions.validator = bv.Nullable(bv.List(FolderAction_validator))
GetMetadataArgs._all_field_names_ = set([
GetMetadataArgs._all_fields_ = [
    ('shared_folder_id', GetMetadataArgs.shared_folder_id.validator),
    ('actions', GetMetadataArgs.actions.validator),
SharedLinkError._shared_link_not_found_validator = bv.Void()
SharedLinkError._shared_link_access_denied_validator = bv.Void()
SharedLinkError._unsupported_link_type_validator = bv.Void()
SharedLinkError._other_validator = bv.Void()
SharedLinkError._tagmap = {
    'shared_link_not_found': SharedLinkError._shared_link_not_found_validator,
    'shared_link_access_denied': SharedLinkError._shared_link_access_denied_validator,
    'unsupported_link_type': SharedLinkError._unsupported_link_type_validator,
    'other': SharedLinkError._other_validator,
SharedLinkError.shared_link_not_found = SharedLinkError('shared_link_not_found')
SharedLinkError.shared_link_access_denied = SharedLinkError('shared_link_access_denied')
SharedLinkError.unsupported_link_type = SharedLinkError('unsupported_link_type')
SharedLinkError.other = SharedLinkError('other')
GetSharedLinkFileError._shared_link_is_directory_validator = bv.Void()
GetSharedLinkFileError._tagmap = {
    'shared_link_is_directory': GetSharedLinkFileError._shared_link_is_directory_validator,
GetSharedLinkFileError._tagmap.update(SharedLinkError._tagmap)
GetSharedLinkFileError.shared_link_is_directory = GetSharedLinkFileError('shared_link_is_directory')
GetSharedLinkMetadataArg.url.validator = bv.String()
GetSharedLinkMetadataArg.path.validator = bv.Nullable(Path_validator)
GetSharedLinkMetadataArg.link_password.validator = bv.Nullable(bv.String())
GetSharedLinkMetadataArg._all_field_names_ = set([
    'link_password',
GetSharedLinkMetadataArg._all_fields_ = [
    ('url', GetSharedLinkMetadataArg.url.validator),
    ('path', GetSharedLinkMetadataArg.path.validator),
    ('link_password', GetSharedLinkMetadataArg.link_password.validator),
GetSharedLinksArg.path.validator = bv.Nullable(bv.String())
GetSharedLinksArg._all_field_names_ = set(['path'])
GetSharedLinksArg._all_fields_ = [('path', GetSharedLinksArg.path.validator)]
GetSharedLinksError._path_validator = files.MalformedPathError_validator
GetSharedLinksError._other_validator = bv.Void()
GetSharedLinksError._tagmap = {
    'path': GetSharedLinksError._path_validator,
    'other': GetSharedLinksError._other_validator,
GetSharedLinksError.other = GetSharedLinksError('other')
GetSharedLinksResult.links.validator = bv.List(LinkMetadata_validator)
GetSharedLinksResult._all_field_names_ = set(['links'])
GetSharedLinksResult._all_fields_ = [('links', GetSharedLinksResult.links.validator)]
GroupInfo.group_type.validator = team_common.GroupType_validator
GroupInfo.is_member.validator = bv.Boolean()
GroupInfo.is_owner.validator = bv.Boolean()
GroupInfo.same_team.validator = bv.Boolean()
GroupInfo._all_field_names_ = team_common.GroupSummary._all_field_names_.union(set([
    'group_type',
    'is_member',
    'is_owner',
    'same_team',
GroupInfo._all_fields_ = team_common.GroupSummary._all_fields_ + [
    ('group_type', GroupInfo.group_type.validator),
    ('is_member', GroupInfo.is_member.validator),
    ('is_owner', GroupInfo.is_owner.validator),
    ('same_team', GroupInfo.same_team.validator),
MembershipInfo.access_type.validator = AccessLevel_validator
MembershipInfo.permissions.validator = bv.Nullable(bv.List(MemberPermission_validator))
MembershipInfo.initials.validator = bv.Nullable(bv.String())
MembershipInfo.is_inherited.validator = bv.Boolean()
MembershipInfo._all_field_names_ = set([
    'access_type',
    'permissions',
    'initials',
    'is_inherited',
MembershipInfo._all_fields_ = [
    ('access_type', MembershipInfo.access_type.validator),
    ('permissions', MembershipInfo.permissions.validator),
    ('initials', MembershipInfo.initials.validator),
    ('is_inherited', MembershipInfo.is_inherited.validator),
GroupMembershipInfo.group.validator = GroupInfo_validator
GroupMembershipInfo._all_field_names_ = MembershipInfo._all_field_names_.union(set(['group']))
GroupMembershipInfo._all_fields_ = MembershipInfo._all_fields_ + [('group', GroupMembershipInfo.group.validator)]
InsufficientPlan.message.validator = bv.String()
InsufficientPlan.upsell_url.validator = bv.Nullable(bv.String())
InsufficientPlan._all_field_names_ = set([
    'message',
    'upsell_url',
InsufficientPlan._all_fields_ = [
    ('message', InsufficientPlan.message.validator),
    ('upsell_url', InsufficientPlan.upsell_url.validator),
InsufficientQuotaAmounts.space_needed.validator = bv.UInt64()
InsufficientQuotaAmounts.space_shortage.validator = bv.UInt64()
InsufficientQuotaAmounts.space_left.validator = bv.UInt64()
InsufficientQuotaAmounts._all_field_names_ = set([
    'space_needed',
    'space_shortage',
    'space_left',
InsufficientQuotaAmounts._all_fields_ = [
    ('space_needed', InsufficientQuotaAmounts.space_needed.validator),
    ('space_shortage', InsufficientQuotaAmounts.space_shortage.validator),
    ('space_left', InsufficientQuotaAmounts.space_left.validator),
InviteeInfo._email_validator = common.EmailAddress_validator
InviteeInfo._other_validator = bv.Void()
InviteeInfo._tagmap = {
    'email': InviteeInfo._email_validator,
    'other': InviteeInfo._other_validator,
InviteeInfo.other = InviteeInfo('other')
InviteeMembershipInfo.invitee.validator = InviteeInfo_validator
InviteeMembershipInfo.user.validator = bv.Nullable(UserInfo_validator)
InviteeMembershipInfo._all_field_names_ = MembershipInfo._all_field_names_.union(set([
InviteeMembershipInfo._all_fields_ = MembershipInfo._all_fields_ + [
    ('invitee', InviteeMembershipInfo.invitee.validator),
    ('user', InviteeMembershipInfo.user.validator),
JobError._unshare_folder_error_validator = UnshareFolderError_validator
JobError._remove_folder_member_error_validator = RemoveFolderMemberError_validator
JobError._relinquish_folder_membership_error_validator = RelinquishFolderMembershipError_validator
JobError._other_validator = bv.Void()
JobError._tagmap = {
    'unshare_folder_error': JobError._unshare_folder_error_validator,
    'remove_folder_member_error': JobError._remove_folder_member_error_validator,
    'relinquish_folder_membership_error': JobError._relinquish_folder_membership_error_validator,
    'other': JobError._other_validator,
JobError.other = JobError('other')
JobStatus._complete_validator = bv.Void()
JobStatus._failed_validator = JobError_validator
JobStatus._tagmap = {
    'complete': JobStatus._complete_validator,
    'failed': JobStatus._failed_validator,
JobStatus._tagmap.update(async_.PollResultBase._tagmap)
JobStatus.complete = JobStatus('complete')
LinkAccessLevel._viewer_validator = bv.Void()
LinkAccessLevel._editor_validator = bv.Void()
LinkAccessLevel._other_validator = bv.Void()
LinkAccessLevel._tagmap = {
    'viewer': LinkAccessLevel._viewer_validator,
    'editor': LinkAccessLevel._editor_validator,
    'other': LinkAccessLevel._other_validator,
LinkAccessLevel.viewer = LinkAccessLevel('viewer')
LinkAccessLevel.editor = LinkAccessLevel('editor')
LinkAccessLevel.other = LinkAccessLevel('other')
LinkAction._change_access_level_validator = bv.Void()
LinkAction._change_audience_validator = bv.Void()
LinkAction._remove_expiry_validator = bv.Void()
LinkAction._remove_password_validator = bv.Void()
LinkAction._set_expiry_validator = bv.Void()
LinkAction._set_password_validator = bv.Void()
LinkAction._other_validator = bv.Void()
LinkAction._tagmap = {
    'change_access_level': LinkAction._change_access_level_validator,
    'change_audience': LinkAction._change_audience_validator,
    'remove_expiry': LinkAction._remove_expiry_validator,
    'remove_password': LinkAction._remove_password_validator,
    'set_expiry': LinkAction._set_expiry_validator,
    'set_password': LinkAction._set_password_validator,
    'other': LinkAction._other_validator,
LinkAction.change_access_level = LinkAction('change_access_level')
LinkAction.change_audience = LinkAction('change_audience')
LinkAction.remove_expiry = LinkAction('remove_expiry')
LinkAction.remove_password = LinkAction('remove_password')
LinkAction.set_expiry = LinkAction('set_expiry')
LinkAction.set_password = LinkAction('set_password')
LinkAction.other = LinkAction('other')
LinkAudience._public_validator = bv.Void()
LinkAudience._team_validator = bv.Void()
LinkAudience._no_one_validator = bv.Void()
LinkAudience._password_validator = bv.Void()
LinkAudience._members_validator = bv.Void()
LinkAudience._other_validator = bv.Void()
LinkAudience._tagmap = {
    'public': LinkAudience._public_validator,
    'team': LinkAudience._team_validator,
    'no_one': LinkAudience._no_one_validator,
    'password': LinkAudience._password_validator,
    'members': LinkAudience._members_validator,
    'other': LinkAudience._other_validator,
LinkAudience.public = LinkAudience('public')
LinkAudience.team = LinkAudience('team')
LinkAudience.no_one = LinkAudience('no_one')
LinkAudience.password = LinkAudience('password')
LinkAudience.members = LinkAudience('members')
LinkAudience.other = LinkAudience('other')
VisibilityPolicyDisallowedReason._delete_and_recreate_validator = bv.Void()
VisibilityPolicyDisallowedReason._restricted_by_shared_folder_validator = bv.Void()
VisibilityPolicyDisallowedReason._restricted_by_team_validator = bv.Void()
VisibilityPolicyDisallowedReason._user_not_on_team_validator = bv.Void()
VisibilityPolicyDisallowedReason._user_account_type_validator = bv.Void()
VisibilityPolicyDisallowedReason._permission_denied_validator = bv.Void()
VisibilityPolicyDisallowedReason._other_validator = bv.Void()
VisibilityPolicyDisallowedReason._tagmap = {
    'delete_and_recreate': VisibilityPolicyDisallowedReason._delete_and_recreate_validator,
    'restricted_by_shared_folder': VisibilityPolicyDisallowedReason._restricted_by_shared_folder_validator,
    'restricted_by_team': VisibilityPolicyDisallowedReason._restricted_by_team_validator,
    'user_not_on_team': VisibilityPolicyDisallowedReason._user_not_on_team_validator,
    'user_account_type': VisibilityPolicyDisallowedReason._user_account_type_validator,
    'permission_denied': VisibilityPolicyDisallowedReason._permission_denied_validator,
    'other': VisibilityPolicyDisallowedReason._other_validator,
VisibilityPolicyDisallowedReason.delete_and_recreate = VisibilityPolicyDisallowedReason('delete_and_recreate')
VisibilityPolicyDisallowedReason.restricted_by_shared_folder = VisibilityPolicyDisallowedReason('restricted_by_shared_folder')
VisibilityPolicyDisallowedReason.restricted_by_team = VisibilityPolicyDisallowedReason('restricted_by_team')
VisibilityPolicyDisallowedReason.user_not_on_team = VisibilityPolicyDisallowedReason('user_not_on_team')
VisibilityPolicyDisallowedReason.user_account_type = VisibilityPolicyDisallowedReason('user_account_type')
VisibilityPolicyDisallowedReason.permission_denied = VisibilityPolicyDisallowedReason('permission_denied')
VisibilityPolicyDisallowedReason.other = VisibilityPolicyDisallowedReason('other')
LinkAudienceDisallowedReason._tagmap = {
LinkAudienceDisallowedReason._tagmap.update(VisibilityPolicyDisallowedReason._tagmap)
LinkAudienceOption.audience.validator = LinkAudience_validator
LinkAudienceOption.allowed.validator = bv.Boolean()
LinkAudienceOption.disallowed_reason.validator = bv.Nullable(LinkAudienceDisallowedReason_validator)
LinkAudienceOption._all_field_names_ = set([
    'allowed',
    'disallowed_reason',
LinkAudienceOption._all_fields_ = [
    ('audience', LinkAudienceOption.audience.validator),
    ('allowed', LinkAudienceOption.allowed.validator),
    ('disallowed_reason', LinkAudienceOption.disallowed_reason.validator),
LinkExpiry._remove_expiry_validator = bv.Void()
LinkExpiry._set_expiry_validator = common.DropboxTimestamp_validator
LinkExpiry._other_validator = bv.Void()
LinkExpiry._tagmap = {
    'remove_expiry': LinkExpiry._remove_expiry_validator,
    'set_expiry': LinkExpiry._set_expiry_validator,
    'other': LinkExpiry._other_validator,
LinkExpiry.remove_expiry = LinkExpiry('remove_expiry')
LinkExpiry.other = LinkExpiry('other')
LinkPassword._remove_password_validator = bv.Void()
LinkPassword._set_password_validator = bv.String()
LinkPassword._other_validator = bv.Void()
LinkPassword._tagmap = {
    'remove_password': LinkPassword._remove_password_validator,
    'set_password': LinkPassword._set_password_validator,
    'other': LinkPassword._other_validator,
LinkPassword.remove_password = LinkPassword('remove_password')
LinkPassword.other = LinkPassword('other')
LinkPermission.action.validator = LinkAction_validator
LinkPermission.allow.validator = bv.Boolean()
LinkPermission.reason.validator = bv.Nullable(PermissionDeniedReason_validator)
LinkPermission._all_field_names_ = set([
LinkPermission._all_fields_ = [
    ('action', LinkPermission.action.validator),
    ('allow', LinkPermission.allow.validator),
    ('reason', LinkPermission.reason.validator),
LinkPermissions.resolved_visibility.validator = bv.Nullable(ResolvedVisibility_validator)
LinkPermissions.requested_visibility.validator = bv.Nullable(RequestedVisibility_validator)
LinkPermissions.can_revoke.validator = bv.Boolean()
LinkPermissions.revoke_failure_reason.validator = bv.Nullable(SharedLinkAccessFailureReason_validator)
LinkPermissions.effective_audience.validator = bv.Nullable(LinkAudience_validator)
LinkPermissions.link_access_level.validator = bv.Nullable(LinkAccessLevel_validator)
LinkPermissions.visibility_policies.validator = bv.List(VisibilityPolicy_validator)
LinkPermissions.can_set_expiry.validator = bv.Boolean()
LinkPermissions.can_remove_expiry.validator = bv.Boolean()
LinkPermissions.allow_download.validator = bv.Boolean()
LinkPermissions.can_allow_download.validator = bv.Boolean()
LinkPermissions.can_disallow_download.validator = bv.Boolean()
LinkPermissions.allow_comments.validator = bv.Boolean()
LinkPermissions.team_restricts_comments.validator = bv.Boolean()
LinkPermissions.audience_options.validator = bv.Nullable(bv.List(LinkAudienceOption_validator))
LinkPermissions.can_set_password.validator = bv.Nullable(bv.Boolean())
LinkPermissions.can_remove_password.validator = bv.Nullable(bv.Boolean())
LinkPermissions.require_password.validator = bv.Nullable(bv.Boolean())
LinkPermissions.can_use_extended_sharing_controls.validator = bv.Nullable(bv.Boolean())
LinkPermissions._all_field_names_ = set([
    'resolved_visibility',
    'requested_visibility',
    'can_revoke',
    'revoke_failure_reason',
    'effective_audience',
    'link_access_level',
    'visibility_policies',
    'can_set_expiry',
    'can_remove_expiry',
    'allow_download',
    'can_allow_download',
    'can_disallow_download',
    'allow_comments',
    'team_restricts_comments',
    'can_set_password',
    'can_remove_password',
    'require_password',
    'can_use_extended_sharing_controls',
LinkPermissions._all_fields_ = [
    ('resolved_visibility', LinkPermissions.resolved_visibility.validator),
    ('requested_visibility', LinkPermissions.requested_visibility.validator),
    ('can_revoke', LinkPermissions.can_revoke.validator),
    ('revoke_failure_reason', LinkPermissions.revoke_failure_reason.validator),
    ('effective_audience', LinkPermissions.effective_audience.validator),
    ('link_access_level', LinkPermissions.link_access_level.validator),
    ('visibility_policies', LinkPermissions.visibility_policies.validator),
    ('can_set_expiry', LinkPermissions.can_set_expiry.validator),
    ('can_remove_expiry', LinkPermissions.can_remove_expiry.validator),
    ('allow_download', LinkPermissions.allow_download.validator),
    ('can_allow_download', LinkPermissions.can_allow_download.validator),
    ('can_disallow_download', LinkPermissions.can_disallow_download.validator),
    ('allow_comments', LinkPermissions.allow_comments.validator),
    ('team_restricts_comments', LinkPermissions.team_restricts_comments.validator),
    ('audience_options', LinkPermissions.audience_options.validator),
    ('can_set_password', LinkPermissions.can_set_password.validator),
    ('can_remove_password', LinkPermissions.can_remove_password.validator),
    ('require_password', LinkPermissions.require_password.validator),
    ('can_use_extended_sharing_controls', LinkPermissions.can_use_extended_sharing_controls.validator),
LinkSettings.access_level.validator = bv.Nullable(AccessLevel_validator)
LinkSettings.audience.validator = bv.Nullable(LinkAudience_validator)
LinkSettings.expiry.validator = bv.Nullable(LinkExpiry_validator)
LinkSettings.password.validator = bv.Nullable(LinkPassword_validator)
LinkSettings._all_field_names_ = set([
LinkSettings._all_fields_ = [
    ('access_level', LinkSettings.access_level.validator),
    ('audience', LinkSettings.audience.validator),
    ('expiry', LinkSettings.expiry.validator),
    ('password', LinkSettings.password.validator),
ListFileMembersArg.file.validator = PathOrId_validator
ListFileMembersArg.actions.validator = bv.Nullable(bv.List(MemberAction_validator))
ListFileMembersArg.include_inherited.validator = bv.Boolean()
ListFileMembersArg.limit.validator = bv.UInt32(min_value=1, max_value=300)
ListFileMembersArg._all_field_names_ = set([
    'include_inherited',
ListFileMembersArg._all_fields_ = [
    ('file', ListFileMembersArg.file.validator),
    ('actions', ListFileMembersArg.actions.validator),
    ('include_inherited', ListFileMembersArg.include_inherited.validator),
    ('limit', ListFileMembersArg.limit.validator),
ListFileMembersBatchArg.files.validator = bv.List(PathOrId_validator, max_items=100)
ListFileMembersBatchArg.limit.validator = bv.UInt32(max_value=20)
ListFileMembersBatchArg._all_field_names_ = set([
ListFileMembersBatchArg._all_fields_ = [
    ('files', ListFileMembersBatchArg.files.validator),
    ('limit', ListFileMembersBatchArg.limit.validator),
ListFileMembersBatchResult.file.validator = PathOrId_validator
ListFileMembersBatchResult.result.validator = ListFileMembersIndividualResult_validator
ListFileMembersBatchResult._all_field_names_ = set([
ListFileMembersBatchResult._all_fields_ = [
    ('file', ListFileMembersBatchResult.file.validator),
    ('result', ListFileMembersBatchResult.result.validator),
ListFileMembersContinueArg.cursor.validator = bv.String()
ListFileMembersContinueArg._all_field_names_ = set(['cursor'])
ListFileMembersContinueArg._all_fields_ = [('cursor', ListFileMembersContinueArg.cursor.validator)]
ListFileMembersContinueError._user_error_validator = SharingUserError_validator
ListFileMembersContinueError._access_error_validator = SharingFileAccessError_validator
ListFileMembersContinueError._invalid_cursor_validator = bv.Void()
ListFileMembersContinueError._other_validator = bv.Void()
ListFileMembersContinueError._tagmap = {
    'user_error': ListFileMembersContinueError._user_error_validator,
    'access_error': ListFileMembersContinueError._access_error_validator,
    'invalid_cursor': ListFileMembersContinueError._invalid_cursor_validator,
    'other': ListFileMembersContinueError._other_validator,
ListFileMembersContinueError.invalid_cursor = ListFileMembersContinueError('invalid_cursor')
ListFileMembersContinueError.other = ListFileMembersContinueError('other')
ListFileMembersCountResult.members.validator = SharedFileMembers_validator
ListFileMembersCountResult.member_count.validator = bv.UInt32()
ListFileMembersCountResult._all_field_names_ = set([
    'member_count',
ListFileMembersCountResult._all_fields_ = [
    ('members', ListFileMembersCountResult.members.validator),
    ('member_count', ListFileMembersCountResult.member_count.validator),
ListFileMembersError._user_error_validator = SharingUserError_validator
ListFileMembersError._access_error_validator = SharingFileAccessError_validator
ListFileMembersError._other_validator = bv.Void()
ListFileMembersError._tagmap = {
    'user_error': ListFileMembersError._user_error_validator,
    'access_error': ListFileMembersError._access_error_validator,
    'other': ListFileMembersError._other_validator,
ListFileMembersError.other = ListFileMembersError('other')
ListFileMembersIndividualResult._result_validator = ListFileMembersCountResult_validator
ListFileMembersIndividualResult._access_error_validator = SharingFileAccessError_validator
ListFileMembersIndividualResult._other_validator = bv.Void()
ListFileMembersIndividualResult._tagmap = {
    'result': ListFileMembersIndividualResult._result_validator,
    'access_error': ListFileMembersIndividualResult._access_error_validator,
    'other': ListFileMembersIndividualResult._other_validator,
ListFileMembersIndividualResult.other = ListFileMembersIndividualResult('other')
ListFilesArg.limit.validator = bv.UInt32(min_value=1, max_value=300)
ListFilesArg.actions.validator = bv.Nullable(bv.List(FileAction_validator))
ListFilesArg._all_field_names_ = set([
ListFilesArg._all_fields_ = [
    ('limit', ListFilesArg.limit.validator),
    ('actions', ListFilesArg.actions.validator),
ListFilesContinueArg.cursor.validator = bv.String()
ListFilesContinueArg._all_field_names_ = set(['cursor'])
ListFilesContinueArg._all_fields_ = [('cursor', ListFilesContinueArg.cursor.validator)]
ListFilesContinueError._user_error_validator = SharingUserError_validator
ListFilesContinueError._invalid_cursor_validator = bv.Void()
ListFilesContinueError._other_validator = bv.Void()
ListFilesContinueError._tagmap = {
    'user_error': ListFilesContinueError._user_error_validator,
    'invalid_cursor': ListFilesContinueError._invalid_cursor_validator,
    'other': ListFilesContinueError._other_validator,
ListFilesContinueError.invalid_cursor = ListFilesContinueError('invalid_cursor')
ListFilesContinueError.other = ListFilesContinueError('other')
ListFilesResult.entries.validator = bv.List(SharedFileMetadata_validator)
ListFilesResult.cursor.validator = bv.Nullable(bv.String())
ListFilesResult._all_field_names_ = set([
ListFilesResult._all_fields_ = [
    ('entries', ListFilesResult.entries.validator),
    ('cursor', ListFilesResult.cursor.validator),
ListFolderMembersCursorArg.actions.validator = bv.Nullable(bv.List(MemberAction_validator))
ListFolderMembersCursorArg.limit.validator = bv.UInt32(min_value=1, max_value=1000)
ListFolderMembersCursorArg._all_field_names_ = set([
ListFolderMembersCursorArg._all_fields_ = [
    ('actions', ListFolderMembersCursorArg.actions.validator),
    ('limit', ListFolderMembersCursorArg.limit.validator),
ListFolderMembersArgs.shared_folder_id.validator = common.SharedFolderId_validator
ListFolderMembersArgs._all_field_names_ = ListFolderMembersCursorArg._all_field_names_.union(set(['shared_folder_id']))
ListFolderMembersArgs._all_fields_ = ListFolderMembersCursorArg._all_fields_ + [('shared_folder_id', ListFolderMembersArgs.shared_folder_id.validator)]
ListFolderMembersContinueArg.cursor.validator = bv.String()
ListFolderMembersContinueArg._all_field_names_ = set(['cursor'])
ListFolderMembersContinueArg._all_fields_ = [('cursor', ListFolderMembersContinueArg.cursor.validator)]
ListFolderMembersContinueError._access_error_validator = SharedFolderAccessError_validator
ListFolderMembersContinueError._invalid_cursor_validator = bv.Void()
ListFolderMembersContinueError._other_validator = bv.Void()
ListFolderMembersContinueError._tagmap = {
    'access_error': ListFolderMembersContinueError._access_error_validator,
    'invalid_cursor': ListFolderMembersContinueError._invalid_cursor_validator,
    'other': ListFolderMembersContinueError._other_validator,
ListFolderMembersContinueError.invalid_cursor = ListFolderMembersContinueError('invalid_cursor')
ListFolderMembersContinueError.other = ListFolderMembersContinueError('other')
ListFoldersArgs.limit.validator = bv.UInt32(min_value=1, max_value=1000)
ListFoldersArgs.actions.validator = bv.Nullable(bv.List(FolderAction_validator))
ListFoldersArgs._all_field_names_ = set([
ListFoldersArgs._all_fields_ = [
    ('limit', ListFoldersArgs.limit.validator),
    ('actions', ListFoldersArgs.actions.validator),
ListFoldersContinueArg.cursor.validator = bv.String()
ListFoldersContinueArg._all_field_names_ = set(['cursor'])
ListFoldersContinueArg._all_fields_ = [('cursor', ListFoldersContinueArg.cursor.validator)]
ListFoldersContinueError._invalid_cursor_validator = bv.Void()
ListFoldersContinueError._other_validator = bv.Void()
ListFoldersContinueError._tagmap = {
    'invalid_cursor': ListFoldersContinueError._invalid_cursor_validator,
    'other': ListFoldersContinueError._other_validator,
ListFoldersContinueError.invalid_cursor = ListFoldersContinueError('invalid_cursor')
ListFoldersContinueError.other = ListFoldersContinueError('other')
ListFoldersResult.entries.validator = bv.List(SharedFolderMetadata_validator)
ListFoldersResult.cursor.validator = bv.Nullable(bv.String())
ListFoldersResult._all_field_names_ = set([
ListFoldersResult._all_fields_ = [
    ('entries', ListFoldersResult.entries.validator),
    ('cursor', ListFoldersResult.cursor.validator),
ListSharedLinksArg.path.validator = bv.Nullable(ReadPath_validator)
ListSharedLinksArg.cursor.validator = bv.Nullable(bv.String())
ListSharedLinksArg.direct_only.validator = bv.Nullable(bv.Boolean())
ListSharedLinksArg._all_field_names_ = set([
    'direct_only',
ListSharedLinksArg._all_fields_ = [
    ('path', ListSharedLinksArg.path.validator),
    ('cursor', ListSharedLinksArg.cursor.validator),
    ('direct_only', ListSharedLinksArg.direct_only.validator),
ListSharedLinksError._path_validator = files.LookupError_validator
ListSharedLinksError._reset_validator = bv.Void()
ListSharedLinksError._other_validator = bv.Void()
ListSharedLinksError._tagmap = {
    'path': ListSharedLinksError._path_validator,
    'reset': ListSharedLinksError._reset_validator,
    'other': ListSharedLinksError._other_validator,
ListSharedLinksError.reset = ListSharedLinksError('reset')
ListSharedLinksError.other = ListSharedLinksError('other')
ListSharedLinksResult.links.validator = bv.List(SharedLinkMetadata_validator)
ListSharedLinksResult.has_more.validator = bv.Boolean()
ListSharedLinksResult.cursor.validator = bv.Nullable(bv.String())
ListSharedLinksResult._all_field_names_ = set([
    'links',
ListSharedLinksResult._all_fields_ = [
    ('links', ListSharedLinksResult.links.validator),
    ('has_more', ListSharedLinksResult.has_more.validator),
    ('cursor', ListSharedLinksResult.cursor.validator),
MemberAccessLevelResult.access_level.validator = bv.Nullable(AccessLevel_validator)
MemberAccessLevelResult.warning.validator = bv.Nullable(bv.String())
MemberAccessLevelResult.access_details.validator = bv.Nullable(bv.List(ParentFolderAccessInfo_validator))
MemberAccessLevelResult._all_field_names_ = set([
    'access_details',
MemberAccessLevelResult._all_fields_ = [
    ('access_level', MemberAccessLevelResult.access_level.validator),
    ('warning', MemberAccessLevelResult.warning.validator),
    ('access_details', MemberAccessLevelResult.access_details.validator),
MemberAction._leave_a_copy_validator = bv.Void()
MemberAction._make_editor_validator = bv.Void()
MemberAction._make_owner_validator = bv.Void()
MemberAction._make_viewer_validator = bv.Void()
MemberAction._make_viewer_no_comment_validator = bv.Void()
MemberAction._remove_validator = bv.Void()
MemberAction._other_validator = bv.Void()
MemberAction._tagmap = {
    'leave_a_copy': MemberAction._leave_a_copy_validator,
    'make_editor': MemberAction._make_editor_validator,
    'make_owner': MemberAction._make_owner_validator,
    'make_viewer': MemberAction._make_viewer_validator,
    'make_viewer_no_comment': MemberAction._make_viewer_no_comment_validator,
    'remove': MemberAction._remove_validator,
    'other': MemberAction._other_validator,
MemberAction.leave_a_copy = MemberAction('leave_a_copy')
MemberAction.make_editor = MemberAction('make_editor')
MemberAction.make_owner = MemberAction('make_owner')
MemberAction.make_viewer = MemberAction('make_viewer')
MemberAction.make_viewer_no_comment = MemberAction('make_viewer_no_comment')
MemberAction.remove = MemberAction('remove')
MemberAction.other = MemberAction('other')
MemberPermission.action.validator = MemberAction_validator
MemberPermission.allow.validator = bv.Boolean()
MemberPermission.reason.validator = bv.Nullable(PermissionDeniedReason_validator)
MemberPermission._all_field_names_ = set([
MemberPermission._all_fields_ = [
    ('action', MemberPermission.action.validator),
    ('allow', MemberPermission.allow.validator),
    ('reason', MemberPermission.reason.validator),
MemberPolicy._team_validator = bv.Void()
MemberPolicy._anyone_validator = bv.Void()
MemberPolicy._other_validator = bv.Void()
MemberPolicy._tagmap = {
    'team': MemberPolicy._team_validator,
    'anyone': MemberPolicy._anyone_validator,
    'other': MemberPolicy._other_validator,
MemberPolicy.team = MemberPolicy('team')
MemberPolicy.anyone = MemberPolicy('anyone')
MemberPolicy.other = MemberPolicy('other')
MemberSelector._dropbox_id_validator = DropboxId_validator
MemberSelector._email_validator = common.EmailAddress_validator
MemberSelector._other_validator = bv.Void()
MemberSelector._tagmap = {
    'dropbox_id': MemberSelector._dropbox_id_validator,
    'email': MemberSelector._email_validator,
    'other': MemberSelector._other_validator,
MemberSelector.other = MemberSelector('other')
ModifySharedLinkSettingsArgs.url.validator = bv.String()
ModifySharedLinkSettingsArgs.settings.validator = SharedLinkSettings_validator
ModifySharedLinkSettingsArgs.remove_expiration.validator = bv.Boolean()
ModifySharedLinkSettingsArgs._all_field_names_ = set([
    'remove_expiration',
ModifySharedLinkSettingsArgs._all_fields_ = [
    ('url', ModifySharedLinkSettingsArgs.url.validator),
    ('settings', ModifySharedLinkSettingsArgs.settings.validator),
    ('remove_expiration', ModifySharedLinkSettingsArgs.remove_expiration.validator),
ModifySharedLinkSettingsError._settings_error_validator = SharedLinkSettingsError_validator
ModifySharedLinkSettingsError._email_not_verified_validator = bv.Void()
ModifySharedLinkSettingsError._tagmap = {
    'settings_error': ModifySharedLinkSettingsError._settings_error_validator,
    'email_not_verified': ModifySharedLinkSettingsError._email_not_verified_validator,
ModifySharedLinkSettingsError._tagmap.update(SharedLinkError._tagmap)
ModifySharedLinkSettingsError.email_not_verified = ModifySharedLinkSettingsError('email_not_verified')
MountFolderArg.shared_folder_id.validator = common.SharedFolderId_validator
MountFolderArg._all_field_names_ = set(['shared_folder_id'])
MountFolderArg._all_fields_ = [('shared_folder_id', MountFolderArg.shared_folder_id.validator)]
MountFolderError._access_error_validator = SharedFolderAccessError_validator
MountFolderError._inside_shared_folder_validator = bv.Void()
MountFolderError._insufficient_quota_validator = InsufficientQuotaAmounts_validator
MountFolderError._already_mounted_validator = bv.Void()
MountFolderError._no_permission_validator = bv.Void()
MountFolderError._not_mountable_validator = bv.Void()
MountFolderError._other_validator = bv.Void()
MountFolderError._tagmap = {
    'access_error': MountFolderError._access_error_validator,
    'inside_shared_folder': MountFolderError._inside_shared_folder_validator,
    'insufficient_quota': MountFolderError._insufficient_quota_validator,
    'already_mounted': MountFolderError._already_mounted_validator,
    'no_permission': MountFolderError._no_permission_validator,
    'not_mountable': MountFolderError._not_mountable_validator,
    'other': MountFolderError._other_validator,
MountFolderError.inside_shared_folder = MountFolderError('inside_shared_folder')
MountFolderError.already_mounted = MountFolderError('already_mounted')
MountFolderError.no_permission = MountFolderError('no_permission')
MountFolderError.not_mountable = MountFolderError('not_mountable')
MountFolderError.other = MountFolderError('other')
ParentFolderAccessInfo.folder_name.validator = bv.String()
ParentFolderAccessInfo.shared_folder_id.validator = common.SharedFolderId_validator
ParentFolderAccessInfo.permissions.validator = bv.List(MemberPermission_validator)
ParentFolderAccessInfo.path.validator = bv.String()
ParentFolderAccessInfo._all_field_names_ = set([
    'folder_name',
ParentFolderAccessInfo._all_fields_ = [
    ('folder_name', ParentFolderAccessInfo.folder_name.validator),
    ('shared_folder_id', ParentFolderAccessInfo.shared_folder_id.validator),
    ('permissions', ParentFolderAccessInfo.permissions.validator),
    ('path', ParentFolderAccessInfo.path.validator),
PathLinkMetadata.path.validator = bv.String()
PathLinkMetadata._field_names_ = set(['path'])
PathLinkMetadata._all_field_names_ = LinkMetadata._all_field_names_.union(PathLinkMetadata._field_names_)
PathLinkMetadata._fields_ = [('path', PathLinkMetadata.path.validator)]
PathLinkMetadata._all_fields_ = LinkMetadata._all_fields_ + PathLinkMetadata._fields_
PendingUploadMode._file_validator = bv.Void()
PendingUploadMode._folder_validator = bv.Void()
PendingUploadMode._tagmap = {
    'file': PendingUploadMode._file_validator,
    'folder': PendingUploadMode._folder_validator,
PendingUploadMode.file = PendingUploadMode('file')
PendingUploadMode.folder = PendingUploadMode('folder')
PermissionDeniedReason._user_not_same_team_as_owner_validator = bv.Void()
PermissionDeniedReason._user_not_allowed_by_owner_validator = bv.Void()
PermissionDeniedReason._target_is_indirect_member_validator = bv.Void()
PermissionDeniedReason._target_is_owner_validator = bv.Void()
PermissionDeniedReason._target_is_self_validator = bv.Void()
PermissionDeniedReason._target_not_active_validator = bv.Void()
PermissionDeniedReason._folder_is_limited_team_folder_validator = bv.Void()
PermissionDeniedReason._owner_not_on_team_validator = bv.Void()
PermissionDeniedReason._permission_denied_validator = bv.Void()
PermissionDeniedReason._restricted_by_team_validator = bv.Void()
PermissionDeniedReason._user_account_type_validator = bv.Void()
PermissionDeniedReason._user_not_on_team_validator = bv.Void()
PermissionDeniedReason._folder_is_inside_shared_folder_validator = bv.Void()
PermissionDeniedReason._restricted_by_parent_folder_validator = bv.Void()
PermissionDeniedReason._insufficient_plan_validator = InsufficientPlan_validator
PermissionDeniedReason._other_validator = bv.Void()
PermissionDeniedReason._tagmap = {
    'user_not_same_team_as_owner': PermissionDeniedReason._user_not_same_team_as_owner_validator,
    'user_not_allowed_by_owner': PermissionDeniedReason._user_not_allowed_by_owner_validator,
    'target_is_indirect_member': PermissionDeniedReason._target_is_indirect_member_validator,
    'target_is_owner': PermissionDeniedReason._target_is_owner_validator,
    'target_is_self': PermissionDeniedReason._target_is_self_validator,
    'target_not_active': PermissionDeniedReason._target_not_active_validator,
    'folder_is_limited_team_folder': PermissionDeniedReason._folder_is_limited_team_folder_validator,
    'owner_not_on_team': PermissionDeniedReason._owner_not_on_team_validator,
    'permission_denied': PermissionDeniedReason._permission_denied_validator,
    'restricted_by_team': PermissionDeniedReason._restricted_by_team_validator,
    'user_account_type': PermissionDeniedReason._user_account_type_validator,
    'user_not_on_team': PermissionDeniedReason._user_not_on_team_validator,
    'folder_is_inside_shared_folder': PermissionDeniedReason._folder_is_inside_shared_folder_validator,
    'restricted_by_parent_folder': PermissionDeniedReason._restricted_by_parent_folder_validator,
    'insufficient_plan': PermissionDeniedReason._insufficient_plan_validator,
    'other': PermissionDeniedReason._other_validator,
PermissionDeniedReason.user_not_same_team_as_owner = PermissionDeniedReason('user_not_same_team_as_owner')
PermissionDeniedReason.user_not_allowed_by_owner = PermissionDeniedReason('user_not_allowed_by_owner')
PermissionDeniedReason.target_is_indirect_member = PermissionDeniedReason('target_is_indirect_member')
PermissionDeniedReason.target_is_owner = PermissionDeniedReason('target_is_owner')
PermissionDeniedReason.target_is_self = PermissionDeniedReason('target_is_self')
PermissionDeniedReason.target_not_active = PermissionDeniedReason('target_not_active')
PermissionDeniedReason.folder_is_limited_team_folder = PermissionDeniedReason('folder_is_limited_team_folder')
PermissionDeniedReason.owner_not_on_team = PermissionDeniedReason('owner_not_on_team')
PermissionDeniedReason.permission_denied = PermissionDeniedReason('permission_denied')
PermissionDeniedReason.restricted_by_team = PermissionDeniedReason('restricted_by_team')
PermissionDeniedReason.user_account_type = PermissionDeniedReason('user_account_type')
PermissionDeniedReason.user_not_on_team = PermissionDeniedReason('user_not_on_team')
PermissionDeniedReason.folder_is_inside_shared_folder = PermissionDeniedReason('folder_is_inside_shared_folder')
PermissionDeniedReason.restricted_by_parent_folder = PermissionDeniedReason('restricted_by_parent_folder')
PermissionDeniedReason.other = PermissionDeniedReason('other')
RelinquishFileMembershipArg.file.validator = PathOrId_validator
RelinquishFileMembershipArg._all_field_names_ = set(['file'])
RelinquishFileMembershipArg._all_fields_ = [('file', RelinquishFileMembershipArg.file.validator)]
RelinquishFileMembershipError._access_error_validator = SharingFileAccessError_validator
RelinquishFileMembershipError._group_access_validator = bv.Void()
RelinquishFileMembershipError._no_permission_validator = bv.Void()
RelinquishFileMembershipError._other_validator = bv.Void()
RelinquishFileMembershipError._tagmap = {
    'access_error': RelinquishFileMembershipError._access_error_validator,
    'group_access': RelinquishFileMembershipError._group_access_validator,
    'no_permission': RelinquishFileMembershipError._no_permission_validator,
    'other': RelinquishFileMembershipError._other_validator,
RelinquishFileMembershipError.group_access = RelinquishFileMembershipError('group_access')
RelinquishFileMembershipError.no_permission = RelinquishFileMembershipError('no_permission')
RelinquishFileMembershipError.other = RelinquishFileMembershipError('other')
RelinquishFolderMembershipArg.shared_folder_id.validator = common.SharedFolderId_validator
RelinquishFolderMembershipArg.leave_a_copy.validator = bv.Boolean()
RelinquishFolderMembershipArg._all_field_names_ = set([
    'leave_a_copy',
RelinquishFolderMembershipArg._all_fields_ = [
    ('shared_folder_id', RelinquishFolderMembershipArg.shared_folder_id.validator),
    ('leave_a_copy', RelinquishFolderMembershipArg.leave_a_copy.validator),
RelinquishFolderMembershipError._access_error_validator = SharedFolderAccessError_validator
RelinquishFolderMembershipError._folder_owner_validator = bv.Void()
RelinquishFolderMembershipError._mounted_validator = bv.Void()
RelinquishFolderMembershipError._group_access_validator = bv.Void()
RelinquishFolderMembershipError._team_folder_validator = bv.Void()
RelinquishFolderMembershipError._no_permission_validator = bv.Void()
RelinquishFolderMembershipError._no_explicit_access_validator = bv.Void()
RelinquishFolderMembershipError._other_validator = bv.Void()
RelinquishFolderMembershipError._tagmap = {
    'access_error': RelinquishFolderMembershipError._access_error_validator,
    'folder_owner': RelinquishFolderMembershipError._folder_owner_validator,
    'mounted': RelinquishFolderMembershipError._mounted_validator,
    'group_access': RelinquishFolderMembershipError._group_access_validator,
    'team_folder': RelinquishFolderMembershipError._team_folder_validator,
    'no_permission': RelinquishFolderMembershipError._no_permission_validator,
    'no_explicit_access': RelinquishFolderMembershipError._no_explicit_access_validator,
    'other': RelinquishFolderMembershipError._other_validator,
RelinquishFolderMembershipError.folder_owner = RelinquishFolderMembershipError('folder_owner')
RelinquishFolderMembershipError.mounted = RelinquishFolderMembershipError('mounted')
RelinquishFolderMembershipError.group_access = RelinquishFolderMembershipError('group_access')
RelinquishFolderMembershipError.team_folder = RelinquishFolderMembershipError('team_folder')
RelinquishFolderMembershipError.no_permission = RelinquishFolderMembershipError('no_permission')
RelinquishFolderMembershipError.no_explicit_access = RelinquishFolderMembershipError('no_explicit_access')
RelinquishFolderMembershipError.other = RelinquishFolderMembershipError('other')
RemoveFileMemberArg.file.validator = PathOrId_validator
RemoveFileMemberArg.member.validator = MemberSelector_validator
RemoveFileMemberArg._all_field_names_ = set([
RemoveFileMemberArg._all_fields_ = [
    ('file', RemoveFileMemberArg.file.validator),
    ('member', RemoveFileMemberArg.member.validator),
RemoveFileMemberError._user_error_validator = SharingUserError_validator
RemoveFileMemberError._access_error_validator = SharingFileAccessError_validator
RemoveFileMemberError._no_explicit_access_validator = MemberAccessLevelResult_validator
RemoveFileMemberError._other_validator = bv.Void()
RemoveFileMemberError._tagmap = {
    'user_error': RemoveFileMemberError._user_error_validator,
    'access_error': RemoveFileMemberError._access_error_validator,
    'no_explicit_access': RemoveFileMemberError._no_explicit_access_validator,
    'other': RemoveFileMemberError._other_validator,
RemoveFileMemberError.other = RemoveFileMemberError('other')
RemoveFolderMemberArg.shared_folder_id.validator = common.SharedFolderId_validator
RemoveFolderMemberArg.member.validator = MemberSelector_validator
RemoveFolderMemberArg.leave_a_copy.validator = bv.Boolean()
RemoveFolderMemberArg._all_field_names_ = set([
RemoveFolderMemberArg._all_fields_ = [
    ('shared_folder_id', RemoveFolderMemberArg.shared_folder_id.validator),
    ('member', RemoveFolderMemberArg.member.validator),
    ('leave_a_copy', RemoveFolderMemberArg.leave_a_copy.validator),
RemoveFolderMemberError._access_error_validator = SharedFolderAccessError_validator
RemoveFolderMemberError._member_error_validator = SharedFolderMemberError_validator
RemoveFolderMemberError._folder_owner_validator = bv.Void()
RemoveFolderMemberError._group_access_validator = bv.Void()
RemoveFolderMemberError._team_folder_validator = bv.Void()
RemoveFolderMemberError._no_permission_validator = bv.Void()
RemoveFolderMemberError._too_many_files_validator = bv.Void()
RemoveFolderMemberError._other_validator = bv.Void()
RemoveFolderMemberError._tagmap = {
    'access_error': RemoveFolderMemberError._access_error_validator,
    'member_error': RemoveFolderMemberError._member_error_validator,
    'folder_owner': RemoveFolderMemberError._folder_owner_validator,
    'group_access': RemoveFolderMemberError._group_access_validator,
    'team_folder': RemoveFolderMemberError._team_folder_validator,
    'no_permission': RemoveFolderMemberError._no_permission_validator,
    'too_many_files': RemoveFolderMemberError._too_many_files_validator,
    'other': RemoveFolderMemberError._other_validator,
RemoveFolderMemberError.folder_owner = RemoveFolderMemberError('folder_owner')
RemoveFolderMemberError.group_access = RemoveFolderMemberError('group_access')
RemoveFolderMemberError.team_folder = RemoveFolderMemberError('team_folder')
RemoveFolderMemberError.no_permission = RemoveFolderMemberError('no_permission')
RemoveFolderMemberError.too_many_files = RemoveFolderMemberError('too_many_files')
RemoveFolderMemberError.other = RemoveFolderMemberError('other')
RemoveMemberJobStatus._complete_validator = MemberAccessLevelResult_validator
RemoveMemberJobStatus._failed_validator = RemoveFolderMemberError_validator
RemoveMemberJobStatus._tagmap = {
    'complete': RemoveMemberJobStatus._complete_validator,
    'failed': RemoveMemberJobStatus._failed_validator,
RemoveMemberJobStatus._tagmap.update(async_.PollResultBase._tagmap)
RequestedLinkAccessLevel._viewer_validator = bv.Void()
RequestedLinkAccessLevel._editor_validator = bv.Void()
RequestedLinkAccessLevel._max_validator = bv.Void()
RequestedLinkAccessLevel._default_validator = bv.Void()
RequestedLinkAccessLevel._other_validator = bv.Void()
RequestedLinkAccessLevel._tagmap = {
    'viewer': RequestedLinkAccessLevel._viewer_validator,
    'editor': RequestedLinkAccessLevel._editor_validator,
    'max': RequestedLinkAccessLevel._max_validator,
    'default': RequestedLinkAccessLevel._default_validator,
    'other': RequestedLinkAccessLevel._other_validator,
RequestedLinkAccessLevel.viewer = RequestedLinkAccessLevel('viewer')
RequestedLinkAccessLevel.editor = RequestedLinkAccessLevel('editor')
RequestedLinkAccessLevel.max = RequestedLinkAccessLevel('max')
RequestedLinkAccessLevel.default = RequestedLinkAccessLevel('default')
RequestedLinkAccessLevel.other = RequestedLinkAccessLevel('other')
RevokeSharedLinkArg.url.validator = bv.String()
RevokeSharedLinkArg._all_field_names_ = set(['url'])
RevokeSharedLinkArg._all_fields_ = [('url', RevokeSharedLinkArg.url.validator)]
RevokeSharedLinkError._shared_link_malformed_validator = bv.Void()
RevokeSharedLinkError._tagmap = {
    'shared_link_malformed': RevokeSharedLinkError._shared_link_malformed_validator,
RevokeSharedLinkError._tagmap.update(SharedLinkError._tagmap)
RevokeSharedLinkError.shared_link_malformed = RevokeSharedLinkError('shared_link_malformed')
SetAccessInheritanceArg.access_inheritance.validator = AccessInheritance_validator
SetAccessInheritanceArg.shared_folder_id.validator = common.SharedFolderId_validator
SetAccessInheritanceArg._all_field_names_ = set([
    'access_inheritance',
SetAccessInheritanceArg._all_fields_ = [
    ('access_inheritance', SetAccessInheritanceArg.access_inheritance.validator),
    ('shared_folder_id', SetAccessInheritanceArg.shared_folder_id.validator),
SetAccessInheritanceError._access_error_validator = SharedFolderAccessError_validator
SetAccessInheritanceError._no_permission_validator = bv.Void()
SetAccessInheritanceError._other_validator = bv.Void()
SetAccessInheritanceError._tagmap = {
    'access_error': SetAccessInheritanceError._access_error_validator,
    'no_permission': SetAccessInheritanceError._no_permission_validator,
    'other': SetAccessInheritanceError._other_validator,
SetAccessInheritanceError.no_permission = SetAccessInheritanceError('no_permission')
SetAccessInheritanceError.other = SetAccessInheritanceError('other')
ShareFolderArgBase.acl_update_policy.validator = bv.Nullable(AclUpdatePolicy_validator)
ShareFolderArgBase.force_async.validator = bv.Boolean()
ShareFolderArgBase.member_policy.validator = bv.Nullable(MemberPolicy_validator)
ShareFolderArgBase.path.validator = files.WritePathOrId_validator
ShareFolderArgBase.shared_link_policy.validator = bv.Nullable(SharedLinkPolicy_validator)
ShareFolderArgBase.viewer_info_policy.validator = bv.Nullable(ViewerInfoPolicy_validator)
ShareFolderArgBase.access_inheritance.validator = AccessInheritance_validator
ShareFolderArgBase._all_field_names_ = set([
ShareFolderArgBase._all_fields_ = [
    ('acl_update_policy', ShareFolderArgBase.acl_update_policy.validator),
    ('force_async', ShareFolderArgBase.force_async.validator),
    ('member_policy', ShareFolderArgBase.member_policy.validator),
    ('path', ShareFolderArgBase.path.validator),
    ('shared_link_policy', ShareFolderArgBase.shared_link_policy.validator),
    ('viewer_info_policy', ShareFolderArgBase.viewer_info_policy.validator),
    ('access_inheritance', ShareFolderArgBase.access_inheritance.validator),
ShareFolderArg.actions.validator = bv.Nullable(bv.List(FolderAction_validator))
ShareFolderArg.link_settings.validator = bv.Nullable(LinkSettings_validator)
ShareFolderArg._all_field_names_ = ShareFolderArgBase._all_field_names_.union(set([
    'link_settings',
ShareFolderArg._all_fields_ = ShareFolderArgBase._all_fields_ + [
    ('actions', ShareFolderArg.actions.validator),
    ('link_settings', ShareFolderArg.link_settings.validator),
ShareFolderErrorBase._email_unverified_validator = bv.Void()
ShareFolderErrorBase._bad_path_validator = SharePathError_validator
ShareFolderErrorBase._team_policy_disallows_member_policy_validator = bv.Void()
ShareFolderErrorBase._disallowed_shared_link_policy_validator = bv.Void()
ShareFolderErrorBase._other_validator = bv.Void()
ShareFolderErrorBase._tagmap = {
    'email_unverified': ShareFolderErrorBase._email_unverified_validator,
    'bad_path': ShareFolderErrorBase._bad_path_validator,
    'team_policy_disallows_member_policy': ShareFolderErrorBase._team_policy_disallows_member_policy_validator,
    'disallowed_shared_link_policy': ShareFolderErrorBase._disallowed_shared_link_policy_validator,
    'other': ShareFolderErrorBase._other_validator,
ShareFolderErrorBase.email_unverified = ShareFolderErrorBase('email_unverified')
ShareFolderErrorBase.team_policy_disallows_member_policy = ShareFolderErrorBase('team_policy_disallows_member_policy')
ShareFolderErrorBase.disallowed_shared_link_policy = ShareFolderErrorBase('disallowed_shared_link_policy')
ShareFolderErrorBase.other = ShareFolderErrorBase('other')
ShareFolderError._no_permission_validator = bv.Void()
ShareFolderError._tagmap = {
    'no_permission': ShareFolderError._no_permission_validator,
ShareFolderError._tagmap.update(ShareFolderErrorBase._tagmap)
ShareFolderError.no_permission = ShareFolderError('no_permission')
ShareFolderJobStatus._complete_validator = SharedFolderMetadata_validator
ShareFolderJobStatus._failed_validator = ShareFolderError_validator
ShareFolderJobStatus._tagmap = {
    'complete': ShareFolderJobStatus._complete_validator,
    'failed': ShareFolderJobStatus._failed_validator,
ShareFolderJobStatus._tagmap.update(async_.PollResultBase._tagmap)
ShareFolderLaunch._complete_validator = SharedFolderMetadata_validator
ShareFolderLaunch._tagmap = {
    'complete': ShareFolderLaunch._complete_validator,
ShareFolderLaunch._tagmap.update(async_.LaunchResultBase._tagmap)
SharePathError._is_file_validator = bv.Void()
SharePathError._inside_shared_folder_validator = bv.Void()
SharePathError._contains_shared_folder_validator = bv.Void()
SharePathError._contains_app_folder_validator = bv.Void()
SharePathError._contains_team_folder_validator = bv.Void()
SharePathError._is_app_folder_validator = bv.Void()
SharePathError._inside_app_folder_validator = bv.Void()
SharePathError._is_public_folder_validator = bv.Void()
SharePathError._inside_public_folder_validator = bv.Void()
SharePathError._already_shared_validator = SharedFolderMetadata_validator
SharePathError._invalid_path_validator = bv.Void()
SharePathError._is_osx_package_validator = bv.Void()
SharePathError._inside_osx_package_validator = bv.Void()
SharePathError._is_vault_validator = bv.Void()
SharePathError._is_vault_locked_validator = bv.Void()
SharePathError._is_family_validator = bv.Void()
SharePathError._other_validator = bv.Void()
SharePathError._tagmap = {
    'is_file': SharePathError._is_file_validator,
    'inside_shared_folder': SharePathError._inside_shared_folder_validator,
    'contains_shared_folder': SharePathError._contains_shared_folder_validator,
    'contains_app_folder': SharePathError._contains_app_folder_validator,
    'contains_team_folder': SharePathError._contains_team_folder_validator,
    'is_app_folder': SharePathError._is_app_folder_validator,
    'inside_app_folder': SharePathError._inside_app_folder_validator,
    'is_public_folder': SharePathError._is_public_folder_validator,
    'inside_public_folder': SharePathError._inside_public_folder_validator,
    'already_shared': SharePathError._already_shared_validator,
    'invalid_path': SharePathError._invalid_path_validator,
    'is_osx_package': SharePathError._is_osx_package_validator,
    'inside_osx_package': SharePathError._inside_osx_package_validator,
    'is_vault': SharePathError._is_vault_validator,
    'is_vault_locked': SharePathError._is_vault_locked_validator,
    'is_family': SharePathError._is_family_validator,
    'other': SharePathError._other_validator,
SharePathError.is_file = SharePathError('is_file')
SharePathError.inside_shared_folder = SharePathError('inside_shared_folder')
SharePathError.contains_shared_folder = SharePathError('contains_shared_folder')
SharePathError.contains_app_folder = SharePathError('contains_app_folder')
SharePathError.contains_team_folder = SharePathError('contains_team_folder')
SharePathError.is_app_folder = SharePathError('is_app_folder')
SharePathError.inside_app_folder = SharePathError('inside_app_folder')
SharePathError.is_public_folder = SharePathError('is_public_folder')
SharePathError.inside_public_folder = SharePathError('inside_public_folder')
SharePathError.invalid_path = SharePathError('invalid_path')
SharePathError.is_osx_package = SharePathError('is_osx_package')
SharePathError.inside_osx_package = SharePathError('inside_osx_package')
SharePathError.is_vault = SharePathError('is_vault')
SharePathError.is_vault_locked = SharePathError('is_vault_locked')
SharePathError.is_family = SharePathError('is_family')
SharePathError.other = SharePathError('other')
SharedContentLinkMetadata.audience_exceptions.validator = bv.Nullable(AudienceExceptions_validator)
SharedContentLinkMetadata.url.validator = bv.String()
SharedContentLinkMetadata._all_field_names_ = SharedContentLinkMetadataBase._all_field_names_.union(set([
    'audience_exceptions',
SharedContentLinkMetadata._all_fields_ = SharedContentLinkMetadataBase._all_fields_ + [
    ('audience_exceptions', SharedContentLinkMetadata.audience_exceptions.validator),
    ('url', SharedContentLinkMetadata.url.validator),
SharedFileMembers.users.validator = bv.List(UserFileMembershipInfo_validator)
SharedFileMembers.groups.validator = bv.List(GroupMembershipInfo_validator)
SharedFileMembers.invitees.validator = bv.List(InviteeMembershipInfo_validator)
SharedFileMembers.cursor.validator = bv.Nullable(bv.String())
SharedFileMembers._all_field_names_ = set([
    'groups',
SharedFileMembers._all_fields_ = [
    ('users', SharedFileMembers.users.validator),
    ('groups', SharedFileMembers.groups.validator),
    ('invitees', SharedFileMembers.invitees.validator),
    ('cursor', SharedFileMembers.cursor.validator),
SharedFileMetadata.access_type.validator = bv.Nullable(AccessLevel_validator)
SharedFileMetadata.id.validator = files.FileId_validator
SharedFileMetadata.expected_link_metadata.validator = bv.Nullable(ExpectedSharedContentLinkMetadata_validator)
SharedFileMetadata.link_metadata.validator = bv.Nullable(SharedContentLinkMetadata_validator)
SharedFileMetadata.name.validator = bv.String()
SharedFileMetadata.owner_display_names.validator = bv.Nullable(bv.List(bv.String()))
SharedFileMetadata.owner_team.validator = bv.Nullable(users.Team_validator)
SharedFileMetadata.parent_shared_folder_id.validator = bv.Nullable(common.SharedFolderId_validator)
SharedFileMetadata.path_display.validator = bv.Nullable(bv.String())
SharedFileMetadata.path_lower.validator = bv.Nullable(bv.String())
SharedFileMetadata.permissions.validator = bv.Nullable(bv.List(FilePermission_validator))
SharedFileMetadata.policy.validator = FolderPolicy_validator
SharedFileMetadata.preview_url.validator = bv.String()
SharedFileMetadata.time_invited.validator = bv.Nullable(common.DropboxTimestamp_validator)
SharedFileMetadata._all_field_names_ = set([
    'expected_link_metadata',
    'owner_display_names',
    'owner_team',
    'policy',
    'time_invited',
SharedFileMetadata._all_fields_ = [
    ('access_type', SharedFileMetadata.access_type.validator),
    ('id', SharedFileMetadata.id.validator),
    ('expected_link_metadata', SharedFileMetadata.expected_link_metadata.validator),
    ('link_metadata', SharedFileMetadata.link_metadata.validator),
    ('name', SharedFileMetadata.name.validator),
    ('owner_display_names', SharedFileMetadata.owner_display_names.validator),
    ('owner_team', SharedFileMetadata.owner_team.validator),
    ('parent_shared_folder_id', SharedFileMetadata.parent_shared_folder_id.validator),
    ('path_display', SharedFileMetadata.path_display.validator),
    ('path_lower', SharedFileMetadata.path_lower.validator),
    ('permissions', SharedFileMetadata.permissions.validator),
    ('policy', SharedFileMetadata.policy.validator),
    ('preview_url', SharedFileMetadata.preview_url.validator),
    ('time_invited', SharedFileMetadata.time_invited.validator),
SharedFolderAccessError._invalid_id_validator = bv.Void()
SharedFolderAccessError._not_a_member_validator = bv.Void()
SharedFolderAccessError._invalid_member_validator = bv.Void()
SharedFolderAccessError._email_unverified_validator = bv.Void()
SharedFolderAccessError._unmounted_validator = bv.Void()
SharedFolderAccessError._other_validator = bv.Void()
SharedFolderAccessError._tagmap = {
    'invalid_id': SharedFolderAccessError._invalid_id_validator,
    'not_a_member': SharedFolderAccessError._not_a_member_validator,
    'invalid_member': SharedFolderAccessError._invalid_member_validator,
    'email_unverified': SharedFolderAccessError._email_unverified_validator,
    'unmounted': SharedFolderAccessError._unmounted_validator,
    'other': SharedFolderAccessError._other_validator,
SharedFolderAccessError.invalid_id = SharedFolderAccessError('invalid_id')
SharedFolderAccessError.not_a_member = SharedFolderAccessError('not_a_member')
SharedFolderAccessError.invalid_member = SharedFolderAccessError('invalid_member')
SharedFolderAccessError.email_unverified = SharedFolderAccessError('email_unverified')
SharedFolderAccessError.unmounted = SharedFolderAccessError('unmounted')
SharedFolderAccessError.other = SharedFolderAccessError('other')
SharedFolderMemberError._invalid_dropbox_id_validator = bv.Void()
SharedFolderMemberError._not_a_member_validator = bv.Void()
SharedFolderMemberError._no_explicit_access_validator = MemberAccessLevelResult_validator
SharedFolderMemberError._other_validator = bv.Void()
SharedFolderMemberError._tagmap = {
    'invalid_dropbox_id': SharedFolderMemberError._invalid_dropbox_id_validator,
    'not_a_member': SharedFolderMemberError._not_a_member_validator,
    'no_explicit_access': SharedFolderMemberError._no_explicit_access_validator,
    'other': SharedFolderMemberError._other_validator,
SharedFolderMemberError.invalid_dropbox_id = SharedFolderMemberError('invalid_dropbox_id')
SharedFolderMemberError.not_a_member = SharedFolderMemberError('not_a_member')
SharedFolderMemberError.other = SharedFolderMemberError('other')
SharedFolderMembers.users.validator = bv.List(UserMembershipInfo_validator)
SharedFolderMembers.groups.validator = bv.List(GroupMembershipInfo_validator)
SharedFolderMembers.invitees.validator = bv.List(InviteeMembershipInfo_validator)
SharedFolderMembers.cursor.validator = bv.Nullable(bv.String())
SharedFolderMembers._all_field_names_ = set([
SharedFolderMembers._all_fields_ = [
    ('users', SharedFolderMembers.users.validator),
    ('groups', SharedFolderMembers.groups.validator),
    ('invitees', SharedFolderMembers.invitees.validator),
    ('cursor', SharedFolderMembers.cursor.validator),
SharedFolderMetadataBase.access_type.validator = AccessLevel_validator
SharedFolderMetadataBase.is_inside_team_folder.validator = bv.Boolean()
SharedFolderMetadataBase.is_team_folder.validator = bv.Boolean()
SharedFolderMetadataBase.owner_display_names.validator = bv.Nullable(bv.List(bv.String()))
SharedFolderMetadataBase.owner_team.validator = bv.Nullable(users.Team_validator)
SharedFolderMetadataBase.parent_shared_folder_id.validator = bv.Nullable(common.SharedFolderId_validator)
SharedFolderMetadataBase.path_display.validator = bv.Nullable(bv.String())
SharedFolderMetadataBase.path_lower.validator = bv.Nullable(bv.String())
SharedFolderMetadataBase.parent_folder_name.validator = bv.Nullable(bv.String())
SharedFolderMetadataBase._all_field_names_ = set([
    'is_inside_team_folder',
    'parent_folder_name',
SharedFolderMetadataBase._all_fields_ = [
    ('access_type', SharedFolderMetadataBase.access_type.validator),
    ('is_inside_team_folder', SharedFolderMetadataBase.is_inside_team_folder.validator),
    ('is_team_folder', SharedFolderMetadataBase.is_team_folder.validator),
    ('owner_display_names', SharedFolderMetadataBase.owner_display_names.validator),
    ('owner_team', SharedFolderMetadataBase.owner_team.validator),
    ('parent_shared_folder_id', SharedFolderMetadataBase.parent_shared_folder_id.validator),
    ('path_display', SharedFolderMetadataBase.path_display.validator),
    ('path_lower', SharedFolderMetadataBase.path_lower.validator),
    ('parent_folder_name', SharedFolderMetadataBase.parent_folder_name.validator),
SharedFolderMetadata.link_metadata.validator = bv.Nullable(SharedContentLinkMetadata_validator)
SharedFolderMetadata.name.validator = bv.String()
SharedFolderMetadata.permissions.validator = bv.Nullable(bv.List(FolderPermission_validator))
SharedFolderMetadata.policy.validator = FolderPolicy_validator
SharedFolderMetadata.preview_url.validator = bv.String()
SharedFolderMetadata.shared_folder_id.validator = common.SharedFolderId_validator
SharedFolderMetadata.time_invited.validator = common.DropboxTimestamp_validator
SharedFolderMetadata.access_inheritance.validator = AccessInheritance_validator
SharedFolderMetadata._all_field_names_ = SharedFolderMetadataBase._all_field_names_.union(set([
SharedFolderMetadata._all_fields_ = SharedFolderMetadataBase._all_fields_ + [
    ('link_metadata', SharedFolderMetadata.link_metadata.validator),
    ('name', SharedFolderMetadata.name.validator),
    ('permissions', SharedFolderMetadata.permissions.validator),
    ('policy', SharedFolderMetadata.policy.validator),
    ('preview_url', SharedFolderMetadata.preview_url.validator),
    ('shared_folder_id', SharedFolderMetadata.shared_folder_id.validator),
    ('time_invited', SharedFolderMetadata.time_invited.validator),
    ('access_inheritance', SharedFolderMetadata.access_inheritance.validator),
SharedLinkAccessFailureReason._login_required_validator = bv.Void()
SharedLinkAccessFailureReason._email_verify_required_validator = bv.Void()
SharedLinkAccessFailureReason._password_required_validator = bv.Void()
SharedLinkAccessFailureReason._team_only_validator = bv.Void()
SharedLinkAccessFailureReason._owner_only_validator = bv.Void()
SharedLinkAccessFailureReason._other_validator = bv.Void()
SharedLinkAccessFailureReason._tagmap = {
    'login_required': SharedLinkAccessFailureReason._login_required_validator,
    'email_verify_required': SharedLinkAccessFailureReason._email_verify_required_validator,
    'password_required': SharedLinkAccessFailureReason._password_required_validator,
    'team_only': SharedLinkAccessFailureReason._team_only_validator,
    'owner_only': SharedLinkAccessFailureReason._owner_only_validator,
    'other': SharedLinkAccessFailureReason._other_validator,
SharedLinkAccessFailureReason.login_required = SharedLinkAccessFailureReason('login_required')
SharedLinkAccessFailureReason.email_verify_required = SharedLinkAccessFailureReason('email_verify_required')
SharedLinkAccessFailureReason.password_required = SharedLinkAccessFailureReason('password_required')
SharedLinkAccessFailureReason.team_only = SharedLinkAccessFailureReason('team_only')
SharedLinkAccessFailureReason.owner_only = SharedLinkAccessFailureReason('owner_only')
SharedLinkAccessFailureReason.other = SharedLinkAccessFailureReason('other')
SharedLinkAlreadyExistsMetadata._metadata_validator = SharedLinkMetadata_validator
SharedLinkAlreadyExistsMetadata._other_validator = bv.Void()
SharedLinkAlreadyExistsMetadata._tagmap = {
    'metadata': SharedLinkAlreadyExistsMetadata._metadata_validator,
    'other': SharedLinkAlreadyExistsMetadata._other_validator,
SharedLinkAlreadyExistsMetadata.other = SharedLinkAlreadyExistsMetadata('other')
SharedLinkPolicy._anyone_validator = bv.Void()
SharedLinkPolicy._team_validator = bv.Void()
SharedLinkPolicy._members_validator = bv.Void()
SharedLinkPolicy._other_validator = bv.Void()
SharedLinkPolicy._tagmap = {
    'anyone': SharedLinkPolicy._anyone_validator,
    'team': SharedLinkPolicy._team_validator,
    'members': SharedLinkPolicy._members_validator,
    'other': SharedLinkPolicy._other_validator,
SharedLinkPolicy.anyone = SharedLinkPolicy('anyone')
SharedLinkPolicy.team = SharedLinkPolicy('team')
SharedLinkPolicy.members = SharedLinkPolicy('members')
SharedLinkPolicy.other = SharedLinkPolicy('other')
SharedLinkSettings.require_password.validator = bv.Nullable(bv.Boolean())
SharedLinkSettings.link_password.validator = bv.Nullable(bv.String())
SharedLinkSettings.expires.validator = bv.Nullable(common.DropboxTimestamp_validator)
SharedLinkSettings.audience.validator = bv.Nullable(LinkAudience_validator)
SharedLinkSettings.access.validator = bv.Nullable(RequestedLinkAccessLevel_validator)
SharedLinkSettings.requested_visibility.validator = bv.Nullable(RequestedVisibility_validator)
SharedLinkSettings.allow_download.validator = bv.Nullable(bv.Boolean())
SharedLinkSettings._all_field_names_ = set([
    'access',
SharedLinkSettings._all_fields_ = [
    ('require_password', SharedLinkSettings.require_password.validator),
    ('link_password', SharedLinkSettings.link_password.validator),
    ('expires', SharedLinkSettings.expires.validator),
    ('audience', SharedLinkSettings.audience.validator),
    ('access', SharedLinkSettings.access.validator),
    ('requested_visibility', SharedLinkSettings.requested_visibility.validator),
    ('allow_download', SharedLinkSettings.allow_download.validator),
SharedLinkSettingsError._invalid_settings_validator = bv.Void()
SharedLinkSettingsError._not_authorized_validator = bv.Void()
SharedLinkSettingsError._tagmap = {
    'invalid_settings': SharedLinkSettingsError._invalid_settings_validator,
    'not_authorized': SharedLinkSettingsError._not_authorized_validator,
SharedLinkSettingsError.invalid_settings = SharedLinkSettingsError('invalid_settings')
SharedLinkSettingsError.not_authorized = SharedLinkSettingsError('not_authorized')
SharingFileAccessError._no_permission_validator = bv.Void()
SharingFileAccessError._invalid_file_validator = bv.Void()
SharingFileAccessError._is_folder_validator = bv.Void()
SharingFileAccessError._inside_public_folder_validator = bv.Void()
SharingFileAccessError._inside_osx_package_validator = bv.Void()
SharingFileAccessError._other_validator = bv.Void()
SharingFileAccessError._tagmap = {
    'no_permission': SharingFileAccessError._no_permission_validator,
    'invalid_file': SharingFileAccessError._invalid_file_validator,
    'is_folder': SharingFileAccessError._is_folder_validator,
    'inside_public_folder': SharingFileAccessError._inside_public_folder_validator,
    'inside_osx_package': SharingFileAccessError._inside_osx_package_validator,
    'other': SharingFileAccessError._other_validator,
SharingFileAccessError.no_permission = SharingFileAccessError('no_permission')
SharingFileAccessError.invalid_file = SharingFileAccessError('invalid_file')
SharingFileAccessError.is_folder = SharingFileAccessError('is_folder')
SharingFileAccessError.inside_public_folder = SharingFileAccessError('inside_public_folder')
SharingFileAccessError.inside_osx_package = SharingFileAccessError('inside_osx_package')
SharingFileAccessError.other = SharingFileAccessError('other')
SharingUserError._email_unverified_validator = bv.Void()
SharingUserError._other_validator = bv.Void()
SharingUserError._tagmap = {
    'email_unverified': SharingUserError._email_unverified_validator,
    'other': SharingUserError._other_validator,
SharingUserError.email_unverified = SharingUserError('email_unverified')
SharingUserError.other = SharingUserError('other')
TeamMemberInfo.team_info.validator = TeamInfo_validator
TeamMemberInfo.display_name.validator = bv.String()
TeamMemberInfo.member_id.validator = bv.Nullable(bv.String())
TeamMemberInfo._all_field_names_ = set([
    'team_info',
    'display_name',
    'member_id',
TeamMemberInfo._all_fields_ = [
    ('team_info', TeamMemberInfo.team_info.validator),
    ('display_name', TeamMemberInfo.display_name.validator),
    ('member_id', TeamMemberInfo.member_id.validator),
TransferFolderArg.shared_folder_id.validator = common.SharedFolderId_validator
TransferFolderArg.to_dropbox_id.validator = DropboxId_validator
TransferFolderArg._all_field_names_ = set([
    'to_dropbox_id',
TransferFolderArg._all_fields_ = [
    ('shared_folder_id', TransferFolderArg.shared_folder_id.validator),
    ('to_dropbox_id', TransferFolderArg.to_dropbox_id.validator),
TransferFolderError._access_error_validator = SharedFolderAccessError_validator
TransferFolderError._invalid_dropbox_id_validator = bv.Void()
TransferFolderError._new_owner_not_a_member_validator = bv.Void()
TransferFolderError._new_owner_unmounted_validator = bv.Void()
TransferFolderError._new_owner_email_unverified_validator = bv.Void()
TransferFolderError._team_folder_validator = bv.Void()
TransferFolderError._no_permission_validator = bv.Void()
TransferFolderError._other_validator = bv.Void()
TransferFolderError._tagmap = {
    'access_error': TransferFolderError._access_error_validator,
    'invalid_dropbox_id': TransferFolderError._invalid_dropbox_id_validator,
    'new_owner_not_a_member': TransferFolderError._new_owner_not_a_member_validator,
    'new_owner_unmounted': TransferFolderError._new_owner_unmounted_validator,
    'new_owner_email_unverified': TransferFolderError._new_owner_email_unverified_validator,
    'team_folder': TransferFolderError._team_folder_validator,
    'no_permission': TransferFolderError._no_permission_validator,
    'other': TransferFolderError._other_validator,
TransferFolderError.invalid_dropbox_id = TransferFolderError('invalid_dropbox_id')
TransferFolderError.new_owner_not_a_member = TransferFolderError('new_owner_not_a_member')
TransferFolderError.new_owner_unmounted = TransferFolderError('new_owner_unmounted')
TransferFolderError.new_owner_email_unverified = TransferFolderError('new_owner_email_unverified')
TransferFolderError.team_folder = TransferFolderError('team_folder')
TransferFolderError.no_permission = TransferFolderError('no_permission')
TransferFolderError.other = TransferFolderError('other')
UnmountFolderArg.shared_folder_id.validator = common.SharedFolderId_validator
UnmountFolderArg._all_field_names_ = set(['shared_folder_id'])
UnmountFolderArg._all_fields_ = [('shared_folder_id', UnmountFolderArg.shared_folder_id.validator)]
UnmountFolderError._access_error_validator = SharedFolderAccessError_validator
UnmountFolderError._no_permission_validator = bv.Void()
UnmountFolderError._not_unmountable_validator = bv.Void()
UnmountFolderError._other_validator = bv.Void()
UnmountFolderError._tagmap = {
    'access_error': UnmountFolderError._access_error_validator,
    'no_permission': UnmountFolderError._no_permission_validator,
    'not_unmountable': UnmountFolderError._not_unmountable_validator,
    'other': UnmountFolderError._other_validator,
UnmountFolderError.no_permission = UnmountFolderError('no_permission')
UnmountFolderError.not_unmountable = UnmountFolderError('not_unmountable')
UnmountFolderError.other = UnmountFolderError('other')
UnshareFileArg.file.validator = PathOrId_validator
UnshareFileArg._all_field_names_ = set(['file'])
UnshareFileArg._all_fields_ = [('file', UnshareFileArg.file.validator)]
UnshareFileError._user_error_validator = SharingUserError_validator
UnshareFileError._access_error_validator = SharingFileAccessError_validator
UnshareFileError._other_validator = bv.Void()
UnshareFileError._tagmap = {
    'user_error': UnshareFileError._user_error_validator,
    'access_error': UnshareFileError._access_error_validator,
    'other': UnshareFileError._other_validator,
UnshareFileError.other = UnshareFileError('other')
UnshareFolderArg.shared_folder_id.validator = common.SharedFolderId_validator
UnshareFolderArg.leave_a_copy.validator = bv.Boolean()
UnshareFolderArg._all_field_names_ = set([
UnshareFolderArg._all_fields_ = [
    ('shared_folder_id', UnshareFolderArg.shared_folder_id.validator),
    ('leave_a_copy', UnshareFolderArg.leave_a_copy.validator),
UnshareFolderError._access_error_validator = SharedFolderAccessError_validator
UnshareFolderError._team_folder_validator = bv.Void()
UnshareFolderError._no_permission_validator = bv.Void()
UnshareFolderError._too_many_files_validator = bv.Void()
UnshareFolderError._other_validator = bv.Void()
UnshareFolderError._tagmap = {
    'access_error': UnshareFolderError._access_error_validator,
    'team_folder': UnshareFolderError._team_folder_validator,
    'no_permission': UnshareFolderError._no_permission_validator,
    'too_many_files': UnshareFolderError._too_many_files_validator,
    'other': UnshareFolderError._other_validator,
UnshareFolderError.team_folder = UnshareFolderError('team_folder')
UnshareFolderError.no_permission = UnshareFolderError('no_permission')
UnshareFolderError.too_many_files = UnshareFolderError('too_many_files')
UnshareFolderError.other = UnshareFolderError('other')
UpdateFileMemberArgs.file.validator = PathOrId_validator
UpdateFileMemberArgs.member.validator = MemberSelector_validator
UpdateFileMemberArgs.access_level.validator = AccessLevel_validator
UpdateFileMemberArgs._all_field_names_ = set([
UpdateFileMemberArgs._all_fields_ = [
    ('file', UpdateFileMemberArgs.file.validator),
    ('member', UpdateFileMemberArgs.member.validator),
    ('access_level', UpdateFileMemberArgs.access_level.validator),
UpdateFolderMemberArg.shared_folder_id.validator = common.SharedFolderId_validator
UpdateFolderMemberArg.member.validator = MemberSelector_validator
UpdateFolderMemberArg.access_level.validator = AccessLevel_validator
UpdateFolderMemberArg._all_field_names_ = set([
UpdateFolderMemberArg._all_fields_ = [
    ('shared_folder_id', UpdateFolderMemberArg.shared_folder_id.validator),
    ('member', UpdateFolderMemberArg.member.validator),
    ('access_level', UpdateFolderMemberArg.access_level.validator),
UpdateFolderMemberError._access_error_validator = SharedFolderAccessError_validator
UpdateFolderMemberError._member_error_validator = SharedFolderMemberError_validator
UpdateFolderMemberError._no_explicit_access_validator = AddFolderMemberError_validator
UpdateFolderMemberError._insufficient_plan_validator = bv.Void()
UpdateFolderMemberError._no_permission_validator = bv.Void()
UpdateFolderMemberError._other_validator = bv.Void()
UpdateFolderMemberError._tagmap = {
    'access_error': UpdateFolderMemberError._access_error_validator,
    'member_error': UpdateFolderMemberError._member_error_validator,
    'no_explicit_access': UpdateFolderMemberError._no_explicit_access_validator,
    'insufficient_plan': UpdateFolderMemberError._insufficient_plan_validator,
    'no_permission': UpdateFolderMemberError._no_permission_validator,
    'other': UpdateFolderMemberError._other_validator,
UpdateFolderMemberError.insufficient_plan = UpdateFolderMemberError('insufficient_plan')
UpdateFolderMemberError.no_permission = UpdateFolderMemberError('no_permission')
UpdateFolderMemberError.other = UpdateFolderMemberError('other')
UpdateFolderPolicyArg.shared_folder_id.validator = common.SharedFolderId_validator
UpdateFolderPolicyArg.member_policy.validator = bv.Nullable(MemberPolicy_validator)
UpdateFolderPolicyArg.acl_update_policy.validator = bv.Nullable(AclUpdatePolicy_validator)
UpdateFolderPolicyArg.viewer_info_policy.validator = bv.Nullable(ViewerInfoPolicy_validator)
UpdateFolderPolicyArg.shared_link_policy.validator = bv.Nullable(SharedLinkPolicy_validator)
UpdateFolderPolicyArg.link_settings.validator = bv.Nullable(LinkSettings_validator)
UpdateFolderPolicyArg.actions.validator = bv.Nullable(bv.List(FolderAction_validator))
UpdateFolderPolicyArg._all_field_names_ = set([
UpdateFolderPolicyArg._all_fields_ = [
    ('shared_folder_id', UpdateFolderPolicyArg.shared_folder_id.validator),
    ('member_policy', UpdateFolderPolicyArg.member_policy.validator),
    ('acl_update_policy', UpdateFolderPolicyArg.acl_update_policy.validator),
    ('viewer_info_policy', UpdateFolderPolicyArg.viewer_info_policy.validator),
    ('shared_link_policy', UpdateFolderPolicyArg.shared_link_policy.validator),
    ('link_settings', UpdateFolderPolicyArg.link_settings.validator),
    ('actions', UpdateFolderPolicyArg.actions.validator),
UpdateFolderPolicyError._access_error_validator = SharedFolderAccessError_validator
UpdateFolderPolicyError._not_on_team_validator = bv.Void()
UpdateFolderPolicyError._team_policy_disallows_member_policy_validator = bv.Void()
UpdateFolderPolicyError._disallowed_shared_link_policy_validator = bv.Void()
UpdateFolderPolicyError._no_permission_validator = bv.Void()
UpdateFolderPolicyError._team_folder_validator = bv.Void()
UpdateFolderPolicyError._other_validator = bv.Void()
UpdateFolderPolicyError._tagmap = {
    'access_error': UpdateFolderPolicyError._access_error_validator,
    'not_on_team': UpdateFolderPolicyError._not_on_team_validator,
    'team_policy_disallows_member_policy': UpdateFolderPolicyError._team_policy_disallows_member_policy_validator,
    'disallowed_shared_link_policy': UpdateFolderPolicyError._disallowed_shared_link_policy_validator,
    'no_permission': UpdateFolderPolicyError._no_permission_validator,
    'team_folder': UpdateFolderPolicyError._team_folder_validator,
    'other': UpdateFolderPolicyError._other_validator,
UpdateFolderPolicyError.not_on_team = UpdateFolderPolicyError('not_on_team')
UpdateFolderPolicyError.team_policy_disallows_member_policy = UpdateFolderPolicyError('team_policy_disallows_member_policy')
UpdateFolderPolicyError.disallowed_shared_link_policy = UpdateFolderPolicyError('disallowed_shared_link_policy')
UpdateFolderPolicyError.no_permission = UpdateFolderPolicyError('no_permission')
UpdateFolderPolicyError.team_folder = UpdateFolderPolicyError('team_folder')
UpdateFolderPolicyError.other = UpdateFolderPolicyError('other')
UserMembershipInfo.user.validator = UserInfo_validator
UserMembershipInfo._all_field_names_ = MembershipInfo._all_field_names_.union(set(['user']))
UserMembershipInfo._all_fields_ = MembershipInfo._all_fields_ + [('user', UserMembershipInfo.user.validator)]
UserFileMembershipInfo.time_last_seen.validator = bv.Nullable(common.DropboxTimestamp_validator)
UserFileMembershipInfo.platform_type.validator = bv.Nullable(seen_state.PlatformType_validator)
UserFileMembershipInfo._all_field_names_ = UserMembershipInfo._all_field_names_.union(set([
    'time_last_seen',
    'platform_type',
UserFileMembershipInfo._all_fields_ = UserMembershipInfo._all_fields_ + [
    ('time_last_seen', UserFileMembershipInfo.time_last_seen.validator),
    ('platform_type', UserFileMembershipInfo.platform_type.validator),
UserInfo.account_id.validator = users_common.AccountId_validator
UserInfo.email.validator = bv.String()
UserInfo.display_name.validator = bv.String()
UserInfo.same_team.validator = bv.Boolean()
UserInfo.team_member_id.validator = bv.Nullable(bv.String())
UserInfo._all_field_names_ = set([
    'team_member_id',
UserInfo._all_fields_ = [
    ('account_id', UserInfo.account_id.validator),
    ('email', UserInfo.email.validator),
    ('display_name', UserInfo.display_name.validator),
    ('same_team', UserInfo.same_team.validator),
    ('team_member_id', UserInfo.team_member_id.validator),
ViewerInfoPolicy._enabled_validator = bv.Void()
ViewerInfoPolicy._disabled_validator = bv.Void()
ViewerInfoPolicy._other_validator = bv.Void()
ViewerInfoPolicy._tagmap = {
    'enabled': ViewerInfoPolicy._enabled_validator,
    'disabled': ViewerInfoPolicy._disabled_validator,
    'other': ViewerInfoPolicy._other_validator,
ViewerInfoPolicy.enabled = ViewerInfoPolicy('enabled')
ViewerInfoPolicy.disabled = ViewerInfoPolicy('disabled')
ViewerInfoPolicy.other = ViewerInfoPolicy('other')
Visibility._public_validator = bv.Void()
Visibility._team_only_validator = bv.Void()
Visibility._password_validator = bv.Void()
Visibility._team_and_password_validator = bv.Void()
Visibility._shared_folder_only_validator = bv.Void()
Visibility._other_validator = bv.Void()
Visibility._tagmap = {
    'public': Visibility._public_validator,
    'team_only': Visibility._team_only_validator,
    'password': Visibility._password_validator,
    'team_and_password': Visibility._team_and_password_validator,
    'shared_folder_only': Visibility._shared_folder_only_validator,
    'other': Visibility._other_validator,
Visibility.public = Visibility('public')
Visibility.team_only = Visibility('team_only')
Visibility.password = Visibility('password')
Visibility.team_and_password = Visibility('team_and_password')
Visibility.shared_folder_only = Visibility('shared_folder_only')
Visibility.other = Visibility('other')
VisibilityPolicy.policy.validator = RequestedVisibility_validator
VisibilityPolicy.resolved_policy.validator = AlphaResolvedVisibility_validator
VisibilityPolicy.allowed.validator = bv.Boolean()
VisibilityPolicy.disallowed_reason.validator = bv.Nullable(VisibilityPolicyDisallowedReason_validator)
VisibilityPolicy._all_field_names_ = set([
    'resolved_policy',
VisibilityPolicy._all_fields_ = [
    ('policy', VisibilityPolicy.policy.validator),
    ('resolved_policy', VisibilityPolicy.resolved_policy.validator),
    ('allowed', VisibilityPolicy.allowed.validator),
    ('disallowed_reason', VisibilityPolicy.disallowed_reason.validator),
AddFileMemberArgs.quiet.default = False
AddFileMemberArgs.access_level.default = AccessLevel.viewer
AddFileMemberArgs.add_message_as_comment.default = False
AddFolderMemberArg.quiet.default = False
AddMember.access_level.default = AccessLevel.viewer
CreateSharedLinkArg.short_url.default = False
MembershipInfo.is_inherited.default = False
ListFileMembersArg.include_inherited.default = True
ListFileMembersArg.limit.default = 100
ListFileMembersBatchArg.limit.default = 10
ListFilesArg.limit.default = 100
ListFolderMembersCursorArg.limit.default = 1000
ListFoldersArgs.limit.default = 1000
ModifySharedLinkSettingsArgs.remove_expiration.default = False
RelinquishFolderMembershipArg.leave_a_copy.default = False
SetAccessInheritanceArg.access_inheritance.default = AccessInheritance.inherit
ShareFolderArgBase.force_async.default = False
ShareFolderArgBase.access_inheritance.default = AccessInheritance.inherit
SharedFolderMetadata.access_inheritance.default = AccessInheritance.inherit
UnshareFolderArg.leave_a_copy.default = False
add_file_member = bb.Route(
    'add_file_member',
    AddFileMemberArgs_validator,
    bv.List(FileMemberActionResult_validator),
    AddFileMemberError_validator,
add_folder_member = bb.Route(
    'add_folder_member',
    AddFolderMemberArg_validator,
    AddFolderMemberError_validator,
check_job_status = bb.Route(
    'check_job_status',
    JobStatus_validator,
check_remove_member_job_status = bb.Route(
    'check_remove_member_job_status',
    RemoveMemberJobStatus_validator,
check_share_job_status = bb.Route(
    'check_share_job_status',
    ShareFolderJobStatus_validator,
create_shared_link = bb.Route(
    'create_shared_link',
    CreateSharedLinkArg_validator,
    PathLinkMetadata_validator,
    CreateSharedLinkError_validator,
create_shared_link_with_settings = bb.Route(
    'create_shared_link_with_settings',
    CreateSharedLinkWithSettingsArg_validator,
    SharedLinkMetadata_validator,
    CreateSharedLinkWithSettingsError_validator,
get_file_metadata = bb.Route(
    'get_file_metadata',
    GetFileMetadataArg_validator,
    SharedFileMetadata_validator,
    GetFileMetadataError_validator,
get_file_metadata_batch = bb.Route(
    'get_file_metadata/batch',
    GetFileMetadataBatchArg_validator,
    bv.List(GetFileMetadataBatchResult_validator),
    SharingUserError_validator,
get_folder_metadata = bb.Route(
    'get_folder_metadata',
    GetMetadataArgs_validator,
    SharedFolderMetadata_validator,
    SharedFolderAccessError_validator,
get_shared_link_file = bb.Route(
    'get_shared_link_file',
    GetSharedLinkFileArg_validator,
    GetSharedLinkFileError_validator,
get_shared_link_metadata = bb.Route(
    'get_shared_link_metadata',
    GetSharedLinkMetadataArg_validator,
    SharedLinkError_validator,
get_shared_links = bb.Route(
    'get_shared_links',
    GetSharedLinksArg_validator,
    GetSharedLinksResult_validator,
    GetSharedLinksError_validator,
list_file_members = bb.Route(
    'list_file_members',
    ListFileMembersArg_validator,
    SharedFileMembers_validator,
    ListFileMembersError_validator,
list_file_members_batch = bb.Route(
    'list_file_members/batch',
    ListFileMembersBatchArg_validator,
    bv.List(ListFileMembersBatchResult_validator),
list_file_members_continue = bb.Route(
    'list_file_members/continue',
    ListFileMembersContinueArg_validator,
    ListFileMembersContinueError_validator,
list_folder_members = bb.Route(
    'list_folder_members',
    ListFolderMembersArgs_validator,
    SharedFolderMembers_validator,
list_folder_members_continue = bb.Route(
    'list_folder_members/continue',
    ListFolderMembersContinueArg_validator,
    ListFolderMembersContinueError_validator,
list_folders = bb.Route(
    'list_folders',
    ListFoldersArgs_validator,
    ListFoldersResult_validator,
list_folders_continue = bb.Route(
    'list_folders/continue',
    ListFoldersContinueArg_validator,
    ListFoldersContinueError_validator,
list_mountable_folders = bb.Route(
    'list_mountable_folders',
list_mountable_folders_continue = bb.Route(
    'list_mountable_folders/continue',
list_received_files = bb.Route(
    'list_received_files',
    ListFilesArg_validator,
    ListFilesResult_validator,
list_received_files_continue = bb.Route(
    'list_received_files/continue',
    ListFilesContinueArg_validator,
    ListFilesContinueError_validator,
list_shared_links = bb.Route(
    'list_shared_links',
    ListSharedLinksArg_validator,
    ListSharedLinksResult_validator,
    ListSharedLinksError_validator,
modify_shared_link_settings = bb.Route(
    'modify_shared_link_settings',
    ModifySharedLinkSettingsArgs_validator,
    ModifySharedLinkSettingsError_validator,
mount_folder = bb.Route(
    'mount_folder',
    MountFolderArg_validator,
    MountFolderError_validator,
relinquish_file_membership = bb.Route(
    'relinquish_file_membership',
    RelinquishFileMembershipArg_validator,
    RelinquishFileMembershipError_validator,
relinquish_folder_membership = bb.Route(
    'relinquish_folder_membership',
    RelinquishFolderMembershipArg_validator,
    async_.LaunchEmptyResult_validator,
    RelinquishFolderMembershipError_validator,
remove_file_member = bb.Route(
    'remove_file_member',
    RemoveFileMemberArg_validator,
    FileMemberActionIndividualResult_validator,
    RemoveFileMemberError_validator,
remove_file_member_2 = bb.Route(
    'remove_file_member_2',
    FileMemberRemoveActionResult_validator,
remove_folder_member = bb.Route(
    'remove_folder_member',
    RemoveFolderMemberArg_validator,
    async_.LaunchResultBase_validator,
    RemoveFolderMemberError_validator,
revoke_shared_link = bb.Route(
    'revoke_shared_link',
    RevokeSharedLinkArg_validator,
    RevokeSharedLinkError_validator,
set_access_inheritance = bb.Route(
    'set_access_inheritance',
    SetAccessInheritanceArg_validator,
    ShareFolderLaunch_validator,
    SetAccessInheritanceError_validator,
share_folder = bb.Route(
    'share_folder',
    ShareFolderArg_validator,
    ShareFolderError_validator,
transfer_folder = bb.Route(
    'transfer_folder',
    TransferFolderArg_validator,
    TransferFolderError_validator,
unmount_folder = bb.Route(
    'unmount_folder',
    UnmountFolderArg_validator,
    UnmountFolderError_validator,
unshare_file = bb.Route(
    'unshare_file',
    UnshareFileArg_validator,
    UnshareFileError_validator,
unshare_folder = bb.Route(
    'unshare_folder',
    UnshareFolderArg_validator,
    UnshareFolderError_validator,
update_file_member = bb.Route(
    'update_file_member',
    UpdateFileMemberArgs_validator,
    MemberAccessLevelResult_validator,
    FileMemberActionError_validator,
update_folder_member = bb.Route(
    'update_folder_member',
    UpdateFolderMemberArg_validator,
    UpdateFolderMemberError_validator,
update_folder_policy = bb.Route(
    'update_folder_policy',
    UpdateFolderPolicyArg_validator,
    UpdateFolderPolicyError_validator,
    'add_file_member': add_file_member,
    'add_folder_member': add_folder_member,
    'check_job_status': check_job_status,
    'check_remove_member_job_status': check_remove_member_job_status,
    'check_share_job_status': check_share_job_status,
    'create_shared_link': create_shared_link,
    'create_shared_link_with_settings': create_shared_link_with_settings,
    'get_file_metadata': get_file_metadata,
    'get_file_metadata/batch': get_file_metadata_batch,
    'get_folder_metadata': get_folder_metadata,
    'get_shared_link_file': get_shared_link_file,
    'get_shared_link_metadata': get_shared_link_metadata,
    'get_shared_links': get_shared_links,
    'list_file_members': list_file_members,
    'list_file_members/batch': list_file_members_batch,
    'list_file_members/continue': list_file_members_continue,
    'list_folder_members': list_folder_members,
    'list_folder_members/continue': list_folder_members_continue,
    'list_folders': list_folders,
    'list_folders/continue': list_folders_continue,
    'list_mountable_folders': list_mountable_folders,
    'list_mountable_folders/continue': list_mountable_folders_continue,
    'list_received_files': list_received_files,
    'list_received_files/continue': list_received_files_continue,
    'list_shared_links': list_shared_links,
    'modify_shared_link_settings': modify_shared_link_settings,
    'mount_folder': mount_folder,
    'relinquish_file_membership': relinquish_file_membership,
    'relinquish_folder_membership': relinquish_folder_membership,
    'remove_file_member': remove_file_member,
    'remove_file_member_2': remove_file_member_2,
    'remove_folder_member': remove_folder_member,
    'revoke_shared_link': revoke_shared_link,
    'set_access_inheritance': set_access_inheritance,
    'share_folder': share_folder,
    'transfer_folder': transfer_folder,
    'unmount_folder': unmount_folder,
    'unshare_file': unshare_file,
    'unshare_folder': unshare_folder,
    'update_file_member': update_file_member,
    'update_folder_member': update_folder_member,
    'update_folder_policy': update_folder_policy,
