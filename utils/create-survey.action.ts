import { SurveyMonkeyBaseAction } from '../surveymonkey-base.action';
 * Action to create a new SurveyMonkey survey programmatically
 * Security: Uses secure credential lookup via CompanyID instead of accepting tokens directly.
 *   ActionName: 'Create SurveyMonkey',
 *     Value: 'Customer Satisfaction Survey'
 *     Name: 'Pages',
 *       title: 'Page 1',
 *       questions: [{
 *         family: 'single_choice',
 *         subtype: 'vertical',
 *         heading: 'How satisfied are you with our service?',
 *         answers: {
 *           choices: [
 *             { text: 'Very Satisfied' },
 *             { text: 'Satisfied' },
 *             { text: 'Neutral' },
 *             { text: 'Dissatisfied' }
 *           ]
 *       }]
 *     Name: 'Language',
 *     Value: 'en'
@RegisterClass(BaseAction, 'CreateSurveyMonkeyAction')
export class CreateSurveyMonkeyAction extends SurveyMonkeyBaseAction {
        return 'Creates a new SurveyMonkey survey with specified title, pages, and questions. Supports all question families including single_choice, multiple_choice, matrix, open_ended, demographic, datetime, and presentation. Returns the new survey ID, URLs, and page count.';
                    Message: 'Context user is required for SurveyMonkey API calls'
            const pages = this.getParamValue(params.Params, 'Pages');
            if (!pages || !Array.isArray(pages) || pages.length === 0) {
                    ResultCode: 'MISSING_PAGES',
                    Message: 'Pages parameter is required and must be a non-empty array'
            const surveyData: Record<string, any> = {
                pages
            const language = this.getParamValue(params.Params, 'Language');
            if (language) {
                surveyData.language = language;
            const buttonsText = this.getParamValue(params.Params, 'ButtonsText');
            if (buttonsText && typeof buttonsText === 'object') {
                surveyData.buttons_text = buttonsText;
            const response = await this.getAxiosInstance(accessToken).post('/surveys', surveyData);
            const createdSurvey = response.data;
            const surveyDetails = await this.getSurveyMonkeyDetails(createdSurvey.id, accessToken);
            const surveyUrl = surveyDetails.collect_url || `https://www.surveymonkey.com/r/${createdSurvey.id}`;
            const editUrl = surveyDetails.edit_url || `https://www.surveymonkey.com/create/?sm=${createdSurvey.id}`;
                    Name: 'Survey',
                    Value: surveyDetails
                    Name: 'SurveyID',
                    Value: createdSurvey.id
                    Value: surveyDetails.title
                    Name: 'SurveyURL',
                    Value: surveyUrl
                    Name: 'EditURL',
                    Value: editUrl
                    Name: 'PageCount',
                    Value: surveyDetails.page_count
                Message: `Successfully created survey "${surveyDetails.title}" (ID: ${createdSurvey.id}). View at: ${surveyUrl}`
                Message: this.buildFormErrorMessage('Create SurveyMonkey Survey', errorMessage, error)
                Name: 'Pages',
                Name: 'Language',
                Name: 'ButtonsText',
