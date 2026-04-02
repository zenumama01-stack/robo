 * Action to retrieve a specific response from Google Forms by response ID
 *   ActionName: 'Get Single Google Forms Response',
 *     Value: 'abc123xyz'
 *     Name: 'ResponseID',
 *     Value: 'ACYDBNhVXdW3lFGH...'
@RegisterClass(BaseAction, 'GetSingleGoogleFormsResponseAction')
export class GetSingleGoogleFormsResponseAction extends GoogleFormsBaseAction {
        return 'Retrieves details of a specific Google Forms response by its unique response ID. Includes all answers, metadata, submission timing information, and respondent email if available.';
            const responseId = this.getParamValue(params.Params, 'ResponseID');
            if (!responseId) {
                    ResultCode: 'MISSING_RESPONSE_ID',
                    Message: 'ResponseID parameter is required'
            const gfResponse = await this.getSingleGoogleFormsResponse(formId, responseId, accessToken);
            const normalizedResponse = this.normalizeGoogleFormsResponse(gfResponse);
            // Build respondent object with email if available
            const respondent: { email?: string } = {};
            if (gfResponse.respondentEmail) {
                respondent.email = gfResponse.respondentEmail;
                    Name: 'Response',
                    Value: normalizedResponse
                    Name: 'ResponseID',
                    Value: normalizedResponse.responseId
                    Name: 'SubmittedAt',
                    Value: normalizedResponse.submittedAt
                    Name: 'Answers',
                    Value: normalizedResponse.answers
                    Name: 'Respondent',
                    Value: respondent
                Message: `Successfully retrieved response ${responseId} from Google Forms`
                Message: this.buildFormErrorMessage('Get Single Google Forms Response', errorMessage, error)
 * Action to retrieve a specific response from SurveyMonkey by response ID
 *   ActionName: 'Get Single SurveyMonkey Response',
 *     Value: '987654321'
@RegisterClass(BaseAction, 'GetSingleSurveyMonkeyResponseAction')
export class GetSingleSurveyMonkeyResponseAction extends SurveyMonkeyBaseAction {
        return 'Retrieves details of a specific SurveyMonkey response by its unique response ID. Includes all answers, metadata, response status, and submission timing information.';
            const smResponse = await this.getSingleSurveyMonkeyResponse(surveyId, responseId, accessToken);
            const normalizedResponse = this.normalizeSurveyMonkeyResponse(smResponse);
                    Value: smResponse.response_status
                    Value: normalizedResponse.metadata
                Message: `Successfully retrieved response ${responseId} from SurveyMonkey`
                Message: this.buildFormErrorMessage('Get Single SurveyMonkey Response', errorMessage, error)
 * Action to retrieve a specific response from Typeform by response token
 *   ActionName: 'Get Single Typeform Response',
 *     Name: 'ResponseToken',
 *     Value: 'xyz789'
@RegisterClass(BaseAction, 'GetSingleTypeformResponseAction')
export class GetSingleTypeformResponseAction extends TypeformBaseAction {
        return 'Retrieves details of a specific Typeform response by its unique response token. Includes all answers, metadata, hidden fields, and calculated values.';
            const responseToken = this.getParamValue(params.Params, 'ResponseToken');
            if (!responseToken) {
                    ResultCode: 'MISSING_RESPONSE_TOKEN',
                    Message: 'ResponseToken parameter is required'
            const tfResponse = await this.getSingleTypeformResponse(formId, responseToken, apiToken);
            const normalizedResponse = this.normalizeTypeformResponse(tfResponse, formFields);
                    Value: normalizedResponse.completed
                    Value: normalizedResponse.answerDetails
                    Value: normalizedResponse.hiddenFields
                    Name: 'CalculatedFields',
                    Value: normalizedResponse.calculatedFields
                Message: `Successfully retrieved response ${responseToken} from Typeform`
                Message: this.buildFormErrorMessage('Get Single Typeform Response', errorMessage, error)
                Name: 'ResponseToken',
