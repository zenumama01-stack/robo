This namespace contains endpoints and data types for user management.
class Account(bb.Struct):
    The amount of detail revealed about an account depends on the user being
    queried and the user making the query.
    :ivar users.Account.account_id: The user's unique Dropbox ID.
    :ivar users.Account.name: Details of a user's name.
    :ivar users.Account.email: The user's email address. Do not rely on this
        without checking the ``email_verified`` field. Even then, it's possible
        that the user has since lost access to their email.
    :ivar users.Account.email_verified: Whether the user has verified their
    :ivar users.Account.profile_photo_url: URL for the photo representing the
        user, if one is set.
    :ivar users.Account.disabled: Whether the user has been disabled.
        '_disabled_value',
                 disabled=None,
        self._disabled_value = bb.NOT_SET
        if disabled is not None:
            self.disabled = disabled
    # Instance attribute type: Name (validator is set below)
    disabled = bb.Attribute("disabled")
        super(Account, self)._process_custom_annotations(annotation_type, field_path, processor)
Account_validator = bv.Struct(Account)
class BasicAccount(Account):
    Basic information about any account.
    :ivar users.BasicAccount.is_teammate: Whether this user is a teammate of the
        current user. If this account is the current user's account, then this
        will be ``True``.
    :ivar users.BasicAccount.team_member_id: The user's unique team member id.
        This field will only be present if the user is part of a team and
        ``is_teammate`` is ``True``.
        '_is_teammate_value',
                 is_teammate=None,
                 profile_photo_url=None,
        super(BasicAccount, self).__init__(account_id,
                                           disabled,
        self._is_teammate_value = bb.NOT_SET
        if is_teammate is not None:
            self.is_teammate = is_teammate
    is_teammate = bb.Attribute("is_teammate")
        super(BasicAccount, self)._process_custom_annotations(annotation_type, field_path, processor)
BasicAccount_validator = bv.Struct(BasicAccount)
class FileLockingValue(bb.Union):
    The value for ``UserFeature.file_locking``.
    :ivar bool users.FileLockingValue.enabled: When this value is True, the user
        can lock files in shared directories. When the value is False the user
        can unlock the files they have locked or request to unlock files locked
        by others.
        :rtype: FileLockingValue
        When this value is True, the user can lock files in shared directories.
        When the value is False the user can unlock the files they have locked
        or request to unlock files locked by others.
        super(FileLockingValue, self)._process_custom_annotations(annotation_type, field_path, processor)
FileLockingValue_validator = bv.Union(FileLockingValue)
class FullAccount(Account):
    Detailed information about the current user's account.
    :ivar users.FullAccount.country: The user's two-letter country code, if
        available. Country codes are based on `ISO 3166-1
        <http://en.wikipedia.org/wiki/ISO_3166-1>`_.
    :ivar users.FullAccount.locale: The language that the user specified. Locale
        tags will be `IETF language tags
        <http://en.wikipedia.org/wiki/IETF_language_tag>`_.
    :ivar users.FullAccount.referral_link: The user's `referral link
        <https://www.dropbox.com/referrals>`_.
    :ivar users.FullAccount.team: If this account is a member of a team,
        information about that team.
    :ivar users.FullAccount.team_member_id: This account's unique team member
        id. This field will only be present if ``team`` is present.
    :ivar users.FullAccount.is_paired: Whether the user has a personal and work
        account. If the current account is personal, then ``team`` will always
        be None, but ``is_paired`` will indicate if a work account is linked.
    :ivar users.FullAccount.account_type: What type of account this user has.
    :ivar users.FullAccount.root_info: The root info for this account.
        '_referral_link_value',
        '_is_paired_value',
        '_account_type_value',
        '_root_info_value',
                 locale=None,
                 referral_link=None,
                 is_paired=None,
                 account_type=None,
                 root_info=None,
                 team=None,
        super(FullAccount, self).__init__(account_id,
        self._referral_link_value = bb.NOT_SET
        self._is_paired_value = bb.NOT_SET
        self._account_type_value = bb.NOT_SET
        self._root_info_value = bb.NOT_SET
        if referral_link is not None:
            self.referral_link = referral_link
        if is_paired is not None:
            self.is_paired = is_paired
        if account_type is not None:
            self.account_type = account_type
        if root_info is not None:
            self.root_info = root_info
    locale = bb.Attribute("locale")
    referral_link = bb.Attribute("referral_link")
    # Instance attribute type: FullTeam (validator is set below)
    is_paired = bb.Attribute("is_paired")
    # Instance attribute type: users_common.AccountType (validator is set below)
    account_type = bb.Attribute("account_type", user_defined=True)
    # Instance attribute type: common.RootInfo (validator is set below)
    root_info = bb.Attribute("root_info", user_defined=True)
        super(FullAccount, self)._process_custom_annotations(annotation_type, field_path, processor)
FullAccount_validator = bv.Struct(FullAccount)
class Team(bb.Struct):
    Information about a team.
    :ivar users.Team.id: The team's unique ID.
    :ivar users.Team.name: The name of the team.
        super(Team, self)._process_custom_annotations(annotation_type, field_path, processor)
Team_validator = bv.Struct(Team)
class FullTeam(Team):
    Detailed information about a team.
    :ivar users.FullTeam.sharing_policies: Team policies governing sharing.
    :ivar users.FullTeam.office_addin_policy: Team policy governing the use of
        the Office Add-In.
        '_sharing_policies_value',
        '_office_addin_policy_value',
                 sharing_policies=None,
                 office_addin_policy=None):
        super(FullTeam, self).__init__(id,
        self._sharing_policies_value = bb.NOT_SET
        self._office_addin_policy_value = bb.NOT_SET
        if sharing_policies is not None:
            self.sharing_policies = sharing_policies
        if office_addin_policy is not None:
            self.office_addin_policy = office_addin_policy
    # Instance attribute type: team_policies.TeamSharingPolicies (validator is set below)
    sharing_policies = bb.Attribute("sharing_policies", user_defined=True)
    # Instance attribute type: team_policies.OfficeAddInPolicy (validator is set below)
    office_addin_policy = bb.Attribute("office_addin_policy", user_defined=True)
        super(FullTeam, self)._process_custom_annotations(annotation_type, field_path, processor)
FullTeam_validator = bv.Struct(FullTeam)
class GetAccountArg(bb.Struct):
    :ivar users.GetAccountArg.account_id: A user's account identifier.
        super(GetAccountArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetAccountArg_validator = bv.Struct(GetAccountArg)
class GetAccountBatchArg(bb.Struct):
    :ivar users.GetAccountBatchArg.account_ids: List of user account
        identifiers.  Should not contain any duplicate account IDs.
        '_account_ids_value',
                 account_ids=None):
        self._account_ids_value = bb.NOT_SET
        if account_ids is not None:
            self.account_ids = account_ids
    account_ids = bb.Attribute("account_ids")
        super(GetAccountBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
GetAccountBatchArg_validator = bv.Struct(GetAccountBatchArg)
class GetAccountBatchError(bb.Union):
    :ivar str users.GetAccountBatchError.no_account: The value is an account ID
        specified in :field:`GetAccountBatchArg.account_ids` that does not
        exist.
    def no_account(cls, val):
        Create an instance of this class set to the ``no_account`` tag with
        :rtype: GetAccountBatchError
        return cls('no_account', val)
    def is_no_account(self):
        Check if the union tag is ``no_account``.
        return self._tag == 'no_account'
    def get_no_account(self):
        The value is an account ID specified in
        ``GetAccountBatchArg.account_ids`` that does not exist.
        Only call this if :meth:`is_no_account` is true.
        if not self.is_no_account():
            raise AttributeError("tag 'no_account' not set")
        super(GetAccountBatchError, self)._process_custom_annotations(annotation_type, field_path, processor)
GetAccountBatchError_validator = bv.Union(GetAccountBatchError)
class GetAccountError(bb.Union):
    :ivar users.GetAccountError.no_account: The specified
        ``GetAccountArg.account_id`` does not exist.
    no_account = None
        super(GetAccountError, self)._process_custom_annotations(annotation_type, field_path, processor)
GetAccountError_validator = bv.Union(GetAccountError)
class IndividualSpaceAllocation(bb.Struct):
    :ivar users.IndividualSpaceAllocation.allocated: The total space allocated
        to the user's account (bytes).
        '_allocated_value',
                 allocated=None):
        self._allocated_value = bb.NOT_SET
        if allocated is not None:
            self.allocated = allocated
    allocated = bb.Attribute("allocated")
        super(IndividualSpaceAllocation, self)._process_custom_annotations(annotation_type, field_path, processor)
IndividualSpaceAllocation_validator = bv.Struct(IndividualSpaceAllocation)
class Name(bb.Struct):
    Representations for a person's name to assist with internationalization.
    :ivar users.Name.given_name: Also known as a first name.
    :ivar users.Name.surname: Also known as a last name or family name.
    :ivar users.Name.familiar_name: Locale-dependent name. In the US, a person's
        familiar name is their ``given_name``, but elsewhere, it could be any
        combination of a person's ``given_name`` and ``surname``.
    :ivar users.Name.display_name: A name that can be used directly to represent
        the name of a user's Dropbox account.
    :ivar users.Name.abbreviated_name: An abbreviated form of the person's name.
        Their initials in most locales.
        '_familiar_name_value',
        '_abbreviated_name_value',
                 familiar_name=None,
                 abbreviated_name=None):
        self._familiar_name_value = bb.NOT_SET
        self._abbreviated_name_value = bb.NOT_SET
        if familiar_name is not None:
            self.familiar_name = familiar_name
        if abbreviated_name is not None:
            self.abbreviated_name = abbreviated_name
    familiar_name = bb.Attribute("familiar_name")
    abbreviated_name = bb.Attribute("abbreviated_name")
        super(Name, self)._process_custom_annotations(annotation_type, field_path, processor)
Name_validator = bv.Struct(Name)
class PaperAsFilesValue(bb.Union):
    The value for ``UserFeature.paper_as_files``.
    :ivar bool users.PaperAsFilesValue.enabled: When this value is true, the
        user's Paper docs are accessible in Dropbox with the .paper extension
        and must be accessed via the /files endpoints.  When this value is
        false, the user's Paper docs are stored separate from Dropbox files and
        folders and should be accessed via the /paper endpoints.
        :rtype: PaperAsFilesValue
        When this value is true, the user's Paper docs are accessible in Dropbox
        with the .paper extension and must be accessed via the /files endpoints.
        When this value is false, the user's Paper docs are stored separate from
        Dropbox files and folders and should be accessed via the /paper
        endpoints.
        super(PaperAsFilesValue, self)._process_custom_annotations(annotation_type, field_path, processor)
PaperAsFilesValue_validator = bv.Union(PaperAsFilesValue)
class SpaceAllocation(bb.Union):
    Space is allocated differently based on the type of account.
    :ivar IndividualSpaceAllocation SpaceAllocation.individual: The user's space
        allocation applies only to their individual account.
    :ivar TeamSpaceAllocation SpaceAllocation.team: The user shares space with
        other members of their team.
    def individual(cls, val):
        Create an instance of this class set to the ``individual`` tag with
        :param IndividualSpaceAllocation val:
        :rtype: SpaceAllocation
        return cls('individual', val)
        :param TeamSpaceAllocation val:
    def is_individual(self):
        Check if the union tag is ``individual``.
        return self._tag == 'individual'
    def get_individual(self):
        The user's space allocation applies only to their individual account.
        Only call this if :meth:`is_individual` is true.
        :rtype: IndividualSpaceAllocation
        if not self.is_individual():
            raise AttributeError("tag 'individual' not set")
        The user shares space with other members of their team.
        :rtype: TeamSpaceAllocation
        super(SpaceAllocation, self)._process_custom_annotations(annotation_type, field_path, processor)
SpaceAllocation_validator = bv.Union(SpaceAllocation)
class SpaceUsage(bb.Struct):
    Information about a user's space usage and quota.
    :ivar users.SpaceUsage.used: The user's total space usage (bytes).
    :ivar users.SpaceUsage.allocation: The user's space allocation.
        '_used_value',
        '_allocation_value',
                 used=None,
                 allocation=None):
        self._used_value = bb.NOT_SET
        self._allocation_value = bb.NOT_SET
        if used is not None:
            self.used = used
        if allocation is not None:
            self.allocation = allocation
    used = bb.Attribute("used")
    # Instance attribute type: SpaceAllocation (validator is set below)
    allocation = bb.Attribute("allocation", user_defined=True)
        super(SpaceUsage, self)._process_custom_annotations(annotation_type, field_path, processor)
