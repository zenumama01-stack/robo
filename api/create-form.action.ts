import { JotFormBaseAction } from '../jotform-base.action';
 * Action to create a new JotForm programmatically
 * Security: API credentials are retrieved securely from Company Integrations
 * instead of being passed as parameters.
 *   ActionName: 'Create JotForm',
 *     Value: 'your-company-id'
 *     Name: 'Title',
 *     Value: 'Customer Feedback Form'
 *     Name: 'Questions',
 *     Value: [{
 *       type: 'control_text',
 *       text: 'What is your name?',
 *       name: 'name',
 *       required: 'Yes'
 *     }, {
 *       type: 'control_email',
 *       text: 'What is your email address?',
 *       name: 'email',
 *     }]
 *     Name: 'Properties',
 *     Value: {
 *       thankYouURL: 'https://example.com/thank-you',
 *       requiresPassword: false
@RegisterClass(BaseAction, 'CreateJotFormAction')
export class CreateJotFormAction extends JotFormBaseAction {
        return 'Creates a new JotForm with specified title, questions, and properties. Supports all question types including text inputs, email, dropdowns, checkboxes, file uploads, and more. Returns the new form ID, public URL, and admin URL.';
                    Message: 'Context user is required for JotForm API calls'
            const apiToken = await this.getSecureAPIToken(companyId, contextUser);
            const title = this.getParamValue(params.Params, 'Title');
                    ResultCode: 'MISSING_TITLE',
                    Message: 'Title parameter is required'
            const questions = this.getParamValue(params.Params, 'Questions');
            if (!questions || !Array.isArray(questions) || questions.length === 0) {
                    ResultCode: 'MISSING_QUESTIONS',
                    Message: 'Questions parameter is required and must be a non-empty array'
            const region = this.getParamValue(params.Params, 'Region') as 'us' | 'eu' | 'hipaa' | undefined;
            const formData: Record<string, any> = {
                'questions': this.buildQuestionsObject(questions),
                'properties': {
                    'title': title
            const properties = this.getParamValue(params.Params, 'Properties');
            if (properties && typeof properties === 'object') {
                formData.properties = {
                    ...formData.properties,
                    ...properties
            const response = await this.getAxiosInstance(apiToken, region).post('/user/forms', null, {
                    apiKey: apiToken,
                    ...this.flattenFormData(formData)
            const createdForm = response.data.content;
            const formUrl = `https://form.jotform.com/${createdForm.id}`;
            const adminUrl = `https://www.jotform.com/build/${createdForm.id}`;
                    Name: 'FormDetails',
                    Value: createdForm
                    Value: createdForm.id
                    Name: 'FormURL',
                    Value: formUrl
                    Name: 'AdminURL',
                    Value: adminUrl
                Message: `Successfully created JotForm "${title}" (ID: ${createdForm.id}). View at: ${formUrl}`
                Message: this.buildFormErrorMessage('Create JotForm', errorMessage, error)
     * Build questions object in JotForm API format
     * JotForm uses numeric keys for questions (1, 2, 3, etc.)
    private buildQuestionsObject(questions: any[]): Record<string, any> {
        const questionsObj: Record<string, any> = {};
        questions.forEach((question, index) => {
            const questionId = index + 1;
            questionsObj[questionId] = {
                type: question.type,
                text: question.text,
                name: question.name || `question_${questionId}`,
                order: String(questionId),
                ...this.extractQuestionProperties(question)
        return questionsObj;
     * Extract additional question properties (required, validation, options, etc.)
    private extractQuestionProperties(question: any): Record<string, any> {
        const props: Record<string, any> = {};
        if (question.required != null) {
            props.required = question.required === 'Yes' || question.required === true ? 'Yes' : 'No';
        if (question.validation) {
            props.validation = question.validation;
        if (question.sublabel) {
            props.sublabel = question.sublabel;
        if (question.hint) {
            props.hint = question.hint;
        if (question.options && Array.isArray(question.options)) {
            props.options = question.options.join('|');
        if (question.special) {
            props.special = question.special;
        if (question.size) {
            props.size = question.size;
        if (question.maxsize) {
            props.maxsize = question.maxsize;
        if (question.allowTime) {
            props.allowTime = question.allowTime;
        if (question.payment) {
            props.payment = question.payment;
     * Flatten nested form data into JotForm API parameter format
     * JotForm API expects: properties[title]=value, questions[1][type]=value, etc.
    private flattenFormData(data: Record<string, any>, prefix: string = ''): Record<string, any> {
        const flattened: Record<string, any> = {};
            const paramKey = prefix ? `${prefix}[${key}]` : key;
                Object.assign(flattened, this.flattenFormData(value, paramKey));
            } else if (value != null) {
                flattened[paramKey] = value;
        return flattened;
                Name: 'Questions',
                Name: 'Region',
import { TypeformBaseAction } from '../typeform-base.action';
 * Action to create a new Typeform programmatically
 * Security: API credentials are retrieved securely from Company Integrations table or environment variables.
 * Never pass API tokens as action parameters.
 *   ActionName: 'Create Typeform',
 *     Value: '12345'
 *     Value: 'Customer Feedback Survey'
 *     Name: 'Fields',
 *       type: 'short_text',
 *       title: 'What is your name?',
 *       ref: 'name'
 *       type: 'email',
 *       title: 'What is your email?',
 *       ref: 'email',
 *       validations: { required: true }
@RegisterClass(BaseAction, 'CreateTypeformAction')
export class CreateTypeformAction extends TypeformBaseAction {
        return 'Creates a new Typeform with specified title, fields, and settings. Supports all field types including text, email, multiple choice, rating, and more. Returns the new form ID and shareable link.';
                    Message: 'Context user is required for Typeform API calls'
            const fields = this.getParamValue(params.Params, 'Fields');
            if (!fields || !Array.isArray(fields) || fields.length === 0) {
                    ResultCode: 'MISSING_FIELDS',
                    Message: 'Fields parameter is required and must be a non-empty array'
            // Securely retrieve API token using company integration
            const formData: any = {
                fields
            const settings = this.getParamValue(params.Params, 'Settings');
            if (settings) {
                formData.settings = settings;
            const logic = this.getParamValue(params.Params, 'Logic');
            if (logic && Array.isArray(logic)) {
                formData.logic = logic;
            const hiddenFields = this.getParamValue(params.Params, 'HiddenFields');
            if (hiddenFields && Array.isArray(hiddenFields)) {
                formData.hidden = hiddenFields;
            const themeId = this.getParamValue(params.Params, 'ThemeID');
            if (themeId) {
                formData.theme = { href: `https://api.typeform.com/themes/${themeId}` };
            const workspaceId = this.getParamValue(params.Params, 'WorkspaceID');
            if (workspaceId) {
                formData.workspace = { href: `https://api.typeform.com/workspaces/${workspaceId}` };
            const response = await this.getAxiosInstance(apiToken).post('/forms', formData);
            const createdForm = response.data;
            const formUrl = createdForm._links?.display || `https://form.typeform.com/to/${createdForm.id}`;
                    Value: createdForm.title
                    Name: 'FieldCount',
                    Value: createdForm.fields?.length || 0
                Message: `Successfully created form "${createdForm.title}" (ID: ${createdForm.id}). View at: ${formUrl}`
                Message: this.buildFormErrorMessage('Create Typeform', errorMessage, error)
                Name: 'Fields',
                Name: 'Settings',
                Name: 'Logic',
                Name: 'HiddenFields',
                Name: 'ThemeID',
                Name: 'WorkspaceID',
