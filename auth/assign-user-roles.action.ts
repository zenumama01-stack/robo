import { MJUserRoleEntity } from "@memberjunction/core-entities";
 * Assigns one or more roles to a user by creating UserRole associations.
 * This action accepts either a single role name or an array of role names.
 * It gracefully handles both cases and assigns all specified roles to the user.
@RegisterClass(BaseAction, "AssignUserRolesAction")
export class AssignUserRolesAction extends BaseAction {
            const userID = params.Params.find(p => p.Name === 'UserID')?.Value as string;
            const roleNamesParam = params.Params.find(p => p.Name === 'RoleNames')?.Value;
            const roleNameParam = params.Params.find(p => p.Name === 'RoleName')?.Value;
            if (!userID) {
                    Message: 'UserID is required'
            // Handle both single and multiple role inputs
            let roleNames: string[] = [];
            if (roleNamesParam) {
                // If RoleNames is provided, use it
                if (Array.isArray(roleNamesParam)) {
                    roleNames = roleNamesParam;
                } else if (typeof roleNamesParam === 'string') {
                    roleNames = [roleNamesParam];
            } else if (roleNameParam) {
                // If RoleName (singular) is provided, use it
                if (Array.isArray(roleNameParam)) {
                    roleNames = roleNameParam;
                } else if (typeof roleNameParam === 'string') {
                    roleNames = [roleNameParam];
            if (roleNames.length === 0) {
                    Message: 'RoleNames or RoleName must be provided as a string or array'
            // Validate user exists - check UserCache first
            // For newly created users, the cache might not be updated yet
            const user = UserCache.Users?.find(u => u.ID === userID);
            const existingRoleIDs = user?.UserRoles ? user.UserRoles.map(ur => ur.RoleID) : [];
            // Get all roles and validate they exist using Metadata cache
            const allRoles = md.Roles;
            const foundRoles = roleNames.map(name => {
                return allRoles.find(r => r.Name === name);
            }).filter(r => r != null);
            const foundRoleNames = foundRoles.map(r => r.Name);
            const missingRoles = roleNames.filter(name => !foundRoleNames.includes(name));
            if (missingRoles.length > 0) {
                    ResultCode: 'ROLES_NOT_FOUND',
                    Message: `The following roles do not exist: ${missingRoles.join(', ')}`
            // Process each role
            const assignedRoles: { roleID: string, roleName: string, userRoleID: string }[] = [];
            const skippedRoles: string[] = [];
            for (const role of foundRoles) {
                if (existingRoleIDs.includes(role.ID)) {
                    skippedRoles.push(role.Name);
                    // Create UserRole entity
                    const userRole = await md.GetEntityObject<MJUserRoleEntity>('MJ: User Roles', params.ContextUser);
                    userRole.UserID = userID;
                    userRole.RoleID = role.ID;
                    if (await userRole.Save()) {
                        assignedRoles.push({
                            roleID: role.ID,
                            roleName: role.Name,
                            userRoleID: userRole.ID
                        errors.push(`Failed to assign role '${role.Name}'`);
                    errors.push(`Error assigning role '${role.Name}': ${e.message}`);
            let message = '';
            if (assignedRoles.length > 0) {
                message += `Successfully assigned ${assignedRoles.length} role(s): ${assignedRoles.map(r => r.roleName).join(', ')}. `;
            if (skippedRoles.length > 0) {
                message += `Skipped ${skippedRoles.length} already assigned role(s): ${skippedRoles.join(', ')}. `;
            if (errors.length > 0) {
                message += `Errors: ${errors.join('; ')}`;
                Name: 'AssignedRoleIDs',
                Value: assignedRoles.map(r => r.roleID),
                Name: 'AssignedUserRoleIDs',
                Value: assignedRoles.map(r => r.userRoleID),
                Name: 'AssignedRoleNames',
                Value: assignedRoles.map(r => r.roleName),
                Name: 'AssignedCount',
                Value: assignedRoles.length,
                Success: errors.length === 0 && (assignedRoles.length > 0 || skippedRoles.length > 0),
                Message: message.trim(),
                Message: `Error assigning user roles: ${e.message}`
