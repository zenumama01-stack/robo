 * Action to create tasks in HubSpot
@RegisterClass(BaseAction, 'CreateTaskAction')
export class CreateTaskAction extends HubSpotBaseAction {
     * Create a task in HubSpot
            const subject = this.getParamValue(Params, 'Subject');
            const body = this.getParamValue(Params, 'Body');
            const status = this.getParamValue(Params, 'Status') || 'NOT_STARTED';
            const priority = this.getParamValue(Params, 'Priority') || 'NONE';
            const dueDate = this.getParamValue(Params, 'DueDate');
            const reminderDate = this.getParamValue(Params, 'ReminderDate');
            const companyIds = this.getParamValue(Params, 'CompanyIds');
            const dealIds = this.getParamValue(Params, 'DealIds');
            const taskType = this.getParamValue(Params, 'TaskType') || 'TODO';
            const queueId = this.getParamValue(Params, 'QueueId');
            if (!subject) {
                    Message: 'Subject is required',
            // Validate status
            const validStatuses = ['NOT_STARTED', 'IN_PROGRESS', 'WAITING', 'COMPLETED', 'DEFERRED'];
            if (!validStatuses.includes(status.toUpperCase())) {
                    Message: `Invalid Status. Must be one of: ${validStatuses.join(', ')}`,
            // Validate priority
            const validPriorities = ['NONE', 'LOW', 'MEDIUM', 'HIGH'];
            if (!validPriorities.includes(priority.toUpperCase())) {
                    Message: `Invalid Priority. Must be one of: ${validPriorities.join(', ')}`,
            // Prepare task properties
            const taskProperties: any = {
                hs_task_subject: subject,
                hs_task_body: body || '',
                hs_task_status: status.toUpperCase(),
                hs_task_priority: priority.toUpperCase(),
                hs_task_type: taskType
            // Add dates if provided
            if (dueDate) {
                const dueDateObj = new Date(dueDate);
                if (isNaN(dueDateObj.getTime())) {
                        Message: 'Invalid DueDate format',
                // HubSpot expects date in midnight UTC
                dueDateObj.setUTCHours(0, 0, 0, 0);
                taskProperties.hs_timestamp = dueDateObj.getTime();
            if (reminderDate) {
                const reminderDateObj = new Date(reminderDate);
                if (isNaN(reminderDateObj.getTime())) {
                        Message: 'Invalid ReminderDate format',
                taskProperties.hs_task_reminders = reminderDateObj.getTime();
            // Add owner if provided
            if (ownerId) {
                taskProperties.hubspot_owner_id = ownerId;
            // Add queue if provided
            if (queueId) {
                taskProperties.hs_queue_membership_ids = queueId;
            // Create the task using the objects API
            const task = await this.makeHubSpotRequest<any>(
                'objects/tasks',
                { properties: taskProperties },
            // Associate with contacts, companies, and deals
            const associationResults = [];
            if (contactIds && Array.isArray(contactIds) && contactIds.length > 0) {
                            'tasks',
                            task.id,
            if (companyIds && Array.isArray(companyIds) && companyIds.length > 0) {
            if (dealIds && Array.isArray(dealIds) && dealIds.length > 0) {
                for (const dealId of dealIds) {
                            dealId,
                            type: 'deal',
                            id: dealId,
            // Format task details
            const taskDetails = this.mapHubSpotProperties(task);
                taskId: taskDetails.id,
                subject: taskDetails.hs_task_subject,
                status: taskDetails.hs_task_status,
                priority: taskDetails.hs_task_priority,
                dueDate: taskDetails.hs_timestamp ? new Date(parseInt(taskDetails.hs_timestamp)).toISOString() : null,
                reminderDate: taskDetails.hs_task_reminders ? new Date(parseInt(taskDetails.hs_task_reminders)).toISOString() : null,
                owner: taskDetails.hubspot_owner_id,
                createdAt: taskDetails.createdAt,
                portalUrl: `https://app.hubspot.com/contacts/tasks/${taskDetails.id}`,
                associations: associationResults
            const taskDetailsParam = outputParams.find(p => p.Name === 'TaskDetails');
            if (taskDetailsParam) taskDetailsParam.Value = taskDetails;
                Message: `Successfully created task: ${taskDetails.hs_task_subject}`,
                Message: `Error creating task: ${errorMessage}`,
                Name: 'Subject',
                Name: 'Body',
                Value: 'NOT_STARTED'
                Value: 'NONE'
                Name: 'DueDate',
                Name: 'ReminderDate',
                Name: 'CompanyIds',
                Name: 'DealIds',
                Name: 'TaskType',
                Value: 'TODO'
                Name: 'QueueId',
                Name: 'TaskDetails',
        return 'Creates a task in HubSpot with due dates and optional associations to contacts, companies, and deals';
