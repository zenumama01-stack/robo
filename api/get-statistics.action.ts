import { FormStatistics } from '../../../base/base-form-builder.action';
 * Action to calculate aggregate statistics from Google Forms responses
 *   ActionName: 'Get Google Forms Response Statistics',
 *     Value: '1a2b3c4d5e'
@RegisterClass(BaseAction, 'GetGoogleFormsStatisticsAction')
export class GetGoogleFormsStatisticsAction extends GoogleFormsBaseAction {
        return 'Calculates comprehensive statistics from Google Forms responses including completion rates, response trends, popular answers, and time-based analytics. Useful for dashboards and reporting.';
            const includeTopAnswers = this.getParamValue(params.Params, 'IncludeTopAnswers') !== false;
            const topAnswersLimit = this.getParamValue(params.Params, 'TopAnswersLimit') || 10;
            // Get all responses from Google Forms
            // Normalize responses to common format
            const responses = gfResponses.map(r => this.normalizeGoogleFormsResponse(r));
            const completedResponses = responses.filter(r => r.completed);
            const partialResponses = responses.filter(r => !r.completed);
            const completionRate = this.calculateCompletionRate(
                completedResponses.length,
                responses.length
            const responsesByDate = this.groupResponsesByDate(responses);
            const statistics: FormStatistics = {
                totalResponses: responses.length,
                completedResponses: completedResponses.length,
                partialResponses: partialResponses.length,
                completionRate,
                averageCompletionTime: this.calculateAverageCompletionTime(responses),
                responsesByDate
            // Calculate top answers if requested
            if (includeTopAnswers && responses.length > 0) {
                const topAnswers: Record<string, Array<{ answer: string; count: number }>> = {};
                        allFieldIds.add(answer.fieldId);
                    const fieldTopAnswers = this.findTopAnswers(responses, fieldId, topAnswersLimit);
                    if (fieldTopAnswers.length > 0) {
                        topAnswers[fieldId] = fieldTopAnswers;
                statistics.topAnswers = topAnswers;
            // Calculate time-based breakdowns
            const dailyBreakdown = this.calculateDailyBreakdown(responses);
            const hourlyDistribution = this.calculateHourlyDistribution(responses);
            // Build output parameters
                    Value: statistics
                    Name: 'TotalResponses',
                    Value: statistics.totalResponses
                    Name: 'CompletedResponses',
                    Value: statistics.completedResponses
                    Name: 'PartialResponses',
                    Value: statistics.partialResponses
                    Name: 'CompletionRate',
                    Value: statistics.completionRate
                    Name: 'AverageCompletionTime',
                    Value: statistics.averageCompletionTime
                    Name: 'ResponsesByDate',
                    Value: statistics.responsesByDate
                    Name: 'DailyBreakdown',
                    Value: dailyBreakdown
                    Name: 'HourlyDistribution',
                    Value: hourlyDistribution
            if (includeTopAnswers) {
                outputParams.push({
                    Name: 'TopAnswers',
                    Value: statistics.topAnswers
                Message: `Successfully calculated statistics from ${responses.length} Google Forms responses (${completionRate}% completion rate)`
                Message: this.buildFormErrorMessage('Get Google Forms Statistics', errorMessage, error)
     * Calculate daily breakdown of responses
    private calculateDailyBreakdown(responses: any[]): Array<{ date: string; count: number; completed: number; partial: number }> {
        const daily: Record<string, { count: number; completed: number; partial: number }> = {};
            if (!daily[dateKey]) {
                daily[dateKey] = { count: 0, completed: 0, partial: 0 };
            daily[dateKey].count++;
            if (response.completed) {
                daily[dateKey].completed++;
                daily[dateKey].partial++;
        return Object.entries(daily)
            .map(([date, stats]) => ({ date, ...stats }))
     * Calculate hourly distribution of responses
    private calculateHourlyDistribution(responses: any[]): Array<{ hour: number; count: number }> {
        const hourly: Record<number, number> = {};
        // Initialize all hours
        for (let i = 0; i < 24; i++) {
            hourly[i] = 0;
        // Count responses by hour
            const hour = response.submittedAt.getHours();
            hourly[hour]++;
        return Object.entries(hourly).map(([hour, count]) => ({
            hour: parseInt(hour),
            count
                Value: 10000
                Name: 'IncludeTopAnswers',
                Name: 'TopAnswersLimit',
                Value: 10
 * Action to calculate aggregate statistics from JotForm submissions
 *   ActionName: 'Get JotForm Submission Statistics',
@RegisterClass(BaseAction, 'GetJotFormStatisticsAction')
export class GetJotFormStatisticsAction extends JotFormBaseAction {
        return 'Calculates comprehensive statistics from JotForm submissions including completion rates, submission trends, popular answers, and time-based analytics. Useful for dashboards and reporting.';
            // Parse filter if provided as string
            let filter: Record<string, string> | undefined;
                if (typeof filterParam === 'string') {
                        filter = JSON.parse(filterParam);
                            Message: 'Filter parameter must be a valid JSON object'
                    filter = filterParam;
            // Get all submissions from JotForm
            // Normalize submissions to common format
            const completedSubmissions = submissions.filter(s => s.completed);
            const partialSubmissions = submissions.filter(s => !s.completed);
                completedSubmissions.length,
                submissions.length
            const responsesByDate = this.groupResponsesByDate(submissions);
                totalResponses: submissions.length,
                completedResponses: completedSubmissions.length,
                partialResponses: partialSubmissions.length,
                averageCompletionTime: this.calculateAverageCompletionTime(submissions),
            if (includeTopAnswers && submissions.length > 0) {
                for (const submission of submissions) {
                    for (const answer of submission.answerDetails) {
                    const fieldTopAnswers = this.findTopAnswers(submissions, fieldId, topAnswersLimit);
            const dailyBreakdown = this.calculateDailyBreakdown(submissions);
            const hourlyDistribution = this.calculateHourlyDistribution(submissions);
                    Name: 'TotalSubmissions',
                    Name: 'CompletedSubmissions',
                    Name: 'PartialSubmissions',
                Message: `Successfully calculated statistics from ${submissions.length} JotForm submissions (${completionRate}% completion rate)`
                Message: this.buildFormErrorMessage('Get JotForm Statistics', errorMessage, error)
     * Calculate daily breakdown of submissions
    private calculateDailyBreakdown(submissions: any[]): Array<{ date: string; count: number; completed: number; partial: number }> {
            const dateKey = submission.submittedAt.toISOString().split('T')[0];
            if (submission.completed) {
     * Calculate hourly distribution of submissions
    private calculateHourlyDistribution(submissions: any[]): Array<{ hour: number; count: number }> {
        // Count submissions by hour
            const hour = submission.submittedAt.getHours();
 * Action to calculate aggregate statistics from SurveyMonkey responses
 *   ActionName: 'Get SurveyMonkey Response Statistics',
@RegisterClass(BaseAction, 'GetSurveyMonkeyStatisticsAction')
export class GetSurveyMonkeyStatisticsAction extends SurveyMonkeyBaseAction {
        return 'Calculates comprehensive statistics from SurveyMonkey responses including completion rates, response trends, popular answers, and time-based analytics. Useful for dashboards and reporting.';
                Message: `Successfully calculated statistics from ${responses.length} SurveyMonkey responses (${completionRate}% completion rate)`
                Message: this.buildFormErrorMessage('Get SurveyMonkey Statistics', errorMessage, error)
 * Action to calculate aggregate statistics from Typeform responses
 *   ActionName: 'Get Typeform Response Statistics',
@RegisterClass(BaseAction, 'GetTypeformStatisticsAction')
export class GetTypeformStatisticsAction extends TypeformBaseAction {
        return 'Calculates comprehensive statistics from Typeform responses including completion rates, response trends, popular answers, and time-based analytics. Useful for dashboards and reporting.';
                Message: `Successfully calculated statistics from ${responses.length} Typeform responses (${completionRate}% completion rate)`
                Message: this.buildFormErrorMessage('Get Typeform Statistics', errorMessage, error)
                Value: 10,
