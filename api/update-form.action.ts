 * Action to update an existing JotForm
 * IMPORTANT: JotForm uses separate endpoints for properties and questions.
 * Use the MergeWithExisting parameter to safely update only specified properties
 * while preserving existing ones.
 *   ActionName: 'Update JotForm',
 *     Value: 'Updated Survey Title'
 *     Name: 'MergeWithExisting',
@RegisterClass(BaseAction, 'UpdateJotFormAction')
export class UpdateJotFormAction extends JotFormBaseAction {
        return 'Updates an existing JotForm. Set MergeWithExisting=true (default) to safely update only specified properties while preserving others. Set to false to replace entire form data (not recommended).';
            const mergeWithExisting = this.getParamValue(params.Params, 'MergeWithExisting') !== false;
            // Check if any updates are provided
            if (!title && !questions && !properties) {
                    ResultCode: 'NO_CHANGES_PROVIDED',
                    Message: 'At least one of Title, Questions, or Properties must be provided'
            const updatedFields: string[] = [];
            let existingForm: any = null;
            // Get existing form if merging
            if (mergeWithExisting) {
                existingForm = await this.getJotFormDetails(formId, apiToken, region);
            const axiosInstance = this.getAxiosInstance(apiToken, region);
            // Update properties (including title)
            if (title || properties) {
                let propertiesToUpdate: Record<string, any> = {};
                // Start with existing properties if merging
                if (mergeWithExisting && existingForm?.properties) {
                    propertiesToUpdate = { ...existingForm.properties };
                // Add title if provided
                if (title) {
                    propertiesToUpdate.title = title;
                    updatedFields.push('title');
                // Merge additional properties if provided
                if (properties) {
                    const propsToMerge = typeof properties === 'string' ? JSON.parse(properties) : properties;
                    propertiesToUpdate = { ...propertiesToUpdate, ...propsToMerge };
                    updatedFields.push('properties');
                // Update properties via PUT endpoint
                await axiosInstance.put(
                    `/form/${formId}/properties`,
                    { properties: propertiesToUpdate }
            // Update questions if provided
            if (questions) {
                let questionsToUpdate = Array.isArray(questions) ? questions :
                                       typeof questions === 'string' ? JSON.parse(questions) : questions;
                // If merging, get existing questions and merge
                    const existingQuestions = await this.getJotFormQuestions(formId, apiToken, region);
                    // Merge questions by ID or order
                    const mergedQuestions: Record<string, any> = { ...existingQuestions };
                    if (Array.isArray(questionsToUpdate)) {
                        // If questions array is provided, update by index/ID
                        questionsToUpdate.forEach((question: any, index: number) => {
                            const questionId = question.qid || Object.keys(existingQuestions)[index];
                            if (questionId) {
                                mergedQuestions[questionId] = { ...mergedQuestions[questionId], ...question };
                        // If questions object is provided, merge directly
                        Object.assign(mergedQuestions, questionsToUpdate);
                    questionsToUpdate = mergedQuestions;
                // Update questions via POST endpoint
                await axiosInstance.post(
                    { questions: questionsToUpdate }
                updatedFields.push('questions');
            // Get updated form details
            const updatedForm = await this.getJotFormDetails(formId, apiToken, region);
            // Construct form URL
            const formUrl = updatedForm.url || `https://form.jotform.com/${formId}`;
                    Value: formId
                    Name: 'UpdatedFields',
                    Value: updatedFields
                    Value: updatedForm
            // Add output params to result
                Message: `Successfully updated JotForm "${updatedForm.title || formId}". Updated fields: ${updatedFields.join(', ')}`
                Message: this.buildFormErrorMessage('Update JotForm', errorMessage, error)
                Name: 'MergeWithExisting',
 * Action to update an existing Typeform
 * IMPORTANT: This uses PUT which replaces the entire form. If you only provide some fields,
 * the rest will be deleted. Use the MergeWithExisting parameter to automatically fetch
 * the current form and merge your changes.
 *   ActionName: 'Update Typeform',
@RegisterClass(BaseAction, 'UpdateTypeformAction')
export class UpdateTypeformAction extends TypeformBaseAction {
        return 'Updates an existing Typeform. WARNING: Uses PUT which replaces all form data - fields not included will be deleted. Set MergeWithExisting=true to safely update only specified properties while preserving others.';
            let formData: any = {};
                const existingResponse = await this.getAxiosInstance(apiToken).get(`/forms/${formId}`);
                formData = existingResponse.data;
                delete formData.id;
                delete formData._links;
                delete formData.created_at;
                delete formData.last_updated_at;
                delete formData.self;
                formData.title = title;
            if (fields && Array.isArray(fields)) {
                formData.fields = fields;
            } else if (!mergeWithExisting) {
                    Message: 'Fields parameter is required when MergeWithExisting is false'
                if (mergeWithExisting && formData.settings) {
                    formData.settings = { ...formData.settings, ...settings };
            if (logic !== undefined) {
            if (hiddenFields !== undefined) {
            const response = await this.getAxiosInstance(apiToken).put(`/forms/${formId}`, formData);
            const updatedForm = response.data;
            const formUrl = updatedForm._links?.display || `https://form.typeform.com/to/${updatedForm.id}`;
                    Value: updatedForm.id
                    Value: updatedForm.title
                    Value: updatedForm.fields?.length || 0
                    Name: 'LastUpdated',
                    Value: updatedForm.last_updated_at ? new Date(updatedForm.last_updated_at) : new Date()
                Message: `Successfully updated form "${updatedForm.title}" (ID: ${updatedForm.id})`
                Message: this.buildFormErrorMessage('Update Typeform', errorMessage, error)
