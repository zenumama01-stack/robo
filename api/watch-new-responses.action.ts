 * Action to watch for new SurveyMonkey responses since last check
 *   ActionName: 'Watch for New SurveyMonkey Responses',
@RegisterClass(BaseAction, 'WatchNewSurveyMonkeyResponsesAction')
export class WatchNewSurveyMonkeyResponsesAction extends SurveyMonkeyBaseAction {
        return 'Polls SurveyMonkey for new responses since the last check. Returns only new submissions and updates the last checked timestamp. Perfect for triggering workflows, sending notifications, or syncing data to other systems.';
            const maxResponses = this.getParamValue(params.Params, 'MaxResponses') || 1000;
                start_created_at: lastChecked,
                sort_by: 'date_created',
                sort_order: 'ASC',
                status: onlyCompleted ? 'completed' : undefined,
            const newResponses = smResponses.map(r => this.normalizeSurveyMonkeyResponse(r));
            const completedCount = newResponses.filter(r => r.completed).length;
            const partialCount = newResponses.filter(r => !r.completed).length;
            const responsesByType: Record<string, any[]> = {};
            if (newResponses.length > 0) {
                const firstResponse = newResponses[0];
                for (const answer of firstResponse.answerDetails) {
                    responsesByType[answer.fieldType] = [];
                for (const response of newResponses) {
                        if (!responsesByType[answer.fieldType]) {
                        responsesByType[answer.fieldType].push({
                            responseId: response.responseId,
                            submittedAt: response.submittedAt
            const emails = this.extractEmailFromResponses(newResponses);
                    Name: 'NewResponses',
                    Value: newResponses
                    Value: newResponses.length
                    Name: 'ResponsesByType',
                    Value: responsesByType
                    Name: 'HasNewResponses',
                    Value: newResponses.length > 0
            const message = newResponses.length > 0
                ? `Found ${newResponses.length} new responses (${completedCount} completed, ${partialCount} partial) since ${new Date(lastChecked).toLocaleString()}`
                : `No new responses since ${new Date(lastChecked).toLocaleString()}`;
                ResultCode: newResponses.length > 0 ? 'NEW_RESPONSES' : 'NO_NEW_RESPONSES',
                Message: this.buildFormErrorMessage('Watch New SurveyMonkey Responses', errorMessage, error)
 * Action to watch for new Typeform responses since last check
 *   ActionName: 'Watch for New Typeform Responses',
@RegisterClass(BaseAction, 'WatchNewTypeformResponsesAction')
export class WatchNewTypeformResponsesAction extends TypeformBaseAction {
        return 'Polls Typeform for new responses since the last check. Returns only new submissions and updates the last checked timestamp. Perfect for triggering workflows, sending notifications, or syncing data to other systems.';
                since: lastChecked,
                completed: onlyCompleted ? true : undefined,
                sort: 'submitted_at,asc',
            const newResponses = tfResponses.map(r => this.normalizeTypeformResponse(r));
                    Name: 'NewResponseCount',
                Message: this.buildFormErrorMessage('Watch New Typeform Responses', errorMessage, error)
                Value: 60,
                Value: 1000,
