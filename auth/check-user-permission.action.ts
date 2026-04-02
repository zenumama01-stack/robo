 * Verifies if the current user has a specific role or permission to perform administrative tasks.
 * Supports checking for roles, authorizations, or entity permissions.
@RegisterClass(BaseAction, "CheckUserPermissionAction")
export class CheckUserPermissionAction extends BaseAction {
            const permissionType = params.Params.find(p => p.Name === 'PermissionType')?.Value as string || 'Role';
            const permissionName = params.Params.find(p => p.Name === 'PermissionName')?.Value as string;
            const userID = params.Params.find(p => p.Name === 'UserID')?.Value as string || params.ContextUser?.ID;
            if (!permissionName) {
                    Message: 'PermissionName parameter is required'
            // Validate permission type
            const validTypes = ['Role', 'Authorization', 'EntityPermission'];
            if (!validTypes.includes(permissionType)) {
                    ResultCode: 'INVALID_PERMISSION_TYPE',
                    Message: `Invalid permission type. Must be one of: ${validTypes.join(', ')}`
            const currentUser = params.ContextUser;
            if (!currentUser && !userID) {
                    Message: 'No authenticated user context found and no UserID provided'
            let hasPermission = false;
            const userRoles: string[] = [];
            // Check based on permission type
            switch (permissionType) {
                case 'Role':
                    // Get user's roles from UserCache
                    const cachedUser = UserCache.Users?.find(u => u.ID === userID);
                    if (cachedUser && cachedUser.UserRoles) {
                        cachedUser.UserRoles.forEach(ur => userRoles.push(ur.Role));
                        hasPermission = cachedUser.UserRoles.some(ur => ur.Role === permissionName);
                case 'Authorization':
                    // Check authorization roles
                    const authRv = new RunView();
                    const authResult = await authRv.RunView({
                        EntityName: 'MJ: Authorization Roles',
                        ExtraFilter: `RoleID IN (SELECT RoleID FROM ${Metadata.Provider.ConfigData.MJCoreSchemaName}.vwUserRoles WHERE UserID='${userID}') AND AuthorizationID IN (SELECT ID FROM ${Metadata.Provider.ConfigData.MJCoreSchemaName}.vwAuthorizations WHERE Name='${permissionName}')`,
                    hasPermission = authResult.Success && authResult.Results && authResult.Results.length > 0;
                case 'EntityPermission':
                    // Use the Metadata API to check entity permissions properly
                    const entityInfo = md.EntityByName(permissionName);
                            Message: `Entity '${permissionName}' not found`
                    // Get user permissions for this entity - Note: MJCore has typo in method name
                    const userPermissions = entityInfo.GetUserPermisions(currentUser);
                    // For EntityPermission, check if user has any permission (for HasPermission output)
                    hasPermission = userPermissions.CanCreate || userPermissions.CanRead || 
                                  userPermissions.CanUpdate || userPermissions.CanDelete;
                    // Add all CRUD permissions as output parameters
                    params.Params.push(
                            Name: 'CanCreate',
                            Value: userPermissions.CanCreate,
                            Name: 'CanUpdate',
                            Value: userPermissions.CanUpdate,
                            Name: 'CanRead',
                            Value: userPermissions.CanRead,
                            Name: 'CanDelete',
                            Value: userPermissions.CanDelete,
                Name: 'HasPermission',
                Value: hasPermission,
            if (userRoles.length > 0) {
                    Name: 'UserRoles',
                    Value: userRoles,
                ResultCode: hasPermission ? 'SUCCESS' : 'PERMISSION_DENIED',
                Message: hasPermission 
                    ? `User has required ${permissionType}: ${permissionName}`
                    : `User does not have required ${permissionType}: ${permissionName}`,
                Message: `Error checking user permission: ${e.message}`
