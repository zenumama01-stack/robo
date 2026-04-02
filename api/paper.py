This namespace contains endpoints and data types for managing docs and folders in Dropbox Paper.
New Paper users will see docs they create in their filesystem as '.paper' files alongside their other Dropbox content. The /paper endpoints are being deprecated and you'll need to use /files and /sharing endpoints to interact with their Paper content. Read more in the `Paper Migration Guide <https://www.dropbox.com/lp/developers/reference/paper-migration-guide>`_.
class AddMember(bb.Struct):
    :ivar paper.AddMember.permission_level: Permission for the user.
    :ivar paper.AddMember.member: User which should be added to the Paper doc.
        Specify only email address or Dropbox account ID.
        '_permission_level_value',
        '_member_value',
                 member=None,
                 permission_level=None):
        self._permission_level_value = bb.NOT_SET
        self._member_value = bb.NOT_SET
        if permission_level is not None:
            self.permission_level = permission_level
        if member is not None:
            self.member = member
    # Instance attribute type: PaperDocPermissionLevel (validator is set below)
    permission_level = bb.Attribute("permission_level", user_defined=True)
    # Instance attribute type: sharing.MemberSelector (validator is set below)
    member = bb.Attribute("member", user_defined=True)
        super(AddMember, self)._process_custom_annotations(annotation_type, field_path, processor)
AddMember_validator = bv.Struct(AddMember)
class RefPaperDoc(bb.Struct):
    :ivar paper.RefPaperDoc.doc_id: The Paper doc ID.
        '_doc_id_value',
                 doc_id=None):
        self._doc_id_value = bb.NOT_SET
        if doc_id is not None:
            self.doc_id = doc_id
    doc_id = bb.Attribute("doc_id")
        super(RefPaperDoc, self)._process_custom_annotations(annotation_type, field_path, processor)
RefPaperDoc_validator = bv.Struct(RefPaperDoc)
class AddPaperDocUser(RefPaperDoc):
    :ivar paper.AddPaperDocUser.members: User which should be added to the Paper
        doc. Specify only email address or Dropbox account ID.
    :ivar paper.AddPaperDocUser.custom_message: A personal message that will be
    :ivar paper.AddPaperDocUser.quiet: Clients should set this to true if no
        email message shall be sent to added users.
        '_members_value',
        '_custom_message_value',
        '_quiet_value',
                 doc_id=None,
                 members=None,
                 quiet=None):
        super(AddPaperDocUser, self).__init__(doc_id)
        self._members_value = bb.NOT_SET
        self._custom_message_value = bb.NOT_SET
        self._quiet_value = bb.NOT_SET
        if members is not None:
            self.members = members
        if custom_message is not None:
            self.custom_message = custom_message
        if quiet is not None:
    # Instance attribute type: list of [AddMember] (validator is set below)
    members = bb.Attribute("members")
    custom_message = bb.Attribute("custom_message", nullable=True)
    quiet = bb.Attribute("quiet")
        super(AddPaperDocUser, self)._process_custom_annotations(annotation_type, field_path, processor)
AddPaperDocUser_validator = bv.Struct(AddPaperDocUser)
class AddPaperDocUserMemberResult(bb.Struct):
    Per-member result for
    :meth:`dropbox.dropbox_client.Dropbox.paper_docs_users_add`.
    :ivar paper.AddPaperDocUserMemberResult.member: One of specified input
    :ivar paper.AddPaperDocUserMemberResult.result: The outcome of the action on
        this member.
    # Instance attribute type: AddPaperDocUserResult (validator is set below)
    result = bb.Attribute("result", user_defined=True)
        super(AddPaperDocUserMemberResult, self)._process_custom_annotations(annotation_type, field_path, processor)
AddPaperDocUserMemberResult_validator = bv.Struct(AddPaperDocUserMemberResult)
class AddPaperDocUserResult(bb.Union):
    :ivar paper.AddPaperDocUserResult.success: User was successfully added to
        the Paper doc.
    :ivar paper.AddPaperDocUserResult.unknown_error: Something unexpected
        happened when trying to add the user to the Paper doc.
    :ivar paper.AddPaperDocUserResult.sharing_outside_team_disabled: The Paper
        doc can be shared only with team members.
    :ivar paper.AddPaperDocUserResult.daily_limit_reached: The daily limit of
        how many users can be added to the Paper doc was reached.
    :ivar paper.AddPaperDocUserResult.user_is_owner: Owner's permissions cannot
        be changed.
    :ivar paper.AddPaperDocUserResult.failed_user_data_retrieval: User data
        could not be retrieved. Clients should retry.
    :ivar paper.AddPaperDocUserResult.permission_already_granted: This user
        already has the correct permission to the Paper doc.
    success = None
    unknown_error = None
    sharing_outside_team_disabled = None
    daily_limit_reached = None
    user_is_owner = None
    failed_user_data_retrieval = None
    permission_already_granted = None
    def is_unknown_error(self):
        Check if the union tag is ``unknown_error``.
        return self._tag == 'unknown_error'
    def is_sharing_outside_team_disabled(self):
        Check if the union tag is ``sharing_outside_team_disabled``.
        return self._tag == 'sharing_outside_team_disabled'
    def is_daily_limit_reached(self):
        Check if the union tag is ``daily_limit_reached``.
        return self._tag == 'daily_limit_reached'
    def is_user_is_owner(self):
        Check if the union tag is ``user_is_owner``.
        return self._tag == 'user_is_owner'
    def is_failed_user_data_retrieval(self):
        Check if the union tag is ``failed_user_data_retrieval``.
        return self._tag == 'failed_user_data_retrieval'
    def is_permission_already_granted(self):
        Check if the union tag is ``permission_already_granted``.
        return self._tag == 'permission_already_granted'
        super(AddPaperDocUserResult, self)._process_custom_annotations(annotation_type, field_path, processor)
AddPaperDocUserResult_validator = bv.Union(AddPaperDocUserResult)
class Cursor(bb.Struct):
    :ivar paper.Cursor.value: The actual cursor value.
    :ivar paper.Cursor.expiration: Expiration time of ``value``. Some cursors
        might have expiration time assigned. This is a UTC value after which the
        cursor is no longer valid and the API starts returning an error. If
        cursor expires a new one needs to be obtained and pagination needs to be
        restarted. Some cursors might be short-lived some cursors might be
        long-lived. This really depends on the sorting type and order, e.g.: 1.
        on one hand, listing docs created by the user, sorted by the created
        time ascending will have undefinite expiration because the results
        cannot change while the iteration is happening. This cursor would be
        suitable for long term polling. 2. on the other hand, listing docs
        sorted by the last modified time will have a very short expiration as
        docs do get modified very often and the modified time can be changed
        while the iteration is happening thus altering the results.
        '_expiration_value',
                 value=None,
                 expiration=None):
        self._expiration_value = bb.NOT_SET
        if expiration is not None:
    expiration = bb.Attribute("expiration", nullable=True)
        super(Cursor, self)._process_custom_annotations(annotation_type, field_path, processor)
Cursor_validator = bv.Struct(Cursor)
class PaperApiBaseError(bb.Union):
    :ivar paper.PaperApiBaseError.insufficient_permissions: Your account does
        not have permissions to perform this action. This may be due to it only
        having access to Paper as files in the Dropbox filesystem. For more
        information, refer to the `Paper Migration Guide
        <https://www.dropbox.com/lp/developers/reference/paper-migration-guide>`_.
        super(PaperApiBaseError, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperApiBaseError_validator = bv.Union(PaperApiBaseError)
class DocLookupError(PaperApiBaseError):
    :ivar paper.DocLookupError.doc_not_found: The required doc was not found.
    doc_not_found = None
    def is_doc_not_found(self):
        Check if the union tag is ``doc_not_found``.
        return self._tag == 'doc_not_found'
        super(DocLookupError, self)._process_custom_annotations(annotation_type, field_path, processor)
DocLookupError_validator = bv.Union(DocLookupError)
class DocSubscriptionLevel(bb.Union):
    The subscription level of a Paper doc.
    :ivar paper.DocSubscriptionLevel.default: No change email messages unless
        you're the creator.
    :ivar paper.DocSubscriptionLevel.ignore: Ignored: Not shown in pad lists or
        activity and no email message is sent.
    :ivar paper.DocSubscriptionLevel.every: Subscribed: Shown in pad lists and
        activity and change email messages are sent.
    :ivar paper.DocSubscriptionLevel.no_email: Unsubscribed: Shown in pad lists,
        but not in activity and no change email messages are sent.
    ignore = None
    every = None
    no_email = None
    def is_ignore(self):
        Check if the union tag is ``ignore``.
        return self._tag == 'ignore'
    def is_every(self):
        Check if the union tag is ``every``.
        return self._tag == 'every'
    def is_no_email(self):
        Check if the union tag is ``no_email``.
        return self._tag == 'no_email'
        super(DocSubscriptionLevel, self)._process_custom_annotations(annotation_type, field_path, processor)
DocSubscriptionLevel_validator = bv.Union(DocSubscriptionLevel)
class ExportFormat(bb.Union):
    The desired export format of the Paper doc.
    :ivar paper.ExportFormat.html: The HTML export format.
    :ivar paper.ExportFormat.markdown: The markdown export format.
        super(ExportFormat, self)._process_custom_annotations(annotation_type, field_path, processor)
ExportFormat_validator = bv.Union(ExportFormat)
class Folder(bb.Struct):
    Data structure representing a Paper folder.
    :ivar paper.Folder.id: Paper folder ID. This ID uniquely identifies the
    :ivar paper.Folder.name: Paper folder name.
                 name=None):
        super(Folder, self)._process_custom_annotations(annotation_type, field_path, processor)
