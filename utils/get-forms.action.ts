 * Action to retrieve a list of all TypeForms for a company
 *   ActionName: 'Get Typeform Forms',
@RegisterClass(BaseAction, 'GetTypeformFormsAction')
export class GetTypeformFormsAction extends TypeformBaseAction {
        return 'Retrieves a list of all TypeForms for the specified company including form IDs, titles, and basic metadata. Use this to discover available forms before processing submissions.';
            // Get secure API token
                    ResultCode: 'API_TOKEN_NOT_FOUND',
                    Message: 'Typeform API token not found. Please configure Typeform integration for this company.'
            // Call Typeform API to list forms
            const response = await fetch('https://api.typeform.com/forms', {
                    ResultCode: 'API_ERROR',
                    Message: `Typeform API error: ${response.status} ${response.statusText}. ${errorText}`
            const data = await response.json() as { items?: any[] };
            const forms = data.items || [];
            // Transform to a more user-friendly format
            const transformedForms = forms.map((form: any) => ({
                description: form.description || '',
                status: form.status,
                lastUpdatedAt: form.last_updated_at ? new Date(form.last_updated_at) : null,
                type: form.type,
                    language: form.settings?.language || 'en',
                    progress_bar: form.settings?.progress_bar || 'show',
                    show_progress_bar: form.settings?.show_progress_bar || false
                statistics: {
                    total_responses: form.statistics?.total_responses || 0,
                    total_conversions: form.statistics?.total_conversions || 0,
                    average_completion_time: form.statistics?.average_completion_time || 0
                Message: `Successfully retrieved ${forms.length} TypeForms from Typeform API`,
                Params: [
                        Name: 'Forms',
                        Value: transformedForms
                        Value: forms.length
                Message: this.buildFormErrorMessage('Get Typeform Forms', errorMessage, error)
    public get Returns(): ActionParam[] {
