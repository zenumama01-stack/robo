import { ApplicationInfo, EntitySaveOptions, LogError, LogStatus, Metadata, RunView, RunViewResult, UserInfo } from "@memberjunction/core";
import { configInfo } from "../config.js";
import { MJUserEntity, MJUserRoleEntity, MJUserApplicationEntity, MJUserApplicationEntityEntity, MJApplicationEntityType, MJApplicationEntityEntityType } from "@memberjunction/core-entities";
export class NewUserBase {
    public async createNewUser(firstName: string, lastName: string, email: string, linkedRecordType: string = 'None', linkedEntityId?: string, linkedEntityRecordId?: string): Promise<MJUserEntity | null> {
            let contextUser: UserInfo | null = null;
            const contextUserForNewUserCreation: string = configInfo?.userHandling?.contextUserForNewUserCreation;
            if(contextUserForNewUserCreation){
                contextUser = UserCache.Instance.UserByName(contextUserForNewUserCreation);
                LogError(`Failed to load context user ${configInfo?.userHandling?.contextUserForNewUserCreation}, using an existing user with the Owner role instead`);
                contextUser = UserCache.Users.find(user => user.Type.trim().toLowerCase() ==='owner')!;
                    LogError(`No existing users found in the database with the Owner role, cannot create a new user`);
            const user = await md.GetEntityObject<MJUserEntity>('MJ: Users', contextUser) // To-Do - change this to be a different defined user for the user creation process
            user.Name = email;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.Type = 'User';
            user.LinkedRecordType = linkedRecordType;
            if (linkedEntityId){
                user.LinkedEntityID = linkedEntityId;
            if (linkedEntityRecordId){
                user.LinkedEntityRecordID = linkedEntityRecordId;
            const saveResult: boolean = await user.Save();
                LogError(`Failed to create new user ${firstName} ${lastName} ${email}:`, undefined, user.LatestResult);
            if(configInfo.userHandling && configInfo.userHandling.newUserRoles){
                // user created, now create however many roles we need to create for this user based on the config settings
                LogStatus(`User ${user.Email} created, assigning roles`);
                for (const role of configInfo.userHandling.newUserRoles) {
                    const userRoleEntity: MJUserRoleEntity = await md.GetEntityObject<MJUserRoleEntity>('MJ: User Roles', contextUser);
                    userRoleEntity.NewRecord();
                    userRoleEntity.UserID = user.ID;
                    const userRole = md.Roles.find(r => r.Name === role);
                    if (!userRole) {
                        LogError(`Role ${role} not found in the database, cannot assign to new user ${user.Name}`);
                    userRoleEntity.RoleID = userRole.ID;
                    const roleSaveResult: boolean = await userRoleEntity.Save();
                    if(roleSaveResult){
                        LogStatus(`Assigned role ${role} to new user ${user.Name}`);
                        LogError(`Failed to assign role ${role} to new user ${user.Name}:`, undefined, userRoleEntity.LatestResult);
            if (configInfo.userHandling && configInfo.userHandling.CreateUserApplicationRecords) {
                LogStatus("Creating User Applications for new user: " + user.Name);
                // Determine which applications to create UserApplication records for
                // If UserApplications config array has entries, use those
                // Otherwise, fall back to applications with DefaultForNewUser = true
                let applicationsToCreate: ApplicationInfo[] = [];
                if (configInfo.userHandling.UserApplications && configInfo.userHandling.UserApplications.length > 0) {
                    // Use explicitly configured applications
                    for (const appName of configInfo.userHandling.UserApplications) {
                        if (application) {
                            applicationsToCreate.push(application);
                            LogError(`Application ${appName} not found in the Metadata, cannot assign to new user ${user.Name}`);
                    // Fall back to DefaultForNewUser applications from metadata, sorted by DefaultSequence
                    LogStatus(`No UserApplications configured, using DefaultForNewUser applications for new user ${user.Name}`);
                    applicationsToCreate = md.Applications
                        .filter(a => a.DefaultForNewUser)
                        .sort((a, b) => (a.DefaultSequence ?? 100) - (b.DefaultSequence ?? 100));
                    LogStatus(`Found ${applicationsToCreate.length} applications with DefaultForNewUser=true`);
                // Create UserApplication records for each application
                for (const [appIndex, application] of applicationsToCreate.entries()) {
                    const userApplication: MJUserApplicationEntity = await md.GetEntityObject<MJUserApplicationEntity>('MJ: User Applications', contextUser);
                    userApplication.Sequence = appIndex; // Set sequence based on order
                        LogStatus(`Created User Application ${application.Name} for new user ${user.Name}`);
                            LogError(`Failed to load Application Entities for Application ${application.Name} for new user ${user.Name}:`, undefined, rvResult.ErrorMessage);
                        LogStatus(`Creating ${rvResult.Results.length} User Application Entities for User Application ${application.Name} for new user ${user.Name}`);
                            const userAppEntity: MJUserApplicationEntityEntity = await md.GetEntityObject<MJUserApplicationEntityEntity>('MJ: User Application Entities', contextUser);
                            userAppEntity.EntityID = appEntity.EntityID;
                        LogError(`Failed to create User Application ${application.Name} for new user ${user.Name}:`, undefined, userApplication.LatestResult);