Folder_validator = bv.Struct(Folder)
class FolderSharingPolicyType(bb.Union):
    The sharing policy of a Paper folder. The sharing policy of subfolders is
    inherited from the root folder.
    :ivar paper.FolderSharingPolicyType.team: Everyone in your team and anyone
        directly invited can access this folder.
    :ivar paper.FolderSharingPolicyType.invite_only: Only people directly
        invited can access this folder.
    invite_only = None
    def is_invite_only(self):
        Check if the union tag is ``invite_only``.
        return self._tag == 'invite_only'
        super(FolderSharingPolicyType, self)._process_custom_annotations(annotation_type, field_path, processor)
FolderSharingPolicyType_validator = bv.Union(FolderSharingPolicyType)
class FolderSubscriptionLevel(bb.Union):
    The subscription level of a Paper folder.
    :ivar paper.FolderSubscriptionLevel.none: Not shown in activity, no email
        messages.
    :ivar paper.FolderSubscriptionLevel.activity_only: Shown in activity, no
        email messages.
    :ivar paper.FolderSubscriptionLevel.daily_emails: Shown in activity, daily
    :ivar paper.FolderSubscriptionLevel.weekly_emails: Shown in activity, weekly
    none = None
    activity_only = None
    daily_emails = None
    weekly_emails = None
    def is_none(self):
        Check if the union tag is ``none``.
        return self._tag == 'none'
    def is_activity_only(self):
        Check if the union tag is ``activity_only``.
        return self._tag == 'activity_only'
    def is_daily_emails(self):
        Check if the union tag is ``daily_emails``.
        return self._tag == 'daily_emails'
    def is_weekly_emails(self):
        Check if the union tag is ``weekly_emails``.
        return self._tag == 'weekly_emails'
        super(FolderSubscriptionLevel, self)._process_custom_annotations(annotation_type, field_path, processor)
FolderSubscriptionLevel_validator = bv.Union(FolderSubscriptionLevel)
class FoldersContainingPaperDoc(bb.Struct):
    Metadata about Paper folders containing the specififed Paper doc.
    :ivar paper.FoldersContainingPaperDoc.folder_sharing_policy_type: The
        sharing policy of the folder containing the Paper doc.
    :ivar paper.FoldersContainingPaperDoc.folders: The folder path. If present
        the first folder is the root folder.
        '_folder_sharing_policy_type_value',
        '_folders_value',
                 folder_sharing_policy_type=None,
                 folders=None):
        self._folder_sharing_policy_type_value = bb.NOT_SET
        self._folders_value = bb.NOT_SET
        if folder_sharing_policy_type is not None:
            self.folder_sharing_policy_type = folder_sharing_policy_type
        if folders is not None:
            self.folders = folders
    # Instance attribute type: FolderSharingPolicyType (validator is set below)
    folder_sharing_policy_type = bb.Attribute("folder_sharing_policy_type", nullable=True, user_defined=True)
    # Instance attribute type: list of [Folder] (validator is set below)
    folders = bb.Attribute("folders", nullable=True)
        super(FoldersContainingPaperDoc, self)._process_custom_annotations(annotation_type, field_path, processor)
FoldersContainingPaperDoc_validator = bv.Struct(FoldersContainingPaperDoc)
    The import format of the incoming data.
    :ivar paper.ImportFormat.html: The provided data is interpreted as standard
    :ivar paper.ImportFormat.markdown: The provided data is interpreted as
        markdown. The first line of the provided document will be used as the
        doc title.
    :ivar paper.ImportFormat.plain_text: The provided data is interpreted as
        plain text. The first line of the provided document will be used as the
class InviteeInfoWithPermissionLevel(bb.Struct):
    :ivar paper.InviteeInfoWithPermissionLevel.invitee: Email address invited to
    :ivar paper.InviteeInfoWithPermissionLevel.permission_level: Permission
        level for the invitee.
        '_invitee_value',
                 invitee=None,
        self._invitee_value = bb.NOT_SET
        if invitee is not None:
            self.invitee = invitee
    # Instance attribute type: sharing.InviteeInfo (validator is set below)
    invitee = bb.Attribute("invitee", user_defined=True)
        super(InviteeInfoWithPermissionLevel, self)._process_custom_annotations(annotation_type, field_path, processor)
