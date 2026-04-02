class GroupManagementType(bb.Union):
    The group type determines how a group is managed.
    :ivar team_common.GroupManagementType.user_managed: A group which is managed
        by selected users.
    :ivar team_common.GroupManagementType.company_managed: A group which is
        managed by team admins only.
    :ivar team_common.GroupManagementType.system_managed: A group which is
        managed automatically by Dropbox.
    user_managed = None
    company_managed = None
    system_managed = None
    def is_user_managed(self):
        Check if the union tag is ``user_managed``.
        return self._tag == 'user_managed'
    def is_company_managed(self):
        Check if the union tag is ``company_managed``.
        return self._tag == 'company_managed'
    def is_system_managed(self):
        Check if the union tag is ``system_managed``.
        return self._tag == 'system_managed'
        super(GroupManagementType, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupManagementType_validator = bv.Union(GroupManagementType)
class GroupSummary(bb.Struct):
    Information about a group.
    :ivar team_common.GroupSummary.group_external_id: External ID of group. This
        is an arbitrary ID that an admin can attach to a group.
    :ivar team_common.GroupSummary.member_count: The number of members in the
    :ivar team_common.GroupSummary.group_management_type: Who is allowed to
        manage the group.
        '_group_id_value',
        self._group_id_value = bb.NOT_SET
        if group_id is not None:
            self.group_id = group_id
    group_id = bb.Attribute("group_id")
    member_count = bb.Attribute("member_count", nullable=True)
    # Instance attribute type: GroupManagementType (validator is set below)
    group_management_type = bb.Attribute("group_management_type", user_defined=True)
        super(GroupSummary, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupSummary_validator = bv.Struct(GroupSummary)
class GroupType(bb.Union):
    The group type determines how a group is created and managed.
    :ivar team_common.GroupType.team: A group to which team members are
        automatically added. Applicable to `team folders
        <https://www.dropbox.com/help/986>`_ only.
    :ivar team_common.GroupType.user_managed: A group is created and managed by
        a user.
        super(GroupType, self)._process_custom_annotations(annotation_type, field_path, processor)
GroupType_validator = bv.Union(GroupType)
class MemberSpaceLimitType(bb.Union):
    The type of the space limit imposed on a team member.
    :ivar team_common.MemberSpaceLimitType.off: The team member does not have
        imposed space limit.
    :ivar team_common.MemberSpaceLimitType.alert_only: The team member has soft
        imposed space limit - the limit is used for display and for
        notifications.
    :ivar team_common.MemberSpaceLimitType.stop_sync: The team member has hard
        imposed space limit - Dropbox file sync will stop after the limit is
    off = None
    alert_only = None
    stop_sync = None
    def is_off(self):
        Check if the union tag is ``off``.
        return self._tag == 'off'
    def is_alert_only(self):
        Check if the union tag is ``alert_only``.
        return self._tag == 'alert_only'
    def is_stop_sync(self):
        Check if the union tag is ``stop_sync``.
        return self._tag == 'stop_sync'
        super(MemberSpaceLimitType, self)._process_custom_annotations(annotation_type, field_path, processor)
MemberSpaceLimitType_validator = bv.Union(MemberSpaceLimitType)
class TimeRange(bb.Struct):
    Time range.
    :ivar team_common.TimeRange.start_time: Optional starting time (inclusive).
    :ivar team_common.TimeRange.end_time: Optional ending time (exclusive).
        '_start_time_value',
        '_end_time_value',
                 start_time=None,
                 end_time=None):
        self._start_time_value = bb.NOT_SET
        self._end_time_value = bb.NOT_SET
        if start_time is not None:
            self.start_time = start_time
        if end_time is not None:
            self.end_time = end_time
    start_time = bb.Attribute("start_time", nullable=True)
    end_time = bb.Attribute("end_time", nullable=True)
        super(TimeRange, self)._process_custom_annotations(annotation_type, field_path, processor)
TimeRange_validator = bv.Struct(TimeRange)
GroupExternalId_validator = bv.String()
GroupId_validator = bv.String()
MemberExternalId_validator = bv.String(max_length=64)
ResellerId_validator = bv.String()
TeamId_validator = bv.String()
TeamMemberId_validator = bv.String()
GroupManagementType._user_managed_validator = bv.Void()
GroupManagementType._company_managed_validator = bv.Void()
GroupManagementType._system_managed_validator = bv.Void()
GroupManagementType._other_validator = bv.Void()
GroupManagementType._tagmap = {
    'user_managed': GroupManagementType._user_managed_validator,
    'company_managed': GroupManagementType._company_managed_validator,
    'system_managed': GroupManagementType._system_managed_validator,
    'other': GroupManagementType._other_validator,
GroupManagementType.user_managed = GroupManagementType('user_managed')
GroupManagementType.company_managed = GroupManagementType('company_managed')
GroupManagementType.system_managed = GroupManagementType('system_managed')
GroupManagementType.other = GroupManagementType('other')
GroupSummary.group_name.validator = bv.String()
GroupSummary.group_id.validator = GroupId_validator
GroupSummary.group_external_id.validator = bv.Nullable(GroupExternalId_validator)
GroupSummary.member_count.validator = bv.Nullable(bv.UInt32())
GroupSummary.group_management_type.validator = GroupManagementType_validator
GroupSummary._all_field_names_ = set([
    'group_id',
GroupSummary._all_fields_ = [
    ('group_name', GroupSummary.group_name.validator),
    ('group_id', GroupSummary.group_id.validator),
    ('group_external_id', GroupSummary.group_external_id.validator),
    ('member_count', GroupSummary.member_count.validator),
    ('group_management_type', GroupSummary.group_management_type.validator),
GroupType._team_validator = bv.Void()
GroupType._user_managed_validator = bv.Void()
GroupType._other_validator = bv.Void()
GroupType._tagmap = {
    'team': GroupType._team_validator,
    'user_managed': GroupType._user_managed_validator,
    'other': GroupType._other_validator,
GroupType.team = GroupType('team')
GroupType.user_managed = GroupType('user_managed')
GroupType.other = GroupType('other')
MemberSpaceLimitType._off_validator = bv.Void()
MemberSpaceLimitType._alert_only_validator = bv.Void()
MemberSpaceLimitType._stop_sync_validator = bv.Void()
MemberSpaceLimitType._other_validator = bv.Void()
MemberSpaceLimitType._tagmap = {
    'off': MemberSpaceLimitType._off_validator,
    'alert_only': MemberSpaceLimitType._alert_only_validator,
    'stop_sync': MemberSpaceLimitType._stop_sync_validator,
    'other': MemberSpaceLimitType._other_validator,
MemberSpaceLimitType.off = MemberSpaceLimitType('off')
MemberSpaceLimitType.alert_only = MemberSpaceLimitType('alert_only')
MemberSpaceLimitType.stop_sync = MemberSpaceLimitType('stop_sync')
MemberSpaceLimitType.other = MemberSpaceLimitType('other')
TimeRange.start_time.validator = bv.Nullable(common.DropboxTimestamp_validator)
TimeRange.end_time.validator = bv.Nullable(common.DropboxTimestamp_validator)
TimeRange._all_field_names_ = set([
    'start_time',
    'end_time',
TimeRange._all_fields_ = [
    ('start_time', TimeRange.start_time.validator),
    ('end_time', TimeRange.end_time.validator),
