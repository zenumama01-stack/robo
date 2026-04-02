import { CreateRecordAction } from "../crud/create-record.action";
 * Creates a new employee record in the MemberJunction system.
 * This action extends CreateRecordAction to leverage existing record creation functionality
 * while adding employee-specific validation and business logic.
@RegisterClass(BaseAction, "CreateEmployeeAction")
export class CreateEmployeeAction extends CreateRecordAction {
            // Extract employee-specific parameters
            const email = params.Params.find(p => p.Name === 'Email')?.Value as string;
            const firstName = params.Params.find(p => p.Name === 'FirstName')?.Value as string;
            const lastName = params.Params.find(p => p.Name === 'LastName')?.Value as string;
            const title = params.Params.find(p => p.Name === 'Title')?.Value as string;
            const phone = params.Params.find(p => p.Name === 'Phone')?.Value as string;
            const companyID = params.Params.find(p => p.Name === 'CompanyID')?.Value as string;
            const supervisorID = params.Params.find(p => p.Name === 'SupervisorID')?.Value as string;
            const active = params.Params.find(p => p.Name === 'Active')?.Value !== false;
            if (!email || !firstName || !lastName || !companyID) {
                    Message: 'Email, FirstName, LastName, and CompanyID are required'
            // Check if email already exists for another employee
            const existingEmployee = await rv.RunView({
                EntityName: 'MJ: Employees',
                ExtraFilter: `Email='${email.replace(/'/g, "''")}'`,
            if (existingEmployee.Success && existingEmployee.Results && existingEmployee.Results.length > 0) {
                    ResultCode: 'EMAIL_EXISTS',
                    Message: `Email address '${email}' already exists for another employee`
            // Validate company exists
            const companyCheck = await rv.RunView({
                EntityName: 'MJ: Companies',
                ExtraFilter: `ID='${companyID}'`,
            if (!companyCheck.Success || !companyCheck.Results || companyCheck.Results.length === 0) {
                    ResultCode: 'INVALID_COMPANY',
                    Message: `Company ID '${companyID}' does not exist`
            // Validate supervisor if provided
            if (supervisorID) {
                const supervisorCheck = await rv.RunView({
                    ExtraFilter: `ID='${supervisorID}'`,
                if (!supervisorCheck.Success || !supervisorCheck.Results || supervisorCheck.Results.length === 0) {
                        ResultCode: 'INVALID_SUPERVISOR',
                        Message: `Supervisor ID '${supervisorID}' does not exist`
            // Prepare fields for base CreateRecordAction
            const fields: Record<string, any> = {
                FirstName: firstName,
                LastName: lastName,
                Email: email,
                CompanyID: companyID,
                Active: active
            if (title) fields.Title = title;
            if (phone) fields.Phone = phone;
            if (supervisorID) fields.SupervisorID = supervisorID;
            // Transform parameters to match CreateRecordAction format
            const createParams: RunActionParams = {
                    { Name: 'EntityName', Value: 'Employees', Type: 'Input' },
                    { Name: 'Fields', Value: fields, Type: 'Input' }
            // Call parent CreateRecordAction
            const result = await super.InternalRunAction(createParams);
                // Extract the created employee ID
                const primaryKey = result.Params?.find(p => p.Name === 'PrimaryKey')?.Value as Record<string, any>;
                if (primaryKey) {
                        Name: 'EmployeeID',
                        Value: primaryKey.ID,
                Message: `Error creating employee: ${e.message}`