SpaceUsage_validator = bv.Struct(SpaceUsage)
class TeamSpaceAllocation(bb.Struct):
    :ivar users.TeamSpaceAllocation.used: The total space currently used by the
        user's team (bytes).
    :ivar users.TeamSpaceAllocation.allocated: The total space allocated to the
    :ivar users.TeamSpaceAllocation.user_within_team_space_allocated: The total
        space allocated to the user within its team allocated space (0 means
        that no restriction is imposed on the user's quota within its team).
    :ivar users.TeamSpaceAllocation.user_within_team_space_limit_type: The type
        of the space limit imposed on the team member (off, alert_only,
        stop_sync).
    :ivar users.TeamSpaceAllocation.user_within_team_space_used_cached: An
        accurate cached calculation of a team member's total space usage
        (bytes).
        '_user_within_team_space_allocated_value',
        '_user_within_team_space_limit_type_value',
        '_user_within_team_space_used_cached_value',
                 allocated=None,
                 user_within_team_space_allocated=None,
                 user_within_team_space_limit_type=None,
                 user_within_team_space_used_cached=None):
        self._user_within_team_space_allocated_value = bb.NOT_SET
        self._user_within_team_space_limit_type_value = bb.NOT_SET
        self._user_within_team_space_used_cached_value = bb.NOT_SET
        if user_within_team_space_allocated is not None:
            self.user_within_team_space_allocated = user_within_team_space_allocated
        if user_within_team_space_limit_type is not None:
            self.user_within_team_space_limit_type = user_within_team_space_limit_type
        if user_within_team_space_used_cached is not None:
            self.user_within_team_space_used_cached = user_within_team_space_used_cached
    user_within_team_space_allocated = bb.Attribute("user_within_team_space_allocated")
    # Instance attribute type: team_common.MemberSpaceLimitType (validator is set below)
    user_within_team_space_limit_type = bb.Attribute("user_within_team_space_limit_type", user_defined=True)
    user_within_team_space_used_cached = bb.Attribute("user_within_team_space_used_cached")
        super(TeamSpaceAllocation, self)._process_custom_annotations(annotation_type, field_path, processor)
