 * Action to watch for new JotForm submissions since last check
 * Useful for workflow automation and real-time notifications
 *   ActionName: 'Watch for New JotForm Submissions',
 *     Name: 'LastCheckedTimestamp',
 *     Value: '2024-01-01T12:00:00Z'
@RegisterClass(BaseAction, 'WatchNewJotFormSubmissionsAction')
export class WatchNewJotFormSubmissionsAction extends JotFormBaseAction {
        return 'Polls JotForm for new submissions since the last check. Returns only new submissions and updates the last checked timestamp. Perfect for triggering workflows, sending notifications, or syncing data to other systems.';
            let lastChecked = this.getParamValue(params.Params, 'LastCheckedTimestamp');
            const onlyCompleted = this.getParamValue(params.Params, 'OnlyCompleted') === true;
            const maxSubmissions = this.getParamValue(params.Params, 'MaxSubmissions') || 1000;
            if (!lastChecked) {
                const defaultLookback = this.getParamValue(params.Params, 'DefaultLookbackMinutes') || 60;
                const lookbackDate = new Date();
                lookbackDate.setMinutes(lookbackDate.getMinutes() - defaultLookback);
                lastChecked = lookbackDate.toISOString();
            const currentTimestamp = new Date().toISOString();
            // Build filter for JotForm API
            // JotForm uses created_at field with "gt:" prefix for greater than
            const filter: Record<string, string> = {
                created_at: `gt:${lastChecked}`
            if (onlyCompleted) {
                filter.status = 'ACTIVE';
            const jfSubmissions = await this.getAllJotFormSubmissions(formId, apiToken, {
                orderby: 'created_at',
            const newSubmissions = jfSubmissions.map(s => this.normalizeJotFormSubmission(s));
            const completedCount = newSubmissions.filter(s => s.completed).length;
            const partialCount = newSubmissions.filter(s => !s.completed).length;
            // Group submissions by field type for analysis
            const submissionsByType: Record<string, any[]> = {};
            if (newSubmissions.length > 0) {
                const firstSubmission = newSubmissions[0];
                for (const answer of firstSubmission.answerDetails) {
                    submissionsByType[answer.fieldType] = [];
                for (const submission of newSubmissions) {
                        if (!submissionsByType[answer.fieldType]) {
                        submissionsByType[answer.fieldType].push({
                            submissionId: submission.responseId,
                            question: answer.question,
                            answer: answer.answer,
                            submittedAt: submission.submittedAt
            const emails = this.extractEmailFromResponses(newSubmissions);
                    Name: 'NewSubmissions',
                    Value: newSubmissions
                    Value: newSubmissions.length
                    Name: 'CompletedCount',
                    Value: completedCount
                    Name: 'PartialCount',
                    Value: partialCount
                    Name: 'LastChecked',
                    Value: currentTimestamp
                    Name: 'PreviouslyChecked',
                    Value: lastChecked
                    Name: 'SubmissionsByType',
                    Value: submissionsByType
                    Name: 'ExtractedEmails',
                    Value: emails
                    Name: 'HasNewSubmissions',
                    Value: newSubmissions.length > 0
            const message = newSubmissions.length > 0
                ? `Found ${newSubmissions.length} new submissions (${completedCount} completed, ${partialCount} partial) since ${new Date(lastChecked).toLocaleString()}`
                : `No new submissions since ${new Date(lastChecked).toLocaleString()}`;
                ResultCode: newSubmissions.length > 0 ? 'NEW_SUBMISSIONS' : 'NO_NEW_SUBMISSIONS',
                Message: message
                Message: this.buildFormErrorMessage('Watch New JotForm Submissions', errorMessage, error)
                Name: 'LastCheckedTimestamp',
                Name: 'DefaultLookbackMinutes',
                Value: 60
                Name: 'OnlyCompleted',