InviteeInfoWithPermissionLevel_validator = bv.Struct(InviteeInfoWithPermissionLevel)
class ListDocsCursorError(bb.Union):
    def cursor_error(cls, val):
        Create an instance of this class set to the ``cursor_error`` tag with
        :param PaperApiCursorError val:
        :rtype: ListDocsCursorError
        return cls('cursor_error', val)
    def is_cursor_error(self):
        Check if the union tag is ``cursor_error``.
        return self._tag == 'cursor_error'
    def get_cursor_error(self):
        Only call this if :meth:`is_cursor_error` is true.
        :rtype: PaperApiCursorError
        if not self.is_cursor_error():
            raise AttributeError("tag 'cursor_error' not set")
        super(ListDocsCursorError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListDocsCursorError_validator = bv.Union(ListDocsCursorError)
class ListPaperDocsArgs(bb.Struct):
    :ivar paper.ListPaperDocsArgs.filter_by: Allows user to specify how the
        Paper docs should be filtered.
    :ivar paper.ListPaperDocsArgs.sort_by: Allows user to specify how the Paper
        docs should be sorted.
    :ivar paper.ListPaperDocsArgs.sort_order: Allows user to specify the sort
        order of the result.
    :ivar paper.ListPaperDocsArgs.limit: Size limit per batch. The maximum
        number of docs that can be retrieved per batch is 1000. Higher value
        results in invalid arguments error.
        '_filter_by_value',
        '_sort_by_value',
        '_sort_order_value',
                 filter_by=None,
                 sort_by=None,
                 sort_order=None,
        self._filter_by_value = bb.NOT_SET
        self._sort_by_value = bb.NOT_SET
        self._sort_order_value = bb.NOT_SET
        if filter_by is not None:
            self.filter_by = filter_by
        if sort_by is not None:
            self.sort_by = sort_by
        if sort_order is not None:
            self.sort_order = sort_order
    # Instance attribute type: ListPaperDocsFilterBy (validator is set below)
    filter_by = bb.Attribute("filter_by", user_defined=True)
    # Instance attribute type: ListPaperDocsSortBy (validator is set below)
    sort_by = bb.Attribute("sort_by", user_defined=True)
    # Instance attribute type: ListPaperDocsSortOrder (validator is set below)
    sort_order = bb.Attribute("sort_order", user_defined=True)
        super(ListPaperDocsArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
ListPaperDocsArgs_validator = bv.Struct(ListPaperDocsArgs)
class ListPaperDocsContinueArgs(bb.Struct):
    :ivar paper.ListPaperDocsContinueArgs.cursor: The cursor obtained from
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_list` or
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_list_continue`. Allows
        for pagination.
        super(ListPaperDocsContinueArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
ListPaperDocsContinueArgs_validator = bv.Struct(ListPaperDocsContinueArgs)
class ListPaperDocsFilterBy(bb.Union):
    :ivar paper.ListPaperDocsFilterBy.docs_accessed: Fetches all Paper doc IDs
        that the user has ever accessed.
    :ivar paper.ListPaperDocsFilterBy.docs_created: Fetches only the Paper doc
        IDs that the user has created.
    docs_accessed = None
    docs_created = None
    def is_docs_accessed(self):
        Check if the union tag is ``docs_accessed``.
        return self._tag == 'docs_accessed'
    def is_docs_created(self):
        Check if the union tag is ``docs_created``.
        return self._tag == 'docs_created'
        super(ListPaperDocsFilterBy, self)._process_custom_annotations(annotation_type, field_path, processor)
ListPaperDocsFilterBy_validator = bv.Union(ListPaperDocsFilterBy)
class ListPaperDocsResponse(bb.Struct):
    :ivar paper.ListPaperDocsResponse.doc_ids: The list of Paper doc IDs that
        can be used to access the given Paper docs or supplied to other API
        methods. The list is sorted in the order specified by the initial call
        to :meth:`dropbox.dropbox_client.Dropbox.paper_docs_list`.
    :ivar paper.ListPaperDocsResponse.cursor: Pass the cursor into
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_list_continue` to
        paginate through all files. The cursor preserves all properties as
        specified in the original call to
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_list`.
    :ivar paper.ListPaperDocsResponse.has_more: Will be set to True if a
        subsequent call with the provided cursor to
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_list_continue` returns
        immediately with some results. If set to False please allow some delay
        before making another call to
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_list_continue`.
        '_doc_ids_value',
                 doc_ids=None,
        self._doc_ids_value = bb.NOT_SET
        if doc_ids is not None:
            self.doc_ids = doc_ids
    doc_ids = bb.Attribute("doc_ids")
    # Instance attribute type: Cursor (validator is set below)
        super(ListPaperDocsResponse, self)._process_custom_annotations(annotation_type, field_path, processor)
ListPaperDocsResponse_validator = bv.Struct(ListPaperDocsResponse)
class ListPaperDocsSortBy(bb.Union):
    :ivar paper.ListPaperDocsSortBy.accessed: Sorts the Paper docs by the time
        they were last accessed.
    :ivar paper.ListPaperDocsSortBy.modified: Sorts the Paper docs by the time
        they were last modified.
    :ivar paper.ListPaperDocsSortBy.created: Sorts the Paper docs by the
        creation time.
    accessed = None
    modified = None
    created = None
    def is_accessed(self):
        Check if the union tag is ``accessed``.
        return self._tag == 'accessed'
    def is_modified(self):
        Check if the union tag is ``modified``.
        return self._tag == 'modified'
    def is_created(self):
        Check if the union tag is ``created``.
        return self._tag == 'created'
        super(ListPaperDocsSortBy, self)._process_custom_annotations(annotation_type, field_path, processor)
ListPaperDocsSortBy_validator = bv.Union(ListPaperDocsSortBy)
class ListPaperDocsSortOrder(bb.Union):
    :ivar paper.ListPaperDocsSortOrder.ascending: Sorts the search result in
        ascending order.
    :ivar paper.ListPaperDocsSortOrder.descending: Sorts the search result in
    ascending = None
    descending = None
    def is_ascending(self):
        Check if the union tag is ``ascending``.
        return self._tag == 'ascending'
    def is_descending(self):
        Check if the union tag is ``descending``.
        return self._tag == 'descending'
        super(ListPaperDocsSortOrder, self)._process_custom_annotations(annotation_type, field_path, processor)
ListPaperDocsSortOrder_validator = bv.Union(ListPaperDocsSortOrder)
class ListUsersCursorError(PaperApiBaseError):
    :ivar paper.ListUsersCursorError.doc_not_found: The required doc was not
        :rtype: ListUsersCursorError
        super(ListUsersCursorError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListUsersCursorError_validator = bv.Union(ListUsersCursorError)
class ListUsersOnFolderArgs(RefPaperDoc):
    :ivar paper.ListUsersOnFolderArgs.limit: Size limit per batch. The maximum
        number of users that can be retrieved per batch is 1000. Higher value
        super(ListUsersOnFolderArgs, self).__init__(doc_id)
        super(ListUsersOnFolderArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
ListUsersOnFolderArgs_validator = bv.Struct(ListUsersOnFolderArgs)
class ListUsersOnFolderContinueArgs(RefPaperDoc):
    :ivar paper.ListUsersOnFolderContinueArgs.cursor: The cursor obtained from
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_folder_users_list` or
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_folder_users_list_continue`.
        Allows for pagination.
        super(ListUsersOnFolderContinueArgs, self).__init__(doc_id)
        super(ListUsersOnFolderContinueArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
ListUsersOnFolderContinueArgs_validator = bv.Struct(ListUsersOnFolderContinueArgs)
class ListUsersOnFolderResponse(bb.Struct):
    :ivar paper.ListUsersOnFolderResponse.invitees: List of email addresses that
        are invited on the Paper folder.
    :ivar paper.ListUsersOnFolderResponse.users: List of users that are invited
        on the Paper folder.
    :ivar paper.ListUsersOnFolderResponse.cursor: Pass the cursor into
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_folder_users_list_continue`
        to paginate through all users. The cursor preserves all properties as
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_folder_users_list`.
    :ivar paper.ListUsersOnFolderResponse.has_more: Will be set to True if a
        returns immediately with some results. If set to False please allow some
        delay before making another call to
        '_invitees_value',
        '_users_value',
                 invitees=None,
                 users=None,
        self._invitees_value = bb.NOT_SET
        self._users_value = bb.NOT_SET
        if invitees is not None:
            self.invitees = invitees
        if users is not None:
            self.users = users
    # Instance attribute type: list of [sharing.InviteeInfo] (validator is set below)
    invitees = bb.Attribute("invitees")
    # Instance attribute type: list of [sharing.UserInfo] (validator is set below)
    users = bb.Attribute("users")
        super(ListUsersOnFolderResponse, self)._process_custom_annotations(annotation_type, field_path, processor)
ListUsersOnFolderResponse_validator = bv.Struct(ListUsersOnFolderResponse)
class ListUsersOnPaperDocArgs(RefPaperDoc):
    :ivar paper.ListUsersOnPaperDocArgs.limit: Size limit per batch. The maximum
    :ivar paper.ListUsersOnPaperDocArgs.filter_by: Specify this attribute if you
        want to obtain users that have already accessed the Paper doc.
                 filter_by=None):
        super(ListUsersOnPaperDocArgs, self).__init__(doc_id)
    # Instance attribute type: UserOnPaperDocFilter (validator is set below)
        super(ListUsersOnPaperDocArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
ListUsersOnPaperDocArgs_validator = bv.Struct(ListUsersOnPaperDocArgs)
class ListUsersOnPaperDocContinueArgs(RefPaperDoc):
    :ivar paper.ListUsersOnPaperDocContinueArgs.cursor: The cursor obtained from
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_users_list` or
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_users_list_continue`.
        super(ListUsersOnPaperDocContinueArgs, self).__init__(doc_id)
        super(ListUsersOnPaperDocContinueArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
ListUsersOnPaperDocContinueArgs_validator = bv.Struct(ListUsersOnPaperDocContinueArgs)
class ListUsersOnPaperDocResponse(bb.Struct):
    :ivar paper.ListUsersOnPaperDocResponse.invitees: List of email addresses
        with their respective permission levels that are invited on the Paper
        doc.
    :ivar paper.ListUsersOnPaperDocResponse.users: List of users with their
        respective permission levels that are invited on the Paper folder.
    :ivar paper.ListUsersOnPaperDocResponse.doc_owner: The Paper doc owner. This
        field is populated on every single response.
    :ivar paper.ListUsersOnPaperDocResponse.cursor: Pass the cursor into
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_users_list_continue` to
        paginate through all users. The cursor preserves all properties as
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_users_list`.
    :ivar paper.ListUsersOnPaperDocResponse.has_more: Will be set to True if a
        :meth:`dropbox.dropbox_client.Dropbox.paper_docs_users_list_continue`
        '_doc_owner_value',
                 doc_owner=None,
        self._doc_owner_value = bb.NOT_SET
        if doc_owner is not None:
            self.doc_owner = doc_owner
    # Instance attribute type: list of [InviteeInfoWithPermissionLevel] (validator is set below)
    # Instance attribute type: list of [UserInfoWithPermissionLevel] (validator is set below)
    # Instance attribute type: sharing.UserInfo (validator is set below)
    doc_owner = bb.Attribute("doc_owner", user_defined=True)
        super(ListUsersOnPaperDocResponse, self)._process_custom_annotations(annotation_type, field_path, processor)
ListUsersOnPaperDocResponse_validator = bv.Struct(ListUsersOnPaperDocResponse)
class PaperApiCursorError(bb.Union):
    :ivar paper.PaperApiCursorError.expired_cursor: The provided cursor is
        expired.
    :ivar paper.PaperApiCursorError.invalid_cursor: The provided cursor is
        invalid.
    :ivar paper.PaperApiCursorError.wrong_user_in_cursor: The provided cursor
        contains invalid user.
    :ivar paper.PaperApiCursorError.reset: Indicates that the cursor has been
        invalidated. Call the corresponding non-continue endpoint to obtain a
        new cursor.
    expired_cursor = None
    wrong_user_in_cursor = None
    def is_expired_cursor(self):
        Check if the union tag is ``expired_cursor``.
        return self._tag == 'expired_cursor'
    def is_wrong_user_in_cursor(self):
        Check if the union tag is ``wrong_user_in_cursor``.
        return self._tag == 'wrong_user_in_cursor'
        super(PaperApiCursorError, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperApiCursorError_validator = bv.Union(PaperApiCursorError)
class PaperDocCreateArgs(bb.Struct):
    :ivar paper.PaperDocCreateArgs.parent_folder_id: The Paper folder ID where
        the Paper document should be created. The API user has to have write
    :ivar paper.PaperDocCreateArgs.import_format: The format of provided data.
        '_parent_folder_id_value',
        self._parent_folder_id_value = bb.NOT_SET
        if parent_folder_id is not None:
            self.parent_folder_id = parent_folder_id
    parent_folder_id = bb.Attribute("parent_folder_id", nullable=True)
        super(PaperDocCreateArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperDocCreateArgs_validator = bv.Struct(PaperDocCreateArgs)
class PaperDocCreateError(PaperApiBaseError):
    :ivar paper.PaperDocCreateError.content_malformed: The provided content was
    :ivar paper.PaperDocCreateError.folder_not_found: The specified Paper folder
        is cannot be found.
    :ivar paper.PaperDocCreateError.doc_length_exceeded: The newly created Paper
        doc would be too large. Please split the content into multiple docs.
    :ivar paper.PaperDocCreateError.image_size_exceeded: The imported document
    folder_not_found = None
    def is_folder_not_found(self):
        Check if the union tag is ``folder_not_found``.
        return self._tag == 'folder_not_found'
        super(PaperDocCreateError, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperDocCreateError_validator = bv.Union(PaperDocCreateError)
class PaperDocCreateUpdateResult(bb.Struct):
    :ivar paper.PaperDocCreateUpdateResult.doc_id: Doc ID of the newly created
    :ivar paper.PaperDocCreateUpdateResult.revision: The Paper doc revision.
        Simply an ever increasing number.
    :ivar paper.PaperDocCreateUpdateResult.title: The Paper doc title.
        '_revision_value',
                 revision=None,
                 title=None):
        self._revision_value = bb.NOT_SET
        if revision is not None:
            self.revision = revision
    revision = bb.Attribute("revision")
        super(PaperDocCreateUpdateResult, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperDocCreateUpdateResult_validator = bv.Struct(PaperDocCreateUpdateResult)
class PaperDocExport(RefPaperDoc):
        super(PaperDocExport, self).__init__(doc_id)
    # Instance attribute type: ExportFormat (validator is set below)
    export_format = bb.Attribute("export_format", user_defined=True)
        super(PaperDocExport, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperDocExport_validator = bv.Struct(PaperDocExport)
class PaperDocExportResult(bb.Struct):
    :ivar paper.PaperDocExportResult.owner: The Paper doc owner's email address.
    :ivar paper.PaperDocExportResult.title: The Paper doc title.
    :ivar paper.PaperDocExportResult.revision: The Paper doc revision. Simply an
        ever increasing number.
    :ivar paper.PaperDocExportResult.mime_type: MIME type of the export. This
        corresponds to :class:`ExportFormat` specified in the request.
        '_owner_value',
        '_mime_type_value',
                 owner=None,
                 mime_type=None):
        self._owner_value = bb.NOT_SET
        self._mime_type_value = bb.NOT_SET
        if owner is not None:
            self.owner = owner
        if mime_type is not None:
            self.mime_type = mime_type
    owner = bb.Attribute("owner")
    mime_type = bb.Attribute("mime_type")
        super(PaperDocExportResult, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperDocExportResult_validator = bv.Struct(PaperDocExportResult)
class PaperDocPermissionLevel(bb.Union):
    :ivar paper.PaperDocPermissionLevel.edit: User will be granted edit
    :ivar paper.PaperDocPermissionLevel.view_and_comment: User will be granted
        view and comment permissions.
    edit = None
    view_and_comment = None
    def is_edit(self):
        Check if the union tag is ``edit``.
        return self._tag == 'edit'
    def is_view_and_comment(self):
        Check if the union tag is ``view_and_comment``.
        return self._tag == 'view_and_comment'
        super(PaperDocPermissionLevel, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperDocPermissionLevel_validator = bv.Union(PaperDocPermissionLevel)
class PaperDocSharingPolicy(RefPaperDoc):
    :ivar paper.PaperDocSharingPolicy.sharing_policy: The default sharing policy
        to be set for the Paper doc.
        '_sharing_policy_value',
                 sharing_policy=None):
        super(PaperDocSharingPolicy, self).__init__(doc_id)
        self._sharing_policy_value = bb.NOT_SET
        if sharing_policy is not None:
            self.sharing_policy = sharing_policy
    # Instance attribute type: SharingPolicy (validator is set below)
    sharing_policy = bb.Attribute("sharing_policy", user_defined=True)
        super(PaperDocSharingPolicy, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperDocSharingPolicy_validator = bv.Struct(PaperDocSharingPolicy)
class PaperDocUpdateArgs(RefPaperDoc):
    :ivar paper.PaperDocUpdateArgs.doc_update_policy: The policy used for the
        current update call.
    :ivar paper.PaperDocUpdateArgs.revision: The latest doc revision. This value
        must match the head revision or an error code will be returned. This is
        to prevent colliding writes.
    :ivar paper.PaperDocUpdateArgs.import_format: The format of provided data.
        super(PaperDocUpdateArgs, self).__init__(doc_id)
        super(PaperDocUpdateArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperDocUpdateArgs_validator = bv.Struct(PaperDocUpdateArgs)
class PaperDocUpdateError(DocLookupError):
    :ivar paper.PaperDocUpdateError.content_malformed: The provided content was
    :ivar paper.PaperDocUpdateError.revision_mismatch: The provided revision
        does not match the document head.
    :ivar paper.PaperDocUpdateError.doc_length_exceeded: The newly created Paper
        doc would be too large, split the content into multiple docs.
    :ivar paper.PaperDocUpdateError.image_size_exceeded: The imported document
    :ivar paper.PaperDocUpdateError.doc_archived: This operation is not allowed
        on archived Paper docs.
    :ivar paper.PaperDocUpdateError.doc_deleted: This operation is not allowed
        on deleted Paper docs.
        super(PaperDocUpdateError, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperDocUpdateError_validator = bv.Union(PaperDocUpdateError)
    :ivar paper.PaperDocUpdatePolicy.append: The content will be appended to the
    :ivar paper.PaperDocUpdatePolicy.prepend: The content will be prepended to
        the doc. The doc title will not be affected.
    :ivar paper.PaperDocUpdatePolicy.overwrite_all: The document will be
        overwitten at the head with the provided content.
    overwrite_all = None
    def is_overwrite_all(self):
        Check if the union tag is ``overwrite_all``.
        return self._tag == 'overwrite_all'
class PaperFolderCreateArg(bb.Struct):
    :ivar paper.PaperFolderCreateArg.name: The name of the new Paper folder.
    :ivar paper.PaperFolderCreateArg.parent_folder_id: The encrypted Paper
        folder Id where the new Paper folder should be created. The API user has
        to have write access to this folder or error is thrown. If not supplied,
        the new folder will be created at top level.
    :ivar paper.PaperFolderCreateArg.is_team_folder: Whether the folder to be
        created should be a team folder. This value will be ignored if
        parent_folder_id is supplied, as the new folder will inherit the type
        (private or team folder) from its parent. We will by default create a
        top-level private folder if both parent_folder_id and is_team_folder are
        not supplied.
        '_is_team_folder_value',
        self._is_team_folder_value = bb.NOT_SET
        if is_team_folder is not None:
            self.is_team_folder = is_team_folder
    is_team_folder = bb.Attribute("is_team_folder", nullable=True)
        super(PaperFolderCreateArg, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperFolderCreateArg_validator = bv.Struct(PaperFolderCreateArg)
class PaperFolderCreateError(PaperApiBaseError):
    :ivar paper.PaperFolderCreateError.folder_not_found: The specified parent
        Paper folder cannot be found.
    :ivar paper.PaperFolderCreateError.invalid_folder_id: The folder id cannot
        be decrypted to valid folder id.
    invalid_folder_id = None
    def is_invalid_folder_id(self):
        Check if the union tag is ``invalid_folder_id``.
        return self._tag == 'invalid_folder_id'
        super(PaperFolderCreateError, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperFolderCreateError_validator = bv.Union(PaperFolderCreateError)
class PaperFolderCreateResult(bb.Struct):
    :ivar paper.PaperFolderCreateResult.folder_id: Folder ID of the newly
        created folder.
        '_folder_id_value',
                 folder_id=None):
        self._folder_id_value = bb.NOT_SET
        if folder_id is not None:
            self.folder_id = folder_id
    folder_id = bb.Attribute("folder_id")
        super(PaperFolderCreateResult, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperFolderCreateResult_validator = bv.Struct(PaperFolderCreateResult)
class RemovePaperDocUser(RefPaperDoc):
    :ivar paper.RemovePaperDocUser.member: User which should be removed from the
        Paper doc. Specify only email address or Dropbox account ID.
                 member=None):
        super(RemovePaperDocUser, self).__init__(doc_id)
        super(RemovePaperDocUser, self)._process_custom_annotations(annotation_type, field_path, processor)
RemovePaperDocUser_validator = bv.Struct(RemovePaperDocUser)
class SharingPolicy(bb.Struct):
    Sharing policy of Paper doc.
    :ivar paper.SharingPolicy.public_sharing_policy: This value applies to the
        non-team members.
    :ivar paper.SharingPolicy.team_sharing_policy: This value applies to the
        team members only. The value is null for all personal accounts.
        '_public_sharing_policy_value',
        '_team_sharing_policy_value',
                 public_sharing_policy=None,
                 team_sharing_policy=None):
        self._public_sharing_policy_value = bb.NOT_SET
        self._team_sharing_policy_value = bb.NOT_SET
        if public_sharing_policy is not None:
            self.public_sharing_policy = public_sharing_policy
        if team_sharing_policy is not None:
            self.team_sharing_policy = team_sharing_policy
    # Instance attribute type: SharingPublicPolicyType (validator is set below)
    public_sharing_policy = bb.Attribute("public_sharing_policy", nullable=True, user_defined=True)
    # Instance attribute type: SharingTeamPolicyType (validator is set below)
    team_sharing_policy = bb.Attribute("team_sharing_policy", nullable=True, user_defined=True)
        super(SharingPolicy, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingPolicy_validator = bv.Struct(SharingPolicy)
class SharingTeamPolicyType(bb.Union):
    The sharing policy type of the Paper doc.
    :ivar paper.SharingTeamPolicyType.people_with_link_can_edit: Users who have
        a link to this doc can edit it.
    :ivar paper.SharingTeamPolicyType.people_with_link_can_view_and_comment:
        Users who have a link to this doc can view and comment on it.
    :ivar paper.SharingTeamPolicyType.invite_only: Users must be explicitly
        invited to this doc.
    people_with_link_can_edit = None
    people_with_link_can_view_and_comment = None
    def is_people_with_link_can_edit(self):
        Check if the union tag is ``people_with_link_can_edit``.
        return self._tag == 'people_with_link_can_edit'
    def is_people_with_link_can_view_and_comment(self):
        Check if the union tag is ``people_with_link_can_view_and_comment``.
        return self._tag == 'people_with_link_can_view_and_comment'
        super(SharingTeamPolicyType, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingTeamPolicyType_validator = bv.Union(SharingTeamPolicyType)
class SharingPublicPolicyType(SharingTeamPolicyType):
    :ivar paper.SharingPublicPolicyType.disabled: Value used to indicate that
        doc sharing is enabled only within team.
    disabled = None
    def is_disabled(self):
        Check if the union tag is ``disabled``.
        return self._tag == 'disabled'
        super(SharingPublicPolicyType, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingPublicPolicyType_validator = bv.Union(SharingPublicPolicyType)
class UserInfoWithPermissionLevel(bb.Struct):
    :ivar paper.UserInfoWithPermissionLevel.user: User shared on the Paper doc.
    :ivar paper.UserInfoWithPermissionLevel.permission_level: Permission level
        for the user.
        '_user_value',
                 user=None,
        self._user_value = bb.NOT_SET
        if user is not None:
            self.user = user
    user = bb.Attribute("user", user_defined=True)
        super(UserInfoWithPermissionLevel, self)._process_custom_annotations(annotation_type, field_path, processor)
UserInfoWithPermissionLevel_validator = bv.Struct(UserInfoWithPermissionLevel)
class UserOnPaperDocFilter(bb.Union):
    :ivar paper.UserOnPaperDocFilter.visited: all users who have visited the
    :ivar paper.UserOnPaperDocFilter.shared: All uses who are shared on the
        Paper doc. This includes all users who have visited the Paper doc as
        well as those who have not.
    visited = None
    shared = None
    def is_visited(self):
        Check if the union tag is ``visited``.
        return self._tag == 'visited'
    def is_shared(self):
        Check if the union tag is ``shared``.
        return self._tag == 'shared'
        super(UserOnPaperDocFilter, self)._process_custom_annotations(annotation_type, field_path, processor)
UserOnPaperDocFilter_validator = bv.Union(UserOnPaperDocFilter)
# Paper doc ID.
PaperDocId_validator = bv.String()
AddMember.permission_level.validator = PaperDocPermissionLevel_validator
AddMember.member.validator = sharing.MemberSelector_validator
AddMember._all_field_names_ = set([
    'permission_level',
    'member',
AddMember._all_fields_ = [
    ('permission_level', AddMember.permission_level.validator),
    ('member', AddMember.member.validator),
RefPaperDoc.doc_id.validator = PaperDocId_validator
RefPaperDoc._all_field_names_ = set(['doc_id'])
RefPaperDoc._all_fields_ = [('doc_id', RefPaperDoc.doc_id.validator)]
AddPaperDocUser.members.validator = bv.List(AddMember_validator, max_items=20)
AddPaperDocUser.custom_message.validator = bv.Nullable(bv.String())
AddPaperDocUser.quiet.validator = bv.Boolean()
AddPaperDocUser._all_field_names_ = RefPaperDoc._all_field_names_.union(set([
    'members',
    'custom_message',
    'quiet',
AddPaperDocUser._all_fields_ = RefPaperDoc._all_fields_ + [
    ('members', AddPaperDocUser.members.validator),
    ('custom_message', AddPaperDocUser.custom_message.validator),
    ('quiet', AddPaperDocUser.quiet.validator),
AddPaperDocUserMemberResult.member.validator = sharing.MemberSelector_validator
AddPaperDocUserMemberResult.result.validator = AddPaperDocUserResult_validator
AddPaperDocUserMemberResult._all_field_names_ = set([
    'result',
AddPaperDocUserMemberResult._all_fields_ = [
    ('member', AddPaperDocUserMemberResult.member.validator),
    ('result', AddPaperDocUserMemberResult.result.validator),
AddPaperDocUserResult._success_validator = bv.Void()
AddPaperDocUserResult._unknown_error_validator = bv.Void()
AddPaperDocUserResult._sharing_outside_team_disabled_validator = bv.Void()
AddPaperDocUserResult._daily_limit_reached_validator = bv.Void()
AddPaperDocUserResult._user_is_owner_validator = bv.Void()
AddPaperDocUserResult._failed_user_data_retrieval_validator = bv.Void()
AddPaperDocUserResult._permission_already_granted_validator = bv.Void()
AddPaperDocUserResult._other_validator = bv.Void()
AddPaperDocUserResult._tagmap = {
    'success': AddPaperDocUserResult._success_validator,
    'unknown_error': AddPaperDocUserResult._unknown_error_validator,
    'sharing_outside_team_disabled': AddPaperDocUserResult._sharing_outside_team_disabled_validator,
    'daily_limit_reached': AddPaperDocUserResult._daily_limit_reached_validator,
    'user_is_owner': AddPaperDocUserResult._user_is_owner_validator,
    'failed_user_data_retrieval': AddPaperDocUserResult._failed_user_data_retrieval_validator,
    'permission_already_granted': AddPaperDocUserResult._permission_already_granted_validator,
    'other': AddPaperDocUserResult._other_validator,
AddPaperDocUserResult.success = AddPaperDocUserResult('success')
AddPaperDocUserResult.unknown_error = AddPaperDocUserResult('unknown_error')
AddPaperDocUserResult.sharing_outside_team_disabled = AddPaperDocUserResult('sharing_outside_team_disabled')
AddPaperDocUserResult.daily_limit_reached = AddPaperDocUserResult('daily_limit_reached')
AddPaperDocUserResult.user_is_owner = AddPaperDocUserResult('user_is_owner')
AddPaperDocUserResult.failed_user_data_retrieval = AddPaperDocUserResult('failed_user_data_retrieval')
AddPaperDocUserResult.permission_already_granted = AddPaperDocUserResult('permission_already_granted')
AddPaperDocUserResult.other = AddPaperDocUserResult('other')
Cursor.value.validator = bv.String()
Cursor.expiration.validator = bv.Nullable(common.DropboxTimestamp_validator)
Cursor._all_field_names_ = set([
    'expiration',
Cursor._all_fields_ = [
    ('value', Cursor.value.validator),
    ('expiration', Cursor.expiration.validator),
PaperApiBaseError._insufficient_permissions_validator = bv.Void()
PaperApiBaseError._other_validator = bv.Void()
PaperApiBaseError._tagmap = {
    'insufficient_permissions': PaperApiBaseError._insufficient_permissions_validator,
    'other': PaperApiBaseError._other_validator,
PaperApiBaseError.insufficient_permissions = PaperApiBaseError('insufficient_permissions')
PaperApiBaseError.other = PaperApiBaseError('other')
DocLookupError._doc_not_found_validator = bv.Void()
DocLookupError._tagmap = {
    'doc_not_found': DocLookupError._doc_not_found_validator,
DocLookupError._tagmap.update(PaperApiBaseError._tagmap)
DocLookupError.doc_not_found = DocLookupError('doc_not_found')
DocSubscriptionLevel._default_validator = bv.Void()
DocSubscriptionLevel._ignore_validator = bv.Void()
DocSubscriptionLevel._every_validator = bv.Void()
DocSubscriptionLevel._no_email_validator = bv.Void()
DocSubscriptionLevel._tagmap = {
    'default': DocSubscriptionLevel._default_validator,
    'ignore': DocSubscriptionLevel._ignore_validator,
    'every': DocSubscriptionLevel._every_validator,
    'no_email': DocSubscriptionLevel._no_email_validator,
DocSubscriptionLevel.default = DocSubscriptionLevel('default')
DocSubscriptionLevel.ignore = DocSubscriptionLevel('ignore')
DocSubscriptionLevel.every = DocSubscriptionLevel('every')
DocSubscriptionLevel.no_email = DocSubscriptionLevel('no_email')
ExportFormat._html_validator = bv.Void()
ExportFormat._markdown_validator = bv.Void()
ExportFormat._other_validator = bv.Void()
ExportFormat._tagmap = {
    'html': ExportFormat._html_validator,
    'markdown': ExportFormat._markdown_validator,
    'other': ExportFormat._other_validator,
ExportFormat.html = ExportFormat('html')
ExportFormat.markdown = ExportFormat('markdown')
ExportFormat.other = ExportFormat('other')
Folder.id.validator = bv.String()
Folder.name.validator = bv.String()
Folder._all_field_names_ = set([
Folder._all_fields_ = [
    ('id', Folder.id.validator),
    ('name', Folder.name.validator),
FolderSharingPolicyType._team_validator = bv.Void()
FolderSharingPolicyType._invite_only_validator = bv.Void()
FolderSharingPolicyType._tagmap = {
    'team': FolderSharingPolicyType._team_validator,
    'invite_only': FolderSharingPolicyType._invite_only_validator,
FolderSharingPolicyType.team = FolderSharingPolicyType('team')
FolderSharingPolicyType.invite_only = FolderSharingPolicyType('invite_only')
FolderSubscriptionLevel._none_validator = bv.Void()
FolderSubscriptionLevel._activity_only_validator = bv.Void()
FolderSubscriptionLevel._daily_emails_validator = bv.Void()
FolderSubscriptionLevel._weekly_emails_validator = bv.Void()
FolderSubscriptionLevel._tagmap = {
    'none': FolderSubscriptionLevel._none_validator,
    'activity_only': FolderSubscriptionLevel._activity_only_validator,
    'daily_emails': FolderSubscriptionLevel._daily_emails_validator,
    'weekly_emails': FolderSubscriptionLevel._weekly_emails_validator,
FolderSubscriptionLevel.none = FolderSubscriptionLevel('none')
FolderSubscriptionLevel.activity_only = FolderSubscriptionLevel('activity_only')
FolderSubscriptionLevel.daily_emails = FolderSubscriptionLevel('daily_emails')
FolderSubscriptionLevel.weekly_emails = FolderSubscriptionLevel('weekly_emails')
FoldersContainingPaperDoc.folder_sharing_policy_type.validator = bv.Nullable(FolderSharingPolicyType_validator)
FoldersContainingPaperDoc.folders.validator = bv.Nullable(bv.List(Folder_validator))
FoldersContainingPaperDoc._all_field_names_ = set([
    'folder_sharing_policy_type',
    'folders',
FoldersContainingPaperDoc._all_fields_ = [
    ('folder_sharing_policy_type', FoldersContainingPaperDoc.folder_sharing_policy_type.validator),
    ('folders', FoldersContainingPaperDoc.folders.validator),
InviteeInfoWithPermissionLevel.invitee.validator = sharing.InviteeInfo_validator
InviteeInfoWithPermissionLevel.permission_level.validator = PaperDocPermissionLevel_validator
InviteeInfoWithPermissionLevel._all_field_names_ = set([
    'invitee',
InviteeInfoWithPermissionLevel._all_fields_ = [
    ('invitee', InviteeInfoWithPermissionLevel.invitee.validator),
    ('permission_level', InviteeInfoWithPermissionLevel.permission_level.validator),
ListDocsCursorError._cursor_error_validator = PaperApiCursorError_validator
ListDocsCursorError._other_validator = bv.Void()
ListDocsCursorError._tagmap = {
    'cursor_error': ListDocsCursorError._cursor_error_validator,
    'other': ListDocsCursorError._other_validator,
ListDocsCursorError.other = ListDocsCursorError('other')
ListPaperDocsArgs.filter_by.validator = ListPaperDocsFilterBy_validator
ListPaperDocsArgs.sort_by.validator = ListPaperDocsSortBy_validator
ListPaperDocsArgs.sort_order.validator = ListPaperDocsSortOrder_validator
ListPaperDocsArgs.limit.validator = bv.Int32(min_value=1, max_value=1000)
ListPaperDocsArgs._all_field_names_ = set([
    'filter_by',
    'sort_by',
    'sort_order',
ListPaperDocsArgs._all_fields_ = [
    ('filter_by', ListPaperDocsArgs.filter_by.validator),
    ('sort_by', ListPaperDocsArgs.sort_by.validator),
    ('sort_order', ListPaperDocsArgs.sort_order.validator),
    ('limit', ListPaperDocsArgs.limit.validator),
ListPaperDocsContinueArgs.cursor.validator = bv.String()
ListPaperDocsContinueArgs._all_field_names_ = set(['cursor'])
ListPaperDocsContinueArgs._all_fields_ = [('cursor', ListPaperDocsContinueArgs.cursor.validator)]
ListPaperDocsFilterBy._docs_accessed_validator = bv.Void()
ListPaperDocsFilterBy._docs_created_validator = bv.Void()
ListPaperDocsFilterBy._other_validator = bv.Void()
ListPaperDocsFilterBy._tagmap = {
    'docs_accessed': ListPaperDocsFilterBy._docs_accessed_validator,
    'docs_created': ListPaperDocsFilterBy._docs_created_validator,
    'other': ListPaperDocsFilterBy._other_validator,
ListPaperDocsFilterBy.docs_accessed = ListPaperDocsFilterBy('docs_accessed')
ListPaperDocsFilterBy.docs_created = ListPaperDocsFilterBy('docs_created')
ListPaperDocsFilterBy.other = ListPaperDocsFilterBy('other')
ListPaperDocsResponse.doc_ids.validator = bv.List(PaperDocId_validator)
ListPaperDocsResponse.cursor.validator = Cursor_validator
ListPaperDocsResponse.has_more.validator = bv.Boolean()
ListPaperDocsResponse._all_field_names_ = set([
    'doc_ids',
ListPaperDocsResponse._all_fields_ = [
    ('doc_ids', ListPaperDocsResponse.doc_ids.validator),
    ('cursor', ListPaperDocsResponse.cursor.validator),
    ('has_more', ListPaperDocsResponse.has_more.validator),
ListPaperDocsSortBy._accessed_validator = bv.Void()
ListPaperDocsSortBy._modified_validator = bv.Void()
ListPaperDocsSortBy._created_validator = bv.Void()
ListPaperDocsSortBy._other_validator = bv.Void()
ListPaperDocsSortBy._tagmap = {
    'accessed': ListPaperDocsSortBy._accessed_validator,
    'modified': ListPaperDocsSortBy._modified_validator,
    'created': ListPaperDocsSortBy._created_validator,
    'other': ListPaperDocsSortBy._other_validator,
ListPaperDocsSortBy.accessed = ListPaperDocsSortBy('accessed')
ListPaperDocsSortBy.modified = ListPaperDocsSortBy('modified')
ListPaperDocsSortBy.created = ListPaperDocsSortBy('created')
ListPaperDocsSortBy.other = ListPaperDocsSortBy('other')
ListPaperDocsSortOrder._ascending_validator = bv.Void()
ListPaperDocsSortOrder._descending_validator = bv.Void()
ListPaperDocsSortOrder._other_validator = bv.Void()
ListPaperDocsSortOrder._tagmap = {
    'ascending': ListPaperDocsSortOrder._ascending_validator,
    'descending': ListPaperDocsSortOrder._descending_validator,
    'other': ListPaperDocsSortOrder._other_validator,
ListPaperDocsSortOrder.ascending = ListPaperDocsSortOrder('ascending')
ListPaperDocsSortOrder.descending = ListPaperDocsSortOrder('descending')
ListPaperDocsSortOrder.other = ListPaperDocsSortOrder('other')
ListUsersCursorError._doc_not_found_validator = bv.Void()
ListUsersCursorError._cursor_error_validator = PaperApiCursorError_validator
ListUsersCursorError._tagmap = {
    'doc_not_found': ListUsersCursorError._doc_not_found_validator,
    'cursor_error': ListUsersCursorError._cursor_error_validator,
ListUsersCursorError._tagmap.update(PaperApiBaseError._tagmap)
ListUsersCursorError.doc_not_found = ListUsersCursorError('doc_not_found')
ListUsersOnFolderArgs.limit.validator = bv.Int32(min_value=1, max_value=1000)
ListUsersOnFolderArgs._all_field_names_ = RefPaperDoc._all_field_names_.union(set(['limit']))
ListUsersOnFolderArgs._all_fields_ = RefPaperDoc._all_fields_ + [('limit', ListUsersOnFolderArgs.limit.validator)]
ListUsersOnFolderContinueArgs.cursor.validator = bv.String()
ListUsersOnFolderContinueArgs._all_field_names_ = RefPaperDoc._all_field_names_.union(set(['cursor']))
ListUsersOnFolderContinueArgs._all_fields_ = RefPaperDoc._all_fields_ + [('cursor', ListUsersOnFolderContinueArgs.cursor.validator)]
ListUsersOnFolderResponse.invitees.validator = bv.List(sharing.InviteeInfo_validator)
ListUsersOnFolderResponse.users.validator = bv.List(sharing.UserInfo_validator)
ListUsersOnFolderResponse.cursor.validator = Cursor_validator
ListUsersOnFolderResponse.has_more.validator = bv.Boolean()
ListUsersOnFolderResponse._all_field_names_ = set([
    'invitees',
ListUsersOnFolderResponse._all_fields_ = [
    ('invitees', ListUsersOnFolderResponse.invitees.validator),
    ('users', ListUsersOnFolderResponse.users.validator),
    ('cursor', ListUsersOnFolderResponse.cursor.validator),
    ('has_more', ListUsersOnFolderResponse.has_more.validator),
ListUsersOnPaperDocArgs.limit.validator = bv.Int32(min_value=1, max_value=1000)
ListUsersOnPaperDocArgs.filter_by.validator = UserOnPaperDocFilter_validator
ListUsersOnPaperDocArgs._all_field_names_ = RefPaperDoc._all_field_names_.union(set([
ListUsersOnPaperDocArgs._all_fields_ = RefPaperDoc._all_fields_ + [
    ('limit', ListUsersOnPaperDocArgs.limit.validator),
    ('filter_by', ListUsersOnPaperDocArgs.filter_by.validator),
ListUsersOnPaperDocContinueArgs.cursor.validator = bv.String()
ListUsersOnPaperDocContinueArgs._all_field_names_ = RefPaperDoc._all_field_names_.union(set(['cursor']))
ListUsersOnPaperDocContinueArgs._all_fields_ = RefPaperDoc._all_fields_ + [('cursor', ListUsersOnPaperDocContinueArgs.cursor.validator)]
ListUsersOnPaperDocResponse.invitees.validator = bv.List(InviteeInfoWithPermissionLevel_validator)
ListUsersOnPaperDocResponse.users.validator = bv.List(UserInfoWithPermissionLevel_validator)
ListUsersOnPaperDocResponse.doc_owner.validator = sharing.UserInfo_validator
ListUsersOnPaperDocResponse.cursor.validator = Cursor_validator
ListUsersOnPaperDocResponse.has_more.validator = bv.Boolean()
ListUsersOnPaperDocResponse._all_field_names_ = set([
    'doc_owner',
ListUsersOnPaperDocResponse._all_fields_ = [
    ('invitees', ListUsersOnPaperDocResponse.invitees.validator),
    ('users', ListUsersOnPaperDocResponse.users.validator),
    ('doc_owner', ListUsersOnPaperDocResponse.doc_owner.validator),
    ('cursor', ListUsersOnPaperDocResponse.cursor.validator),
    ('has_more', ListUsersOnPaperDocResponse.has_more.validator),
PaperApiCursorError._expired_cursor_validator = bv.Void()
PaperApiCursorError._invalid_cursor_validator = bv.Void()
PaperApiCursorError._wrong_user_in_cursor_validator = bv.Void()
PaperApiCursorError._reset_validator = bv.Void()
PaperApiCursorError._other_validator = bv.Void()
PaperApiCursorError._tagmap = {
    'expired_cursor': PaperApiCursorError._expired_cursor_validator,
    'invalid_cursor': PaperApiCursorError._invalid_cursor_validator,
    'wrong_user_in_cursor': PaperApiCursorError._wrong_user_in_cursor_validator,
    'reset': PaperApiCursorError._reset_validator,
    'other': PaperApiCursorError._other_validator,
PaperApiCursorError.expired_cursor = PaperApiCursorError('expired_cursor')
PaperApiCursorError.invalid_cursor = PaperApiCursorError('invalid_cursor')
PaperApiCursorError.wrong_user_in_cursor = PaperApiCursorError('wrong_user_in_cursor')
PaperApiCursorError.reset = PaperApiCursorError('reset')
PaperApiCursorError.other = PaperApiCursorError('other')
PaperDocCreateArgs.parent_folder_id.validator = bv.Nullable(bv.String())
PaperDocCreateArgs.import_format.validator = ImportFormat_validator
PaperDocCreateArgs._all_field_names_ = set([
    'parent_folder_id',
PaperDocCreateArgs._all_fields_ = [
    ('parent_folder_id', PaperDocCreateArgs.parent_folder_id.validator),
    ('import_format', PaperDocCreateArgs.import_format.validator),
PaperDocCreateError._content_malformed_validator = bv.Void()
PaperDocCreateError._folder_not_found_validator = bv.Void()
PaperDocCreateError._doc_length_exceeded_validator = bv.Void()
PaperDocCreateError._image_size_exceeded_validator = bv.Void()
PaperDocCreateError._tagmap = {
    'content_malformed': PaperDocCreateError._content_malformed_validator,
    'folder_not_found': PaperDocCreateError._folder_not_found_validator,
    'doc_length_exceeded': PaperDocCreateError._doc_length_exceeded_validator,
    'image_size_exceeded': PaperDocCreateError._image_size_exceeded_validator,
PaperDocCreateError._tagmap.update(PaperApiBaseError._tagmap)
PaperDocCreateError.content_malformed = PaperDocCreateError('content_malformed')
PaperDocCreateError.folder_not_found = PaperDocCreateError('folder_not_found')
PaperDocCreateError.doc_length_exceeded = PaperDocCreateError('doc_length_exceeded')
PaperDocCreateError.image_size_exceeded = PaperDocCreateError('image_size_exceeded')
PaperDocCreateUpdateResult.doc_id.validator = bv.String()
PaperDocCreateUpdateResult.revision.validator = bv.Int64()
PaperDocCreateUpdateResult.title.validator = bv.String()
PaperDocCreateUpdateResult._all_field_names_ = set([
    'doc_id',
    'revision',
PaperDocCreateUpdateResult._all_fields_ = [
    ('doc_id', PaperDocCreateUpdateResult.doc_id.validator),
    ('revision', PaperDocCreateUpdateResult.revision.validator),
    ('title', PaperDocCreateUpdateResult.title.validator),
PaperDocExport.export_format.validator = ExportFormat_validator
PaperDocExport._all_field_names_ = RefPaperDoc._all_field_names_.union(set(['export_format']))
PaperDocExport._all_fields_ = RefPaperDoc._all_fields_ + [('export_format', PaperDocExport.export_format.validator)]
PaperDocExportResult.owner.validator = bv.String()
PaperDocExportResult.title.validator = bv.String()
PaperDocExportResult.revision.validator = bv.Int64()
PaperDocExportResult.mime_type.validator = bv.String()
PaperDocExportResult._all_field_names_ = set([
    'owner',
    'mime_type',
PaperDocExportResult._all_fields_ = [
    ('owner', PaperDocExportResult.owner.validator),
    ('title', PaperDocExportResult.title.validator),
    ('revision', PaperDocExportResult.revision.validator),
    ('mime_type', PaperDocExportResult.mime_type.validator),
PaperDocPermissionLevel._edit_validator = bv.Void()
PaperDocPermissionLevel._view_and_comment_validator = bv.Void()
PaperDocPermissionLevel._other_validator = bv.Void()
PaperDocPermissionLevel._tagmap = {
    'edit': PaperDocPermissionLevel._edit_validator,
    'view_and_comment': PaperDocPermissionLevel._view_and_comment_validator,
    'other': PaperDocPermissionLevel._other_validator,
PaperDocPermissionLevel.edit = PaperDocPermissionLevel('edit')
PaperDocPermissionLevel.view_and_comment = PaperDocPermissionLevel('view_and_comment')
PaperDocPermissionLevel.other = PaperDocPermissionLevel('other')
PaperDocSharingPolicy.sharing_policy.validator = SharingPolicy_validator
PaperDocSharingPolicy._all_field_names_ = RefPaperDoc._all_field_names_.union(set(['sharing_policy']))
PaperDocSharingPolicy._all_fields_ = RefPaperDoc._all_fields_ + [('sharing_policy', PaperDocSharingPolicy.sharing_policy.validator)]
PaperDocUpdateArgs.doc_update_policy.validator = PaperDocUpdatePolicy_validator
PaperDocUpdateArgs.revision.validator = bv.Int64()
PaperDocUpdateArgs.import_format.validator = ImportFormat_validator
PaperDocUpdateArgs._all_field_names_ = RefPaperDoc._all_field_names_.union(set([
PaperDocUpdateArgs._all_fields_ = RefPaperDoc._all_fields_ + [
    ('doc_update_policy', PaperDocUpdateArgs.doc_update_policy.validator),
    ('revision', PaperDocUpdateArgs.revision.validator),
    ('import_format', PaperDocUpdateArgs.import_format.validator),
PaperDocUpdateError._content_malformed_validator = bv.Void()
PaperDocUpdateError._revision_mismatch_validator = bv.Void()
PaperDocUpdateError._doc_length_exceeded_validator = bv.Void()
PaperDocUpdateError._image_size_exceeded_validator = bv.Void()
PaperDocUpdateError._doc_archived_validator = bv.Void()
PaperDocUpdateError._doc_deleted_validator = bv.Void()
PaperDocUpdateError._tagmap = {
    'content_malformed': PaperDocUpdateError._content_malformed_validator,
    'revision_mismatch': PaperDocUpdateError._revision_mismatch_validator,
    'doc_length_exceeded': PaperDocUpdateError._doc_length_exceeded_validator,
    'image_size_exceeded': PaperDocUpdateError._image_size_exceeded_validator,
    'doc_archived': PaperDocUpdateError._doc_archived_validator,
    'doc_deleted': PaperDocUpdateError._doc_deleted_validator,
PaperDocUpdateError._tagmap.update(DocLookupError._tagmap)
PaperDocUpdateError.content_malformed = PaperDocUpdateError('content_malformed')
PaperDocUpdateError.revision_mismatch = PaperDocUpdateError('revision_mismatch')
PaperDocUpdateError.doc_length_exceeded = PaperDocUpdateError('doc_length_exceeded')
PaperDocUpdateError.image_size_exceeded = PaperDocUpdateError('image_size_exceeded')
PaperDocUpdateError.doc_archived = PaperDocUpdateError('doc_archived')
PaperDocUpdateError.doc_deleted = PaperDocUpdateError('doc_deleted')
PaperDocUpdatePolicy._overwrite_all_validator = bv.Void()
    'overwrite_all': PaperDocUpdatePolicy._overwrite_all_validator,
PaperDocUpdatePolicy.overwrite_all = PaperDocUpdatePolicy('overwrite_all')
PaperFolderCreateArg.name.validator = bv.String()
PaperFolderCreateArg.parent_folder_id.validator = bv.Nullable(bv.String())
PaperFolderCreateArg.is_team_folder.validator = bv.Nullable(bv.Boolean())
PaperFolderCreateArg._all_field_names_ = set([
    'is_team_folder',
PaperFolderCreateArg._all_fields_ = [
    ('name', PaperFolderCreateArg.name.validator),
    ('parent_folder_id', PaperFolderCreateArg.parent_folder_id.validator),
    ('is_team_folder', PaperFolderCreateArg.is_team_folder.validator),
PaperFolderCreateError._folder_not_found_validator = bv.Void()
PaperFolderCreateError._invalid_folder_id_validator = bv.Void()
PaperFolderCreateError._tagmap = {
    'folder_not_found': PaperFolderCreateError._folder_not_found_validator,
    'invalid_folder_id': PaperFolderCreateError._invalid_folder_id_validator,
PaperFolderCreateError._tagmap.update(PaperApiBaseError._tagmap)
PaperFolderCreateError.folder_not_found = PaperFolderCreateError('folder_not_found')
PaperFolderCreateError.invalid_folder_id = PaperFolderCreateError('invalid_folder_id')
PaperFolderCreateResult.folder_id.validator = bv.String()
PaperFolderCreateResult._all_field_names_ = set(['folder_id'])
PaperFolderCreateResult._all_fields_ = [('folder_id', PaperFolderCreateResult.folder_id.validator)]
RemovePaperDocUser.member.validator = sharing.MemberSelector_validator
RemovePaperDocUser._all_field_names_ = RefPaperDoc._all_field_names_.union(set(['member']))
RemovePaperDocUser._all_fields_ = RefPaperDoc._all_fields_ + [('member', RemovePaperDocUser.member.validator)]
SharingPolicy.public_sharing_policy.validator = bv.Nullable(SharingPublicPolicyType_validator)
SharingPolicy.team_sharing_policy.validator = bv.Nullable(SharingTeamPolicyType_validator)
SharingPolicy._all_field_names_ = set([
    'public_sharing_policy',
    'team_sharing_policy',
SharingPolicy._all_fields_ = [
    ('public_sharing_policy', SharingPolicy.public_sharing_policy.validator),
    ('team_sharing_policy', SharingPolicy.team_sharing_policy.validator),
SharingTeamPolicyType._people_with_link_can_edit_validator = bv.Void()
SharingTeamPolicyType._people_with_link_can_view_and_comment_validator = bv.Void()
SharingTeamPolicyType._invite_only_validator = bv.Void()
SharingTeamPolicyType._tagmap = {
    'people_with_link_can_edit': SharingTeamPolicyType._people_with_link_can_edit_validator,
    'people_with_link_can_view_and_comment': SharingTeamPolicyType._people_with_link_can_view_and_comment_validator,
    'invite_only': SharingTeamPolicyType._invite_only_validator,
SharingTeamPolicyType.people_with_link_can_edit = SharingTeamPolicyType('people_with_link_can_edit')
SharingTeamPolicyType.people_with_link_can_view_and_comment = SharingTeamPolicyType('people_with_link_can_view_and_comment')
SharingTeamPolicyType.invite_only = SharingTeamPolicyType('invite_only')
SharingPublicPolicyType._disabled_validator = bv.Void()
SharingPublicPolicyType._tagmap = {
    'disabled': SharingPublicPolicyType._disabled_validator,
SharingPublicPolicyType._tagmap.update(SharingTeamPolicyType._tagmap)
SharingPublicPolicyType.disabled = SharingPublicPolicyType('disabled')
UserInfoWithPermissionLevel.user.validator = sharing.UserInfo_validator
UserInfoWithPermissionLevel.permission_level.validator = PaperDocPermissionLevel_validator
UserInfoWithPermissionLevel._all_field_names_ = set([
UserInfoWithPermissionLevel._all_fields_ = [
    ('user', UserInfoWithPermissionLevel.user.validator),
    ('permission_level', UserInfoWithPermissionLevel.permission_level.validator),
UserOnPaperDocFilter._visited_validator = bv.Void()
UserOnPaperDocFilter._shared_validator = bv.Void()
UserOnPaperDocFilter._other_validator = bv.Void()
UserOnPaperDocFilter._tagmap = {
    'visited': UserOnPaperDocFilter._visited_validator,
    'shared': UserOnPaperDocFilter._shared_validator,
    'other': UserOnPaperDocFilter._other_validator,
UserOnPaperDocFilter.visited = UserOnPaperDocFilter('visited')
UserOnPaperDocFilter.shared = UserOnPaperDocFilter('shared')
UserOnPaperDocFilter.other = UserOnPaperDocFilter('other')
AddMember.permission_level.default = PaperDocPermissionLevel.edit
AddPaperDocUser.quiet.default = False
ListPaperDocsArgs.filter_by.default = ListPaperDocsFilterBy.docs_accessed
ListPaperDocsArgs.sort_by.default = ListPaperDocsSortBy.accessed
ListPaperDocsArgs.sort_order.default = ListPaperDocsSortOrder.ascending
ListPaperDocsArgs.limit.default = 1000
ListUsersOnFolderArgs.limit.default = 1000
ListUsersOnPaperDocArgs.limit.default = 1000
ListUsersOnPaperDocArgs.filter_by.default = UserOnPaperDocFilter.shared
docs_archive = bb.Route(
    'docs/archive',
    RefPaperDoc_validator,
    DocLookupError_validator,
docs_create = bb.Route(
    'docs/create',
    PaperDocCreateArgs_validator,
    PaperDocCreateUpdateResult_validator,
    PaperDocCreateError_validator,
docs_download = bb.Route(
    'docs/download',
    PaperDocExport_validator,
    PaperDocExportResult_validator,
docs_folder_users_list = bb.Route(
    'docs/folder_users/list',
    ListUsersOnFolderArgs_validator,
    ListUsersOnFolderResponse_validator,
docs_folder_users_list_continue = bb.Route(
    'docs/folder_users/list/continue',
    ListUsersOnFolderContinueArgs_validator,
    ListUsersCursorError_validator,
docs_get_folder_info = bb.Route(
    'docs/get_folder_info',
    FoldersContainingPaperDoc_validator,
docs_list = bb.Route(
    'docs/list',
    ListPaperDocsArgs_validator,
    ListPaperDocsResponse_validator,
docs_list_continue = bb.Route(
    'docs/list/continue',
    ListPaperDocsContinueArgs_validator,
    ListDocsCursorError_validator,
docs_permanently_delete = bb.Route(
    'docs/permanently_delete',
docs_sharing_policy_get = bb.Route(
    'docs/sharing_policy/get',
    SharingPolicy_validator,
docs_sharing_policy_set = bb.Route(
    'docs/sharing_policy/set',
    PaperDocSharingPolicy_validator,
docs_update = bb.Route(
    'docs/update',
    PaperDocUpdateArgs_validator,
    PaperDocUpdateError_validator,
docs_users_add = bb.Route(
    'docs/users/add',
    AddPaperDocUser_validator,
    bv.List(AddPaperDocUserMemberResult_validator),
docs_users_list = bb.Route(
    'docs/users/list',
    ListUsersOnPaperDocArgs_validator,
    ListUsersOnPaperDocResponse_validator,
docs_users_list_continue = bb.Route(
    'docs/users/list/continue',
    ListUsersOnPaperDocContinueArgs_validator,
docs_users_remove = bb.Route(
    'docs/users/remove',
    RemovePaperDocUser_validator,
folders_create = bb.Route(
    'folders/create',
    PaperFolderCreateArg_validator,
    PaperFolderCreateResult_validator,
    PaperFolderCreateError_validator,
    'docs/archive': docs_archive,
    'docs/create': docs_create,
    'docs/download': docs_download,
    'docs/folder_users/list': docs_folder_users_list,
    'docs/folder_users/list/continue': docs_folder_users_list_continue,
    'docs/get_folder_info': docs_get_folder_info,
    'docs/list': docs_list,
    'docs/list/continue': docs_list_continue,
    'docs/permanently_delete': docs_permanently_delete,
    'docs/sharing_policy/get': docs_sharing_policy_get,
    'docs/sharing_policy/set': docs_sharing_policy_set,
    'docs/update': docs_update,
    'docs/users/add': docs_users_add,
    'docs/users/list': docs_users_list,
    'docs/users/list/continue': docs_users_list_continue,
    'docs/users/remove': docs_users_remove,
    'folders/create': folders_create,