TeamSpaceAllocation_validator = bv.Struct(TeamSpaceAllocation)
class UserFeature(bb.Union):
    A set of features that a Dropbox User account may have configured.
    :ivar users.UserFeature.paper_as_files: This feature contains information
        about how the user's Paper files are stored.
    :ivar users.UserFeature.file_locking: This feature allows users to lock
        files in order to restrict other users from editing them.
    paper_as_files = None
    file_locking = None
    def is_paper_as_files(self):
        Check if the union tag is ``paper_as_files``.
        return self._tag == 'paper_as_files'
    def is_file_locking(self):
        Check if the union tag is ``file_locking``.
        return self._tag == 'file_locking'
        super(UserFeature, self)._process_custom_annotations(annotation_type, field_path, processor)
UserFeature_validator = bv.Union(UserFeature)
class UserFeatureValue(bb.Union):
    Values that correspond to entries in :class:`UserFeature`.
    def paper_as_files(cls, val):
        Create an instance of this class set to the ``paper_as_files`` tag with
        :param PaperAsFilesValue val:
        :rtype: UserFeatureValue
        return cls('paper_as_files', val)
    def file_locking(cls, val):
        Create an instance of this class set to the ``file_locking`` tag with
        :param FileLockingValue val:
        return cls('file_locking', val)
    def get_paper_as_files(self):
        Only call this if :meth:`is_paper_as_files` is true.
        if not self.is_paper_as_files():
            raise AttributeError("tag 'paper_as_files' not set")
    def get_file_locking(self):
        Only call this if :meth:`is_file_locking` is true.
        if not self.is_file_locking():
            raise AttributeError("tag 'file_locking' not set")
        super(UserFeatureValue, self)._process_custom_annotations(annotation_type, field_path, processor)
