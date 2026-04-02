import { LearnWorldsBaseAction } from '../learnworlds-base.action';
 * Action to create a new user in LearnWorlds
@RegisterClass(BaseAction, 'CreateUserAction')
export class CreateUserAction extends LearnWorldsBaseAction {
     * Create a new user
            const username = this.getParamValue(Params, 'Username');
            const password = this.getParamValue(Params, 'Password');
            const role = this.getParamValue(Params, 'Role') || 'student';
            const isActive = this.getParamValue(Params, 'IsActive') !== false;
            const sendWelcomeEmail = this.getParamValue(Params, 'SendWelcomeEmail') !== false;
            const tags = this.getParamValue(Params, 'Tags');
            const customFields = this.getParamValue(Params, 'CustomFields');
            const enrollInCourses = this.getParamValue(Params, 'EnrollInCourses');
            // Prepare user data
            const userData: any = {
                role: role,
                is_active: isActive
            if (username) userData.username = username;
            if (password) userData.password = password;
            if (firstName) userData.first_name = firstName;
            if (lastName) userData.last_name = lastName;
            if (sendWelcomeEmail !== undefined) userData.send_welcome_email = sendWelcomeEmail;
            // Add tags if provided (expecting comma-separated string or array)
            if (tags) {
                userData.tags = Array.isArray(tags) ? tags : tags.split(',').map((t: string) => t.trim());
            // Add custom fields if provided
            if (customFields) {
                userData.custom_fields = customFields;
            // Create user
            const newUser = await this.makeLearnWorldsRequest<any>(
                'users',
                userData,
            // Format user details
            const userDetails = {
                id: newUser.id,
                email: newUser.email,
                username: newUser.username,
                firstName: newUser.first_name,
                lastName: newUser.last_name,
                fullName: `${newUser.first_name || ''} ${newUser.last_name || ''}`.trim(),
                role: newUser.role,
                status: newUser.is_active ? 'active' : 'inactive',
                tags: newUser.tags || [],
                customFields: newUser.custom_fields || {},
                createdAt: newUser.created_at,
                loginUrl: newUser.login_url,
                resetPasswordUrl: newUser.reset_password_url
            // Enroll in courses if requested
            const enrollmentResults: any[] = [];
            if (enrollInCourses && enrollInCourses.length > 0) {
                const courseIds = Array.isArray(enrollInCourses) ? enrollInCourses : [enrollInCourses];
                for (const courseId of courseIds) {
                        const enrollData = await this.makeLearnWorldsRequest<any>(
                            `courses/${courseId}/enrollments`,
                                user_id: newUser.id,
                                justification: 'Enrolled during user creation',
                                notify_user: false
                        enrollmentResults.push({
                            courseId: courseId,
                            enrollmentId: enrollData.id
                    } catch (enrollError) {
                            error: enrollError instanceof Error ? enrollError.message : 'Enrollment failed'
                userId: userDetails.id,
                email: userDetails.email,
                username: userDetails.username,
                fullName: userDetails.fullName,
                role: userDetails.role,
                status: userDetails.status,
                welcomeEmailSent: sendWelcomeEmail,
                coursesEnrolled: enrollmentResults.filter(r => r.success).length,
                totalCoursesRequested: enrollmentResults.length,
                loginUrl: userDetails.loginUrl
            const userDetailsParam = outputParams.find(p => p.Name === 'UserDetails');
            if (userDetailsParam) userDetailsParam.Value = userDetails;
            const enrollmentResultsParam = outputParams.find(p => p.Name === 'EnrollmentResults');
            if (enrollmentResultsParam) enrollmentResultsParam.Value = enrollmentResults;
                Message: `Successfully created user ${userDetails.email}`,
                Message: `Error creating user: ${errorMessage}`,
        const baseParams = this.getCommonLMSParams();
                Name: 'Username',
                Name: 'Password',
                Name: 'Role',
                Value: 'student'
                Name: 'IsActive',
                Name: 'SendWelcomeEmail',
                Name: 'Tags',
                Name: 'CustomFields',
                Name: 'EnrollInCourses',
                Name: 'UserDetails',
                Name: 'EnrollmentResults',
        return 'Creates a new user in LearnWorlds with optional course enrollments and welcome email';
 * Creates a new user in the MemberJunction system with validation and optional employee linking.
 * while adding user-specific validation and business logic.
@RegisterClass(BaseAction, "CreateUserAction")
export class CreateUserAction extends CreateRecordAction {
            // Extract user-specific parameters
            const type = params.Params.find(p => p.Name === 'Type')?.Value as string || 'User';
            const isActive = params.Params.find(p => p.Name === 'IsActive')?.Value !== false;
            const employeeID = params.Params.find(p => p.Name === 'EmployeeID')?.Value as string;
            if (!email || !firstName || !lastName) {
                    Message: 'Email, FirstName, and LastName are required'
            // Check if email already exists using UserCache
            const existingUser = UserCache.Users?.find(u => 
                u.Email?.toLowerCase() === email.toLowerCase()
            if (existingUser) {
                    Message: `Email address '${email}' already exists in the system`
            // Validate employee ID if provided
            if (employeeID) {
                const employeeCheck = await rv.RunView({
                    ExtraFilter: `ID='${employeeID}'`,
                if (!employeeCheck.Success || !employeeCheck.Results || employeeCheck.Results.length === 0) {
                        ResultCode: 'INVALID_EMPLOYEE',
                        Message: `Employee ID '${employeeID}' does not exist`
                Name: `${firstName} ${lastName}`,
                Type: type,
                IsActive: isActive
                fields.EmployeeID = employeeID;
                fields.LinkedRecordType = 'Employee';
                fields.LinkedEntityID = '@lookup:Entities.Name=Employees';
                fields.LinkedEntityRecordID = employeeID;
                    { Name: 'EntityName', Value: 'Users', Type: 'Input' },
                // Extract the created user ID and add user-specific output parameters
                        Value: fields.Name,
                Message: `Error creating user: ${e.message}`
