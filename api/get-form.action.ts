 * Action to retrieve complete details of a Google Form including all questions, settings, and configuration
 *   ActionName: 'Get Google Forms Details',
 *     Value: '1FAIpQLSe...'
@RegisterClass(BaseAction, 'GetGoogleFormAction')
export class GetGoogleFormAction extends GoogleFormsBaseAction {
        return 'Retrieves complete details of a Google Form including title, questions, logic, settings, and metadata. Use this to inspect form configurations, analyze form structure, and backup form data.';
            // Fetch form details from Google Forms API
            const form = await this.getGoogleFormsDetails(formId, accessToken);
            // Extract form information
            const questionCount = form.items?.filter(item => item.questionItem).length || 0;
            const title = form.info.title || '';
            const description = form.info.description || '';
            const documentTitle = form.info.documentTitle || title;
            // Build comprehensive form details object
            const formOutput = {
                formId: form.formId,
                info: form.info,
                items: form.items,
                settings: form.settings,
                revisionId: form.revisionId,
                responderUri: form.responderUri,
                linkedSheetId: form.linkedSheetId
            // Prepare output parameters
                    Name: 'Form',
                    Value: formOutput
                    Value: form.formId
                    Name: 'Title',
                    Value: title
                    Value: description
                    Name: 'QuestionCount',
                    Value: questionCount
                    Name: 'DocumentTitle',
                    Value: documentTitle
            // Update params with output values
                Message: `Successfully retrieved Google Form "${title}" with ${questionCount} questions`
                Message: this.buildFormErrorMessage('Get Google Forms Details', errorMessage, error)
 * Action to retrieve complete details of a JotForm including questions, properties, and settings
 *   ActionName: 'Get JotForm Details',
 *     Name: 'Region',
 *     Value: 'us'
@RegisterClass(BaseAction, 'GetJotFormAction')
export class GetJotFormAction extends JotFormBaseAction {
        return 'Retrieves complete details of a JotForm including title, status, questions, properties, and settings. Use this to inspect or backup form configurations, analyze form structure, and access form metadata.';
            // Fetch form details and questions from JotForm API
            const formDetails = await this.getJotFormDetails(formId, apiToken, region);
            const questions = await this.getJotFormQuestions(formId, apiToken, region);
            // Extract comprehensive form information
            const questionCount = Object.keys(questions || {}).length;
            const formUrl = formDetails.url || `https://form.jotform.com/${formId}`;
            const createdAt = formDetails.created_at || '';
            const updatedAt = formDetails.updated_at || '';
            const formDetailsOutput = {
                id: formDetails.id,
                title: formDetails.title,
                status: formDetails.status,
                questions: questions,
                properties: {
                    height: formDetails.height,
                    slug: formDetails.slug,
                    count: formDetails.count,
                    favorite: formDetails.favorite,
                    archived: formDetails.archived,
                    last: formDetails.last,
                    new: formDetails.new
                settings: {
                    width: formDetails.width,
                    created_at: formDetails.created_at,
                    updated_at: formDetails.updated_at
                    Value: formDetailsOutput
                    Value: formDetails.id
                    Value: formDetails.title
                    Value: formDetails.status
                    Value: questions
                    Name: 'CreatedAt',
                    Value: createdAt
                    Name: 'UpdatedAt',
                    Value: updatedAt
                Message: `Successfully retrieved form "${formDetails.title}" with ${questionCount} questions`
                Message: this.buildFormErrorMessage('Get JotForm Details', errorMessage, error)
 * Action to retrieve details of a Typeform including all fields, settings, and logic
 *   ActionName: 'Get Typeform Details',
@RegisterClass(BaseAction, 'GetTypeformAction')
export class GetTypeformAction extends TypeformBaseAction {
        return 'Retrieves complete details of a Typeform including title, fields, logic jumps, settings, theme, and metadata. Use this to inspect or backup form configurations.';
            const form = response.data;
            const fieldCount = form.fields?.length || 0;
            const fieldTypes = form.fields?.map((f: any) => f.type) || [];
            const uniqueFieldTypes = [...new Set(fieldTypes)];
            const hasLogic = form.logic && form.logic.length > 0;
            const hasHiddenFields = form.hidden && form.hidden.length > 0;
                id: form.id,
                title: form.title,
                fieldCount,
                uniqueFieldTypes,
                hasLogic,
                hasHiddenFields,
                theme: form.theme_id || form.theme?.name,
                workspace: form.workspace?.href,
                createdAt: form._links?.display ? new Date() : undefined,
                lastUpdated: form.last_updated_at ? new Date(form.last_updated_at) : undefined
                    Value: form
                    Value: form.id
                    Value: form.title
                    Value: form.fields
                    Value: fieldCount
                    Value: form.settings
                    Value: form.logic
                    Name: 'Theme',
                    Value: form.theme
                    Value: form.hidden
                Message: `Successfully retrieved form "${form.title}" with ${fieldCount} fields`
                Message: this.buildFormErrorMessage('Get Typeform Details', errorMessage, error)