UserFeatureValue_validator = bv.Union(UserFeatureValue)
class UserFeaturesGetValuesBatchArg(bb.Struct):
    :ivar users.UserFeaturesGetValuesBatchArg.features: A list of features in
        :class:`UserFeature`. If the list is empty, this route will return
        :class:`UserFeaturesGetValuesBatchError`.
    # Instance attribute type: list of [UserFeature] (validator is set below)
        super(UserFeaturesGetValuesBatchArg, self)._process_custom_annotations(annotation_type, field_path, processor)
UserFeaturesGetValuesBatchArg_validator = bv.Struct(UserFeaturesGetValuesBatchArg)
class UserFeaturesGetValuesBatchError(bb.Union):
    :ivar users.UserFeaturesGetValuesBatchError.empty_features_list: At least
        one :class:`UserFeature` must be included in the
        :class:`UserFeaturesGetValuesBatchArg`.features list.
        super(UserFeaturesGetValuesBatchError, self)._process_custom_annotations(annotation_type, field_path, processor)
UserFeaturesGetValuesBatchError_validator = bv.Union(UserFeaturesGetValuesBatchError)
class UserFeaturesGetValuesBatchResult(bb.Struct):
    # Instance attribute type: list of [UserFeatureValue] (validator is set below)
        super(UserFeaturesGetValuesBatchResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UserFeaturesGetValuesBatchResult_validator = bv.Struct(UserFeaturesGetValuesBatchResult)
GetAccountBatchResult_validator = bv.List(BasicAccount_validator)
Account.account_id.validator = users_common.AccountId_validator
Account.name.validator = Name_validator
Account.email.validator = bv.String()
Account.email_verified.validator = bv.Boolean()
Account.profile_photo_url.validator = bv.Nullable(bv.String())
Account.disabled.validator = bv.Boolean()
Account._all_field_names_ = set([
    'disabled',
Account._all_fields_ = [
    ('account_id', Account.account_id.validator),
    ('name', Account.name.validator),
    ('email', Account.email.validator),
    ('email_verified', Account.email_verified.validator),
    ('profile_photo_url', Account.profile_photo_url.validator),
    ('disabled', Account.disabled.validator),
BasicAccount.is_teammate.validator = bv.Boolean()
BasicAccount.team_member_id.validator = bv.Nullable(bv.String())
BasicAccount._all_field_names_ = Account._all_field_names_.union(set([
    'is_teammate',
BasicAccount._all_fields_ = Account._all_fields_ + [
    ('is_teammate', BasicAccount.is_teammate.validator),
    ('team_member_id', BasicAccount.team_member_id.validator),
FileLockingValue._enabled_validator = bv.Boolean()
FileLockingValue._other_validator = bv.Void()
FileLockingValue._tagmap = {
    'enabled': FileLockingValue._enabled_validator,
    'other': FileLockingValue._other_validator,
FileLockingValue.other = FileLockingValue('other')
FullAccount.country.validator = bv.Nullable(bv.String(min_length=2, max_length=2))
FullAccount.locale.validator = bv.String(min_length=2)
FullAccount.referral_link.validator = bv.String()
FullAccount.team.validator = bv.Nullable(FullTeam_validator)
FullAccount.team_member_id.validator = bv.Nullable(bv.String())
FullAccount.is_paired.validator = bv.Boolean()
FullAccount.account_type.validator = users_common.AccountType_validator
FullAccount.root_info.validator = common.RootInfo_validator
FullAccount._all_field_names_ = Account._all_field_names_.union(set([
    'referral_link',
    'is_paired',
    'account_type',
    'root_info',
FullAccount._all_fields_ = Account._all_fields_ + [
    ('country', FullAccount.country.validator),
    ('locale', FullAccount.locale.validator),
    ('referral_link', FullAccount.referral_link.validator),
    ('team', FullAccount.team.validator),
    ('team_member_id', FullAccount.team_member_id.validator),
    ('is_paired', FullAccount.is_paired.validator),
    ('account_type', FullAccount.account_type.validator),
    ('root_info', FullAccount.root_info.validator),
Team.id.validator = bv.String()
Team.name.validator = bv.String()
Team._all_field_names_ = set([
Team._all_fields_ = [
    ('id', Team.id.validator),
    ('name', Team.name.validator),
FullTeam.sharing_policies.validator = team_policies.TeamSharingPolicies_validator
FullTeam.office_addin_policy.validator = team_policies.OfficeAddInPolicy_validator
FullTeam._all_field_names_ = Team._all_field_names_.union(set([
    'sharing_policies',
    'office_addin_policy',
FullTeam._all_fields_ = Team._all_fields_ + [
    ('sharing_policies', FullTeam.sharing_policies.validator),
    ('office_addin_policy', FullTeam.office_addin_policy.validator),
GetAccountArg.account_id.validator = users_common.AccountId_validator
GetAccountArg._all_field_names_ = set(['account_id'])
GetAccountArg._all_fields_ = [('account_id', GetAccountArg.account_id.validator)]
GetAccountBatchArg.account_ids.validator = bv.List(users_common.AccountId_validator, min_items=1)
GetAccountBatchArg._all_field_names_ = set(['account_ids'])
GetAccountBatchArg._all_fields_ = [('account_ids', GetAccountBatchArg.account_ids.validator)]
GetAccountBatchError._no_account_validator = users_common.AccountId_validator
GetAccountBatchError._other_validator = bv.Void()
GetAccountBatchError._tagmap = {
    'no_account': GetAccountBatchError._no_account_validator,
    'other': GetAccountBatchError._other_validator,
GetAccountBatchError.other = GetAccountBatchError('other')
GetAccountError._no_account_validator = bv.Void()
GetAccountError._other_validator = bv.Void()
GetAccountError._tagmap = {
    'no_account': GetAccountError._no_account_validator,
    'other': GetAccountError._other_validator,
GetAccountError.no_account = GetAccountError('no_account')
GetAccountError.other = GetAccountError('other')
IndividualSpaceAllocation.allocated.validator = bv.UInt64()
IndividualSpaceAllocation._all_field_names_ = set(['allocated'])
IndividualSpaceAllocation._all_fields_ = [('allocated', IndividualSpaceAllocation.allocated.validator)]
Name.given_name.validator = bv.String()
Name.surname.validator = bv.String()
Name.familiar_name.validator = bv.String()
Name.display_name.validator = bv.String()
Name.abbreviated_name.validator = bv.String()
Name._all_field_names_ = set([
    'familiar_name',
    'abbreviated_name',
Name._all_fields_ = [
    ('given_name', Name.given_name.validator),
    ('surname', Name.surname.validator),
    ('familiar_name', Name.familiar_name.validator),
    ('display_name', Name.display_name.validator),
    ('abbreviated_name', Name.abbreviated_name.validator),
PaperAsFilesValue._enabled_validator = bv.Boolean()
PaperAsFilesValue._other_validator = bv.Void()
PaperAsFilesValue._tagmap = {
    'enabled': PaperAsFilesValue._enabled_validator,
    'other': PaperAsFilesValue._other_validator,
PaperAsFilesValue.other = PaperAsFilesValue('other')
SpaceAllocation._individual_validator = IndividualSpaceAllocation_validator
SpaceAllocation._team_validator = TeamSpaceAllocation_validator
SpaceAllocation._other_validator = bv.Void()
SpaceAllocation._tagmap = {
    'individual': SpaceAllocation._individual_validator,
    'team': SpaceAllocation._team_validator,
    'other': SpaceAllocation._other_validator,
SpaceAllocation.other = SpaceAllocation('other')
SpaceUsage.used.validator = bv.UInt64()
SpaceUsage.allocation.validator = SpaceAllocation_validator
SpaceUsage._all_field_names_ = set([
    'used',
    'allocation',
SpaceUsage._all_fields_ = [
    ('used', SpaceUsage.used.validator),
    ('allocation', SpaceUsage.allocation.validator),
TeamSpaceAllocation.used.validator = bv.UInt64()
TeamSpaceAllocation.allocated.validator = bv.UInt64()
TeamSpaceAllocation.user_within_team_space_allocated.validator = bv.UInt64()
TeamSpaceAllocation.user_within_team_space_limit_type.validator = team_common.MemberSpaceLimitType_validator
TeamSpaceAllocation.user_within_team_space_used_cached.validator = bv.UInt64()
TeamSpaceAllocation._all_field_names_ = set([
    'allocated',
    'user_within_team_space_allocated',
    'user_within_team_space_limit_type',
    'user_within_team_space_used_cached',
TeamSpaceAllocation._all_fields_ = [
    ('used', TeamSpaceAllocation.used.validator),
    ('allocated', TeamSpaceAllocation.allocated.validator),
    ('user_within_team_space_allocated', TeamSpaceAllocation.user_within_team_space_allocated.validator),
    ('user_within_team_space_limit_type', TeamSpaceAllocation.user_within_team_space_limit_type.validator),
    ('user_within_team_space_used_cached', TeamSpaceAllocation.user_within_team_space_used_cached.validator),
UserFeature._paper_as_files_validator = bv.Void()
UserFeature._file_locking_validator = bv.Void()
UserFeature._other_validator = bv.Void()
UserFeature._tagmap = {
    'paper_as_files': UserFeature._paper_as_files_validator,
    'file_locking': UserFeature._file_locking_validator,
    'other': UserFeature._other_validator,
UserFeature.paper_as_files = UserFeature('paper_as_files')
UserFeature.file_locking = UserFeature('file_locking')
UserFeature.other = UserFeature('other')
UserFeatureValue._paper_as_files_validator = PaperAsFilesValue_validator
UserFeatureValue._file_locking_validator = FileLockingValue_validator
UserFeatureValue._other_validator = bv.Void()
UserFeatureValue._tagmap = {
    'paper_as_files': UserFeatureValue._paper_as_files_validator,
    'file_locking': UserFeatureValue._file_locking_validator,
    'other': UserFeatureValue._other_validator,
UserFeatureValue.other = UserFeatureValue('other')
UserFeaturesGetValuesBatchArg.features.validator = bv.List(UserFeature_validator)
UserFeaturesGetValuesBatchArg._all_field_names_ = set(['features'])
UserFeaturesGetValuesBatchArg._all_fields_ = [('features', UserFeaturesGetValuesBatchArg.features.validator)]
UserFeaturesGetValuesBatchError._empty_features_list_validator = bv.Void()
UserFeaturesGetValuesBatchError._other_validator = bv.Void()
UserFeaturesGetValuesBatchError._tagmap = {
    'empty_features_list': UserFeaturesGetValuesBatchError._empty_features_list_validator,
    'other': UserFeaturesGetValuesBatchError._other_validator,
UserFeaturesGetValuesBatchError.empty_features_list = UserFeaturesGetValuesBatchError('empty_features_list')
UserFeaturesGetValuesBatchError.other = UserFeaturesGetValuesBatchError('other')
UserFeaturesGetValuesBatchResult.values.validator = bv.List(UserFeatureValue_validator)
UserFeaturesGetValuesBatchResult._all_field_names_ = set(['values'])
UserFeaturesGetValuesBatchResult._all_fields_ = [('values', UserFeaturesGetValuesBatchResult.values.validator)]
    UserFeaturesGetValuesBatchArg_validator,
    UserFeaturesGetValuesBatchResult_validator,
    UserFeaturesGetValuesBatchError_validator,
get_account = bb.Route(
    'get_account',
    GetAccountArg_validator,
    BasicAccount_validator,
    GetAccountError_validator,
get_account_batch = bb.Route(
    'get_account_batch',
    GetAccountBatchArg_validator,
    GetAccountBatchResult_validator,
    GetAccountBatchError_validator,
get_current_account = bb.Route(
    'get_current_account',
    FullAccount_validator,
get_space_usage = bb.Route(
    'get_space_usage',
    SpaceUsage_validator,
    'get_account': get_account,
    'get_account_batch': get_account_batch,
    'get_current_account': get_current_account,
    'get_space_usage': get_space_usage,
from fastapi import APIRouter
router = APIRouter()
@router.get("/users/", tags=["users"])
async def read_users():
    return [{"username": "Rick"}, {"username": "Morty"}]
@router.get("/users/me", tags=["users"])
async def read_user_me():
    return {"username": "fakecurrentuser"}
@router.get("/users/{username}", tags=["users"])
async def read_user(username: str):
    return {"username": username}
from typing import List, Dict, Any, Optional
class UsersManagementClient:
        self.base_url = base_url.rstrip("/")
        if self.api_key:
            headers["Authorization"] = f"Bearer {self.api_key}"
    def list_users(self, params: Optional[Dict[str, Any]] = None) -> List[Dict[str, Any]]:
        """List users (GET /user/list)"""
        url = f"{self.base_url}/user/list"
        response = requests.get(url, headers=self._get_headers(), params=params)
            raise UnauthorizedError(response.text)
        return response.json().get("users", response.json())
    def get_user(self, user_id: Optional[str] = None) -> Dict[str, Any]:
        """Get user info (GET /user/info)"""
        url = f"{self.base_url}/user/info"
        params = {"user_id": user_id} if user_id else {}
            raise NotFoundError(response.text)
    def create_user(self, user_data: Dict[str, Any]) -> Dict[str, Any]:
        """Create a new user (POST /user/new)"""
        url = f"{self.base_url}/user/new"
        response = requests.post(url, headers=self._get_headers(), json=user_data)
    def delete_user(self, user_ids: List[str]) -> Dict[str, Any]:
        """Delete users (POST /user/delete)"""
        url = f"{self.base_url}/user/delete"
        response = requests.post(url, headers=self._get_headers(), json={"user_ids": user_ids})
from ... import UsersManagementClient
    """Manage users on your LiteLLM proxy server"""
@users.command("list")
def list_users(ctx: click.Context):
    """List all users"""
    client = UsersManagementClient(base_url=ctx.obj["base_url"], api_key=ctx.obj["api_key"])
    users = client.list_users()
    if isinstance(users, dict) and "users" in users:
        users = users["users"]
    if not users:
        click.echo("No users found.")
    table = Table(title="Users")
    table.add_column("User ID", style="cyan")
    table.add_column("Email", style="green")
    table.add_column("Role", style="magenta")
    table.add_column("Teams", style="yellow")
    for user in users:
            str(user.get("user_id", "")),
            str(user.get("user_email", "")),
            str(user.get("user_role", "")),
            ", ".join(user.get("teams", []) or [])
@users.command("get")
@click.option("--id", "user_id", help="ID of the user to retrieve")
def get_user(ctx: click.Context, user_id: str):
    """Get information about a specific user"""
    result = client.get_user(user_id=user_id)
@users.command("create")
@click.option("--email", required=True, help="User email")
@click.option("--role", default="internal_user", help="User role")
@click.option("--alias", default=None, help="User alias")
@click.option("--team", multiple=True, help="Team IDs (can specify multiple)")
@click.option("--max-budget", type=float, default=None, help="Max budget for user")
def create_user(ctx: click.Context, email, role, alias, team, max_budget):
    """Create a new user"""
    user_data = {
        "user_email": email,
        "user_role": role,
    if alias:
        user_data["user_alias"] = alias
    if team:
        user_data["teams"] = list(team)
    if max_budget is not None:
        user_data["max_budget"] = max_budget
    result = client.create_user(user_data)
@users.command("delete")
@click.argument("user_ids", nargs=-1)
def delete_user(ctx: click.Context, user_ids):
    """Delete one or more users by user_id"""
    result = client.delete_user(list(user_ids))
