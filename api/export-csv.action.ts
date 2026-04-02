import { GoogleFormsBaseAction } from '../googleforms-base.action';
 * Action to export Google Forms responses as CSV format
 * Security: This action uses secure credential lookup via Company Integrations.
 * API credentials are retrieved from environment variables or the database based on CompanyID.
 *   ActionName: 'Export Google Forms Responses to CSV',
 *     Name: 'CompanyID',
 *     Value: 'company-uuid-here'
 *     Name: 'FormID',
 *     Value: '1a2b3c4d5e6f7g8h9i0j'
 *     Name: 'IncludeMetadata',
 *     Value: true
@RegisterClass(BaseAction, 'ExportGoogleFormsCSVAction')
export class ExportGoogleFormsCSVAction extends GoogleFormsBaseAction {
        return 'Exports Google Forms responses to CSV format for use in spreadsheets, data analysis tools, or archival. Supports custom delimiters and optional metadata columns. Requires OAuth 2.0 access token with forms.responses.readonly scope.';
                    ResultCode: 'MISSING_CONTEXT_USER',
                    Message: 'Context user is required for Google Forms API calls'
            const companyId = this.getParamValue(params.Params, 'CompanyID');
            const formId = this.getParamValue(params.Params, 'FormID');
            if (!formId) {
                    ResultCode: 'MISSING_FORM_ID',
                    Message: 'FormID parameter is required'
            const accessToken = await this.getSecureAPIToken(companyId, contextUser);
            const includeMetadata = this.getParamValue(params.Params, 'IncludeMetadata') !== false;
            const delimiter = this.getParamValue(params.Params, 'Delimiter') || ',';
            const maxResponses = this.getParamValue(params.Params, 'MaxResponses') || 10000;
            const gfResponses = await this.getAllGoogleFormsResponses(formId, accessToken, {
                maxResponses
            if (gfResponses.length === 0) {
                    ResultCode: 'NO_DATA',
                    Message: 'No responses found for this form'
            const responses = gfResponses.map(r => {
                const normalized = this.normalizeGoogleFormsResponse(r);
                // Set the formId since it's not included in the response object
                normalized.formId = formId;
            const { csv, headers } = this.convertToCSV(responses, includeMetadata, delimiter);
                    Name: 'CSVData',
                    Value: csv
                    Name: 'RowCount',
                    Value: responses.length
                    Name: 'Headers',
                    Value: headers
                    Name: 'ColumnCount',
                    Value: headers.length
                    Name: 'FileSize',
                    Value: Buffer.byteLength(csv, 'utf8')
            for (const outputParam of outputParams) {
                const existingParam = params.Params.find(p => p.Name === outputParam.Name);
                if (existingParam) {
                    existingParam.Value = outputParam.Value;
                    params.Params.push(outputParam);
                Message: `Successfully exported ${responses.length} responses to CSV (${headers.length} columns, ${Buffer.byteLength(csv, 'utf8')} bytes)`
                Message: this.buildFormErrorMessage('Export Google Forms to CSV', errorMessage, error)
                Value: null,
                Name: 'IncludeMetadata',
                Value: true,
                Name: 'Delimiter',
                Value: ',',
                Name: 'MaxResponses',
                Value: 10000,
 * Action to export JotForm submissions as CSV format
 *   ActionName: 'Export JotForm Submissions to CSV',
 *     Value: '123456789'
@RegisterClass(BaseAction, 'ExportJotFormCSVAction')
export class ExportJotFormCSVAction extends JotFormBaseAction {
        return 'Exports JotForm submissions to CSV format for use in spreadsheets, data analysis tools, or archival. Supports custom delimiters, optional metadata columns, and regional API endpoints.';
            const apiKey = await this.getSecureAPIToken(companyId, contextUser);
            const filterParam = this.getParamValue(params.Params, 'Filter');
            const maxSubmissions = this.getParamValue(params.Params, 'MaxSubmissions') || 10000;
            let filter: Record<string, string> | undefined = undefined;
            if (filterParam) {
                    filter = typeof filterParam === 'string' ? JSON.parse(filterParam) : filterParam;
                        ResultCode: 'INVALID_FILTER',
                        Message: 'Filter parameter must be valid JSON object'
            const jfSubmissions = await this.getAllJotFormSubmissions(formId, apiKey, {
                maxSubmissions,
                region
            if (jfSubmissions.length === 0) {
                    Message: 'No submissions found matching the criteria'
            const submissions = jfSubmissions.map(s => this.normalizeJotFormSubmission(s));
            const { csv, headers } = this.convertToCSV(submissions, includeMetadata, delimiter);
                    Value: submissions.length
                Message: `Successfully exported ${submissions.length} submissions to CSV (${headers.length} columns, ${Buffer.byteLength(csv, 'utf8')} bytes)`
                Message: this.buildFormErrorMessage('Export JotForm to CSV', errorMessage, error)
                Value: 'us'
                Name: 'Filter',
                Value: ','
                Name: 'MaxSubmissions',
 * Action to export SurveyMonkey responses as CSV format
 *   ActionName: 'Export SurveyMonkey Responses to CSV',
 *     Name: 'SurveyID',
 *     Value: 'abc123'
@RegisterClass(BaseAction, 'ExportSurveyMonkeyCSVAction')
export class ExportSurveyMonkeyCSVAction extends SurveyMonkeyBaseAction {
        return 'Exports SurveyMonkey responses to CSV format for use in spreadsheets, data analysis tools, or archival. Supports custom delimiters, date range filtering, and optional metadata columns.';
            const surveyId = this.getParamValue(params.Params, 'SurveyID');
            if (!surveyId) {
                    ResultCode: 'MISSING_SURVEY_ID',
                    Message: 'SurveyID parameter is required'
            const startCreatedAt = this.getParamValue(params.Params, 'StartCreatedAt');
            const endCreatedAt = this.getParamValue(params.Params, 'EndCreatedAt');
            const smResponses = await this.getAllSurveyMonkeyResponses(surveyId, accessToken, {
                start_created_at: startCreatedAt,
                end_created_at: endCreatedAt,
            if (smResponses.length === 0) {
                    Message: 'No responses found matching the criteria'
            const responses = smResponses.map(r => this.normalizeSurveyMonkeyResponse(r));
                Message: this.buildFormErrorMessage('Export SurveyMonkey to CSV', errorMessage, error)
                Name: 'StartCreatedAt',
                Name: 'EndCreatedAt',
 * Action to export Typeform responses as CSV format
 *   ActionName: 'Export Typeform Responses to CSV',
@RegisterClass(BaseAction, 'ExportTypeformCSVAction')
export class ExportTypeformCSVAction extends TypeformBaseAction {
        return 'Exports Typeform responses to CSV format for use in spreadsheets, data analysis tools, or archival. Supports custom delimiters and optional metadata columns.';
            const since = this.getParamValue(params.Params, 'Since');
            const until = this.getParamValue(params.Params, 'Until');
            const completed = this.getParamValue(params.Params, 'Completed');
            const tfResponses = await this.getAllTypeformResponses(formId, apiToken, {
                since,
                until,
            if (tfResponses.length === 0) {
            const responses = tfResponses.map(r => this.normalizeTypeformResponse(r));
                Message: this.buildFormErrorMessage('Export Typeform to CSV', errorMessage, error)
                Name: 'Since',
                Name: 'Until',
                Name: 'Completed',
