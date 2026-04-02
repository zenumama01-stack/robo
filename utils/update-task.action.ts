 * Action to update task status and details in HubSpot
@RegisterClass(BaseAction, 'UpdateTaskAction')
export class UpdateTaskAction extends HubSpotBaseAction {
     * Update a task in HubSpot
            const taskId = this.getParamValue(Params, 'TaskId');
            const status = this.getParamValue(Params, 'Status');
            const completedDate = this.getParamValue(Params, 'CompletedDate');
            const taskType = this.getParamValue(Params, 'TaskType');
                    Message: 'TaskId is required',
            // Build update properties
            const updateProperties: any = {};
            // Update status if provided
                updateProperties.hs_task_status = status.toUpperCase();
                // If marking as completed and no completed date provided, use current time
                if (status.toUpperCase() === 'COMPLETED' && !completedDate) {
                    updateProperties.hs_task_completion_date = Date.now();
            // Update priority if provided
                updateProperties.hs_task_priority = priority.toUpperCase();
            // Update other fields if provided
            if (subject) updateProperties.hs_task_subject = subject;
            if (body !== undefined) updateProperties.hs_task_body = body;
            if (taskType) updateProperties.hs_task_type = taskType;
            if (ownerId !== undefined) updateProperties.hubspot_owner_id = ownerId;
            // Update dates if provided
            if (dueDate !== undefined) {
                if (dueDate === null) {
                    // Allow clearing the due date
                    updateProperties.hs_timestamp = '';
                    updateProperties.hs_timestamp = dueDateObj.getTime();
            if (reminderDate !== undefined) {
                if (reminderDate === null) {
                    // Allow clearing the reminder
                    updateProperties.hs_task_reminders = '';
                    updateProperties.hs_task_reminders = reminderDateObj.getTime();
            if (completedDate) {
                const completedDateObj = new Date(completedDate);
                if (isNaN(completedDateObj.getTime())) {
                        Message: 'Invalid CompletedDate format',
                updateProperties.hs_task_completion_date = completedDateObj.getTime();
            if (Object.keys(updateProperties).length === 0) {
            // Get current task to show before/after
            let originalTask;
                originalTask = await this.makeHubSpotRequest<any>(
                    `objects/tasks/${taskId}`,
                    ResultCode: 'TASK_NOT_FOUND',
                    Message: `Task with ID ${taskId} not found`,
            const updatedTask = await this.makeHubSpotRequest<any>(
                { properties: updateProperties },
            const originalDetails = this.mapHubSpotProperties(originalTask);
            const updatedDetails = this.mapHubSpotProperties(updatedTask);
            for (const key of Object.keys(updateProperties)) {
                const fieldName = key.replace('hs_task_', '').replace('hs_', '').replace(/_/g, ' ');
                changes[fieldName] = {
                    from: originalDetails[key],
                    to: updatedDetails[key]
                taskId: updatedDetails.id,
                subject: updatedDetails.hs_task_subject,
                status: updatedDetails.hs_task_status,
                priority: updatedDetails.hs_task_priority,
                dueDate: updatedDetails.hs_timestamp ? new Date(parseInt(updatedDetails.hs_timestamp)).toISOString() : null,
                reminderDate: updatedDetails.hs_task_reminders ? new Date(parseInt(updatedDetails.hs_task_reminders)).toISOString() : null,
                completedDate: updatedDetails.hs_task_completion_date ? new Date(parseInt(updatedDetails.hs_task_completion_date)).toISOString() : null,
                owner: updatedDetails.hubspot_owner_id,
                updatedAt: updatedDetails.updatedAt,
                portalUrl: `https://app.hubspot.com/contacts/tasks/${updatedDetails.id}`,
                changes: changes
            const originalDetailsParam = outputParams.find(p => p.Name === 'OriginalDetails');
            if (originalDetailsParam) originalDetailsParam.Value = originalDetails;
            const updatedDetailsParam = outputParams.find(p => p.Name === 'UpdatedDetails');
            if (updatedDetailsParam) updatedDetailsParam.Value = updatedDetails;
                Message: `Successfully updated task: ${updatedDetails.hs_task_subject}`,
                Message: `Error updating task: ${errorMessage}`,
                Name: 'TaskId',
                Name: 'CompletedDate',
                Name: 'OriginalDetails',
                Name: 'UpdatedDetails',
        return 'Updates task status and other properties in HubSpot';
