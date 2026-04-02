 * Validates that an email address is not already in use by another user in the system.
 * Returns information about any existing user with the same email.
@RegisterClass(BaseAction, "ValidateEmailUniqueAction")
export class ValidateEmailUniqueAction extends BaseAction {
        console.log('VALIDATE EMAIL UNIQUE ACTION - InternalRunAction called with params:', {
            paramCount: params.Params.length,
            params: params.Params.map(p => ({ Name: p.Name, Value: p.Value, Type: p.Type })),
            contextUser: params.ContextUser?.Email
            // Extract email parameter
                    Message: 'Email parameter is required'
            // Basic email format validation
            if (!emailRegex.test(email)) {
                    ResultCode: 'INVALID_EMAIL_FORMAT',
                    Message: 'Invalid email address format'
            // Check if email exists in UserCache
            let isUnique = true;
            let existingUserID: string | null = null;
            let existingUserName: string | null = null;
                existingUserID = existingUser.ID;
                existingUserName = existingUser.Name;
                Name: 'IsUnique',
                Value: isUnique,
            if (!isUnique && existingUserID) {
                    Name: 'ExistingUserID',
                    Value: existingUserID,
                    Name: 'ExistingUserName',
                    Value: existingUserName,
                Message: isUnique 
                    ? `Email '${email}' is available`
                    : `Email '${email}' is already in use by ${existingUserName}`,
                Message: `Error validating email: ${e.message}`
