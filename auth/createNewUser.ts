import { ApplicationInfo, LogError, LogStatus, Metadata, RunView, RunViewResult, UserInfo } from "@memberjunction/core";
import { NewUserSetup } from "../Config/config";
import { MJUserEntity, MJUserRoleEntity, MJUserApplicationEntity, MJUserApplicationEntityEntity, MJApplicationEntityEntityType } from "@memberjunction/core-entities";
import { logError, logStatus } from "./status_logging";
 * Base class for creating a new user in the system, you can sub-class this class to create your own user creation logic
export class CreateNewUserBase {
    public async createNewUser(newUserSetup: NewUserSetup): Promise<{Success: boolean, Message: string, Severity: 'warning' | 'error' | undefined}> {
            const matches: UserInfo = UserCache.Users.find(u => u?.Type?.trim().toLowerCase() ==='owner')!;
            const currentUser = matches ? matches : UserCache.Users[0]; // if we don't find an Owner, use the first user in the cache
                    Message: "No existing users found in the database, cannot create a new user",
                    Severity: 'error'
                // check for existing user by email
                const existingNewUser = UserCache.Users.find(u => u.Email === newUserSetup.Email);
                if (!existingNewUser) {
                    if (newUserSetup.Email && newUserSetup.Email.length > 0) {
                        logStatus("Attempting to create new user: " + newUserSetup.Email);
                        const user = <MJUserEntity>await md.GetEntityObject('MJ: Users', currentUser);
                        user.Name = newUserSetup.UserName ? newUserSetup.UserName : newUserSetup.Email;
                        user.FirstName = newUserSetup.FirstName;
                        user.LastName = newUserSetup.LastName;
                        user.Email = newUserSetup.Email;
                        user.Type = 'Owner';
                        user.IsActive = true;
                        if (await user.Save()) {
                            // save was successful, so we can create the User Roles
                            for (let i = 0; i < newUserSetup.Roles.length; i++) {
                                const roleName = newUserSetup.Roles[i];
                                const roleID = md.Roles.find(r => r.Name === roleName)?.ID;
                                if (!roleID) {
                                    logError("   Role not found: " + roleName + ", skipping");
                                const userRole = <MJUserRoleEntity>await md.GetEntityObject('MJ: User Roles', currentUser);
                                userRole.UserID = user.ID;
                                userRole.RoleID = roleID;
                                    logStatus("   Created User Role: " + roleName);
                                    logError("   Failed to create User Role: " + roleName);
                            // Create UserApplication records if specified in the config
                            if (newUserSetup.CreateUserApplicationRecords) {
                                logStatus("Creating User Applications for new user: " + user.Name);
                                for(const appName of newUserSetup.UserApplications){
                                    const toLowerCase: string = appName.trim().toLocaleLowerCase();
                                    const application: ApplicationInfo | undefined = md.Applications.find(a => a.Name.trim().toLocaleLowerCase() === toLowerCase);
                                    if (!application) {
                                        logError(`Application ${appName} not found in the Metadata, cannot assign to new user ${user.Name}`);
                                    const userApplication: MJUserApplicationEntity = await md.GetEntityObject<MJUserApplicationEntity>('MJ: User Applications', currentUser);
                                    userApplication.NewRecord();
                                    userApplication.UserID = user.ID;
                                    userApplication.ApplicationID = application.ID;
                                    userApplication.IsActive = true;
                                    const userApplicationSaveResult: boolean = await userApplication.Save();
                                    if(userApplicationSaveResult){
                                        logStatus(`Created User Application ${appName} for new user ${user.Name}`);
                                        //now create a MJUserApplicationEntity records for each entity in the application
                                        const rvResult: RunViewResult<MJApplicationEntityEntityType> = await rv.RunView({
                                            ExtraFilter: `ApplicationID = '${application.ID}' and DefaultForNewUser = 1`,
                                            LogError(`Failed to load Application Entities for Application ${appName} for new user ${user.Name}:`, undefined, rvResult.ErrorMessage);
                                        LogStatus(`Creating ${rvResult.Results.length} User Application Entities for User Application ${appName} for new user ${user.Name}`);
                                        for(const [index, appEntity] of rvResult.Results.entries()){
                                            const userAppEntity: MJUserApplicationEntityEntity = await md.GetEntityObject<MJUserApplicationEntityEntity>('MJ: User Application Entities', currentUser);
                                            userAppEntity.NewRecord();
                                            userAppEntity.UserApplicationID = userApplication.ID;
                                            userAppEntity.EntityID = appEntity.EntityID!;
                                            userAppEntity.Sequence = index;
                                            const userAppEntitySaveResult: boolean = await userAppEntity.Save();
                                            if(userAppEntitySaveResult){
                                                LogStatus(`Created User Application Entity ${appEntity.Entity} for new user ${user.Name}`);
                                                LogError(`Failed to create User Application Entity for new user ${user.Name}:`, undefined, userAppEntity.LatestResult);
                                        logError(`Failed to create User Application ${appName} for new user ${user.Name}:`, undefined, userApplication.LatestResult);
                            logStatus("Finished creating new user: " + newUserSetup.Email);
                                Message: "Successfully created new user: " + newUserSetup.Email,
                                Severity: undefined
                            // saving the user failed, so we don't atempt to create User Roles, throw error
                                Message: "Failed to save new user: " + newUserSetup.Email,
                            Message: "No email address provided for new user, cannot create new user. Params:" + JSON.stringify(newUserSetup),
                            Severity: 'warning'
                        Message: "New user setup is already complete, skipping",
                    Message: "No newUserSetup object provided, createNewUser() shouldn't be called without a valid object",
                Message: "Error attemping to create a new user: " + err,
