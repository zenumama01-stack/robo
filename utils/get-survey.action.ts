 * Action to retrieve complete details of a SurveyMonkey survey including pages, questions, and settings
 *   ActionName: 'Get SurveyMonkey Details',
@RegisterClass(BaseAction, 'GetSurveyMonkeyAction')
export class GetSurveyMonkeyDetailsAction extends SurveyMonkeyBaseAction {
        return 'Retrieves complete details of a SurveyMonkey survey including title, pages, questions, settings, and metadata. Use this to inspect or backup survey configurations, analyze survey structure, and access survey metadata.';
            // Fetch complete survey details from SurveyMonkey API
            const response = await this.getAxiosInstance(accessToken).get(`/surveys/${surveyId}/details`);
            const survey = response.data;
            // Extract comprehensive survey information
            const questionCount = survey.question_count || 0;
            const pageCount = survey.page_count || 0;
            const createdAt = survey.date_created || '';
            const updatedAt = survey.date_modified || '';
            // Determine survey status (active if response count exists, otherwise draft)
            const status = survey.response_count > 0 ? 'active' : 'draft';
            // Build comprehensive survey details object
            const surveyOutput = {
                id: survey.id,
                title: survey.title,
                nickname: survey.nickname,
                href: survey.href,
                language: survey.language,
                questionCount,
                pageCount,
                responseCount: survey.response_count,
                createdAt,
                updatedAt,
                pages: survey.pages || [],
                    buttons_text: survey.buttons_text || {},
                    custom_variables: survey.custom_variables || {},
                    folder_id: survey.folder_id,
                    category: survey.category,
                    quiz_options: survey.quiz_options,
                    preview: survey.preview,
                    edit_url: survey.edit_url,
                    collect_url: survey.collect_url,
                    analyze_url: survey.analyze_url,
                    summary_url: survey.summary_url
                    Value: surveyOutput
                    Value: survey.id
                    Value: survey.title
                    Value: pageCount
                    Value: status
                Message: `Successfully retrieved survey "${survey.title}" with ${questionCount} questions across ${pageCount} pages`
                Message: this.buildFormErrorMessage('Get SurveyMonkey Details', errorMessage, error)
