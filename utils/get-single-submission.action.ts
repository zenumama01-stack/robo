 * Action to retrieve a specific submission from JotForm by submission ID
 *   ActionName: 'Get Single JotForm Submission',
 *     Name: 'SubmissionID',
 *     Value: '1234567890'
@RegisterClass(BaseAction, 'GetSingleJotFormSubmissionAction')
export class GetSingleJotFormSubmissionAction extends JotFormBaseAction {
        return 'Retrieves details of a specific JotForm submission by its unique submission ID. Includes all answers, metadata, IP address, and submission status.';
            const submissionId = this.getParamValue(params.Params, 'SubmissionID');
            if (!submissionId) {
                    ResultCode: 'MISSING_SUBMISSION_ID',
                    Message: 'SubmissionID parameter is required'
            const jfSubmission = await this.getSingleJotFormSubmission(submissionId, apiKey, region);
            const normalizedSubmission = this.normalizeJotFormSubmission(jfSubmission);
                    Name: 'Submission',
                    Value: normalizedSubmission
                    Name: 'SubmissionID',
                    Value: normalizedSubmission.responseId
                    Value: normalizedSubmission.submittedAt
                    Value: jfSubmission.status
                    Value: normalizedSubmission.answers
                Message: `Successfully retrieved submission ${submissionId} from JotForm`
                Message: this.buildFormErrorMessage('Get Single JotForm Submission', errorMessage, error)
