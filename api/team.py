class DeviceSession(bb.Struct):
    :ivar team.DeviceSession.session_id: The session id.
    :ivar team.DeviceSession.ip_address: The IP address of the last activity
        from this session.
    :ivar team.DeviceSession.country: The country from which the last activity
        from this session was made.
    :ivar team.DeviceSession.created: The time this session was created.
    :ivar team.DeviceSession.updated: The time of the last activity from this
        '_ip_address_value',
        '_country_value',
        '_updated_value',
                 ip_address=None,
                 country=None,
                 updated=None):
        self._ip_address_value = bb.NOT_SET
        self._country_value = bb.NOT_SET
        self._updated_value = bb.NOT_SET
        if ip_address is not None:
            self.ip_address = ip_address
        if country is not None:
            self.country = country
        if updated is not None:
            self.updated = updated
    ip_address = bb.Attribute("ip_address", nullable=True)
    country = bb.Attribute("country", nullable=True)
    updated = bb.Attribute("updated", nullable=True)
        super(DeviceSession, self)._process_custom_annotations(annotation_type, field_path, processor)
DeviceSession_validator = bv.Struct(DeviceSession)
class ActiveWebSession(DeviceSession):
    Information on active web sessions.
    :ivar team.ActiveWebSession.user_agent: Information on the hosting device.
    :ivar team.ActiveWebSession.os: Information on the hosting operating system.
    :ivar team.ActiveWebSession.browser: Information on the browser used for
        this web session.
    :ivar team.ActiveWebSession.expires: The time this session expires.
        '_user_agent_value',
        '_os_value',
        '_browser_value',
                 os=None,
                 browser=None,
                 updated=None,
        super(ActiveWebSession, self).__init__(session_id,
                                               ip_address,
                                               updated)
        self._user_agent_value = bb.NOT_SET
        self._os_value = bb.NOT_SET
        self._browser_value = bb.NOT_SET
        if user_agent is not None:
        if os is not None:
            self.os = os
        if browser is not None:
            self.browser = browser
    user_agent = bb.Attribute("user_agent")
    os = bb.Attribute("os")
    browser = bb.Attribute("browser")
        super(ActiveWebSession, self)._process_custom_annotations(annotation_type, field_path, processor)
ActiveWebSession_validator = bv.Struct(ActiveWebSession)
class AddSecondaryEmailResult(bb.Union):
    Result of trying to add a secondary email to a user. 'success' is the only
    value indicating that a secondary email was successfully added to a user.
    The other values explain the type of error that occurred, and include the
    email for which the error occurred.
    :ivar secondary_emails.SecondaryEmail team.AddSecondaryEmailResult.success:
        Describes a secondary email that was successfully added to a user.
    :ivar str team.AddSecondaryEmailResult.unavailable: Secondary email is not
        available to be claimed by the user.
    :ivar str team.AddSecondaryEmailResult.already_pending: Secondary email is
        already a pending email for the user.
    :ivar str team.AddSecondaryEmailResult.already_owned_by_user: Secondary
        email is already a verified email for the user.
    :ivar str team.AddSecondaryEmailResult.reached_limit: User already has the
        maximum number of secondary emails allowed.
    :ivar str team.AddSecondaryEmailResult.transient_error: A transient error
        occurred. Please try again later.
    :ivar str team.AddSecondaryEmailResult.too_many_updates: An error occurred
        due to conflicting updates. Please try again later.
    :ivar str team.AddSecondaryEmailResult.unknown_error: An unknown error
    :ivar str team.AddSecondaryEmailResult.rate_limited: Too many emails are
        being sent to this email address. Please try again later.
        :param secondary_emails.SecondaryEmail val:
        :rtype: AddSecondaryEmailResult
    def unavailable(cls, val):
        Create an instance of this class set to the ``unavailable`` tag with
        return cls('unavailable', val)
    def already_pending(cls, val):
        Create an instance of this class set to the ``already_pending`` tag with
        return cls('already_pending', val)
    def already_owned_by_user(cls, val):
        Create an instance of this class set to the ``already_owned_by_user``
        return cls('already_owned_by_user', val)
    def reached_limit(cls, val):
        Create an instance of this class set to the ``reached_limit`` tag with
        return cls('reached_limit', val)
    def transient_error(cls, val):
        Create an instance of this class set to the ``transient_error`` tag with
        return cls('transient_error', val)
    def too_many_updates(cls, val):
        Create an instance of this class set to the ``too_many_updates`` tag
        return cls('too_many_updates', val)
    def unknown_error(cls, val):
        Create an instance of this class set to the ``unknown_error`` tag with
        return cls('unknown_error', val)
    def rate_limited(cls, val):
        Create an instance of this class set to the ``rate_limited`` tag with
        return cls('rate_limited', val)
    def is_unavailable(self):
        Check if the union tag is ``unavailable``.
        return self._tag == 'unavailable'
    def is_already_pending(self):
        Check if the union tag is ``already_pending``.
        return self._tag == 'already_pending'
    def is_already_owned_by_user(self):
        Check if the union tag is ``already_owned_by_user``.
        return self._tag == 'already_owned_by_user'
    def is_reached_limit(self):
        Check if the union tag is ``reached_limit``.
        return self._tag == 'reached_limit'
    def is_too_many_updates(self):
        Check if the union tag is ``too_many_updates``.
        return self._tag == 'too_many_updates'
    def is_rate_limited(self):
        Check if the union tag is ``rate_limited``.
        return self._tag == 'rate_limited'
        :rtype: secondary_emails.SecondaryEmail
    def get_unavailable(self):
        Secondary email is not available to be claimed by the user.
        Only call this if :meth:`is_unavailable` is true.
        if not self.is_unavailable():
            raise AttributeError("tag 'unavailable' not set")
    def get_already_pending(self):
        Secondary email is already a pending email for the user.
        Only call this if :meth:`is_already_pending` is true.
        if not self.is_already_pending():
            raise AttributeError("tag 'already_pending' not set")
    def get_already_owned_by_user(self):
        Secondary email is already a verified email for the user.
        Only call this if :meth:`is_already_owned_by_user` is true.
        if not self.is_already_owned_by_user():
            raise AttributeError("tag 'already_owned_by_user' not set")
    def get_reached_limit(self):
        User already has the maximum number of secondary emails allowed.
        Only call this if :meth:`is_reached_limit` is true.
        if not self.is_reached_limit():
            raise AttributeError("tag 'reached_limit' not set")
    def get_transient_error(self):
        A transient error occurred. Please try again later.
        Only call this if :meth:`is_transient_error` is true.
        if not self.is_transient_error():
            raise AttributeError("tag 'transient_error' not set")
    def get_too_many_updates(self):
        An error occurred due to conflicting updates. Please try again later.
        Only call this if :meth:`is_too_many_updates` is true.
        if not self.is_too_many_updates():
            raise AttributeError("tag 'too_many_updates' not set")
    def get_unknown_error(self):
        An unknown error occurred.
        Only call this if :meth:`is_unknown_error` is true.
        if not self.is_unknown_error():
            raise AttributeError("tag 'unknown_error' not set")
    def get_rate_limited(self):
        Too many emails are being sent to this email address. Please try again
        later.
        Only call this if :meth:`is_rate_limited` is true.
        if not self.is_rate_limited():
            raise AttributeError("tag 'rate_limited' not set")
        super(AddSecondaryEmailResult, self)._process_custom_annotations(annotation_type, field_path, processor)
AddSecondaryEmailResult_validator = bv.Union(AddSecondaryEmailResult)
class AddSecondaryEmailsArg(bb.Struct):
    :ivar team.AddSecondaryEmailsArg.new_secondary_emails: List of users and
        secondary emails to add.
        '_new_secondary_emails_value',
                 new_secondary_emails=None):
        self._new_secondary_emails_value = bb.NOT_SET
        if new_secondary_emails is not None:
            self.new_secondary_emails = new_secondary_emails
    # Instance attribute type: list of [UserSecondaryEmailsArg] (validator is set below)
    new_secondary_emails = bb.Attribute("new_secondary_emails")
        super(AddSecondaryEmailsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
AddSecondaryEmailsArg_validator = bv.Struct(AddSecondaryEmailsArg)
class AddSecondaryEmailsError(bb.Union):
    Error returned when adding secondary emails fails.
    :ivar team.AddSecondaryEmailsError.secondary_emails_disabled: Secondary
        emails are disabled for the team.
    :ivar team.AddSecondaryEmailsError.too_many_emails: A maximum of 20
        secondary emails can be added in a single call.
    secondary_emails_disabled = None
    too_many_emails = None
    def is_secondary_emails_disabled(self):
        Check if the union tag is ``secondary_emails_disabled``.
        return self._tag == 'secondary_emails_disabled'
    def is_too_many_emails(self):
        Check if the union tag is ``too_many_emails``.
        return self._tag == 'too_many_emails'
        super(AddSecondaryEmailsError, self)._process_custom_annotations(annotation_type, field_path, processor)
AddSecondaryEmailsError_validator = bv.Union(AddSecondaryEmailsError)
class AddSecondaryEmailsResult(bb.Struct):
    :ivar team.AddSecondaryEmailsResult.results: List of users and secondary
        email results.
        '_results_value',
                 results=None):
        self._results_value = bb.NOT_SET
        if results is not None:
            self.results = results
    # Instance attribute type: list of [UserAddResult] (validator is set below)
    results = bb.Attribute("results")
        super(AddSecondaryEmailsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
AddSecondaryEmailsResult_validator = bv.Struct(AddSecondaryEmailsResult)
class AdminTier(bb.Union):
    Describes which team-related admin permissions a user has.
    :ivar team.AdminTier.team_admin: User is an administrator of the team - has
        all permissions.
    :ivar team.AdminTier.user_management_admin: User can do most user
        provisioning, de-provisioning and management.
    :ivar team.AdminTier.support_admin: User can do a limited set of common
        support tasks for existing users. Note: Dropbox is adding new types of
        admin roles; these may display as support_admin.
    :ivar team.AdminTier.member_only: User is not an admin of the team.
    team_admin = None
    user_management_admin = None
    support_admin = None
    member_only = None
    def is_team_admin(self):
        Check if the union tag is ``team_admin``.
        return self._tag == 'team_admin'
    def is_user_management_admin(self):
        Check if the union tag is ``user_management_admin``.
        return self._tag == 'user_management_admin'
    def is_support_admin(self):
        Check if the union tag is ``support_admin``.
        return self._tag == 'support_admin'
    def is_member_only(self):
        Check if the union tag is ``member_only``.
        return self._tag == 'member_only'
        super(AdminTier, self)._process_custom_annotations(annotation_type, field_path, processor)
AdminTier_validator = bv.Union(AdminTier)
class ApiApp(bb.Struct):
    Information on linked third party applications.
    :ivar team.ApiApp.app_id: The application unique id.
    :ivar team.ApiApp.app_name: The application name.
    :ivar team.ApiApp.publisher: The application publisher name.
    :ivar team.ApiApp.publisher_url: The publisher's URL.
    :ivar team.ApiApp.linked: The time this application was linked.
    :ivar team.ApiApp.is_app_folder: Whether the linked application uses a
        dedicated folder.
        '_app_id_value',
        '_app_name_value',
        '_publisher_value',
        '_publisher_url_value',
        '_linked_value',
        '_is_app_folder_value',
                 app_id=None,
                 app_name=None,
                 is_app_folder=None,
                 publisher=None,
                 publisher_url=None,
                 linked=None):
        self._app_id_value = bb.NOT_SET
        self._app_name_value = bb.NOT_SET
        self._publisher_value = bb.NOT_SET
        self._publisher_url_value = bb.NOT_SET
        self._linked_value = bb.NOT_SET
        self._is_app_folder_value = bb.NOT_SET
        if app_id is not None:
            self.app_id = app_id
        if app_name is not None:
            self.app_name = app_name
        if publisher is not None:
            self.publisher = publisher
        if publisher_url is not None:
            self.publisher_url = publisher_url
        if linked is not None:
            self.linked = linked
        if is_app_folder is not None:
            self.is_app_folder = is_app_folder
    app_id = bb.Attribute("app_id")
    app_name = bb.Attribute("app_name")
    publisher = bb.Attribute("publisher", nullable=True)
    publisher_url = bb.Attribute("publisher_url", nullable=True)
    linked = bb.Attribute("linked", nullable=True)
    is_app_folder = bb.Attribute("is_app_folder")
        super(ApiApp, self)._process_custom_annotations(annotation_type, field_path, processor)
ApiApp_validator = bv.Struct(ApiApp)
class BaseDfbReport(bb.Struct):
    Base report structure.
    :ivar team.BaseDfbReport.start_date: First date present in the results as
        'YYYY-MM-DD' or None.
        '_start_date_value',
                 start_date=None):
        self._start_date_value = bb.NOT_SET
        if start_date is not None:
            self.start_date = start_date
    start_date = bb.Attribute("start_date")
        super(BaseDfbReport, self)._process_custom_annotations(annotation_type, field_path, processor)
BaseDfbReport_validator = bv.Struct(BaseDfbReport)
class BaseTeamFolderError(bb.Union):
    Base error that all errors for existing team folders should extend.
        :param TeamFolderAccessError val:
        :rtype: BaseTeamFolderError
    def status_error(cls, val):
        Create an instance of this class set to the ``status_error`` tag with
        :param TeamFolderInvalidStatusError val:
        return cls('status_error', val)
    def team_shared_dropbox_error(cls, val):
        ``team_shared_dropbox_error`` tag with value ``val``.
        :param TeamFolderTeamSharedDropboxError val:
        return cls('team_shared_dropbox_error', val)
    def is_status_error(self):
        Check if the union tag is ``status_error``.
        return self._tag == 'status_error'
    def is_team_shared_dropbox_error(self):
        Check if the union tag is ``team_shared_dropbox_error``.
        return self._tag == 'team_shared_dropbox_error'
        :rtype: TeamFolderAccessError
    def get_status_error(self):
        Only call this if :meth:`is_status_error` is true.
        :rtype: TeamFolderInvalidStatusError
        if not self.is_status_error():
            raise AttributeError("tag 'status_error' not set")
    def get_team_shared_dropbox_error(self):
        Only call this if :meth:`is_team_shared_dropbox_error` is true.
        :rtype: TeamFolderTeamSharedDropboxError
        if not self.is_team_shared_dropbox_error():
            raise AttributeError("tag 'team_shared_dropbox_error' not set")
        super(BaseTeamFolderError, self)._process_custom_annotations(annotation_type, field_path, processor)
BaseTeamFolderError_validator = bv.Union(BaseTeamFolderError)
class CustomQuotaError(bb.Union):
    Error returned when getting member custom quota.
    :ivar team.CustomQuotaError.too_many_users: A maximum of 1000 users can be
        set for a single call.
    too_many_users = None
    def is_too_many_users(self):
        Check if the union tag is ``too_many_users``.
        return self._tag == 'too_many_users'
        super(CustomQuotaError, self)._process_custom_annotations(annotation_type, field_path, processor)
CustomQuotaError_validator = bv.Union(CustomQuotaError)
class CustomQuotaResult(bb.Union):
    User custom quota.
    :ivar UserCustomQuotaResult CustomQuotaResult.success: User's custom quota.
    :ivar UserSelectorArg CustomQuotaResult.invalid_user: Invalid user (not in
        team).
        :param UserCustomQuotaResult val:
        :rtype: CustomQuotaResult
    def invalid_user(cls, val):
        Create an instance of this class set to the ``invalid_user`` tag with
        :param UserSelectorArg val:
        return cls('invalid_user', val)
    def is_invalid_user(self):
        Check if the union tag is ``invalid_user``.
        return self._tag == 'invalid_user'
        User's custom quota.
        :rtype: UserCustomQuotaResult
    def get_invalid_user(self):
        Invalid user (not in team).
        Only call this if :meth:`is_invalid_user` is true.
        :rtype: UserSelectorArg
        if not self.is_invalid_user():
            raise AttributeError("tag 'invalid_user' not set")
        super(CustomQuotaResult, self)._process_custom_annotations(annotation_type, field_path, processor)
CustomQuotaResult_validator = bv.Union(CustomQuotaResult)
class CustomQuotaUsersArg(bb.Struct):
    :ivar team.CustomQuotaUsersArg.users: List of users.
    # Instance attribute type: list of [UserSelectorArg] (validator is set below)
        super(CustomQuotaUsersArg, self)._process_custom_annotations(annotation_type, field_path, processor)
CustomQuotaUsersArg_validator = bv.Struct(CustomQuotaUsersArg)
class DateRange(bb.Struct):
    Input arguments that can be provided for most reports.
    :ivar team.DateRange.start_date: Optional starting date (inclusive). If
        start_date is None or too long ago, this field will  be set to 6 months
        ago.
    :ivar team.DateRange.end_date: Optional ending date (exclusive).
        '_end_date_value',
        self._end_date_value = bb.NOT_SET
        if end_date is not None:
            self.end_date = end_date
    start_date = bb.Attribute("start_date", nullable=True)
    end_date = bb.Attribute("end_date", nullable=True)
        super(DateRange, self)._process_custom_annotations(annotation_type, field_path, processor)
DateRange_validator = bv.Struct(DateRange)
class DateRangeError(bb.Union):
    Errors that can originate from problems in input arguments to reports.
        super(DateRangeError, self)._process_custom_annotations(annotation_type, field_path, processor)
DateRangeError_validator = bv.Union(DateRangeError)
class DeleteSecondaryEmailResult(bb.Union):
    Result of trying to delete a secondary email address. 'success' is the only
    value indicating that a secondary email was successfully deleted. The other
    values explain the type of error that occurred, and include the email for
    which the error occurred.
    :ivar str team.DeleteSecondaryEmailResult.success: The secondary email was
        successfully deleted.
    :ivar str team.DeleteSecondaryEmailResult.not_found: The email address was
        not found for the user.
    :ivar str team.DeleteSecondaryEmailResult.cannot_remove_primary: The email
        address is the primary email address of the user, and cannot be removed.
        :rtype: DeleteSecondaryEmailResult
    def not_found(cls, val):
        Create an instance of this class set to the ``not_found`` tag with value
        return cls('not_found', val)
    def cannot_remove_primary(cls, val):
        Create an instance of this class set to the ``cannot_remove_primary``
        return cls('cannot_remove_primary', val)
    def is_cannot_remove_primary(self):
        Check if the union tag is ``cannot_remove_primary``.
        return self._tag == 'cannot_remove_primary'
        The secondary email was successfully deleted.
    def get_not_found(self):
        The email address was not found for the user.
        Only call this if :meth:`is_not_found` is true.
        if not self.is_not_found():
            raise AttributeError("tag 'not_found' not set")
    def get_cannot_remove_primary(self):
        The email address is the primary email address of the user, and cannot
        be removed.
        Only call this if :meth:`is_cannot_remove_primary` is true.
        if not self.is_cannot_remove_primary():
            raise AttributeError("tag 'cannot_remove_primary' not set")
        super(DeleteSecondaryEmailResult, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteSecondaryEmailResult_validator = bv.Union(DeleteSecondaryEmailResult)
class DeleteSecondaryEmailsArg(bb.Struct):
    :ivar team.DeleteSecondaryEmailsArg.emails_to_delete: List of users and
        their secondary emails to delete.
        '_emails_to_delete_value',
                 emails_to_delete=None):
        self._emails_to_delete_value = bb.NOT_SET
        if emails_to_delete is not None:
            self.emails_to_delete = emails_to_delete
    emails_to_delete = bb.Attribute("emails_to_delete")
        super(DeleteSecondaryEmailsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteSecondaryEmailsArg_validator = bv.Struct(DeleteSecondaryEmailsArg)
class DeleteSecondaryEmailsResult(bb.Struct):
    # Instance attribute type: list of [UserDeleteResult] (validator is set below)
        super(DeleteSecondaryEmailsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteSecondaryEmailsResult_validator = bv.Struct(DeleteSecondaryEmailsResult)
class DesktopClientSession(DeviceSession):
    Information about linked Dropbox desktop client sessions.
    :ivar team.DesktopClientSession.host_name: Name of the hosting desktop.
    :ivar team.DesktopClientSession.client_type: The Dropbox desktop client
        type.
    :ivar team.DesktopClientSession.client_version: The Dropbox client version.
    :ivar team.DesktopClientSession.platform: Information on the hosting
    :ivar team.DesktopClientSession.is_delete_on_unlink_supported: Whether it's
        possible to delete all of the account files upon unlinking.
        '_host_name_value',
        '_client_type_value',
        '_client_version_value',
        '_platform_value',
        '_is_delete_on_unlink_supported_value',
                 host_name=None,
                 client_type=None,
                 client_version=None,
                 platform=None,
                 is_delete_on_unlink_supported=None,
        super(DesktopClientSession, self).__init__(session_id,
        self._host_name_value = bb.NOT_SET
        self._client_type_value = bb.NOT_SET
        self._client_version_value = bb.NOT_SET
        self._platform_value = bb.NOT_SET
        self._is_delete_on_unlink_supported_value = bb.NOT_SET
        if host_name is not None:
            self.host_name = host_name
        if client_type is not None:
            self.client_type = client_type
        if client_version is not None:
            self.client_version = client_version
        if platform is not None:
            self.platform = platform
        if is_delete_on_unlink_supported is not None:
            self.is_delete_on_unlink_supported = is_delete_on_unlink_supported
    host_name = bb.Attribute("host_name")
    # Instance attribute type: DesktopPlatform (validator is set below)
    client_type = bb.Attribute("client_type", user_defined=True)
    client_version = bb.Attribute("client_version")
    platform = bb.Attribute("platform")
    is_delete_on_unlink_supported = bb.Attribute("is_delete_on_unlink_supported")
        super(DesktopClientSession, self)._process_custom_annotations(annotation_type, field_path, processor)
DesktopClientSession_validator = bv.Struct(DesktopClientSession)
class DesktopPlatform(bb.Union):
    :ivar team.DesktopPlatform.windows: Official Windows Dropbox desktop client.
    :ivar team.DesktopPlatform.mac: Official Mac Dropbox desktop client.
    :ivar team.DesktopPlatform.linux: Official Linux Dropbox desktop client.
    windows = None
    mac = None
    linux = None
    def is_windows(self):
        Check if the union tag is ``windows``.
        return self._tag == 'windows'
    def is_mac(self):
        Check if the union tag is ``mac``.
        return self._tag == 'mac'
    def is_linux(self):
        Check if the union tag is ``linux``.
        return self._tag == 'linux'
        super(DesktopPlatform, self)._process_custom_annotations(annotation_type, field_path, processor)
DesktopPlatform_validator = bv.Union(DesktopPlatform)
class DeviceSessionArg(bb.Struct):
    :ivar team.DeviceSessionArg.session_id: The session id.
    :ivar team.DeviceSessionArg.team_member_id: The unique id of the member
        owning the device.
    team_member_id = bb.Attribute("team_member_id")
        super(DeviceSessionArg, self)._process_custom_annotations(annotation_type, field_path, processor)
DeviceSessionArg_validator = bv.Struct(DeviceSessionArg)
class DevicesActive(bb.Struct):
    Each of the items is an array of values, one value per day. The value is the
    number of devices active within a time window, ending with that day. If
    there is no data for a day, then the value will be None.
    :ivar team.DevicesActive.windows: Array of number of linked windows
        (desktop) clients with activity.
    :ivar team.DevicesActive.macos: Array of number of linked mac (desktop)
        clients with activity.
    :ivar team.DevicesActive.linux: Array of number of linked linus (desktop)
    :ivar team.DevicesActive.ios: Array of number of linked ios devices with
        activity.
    :ivar team.DevicesActive.android: Array of number of linked android devices
        with activity.
    :ivar team.DevicesActive.other: Array of number of other linked devices
        (blackberry, windows phone, etc)  with activity.
    :ivar team.DevicesActive.total: Array of total number of linked clients with
        '_windows_value',
        '_macos_value',
        '_linux_value',
        '_ios_value',
        '_android_value',
        '_other_value',
        '_total_value',
                 windows=None,
                 macos=None,
                 linux=None,
                 ios=None,
                 android=None,
                 other=None,
                 total=None):
        self._windows_value = bb.NOT_SET
        self._macos_value = bb.NOT_SET
        self._linux_value = bb.NOT_SET
        self._ios_value = bb.NOT_SET
        self._android_value = bb.NOT_SET
        self._other_value = bb.NOT_SET
        self._total_value = bb.NOT_SET
        if windows is not None:
            self.windows = windows
        if macos is not None:
            self.macos = macos
        if linux is not None:
            self.linux = linux
        if ios is not None:
            self.ios = ios
        if android is not None:
            self.android = android
        if other is not None:
            self.other = other
        if total is not None:
            self.total = total
    # Instance attribute type: list of [Optional[int]] (validator is set below)
    windows = bb.Attribute("windows")
    macos = bb.Attribute("macos")
    linux = bb.Attribute("linux")
    ios = bb.Attribute("ios")
    android = bb.Attribute("android")
    other = bb.Attribute("other")
    total = bb.Attribute("total")
        super(DevicesActive, self)._process_custom_annotations(annotation_type, field_path, processor)
DevicesActive_validator = bv.Struct(DevicesActive)
class ExcludedUsersListArg(bb.Struct):
    Excluded users list argument.
    :ivar team.ExcludedUsersListArg.limit: Number of results to return per call.
        super(ExcludedUsersListArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ExcludedUsersListArg_validator = bv.Struct(ExcludedUsersListArg)
class ExcludedUsersListContinueArg(bb.Struct):
    Excluded users list continue argument.
    :ivar team.ExcludedUsersListContinueArg.cursor: Indicates from what point to
        get the next set of users.
        super(ExcludedUsersListContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ExcludedUsersListContinueArg_validator = bv.Struct(ExcludedUsersListContinueArg)
class ExcludedUsersListContinueError(bb.Union):
    Excluded users list continue error.
    :ivar team.ExcludedUsersListContinueError.invalid_cursor: The cursor is
        super(ExcludedUsersListContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
ExcludedUsersListContinueError_validator = bv.Union(ExcludedUsersListContinueError)
class ExcludedUsersListError(bb.Union):
    Excluded users list error.
    :ivar team.ExcludedUsersListError.list_error: An error occurred.
    list_error = None
    def is_list_error(self):
        Check if the union tag is ``list_error``.
        return self._tag == 'list_error'
        super(ExcludedUsersListError, self)._process_custom_annotations(annotation_type, field_path, processor)
ExcludedUsersListError_validator = bv.Union(ExcludedUsersListError)
class ExcludedUsersListResult(bb.Struct):
    Excluded users list result.
    :ivar team.ExcludedUsersListResult.cursor: Pass the cursor into
        :meth:`dropbox.dropbox_client.Dropbox.team_member_space_limits_excluded_users_list_continue`
        to obtain additional excluded users.
    :ivar team.ExcludedUsersListResult.has_more: Is true if there are additional
        excluded users that have not been returned yet. An additional call to
        can retrieve them.
    # Instance attribute type: list of [MemberProfile] (validator is set below)
        super(ExcludedUsersListResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ExcludedUsersListResult_validator = bv.Struct(ExcludedUsersListResult)
class ExcludedUsersUpdateArg(bb.Struct):
    Argument of excluded users update operation. Should include a list of users
    to add/remove (according to endpoint), Maximum size of the list is 1000
    :ivar team.ExcludedUsersUpdateArg.users: List of users to be added/removed.
    users = bb.Attribute("users", nullable=True)
        super(ExcludedUsersUpdateArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ExcludedUsersUpdateArg_validator = bv.Struct(ExcludedUsersUpdateArg)
class ExcludedUsersUpdateError(bb.Union):
    Excluded users update error.
    :ivar team.ExcludedUsersUpdateError.users_not_in_team: At least one of the
        users is not part of your team.
    :ivar team.ExcludedUsersUpdateError.too_many_users: A maximum of 1000 users
        for each of addition/removal can be supplied.
    users_not_in_team = None
    def is_users_not_in_team(self):
        Check if the union tag is ``users_not_in_team``.
        return self._tag == 'users_not_in_team'
        super(ExcludedUsersUpdateError, self)._process_custom_annotations(annotation_type, field_path, processor)
ExcludedUsersUpdateError_validator = bv.Union(ExcludedUsersUpdateError)
class ExcludedUsersUpdateResult(bb.Struct):
    Excluded users update result.
    :ivar team.ExcludedUsersUpdateResult.status: Update status.
        '_status_value',
                 status=None):
        self._status_value = bb.NOT_SET
        if status is not None:
            self.status = status
    # Instance attribute type: ExcludedUsersUpdateStatus (validator is set below)
    status = bb.Attribute("status", user_defined=True)
        super(ExcludedUsersUpdateResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ExcludedUsersUpdateResult_validator = bv.Struct(ExcludedUsersUpdateResult)
class ExcludedUsersUpdateStatus(bb.Union):
    Excluded users update operation status.
    :ivar team.ExcludedUsersUpdateStatus.success: Update successful.
        super(ExcludedUsersUpdateStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
ExcludedUsersUpdateStatus_validator = bv.Union(ExcludedUsersUpdateStatus)
class Feature(bb.Union):
    A set of features that a Dropbox Business account may support.
    :ivar team.Feature.upload_api_rate_limit: The number of upload API calls
        allowed per month.
    :ivar team.Feature.has_team_shared_dropbox: Does this team have a shared
        team root.
    :ivar team.Feature.has_team_file_events: Does this team have file events.
    :ivar team.Feature.has_team_selective_sync: Does this team have team
        selective sync enabled.
    upload_api_rate_limit = None
    has_team_shared_dropbox = None
    has_team_file_events = None
    has_team_selective_sync = None
    def is_upload_api_rate_limit(self):
        Check if the union tag is ``upload_api_rate_limit``.
        return self._tag == 'upload_api_rate_limit'
    def is_has_team_shared_dropbox(self):
        Check if the union tag is ``has_team_shared_dropbox``.
        return self._tag == 'has_team_shared_dropbox'
    def is_has_team_file_events(self):
        Check if the union tag is ``has_team_file_events``.
        return self._tag == 'has_team_file_events'
    def is_has_team_selective_sync(self):
        Check if the union tag is ``has_team_selective_sync``.
        return self._tag == 'has_team_selective_sync'
        super(Feature, self)._process_custom_annotations(annotation_type, field_path, processor)
Feature_validator = bv.Union(Feature)
class FeatureValue(bb.Union):
    The values correspond to entries in :class:`Feature`. You may get different
    value according to your Dropbox Business plan.
    def upload_api_rate_limit(cls, val):
        Create an instance of this class set to the ``upload_api_rate_limit``
        :param UploadApiRateLimitValue val:
        :rtype: FeatureValue
        return cls('upload_api_rate_limit', val)
    def has_team_shared_dropbox(cls, val):
        Create an instance of this class set to the ``has_team_shared_dropbox``
        :param HasTeamSharedDropboxValue val:
        return cls('has_team_shared_dropbox', val)
    def has_team_file_events(cls, val):
        Create an instance of this class set to the ``has_team_file_events`` tag
        :param HasTeamFileEventsValue val:
        return cls('has_team_file_events', val)
    def has_team_selective_sync(cls, val):
        Create an instance of this class set to the ``has_team_selective_sync``
        :param HasTeamSelectiveSyncValue val:
        return cls('has_team_selective_sync', val)
    def get_upload_api_rate_limit(self):
        Only call this if :meth:`is_upload_api_rate_limit` is true.
        :rtype: UploadApiRateLimitValue
        if not self.is_upload_api_rate_limit():
            raise AttributeError("tag 'upload_api_rate_limit' not set")
    def get_has_team_shared_dropbox(self):
        Only call this if :meth:`is_has_team_shared_dropbox` is true.
        :rtype: HasTeamSharedDropboxValue
        if not self.is_has_team_shared_dropbox():
            raise AttributeError("tag 'has_team_shared_dropbox' not set")
    def get_has_team_file_events(self):
        Only call this if :meth:`is_has_team_file_events` is true.
        :rtype: HasTeamFileEventsValue
        if not self.is_has_team_file_events():
            raise AttributeError("tag 'has_team_file_events' not set")
    def get_has_team_selective_sync(self):
        Only call this if :meth:`is_has_team_selective_sync` is true.
        :rtype: HasTeamSelectiveSyncValue
        if not self.is_has_team_selective_sync():
            raise AttributeError("tag 'has_team_selective_sync' not set")
        super(FeatureValue, self)._process_custom_annotations(annotation_type, field_path, processor)
FeatureValue_validator = bv.Union(FeatureValue)
class FeaturesGetValuesBatchArg(bb.Struct):
    :ivar team.FeaturesGetValuesBatchArg.features: A list of features in
        :class:`Feature`. If the list is empty, this route will return
        :class:`FeaturesGetValuesBatchError`.
        '_features_value',
                 features=None):
        self._features_value = bb.NOT_SET
        if features is not None:
            self.features = features
    # Instance attribute type: list of [Feature] (validator is set below)
    features = bb.Attribute("features")
        super(FeaturesGetValuesBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
FeaturesGetValuesBatchArg_validator = bv.Struct(FeaturesGetValuesBatchArg)
class FeaturesGetValuesBatchError(bb.Union):
    :ivar team.FeaturesGetValuesBatchError.empty_features_list: At least one
        :class:`Feature` must be included in the
        :class:`FeaturesGetValuesBatchArg`.features list.
    empty_features_list = None
    def is_empty_features_list(self):
        Check if the union tag is ``empty_features_list``.
        return self._tag == 'empty_features_list'
        super(FeaturesGetValuesBatchError, self)._process_custom_annotations(annotation_type, field_path, processor)
FeaturesGetValuesBatchError_validator = bv.Union(FeaturesGetValuesBatchError)
class FeaturesGetValuesBatchResult(bb.Struct):
        '_values_value',
                 values=None):
        self._values_value = bb.NOT_SET
        if values is not None:
            self.values = values
    # Instance attribute type: list of [FeatureValue] (validator is set below)
    values = bb.Attribute("values")
        super(FeaturesGetValuesBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
FeaturesGetValuesBatchResult_validator = bv.Struct(FeaturesGetValuesBatchResult)
class GetActivityReport(BaseDfbReport):
    Activity Report Result. Each of the items in the storage report is an array
    of values, one value per day. If there is no data for a day, then the value
    will be None.
    :ivar team.GetActivityReport.adds: Array of total number of adds by team
    :ivar team.GetActivityReport.edits: Array of number of edits by team
        members. If the same user edits the same file multiple times this is
        counted as a single edit.
    :ivar team.GetActivityReport.deletes: Array of total number of deletes by
    :ivar team.GetActivityReport.active_users_28_day: Array of the number of
        users who have been active in the last 28 days.
    :ivar team.GetActivityReport.active_users_7_day: Array of the number of
        users who have been active in the last week.
    :ivar team.GetActivityReport.active_users_1_day: Array of the number of
        users who have been active in the last day.
    :ivar team.GetActivityReport.active_shared_folders_28_day: Array of the
        number of shared folders with some activity in the last 28 days.
    :ivar team.GetActivityReport.active_shared_folders_7_day: Array of the
        number of shared folders with some activity in the last week.
    :ivar team.GetActivityReport.active_shared_folders_1_day: Array of the
        number of shared folders with some activity in the last day.
    :ivar team.GetActivityReport.shared_links_created: Array of the number of
        shared links created.
    :ivar team.GetActivityReport.shared_links_viewed_by_team: Array of the
        number of views by team users to shared links created by the team.
    :ivar team.GetActivityReport.shared_links_viewed_by_outside_user: Array of
        the number of views by users outside of the team to shared links created
        by the team.
    :ivar team.GetActivityReport.shared_links_viewed_by_not_logged_in: Array of
        the number of views by non-logged-in users to shared links created by
        the team.
    :ivar team.GetActivityReport.shared_links_viewed_total: Array of the total
        number of views to shared links created by the team.
        '_adds_value',
        '_edits_value',
        '_deletes_value',
        '_active_users_28_day_value',
        '_active_users_7_day_value',
        '_active_users_1_day_value',
        '_active_shared_folders_28_day_value',
        '_active_shared_folders_7_day_value',
        '_active_shared_folders_1_day_value',
        '_shared_links_created_value',
        '_shared_links_viewed_by_team_value',
        '_shared_links_viewed_by_outside_user_value',
        '_shared_links_viewed_by_not_logged_in_value',
        '_shared_links_viewed_total_value',
                 adds=None,
                 edits=None,
                 deletes=None,
                 active_users_28_day=None,
                 active_users_7_day=None,
                 active_users_1_day=None,
                 active_shared_folders_28_day=None,
                 active_shared_folders_7_day=None,
                 active_shared_folders_1_day=None,
                 shared_links_created=None,
                 shared_links_viewed_by_team=None,
                 shared_links_viewed_by_outside_user=None,
                 shared_links_viewed_by_not_logged_in=None,
                 shared_links_viewed_total=None):
        super(GetActivityReport, self).__init__(start_date)
        self._adds_value = bb.NOT_SET
        self._edits_value = bb.NOT_SET
        self._deletes_value = bb.NOT_SET
        self._active_users_28_day_value = bb.NOT_SET
        self._active_users_7_day_value = bb.NOT_SET
        self._active_users_1_day_value = bb.NOT_SET
        self._active_shared_folders_28_day_value = bb.NOT_SET
        self._active_shared_folders_7_day_value = bb.NOT_SET
        self._active_shared_folders_1_day_value = bb.NOT_SET
        self._shared_links_created_value = bb.NOT_SET
        self._shared_links_viewed_by_team_value = bb.NOT_SET
        self._shared_links_viewed_by_outside_user_value = bb.NOT_SET
        self._shared_links_viewed_by_not_logged_in_value = bb.NOT_SET
        self._shared_links_viewed_total_value = bb.NOT_SET
        if adds is not None:
            self.adds = adds
        if edits is not None:
            self.edits = edits
        if deletes is not None:
            self.deletes = deletes
        if active_users_28_day is not None:
            self.active_users_28_day = active_users_28_day
        if active_users_7_day is not None:
            self.active_users_7_day = active_users_7_day
        if active_users_1_day is not None:
            self.active_users_1_day = active_users_1_day
        if active_shared_folders_28_day is not None:
            self.active_shared_folders_28_day = active_shared_folders_28_day
        if active_shared_folders_7_day is not None:
            self.active_shared_folders_7_day = active_shared_folders_7_day
        if active_shared_folders_1_day is not None:
            self.active_shared_folders_1_day = active_shared_folders_1_day
        if shared_links_created is not None:
            self.shared_links_created = shared_links_created
        if shared_links_viewed_by_team is not None:
            self.shared_links_viewed_by_team = shared_links_viewed_by_team
        if shared_links_viewed_by_outside_user is not None:
            self.shared_links_viewed_by_outside_user = shared_links_viewed_by_outside_user
        if shared_links_viewed_by_not_logged_in is not None:
            self.shared_links_viewed_by_not_logged_in = shared_links_viewed_by_not_logged_in
        if shared_links_viewed_total is not None:
            self.shared_links_viewed_total = shared_links_viewed_total
    adds = bb.Attribute("adds")
    edits = bb.Attribute("edits")
    deletes = bb.Attribute("deletes")
    active_users_28_day = bb.Attribute("active_users_28_day")
    active_users_7_day = bb.Attribute("active_users_7_day")
    active_users_1_day = bb.Attribute("active_users_1_day")
    active_shared_folders_28_day = bb.Attribute("active_shared_folders_28_day")
    active_shared_folders_7_day = bb.Attribute("active_shared_folders_7_day")
    active_shared_folders_1_day = bb.Attribute("active_shared_folders_1_day")
    shared_links_created = bb.Attribute("shared_links_created")
    shared_links_viewed_by_team = bb.Attribute("shared_links_viewed_by_team")
    shared_links_viewed_by_outside_user = bb.Attribute("shared_links_viewed_by_outside_user")
    shared_links_viewed_by_not_logged_in = bb.Attribute("shared_links_viewed_by_not_logged_in")
    shared_links_viewed_total = bb.Attribute("shared_links_viewed_total")
        super(GetActivityReport, self)._process_custom_annotations(annotation_type, field_path, processor)
GetActivityReport_validator = bv.Struct(GetActivityReport)
class GetDevicesReport(BaseDfbReport):
    Devices Report Result. Contains subsections for different time ranges of
    activity. Each of the items in each subsection of the storage report is an
    array of values, one value per day. If there is no data for a day, then the
    value will be None.
    :ivar team.GetDevicesReport.active_1_day: Report of the number of devices
        active in the last day.
    :ivar team.GetDevicesReport.active_7_day: Report of the number of devices
        active in the last 7 days.
    :ivar team.GetDevicesReport.active_28_day: Report of the number of devices
        active in the last 28 days.
        '_active_1_day_value',
        '_active_7_day_value',
        '_active_28_day_value',
                 active_1_day=None,
                 active_7_day=None,
                 active_28_day=None):
        super(GetDevicesReport, self).__init__(start_date)
        self._active_1_day_value = bb.NOT_SET
        self._active_7_day_value = bb.NOT_SET
        self._active_28_day_value = bb.NOT_SET
        if active_1_day is not None:
            self.active_1_day = active_1_day
        if active_7_day is not None:
            self.active_7_day = active_7_day
        if active_28_day is not None:
            self.active_28_day = active_28_day
    # Instance attribute type: DevicesActive (validator is set below)
    active_1_day = bb.Attribute("active_1_day", user_defined=True)
    active_7_day = bb.Attribute("active_7_day", user_defined=True)
    active_28_day = bb.Attribute("active_28_day", user_defined=True)
        super(GetDevicesReport, self)._process_custom_annotations(annotation_type, field_path, processor)
GetDevicesReport_validator = bv.Struct(GetDevicesReport)
class GetMembershipReport(BaseDfbReport):
    Membership Report Result. Each of the items in the storage report is an
    :ivar team.GetMembershipReport.team_size: Team size, for each day.
    :ivar team.GetMembershipReport.pending_invites: The number of pending
        invites to the team, for each day.
    :ivar team.GetMembershipReport.members_joined: The number of members that
        joined the team, for each day.
    :ivar team.GetMembershipReport.suspended_members: The number of suspended
        team members, for each day.
    :ivar team.GetMembershipReport.licenses: The total number of licenses the
        team has, for each day.
        '_team_size_value',
        '_pending_invites_value',
        '_members_joined_value',
        '_suspended_members_value',
        '_licenses_value',
                 team_size=None,
                 pending_invites=None,
                 members_joined=None,
                 suspended_members=None,
                 licenses=None):
        super(GetMembershipReport, self).__init__(start_date)
        self._team_size_value = bb.NOT_SET
        self._pending_invites_value = bb.NOT_SET
        self._members_joined_value = bb.NOT_SET
        self._suspended_members_value = bb.NOT_SET
        self._licenses_value = bb.NOT_SET
        if team_size is not None:
            self.team_size = team_size
        if pending_invites is not None:
            self.pending_invites = pending_invites
        if members_joined is not None:
            self.members_joined = members_joined
        if suspended_members is not None:
            self.suspended_members = suspended_members
        if licenses is not None:
            self.licenses = licenses
    team_size = bb.Attribute("team_size")
    pending_invites = bb.Attribute("pending_invites")
    members_joined = bb.Attribute("members_joined")
    suspended_members = bb.Attribute("suspended_members")
    licenses = bb.Attribute("licenses")
        super(GetMembershipReport, self)._process_custom_annotations(annotation_type, field_path, processor)
GetMembershipReport_validator = bv.Struct(GetMembershipReport)
class GetStorageReport(BaseDfbReport):
    Storage Report Result. Each of the items in the storage report is an array
    :ivar team.GetStorageReport.total_usage: Sum of the shared, unshared, and
        datastore usages, for each day.
    :ivar team.GetStorageReport.shared_usage: Array of the combined size (bytes)
        of team members' shared folders, for each day.
    :ivar team.GetStorageReport.unshared_usage: Array of the combined size
        (bytes) of team members' root namespaces, for each day.
    :ivar team.GetStorageReport.shared_folders: Array of the number of shared
        folders owned by team members, for each day.
    :ivar team.GetStorageReport.member_storage_map: Array of storage summaries
        of team members' account sizes. Each storage summary is an array of key,
        value pairs, where each pair describes a storage bucket. The key
        indicates the upper bound of the bucket and the value is the number of
        users in that bucket. There is one such summary per day. If there is no
        data for a day, the storage summary will be empty.
        '_total_usage_value',
        '_shared_usage_value',
        '_unshared_usage_value',
        '_shared_folders_value',
        '_member_storage_map_value',
                 total_usage=None,
                 shared_usage=None,
                 unshared_usage=None,
                 shared_folders=None,
                 member_storage_map=None):
        super(GetStorageReport, self).__init__(start_date)
        self._total_usage_value = bb.NOT_SET
        self._shared_usage_value = bb.NOT_SET
        self._unshared_usage_value = bb.NOT_SET
        self._shared_folders_value = bb.NOT_SET
        self._member_storage_map_value = bb.NOT_SET
        if total_usage is not None:
            self.total_usage = total_usage
        if shared_usage is not None:
            self.shared_usage = shared_usage
        if unshared_usage is not None:
            self.unshared_usage = unshared_usage
        if shared_folders is not None:
            self.shared_folders = shared_folders
        if member_storage_map is not None:
            self.member_storage_map = member_storage_map
    total_usage = bb.Attribute("total_usage")
    shared_usage = bb.Attribute("shared_usage")
    unshared_usage = bb.Attribute("unshared_usage")
    shared_folders = bb.Attribute("shared_folders")
    # Instance attribute type: list of [list of [StorageBucket]] (validator is set below)
    member_storage_map = bb.Attribute("member_storage_map")
        super(GetStorageReport, self)._process_custom_annotations(annotation_type, field_path, processor)
GetStorageReport_validator = bv.Struct(GetStorageReport)
class GroupAccessType(bb.Union):
    Role of a user in group.
    :ivar team.GroupAccessType.member: User is a member of the group, but has no
        special permissions.
    :ivar team.GroupAccessType.owner: User can rename the group, and add/remove
    member = None
    def is_member(self):
        Check if the union tag is ``member``.
        return self._tag == 'member'
        super(GroupAccessType, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupAccessType_validator = bv.Union(GroupAccessType)
class GroupCreateArg(bb.Struct):
    :ivar team.GroupCreateArg.group_name: Group name.
    :ivar team.GroupCreateArg.add_creator_as_owner: Automatically add the
        creator of the group.
    :ivar team.GroupCreateArg.group_external_id: The creator of a team can
    :ivar team.GroupCreateArg.group_management_type: Whether the team can be
        managed by selected users, or only by team admins.
        '_group_name_value',
        '_add_creator_as_owner_value',
        '_group_external_id_value',
        '_group_management_type_value',
                 add_creator_as_owner=None,
        self._group_name_value = bb.NOT_SET
        self._add_creator_as_owner_value = bb.NOT_SET
        self._group_external_id_value = bb.NOT_SET
        self._group_management_type_value = bb.NOT_SET
        if group_name is not None:
            self.group_name = group_name
        if add_creator_as_owner is not None:
            self.add_creator_as_owner = add_creator_as_owner
        if group_external_id is not None:
            self.group_external_id = group_external_id
        if group_management_type is not None:
            self.group_management_type = group_management_type
    group_name = bb.Attribute("group_name")
    add_creator_as_owner = bb.Attribute("add_creator_as_owner")
    group_external_id = bb.Attribute("group_external_id", nullable=True)
    # Instance attribute type: team_common.GroupManagementType (validator is set below)
    group_management_type = bb.Attribute("group_management_type", nullable=True, user_defined=True)
        super(GroupCreateArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupCreateArg_validator = bv.Struct(GroupCreateArg)
class GroupCreateError(bb.Union):
    :ivar team.GroupCreateError.group_name_already_used: The requested group
        name is already being used by another group.
    :ivar team.GroupCreateError.group_name_invalid: Group name is empty or has
        invalid characters.
    :ivar team.GroupCreateError.external_id_already_in_use: The requested
        external ID is already being used by another group.
    :ivar team.GroupCreateError.system_managed_group_disallowed: System-managed
        group cannot be manually created.
    group_name_already_used = None
    group_name_invalid = None
    external_id_already_in_use = None
    system_managed_group_disallowed = None
    def is_group_name_already_used(self):
        Check if the union tag is ``group_name_already_used``.
        return self._tag == 'group_name_already_used'
    def is_group_name_invalid(self):
        Check if the union tag is ``group_name_invalid``.
        return self._tag == 'group_name_invalid'
    def is_external_id_already_in_use(self):
        Check if the union tag is ``external_id_already_in_use``.
        return self._tag == 'external_id_already_in_use'
    def is_system_managed_group_disallowed(self):
        Check if the union tag is ``system_managed_group_disallowed``.
        return self._tag == 'system_managed_group_disallowed'
        super(GroupCreateError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupCreateError_validator = bv.Union(GroupCreateError)
class GroupSelectorError(bb.Union):
    Error that can be raised when :class:`GroupSelector` is used.
    :ivar team.GroupSelectorError.group_not_found: No matching group found. No
        groups match the specified group ID.
    group_not_found = None
    def is_group_not_found(self):
        Check if the union tag is ``group_not_found``.
        return self._tag == 'group_not_found'
        super(GroupSelectorError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupSelectorError_validator = bv.Union(GroupSelectorError)
class GroupSelectorWithTeamGroupError(GroupSelectorError):
    Error that can be raised when :class:`GroupSelector` is used and team groups
    are disallowed from being used.
    :ivar team.GroupSelectorWithTeamGroupError.system_managed_group_disallowed:
        This operation is not supported on system-managed groups.
        super(GroupSelectorWithTeamGroupError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupSelectorWithTeamGroupError_validator = bv.Union(GroupSelectorWithTeamGroupError)
class GroupDeleteError(GroupSelectorWithTeamGroupError):
    :ivar team.GroupDeleteError.group_already_deleted: This group has already
        been deleted.
    group_already_deleted = None
    def is_group_already_deleted(self):
        Check if the union tag is ``group_already_deleted``.
        return self._tag == 'group_already_deleted'
        super(GroupDeleteError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupDeleteError_validator = bv.Union(GroupDeleteError)
class GroupFullInfo(team_common.GroupSummary):
    Full description of a group.
    :ivar team.GroupFullInfo.members: List of group members.
    :ivar team.GroupFullInfo.created: The group creation time as a UTC timestamp
        in milliseconds since the Unix epoch.
                 member_count=None,
        super(GroupFullInfo, self).__init__(group_name,
    # Instance attribute type: list of [GroupMemberInfo] (validator is set below)
    members = bb.Attribute("members", nullable=True)
        super(GroupFullInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupFullInfo_validator = bv.Struct(GroupFullInfo)
class GroupMemberInfo(bb.Struct):
    Profile of group member, and role in group.
    :ivar team.GroupMemberInfo.profile: Profile of group member.
    :ivar team.GroupMemberInfo.access_type: The role that the user has in the
        '_profile_value',
                 profile=None,
        self._profile_value = bb.NOT_SET
            self.profile = profile
    # Instance attribute type: MemberProfile (validator is set below)
    profile = bb.Attribute("profile", user_defined=True)
    # Instance attribute type: GroupAccessType (validator is set below)
        super(GroupMemberInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMemberInfo_validator = bv.Struct(GroupMemberInfo)
class GroupMemberSelector(bb.Struct):
    Argument for selecting a group and a single user.
    :ivar team.GroupMemberSelector.group: Specify a group.
    :ivar team.GroupMemberSelector.user: Identity of a user that is a member of
        ``group``.
    # Instance attribute type: GroupSelector (validator is set below)
    # Instance attribute type: UserSelectorArg (validator is set below)
        super(GroupMemberSelector, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMemberSelector_validator = bv.Struct(GroupMemberSelector)
class GroupMemberSelectorError(GroupSelectorWithTeamGroupError):
    Error that can be raised when :class:`GroupMemberSelector` is used, and the
    user is required to be a member of the specified group.
    :ivar team.GroupMemberSelectorError.member_not_in_group: The specified user
        is not a member of this group.
    member_not_in_group = None
    def is_member_not_in_group(self):
        Check if the union tag is ``member_not_in_group``.
        return self._tag == 'member_not_in_group'
        super(GroupMemberSelectorError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMemberSelectorError_validator = bv.Union(GroupMemberSelectorError)
class GroupMemberSetAccessTypeError(GroupMemberSelectorError):
        team.GroupMemberSetAccessTypeError.user_cannot_be_manager_of_company_managed_group:
        A company managed group cannot be managed by a user.
    user_cannot_be_manager_of_company_managed_group = None
    def is_user_cannot_be_manager_of_company_managed_group(self):
        Check if the union tag is ``user_cannot_be_manager_of_company_managed_group``.
        return self._tag == 'user_cannot_be_manager_of_company_managed_group'
        super(GroupMemberSetAccessTypeError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMemberSetAccessTypeError_validator = bv.Union(GroupMemberSetAccessTypeError)
class IncludeMembersArg(bb.Struct):
    :ivar team.IncludeMembersArg.return_members: Whether to return the list of
        members in the group.  Note that the default value will cause all the
        group members  to be returned in the response. This may take a long time
        for large groups.
        '_return_members_value',
                 return_members=None):
        self._return_members_value = bb.NOT_SET
        if return_members is not None:
            self.return_members = return_members
    return_members = bb.Attribute("return_members")
        super(IncludeMembersArg, self)._process_custom_annotations(annotation_type, field_path, processor)
IncludeMembersArg_validator = bv.Struct(IncludeMembersArg)
class GroupMembersAddArg(IncludeMembersArg):
    :ivar team.GroupMembersAddArg.group: Group to which users will be added.
    :ivar team.GroupMembersAddArg.members: List of users to be added to the
        super(GroupMembersAddArg, self).__init__(return_members)
    # Instance attribute type: list of [MemberAccess] (validator is set below)
        super(GroupMembersAddArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMembersAddArg_validator = bv.Struct(GroupMembersAddArg)
class GroupMembersAddError(GroupSelectorWithTeamGroupError):
    :ivar team.GroupMembersAddError.duplicate_user: You cannot add duplicate
        users. One or more of the members you are trying to add is already a
        member of the group.
    :ivar team.GroupMembersAddError.group_not_in_team: Group is not in this
        team. You cannot add members to a group that is outside of your team.
    :ivar list of [str] team.GroupMembersAddError.members_not_in_team: These
        members are not part of your team. Currently, you cannot add members to
        a group if they are not part of your team, though this may change in a
        subsequent version. To add new members to your Dropbox Business team,
        use the :route:`members/add` endpoint.
    :ivar list of [str] team.GroupMembersAddError.users_not_found: These users
        were not found in Dropbox.
    :ivar team.GroupMembersAddError.user_must_be_active_to_be_owner: A suspended
        user cannot be added to a group as ``GroupAccessType.owner``.
    :ivar list of [str]
        team.GroupMembersAddError.user_cannot_be_manager_of_company_managed_group:
        A company-managed group cannot be managed by a user.
    duplicate_user = None
    group_not_in_team = None
    user_must_be_active_to_be_owner = None
    def members_not_in_team(cls, val):
        Create an instance of this class set to the ``members_not_in_team`` tag
        :rtype: GroupMembersAddError
        return cls('members_not_in_team', val)
    def users_not_found(cls, val):
        Create an instance of this class set to the ``users_not_found`` tag with
        return cls('users_not_found', val)
    def user_cannot_be_manager_of_company_managed_group(cls, val):
        ``user_cannot_be_manager_of_company_managed_group`` tag with value
        return cls('user_cannot_be_manager_of_company_managed_group', val)
    def is_duplicate_user(self):
        Check if the union tag is ``duplicate_user``.
        return self._tag == 'duplicate_user'
    def is_group_not_in_team(self):
        Check if the union tag is ``group_not_in_team``.
        return self._tag == 'group_not_in_team'
    def is_members_not_in_team(self):
        Check if the union tag is ``members_not_in_team``.
        return self._tag == 'members_not_in_team'
    def is_users_not_found(self):
        Check if the union tag is ``users_not_found``.
        return self._tag == 'users_not_found'
    def is_user_must_be_active_to_be_owner(self):
        Check if the union tag is ``user_must_be_active_to_be_owner``.
        return self._tag == 'user_must_be_active_to_be_owner'
    def get_members_not_in_team(self):
        These members are not part of your team. Currently, you cannot add
        members to a group if they are not part of your team, though this may
        change in a subsequent version. To add new members to your Dropbox
        Business team, use the
        :meth:`dropbox.dropbox_client.Dropbox.team_members_add` endpoint.
        Only call this if :meth:`is_members_not_in_team` is true.
        if not self.is_members_not_in_team():
            raise AttributeError("tag 'members_not_in_team' not set")
    def get_users_not_found(self):
        These users were not found in Dropbox.
        Only call this if :meth:`is_users_not_found` is true.
        if not self.is_users_not_found():
            raise AttributeError("tag 'users_not_found' not set")
    def get_user_cannot_be_manager_of_company_managed_group(self):
        Only call this if :meth:`is_user_cannot_be_manager_of_company_managed_group` is true.
        if not self.is_user_cannot_be_manager_of_company_managed_group():
            raise AttributeError("tag 'user_cannot_be_manager_of_company_managed_group' not set")
        super(GroupMembersAddError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMembersAddError_validator = bv.Union(GroupMembersAddError)
class GroupMembersChangeResult(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.team_groups_members_add` and
    :meth:`dropbox.dropbox_client.Dropbox.team_groups_members_remove`.
    :ivar team.GroupMembersChangeResult.group_info: The group info after member
        change operation has been performed.
    :ivar team.GroupMembersChangeResult.async_job_id: For legacy purposes
        async_job_id will always return one space ' '. Formerly, it was an ID
        that was used to obtain the status of granting/revoking group-owned
        resources. It's no longer necessary because the async processing now
        '_group_info_value',
                 group_info=None,
        self._group_info_value = bb.NOT_SET
        if group_info is not None:
            self.group_info = group_info
    # Instance attribute type: GroupFullInfo (validator is set below)
    group_info = bb.Attribute("group_info", user_defined=True)
        super(GroupMembersChangeResult, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMembersChangeResult_validator = bv.Struct(GroupMembersChangeResult)
class GroupMembersRemoveArg(IncludeMembersArg):
    :ivar team.GroupMembersRemoveArg.group: Group from which users will be
        removed.
    :ivar team.GroupMembersRemoveArg.users: List of users to be removed from the
        super(GroupMembersRemoveArg, self).__init__(return_members)
        super(GroupMembersRemoveArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMembersRemoveArg_validator = bv.Struct(GroupMembersRemoveArg)
class GroupMembersSelectorError(GroupSelectorWithTeamGroupError):
    Error that can be raised when :class:`GroupMembersSelector` is used, and the
    users are required to be members of the specified group.
    :ivar team.GroupMembersSelectorError.member_not_in_group: At least one of
        the specified users is not a member of the group.
        super(GroupMembersSelectorError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMembersSelectorError_validator = bv.Union(GroupMembersSelectorError)
class GroupMembersRemoveError(GroupMembersSelectorError):
    :ivar team.GroupMembersRemoveError.group_not_in_team: Group is not in this
        team. You cannot remove members from a group that is outside of your
    :ivar list of [str] team.GroupMembersRemoveError.members_not_in_team: These
        members are not part of your team.
    :ivar list of [str] team.GroupMembersRemoveError.users_not_found: These
        users were not found in Dropbox.
        :rtype: GroupMembersRemoveError
        These members are not part of your team.
        super(GroupMembersRemoveError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMembersRemoveError_validator = bv.Union(GroupMembersRemoveError)
class GroupMembersSelector(bb.Struct):
    Argument for selecting a group and a list of users.
    :ivar team.GroupMembersSelector.group: Specify a group.
    :ivar team.GroupMembersSelector.users: A list of users that are members of
    # Instance attribute type: UsersSelectorArg (validator is set below)
    users = bb.Attribute("users", user_defined=True)
        super(GroupMembersSelector, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMembersSelector_validator = bv.Struct(GroupMembersSelector)
class GroupMembersSetAccessTypeArg(GroupMemberSelector):
    :ivar team.GroupMembersSetAccessTypeArg.access_type: New group access type
        the user will have.
    :ivar team.GroupMembersSetAccessTypeArg.return_members: Whether to return
        the list of members in the group.  Note that the default value will
        cause all the group members  to be returned in the response. This may
        take a long time for large groups.
        super(GroupMembersSetAccessTypeArg, self).__init__(group,
                                                           user)
        super(GroupMembersSetAccessTypeArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupMembersSetAccessTypeArg_validator = bv.Struct(GroupMembersSetAccessTypeArg)
class GroupSelector(bb.Union):
    Argument for selecting a single group, either by group_id or by external
    group ID.
    :ivar str team.GroupSelector.group_id: Group ID.
    :ivar str team.GroupSelector.group_external_id: External ID of the group.
    def group_id(cls, val):
        Create an instance of this class set to the ``group_id`` tag with value
        :rtype: GroupSelector
        return cls('group_id', val)
    def group_external_id(cls, val):
        Create an instance of this class set to the ``group_external_id`` tag
        return cls('group_external_id', val)
    def is_group_id(self):
        Check if the union tag is ``group_id``.
        return self._tag == 'group_id'
    def is_group_external_id(self):
        Check if the union tag is ``group_external_id``.
        return self._tag == 'group_external_id'
    def get_group_id(self):
        Group ID.
        Only call this if :meth:`is_group_id` is true.
        if not self.is_group_id():
            raise AttributeError("tag 'group_id' not set")
    def get_group_external_id(self):
        External ID of the group.
        Only call this if :meth:`is_group_external_id` is true.
        if not self.is_group_external_id():
            raise AttributeError("tag 'group_external_id' not set")
        super(GroupSelector, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupSelector_validator = bv.Union(GroupSelector)
class GroupUpdateArgs(IncludeMembersArg):
    :ivar team.GroupUpdateArgs.group: Specify a group.
    :ivar team.GroupUpdateArgs.new_group_name: Optional argument. Set group name
    :ivar team.GroupUpdateArgs.new_group_external_id: Optional argument. New
        group external ID. If the argument is None, the group's external_id
        won't be updated. If the argument is empty string, the group's external
        id will be cleared.
    :ivar team.GroupUpdateArgs.new_group_management_type: Set new group
        management type, if provided.
        '_new_group_name_value',
        '_new_group_external_id_value',
        '_new_group_management_type_value',
                 return_members=None,
        super(GroupUpdateArgs, self).__init__(return_members)
        self._new_group_name_value = bb.NOT_SET
        self._new_group_external_id_value = bb.NOT_SET
        self._new_group_management_type_value = bb.NOT_SET
        if new_group_name is not None:
            self.new_group_name = new_group_name
        if new_group_external_id is not None:
            self.new_group_external_id = new_group_external_id
        if new_group_management_type is not None:
            self.new_group_management_type = new_group_management_type
    new_group_name = bb.Attribute("new_group_name", nullable=True)
    new_group_external_id = bb.Attribute("new_group_external_id", nullable=True)
    new_group_management_type = bb.Attribute("new_group_management_type", nullable=True, user_defined=True)
        super(GroupUpdateArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupUpdateArgs_validator = bv.Struct(GroupUpdateArgs)
class GroupUpdateError(GroupSelectorWithTeamGroupError):
    :ivar team.GroupUpdateError.group_name_already_used: The requested group
    :ivar team.GroupUpdateError.group_name_invalid: Group name is empty or has
    :ivar team.GroupUpdateError.external_id_already_in_use: The requested
        super(GroupUpdateError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupUpdateError_validator = bv.Union(GroupUpdateError)
class GroupsGetInfoError(bb.Union):
    :ivar team.GroupsGetInfoError.group_not_on_team: The group is not on your
        super(GroupsGetInfoError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsGetInfoError_validator = bv.Union(GroupsGetInfoError)
class GroupsGetInfoItem(bb.Union):
    :ivar str team.GroupsGetInfoItem.id_not_found: An ID that was provided as a
        parameter to :route:`groups/get_info`, and did not match a corresponding
        group. The ID can be a group ID, or an external ID, depending on how the
        method was called.
    :ivar GroupFullInfo GroupsGetInfoItem.group_info: Info about a group.
    def id_not_found(cls, val):
        Create an instance of this class set to the ``id_not_found`` tag with
        :rtype: GroupsGetInfoItem
        return cls('id_not_found', val)
    def group_info(cls, val):
        Create an instance of this class set to the ``group_info`` tag with
        :param GroupFullInfo val:
        return cls('group_info', val)
    def is_id_not_found(self):
        Check if the union tag is ``id_not_found``.
        return self._tag == 'id_not_found'
    def is_group_info(self):
        Check if the union tag is ``group_info``.
        return self._tag == 'group_info'
    def get_id_not_found(self):
        An ID that was provided as a parameter to
        :meth:`dropbox.dropbox_client.Dropbox.team_groups_get_info`, and did not
        match a corresponding group. The ID can be a group ID, or an external
        ID, depending on how the method was called.
        Only call this if :meth:`is_id_not_found` is true.
        if not self.is_id_not_found():
            raise AttributeError("tag 'id_not_found' not set")
    def get_group_info(self):
        Info about a group.
        Only call this if :meth:`is_group_info` is true.
        :rtype: GroupFullInfo
        if not self.is_group_info():
            raise AttributeError("tag 'group_info' not set")
        super(GroupsGetInfoItem, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsGetInfoItem_validator = bv.Union(GroupsGetInfoItem)
class GroupsListArg(bb.Struct):
    :ivar team.GroupsListArg.limit: Number of results to return per call.
        super(GroupsListArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsListArg_validator = bv.Struct(GroupsListArg)
class GroupsListContinueArg(bb.Struct):
    :ivar team.GroupsListContinueArg.cursor: Indicates from what point to get
        the next set of groups.
        super(GroupsListContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsListContinueArg_validator = bv.Struct(GroupsListContinueArg)
class GroupsListContinueError(bb.Union):
    :ivar team.GroupsListContinueError.invalid_cursor: The cursor is invalid.
        super(GroupsListContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsListContinueError_validator = bv.Union(GroupsListContinueError)
class GroupsListResult(bb.Struct):
    :ivar team.GroupsListResult.cursor: Pass the cursor into
        :meth:`dropbox.dropbox_client.Dropbox.team_groups_list_continue` to
        obtain the additional groups.
    :ivar team.GroupsListResult.has_more: Is true if there are additional groups
        that have not been returned yet. An additional call to
        :meth:`dropbox.dropbox_client.Dropbox.team_groups_list_continue` can
    # Instance attribute type: list of [team_common.GroupSummary] (validator is set below)
        super(GroupsListResult, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsListResult_validator = bv.Struct(GroupsListResult)
class GroupsMembersListArg(bb.Struct):
    :ivar team.GroupsMembersListArg.group: The group whose members are to be
        listed.
    :ivar team.GroupsMembersListArg.limit: Number of results to return per call.
        super(GroupsMembersListArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsMembersListArg_validator = bv.Struct(GroupsMembersListArg)
class GroupsMembersListContinueArg(bb.Struct):
    :ivar team.GroupsMembersListContinueArg.cursor: Indicates from what point to
        get the next set of groups.
        super(GroupsMembersListContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsMembersListContinueArg_validator = bv.Struct(GroupsMembersListContinueArg)
class GroupsMembersListContinueError(bb.Union):
    :ivar team.GroupsMembersListContinueError.invalid_cursor: The cursor is
        super(GroupsMembersListContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsMembersListContinueError_validator = bv.Union(GroupsMembersListContinueError)
class GroupsMembersListResult(bb.Struct):
    :ivar team.GroupsMembersListResult.cursor: Pass the cursor into
        :meth:`dropbox.dropbox_client.Dropbox.team_groups_members_list_continue`
        to obtain additional group members.
    :ivar team.GroupsMembersListResult.has_more: Is true if there are additional
        group members that have not been returned yet. An additional call to
        super(GroupsMembersListResult, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsMembersListResult_validator = bv.Struct(GroupsMembersListResult)
class GroupsPollError(async_.PollError):
    :ivar team.GroupsPollError.access_denied: You are not allowed to poll this
        job.
        super(GroupsPollError, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsPollError_validator = bv.Union(GroupsPollError)
class GroupsSelector(bb.Union):
    Argument for selecting a list of groups, either by group_ids, or external
    group IDs.
    :ivar list of [str] team.GroupsSelector.group_ids: List of group IDs.
    :ivar list of [str] team.GroupsSelector.group_external_ids: List of external
        IDs of groups.
    def group_ids(cls, val):
        Create an instance of this class set to the ``group_ids`` tag with value
        :rtype: GroupsSelector
        return cls('group_ids', val)
    def group_external_ids(cls, val):
        Create an instance of this class set to the ``group_external_ids`` tag
        return cls('group_external_ids', val)
    def is_group_ids(self):
        Check if the union tag is ``group_ids``.
        return self._tag == 'group_ids'
    def is_group_external_ids(self):
        Check if the union tag is ``group_external_ids``.
        return self._tag == 'group_external_ids'
    def get_group_ids(self):
        List of group IDs.
        Only call this if :meth:`is_group_ids` is true.
        if not self.is_group_ids():
            raise AttributeError("tag 'group_ids' not set")
    def get_group_external_ids(self):
        List of external IDs of groups.
        Only call this if :meth:`is_group_external_ids` is true.
        if not self.is_group_external_ids():
            raise AttributeError("tag 'group_external_ids' not set")
        super(GroupsSelector, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupsSelector_validator = bv.Union(GroupsSelector)
class HasTeamFileEventsValue(bb.Union):
    The value for ``Feature.has_team_file_events``.
    :ivar bool team.HasTeamFileEventsValue.enabled: Does this team have file
    def enabled(cls, val):
        Create an instance of this class set to the ``enabled`` tag with value
        :param bool val:
        return cls('enabled', val)
    def get_enabled(self):
        Does this team have file events.
        Only call this if :meth:`is_enabled` is true.
        if not self.is_enabled():
            raise AttributeError("tag 'enabled' not set")
        super(HasTeamFileEventsValue, self)._process_custom_annotations(annotation_type, field_path, processor)
HasTeamFileEventsValue_validator = bv.Union(HasTeamFileEventsValue)
class HasTeamSelectiveSyncValue(bb.Union):
    The value for ``Feature.has_team_selective_sync``.
    :ivar bool team.HasTeamSelectiveSyncValue.has_team_selective_sync: Does this
        team have team selective sync enabled.
        Does this team have team selective sync enabled.
        super(HasTeamSelectiveSyncValue, self)._process_custom_annotations(annotation_type, field_path, processor)
HasTeamSelectiveSyncValue_validator = bv.Union(HasTeamSelectiveSyncValue)
class HasTeamSharedDropboxValue(bb.Union):
    The value for ``Feature.has_team_shared_dropbox``.
    :ivar bool team.HasTeamSharedDropboxValue.has_team_shared_dropbox: Does this
        team have a shared team root.
        Does this team have a shared team root.
        super(HasTeamSharedDropboxValue, self)._process_custom_annotations(annotation_type, field_path, processor)
HasTeamSharedDropboxValue_validator = bv.Union(HasTeamSharedDropboxValue)
class LegalHoldHeldRevisionMetadata(bb.Struct):
    :ivar team.LegalHoldHeldRevisionMetadata.new_filename: The held revision
        filename.
    :ivar team.LegalHoldHeldRevisionMetadata.original_revision_id: The id of the
        held revision.
    :ivar team.LegalHoldHeldRevisionMetadata.original_file_path: The original
        path of the held revision.
    :ivar team.LegalHoldHeldRevisionMetadata.server_modified: The last time the
        file was modified on Dropbox.
    :ivar team.LegalHoldHeldRevisionMetadata.author_member_id: The member id of
        the revision's author.
    :ivar team.LegalHoldHeldRevisionMetadata.author_member_status: The member
        status of the revision's author.
    :ivar team.LegalHoldHeldRevisionMetadata.author_email: The email address of
        the held revision author.
    :ivar team.LegalHoldHeldRevisionMetadata.file_type: The type of the held
        revision's file.
    :ivar team.LegalHoldHeldRevisionMetadata.size: The file size in bytes.
    :ivar team.LegalHoldHeldRevisionMetadata.content_hash: A hash of the file
        content. This field can be used to verify data integrity. For more
        information see our `Content hash
        '_new_filename_value',
        '_original_revision_id_value',
        '_original_file_path_value',
        '_author_member_id_value',
        '_author_member_status_value',
        '_author_email_value',
        '_file_type_value',
                 new_filename=None,
                 original_revision_id=None,
                 original_file_path=None,
                 author_member_id=None,
                 author_member_status=None,
                 author_email=None,
                 file_type=None,
        self._new_filename_value = bb.NOT_SET
        self._original_revision_id_value = bb.NOT_SET
        self._original_file_path_value = bb.NOT_SET
        self._author_member_id_value = bb.NOT_SET
        self._author_member_status_value = bb.NOT_SET
        self._author_email_value = bb.NOT_SET
        self._file_type_value = bb.NOT_SET
        if new_filename is not None:
            self.new_filename = new_filename
        if original_revision_id is not None:
            self.original_revision_id = original_revision_id
        if original_file_path is not None:
            self.original_file_path = original_file_path
        if author_member_id is not None:
            self.author_member_id = author_member_id
        if author_member_status is not None:
            self.author_member_status = author_member_status
        if author_email is not None:
            self.author_email = author_email
            self.file_type = file_type
    new_filename = bb.Attribute("new_filename")
    original_revision_id = bb.Attribute("original_revision_id")
    original_file_path = bb.Attribute("original_file_path")
    author_member_id = bb.Attribute("author_member_id")
    # Instance attribute type: TeamMemberStatus (validator is set below)
    author_member_status = bb.Attribute("author_member_status", user_defined=True)
    author_email = bb.Attribute("author_email")
    file_type = bb.Attribute("file_type")
    content_hash = bb.Attribute("content_hash")
        super(LegalHoldHeldRevisionMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldHeldRevisionMetadata_validator = bv.Struct(LegalHoldHeldRevisionMetadata)
class LegalHoldPolicy(bb.Struct):
    :ivar team.LegalHoldPolicy.id: The legal hold id.
    :ivar team.LegalHoldPolicy.name: Policy name.
    :ivar team.LegalHoldPolicy.description: A description of the legal hold
    :ivar team.LegalHoldPolicy.activation_time: The time at which the legal hold
        was activated.
    :ivar team.LegalHoldPolicy.members: Team members IDs and number of
        permanently deleted members under hold.
    :ivar team.LegalHoldPolicy.status: The current state of the hold.
    :ivar team.LegalHoldPolicy.start_date: Start date of the legal hold policy.
    :ivar team.LegalHoldPolicy.end_date: End date of the legal hold policy.
        '_activation_time_value',
                 status=None,
                 activation_time=None,
        self._activation_time_value = bb.NOT_SET
        if activation_time is not None:
            self.activation_time = activation_time
    activation_time = bb.Attribute("activation_time", nullable=True)
    # Instance attribute type: MembersInfo (validator is set below)
    # Instance attribute type: LegalHoldStatus (validator is set below)
        super(LegalHoldPolicy, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldPolicy_validator = bv.Struct(LegalHoldPolicy)
class LegalHoldStatus(bb.Union):
    :ivar team.LegalHoldStatus.active: The legal hold policy is active.
    :ivar team.LegalHoldStatus.released: The legal hold policy was released.
    :ivar team.LegalHoldStatus.activating: The legal hold policy is activating.
    :ivar team.LegalHoldStatus.updating: The legal hold policy is updating.
    :ivar team.LegalHoldStatus.exporting: The legal hold policy is exporting.
    :ivar team.LegalHoldStatus.releasing: The legal hold policy is releasing.
    released = None
    activating = None
    updating = None
    exporting = None
    releasing = None
    def is_released(self):
        Check if the union tag is ``released``.
        return self._tag == 'released'
    def is_activating(self):
        Check if the union tag is ``activating``.
        return self._tag == 'activating'
    def is_updating(self):
        Check if the union tag is ``updating``.
        return self._tag == 'updating'
    def is_exporting(self):
        Check if the union tag is ``exporting``.
        return self._tag == 'exporting'
    def is_releasing(self):
        Check if the union tag is ``releasing``.
        return self._tag == 'releasing'
        super(LegalHoldStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldStatus_validator = bv.Union(LegalHoldStatus)
class LegalHoldsError(bb.Union):
    :ivar team.LegalHoldsError.unknown_legal_hold_error: There has been an
        unknown legal hold error.
    :ivar team.LegalHoldsError.insufficient_permissions: You don't have
        permissions to perform this action.
    unknown_legal_hold_error = None
    def is_unknown_legal_hold_error(self):
        Check if the union tag is ``unknown_legal_hold_error``.
        return self._tag == 'unknown_legal_hold_error'
        super(LegalHoldsError, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsError_validator = bv.Union(LegalHoldsError)
class LegalHoldsGetPolicyArg(bb.Struct):
    :ivar team.LegalHoldsGetPolicyArg.id: The legal hold Id.
        super(LegalHoldsGetPolicyArg, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsGetPolicyArg_validator = bv.Struct(LegalHoldsGetPolicyArg)
class LegalHoldsGetPolicyError(LegalHoldsError):
    :ivar team.LegalHoldsGetPolicyError.legal_hold_policy_not_found: Legal hold
        policy does not exist for ``LegalHoldsGetPolicyArg.id``.
    legal_hold_policy_not_found = None
    def is_legal_hold_policy_not_found(self):
        Check if the union tag is ``legal_hold_policy_not_found``.
        return self._tag == 'legal_hold_policy_not_found'
        super(LegalHoldsGetPolicyError, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsGetPolicyError_validator = bv.Union(LegalHoldsGetPolicyError)
class LegalHoldsListHeldRevisionResult(bb.Struct):
    :ivar team.LegalHoldsListHeldRevisionResult.entries: List of file entries
        that under the hold.
    :ivar team.LegalHoldsListHeldRevisionResult.cursor: The cursor idicates
        where to continue reading file metadata entries for the next API call.
        When there are no more entries, the cursor will return none. Pass the
        cursor into /2/team/legal_holds/list_held_revisions/continue.
    :ivar team.LegalHoldsListHeldRevisionResult.has_more: True if there are more
        file entries that haven't been returned. You can retrieve them with a
        call to /legal_holds/list_held_revisions_continue.
    # Instance attribute type: list of [LegalHoldHeldRevisionMetadata] (validator is set below)
        super(LegalHoldsListHeldRevisionResult, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsListHeldRevisionResult_validator = bv.Struct(LegalHoldsListHeldRevisionResult)
class LegalHoldsListHeldRevisionsArg(bb.Struct):
    :ivar team.LegalHoldsListHeldRevisionsArg.id: The legal hold Id.
        super(LegalHoldsListHeldRevisionsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsListHeldRevisionsArg_validator = bv.Struct(LegalHoldsListHeldRevisionsArg)
class LegalHoldsListHeldRevisionsContinueArg(bb.Struct):
    :ivar team.LegalHoldsListHeldRevisionsContinueArg.id: The legal hold Id.
    :ivar team.LegalHoldsListHeldRevisionsContinueArg.cursor: The cursor
        idicates where to continue reading file metadata entries for the next
        API call. When there are no more entries, the cursor will return none.
        super(LegalHoldsListHeldRevisionsContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsListHeldRevisionsContinueArg_validator = bv.Struct(LegalHoldsListHeldRevisionsContinueArg)
class LegalHoldsListHeldRevisionsContinueError(bb.Union):
        team.LegalHoldsListHeldRevisionsContinueError.unknown_legal_hold_error:
        There has been an unknown legal hold error.
    :ivar team.LegalHoldsListHeldRevisionsContinueError.transient_error:
        Temporary infrastructure failure, please retry.
    :ivar team.LegalHoldsListHeldRevisionsContinueError.reset: Indicates that
        :meth:`dropbox.dropbox_client.Dropbox.team_legal_holds_list_held_revisions_continue`
        again with an empty cursor to obtain a new cursor.
        super(LegalHoldsListHeldRevisionsContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsListHeldRevisionsContinueError_validator = bv.Union(LegalHoldsListHeldRevisionsContinueError)
class LegalHoldsListHeldRevisionsError(LegalHoldsError):
    :ivar team.LegalHoldsListHeldRevisionsError.transient_error: Temporary
        infrastructure failure, please retry.
    :ivar team.LegalHoldsListHeldRevisionsError.legal_hold_still_empty: The
        legal hold is not holding any revisions yet.
    :ivar team.LegalHoldsListHeldRevisionsError.inactive_legal_hold: Trying to
        list revisions for an inactive legal hold.
    legal_hold_still_empty = None
    inactive_legal_hold = None
    def is_legal_hold_still_empty(self):
        Check if the union tag is ``legal_hold_still_empty``.
        return self._tag == 'legal_hold_still_empty'
    def is_inactive_legal_hold(self):
        Check if the union tag is ``inactive_legal_hold``.
        return self._tag == 'inactive_legal_hold'
        super(LegalHoldsListHeldRevisionsError, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsListHeldRevisionsError_validator = bv.Union(LegalHoldsListHeldRevisionsError)
class LegalHoldsListPoliciesArg(bb.Struct):
    :ivar team.LegalHoldsListPoliciesArg.include_released: Whether to return
        holds that were released.
        '_include_released_value',
                 include_released=None):
        self._include_released_value = bb.NOT_SET
        if include_released is not None:
            self.include_released = include_released
    include_released = bb.Attribute("include_released")
        super(LegalHoldsListPoliciesArg, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsListPoliciesArg_validator = bv.Struct(LegalHoldsListPoliciesArg)
class LegalHoldsListPoliciesError(LegalHoldsError):
    :ivar team.LegalHoldsListPoliciesError.transient_error: Temporary
        super(LegalHoldsListPoliciesError, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsListPoliciesError_validator = bv.Union(LegalHoldsListPoliciesError)
class LegalHoldsListPoliciesResult(bb.Struct):
        '_policies_value',
                 policies=None):
        self._policies_value = bb.NOT_SET
        if policies is not None:
            self.policies = policies
    # Instance attribute type: list of [LegalHoldPolicy] (validator is set below)
    policies = bb.Attribute("policies")
        super(LegalHoldsListPoliciesResult, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsListPoliciesResult_validator = bv.Struct(LegalHoldsListPoliciesResult)
class LegalHoldsPolicyCreateArg(bb.Struct):
    :ivar team.LegalHoldsPolicyCreateArg.name: Policy name.
    :ivar team.LegalHoldsPolicyCreateArg.description: A description of the legal
        hold policy.
    :ivar team.LegalHoldsPolicyCreateArg.members: List of team member IDs added
        to the hold.
    :ivar team.LegalHoldsPolicyCreateArg.start_date: start date of the legal
    :ivar team.LegalHoldsPolicyCreateArg.end_date: end date of the legal hold
        super(LegalHoldsPolicyCreateArg, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsPolicyCreateArg_validator = bv.Struct(LegalHoldsPolicyCreateArg)
class LegalHoldsPolicyCreateError(LegalHoldsError):
    :ivar team.LegalHoldsPolicyCreateError.start_date_is_later_than_end_date:
        Start date must be earlier than end date.
    :ivar team.LegalHoldsPolicyCreateError.empty_members_list: The users list
        must have at least one user.
    :ivar team.LegalHoldsPolicyCreateError.invalid_members: Some members in the
        members list are not valid to be placed under legal hold.
        team.LegalHoldsPolicyCreateError.number_of_users_on_hold_is_greater_than_hold_limitation:
        You cannot add more than 5 users in a legal hold.
    :ivar team.LegalHoldsPolicyCreateError.transient_error: Temporary
    :ivar team.LegalHoldsPolicyCreateError.name_must_be_unique: The name
        provided is already in use by another legal hold.
    :ivar team.LegalHoldsPolicyCreateError.team_exceeded_legal_hold_quota: Team
        exceeded legal hold quota.
    :ivar team.LegalHoldsPolicyCreateError.invalid_date: The provided date is
    start_date_is_later_than_end_date = None
    empty_members_list = None
    invalid_members = None
    number_of_users_on_hold_is_greater_than_hold_limitation = None
    name_must_be_unique = None
    team_exceeded_legal_hold_quota = None
    invalid_date = None
    def is_start_date_is_later_than_end_date(self):
        Check if the union tag is ``start_date_is_later_than_end_date``.
        return self._tag == 'start_date_is_later_than_end_date'
    def is_empty_members_list(self):
        Check if the union tag is ``empty_members_list``.
        return self._tag == 'empty_members_list'
    def is_invalid_members(self):
        Check if the union tag is ``invalid_members``.
        return self._tag == 'invalid_members'
    def is_number_of_users_on_hold_is_greater_than_hold_limitation(self):
        Check if the union tag is ``number_of_users_on_hold_is_greater_than_hold_limitation``.
        return self._tag == 'number_of_users_on_hold_is_greater_than_hold_limitation'
    def is_name_must_be_unique(self):
        Check if the union tag is ``name_must_be_unique``.
        return self._tag == 'name_must_be_unique'
    def is_team_exceeded_legal_hold_quota(self):
        Check if the union tag is ``team_exceeded_legal_hold_quota``.
        return self._tag == 'team_exceeded_legal_hold_quota'
    def is_invalid_date(self):
        Check if the union tag is ``invalid_date``.
        return self._tag == 'invalid_date'
        super(LegalHoldsPolicyCreateError, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsPolicyCreateError_validator = bv.Union(LegalHoldsPolicyCreateError)
class LegalHoldsPolicyReleaseArg(bb.Struct):
    :ivar team.LegalHoldsPolicyReleaseArg.id: The legal hold Id.
        super(LegalHoldsPolicyReleaseArg, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsPolicyReleaseArg_validator = bv.Struct(LegalHoldsPolicyReleaseArg)
class LegalHoldsPolicyReleaseError(LegalHoldsError):
        team.LegalHoldsPolicyReleaseError.legal_hold_performing_another_operation:
        Legal hold is currently performing another operation.
    :ivar team.LegalHoldsPolicyReleaseError.legal_hold_already_releasing: Legal
        hold is currently performing a release or is already released.
    :ivar team.LegalHoldsPolicyReleaseError.legal_hold_policy_not_found: Legal
        hold policy does not exist for ``LegalHoldsPolicyReleaseArg.id``.
    legal_hold_performing_another_operation = None
    legal_hold_already_releasing = None
    def is_legal_hold_performing_another_operation(self):
        Check if the union tag is ``legal_hold_performing_another_operation``.
        return self._tag == 'legal_hold_performing_another_operation'
    def is_legal_hold_already_releasing(self):
        Check if the union tag is ``legal_hold_already_releasing``.
        return self._tag == 'legal_hold_already_releasing'
        super(LegalHoldsPolicyReleaseError, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsPolicyReleaseError_validator = bv.Union(LegalHoldsPolicyReleaseError)
class LegalHoldsPolicyUpdateArg(bb.Struct):
    :ivar team.LegalHoldsPolicyUpdateArg.id: The legal hold Id.
    :ivar team.LegalHoldsPolicyUpdateArg.name: Policy new name.
    :ivar team.LegalHoldsPolicyUpdateArg.description: Policy new description.
    :ivar team.LegalHoldsPolicyUpdateArg.members: List of team member IDs to
        apply the policy on.
        super(LegalHoldsPolicyUpdateArg, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsPolicyUpdateArg_validator = bv.Struct(LegalHoldsPolicyUpdateArg)
class LegalHoldsPolicyUpdateError(LegalHoldsError):
    :ivar team.LegalHoldsPolicyUpdateError.transient_error: Temporary
    :ivar team.LegalHoldsPolicyUpdateError.inactive_legal_hold: Trying to
        release an inactive legal hold.
        team.LegalHoldsPolicyUpdateError.legal_hold_performing_another_operation:
    :ivar team.LegalHoldsPolicyUpdateError.invalid_members: Some members in the
        team.LegalHoldsPolicyUpdateError.number_of_users_on_hold_is_greater_than_hold_limitation:
    :ivar team.LegalHoldsPolicyUpdateError.empty_members_list: The users list
    :ivar team.LegalHoldsPolicyUpdateError.name_must_be_unique: The name
    :ivar team.LegalHoldsPolicyUpdateError.legal_hold_policy_not_found: Legal
        hold policy does not exist for ``LegalHoldsPolicyUpdateArg.id``.
        super(LegalHoldsPolicyUpdateError, self)._process_custom_annotations(annotation_type, field_path, processor)
LegalHoldsPolicyUpdateError_validator = bv.Union(LegalHoldsPolicyUpdateError)
class ListMemberAppsArg(bb.Struct):
    :ivar team.ListMemberAppsArg.team_member_id: The team member id.
        super(ListMemberAppsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMemberAppsArg_validator = bv.Struct(ListMemberAppsArg)
class ListMemberAppsError(bb.Union):
    Error returned by
    :meth:`dropbox.dropbox_client.Dropbox.team_linked_apps_list_member_linked_apps`.
    :ivar team.ListMemberAppsError.member_not_found: Member not found.
    member_not_found = None
    def is_member_not_found(self):
        Check if the union tag is ``member_not_found``.
        return self._tag == 'member_not_found'
        super(ListMemberAppsError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMemberAppsError_validator = bv.Union(ListMemberAppsError)
class ListMemberAppsResult(bb.Struct):
    :ivar team.ListMemberAppsResult.linked_api_apps: List of third party
        applications linked by this team member.
        '_linked_api_apps_value',
                 linked_api_apps=None):
        self._linked_api_apps_value = bb.NOT_SET
        if linked_api_apps is not None:
            self.linked_api_apps = linked_api_apps
    # Instance attribute type: list of [ApiApp] (validator is set below)
    linked_api_apps = bb.Attribute("linked_api_apps")
        super(ListMemberAppsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMemberAppsResult_validator = bv.Struct(ListMemberAppsResult)
class ListMemberDevicesArg(bb.Struct):
    :ivar team.ListMemberDevicesArg.team_member_id: The team's member id.
    :ivar team.ListMemberDevicesArg.include_web_sessions: Whether to list web
        sessions of the team's member.
    :ivar team.ListMemberDevicesArg.include_desktop_clients: Whether to list
        linked desktop devices of the team's member.
    :ivar team.ListMemberDevicesArg.include_mobile_clients: Whether to list
        linked mobile devices of the team's member.
        '_include_web_sessions_value',
        '_include_desktop_clients_value',
        '_include_mobile_clients_value',
                 team_member_id=None,
                 include_web_sessions=None,
                 include_desktop_clients=None,
                 include_mobile_clients=None):
        self._include_web_sessions_value = bb.NOT_SET
        self._include_desktop_clients_value = bb.NOT_SET
        self._include_mobile_clients_value = bb.NOT_SET
        if include_web_sessions is not None:
            self.include_web_sessions = include_web_sessions
        if include_desktop_clients is not None:
            self.include_desktop_clients = include_desktop_clients
        if include_mobile_clients is not None:
            self.include_mobile_clients = include_mobile_clients
    include_web_sessions = bb.Attribute("include_web_sessions")
    include_desktop_clients = bb.Attribute("include_desktop_clients")
    include_mobile_clients = bb.Attribute("include_mobile_clients")
        super(ListMemberDevicesArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMemberDevicesArg_validator = bv.Struct(ListMemberDevicesArg)
class ListMemberDevicesError(bb.Union):
    :ivar team.ListMemberDevicesError.member_not_found: Member not found.
        super(ListMemberDevicesError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMemberDevicesError_validator = bv.Union(ListMemberDevicesError)
class ListMemberDevicesResult(bb.Struct):
    :ivar team.ListMemberDevicesResult.active_web_sessions: List of web sessions
        made by this team member.
    :ivar team.ListMemberDevicesResult.desktop_client_sessions: List of desktop
        clients used by this team member.
    :ivar team.ListMemberDevicesResult.mobile_client_sessions: List of mobile
        client used by this team member.
        '_active_web_sessions_value',
        '_desktop_client_sessions_value',
        '_mobile_client_sessions_value',
                 active_web_sessions=None,
                 desktop_client_sessions=None,
                 mobile_client_sessions=None):
        self._active_web_sessions_value = bb.NOT_SET
        self._desktop_client_sessions_value = bb.NOT_SET
        self._mobile_client_sessions_value = bb.NOT_SET
        if active_web_sessions is not None:
            self.active_web_sessions = active_web_sessions
        if desktop_client_sessions is not None:
            self.desktop_client_sessions = desktop_client_sessions
        if mobile_client_sessions is not None:
            self.mobile_client_sessions = mobile_client_sessions
    # Instance attribute type: list of [ActiveWebSession] (validator is set below)
    active_web_sessions = bb.Attribute("active_web_sessions", nullable=True)
    # Instance attribute type: list of [DesktopClientSession] (validator is set below)
    desktop_client_sessions = bb.Attribute("desktop_client_sessions", nullable=True)
    # Instance attribute type: list of [MobileClientSession] (validator is set below)
    mobile_client_sessions = bb.Attribute("mobile_client_sessions", nullable=True)
        super(ListMemberDevicesResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMemberDevicesResult_validator = bv.Struct(ListMemberDevicesResult)
class ListMembersAppsArg(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.team_linked_apps_list_members_linked_apps`.
    :ivar team.ListMembersAppsArg.cursor: At the first call to the
        :meth:`dropbox.dropbox_client.Dropbox.team_linked_apps_list_members_linked_apps`
        the cursor shouldn't be passed. Then, if the result of the call includes
        a cursor, the following requests should include the received cursors in
        order to receive the next sub list of the team applications.
        super(ListMembersAppsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMembersAppsArg_validator = bv.Struct(ListMembersAppsArg)
class ListMembersAppsError(bb.Union):
    :ivar team.ListMembersAppsError.reset: Indicates that the cursor has been
        super(ListMembersAppsError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMembersAppsError_validator = bv.Union(ListMembersAppsError)
class ListMembersAppsResult(bb.Struct):
    Information returned by
    :ivar team.ListMembersAppsResult.apps: The linked applications of each
        member of the team.
    :ivar team.ListMembersAppsResult.has_more: If true, then there are more apps
        to retrieve the rest.
    :ivar team.ListMembersAppsResult.cursor: Pass the cursor into
        to receive the next sub list of team's applications.
        '_apps_value',
                 apps=None,
        self._apps_value = bb.NOT_SET
        if apps is not None:
            self.apps = apps
    # Instance attribute type: list of [MemberLinkedApps] (validator is set below)
    apps = bb.Attribute("apps")
        super(ListMembersAppsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMembersAppsResult_validator = bv.Struct(ListMembersAppsResult)
class ListMembersDevicesArg(bb.Struct):
    :ivar team.ListMembersDevicesArg.cursor: At the first call to the
        :meth:`dropbox.dropbox_client.Dropbox.team_devices_list_members_devices`
        order to receive the next sub list of team devices.
    :ivar team.ListMembersDevicesArg.include_web_sessions: Whether to list web
        sessions of the team members.
    :ivar team.ListMembersDevicesArg.include_desktop_clients: Whether to list
        desktop clients of the team members.
    :ivar team.ListMembersDevicesArg.include_mobile_clients: Whether to list
        mobile clients of the team members.
        super(ListMembersDevicesArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMembersDevicesArg_validator = bv.Struct(ListMembersDevicesArg)
class ListMembersDevicesError(bb.Union):
    :ivar team.ListMembersDevicesError.reset: Indicates that the cursor has been
        super(ListMembersDevicesError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMembersDevicesError_validator = bv.Union(ListMembersDevicesError)
class ListMembersDevicesResult(bb.Struct):
    :ivar team.ListMembersDevicesResult.devices: The devices of each member of
    :ivar team.ListMembersDevicesResult.has_more: If true, then there are more
        devices available. Pass the cursor to
    :ivar team.ListMembersDevicesResult.cursor: Pass the cursor into
        to receive the next sub list of team's devices.
        '_devices_value',
                 devices=None,
        self._devices_value = bb.NOT_SET
        if devices is not None:
            self.devices = devices
    # Instance attribute type: list of [MemberDevices] (validator is set below)
    devices = bb.Attribute("devices")
        super(ListMembersDevicesResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListMembersDevicesResult_validator = bv.Struct(ListMembersDevicesResult)
class ListTeamAppsArg(bb.Struct):
    :meth:`dropbox.dropbox_client.Dropbox.team_linked_apps_list_team_linked_apps`.
    :ivar team.ListTeamAppsArg.cursor: At the first call to the
        :meth:`dropbox.dropbox_client.Dropbox.team_linked_apps_list_team_linked_apps`
        super(ListTeamAppsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListTeamAppsArg_validator = bv.Struct(ListTeamAppsArg)
class ListTeamAppsError(bb.Union):
    :ivar team.ListTeamAppsError.reset: Indicates that the cursor has been
        super(ListTeamAppsError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListTeamAppsError_validator = bv.Union(ListTeamAppsError)
class ListTeamAppsResult(bb.Struct):
    :ivar team.ListTeamAppsResult.apps: The linked applications of each member
        of the team.
    :ivar team.ListTeamAppsResult.has_more: If true, then there are more apps
    :ivar team.ListTeamAppsResult.cursor: Pass the cursor into
        super(ListTeamAppsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListTeamAppsResult_validator = bv.Struct(ListTeamAppsResult)
class ListTeamDevicesArg(bb.Struct):
    :ivar team.ListTeamDevicesArg.cursor: At the first call to the
        :meth:`dropbox.dropbox_client.Dropbox.team_devices_list_team_devices`
    :ivar team.ListTeamDevicesArg.include_web_sessions: Whether to list web
    :ivar team.ListTeamDevicesArg.include_desktop_clients: Whether to list
    :ivar team.ListTeamDevicesArg.include_mobile_clients: Whether to list mobile
        clients of the team members.
        super(ListTeamDevicesArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ListTeamDevicesArg_validator = bv.Struct(ListTeamDevicesArg)
class ListTeamDevicesError(bb.Union):
    :ivar team.ListTeamDevicesError.reset: Indicates that the cursor has been
        super(ListTeamDevicesError, self)._process_custom_annotations(annotation_type, field_path, processor)
ListTeamDevicesError_validator = bv.Union(ListTeamDevicesError)
class ListTeamDevicesResult(bb.Struct):
    :ivar team.ListTeamDevicesResult.devices: The devices of each member of the
    :ivar team.ListTeamDevicesResult.has_more: If true, then there are more
        :meth:`dropbox.dropbox_client.Dropbox.team_devices_list_team_devices` to
    :ivar team.ListTeamDevicesResult.cursor: Pass the cursor into
        receive the next sub list of team's devices.
        super(ListTeamDevicesResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ListTeamDevicesResult_validator = bv.Struct(ListTeamDevicesResult)
class MemberAccess(bb.Struct):
    Specify access type a member should have when joined to a group.
    :ivar team.MemberAccess.user: Identity of a user.
    :ivar team.MemberAccess.access_type: Access type.
        super(MemberAccess, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberAccess_validator = bv.Struct(MemberAccess)
class MemberAddArgBase(bb.Struct):
    :ivar team.MemberAddArgBase.member_given_name: Member's first name.
    :ivar team.MemberAddArgBase.member_surname: Member's last name.
    :ivar team.MemberAddArgBase.member_external_id: External ID for member.
    :ivar team.MemberAddArgBase.member_persistent_id: Persistent ID for member.
        This field is only available to teams using persistent ID SAML
        configuration.
    :ivar team.MemberAddArgBase.send_welcome_email: Whether to send a welcome
        email to the member. If send_welcome_email is false, no email invitation
        will be sent to the user. This may be useful for apps using single
        sign-on (SSO) flows for onboarding that want to handle announcements
        themselves.
    :ivar team.MemberAddArgBase.is_directory_restricted: Whether a user is
        directory restricted.
        '_member_email_value',
        '_member_given_name_value',
        '_member_surname_value',
        '_member_external_id_value',
        '_member_persistent_id_value',
        '_send_welcome_email_value',
        '_is_directory_restricted_value',
                 member_email=None,
                 member_given_name=None,
                 member_surname=None,
                 member_external_id=None,
                 member_persistent_id=None,
                 send_welcome_email=None,
                 is_directory_restricted=None):
        self._member_email_value = bb.NOT_SET
        self._member_given_name_value = bb.NOT_SET
        self._member_surname_value = bb.NOT_SET
        self._member_external_id_value = bb.NOT_SET
        self._member_persistent_id_value = bb.NOT_SET
        self._send_welcome_email_value = bb.NOT_SET
        self._is_directory_restricted_value = bb.NOT_SET
        if member_email is not None:
            self.member_email = member_email
        if member_given_name is not None:
            self.member_given_name = member_given_name
        if member_surname is not None:
            self.member_surname = member_surname
        if member_external_id is not None:
            self.member_external_id = member_external_id
        if member_persistent_id is not None:
            self.member_persistent_id = member_persistent_id
        if send_welcome_email is not None:
            self.send_welcome_email = send_welcome_email
        if is_directory_restricted is not None:
            self.is_directory_restricted = is_directory_restricted
    member_email = bb.Attribute("member_email")
    member_given_name = bb.Attribute("member_given_name", nullable=True)
    member_surname = bb.Attribute("member_surname", nullable=True)
    member_external_id = bb.Attribute("member_external_id", nullable=True)
    member_persistent_id = bb.Attribute("member_persistent_id", nullable=True)
    send_welcome_email = bb.Attribute("send_welcome_email")
    is_directory_restricted = bb.Attribute("is_directory_restricted", nullable=True)
        super(MemberAddArgBase, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberAddArgBase_validator = bv.Struct(MemberAddArgBase)
class MemberAddArg(MemberAddArgBase):
        '_role_value',
                 is_directory_restricted=None,
                 role=None):
        super(MemberAddArg, self).__init__(member_email,
                                           member_given_name,
                                           member_surname,
                                           member_external_id,
                                           member_persistent_id,
                                           send_welcome_email,
                                           is_directory_restricted)
        self._role_value = bb.NOT_SET
        if role is not None:
            self.role = role
    # Instance attribute type: AdminTier (validator is set below)
    role = bb.Attribute("role", user_defined=True)
        super(MemberAddArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberAddArg_validator = bv.Struct(MemberAddArg)
class MemberAddResultBase(bb.Union):
    :ivar str team.MemberAddResultBase.team_license_limit: Team is already full.
        The organization has no available licenses.
    :ivar str team.MemberAddResultBase.free_team_member_limit_reached: Team is
        already full. The free team member limit has been reached.
    :ivar str team.MemberAddResultBase.user_already_on_team: User is already on
        this team. The provided email address is associated with a user who is
        already a member of (including in recoverable state) or invited to the
    :ivar str team.MemberAddResultBase.user_on_another_team: User is already on
        another team. The provided email address is associated with a user that
        is already a member or invited to another team.
    :ivar str team.MemberAddResultBase.user_already_paired: User is already
        paired.
    :ivar str team.MemberAddResultBase.user_migration_failed: User migration has
    :ivar str team.MemberAddResultBase.duplicate_external_member_id: A user with
        the given external member ID already exists on the team (including in
        recoverable state).
    :ivar str team.MemberAddResultBase.duplicate_member_persistent_id: A user
        with the given persistent ID already exists on the team (including in
    :ivar str team.MemberAddResultBase.persistent_id_disabled: Persistent ID is
        only available to teams with persistent ID SAML configuration. Please
        contact Dropbox for more information.
    :ivar str team.MemberAddResultBase.user_creation_failed: User creation has
    def team_license_limit(cls, val):
        Create an instance of this class set to the ``team_license_limit`` tag
        :rtype: MemberAddResultBase
        return cls('team_license_limit', val)
    def free_team_member_limit_reached(cls, val):
        ``free_team_member_limit_reached`` tag with value ``val``.
        return cls('free_team_member_limit_reached', val)
    def user_already_on_team(cls, val):
        Create an instance of this class set to the ``user_already_on_team`` tag
        return cls('user_already_on_team', val)
    def user_on_another_team(cls, val):
        Create an instance of this class set to the ``user_on_another_team`` tag
        return cls('user_on_another_team', val)
    def user_already_paired(cls, val):
        Create an instance of this class set to the ``user_already_paired`` tag
        return cls('user_already_paired', val)
    def user_migration_failed(cls, val):
        Create an instance of this class set to the ``user_migration_failed``
        return cls('user_migration_failed', val)
    def duplicate_external_member_id(cls, val):
        ``duplicate_external_member_id`` tag with value ``val``.
        return cls('duplicate_external_member_id', val)
    def duplicate_member_persistent_id(cls, val):
        ``duplicate_member_persistent_id`` tag with value ``val``.
        return cls('duplicate_member_persistent_id', val)
    def persistent_id_disabled(cls, val):
        Create an instance of this class set to the ``persistent_id_disabled``
        return cls('persistent_id_disabled', val)
    def user_creation_failed(cls, val):
        Create an instance of this class set to the ``user_creation_failed`` tag
        return cls('user_creation_failed', val)
    def is_team_license_limit(self):
        Check if the union tag is ``team_license_limit``.
        return self._tag == 'team_license_limit'
    def is_free_team_member_limit_reached(self):
        Check if the union tag is ``free_team_member_limit_reached``.
        return self._tag == 'free_team_member_limit_reached'
    def is_user_already_on_team(self):
        Check if the union tag is ``user_already_on_team``.
        return self._tag == 'user_already_on_team'
    def is_user_on_another_team(self):
        Check if the union tag is ``user_on_another_team``.
        return self._tag == 'user_on_another_team'
    def is_user_already_paired(self):
        Check if the union tag is ``user_already_paired``.
        return self._tag == 'user_already_paired'
    def is_user_migration_failed(self):
        Check if the union tag is ``user_migration_failed``.
        return self._tag == 'user_migration_failed'
    def is_duplicate_external_member_id(self):
        Check if the union tag is ``duplicate_external_member_id``.
        return self._tag == 'duplicate_external_member_id'
    def is_duplicate_member_persistent_id(self):
        Check if the union tag is ``duplicate_member_persistent_id``.
        return self._tag == 'duplicate_member_persistent_id'
    def is_persistent_id_disabled(self):
        Check if the union tag is ``persistent_id_disabled``.
        return self._tag == 'persistent_id_disabled'
    def is_user_creation_failed(self):
        Check if the union tag is ``user_creation_failed``.
        return self._tag == 'user_creation_failed'
    def get_team_license_limit(self):
        Team is already full. The organization has no available licenses.
        Only call this if :meth:`is_team_license_limit` is true.
        if not self.is_team_license_limit():
            raise AttributeError("tag 'team_license_limit' not set")
    def get_free_team_member_limit_reached(self):
        Team is already full. The free team member limit has been reached.
        Only call this if :meth:`is_free_team_member_limit_reached` is true.
        if not self.is_free_team_member_limit_reached():
            raise AttributeError("tag 'free_team_member_limit_reached' not set")
    def get_user_already_on_team(self):
        User is already on this team. The provided email address is associated
        with a user who is already a member of (including in recoverable state)
        or invited to the team.
        Only call this if :meth:`is_user_already_on_team` is true.
        if not self.is_user_already_on_team():
            raise AttributeError("tag 'user_already_on_team' not set")
    def get_user_on_another_team(self):
        User is already on another team. The provided email address is
        associated with a user that is already a member or invited to another
        Only call this if :meth:`is_user_on_another_team` is true.
        if not self.is_user_on_another_team():
            raise AttributeError("tag 'user_on_another_team' not set")
    def get_user_already_paired(self):
        User is already paired.
        Only call this if :meth:`is_user_already_paired` is true.
        if not self.is_user_already_paired():
            raise AttributeError("tag 'user_already_paired' not set")
    def get_user_migration_failed(self):
        User migration has failed.
        Only call this if :meth:`is_user_migration_failed` is true.
        if not self.is_user_migration_failed():
            raise AttributeError("tag 'user_migration_failed' not set")
    def get_duplicate_external_member_id(self):
        A user with the given external member ID already exists on the team
        (including in recoverable state).
        Only call this if :meth:`is_duplicate_external_member_id` is true.
        if not self.is_duplicate_external_member_id():
            raise AttributeError("tag 'duplicate_external_member_id' not set")
    def get_duplicate_member_persistent_id(self):
        A user with the given persistent ID already exists on the team
        Only call this if :meth:`is_duplicate_member_persistent_id` is true.
        if not self.is_duplicate_member_persistent_id():
            raise AttributeError("tag 'duplicate_member_persistent_id' not set")
    def get_persistent_id_disabled(self):
        Persistent ID is only available to teams with persistent ID SAML
        configuration. Please contact Dropbox for more information.
        Only call this if :meth:`is_persistent_id_disabled` is true.
        if not self.is_persistent_id_disabled():
            raise AttributeError("tag 'persistent_id_disabled' not set")
    def get_user_creation_failed(self):
        User creation has failed.
        Only call this if :meth:`is_user_creation_failed` is true.
        if not self.is_user_creation_failed():
            raise AttributeError("tag 'user_creation_failed' not set")
        super(MemberAddResultBase, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberAddResultBase_validator = bv.Union(MemberAddResultBase)
class MemberAddResult(MemberAddResultBase):
    Describes the result of attempting to add a single user to the team.
    'success' is the only value indicating that a user was indeed added to the
    team - the other values explain the type of failure that occurred, and
    include the email of the user for which the operation has failed.
    :ivar TeamMemberInfo MemberAddResult.success: Describes a user that was
        successfully added to the team.
        :param TeamMemberInfo val:
        :rtype: MemberAddResult
        Describes a user that was successfully added to the team.
        :rtype: TeamMemberInfo
        super(MemberAddResult, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberAddResult_validator = bv.Union(MemberAddResult)
class MemberAddV2Arg(MemberAddArgBase):
        '_role_ids_value',
                 role_ids=None):
        super(MemberAddV2Arg, self).__init__(member_email,
        self._role_ids_value = bb.NOT_SET
        if role_ids is not None:
            self.role_ids = role_ids
    role_ids = bb.Attribute("role_ids", nullable=True)
        super(MemberAddV2Arg, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberAddV2Arg_validator = bv.Struct(MemberAddV2Arg)
class MemberAddV2Result(MemberAddResultBase):
    :ivar TeamMemberInfoV2 MemberAddV2Result.success: Describes a user that was
        :param TeamMemberInfoV2 val:
        :rtype: MemberAddV2Result
        :rtype: TeamMemberInfoV2
        super(MemberAddV2Result, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberAddV2Result_validator = bv.Union(MemberAddV2Result)
class MemberDevices(bb.Struct):
    Information on devices of a team's member.
    :ivar team.MemberDevices.team_member_id: The member unique Id.
    :ivar team.MemberDevices.web_sessions: List of web sessions made by this
        team member.
    :ivar team.MemberDevices.desktop_clients: List of desktop clients by this
    :ivar team.MemberDevices.mobile_clients: List of mobile clients by this team
        member.
        '_web_sessions_value',
        '_desktop_clients_value',
        '_mobile_clients_value',
                 web_sessions=None,
                 desktop_clients=None,
                 mobile_clients=None):
        self._web_sessions_value = bb.NOT_SET
        self._desktop_clients_value = bb.NOT_SET
        self._mobile_clients_value = bb.NOT_SET
        if web_sessions is not None:
            self.web_sessions = web_sessions
        if desktop_clients is not None:
            self.desktop_clients = desktop_clients
        if mobile_clients is not None:
            self.mobile_clients = mobile_clients
    web_sessions = bb.Attribute("web_sessions", nullable=True)
    desktop_clients = bb.Attribute("desktop_clients", nullable=True)
    mobile_clients = bb.Attribute("mobile_clients", nullable=True)
        super(MemberDevices, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberDevices_validator = bv.Struct(MemberDevices)
class MemberLinkedApps(bb.Struct):
    Information on linked applications of a team member.
    :ivar team.MemberLinkedApps.team_member_id: The member unique Id.
    :ivar team.MemberLinkedApps.linked_api_apps: List of third party
        super(MemberLinkedApps, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberLinkedApps_validator = bv.Struct(MemberLinkedApps)
class MemberProfile(bb.Struct):
    Basic member profile.
    :ivar team.MemberProfile.team_member_id: ID of user as a member of a team.
    :ivar team.MemberProfile.external_id: External ID that a team can attach to
        the user. An application using the API may find it easier to use their
        own IDs instead of Dropbox IDs like account_id or team_member_id.
    :ivar team.MemberProfile.account_id: A user's account identifier.
    :ivar team.MemberProfile.email: Email address of user.
    :ivar team.MemberProfile.email_verified: Is true if the user's email is
        verified to be owned by the user.
    :ivar team.MemberProfile.secondary_emails: Secondary emails of a user.
    :ivar team.MemberProfile.status: The user's status as a member of a specific
    :ivar team.MemberProfile.name: Representations for a person's name.
    :ivar team.MemberProfile.membership_type: The user's membership type: full
        (normal team member) vs limited (does not use a license; no access to
        the team's shared quota).
    :ivar team.MemberProfile.invited_on: The date and time the user was invited
        to the team (contains value only when the member's status matches
        ``TeamMemberStatus.invited``).
    :ivar team.MemberProfile.joined_on: The date and time the user joined as a
        member of a specific team.
    :ivar team.MemberProfile.suspended_on: The date and time the user was
        suspended from the team (contains value only when the member's status
        matches ``TeamMemberStatus.suspended``).
    :ivar team.MemberProfile.persistent_id: Persistent ID that a team can attach
        to the user. The persistent ID is unique ID to be used for SAML
    :ivar team.MemberProfile.is_directory_restricted: Whether the user is a
        directory restricted user.
    :ivar team.MemberProfile.profile_photo_url: URL for the photo representing
        the user, if one is set.
        '_external_id_value',
        '_secondary_emails_value',
        '_membership_type_value',
        '_invited_on_value',
        '_joined_on_value',
        '_suspended_on_value',
        '_persistent_id_value',
                 membership_type=None,
                 external_id=None,
                 secondary_emails=None,
                 invited_on=None,
                 joined_on=None,
                 suspended_on=None,
                 persistent_id=None,
        self._external_id_value = bb.NOT_SET
        self._secondary_emails_value = bb.NOT_SET
        self._membership_type_value = bb.NOT_SET
        self._invited_on_value = bb.NOT_SET
        self._joined_on_value = bb.NOT_SET
        self._suspended_on_value = bb.NOT_SET
        self._persistent_id_value = bb.NOT_SET
        if external_id is not None:
            self.external_id = external_id
        if secondary_emails is not None:
            self.secondary_emails = secondary_emails
        if membership_type is not None:
            self.membership_type = membership_type
        if invited_on is not None:
            self.invited_on = invited_on
        if joined_on is not None:
            self.joined_on = joined_on
        if suspended_on is not None:
            self.suspended_on = suspended_on
        if persistent_id is not None:
            self.persistent_id = persistent_id
    external_id = bb.Attribute("external_id", nullable=True)
    email_verified = bb.Attribute("email_verified")
    # Instance attribute type: list of [secondary_emails.SecondaryEmail] (validator is set below)
    secondary_emails = bb.Attribute("secondary_emails", nullable=True)
    # Instance attribute type: users.Name (validator is set below)
    name = bb.Attribute("name", user_defined=True)
    # Instance attribute type: TeamMembershipType (validator is set below)
    membership_type = bb.Attribute("membership_type", user_defined=True)
    invited_on = bb.Attribute("invited_on", nullable=True)
    joined_on = bb.Attribute("joined_on", nullable=True)
    suspended_on = bb.Attribute("suspended_on", nullable=True)
    persistent_id = bb.Attribute("persistent_id", nullable=True)
    profile_photo_url = bb.Attribute("profile_photo_url", nullable=True)
        super(MemberProfile, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberProfile_validator = bv.Struct(MemberProfile)
class UserSelectorError(bb.Union):
    Error that can be returned whenever a struct derived from
    :class:`UserSelectorArg` is used.
    :ivar team.UserSelectorError.user_not_found: No matching user found. The
        provided team_member_id, email, or external_id does not exist on this
    user_not_found = None
    def is_user_not_found(self):
        Check if the union tag is ``user_not_found``.
        return self._tag == 'user_not_found'
        super(UserSelectorError, self)._process_custom_annotations(annotation_type, field_path, processor)
UserSelectorError_validator = bv.Union(UserSelectorError)
class MemberSelectorError(UserSelectorError):
    :ivar team.MemberSelectorError.user_not_in_team: The user is not a member of
    user_not_in_team = None
    def is_user_not_in_team(self):
        Check if the union tag is ``user_not_in_team``.
        return self._tag == 'user_not_in_team'
        super(MemberSelectorError, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberSelectorError_validator = bv.Union(MemberSelectorError)
class MembersAddArgBase(bb.Struct):
    :ivar team.MembersAddArgBase.force_async: Whether to force the add to happen
        super(MembersAddArgBase, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersAddArgBase_validator = bv.Struct(MembersAddArgBase)
class MembersAddArg(MembersAddArgBase):
    :ivar team.MembersAddArg.new_members: Details of new members to be added to
        '_new_members_value',
                 new_members=None,
        super(MembersAddArg, self).__init__(force_async)
        self._new_members_value = bb.NOT_SET
        if new_members is not None:
            self.new_members = new_members
    # Instance attribute type: list of [MemberAddArg] (validator is set below)
    new_members = bb.Attribute("new_members")
        super(MembersAddArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersAddArg_validator = bv.Struct(MembersAddArg)
class MembersAddJobStatus(async_.PollResultBase):
    :ivar list of [MemberAddResult] team.MembersAddJobStatus.complete: The
        asynchronous job has finished. For each member that was specified in the
        parameter :type:`MembersAddArg` that was provided to
        :route:`members/add`, a corresponding item is returned in this list.
    :ivar str team.MembersAddJobStatus.failed: The asynchronous job returned an
        error. The string contains an error message.
        :param list of [MemberAddResult] val:
        :rtype: MembersAddJobStatus
        The asynchronous job has finished. For each member that was specified in
        the parameter :class:`MembersAddArg` that was provided to
        :meth:`dropbox.dropbox_client.Dropbox.team_members_add`, a corresponding
        item is returned in this list.
        :rtype: list of [MemberAddResult]
        The asynchronous job returned an error. The string contains an error
        super(MembersAddJobStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersAddJobStatus_validator = bv.Union(MembersAddJobStatus)
class MembersAddJobStatusV2Result(async_.PollResultBase):
    :ivar list of [MemberAddV2Result] team.MembersAddJobStatusV2Result.complete:
        the parameter :type:`MembersAddArg` that was provided to
        :route:`members/add:2`, a corresponding item is returned in this list.
    :ivar str team.MembersAddJobStatusV2Result.failed: The asynchronous job
        returned an error. The string contains an error message.
        :param list of [MemberAddV2Result] val:
        :rtype: MembersAddJobStatusV2Result
        :rtype: list of [MemberAddV2Result]
        super(MembersAddJobStatusV2Result, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersAddJobStatusV2Result_validator = bv.Union(MembersAddJobStatusV2Result)
class MembersAddLaunch(async_.LaunchResultBase):
        :rtype: MembersAddLaunch
        super(MembersAddLaunch, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersAddLaunch_validator = bv.Union(MembersAddLaunch)
class MembersAddLaunchV2Result(async_.LaunchResultBase):
        :rtype: MembersAddLaunchV2Result
        super(MembersAddLaunchV2Result, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersAddLaunchV2Result_validator = bv.Union(MembersAddLaunchV2Result)
class MembersAddV2Arg(MembersAddArgBase):
    :ivar team.MembersAddV2Arg.new_members: Details of new members to be added
        to the team.
        super(MembersAddV2Arg, self).__init__(force_async)
    # Instance attribute type: list of [MemberAddV2Arg] (validator is set below)
        super(MembersAddV2Arg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersAddV2Arg_validator = bv.Struct(MembersAddV2Arg)
class MembersDeactivateBaseArg(bb.Struct):
    :ivar team.MembersDeactivateBaseArg.user: Identity of user to
        remove/suspend/have their files moved.
        super(MembersDeactivateBaseArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersDeactivateBaseArg_validator = bv.Struct(MembersDeactivateBaseArg)
class MembersDataTransferArg(MembersDeactivateBaseArg):
    :ivar team.MembersDataTransferArg.transfer_dest_id: Files from the deleted
        member account will be transferred to this user.
    :ivar team.MembersDataTransferArg.transfer_admin_id: Errors during the
        transfer process will be sent via email to this user.
        '_transfer_dest_id_value',
        '_transfer_admin_id_value',
                 transfer_admin_id=None):
        super(MembersDataTransferArg, self).__init__(user)
        self._transfer_dest_id_value = bb.NOT_SET
        self._transfer_admin_id_value = bb.NOT_SET
        if transfer_dest_id is not None:
            self.transfer_dest_id = transfer_dest_id
        if transfer_admin_id is not None:
            self.transfer_admin_id = transfer_admin_id
    transfer_dest_id = bb.Attribute("transfer_dest_id", user_defined=True)
    transfer_admin_id = bb.Attribute("transfer_admin_id", user_defined=True)
        super(MembersDataTransferArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersDataTransferArg_validator = bv.Struct(MembersDataTransferArg)
class MembersDeactivateArg(MembersDeactivateBaseArg):
    :ivar team.MembersDeactivateArg.wipe_data: If provided, controls if the
        user's data will be deleted on their linked devices.
        '_wipe_data_value',
                 wipe_data=None):
        super(MembersDeactivateArg, self).__init__(user)
        self._wipe_data_value = bb.NOT_SET
        if wipe_data is not None:
            self.wipe_data = wipe_data
    wipe_data = bb.Attribute("wipe_data")
        super(MembersDeactivateArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersDeactivateArg_validator = bv.Struct(MembersDeactivateArg)
class MembersDeactivateError(UserSelectorError):
    :ivar team.MembersDeactivateError.user_not_in_team: The user is not a member
        super(MembersDeactivateError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersDeactivateError_validator = bv.Union(MembersDeactivateError)
class MembersDeleteProfilePhotoArg(bb.Struct):
    :ivar team.MembersDeleteProfilePhotoArg.user: Identity of the user whose
        profile photo will be deleted.
        super(MembersDeleteProfilePhotoArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersDeleteProfilePhotoArg_validator = bv.Struct(MembersDeleteProfilePhotoArg)
class MembersDeleteProfilePhotoError(MemberSelectorError):
    :ivar team.MembersDeleteProfilePhotoError.set_profile_disallowed: Modifying
        deleted users is not allowed.
    set_profile_disallowed = None
    def is_set_profile_disallowed(self):
        Check if the union tag is ``set_profile_disallowed``.
        return self._tag == 'set_profile_disallowed'
        super(MembersDeleteProfilePhotoError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersDeleteProfilePhotoError_validator = bv.Union(MembersDeleteProfilePhotoError)
class MembersGetAvailableTeamMemberRolesResult(bb.Struct):
    Available TeamMemberRole for the connected team. To be used with
    :meth:`dropbox.dropbox_client.Dropbox.team_members_set_admin_permissions`.
    :ivar team.MembersGetAvailableTeamMemberRolesResult.roles: Available roles.
        '_roles_value',
                 roles=None):
        self._roles_value = bb.NOT_SET
        if roles is not None:
            self.roles = roles
    # Instance attribute type: list of [TeamMemberRole] (validator is set below)
    roles = bb.Attribute("roles")
        super(MembersGetAvailableTeamMemberRolesResult, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersGetAvailableTeamMemberRolesResult_validator = bv.Struct(MembersGetAvailableTeamMemberRolesResult)
class MembersGetInfoArgs(bb.Struct):
    :ivar team.MembersGetInfoArgs.members: List of team members.
        super(MembersGetInfoArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersGetInfoArgs_validator = bv.Struct(MembersGetInfoArgs)
class MembersGetInfoError(bb.Union):
        super(MembersGetInfoError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersGetInfoError_validator = bv.Union(MembersGetInfoError)
class MembersGetInfoItemBase(bb.Union):
    :ivar str team.MembersGetInfoItemBase.id_not_found: An ID that was provided
        as a parameter to :route:`members/get_info` or
        :route:`members/get_info:2`, and did not match a corresponding user.
        This might be a team_member_id, an email, or an external ID, depending
        on how the method was called.
        :rtype: MembersGetInfoItemBase
        :meth:`dropbox.dropbox_client.Dropbox.team_members_get_info` or
        :meth:`dropbox.dropbox_client.Dropbox.team_members_get_info`, and did
        not match a corresponding user. This might be a team_member_id, an
        email, or an external ID, depending on how the method was called.
        super(MembersGetInfoItemBase, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersGetInfoItemBase_validator = bv.Union(MembersGetInfoItemBase)
class MembersGetInfoItem(MembersGetInfoItemBase):
    Describes a result obtained for a single user whose id was specified in the
    parameter of :meth:`dropbox.dropbox_client.Dropbox.team_members_get_info`.
    :ivar TeamMemberInfo MembersGetInfoItem.member_info: Info about a team
    def member_info(cls, val):
        Create an instance of this class set to the ``member_info`` tag with
        :rtype: MembersGetInfoItem
        return cls('member_info', val)
    def is_member_info(self):
        Check if the union tag is ``member_info``.
        return self._tag == 'member_info'
    def get_member_info(self):
        Info about a team member.
        Only call this if :meth:`is_member_info` is true.
        if not self.is_member_info():
            raise AttributeError("tag 'member_info' not set")
        super(MembersGetInfoItem, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersGetInfoItem_validator = bv.Union(MembersGetInfoItem)
class MembersGetInfoItemV2(MembersGetInfoItemBase):
    :ivar TeamMemberInfoV2 MembersGetInfoItemV2.member_info: Info about a team
        :rtype: MembersGetInfoItemV2
        super(MembersGetInfoItemV2, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersGetInfoItemV2_validator = bv.Union(MembersGetInfoItemV2)
class MembersGetInfoV2Arg(bb.Struct):
    :ivar team.MembersGetInfoV2Arg.members: List of team members.
        super(MembersGetInfoV2Arg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersGetInfoV2Arg_validator = bv.Struct(MembersGetInfoV2Arg)
class MembersGetInfoV2Result(bb.Struct):
    :ivar team.MembersGetInfoV2Result.members_info: List of team members info.
        '_members_info_value',
                 members_info=None):
        self._members_info_value = bb.NOT_SET
        if members_info is not None:
            self.members_info = members_info
    # Instance attribute type: list of [MembersGetInfoItemV2] (validator is set below)
    members_info = bb.Attribute("members_info")
        super(MembersGetInfoV2Result, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersGetInfoV2Result_validator = bv.Struct(MembersGetInfoV2Result)
class MembersInfo(bb.Struct):
    :ivar team.MembersInfo.team_member_ids: Team member IDs of the users under
        this hold.
    :ivar team.MembersInfo.permanently_deleted_users: The number of permanently
        deleted users that were under this hold.
        '_team_member_ids_value',
        '_permanently_deleted_users_value',
                 team_member_ids=None,
                 permanently_deleted_users=None):
        self._team_member_ids_value = bb.NOT_SET
        self._permanently_deleted_users_value = bb.NOT_SET
        if team_member_ids is not None:
            self.team_member_ids = team_member_ids
        if permanently_deleted_users is not None:
            self.permanently_deleted_users = permanently_deleted_users
    team_member_ids = bb.Attribute("team_member_ids")
    permanently_deleted_users = bb.Attribute("permanently_deleted_users")
        super(MembersInfo, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersInfo_validator = bv.Struct(MembersInfo)
class MembersListArg(bb.Struct):
    :ivar team.MembersListArg.limit: Number of results to return per call.
    :ivar team.MembersListArg.include_removed: Whether to return removed
        '_include_removed_value',
                 include_removed=None):
        self._include_removed_value = bb.NOT_SET
        if include_removed is not None:
            self.include_removed = include_removed
    include_removed = bb.Attribute("include_removed")
        super(MembersListArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersListArg_validator = bv.Struct(MembersListArg)
class MembersListContinueArg(bb.Struct):
    :ivar team.MembersListContinueArg.cursor: Indicates from what point to get
        the next set of members.
        super(MembersListContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersListContinueArg_validator = bv.Struct(MembersListContinueArg)
class MembersListContinueError(bb.Union):
    :ivar team.MembersListContinueError.invalid_cursor: The cursor is invalid.
        super(MembersListContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersListContinueError_validator = bv.Union(MembersListContinueError)
class MembersListError(bb.Union):
        super(MembersListError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersListError_validator = bv.Union(MembersListError)
class MembersListResult(bb.Struct):
    :ivar team.MembersListResult.members: List of team members.
    :ivar team.MembersListResult.cursor: Pass the cursor into
        :meth:`dropbox.dropbox_client.Dropbox.team_members_list_continue` to
        obtain the additional members.
    :ivar team.MembersListResult.has_more: Is true if there are additional team
        members that have not been returned yet. An additional call to
        :meth:`dropbox.dropbox_client.Dropbox.team_members_list_continue` can
    # Instance attribute type: list of [TeamMemberInfo] (validator is set below)
        super(MembersListResult, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersListResult_validator = bv.Struct(MembersListResult)
class MembersListV2Result(bb.Struct):
    :ivar team.MembersListV2Result.members: List of team members.
    :ivar team.MembersListV2Result.cursor: Pass the cursor into
    :ivar team.MembersListV2Result.has_more: Is true if there are additional
        team members that have not been returned yet. An additional call to
    # Instance attribute type: list of [TeamMemberInfoV2] (validator is set below)
        super(MembersListV2Result, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersListV2Result_validator = bv.Struct(MembersListV2Result)
class MembersRecoverArg(bb.Struct):
    :ivar team.MembersRecoverArg.user: Identity of user to recover.
        super(MembersRecoverArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersRecoverArg_validator = bv.Struct(MembersRecoverArg)
class MembersRecoverError(UserSelectorError):
    :ivar team.MembersRecoverError.user_unrecoverable: The user is not
        recoverable.
    :ivar team.MembersRecoverError.user_not_in_team: The user is not a member of
    :ivar team.MembersRecoverError.team_license_limit: Team is full. The
        organization has no available licenses.
    user_unrecoverable = None
    team_license_limit = None
    def is_user_unrecoverable(self):
        Check if the union tag is ``user_unrecoverable``.
        return self._tag == 'user_unrecoverable'
        super(MembersRecoverError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersRecoverError_validator = bv.Union(MembersRecoverError)
class MembersRemoveArg(MembersDeactivateArg):
    :ivar team.MembersRemoveArg.transfer_dest_id: If provided, files from the
        deleted member account will be transferred to this user.
    :ivar team.MembersRemoveArg.transfer_admin_id: If provided, errors during
        the transfer process will be sent via email to this user. If the
        transfer_dest_id argument was provided, then this argument must be
        provided as well.
    :ivar team.MembersRemoveArg.keep_account: Downgrade the member to a Basic
        account. The user will retain the email address associated with their
        Dropbox  account and data in their account that is not restricted to
        team members. In order to keep the account the argument ``wipe_data``
    :ivar team.MembersRemoveArg.retain_team_shares: If provided, allows removed
        users to keep access to Dropbox folders (not Dropbox Paper folders)
        already explicitly shared with them (not via a group) when they are
        downgraded to a Basic account. Users will not retain access to folders
        that do not allow external sharing. In order to keep the sharing
        relationships, the arguments ``wipe_data`` should be set to ``False``
        and ``keep_account`` should be set to ``True``.
        '_keep_account_value',
        '_retain_team_shares_value',
                 wipe_data=None,
                 keep_account=None,
                 retain_team_shares=None):
        super(MembersRemoveArg, self).__init__(user,
        self._keep_account_value = bb.NOT_SET
        self._retain_team_shares_value = bb.NOT_SET
        if keep_account is not None:
            self.keep_account = keep_account
        if retain_team_shares is not None:
            self.retain_team_shares = retain_team_shares
    transfer_dest_id = bb.Attribute("transfer_dest_id", nullable=True, user_defined=True)
    transfer_admin_id = bb.Attribute("transfer_admin_id", nullable=True, user_defined=True)
    keep_account = bb.Attribute("keep_account")
    retain_team_shares = bb.Attribute("retain_team_shares")
        super(MembersRemoveArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersRemoveArg_validator = bv.Struct(MembersRemoveArg)
class MembersTransferFilesError(MembersDeactivateError):
        team.MembersTransferFilesError.removed_and_transfer_dest_should_differ:
        Expected removed user and transfer_dest user to be different.
        team.MembersTransferFilesError.removed_and_transfer_admin_should_differ:
        Expected removed user and transfer_admin user to be different.
    :ivar team.MembersTransferFilesError.transfer_dest_user_not_found: No
        matching user found for the argument transfer_dest_id.
    :ivar team.MembersTransferFilesError.transfer_dest_user_not_in_team: The
        provided transfer_dest_id does not exist on this team.
    :ivar team.MembersTransferFilesError.transfer_admin_user_not_in_team: The
        provided transfer_admin_id does not exist on this team.
    :ivar team.MembersTransferFilesError.transfer_admin_user_not_found: No
        matching user found for the argument transfer_admin_id.
    :ivar team.MembersTransferFilesError.unspecified_transfer_admin_id: The
        transfer_admin_id argument must be provided when file transfer is
        requested.
    :ivar team.MembersTransferFilesError.transfer_admin_is_not_admin: Specified
        transfer_admin user is not a team admin.
    :ivar team.MembersTransferFilesError.recipient_not_verified: The recipient
        user's email is not verified.
    removed_and_transfer_dest_should_differ = None
    removed_and_transfer_admin_should_differ = None
    transfer_dest_user_not_found = None
    transfer_dest_user_not_in_team = None
    transfer_admin_user_not_in_team = None
    transfer_admin_user_not_found = None
    unspecified_transfer_admin_id = None
    transfer_admin_is_not_admin = None
    recipient_not_verified = None
    def is_removed_and_transfer_dest_should_differ(self):
        Check if the union tag is ``removed_and_transfer_dest_should_differ``.
        return self._tag == 'removed_and_transfer_dest_should_differ'
    def is_removed_and_transfer_admin_should_differ(self):
        Check if the union tag is ``removed_and_transfer_admin_should_differ``.
        return self._tag == 'removed_and_transfer_admin_should_differ'
    def is_transfer_dest_user_not_found(self):
        Check if the union tag is ``transfer_dest_user_not_found``.
        return self._tag == 'transfer_dest_user_not_found'
    def is_transfer_dest_user_not_in_team(self):
        Check if the union tag is ``transfer_dest_user_not_in_team``.
        return self._tag == 'transfer_dest_user_not_in_team'
    def is_transfer_admin_user_not_in_team(self):
        Check if the union tag is ``transfer_admin_user_not_in_team``.
        return self._tag == 'transfer_admin_user_not_in_team'
    def is_transfer_admin_user_not_found(self):
        Check if the union tag is ``transfer_admin_user_not_found``.
        return self._tag == 'transfer_admin_user_not_found'
    def is_unspecified_transfer_admin_id(self):
        Check if the union tag is ``unspecified_transfer_admin_id``.
        return self._tag == 'unspecified_transfer_admin_id'
    def is_transfer_admin_is_not_admin(self):
        Check if the union tag is ``transfer_admin_is_not_admin``.
        return self._tag == 'transfer_admin_is_not_admin'
    def is_recipient_not_verified(self):
        Check if the union tag is ``recipient_not_verified``.
        return self._tag == 'recipient_not_verified'
        super(MembersTransferFilesError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersTransferFilesError_validator = bv.Union(MembersTransferFilesError)
class MembersRemoveError(MembersTransferFilesError):
    :ivar team.MembersRemoveError.remove_last_admin: The user is the last admin
        of the team, so it cannot be removed from it.
    :ivar team.MembersRemoveError.cannot_keep_account_and_transfer: Cannot keep
        account and transfer the data to another user at the same time.
    :ivar team.MembersRemoveError.cannot_keep_account_and_delete_data: Cannot
        keep account and delete the data at the same time. To keep the account
        the argument wipe_data should be set to ``False``.
    :ivar team.MembersRemoveError.email_address_too_long_to_be_disabled: The
        email address of the user is too long to be disabled.
    :ivar team.MembersRemoveError.cannot_keep_invited_user_account: Cannot keep
        account of an invited user.
    :ivar team.MembersRemoveError.cannot_retain_shares_when_data_wiped: Cannot
        retain team shares when the user's data is marked for deletion on their
        linked devices. The argument wipe_data should be set to ``False``.
    :ivar team.MembersRemoveError.cannot_retain_shares_when_no_account_kept: The
        user's account must be kept in order to retain team shares. The argument
        keep_account should be set to ``True``.
        team.MembersRemoveError.cannot_retain_shares_when_team_external_sharing_off:
        Externally sharing files, folders, and links must be enabled in team
        settings in order to retain team shares for the user.
    :ivar team.MembersRemoveError.cannot_keep_account: Only a team admin, can
        convert this account to a Basic account.
    :ivar team.MembersRemoveError.cannot_keep_account_under_legal_hold: This
        user content is currently being held. To convert this member's account
        to a Basic account, you'll first need to remove them from the hold.
    :ivar team.MembersRemoveError.cannot_keep_account_required_to_sign_tos: To
        convert this member to a Basic account, they'll first need to sign in to
        Dropbox and agree to the terms of service.
    remove_last_admin = None
    cannot_keep_account_and_transfer = None
    cannot_keep_account_and_delete_data = None
    email_address_too_long_to_be_disabled = None
    cannot_keep_invited_user_account = None
    cannot_retain_shares_when_data_wiped = None
    cannot_retain_shares_when_no_account_kept = None
    cannot_retain_shares_when_team_external_sharing_off = None
    cannot_keep_account = None
    cannot_keep_account_under_legal_hold = None
    cannot_keep_account_required_to_sign_tos = None
    def is_remove_last_admin(self):
        Check if the union tag is ``remove_last_admin``.
        return self._tag == 'remove_last_admin'
    def is_cannot_keep_account_and_transfer(self):
        Check if the union tag is ``cannot_keep_account_and_transfer``.
        return self._tag == 'cannot_keep_account_and_transfer'
    def is_cannot_keep_account_and_delete_data(self):
        Check if the union tag is ``cannot_keep_account_and_delete_data``.
        return self._tag == 'cannot_keep_account_and_delete_data'
    def is_email_address_too_long_to_be_disabled(self):
        Check if the union tag is ``email_address_too_long_to_be_disabled``.
        return self._tag == 'email_address_too_long_to_be_disabled'
    def is_cannot_keep_invited_user_account(self):
        Check if the union tag is ``cannot_keep_invited_user_account``.
        return self._tag == 'cannot_keep_invited_user_account'
    def is_cannot_retain_shares_when_data_wiped(self):
        Check if the union tag is ``cannot_retain_shares_when_data_wiped``.
        return self._tag == 'cannot_retain_shares_when_data_wiped'
    def is_cannot_retain_shares_when_no_account_kept(self):
        Check if the union tag is ``cannot_retain_shares_when_no_account_kept``.
        return self._tag == 'cannot_retain_shares_when_no_account_kept'
    def is_cannot_retain_shares_when_team_external_sharing_off(self):
        Check if the union tag is ``cannot_retain_shares_when_team_external_sharing_off``.
        return self._tag == 'cannot_retain_shares_when_team_external_sharing_off'
    def is_cannot_keep_account(self):
        Check if the union tag is ``cannot_keep_account``.
        return self._tag == 'cannot_keep_account'
    def is_cannot_keep_account_under_legal_hold(self):
        Check if the union tag is ``cannot_keep_account_under_legal_hold``.
        return self._tag == 'cannot_keep_account_under_legal_hold'
    def is_cannot_keep_account_required_to_sign_tos(self):
        Check if the union tag is ``cannot_keep_account_required_to_sign_tos``.
        return self._tag == 'cannot_keep_account_required_to_sign_tos'
        super(MembersRemoveError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersRemoveError_validator = bv.Union(MembersRemoveError)
class MembersSendWelcomeError(MemberSelectorError):
        super(MembersSendWelcomeError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSendWelcomeError_validator = bv.Union(MembersSendWelcomeError)
class MembersSetPermissions2Arg(bb.Struct):
    :ivar team.MembersSetPermissions2Arg.user: Identity of user whose role will
        be set.
    :ivar team.MembersSetPermissions2Arg.new_roles: The new roles for the
        member. Send empty list to make user member only. For now, only up to
        one role is allowed.
        '_new_roles_value',
        self._new_roles_value = bb.NOT_SET
        if new_roles is not None:
            self.new_roles = new_roles
    new_roles = bb.Attribute("new_roles", nullable=True)
        super(MembersSetPermissions2Arg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSetPermissions2Arg_validator = bv.Struct(MembersSetPermissions2Arg)
class MembersSetPermissions2Error(UserSelectorError):
    :ivar team.MembersSetPermissions2Error.last_admin: Cannot remove the admin
        setting of the last admin.
    :ivar team.MembersSetPermissions2Error.user_not_in_team: The user is not a
    :ivar team.MembersSetPermissions2Error.cannot_set_permissions: Cannot
        remove/grant permissions. This can happen if the team member is
        suspended.
    :ivar team.MembersSetPermissions2Error.role_not_found: No matching role
        found. At least one of the provided new_roles does not exist on this
    last_admin = None
    cannot_set_permissions = None
    role_not_found = None
    def is_last_admin(self):
        Check if the union tag is ``last_admin``.
        return self._tag == 'last_admin'
    def is_cannot_set_permissions(self):
        Check if the union tag is ``cannot_set_permissions``.
        return self._tag == 'cannot_set_permissions'
    def is_role_not_found(self):
        Check if the union tag is ``role_not_found``.
        return self._tag == 'role_not_found'
        super(MembersSetPermissions2Error, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSetPermissions2Error_validator = bv.Union(MembersSetPermissions2Error)
class MembersSetPermissions2Result(bb.Struct):
    :ivar team.MembersSetPermissions2Result.team_member_id: The member ID of the
        user to which the change was applied.
    :ivar team.MembersSetPermissions2Result.roles: The roles after the change.
        Empty in case the user become a non-admin.
    roles = bb.Attribute("roles", nullable=True)
        super(MembersSetPermissions2Result, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSetPermissions2Result_validator = bv.Struct(MembersSetPermissions2Result)
class MembersSetPermissionsArg(bb.Struct):
    :ivar team.MembersSetPermissionsArg.user: Identity of user whose role will
    :ivar team.MembersSetPermissionsArg.new_role: The new role of the member.
        '_new_role_value',
                 new_role=None):
        self._new_role_value = bb.NOT_SET
        if new_role is not None:
            self.new_role = new_role
    new_role = bb.Attribute("new_role", user_defined=True)
        super(MembersSetPermissionsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSetPermissionsArg_validator = bv.Struct(MembersSetPermissionsArg)
class MembersSetPermissionsError(UserSelectorError):
    :ivar team.MembersSetPermissionsError.last_admin: Cannot remove the admin
    :ivar team.MembersSetPermissionsError.user_not_in_team: The user is not a
    :ivar team.MembersSetPermissionsError.cannot_set_permissions: Cannot
        remove/grant permissions.
    :ivar team.MembersSetPermissionsError.team_license_limit: Team is full. The
        super(MembersSetPermissionsError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSetPermissionsError_validator = bv.Union(MembersSetPermissionsError)
class MembersSetPermissionsResult(bb.Struct):
    :ivar team.MembersSetPermissionsResult.team_member_id: The member ID of the
    :ivar team.MembersSetPermissionsResult.role: The role after the change.
        super(MembersSetPermissionsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSetPermissionsResult_validator = bv.Struct(MembersSetPermissionsResult)
class MembersSetProfileArg(bb.Struct):
    identify the user account. At least one of new_email, new_external_id,
    new_given_name, and/or new_surname must be provided.
    :ivar team.MembersSetProfileArg.user: Identity of user whose profile will be
        set.
    :ivar team.MembersSetProfileArg.new_email: New email for member.
    :ivar team.MembersSetProfileArg.new_external_id: New external ID for member.
    :ivar team.MembersSetProfileArg.new_given_name: New given name for member.
    :ivar team.MembersSetProfileArg.new_surname: New surname for member.
    :ivar team.MembersSetProfileArg.new_persistent_id: New persistent ID. This
        field only available to teams using persistent ID SAML configuration.
    :ivar team.MembersSetProfileArg.new_is_directory_restricted: New value for
        whether the user is a directory restricted user.
        '_new_email_value',
        '_new_external_id_value',
        '_new_given_name_value',
        '_new_surname_value',
        '_new_persistent_id_value',
        '_new_is_directory_restricted_value',
        self._new_email_value = bb.NOT_SET
        self._new_external_id_value = bb.NOT_SET
        self._new_given_name_value = bb.NOT_SET
        self._new_surname_value = bb.NOT_SET
        self._new_persistent_id_value = bb.NOT_SET
        self._new_is_directory_restricted_value = bb.NOT_SET
        if new_email is not None:
            self.new_email = new_email
        if new_external_id is not None:
            self.new_external_id = new_external_id
        if new_given_name is not None:
            self.new_given_name = new_given_name
        if new_surname is not None:
            self.new_surname = new_surname
        if new_persistent_id is not None:
            self.new_persistent_id = new_persistent_id
        if new_is_directory_restricted is not None:
            self.new_is_directory_restricted = new_is_directory_restricted
    new_email = bb.Attribute("new_email", nullable=True)
    new_external_id = bb.Attribute("new_external_id", nullable=True)
    new_given_name = bb.Attribute("new_given_name", nullable=True)
    new_surname = bb.Attribute("new_surname", nullable=True)
    new_persistent_id = bb.Attribute("new_persistent_id", nullable=True)
    new_is_directory_restricted = bb.Attribute("new_is_directory_restricted", nullable=True)
        super(MembersSetProfileArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSetProfileArg_validator = bv.Struct(MembersSetProfileArg)
class MembersSetProfileError(MemberSelectorError):
    :ivar team.MembersSetProfileError.external_id_and_new_external_id_unsafe: It
        is unsafe to use both external_id and new_external_id.
    :ivar team.MembersSetProfileError.no_new_data_specified: None of new_email,
        new_given_name, new_surname, or new_external_id are specified.
    :ivar team.MembersSetProfileError.email_reserved_for_other_user: Email is
        already reserved for another user.
    :ivar team.MembersSetProfileError.external_id_used_by_other_user: The
        external ID is already in use by another team member.
    :ivar team.MembersSetProfileError.set_profile_disallowed: Modifying deleted
        users is not allowed.
    :ivar team.MembersSetProfileError.param_cannot_be_empty: Parameter new_email
        cannot be empty.
    :ivar team.MembersSetProfileError.persistent_id_disabled: Persistent ID is
    :ivar team.MembersSetProfileError.persistent_id_used_by_other_user: The
        persistent ID is already in use by another team member.
    :ivar team.MembersSetProfileError.directory_restricted_off: Directory
        Restrictions option is not available.
    external_id_and_new_external_id_unsafe = None
    no_new_data_specified = None
    email_reserved_for_other_user = None
    external_id_used_by_other_user = None
    param_cannot_be_empty = None
    persistent_id_disabled = None
    persistent_id_used_by_other_user = None
    directory_restricted_off = None
    def is_external_id_and_new_external_id_unsafe(self):
        Check if the union tag is ``external_id_and_new_external_id_unsafe``.
        return self._tag == 'external_id_and_new_external_id_unsafe'
    def is_no_new_data_specified(self):
        Check if the union tag is ``no_new_data_specified``.
        return self._tag == 'no_new_data_specified'
    def is_email_reserved_for_other_user(self):
        Check if the union tag is ``email_reserved_for_other_user``.
        return self._tag == 'email_reserved_for_other_user'
    def is_external_id_used_by_other_user(self):
        Check if the union tag is ``external_id_used_by_other_user``.
        return self._tag == 'external_id_used_by_other_user'
    def is_param_cannot_be_empty(self):
        Check if the union tag is ``param_cannot_be_empty``.
        return self._tag == 'param_cannot_be_empty'
    def is_persistent_id_used_by_other_user(self):
        Check if the union tag is ``persistent_id_used_by_other_user``.
        return self._tag == 'persistent_id_used_by_other_user'
    def is_directory_restricted_off(self):
        Check if the union tag is ``directory_restricted_off``.
        return self._tag == 'directory_restricted_off'
        super(MembersSetProfileError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSetProfileError_validator = bv.Union(MembersSetProfileError)
class MembersSetProfilePhotoArg(bb.Struct):
    :ivar team.MembersSetProfilePhotoArg.user: Identity of the user whose
        profile photo will be set.
    :ivar team.MembersSetProfilePhotoArg.photo: Image to set as the member's new
    # Instance attribute type: account.PhotoSourceArg (validator is set below)
        super(MembersSetProfilePhotoArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSetProfilePhotoArg_validator = bv.Struct(MembersSetProfilePhotoArg)
class MembersSetProfilePhotoError(MemberSelectorError):
    :ivar team.MembersSetProfilePhotoError.set_profile_disallowed: Modifying
    def photo_error(cls, val):
        Create an instance of this class set to the ``photo_error`` tag with
        :param account.SetProfilePhotoError val:
        :rtype: MembersSetProfilePhotoError
        return cls('photo_error', val)
    def is_photo_error(self):
        Check if the union tag is ``photo_error``.
        return self._tag == 'photo_error'
    def get_photo_error(self):
        Only call this if :meth:`is_photo_error` is true.
        :rtype: account.SetProfilePhotoError
        if not self.is_photo_error():
            raise AttributeError("tag 'photo_error' not set")
        super(MembersSetProfilePhotoError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSetProfilePhotoError_validator = bv.Union(MembersSetProfilePhotoError)
class MembersSuspendError(MembersDeactivateError):
    :ivar team.MembersSuspendError.suspend_inactive_user: The user is not
        active, so it cannot be suspended.
    :ivar team.MembersSuspendError.suspend_last_admin: The user is the last
        admin of the team, so it cannot be suspended.
    :ivar team.MembersSuspendError.team_license_limit: Team is full. The
    suspend_inactive_user = None
    suspend_last_admin = None
    def is_suspend_inactive_user(self):
        Check if the union tag is ``suspend_inactive_user``.
        return self._tag == 'suspend_inactive_user'
    def is_suspend_last_admin(self):
        Check if the union tag is ``suspend_last_admin``.
        return self._tag == 'suspend_last_admin'
        super(MembersSuspendError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersSuspendError_validator = bv.Union(MembersSuspendError)
class MembersTransferFormerMembersFilesError(MembersTransferFilesError):
        team.MembersTransferFormerMembersFilesError.user_data_is_being_transferred:
        The user's data is being transferred. Please wait some time before
        retrying.
    :ivar team.MembersTransferFormerMembersFilesError.user_not_removed: No
        matching removed user found for the argument user.
        team.MembersTransferFormerMembersFilesError.user_data_cannot_be_transferred:
        User files aren't transferable anymore.
        team.MembersTransferFormerMembersFilesError.user_data_already_transferred:
        User's data has already been transferred to another user.
    user_data_is_being_transferred = None
    user_not_removed = None
    user_data_cannot_be_transferred = None
    user_data_already_transferred = None
    def is_user_data_is_being_transferred(self):
        Check if the union tag is ``user_data_is_being_transferred``.
        return self._tag == 'user_data_is_being_transferred'
    def is_user_not_removed(self):
        Check if the union tag is ``user_not_removed``.
        return self._tag == 'user_not_removed'
    def is_user_data_cannot_be_transferred(self):
        Check if the union tag is ``user_data_cannot_be_transferred``.
        return self._tag == 'user_data_cannot_be_transferred'
    def is_user_data_already_transferred(self):
        Check if the union tag is ``user_data_already_transferred``.
        return self._tag == 'user_data_already_transferred'
        super(MembersTransferFormerMembersFilesError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersTransferFormerMembersFilesError_validator = bv.Union(MembersTransferFormerMembersFilesError)
class MembersUnsuspendArg(bb.Struct):
    :ivar team.MembersUnsuspendArg.user: Identity of user to unsuspend.
        super(MembersUnsuspendArg, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersUnsuspendArg_validator = bv.Struct(MembersUnsuspendArg)
class MembersUnsuspendError(MembersDeactivateError):
    :ivar team.MembersUnsuspendError.unsuspend_non_suspended_member: The user is
        unsuspended, so it cannot be unsuspended again.
    :ivar team.MembersUnsuspendError.team_license_limit: Team is full. The
    unsuspend_non_suspended_member = None
    def is_unsuspend_non_suspended_member(self):
        Check if the union tag is ``unsuspend_non_suspended_member``.
        return self._tag == 'unsuspend_non_suspended_member'
        super(MembersUnsuspendError, self)._process_custom_annotations(annotation_type, field_path, processor)
MembersUnsuspendError_validator = bv.Union(MembersUnsuspendError)
class MobileClientPlatform(bb.Union):
    :ivar team.MobileClientPlatform.iphone: Official Dropbox iPhone client.
    :ivar team.MobileClientPlatform.ipad: Official Dropbox iPad client.
    :ivar team.MobileClientPlatform.android: Official Dropbox Android client.
    :ivar team.MobileClientPlatform.windows_phone: Official Dropbox Windows
        phone client.
    :ivar team.MobileClientPlatform.blackberry: Official Dropbox Blackberry
    iphone = None
    ipad = None
    android = None
    windows_phone = None
    blackberry = None
    def is_iphone(self):
        Check if the union tag is ``iphone``.
        return self._tag == 'iphone'
    def is_ipad(self):
        Check if the union tag is ``ipad``.
        return self._tag == 'ipad'
    def is_android(self):
        Check if the union tag is ``android``.
        return self._tag == 'android'
    def is_windows_phone(self):
        Check if the union tag is ``windows_phone``.
        return self._tag == 'windows_phone'
    def is_blackberry(self):
        Check if the union tag is ``blackberry``.
        return self._tag == 'blackberry'
        super(MobileClientPlatform, self)._process_custom_annotations(annotation_type, field_path, processor)
MobileClientPlatform_validator = bv.Union(MobileClientPlatform)
class MobileClientSession(DeviceSession):
    Information about linked Dropbox mobile client sessions.
    :ivar team.MobileClientSession.device_name: The device name.
    :ivar team.MobileClientSession.client_type: The mobile application type.
    :ivar team.MobileClientSession.client_version: The dropbox client version.
    :ivar team.MobileClientSession.os_version: The hosting OS version.
    :ivar team.MobileClientSession.last_carrier: last carrier used by the
        '_device_name_value',
        '_os_version_value',
        '_last_carrier_value',
                 device_name=None,
                 os_version=None,
                 last_carrier=None):
        super(MobileClientSession, self).__init__(session_id,
        self._device_name_value = bb.NOT_SET
        self._os_version_value = bb.NOT_SET
        self._last_carrier_value = bb.NOT_SET
        if device_name is not None:
            self.device_name = device_name
        if os_version is not None:
            self.os_version = os_version
        if last_carrier is not None:
            self.last_carrier = last_carrier
    device_name = bb.Attribute("device_name")
    # Instance attribute type: MobileClientPlatform (validator is set below)
    client_version = bb.Attribute("client_version", nullable=True)
    os_version = bb.Attribute("os_version", nullable=True)
    last_carrier = bb.Attribute("last_carrier", nullable=True)
        super(MobileClientSession, self)._process_custom_annotations(annotation_type, field_path, processor)
MobileClientSession_validator = bv.Struct(MobileClientSession)
class NamespaceMetadata(bb.Struct):
    Properties of a namespace.
    :ivar team.NamespaceMetadata.name: The name of this namespace.
    :ivar team.NamespaceMetadata.namespace_id: The ID of this namespace.
    :ivar team.NamespaceMetadata.namespace_type: The type of this namespace.
    :ivar team.NamespaceMetadata.team_member_id: If this is a team member or app
        folder, the ID of the owning team member. Otherwise, this field is not
        present.
        '_namespace_id_value',
        '_namespace_type_value',
                 namespace_id=None,
                 namespace_type=None,
        self._namespace_id_value = bb.NOT_SET
        self._namespace_type_value = bb.NOT_SET
        if namespace_id is not None:
            self.namespace_id = namespace_id
        if namespace_type is not None:
            self.namespace_type = namespace_type
    namespace_id = bb.Attribute("namespace_id")
    # Instance attribute type: NamespaceType (validator is set below)
    namespace_type = bb.Attribute("namespace_type", user_defined=True)
        super(NamespaceMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
NamespaceMetadata_validator = bv.Struct(NamespaceMetadata)
class NamespaceType(bb.Union):
    :ivar team.NamespaceType.app_folder: App sandbox folder.
    :ivar team.NamespaceType.shared_folder: Shared folder.
    :ivar team.NamespaceType.team_folder: Top-level team-owned folder.
    :ivar team.NamespaceType.team_member_folder: Team member's home folder.
    app_folder = None
    shared_folder = None
    team_member_folder = None
    def is_app_folder(self):
        Check if the union tag is ``app_folder``.
        return self._tag == 'app_folder'
    def is_shared_folder(self):
        Check if the union tag is ``shared_folder``.
        return self._tag == 'shared_folder'
    def is_team_member_folder(self):
        Check if the union tag is ``team_member_folder``.
        return self._tag == 'team_member_folder'
        super(NamespaceType, self)._process_custom_annotations(annotation_type, field_path, processor)
NamespaceType_validator = bv.Union(NamespaceType)
class RemoveCustomQuotaResult(bb.Union):
    User result for setting member custom quota.
    :ivar UserSelectorArg RemoveCustomQuotaResult.success: Successfully removed
    :ivar UserSelectorArg RemoveCustomQuotaResult.invalid_user: Invalid user
        (not in team).
        :rtype: RemoveCustomQuotaResult
        Successfully removed user.
        super(RemoveCustomQuotaResult, self)._process_custom_annotations(annotation_type, field_path, processor)
RemoveCustomQuotaResult_validator = bv.Union(RemoveCustomQuotaResult)
class RemovedStatus(bb.Struct):
    :ivar team.RemovedStatus.is_recoverable: True if the removed team member is
    :ivar team.RemovedStatus.is_disconnected: True if the team member's account
        was converted to individual account.
        '_is_recoverable_value',
        '_is_disconnected_value',
                 is_recoverable=None,
                 is_disconnected=None):
        self._is_recoverable_value = bb.NOT_SET
        self._is_disconnected_value = bb.NOT_SET
        if is_recoverable is not None:
            self.is_recoverable = is_recoverable
        if is_disconnected is not None:
            self.is_disconnected = is_disconnected
    is_recoverable = bb.Attribute("is_recoverable")
    is_disconnected = bb.Attribute("is_disconnected")
        super(RemovedStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
RemovedStatus_validator = bv.Struct(RemovedStatus)
class ResendSecondaryEmailResult(bb.Union):
    Result of trying to resend verification email to a secondary email address.
    'success' is the only value indicating that a verification email was
    successfully sent. The other values explain the type of error that occurred,
    and include the email for which the error occurred.
    :ivar str team.ResendSecondaryEmailResult.success: A verification email was
        successfully sent to the secondary email address.
    :ivar str team.ResendSecondaryEmailResult.not_pending: This secondary email
        address is not pending for the user.
    :ivar str team.ResendSecondaryEmailResult.rate_limited: Too many emails are
        :rtype: ResendSecondaryEmailResult
    def not_pending(cls, val):
        Create an instance of this class set to the ``not_pending`` tag with
        return cls('not_pending', val)
    def is_not_pending(self):
        Check if the union tag is ``not_pending``.
        return self._tag == 'not_pending'
        A verification email was successfully sent to the secondary email
    def get_not_pending(self):
        This secondary email address is not pending for the user.
        Only call this if :meth:`is_not_pending` is true.
        if not self.is_not_pending():
            raise AttributeError("tag 'not_pending' not set")
        super(ResendSecondaryEmailResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ResendSecondaryEmailResult_validator = bv.Union(ResendSecondaryEmailResult)
class ResendVerificationEmailArg(bb.Struct):
    :ivar team.ResendVerificationEmailArg.emails_to_resend: List of users and
        secondary emails to resend verification emails to.
        '_emails_to_resend_value',
                 emails_to_resend=None):
        self._emails_to_resend_value = bb.NOT_SET
        if emails_to_resend is not None:
            self.emails_to_resend = emails_to_resend
    emails_to_resend = bb.Attribute("emails_to_resend")
        super(ResendVerificationEmailArg, self)._process_custom_annotations(annotation_type, field_path, processor)
ResendVerificationEmailArg_validator = bv.Struct(ResendVerificationEmailArg)
class ResendVerificationEmailResult(bb.Struct):
    List of users and resend results.
    # Instance attribute type: list of [UserResendResult] (validator is set below)
        super(ResendVerificationEmailResult, self)._process_custom_annotations(annotation_type, field_path, processor)
ResendVerificationEmailResult_validator = bv.Struct(ResendVerificationEmailResult)
class RevokeDesktopClientArg(DeviceSessionArg):
    :ivar team.RevokeDesktopClientArg.delete_on_unlink: Whether to delete all
        files of the account (this is possible only if supported by the desktop
        client and  will be made the next time the client access the account).
        '_delete_on_unlink_value',
                 delete_on_unlink=None):
        super(RevokeDesktopClientArg, self).__init__(session_id,
        self._delete_on_unlink_value = bb.NOT_SET
        if delete_on_unlink is not None:
            self.delete_on_unlink = delete_on_unlink
    delete_on_unlink = bb.Attribute("delete_on_unlink")
        super(RevokeDesktopClientArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeDesktopClientArg_validator = bv.Struct(RevokeDesktopClientArg)
class RevokeDeviceSessionArg(bb.Union):
    :ivar DeviceSessionArg RevokeDeviceSessionArg.web_session: End an active
    :ivar RevokeDesktopClientArg RevokeDeviceSessionArg.desktop_client: Unlink a
        linked desktop device.
    :ivar DeviceSessionArg RevokeDeviceSessionArg.mobile_client: Unlink a linked
        mobile device.
    def web_session(cls, val):
        Create an instance of this class set to the ``web_session`` tag with
        :param DeviceSessionArg val:
        :rtype: RevokeDeviceSessionArg
        return cls('web_session', val)
    def desktop_client(cls, val):
        Create an instance of this class set to the ``desktop_client`` tag with
        :param RevokeDesktopClientArg val:
        return cls('desktop_client', val)
    def mobile_client(cls, val):
        Create an instance of this class set to the ``mobile_client`` tag with
        return cls('mobile_client', val)
    def is_web_session(self):
        Check if the union tag is ``web_session``.
        return self._tag == 'web_session'
    def is_desktop_client(self):
        Check if the union tag is ``desktop_client``.
        return self._tag == 'desktop_client'
    def is_mobile_client(self):
        Check if the union tag is ``mobile_client``.
        return self._tag == 'mobile_client'
    def get_web_session(self):
        End an active session.
        Only call this if :meth:`is_web_session` is true.
        :rtype: DeviceSessionArg
        if not self.is_web_session():
            raise AttributeError("tag 'web_session' not set")
    def get_desktop_client(self):
        Unlink a linked desktop device.
        Only call this if :meth:`is_desktop_client` is true.
        :rtype: RevokeDesktopClientArg
        if not self.is_desktop_client():
            raise AttributeError("tag 'desktop_client' not set")
    def get_mobile_client(self):
        Unlink a linked mobile device.
        Only call this if :meth:`is_mobile_client` is true.
        if not self.is_mobile_client():
            raise AttributeError("tag 'mobile_client' not set")
        super(RevokeDeviceSessionArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeDeviceSessionArg_validator = bv.Union(RevokeDeviceSessionArg)
class RevokeDeviceSessionBatchArg(bb.Struct):
        '_revoke_devices_value',
                 revoke_devices=None):
        self._revoke_devices_value = bb.NOT_SET
        if revoke_devices is not None:
            self.revoke_devices = revoke_devices
    # Instance attribute type: list of [RevokeDeviceSessionArg] (validator is set below)
    revoke_devices = bb.Attribute("revoke_devices")
        super(RevokeDeviceSessionBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeDeviceSessionBatchArg_validator = bv.Struct(RevokeDeviceSessionBatchArg)
class RevokeDeviceSessionBatchError(bb.Union):
        super(RevokeDeviceSessionBatchError, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeDeviceSessionBatchError_validator = bv.Union(RevokeDeviceSessionBatchError)
class RevokeDeviceSessionBatchResult(bb.Struct):
        '_revoke_devices_status_value',
                 revoke_devices_status=None):
        self._revoke_devices_status_value = bb.NOT_SET
        if revoke_devices_status is not None:
            self.revoke_devices_status = revoke_devices_status
    # Instance attribute type: list of [RevokeDeviceSessionStatus] (validator is set below)
    revoke_devices_status = bb.Attribute("revoke_devices_status")
        super(RevokeDeviceSessionBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeDeviceSessionBatchResult_validator = bv.Struct(RevokeDeviceSessionBatchResult)
class RevokeDeviceSessionError(bb.Union):
    :ivar team.RevokeDeviceSessionError.device_session_not_found: Device session
        not found.
    :ivar team.RevokeDeviceSessionError.member_not_found: Member not found.
    device_session_not_found = None
    def is_device_session_not_found(self):
        Check if the union tag is ``device_session_not_found``.
        return self._tag == 'device_session_not_found'
        super(RevokeDeviceSessionError, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeDeviceSessionError_validator = bv.Union(RevokeDeviceSessionError)
class RevokeDeviceSessionStatus(bb.Struct):
    :ivar team.RevokeDeviceSessionStatus.success: Result of the revoking
    :ivar team.RevokeDeviceSessionStatus.error_type: The error cause in case of
        a failure.
        '_success_value',
        '_error_type_value',
                 success=None,
                 error_type=None):
        self._success_value = bb.NOT_SET
        self._error_type_value = bb.NOT_SET
        if success is not None:
            self.success = success
        if error_type is not None:
            self.error_type = error_type
    success = bb.Attribute("success")
    # Instance attribute type: RevokeDeviceSessionError (validator is set below)
    error_type = bb.Attribute("error_type", nullable=True, user_defined=True)
        super(RevokeDeviceSessionStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeDeviceSessionStatus_validator = bv.Struct(RevokeDeviceSessionStatus)
class RevokeLinkedApiAppArg(bb.Struct):
    :ivar team.RevokeLinkedApiAppArg.app_id: The application's unique id.
    :ivar team.RevokeLinkedApiAppArg.team_member_id: The unique id of the member
    :ivar team.RevokeLinkedApiAppArg.keep_app_folder: This flag is not longer
        supported, the application dedicated folder (in case the application
        uses  one) will be kept.
        '_keep_app_folder_value',
                 keep_app_folder=None):
        self._keep_app_folder_value = bb.NOT_SET
        if keep_app_folder is not None:
            self.keep_app_folder = keep_app_folder
    keep_app_folder = bb.Attribute("keep_app_folder")
        super(RevokeLinkedApiAppArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeLinkedApiAppArg_validator = bv.Struct(RevokeLinkedApiAppArg)
class RevokeLinkedApiAppBatchArg(bb.Struct):
        '_revoke_linked_app_value',
                 revoke_linked_app=None):
        self._revoke_linked_app_value = bb.NOT_SET
        if revoke_linked_app is not None:
            self.revoke_linked_app = revoke_linked_app
    # Instance attribute type: list of [RevokeLinkedApiAppArg] (validator is set below)
    revoke_linked_app = bb.Attribute("revoke_linked_app")
        super(RevokeLinkedApiAppBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeLinkedApiAppBatchArg_validator = bv.Struct(RevokeLinkedApiAppBatchArg)
class RevokeLinkedAppBatchError(bb.Union):
    :meth:`dropbox.dropbox_client.Dropbox.team_linked_apps_revoke_linked_app_batch`.
        super(RevokeLinkedAppBatchError, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeLinkedAppBatchError_validator = bv.Union(RevokeLinkedAppBatchError)
class RevokeLinkedAppBatchResult(bb.Struct):
        '_revoke_linked_app_status_value',
                 revoke_linked_app_status=None):
        self._revoke_linked_app_status_value = bb.NOT_SET
        if revoke_linked_app_status is not None:
            self.revoke_linked_app_status = revoke_linked_app_status
    # Instance attribute type: list of [RevokeLinkedAppStatus] (validator is set below)
    revoke_linked_app_status = bb.Attribute("revoke_linked_app_status")
        super(RevokeLinkedAppBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeLinkedAppBatchResult_validator = bv.Struct(RevokeLinkedAppBatchResult)
class RevokeLinkedAppError(bb.Union):
    :meth:`dropbox.dropbox_client.Dropbox.team_linked_apps_revoke_linked_app`.
    :ivar team.RevokeLinkedAppError.app_not_found: Application not found.
    :ivar team.RevokeLinkedAppError.member_not_found: Member not found.
    :ivar team.RevokeLinkedAppError.app_folder_removal_not_supported: App folder
        removal is not supported.
    app_not_found = None
    app_folder_removal_not_supported = None
    def is_app_not_found(self):
        Check if the union tag is ``app_not_found``.
        return self._tag == 'app_not_found'
    def is_app_folder_removal_not_supported(self):
        Check if the union tag is ``app_folder_removal_not_supported``.
        return self._tag == 'app_folder_removal_not_supported'
        super(RevokeLinkedAppError, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeLinkedAppError_validator = bv.Union(RevokeLinkedAppError)
class RevokeLinkedAppStatus(bb.Struct):
    :ivar team.RevokeLinkedAppStatus.success: Result of the revoking request.
    :ivar team.RevokeLinkedAppStatus.error_type: The error cause in case of a
        failure.
    # Instance attribute type: RevokeLinkedAppError (validator is set below)
        super(RevokeLinkedAppStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
RevokeLinkedAppStatus_validator = bv.Struct(RevokeLinkedAppStatus)
class SetCustomQuotaArg(bb.Struct):
    :ivar team.SetCustomQuotaArg.users_and_quotas: List of users and their
        custom quotas.
        '_users_and_quotas_value',
                 users_and_quotas=None):
        self._users_and_quotas_value = bb.NOT_SET
        if users_and_quotas is not None:
            self.users_and_quotas = users_and_quotas
    # Instance attribute type: list of [UserCustomQuotaArg] (validator is set below)
    users_and_quotas = bb.Attribute("users_and_quotas")
        super(SetCustomQuotaArg, self)._process_custom_annotations(annotation_type, field_path, processor)
SetCustomQuotaArg_validator = bv.Struct(SetCustomQuotaArg)
class SetCustomQuotaError(CustomQuotaError):
    Error returned when setting member custom quota.
    :ivar team.SetCustomQuotaError.some_users_are_excluded: Some of the users
        are on the excluded users list and can't have custom quota set.
    some_users_are_excluded = None
    def is_some_users_are_excluded(self):
        Check if the union tag is ``some_users_are_excluded``.
        return self._tag == 'some_users_are_excluded'
        super(SetCustomQuotaError, self)._process_custom_annotations(annotation_type, field_path, processor)
SetCustomQuotaError_validator = bv.Union(SetCustomQuotaError)
class SharingAllowlistAddArgs(bb.Struct):
    Structure representing Approve List entries. Domain and emails are
    supported. At least one entry of any supported type is required.
    :ivar team.SharingAllowlistAddArgs.domains: List of domains represented by
        valid string representation (RFC-1034/5).
    :ivar team.SharingAllowlistAddArgs.emails: List of emails represented by
        valid string representation (RFC-5322/822).
        '_domains_value',
        '_emails_value',
        self._domains_value = bb.NOT_SET
        self._emails_value = bb.NOT_SET
        if domains is not None:
            self.domains = domains
        if emails is not None:
            self.emails = emails
    domains = bb.Attribute("domains", nullable=True)
    emails = bb.Attribute("emails", nullable=True)
        super(SharingAllowlistAddArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingAllowlistAddArgs_validator = bv.Struct(SharingAllowlistAddArgs)
class SharingAllowlistAddError(bb.Union):
    :ivar str team.SharingAllowlistAddError.malformed_entry: One of provided
        values is not valid.
    :ivar team.SharingAllowlistAddError.no_entries_provided: Neither single
        domain nor email provided.
    :ivar team.SharingAllowlistAddError.too_many_entries_provided: Too many
        entries provided within one call.
    :ivar team.SharingAllowlistAddError.team_limit_reached: Team entries limit
        reached.
    :ivar team.SharingAllowlistAddError.unknown_error: Unknown error.
    :ivar str team.SharingAllowlistAddError.entries_already_exist: Entries
        already exists.
    no_entries_provided = None
    too_many_entries_provided = None
    team_limit_reached = None
    def malformed_entry(cls, val):
        Create an instance of this class set to the ``malformed_entry`` tag with
        :rtype: SharingAllowlistAddError
        return cls('malformed_entry', val)
    def entries_already_exist(cls, val):
        Create an instance of this class set to the ``entries_already_exist``
        return cls('entries_already_exist', val)
    def is_malformed_entry(self):
        Check if the union tag is ``malformed_entry``.
        return self._tag == 'malformed_entry'
    def is_no_entries_provided(self):
        Check if the union tag is ``no_entries_provided``.
        return self._tag == 'no_entries_provided'
    def is_too_many_entries_provided(self):
        Check if the union tag is ``too_many_entries_provided``.
        return self._tag == 'too_many_entries_provided'
    def is_team_limit_reached(self):
        Check if the union tag is ``team_limit_reached``.
        return self._tag == 'team_limit_reached'
    def is_entries_already_exist(self):
        Check if the union tag is ``entries_already_exist``.
        return self._tag == 'entries_already_exist'
    def get_malformed_entry(self):
        One of provided values is not valid.
        Only call this if :meth:`is_malformed_entry` is true.
        if not self.is_malformed_entry():
            raise AttributeError("tag 'malformed_entry' not set")
    def get_entries_already_exist(self):
        Entries already exists.
        Only call this if :meth:`is_entries_already_exist` is true.
        if not self.is_entries_already_exist():
            raise AttributeError("tag 'entries_already_exist' not set")
        super(SharingAllowlistAddError, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingAllowlistAddError_validator = bv.Union(SharingAllowlistAddError)
class SharingAllowlistAddResponse(bb.Struct):
    This struct is empty. The comment here is intentionally emitted to avoid
    indentation issues with Stone.
        super(SharingAllowlistAddResponse, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingAllowlistAddResponse_validator = bv.Struct(SharingAllowlistAddResponse)
class SharingAllowlistListArg(bb.Struct):
    :ivar team.SharingAllowlistListArg.limit: The number of entries to fetch at
        one time.
        super(SharingAllowlistListArg, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingAllowlistListArg_validator = bv.Struct(SharingAllowlistListArg)
class SharingAllowlistListContinueArg(bb.Struct):
    :ivar team.SharingAllowlistListContinueArg.cursor: The cursor returned from
        a previous call to
        :meth:`dropbox.dropbox_client.Dropbox.team_sharing_allowlist_list` or
        :meth:`dropbox.dropbox_client.Dropbox.team_sharing_allowlist_list_continue`.
        super(SharingAllowlistListContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingAllowlistListContinueArg_validator = bv.Struct(SharingAllowlistListContinueArg)
class SharingAllowlistListContinueError(bb.Union):
    :ivar team.SharingAllowlistListContinueError.invalid_cursor: Provided cursor
        is not valid.
        super(SharingAllowlistListContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingAllowlistListContinueError_validator = bv.Union(SharingAllowlistListContinueError)
class SharingAllowlistListError(bb.Struct):
        super(SharingAllowlistListError, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingAllowlistListError_validator = bv.Struct(SharingAllowlistListError)
class SharingAllowlistListResponse(bb.Struct):
    :ivar team.SharingAllowlistListResponse.domains: List of domains represented
        by valid string representation (RFC-1034/5).
    :ivar team.SharingAllowlistListResponse.emails: List of emails represented
        by valid string representation (RFC-5322/822).
    :ivar team.SharingAllowlistListResponse.cursor: If this is nonempty, there
        are more entries that can be fetched with
    :ivar team.SharingAllowlistListResponse.has_more: if true indicates that
        more entries can be fetched with
                 emails=None,
    domains = bb.Attribute("domains")
    emails = bb.Attribute("emails")
        super(SharingAllowlistListResponse, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingAllowlistListResponse_validator = bv.Struct(SharingAllowlistListResponse)
class SharingAllowlistRemoveArgs(bb.Struct):
    :ivar team.SharingAllowlistRemoveArgs.domains: List of domains represented
    :ivar team.SharingAllowlistRemoveArgs.emails: List of emails represented by
        super(SharingAllowlistRemoveArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingAllowlistRemoveArgs_validator = bv.Struct(SharingAllowlistRemoveArgs)
class SharingAllowlistRemoveError(bb.Union):
    :ivar str team.SharingAllowlistRemoveError.malformed_entry: One of provided
    :ivar str team.SharingAllowlistRemoveError.entries_do_not_exist: One or more
        provided values do not exist.
    :ivar team.SharingAllowlistRemoveError.no_entries_provided: Neither single
    :ivar team.SharingAllowlistRemoveError.too_many_entries_provided: Too many
    :ivar team.SharingAllowlistRemoveError.unknown_error: Unknown error.
        :rtype: SharingAllowlistRemoveError
    def entries_do_not_exist(cls, val):
        Create an instance of this class set to the ``entries_do_not_exist`` tag
        return cls('entries_do_not_exist', val)
    def is_entries_do_not_exist(self):
        Check if the union tag is ``entries_do_not_exist``.
        return self._tag == 'entries_do_not_exist'
    def get_entries_do_not_exist(self):
        One or more provided values do not exist.
        Only call this if :meth:`is_entries_do_not_exist` is true.
        if not self.is_entries_do_not_exist():
            raise AttributeError("tag 'entries_do_not_exist' not set")
        super(SharingAllowlistRemoveError, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingAllowlistRemoveError_validator = bv.Union(SharingAllowlistRemoveError)
class SharingAllowlistRemoveResponse(bb.Struct):
        super(SharingAllowlistRemoveResponse, self)._process_custom_annotations(annotation_type, field_path, processor)
SharingAllowlistRemoveResponse_validator = bv.Struct(SharingAllowlistRemoveResponse)
class StorageBucket(bb.Struct):
    Describes the number of users in a specific storage bucket.
    :ivar team.StorageBucket.bucket: The name of the storage bucket. For
        example, '1G' is a bucket of users with storage size up to 1 Giga.
    :ivar team.StorageBucket.users: The number of people whose storage is in the
        range of this storage bucket.
        '_bucket_value',
                 bucket=None,
        self._bucket_value = bb.NOT_SET
        if bucket is not None:
            self.bucket = bucket
    bucket = bb.Attribute("bucket")
        super(StorageBucket, self)._process_custom_annotations(annotation_type, field_path, processor)
StorageBucket_validator = bv.Struct(StorageBucket)
class TeamFolderAccessError(bb.Union):
    :ivar team.TeamFolderAccessError.invalid_team_folder_id: The team folder ID
    :ivar team.TeamFolderAccessError.no_access: The authenticated app does not
        have permission to manage that team folder.
    invalid_team_folder_id = None
    def is_invalid_team_folder_id(self):
        Check if the union tag is ``invalid_team_folder_id``.
        return self._tag == 'invalid_team_folder_id'
        super(TeamFolderAccessError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderAccessError_validator = bv.Union(TeamFolderAccessError)
class TeamFolderActivateError(BaseTeamFolderError):
        super(TeamFolderActivateError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderActivateError_validator = bv.Union(TeamFolderActivateError)
class TeamFolderIdArg(bb.Struct):
    :ivar team.TeamFolderIdArg.team_folder_id: The ID of the team folder.
        '_team_folder_id_value',
                 team_folder_id=None):
        self._team_folder_id_value = bb.NOT_SET
        if team_folder_id is not None:
            self.team_folder_id = team_folder_id
    team_folder_id = bb.Attribute("team_folder_id")
        super(TeamFolderIdArg, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderIdArg_validator = bv.Struct(TeamFolderIdArg)
class TeamFolderArchiveArg(TeamFolderIdArg):
    :ivar team.TeamFolderArchiveArg.force_async_off: Whether to force the
        archive to happen synchronously.
        '_force_async_off_value',
                 team_folder_id=None,
                 force_async_off=None):
        super(TeamFolderArchiveArg, self).__init__(team_folder_id)
        self._force_async_off_value = bb.NOT_SET
        if force_async_off is not None:
            self.force_async_off = force_async_off
    force_async_off = bb.Attribute("force_async_off")
        super(TeamFolderArchiveArg, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderArchiveArg_validator = bv.Struct(TeamFolderArchiveArg)
class TeamFolderArchiveError(BaseTeamFolderError):
        super(TeamFolderArchiveError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderArchiveError_validator = bv.Union(TeamFolderArchiveError)
class TeamFolderArchiveJobStatus(async_.PollResultBase):
    :ivar TeamFolderMetadata TeamFolderArchiveJobStatus.complete: The archive
        job has finished. The value is the metadata for the resulting team
    :ivar TeamFolderArchiveError TeamFolderArchiveJobStatus.failed: Error
        occurred while performing an asynchronous job from
        :meth:`dropbox.dropbox_client.Dropbox.team_team_folder_archive`.
        :param TeamFolderMetadata val:
        :rtype: TeamFolderArchiveJobStatus
        :param TeamFolderArchiveError val:
        The archive job has finished. The value is the metadata for the
        resulting team folder.
        :rtype: TeamFolderMetadata
        :rtype: TeamFolderArchiveError
        super(TeamFolderArchiveJobStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderArchiveJobStatus_validator = bv.Union(TeamFolderArchiveJobStatus)
class TeamFolderArchiveLaunch(async_.LaunchResultBase):
        :rtype: TeamFolderArchiveLaunch
        super(TeamFolderArchiveLaunch, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderArchiveLaunch_validator = bv.Union(TeamFolderArchiveLaunch)
class TeamFolderCreateArg(bb.Struct):
    :ivar team.TeamFolderCreateArg.name: Name for the new team folder.
    :ivar team.TeamFolderCreateArg.sync_setting: The sync setting to apply to
        this team folder. Only permitted if the team has team selective sync
        enabled.
    # Instance attribute type: files.SyncSettingArg (validator is set below)
    sync_setting = bb.Attribute("sync_setting", nullable=True, user_defined=True)
        super(TeamFolderCreateArg, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderCreateArg_validator = bv.Struct(TeamFolderCreateArg)
class TeamFolderCreateError(bb.Union):
    :ivar team.TeamFolderCreateError.invalid_folder_name: The provided name
        cannot be used.
    :ivar team.TeamFolderCreateError.folder_name_already_used: There is already
        a team folder with the provided name.
    :ivar team.TeamFolderCreateError.folder_name_reserved: The provided name
        cannot be used because it is reserved.
    :ivar SyncSettingsError TeamFolderCreateError.sync_settings_error: An error
        occurred setting the sync settings.
    invalid_folder_name = None
    folder_name_already_used = None
    folder_name_reserved = None
    def sync_settings_error(cls, val):
        Create an instance of this class set to the ``sync_settings_error`` tag
        :param files.SyncSettingsError val:
        :rtype: TeamFolderCreateError
        return cls('sync_settings_error', val)
    def is_invalid_folder_name(self):
        Check if the union tag is ``invalid_folder_name``.
        return self._tag == 'invalid_folder_name'
    def is_folder_name_already_used(self):
        Check if the union tag is ``folder_name_already_used``.
        return self._tag == 'folder_name_already_used'
    def is_folder_name_reserved(self):
        Check if the union tag is ``folder_name_reserved``.
        return self._tag == 'folder_name_reserved'
    def is_sync_settings_error(self):
        Check if the union tag is ``sync_settings_error``.
        return self._tag == 'sync_settings_error'
    def get_sync_settings_error(self):
        An error occurred setting the sync settings.
        Only call this if :meth:`is_sync_settings_error` is true.
        :rtype: files.SyncSettingsError
        if not self.is_sync_settings_error():
            raise AttributeError("tag 'sync_settings_error' not set")
        super(TeamFolderCreateError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderCreateError_validator = bv.Union(TeamFolderCreateError)
class TeamFolderGetInfoItem(bb.Union):
    :ivar str team.TeamFolderGetInfoItem.id_not_found: An ID that was provided
        as a parameter to :route:`team_folder/get_info` did not match any of the
        team's team folders.
    :ivar TeamFolderMetadata TeamFolderGetInfoItem.team_folder_metadata:
        Properties of a team folder.
        :rtype: TeamFolderGetInfoItem
    def team_folder_metadata(cls, val):
        Create an instance of this class set to the ``team_folder_metadata`` tag
        return cls('team_folder_metadata', val)
    def is_team_folder_metadata(self):
        Check if the union tag is ``team_folder_metadata``.
        return self._tag == 'team_folder_metadata'
        :meth:`dropbox.dropbox_client.Dropbox.team_team_folder_get_info` did not
        match any of the team's team folders.
    def get_team_folder_metadata(self):
        Only call this if :meth:`is_team_folder_metadata` is true.
        if not self.is_team_folder_metadata():
            raise AttributeError("tag 'team_folder_metadata' not set")
        super(TeamFolderGetInfoItem, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderGetInfoItem_validator = bv.Union(TeamFolderGetInfoItem)
class TeamFolderIdListArg(bb.Struct):
    :ivar team.TeamFolderIdListArg.team_folder_ids: The list of team folder IDs.
        '_team_folder_ids_value',
                 team_folder_ids=None):
        self._team_folder_ids_value = bb.NOT_SET
        if team_folder_ids is not None:
            self.team_folder_ids = team_folder_ids
    team_folder_ids = bb.Attribute("team_folder_ids")
        super(TeamFolderIdListArg, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderIdListArg_validator = bv.Struct(TeamFolderIdListArg)
class TeamFolderInvalidStatusError(bb.Union):
    :ivar team.TeamFolderInvalidStatusError.active: The folder is active and the
        operation did not succeed.
    :ivar team.TeamFolderInvalidStatusError.archived: The folder is archived and
        the operation did not succeed.
    :ivar team.TeamFolderInvalidStatusError.archive_in_progress: The folder is
        being archived and the operation did not succeed.
    archived = None
    archive_in_progress = None
    def is_archived(self):
        Check if the union tag is ``archived``.
        return self._tag == 'archived'
    def is_archive_in_progress(self):
        Check if the union tag is ``archive_in_progress``.
        return self._tag == 'archive_in_progress'
        super(TeamFolderInvalidStatusError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderInvalidStatusError_validator = bv.Union(TeamFolderInvalidStatusError)
class TeamFolderListArg(bb.Struct):
    :ivar team.TeamFolderListArg.limit: The maximum number of results to return
        super(TeamFolderListArg, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderListArg_validator = bv.Struct(TeamFolderListArg)
class TeamFolderListContinueArg(bb.Struct):
    :ivar team.TeamFolderListContinueArg.cursor: Indicates from what point to
        get the next set of team folders.
        super(TeamFolderListContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderListContinueArg_validator = bv.Struct(TeamFolderListContinueArg)
class TeamFolderListContinueError(bb.Union):
    :ivar team.TeamFolderListContinueError.invalid_cursor: The cursor is
        super(TeamFolderListContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderListContinueError_validator = bv.Union(TeamFolderListContinueError)
class TeamFolderListError(bb.Struct):
        '_access_error_value',
                 access_error=None):
        self._access_error_value = bb.NOT_SET
        if access_error is not None:
            self.access_error = access_error
    # Instance attribute type: TeamFolderAccessError (validator is set below)
    access_error = bb.Attribute("access_error", user_defined=True)
        super(TeamFolderListError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderListError_validator = bv.Struct(TeamFolderListError)
class TeamFolderListResult(bb.Struct):
    Result for :meth:`dropbox.dropbox_client.Dropbox.team_team_folder_list` and
    :meth:`dropbox.dropbox_client.Dropbox.team_team_folder_list_continue`.
    :ivar team.TeamFolderListResult.team_folders: List of all team folders in
        the authenticated team.
    :ivar team.TeamFolderListResult.cursor: Pass the cursor into
        :meth:`dropbox.dropbox_client.Dropbox.team_team_folder_list_continue` to
        obtain additional team folders.
    :ivar team.TeamFolderListResult.has_more: Is true if there are additional
        team folders that have not been returned yet. An additional call to
        :meth:`dropbox.dropbox_client.Dropbox.team_team_folder_list_continue`
        '_team_folders_value',
                 team_folders=None,
        self._team_folders_value = bb.NOT_SET
        if team_folders is not None:
            self.team_folders = team_folders
    # Instance attribute type: list of [TeamFolderMetadata] (validator is set below)
    team_folders = bb.Attribute("team_folders")
        super(TeamFolderListResult, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderListResult_validator = bv.Struct(TeamFolderListResult)
class TeamFolderMetadata(bb.Struct):
    :ivar team.TeamFolderMetadata.team_folder_id: The ID of the team folder.
    :ivar team.TeamFolderMetadata.name: The name of the team folder.
    :ivar team.TeamFolderMetadata.status: The status of the team folder.
    :ivar team.TeamFolderMetadata.is_team_shared_dropbox: True if this team
        folder is a shared team root.
    :ivar team.TeamFolderMetadata.sync_setting: The sync setting applied to this
    :ivar team.TeamFolderMetadata.content_sync_settings: Sync settings applied
        to contents of this team folder.
        '_is_team_shared_dropbox_value',
        '_content_sync_settings_value',
                 is_team_shared_dropbox=None,
        self._is_team_shared_dropbox_value = bb.NOT_SET
        self._content_sync_settings_value = bb.NOT_SET
        if is_team_shared_dropbox is not None:
            self.is_team_shared_dropbox = is_team_shared_dropbox
        if content_sync_settings is not None:
            self.content_sync_settings = content_sync_settings
    # Instance attribute type: TeamFolderStatus (validator is set below)
    is_team_shared_dropbox = bb.Attribute("is_team_shared_dropbox")
    # Instance attribute type: files.SyncSetting (validator is set below)
    # Instance attribute type: list of [files.ContentSyncSetting] (validator is set below)
    content_sync_settings = bb.Attribute("content_sync_settings")
        super(TeamFolderMetadata, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderMetadata_validator = bv.Struct(TeamFolderMetadata)
class TeamFolderPermanentlyDeleteError(BaseTeamFolderError):
        super(TeamFolderPermanentlyDeleteError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderPermanentlyDeleteError_validator = bv.Union(TeamFolderPermanentlyDeleteError)
class TeamFolderRenameArg(TeamFolderIdArg):
    :ivar team.TeamFolderRenameArg.name: New team folder name.
        super(TeamFolderRenameArg, self).__init__(team_folder_id)
        super(TeamFolderRenameArg, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderRenameArg_validator = bv.Struct(TeamFolderRenameArg)
class TeamFolderRenameError(BaseTeamFolderError):
    :ivar team.TeamFolderRenameError.invalid_folder_name: The provided folder
        name cannot be used.
    :ivar team.TeamFolderRenameError.folder_name_already_used: There is already
        a team folder with the same name.
    :ivar team.TeamFolderRenameError.folder_name_reserved: The provided name
        super(TeamFolderRenameError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderRenameError_validator = bv.Union(TeamFolderRenameError)
class TeamFolderStatus(bb.Union):
    :ivar team.TeamFolderStatus.active: The team folder and sub-folders are
        available to all members.
    :ivar team.TeamFolderStatus.archived: The team folder is not accessible
        outside of the team folder manager.
    :ivar team.TeamFolderStatus.archive_in_progress: The team folder is not
        accessible outside of the team folder manager.
        super(TeamFolderStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderStatus_validator = bv.Union(TeamFolderStatus)
class TeamFolderTeamSharedDropboxError(bb.Union):
    :ivar team.TeamFolderTeamSharedDropboxError.disallowed: This action is not
        allowed for a shared team root.
    disallowed = None
    def is_disallowed(self):
        Check if the union tag is ``disallowed``.
        return self._tag == 'disallowed'
        super(TeamFolderTeamSharedDropboxError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderTeamSharedDropboxError_validator = bv.Union(TeamFolderTeamSharedDropboxError)
class TeamFolderUpdateSyncSettingsArg(TeamFolderIdArg):
    :ivar team.TeamFolderUpdateSyncSettingsArg.sync_setting: Sync setting to
        apply to the team folder itself. Only meaningful if the team folder is
        not a shared team root.
    :ivar team.TeamFolderUpdateSyncSettingsArg.content_sync_settings: Sync
        settings to apply to contents of this team folder.
        super(TeamFolderUpdateSyncSettingsArg, self).__init__(team_folder_id)
    # Instance attribute type: list of [files.ContentSyncSettingArg] (validator is set below)
    content_sync_settings = bb.Attribute("content_sync_settings", nullable=True)
        super(TeamFolderUpdateSyncSettingsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderUpdateSyncSettingsArg_validator = bv.Struct(TeamFolderUpdateSyncSettingsArg)
class TeamFolderUpdateSyncSettingsError(BaseTeamFolderError):
    :ivar SyncSettingsError
        TeamFolderUpdateSyncSettingsError.sync_settings_error: An error occurred
        setting the sync settings.
        :rtype: TeamFolderUpdateSyncSettingsError
        super(TeamFolderUpdateSyncSettingsError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamFolderUpdateSyncSettingsError_validator = bv.Union(TeamFolderUpdateSyncSettingsError)
class TeamGetInfoResult(bb.Struct):
    :ivar team.TeamGetInfoResult.name: The name of the team.
    :ivar team.TeamGetInfoResult.team_id: The ID of the team.
    :ivar team.TeamGetInfoResult.num_licensed_users: The number of licenses
        available to the team.
    :ivar team.TeamGetInfoResult.num_provisioned_users: The number of accounts
        that have been invited or are already active members of the team.
    :ivar team.TeamGetInfoResult.num_used_licenses: The number of licenses used
        on the team.
        '_team_id_value',
        '_num_licensed_users_value',
        '_num_provisioned_users_value',
        '_num_used_licenses_value',
                 team_id=None,
                 num_licensed_users=None,
                 num_provisioned_users=None,
                 policies=None,
                 num_used_licenses=None):
        self._team_id_value = bb.NOT_SET
        self._num_licensed_users_value = bb.NOT_SET
        self._num_provisioned_users_value = bb.NOT_SET
        self._num_used_licenses_value = bb.NOT_SET
        if team_id is not None:
            self.team_id = team_id
        if num_licensed_users is not None:
            self.num_licensed_users = num_licensed_users
        if num_provisioned_users is not None:
            self.num_provisioned_users = num_provisioned_users
        if num_used_licenses is not None:
            self.num_used_licenses = num_used_licenses
    team_id = bb.Attribute("team_id")
    num_licensed_users = bb.Attribute("num_licensed_users")
    num_provisioned_users = bb.Attribute("num_provisioned_users")
    num_used_licenses = bb.Attribute("num_used_licenses")
    # Instance attribute type: team_policies.TeamMemberPolicies (validator is set below)
    policies = bb.Attribute("policies", user_defined=True)
        super(TeamGetInfoResult, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamGetInfoResult_validator = bv.Struct(TeamGetInfoResult)
    :ivar team.TeamMemberInfo.profile: Profile of a user as a member of a team.
    :ivar team.TeamMemberInfo.role: The user's role in the team.
    # Instance attribute type: TeamMemberProfile (validator is set below)
class TeamMemberInfoV2(bb.Struct):
    :ivar team.TeamMemberInfoV2.profile: Profile of a user as a member of a
    :ivar team.TeamMemberInfoV2.roles: The user's roles in the team.
        super(TeamMemberInfoV2, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamMemberInfoV2_validator = bv.Struct(TeamMemberInfoV2)
class TeamMemberInfoV2Result(bb.Struct):
    Information about a team member, after the change, like at
    :meth:`dropbox.dropbox_client.Dropbox.team_members_set_profile`.
    :ivar team.TeamMemberInfoV2Result.member_info: Member info, after the
        change.
        '_member_info_value',
                 member_info=None):
        self._member_info_value = bb.NOT_SET
        if member_info is not None:
            self.member_info = member_info
    # Instance attribute type: TeamMemberInfoV2 (validator is set below)
    member_info = bb.Attribute("member_info", user_defined=True)
        super(TeamMemberInfoV2Result, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamMemberInfoV2Result_validator = bv.Struct(TeamMemberInfoV2Result)
class TeamMemberProfile(MemberProfile):
    Profile of a user as a member of a team.
    :ivar team.TeamMemberProfile.groups: List of group IDs of groups that the
        user belongs to.
    :ivar team.TeamMemberProfile.member_folder_id: The namespace id of the
        user's root folder.
        '_member_folder_id_value',
                 member_folder_id=None,
        super(TeamMemberProfile, self).__init__(team_member_id,
                                                email_verified,
                                                membership_type,
                                                external_id,
                                                secondary_emails,
                                                invited_on,
                                                joined_on,
                                                suspended_on,
                                                persistent_id,
                                                is_directory_restricted,
                                                profile_photo_url)
        self._member_folder_id_value = bb.NOT_SET
        if member_folder_id is not None:
            self.member_folder_id = member_folder_id
    member_folder_id = bb.Attribute("member_folder_id")
        super(TeamMemberProfile, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamMemberProfile_validator = bv.Struct(TeamMemberProfile)
class TeamMemberRole(bb.Struct):
    A role which can be attached to a team member. This replaces AdminTier; each
    AdminTier corresponds to a new TeamMemberRole with a matching name.
    :ivar team.TeamMemberRole.role_id: A string containing encoded role ID. For
        roles defined by Dropbox, this is the same across all teams.
    :ivar team.TeamMemberRole.name: The role display name.
    :ivar team.TeamMemberRole.description: Role description. Describes which
        permissions come with this role.
        '_role_id_value',
                 role_id=None,
        self._role_id_value = bb.NOT_SET
        if role_id is not None:
            self.role_id = role_id
    role_id = bb.Attribute("role_id")
        super(TeamMemberRole, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamMemberRole_validator = bv.Struct(TeamMemberRole)
class TeamMemberStatus(bb.Union):
    The user's status as a member of a specific team.
    :ivar team.TeamMemberStatus.active: User has successfully joined the team.
    :ivar team.TeamMemberStatus.invited: User has been invited to a team, but
        has not joined the team yet.
    :ivar team.TeamMemberStatus.suspended: User is no longer a member of the
        team, but the account can be un-suspended, re-establishing the user as a
    :ivar RemovedStatus TeamMemberStatus.removed: User is no longer a member of
        the team. Removed users are only listed when include_removed is true in
        members/list.
    invited = None
    suspended = None
    def removed(cls, val):
        Create an instance of this class set to the ``removed`` tag with value
        :param RemovedStatus val:
        :rtype: TeamMemberStatus
        return cls('removed', val)
    def is_invited(self):
        Check if the union tag is ``invited``.
        return self._tag == 'invited'
    def is_suspended(self):
        Check if the union tag is ``suspended``.
        return self._tag == 'suspended'
    def is_removed(self):
        Check if the union tag is ``removed``.
        return self._tag == 'removed'
    def get_removed(self):
        User is no longer a member of the team. Removed users are only listed
        when include_removed is true in members/list.
        Only call this if :meth:`is_removed` is true.
        :rtype: RemovedStatus
        if not self.is_removed():
            raise AttributeError("tag 'removed' not set")
        super(TeamMemberStatus, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamMemberStatus_validator = bv.Union(TeamMemberStatus)
class TeamMembershipType(bb.Union):
    :ivar team.TeamMembershipType.full: User uses a license and has full access
        to team resources like the shared quota.
    :ivar team.TeamMembershipType.limited: User does not have access to the
        shared quota and team admins have restricted administrative control.
    full = None
    limited = None
    def is_full(self):
        Check if the union tag is ``full``.
        return self._tag == 'full'
    def is_limited(self):
        Check if the union tag is ``limited``.
        return self._tag == 'limited'
        super(TeamMembershipType, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamMembershipType_validator = bv.Union(TeamMembershipType)
class TeamNamespacesListArg(bb.Struct):
    :ivar team.TeamNamespacesListArg.limit: Specifying a value here has no
        effect.
        super(TeamNamespacesListArg, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamNamespacesListArg_validator = bv.Struct(TeamNamespacesListArg)
class TeamNamespacesListContinueArg(bb.Struct):
    :ivar team.TeamNamespacesListContinueArg.cursor: Indicates from what point
        to get the next set of team-accessible namespaces.
        super(TeamNamespacesListContinueArg, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamNamespacesListContinueArg_validator = bv.Struct(TeamNamespacesListContinueArg)
class TeamNamespacesListError(bb.Union):
    :ivar team.TeamNamespacesListError.invalid_arg: Argument passed in is
    invalid_arg = None
    def is_invalid_arg(self):
        Check if the union tag is ``invalid_arg``.
        return self._tag == 'invalid_arg'
        super(TeamNamespacesListError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamNamespacesListError_validator = bv.Union(TeamNamespacesListError)
class TeamNamespacesListContinueError(TeamNamespacesListError):
    :ivar team.TeamNamespacesListContinueError.invalid_cursor: The cursor is
        super(TeamNamespacesListContinueError, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamNamespacesListContinueError_validator = bv.Union(TeamNamespacesListContinueError)
class TeamNamespacesListResult(bb.Struct):
    Result for :meth:`dropbox.dropbox_client.Dropbox.team_namespaces_list`.
    :ivar team.TeamNamespacesListResult.namespaces: List of all namespaces the
        team can access.
    :ivar team.TeamNamespacesListResult.cursor: Pass the cursor into
        :meth:`dropbox.dropbox_client.Dropbox.team_namespaces_list_continue` to
        obtain additional namespaces. Note that duplicate namespaces may be
    :ivar team.TeamNamespacesListResult.has_more: Is true if there are
        additional namespaces that have not been returned yet.
        '_namespaces_value',
                 namespaces=None,
        self._namespaces_value = bb.NOT_SET
        if namespaces is not None:
            self.namespaces = namespaces
    # Instance attribute type: list of [NamespaceMetadata] (validator is set below)
    namespaces = bb.Attribute("namespaces")
        super(TeamNamespacesListResult, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamNamespacesListResult_validator = bv.Struct(TeamNamespacesListResult)
class TeamReportFailureReason(bb.Union):
    :ivar team.TeamReportFailureReason.temporary_error: We couldn't create the
        report, but we think this was a fluke. Everything should work if you try
        it again.
    :ivar team.TeamReportFailureReason.many_reports_at_once: Too many other
        reports are being created right now. Try creating this report again once
        the others finish.
    :ivar team.TeamReportFailureReason.too_much_data: We couldn't create the
        report. Try creating the report again with less data.
    temporary_error = None
    many_reports_at_once = None
    too_much_data = None
    def is_temporary_error(self):
        Check if the union tag is ``temporary_error``.
        return self._tag == 'temporary_error'
    def is_many_reports_at_once(self):
        Check if the union tag is ``many_reports_at_once``.
        return self._tag == 'many_reports_at_once'
    def is_too_much_data(self):
        Check if the union tag is ``too_much_data``.
        return self._tag == 'too_much_data'
        super(TeamReportFailureReason, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamReportFailureReason_validator = bv.Union(TeamReportFailureReason)
class TokenGetAuthenticatedAdminError(bb.Union):
    :meth:`dropbox.dropbox_client.Dropbox.team_token_get_authenticated_admin`.
    :ivar team.TokenGetAuthenticatedAdminError.mapping_not_found: The current
        token is not associated with a team admin, because mappings were not
        recorded when the token was created. Consider re-authorizing a new
        access token to record its authenticating admin.
    :ivar team.TokenGetAuthenticatedAdminError.admin_not_active: Either the team
        admin that authorized this token is no longer an active member of the
        team or no longer a team admin.
    mapping_not_found = None
    admin_not_active = None
    def is_mapping_not_found(self):
        Check if the union tag is ``mapping_not_found``.
        return self._tag == 'mapping_not_found'
    def is_admin_not_active(self):
        Check if the union tag is ``admin_not_active``.
        return self._tag == 'admin_not_active'
        super(TokenGetAuthenticatedAdminError, self)._process_custom_annotations(annotation_type, field_path, processor)
TokenGetAuthenticatedAdminError_validator = bv.Union(TokenGetAuthenticatedAdminError)
class TokenGetAuthenticatedAdminResult(bb.Struct):
    Results for
    :ivar team.TokenGetAuthenticatedAdminResult.admin_profile: The admin who
        authorized the token.
        '_admin_profile_value',
                 admin_profile=None):
        self._admin_profile_value = bb.NOT_SET
        if admin_profile is not None:
            self.admin_profile = admin_profile
    admin_profile = bb.Attribute("admin_profile", user_defined=True)
        super(TokenGetAuthenticatedAdminResult, self)._process_custom_annotations(annotation_type, field_path, processor)
TokenGetAuthenticatedAdminResult_validator = bv.Struct(TokenGetAuthenticatedAdminResult)
class UploadApiRateLimitValue(bb.Union):
    The value for ``Feature.upload_api_rate_limit``.
    :ivar team.UploadApiRateLimitValue.unlimited: This team has unlimited upload
        API quota. So far both server version account and legacy  account type
        have unlimited monthly upload api quota.
    :ivar int team.UploadApiRateLimitValue.limit: The number of upload API calls
    unlimited = None
    def limit(cls, val):
        Create an instance of this class set to the ``limit`` tag with value
        return cls('limit', val)
    def is_unlimited(self):
        Check if the union tag is ``unlimited``.
        return self._tag == 'unlimited'
    def is_limit(self):
        Check if the union tag is ``limit``.
        return self._tag == 'limit'
    def get_limit(self):
        The number of upload API calls allowed per month.
        Only call this if :meth:`is_limit` is true.
        if not self.is_limit():
            raise AttributeError("tag 'limit' not set")
        super(UploadApiRateLimitValue, self)._process_custom_annotations(annotation_type, field_path, processor)
UploadApiRateLimitValue_validator = bv.Union(UploadApiRateLimitValue)
class UserAddResult(bb.Union):
    Result of trying to add secondary emails to a user. 'success' is the only
    value indicating that a user was successfully retrieved for adding secondary
    emails. The other values explain the type of error that occurred, and
    include the user for which the error occurred.
    :ivar UserSecondaryEmailsResult UserAddResult.success: Describes a user and
        the results for each attempt to add a secondary email.
    :ivar UserSelectorArg UserAddResult.invalid_user: Specified user is not a
        valid target for adding secondary emails.
    :ivar UserSelectorArg UserAddResult.unverified: Secondary emails can only be
        added to verified users.
    :ivar UserSelectorArg UserAddResult.placeholder_user: Secondary emails
        cannot be added to placeholder users.
        :param UserSecondaryEmailsResult val:
        :rtype: UserAddResult
    def unverified(cls, val):
        Create an instance of this class set to the ``unverified`` tag with
        return cls('unverified', val)
    def placeholder_user(cls, val):
        Create an instance of this class set to the ``placeholder_user`` tag
        return cls('placeholder_user', val)
    def is_unverified(self):
        Check if the union tag is ``unverified``.
        return self._tag == 'unverified'
    def is_placeholder_user(self):
        Check if the union tag is ``placeholder_user``.
        return self._tag == 'placeholder_user'
        Describes a user and the results for each attempt to add a secondary
        email.
        :rtype: UserSecondaryEmailsResult
        Specified user is not a valid target for adding secondary emails.
    def get_unverified(self):
        Secondary emails can only be added to verified users.
        Only call this if :meth:`is_unverified` is true.
        if not self.is_unverified():
            raise AttributeError("tag 'unverified' not set")
    def get_placeholder_user(self):
        Secondary emails cannot be added to placeholder users.
        Only call this if :meth:`is_placeholder_user` is true.
        if not self.is_placeholder_user():
            raise AttributeError("tag 'placeholder_user' not set")
        super(UserAddResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UserAddResult_validator = bv.Union(UserAddResult)
class UserCustomQuotaArg(bb.Struct):
    User and their required custom quota in GB (1 TB = 1024 GB).
        '_quota_gb_value',
                 quota_gb=None):
        self._quota_gb_value = bb.NOT_SET
        if quota_gb is not None:
            self.quota_gb = quota_gb
    quota_gb = bb.Attribute("quota_gb")
        super(UserCustomQuotaArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UserCustomQuotaArg_validator = bv.Struct(UserCustomQuotaArg)
class UserCustomQuotaResult(bb.Struct):
    User and their custom quota in GB (1 TB = 1024 GB).  No quota returns if the
    user has no custom quota set.
    quota_gb = bb.Attribute("quota_gb", nullable=True)
        super(UserCustomQuotaResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UserCustomQuotaResult_validator = bv.Struct(UserCustomQuotaResult)
class UserDeleteEmailsResult(bb.Struct):
    # Instance attribute type: list of [DeleteSecondaryEmailResult] (validator is set below)
        super(UserDeleteEmailsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UserDeleteEmailsResult_validator = bv.Struct(UserDeleteEmailsResult)
class UserDeleteResult(bb.Union):
    Result of trying to delete a user's secondary emails. 'success' is the only
    value indicating that a user was successfully retrieved for deleting
    secondary emails. The other values explain the type of error that occurred,
    and include the user for which the error occurred.
    :ivar UserDeleteEmailsResult UserDeleteResult.success: Describes a user and
        the results for each attempt to delete a secondary email.
    :ivar UserSelectorArg UserDeleteResult.invalid_user: Specified user is not a
        valid target for deleting secondary emails.
        :param UserDeleteEmailsResult val:
        :rtype: UserDeleteResult
        Describes a user and the results for each attempt to delete a secondary
        :rtype: UserDeleteEmailsResult
        Specified user is not a valid target for deleting secondary emails.
        super(UserDeleteResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UserDeleteResult_validator = bv.Union(UserDeleteResult)
class UserResendEmailsResult(bb.Struct):
    # Instance attribute type: list of [ResendSecondaryEmailResult] (validator is set below)
        super(UserResendEmailsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UserResendEmailsResult_validator = bv.Struct(UserResendEmailsResult)
class UserResendResult(bb.Union):
    Result of trying to resend verification emails to a user. 'success' is the
    only value indicating that a user was successfully retrieved for sending
    verification emails. The other values explain the type of error that
    occurred, and include the user for which the error occurred.
    :ivar UserResendEmailsResult UserResendResult.success: Describes a user and
        the results for each attempt to resend verification emails.
    :ivar UserSelectorArg UserResendResult.invalid_user: Specified user is not a
        valid target for resending verification emails.
        :param UserResendEmailsResult val:
        :rtype: UserResendResult
        Describes a user and the results for each attempt to resend verification
        emails.
        :rtype: UserResendEmailsResult
        Specified user is not a valid target for resending verification emails.
        super(UserResendResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UserResendResult_validator = bv.Union(UserResendResult)
class UserSecondaryEmailsArg(bb.Struct):
    User and a list of secondary emails.
                 secondary_emails=None):
    secondary_emails = bb.Attribute("secondary_emails")
        super(UserSecondaryEmailsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UserSecondaryEmailsArg_validator = bv.Struct(UserSecondaryEmailsArg)
class UserSecondaryEmailsResult(bb.Struct):
    # Instance attribute type: list of [AddSecondaryEmailResult] (validator is set below)
        super(UserSecondaryEmailsResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UserSecondaryEmailsResult_validator = bv.Struct(UserSecondaryEmailsResult)
class UserSelectorArg(bb.Union):
    Argument for selecting a single user, either by team_member_id, external_id
    or email.
    def team_member_id(cls, val):
        Create an instance of this class set to the ``team_member_id`` tag with
        return cls('team_member_id', val)
    def external_id(cls, val):
        Create an instance of this class set to the ``external_id`` tag with
        return cls('external_id', val)
    def is_team_member_id(self):
        Check if the union tag is ``team_member_id``.
        return self._tag == 'team_member_id'
    def is_external_id(self):
        Check if the union tag is ``external_id``.
        return self._tag == 'external_id'
    def get_team_member_id(self):
        Only call this if :meth:`is_team_member_id` is true.
        if not self.is_team_member_id():
            raise AttributeError("tag 'team_member_id' not set")
    def get_external_id(self):
        Only call this if :meth:`is_external_id` is true.
        if not self.is_external_id():
            raise AttributeError("tag 'external_id' not set")
        super(UserSelectorArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UserSelectorArg_validator = bv.Union(UserSelectorArg)
class UsersSelectorArg(bb.Union):
    Argument for selecting a list of users, either by team_member_ids,
    external_ids or emails.
    :ivar list of [str] team.UsersSelectorArg.team_member_ids: List of member
    :ivar list of [str] team.UsersSelectorArg.external_ids: List of external
        user IDs.
    :ivar list of [str] team.UsersSelectorArg.emails: List of email addresses.
    def team_member_ids(cls, val):
        Create an instance of this class set to the ``team_member_ids`` tag with
        :rtype: UsersSelectorArg
        return cls('team_member_ids', val)
    def external_ids(cls, val):
        Create an instance of this class set to the ``external_ids`` tag with
        return cls('external_ids', val)
    def emails(cls, val):
        Create an instance of this class set to the ``emails`` tag with value
        return cls('emails', val)
    def is_team_member_ids(self):
        Check if the union tag is ``team_member_ids``.
        return self._tag == 'team_member_ids'
    def is_external_ids(self):
        Check if the union tag is ``external_ids``.
        return self._tag == 'external_ids'
    def is_emails(self):
        Check if the union tag is ``emails``.
        return self._tag == 'emails'
    def get_team_member_ids(self):
        List of member IDs.
        Only call this if :meth:`is_team_member_ids` is true.
        if not self.is_team_member_ids():
            raise AttributeError("tag 'team_member_ids' not set")
    def get_external_ids(self):
        List of external user IDs.
        Only call this if :meth:`is_external_ids` is true.
        if not self.is_external_ids():
            raise AttributeError("tag 'external_ids' not set")
    def get_emails(self):
        List of email addresses.
        Only call this if :meth:`is_emails` is true.
        if not self.is_emails():
            raise AttributeError("tag 'emails' not set")
        super(UsersSelectorArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UsersSelectorArg_validator = bv.Union(UsersSelectorArg)
GroupsGetInfoResult_validator = bv.List(GroupsGetInfoItem_validator)
LegalHoldId_validator = bv.String(pattern='^pid_dbhid:.+')
LegalHoldPolicyDescription_validator = bv.String(max_length=501)
LegalHoldPolicyName_validator = bv.String(max_length=140)
LegalHoldsGetPolicyResult_validator = LegalHoldPolicy_validator
LegalHoldsGetPolicyResult = LegalHoldPolicy
LegalHoldsPolicyCreateResult_validator = LegalHoldPolicy_validator
LegalHoldsPolicyCreateResult = LegalHoldPolicy
LegalHoldsPolicyUpdateResult_validator = LegalHoldPolicy_validator
LegalHoldsPolicyUpdateResult = LegalHoldPolicy
ListHeldRevisionCursor_validator = bv.String(min_length=1)
MembersGetInfoResult_validator = bv.List(MembersGetInfoItem_validator)
NumberPerDay_validator = bv.List(bv.Nullable(bv.UInt64()))
Path_validator = bv.String(pattern='(/(.|[\\r\\n])*)?')
SecondaryEmail_validator = secondary_emails.SecondaryEmail_validator
SecondaryEmail = secondary_emails.SecondaryEmail
TeamMemberRoleId_validator = bv.String(max_length=128, pattern='pid_dbtmr:.*')
UserQuota_validator = bv.UInt32(min_value=15)
DeviceSession.session_id.validator = bv.String()
DeviceSession.ip_address.validator = bv.Nullable(bv.String())
DeviceSession.country.validator = bv.Nullable(bv.String())
DeviceSession.created.validator = bv.Nullable(common.DropboxTimestamp_validator)
DeviceSession.updated.validator = bv.Nullable(common.DropboxTimestamp_validator)
DeviceSession._all_field_names_ = set([
    'ip_address',
    'updated',
DeviceSession._all_fields_ = [
    ('session_id', DeviceSession.session_id.validator),
    ('ip_address', DeviceSession.ip_address.validator),
    ('country', DeviceSession.country.validator),
    ('created', DeviceSession.created.validator),
    ('updated', DeviceSession.updated.validator),
ActiveWebSession.user_agent.validator = bv.String()
ActiveWebSession.os.validator = bv.String()
ActiveWebSession.browser.validator = bv.String()
ActiveWebSession.expires.validator = bv.Nullable(common.DropboxTimestamp_validator)
ActiveWebSession._all_field_names_ = DeviceSession._all_field_names_.union(set([
    'user_agent',
    'os',
    'browser',
ActiveWebSession._all_fields_ = DeviceSession._all_fields_ + [
    ('user_agent', ActiveWebSession.user_agent.validator),
    ('os', ActiveWebSession.os.validator),
    ('browser', ActiveWebSession.browser.validator),
    ('expires', ActiveWebSession.expires.validator),
AddSecondaryEmailResult._success_validator = SecondaryEmail_validator
AddSecondaryEmailResult._unavailable_validator = common.EmailAddress_validator
AddSecondaryEmailResult._already_pending_validator = common.EmailAddress_validator
AddSecondaryEmailResult._already_owned_by_user_validator = common.EmailAddress_validator
AddSecondaryEmailResult._reached_limit_validator = common.EmailAddress_validator
AddSecondaryEmailResult._transient_error_validator = common.EmailAddress_validator
AddSecondaryEmailResult._too_many_updates_validator = common.EmailAddress_validator
AddSecondaryEmailResult._unknown_error_validator = common.EmailAddress_validator
AddSecondaryEmailResult._rate_limited_validator = common.EmailAddress_validator
AddSecondaryEmailResult._other_validator = bv.Void()
AddSecondaryEmailResult._tagmap = {
    'success': AddSecondaryEmailResult._success_validator,
    'unavailable': AddSecondaryEmailResult._unavailable_validator,
    'already_pending': AddSecondaryEmailResult._already_pending_validator,
    'already_owned_by_user': AddSecondaryEmailResult._already_owned_by_user_validator,
    'reached_limit': AddSecondaryEmailResult._reached_limit_validator,
    'transient_error': AddSecondaryEmailResult._transient_error_validator,
    'too_many_updates': AddSecondaryEmailResult._too_many_updates_validator,
    'unknown_error': AddSecondaryEmailResult._unknown_error_validator,
    'rate_limited': AddSecondaryEmailResult._rate_limited_validator,
    'other': AddSecondaryEmailResult._other_validator,
AddSecondaryEmailResult.other = AddSecondaryEmailResult('other')
AddSecondaryEmailsArg.new_secondary_emails.validator = bv.List(UserSecondaryEmailsArg_validator)
AddSecondaryEmailsArg._all_field_names_ = set(['new_secondary_emails'])
AddSecondaryEmailsArg._all_fields_ = [('new_secondary_emails', AddSecondaryEmailsArg.new_secondary_emails.validator)]
AddSecondaryEmailsError._secondary_emails_disabled_validator = bv.Void()
AddSecondaryEmailsError._too_many_emails_validator = bv.Void()
AddSecondaryEmailsError._other_validator = bv.Void()
AddSecondaryEmailsError._tagmap = {
    'secondary_emails_disabled': AddSecondaryEmailsError._secondary_emails_disabled_validator,
    'too_many_emails': AddSecondaryEmailsError._too_many_emails_validator,
    'other': AddSecondaryEmailsError._other_validator,
AddSecondaryEmailsError.secondary_emails_disabled = AddSecondaryEmailsError('secondary_emails_disabled')
AddSecondaryEmailsError.too_many_emails = AddSecondaryEmailsError('too_many_emails')
AddSecondaryEmailsError.other = AddSecondaryEmailsError('other')
AddSecondaryEmailsResult.results.validator = bv.List(UserAddResult_validator)
AddSecondaryEmailsResult._all_field_names_ = set(['results'])
AddSecondaryEmailsResult._all_fields_ = [('results', AddSecondaryEmailsResult.results.validator)]
AdminTier._team_admin_validator = bv.Void()
AdminTier._user_management_admin_validator = bv.Void()
AdminTier._support_admin_validator = bv.Void()
AdminTier._member_only_validator = bv.Void()
AdminTier._tagmap = {
    'team_admin': AdminTier._team_admin_validator,
    'user_management_admin': AdminTier._user_management_admin_validator,
    'support_admin': AdminTier._support_admin_validator,
    'member_only': AdminTier._member_only_validator,
AdminTier.team_admin = AdminTier('team_admin')
AdminTier.user_management_admin = AdminTier('user_management_admin')
AdminTier.support_admin = AdminTier('support_admin')
AdminTier.member_only = AdminTier('member_only')
ApiApp.app_id.validator = bv.String()
ApiApp.app_name.validator = bv.String()
ApiApp.publisher.validator = bv.Nullable(bv.String())
ApiApp.publisher_url.validator = bv.Nullable(bv.String())
ApiApp.linked.validator = bv.Nullable(common.DropboxTimestamp_validator)
ApiApp.is_app_folder.validator = bv.Boolean()
ApiApp._all_field_names_ = set([
    'app_id',
    'app_name',
    'publisher',
    'publisher_url',
    'linked',
    'is_app_folder',
ApiApp._all_fields_ = [
    ('app_id', ApiApp.app_id.validator),
    ('app_name', ApiApp.app_name.validator),
    ('publisher', ApiApp.publisher.validator),
    ('publisher_url', ApiApp.publisher_url.validator),
    ('linked', ApiApp.linked.validator),
    ('is_app_folder', ApiApp.is_app_folder.validator),
BaseDfbReport.start_date.validator = bv.String()
BaseDfbReport._all_field_names_ = set(['start_date'])
BaseDfbReport._all_fields_ = [('start_date', BaseDfbReport.start_date.validator)]
BaseTeamFolderError._access_error_validator = TeamFolderAccessError_validator
BaseTeamFolderError._status_error_validator = TeamFolderInvalidStatusError_validator
BaseTeamFolderError._team_shared_dropbox_error_validator = TeamFolderTeamSharedDropboxError_validator
BaseTeamFolderError._other_validator = bv.Void()
BaseTeamFolderError._tagmap = {
    'access_error': BaseTeamFolderError._access_error_validator,
    'status_error': BaseTeamFolderError._status_error_validator,
    'team_shared_dropbox_error': BaseTeamFolderError._team_shared_dropbox_error_validator,
    'other': BaseTeamFolderError._other_validator,
BaseTeamFolderError.other = BaseTeamFolderError('other')
CustomQuotaError._too_many_users_validator = bv.Void()
CustomQuotaError._other_validator = bv.Void()
CustomQuotaError._tagmap = {
    'too_many_users': CustomQuotaError._too_many_users_validator,
    'other': CustomQuotaError._other_validator,
CustomQuotaError.too_many_users = CustomQuotaError('too_many_users')
CustomQuotaError.other = CustomQuotaError('other')
CustomQuotaResult._success_validator = UserCustomQuotaResult_validator
CustomQuotaResult._invalid_user_validator = UserSelectorArg_validator
CustomQuotaResult._other_validator = bv.Void()
CustomQuotaResult._tagmap = {
    'success': CustomQuotaResult._success_validator,
    'invalid_user': CustomQuotaResult._invalid_user_validator,
    'other': CustomQuotaResult._other_validator,
CustomQuotaResult.other = CustomQuotaResult('other')
CustomQuotaUsersArg.users.validator = bv.List(UserSelectorArg_validator)
CustomQuotaUsersArg._all_field_names_ = set(['users'])
CustomQuotaUsersArg._all_fields_ = [('users', CustomQuotaUsersArg.users.validator)]
DateRange.start_date.validator = bv.Nullable(common.Date_validator)
DateRange.end_date.validator = bv.Nullable(common.Date_validator)
DateRange._all_field_names_ = set([
    'start_date',
    'end_date',
DateRange._all_fields_ = [
    ('start_date', DateRange.start_date.validator),
    ('end_date', DateRange.end_date.validator),
DateRangeError._other_validator = bv.Void()
DateRangeError._tagmap = {
    'other': DateRangeError._other_validator,
DateRangeError.other = DateRangeError('other')
DeleteSecondaryEmailResult._success_validator = common.EmailAddress_validator
DeleteSecondaryEmailResult._not_found_validator = common.EmailAddress_validator
DeleteSecondaryEmailResult._cannot_remove_primary_validator = common.EmailAddress_validator
DeleteSecondaryEmailResult._other_validator = bv.Void()
DeleteSecondaryEmailResult._tagmap = {
    'success': DeleteSecondaryEmailResult._success_validator,
    'not_found': DeleteSecondaryEmailResult._not_found_validator,
    'cannot_remove_primary': DeleteSecondaryEmailResult._cannot_remove_primary_validator,
    'other': DeleteSecondaryEmailResult._other_validator,
DeleteSecondaryEmailResult.other = DeleteSecondaryEmailResult('other')
DeleteSecondaryEmailsArg.emails_to_delete.validator = bv.List(UserSecondaryEmailsArg_validator)
DeleteSecondaryEmailsArg._all_field_names_ = set(['emails_to_delete'])
DeleteSecondaryEmailsArg._all_fields_ = [('emails_to_delete', DeleteSecondaryEmailsArg.emails_to_delete.validator)]
DeleteSecondaryEmailsResult.results.validator = bv.List(UserDeleteResult_validator)
DeleteSecondaryEmailsResult._all_field_names_ = set(['results'])
DeleteSecondaryEmailsResult._all_fields_ = [('results', DeleteSecondaryEmailsResult.results.validator)]
DesktopClientSession.host_name.validator = bv.String()
DesktopClientSession.client_type.validator = DesktopPlatform_validator
DesktopClientSession.client_version.validator = bv.String()
DesktopClientSession.platform.validator = bv.String()
DesktopClientSession.is_delete_on_unlink_supported.validator = bv.Boolean()
DesktopClientSession._all_field_names_ = DeviceSession._all_field_names_.union(set([
    'host_name',
    'client_type',
    'client_version',
    'platform',
    'is_delete_on_unlink_supported',
DesktopClientSession._all_fields_ = DeviceSession._all_fields_ + [
    ('host_name', DesktopClientSession.host_name.validator),
    ('client_type', DesktopClientSession.client_type.validator),
    ('client_version', DesktopClientSession.client_version.validator),
    ('platform', DesktopClientSession.platform.validator),
    ('is_delete_on_unlink_supported', DesktopClientSession.is_delete_on_unlink_supported.validator),
DesktopPlatform._windows_validator = bv.Void()
DesktopPlatform._mac_validator = bv.Void()
DesktopPlatform._linux_validator = bv.Void()
DesktopPlatform._other_validator = bv.Void()
DesktopPlatform._tagmap = {
    'windows': DesktopPlatform._windows_validator,
    'mac': DesktopPlatform._mac_validator,
    'linux': DesktopPlatform._linux_validator,
    'other': DesktopPlatform._other_validator,
DesktopPlatform.windows = DesktopPlatform('windows')
DesktopPlatform.mac = DesktopPlatform('mac')
DesktopPlatform.linux = DesktopPlatform('linux')
DesktopPlatform.other = DesktopPlatform('other')
DeviceSessionArg.session_id.validator = bv.String()
DeviceSessionArg.team_member_id.validator = bv.String()
DeviceSessionArg._all_field_names_ = set([
DeviceSessionArg._all_fields_ = [
    ('session_id', DeviceSessionArg.session_id.validator),
    ('team_member_id', DeviceSessionArg.team_member_id.validator),
DevicesActive.windows.validator = NumberPerDay_validator
DevicesActive.macos.validator = NumberPerDay_validator
DevicesActive.linux.validator = NumberPerDay_validator
DevicesActive.ios.validator = NumberPerDay_validator
DevicesActive.android.validator = NumberPerDay_validator
DevicesActive.other.validator = NumberPerDay_validator
DevicesActive.total.validator = NumberPerDay_validator
DevicesActive._all_field_names_ = set([
    'windows',
    'macos',
    'ios',
    'android',
    'other',
    'total',
DevicesActive._all_fields_ = [
    ('windows', DevicesActive.windows.validator),
    ('macos', DevicesActive.macos.validator),
    ('linux', DevicesActive.linux.validator),
    ('ios', DevicesActive.ios.validator),
    ('android', DevicesActive.android.validator),
    ('other', DevicesActive.other.validator),
    ('total', DevicesActive.total.validator),
ExcludedUsersListArg.limit.validator = bv.UInt32(min_value=1, max_value=1000)
ExcludedUsersListArg._all_field_names_ = set(['limit'])
ExcludedUsersListArg._all_fields_ = [('limit', ExcludedUsersListArg.limit.validator)]
ExcludedUsersListContinueArg.cursor.validator = bv.String()
ExcludedUsersListContinueArg._all_field_names_ = set(['cursor'])
ExcludedUsersListContinueArg._all_fields_ = [('cursor', ExcludedUsersListContinueArg.cursor.validator)]
ExcludedUsersListContinueError._invalid_cursor_validator = bv.Void()
ExcludedUsersListContinueError._other_validator = bv.Void()
ExcludedUsersListContinueError._tagmap = {
    'invalid_cursor': ExcludedUsersListContinueError._invalid_cursor_validator,
    'other': ExcludedUsersListContinueError._other_validator,
ExcludedUsersListContinueError.invalid_cursor = ExcludedUsersListContinueError('invalid_cursor')
ExcludedUsersListContinueError.other = ExcludedUsersListContinueError('other')
ExcludedUsersListError._list_error_validator = bv.Void()
ExcludedUsersListError._other_validator = bv.Void()
ExcludedUsersListError._tagmap = {
    'list_error': ExcludedUsersListError._list_error_validator,
    'other': ExcludedUsersListError._other_validator,
ExcludedUsersListError.list_error = ExcludedUsersListError('list_error')
ExcludedUsersListError.other = ExcludedUsersListError('other')
ExcludedUsersListResult.users.validator = bv.List(MemberProfile_validator)
ExcludedUsersListResult.cursor.validator = bv.Nullable(bv.String())
ExcludedUsersListResult.has_more.validator = bv.Boolean()
ExcludedUsersListResult._all_field_names_ = set([
ExcludedUsersListResult._all_fields_ = [
    ('users', ExcludedUsersListResult.users.validator),
    ('cursor', ExcludedUsersListResult.cursor.validator),
    ('has_more', ExcludedUsersListResult.has_more.validator),
ExcludedUsersUpdateArg.users.validator = bv.Nullable(bv.List(UserSelectorArg_validator))
ExcludedUsersUpdateArg._all_field_names_ = set(['users'])
ExcludedUsersUpdateArg._all_fields_ = [('users', ExcludedUsersUpdateArg.users.validator)]
ExcludedUsersUpdateError._users_not_in_team_validator = bv.Void()
ExcludedUsersUpdateError._too_many_users_validator = bv.Void()
ExcludedUsersUpdateError._other_validator = bv.Void()
ExcludedUsersUpdateError._tagmap = {
    'users_not_in_team': ExcludedUsersUpdateError._users_not_in_team_validator,
    'too_many_users': ExcludedUsersUpdateError._too_many_users_validator,
    'other': ExcludedUsersUpdateError._other_validator,
ExcludedUsersUpdateError.users_not_in_team = ExcludedUsersUpdateError('users_not_in_team')
ExcludedUsersUpdateError.too_many_users = ExcludedUsersUpdateError('too_many_users')
ExcludedUsersUpdateError.other = ExcludedUsersUpdateError('other')
ExcludedUsersUpdateResult.status.validator = ExcludedUsersUpdateStatus_validator
ExcludedUsersUpdateResult._all_field_names_ = set(['status'])
ExcludedUsersUpdateResult._all_fields_ = [('status', ExcludedUsersUpdateResult.status.validator)]
ExcludedUsersUpdateStatus._success_validator = bv.Void()
ExcludedUsersUpdateStatus._other_validator = bv.Void()
ExcludedUsersUpdateStatus._tagmap = {
    'success': ExcludedUsersUpdateStatus._success_validator,
    'other': ExcludedUsersUpdateStatus._other_validator,
ExcludedUsersUpdateStatus.success = ExcludedUsersUpdateStatus('success')
ExcludedUsersUpdateStatus.other = ExcludedUsersUpdateStatus('other')
Feature._upload_api_rate_limit_validator = bv.Void()
Feature._has_team_shared_dropbox_validator = bv.Void()
Feature._has_team_file_events_validator = bv.Void()
Feature._has_team_selective_sync_validator = bv.Void()
Feature._other_validator = bv.Void()
Feature._tagmap = {
    'upload_api_rate_limit': Feature._upload_api_rate_limit_validator,
    'has_team_shared_dropbox': Feature._has_team_shared_dropbox_validator,
    'has_team_file_events': Feature._has_team_file_events_validator,
    'has_team_selective_sync': Feature._has_team_selective_sync_validator,
    'other': Feature._other_validator,
Feature.upload_api_rate_limit = Feature('upload_api_rate_limit')
Feature.has_team_shared_dropbox = Feature('has_team_shared_dropbox')
Feature.has_team_file_events = Feature('has_team_file_events')
Feature.has_team_selective_sync = Feature('has_team_selective_sync')
Feature.other = Feature('other')
FeatureValue._upload_api_rate_limit_validator = UploadApiRateLimitValue_validator
FeatureValue._has_team_shared_dropbox_validator = HasTeamSharedDropboxValue_validator
FeatureValue._has_team_file_events_validator = HasTeamFileEventsValue_validator
FeatureValue._has_team_selective_sync_validator = HasTeamSelectiveSyncValue_validator
FeatureValue._other_validator = bv.Void()
FeatureValue._tagmap = {
    'upload_api_rate_limit': FeatureValue._upload_api_rate_limit_validator,
    'has_team_shared_dropbox': FeatureValue._has_team_shared_dropbox_validator,
    'has_team_file_events': FeatureValue._has_team_file_events_validator,
    'has_team_selective_sync': FeatureValue._has_team_selective_sync_validator,
    'other': FeatureValue._other_validator,
FeatureValue.other = FeatureValue('other')
FeaturesGetValuesBatchArg.features.validator = bv.List(Feature_validator)
FeaturesGetValuesBatchArg._all_field_names_ = set(['features'])
FeaturesGetValuesBatchArg._all_fields_ = [('features', FeaturesGetValuesBatchArg.features.validator)]
FeaturesGetValuesBatchError._empty_features_list_validator = bv.Void()
FeaturesGetValuesBatchError._other_validator = bv.Void()
FeaturesGetValuesBatchError._tagmap = {
    'empty_features_list': FeaturesGetValuesBatchError._empty_features_list_validator,
    'other': FeaturesGetValuesBatchError._other_validator,
FeaturesGetValuesBatchError.empty_features_list = FeaturesGetValuesBatchError('empty_features_list')
FeaturesGetValuesBatchError.other = FeaturesGetValuesBatchError('other')
FeaturesGetValuesBatchResult.values.validator = bv.List(FeatureValue_validator)
FeaturesGetValuesBatchResult._all_field_names_ = set(['values'])
FeaturesGetValuesBatchResult._all_fields_ = [('values', FeaturesGetValuesBatchResult.values.validator)]
GetActivityReport.adds.validator = NumberPerDay_validator
GetActivityReport.edits.validator = NumberPerDay_validator
GetActivityReport.deletes.validator = NumberPerDay_validator
GetActivityReport.active_users_28_day.validator = NumberPerDay_validator
GetActivityReport.active_users_7_day.validator = NumberPerDay_validator
GetActivityReport.active_users_1_day.validator = NumberPerDay_validator
GetActivityReport.active_shared_folders_28_day.validator = NumberPerDay_validator
GetActivityReport.active_shared_folders_7_day.validator = NumberPerDay_validator
GetActivityReport.active_shared_folders_1_day.validator = NumberPerDay_validator
GetActivityReport.shared_links_created.validator = NumberPerDay_validator
GetActivityReport.shared_links_viewed_by_team.validator = NumberPerDay_validator
GetActivityReport.shared_links_viewed_by_outside_user.validator = NumberPerDay_validator
GetActivityReport.shared_links_viewed_by_not_logged_in.validator = NumberPerDay_validator
GetActivityReport.shared_links_viewed_total.validator = NumberPerDay_validator
GetActivityReport._all_field_names_ = BaseDfbReport._all_field_names_.union(set([
    'adds',
    'edits',
    'deletes',
    'active_users_28_day',
    'active_users_7_day',
    'active_users_1_day',
    'active_shared_folders_28_day',
    'active_shared_folders_7_day',
    'active_shared_folders_1_day',
    'shared_links_created',
    'shared_links_viewed_by_team',
    'shared_links_viewed_by_outside_user',
    'shared_links_viewed_by_not_logged_in',
    'shared_links_viewed_total',
GetActivityReport._all_fields_ = BaseDfbReport._all_fields_ + [
    ('adds', GetActivityReport.adds.validator),
    ('edits', GetActivityReport.edits.validator),
    ('deletes', GetActivityReport.deletes.validator),
    ('active_users_28_day', GetActivityReport.active_users_28_day.validator),
    ('active_users_7_day', GetActivityReport.active_users_7_day.validator),
    ('active_users_1_day', GetActivityReport.active_users_1_day.validator),
    ('active_shared_folders_28_day', GetActivityReport.active_shared_folders_28_day.validator),
    ('active_shared_folders_7_day', GetActivityReport.active_shared_folders_7_day.validator),
    ('active_shared_folders_1_day', GetActivityReport.active_shared_folders_1_day.validator),
    ('shared_links_created', GetActivityReport.shared_links_created.validator),
    ('shared_links_viewed_by_team', GetActivityReport.shared_links_viewed_by_team.validator),
    ('shared_links_viewed_by_outside_user', GetActivityReport.shared_links_viewed_by_outside_user.validator),
    ('shared_links_viewed_by_not_logged_in', GetActivityReport.shared_links_viewed_by_not_logged_in.validator),
    ('shared_links_viewed_total', GetActivityReport.shared_links_viewed_total.validator),
GetDevicesReport.active_1_day.validator = DevicesActive_validator
GetDevicesReport.active_7_day.validator = DevicesActive_validator
GetDevicesReport.active_28_day.validator = DevicesActive_validator
GetDevicesReport._all_field_names_ = BaseDfbReport._all_field_names_.union(set([
    'active_1_day',
    'active_7_day',
    'active_28_day',
GetDevicesReport._all_fields_ = BaseDfbReport._all_fields_ + [
    ('active_1_day', GetDevicesReport.active_1_day.validator),
    ('active_7_day', GetDevicesReport.active_7_day.validator),
    ('active_28_day', GetDevicesReport.active_28_day.validator),
GetMembershipReport.team_size.validator = NumberPerDay_validator
GetMembershipReport.pending_invites.validator = NumberPerDay_validator
GetMembershipReport.members_joined.validator = NumberPerDay_validator
GetMembershipReport.suspended_members.validator = NumberPerDay_validator
GetMembershipReport.licenses.validator = NumberPerDay_validator
GetMembershipReport._all_field_names_ = BaseDfbReport._all_field_names_.union(set([
    'team_size',
    'pending_invites',
    'members_joined',
    'suspended_members',
    'licenses',
GetMembershipReport._all_fields_ = BaseDfbReport._all_fields_ + [
    ('team_size', GetMembershipReport.team_size.validator),
    ('pending_invites', GetMembershipReport.pending_invites.validator),
    ('members_joined', GetMembershipReport.members_joined.validator),
    ('suspended_members', GetMembershipReport.suspended_members.validator),
    ('licenses', GetMembershipReport.licenses.validator),
GetStorageReport.total_usage.validator = NumberPerDay_validator
GetStorageReport.shared_usage.validator = NumberPerDay_validator
GetStorageReport.unshared_usage.validator = NumberPerDay_validator
GetStorageReport.shared_folders.validator = NumberPerDay_validator
GetStorageReport.member_storage_map.validator = bv.List(bv.List(StorageBucket_validator))
GetStorageReport._all_field_names_ = BaseDfbReport._all_field_names_.union(set([
    'total_usage',
    'shared_usage',
    'unshared_usage',
    'shared_folders',
    'member_storage_map',
GetStorageReport._all_fields_ = BaseDfbReport._all_fields_ + [
    ('total_usage', GetStorageReport.total_usage.validator),
    ('shared_usage', GetStorageReport.shared_usage.validator),
    ('unshared_usage', GetStorageReport.unshared_usage.validator),
    ('shared_folders', GetStorageReport.shared_folders.validator),
    ('member_storage_map', GetStorageReport.member_storage_map.validator),
GroupAccessType._member_validator = bv.Void()
GroupAccessType._owner_validator = bv.Void()
GroupAccessType._tagmap = {
    'member': GroupAccessType._member_validator,
    'owner': GroupAccessType._owner_validator,
GroupAccessType.member = GroupAccessType('member')
GroupAccessType.owner = GroupAccessType('owner')
GroupCreateArg.group_name.validator = bv.String()
GroupCreateArg.add_creator_as_owner.validator = bv.Boolean()
GroupCreateArg.group_external_id.validator = bv.Nullable(team_common.GroupExternalId_validator)
GroupCreateArg.group_management_type.validator = bv.Nullable(team_common.GroupManagementType_validator)
GroupCreateArg._all_field_names_ = set([
    'group_name',
    'add_creator_as_owner',
    'group_external_id',
    'group_management_type',
GroupCreateArg._all_fields_ = [
    ('group_name', GroupCreateArg.group_name.validator),
    ('add_creator_as_owner', GroupCreateArg.add_creator_as_owner.validator),
    ('group_external_id', GroupCreateArg.group_external_id.validator),
    ('group_management_type', GroupCreateArg.group_management_type.validator),
GroupCreateError._group_name_already_used_validator = bv.Void()
GroupCreateError._group_name_invalid_validator = bv.Void()
GroupCreateError._external_id_already_in_use_validator = bv.Void()
GroupCreateError._system_managed_group_disallowed_validator = bv.Void()
GroupCreateError._other_validator = bv.Void()
GroupCreateError._tagmap = {
    'group_name_already_used': GroupCreateError._group_name_already_used_validator,
    'group_name_invalid': GroupCreateError._group_name_invalid_validator,
    'external_id_already_in_use': GroupCreateError._external_id_already_in_use_validator,
    'system_managed_group_disallowed': GroupCreateError._system_managed_group_disallowed_validator,
    'other': GroupCreateError._other_validator,
GroupCreateError.group_name_already_used = GroupCreateError('group_name_already_used')
GroupCreateError.group_name_invalid = GroupCreateError('group_name_invalid')
GroupCreateError.external_id_already_in_use = GroupCreateError('external_id_already_in_use')
GroupCreateError.system_managed_group_disallowed = GroupCreateError('system_managed_group_disallowed')
GroupCreateError.other = GroupCreateError('other')
GroupSelectorError._group_not_found_validator = bv.Void()
GroupSelectorError._other_validator = bv.Void()
GroupSelectorError._tagmap = {
    'group_not_found': GroupSelectorError._group_not_found_validator,
    'other': GroupSelectorError._other_validator,
GroupSelectorError.group_not_found = GroupSelectorError('group_not_found')
GroupSelectorError.other = GroupSelectorError('other')
GroupSelectorWithTeamGroupError._system_managed_group_disallowed_validator = bv.Void()
GroupSelectorWithTeamGroupError._tagmap = {
    'system_managed_group_disallowed': GroupSelectorWithTeamGroupError._system_managed_group_disallowed_validator,
GroupSelectorWithTeamGroupError._tagmap.update(GroupSelectorError._tagmap)
GroupSelectorWithTeamGroupError.system_managed_group_disallowed = GroupSelectorWithTeamGroupError('system_managed_group_disallowed')
GroupDeleteError._group_already_deleted_validator = bv.Void()
GroupDeleteError._tagmap = {
    'group_already_deleted': GroupDeleteError._group_already_deleted_validator,
GroupDeleteError._tagmap.update(GroupSelectorWithTeamGroupError._tagmap)
GroupDeleteError.group_already_deleted = GroupDeleteError('group_already_deleted')
GroupFullInfo.members.validator = bv.Nullable(bv.List(GroupMemberInfo_validator))
GroupFullInfo.created.validator = bv.UInt64()
GroupFullInfo._all_field_names_ = team_common.GroupSummary._all_field_names_.union(set([
GroupFullInfo._all_fields_ = team_common.GroupSummary._all_fields_ + [
    ('members', GroupFullInfo.members.validator),
    ('created', GroupFullInfo.created.validator),
GroupMemberInfo.profile.validator = MemberProfile_validator
GroupMemberInfo.access_type.validator = GroupAccessType_validator
GroupMemberInfo._all_field_names_ = set([
    'profile',
GroupMemberInfo._all_fields_ = [
    ('profile', GroupMemberInfo.profile.validator),
    ('access_type', GroupMemberInfo.access_type.validator),
GroupMemberSelector.group.validator = GroupSelector_validator
GroupMemberSelector.user.validator = UserSelectorArg_validator
GroupMemberSelector._all_field_names_ = set([
    'group',
GroupMemberSelector._all_fields_ = [
    ('group', GroupMemberSelector.group.validator),
    ('user', GroupMemberSelector.user.validator),
GroupMemberSelectorError._member_not_in_group_validator = bv.Void()
GroupMemberSelectorError._tagmap = {
    'member_not_in_group': GroupMemberSelectorError._member_not_in_group_validator,
GroupMemberSelectorError._tagmap.update(GroupSelectorWithTeamGroupError._tagmap)
GroupMemberSelectorError.member_not_in_group = GroupMemberSelectorError('member_not_in_group')
GroupMemberSetAccessTypeError._user_cannot_be_manager_of_company_managed_group_validator = bv.Void()
GroupMemberSetAccessTypeError._tagmap = {
    'user_cannot_be_manager_of_company_managed_group': GroupMemberSetAccessTypeError._user_cannot_be_manager_of_company_managed_group_validator,
GroupMemberSetAccessTypeError._tagmap.update(GroupMemberSelectorError._tagmap)
GroupMemberSetAccessTypeError.user_cannot_be_manager_of_company_managed_group = GroupMemberSetAccessTypeError('user_cannot_be_manager_of_company_managed_group')
IncludeMembersArg.return_members.validator = bv.Boolean()
IncludeMembersArg._all_field_names_ = set(['return_members'])
IncludeMembersArg._all_fields_ = [('return_members', IncludeMembersArg.return_members.validator)]
GroupMembersAddArg.group.validator = GroupSelector_validator
GroupMembersAddArg.members.validator = bv.List(MemberAccess_validator)
GroupMembersAddArg._all_field_names_ = IncludeMembersArg._all_field_names_.union(set([
GroupMembersAddArg._all_fields_ = IncludeMembersArg._all_fields_ + [
    ('group', GroupMembersAddArg.group.validator),
    ('members', GroupMembersAddArg.members.validator),
GroupMembersAddError._duplicate_user_validator = bv.Void()
GroupMembersAddError._group_not_in_team_validator = bv.Void()
GroupMembersAddError._members_not_in_team_validator = bv.List(bv.String())
GroupMembersAddError._users_not_found_validator = bv.List(bv.String())
GroupMembersAddError._user_must_be_active_to_be_owner_validator = bv.Void()
GroupMembersAddError._user_cannot_be_manager_of_company_managed_group_validator = bv.List(bv.String())
GroupMembersAddError._tagmap = {
    'duplicate_user': GroupMembersAddError._duplicate_user_validator,
    'group_not_in_team': GroupMembersAddError._group_not_in_team_validator,
    'members_not_in_team': GroupMembersAddError._members_not_in_team_validator,
    'users_not_found': GroupMembersAddError._users_not_found_validator,
    'user_must_be_active_to_be_owner': GroupMembersAddError._user_must_be_active_to_be_owner_validator,
    'user_cannot_be_manager_of_company_managed_group': GroupMembersAddError._user_cannot_be_manager_of_company_managed_group_validator,
GroupMembersAddError._tagmap.update(GroupSelectorWithTeamGroupError._tagmap)
GroupMembersAddError.duplicate_user = GroupMembersAddError('duplicate_user')
GroupMembersAddError.group_not_in_team = GroupMembersAddError('group_not_in_team')
GroupMembersAddError.user_must_be_active_to_be_owner = GroupMembersAddError('user_must_be_active_to_be_owner')
GroupMembersChangeResult.group_info.validator = GroupFullInfo_validator
GroupMembersChangeResult.async_job_id.validator = async_.AsyncJobId_validator
GroupMembersChangeResult._all_field_names_ = set([
    'group_info',
    'async_job_id',
GroupMembersChangeResult._all_fields_ = [
    ('group_info', GroupMembersChangeResult.group_info.validator),
    ('async_job_id', GroupMembersChangeResult.async_job_id.validator),
GroupMembersRemoveArg.group.validator = GroupSelector_validator
GroupMembersRemoveArg.users.validator = bv.List(UserSelectorArg_validator)
GroupMembersRemoveArg._all_field_names_ = IncludeMembersArg._all_field_names_.union(set([
GroupMembersRemoveArg._all_fields_ = IncludeMembersArg._all_fields_ + [
    ('group', GroupMembersRemoveArg.group.validator),
    ('users', GroupMembersRemoveArg.users.validator),
GroupMembersSelectorError._member_not_in_group_validator = bv.Void()
GroupMembersSelectorError._tagmap = {
    'member_not_in_group': GroupMembersSelectorError._member_not_in_group_validator,
GroupMembersSelectorError._tagmap.update(GroupSelectorWithTeamGroupError._tagmap)
GroupMembersSelectorError.member_not_in_group = GroupMembersSelectorError('member_not_in_group')
GroupMembersRemoveError._group_not_in_team_validator = bv.Void()
GroupMembersRemoveError._members_not_in_team_validator = bv.List(bv.String())
GroupMembersRemoveError._users_not_found_validator = bv.List(bv.String())
GroupMembersRemoveError._tagmap = {
    'group_not_in_team': GroupMembersRemoveError._group_not_in_team_validator,
    'members_not_in_team': GroupMembersRemoveError._members_not_in_team_validator,
    'users_not_found': GroupMembersRemoveError._users_not_found_validator,
GroupMembersRemoveError._tagmap.update(GroupMembersSelectorError._tagmap)
GroupMembersRemoveError.group_not_in_team = GroupMembersRemoveError('group_not_in_team')
GroupMembersSelector.group.validator = GroupSelector_validator
GroupMembersSelector.users.validator = UsersSelectorArg_validator
GroupMembersSelector._all_field_names_ = set([
GroupMembersSelector._all_fields_ = [
    ('group', GroupMembersSelector.group.validator),
    ('users', GroupMembersSelector.users.validator),
GroupMembersSetAccessTypeArg.access_type.validator = GroupAccessType_validator
GroupMembersSetAccessTypeArg.return_members.validator = bv.Boolean()
GroupMembersSetAccessTypeArg._all_field_names_ = GroupMemberSelector._all_field_names_.union(set([
    'return_members',
GroupMembersSetAccessTypeArg._all_fields_ = GroupMemberSelector._all_fields_ + [
    ('access_type', GroupMembersSetAccessTypeArg.access_type.validator),
    ('return_members', GroupMembersSetAccessTypeArg.return_members.validator),
GroupSelector._group_id_validator = team_common.GroupId_validator
GroupSelector._group_external_id_validator = team_common.GroupExternalId_validator
GroupSelector._tagmap = {
    'group_id': GroupSelector._group_id_validator,
    'group_external_id': GroupSelector._group_external_id_validator,
GroupUpdateArgs.group.validator = GroupSelector_validator
GroupUpdateArgs.new_group_name.validator = bv.Nullable(bv.String())
GroupUpdateArgs.new_group_external_id.validator = bv.Nullable(team_common.GroupExternalId_validator)
GroupUpdateArgs.new_group_management_type.validator = bv.Nullable(team_common.GroupManagementType_validator)
GroupUpdateArgs._all_field_names_ = IncludeMembersArg._all_field_names_.union(set([
    'new_group_name',
    'new_group_external_id',
    'new_group_management_type',
GroupUpdateArgs._all_fields_ = IncludeMembersArg._all_fields_ + [
    ('group', GroupUpdateArgs.group.validator),
    ('new_group_name', GroupUpdateArgs.new_group_name.validator),
    ('new_group_external_id', GroupUpdateArgs.new_group_external_id.validator),
    ('new_group_management_type', GroupUpdateArgs.new_group_management_type.validator),
GroupUpdateError._group_name_already_used_validator = bv.Void()
GroupUpdateError._group_name_invalid_validator = bv.Void()
GroupUpdateError._external_id_already_in_use_validator = bv.Void()
GroupUpdateError._tagmap = {
    'group_name_already_used': GroupUpdateError._group_name_already_used_validator,
    'group_name_invalid': GroupUpdateError._group_name_invalid_validator,
    'external_id_already_in_use': GroupUpdateError._external_id_already_in_use_validator,
GroupUpdateError._tagmap.update(GroupSelectorWithTeamGroupError._tagmap)
GroupUpdateError.group_name_already_used = GroupUpdateError('group_name_already_used')
GroupUpdateError.group_name_invalid = GroupUpdateError('group_name_invalid')
GroupUpdateError.external_id_already_in_use = GroupUpdateError('external_id_already_in_use')
GroupsGetInfoError._group_not_on_team_validator = bv.Void()
GroupsGetInfoError._other_validator = bv.Void()
GroupsGetInfoError._tagmap = {
    'group_not_on_team': GroupsGetInfoError._group_not_on_team_validator,
    'other': GroupsGetInfoError._other_validator,
GroupsGetInfoError.group_not_on_team = GroupsGetInfoError('group_not_on_team')
GroupsGetInfoError.other = GroupsGetInfoError('other')
GroupsGetInfoItem._id_not_found_validator = bv.String()
GroupsGetInfoItem._group_info_validator = GroupFullInfo_validator
GroupsGetInfoItem._tagmap = {
    'id_not_found': GroupsGetInfoItem._id_not_found_validator,
    'group_info': GroupsGetInfoItem._group_info_validator,
GroupsListArg.limit.validator = bv.UInt32(min_value=1, max_value=1000)
GroupsListArg._all_field_names_ = set(['limit'])
GroupsListArg._all_fields_ = [('limit', GroupsListArg.limit.validator)]
GroupsListContinueArg.cursor.validator = bv.String()
GroupsListContinueArg._all_field_names_ = set(['cursor'])
GroupsListContinueArg._all_fields_ = [('cursor', GroupsListContinueArg.cursor.validator)]
GroupsListContinueError._invalid_cursor_validator = bv.Void()
GroupsListContinueError._other_validator = bv.Void()
GroupsListContinueError._tagmap = {
    'invalid_cursor': GroupsListContinueError._invalid_cursor_validator,
    'other': GroupsListContinueError._other_validator,
GroupsListContinueError.invalid_cursor = GroupsListContinueError('invalid_cursor')
GroupsListContinueError.other = GroupsListContinueError('other')
GroupsListResult.groups.validator = bv.List(team_common.GroupSummary_validator)
GroupsListResult.cursor.validator = bv.String()
GroupsListResult.has_more.validator = bv.Boolean()
GroupsListResult._all_field_names_ = set([
GroupsListResult._all_fields_ = [
    ('groups', GroupsListResult.groups.validator),
    ('cursor', GroupsListResult.cursor.validator),
    ('has_more', GroupsListResult.has_more.validator),
GroupsMembersListArg.group.validator = GroupSelector_validator
GroupsMembersListArg.limit.validator = bv.UInt32(min_value=1, max_value=1000)
GroupsMembersListArg._all_field_names_ = set([
GroupsMembersListArg._all_fields_ = [
    ('group', GroupsMembersListArg.group.validator),
    ('limit', GroupsMembersListArg.limit.validator),
GroupsMembersListContinueArg.cursor.validator = bv.String()
GroupsMembersListContinueArg._all_field_names_ = set(['cursor'])
GroupsMembersListContinueArg._all_fields_ = [('cursor', GroupsMembersListContinueArg.cursor.validator)]
GroupsMembersListContinueError._invalid_cursor_validator = bv.Void()
GroupsMembersListContinueError._other_validator = bv.Void()
GroupsMembersListContinueError._tagmap = {
    'invalid_cursor': GroupsMembersListContinueError._invalid_cursor_validator,
    'other': GroupsMembersListContinueError._other_validator,
GroupsMembersListContinueError.invalid_cursor = GroupsMembersListContinueError('invalid_cursor')
GroupsMembersListContinueError.other = GroupsMembersListContinueError('other')
GroupsMembersListResult.members.validator = bv.List(GroupMemberInfo_validator)
GroupsMembersListResult.cursor.validator = bv.String()
GroupsMembersListResult.has_more.validator = bv.Boolean()
GroupsMembersListResult._all_field_names_ = set([
GroupsMembersListResult._all_fields_ = [
    ('members', GroupsMembersListResult.members.validator),
    ('cursor', GroupsMembersListResult.cursor.validator),
    ('has_more', GroupsMembersListResult.has_more.validator),
GroupsPollError._access_denied_validator = bv.Void()
GroupsPollError._tagmap = {
    'access_denied': GroupsPollError._access_denied_validator,
GroupsPollError._tagmap.update(async_.PollError._tagmap)
GroupsPollError.access_denied = GroupsPollError('access_denied')
GroupsSelector._group_ids_validator = bv.List(team_common.GroupId_validator)
GroupsSelector._group_external_ids_validator = bv.List(bv.String())
GroupsSelector._tagmap = {
    'group_ids': GroupsSelector._group_ids_validator,
    'group_external_ids': GroupsSelector._group_external_ids_validator,
HasTeamFileEventsValue._enabled_validator = bv.Boolean()
HasTeamFileEventsValue._other_validator = bv.Void()
HasTeamFileEventsValue._tagmap = {
    'enabled': HasTeamFileEventsValue._enabled_validator,
    'other': HasTeamFileEventsValue._other_validator,
HasTeamFileEventsValue.other = HasTeamFileEventsValue('other')
HasTeamSelectiveSyncValue._has_team_selective_sync_validator = bv.Boolean()
HasTeamSelectiveSyncValue._other_validator = bv.Void()
HasTeamSelectiveSyncValue._tagmap = {
    'has_team_selective_sync': HasTeamSelectiveSyncValue._has_team_selective_sync_validator,
    'other': HasTeamSelectiveSyncValue._other_validator,
HasTeamSelectiveSyncValue.other = HasTeamSelectiveSyncValue('other')
HasTeamSharedDropboxValue._has_team_shared_dropbox_validator = bv.Boolean()
HasTeamSharedDropboxValue._other_validator = bv.Void()
HasTeamSharedDropboxValue._tagmap = {
    'has_team_shared_dropbox': HasTeamSharedDropboxValue._has_team_shared_dropbox_validator,
    'other': HasTeamSharedDropboxValue._other_validator,
HasTeamSharedDropboxValue.other = HasTeamSharedDropboxValue('other')
LegalHoldHeldRevisionMetadata.new_filename.validator = bv.String()
LegalHoldHeldRevisionMetadata.original_revision_id.validator = files.Rev_validator
LegalHoldHeldRevisionMetadata.original_file_path.validator = Path_validator
LegalHoldHeldRevisionMetadata.server_modified.validator = common.DropboxTimestamp_validator
LegalHoldHeldRevisionMetadata.author_member_id.validator = team_common.TeamMemberId_validator
LegalHoldHeldRevisionMetadata.author_member_status.validator = TeamMemberStatus_validator
LegalHoldHeldRevisionMetadata.author_email.validator = common.EmailAddress_validator
LegalHoldHeldRevisionMetadata.file_type.validator = bv.String()
LegalHoldHeldRevisionMetadata.size.validator = bv.UInt64()
LegalHoldHeldRevisionMetadata.content_hash.validator = files.Sha256HexHash_validator
LegalHoldHeldRevisionMetadata._all_field_names_ = set([
    'new_filename',
    'original_revision_id',
    'original_file_path',
    'author_member_id',
    'author_member_status',
    'author_email',
    'file_type',
LegalHoldHeldRevisionMetadata._all_fields_ = [
    ('new_filename', LegalHoldHeldRevisionMetadata.new_filename.validator),
    ('original_revision_id', LegalHoldHeldRevisionMetadata.original_revision_id.validator),
    ('original_file_path', LegalHoldHeldRevisionMetadata.original_file_path.validator),
    ('server_modified', LegalHoldHeldRevisionMetadata.server_modified.validator),
    ('author_member_id', LegalHoldHeldRevisionMetadata.author_member_id.validator),
    ('author_member_status', LegalHoldHeldRevisionMetadata.author_member_status.validator),
    ('author_email', LegalHoldHeldRevisionMetadata.author_email.validator),
    ('file_type', LegalHoldHeldRevisionMetadata.file_type.validator),
    ('size', LegalHoldHeldRevisionMetadata.size.validator),
    ('content_hash', LegalHoldHeldRevisionMetadata.content_hash.validator),
LegalHoldPolicy.id.validator = LegalHoldId_validator
LegalHoldPolicy.name.validator = LegalHoldPolicyName_validator
LegalHoldPolicy.description.validator = bv.Nullable(LegalHoldPolicyDescription_validator)
LegalHoldPolicy.activation_time.validator = bv.Nullable(common.DropboxTimestamp_validator)
LegalHoldPolicy.members.validator = MembersInfo_validator
LegalHoldPolicy.status.validator = LegalHoldStatus_validator
LegalHoldPolicy.start_date.validator = common.DropboxTimestamp_validator
LegalHoldPolicy.end_date.validator = bv.Nullable(common.DropboxTimestamp_validator)
LegalHoldPolicy._all_field_names_ = set([
    'activation_time',
LegalHoldPolicy._all_fields_ = [
    ('id', LegalHoldPolicy.id.validator),
    ('name', LegalHoldPolicy.name.validator),
    ('description', LegalHoldPolicy.description.validator),
    ('activation_time', LegalHoldPolicy.activation_time.validator),
    ('members', LegalHoldPolicy.members.validator),
    ('status', LegalHoldPolicy.status.validator),
    ('start_date', LegalHoldPolicy.start_date.validator),
    ('end_date', LegalHoldPolicy.end_date.validator),
LegalHoldStatus._active_validator = bv.Void()
LegalHoldStatus._released_validator = bv.Void()
LegalHoldStatus._activating_validator = bv.Void()
LegalHoldStatus._updating_validator = bv.Void()
LegalHoldStatus._exporting_validator = bv.Void()
LegalHoldStatus._releasing_validator = bv.Void()
LegalHoldStatus._other_validator = bv.Void()
LegalHoldStatus._tagmap = {
    'active': LegalHoldStatus._active_validator,
    'released': LegalHoldStatus._released_validator,
    'activating': LegalHoldStatus._activating_validator,
    'updating': LegalHoldStatus._updating_validator,
    'exporting': LegalHoldStatus._exporting_validator,
    'releasing': LegalHoldStatus._releasing_validator,
    'other': LegalHoldStatus._other_validator,
LegalHoldStatus.active = LegalHoldStatus('active')
LegalHoldStatus.released = LegalHoldStatus('released')
LegalHoldStatus.activating = LegalHoldStatus('activating')
LegalHoldStatus.updating = LegalHoldStatus('updating')
LegalHoldStatus.exporting = LegalHoldStatus('exporting')
LegalHoldStatus.releasing = LegalHoldStatus('releasing')
LegalHoldStatus.other = LegalHoldStatus('other')
LegalHoldsError._unknown_legal_hold_error_validator = bv.Void()
LegalHoldsError._insufficient_permissions_validator = bv.Void()
LegalHoldsError._other_validator = bv.Void()
LegalHoldsError._tagmap = {
    'unknown_legal_hold_error': LegalHoldsError._unknown_legal_hold_error_validator,
    'insufficient_permissions': LegalHoldsError._insufficient_permissions_validator,
    'other': LegalHoldsError._other_validator,
LegalHoldsError.unknown_legal_hold_error = LegalHoldsError('unknown_legal_hold_error')
LegalHoldsError.insufficient_permissions = LegalHoldsError('insufficient_permissions')
LegalHoldsError.other = LegalHoldsError('other')
LegalHoldsGetPolicyArg.id.validator = LegalHoldId_validator
LegalHoldsGetPolicyArg._all_field_names_ = set(['id'])
LegalHoldsGetPolicyArg._all_fields_ = [('id', LegalHoldsGetPolicyArg.id.validator)]
LegalHoldsGetPolicyError._legal_hold_policy_not_found_validator = bv.Void()
LegalHoldsGetPolicyError._tagmap = {
    'legal_hold_policy_not_found': LegalHoldsGetPolicyError._legal_hold_policy_not_found_validator,
LegalHoldsGetPolicyError._tagmap.update(LegalHoldsError._tagmap)
LegalHoldsGetPolicyError.legal_hold_policy_not_found = LegalHoldsGetPolicyError('legal_hold_policy_not_found')
LegalHoldsListHeldRevisionResult.entries.validator = bv.List(LegalHoldHeldRevisionMetadata_validator)
LegalHoldsListHeldRevisionResult.cursor.validator = bv.Nullable(ListHeldRevisionCursor_validator)
LegalHoldsListHeldRevisionResult.has_more.validator = bv.Boolean()
LegalHoldsListHeldRevisionResult._all_field_names_ = set([
LegalHoldsListHeldRevisionResult._all_fields_ = [
    ('entries', LegalHoldsListHeldRevisionResult.entries.validator),
    ('cursor', LegalHoldsListHeldRevisionResult.cursor.validator),
    ('has_more', LegalHoldsListHeldRevisionResult.has_more.validator),
LegalHoldsListHeldRevisionsArg.id.validator = LegalHoldId_validator
LegalHoldsListHeldRevisionsArg._all_field_names_ = set(['id'])
LegalHoldsListHeldRevisionsArg._all_fields_ = [('id', LegalHoldsListHeldRevisionsArg.id.validator)]
LegalHoldsListHeldRevisionsContinueArg.id.validator = LegalHoldId_validator
LegalHoldsListHeldRevisionsContinueArg.cursor.validator = bv.Nullable(ListHeldRevisionCursor_validator)
LegalHoldsListHeldRevisionsContinueArg._all_field_names_ = set([
LegalHoldsListHeldRevisionsContinueArg._all_fields_ = [
    ('id', LegalHoldsListHeldRevisionsContinueArg.id.validator),
    ('cursor', LegalHoldsListHeldRevisionsContinueArg.cursor.validator),
LegalHoldsListHeldRevisionsContinueError._unknown_legal_hold_error_validator = bv.Void()
LegalHoldsListHeldRevisionsContinueError._transient_error_validator = bv.Void()
LegalHoldsListHeldRevisionsContinueError._reset_validator = bv.Void()
LegalHoldsListHeldRevisionsContinueError._other_validator = bv.Void()
LegalHoldsListHeldRevisionsContinueError._tagmap = {
    'unknown_legal_hold_error': LegalHoldsListHeldRevisionsContinueError._unknown_legal_hold_error_validator,
    'transient_error': LegalHoldsListHeldRevisionsContinueError._transient_error_validator,
    'reset': LegalHoldsListHeldRevisionsContinueError._reset_validator,
    'other': LegalHoldsListHeldRevisionsContinueError._other_validator,
LegalHoldsListHeldRevisionsContinueError.unknown_legal_hold_error = LegalHoldsListHeldRevisionsContinueError('unknown_legal_hold_error')
LegalHoldsListHeldRevisionsContinueError.transient_error = LegalHoldsListHeldRevisionsContinueError('transient_error')
LegalHoldsListHeldRevisionsContinueError.reset = LegalHoldsListHeldRevisionsContinueError('reset')
LegalHoldsListHeldRevisionsContinueError.other = LegalHoldsListHeldRevisionsContinueError('other')
LegalHoldsListHeldRevisionsError._transient_error_validator = bv.Void()
LegalHoldsListHeldRevisionsError._legal_hold_still_empty_validator = bv.Void()
LegalHoldsListHeldRevisionsError._inactive_legal_hold_validator = bv.Void()
LegalHoldsListHeldRevisionsError._tagmap = {
    'transient_error': LegalHoldsListHeldRevisionsError._transient_error_validator,
    'legal_hold_still_empty': LegalHoldsListHeldRevisionsError._legal_hold_still_empty_validator,
    'inactive_legal_hold': LegalHoldsListHeldRevisionsError._inactive_legal_hold_validator,
LegalHoldsListHeldRevisionsError._tagmap.update(LegalHoldsError._tagmap)
LegalHoldsListHeldRevisionsError.transient_error = LegalHoldsListHeldRevisionsError('transient_error')
LegalHoldsListHeldRevisionsError.legal_hold_still_empty = LegalHoldsListHeldRevisionsError('legal_hold_still_empty')
LegalHoldsListHeldRevisionsError.inactive_legal_hold = LegalHoldsListHeldRevisionsError('inactive_legal_hold')
LegalHoldsListPoliciesArg.include_released.validator = bv.Boolean()
LegalHoldsListPoliciesArg._all_field_names_ = set(['include_released'])
LegalHoldsListPoliciesArg._all_fields_ = [('include_released', LegalHoldsListPoliciesArg.include_released.validator)]
LegalHoldsListPoliciesError._transient_error_validator = bv.Void()
LegalHoldsListPoliciesError._tagmap = {
    'transient_error': LegalHoldsListPoliciesError._transient_error_validator,
LegalHoldsListPoliciesError._tagmap.update(LegalHoldsError._tagmap)
LegalHoldsListPoliciesError.transient_error = LegalHoldsListPoliciesError('transient_error')
LegalHoldsListPoliciesResult.policies.validator = bv.List(LegalHoldPolicy_validator)
LegalHoldsListPoliciesResult._all_field_names_ = set(['policies'])
LegalHoldsListPoliciesResult._all_fields_ = [('policies', LegalHoldsListPoliciesResult.policies.validator)]
LegalHoldsPolicyCreateArg.name.validator = LegalHoldPolicyName_validator
LegalHoldsPolicyCreateArg.description.validator = bv.Nullable(LegalHoldPolicyDescription_validator)
LegalHoldsPolicyCreateArg.members.validator = bv.List(team_common.TeamMemberId_validator)
LegalHoldsPolicyCreateArg.start_date.validator = bv.Nullable(common.DropboxTimestamp_validator)
LegalHoldsPolicyCreateArg.end_date.validator = bv.Nullable(common.DropboxTimestamp_validator)
LegalHoldsPolicyCreateArg._all_field_names_ = set([
LegalHoldsPolicyCreateArg._all_fields_ = [
    ('name', LegalHoldsPolicyCreateArg.name.validator),
    ('description', LegalHoldsPolicyCreateArg.description.validator),
    ('members', LegalHoldsPolicyCreateArg.members.validator),
    ('start_date', LegalHoldsPolicyCreateArg.start_date.validator),
    ('end_date', LegalHoldsPolicyCreateArg.end_date.validator),
LegalHoldsPolicyCreateError._start_date_is_later_than_end_date_validator = bv.Void()
LegalHoldsPolicyCreateError._empty_members_list_validator = bv.Void()
LegalHoldsPolicyCreateError._invalid_members_validator = bv.Void()
LegalHoldsPolicyCreateError._number_of_users_on_hold_is_greater_than_hold_limitation_validator = bv.Void()
LegalHoldsPolicyCreateError._transient_error_validator = bv.Void()
LegalHoldsPolicyCreateError._name_must_be_unique_validator = bv.Void()
LegalHoldsPolicyCreateError._team_exceeded_legal_hold_quota_validator = bv.Void()
LegalHoldsPolicyCreateError._invalid_date_validator = bv.Void()
LegalHoldsPolicyCreateError._tagmap = {
    'start_date_is_later_than_end_date': LegalHoldsPolicyCreateError._start_date_is_later_than_end_date_validator,
    'empty_members_list': LegalHoldsPolicyCreateError._empty_members_list_validator,
    'invalid_members': LegalHoldsPolicyCreateError._invalid_members_validator,
    'number_of_users_on_hold_is_greater_than_hold_limitation': LegalHoldsPolicyCreateError._number_of_users_on_hold_is_greater_than_hold_limitation_validator,
    'transient_error': LegalHoldsPolicyCreateError._transient_error_validator,
    'name_must_be_unique': LegalHoldsPolicyCreateError._name_must_be_unique_validator,
    'team_exceeded_legal_hold_quota': LegalHoldsPolicyCreateError._team_exceeded_legal_hold_quota_validator,
    'invalid_date': LegalHoldsPolicyCreateError._invalid_date_validator,
LegalHoldsPolicyCreateError._tagmap.update(LegalHoldsError._tagmap)
LegalHoldsPolicyCreateError.start_date_is_later_than_end_date = LegalHoldsPolicyCreateError('start_date_is_later_than_end_date')
LegalHoldsPolicyCreateError.empty_members_list = LegalHoldsPolicyCreateError('empty_members_list')
LegalHoldsPolicyCreateError.invalid_members = LegalHoldsPolicyCreateError('invalid_members')
LegalHoldsPolicyCreateError.number_of_users_on_hold_is_greater_than_hold_limitation = LegalHoldsPolicyCreateError('number_of_users_on_hold_is_greater_than_hold_limitation')
LegalHoldsPolicyCreateError.transient_error = LegalHoldsPolicyCreateError('transient_error')
LegalHoldsPolicyCreateError.name_must_be_unique = LegalHoldsPolicyCreateError('name_must_be_unique')
LegalHoldsPolicyCreateError.team_exceeded_legal_hold_quota = LegalHoldsPolicyCreateError('team_exceeded_legal_hold_quota')
LegalHoldsPolicyCreateError.invalid_date = LegalHoldsPolicyCreateError('invalid_date')
LegalHoldsPolicyReleaseArg.id.validator = LegalHoldId_validator
LegalHoldsPolicyReleaseArg._all_field_names_ = set(['id'])
LegalHoldsPolicyReleaseArg._all_fields_ = [('id', LegalHoldsPolicyReleaseArg.id.validator)]
LegalHoldsPolicyReleaseError._legal_hold_performing_another_operation_validator = bv.Void()
LegalHoldsPolicyReleaseError._legal_hold_already_releasing_validator = bv.Void()
LegalHoldsPolicyReleaseError._legal_hold_policy_not_found_validator = bv.Void()
LegalHoldsPolicyReleaseError._tagmap = {
    'legal_hold_performing_another_operation': LegalHoldsPolicyReleaseError._legal_hold_performing_another_operation_validator,
    'legal_hold_already_releasing': LegalHoldsPolicyReleaseError._legal_hold_already_releasing_validator,
    'legal_hold_policy_not_found': LegalHoldsPolicyReleaseError._legal_hold_policy_not_found_validator,
LegalHoldsPolicyReleaseError._tagmap.update(LegalHoldsError._tagmap)
LegalHoldsPolicyReleaseError.legal_hold_performing_another_operation = LegalHoldsPolicyReleaseError('legal_hold_performing_another_operation')
LegalHoldsPolicyReleaseError.legal_hold_already_releasing = LegalHoldsPolicyReleaseError('legal_hold_already_releasing')
LegalHoldsPolicyReleaseError.legal_hold_policy_not_found = LegalHoldsPolicyReleaseError('legal_hold_policy_not_found')
LegalHoldsPolicyUpdateArg.id.validator = LegalHoldId_validator
LegalHoldsPolicyUpdateArg.name.validator = bv.Nullable(LegalHoldPolicyName_validator)
LegalHoldsPolicyUpdateArg.description.validator = bv.Nullable(LegalHoldPolicyDescription_validator)
LegalHoldsPolicyUpdateArg.members.validator = bv.Nullable(bv.List(team_common.TeamMemberId_validator))
LegalHoldsPolicyUpdateArg._all_field_names_ = set([
LegalHoldsPolicyUpdateArg._all_fields_ = [
    ('id', LegalHoldsPolicyUpdateArg.id.validator),
    ('name', LegalHoldsPolicyUpdateArg.name.validator),
    ('description', LegalHoldsPolicyUpdateArg.description.validator),
    ('members', LegalHoldsPolicyUpdateArg.members.validator),
LegalHoldsPolicyUpdateError._transient_error_validator = bv.Void()
LegalHoldsPolicyUpdateError._inactive_legal_hold_validator = bv.Void()
LegalHoldsPolicyUpdateError._legal_hold_performing_another_operation_validator = bv.Void()
LegalHoldsPolicyUpdateError._invalid_members_validator = bv.Void()
LegalHoldsPolicyUpdateError._number_of_users_on_hold_is_greater_than_hold_limitation_validator = bv.Void()
LegalHoldsPolicyUpdateError._empty_members_list_validator = bv.Void()
LegalHoldsPolicyUpdateError._name_must_be_unique_validator = bv.Void()
LegalHoldsPolicyUpdateError._legal_hold_policy_not_found_validator = bv.Void()
LegalHoldsPolicyUpdateError._tagmap = {
    'transient_error': LegalHoldsPolicyUpdateError._transient_error_validator,
    'inactive_legal_hold': LegalHoldsPolicyUpdateError._inactive_legal_hold_validator,
    'legal_hold_performing_another_operation': LegalHoldsPolicyUpdateError._legal_hold_performing_another_operation_validator,
    'invalid_members': LegalHoldsPolicyUpdateError._invalid_members_validator,
    'number_of_users_on_hold_is_greater_than_hold_limitation': LegalHoldsPolicyUpdateError._number_of_users_on_hold_is_greater_than_hold_limitation_validator,
    'empty_members_list': LegalHoldsPolicyUpdateError._empty_members_list_validator,
    'name_must_be_unique': LegalHoldsPolicyUpdateError._name_must_be_unique_validator,
    'legal_hold_policy_not_found': LegalHoldsPolicyUpdateError._legal_hold_policy_not_found_validator,
LegalHoldsPolicyUpdateError._tagmap.update(LegalHoldsError._tagmap)
LegalHoldsPolicyUpdateError.transient_error = LegalHoldsPolicyUpdateError('transient_error')
LegalHoldsPolicyUpdateError.inactive_legal_hold = LegalHoldsPolicyUpdateError('inactive_legal_hold')
LegalHoldsPolicyUpdateError.legal_hold_performing_another_operation = LegalHoldsPolicyUpdateError('legal_hold_performing_another_operation')
LegalHoldsPolicyUpdateError.invalid_members = LegalHoldsPolicyUpdateError('invalid_members')
LegalHoldsPolicyUpdateError.number_of_users_on_hold_is_greater_than_hold_limitation = LegalHoldsPolicyUpdateError('number_of_users_on_hold_is_greater_than_hold_limitation')
LegalHoldsPolicyUpdateError.empty_members_list = LegalHoldsPolicyUpdateError('empty_members_list')
LegalHoldsPolicyUpdateError.name_must_be_unique = LegalHoldsPolicyUpdateError('name_must_be_unique')
LegalHoldsPolicyUpdateError.legal_hold_policy_not_found = LegalHoldsPolicyUpdateError('legal_hold_policy_not_found')
ListMemberAppsArg.team_member_id.validator = bv.String()
ListMemberAppsArg._all_field_names_ = set(['team_member_id'])
ListMemberAppsArg._all_fields_ = [('team_member_id', ListMemberAppsArg.team_member_id.validator)]
ListMemberAppsError._member_not_found_validator = bv.Void()
ListMemberAppsError._other_validator = bv.Void()
ListMemberAppsError._tagmap = {
    'member_not_found': ListMemberAppsError._member_not_found_validator,
    'other': ListMemberAppsError._other_validator,
ListMemberAppsError.member_not_found = ListMemberAppsError('member_not_found')
ListMemberAppsError.other = ListMemberAppsError('other')
ListMemberAppsResult.linked_api_apps.validator = bv.List(ApiApp_validator)
ListMemberAppsResult._all_field_names_ = set(['linked_api_apps'])
ListMemberAppsResult._all_fields_ = [('linked_api_apps', ListMemberAppsResult.linked_api_apps.validator)]
ListMemberDevicesArg.team_member_id.validator = bv.String()
ListMemberDevicesArg.include_web_sessions.validator = bv.Boolean()
ListMemberDevicesArg.include_desktop_clients.validator = bv.Boolean()
ListMemberDevicesArg.include_mobile_clients.validator = bv.Boolean()
ListMemberDevicesArg._all_field_names_ = set([
    'include_web_sessions',
    'include_desktop_clients',
    'include_mobile_clients',
ListMemberDevicesArg._all_fields_ = [
    ('team_member_id', ListMemberDevicesArg.team_member_id.validator),
    ('include_web_sessions', ListMemberDevicesArg.include_web_sessions.validator),
    ('include_desktop_clients', ListMemberDevicesArg.include_desktop_clients.validator),
    ('include_mobile_clients', ListMemberDevicesArg.include_mobile_clients.validator),
ListMemberDevicesError._member_not_found_validator = bv.Void()
ListMemberDevicesError._other_validator = bv.Void()
ListMemberDevicesError._tagmap = {
    'member_not_found': ListMemberDevicesError._member_not_found_validator,
    'other': ListMemberDevicesError._other_validator,
ListMemberDevicesError.member_not_found = ListMemberDevicesError('member_not_found')
ListMemberDevicesError.other = ListMemberDevicesError('other')
ListMemberDevicesResult.active_web_sessions.validator = bv.Nullable(bv.List(ActiveWebSession_validator))
ListMemberDevicesResult.desktop_client_sessions.validator = bv.Nullable(bv.List(DesktopClientSession_validator))
ListMemberDevicesResult.mobile_client_sessions.validator = bv.Nullable(bv.List(MobileClientSession_validator))
ListMemberDevicesResult._all_field_names_ = set([
    'active_web_sessions',
    'desktop_client_sessions',
    'mobile_client_sessions',
ListMemberDevicesResult._all_fields_ = [
    ('active_web_sessions', ListMemberDevicesResult.active_web_sessions.validator),
    ('desktop_client_sessions', ListMemberDevicesResult.desktop_client_sessions.validator),
    ('mobile_client_sessions', ListMemberDevicesResult.mobile_client_sessions.validator),
ListMembersAppsArg.cursor.validator = bv.Nullable(bv.String())
ListMembersAppsArg._all_field_names_ = set(['cursor'])
ListMembersAppsArg._all_fields_ = [('cursor', ListMembersAppsArg.cursor.validator)]
ListMembersAppsError._reset_validator = bv.Void()
ListMembersAppsError._other_validator = bv.Void()
ListMembersAppsError._tagmap = {
    'reset': ListMembersAppsError._reset_validator,
    'other': ListMembersAppsError._other_validator,
ListMembersAppsError.reset = ListMembersAppsError('reset')
ListMembersAppsError.other = ListMembersAppsError('other')
ListMembersAppsResult.apps.validator = bv.List(MemberLinkedApps_validator)
ListMembersAppsResult.has_more.validator = bv.Boolean()
ListMembersAppsResult.cursor.validator = bv.Nullable(bv.String())
ListMembersAppsResult._all_field_names_ = set([
    'apps',
ListMembersAppsResult._all_fields_ = [
    ('apps', ListMembersAppsResult.apps.validator),
    ('has_more', ListMembersAppsResult.has_more.validator),
    ('cursor', ListMembersAppsResult.cursor.validator),
ListMembersDevicesArg.cursor.validator = bv.Nullable(bv.String())
ListMembersDevicesArg.include_web_sessions.validator = bv.Boolean()
ListMembersDevicesArg.include_desktop_clients.validator = bv.Boolean()
ListMembersDevicesArg.include_mobile_clients.validator = bv.Boolean()
ListMembersDevicesArg._all_field_names_ = set([
ListMembersDevicesArg._all_fields_ = [
    ('cursor', ListMembersDevicesArg.cursor.validator),
    ('include_web_sessions', ListMembersDevicesArg.include_web_sessions.validator),
    ('include_desktop_clients', ListMembersDevicesArg.include_desktop_clients.validator),
    ('include_mobile_clients', ListMembersDevicesArg.include_mobile_clients.validator),
ListMembersDevicesError._reset_validator = bv.Void()
ListMembersDevicesError._other_validator = bv.Void()
ListMembersDevicesError._tagmap = {
    'reset': ListMembersDevicesError._reset_validator,
    'other': ListMembersDevicesError._other_validator,
ListMembersDevicesError.reset = ListMembersDevicesError('reset')
ListMembersDevicesError.other = ListMembersDevicesError('other')
ListMembersDevicesResult.devices.validator = bv.List(MemberDevices_validator)
ListMembersDevicesResult.has_more.validator = bv.Boolean()
ListMembersDevicesResult.cursor.validator = bv.Nullable(bv.String())
ListMembersDevicesResult._all_field_names_ = set([
    'devices',
ListMembersDevicesResult._all_fields_ = [
    ('devices', ListMembersDevicesResult.devices.validator),
    ('has_more', ListMembersDevicesResult.has_more.validator),
    ('cursor', ListMembersDevicesResult.cursor.validator),
ListTeamAppsArg.cursor.validator = bv.Nullable(bv.String())
ListTeamAppsArg._all_field_names_ = set(['cursor'])
ListTeamAppsArg._all_fields_ = [('cursor', ListTeamAppsArg.cursor.validator)]
ListTeamAppsError._reset_validator = bv.Void()
ListTeamAppsError._other_validator = bv.Void()
ListTeamAppsError._tagmap = {
    'reset': ListTeamAppsError._reset_validator,
    'other': ListTeamAppsError._other_validator,
ListTeamAppsError.reset = ListTeamAppsError('reset')
ListTeamAppsError.other = ListTeamAppsError('other')
ListTeamAppsResult.apps.validator = bv.List(MemberLinkedApps_validator)
ListTeamAppsResult.has_more.validator = bv.Boolean()
ListTeamAppsResult.cursor.validator = bv.Nullable(bv.String())
ListTeamAppsResult._all_field_names_ = set([
ListTeamAppsResult._all_fields_ = [
    ('apps', ListTeamAppsResult.apps.validator),
    ('has_more', ListTeamAppsResult.has_more.validator),
    ('cursor', ListTeamAppsResult.cursor.validator),
ListTeamDevicesArg.cursor.validator = bv.Nullable(bv.String())
ListTeamDevicesArg.include_web_sessions.validator = bv.Boolean()
ListTeamDevicesArg.include_desktop_clients.validator = bv.Boolean()
ListTeamDevicesArg.include_mobile_clients.validator = bv.Boolean()
ListTeamDevicesArg._all_field_names_ = set([
ListTeamDevicesArg._all_fields_ = [
    ('cursor', ListTeamDevicesArg.cursor.validator),
    ('include_web_sessions', ListTeamDevicesArg.include_web_sessions.validator),
    ('include_desktop_clients', ListTeamDevicesArg.include_desktop_clients.validator),
    ('include_mobile_clients', ListTeamDevicesArg.include_mobile_clients.validator),
ListTeamDevicesError._reset_validator = bv.Void()
ListTeamDevicesError._other_validator = bv.Void()
ListTeamDevicesError._tagmap = {
    'reset': ListTeamDevicesError._reset_validator,
    'other': ListTeamDevicesError._other_validator,
ListTeamDevicesError.reset = ListTeamDevicesError('reset')
ListTeamDevicesError.other = ListTeamDevicesError('other')
ListTeamDevicesResult.devices.validator = bv.List(MemberDevices_validator)
ListTeamDevicesResult.has_more.validator = bv.Boolean()
ListTeamDevicesResult.cursor.validator = bv.Nullable(bv.String())
ListTeamDevicesResult._all_field_names_ = set([
ListTeamDevicesResult._all_fields_ = [
    ('devices', ListTeamDevicesResult.devices.validator),
    ('has_more', ListTeamDevicesResult.has_more.validator),
    ('cursor', ListTeamDevicesResult.cursor.validator),
MemberAccess.user.validator = UserSelectorArg_validator
MemberAccess.access_type.validator = GroupAccessType_validator
MemberAccess._all_field_names_ = set([
MemberAccess._all_fields_ = [
    ('user', MemberAccess.user.validator),
    ('access_type', MemberAccess.access_type.validator),
MemberAddArgBase.member_email.validator = common.EmailAddress_validator
MemberAddArgBase.member_given_name.validator = bv.Nullable(common.OptionalNamePart_validator)
MemberAddArgBase.member_surname.validator = bv.Nullable(common.OptionalNamePart_validator)
MemberAddArgBase.member_external_id.validator = bv.Nullable(team_common.MemberExternalId_validator)
MemberAddArgBase.member_persistent_id.validator = bv.Nullable(bv.String())
MemberAddArgBase.send_welcome_email.validator = bv.Boolean()
MemberAddArgBase.is_directory_restricted.validator = bv.Nullable(bv.Boolean())
MemberAddArgBase._all_field_names_ = set([
    'member_email',
    'member_given_name',
    'member_surname',
    'member_external_id',
    'member_persistent_id',
    'send_welcome_email',
    'is_directory_restricted',
MemberAddArgBase._all_fields_ = [
    ('member_email', MemberAddArgBase.member_email.validator),
    ('member_given_name', MemberAddArgBase.member_given_name.validator),
    ('member_surname', MemberAddArgBase.member_surname.validator),
    ('member_external_id', MemberAddArgBase.member_external_id.validator),
    ('member_persistent_id', MemberAddArgBase.member_persistent_id.validator),
    ('send_welcome_email', MemberAddArgBase.send_welcome_email.validator),
    ('is_directory_restricted', MemberAddArgBase.is_directory_restricted.validator),
MemberAddArg.role.validator = AdminTier_validator
MemberAddArg._all_field_names_ = MemberAddArgBase._all_field_names_.union(set(['role']))
MemberAddArg._all_fields_ = MemberAddArgBase._all_fields_ + [('role', MemberAddArg.role.validator)]
MemberAddResultBase._team_license_limit_validator = common.EmailAddress_validator
MemberAddResultBase._free_team_member_limit_reached_validator = common.EmailAddress_validator
MemberAddResultBase._user_already_on_team_validator = common.EmailAddress_validator
MemberAddResultBase._user_on_another_team_validator = common.EmailAddress_validator
MemberAddResultBase._user_already_paired_validator = common.EmailAddress_validator
MemberAddResultBase._user_migration_failed_validator = common.EmailAddress_validator
MemberAddResultBase._duplicate_external_member_id_validator = common.EmailAddress_validator
MemberAddResultBase._duplicate_member_persistent_id_validator = common.EmailAddress_validator
MemberAddResultBase._persistent_id_disabled_validator = common.EmailAddress_validator
MemberAddResultBase._user_creation_failed_validator = common.EmailAddress_validator
MemberAddResultBase._tagmap = {
    'team_license_limit': MemberAddResultBase._team_license_limit_validator,
    'free_team_member_limit_reached': MemberAddResultBase._free_team_member_limit_reached_validator,
    'user_already_on_team': MemberAddResultBase._user_already_on_team_validator,
    'user_on_another_team': MemberAddResultBase._user_on_another_team_validator,
    'user_already_paired': MemberAddResultBase._user_already_paired_validator,
    'user_migration_failed': MemberAddResultBase._user_migration_failed_validator,
    'duplicate_external_member_id': MemberAddResultBase._duplicate_external_member_id_validator,
    'duplicate_member_persistent_id': MemberAddResultBase._duplicate_member_persistent_id_validator,
    'persistent_id_disabled': MemberAddResultBase._persistent_id_disabled_validator,
    'user_creation_failed': MemberAddResultBase._user_creation_failed_validator,
MemberAddResult._success_validator = TeamMemberInfo_validator
MemberAddResult._tagmap = {
    'success': MemberAddResult._success_validator,
MemberAddResult._tagmap.update(MemberAddResultBase._tagmap)
MemberAddV2Arg.role_ids.validator = bv.Nullable(bv.List(TeamMemberRoleId_validator, max_items=1))
MemberAddV2Arg._all_field_names_ = MemberAddArgBase._all_field_names_.union(set(['role_ids']))
MemberAddV2Arg._all_fields_ = MemberAddArgBase._all_fields_ + [('role_ids', MemberAddV2Arg.role_ids.validator)]
MemberAddV2Result._success_validator = TeamMemberInfoV2_validator
MemberAddV2Result._other_validator = bv.Void()
MemberAddV2Result._tagmap = {
    'success': MemberAddV2Result._success_validator,
    'other': MemberAddV2Result._other_validator,
MemberAddV2Result._tagmap.update(MemberAddResultBase._tagmap)
MemberAddV2Result.other = MemberAddV2Result('other')
MemberDevices.team_member_id.validator = bv.String()
MemberDevices.web_sessions.validator = bv.Nullable(bv.List(ActiveWebSession_validator))
MemberDevices.desktop_clients.validator = bv.Nullable(bv.List(DesktopClientSession_validator))
MemberDevices.mobile_clients.validator = bv.Nullable(bv.List(MobileClientSession_validator))
MemberDevices._all_field_names_ = set([
    'web_sessions',
    'desktop_clients',
    'mobile_clients',
MemberDevices._all_fields_ = [
    ('team_member_id', MemberDevices.team_member_id.validator),
    ('web_sessions', MemberDevices.web_sessions.validator),
    ('desktop_clients', MemberDevices.desktop_clients.validator),
    ('mobile_clients', MemberDevices.mobile_clients.validator),
MemberLinkedApps.team_member_id.validator = bv.String()
MemberLinkedApps.linked_api_apps.validator = bv.List(ApiApp_validator)
MemberLinkedApps._all_field_names_ = set([
    'linked_api_apps',
MemberLinkedApps._all_fields_ = [
    ('team_member_id', MemberLinkedApps.team_member_id.validator),
    ('linked_api_apps', MemberLinkedApps.linked_api_apps.validator),
MemberProfile.team_member_id.validator = team_common.TeamMemberId_validator
MemberProfile.external_id.validator = bv.Nullable(bv.String())
MemberProfile.account_id.validator = bv.Nullable(users_common.AccountId_validator)
MemberProfile.email.validator = bv.String()
MemberProfile.email_verified.validator = bv.Boolean()
MemberProfile.secondary_emails.validator = bv.Nullable(bv.List(secondary_emails.SecondaryEmail_validator))
MemberProfile.status.validator = TeamMemberStatus_validator
MemberProfile.name.validator = users.Name_validator
MemberProfile.membership_type.validator = TeamMembershipType_validator
MemberProfile.invited_on.validator = bv.Nullable(common.DropboxTimestamp_validator)
MemberProfile.joined_on.validator = bv.Nullable(common.DropboxTimestamp_validator)
MemberProfile.suspended_on.validator = bv.Nullable(common.DropboxTimestamp_validator)
MemberProfile.persistent_id.validator = bv.Nullable(bv.String())
MemberProfile.is_directory_restricted.validator = bv.Nullable(bv.Boolean())
MemberProfile.profile_photo_url.validator = bv.Nullable(bv.String())
MemberProfile._all_field_names_ = set([
    'external_id',
    'secondary_emails',
    'membership_type',
    'invited_on',
    'joined_on',
    'suspended_on',
    'persistent_id',
    'profile_photo_url',
MemberProfile._all_fields_ = [
    ('team_member_id', MemberProfile.team_member_id.validator),
    ('external_id', MemberProfile.external_id.validator),
    ('account_id', MemberProfile.account_id.validator),
    ('email', MemberProfile.email.validator),
    ('email_verified', MemberProfile.email_verified.validator),
    ('secondary_emails', MemberProfile.secondary_emails.validator),
    ('status', MemberProfile.status.validator),
    ('name', MemberProfile.name.validator),
    ('membership_type', MemberProfile.membership_type.validator),
    ('invited_on', MemberProfile.invited_on.validator),
    ('joined_on', MemberProfile.joined_on.validator),
    ('suspended_on', MemberProfile.suspended_on.validator),
    ('persistent_id', MemberProfile.persistent_id.validator),
    ('is_directory_restricted', MemberProfile.is_directory_restricted.validator),
    ('profile_photo_url', MemberProfile.profile_photo_url.validator),
UserSelectorError._user_not_found_validator = bv.Void()
UserSelectorError._tagmap = {
    'user_not_found': UserSelectorError._user_not_found_validator,
UserSelectorError.user_not_found = UserSelectorError('user_not_found')
MemberSelectorError._user_not_in_team_validator = bv.Void()
MemberSelectorError._tagmap = {
    'user_not_in_team': MemberSelectorError._user_not_in_team_validator,
MemberSelectorError._tagmap.update(UserSelectorError._tagmap)
MemberSelectorError.user_not_in_team = MemberSelectorError('user_not_in_team')
MembersAddArgBase.force_async.validator = bv.Boolean()
MembersAddArgBase._all_field_names_ = set(['force_async'])
MembersAddArgBase._all_fields_ = [('force_async', MembersAddArgBase.force_async.validator)]
MembersAddArg.new_members.validator = bv.List(MemberAddArg_validator)
MembersAddArg._all_field_names_ = MembersAddArgBase._all_field_names_.union(set(['new_members']))
MembersAddArg._all_fields_ = MembersAddArgBase._all_fields_ + [('new_members', MembersAddArg.new_members.validator)]
MembersAddJobStatus._complete_validator = bv.List(MemberAddResult_validator)
MembersAddJobStatus._failed_validator = bv.String()
MembersAddJobStatus._tagmap = {
    'complete': MembersAddJobStatus._complete_validator,
    'failed': MembersAddJobStatus._failed_validator,
MembersAddJobStatus._tagmap.update(async_.PollResultBase._tagmap)
MembersAddJobStatusV2Result._complete_validator = bv.List(MemberAddV2Result_validator)
MembersAddJobStatusV2Result._failed_validator = bv.String()
MembersAddJobStatusV2Result._other_validator = bv.Void()
MembersAddJobStatusV2Result._tagmap = {
    'complete': MembersAddJobStatusV2Result._complete_validator,
    'failed': MembersAddJobStatusV2Result._failed_validator,
    'other': MembersAddJobStatusV2Result._other_validator,
MembersAddJobStatusV2Result._tagmap.update(async_.PollResultBase._tagmap)
MembersAddJobStatusV2Result.other = MembersAddJobStatusV2Result('other')
MembersAddLaunch._complete_validator = bv.List(MemberAddResult_validator)
MembersAddLaunch._tagmap = {
    'complete': MembersAddLaunch._complete_validator,
MembersAddLaunch._tagmap.update(async_.LaunchResultBase._tagmap)
MembersAddLaunchV2Result._complete_validator = bv.List(MemberAddV2Result_validator)
MembersAddLaunchV2Result._other_validator = bv.Void()
MembersAddLaunchV2Result._tagmap = {
    'complete': MembersAddLaunchV2Result._complete_validator,
    'other': MembersAddLaunchV2Result._other_validator,
MembersAddLaunchV2Result._tagmap.update(async_.LaunchResultBase._tagmap)
MembersAddLaunchV2Result.other = MembersAddLaunchV2Result('other')
MembersAddV2Arg.new_members.validator = bv.List(MemberAddV2Arg_validator)
MembersAddV2Arg._all_field_names_ = MembersAddArgBase._all_field_names_.union(set(['new_members']))
MembersAddV2Arg._all_fields_ = MembersAddArgBase._all_fields_ + [('new_members', MembersAddV2Arg.new_members.validator)]
MembersDeactivateBaseArg.user.validator = UserSelectorArg_validator
MembersDeactivateBaseArg._all_field_names_ = set(['user'])
MembersDeactivateBaseArg._all_fields_ = [('user', MembersDeactivateBaseArg.user.validator)]
MembersDataTransferArg.transfer_dest_id.validator = UserSelectorArg_validator
MembersDataTransferArg.transfer_admin_id.validator = UserSelectorArg_validator
MembersDataTransferArg._all_field_names_ = MembersDeactivateBaseArg._all_field_names_.union(set([
    'transfer_dest_id',
    'transfer_admin_id',
MembersDataTransferArg._all_fields_ = MembersDeactivateBaseArg._all_fields_ + [
    ('transfer_dest_id', MembersDataTransferArg.transfer_dest_id.validator),
    ('transfer_admin_id', MembersDataTransferArg.transfer_admin_id.validator),
MembersDeactivateArg.wipe_data.validator = bv.Boolean()
MembersDeactivateArg._all_field_names_ = MembersDeactivateBaseArg._all_field_names_.union(set(['wipe_data']))
MembersDeactivateArg._all_fields_ = MembersDeactivateBaseArg._all_fields_ + [('wipe_data', MembersDeactivateArg.wipe_data.validator)]
MembersDeactivateError._user_not_in_team_validator = bv.Void()
MembersDeactivateError._other_validator = bv.Void()
MembersDeactivateError._tagmap = {
    'user_not_in_team': MembersDeactivateError._user_not_in_team_validator,
    'other': MembersDeactivateError._other_validator,
MembersDeactivateError._tagmap.update(UserSelectorError._tagmap)
MembersDeactivateError.user_not_in_team = MembersDeactivateError('user_not_in_team')
MembersDeactivateError.other = MembersDeactivateError('other')
MembersDeleteProfilePhotoArg.user.validator = UserSelectorArg_validator
MembersDeleteProfilePhotoArg._all_field_names_ = set(['user'])
MembersDeleteProfilePhotoArg._all_fields_ = [('user', MembersDeleteProfilePhotoArg.user.validator)]
MembersDeleteProfilePhotoError._set_profile_disallowed_validator = bv.Void()
MembersDeleteProfilePhotoError._other_validator = bv.Void()
MembersDeleteProfilePhotoError._tagmap = {
    'set_profile_disallowed': MembersDeleteProfilePhotoError._set_profile_disallowed_validator,
    'other': MembersDeleteProfilePhotoError._other_validator,
MembersDeleteProfilePhotoError._tagmap.update(MemberSelectorError._tagmap)
MembersDeleteProfilePhotoError.set_profile_disallowed = MembersDeleteProfilePhotoError('set_profile_disallowed')
MembersDeleteProfilePhotoError.other = MembersDeleteProfilePhotoError('other')
MembersGetAvailableTeamMemberRolesResult.roles.validator = bv.List(TeamMemberRole_validator)
MembersGetAvailableTeamMemberRolesResult._all_field_names_ = set(['roles'])
MembersGetAvailableTeamMemberRolesResult._all_fields_ = [('roles', MembersGetAvailableTeamMemberRolesResult.roles.validator)]
MembersGetInfoArgs.members.validator = bv.List(UserSelectorArg_validator)
MembersGetInfoArgs._all_field_names_ = set(['members'])
MembersGetInfoArgs._all_fields_ = [('members', MembersGetInfoArgs.members.validator)]
MembersGetInfoError._other_validator = bv.Void()
MembersGetInfoError._tagmap = {
    'other': MembersGetInfoError._other_validator,
MembersGetInfoError.other = MembersGetInfoError('other')
MembersGetInfoItemBase._id_not_found_validator = bv.String()
MembersGetInfoItemBase._tagmap = {
    'id_not_found': MembersGetInfoItemBase._id_not_found_validator,
MembersGetInfoItem._member_info_validator = TeamMemberInfo_validator
MembersGetInfoItem._tagmap = {
    'member_info': MembersGetInfoItem._member_info_validator,
MembersGetInfoItem._tagmap.update(MembersGetInfoItemBase._tagmap)
MembersGetInfoItemV2._member_info_validator = TeamMemberInfoV2_validator
MembersGetInfoItemV2._other_validator = bv.Void()
MembersGetInfoItemV2._tagmap = {
    'member_info': MembersGetInfoItemV2._member_info_validator,
    'other': MembersGetInfoItemV2._other_validator,
MembersGetInfoItemV2._tagmap.update(MembersGetInfoItemBase._tagmap)
MembersGetInfoItemV2.other = MembersGetInfoItemV2('other')
MembersGetInfoV2Arg.members.validator = bv.List(UserSelectorArg_validator)
MembersGetInfoV2Arg._all_field_names_ = set(['members'])
MembersGetInfoV2Arg._all_fields_ = [('members', MembersGetInfoV2Arg.members.validator)]
MembersGetInfoV2Result.members_info.validator = bv.List(MembersGetInfoItemV2_validator)
MembersGetInfoV2Result._all_field_names_ = set(['members_info'])
MembersGetInfoV2Result._all_fields_ = [('members_info', MembersGetInfoV2Result.members_info.validator)]
MembersInfo.team_member_ids.validator = bv.List(team_common.TeamMemberId_validator)
MembersInfo.permanently_deleted_users.validator = bv.UInt64()
MembersInfo._all_field_names_ = set([
    'team_member_ids',
    'permanently_deleted_users',
MembersInfo._all_fields_ = [
    ('team_member_ids', MembersInfo.team_member_ids.validator),
    ('permanently_deleted_users', MembersInfo.permanently_deleted_users.validator),
MembersListArg.limit.validator = bv.UInt32(min_value=1, max_value=1000)
MembersListArg.include_removed.validator = bv.Boolean()
MembersListArg._all_field_names_ = set([
    'include_removed',
MembersListArg._all_fields_ = [
    ('limit', MembersListArg.limit.validator),
    ('include_removed', MembersListArg.include_removed.validator),
MembersListContinueArg.cursor.validator = bv.String()
MembersListContinueArg._all_field_names_ = set(['cursor'])
MembersListContinueArg._all_fields_ = [('cursor', MembersListContinueArg.cursor.validator)]
MembersListContinueError._invalid_cursor_validator = bv.Void()
MembersListContinueError._other_validator = bv.Void()
MembersListContinueError._tagmap = {
    'invalid_cursor': MembersListContinueError._invalid_cursor_validator,
    'other': MembersListContinueError._other_validator,
MembersListContinueError.invalid_cursor = MembersListContinueError('invalid_cursor')
MembersListContinueError.other = MembersListContinueError('other')
MembersListError._other_validator = bv.Void()
MembersListError._tagmap = {
    'other': MembersListError._other_validator,
MembersListError.other = MembersListError('other')
MembersListResult.members.validator = bv.List(TeamMemberInfo_validator)
MembersListResult.cursor.validator = bv.String()
MembersListResult.has_more.validator = bv.Boolean()
MembersListResult._all_field_names_ = set([
MembersListResult._all_fields_ = [
    ('members', MembersListResult.members.validator),
    ('cursor', MembersListResult.cursor.validator),
    ('has_more', MembersListResult.has_more.validator),
MembersListV2Result.members.validator = bv.List(TeamMemberInfoV2_validator)
MembersListV2Result.cursor.validator = bv.String()
MembersListV2Result.has_more.validator = bv.Boolean()
MembersListV2Result._all_field_names_ = set([
MembersListV2Result._all_fields_ = [
    ('members', MembersListV2Result.members.validator),
    ('cursor', MembersListV2Result.cursor.validator),
    ('has_more', MembersListV2Result.has_more.validator),
MembersRecoverArg.user.validator = UserSelectorArg_validator
MembersRecoverArg._all_field_names_ = set(['user'])
MembersRecoverArg._all_fields_ = [('user', MembersRecoverArg.user.validator)]
MembersRecoverError._user_unrecoverable_validator = bv.Void()
MembersRecoverError._user_not_in_team_validator = bv.Void()
MembersRecoverError._team_license_limit_validator = bv.Void()
MembersRecoverError._other_validator = bv.Void()
MembersRecoverError._tagmap = {
    'user_unrecoverable': MembersRecoverError._user_unrecoverable_validator,
    'user_not_in_team': MembersRecoverError._user_not_in_team_validator,
    'team_license_limit': MembersRecoverError._team_license_limit_validator,
    'other': MembersRecoverError._other_validator,
MembersRecoverError._tagmap.update(UserSelectorError._tagmap)
MembersRecoverError.user_unrecoverable = MembersRecoverError('user_unrecoverable')
MembersRecoverError.user_not_in_team = MembersRecoverError('user_not_in_team')
MembersRecoverError.team_license_limit = MembersRecoverError('team_license_limit')
MembersRecoverError.other = MembersRecoverError('other')
MembersRemoveArg.transfer_dest_id.validator = bv.Nullable(UserSelectorArg_validator)
MembersRemoveArg.transfer_admin_id.validator = bv.Nullable(UserSelectorArg_validator)
MembersRemoveArg.keep_account.validator = bv.Boolean()
MembersRemoveArg.retain_team_shares.validator = bv.Boolean()
MembersRemoveArg._all_field_names_ = MembersDeactivateArg._all_field_names_.union(set([
    'keep_account',
    'retain_team_shares',
MembersRemoveArg._all_fields_ = MembersDeactivateArg._all_fields_ + [
    ('transfer_dest_id', MembersRemoveArg.transfer_dest_id.validator),
    ('transfer_admin_id', MembersRemoveArg.transfer_admin_id.validator),
    ('keep_account', MembersRemoveArg.keep_account.validator),
    ('retain_team_shares', MembersRemoveArg.retain_team_shares.validator),
MembersTransferFilesError._removed_and_transfer_dest_should_differ_validator = bv.Void()
MembersTransferFilesError._removed_and_transfer_admin_should_differ_validator = bv.Void()
MembersTransferFilesError._transfer_dest_user_not_found_validator = bv.Void()
MembersTransferFilesError._transfer_dest_user_not_in_team_validator = bv.Void()
MembersTransferFilesError._transfer_admin_user_not_in_team_validator = bv.Void()
MembersTransferFilesError._transfer_admin_user_not_found_validator = bv.Void()
MembersTransferFilesError._unspecified_transfer_admin_id_validator = bv.Void()
MembersTransferFilesError._transfer_admin_is_not_admin_validator = bv.Void()
MembersTransferFilesError._recipient_not_verified_validator = bv.Void()
MembersTransferFilesError._tagmap = {
    'removed_and_transfer_dest_should_differ': MembersTransferFilesError._removed_and_transfer_dest_should_differ_validator,
    'removed_and_transfer_admin_should_differ': MembersTransferFilesError._removed_and_transfer_admin_should_differ_validator,
    'transfer_dest_user_not_found': MembersTransferFilesError._transfer_dest_user_not_found_validator,
    'transfer_dest_user_not_in_team': MembersTransferFilesError._transfer_dest_user_not_in_team_validator,
    'transfer_admin_user_not_in_team': MembersTransferFilesError._transfer_admin_user_not_in_team_validator,
    'transfer_admin_user_not_found': MembersTransferFilesError._transfer_admin_user_not_found_validator,
    'unspecified_transfer_admin_id': MembersTransferFilesError._unspecified_transfer_admin_id_validator,
    'transfer_admin_is_not_admin': MembersTransferFilesError._transfer_admin_is_not_admin_validator,
    'recipient_not_verified': MembersTransferFilesError._recipient_not_verified_validator,
MembersTransferFilesError._tagmap.update(MembersDeactivateError._tagmap)
MembersTransferFilesError.removed_and_transfer_dest_should_differ = MembersTransferFilesError('removed_and_transfer_dest_should_differ')
MembersTransferFilesError.removed_and_transfer_admin_should_differ = MembersTransferFilesError('removed_and_transfer_admin_should_differ')
MembersTransferFilesError.transfer_dest_user_not_found = MembersTransferFilesError('transfer_dest_user_not_found')
MembersTransferFilesError.transfer_dest_user_not_in_team = MembersTransferFilesError('transfer_dest_user_not_in_team')
MembersTransferFilesError.transfer_admin_user_not_in_team = MembersTransferFilesError('transfer_admin_user_not_in_team')
MembersTransferFilesError.transfer_admin_user_not_found = MembersTransferFilesError('transfer_admin_user_not_found')
MembersTransferFilesError.unspecified_transfer_admin_id = MembersTransferFilesError('unspecified_transfer_admin_id')
MembersTransferFilesError.transfer_admin_is_not_admin = MembersTransferFilesError('transfer_admin_is_not_admin')
MembersTransferFilesError.recipient_not_verified = MembersTransferFilesError('recipient_not_verified')
MembersRemoveError._remove_last_admin_validator = bv.Void()
MembersRemoveError._cannot_keep_account_and_transfer_validator = bv.Void()
MembersRemoveError._cannot_keep_account_and_delete_data_validator = bv.Void()
MembersRemoveError._email_address_too_long_to_be_disabled_validator = bv.Void()
MembersRemoveError._cannot_keep_invited_user_account_validator = bv.Void()
MembersRemoveError._cannot_retain_shares_when_data_wiped_validator = bv.Void()
MembersRemoveError._cannot_retain_shares_when_no_account_kept_validator = bv.Void()
MembersRemoveError._cannot_retain_shares_when_team_external_sharing_off_validator = bv.Void()
MembersRemoveError._cannot_keep_account_validator = bv.Void()
MembersRemoveError._cannot_keep_account_under_legal_hold_validator = bv.Void()
MembersRemoveError._cannot_keep_account_required_to_sign_tos_validator = bv.Void()
MembersRemoveError._tagmap = {
    'remove_last_admin': MembersRemoveError._remove_last_admin_validator,
    'cannot_keep_account_and_transfer': MembersRemoveError._cannot_keep_account_and_transfer_validator,
    'cannot_keep_account_and_delete_data': MembersRemoveError._cannot_keep_account_and_delete_data_validator,
    'email_address_too_long_to_be_disabled': MembersRemoveError._email_address_too_long_to_be_disabled_validator,
    'cannot_keep_invited_user_account': MembersRemoveError._cannot_keep_invited_user_account_validator,
    'cannot_retain_shares_when_data_wiped': MembersRemoveError._cannot_retain_shares_when_data_wiped_validator,
    'cannot_retain_shares_when_no_account_kept': MembersRemoveError._cannot_retain_shares_when_no_account_kept_validator,
    'cannot_retain_shares_when_team_external_sharing_off': MembersRemoveError._cannot_retain_shares_when_team_external_sharing_off_validator,
    'cannot_keep_account': MembersRemoveError._cannot_keep_account_validator,
    'cannot_keep_account_under_legal_hold': MembersRemoveError._cannot_keep_account_under_legal_hold_validator,
    'cannot_keep_account_required_to_sign_tos': MembersRemoveError._cannot_keep_account_required_to_sign_tos_validator,
MembersRemoveError._tagmap.update(MembersTransferFilesError._tagmap)
MembersRemoveError.remove_last_admin = MembersRemoveError('remove_last_admin')
MembersRemoveError.cannot_keep_account_and_transfer = MembersRemoveError('cannot_keep_account_and_transfer')
MembersRemoveError.cannot_keep_account_and_delete_data = MembersRemoveError('cannot_keep_account_and_delete_data')
MembersRemoveError.email_address_too_long_to_be_disabled = MembersRemoveError('email_address_too_long_to_be_disabled')
MembersRemoveError.cannot_keep_invited_user_account = MembersRemoveError('cannot_keep_invited_user_account')
MembersRemoveError.cannot_retain_shares_when_data_wiped = MembersRemoveError('cannot_retain_shares_when_data_wiped')
MembersRemoveError.cannot_retain_shares_when_no_account_kept = MembersRemoveError('cannot_retain_shares_when_no_account_kept')
MembersRemoveError.cannot_retain_shares_when_team_external_sharing_off = MembersRemoveError('cannot_retain_shares_when_team_external_sharing_off')
MembersRemoveError.cannot_keep_account = MembersRemoveError('cannot_keep_account')
MembersRemoveError.cannot_keep_account_under_legal_hold = MembersRemoveError('cannot_keep_account_under_legal_hold')
MembersRemoveError.cannot_keep_account_required_to_sign_tos = MembersRemoveError('cannot_keep_account_required_to_sign_tos')
MembersSendWelcomeError._other_validator = bv.Void()
MembersSendWelcomeError._tagmap = {
    'other': MembersSendWelcomeError._other_validator,
MembersSendWelcomeError._tagmap.update(MemberSelectorError._tagmap)
MembersSendWelcomeError.other = MembersSendWelcomeError('other')
MembersSetPermissions2Arg.user.validator = UserSelectorArg_validator
MembersSetPermissions2Arg.new_roles.validator = bv.Nullable(bv.List(TeamMemberRoleId_validator, max_items=1))
MembersSetPermissions2Arg._all_field_names_ = set([
    'new_roles',
MembersSetPermissions2Arg._all_fields_ = [
    ('user', MembersSetPermissions2Arg.user.validator),
    ('new_roles', MembersSetPermissions2Arg.new_roles.validator),
MembersSetPermissions2Error._last_admin_validator = bv.Void()
MembersSetPermissions2Error._user_not_in_team_validator = bv.Void()
MembersSetPermissions2Error._cannot_set_permissions_validator = bv.Void()
MembersSetPermissions2Error._role_not_found_validator = bv.Void()
MembersSetPermissions2Error._other_validator = bv.Void()
MembersSetPermissions2Error._tagmap = {
    'last_admin': MembersSetPermissions2Error._last_admin_validator,
    'user_not_in_team': MembersSetPermissions2Error._user_not_in_team_validator,
    'cannot_set_permissions': MembersSetPermissions2Error._cannot_set_permissions_validator,
    'role_not_found': MembersSetPermissions2Error._role_not_found_validator,
    'other': MembersSetPermissions2Error._other_validator,
MembersSetPermissions2Error._tagmap.update(UserSelectorError._tagmap)
MembersSetPermissions2Error.last_admin = MembersSetPermissions2Error('last_admin')
MembersSetPermissions2Error.user_not_in_team = MembersSetPermissions2Error('user_not_in_team')
MembersSetPermissions2Error.cannot_set_permissions = MembersSetPermissions2Error('cannot_set_permissions')
MembersSetPermissions2Error.role_not_found = MembersSetPermissions2Error('role_not_found')
MembersSetPermissions2Error.other = MembersSetPermissions2Error('other')
MembersSetPermissions2Result.team_member_id.validator = team_common.TeamMemberId_validator
MembersSetPermissions2Result.roles.validator = bv.Nullable(bv.List(TeamMemberRole_validator))
MembersSetPermissions2Result._all_field_names_ = set([
    'roles',
MembersSetPermissions2Result._all_fields_ = [
    ('team_member_id', MembersSetPermissions2Result.team_member_id.validator),
    ('roles', MembersSetPermissions2Result.roles.validator),
MembersSetPermissionsArg.user.validator = UserSelectorArg_validator
MembersSetPermissionsArg.new_role.validator = AdminTier_validator
MembersSetPermissionsArg._all_field_names_ = set([
    'new_role',
MembersSetPermissionsArg._all_fields_ = [
    ('user', MembersSetPermissionsArg.user.validator),
    ('new_role', MembersSetPermissionsArg.new_role.validator),
MembersSetPermissionsError._last_admin_validator = bv.Void()
MembersSetPermissionsError._user_not_in_team_validator = bv.Void()
MembersSetPermissionsError._cannot_set_permissions_validator = bv.Void()
MembersSetPermissionsError._team_license_limit_validator = bv.Void()
MembersSetPermissionsError._other_validator = bv.Void()
MembersSetPermissionsError._tagmap = {
    'last_admin': MembersSetPermissionsError._last_admin_validator,
    'user_not_in_team': MembersSetPermissionsError._user_not_in_team_validator,
    'cannot_set_permissions': MembersSetPermissionsError._cannot_set_permissions_validator,
    'team_license_limit': MembersSetPermissionsError._team_license_limit_validator,
    'other': MembersSetPermissionsError._other_validator,
MembersSetPermissionsError._tagmap.update(UserSelectorError._tagmap)
MembersSetPermissionsError.last_admin = MembersSetPermissionsError('last_admin')
MembersSetPermissionsError.user_not_in_team = MembersSetPermissionsError('user_not_in_team')
MembersSetPermissionsError.cannot_set_permissions = MembersSetPermissionsError('cannot_set_permissions')
MembersSetPermissionsError.team_license_limit = MembersSetPermissionsError('team_license_limit')
MembersSetPermissionsError.other = MembersSetPermissionsError('other')
MembersSetPermissionsResult.team_member_id.validator = team_common.TeamMemberId_validator
MembersSetPermissionsResult.role.validator = AdminTier_validator
MembersSetPermissionsResult._all_field_names_ = set([
    'role',
MembersSetPermissionsResult._all_fields_ = [
    ('team_member_id', MembersSetPermissionsResult.team_member_id.validator),
    ('role', MembersSetPermissionsResult.role.validator),
MembersSetProfileArg.user.validator = UserSelectorArg_validator
MembersSetProfileArg.new_email.validator = bv.Nullable(common.EmailAddress_validator)
MembersSetProfileArg.new_external_id.validator = bv.Nullable(team_common.MemberExternalId_validator)
MembersSetProfileArg.new_given_name.validator = bv.Nullable(common.OptionalNamePart_validator)
MembersSetProfileArg.new_surname.validator = bv.Nullable(common.OptionalNamePart_validator)
MembersSetProfileArg.new_persistent_id.validator = bv.Nullable(bv.String())
MembersSetProfileArg.new_is_directory_restricted.validator = bv.Nullable(bv.Boolean())
MembersSetProfileArg._all_field_names_ = set([
    'new_email',
    'new_external_id',
    'new_given_name',
    'new_surname',
    'new_persistent_id',
    'new_is_directory_restricted',
MembersSetProfileArg._all_fields_ = [
    ('user', MembersSetProfileArg.user.validator),
    ('new_email', MembersSetProfileArg.new_email.validator),
    ('new_external_id', MembersSetProfileArg.new_external_id.validator),
    ('new_given_name', MembersSetProfileArg.new_given_name.validator),
    ('new_surname', MembersSetProfileArg.new_surname.validator),
    ('new_persistent_id', MembersSetProfileArg.new_persistent_id.validator),
    ('new_is_directory_restricted', MembersSetProfileArg.new_is_directory_restricted.validator),
MembersSetProfileError._external_id_and_new_external_id_unsafe_validator = bv.Void()
MembersSetProfileError._no_new_data_specified_validator = bv.Void()
MembersSetProfileError._email_reserved_for_other_user_validator = bv.Void()
MembersSetProfileError._external_id_used_by_other_user_validator = bv.Void()
MembersSetProfileError._set_profile_disallowed_validator = bv.Void()
MembersSetProfileError._param_cannot_be_empty_validator = bv.Void()
MembersSetProfileError._persistent_id_disabled_validator = bv.Void()
MembersSetProfileError._persistent_id_used_by_other_user_validator = bv.Void()
MembersSetProfileError._directory_restricted_off_validator = bv.Void()
MembersSetProfileError._other_validator = bv.Void()
MembersSetProfileError._tagmap = {
    'external_id_and_new_external_id_unsafe': MembersSetProfileError._external_id_and_new_external_id_unsafe_validator,
    'no_new_data_specified': MembersSetProfileError._no_new_data_specified_validator,
    'email_reserved_for_other_user': MembersSetProfileError._email_reserved_for_other_user_validator,
    'external_id_used_by_other_user': MembersSetProfileError._external_id_used_by_other_user_validator,
    'set_profile_disallowed': MembersSetProfileError._set_profile_disallowed_validator,
    'param_cannot_be_empty': MembersSetProfileError._param_cannot_be_empty_validator,
    'persistent_id_disabled': MembersSetProfileError._persistent_id_disabled_validator,
    'persistent_id_used_by_other_user': MembersSetProfileError._persistent_id_used_by_other_user_validator,
    'directory_restricted_off': MembersSetProfileError._directory_restricted_off_validator,
    'other': MembersSetProfileError._other_validator,
MembersSetProfileError._tagmap.update(MemberSelectorError._tagmap)
MembersSetProfileError.external_id_and_new_external_id_unsafe = MembersSetProfileError('external_id_and_new_external_id_unsafe')
MembersSetProfileError.no_new_data_specified = MembersSetProfileError('no_new_data_specified')
MembersSetProfileError.email_reserved_for_other_user = MembersSetProfileError('email_reserved_for_other_user')
MembersSetProfileError.external_id_used_by_other_user = MembersSetProfileError('external_id_used_by_other_user')
MembersSetProfileError.set_profile_disallowed = MembersSetProfileError('set_profile_disallowed')
MembersSetProfileError.param_cannot_be_empty = MembersSetProfileError('param_cannot_be_empty')
MembersSetProfileError.persistent_id_disabled = MembersSetProfileError('persistent_id_disabled')
MembersSetProfileError.persistent_id_used_by_other_user = MembersSetProfileError('persistent_id_used_by_other_user')
MembersSetProfileError.directory_restricted_off = MembersSetProfileError('directory_restricted_off')
MembersSetProfileError.other = MembersSetProfileError('other')
MembersSetProfilePhotoArg.user.validator = UserSelectorArg_validator
MembersSetProfilePhotoArg.photo.validator = account.PhotoSourceArg_validator
MembersSetProfilePhotoArg._all_field_names_ = set([
    'photo',
MembersSetProfilePhotoArg._all_fields_ = [
    ('user', MembersSetProfilePhotoArg.user.validator),
    ('photo', MembersSetProfilePhotoArg.photo.validator),
MembersSetProfilePhotoError._set_profile_disallowed_validator = bv.Void()
MembersSetProfilePhotoError._photo_error_validator = account.SetProfilePhotoError_validator
MembersSetProfilePhotoError._other_validator = bv.Void()
MembersSetProfilePhotoError._tagmap = {
    'set_profile_disallowed': MembersSetProfilePhotoError._set_profile_disallowed_validator,
    'photo_error': MembersSetProfilePhotoError._photo_error_validator,
    'other': MembersSetProfilePhotoError._other_validator,
MembersSetProfilePhotoError._tagmap.update(MemberSelectorError._tagmap)
MembersSetProfilePhotoError.set_profile_disallowed = MembersSetProfilePhotoError('set_profile_disallowed')
MembersSetProfilePhotoError.other = MembersSetProfilePhotoError('other')
MembersSuspendError._suspend_inactive_user_validator = bv.Void()
MembersSuspendError._suspend_last_admin_validator = bv.Void()
MembersSuspendError._team_license_limit_validator = bv.Void()
MembersSuspendError._tagmap = {
    'suspend_inactive_user': MembersSuspendError._suspend_inactive_user_validator,
    'suspend_last_admin': MembersSuspendError._suspend_last_admin_validator,
    'team_license_limit': MembersSuspendError._team_license_limit_validator,
MembersSuspendError._tagmap.update(MembersDeactivateError._tagmap)
MembersSuspendError.suspend_inactive_user = MembersSuspendError('suspend_inactive_user')
MembersSuspendError.suspend_last_admin = MembersSuspendError('suspend_last_admin')
MembersSuspendError.team_license_limit = MembersSuspendError('team_license_limit')
MembersTransferFormerMembersFilesError._user_data_is_being_transferred_validator = bv.Void()
MembersTransferFormerMembersFilesError._user_not_removed_validator = bv.Void()
MembersTransferFormerMembersFilesError._user_data_cannot_be_transferred_validator = bv.Void()
MembersTransferFormerMembersFilesError._user_data_already_transferred_validator = bv.Void()
MembersTransferFormerMembersFilesError._tagmap = {
    'user_data_is_being_transferred': MembersTransferFormerMembersFilesError._user_data_is_being_transferred_validator,
    'user_not_removed': MembersTransferFormerMembersFilesError._user_not_removed_validator,
    'user_data_cannot_be_transferred': MembersTransferFormerMembersFilesError._user_data_cannot_be_transferred_validator,
    'user_data_already_transferred': MembersTransferFormerMembersFilesError._user_data_already_transferred_validator,
MembersTransferFormerMembersFilesError._tagmap.update(MembersTransferFilesError._tagmap)
MembersTransferFormerMembersFilesError.user_data_is_being_transferred = MembersTransferFormerMembersFilesError('user_data_is_being_transferred')
MembersTransferFormerMembersFilesError.user_not_removed = MembersTransferFormerMembersFilesError('user_not_removed')
MembersTransferFormerMembersFilesError.user_data_cannot_be_transferred = MembersTransferFormerMembersFilesError('user_data_cannot_be_transferred')
MembersTransferFormerMembersFilesError.user_data_already_transferred = MembersTransferFormerMembersFilesError('user_data_already_transferred')
MembersUnsuspendArg.user.validator = UserSelectorArg_validator
MembersUnsuspendArg._all_field_names_ = set(['user'])
MembersUnsuspendArg._all_fields_ = [('user', MembersUnsuspendArg.user.validator)]
MembersUnsuspendError._unsuspend_non_suspended_member_validator = bv.Void()
MembersUnsuspendError._team_license_limit_validator = bv.Void()
MembersUnsuspendError._tagmap = {
    'unsuspend_non_suspended_member': MembersUnsuspendError._unsuspend_non_suspended_member_validator,
    'team_license_limit': MembersUnsuspendError._team_license_limit_validator,
MembersUnsuspendError._tagmap.update(MembersDeactivateError._tagmap)
MembersUnsuspendError.unsuspend_non_suspended_member = MembersUnsuspendError('unsuspend_non_suspended_member')
MembersUnsuspendError.team_license_limit = MembersUnsuspendError('team_license_limit')
MobileClientPlatform._iphone_validator = bv.Void()
MobileClientPlatform._ipad_validator = bv.Void()
MobileClientPlatform._android_validator = bv.Void()
MobileClientPlatform._windows_phone_validator = bv.Void()
MobileClientPlatform._blackberry_validator = bv.Void()
MobileClientPlatform._other_validator = bv.Void()
MobileClientPlatform._tagmap = {
    'iphone': MobileClientPlatform._iphone_validator,
    'ipad': MobileClientPlatform._ipad_validator,
    'android': MobileClientPlatform._android_validator,
    'windows_phone': MobileClientPlatform._windows_phone_validator,
    'blackberry': MobileClientPlatform._blackberry_validator,
    'other': MobileClientPlatform._other_validator,
MobileClientPlatform.iphone = MobileClientPlatform('iphone')
MobileClientPlatform.ipad = MobileClientPlatform('ipad')
MobileClientPlatform.android = MobileClientPlatform('android')
MobileClientPlatform.windows_phone = MobileClientPlatform('windows_phone')
MobileClientPlatform.blackberry = MobileClientPlatform('blackberry')
MobileClientPlatform.other = MobileClientPlatform('other')
MobileClientSession.device_name.validator = bv.String()
MobileClientSession.client_type.validator = MobileClientPlatform_validator
MobileClientSession.client_version.validator = bv.Nullable(bv.String())
MobileClientSession.os_version.validator = bv.Nullable(bv.String())
MobileClientSession.last_carrier.validator = bv.Nullable(bv.String())
MobileClientSession._all_field_names_ = DeviceSession._all_field_names_.union(set([
    'device_name',
    'os_version',
    'last_carrier',
MobileClientSession._all_fields_ = DeviceSession._all_fields_ + [
    ('device_name', MobileClientSession.device_name.validator),
    ('client_type', MobileClientSession.client_type.validator),
    ('client_version', MobileClientSession.client_version.validator),
    ('os_version', MobileClientSession.os_version.validator),
    ('last_carrier', MobileClientSession.last_carrier.validator),
NamespaceMetadata.name.validator = bv.String()
NamespaceMetadata.namespace_id.validator = common.SharedFolderId_validator
NamespaceMetadata.namespace_type.validator = NamespaceType_validator
NamespaceMetadata.team_member_id.validator = bv.Nullable(team_common.TeamMemberId_validator)
NamespaceMetadata._all_field_names_ = set([
    'namespace_id',
    'namespace_type',
NamespaceMetadata._all_fields_ = [
    ('name', NamespaceMetadata.name.validator),
    ('namespace_id', NamespaceMetadata.namespace_id.validator),
    ('namespace_type', NamespaceMetadata.namespace_type.validator),
    ('team_member_id', NamespaceMetadata.team_member_id.validator),
NamespaceType._app_folder_validator = bv.Void()
NamespaceType._shared_folder_validator = bv.Void()
NamespaceType._team_folder_validator = bv.Void()
NamespaceType._team_member_folder_validator = bv.Void()
NamespaceType._other_validator = bv.Void()
NamespaceType._tagmap = {
    'app_folder': NamespaceType._app_folder_validator,
    'shared_folder': NamespaceType._shared_folder_validator,
    'team_folder': NamespaceType._team_folder_validator,
    'team_member_folder': NamespaceType._team_member_folder_validator,
    'other': NamespaceType._other_validator,
NamespaceType.app_folder = NamespaceType('app_folder')
NamespaceType.shared_folder = NamespaceType('shared_folder')
NamespaceType.team_folder = NamespaceType('team_folder')
NamespaceType.team_member_folder = NamespaceType('team_member_folder')
NamespaceType.other = NamespaceType('other')
RemoveCustomQuotaResult._success_validator = UserSelectorArg_validator
RemoveCustomQuotaResult._invalid_user_validator = UserSelectorArg_validator
RemoveCustomQuotaResult._other_validator = bv.Void()
RemoveCustomQuotaResult._tagmap = {
    'success': RemoveCustomQuotaResult._success_validator,
    'invalid_user': RemoveCustomQuotaResult._invalid_user_validator,
    'other': RemoveCustomQuotaResult._other_validator,
RemoveCustomQuotaResult.other = RemoveCustomQuotaResult('other')
RemovedStatus.is_recoverable.validator = bv.Boolean()
RemovedStatus.is_disconnected.validator = bv.Boolean()
RemovedStatus._all_field_names_ = set([
    'is_recoverable',
    'is_disconnected',
RemovedStatus._all_fields_ = [
    ('is_recoverable', RemovedStatus.is_recoverable.validator),
    ('is_disconnected', RemovedStatus.is_disconnected.validator),
ResendSecondaryEmailResult._success_validator = common.EmailAddress_validator
ResendSecondaryEmailResult._not_pending_validator = common.EmailAddress_validator
ResendSecondaryEmailResult._rate_limited_validator = common.EmailAddress_validator
ResendSecondaryEmailResult._other_validator = bv.Void()
ResendSecondaryEmailResult._tagmap = {
    'success': ResendSecondaryEmailResult._success_validator,
    'not_pending': ResendSecondaryEmailResult._not_pending_validator,
    'rate_limited': ResendSecondaryEmailResult._rate_limited_validator,
    'other': ResendSecondaryEmailResult._other_validator,
ResendSecondaryEmailResult.other = ResendSecondaryEmailResult('other')
ResendVerificationEmailArg.emails_to_resend.validator = bv.List(UserSecondaryEmailsArg_validator)
ResendVerificationEmailArg._all_field_names_ = set(['emails_to_resend'])
ResendVerificationEmailArg._all_fields_ = [('emails_to_resend', ResendVerificationEmailArg.emails_to_resend.validator)]
ResendVerificationEmailResult.results.validator = bv.List(UserResendResult_validator)
ResendVerificationEmailResult._all_field_names_ = set(['results'])
ResendVerificationEmailResult._all_fields_ = [('results', ResendVerificationEmailResult.results.validator)]
RevokeDesktopClientArg.delete_on_unlink.validator = bv.Boolean()
RevokeDesktopClientArg._all_field_names_ = DeviceSessionArg._all_field_names_.union(set(['delete_on_unlink']))
RevokeDesktopClientArg._all_fields_ = DeviceSessionArg._all_fields_ + [('delete_on_unlink', RevokeDesktopClientArg.delete_on_unlink.validator)]
RevokeDeviceSessionArg._web_session_validator = DeviceSessionArg_validator
RevokeDeviceSessionArg._desktop_client_validator = RevokeDesktopClientArg_validator
RevokeDeviceSessionArg._mobile_client_validator = DeviceSessionArg_validator
RevokeDeviceSessionArg._tagmap = {
    'web_session': RevokeDeviceSessionArg._web_session_validator,
    'desktop_client': RevokeDeviceSessionArg._desktop_client_validator,
    'mobile_client': RevokeDeviceSessionArg._mobile_client_validator,
RevokeDeviceSessionBatchArg.revoke_devices.validator = bv.List(RevokeDeviceSessionArg_validator)
RevokeDeviceSessionBatchArg._all_field_names_ = set(['revoke_devices'])
RevokeDeviceSessionBatchArg._all_fields_ = [('revoke_devices', RevokeDeviceSessionBatchArg.revoke_devices.validator)]
RevokeDeviceSessionBatchError._other_validator = bv.Void()
RevokeDeviceSessionBatchError._tagmap = {
    'other': RevokeDeviceSessionBatchError._other_validator,
RevokeDeviceSessionBatchError.other = RevokeDeviceSessionBatchError('other')
RevokeDeviceSessionBatchResult.revoke_devices_status.validator = bv.List(RevokeDeviceSessionStatus_validator)
RevokeDeviceSessionBatchResult._all_field_names_ = set(['revoke_devices_status'])
RevokeDeviceSessionBatchResult._all_fields_ = [('revoke_devices_status', RevokeDeviceSessionBatchResult.revoke_devices_status.validator)]
RevokeDeviceSessionError._device_session_not_found_validator = bv.Void()
RevokeDeviceSessionError._member_not_found_validator = bv.Void()
RevokeDeviceSessionError._other_validator = bv.Void()
RevokeDeviceSessionError._tagmap = {
    'device_session_not_found': RevokeDeviceSessionError._device_session_not_found_validator,
    'member_not_found': RevokeDeviceSessionError._member_not_found_validator,
    'other': RevokeDeviceSessionError._other_validator,
RevokeDeviceSessionError.device_session_not_found = RevokeDeviceSessionError('device_session_not_found')
RevokeDeviceSessionError.member_not_found = RevokeDeviceSessionError('member_not_found')
RevokeDeviceSessionError.other = RevokeDeviceSessionError('other')
RevokeDeviceSessionStatus.success.validator = bv.Boolean()
RevokeDeviceSessionStatus.error_type.validator = bv.Nullable(RevokeDeviceSessionError_validator)
RevokeDeviceSessionStatus._all_field_names_ = set([
    'error_type',
RevokeDeviceSessionStatus._all_fields_ = [
    ('success', RevokeDeviceSessionStatus.success.validator),
    ('error_type', RevokeDeviceSessionStatus.error_type.validator),
RevokeLinkedApiAppArg.app_id.validator = bv.String()
RevokeLinkedApiAppArg.team_member_id.validator = bv.String()
RevokeLinkedApiAppArg.keep_app_folder.validator = bv.Boolean()
RevokeLinkedApiAppArg._all_field_names_ = set([
    'keep_app_folder',
RevokeLinkedApiAppArg._all_fields_ = [
    ('app_id', RevokeLinkedApiAppArg.app_id.validator),
    ('team_member_id', RevokeLinkedApiAppArg.team_member_id.validator),
    ('keep_app_folder', RevokeLinkedApiAppArg.keep_app_folder.validator),
RevokeLinkedApiAppBatchArg.revoke_linked_app.validator = bv.List(RevokeLinkedApiAppArg_validator)
RevokeLinkedApiAppBatchArg._all_field_names_ = set(['revoke_linked_app'])
RevokeLinkedApiAppBatchArg._all_fields_ = [('revoke_linked_app', RevokeLinkedApiAppBatchArg.revoke_linked_app.validator)]
RevokeLinkedAppBatchError._other_validator = bv.Void()
RevokeLinkedAppBatchError._tagmap = {
    'other': RevokeLinkedAppBatchError._other_validator,
RevokeLinkedAppBatchError.other = RevokeLinkedAppBatchError('other')
RevokeLinkedAppBatchResult.revoke_linked_app_status.validator = bv.List(RevokeLinkedAppStatus_validator)
RevokeLinkedAppBatchResult._all_field_names_ = set(['revoke_linked_app_status'])
RevokeLinkedAppBatchResult._all_fields_ = [('revoke_linked_app_status', RevokeLinkedAppBatchResult.revoke_linked_app_status.validator)]
RevokeLinkedAppError._app_not_found_validator = bv.Void()
RevokeLinkedAppError._member_not_found_validator = bv.Void()
RevokeLinkedAppError._app_folder_removal_not_supported_validator = bv.Void()
RevokeLinkedAppError._other_validator = bv.Void()
RevokeLinkedAppError._tagmap = {
    'app_not_found': RevokeLinkedAppError._app_not_found_validator,
    'member_not_found': RevokeLinkedAppError._member_not_found_validator,
    'app_folder_removal_not_supported': RevokeLinkedAppError._app_folder_removal_not_supported_validator,
    'other': RevokeLinkedAppError._other_validator,
RevokeLinkedAppError.app_not_found = RevokeLinkedAppError('app_not_found')
RevokeLinkedAppError.member_not_found = RevokeLinkedAppError('member_not_found')
RevokeLinkedAppError.app_folder_removal_not_supported = RevokeLinkedAppError('app_folder_removal_not_supported')
RevokeLinkedAppError.other = RevokeLinkedAppError('other')
RevokeLinkedAppStatus.success.validator = bv.Boolean()
RevokeLinkedAppStatus.error_type.validator = bv.Nullable(RevokeLinkedAppError_validator)
RevokeLinkedAppStatus._all_field_names_ = set([
RevokeLinkedAppStatus._all_fields_ = [
    ('success', RevokeLinkedAppStatus.success.validator),
    ('error_type', RevokeLinkedAppStatus.error_type.validator),
SetCustomQuotaArg.users_and_quotas.validator = bv.List(UserCustomQuotaArg_validator)
SetCustomQuotaArg._all_field_names_ = set(['users_and_quotas'])
SetCustomQuotaArg._all_fields_ = [('users_and_quotas', SetCustomQuotaArg.users_and_quotas.validator)]
SetCustomQuotaError._some_users_are_excluded_validator = bv.Void()
SetCustomQuotaError._tagmap = {
    'some_users_are_excluded': SetCustomQuotaError._some_users_are_excluded_validator,
SetCustomQuotaError._tagmap.update(CustomQuotaError._tagmap)
SetCustomQuotaError.some_users_are_excluded = SetCustomQuotaError('some_users_are_excluded')
SharingAllowlistAddArgs.domains.validator = bv.Nullable(bv.List(bv.String()))
SharingAllowlistAddArgs.emails.validator = bv.Nullable(bv.List(bv.String()))
SharingAllowlistAddArgs._all_field_names_ = set([
    'domains',
    'emails',
SharingAllowlistAddArgs._all_fields_ = [
    ('domains', SharingAllowlistAddArgs.domains.validator),
    ('emails', SharingAllowlistAddArgs.emails.validator),
SharingAllowlistAddError._malformed_entry_validator = bv.String()
SharingAllowlistAddError._no_entries_provided_validator = bv.Void()
SharingAllowlistAddError._too_many_entries_provided_validator = bv.Void()
SharingAllowlistAddError._team_limit_reached_validator = bv.Void()
SharingAllowlistAddError._unknown_error_validator = bv.Void()
SharingAllowlistAddError._entries_already_exist_validator = bv.String()
SharingAllowlistAddError._other_validator = bv.Void()
SharingAllowlistAddError._tagmap = {
    'malformed_entry': SharingAllowlistAddError._malformed_entry_validator,
    'no_entries_provided': SharingAllowlistAddError._no_entries_provided_validator,
    'too_many_entries_provided': SharingAllowlistAddError._too_many_entries_provided_validator,
    'team_limit_reached': SharingAllowlistAddError._team_limit_reached_validator,
    'unknown_error': SharingAllowlistAddError._unknown_error_validator,
    'entries_already_exist': SharingAllowlistAddError._entries_already_exist_validator,
    'other': SharingAllowlistAddError._other_validator,
SharingAllowlistAddError.no_entries_provided = SharingAllowlistAddError('no_entries_provided')
SharingAllowlistAddError.too_many_entries_provided = SharingAllowlistAddError('too_many_entries_provided')
SharingAllowlistAddError.team_limit_reached = SharingAllowlistAddError('team_limit_reached')
SharingAllowlistAddError.unknown_error = SharingAllowlistAddError('unknown_error')
SharingAllowlistAddError.other = SharingAllowlistAddError('other')
SharingAllowlistAddResponse._all_field_names_ = set([])
SharingAllowlistAddResponse._all_fields_ = []
SharingAllowlistListArg.limit.validator = bv.UInt32(min_value=1, max_value=1000)
SharingAllowlistListArg._all_field_names_ = set(['limit'])
SharingAllowlistListArg._all_fields_ = [('limit', SharingAllowlistListArg.limit.validator)]
SharingAllowlistListContinueArg.cursor.validator = bv.String()
SharingAllowlistListContinueArg._all_field_names_ = set(['cursor'])
SharingAllowlistListContinueArg._all_fields_ = [('cursor', SharingAllowlistListContinueArg.cursor.validator)]
SharingAllowlistListContinueError._invalid_cursor_validator = bv.Void()
SharingAllowlistListContinueError._other_validator = bv.Void()
SharingAllowlistListContinueError._tagmap = {
    'invalid_cursor': SharingAllowlistListContinueError._invalid_cursor_validator,
    'other': SharingAllowlistListContinueError._other_validator,
SharingAllowlistListContinueError.invalid_cursor = SharingAllowlistListContinueError('invalid_cursor')
SharingAllowlistListContinueError.other = SharingAllowlistListContinueError('other')
SharingAllowlistListError._all_field_names_ = set([])
SharingAllowlistListError._all_fields_ = []
SharingAllowlistListResponse.domains.validator = bv.List(bv.String())
SharingAllowlistListResponse.emails.validator = bv.List(bv.String())
SharingAllowlistListResponse.cursor.validator = bv.String()
SharingAllowlistListResponse.has_more.validator = bv.Boolean()
SharingAllowlistListResponse._all_field_names_ = set([
SharingAllowlistListResponse._all_fields_ = [
    ('domains', SharingAllowlistListResponse.domains.validator),
    ('emails', SharingAllowlistListResponse.emails.validator),
    ('cursor', SharingAllowlistListResponse.cursor.validator),
    ('has_more', SharingAllowlistListResponse.has_more.validator),
SharingAllowlistRemoveArgs.domains.validator = bv.Nullable(bv.List(bv.String()))
SharingAllowlistRemoveArgs.emails.validator = bv.Nullable(bv.List(bv.String()))
SharingAllowlistRemoveArgs._all_field_names_ = set([
SharingAllowlistRemoveArgs._all_fields_ = [
    ('domains', SharingAllowlistRemoveArgs.domains.validator),
    ('emails', SharingAllowlistRemoveArgs.emails.validator),
SharingAllowlistRemoveError._malformed_entry_validator = bv.String()
SharingAllowlistRemoveError._entries_do_not_exist_validator = bv.String()
SharingAllowlistRemoveError._no_entries_provided_validator = bv.Void()
SharingAllowlistRemoveError._too_many_entries_provided_validator = bv.Void()
SharingAllowlistRemoveError._unknown_error_validator = bv.Void()
SharingAllowlistRemoveError._other_validator = bv.Void()
SharingAllowlistRemoveError._tagmap = {
    'malformed_entry': SharingAllowlistRemoveError._malformed_entry_validator,
    'entries_do_not_exist': SharingAllowlistRemoveError._entries_do_not_exist_validator,
    'no_entries_provided': SharingAllowlistRemoveError._no_entries_provided_validator,
    'too_many_entries_provided': SharingAllowlistRemoveError._too_many_entries_provided_validator,
    'unknown_error': SharingAllowlistRemoveError._unknown_error_validator,
    'other': SharingAllowlistRemoveError._other_validator,
SharingAllowlistRemoveError.no_entries_provided = SharingAllowlistRemoveError('no_entries_provided')
SharingAllowlistRemoveError.too_many_entries_provided = SharingAllowlistRemoveError('too_many_entries_provided')
SharingAllowlistRemoveError.unknown_error = SharingAllowlistRemoveError('unknown_error')
SharingAllowlistRemoveError.other = SharingAllowlistRemoveError('other')
SharingAllowlistRemoveResponse._all_field_names_ = set([])
SharingAllowlistRemoveResponse._all_fields_ = []
StorageBucket.bucket.validator = bv.String()
StorageBucket.users.validator = bv.UInt64()
StorageBucket._all_field_names_ = set([
    'bucket',
StorageBucket._all_fields_ = [
    ('bucket', StorageBucket.bucket.validator),
    ('users', StorageBucket.users.validator),
TeamFolderAccessError._invalid_team_folder_id_validator = bv.Void()
TeamFolderAccessError._no_access_validator = bv.Void()
TeamFolderAccessError._other_validator = bv.Void()
TeamFolderAccessError._tagmap = {
    'invalid_team_folder_id': TeamFolderAccessError._invalid_team_folder_id_validator,
    'no_access': TeamFolderAccessError._no_access_validator,
    'other': TeamFolderAccessError._other_validator,
TeamFolderAccessError.invalid_team_folder_id = TeamFolderAccessError('invalid_team_folder_id')
TeamFolderAccessError.no_access = TeamFolderAccessError('no_access')
TeamFolderAccessError.other = TeamFolderAccessError('other')
TeamFolderActivateError._tagmap = {
TeamFolderActivateError._tagmap.update(BaseTeamFolderError._tagmap)
TeamFolderIdArg.team_folder_id.validator = common.SharedFolderId_validator
TeamFolderIdArg._all_field_names_ = set(['team_folder_id'])
TeamFolderIdArg._all_fields_ = [('team_folder_id', TeamFolderIdArg.team_folder_id.validator)]
TeamFolderArchiveArg.force_async_off.validator = bv.Boolean()
TeamFolderArchiveArg._all_field_names_ = TeamFolderIdArg._all_field_names_.union(set(['force_async_off']))
TeamFolderArchiveArg._all_fields_ = TeamFolderIdArg._all_fields_ + [('force_async_off', TeamFolderArchiveArg.force_async_off.validator)]
TeamFolderArchiveError._tagmap = {
TeamFolderArchiveError._tagmap.update(BaseTeamFolderError._tagmap)
TeamFolderArchiveJobStatus._complete_validator = TeamFolderMetadata_validator
TeamFolderArchiveJobStatus._failed_validator = TeamFolderArchiveError_validator
TeamFolderArchiveJobStatus._tagmap = {
    'complete': TeamFolderArchiveJobStatus._complete_validator,
    'failed': TeamFolderArchiveJobStatus._failed_validator,
TeamFolderArchiveJobStatus._tagmap.update(async_.PollResultBase._tagmap)
TeamFolderArchiveLaunch._complete_validator = TeamFolderMetadata_validator
TeamFolderArchiveLaunch._tagmap = {
    'complete': TeamFolderArchiveLaunch._complete_validator,
TeamFolderArchiveLaunch._tagmap.update(async_.LaunchResultBase._tagmap)
TeamFolderCreateArg.name.validator = bv.String()
TeamFolderCreateArg.sync_setting.validator = bv.Nullable(files.SyncSettingArg_validator)
TeamFolderCreateArg._all_field_names_ = set([
TeamFolderCreateArg._all_fields_ = [
    ('name', TeamFolderCreateArg.name.validator),
    ('sync_setting', TeamFolderCreateArg.sync_setting.validator),
TeamFolderCreateError._invalid_folder_name_validator = bv.Void()
TeamFolderCreateError._folder_name_already_used_validator = bv.Void()
TeamFolderCreateError._folder_name_reserved_validator = bv.Void()
TeamFolderCreateError._sync_settings_error_validator = files.SyncSettingsError_validator
TeamFolderCreateError._other_validator = bv.Void()
TeamFolderCreateError._tagmap = {
    'invalid_folder_name': TeamFolderCreateError._invalid_folder_name_validator,
    'folder_name_already_used': TeamFolderCreateError._folder_name_already_used_validator,
    'folder_name_reserved': TeamFolderCreateError._folder_name_reserved_validator,
    'sync_settings_error': TeamFolderCreateError._sync_settings_error_validator,
    'other': TeamFolderCreateError._other_validator,
TeamFolderCreateError.invalid_folder_name = TeamFolderCreateError('invalid_folder_name')
TeamFolderCreateError.folder_name_already_used = TeamFolderCreateError('folder_name_already_used')
TeamFolderCreateError.folder_name_reserved = TeamFolderCreateError('folder_name_reserved')
TeamFolderCreateError.other = TeamFolderCreateError('other')
TeamFolderGetInfoItem._id_not_found_validator = bv.String()
TeamFolderGetInfoItem._team_folder_metadata_validator = TeamFolderMetadata_validator
TeamFolderGetInfoItem._tagmap = {
    'id_not_found': TeamFolderGetInfoItem._id_not_found_validator,
    'team_folder_metadata': TeamFolderGetInfoItem._team_folder_metadata_validator,
TeamFolderIdListArg.team_folder_ids.validator = bv.List(common.SharedFolderId_validator, min_items=1)
TeamFolderIdListArg._all_field_names_ = set(['team_folder_ids'])
TeamFolderIdListArg._all_fields_ = [('team_folder_ids', TeamFolderIdListArg.team_folder_ids.validator)]
TeamFolderInvalidStatusError._active_validator = bv.Void()
TeamFolderInvalidStatusError._archived_validator = bv.Void()
TeamFolderInvalidStatusError._archive_in_progress_validator = bv.Void()
TeamFolderInvalidStatusError._other_validator = bv.Void()
TeamFolderInvalidStatusError._tagmap = {
    'active': TeamFolderInvalidStatusError._active_validator,
    'archived': TeamFolderInvalidStatusError._archived_validator,
    'archive_in_progress': TeamFolderInvalidStatusError._archive_in_progress_validator,
    'other': TeamFolderInvalidStatusError._other_validator,
TeamFolderInvalidStatusError.active = TeamFolderInvalidStatusError('active')
TeamFolderInvalidStatusError.archived = TeamFolderInvalidStatusError('archived')
TeamFolderInvalidStatusError.archive_in_progress = TeamFolderInvalidStatusError('archive_in_progress')
TeamFolderInvalidStatusError.other = TeamFolderInvalidStatusError('other')
TeamFolderListArg.limit.validator = bv.UInt32(min_value=1, max_value=1000)
TeamFolderListArg._all_field_names_ = set(['limit'])
TeamFolderListArg._all_fields_ = [('limit', TeamFolderListArg.limit.validator)]
TeamFolderListContinueArg.cursor.validator = bv.String()
TeamFolderListContinueArg._all_field_names_ = set(['cursor'])
TeamFolderListContinueArg._all_fields_ = [('cursor', TeamFolderListContinueArg.cursor.validator)]
TeamFolderListContinueError._invalid_cursor_validator = bv.Void()
TeamFolderListContinueError._other_validator = bv.Void()
TeamFolderListContinueError._tagmap = {
    'invalid_cursor': TeamFolderListContinueError._invalid_cursor_validator,
    'other': TeamFolderListContinueError._other_validator,
TeamFolderListContinueError.invalid_cursor = TeamFolderListContinueError('invalid_cursor')
TeamFolderListContinueError.other = TeamFolderListContinueError('other')
TeamFolderListError.access_error.validator = TeamFolderAccessError_validator
TeamFolderListError._all_field_names_ = set(['access_error'])
TeamFolderListError._all_fields_ = [('access_error', TeamFolderListError.access_error.validator)]
TeamFolderListResult.team_folders.validator = bv.List(TeamFolderMetadata_validator)
TeamFolderListResult.cursor.validator = bv.String()
TeamFolderListResult.has_more.validator = bv.Boolean()
TeamFolderListResult._all_field_names_ = set([
    'team_folders',
TeamFolderListResult._all_fields_ = [
    ('team_folders', TeamFolderListResult.team_folders.validator),
    ('cursor', TeamFolderListResult.cursor.validator),
    ('has_more', TeamFolderListResult.has_more.validator),
TeamFolderMetadata.team_folder_id.validator = common.SharedFolderId_validator
TeamFolderMetadata.name.validator = bv.String()
TeamFolderMetadata.status.validator = TeamFolderStatus_validator
TeamFolderMetadata.is_team_shared_dropbox.validator = bv.Boolean()
TeamFolderMetadata.sync_setting.validator = files.SyncSetting_validator
TeamFolderMetadata.content_sync_settings.validator = bv.List(files.ContentSyncSetting_validator)
TeamFolderMetadata._all_field_names_ = set([
    'team_folder_id',
    'is_team_shared_dropbox',
    'content_sync_settings',
TeamFolderMetadata._all_fields_ = [
    ('team_folder_id', TeamFolderMetadata.team_folder_id.validator),
    ('name', TeamFolderMetadata.name.validator),
    ('status', TeamFolderMetadata.status.validator),
    ('is_team_shared_dropbox', TeamFolderMetadata.is_team_shared_dropbox.validator),
    ('sync_setting', TeamFolderMetadata.sync_setting.validator),
    ('content_sync_settings', TeamFolderMetadata.content_sync_settings.validator),
TeamFolderPermanentlyDeleteError._tagmap = {
TeamFolderPermanentlyDeleteError._tagmap.update(BaseTeamFolderError._tagmap)
TeamFolderRenameArg.name.validator = bv.String()
TeamFolderRenameArg._all_field_names_ = TeamFolderIdArg._all_field_names_.union(set(['name']))
TeamFolderRenameArg._all_fields_ = TeamFolderIdArg._all_fields_ + [('name', TeamFolderRenameArg.name.validator)]
TeamFolderRenameError._invalid_folder_name_validator = bv.Void()
TeamFolderRenameError._folder_name_already_used_validator = bv.Void()
TeamFolderRenameError._folder_name_reserved_validator = bv.Void()
TeamFolderRenameError._tagmap = {
    'invalid_folder_name': TeamFolderRenameError._invalid_folder_name_validator,
    'folder_name_already_used': TeamFolderRenameError._folder_name_already_used_validator,
    'folder_name_reserved': TeamFolderRenameError._folder_name_reserved_validator,
TeamFolderRenameError._tagmap.update(BaseTeamFolderError._tagmap)
TeamFolderRenameError.invalid_folder_name = TeamFolderRenameError('invalid_folder_name')
TeamFolderRenameError.folder_name_already_used = TeamFolderRenameError('folder_name_already_used')
TeamFolderRenameError.folder_name_reserved = TeamFolderRenameError('folder_name_reserved')
TeamFolderStatus._active_validator = bv.Void()
TeamFolderStatus._archived_validator = bv.Void()
TeamFolderStatus._archive_in_progress_validator = bv.Void()
TeamFolderStatus._other_validator = bv.Void()
TeamFolderStatus._tagmap = {
    'active': TeamFolderStatus._active_validator,
    'archived': TeamFolderStatus._archived_validator,
    'archive_in_progress': TeamFolderStatus._archive_in_progress_validator,
    'other': TeamFolderStatus._other_validator,
TeamFolderStatus.active = TeamFolderStatus('active')
TeamFolderStatus.archived = TeamFolderStatus('archived')
TeamFolderStatus.archive_in_progress = TeamFolderStatus('archive_in_progress')
TeamFolderStatus.other = TeamFolderStatus('other')
TeamFolderTeamSharedDropboxError._disallowed_validator = bv.Void()
TeamFolderTeamSharedDropboxError._other_validator = bv.Void()
TeamFolderTeamSharedDropboxError._tagmap = {
    'disallowed': TeamFolderTeamSharedDropboxError._disallowed_validator,
    'other': TeamFolderTeamSharedDropboxError._other_validator,
TeamFolderTeamSharedDropboxError.disallowed = TeamFolderTeamSharedDropboxError('disallowed')
TeamFolderTeamSharedDropboxError.other = TeamFolderTeamSharedDropboxError('other')
TeamFolderUpdateSyncSettingsArg.sync_setting.validator = bv.Nullable(files.SyncSettingArg_validator)
TeamFolderUpdateSyncSettingsArg.content_sync_settings.validator = bv.Nullable(bv.List(files.ContentSyncSettingArg_validator))
TeamFolderUpdateSyncSettingsArg._all_field_names_ = TeamFolderIdArg._all_field_names_.union(set([
TeamFolderUpdateSyncSettingsArg._all_fields_ = TeamFolderIdArg._all_fields_ + [
    ('sync_setting', TeamFolderUpdateSyncSettingsArg.sync_setting.validator),
    ('content_sync_settings', TeamFolderUpdateSyncSettingsArg.content_sync_settings.validator),
TeamFolderUpdateSyncSettingsError._sync_settings_error_validator = files.SyncSettingsError_validator
TeamFolderUpdateSyncSettingsError._tagmap = {
    'sync_settings_error': TeamFolderUpdateSyncSettingsError._sync_settings_error_validator,
TeamFolderUpdateSyncSettingsError._tagmap.update(BaseTeamFolderError._tagmap)
TeamGetInfoResult.name.validator = bv.String()
TeamGetInfoResult.team_id.validator = bv.String()
TeamGetInfoResult.num_licensed_users.validator = bv.UInt32()
TeamGetInfoResult.num_provisioned_users.validator = bv.UInt32()
TeamGetInfoResult.num_used_licenses.validator = bv.UInt32()
TeamGetInfoResult.policies.validator = team_policies.TeamMemberPolicies_validator
TeamGetInfoResult._all_field_names_ = set([
    'team_id',
    'num_licensed_users',
    'num_provisioned_users',
    'num_used_licenses',
    'policies',
TeamGetInfoResult._all_fields_ = [
    ('name', TeamGetInfoResult.name.validator),
    ('team_id', TeamGetInfoResult.team_id.validator),
    ('num_licensed_users', TeamGetInfoResult.num_licensed_users.validator),
    ('num_provisioned_users', TeamGetInfoResult.num_provisioned_users.validator),
    ('num_used_licenses', TeamGetInfoResult.num_used_licenses.validator),
    ('policies', TeamGetInfoResult.policies.validator),
TeamMemberInfo.profile.validator = TeamMemberProfile_validator
TeamMemberInfo.role.validator = AdminTier_validator
    ('profile', TeamMemberInfo.profile.validator),
    ('role', TeamMemberInfo.role.validator),
TeamMemberInfoV2.profile.validator = TeamMemberProfile_validator
TeamMemberInfoV2.roles.validator = bv.Nullable(bv.List(TeamMemberRole_validator))
TeamMemberInfoV2._all_field_names_ = set([
TeamMemberInfoV2._all_fields_ = [
    ('profile', TeamMemberInfoV2.profile.validator),
    ('roles', TeamMemberInfoV2.roles.validator),
TeamMemberInfoV2Result.member_info.validator = TeamMemberInfoV2_validator
TeamMemberInfoV2Result._all_field_names_ = set(['member_info'])
TeamMemberInfoV2Result._all_fields_ = [('member_info', TeamMemberInfoV2Result.member_info.validator)]
TeamMemberProfile.groups.validator = bv.List(team_common.GroupId_validator)
TeamMemberProfile.member_folder_id.validator = common.NamespaceId_validator
TeamMemberProfile._all_field_names_ = MemberProfile._all_field_names_.union(set([
    'member_folder_id',
TeamMemberProfile._all_fields_ = MemberProfile._all_fields_ + [
    ('groups', TeamMemberProfile.groups.validator),
    ('member_folder_id', TeamMemberProfile.member_folder_id.validator),
TeamMemberRole.role_id.validator = TeamMemberRoleId_validator
TeamMemberRole.name.validator = bv.String(max_length=32)
TeamMemberRole.description.validator = bv.String(max_length=256)
TeamMemberRole._all_field_names_ = set([
    'role_id',
TeamMemberRole._all_fields_ = [
    ('role_id', TeamMemberRole.role_id.validator),
    ('name', TeamMemberRole.name.validator),
    ('description', TeamMemberRole.description.validator),
TeamMemberStatus._active_validator = bv.Void()
TeamMemberStatus._invited_validator = bv.Void()
TeamMemberStatus._suspended_validator = bv.Void()
TeamMemberStatus._removed_validator = RemovedStatus_validator
TeamMemberStatus._tagmap = {
    'active': TeamMemberStatus._active_validator,
    'invited': TeamMemberStatus._invited_validator,
    'suspended': TeamMemberStatus._suspended_validator,
    'removed': TeamMemberStatus._removed_validator,
TeamMemberStatus.active = TeamMemberStatus('active')
TeamMemberStatus.invited = TeamMemberStatus('invited')
TeamMemberStatus.suspended = TeamMemberStatus('suspended')
TeamMembershipType._full_validator = bv.Void()
TeamMembershipType._limited_validator = bv.Void()
TeamMembershipType._tagmap = {
    'full': TeamMembershipType._full_validator,
    'limited': TeamMembershipType._limited_validator,
TeamMembershipType.full = TeamMembershipType('full')
TeamMembershipType.limited = TeamMembershipType('limited')
TeamNamespacesListArg.limit.validator = bv.UInt32(min_value=1, max_value=1000)
TeamNamespacesListArg._all_field_names_ = set(['limit'])
TeamNamespacesListArg._all_fields_ = [('limit', TeamNamespacesListArg.limit.validator)]
TeamNamespacesListContinueArg.cursor.validator = bv.String()
TeamNamespacesListContinueArg._all_field_names_ = set(['cursor'])
TeamNamespacesListContinueArg._all_fields_ = [('cursor', TeamNamespacesListContinueArg.cursor.validator)]
TeamNamespacesListError._invalid_arg_validator = bv.Void()
TeamNamespacesListError._other_validator = bv.Void()
TeamNamespacesListError._tagmap = {
    'invalid_arg': TeamNamespacesListError._invalid_arg_validator,
    'other': TeamNamespacesListError._other_validator,
TeamNamespacesListError.invalid_arg = TeamNamespacesListError('invalid_arg')
TeamNamespacesListError.other = TeamNamespacesListError('other')
TeamNamespacesListContinueError._invalid_cursor_validator = bv.Void()
TeamNamespacesListContinueError._tagmap = {
    'invalid_cursor': TeamNamespacesListContinueError._invalid_cursor_validator,
TeamNamespacesListContinueError._tagmap.update(TeamNamespacesListError._tagmap)
TeamNamespacesListContinueError.invalid_cursor = TeamNamespacesListContinueError('invalid_cursor')
TeamNamespacesListResult.namespaces.validator = bv.List(NamespaceMetadata_validator)
TeamNamespacesListResult.cursor.validator = bv.String()
TeamNamespacesListResult.has_more.validator = bv.Boolean()
TeamNamespacesListResult._all_field_names_ = set([
    'namespaces',
TeamNamespacesListResult._all_fields_ = [
    ('namespaces', TeamNamespacesListResult.namespaces.validator),
    ('cursor', TeamNamespacesListResult.cursor.validator),
    ('has_more', TeamNamespacesListResult.has_more.validator),
TeamReportFailureReason._temporary_error_validator = bv.Void()
TeamReportFailureReason._many_reports_at_once_validator = bv.Void()
TeamReportFailureReason._too_much_data_validator = bv.Void()
TeamReportFailureReason._other_validator = bv.Void()
TeamReportFailureReason._tagmap = {
    'temporary_error': TeamReportFailureReason._temporary_error_validator,
    'many_reports_at_once': TeamReportFailureReason._many_reports_at_once_validator,
    'too_much_data': TeamReportFailureReason._too_much_data_validator,
    'other': TeamReportFailureReason._other_validator,
TeamReportFailureReason.temporary_error = TeamReportFailureReason('temporary_error')
TeamReportFailureReason.many_reports_at_once = TeamReportFailureReason('many_reports_at_once')
TeamReportFailureReason.too_much_data = TeamReportFailureReason('too_much_data')
TeamReportFailureReason.other = TeamReportFailureReason('other')
TokenGetAuthenticatedAdminError._mapping_not_found_validator = bv.Void()
TokenGetAuthenticatedAdminError._admin_not_active_validator = bv.Void()
TokenGetAuthenticatedAdminError._other_validator = bv.Void()
TokenGetAuthenticatedAdminError._tagmap = {
    'mapping_not_found': TokenGetAuthenticatedAdminError._mapping_not_found_validator,
    'admin_not_active': TokenGetAuthenticatedAdminError._admin_not_active_validator,
    'other': TokenGetAuthenticatedAdminError._other_validator,
TokenGetAuthenticatedAdminError.mapping_not_found = TokenGetAuthenticatedAdminError('mapping_not_found')
TokenGetAuthenticatedAdminError.admin_not_active = TokenGetAuthenticatedAdminError('admin_not_active')
TokenGetAuthenticatedAdminError.other = TokenGetAuthenticatedAdminError('other')
TokenGetAuthenticatedAdminResult.admin_profile.validator = TeamMemberProfile_validator
TokenGetAuthenticatedAdminResult._all_field_names_ = set(['admin_profile'])
TokenGetAuthenticatedAdminResult._all_fields_ = [('admin_profile', TokenGetAuthenticatedAdminResult.admin_profile.validator)]
UploadApiRateLimitValue._unlimited_validator = bv.Void()
UploadApiRateLimitValue._limit_validator = bv.UInt32()
UploadApiRateLimitValue._other_validator = bv.Void()
UploadApiRateLimitValue._tagmap = {
    'unlimited': UploadApiRateLimitValue._unlimited_validator,
    'limit': UploadApiRateLimitValue._limit_validator,
    'other': UploadApiRateLimitValue._other_validator,
UploadApiRateLimitValue.unlimited = UploadApiRateLimitValue('unlimited')
UploadApiRateLimitValue.other = UploadApiRateLimitValue('other')
UserAddResult._success_validator = UserSecondaryEmailsResult_validator
UserAddResult._invalid_user_validator = UserSelectorArg_validator
UserAddResult._unverified_validator = UserSelectorArg_validator
UserAddResult._placeholder_user_validator = UserSelectorArg_validator
UserAddResult._other_validator = bv.Void()
UserAddResult._tagmap = {
    'success': UserAddResult._success_validator,
    'invalid_user': UserAddResult._invalid_user_validator,
    'unverified': UserAddResult._unverified_validator,
    'placeholder_user': UserAddResult._placeholder_user_validator,
    'other': UserAddResult._other_validator,
UserAddResult.other = UserAddResult('other')
UserCustomQuotaArg.user.validator = UserSelectorArg_validator
UserCustomQuotaArg.quota_gb.validator = UserQuota_validator
UserCustomQuotaArg._all_field_names_ = set([
    'quota_gb',
UserCustomQuotaArg._all_fields_ = [
    ('user', UserCustomQuotaArg.user.validator),
    ('quota_gb', UserCustomQuotaArg.quota_gb.validator),
UserCustomQuotaResult.user.validator = UserSelectorArg_validator
UserCustomQuotaResult.quota_gb.validator = bv.Nullable(UserQuota_validator)
UserCustomQuotaResult._all_field_names_ = set([
UserCustomQuotaResult._all_fields_ = [
    ('user', UserCustomQuotaResult.user.validator),
    ('quota_gb', UserCustomQuotaResult.quota_gb.validator),
UserDeleteEmailsResult.user.validator = UserSelectorArg_validator
UserDeleteEmailsResult.results.validator = bv.List(DeleteSecondaryEmailResult_validator)
UserDeleteEmailsResult._all_field_names_ = set([
    'results',
UserDeleteEmailsResult._all_fields_ = [
    ('user', UserDeleteEmailsResult.user.validator),
    ('results', UserDeleteEmailsResult.results.validator),
UserDeleteResult._success_validator = UserDeleteEmailsResult_validator
UserDeleteResult._invalid_user_validator = UserSelectorArg_validator
UserDeleteResult._other_validator = bv.Void()
UserDeleteResult._tagmap = {
    'success': UserDeleteResult._success_validator,
    'invalid_user': UserDeleteResult._invalid_user_validator,
    'other': UserDeleteResult._other_validator,
UserDeleteResult.other = UserDeleteResult('other')
UserResendEmailsResult.user.validator = UserSelectorArg_validator
UserResendEmailsResult.results.validator = bv.List(ResendSecondaryEmailResult_validator)
UserResendEmailsResult._all_field_names_ = set([
UserResendEmailsResult._all_fields_ = [
    ('user', UserResendEmailsResult.user.validator),
    ('results', UserResendEmailsResult.results.validator),
UserResendResult._success_validator = UserResendEmailsResult_validator
UserResendResult._invalid_user_validator = UserSelectorArg_validator
UserResendResult._other_validator = bv.Void()
UserResendResult._tagmap = {
    'success': UserResendResult._success_validator,
    'invalid_user': UserResendResult._invalid_user_validator,
    'other': UserResendResult._other_validator,
UserResendResult.other = UserResendResult('other')
UserSecondaryEmailsArg.user.validator = UserSelectorArg_validator
UserSecondaryEmailsArg.secondary_emails.validator = bv.List(common.EmailAddress_validator)
UserSecondaryEmailsArg._all_field_names_ = set([
UserSecondaryEmailsArg._all_fields_ = [
    ('user', UserSecondaryEmailsArg.user.validator),
    ('secondary_emails', UserSecondaryEmailsArg.secondary_emails.validator),
UserSecondaryEmailsResult.user.validator = UserSelectorArg_validator
UserSecondaryEmailsResult.results.validator = bv.List(AddSecondaryEmailResult_validator)
UserSecondaryEmailsResult._all_field_names_ = set([
UserSecondaryEmailsResult._all_fields_ = [
    ('user', UserSecondaryEmailsResult.user.validator),
    ('results', UserSecondaryEmailsResult.results.validator),
UserSelectorArg._team_member_id_validator = team_common.TeamMemberId_validator
UserSelectorArg._external_id_validator = team_common.MemberExternalId_validator
UserSelectorArg._email_validator = common.EmailAddress_validator
UserSelectorArg._tagmap = {
    'team_member_id': UserSelectorArg._team_member_id_validator,
    'external_id': UserSelectorArg._external_id_validator,
    'email': UserSelectorArg._email_validator,
UsersSelectorArg._team_member_ids_validator = bv.List(team_common.TeamMemberId_validator)
UsersSelectorArg._external_ids_validator = bv.List(team_common.MemberExternalId_validator)
UsersSelectorArg._emails_validator = bv.List(common.EmailAddress_validator)
UsersSelectorArg._tagmap = {
    'team_member_ids': UsersSelectorArg._team_member_ids_validator,
    'external_ids': UsersSelectorArg._external_ids_validator,
    'emails': UsersSelectorArg._emails_validator,
ExcludedUsersListArg.limit.default = 1000
GroupCreateArg.add_creator_as_owner.default = False
IncludeMembersArg.return_members.default = True
GroupMembersSetAccessTypeArg.return_members.default = True
GroupsListArg.limit.default = 1000
GroupsMembersListArg.limit.default = 1000
LegalHoldsListPoliciesArg.include_released.default = False
ListMemberDevicesArg.include_web_sessions.default = True
ListMemberDevicesArg.include_desktop_clients.default = True
ListMemberDevicesArg.include_mobile_clients.default = True
ListMembersDevicesArg.include_web_sessions.default = True
ListMembersDevicesArg.include_desktop_clients.default = True
ListMembersDevicesArg.include_mobile_clients.default = True
ListTeamDevicesArg.include_web_sessions.default = True
ListTeamDevicesArg.include_desktop_clients.default = True
ListTeamDevicesArg.include_mobile_clients.default = True
MemberAddArgBase.send_welcome_email.default = True
MemberAddArg.role.default = AdminTier.member_only
MembersAddArgBase.force_async.default = False
MembersDeactivateArg.wipe_data.default = True
MembersListArg.limit.default = 1000
MembersListArg.include_removed.default = False
MembersRemoveArg.keep_account.default = False
MembersRemoveArg.retain_team_shares.default = False
RevokeDesktopClientArg.delete_on_unlink.default = False
RevokeLinkedApiAppArg.keep_app_folder.default = True
SharingAllowlistListArg.limit.default = 1000
SharingAllowlistListResponse.cursor.default = ''
SharingAllowlistListResponse.has_more.default = False
TeamFolderArchiveArg.force_async_off.default = False
TeamFolderListArg.limit.default = 1000
TeamGetInfoResult.num_used_licenses.default = 0
TeamNamespacesListArg.limit.default = 1000
devices_list_member_devices = bb.Route(
    'devices/list_member_devices',
    ListMemberDevicesArg_validator,
    ListMemberDevicesResult_validator,
    ListMemberDevicesError_validator,
devices_list_members_devices = bb.Route(
    'devices/list_members_devices',
    ListMembersDevicesArg_validator,
    ListMembersDevicesResult_validator,
    ListMembersDevicesError_validator,
devices_list_team_devices = bb.Route(
    'devices/list_team_devices',
    ListTeamDevicesArg_validator,
    ListTeamDevicesResult_validator,
    ListTeamDevicesError_validator,
devices_revoke_device_session = bb.Route(
    'devices/revoke_device_session',
    RevokeDeviceSessionArg_validator,
    RevokeDeviceSessionError_validator,
devices_revoke_device_session_batch = bb.Route(
    'devices/revoke_device_session_batch',
    RevokeDeviceSessionBatchArg_validator,
    RevokeDeviceSessionBatchResult_validator,
    RevokeDeviceSessionBatchError_validator,
features_get_values = bb.Route(
    'features/get_values',
    FeaturesGetValuesBatchArg_validator,
    FeaturesGetValuesBatchResult_validator,
    FeaturesGetValuesBatchError_validator,
get_info = bb.Route(
    'get_info',
    TeamGetInfoResult_validator,
groups_create = bb.Route(
    'groups/create',
    GroupCreateArg_validator,
    GroupFullInfo_validator,
    GroupCreateError_validator,
groups_delete = bb.Route(
    'groups/delete',
    GroupSelector_validator,
    GroupDeleteError_validator,
groups_get_info = bb.Route(
    'groups/get_info',
    GroupsSelector_validator,
    GroupsGetInfoResult_validator,
    GroupsGetInfoError_validator,
groups_job_status_get = bb.Route(
    'groups/job_status/get',
    async_.PollEmptyResult_validator,
    GroupsPollError_validator,
groups_list = bb.Route(
    'groups/list',
    GroupsListArg_validator,
    GroupsListResult_validator,
groups_list_continue = bb.Route(
    'groups/list/continue',
    GroupsListContinueArg_validator,
    GroupsListContinueError_validator,
groups_members_add = bb.Route(
    'groups/members/add',
    GroupMembersAddArg_validator,
    GroupMembersChangeResult_validator,
    GroupMembersAddError_validator,
groups_members_list = bb.Route(
    'groups/members/list',
    GroupsMembersListArg_validator,
    GroupsMembersListResult_validator,
    GroupSelectorError_validator,
groups_members_list_continue = bb.Route(
    'groups/members/list/continue',
    GroupsMembersListContinueArg_validator,
    GroupsMembersListContinueError_validator,
groups_members_remove = bb.Route(
    'groups/members/remove',
    GroupMembersRemoveArg_validator,
    GroupMembersRemoveError_validator,
groups_members_set_access_type = bb.Route(
    'groups/members/set_access_type',
    GroupMembersSetAccessTypeArg_validator,
    GroupMemberSetAccessTypeError_validator,
groups_update = bb.Route(
    'groups/update',
    GroupUpdateArgs_validator,
    GroupUpdateError_validator,
legal_holds_create_policy = bb.Route(
    'legal_holds/create_policy',
    LegalHoldsPolicyCreateArg_validator,
    LegalHoldsPolicyCreateResult_validator,
    LegalHoldsPolicyCreateError_validator,
legal_holds_get_policy = bb.Route(
    'legal_holds/get_policy',
    LegalHoldsGetPolicyArg_validator,
    LegalHoldsGetPolicyResult_validator,
    LegalHoldsGetPolicyError_validator,
legal_holds_list_held_revisions = bb.Route(
    'legal_holds/list_held_revisions',
    LegalHoldsListHeldRevisionsArg_validator,
    LegalHoldsListHeldRevisionResult_validator,
    LegalHoldsListHeldRevisionsError_validator,
legal_holds_list_held_revisions_continue = bb.Route(
    'legal_holds/list_held_revisions_continue',
    LegalHoldsListHeldRevisionsContinueArg_validator,
legal_holds_list_policies = bb.Route(
    'legal_holds/list_policies',
    LegalHoldsListPoliciesArg_validator,
    LegalHoldsListPoliciesResult_validator,
    LegalHoldsListPoliciesError_validator,
legal_holds_release_policy = bb.Route(
    'legal_holds/release_policy',
    LegalHoldsPolicyReleaseArg_validator,
    LegalHoldsPolicyReleaseError_validator,
legal_holds_update_policy = bb.Route(
    'legal_holds/update_policy',
    LegalHoldsPolicyUpdateArg_validator,
    LegalHoldsPolicyUpdateResult_validator,
    LegalHoldsPolicyUpdateError_validator,
linked_apps_list_member_linked_apps = bb.Route(
    'linked_apps/list_member_linked_apps',
    ListMemberAppsArg_validator,
    ListMemberAppsResult_validator,
    ListMemberAppsError_validator,
linked_apps_list_members_linked_apps = bb.Route(
    'linked_apps/list_members_linked_apps',
    ListMembersAppsArg_validator,
    ListMembersAppsResult_validator,
    ListMembersAppsError_validator,
linked_apps_list_team_linked_apps = bb.Route(
    'linked_apps/list_team_linked_apps',
    ListTeamAppsArg_validator,
    ListTeamAppsResult_validator,
    ListTeamAppsError_validator,
linked_apps_revoke_linked_app = bb.Route(
    'linked_apps/revoke_linked_app',
    RevokeLinkedApiAppArg_validator,
    RevokeLinkedAppError_validator,
linked_apps_revoke_linked_app_batch = bb.Route(
    'linked_apps/revoke_linked_app_batch',
    RevokeLinkedApiAppBatchArg_validator,
    RevokeLinkedAppBatchResult_validator,
    RevokeLinkedAppBatchError_validator,
member_space_limits_excluded_users_add = bb.Route(
    'member_space_limits/excluded_users/add',
    ExcludedUsersUpdateArg_validator,
    ExcludedUsersUpdateResult_validator,
    ExcludedUsersUpdateError_validator,
member_space_limits_excluded_users_list = bb.Route(
    'member_space_limits/excluded_users/list',
    ExcludedUsersListArg_validator,
    ExcludedUsersListResult_validator,
    ExcludedUsersListError_validator,
member_space_limits_excluded_users_list_continue = bb.Route(
    'member_space_limits/excluded_users/list/continue',
    ExcludedUsersListContinueArg_validator,
    ExcludedUsersListContinueError_validator,
member_space_limits_excluded_users_remove = bb.Route(
    'member_space_limits/excluded_users/remove',
member_space_limits_get_custom_quota = bb.Route(
    'member_space_limits/get_custom_quota',
    CustomQuotaUsersArg_validator,
    bv.List(CustomQuotaResult_validator),
    CustomQuotaError_validator,
member_space_limits_remove_custom_quota = bb.Route(
    'member_space_limits/remove_custom_quota',
    bv.List(RemoveCustomQuotaResult_validator),
member_space_limits_set_custom_quota = bb.Route(
    'member_space_limits/set_custom_quota',
    SetCustomQuotaArg_validator,
    SetCustomQuotaError_validator,
members_add_v2 = bb.Route(
    'members/add',
    MembersAddV2Arg_validator,
    MembersAddLaunchV2Result_validator,
members_add = bb.Route(
    MembersAddArg_validator,
    MembersAddLaunch_validator,
members_add_job_status_get_v2 = bb.Route(
    'members/add/job_status/get',
    MembersAddJobStatusV2Result_validator,
members_add_job_status_get = bb.Route(
    MembersAddJobStatus_validator,
members_delete_profile_photo_v2 = bb.Route(
    'members/delete_profile_photo',
    MembersDeleteProfilePhotoArg_validator,
    TeamMemberInfoV2Result_validator,
    MembersDeleteProfilePhotoError_validator,
members_delete_profile_photo = bb.Route(
    TeamMemberInfo_validator,
members_get_available_team_member_roles = bb.Route(
    'members/get_available_team_member_roles',
    MembersGetAvailableTeamMemberRolesResult_validator,
members_get_info_v2 = bb.Route(
    'members/get_info',
    MembersGetInfoV2Arg_validator,
    MembersGetInfoV2Result_validator,
    MembersGetInfoError_validator,
members_get_info = bb.Route(
    MembersGetInfoArgs_validator,
    MembersGetInfoResult_validator,
members_list_v2 = bb.Route(
    'members/list',
    MembersListArg_validator,
    MembersListV2Result_validator,
    MembersListError_validator,
members_list = bb.Route(
    MembersListResult_validator,
members_list_continue_v2 = bb.Route(
    'members/list/continue',
    MembersListContinueArg_validator,
    MembersListContinueError_validator,
members_list_continue = bb.Route(
members_move_former_member_files = bb.Route(
    'members/move_former_member_files',
    MembersDataTransferArg_validator,
    MembersTransferFormerMembersFilesError_validator,
members_move_former_member_files_job_status_check = bb.Route(
    'members/move_former_member_files/job_status/check',
members_recover = bb.Route(
    'members/recover',
    MembersRecoverArg_validator,
    MembersRecoverError_validator,
members_remove = bb.Route(
    'members/remove',
    MembersRemoveArg_validator,
    MembersRemoveError_validator,
members_remove_job_status_get = bb.Route(
    'members/remove/job_status/get',
members_secondary_emails_add = bb.Route(
    'members/secondary_emails/add',
    AddSecondaryEmailsArg_validator,
    AddSecondaryEmailsResult_validator,
    AddSecondaryEmailsError_validator,
members_secondary_emails_delete = bb.Route(
    'members/secondary_emails/delete',
    DeleteSecondaryEmailsArg_validator,
    DeleteSecondaryEmailsResult_validator,
members_secondary_emails_resend_verification_emails = bb.Route(
    'members/secondary_emails/resend_verification_emails',
    ResendVerificationEmailArg_validator,
    ResendVerificationEmailResult_validator,
members_send_welcome_email = bb.Route(
    'members/send_welcome_email',
    UserSelectorArg_validator,
    MembersSendWelcomeError_validator,
members_set_admin_permissions_v2 = bb.Route(
    'members/set_admin_permissions',
    MembersSetPermissions2Arg_validator,
    MembersSetPermissions2Result_validator,
    MembersSetPermissions2Error_validator,
members_set_admin_permissions = bb.Route(
    MembersSetPermissionsArg_validator,
    MembersSetPermissionsResult_validator,
    MembersSetPermissionsError_validator,
members_set_profile_v2 = bb.Route(
    'members/set_profile',
    MembersSetProfileArg_validator,
    MembersSetProfileError_validator,
members_set_profile = bb.Route(
members_set_profile_photo_v2 = bb.Route(
    'members/set_profile_photo',
    MembersSetProfilePhotoArg_validator,
    MembersSetProfilePhotoError_validator,
members_set_profile_photo = bb.Route(
members_suspend = bb.Route(
    'members/suspend',
    MembersDeactivateArg_validator,
    MembersSuspendError_validator,
members_unsuspend = bb.Route(
    'members/unsuspend',
    MembersUnsuspendArg_validator,
    MembersUnsuspendError_validator,
namespaces_list = bb.Route(
    'namespaces/list',
    TeamNamespacesListArg_validator,
    TeamNamespacesListResult_validator,
    TeamNamespacesListError_validator,
namespaces_list_continue = bb.Route(
    'namespaces/list/continue',
    TeamNamespacesListContinueArg_validator,
    TeamNamespacesListContinueError_validator,
properties_template_add = bb.Route(
    'properties/template/add',
    file_properties.AddTemplateArg_validator,
    file_properties.AddTemplateResult_validator,
    file_properties.ModifyTemplateError_validator,
properties_template_update = bb.Route(
    'properties/template/update',
    file_properties.UpdateTemplateArg_validator,
    file_properties.UpdateTemplateResult_validator,
reports_get_activity = bb.Route(
    'reports/get_activity',
    DateRange_validator,
    GetActivityReport_validator,
    DateRangeError_validator,
reports_get_devices = bb.Route(
    'reports/get_devices',
    GetDevicesReport_validator,
reports_get_membership = bb.Route(
    'reports/get_membership',
    GetMembershipReport_validator,
reports_get_storage = bb.Route(
    'reports/get_storage',
    GetStorageReport_validator,
sharing_allowlist_add = bb.Route(
    'sharing_allowlist/add',
    SharingAllowlistAddArgs_validator,
    SharingAllowlistAddResponse_validator,
    SharingAllowlistAddError_validator,
sharing_allowlist_list = bb.Route(
    'sharing_allowlist/list',
    SharingAllowlistListArg_validator,
    SharingAllowlistListResponse_validator,
    SharingAllowlistListError_validator,
sharing_allowlist_list_continue = bb.Route(
    'sharing_allowlist/list/continue',
    SharingAllowlistListContinueArg_validator,
    SharingAllowlistListContinueError_validator,
sharing_allowlist_remove = bb.Route(
    'sharing_allowlist/remove',
    SharingAllowlistRemoveArgs_validator,
    SharingAllowlistRemoveResponse_validator,
    SharingAllowlistRemoveError_validator,
team_folder_activate = bb.Route(
    'team_folder/activate',
    TeamFolderIdArg_validator,
    TeamFolderMetadata_validator,
    TeamFolderActivateError_validator,
team_folder_archive = bb.Route(
    'team_folder/archive',
    TeamFolderArchiveArg_validator,
    TeamFolderArchiveLaunch_validator,
    TeamFolderArchiveError_validator,
team_folder_archive_check = bb.Route(
    'team_folder/archive/check',
    TeamFolderArchiveJobStatus_validator,
team_folder_create = bb.Route(
    'team_folder/create',
    TeamFolderCreateArg_validator,
    TeamFolderCreateError_validator,
team_folder_get_info = bb.Route(
    'team_folder/get_info',
    TeamFolderIdListArg_validator,
    bv.List(TeamFolderGetInfoItem_validator),
team_folder_list = bb.Route(
    'team_folder/list',
    TeamFolderListArg_validator,
    TeamFolderListResult_validator,
    TeamFolderListError_validator,
team_folder_list_continue = bb.Route(
    'team_folder/list/continue',
    TeamFolderListContinueArg_validator,
    TeamFolderListContinueError_validator,
team_folder_permanently_delete = bb.Route(
    'team_folder/permanently_delete',
    TeamFolderPermanentlyDeleteError_validator,
team_folder_rename = bb.Route(
    'team_folder/rename',
    TeamFolderRenameArg_validator,
    TeamFolderRenameError_validator,
team_folder_update_sync_settings = bb.Route(
    'team_folder/update_sync_settings',
    TeamFolderUpdateSyncSettingsArg_validator,
    TeamFolderUpdateSyncSettingsError_validator,
token_get_authenticated_admin = bb.Route(
    'token/get_authenticated_admin',
    TokenGetAuthenticatedAdminResult_validator,
    TokenGetAuthenticatedAdminError_validator,
    'devices/list_member_devices': devices_list_member_devices,
    'devices/list_members_devices': devices_list_members_devices,
    'devices/list_team_devices': devices_list_team_devices,
    'devices/revoke_device_session': devices_revoke_device_session,
    'devices/revoke_device_session_batch': devices_revoke_device_session_batch,
    'features/get_values': features_get_values,
    'get_info': get_info,
    'groups/create': groups_create,
    'groups/delete': groups_delete,
    'groups/get_info': groups_get_info,
    'groups/job_status/get': groups_job_status_get,
    'groups/list': groups_list,
    'groups/list/continue': groups_list_continue,
    'groups/members/add': groups_members_add,
    'groups/members/list': groups_members_list,
    'groups/members/list/continue': groups_members_list_continue,
    'groups/members/remove': groups_members_remove,
    'groups/members/set_access_type': groups_members_set_access_type,
    'groups/update': groups_update,
    'legal_holds/create_policy': legal_holds_create_policy,
    'legal_holds/get_policy': legal_holds_get_policy,
    'legal_holds/list_held_revisions': legal_holds_list_held_revisions,
    'legal_holds/list_held_revisions_continue': legal_holds_list_held_revisions_continue,
    'legal_holds/list_policies': legal_holds_list_policies,
    'legal_holds/release_policy': legal_holds_release_policy,
    'legal_holds/update_policy': legal_holds_update_policy,
    'linked_apps/list_member_linked_apps': linked_apps_list_member_linked_apps,
    'linked_apps/list_members_linked_apps': linked_apps_list_members_linked_apps,
    'linked_apps/list_team_linked_apps': linked_apps_list_team_linked_apps,
    'linked_apps/revoke_linked_app': linked_apps_revoke_linked_app,
    'linked_apps/revoke_linked_app_batch': linked_apps_revoke_linked_app_batch,
    'member_space_limits/excluded_users/add': member_space_limits_excluded_users_add,
    'member_space_limits/excluded_users/list': member_space_limits_excluded_users_list,
    'member_space_limits/excluded_users/list/continue': member_space_limits_excluded_users_list_continue,
    'member_space_limits/excluded_users/remove': member_space_limits_excluded_users_remove,
    'member_space_limits/get_custom_quota': member_space_limits_get_custom_quota,
    'member_space_limits/remove_custom_quota': member_space_limits_remove_custom_quota,
    'member_space_limits/set_custom_quota': member_space_limits_set_custom_quota,
    'members/add:2': members_add_v2,
    'members/add': members_add,
    'members/add/job_status/get:2': members_add_job_status_get_v2,
    'members/add/job_status/get': members_add_job_status_get,
    'members/delete_profile_photo:2': members_delete_profile_photo_v2,
    'members/delete_profile_photo': members_delete_profile_photo,
    'members/get_available_team_member_roles': members_get_available_team_member_roles,
    'members/get_info:2': members_get_info_v2,
    'members/get_info': members_get_info,
    'members/list:2': members_list_v2,
    'members/list': members_list,
    'members/list/continue:2': members_list_continue_v2,
    'members/list/continue': members_list_continue,
    'members/move_former_member_files': members_move_former_member_files,
    'members/move_former_member_files/job_status/check': members_move_former_member_files_job_status_check,
    'members/recover': members_recover,
    'members/remove': members_remove,
    'members/remove/job_status/get': members_remove_job_status_get,
    'members/secondary_emails/add': members_secondary_emails_add,
    'members/secondary_emails/delete': members_secondary_emails_delete,
    'members/secondary_emails/resend_verification_emails': members_secondary_emails_resend_verification_emails,
    'members/send_welcome_email': members_send_welcome_email,
    'members/set_admin_permissions:2': members_set_admin_permissions_v2,
    'members/set_admin_permissions': members_set_admin_permissions,
    'members/set_profile:2': members_set_profile_v2,
    'members/set_profile': members_set_profile,
    'members/set_profile_photo:2': members_set_profile_photo_v2,
    'members/set_profile_photo': members_set_profile_photo,
    'members/suspend': members_suspend,
    'members/unsuspend': members_unsuspend,
    'namespaces/list': namespaces_list,
    'namespaces/list/continue': namespaces_list_continue,
    'properties/template/add': properties_template_add,
    'properties/template/update': properties_template_update,
    'reports/get_activity': reports_get_activity,
    'reports/get_devices': reports_get_devices,
    'reports/get_membership': reports_get_membership,
    'reports/get_storage': reports_get_storage,
    'sharing_allowlist/add': sharing_allowlist_add,
    'sharing_allowlist/list': sharing_allowlist_list,
    'sharing_allowlist/list/continue': sharing_allowlist_list_continue,
    'sharing_allowlist/remove': sharing_allowlist_remove,
    'team_folder/activate': team_folder_activate,
    'team_folder/archive': team_folder_archive,
    'team_folder/archive/check': team_folder_archive_check,
    'team_folder/create': team_folder_create,
    'team_folder/get_info': team_folder_get_info,
    'team_folder/list': team_folder_list,
    'team_folder/list/continue': team_folder_list_continue,
    'team_folder/permanently_delete': team_folder_permanently_delete,
    'team_folder/rename': team_folder_rename,
    'team_folder/update_sync_settings': team_folder_update_sync_settings,
    'token/get_authenticated_admin': token_get_authenticated_admin,
