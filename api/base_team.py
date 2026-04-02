class DropboxTeamBase(object):
    def file_properties_templates_add_for_team(self,
        Add a template associated with a team. See
        :meth:`file_properties_properties_add` to add properties to a file or
        folder. Note: this endpoint will create team-owned templates.
            scope: files.team_metadata.write
            file_properties.templates_add_for_team,
    def file_properties_templates_get_for_team(self,
        Get the schema for a specified template.
            file_properties.templates_get_for_team,
    def file_properties_templates_list_for_team(self):
        template use :meth:`file_properties_templates_get_for_team`.
            file_properties.templates_list_for_team,
    def file_properties_templates_remove_for_team(self,
            file_properties.templates_remove_for_team,
    def file_properties_templates_update_for_team(self,
        Update a template associated with a team. This route can update the
        templates.
            file_properties.templates_update_for_team,
    def team_devices_list_member_devices(self,
                                         team_member_id,
                                         include_web_sessions=True,
                                         include_desktop_clients=True,
                                         include_mobile_clients=True):
        List all device sessions of a team's member.
            scope: sessions.list
        :param str team_member_id: The team's member id.
        :param bool include_web_sessions: Whether to list web sessions of the
            team's member.
        :param bool include_desktop_clients: Whether to list linked desktop
            devices of the team's member.
        :param bool include_mobile_clients: Whether to list linked mobile
        :rtype: :class:`dropbox.team.ListMemberDevicesResult`
            :class:`dropbox.team.ListMemberDevicesError`
        arg = team.ListMemberDevicesArg(team_member_id,
                                        include_web_sessions,
                                        include_desktop_clients,
                                        include_mobile_clients)
            team.devices_list_member_devices,
            'team',
    def team_devices_list_members_devices(self,
        List all device sessions of a team. Permission : Team member file
        :param Nullable[str] cursor: At the first call to the
            :meth:`team_devices_list_members_devices` the cursor shouldn't be
            passed. Then, if the result of the call includes a cursor, the
            following requests should include the received cursors in order to
            receive the next sub list of team devices.
            team members.
        :param bool include_desktop_clients: Whether to list desktop clients of
            the team members.
        :param bool include_mobile_clients: Whether to list mobile clients of
        :rtype: :class:`dropbox.team.ListMembersDevicesResult`
            :class:`dropbox.team.ListMembersDevicesError`
        arg = team.ListMembersDevicesArg(cursor,
            team.devices_list_members_devices,
    def team_devices_list_team_devices(self,
            :meth:`team_devices_list_team_devices` the cursor shouldn't be
        :rtype: :class:`dropbox.team.ListTeamDevicesResult`
            :class:`dropbox.team.ListTeamDevicesError`
            'devices/list_team_devices is deprecated. Use devices/list_members_devices.',
        arg = team.ListTeamDevicesArg(cursor,
            team.devices_list_team_devices,
    def team_devices_revoke_device_session(self,
                                           arg):
        Revoke a device session of a team's member.
            scope: sessions.modify
        :type arg: :class:`dropbox.team.RevokeDeviceSessionArg`
            :class:`dropbox.team.RevokeDeviceSessionError`
            team.devices_revoke_device_session,
    def team_devices_revoke_device_session_batch(self,
                                                 revoke_devices):
        Revoke a list of device sessions of team members.
        :type revoke_devices: List[:class:`dropbox.team.RevokeDeviceSessionArg`]
        :rtype: :class:`dropbox.team.RevokeDeviceSessionBatchResult`
            :class:`dropbox.team.RevokeDeviceSessionBatchError`
        arg = team.RevokeDeviceSessionBatchArg(revoke_devices)
            team.devices_revoke_device_session_batch,
    def team_features_get_values(self,
        Get the values for one or more featues. This route allows you to check
        your account's capability for what feature you can access or what value
        you have for certain features. Permission : Team information.
            scope: team_info.read
        :param List[:class:`dropbox.team.Feature`] features: A list of features
            in :class:`dropbox.team.Feature`. If the list is empty, this route
            will return :class:`dropbox.team.FeaturesGetValuesBatchError`.
        :rtype: :class:`dropbox.team.FeaturesGetValuesBatchResult`
            :class:`dropbox.team.FeaturesGetValuesBatchError`
        arg = team.FeaturesGetValuesBatchArg(features)
            team.features_get_values,
    def team_get_info(self):
        Retrieves information about a team.
        :rtype: :class:`dropbox.team.TeamGetInfoResult`
            team.get_info,
    def team_groups_create(self,
                           group_name,
                           add_creator_as_owner=False,
                           group_external_id=None,
                           group_management_type=None):
        Creates a new, empty group, with a requested name. Permission : Team
        member management.
            scope: groups.write
        :param str group_name: Group name.
        :param bool add_creator_as_owner: Automatically add the creator of the
        :param Nullable[str] group_external_id: The creator of a team can
            associate an arbitrary external ID to the group.
        :param Nullable[:class:`dropbox.team.GroupManagementType`]
            group_management_type: Whether the team can be managed by selected
            users, or only by team admins.
        :rtype: :class:`dropbox.team.GroupFullInfo`
            :class:`dropbox.team.GroupCreateError`
        arg = team.GroupCreateArg(group_name,
                                  add_creator_as_owner,
                                  group_external_id,
                                  group_management_type)
            team.groups_create,
    def team_groups_delete(self,
        Deletes a group. The group is deleted immediately. However the revoking
        of group-owned resources may take additional time. Use the
        :meth:`team_groups_job_status_get` to determine whether this process has
        completed. Permission : Team member management.
        :param arg: Argument for selecting a single group, either by group_id or
            by external group ID.
        :type arg: :class:`dropbox.team.GroupSelector`
        :rtype: :class:`dropbox.team.LaunchEmptyResult`
            :class:`dropbox.team.GroupDeleteError`
            team.groups_delete,
    def team_groups_get_info(self,
        Retrieves information about one or more groups. Note that the optional
        field  ``GroupFullInfo.members`` is not returned for system-managed
        groups. Permission : Team Information.
            scope: groups.read
        :param arg: Argument for selecting a list of groups, either by
            group_ids, or external group IDs.
        :type arg: :class:`dropbox.team.GroupsSelector`
        :rtype: List[:class:`dropbox.team.GroupsGetInfoItem`]
            :class:`dropbox.team.GroupsGetInfoError`
            team.groups_get_info,
    def team_groups_job_status_get(self,
        Once an async_job_id is returned from :meth:`team_groups_delete`,
        :meth:`team_groups_members_add` , or :meth:`team_groups_members_remove`
        use this method to poll the status of granting/revoking group members'
        access to group-owned resources. Permission : Team member management.
        :rtype: :class:`dropbox.team.PollEmptyResult`
            :class:`dropbox.team.GroupsPollError`
            team.groups_job_status_get,
    def team_groups_list(self,
        Lists groups on a team. Permission : Team Information.
        :param int limit: Number of results to return per call.
        :rtype: :class:`dropbox.team.GroupsListResult`
        arg = team.GroupsListArg(limit)
            team.groups_list,
    def team_groups_list_continue(self,
        Once a cursor has been retrieved from :meth:`team_groups_list`, use this
        to paginate through all groups. Permission : Team Information.
        :param str cursor: Indicates from what point to get the next set of
            groups.
            :class:`dropbox.team.GroupsListContinueError`
        arg = team.GroupsListContinueArg(cursor)
            team.groups_list_continue,
    def team_groups_members_add(self,
                                return_members=True):
        Adds members to a group. The members are added immediately. However the
        granting of group-owned resources may take additional time. Use the
        :param group: Group to which users will be added.
        :type group: :class:`dropbox.team.GroupSelector`
        :param List[:class:`dropbox.team.MemberAccess`] members: List of users
            to be added to the group.
        :rtype: :class:`dropbox.team.GroupMembersChangeResult`
            :class:`dropbox.team.GroupMembersAddError`
        arg = team.GroupMembersAddArg(group,
                                      return_members)
            team.groups_members_add,
    def team_groups_members_list(self,
        Lists members of a group. Permission : Team Information.
        :param group: The group whose members are to be listed.
        :rtype: :class:`dropbox.team.GroupsMembersListResult`
            :class:`dropbox.team.GroupSelectorError`
        arg = team.GroupsMembersListArg(group,
            team.groups_members_list,
    def team_groups_members_list_continue(self,
        Once a cursor has been retrieved from :meth:`team_groups_members_list`,
        use this to paginate through all members of the group. Permission : Team
            :class:`dropbox.team.GroupsMembersListContinueError`
        arg = team.GroupsMembersListContinueArg(cursor)
            team.groups_members_list_continue,
    def team_groups_members_remove(self,
                                   users,
        Removes members from a group. The members are removed immediately.
        However the revoking of group-owned resources may take additional time.
        Use the :meth:`team_groups_job_status_get` to determine whether this
        process has completed. This method permits removing the only owner of a
        group, even in cases where this is not possible via the web client.
        Permission : Team member management.
        :param group: Group from which users will be removed.
        :param List[:class:`dropbox.team.UserSelectorArg`] users: List of users
            to be removed from the group.
            :class:`dropbox.team.GroupMembersRemoveError`
        arg = team.GroupMembersRemoveArg(group,
            team.groups_members_remove,
    def team_groups_members_set_access_type(self,
                                            access_type,
        Sets a member's access type in a group. Permission : Team member
        management.
        :param access_type: New group access type the user will have.
        :type access_type: :class:`dropbox.team.GroupAccessType`
        :param bool return_members: Whether to return the list of members in the
            group.  Note that the default value will cause all the group members
            to be returned in the response. This may take a long time for large
            :class:`dropbox.team.GroupMemberSetAccessTypeError`
        arg = team.GroupMembersSetAccessTypeArg(group,
            team.groups_members_set_access_type,
    def team_groups_update(self,
                           return_members=True,
                           new_group_name=None,
                           new_group_external_id=None,
                           new_group_management_type=None):
        Updates a group's name and/or external ID. Permission : Team member
        :param group: Specify a group.
        :param Nullable[str] new_group_name: Optional argument. Set group name
            to this if provided.
        :param Nullable[str] new_group_external_id: Optional argument. New group
            external ID. If the argument is None, the group's external_id won't
            be updated. If the argument is empty string, the group's external id
            will be cleared.
            new_group_management_type: Set new group management type, if
            provided.
            :class:`dropbox.team.GroupUpdateError`
        arg = team.GroupUpdateArgs(group,
                                   return_members,
                                   new_group_name,
                                   new_group_external_id,
                                   new_group_management_type)
            team.groups_update,
    def team_legal_holds_create_policy(self,
                                       start_date=None,
                                       end_date=None):
        Creates new legal hold policy. Note: Legal Holds is a paid add-on. Not
        all teams have the feature. Permission : Team member file access.
            scope: team_data.governance.write
        :param str name: Policy name.
        :param Nullable[str] description: A description of the legal hold
            policy.
        :param List[str] members: List of team member IDs added to the hold.
        :param Nullable[datetime] start_date: start date of the legal hold
        :param Nullable[datetime] end_date: end date of the legal hold policy.
        :rtype: :class:`dropbox.team.LegalHoldPolicy`
            :class:`dropbox.team.LegalHoldsPolicyCreateError`
        arg = team.LegalHoldsPolicyCreateArg(name,
                                             start_date,
                                             end_date)
            team.legal_holds_create_policy,
    def team_legal_holds_get_policy(self,
        Gets a legal hold by Id. Note: Legal Holds is a paid add-on. Not all
        teams have the feature. Permission : Team member file access.
        :param str id: The legal hold Id.
            :class:`dropbox.team.LegalHoldsGetPolicyError`
        arg = team.LegalHoldsGetPolicyArg(id)
            team.legal_holds_get_policy,
    def team_legal_holds_list_held_revisions(self,
        List the file metadata that's under the hold. Note: Legal Holds is a
        paid add-on. Not all teams have the feature. Permission : Team member
        file access.
        :rtype: :class:`dropbox.team.LegalHoldsListHeldRevisionResult`
            :class:`dropbox.team.LegalHoldsListHeldRevisionsError`
        arg = team.LegalHoldsListHeldRevisionsArg(id)
            team.legal_holds_list_held_revisions,
    def team_legal_holds_list_held_revisions_continue(self,
                                                      cursor=None):
        Continue listing the file metadata that's under the hold. Note: Legal
        Holds is a paid add-on. Not all teams have the feature. Permission :
        Team member file access.
        :param Nullable[str] cursor: The cursor idicates where to continue
            reading file metadata entries for the next API call. When there are
            no more entries, the cursor will return none.
        arg = team.LegalHoldsListHeldRevisionsContinueArg(id,
            team.legal_holds_list_held_revisions_continue,
    def team_legal_holds_list_policies(self,
                                       include_released=False):
        Lists legal holds on a team. Note: Legal Holds is a paid add-on. Not all
        :param bool include_released: Whether to return holds that were
            released.
        :rtype: :class:`dropbox.team.LegalHoldsListPoliciesResult`
            :class:`dropbox.team.LegalHoldsListPoliciesError`
        arg = team.LegalHoldsListPoliciesArg(include_released)
            team.legal_holds_list_policies,
    def team_legal_holds_release_policy(self,
        Releases a legal hold by Id. Note: Legal Holds is a paid add-on. Not all
            :class:`dropbox.team.LegalHoldsPolicyReleaseError`
        arg = team.LegalHoldsPolicyReleaseArg(id)
            team.legal_holds_release_policy,
    def team_legal_holds_update_policy(self,
                                       members=None):
        Updates a legal hold. Note: Legal Holds is a paid add-on. Not all teams
        have the feature. Permission : Team member file access.
        :param Nullable[str] name: Policy new name.
        :param Nullable[str] description: Policy new description.
        :param Nullable[List[str]] members: List of team member IDs to apply the
            policy on.
            :class:`dropbox.team.LegalHoldsPolicyUpdateError`
        arg = team.LegalHoldsPolicyUpdateArg(id,
                                             members)
            team.legal_holds_update_policy,
    def team_linked_apps_list_member_linked_apps(self,
                                                 team_member_id):
        List all linked applications of the team member. Note, this endpoint
        does not list any team-linked applications.
        :param str team_member_id: The team member id.
        :rtype: :class:`dropbox.team.ListMemberAppsResult`
            :class:`dropbox.team.ListMemberAppsError`
        arg = team.ListMemberAppsArg(team_member_id)
            team.linked_apps_list_member_linked_apps,
    def team_linked_apps_list_members_linked_apps(self,
        List all applications linked to the team members' accounts. Note, this
        endpoint does not list any team-linked applications.
            :meth:`team_linked_apps_list_members_linked_apps` the cursor
            shouldn't be passed. Then, if the result of the call includes a
            cursor, the following requests should include the received cursors
            in order to receive the next sub list of the team applications.
        :rtype: :class:`dropbox.team.ListMembersAppsResult`
            :class:`dropbox.team.ListMembersAppsError`
        arg = team.ListMembersAppsArg(cursor)
            team.linked_apps_list_members_linked_apps,
    def team_linked_apps_list_team_linked_apps(self,
        endpoint doesn't list any team-linked applications.
            :meth:`team_linked_apps_list_team_linked_apps` the cursor shouldn't
            be passed. Then, if the result of the call includes a cursor, the
            receive the next sub list of the team applications.
        :rtype: :class:`dropbox.team.ListTeamAppsResult`
            :class:`dropbox.team.ListTeamAppsError`
            'linked_apps/list_team_linked_apps is deprecated. Use linked_apps/list_members_linked_apps.',
        arg = team.ListTeamAppsArg(cursor)
            team.linked_apps_list_team_linked_apps,
    def team_linked_apps_revoke_linked_app(self,
                                           app_id,
                                           keep_app_folder=True):
        Revoke a linked application of the team member.
        :param str app_id: The application's unique id.
        :param str team_member_id: The unique id of the member owning the
            device.
        :param bool keep_app_folder: This flag is not longer supported, the
            application dedicated folder (in case the application uses  one)
            will be kept.
            :class:`dropbox.team.RevokeLinkedAppError`
        arg = team.RevokeLinkedApiAppArg(app_id,
                                         keep_app_folder)
            team.linked_apps_revoke_linked_app,
    def team_linked_apps_revoke_linked_app_batch(self,
                                                 revoke_linked_app):
        Revoke a list of linked applications of the team members.
        :type revoke_linked_app:
        List[:class:`dropbox.team.RevokeLinkedApiAppArg`]
        :rtype: :class:`dropbox.team.RevokeLinkedAppBatchResult`
            :class:`dropbox.team.RevokeLinkedAppBatchError`
        arg = team.RevokeLinkedApiAppBatchArg(revoke_linked_app)
            team.linked_apps_revoke_linked_app_batch,
    def team_member_space_limits_excluded_users_add(self,
                                                    users=None):
        Add users to member space limits excluded users list.
            scope: members.write
        :param Nullable[List[:class:`dropbox.team.UserSelectorArg`]] users: List
            of users to be added/removed.
        :rtype: :class:`dropbox.team.ExcludedUsersUpdateResult`
            :class:`dropbox.team.ExcludedUsersUpdateError`
        arg = team.ExcludedUsersUpdateArg(users)
            team.member_space_limits_excluded_users_add,
    def team_member_space_limits_excluded_users_list(self,
        List member space limits excluded users.
            scope: members.read
        :rtype: :class:`dropbox.team.ExcludedUsersListResult`
            :class:`dropbox.team.ExcludedUsersListError`
        arg = team.ExcludedUsersListArg(limit)
            team.member_space_limits_excluded_users_list,
    def team_member_space_limits_excluded_users_list_continue(self,
        Continue listing member space limits excluded users.
            users.
            :class:`dropbox.team.ExcludedUsersListContinueError`
        arg = team.ExcludedUsersListContinueArg(cursor)
            team.member_space_limits_excluded_users_list_continue,
    def team_member_space_limits_excluded_users_remove(self,
        Remove users from member space limits excluded users list.
            team.member_space_limits_excluded_users_remove,
    def team_member_space_limits_get_custom_quota(self,
                                                  users):
        Get users custom quota. A maximum of 1000 members can be specified in a
        single call. Note: to apply a custom space limit, a team admin needs to
        set a member space limit for the team first. (the team admin can check
        the settings here: https://www.dropbox.com/team/admin/settings/space).
        :param List[:class:`dropbox.team.UserSelectorArg`] users: List of users.
        :rtype: List[:class:`dropbox.team.CustomQuotaResult`]
            :class:`dropbox.team.CustomQuotaError`
        arg = team.CustomQuotaUsersArg(users)
            team.member_space_limits_get_custom_quota,
    def team_member_space_limits_remove_custom_quota(self,
        Remove users custom quota. A maximum of 1000 members can be specified in
        a single call. Note: to apply a custom space limit, a team admin needs
        to set a member space limit for the team first. (the team admin can
        check the settings here:
        https://www.dropbox.com/team/admin/settings/space).
        :rtype: List[:class:`dropbox.team.RemoveCustomQuotaResult`]
            team.member_space_limits_remove_custom_quota,
    def team_member_space_limits_set_custom_quota(self,
                                                  users_and_quotas):
        Set users custom quota. Custom quota has to be at least 15GB. A maximum
        of 1000 members can be specified in a single call. Note: to apply a
        custom space limit, a team admin needs to set a member space limit for
        the team first. (the team admin can check the settings here:
        :param List[:class:`dropbox.team.UserCustomQuotaArg`] users_and_quotas:
            List of users and their custom quotas.
            :class:`dropbox.team.SetCustomQuotaError`
        arg = team.SetCustomQuotaArg(users_and_quotas)
            team.member_space_limits_set_custom_quota,
    def team_members_add_v2(self,
                            new_members,
        Adds members to a team. Permission : Team member management A maximum of
        20 members can be specified in a single call. If no Dropbox account
        exists with the email address specified, a new Dropbox account will be
        created with the given email address, and that account will be invited
        to the team. If a personal Dropbox account exists with the email address
        specified in the call, this call will create a placeholder Dropbox
        account for the user on the team and send an email inviting the user to
        migrate their existing personal account onto the team. Team member
        management apps are required to set an initial given_name and surname
        for a user to use in the team invitation and for 'Perform as team
        member' actions taken on the user before they become 'active'.
        :param List[:class:`dropbox.team.MemberAddV2Arg`] new_members: Details
            of new members to be added to the team.
        :rtype: :class:`dropbox.team.MembersAddLaunchV2Result`
        arg = team.MembersAddV2Arg(new_members,
            team.members_add_v2,
    def team_members_add(self,
        :param List[:class:`dropbox.team.MemberAddArg`] new_members: Details of
            new members to be added to the team.
        :rtype: :class:`dropbox.team.MembersAddLaunch`
        arg = team.MembersAddArg(new_members,
            team.members_add,
    def team_members_add_job_status_get_v2(self,
        Once an async_job_id is returned from :meth:`team_members_add_v2` , use
        this to poll the status of the asynchronous request. Permission : Team
        :rtype: :class:`dropbox.team.MembersAddJobStatusV2Result`
            :class:`dropbox.team.PollError`
            team.members_add_job_status_get_v2,
    def team_members_add_job_status_get(self,
        Once an async_job_id is returned from :meth:`team_members_add` , use
        :rtype: :class:`dropbox.team.MembersAddJobStatus`
            team.members_add_job_status_get,
    def team_members_delete_profile_photo_v2(self,
                                             user):
        Deletes a team member's profile photo. Permission : Team member
        :param user: Identity of the user whose profile photo will be deleted.
        :type user: :class:`dropbox.team.UserSelectorArg`
        :rtype: :class:`dropbox.team.TeamMemberInfoV2Result`
            :class:`dropbox.team.MembersDeleteProfilePhotoError`
        arg = team.MembersDeleteProfilePhotoArg(user)
            team.members_delete_profile_photo_v2,
    def team_members_delete_profile_photo(self,
        :rtype: :class:`dropbox.team.TeamMemberInfo`
            team.members_delete_profile_photo,
    def team_members_get_available_team_member_roles(self):
        Get available TeamMemberRoles for the connected team. To be used with
        :meth:`team_members_set_admin_permissions_v2`. Permission : Team member
        :rtype: :class:`dropbox.team.MembersGetAvailableTeamMemberRolesResult`
            team.members_get_available_team_member_roles,
    def team_members_get_info_v2(self,
                                 members):
        Returns information about multiple team members. Permission : Team
        information This endpoint will return
        ``MembersGetInfoItem.id_not_found``, for IDs (or emails) that cannot be
        matched to a valid team member.
        :param List[:class:`dropbox.team.UserSelectorArg`] members: List of team
        :rtype: :class:`dropbox.team.MembersGetInfoV2Result`
            :class:`dropbox.team.MembersGetInfoError`
        arg = team.MembersGetInfoV2Arg(members)
            team.members_get_info_v2,
    def team_members_get_info(self,
        :rtype: List[:class:`dropbox.team.MembersGetInfoItem`]
        arg = team.MembersGetInfoArgs(members)
            team.members_get_info,
    def team_members_list_v2(self,
                             include_removed=False):
        Lists members of a team. Permission : Team information.
        :param bool include_removed: Whether to return removed members.
        :rtype: :class:`dropbox.team.MembersListV2Result`
            :class:`dropbox.team.MembersListError`
        arg = team.MembersListArg(limit,
                                  include_removed)
            team.members_list_v2,
    def team_members_list(self,
        :rtype: :class:`dropbox.team.MembersListResult`
            team.members_list,
    def team_members_list_continue_v2(self,
        Once a cursor has been retrieved from :meth:`team_members_list_v2`, use
        this to paginate through all team members. Permission : Team
            :class:`dropbox.team.MembersListContinueError`
        arg = team.MembersListContinueArg(cursor)
            team.members_list_continue_v2,
    def team_members_list_continue(self,
        Once a cursor has been retrieved from :meth:`team_members_list`, use
            team.members_list_continue,
    def team_members_move_former_member_files(self,
                                              transfer_dest_id,
                                              transfer_admin_id):
        Moves removed member's files to a different member. This endpoint
        initiates an asynchronous job. To obtain the final result of the job,
        the client should periodically poll
        :meth:`team_members_move_former_member_files_job_status_check`.
        :param transfer_dest_id: Files from the deleted member account will be
            transferred to this user.
        :type transfer_dest_id: :class:`dropbox.team.UserSelectorArg`
        :param transfer_admin_id: Errors during the transfer process will be
            sent via email to this user.
        :type transfer_admin_id: :class:`dropbox.team.UserSelectorArg`
            :class:`dropbox.team.MembersTransferFormerMembersFilesError`
        arg = team.MembersDataTransferArg(user,
                                          transfer_admin_id)
            team.members_move_former_member_files,
    def team_members_move_former_member_files_job_status_check(self,
        Once an async_job_id is returned from
        :meth:`team_members_move_former_member_files` , use this to poll the
        status of the asynchronous request. Permission : Team member management.
            team.members_move_former_member_files_job_status_check,
    def team_members_recover(self,
        Recover a deleted member. Permission : Team member management Exactly
        one of team_member_id, email, or external_id must be provided to
        identify the user account.
            scope: members.delete
        :param user: Identity of user to recover.
            :class:`dropbox.team.MembersRecoverError`
        arg = team.MembersRecoverArg(user)
            team.members_recover,
    def team_members_remove(self,
                            wipe_data=True,
                            transfer_dest_id=None,
                            transfer_admin_id=None,
                            keep_account=False,
                            retain_team_shares=False):
        Removes a member from a team. Permission : Team member management
        Exactly one of team_member_id, email, or external_id must be provided to
        identify the user account. Accounts can be recovered via
        :meth:`team_members_recover` for a 7 day period or until the account has
        been permanently deleted or transferred to another account (whichever
        comes first). Calling :meth:`team_members_add` while a user is still
        recoverable on your team will return with
        ``MemberAddResult.user_already_on_team``. Accounts can have their files
        transferred via the admin console for a limited time, based on the
        version history length associated with the team (180 days for most
        teams). This endpoint may initiate an asynchronous job. To obtain the
        final result of the job, the client should periodically poll
        :meth:`team_members_remove_job_status_get`.
        :param Nullable[:class:`dropbox.team.UserSelectorArg`] transfer_dest_id:
            If provided, files from the deleted member account will be
        :param Nullable[:class:`dropbox.team.UserSelectorArg`]
            transfer_admin_id: If provided, errors during the transfer process
            will be sent via email to this user. If the transfer_dest_id
            argument was provided, then this argument must be provided as well.
        :param bool keep_account: Downgrade the member to a Basic account. The
            user will retain the email address associated with their Dropbox
            account and data in their account that is not restricted to team
            members. In order to keep the account the argument ``wipe_data``
            should be set to ``False``.
        :param bool retain_team_shares: If provided, allows removed users to
            keep access to Dropbox folders (not Dropbox Paper folders) already
            explicitly shared with them (not via a group) when they are
            downgraded to a Basic account. Users will not retain access to
            folders that do not allow external sharing. In order to keep the
            sharing relationships, the arguments ``wipe_data`` should be set to
            ``False`` and ``keep_account`` should be set to ``True``.
            :class:`dropbox.team.MembersRemoveError`
        arg = team.MembersRemoveArg(user,
                                    wipe_data,
                                    transfer_admin_id,
                                    keep_account,
                                    retain_team_shares)
            team.members_remove,
    def team_members_remove_job_status_get(self,
        Once an async_job_id is returned from :meth:`team_members_remove` , use
            team.members_remove_job_status_get,
    def team_members_secondary_emails_add(self,
                                          new_secondary_emails):
        Add secondary emails to users. Permission : Team member management.
        Emails that are on verified domains will be verified automatically. For
        each email address not on a verified domain a verification email will be
        sent.
        :param List[:class:`dropbox.team.UserSecondaryEmailsArg`]
            new_secondary_emails: List of users and secondary emails to add.
        :rtype: :class:`dropbox.team.AddSecondaryEmailsResult`
            :class:`dropbox.team.AddSecondaryEmailsError`
        arg = team.AddSecondaryEmailsArg(new_secondary_emails)
            team.members_secondary_emails_add,
    def team_members_secondary_emails_delete(self,
                                             emails_to_delete):
        Delete secondary emails from users Permission : Team member management.
        Users will be notified of deletions of verified secondary emails at both
        the secondary email and their primary email.
            emails_to_delete: List of users and their secondary emails to
            delete.
        :rtype: :class:`dropbox.team.DeleteSecondaryEmailsResult`
        arg = team.DeleteSecondaryEmailsArg(emails_to_delete)
            team.members_secondary_emails_delete,
    def team_members_secondary_emails_resend_verification_emails(self,
                                                                 emails_to_resend):
        Resend secondary email verification emails. Permission : Team member
            emails_to_resend: List of users and secondary emails to resend
            verification emails to.
        :rtype: :class:`dropbox.team.ResendVerificationEmailResult`
        arg = team.ResendVerificationEmailArg(emails_to_resend)
            team.members_secondary_emails_resend_verification_emails,
    def team_members_send_welcome_email(self,
        Sends welcome email to pending team member. Permission : Team member
        management Exactly one of team_member_id, email, or external_id must be
        provided to identify the user account. No-op if team member is not
        pending.
        :param arg: Argument for selecting a single user, either by
            team_member_id, external_id or email.
        :type arg: :class:`dropbox.team.UserSelectorArg`
            :class:`dropbox.team.MembersSendWelcomeError`
            team.members_send_welcome_email,
    def team_members_set_admin_permissions_v2(self,
                                              new_roles=None):
        Updates a team member's permissions. Permission : Team member
        :param user: Identity of user whose role will be set.
        :param Nullable[List[str]] new_roles: The new roles for the member. Send
            empty list to make user member only. For now, only up to one role is
            allowed.
        :rtype: :class:`dropbox.team.MembersSetPermissions2Result`
            :class:`dropbox.team.MembersSetPermissions2Error`
        arg = team.MembersSetPermissions2Arg(user,
                                             new_roles)
            team.members_set_admin_permissions_v2,
    def team_members_set_admin_permissions(self,
                                           new_role):
        :param new_role: The new role of the member.
        :type new_role: :class:`dropbox.team.AdminTier`
        :rtype: :class:`dropbox.team.MembersSetPermissionsResult`
            :class:`dropbox.team.MembersSetPermissionsError`
        arg = team.MembersSetPermissionsArg(user,
                                            new_role)
            team.members_set_admin_permissions,
    def team_members_set_profile_v2(self,
                                    new_email=None,
                                    new_external_id=None,
                                    new_given_name=None,
                                    new_surname=None,
                                    new_persistent_id=None,
                                    new_is_directory_restricted=None):
        Updates a team member's profile. Permission : Team member management.
        :param user: Identity of user whose profile will be set.
        :param Nullable[str] new_email: New email for member.
        :param Nullable[str] new_external_id: New external ID for member.
        :param Nullable[str] new_given_name: New given name for member.
        :param Nullable[str] new_surname: New surname for member.
        :param Nullable[str] new_persistent_id: New persistent ID. This field
            only available to teams using persistent ID SAML configuration.
        :param Nullable[bool] new_is_directory_restricted: New value for whether
            the user is a directory restricted user.
            :class:`dropbox.team.MembersSetProfileError`
        arg = team.MembersSetProfileArg(user,
                                        new_email,
                                        new_external_id,
                                        new_given_name,
                                        new_surname,
                                        new_persistent_id,
                                        new_is_directory_restricted)
            team.members_set_profile_v2,
    def team_members_set_profile(self,
            team.members_set_profile,
    def team_members_set_profile_photo_v2(self,
        Updates a team member's profile photo. Permission : Team member
        :param user: Identity of the user whose profile photo will be set.
        :param photo: Image to set as the member's new profile photo.
        :type photo: :class:`dropbox.team.PhotoSourceArg`
            :class:`dropbox.team.MembersSetProfilePhotoError`
        arg = team.MembersSetProfilePhotoArg(user,
                                             photo)
            team.members_set_profile_photo_v2,
    def team_members_set_profile_photo(self,
            team.members_set_profile_photo,
    def team_members_suspend(self,
                             wipe_data=True):
        Suspend a member from a team. Permission : Team member management
        :param bool wipe_data: If provided, controls if the user's data will be
            deleted on their linked devices.
            :class:`dropbox.team.MembersSuspendError`
        arg = team.MembersDeactivateArg(user,
                                        wipe_data)
            team.members_suspend,
    def team_members_unsuspend(self,
        Unsuspend a member from a team. Permission : Team member management
        :param user: Identity of user to unsuspend.
            :class:`dropbox.team.MembersUnsuspendError`
        arg = team.MembersUnsuspendArg(user)
            team.members_unsuspend,
    def team_namespaces_list(self,
        Returns a list of all team-accessible namespaces. This list includes
        team folders, shared folders containing team members, team members' home
        namespaces, and team members' app folders. Home namespaces and app
        folders are always owned by this team or members of the team, but shared
        folders may be owned by other users or other teams. Duplicates may occur
        in the list.
            scope: team_data.member
        :param int limit: Specifying a value here has no effect.
        :rtype: :class:`dropbox.team.TeamNamespacesListResult`
            :class:`dropbox.team.TeamNamespacesListError`
        arg = team.TeamNamespacesListArg(limit)
            team.namespaces_list,
    def team_namespaces_list_continue(self,
        Once a cursor has been retrieved from :meth:`team_namespaces_list`, use
        this to paginate through all team-accessible namespaces. Duplicates may
        occur in the list.
            team-accessible namespaces.
            :class:`dropbox.team.TeamNamespacesListContinueError`
        arg = team.TeamNamespacesListContinueArg(cursor)
            team.namespaces_list_continue,
    def team_properties_template_add(self,
        Permission : Team member file access.
        :rtype: :class:`dropbox.team.AddTemplateResult`
            :class:`dropbox.team.ModifyTemplateError`
            'properties/template/add is deprecated.',
            team.properties_template_add,
    def team_properties_template_get(self,
        Permission : Team member file access. The scope for the route is
        files.team_metadata.write.
            :meth:`team_templates_add_for_user` or
            :meth:`team_templates_add_for_team`.
        :rtype: :class:`dropbox.team.GetTemplateResult`
            :class:`dropbox.team.TemplateError`
            team.properties_template_get,
    def team_properties_template_list(self):
        :rtype: :class:`dropbox.team.ListTemplateResult`
            team.properties_template_list,
    def team_properties_template_update(self,
        :param Nullable[List[:class:`dropbox.team.PropertyFieldTemplate`]]
        :rtype: :class:`dropbox.team.UpdateTemplateResult`
            'properties/template/update is deprecated.',
            team.properties_template_update,
    def team_reports_get_activity(self,
        Retrieves reporting data about a team's user activity. Deprecated: Will
        be removed on July 1st 2021.
        :param Nullable[datetime] start_date: Optional starting date
            (inclusive). If start_date is None or too long ago, this field will
            be set to 6 months ago.
        :param Nullable[datetime] end_date: Optional ending date (exclusive).
        :rtype: :class:`dropbox.team.GetActivityReport`
            :class:`dropbox.team.DateRangeError`
            'reports/get_activity is deprecated.',
        arg = team.DateRange(start_date,
            team.reports_get_activity,
    def team_reports_get_devices(self,
        Retrieves reporting data about a team's linked devices. Deprecated: Will
        :rtype: :class:`dropbox.team.GetDevicesReport`
            'reports/get_devices is deprecated.',
            team.reports_get_devices,
    def team_reports_get_membership(self,
        Retrieves reporting data about a team's membership. Deprecated: Will be
        removed on July 1st 2021.
        :rtype: :class:`dropbox.team.GetMembershipReport`
            'reports/get_membership is deprecated.',
            team.reports_get_membership,
    def team_reports_get_storage(self,
        Retrieves reporting data about a team's storage usage. Deprecated: Will
        :rtype: :class:`dropbox.team.GetStorageReport`
            'reports/get_storage is deprecated.',
            team.reports_get_storage,
    def team_sharing_allowlist_add(self,
                                   domains=None,
                                   emails=None):
        Endpoint adds Approve List entries. Changes are effective immediately.
        Changes are committed in transaction. In case of single validation error
        - all entries are rejected. Valid domains (RFC-1034/5) and emails
        (RFC-5322/822) are accepted. Added entries cannot overflow limit of
        10000 entries per team. Maximum 100 entries per call is allowed.
            scope: team_info.write
        :param Nullable[List[str]] domains: List of domains represented by valid
            string representation (RFC-1034/5).
        :param Nullable[List[str]] emails: List of emails represented by valid
            string representation (RFC-5322/822).
        :rtype: :class:`dropbox.team.SharingAllowlistAddResponse`
            :class:`dropbox.team.SharingAllowlistAddError`
        arg = team.SharingAllowlistAddArgs(domains,
                                           emails)
            team.sharing_allowlist_add,
    def team_sharing_allowlist_list(self,
        Lists Approve List entries for given team, from newest to oldest,
        returning up to `limit` entries at a time. If there are more than
        `limit` entries associated with the current team, more can be fetched by
        passing the returned `cursor` to
        :meth:`team_sharing_allowlist_list_continue`.
        :param int limit: The number of entries to fetch at one time.
        :rtype: :class:`dropbox.team.SharingAllowlistListResponse`
        arg = team.SharingAllowlistListArg(limit)
            team.sharing_allowlist_list,
    def team_sharing_allowlist_list_continue(self,
        Lists entries associated with given team, starting from a the cursor.
        See :meth:`team_sharing_allowlist_list`.
        :param str cursor: The cursor returned from a previous call to
            :meth:`team_sharing_allowlist_list` or
            :class:`dropbox.team.SharingAllowlistListContinueError`
        arg = team.SharingAllowlistListContinueArg(cursor)
            team.sharing_allowlist_list_continue,
    def team_sharing_allowlist_remove(self,
        Endpoint removes Approve List entries. Changes are effective
        immediately. Changes are committed in transaction. In case of single
        validation error - all entries are rejected. Valid domains (RFC-1034/5)
        and emails (RFC-5322/822) are accepted. Entries being removed have to be
        present on the list. Maximum 1000 entries per call is allowed.
        :rtype: :class:`dropbox.team.SharingAllowlistRemoveResponse`
            :class:`dropbox.team.SharingAllowlistRemoveError`
        arg = team.SharingAllowlistRemoveArgs(domains,
            team.sharing_allowlist_remove,
    def team_team_folder_activate(self,
                                  team_folder_id):
        Sets an archived team folder's status to active. Permission : Team
        member file access.
            scope: team_data.content.write
        :param str team_folder_id: The ID of the team folder.
        :rtype: :class:`dropbox.team.TeamFolderMetadata`
        arg = team.TeamFolderIdArg(team_folder_id)
            team.team_folder_activate,
    def team_team_folder_archive(self,
                                 team_folder_id,
                                 force_async_off=False):
        Sets an active team folder's status to archived and removes all folder
        and file members. This endpoint cannot be used for teams that have a
        shared team space. Permission : Team member file access.
        :param bool force_async_off: Whether to force the archive to happen
            synchronously.
        :rtype: :class:`dropbox.team.TeamFolderArchiveLaunch`
        arg = team.TeamFolderArchiveArg(team_folder_id,
                                        force_async_off)
            team.team_folder_archive,
    def team_team_folder_archive_check(self,
        Returns the status of an asynchronous job for archiving a team folder.
        :rtype: :class:`dropbox.team.TeamFolderArchiveJobStatus`
            team.team_folder_archive_check,
    def team_team_folder_create(self,
                                sync_setting=None):
        Creates a new, active, team folder with no members. This endpoint can
        only be used for teams that do not already have a shared team space.
        :param str name: Name for the new team folder.
        :param Nullable[:class:`dropbox.team.SyncSettingArg`] sync_setting: The
            sync setting to apply to this team folder. Only permitted if the
            team has team selective sync enabled.
            :class:`dropbox.team.TeamFolderCreateError`
        arg = team.TeamFolderCreateArg(name,
                                       sync_setting)
            team.team_folder_create,
    def team_team_folder_get_info(self,
                                  team_folder_ids):
        Retrieves metadata for team folders. Permission : Team member file
            scope: team_data.content.read
        :param List[str] team_folder_ids: The list of team folder IDs.
        :rtype: List[:class:`dropbox.team.TeamFolderGetInfoItem`]
        arg = team.TeamFolderIdListArg(team_folder_ids)
            team.team_folder_get_info,
    def team_team_folder_list(self,
        Lists all team folders. Permission : Team member file access.
        :rtype: :class:`dropbox.team.TeamFolderListResult`
            :class:`dropbox.team.TeamFolderListError`
        arg = team.TeamFolderListArg(limit)
            team.team_folder_list,
    def team_team_folder_list_continue(self,
        Once a cursor has been retrieved from :meth:`team_team_folder_list`, use
        this to paginate through all team folders. Permission : Team member file
        :param str cursor: Indicates from what point to get the next set of team
            folders.
            :class:`dropbox.team.TeamFolderListContinueError`
        arg = team.TeamFolderListContinueArg(cursor)
            team.team_folder_list_continue,
    def team_team_folder_permanently_delete(self,
        Permanently deletes an archived team folder. This endpoint cannot be
        used for teams that have a shared team space. Permission : Team member
            team.team_folder_permanently_delete,
    def team_team_folder_rename(self,
                                name):
        Changes an active team folder's name. Permission : Team member file
        :param str name: New team folder name.
            :class:`dropbox.team.TeamFolderRenameError`
        arg = team.TeamFolderRenameArg(team_folder_id,
                                       name)
            team.team_folder_rename,
    def team_team_folder_update_sync_settings(self,
                                              sync_setting=None,
                                              content_sync_settings=None):
        Updates the sync settings on a team folder or its contents.  Use of this
        endpoint requires that the team has team selective sync enabled.
        :param Nullable[:class:`dropbox.team.SyncSettingArg`] sync_setting: Sync
            setting to apply to the team folder itself. Only meaningful if the
            team folder is not a shared team root.
        :param Nullable[List[:class:`dropbox.team.ContentSyncSettingArg`]]
            content_sync_settings: Sync settings to apply to contents of this
            team folder.
            :class:`dropbox.team.TeamFolderUpdateSyncSettingsError`
        arg = team.TeamFolderUpdateSyncSettingsArg(team_folder_id,
                                                   sync_setting,
                                                   content_sync_settings)
            team.team_folder_update_sync_settings,
    def team_token_get_authenticated_admin(self):
        Returns the member profile of the admin who generated the team access
        token used to make the call.
        :rtype: :class:`dropbox.team.TokenGetAuthenticatedAdminResult`
            :class:`dropbox.team.TokenGetAuthenticatedAdminError`
            team.token_get_authenticated_admin,
    def team_log_get_events(self,
                            account_id=None,
                            time=None,
                            category=None,
                            event_type=None):
        Retrieves team events. If the result's ``GetTeamEventsResult.has_more``
        field is ``True``, call :meth:`team_log_get_events_continue` with the
        returned cursor to retrieve more entries. If end_time is not specified
        in your request, you may use the returned cursor to poll
        :meth:`team_log_get_events_continue` for new events. Many attributes
        note 'may be missing due to historical data gap'. Note that the
        file_operations category and & analogous paper events are not available
        on all Dropbox Business `plans </business/plans-comparison>`_. Use
        `features/get_values
        </developers/documentation/http/teams#team-features-get_values>`_ to
        check for this feature. Permission : Team Auditing.
            scope: events.read
        :param int limit: The maximal number of results to return per call. Note
            that some calls may not return ``limit`` number of events, and may
            even return no events, even with `has_more` set to true. In this
            case, callers should fetch again using
            :meth:`team_log_get_events_continue`.
        :param Nullable[str] account_id: Filter the events by account ID. Return
            only events with this account_id as either Actor, Context, or
            Participants.
        :param Nullable[:class:`dropbox.team_log.TimeRange`] time: Filter by
            time range.
        :param Nullable[:class:`dropbox.team_log.EventCategory`] category:
            Filter the returned events to a single category. Note that category
            shouldn't be provided together with event_type.
        :param Nullable[:class:`dropbox.team_log.EventTypeArg`] event_type:
            Filter the returned events to a single event type. Note that
            event_type shouldn't be provided together with category.
        :rtype: :class:`dropbox.team_log.GetTeamEventsResult`
            :class:`dropbox.team_log.GetTeamEventsError`
        arg = team_log.GetTeamEventsArg(limit,
                                        account_id,
                                        event_type)
            team_log.get_events,
            'team_log',
    def team_log_get_events_continue(self,
        Once a cursor has been retrieved from :meth:`team_log_get_events`, use
        this to paginate through all events. Permission : Team Auditing.
            events.
            :class:`dropbox.team_log.GetTeamEventsContinueError`
        arg = team_log.GetTeamEventsContinueArg(cursor)
            team_log.get_events_continue,
