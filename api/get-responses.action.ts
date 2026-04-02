 * Action to retrieve responses from a SurveyMonkey survey with comprehensive filtering options
 *   ActionName: 'Get SurveyMonkey Responses',
 *     Value: '12345678'
 *     Name: 'StartCreatedAt',
 *     Value: '2024-01-01T00:00:00Z'
 *     Name: 'Status',
 *     Value: 'completed'
@RegisterClass(BaseAction, 'GetSurveyMonkeyResponsesAction')
export class GetSurveyMonkeyResponsesAction extends SurveyMonkeyBaseAction {
        return 'Retrieves responses from a SurveyMonkey survey with filtering, pagination, and sorting capabilities. Supports date ranges, completion status, and response status filtering.';
            const pageSize = this.getParamValue(params.Params, 'PageSize') || 100;
            const page = this.getParamValue(params.Params, 'Page');
            const startModifiedAt = this.getParamValue(params.Params, 'StartModifiedAt');
            const endModifiedAt = this.getParamValue(params.Params, 'EndModifiedAt');
            const sortOrder = this.getParamValue(params.Params, 'SortOrder') as 'ASC' | 'DESC' | undefined;
            const sortBy = this.getParamValue(params.Params, 'SortBy') as 'date_modified' | 'date_created' | undefined;
            const status = this.getParamValue(params.Params, 'Status') as 'completed' | 'partial' | 'overquota' | 'disqualified' | undefined;
            let responses: FormResponse[];
            let totalResponses: number;
            let currentPage: number;
            let totalPages: number;
            let hasMore: boolean;
                    start_modified_at: startModifiedAt,
                    end_modified_at: endModifiedAt,
                    sort_order: sortOrder,
                    sort_by: sortBy,
                responses = smResponses.map(r => this.normalizeSurveyMonkeyResponse(r));
                totalResponses = responses.length;
                currentPage = 1;
                totalPages = 1;
                    per_page: Math.min(pageSize, 100),
                    page,
                responses = result.data.map(r => this.normalizeSurveyMonkeyResponse(r));
                totalResponses = result.total;
                currentPage = result.page;
                totalPages = Math.ceil(result.total / result.per_page);
                hasMore = !!result.links.next;
                    Name: 'Responses',
                    Value: responses
                    Value: totalResponses
                    Name: 'CurrentPage',
                    Value: currentPage
                    Name: 'TotalPages',
                    Value: totalPages
                    Value: hasMore
                Message: `Successfully retrieved ${responses.length} responses from SurveyMonkey${getAllPages ? '' : ` (page ${currentPage} of ${totalPages}, ${totalResponses} total)`}`
                Message: this.buildFormErrorMessage('Get SurveyMonkey Responses', errorMessage, error)
                Name: 'PageSize',
                Name: 'Page',
                Name: 'StartModifiedAt',
                Name: 'EndModifiedAt',
 * Action to retrieve responses from a Typeform with comprehensive filtering options
 *   ActionName: 'Get Typeform Responses',
 *     Name: 'Since',
 *     Name: 'Completed',
@RegisterClass(BaseAction, 'GetTypeformResponsesAction')
export class GetTypeformResponsesAction extends TypeformBaseAction {
        return 'Retrieves responses from a Typeform with filtering, pagination, and search capabilities. Supports date ranges, completion status, and text search across answers.';
            const pageSize = this.getParamValue(params.Params, 'PageSize') || 25;
            const after = this.getParamValue(params.Params, 'After');
            const before = this.getParamValue(params.Params, 'Before');
            const sort = this.getParamValue(params.Params, 'Sort') || 'submitted_at,desc';
            const query = this.getParamValue(params.Params, 'Query');
            let totalItems: number;
            let pageCount: number;
            let responseTokens: { before?: string; after?: string } = {};
            // Get form details to fetch field titles for simpleAnswers
            let formFields: any[] = [];
                const formDetails = await this.getFormDetails(formId, apiToken);
                formFields = formDetails.fields || [];
            } catch (formError) {
                LogStatus(`Warning: Could not fetch form details for simpleAnswers generation: ${formError.message}`);
                // Continue without simpleAnswers - this is not a critical failure
                    sort,
                responses = tfResponses.map(r => this.normalizeTypeformResponse(r, formFields));
                totalItems = responses.length;
                pageCount = 1;
                const fieldsArray = fields ? String(fields).split(',').map((f: string) => f.trim()) : undefined;
                    pageSize: Math.min(pageSize, 1000),
                    after,
                    before,
                    fields: fieldsArray
                responses = result.items.map(r => this.normalizeTypeformResponse(r, formFields));
                totalItems = result.total_items;
                pageCount = result.page_count;
                if (result.items.length > 0) {
                    responseTokens.after = result.items[result.items.length - 1].token;
                    responseTokens.before = result.items[0].token;
                    Name: 'TotalItems',
                    Value: totalItems
                    Name: 'ResponseTokens',
                    Value: responseTokens
                    Name: 'AnswerDetails',
                    Value: responses.map(r => r.answerDetails)
                    Value: responses.map(r => r.answers)
                Message: `Successfully retrieved ${responses.length} responses from Typeform (${totalItems} total available)`
                Message: this.buildFormErrorMessage('Get Typeform Responses', errorMessage, error)
                Value: 25,
                Name: 'Before',
                Name: 'Sort',
                Value: 'submitted_at,desc', 
                Name: 'Query',
                Value: false, 
